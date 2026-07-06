////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// String table specifically for Unity physics material names.
	/// Implemented so that tires can more efficiently replicate which ground material they are touching.
	/// </summary>
	public static class PhysicsMaterialNetTable
	{
		/// <summary>
		/// Get an ID that can be used to reference a physics material name over the network. If given material name
		/// isn't supported (e.g., not registered in a PhysicsMaterialAsset or over max material limit) returns
		/// <see cref="PhysicsMaterialNetId.NULL"/> instead.
		/// </summary>
		public static PhysicsMaterialNetId GetNetId(string materialName)
		{
			if (string.IsNullOrEmpty(materialName))
			{
				return PhysicsMaterialNetId.NULL;
			}

			if (nameToId.TryGetValue(materialName, out uint id))
			{
				return new PhysicsMaterialNetId(id);
			}
			else
			{
				return PhysicsMaterialNetId.NULL;
			}
		}

		/// <summary>
		/// Get name of a physics material from network ID. Returns null if ID is null, e.g., if the sent name wasn't
		/// registered or was over the max material limit.
		/// </summary>
		public static string GetMaterialName(PhysicsMaterialNetId netId)
		{
			if (netId.id > 0)
			{
				if (idToName.TryGetValue(netId.id, out string name))
				{
					return name;
				}
			}

			return null;
		}

		/// <summary>
		/// Called when resetting network state.
		/// </summary>
		internal static void Clear()
		{
			nameToId.Clear();
			idToName.Clear();
		}

		/// <summary>
		/// Called on server and singleplayer before loading level.
		/// </summary>
		internal static void ServerPopulateTable()
		{
			uint nextId = 1;

			foreach (KeyValuePair<System.Guid, PhysicsMaterialAsset> pair in PhysicMaterialCustomData.GetAssets())
			{
				PhysicsMaterialAsset asset = pair.Value;
				if (asset.physicMaterialNames == null || asset.physicMaterialNames.Length < 1)
				{
					continue;
				}

				uint id = nextId;
				++nextId;
				bool hasAddedNameForId = false;

				foreach (string name in asset.physicMaterialNames)
				{
					if (nameToId.ContainsKey(name))
					{
						// Duplicated name?
						UnturnedLog.warn($"Multiple physics material assets contain Unity name \"{name}\"");
						continue;
					}

					nameToId[name] = id;
					if (!hasAddedNameForId)
					{
						idToName[id] = name;
						hasAddedNameForId = true;
					}
				}
			}

			idBitCount = NetPakConst.CountBits(nextId);
			UnturnedLog.info($"Server registered {nameToId.Count} Unity physics material names with {nextId - 1} unique IDs ({idBitCount} bits)");
		}

		internal static void Send(ITransportConnection transportConnection)
		{
			SendMappings.Invoke(ENetReliability.Reliable, transportConnection, SendMappings_Write);
		}

		private static void SendMappings_Write(NetPakWriter writer)
		{
			writer.WriteUInt8((byte) nameToId.Count);
			writer.WriteUInt8((byte) idBitCount);
			foreach (KeyValuePair<string, uint> pairing in nameToId)
			{
				string materialName = pairing.Key;
				uint id = pairing.Value;
				writer.WriteString(materialName, lengthBitCount: PhysicsTool.NAME_LENGTH_BITS);
				writer.WriteBits(id, idBitCount);
			}
		}

		private static readonly ClientStaticMethod SendMappings = ClientStaticMethod.Get(ReceiveMappings);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveMappings(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			byte count;
			reader.ReadUInt8(out count);
			reader.ReadUInt8(out byte tempIdBitCount);
			idBitCount = tempIdBitCount;

			for (int index = 0; index < count; ++index)
			{
				reader.ReadString(out string materialName, lengthBitCount: PhysicsTool.NAME_LENGTH_BITS);
				reader.ReadBits(idBitCount, out uint id);

				nameToId[materialName] = id;
				idToName[id] = materialName;
			}

			UnturnedLog.info($"Client received {nameToId.Count} Unity physics material names ({idBitCount} bits)");
		}

		/// <summary>
		/// Number of bits needed to replicate PhysicsMaterialNetId.
		/// </summary>
		internal static int idBitCount;

		private static Dictionary<string, uint> nameToId = new Dictionary<string, uint>();
		private static Dictionary<uint, string> idToName = new Dictionary<uint, string>();
	}
}
