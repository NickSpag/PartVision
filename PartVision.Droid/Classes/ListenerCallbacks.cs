using System;
using Android.Media;
using Java.Lang;
using Android.OS;
using System.IO;
using Android.Graphics;
using Android.Test.Suitebuilder;

namespace PartVision.Droid
{
	public class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
	{
		CaptureCameraFrames frameCapturer;
		Handler backgroundHandler;

		public ImageAvailableListener(CaptureCameraFrames frameCapturer)
		{
			this.frameCapturer = frameCapturer;
			backgroundHandler = frameCapturer.backgroundHandler;
		}

		public bool analyzing { get; set; } = false;

		public void TakeNextStill()
		{
			analyzing = true;
		}

		int evenOdd = 0;

		public void OnImageAvailable(ImageReader reader)
		{
			System.Console.WriteLine("Image available");

			//as of now you actually have to get the next image even if you don't plan on doing anything with it
			//if you don't acess the next image it doesnt count towards the "max images" property but it does
			//count towards some sort of invisible memory limit and it'll stop collecting frames after a few.
			var image = reader.AcquireNextImage();

			if (analyzing)
			{
				if (evenOdd % 2 == 0)
				{
					frameCapturer.FrameAvailable(image);
					//analyzing = false;
				}
				//todo frameskipping
				//var image = reader.AcquireNextImage();

			}
			else
			{
				image.Close();
				image.Dispose();
			}
		}
	}

	public class ImageAnalyzer : Java.Lang.Object, IRunnable
	{
		Image frameImage;

		public delegate void FrameAnalyzedDelegate(byte[] bytes);

		private FrameAnalyzedDelegate frameAnalyzedHandler;

		public ImageAnalyzer(Image image, FrameAnalyzedDelegate frameAnalyzedHandler)
		{
			frameImage = image;
			this.frameAnalyzedHandler = frameAnalyzedHandler;
		}

		public void Run()
		{
			var memoryStream = new MemoryStream();

			var buffer = frameImage.GetPlanes()[0].Buffer;

			byte[] bytes = new byte[buffer.Capacity()];
			buffer.Get(bytes);

			frameAnalyzedHandler(bytes);

			frameImage?.Close();
			frameImage?.Dispose();
		}
	}

	public class ListenerCallbacks
	{
		public ListenerCallbacks()
		{
		}
	}
}
