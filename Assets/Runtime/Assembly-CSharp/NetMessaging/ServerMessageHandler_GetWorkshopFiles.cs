////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class ServerMessageHandler_GetWorkshopFiles
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			// Since this message happens before full connection is established we require sender
			// to spend ~1KB upload bandwidth to hopefully help prevent spam.
			byte[] emptyBuffer;
			int emptyBufferOffset;
			if (!reader.ReadBytesPtr(960, out emptyBuffer, out emptyBufferOffset))
			{
				Provider.refuseGarbageConnection(transportConnection, "missing empty buffer");
				return;
			}

			string headerString;
			if (!reader.ReadString(out headerString))
			{
				Provider.refuseGarbageConnection(transportConnection, "failed to read header string");
				return;
			}

			if (!string.Equals(headerString, "Hello!", System.StringComparison.Ordinal))
			{
				Provider.refuseGarbageConnection(transportConnection, "invalid header string");
				return;
			}

			if (rateLimiter.IsBlocked(transportConnection))
			{
				if (NetMessages.shouldLogBadMessages)
				{
					UnturnedLog.info($"Ignoring GetWorkshopFiles message from {transportConnection} because they requested recently");
				}
				Provider.IncrementBadPacketsFromConnection(transportConnection);
				return;
			}

			NetMessages.SendMessageToClient(EClientMessage.DownloadWorkshopFiles, ENetReliability.Reliable, transportConnection, (NetPakWriter writer) =>
			{
				// Client needs this information before loading the level.
				writer.WriteEnum(Provider.authorityHoliday);

				writer.WriteList(Provider.serverRequiredWorkshopFiles, WriteServerRequiredWorkshopFile, MAX_FILES);

				writer.WriteString(Provider.serverName);
				writer.WriteString(Provider.map);
				writer.WriteBit(Provider.isPvP);
				writer.WriteBit(Provider.hasCheats);
				writer.WriteBit(Provider.isVacActive);
				writer.WriteBit(Provider.isThirdpartyAntiCheatActive);
				writer.WriteBit(Provider.isGold);
				writer.WriteEnum(Provider.mode);
				writer.WriteEnum(Provider.cameraMode);
				writer.WriteUInt8(Provider.maxPlayers);

				// Nelson 2024-04-26: We now send these additional details so that the client can create a bookmark.
				writer.WriteString(Provider.configData.Browser.BookmarkHost);
				writer.WriteString(Provider.configData.Browser.Thumbnail);
				writer.WriteString(Provider.configData.Browser.Desc_Server_List);
			});
		}

		private static bool WriteServerRequiredWorkshopFile(NetPakWriter writer, Provider.ServerRequiredWorkshopFile item)
		{
			return writer.WriteUInt64(item.fileId) && writer.WriteDateTime(item.timestamp);
		}

		private static readonly NetLength MAX_FILES = new NetLength(255);

		/// <summary>
		/// Nelson 2025-05-13: replacing the "workshop request log" which used transport connection hash code with this
		/// more recent IP address and Steam ID rate limiter.
		/// </summary>
		private static TransportConnectionRateLimiter rateLimiter = new TransportConnectionRateLimiter()
		{
			window = 30.0f,
			threshold = 1,
		};
	}
}
