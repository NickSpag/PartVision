using System;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.TextToSpeech;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PartVision.Standard
{
	public class WorkViewModel : BaseViewModel
	{
		private ICaptureFrames frameCapturer;

		public WorkViewModel()
		{
			BeginWorkCommand = new Command(BeginWorkSequence);
			TakeStillCommand = new Command(TakeStill);
			WatchGestureCommand = new Command(WatchForGesture);
		}

		public ICommand BeginWorkCommand { get; }

		public ICommand TakeStillCommand { get; }

		public ICommand WatchGestureCommand { get; }

		private StreamImageSource _frame;

		public ImageSource Frame { get; set; }

		public void LoadLatestModel()
		{

		}

		public async void TakeStill()
		{
			frameCapturer.BeginCapture();

			await Task.Delay(TimeSpan.FromSeconds(1));

			frameCapturer.TakeStill(SetFrame);
		}

		public async void WatchForGesture()
		{
			var waitCancellation = new CancellationTokenSource();
			var speechCancellation = new CancellationTokenSource();

			GestureCommand? recognizedGesture = null;

			CrossTextToSpeech.Current.Speak("Watching for gesture", null, null, null, null, speechCancellation.Token);

			frameCapturer = DependencyService.Get<ICaptureFrames>();
			frameCapturer.WatchForGesture(GestureCaught);

			try
			{
				await Task.Delay(TimeSpan.FromSeconds(10), waitCancellation.Token);
			}
			catch (Exception ex)
			{
				if (recognizedGesture.HasValue)
				{
					switch (recognizedGesture.Value)
					{
						case GestureCommand.Recognize:
							speechCancellation.Cancel();
							await CrossTextToSpeech.Current.Speak("Recognition gesture.");
							//await BeginRecognition();
							break;
						case GestureCommand.Train:
							speechCancellation.Cancel();
							await CrossTextToSpeech.Current.Speak("Training gesture.");
							//await BeginTraining();
							break;
						default:
							await CrossTextToSpeech.Current.Speak("No gesture recognizezd");
							break;
					}
				}
			}

			frameCapturer.StopCapture();

			void GestureCaught(GestureCommand gestureGiven)
			{
				//order is important here
				recognizedGesture = gestureGiven;
				waitCancellation.Cancel();
			}
		}


		public void BeginCapture()
		{

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

		private async void BeginWorkSequence()
		{
			//initiate camera
			//watch for gestures

			await CrossTextToSpeech.Current.Speak("Ready to work");


		}


		private async Task BeginTraining()
		{
			var images = new List<byte[]>();

			var trainingItemSession = Guid.NewGuid();

			var frameCapture = DependencyService.Get<ICaptureFrames>();
			frameCapture.BeginCapture();

			await CrossTextToSpeech.Current.Speak("Prepare to take photos");

			await TakePhotos();

			var batch = new PVPartBatch(images);

			await VisionService.UploadPartBatch(batch);

			void AddImage(byte[] bytes)
			{
				images.Add(bytes);
			}

			async Task TakePhotos()
			{
				for (int i = 0; i < 5; i++)
				{
					await Task.Delay(TimeSpan.FromSeconds(1));

					var index = i + 1;

					await CrossTextToSpeech.Current.Speak($"Taking photo {index}");

					frameCapturer.TakeStill(AddImage);
				}
			}
		}

		private async Task BeginRecognition()
		{

		}

		private async Task BeginModelDetection()
		{

		}

		private async Task BeginOCRDetection()
		{

		}


		//sequenceID + "needsIdentify"
		//sighted person app - gets all tags from customvision
		//if tag starts or ends with needsIdentify, gets images for that tag
		//shows gallery of sequences and their images that need identifying.
	}
}
