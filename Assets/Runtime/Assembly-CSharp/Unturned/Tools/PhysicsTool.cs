////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
//#define PROFILE_MATERIAL_LOOKUP
#endif
using SDG.Framework.Landscapes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public class PhysicsTool
	{
		/// <summary>
		/// Ehh kind of a stretch to mark this obsolete or for backwards compatibility. Introducing it for road assets
		/// which specify the PhysicMaterial to assign to the colliders. Specifying an asset path is available, but
		/// for the common case we will use the built-in vanilla resources.
		/// </summary>
		public static PhysicMaterial LoadResourceForLegacyMaterial(EPhysicsMaterial material)
		{
			switch (material)
			{
				default:
				case EPhysicsMaterial.NONE:
					return null;

				case EPhysicsMaterial.CLOTH_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Cloth_Dynamic");

				case EPhysicsMaterial.CLOTH_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Cloth_Static");

				case EPhysicsMaterial.TILE_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Tile_Dynamic");

				case EPhysicsMaterial.TILE_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Tile_Static");

				case EPhysicsMaterial.CONCRETE_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Concrete_Dynamic");

				case EPhysicsMaterial.CONCRETE_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Concrete_Static");

				case EPhysicsMaterial.FLESH_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Flesh_Dynamic");

				case EPhysicsMaterial.GRAVEL_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Gravel_Dynamic");

				case EPhysicsMaterial.GRAVEL_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Gravel_Static");

				case EPhysicsMaterial.METAL_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Metal_Dynamic");

				case EPhysicsMaterial.METAL_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Metal_Static");

				case EPhysicsMaterial.METAL_SLIP:
					return Resources.Load<PhysicMaterial>("Physics/Metal_Slip");

				case EPhysicsMaterial.WOOD_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Wood_Dynamic");

				case EPhysicsMaterial.WOOD_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Wood_Static");

				case EPhysicsMaterial.FOLIAGE_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Foliage_Static");

				case EPhysicsMaterial.FOLIAGE_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Foliage_Dynamic");

				case EPhysicsMaterial.SNOW_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Snow_Static");

				case EPhysicsMaterial.ICE_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Ice_Static");

				case EPhysicsMaterial.WATER_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Water");

				case EPhysicsMaterial.ALIEN_DYNAMIC:
					return Resources.Load<PhysicMaterial>("Physics/Alien_Dynamic");

				case EPhysicsMaterial.SAND_STATIC:
					return Resources.Load<PhysicMaterial>("Physics/Sand_Static");
			}
		}

		[System.Obsolete("Intended for backwards compatibility")]
		public static string GetNameOfLegacyMaterial(EPhysicsMaterial material)
		{
			switch (material)
			{
				default:
				case EPhysicsMaterial.NONE:
					return null;

				case EPhysicsMaterial.CLOTH_DYNAMIC:
				case EPhysicsMaterial.CLOTH_STATIC:
					return "Cloth";

				case EPhysicsMaterial.TILE_DYNAMIC:
				case EPhysicsMaterial.TILE_STATIC:
					return "Tile";

				case EPhysicsMaterial.CONCRETE_DYNAMIC:
				case EPhysicsMaterial.CONCRETE_STATIC:
					return "Concrete";

				case EPhysicsMaterial.FLESH_DYNAMIC:
					return "Flesh";

				case EPhysicsMaterial.GRAVEL_DYNAMIC:
				case EPhysicsMaterial.GRAVEL_STATIC:
					return "Gravel";

				case EPhysicsMaterial.METAL_DYNAMIC:
				case EPhysicsMaterial.METAL_STATIC:
				case EPhysicsMaterial.METAL_SLIP:
					return "Metal";

				case EPhysicsMaterial.WOOD_DYNAMIC:
				case EPhysicsMaterial.WOOD_STATIC:
					return "Wood";

				case EPhysicsMaterial.FOLIAGE_STATIC:
				case EPhysicsMaterial.FOLIAGE_DYNAMIC:
					return "Foliage";

				case EPhysicsMaterial.SNOW_STATIC:
					return "Snow";

				case EPhysicsMaterial.ICE_STATIC:
					return "Ice";

				case EPhysicsMaterial.WATER_STATIC:
					return "Water";

				case EPhysicsMaterial.ALIEN_DYNAMIC:
					return "Alien";

				case EPhysicsMaterial.SAND_STATIC:
					return "Sand";
			}
		}

		public static string GetTerrainMaterialName(Vector3 position)
		{
#if PROFILE_MATERIAL_LOOKUP
			Profiler.BeginSample("GetTerrainMaterialName");
#endif
			string result = null;
			AssetReference<LandscapeMaterialAsset> reference;
			if (Landscape.getSplatmapMaterial(position, out reference))
			{
				LandscapeMaterialAsset materialAsset = Assets.find(reference);
				if (materialAsset != null)
				{
					result = materialAsset.physicsMaterialName;
				}
			}
#if PROFILE_MATERIAL_LOOKUP
			Profiler.EndSample();
#endif
			return result;
		}

		[System.Obsolete("Replaced by GetTerrainMaterialName")]
		public static EPhysicsMaterial checkMaterial(Vector3 point)
		{
			AssetReference<LandscapeMaterialAsset> reference;
			if (Landscape.getSplatmapMaterial(point, out reference))
			{
				LandscapeMaterialAsset materialAsset = Assets.find(reference);
				if (materialAsset != null)
				{
					return materialAsset.physicsMaterial;
				}
			}

			return EPhysicsMaterial.NONE;
		}

		[System.Obsolete("Network attachment removes the need for distinction between dynamic and static materials")]
		public static bool isMaterialDynamic(EPhysicsMaterial material)
		{
			switch (material)
			{
				case EPhysicsMaterial.CLOTH_DYNAMIC:
					return true;
				case EPhysicsMaterial.TILE_DYNAMIC:
					return true;
				case EPhysicsMaterial.CONCRETE_DYNAMIC:
					return true;
				case EPhysicsMaterial.FLESH_DYNAMIC:
					return true;
				case EPhysicsMaterial.GRAVEL_DYNAMIC:
					return true;
				case EPhysicsMaterial.METAL_DYNAMIC:
					return true;
				case EPhysicsMaterial.WOOD_DYNAMIC:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Get legacy enum corresponding to Unity physics material object name.
		/// Moved from obsolete <cref>checkMaterial</cref> method.
		/// </summary>
		[System.Obsolete("Intended for backwards compatibility")]
		public static EPhysicsMaterial GetLegacyMaterialByName(string name)
		{
			switch (name)
			{
				case "Cloth":
					return EPhysicsMaterial.CLOTH_STATIC;
				case "Cloth_Dynamic":
					return EPhysicsMaterial.CLOTH_DYNAMIC;
				case "Cloth_Static":
					return EPhysicsMaterial.CLOTH_STATIC;

				case "Tile":
					return EPhysicsMaterial.TILE_STATIC;
				case "Tile_Dynamic":
					return EPhysicsMaterial.TILE_DYNAMIC;
				case "Tile_Static":
					return EPhysicsMaterial.TILE_STATIC;

				case "Concrete":
					return EPhysicsMaterial.CONCRETE_STATIC;
				case "Concrete_Dynamic":
					return EPhysicsMaterial.CONCRETE_DYNAMIC;
				case "Concrete_Static":
					return EPhysicsMaterial.CONCRETE_STATIC;

				case "Flesh":
					return EPhysicsMaterial.FLESH_DYNAMIC;
				case "Flesh_Dynamic":
					return EPhysicsMaterial.FLESH_DYNAMIC;
				case "Flesh_Static":
					return EPhysicsMaterial.FLESH_DYNAMIC;

				case "Gravel":
					return EPhysicsMaterial.GRAVEL_STATIC;
				case "Gravel_Dynamic":
					return EPhysicsMaterial.GRAVEL_DYNAMIC;
				case "Gravel_Static":
					return EPhysicsMaterial.GRAVEL_STATIC;

				case "Metal":
					return EPhysicsMaterial.METAL_STATIC;
				case "Metal_Dynamic":
					return EPhysicsMaterial.METAL_DYNAMIC;
				case "Metal_Static":
					return EPhysicsMaterial.METAL_STATIC;
				case "Metal_Slip":
					return EPhysicsMaterial.METAL_SLIP;

				case "Wood":
					return EPhysicsMaterial.WOOD_STATIC;
				case "Wood_Dynamic":
					return EPhysicsMaterial.WOOD_DYNAMIC;
				case "Wood_Static":
					return EPhysicsMaterial.WOOD_STATIC;

				case "Foliage":
					return EPhysicsMaterial.FOLIAGE_STATIC;
				case "Foliage_Dynamic":
					return EPhysicsMaterial.FOLIAGE_DYNAMIC;
				case "Foliage_Static":
					return EPhysicsMaterial.FOLIAGE_STATIC;

				case "Water":
					return EPhysicsMaterial.WATER_STATIC;
				case "Water_Dynamic":
					return EPhysicsMaterial.WATER_STATIC;
				case "Water_Static":
					return EPhysicsMaterial.WATER_STATIC;

				case "Snow":
					return EPhysicsMaterial.SNOW_STATIC;
				case "Snow_Dynamic":
					return EPhysicsMaterial.SNOW_STATIC;
				case "Snow_Static":
					return EPhysicsMaterial.SNOW_STATIC;

				case "Ice":
					return EPhysicsMaterial.ICE_STATIC;
				case "Ice_Dynamic":
					return EPhysicsMaterial.ICE_STATIC;
				case "Ice_Static":
					return EPhysicsMaterial.ICE_STATIC;

				case "Sand":
					return EPhysicsMaterial.SAND_STATIC;
				case "Sand_Dynamic":
					return EPhysicsMaterial.SAND_STATIC;
				case "Sand_Static":
					return EPhysicsMaterial.SAND_STATIC;

				default:
					return EPhysicsMaterial.NONE;
			}
		}

		[System.Obsolete("Replaced by GetMaterialName")]
		public static EPhysicsMaterial checkMaterial(Collider collider)
		{
			if (collider.sharedMaterial == null)
			{
				return EPhysicsMaterial.NONE;
			}

			return GetLegacyMaterialByName(GetColliderSharedPhysicsMaterialName(collider));
		}

		public static string GetMaterialName(Vector3 point, Transform transform, Collider collider)
		{
			string result;

#if PROFILE_MATERIAL_LOOKUP
			Profiler.BeginSample("GetMaterialName.IsPointUnderwater");
#endif
			bool isUnderwater = SDG.Framework.Water.WaterUtility.isPointUnderwater(point);
#if PROFILE_MATERIAL_LOOKUP
			Profiler.EndSample();
#endif
			if (isUnderwater)
			{
				result = "Water_Static";
			}
			else if (transform != null && transform.CompareTag("Ground"))
			{
				result = GetTerrainMaterialName(point);
			}
			else
			{
				result = GetColliderSharedPhysicsMaterialName(collider);
			}

			return result;
		}

		public static string GetMaterialName(RaycastHit hit)
		{
			return GetMaterialName(hit.point, hit.transform, hit.collider);
		}

		public static string GetMaterialName(WheelHit hit)
		{
			return GetMaterialName(hit.point, hit.collider?.transform, hit.collider);
		}

		/// <summary>
		/// If collider and its physics material are not null, get the physics material's name. Null otherwise.
		/// 
		/// Nelson 2025-04-22: this method may seem silly on first glance. However, I tracked down some every-frame
		/// memory allocation to getting the PhysicMaterial.name property. This method caches the instance ID to
		/// name lookup in a dictionary to avoid that. Note: we don't worry about clearing the dictionary because
		/// there aren't very many physics materials.
		/// </summary>
		public static string GetColliderSharedPhysicsMaterialName(Collider collider)
		{
#if PROFILE_MATERIAL_LOOKUP
			Profiler.BeginSample("GetColliderSharedPhysicsMaterialName");
#endif
			string result = null;

			PhysicMaterial material = collider?.sharedMaterial;
			if (material != null)
			{
				int id = material.GetInstanceID();
				if (!physicsMaterialToName.TryGetValue(id, out result))
				{
					result = material.name;
					physicsMaterialToName[id] = result;
				}
			}
#if PROFILE_MATERIAL_LOOKUP
			Profiler.EndSample();
#endif
			return result;
		}
		private static Dictionary<int, string> physicsMaterialToName = new Dictionary<int, string>();

		internal const int NAME_LENGTH_BITS = 6;
	}
}
