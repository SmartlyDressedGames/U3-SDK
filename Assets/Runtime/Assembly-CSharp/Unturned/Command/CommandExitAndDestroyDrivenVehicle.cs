////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2025-01-28: This command reproduces a bug destroying the player gameObject if the vehicle is
	/// destroyed on the same frame as the request to exit.
	/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/4760#issuecomment-2613090165
	/// </summary>
	public class CommandExitAndDestroyDrivenVehicle : Command
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

			byte seat;
			Vector3 point;
			byte angle;
			bool wasSeated = vehicle.forceRemovePlayer(out seat, executorID, out point, out angle);
			Debug.Assert(wasSeated, "Was seated");
			// This is essentially what VehicleManager.ReceiveExitVehicleRequest does.
			VehicleManager.sendExitVehicle(vehicle, seat, point, angle, false);
			Debug.Assert(player.player.movement.getVehicle() == vehicle, "Vehicle should be valid due to deferral");
			Debug.Assert(player.player.movement.hasPendingVehicleChange, "Exiting should be deferred");
			Debug.Assert(!vehicle.findPlayerSeat(executorID, out byte newSeat), "Player should not be seated during deferred exit");
			VehicleManager.askVehicleDestroy(vehicle);
			Debug.Assert(player.player.movement.getVehicle() == null, "Vehicle should be null");
			Debug.Assert(!player.player.movement.hasPendingVehicleChange, "Pending vehicle change should be canceled");
		}

		public CommandExitAndDestroyDrivenVehicle(Local newLocalization)
		{
			localization = newLocalization;
			_command = "ExitAndDestroyDrivenVehicle";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
