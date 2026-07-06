////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using Steamworks;

namespace SDG.Unturned
{
	public static class LiveConfig
	{
		/// <summary>
		/// Called during startup and when returning to the main menu.
		/// </summary>
		public static void Refresh()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (useEditorLiveConfig)
			{
				if (LiveConfigManager.Get().HasEditorLiveConfigFile())
				{
					float delaySeconds = shouldDelayEditorLiveConfig ? 30.0f : 0.0f;
					LiveConfigManager.Get().LoadFromFile(delaySeconds);
					return;
				}
				else
				{
					UnturnedLog.info($"Ignoring {useEditorLiveConfig.flag} because file does not exist");
				}
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			if (SteamUser.BLoggedOn() && Provider.allowWebRequests)
			{
				LiveConfigManager.Get().Refresh();
			}
		}

		/// <summary>
		/// Result is never null, but may be empty or out-of-date.
		/// </summary>
		public static LiveConfigData Get()
		{
			return LiveConfigManager.Get().config;
		}

		public static bool WasPopulated => LiveConfigManager.Get().wasPopulated;

		public static event System.Action OnRefreshed
		{
			add => LiveConfigManager.Get().OnConfigRefreshed += value;
			remove => LiveConfigManager.Get().OnConfigRefreshed -= value;
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		internal static CommandLineFlag useEditorLiveConfig = new CommandLineFlag(false, "-EditorLiveConfig");
		internal static CommandLineFlag shouldDelayEditorLiveConfig = new CommandLineFlag(false, "-DelayEditorLiveConfig");
#endif // !UNITY_EDITOR || DEVELOPMENT_BUILD
	}

	public static class LiveConfigEx
	{
		public static bool IsNowFeaturedTimeOrBypassed(this MainMenuWorkshopFeaturedLiveConfig config)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (LiveConfig.useEditorLiveConfig)
			{
				return true;
			}
#endif // !UNITY_EDITOR || DEVELOPMENT_BUILD

			return config.IsNowFeaturedTime;
		}
	}
}
#endif // !DEDICATED_SERVER
