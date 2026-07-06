////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	internal struct PhysicsMaterialCharacterFrictionProperties
	{
		public EPhysicsMaterialCharacterFrictionMode mode;
		public float accelerationMultiplier;
		public float decelerationMultiplier;
		public float maxSpeedMultiplier;
	}

	/// <summary>
	/// Work-in-progress plan to allow modders to create custom physics effects.
	/// </summary>
	internal static class PhysicMaterialCustomData
	{
		public static OneShotAudioDefinition GetAudioDef(string materialName, string propertyName)
		{
			Profiler.BeginSample("PhysicMaterialCustomData.GetAudioDef");
			OneShotAudioDefinition audioDef = null;
			foreach (CombinedPhysicMaterialInfo info in EnumerateInfo(materialName))
			{
				MasterBundleReference<OneShotAudioDefinition> assetRef;
				if (info.audioDefs.TryGetValue(propertyName, out assetRef))
				{
					audioDef = assetRef.loadAsset();
					break;
				}
			}
			Profiler.EndSample();

			return audioDef;
		}

		public static AssetReference<EffectAsset> WipDoNotUseTemp_GetBulletImpactEffect(string materialName)
		{
			Profiler.BeginSample("PhysicMaterialCustomData.GetBulletImpactEffect");
			AssetReference<EffectAsset> effect = default;
			foreach (CombinedPhysicMaterialInfo info in EnumerateInfo(materialName))
			{
				if (info.bulletImpactEffect.isValid)
				{
					effect = info.bulletImpactEffect;
					break;
				}
			}
			Profiler.EndSample();

			return effect;
		}

		public static AssetReference<EffectAsset> GetTireMotionEffect(string materialName)
		{
			Profiler.BeginSample("PhysicMaterialCustomData.GetTireMotionEffect");
			AssetReference<EffectAsset> effect = default;
			foreach (CombinedPhysicMaterialInfo info in EnumerateInfo(materialName))
			{
				if (info.tireMotionEffect.isValid)
				{
					effect = info.tireMotionEffect;
					break;
				}
			}
			Profiler.EndSample();

			return effect;
		}

		public static PhysicsMaterialCharacterFrictionProperties GetCharacterFrictionProperties(string materialName)
		{
			Profiler.BeginSample("PhysicMaterialCustomData.GetSlipMode");

			PhysicsMaterialCharacterFrictionProperties properties;
			properties.mode = EPhysicsMaterialCharacterFrictionMode.ImmediatelyResponsive;
			properties.accelerationMultiplier = 1.0f;
			properties.decelerationMultiplier = 1.0f;
			properties.maxSpeedMultiplier = 1.0f;

			bool hasMode = false;
			bool hasAccel = false;
			bool hasDecel = false;
			bool hasMaxSpeed = false;

			foreach (CombinedPhysicMaterialInfo info in EnumerateInfo(materialName))
			{
				if (!hasMode && info.characterFrictionMode != EPhysicsMaterialCharacterFrictionMode.ImmediatelyResponsive)
				{
					properties.mode = info.characterFrictionMode;
					hasMode = true;
				}

				if (!hasAccel && info.characterAccelerationMultiplier.HasValue)
				{
					hasAccel = true;
					properties.accelerationMultiplier = info.characterAccelerationMultiplier.Value;
				}

				if (!hasDecel && info.characterDecelerationMultiplier.HasValue)
				{
					hasDecel = true;
					properties.decelerationMultiplier = info.characterDecelerationMultiplier.Value;
				}

				if (!hasMaxSpeed && info.characterMaxSpeedMultiplier.HasValue)
				{
					hasMaxSpeed = true;
					properties.maxSpeedMultiplier = info.characterMaxSpeedMultiplier.Value;
				}

				if (hasMode & hasAccel & hasDecel & hasMaxSpeed)
				{
					break;
				}
			}

			Profiler.EndSample();

			return properties;
		}

		/// <summary>
		/// Can crops be planted on a given material?
		/// </summary>
		public static bool IsArable(string materialName)
		{
			Profiler.BeginSample("PhysicMaterialCustomData.IsAirable");
			bool isArable = false;
			foreach (CombinedPhysicMaterialInfo info in EnumerateInfo(materialName))
			{
				if (info.isArable.HasValue)
				{
					isArable = info.isArable.Value;
					break;
				}
			}
			Profiler.EndSample();

			return isArable;
		}

		/// <summary>
		/// Can oil drills be placed on a given material?
		/// </summary>
		public static bool HasOil(string materialName)
		{
			Profiler.BeginSample("PhysicMaterialCustomData.HasOil");
			bool hasOil = false;
			foreach (CombinedPhysicMaterialInfo info in EnumerateInfo(materialName))
			{
				if (info.hasOil.HasValue)
				{
					hasOil = info.hasOil.Value;
					break;
				}
			}
			Profiler.EndSample();

			return hasOil;
		}

		public static void RegisterAsset(PhysicsMaterialAsset asset)
		{
			baseAssets[asset.GUID] = asset;
			needsRebuild = true;
		}

		public static void RegisterAsset(PhysicsMaterialExtensionAsset asset)
		{
			extensionAssets[asset.GUID] = asset;
			needsRebuild = true;
		}

		public static Dictionary<System.Guid, PhysicsMaterialAsset> GetAssets()
		{
			return baseAssets;
		}

		public static Dictionary<System.Guid, PhysicsMaterialExtensionAsset> GetExtensionAssets()
		{
			return extensionAssets;
		}

		private static List<CombinedPhysicMaterialInfo> EnumerateInfo(string materialName)
		{
			Profiler.BeginSample("PhysicMaterialCustomData.EnumerateInfo()");
			enumerableInfos.Clear();

			if (string.IsNullOrEmpty(materialName))
			{
				Profiler.EndSample();
				return enumerableInfos;
			}

			if (needsRebuild)
			{
				needsRebuild = false;
				Rebuild();
			}

			CombinedPhysicMaterialInfo info;
			if (nameInfos.TryGetValue(materialName, out info))
			{
				do
				{
					enumerableInfos.Add(info);
					info = info.fallback;
				}
				while (info != null);
			}

			Profiler.EndSample();
			return enumerableInfos;
		}

		private static void PopulateInfo(CombinedPhysicMaterialInfo info, PhysicsMaterialAssetBase asset)
		{
			if (asset.audioDefs != null)
			{
				foreach (KeyValuePair<string, MasterBundleReference<OneShotAudioDefinition>> pair in asset.audioDefs)
				{
					info.audioDefs.Add(pair.Key, pair.Value);
				}
			}
		}

		private static void Rebuild()
		{
			Profiler.BeginSample("PhysicMaterialCustomData.Rebuild");

			nameInfos.Clear();
			Dictionary<Guid, CombinedPhysicMaterialInfo> baseInfos = new Dictionary<Guid, CombinedPhysicMaterialInfo>();

			// Populate base assets
			foreach (KeyValuePair<Guid, PhysicsMaterialAsset> pair in baseAssets)
			{
				PhysicsMaterialAsset asset = pair.Value;

				CombinedPhysicMaterialInfo combinedInfo = null;
				foreach (string name in asset.physicMaterialNames)
				{
					if (nameInfos.TryGetValue(name, out combinedInfo))
					{
						Assets.ReportError(asset, $"physics material name \"{name}\" already taken by {combinedInfo.baseAsset.name}");
						break;
					}
				}

				if (combinedInfo != null)
					continue;

				if (baseInfos.TryGetValue(asset.GUID, out combinedInfo))
				{
					// How did this happen? Assets already have unique guids during loading.
					Assets.ReportError(asset, $"guid \"{asset.GUID}\" already taken by {combinedInfo.baseAsset.name}");
					continue;
				}

				combinedInfo = new CombinedPhysicMaterialInfo();
				combinedInfo.baseAsset = asset;
				foreach (string name in asset.physicMaterialNames)
				{
					nameInfos[name] = combinedInfo;
				}
				baseInfos[asset.GUID] = combinedInfo;
				combinedInfo.bulletImpactEffect = asset.bulletImpactEffect;
				combinedInfo.tireMotionEffect = asset.tireMotionEffect;
				combinedInfo.characterFrictionMode = asset.characterFrictionMode;
				combinedInfo.isArable = asset.isArable;
				combinedInfo.hasOil = asset.hasOil;
				combinedInfo.characterAccelerationMultiplier = asset.characterAccelerationMultiplier;
				combinedInfo.characterDecelerationMultiplier = asset.characterDecelerationMultiplier;
				combinedInfo.characterMaxSpeedMultiplier = asset.characterMaxSpeedMultiplier;
				PopulateInfo(combinedInfo, asset);
			}

			// Set fallbacks
			foreach (KeyValuePair<string, CombinedPhysicMaterialInfo> pair in nameInfos)
			{
				CombinedPhysicMaterialInfo info = pair.Value;
				if (info.baseAsset.fallbackRef.isValid && !baseInfos.TryGetValue(info.baseAsset.fallbackRef.GUID, out info.fallback))
				{
					Assets.ReportError(info.baseAsset, $"unable to find fallback asset {info.baseAsset.fallbackRef}");
				}
			}

			// Extend base assets
			foreach (KeyValuePair<Guid, PhysicsMaterialExtensionAsset> pair in extensionAssets)
			{
				PhysicsMaterialExtensionAsset extAsset = pair.Value;

				CombinedPhysicMaterialInfo info;
				if (!baseInfos.TryGetValue(extAsset.baseRef.GUID, out info))
				{
					Assets.ReportError(extAsset, $"unable to find base asset {extAsset.baseRef}");
					continue;
				}

				PopulateInfo(info, extAsset);
			}

			Profiler.EndSample();
		}

		private class CombinedPhysicMaterialInfo
		{
			public PhysicsMaterialAsset baseAsset;
			public CombinedPhysicMaterialInfo fallback;
			public Dictionary<string, MasterBundleReference<OneShotAudioDefinition>> audioDefs = new Dictionary<string, MasterBundleReference<OneShotAudioDefinition>>(StringComparer.OrdinalIgnoreCase);
			public AssetReference<EffectAsset> bulletImpactEffect;
			public AssetReference<EffectAsset> tireMotionEffect;
			public EPhysicsMaterialCharacterFrictionMode characterFrictionMode;
			public bool? isArable;
			public bool? hasOil;
			public float? characterAccelerationMultiplier;
			public float? characterDecelerationMultiplier;
			public float? characterMaxSpeedMultiplier;
		}

		private static Dictionary<string, CombinedPhysicMaterialInfo> nameInfos = new Dictionary<string, CombinedPhysicMaterialInfo>(StringComparer.OrdinalIgnoreCase);
		private static Dictionary<Guid, PhysicsMaterialAsset> baseAssets = new Dictionary<Guid, PhysicsMaterialAsset>();
		private static Dictionary<Guid, PhysicsMaterialExtensionAsset> extensionAssets = new Dictionary<Guid, PhysicsMaterialExtensionAsset>();
		private static List<CombinedPhysicMaterialInfo> enumerableInfos = new List<CombinedPhysicMaterialInfo>();
		private static bool needsRebuild = false;
	}
}
