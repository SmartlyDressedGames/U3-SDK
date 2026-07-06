////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	internal class OwnershipTool
	{
		// checks using local client info
		public static bool checkToggle(ulong player, ulong group)
		{
			if (Dedicator.IsDedicatedServer)
			{
				return false;
			}

			return checkToggle(Provider.client, player, Player.LocalPlayer.quests.groupID, group);
		}

		public static bool checkToggle(CSteamID player_0, ulong player_1, CSteamID group_0, ulong group_1)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer)
			{
				return true;
			}

			return player_0.m_SteamID == player_1 || (group_0 != CSteamID.Nil && group_0.m_SteamID == group_1);
		}
	}
}
