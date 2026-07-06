////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Implemented by assets which gun supports checking for damage falloff.
	/// When implemented, PopulateAsset should call PopulateArmorFalloff.
	/// </summary>
	public interface IArmorFalloff
	{
		/// <summary>
		/// Ranged damage (guns) from greater than this distance finishes decreasing toward falloff multiplier.
		/// Defaults to -1, in which case armor falloff is ignored.
		/// </summary>
		public float ArmorFalloffMaxRange
		{
			get;
			set;
		}

		/// <summary>
		/// Ranged damage (guns) from greater than this distance begins decreasing toward falloff multiplier.
		/// Defaults to ArmorFalloffMaxRange.
		/// </summary>
		public float ArmorFalloffRange
		{
			get;
			set;
		}

		/// <summary>
		/// [0, 1] normalized percentage of incoming damage to apply past IncomingDamageFalloffMaxRange.
		/// </summary>
		public float ArmorFalloffMultiplier
		{
			get;
			set;
		}
	}

	public static class IArmorFalloffEx
	{
		/// <summary>
		/// Should hitmarker be shown client-side for a given range?
		/// </summary>
		public static bool DoesArmorFalloffShowHitmarker(this IArmorFalloff instance, float distance)
		{
			return instance.ArmorFalloffMaxRange < -0.5f || instance.ArmorFalloffMultiplier > 0.00001f || distance < instance.ArmorFalloffMaxRange;
		}

		/// <summary>
		/// Amount to multiply damage by at a given range.
		/// </summary>
		public static float GetArmorFalloffMultiplier(this IArmorFalloff instance, float distance)
		{
			if (instance.ArmorFalloffMaxRange < -0.5f)
			{
				return 1.0f;
			}

			float t = Mathf.InverseLerp(instance.ArmorFalloffRange, instance.ArmorFalloffMaxRange, distance);
			return Mathf.Lerp(1.0f, instance.ArmorFalloffMultiplier, t);
		}

		public static void PopulateArmorFalloff(this IArmorFalloff instance, in PopulateAssetParameters p)
		{
			instance.ArmorFalloffMaxRange = p.data.ParseFloat("Armor_FalloffMaxRange", -1.0f);
			instance.ArmorFalloffRange = p.data.ParseFloat("Armor_FalloffRange", instance.ArmorFalloffMaxRange);
			instance.ArmorFalloffMultiplier = p.data.ParseFloat("Armor_FalloffMultiplier", 1.0f);
		}
	}
}
