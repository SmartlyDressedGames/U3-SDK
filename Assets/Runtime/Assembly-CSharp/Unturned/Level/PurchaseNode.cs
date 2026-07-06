////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PurchaseNode : Node
	{
		public static readonly float MIN_SIZE = 2;
		public static readonly float MAX_SIZE = 16;

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

		public ushort id;
		public uint cost;

		public PurchaseNode(Vector3 newPoint) : this(newPoint, 0, 0, 0)
		{

		}

		public PurchaseNode(Vector3 newPoint, float newRadius, ushort newID, uint newCost)
		{
			_point = newPoint;
			_normalizedRadius = newRadius;
			id = newID;
			cost = newCost;

			_type = ENodeType.PURCHASE;
		}
	}
}
