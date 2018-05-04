using System;

using Xamarin.Forms;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;

namespace PartVision.Standard
{
	public partial class App : Application
	{
		public static bool UseMockDataStore = true;
		public static string BackendUrl = "https://localhost:5000";

		public App()
		{
			InitializeComponent();

			if (UseMockDataStore)
				DependencyService.Register<MockDataStore>();
			else
				DependencyService.Register<CloudDataStore>();

			if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
				MainPage = new MainPage();
			else
				MainPage = new NavigationPage(new MainPage());

			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		}

		protected override void OnStart()
		{
			base.OnStart();

			AppCenter.Start("android=4ab05b6b-d53d-49eb-a08c-27d3d9dea8be;" +
							"ios={619623ea-632d-4327-9ff5-52a0c6957d78}",
							typeof(Analytics), typeof(Crashes));
		}

		void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			System.Console.WriteLine("**Unobserved Exception**");
			System.Console.WriteLine(e.Exception.Message);

			if (e.Exception.InnerExceptions.Count > 0)
			{
				foreach (var innerException in e.Exception.InnerExceptions)
				{
					System.Console.WriteLine($"Inner exceptions: {innerException.InnerException.Message}");
				}
			}
		}
	}
}
