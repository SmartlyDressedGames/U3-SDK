////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define ECON_TICKETS_BROKEN
using SDG.NetPak;
using Steamworks;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_Verify
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			Provider.isWaitingForConnectResponse = false;

			SteamNetworkingIdentity serverIdentity = new SteamNetworkingIdentity();
			serverIdentity.SetSteamID(Provider.server);

			byte[] ticket = Provider.openTicket(serverIdentity);
			if (ticket == null)
			{
				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_EMPTY;
				Provider.RequestDisconnect("opening Steam auth ticket failed");
				return;
			}

			UnturnedLog.info("Authenticating with server");

#if !DEDICATED_SERVER
			Provider.isWaitingForAuthenticationResponse = true;
			Provider.sentAuthenticationRequestTime = UnityEngine.Time.realtimeSinceStartupAsDouble;
#endif // !DEDICATED_SERVER

			NetMessages.SendMessageToServer(EServerMessage.Authenticate, NetTransport.ENetReliability.Reliable, (NetPakWriter writer) =>
			{
				writer.WriteUInt16((ushort) ticket.Length);
				writer.WriteBytes(ticket);
				WriteEconomyDetails(writer);
			});
		}

		private static void WriteEconomyDetails(NetPakWriter writer)
		{
#if ECON_TICKETS_BROKEN
			if (Provider.provider.economyService.wearingResult == SteamInventoryResult_t.Invalid)
			{
				writer.WriteUInt16(0); // 0 items.
				return;
			}

			// Use exiting wearingResult rather than making a separate system.
			SteamItemDetails_t[] tempDetails = null;

			uint tempSize = 0;
			if (SteamInventory.GetResultItems(Provider.provider.economyService.wearingResult, null, ref tempSize) && tempSize > 0)
			{
				tempDetails = new SteamItemDetails_t[tempSize];
				SteamInventory.GetResultItems(Provider.provider.economyService.wearingResult, tempDetails, ref tempSize);
			}
			else
			{
				tempDetails = new SteamItemDetails_t[tempSize];
			}

			writer.WriteUInt16((ushort) tempDetails.Length);
			for (uint itemIndex = 0; itemIndex < tempDetails.Length; ++itemIndex)
			{
				SteamItemDetails_t details = tempDetails[itemIndex];
				writer.WriteSteamItemDefID(details.m_iDefinition);
				writer.WriteSteamItemInstanceID(details.m_itemId);
				
				string tags;
				uint tagsSize = 1024;
				if (!SteamInventory.GetResultItemProperty(Provider.provider.economyService.wearingResult, itemIndex, "tags", out tags, ref tagsSize) || tagsSize == 0)
				{
					tags = string.Empty;
				}
				writer.WriteString(tags);

				string dynamicProps;
				uint dynamicPropsSize = 1024;
				if (!SteamInventory.GetResultItemProperty(Provider.provider.economyService.wearingResult, itemIndex, "dynamic_props", out dynamicProps, ref dynamicPropsSize) || dynamicPropsSize == 0)
				{
					dynamicProps = string.Empty;
				}
				writer.WriteString(dynamicProps);
			}

			// Destroy result after we've copied out all the tags/dynamic_props info.
			SteamInventory.DestroyResult(Provider.provider.economyService.wearingResult);
			Provider.provider.economyService.wearingResult = SteamInventoryResult_t.Invalid;
#else
			if (Provider.provider.economyService.wearingResult == SteamInventoryResult_t.Invalid)
			{
				writer.WriteUInt16(0); // 0 buffer length
				return;
			}

			uint bufferLength;
			bool serializeGotLength = SteamInventory.SerializeResult(Provider.provider.economyService.wearingResult, null, out bufferLength);
			if (serializeGotLength && bufferLength <= ushort.MaxValue)
			{
				byte[] buffer = new byte[bufferLength];
				bool serialized = SteamInventory.SerializeResult(Provider.provider.economyService.wearingResult, buffer, out bufferLength);
				if (!serialized)
				{
					UnturnedLog.warn("SteamInventory.SerializeResult returned false the second time");
				}

				writer.WriteUInt16((ushort) bufferLength);
				writer.WriteBytes(buffer);

				SteamInventory.DestroyResult(Provider.provider.economyService.wearingResult);
				Provider.provider.economyService.wearingResult = SteamInventoryResult_t.Invalid;
			}
			else
			{
				SteamInventory.DestroyResult(Provider.provider.economyService.wearingResult);
				Provider.provider.economyService.wearingResult = SteamInventoryResult_t.Invalid;

				Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ECON_SERIALIZE;
				Provider.RequestDisconnect(serializeGotLength ? "SteamInventory.SerializeResult length too large!" : "SteamInventory.SerializeResult failed");
				return;
			}
#endif
		}
	}
}
