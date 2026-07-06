////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Layerer
	{
		public static void relayer(Transform target, int layer)
		{
			if (target == null)
			{
				return;
			}

			target.gameObject.layer = layer;
			for (int index = 0; index < target.childCount; index++)
			{
				relayer(target.GetChild(index), layer);
			}
		}

		public static void viewmodel(Transform target)
		{
			if (target.GetComponent<Renderer>() != null)
			{
				target.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				target.GetComponent<Renderer>().receiveShadows = false;

				target.tag = "Viewmodel";
				target.gameObject.layer = LayerMasks.VIEWMODEL;
			}
			else
			{
				LODGroup lodGroup = target.GetComponent<LODGroup>();
				if (lodGroup != null)
				{
					foreach (Renderer renderer in new LodGroupEnumerator(lodGroup))
					{
						renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
						renderer.receiveShadows = false;

						renderer.gameObject.tag = "Viewmodel";
						renderer.gameObject.layer = LayerMasks.VIEWMODEL;
					}
				}
			}
		}

		public static void enemy(Transform target)
		{
			if (target.GetComponent<Renderer>() != null)
			{
				target.tag = "Enemy";
				target.gameObject.layer = LayerMasks.ENEMY;
			}
			else
			{
				LODGroup lodGroup = target.GetComponent<LODGroup>();
				if (lodGroup != null)
				{
					foreach (Renderer renderer in new LodGroupEnumerator(lodGroup))
					{
						renderer.gameObject.tag = "Enemy";
						renderer.gameObject.layer = LayerMasks.ENEMY;
					}
				}
			}
		}
	}
}
