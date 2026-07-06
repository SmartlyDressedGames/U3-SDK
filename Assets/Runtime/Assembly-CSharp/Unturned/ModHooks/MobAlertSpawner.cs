////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows Unity events to startle nearby animals and zombies.
	/// </summary>
	[AddComponentMenu("Unturned/Mob Alert Spawner")]
	public class MobAlertSpawner : MonoBehaviour
	{
		public float AlertRadius;

		/// <summary>
		/// Transform to spawn the alert at.
		/// If unset, this game object's transform will be used instead.
		/// </summary>
		public Transform AlertOriginTransformOverride;

		/// <summary>
		/// If true, find the nearest player within ScanForPlayersRadius, and use the player's position as the alert
		/// origin.
		/// </summary>
		public bool UseScanForPlayers;

		public float ScanForPlayersRadius;

		/// <summary>
		/// If UseScanForPlayers is enabled and this is true, an alert is broadcast even if no nearby player was found.
		/// Otherwise, alert is ignored if no nearby player was found.
		/// </summary>
		public bool ScanForPlayersUseTransformAsFallback;

		public void Alert()
		{
#if GAME
			if (!Provider.isServer)
			{
				return;
			}

			if (EffectManager.isInstantiatingEffectForPreload)
			{
				// Prevent effect preload from unintentionally spawning stuff.
				return;
			}

			Vector3 position;
			if (AlertOriginTransformOverride != null)
			{
				position = AlertOriginTransformOverride.position;
			}
			else
			{
				position = transform.position;
			}

			if (UseScanForPlayers)
			{
				Player nearestPlayer = PlayerTool.GetNearestPlayerInRadius(position, ScanForPlayersRadius * ScanForPlayersRadius);
				if (nearestPlayer != null)
				{
					position = nearestPlayer.transform.position;
				}
				else if (!ScanForPlayersUseTransformAsFallback)
				{
					return;
				}
			}

			AlertTool.alert(position, AlertRadius);
#endif // GAME
		}
	}
}
