using System;
using Plugin.Settings.Abstractions;
using Plugin.Settings;
namespace PartVision.Standard
{
	public static partial class Help
	{

		public static class Settings
		{
			private static ISettings appSettings => CrossSettings.Current;

			public static UserRole? CurrentRole
			{
				get
				{
					switch (appSettings.GetValueOrDefault(nameof(CurrentRole), string.Empty))
					{
						case "Sighted":
							return UserRole.Sighted;
						case "Unsighted":
							return UserRole.Unsighted;
						default:
							return null;
					}
				}
				set
				{
					if (value.HasValue)
					{
						appSettings.AddOrUpdateValue(nameof(CurrentRole), value.Value.AsString());
					}
				}
			}
		}
	}
}
