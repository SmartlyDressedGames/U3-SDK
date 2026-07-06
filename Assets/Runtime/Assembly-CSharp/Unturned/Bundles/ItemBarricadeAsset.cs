////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public class ItemBarricadeAsset : ItemPlaceableAsset
	{
		protected GameObject _barricade;
		public GameObject barricade => _barricade;

		[System.Obsolete("Only one of Barricade.prefab or Clip.prefab are loaded now as _barricade")]
		public GameObject clip => _barricade;

		protected GameObject _nav;
		public GameObject nav => _nav;

		protected AudioClip _use;
		public AudioClip use => _use;

		public override byte[] getState(EItemOrigin origin)
		{
			if (build == EBuild.DOOR || build == EBuild.GATE || build == EBuild.SHUTTER || build == EBuild.HATCH)
			{
				return new byte[17];
				// 0-7 owner
				// 8-15 group
				// 16 interact state
			}
			else if (build == EBuild.BED)
			{
				return new byte[8];
			}
			//else if(build == EBuild.STORAGE || build == EBuild.STORAGE_WALL)
			//{
			//	return new byte[17];
			//	// 0-7 owner
			//	// 8-15 group
			//	// 16 interact state
			//}
			else if (build == EBuild.FARM)
			{
				// Nelson 2024-02-05: Previously, this returned a 4 byte array of zeroes.
				// Now we default to the current time so that plants spawned by default
				// can grow properly. (public issue #4320)
				byte[] newState = new byte[4];
				System.BitConverter.TryWriteBytes(newState, Provider.time);
				return newState;
			}
			else if (build == EBuild.TORCH || build == EBuild.CAMPFIRE || build == EBuild.OVEN || build == EBuild.SPOT || build == EBuild.SAFEZONE || build == EBuild.OXYGENATOR || build == EBuild.BARREL_RAIN || build == EBuild.CAGE)
			{
				return new byte[1];
				// lit
			}
			else if (build == EBuild.OIL)
			{
				return new byte[2];
			}
			else if (build == EBuild.SIGN || build == EBuild.SIGN_WALL || build == EBuild.NOTE)
			{
				return new byte[17];
				// 0-7 owner
				// 8-15 group
				// 16 text length
			}
			else if (build == EBuild.STEREO)
			{
				return new byte[17];
				// 0-15 song GUID
				// 16 volume
			}
			else if (build == EBuild.MANNEQUIN)
			{
				return new byte[73];
				// 16 bytes for owner
				// 28 cosmetic int ids
				// 21 bytes for items/quality
				// 7 bytes for 0 length states
			}
			else
			{
				return new byte[0];
			}
		}

		protected EBuild _build;
		public EBuild build => _build;

		protected ushort _health;
		public ushort health => _health;

		protected float _range;
		public float range => _range;

		protected float _radius;
		public float radius => _radius;

		protected float _offset;
		public float offset => _offset;

		private System.Guid _explosionGuid;
		public System.Guid explosionGuid => _explosionGuid;

		protected ushort _explosion;
		public ushort explosion
		{
			[System.Obsolete]
			get => _explosion;
		}

		public EffectAsset FindExplosionEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(_explosionGuid, _explosion);
#pragma warning restore
		}

		/// <summary>
		/// If false this barricade cannot take damage.
		/// </summary>
		public bool canBeDamaged = true;

		/// <summary>
		/// Modded barricades can disable pooling if they have custom incompatible logic.
		/// </summary>
		public bool eligibleForPooling = true;

		protected bool _isLocked;
		public bool isLocked => _isLocked;

		protected bool _isVulnerable;
		public bool isVulnerable => _isVulnerable;

		public EArmorTier armorTier
		{
			get;
			protected set;
		}

		protected bool _bypassClaim;
		public bool bypassClaim => _bypassClaim;

		public bool allowPlacementOnVehicle
		{
			get;
			protected set;
		}

		protected bool _isRepairable;
		public bool isRepairable => _isRepairable;

		protected bool _proofExplosion;
		public bool proofExplosion => _proofExplosion;

		protected bool _isUnpickupable;
		public bool isUnpickupable => _isUnpickupable;

		/// <summary>
		/// Defaults to false, except for explosive charges which bypass claims.
		/// Useful for collectible barricades that raiders can steal without destroying.
		/// </summary>
		public bool shouldBypassPickupOwnership;

		/// <summary>
		/// Defaults to false, except for explosive charges which bypass claims.
		/// If true the item can be placed inside player clip volumes. (out of bounds)
		/// </summary>
		public bool AllowPlacementInsideClipVolumes
		{
			get;
			private set;
		}

		protected bool _isSalvageable;
		public bool isSalvageable => _isSalvageable;

		public float salvageDurationMultiplier
		{
			get;
			protected set;
		}

		protected bool _isSaveable;
		public bool isSaveable => _isSaveable;

		/// <summary>
		/// Should door colliders remain active while animation is playing?
		/// Useful in special cases such as modded elevators, but prone to physics exploits.
		/// </summary>
		public bool allowCollisionWhileAnimating
		{
			get;
			protected set;
		}

		public override bool shouldFriendlySentryTargetUser => true;

#if !DEDICATED_SERVER
		/// <summary>
		/// Optional alternative barricade prefab specifically for the client preview spawned.
		/// </summary>
		public MasterBundleReference<GameObject> placementPreviewRef;
#endif // !DEDICATED_SERVER

		public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
		{
			return safezone.CurrentlyAllowsBuilding;
		}

		public bool useWaterHeightTransparentSort
		{
			get;
			protected set;
		}

		/// <summary>
		/// By default, vehicles with "hooks" (such as the Skycrane) cannot pick up vehicles with barricades attached.
		/// If all barricades on the vehicle set this to true then the vehicle *can* be picked up. Defaults to false.
		/// </summary>
		public bool CanParentVehicleBePickedUp
		{
			get;
			protected set;
		}

		private System.Guid _vehicleGuid;
		/// <summary>
		/// Vehicle to place.
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		public System.Guid VehicleGuid => _vehicleGuid;

		private ushort _vehicleId;
		/// <summary>
		/// Legacy ID of vehicle to place.
		/// Supports redirects by VehicleRedirectorAsset. If redirector's SpawnPaintColor is set, that color is used.
		/// </summary>
		public ushort VehicleId
		{
			[System.Obsolete]
			get => _vehicleId;
		}

		/// <summary>
		/// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
		/// vehicle spawner requires to properly set paint color.
		/// </summary>
		internal Asset FindVehicleAsset()
		{
#pragma warning disable
			return Assets.FindBaseVehicleAssetByGuidOrLegacyId(_vehicleGuid, _vehicleId);
#pragma warning restore
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public CustomSampler instantiationSampler;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (build == EBuild.VEHICLE)
			{
				// Vehicle doesn't really use barricade properties.
				return;
			}

			if (_health > 0)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Health", _health), DescSort_BuildableCommon);
			}

			switch (armorTier)
			{
				case EArmorTier.LOW:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_ArmorTier_Low"), DescSort_BuildableCommon);
					break;
				case EArmorTier.HIGH:
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_ArmorTier_High"), DescSort_BuildableCommon);
					break;
			}

			if (_isUnpickupable)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_CannotPickup"), DescSort_BuildableCommon);
			}
			else if (!_isSalvageable) // Cannot salvage is implied if cannot pickup.
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_CannotSalvage"), DescSort_BuildableCommon);
			}

			if (!isRepairable)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_CannotRepair"), false), DescSort_BuildableCommon + DescSort_Detrimental);
			}

			if (proofExplosion)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_ExplosionProof"), true), DescSort_BuildableCommon + DescSort_Beneficial);
			}

			if (isLocked)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Lockable"), true), DescSort_BuildableCommon + DescSort_Beneficial);
			}

			if (!_isVulnerable)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Invulnerable"), DescSort_BuildableCommon + DescSort_Beneficial);
			}
		}

		/// <summary>
		/// Nelson 2025-09-08: experimentally exposing to PlayerInput for server-side barricade hit validation. If
		/// hasClipPrefab is false then client-supplied colliderTransform must be valid.
		/// </summary>
		internal bool hasClipPrefab;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			bool shouldLoadBarricadePrefab;
			hasClipPrefab = p.data.ParseBool("Has_Clip_Prefab", defaultValue: true);
			if (Dedicator.IsDedicatedServer && hasClipPrefab)
			{
				_barricade = p.bundle.load<GameObject>("Clip");
				if (barricade == null)
				{
					shouldLoadBarricadePrefab = true;
					Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Barricade\" GameObject instead");
				}
				else
				{
					shouldLoadBarricadePrefab = false;
				}
			}
			else
			{
				shouldLoadBarricadePrefab = true;
			}

			// Nelson 2025-04-08: if changing whether barricade is pre-loaded please update the Heat crafting tag
			// backwards compatibility logic.
			if (shouldLoadBarricadePrefab)
			{
				_barricade = p.bundle.load<GameObject>("Barricade");
				if (barricade == null)
				{
					Assets.ReportError(this, "missing \"Barricade\" GameObject");
				}
				else
				{
					if (Dedicator.IsDedicatedServer)
					{
						// Optimize client prefab for server usage.
						ServerPrefabUtil.RemoveClientComponents(_barricade, this);
					}
				}
			}

			if (barricade != null)
			{
				if (Assets.shouldValidateAssets)
				{
					AssetValidation.searchGameObjectForErrors(this, barricade);
				}

				// Zero transform so that we can get bounds easily.
				barricade.transform.localPosition = Vector3.zero;
				barricade.transform.localRotation = Quaternion.identity;
			}

#if !DEDICATED_SERVER
			placementPreviewRef = p.data.readMasterBundleReference<GameObject>("PlacementPreviewPrefab", p.bundle);
#endif // !DEDICATED_SERVER

			_nav = p.bundle.load<GameObject>("Nav");
			_use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "PlacementAudioClip");

			_build = (EBuild) System.Enum.Parse(typeof(EBuild), p.data.GetString("Build"), true);

			if (build == EBuild.DOOR || build == EBuild.GATE || build == EBuild.SHUTTER)
			{
				if (barricade != null && barricade.transform.Find("Placeholder") == null)
				{
					Assets.ReportError(this, "missing 'Placeholder' Collider");
				}
			}

			_health = p.data.ParseUInt16("Health");
			_range = p.data.ParseFloat("Range");
			_radius = p.data.ParseFloat("Radius");
			_offset = p.data.ParseFloat("Offset");

			if (radius > 0.05f && Mathf.Abs(radius - offset) < 0.05f)
			{
				// For some barricades the sphere overlap radius is super close to the surface offset
				// value, so in those cases we give it some extra wiggle-room of 0.05
				_radius -= 0.05f;
			}

			_explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionGuid);

			if (build == EBuild.VEHICLE)
			{
				// Sigh.
				_vehicleId = _explosion;
				_vehicleGuid = _explosionGuid;
			}

			canBeDamaged = p.data.ParseBool("Can_Be_Damaged", defaultValue: true);

			bool defaultEligibleForPooling = build != EBuild.BEACON;
			eligibleForPooling = p.data.ParseBool("Eligible_For_Pooling", defaultValue: defaultEligibleForPooling);

			_isLocked = p.data.ContainsKey("Locked");
			_isVulnerable = p.data.ContainsKey("Vulnerable");

			// Nelson 2024-09-18: Previously, Bypass_Claim was a flag, not a bool. Explosive charges would always bypass
			// claims by default which we got a request about, so now it's a bool that can be turned off if set.
			if (p.data.TryParseBool("Bypass_Claim", out bool bypassClaimValue))
			{
				_bypassClaim = bypassClaimValue;
			}
			else if (p.data.ContainsKey("Bypass_Claim"))
			{
				_bypassClaim = true;
			}
			else
			{
				_bypassClaim = build == EBuild.CHARGE;
			}

			bool defaultAllowPlacementOnVehicle = build != EBuild.BED && build != EBuild.SENTRY && build != EBuild.SENTRY_FREEFORM; // Beds and sentries cannot be placed on most vehicles.
			allowPlacementOnVehicle = p.data.ParseBool("Allow_Placement_On_Vehicle", defaultValue: defaultAllowPlacementOnVehicle);

			_isRepairable = !p.data.ContainsKey("Unrepairable");
			_proofExplosion = p.data.ContainsKey("Proof_Explosion");
			_isUnpickupable = p.data.ContainsKey("Unpickupable");
			shouldBypassPickupOwnership = p.data.ParseBool("Bypass_Pickup_Ownership", defaultValue: build == EBuild.CHARGE);
			AllowPlacementInsideClipVolumes = p.data.ParseBool("Allow_Placement_Inside_Clip_Volumes", defaultValue: build == EBuild.CHARGE);
			_isSalvageable = !p.data.ContainsKey("Unsalvageable");
			salvageDurationMultiplier = p.data.ParseFloat("Salvage_Duration_Multiplier", 1.0f);
			_isSaveable = !p.data.ContainsKey("Unsaveable");
			allowCollisionWhileAnimating = p.data.ParseBool("Allow_Collision_While_Animating", defaultValue: false);
			useWaterHeightTransparentSort = p.data.ContainsKey("Use_Water_Height_Transparent_Sort");

			// Nelson 2025-01-27: 3.24.7.0 update mistakenly released mentioning "CanParentVehicleBePickedUp" when
			// the .dat property was still called "CanVehicleHookWhileAttached". I think "CanParentVehicleBePickedUp"
			// is a nicer name, but we should maintain compatibility with barricades using the other name.
			// (public issue #4842)
			if (p.data.ContainsKey("CanVehicleHookWhileAttached"))
			{
				CanParentVehicleBePickedUp = p.data.ParseBool("CanVehicleHookWhileAttached");
			}
			else
			{
				CanParentVehicleBePickedUp = p.data.ParseBool("CanParentVehicleBePickedUp");
			}

			if (p.data.ContainsKey("Armor_Tier"))
			{
				armorTier = (EArmorTier) System.Enum.Parse(typeof(EArmorTier), p.data.GetString("Armor_Tier"), true);
			}
			else
			{
				if (name.Contains("Metal"))
				{
					armorTier = EArmorTier.HIGH;
				}
				else
				{
					armorTier = EArmorTier.LOW;
				}
			}

			if (build == EBuild.OVEN || build == EBuild.TORCH || build == EBuild.CAMPFIRE)
			{
				if (p.data.ParseBool("RequiresHeatSourceCraftingTagConversion", defaultValue: true)
					&& _barricade != null)
				{
					Transform fire = _barricade.transform.Find("Fire");
					if (fire != null)
					{
						// Add "Heat Source" crafting tag to be visible in item description.
						// Nelson 2025-04-11: the pizza oven which uses the Heat Source tag and an "Oven" tag.
						// We should preserve any tags configured in the asset itself so that automatic
						// heat tag conversion can be used.
						if (PlaceableProvidedCraftingTags == null)
						{
							PlaceableProvidedCraftingTags = new CachingAssetRef[1]
							{
								PowerTool.VanillaCraftingHeatTag,
							};
						}
						else if (Assets.shouldValidateAssets)
						{
							bool hasVanillaHeatTag = false;
							foreach (CachingAssetRef tagRef in PlaceableProvidedCraftingTags)
							{
								if (tagRef == PowerTool.VanillaCraftingHeatTag)
								{
									hasVanillaHeatTag = true;
									break;
								}
							}

							if (!hasVanillaHeatTag)
							{
								ReportAssetError("specifies PlaceableProvidedCraftingTags without Heat Source tag but has RequiresHeatSourceCraftingTagConversion enabled");
							}
						}

						// Add modifier which *removes* Heat Source when Fire effect is inactive.
						CraftingTagModifierComponent modifier = fire.gameObject.AddComponent<CraftingTagModifierComponent>();
						modifier.tagGuids = new string[1] { "20f30322bbcc4b01a4f116d22b24c21a" };
						modifier.mode = CraftingTagModifierComponent.EMode.Remove;
						modifier.activationRequirement = CraftingTagModifierComponent.EActivationRequirement.Invert;

						CraftingTagProviderComponent tagProvider = _barricade.GetOrAddComponent<CraftingTagProviderComponent>();
						if (tagProvider.modifiers != null && tagProvider.modifiers.Length > 0)
						{
							ReportAssetError("has RequiresHeatSourceCraftingTagConversion enabled, but barricade already has a CraftingTagProviderComponent attached!");
						}
						tagProvider.modifiers = new CraftingTagModifierComponent[] { modifier };
					}
				}
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			instantiationSampler = CustomSampler.Create(name);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Barricade
			// Game data for Barricade Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Barricade");
			data.Append("GUID", GUID); // Key

			data.Append("Build", build);
			data.Append("Health", health);
			data.Append("Range", range);
			data.Append("Radius", radius);
			data.Append("Offset", offset);
#pragma warning disable
			data.Append("Explosion", explosion);
#pragma warning restore
			data.Append("Can_Be_Damaged", canBeDamaged);
			data.Append("Eligible_For_Pooling", eligibleForPooling);
			data.Append("Locked", isLocked);
			data.Append("Vulnerable", isVulnerable);
			data.Append("Bypass_Claim", bypassClaim);
			data.Append("Allow_Placement_On_Vehicle", allowPlacementOnVehicle);
			data.Append("Unrepairable", !isRepairable); // Get original value.
			data.Append("Proof_Explosion", proofExplosion);
			data.Append("Unpickupable", isUnpickupable);
			data.Append("Bypass_Pickup_Ownership", shouldBypassPickupOwnership);
			data.Append("Allow_Placement_Inside_Clip_Volumes", AllowPlacementInsideClipVolumes);
			data.Append("Unsalvageable", !isSalvageable); // Get original value.
			data.Append("Salvage_Duration_Multiplier", salvageDurationMultiplier);
			data.Append("Unsaveable", !isSaveable); // Get original value.
			data.Append("Allow_Collision_While_Animating", allowCollisionWhileAnimating);
			data.Append("Use_Water_Height_Transparent_Sort", useWaterHeightTransparentSort);
			data.Append("CanParentVehicleBePickedUp", CanParentVehicleBePickedUp);
			data.Append("Armor_Tier", armorTier);
		}

		protected override AudioReference GetDefaultInventoryAudio()
		{
			if (name.Contains("Seed", System.StringComparison.InvariantCultureIgnoreCase))
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/Seeds.asset");
			}
			if (name.Contains("Metal", System.StringComparison.InvariantCultureIgnoreCase))
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallMetal.asset");
			}

			if (size_x <= 1 || size_y <= 1)
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/LightMetalEquipment.asset");
			}
			else if (size_x <= 2 || size_y <= 2)
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/MediumMetalEquipment.asset");
			}
			else
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/HeavyMetalEquipment.asset");
			}
		}
	}
}
