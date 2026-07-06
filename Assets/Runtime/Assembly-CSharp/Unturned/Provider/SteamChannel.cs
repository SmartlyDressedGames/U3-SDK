////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SDG.Unturned
{
	public enum ESteamCallValidation
	{
		NONE,
		/// <summary>
		/// Only RPCs from the server will be allowed to invoke this method.
		/// </summary>
		ONLY_FROM_SERVER,
		/// <summary>
		/// RPCs are only allowed to invoke this method if we're running as server.
		/// </summary>
		SERVERSIDE,
		/// <summary>
		/// Only RPCs from the owner of the object will be allowed to invoke this method.
		/// </summary>
		ONLY_FROM_OWNER,
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class SteamCall : Attribute
	{
		public ESteamCallValidation validation;

		/// <summary>
		/// Maximum number of calls per-second per-player.
		/// </summary>
		public int ratelimitHz = -1;

		/// <summary>
		/// Minimum seconds between calls per-player.
		/// Initialized from ratelimitHz when gathering RPCs.
		/// </summary>
		public float ratelimitSeconds = -1;

		/// <summary>
		/// Index into per-connection rate limiting array.
		/// </summary>
		public int rateLimitIndex = -1;

		/// <summary>
		/// Backwards compatibility for older invoke by name code e.g. plugins.
		/// </summary>
		public string legacyName;

		public ENetInvocationDeferMode deferMode;

		public SteamCall(ESteamCallValidation validation)
		{
			this.validation = validation;
		}
	}

	[System.Obsolete]
	public delegate void TriggerSend(SteamPlayer player, string name, ESteamCall mode, ESteamPacket type, params object[] arguments);
	[System.Obsolete]
	public delegate void TriggerReceive(SteamChannel channel, CSteamID steamID, byte[] packet, int offset, int size);

	public class SteamChannel : MonoBehaviour
	{
		/// <summary>
		/// If changing header size remember to update PlayerManager and allocPlayerChannelId.
		/// </summary>
		public const int CHANNEL_ID_HEADER_SIZE = 1;

		public const int RPC_HEADER_SIZE = 2;
		[System.Obsolete]
		public const int VOICE_HEADER_SIZE = 3;
		/// <summary>
		/// How far to shift compressed voice data.
		/// </summary>
		[System.Obsolete]
		public const int VOICE_DATA_OFFSET = CHANNEL_ID_HEADER_SIZE + RPC_HEADER_SIZE + VOICE_HEADER_SIZE;

		public SteamChannelMethod[] calls
		{
			get;
			protected set;
		}

		public int id;
		public SteamPlayer owner;

		/// <summary>
		/// If true, this object is owned by a locally-controlled player.
		/// For example, some code is not run for "remote" players.
		/// Always true in singleplayer. Always false on dedicated server.
		/// </summary>
		public bool IsLocalPlayer
		{
#pragma warning disable
			get => isOwner;
			internal set { isOwner = value; }
#pragma warning restore
		}

		/// <summary>
		/// Use on server when invoking client methods on the owning player.
		/// </summary>
		public ITransportConnection GetOwnerTransportConnection()
		{
			return owner?.transportConnection;
		}

		[System.Obsolete]
		public bool checkServer(CSteamID steamID)
		{
			return steamID == Provider.server;
		}

		[System.Obsolete]
		public bool checkOwner(CSteamID steamID)
		{
			if (owner == null)
			{
				return false;
			}

			return steamID == owner.playerID.steamID;
		}

		/// <summary>
		/// Replacement for ESteamCall.NOT_OWNER.
		/// </summary>
		public PooledTransportConnectionList GatherRemoteClientConnectionsExcludingOwner()
		{
			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			foreach (SteamPlayer potentialRecipient in Provider.clients)
			{
#if !DEDICATED_SERVER
				if (potentialRecipient.IsLocalServerHost)
					continue;
#endif // !DEDICATED_SERVER

				if (potentialRecipient != owner)
				{
					list.Add(potentialRecipient.transportConnection);
				}
			}
			return list;
		}

		[System.Obsolete("Replaced by GatherRemoteClientConnectionsExcludingOwner")]
		public IEnumerable<ITransportConnection> EnumerateClients_RemoteNotOwner()
		{
			return GatherRemoteClientConnectionsExcludingOwner();
		}

		public PooledTransportConnectionList GatherRemoteClientConnectionsWithinSphereExcludingOwner(Vector3 position, float radius)
		{
			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			float sqrRadius = radius * radius;
			foreach (SteamPlayer potentialRecipient in Provider.clients)
			{
#if !DEDICATED_SERVER
				if (potentialRecipient.IsLocalServerHost)
					continue;
#endif // !DEDICATED_SERVER

				if (potentialRecipient != owner
					&& potentialRecipient.player != null
					&& (potentialRecipient.player.transform.position - position).sqrMagnitude < sqrRadius)
				{
					list.Add(potentialRecipient.transportConnection);
				}
			}
			return list;
		}

		[System.Obsolete("Replaced by GatherRemoteClientConnectionsWithinSphereExcludingOwner")]
		public IEnumerable<ITransportConnection> EnumerateClients_RemoteNotOwnerWithinSphere(Vector3 position, float radius)
		{
			return GatherRemoteClientConnectionsWithinSphereExcludingOwner(position, radius);
		}

		public PooledTransportConnectionList GatherOwnerAndClientConnectionsWithinSphere(Vector3 position, float radius)
		{
			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			float sqrRadius = radius * radius;
			foreach (SteamPlayer potentialRecipient in Provider.clients)
			{
				if (potentialRecipient == owner ||
					(potentialRecipient.player != null && (potentialRecipient.player.transform.position - position).sqrMagnitude < sqrRadius))
				{
					list.Add(potentialRecipient.transportConnection);
				}
			}
			return list;
		}

		[System.Obsolete("Replaced by GatherOwnerAndClientConnectionsWithinSphere")]
		public IEnumerable<ITransportConnection> EnumerateClients_WithinSphereOrOwner(Vector3 position, float radius)
		{
			return GatherOwnerAndClientConnectionsWithinSphere(position, radius);
		}

		/// <summary>
		/// Don't use this. Originally added so that Rocketmod didn't have to inject into the game's assembly.
		/// </summary>
		[System.Obsolete("Will be deprecated soon. Please discuss on the issue tracker and we will find an alternative.")]
		public static TriggerReceive onTriggerReceive;
		private static bool warnedAboutTriggerReceive;

		/// <returns>True if the call succeeded, or false if the sender should be refused.</returns>
		[System.Obsolete]
		public bool receive(CSteamID steamID, byte[] packet, int offset, int size)
		{
#pragma warning disable
			if (onTriggerReceive != null)
#pragma warning restore
			{
				if (!warnedAboutTriggerReceive)
				{
					warnedAboutTriggerReceive = true;
					CommandWindow.LogError("Plugin(s) using unsafe onTriggerReceive which will be deprecated soon.");
				}

				try
				{
					byte[] payload = packet;
					if (Provider.useConstNetEvents)
					{
						payload = new byte[offset + size];
						Array.Copy(packet, payload, payload.Length);
					}

#pragma warning disable
					onTriggerReceive(this, steamID, payload, offset, size);
#pragma warning restore

					if (Provider.useConstNetEvents && Provider.hasNetBufferChanged(packet, payload, offset, size))
					{
						CommandWindow.LogError("Plugin(s) modified buffer during onTriggerReceive!");
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.warn("Plugin raised an exception from SteamChannel.onTriggerReceive:");
					UnturnedLog.exception(exception);
				}
			}

			if (size < CHANNEL_ID_HEADER_SIZE + RPC_HEADER_SIZE)
			{
				// UnturnedLog.warn("RPC from " + steamID + " packet too small, so we're refusing them");
				return true;
			}

			int index = packet[offset + 1];

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			int channelHeader;
			if (Provider.getChannelHeader(packet, size, offset, out channelHeader))
			{
				if (channelHeader != id)
				{
					CommandWindow.LogErrorFormat("\tChannel {0} ({1}) received message with wrong channel in header ({2})", id, name, channelHeader);
				}
			}
			else
			{
				CommandWindow.LogErrorFormat("\tChannel {0} ({1}) unable to determine channel from header", id, name);
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			buildCallArrayIfDirty();
			if (index < 0 || index >= calls.Length)
			{
				// Originally this caused a kick, but indexes are rebuilt when items are equipped so it's unreliable.
				//UnturnedLog.warn("RPC from " + steamID + " call index " + index + " out of bounds, so we're refusing them");
				return true;
			}

			ESteamPacket type = (ESteamPacket) packet[offset];

			bool validated;
			switch (calls[index].attribute.validation)
			{
				case ESteamCallValidation.NONE:
					validated = true;
					break;

				case ESteamCallValidation.ONLY_FROM_SERVER:
					validated = steamID == Provider.server; // Valid if it was sent by the server.
					break;

				case ESteamCallValidation.SERVERSIDE:
					validated = Provider.isServer; // Valid only when running as server.
					break;

				case ESteamCallValidation.ONLY_FROM_OWNER:
					// Note that some ONLY_FROM_OWNER methods are clientside, e.g. tellVoice, so we DON'T do an isServer check at least for now.
					validated = owner != null && steamID == owner.playerID.steamID; // Valid if it was sent by the owner of this channel.
					break;

				default:
					validated = false;
					UnturnedLog.warn("Unhandled RPC validation type on method: " + calls[index].method.Name);
					break;
			}

			if (!validated)
			{
				// Previously this caused a kick, but which RPC the server thinks it's calling can be different from the client
				// if the RPC indexes changed after sending. This could be avoided by sending the RPC name to be invoked...
				// For the moment we just silently ignore the request.
				//UnturnedLog.warn("RPC " + calls[index].method.Name + " from " + steamID + " didn't pass validation type " + calls[index].attribute.validation + ", so we're refusing them");
				return true;
			}

			if (calls[index].attribute.rateLimitIndex >= 0)
			{
				string rpcId = calls[index].method.Name;

				SteamPlayer player = PlayerTool.getSteamPlayer(steamID);
				if (player == null)
				{
					// Receive is only called by Provider if a SteamPlayer was found in the first place though...
					UnturnedLog.info("RPC " + rpcId + " on channel " + id + " called without player sender, so we're ignoring it");
					return true;
				}

				float currentTime = Time.realtimeSinceStartup;
				float nextAllowedTime = player.rpcAllowedTimes[calls[index].attribute.rateLimitIndex];
				if (currentTime < nextAllowedTime)
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					CommandWindow.LogWarningFormat("Hit {0} rate limit on channel {1}", rpcId, id);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					return true; // Return valid because we do not necessarily want to kick them.
				}
				else
				{
					player.rpcAllowedTimes[calls[index].attribute.rateLimitIndex] = currentTime + calls[index].attribute.ratelimitSeconds;
				}
			}

#if STATDEBUG
			calls[index].received ++;
#endif

#if NETDEBUG
				UnturnedLog.info("Receiving call "+calls[index].method.Name+" with "+size+" bytes from "+steamID+" on channel "+id+".");
#endif

			UnityEngine.Profiling.Profiler.BeginSample("Receive " + calls[index].method.Name, gameObject);

			try
			{
				if (calls[index].types.Length > 0)
				{
					object[] objects = SteamPacker.getObjectsForLegacyRPC(offset, CHANNEL_ID_HEADER_SIZE + RPC_HEADER_SIZE, size, packet, calls[index].types, calls[index].typesReadOffset);
					switch (calls[index].contextType)
					{
						case SteamChannelMethod.EContextType.Client:
#pragma warning disable
							objects[calls[index].contextParameterIndex] = new ClientInvocationContext();
#pragma warning restore
							break;

						case SteamChannelMethod.EContextType.Server:
#pragma warning disable
							objects[calls[index].contextParameterIndex] = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
#pragma warning restore
							break;
					}

					if (calls[index].method.IsStatic)
					{
						calls[index].method.Invoke(null, objects);
					}
					else
					{
						calls[index].method.Invoke(calls[index].component, objects);
					}
				}
				else
				{
					UnityEngine.Profiling.Profiler.BeginSample("Invoke Empty");
					calls[index].method.Invoke(calls[index].component, null);
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.info("Exception raised when RPC invoked {0}:", calls[index].method.Name);
				UnturnedLog.exception(exception);
			}

			UnityEngine.Profiling.Profiler.EndSample();
			return true;
		}

		[System.Obsolete]
		public object read(Type type)
		{
			return SteamPacker.read(type);
		}

		[System.Obsolete]
		public object[] read(Type type_0, Type type_1, Type type_2)
		{
			return SteamPacker.read(type_0, type_1, type_2);
		}

		[System.Obsolete]
		public object[] read(Type type_0, Type type_1, Type type_2, Type type_3)
		{
			return SteamPacker.read(type_0, type_1, type_2, type_3);
		}

		[System.Obsolete]
		public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
		{
			return SteamPacker.read(type_0, type_1, type_2, type_3, type_4, type_5);
		}

		[System.Obsolete]
		public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
		{
			return SteamPacker.read(type_0, type_1, type_2, type_3, type_4, type_5, type_6);
		}

		[System.Obsolete]
		public object[] read(params Type[] types)
		{
			return SteamPacker.read(types);
		}

		[System.Obsolete]
		public void write(object objects)
		{ }

		[System.Obsolete]
		public void write(object object_0, object object_1, object object_2)
		{ }

		[System.Obsolete]
		public void write(object object_0, object object_1, object object_2, object object_3)
		{ }

		[System.Obsolete]
		public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5)
		{ }

		[System.Obsolete]
		public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5, object object_6)
		{ }

		[System.Obsolete]
		public void write(params object[] objects)
		{ }

		[System.Obsolete]
		public bool longBinaryData
		{
			get => SteamPacker.longBinaryData;

			set => SteamPacker.longBinaryData = value;
		}

		[System.Obsolete]
		public void openWrite()
		{ }

		[System.Obsolete]
		public void closeWrite(string name, CSteamID steamID, ESteamPacket type)
		{ }

		[System.Obsolete]
		public void closeWrite(string name, ESteamCall mode, byte bound, ESteamPacket type)
		{ }

		[System.Obsolete]
		public void closeWrite(string name, ESteamCall mode, byte x, byte y, byte area, ESteamPacket type)
		{ }

		[System.Obsolete]
		public void closeWrite(string name, ESteamCall mode, ESteamPacket type)
		{ }

		[System.Obsolete]
		public void send(string name, CSteamID steamID, ESteamPacket type, params object[] arguments)
		{
			int index = getCall(name);

			if (index == -1)
			{
				return;
			}

			int size;
			byte[] packet;
			getPacket(type, index, out size, out packet, arguments);

			if (IsLocalPlayer && steamID == Provider.client)
			{
				receive(Provider.client, packet, 0, size);
			}
			else if (Provider.isServer && steamID == Provider.server)
			{
				receive(Provider.server, packet, 0, size);
			}
			else
			{
				Provider.send(steamID, type, packet, size, 0);
			}
		}

		[System.Obsolete]
		public void sendAside(string name, CSteamID steamID, ESteamPacket type, params object[] arguments)
		{
			// This method was not referenced by the base game.
		}

		[System.Obsolete]
		public void send(ESteamCall mode, byte bound, ESteamPacket type, int size, byte[] packet)
		{
#pragma warning disable
			if (mode == ESteamCall.SERVER)
#pragma warning restore
			{
				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					throw new System.NotSupportedException();
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.ALL)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && Provider.clients[step].player.movement.bound == bound)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					receive(Provider.client, packet, 0, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OTHERS)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && Provider.clients[step].player.movement.bound == bound)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OWNER)
#pragma warning restore
			{
				if (IsLocalPlayer)
				{
					receive(owner.playerID.steamID, packet, 0, size);
				}
				else
				{
					Provider.sendToClient(owner.transportConnection, type, packet, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.NOT_OWNER)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != owner.playerID.steamID && Provider.clients[step].player != null && Provider.clients[step].player.movement.bound == bound)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.CLIENTS)
#pragma warning restore
			{
				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && Provider.clients[step].player.movement.bound == bound)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isClient)
				{
					receive(Provider.client, packet, 0, size);
				}
			}
		}

		[System.Obsolete]
		public void send(string name, ESteamCall mode, byte bound, ESteamPacket type, params object[] arguments)
		{
			int index = getCall(name);

			if (index == -1)
			{
				return;
			}

			int size;
			byte[] packet;
			getPacket(type, index, out size, out packet, arguments);

			send(mode, bound, type, size, packet);
		}

		[System.Obsolete]
		public void send(ESteamCall mode, byte x, byte y, byte area, ESteamPacket type, int size, byte[] packet)
		{
#pragma warning disable
			if (mode == ESteamCall.SERVER)
#pragma warning restore
			{
				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					throw new System.NotSupportedException();
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.ALL)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && Regions.checkArea(x, y, Provider.clients[step].player.movement.region_x, Provider.clients[step].player.movement.region_y, area))
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					receive(Provider.client, packet, 0, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OTHERS)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && Regions.checkArea(x, y, Provider.clients[step].player.movement.region_x, Provider.clients[step].player.movement.region_y, area))
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OWNER)
#pragma warning restore
			{
				if (IsLocalPlayer)
				{
					receive(owner.playerID.steamID, packet, 0, size);
				}
				else
				{
					Provider.sendToClient(owner.transportConnection, type, packet, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.NOT_OWNER)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != owner.playerID.steamID && Provider.clients[step].player != null && Regions.checkArea(x, y, Provider.clients[step].player.movement.region_x, Provider.clients[step].player.movement.region_y, area))
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.CLIENTS)
#pragma warning restore
			{
				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && Regions.checkArea(x, y, Provider.clients[step].player.movement.region_x, Provider.clients[step].player.movement.region_y, area))
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isClient)
				{
					receive(Provider.client, packet, 0, size);
				}
			}
		}

		[System.Obsolete]
		public void send(string name, ESteamCall mode, byte x, byte y, byte area, ESteamPacket type, params object[] arguments)
		{
			int index = getCall(name);

			if (index == -1)
			{
				return;
			}

			int size;
			byte[] packet;
			getPacket(type, index, out size, out packet, arguments);

			send(mode, x, y, area, type, size, packet);
		}

		[System.Obsolete]
		public void send(ESteamCall mode, ESteamPacket type, int size, byte[] packet)
		{
#pragma warning disable
			if (mode == ESteamCall.SERVER)
#pragma warning restore
			{
				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					throw new System.NotSupportedException();
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.ALL)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					receive(Provider.client, packet, 0, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OTHERS)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OWNER)
#pragma warning restore
			{
				if (IsLocalPlayer)
				{
					receive(owner.playerID.steamID, packet, 0, size);
				}
				else
				{
					Provider.sendToClient(owner.transportConnection, type, packet, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.NOT_OWNER)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != owner.playerID.steamID)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.CLIENTS)
#pragma warning restore
			{
				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isClient)
				{
					receive(Provider.client, packet, 0, size);
				}
			}
		}

		/// <summary>
		/// Don't use this. Originally added so that Rocketmod didn't have to inject into the game's assembly.
		/// </summary>
		[System.Obsolete("Will be deprecated soon. Please discuss on the issue tracker and we will find an alternative.")]
		public static TriggerSend onTriggerSend;
		private static bool warnedAboutTriggerSend;

		[System.Obsolete]
		public void send(string name, ESteamCall mode, ESteamPacket type, params object[] arguments)
		{
#pragma warning disable
			if (onTriggerSend != null)
#pragma warning restore
			{
				if (!warnedAboutTriggerSend)
				{
					warnedAboutTriggerSend = true;
					CommandWindow.LogError("Plugin(s) using unsafe onTriggerSend which will be deprecated soon.");
				}

				try
				{
#pragma warning disable
					onTriggerSend(owner, name, mode, type, arguments);
#pragma warning restore
				}
				catch (System.Exception exception)
				{
					UnturnedLog.warn("Plugin raised an exception from SteamChannel.onTriggerSend:");
					UnturnedLog.exception(exception);
				}
			}

			int index = getCall(name);

			if (index == -1)
			{
				return;
			}

			int size;
			byte[] packet;
			getPacket(type, index, out size, out packet, arguments);

			send(mode, type, size, packet);
		}

		[System.Obsolete]
		public void send(ESteamCall mode, Vector3 point, float radius, ESteamPacket type, int size, byte[] packet)
		{
			radius *= radius;

#pragma warning disable
			if (mode == ESteamCall.SERVER)
#pragma warning restore
			{
				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					throw new System.NotSupportedException();
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.ALL)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && (Provider.clients[step].player.transform.position - point).sqrMagnitude < radius)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isServer)
				{
					receive(Provider.server, packet, 0, size);
				}
				else
				{
					receive(Provider.client, packet, 0, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OTHERS)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && (Provider.clients[step].player.transform.position - point).sqrMagnitude < radius)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.OWNER)
#pragma warning restore
			{
				if (IsLocalPlayer)
				{
					receive(owner.playerID.steamID, packet, 0, size);
				}
				else
				{
					Provider.sendToClient(owner.transportConnection, type, packet, size);
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.NOT_OWNER)
#pragma warning restore
			{
				if (!Provider.isServer)
				{
					throw new System.NotSupportedException();
				}

				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != owner.playerID.steamID && Provider.clients[step].player != null && (Provider.clients[step].player.transform.position - point).sqrMagnitude < radius)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}
			}
#pragma warning disable
			else if (mode == ESteamCall.CLIENTS)
#pragma warning restore
			{
				for (int step = 0; step < Provider.clients.Count; step++)
				{
					if (Provider.clients[step].playerID.steamID != Provider.client && Provider.clients[step].player != null && (Provider.clients[step].player.transform.position - point).sqrMagnitude < radius)
					{
						Provider.sendToClient(Provider.clients[step].transportConnection, type, packet, size);
					}
				}

				if (Provider.isClient)
				{
					receive(Provider.client, packet, 0, size);
				}
			}
		}

		[System.Obsolete]
		public void send(string name, ESteamCall mode, Vector3 point, float radius, ESteamPacket type, params object[] arguments)
		{
			int index = getCall(name);

			if (index == -1)
			{
				return;
			}

			int size;
			byte[] packet;
			getPacket(type, index, out size, out packet, arguments);

			send(mode, point, radius, type, size, packet);
		}

		/// <summary>
		/// Calls array needs rebuilding the next time it is used.
		/// Should be invoked when adding/removing components with RPCs.
		/// </summary>
		public void markDirty()
		{
			callArrayDirty = true;
		}

		/// <summary>
		/// Does array of RPCs need to be rebuilt?
		/// </summary>
		private bool callArrayDirty = true;

		// Working lists for buildCallArray.
		private static List<SteamChannelMethod> workingCalls = new List<SteamChannelMethod>();
		private static List<Component> workingComponents = new List<Component>();

		/// <summary>
		/// Find methods with SteamCall attribute, and gather them into an array.
		/// </summary>
		private void buildCallArray()
		{
			workingCalls.Clear();
			workingComponents.Clear();

			GetComponents(workingComponents);
			foreach (Component component in workingComponents)
			{
				if ((component.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable)
				{
					// We use NotEditable as a "pending destroy" flag for dynamic useable component.
					continue;
				}

				MemberInfo[] componentMethods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
				foreach (MethodInfo method in componentMethods)
				{
					SteamCall attribute = method.GetCustomAttribute<SteamCall>();
					if (attribute == null)
						continue;

					string legacyMethodName = attribute.legacyName;
					if (string.IsNullOrEmpty(legacyMethodName))
					{
						legacyMethodName = method.Name;
					}

					ParameterInfo[] parameters = method.GetParameters();
					Type[] types = new Type[parameters.Length];

					for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
					{
						types[parameterIndex] = parameters[parameterIndex].ParameterType;
					}

					int typesReadOffset = 0;
					SteamChannelMethod.EContextType contextType = default;
					int contextParameterIndex = -1;

					if (typesReadOffset < types.Length)
					{
						if (types[typesReadOffset].GetElementType() == typeof(ClientInvocationContext))
						{
							contextParameterIndex = typesReadOffset;
							++typesReadOffset;
							contextType = SteamChannelMethod.EContextType.Client;
						}
						else if (types[typesReadOffset].GetElementType() == typeof(ServerInvocationContext))
						{
							contextParameterIndex = typesReadOffset;
							++typesReadOffset;
							contextType = SteamChannelMethod.EContextType.Server;
						}
					}

					if (attribute.ratelimitHz > 0)
					{
						attribute.ratelimitSeconds = 1.0f / attribute.ratelimitHz;

						ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(method.DeclaringType, method.Name);
						if (serverMethodInfo != null)
						{
							attribute.rateLimitIndex = serverMethodInfo.rateLimitIndex;
						}
						//UnturnedLog.info("{0} {1}Hz {2}s", method.Name, attribute.ratelimitHz, attribute.ratelimitSeconds);
					}

					workingCalls.Add(new SteamChannelMethod(component, method, legacyMethodName, types, typesReadOffset, contextType, contextParameterIndex, attribute));
				}
			}

			//for(int index = 0; index < found.Count; index++)
			//{
			//	UnturnedLog.info("Assigning {0} to {1} on channel {2}", found[index].method.Name, index);
			//}

			calls = workingCalls.ToArray();

			if (calls.Length > 235)
			{
				CommandWindow.LogError(name + " approaching 255 methods!");
			}
		}

		private void buildCallArrayIfDirty()
		{
			if (callArrayDirty)
			{
				callArrayDirty = false;
				buildCallArray();
			}
		}

		public void setup()
		{
			Provider.openChannel(this);
		}

		private void encodeChannelId(byte[] packet)
		{
			packet[RPC_HEADER_SIZE] = (byte) (id & 0xFF);
			//packet[2] = (byte) ((id >> 24) & 0xFF);
			//packet[3] = (byte) ((id >> 16) & 0xFF);
			//packet[4] = (byte) ((id >> 8) & 0xFF);
			//packet[5] = (byte) (id & 0xFF);
		}

		[System.Obsolete]
		public void getPacket(ESteamPacket type, int index, out int size, out byte[] packet)
		{
			packet = SteamPacker.closeWrite(out size);
			packet[0] = (byte) type;
			packet[1] = (byte) index;
			encodeChannelId(packet);
		}

		/// <summary>
		/// Encode byte array of voice data to send.
		/// </summary>
		[System.Obsolete]
		public void encodeVoicePacket(byte callIndex, out int size, out byte[] packet, byte[] bytes, ushort length, bool usingWalkieTalkie)
		{
			size = 0;
			packet = null;
		}

		/// <summary>
		/// Decode voice parameters from byte array.
		/// </summary>
		[System.Obsolete]
		public void decodeVoicePacket(byte[] packet, out uint compressedSize, out bool usingWalkieTalkie)
		{
			compressedSize = 0;
			usingWalkieTalkie = false;
		}

		[System.Obsolete]
		public void sendVoicePacket(SteamPlayer player, byte[] packet, int packetSize)
		{ }

		[System.Obsolete]
		public void getPacket(ESteamPacket type, int index, out int size, out byte[] packet, params object[] arguments)
		{
			packet = SteamPacker.getBytes(CHANNEL_ID_HEADER_SIZE + RPC_HEADER_SIZE, out size, arguments);
			packet[0] = (byte) type;
			packet[1] = (byte) index;
			encodeChannelId(packet);
		}

		[System.Obsolete]
		public int getCall(string name)
		{
			buildCallArrayIfDirty();

			for (int index = 0; index < calls.Length; index++)
			{
				if (calls[index].legacyMethodName == name)
				{
#if STATDEBUG
					calls[index].sent ++;
#endif

					return index;
				}
			}

			CommandWindow.LogError("Failed to find a method named: " + name);
			return -1;
		}

		private void OnDestroy()
		{
			if (id != 0)
			{
				Provider.closeChannel(this);
			}
		}

		[System.Obsolete("Renamed to IsLocalPlayer")]
		public bool isOwner;
	}
}
