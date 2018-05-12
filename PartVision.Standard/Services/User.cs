using System;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;

namespace PartVision.Standard
{
	public static class User
	{
		public static UserRole? CurrentRole => Help.Settings.CurrentRole;

		public static async Task<bool> SetRole(UserRole role)
		{
			switch (role)
			{
				case UserRole.Sighted:
					return await RegisterSightedUser();
				case UserRole.Unsighted:
				default:
					return await RegisterUnsightedUser();
			}
		}

		public static async Task<bool> RegisterUnsightedUser()
		{
			Help.Settings.CurrentRole = UserRole.Unsighted;

			return await Help.Permissions.RequestIfNotGranted(Permission.Camera);
		}


		public static async Task<bool> RegisterSightedUser()
		{
			Help.Settings.CurrentRole = UserRole.Sighted;

			//await Help.Permissions.RequestIfNotGranted(Permission.)
			//get notification permissions
			//hit notifications api and register device for sightedrequest tag
			return false;
		}

		public static void CheckExistingUserRole()
		{

		}

	}
}
