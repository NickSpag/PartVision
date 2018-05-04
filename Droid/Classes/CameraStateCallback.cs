using System;
using Android.Hardware.Camera2;
using Android.Widget;

namespace PartVision.Droid
{
    public class CameraStateCallback : CameraDevice.StateCallback
    {
        CaptureCameraFrames fragment;

        public CameraStateCallback(CaptureCameraFrames captureFragment)
        {
            fragment = captureFragment;
        }

        public override void OnOpened(CameraDevice camera)
        {
            fragment.cameraDevice = camera;

            fragment.StartPreviewing();

            fragment.cameraOpenCloseLock.Release();

            //if (fragment.textureView != null)
            //fragment.configureTransform(fragment.textureView.Width, fragment.textureView.Height);
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            fragment.cameraOpenCloseLock.Release();
            camera.Close();
            fragment.cameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            fragment.cameraOpenCloseLock.Release();
            camera.Close();
            fragment.cameraDevice = null;

            if (fragment.Activity != null)
                fragment.Activity.Finish();
        }



    }
}
