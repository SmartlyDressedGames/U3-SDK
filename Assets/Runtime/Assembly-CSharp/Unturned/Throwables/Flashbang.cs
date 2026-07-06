////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace SDG.Unturned
{
	public class Flashbang : MonoBehaviour, IExplodableThrowable
	{
		public Color color = Color.white;
		public float fuseLength = 2.5f;
		public bool playAudioSource = true;

		public void Explode()
		{
			if (playAudioSource)
			{
				AudioSource audioComponent = GetComponent<AudioSource>();
				if (audioComponent != null)
				{
					audioComponent.Play();
				}
			}

			Light lightComponent = GetComponent<Light>();
			if (lightComponent != null && !lightComponent.enabled)
			{
				// Only consider inactive lights in case modders are using this component in an unexpected way.
				lightComponent.enabled = true;
				StartCoroutine(DisableLightNextFrame(lightComponent));
			}

			if (MainCamera.instance != null)
			{
				Vector3 offset = transform.position - MainCamera.instance.transform.position;

				if (offset.sqrMagnitude < 1024) // max range of 32 meters
				{
					float angle = Vector3.Dot(offset.normalized, MainCamera.instance.transform.forward);

					if (angle > -0.25f)
					{
						float distance = offset.magnitude;

						RaycastHit hit;
						if (distance < 0.5f || !Physics.Raycast(new Ray(MainCamera.instance.transform.position, offset / distance), out hit, distance - 0.5f, RayMasks.DAMAGE_SERVER, QueryTriggerInteraction.Ignore))
						{
							float angleFactor;
							if (angle > 0.5f)
							{
								angleFactor = 1.0f;
							}
							else
							{
								angleFactor = (angle + 0.25f) / 0.75f;
							}

							float distanceFactor;
							if (distance > 8.0f)
							{
								distanceFactor = 1.0f - ((distance - 8.0f) / 24.0f);
							}
							else
							{
								distanceFactor = 1.0f;
							}

							PlayerUI.stun(color, angleFactor * distanceFactor);
						}
					}
				}
			}

			AlertTool.alert(transform.position, 32.0f);

			Destroy(gameObject, 2.5f);
		}

		private void Start()
		{
			Invoke("Explode", fuseLength);
		}

		private IEnumerator DisableLightNextFrame(Light lightComponent)
		{
			yield return null;

			if (lightComponent != null)
			{
				lightComponent.enabled = false;
			}
		}
	}
}
