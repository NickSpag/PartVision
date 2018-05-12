using System;
using Android.Hardware.Camera2;

namespace PartVision.Droid
{

    public class PreviewCaptureStateCallback : CameraCaptureSession.StateCallback
    {
        CaptureCameraFrames frameCapturer;

        public PreviewCaptureStateCallback(CaptureCameraFrames capturer)
        {
            frameCapturer = capturer;
        }
        public override void OnConfigured(CameraCaptureSession session)
        {
            frameCapturer.UpdatePreviewSession(session);
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            //if (null != fragment.Activity)
            //Toast.MakeText(fragment.Activity, "Failed", ToastLength.Short).Show();
            System.Console.WriteLine("configuration failed");
        }
    }
}
