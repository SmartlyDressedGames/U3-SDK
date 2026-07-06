////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_AUDIO_SOURCE_POOL
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Wraps audio source to prevent caller from meddling with it, and to allow the implementation
	/// to change in the future if necessary.
	/// </summary>
	public struct OneShotAudioHandle
	{
		internal OneShotAudioHandle(PooledAudioSource audioSource)
		{
			this.audioSource = audioSource;
			playId = audioSource.playId;
		}

		public bool IsValid => audioSource != null && !audioSource.isInPool && playId == audioSource.playId;

		public void Stop()
		{
			// Considered stopping coroutine as well, but that would be problematic with the shared
			// "waitForOneSecond" coroutine object.
			if (audioSource != null)
			{
				if (!audioSource.isInPool)
				{
					if (playId == audioSource.playId)
					{
#if LOG_AUDIO_SOURCE_POOL
						UnturnedLog.info($"Manually stopping audio play ID {playId} on source ID {audioSource.sourceId}");
#endif // LOG_AUDIO_SOURCE_POOL
						AudioSourcePool.Get().StopAndReleaseAudioSource(audioSource);
					}
					else
					{
#if LOG_AUDIO_SOURCE_POOL
						UnturnedLog.info($"Tried stopping audio play ID {playId} but source ID {audioSource.sourceId} has different play ID {audioSource.playId}");
#endif // LOG_AUDIO_SOURCE_POOL
					}
				}
				else
				{
#if LOG_AUDIO_SOURCE_POOL
					UnturnedLog.info($"Tried stopping audio play ID {playId} but source ID {audioSource.sourceId} is already back in the pool");
#endif // LOG_AUDIO_SOURCE_POOL
				}

				audioSource = null;
				playId = 0;
			}
		}

		private PooledAudioSource audioSource;
		private int playId;
	}

	public struct OneShotAudioParameters
	{
		public OneShotAudioParameters(Vector3 position, AudioClip clip, Transform parent)
		{
			this.position = position;
			this.clip = clip;
			this.parent = parent;
			volume = 1.0f;
			pitch = 1.0f;
			spatialBlend = 1.0f; // 3D
			rolloffMode = AudioRolloffMode.Linear;
			minDistance = 1.0f;
			maxDistance = 32.0f;
			looping = false;
			outputAudioMixerGroup = null;
		}

		public OneShotAudioParameters(Vector3 position, AudioClip clip) : this(position, clip, null)
		{ }

		public OneShotAudioParameters(Transform parent, AudioClip clip) : this(parent.position, clip, parent)
		{ }

		public OneShotAudioParameters(Transform parent, AudioReference audioRef) : this(parent.position, null, parent)
		{
			clip = audioRef.LoadAudioClip(out volume, out pitch);
		}

		public OneShotAudioParameters(Vector3 position, AudioReference audioRef) : this(position, null, null)
		{
			clip = audioRef.LoadAudioClip(out volume, out pitch);
		}

		/// <summary>
		/// 2D audio.
		/// </summary>
		public OneShotAudioParameters(AudioClip clip) : this(Vector3.zero, clip, null)
		{
			spatialBlend = 0.0f; // 2D
		}

		/// <summary>
		/// 2D audio.
		/// </summary>
		public OneShotAudioParameters(AudioReference audioRef) : this(Vector3.zero, null, null)
		{
			clip = audioRef.LoadAudioClip(out volume, out pitch);
			spatialBlend = 0.0f; // 2D
		}

		public void RandomizeVolume(float min, float max)
		{
			// *= to play nice with OneShotAudioDefinition default range
			volume *= Random.Range(min, max);
		}

		public void RandomizePitch(float min, float max)
		{
			// *= to play nice with OneShotAudioDefinition default range
			pitch *= Random.Range(min, max);
		}

		public void SetSpatialBlend2D()
		{
			spatialBlend = 0.0f;
		}

		public void SetSpatialBlend3D()
		{
			spatialBlend = 1.0f;
		}

		public void SetLinearRolloff(float min, float max)
		{
			rolloffMode = AudioRolloffMode.Linear;
			minDistance = min;
			maxDistance = max;
		}

		public OneShotAudioHandle Play()
		{
			return AudioSourcePool.Get().Play(ref this);
		}

		public Vector3 position;
		public AudioClip clip;

		/// <summary>
		/// Optional parent transform to attach the audio source to.
		/// </summary>
		public Transform parent;

		public float volume;
		public float pitch;
		/// <summary>
		/// 0 = 2D, 1 = 3D
		/// </summary>
		public float spatialBlend;
		public AudioRolloffMode rolloffMode;
		public float minDistance;
		public float maxDistance;
		/// <summary>
		/// If true, caller is responsible for returning audio source to the pool.
		/// Antithetical to the OneShot naming (originally referring to AudioSource.PlayOneShot),
		/// but this struct is moreso for requesting an audio source from the pool.
		/// </summary>
		public bool looping;
		public UnityEngine.Audio.AudioMixerGroup outputAudioMixerGroup;
	}

	/// <summary>
	/// Associates an ID with the instance of the sound being played. This ensures that if Stop() is called
	/// on an old handle it will not stop playing the audio if the component has already been recycled.
	/// </summary>
	internal class PooledAudioSource
	{
		public AudioSource component;
		public int sourceId;
		public int playId;

		/// <summary>
		/// True while inactive, false while playing.
		/// </summary>
		public bool isInPool;
	}

	internal class AudioSourcePool : MonoBehaviour
	{
		public static AudioSourcePool Get()
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("AudioSourcePool");
				instance = gameObject.AddComponent<AudioSourcePool>();
			}

			return instance;
		}

		internal OneShotAudioHandle Play(ref OneShotAudioParameters parameters)
		{
			if (parameters.clip == null || Dedicator.IsDedicatedServer || MainCamera.instance == null)
				return default;

			if (MathfEx.IsNearlyEqual(parameters.spatialBlend, 1.0f, tolerance: 0.001f)) // Is 3D?
			{
				Vector3 audioListenerPosition = MainCamera.instance.transform.position;
				if ((audioListenerPosition - parameters.position).sqrMagnitude > MathfEx.Square(parameters.maxDistance))
				{
					// Skip playing one-off sounds that will not be heard e.g. zombie roaring far away.
					return default;
				}
			}

			PooledAudioSource audioSource;
			int poolCount = availableComponents.Count;
			if (poolCount > 0)
			{
				int poolIndex = poolCount - 1;
				audioSource = availableComponents[poolIndex];
				availableComponents.RemoveAt(poolIndex);
				audioSource.component.enabled = true;

#if LOG_AUDIO_SOURCE_POOL
				UnturnedLog.info($"Claimed pooled audio source ID {audioSource.sourceId} (available: {availableComponents.Count})");
#endif // LOG_AUDIO_SOURCE_POOL
			}
			else
			{
				audioSource = new PooledAudioSource();
				audioSource.sourceId = nextSourceId;
				nextSourceId += 1;

				GameObject gameObject = new GameObject("PooledAudioSource");
				audioSource.component = gameObject.AddComponent<AudioSource>();
				audioSource.component.dopplerLevel = 0.0f;
				audioSource.component.playOnAwake = false;

#if LOG_AUDIO_SOURCE_POOL
				UnturnedLog.info($"Instantiated pooled audio source ID {audioSource.sourceId}");
#endif // LOG_AUDIO_SOURCE_POOL
			}

#if UNITY_EDITOR
			audioSource.component.name = $"[{audioSource.sourceId}] {parameters.clip.name}";
#endif

			Transform componentTransform = audioSource.component.transform;
			componentTransform.parent = parameters.parent;
			componentTransform.localScale = Vector3.one; // Scale gradually drifts when repeatedly attached/detached. 
			componentTransform.position = parameters.position;

			audioSource.component.outputAudioMixerGroup = parameters.outputAudioMixerGroup;
			if (audioSource.component.outputAudioMixerGroup == null)
			{
				audioSource.component.outputAudioMixerGroup = UnturnedAudioMixer.GetDefaultGroup();
			}

			audioSource.component.clip = parameters.clip;
			audioSource.component.volume = parameters.volume;
			audioSource.component.pitch = parameters.pitch;
			audioSource.component.spatialBlend = parameters.spatialBlend;
			audioSource.component.rolloffMode = parameters.rolloffMode;
			audioSource.component.minDistance = parameters.minDistance;
			audioSource.component.maxDistance = parameters.maxDistance;
			audioSource.component.loop = parameters.looping;
			audioSource.component.Play();

			audioSource.isInPool = false;
			audioSource.playId = nextPlayId;
			nextPlayId += 1;

#if LOG_AUDIO_SOURCE_POOL
			UnturnedLog.info($"Pooled audio source ID {audioSource.sourceId} play ID {audioSource.playId} clip {parameters.clip.name}");
#endif // LOG_AUDIO_SOURCE_POOL

			if (!parameters.looping)
			{
				StartCoroutine(PlayCoroutine(audioSource, audioSource.playId, (parameters.clip.length / parameters.pitch) + 0.1f));
			}

			return new OneShotAudioHandle(audioSource);
		}

		internal void StopAndReleaseAudioSource(PooledAudioSource audioSource)
		{
			Debug.Assert(!audioSource.isInPool);

			// Unfortunately component may have been destroyed if it was attached to an object that got destroyed,
			// e.g. player logged off shortly after reloading, but that is relatively unlikely.  
			if (audioSource.component != null)
			{
				audioSource.component.enabled = false;

				// 2023-02-27: weird 9.7ms spike during SetParent, so avoid calling SetParent if possible.
				if (audioSource.component.transform.parent != null)
				{
					// Restore parent to prevent audio source from being destroyed if it was attached.
					audioSource.component.transform.parent = null;
				}

				availableComponents.Add(audioSource);
				audioSource.isInPool = true;
				audioSource.playId = 0;

#if LOG_AUDIO_SOURCE_POOL
				UnturnedLog.info($"Returned audio source ID {audioSource.sourceId} to pool (available: {availableComponents.Count})");
#endif // LOG_AUDIO_SOURCE_POOL
			}
			else
			{
#if LOG_AUDIO_SOURCE_POOL
				UnturnedLog.info($"Tried returning audio source ID {audioSource.sourceId} to pool but component is null");
#endif // LOG_AUDIO_SOURCE_POOL
			}
		}

		/// <summary>
		/// Timer needs playId as well in case source has been recycled by the time duration expires.
		/// </summary>
		private IEnumerator PlayCoroutine(PooledAudioSource audioSource, int playId, float duration)
		{
			if (duration < 1.0f)
			{
				// Most of these one-off clips are rather short, so we can save some GC performance by
				// recycling the yield instruction.
				yield return waitForOneSecond;
			}
			else
			{
				yield return new WaitForSeconds(duration);
			}

#if LOG_AUDIO_SOURCE_POOL
			UnturnedLog.info($"Audio source ID {audioSource.sourceId} play ID {audioSource.playId} finished {duration}s wait");
#endif // LOG_AUDIO_SOURCE_POOL
			if (!audioSource.isInPool)
			{
				if (audioSource.playId == playId)
				{
#if LOG_AUDIO_SOURCE_POOL
					UnturnedLog.info($"Timer stopping audio play ID {playId} on source ID {audioSource.sourceId}");
#endif // LOG_AUDIO_SOURCE_POOL
					StopAndReleaseAudioSource(audioSource);
				}
				else
				{
					// Has already been recycled, probably after a manual call to stop.
#if LOG_AUDIO_SOURCE_POOL
					UnturnedLog.info($"Timer tried stopping audio play ID {playId} but source ID {audioSource.sourceId} has different play ID {audioSource.playId}");
#endif // LOG_AUDIO_SOURCE_POOL
				}
			}
			else
			{
#if LOG_AUDIO_SOURCE_POOL
				UnturnedLog.info($"Tried returning audio source ID {audioSource.sourceId} to pool after wait but it was already stopped");
#endif // LOG_AUDIO_SOURCE_POOL
			}
		}

		private void OnEnable()
		{
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;
		}

		private void OnDisable()
		{
			CommandLogMemoryUsage.OnExecuted -= OnLogMemoryUsage;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Audio source pool size: {availableComponents?.Count}");
		}

		private WaitForSeconds waitForOneSecond = new WaitForSeconds(1.0f);
		private static AudioSourcePool instance = null;
		private List<PooledAudioSource> availableComponents = new List<PooledAudioSource>();
		private int nextPlayId = 1;
		private int nextSourceId = 1;
	}
}
#else // DEDICATED_SERVER
namespace SDG.Unturned
{
	public struct OneShotAudioHandle
	{
		public void Stop()
		{
		}
	}
}
#endif // DEDICATED_SERVER
