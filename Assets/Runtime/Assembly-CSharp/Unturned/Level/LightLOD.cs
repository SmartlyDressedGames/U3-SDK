////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class LightLOD : MonoBehaviour
	{
		public Light targetLight;

#if GAME
		private float intensityStart;

		private float transitionStart;
		private float transitionEnd;
		private float transitionMagnitude;

		private float sqrTransitionStart;
		private float sqrTransitionEnd;

		private void apply()
		{
			if (targetLight == null || targetLight.type == LightType.Area || targetLight.type == LightType.Directional)
			{
				return;
			}

			if (MainCamera.instance == null)
			{
				return;
			}

			Vector3 offset = transform.position - MainCamera.instance.transform.position;
			float sqrMagnitude = offset.sqrMagnitude;

			if (sqrMagnitude < sqrTransitionStart)
			{
				if (!targetLight.enabled)
				{
					targetLight.intensity = intensityStart;
					targetLight.enabled = true;
				}
			}
			else if (sqrMagnitude > sqrTransitionEnd)
			{
				if (targetLight.enabled)
				{
					targetLight.intensity = 0.0f;
					targetLight.enabled = false;
				}
			}
			else
			{
				float magnitude = offset.magnitude;
				float transition = (magnitude - transitionStart) / transitionMagnitude;

				targetLight.intensity = Mathf.Lerp(intensityStart, 0.0f, transition);

				if (!targetLight.enabled)
				{
					targetLight.enabled = true;
				}
			}
		}

		private void Update()
		{
			apply();
		}

		private void Start()
		{
			if (targetLight == null || targetLight.type == LightType.Area || targetLight.type == LightType.Directional)
			{
				enabled = false;
				return;
			}

			if (HelperClass.WantsLightLodsOff)
			{
				enabled = false;
				targetLight.enabled = true; // Ensure light is enabled because apply() would've otherwise enabled.
				return;
			}

			intensityStart = targetLight.intensity;

			if (targetLight.type == LightType.Point)
			{
				transitionStart = targetLight.range * 13.0f;
				transitionEnd = targetLight.range * 15.0f;
			}
			else if (targetLight.type == LightType.Spot)
			{
				transitionStart = Mathf.Max(64.0f, targetLight.range) * 1.75f;
				transitionEnd = Mathf.Max(64.0f, targetLight.range) * 2.0f;
			}

			transitionMagnitude = transitionEnd - transitionStart;

			sqrTransitionStart = transitionStart * transitionStart;
			sqrTransitionEnd = transitionEnd * transitionEnd;

			apply();
		}

		/// <summary>
		/// Prevents static member from being initialized during MonoBehaviour construction. (Unity warning)
		/// </summary>
		private static class HelperClass
		{
			public static bool WantsLightLodsOff => disableLightLods || GraphicsSettings.WantsCinematicMode;
			public static CommandLineFlag disableLightLods = new CommandLineFlag(false, "-DisableLightLODs");
		}
#endif // GAME
	}
}
