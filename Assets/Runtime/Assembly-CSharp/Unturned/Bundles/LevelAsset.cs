////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum EZombieDifficultyAssetPrioritization
	{
		/// <summary>
		/// Default. Per-navmesh difficulty asset takes priority over per-table/type difficulty asset.
		/// If per-navmesh asset is null the per-table asset is the fallback.
		/// </summary>
		NavmeshOverridesTable,

		/// <summary>
		/// Per-table/type difficulty asset takes priority over per-navmesh difficulty asset.
		/// If per-table asset is null the per-navmesh asset is the fallback.
		/// </summary>
		TableOverridesNavmesh,
	}

	public class LevelAsset : Asset
	{
		public static AssetReference<LevelAsset> defaultLevel = new AssetReference<LevelAsset>(new System.Guid("12dc9fdbe9974022afd21158ad54b76a"));
		internal static MasterBundleReference<AudioClip> DefaultDeathMusicRef = new MasterBundleReference<AudioClip>("core.masterbundle", "Music/Death.mp3");

		public TypeReference<GameMode> defaultGameMode;


		public List<TypeReference<GameMode>> supportedGameModes;

		public MasterBundleReference<GameObject> dropshipPrefab;
		public AssetReference<AirdropAsset> airdropRef;

		public const float DEFAULT_UNDERWATER_FOG_DENSITY = 0.075f;
		/// <summary>
		/// Intensity of fog effect while camera is inside a water volume.
		/// Defaults to 0.075.
		/// </summary>
		public float UnderwaterFogDensity
		{
			get;
			set;
		} = DEFAULT_UNDERWATER_FOG_DENSITY;

		/// <summary>
		/// Player stealth radius cannot go below this value.
		/// </summary>
		public float minStealthRadius;

		/// <summary>
		/// Deal damage and break legs if speed is greater than this value.
		/// </summary>
		public float fallDamageSpeedThreshold;

		/// <summary>
		/// By default players in singleplayer and admins in multiplayer have a faster salvage time.
		/// This option was requested for maps with entirely custom balanced salvage times.
		/// </summary>
		public bool enableAdminFasterSalvageDuration = true;

		public List<AssetReference<CraftingBlacklistAsset>> craftingBlacklists;

		/// <summary>
		/// Cached result of finding all craftingBlacklists.
		/// </summary>
		private List<CraftingBlacklistAsset> resolvedCraftingBlacklists;

		public struct SchedulableWeather
		{
			public AssetReference<WeatherAssetBase> assetRef;
			public float minFrequency;
			public float maxFrequency;
			public float minDuration;
			public float maxDuration;
		}

		/// <summary>
		/// Determines which weather can naturally occur in this level.
		/// Null if empty.
		/// </summary>
		public SchedulableWeather[] schedulableWeathers;

		/// <summary>
		/// If set, this weather will always be active and scheduled weather is disabled.
		/// </summary>
		public AssetReference<WeatherAssetBase> perpetualWeatherRef;

		public struct LoadingScreenMusic
		{
			public MasterBundleReference<AudioClip> loopRef;
			public MasterBundleReference<AudioClip> outroRef;
			public float loopVolume;
			public float outroVolume;
		}

		public LoadingScreenMusic[] loadingScreenMusic;

		/// <summary>
		/// Audio clip to play in 2D when a player dies.
		/// </summary>
		public MasterBundleReference<AudioClip> DeathMusicRef
		{
			get;
			private set;
		}

		/// <summary>
		/// Defaults to false because some servers have rules and info on the loading screen.
		/// </summary>
		public bool shouldAnimateBackgroundImage;

		/// <summary>
		/// Volume weather mask used while not inside an ambience volume.
		/// </summary>
		public uint globalWeatherMask;

		public class SkillRule
		{
			public int defaultLevel;
			public int maxUnlockableLevel;
			public float costMultiplier;

			/// <summary>
			/// If >= 0, overrides vanilla skill cost.
			/// Defaults to -1.
			/// </summary>
			public int baseCostOverride;

			/// <summary>
			/// If >= 0, overrides vanilla increase in skill cost with each level.
			/// For example, if the base cost is 10 and this is 15, the first level will cost 10 XP,
			/// the second level 25 XP, the third 40 XP, so on and so forth.
			/// Defaults to -1.
			/// </summary>
			public int perLevelCostIncreaseOverride;
		}

		/// <summary>
		/// Allows level to override skill max levels.
		/// Can be turned off with config Prevent_Level_Skill_Overrides true.
		/// Null if empty, otherwise matches 1:1 with PlayerSkills._skills.
		/// </summary>
		public SkillRule[][] skillRules;

		/// <summary>
		/// If false, clouds are removed from the skybox.
		/// </summary>
		public bool hasClouds = true;

		/// <summary>
		/// If set, instantiate this particle system and set its material color to cloud color.
		/// </summary>
		public MasterBundleReference<GameObject> CloudOverridePrefab
		{
			get;
			set;
		}

		public struct CloudOverrideParticleSystemsPath : IDatParseable
		{
			public string ComponentPath
			{
				get;
				set;
			}

			/// <summary>
			/// Multiplier for CloudOverrideParticlesPrefab emission rate according to level's clouds intensity.
			/// </summary>
			public float RateOverTimeScale
			{
				get;
				set;
			}

			/// <summary>
			/// Particle system's material instance will have these color properties set to the level's cloud color.
			/// Defaults to _Color.
			/// </summary>
			public string[] MaterialColorPropertyNames
			{
				get;
				set;
			}

			/// <summary>
			/// t passed into ParticleSystem.Simulate when clouds need an update.
			/// </summary>
			public float WarmupTime;

			public bool TryParse(IDatNode node)
			{
				if (node is IDatDictionary dict)
				{
					ComponentPath = dict.GetString("Path");
					RateOverTimeScale = dict.ParseFloat("RateOverTimeScale");
					if (dict.TryGetList("MaterialColorPropertyNames", out IDatList list))
					{
						List<string> tempNames = new List<string>();
						foreach (IDatValue value in list)
						{
							if (!value.IsValueNullOrEmpty())
							{
								tempNames.Add(value.Value);
							}
						}
						MaterialColorPropertyNames = tempNames.ToArray();
					}
					else
					{
						MaterialColorPropertyNames = new string[] { "_Color" };
					}
					WarmupTime = dict.ParseFloat("WarmupTime");
					return true;
				}

				return false;
			}
		}

		public CloudOverrideParticleSystemsPath[] CloudOverrideParticleSystemPaths
		{
			get;
			set;
		}

		public struct DefaultLoadoutItem : IDatParseable
		{
			public CachingBcAssetRef assetRef;
			public int amount;
			public EItemOrigin origin;

			public ItemAsset ResolveAsset(System.Func<string> errorContextCallback)
			{
				Asset asset = assetRef.Get();
				if (asset == null)
				{
					UnturnedLog.warn($"{errorContextCallback?.Invoke() ?? "Unknown"} unable to find asset {assetRef}");
					return null;
				}

				if (asset is SpawnAsset spawnAsset)
				{
					asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.ITEM, errorContextCallback);
					if (asset == null)
					{
						// Allow spawn table to not spawn something.
						return null;
					}
				}

				ItemAsset itemAsset = asset as ItemAsset;
				if (itemAsset == null)
				{
					UnturnedLog.warn($"{errorContextCallback?.Invoke() ?? "Unknown"} tried to spawn non-item asset {asset.FriendlyNameWithFriendlyType}");
					return null;
				}

				return itemAsset;
			}

			public bool TryParse(IDatNode node)
			{
				if (node is IDatDictionary dict)
				{
					bool success = dict.TryParseBcAssetRef("Asset", EAssetType.ITEM, out assetRef);
					amount = dict.ParseInt32("Amount", 1);
					origin = dict.ParseEnum("Origin", EItemOrigin.WORLD);
					return success;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// If set, overrides the per-skillset items players spawn with.
		/// Can be used to prevent skillset default items in singleplayer.
		/// Server "Loadout" command takes priority over this option.
		/// Defaults to null.
		/// </summary>
		public DefaultLoadoutItem[][] DefaultSkillsetLoadouts
		{
			get;
			set;
		}

		public bool HasSkillsetLoadoutsOverride
		{
			get => DefaultSkillsetLoadouts != null;
		}

		public DefaultLoadoutItem[] GetSkillsetLoadoutOrNull(EPlayerSkillset skillset)
		{
			return DefaultSkillsetLoadouts != null ? DefaultSkillsetLoadouts[(int) skillset] : null;
		}

		internal class TerrainColorRule : IDatParseable
		{
			public float ruleHue;
			public float ruleSaturation;
			public float ruleValue;
			public float hueThreshold;
			public float saturationThreshold;
			public float valueThreshold;

			public enum EComparisonResult
			{
				TooSimilar,
				OutsideHueThreshold,
				OutsideSaturationThreshold,
				OutsideValueThreshold,
			}

			public EComparisonResult CompareColors(float inputHue, float inputSaturation, float inputValue)
			{
				// Hue loops around, so we need to consider case where hues are near ends, e.g., 0.1 and 0.9:
				float distance;
				float loopDistance;
				if (inputHue < ruleHue)
				{
					distance = ruleHue - inputHue;
					loopDistance = inputHue + 1.0f - ruleHue;
				}
				else
				{
					distance = inputHue - ruleHue;
					loopDistance = ruleHue + 1.0f - inputHue;
				}
				if (distance > hueThreshold && loopDistance > hueThreshold)
				{
					return EComparisonResult.OutsideHueThreshold;
				}

				if (Mathf.Abs(inputSaturation - ruleSaturation) > saturationThreshold)
				{
					return EComparisonResult.OutsideSaturationThreshold;
				}

				if (Mathf.Abs(inputValue - ruleValue) > valueThreshold)
				{
					return EComparisonResult.OutsideValueThreshold;
				}

				// H, S, and V are all within thresholds.
				return EComparisonResult.TooSimilar;
			}

			public bool TryParse(IDatNode node)
			{
				if (node is IDatDictionary dictionary)
				{
					bool hasRequiredValues = dictionary.TryParseColor32RGB("Color", out Color32 color);
					Color.RGBToHSV(color, out ruleHue, out ruleSaturation, out ruleValue);
					hasRequiredValues &= dictionary.TryParseFloat("HueThreshold", out hueThreshold);
					hasRequiredValues &= dictionary.TryParseFloat("SaturationThreshold", out saturationThreshold);
					hasRequiredValues &= dictionary.TryParseFloat("ValueThreshold", out valueThreshold);
					return hasRequiredValues;
				}

				return false;
			}
		}

		/// <summary>
		/// Players are kicked from multiplayer if their skin color is within threshold of any of these rules.
		/// </summary>
		internal List<TerrainColorRule> terrainColorRules;

		public bool isBlueprintBlacklisted(Blueprint blueprint)
		{
			if (craftingBlacklists == null || blueprint == null)
				return false;

			if (resolvedCraftingBlacklists == null)
			{
				resolvedCraftingBlacklists = new List<CraftingBlacklistAsset>(craftingBlacklists.Count);
				foreach (AssetReference<CraftingBlacklistAsset> craftingBlacklistRef in craftingBlacklists)
				{
					CraftingBlacklistAsset craftingBlacklist = craftingBlacklistRef.Find();
					if (craftingBlacklist != null)
					{
						resolvedCraftingBlacklists.Add(craftingBlacklist);
					}
					else
					{
						Assets.ReportError(this, $"unable to find crafting blacklist {craftingBlacklistRef}");
					}
				}
			}

			foreach (CraftingBlacklistAsset craftingBlacklist in resolvedCraftingBlacklists)
			{
				if (craftingBlacklist.isBlueprintBlacklisted(blueprint))
				{
					return true;
				}
			}

			return false;
		}

		public EZombieDifficultyAssetPrioritization ZombieDifficultyAssetPrioritization
		{
			get;
			set;
		}

		/// <summary>
		/// If true, bypasses SafezoneNode no-buildables mode in singleplayer.
		/// </summary>
		public bool ShouldAllowBuildingInSafezonesInSingleplayer
		{
			get;
			set;
		}

		/// <summary>
		/// Blueprints can test for these tags. (Future extension here?)
		/// </summary>
		public CachingAssetRef[] Tags
		{
			get;
			set;
		}

		private CachingAssetRef _defaultFishSpawnTable;
		/// <summary>
		/// Fishing rods using per-water-volume fishing spawn table fallback to this table.
		/// </summary>
		public CachingAssetRef DefaultFishSpawnTable
		{
			get => _defaultFishSpawnTable;
			set => _defaultFishSpawnTable = value;
		}

		public SpawnAsset GetDefaultFishingSpawnTable()
		{
			return _defaultFishSpawnTable.Get<SpawnAsset>();
		}

		/// <summary>
		/// If true, this level has assigned fishing spawn tables to water volumes and/or set
		/// the default table. Defaults to false. Enables fishing rods to work on all maps
		/// regardless of when they were designed.
		/// </summary>
		public bool SupportsFishingVolumes
		{
			get;
			set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			defaultGameMode = p.data.ParseStruct<TypeReference<GameMode>>("Default_Game_Mode");

			if (p.data.TryGetList("Supported_Game_Modes", out IDatList gameModesList))
			{
				supportedGameModes = gameModesList.ParseListOfStructs<TypeReference<GameMode>>();
			}

			dropshipPrefab = p.data.ParseStruct<MasterBundleReference<GameObject>>("Dropship");
			airdropRef = p.data.ParseStruct<AssetReference<AirdropAsset>>("Airdrop");

			if (p.data.TryGetList("Crafting_Blacklists", out IDatList craftingBlacklistNodes))
			{
				if (craftingBlacklistNodes.Count > 0)
				{
					craftingBlacklists = craftingBlacklistNodes.ParseListOfStructs<AssetReference<CraftingBlacklistAsset>>();
				}
			}

			if (p.data.TryGetList("Weather_Types", out IDatList weatherTypesList))
			{
				List<SchedulableWeather> pendingScheduleWeathers = new List<SchedulableWeather>(weatherTypesList.Count);
				for (int index = 0; index < weatherTypesList.Count; ++index)
				{
					if (!(weatherTypesList[index] is IDatDictionary weatherReader))
						continue;

					SchedulableWeather weather = new SchedulableWeather();
					weather.assetRef = weatherReader.ParseStruct<AssetReference<WeatherAssetBase>>("Asset");
					weather.minFrequency = Mathf.Max(0.0f, weatherReader.ParseFloat("Min_Frequency"));
					weather.maxFrequency = Mathf.Max(0.0f, weatherReader.ParseFloat("Max_Frequency"));
					weather.minDuration = Mathf.Max(0.0f, weatherReader.ParseFloat("Min_Duration"));
					weather.maxDuration = Mathf.Max(0.0f, weatherReader.ParseFloat("Max_Duration"));

					if (Mathf.Max(weather.minDuration, weather.maxDuration) > 0.001f)
					{
						pendingScheduleWeathers.Add(weather);
					}
					else
					{
						UnturnedLog.warn("Disabling level {0} weather {1} because max duration is zero", this, weather.assetRef);
					}
				}
				if (pendingScheduleWeathers.Count > 0)
				{
					schedulableWeathers = pendingScheduleWeathers.ToArray();
				}
			}
			perpetualWeatherRef = p.data.ParseStruct<AssetReference<WeatherAssetBase>>("Perpetual_Weather_Asset");

			if (p.data.TryGetList("Loading_Screen_Music", out IDatList musicList))
			{
				loadingScreenMusic = new LoadingScreenMusic[musicList.Count];
				for (int index = 0; index < musicList.Count; ++index)
				{
					IDatNode node = musicList[index];
					if (!(node is IDatDictionary dictionary))
						continue;

					LoadingScreenMusic music = new LoadingScreenMusic();
					music.loopRef = dictionary.ParseStruct<MasterBundleReference<AudioClip>>("Loop");
					music.outroRef = dictionary.ParseStruct<MasterBundleReference<AudioClip>>("Outro");

					if (dictionary.ContainsKey("Loop_Volume"))
					{
						music.loopVolume = dictionary.ParseFloat("Loop_Volume");
					}
					else
					{
						music.loopVolume = 1.0f;
					}

					if (dictionary.ContainsKey("Outro_Volume"))
					{
						music.outroVolume = dictionary.ParseFloat("Outro_Volume");
					}
					else
					{
						music.outroVolume = 1.0f;
					}

					loadingScreenMusic[index] = music;
				}
			}

			// Using TryParse here enables level to remove default death music if they want.
			if (p.data.TryParseStruct("Death_Music", out MasterBundleReference<AudioClip> deathMusic))
			{
				DeathMusicRef = deathMusic;
			}
			else
			{
				DeathMusicRef = DefaultDeathMusicRef;
			}

			shouldAnimateBackgroundImage = p.data.ParseBool("Should_Animate_Background_Image");

			if (p.data.ContainsKey("Global_Weather_Mask"))
			{
				globalWeatherMask = p.data.ParseUInt32("Global_Weather_Mask");
			}
			else
			{
				globalWeatherMask = uint.MaxValue;
			}

			if (p.data.TryGetList("Skills", out IDatList skillsList))
			{
				skillRules = new SkillRule[PlayerSkills.SPECIALITIES][];
				skillRules[(int) EPlayerSpeciality.OFFENSE] = new SkillRule[7];
				skillRules[(int) EPlayerSpeciality.DEFENSE] = new SkillRule[7];
				skillRules[(int) EPlayerSpeciality.SUPPORT] = new SkillRule[8];

				for (int index = 0; index < skillsList.Count; ++index)
				{
					if (!(skillsList[index] is IDatDictionary skillReader))
						continue;

					string id = skillReader.GetString("Id");
					int specialityIndex;
					int skillIndex;
					if (!PlayerSkills.TryParseIndices(id, out specialityIndex, out skillIndex))
					{
						UnturnedLog.warn("Level {0} unable to parse skill index {1} ({2})", this, index, id);
						continue;
					}

					SkillRule rule = new SkillRule();
					rule.defaultLevel = skillReader.ParseInt32("Default_Level");

					if (skillReader.ContainsKey("Max_Unlockable_Level"))
					{
						rule.maxUnlockableLevel = skillReader.ParseInt32("Max_Unlockable_Level");
					}
					else
					{
						rule.maxUnlockableLevel = -1;
					}

					if (skillReader.ContainsKey("Cost_Multiplier"))
					{
						rule.costMultiplier = skillReader.ParseFloat("Cost_Multiplier");
					}
					else
					{
						rule.costMultiplier = 1.0f;
					}

					rule.baseCostOverride = skillReader.ParseInt32("Base_Cost", -1);
					rule.perLevelCostIncreaseOverride = skillReader.ParseInt32("Per_Level_Cost_Increase", -1);

					skillRules[specialityIndex][skillIndex] = rule;
				}
			}

			minStealthRadius = p.data.ParseFloat("Min_Stealth_Radius");
			fallDamageSpeedThreshold = p.data.ParseFloat("Fall_Damage_Speed_Threshold");

			if (p.data.ContainsKey("Enable_Admin_Faster_Salvage_Duration"))
			{
				enableAdminFasterSalvageDuration = p.data.ParseBool("Enable_Admin_Faster_Salvage_Duration");
			}

			if (p.data.ContainsKey("Has_Clouds"))
			{
				hasClouds = p.data.ParseBool("Has_Clouds");

				if (!hasClouds)
				{
					CloudOverridePrefab = p.data.readMasterBundleReference<GameObject>("CloudOverride_Prefab", p.bundle);
					CloudOverrideParticleSystemPaths = p.data.ParseArrayOfStructs<CloudOverrideParticleSystemsPath>("CloudOverride_ParticleSystems");
				}
			}
			else
			{
				hasClouds = true;
			}

			if (p.data.TryGetDictionary("Skillset_Loadouts", out IDatDictionary skillsetLoadoutsDictionary))
			{
				int skillsetCount = ((int) EPlayerSkillset.MEDIC) + 1;
				DefaultSkillsetLoadouts = new DefaultLoadoutItem[skillsetCount][];

				for (int skillsetIndex = 0; skillsetIndex < skillsetCount; ++skillsetIndex)
				{
					EPlayerSkillset skillset = (EPlayerSkillset) skillsetIndex;
					string skillsetKey = skillset.ToString();

					if (skillsetLoadoutsDictionary.TryGetList(skillsetKey, out IDatList itemsNode))
					{
						DefaultSkillsetLoadouts[skillsetIndex] = itemsNode.ParseArrayOfStructs<DefaultLoadoutItem>();
					}
				}
			}

			if (p.data.TryGetList("TerrainColors", out IDatList terrianColorsDatList))
			{
				List<TerrainColorRule> pendingRules = new List<TerrainColorRule>(terrianColorsDatList.Count);
				for (int datIndex = 0; datIndex < terrianColorsDatList.Count; ++datIndex)
				{
					IDatNode node = terrianColorsDatList[datIndex];
					TerrainColorRule rule = new TerrainColorRule();
					if (rule.TryParse(node))
					{
						bool blocksAnyDefaultSkinColor = false;
						foreach (Color defaultSkinColor in Customization.SKINS)
						{
							Color.RGBToHSV(defaultSkinColor, out float hue, out float saturation, out float value);
							TerrainColorRule.EComparisonResult comparisonResult = rule.CompareColors(hue, saturation, value);
							if (comparisonResult == TerrainColorRule.EComparisonResult.TooSimilar)
							{
								blocksAnyDefaultSkinColor = true;
								string reportSkinColor = Palette.hex(defaultSkinColor);
								Assets.ReportError(this, $"skipping TerrainColor entry {datIndex} because it blocks default skin color {reportSkinColor}");
								break;
							}
						}

						if (!blocksAnyDefaultSkinColor)
						{
							pendingRules.Add(rule);
						}
					}
					else
					{
						Assets.ReportError(this, $"unable to parse entry in TerrainColors: {node.DebugDumpToString()}");
					}
				}
				if (pendingRules.Count > 0)
				{
					terrainColorRules = pendingRules;
				}
				else
				{
					Assets.ReportError(this, $"TerrainColors list is empty");
				}
			}

			UnderwaterFogDensity = p.data.ParseFloat("UnderwaterFogDensity", DEFAULT_UNDERWATER_FOG_DENSITY);
			ZombieDifficultyAssetPrioritization = p.data.ParseEnum("ZombieDifficultyAssetPrioritization", EZombieDifficultyAssetPrioritization.NavmeshOverridesTable);
			ShouldAllowBuildingInSafezonesInSingleplayer = p.data.ParseBool("Allow_Building_In_Safezone_In_Singleplayer");
			Tags = p.data.ParseArrayOfStructs<CachingAssetRef>("Tags");

			SupportsFishingVolumes = p.data.ParseBool("Supports_Fishing_Volumes");
			_defaultFishSpawnTable = p.data.ParseAssetRef("Default_Fish_Spawn_Table");
		}

		public string OnGetFishErrorContext()
		{
			return $"{FriendlyName} level asset";
		}

		public LevelAsset() : base()
		{
			supportedGameModes = new List<TypeReference<GameMode>>();
		}
	}
}
