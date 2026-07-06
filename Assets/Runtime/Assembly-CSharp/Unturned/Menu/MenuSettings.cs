////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class MenuSettings
	{
		private static bool hasLoaded;

		public static void load()
		{
			FilterSettings.load();
			PlaySettings.load();
			GraphicsSettings.load();
			ControlsSettings.load();
			OptionsSettings.load();

			hasLoaded = true;
		}

		public static void save()
		{
			if (!hasLoaded)
			{
				UnturnedLog.info("Skipping saving menu settings because they were not loaded");
				return;
			}

			FilterSettings.save();
			PlaySettings.save();
			GraphicsSettings.save();
			ControlsSettings.save();
			OptionsSettings.save();
			UnturnedLog.info("Saved menu settings");
		}

		public static void SaveGraphicsIfLoaded()
		{
			if (hasLoaded)
			{
				GraphicsSettings.save();
			}
		}

		public static void SaveControlsIfLoaded()
		{
			if (hasLoaded)
			{
				ControlsSettings.save();
			}
		}

		public static void SaveOptionsIfLoaded()
		{
			if (hasLoaded)
			{
				OptionsSettings.save();
			}
		}
	}
}
