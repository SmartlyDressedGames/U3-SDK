////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Matches level with same file name AND workshop file ID.
	/// </summary>
	internal struct SavedLevelSelection
	{
		public string name;
		public ulong workshopFileId;

		public void Read(Block block)
		{
			name = block.readString();
			workshopFileId = block.readUInt64();
		}

		public void Write(Block block)
		{
			block.writeString(name);
			block.writeUInt64(workshopFileId);
		}

		public void Clear()
		{
			name = string.Empty;
			workshopFileId = 0;
		}

		public SavedLevelSelection(LevelInfo level)
		{
			if (level != null)
			{
				name = level.name;
				workshopFileId = level.publishedFileId;
			}
			else
			{
				name = string.Empty;
				workshopFileId = 0;
			}
		}
	}

	public class PlaySettings
	{
		/// <summary>
		/// Version before named version constants were introduced. (2023-11-08)
		/// </summary>
		public const byte SAVEDATA_VERSION_INITIAL = 11;
		public const byte SAVEDATA_VERSION_REMOVED_MATCHMAKING = 12;
		/// <summary>
		/// Moved into ServerListFilters.
		/// </summary>
		public const byte SAVEDATA_VERSION_REMOVED_SERVER_NAME_FILTER = 13;
		public const byte SAVEDATA_VERSION_PERSIST_LEVEL_WORKSHOP_FILE_ID = 14;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_PERSIST_LEVEL_WORKSHOP_FILE_ID;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		public static string connectHost;
		public static ushort connectPort;

		public static string connectPassword;
		public static string serversPassword;

		public static EGameMode singleplayerMode;
		public static bool singleplayerCheats;
		[System.Obsolete]
		public static string singleplayerMap;
		[System.Obsolete]
		public static string editorMap;
		internal static SavedLevelSelection singleplayerLevelSelection;
		internal static SavedLevelSelection editorLevelSelection;
		public static bool isVR;
		public static ESingleplayerMapCategory singleplayerCategory;

		public static void load()
		{
			if (ReadWrite.fileExists("/Play.dat", true))
			{
				Block block = ReadWrite.readBlock("/Play.dat", true, 0);

				if (block != null)
				{
					byte version = block.readByte();

					if (version > 1)
					{
						connectHost = block.readString();
						connectPort = block.readUInt16();

						connectPassword = block.readString();

						if (version > 3 && version < SAVEDATA_VERSION_REMOVED_SERVER_NAME_FILTER)
						{
							// server name
							block.readString();
						}

						serversPassword = block.readString();

						singleplayerMode = (EGameMode) block.readByte();
						if (version < 8)
						{
							singleplayerMode = EGameMode.NORMAL;
						}

						if (version > 10 && version < SAVEDATA_VERSION_REMOVED_MATCHMAKING)
						{
							// matchmaking mode
							block.readByte();
						}

						if (version < 7)
						{
							singleplayerCheats = false;
						}
						else
						{
							singleplayerCheats = block.readBoolean();
						}

#pragma warning disable
						if (version > 4 && version < SAVEDATA_VERSION_PERSIST_LEVEL_WORKSHOP_FILE_ID)
						{
							singleplayerMap = block.readString();
							editorMap = block.readString();
						}
						else
						{
							singleplayerMap = "";
							editorMap = "";
						}
#pragma warning restore

						if (version > 10 && version < SAVEDATA_VERSION_REMOVED_MATCHMAKING)
						{
							// matchmaking map
							block.readString();
						}

						if (version > 5)
						{
							isVR = block.readBoolean();

							if (version < 9)
							{
								isVR = false;
							}
						}
						else
						{
							isVR = false;
						}

						if (version < 10)
						{
							singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
						}
						else
						{
							singleplayerCategory = (ESingleplayerMapCategory) block.readByte();
						}

						if (version >= SAVEDATA_VERSION_PERSIST_LEVEL_WORKSHOP_FILE_ID)
						{
							singleplayerLevelSelection.Read(block);
							editorLevelSelection.Read(block);
						}
						else
						{
#pragma warning disable
							singleplayerLevelSelection.name = singleplayerMap;
							editorLevelSelection.name = editorMap;
#pragma warning restore
						}

#if CLOUDDEBUG
						UnturnedLog.info("Play: " + connectIP + " " + connectPort + " " + connectPassword + " " + serversPassword);
#endif

						return;
					}
				}
			}

			connectHost = "127.0.0.1";
			connectPort = 27015;

			connectPassword = "";
			serversPassword = string.Empty;

			singleplayerMode = EGameMode.NORMAL;
			singleplayerCheats = false;

#pragma warning disable
			singleplayerMap = "";
			editorMap = "";
#pragma warning restore
			singleplayerLevelSelection.name = string.Empty;
			editorLevelSelection.name = string.Empty;

			singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION_NEWEST);

			block.writeString(connectHost);
			block.writeUInt16(connectPort);

			block.writeString(connectPassword);
			block.writeString(serversPassword);

			block.writeByte((byte) singleplayerMode);
			block.writeBoolean(singleplayerCheats);

			block.writeBoolean(isVR);

			block.writeByte((byte) singleplayerCategory);

			singleplayerLevelSelection.Write(block);
			editorLevelSelection.Write(block);

			ReadWrite.writeBlock("/Play.dat", true, block);
		}
	}
}
