using System;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.TextToSpeech;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PartVision.Standard
{
	public class WorkViewModel : BaseViewModel
	{
		private ICaptureFrames frameCapturer;

		public WorkViewModel()
		{
			BeginWorkCommand = new Command(BeginWorkSequence);
			TakeStillCommand = new Command(TakeStill);
			BeginCaptureCommand = new Command(BeginCapture);
		}

		public ICommand BeginWorkCommand { get; }

		public ICommand TakeStillCommand { get; }

		public ICommand BeginCaptureCommand { get; }

		private StreamImageSource _frame;

		public ImageSource Frame { get; set; }

		public void LoadLatestModel()
		{

		}

		public void TakeStill()
		{
			//CrossTextToSpeech.Current.Speak("Training Gesture");
			frameCapturer.TakeStill(SetFrame);
		}

		public void BeginCapture()
		{
			//CrossTextToSpeech.Current.Speak("Recognition Gesture");

			frameCapturer = DependencyService.Get<ICaptureFrames>();
			frameCapturer.BeginCapture();
		}

		private void SetFrame(byte[] bytes)
		{
			try
			{
				//var decodedByteArray = System.Convert.FromBase64String(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
				Frame = ImageSource.FromStream(() => new MemoryStream(bytes));

				OnPropertyChanged(nameof(Frame));

				System.Console.WriteLine("frame set");
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);
			}
		}

		private void SetFrame(Stream stream)
		{
			Frame = ImageSource.FromStream(() => stream);

			System.Console.WriteLine("frame set");
		}

		private async void BeginWorkSequence()
		{
			//initiate camera
			//watch for gestures

			await CrossTextToSpeech.Current.Speak("Ready to work");


		}

		private async Task BeginModelDetection()
		{

		}

		private async Task BeginOCRDetection()
		{

		}

		private async Task BeginTrainingProcess()
		{
			var images = new List<Stream>();

			var trainingItemSession = Guid.NewGuid();

			var frameCapturer = DependencyService.Get<ICaptureFrames>();

			await CrossTextToSpeech.Current.Speak("Prepare to take photos");

			await Task.Delay(TimeSpan.FromSeconds(2.5));

			for (int i = 0; i < 5; i++)
			{
				var index = i + 1;

				await CrossTextToSpeech.Current.Speak($"Taking photo {index}");

				//frameCapturer.TakeStill();
			}



		}

		private async Task TakeVideo()
		{
			await CrossTextToSpeech.Current.Speak("Taking photo");
		}


		//sequenceID + "needsIdentify"
		//sighted person app - gets all tags from customvision
		//if tag starts or ends with needsIdentify, gets images for that tag
		//shows gallery of sequences and their images that need identifying.
	}
}
