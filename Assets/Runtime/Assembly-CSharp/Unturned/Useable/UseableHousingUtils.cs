////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal static class UseableHousingUtils
	{
		public static Transform InstantiatePlacementPreview(ItemStructureAsset asset)
		{
			Transform placementPreviewTransform = null;
#if !DEDICATED_SERVER
			GameObject placementPreviewPrefab = asset.placementPreviewRef.loadAsset();
			if (placementPreviewPrefab != null)
			{
				placementPreviewTransform = Object.Instantiate(placementPreviewPrefab).transform;
			}
#endif // !DEDICATED_SERVER
			if (placementPreviewTransform == null)
			{
				placementPreviewTransform = StructureTool.getStructure(asset.id, 0);
			}
			placementPreviewTransform.position = Vector3.zero;
			placementPreviewTransform.rotation = Quaternion.Euler(-90, 0, 0);

			// Prevent any colliders from interfering with placement.
			Collider[] colliders = placementPreviewTransform.GetComponentsInChildren<Collider>();
			foreach (Collider colliderToRemove in colliders)
			{
				Object.Destroy(colliderToRemove);
			}

			HighlighterTool.help(placementPreviewTransform, false);

			if (placementPreviewTransform.Find("Clip") != null)
			{
				Object.Destroy(placementPreviewTransform.Find("Clip").gameObject);
			}

			if (placementPreviewTransform.Find("Nav") != null)
			{
				Object.Destroy(placementPreviewTransform.Find("Nav").gameObject);
			}

			if (placementPreviewTransform.Find("Cutter") != null)
			{
				Object.Destroy(placementPreviewTransform.Find("Cutter").gameObject);
			}

			if (placementPreviewTransform.Find("Block") != null)
			{
				Object.Destroy(placementPreviewTransform.Find("Block").gameObject);
			}

			return placementPreviewTransform;
		}

		public static EHousingPlacementResult ValidatePendingPlacement(ItemStructureAsset asset, ref Vector3 position, float yaw, ref string obstructionHint)
		{
			try
			{
				switch (asset.construct)
				{
					case EConstruct.FLOOR:
						return StructureManager.housingConnections.ValidateSquareFloorPlacement(asset.terrainTestHeight, ref position, yaw, ref obstructionHint);

					case EConstruct.WALL:
						return StructureManager.housingConnections.ValidateWallPlacement(ref position, HousingConnections.WALL_PIVOT_OFFSET, asset.requiresPillars, true, ref obstructionHint);

					case EConstruct.RAMPART:
						return StructureManager.housingConnections.ValidateWallPlacement(ref position, HousingConnections.RAMPART_PIVOT_OFFSET, asset.requiresPillars, false, ref obstructionHint);

					case EConstruct.ROOF:
						return StructureManager.housingConnections.ValidateSquareRoofPlacement(ref position, yaw, ref obstructionHint);

					case EConstruct.PILLAR:
						return StructureManager.housingConnections.ValidatePillarPlacement(ref position, HousingConnections.WALL_PIVOT_OFFSET, ref obstructionHint);

					case EConstruct.POST:
						return StructureManager.housingConnections.ValidatePillarPlacement(ref position, HousingConnections.RAMPART_PIVOT_OFFSET, ref obstructionHint);

					case EConstruct.FLOOR_POLY:
						return StructureManager.housingConnections.ValidateTriangleFloorPlacement(asset.terrainTestHeight, ref position, yaw, ref obstructionHint);

					case EConstruct.ROOF_POLY:
						return StructureManager.housingConnections.ValidateTriangleRoofPlacement(ref position, yaw, ref obstructionHint);
				}

				UnturnedLog.error("Unhandled housing type: " + asset.construct);
			}
			catch (System.Exception e)
			{
				// try/catch because I do not want to risk breaking structures in the big update
				UnturnedLog.exception(e, "Caught exception while validating housing placement:");
			}

			return EHousingPlacementResult.Success;
		}

		public static bool FindPlacement(ItemStructureAsset asset, Player player, float rotationOffset, float foundationOffset, out Vector3 pendingPlacementPosition, out float pendingPlacementYaw)
		{
			SteamChannel channel = player.channel;
			pendingPlacementPosition = default;
			pendingPlacementYaw = player.look.yaw;
			EHousingPlacementResult result = EHousingPlacementResult.Success;
			string obstructionHint = null;
			Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);

			if (asset.construct == EConstruct.FLOOR || asset.construct == EConstruct.FLOOR_POLY)
			{
				if (!StructureManager.housingConnections.FindEmptyFloorSlot(ray, false, out pendingPlacementPosition, out pendingPlacementYaw))
				{
					RaycastHit hit;
					bool hitAnything = Physics.SphereCast(ray, 0.1f, out hit, asset.range, RayMasks.STRUCTURE_INTERACT);
					if (hitAnything)
					{
						pendingPlacementPosition = hit.point;
						pendingPlacementYaw = player.look.yaw;
						if (asset.construct == EConstruct.FLOOR_POLY)
						{
							pendingPlacementPosition += Quaternion.Euler(0.0f, pendingPlacementYaw, 0.0f) * new Vector3(0.0f, 0.0f, -HousingConnections.TRIANGLE_CENTER_PIVOT_OFFSET);
						}
						pendingPlacementPosition += new Vector3(0.0f, foundationOffset, 0.0f);

						if (!StructureManager.housingConnections.DoesHitCountAsTerrain(hit))
						{
							result = EHousingPlacementResult.MissingGround;
						}
					}
					else
					{
						pendingPlacementPosition = Vector3.zero;

						return false;
					}
				}
			}
			else if (asset.construct == EConstruct.WALL || asset.construct == EConstruct.RAMPART)
			{
				if (StructureManager.housingConnections.FindEmptyWallSlot(ray, out pendingPlacementPosition, out pendingPlacementYaw))
				{
					if (asset.construct == EConstruct.RAMPART)
					{
						pendingPlacementPosition += Vector3.down * 1.225f;
					}
				}
				else
				{
					result = EHousingPlacementResult.MissingSlot;
				}
			}
			else if (asset.construct == EConstruct.ROOF || asset.construct == EConstruct.ROOF_POLY)
			{
				if (!StructureManager.housingConnections.FindEmptyFloorSlot(ray, true, out pendingPlacementPosition, out pendingPlacementYaw))
				{
					result = EHousingPlacementResult.MissingSlot;
				}
			}
			else if (asset.construct == EConstruct.PILLAR || asset.construct == EConstruct.POST)
			{
				if (StructureManager.housingConnections.FindEmptyPillarSlot(ray, out pendingPlacementPosition, out pendingPlacementYaw))
				{
					if (asset.construct == EConstruct.POST)
					{
						pendingPlacementPosition += Vector3.down * 1.225f;
					}
				}
				else
				{
					result = EHousingPlacementResult.MissingSlot;
				}
			}

			if (result == EHousingPlacementResult.Success)
			{
				result = ValidatePendingPlacement(asset, ref pendingPlacementPosition, pendingPlacementYaw + rotationOffset, ref obstructionHint);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (result == EHousingPlacementResult.MissingSlot)
				{
					UnturnedLog.warn("MissingSlot housing result is probably a bug because we *did* find a slot");
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}

			if (channel.IsLocalPlayer)
			{
				if (result == EHousingPlacementResult.MissingSlot)
				{
					switch (asset.construct)
					{
						case EConstruct.WALL:
						case EConstruct.RAMPART:
							PlayerUI.hint(null, EPlayerMessage.WALL);
							break;

						case EConstruct.ROOF:
						case EConstruct.ROOF_POLY:
							PlayerUI.hint(null, EPlayerMessage.ROOF);
							break;

						case EConstruct.PILLAR:
						case EConstruct.POST:
							PlayerUI.hint(null, EPlayerMessage.CORNER);
							break;
					}
				}
				else if (result == EHousingPlacementResult.Obstructed)
				{
					if (string.IsNullOrEmpty(obstructionHint))
					{
						PlayerUI.hint(null, EPlayerMessage.BLOCKED);
					}
					else
					{
						PlayerUI.hint(null, EPlayerMessage.PLACEMENT_OBSTRUCTED_BY, obstructionHint, Color.white);
					}
				}
				else if (result == EHousingPlacementResult.MissingPillar)
				{
					switch (asset.construct)
					{
						case EConstruct.WALL:
						case EConstruct.ROOF:
						case EConstruct.ROOF_POLY:
							PlayerUI.hint(null, EPlayerMessage.PILLAR);
							break;

						case EConstruct.RAMPART:
							PlayerUI.hint(null, EPlayerMessage.POST);
							break;
					}
				}
				else if (result == EHousingPlacementResult.MissingGround)
				{
					PlayerUI.hint(null, EPlayerMessage.GROUND);
				}
				else if (result == EHousingPlacementResult.ObstructedByGround)
				{
					PlayerUI.hint(null, EPlayerMessage.PLACEMENT_OBSTRUCTED_BY_GROUND);
				}
			}

			return result == EHousingPlacementResult.Success;
		}

		public static bool IsPendingPositionValid(Player player, Vector3 pendingPlacementPosition)
		{
			SteamChannel channel = player.channel;

			if (player.movement.isSafe && !player.movement.isSafeInfo.CurrentlyAllowsBuilding)
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.SAFEZONE);
				}

				return false;
			}

			if (!Level.isPointWithinValidHeight(pendingPlacementPosition.y))
			{
				PlayerUI.hint(null, EPlayerMessage.BOUNDS);

				return false;
			}

			if (!ClaimManager.checkCanBuild(pendingPlacementPosition, channel.owner.playerID.steamID, player.quests.groupID, false))
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.CLAIM);
				}

				return false;
			}

			bool bypassNoBuild = Provider.modeConfigData?.Gameplay?.Bypass_No_Building_Zones ?? false;

			if (!bypassNoBuild && SDG.Framework.Devkit.PlayerClipVolumeManager.Get().IsPositionInsideAnyVolume(pendingPlacementPosition))
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.BOUNDS);
				}

				return false;
			}

			if (!LevelPlayers.checkCanBuild(pendingPlacementPosition))
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.SPAWN);
				}

				return false;
			}

			if (!bypassNoBuild && NoStructuresVolumeManager.Get().IsPositionInsideAnyVolume(pendingPlacementPosition))
			{
				if (channel.IsLocalPlayer)
				{
					PlayerUI.hint(null, EPlayerMessage.INSIDE_NO_STRUCTURES_VOLUME);
				}

				return false;
			}

			return true;
		}

		public const float FOUNDATION_MOUSE_SCROLL_MULTIPLIER = 0.05f;
		public const float FOUNDATION_MIN_OFFSET = -1.0f;
		public const float FOUNDATION_MAX_OFFSET = 1.0f;
	}
}
