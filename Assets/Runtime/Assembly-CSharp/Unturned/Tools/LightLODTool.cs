////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LightLODTool
	{
		private static List<Light> lightsInChildren = new List<Light>();

		public static void applyLightLOD(Transform transform)
		{
			if (transform == null)
			{
				return;
			}

			lightsInChildren.Clear();
			transform.GetComponentsInChildren(true, lightsInChildren);

			for (int index = 0; index < lightsInChildren.Count; index++)
			{
				Light light = lightsInChildren[index];

				if (light.type == LightType.Area || light.type == LightType.Directional)
				{
					continue;
				}

				LightLOD lod = light.gameObject.AddComponent<LightLOD>();
				lod.targetLight = light;
			}
		}
	}
}