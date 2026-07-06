////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Steamworks;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class CommandVehicle : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			if (!Provider.hasCheats)
			{
				CommandWindow.LogError(localization.format("CheatsErrorText"));
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length < 1 || components.Length > 3)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player;
			bool isMe = false;
			if (!PlayerTool.tryGetSteamPlayer(components[0], out player))
			{
				player = PlayerTool.getSteamPlayer(executorID);

				if (player == null)
				{
					CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
					return;
				}
				else
				{
					isMe = true;
				}
			}

			Asset vehicleAsset;
			string vehicleParameter = components[isMe ? 0 : 1];
			if (System.Guid.TryParse(vehicleParameter, out System.Guid vehicleAssetGuid))
			{
				vehicleAsset = Assets.find(vehicleAssetGuid);
			}
			else if (ushort.TryParse(vehicleParameter, out ushort vehicleLegacyId))
			{
				vehicleAsset = Assets.find(EAssetType.VEHICLE, vehicleLegacyId);
			}
			else
			{
				vehicleAsset = FindByString(vehicleParameter);
			}

			if (vehicleAsset == null)
			{
				CommandWindow.LogError(localization.format("NoVehicleIDErrorText", vehicleParameter));
				return;
			}

			InteractableVehicle spawnedVehicle = VehicleTool.SpawnVehicleForPlayer(player.player, vehicleAsset);
			if (spawnedVehicle == null)
			{
				CommandWindow.LogError(localization.format("NoVehicleIDErrorText", vehicleAsset.FriendlyName));
				return;
			}

			CommandWindow.Log(localization.format("VehicleText", player.playerID.playerName, spawnedVehicle.asset.FriendlyName));
		}

		public CommandVehicle(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("VehicleCommandText");
			_info = localization.format("VehicleInfoText");
			_help = localization.format("VehicleHelpText");
		}

		private Asset FindByString(string input)
		{
			input = input.Trim();
			if (string.IsNullOrEmpty(input))
				return null;

			List<VehicleAsset> allVehicleAssets = new List<VehicleAsset>();
			Assets.find(allVehicleAssets);

			// File name matches take priority because they tend to be more specific than the display name.
			// For example, "Ambulance_German" is just called "Ambulance" in-game.

			// Find exact matches by file name.
			foreach (VehicleAsset testAsset in allVehicleAssets)
			{
				if (string.Equals(input, testAsset.name, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			// Find exact matches by display name.
			foreach (VehicleAsset testAsset in allVehicleAssets)
			{
				if (string.Equals(input, testAsset.vehicleName, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			// Find partial matches by file name.
			foreach (VehicleAsset testAsset in allVehicleAssets)
			{
				if (testAsset.name.Contains(input, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			// Find partial matches by display name.
			foreach (VehicleAsset testAsset in allVehicleAssets)
			{
				if (testAsset.vehicleName.Contains(input, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			return null;
		}
	}
}
