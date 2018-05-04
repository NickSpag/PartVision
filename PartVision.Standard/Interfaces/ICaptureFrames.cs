using System;
using System.IO;

namespace PartVision.Standard
{
	public interface ICaptureFrames
	{
		void BeginCapture();
		void StopCapture();

		void TakeStill(Action<byte[]> bytes);
	}
}
