using System;

namespace PartVision.Standard
{
	public class PVImageUpload : BaseModel
	{
		private string _status = "Preparing";
		public string Status
		{
			get => _status;
			set
			{
				_status = value;
				OnPropertyChanged();
			}
		}

		GestureCommand commandType;

		public PVImageUpload(GestureCommand gestureCommandType)
		{
			commandType = gestureCommandType;
		}

		public async Task<bool> UploadImage(MediaFile image)
		{
			var stream = image.GetStream();

			Status = "Uploading";

			var success = await VisionService.UploadHandGestureImage(commandType, stream);

			Status = success ? "Finished" : "Failed";

			return success;
		}
	}
}
