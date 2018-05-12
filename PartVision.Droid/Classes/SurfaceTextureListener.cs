using System;
using Android.Graphics;
using Android.Views;

namespace PartVision.Droid
{
    public class SurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        CaptureCameraFrames frameCapturer;
        public SurfaceTextureListener(CaptureCameraFrames frag)
        {
            frameCapturer = frag;
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface_texture, int width, int height)
        {
            //originally frameCapturer's OnResume would open the camera if the texture was availabile,
            //if it wasnt it would configure the texture and use this listener/method  to open the camera once 
            //this method was called and the texture became available
            //since we aren't actually displaying the texture this entire class might be irellevant

            //frameCapturer.openCamera(width, height);
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface_texture, int width, int height)
        {
            //frameCapturer.configureTransform(width, height);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface_texture)
        {
            return true;
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface_texture)
        {
        }

    }

}
