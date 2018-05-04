using System.Windows.Input;
using System.Threading.Tasks;

using Xamarin.Forms;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Media.Abstractions;
using Plugin.Media;
using System.Collections.ObjectModel;
using System;

namespace PartVision.Standard
{
	public class CameraPageViewModel : BaseViewModel
	{
		GestureCommand commandType => IsRecognizeCommand ? GestureCommand.Recognize : GestureCommand.Train;

		public ObservableCollection<PVImageUpload> GestureImages { get; set; } = new ObservableCollection<PVImageUpload>();
		public bool IsRecognizeCommand { get; set; }

		public CameraPageViewModel()
		{
			Title = "Camera";

			TakePictureCommand = new Command(TakePicture);
			TestCommand = new Command(TestVision);
		}

		public ICommand TakePictureCommand { get; }

		public ICommand TestCommand { get; }

		private async void TestVision()
		{
			var frameCapture = DependencyService.Get<ICaptureFrames>();

			frameCapture.BeginCapture();

			await Task.Delay(TimeSpan.FromSeconds(10));

			frameCapture.StopCapture();
		}

		private async void TakePicture()
		{
			if (!await CheckCameraPermission()) return;

			var options = new StoreCameraMediaOptions { PhotoSize = PhotoSize.Small };
			var image = await CrossMedia.Current.TakePhotoAsync(options);

			//in case they cancel the photo taking.
			if (image == null) return;

			var upload = new PVImageUpload(IsRecognizeCommand ? GestureCommand.Recognize : GestureCommand.Train);
			GestureImages.Add(upload);

			await upload.UploadImage(image);// ? "Success!" : "Failure";

			//TriggerUploadMessage(await upload.UploadImage(image) ? "Success!" : "Failure");
			//var success = await upload.UploadImage(image)
			//System.Console.WriteLine("Picture upload: " + await upload.UploadImage(image));
		}

		private async Task<bool> CheckCameraPermission()
		{
			var cameraGranted = (await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera) == PermissionStatus.Granted);

			if (!cameraGranted)
			{
				return await RequestCameraPermission();
			}

			return true;
		}

		private async Task<bool> RequestCameraPermission()
		{
			var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);

			bool status = false;

			if (results.ContainsKey(Permission.Camera))
			{
				status = (results[Permission.Camera] == PermissionStatus.Granted);
			}
			return status;
		}

		#region Events
		public class MyEventArgs : EventArgs
		{
			public string Message { get; set; }
		}

		public event EventHandler<MyEventArgs> UploadMessage;

		public void TriggerUploadMessage(string message)
		{
			if (UploadMessage != null)
			{
				UploadMessage(null, new MyEventArgs { Message = message });
			}
		}

		#endregion
	}
}
