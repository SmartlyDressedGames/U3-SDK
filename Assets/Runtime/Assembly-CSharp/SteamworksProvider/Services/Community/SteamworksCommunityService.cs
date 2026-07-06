////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Community;
using SDG.Unturned; // for steamgroup classes (temp)
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.SteamworksProvider.Services.Community
{
	public class SteamworksCommunityService : Service, ICommunityService
	{
		private Dictionary<CSteamID, SteamGroup> cachedGroups;
		private Dictionary<CSteamID, Texture2D> cachedAvatars;

		public void setStatus(string status)
		{
			SteamFriends.SetRichPresence("status", status);
		}

		public Texture2D getIcon(int id)
		{
			if (id < 0)
				return null;

			uint rawWidth;
			uint rawHeight;

			if (SteamUtils.GetImageSize(id, out rawWidth, out rawHeight) == false)
				return null;

			// Nelson 2024-12-04: Adding this check because I received a log file with a weird exception here:
			// > No texture data provided to LoadRawTextureData
			// > Texture has out of range width / height
			// > UnityException: Failed to create texture because of invalid parameters.
			if (rawWidth < 1 || rawHeight < 1 || rawWidth > 2048 || rawHeight > 2048)
				return null;

			byte[] rawData = new byte[rawWidth * rawHeight * 4];
			if (SteamUtils.GetImageRGBA(id, rawData, rawData.Length) == false)
				return null;

			int width = (int) rawWidth;
			int height = (int) rawHeight;

			int numRowsToFlip = height / 2; // Rounded down e.g. 7 row image does not need middle row flipped.
			for (int sourceRow = 0; sourceRow < numRowsToFlip; ++sourceRow)
			{
				int destinationRow = height - 1 - sourceRow;

				// Indices into rawData:
				int sourceRowIndex = sourceRow * width * 4;
				int destinationRowIndex = destinationRow * width * 4;

				for (int column = 0; column < width; ++column)
				{
					int sourceColumnIndex = sourceRowIndex + (column * 4);
					int destinationColumnIndex = destinationRowIndex + (column * 4);

					for (int channel = 0; channel < 4; ++channel)
					{
						int sourceIndex = sourceColumnIndex + channel;
						int destinationIndex = destinationColumnIndex + channel;

						// Swap value between source and destination:
						byte tempValue = rawData[sourceIndex];
						rawData[sourceIndex] = rawData[destinationIndex];
						rawData[destinationIndex] = tempValue;
					}
				}
			}

			Texture2D icon;
			try
			{
				icon = new Texture2D(width, height, TextureFormat.RGBA32, false);
				icon.hideFlags = HideFlags.HideAndDontSave;
				icon.LoadRawTextureData(rawData);
				const bool updateMipMaps = true;
				const bool makeNonReadableOnCPU = true;
				icon.Apply(updateMipMaps, makeNonReadableOnCPU);
			}
			catch (System.Exception exception)
			{
				icon = null;
				UnturnedLog.exception(exception, $"Caught exception creating Steam avatar {rawWidth}x{rawHeight}:");
			}

			/*
			Texture2D flippedIcon = new Texture2D(width, height, TextureFormat.RGBA32, false);
			flippedIcon.hideFlags = HideFlags.HideAndDontSave;
			for(int row = 0; row < height; ++row)
			{
				flippedIcon.SetPixels(0, row, width, 1, temporaryIcon.GetPixels(0, height - 1 - row, width, 1));
			}

			const bool updateMipMaps = true;
			const bool makeNonReadableOnCPU = true;
			flippedIcon.Apply(updateMipMaps, makeNonReadableOnCPU);
			*/

			return icon;
		}

		public Texture2D getIcon(CSteamID steamID, bool shouldCache = false)
		{
			Texture2D icon = null;

			if (!shouldCache || !cachedAvatars.TryGetValue(steamID, out icon))
			{
				icon = getIcon(SteamFriends.GetSmallFriendAvatar(steamID));
				if (shouldCache)
				{
					cachedAvatars.Add(steamID, icon);
				}
			}

			return icon;
		}

		public SteamGroup getCachedGroup(CSteamID steamID)
		{
			SteamGroup group;
			cachedGroups.TryGetValue(steamID, out group);

			return group;
		}

		public SteamGroup[] getGroups()
		{
			SteamGroup[] groups = new SteamGroup[SteamFriends.GetClanCount()];

			for (int index = 0; index < groups.Length; index++)
			{
				CSteamID steamID = SteamFriends.GetClanByIndex(index);

				SteamGroup group = getCachedGroup(steamID);
				if (group == null)
				{
					string name = SteamFriends.GetClanName(steamID);
					Texture2D icon = getIcon(steamID);
					group = new SteamGroup(steamID, name, icon);
					cachedGroups.Add(steamID, group);
				}

				groups[index] = group;
			}

			return groups;
		}

		public bool checkGroup(CSteamID steamID)
		{
			for (int index = 0; index < SteamFriends.GetClanCount(); index++)
			{
				if (SteamFriends.GetClanByIndex(index) == steamID)
				{
					return true;
				}
			}

			return false;
		}

		public SteamworksCommunityService()
		{
			cachedGroups = new Dictionary<CSteamID, SteamGroup>();
			cachedAvatars = new Dictionary<CSteamID, Texture2D>();
		}
	}
}
