////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class LevelObjectRefComponent : MonoBehaviour, ICraftingTagProvider
	{
		internal LevelObject levelObjectOwner;
		private CraftingTagProviderComponent modHook;

		#region ICraftingTagProvider
		public Asset GetTagProviderAsset()
		{
			return levelObjectOwner?.asset;
		}

		public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
		{
			if (modHook != null)
			{
				p.ApplyModHooks(modHook);
			}
		}

		public bool HasAnyCraftingTagsConfigured()
		{
			return modHook != null;
		}

		public bool Equals(ICraftingTagProvider obj)
		{
			return ReferenceEquals(this, obj);
		}
		#endregion ICraftingTagProvider

		private void Start()
		{
			modHook = GetComponent<CraftingTagProviderComponent>();
		}

		private void OnDestroy()
		{
			if (levelObjectOwner != null)
			{
				levelObjectOwner.OnDestroy();
			}
		}
	}

	public class LevelObject
	{
		private static List<Rigidbody> reuseableRigidbodyList = new List<Rigidbody>();

		/// <summary>
		/// If true, object is within a culling volume.
		/// Name is old and not very specific, but not changing because it's public.
		/// </summary>
		public bool isSpeciallyCulled;

		private bool isDecal;

		private Transform _transform;
		public Transform transform => _transform;

		/// <summary>
		/// Transform created to preserve objects whose assets failed to load.
		/// Separate from default transform to avoid messing with old behavior when transform is null.
		/// </summary>
		public Transform placeholderTransform
		{
			get;
			protected set;
		}

		private Transform _skybox;
		public Transform skybox => _skybox;

		private List<Renderer> renderers;

		private ushort _id;
		public ushort id => _id;

		private System.Guid _GUID;
		public System.Guid GUID => _GUID;

		private uint _instanceID;
		public uint instanceID => _instanceID;

		internal AssetReference<MaterialPaletteAsset> customMaterialOverride;
		internal int materialIndexOverride = -1;

		/// <summary>
		/// If true, <see cref="ownedCullingVolume"/> can be instantiated. Defaults to true.
		/// Enables mappers to remove culling volumes embedded in objects if they're causing issues.
		/// </summary>
		internal bool isOwnedCullingVolumeAllowed;

		public byte[] state;

		private ObjectAsset _asset;
		public ObjectAsset asset => _asset;

		private InteractableObject _interactableObj;
		public InteractableObject interactable => _interactableObj;

		private InteractableObjectRubble _rubble;
		public InteractableObjectRubble rubble => _rubble;

		/// <summary>
		/// Can this object's rubble be damaged?
		/// Allows holiday restrictions to be taken into account. (Otherwise holiday presents could be destroyed out of season.)
		/// </summary>
		public bool canDamageRubble => areConditionsMet;

		public ELevelObjectPlacementOrigin placementOrigin
		{
			get;
			protected set;
		}

		[System.Obsolete]
		public bool isCollisionEnabled
		{
			get;
			private set;
		}

		[System.Obsolete]
		public bool isVisualEnabled
		{
			get;
			private set;
		}

		[System.Obsolete]
		public bool isSkyboxEnabled
		{
			get;
			private set;
		}

		public bool isLandmarkQualityMet
		{
			get
			{
				if (asset == null)
				{
					return false;
				}

				if (Dedicator.IsDedicatedServer)
				{
					return false;
				}

				return GraphicsSettings.landmarkQuality >= asset.landmarkQuality;
			}
		}

#if !DEDICATED_SERVER
		internal CullingVolume ownedCullingVolume;
#endif // !DEDICATED_SERVER

		private bool areConditionsMet;
		private bool haveConditionsBeenChecked;

		internal void SetIsActiveInRegion(bool isActive)
		{
			if (isActiveInRegion != isActive)
			{
				isActiveInRegion = isActive;
				UpdateActiveAndRenderersEnabled();
			}
		}
		/// <summary>
		/// Object activation is time-sliced, so this does not necessarily match whether the region is active.
		/// </summary>
		internal bool isActiveInRegion
		{
			get;
			private set;
		} = false;

		internal void SetIsSkyboxActiveInRegion(bool isActive)
		{
			if (isSkyboxActiveInRegion != isActive)
			{
				isSkyboxActiveInRegion = isActive;
				UpdateSkyboxActive();
			}
		}

		internal bool isSkyboxActiveInRegion
		{
			get;
			private set;
		} = false;

		internal void SetIsVisibleInCullingVolume(bool isVisible)
		{
			if (isVisibleInCullingVolume != isVisible)
			{
				isVisibleInCullingVolume = isVisible;
				UpdateActiveAndRenderersEnabled();
			}
		}

		/// <summary>
		/// Defaults to true because most objects are not inside a culling volume. 
		/// </summary>
		internal bool isVisibleInCullingVolume
		{
			get;
			private set;
		} = true;

		internal void SetIsActiveOverrideForSatelliteCapture(bool isActive)
		{
			if (transform != null)
			{
				transform.gameObject.SetActive(isActive);
			}

			if (skybox != null)
			{
				skybox.gameObject.SetActive(false);
			}

#if !DEDICATED_SERVER
			// Setting to true enables if they were disabled, but disabling is unnecessary
			// when not isActive because gameObject is inactive in that case.
			SetRenderersEnabled(true);
#endif // !DEDICATED_SERVER
		}

		public void destroy()
		{
			if (transform)
			{
				Object.Destroy(transform.gameObject);
			}

			if (skybox)
			{
				Object.Destroy(skybox.gameObject);
			}

#if !DEDICATED_SERVER
			if (ownedCullingVolume != null)
			{
				Object.Destroy(ownedCullingVolume.gameObject);
				ownedCullingVolume = null;
			}
#endif // !DEDICATED_SERVER

			if (_instanceID > 0)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				bool wasRegistered = LevelObjects.instanceIdToObject.TryGetValue(_instanceID, out LevelObject existingObject);
				if (wasRegistered)
				{
					if (existingObject != this)
					{
						UnturnedLog.error($"Destroying object with instance ID {_instanceID} that doesn't match existing registered object!");
					}
				}
				else
				{
					UnturnedLog.error($"Destroying object with instance ID {_instanceID} that wasn't in instanceIdToObject dictionary!");
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

				LevelObjects.instanceIdToObject.Remove(_instanceID);
			}
		}

		internal void ReapplyMaterialOverrides()
		{
			Material materialOverride = GetMaterialOverride();
			if (materialOverride == null)
				return;

			if (skybox != null)
			{
				renderers.Clear();
				skybox.GetComponentsInChildren(true, renderers);
				foreach (Renderer renderer in renderers)
				{
					renderer.sharedMaterial = materialOverride;
				}
			}

			if (transform != null)
			{
				renderers.Clear();
				transform.GetComponentsInChildren(true, renderers);
				foreach (Renderer renderer in renderers)
				{
					renderer.sharedMaterial = materialOverride;
				}
			}
		}

		internal void ReapplyOwnedCullingVolumeAllowed()
		{
#if !DEDICATED_SERVER
			if (asset.lod != EObjectLOD.NONE && isOwnedCullingVolumeAllowed)
			{
				if (ownedCullingVolume == null)
				{
					GameObject volumeGameObject = new GameObject();
					ownedCullingVolume = volumeGameObject.AddComponent<CullingVolume>();
					// We don't call LevelHierarchy.initItem because it shouldn't have an instance ID. (not saved)
					ownedCullingVolume.SetupForLevelObject(this);
				}
			}
			else
			{
				if (ownedCullingVolume != null)
				{
					Object.Destroy(ownedCullingVolume.gameObject);
					ownedCullingVolume = null;
				}
			}
#endif // !DEDICATED_SERVER
		}

		private bool interactableWantsNavActive = true;
		/// <summary>
		/// Used by InteractableObjectBinaryState to indicate whether it wants navGameObject to be active. It's only
		/// active if both IOBS and Rubble want it active.
		/// </summary>
		internal void SetInteractableWantsNavActive(bool wantsNavActive)
		{
			if (interactableWantsNavActive != wantsNavActive)
			{
				interactableWantsNavActive = wantsNavActive;
				UpdateNavActive();
			}
		}

		private bool rubbleWantsNavActive = true;
		/// <summary>
		/// Used by InteractableObjectRubble to indicate whether it wants navGameObject to be active. It's only
		/// active if both IOBS and Rubble want it active.
		/// </summary>
		internal void SetRubbleWantsNavActive(bool wantsNavActive)
		{
			if (rubbleWantsNavActive != wantsNavActive)
			{
				rubbleWantsNavActive = wantsNavActive;
				UpdateNavActive();
			}
		}

		private Material GetMaterialOverride()
		{
			Material materialOverride = null;

			AssetReference<MaterialPaletteAsset> materialPaletteReference = customMaterialOverride;
			if (!materialPaletteReference.isValid)
			{
				materialPaletteReference = asset.materialPalette;
			}

			if (materialPaletteReference.isValid)
			{
				MaterialPaletteAsset materialPalette = Assets.find(materialPaletteReference);
				if (materialPalette != null && materialPalette.materials != null && materialPalette.materials.Count > 0)
				{
					int materialIndex;

					if (materialIndexOverride == -1)
					{
						Random.State seed = Random.state;
						Random.InitState((int) instanceID);
						materialIndex = Random.Range(0, materialPalette.materials.Count);
						Random.state = seed;
					}
					else
					{
						materialIndex = Mathf.Clamp(materialIndexOverride, 0, materialPalette.materials.Count - 1);
					}

					materialOverride = Assets.load(materialPalette.materials[materialIndex]);
					if (materialOverride == null)
					{
						UnturnedLog.warn($"Object \"{asset?.FriendlyName}\" with palette \"{materialPalette.FriendlyName}\" has invalid material at index {materialIndex}");
					}
				}
				else
				{
					string positionString = _transform != null ? _transform.position.ToString() : "(no transform)";
					UnturnedLog.warn($"Object \"{asset?.FriendlyName}\" at {positionString} has invalid material palette {materialPaletteReference}");
				}
			}

			return materialOverride;
		}

		private void updateConditions()
		{
			if (asset == null)
			{
				return;
			}

			bool newAreConditionsMet = true;

			if (!Dedicator.IsDedicatedServer)
			{
				// Only client uses NPC conditions, whereas dedicated server uses this for holiday restrictions.
				newAreConditionsMet = asset.areConditionsMet(Player.LocalPlayer);

				// Disable "gore" objects like blood splatters.
				newAreConditionsMet &= OptionsSettings.EnableGore || !asset.isGore;
			}

			if (newAreConditionsMet)
			{
				// Only bother checking holiday if we might be active.
				// Both client and server check for holiday restrictions.
				if (asset.holidayRestriction != ENPCHoliday.NONE)
				{
					newAreConditionsMet = HolidayUtil.isHolidayActive(asset.holidayRestriction);
				}
			}

			if (areConditionsMet != newAreConditionsMet || !haveConditionsBeenChecked)
			{
				areConditionsMet = newAreConditionsMet;
				haveConditionsBeenChecked = true;
				UpdateActiveAndRenderersEnabled();
			}
		}

		private void onExternalConditionsUpdated()
		{
			updateConditions();
		}

		private void OnLocalPlayerQuestsChanged(ushort id)
		{
			updateConditions();
		}

		/// <summary>
		/// Used if the object asset has weather blend alpha conditions.
		/// </summary>
		private void OnWeatherBlendAlphaChanged(WeatherAssetBase weatherAsset, float blendAlpha)
		{
			updateConditions();
		}

		/// <summary>
		/// Used if the object asset has weather status conditions.
		/// </summary>
		private void OnWeatherStatusChanged(WeatherAssetBase weatherAsset, EWeatherStatusChange statusChange)
		{
			updateConditions();
		}

		private void onFlagsUpdated()
		{
			updateConditions();
		}

		/// <summary>
		/// Callback when an individual quest flag changes for the local player.
		/// Refreshes visibility conditions if the flag was relevant to this object.
		/// </summary>
		private void onFlagUpdated(ushort id)
		{
			if (associatedFlags != null && associatedFlags.Contains(id))
			{
				updateConditions();
			}
		}

		private void onPlayerCreated(Player player)
		{
			if (player.channel.IsLocalPlayer)
			{
				Player.onPlayerCreated -= onPlayerCreated;

				bool hasTimeOfDayCondition = false;
				bool hasQuestCondition = false;

				if (asset.visibilityConditionsList.conditions != null)
				{
					foreach (INPCCondition condition in asset.visibilityConditionsList.conditions)
					{
						if (condition is NPCTimeOfDayCondition || condition is NPCIsFullMoonCondition || condition is NPCDateCounterCondition)
						{
							hasTimeOfDayCondition = true;
						}
						else if (condition is NPCQuestCondition)
						{
							hasQuestCondition = true;
						}
					}

					foreach (INPCCondition condition in asset.visibilityConditionsList.conditions)
					{
						if (condition is NPCWeatherBlendAlphaCondition weatherBlendAlphaCondition)
						{
							WeatherEventListenerManager.AddBlendAlphaListener(weatherBlendAlphaCondition.weather.GUID, OnWeatherBlendAlphaChanged);
						}
						else if (condition is NPCWeatherStatusCondition weatherStatusCondition)
						{
							WeatherEventListenerManager.AddStatusListener(weatherStatusCondition.weather.GUID, OnWeatherStatusChanged);
						}
					}
				}

				if (hasTimeOfDayCondition)
				{
					Player.LocalPlayer.quests.onExternalConditionsUpdated += onExternalConditionsUpdated;
				}

				Player.LocalPlayer.quests.onFlagsUpdated += onFlagsUpdated;

				// Unfortunately this is done per-object rather than per-asset for now because
				// it depends which assets are loaded at the time it is called. In particular
				// NPCQuestCondition needs to look up the quest asset when using GUIDs. 
				associatedFlags = asset.GetConditionAssociatedFlags();
				if (associatedFlags != null)
				{
					Player.LocalPlayer.quests.onFlagUpdated += onFlagUpdated;
				}

				if (hasQuestCondition)
				{
					Player.LocalPlayer.quests.OnLocalPlayerQuestsChanged += OnLocalPlayerQuestsChanged;
				}

				updateConditions();
			}
		}

		private void LoadAsset()
		{
			if (Assets.shouldLoadAnyAssets == false)
			{
				// Early return to prevent spamming log with missing asset warnings.
				_asset = null;
				return;
			}

			if (GUID == System.Guid.Empty)
			{
				// We don't worry about asset integrity in this branch because only really old maps were
				// saved without GUIDs at this time. Players can't use this to cheat because a modified
				// map without GUIDs wouldn't match the server's Objects.dat file hash.

				_asset = Assets.find(EAssetType.OBJECT, id) as ObjectAsset;
				if (asset != null)
				{
					UnturnedLog.info("Object without GUID loaded by legacy ID {0}, updating to {1} \"{2}\"", asset.id, asset.GUID, asset.FriendlyName);
					_GUID = asset.GUID;
				}
				else
				{
					UnturnedLog.warn("Unable to find object by legacy ID {0}", id);
				}
			}
			else
			{
				_asset = Assets.find(new AssetReference<ObjectAsset>(GUID));

				if (!Dedicator.IsDedicatedServer)
				{
					// Regardless of whether asset was found we submit this first asset prior to fallback.
					// If the asset is missing on the server no worries because ServerAddKnownMissingAsset is called in the next branch.
					ClientAssetIntegrity.QueueRequest(GUID, asset, "Object");
				}

				if (asset == null)
				{
					// If object is missing on the server then do not kick clients for missing it as well.
					ClientAssetIntegrity.ServerAddKnownMissingAsset(GUID, "Object");

					// We don't submit an asset integrity check for the fallback!
					// It could be a different object on client and server. Ideally the map would be updated to fix these missing GUIDs.
					_asset = Assets.find(EAssetType.OBJECT, id) as ObjectAsset;
					if (asset != null)
					{
						UnturnedLog.info($"Unable to find object for GUID {GUID:N} found by legacy ID {id}, updating to {asset.GUID:N} \"{asset.FriendlyName}\"");
						_GUID = asset.GUID;
					}
					else
					{
						UnturnedLog.warn($"Unable to find object for GUID {GUID:N}, nor by legacy ID {id}");
					}
				}
			}
		}

		[System.Obsolete]
		public LevelObject(Vector3 newPoint,
							Quaternion newRotation,
							Vector3 newScale,
							ushort newID,
							string newName,
							System.Guid newGUID,
							ELevelObjectPlacementOrigin newPlacementOrigin,
							uint newInstanceID) : this(newPoint,
																					newRotation,
																					newScale,
																					newID,
																					newName,
																					newGUID,
																					newPlacementOrigin,
																					newInstanceID,
																					AssetReference<MaterialPaletteAsset>.invalid,
																					-1,
																					false)
		{ }

		[System.Obsolete]
		public LevelObject(Vector3 newPoint, Quaternion newRotation, Vector3 newScale, ushort newID, string newName, System.Guid newGUID, ELevelObjectPlacementOrigin newPlacementOrigin, uint newInstanceID, AssetReference<MaterialPaletteAsset> customMaterialOverride, int materialIndexOverride, bool newIsHierarchyItem)
			: this(newPoint, newRotation, newScale, newID, newGUID, newPlacementOrigin, newInstanceID, customMaterialOverride, materialIndexOverride, null, NetId.INVALID)
		{ }

		[System.Obsolete]
		internal LevelObject(Vector3 newPoint, Quaternion newRotation, Vector3 newScale, ushort newID, System.Guid newGUID, ELevelObjectPlacementOrigin newPlacementOrigin, uint newInstanceID, AssetReference<MaterialPaletteAsset> customMaterialOverride, int materialIndexOverride, DevkitHierarchyWorldObject devkitOwner, NetId netId)
			: this(newPoint, newRotation, newScale, newID, newGUID, newPlacementOrigin, newInstanceID, customMaterialOverride, materialIndexOverride, netId, true)
		{ }

		internal LevelObject(Vector3 newPoint, Quaternion newRotation, Vector3 newScale, ushort newID, System.Guid newGUID, ELevelObjectPlacementOrigin newPlacementOrigin, uint newInstanceID, AssetReference<MaterialPaletteAsset> customMaterialOverride, int materialIndexOverride, NetId netId, bool isOwnedCullingVolumeAllowed)
		{
			_id = newID;
			_GUID = newGUID;
			_instanceID = newInstanceID;
			placementOrigin = newPlacementOrigin;
			this.customMaterialOverride = customMaterialOverride;
			this.materialIndexOverride = materialIndexOverride;
			this.isOwnedCullingVolumeAllowed = isOwnedCullingVolumeAllowed;

			if (_instanceID > 0)
			{
				LevelObjects.instanceIdToObject[_instanceID] = this;
			}

			LoadAsset(); // Sets _asset based on the id and GUID values.
			if (asset == null)
			{
				if (LevelObjects.preserveMissingAssets)
				{
					placeholderTransform = new GameObject().transform;
					placeholderTransform.position = newPoint;
					placeholderTransform.rotation = newRotation;
					placeholderTransform.localScale = newScale;
				}

				// ID, GUID, transform, placementOrigin and instanceID are saved,
				// so they must be set before this early return to preserve missing assets.
				return;
			}

			if (asset.IsClutter && Level.ShouldSkipInstantiatingClutter)
			{
				return;
			}

			state = asset.getState();

			areConditionsMet = true;
			haveConditionsBeenChecked = false;

			GameObject modelPrefab = asset.GetOrLoadModel(Level.isEditor);

			if (Dedicator.IsDedicatedServer)
			{
				if (modelPrefab != null)
				{
					GameObject modelGameObject = Object.Instantiate(modelPrefab, newPoint, newRotation);
					_transform = modelGameObject.transform;
					modelGameObject.name = asset.name;
					modelGameObject.GetOrAddComponent<LevelObjectRefComponent>().levelObjectOwner = this;
					NetIdRegistry.AssignTransform(netId, _transform);

					isDecal = transform.Find("Decal");
					if (isDecal)
					{
						// Nelson 2023-10-20: decal wasn't getting destroyed properly. (public issue #4145)
						modelGameObject.hideFlags = default;
					}

					if (asset.useScale)
					{
						transform.localScale = newScale;
					}
				}

				renderers = null;
			}
			else if (modelPrefab != null)
			{
				GameObject modelGameObject = Object.Instantiate(modelPrefab, newPoint, newRotation);
				_transform = modelGameObject.transform;
				modelGameObject.name = asset.name;
				modelGameObject.GetOrAddComponent<LevelObjectRefComponent>().levelObjectOwner = this;

				if (!netId.IsNull())
				{
					NetIdRegistry.AssignTransform(netId, _transform);
				}

				isDecal = transform.Find("Decal");
				if (isDecal)
				{
					// Nelson 2023-10-20: decal wasn't getting destroyed properly. (public issue #4145)
					modelGameObject.hideFlags = default;
				}

				if (asset.useScale)
				{
					transform.localScale = newScale;
				}

				if (asset.useWaterHeightTransparentSort)
				{
					transform.gameObject.AddComponent<WaterHeightTransparentSort>();
				}

				if (asset.shouldAddNightLightScript)
				{
					NightLight nightLight = transform.gameObject.AddComponent<NightLight>();
					Transform lightTransform = transform.Find("Light");
					if (lightTransform)
					{
						nightLight.target = lightTransform.GetComponent<Light>();
					}
				}

				renderers = new List<Renderer>();
				Material materialOverride = GetMaterialOverride();

				GameObject skyboxPrefab = asset.skyboxGameObject?.getOrLoad();
				if (skyboxPrefab != null)
				{
					GameObject skyboxGameObject = Object.Instantiate(skyboxPrefab, newPoint, newRotation);
					_skybox = skyboxGameObject.transform;
					skyboxGameObject.name = asset.name + "_Skybox";

					if (asset.useScale)
					{
						skybox.localScale = newScale;
					}

					skybox.GetComponentsInChildren(true, renderers);
					for (int index = 0; index < renderers.Count; index++)
					{
						renderers[index].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

						if (materialOverride != null)
						{
							renderers[index].sharedMaterial = materialOverride;
						}
					}
					renderers.Clear();
				}

				transform.GetComponentsInChildren(true, renderers);
				if (materialOverride != null)
				{
					for (int index = 0; index < renderers.Count; index++)
					{
						renderers[index].sharedMaterial = materialOverride;
					}
				}

				UpdateActiveAndRenderersEnabled();
			}

			if (transform != null)
			{
				if (isDecal && !Level.isEditor && asset.interactability == EObjectInteractability.NONE && asset.rubble == EObjectRubble.NONE)
				{
					Collider collider = transform.GetComponent<Collider>();
					if (collider != null)
					{
						Object.Destroy(collider);
					}
				}

				if (Level.isEditor)
				{
					// Adding rigidbody in the editor allows object to be moved without destroying performance.
					Rigidbody rigidbody = transform.GetOrAddComponent<Rigidbody>();
					rigidbody.useGravity = false;
					rigidbody.isKinematic = true;
				}
				else
				{
					Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
					if (rigidbody != null)
					{
						Object.Destroy(rigidbody);
					}

					// Interactable objects need colliders in order to be triggered, but in-general small objects are purely visual.
					if (asset.type == EObjectType.SMALL && asset.interactability == EObjectInteractability.NONE && asset.rubble == EObjectRubble.NONE)
					{
						Collider collider = transform.GetComponent<Collider>();
						if (collider != null)
						{
							Object.Destroy(collider);
						}
					}
				}

				bool shouldLoadNav = false;
				if (Provider.isServer)
				{
					shouldLoadNav = asset.ShouldLoadNavOnServer;
				}
				else if (Level.isEditor)
				{
					shouldLoadNav = asset.ShouldLoadNavInEditor;
				}

				if (shouldLoadNav)
				{
					GameObject navPrefab = asset.navGameObject?.getOrLoad();
					if (navPrefab != null)
					{
						navGameObject = Object.Instantiate(navPrefab);
						Transform nav = navGameObject.transform;
						nav.name = "Nav";
						nav.parent = transform;
						nav.localPosition = Vector3.zero;
						nav.localRotation = Quaternion.identity;
						nav.localScale = Vector3.one;

						if (Level.isEditor)
						{
							// Adding rigidbody in the editor allows object to be moved without destroying performance.
							Rigidbody rigidbody = nav.GetOrAddComponent<Rigidbody>();
							rigidbody.useGravity = false;
							rigidbody.isKinematic = true;
						}
						else
						{
							reuseableRigidbodyList.Clear();
							nav.GetComponentsInChildren(reuseableRigidbodyList);
							foreach (Rigidbody rigidbody in reuseableRigidbodyList)
							{
								Object.Destroy(rigidbody);
							}
						}
					}
				}

				if (Provider.isServer)
				{
					Transform killSearchTransform;

					GameObject triggersGameObject = asset.triggersGameObject?.getOrLoad();
					if (triggersGameObject != null)
					{
						Transform triggers = Object.Instantiate(triggersGameObject).transform;
						triggers.name = "Triggers";
						triggers.parent = transform;
						triggers.localPosition = Vector3.zero;
						triggers.localRotation = Quaternion.identity;
						triggers.localScale = Vector3.one;
						killSearchTransform = triggers;
					}
					else
					{
						killSearchTransform = transform;
					}

					// Allow custom objects to easily put kill volumes inside walls.
					if (asset.shouldAddKillTriggers)
					{
						foreach (Transform child in killSearchTransform)
						{
							if (child.name.Equals("Kill", System.StringComparison.OrdinalIgnoreCase))
							{
								child.tag = "Trap";
								child.gameObject.layer = LayerMasks.TRAP;
								child.gameObject.AddComponent<Barrier>();
							}
						}
					}
				}

				if (asset.type != EObjectType.SMALL)
				{
					if (Level.isEditor)
					{
						Transform block = transform.Find("Block");

						if (block != null && transform.GetComponent<Collider>() == null)
						{
							BoxCollider blocker = block.GetComponent<BoxCollider>();
							if (blocker != null)
							{
								BoxCollider box = transform.gameObject.AddComponent<BoxCollider>();
								box.center = blocker.center;
								box.size = blocker.size;
							}
						}
					}
					else if (Provider.isClient)
					{
						GameObject slotsGameObject = asset.slotsGameObject?.getOrLoad();
						if (slotsGameObject != null)
						{
							Transform slots = GameObject.Instantiate(slotsGameObject).transform;
							slots.name = "Slots";
							slots.parent = transform;
							slots.localPosition = Vector3.zero;
							slots.localRotation = Quaternion.identity;
							slots.localScale = Vector3.one;

							reuseableRigidbodyList.Clear();
							slots.GetComponentsInChildren(reuseableRigidbodyList);
							foreach (Rigidbody rigidbody in reuseableRigidbodyList)
							{
								Object.Destroy(rigidbody);
							}
						}
					}
				}

				if (asset.interactability != EObjectInteractability.NONE)
				{
					if (asset.interactability == EObjectInteractability.BINARY_STATE)
					{
						_interactableObj = transform.gameObject.AddComponent<InteractableObjectBinaryState>();
					}
					else if (asset.interactability == EObjectInteractability.DROPPER)
					{
						_interactableObj = transform.gameObject.AddComponent<InteractableObjectDropper>();
					}
					else if (asset.interactability == EObjectInteractability.NOTE)
					{
						_interactableObj = transform.gameObject.AddComponent<InteractableObjectNote>();
					}
					else if (asset.interactability == EObjectInteractability.WATER || asset.interactability == EObjectInteractability.FUEL)
					{
						_interactableObj = transform.gameObject.AddComponent<InteractableObjectResource>();
					}
					else if (asset.interactability == EObjectInteractability.NPC)
					{
						_interactableObj = transform.gameObject.AddComponent<InteractableObjectNPC>();
					}
					else if (asset.interactability == EObjectInteractability.QUEST)
					{
						_interactableObj = transform.gameObject.AddComponent<InteractableObjectQuest>();
					}
					else if (asset.interactability == EObjectInteractability.DIALOGUE)
					{
						_interactableObj = transform.gameObject.AddComponent<InteractableObjectDialogue>();
					}

					if (interactable != null)
					{
						interactable.owningLevelObject = this;
						interactable.updateState(asset, state);
					}
				}

				if (asset.rubble != EObjectRubble.NONE)
				{
					if (asset.rubble == EObjectRubble.DESTROY)
					{
						_rubble = transform.gameObject.AddComponent<InteractableObjectRubble>();
						_rubble.owningLevelObject = this;
					}

					if (rubble != null)
					{
						rubble.updateState(asset, state);
					}

					if (asset.rubbleEditor == EObjectRubbleEditor.DEAD && Level.isEditor)
					{
						Transform editor = transform.Find("Editor");
						if (editor != null)
						{
							editor.gameObject.SetActive(true);
						}
					}
				}

				bool waitingForPlayerConditions = false;
				if (!asset.visibilityConditionsList.IsEmpty)
				{
					if (!Level.isEditor && !Dedicator.IsDedicatedServer)
					{
						areConditionsMet = false;

						waitingForPlayerConditions = true;
						Player.onPlayerCreated += onPlayerCreated;
					}
				}

				// If locally using player quest conditions (client) then holiday restriction will be handled once player is available.
				if (!waitingForPlayerConditions && (asset.holidayRestriction != ENPCHoliday.NONE || asset.isGore))
				{
					if (!Level.isEditor)
					{
						// Ran on clients and dedicated server (unlike conditions), but all objects are visible in the editor.
						areConditionsMet = false;
						updateConditions();
					}
				}

				if (!Dedicator.IsDedicatedServer && asset.isGore)
				{
					OptionsSettings.OnEnableGoreChanged += updateConditions;
				}

				if (asset.foliage.isValid)
				{
					SDG.Framework.Foliage.FoliageSurfaceComponent surf = transform.gameObject.AddComponent<SDG.Framework.Foliage.FoliageSurfaceComponent>();
					surf.foliage = asset.foliage;
					surf.surfaceCollider = transform.gameObject.GetComponent<Collider>();
				}

#if !DEDICATED_SERVER
				if (asset.lod != EObjectLOD.NONE && isOwnedCullingVolumeAllowed)
				{
					ReapplyOwnedCullingVolumeAllowed();
				}
#endif // !DEDICATED_SERVER
			}
		}

		internal void UpdateActiveAndRenderersEnabled()
		{
#pragma warning disable
			isCollisionEnabled = isActiveInRegion;
			isVisualEnabled = isActiveInRegion;
#pragma warning restore

			bool isActiveOrCinematic = isActiveInRegion || GraphicsSettings.WantsCinematicMode;

			bool shouldGameObjectBeActive;
			bool shouldRenderersBeEnabled;

			if (isDecal || (asset != null && asset.type == EObjectType.NPC))
			{
				// Decals and NPCs don't have important collision and can directly use root activation.
				// This is convenient for NPC clothes/equipment as well.
				shouldGameObjectBeActive = ((isActiveOrCinematic && (isVisibleInCullingVolume || WantsCullingVolumesOff)) || Dedicator.IsDedicatedServer) && areConditionsMet;

				// Renderers can be ignored because gameObject activation/deactivation affects them.
				shouldRenderersBeEnabled = true;
			}
			else
			{
				bool shouldCollisionAlwaysBeEnabled = (asset != null && asset.isCollisionImportant && Provider.isServer) || Dedicator.IsDedicatedServer;

				// Culling volume is ignored for object activation because item physics may be resting on invisible objects.
				shouldGameObjectBeActive = (isActiveOrCinematic || shouldCollisionAlwaysBeEnabled) && areConditionsMet;

				shouldRenderersBeEnabled = isActiveOrCinematic && (isVisibleInCullingVolume || WantsCullingVolumesOff) && areConditionsMet;
			}

			if (transform != null)
			{
				transform.gameObject.SetActive(shouldGameObjectBeActive);
			}

#if !DEDICATED_SERVER
			SetRenderersEnabled(shouldRenderersBeEnabled);
#endif // !DEDICATED_SERVER

			UpdateSkyboxActive();
		}

		/// <summary>
		/// Separate from UpdateActiveAndRenderersEnabled so graphics settings can call it.
		/// </summary>
		internal void UpdateSkyboxActive()
		{
#pragma warning disable
			isSkyboxEnabled = !isActiveInRegion && isSkyboxActiveInRegion;
#pragma warning restore

#if !DEDICATED_SERVER
			if (skybox != null)
			{
				skybox.gameObject.SetActive(!isActiveInRegion && isSkyboxActiveInRegion && isLandmarkQualityMet
					&& areConditionsMet && !GraphicsSettings.WantsCinematicMode);
			}
#endif // !DEDICATED_SERVER
		}

#if !DEDICATED_SERVER
		private void SetRenderersEnabled(bool isEnabled)
		{
			if (areRenderersEnabled == isEnabled)
				return;
			areRenderersEnabled = isEnabled;

			if (renderers != null)
			{
				foreach (Renderer renderer in renderers)
				{
					if (renderer != null)
					{
						renderer.enabled = areRenderersEnabled;
					}
				}
			}
		}

		/// <summary>
		/// Assume renderers default to enabled.
		/// </summary>
		private bool areRenderersEnabled = true;
#endif // !DEDICATED_SERVER

		private void UpdateNavActive()
		{
			if (navGameObject != null)
			{
				navGameObject.SetActive(interactableWantsNavActive && rubbleWantsNavActive);
			}
		}

		private GameObject navGameObject;
		private HashSet<ushort> associatedFlags;

		private static bool WantsCullingVolumesOff => disableCullingVolumes || GraphicsSettings.WantsCinematicMode;

		private static CommandLineFlag disableCullingVolumes = new CommandLineFlag(false, "-DisableCullingVolumes");

		[System.Obsolete()]
		public string name => null;

		[System.Obsolete("Replaced by SetIsActiveInRegion(true)")]
		public void enableCollision()
		{
			SetIsActiveInRegion(true);
		}

		[System.Obsolete("Replaced by SetIsActiveInRegion(true)")]
		public void enableVisual()
		{
			SetIsActiveInRegion(true);
		}

		[System.Obsolete("Replaced by SetIsActiveInRegion(false)")]
		public void enableSkybox()
		{
			SetIsActiveInRegion(false);
		}

		[System.Obsolete("Replaced by SetIsActiveInRegion(false)")]
		public void disableCollision()
		{
			SetIsActiveInRegion(false);
		}

		[System.Obsolete("Replaced by SetIsActiveInRegion(false)")]
		public void disableVisual()
		{
			SetIsActiveInRegion(false);
		}

		[System.Obsolete("Replaced by SetIsActiveInRegion(true)")]
		public void disableSkybox()
		{
			SetIsActiveInRegion(true);
		}

		/// <summary>
		/// Unlike destroy() this is called when Unity object is destroyed for any reason.
		/// </summary>
		internal void OnDestroy()
		{
			if (asset != null && !Dedicator.IsDedicatedServer && asset.isGore)
			{
				OptionsSettings.OnEnableGoreChanged -= updateConditions;
			}
		}
	}
}
