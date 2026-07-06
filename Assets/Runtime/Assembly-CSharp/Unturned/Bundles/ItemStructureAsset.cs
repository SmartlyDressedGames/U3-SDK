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
	public class ItemStructureAsset : ItemPlaceableAsset
	{
		protected GameObject _structure;
		public GameObject structure => _structure;

		[System.Obsolete("Only one of Structure.prefab or Clip.prefab are loaded now as _structure")]
		public GameObject clip => _structure;

		protected GameObject _nav;
		public GameObject nav => _nav;

		protected AudioClip _use;
		public AudioClip use => _use;

		protected EConstruct _construct;
		public EConstruct construct => _construct;

		protected ushort _health;
		public ushort health => _health;

		protected float _range;
		public float range => _range;

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
		/// If false this structure cannot take damage.
		/// </summary>
		public bool canBeDamaged = true;

		/// <summary>
		/// Modded structures can disable pooling if they have custom incompatible logic.
		/// </summary>
		public bool eligibleForPooling = true;

		public bool requiresPillars = true;

		protected bool _isVulnerable;
		public bool isVulnerable => _isVulnerable;

		protected bool _isRepairable;
		public bool isRepairable => _isRepairable;

		protected bool _proofExplosion;
		public bool proofExplosion => _proofExplosion;

		protected bool _isUnpickupable;
		public bool isUnpickupable => _isUnpickupable;

		protected bool _isSalvageable;
		public bool isSalvageable => _isSalvageable;

		public float salvageDurationMultiplier
		{
			get;
			protected set;
		}

		protected bool _isSaveable;
		public bool isSaveable => _isSaveable;

		public EArmorTier armorTier
		{
			get;
			protected set;
		}

		public float foliageCutRadius
		{
			get;
			protected set;
		}

		/// <summary>
		/// Length of raycast downward from pivot to check floor is above terrain.
		/// Vanilla floors can be placed a maximum of 10 meters above terrain.
		/// </summary>
		public float terrainTestHeight
		{
			get;
			protected set;
		}

		public override bool shouldFriendlySentryTargetUser => true;

		public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
		{
			return safezone.CurrentlyAllowsBuilding;
		}

#if !DEDICATED_SERVER
		/// <summary>
		/// Optional alternative structure prefab specifically for the client preview spawned.
		/// </summary>
		public MasterBundleReference<GameObject> placementPreviewRef;
#endif // !DEDICATED_SERVER

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public CustomSampler instantiationSampler;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Health", _health), DescSort_BuildableCommon);

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

			if (!_isVulnerable)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Buildable_Invulnerable"), DescSort_BuildableCommon);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			bool shouldLoadStructurePrefab;
			if (Dedicator.IsDedicatedServer && p.data.ParseBool("Has_Clip_Prefab", defaultValue: true))
			{
				_structure = p.bundle.load<GameObject>("Clip");
				if (structure == null)
				{
					shouldLoadStructurePrefab = true;
					Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Structure\" GameObject instead");
				}
				else
				{
					shouldLoadStructurePrefab = false;
					AssetValidation.searchGameObjectForErrors(this, structure);
				}
			}
			else
			{
				shouldLoadStructurePrefab = true;
			}

			if (shouldLoadStructurePrefab)
			{
				_structure = p.bundle.load<GameObject>("Structure");
				if (structure == null)
				{
					Assets.ReportError(this, "missing \"Structure\" GameObject");
				}
				else
				{
					AssetValidation.searchGameObjectForErrors(this, structure);

					if (Dedicator.IsDedicatedServer)
					{
						// Optimize client prefab for server usage.
						ServerPrefabUtil.RemoveClientComponents(_structure, this);
						RemoveClientComponents(_structure);
					}
					else
					{
						// Ensure that regardless of LOD bias or draw distance, entities cannot be seen inside bases.
						LODGroup lodGroup = structure.GetComponent<LODGroup>();
						if (lodGroup != null)
						{
							lodGroup.DisableCulling();
						}
					}
				}
			}

#if !DEDICATED_SERVER
			placementPreviewRef = p.data.readMasterBundleReference<GameObject>("PlacementPreviewPrefab", p.bundle);
#endif // !DEDICATED_SERVER

			_nav = p.bundle.load<GameObject>("Nav");
			_use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "PlacementAudioClip");

			_construct = (EConstruct) System.Enum.Parse(typeof(EConstruct), p.data.GetString("Construct"), true);

			_health = p.data.ParseUInt16("Health");
			_range = p.data.ParseFloat("Range");

			_explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionGuid);
			canBeDamaged = p.data.ParseBool("Can_Be_Damaged", defaultValue: true);
			eligibleForPooling = p.data.ParseBool("Eligible_For_Pooling", defaultValue: true);
			requiresPillars = p.data.ParseBool("Requires_Pillars", defaultValue: true);
			_isVulnerable = p.data.ContainsKey("Vulnerable");
			_isRepairable = !p.data.ContainsKey("Unrepairable");
			_proofExplosion = p.data.ContainsKey("Proof_Explosion");
			_isUnpickupable = p.data.ContainsKey("Unpickupable");
			_isSalvageable = !p.data.ContainsKey("Unsalvageable");
			salvageDurationMultiplier = p.data.ParseFloat("Salvage_Duration_Multiplier", 1.0f);
			_isSaveable = !p.data.ContainsKey("Unsaveable");

			if (p.data.ContainsKey("Armor_Tier"))
			{
				armorTier = (EArmorTier) System.Enum.Parse(typeof(EArmorTier), p.data.GetString("Armor_Tier"), true);
			}
			else
			{
				if (name.Contains("Metal") || name.Contains("Brick"))
				{
					armorTier = EArmorTier.HIGH;
				}
				else
				{
					armorTier = EArmorTier.LOW;
				}
			}

			foliageCutRadius = p.data.ParseFloat("Foliage_Cut_Radius", defaultValue: 6.0f);
			terrainTestHeight = p.data.ParseFloat("Terrain_Test_Height", defaultValue: 10.0f);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			instantiationSampler = CustomSampler.Create(name);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Structure
			// Game data for Structure Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Structure");
			data.Append("GUID", GUID); // Key

			data.Append("Construct", construct);
			data.Append("Health", health);
			data.Append("Range", range);
#pragma warning disable
			data.Append("Explosion", explosion);
#pragma warning restore
			data.Append("Can_Be_Damaged", canBeDamaged);
			data.Append("Eligible_For_Pooling", eligibleForPooling);
			data.Append("Requires_Pillars", requiresPillars);
			data.Append("Vulnerable", isVulnerable);
			data.Append("Unrepairable", !isRepairable); // Get original value.
			data.Append("Proof_Explosion", proofExplosion);
			data.Append("Unpickupable", isUnpickupable);
			data.Append("Unsalvageable", !isSalvageable); // Get original value.
			data.Append("Salvage_Duration_Multiplier", salvageDurationMultiplier);
			data.Append("Unsaveable", !isSaveable); // Get original value.
			data.Append("Armor_Tier", armorTier);
			data.Append("Foliage_Cut_Radius", foliageCutRadius);
			data.Append("Terrain_Test_Height", terrainTestHeight);
		}

		protected override AudioReference GetDefaultInventoryAudio()
		{
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

		/// <summary>
		/// Called on the dedicated server to optimize client prefab for server usage.
		/// </summary>
		private void RemoveClientComponents(GameObject gameObject)
		{
			// Remove invisible snapping colliders which are still used by barricades (as of 2022-06-21), but not used by the server.
			foreach (Transform child in gameObject.transform)
			{
				if (child.name == "Climb" || child.name == "Hatch" || child.name == "Slot" || child.name == "Door" || child.name == "Gate")
				{
					transformsToDestroy.Add(child);
				}
			}

			foreach (Transform child in transformsToDestroy)
			{
				// allowDestroyingAssets is required for destroying components without making an instantiated duplicate.
				Object.DestroyImmediate(child.gameObject, /*allowDestroyingAssets*/ true);
			}

			transformsToDestroy.Clear();
		}
		private static List<Transform> transformsToDestroy = new List<Transform>();
	}
}
