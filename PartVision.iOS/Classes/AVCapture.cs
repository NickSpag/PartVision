using System;
using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using Vision;
using CoreFoundation;
using PartVision.iOS;
using PartVision.Standard;

[assembly: Xamarin.Forms.Dependency(typeof(AVCapture))]
namespace PartVision.iOS
{
	public class AVCapture : AVCaptureVideoDataOutputSampleBufferDelegate, ICaptureFrames
	{
		private AVCaptureSession captureSession;
		private AVCaptureDeviceInput deviceInput;
		private AVCaptureVideoDataOutput deviceOutput;
		private AVCaptureDevice captureDevice;

		private DispatchQueue queue = new DispatchQueue("videoQueue");

		private VNRequest[] requests;

		public bool isCapturing = false;

		public AVCapture()
		{

		}

		public void WatchForGesture(Action<GestureCommand> gestureRecognizedCallback)
		{
			PrepareToCapture(DetectionSession.ForGesture(gestureRecognizedCallback));
		}

		public void WatchForPart(Action<string> partRecognizedCallback)
		{
			PrepareToCapture(DetectionSession.ForPart(partRecognizedCallback));
		}

		public void BeginCapture()
		{
			PrepareToCapture(this);
		}

		private void PrepareToCapture(IAVCaptureVideoDataOutputSampleBufferDelegate detectionDelegate)
		{
			//clear session if in the middle of a previous one
			if (isCapturing)
			{
				StopCapture();
			}

			PrepareCaptureSessionForDetection(detectionDelegate);

			isCapturing = true;
			captureSession.StartRunning();
		}

		private void PrepareCaptureSessionForDetection(IAVCaptureVideoDataOutputSampleBufferDelegate detectionDelegate)
		{
			try
			{
				captureSession = new AVCaptureSession();
				captureSession.SessionPreset = AVCaptureSession.Preset352x288;

				captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);

				deviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
				deviceOutput = new AVCaptureVideoDataOutput();

				deviceOutput.WeakVideoSettings = new CVPixelBufferAttributes { PixelFormatType = CVPixelFormatType.CV32BGRA }.Dictionary;

				deviceOutput.SetSampleBufferDelegateQueue(detectionDelegate, queue);

				captureSession.AddInput(deviceInput);
				captureSession.AddOutput(deviceOutput);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);
			}
		}

		public void StopCapture()
		{
			if (captureSession != null)
			{
				captureSession.StopRunning();
				captureSession.Dispose();

				isCapturing = false;
			}
		}

		bool stillNeedsTaken = false;
		Action<byte[]> stillTakenCallback;

		public void TakeStill(Action<byte[]> stillCallback)
		{
			stillTakenCallback = stillCallback;
			stillNeedsTaken = true;
		}

		#region Methods: IAVCaptureVideoDataOutputSampleBufferDelegate
		private static NSDictionary empty = new NSDictionary();

		public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			System.Console.WriteLine("Received frame.");

			try
			{
				if (stillNeedsTaken)
				{
					//take still code
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

		public override void DidDropSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			System.Console.WriteLine("Dropped frame.");
			sampleBuffer.Dispose();
		}
		#endregion
	}
}
