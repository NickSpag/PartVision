using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace PartVision
{
    public partial class CameraPage : ContentPage
    {
        public CameraPage()
        {
            InitializeComponent();
        }

        private void DisplayMessage(object sender, CameraPageViewModel.MyEventArgs e)
        {
            DisplayAlert("Image Uploaded", e.Message, "Ok");
        }

        public CameraPageViewModel ViewModel { get; set; }

        protected override void OnBindingContextChanged()
        {
            if (BindingContext is CameraPageViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.UploadMessage += DisplayMessage;
            }

            base.OnBindingContextChanged();
        }
    }
}
