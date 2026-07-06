////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableStructure : Useable
	{
		public ItemStructureAsset equippedStructureAsset => player.equipment.asset as ItemStructureAsset;

		[System.Obsolete]
		public void askStructure(CSteamID steamID, Vector3 newPoint, float newAngle)
		{

		}

		private static readonly ServerInstanceMethod<Vector3, float> SendBuildStructure = ServerInstanceMethod<Vector3, float>.Get(typeof(UseableStructure), nameof(ReceiveBuildStructure));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
		public void ReceiveBuildStructure(in ServerInvocationContext context, [NetPakVector3(fracBitCount: StructureManager.POSITION_FRAC_BIT_COUNT)] Vector3 newPoint, float newAngle)
		{
			if (hasServerReceivedBuildRequest)
			{
				context.LogWarning("already requested");
				return;
			}

			hasServerReceivedBuildRequest = true;

			if ((newPoint - player.look.aim.position).sqrMagnitude < HousingConnections.MAX_PLACEMENT_SQR_DISTANCE)
			{
				serverPlacementPosition = newPoint;
				serverPlacementYaw = newAngle;

				if (!UseableHousingUtils.IsPendingPositionValid(player, serverPlacementPosition))
				{
					isServerBuildRequestInitiallyApproved = false;
					context.LogWarning("position invalid");
				}
				else
				{
					string obstructionHint = null;
					EHousingPlacementResult result = UseableHousingUtils.ValidatePendingPlacement(equippedStructureAsset, ref serverPlacementPosition, serverPlacementYaw, ref obstructionHint);
					if (result == EHousingPlacementResult.Success)
					{
						isServerBuildRequestInitiallyApproved = true;
					}
					else
					{
						isServerBuildRequestInitiallyApproved = false;
						context.LogWarning($"housing link result invalid: {result} \"{obstructionHint}\"");
					}
				}
			}
			else
			{
				context.LogWarning("out of range");
			}
		}

		[System.Obsolete]
		public void askConstruct(CSteamID steamID)
		{
			ReceivePlayConstruct();
		}

		private static readonly ClientInstanceMethod SendPlayConstruct = ClientInstanceMethod.Get(typeof(UseableStructure), nameof(ReceivePlayConstruct));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askConstruct))]
		public void ReceivePlayConstruct()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				PlayUseAnimation();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (player.movement.getVehicle() != null)
			{
				return false;
			}

			if (Dedicator.IsDedicatedServer ? isServerBuildRequestInitiallyApproved : UpdatePendingPlacement())
			{
				if (channel.IsLocalPlayer)
				{
					SendBuildStructure.Invoke(GetNetId(), ENetReliability.Reliable, pendingPlacementPosition, pendingPlacementYaw + customRotationOffset);
				}

				player.equipment.isBusy = true;

				PlayUseAnimation();

				if (Provider.isServer)
				{
					SendPlayConstruct.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
				}
			}
			else
			{
				if (Dedicator.IsDedicatedServer && hasServerReceivedBuildRequest)
				{
					player.equipment.dequip();
				}
			}

			return true;
		}

		public override bool startSecondary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (channel.IsLocalPlayer)
			{
				if (equippedStructureAsset.construct == EConstruct.FLOOR_POLY || equippedStructureAsset.construct == EConstruct.ROOF_POLY)
				{
					// Disable rotating triangles because their pivot is off-center.
					return false;
				}

				float delta;
				if (equippedStructureAsset.construct == EConstruct.FLOOR || equippedStructureAsset.construct == EConstruct.ROOF)
				{
					delta = 90.0f;
				}
				else if (equippedStructureAsset.construct == EConstruct.RAMPART || equippedStructureAsset.construct == EConstruct.WALL)
				{
					delta = 180.0f;
				}
				else
				{
					delta = 30.0f;
				}
				if (InputEx.GetKey(KeyCode.LeftShift))
				{
					delta *= -1.0f;
				}
				customRotationOffset += delta;
			}

			return true;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			useAnimationDuration = player.animator.GetAnimationLength("Use");

			if (channel.IsLocalPlayer)
			{
				isPlacementPreviewValid = false;
				placementPreviewTransform = UseableHousingUtils.InstantiatePlacementPreview(equippedStructureAsset);
			}
		}

		public override void dequip()
		{
			if (channel.IsLocalPlayer)
			{
				if (placementPreviewTransform != null)
				{
					Destroy(placementPreviewTransform.gameObject);
				}
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUseAnimationPlaying && HasFinishedUseAnimation)
			{
				player.equipment.isBusy = false;

				if (Provider.isServer)
				{
					if (!UseableHousingUtils.IsPendingPositionValid(player, serverPlacementPosition)) // Double-check everything is still OK in-case another buildable was placed while we were placing this one.
					{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
						CommandWindow.LogWarning("Placement position no longer valid");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
						player.equipment.dequip();
					}
					else
					{
						string obstructionHint = string.Empty;
						EHousingPlacementResult housingResult = UseableHousingUtils.ValidatePendingPlacement(equippedStructureAsset, ref serverPlacementPosition, serverPlacementYaw, ref obstructionHint);
						if (housingResult != EHousingPlacementResult.Success)
						{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
							CommandWindow.LogWarning($"Housing link result no longer valid: {housingResult} \"{obstructionHint}\"");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
							player.equipment.dequip();
						}
						else
						{
							ItemStructureAsset asset = equippedStructureAsset;

							bool success = false;
							if (asset != null)
							{
								player.sendStat(EPlayerStat.FOUND_BUILDABLES);

								success = StructureManager.dropStructure(new Structure(asset, asset.health), serverPlacementPosition, 0, serverPlacementYaw, 0, channel.owner.playerID.steamID.m_SteamID, player.quests.groupID.m_SteamID);
							}

							if (success)
								player.equipment.use();
							else
								player.equipment.dequip();
						}
					}
				}
			}
		}

		public override void tick()
		{
			if (isWaitingForSoundTrigger && HasReachedSoundTrigger)
			{
				isWaitingForSoundTrigger = false;

				if (!Dedicator.IsDedicatedServer)
				{
					player.playSound(equippedStructureAsset.use);
				}

				if (Provider.isServer)
				{
					AlertTool.alert(transform.position, 8);
				}
			}

			if (channel.IsLocalPlayer)
			{
				if (placementPreviewTransform == null)
				{
					return;
				}

				if (!isUseAnimationPlaying)
				{
					bool isPlacementValid = UpdatePendingPlacement();
					if (isPlacementPreviewValid != isPlacementValid)
					{
						isPlacementPreviewValid = isPlacementValid;
						HighlighterTool.help(placementPreviewTransform, isPlacementPreviewValid);
					}
				}

				float scrollWheelInput = Glazier.Get().ShouldGameProcessInput ? Input.GetAxis("mouse_z") : 0.0f;
				foundationPositionOffset = Mathf.Clamp(foundationPositionOffset + (scrollWheelInput * UseableHousingUtils.FOUNDATION_MOUSE_SCROLL_MULTIPLIER), UseableHousingUtils.FOUNDATION_MIN_OFFSET, UseableHousingUtils.FOUNDATION_MAX_OFFSET);
				animatedRotationOffset = Mathf.Lerp(animatedRotationOffset, customRotationOffset, 8 * Time.deltaTime);
				placementPreviewTransform.position = pendingPlacementPosition;
				placementPreviewTransform.rotation = Quaternion.Euler(-90, pendingPlacementYaw + animatedRotationOffset, 0);
			}
		}

		private void PlayUseAnimation()
		{
			useAnimationStartTime = Time.timeAsDouble;
			isUseAnimationPlaying = true;
			isWaitingForSoundTrigger = true;

			player.animator.play("Use", false);
		}

		private bool UpdatePendingPlacement()
		{
			if (!UseableHousingUtils.FindPlacement(equippedStructureAsset, player, customRotationOffset, foundationPositionOffset, out pendingPlacementPosition, out pendingPlacementYaw))
			{
				return false;
			}

			if (!UseableHousingUtils.IsPendingPositionValid(player, pendingPlacementPosition))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Stripped-down version of structure prefab for previewing where the structure will be spawned.
		/// </summary>
		private Transform placementPreviewTransform;

		/// <summary>
		/// Whether preview object is currently highlighted positively.
		/// </summary>
		private bool isPlacementPreviewValid;

		/// <summary>
		/// Time when "Use" animation clip started playing in seconds.
		/// </summary>
		private double useAnimationStartTime;

		/// <summary>
		/// Length of "Use" animation clip in seconds.
		/// </summary>
		private float useAnimationDuration;

		/// <summary>
		/// True when animation starts playing, false after placement sound is played.
		/// </summary>
		private bool isWaitingForSoundTrigger;

		/// <summary>
		/// Whether the "Use" animation clip started playing.
		/// </summary>
		private bool isUseAnimationPlaying;

		/// <summary>
		/// If running as server, whether ReceiveBuildStructure has been called yet.
		/// </summary>
		private bool hasServerReceivedBuildRequest;

		/// <summary>
		/// Whether basic range and claim checks passed.
		/// </summary>
		private bool isServerBuildRequestInitiallyApproved;

		/// <summary>
		/// Position the item should be spawned at.
		/// </summary>
		private Vector3 pendingPlacementPosition;

		/// <summary>
		/// Rotation the item should be spawned at.
		/// </summary>
		private float pendingPlacementYaw;

		/// <summary>
		/// Interpolated toward customRotationOffset.
		/// </summary>
		private float animatedRotationOffset;

		/// <summary>
		/// Allows players to flip walls.
		/// </summary>
		private float customRotationOffset;

		/// <summary>
		/// Vertical offset using scroll wheel.
		/// </summary>
		private float foundationPositionOffset;

		// Server version of pendingPlacement values to simplify singleplayer.
		private Vector3 serverPlacementPosition;
		private float serverPlacementYaw;

		/// <summary>
		/// Whether enough time has passed for "Use" animation to finish.
		/// </summary>
		private bool HasFinishedUseAnimation => Time.timeAsDouble - useAnimationStartTime > useAnimationDuration;

		/// <summary>
		/// Whether animation has reached the time when placement sound should play.
		/// </summary>
		private bool HasReachedSoundTrigger => Time.timeAsDouble - useAnimationStartTime > useAnimationDuration * 0.8f;
	}
}
