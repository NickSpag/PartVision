using System;
using System.Runtime.CompilerServices;
using AVFoundation;
using CoreML;
using Foundation;
using Vision;
using PartVision.Standard;
using CoreMedia;
using CoreVideo;
using ImageIO;

namespace PartVision.iOS
{
	public class DetectionSession : AVCaptureVideoDataOutputSampleBufferDelegate
	{
		private VNRequest[] requests;
		private DetectionType detectionType;
		private Action<GestureCommand> gestureRecognizedCallback;
		private Action<string> partRecognizedCallback;

		private int frameCount = 0;
		private int recognizeCount = 0;
		private int trainCount = 0;

		private DetectionSession() { }

		public static DetectionSession ForGesture(Action<GestureCommand> gestureCallback)
		{
			if (gestureCallback is null)
			{
				return null;
			}

			var session = new DetectionSession()
			{
				detectionType = DetectionType.Gesture,
				gestureRecognizedCallback = gestureCallback
			};

			session.PrepareVisionRequests();
			return session;
		}

		public static DetectionSession ForPart(Action<string> partCallback)
		{
			if (partCallback is null)
			{
				return null;
			}

			var session = new DetectionSession()
			{
				detectionType = DetectionType.Part,
				partRecognizedCallback = partCallback,
			};

			session.PrepareVisionRequests();

			return session;
		}

		private MLModel PrepareCoreMLModel()
		{
			var modelPath = NSBundle.MainBundle.GetUrlForResource("firstTest", "mlmodelc");
			var model = MLModel.Create(modelPath, out NSError error1);

			CheckModelErrors(error1);

			return model;
		}

		private VNCoreMLModel PrepareModelForVision(MLModel model)
		{
			var visionOptimizedModel = VNCoreMLModel.FromMLModel(model, out NSError error);

			CheckModelErrors(error);

			return visionOptimizedModel;
		}

		private void PrepareVisionRequests()
		{
			var model = PrepareCoreMLModel();

			var mlRequest = new VNCoreMLRequest(PrepareModelForVision(model), HandleVNRequestCompletionHandler);

			requests = new VNRequest[] { mlRequest };
		}

		private void HandleVNRequestCompletionHandler(VNRequest request, NSError error)
		{
			var observations = request.GetResults<VNClassificationObservation>();

			foreach (var observation in observations)
			{
				RecordObservation(observation);
			}
		}

		private void RecordObservation(VNClassificationObservation observation)
		{
			//todo: observation logic
			switch (detectionType)
			{
				case DetectionType.Gesture:
					CheckGesture(observation);
					break;
				case DetectionType.Part:
					CheckPart(observation);
					break;
				default:
					break;
			}
		}

		private void CheckGesture(VNClassificationObservation observation)
		{
			System.Diagnostics.Debug.WriteLine($"{observation.Identifier} at {Math.Round(observation.Confidence, 2)}");

			if (observation.Identifier.Contains("Recognize") && observation.Confidence > .70)
			{
				recognizeCount++;
			}

			if (observation.Identifier.Contains("Train") && observation.Confidence > .70)
			{
				trainCount++;
			}

			if (recognizeCount > 2 || trainCount > 2)
			{
				if (recognizeCount > trainCount)
				{
					gestureRecognizedCallback(GestureCommand.Recognize);
				}
				else if (recognizeCount < trainCount)
				{
					gestureRecognizedCallback(GestureCommand.Train);
				}
				else
				{
					//FIXME: race condition / brittle
					throw new Exception("It's detected both gestures at the same time");
				}

				trainCount = 0;
				recognizeCount = 0;
			}
		}

		private void CheckPart(VNClassificationObservation observation)
		{

		}

		private void CheckModelErrors(NSError error, [CallerMemberNameAttribute] string name = null)
		{
			if (error != null)
			{
				throw new Exception($"Core ML/Vission issue" + (string.IsNullOrEmpty(name) ? " detected" : $"at {name}" +
																$"\n {error.Description}"));
			}
		}


		#region Methods: IAVCaptureVideoDataOutputSampleBufferDelegate
		private static NSDictionary empty = new NSDictionary();

		public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			System.Console.WriteLine("Received frame.");

			try
			{
				frameCount++;

				if (frameCount > 4)
				{
					PerformRequestOnFrame(sampleBuffer.GetImageBuffer());
					frameCount = 0;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			finally
			{
				sampleBuffer.Dispose();
			}
		}

		private void PerformRequestOnFrame(CVImageBuffer imageBuffer)
		{
			using (var pixelBuffer = imageBuffer as CVPixelBuffer)
			using (var imageRequestHandler = new VNImageRequestHandler(pixelBuffer, CGImagePropertyOrientation.Right, empty))
			{
				if (requests != null)
				{
					imageRequestHandler.Perform(requests, out NSError error);

					if (error != null)
					{
						System.Diagnostics.Debug.WriteLine("NSerror on perform request: " + error?.Description);
					}
				}
			}
		}

		public override void DidDropSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			System.Console.WriteLine("Dropped frame.");
			sampleBuffer.Dispose();
		}
		#endregion
	}
}
