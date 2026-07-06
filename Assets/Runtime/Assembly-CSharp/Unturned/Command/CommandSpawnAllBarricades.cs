////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandSpawnAllBarricades : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (!Provider.hasCheats)
				return;

			SteamPlayer player = PlayerTool.getSteamPlayer(executorID);
			if (player == null)
				return;

			List<ItemBarricadeAsset> assets = new List<ItemBarricadeAsset>();
			Assets.find(assets);

			Vector3 position = player.player.look.aim.position + (player.player.look.aim.forward * 5.0f);

			foreach (ItemBarricadeAsset asset in assets)
			{
				Barricade barricade = new Barricade(asset);
				BarricadeManager.dropBarricade(barricade, null, position, 0.0f, 0.0f, 0.0f, player.playerID.steamID.m_SteamID, player.player.quests.groupID.m_SteamID);
			}
		}

		public CommandSpawnAllBarricades(Local newLocalization)
		{
			localization = newLocalization;
			_command = "SpawnAllBarricades";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
