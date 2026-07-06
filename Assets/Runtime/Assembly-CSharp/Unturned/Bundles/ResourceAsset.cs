////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ResourceAsset : Asset, IArmorFalloff
	{
		#region IArmorFalloff
		public float ArmorFalloffMaxRange { get; set; }
		public float ArmorFalloffRange { get; set; }
		public float ArmorFalloffMultiplier { get; set; }
		#endregion IArmorFalloff

		private static List<MeshFilter> meshes = new List<MeshFilter>();
		private static Shader shader;

		protected string _resourceName;
		public string resourceName
		{
			get
			{
				switch (holidayRestriction)
				{
					case ENPCHoliday.HALLOWEEN:
						return _resourceName + " [HW]";
					case ENPCHoliday.CHRISTMAS:
						return _resourceName + " [XMAS]";
					case ENPCHoliday.APRIL_FOOLS:
						return _resourceName + " [AF]";
					case ENPCHoliday.VALENTINES:
						return _resourceName + " [V]";
					case ENPCHoliday.PRIDE_MONTH:
						return _resourceName + " [PM]";
					case ENPCHoliday.LUNAR_NEW_YEAR:
						return _resourceName + " [LNY]";
					default:
						return _resourceName;
				}
			}
		}

		public override string FriendlyName => resourceName;

		protected GameObject _modelGameObject;
		public GameObject modelGameObject => _modelGameObject;

		protected GameObject _stumpGameObject;
		public GameObject stumpGameObject => _stumpGameObject;

		protected GameObject _skyboxGameObject;
		public GameObject skyboxGameObject => _skyboxGameObject;

		protected GameObject _debrisGameObject;
		public GameObject debrisGameObject => _debrisGameObject;

		public Material skyboxMaterial
		{
			get;
			private set;
		}


		public ushort health;


		public uint rewardXP;


		[System.Obsolete("Replaced by MinRandomScale and MaxRandomScale.")]
		public float scale;

		public float verticalOffset;

		/// <summary>
		/// Distance along tree's local up axis to offset debris spawn position. Defaults to 1.0.
		/// </summary>
		public float DebrisVerticalOffset
		{
			get;
			protected set;
		}

		public float MinRandomAngleDeviation
		{
			get;
			protected set;
		}

		/// <summary>
		/// Before <see cref="FoliageResourceInfoAsset"/> had randomization properties each tree has
		/// some random rotation and scale variation based on its position. This property controls
		/// the rotation away from upright.
		/// </summary>
		public float MaxRandomAngleDeviation
		{
			get;
			protected set;
		}

		public float MinRandomUniformScale
		{
			get;
			protected set;
		}

		public float MaxRandomUniformScale
		{
			get;
			protected set;
		}

		private System.Guid _explosionGuid;
		public System.Guid explosionGuid => _explosionGuid;
		public ushort explosion;

		public EffectAsset FindExplosionEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(_explosionGuid, explosion);
#pragma warning restore
		}


		public ushort log;


		public ushort stick;


		public byte rewardMin;


		public byte rewardMax;


		public ushort rewardID;


		public bool isForage;

		/// <summary>
		/// Amount of experience to reward foraging player.
		/// </summary>
		public uint forageRewardExperience;

		/// <summary>
		/// Forageable resource message.
		/// </summary>
		public string interactabilityText;


		public bool hasDebris;

		/// <summary>
		/// Weapon must have matching blade ID to damage tree.
		/// Both weapons and trees default to zero so they can be damaged by default.
		/// </summary>

		public byte bladeID;


		public float reset;

		public bool vulnerableToFists
		{
			get;
			protected set;
		}

		public bool vulnerableToAllMeleeWeapons
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, prevent collisions between falling tree and the stump. (i.e., debris can fall through stump)
		/// Defaults to true.
		/// </summary>
		public bool ShouldIgnoreCollisionBetweenStumpAndDebris
		{
			get;
			protected set;
		}

		public override EAssetType assetCategory => EAssetType.RESOURCE;

		/// <summary>
		/// Only activated during this holiday.
		/// </summary>
		public ENPCHoliday holidayRestriction
		{
			get;
			protected set;
		}

		/// <summary>
		/// Tree to use during the Christmas event instead.
		/// </summary>
		public AssetReference<ResourceAsset> christmasRedirect;

		/// <summary>
		/// Tree to use during the Halloween event instead.
		/// </summary>
		public AssetReference<ResourceAsset> halloweenRedirect;

		/// <summary>
		/// Get asset ref to replace this one for holiday, or null if it should not be redirected.
		/// </summary>
		public AssetReference<ResourceAsset> getHolidayRedirect()
		{
			switch (HolidayUtil.getActiveHoliday())
			{
				case ENPCHoliday.CHRISTMAS:
					return christmasRedirect;

				case ENPCHoliday.HALLOWEEN:
					return halloweenRedirect;

				default:
					return AssetReference<ResourceAsset>.invalid;
			}
		}

		public EObjectChart chart;

		public void GetLegacyRotationAndScale(Vector3 point, out Quaternion rotation, out Vector3 scale)
		{
			float seed = Mathf.Sin(((point.x + 4096) * 32) + ((point.z + 4096) * 32));
			// Nelson 2024-12-11: It's like this for backwards compatibility. :P
			float lerpWeight = (seed + 1.0f) * 0.5f;
			float angleDeviation = Mathf.Lerp(MinRandomAngleDeviation, MaxRandomAngleDeviation, lerpWeight);
			rotation = Quaternion.Euler(angleDeviation, seed * 360, 0);
			float randomScale = Mathf.Lerp(MinRandomUniformScale, MaxRandomUniformScale, lerpWeight);
			scale = new Vector3(randomScale, randomScale, randomScale);
		}

		protected void applyDefaultLODs(LODGroup lod, bool fade)
		{
			LOD[] lods = lod.GetLODs();
			lods[0].screenRelativeTransitionHeight = fade ? 0.7f : 0.6f;
			lods[1].screenRelativeTransitionHeight = fade ? 0.5f : 0.4f;
			lods[2].screenRelativeTransitionHeight = 0.15f;
			lods[3].screenRelativeTransitionHeight = 0.03f;
			lod.SetLODs(lods);
		}

		public bool shouldExcludeFromLevelBatching;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (id < 50 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 50");
			}

			_resourceName = p.localization.format("Name");

			if (Dedicator.IsDedicatedServer)
			{
				if (p.data.ParseBool("Has_Clip_Prefab", defaultValue: true))
				{
					_modelGameObject = p.bundle.load<GameObject>("Resource_Clip");
					if (_modelGameObject == null)
					{
						Assets.ReportError(this, "missing \"Resource_Clip\" GameObject, loading \"Resource\" GameObject instead");
					}

					_stumpGameObject = p.bundle.load<GameObject>("Stump_Clip");
					if (_stumpGameObject == null)
					{
						Assets.ReportError(this, "missing \"Stump_Clip\" GameObject, loading \"Stump\" GameObject instead");
					}
				}

				if (_modelGameObject == null)
				{
					_modelGameObject = p.bundle.load<GameObject>("Resource");
					if (_modelGameObject == null)
					{
						Assets.ReportError(this, "missing \"Resource\" GameObject");
					}
					else
					{
						// Optimize client prefab for server usage.
						ServerPrefabUtil.RemoveClientComponents(_modelGameObject, this);
					}
				}

				if (_stumpGameObject == null)
				{
					_stumpGameObject = p.bundle.load<GameObject>("Stump");
					if (_stumpGameObject == null)
					{
						Assets.ReportError(this, "missing \"Stump\" GameObject");
					}
					else
					{
						// Optimize client prefab for server usage.
						ServerPrefabUtil.RemoveClientComponents(_stumpGameObject, this);
					}
				}
			}
			else
			{
				_modelGameObject = p.bundle.load<GameObject>("Resource_Old");
				if (_modelGameObject == null)
				{
					_modelGameObject = p.bundle.load<GameObject>("Resource");
				}
				if (_modelGameObject == null)
				{
					Assets.ReportError(this, "missing \"Resource\" GameObject");
				}

				_stumpGameObject = p.bundle.load<GameObject>("Stump_Old");
				if (_stumpGameObject == null)
				{
					_stumpGameObject = p.bundle.load<GameObject>("Stump");
				}
				if (_stumpGameObject == null)
				{
					Assets.ReportError(this, "missing \"Stump\" GameObject");
				}

				_skyboxGameObject = p.bundle.load<GameObject>("Skybox_Old");
				if (_skyboxGameObject == null)
				{
					_skyboxGameObject = p.bundle.load<GameObject>("Skybox");
				}

				_debrisGameObject = p.bundle.load<GameObject>("Debris_Old");
				if (_debrisGameObject == null)
				{
					_debrisGameObject = p.bundle.load<GameObject>("Debris");
				}

				if (p.data.ContainsKey("Auto_Skybox") && skyboxGameObject)
				{
					Transform model_0 = modelGameObject.transform.Find("Model_0");

					if (model_0)
					{
						meshes.Clear();
						model_0.GetComponentsInChildren(true, meshes);

						if (meshes.Count > 0)
						{
							Bounds bound = new Bounds();
							for (int index = 0; index < meshes.Count; index++)
							{
								Mesh mesh = meshes[index].sharedMesh;

								if (mesh == null)
								{
									continue;
								}

								Bounds other = mesh.bounds;
								bound.Encapsulate(other.min);
								bound.Encapsulate(other.max);
							}

							if (bound.min.y < 0)
							{
								float min = Mathf.Abs(bound.min.z);
								bound.center += new Vector3(0.0f, 0.0f, min / 2);
								bound.size -= new Vector3(0.0f, 0.0f, min);
							}

							float range = Mathf.Max(bound.size.x, bound.size.y);
							float height = bound.size.z;

							skyboxGameObject.transform.localScale = new Vector3(height, height, height);

							Transform icon = GameObject.Instantiate(modelGameObject).transform;

							Transform hook_0 = new GameObject().transform;
							hook_0.parent = icon;
							hook_0.localPosition = new Vector3(0.0f, height / 2, -range / 2);
							hook_0.localRotation = Quaternion.identity;

							Transform hook_1 = new GameObject().transform;
							hook_1.parent = icon;
							hook_1.localPosition = new Vector3(-range / 2, height / 2, 0);
							hook_1.localRotation = Quaternion.Euler(0, 90.0f, 0.0f);

							if (!shader)
							{
								shader = Shader.Find("Custom/Card");
							}

							Texture2D texture = ItemTool.getCard(icon, hook_0, hook_1, 64, 64, height / 2, range);
							skyboxMaterial = new Material(shader);
							skyboxMaterial.mainTexture = texture;
						}
					}
				}
			}

			// Required for hit detection which uses CompareTag.
			{
				if (_modelGameObject != null)
				{
					_modelGameObject.SetTagIfUntaggedRecursively("Resource");
				}
				if (_stumpGameObject != null)
				{
					_stumpGameObject.SetTagIfUntaggedRecursively("Resource");
				}
				if (_skyboxGameObject != null)
				{
					_skyboxGameObject.SetTagIfUntaggedRecursively("Resource");
				}
			}

			health = p.data.ParseUInt16("Health");

			MinRandomAngleDeviation = p.data.ParseFloat("RandomAngleDeviation_Min", defaultValue: -5.0f);
			MaxRandomAngleDeviation = p.data.ParseFloat("RandomAngleDeviation_Max", defaultValue: 5.0f);

			// Nelson 2024-12-11: The old in-game transform scale was:
			// 1.1f + asset.scale + (seed * asset.scale)
			// where "seed" is [-1, 1]
			if (p.data.TryParseFloat("Scale", out float legacyScale))
			{
#pragma warning disable
				scale = legacyScale;
#pragma warning restore
				MinRandomUniformScale = 1.1f;
				MaxRandomUniformScale = 1.1f + legacyScale * 2.0f;
			}
			else
			{
				MinRandomUniformScale = p.data.ParseFloat("RandomUniformScale_Min", defaultValue: 1.1f);
				MaxRandomUniformScale = p.data.ParseFloat("RandomUniformScale_Max", defaultValue: 1.1f);
#pragma warning disable
				scale = (MaxRandomUniformScale - MinRandomUniformScale) * 0.5f;
#pragma warning restore
			}

			verticalOffset = p.data.ParseFloat("Vertical_Offset", defaultValue: -0.75f);
			DebrisVerticalOffset = p.data.ParseFloat("Debris_Vertical_Offset", 1.0f);

			explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionGuid);
			log = p.data.ParseUInt16("Log");
			stick = p.data.ParseUInt16("Stick");

			rewardID = p.data.ParseUInt16("Reward_ID");
			rewardXP = p.data.ParseUInt32("Reward_XP");

			if (p.data.ContainsKey("Reward_Min"))
			{
				rewardMin = p.data.ParseUInt8("Reward_Min");
			}
			else
			{
				rewardMin = 6;
			}

			if (p.data.ContainsKey("Reward_Max"))
			{
				rewardMax = p.data.ParseUInt8("Reward_Max");
			}
			else
			{
				rewardMax = 9;
			}

			bladeID = p.data.ParseUInt8("BladeID");
			vulnerableToFists = p.data.ParseBool("Vulnerable_To_Fists", defaultValue: false);
			vulnerableToAllMeleeWeapons = p.data.ParseBool("Vulnerable_To_All_Melee_Weapons", defaultValue: false);
			ShouldIgnoreCollisionBetweenStumpAndDebris = p.data.ParseBool("Ignore_Collision_Between_Stump_And_Debris", true);

			reset = p.data.ParseFloat("Reset");

			isForage = p.data.ContainsKey("Forage");
			if (isForage && _modelGameObject != null)
			{
				Transform forageTransform = _modelGameObject.transform.Find("Forage");
				if (forageTransform != null)
				{
					// For some reason these were on the Water layer? Edit 2021-09-16: issue #2825 damaging berry bushes. 
					forageTransform.gameObject.layer = LayerMasks.RESOURCE;
				}
				else
				{
					Assets.ReportError(this, "foragable resource missing \"Forage\" GameObject");
				}
			}
			forageRewardExperience = p.data.ParseUInt32("Forage_Reward_Experience", defaultValue: 1);

			if (isForage)
			{
				// Nelson 2025-11-07: previously this used read() which didn't handle fallbacks.
				interactabilityText = p.localization.FormatOrEmpty("Interact");
				interactabilityText = ItemTool.filterRarityRichText(interactabilityText);
			}

			hasDebris = !p.data.ContainsKey("No_Debris");

			if (p.data.ContainsKey("Holiday_Restriction"))
			{
				holidayRestriction = (ENPCHoliday) System.Enum.Parse(typeof(ENPCHoliday), p.data.GetString("Holiday_Restriction"), true);
				if (holidayRestriction == ENPCHoliday.NONE)
				{
					Assets.ReportError(this, "has no holiday restriction, so value is ignored");
				}
			}
			else
			{
				holidayRestriction = ENPCHoliday.NONE;
			}

			christmasRedirect = p.data.readAssetReference<ResourceAsset>("Christmas_Redirect");
			halloweenRedirect = p.data.readAssetReference<ResourceAsset>("Halloween_Redirect");

			chart = p.data.ParseEnum("Chart", EObjectChart.NONE);

			shouldExcludeFromLevelBatching = p.data.ParseBool("Exclude_From_Level_Batching");

			this.PopulateArmorFalloff(in p); // this. is necessary, at least in current C# version.
		}

		internal string OnGetRewardSpawnTableErrorContext()
		{
			return $"{FriendlyName} reward";
		}
	}
}
