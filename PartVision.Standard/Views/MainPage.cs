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
				default:
					cameraPage = new CameraPage()
					{
						Title = "Camera"
					};
					workPage = new WorkPage()
					{
						Title = "Work"
					};
					//itemsPage = new NavigationPage(new ItemsPage())
					//{
					//    Title = "Browse"
					//};
					//aboutPage = new NavigationPage(new AboutPage())
					//{
					//    Title = "About"
					//};
					//itemsPage.Icon = "tab_feed.png";
					//aboutPage.Icon = "tab_about.png";
					break;
					//default:
					//cameraPage = new CameraPage()
					//{
					//    Title = "Camera"
					//};
					//itemsPage = new ItemsPage()
					//{
					//    Title = "Browse"
					//};

					//aboutPage = new AboutPage()
					//{
					//    Title = "About"
					//};
					//break;
			}
			Children.Add(cameraPage);
			Children.Add(workPage);
			//Children.Add(itemsPage);
			//Children.Add(aboutPage);

			Title = Children[0].Title;
		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			Title = CurrentPage?.Title ?? string.Empty;
		}
	}
}
