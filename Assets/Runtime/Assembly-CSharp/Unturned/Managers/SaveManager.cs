////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public delegate void SaveHandler();

	public class SaveManager : SteamCaller
	{
		public static SaveHandler onPreSave;
		public static SaveHandler onPostSave;

		private static void broadcastPreSave()
		{
			try
			{
				onPreSave?.Invoke();
			}
			catch (System.Exception exception)
			{
				UnturnedLog.warn("Plugin raised exception during onPreSave:");
				UnturnedLog.exception(exception);
			}
		}

		private static void broadcastPostSave()
		{
			try
			{
				onPostSave?.Invoke();
			}
			catch (System.Exception exception)
			{
				UnturnedLog.warn("Plugin raised exception during onPostSave:");
				UnturnedLog.exception(exception);
			}
		}

		public static void save()
		{
#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			if (!Level.isLoaded)
			{
				// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2666#issuecomment-869121335
				UnturnedLog.warn("Ignoring request to save before level finished loading");
				return;
			}

			broadcastPreSave();

			if (Level.info != null && Level.info.type == ELevelType.SURVIVAL)
			{
				foreach (SteamPlayer client in Provider.clients)
				{
					if (client == null || client.player == null)
						continue;

					client.player.save();
				}

				VehicleManager.save();
				BarricadeManager.save();
				StructureManager.save();
				ObjectManager.save();
				LightingManager.save();
				GroupManager.save();
			}

			if (Dedicator.IsDedicatedServer)
			{
				SteamWhitelist.save();
				SteamBlacklist.save();
				SteamAdminlist.save();
			}

			broadcastPostSave();
		}

		private static void onServerShutdown()
		{
			if (Provider.isServer && Level.isLoaded)
			{
				// Nelson 2024-10-04: Server is closing or singleplayer is exiting, so apply any delayed quest rewards before save.
				foreach (SteamPlayer client in Provider.clients)
				{
					if (client != null && client.player != null)
					{
						try
						{
							// Nelson 2024-10-04: Player is disconnecting, so apply any delayed quest rewards before save.
							client.player.quests.InterruptDelayedQuestRewards(EDelayedQuestRewardsInterruption.Shutdown);
						}
						catch (System.Exception exception)
						{
							UnturnedLog.exception(exception, "Caught exception interrupting delayed quest rewards during shutdown:");
						}
					}
				}

				UnturnedLog.info("Saving during server shutdown");
				save();
			}
		}

		private static void onServerDisconnected(CSteamID steamID)
		{
			if (Provider.isServer && Level.isLoaded)
			{
				Player player = PlayerTool.getPlayer(steamID);

				if (player != null)
				{
					try
					{
						// Nelson 2024-10-04: Player is disconnecting, so apply any delayed quest rewards before save.
						player.quests.InterruptDelayedQuestRewards(EDelayedQuestRewardsInterruption.Disconnection);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, "Caught exception interrupting delayed quest rewards during disconnect:");
					}

					player.save();
				}
			}
		}

		private void Start()
		{
			//manager = this;

			Provider.onServerShutdown += onServerShutdown;
			Provider.onServerDisconnected += onServerDisconnected;
		}
	}
}
