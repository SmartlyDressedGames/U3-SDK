////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal static class PrefabUtil
	{
		public static void DestroyCollidersInChildren(GameObject gameObject, bool includeInactive)
		{
			gameObject.GetComponentsInChildren(includeInactive, workingColliders);

			foreach (Component component in workingColliders)
			{
				Object.Destroy(component);
			}

			workingColliders.Clear();
		}

		private static List<Collider> workingColliders = new List<Collider>();
	}
}
