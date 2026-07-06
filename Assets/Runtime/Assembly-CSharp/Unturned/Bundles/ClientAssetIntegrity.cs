////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define LOG_CLIENT_ASSET_INTEGRITY

using SDG.NetPak;
using SDG.NetTransport;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class ClientAssetIntegrity
	{
		/// <summary>
		/// Reset prior to joining a new server.
		/// </summary>
		public static void Clear()
		{
			timer = 0.0f;
			validatedGuids.Clear();
			serverKnownMissingGuids.Clear();
			pendingValidation.Clear();

#if LOG_CLIENT_ASSET_INTEGRITY
			UnturnedLog.info("Cleared client asset integrity");
#endif // LOG_CLIENT_ASSET_INTEGRITY
		}

		/// <summary>
		/// By default if the client submits an asset guid which the server cannot find an asset for the client will
		/// be kicked. This is necessary to prevent cheaters from spamming huge numbers of random guids. In certain cases
		/// like a terrain material missing the server knows the client will be missing it as well, and can register
		/// it here to prevent the client from being kicked unnecessarily.
		/// </summary>
		public static void ServerAddKnownMissingAsset(System.Guid guid, string context)
		{
			if (guid != System.Guid.Empty)
			{
				bool wasAdded = serverKnownMissingGuids.Add(guid);
				if (wasAdded)
				{
					UnturnedLog.info($"Context \"{context}\" known missing asset {guid:N}, server will not kick clients for this");
				}
			}
		}

		/// <summary>
		/// Send asset hash (or lack thereof) to server.
		/// 
		/// IMPORTANT: should only be called in cases where the server has verified the asset exists by loading it,
		/// otherwise only if the asset exists on the client. This is because the server kicks if the asset does not
		/// exist in order to prevent hacked clients from spamming requests. Context parameter is intended to help
		/// narrow down cases where this rule is being broken.
		/// </summary>
		public static void QueueRequest(System.Guid guid, Asset asset, string context)
		{
			if (guid == System.Guid.Empty)
				return;

			bool wasAdded = validatedGuids.Add(guid);
			if (!wasAdded)
			{
				// Already validated with server, or at least queued for validation.
#if LOG_CLIENT_ASSET_INTEGRITY
				if (asset != null)
				{
					UnturnedLog.info($"Ignored repeated request to queue \"{asset.FriendlyName}\" ({guid.ToString("N")}) for client asset integrity");
				}
				else
				{
					UnturnedLog.info($"Ignored repeated request to queue {guid.ToString("N")} (missing asset) for client asset integrity");
				}
#endif // LOG_CLIENT_ASSET_INTEGRITY
				return;
			}

			if (asset == null)
			{
				// We should get kicked for this missing asset, so log the context to help track down cases where the
				// server sent an asset guid without validating it exists. (our code should already be verified as
				// not doing this, but plugins might be doing something manually)
				UnturnedLog.warn($"Context \"{context}\" missing asset {guid:N}");
			}

			pendingValidation.Add(new KeyValuePair<System.Guid, Asset>(guid, asset));
#if LOG_CLIENT_ASSET_INTEGRITY
			if (asset != null)
			{
				UnturnedLog.info($"Queued \"{asset.FriendlyName}\" ({guid.ToString("N")}) client asset integrity request (context \"{context}\")");
			}
			else
			{
				UnturnedLog.info($"Queued {guid.ToString("N")} (missing asset) client asset integrity request (context \"{context}\")");
			}
#endif // LOG_CLIENT_ASSET_INTEGRITY
		}

		/// <summary>
		/// Send asset hash to server.
		/// Used in cases where server does not verify asset exists. (see other method's comment)
		/// </summary>
		public static void QueueRequest(Asset asset)
		{
			if (asset != null)
			{
				QueueRequest(asset.GUID, asset, null);
			}
		}

		/// <summary>
		/// Called each Update on the client.
		/// </summary>
		public static void SendRequests()
		{
			if (pendingValidation.Count < 1)
				return;

			timer += Time.unscaledDeltaTime;
			if (timer > 0.1f) // 10 requests per second.
			{
				timer = 0.0f;
			}
			else
			{
				return;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (Player.LocalPlayer?.channel?.owner?.playerID?.BypassIntegrityChecks ?? false)
			{
				pendingValidation.Clear();
				return;
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			NetMessages.SendMessageToServer(EServerMessage.ValidateAssets, ENetReliability.Reliable, (NetPakWriter writer) =>
			{
				int pendingCount = pendingValidation.Count;

				// Squeeze in an extra item because zero is never sent.
				int actualMaxCount = ((int) ServerMessageHandler_ValidateAssets.MAX_ASSETS.value) + 1;

				int removalCount = Mathf.Min(pendingCount, actualMaxCount);
				int removalIndex = pendingCount - removalCount;

				uint itemCountBits = (uint) (removalCount - 1);
				writer.WriteBits(itemCountBits, ServerMessageHandler_ValidateAssets.MAX_ASSETS.bitCount);

				uint hasHashFlags = 0;
				int flagIndex = 0;
				for (int itemIndex = pendingCount - 1; itemIndex >= removalIndex; --itemIndex, ++flagIndex)
				{
					KeyValuePair<System.Guid, Asset> item = pendingValidation[itemIndex];
					if (item.Value != null && item.Value.hash != null && item.Value.hash.Length == 20)
					{
						hasHashFlags |= 1U << flagIndex;
					}
				}

				writer.WriteBits(hasHashFlags, removalCount);

				flagIndex = 0;
				for (int itemIndex = pendingCount - 1; itemIndex >= removalIndex; --itemIndex, ++flagIndex)
				{
					KeyValuePair<System.Guid, Asset> item = pendingValidation[itemIndex];
					System.Guid guid = item.Key;
					writer.WriteGuid(guid);
					if ((hasHashFlags & (1U << flagIndex)) > 0)
					{
						Asset asset = item.Value;
						if (asset.originMasterBundle != null && asset.originMasterBundle.doesHashFileExist && asset.originMasterBundle.hash != null && asset.originMasterBundle.hash.Length == 20)
						{
							// Combine .dat/.asset hash with Unity bundle hash to prevent cheaters from replacing bundle.
							byte[] combinedHash = Hash.combine(asset.hash, asset.originMasterBundle.hash);
							writer.WriteBytes(combinedHash);
#if LOG_CLIENT_ASSET_INTEGRITY
							UnturnedLog.info($"\"{asset.FriendlyName}\" ({guid.ToString("N")}) data+MB hash: {Hash.toString(combinedHash)}");
#endif // LOG_CLIENT_ASSET_INTEGRITY
						}
						else
						{
							writer.WriteBytes(asset.hash);
#if LOG_CLIENT_ASSET_INTEGRITY
							UnturnedLog.info($"\"{asset.FriendlyName}\" ({guid.ToString("N")}) data hash: {Hash.toString(asset.hash)}");
#endif // LOG_CLIENT_ASSET_INTEGRITY
						}
					}
				}

				pendingValidation.RemoveRange(removalIndex, removalCount);
#if LOG_CLIENT_ASSET_INTEGRITY
				UnturnedLog.info($"Sent {removalCount} of {pendingCount} pending client asset integrity request(s)");
#endif // LOG_CLIENT_ASSET_INTEGRITY
			});
		}

		private static float timer;
		private static HashSet<System.Guid> validatedGuids = new HashSet<System.Guid>();
		internal static HashSet<System.Guid> serverKnownMissingGuids = new HashSet<System.Guid>();
		private static List<KeyValuePair<System.Guid, Asset>> pendingValidation = new List<KeyValuePair<System.Guid, Asset>>();
	}
}
