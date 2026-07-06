////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	[Obsolete("NPCEventHandler provides the instigating player.")]
	public delegate void NPCEventTriggeredHandler(string id);

	public delegate void NPCEventHandler(Player instigatingPlayer, string eventId);

	public enum ENPCEventReplicationMode
	{
		/// <summary>
		/// Do not replicate to clients. Run the event on the listen server (singleplayer) / dedicated server.
		/// Equivalent to the `shouldReplicate = false` parameter.
		/// Default.
		/// </summary>
		AuthorityOnly,

		/// <summary>
		/// Replicate to clients. Run the event everywhere.
		/// Replaces the `shouldReplicate = true` parameter.
		/// </summary>
		AuthorityAndClients,

		/// <summary>
		/// Only runs the event for the instigating player.
		/// </summary>
		InstigatorOnly,
	}

	/// <summary>
	/// Allows NPCs to trigger plugin or script events.
	/// </summary>
	public class NPCEventManager
	{
		[Obsolete("onEvent provides the instigating player.")]
#pragma warning disable
		public static event NPCEventTriggeredHandler eventTriggered;
#pragma warning restore

		/// <summary>
		/// instigatingPlayer can be null. For example, if instigated by NpcGlobalEventMessenger.
		/// </summary>
		public static event NPCEventHandler onEvent;

		[Obsolete("broadcastEvent provides the instigating player.")]
		public static void triggerEventTriggered(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return;
			}

#pragma warning disable
			var handler = eventTriggered;
			if (handler != null)
			{
				handler(id);
			}
#pragma warning restore
		}

		public static void broadcastEvent(Player instigatingPlayer, string eventId)
		{
			BroadcastEvent(instigatingPlayer, eventId, ENPCEventReplicationMode.AuthorityOnly);
		}

		public static void broadcastEvent(Player instigatingPlayer, string eventId, bool shouldReplicate = false)
		{
			ENPCEventReplicationMode replicationMode = shouldReplicate ? ENPCEventReplicationMode.AuthorityAndClients : ENPCEventReplicationMode.AuthorityOnly;
			BroadcastEvent(instigatingPlayer, eventId, replicationMode);
		}

		public static void BroadcastEvent(Player instigatingPlayer, string eventId, ENPCEventReplicationMode replicationMode)
		{
			if (string.IsNullOrEmpty(eventId))
				return;

			bool invokeLocally;
			bool shouldReplicate;
			switch (replicationMode)
			{
				default:
				case ENPCEventReplicationMode.AuthorityOnly:
				{
					invokeLocally = true;
					shouldReplicate = false;
					break;
				}

				case ENPCEventReplicationMode.AuthorityAndClients:
				{
					invokeLocally = true;
					shouldReplicate = true;
					break;
				}

				case ENPCEventReplicationMode.InstigatorOnly:
				{
					if (instigatingPlayer == null)
					{
						return;
					}

					invokeLocally = instigatingPlayer.channel.IsLocalPlayer;
					shouldReplicate = !invokeLocally;
					break;
				}
			}

			if (invokeLocally)
			{
				try
				{
					onEvent?.Invoke(instigatingPlayer, eventId);
				}
				catch (Exception e)
				{
					UnturnedLog.exception(e, "Exception raised during server NPC event \"{0}\"", eventId);
				}
			}

			if (shouldReplicate)
			{
				byte playerChannelId;
				if (instigatingPlayer != null && instigatingPlayer.channel != null && instigatingPlayer.channel.owner != null)
				{
					playerChannelId = (byte) instigatingPlayer.channel.owner.channel;
				}
				else
				{
					playerChannelId = 0;
				}

				if (replicationMode == ENPCEventReplicationMode.InstigatorOnly)
				{
					SendBroadcast.Invoke(NetTransport.ENetReliability.Reliable, instigatingPlayer.channel?.GetOwnerTransportConnection(), playerChannelId, eventId);
				}
				else
				{
					SendBroadcast.Invoke(NetTransport.ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), playerChannelId, eventId);
				}
			}
		}

		private static readonly ClientStaticMethod<byte, string> SendBroadcast = ClientStaticMethod<byte, string>.Get(ReceiveBroadcast);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveBroadcast(byte channelId, string eventId)
		{
			SteamPlayer instigatingClient = PlayerTool.findSteamPlayerByChannel(channelId);
			Player instigatingPlayer = instigatingClient?.player; // Use if available, but not necessary on the client anyway.

			try
			{
				onEvent?.Invoke(instigatingPlayer, eventId);
			}
			catch (Exception e)
			{
				UnturnedLog.exception(e, "Exception raised during client NPC event \"{0}\"", eventId);
			}
		}
	}
}
