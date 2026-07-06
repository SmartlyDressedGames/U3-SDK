////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MythicAsset : Asset
	{
		public string particleTagName
		{
			get;
			protected set;
		}

		protected GameObject _systemArea;
		public GameObject systemArea => _systemArea;

		protected GameObject _systemHook;
		public GameObject systemHook => _systemHook;

		protected GameObject _systemFirst;
		public GameObject systemFirst => _systemFirst;

		protected GameObject _systemThird;
		public GameObject systemThird => _systemThird;

		/// <summary>
		/// If true, vest and backpack spawn System_Area instead of System_Hook.
		/// </summary>
		public bool ShouldBodyCosmeticsUseAreaPrefab
		{
			get;
			protected set;
		}

		public override EAssetType assetCategory => EAssetType.MYTHIC;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (id < 500 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 500");
			}

			if (!Dedicator.IsDedicatedServer)
			{
				particleTagName = p.localization.format("Particle_Tag_Name");
				if (string.IsNullOrEmpty(particleTagName))
				{
					particleTagName = name;
				}

				_systemArea = p.bundle.load<GameObject>("System_Area");
				_systemHook = p.bundle.load<GameObject>("System_Hook");
				_systemFirst = p.bundle.load<GameObject>("System_First");
				_systemThird = p.bundle.load<GameObject>("System_Third");

				ShouldBodyCosmeticsUseAreaPrefab = p.data.ParseBool("Body_Cosmetics_Use_System_Area");

				if (Assets.shouldValidateAssets)
				{
					if (systemArea != null) // Shirt or Pants
					{
						AssetValidation.ValidateLayersEqualRecursive(this, systemArea, LayerMasks.ENEMY);
						ValidateRecursively(systemArea.transform);
					}

					if (systemHook != null) // Hat, Mask, Glasses, Backpack, or Vest
					{
						AssetValidation.ValidateLayersEqualRecursive(this, systemHook, LayerMasks.ENEMY);
						ValidateRecursively(systemHook.transform);
					}

					if (systemFirst != null) // 1st-person Weapon
					{
						AssetValidation.ValidateLayersEqualRecursive(this, systemFirst, LayerMasks.VIEWMODEL);
						ValidateRecursively(systemFirst.transform);
					}

					if (systemThird != null) // 3rd-person Weapon
					{
						AssetValidation.ValidateLayersEqualRecursive(this, systemThird, LayerMasks.ITEM);
						ValidateRecursively(systemThird.transform);
					}
				}

				if (systemArea == null && systemHook == null && systemFirst == null && systemThird == null)
				{
					Assets.ReportError(this, "missing all effect prefabs");
				}
			}
		}

		private void ValidateRecursively(Transform transform)
		{
			ParticleSystem ps = transform.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				ParticleSystem.CollisionModule collisionModule = ps.collision;
				if (collisionModule.enabled)
				{
					const int reasonableMask = RayMasks.RESOURCE
						| RayMasks.LARGE
						| RayMasks.MEDIUM
						| RayMasks.ENVIRONMENT
						| RayMasks.GROUND
						| RayMasks.VEHICLE
						| RayMasks.BARRICADE
						| RayMasks.STRUCTURE;
					if (collisionModule.collidesWith != reasonableMask)
					{
						ReportAssetError($"particle system {transform.GetSceneHierarchyPath()} collision mask includes unexpected layers");
					}

					if (!MathfEx.IsNearlyZero(collisionModule.colliderForce))
					{
						ReportAssetError($"particle system {transform.GetSceneHierarchyPath()} should have zero collider force scale");
					}
				}

				if (!ps.useAutoRandomSeed)
				{
					ReportAssetError($"particle system {transform.GetSceneHierarchyPath()} auto random seed is OFF");
				}
			}

			foreach (Transform child in transform)
			{
				ValidateRecursively(child);
			}
		}
	}
}
