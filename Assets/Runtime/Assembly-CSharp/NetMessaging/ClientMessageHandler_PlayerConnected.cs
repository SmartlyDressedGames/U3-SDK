////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class ClientMessageHandler_PlayerConnected
	{
		internal static void ReadMessage(NetPakReader reader)
		{
			NetId netId;
			reader.ReadNetId(out netId);
			CSteamID steamID;
			reader.ReadSteamID(out steamID);
			byte characterID;
			reader.ReadUInt8(out characterID);
			string playerName;
			reader.ReadString(out playerName);
			string characterName;
			reader.ReadString(out characterName);
			Vector3 position;
			reader.ReadClampedVector3(out position);
			byte compressedAngle;
			reader.ReadUInt8(out compressedAngle);
			bool isPro;
			reader.ReadBit(out isPro);
			bool isAdmin;
			reader.ReadBit(out isAdmin);
			byte channel;
			reader.ReadUInt8(out channel);
			CSteamID groupID;
			reader.ReadSteamID(out groupID);
			string nickName;
			reader.ReadString(out nickName);
			byte face;
			reader.ReadUInt8(out face);
			byte hair;
			reader.ReadUInt8(out hair);
			byte beard;
			reader.ReadUInt8(out beard);
			Color32 skinColor;
			reader.ReadColor32RGB(out skinColor);
			Color32 hairColor;
			reader.ReadColor32RGB(out hairColor);
			Color32 markerColor;
			reader.ReadColor32RGB(out markerColor);
			Color32 beardColor;
			reader.ReadColor32RGB(out beardColor);
			bool leftHanded;
			reader.ReadBit(out leftHanded);
			int shirtItem;
			reader.ReadInt32(out shirtItem);
			int pantsItem;
			reader.ReadInt32(out pantsItem);
			int hatItem;
			reader.ReadInt32(out hatItem);
			int backpackItem;
			reader.ReadInt32(out backpackItem);
			int vestItem;
			reader.ReadInt32(out vestItem);
			int maskItem;
			reader.ReadInt32(out maskItem);
			int glassesItem;
			reader.ReadInt32(out glassesItem);
			skinItems.Clear();
			reader.ReadList(skinItems, (out int item) => { return reader.ReadInt32(out item); }, MAX_LENGTH);
			skinTags.Clear();
			reader.ReadList(skinTags, (out string tag) => { return reader.ReadString(out tag); }, MAX_LENGTH);
			skinDynamicProps.Clear();
			reader.ReadList(skinDynamicProps, (out string dynProp) => { return reader.ReadString(out dynProp); }, MAX_LENGTH);
			EPlayerSkillset skillset;
			reader.ReadEnum(out skillset);
			string language;
			reader.ReadString(out language);

			SteamPlayer newClient = Provider.addPlayer(null,
				netId,
				new SteamPlayerID(steamID, characterID, playerName, characterName, nickName, groupID),
				position,
				compressedAngle,
				isPro,
				isAdmin,
				channel,
				face,
				hair,
				beard,
				skinColor,
				hairColor,
				markerColor,
				beardColor,
				leftHanded,
				shirtItem,
				pantsItem,
				hatItem,
				backpackItem,
				vestItem,
				maskItem,
				glassesItem,
				skinItems.ToArray(),
				skinTags.ToArray(),
				skinDynamicProps.ToArray(),
				skillset,
				language,
				CSteamID.Nil,
				default);
			newClient.player.InitializePlayer();
		}

		private static List<int> skinItems = new List<int>();
		private static List<string> skinTags = new List<string>();
		private static List<string> skinDynamicProps = new List<string>();
		private static readonly NetLength MAX_LENGTH = new NetLength(255);
	}
}
