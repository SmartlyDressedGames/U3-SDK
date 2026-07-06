////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Nelson 2025-01-28: This command reproduces a bug destroying the player gameObject if the vehicle is
	/// destroyed on the same frame as the request to enter.
	/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/4760#issuecomment-2613090165
	/// </summary>
	public class CommandEnterAndDestroyNearestVehicle : Command
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

			InteractableVehicle currentVehicle = player.player.movement.getVehicle();
			if (currentVehicle != null)
			{
				CommandWindow.LogError("Cannot enter and destroy nearest vehicle if already driving");
				return;
			}

			InteractableVehicle vehicle = null;
			float closestDistance = 16.0f;
			foreach (InteractableVehicle testVehicle in VehicleManager.vehicles)
			{
				float distance = Vector3.Distance(testVehicle.transform.position, player.player.transform.position);
				if (distance < closestDistance)
				{
					vehicle = testVehicle;
					closestDistance = distance;
				}
			}

			if (vehicle == null)
			{
				CommandWindow.LogError("No nearby vehicle to enter and destroy");
				return;
			}

			// This is essentially what VehicleManager.ReceiveEnterVehicleRequest does.
			if (!vehicle.tryAddPlayer(out byte seatIndex, player.player))
			{
				CommandWindow.LogError("No seat for vehicle to enter and destroy");
				return;
			}

			VehicleManager.SendEnterVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), vehicle.instanceID, seatIndex, executorID);
			Debug.Assert(player.player.movement.getVehicle() == null, "Vehicle should be null due to deferral");
			Debug.Assert(player.player.movement.hasPendingVehicleChange, "Entering should be deferred");
			Debug.Assert(vehicle.findPlayerSeat(executorID, out byte newSeat), "Player should be seated during deferred enter");

			VehicleManager.askVehicleDestroy(vehicle);
			Debug.Assert(!vehicle.findPlayerSeat(executorID, out byte newSeat2), "Player should not be seated after destroy");
			Debug.Assert(player.player.movement.getVehicle() == null, "Vehicle should be null");
		}

		public CommandEnterAndDestroyNearestVehicle(Local newLocalization)
		{
			localization = newLocalization;
			_command = "EnterAndDestroyNearestVehicle";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
