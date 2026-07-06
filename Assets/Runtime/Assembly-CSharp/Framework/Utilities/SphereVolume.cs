////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Utilities
{
	public struct SphereVolume : IShapeVolume
	{
		public Vector3 center;
		public float radius;

		public bool containsPoint(Vector3 point)
		{
			if (Mathf.Abs(point.x - center.x) >= radius)
			{
				return false;
			}

			if (Mathf.Abs(point.y - center.y) >= radius)
			{
				return false;
			}

			if (Mathf.Abs(point.z - center.z) >= radius)
			{
				return false;
			}

			float sqrRadius = radius * radius;
			return (point - center).sqrMagnitude < sqrRadius;
		}

		public Bounds worldBounds
		{
			get
			{
				float circumference = radius * 2;
				return new Bounds(center, new Vector3(circumference, circumference, circumference));
			}
		}

		public float internalVolume => 4f / 3f * Mathf.PI * radius * radius * radius;

		public float surfaceArea => 4 * Mathf.PI * radius * radius;

		public SphereVolume(Vector3 newCenter, float newRadius)
		{
			center = newCenter;
			radius = newRadius;
		}
	}
}
