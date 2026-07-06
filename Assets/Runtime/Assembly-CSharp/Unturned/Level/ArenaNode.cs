////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ArenaNode : Node
	{
		public static readonly float MIN_SIZE = 128;
		public static readonly float MAX_SIZE = 8192;

		internal float _normalizedRadius;

		/// <summary>
		/// This value is confusing because in the level editor it is the normalized radius, but in-game it is the radius.
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
					return CalculateRadiusFromNormalizedRadius(_normalizedRadius);
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

		public ArenaNode(Vector3 newPoint) : this(newPoint, 0f)
		{

		}

		public ArenaNode(Vector3 newPoint, float newRadius)
		{
			_point = newPoint;
			_normalizedRadius = newRadius;
			_type = ENodeType.ARENA;
		}
	}
}
