////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandDestroyDrivenVehicle : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (!Provider.hasCheats)
				return;

			SteamPlayer player = PlayerTool.getSteamPlayer(executorID);
			if (player == null || player.player == null)
				return;

			InteractableVehicle vehicle = player.player.movement.getVehicle();
			if (vehicle == null)
				return;

			VehicleManager.askVehicleDestroy(vehicle);
			UnityEngine.Debug.Assert(player.player.movement.getVehicle() == null, "Vehicle should be null");
			UnityEngine.Debug.Assert(!player.player.movement.hasPendingVehicleChange, "Pending vehicle change should be canceled");
		}

		public CommandDestroyDrivenVehicle(Local newLocalization)
		{
			localization = newLocalization;
			_command = "DestroyDrivenVehicle";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
