////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#define VEHICLES

using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerManager : SteamCaller
	{
		internal const float MAX_VISIBLE_DISTANCE = 576;
		internal const float SQR_MAX_VISIBLE_DISTANCE = MAX_VISIBLE_DISTANCE * MAX_VISIBLE_DISTANCE;

		/// <summary>
		/// Position to place players outside visible range.
		/// Defaults to as far away as supported by default clamped Vector3 precision.
		/// Doesn't use world origin because that would potentially increase rendering cost for clients near the origin.
		/// </summary>
		public static Vector3 CulledPosition
		{
			get;
			set;
		} = new Vector3(-4095, -4095, -4095);

		[System.Obsolete]
		public static ushort updates;
		private static float lastTick;
		private static uint seq;

		[System.Obsolete]
		public void tellPlayerStates(CSteamID steamID)
		{ }

		private static double lastReceivePlayerStates;

		/// <summary>
		/// Whether local client is currently penalized for potentially using a lag switch. Server has an equivalent check which reduces
		/// damage dealt, whereas the clientside check stops shooting in order to prevent abuse of inbound-only lagswitches. For example,
		/// if a cheater freezes enemy positions by dropping inbound traffic while still sending movement and shooting outbound traffic.
		/// </summary>
		internal static bool IsClientUnderFakeLagPenalty
		{
			get
			{
				bool disableTimer = false;

				// Disable timer in singleplayer.
				disableTimer |= Provider.isServer;

				// Lag switching is not an issue in PvE mode.
				disableTimer |= !Provider.isPvP;

				// We are the only player on the server, so nobody is affected by lag switching.
				disableTimer |= Provider.clients.Count < 2;

				return Time.realtimeSinceStartupAsDouble - lastReceivePlayerStates > 2.0f && !disableTimer;
			}
		}

		/// <summary>
		/// Will test player be culled for viewer at a given position?
		///
		/// Members of the same group are always visible to each other. (Used by map and HUD name overlay.)
		///
		/// Admins with the Spectator Overlay enabled are able to see all clients.
		/// Similarly, plugins can set ServerAllowKnowledgeOfAllClientPositions to show all clients.
		///
		/// Players in vehicles:
		/// VehicleManager notifies all clients when a player enters a vehicle, so a client may know the player's
		/// position even if this method suggests otherwise. When exiting the vehicle, CulledPosition is sent
		/// instead of the real exit position to clients who should cull the new position.
		/// </summary>
		public static bool IsPlayerCulledAtPosition(SteamPlayer testPlayer, Vector3 testPosition, SteamPlayer viewer, Vector3 viewerPosition)
		{
			if (testPlayer.isMemberOfSameGroupAs(viewer))
			{
				return false;
			}

			if (viewer.player.AdminUsageFlags.HasFlag(EPlayerAdminUsageFlags.SpectatorStatsOverlay))
			{
				return false;
			}

			if (viewer.player.ServerAllowKnowledgeOfAllClientPositions)
			{
				return false;
			}

			return (viewerPosition - testPosition).GetHorizontalSqrMagnitude() > SQR_MAX_VISIBLE_DISTANCE;
		}

		private static readonly ClientStaticMethod SendPlayerStates = ClientStaticMethod.Get(ReceivePlayerStates);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceivePlayerStates(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			uint newSeq;
			reader.ReadUInt32(out newSeq);
			if (newSeq <= seq)
				return;
			seq = newSeq;

			lastReceivePlayerStates = Time.realtimeSinceStartupAsDouble;

			ushort count;
			reader.ReadUInt16(out count);
			if (count < 1)
			{
				// Receiving zero happens to update lastReceivePlayerStates.
				return;
			}

			for (ushort index = 0; index < count; ++index)
			{
				byte id;
				reader.ReadUInt8(out id);
				Vector3 position;
				reader.ReadClampedVector3(out position);
				byte pitch;
				reader.ReadUInt8(out pitch);
				byte yaw;
				reader.ReadUInt8(out yaw);

				SteamPlayer updated = PlayerTool.findSteamPlayerByChannel(id);
				if (updated == null || updated.player == null || updated.player.movement == null)
					continue;

				updated.player.movement.tellState(position, pitch, yaw);
			}

#if WITH_NSB_LOGGING
			receivedPlayerUpdate = Time.realtimeSinceStartup;
#endif // WITH_NSB_LOGGING
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				seq = 0;

#if WITH_NSB_LOGGING
				receivedPlayerUpdate = Time.realtimeSinceStartup;
#endif // WITH_NSB_LOGGING
			}
		}

		private List<SteamPlayer> playersToSend = new List<SteamPlayer>();
		private float lastSendOverflowWarning;

		private void sendPlayerStates()
		{
			seq++;

			for (int clientIndex = 0; clientIndex < Provider.clients.Count; clientIndex++)
			{
				SteamPlayer client = Provider.clients[clientIndex];

				if (client == null || client.player == null)
				{
					continue;
				}

				Vector3 recipientPosition = client.model.transform.position;

				ushort updateCount = 0;
				playersToSend.Clear();

				for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
				{
					if (playerIndex == clientIndex)
					{
						// Do not send info about self.
						continue;
					}

					SteamPlayer player = Provider.clients[playerIndex];

					if (player == null || player.player == null || player.player.movement == null || player.player.movement.updates == null)
					{
						continue;
					}

					// Should be identical to IsPlayerCulledAtPosition.
					// Please refer to that method's comment for explanation (e.g., culling in vehicles).
					bool newIsCulled;
					if (!player.isMemberOfSameGroupAs(client)
						&& !client.player.AdminUsageFlags.HasFlag(EPlayerAdminUsageFlags.SpectatorStatsOverlay)
						&& !client.player.ServerAllowKnowledgeOfAllClientPositions)
					{
						Vector3 sendPosition;
						if (!player.player.movement.hasMostRecentlyAddedUpdate)
						{
							sendPosition = player.model.transform.position;
						}
						else
						{
							sendPosition = player.player.movement.mostRecentlyAddedUpdate.pos;
						}

						newIsCulled = (recipientPosition - sendPosition).GetHorizontalSqrMagnitude() > SQR_MAX_VISIBLE_DISTANCE;
					}
					else
					{
						newIsCulled = false;
					}

					bool wasCulled = client.culledPlayers.Contains(player.playerID.steamID);
					bool isCulledChanged = newIsCulled != wasCulled;
					if (isCulledChanged)
					{
						if (newIsCulled)
						{
							client.culledPlayers.Add(player.playerID.steamID);
						}
						else
						{
							client.culledPlayers.Remove(player.playerID.steamID);
						}
					}

					if (isCulledChanged || (!newIsCulled && player.player.movement.updates.Count > 0))
					{
						playersToSend.Add(player);
						updateCount += (ushort) Mathf.Max(player.player.movement.updates.Count, 1);
					}
				}

				// Sending zero updates is valid because client uses ReceivePlayerStates to test for inbound lag switches.
				SendPlayerStates.Invoke(ENetReliability.Unreliable, client.transportConnection, SendPlayerStates_Write, updateCount, client);

#if WITH_NSB_LOGGING
				client.sentPlayerUpdate = Time.realtimeSinceStartup;
				sentAnyPlayerUpdate = Time.realtimeSinceStartup;
#endif // WITH_NSB_LOGGING
			}

			for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
			{
				SteamPlayer player = Provider.clients[playerIndex];

				if (player == null || player.player == null || player.player.movement == null || player.player.movement.updates == null || player.player.movement.updates.Count == 0)
				{
					continue;
				}

				player.player.movement.updates.Clear();
			}

#if WITH_NSB_LOGGING
			float timeSinceAnyUpdate = Time.realtimeSinceStartup - sentAnyPlayerUpdate;
			if(Provider.clients.Count > 1 && NsbLog.isEnabledOnServer && timeSinceAnyUpdate < 10) // > 1 because self is skipped
			{
				foreach(SteamPlayer client in Provider.clients)
				{
					if(client == null)
						continue;

					float timeSinceUpdate = Time.realtimeSinceStartup - client.sentPlayerUpdate;
					if(timeSinceUpdate > 10)
					{
						client.sentPlayerUpdate = Time.realtimeSinceStartup; // Prevent warning spam.
						NsbLog.WarningFormat("{0}s since we sent tellPlayerStates to {1}", timeSinceUpdate, client.playerID);
					}
				}
			}
#endif // WITH_NSB_LOGGING
		}

		private void SendPlayerStates_Write(NetPakWriter writer, ushort updateCount, SteamPlayer forClient)
		{
			Vector3 recipientPosition = forClient.model.transform.position;

			writer.WriteUInt32(seq);
			writer.WriteUInt16(updateCount);
			foreach (SteamPlayer player in playersToSend)
			{
				if (forClient.culledPlayers.Contains(player.playerID.steamID))
				{
					// Player has just come culled.

					writer.WriteUInt8((byte) player.channel);
					writer.WriteClampedVector3(CulledPosition);
					writer.WriteUInt8(90); // Pitch (Forward)
					writer.WriteUInt8(0); // Yaw
					continue;
				}

				if (player.player.movement.updates.Count < 1)
				{
					// Player is no longer culled.

					writer.WriteUInt8((byte) player.channel);
					if (player.player.movement.hasMostRecentlyAddedUpdate)
					{
						PlayerStateUpdate state = player.player.movement.mostRecentlyAddedUpdate;
						writer.WriteClampedVector3(state.pos);
						writer.WriteUInt8(state.angle);
						writer.WriteUInt8(state.rot);
					}
					else
					{
						writer.WriteClampedVector3(player.model.transform.position);
						writer.WriteUInt8(90); // Pitch (Forward)
						writer.WriteUInt8(MeasurementTool.angleToByte(player.model.transform.rotation.eulerAngles.y)); // Yaw

					}
				}
				else
				{
					for (int updateIndex = 0; updateIndex < player.player.movement.updates.Count; updateIndex++) // high priority
					{
						PlayerStateUpdate state = player.player.movement.updates[updateIndex];
						writer.WriteUInt8((byte) player.channel);
						writer.WriteClampedVector3(state.pos);
						writer.WriteUInt8(state.angle); // Pitch
						writer.WriteUInt8(state.rot); // Yaw
					}
				}
			}

			if (writer.errors != NetPakWriter.EErrorFlags.None && Time.realtimeSinceStartup - lastSendOverflowWarning > 1.0f)
			{
				lastSendOverflowWarning = Time.realtimeSinceStartup;
				CommandWindow.LogWarningFormat("Error {0} writing player states. The player count is {1}.", writer.errors, Provider.clients.Count);
			}
		}

#if WITH_NSB_LOGGING
		private float sentAnyPlayerUpdate;
		float receivedPlayerUpdate;
		void clientNsbTest()
		{
			// It is reasonable that we might not receive vehicle, animal or zombie snapshots for a while,
			// but players are always moving so this should be a sign for suspicion.

			float timeSinceUpdate = Time.realtimeSinceStartup - receivedPlayerUpdate;
			if(timeSinceUpdate > 10)
			{
				receivedPlayerUpdate = Time.realtimeSinceStartup; // Prevent warning spam.
				NsbLog.WarningFormat("{0}s since we received tellPlayerStates", timeSinceUpdate);
			}
		}
#endif // WITH_NSB_LOGGING 

		private void Update()
		{
#if WITH_NSB_LOGGING
			if(Provider.isServer == false && Provider.clients != null && Provider.clients.Count > 1)
			{
				clientNsbTest();
			}
#endif // WITH_NSB_LOGGING

			if (!Provider.isServer || !Level.isLoaded)
			{
				return;
			}

			if (Dedicator.IsDedicatedServer && Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
			{
				lastTick += Provider.UPDATE_TIME;
				if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
				{
					lastTick = Time.realtimeSinceStartup;
				}

				sendPlayerStates();
			}
		}

		private void Start()
		{
			Level.onLevelLoaded += onLevelLoaded;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Players: {Provider.clients?.Count}");
		}
	}
}
