using System;
using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using Vision;
using CoreML;
using System.Runtime.CompilerServices;
using ImageIO;
using CoreFoundation;
using PartVision.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(AVCapture))]
namespace PartVision.iOS
{
    public class AVCapture : AVCaptureVideoDataOutputSampleBufferDelegate, ICaptureFrames
    {

        private AVCaptureSession captureSession;
        private AVCaptureDeviceInput deviceInput;
        private AVCaptureVideoDataOutput deviceOutput;
        private AVCaptureDevice captureDevice;

        private DispatchQueue queue = new DispatchQueue("videoQueue");

        private VNRequest[] requests;

        public bool isCapturing = false;

        public AVCapture()
        {

        }

        public void BeginCapture()
        {
            if (isCapturing)
            {
                StopCapture();
                return;
            }

            isCapturing = true;

            captureSession = new AVCaptureSession();
            captureSession.SessionPreset = AVCaptureSession.Preset352x288;

            PrepareSessionWithDevice(captureSession);

            PrepareVisionRequests();

            //ConfigureImageLayer(captureSession);

            captureSession.StartRunning();
        }

        private void PrepareSessionWithDevice(AVCaptureSession session)
        {
            try
            {
                captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);

                deviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
                deviceOutput = new AVCaptureVideoDataOutput();

                deviceOutput.WeakVideoSettings = new CVPixelBufferAttributes { PixelFormatType = CVPixelFormatType.CV32BGRA }.Dictionary;

                deviceOutput.SetSampleBufferDelegateQueue(this, queue);

                captureSession.AddInput(deviceInput);
                captureSession.AddOutput(deviceOutput);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        public void StopCapture()
        {
            if (captureSession != null)
            {
                captureSession.StopRunning();
                captureSession.Dispose();

                isCapturing = false;
            }
        }

        private MLModel PrepareCoreMLModel()
        {
            var modelPath = NSBundle.MainBundle.GetUrlForResource("firstTest", "mlmodelc");
            var model = MLModel.Create(modelPath, out NSError error1);

            CheckModelErrors(error1);

            return model;
        }

        private VNCoreMLModel PrepareModelForVision(MLModel model)
        {
            var visionOptimizedModel = VNCoreMLModel.FromMLModel(model, out NSError error);

            CheckModelErrors(error);

            return visionOptimizedModel;
        }

        private void PrepareVisionRequests()
        {
            var model = PrepareCoreMLModel();

            var mlRequest = new VNCoreMLRequest(PrepareModelForVision(model), HandleVNRequestCompletionHandler);

            requests = new VNRequest[] { mlRequest };
        }

        private void HandleVNRequestCompletionHandler(VNRequest request, NSError error)
        {
            var observations = request.GetResults<VNClassificationObservation>();

            foreach (var observation in observations)
            {
                RecordObservation(observation);
            }
        }

        //int recognizeCount = 0;
        //int trainCount = 0;

        private void RecordObservation(VNClassificationObservation observation)
        {
            //todo: observation logic
            System.Diagnostics.Debug.WriteLine($"{observation.Identifier} at {Math.Round(observation.Confidence, 2)}");

            //if (observation.Identifier.Contains("Recognize"))
            //{
            //    recognizeCount++;
            //}

            //if (recognizeCount > 2)
            //{
            //    BeginInvokeOnMainThread(() =>
            //    {
            //      CrossTextToSpeech.Current.Speak($"{observation.Identifier} at {Math.Round(observation.Confidence, 2)}");
            //    });
            //
            //    recognizeCount = 0;
            //}
        }

        private void CheckModelErrors(NSError error, [CallerMemberNameAttribute] string name = null)
        {
            if (error != null)
            {
                throw new Exception($"Core ML/Vission issue" + (string.IsNullOrEmpty(name) ? " detected" : $"at {name}" +
                                                                $"\n {error.Description}"));
            }
        }

        public void TakeStill(Action<byte[]> handler)
        {

        }

        #region Methods: IAVCaptureVideoDataOutputSampleBufferDelegate
        private int frameCount = 0;
        private static NSDictionary empty = new NSDictionary();

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            try
            {
                frameCount++;

                if (frameCount > 5)
                {
                    frameCount = 0;

                    System.Console.WriteLine("Requesting");
                    PerformRequestOnFrame(sampleBuffer.GetImageBuffer());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                sampleBuffer.Dispose();
            }
        }

        private void PerformRequestOnFrame(CVImageBuffer imageBuffer)
        {
            using (var pixelBuffer = imageBuffer as CVPixelBuffer)
            using (var imageRequestHandler = new VNImageRequestHandler(pixelBuffer, CGImagePropertyOrientation.Right, empty))
            {
                if (requests != null)
                {
                    imageRequestHandler.Perform(requests, out NSError error);

                    if (error != null)
                    {
                        System.Diagnostics.Debug.WriteLine("NSerror on perform request: " + error?.Description);
                    }
                }
            }
        }

        public override void DidDropSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            System.Console.WriteLine("Dropped frame.");
            sampleBuffer.Dispose();
        }
        #endregion
    }
}
