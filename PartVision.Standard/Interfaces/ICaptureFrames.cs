using System;
using System.IO;

namespace PartVision.Standard
{
	public interface ICaptureFrames
	{
		void BeginCapture();
		void StopCapture();

		void WatchForGesture(Action<GestureCommand> gestureCallback);
		void WatchForPart(Action<string> partCallback);

		void TakeStill(Action<byte[]> bytes);
	}
}
