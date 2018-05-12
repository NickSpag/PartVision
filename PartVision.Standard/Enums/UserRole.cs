using System;

namespace PartVision.Standard
{
	public static partial class Extensions
	{
		public static string AsString(this UserRole role)
		{
			switch (role)
			{
				case UserRole.Sighted:
					return "Sighted";

				default:
				case UserRole.Unsighted:
					return "Unsighted";

			}
		}
	}

	public enum UserRole
	{
		Sighted,
		Unsighted
	}
}
