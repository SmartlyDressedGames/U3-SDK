////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define WITH_CAMERA_SWEEP_GIZMOS // Draw sweeps and hits when right arrow key is pressed.

using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void PerspectiveUpdated(EPlayerPerspective newPerspective);

	public partial class PlayerLook : PlayerCaller
	{
		private static readonly float HEIGHT_LOOK_SIT = 1.6f;
		private static readonly float HEIGHT_LOOK_STAND = 1.75f;
		private static readonly float HEIGHT_LOOK_CROUCH = 1.2f;
		private static readonly float HEIGHT_LOOK_PRONE = 0.35f;

		public float heightLook
		{
			get
			{
				if (player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
				{
					return HEIGHT_LOOK_SIT;
				}
				else if (player.stance.stance == EPlayerStance.STAND || player.stance.stance == EPlayerStance.SPRINT || player.stance.stance == EPlayerStance.CLIMB || player.stance.stance == EPlayerStance.SWIM || player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
				{
					return HEIGHT_LOOK_STAND;
				}
				else if (player.stance.stance == EPlayerStance.CROUCH)
				{
					return HEIGHT_LOOK_CROUCH;
				}
				else if (player.stance.stance == EPlayerStance.PRONE)
				{
					return HEIGHT_LOOK_PRONE;
				}

				return 0;
			}
		}

		private static readonly float HEIGHT_CAMERA_SIT = 0.7f;
		private static readonly float HEIGHT_CAMERA_STAND = 1.05f;
		private static readonly float HEIGHT_CAMERA_CROUCH = 0.95f;
		private static readonly float HEIGHT_CAMERA_PRONE = 0.3f;

		private float heightCamera
		{
			get
			{
				if (player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
				{
					return HEIGHT_CAMERA_SIT;
				}
				else if (player.stance.stance == EPlayerStance.STAND || player.stance.stance == EPlayerStance.SPRINT || player.stance.stance == EPlayerStance.CLIMB || player.stance.stance == EPlayerStance.SWIM || player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
				{
					return HEIGHT_CAMERA_STAND;
				}
				else if (player.stance.stance == EPlayerStance.CROUCH)
				{
					return HEIGHT_CAMERA_CROUCH;
				}
				else if (player.stance.stance == EPlayerStance.PRONE)
				{
					return HEIGHT_CAMERA_PRONE;
				}

				return 0;
			}
		}

		private static readonly float MIN_ANGLE_SIT = 60;
		private static readonly float MAX_ANGLE_SIT = 120;
		private static readonly float MIN_ANGLE_CLIMB = 45;
		private static readonly float MAX_ANGLE_CLIMB = 100;
		private static readonly float MIN_ANGLE_SWIM = 45;
		private static readonly float MAX_ANGLE_SWIM = 135;
		private static readonly float MIN_ANGLE_STAND = 0;
		private static readonly float MAX_ANGLE_STAND = 180;
		private static readonly float MIN_ANGLE_CROUCH = 20;
		private static readonly float MAX_ANGLE_CROUCH = 160;
		private static readonly float MIN_ANGLE_PRONE = 60;
		private static readonly float MAX_ANGLE_PRONE = 120;

		public PerspectiveUpdated onPerspectiveUpdated;

		private Camera _characterCamera;
		public Camera characterCamera => _characterCamera;

		private Camera _scopeCamera;
		public Camera scopeCamera => _scopeCamera;

		/// <summary>
		/// Material instantiated when dual-render scopes are enabled.
		/// Overrides the material of the gun sight attachment.
		/// </summary>
		public Material scopeMaterial
		{
			get;
			private set;
		}

		internal bool isSingleRenderScopeVisionAppliedToLighting;
		private bool _isScopeActive;
		public bool isScopeActive => _isScopeActive;

		private bool _isScopeHalfwayAimedIn;
		internal bool IsScopeHalfwayAimedIn
		{
			get => _isScopeHalfwayAimedIn;
			set
			{
				if (_isScopeHalfwayAimedIn != value)
				{
					_isScopeHalfwayAimedIn = value;
					UpdateSingleRenderScope();
				}
			}
		}

		internal float scopeAlpha;

		private ELightingVision scopeVision;
		private Color scopeNightvisionColor;
		private float scopeNightvisionFogIntensity;
		private ELightingVision tempVision;
		private Color tempNightvisionColor;
		private float tempNightvisionFogIntensity;

		private Transform _aim;
		public Transform aim => _aim;

		private static float characterHeight;
		private static float _characterYaw;
		public static float characterYaw;
		private static float killcam;

		// These are used to distort input while under the effects of hallucinogens.
		private float yawInputMultiplier;
		private float pitchInputMultiplier;

		private float _pitch = 90.0f;
		/// <summary>
		/// Unintuitively (to say the least), a pitch of 0 is up, 90 is forward, and 180 is down.
		/// </summary>
		public float pitch => _pitch;

		private float _yaw;
		public float yaw => _yaw;

		internal void TeleportYaw(float newYaw)
		{
			_yaw = newYaw;
			clampYaw();
			transform.localRotation = Quaternion.Euler(0, _yaw, 0);
		}

		/// <summary>
		/// Nelson 2025-06-27: previously, stopping aim cancelled the sway offset immediately. When
		/// experimenting with removing the dual-render scope blur this felt jarring.
		/// </summary>
		internal void ConvertScopeSwayToInputRotation()
		{
			_pitch += player.animator.scopeSway.x;
			_yaw += player.animator.scopeSway.y;
			player.animator.scopeSway = Vector3.zero;
		}

		private float _look_x;
		public float look_x => _look_x;

		private float _look_y;
		public float look_y => _look_y;

		private float _orbitPitch;
		public float orbitPitch => _orbitPitch;

		private float _orbitYaw;
		public float orbitYaw => _orbitYaw;

		public float orbitSpeed = 16;
		/// <summary>
		/// Reset to actual fov when first used.
		/// </summary>
		public float freecamVerticalFieldOfView = -1.0f;
		public Vector3 lockPosition;
		public Vector3 orbitPosition;
		/// <summary>
		/// If true, freecam controls take input priority.
		/// Previously named isOrbiting.
		/// </summary>
		public bool IsControllingFreecam;
		public bool isTracking;
		public bool isLocking;
		public bool isFocusing;
		public bool isSmoothing;

		/// <summary>
		/// Should player stats be visible in spectator mode?
		/// </summary>
		public bool areSpecStatsVisible
		{
			get;
			protected set;
		}

		public bool isIgnoringInput;

		private Vector3 smoothPosition;
		private Quaternion smoothRotation;

		public bool IsLocallyUsingFreecam => IsControllingFreecam || isTracking || isLocking || isFocusing;

		public byte angle;
		public byte rot;

		//		private float buffer_x;
		//		private float buffer_y;

		private float recoil_x;
		private float recoil_y;

		public byte lastAngle;
		public byte lastRot;
		private Quaternion flinchLocalRotation;
		public Rk4SpringQ targetExplosionLocalRotation;
		/// <summary>
		/// Smoothing adds some initial blend-in which felt nicer for explosion rumble.
		/// </summary>
		private Quaternion smoothedExplosionLocalRotation = Quaternion.identity;
		public float explosionSmoothingSpeed;

		private float mainCameraTargetFieldOfView;
		private void UpdateMainCameraTargetFieldOfView(float target)
		{
			if (mainCameraTargetFieldOfView < 0.001f)
			{
				mainCameraTargetFieldOfView = target;
			}
			else
			{
				mainCameraTargetFieldOfView = Mathf.Lerp(mainCameraTargetFieldOfView, target, 8f * Time.deltaTime);
			}
		}

		internal float mainCameraZoomFactor;
		internal float scopeCameraZoomFactor;
		private bool mainCameraZoomFactorUsesScopeAlpha;
		private float eyes;

		/// <summary>
		/// Slightly clamped third-person version of "eyes" value to prevent sweep from hitting floor.
		/// </summary>
		private float thirdPersonEyeHeight;

		public bool shouldUseZoomFactorForSensitivity;

		private EPlayerPerspective _perspective;
		public EPlayerPerspective perspective => _perspective;

		/// <summary>
		/// Get point-of-view in world-space.
		/// </summary>
		public Vector3 getEyesPosition()
		{
			return aim.position;
		}

		/// <summary>
		/// Get point of view in worldspace without the left/right leaning modifier.
		/// </summary>
		public Vector3 GetEyesPositionWithoutLeaning()
		{
			// The "aim" transform is child of another transform with zeroed position which gets rotated according to
			// the leaning angle, so transforming the aim localPosition effectively bypasses that transformation.
			return transform.TransformPoint(aim.localPosition);
		}

		private RenderTexture scopeRenderTexture;

		public void updateScope(EGraphicQuality quality)
		{
			bool wantsRenderTexture = true;
			bool isDualRender = false;
			int desiredResolution = 0;

			if (IsUsing2DScope)
			{
				wantsRenderTexture = false;
			}
			else
			{
				switch (quality)
				{
					case EGraphicQuality.LOW:
						isDualRender = true;
						desiredResolution = 256;
						break;

					case EGraphicQuality.MEDIUM:
						isDualRender = true;
						desiredResolution = 512;
						break;

					case EGraphicQuality.HIGH:
						isDualRender = true;
						desiredResolution = 1024;
						break;

					case EGraphicQuality.ULTRA:
						isDualRender = true;
						desiredResolution = 2048;
						break;

					default:
					case EGraphicQuality.OFF:
						isDualRender = false;
						desiredResolution = Mathf.Min(Screen.width, Screen.height);
						break;
				}
			}

			if (scopeRenderTexture != null && (scopeRenderTexture.width != desiredResolution || !wantsRenderTexture))
			{
				if (scopeCamera.targetTexture == scopeRenderTexture)
				{
					// Avoid engine warning about releasing targetTexture.
					scopeCamera.targetTexture = null;
				}

				Destroy(scopeRenderTexture);
				scopeRenderTexture = null;
			}

			if (scopeRenderTexture == null && wantsRenderTexture)
			{
				var graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
				var depthStencilFormat = isDualRender ?
					UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt
					: UnityEngine.Experimental.Rendering.GraphicsFormat.None;
				scopeRenderTexture = new RenderTexture(desiredResolution, desiredResolution, graphicsFormat, depthStencilFormat);
				// Nelson 2025-07-04: we received some feedback that full-screen scope can feel blurry with mipmaps enabled.
				// Leaning toward that being higher importance than aliasing when unscoped (original issue #5079).
				//scopeRenderTexture.useMipMap = true;
				scopeRenderTexture.name = isDualRender ? "Dual-Render Scope" : "Single-Render Scope";
				scopeRenderTexture.hideFlags = HideFlags.HideAndDontSave;
			}

			if (isDualRender)
			{
				scopeCamera.targetTexture = scopeRenderTexture;
				UnturnedPostProcess.instance.SetSingleRenderScopeTarget(null);
			}
			else
			{
				scopeCamera.targetTexture = null;
				UnturnedPostProcess.instance.SetSingleRenderScopeTarget(scopeRenderTexture);
			}

			if (scopeMaterial == null)
			{
				scopeMaterial = Instantiate(Resources.Load<Material>("Materials/Scope"));
			}
			scopeMaterial.SetTexture("_MainTex", scopeRenderTexture);

			//UnturnedLog.info($"UpdateScope IsUsing2DScope: {IsUsing2DScope} wantsRenderTexture: {wantsRenderTexture} desiredResolution: {desiredResolution} rt: {scopeRenderTexture}");

			scopeCamera.enabled = isScopeActive && scopeCamera.targetTexture != null && scopeVision == ELightingVision.NONE;

			UpdateSingleRenderScope();

			if (player.equipment.useable is UseableGun gun)
			{
				gun.UpdateScopeAlpha();
			}
		}

		/// <summary>
		/// Moves legacy image effect dependency out of SDK release.
		/// </summary>
		partial void UpdateScopeGrayscaleEnabled();

		public void enableScope(float zoom, ItemSightAsset sightAsset)
		{
			scopeCameraZoomFactor = zoom;
			_isScopeActive = true;
			scopeVision = sightAsset.vision;
			scopeNightvisionColor = sightAsset.nightvisionColor;
			scopeNightvisionFogIntensity = sightAsset.nightvisionFogIntensity;
			scopeCamera.enabled = scopeCamera.targetTexture != null && scopeVision == ELightingVision.NONE;
			UpdateScopeGrayscaleEnabled();
			UpdateSingleRenderScope();
		}

		public void disableScope()
		{
			scopeCamera.enabled = false;
			_isScopeActive = false;
			scopeVision = ELightingVision.NONE;
			scopeAlpha = 0.0f;
			UpdateSingleRenderScope();
		}

		internal bool IsUsing2DScope => (Provider.modeConfigData?.Gameplay?.Use_2D_Scope_Overlay ?? false) && GraphicsSettings.scopeQuality == EGraphicQuality.OFF;

		internal void UpdateScopeOverlay()
		{
			bool visible;
			if (player.equipment.useable is UseableGun gun)
			{
				visible = gun.isAiming && IsUsing2DScope && isScopeActive && perspective == EPlayerPerspective.FIRST && PlayerLifeUI.scopeOverlay.scopeImage.Texture != null;
			}
			else
			{
				visible = false;
			}

			PlayerUI.updateScope(visible);
			UpdateScopeVisionAppliedToLighting();
		}

		private void UpdateSingleRenderScope()
		{
			bool shouldBeActive = perspective == EPlayerPerspective.FIRST && isScopeActive
				&& GraphicsSettings.scopeQuality == EGraphicQuality.OFF && !IsUsing2DScope;

			UnturnedPostProcess.instance.SetSingleRenderScopeIsActive(shouldBeActive);
			UpdateScopeVisionAppliedToLighting();
		}

		private void UpdateScopeVisionAppliedToLighting()
		{
			bool shouldBeActive;
			if (IsUsing2DScope)
			{
				shouldBeActive = PlayerLifeUI.scopeOverlay?.IsVisible ?? false;
			}
			else
			{
				shouldBeActive = UnturnedPostProcess.instance.IsSingleRenderScopeActive() && IsScopeHalfwayAimedIn;
			}

			if (shouldBeActive && scopeVision != ELightingVision.NONE)
			{
				if (!isSingleRenderScopeVisionAppliedToLighting)
				{
					isSingleRenderScopeVisionAppliedToLighting = true;
					ApplyScopeVisionToLighting();
				}
			}
			else
			{
				if (isSingleRenderScopeVisionAppliedToLighting)
				{
					isSingleRenderScopeVisionAppliedToLighting = false;
					// Apply glasses vision properties rather than RestoreSavedLightingVision because
					// it may have changed since the overlay was activated. (public issue #4257)
					player.equipment.updateVision();
				}
			}
		}

		[System.Obsolete("this was never supported server-side")]
		public void setPerspective(EPlayerPerspective newPerspective)
		{
			// Plugins may have been calling this function, so mark it obsolete and throw an exception rather than
			// simply removing when refactoring to make private.
			throw new System.NotSupportedException("this wwas never supported server-side");
		}

		private void setActivePerspective(EPlayerPerspective newPerspective)
		{
			_perspective = newPerspective;

			if (perspective == EPlayerPerspective.FIRST)
			{
				MainCamera.instance.transform.parent = player.first;
				MainCamera.instance.transform.localPosition = Vector3.up * eyes;

				IsControllingFreecam = false;
				isTracking = false;
				isLocking = false;
				isFocusing = false;
				player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Freecam, false);

				if (PlayerWorkzoneUI.active)
				{
					PlayerWorkzoneUI.close();
					PlayerLifeUI.open();
				}
			}
			else
			{
				MainCamera.instance.transform.parent = player.transform;
			}

			UpdateSingleRenderScope();

			onPerspectiveUpdated?.Invoke(perspective);

			UnturnedPostProcess.instance.notifyPerspectiveChanged();
		}

		private void ApplyScopeVisionToLighting()
		{
			tempVision = LevelLighting.vision;
			tempNightvisionColor = LevelLighting.nightvisionColor;
			tempNightvisionFogIntensity = LevelLighting.nightvisionFogIntensity;
			LevelLighting.vision = scopeVision;
			LevelLighting.nightvisionColor = scopeNightvisionColor;
			LevelLighting.nightvisionFogIntensity = scopeNightvisionFogIntensity;
			LevelLighting.updateLighting();
			LevelLighting.ForceRefreshForLatestViewer();
			PlayerLifeUI.updateGrayscale();
		}

		/// <summary>
		/// This is only used after capturing dual-render scope, not when exiting scope overlay.
		/// Otherwise the lighting vision may have changed between entering and exiting the scope.
		/// </summary>
		private void RestoreSavedLightingVision()
		{
			LevelLighting.vision = tempVision;
			LevelLighting.nightvisionColor = tempNightvisionColor;
			LevelLighting.nightvisionFogIntensity = tempNightvisionFogIntensity;
			LevelLighting.updateLighting();
			LevelLighting.ForceRefreshForLatestViewer();
			PlayerLifeUI.updateGrayscale();
			tempVision = ELightingVision.NONE;
		}

		public void enableZoom(float zoom, bool useAlpha)
		{
			mainCameraZoomFactor = zoom;
			mainCameraZoomFactorUsesScopeAlpha = useAlpha;
		}

		public void disableZoom()
		{
			mainCameraZoomFactor = 0.0f;
			mainCameraZoomFactorUsesScopeAlpha = false;
		}

		public void updateRot()
		{
			if (pitch < 0)
			{
				angle = 0;
			}
			else if (pitch > 180)
			{
				angle = 180;
			}
			else
			{
				angle = (byte) pitch;
			}

			rot = MeasurementTool.angleToByte(yaw);
		}

		public void updateLook()
		{
			_pitch = 90;
			_yaw = transform.localRotation.eulerAngles.y;
			updateRot();

			if (channel.IsLocalPlayer)
			{
				if (perspective == EPlayerPerspective.FIRST)
				{
					MainCamera.instance.transform.localRotation = Quaternion.Euler(pitch - 90, 0, 0);

					MainCamera.instance.transform.localPosition = Vector3.up * eyes;
				}
			}
		}

		public void recoil(float x, float y, float h, float v)
		{
			_yaw += x;
			_pitch -= y;

			recoil_x += x * h;
			recoil_y += y * v;
		}

		public void simulate(float look_x, float look_y, float delta)
		{
			_pitch = look_y;
			_yaw = look_x;

			clampPitch();
			clampYaw();
			updateRot();

			if (player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
			{
				transform.localRotation = Quaternion.identity;
			}
			else
			{
				transform.localRotation = Quaternion.Euler(0, yaw, 0);
			}

			if (player.movement.getVehicle() != null && player.movement.getVehicle().passengers[player.movement.getSeat()].turret != null)
			{
				Passenger seat = player.movement.getVehicle().passengers[player.movement.getSeat()];
				if (seat.turretYaw != null)
				{
					seat.turretYaw.localRotation = seat.rotationYaw * Quaternion.Euler(0, yaw, 0);
				}
				if (seat.turretPitch != null)
				{
					seat.turretPitch.localRotation = seat.rotationPitch * Quaternion.Euler(pitch - 90, 0, 0);
				}
			}

			updateAim(delta);
		}

		/// <summary>
		/// Clamp _pitch within the [0, 180] range.
		/// </summary>
		private void clampPitch()
		{
			float pitchMin;
			float pitchMax;

			Passenger seat = player.movement.getVehicleSeat();
			if (seat != null)
			{
				if (seat.turret != null)
				{
					pitchMin = seat.turret.pitchMin;
					pitchMax = seat.turret.pitchMax;
				}
				else
				{
					pitchMin = MIN_ANGLE_SIT;
					pitchMax = MAX_ANGLE_SIT;
				}
			}
			else if (player.stance.stance == EPlayerStance.STAND || player.stance.stance == EPlayerStance.SPRINT)
			{
				pitchMin = MIN_ANGLE_STAND;
				pitchMax = MAX_ANGLE_STAND;
			}
			else if (player.stance.stance == EPlayerStance.CLIMB)
			{
				pitchMin = MIN_ANGLE_CLIMB;
				pitchMax = MAX_ANGLE_CLIMB;
			}
			else if (player.stance.stance == EPlayerStance.SWIM)
			{
				pitchMin = MIN_ANGLE_SWIM;
				pitchMax = MAX_ANGLE_SWIM;
			}
			else if (player.stance.stance == EPlayerStance.CROUCH)
			{
				pitchMin = MIN_ANGLE_CROUCH;
				pitchMax = MAX_ANGLE_CROUCH;
			}
			else if (player.stance.stance == EPlayerStance.PRONE)
			{
				pitchMin = MIN_ANGLE_PRONE;
				pitchMax = MAX_ANGLE_PRONE;
			}
			else
			{
				pitchMin = 0.0f;
				pitchMax = 180.0f;
			}

			_pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
		}

		/// <summary>
		/// Clamp yaw while seated, and keep within the [-360, 360] range.
		/// </summary>
		private void clampYaw()
		{
			_yaw %= 360.0f; // Wrap before clamping because turret range may allow spinning.

			Passenger seat = player.movement.getVehicleSeat();
			if (seat == null)
				return;

			float yawMin;
			float yawMax;

			if (seat.turret != null)
			{
				yawMin = seat.turret.yawMin;
				yawMax = seat.turret.yawMax;
			}
			else if (player.stance.stance == EPlayerStance.DRIVING)
			{
				yawMin = -160.0f;
				yawMax = 160.0f;
			}
			else
			{
				yawMin = -90.0f;
				yawMax = 90.0f;
			}

			_yaw = Mathf.Clamp(_yaw, yawMin, yawMax);
		}

		public void updateAim(float delta)
		{
			if (player.movement.getVehicle() != null && player.movement.getVehicle().passengers[player.movement.getSeat()].turret != null && player.movement.getVehicle().passengers[player.movement.getSeat()].turret.useAimCamera)
			{
				Passenger seat = player.movement.getVehicle().passengers[player.movement.getSeat()];

				if (seat.turretAim != null)
				{
					aim.position = seat.turretAim.position;
					aim.rotation = seat.turretAim.rotation;
				}
			}
			else
			{
				aim.localPosition = Vector3.Lerp(aim.localPosition, Vector3.up * heightLook, 4 * delta);

				if (player.stance.stance == EPlayerStance.SITTING || player.stance.stance == EPlayerStance.DRIVING)
				{
					aim.parent.localRotation = Quaternion.Euler(0, yaw, 0);
				}
				else
				{
					if (player.animator.leanObstructed)
					{
						aim.parent.localRotation = Quaternion.identity;
					}
					else
					{
						aim.parent.localRotation = Quaternion.Lerp(aim.parent.localRotation, Quaternion.Euler(0, 0, player.animator.lean * HumanAnimator.LEAN), 4 * delta);
					}
				}

				aim.localRotation = Quaternion.Euler(pitch - 90 + player.animator.scopeSway.x, player.animator.scopeSway.y, 0);
			}
		}

		internal void FlinchFromDamage(byte damageAmount, Vector3 worldDirection)
		{
			Camera mainCamera = MainCamera.instance;
			if (mainCamera == null)
				return;

			GameplayConfigData config = Provider.modeConfigData?.Gameplay;
			if (config != null && !config.Enable_Damage_Flinch)
				return;

			float magnitude = Mathf.Min(damageAmount, 25) * 0.5f;

			// Toughness skill reduces rotation.
			float skillMultiplier = 1.0f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.TOUGHNESS) * 0.75f);
			magnitude *= skillMultiplier;

			EDamageFlinchMode damageFlinchMode = OptionsSettings.damageFlinchMode;
			if (config != null && config.Disable_Motion_Sickness_Options)
			{
				// Nelson 2025-06-27: public issue #5073 points out that all these accessibility options have
				// gradually detracted from combat-realism modes. Having distinct PvE and PvP focus in the same game
				// seems tricky in this regard and maybe something we can handle better in the future. If Unturned were
				// primarily PvE-focused it would be reasonable to always enable these accessibility options, or if it
				// were primarily PvP-focused it wouldn't be fair to include them. In the meantime, servers can opt-out
				// of accessibility for this reason.
				damageFlinchMode = EDamageFlinchMode.Directional;
			}
			else
			{
				// Nelson 2024-10-04: we received accessibility complaints from players experiencing motion sickness
				// from damage flinch, for example public issue #4675. Older versions used to only rotate around the forward
				// axis which might've reduced motion sickness. Admittedly, adding intensity options will make the
				// toughness skill less useful, but I think the accessibility aspect is more important. Some PvPers will
				// turn off damage flinch entirely for the gameplay benefit, but on the other hand it might be useful
				// in directional mode to determine where you're getting attacked from.
				magnitude *= OptionsSettings.damageFlinchIntensity;
			}

			// This produces an upward flinch when facing toward the damage.
			Vector3 worldRotationAxis = Vector3.Cross(Vector3.up, worldDirection).normalized;
			Vector3 localRotationAxis = mainCamera.transform.InverseTransformDirection(worldRotationAxis);

			if (damageFlinchMode == EDamageFlinchMode.RollOnly)
			{
				if (Mathf.Abs(localRotationAxis.z) < 0.001f)
				{
					// None of the rotation is around the roll axis.
					return;
				}

				localRotationAxis.x = 0.0f;
				localRotationAxis.y = 0.0f;
				localRotationAxis = localRotationAxis.normalized;
				// Now rotating only clockwise or counterclockwise.
			}

			// Rotation is in local space because main camera rotation is applied in local space.
			flinchLocalRotation *= Quaternion.AngleAxis(magnitude, localRotationAxis);
		}

		internal void FlinchFromExplosion(Vector3 position, float radius, float magnitudeDegrees)
		{
			Camera mainCamera = MainCamera.instance;
			if (mainCamera == null)
				return;

			GameplayConfigData config = Provider.modeConfigData?.Gameplay;
			if (config != null && !config.Enable_Explosion_Camera_Shake)
				return;

			Vector3 relativeToExplosion = mainCamera.transform.position - position;
			float distanceFromExplosion = relativeToExplosion.magnitude;
			if (distanceFromExplosion <= 0.0f || distanceFromExplosion >= radius)
				return;

			Vector3 worldDirection = relativeToExplosion / distanceFromExplosion;

			// This produces an upward flinch when facing toward the damage.
			Vector3 worldRotationAxis = Vector3.Cross(Vector3.up, worldDirection).normalized;
			Vector3 localRotationAxis = mainCamera.transform.InverseTransformDirection(worldRotationAxis);

			// Toughness skill reduces rotation.
			float skillMultiplier = 1.0f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.TOUGHNESS) * 0.5f);

			// Exponential falloff.
			float distanceMultiplier = 1.0f - MathfEx.Square(distanceFromExplosion / radius);

			magnitudeDegrees *= skillMultiplier * distanceMultiplier;
			if (config == null || !config.Disable_Motion_Sickness_Options)
			{
				magnitudeDegrees *= OptionsSettings.cameraShakeIntensity;
			}

			if (MathfEx.IsNearlyZero(magnitudeDegrees))
				return;

			// Rotation is in local space because main camera rotation is applied in local space.
			targetExplosionLocalRotation.currentRotation *= Quaternion.AngleAxis(magnitudeDegrees, localRotationAxis);

			player.animator.FlinchFromExplosion(worldDirection, magnitudeDegrees);
		}

		private void onVisionUpdated(bool isViewing)
		{
			if (isViewing)
			{
				yawInputMultiplier = Random.value < 0.25 ? -1.0f : 1.0f;
				pitchInputMultiplier = Random.value < 0.25 ? -1.0f : 1.0f;
			}
			else
			{
				yawInputMultiplier = 1.0f;
				pitchInputMultiplier = 1.0f;
			}
		}

		private void onLifeUpdated(bool isDead)
		{
			if (isDead)
			{
				killcam = transform.rotation.eulerAngles.y;
			}
		}

		private EVehicleThirdPersonCameraMode GetVehicleThirdPersonCameraMode(InteractableVehicle vehicle)
		{
			if (vehicle != null && vehicle.asset != null)
			{
				switch (vehicle.asset.engine)
				{
					case EEngine.HELICOPTER:
					case EEngine.BLIMP:
					case EEngine.PLANE:
						return OptionsSettings.vehicleAircraftThirdPersonCameraMode;
				}
			}

			return OptionsSettings.vehicleThirdPersonCameraMode;
		}

		private EVehicleThirdPersonCameraMode GetCurrentVehicleThirdPersonCameraMode()
		{
			InteractableVehicle vehicle = player.movement.getVehicle();
			return GetVehicleThirdPersonCameraMode(vehicle);
		}

		private void onSeated(bool isDriver, bool inVehicle, bool wasVehicle, InteractableVehicle oldVehicle, InteractableVehicle newVehicle)
		{
			if (!wasVehicle)
			{
				_orbitPitch = 22.5f;
				if (GetVehicleThirdPersonCameraMode(newVehicle) == EVehicleThirdPersonCameraMode.RotationDetached)
				{
					_orbitYaw = newVehicle?.transform.rotation.eulerAngles.y ?? 0.0f;
				}
				else
				{
					_orbitYaw = 0.0f;
				}
			}

			if (Provider.cameraMode == ECameraMode.VEHICLE && perspective == EPlayerPerspective.THIRD)
			{
				if (!isDriver)
				{
					setActivePerspective(EPlayerPerspective.FIRST);
				}
			}
		}

		/// <summary>
		/// Can spectating be used without admin powers?
		/// Plugins can enable spectator mode.
		/// </summary>
		protected bool allowFreecamWithoutAdmin = false;

		/// <summary>
		/// Can workzone be used without admin powers?
		/// Plugins can enable workzone permissions.
		/// </summary>
		protected bool allowWorkzoneWithoutAdmin = false;

		/// <summary>
		/// Can spectator overlays be used without admin powers?
		/// Plugins can enable specstats permissions.
		/// </summary>
		protected bool allowSpecStatsWithoutAdmin = false;

		public bool canUseFreecam
		{
			get
			{
				if (allowFreecamWithoutAdmin)
				{
					return true;
				}
				else
				{
					return channel.owner.isAdmin;
				}
			}
		}

		public bool canUseWorkzone
		{
			get
			{
				if (allowWorkzoneWithoutAdmin)
				{
					return true;
				}
				else
				{
					return channel.owner.isAdmin;
				}
			}
		}

		public bool canUseSpecStats
		{
			get
			{
				if (allowSpecStatsWithoutAdmin)
				{
					return true;
				}
				else
				{
					return channel.owner.isAdmin;
				}
			}
		}

		[System.Obsolete]
		public void tellFreecamAllowed(CSteamID senderId, bool isAllowed)
		{
			ReceiveFreecamAllowed(isAllowed);
		}

		private static readonly ClientInstanceMethod<bool> SendFreecamAllowed = ClientInstanceMethod<bool>.Get(typeof(PlayerLook), nameof(ReceiveFreecamAllowed));
		/// <summary>
		/// Called from the server to allow spectating without admin powers.
		/// Only used by plugins.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellFreecamAllowed))]
		public void ReceiveFreecamAllowed(bool isAllowed)
		{
			allowFreecamWithoutAdmin = isAllowed;
			if (!canUseFreecam && IsLocallyUsingFreecam)
			{
				IsControllingFreecam = false;
				isTracking = false;
				isLocking = false;
				isFocusing = false;
				player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Freecam, false);
			}
		}

		/// <summary>
		/// Allow use of spectator mode without admin powers.
		/// Only used by plugins.
		/// </summary>
		public void sendFreecamAllowed(bool isAllowed)
		{
			allowFreecamWithoutAdmin = isAllowed;
			SendFreecamAllowed.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), isAllowed);
		}

		[System.Obsolete]
		public void tellWorkzoneAllowed(CSteamID senderId, bool isAllowed)
		{
			ReceiveWorkzoneAllowed(isAllowed);
		}

		/// <summary>
		/// Called from the server to allow workzone without admin powers.
		/// Only used by plugins.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWorkzoneAllowed))]
		public void ReceiveWorkzoneAllowed(bool isAllowed)
		{
			allowWorkzoneWithoutAdmin = isAllowed;
			if (!canUseWorkzone && PlayerWorkzoneUI.active)
			{
				PlayerWorkzoneUI.close();
				PlayerLifeUI.open();
			}
		}

		private static readonly ClientInstanceMethod<bool> SendWorkzoneAllowed = ClientInstanceMethod<bool>.Get(typeof(PlayerLook), nameof(ReceiveWorkzoneAllowed));
		/// <summary>
		/// Allow use of workzone mode without admin powers.
		/// Only used by plugins.
		/// </summary>
		public void sendWorkzoneAllowed(bool isAllowed)
		{
			allowWorkzoneWithoutAdmin = isAllowed;
			SendWorkzoneAllowed.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), isAllowed);
		}

		[System.Obsolete]
		public void tellSpecStatsAllowed(CSteamID senderId, bool isAllowed)
		{
			ReceiveSpecStatsAllowed(isAllowed);
		}

		private static readonly ClientInstanceMethod<bool> SendSpecStatsAllowed = ClientInstanceMethod<bool>.Get(typeof(PlayerLook), nameof(ReceiveSpecStatsAllowed));
		/// <summary>
		/// Called from the server to allow spectator overlays without admin powers.
		/// Only used by plugins.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSpecStatsAllowed))]
		public void ReceiveSpecStatsAllowed(bool isAllowed)
		{
			allowSpecStatsWithoutAdmin = isAllowed;
			if (!canUseSpecStats)
			{
				areSpecStatsVisible = false;
				player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.SpectatorStatsOverlay, false);
			}
		}

		/// <summary>
		/// Allow use of spectator overlay mode without admin powers.
		/// Only used by plugins.
		/// </summary>
		public void sendSpecStatsAllowed(bool isAllowed)
		{
			allowSpecStatsWithoutAdmin = isAllowed;
			SendSpecStatsAllowed.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), isAllowed);
		}

		/// <summary>
		/// Multiple hits are necessary because the first returned hit is not always the closest.
		/// </summary>
		private static RaycastHit[] sweepHits = new RaycastHit[8];

		// Third-person camera sphere sweep radius. Needs to be a large similar to first-person because
		// otherwise wide aspect ratios with wide field of view easily clips through walls.
		private const float NEAR_CLIP_SWEEP_RADIUS = 0.39f; // PlayerStance.RADIUS

		/// <summary>
		/// Sweep a sphere to find collisions blocking the third-person camera.
		/// </summary>
		/// <returns>Valid world-space camera position.</returns>
		private Vector3 sphereCastCamera(Vector3 origin, Vector3 direction, float length, int layerMask)
		{
			Ray ray = new Ray(origin, direction);
			int hitCount = Physics.SphereCastNonAlloc(ray, NEAR_CLIP_SWEEP_RADIUS, sweepHits, length, layerMask, QueryTriggerInteraction.Ignore);

#if WITH_CAMERA_SWEEP_GIZMOS
			bool captureSweep = InputEx.GetKeyDown(KeyCode.RightArrow);
			if(captureSweep)
			{
				RuntimeGizmos.Get().Spherecast(ray, NEAR_CLIP_SWEEP_RADIUS, length, Color.green, lifespan: 10.0f);
			}
#endif

			float closestDistance = length;

			for (int hitIndex = 0; hitIndex < hitCount; ++hitIndex)
			{
				closestDistance = Mathf.Min(closestDistance, sweepHits[hitIndex].distance);

#if WITH_CAMERA_SWEEP_GIZMOS
				if(captureSweep)
				{
					Vector3 hitSurfacePoint = sweepHits[hitIndex].point;
					Vector3 hitSpherePoint = origin + direction * sweepHits[hitIndex].distance;
					RuntimeGizmos.Get().Sphere(hitSurfacePoint, 0.05f, Color.red, lifespan: 10.0f);
					RuntimeGizmos.Get().Sphere(hitSpherePoint, 0.05f, Color.white, lifespan: 10.0f);
					RuntimeGizmos.Get().Line(hitSurfacePoint, hitSpherePoint, Color.red, lifespan: 10.0f);
				}
#endif
			}

			return origin + (direction * closestDistance);
		}

		private void Update()
		{
			if (channel.IsLocalPlayer)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Spectate");

				if (InputEx.GetKey(KeyCode.LeftShift))
				{
					if (canUseFreecam) // sp || admin
					{
						if (InputEx.GetKeyDown(KeyCode.F1))
						{
							IsControllingFreecam = !IsControllingFreecam;

							if (IsControllingFreecam && !isTracking && !isLocking && !isFocusing)
							{
								isTracking = true;
							}

							player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Freecam, IsLocallyUsingFreecam);
						}

						if (InputEx.GetKeyDown(KeyCode.F2))
						{
							isTracking = !isTracking;

							if (isTracking)
							{
								isLocking = false;
								isFocusing = false;
							}

							player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Freecam, IsLocallyUsingFreecam);
						}

						if (InputEx.GetKeyDown(KeyCode.F3))
						{
							isLocking = !isLocking;

							if (isLocking)
							{
								isTracking = false;
								isFocusing = false;
								lockPosition = player.first.position;
							}

							player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Freecam, IsLocallyUsingFreecam);
						}

						if (InputEx.GetKeyDown(KeyCode.F4))
						{
							isFocusing = !isFocusing;

							if (isFocusing)
							{
								isTracking = false;
								isLocking = false;
								lockPosition = player.first.position;
							}

							player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.Freecam, IsLocallyUsingFreecam);
						}

						if (InputEx.GetKeyDown(KeyCode.F5))
						{
							isSmoothing = !isSmoothing;
						}
					}

					if (InputEx.GetKeyDown(KeyCode.F6))
					{
						if (PlayerWorkzoneUI.active)
						{
							PlayerWorkzoneUI.close();
							PlayerLifeUI.open();
						}
						else
						{
							if (canUseWorkzone && perspective == EPlayerPerspective.THIRD)
							{
								PlayerWorkzoneUI.open();
								PlayerLifeUI.close();
							}
						}
					}

					if (InputEx.GetKeyDown(KeyCode.F7))
					{
						if (areSpecStatsVisible)
						{
							areSpecStatsVisible = false;
						}
						else
						{
							if (canUseSpecStats)
							{
								areSpecStatsVisible = true;
							}
						}
						player.ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags.SpectatorStatsOverlay, areSpecStatsVisible);
					}
				}

				float targetEyeHeight = heightLook;
				eyes = Mathf.Lerp(eyes, targetEyeHeight, 4 * Time.deltaTime);
				if (player.movement.controller != null)
				{
					float minHeight = NEAR_CLIP_SWEEP_RADIUS + 0.005f;
					float maxHeight = player.movement.controller.height - NEAR_CLIP_SWEEP_RADIUS - 0.005f;
					thirdPersonEyeHeight = Mathf.Lerp(thirdPersonEyeHeight, Mathf.Clamp(targetEyeHeight, minHeight, maxHeight), 4 * Time.deltaTime);
				}

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Get Main Camera");

				Camera mainCamera = MainCamera.instance;

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Swap");

				if (player.life.IsAlive && !PlayerUI.window.showCursor)
				{
					if (InputEx.GetKeyDown(ControlsSettings.perspective) && (Provider.cameraMode == ECameraMode.BOTH || (Provider.cameraMode == ECameraMode.VEHICLE && player.stance.stance == EPlayerStance.DRIVING)))
					{
						EPlayerPerspective newPerspective;
						if (perspective == EPlayerPerspective.FIRST)
						{
							newPerspective = EPlayerPerspective.THIRD;
						}
						else
						{
							newPerspective = EPlayerPerspective.FIRST;
						}

						setActivePerspective(newPerspective);
					}

					if (IsLocallyUsingFreecam)
					{
						// Force into 3P when using freecam, even on 1P servers.
						if (perspective != EPlayerPerspective.THIRD)
						{
							setActivePerspective(EPlayerPerspective.THIRD);
						}

						if (InputEx.GetKeyDown(KeyCode.C) && InputEx.GetKey(KeyCode.LeftControl))
						{
							ChatManager.CopyCameraTransform();
						}

						if (InputEx.GetKeyDown(KeyCode.V) && InputEx.GetKey(KeyCode.LeftControl))
						{
							string input = GUIUtility.systemCopyBuffer;
							int positionRotationDelimiter = input.IndexOf(':');
							if (positionRotationDelimiter >= 0)
							{
								string positionSubstring = input.Substring(0, positionRotationDelimiter);
								string rotationSubstring = input.Substring(positionRotationDelimiter + 1);
								int pitchYawDelimiterIndex = rotationSubstring.IndexOf(',');
								string pitchSubstring = rotationSubstring.Substring(0, pitchYawDelimiterIndex);
								string yawSubstring = rotationSubstring.Substring(pitchYawDelimiterIndex + 1);
								if (Vector3Ex.TryParseVector3(positionSubstring, out Vector3 parsedPosition)
									&& float.TryParse(pitchSubstring, out float parsedPitch)
									&& float.TryParse(yawSubstring, out float parsedYaw))
								{
									if (isLocking)
									{
										orbitPosition = parsedPosition - lockPosition;
									}
									else
									{
										orbitPosition = parsedPosition - player.first.position;
									}

									// Nelson 2024-11-11: Since negative pitch is up from horizon (counterintuitive) 
									// the negative may be expressed as a positive number below 360. (e.g., 350 to
									// represent -10 degrees.) In that case we need to clamp it back to [-90, 90].
									if (parsedPitch > 180.0f)
									{
										parsedPitch = parsedPitch - 360.0f;
									}

									_orbitPitch = Mathf.Clamp(parsedPitch, -90.0f, 90.0f);
									_orbitYaw = parsedYaw;
								}
							}
						}
					}
					else
					{
						// Force back into 1P when not using freecam on 1P servers.
						if (Provider.cameraMode == ECameraMode.FIRST || (Provider.cameraMode == ECameraMode.VEHICLE && player.stance.stance != EPlayerStance.DRIVING))
						{
							if (perspective != EPlayerPerspective.FIRST)
							{
								setActivePerspective(EPlayerPerspective.FIRST);
							}
						}
					}
				}

				float zoomDesiredVerticalFieldOfView = OptionsSettings.GetZoomBaseFieldOfView();
				bool isMainCameraZoomFactorActive = false;
				bool isSingleRenderScopeZoomFactorActive = false;

				if (IsLocallyUsingFreecam)
				{
					if (freecamVerticalFieldOfView < 0.1f)
					{
						freecamVerticalFieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
					}

					if (isSmoothing)
					{
						mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, freecamVerticalFieldOfView, 4.0f * Time.deltaTime);
					}
					else
					{
						mainCamera.fieldOfView = freecamVerticalFieldOfView;
					}
				}
				else
				{
					// Nelson 2025-07-04: there's a lot in common between these branches. The main difference is that
					// players requesting the 2D scopes back prefer the lerp(a, b, 8 * t) blend, rather than scope alpha.
					if (IsUsing2DScope)
					{
						bool isAiming = (player.equipment.useable as UseableGun)?.isAiming ?? false;// total hack :P
						float targetFieldOfView;
						if (isScopeActive && scopeCameraZoomFactor > float.Epsilon && isAiming && perspective == EPlayerPerspective.FIRST)
						{
							targetFieldOfView = zoomDesiredVerticalFieldOfView / scopeCameraZoomFactor;
							isSingleRenderScopeZoomFactorActive = true;
						}
						else if (mainCameraZoomFactor > float.Epsilon && (!mainCameraZoomFactorUsesScopeAlpha || isAiming))
						{
							targetFieldOfView = zoomDesiredVerticalFieldOfView / mainCameraZoomFactor;
							isMainCameraZoomFactorActive = true;
						}
						else
						{
							float sprintFovBoost = player.stance.stance == EPlayerStance.SPRINT ? (OptionsSettings.sprintFovBoostIntensity * 10f) : 0f;
							targetFieldOfView = OptionsSettings.DesiredVerticalFieldOfView + sprintFovBoost;
						}
						UpdateMainCameraTargetFieldOfView(targetFieldOfView);
						mainCamera.fieldOfView = mainCameraTargetFieldOfView;
					}
					else
					{
						float targetFieldOfView;
						if (mainCameraZoomFactor > float.Epsilon && !mainCameraZoomFactorUsesScopeAlpha)
						{
							targetFieldOfView = zoomDesiredVerticalFieldOfView / mainCameraZoomFactor;
							isMainCameraZoomFactorActive = true;
						}
						else
						{
							float sprintFovBoost = player.stance.stance == EPlayerStance.SPRINT ? (OptionsSettings.sprintFovBoostIntensity * 10f) : 0f;
							targetFieldOfView = OptionsSettings.DesiredVerticalFieldOfView + sprintFovBoost;
						}
						UpdateMainCameraTargetFieldOfView(targetFieldOfView);

						if (isScopeActive && scopeCameraZoomFactor > float.Epsilon && GraphicsSettings.scopeQuality == EGraphicQuality.OFF && perspective == EPlayerPerspective.FIRST)
						{
							isSingleRenderScopeZoomFactorActive = scopeAlpha > 0.001f;
							mainCamera.fieldOfView = Mathf.Lerp(mainCameraTargetFieldOfView, zoomDesiredVerticalFieldOfView / scopeCameraZoomFactor, scopeAlpha);
						}
						else if (mainCameraZoomFactor > float.Epsilon && mainCameraZoomFactorUsesScopeAlpha && scopeAlpha > 0.0001f)
						{
							isMainCameraZoomFactorActive = true;
							mainCamera.fieldOfView = Mathf.Lerp(mainCameraTargetFieldOfView, zoomDesiredVerticalFieldOfView / mainCameraZoomFactor, scopeAlpha);
						}
						else
						{
							mainCamera.fieldOfView = mainCameraTargetFieldOfView;
						}
					}
				}
				if (isScopeActive && scopeCamera != null && scopeCameraZoomFactor > 0.0f)
				{
					scopeCamera.fieldOfView = zoomDesiredVerticalFieldOfView / scopeCameraZoomFactor;
				}

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Input");

				_look_x = 0;
				_look_y = 0;

				if (PlayerUI.window.isCursorLocked && !isIgnoringInput)
				{
					if (IsControllingFreecam)
					{
						if (!player.workzone.isBuilding || InputEx.GetKey(ControlsSettings.secondary))
						{
							float zoomSensitivity = 1.0f;
							switch (ControlsSettings.sensitivityScalingMode)
							{
								case ESensitivityScalingMode.ProjectionRatio:
									// Gamers online refer to this as "focal length sensitivity scaling". We adjust sensitivity
									// according to the ratio of projected size on the screen. Note that the ratio is the same
									// regardless of whether vertical or horizontal field of view is used.
									float halfCurrentFovRadians = Mathf.Deg2Rad * mainCamera.fieldOfView * 0.5f;
									float halfDesiredFovRadians = Mathf.Deg2Rad * OptionsSettings.DesiredVerticalFieldOfView * 0.5f;
									float coefficient = ControlsSettings.projectionRatioCoefficient;
									zoomSensitivity = Mathf.Atan(coefficient * Mathf.Tan(halfCurrentFovRadians)) / Mathf.Atan(coefficient * Mathf.Tan(halfDesiredFovRadians));
									break;

								case ESensitivityScalingMode.ZoomFactor:
								case ESensitivityScalingMode.Legacy:
									float zoomFactor = OptionsSettings.DesiredVerticalFieldOfView / mainCamera.fieldOfView;
									if (zoomFactor > 0.0f)
									{
										zoomSensitivity = 1.0f / zoomFactor;
									}
									break;
							}

							_orbitYaw += ControlsSettings.mouseAimSensitivity * zoomSensitivity * Input.GetAxis("mouse_x") * yawInputMultiplier;

							if (ControlsSettings.invert)
							{
								_orbitPitch += ControlsSettings.mouseAimSensitivity * zoomSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
							}
							else
							{
								_orbitPitch -= ControlsSettings.mouseAimSensitivity * zoomSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
							}
						}
					}
					else
					{
						if (perspective == EPlayerPerspective.FIRST || isTracking || isLocking || isFocusing)
						{
							_look_x = ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_x") * yawInputMultiplier;
							_look_y = ControlsSettings.mouseAimSensitivity * -Input.GetAxis("mouse_y") * pitchInputMultiplier;
						}

						if (InputEx.GetKey(ControlsSettings.rollLeft))
						{
							_look_x = player.movement.getVehicle() != null ? -player.movement.getVehicle().asset.airTurnResponsiveness : -1;
						}
						else if (InputEx.GetKey(ControlsSettings.rollRight))
						{
							_look_x = player.movement.getVehicle() != null ? player.movement.getVehicle().asset.airTurnResponsiveness : 1;
						}

						if (InputEx.GetKey(ControlsSettings.pitchUp))
						{
							_look_y = player.movement.getVehicle() != null ? -player.movement.getVehicle().asset.airTurnResponsiveness : -1;
						}
						else if (InputEx.GetKey(ControlsSettings.pitchDown))
						{
							_look_y = player.movement.getVehicle() != null ? player.movement.getVehicle().asset.airTurnResponsiveness : 1;
						}

						if (ControlsSettings.invertFlight)
						{
							_look_y *= -1;
						}

						float zoomSensitivity = 1.0f;
						switch (ControlsSettings.sensitivityScalingMode)
						{
							case ESensitivityScalingMode.ProjectionRatio:
								// Gamers online refer to this as "focal length sensitivity scaling". We adjust sensitivity
								// according to the ratio of projected size on the screen. Note that the ratio is the same
								// regardless of whether vertical or horizontal field of view is used.
								float currentFovDegrees = shouldUseZoomFactorForSensitivity && isScopeActive && perspective == EPlayerPerspective.FIRST && scopeCameraZoomFactor > 0.0f
									? scopeCamera.fieldOfView : mainCamera.fieldOfView;
								float halfCurrentFovRadians = Mathf.Deg2Rad * currentFovDegrees * 0.5f;
								float halfDesiredFovRadians = Mathf.Deg2Rad * OptionsSettings.DesiredVerticalFieldOfView * 0.5f;
								float coefficient = ControlsSettings.projectionRatioCoefficient;
								zoomSensitivity = Mathf.Atan(coefficient * Mathf.Tan(halfCurrentFovRadians)) / Mathf.Atan(coefficient * Mathf.Tan(halfDesiredFovRadians));
								break;

							case ESensitivityScalingMode.ZoomFactor:
							case ESensitivityScalingMode.Legacy:
								if (shouldUseZoomFactorForSensitivity)
								{
									if (isScopeActive && perspective == EPlayerPerspective.FIRST && scopeCameraZoomFactor > 0.0f)
									{
										zoomSensitivity = 1.0f / scopeCameraZoomFactor;
									}
									else if (isMainCameraZoomFactorActive)
									{
										zoomSensitivity = 1.0f / mainCameraZoomFactor;
									}
								}
								break;
						}

						if (player.movement.getVehicle() != null && perspective == EPlayerPerspective.THIRD)
						{
							_orbitYaw += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_x") * yawInputMultiplier;
							_orbitYaw = orbitYaw % 360.0f;
						}
						else
						{
							if (player.movement.getVehicle() == null || !player.movement.getVehicle().asset.hasLockMouse || !player.movement.getVehicle().isDriver)
							{
								_yaw += ControlsSettings.mouseAimSensitivity * zoomSensitivity * Input.GetAxis("mouse_x") * yawInputMultiplier;
							}
						}

						if (player.movement.getVehicle() != null && perspective == EPlayerPerspective.THIRD)
						{
							if (ControlsSettings.invert)
							{
								_orbitPitch += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
							}
							else
							{
								_orbitPitch -= ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
							}
						}
						else
						{
							if (player.movement.getVehicle() == null || !player.movement.getVehicle().asset.hasLockMouse || !player.movement.getVehicle().isDriver)
							{
								if (ControlsSettings.invert)
								{
									_pitch += ControlsSettings.mouseAimSensitivity * zoomSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
								}
								else
								{
									_pitch -= ControlsSettings.mouseAimSensitivity * zoomSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
								}
							}
						}
					}
				}

				if (float.IsInfinity(yaw) || float.IsNaN(yaw))
				{
					_yaw = 0;
				}

				if (float.IsInfinity(pitch) || float.IsNaN(pitch))
				{
					_pitch = 90;
				}

				if (float.IsInfinity(orbitYaw) || float.IsNaN(orbitYaw))
				{
					_orbitYaw = 0;
				}

				if (float.IsInfinity(orbitPitch) || float.IsNaN(orbitPitch))
				{
					_orbitPitch = 0;
				}

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Angle");

				float newRecoil_X = Mathf.Lerp(recoil_x, 0, 4 * Time.deltaTime);
				float recoilDelta_X = newRecoil_X - recoil_x;
				recoil_x = newRecoil_X;

				float newRecoil_Y = Mathf.Lerp(recoil_y, 0, 4 * Time.deltaTime);
				float recoilDelta_Y = newRecoil_Y - recoil_y;
				recoil_y = newRecoil_Y;

				_yaw += recoilDelta_X;
				_pitch -= recoilDelta_Y;

				flinchLocalRotation = Quaternion.Lerp(flinchLocalRotation, Quaternion.identity, 4 * Time.deltaTime);
				smoothedExplosionLocalRotation = Quaternion.Lerp(smoothedExplosionLocalRotation, targetExplosionLocalRotation.currentRotation, explosionSmoothingSpeed * Time.deltaTime);
				targetExplosionLocalRotation.Update(Time.deltaTime);

				clampPitch();
				clampYaw();

				if (orbitPitch > 90)
				{
					_orbitPitch = 90;
				}
				else if (orbitPitch < -90)
				{
					_orbitPitch = -90;
				}

				_characterYaw = Mathf.Lerp(_characterYaw, characterYaw + 180, 4 * Time.deltaTime);
				characterCamera.transform.rotation = Quaternion.Euler(20, _characterYaw, 0);
				characterCamera.transform.position = player.character.position - (characterCamera.transform.forward * 3.5f) + (Vector3.up * characterHeight);

				if (player.life.isDead)
				{
					killcam += -16 * Time.deltaTime;
					mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(32, killcam, 0), 2 * Time.deltaTime);
				}
				else
				{
					if ((player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING) && perspective == EPlayerPerspective.THIRD)
					{
						if (GetCurrentVehicleThirdPersonCameraMode() == EVehicleThirdPersonCameraMode.RotationDetached)
						{
							mainCamera.transform.rotation = Quaternion.Euler(orbitPitch, orbitYaw, 0);
						}
						else
						{
							mainCamera.transform.localRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0);
						}
					}
					else if (player.stance.stance == EPlayerStance.DRIVING)
					{
						mainCamera.transform.localRotation = Quaternion.Euler(pitch - 90, yaw / 10f, 0);
						mainCamera.transform.Rotate(transform.up, yaw, Space.World);
					}
					else if (player.stance.stance == EPlayerStance.SITTING)
					{
						mainCamera.transform.localRotation = Quaternion.Euler(pitch - 90 + player.animator.scopeSway.x, player.animator.scopeSway.y, 0);
						mainCamera.transform.Rotate(transform.up, yaw, Space.World);
					}
					else
					{
						if (perspective == EPlayerPerspective.FIRST)
						{
							mainCamera.transform.localRotation = smoothedExplosionLocalRotation * flinchLocalRotation * Quaternion.Euler(pitch - 90 + player.animator.scopeSway.x, player.animator.scopeSway.y, 0.0f);
						}
						else
						{
							mainCamera.transform.localRotation = smoothedExplosionLocalRotation * flinchLocalRotation * Quaternion.Euler(pitch - 90 + player.animator.scopeSway.x, (player.animator.shoulder * -5f) + player.animator.scopeSway.y, 0);
						}

						transform.localRotation = Quaternion.Euler(0, yaw, 0);
					}

					if (IsLocallyUsingFreecam)
					{
						if (isFocusing)
						{
							Vector3 focalPoint = player.first.position + Vector3.up;
							Vector3 focusFromPoint = lockPosition + orbitPosition;
							Vector3 focusDirection = (focalPoint - focusFromPoint).normalized;
							Quaternion focusRotation = Quaternion.LookRotation(focusDirection);
							if (isSmoothing)
							{
								smoothRotation = Quaternion.Lerp(smoothRotation, focusRotation, 4 * Time.deltaTime);
								mainCamera.transform.rotation = smoothRotation;
							}
							else
							{
								mainCamera.transform.rotation = focusRotation;
							}
						}
						else
						{
							if (isSmoothing)
							{
								smoothRotation = Quaternion.Lerp(smoothRotation, Quaternion.Euler(orbitPitch, orbitYaw, 0), 4 * Time.deltaTime);
								mainCamera.transform.rotation = smoothRotation;
							}
							else
							{
								mainCamera.transform.rotation = Quaternion.Euler(orbitPitch, orbitYaw, 0);
							}
						}
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Position");

				if (player.life.isDead)
				{
					Vector3 origin = player.first.position + Vector3.up;
					Vector3 direction = -mainCamera.transform.forward;
					float length = 4.0f;
					mainCamera.transform.position = sphereCastCamera(origin, direction, length, RayMasks.BLOCK_KILLCAM);
				}
				else
				{
					if (IsLocallyUsingFreecam)
					{
						if (isLocking || isFocusing)
						{
							mainCamera.transform.position = lockPosition + orbitPosition;
						}
						else if (IsControllingFreecam || isTracking)
						{
							if (isSmoothing)
							{
								smoothPosition = Vector3.Lerp(smoothPosition, orbitPosition, 4 * Time.deltaTime);
								mainCamera.transform.position = player.first.position + smoothPosition;
							}
							else
							{
								mainCamera.transform.position = player.first.position + orbitPosition;
							}
						}
					}
					else if ((player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING) && perspective == EPlayerPerspective.THIRD)
					{
						Vector3 origin = player.first.transform.position + (Vector3.up * eyes);
						Transform cameraFocusTransform = player.movement.getVehicle().transform.Find("Camera_Focus");
						if (cameraFocusTransform != null)
						{
							origin = cameraFocusTransform.position;
						}

						// Nelson 2024-09-20: In vehicle update this got mixed up to always use AnimatedForwardVelocity,
						// but previously that value was AnimatedVelocityInput for non-car vehicles. Players reported
						// this because it would bring the camera closer when changing directions e.g. in airplanes.
						float speedForCameraDistance;
						if (player.movement.getVehicle().asset.engine == EEngine.CAR)
						{
							speedForCameraDistance = player.movement.getVehicle().AnimatedForwardVelocity;
						}
						else
						{
							speedForCameraDistance = player.movement.getVehicle().AnimatedVelocityInput;
						}

						float dist = player.movement.getVehicle().asset.camFollowDistance + speedForCameraDistance * 0.1f;
						Vector3 direction = -mainCamera.transform.forward;

						mainCamera.transform.position = sphereCastCamera(origin, direction, dist, RayMasks.BLOCK_VEHICLECAM);
					}
					else if (player.stance.stance == EPlayerStance.DRIVING)
					{
						float verticalOffset = player.movement.getVehicle().asset.camDriverOffset + player.movement.getVehicle().asset.camPassengerOffset;
						if (yaw > 0)
						{
							mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, (Vector3.up * (heightLook + verticalOffset)) - (Vector3.left * yaw / 360f), 4 * Time.deltaTime);
						}
						else
						{
							mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, (Vector3.up * (heightLook + verticalOffset)) - (Vector3.left * yaw / 240f), 4 * Time.deltaTime);
						}
					}
					else
					{
						if (perspective == EPlayerPerspective.FIRST)
						{
							float verticalOffset;
							if (player.stance.stance == EPlayerStance.SITTING && player.movement.getVehicle() != null)
							{
								verticalOffset = player.movement.getVehicle().asset.camPassengerOffset;
							}
							else
							{
								verticalOffset = 0.0f;

								// Not sitting, not driving, and in first-person.
								// In this case we need to check for obstructions slightly above camera to prevent overlap.
								const float castRadius = 0.25f;
								Vector3 castOrigin = player.first.position + new Vector3(0, HEIGHT_LOOK_PRONE - castRadius, 0);
								Vector3 castDirection = Vector3.up;
								float castLength = PlayerMovement.HEIGHT_STAND - HEIGHT_LOOK_PRONE - castRadius;
								RaycastHit upwardHit;
								bool surfaceAboveHead = Physics.SphereCast(castOrigin, castRadius, castDirection, out upwardHit, castLength, RayMasks.BLOCK_PLAYERCAM_1P, QueryTriggerInteraction.Ignore);
								if (surfaceAboveHead)
								{
									float surfaceHeight = upwardHit.point.y - player.first.position.y; // Distance returned is not the same as Y - Y.
									float maxEyesHeight = surfaceHeight - castRadius; // Eyes cannot be within certain distance of the surface or camera will clip through.
									eyes = Mathf.Min(eyes, maxEyesHeight);
								}
							}

							mainCamera.transform.localPosition = new Vector3(0, eyes + verticalOffset, 0);
						}
						else
						{
							Vector3 direction;
							if (Provider.modeConfigData.Gameplay.Allow_Shoulder_Camera)
							{
								direction = (mainCamera.transform.forward * -1.5f) + (mainCamera.transform.up * 0.25f) + (mainCamera.transform.right * player.animator.shoulder * 1f);
							}
							else
							{
								direction = (mainCamera.transform.forward * -1.5f) + (mainCamera.transform.up * 0.5f) + (mainCamera.transform.right * player.animator.shoulder2 * 0.5f);
							}
							direction.Normalize();

							Vector3 origin = player.first.position + new Vector3(0.0f, thirdPersonEyeHeight, 0.0f);
							float length = 2.0f;
							mainCamera.transform.position = sphereCastCamera(origin, direction, length, RayMasks.BLOCK_PLAYERCAM);
						}
					}

					characterHeight = Mathf.Lerp(characterHeight, heightCamera, 4 * Time.deltaTime);
				}

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Local");

				if (player.movement.getVehicle() != null && player.movement.getVehicle().asset.engine == EEngine.PLANE && player.movement.getVehicle().AnimatedForwardVelocity > 16)
				{
					LevelLighting.UpdateForViewer(mainCamera.transform.position, Mathf.Lerp(0, 1.0f, (player.movement.getVehicle().AnimatedForwardVelocity - 16) / 8.0f), Time.deltaTime);
				}
				else if (player.movement.getVehicle() != null && (player.movement.getVehicle().asset.engine == EEngine.HELICOPTER || player.movement.getVehicle().asset.engine == EEngine.BLIMP) && player.movement.getVehicle().AnimatedForwardVelocity > 4)
				{
					LevelLighting.UpdateForViewer(mainCamera.transform.position, Mathf.Lerp(0, 1.0f, (player.movement.getVehicle().AnimatedForwardVelocity - 8) / 8.0f), Time.deltaTime);
				}
				else
				{
					LevelLighting.UpdateForViewer(mainCamera.transform.position, 0.0f, Time.deltaTime);
				}
				player.animator.viewmodelParentTransform.rotation = mainCamera.transform.rotation;

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Scope");

				if (isScopeActive && scopeCamera.targetTexture != null && scopeVision != ELightingVision.NONE)
				{
					ApplyScopeVisionToLighting();
					scopeCamera.Render();
					RestoreSavedLightingVision();
				}

				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("Turret");

				if (player.movement.getVehicle() != null && player.movement.getVehicle().passengers[player.movement.getSeat()].turret != null)
				{
					Passenger seat = player.movement.getVehicle().passengers[player.movement.getSeat()];
					if (seat.turretYaw != null)
					{
						seat.turretYaw.localRotation = seat.rotationYaw * Quaternion.Euler(0, yaw, 0);
					}
					if (seat.turretPitch != null)
					{
						seat.turretPitch.localRotation = seat.rotationPitch * Quaternion.Euler(pitch - 90, 0, 0);
					}

					if (perspective == EPlayerPerspective.FIRST)
					{
						if (player.movement.getVehicle().passengers[player.movement.getSeat()].turret.useAimCamera)
						{
							mainCamera.transform.position = seat.turretAim.position;
							mainCamera.transform.rotation = seat.turretAim.rotation;
						}
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();

				if (Framework.Foliage.FoliageSettings.drawFocus)
				{
					if (isMainCameraZoomFactorActive || (isScopeActive && scopeCamera.targetTexture != null) || isSingleRenderScopeZoomFactorActive)
					{
						Framework.Foliage.FoliageSystem.isFocused = true;

						RaycastHit focus;
						if (Physics.Raycast(MainCamera.instance.transform.position, MainCamera.instance.transform.forward, out focus, Framework.Foliage.FoliageSettings.focusDistance, RayMasks.FOLIAGE_FOCUS))
						{
							Framework.Foliage.FoliageSystem.focusPosition = focus.point;

							if (isScopeActive && scopeCamera.targetTexture != null)
							{
								Framework.Foliage.FoliageSystem.focusCamera = scopeCamera;
							}
							else
							{
								Framework.Foliage.FoliageSystem.focusCamera = MainCamera.instance;
							}
						}
					}
					else
					{
						Framework.Foliage.FoliageSystem.isFocused = false;
					}
				}
			}
			else if (!Provider.isServer)
			{
				if (player.stance.stance == EPlayerStance.DRIVING || player.stance.stance == EPlayerStance.SITTING)
				{
					transform.localRotation = Quaternion.identity;
				}
				else
				{
					_pitch = player.movement.snapshot.pitch;
					_yaw = player.movement.snapshot.yaw;

					transform.localRotation = Quaternion.Euler(0, yaw, 0);
				}

				if (player.movement.getVehicle() != null && player.movement.getVehicle().passengers[player.movement.getSeat()].turret != null)
				{
					Passenger seat = player.movement.getVehicle().passengers[player.movement.getSeat()];
					if (seat.turretYaw != null)
					{
						seat.turretYaw.localRotation = seat.rotationYaw * Quaternion.Euler(0, player.movement.snapshot.yaw, 0);
					}
					if (seat.turretPitch != null)
					{
						seat.turretPitch.localRotation = seat.rotationPitch * Quaternion.Euler(player.movement.snapshot.pitch - 90, 0, 0);
					}
				}
			}

			if (!Dedicator.IsDedicatedServer)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Aim");

				updateAim(Time.deltaTime);

				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		internal void InitializePlayer()
		{
			_aim = transform.Find("Aim").Find("Fire");

			updateLook();

			yawInputMultiplier = 1.0f;
			pitchInputMultiplier = 1.0f;

			if (channel.IsLocalPlayer)
			{
				if (Provider.cameraMode == ECameraMode.THIRD)
				{
					_perspective = EPlayerPerspective.THIRD;

					MainCamera.instance.transform.parent = player.transform;
				}
				else
				{
					_perspective = EPlayerPerspective.FIRST;
				}

				MainCamera.instance.fieldOfView = OptionsSettings.DesiredVerticalFieldOfView;

				targetExplosionLocalRotation.currentRotation = Quaternion.identity;
				targetExplosionLocalRotation.targetRotation = Quaternion.identity;

				characterHeight = 0;
				_characterYaw = 180;
				characterYaw = 0;

				if (player.character != null)
				{
					_characterCamera = player.character.Find("Camera").GetComponent<Camera>();
					_characterCamera.eventMask = 0;
				}

				_scopeCamera = MainCamera.instance.transform.Find("Scope").GetComponent<Camera>();
				scopeCamera.layerCullDistances = MainCamera.instance.layerCullDistances;
				scopeCamera.layerCullSpherical = MainCamera.instance.layerCullSpherical;
				scopeCamera.fieldOfView = 10.0f;
				scopeCamera.eventMask = 0;
				UnturnedPostProcess.instance.setScopeCamera(scopeCamera);

				LevelLighting.updateLighting();

				player.life.onVisionUpdated += onVisionUpdated;
				player.life.onLifeUpdated += onLifeUpdated;
				player.movement.onSeated += onSeated;
			}
		}

		private void OnDestroy()
		{
			if (scopeRenderTexture != null)
			{
				Destroy(scopeRenderTexture);
				scopeRenderTexture = null;
			}

			if (scopeMaterial != null)
			{
				Destroy(scopeMaterial);
				scopeMaterial = null;
			}
		}

		[System.Obsolete]
		public bool isCam => IsLocallyUsingFreecam;
	}
}
