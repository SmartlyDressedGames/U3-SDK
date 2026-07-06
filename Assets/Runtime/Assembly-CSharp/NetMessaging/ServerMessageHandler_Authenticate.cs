////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define ECON_TICKETS_BROKEN
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_Authenticate
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			SteamPending player = Provider.findPendingPlayer(transportConnection);
			// 2022-04-14: whoops, this was not checking hasSentVerifyPacket, allowing queue bypass!
			if (player == null || !player.hasSentVerifyPacket)
			{
				Provider.reject(transportConnection, ESteamRejection.NOT_PENDING);
				return;
			}

			// Double-check server is not full in case plugin accepted multiple clients at once. (sigh)
			if (Provider.clients.Count + 1 > Provider.maxPlayers)
			{
				Provider.reject(transportConnection, ESteamRejection.SERVER_FULL);
				return;
			}

			UnturnedLog.info($"Received authentication request from queued player {player.playerID}");

			ushort ticketLength;
			reader.ReadUInt16(out ticketLength);
			byte[] ticket = new byte[ticketLength];
			reader.ReadBytes(ticket);

			if (Dedicator.offlineOnly)
			{
				// Do not use ticket.

				player.assignedPro = player.isPro; // Trust their reported gold status.
				player.assignedAdmin = SteamAdminlist.checkAdmin(player.playerID.steamID);

				player.hasAuthentication = true;

				UnturnedLog.info($"Skipping Steam authentication for queued player {player.playerID} because we are running offline-only");
			}
			else
			{
				if (!Provider.verifyTicket(player.playerID.steamID, ticket))
				{
					Provider.reject(transportConnection, ESteamRejection.AUTH_VERIFICATION);
					return;
				}

				UnturnedLog.info($"Submitted Steam authentication request for queued player {player.playerID}");
			}

			if (!ReadEconomyDetails(player, reader))
			{
				// Function returns false if it rejected the player.
				return;
			}

			// Nelson 2025-05-13: check IP address rule relatively late in this process in case there's much overhead
			// to requesting all connected clients' IP addresses from server transport.
			if (Provider.IsBlockedByMaxClientsWithSameIpAddressRule(transportConnection, includeQueuedPlayers: false))
			{
				Provider.reject(transportConnection, ESteamRejection.TOO_MANY_CLIENTS_WITH_SAME_IP_ADDRESS);
				return;
			}

			if (player.playerID.group == CSteamID.Nil || Dedicator.offlineOnly)
			{
				player.hasGroup = true;
			}
			else
			{
				if (!SteamGameServer.RequestUserGroupStatus(player.playerID.steamID, player.playerID.group))
				{
					player.playerID.group = CSteamID.Nil;
					player.hasGroup = true;
				}
				else
				{
					UnturnedLog.info($"Submitted Steam group request for queued player {player.playerID}");
				}
			}

			if (player.canAcceptYet)
			{
				// This case only happens in offline mode.
				Provider.accept(player);
			}
		}

		private static bool ReadEconomyDetails(SteamPending player, NetPakReader reader)
		{
#if ECON_TICKETS_BROKEN
			ushort detailsLength;
			reader.ReadUInt16(out detailsLength);
			SteamItemDetails_t[] tempDetails = new SteamItemDetails_t[detailsLength];
			Dictionary<ulong, DynamicEconDetails> tempDynamicDetails = new Dictionary<ulong, DynamicEconDetails>();

			for (uint itemIndex = 0; itemIndex < detailsLength; ++itemIndex)
			{
				SteamItemDetails_t details = new SteamItemDetails_t();
				bool success = reader.ReadSteamItemDefID(out details.m_iDefinition);
				success &= reader.ReadSteamItemInstanceID(out details.m_itemId);
				
				DynamicEconDetails dynamicDetails = new DynamicEconDetails();
				success &= reader.ReadString(out dynamicDetails.tags);
				success &= reader.ReadString(out dynamicDetails.dynamic_props);

				success &= !tempDynamicDetails.ContainsKey(details.m_itemId.m_SteamItemInstanceID);

				if(!success)
				{
					Provider.reject(player.playerID.steamID, ESteamRejection.AUTH_ECON_DESERIALIZE);
					return false;
				}

				tempDetails[itemIndex] = details;
				tempDynamicDetails.Add(details.m_itemId.m_SteamItemInstanceID, dynamicDetails);
			}

			player.inventoryResult = SteamInventoryResult_t.Invalid;
			player.inventoryDetails = tempDetails;
			player.dynamicInventoryDetails = tempDynamicDetails;
			player.inventoryDetailsReady();
#else
			ushort bufferLength;
			reader.ReadUInt16(out bufferLength);
			if (bufferLength > 0)
			{
				byte[] resultBuffer = new byte[bufferLength];
				reader.ReadBytes(resultBuffer);

				if (!SteamGameServerInventory.DeserializeResult(out player.inventoryResult, resultBuffer, bufferLength, false))
				{
					Provider.reject(player.transportConnection, ESteamRejection.AUTH_ECON_DESERIALIZE);
					return false;
				}
			}
			else
			{
				player.shirtItem = 0;
				player.pantsItem = 0;
				player.hatItem = 0;
				player.backpackItem = 0;
				player.vestItem = 0;
				player.maskItem = 0;
				player.glassesItem = 0;
				player.skinItems = new int[0];
				player.skinTags = new string[0];
				player.skinDynamicProps = new string[0];

				player.packageShirt = 0;
				player.packagePants = 0;
				player.packageHat = 0;
				player.packageBackpack = 0;
				player.packageVest = 0;
				player.packageMask = 0;
				player.packageGlasses = 0;
				player.packageSkins = new ulong[0];

				player.inventoryResult = SteamInventoryResult_t.Invalid;
				player.inventoryDetails = new SteamItemDetails_t[0];
				player.hasProof = true;
			}
#endif
			return true;
		}
	}
}
