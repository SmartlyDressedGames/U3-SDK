////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class NightLight : MonoBehaviour
	{
		public Light target;

		private Material material;
		private Color emissionColor;
		private bool isListeningLoad;
		private bool isListeningTime;

		private void onLevelLoaded(int index)
		{
			if (isListeningLoad)
			{
				isListeningLoad = false;
				Level.onLevelLoaded -= onLevelLoaded;
			}

			if (!isListeningTime)
			{
				isListeningTime = true;
				LightingManager.onDayNightUpdated += onDayNightUpdated;
			}

			onDayNightUpdated(LightingManager.isDaytime);
		}

		private void onDayNightUpdated(bool isDaytime)
		{
			if (target != null)
			{
				target.gameObject.SetActive(!isDaytime);
			}

			if (material != null)
			{
				material.SetColor("_EmissionColor", isDaytime ? Color.black : emissionColor);
			}
		}

		private void Awake()
		{
			material = HighlighterTool.getMaterialInstance(transform);
			if (material != null)
			{
				emissionColor = material.GetColor("_EmissionColor");
				if (emissionColor.IsNearlyBlack())
				{
					emissionColor = new Color(1.5f, 1.5f, 1.5f);
				}
			}

			if (Level.isEditor)
			{
				onDayNightUpdated(false);

				return;
			}

			if (!isListeningLoad)
			{
				isListeningLoad = true;
				Level.onLevelLoaded += onLevelLoaded;
			}
		}

		private void OnDestroy()
		{
			if (material != null)
			{
				DestroyImmediate(material);
			}

			if (isListeningLoad)
			{
				isListeningLoad = false;
				Level.onLevelLoaded -= onLevelLoaded;
			}

			if (isListeningTime)
			{
				isListeningTime = false;
				LightingManager.onDayNightUpdated -= onDayNightUpdated;
			}
		}
	}
}
