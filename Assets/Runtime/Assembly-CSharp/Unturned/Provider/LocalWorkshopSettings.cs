////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public interface ILocalWorkshopSettings
	{
		bool getEnabled(PublishedFileId_t fileId);
		void setEnabled(PublishedFileId_t fileId, bool newEnabled);
	}

	public class LocalWorkshopSettings
	{
		public static ILocalWorkshopSettings get()
		{
			if (instance == null)
			{
				instance = new LocalWorkshopSettingsImplementation();
			}

			return instance;
		}

		private static ILocalWorkshopSettings instance;
	}

	internal class LocalWorkshopSettingsImplementation : ILocalWorkshopSettings
	{
		public bool getEnabled(PublishedFileId_t fileId)
		{
			string key = formatEnabledKey(fileId);
			bool enabled;
			if (ConvenientSavedata.get().read(key, out enabled))
			{
				return enabled;
			}
			else
			{
				// Enabled by default.
				return true;
			}
		}

		public void setEnabled(PublishedFileId_t fileId, bool newEnabled)
		{
			string key = formatEnabledKey(fileId);
			ConvenientSavedata.get().write(key, newEnabled);
		}

		private string formatEnabledKey(PublishedFileId_t fileId)
		{
			return "Enabled_Workshop_Item_" + fileId;
		}
	}
}
