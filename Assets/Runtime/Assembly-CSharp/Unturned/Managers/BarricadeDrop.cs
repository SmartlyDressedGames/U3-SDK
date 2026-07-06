////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal class BarricadeRefComponent : MonoBehaviour, IExplosionDamageable, ICraftingTagProvider, IOwnershipInfo
	{
		internal BarricadeDrop tempNotSureIfBarricadeShouldBeAComponentYet;
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
				BarricadeDrop drop = tempNotSureIfBarricadeShouldBeAComponentYet;
				if (drop != null)
				{
					ItemBarricadeAsset asset = drop.asset;
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
			if (!damageParameters.shouldAffectBarricades)
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

			BarricadeManager.damage(transform,
				explosionParameters.barricadeDamage, 1.0f - (range / explosionParameters.damageRadius), true,
				instigatorSteamID: explosionParameters.killer, damageOrigin: explosionParameters.damageOrigin);
		}
		#endregion IExplosionDamageable

		#region ICraftingTagProvider
		public Asset GetTagProviderAsset()
		{
			return tempNotSureIfBarricadeShouldBeAComponentYet?.asset;
		}

		public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
		{
			Profiler.BeginSample("BarricadeRefComponent.GetAvailableTags");

			ItemPlaceableAsset asset = tempNotSureIfBarricadeShouldBeAComponentYet?.asset;
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

			Profiler.EndSample(); // BarricadeRefComponent.GetAvailableTags
		}

		public bool HasAnyCraftingTagsConfigured()
		{
			return modHook != null
				|| !(tempNotSureIfBarricadeShouldBeAComponentYet?.asset?.PlaceableProvidedCraftingTags.IsNullOrEmpty() ?? true);
		}

		public bool Equals(ICraftingTagProvider obj)
		{
			return ReferenceEquals(this, obj);
		}
		#endregion ICraftingTagProvider

		#region IOwnershipInfo
		public bool TryGetOwnership(out ulong ownerUser, out ulong ownerGroup)
		{
			BarricadeData serversideData = tempNotSureIfBarricadeShouldBeAComponentYet?.GetServersideData();
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

	public class BarricadeDrop
	{
		public delegate void SalvageRequestHandler(BarricadeDrop barricade, SteamPlayer instigatorClient, ref bool shouldAllow);
		public static event SalvageRequestHandler OnSalvageRequested_Global;

		private Transform _model;
		public Transform model => _model;

		private Interactable _interactable;
		public Interactable interactable => _interactable;

		public uint instanceID =>
				// BarricadeData needs to be cleaned up, but I made this change here to clarify that instanceID is no
				// longer replicated to client now that NetId is used for identification between client and server.
				serversideData != null ? serversideData.instanceID : 0;

		public ItemBarricadeAsset asset
		{
			get;
			protected set;
		}

		public bool IsChildOfVehicle => _model != null && _model.parent != null && _model.parent.CompareTag("Vehicle");

		public BarricadeData GetServersideData()
		{
			return serversideData;
		}

		public NetId GetNetId()
		{
			return _netId;
		}

#if UNITY_EDITOR
		public
#else
		internal
#endif // UNITY_EDITOR
			void AssignNetId(NetId netId)
		{
			_netId = netId;
			NetIdRegistry.Assign(netId, this);
			NetIdRegistry.AssignTransform(netId + 1, _model);

			if (_interactable != null)
			{
				_interactable.AssignNetId(netId + 2);
			}
		}

#if UNITY_EDITOR
		public
#else
		internal
#endif // UNITY_EDITOR
			void ReleaseNetId()
		{
			if (_interactable != null)
			{
				_interactable.ReleaseNetId();
			}

			NetIdRegistry.ReleaseTransform(_netId + 1, _model);
			NetIdRegistry.Release(_netId);
			_netId.Clear();
		}

#if UNITY_EDITOR
		public
#else
		internal
#endif // UNITY_EDITOR
			static readonly ClientInstanceMethod<byte> SendHealth = ClientInstanceMethod<byte>.Get(typeof(BarricadeDrop), nameof(ReceiveHealth));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveHealth(byte hp)
		{
			Interactable2HP health = model.GetComponent<Interactable2HP>();
			if (health != null)
			{
				health.hp = hp;
			}
		}

		internal static readonly ClientInstanceMethod<byte, byte, ushort, Vector3, Quaternion> SendTransform =
			ClientInstanceMethod<byte, byte, ushort, Vector3, Quaternion>.Get(typeof(BarricadeDrop), nameof(ReceiveTransform));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveTransform(in ClientInvocationContext context, byte old_x, byte old_y, ushort oldPlant, [NetPakVector3(fracBitCount: BarricadeManager.POSITION_FRAC_BIT_COUNT)] Vector3 point, [NetPakSpecialQuaternion(yawBitCount: BarricadeManager.YAW_BIT_COUNT)] Quaternion rotation)
		{
			// oldRegion is provided by server in case editor locally moved them already.
			BarricadeRegion oldRegion;
			if (!BarricadeManager.tryGetRegion(old_x, old_y, oldPlant, out oldRegion))
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

			model.position = point;
			model.rotation = rotation;

			if (oldPlant == ushort.MaxValue)
			{
				byte new_x;
				byte new_y;

				if (Regions.tryGetCoordinate(point, out new_x, out new_y))
				{
					if (old_x != new_x || old_y != new_y)
					{
						BarricadeRegion newRegion = BarricadeManager.regions[new_x, new_y];

						oldRegion.drops.Remove(this);
						if (newRegion.isNetworked || Provider.isServer)
						{
							newRegion.drops.Add(this);
						}
						else if (!Provider.isServer)
						{
							CustomDestroy();
						}

						if (Provider.isServer)
						{
#pragma warning disable
							oldRegion.barricades.Remove(serversideData);
							newRegion.barricades.Add(serversideData);
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
			else
			{
				if (Provider.isServer)
				{
					serversideData.point = model.localPosition;
					serversideData.rotation = model.localRotation;
				}
			}
		}

		internal static readonly ServerInstanceMethod<Vector3, Quaternion> SendTransformRequest = ServerInstanceMethod<Vector3, Quaternion>.Get(typeof(BarricadeDrop), nameof(ReceiveTransformRequest));
		/// <summary>
		/// Not using rate limit attribute because this is potentially called for hundreds of barricades at once,
		/// and only admins will actually be allowed to apply the transform.
		/// </summary>
		[SteamCall(ESteamCallValidation.SERVERSIDE)]
		public void ReceiveTransformRequest(in ServerInvocationContext context, [NetPakVector3(fracBitCount: BarricadeManager.POSITION_FRAC_BIT_COUNT)] Vector3 point, [NetPakSpecialQuaternion(yawBitCount: BarricadeManager.YAW_BIT_COUNT)] Quaternion rotation)
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

			if (_model == null)
			{
				context.LogWarning("Null barricade transform");
				return;
			}

			byte old_x;
			byte old_y;
			ushort oldPlant;
			BarricadeRegion oldRegion;
			if (_model.parent != null && _model.parent.CompareTag("Vehicle"))
			{
				if (!BarricadeManager.tryGetPlant(_model.parent, out old_x, out old_y, out oldPlant, out oldRegion))
				{
					context.LogWarning("invalid old region (has parent)");
					return;
				}
			}
			else
			{
				// Nelson 2025-07-01: singleplayer moves actual transform, so use serverside stored position.
				if (!Regions.tryGetCoordinate(_model.position, out old_x, out old_y))
				{
					context.LogWarning("invalid old region (no parent)");
					return;
				}

				oldPlant = ushort.MaxValue;
			}

			if (BarricadeManager.onTransformRequested != null)
			{
				Vector3 eulerAngles = rotation.eulerAngles;
				byte preEventAngle_x = MeasurementTool.angleToByte(eulerAngles.x);
				byte preEventAngle_y = MeasurementTool.angleToByte(eulerAngles.y);
				byte preEventAngle_z = MeasurementTool.angleToByte(eulerAngles.z);
				byte postEventAngle_x = preEventAngle_x;
				byte postEventAngle_y = preEventAngle_y;
				byte postEventAngle_z = preEventAngle_z;

				bool shouldAllow = true;
				BarricadeManager.onTransformRequested.Invoke(player.channel.owner.playerID.steamID, old_x, old_y, oldPlant, instanceID, ref point, ref postEventAngle_x, ref postEventAngle_y, ref postEventAngle_z, ref shouldAllow);

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
					//
					// 2021-08-11: hack, originally this used the serversideData transform (local space), but the
					// transform RPC operates in worldspace so we need to get the worldspace values.
					point = model.position;
					rotation = model.rotation;
				}
			}

			BarricadeManager.InternalSetBarricadeTransform(old_x, old_y, oldPlant, this, point, rotation);
		}

		private static List<Interactable2SalvageBarricade> workingSalvageArray = new List<Interactable2SalvageBarricade>();
		internal static readonly ClientInstanceMethod<ulong, ulong> SendOwnerAndGroup =
			ClientInstanceMethod<ulong, ulong>.Get(typeof(BarricadeDrop), nameof(ReceiveOwnerAndGroup));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveOwnerAndGroup(ulong newOwner, ulong newGroup)
		{
			workingSalvageArray.Clear();
			model.GetComponentsInChildren(workingSalvageArray);

			foreach (Interactable2SalvageBarricade salvage in workingSalvageArray)
			{
				salvage.owner = newOwner;
				salvage.group = newGroup;
			}
		}

		internal static readonly ClientInstanceMethod<byte[]> SendUpdateState =
			ClientInstanceMethod<byte[]>.Get(typeof(BarricadeDrop), nameof(ReceiveUpdateState));
		/// <summary>
		/// Only used by plugins.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveUpdateState(byte[] newState)
		{
			if (asset == null || interactable == null)
			{
				UnturnedLog.warn("tellBarricadeUpdateState was missing asset/interactable");
				return;
			}

			interactable.updateState(asset, newState);
		}

		internal static readonly ServerInstanceMethod SendSalvageRequest = ServerInstanceMethod.Get(typeof(BarricadeDrop), nameof(ReceiveSalvageRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10)]
		public void ReceiveSalvageRequest(in ServerInvocationContext context)
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

			if ((_model.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			if (!asset.shouldBypassPickupOwnership)
			{
				if (!OwnershipTool.checkToggle(player.channel.owner.playerID.steamID, serversideData.owner, player.quests.groupID, serversideData.group))
				{
					return;
				}
			}

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(_model, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			bool shouldAllow = true;
#pragma warning disable
			if (BarricadeManager.onSalvageBarricadeRequested != null)
			{
				ushort index = (ushort) region.drops.IndexOf(this);
				BarricadeManager.onSalvageBarricadeRequested(player.channel.owner.playerID.steamID, x, y, plant, index, ref shouldAllow);
			}
#pragma warning restore
			OnSalvageRequested_Global?.Invoke(this, context.GetCallingPlayer(), ref shouldAllow);

			if (!shouldAllow)
			{
				return;
			}

			if (asset.isUnpickupable)
			{
				return;
			}

			if (serversideData.barricade.health >= asset.health)
			{
				asset.GrantSalvageItems(player, true);
			}
			else if (asset.isSalvageable)
			{
				asset.GrantSalvageItems(player, false);
			}

			BarricadeManager.destroyBarricade(this, x, y, plant);
		}

		private static List<IManualOnDestroy> destroyEventComponents = new List<IManualOnDestroy>();

		internal void CustomDestroy()
		{
			try
			{
				// This component drops items at current position rather than origin (after moving to zero).
				// It predates pooling, would nowadays probably be called something like "IPooledBarricadeComponent".
				// 2023-03-22: allow multiple components for plugins. (public issue #3773)
				destroyEventComponents.Clear();
				model.GetComponents(destroyEventComponents);
				foreach (IManualOnDestroy component in destroyEventComponents)
				{
					component.ManualOnDestroy();
				}

				ReleaseNetId();
				model.position = Vector3.zero;
				BarricadeManager.instance.DestroyOrReleaseBarricade(asset, model.gameObject);
			}
			catch (System.Exception ex)
			{
				// Catch because it is critical that calling code continues. 
				UnturnedLog.exception(ex, "Exception destroying barricade {0}:", asset);
			}
		}

		/// <summary>
		/// See BarricadeRegion.FindBarricadeByRootFast comment.
		/// </summary>
		internal static BarricadeDrop FindByRootFast(Transform rootTransform)
		{
			return rootTransform.GetComponent<BarricadeRefComponent>().tempNotSureIfBarricadeShouldBeAComponentYet;
		}

		/// <summary>
		/// For code which does not know whether transform exists and/or even is a barricade.
		/// See BarricadeRegion.FindBarricadeByRootFast comment.
		/// </summary>
		internal static BarricadeDrop FindByTransformFastMaybeNull(Transform transform)
		{
			return transform?.root.GetComponent<BarricadeRefComponent>()?.tempNotSureIfBarricadeShouldBeAComponentYet;
		}

#if UNITY_EDITOR
		public
#else
		internal
#endif // UNITY_EDITOR
			BarricadeDrop(Transform newModel, Interactable newInteractable, ItemBarricadeAsset newAsset)
		{
			_model = newModel;
			_interactable = newInteractable;
			asset = newAsset;
		}

		[System.Obsolete]
		public BarricadeDrop(Transform newModel, Interactable newInteractable, uint newInstanceID, ItemBarricadeAsset newAsset)
			: this(newModel, newInteractable, newAsset)
		{ }

#if UNITY_EDITOR
		[System.Obsolete("Only for use by net generator tests.")]
		public BarricadeDrop()
		{ }
#endif // UNITY_EDITOR

		private NetId _netId;
		internal BarricadeData serversideData;
	}
}
