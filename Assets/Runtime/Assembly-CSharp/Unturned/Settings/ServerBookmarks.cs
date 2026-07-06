////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// Sort servers by name A to Z.
	/// </summary>
	internal class ServerBookmarkComparer_NameAscending : IComparer<ServerBookmarkDetails>
	{
		public virtual int Compare(ServerBookmarkDetails lhs, ServerBookmarkDetails rhs)
		{
			return lhs.name.CompareTo(rhs.name);
		}
	}

	/// <summary>
	/// Sort servers by name Z to A.
	/// </summary>
	internal class ServerBookmarkComparer_NameDescending : ServerBookmarkComparer_NameAscending
	{
		public override int Compare(ServerBookmarkDetails lhs, ServerBookmarkDetails rhs)
		{
			return -base.Compare(lhs, rhs);
		}
	}

	internal class ServerBookmarkDetails
	{
		/// <summary>
		/// Persistent identifier for server. Relies on server assigning a Game Server Login Token (GSLT).
		/// i.e., servers without GSLT cannot be bookmarked.
		/// </summary>
		public CSteamID steamId;

		/// <summary>
		/// IP address or DNS name to use as-is, or a web address to perform GET request.
		/// Servers not using Fake IP can specify just a DNS entry and a static query port.
		/// Servers using Fake IP are assigned random ports at startup, but can implement a web API endpoint to return
		/// the IP and port.
		///
		/// Nelson 2025-01-20: Making this optional now. The downside is we can't perform a Steam A2S query without
		/// IP/port, but the upside is players can more easily join their non-port-forwarded servers.
		/// </summary>
		public string host;

		/// <summary>
		/// Steam query port. Zero for servers using Fake IP.
		/// </summary>
		public ushort queryPort;

		/// <summary>
		/// Name updated from SteamServerAdvertisement.
		/// </summary>
		public string name;

		/// <summary>
		/// Short description updated from SteamServerAdvertisement.
		/// </summary>
		public string description;

		/// <summary>
		/// Small icon updated from SteamServerAdvertisement.
		/// </summary>
		public string thumbnailUrl;

		/// <summary>
		/// Used by UI to track whether it's been added/removed.
		/// </summary>
		public bool isBookmarked = true;

		public void UpdateFromAdvertisement(SteamServerAdvertisement advertisement)
		{
			// Doesn't update "host" because we need rules response for that.
			if (advertisement.IsAddressUsingSteamFakeIP())
			{
				// Port is randomized on startup when using Fake IP.
				queryPort = 0;
			}
			else
			{
				queryPort = advertisement.queryPort;
			}
			name = advertisement.name;
			description = advertisement.descText;
			thumbnailUrl = advertisement.thumbnailURL;
		}

		public void UpdateFromWorkshopResponse(Provider.CachedWorkshopResponse workshopResponse)
		{
			host = workshopResponse.bookmarkHost;
			name = workshopResponse.serverName;
			description = workshopResponse.serverDescription;
			thumbnailUrl = workshopResponse.thumbnailUrl;
		}

		public override string ToString()
		{
			return $"SteamID: {steamId} Host: \"{host}\" Port: {queryPort} Name: \"{name}\" Description: \"{description}\" Thumbnail URL: \"{thumbnailUrl}\"";
		}
	}

	/// <summary>
	/// Allows player to save server advertisement to join again later. Semi-replacement for Steam's built-in favorites
	/// and history lists because as of 2024-04-26 they don't seem to work properly with Fake IP.
	/// </summary>
	internal class ServerBookmarksManager
	{
		public static ServerBookmarkDetails FindBookmarkDetails(CSteamID steamId)
		{
			if (!steamId.BPersistentGameServerAccount())
			{
				// Cannot be bookmarked because it has no persistent ID.
				return null;
			}

			ServerBookmarksManager manager = Get();
			foreach (ServerBookmarkDetails details in manager.bookmarkDetails)
			{
				if (details.steamId == steamId)
				{
					return details;
				}
			}

			return null;
		}

		/// <returns>details if advertisement is bookmarked.</returns>
		public static ServerBookmarkDetails FindBookmarkDetails(SteamServerAdvertisement advertisement)
		{
			if (!advertisement.steamID.BPersistentGameServerAccount())
			{
				// Cannot be bookmarked because it has no persistent ID.
				return null;
			}

			ServerBookmarksManager manager = Get();
			foreach (ServerBookmarkDetails details in manager.bookmarkDetails)
			{
				if (details.steamId == advertisement.steamID)
				{
					return details;
				}
			}

			return null;
		}

		public static void RemoveBookmark(CSteamID steamId)
		{
			if (!steamId.BPersistentGameServerAccount())
			{
				UnturnedLog.error("Bookmark option should not have been available because server has no ID");
				return;
			}

			ServerBookmarksManager manager = Get();
			for (int index = 0; index < manager.bookmarkDetails.Count; ++index)
			{
				ServerBookmarkDetails details = manager.bookmarkDetails[index];
				if (details.steamId == steamId)
				{
					UnturnedLog.info($"Removed server bookmark ({details})");
					manager.bookmarkDetails.RemoveAt(index);
					manager.isDirty = true;
					return;
				}
			}
		}

		public static ServerBookmarkDetails AddBookmark(SteamServerAdvertisement advertisement, string bookmarkHost)
		{
			if (!advertisement.steamID.BPersistentGameServerAccount())
			{
				UnturnedLog.error("Bookmark option should not have been available because server has no ID");
				return null;
			}

			ServerBookmarksManager manager = Get();
			ServerBookmarkDetails newDetails = new ServerBookmarkDetails();
			newDetails.steamId = advertisement.steamID;
			newDetails.host = bookmarkHost;
			newDetails.UpdateFromAdvertisement(advertisement);
			manager.bookmarkDetails.Add(newDetails);
			manager.isDirty = true;
			UnturnedLog.info($"Added server bookmark ({newDetails})");
			return newDetails;
		}

		public static ServerBookmarkDetails AddBookmark(CSteamID steamId, string bookmarkHost, ushort queryPort,
				string name, string description, string thumbnailUrl)
		{
			if (!steamId.BPersistentGameServerAccount())
			{
				UnturnedLog.error("Bookmark option should not have been available because server has no ID");
				return null;
			}

			ServerBookmarksManager manager = Get();
			ServerBookmarkDetails newDetails = new ServerBookmarkDetails();
			newDetails.steamId = steamId;
			newDetails.host = bookmarkHost;
			newDetails.queryPort = queryPort;
			newDetails.name = name;
			newDetails.description = description;
			newDetails.thumbnailUrl = thumbnailUrl;
			manager.bookmarkDetails.Add(newDetails);
			manager.isDirty = true;
			UnturnedLog.info($"Added server bookmark ({newDetails})");
			return newDetails;
		}

		/// <summary>
		/// Restore a removed bookmark.
		/// </summary>
		public static void AddBookmark(ServerBookmarkDetails details)
		{
			ServerBookmarksManager manager = Get();
			manager.bookmarkDetails.Add(details);
			manager.isDirty = true;
			UnturnedLog.info($"Added server bookmark ({details})");
		}

		public static List<ServerBookmarkDetails> GetList()
		{
			return Get().bookmarkDetails;
		}

		public static void SaveIfDirty()
		{
			if (instance == null)
			{
				// Perhaps nobody ever called load, in which case do not clobber existing file.
				UnturnedLog.info("Skipped saving server bookmarks");
				return;
			}

			if (instance.isDirty)
			{
				instance.isDirty = false;
				instance.Save();
				UnturnedLog.info("Saved server bookmarks");
			}
		}

		public static void MarkDirty()
		{
			if (instance != null)
			{
				instance.isDirty = true;
			}
		}

		private static ServerBookmarksManager Get()
		{
			if (instance == null)
			{
				Load();
			}

			return instance;
		}

		private static void Load()
		{
			if (ReadWrite.fileExists(RELATIVE_PATH, false, true))
			{
				try
				{
					instance = new ServerBookmarksManager();

					River river = new River(RELATIVE_PATH, true, false, true);
					byte version = river.readByte();

					int bookmarkCount = river.readInt32();
					instance.bookmarkDetails = new List<ServerBookmarkDetails>();
					for (int index = 0; index < bookmarkCount; ++index)
					{
						ServerBookmarkDetails details = new ServerBookmarkDetails();
						details.steamId = river.readSteamID();
						details.host = river.readString();
						details.queryPort = river.readUInt16();
						details.name = river.readString();
						details.description = river.readString();
						details.thumbnailUrl = river.readString();

						if (!details.steamId.BPersistentGameServerAccount())
						{
							UnturnedLog.info($"Discarding server bookmark for \"{details.name}\" at {details.host}:{details.queryPort} because Steam ID ({details.steamId}) is invalid");
							continue;
						}

						instance.bookmarkDetails.Add(details);
					}

					UnturnedLog.info($"Loaded server bookmarks: {bookmarkCount}");
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, "Caught exception loading server bookmarks:");
					instance = new ServerBookmarksManager();
					instance.Reset();
				}
			}
			else
			{
				instance = new ServerBookmarksManager();
				instance.Reset();
			}
		}

		private void Save()
		{
			// Catch exception because if IO fails (e.g. if user marked file read-only) we do not want to break. 
			try
			{
				River river = new River(RELATIVE_PATH, true, false, false);
				river.writeByte(1); // Version

				river.writeInt32(bookmarkDetails.Count);
				foreach (ServerBookmarkDetails details in bookmarkDetails)
				{
					river.writeSteamID(details.steamId);
					river.writeString(details.host);
					river.writeUInt16(details.queryPort);
					river.writeString(details.name);
					river.writeString(details.description);
					river.writeString(details.thumbnailUrl);
				}

				river.closeRiver();
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception saving server bookmarks:");
			}
		}

		private void Reset()
		{
			bookmarkDetails = new List<ServerBookmarkDetails>();
		}

		private List<ServerBookmarkDetails> bookmarkDetails;
		private bool isDirty;

		private static ServerBookmarksManager instance;
		private const string RELATIVE_PATH = "/Cloud/ServerBookmarks.bin";
	}
}
#endif // !DEDICATED_SERVER
