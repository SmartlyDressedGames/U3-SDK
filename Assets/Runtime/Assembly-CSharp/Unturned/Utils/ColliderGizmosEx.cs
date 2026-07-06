////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class ColliderGizmosEx
	{
		public static void DrawCapsuleGizmo(this CapsuleCollider collider, Color color, float lifespan = 0.0f)
		{
			Vector3 point0;
			Vector3 point1;
			collider.GetCapsulePoints(out point0, out point1);
			RuntimeGizmos.Get().Capsule(point0, point1, collider.radius, color, lifespan);
		}

		public static void DrawSphereGizmo(this SphereCollider collider, Color color, float lifespan = 0.0f)
		{
			Vector3 center = collider.TransformSphereCenter();
			RuntimeGizmos.Get().Sphere(center, collider.radius, color, lifespan);
		}
	}
}
