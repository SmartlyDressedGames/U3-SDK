////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public static class ControllerColliderHitEx
	{
		public static string ToDebugString(this ControllerColliderHit hit)
		{
			return $"Collider: {hit.collider} Rigidbody: {hit.rigidbody} Transform: {hit.transform.GetSceneHierarchyPath()} Point: {hit.point} Normal: {hit.normal} MoveDirection: {hit.moveDirection} MoveLength: {hit.moveLength}";
		}
	}
}
