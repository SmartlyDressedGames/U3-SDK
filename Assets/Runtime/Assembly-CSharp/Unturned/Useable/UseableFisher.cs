////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
//#define INSTANT_FISHING // Useful when debugging catch challenge.
//#define DRAW_FISH_TARGET_POSITION
//#define LOG_FISHING_CATCH_CHALLENGE
#endif
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableFisher : Useable
	{
		enum EFishingState
		{
			/// <summary>
			/// Standing with the rod out, not using it.
			/// </summary>
			Idle,

			/// <summary>
			/// Strength gauge is active.
			/// </summary>
			PreparingToCast,

			/// <summary>
			/// Bobber is floating in the water.
			/// The line is out and can be reeled back in.
			/// </summary>
			LineDeployed,

			/// <summary>
			/// Only applicable for fishing rods which opt-in.
			/// Player is doing the challenge before the item is received.
			/// </summary>
			CatchChallenge,
		}

		private float startedCast;
		private float startedReel;
		private float castAnimationLength;
		private float reelAnimationLength;

		private bool isPlayingCastAnimation;
		private bool isPlayingReelAnimation;

		EFishingState fishingState = EFishingState.Idle;

		/// <summary>
		/// If true, bobber will spawn or destroy once animation trigger is reached.
		/// </summary>
		private bool isWaitingForAnimationTrigger;

		/// <summary>
		/// If false, bobber has started floating.
		/// </summary>
		private bool isWaitingForBobberToFindWater;

		/// <summary>
		/// Server decides which item will be caught next.
		/// Client is notified shortly before they can catch the item.
		/// Used in the challenge UI on the client.
		/// </summary>
		private CachingAssetRef nextRewardItem;

		/// <summary>
		/// Server sends a random seed for challenge.
		/// </summary>
		private int nextRewardSeed;

		private WaterVolume serverWaterVolume;
		private bool serverHasClientConfirmedCatch;

		private Transform bobberTransform;
		private Rigidbody bobberRigidbody;
		private Transform firstHook;
		private Transform thirdHook;
		private LineRenderer firstLine;
		private LineRenderer thirdLine;

		private Vector3 waterSurfacePosition;

		private uint strengthTime;
		private float strengthMultiplier;

		private int ticksUntilFishRelocates;
		private int fishTargetPosition;
		private int fishPosition;
		private int fishVelocity;
		/// <summary>
		/// Position of challenge player cursor.
		/// </summary>
		private int challengeInputPosition;
		/// <summary>
		/// Velocity of fishing challenge player cursor.
		/// </summary>
		private int challengeInputVelocity;
		private int challengeCaptureProgress;
		private int challengeCaptureProgressPerTick;
		private int challengeEscapeProgressPerTick;
		private bool challengeInputWantsToPullUp;

		/// <summary>
		/// Decreased until a notification is sent to the client they can catch a fish.
		/// </summary>
		private float serverTimeUntilFishAppears;

		private bool serverHasSentFishNotification;

		/// <summary>
		/// Increased after fish notification is sent/received.
		/// </summary>
		private float timeSinceFishNotification = 999f;

		/// <summary>
		/// Whether animation to indicate the fish can be caught has played yet.
		/// </summary>
		private bool hasPlayedTugAnimation;

		private ISleekBox castStrengthBox;
		private ISleekElement castStrengthArea;
		private ISleekImage castStrengthBar;

		private ISleekBox challengeBox;
		private ISleekImage challengeWater;
		private ISleekImage challengeCursor;
		private ISleekElement challengeProgressBarContainer;
		private ISleekImage challengeSuccessBar;
		private ISleekImage challengeFailureBar;
		private SleekItemIcon challengePrizeIcon;
		private FishingCatchableProperties catchableProperties;
#if DRAW_FISH_TARGET_POSITION
		private ISleekImage fishTargetPositionPreview;
#endif

#if !DEDICATED_SERVER
		private static AudioReference fishingLoopAudioRef = new AudioReference("core.masterbundle", "Sounds/Fishing/FishingChallengeLoop.wav");
		private static AudioReference fishingFailureAudioRef = new AudioReference("core.masterbundle", "Sounds/Fishing/FishingChallengeFailure.wav");
		private static AudioReference fishingSuccessAudioRef = new AudioReference("core.masterbundle", "Sounds/Fishing/FishingChallengeSuccess.asset");
		private OneShotAudioHandle fishingLoopAudioHandle;

		private void SetPlayingFishingLoop(bool playing)
		{
			if (playing)
			{
				if (!fishingLoopAudioHandle.IsValid)
				{
					OneShotAudioParameters playParams = new OneShotAudioParameters(transform, fishingLoopAudioRef);
					playParams.looping = true;
					fishingLoopAudioHandle = playParams.Play();
				}
			}
			else
			{
				fishingLoopAudioHandle.Stop();
			}
		}

		private void PlayFishingFailure()
		{
			OneShotAudioParameters playParams = new OneShotAudioParameters(transform.position, fishingFailureAudioRef);
			playParams.RandomizePitch(0.95f, 1.05f);
			playParams.RandomizeVolume(0.95f, 1.05f);
			playParams.Play();
		}

		private void PlayFishingSuccess()
		{
			OneShotAudioParameters playParams = new OneShotAudioParameters(waterSurfacePosition, fishingSuccessAudioRef);
			playParams.RandomizePitch(0.95f, 1.05f);
			playParams.RandomizeVolume(0.95f, 1.05f);
			playParams.Play();
		}
#endif

		/// <summary>
		/// If true, this item has closed PlayerLifeUI.
		/// </summary>
		private bool hasClosedMainHud;

		public override bool isUseableShowingMenu => castStrengthBox != null && castStrengthBox.IsVisible;

		private bool HasFinishedCastAnimation => Time.realtimeSinceStartup - startedCast > castAnimationLength;

		private bool HasFinishedReelAnimation => Time.realtimeSinceStartup - startedReel > reelAnimationLength;

		/// <summary>
		/// If true, enough time passed since starting Cast or Reel animation to apply its effects (e.g., spawning projectile).
		/// </summary>
		private bool HasReachedAnimationTrigger => isPlayingCastAnimation ? Time.realtimeSinceStartup - startedCast > castAnimationLength * 0.45f : Time.realtimeSinceStartup - startedReel > reelAnimationLength * 0.75f;

		private void PlayReelAnimation()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemFisherAsset) player.equipment.asset).reel);
			}

			player.animator.play("Reel", false);
		}

		private void UpdateCastStrengthGaugeVisible(bool visible)
		{
			castStrengthBox.IsVisible = visible;

			bool wantsMainHudClosed = castStrengthBox.IsVisible;
			if (hasClosedMainHud != visible)
			{
				hasClosedMainHud = visible;
				if (wantsMainHudClosed)
				{
					PlayerLifeUI.close();
				}
				else
				{
					PlayerLifeUI.open();
				}
			}
		}

		private static readonly ServerInstanceMethod<NetId> SendBobberInWaterConfirmation = ServerInstanceMethod<NetId>.Get(typeof(UseableFisher), nameof(ReceiveBobberInWaterConfirmation));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
		public void ReceiveBobberInWaterConfirmation(in ServerInvocationContext context, NetId waterVolumeNetId)
		{
			serverWaterVolume = NetIdRegistry.Get<WaterVolume>(waterVolumeNetId);
			if (serverWaterVolume == null)
			{
				// It's likely the legacy auto-added water.
				serverWaterVolume = WaterVolumeManager.seaLevelVolume;

				LogCatchChallenge($"Bobber in water, defaulting to default sea volume (unassigned net ID: {waterVolumeNetId})");
			}
			else
			{
				LogCatchChallenge($"Bobber in water, found water volume {serverWaterVolume} (net ID {waterVolumeNetId})");
			}

			ResetTimeUntilFishAppears();
		}

		private static readonly ServerInstanceMethod SendCatchConfirmation = ServerInstanceMethod.Get(typeof(UseableFisher), nameof(ReceiveCatchConfirmation));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
		public void ReceiveCatchConfirmation(in ServerInvocationContext context)
		{
			if (!serverHasSentFishNotification)
			{
				context.LogWarning("server had not sent a fish notification");
				return;
			}

			if (timeSinceFishNotification <= WARNING_DURATION + CATCH_WINDOW + SERVER_LENIENCY_WINDOW)
			{
				// At first glance this may seem pointless. The reason is for the server to use the
				// client's understanding of the catch timing window so that entering challenge is
				// consistent between client/server, plus a small leniency window.
				serverHasClientConfirmedCatch = true;

				LogCatchChallenge($"Client confirmed catch");
			}
			else
			{
				LogCatchChallenge($"Ignoring catch confirmation outside window");
			}
		}

		private static readonly ClientInstanceMethod<System.Guid, int> SendFishNotification = ClientInstanceMethod<System.Guid, int>.Get(typeof(UseableFisher), nameof(ReceiveFishNotification));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveFishNotification(System.Guid nextRewardGuid, int newSeed)
		{
			timeSinceFishNotification = 0.0f;
			hasPlayedTugAnimation = false;
			nextRewardItem = nextRewardGuid;
			nextRewardSeed = newSeed;
			LogCatchChallenge($"Fishing challenge seed: {nextRewardSeed} Next item: {nextRewardItem.Get()?.FriendlyName}");

			if (!Dedicator.IsDedicatedServer)
			{
				Quaternion splashRotation = Quaternion.Euler(-90, Random.Range(0f, 360f), 0);

				GameObject splashPrefab = Assets.coreMasterBundle.LoadAsset<GameObject>("Fishers/Splash.prefab");
				Transform splash = Instantiate(splashPrefab, waterSurfacePosition, splashRotation).transform;
				splash.name = "Splash";
				EffectManager.RegisterDebris(splash.gameObject);

				Destroy(splash.gameObject, 8);
			}
		}

		[System.Obsolete]
		public void askReel(CSteamID steamID)
		{
			ReceivePlayReel();
		}

		private static readonly ClientInstanceMethod SendPlayReel = ClientInstanceMethod.Get(typeof(UseableFisher), nameof(ReceivePlayReel));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askReel))]
		public void ReceivePlayReel()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				PlayReelAnimation();
			}
		}

		private void PlayCastAnimation()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				player.playSound(((ItemFisherAsset) player.equipment.asset).cast);
			}

			player.animator.play("Cast", false);
		}

		[System.Obsolete]
		public void askCast(CSteamID steamID)
		{
			ReceivePlayCast();
		}

		private static readonly ClientInstanceMethod SendPlayCast = ClientInstanceMethod.Get(typeof(UseableFisher), nameof(ReceivePlayCast));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askCast))]
		public void ReceivePlayCast()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				PlayCastAnimation();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (fishingState == EFishingState.Idle)
			{
				fishingState = EFishingState.PreparingToCast;

				strengthTime = 0;
				strengthMultiplier = 0.0f;

				if (channel.IsLocalPlayer)
				{
					UpdateCastStrengthGaugeVisible(true);
				}
			}
			else if (fishingState == EFishingState.LineDeployed)
			{
				ItemFisherAsset fisherAsset = GetEquippedAsset<ItemFisherAsset>();

				bool isWithinWindow = serverHasClientConfirmedCatch;
				if (channel.IsLocalPlayer)
				{
					if (timeSinceFishNotification >= WARNING_DURATION && timeSinceFishNotification <= WARNING_DURATION + CATCH_WINDOW)
					{
						SendCatchConfirmation.Invoke(GetNetId(), ENetReliability.Reliable);
						isWithinWindow = true;
					}
				}
				serverHasClientConfirmedCatch = false;

				LogCatchChallenge($"Input within catch window: {isWithinWindow}");

				if (fisherAsset != null && fisherAsset.EnableCatchChallenge && (Provider.modeConfigData?.Gameplay?.Enable_Fishing_Catch_Challenge ?? false))
				{
					if (isWithinWindow)
					{
						fishingState = EFishingState.CatchChallenge;
						LogCatchChallenge("Entering fishing challenge");

						player.animator.play("Catch_Loop", false);

						ItemAsset rewardAsset = nextRewardItem.Get<ItemAsset>();
						if (rewardAsset != null && rewardAsset.FishingCatchable != null)
						{
							catchableProperties = rewardAsset.FishingCatchable;
							LogCatchChallenge($"Using {rewardAsset.itemName} catchable properties: {catchableProperties}");
						}
						else
						{
							catchableProperties = FishingCatchableProperties.Default;
							LogCatchChallenge($"Using default catchable properties: {catchableProperties}");
						}

						ticksUntilFishRelocates = 0;
						Random.State stateToRestore = Random.state;
						Random.InitState(nextRewardSeed);
						fishTargetPosition = Random.Range(catchableProperties.minTargetPosition, catchableProperties.maxTargetPosition + 1);
						Random.state = stateToRestore;
						fishPosition = fishTargetPosition;
						fishVelocity = 0;
						challengeCaptureProgress = 0;

						LogCatchChallenge($"Initial fish position: {fishTargetPosition}");

						float normalizedSkillLevel = player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.FISHING);
						challengeCaptureProgressPerTick = Mathf.RoundToInt(FishingCatchableProperties.TIME_SCALE * (1f + normalizedSkillLevel * 0.2f) * fisherAsset.CatchChallengeCaptureSpeedMultiplier);
						challengeEscapeProgressPerTick = Mathf.RoundToInt(FishingCatchableProperties.TIME_SCALE * (1f - normalizedSkillLevel * 0.2f) * fisherAsset.CatchChallengeEscapeSpeedMultiplier);

						challengeInputPosition = Mathf.Clamp(fishTargetPosition - fisherAsset.CatchChallengeCursorSize / 2,
							0, FishingCatchableProperties.FIXED_POINT_SCALE - fisherAsset.CatchChallengeCursorSize);
						challengeInputVelocity = 0;
						challengeInputWantsToPullUp = true;

						if (channel.IsLocalPlayer)
						{
#if !DEDICATED_SERVER
							SetPlayingFishingLoop(true);
#endif

							challengePrizeIcon.Refresh(rewardAsset.id, 100, rewardAsset.getState(), rewardAsset);
							challengePrizeIcon.SizeOffset_X = rewardAsset.size_x * 50;
							challengePrizeIcon.SizeOffset_Y = rewardAsset.size_y * 50;
							challengePrizeIcon.PositionOffset_X = challengePrizeIcon.SizeOffset_X / -2;
							challengePrizeIcon.PositionOffset_Y = challengePrizeIcon.SizeOffset_Y / -2;

							challengeWater.SizeOffset_X = challengePrizeIcon.SizeOffset_X + 20;
							Color waterColor = LevelLighting.getSeaColor("_BaseColor");
							waterColor.a = 1;
							challengeWater.TintColor = waterColor;

							challengeProgressBarContainer.PositionOffset_X = challengeWater.PositionOffset_X + challengeWater.SizeOffset_X + 10;

							challengeBox.SizeOffset_X = challengeProgressBarContainer.PositionOffset_X + challengeProgressBarContainer.SizeOffset_X + 10;
							challengeBox.PositionOffset_X = challengeWater.SizeOffset_X / -2 - 10;
							challengeBox.IsVisible = true;
						}
					}
					else
					{
						ReelIn();
					}
				}
				else
				{
					if (Provider.isServer && isWithinWindow)
					{
						GrantRewards();
					}

					ReelIn();
				}
			}
			else if (fishingState == EFishingState.CatchChallenge)
			{
				challengeInputWantsToPullUp = true;
				LogCatchChallenge("Player resumed holding input");
			}

			return true;
		}

		public override void stopPrimary()
		{
			if (player.equipment.isBusy)
			{
				return;
			}

			if (fishingState == EFishingState.PreparingToCast)
			{
				fishingState = EFishingState.LineDeployed;

				if (channel.IsLocalPlayer)
				{
					UpdateCastStrengthGaugeVisible(false);
				}

				serverWaterVolume = null;

				player.equipment.isBusy = true;
				startedCast = Time.realtimeSinceStartup;
				isPlayingCastAnimation = true;

				if (channel.IsLocalPlayer)
				{
					isWaitingForAnimationTrigger = true;
				}

				PlayCastAnimation();

				if (Provider.isServer)
				{
					SendPlayCast.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());

					AlertTool.alert(transform.position, 8);
				}
			}
			else if (fishingState == EFishingState.CatchChallenge)
			{
				challengeInputWantsToPullUp = false;
				LogCatchChallenge("Player stopped holding input");
			}
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			castAnimationLength = player.animator.GetAnimationLength("Cast");
			reelAnimationLength = player.animator.GetAnimationLength("Reel");

			if (channel.IsLocalPlayer)
			{
				firstHook = player.equipment.firstModel.Find("Hook");
				thirdHook = player.equipment.thirdModel.Find("Hook");

				firstLine = (LineRenderer) player.equipment.firstModel.Find("Line").GetComponent<Renderer>();
				firstLine.tag = "Viewmodel";
				firstLine.gameObject.layer = LayerMasks.VIEWMODEL;
				firstLine.gameObject.SetActive(true);

				thirdLine = (LineRenderer) player.equipment.thirdModel.Find("Line").GetComponent<Renderer>();
				thirdLine.gameObject.SetActive(true);

				castStrengthBox = Glazier.Get().CreateBox();
				castStrengthBox.PositionOffset_X = -20;
				castStrengthBox.PositionOffset_Y = -110;
				castStrengthBox.PositionScale_X = 0.5f;
				castStrengthBox.PositionScale_Y = 0.5f;
				castStrengthBox.SizeOffset_X = 40;
				castStrengthBox.SizeOffset_Y = 220;
				PlayerUI.container.AddChild(castStrengthBox);
				castStrengthBox.IsVisible = false;

				castStrengthArea = Glazier.Get().CreateFrame();
				castStrengthArea.PositionOffset_X = 10;
				castStrengthArea.PositionOffset_Y = 10;
				castStrengthArea.SizeOffset_X = -20;
				castStrengthArea.SizeOffset_Y = -20;
				castStrengthArea.SizeScale_X = 1.0f;
				castStrengthArea.SizeScale_Y = 1.0f;
				castStrengthBox.AddChild(castStrengthArea);

				castStrengthBar = Glazier.Get().CreateImage();
				castStrengthBar.SizeScale_X = 1.0f;
				castStrengthBar.SizeScale_Y = 1.0f;
				castStrengthBar.Texture = GlazierResources.PixelTexture;
				castStrengthArea.AddChild(castStrengthBar);

				challengeBox = Glazier.Get().CreateBox();
				challengeBox.PositionOffset_Y = -160;
				challengeBox.PositionScale_X = 0.5f;
				challengeBox.PositionScale_Y = 0.5f;
				challengeBox.SizeOffset_X = 120;
				challengeBox.SizeOffset_Y = 320;
				PlayerLifeUI.container.AddChild(challengeBox);
				challengeBox.IsVisible = false;

				challengeWater = Glazier.Get().CreateImage();
				challengeWater.PositionOffset_X = 10;
				challengeWater.PositionOffset_Y = 10;
				challengeWater.SizeOffset_X = 80;
				challengeWater.SizeOffset_Y = -20;
				challengeWater.SizeScale_Y = 1;
				challengeWater.Texture = GlazierResources.PixelTexture;
				challengeBox.AddChild(challengeWater);

				challengeCursor = Glazier.Get().CreateImage();
				challengeCursor.TintColor = ESleekTint.FOREGROUND;
				challengeCursor.SizeScale_X = 1;
				challengeCursor.SizeScale_Y = GetEquippedAsset<ItemFisherAsset>().CatchChallengeCursorSize / (float) FishingCatchableProperties.FIXED_POINT_SCALE;
				challengeCursor.Texture = GlazierResources.PixelTexture;
				challengeWater.AddChild(challengeCursor);

				challengeProgressBarContainer = Glazier.Get().CreateFrame();
				challengeProgressBarContainer.PositionOffset_X = 100;
				challengeProgressBarContainer.PositionOffset_Y = 10;
				challengeProgressBarContainer.SizeOffset_X = 10;
				challengeProgressBarContainer.SizeOffset_Y = -20;
				challengeProgressBarContainer.SizeScale_Y = 1f;
				challengeBox.AddChild(challengeProgressBarContainer);

				challengeSuccessBar = Glazier.Get().CreateImage();
				challengeSuccessBar.SizeScale_X = 1f;
				challengeSuccessBar.Texture = GlazierResources.PixelTexture;
				challengeProgressBarContainer.AddChild(challengeSuccessBar);

				challengeFailureBar = Glazier.Get().CreateImage();
				challengeFailureBar.SizeScale_X = 1f;
				challengeFailureBar.Texture = GlazierResources.PixelTexture;
				challengeFailureBar.TintColor = ESleekTint.BAD;
				challengeProgressBarContainer.AddChild(challengeFailureBar);

				challengePrizeIcon = new SleekItemIcon();
				challengePrizeIcon.PositionScale_X = 0.5f;
				challengeWater.AddChild(challengePrizeIcon);

#if DRAW_FISH_TARGET_POSITION
				fishTargetPositionPreview = Glazier.Get().CreateImage();
				fishTargetPositionPreview.TintColor = new Color(1, 0, 0);
				fishTargetPositionPreview.SizeOffset_X = 4;
				fishTargetPositionPreview.SizeOffset_Y = 2;
				fishTargetPositionPreview.PositionOffset_X = fishTargetPositionPreview.SizeOffset_X / -2;
				fishTargetPositionPreview.PositionOffset_Y = fishTargetPositionPreview.SizeOffset_Y / -2;
				fishTargetPositionPreview.PositionScale_X = 0.5f;
				fishTargetPositionPreview.Texture = GlazierResources.PixelTexture;
				challengeWater.AddChild(fishTargetPositionPreview);
#endif // DRAW_FISH_TARGET_POSITION
			}
		}

		public override void dequip()
		{
			if (channel.IsLocalPlayer)
			{
				if (bobberTransform != null)
				{
					Destroy(bobberTransform.gameObject);
				}

				if (castStrengthBox != null)
				{
					PlayerUI.container.RemoveChild(castStrengthBox);
				}

				if (challengeBox != null)
				{
					PlayerLifeUI.container.RemoveChild(challengeBox);
				}

#if !DEDICATED_SERVER
				SetPlayingFishingLoop(false);
#endif

				if (hasClosedMainHud)
				{
					hasClosedMainHud = false;
					PlayerLifeUI.open();
				}
			}
		}

		public override void tock(uint clock)
		{
			if (fishingState == EFishingState.PreparingToCast)
			{
				strengthTime++;

				uint period = 100 + ((uint) player.skills.skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.FISHING].level * 20);
				strengthMultiplier = 1.0f - Mathf.Abs(Mathf.Sin((strengthTime + (period / 2)) % period / (float) period * Mathf.PI));
				strengthMultiplier *= strengthMultiplier;

				if (channel.IsLocalPlayer)
				{
					if (castStrengthBar != null)
					{
						castStrengthBar.PositionScale_Y = 1.0f - strengthMultiplier;
						castStrengthBar.SizeScale_Y = strengthMultiplier;

						castStrengthBar.TintColor = ItemTool.getQualityColor(strengthMultiplier);
					}
				}
			}
			else if (fishingState == EFishingState.CatchChallenge)
			{
				ItemFisherAsset equippedAsset = GetEquippedAsset<ItemFisherAsset>();

				if (ticksUntilFishRelocates > 0)
				{
					--ticksUntilFishRelocates;
				}
				else
				{
					Random.State stateToRestore = Random.state;
					Random.InitState(nextRewardSeed);
					nextRewardSeed = Random.Range(int.MinValue, int.MaxValue);
					LogCatchChallenge($"Next random seed: {nextRewardSeed}");

					ticksUntilFishRelocates = Random.Range(catchableProperties.minChangeTargetTicks, catchableProperties.maxChangeTargetTicks);
					int randomDelta = Random.Range(catchableProperties.minTargetDelta, catchableProperties.maxTargetDelta);
					if (fishTargetPosition + randomDelta > catchableProperties.maxTargetPosition)
					{
						fishTargetPosition = Mathf.Max(catchableProperties.minTargetPosition, fishTargetPosition - randomDelta);
					}
					else if (fishTargetPosition - randomDelta < catchableProperties.minTargetPosition)
					{
						fishTargetPosition = Mathf.Min(catchableProperties.maxTargetPosition, fishTargetPosition + randomDelta);
					}
					else
					{
						if (Random.value < 0.5f)
						{
							randomDelta = -randomDelta;
						}
						fishTargetPosition = fishTargetPosition + randomDelta;
					}
					LogCatchChallenge($"Fish target position changed: {fishTargetPosition}");

					Random.state = stateToRestore;
				}

				int acceleration = ((catchableProperties.springStiffness * (fishTargetPosition - fishPosition)) / FishingCatchableProperties.FIXED_POINT_SCALE) - ((catchableProperties.springDamping * fishVelocity) / FishingCatchableProperties.FIXED_POINT_SCALE);

				const int DELTA_TIME = 50;
				acceleration = Mathf.Clamp(acceleration, -catchableProperties.maxDownwardAcceleration, catchableProperties.maxUpwardAcceleration);
				fishVelocity += acceleration / DELTA_TIME;
				fishVelocity = Mathf.Clamp(fishVelocity, -catchableProperties.maxDownwardSpeed, catchableProperties.maxUpwardSpeed);

				fishPosition += fishVelocity / DELTA_TIME;
				if (fishPosition > FishingCatchableProperties.FIXED_POINT_SCALE)
				{
					fishPosition = FishingCatchableProperties.FIXED_POINT_SCALE - (fishPosition - FishingCatchableProperties.FIXED_POINT_SCALE);
					fishVelocity = -fishVelocity * catchableProperties.upperRestitution / FishingCatchableProperties.FIXED_POINT_SCALE;
				}
				else if (fishPosition < 0)
				{
					fishPosition = -fishPosition;
					fishVelocity = -fishVelocity * catchableProperties.lowerRestitution / FishingCatchableProperties.FIXED_POINT_SCALE;
				}

				if (challengeInputWantsToPullUp)
				{
					challengeInputVelocity += equippedAsset.CatchChallengeAcceleration / DELTA_TIME;
				}
				else
				{
					challengeInputVelocity -= equippedAsset.CatchChallengeGravity / DELTA_TIME;
				}
				challengeInputPosition += challengeInputVelocity / DELTA_TIME;
				if (challengeInputPosition + equippedAsset.CatchChallengeCursorSize > FishingCatchableProperties.FIXED_POINT_SCALE)
				{
					challengeInputPosition = FishingCatchableProperties.FIXED_POINT_SCALE - equippedAsset.CatchChallengeCursorSize - (challengeInputPosition + equippedAsset.CatchChallengeCursorSize - FishingCatchableProperties.FIXED_POINT_SCALE);
					challengeInputVelocity = -challengeInputVelocity * equippedAsset.CatchChallengeUpperRestitution / FishingCatchableProperties.FIXED_POINT_SCALE;
				}
				else if (challengeInputPosition < 0)
				{
					challengeInputPosition = 0;
					challengeInputVelocity = -challengeInputVelocity * equippedAsset.CatchChallengeLowerRestitution / FishingCatchableProperties.FIXED_POINT_SCALE;
				}

				bool isFishWithinCursor = fishPosition >= challengeInputPosition && fishPosition <= challengeInputPosition + equippedAsset.CatchChallengeCursorSize;
				if (isFishWithinCursor)
				{
					challengeCaptureProgress = Mathf.Min(Mathf.Max(0, challengeCaptureProgress + challengeCaptureProgressPerTick), catchableProperties.captureTicks);
				}
				else
				{
					challengeCaptureProgress = Mathf.Max(challengeCaptureProgress - challengeEscapeProgressPerTick, -catchableProperties.escapeTicks);
				}

				if (challengeCaptureProgress == catchableProperties.captureTicks)
				{
					LogCatchChallenge("Challenge success!");

					if (channel.IsLocalPlayer)
					{
						challengeBox.IsVisible = false;

#if !DEDICATED_SERVER
						SetPlayingFishingLoop(false);
						PlayFishingSuccess();

						ItemAsset rewardAsset = nextRewardItem.Get<ItemAsset>();
						if (rewardAsset != null)
						{
							rewardAsset.PlayInventoryAudio2D();
						}
#endif
					}

					if (Provider.isServer)
					{
						GrantRewards();
					}

					ReelIn();
				}
				else if (challengeCaptureProgress == -catchableProperties.escapeTicks)
				{
					LogCatchChallenge("Challenge failure!");
					player.animator.play("Catch_Failure", false);

					if (channel.IsLocalPlayer)
					{
						challengeBox.IsVisible = false;

#if !DEDICATED_SERVER
						SetPlayingFishingLoop(false);
						PlayFishingFailure();
#endif
					}

					fishingState = EFishingState.LineDeployed;
					if (Provider.isServer)
					{
						ResetTimeUntilFishAppears();
					}
				}

				if (channel.IsLocalPlayer)
				{
					if (challengePrizeIcon != null)
					{
						challengePrizeIcon.PositionScale_Y = 1.0f - (fishPosition / (float) FishingCatchableProperties.FIXED_POINT_SCALE);
					}

					if (challengeCursor != null)
					{
						challengeCursor.PositionScale_Y = 1.0f - ((challengeInputPosition + equippedAsset.CatchChallengeCursorSize) / (float) FishingCatchableProperties.FIXED_POINT_SCALE);
						challengeCursor.TintColor = isFishWithinCursor ? ESleekTint.FOREGROUND : ESleekTint.BAD;
					}

					if (challengeSuccessBar != null)
					{
						challengeSuccessBar.IsVisible = challengeCaptureProgress > 0;
						float normalizedProgress = challengeCaptureProgress / (float) catchableProperties.captureTicks;
						challengeSuccessBar.SizeScale_Y = normalizedProgress;
						challengeSuccessBar.PositionScale_Y = 1.0f - normalizedProgress;
						challengeSuccessBar.TintColor = ItemTool.getQualityColor(normalizedProgress);
					}

					if (challengeFailureBar != null)
					{
						challengeFailureBar.IsVisible = challengeCaptureProgress < 0;
						float normalizedProgress = -challengeCaptureProgress / (float) catchableProperties.escapeTicks;
						challengeFailureBar.SizeScale_Y = normalizedProgress;
					}

#if DRAW_FISH_TARGET_POSITION
					if (fishTargetPositionPreview != null)
					{
						fishTargetPositionPreview.PositionScale_Y = 1.0f - (fishTargetPosition / (float) FishingCatchableProperties.FIXED_POINT_SCALE);
					}
#endif // DRAW_FISH_TARGET_POSITION
				}
			}
		}

		public override void tick()
		{
			if (!player.equipment.IsEquipAnimationFinished)
			{
				return;
			}

			if (!channel.IsLocalPlayer)
			{
				return;
			}

			if (isWaitingForAnimationTrigger && HasReachedAnimationTrigger)
			{
				isWaitingForAnimationTrigger = false;

				if (isPlayingCastAnimation)
				{
					Vector3 origin = player.look.aim.position;
					Vector3 direction = player.look.aim.forward;
					RaycastHit hit;
					if (Physics.Raycast(new Ray(origin, direction), out hit, 1.5f, RayMasks.DAMAGE_SERVER))
					{
						// Wall is blocking aim. Ensure the bobber spawns at least 0.5m away from the wall.
						origin += direction * (hit.distance - 0.5f);
					}
					else
					{
						origin += direction;
					}

					GameObject bobPrefab = Assets.coreMasterBundle.LoadAsset<GameObject>("Fishers/Bob.prefab");
					bobberTransform = Instantiate(bobPrefab, origin, Quaternion.identity).transform;
					bobberTransform.name = "Bob";

					bobberRigidbody = bobberTransform.GetComponent<Rigidbody>();
					if (bobberRigidbody != null)
					{
						bobberRigidbody.AddForce(direction * Mathf.Lerp(500.0f, 1000.0f, strengthMultiplier));
						bobberRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
					}

					isWaitingForBobberToFindWater = true;
				}
				else if (isPlayingReelAnimation)
				{
					if (bobberTransform != null)
					{
						Destroy(bobberTransform.gameObject);
					}
				}
			}

			UpdateLineEndpoints();

			if (bobberTransform != null)
			{
				UpdateBobber();
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isPlayingCastAnimation && HasFinishedCastAnimation)
			{
				player.equipment.isBusy = false;
				isPlayingCastAnimation = false;
			}
			else if (isPlayingReelAnimation && HasFinishedReelAnimation)
			{
				player.equipment.isBusy = false;
				isPlayingReelAnimation = false;
			}

			timeSinceFishNotification += PlayerInput.RATE;

			if (Provider.isServer && fishingState == EFishingState.LineDeployed && serverWaterVolume != null)
			{
				serverTimeUntilFishAppears -= PlayerInput.RATE;
				if (serverTimeUntilFishAppears <= 0.0f)
				{
					if (!serverHasSentFishNotification)
					{
						serverHasSentFishNotification = true;
						timeSinceFishNotification = 0.0f;

						ItemFisherAsset fisherAsset = ((ItemFisherAsset) player.equipment.asset);
						ItemAsset rewardAsset;
						if (fisherAsset.FishingRewardMode == EFishingRewardMode.WaterVolumes && (Level.getAsset()?.SupportsFishingVolumes ?? false))
						{
							// serverWaterVolume may have been destroyed since ReceiveBobberInWaterConfirmation
							if (serverWaterVolume != null)
							{
								SpawnAsset spawnAsset = serverWaterVolume.GetFishSpawnTable();
								if (spawnAsset != null)
								{
									rewardAsset = SpawnTableTool.Resolve<ItemAsset>(spawnAsset, EAssetType.ITEM, serverWaterVolume.OnGetFishErrorContext);
								}
								else
								{
									spawnAsset = Level.getAsset()?.GetDefaultFishingSpawnTable();
									if (spawnAsset != null)
									{
										rewardAsset = SpawnTableTool.Resolve<ItemAsset>(spawnAsset, EAssetType.ITEM, Level.getAsset().OnGetFishErrorContext);
									}
									else
									{
										// Other fishing volumes may have spawn tables.
										rewardAsset = null;
									}
								}
							}
							else
							{
								rewardAsset = null;
							}
						}
						else
						{
							rewardAsset = SpawnTableTool.Resolve<ItemAsset>(fisherAsset.rewardID, EAssetType.ITEM, OnGetRewardErrorContext);
						}
						nextRewardItem = rewardAsset;

						System.Guid rewardGuid = rewardAsset?.GUID ?? System.Guid.Empty;
						nextRewardSeed = Random.Range(int.MinValue, int.MaxValue);
						LogCatchChallenge($"Server selected seed: {nextRewardSeed} Next item: {nextRewardItem.Get()?.FriendlyName}");
						SendFishNotification.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), rewardGuid, nextRewardSeed);
					}

					if (timeSinceFishNotification > 5.0f) // Client missed their opportunity. :(
					{
						ResetTimeUntilFishAppears();
					}
				}
			}
		}

		private void ResetTimeUntilFishAppears()
		{
			serverHasSentFishNotification = false;

			float minInterval = Provider.modeConfigData?.Gameplay?.Min_Fishing_Bite_Interval ?? 1.0f;
			float maxInterval = Provider.modeConfigData?.Gameplay?.Max_Fishing_Bite_Interval ?? 1.0f;
			float maxStrengthMultiplier = Provider.modeConfigData?.Gameplay?.Fishing_MaxStrength_Bite_Interval_Multiplier ?? 1.0f;

			serverTimeUntilFishAppears = Random.Range(minInterval, maxInterval);
			serverTimeUntilFishAppears *= Mathf.Lerp(1.0f, maxStrengthMultiplier, strengthMultiplier);
			serverTimeUntilFishAppears *= GetEquippedAsset<ItemFisherAsset>().FishBiteIntervalMultiplier;
			serverTimeUntilFishAppears *= LevelLighting.GetFishingBiteIntervalMultiplier(player.movement.WeatherMask);

#if INSTANT_FISHING
			serverTimeUntilFishAppears = 2;
#endif
		}

		private void UpdateLineEndpoints()
		{
			if (bobberTransform != null)
			{
				if (player.look.perspective == EPlayerPerspective.FIRST)
				{
					Vector3 screen = MainCamera.instance.WorldToViewportPoint(bobberTransform.position);
					Vector3 world = player.animator.viewmodelCamera.ViewportToWorldPoint(screen);

					firstLine.SetPosition(0, firstHook.position);
					firstLine.SetPosition(1, world);
				}
				else
				{
					thirdLine.SetPosition(0, thirdHook.position);
					thirdLine.SetPosition(1, bobberTransform.position);
				}
			}
			else
			{
				if (player.look.perspective == EPlayerPerspective.FIRST)
				{
					firstLine.SetPosition(0, Vector3.zero);
					firstLine.SetPosition(1, Vector3.zero);
				}
				else
				{
					thirdLine.SetPosition(0, Vector3.zero);
					thirdLine.SetPosition(1, Vector3.zero);
				}
			}
		}

		private void UpdateBobber()
		{
			if (isWaitingForBobberToFindWater)
			{
				WaterVolume overlappingVolume = WaterVolumeManager.Get().GetFishingVolume(bobberTransform.position);
				bool isUnderwater = overlappingVolume != null;
				float surfaceElevation = overlappingVolume != null ? WaterUtility.getWaterSurfaceElevation(overlappingVolume, bobberTransform.position) : -1024f;
				float minimumDepth = 4f;
				if (overlappingVolume != null && overlappingVolume.FishingMinimumDepthOverride > -0.5f)
				{
					minimumDepth = overlappingVolume.FishingMinimumDepthOverride;
				}
				if (isUnderwater && bobberTransform.position.y < surfaceElevation - minimumDepth)
				{
					// Disable CCD before enabling isKinematic otherwise Unity logs an error (private issue #1894)
					bobberRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
					bobberRigidbody.useGravity = false;
					bobberRigidbody.isKinematic = true;

					waterSurfacePosition = bobberTransform.position;
					waterSurfacePosition.y = surfaceElevation;

					isWaitingForBobberToFindWater = false;

					NetId waterNetId = overlappingVolume.GetNetIdFromInstanceId();
					SendBobberInWaterConfirmation.Invoke(GetNetId(), ENetReliability.Reliable, waterNetId);
				}
			}
			else
			{
				if (timeSinceFishNotification >= WARNING_DURATION && timeSinceFishNotification <= WARNING_DURATION + CATCH_WINDOW)
				{
					if (!hasPlayedTugAnimation)
					{
						hasPlayedTugAnimation = true;

						if (!isPlayingReelAnimation) // Player may have started reeling in too early.
						{
							player.playSound(((ItemFisherAsset) player.equipment.asset).tug);

							player.animator.play("Tug", false);
						}
					}

					bobberRigidbody.MovePosition(Vector3.Lerp(bobberTransform.position, waterSurfacePosition + (Vector3.down * 4f) + (Vector3.left * Random.Range(-4f, 4f)) + (Vector3.forward * Random.Range(-4f, 4f)), 4 * Time.deltaTime));
				}
				else
				{
					bobberRigidbody.MovePosition(Vector3.Lerp(bobberTransform.position, waterSurfacePosition + (Vector3.up * Mathf.Sin(Time.time) * 0.25f), 4 * Time.deltaTime));
				}
			}
		}

		private void ReelIn()
		{
			fishingState = EFishingState.Idle;

			player.equipment.isBusy = true;
			startedReel = Time.realtimeSinceStartup;
			isPlayingReelAnimation = true;

			if (channel.IsLocalPlayer)
			{
				isWaitingForAnimationTrigger = true;
			}

			PlayReelAnimation();

			if (Provider.isServer)
			{
				SendPlayReel.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());

				AlertTool.alert(transform.position, 8);
			}
		}

		private void GrantRewards()
		{
			ItemAsset rewardAsset = nextRewardItem.Get<ItemAsset>();
			if (rewardAsset != null)
			{
				player.inventory.forceAddItem(new Item(rewardAsset, EItemOrigin.NATURE), false);
			}

			player.sendStat(EPlayerStat.FOUND_FISHES);

			ItemFisherAsset fisherAsset = GetEquippedAsset<ItemFisherAsset>();

			int xp = Random.Range(fisherAsset.rewardExperienceMin, fisherAsset.rewardExperienceMax + 1);
			if (xp > 0)
			{
				player.skills.askPay((uint) xp);
			}

			fisherAsset.rewardsList.Grant(player);
		}

		private string OnGetRewardErrorContext()
		{
			return $"fishing {player.equipment.asset?.FriendlyName} reward";
		}

		[System.Diagnostics.Conditional("LOG_FISHING_CATCH_CHALLENGE")]
		private void LogCatchChallenge(object text)
		{
			CommandWindow.Log($"[Fishing Catch Challenge]: {text}");
		}

		private const float WARNING_DURATION = 1.0f; // How long bubbles are visible before fish takes the bait.
		private const float CATCH_WINDOW = 1.4f; // How long player has to press an input before the fish escapes.
		private const float SERVER_LENIENCY_WINDOW = 1.0f; // Extra time server allows for network delays.
	}
}
