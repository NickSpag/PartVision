using System;
using System.Windows.Input;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace PartVision.Standard
{
	public class EntryPageViewModel : BaseViewModel
	{
		public EntryPageViewModel()
		{
			SelectUserRoleCommand = new Command<UserRole>(SelectUserRole);
		}

		public ICommand SelectUserRoleCommand { get; }

		private async void SelectUserRole(UserRole role)
		{
			await User.SetRole(role);

			OnSelectionFinished(new EventArgs());
		}

		public event EventHandler SelectionFinished;

		protected virtual void OnSelectionFinished(EventArgs e)
		{
			var handler = SelectionFinished;

			if (handler != null)
			{
				handler(this, e);
			}
		}
	}
}