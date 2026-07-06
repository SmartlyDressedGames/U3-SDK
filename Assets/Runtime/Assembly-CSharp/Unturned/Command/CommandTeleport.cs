////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class CommandTeleport : Command
	{
		/// <summary>
		/// Cast a ray from the sky to find highest point.
		/// </summary>
		protected bool raycastFromSkyToPosition(ref Vector3 position)
		{
			Vector3 origin = position;
			position.y = 1024;
			RaycastHit hit;
			if (Physics.Raycast(position, Vector3.down, out hit, 2048, RayMasks.WAYPOINT))
			{
				position = hit.point + Vector3.up;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Cast a ray from slightly above point so indoor teleport nodes work.
		/// </summary>
		protected void raycastFromNearPosition(ref Vector3 position)
		{
			RaycastHit hit;
			if (Physics.Raycast(position + new Vector3(0.0f, 4.0f, 0.0f), Vector3.down, out hit, 8.0f, RayMasks.WAYPOINT))
			{
				position = hit.point + Vector3.up;
			}
		}

		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length < 1 || components.Length > 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player_0;
			bool isMe = components.Length == 1;

			if (isMe)
			{
				player_0 = PlayerTool.getSteamPlayer(executorID);
			}
			else
			{
				PlayerTool.tryGetSteamPlayer(components[0], out player_0);
			}

			if (player_0 == null)
			{
				CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
				return;
			}

			SteamPlayer player_1;
			if (PlayerTool.tryGetSteamPlayer(components[isMe ? 0 : 1], out player_1))
			{
				if (player_1.player.movement.getVehicle() != null)
				{
					CommandWindow.LogError(localization.format("NoVehicleErrorText"));
					return;
				}
				else
				{
					bool teleported = player_0.player.teleportToPlayer(player_1.player);
					if (teleported)
						CommandWindow.Log(localization.format("TeleportText", player_0.playerID.playerName, player_1.playerID.playerName));
					else
						CommandWindow.LogError(localization.format("TeleportObstructed", player_0.playerID.playerName, player_1.playerID.playerName));
				}
			}
			else
			{
				if (components[isMe ? 0 : 1].Equals(localization.format("WaypointCommand"), System.StringComparison.InvariantCultureIgnoreCase) && player_0.player.quests.isMarkerPlaced)
				{
					Vector3 waypoint = player_0.player.quests.markerPosition;
					if (raycastFromSkyToPosition(ref waypoint))
					{
						bool teleported = player_0.player.teleportToLocation(waypoint, player_0.player.transform.rotation.eulerAngles.y);
						if (teleported)
							CommandWindow.Log(localization.format("TeleportText", player_0.playerID.playerName, localization.format("WaypointText")));
						else
							CommandWindow.LogError(localization.format("TeleportObstructed", player_0.playerID.playerName, localization.format("WaypointText")));
					}
				}
				else if (components[isMe ? 0 : 1].Equals(localization.format("BedCommand"), System.StringComparison.InvariantCultureIgnoreCase))
				{
					bool teleported = player_0.player.teleportToBed();
					if (teleported)
						CommandWindow.Log(localization.format("TeleportText", player_0.playerID.playerName, localization.format("BedText")));
					else
						CommandWindow.LogError(localization.format("TeleportObstructed", player_0.playerID.playerName, localization.format("BedText")));
				}
				else
				{
					LocationDevkitNode node = null;

					foreach (LocationDevkitNode testNode in LocationDevkitNodeSystem.Get().GetAllNodes())
					{
						if (NameTool.checkNames(components[isMe ? 0 : 1], testNode.locationName))
						{
							node = testNode;
							break;
						}
					}

					if (node != null)
					{
						Vector3 position = node.transform.position;
						raycastFromNearPosition(ref position);
						bool teleported = player_0.player.teleportToLocation(position, player_0.player.transform.rotation.eulerAngles.y);
						if (teleported)
							CommandWindow.Log(localization.format("TeleportText", player_0.playerID.playerName, node.name));
						else
							CommandWindow.LogError(localization.format("TeleportObstructed", player_0.playerID.playerName, node.name));
					}
					else
					{
						CommandWindow.LogError(localization.format("NoLocationErrorText", components[isMe ? 0 : 1]));
					}
				}
			}
		}

		public CommandTeleport(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("TeleportCommandText");
			_info = localization.format("TeleportInfoText");
			_help = localization.format("TeleportHelpText");
		}
	}
}
