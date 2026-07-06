////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class WaterHeightTransparentSort : MonoBehaviour
	{
		protected bool isUnderwater;
		protected Material material;

		internal void updateRenderQueue()
		{
			if (material == null)
				return;

			if (SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position))
			{
				if (LevelLighting.isSea)
				{
					material.renderQueue = 3100; // render after water
				}
				else
				{
					material.renderQueue = 2900; // render before water
				}
			}
			else
			{
				if (LevelLighting.isSea)
				{
					material.renderQueue = 2900; // render before water
				}
				else
				{
					material.renderQueue = 3100; // render after water
				}
			}
		}

		protected void handleIsSeaChanged(bool isSea)
		{
			updateRenderQueue();
		}

		protected void Start()
		{
			material = HighlighterTool.getMaterialInstance(transform);

			if (material != null)
			{
				LevelLighting.isSeaChanged += handleIsSeaChanged;

				updateRenderQueue();
			}
		}

		protected void OnDestroy()
		{
			if (material != null)
			{
				LevelLighting.isSeaChanged -= handleIsSeaChanged;

				DestroyImmediate(material);
				material = null;
			}
		}
	}
}
