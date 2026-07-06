////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// Each level should have a 380x80 Icon.png file.
	/// This class caches them so that the server list can show them quickly.
	/// </summary>
	public static class LevelIconCache
	{
		public static Texture2D GetOrLoadIcon(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			name = name.Trim();

			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			if (icons.TryGetValue(name, out Texture2D icon))
			{
				return icon;
			}

			Texture2D result = LoadIcon(name);
			icons.Add(name, result);
			return result;
		}

		public static Texture2D GetOrLoadIcon(LevelInfo levelInfo)
		{
			if (icons.TryGetValue(levelInfo.name, out Texture2D icon))
			{
				return icon;
			}

			Texture2D result = LoadIcon(levelInfo);
			icons.Add(levelInfo.name, result);
			return result;
		}

		private static Texture2D LoadIcon(string name)
		{
			LevelInfo levelInfo = Level.getLevel(name);
			if (levelInfo != null)
			{
				return LoadIcon(levelInfo);
			}

			string iconsDir = Path.Join(ReadWrite.PATH, "CuratedMapIcons");
#if UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST
			if (!Directory.Exists(iconsDir) && Provider.steamAppInstallDirectory != null)
			{
				iconsDir = PathEx.Join(Provider.steamAppInstallDirectory, "CuratedMapIcons");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || !WITH_NOREDIST

			foreach (CuratedMapLink link in Provider.statusData.Maps.Curated_Map_Links)
			{
				if (string.Equals(link.Name, name, StringComparison.OrdinalIgnoreCase))
				{
					string iconPath = Path.Join(iconsDir, $"{link.Workshop_File_Id}.png");
					if (ReadWrite.fileExists(iconPath, false, false))
					{
						return ReadWrite.readTextureFromFile(iconPath);
					}
				}
			}

			return null;
		}

		private static Texture2D LoadIcon(LevelInfo levelInfo)
		{
			string iconPath = System.IO.Path.Combine(levelInfo.path, "Icon.png");
			if (ReadWrite.fileExists(iconPath, false, false))
			{
				return ReadWrite.readTextureFromFile(iconPath);
			}

			return null;
		}

		private static Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
	}
}
