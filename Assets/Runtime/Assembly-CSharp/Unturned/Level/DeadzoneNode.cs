////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum EDeadzoneType
	{
		/// <summary>
		/// Original type on the Russia map which requires a mask with filters.
		/// </summary>
		DefaultRadiation,

		/// <summary>
		/// Requires a mask with filters and full body suit.
		/// </summary>
		FullSuitRadiation,
	}

	public interface IDeadzoneNode
	{
		EDeadzoneType DeadzoneType { get; }

		/// <summary>
		/// Damage dealt to players while inside the volume if they *don't* have clothing matching the deadzone type.
		/// Could help prevent players from running in and out to grab a few items without dieing.
		/// </summary>
		float UnprotectedDamagePerSecond { get; }

		/// <summary>
		/// Damage dealt to players while inside the volume if they *do* have clothing matching the deadzone type.
		/// For example, an area could be so dangerous that even with protection they take a constant 0.1 DPS.
		/// </summary>
		float ProtectedDamagePerSecond { get; }

		/// <summary>
		/// Virus damage to players while inside the volume if they *don't* have clothing matching the deadzone type.
		/// Defaults to 6.25 to preserve behavior from before adding this property.
		/// </summary>
		float UnprotectedRadiationPerSecond { get; }

		/// <summary>
		/// Rate of depletion from gasmask filter's quality/durability.
		/// Defaults to 0.4 to preserve behavior from before adding this property.
		/// </summary>
		float MaskFilterDamagePerSecond { get; }
	}

	public class DeadzoneNode : Node, IDeadzoneNode
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

		private EDeadzoneType _deadzoneType;
		public EDeadzoneType DeadzoneType
		{
			get => _deadzoneType;
			set => _deadzoneType = value;
		}

		/// <summary>
		/// Nelson 2024-06-10: Added this property after nodes were converted to volumes. i.e., only old levels from
		/// before this property were added still have nodes, so it's expected that they won't deal damage over time.
		/// </summary>
		public float UnprotectedDamagePerSecond
		{
			get => 0.0f;
		}

		/// <summary>
		/// Same description as <see cref="UnprotectedDamagePerSecond"/>.
		/// </summary>
		public float ProtectedDamagePerSecond
		{
			get => 0.0f;
		}

		/// <summary>
		/// Same description as <see cref="UnprotectedDamagePerSecond"/>.
		/// </summary>
		public float UnprotectedRadiationPerSecond
		{
			get => 6.25f;
		}

		/// <summary>
		/// Same description as <see cref="UnprotectedDamagePerSecond"/>.
		/// </summary>
		public float MaskFilterDamagePerSecond
		{
			get => 0.4f;
		}

		public DeadzoneNode(Vector3 newPoint) : this(newPoint, 0f, EDeadzoneType.DefaultRadiation)
		{ }

		public DeadzoneNode(Vector3 newPoint, float newRadius, EDeadzoneType newDeadzoneType)
		{
			_point = newPoint;
			_deadzoneType = newDeadzoneType;
			_normalizedRadius = newRadius;
			_type = ENodeType.DEADZONE;
		}
	}
}
