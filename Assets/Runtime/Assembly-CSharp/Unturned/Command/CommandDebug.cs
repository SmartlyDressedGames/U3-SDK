////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandDebug : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			CommandWindow.Log(localization.format("DebugText"));
			CommandWindow.Log(localization.format("DebugUPSText", Mathf.CeilToInt(Provider.debugUPS / 50.0f * 100.0f)));
			CommandWindow.Log(localization.format("DebugTPSText", Mathf.CeilToInt(Provider.debugTPS / 50.0f * 100.0f)));
			CommandWindow.Log(localization.format("DebugZombiesText", ZombieManager.tickingZombies.Count));
			CommandWindow.Log(localization.format("DebugAnimalsText", AnimalManager.tickingAnimals.Count));

#if STATDEBUG
			List<SteamChannelMethod> methods = new List<SteamChannelMethod>();
			for(int channelIndex = 0; channelIndex < Provider.receivers.Count; channelIndex ++)
			{
				SteamChannel channel = Provider.receivers[channelIndex];

				for(int callIndex = 0; callIndex < channel.calls.Length; callIndex ++)
				{
					SteamChannelMethod method = channel.calls[callIndex];

					methods.Add(method);
				}
			}

			bool sorted = false;
			while(!sorted)
			{
				bool swapped = false;
				
				for(int index = 0; index < methods.Count - 1; index ++)
				{
					SteamChannelMethod method_0 = (SteamChannelMethod) methods[index];
					SteamChannelMethod method_1 = (SteamChannelMethod) methods[index + 1];
					
					if(method_0.sent < method_1.sent)
					{
						methods[index] = method_1;
						methods[index + 1] = method_0;
						
						swapped = true;
					}
				}
				
				if(!swapped)
				{
					sorted = true;
				}
			}

			CommandWindow.Log("SENT");
			for(int index = 0; index < methods.Count; index ++)
			{
				SteamChannelMethod method = methods[index];

				if(method.sent > 0)
				{
					CommandWindow.Log(method.method.Name + " : " + method.sent);
				}
			}

			sorted = false;
			while(!sorted)
			{
				bool swapped = false;
				
				for(int index = 0; index < methods.Count - 1; index ++)
				{
					SteamChannelMethod method_0 = (SteamChannelMethod) methods[index];
					SteamChannelMethod method_1 = (SteamChannelMethod) methods[index + 1];
					
					if(method_0.received < method_1.received)
					{
						methods[index] = method_1;
						methods[index + 1] = method_0;
						
						swapped = true;
					}
				}
				
				if(!swapped)
				{
					sorted = true;
				}
			}

			CommandWindow.Log("RECEIVED");
			for(int index = 0; index < methods.Count; index ++)
			{
				SteamChannelMethod method = methods[index];

				if(method.received > 0)
				{
					CommandWindow.Log(method.method.Name + " : " + method.received);
				}
			}

#endif
		}

		public CommandDebug(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("DebugCommandText");
			_info = localization.format("DebugInfoText");
			_help = localization.format("DebugHelpText");
		}
	}
}
