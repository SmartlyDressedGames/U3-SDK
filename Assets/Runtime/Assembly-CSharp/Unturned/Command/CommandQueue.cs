////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandQueue : Command
	{
		public static readonly byte MAX_NUMBER = 64;

		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (parameter == "a")
			{
				SteamPending dummy = new SteamPending();

				if (Provider.pending.Count == 1)
				{
					dummy.sendVerifyPacket();
				}

				Provider.pending.Add(dummy);
				CommandWindow.Log("add dummy");
				return;
			}
			else if (parameter == "r")
			{
				Provider.reject(CSteamID.Nil, ESteamRejection.PING);
				CommandWindow.Log("rmv dummy");
				return;
			}
			else if (parameter == "ad")
			{
				for (int index = 0; index < 12; index++)
				{
					Provider.pending.Add(new SteamPending(null, new SteamPlayerID(CSteamID.Nil, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, 0, 0, 0, Color.white, Color.white, Color.white, Color.white, false, 0, 0, 0, 0, 0, 0, 0, new ulong[0], EPlayerSkillset.NONE, "english", CSteamID.Nil, default));
					Provider.accept(new SteamPlayerID(CSteamID.Nil, 1, "dummy", "dummy", "dummy", CSteamID.Nil), true, true, 0, 0, 0, Color.white, Color.white, Color.white, Color.white, false, 0, 0, 0, 0, 0, 0, 0, new int[0], new string[0], new string[0], EPlayerSkillset.NONE, "english", CSteamID.Nil, default);
				}
			}
			else if (parameter == "rd")
			{
				for (int index = Provider.clients.Count - 1; index >= 0; index--)
				{
					Provider.kick(CSteamID.Nil, "");
				}
			}

			byte queueSize;
			if (!byte.TryParse(parameter, out queueSize))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			if (queueSize > MAX_NUMBER)
			{
				CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
				return;
			}

			Provider.queueSize = queueSize;
			CommandWindow.Log(localization.format("QueueText", queueSize));
		}

		public CommandQueue(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("QueueCommandText");
			_info = localization.format("QueueInfoText");
			_help = localization.format("QueueHelpText");
		}
	}
}
