////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class SteamGroup
	{
		private CSteamID _steamID;
		public CSteamID steamID => _steamID;

		private string _name;
		public string name => _name;

		private Texture2D _icon;
		public Texture2D icon => _icon;

		public SteamGroup(CSteamID newSteamID, string newName, Texture2D newIcon)
		{
			_steamID = newSteamID;
			_name = newName;
			_icon = newIcon;
		}
	}
}