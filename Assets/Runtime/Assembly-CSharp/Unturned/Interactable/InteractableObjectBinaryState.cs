////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// Experimented with disabling the Animation component when not playing animations,
// but the performance difference was negligible in Seattle and introduced other
// complications related to animations not having been updated once rendered.
// #define DISABLE_ANIMATION_AFTER_PLAY

using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableObjectBinaryState : InteractableObject
	{
		private bool _isUsed;
		public bool isUsed => _isUsed;

		private bool isInit;

		public bool isUsable => Time.realtimeSinceStartup - lastUsed > objectAsset.interactabilityDelay && (objectAsset.interactabilityPower == EObjectInteractabilityPower.NONE || isWired);

		public bool checkCanReset(float multiplier)
		{
			return isUsed && objectAsset.interactabilityReset > 1 && Time.realtimeSinceStartup - lastUsed > objectAsset.interactabilityReset * multiplier;
		}

		private float lastUsed = -9999;

		private Animation animationComponent;
#if DISABLE_ANIMATION_AFTER_PLAY
		private Coroutine animationCoroutine;

		private IEnumerator playAnimationAndDisable(string clipName, bool applyInstantly)
		{
			animationComponent.enabled = true;

			// Wait until next animation frame for enabled to take effect.
			yield return null;
			yield return null;

			animationComponent.Play(clipName);

			AnimationState state = animationComponent[clipName];

			if(applyInstantly)
			{
				// Wait until next animation frame to skip.
				yield return null;
				yield return null;

				state.normalizedTime = 1.0f;

				// Wait until next animation frame to disable.
				yield return null;
				yield return null;
			}
			else
			{
				yield return new WaitForSeconds(state.length + 0.5f);
			}
			
			animationComponent.enabled = false;
		}
#endif // DISABLE_ANIMATION_AFTER_PLAY

		private void initAnimationComponent()
		{
			string animComponentPath = _objectAsset.interactabilityChildPathOverride;
			if (string.IsNullOrEmpty(animComponentPath))
			{
				animComponentPath = "Root";
			}

			Transform animationComponentOwner = transform.Find(animComponentPath);
			if (animationComponentOwner != null)
			{
				animationComponent = animationComponentOwner.GetComponent<Animation>();
				animationComponent.playAutomatically = false;
				animationComponent.clip = null;
			}
		}

		private void updateAnimationComponent(bool applyInstantly)
		{
			if (animationComponent == null)
				return;

			string clipName = isUsed ? "Open" : "Close";
			if (animationComponent.GetClip(clipName) == null)
				return;

#if DISABLE_ANIMATION_AFTER_PLAY
			if(animationComponent.gameObject.activeInHierarchy == false)
			{
				// Unity logs an error if coroutine is run while gameObject is inactive (see first early return),
				// but worst case we will get fixed up when OnEnabled is run.
				return;
			}

			if(animationCoroutine != null)
			{
				// Cancel previous disableAnimationAfterDelay.
				StopCoroutine(animationCoroutine);
				animationCoroutine = null;
			}

			animationCoroutine = StartCoroutine(playAnimationAndDisable(clipName, applyInstantly));
#else
			animationComponent.Play(clipName);
			if (applyInstantly)
			{
				animationComponent[clipName].normalizedTime = 1.0f;
			}
#endif // #if DISABLE_ANIMATION_AFTER_PLAY
		}

		private AudioSource audioSourceComponent;

		private void initAudioSourceComponent()
		{
			audioSourceComponent = transform.GetComponent<AudioSource>();
		}

		private void updateAudioSourceComponent()
		{
			if (audioSourceComponent != null)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					audioSourceComponent.Play();
				}
			}
		}

		private IUnturnedNavmeshCutInterface cutComponent;

		private void InitNav()
		{
			if (objectAsset.interactabilityNav != EObjectInteractabilityNav.NONE)
			{
				cutComponent = UnturnedPathfinding.Get().CreateCutForIOBS(this);
			}
		}

		private void UpdateNav()
		{
			bool shouldBlockNavigation;
			switch (objectAsset.interactabilityNav)
			{
				default:
				case EObjectInteractabilityNav.NONE:
					return;

				case EObjectInteractabilityNav.ON:
					shouldBlockNavigation = isUsed;
					break;

				case EObjectInteractabilityNav.OFF:
					shouldBlockNavigation = !isUsed;
					break;
			}

			if (cutComponent != null)
			{
				cutComponent.IsActive = shouldBlockNavigation;
			}
			else if (owningLevelObject != null)
			{
				owningLevelObject.SetInteractableWantsNavActive(shouldBlockNavigation);
			}
		}

		private Material emissiveMaterialInstance;
		private GameObject toggleGameObject;

		private void initToggleGameObject()
		{
			Transform toggle = transform.FindChildRecursive("Toggle");
			LightLODTool.applyLightLOD(toggle);

			if (toggle != null)
			{
				toggleGameObject = toggle.gameObject;

				if (_objectAsset.iobsEmissiveMaterialMode == EInteractableObjectBinaryStateEmissiveMaterialMode.Auto)
				{
					emissiveMaterialInstance = HighlighterTool.getMaterialInstance(toggle.parent);
				}
			}
		}

		private void updateToggleGameObject()
		{
			if (toggleGameObject != null)
			{
				bool shouldBeActive = objectAsset.interactabilityPower == EObjectInteractabilityPower.STAY
					? isUsed && isWired : isUsed;

				toggleGameObject.SetActive(shouldBeActive);

				if (emissiveMaterialInstance != null)
				{
					emissiveMaterialInstance.SetColor("_EmissionColor", shouldBeActive ? new Color(2f, 2f, 2f) : Color.black);
				}
			}
		}

		public void updateToggle(bool newUsed)
		{
			lastUsed = Time.realtimeSinceStartup;
			_isUsed = newUsed;

			updateAnimationComponent(false);
			UpdateNav();
			updateAudioSourceComponent();
			updateToggleGameObject();

			onStateChanged?.Invoke(this);
		}

		// called when wired with a generator/not
		protected override void updateWired()
		{
			updateToggleGameObject();
		}

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			_isUsed = state[0] == 1;

			if (!isInit)
			{
				isInit = true;

				initAnimationComponent();
				InitNav();
				initAudioSourceComponent();
				initToggleGameObject();
			}

			updateAnimationComponent(true);
			UpdateNav();
			updateToggleGameObject();

			onStateInitialized?.Invoke(this);
		}

		public delegate void UsedChanged(InteractableObjectBinaryState sender);
		/// <summary>
		/// Invoked after state is first loaded, synced from server when entering relevancy, or reset.
		/// </summary>
		public event UsedChanged onStateInitialized;
		/// <summary>
		/// Invoked after interaction changes state.
		/// </summary>
		public event UsedChanged onStateChanged;

		/// <summary>
		/// Number of event hooks monitoring or controlling this.
		/// Used to allow client to control remote objects on server.
		/// </summary>
		public int modHookCounter;

		public void SetUsedFromClientOrServer(bool newUsed, InteractableObjectBinaryStateEventHook.EListenServerHostMode listenServerHostMode)
		{
			if (newUsed == isUsed)
				return;

			bool shouldRequestAsClient;
			if (Dedicator.IsDedicatedServer)
			{
				shouldRequestAsClient = false;
			}
			else if (Provider.isServer)
			{
				switch (listenServerHostMode)
				{
					default:
					case InteractableObjectBinaryStateEventHook.EListenServerHostMode.RequestAsClient:
						shouldRequestAsClient = true;
						break;

					case InteractableObjectBinaryStateEventHook.EListenServerHostMode.OverrideState:
						shouldRequestAsClient = false;
						break;
				}
			}
			else
			{
				shouldRequestAsClient = true;
			}

			if (shouldRequestAsClient)
			{
				// Request server to toggle, and apply any player conditions / rewards.
				ObjectManager.toggleObjectBinaryState(transform, newUsed);
			}
			else
			{
				// Authority change state directly.
				ObjectManager.forceObjectBinaryState(transform, newUsed);
			}
		}

		private float lastEffect;

		public override void use()
		{
			bool desiredState = !isUsed;

			EffectAsset effectAsset = objectAsset.FindInteractabilityEffectAsset();
			if (effectAsset != null)
			{
				if (Time.realtimeSinceStartup - lastEffect > 1.0f)
				{
					lastEffect = Time.realtimeSinceStartup;

					Transform effectTransform = transform.Find("Effect");
					if (effectTransform != null)
					{
						// We take Effect transform to mean they always want it to spawn regardless of On / Off.
						EffectManager.effect(effectAsset, effectTransform.position, effectTransform.forward);
					}
					else if (desiredState)
					{
						Transform onEffectTransform = transform.Find("Effect_On");
						if (onEffectTransform != null)
						{
							// Only spawn effect when state is On.
							EffectManager.effect(effectAsset, onEffectTransform.position, onEffectTransform.forward);
						}
					}
					else if (!desiredState)
					{
						// Effects were added to IOBS much later, so we do not fallback to spawning if there is no Effect transform.

						Transform offEffectTransform = transform.Find("Effect_Off");
						if (offEffectTransform != null)
						{
							// Only spawn effect when state is Off.
							EffectManager.effect(effectAsset, offEffectTransform.position, offEffectTransform.forward);
						}
					}
				}
			}

			ObjectManager.toggleObjectBinaryState(transform, desiredState);
		}

		public override bool checkInteractable()
		{
			return !objectAsset.interactabilityRemote;
		}

		public override bool checkUseable()
		{
			return (objectAsset.interactabilityPower == EObjectInteractabilityPower.NONE || isWired) && objectAsset.areInteractabilityConditionsMet(Player.LocalPlayer);
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			INPCCondition unmetCondition = objectAsset.interactabilityConditionsList.GetFirstUnmetCondition(Player.LocalPlayer);
			if (unmetCondition != null)
			{
				message = EPlayerMessage.CONDITION;
				text = unmetCondition.formatCondition(Player.LocalPlayer);
				color = Color.white;
				return true;
			}

			if (objectAsset.interactabilityPower != EObjectInteractabilityPower.NONE && !isWired)
			{
				message = EPlayerMessage.POWER;
			}
			else if (isUsed)
			{
				switch (objectAsset.interactabilityHint)
				{
					case EObjectInteractabilityHint.DOOR:
						message = EPlayerMessage.DOOR_CLOSE;
						break;
					case EObjectInteractabilityHint.SWITCH:
						message = EPlayerMessage.SPOT_OFF;
						break;
					case EObjectInteractabilityHint.FIRE:
						message = EPlayerMessage.FIRE_OFF;
						break;
					case EObjectInteractabilityHint.GENERATOR:
						message = EPlayerMessage.GENERATOR_OFF;
						break;
					case EObjectInteractabilityHint.USE:
						message = EPlayerMessage.USE;
						break;

					case EObjectInteractabilityHint.CUSTOM:
						message = EPlayerMessage.INTERACT;
						text = objectAsset.interactabilityText;
						color = Color.white;
						return true;

					default:
						message = EPlayerMessage.NONE;
						break;
				}
			}
			else
			{
				switch (objectAsset.interactabilityHint)
				{
					case EObjectInteractabilityHint.DOOR:
						message = EPlayerMessage.DOOR_OPEN;
						break;
					case EObjectInteractabilityHint.SWITCH:
						message = EPlayerMessage.SPOT_ON;
						break;
					case EObjectInteractabilityHint.FIRE:
						message = EPlayerMessage.FIRE_ON;
						break;
					case EObjectInteractabilityHint.GENERATOR:
						message = EPlayerMessage.GENERATOR_ON;
						break;
					case EObjectInteractabilityHint.USE:
						message = EPlayerMessage.USE;
						break;

					case EObjectInteractabilityHint.CUSTOM:
						message = EPlayerMessage.INTERACT;
						text = objectAsset.interactabilityText;
						color = Color.white;
						return true;

					default:
						message = EPlayerMessage.NONE;
						break;
				}
			}

			text = "";
			color = Color.white;
			return true;
		}

		private void OnEnable()
		{
			updateAnimationComponent(true);
		}

		private void OnDestroy()
		{
			if (emissiveMaterialInstance != null)
			{
				Destroy(emissiveMaterialInstance);
			}
		}

		[System.Obsolete("Matches behavior from before addition of EListenServerHostMode.")]
		public void setUsedFromClientOrServer(bool newUsed)
		{
			SetUsedFromClientOrServer(newUsed, InteractableObjectBinaryStateEventHook.EListenServerHostMode.RequestAsClient);
		}
	}
}
