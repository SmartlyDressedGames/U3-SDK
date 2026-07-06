////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define LOG_CLIENT_ASSET_INTEGRITY

using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows file name to be included in kick message that client would otherwise not know.
	/// </summary>
	internal static class ServerMessageHandler_ValidateAssets
	{
		internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
		{
			SteamPlayer player = Provider.findPlayer(transportConnection);
			if (player == null)
			{
				if (NetMessages.shouldLogBadMessages)
				{
					UnturnedLog.info($"Ignoring ValidateAssets message from {transportConnection} because there is no associated player");
				}
				Provider.IncrementBadPacketsFromConnection(transportConnection);
				return;
			}

			uint itemCountBits;
			if (!reader.ReadBits(MAX_ASSETS.bitCount, out itemCountBits))
			{
				Provider.kick(player.playerID.steamID, $"ValidateAssets unable to read {nameof(itemCountBits)}");
				return;
			}

			int itemCount = (int) itemCountBits;
			if (itemCount > MAX_ASSETS.value)
			{
				Provider.kick(player.playerID.steamID, $"ValidateAssets invalid {nameof(itemCount)}");
				return;
			}
			++itemCount; // Squeeze in an extra item because zero is never sent.

			uint hasHashFlags;
			if (!reader.ReadBits(itemCount, out hasHashFlags))
			{
				Provider.kick(player.playerID.steamID, $"ValidateAssets unable to read {nameof(hasHashFlags)}");
				return;
			}

			for (int index = 0; index < itemCount; ++index)
			{
				System.Guid guid;
				if (!reader.ReadGuid(out guid))
				{
					Provider.kick(player.playerID.steamID, $"ValidateAssets unable to read {nameof(guid)}");
					return;
				}

				if (guid == System.Guid.Empty)
				{
					// Client has a check to prevent sending empty guids.
					Provider.kick(player.playerID.steamID, $"ValidateAssets empty {nameof(guid)}");
					return;
				}

				bool wasAdded = player.validatedGuids.Add(guid);
				if (!wasAdded)
				{
					// Client has a check to prevent sending the same guid twice.
					Provider.kick(player.playerID.steamID, $"ValidateAssets duplicate {nameof(guid)}");
					return;
				}

				bool clientHasHash = (hasHashFlags & (1U << index)) > 0;
				if (clientHasHash)
				{
					if (!reader.ReadBytes(clientHash))
					{
						Provider.kick(player.playerID.steamID, $"ValidateAssets unable to read {nameof(clientHash)}");
						return;
					}
				}

				if (ClientAssetIntegrity.serverKnownMissingGuids.Contains(guid))
				{
					// Server knows this guid is valid (e.g. referenced in level) but is also missing the asset,
					// so do not kick the client for missing this asset.
					continue;
				}

				Asset asset = Assets.find(guid);
				if (asset == null)
				{
					// Server will be missing assets if -SkipAssets is set, so don't kick. (public issue #4210)
					if (Assets.shouldLoadAnyAssets)
					{
						// Server does not have the asset. Kicking is a trade-off because maybe savedata was loaded after a mod
						// was removed, or maybe plugin specified asset which is missing from the server assets, but we need
						// to kick for this to prevent hacked clients from spamming the server with invalid guids. Systems
						// which send guid without loading on the server first should NOT send validate asset requests to
						// help reduce invalid kicks.
						//
						// We call dismiss rather than kick because KickForInvalidGuid formats using the clientside asset names.
						UnturnedLog.info($"Kicking {transportConnection} for invalid file integrity request guid: {guid:N}");
						Assets.SendKickForInvalidGuid.Invoke(ENetReliability.Reliable, transportConnection, guid);
						Provider.dismiss(player.playerID.steamID);
					}
					return;
				}

				if (!asset.ShouldVerifyHash)
				{
					// Hash is always sent regardless of shouldVerifyHash on client otherwise client would just disable it.
					// Some hosts want to disable hash verification so that they can edit storage size and armor on server.
					// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2891
#if LOG_CLIENT_ASSET_INTEGRITY
					CommandWindow.Log($"Ignored \"{asset.FriendlyName}\" ({guid.ToString("N")}) client asset integrity request because it was disabled");
#endif // LOG_CLIENT_ASSET_INTEGRITY
					continue;
				}

				if (clientHasHash)
				{
					byte[] serverHash = asset.hash;
					if (asset.originMasterBundle != null && asset.originMasterBundle.serverHashes != null)
					{
						byte[] platformHash = asset.originMasterBundle.serverHashes.GetPlatformHash(player.clientPlatform);
						if (platformHash != null)
						{
							// Combine .dat/.asset hash with Unity bundle hash to prevent cheaters from replacing bundle.
							serverHash = Hash.combine(serverHash, platformHash);
						}
					}

					if (!Hash.verifyHash(clientHash, serverHash))
					{
						string serverAssetOrigin = asset.origin?.name;
						if (string.IsNullOrEmpty(serverAssetOrigin))
						{
							serverAssetOrigin = "Unknown";
						}

						// We call dismiss rather than kick because KickForHashMismatch formats using the clientside asset names.
						UnturnedLog.info($"Kicking {transportConnection} for asset hash mismatch: \"{asset.FriendlyName}\" Type: {asset.GetTypeFriendlyName()} File: \"{asset.name}\" Id: {guid:N} Client: {Hash.toString(clientHash)} Server: {Hash.toString(serverHash)}");
						Assets.SendKickForHashMismatch.Invoke(ENetReliability.Reliable, transportConnection, guid, asset.name, asset.FriendlyName, serverHash, asset.originMasterBundle?.assetBundleNameWithoutExtension, serverAssetOrigin);
						Provider.dismiss(player.playerID.steamID);
						return;
					}
				}
				else if (asset.hash != null && asset.hash.Length == 20)
				{
					// Client does not have asset but server does.
					Provider.kick(player.playerID.steamID, $"missing asset: \"{asset.FriendlyName}\" File: \"{asset.name}\" Id: {guid:N}");
					return;
				}

#if LOG_CLIENT_ASSET_INTEGRITY
				CommandWindow.Log($"Validated \"{asset.FriendlyName}\" \"{asset.name}\" ({guid.ToString("N")}) client asset integrity");
#endif // LOG_CLIENT_ASSET_INTEGRITY
			}
		}

		/// <summary>
		/// Actual max value is plus one because message never contains zero items.
		/// </summary>
		internal static readonly NetLength MAX_ASSETS = new NetLength(7);

		private static byte[] clientHash = new byte[20];
	}
}
