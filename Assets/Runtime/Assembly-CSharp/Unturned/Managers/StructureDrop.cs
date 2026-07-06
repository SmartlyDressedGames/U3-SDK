////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal class StructureRefComponent : MonoBehaviour, IExplosionDamageable, ICraftingTagProvider, IOwnershipInfo
	{
		internal StructureDrop tempNotSureIfStructureShouldBeAComponentYet;
		private CraftingTagProviderComponent modHook;

		#region IExplosionDamageable
		public bool Equals(IExplosionDamageable obj)
		{
			return ReferenceEquals(this, obj);
		}

		public bool IsEligibleForExplosionDamage
		{
			get
			{
				StructureDrop drop = tempNotSureIfStructureShouldBeAComponentYet;
				if (drop != null)
				{
					ItemStructureAsset asset = drop.asset;
					if (asset != null && !asset.proofExplosion)
					{
						return true;
					}
				}

				return false;
			}
		}

		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
		{
			return CollisionUtil.ClosestPoint(gameObject, explosionCenter, false, DamageTool.EXPLOSION_CLOSEST_POINT_LAYER_MASK);
		}

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
		{
			if (!damageParameters.shouldAffectStructures)
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

			StructureManager.damage(transform, normal,
				explosionParameters.structureDamage, 1.0f - (range / explosionParameters.damageRadius), true,
				instigatorSteamID: explosionParameters.killer, damageOrigin: explosionParameters.damageOrigin);
		}
		#endregion IExplosionDamageable

		#region ICraftingTagProvider
		public Asset GetTagProviderAsset()
		{
			return tempNotSureIfStructureShouldBeAComponentYet?.asset;
		}

		public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
		{
			ItemPlaceableAsset asset = tempNotSureIfStructureShouldBeAComponentYet?.asset;
			if (asset != null)
			{
				if (asset.PlaceableProvidedCraftingTags != null)
				{
					for (int index = 0; index < asset.PlaceableProvidedCraftingTags.Length; ++index)
					{
						ref CachingAssetRef tagRef = ref asset.PlaceableProvidedCraftingTags[index];
						TagAsset tagAsset = tagRef.Get<TagAsset>();
						if (tagAsset != null)
						{
							p.ResultTags.Add(tagAsset);
						}
					}
				}
			}
			if (modHook != null)
			{
				p.ApplyModHooks(modHook);
			}
		}

		public bool HasAnyCraftingTagsConfigured()
		{
			return modHook != null
				|| !(tempNotSureIfStructureShouldBeAComponentYet?.asset?.PlaceableProvidedCraftingTags.IsNullOrEmpty() ?? true);
		}

		public bool Equals(ICraftingTagProvider obj)
		{
			return ReferenceEquals(this, obj);
		}
		#endregion ICraftingTagProvider

		#region IOwnershipInfo
		public bool TryGetOwnership(out ulong ownerUser, out ulong ownerGroup)
		{
			StructureData serversideData = tempNotSureIfStructureShouldBeAComponentYet?.GetServersideData();
			if (serversideData != null)
			{
				ownerUser = serversideData.owner;
				ownerGroup = serversideData.group;
				return true;
			}
			else
			{
				ownerUser = 0;
				ownerGroup = 0;
				return false;
			}
		}
		#endregion IOwnershipInfo

		private void Start()
		{
			modHook = GetComponent<CraftingTagProviderComponent>();
		}
	}

	public class StructureDrop
	{
		public delegate void SalvageRequestHandler(StructureDrop structure, SteamPlayer instigatorClient, ref bool shouldAllow);
		public static event SalvageRequestHandler OnSalvageRequested_Global;
		private Transform _model;
		public Transform model => _model;
		public uint instanceID =>
		// StructureData needs to be cleaned up, but I made this change here to clarify that instanceID is no
		// longer replicated to client now that NetId is used for identification between client and server.
				serversideData != null ? serversideData.instanceID : 0;

		public ItemStructureAsset asset
		{
			get;
			protected set;
		}

		public StructureData GetServersideData()
		{
			return serversideData;
		}

		public NetId GetNetId()
		{
			return _netId;
		}

		internal void AssignNetId(NetId netId)
		{
			_netId = netId;
			NetIdRegistry.Assign(netId, this);
			NetIdRegistry.AssignTransform(netId + 1, _model);
		}

		internal void ReleaseNetId()
		{
			NetIdRegistry.ReleaseTransform(_netId + 1, _model);
			NetIdRegistry.Release(_netId);
			_netId.Clear();
		}

		internal static readonly ClientInstanceMethod<byte> SendHealth = ClientInstanceMethod<byte>.Get(typeof(StructureDrop), nameof(ReceiveHealth));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveHealth(byte hp)
		{
			Interactable2HP health = model.GetComponent<Interactable2HP>();
			if (health != null)
			{
				health.hp = hp;
			}
		}

		internal static readonly ClientInstanceMethod<byte, byte, Vector3, Quaternion> SendTransform =
			ClientInstanceMethod<byte, byte, Vector3, Quaternion>.Get(typeof(StructureDrop), nameof(ReceiveTransform));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveTransform(in ClientInvocationContext context, byte old_x, byte old_y, [NetPakVector3(fracBitCount: StructureManager.POSITION_FRAC_BIT_COUNT)] Vector3 point, [NetPakSpecialQuaternion(yawBitCount: StructureManager.YAW_BIT_COUNT)] Quaternion rotation)
		{
			// oldRegion is provided by server in case editor locally moved them already.
			StructureRegion oldRegion;
			if (!StructureManager.tryGetRegion(old_x, old_y, out oldRegion))
			{
				context.LogWarning("invalid old region");
				return;
			}

			if (!Provider.isServer)
			{
				if (!oldRegion.isNetworked)
				{
					return;
				}
			}

			// Disconnect and then reconnect after moving.
			try
			{
				StructureManager.housingConnections.UnlinkConnections(this);
			}
			catch (System.Exception e)
			{
				// try/catch because I do not want to risk breaking structures in the big update
				UnturnedLog.exception(e, "Caught exception while unlinking housing connections:");
			}

#if !DEDICATED_SERVER
			bool hasFoliageCut = foliageCut != null;
			if (hasFoliageCut)
			{
				RemoveFoliageCut();
			}
#endif // !DEDICATED_SERVER

			model.position = point;
			model.rotation = rotation;

			try
			{
				StructureManager.housingConnections.LinkConnections(this);
			}
			catch (System.Exception e)
			{
				// try/catch because I do not want to risk breaking structures in the big update
				UnturnedLog.exception(e, "Caught exception while linking housing connections:");
			}

#if !DEDICATED_SERVER
			if (hasFoliageCut)
			{
				AddFoliageCut();
			}
#endif // !DEDICATED_SERVER

			byte new_x;
			byte new_y;

			if (Regions.tryGetCoordinate(point, out new_x, out new_y))
			{
				if (old_x != new_x || old_y != new_y)
				{
					StructureRegion newRegion = StructureManager.regions[new_x, new_y];

					bool wasRemoved = oldRegion.drops.Remove(this);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					if (!wasRemoved)
					{
						UnturnedLog.warn($"Likely bug in StructureDrop.ReceiveTransform: {asset?.FriendlyName} not removed from old cell {old_x}, {old_y} moving to {new_x}, {new_y}");
					}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

					if (newRegion.isNetworked || Provider.isServer)
					{
						newRegion.drops.Add(this);
					}
					else if (!Provider.isServer)
					{
						StructureManager.instance.DestroyOrReleaseStructure(this);
						ReleaseNetId();
					}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
					StructureManager.CheckStructureRegionCoordIsCorrect(this, new_x, new_y, "StructureDrop.ReceiveTransform");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

					if (Provider.isServer)
					{
#pragma warning disable
						oldRegion.structures.Remove(serversideData);
						newRegion.structures.Add(serversideData);
#pragma warning restore
					}
				}
			}

			if (Provider.isServer)
			{
				serversideData.point = point;
				serversideData.rotation = rotation;
			}
		}

		internal static readonly ServerInstanceMethod<Vector3, Quaternion> SendTransformRequest =
			ServerInstanceMethod<Vector3, Quaternion>.Get(typeof(StructureDrop), nameof(ReceiveTransformRequest));
		/// <summary>
		/// Not using rate limit attribute because this is potentially called for hundreds of structures at once,
		/// and only admins will actually be allowed to apply the transform.
		/// </summary>
		[SteamCall(ESteamCallValidation.SERVERSIDE)]
		public void ReceiveTransformRequest(in ServerInvocationContext context, [NetPakVector3(fracBitCount: StructureManager.POSITION_FRAC_BIT_COUNT)] Vector3 point, [NetPakSpecialQuaternion(yawBitCount: StructureManager.YAW_BIT_COUNT)] Quaternion rotation)
		{
			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if (!player.look.canUseWorkzone)
			{
				return;
			}

			// Nelson 2025-07-01: singleplayer moves actual transform, so use serverside stored position.
			if (!Regions.tryGetCoordinate(serversideData.point, out byte old_x, out byte old_y))
			{
				context.LogWarning("invalid old region");
				return;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			StructureManager.CheckStructureRegionCoordIsCorrect(this, old_x, old_y, "StructureDrop.ReceiveTransform");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			if (StructureManager.onTransformRequested != null)
			{
				Vector3 eulerAngles = rotation.eulerAngles;
				byte preEventAngle_x = MeasurementTool.angleToByte(eulerAngles.x);
				byte preEventAngle_y = MeasurementTool.angleToByte(eulerAngles.y);
				byte preEventAngle_z = MeasurementTool.angleToByte(eulerAngles.z);
				byte postEventAngle_x = preEventAngle_x;
				byte postEventAngle_y = preEventAngle_y;
				byte postEventAngle_z = preEventAngle_z;

				bool shouldAllow = true;
				StructureManager.onTransformRequested.Invoke(player.channel.owner.playerID.steamID, old_x, old_y, instanceID, ref point, ref postEventAngle_x, ref postEventAngle_y, ref postEventAngle_z, ref shouldAllow);

				if (postEventAngle_x != preEventAngle_x || postEventAngle_y != preEventAngle_y || postEventAngle_z != preEventAngle_z)
				{
					float angle_x = MeasurementTool.byteToAngle(postEventAngle_x);
					float angle_y = MeasurementTool.byteToAngle(postEventAngle_y);
					float angle_z = MeasurementTool.byteToAngle(postEventAngle_z);
					rotation = Quaternion.Euler(angle_x, angle_y, angle_z);
				}

				if (shouldAllow == false)
				{
					// Editor locally moved them, but was not allowed, so we revert their transform and notify.
					point = serversideData.point;
					rotation = serversideData.rotation;
				}
			}

			StructureManager.InternalSetStructureTransform(old_x, old_y, this, point, rotation);
		}

		private static List<Interactable2SalvageStructure> workingSalvageArray = new List<Interactable2SalvageStructure>();
		internal static readonly ClientInstanceMethod<ulong, ulong> SendOwnerAndGroup =
			ClientInstanceMethod<ulong, ulong>.Get(typeof(StructureDrop), nameof(ReceiveOwnerAndGroup));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveOwnerAndGroup(ulong newOwner, ulong newGroup)
		{
			workingSalvageArray.Clear();
			_model.GetComponentsInChildren(workingSalvageArray);

			foreach (Interactable2SalvageStructure salvage in workingSalvageArray)
			{
				salvage.owner = newOwner;
				salvage.group = newGroup;
			}
		}

		internal static readonly ServerInstanceMethod SendSalvageRequest =
			ServerInstanceMethod.Get(typeof(StructureDrop), nameof(ReceiveSalvageRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10)]
		public void ReceiveSalvageRequest(in ServerInvocationContext context)
		{
			byte x;
			byte y;
			StructureRegion region;
			if (!StructureManager.tryGetRegion(_model, out x, out y, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if ((_model.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			if (!OwnershipTool.checkToggle(player.channel.owner.playerID.steamID, serversideData.owner, player.quests.groupID, serversideData.group))
			{
				return;
			}

			bool shouldAllow = true;
#pragma warning disable
			if (StructureManager.onSalvageStructureRequested != null)
			{
				ushort index = (ushort) region.drops.IndexOf(this);
				StructureManager.onSalvageStructureRequested(player.channel.owner.playerID.steamID, x, y, index, ref shouldAllow);
			}
#pragma warning restore
			OnSalvageRequested_Global?.Invoke(this, context.GetCallingPlayer(), ref shouldAllow);

			if (!shouldAllow)
			{
				return;
			}

			if (asset != null)
			{
				if (asset.isUnpickupable)
				{
					return;
				}

				if (serversideData.structure.health >= asset.health)
				{
					asset.GrantSalvageItems(player, true);
				}
				else if (asset.isSalvageable)
				{
					asset.GrantSalvageItems(player, false);
				}
			}

			StructureManager.destroyStructure(this, x, y, (_model.position - player.transform.position).normalized * 100, true);
		}

		/// <summary>
		/// See BarricadeRegion.FindBarricadeByRootFast comment.
		/// </summary>
		internal static StructureDrop FindByRootFast(Transform rootTransform)
		{
			// Nelson 2024-04-25: If changing this please also update StructureRegion.FindStructureByRootTransform. I'm
			// changing them to use the internal component for the meantime. (public issue #4435)
			return rootTransform.GetComponent<StructureRefComponent>().tempNotSureIfStructureShouldBeAComponentYet;
		}

		/// <summary>
		/// For code which does not know whether transform exists and/or even is part of a house.
		/// See BarricadeRegion.FindBarricadeByRootFast comment.
		/// </summary>
		internal static StructureDrop FindByTransformFastMaybeNull(Transform transform)
		{
			return transform?.root.GetComponent<StructureRefComponent>()?.tempNotSureIfStructureShouldBeAComponentYet;
		}

		internal StructureDrop(Transform newModel, ItemStructureAsset newAsset)
		{
			_model = newModel;
			asset = newAsset;
		}

		[System.Obsolete]
		public StructureDrop(Transform newModel, uint newInstanceID) : this(newModel, null)
		{ }

#if UNITY_EDITOR
		[System.Obsolete("Only for use by net generator tests.")]
		public StructureDrop()
		{ }
#endif // UNITY_EDITOR

#if !DEDICATED_SERVER
		internal void AddFoliageCut()
		{
			if (!Dedicator.IsDedicatedServer && foliageCut == null && asset != null && (asset.construct == EConstruct.FLOOR || asset.construct == EConstruct.FLOOR_POLY) && asset.foliageCutRadius > 0.01f)
			{
				foliageCut = new Framework.Foliage.FoliageCut(model.position, asset.foliageCutRadius, 8);
				SDG.Framework.Foliage.FoliageSystem.AddCut(foliageCut);
			}
		}

		internal void RemoveFoliageCut()
		{
			if (foliageCut != null)
			{
				SDG.Framework.Foliage.FoliageSystem.RemoveCut(foliageCut);
				foliageCut = null;
			}
		}
#endif // !DEDICATED_SERVER

		private NetId _netId;
		internal StructureData serversideData;
		internal HousingConnectionData housingConnectionData;

#if !DEDICATED_SERVER
		internal SDG.Framework.Foliage.FoliageCut foliageCut;
#endif // !DEDICATED_SERVER
	}
}
