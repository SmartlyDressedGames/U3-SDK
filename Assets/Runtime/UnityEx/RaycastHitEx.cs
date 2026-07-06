////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class RaycastHitEx
	{
		public static string ToDebugString(this RaycastHit hit)
		{
			return string.Format("Collider: {0} Rigidbody: {1} Transform: {2} Position: {3} Normal: {4}", hit.collider, hit.rigidbody, hit.transform.GetSceneHierarchyPath(), hit.point, hit.normal);
		}
	}
}
