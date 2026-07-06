////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned
{
	/// <summary>
	/// Map creator can specify a constant weather mode.
	/// </summary>
	public enum ELevelWeatherOverride
	{
		NONE,
		RAIN,
		SNOW,
	}

	/// <summary>
	/// Associates a train vehicle ID with the index of a road path to spawn it on.
	/// The level only spawns the train if this vehicle ID isn't present in the map yet, so every train on the map has to be different.
	/// </summary>
	public class LevelTrainAssociation
	{
		public ushort VehicleID;
		public ushort RoadIndex;
		public float Min_Spawn_Placement = 0.1f;
		public float Max_Spawn_Placement = 0.9f;
	}

	public class LevelInfoConfigData
	{
		public string[] Creators;
		public string[] Collaborators;
		public string[] Thanks;
		public Dictionary<string, string[]> CustomCredits;
		public int Item;
		public int[] Associated_Stockpile_Items;
		public string Feedback;
		public AssetReference<LevelAsset> Asset;
		public List<LevelTrainAssociation> Trains;
		public Dictionary<string, object> Mode_Config_Overrides;
		public Dictionary<string, object> EasyDifficulty_Config_Overrides;
		public Dictionary<string, object> NormalDifficulty_Config_Overrides;
		public Dictionary<string, object> HardDifficulty_Config_Overrides;

		public bool Allow_Underwater_Features;
		public bool Terrain_Snow_Sparkle;
		public bool Use_Legacy_Clip_Borders;
		public bool Use_Legacy_Ground;
		public bool Use_Legacy_Water;

		/// <summary>
		/// Should underwater bubble particles be activated?
		/// </summary>
		public bool Use_Vanilla_Bubbles;

		/// <summary>
		/// Should positions underground be clamped above ground?
		/// Underground volumes are used to whitelist valid positions.
		/// </summary>
		public bool Use_Underground_Whitelist;

		public bool Use_Legacy_Snow_Height;
		public bool Use_Legacy_Fog_Height;
		public bool Use_Legacy_Oxygen_Height;
		public bool Use_Rain_Volumes;
		public bool Use_Snow_Volumes;
		public bool Is_Aurora_Borealis_Visible;
		public bool Snow_Affects_Temperature;
		public ELevelWeatherOverride Weather_Override;
		public bool Has_Atmosphere;
		public bool Allow_Crafting;
		public bool Allow_Skills;
		public bool Allow_Information;

		/// <summary>
		/// If true, certain objects redirect to load others in-game.
		/// </summary>
		public bool Allow_Holiday_Redirects;

		/// <summary>
		/// If true, electric objects are always powered, and generators have no effect.
		/// </summary>
		public bool Has_Global_Electricity;

		public float Gravity;
		public float Blimp_Altitude;
		public float Max_Walkable_Slope;
		public float Prevent_Building_Near_Spawnpoint_Radius;

		public ESingleplayerMapCategory Category;

		public bool PlayerUI_HealthVisible = true;
		public bool PlayerUI_FoodVisible = true;
		public bool PlayerUI_WaterVisible = true;
		public bool PlayerUI_VirusVisible = true;
		public bool PlayerUI_StaminaVisible = true;
		public bool PlayerUI_OxygenVisible = true;
		public bool PlayerUI_GunVisible = true;

		/// <summary>
		/// Display version in the format "a.b.c.d".
		/// </summary>
		public string Version;

		/// <summary>
		/// Version string packed into integer.
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		public uint PackedVersion;

		/// <summary>
		/// Number of custom tips defined in per-level localization file.
		/// Tip keys are read as Tip_#
		/// </summary>
		public int Tips;

		/// <summary>
		/// LevelBatching is currently only enabled if map creator has verified it works properly.
		/// </summary>
		public int Batching_Version;

		/// <summary>
		/// Overrides maximum size of textures included in LevelBatching atlas.
		/// When using this, be mindful the combined texture doesn't exceed some reasonable size (~4k?)
		/// </summary>
		public int Batching_Max_Texture_Size = 128;

		/// <summary>
		/// If true, map creator has verified the clutter option works as-expected.
		/// </summary>
		public bool Enable_Clutter_Option;

		/// <summary>
		/// If true, map creator has verified that volumes are ONLY placed in the level editor (not Unity prefabs).
		/// </summary>
		public bool Enable_Static_Volumes;

		public bool Use_Arena_Compactor;
		public List<ArenaLoadout> Arena_Loadouts;
		public List<ArenaLoadout> Spawn_Loadouts;

		public ulong[] RequiredWorkshopFileIds;

		[Newtonsoft.Json.JsonIgnore]
		public byte[] Hash;

		/// <summary>
		/// Can be null if not configured.
		/// </summary>
		public Dictionary<string, object> GetPerDifficultyConfigOverrides(EGameMode mode)
		{
			switch (mode)
			{
				case EGameMode.EASY:
					return EasyDifficulty_Config_Overrides;

				case EGameMode.NORMAL:
					return NormalDifficulty_Config_Overrides;

				case EGameMode.HARD:
					return HardDifficulty_Config_Overrides;

				default:
					return null;
			}
		}

		public LevelInfoConfigData()
		{
			Creators = new string[0];
			Collaborators = new string[0];
			Thanks = new string[0];
			CustomCredits = new Dictionary<string, string[]>();
			Item = 0;
			Associated_Stockpile_Items = new int[0];
			Feedback = null;
			Asset = AssetReference<LevelAsset>.invalid;
			Trains = new List<LevelTrainAssociation>();
			Mode_Config_Overrides = new Dictionary<string, object>();

			Allow_Underwater_Features = false;
			Terrain_Snow_Sparkle = false;
			Use_Legacy_Clip_Borders = true;
			Use_Legacy_Ground = true;
			Use_Legacy_Water = true;
			Use_Vanilla_Bubbles = true;
			Use_Legacy_Snow_Height = true;
			Use_Legacy_Oxygen_Height = true;
			Use_Rain_Volumes = false;
			Use_Snow_Volumes = false;
			Is_Aurora_Borealis_Visible = false;
			Snow_Affects_Temperature = true;
			Has_Atmosphere = true;
			Allow_Crafting = true;
			Allow_Skills = true;
			Allow_Information = true;
			Gravity = -9.81f;
			Blimp_Altitude = 150.0f;
			Max_Walkable_Slope = -1;
			Prevent_Building_Near_Spawnpoint_Radius = 16;

			Category = ESingleplayerMapCategory.MISC;

			Use_Arena_Compactor = true;
			Arena_Loadouts = new List<ArenaLoadout>();
			Spawn_Loadouts = new List<ArenaLoadout>();
			RequiredWorkshopFileIds = new ulong[0];

			Version = "3.0.0.0";
		}
	}

	public class LevelInfo
	{
		/// <summary>
		/// Absolute path to the map folder.
		/// </summary>
		public string path
		{
			get;
			protected set;
		}

		private string _name;
		public string name => _name;

		/// <summary>
		/// Only used for play menu categories at the moment.
		/// </summary>
		public bool isFromWorkshop
		{
			get;
			protected set;
		}

		public ulong publishedFileId
		{
			get;
			protected set;
		}

		/// <summary>
		/// SHA1 hash of the Level.dat file.
		/// </summary>
		public byte[] hash
		{
			get;
			protected set;
		}

		/// <summary>
		/// Test whether this map's workshop file ID is in the curated maps list.
		/// </summary>
		public bool isCurated
		{
			get
			{
				if (isFromWorkshop)
				{
					foreach (CuratedMapLink link in Provider.statusData.Maps.Curated_Map_Links)
					{
						if (link.Workshop_File_Id == publishedFileId)
						{
							return true;
						}
					}

					return false;
				}
				else
				{
					return name == "France" || name == "Canyon Arena"; // Hardcoded until moved to workshop.
				}
			}
		}

		/// <summary>
		/// Web URL to map feedback discussions.
		/// </summary>
		public string feedbackUrl
		{
			get
			{
				if (configData != null && string.IsNullOrEmpty(configData.Feedback) == false)
				{
					// Explicitly configured feedback URL.
					return configData.Feedback;
				}
				else if (isFromWorkshop)
				{
					return "https://steamcommunity.com/sharedfiles/filedetails/discussions/" + publishedFileId;
				}
				else
				{
					return null;
				}
			}
		}

		private ELevelSize _size;
		public ELevelSize size => _size;

		private ELevelType _type;
		public ELevelType type => _type;

		private bool _isEditable;
		public bool isEditable => _isEditable;

		public LevelInfoConfigData configData
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, this info is out-of-date and may have been renamed or deleted.
		/// </summary>
		public bool WasRemovedFromKnownLevels
		{
			get;
			internal set;
		}

		private Local cachedLocalization = null;
		public Local getLocalization()
		{
			if (cachedLocalization == null)
			{
				string mapLanguagePath = path + "/" + Provider.language + ".dat";
				if (ReadWrite.fileExists(mapLanguagePath, false, false))
				{
					cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(mapLanguagePath));
				}
				else
				{
					string customPath = Provider.localizationRoot + "/Maps/" + name + ".dat";
					if (ReadWrite.fileExists(customPath, false, false))
					{
						cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(customPath));
					}
					else
					{
						string altCustomPath = Provider.localizationRoot + "/Maps/" + name.Replace(' ', '_') + ".dat";
						if (ReadWrite.fileExists(altCustomPath, false, false))
						{
							cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(altCustomPath));
						}
					}
				}

				if (cachedLocalization == null)
				{
					string fallbackPath = path + "/English.dat";
					if (ReadWrite.fileExists(fallbackPath, false, false))
					{
						cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(fallbackPath));
					}
					else
					{
						cachedLocalization = new Local();
					}
				}
			}

			return cachedLocalization;
		}

		public string getLocalizedName()
		{
			Local localization = getLocalization();
			if (localization != null && localization.has("Name"))
			{
				return localization.format("Name");
			}
			else
			{
				return name;
			}
		}

		/// <summary>
		/// Preview.png should be 320x180
		/// </summary>
		public string GetPreviewImageFilePath()
		{
			string previewPath = Path.Combine(path, "Preview.png");
			if (File.Exists(previewPath))
			{
				return previewPath;
			}

			// Fallback to loading screen image
			return GetLoadingScreenImagePath();
		}

		/// <summary>
		/// Get a random file path in the /Screenshots folder, or fallback to Level.png if it exists.
		/// </summary>
		public string GetLoadingScreenImagePath()
		{
			string screenshotPath = GetRandomScreenshotPath();
			if (!string.IsNullOrEmpty(screenshotPath))
			{
				return screenshotPath;
			}

			// Fallback to legacy loading screen image
			string singleLoadingPath = Path.Combine(path, "Level.png");
			if (File.Exists(singleLoadingPath))
			{
				return singleLoadingPath;
			}

			return null;
		}

		/// <summary>
		/// Get a random file path in the /Screenshots folder
		/// </summary>
		internal string GetRandomScreenshotPath()
		{
			string screenshotsPath = Path.Combine(path, "Screenshots");
			if (!Directory.Exists(screenshotsPath))
			{
				return null;
			}

			return LoadingUI.GetRandomImagePathInDirectory(screenshotsPath, false);
		}

		/// <summary>
		/// If dependency workshop file(s) are configured, check those.
		/// Otherwise, always returns false.
		/// </summary>
		public bool IsMissingAnyDependencies()
		{
			if (configData == null || configData.RequiredWorkshopFileIds == null || configData.RequiredWorkshopFileIds.Length < 1)
			{
				return false;
			}

			foreach (ulong workshopFileId in configData.RequiredWorkshopFileIds)
			{
				AssetOrigin origin = Assets.FindWorkshopFileOrigin(workshopFileId);
				if (origin == null || origin.assets == null || origin.assets.IsEmpty())
				{
					return true;
				}
			}

			return false;
		}

		public LevelInfo(string newPath, string newName, ELevelSize newSize, ELevelType newType, bool newEditable, LevelInfoConfigData newConfigData, ulong publishedFileId, byte[] hash)
		{
			path = newPath;
			_name = newName;
			_size = newSize;
			_type = newType;

			_isEditable = newEditable;
#if UNITY_EDITOR
			_isEditable = true;
#endif // UNITY_EDITOR
			configData = newConfigData;

			isFromWorkshop = publishedFileId > 0;
			this.publishedFileId = publishedFileId;
			this.hash = hash;
		}

		[System.Obsolete("Please use Level.getAsset instead. LevelInfo persists between loads now. (public issue #4273)")]
		public LevelAsset resolveAsset()
		{
			LevelAsset levelAsset = null;

			if (configData != null && configData.Asset.isValid)
			{
				levelAsset = Assets.find(configData.Asset);
			}

			if (levelAsset == null)
			{
				levelAsset = Assets.find(LevelAsset.defaultLevel);
			}

			return levelAsset;
		}
	}
}
