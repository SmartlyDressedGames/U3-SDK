////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	/// <summary>
	/// Utilities for calling workshop functions without worrying about client/server.
	/// This could be nicely refactored into a client and server interface, but not enough time for that right now.
	/// </summary>
	public class WorkshopUtils
	{
		/// <summary>
		/// Client/server safe version of GetQueryUGCNumKeyValueTags.
		/// </summary>
		public static uint getQueryUGCNumKeyValueTags(UGCQueryHandle_t queryHandle, uint resultIndex)
		{
			if (Dedicator.IsDedicatedServer)
				return SteamGameServerUGC.GetQueryUGCNumKeyValueTags(queryHandle, resultIndex);
			else
				return SteamUGC.GetQueryUGCNumKeyValueTags(queryHandle, resultIndex);
		}

		/// <summary>
		/// Client/server safe version of GetQueryUGCKeyValueTag.
		/// </summary>
		public static bool getQueryUGCKeyValueTag(UGCQueryHandle_t queryHandle, uint resultIndex, uint tagIndex, out string key, out string value)
		{
			if (Dedicator.IsDedicatedServer)
				return SteamGameServerUGC.GetQueryUGCKeyValueTag(queryHandle, resultIndex, tagIndex, out key, 255, out value, 255);
			else
				return SteamUGC.GetQueryUGCKeyValueTag(queryHandle, resultIndex, tagIndex, out key, 255, out value, 255);
		}

		/// <summary>
		/// Search for the value associated with a given key.
		/// </summary>
		public static bool findQueryUGCKeyValue(UGCQueryHandle_t queryHandle, uint resultIndex, string key, out string value)
		{
			uint numKeyValueTags = getQueryUGCNumKeyValueTags(queryHandle, resultIndex);
			for (uint tagIndex = 0; tagIndex < numKeyValueTags; ++tagIndex)
			{
				string thisKey;
				string thisValue;
				if (getQueryUGCKeyValueTag(queryHandle, resultIndex, tagIndex, out thisKey, out thisValue))
				{
					if (thisKey.Equals(key, System.StringComparison.InvariantCultureIgnoreCase))
					{
						value = thisValue;
						return true;
					}
				}
			}

			value = null;
			return false;
		}

		/// <summary>
		///Client/server safe version of GetQueryUGCResult.
		/// </summary>
		public static bool getQueryUGCResult(UGCQueryHandle_t queryHandle, uint resultIndex, out SteamUGCDetails_t details)
		{
			if (Dedicator.IsDedicatedServer)
				return SteamGameServerUGC.GetQueryUGCResult(queryHandle, resultIndex, out details);
			else
				return SteamUGC.GetQueryUGCResult(queryHandle, resultIndex, out details);
		}

		/// <summary>
		/// Is file banned?
		/// </summary>
		public static bool getQueryUGCBanned(UGCQueryHandle_t queryHandle, uint resultIndex)
		{
			SteamUGCDetails_t details;
			bool gotResult = getQueryUGCResult(queryHandle, resultIndex, out details);
			return gotResult && details.m_bBanned;
		}
	}
}
