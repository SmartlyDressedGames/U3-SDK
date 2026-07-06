////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Utilities
{
	public struct AACylinderVolume : IShapeVolume
	{
		public Vector3 center;
		public float radius;
		public float height;

		public bool containsPoint(Vector3 point)
		{
			float halfHeight = height / 2;
			if (point.y > center.y - halfHeight && point.y < center.y + halfHeight)
			{
				float sqrRadius = radius * radius;
				return (new Vector2(point.x, point.z) - new Vector2(center.x, center.z)).sqrMagnitude < sqrRadius;
			}
			else
			{
				return false;
			}
		}

		public Bounds worldBounds
		{
			get
			{
				float circumference = radius * 2;
				return new Bounds(center, new Vector3(circumference, height, circumference));
			}
		}

		public float internalVolume => height * Mathf.PI * radius * radius;

		public float surfaceArea => Mathf.PI * radius * radius;

		public AACylinderVolume(Vector3 newCenter, float newRadius, float newHeight)
		{
			center = newCenter;
			radius = newRadius;
			height = newHeight;
		}
	}
}
