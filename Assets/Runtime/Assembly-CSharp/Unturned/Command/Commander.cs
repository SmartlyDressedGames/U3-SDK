////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class Commander
	{
		public static List<Command> commands
		{
			get;
			set;
		}

		public static void register(Command command)
		{
			int insert = commands.BinarySearch(command);

			if (insert < 0)
			{
				insert = ~insert;
			}

			commands.Insert(insert, command);
		}

		public static void deregister(Command command) // mods use this method
		{
			commands.Remove(command);
		}

		public static bool execute(CSteamID executorID, string command)
		{
			try
			{
				string method = command;
				string parameter = "";

				int split = command.IndexOf(' ');
				if (split != -1)
				{
					method = command.Substring(0, split);
					parameter = command.Substring(split + 1, command.Length - split - 1);
				}

				for (int index = 0; index < commands.Count; index++)
				{
					if (commands[index].check(executorID, method, parameter))
					{
						return true;
					}
				}
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Caught exception while executing command string \"{0}\"", command);
			}

			return false;
		}

		public delegate void ServerUnityEventPermissionHandler(ServerTextChatMessenger messenger, string command, ref bool shouldAllow);
		public static event ServerUnityEventPermissionHandler onCheckUnityEventPermissions;

		/// <summary>
		/// Allows Unity events to execute commands from the server.
		/// Messenger context is logged to help track down abusive assets.
		/// </summary>
		public static void execute_UnityEvent(string command, ServerTextChatMessenger messenger)
		{
			if (messenger == null)
				throw new System.ArgumentNullException("messenger");

			if (Dedicator.IsDedicatedServer && !Provider.configData.UnityEvents.Allow_Server_Commands)
			{
				UnturnedLog.info($"Blocking ServerTextChatMessenger component at {messenger.gameObject.GetSceneHierarchyPath()} from executing command \"{command}\" because UnityEvents.Allow_Server_Commands is off");
				return;
			}

			bool shouldAllow = true;
			onCheckUnityEventPermissions?.Invoke(messenger, command, ref shouldAllow);

			UnturnedLog.info("UnityEventCmd {0}: '{1}' Allow: {2}", messenger.gameObject.GetSceneHierarchyPath(), command, shouldAllow);

			if (shouldAllow)
			{
				execute(CSteamID.Nil, command);
			}
		}

		public static void init()
		{
			commands = new List<Command>();

			Local emptyPlaceholder = new Local();

			register(new CommandModules(Localization.read("/Server/ServerCommandModules.dat")));
			register(new CommandReload(Localization.read("/Server/ServerCommandReload.dat")));
			register(new CommandHelp(Localization.read("/Server/ServerCommandHelp.dat")));
			register(new CommandName(Localization.read("/Server/ServerCommandName.dat")));
			register(new CommandPort(Localization.read("/Server/ServerCommandPort.dat")));
			register(new CommandPassword(Localization.read("/Server/ServerCommandPassword.dat")));
			register(new CommandMaxPlayers(Localization.read("/Server/ServerCommandMaxPlayers.dat")));
			register(new CommandQueue(Localization.read("/Server/ServerCommandQueue.dat")));
			register(new CommandMap(Localization.read("/Server/ServerCommandMap.dat")));
			register(new CommandPvE(Localization.read("/Server/ServerCommandPvE.dat")));
			register(new CommandWhitelisted(Localization.read("/Server/ServerCommandWhitelisted.dat")));
			register(new CommandCheats(Localization.read("/Server/ServerCommandCheats.dat")));
			register(new CommandHideAdmins(Localization.read("/Server/ServerCommandHideAdmins.dat")));
			register(new CommandEffectUI(Localization.read("/Server/ServerCommandEffectUI.dat")));
			register(new CommandSync(Localization.read("/Server/ServerCommandSync.dat")));
			register(new CommandFilter(Localization.read("/Server/ServerCommandFilter.dat")));
			register(new CommandVotify(Localization.read("/Server/ServerCommandVotify.dat")));
			register(new CommandMode(Localization.read("/Server/ServerCommandMode.dat")));
			register(new CommandGameMode(Localization.read("/Server/ServerCommandGameMode.dat")));
			register(new CommandGold(Localization.read("/Server/ServerCommandGold.dat")));
			register(new CommandCamera(Localization.read("/Server/ServerCommandCamera.dat")));

			register(new CommandCycle(Localization.read("/Server/ServerCommandCycle.dat")));
			register(new CommandTime(Localization.read("/Server/ServerCommandTime.dat")));
			register(new CommandDay(Localization.read("/Server/ServerCommandDay.dat")));
			register(new CommandNight(Localization.read("/Server/ServerCommandNight.dat")));
			register(new CommandWeather(Localization.read("/Server/ServerCommandWeather.dat")));
			register(new CommandAirdrop(Localization.read("/Server/ServerCommandAirdrop.dat")));

			register(new CommandKick(Localization.read("/Server/ServerCommandKick.dat")));
			register(new CommandSpy(Localization.read("/Server/ServerCommandSpy.dat")));

			register(new CommandBan(Localization.read("/Server/ServerCommandBan.dat")));
			register(new CommandUnban(Localization.read("/Server/ServerCommandUnban.dat")));
			register(new CommandBans(Localization.read("/Server/ServerCommandBans.dat")));

			register(new CommandAdmin(Localization.read("/Server/ServerCommandAdmin.dat")));
			register(new CommandUnadmin(Localization.read("/Server/ServerCommandUnadmin.dat")));
			register(new CommandAdmins(Localization.read("/Server/ServerCommandAdmins.dat")));
			register(new CommandOwner(Localization.read("/Server/ServerCommandOwner.dat")));

			register(new CommandPermit(Localization.read("/Server/ServerCommandPermit.dat")));
			register(new CommandUnpermit(Localization.read("/Server/ServerCommandUnpermit.dat")));
			register(new CommandPermits(Localization.read("/Server/ServerCommandPermits.dat")));

			register(new CommandPlayers(Localization.read("/Server/ServerCommandPlayers.dat")));
			register(new CommandSay(Localization.read("/Server/ServerCommandSay.dat")));
			register(new CommandWelcome(Localization.read("/Server/ServerCommandWelcome.dat")));

			register(new CommandSlay(Localization.read("/Server/ServerCommandSlay.dat")));
			register(new CommandKill(Localization.read("/Server/ServerCommandKill.dat")));
			register(new CommandGive(Localization.read("/Server/ServerCommandGive.dat")));
			register(new CommandUnlockNpcAchievement(emptyPlaceholder));
			register(new CommandScheduledShutdownInfo(emptyPlaceholder));
			register(new CommandSetNpcSpawnId(emptyPlaceholder));
			register(new CommandToggleNpcCutsceneMode(emptyPlaceholder));
			register(new CommandNpcEvent(emptyPlaceholder));
			register(new CommandLoadout(Localization.read("/Server/ServerCommandLoadout.dat")));
			register(new CommandExperience(Localization.read("/Server/ServerCommandExperience.dat")));
			register(new CommandReputation(Localization.read("/Server/ServerCommandReputation.dat")));
			register(new CommandFlag(Localization.read("/Server/ServerCommandFlag.dat")));
			register(new CommandQuest(Localization.read("/Server/ServerCommandQuest.dat")));
			register(new CommandVehicle(Localization.read("/Server/ServerCommandVehicle.dat")));
			register(new CommandAnimal(Localization.read("/Server/ServerCommandAnimal.dat")));
			register(new CommandTeleport(Localization.read("/Server/ServerCommandTeleport.dat")));

			register(new CommandTimeout(Localization.read("/Server/ServerCommandTimeout.dat")));
			register(new CommandChatrate(Localization.read("/Server/ServerCommandChatrate.dat")));
			register(new CommandLog(Localization.read("/Server/ServerCommandLog.dat")));
			register(new CommandLogMemoryUsage(emptyPlaceholder));
			register(new CommandLogTransportConnections(emptyPlaceholder));
			register(new CommandCopyServerCode(emptyPlaceholder));
			register(new CommandCopyFakeIP(emptyPlaceholder));
			register(new CommandDebug(Localization.read("/Server/ServerCommandDebug.dat")));
			register(new CommandBind(Localization.read("/Server/ServerCommandBind.dat")));
			register(new CommandSave(Localization.read("/Server/ServerCommandSave.dat")));
			register(new CommandShutdown(Localization.read("/Server/ServerCommandShutdown.dat")));
			register(new CommandGSLT(Localization.read("/Server/ServerCommandGSLT.dat")));

			register(new CommandDestroyDrivenVehicle(emptyPlaceholder));
			register(new CommandExitAndDestroyDrivenVehicle(emptyPlaceholder));
			register(new CommandEnterAndDestroyNearestVehicle(emptyPlaceholder));
			register(new CommandRewardList(emptyPlaceholder));
			register(new CommandDialogue(emptyPlaceholder));

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			register(new CommandLogAssetOrigins(emptyPlaceholder));
			register(new CommandSpawnAllBarricades(emptyPlaceholder));
			register(new CommandSpawnAllVehicles(emptyPlaceholder));
			register(new CommandSteamClearAchievement(emptyPlaceholder));
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}
	}
}
