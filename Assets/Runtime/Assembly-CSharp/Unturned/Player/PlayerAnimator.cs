////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void GestureUpdated(EPlayerGesture gesture);

	public class PlayerAnimator : PlayerCaller
	{
		public static readonly byte SAVEDATA_VERSION = 2;

		private static readonly float BOB_SPRINT = 0.075f;
		private static readonly float BOB_STAND = 0.05f;
		private static readonly float BOB_CROUCH = 0.025f;
		private static readonly float BOB_PRONE = 0.0125f;
		private static readonly float BOB_SWIM = 0.025f;

		private static readonly float TILT_SPRINT = 5f;
		private static readonly float TILT_STAND = 3f;
		private static readonly float TILT_CROUCH = 2f;
		private static readonly float TILT_PRONE = 1f;
		private static readonly float TILT_SWIM = 10f;

		private static readonly float SPEED_SPRINT = 10f;
		private static readonly float SPEED_STAND = 8f;
		private static readonly float SPEED_CROUCH = 6f;
		private static readonly float SPEED_PRONE = 4f;
		private static readonly float SPEED_SWIM = 6f;

		public GestureUpdated onGestureUpdated;

		/// <summary>
		/// Invoked after tellGesture is called with the new gesture.
		/// </summary>
		public static event System.Action<PlayerAnimator, EPlayerGesture> OnGestureChanged_Global;

		/// <summary>
		/// Empty transform created at the world origin.
		/// The first-person Viewmodel transform is re-parented to this.
		/// </summary>
		public Transform viewmodelParentTransform;

		private CharacterAnimator firstAnimator;
		private CharacterAnimator thirdAnimator;
		private HumanAnimator characterAnimator;

		private SkinnedMeshRenderer firstRenderer_0;

		private SkinnedMeshRenderer thirdRenderer_0;
		private SkinnedMeshRenderer thirdRenderer_1;

		private Transform _firstSkeleton;
		public Transform firstSkeleton => _firstSkeleton;

		private Transform _thirdSkeleton;
		public Transform thirdSkeleton => _thirdSkeleton;

		/// <summary>
		/// Child of the first-person skull transform.
		/// </summary>
		public Transform viewmodelCameraTransform;

		/// <summary>
		/// Camera near world origin masking the first-person arms and weapon.
		/// </summary>
		public Camera viewmodelCamera;

		/// <summary>
		/// Used by gun to hide viewmodel arms while aiming 2D scope, and by chainsaw to shake the viewmodel.
		/// </summary>
		public Vector3 viewmodelCameraLocalPositionOffset;
		/// <summary>
		/// Used to hide viewmodel arms while using a vehicle turret gun.
		/// </summary>
		public Vector3 drivingViewmodelCameraLocalPositionOffset;
		/// <summary>
		/// Offsets main camera and aim rotation while aiming with a scoped gun.
		/// </summary>
		public Vector3 scopeSway;

		/// <summary>
		/// Animated toward viewmodelSwayMultiplier.
		/// </summary>
		private float blendedViewmodelSwayMultiplier;
		/// <summary>
		/// Small number (0.1) while aiming, 1 while not aiming.
		/// Reduces viewmodel animation while aiming to make 3D sights more usable.
		/// </summary>
		public float viewmodelSwayMultiplier;

		/// <summary>
		/// Animated toward viewmodelOffsetPreferenceMultiplier.
		/// </summary>
		private float blendedViewmodelOffsetPreferenceMultiplier;
		/// <summary>
		/// 0 while aiming, 1 while not aiming.
		/// Players can customize the 3D position of the viewmodel on screen, but this needs
		/// to be blended out while aiming down sights otherwise it would not line up with
		/// the center of the screen.
		/// </summary>
		public float viewmodelOffsetPreferenceMultiplier;

		/// <summary>
		/// If true, use the scope aim fov instead of non-scope fov.
		/// Useful for players with high (e.g. 160) fov to be able to use scopes.
		/// </summary>
		public bool viewmodelOffsetPreferenceUseScope;

		/// <summary>
		/// Animated toward viewmodelCameraLocalPositionOffset, recoil, and bayonet offsets.
		/// </summary>
		private Vector3 blendedViewmodelCameraLocalPositionOffset;
		/// <summary>
		/// Abruptly offset when gun is fired, then animated back toward zero.
		/// </summary>
		public Rk4Spring3 recoilViewmodelCameraOffset;
		/// <summary>
		/// Abruptly offset when gun is fired, then animated back toward zero.
		/// x = pitch, y = yaw, z = roll
		/// </summary>
		public Rk4Spring3 recoilViewmodelCameraRotation;
		public Vector3 recoilViewmodelCameraMask = Vector3.one;
		/// <summary>
		/// Abruptly offset when bayonet is used, then animated back toward zero.
		/// </summary>
		private Vector3 bayonetViewmodelCameraOffset;
		/// <summary>
		/// Animated while player is moving.
		/// </summary>
		public Rk4Spring2 viewmodelMovementOffset;
		/// <summary>
		/// Blended from multiple viewmodel parameters and then applied to viewmodelCameraTransform.
		/// </summary>
		private Vector3 viewmodelCameraLocalPosition;

		public Rk4SpringQ viewmodelTargetExplosionLocalRotation;
		/// <summary>
		/// Smoothing adds some initial blend-in which felt nicer for explosion rumble.
		/// </summary>
		private Quaternion viewmodelSmoothedExplosionLocalRotation = Quaternion.identity;
		public float viewmodelExplosionSmoothingSpeed;

		/// <summary>
		/// Meshes are disabled until clothing is received.
		/// </summary>
		private bool isHiddenWaitingForClothing;

		/// <summary>
		/// Target viewmodelCameraLocalPosition except while driving.
		/// </summary>
		private Vector3 desiredViewmodelCameraLocalPosition;

		/// <summary>
		/// Animated while playing is moving.
		/// x = pitch, y = roll
		/// </summary>
		public Rk4Spring2 viewmodelCameraMovementLocalRotation;

		/// <summary>
		/// Offset when player lands.
		/// </summary>
		public Rk4Spring landedSpring;
		public float landedSpringRecoverySpeed;

		private Vector3 viewmodelCameraLocalRotation;

		/// <summary>
		/// Used to measure change in pitch between frames.
		/// </summary>
		private float lastFramePitchInput;
		/// <summary>
		/// Used to measure change in yaw between frames.
		/// </summary>
		private float lastFrameYawInput;
		/// <summary>
		/// Animated according to change in pitch/yaw input between frames so that gun rolls slightly while turning.
		/// </summary>
		public Rk4Spring3 rotationInputViewmodelRoll;

		private bool lastFrameHadItemPosition;
		private Vector3 lastFrameItemPosition;
		/// <summary>
		/// Animated according to change in item position between frames so that animations have more inertia.
		/// </summary>
		public Rk4Spring3 viewmodelItemInertiaRotation;
		/// <summary>
		/// Degrees per meter of item distance travelled.
		/// Pitch is driven by vertical displacement, yaw and roll are driven by horizontal.
		/// x = pitch, y = yaw, z = roll
		/// </summary>
		public Vector3 viewmodelItemInertiaMask;

		private bool inputWantsToLeanLeft;
		public bool leanLeft => inputWantsToLeanLeft;

		private bool inputWantsToLeanRight;
		public bool leanRight => inputWantsToLeanRight;

		internal bool leanObstructed;

		/// <summary>
		/// In third-person this delays leaning in case player only wanted
		/// to switch camera side without leaning.
		/// </summary>
		private float lastCameraSideInputRealtime;

		private int lastLean;
		private int _lean;
		public int lean => _lean;

		private float _shoulder;
		public float shoulder => _shoulder;

		private float _shoulder2;
		public float shoulder2 => _shoulder2;

		private bool inputWantsThirdPersonCameraOnLeftSide;
		public bool side => inputWantsThirdPersonCameraOnLeftSide;

		private EPlayerGesture _gesture;
		public EPlayerGesture gesture => _gesture;

		public CSteamID captorID;
		public ushort captorItem;
		public ushort captorStrength;

		public void AddEquippedItemAnimation(AnimationClip clip, Transform firstPersonModel, Transform thirdPersonModel, Transform characterModel)
		{
			if (clip == null)
				return;

			if (firstAnimator != null)
			{
				firstAnimator.AddEquippedItemAnimation(clip, firstPersonModel);
			}

			if (thirdAnimator != null)
			{
				thirdAnimator.AddEquippedItemAnimation(clip, thirdPersonModel);
			}

			if (characterAnimator != null)
			{
				characterAnimator.AddEquippedItemAnimation(clip, characterModel);
			}
		}

		public void removeAnimation(AnimationClip clip)
		{
			if (clip == null)
				return;

			if (firstAnimator != null)
			{
				firstAnimator.removeAnimation(clip);
			}

			if (thirdAnimator != null)
			{
				thirdAnimator.removeAnimation(clip);
			}

			if (characterAnimator != null)
			{
				characterAnimator.removeAnimation(clip);
			}
		}

		public void setAnimationSpeed(string name, float speed)
		{
			if (firstAnimator != null)
			{
				firstAnimator.setAnimationSpeed(name, speed);
			}

			if (thirdAnimator != null)
			{
				thirdAnimator.setAnimationSpeed(name, speed);
			}

			if (characterAnimator != null)
			{
				characterAnimator.setAnimationSpeed(name, speed);
			}
		}

		public float getAnimationLength(string name)
		{
			return GetAnimationLength(name, scaled: true);
		}

		/// <param name="scaled">If true, include current animation speed modifier.</param>
		public float GetAnimationLength(string name, bool scaled = true)
		{
			if (firstAnimator != null)
			{
				return firstAnimator.GetAnimationLength(name, scaled);
			}

			if (thirdAnimator != null)
			{
				return thirdAnimator.GetAnimationLength(name, scaled);
			}

			return 0f;
		}

		public bool checkExists(string name)
		{
			if (firstAnimator != null)
			{
				return firstAnimator.checkExists(name);
			}

			if (thirdAnimator != null)
			{
				return thirdAnimator.checkExists(name);
			}

			if (characterAnimator != null)
			{
				return characterAnimator.checkExists(name);
			}

			return false;
		}

		public void play(string name, bool smooth)
		{
			bool playingAnimation = false;

			if (firstAnimator != null)
			{
				playingAnimation |= firstAnimator.play(name, smooth);
			}

			if (thirdAnimator != null)
			{
				playingAnimation |= thirdAnimator.play(name, smooth);
			}

			if (characterAnimator != null)
			{
				playingAnimation |= characterAnimator.play(name, smooth);
			}

#if !DEDICATED_SERVER
			if (playingAnimation)
			{
				// Stop inspect audio if already playing.
				// Nelson 2025-03-24: this is such a hack! Moved over from PlayerEquipment.inspect so that inspect
				// audio is canceled when another animation interrupts. If another case like this comes up we should
				// add a proper animation interrupt handler.
				player.equipment.inspectAudioHandle.Stop();
			}
#endif // !DEDICATED_SERVER

			// If the clip exists it will have overridden our gesture state,
			// but if it does not exist our gesture animation continues.
			if (playingAnimation && gesture != EPlayerGesture.NONE)
			{
				_gesture = EPlayerGesture.NONE;
			}
		}

		public void stop(string name)
		{
			if (firstAnimator != null)
			{
				firstAnimator.stop(name);
			}

			if (thirdAnimator != null)
			{
				thirdAnimator.stop(name);
			}

			if (characterAnimator != null)
			{
				characterAnimator.stop(name);
			}
		}

		public void mixAnimation(string name)
		{
			if (firstAnimator != null)
			{
				firstAnimator.mixAnimation(name);
			}

			if (thirdAnimator != null)
			{
				thirdAnimator.mixAnimation(name);
			}

			if (characterAnimator != null)
			{
				characterAnimator.mixAnimation(name);
			}
		}

		public void mixAnimation(string name, bool mixLeftShoulder, bool mixRightShoulder)
		{
			mixAnimation(name, mixLeftShoulder, mixRightShoulder, false);
		}

		public void mixAnimation(string name, bool mixLeftShoulder, bool mixRightShoulder, bool mixSkull)
		{
			if (firstAnimator != null)
			{
				firstAnimator.mixAnimation(name, mixLeftShoulder, mixRightShoulder, mixSkull);
			}

			if (thirdAnimator != null)
			{
				thirdAnimator.mixAnimation(name, mixLeftShoulder, mixRightShoulder, mixSkull);
			}

			if (characterAnimator != null)
			{
				characterAnimator.mixAnimation(name, mixLeftShoulder, mixRightShoulder, mixSkull);
			}
		}

		public void AddRecoilViewmodelCameraOffset(float shake_x, float shake_y, float shake_z)
		{
			recoilViewmodelCameraOffset.currentPosition.x += shake_x;
			recoilViewmodelCameraOffset.currentPosition.y += shake_y;
			recoilViewmodelCameraOffset.currentPosition.z += shake_z;
		}

		public void AddRecoilViewmodelCameraRotation(float cameraYaw, float cameraPitch)
		{
			recoilViewmodelCameraRotation.currentPosition.x += cameraPitch * recoilViewmodelCameraMask.x;
			recoilViewmodelCameraRotation.currentPosition.y += cameraYaw * recoilViewmodelCameraMask.y;
			recoilViewmodelCameraRotation.currentPosition.z += cameraYaw * recoilViewmodelCameraMask.z;
		}

		public void AddBayonetViewmodelCameraOffset(float fling_x, float fling_y, float fling_z)
		{
			bayonetViewmodelCameraOffset.x += fling_x;
			bayonetViewmodelCameraOffset.y += fling_y;
			bayonetViewmodelCameraOffset.z += fling_z;
		}

		/// <summary>
		/// At this point camera is already being shook, we just add some of the same shake to viewmodel for secondary motion.
		/// </summary>
		internal void FlinchFromExplosion(Vector3 worldRotationAxis, float adjustedMagnitudeDegrees)
		{
			Vector3 localRotationAxis = viewmodelCameraTransform.InverseTransformDirection(worldRotationAxis);

			// Does not shake as much as camera.
			adjustedMagnitudeDegrees *= 0.25f;

			// Rotation is in local space because viewmodel camera rotation is applied in local space.
			viewmodelTargetExplosionLocalRotation.currentRotation *= Quaternion.AngleAxis(adjustedMagnitudeDegrees, localRotationAxis);
		}

		public float bob
		{
			get
			{
				if (Player.LocalPlayer.stance.stance == EPlayerStance.SPRINT)
				{
					return BOB_SPRINT * blendedViewmodelSwayMultiplier;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.STAND)
				{
					return BOB_STAND * blendedViewmodelSwayMultiplier;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.CROUCH)
				{
					return BOB_CROUCH * blendedViewmodelSwayMultiplier;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.PRONE)
				{
					return BOB_PRONE * blendedViewmodelSwayMultiplier;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.SWIM)
				{
					return BOB_SWIM * blendedViewmodelSwayMultiplier;
				}
				else
				{
					return 0;
				}
			}
		}

		public float tilt
		{
			get
			{
				if (Player.LocalPlayer.stance.stance == EPlayerStance.SPRINT)
				{
					return TILT_SPRINT * (1 - (blendedViewmodelSwayMultiplier / 2));
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.STAND)
				{
					return TILT_STAND * (1 - (blendedViewmodelSwayMultiplier / 2));
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.CROUCH)
				{
					return TILT_CROUCH * (1 - (blendedViewmodelSwayMultiplier / 2));
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.PRONE)
				{
					return TILT_PRONE * (1 - (blendedViewmodelSwayMultiplier / 2));
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.SWIM)
				{
					return TILT_SWIM * (1 - (blendedViewmodelSwayMultiplier / 2));
				}
				else
				{
					return 0;
				}
			}
		}

		public float roll
		{
			get
			{
				if (Player.LocalPlayer.stance.stance == EPlayerStance.SPRINT)
				{
					return Mathf.Sin(TILT_SPRINT * Time.time * 0.25f) * TILT_SPRINT;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.STAND)
				{
					return Mathf.Sin(TILT_STAND * Time.time * 0.5f) * TILT_STAND * 0.5f;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.SWIM)
				{
					return Mathf.Sin(TILT_SWIM * Time.time * 0.25f) * TILT_SWIM * 0.25f;
				}
				else
				{
					return 0;
				}
			}
		}

		public float speed
		{
			get
			{
				if (Player.LocalPlayer.stance.stance == EPlayerStance.SPRINT)
				{
					return SPEED_SPRINT;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.STAND)
				{
					return SPEED_STAND;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.CROUCH)
				{
					return SPEED_CROUCH;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.PRONE)
				{
					return SPEED_PRONE;
				}
				else if (Player.LocalPlayer.stance.stance == EPlayerStance.SWIM)
				{
					return SPEED_SWIM;
				}
				else
				{
					return 0;
				}
			}
		}

		private void onLifeUpdated(bool isDead)
		{
			if (gesture != EPlayerGesture.NONE)
			{
				if (gesture == EPlayerGesture.INVENTORY_START)
				{
					stop("Gesture_Inventory");
				}
				else if (gesture == EPlayerGesture.SURRENDER_START)
				{
					stop("Gesture_Surrender");
				}
				else if (gesture == EPlayerGesture.ARREST_START)
				{
					stop("Gesture_Arrest");
				}
				else if (gesture == EPlayerGesture.REST_START)
				{
					stop("Gesture_Rest");
				}
				else if (gesture == EPlayerGesture.T_POSE_START)
				{
					stop("T");
				}

				captorID = CSteamID.Nil;
				captorItem = 0;
				captorStrength = 0;
				_gesture = EPlayerGesture.NONE;

				onGestureUpdated?.Invoke(gesture);
			}

			if (channel.IsLocalPlayer)
			{
				UpdateLocalPlayerModelVisibility(isDead, player.look.perspective, player.quests.IsCutsceneModeActive());
			}
			else
			{
				if (!Dedicator.IsDedicatedServer && !isHiddenWaitingForClothing)
				{
					if (thirdRenderer_0 != null)
					{
						thirdRenderer_0.enabled = !isDead;
					}

					if (thirdRenderer_1 != null)
					{
						thirdRenderer_1.enabled = !isDead;
					}
				}

				thirdSkeleton.gameObject.SetActive(!isDead);
			}
		}

		/// <summary>
		/// Called by clothing to make mesh renderers visible.
		/// </summary>
		public void NotifyClothingIsVisible()
		{
			isHiddenWaitingForClothing = false;

			if (!channel.IsLocalPlayer)
			{
				if (!Dedicator.IsDedicatedServer && player.life.IsAlive)
				{
					if (thirdRenderer_0 != null)
					{
						thirdRenderer_0.enabled = true;
					}

					if (thirdRenderer_1 != null)
					{
						thirdRenderer_1.enabled = true;
					}

					thirdSkeleton.gameObject.SetActive(true);
				}
			}
		}

		public static event System.Action<PlayerAnimator> OnLeanChanged_Global;

		[System.Obsolete]
		public void tellLean(CSteamID steamID, byte newLean)
		{
			ReceiveLean(newLean);
		}

		private static readonly ClientInstanceMethod<byte> SendLean = ClientInstanceMethod<byte>.Get(typeof(PlayerAnimator), nameof(ReceiveLean));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLean))]
		public void ReceiveLean(byte newLean)
		{
			_lean = newLean - 1;
		}

		[System.Obsolete]
		public void tellGesture(CSteamID steamID, byte id)
		{
			ReceiveGesture((EPlayerGesture) id);
		}

		private static readonly ClientInstanceMethod<EPlayerGesture> SendGesture = ClientInstanceMethod<EPlayerGesture>.Get(typeof(PlayerAnimator), nameof(ReceiveGesture));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellGesture))]
		public void ReceiveGesture(EPlayerGesture newGesture)
		{
			if (newGesture == EPlayerGesture.INVENTORY_START && gesture == EPlayerGesture.NONE)
			{
				play("Gesture_Inventory", true);
				_gesture = EPlayerGesture.INVENTORY_START;
			}
			else if (newGesture == EPlayerGesture.INVENTORY_STOP && gesture == EPlayerGesture.INVENTORY_START)
			{
				stop("Gesture_Inventory");
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.PICKUP)
			{
				play("Gesture_Pickup", false);
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.PUNCH_LEFT)
			{
				play("Punch_Left", false);
				_gesture = EPlayerGesture.NONE;

				if (!Dedicator.IsDedicatedServer)
				{
					player.equipment.PlayPunchAudioClip();
				}
			}
			else if (newGesture == EPlayerGesture.PUNCH_RIGHT)
			{
				play("Punch_Right", false);
				_gesture = EPlayerGesture.NONE;

				if (!Dedicator.IsDedicatedServer)
				{
					player.equipment.PlayPunchAudioClip();
				}
			}
			else if (newGesture == EPlayerGesture.SURRENDER_START && gesture == EPlayerGesture.NONE)
			{
				play("Gesture_Surrender", true);
				_gesture = EPlayerGesture.SURRENDER_START;
			}
			else if (newGesture == EPlayerGesture.SURRENDER_STOP && gesture == EPlayerGesture.SURRENDER_START)
			{
				stop("Gesture_Surrender");
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.REST_START && gesture == EPlayerGesture.NONE)
			{
				play("Gesture_Rest", true);
				_gesture = EPlayerGesture.REST_START;
			}
			else if (newGesture == EPlayerGesture.REST_STOP && gesture == EPlayerGesture.REST_START)
			{
				stop("Gesture_Rest");
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.T_POSE_START && gesture == EPlayerGesture.NONE)
			{
				play("T", false);
				_gesture = EPlayerGesture.T_POSE_START;
			}
			else if (newGesture == EPlayerGesture.T_POSE_STOP && gesture == EPlayerGesture.T_POSE_START)
			{
				stop("T");
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.ARREST_START)
			{
				play("Gesture_Arrest", true);
				_gesture = EPlayerGesture.ARREST_START;
			}
			else if (newGesture == EPlayerGesture.ARREST_STOP && gesture == EPlayerGesture.ARREST_START)
			{
				stop("Gesture_Arrest");
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.POINT && gesture == EPlayerGesture.NONE)
			{
				play("Gesture_Point", false);
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.WAVE && gesture == EPlayerGesture.NONE)
			{
				play("Gesture_Wave", false);
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.SALUTE && gesture == EPlayerGesture.NONE)
			{
				play("Gesture_Salute", false);
				_gesture = EPlayerGesture.NONE;
			}
			else if (newGesture == EPlayerGesture.FACEPALM && gesture == EPlayerGesture.NONE)
			{
				play("Gesture_Facepalm", false);
				_gesture = EPlayerGesture.NONE;
			}

			onGestureUpdated?.Invoke(gesture);
		}

		public delegate void InventoryGestureListener(bool InInventory);
		/// <summary>
		/// Event for server plugins to monitor whether player is in-inventory.
		/// </summary>
		public InventoryGestureListener onInventoryGesture;

		[System.Obsolete]
		public void askGesture(CSteamID steamID, byte id)
		{
			ReceiveGestureRequest((EPlayerGesture) id);
		}

		private static readonly ServerInstanceMethod<EPlayerGesture> SendGestureRequest = ServerInstanceMethod<EPlayerGesture>.Get(typeof(PlayerAnimator), nameof(ReceiveGestureRequest));
		/// <summary>
		/// Rate limit is relatively high because this RPC handles open/close inventory notification.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 15, legacyName = nameof(askGesture))]
		public void ReceiveGestureRequest(EPlayerGesture newGesture)
		{
			if (newGesture == EPlayerGesture.INVENTORY_STOP)
			{
				if (player.inventory.isStoring && player.inventory.shouldInventoryStopGestureCloseStorage)
				{
					player.inventory.closeStorage();
				}
			}

			if (gesture == EPlayerGesture.ARREST_START) // prevents us from using gestures when arrested
			{
				return;
			}

			if (player.equipment.HasValidUseable)
			{
				return;
			}

			if (player.stance.stance == EPlayerStance.PRONE || player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
			{
				return;
			}

			if (newGesture == EPlayerGesture.INVENTORY_START
				|| newGesture == EPlayerGesture.INVENTORY_STOP
				|| newGesture == EPlayerGesture.SURRENDER_START
				|| newGesture == EPlayerGesture.SURRENDER_STOP
				|| newGesture == EPlayerGesture.POINT
				|| newGesture == EPlayerGesture.WAVE
				|| newGesture == EPlayerGesture.SALUTE
				|| newGesture == EPlayerGesture.FACEPALM
				|| newGesture == EPlayerGesture.REST_START
				|| newGesture == EPlayerGesture.REST_STOP
				|| newGesture == EPlayerGesture.T_POSE_START
				|| newGesture == EPlayerGesture.T_POSE_STOP)
			{
				bool sendToAll = newGesture != EPlayerGesture.INVENTORY_START && newGesture != EPlayerGesture.INVENTORY_STOP;
				sendGesture(newGesture, sendToAll);

				if (!sendToAll && onInventoryGesture != null)
				{
					onInventoryGesture(newGesture == EPlayerGesture.INVENTORY_START);
				}
			}
		}

		public void sendGesture(EPlayerGesture gesture, bool all)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (gesture == EPlayerGesture.INVENTORY_STOP)
				{
					if (player.inventory.isStoring && player.inventory.shouldInventoryStopGestureCloseStorage)
					{
						player.inventory.closeStorage();
					}
				}
			}

			if (Provider.isServer)
			{
				EPlayerStance? requiredStance = null;
				switch (gesture)
				{
					case EPlayerGesture.REST_START:
						requiredStance = EPlayerStance.CROUCH;
						break;

					case EPlayerGesture.T_POSE_START:
						requiredStance = EPlayerStance.STAND;
						break;
				}

				if (requiredStance.HasValue)
				{
					if (player.stance.stance != requiredStance.Value)
					{
						if (player.stance.stance != EPlayerStance.STAND
							&& player.stance.stance != EPlayerStance.CROUCH
							&& player.stance.stance != EPlayerStance.PRONE)
						{
							// Only standing or prone can switch to crouch for rest gesture.
							// Prevents using rest while swimming or other invalid cases.
							return;
						}

						player.stance.checkStance(requiredStance.Value, true);

						if (player.stance.stance != requiredStance.Value)
						{
							// Change stance failed e.g. insufficient space for overlap.
							return;
						}
					}
				}

				if (all)
				{
					SendGesture.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), gesture);
				}
				else
				{
					SendGesture.Invoke(GetNetId(), ENetReliability.Reliable, channel.GatherRemoteClientConnectionsExcludingOwner(), gesture);
				}
				OnGestureChanged_Global?.TryInvoke("OnGestureChanged_Global", this, gesture); // NOT_OWNER is not called on the server.
			}
			else
			{
				if (gesture != EPlayerGesture.INVENTORY_STOP)
				{
					if (player.equipment.HasValidUseable)
					{
						return;
					}

					if (player.stance.stance == EPlayerStance.PRONE || player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
					{
						return;
					}
				}

				SendGestureRequest.Invoke(GetNetId(), ENetReliability.Reliable, gesture); // reliable because of inventory_stop
			}
		}

		private void updateState(CharacterAnimator charAnim)
		{
			if (player.movement.isMoving)
			{
				if (player.stance.stance == EPlayerStance.CLIMB)
				{
					charAnim.state("Move_Climb");
				}
				else if (player.stance.stance == EPlayerStance.SWIM)
				{
					charAnim.state("Move_Swim");
				}
				else if (player.stance.stance == EPlayerStance.SPRINT)
				{
					charAnim.state("Move_Run");
				}
				else if (player.stance.stance == EPlayerStance.STAND)
				{
					charAnim.state("Move_Walk");
				}
				else if (player.stance.stance == EPlayerStance.CROUCH)
				{
					charAnim.state("Move_Crouch");
				}
				else if (player.stance.stance == EPlayerStance.PRONE)
				{
					charAnim.state("Move_Prone");
				}
			}
			else
			{
				if (player.stance.stance == EPlayerStance.DRIVING)
				{
					if (player.movement.getVehicle() != null && player.movement.getVehicle().asset.hasZip)
					{
						charAnim.state("Idle_Zip");
					}
					else if (player.movement.getVehicle() != null && player.movement.getVehicle().asset.hasBicycle)
					{
						charAnim.state("Idle_Bicycle");
						charAnim.setAnimationSpeed("Idle_Bicycle", player.movement.getVehicle().ReplicatedForwardVelocity * player.movement.getVehicle().asset.bicycleAnimSpeed);
					}
					else if (player.movement.getVehicle() != null && player.movement.getVehicle().asset.isReclined)
					{
						charAnim.state("Idle_Reclined");
					}
					else
					{
						charAnim.state("Idle_Drive");
					}
				}
				else if (player.stance.stance == EPlayerStance.SITTING)
				{
					if (player.movement.getVehicle() != null && player.movement.getVehicle().passengers[player.movement.getSeat()].turret != null)
					{
						charAnim.state("Idle_Drive");
					}
					else
					{
						charAnim.state("Idle_Sit");
					}
				}
				else if (player.stance.stance == EPlayerStance.CLIMB)
				{
					charAnim.state("Idle_Climb");
				}
				else if (player.stance.stance == EPlayerStance.SWIM)
				{
					charAnim.state("Idle_Swim");
				}
				else if (player.stance.stance == EPlayerStance.STAND || player.stance.stance == EPlayerStance.SPRINT)
				{
					charAnim.state("Idle_Stand");
				}
				else if (player.stance.stance == EPlayerStance.CROUCH)
				{
					charAnim.state("Idle_Crouch");
				}
				else if (player.stance.stance == EPlayerStance.PRONE)
				{
					charAnim.state("Idle_Prone");
				}
			}
		}

		private void updateHuman(HumanAnimator humanAnim)
		{
			humanAnim.lean = player.channel.owner.IsLeftHanded ? -lean : lean;

			if (player.stance.stance == EPlayerStance.DRIVING
				|| player.stance.stance == EPlayerStance.SITTING
				|| gesture == EPlayerGesture.T_POSE_START)
			{
				humanAnim.pitch = 90;
			}
			else
			{
				humanAnim.pitch = player.look.pitch;
			}

			if (player.stance.stance == EPlayerStance.CROUCH)
			{
				humanAnim.offset = 0.1f;//-0.05f;
			}
			else if (player.stance.stance == EPlayerStance.PRONE)
			{
				humanAnim.offset = 0.2f;//-0.1f;
			}
			else
			{
				humanAnim.offset = 0;
			}

			if (!channel.IsLocalPlayer && Provider.isServer)
			{
				humanAnim.force();
			}
		}

		private void onLanded(float velocity)
		{
			if (velocity < 0.0f)
			{
				if (player.movement.totalGravityMultiplier < 0.67f)
				{
					velocity = Mathf.Max(velocity, -5.0f);
				}
				else
				{
					velocity = Mathf.Max(velocity, -30.0f);
				}

				landedSpring.targetPosition = velocity * -0.5f;
			}
		}

		private static Collider[] leanHits = new Collider[1];
		private bool isLeanSpaceEmpty(Vector3 direction)
		{
			Vector3 startPosition = transform.position + (transform.up * player.look.heightLook);
			float testRadius = PlayerStance.RADIUS;
			float testDistance = 1.2f - testRadius;
			Vector3 endPosition = startPosition + (direction * testDistance);
			int hitCount = Physics.OverlapCapsuleNonAlloc(startPosition, endPosition, testRadius, leanHits, RayMasks.BLOCK_LEAN);
			return hitCount == 0;
		}

		private bool ShouldSnapLeanRotationToZero()
		{
			if (leanObstructed)
			{
				return true;
			}

			// Camera updates at a higher rate the _lean value, so we need to check whether we can lean to prevent
			// players from forcing their camera through walls by quickly spinning.
			if (_lean == 1)
			{
				leanObstructed = !isLeanSpaceEmpty(-transform.right);
			}
			else if (_lean == -1)
			{
				leanObstructed = !isLeanSpaceEmpty(transform.right);
			}

			return leanObstructed;
		}

		public void simulate(uint simulation, bool inputLeanLeft, bool inputLeanRight)
		{
			if (player.stance.stance != EPlayerStance.CLIMB && player.stance.stance != EPlayerStance.SPRINT && player.stance.stance != EPlayerStance.DRIVING && player.stance.stance != EPlayerStance.SITTING)
			{
				// Nelson 2025-01-20: Received a complaint about holding lean left and lean right at the same time
				// preferring lean left. Left==Right will stop lean when no input and when both input.
				if (inputLeanLeft == inputLeanRight)
				{
					_lean = 0;
					leanObstructed = false;
				}
				else if (inputLeanLeft)
				{
					if (isLeanSpaceEmpty(-transform.right))
					{
						_lean = 1;
						leanObstructed = false;
					}
					else
					{
						_lean = 0;
						leanObstructed = true;
					}
				}
				else if (inputLeanRight)
				{
					if (isLeanSpaceEmpty(transform.right))
					{
						_lean = -1;
						leanObstructed = false;
					}
					else
					{
						_lean = 0;
						leanObstructed = true;
					}
				}
			}
			else
			{
				_lean = 0;
				leanObstructed = false;
			}

			if (lastLean != lean)
			{
				lastLean = lean;

				if (Provider.isServer)
				{
					if (lean == -1 || lean == 1)
					{
						if (captorStrength > 0)
						{
							captorStrength--;

							if (captorStrength == 0)
							{
								captorID = CSteamID.Nil;
								captorItem = 0;
								sendGesture(EPlayerGesture.ARREST_STOP, true);

								EffectAsset metal_1 = Metal_1_Ref.Find();
								if (metal_1 != null)
								{
									TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(metal_1);
									triggerEffectParameters.relevantDistance = EffectManager.MEDIUM;
									triggerEffectParameters.position = transform.position;
									triggerEffectParameters.reliable = true;
									EffectManager.triggerEffect(triggerEffectParameters);
								}
							}
						}
					}

					SendLean.Invoke(GetNetId(), ENetReliability.Reliable, channel.GatherRemoteClientConnectionsExcludingOwner(), (byte) (lean + 1));
					OnLeanChanged_Global.TryInvoke("OnLeanChanged_Global", this);
				}
			}
		}

		private static readonly AssetReference<EffectAsset> Metal_1_Ref = new AssetReference<EffectAsset>("805bb3b0752749d1b5cf9959d17e104e"); // (36)

		[System.Obsolete]
		public void askEmote(CSteamID steamID)
		{ }

		internal void SendInitialPlayerState(SteamPlayer client)
		{
			if (gesture != EPlayerGesture.NONE)
			{
				SendGesture.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, gesture);
			}
		}

		internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
		{
			if (gesture != EPlayerGesture.NONE)
			{
				SendGesture.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, gesture);
			}
		}

		private bool hasCalledUpdateLocalPlayerModelVisibility;
		private bool wasLocalPlayerFirstPersonModelVisible;
		private bool wasLocalPlayerThirdPersonModelVisible;

		/// <summary>
		/// Nelson 2024-03-20: Adding this method because (at the time of writing) first and third-person renderers
		/// and skeletons are activated/enabled in InitializePlayer, onPerspectiveUpdated, and onLifeUpdated, and I
		/// want them to be consistent with the addition of the new NPC Cutscene Mode option.
		/// </summary>
		private void UpdateLocalPlayerModelVisibility(bool isDead, EPlayerPerspective perspective, bool isCutsceneModeActive)
		{
			bool firstPersonModelVisible = !isDead && perspective == EPlayerPerspective.FIRST && !isCutsceneModeActive;
			bool thirdPersonModelVisible = !isDead && perspective == EPlayerPerspective.THIRD;

			if (!hasCalledUpdateLocalPlayerModelVisibility || wasLocalPlayerFirstPersonModelVisible != firstPersonModelVisible)
			{
				wasLocalPlayerFirstPersonModelVisible = firstPersonModelVisible;

				if (firstRenderer_0 != null)
				{
					firstRenderer_0.enabled = firstPersonModelVisible;
				}

				firstSkeleton.gameObject.SetActive(firstPersonModelVisible);
			}

			if (!hasCalledUpdateLocalPlayerModelVisibility || wasLocalPlayerThirdPersonModelVisible != thirdPersonModelVisible)
			{
				wasLocalPlayerThirdPersonModelVisible = thirdPersonModelVisible;

				if (thirdRenderer_0 != null)
				{
					thirdRenderer_0.enabled = thirdPersonModelVisible;
				}

				if (thirdRenderer_1 != null)
				{
					thirdRenderer_1.enabled = thirdPersonModelVisible;
				}

				thirdSkeleton.gameObject.SetActive(thirdPersonModelVisible);
			}

			hasCalledUpdateLocalPlayerModelVisibility = true;
		}

		internal void NotifyLocalPlayerCutsceneModeActiveChanged(bool isCutsceneModeActive)
		{
			UpdateLocalPlayerModelVisibility(player.life.isDead, player.look.perspective, isCutsceneModeActive);
		}

		private void onPerspectiveUpdated(EPlayerPerspective newPerspective)
		{
			UpdateLocalPlayerModelVisibility(player.life.isDead, newPerspective, player.quests.IsCutsceneModeActive());
		}

		private void Update()
		{
			if (channel.IsLocalPlayer)
			{
				if (!PlayerUI.window.showCursor)
				{
					if (!player.look.IsControllingFreecam)
					{
						if (ControlsSettings.leaning == EControlMode.TOGGLE)
						{
							if (InputEx.GetKeyDown(ControlsSettings.leanLeft))
							{
								if (player.look.perspective == EPlayerPerspective.FIRST || side)
								{
									if (leanLeft)
									{
										inputWantsToLeanLeft = false;
										inputWantsToLeanRight = false;
									}
									else
									{
										inputWantsToLeanLeft = true;
										inputWantsToLeanRight = false;
									}
								}

								if (!side && leanRight)
								{
									inputWantsToLeanLeft = false;
									inputWantsToLeanRight = false;
								}

								inputWantsThirdPersonCameraOnLeftSide = true;
							}

							if (InputEx.GetKeyDown(ControlsSettings.leanRight))
							{
								if (player.look.perspective == EPlayerPerspective.FIRST || !side)
								{
									if (leanRight)
									{
										inputWantsToLeanLeft = false;
										inputWantsToLeanRight = false;
									}
									else
									{
										inputWantsToLeanLeft = false;
										inputWantsToLeanRight = true;
									}
								}

								if (side && leanLeft)
								{
									inputWantsToLeanLeft = false;
									inputWantsToLeanRight = false;
								}

								inputWantsThirdPersonCameraOnLeftSide = false;
							}
						}
						else
						{
							if (InputEx.GetKeyDown(ControlsSettings.leanLeft))
							{
								inputWantsThirdPersonCameraOnLeftSide = true;

								lastCameraSideInputRealtime = Time.realtimeSinceStartup;
							}

							if (InputEx.GetKeyDown(ControlsSettings.leanRight))
							{
								inputWantsThirdPersonCameraOnLeftSide = false;

								lastCameraSideInputRealtime = Time.realtimeSinceStartup;
							}

							if (InputEx.GetKey(ControlsSettings.leanLeft))
							{
								if (player.look.perspective == EPlayerPerspective.FIRST || Time.realtimeSinceStartup - lastCameraSideInputRealtime > 0.075f)
								{
									inputWantsToLeanLeft = true;
								}
								else
								{
									inputWantsToLeanLeft = false;
								}
							}
							else
							{
								inputWantsToLeanLeft = false;
							}

							if (InputEx.GetKey(ControlsSettings.leanRight))
							{
								if (player.look.perspective == EPlayerPerspective.FIRST || Time.realtimeSinceStartup - lastCameraSideInputRealtime > 0.075f)
								{
									inputWantsToLeanRight = true;
								}
								else
								{
									inputWantsToLeanRight = false;
								}
							}
							else
							{
								inputWantsToLeanRight = false;
							}
						}
					}
				}
				else
				{
					inputWantsToLeanLeft = false;
					inputWantsToLeanRight = false;
				}

				if (firstAnimator != null)
				{
					if (firstAnimator.getAnimationPlaying())
					{
						firstAnimator.state("Idle_Stand");
					}
					else
					{
						updateState(firstAnimator);
					}
				}

				if (thirdAnimator != null)
				{
					updateState(thirdAnimator);
					updateHuman((HumanAnimator) thirdAnimator);
				}

				blendedViewmodelSwayMultiplier = Mathf.Lerp(blendedViewmodelSwayMultiplier, viewmodelSwayMultiplier, 16 * Time.deltaTime);
				blendedViewmodelOffsetPreferenceMultiplier = Mathf.Lerp(blendedViewmodelOffsetPreferenceMultiplier, viewmodelOffsetPreferenceMultiplier, 16 * Time.deltaTime);

				float bobScaleOption = (Provider.modeConfigData?.Gameplay?.Disable_Motion_Sickness_Options ?? false) ? 1.0f : OptionsSettings.viewmodelBobScale;

				Vector3 aimingAlignmentOffset;
				float aimingInertaMultiplier;
				GetAimingViewmodelAlignment(out aimingAlignmentOffset, out aimingInertaMultiplier, out float aimingAlpha);

				float aimingMisalignmentScale = Provider.modeConfigData?.Gameplay?.Viewmodel_AimingMisalignmentMultiplier ?? 1.0f;
				float misalignmentScale = Mathf.Lerp(1.0f, aimingMisalignmentScale, aimingAlpha);

				float aimingJumpLandScale = Provider.modeConfigData?.Gameplay?.Viewmodel_AimingJumpLandMultiplier ?? 1.0f;
				float jumpLandScale = Mathf.Lerp(1.0f, aimingJumpLandScale, aimingAlpha);

				if (player.movement.isMoving)
				{
					viewmodelMovementOffset.targetPosition.x = Mathf.Sin(speed * Time.time) * bob * bobScaleOption * misalignmentScale;
					viewmodelMovementOffset.targetPosition.y = Mathf.Abs(viewmodelMovementOffset.targetPosition.x);
				}
				else
				{
					viewmodelMovementOffset.targetPosition = Vector2.zero;
				}
				viewmodelMovementOffset.Update(Time.deltaTime);

				blendedViewmodelCameraLocalPositionOffset = Vector3.Lerp(blendedViewmodelCameraLocalPositionOffset, viewmodelCameraLocalPositionOffset - recoilViewmodelCameraOffset.currentPosition * misalignmentScale - bayonetViewmodelCameraOffset, 16 * Time.deltaTime);
				recoilViewmodelCameraOffset.Update(Time.deltaTime);
				bayonetViewmodelCameraOffset = Vector3.Lerp(bayonetViewmodelCameraOffset, Vector3.zero, 16 * Time.deltaTime);

				desiredViewmodelCameraLocalPosition.x = -viewmodelMovementOffset.currentPosition.y - blendedViewmodelCameraLocalPositionOffset.y;
				desiredViewmodelCameraLocalPosition.y = viewmodelMovementOffset.currentPosition.x + blendedViewmodelCameraLocalPositionOffset.x;
				desiredViewmodelCameraLocalPosition.z = blendedViewmodelCameraLocalPositionOffset.z;

				desiredViewmodelCameraLocalPosition.x += Provider.preferenceData.Viewmodel.Offset_Vertical * blendedViewmodelOffsetPreferenceMultiplier;
				desiredViewmodelCameraLocalPosition.y += Provider.preferenceData.Viewmodel.Offset_Horizontal * blendedViewmodelOffsetPreferenceMultiplier;
				desiredViewmodelCameraLocalPosition.z -= Provider.preferenceData.Viewmodel.Offset_Depth * blendedViewmodelOffsetPreferenceMultiplier;

				if (player.stance.stance == EPlayerStance.DRIVING)
				{
					viewmodelCameraLocalPosition.x = Mathf.Lerp(viewmodelCameraLocalPosition.x, -drivingViewmodelCameraLocalPositionOffset.y - 0.65f - (Mathf.Abs(player.look.yaw) / 90f * 0.25f), 8 * Time.deltaTime);
					viewmodelCameraLocalPosition.y = Mathf.Lerp(viewmodelCameraLocalPosition.y, drivingViewmodelCameraLocalPositionOffset.x + ((channel.owner.IsLeftHanded ? -1 : 1) * player.movement.getVehicle().AnimatedSteeringAngle * -0.01f), 8 * Time.deltaTime);
					viewmodelCameraLocalPosition.z = Mathf.Lerp(viewmodelCameraLocalPosition.z, drivingViewmodelCameraLocalPositionOffset.z - 0.25f, 8 * Time.deltaTime);
				}
				else
				{
					viewmodelCameraLocalPosition.x = desiredViewmodelCameraLocalPosition.x - 0.45f;
					viewmodelCameraLocalPosition.y = desiredViewmodelCameraLocalPosition.y;
					viewmodelCameraLocalPosition.z = desiredViewmodelCameraLocalPosition.z;
				}

				AddNearDeathViewmodelShake(ref viewmodelCameraLocalPosition, misalignmentScale);

				// If changing how position is set remember to also modify behavior in LateUpdate.
				viewmodelCameraTransform.localPosition = viewmodelCameraLocalPosition + aimingAlignmentOffset;

				if (player.movement.isMoving)
				{
					viewmodelCameraMovementLocalRotation.targetPosition.x = (player.movement.move.z * tilt * viewmodelSwayMultiplier * bobScaleOption * misalignmentScale) + (roll * viewmodelSwayMultiplier * bobScaleOption * misalignmentScale);
					viewmodelCameraMovementLocalRotation.targetPosition.y = (player.movement.move.x * tilt * bobScaleOption * misalignmentScale) + (roll * viewmodelSwayMultiplier * bobScaleOption * misalignmentScale);
				}
				else
				{
					viewmodelCameraMovementLocalRotation.targetPosition = Vector2.zero;
				}
				if (!player.movement.isGrounded)
				{
					viewmodelCameraMovementLocalRotation.targetPosition.x -= 5.0f * misalignmentScale * jumpLandScale;
				}
				viewmodelCameraMovementLocalRotation.Update(Time.deltaTime);

				landedSpring.Update(Time.deltaTime);
				landedSpring.targetPosition = Mathf.Lerp(landedSpring.targetPosition, 0.0f, landedSpringRecoverySpeed * Time.deltaTime);

				viewmodelCameraLocalRotation.x = viewmodelCameraMovementLocalRotation.currentPosition.x + landedSpring.currentPosition * misalignmentScale * jumpLandScale;
				viewmodelCameraLocalRotation.y = 0.0f;
				viewmodelCameraLocalRotation.z = viewmodelCameraMovementLocalRotation.currentPosition.y;

				viewmodelCameraLocalRotation += recoilViewmodelCameraRotation.currentPosition * misalignmentScale;
				recoilViewmodelCameraRotation.Update(Time.deltaTime);

				float deltaPitchInput = Mathf.DeltaAngle(player.look.pitch, lastFramePitchInput);
				lastFramePitchInput = player.look.pitch;
				float deltaYawInput = Mathf.DeltaAngle(player.look.yaw, lastFrameYawInput);
				lastFrameYawInput = player.look.yaw;

				// Rolling works well because it does not interfere with aiming.
				rotationInputViewmodelRoll.Update(Time.deltaTime);
				rotationInputViewmodelRoll.currentPosition.x += deltaPitchInput * -0.03f * viewmodelSwayMultiplier * bobScaleOption * misalignmentScale;
				rotationInputViewmodelRoll.currentPosition.y += deltaYawInput * -0.015f * viewmodelSwayMultiplier * bobScaleOption * misalignmentScale;
				rotationInputViewmodelRoll.currentPosition.z += deltaYawInput * -0.05f * bobScaleOption	* misalignmentScale;
				// Clamp within a reasonable range otherwise spinning rapidly could make the viewmodel go upside-down.
				rotationInputViewmodelRoll.currentPosition = MathfEx.Clamp(rotationInputViewmodelRoll.currentPosition, -10.0f, 10.0f);

				viewmodelCameraLocalRotation += rotationInputViewmodelRoll.currentPosition;

				viewmodelItemInertiaRotation.Update(Time.deltaTime);

				if (player.look.perspective == EPlayerPerspective.FIRST && player.equipment.firstModel != null && (player.equipment.asset?.shouldProcedurallyAnimateInertia ?? false))
				{
					// Relative to the fixed-position skeleton root NOT the camera (because camera is affected by this delta)
					Vector3 itemRelativePosition = viewmodelParentTransform.transform.InverseTransformPoint(player.equipment.firstModel.position);
					if (lastFrameHadItemPosition)
					{
						Vector3 positionDelta = itemRelativePosition - lastFrameItemPosition;
						viewmodelItemInertiaRotation.currentPosition.x += positionDelta.y * viewmodelItemInertiaMask.x;
						viewmodelItemInertiaRotation.currentPosition.y += positionDelta.x * viewmodelItemInertiaMask.y;
						viewmodelItemInertiaRotation.currentPosition.z += positionDelta.x * viewmodelItemInertiaMask.z;
					}

					lastFrameItemPosition = itemRelativePosition;
					lastFrameHadItemPosition = true;
				}
				else
				{
					lastFrameHadItemPosition = false;
				}

				// Clamp within a reasonable range otherwise large equip movements go off-screen.
				viewmodelItemInertiaRotation.currentPosition = MathfEx.Clamp(viewmodelItemInertiaRotation.currentPosition, -5.0f, 5.0f);
				viewmodelCameraLocalRotation += viewmodelItemInertiaRotation.currentPosition * aimingInertaMultiplier;

				viewmodelSmoothedExplosionLocalRotation = Quaternion.Lerp(viewmodelSmoothedExplosionLocalRotation, viewmodelTargetExplosionLocalRotation.currentRotation, viewmodelExplosionSmoothingSpeed * Time.deltaTime);
				viewmodelTargetExplosionLocalRotation.Update(Time.deltaTime);

				if (player.stance.stance == EPlayerStance.DRIVING)
				{
					viewmodelCameraTransform.localRotation = Quaternion.Lerp(viewmodelCameraTransform.localRotation, Quaternion.Euler(player.look.yaw * 60f / MainCamera.instance.fieldOfView * (channel.owner.IsLeftHanded ? 1 : -1), (player.look.pitch - 90) * 60f / MainCamera.instance.fieldOfView, 90 + (player.movement.getVehicle().AnimatedSteeringAngle * (channel.owner.IsLeftHanded ? -1 : 1))), 8 * Time.deltaTime);
				}
				else if (player.stance.stance == EPlayerStance.CLIMB)
				{
					viewmodelCameraTransform.localRotation = Quaternion.Lerp(viewmodelCameraTransform.localRotation, Quaternion.Euler(0, (player.look.pitch - 90) * 60f / MainCamera.instance.fieldOfView, 90), 8 * Time.deltaTime);
				}
				else
				{
					viewmodelCameraTransform.localRotation = viewmodelTargetExplosionLocalRotation.currentRotation * Quaternion.Euler(viewmodelCameraLocalRotation.y, -viewmodelCameraLocalRotation.x, viewmodelCameraLocalRotation.z + 90);
				}

				if (ShouldSnapLeanRotationToZero())
				{
					player.first.transform.localRotation = Quaternion.identity;
				}
				else
				{
					player.first.transform.localRotation = Quaternion.Lerp(player.first.transform.localRotation, Quaternion.Euler(0, 0, lean * HumanAnimator.LEAN), 4.0f * Time.deltaTime);
				}

				viewmodelCamera.fieldOfView = Mathf.Lerp(viewmodelOffsetPreferenceUseScope ? Provider.preferenceData.Viewmodel.Field_Of_View_Aim_Scope
					: Provider.preferenceData.Viewmodel.Field_Of_View_Aim, Provider.preferenceData.Viewmodel.Field_Of_View_Hip, blendedViewmodelOffsetPreferenceMultiplier);

				if (Provider.modeConfigData.Gameplay.Allow_Shoulder_Camera)
				{
					_shoulder = Mathf.Lerp(shoulder, side ? -1 : 1, 8 * Time.deltaTime);
				}
				else
				{
					_shoulder = 0;
				}

				_shoulder2 = Mathf.Lerp(shoulder2, -lean, 8 * Time.deltaTime);
			}
			else
			{
				UnityEngine.Profiling.Profiler.BeginSample("Third");
				if (thirdAnimator != null)
				{
					updateState(thirdAnimator);
					updateHuman((HumanAnimator) thirdAnimator);
				}
				UnityEngine.Profiling.Profiler.EndSample();
			}

			UnityEngine.Profiling.Profiler.BeginSample("Character");
			if (characterAnimator != null)
			{
				updateState(characterAnimator);
				updateHuman(characterAnimator);
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

#if !DEDICATED_SERVER
		/// <summary>
		/// 2023-01-18: Viewmodel camera position was originally set during Update (and still is for compatibility),
		/// but for aiming alignment that uses the previous frame's animation position, so we also modify during
		/// LateUpdate to use this frame's animation position.
		/// </summary>
		private void LateUpdate()
		{
			if (channel.IsLocalPlayer)
			{
				Vector3 aimingAlignmentOffset;
				float aimingInertaMultiplier;
				GetAimingViewmodelAlignment(out aimingAlignmentOffset, out aimingInertaMultiplier, out float aimingAlpha);

				viewmodelCameraTransform.localPosition = viewmodelCameraLocalPosition + aimingAlignmentOffset;
			}
		}
#endif // !DEDICATED_SERVER

		internal void InitializePlayer()
		{
			isHiddenWaitingForClothing = true;

			if (channel.IsLocalPlayer)
			{
				if (player.first != null)
				{
					viewmodelParentTransform = new GameObject().transform;
					viewmodelParentTransform.name = "View";
					viewmodelParentTransform.transform.localPosition = Vector3.zero;

					firstAnimator = MainCamera.instance.transform.Find("Viewmodel").GetComponent<CharacterAnimator>();

					Vector3 localPos = firstAnimator.transform.localPosition;
					Quaternion localRot = firstAnimator.transform.localRotation;

					firstAnimator.transform.parent = viewmodelParentTransform;
					firstAnimator.transform.localPosition = localPos;
					firstAnimator.transform.localRotation = localRot;
					firstAnimator.transform.localScale = new Vector3(channel.owner.IsLeftHanded ? -1 : 1, 1, 1);

					firstRenderer_0 = (SkinnedMeshRenderer) firstAnimator.transform.Find("Model_0").GetComponent<Renderer>();

					_firstSkeleton = firstAnimator.transform.Find("Skeleton");
				}

				if (player.third != null)
				{
					thirdAnimator = player.third.GetComponent<CharacterAnimator>();
					thirdAnimator.transform.localScale = new Vector3(channel.owner.IsLeftHanded ? -1 : 1, 1, 1);

					thirdRenderer_0 = (SkinnedMeshRenderer) thirdAnimator.transform.Find("Model_0").GetComponent<Renderer>();
					thirdRenderer_1 = (SkinnedMeshRenderer) thirdAnimator.transform.Find("Model_1").GetComponent<Renderer>();

					_thirdSkeleton = thirdAnimator.transform.Find("Skeleton");

					thirdSkeleton.Find("Spine").GetComponent<Collider>().enabled = false;
					thirdSkeleton.Find("Spine").Find("Skull").GetComponent<Collider>().enabled = false;
					thirdSkeleton.Find("Spine").Find("Left_Shoulder").Find("Left_Arm").GetComponent<Collider>().enabled = false;
					thirdSkeleton.Find("Spine").Find("Right_Shoulder").Find("Right_Arm").GetComponent<Collider>().enabled = false;
					thirdSkeleton.Find("Left_Hip").Find("Left_Leg").GetComponent<Collider>().enabled = false;
					thirdSkeleton.Find("Right_Hip").Find("Right_Leg").GetComponent<Collider>().enabled = false;
				}

				if (Provider.cameraMode == ECameraMode.THIRD)
				{
					// Maintains previous behavior of setting third-person models active.
					UpdateLocalPlayerModelVisibility(false, EPlayerPerspective.THIRD, player.quests.IsCutsceneModeActive());
				}
				else
				{
					// Maintains previous behavior of setting first-person models active.
					UpdateLocalPlayerModelVisibility(false, EPlayerPerspective.FIRST, player.quests.IsCutsceneModeActive());
				}

				viewmodelCameraTransform = firstSkeleton.Find("Spine").Find("Skull").Find("ViewmodelCamera");
				viewmodelCamera = viewmodelCameraTransform.GetComponent<Camera>();
				UnturnedPostProcess.instance.setOverlayCamera(viewmodelCamera);

				viewmodelCameraLocalPositionOffset = Vector3.zero;
				drivingViewmodelCameraLocalPositionOffset = Vector3.zero;
				scopeSway = Vector3.zero;
				bayonetViewmodelCameraOffset = Vector3.zero;
				viewmodelCameraLocalPosition = Vector3.zero;

				viewmodelTargetExplosionLocalRotation.currentRotation = Quaternion.identity;
				viewmodelTargetExplosionLocalRotation.targetRotation = Quaternion.identity;

				blendedViewmodelSwayMultiplier = 1;
				viewmodelSwayMultiplier = 1;

				blendedViewmodelOffsetPreferenceMultiplier = 1.0f;
				viewmodelOffsetPreferenceMultiplier = 1.0f;

				if (player.character != null)
				{
					characterAnimator = player.character.GetComponent<HumanAnimator>();
					characterAnimator.transform.localScale = new Vector3(channel.owner.IsLeftHanded ? -1 : 1, 1, 1);
				}

				player.movement.onLanded += onLanded;

				inputWantsThirdPersonCameraOnLeftSide = player.channel.owner.IsLeftHanded;

				player.look.onPerspectiveUpdated += onPerspectiveUpdated;
			}
			else
			{
				if (player.third != null)
				{
					thirdAnimator = player.third.GetComponent<CharacterAnimator>();
					thirdAnimator.transform.localScale = new Vector3(channel.owner.IsLeftHanded ? -1 : 1, 1, 1);

					if (!Dedicator.IsDedicatedServer)
					{
						thirdRenderer_0 = (SkinnedMeshRenderer) thirdAnimator.transform.Find("Model_0").GetComponent<Renderer>();
						thirdRenderer_1 = (SkinnedMeshRenderer) thirdAnimator.transform.Find("Model_1").GetComponent<Renderer>();
					}

					_thirdSkeleton = thirdAnimator.transform.Find("Skeleton");
				}
			}

			if (Dedicator.IsDedicatedServer)
			{
				thirdSkeleton.gameObject.SetActive(true);
			}

			mixAnimation("Gesture_Inventory", true, true, true);
			mixAnimation("Gesture_Pickup", false, true);
			mixAnimation("Punch_Left", true, false);
			mixAnimation("Punch_Right", false, true);
			mixAnimation("Gesture_Point", false, true);
			mixAnimation("Gesture_Surrender", true, true);
			mixAnimation("Gesture_Arrest", true, true);
			mixAnimation("Gesture_Wave", true, true, true);
			mixAnimation("Gesture_Salute", false, true);
			mixAnimation("Gesture_Rest");
			mixAnimation("Gesture_Facepalm", false, true, true);
			mixAnimation("T");

			player.life.onLifeUpdated += onLifeUpdated;

			if (Provider.isServer)
			{
				load();
			}
		}

		private void AddNearDeathViewmodelShake(ref Vector3 position, float misalignmentScale)
		{
			const int nearDeathViewmodelShakeThreshold = 25;
			if (player.life.health < nearDeathViewmodelShakeThreshold)
			{
				// Shaky hands near death.
				const float magnitude = 0.005f;
				Vector3 randomVector = new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude));
				float healthMultiplier = 1.0f - (Player.LocalPlayer.life.health / (float) nearDeathViewmodelShakeThreshold); // Fade in as health approaches zero.
				float skillMultiplier = 1.0f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.TOUGHNESS) * 0.75f);
				position += randomVector * healthMultiplier * skillMultiplier * misalignmentScale;
			}
		}

		private void GetAimingViewmodelAlignment(out Vector3 aimingAlignmentOffset, out float aimingInertaMultiplier, out float aimingAlpha)
		{
			aimingAlignmentOffset = Vector3.zero;
			aimingInertaMultiplier = 1.0f; // Disable inertia from ADS anim because it looks bad.
			aimingAlpha = 0.0f;
			if (player.equipment.useable is UseableGun gun)
			{
				Transform aimingAlignmentTransform;
				Vector3 aimingAlignmentLocalPosition;
				gun.GetAimingViewmodelAlignment(out aimingAlignmentTransform, out aimingAlignmentLocalPosition, out aimingAlpha);
				if (aimingAlignmentTransform != null && aimingAlpha > 0.0f)
				{
					Vector3 aimingAlignmentWorldPosition = aimingAlignmentTransform.TransformPoint(aimingAlignmentLocalPosition);
					aimingAlignmentOffset = viewmodelCameraTransform.parent.InverseTransformPoint(aimingAlignmentWorldPosition);
					aimingAlignmentOffset.x += 0.45f;
					aimingAlignmentOffset *= aimingAlpha;
					aimingInertaMultiplier -= aimingAlpha;
				}
			}
		}

		private bool wasLoadCalled;

		public void load()
		{
			wasLoadCalled = true;

			if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Anim.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				Block block = PlayerSavedata.readBlock(channel.owner.playerID, "/Player/Anim.dat", 0);
				byte version = block.readByte();

				_gesture = (EPlayerGesture) block.readByte();
				captorID = block.readSteamID();

				if (version > 1)
				{
					captorItem = block.readUInt16();
				}
				else
				{
					captorItem = 0;
				}

				captorStrength = block.readUInt16();

				if (gesture != EPlayerGesture.ARREST_START)
				{
					_gesture = EPlayerGesture.NONE;
				}

				return;
			}

			_gesture = EPlayerGesture.NONE;
			captorID = CSteamID.Nil;
			captorItem = 0;
			captorStrength = 0;
		}

		public void save()
		{
			if (!wasLoadCalled)
				return;

			if (player.life.isDead)
			{
				if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Anim.dat"))
				{
					PlayerSavedata.deleteFile(channel.owner.playerID, "/Player/Anim.dat");
				}
			}
			else
			{
				Block block = new Block();
				block.writeByte(SAVEDATA_VERSION);

				block.writeByte((byte) gesture);
				block.writeSteamID(captorID);
				block.writeUInt16(captorItem);
				block.writeUInt16(captorStrength);

				PlayerSavedata.writeBlock(channel.owner.playerID, "/Player/Anim.dat", block);
			}
		}
	}
}
