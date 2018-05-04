using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Media;

using Java.Lang;
using Java.Util.Concurrent;

using Plugin.CurrentActivity;
using Plugin.TextToSpeech;
using Org.Tensorflow.Contrib.Android;

using PartVision.Droid;
using PartVision.Standard;

[assembly: Xamarin.Forms.Dependency(typeof(CaptureCameraFrames))]
namespace PartVision.Droid
{
	public class CaptureCameraFrames : Fragment, ICaptureFrames
	{
		private SparseIntArray ORIENTATIONS = new SparseIntArray();

		public CameraDevice cameraDevice;
		public CameraCaptureSession previewSession;

		private CameraStateCallback stateListener;
		private SurfaceTextureListener surfaceTextureListener;

		public CaptureRequest.Builder builder;
		private CaptureRequest.Builder previewBuilder;

		private ImageAvailableListener imageAvailableListener;
		//hold a reference so it doesnt get GC'd
		private ImageReader frameReader;

		public Semaphore cameraOpenCloseLock = new Semaphore(1);

		public Handler backgroundHandler;
		private HandlerThread backgroundThread;

		public CaptureCameraFrames()
		{
			ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
			ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
			ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
			ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);

			surfaceTextureListener = new SurfaceTextureListener(this);
			stateListener = new CameraStateCallback(this);

			PrepareML();
		}

		#region Delegate stuff

		TensorFlowInferenceInterface inferenceInterface;
		List<string> labels;

		private void PrepareML()
		{
			var assets = Application.Context.Assets;
			inferenceInterface = new TensorFlowInferenceInterface(assets, "model.pb");
			var sr = new StreamReader(assets.Open("labels.txt"));
			labels = sr.ReadToEnd()
						   .Split('\n')
						   .Select(s => s.Trim())
						   .Where(s => !string.IsNullOrEmpty(s))
						   .ToList();
		}

		private float[] dataForBitmap(Bitmap bitmap)
		{
			var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, 227, 227, false)
						  .Copy(Bitmap.Config.Argb8888, false);
			var floatValues = new float[227 * 227 * 3];
			var intValues = new int[227 * 227];
			resizedBitmap.GetPixels(intValues, 0, 227, 0, 0, 227, 227);
			for (int i = 0; i < intValues.Length; ++i)
			{
				var val = intValues[i];
				floatValues[i * 3 + 0] = ((val & 0xFF) - 104);
				floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - 117);
				floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - 123);
			}

			return floatValues;
		}

		private void FrameAnalyzed(byte[] bytes)
		{
			var bitmapImage = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, null);

			var floatValues = dataForBitmap(bitmapImage);

			bitmapImage.Dispose();

			var outputs = new float[labels.Count];
			inferenceInterface.Feed("Placeholder", floatValues, 1, 227, 227, 3);
			inferenceInterface.Run(new[] { "loss" });
			inferenceInterface.Fetch("loss", outputs);

			Record(outputs);

			//frameAnalyzedHandler(bytes);
		}

		private int recognize = 0;
		private int train = 0;

		private void Record(float[] outputs)
		{
			var rec = System.Math.Round(outputs[0], 4) * 100;
			var trai = System.Math.Round(outputs[1], 4) * 100;

			if (rec > 60)
			{
				recognize++;
			}

			if (trai > 60)
			{
				train++;
			}

			System.Console.WriteLine($"Recognize Gesture confidence is {System.Math.Round(outputs[0], 4) * 100}%");
			System.Console.WriteLine($"Train Gesture confidence is {System.Math.Round(outputs[1], 4) * 100}%");

			if (train > 1)
			{
				imageAvailableListener.analyzing = false;
				train = 0;
				Train();
			}
			if (recognize > 1)
			{
				recognize = 0;
				imageAvailableListener.analyzing = false;
				Recognize();
			}
		}

		private void Train()
		{
			CrossTextToSpeech.Current.Speak("Training gesture");
		}

		private void Recognize()
		{
			CrossTextToSpeech.Current.Speak("Recognize gesture");
		}

		public void FrameAvailable(Image frame)
		{
			backgroundHandler.Post(new ImageAnalyzer(frame, FrameAnalyzed));
		}

		Action<byte[]> frameAnalyzedHandler;

		public void TakeStill(Action<byte[]> frameHandler)
		{
			frameAnalyzedHandler = frameHandler;
			imageAvailableListener.TakeNextStill();
		}
		#endregion

		public void BeginCapture()
		{
			imageAvailableListener = new ImageAvailableListener(this);

			StartBackgroundThread();

			try
			{
				var manager = (CameraManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.CameraService);
				var cameraId = PrepareCamera(manager);

				//the manager opens the camera and gives it our custom state listener, which will 
				//in turn provide this class with an actual CameraDevice object based on the id we specified,
				//and then tell this class to start its previewing mechanism
				manager.OpenCamera(cameraId, stateListener, backgroundHandler);

			}
			catch (CameraAccessException)
			{
				System.Console.WriteLine("Camera Access");
			}
			catch (NullPointerException)
			{
				System.Console.WriteLine("Null pointer");
			}
			catch (InterruptedException)
			{
				System.Console.WriteLine("Interruption");
			}
		}

		public void StopCapture()
		{
			previewSession?.StopRepeating();
			CloseCamera();
			StopBackgroundThread();
		}

		public void StartPreviewing()
		{
			builder = cameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);
			builder.Set(CaptureRequest.JpegOrientation, (int)270);

			previewBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
			previewBuilder.Set(CaptureRequest.JpegOrientation, (int)270);

			//creates the surfacves. one to mimic a dummy preview of the frame, and one to actually record it.
			//adds those surfaces to the surface list that will be fed to the camera capture session
			//adds those surfaces to the previewbuilder as a target
			var surfaces = new List<Surface>();

			var surfaceTexture = new SurfaceTexture(1);
			var previewSurface = new Surface(surfaceTexture);

			frameReader = ImageReader.NewInstance(300, 600, ImageFormatType.Jpeg, 10);
			frameReader.SetOnImageAvailableListener(imageAvailableListener, backgroundHandler);

			surfaces.Add(frameReader.Surface);
			surfaces.Add(previewSurface);

			previewBuilder.AddTarget(frameReader.Surface);
			builder.AddTarget(previewSurface);

			//this creates previewCaptureStateCallback, and the method will configure it, spurring its OnConfigured
			//method which will call UpdatePreview and give this class its CaptureSession
			cameraDevice.CreateCaptureSession(surfaces, new PreviewCaptureStateCallback(this), backgroundHandler);
		}


		private void SetCaptureRequestBuilder(CaptureRequest.Builder requestBuilder)
		{
			requestBuilder.Set(CaptureRequest.ControlMode, new Integer((int)ControlMode.Auto));
			requestBuilder.Set(CaptureRequest.ColorCorrectionAberrationMode, new Integer((int)ColorCorrectionAberrationMode.HighQuality));
		}

		public void UpdatePreviewSession(CameraCaptureSession session)
		{
			previewSession = session;

			try
			{
				SetCaptureRequestBuilder(previewBuilder);

				HandlerThread thread = new HandlerThread("CameraPreview");
				thread.Start();

				previewSession.SetRepeatingRequest(previewBuilder.Build(), null, backgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
			}
		}

		private string PrepareCamera(CameraManager manager)
		{
			if (!cameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
			{
				throw new RuntimeException("Time out waiting to lock camera opening.");
			}

			return manager.GetCameraIdList()[0];

			//var characteristics = manager.GetCameraCharacteristics(cameraId);
			//var configMap = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

			//videoSize = ChooseVideoSize(configMap.GetOutputSizes(Class.FromType(typeof(MediaRecorder))));
			//previewSize = ChooseOptimalSize(configMap.GetOutputSizes(Class.FromType(typeof(MediaRecorder))), width, height, videoSize);

			////originally these were cast as int?
			//var orientation = Resources.Configuration.Orientation;

			//if (orientation == Android.Content.Res.Orientation.Landscape)
			//{
			//    textureView.SetAspectRatio(previewSize.Width, previewSize.Height);
			//}
			//else
			//{
			//    textureView.SetAspectRatio(previewSize.Height, previewSize.Width);
			//}

			//configureTransform(width, height);

			//return cameraId;
		}

		private void StartBackgroundThread()
		{
			backgroundThread = new HandlerThread("CameraBackground");
			backgroundThread.Start();
			backgroundHandler = new Handler(backgroundThread.Looper);
		}

		private void StopBackgroundThread()
		{
			backgroundThread.QuitSafely();

			try
			{
				backgroundThread.Join();
				backgroundThread = null;
				backgroundHandler = null;
			}
			catch (InterruptedException e)
			{
				e.PrintStackTrace();
			}
		}

		private void CloseCamera()
		{
			try
			{
				cameraOpenCloseLock.Acquire();

				cameraDevice?.Close();
				cameraDevice = null;
			}
			catch (InterruptedException e)
			{
				throw new RuntimeException("Interrupted while trying to lock camera closing.");
			}
			finally
			{
				cameraOpenCloseLock.Release();
			}
		}

	}
}
