////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class FlickeringLight : MonoBehaviour
	{
		public Light target;

		private Material material;

		private float blackoutTime;
		private float blackoutDelay;

		private void Update()
		{
			float intensity = Random.Range(0.9f, 1.4f);

			if (Time.time - blackoutTime < 0.15f)
			{
				intensity = 0.15f;
			}
			else if (Time.time - blackoutTime > blackoutDelay)
			{
				blackoutTime = Time.time;
				blackoutDelay = Random.Range(7.3f, 13.2f);
			}

			if (target != null)
			{
				target.intensity = intensity;
			}

			if (material != null)
			{
				material.SetColor("_EmissionColor", new Color(intensity, intensity, intensity));
			}
		}

		private void Awake()
		{
			material = HighlighterTool.getMaterialInstance(transform);

			blackoutTime = Time.time;
			blackoutDelay = Random.Range(0.0f, 13.2f);
		}

		private void OnDestroy()
		{
			if (material != null)
			{
				DestroyImmediate(material);
			}
		}
	}
}
