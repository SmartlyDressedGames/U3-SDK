////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

#if UNITY_EDITOR
using UnityEditor; // Fix some import settings when running from the editor.
#endif // UNITY_EDITOR

namespace SDG.Unturned
{
	internal enum EInteractableObjectBinaryStateEmissiveMaterialMode
	{
		/// <summary>
		/// Default. Create a material instance for child renderer of Toggle game object.
		/// Downside of this is exclusion from level batching texture atlas.
		/// </summary>
		Auto,

		/// <summary>
		/// Object does not have any toggleable emissive materials.
		/// </summary>
		None,
	}

	public class ObjectAsset : Asset, IArmorFalloff
	{
		#region IArmorFalloff
		public float ArmorFalloffMaxRange { get; set; }
		public float ArmorFalloffRange { get; set; }
		public float ArmorFalloffMultiplier { get; set; }
		#endregion IArmorFalloff

		protected string _objectName;
		public string objectName
		{
			get
			{
				switch (holidayRestriction)
				{
					case ENPCHoliday.HALLOWEEN:
						return _objectName + " [HW]";
					case ENPCHoliday.CHRISTMAS:
						return _objectName + " [XMAS]";
					case ENPCHoliday.APRIL_FOOLS:
						return _objectName + " [AF]";
					case ENPCHoliday.VALENTINES:
						return _objectName + " [V]";
					case ENPCHoliday.PRIDE_MONTH:
						return _objectName + " [PM]";
					case ENPCHoliday.LUNAR_NEW_YEAR:
						return _objectName + " [LNY]";
					default:
						return _objectName;
				}
			}
		}

		public override string FriendlyName => objectName;


		public EObjectType type;

		private GameObject loadedModel;
		/// <summary>
		/// Prevents calling getOrLoad redundantly if asset does not exist.
		/// </summary>
		private bool hasCalledLoadModel;
		private IDeferredAsset<GameObject> clientModel;
		private IDeferredAsset<GameObject> legacyServerModel;

		/// <summary>
		/// If set, overrides model prefab in the level editor. 
		/// </summary>
		private IDeferredAsset<GameObject> editorModel;

		public GameObject GetOrLoadModel(bool isEditor = false)
		{
			if (!hasCalledLoadModel)
			{
				hasCalledLoadModel = true;

				if (legacyServerModel != null)
				{
					loadedModel = legacyServerModel.getOrLoad();
					if (loadedModel == null)
					{
						loadedModel = clientModel.getOrLoad();
					}
				}
				else
				{
					loadedModel = clientModel.getOrLoad();
				}
			}

			// Nelson 2026-01-06: moved after loading regular model so usesScale is initialized.
			if (isEditor && editorModel != null)
			{
				GameObject go = editorModel.getOrLoad();
				if (go != null)
				{
					return go;
				}
			}

			return loadedModel;
		}

		protected void validateModel(GameObject asset)
		{
			if (Mathf.Abs(asset.transform.localScale.x - 1.0f) > 0.01f || Mathf.Abs(asset.transform.localScale.y - 1.0f) > 0.01f || Mathf.Abs(asset.transform.localScale.z - 1.0f) > 0.01f)
			{
				useScale = false;

				Assets.ReportError(this, "should have a scale of one");
			}
			else
			{
				useScale = true;
			}

			Transform block = asset.transform.Find("Block");
			if (block != null)
			{
				if (block.GetComponent<Collider>() != null && block.GetComponent<Collider>().sharedMaterial == null)
				{
					Assets.ReportError(this, "has a 'Block' collider but no physics material");
				}
			}

			Transform mesh = asset.transform.Find("Model_0");

			string expectedTag = string.Empty;
			int expectedLayer = -1;

			if (type == EObjectType.SMALL)
			{
				expectedTag = "Small";
				expectedLayer = LayerMasks.SMALL;
			}
			else if (type == EObjectType.MEDIUM)
			{
				expectedTag = "Medium";
				expectedLayer = LayerMasks.MEDIUM;
			}
			else if (type == EObjectType.LARGE)
			{
				expectedTag = "Large";
				expectedLayer = LayerMasks.LARGE;
			}

			if (expectedLayer == -1)
			{
				Assets.ReportError(this, "has an unknown tag/layer because it has an unhandled EObjectType");
			}
			else
			{
				fixTagAndLayer(asset, expectedTag, expectedLayer);

				if (mesh != null)
				{
					fixTagAndLayer(mesh.gameObject, expectedTag, expectedLayer);
				}

				AssetValidation.searchGameObjectForErrors(this, asset);
			}

			if (Assets.shouldValidateAssets)
			{
				if (interactability == EObjectInteractability.BINARY_STATE)
				{
					Animation animationComponent = asset.transform.Find("Root")?.GetComponent<Animation>();
					if (animationComponent != null)
					{
						validateAnimation(animationComponent, "Open");
						validateAnimation(animationComponent, "Close");
					}
				}

				if (interactability == EObjectInteractability.RUBBLE || rubble != EObjectRubble.NONE)
				{
					Transform sections = asset.transform.Find("Sections");
					if (sections != null && sections.childCount > 8)
					{
						Assets.ReportError(this, $"destructible has {sections.childCount} sections, but the maximum supported is 8");
					}
				}
			}
		}

		/// <summary>
		/// Clip.prefab
		/// </summary>
		protected void OnServerModelLoaded(GameObject asset)
		{
			if (asset == null && type != EObjectType.SMALL)
			{
				Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Object\" GameObject instead");
			}

			if (asset != null)
			{
				validateModel(asset);
			}
		}

		/// <summary>
		/// Object.prefab
		/// </summary>
		protected void OnClientModelLoaded(GameObject asset)
		{
			if (asset == null)
			{
				Assets.ReportError(this, "missing \"Object\" GameObject");
			}
			else
			{
				validateModel(asset);

				if (Dedicator.IsDedicatedServer)
				{
					// Optimize client prefab for server usage.
					ServerPrefabUtil.RemoveClientComponents(asset, this);
				}
			}
		}

		public IDeferredAsset<GameObject> skyboxGameObject;

		public IDeferredAsset<GameObject> navGameObject;

		protected void onNavGameObjectLoaded(GameObject asset)
		{
			if (asset == null && type == EObjectType.LARGE)
			{
				Assets.ReportError(this, "missing Nav GameObject. Highly recommended to fix.");
			}

			if (asset != null)
			{
				fixTagAndLayer(asset, "Navmesh", LayerMasks.NAVMESH);

				if (Assets.shouldValidateAssets)
				{
					ensureNavMeshReadable();
				}
			}

			if (Assets.shouldValidateAssets)
			{
				if (interactabilityNav != EObjectInteractabilityNav.NONE)
				{
					if (asset == null)
					{
						ReportAssetError($"has Interactability_Nav_Mode {interactabilityNav} but no Nav prefab");
					}
					else if (asset.GetComponentInChildren<Collider>(/*includeInactive*/ true) == null)
					{
						Component cutComponent = null;
						System.Type cutComponentType = UnturnedPathfinding.Get().GetCutComponentType();
						if (cutComponentType != null)
						{
							cutComponent = asset.GetComponentInChildren(cutComponentType, /*includeInactive*/ true);
						}

						if (cutComponent == null)
						{
							ReportAssetError($"has Interactability_Nav_Mode {interactabilityNav} but Nav prefab has no collision or navmesh cuts");
						}
					}
				}

				if (RubbleNavMode != EObjectRubbleNavMode.Unaffected)
				{
					if (asset == null)
					{
						ReportAssetError($"has Rubble_Nav_Mode {RubbleNavMode} but no Nav prefab");
					}
					else if (asset.GetComponentInChildren<Collider>(/*includeInactive*/ true) == null)
					{
						Component cutComponent = null;
						System.Type cutComponentType = UnturnedPathfinding.Get().GetCutComponentType();
						if (cutComponentType != null)
						{
							cutComponent = asset.GetComponentInChildren(cutComponentType, /*includeInactive*/ true);
						}

						if (cutComponent == null)
						{
							ReportAssetError($"has Rubble_Nav_Mode {RubbleNavMode} but Nav prefab has no collision or navmesh cuts");
						}
					}
				}
			}
		}

		public IDeferredAsset<GameObject> slotsGameObject;

		protected void onSlotsGameObjectLoaded(GameObject asset)
		{
			if (asset != null)
			{
				// Window items expect Slots to be tagged.
				asset.SetTagIfUntaggedRecursively("Logic");
			}
		}

		public IDeferredAsset<GameObject> triggersGameObject;


		public bool isSnowshoe;
		public bool shouldExcludeFromCullingVolumes;
		public bool shouldExcludeFromLevelBatching;

		/// <summary>
		/// If true, object will be hidden when rendering GPS/satellite view.
		/// Defaults to true if <see cref="holidayRestriction"/> is set.
		/// </summary>
		public bool ShouldExcludeFromSatelliteCapture
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, Nav game object will be instantiated in singleplayer and on dedicated server. Useful for objects
		/// which need to affect navmesh baking without colliding with zombies during gameplay.
		/// Defaults to true for "medium" and "large" objects.
		/// </summary>
		public bool ShouldLoadNavOnServer
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, Nav game object will be instantiated in the level editor. Useful for objects which need collision
		/// with zombies during gameplay without affecting navmesh baking.
		/// Defaults to true for "medium" and "large" objects.
		/// </summary>
		public bool ShouldLoadNavInEditor
		{
			get;
			private set;
		}

		public EObjectChart chart;


		public bool isFuel;


		public bool isRefill;


		public bool isSoft;

		/// <summary>
		/// Should landing on this object inflict fall damage?
		/// </summary>
		public bool causesFallDamage;


		public bool isCollisionImportant;

		/// <summary>
		/// If true, object is not loaded when clutter is turned off in graphics menu.
		/// </summary>
		public bool IsClutter
		{
			get;
			set;
		}

		public bool useScale;

#if !DEDICATED_SERVER
		public EObjectLOD lod;
		public float lodBias;
		public Vector3 cullingVolumeLocalPositionOffset;
		public Vector3 cullingVolumeSizeOffset;
#endif // !DEDICATED_SERVER

		[System.Obsolete]
		public INPCCondition[] conditions;

		internal NPCConditionsList visibilityConditionsList;


		public EObjectInteractability interactability;


		public bool interactabilityRemote;


		public float interactabilityDelay;


		public EObjectInteractabilityHint interactabilityHint;


		public string interactabilityText;


		public EObjectInteractabilityPower interactabilityPower;


		public EObjectInteractabilityEditor interactabilityEditor;


		public EObjectInteractabilityNav interactabilityNav;


		public float interactabilityReset;


		public ushort interactabilityResource;

		private byte[] interactabilityResourceState;


		public ushort[] interactabilityDrops;


		public ushort interactabilityRewardID;
		public EItemOrigin interactabilityRewardItemOrigin;

		public System.Guid interactabilityEffectGuid;
		[System.Obsolete]
		public ushort interactabilityEffect;

		public EffectAsset FindInteractabilityEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(interactabilityEffectGuid, interactabilityEffect);
#pragma warning restore
		}

		internal AssetReference<DialogueAsset> interactabilityDialogueRef;

		/// <summary>
		/// Property is not exposed at the moment because interactability properties should really be moved into some
		/// sort of sub-asset.
		/// </summary>
		public DialogueAsset FindInteractabilityDialogueAsset()
		{
			return interactabilityDialogueRef.Find();
		}

		/// <summary>
		/// Same as interactabilityDialogueRef, not public because it really needs to be cleaned up. :(
		/// </summary>
		internal string interactabilityChildPathOverride;
		internal EInteractableObjectBinaryStateEmissiveMaterialMode iobsEmissiveMaterialMode;
		/// <summary>
		/// If set, overrides objectName as character name in dialogue.
		/// </summary>
		public string InteractabilityDialogueDisplayName
		{
			get;
			set;
		}

		[System.Obsolete]
		public INPCCondition[] interactabilityConditions;

		internal NPCConditionsList interactabilityConditionsList;


		protected NPCRewardsList interactabilityRewards;


		public EObjectRubble rubble;


		public float rubbleReset;


		public ushort rubbleHealth;

		/// <summary>
		/// Effect played when single segment is destroyed.
		/// </summary>
		public System.Guid rubbleEffectGuid;
		[System.Obsolete]
		public ushort rubbleEffect;

		public EffectAsset FindRubbleEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(rubbleEffectGuid, rubbleEffect);
#pragma warning restore
		}

		/// <summary>
		/// Effect played when all segments are destroyed.
		/// </summary>
		public System.Guid rubbleFinaleGuid;
		[System.Obsolete]
		public ushort rubbleFinale;

		public bool IsRubbleFinaleEffectRefNull()
		{
#pragma warning disable
			return rubbleFinale == 0 && rubbleFinaleGuid.IsEmpty();
#pragma warning restore
		}

		public EffectAsset FindRubbleFinaleEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(rubbleFinaleGuid, rubbleFinale);
#pragma warning restore
		}


		public EObjectRubbleEditor rubbleEditor;


		public ushort rubbleRewardID;

		/// <summary>
		/// Weapon must have matching blade ID to damage object.
		/// Both weapons and objects default to zero so they can be damaged by default.
		/// </summary>
		public byte rubbleBladeID;

		/// <summary>
		/// [0, 1] probability of dropping any rewards.
		/// </summary>
		public float rubbleRewardProbability;

		public byte rubbleRewardsMin;
		public byte rubbleRewardsMax;


		public uint rubbleRewardXP;


		public bool rubbleIsVulnerable;


		public bool rubbleProofExplosion;

		/// <summary>
		/// If true, zombies can attack this object if it's blocking them. Defaults to false.
		/// </summary>
		public bool RubbleCanZombiesDamage
		{
			get;
			protected set;
		}

		/// <summary>
		/// Multiplier for damage from zombies if RubbleCanZombiesDamage is true.
		/// </summary>
		public float RubbleZombieDamageMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Controls how rubble affects Nav game object.
		/// </summary>
		public EObjectRubbleNavMode RubbleNavMode
		{
			get;
			protected set;
		}

		/// <summary>
		/// If set (>0), alerts nearby entities when an individual section is destroyed.
		/// </summary>
		public float RubbleSectionDestroyedAlertRadius
		{
			get;
			protected set;
		}

		/// <summary>
		/// If set (>0), alerts nearby entities when all sections are destroyed.
		/// </summary>
		public float RubbleAllSectionsDestroyedAlertRadius
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, all sections respawn at the same time.
		/// </summary>
		public bool RubbleRespawnAllSectionsSimultaneously
		{
			get;
			set;
		}

		public AssetReference<SDG.Framework.Foliage.FoliageInfoCollectionAsset> foliage;


		public bool useWaterHeightTransparentSort;


		public AssetReference<MaterialPaletteAsset> materialPalette;


		public EGraphicQuality landmarkQuality;

		public bool shouldAddNightLightScript;

		/// <summary>
		/// Should colliders in the Triggers GameObject with "Kill" name kill players?
		/// If Triggers GameObject is not set, searches Object instead.
		/// </summary>
		public bool shouldAddKillTriggers;

		public bool allowStructures;

		/// <summary>
		/// Should this object only be visible if gore is enabled?
		/// Allows pre-placed blood splatters to be hidden for younger players.
		/// </summary>
		public bool isGore;

		/// <summary>
		/// Only activated during this holiday.
		/// </summary>
		public ENPCHoliday holidayRestriction
		{
			get;
			protected set;
		}

		/// <summary>
		/// Object to use during the Christmas event instead.
		/// </summary>
		public AssetReference<ObjectAsset> christmasRedirect;

		/// <summary>
		/// Object to use during the Halloween event instead.
		/// </summary>
		public AssetReference<ObjectAsset> halloweenRedirect;

		/// <summary>
		/// Get asset ref to replace this one for holiday, or null if it should not be redirected.
		/// </summary>
		public AssetReference<ObjectAsset> getHolidayRedirect()
		{
			switch (HolidayUtil.getActiveHoliday())
			{
				case ENPCHoliday.CHRISTMAS:
					return christmasRedirect;

				case ENPCHoliday.HALLOWEEN:
					return halloweenRedirect;

				default:
					return AssetReference<ObjectAsset>.invalid;
			}
		}

		public virtual byte[] getState()
		{
			byte[] state;

			if (interactability == EObjectInteractability.BINARY_STATE)
			{
				state = new byte[1];
				state[0] = (byte) (Level.isEditor && interactabilityEditor != EObjectInteractabilityEditor.NONE ? 1 : 0);
			}
			else if (interactability == EObjectInteractability.WATER || interactability == EObjectInteractability.FUEL)
			{
				state = new byte[2];
				state[0] = interactabilityResourceState[0];
				state[1] = interactabilityResourceState[1];
			}
			else
			{
				state = null;
			}

			if (rubble == EObjectRubble.DESTROY)
			{
				if (state != null)
				{
					byte[] newState = new byte[state.Length + 1];
					System.Array.Copy(state, newState, state.Length);
					state = newState;
				}
				else
				{
					state = new byte[1];
				}

				state[state.Length - 1] = (Level.isEditor && rubbleEditor == EObjectRubbleEditor.DEAD) ? byte.MinValue : byte.MaxValue; // 8 bits, each dead or not
			}

			return state;
		}

		public override EAssetType assetCategory => EAssetType.OBJECT;

		public bool areConditionsMet(Player player)
		{
			return visibilityConditionsList.AreConditionsMet(player);
		}

		/// <summary>
		/// If any conditions use flags they will be added to a set,
		/// otherwise null is returned.
		/// </summary>
		internal HashSet<ushort> GetConditionAssociatedFlags()
		{
			if (visibilityConditionsList.conditions == null)
				return null;

			foreach (INPCCondition condition in visibilityConditionsList.conditions)
			{
				condition.GatherAssociatedFlags(tempAssociatedFlags);
			}

			if (tempAssociatedFlags.Count > 0)
			{
				HashSet<ushort> result = tempAssociatedFlags;
				tempAssociatedFlags = new HashSet<ushort>();
				return result;
			}
			else
			{
				return null;
			}
		}
		private static HashSet<ushort> tempAssociatedFlags = new HashSet<ushort>();

		public bool areInteractabilityConditionsMet(Player player)
		{
			return interactabilityConditionsList.AreConditionsMet(player);
		}

		public void ApplyInteractabilityConditions(Player player)
		{
			interactabilityConditionsList.ApplyConditions(player);
		}

		public void GrantInteractabilityRewards(Player player)
		{
			interactabilityRewards.Grant(player);
		}

		/// <summary>
		/// Recursively change all children including root from oldTag to newTag.
		/// Aborts if a child doesn't match the old tag because it might be something we shouldn't change the tag of.
		/// <return>True if tags were all successfully changed.</return>
		/// </summary>
		protected bool recursivelyFixTag(GameObject parentGameObject, string oldTag, string newTag)
		{
			if (parentGameObject.CompareTag(oldTag))
			{
				parentGameObject.tag = newTag;

				int childCount = parentGameObject.transform.childCount;
				for (int childIndex = 0; childIndex < childCount; childIndex++)
				{
					GameObject childGameObject = parentGameObject.transform.GetChild(childIndex).gameObject;
					if (!recursivelyFixTag(childGameObject, oldTag, newTag))
						return false;
				}

				return true;
			}
			else
			{
				Assets.ReportError(this, "unable to automatically fix tag for " + objectName + "'s " + parentGameObject.name + "! Trying to convert tag " + oldTag + " to " + newTag);
				return false;
			}
		}

		/// <summary>
		/// Recursively change all children including root from oldLayer to newLayer.
		/// Aborts if a child doesn't match the old layer because it might be something we shouldn't change the layer of.
		/// <return>True if layers were all successfully changed.</return>
		/// </summary>
		protected bool recursivelyFixLayer(GameObject parentGameObject, int oldLayer, int newLayer)
		{
			if (parentGameObject.layer == oldLayer)
			{
				parentGameObject.layer = newLayer;

				int childCount = parentGameObject.transform.childCount;
				for (int childIndex = 0; childIndex < childCount; childIndex++)
				{
					GameObject childGameObject = parentGameObject.transform.GetChild(childIndex).gameObject;
					if (!recursivelyFixLayer(childGameObject, oldLayer, newLayer))
						return false;
				}

				return true;
			}
			else
			{
				Assets.ReportError(this, "Unable to automatically fix layer for " + objectName + "'s " + parentGameObject.name + "! Trying to convert layer " + oldLayer + " to " + newLayer);
				return false;
			}
		}

		protected void fixTagAndLayer(GameObject rootGameObject, string expectedTag, int expectedLayer)
		{
			if (!rootGameObject.CompareTag(expectedTag))
			{
				string currentTag = rootGameObject.tag;
				recursivelyFixTag(rootGameObject, currentTag, expectedTag);
			}

			if (rootGameObject.layer != expectedLayer)
			{
				int currentLayer = rootGameObject.layer;
				recursivelyFixLayer(rootGameObject, currentLayer, expectedLayer);
			}
		}

		/// <summary>
		/// Called if we have a valid Nav GameObject.
		/// Recast requires any meshes used on the Nav objects to be CPU readable, so we log errors here if they're not marked as such.
		/// </summary>
		private void ensureNavMeshReadable()
		{
			navMCs.Clear();
			navGameObject?.getOrLoad().GetComponentsInChildren(/*includeInactive*/ true, navMCs);
			foreach (MeshCollider mc in navMCs)
			{
				if (mc.sharedMesh == null)
				{
					Assets.ReportError(this, "missing mesh for MeshCollider '" + mc.name + "'");
					continue;
				}

				if (!mc.sharedMesh.isReadable)
				{
					Assets.ReportError(this, "mesh must have read/write enabled for MeshCollider '" + mc.name + "'");

#if UNITY_EDITOR // If we're running in the editor we may as well automatically fix this import option.
					string meshPath = AssetDatabase.GetAssetPath(mc.sharedMesh);
					if (string.IsNullOrEmpty(meshPath))
					{
						Assets.ReportError(this, "unable to get asset path for nav mesh when trying to fix read/write flag");
						continue;
					}

					ModelImporter importer = AssetImporter.GetAtPath(meshPath) as ModelImporter;
					if (importer == null)
					{
						Assets.ReportError(this, "unable to get model importer for nav mesh when trying to fix read/write flag");
						continue;
					}

					UnturnedLog.info("Re-importing nav mesh as readable at: " + meshPath);
					importer.isReadable = true;
					importer.SaveAndReimport();
#endif // UNITY_EDITOR
				}
			}
		}
		private static List<MeshCollider> navMCs = new List<MeshCollider>();

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_objectName = p.localization.format("Name");

			type = (EObjectType) System.Enum.Parse(typeof(EObjectType), p.data.GetString("Type"), true);

			if (type == EObjectType.NPC)
			{
				if (Dedicator.IsDedicatedServer)
				{
					loadedModel = Resources.Load<GameObject>("Characters/NPC_Server");
				}
				else
				{
					loadedModel = Resources.Load<GameObject>("Characters/NPC_Client");
				}
				hasCalledLoadModel = true;

				useScale = true;

				interactability = EObjectInteractability.NPC;
				chart = EObjectChart.IGNORE;
			}
			else if (type == EObjectType.DECAL)
			{
				hasCalledLoadModel = true;
				float decal_x = p.data.ParseFloat("Decal_X", 1.0f);
				float decal_y = p.data.ParseFloat("Decal_Y", 1.0f);

				if (Dedicator.IsDedicatedServer)
				{
					loadedModel = new GameObject("Decal_Template");
					loadedModel.transform.position = new Vector3(-10000, -10000, -10000);
					loadedModel.hideFlags = HideFlags.HideAndDontSave;
					Object.DontDestroyOnLoad(loadedModel);

					BoxCollider boxCollider = loadedModel.AddComponent<BoxCollider>();
					boxCollider.size = new Vector3(decal_y, decal_x, 1);
				}
				else
				{
					float decalLODBias = 1;
					if (p.data.ContainsKey("Decal_LOD_Bias"))
					{
						decalLODBias = p.data.ParseFloat("Decal_LOD_Bias");
					}

					Texture2D decalTexture = p.bundle.load<Texture2D>("Decal");
					if (decalTexture == null)
					{
						Assets.ReportError(this, "missing 'Decal' Texture2D. It will show as pure white without one.");
					}

					bool decalAlpha = p.data.ContainsKey("Decal_Alpha");
					loadedModel = Object.Instantiate(Resources.Load<GameObject>(decalAlpha ? "Materials/Decal_Template_Alpha" : "Materials/Decal_Template_Masked"));
					loadedModel.transform.position = new Vector3(-10000, -10000, -10000);
					loadedModel.hideFlags = HideFlags.HideAndDontSave;
					Object.DontDestroyOnLoad(loadedModel);

					BoxCollider boxCollider = loadedModel.GetComponent<BoxCollider>();
					boxCollider.size = new Vector3(decal_y, decal_x, 1);

					Decal decalComponent = loadedModel.transform.Find("Decal").GetComponent<Decal>();
					Material deferredMaterial = Object.Instantiate(decalComponent.material);
					deferredMaterial.name = "Decal_Deferred";
					deferredMaterial.hideFlags = HideFlags.DontSave;
					deferredMaterial.SetTexture("_MainTex", decalTexture);
					decalComponent.material = deferredMaterial;
					decalComponent.lodBias = decalLODBias;
					decalComponent.transform.localScale = new Vector3(decal_x, decal_y, 1);

					MeshRenderer meshComponent = loadedModel.transform.Find("Mesh").GetComponent<MeshRenderer>();
					Material forwardMaterial = Object.Instantiate(meshComponent.sharedMaterial);
					forwardMaterial.name = "Decal_Forward";
					forwardMaterial.hideFlags = HideFlags.DontSave;
					forwardMaterial.SetTexture("_MainTex", decalTexture);
					meshComponent.sharedMaterial = forwardMaterial;
					meshComponent.transform.localScale = new Vector3(decal_y, decal_x, 1);
				}

				useScale = true;
				chart = EObjectChart.IGNORE;
			}
			else
			{
				if (p.data.ContainsKey("Interactability"))
				{
					interactability = (EObjectInteractability) System.Enum.Parse(typeof(EObjectInteractability), p.data.GetString("Interactability"), true);
					interactabilityRemote = p.data.ContainsKey("Interactability_Remote");
					interactabilityDelay = p.data.ParseFloat("Interactability_Delay");
					interactabilityReset = p.data.ParseFloat("Interactability_Reset");

					if (p.data.ContainsKey("Interactability_Hint"))
					{
						interactabilityHint = (EObjectInteractabilityHint) System.Enum.Parse(typeof(EObjectInteractabilityHint), p.data.GetString("Interactability_Hint"), true);
					}

					if (interactability == EObjectInteractability.NOTE)
					{
						ushort lines = p.data.ParseUInt16("Interactability_Text_Lines");

						System.Text.StringBuilder builder = new System.Text.StringBuilder();
						for (ushort line = 0; line < lines; line++)
						{
							string text = p.localization.format("Interactability_Text_Line_" + line);
							text = ItemTool.filterRarityRichText(text);
							RichTextUtil.replaceNewlineMarkup(ref text);
							builder.AppendLine(text);
						}

						interactabilityText = builder.ToString();
					}
					else
					{
						// Nelson 2025-11-07: previously this used read() which didn't handle fallbacks.
						interactabilityText = p.localization.FormatOrEmpty("Interact");
						if (string.IsNullOrWhiteSpace(interactabilityText))
						{
							if (interactability == EObjectInteractability.QUEST)
							{
								Assets.ReportError(this, "Interact text empty");
							}
						}
						else
						{
							interactabilityText = ItemTool.filterRarityRichText(interactabilityText);
							RichTextUtil.replaceNewlineMarkup(ref interactabilityText);
						}
					}

					if (interactability == EObjectInteractability.DIALOGUE)
					{
						InteractabilityDialogueDisplayName = p.localization.FormatOrNull("Dialogue_Name");
					}

					if (p.data.ContainsKey("Interactability_Power"))
					{
						interactabilityPower = (EObjectInteractabilityPower) System.Enum.Parse(typeof(EObjectInteractabilityPower), p.data.GetString("Interactability_Power"), true);
					}
					else
					{
						interactabilityPower = EObjectInteractabilityPower.NONE;
					}

					if (p.data.ContainsKey("Interactability_Editor"))
					{
						interactabilityEditor = (EObjectInteractabilityEditor) System.Enum.Parse(typeof(EObjectInteractabilityEditor), p.data.GetString("Interactability_Editor"), true);
					}
					else
					{
						interactabilityEditor = EObjectInteractabilityEditor.NONE;
					}

					if (p.data.ContainsKey("Interactability_Nav"))
					{
						interactabilityNav = (EObjectInteractabilityNav) System.Enum.Parse(typeof(EObjectInteractabilityNav), p.data.GetString("Interactability_Nav"), true);
					}
					else
					{
						interactabilityNav = EObjectInteractabilityNav.NONE;
					}

					interactabilityDrops = new ushort[p.data.ParseUInt8("Interactability_Drops")];
					for (byte index = 0; index < interactabilityDrops.Length; index++)
					{
						interactabilityDrops[index] = p.data.ParseUInt16("Interactability_Drop_" + index);
					}

					interactabilityRewardID = p.data.ParseUInt16("Interactability_Reward_ID");
					interactabilityRewardItemOrigin = p.data.ParseEnum("Interactability_RewardItem_Origin", EItemOrigin.NATURE);
#pragma warning disable
					interactabilityEffect = p.data.ParseGuidOrLegacyId("Interactability_Effect", out interactabilityEffectGuid);
#pragma warning restore

					if (interactability == EObjectInteractability.DIALOGUE)
					{
						interactabilityDialogueRef = p.data.readAssetReference<DialogueAsset>("Interactability_Dialogue");
					}

					if (interactability == EObjectInteractability.BINARY_STATE)
					{
						interactabilityChildPathOverride = p.data.GetString("Interactability_Animation_Component_Path");
						iobsEmissiveMaterialMode = p.data.ParseEnum("Interactability_Emissive_Material_Mode", EInteractableObjectBinaryStateEmissiveMaterialMode.Auto);
					}

					interactabilityConditionsList.Parse(p.data, p.localization, this, "Interactability_Conditions", "Interactability_Condition_");
#pragma warning disable
					interactabilityConditions = interactabilityConditionsList.conditions;
#pragma warning restore

					interactabilityRewards.Parse(p.data, p.localization, this, "Interactability_Rewards", "Interactability_Reward_");

					interactabilityResource = p.data.ParseUInt16("Interactability_Resource");
					interactabilityResourceState = System.BitConverter.GetBytes(interactabilityResource);
				}
				else
				{
					interactability = EObjectInteractability.NONE;
					interactabilityPower = EObjectInteractabilityPower.NONE;
					interactabilityEditor = EObjectInteractabilityEditor.NONE;
				}

				if (interactability == EObjectInteractability.RUBBLE)
				{
					rubble = EObjectRubble.DESTROY;
					rubbleReset = p.data.ParseFloat("Interactability_Reset");
					rubbleHealth = p.data.ParseUInt16("Interactability_Health");
#pragma warning disable
					rubbleEffect = p.data.ParseGuidOrLegacyId("Interactability_Effect", out rubbleEffectGuid);
					rubbleFinale = p.data.ParseGuidOrLegacyId("Interactability_Finale", out rubbleFinaleGuid);
#pragma warning restore
					rubbleRewardID = p.data.ParseUInt16("Interactability_Reward_ID");
					rubbleBladeID = p.data.ParseUInt8("Interactability_Blade_ID");
					rubbleRewardProbability = p.data.ParseFloat("Interactability_Reward_Probability", defaultValue: 1f);
					rubbleRewardsMin = p.data.ParseUInt8("Interactability_Rewards_Min", defaultValue: 1);
					rubbleRewardsMax = p.data.ParseUInt8("Interactability_Rewards_Max", defaultValue: 1);
					rubbleRewardXP = p.data.ParseUInt32("Interactability_Reward_XP");
					rubbleIsVulnerable = !p.data.ContainsKey("Interactability_Invulnerable");
					rubbleProofExplosion = p.data.ContainsKey("Interactability_Proof_Explosion");
					RubbleCanZombiesDamage = p.data.ParseBool("Interactability_Can_Zombies_Damage", false);
					RubbleZombieDamageMultiplier = p.data.ParseFloat("Interactability_Zombie_Damage_Multiplier", defaultValue: 1.0f);
					RubbleSectionDestroyedAlertRadius = p.data.ParseFloat("Interactability_Section_Destroyed_Alert_Radius", defaultValue: -1.0f);
					RubbleAllSectionsDestroyedAlertRadius = p.data.ParseFloat("Interactability_All_Sections_Destroyed_Alert_Radius", defaultValue: -1.0f);
					RubbleRespawnAllSectionsSimultaneously = p.data.ParseBool("Interactability_Respawn_All_Sections_Simultaneously");
					RubbleNavMode = p.data.ParseEnum("Interactability_Nav_Mode", EObjectRubbleNavMode.Unaffected);
				}
				else if (p.data.ContainsKey("Rubble"))
				{
					rubble = (EObjectRubble) System.Enum.Parse(typeof(EObjectRubble), p.data.GetString("Rubble"), true);
					rubbleReset = p.data.ParseFloat("Rubble_Reset");
					rubbleHealth = p.data.ParseUInt16("Rubble_Health");
#pragma warning disable
					rubbleEffect = p.data.ParseGuidOrLegacyId("Rubble_Effect", out rubbleEffectGuid);
					rubbleFinale = p.data.ParseGuidOrLegacyId("Rubble_Finale", out rubbleFinaleGuid);
#pragma warning restore
					rubbleRewardID = p.data.ParseUInt16("Rubble_Reward_ID");
					rubbleBladeID = p.data.ParseUInt8("Rubble_Blade_ID");
					rubbleRewardProbability = p.data.ParseFloat("Rubble_Reward_Probability", defaultValue: 1f);
					rubbleRewardsMin = p.data.ParseUInt8("Rubble_Rewards_Min", defaultValue: 1);
					rubbleRewardsMax = p.data.ParseUInt8("Rubble_Rewards_Max", defaultValue: 1);
					rubbleRewardXP = p.data.ParseUInt32("Rubble_Reward_XP");
					rubbleIsVulnerable = !p.data.ContainsKey("Rubble_Invulnerable");
					rubbleProofExplosion = p.data.ContainsKey("Rubble_Proof_Explosion");
					RubbleCanZombiesDamage = p.data.ParseBool("Rubble_Can_Zombies_Damage", false);
					RubbleZombieDamageMultiplier = p.data.ParseFloat("Rubble_Zombie_Damage_Multiplier", defaultValue: 1.0f);
					RubbleSectionDestroyedAlertRadius = p.data.ParseFloat("Rubble_Section_Destroyed_Alert_Radius", defaultValue: -1.0f);
					RubbleAllSectionsDestroyedAlertRadius = p.data.ParseFloat("Rubble_All_Sections_Destroyed_Alert_Radius", defaultValue: -1.0f);
					RubbleRespawnAllSectionsSimultaneously = p.data.ParseBool("Rubble_Respawn_All_Sections_Simultaneously");
					RubbleNavMode = p.data.ParseEnum("Rubble_Nav_Mode", EObjectRubbleNavMode.Unaffected);

					if (p.data.ContainsKey("Rubble_Editor"))
					{
						rubbleEditor = (EObjectRubbleEditor) System.Enum.Parse(typeof(EObjectRubbleEditor), p.data.GetString("Rubble_Editor"), true);
					}
					else
					{
						rubbleEditor = EObjectRubbleEditor.ALIVE;
					}
				}

				if (Dedicator.IsDedicatedServer && p.data.ParseBool("Has_Clip_Prefab", defaultValue: true))
				{
					p.bundle.loadDeferred("Clip", out legacyServerModel, OnServerModelLoaded);
				}
				// Since we will not know until later whether Clip.prefab exists - and it is considered legacy/deprecated anyway,
				// we always load the client object just in case.
				p.bundle.loadDeferred("Object", out clientModel, OnClientModelLoaded);

				if (p.data.ParseBool("Has_Editor_Prefab"))
				{
					p.bundle.loadDeferred("Editor", out editorModel);
				}

				if (!Dedicator.IsDedicatedServer)
				{
					p.bundle.loadDeferred("Skybox", out skyboxGameObject);
				}

				p.bundle.loadDeferred("Nav", out navGameObject, onNavGameObjectLoaded);
				p.bundle.loadDeferred("Slots", out slotsGameObject, onSlotsGameObjectLoaded);
				p.bundle.loadDeferred("Triggers", out triggersGameObject);

				isSnowshoe = p.data.ContainsKey("Snowshoe");

				if (p.data.ContainsKey("Chart"))
				{
					chart = (EObjectChart) System.Enum.Parse(typeof(EObjectChart), p.data.GetString("Chart"), true);
				}
				else
				{
					chart = EObjectChart.NONE;
				}

				isFuel = p.data.ContainsKey("Fuel");
				isRefill = p.data.ContainsKey("Refill");
				isSoft = p.data.ContainsKey("Soft");
				causesFallDamage = p.data.ParseBool("Causes_Fall_Damage", defaultValue: true);

				isCollisionImportant = p.data.ContainsKey("Collision_Important") || type == EObjectType.LARGE;
				shouldExcludeFromCullingVolumes = p.data.ParseBool("Exclude_From_Culling_Volumes");

				if (isFuel || isRefill)
				{
					Assets.ReportError(this, "is using the legacy fuel/water system");
				}

				if (p.data.ContainsKey("LOD"))
				{
#if !DEDICATED_SERVER
					lod = (EObjectLOD) System.Enum.Parse(typeof(EObjectLOD), p.data.GetString("LOD"), true);

					lodBias = p.data.ParseFloat("LOD_Bias");
					if (lodBias < 0.01f)
					{
						lodBias = 1.0f;
					}

					cullingVolumeLocalPositionOffset = p.data.LegacyParseVector3("LOD_Center");
					cullingVolumeSizeOffset = p.data.LegacyParseVector3("LOD_Size");
#endif // !DEDICATED_SERVER
				}

				if (p.data.ContainsKey("Foliage"))
				{
					foliage = new AssetReference<Framework.Foliage.FoliageInfoCollectionAsset>(new System.Guid(p.data.GetString("Foliage")));
				}

				useWaterHeightTransparentSort = p.data.ContainsKey("Use_Water_Height_Transparent_Sort");
				shouldAddNightLightScript = p.data.ContainsKey("Add_Night_Light_Script");
				shouldAddKillTriggers = p.data.ParseBool("Add_Kill_Triggers");
				allowStructures = p.data.ContainsKey("Allow_Structures");

				if (p.data.ContainsKey("Material_Palette"))
				{
					materialPalette = new AssetReference<MaterialPaletteAsset>(p.data.ParseGuid("Material_Palette"));
				}

				if (p.data.ContainsKey("Landmark_Quality"))
				{
					landmarkQuality = (EGraphicQuality) System.Enum.Parse(typeof(EGraphicQuality), p.data.GetString("Landmark_Quality"), true);
					if (landmarkQuality < EGraphicQuality.LOW)
					{
						// 2023-06-16: Some maps had objects set to "Off" as a hack to make them visible when Landmarks
						// were disabled, but they were hidden by the draw distance regardless.
						landmarkQuality = EGraphicQuality.LOW;
					}
				}
				else
				{
					landmarkQuality = EGraphicQuality.LOW;
				}
			}

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

			christmasRedirect = p.data.readAssetReference<ObjectAsset>("Christmas_Redirect");
			halloweenRedirect = p.data.readAssetReference<ObjectAsset>("Halloween_Redirect");

			isGore = p.data.ParseBool("Is_Gore");

			shouldExcludeFromLevelBatching = p.data.ParseBool("Exclude_From_Level_Batching");
			shouldExcludeFromLevelBatching |= type == EObjectType.NPC || type == EObjectType.DECAL;

			bool defaultExcludeFromSatelliteCapture = holidayRestriction != ENPCHoliday.NONE;
			ShouldExcludeFromSatelliteCapture = p.data.ParseBool("Exclude_From_Satellite_Capture", defaultExcludeFromSatelliteCapture);

			bool defaultLoadNav = type == EObjectType.MEDIUM || type == EObjectType.LARGE;
			ShouldLoadNavOnServer = p.data.ParseBool("Load_Nav_On_Server", defaultLoadNav);
			ShouldLoadNavInEditor = p.data.ParseBool("Load_Nav_In_Editor", defaultLoadNav);

			visibilityConditionsList.Parse(p.data, p.localization, this, "Conditions", "Condition_");
#pragma warning disable
			conditions = visibilityConditionsList.conditions;
#pragma warning restore

			IsClutter = p.data.ParseBool("Is_Clutter");

			this.PopulateArmorFalloff(in p); // this. is necessary, at least in current C# version.
		}

		[System.Obsolete("Removed shouldSend parameter")]
		public void applyInteractabilityConditions(Player player, bool shouldSend)
		{
			ApplyInteractabilityConditions(player);
		}

		[System.Obsolete("Removed shouldSend parameter")]
		public void grantInteractabilityRewards(Player player, bool shouldSend)
		{
			GrantInteractabilityRewards(player);
		}
	}
}
