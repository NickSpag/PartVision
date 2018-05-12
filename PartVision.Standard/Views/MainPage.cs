using System;

using Xamarin.Forms;

namespace PartVision.Standard
{
	public class MainPage : TabbedPage
	{
		public MainPage()
		{

			Page workPage, cameraPage = null;

			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					cameraPage = new NavigationPage(new CameraPage())
					{
						Title = "Camera"
					};
					workPage = new NavigationPage(new WorkPage())
					{
						Title = "Work"
					};
					break;
				default:
					cameraPage = new CameraPage()
					{
						Title = "Camera"
					};
					workPage = new WorkPage()
					{
						Title = "Work"
					};
					break;
			}

			Children.Add(cameraPage);
			Children.Add(workPage);

			Title = Children[0].Title;
		}

		protected override async void OnAppearing()
		{
			if (!User.CurrentRole.HasValue)
			{
				await Navigation.PushModalAsync(new EntryPage());
			}
			else
			{
				//populate tabs
			}

			base.OnAppearing();
		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			Title = CurrentPage?.Title ?? string.Empty;
		}
	}
}
