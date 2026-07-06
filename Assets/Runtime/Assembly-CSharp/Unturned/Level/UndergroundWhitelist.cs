////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned
{
	[System.Obsolete("Renamed to UndergroundAllowlist")]
	public static class UndergroundWhitelist
	{
		public static bool isPointInsideVolume(Vector3 worldspacePosition)
		{
			return UndergroundWhitelistVolumeManager.Get().IsPositionInsideAnyVolume(worldspacePosition);
		}

		/// <summary>
		/// If level is using underground whitelist then conditionally clamp world-space position.
		/// </summary>
		[System.Obsolete("Renamed to UndergroundAllowlist.AdjustPosition")]
		public static bool adjustPosition(ref Vector3 worldspacePosition, float offset, float threshold = 0.1f)
		{
			return UndergroundAllowlist.AdjustPosition(ref worldspacePosition, offset, threshold);
		}
	}

	public static class UndergroundAllowlist
	{
		/// <summary>
		/// If level is using underground allowlist then conditionally clamp world-space position.
		/// </summary>
		public static bool AdjustPosition(ref Vector3 worldspacePosition, float offset, float threshold = 0.1f)
		{
			if (Level.info == null || !Level.info.configData.Use_Underground_Whitelist)
			{
				return false;
			}

			if (UndergroundWhitelistVolumeManager.Get().IsPositionInsideAnyVolume(worldspacePosition))
			{
				return false;
			}

			float groundHeight = LevelGround.getHeight(worldspacePosition);
			if (worldspacePosition.y < groundHeight - threshold)
			{
				worldspacePosition.y = groundHeight + offset;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Used by animals and zombies to teleport to a spawnpoint if outside the map.
		/// </summary>
		public static bool IsPositionWithinValidHeight(Vector3 position, float threshold = 0.1f)
		{
			if (position.y < -1024.0f || position.y > 1024.0f)
			{
				return false;
			}

			if (Level.info == null || !Level.info.configData.Use_Underground_Whitelist)
			{
				return true;
			}

			float groundHeight = LevelGround.getHeight(position);
			if (position.y > groundHeight - threshold)
			{
				return true;
			}

			// Position is underground.
			return UndergroundWhitelistVolumeManager.Get().IsPositionInsideAnyVolume(position);
		}

		/// <summary>
		/// Used by housing validation to check item isn't placed underground.
		/// </summary>
		public static bool IsPositionBuildable(Vector3 position)
		{
			if (Level.info == null || !Level.info.configData.Use_Underground_Whitelist)
			{
				return true;
			}

			float groundHeight = LevelGround.getHeight(position);
			if (position.y > groundHeight)
			{
				return true;
			}

			// Position is underground.
			return UndergroundWhitelistVolumeManager.Get().IsPositionInsideAnyVolume(position);
		}
	}
}
