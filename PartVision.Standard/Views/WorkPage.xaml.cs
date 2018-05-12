using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace PartVision.Standard
{
	public partial class WorkPage : ContentPage
	{
		public WorkPage()
		{
			InitializeComponent();
		}

		public WorkViewModel ViewModel;

		protected override void OnBindingContextChanged()
		{
			if (BindingContext is WorkViewModel viewModel)
			{
				ViewModel = viewModel;
			}

			base.OnBindingContextChanged();
		}
	}
}
