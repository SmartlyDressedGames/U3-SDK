////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class TreeRefComponent : MonoBehaviour, IExplosionDamageable, ICraftingTagProvider
	{
		public ResourceSpawnpoint owner;
		private CraftingTagProviderComponent modHook;

		#region IExplosionDamageable
		public bool Equals(IExplosionDamageable obj)
		{
			return ReferenceEquals(this, obj);
		}

		public bool IsEligibleForExplosionDamage
		{
			get => owner != null && !owner.isDead;
		}

		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
		{
			return CollisionUtil.ClosestPoint(gameObject, explosionCenter, false, DamageTool.EXPLOSION_CLOSEST_POINT_LAYER_MASK);
		}

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
		{
			if (!damageParameters.shouldAffectTrees)
			{
				return;
			}

			Vector3 offset = damageParameters.closestPoint - explosionParameters.point;
			float range = offset.magnitude;
			if (range > explosionParameters.damageRadius)
			{
				return;
			}

			Vector3 normal = offset / range;
			if (damageParameters.LineOfSightTest(explosionParameters.point, normal, range, out RaycastHit block))
			{
				if (block.transform != null && !block.transform.IsChildOf(transform))
				{
					return;
				}
			}

			ResourceManager.damage(transform, normal, explosionParameters.resourceDamage,
				1.0f - (range / explosionParameters.damageRadius), 1, out EPlayerKill kill, out uint xp,
				instigatorSteamID: explosionParameters.killer, damageOrigin: explosionParameters.damageOrigin);

			if (kill != EPlayerKill.NONE)
			{
				damageParameters.kills.Add(kill);
			}
			damageParameters.xp += xp;
		}
		#endregion IExplosionDamageable

		#region ICraftingTagProvider
		public Asset GetTagProviderAsset()
		{
			return owner?.asset;
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
	}

	public class ResourceSpawnpoint
	{
		private static List<Collider> colliders = new List<Collider>();

		[System.Obsolete("Unused index into LevelGround.resources for early versions of the level editor.")]
		public byte type;

		[System.Obsolete("Trees are now saved by asset GUID. Please use the asset property rather than finding asset by legacy ID.")]
		public ushort id;

		public System.Guid guid
		{
			get;
			protected set;
		}

		private float _lastDead;
		public float lastDead => _lastDead;

		public bool checkCanReset(float multiplier)
		{
			return isDead && asset != null && asset.reset > 1 && Time.realtimeSinceStartup - lastDead > asset.reset * multiplier;
		}

		public bool isDead => health == 0;

		private bool areConditionsMet;

		// clientside whether we think it's dead or alive, prevents extra wiping/reviving
		private bool isAlive;

		private Vector3 _point;
		public Vector3 point => _point;

		private bool _isGenerated;
		public bool isGenerated => _isGenerated;

		private Quaternion _angle;
		public Quaternion angle => _angle;

		private Vector3 _scale;
		public Vector3 scale => _scale;

		private ResourceAsset _asset;
		public ResourceAsset asset => _asset;

		internal void SetIsActiveInRegion(bool isActive)
		{
			if (isActiveInRegion != isActive)
			{
				isActiveInRegion = isActive;
				UpdateActive();
			}
		}
		/// <summary>
		/// Tree activation is time-sliced, so this does not necessarily match whether the region is active.
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

		private Transform _model;
		public Transform model => _model;

		private Transform _stump;
		public Transform stump => _stump;

		private Transform _skybox;
		public Transform skybox => _skybox;

		/// <summary>
		/// Can this tree be damaged?
		/// Allows holiday restrictions to be taken into account. (Otherwise holiday trees could be destroyed out of season.)
		/// </summary>
		public bool canBeDamaged => areConditionsMet;

		public ushort health;

		public void askDamage(ushort amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

#if EXPLOSIONDEBUG
			health = 0;
#else
			if (amount >= health)
			{
				health = 0;
			}
			else
			{
				health -= amount;
			}
#endif
		}

		public void wipe()
		{
			if (!isAlive)
			{
				return;
			}
			isAlive = false;
			health = 0;

			UpdateActive();
		}

		public void revive()
		{
			if (isAlive)
			{
				return;
			}
			isAlive = true;
			health = asset?.health ?? 1;

			UpdateActive();
		}

		public void kill(Vector3 ragdoll)
		{
			if (!isAlive)
			{
				return;
			}
			isAlive = false;

			_lastDead = Time.realtimeSinceStartup;

			if (asset != null)
			{
				health = 0;

				if (asset.isForage)
				{
					model.Find("Forage")?.gameObject.SetActive(false);
				}
				else
				{
					if (!Dedicator.IsDedicatedServer && asset.hasDebris && GraphicsSettings.debris)
					{
						ragdoll.y += 8;
						ragdoll.x += Random.Range(-16f, 16f);
						ragdoll.z += Random.Range(-16f, 16f);
						ragdoll *= Player.LocalPlayer != null && Player.LocalPlayer.skills.boost == EPlayerBoost.FLIGHT ? 4 : 2;

						if (model != null && asset.modelGameObject != null)
						{
							Vector3 gibPosition= model.position + model.up * asset.DebrisVerticalOffset;

							GameObject gibPrefab;
							if (asset.debrisGameObject == null)
							{
								gibPrefab = asset.modelGameObject;
							}
							else
							{
								gibPrefab = asset.debrisGameObject;
							}

							Transform gib = Object.Instantiate(gibPrefab, gibPosition, model.rotation).transform;
							gib.name = asset.name + "_Debris";
							gib.localScale = model.localScale;

							gib.tag = "Debris";
							gib.gameObject.layer = LayerMasks.DEBRIS;

							gib.gameObject.AddComponent<Rigidbody>();
							gib.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
							gib.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
							gib.GetComponent<Rigidbody>().AddForce(ragdoll);
							gib.GetComponent<Rigidbody>().drag = 1;
							gib.GetComponent<Rigidbody>().angularDrag = 1;

							//if(asset.isSpeedTree)
							//{
							//	Collider collider = gib.GetComponent<Collider>();
							//	if(collider != null)
							//	{
							//		collider.sharedMaterial = Resources.Load<PhysicMaterial>("Physics/Tree");
							//	}
							//}

							GameObject.Destroy(gib.gameObject, 8f);

							if (stump != null && stump.gameObject.activeSelf && asset.ShouldIgnoreCollisionBetweenStumpAndDebris)
							{
								Collider collider = gib.GetComponent<Collider>();

								if (collider != null)
								{
									stump.GetComponents(colliders);

									for (int index = 0; index < colliders.Count; index++)
									{
										Physics.IgnoreCollision(collider, colliders[index]);
									}
								}
							}
						}
					}
				}
			}

			UpdateActive();
		}

		public void destroy()
		{
			if (model != null)
			{
				Object.Destroy(model.gameObject);
			}

			if (stump != null)
			{
				Object.Destroy(stump.gameObject);
			}

			if (skybox != null)
			{
				Object.Destroy(skybox.gameObject);
			}
		}

		internal Vector3 GetEffectSpawnPosition()
		{
			if (model == null)
			{
				return point;
			}

			Transform effectTransform = model.Find("Effect");
			if (effectTransform != null)
			{
				return effectTransform.position;
			}
			else if (asset.hasDebris)
			{
				return model.position + (Vector3.up * 8);
			}
			else
			{
				return model.position;
			}
		}

		internal void SetIsActiveOverrideForSatelliteCapture(bool isActive)
		{
			if (model != null)
			{
				model.gameObject.SetActive(isActive);
			}

			if (stump != null)
			{
				stump.gameObject.SetActive(false);
			}

			if (skybox != null)
			{
				skybox.gameObject.SetActive(false);
			}
		}

		internal void UpdateActive()
		{
			bool isActiveOrCinematic = isActiveInRegion || GraphicsSettings.WantsCinematicMode;

			bool shouldModelBeVisible = isAlive;
			bool shouldStumpBeVisible = !isAlive;
			if (asset != null && asset.isForage)
			{
				shouldModelBeVisible = true;

				model?.Find("Forage")?.gameObject.SetActive(isAlive);
			}

			bool shouldBeActive = areConditionsMet && (Dedicator.IsDedicatedServer || isActiveOrCinematic);

			if (model != null)
			{
				model.gameObject.SetActive(shouldBeActive && shouldModelBeVisible);
			}
			if (stump != null)
			{
				stump.gameObject.SetActive(shouldBeActive && shouldStumpBeVisible);
			}

			if (!Dedicator.IsDedicatedServer)
			{
				UpdateSkyboxActive();
			}
		}

		internal void UpdateSkyboxActive()
		{
#if !DEDICATED_SERVER

			if (skybox != null)
			{
				bool isLandmarkQualityMet = GraphicsSettings.landmarkQuality >= EGraphicQuality.MEDIUM
					&& !GraphicsSettings.WantsCinematicMode;
				skybox.gameObject.SetActive(!isActiveInRegion && isSkyboxActiveInRegion && isLandmarkQualityMet
					&& areConditionsMet && isAlive);
			}

#endif // !DEDICATED_SERVER
		}

		public ResourceSpawnpoint(byte newType, ushort newID, System.Guid newGuid, Vector3 newPoint, Quaternion newRotation, Vector3 newScale, bool newGenerated, NetId netId)
		{
#pragma warning disable
			type = newType;
			id = newID;
#pragma warning restore
			guid = newGuid;
			_point = newPoint;
			_angle = newRotation;
			_scale = newScale;
			_isGenerated = newGenerated;

			if (guid == System.Guid.Empty)
			{
				// We don't worry about asset integrity in this branch because legacy ID might mismatch on client and server.
				// Players can't use this to cheat because a modified map without GUIDs wouldn't match the server's Objects.dat file hash.

#pragma warning disable
				_asset = Assets.find(EAssetType.RESOURCE, id) as ResourceAsset;
				if (asset != null)
				{
					UnturnedLog.info($"Tree without GUID loaded by legacy ID {id}, updating to {asset.GUID:N} \"{asset.FriendlyName}\"");
					guid = asset.GUID;
				}
#pragma warning restore
			}
			else
			{
				_asset = Assets.find(guid) as ResourceAsset;

				if (!Dedicator.IsDedicatedServer)
				{
					// Regardless of whether asset was found we submit this first asset prior to fallback.
					// If the asset is missing on the server no worries because ServerAddKnownMissingAsset is called in the next branch.
					ClientAssetIntegrity.QueueRequest(guid, asset, "Tree");
				}

				if (asset == null)
				{
					// If tree is missing on the server then do not kick clients for missing it as well.
					ClientAssetIntegrity.ServerAddKnownMissingAsset(guid, "Tree");
				}
			}

			if (asset != null)
			{
				health = asset.health;
				isAlive = true;
				areConditionsMet = true;

				GameObject modelPrefab = null;
				if (asset.modelGameObject != null)
				{
					modelPrefab = asset.modelGameObject;
				}

				Vector3 modelPosition = point + (Vector3.up * scale.y * asset.verticalOffset);
				if (modelPrefab != null)
				{
					GameObject modelGameObject = Object.Instantiate(modelPrefab, modelPosition, angle);
					_model = modelGameObject.transform;
					modelGameObject.GetOrAddComponent<TreeRefComponent>().owner = this;
					model.name = asset.name;
					model.localScale = scale;

					if (!Dedicator.IsDedicatedServer)
					{
						if (!Level.isEditor && asset.isForage)
						{
							Transform forageTransform = model.Find("Forage");
							if (forageTransform != null)
							{
								InteractableForage forageable = forageTransform.gameObject.AddComponent<InteractableForage>();
								forageable.asset = asset;
							}
						}

						if (asset.skyboxGameObject != null)
						{
							Quaternion skyboxRotation = angle * Quaternion.Euler(-90.0f, 0.0f, 0.0f);
							GameObject skyboxGameObject = Object.Instantiate(asset.skyboxGameObject, modelPosition, skyboxRotation);
							_skybox = skyboxGameObject.transform;
							skybox.name = asset.name + "_Skybox";
							skybox.localScale = new Vector3(skybox.localScale.x * scale.x, skybox.localScale.z * scale.z, skybox.localScale.z * scale.z);

							if (asset.skyboxMaterial != null)
							{
								skybox.GetComponent<MeshRenderer>().sharedMaterial = asset.skyboxMaterial;
							}
						}
					}

					if (!netId.IsNull())
					{
						NetIdRegistry.AssignTransform(netId, model.transform);
					}
				}

				GameObject stumpGameObject = null;
				if (asset.stumpGameObject != null)
				{
					stumpGameObject = asset.stumpGameObject;
				}

				if (stumpGameObject != null)
				{
					_stump = Object.Instantiate(stumpGameObject, modelPosition, angle).transform;
					stump.name = asset.name + "_Stump";
					stump.localScale = scale;
				}

				if (asset.holidayRestriction != ENPCHoliday.NONE)
				{
					if (!Level.isEditor)
					{
						areConditionsMet = HolidayUtil.isHolidayActive(asset.holidayRestriction);
					}
				}
			}

			UpdateActive();
		}

		public ResourceSpawnpoint(byte newType, ushort newID, Vector3 newPoint, bool newGenerated, NetId netId) : this(newType, newID, System.Guid.Empty, newPoint, Quaternion.identity, Vector3.one, newGenerated, netId)
		{ }

		public ResourceSpawnpoint(ushort newID, Vector3 newPoint, bool newGenerated, NetId netId) : this(0, newID, System.Guid.Empty, newPoint, Quaternion.identity, Vector3.one, newGenerated, netId)
		{ }

		public ResourceSpawnpoint(ushort newID, System.Guid guid, Vector3 newPoint, bool newGenerated, NetId netId) : this(0, newID, guid, newPoint, Quaternion.identity, Vector3.one, newGenerated, netId)
		{ }

		[System.Obsolete("Renamed to isActiveInRegion")]
		public bool isEnabled
		{
			get => isActiveInRegion;
		}

		[System.Obsolete("Renamed to isSkyboxActiveInRegion")]
		public bool isSkyboxEnabled
		{
			get => isSkyboxActiveInRegion;
		}

		[System.Obsolete("Replaced by SetIsActiveInRegion(true)")]
		public void enable()
		{
			SetIsActiveInRegion(true);
		}

		[System.Obsolete("Replaced by SetIsSkyboxActiveInRegion(true)")]
		public void enableSkybox()
		{
			SetIsSkyboxActiveInRegion(true);
		}

		[System.Obsolete("Replaced by SetIsActiveInRegion(false)")]
		public void disable()
		{
			SetIsActiveInRegion(false);
		}

		[System.Obsolete("Replaced by SetIsSkyboxActiveInRegion(false)")]
		public void disableSkybox()
		{
			SetIsSkyboxActiveInRegion(false);
		}

		[System.Obsolete]
		public void forceFullEnable()
		{
			// Nelson 2025-03-27: this was unused in vanilla and seems similar in functionality to this other method.
			SetIsActiveOverrideForSatelliteCapture(true);
		}
	}
}
