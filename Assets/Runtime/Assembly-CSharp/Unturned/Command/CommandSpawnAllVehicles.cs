////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class CommandSpawnAllVehicles : Command
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

			List<VehicleAsset> assets = new List<VehicleAsset>();
			Assets.find(assets);

			foreach (VehicleAsset vehicleAsset in assets)
			{
				VehicleTool.SpawnVehicleForPlayer(player.player, vehicleAsset);
			}
		}

		public CommandSpawnAllVehicles(Local newLocalization)
		{
			localization = newLocalization;
			_command = "SpawnAllVehicles";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
