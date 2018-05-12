using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace PartVision.Standard
{
	public partial class EntryPage : ContentPage
	{
		public EntryPage()
		{
			InitializeComponent();
		}

		EntryPageViewModel ViewModel;

		protected override void OnBindingContextChanged()
		{
			if (BindingContext is EntryPageViewModel viewModel)
			{
				ViewModel = viewModel;
				ViewModel.SelectionFinished += ViewModel_SelectionFinished;
			}

			base.OnBindingContextChanged();
		}

		private async void ViewModel_SelectionFinished(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync(true);
		}

	}
}
