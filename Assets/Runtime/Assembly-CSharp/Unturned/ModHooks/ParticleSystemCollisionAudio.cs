////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SDG.Unturned
{
	[AddComponentMenu("Unturned/Particle System Collision Audio")]
	public class ParticleSystemCollisionAudio : MonoBehaviour
	{
		public ParticleSystem particleSystemComponent;

		[FormerlySerializedAs("audioPrefab")]
		public OneShotAudioDefinition audioDef;

		/// <summary>
		/// If set, audio clip associated with physics material will take priority.
		/// </summary>
		public string materialPropertyName;

		/// <summary>
		/// Collision with speed lower than this value will not play a sound.
		/// </summary>
		public float speedThreshold = 0.01f;

		public float minSpeed = 0.2f;
		public float maxSpeed = 1.0f;
		public float minVolume = 0.5f;
		public float maxVolume = 1.0f;

#if !DEDICATED_SERVER && GAME
		private void OnParticleCollision(GameObject other)
		{
			if (particleSystemComponent == null || other == null || (audioDef == null && string.IsNullOrEmpty(materialPropertyName)))
				return;

			float speedThresholdSquared = speedThreshold * speedThreshold;

			int eventCount = particleSystemComponent.GetCollisionEvents(other, collisionEvents);
			for (int eventIndex = 0; eventIndex < eventCount; ++eventIndex)
			{
				ParticleCollisionEvent item = collisionEvents[eventIndex];

				float speedSquared = item.velocity.sqrMagnitude;
				if (speedSquared < speedThresholdSquared)
					continue;

				float speed = Mathf.Sqrt(speedSquared);
				float normalizedVolume = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
				float volume = Mathf.Lerp(minVolume, maxVolume, normalizedVolume);

				OneShotAudioDefinition audioDefToPlay;
				if (string.IsNullOrEmpty(materialPropertyName))
				{
					// Fallback audioDef is tested for null earlier.
					audioDefToPlay = audioDef;
				}
				else
				{
					string materialName;
					if (item.colliderComponent.transform.CompareTag("Ground"))
					{
						materialName = PhysicsTool.GetTerrainMaterialName(item.intersection);
					}
					else
					{
						materialName = PhysicsTool.GetColliderSharedPhysicsMaterialName(item.colliderComponent as Collider);
					}

					if (!string.IsNullOrEmpty(materialName))
					{
						audioDefToPlay = PhysicMaterialCustomData.GetAudioDef(materialName, materialPropertyName);
						if (audioDefToPlay == null)
						{
							audioDefToPlay = audioDef;
						}
					}
					else
					{
						audioDefToPlay = audioDef;
					}

					if (audioDefToPlay == null)
					{
						// Could be null because fallback audio def is not required.
						continue;
					}
				}

				AudioClip clip = audioDefToPlay.GetRandomClip();
				if (clip == null)
					continue;

				OneShotAudioParameters oneShotParams = new OneShotAudioParameters(item.intersection, clip);
				oneShotParams.volume = volume * audioDefToPlay.volumeMultiplier;
				oneShotParams.RandomizePitch(audioDefToPlay.minPitch, audioDefToPlay.maxPitch);
				oneShotParams.Play();
			}
		}

		/// <summary>
		/// Currently triggers are only used for water.
		/// </summary>
		private void OnParticleTrigger()
		{
			if (particleSystemComponent == null)
				return;

			float speedThresholdSquared = speedThreshold * speedThreshold;

			int eventCount = ParticlePhysicsExtensions.GetTriggerParticles(particleSystemComponent, ParticleSystemTriggerEventType.Enter, enterParticles);
			for (int eventIndex = 0; eventIndex < eventCount; ++eventIndex)
			{
				ParticleSystem.Particle item = enterParticles[eventIndex];

				float speedSquared = item.velocity.sqrMagnitude;
				if (speedSquared < speedThresholdSquared)
					continue;

				float speed = Mathf.Sqrt(speedSquared);
				float normalizedVolume = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
				float volume = Mathf.Lerp(minVolume, maxVolume, normalizedVolume);

				OneShotAudioDefinition audioDefToPlay = PhysicMaterialCustomData.GetAudioDef("Water", materialPropertyName);
				if (audioDefToPlay == null)
					continue;

				AudioClip clip = audioDefToPlay.GetRandomClip();
				if (clip == null)
					continue;

				OneShotAudioParameters oneShotParams = new OneShotAudioParameters(item.position, clip);
				oneShotParams.volume = volume * audioDefToPlay.volumeMultiplier;
				oneShotParams.RandomizePitch(audioDefToPlay.minPitch, audioDefToPlay.maxPitch);
				oneShotParams.Play();
			}
		}

		[System.NonSerialized]
		private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
		[System.NonSerialized]
		private List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>();
#endif // !DEDICATED_SERVER && GAME
	}
}
