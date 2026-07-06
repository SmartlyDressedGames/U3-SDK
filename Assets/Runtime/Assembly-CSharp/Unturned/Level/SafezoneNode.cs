////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SafezoneNode : Node
	{
		public static readonly float MIN_SIZE = 32;
		public static readonly float MAX_SIZE = 1024;

		internal float _normalizedRadius;

		/// <summary>
		/// This value is confusing because in the level editor it is the normalized radius, but in-game it is the square radius.
		/// </summary>
		public float radius
		{
			get
			{
				if (Level.isEditor)
				{
					return _normalizedRadius;
				}
				else
				{
					return MathfEx.Square(CalculateRadiusFromNormalizedRadius(_normalizedRadius));
				}
			}

			set => _normalizedRadius = value;
		}

		public static float CalculateRadiusFromNormalizedRadius(float normalizedRadius)
		{
			return Mathf.Lerp(MIN_SIZE, MAX_SIZE, normalizedRadius) * 0.5f;
		}

		public static float CalculateNormalizedRadiusFromRadius(float radius)
		{
			return Mathf.InverseLerp(MIN_SIZE, MAX_SIZE, radius * 2.0f);
		}

		public bool isHeight;

		/// <summary>
		/// If true, players inside the safezone cannot use items categorized as "weapons" (/hostile).
		/// </summary>
		public bool noWeapons;

		/// <summary>
		/// Please check CurrentlyAllowsBuilding.
		/// Bypassed by LevelAsset's ShouldAllowBuildingInSafezonesInSingleplayer option as well as
		/// Gameplay config's Bypass_Building_In_Safezones option.
		/// </summary>
		public bool noBuildables;

		/// <summary>
		/// If true, players inside the safezone cannot take damage. (Unless damage's bypassSafezone parameter is true.)
		/// For backwards compatibility this is true if noWeapons was true.
		/// </summary>
		public bool noIncomingDamage;

		public bool CurrentlyAllowsBuilding
		{
			get => !noBuildables
				|| (Provider.modeConfigData?.Gameplay?.Bypass_Building_In_Safezones ?? false)
				|| ((Level.getAsset()?.ShouldAllowBuildingInSafezonesInSingleplayer ?? false) && Provider.isServer && !Dedicator.IsDedicatedServer);
		}

		public SafezoneNode(Vector3 newPoint) : this(newPoint, 0f, false, true, true)
		{

		}

		public SafezoneNode(Vector3 newPoint, float newRadius, bool newHeight, bool newNoWeapons, bool newNoBuildables)
		{
			_point = newPoint;

			_normalizedRadius = newRadius;
			isHeight = newHeight;
			noWeapons = newNoWeapons;
			noBuildables = newNoBuildables;
			noIncomingDamage = noWeapons;

			_type = ENodeType.SAFEZONE;
		}
	}
}
