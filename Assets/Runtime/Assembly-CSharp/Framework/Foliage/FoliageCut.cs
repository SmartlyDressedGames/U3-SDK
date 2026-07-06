////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageCut
	{
		public bool ContainsPoint(Vector3 position)
		{
			float halfHeight = height * 0.5f;
			if (position.y > center.y - halfHeight && position.y < center.y + halfHeight)
			{
				float sqrRadius = radius * radius;
				return (new Vector2(position.x, position.z) - new Vector2(center.x, center.z)).sqrMagnitude < sqrRadius;
			}
			else
			{
				return false;
			}
		}

		public FoliageCut(Vector3 center, float radius, float height)
		{
			this.center = center;
			this.radius = radius;
			this.height = height;

			float circumference = radius * 2;
			Bounds worldBounds = new Bounds(center, new Vector3(circumference, height, circumference));
			foliageBounds = new FoliageBounds(worldBounds);
		}

		internal FoliageBounds foliageBounds;
		private Vector3 center;
		private float radius;
		private float height;
	}
}
