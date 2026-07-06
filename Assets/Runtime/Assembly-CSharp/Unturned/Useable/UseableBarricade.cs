////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
//#define LOG_BARRICADE_PLACEMENT
//#define WITH_BARRICADE_PLACEMENT_GIZMOS
//#define WITH_OVERLAP_GIZMOS
//#define LOG_BARRICADE_PLACEMENT_CANCEL
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableBarricade : Useable
	{
		private static List<Collider> colliders = new List<Collider>();
		private static Collider[] checkColliders = new Collider[1];

		private Transform parent;
		private Transform help;
		private Transform guide;
		private Transform arrow;
		private InteractableVehicle parentVehicle;

		private bool boundsUse;
		private bool boundsDoubleDoor;
		private Vector3 boundsCenter;
		private Vector3 boundsExtents;
		private Vector3 boundsOverlap;
		private Quaternion boundsRotation;

		private float startedUse;
		private float useTime;

		private bool inputWantsToRotate;
		private bool isBuilding;
		private bool isUsing;
		private bool isValid;
		private bool wasAsked;
		private int pendingBuildHandle = -1;

		private RaycastHit hit;
		private Vector3 point;
		private float angle_x;
		private float angle_y;
		private float angle_z;
		//private float offset_y;
		private float rotate_x;
		private float rotate_y;
		private float rotate_z;
		private float input_x;
		private float input_y;
		private float input_z;

		private bool isPower;
		private Vector3 powerPoint;
		private List<InteractableClaim> claimsInRadius;
		private List<InteractableGenerator> generatorsInRadius;
		private List<InteractableSafezone> safezonesInRadius;
		private List<InteractableOxygenator> oxygenatorsInRadius;

		private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

		private bool isBuildable => Time.realtimeSinceStartup - startedUse > useTime * 0.8f;

		public ItemBarricadeAsset equippedBarricadeAsset => player.equipment.asset as ItemBarricadeAsset;

		private bool allowRotationInputOnAllAxes => equippedBarricadeAsset.build == EBuild.FREEFORM || equippedBarricadeAsset.build == EBuild.SENTRY_FREEFORM;

		private bool serverAllowAnyRotation => allowRotationInputOnAllAxes || equippedBarricadeAsset.build == EBuild.CHARGE || equippedBarricadeAsset.build == EBuild.CLOCK || equippedBarricadeAsset.build == EBuild.NOTE;

		/// <summary>
		/// Does the item being placed count as a "trap" for the purposes of vehicle placement restrictions?
		/// </summary>
		private bool useTrapRestrictions => equippedBarricadeAsset.type == EItemType.TRAP;

		private bool allowedToPlaceOnVehicle => useTrapRestrictions ? Provider.modeConfigData.Barricades.Allow_Trap_Placement_On_Vehicle
											: Provider.modeConfigData.Barricades.Allow_Item_Placement_On_Vehicle;

		private float maxDistanceFromHull => useTrapRestrictions ? Provider.modeConfigData.Barricades.Max_Trap_Distance_From_Hull
											: Provider.modeConfigData.Barricades.Max_Item_Distance_From_Hull;

		private bool useMaxDistanceFromHull => maxDistanceFromHull > -0.5f;

		private float sqrMaxDistanceFromHull => MathfEx.Square(maxDistanceFromHull);

		[System.Obsolete]
		public void askBarricadeVehicle(CSteamID steamID, Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z, ushort plant)
		{
			// No longer backwards compatible because NetId replaced plant
		}

		private static readonly ServerInstanceMethod<Vector3, float, float, float, NetId> SendBarricadeVehicle = ServerInstanceMethod<Vector3, float, float, float, NetId>.Get(typeof(UseableBarricade), nameof(ReceiveBarricadeVehicle));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
		public void ReceiveBarricadeVehicle(in ServerInvocationContext context, [NetPakVector3(fracBitCount: BarricadeManager.POSITION_FRAC_BIT_COUNT)] Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z, NetId regionNetId)
		{
			if (wasAsked)
			{
				context.LogWarning("already asked to build");
				return;
			}
			wasAsked = true;

			if (!allowedToPlaceOnVehicle)
			{
				context.LogWarning("not allowed to place on vehicle");
				return; // Client should have been warned.
			}

			VehicleBarricadeRegion region = NetIdRegistry.Get<VehicleBarricadeRegion>(regionNetId);
			if (region == null)
			{
				context.LogWarning("null region");
				return;
			}

			InteractableVehicle testVehicle = region.vehicle;
			if (testVehicle == null)
			{
				context.LogWarning("null vehicle");
				return;
			}

			if (useMaxDistanceFromHull)
			{
				Vector3 worldspaceTestPoint = region.parent.TransformPoint(newPoint);
				if (testVehicle.getSqrDistanceFromHull(worldspaceTestPoint) > sqrMaxDistanceFromHull)
				{
					context.LogWarning("too far from hull");
					return; // Too far away, but client should have been warned.
				}
			}

			parent = region.parent;
			parentVehicle = testVehicle;
			point = newPoint;

			if (serverAllowAnyRotation)
			{
				angle_x = newAngle_X;
				angle_z = newAngle_Z;
			}
			else
			{
				angle_x = 0;
				angle_z = 0;
			}
			angle_y = newAngle_Y;

			rotate_x = 0;
			rotate_y = 0;
			rotate_z = 0;
			isValid = checkClaims();
			if (!isValid)
			{
				context.LogWarning("blocked by claims");
			}
		}

		[System.Obsolete]
		public void askBarricadeNone(CSteamID steamID, Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z)
		{
		}

		private static readonly ServerInstanceMethod<Vector3, float, float, float> SendBarricadeNone = ServerInstanceMethod<Vector3, float, float, float>.Get(typeof(UseableBarricade), nameof(ReceiveBarricadeNone));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = nameof(askBarricadeNone))]
		public void ReceiveBarricadeNone(in ServerInvocationContext context, [NetPakVector3(fracBitCount: BarricadeManager.POSITION_FRAC_BIT_COUNT)] Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z)
		{
			if (wasAsked)
				return;
			wasAsked = true;

			if ((newPoint - player.look.aim.position).sqrMagnitude < 256)
			{
				parent = null;
				parentVehicle = null;
				point = newPoint;

				if (serverAllowAnyRotation)
				{
					angle_x = newAngle_X;
					angle_z = newAngle_Z;
				}
				else
				{
					angle_x = 0;
					angle_z = 0;
				}
				angle_y = newAngle_Y;

				rotate_x = 0;
				rotate_y = 0;
				rotate_z = 0;
				isValid = checkClaims();
				if (isValid)
				{
					pendingBuildHandle = BuildRequestManager.registerPendingBuild(point);
				}
				else
				{
					context.LogWarning("blocked by claims");
				}
			}
			else
			{
				context.LogWarning("too far away");
			}
		}

		private bool check()
		{
			if (!checkSpace())
			{
				return false;
			}

			// Updating parent here is important because checkSpace may have changed the target.
			if (equippedBarricadeAsset.build == EBuild.VEHICLE)
			{
				parentVehicle = null;
				parent = null;
			}
			else
			{
				parentVehicle = DamageTool.getVehicle(hit.transform);
				// parent is not necessarily the parentVehicle.transform if placing on a train.
				parent = parentVehicle != null ? hit.transform.root : null;
			}

			if (!checkClaims())
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Should placement ghost material change be done recursively?
		/// e.g. Sentry has a deep hierarchy of meshes.
		/// </summary>
		private bool isHighlightRecursive => equippedBarricadeAsset.build == EBuild.SENTRY || equippedBarricadeAsset.build == EBuild.SENTRY_FREEFORM;

		private Vector3 getPointInWorldSpace()
		{
			// Owner keeps point in world space until asking to build.
			if (parent == null || (channel.IsLocalPlayer && !wasAsked))
			{
				return point;
			}
			else
			{
				// Dedicated server keeps point relative to parent.
				return parent.TransformPoint(point);
			}
		}

		private bool checkClaims()
		{
			if (player.movement.isSafe && !player.movement.isSafeInfo.CurrentlyAllowsBuilding)
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.SAFEZONE);
				}

#if LOG_BARRICADE_PLACEMENT_CANCEL
				UnturnedLog.warn("Placement canceled by no-build zone");
#endif

				return false;
			}

			Vector3 realPoint = getPointInWorldSpace();

			if (channel.IsLocalPlayer && parentVehicle != null) // Server handles these checks in askBarricadeVehicle.
			{
				if (!allowedToPlaceOnVehicle)
				{
					PlayerUI.hint(null, EPlayerMessage.CANNOT_BUILD_ON_VEHICLE);
					return false;
				}

				if (useMaxDistanceFromHull)
				{
					if (parentVehicle.getSqrDistanceFromHull(realPoint) > sqrMaxDistanceFromHull)
					{
						PlayerUI.hint(null, EPlayerMessage.TOO_FAR_FROM_HULL);
						return false;
					}
				}
			}

			// Apparently some servers use the beacon to check whether a claimed point is within town borders,
			// so this message takes priority over the claimed message.
			if (equippedBarricadeAsset.build == EBuild.BEACON)
			{
				if (!LevelNavigation.checkSafeFakeNav(realPoint) || parent != null)
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.NAV);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled because outside navmesh or on vehicle");
#endif

					return false;
				}

				byte bound;
				if (LevelNavigation.tryGetBounds(realPoint, out bound))
				{
					ZombieDifficultyAsset difficulty = ZombieManager.getDifficultyInBound(bound);
					bool allowBeacon = difficulty == null || difficulty.Allow_Horde_Beacon;
					if (!allowBeacon)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.NOT_ALLOWED_HERE);
						}

#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn("Placement canceled because navmesh prevents beacons");
#endif

						return false;
					}
				}
			}

			if (!equippedBarricadeAsset.bypassClaim)
			{
				if (parent != null) // Parent is a vehicle
				{
					if (!ClaimManager.canBuildOnVehicle(parent, channel.owner.playerID.steamID, player.quests.groupID))
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.CLAIM);
						}

#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn("Placement canceled by vehicle claim manager");
#endif

						return false;
					}
				}

				if (!ClaimManager.checkCanBuild(realPoint, channel.owner.playerID.steamID, player.quests.groupID, equippedBarricadeAsset.build == EBuild.CLAIM))
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.CLAIM);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled by claim manager");
#endif

					return false;
				}
			}

			if (!IsPlacementInsideClipVolumesAllowed)
			{
				if (SDG.Framework.Devkit.PlayerClipVolumeManager.Get().IsPositionInsideAnyVolume(realPoint))
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.BOUNDS);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled by clip volume");
#endif

					return false;
				}
			}

			if (equippedBarricadeAsset.build == EBuild.BED)
			{
				if (Level.info == null || Level.info.type == ELevelType.ARENA)
				{
#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled because beds aren't allowed in arena mode");
#endif

					return false;
				}

				// Rudimentary check to prevent spawning inside kill volume because overlap with
				// trigger after respawn is not detected unfortunately. (public issue #1513)
				if (SDG.Framework.Devkit.KillVolumeManager.Get().IsPositionInsideAnyVolume(realPoint))
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.BOUNDS);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled because bed would spawn player inside kill volume");
#endif

					return false;
				}
			}

			if (!Provider.modeConfigData.Gameplay.Bypass_Buildable_Mobility)
			{
				if (parent != null && parentVehicle != null && parentVehicle.asset != null)
				{
					bool allowPlacement;
					switch (parentVehicle.asset.BuildablePlacementRule)
					{
						default:
						case EVehicleBuildablePlacementRule.None:
							allowPlacement = equippedBarricadeAsset.allowPlacementOnVehicle;
							break;

						case EVehicleBuildablePlacementRule.AlwaysAllow:
							allowPlacement = true;
							break;

						case EVehicleBuildablePlacementRule.Block:
							allowPlacement = false;
							break;
					}

					if (!allowPlacement)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.MOBILE);
						}

#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn("Placement canceled because of vehicle mobility rule");
#endif

						return false;
					}
				}
			}

			// Nelson 2023-11-22: Admins are allowed to bypass this restriction. (public issue #4217)
			if (equippedBarricadeAsset.build == EBuild.FREEFORM && !channel.owner.isAdmin)
			{
				bool isOnVehicle = parent != null && parentVehicle != null;
				bool isAllowedByConfig = isOnVehicle ? Provider.modeConfigData.Gameplay.Allow_Freeform_Buildables_On_Vehicles
						: Provider.modeConfigData.Gameplay.Allow_Freeform_Buildables;
				if (!isAllowedByConfig)
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.FREEFORM_BUILDABLE_NOT_ALLOWED);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled by freeform restrictions");
#endif

					return false;
				}
			}

			if (parent != null && parentVehicle != null && parentVehicle.anySeatsOccupied)
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.BUILD_ON_OCCUPIED_VEHICLE);
				}

#if LOG_BARRICADE_PLACEMENT_CANCEL
				UnturnedLog.warn("Placement canceled because vehicle is not empty");
#endif

				return false;
			}

			if (player.movement.getVehicle() != null)
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.CANNOT_BUILD_WHILE_SEATED);
				}

#if LOG_BARRICADE_PLACEMENT_CANCEL
				UnturnedLog.warn("Placement canceled because player is in vehicle");
#endif

				return false;
			}

			if (Level.info == null || Level.info.type != ELevelType.ARENA)
			{
				if (!LevelPlayers.checkCanBuild(realPoint))
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.SPAWN);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled because it's too near a spawnpoint");
#endif

					return false;
				}
			}

			if (!BuildRequestManager.canBuildAt(realPoint, pendingBuildHandle))
			{
#if LOG_BARRICADE_PLACEMENT_CANCEL
				UnturnedLog.warn("Placement canceled by build request manager");
#endif

				return false;
			}

			if (SDG.Framework.Water.WaterUtility.isPointUnderwater(realPoint) && (equippedBarricadeAsset.build == EBuild.CAMPFIRE || equippedBarricadeAsset.build == EBuild.TORCH))
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.UNDERWATER);
				}

#if LOG_BARRICADE_PLACEMENT_CANCEL
				UnturnedLog.warn("Placement canceled because position is underwater");
#endif

				return false;
			}

			if (Dedicator.IsDedicatedServer)
			{
				boundsRotation = BarricadeManager.getRotation((ItemBarricadeAsset) player.equipment.asset, angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z);
				if (parent != null)
				{
					boundsRotation = parent.TransformRotation(boundsRotation);
				}
			}
			else
			{
				boundsRotation = help.rotation;
			}

			int boundsOverlapMask;
			if (parent != null)
				boundsOverlapMask = RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP;
			else
				boundsOverlapMask = RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP_NOT_ON_VEHICLE;

			Vector3 overlapBoundsCenter = realPoint + (boundsRotation * boundsCenter);

#if WITH_OVERLAP_GIZMOS
			RuntimeGizmos.Get().Box(overlapBoundsCenter, boundsRotation, boundsOverlap * 2, Color.red);
#endif

			if (Physics.OverlapBoxNonAlloc(overlapBoundsCenter, boundsOverlap, checkColliders, boundsRotation, boundsOverlapMask, QueryTriggerInteraction.Collide) > 0)
			{
				if (channel.IsLocalPlayer)
				{
#if LOG_BARRICADE_PLACEMENT
					UnturnedLog.warn("Bounding box overlapping: {0}", checkColliders[0].GetSceneHierarchyPath());
#endif
					PlayerUI.hint(null, EPlayerMessage.BLOCKED);
				}

#if LOG_BARRICADE_PLACEMENT_CANCEL
				UnturnedLog.warn($"Placement canceled because of overlaps: {CheckCollidersToString()}");
#endif

				return false;
			}

			if (equippedBarricadeAsset.build == EBuild.BED)
			{
				// Trace from the player's torso to where they placed the bed in case their eyes were slightly inside a surface
				Vector3 bedCenter = realPoint + new Vector3(0, 0.1f, 0);
				Vector3 torsoCenter = player.transform.position + (Vector3.up * (player.look.heightLook * 0.5f));
				RaycastHit block;
				bool hitAnything = Physics.Linecast(torsoCenter, bedCenter, out block, RayMasks.BLOCK_BED_LOS, QueryTriggerInteraction.Ignore);
#if WITH_BARRICADE_PLACEMENT_GIZMOS
				GizmosUtil.Get().Linecast(torsoCenter, bedCenter, block, Color.green, Color.red, lifespan: 3.0f);
#endif // WITH_BARRICADE_PLACEMENT_GIZMOS
				if (hitAnything)
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.BLOCKED);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled because bed spawn position overlaps");
#endif

					return false;
				}
			}

			if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER)
			{
				Vector3 extents = boundsExtents;
				extents.x -= 0.25f;
				extents.y -= 0.5f;
				extents.z += 0.6f;

				if (Physics.OverlapBoxNonAlloc(realPoint + (boundsRotation * boundsCenter), extents, checkColliders, boundsRotation, RayMasks.BLOCK_DOOR_OPENING) > 0)
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.BLOCKED);
					}

#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn($"Placement canceled by door overlap: {CheckCollidersToString()}");
#endif

					return false;
				}

				bool checkLeft = false;
				bool checkRight = false;

				if (equippedBarricadeAsset.build == EBuild.DOOR)
				{
					checkLeft = true;
					checkRight = boundsDoubleDoor;
				}
				else if (equippedBarricadeAsset.build == EBuild.GATE)
				{
					checkLeft = boundsDoubleDoor;
					checkRight = boundsDoubleDoor;
				}
				else if (equippedBarricadeAsset.build == EBuild.SHUTTER)
				{
					checkLeft = true;
					checkRight = true;
				}

				const float CHECK_OVERLAP_RADIUS = 0.75f;

				if (checkLeft)
				{
					Vector3 leftCenter = realPoint + (boundsRotation * new Vector3(-boundsExtents.x, 0, boundsExtents.x));

#if WITH_OVERLAP_GIZMOS
					RuntimeGizmos.Get().Sphere(leftCenter, CHECK_OVERLAP_RADIUS, Color.red);
#endif

					// Check the spots where the door opens into
					if (Physics.OverlapSphereNonAlloc(leftCenter, CHECK_OVERLAP_RADIUS, checkColliders, RayMasks.BLOCK_DOOR_OPENING) > 0)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						}

#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn($"Placement canceled by door left overlap at {leftCenter}: {CheckCollidersToString()}");
#endif

						return false;
					}
				}

				if (checkRight)
				{
					Vector3 rightCenter = realPoint + (boundsRotation * new Vector3(boundsExtents.x, 0, boundsExtents.x));

#if WITH_OVERLAP_GIZMOS
					RuntimeGizmos.Get().Sphere(rightCenter, CHECK_OVERLAP_RADIUS, Color.red);
#endif

					// Check the spot where the door opens into
					if (Physics.OverlapSphereNonAlloc(rightCenter, CHECK_OVERLAP_RADIUS, checkColliders, RayMasks.BLOCK_DOOR_OPENING) > 0)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						}

#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn($"Placement canceled by door right overlap at {rightCenter}: {CheckCollidersToString()}");
#endif

						return false;
					}
				}
			}

			return true;
		}

		private bool checkSpace()
		{
			angle_y = player.look.yaw;

			if (equippedBarricadeAsset.build == EBuild.FORTIFICATION || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.GLASS)
			{
				Physics.Raycast(player.look.aim.position, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);

				if (hit.collider != null)
				{
					Transform colliderTransform = hit.collider.transform;
					if (colliderTransform.CompareTag("Logic") && colliderTransform.name == "Slot")
					{
						point = hit.point - (hit.normal * equippedBarricadeAsset.offset);

						if (Mathf.Abs(Vector3.Dot(colliderTransform.right, Vector3.up)) > 0.5f)
						{
							angle_y = Quaternion.LookRotation(colliderTransform.forward).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 
							if (Vector3.Dot(MainCamera.instance.transform.forward, colliderTransform.forward) < 0.0f)
							{
								angle_y += 180.0f;
							}
						}
						else
						{
							angle_y = Quaternion.LookRotation(colliderTransform.up).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 
							if (Vector3.Dot(MainCamera.instance.transform.forward, colliderTransform.up) > 0.0f)
							{
								angle_y += 180.0f;
							}
						}

						if (equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.GLASS)
						{
							if (colliderTransform.parent.CompareTag("Barricade") || colliderTransform.parent.CompareTag("Structure"))
							{
								point = colliderTransform.position - (hit.normal * equippedBarricadeAsset.offset);
							}
						}

						if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BOUNDS);
							}

							return false;
						}

						if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_WINDOW) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}
					}
					else
					{
						point = Vector3.zero;

						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.WINDOW);
						}

						return false;
					}
				}
				else
				{
					point = Vector3.zero;

					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.WINDOW);
					}

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.BARRICADE || equippedBarricadeAsset.build == EBuild.TANK || equippedBarricadeAsset.build == EBuild.LIBRARY || equippedBarricadeAsset.build == EBuild.BARREL_RAIN || equippedBarricadeAsset.build == EBuild.VEHICLE || equippedBarricadeAsset.build == EBuild.BED || equippedBarricadeAsset.build == EBuild.STORAGE || equippedBarricadeAsset.build == EBuild.MANNEQUIN || equippedBarricadeAsset.build == EBuild.SENTRY || equippedBarricadeAsset.build == EBuild.GENERATOR || equippedBarricadeAsset.build == EBuild.SPOT || equippedBarricadeAsset.build == EBuild.CAMPFIRE || equippedBarricadeAsset.build == EBuild.OVEN || equippedBarricadeAsset.build == EBuild.CLAIM || equippedBarricadeAsset.build == EBuild.SPIKE || equippedBarricadeAsset.build == EBuild.SAFEZONE || equippedBarricadeAsset.build == EBuild.OXYGENATOR || equippedBarricadeAsset.build == EBuild.BEACON || equippedBarricadeAsset.build == EBuild.SIGN || equippedBarricadeAsset.build == EBuild.STEREO)
			{
				if (equippedBarricadeAsset.build == EBuild.VEHICLE)
				{
					// Vehicles needs to raycast upwards, but the spherecast (other branch) can put the point slightly through walls.
					Physics.Raycast(player.look.aim.position, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
				}
				else
				{
					Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
				}

				if (hit.transform != null)
				{
					if (hit.normal.y < 0.01f)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						}

						return false;
					}

					if (hit.normal.y > 0.75)
					{
						point = hit.point + (hit.normal * equippedBarricadeAsset.offset);
					}
					else
					{
						point = hit.point + (Vector3.up * equippedBarricadeAsset.offset);
					}

					// Vehicle has a large offset, so it could get placed upward into a ceiling. This test could probably
					// be re-used for other items, but in the meantime we just want to prevent placing makeshift vehicle
					// into the roof as a way to get out of the map.
					if (equippedBarricadeAsset.build == EBuild.VEHICLE)
					{
						RaycastHit hitInfo;
						bool hitAnything = Physics.Linecast(hit.point, point, out hitInfo, RayMasks.BLOCK_BARRICADE);
#if WITH_BARRICADE_PLACEMENT_GIZMOS
						GizmosUtil.Get().Linecast(hit.point, point, hitInfo, Color.green, Color.red, 0.5f);
#endif // WITH_BARRICADE_PLACEMENT_GIZMOS
						if (hitAnything)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}
					}

					if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BOUNDS);
						}

						return false;
					}

					if (equippedBarricadeAsset.build == EBuild.BED)
					{
						if (Physics.OverlapSphereNonAlloc(point + Vector3.up, 0.95f + equippedBarricadeAsset.offset, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}

						//RaycastHit block;
						//if(Physics.Raycast(point, hit.normal, out block, 1f, RayMasks.BLOCK_BARRICADE, QueryTriggerInteraction.Ignore))
						//{
						//	if(block.collider != hit.collider)
						//	{
						//		if(channel.isOwner)
						//		{
						//			PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						//		}

						//		return false;
						//	}
						//}
					}
					else
					{
						if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}
					}
				}
				else
				{
					point = Vector3.zero;

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.WIRE)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);

				if (hit.transform != null)
				{
					point = hit.point + (hit.normal * equippedBarricadeAsset.offset);

					if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BOUNDS);
						}

						return false;
					}

					if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						}

						return false;
					}
				}
				else
				{
					point = Vector3.zero;

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.FARM || equippedBarricadeAsset.build == EBuild.OIL)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);

				if (hit.transform != null)
				{
					if (hit.normal.y > 0.75)
					{
						point = hit.point + (hit.normal * equippedBarricadeAsset.offset);
					}
					else
					{
						point = hit.point + (Vector3.up * equippedBarricadeAsset.offset);
					}

					string hitMaterialName = PhysicsTool.GetMaterialName(hit);
					if (hit.transform.CompareTag("Ground"))
					{
						if (equippedBarricadeAsset.build == EBuild.FARM)
						{
							if (!(equippedBarricadeAsset as ItemFarmAsset).ignoreSoilRestrictions)
							{
								if (!PhysicMaterialCustomData.IsArable(hitMaterialName))
								{
									if (channel.IsLocalPlayer)
									{
										PlayerUI.hint(null, EPlayerMessage.SOIL);
									}

									return false;
								}
							}
						}
						else
						{
							if (!PhysicMaterialCustomData.HasOil(hitMaterialName))
							{
								if (channel.IsLocalPlayer)
								{
									PlayerUI.hint(null, EPlayerMessage.OIL);
								}

								return false;
							}
						}
					}
					else
					{
						if (equippedBarricadeAsset.build == EBuild.FARM)
						{
							if (!(equippedBarricadeAsset as ItemFarmAsset).ignoreSoilRestrictions)
							{
								if (!PhysicMaterialCustomData.IsArable(hitMaterialName))
								{
									if (channel.IsLocalPlayer)
									{
										PlayerUI.hint(null, EPlayerMessage.SOIL);
									}

									return false;
								}
							}
						}
						else
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.OIL);
							}

							return false;
						}
					}

					if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BOUNDS);
						}

						return false;
					}

					if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						}

						return false;
					}
				}
				else
				{
					point = Vector3.zero;

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.DOOR)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);

				if (hit.collider != null)
				{
					Transform colliderTransform = hit.collider.transform;
					if (colliderTransform.name == "Door")
					{
						point = colliderTransform.position;

						// Nelson 2024-09-23: It seems some modders did actually have a mix of door slot rotations? (public issue #4661)
						if (Mathf.Abs(Vector3.Dot(colliderTransform.up, Vector3.up)) > 0.5f)
						{
							angle_y = Quaternion.LookRotation(colliderTransform.forward).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 
							if (Vector3.Dot(MainCamera.instance.transform.forward, colliderTransform.forward) < 0.0f)
							{
								angle_y += 180.0f;
							}
						}
						else
						{
							angle_y = Quaternion.LookRotation(colliderTransform.up).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 
							if (Vector3.Dot(MainCamera.instance.transform.forward, colliderTransform.up) > 0.0f)
							{
								angle_y += 180.0f;
							}
						}

						if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BOUNDS);
							}

							return false;
						}

						if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_FRAME) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}
					}
					else
					{
						point = Vector3.zero;

						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.DOORWAY);
						}

						return false;
					}
				}
				else
				{
					point = Vector3.zero;

					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.DOORWAY);
					}

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.HATCH)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);

				if (hit.transform != null)
				{
					if (hit.transform.CompareTag("Logic") && hit.transform.name == "Hatch")
					{
						point = hit.transform.position;
						float hitAngle = Quaternion.LookRotation(hit.transform.forward).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 
						angle_y = hitAngle;

						float dot_0 = Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.forward);
						float dot_1 = Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.right);
						float dot_2 = Vector3.Dot(MainCamera.instance.transform.forward, -hit.transform.forward);
						float dot_3 = Vector3.Dot(MainCamera.instance.transform.forward, -hit.transform.right);

						float dot = dot_0;

						if (dot_1 < dot)
						{
							dot = dot_1;
							angle_y = hitAngle + 90.0f;
						}

						if (dot_2 < dot)
						{
							dot = dot_2;
							angle_y = hitAngle + 180.0f;
						}

						if (dot_3 < dot)
						{
							dot = dot_3;
							angle_y = hitAngle + 270.0f;
						}

						angle_y += 180.0f;

						if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BOUNDS);
							}

							return false;
						}

						if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_FRAME) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}
					}
					else
					{
						point = Vector3.zero;

						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.TRAPDOOR);
						}

						return false;
					}
				}
				else
				{
					point = Vector3.zero;

					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.TRAPDOOR);
					}

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.GATE)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);

				if (hit.transform != null)
				{
					if (hit.transform.CompareTag("Logic") && hit.transform.name == "Gate")
					{
						point = hit.transform.position;

						if (Mathf.Abs(Vector3.Dot(hit.transform.up, Vector3.up)) > 0.5f)
						{
							angle_y = Quaternion.LookRotation(hit.transform.forward).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 
							if (Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.forward) < 0.0f)
							{
								angle_y += 180.0f;
							}
						}
						else
						{
							angle_y = Quaternion.LookRotation(hit.transform.up).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 
							if (Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.up) > 0.0f)
							{
								angle_y += 180.0f;
							}
						}

						if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BOUNDS);
							}

							return false;
						}

						if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_FRAME) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}

						if (Physics.OverlapSphereNonAlloc(point + (hit.transform.forward * -1.5f) + (hit.transform.up * -2f), 0.25f, checkColliders, RayMasks.BLOCK_FRAME) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}
					}
					else
					{
						point = Vector3.zero;

						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.GARAGE);
						}

						return false;
					}
				}
				else
				{
					point = Vector3.zero;

					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.GARAGE);
					}

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.LADDER)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.LADDERS_INTERACT);

				if (hit.transform != null)
				{
					if (hit.transform.CompareTag("Logic") && hit.transform.name == "Climb")
					{
						point = hit.transform.position;
						angle_y = Quaternion.LookRotation(hit.transform.forward).eulerAngles.y; // Nelson 2024-08-08: Quickly patching public issue #4621. 

						if (Physics.OverlapSphereNonAlloc(point + (hit.transform.up * 0.5f), 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}

						if (Physics.OverlapSphereNonAlloc(point + (hit.transform.up * -0.5f), 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BLOCKED);
							}

							return false;
						}
					}
					else
					{
						if (Mathf.Abs(hit.normal.y) < 0.1f)
						{
							point = hit.point + (hit.normal * equippedBarricadeAsset.offset);
							angle_y = Quaternion.LookRotation(hit.normal).eulerAngles.y;

							if (Physics.OverlapSphereNonAlloc(point + (Quaternion.Euler(0, angle_y, 0) * Vector3.right * 0.5f), 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
							{
								if (channel.IsLocalPlayer)
								{
									PlayerUI.hint(null, EPlayerMessage.BLOCKED);
								}

								return false;
							}

							if (Physics.OverlapSphereNonAlloc(point + (Quaternion.Euler(0, angle_y, 0) * Vector3.left * 0.5f), 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
							{
								if (channel.IsLocalPlayer)
								{
									PlayerUI.hint(null, EPlayerMessage.BLOCKED);
								}

								return false;
							}
						}
						else
						{
							if (hit.normal.y > 0.75f)
							{
								point = hit.point + (hit.normal * StructureManager.HEIGHT);
							}
							else
							{
								point = hit.point + (Vector3.up * StructureManager.HEIGHT);
							}

							if (Physics.OverlapSphereNonAlloc(point, 0.5f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
							{
								if (channel.IsLocalPlayer)
								{
									PlayerUI.hint(null, EPlayerMessage.BLOCKED);
								}

								return false;
							}
						}

						if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
						{
							if (channel.IsLocalPlayer)
							{
								PlayerUI.hint(null, EPlayerMessage.BOUNDS);
							}

							return false;
						}
					}
				}
				else
				{
					point = Vector3.zero;

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.TORCH || equippedBarricadeAsset.build == EBuild.STORAGE_WALL || equippedBarricadeAsset.build == EBuild.SIGN_WALL || equippedBarricadeAsset.build == EBuild.CAGE || equippedBarricadeAsset.build == EBuild.BARRICADE_WALL)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);

				if (hit.transform != null && Mathf.Abs(hit.normal.y) < 0.1f)
				{
					point = hit.point + (hit.normal * equippedBarricadeAsset.offset);
					angle_y = Quaternion.LookRotation(hit.normal).eulerAngles.y;

					if (Physics.OverlapSphereNonAlloc(point, 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						}

						return false;
					}

					if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BOUNDS);
						}

						return false;
					}

					return true;
				}
				else
				{
					if (channel.IsLocalPlayer)
					{
						PlayerUI.hint(null, EPlayerMessage.WALL);
					}

					point = Vector3.zero;

					return false;
				}
			}

			if (equippedBarricadeAsset.build == EBuild.FREEFORM || equippedBarricadeAsset.build == EBuild.SENTRY_FREEFORM)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);

				if (hit.transform != null)
				{
					Quaternion rotation = Quaternion.Euler(0, angle_y + rotate_y, 0);
					rotation *= Quaternion.Euler(-90 + angle_x + rotate_x, 0, 0);
					rotation *= Quaternion.Euler(0, angle_z + rotate_z, 0);

					point = hit.point + (hit.normal * -0.125f) + (rotation * Vector3.forward * equippedBarricadeAsset.offset);

					if (!IsPlacementInsideClipVolumesAllowed && !Level.checkSafeIncludingClipVolumes(point))
					{
						if (channel.IsLocalPlayer)
						{
							PlayerUI.hint(null, EPlayerMessage.BOUNDS);
						}

						return false;
					}

					if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
					{
						if (channel.IsLocalPlayer)
						{
#if LOG_BARRICADE_PLACEMENT
							UnturnedLog.warn("Sphere overlap: {0}", checkColliders[0].GetSceneHierarchyPath());
#endif

							PlayerUI.hint(null, EPlayerMessage.BLOCKED);
						}

						return false;
					}
				}
				else
				{
					point = Vector3.zero;

					return false;
				}

				return true;
			}

			if (equippedBarricadeAsset.build == EBuild.CHARGE || equippedBarricadeAsset.build == EBuild.CLOCK || equippedBarricadeAsset.build == EBuild.NOTE)
			{
				Physics.SphereCast(player.look.aim.position, 0.1f, player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);

				if (hit.transform != null)
				{
					Vector3 rotation = Quaternion.LookRotation(hit.normal).eulerAngles;
					angle_x = rotation.x;
					angle_y = rotation.y;
					angle_z = rotation.z;

					point = hit.point + (hit.normal * equippedBarricadeAsset.offset);
				}
				else
				{
					point = Vector3.zero;

					return false;
				}

				return true;
			}

			point = Vector3.zero;

			return false;
		}

		private void build()
		{
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;
			isBuilding = true;

			player.animator.play("Use", false);
		}

		[System.Obsolete]
		public void askBuild(CSteamID steamID)
		{
			ReceivePlayBuild();
		}

		private static readonly ClientInstanceMethod SendPlayBuild = ClientInstanceMethod.Get(typeof(UseableBarricade), nameof(ReceivePlayBuild));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askBuild))]
		public void ReceivePlayBuild()
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				build();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (Dedicator.IsDedicatedServer ? isValid : check())
			{
				if (channel.IsLocalPlayer)
				{
					if (parent != null)
					{
						VehicleBarricadeRegion parentRegion = BarricadeManager.FindVehicleRegionByTransform(parent);
						if (parentRegion != null)
						{
							SendBarricadeVehicle.Invoke(GetNetId(), ENetReliability.Reliable, parent.InverseTransformPoint(point), angle_x + rotate_x, angle_y + rotate_y - parent.localRotation.eulerAngles.y, angle_z + rotate_z, parentRegion._netId);
						}
					}
					else
					{
						SendBarricadeNone.Invoke(GetNetId(), ENetReliability.Reliable, point, angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z);
					}
				}

				player.equipment.isBusy = true;

				build();

				if (Provider.isServer)
				{
					SendPlayBuild.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner());
				}

				return true;
			}
			else
			{
				if (Dedicator.IsDedicatedServer && wasAsked)
				{
#if LOG_BARRICADE_PLACEMENT_CANCEL
					UnturnedLog.warn("Placement canceled because startPrimary called but not isValid and wasAsked");
#endif
					player.equipment.dequip();

					return true;
				}
			}

			return false;
		}

		public override bool startSecondary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (equippedBarricadeAsset.build == EBuild.GLASS || equippedBarricadeAsset.build == EBuild.CHARGE || equippedBarricadeAsset.build == EBuild.CLOCK || equippedBarricadeAsset.build == EBuild.NOTE || equippedBarricadeAsset.build == EBuild.FORTIFICATION || equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.HATCH || equippedBarricadeAsset.build == EBuild.TORCH || equippedBarricadeAsset.build == EBuild.CAGE || equippedBarricadeAsset.build == EBuild.STORAGE_WALL || equippedBarricadeAsset.build == EBuild.SIGN_WALL || equippedBarricadeAsset.build == EBuild.BARRICADE_WALL)
			{
				return false;
			}

			player.look.isIgnoringInput = true;
			inputWantsToRotate = true;
			return true;
		}

		public override void stopSecondary()
		{
			player.look.isIgnoringInput = false;
			inputWantsToRotate = false;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			useTime = player.animator.GetAnimationLength("Use");

			if (Dedicator.IsDedicatedServer)
			{
				if (equippedBarricadeAsset.build == EBuild.MANNEQUIN)
				{
					boundsUse = true;
					boundsCenter = new Vector3(0.0f, 0.0f, -0.05f);
					boundsExtents = new Vector3(1.175f, 0.2f, 1.05f);
				}
				else if (equippedBarricadeAsset.barricade != null)
				{
					// needs to be instantiated for bounds to calc
					GameObject helper = Instantiate(equippedBarricadeAsset.barricade, Vector3.zero, Quaternion.identity);
					helper.name = "Helper";

					Collider col;
					if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER)
					{
						col = helper.transform.Find("Placeholder").GetComponent<Collider>();
						boundsDoubleDoor = helper.transform.Find("Skeleton").Find("Hinge") == null;
					}
					else
					{
						col = helper.GetComponentInChildren<Collider>();
					}

					if (col != null)
					{
						boundsUse = true;

						boundsCenter = helper.transform.InverseTransformPoint(col.bounds.center);
						boundsExtents = col.bounds.extents;
					}

					Destroy(helper);
				}

				boundsOverlap = boundsExtents + new Vector3(0.5f, 0.5f, 0.5f);
			}

			if (channel.IsLocalPlayer)
			{
#if !DEDICATED_SERVER
				GameObject placementPreviewPrefab = equippedBarricadeAsset.placementPreviewRef.loadAsset();
				if (placementPreviewPrefab != null)
				{
					help = Instantiate(placementPreviewPrefab).transform;
				}
#endif // !DEDICATED_SERVER
				if (help == null)
				{
					help = BarricadeTool.getBarricade(null, 0, Vector3.zero, Quaternion.identity, player.equipment.itemID, player.equipment.state);
				}
				guide = help.Find("Root");
				if (guide == null)
				{
					guide = help;
				}

				HighlighterTool.help(guide, isValid, isHighlightRecursive);

				arrow = ((GameObject) GameObject.Instantiate(Resources.Load("Build/Arrow"))).transform;
				arrow.name = "Arrow";
				arrow.parent = help;
				arrow.localPosition = Vector3.zero;

				if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.HATCH)
				{
					arrow.localRotation = Quaternion.identity;
				}
				else if (equippedBarricadeAsset.build == EBuild.MANNEQUIN)
				{
					// Hack to make mannequin placement consistent with other buildables. (public issue #3311)
					rotate_y = 180.0f;
					arrow.localEulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
				}
				else
				{
					arrow.localRotation = Quaternion.Euler(90, 0, 0);
				}

				Collider col;
				if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER)
				{
					col = help.Find("Placeholder").GetComponent<Collider>();
					boundsDoubleDoor = help.Find("Skeleton").Find("Hinge") == null;
				}
				else
				{
					col = help.GetComponentInChildren<Collider>();
				}

				if (equippedBarricadeAsset.build == EBuild.MANNEQUIN)
				{
					boundsUse = true;
					boundsCenter = new Vector3(0.0f, 0.0f, -0.05f);
					boundsExtents = new Vector3(1.175f, 0.2f, 1.05f);

					if (col != null)
					{
						Destroy(col);
					}
				}
				else
				{
					if (col != null)
					{
						boundsUse = true;

						boundsCenter = help.InverseTransformPoint(col.bounds.center);
						boundsExtents = col.bounds.extents;

						Destroy(col);
					}
				}

				boundsOverlap = boundsExtents + new Vector3(0.5f, 0.5f, 0.5f);

				if (equippedBarricadeAsset.build == EBuild.GLASS)
				{
					WaterHeightTransparentSort sort = help.GetComponentInChildren<WaterHeightTransparentSort>(true);
					if (sort != null)
					{
						Destroy(sort);
					}
				}

				HighlighterTool.help(arrow, isValid);

				if (help.Find("Radius") != null)
				{
					isPower = true;
					powerPoint = Vector3.zero;
					claimsInRadius = new List<InteractableClaim>();
					generatorsInRadius = new List<InteractableGenerator>();
					safezonesInRadius = new List<InteractableSafezone>();
					oxygenatorsInRadius = new List<InteractableOxygenator>();

					if (equippedBarricadeAsset.build == EBuild.CLAIM || equippedBarricadeAsset.build == EBuild.GENERATOR || equippedBarricadeAsset.build == EBuild.SAFEZONE || equippedBarricadeAsset.build == EBuild.OXYGENATOR)
					{
						help.Find("Radius").gameObject.SetActive(true);
					}
				}

				Interactable interact = help.GetComponent<Interactable>();
				if (interact != null)
				{
					Destroy(interact);
				}

				if (equippedBarricadeAsset.build == EBuild.SPIKE || equippedBarricadeAsset.build == EBuild.WIRE)
				{
					Transform trapTrigger = help.Find("Trap");
					if (trapTrigger != null)
					{
						trapTrigger.DestroyComponentIfExists<InteractableTrap>();
					}
				}

				if (equippedBarricadeAsset.build == EBuild.BEACON)
				{
					Destroy(help.GetComponent<InteractableBeacon>());
				}

				if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.HATCH)
				{
					if (help.Find("Placeholder") != null)
					{
						Destroy(help.Find("Placeholder").gameObject);
					}

					// Nelson 2024-09-18: Removing unnecessary InteractableDoorHinge component, which this section
					// previously looked for to remove door colliders. The hinge component was added to the following
					// transforms:
					Transform hinge = help.Find("Skeleton/Hinge");
					if (hinge != null)
					{
						CleanUpDoorHinge(hinge);
					}
					Transform leftHinge = help.Find("Skeleton/Left_Hinge");
					if (leftHinge != null)
					{
						CleanUpDoorHinge(leftHinge);
					}
					Transform rightHinge = help.Find("Skeleton/Right_Hinge");
					if (rightHinge != null)
					{
						CleanUpDoorHinge(rightHinge);
					}
				}
				else
				{
					if (help.Find("Clip") != null)
					{
						Destroy(help.Find("Clip").gameObject);
					}

					if (help.Find("Nav") != null)
					{
						Destroy(help.Find("Nav").gameObject);
					}

					if (help.Find("Ladder") != null)
					{
						Destroy(help.Find("Ladder").gameObject);
					}

					if (help.Find("Block") != null)
					{
						Destroy(help.Find("Block").gameObject);
					}
				}

				for (int step = 0; step < 2; step++)
				{
					if (help.Find("Climb") != null)
					{
						Destroy(help.Find("Climb").gameObject);
					}
					else
					{
						break;
					}
				}

				// Destroy any remaining colliders
				help.GetComponentsInChildren(true, colliders);
				for (int index = 0; index < colliders.Count; index++)
				{
					Destroy(colliders[index]);
				}
			}
		}

		public override void dequip()
		{
			player.look.isIgnoringInput = false;
			inputWantsToRotate = false;

			if (channel.IsLocalPlayer)
			{
				if (help != null)
				{
					Destroy(help.gameObject);
				}

				if (isPower)
				{
					for (int index = 0; index < claimsInRadius.Count; index++)
					{
						if (claimsInRadius[index] == null)
						{
							continue;
						}

						claimsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
					}
					claimsInRadius.Clear();

					for (int index = 0; index < generatorsInRadius.Count; index++)
					{
						if (generatorsInRadius[index] == null)
						{
							continue;
						}

						generatorsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
					}
					generatorsInRadius.Clear();

					for (int index = 0; index < safezonesInRadius.Count; index++)
					{
						if (safezonesInRadius[index] == null)
						{
							continue;
						}

						safezonesInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
					}
					safezonesInRadius.Clear();

					for (int index = 0; index < oxygenatorsInRadius.Count; index++)
					{
						if (oxygenatorsInRadius[index] == null)
						{
							continue;
						}

						oxygenatorsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
					}
					oxygenatorsInRadius.Clear();
				}
			}

			BuildRequestManager.finishPendingBuild(ref pendingBuildHandle);
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;

				if (Provider.isServer)
				{
					int boundsOverlapMask;
					if (parentVehicle != null)
						boundsOverlapMask = RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP;
					else
						boundsOverlapMask = RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP_NOT_ON_VEHICLE;

					int boundHitCount = 0;
					if (boundsUse)
					{
						boundHitCount = Physics.OverlapBoxNonAlloc(getPointInWorldSpace() + (boundsRotation * boundsCenter),
							boundsOverlap,
							checkColliders,
							boundsRotation,
							boundsOverlapMask,
							QueryTriggerInteraction.Collide);
					}

					if (boundHitCount > 0)
					{
#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn("Placement canceled due to overlaps");
#endif
						player.equipment.dequip();
					}
					else if (parentVehicle != null && parentVehicle.isGoingToRespawn)
					{
#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn("Placement canceled because vehicle will respawn");
#endif
						player.equipment.dequip();
					}
					else if (parentVehicle != null && parentVehicle.isHooked)
					{
#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn("Placement canceled because vehicle is hooked");
#endif
						// Vehicle is constrained to another vehicle, e.g. skycrane. Building now can cause physics
						// problems. Unfortunately client does not know vehicle is hooked, so there is no warning.
						player.equipment.dequip();
					}
					else if (!checkClaims()) // Double-check everything is still OK in-case another buildable was placed while we were placing this one.
					{
#if LOG_BARRICADE_PLACEMENT_CANCEL
						UnturnedLog.warn("Placement canceled because claims changed");
#endif
						player.equipment.dequip();
					}
					else
					{
						ItemBarricadeAsset asset = (ItemBarricadeAsset) player.equipment.asset;
						bool bSuccess = false;

						if (asset != null)
						{
							player.sendStat(EPlayerStat.FOUND_BUILDABLES);

							if (asset.build == EBuild.VEHICLE)
							{
								Asset vehicleAsset = asset.FindVehicleAsset();
								if (vehicleAsset != null)
								{
									bSuccess = VehicleManager.spawnLockedVehicleForPlayerV2(vehicleAsset, point, Quaternion.Euler(angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z), player) != null;
								}
							}
							else
							{
								Barricade barricade = new Barricade(asset);

								if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.SIGN || asset.build == EBuild.SIGN_WALL || asset.build == EBuild.NOTE || asset.build == EBuild.HATCH)
								{
									System.BitConverter.GetBytes(channel.owner.playerID.steamID.m_SteamID).CopyTo(barricade.state, 0);
									System.BitConverter.GetBytes(player.quests.groupID.m_SteamID).CopyTo(barricade.state, 8);
								}
								else if (asset.build == EBuild.BED)
								{
									System.BitConverter.GetBytes(CSteamID.Nil.m_SteamID).CopyTo(barricade.state, 0);
								}
								else if (asset.build == EBuild.STORAGE || asset.build == EBuild.STORAGE_WALL || asset.build == EBuild.MANNEQUIN || asset.build == EBuild.SENTRY || asset.build == EBuild.SENTRY_FREEFORM || asset.build == EBuild.LIBRARY || asset.build == EBuild.MANNEQUIN)
								{
									System.BitConverter.GetBytes(channel.owner.playerID.steamID.m_SteamID).CopyTo(barricade.state, 0);
									System.BitConverter.GetBytes(player.quests.groupID.m_SteamID).CopyTo(barricade.state, 8);
								}
								else if (asset.build == EBuild.FARM)
								{
									System.BitConverter.GetBytes(Provider.time - (uint) (((ItemFarmAsset) player.equipment.asset).growth * (player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.AGRICULTURE) * 0.25f))).CopyTo(barricade.state, 0);
								}
								else if (asset.build == EBuild.TORCH || asset.build == EBuild.CAMPFIRE || asset.build == EBuild.OVEN || asset.build == EBuild.SPOT || asset.build == EBuild.SAFEZONE || asset.build == EBuild.OXYGENATOR || asset.build == EBuild.CAGE)
								{
									barricade.state[0] = 1;
								}
								else if (asset.build == EBuild.GENERATOR)
								{
									barricade.state[0] = 1;
									//System.BitConverter.GetBytes((ushort) (InteractableGenerator.FUEL / 2)).CopyTo(barricade.state, 1);
								}
								else if (asset.build == EBuild.STEREO)
								{
									barricade.state[16] = 100;
								}

								bSuccess = BarricadeManager.dropBarricade(barricade, parent, point, angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z, channel.owner.playerID.steamID.m_SteamID, player.quests.groupID.m_SteamID) != null;
							}
						}

						if (bSuccess)
						{
							player.equipment.use();
						}
						else
						{
#if LOG_BARRICADE_PLACEMENT_CANCEL
							UnturnedLog.warn("Placement canceled because barricade drop failed");
#endif
							player.equipment.dequip();
						}
					}
				}
			}
		}

		private void processRotationInput()
		{
			if (allowRotationInputOnAllAxes)
			{
				if (ControlsSettings.invert)
				{
					input_x += ControlsSettings.mouseAimSensitivity * 2f * Input.GetAxis("mouse_y");
				}
				else
				{
					input_x -= ControlsSettings.mouseAimSensitivity * 2f * Input.GetAxis("mouse_y");
				}
			}

			input_y += ControlsSettings.mouseAimSensitivity * 2f * Input.GetAxis("mouse_x");

			if (allowRotationInputOnAllAxes)
			{
				input_z += ControlsSettings.mouseAimSensitivity * 30f * Input.GetAxis("mouse_z");
			}

			if (InputEx.GetKey(ControlsSettings.snap))
			{
				rotate_x = (int) (input_x / 15.0f) * 15.0f;
				rotate_y = (int) (input_y / 15.0f) * 15.0f;
				rotate_z = (int) (input_z / 15.0f) * 15.0f;
			}
			else
			{
				rotate_x = input_x;
				rotate_y = input_y;
				rotate_z = input_z;
			}
		}

		public override void tick()
		{
			if (isBuilding && isBuildable)
			{
				isBuilding = false;

				if (!Dedicator.IsDedicatedServer)
				{
					player.playSound(equippedBarricadeAsset.use);
				}

				if (Provider.isServer)
				{
					AlertTool.alert(transform.position, 8);
				}
			}

			if (channel.IsLocalPlayer)
			{
				if (help == null)
				{
					return;
				}

				if (isUsing)
				{
					return;
				}

				if (inputWantsToRotate)
				{
					processRotationInput();
				}

				if (check())
				{
					if (!isValid)
					{
						isValid = true;

						HighlighterTool.help(guide, isValid, isHighlightRecursive);

						if (arrow != null)
						{
							HighlighterTool.help(arrow, isValid);
						}
					}
				}
				else
				{
					if (isValid)
					{
						isValid = false;

						HighlighterTool.help(guide, isValid, isHighlightRecursive);

						if (arrow != null)
						{
							HighlighterTool.help(arrow, isValid);
						}
					}
				}

				bool parentUpdated = help.parent != parent;
				if (parentUpdated)
				{
					help.parent = parent;
					help.gameObject.SetActive(false);
					help.gameObject.SetActive(true);
				}

				if (parent != null)
				{
					help.localPosition = parent.InverseTransformPoint(point);
					help.localRotation = Quaternion.Euler(0, angle_y + rotate_y - parent.localRotation.eulerAngles.y, 0);
					help.localRotation *= Quaternion.Euler(((equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.HATCH) ? 0 : -90) + angle_x + rotate_x, 0, 0);
					help.localRotation *= Quaternion.Euler(0, angle_z + rotate_z, 0);
				}
				else
				{
					help.position = point;
					help.rotation = Quaternion.Euler(0, angle_y + rotate_y, 0);
					help.rotation *= Quaternion.Euler(((equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.HATCH) ? 0 : -90) + angle_x + rotate_x, 0, 0);
					help.rotation *= Quaternion.Euler(0, angle_z + rotate_z, 0);
				}

				if (isPower)
				{
					bool powerUpdated = parentUpdated;

					if ((transform.position - powerPoint).sqrMagnitude > 1.0f)
					{
						powerPoint = transform.position;
						powerUpdated = true;
					}

					if (powerUpdated)
					{
						for (int index = 0; index < claimsInRadius.Count; index++)
						{
							if (claimsInRadius[index] == null)
							{
								continue;
							}

							claimsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
						}
						claimsInRadius.Clear();

						for (int index = 0; index < generatorsInRadius.Count; index++)
						{
							if (generatorsInRadius[index] == null)
							{
								continue;
							}

							generatorsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
						}
						generatorsInRadius.Clear();

						for (int index = 0; index < safezonesInRadius.Count; index++)
						{
							if (safezonesInRadius[index] == null)
							{
								continue;
							}

							safezonesInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
						}
						safezonesInRadius.Clear();

						for (int index = 0; index < oxygenatorsInRadius.Count; index++)
						{
							if (oxygenatorsInRadius[index] == null)
							{
								continue;
							}

							oxygenatorsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(false);
						}
						oxygenatorsInRadius.Clear();

						byte x;
						byte y;
						ushort plant;
						BarricadeRegion region;
						BarricadeManager.tryGetPlant(parent, out x, out y, out plant, out region);

						if (equippedBarricadeAsset.build == EBuild.CLAIM)
						{
							PowerTool.checkInteractables(powerPoint, 64.0f, plant, claimsInRadius);
							for (int index = 0; index < claimsInRadius.Count; index++)
							{
								if (claimsInRadius[index] == null)
								{
									continue;
								}

								claimsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(true);
							}
						}
						else
						{
							PowerTool.checkInteractables(powerPoint, 64.0f, plant, generatorsInRadius);
							for (int index = 0; index < generatorsInRadius.Count; index++)
							{
								if (generatorsInRadius[index] == null)
								{
									continue;
								}

								generatorsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(true);
							}
						}

						if (equippedBarricadeAsset.build == EBuild.SAFEZONE)
						{
							PowerTool.checkInteractables(powerPoint, 64.0f, plant, safezonesInRadius);
							for (int index = 0; index < safezonesInRadius.Count; index++)
							{
								if (safezonesInRadius[index] == null)
								{
									continue;
								}

								safezonesInRadius[index].transform.Find("Radius")?.gameObject.SetActive(true);
							}
						}

						if (equippedBarricadeAsset.build == EBuild.OXYGENATOR)
						{
							PowerTool.checkInteractables(powerPoint, 64.0f, plant, oxygenatorsInRadius);
							for (int index = 0; index < oxygenatorsInRadius.Count; index++)
							{
								if (oxygenatorsInRadius[index] == null)
								{
									continue;
								}

								oxygenatorsInRadius[index].transform.Find("Radius")?.gameObject.SetActive(true);
							}
						}
					}
				}
			}
		}

		protected void OnDestroy()
		{
			// Dequip should handle this, but just to be safe:
			BuildRequestManager.finishPendingBuild(ref pendingBuildHandle);
		}

		private void CleanUpDoorHinge(Transform hingeTransform)
		{
			Transform clip = hingeTransform.Find("Clip");
			if (clip != null)
			{
				Destroy(clip.gameObject);
			}

			Transform nav = hingeTransform.Find("Nav");
			if (nav != null)
			{
				Destroy(nav.gameObject);
			}

			Collider col = hingeTransform.GetComponent<Collider>();
			if (col != null)
			{
				Destroy(col);
			}
		}

		private string CheckCollidersToString()
		{
			return checkColliders[0] != null ? checkColliders[0].GetSceneHierarchyPath() : "null";
		}

		private bool IsPlacementInsideClipVolumesAllowed => equippedBarricadeAsset.AllowPlacementInsideClipVolumes || Provider.modeConfigData.Gameplay.Bypass_No_Building_Zones;
	}
}
