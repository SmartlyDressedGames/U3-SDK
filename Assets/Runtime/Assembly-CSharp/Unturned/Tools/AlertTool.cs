////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define LOG_ALERTS
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define ALERT_SPHERE_GIZMOS
// #define ALERT_LINE_OF_SIGHT_GIZMOS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class AlertTool
	{
		private static List<Zombie> zombiesInRadius = new List<Zombie>();
		private static List<Animal> animalsInRadius = new List<Animal>();

		private static bool check(Vector3 forward, Vector3 offset, float sqrRadius, bool sneak, Vector3 spotlightDir, bool isSpotlightOn, bool isLightSensitive)
		{
			if (isSpotlightOn && offset.sqrMagnitude < 576)
			{
				float spotDot = Vector3.Dot(spotlightDir, offset.normalized);

				if (spotDot > (isLightSensitive ? 0.4f : 0.75f))
				{
					return true; // detected flashlight
				}
			}

			if (offset.sqrMagnitude > sqrRadius)
			{
				return false; // too far away anyway
			}

			float sneakDot = Vector3.Dot(forward, offset.normalized);

			if (sneakDot > 0.5 && sneak)
			{
				return false; // behind them and sneaking
			}

			return true;
		}

		/// <summary>
		/// Alerts any agents in the area to the player if needed.
		/// </summary>
		/// <param name="player">The player causing this alert.</param>
		/// <param name="position">The position of the alert.</param>
		/// <param name="radius">The detection radius.</param>
		/// <param name="sneak">Whether or not to hide.</param>
		public static void alert(Player player, Vector3 position, float radius, bool sneak, Vector3 spotDir, bool isSpotOn)
		{
			LevelAsset levelAsset = Level.getAsset();
			float minRadius = levelAsset != null ? levelAsset.minStealthRadius : 0.0f;
			// Require at least 1.0m regardless of level config otherwise player's radius can't overlap zombies.
			minRadius = Mathf.Max(1.0f, minRadius);

			radius *= Provider.modeConfigData.Players.Detect_Radius_Multiplier;
			radius = Mathf.Clamp(radius, minRadius, 64.0f);

			if (player == null)
			{
				return;
			}

			float sqrRadius = radius * radius;
			RaycastHit hit;

			LogAlert("{0} Position: {1} Radius: {2} Nav: {3}", player.channel.owner.playerID.playerName, position, radius, player.movement.nav);

#if ALERT_SPHERE_GIZMOS
			RuntimeGizmos.Get().Sphere(position, radius, Color.red, 1.0f);
#endif // ALERT_SPHERE_GIZMOS

			if (player.movement.nav != 255)
			{
				ZombieRegion zombieRegion = ZombieManager.regions[player.movement.nav];
				if (zombieRegion.HasInfiniteAgroRange)
				{
					for (int index = 0; index < zombieRegion.zombies.Count; index++)
					{
						Zombie zombie = zombieRegion.zombies[index];

						if (zombie.isDead)
						{
							continue;
						}

						if (!zombie.checkAlert(player))
						{
							continue;
						}

						zombie.alert(player);
					}
				}

				zombiesInRadius.Clear();
				ZombieManager.getZombiesInRadius(position, sqrRadius, zombiesInRadius);

				for (int index = 0; index < zombiesInRadius.Count; index++)
				{
					Zombie zombie = zombiesInRadius[index];

					if (zombie.isDead)
					{
						continue;
					}

					if (!zombie.checkAlert(player))
					{
						continue;
					}

					Vector3 offset = zombie.transform.position - position;

					if (!check(zombie.transform.forward, offset, sqrRadius, sneak, spotDir, isSpotOn, zombie.speciality.IsDLVolatile()))
					{
						continue;
					}

					Vector3 rayStart = zombie.transform.position + Vector3.up;
					Physics.Raycast(rayStart, -offset, out hit, offset.magnitude * 0.95f, RayMasks.BLOCK_VISION);
#if ALERT_LINE_OF_SIGHT_GIZMOS
					RuntimeGizmos.Get().Linecast(rayStart, rayStart - offset, hit, Color.green, Color.red);
#endif // ALERT_LINE_OF_SIGHT_GIZMOS

					if (hit.transform != null)
					{
						continue;
					}

					zombie.alert(player);
				}
			}

			animalsInRadius.Clear();
			AnimalManager.getAnimalsInRadius(position, sqrRadius, animalsInRadius);

			for (int index = 0; index < animalsInRadius.Count; index++)
			{
				Animal animal = animalsInRadius[index];

				if (animal.isDead)
				{
					continue;
				}

				if (animal.asset == null)
				{
					continue;
				}

				if (animal.asset.behaviour == EAnimalBehaviour.DEFENSE)
				{
					if (!animal.isFleeing)
					{
						Vector3 offset = animal.transform.position - position;

						if (!check(animal.transform.forward, offset, sqrRadius, sneak, spotDir, isSpotOn, false))
						{
							continue;
						}

						Vector3 rayStart = animal.transform.position + Vector3.up;
						Physics.Raycast(rayStart, -offset, out hit, offset.magnitude * 0.95f, RayMasks.BLOCK_VISION);
#if ALERT_LINE_OF_SIGHT_GIZMOS
						RuntimeGizmos.Get().Linecast(rayStart, rayStart - offset, hit, Color.green, Color.red);
#endif // ALERT_LINE_OF_SIGHT_GIZMOS

						if (hit.transform != null)
						{
							continue;
						}
					}

					animal.alertRunAwayFromPoint(player.transform.position, true);
				}
				else if (animal.asset.behaviour == EAnimalBehaviour.OFFENSE)
				{
					if (!animal.checkAlert(player))
					{
						continue;
					}

					Vector3 offset = animal.transform.position - position;

					if (!check(animal.transform.forward, offset, sqrRadius, sneak, spotDir, isSpotOn, false))
					{
						continue;
					}

					Vector3 rayStart = animal.transform.position + Vector3.up;
					Physics.Raycast(rayStart, -offset, out hit, offset.magnitude * 0.95f, RayMasks.BLOCK_VISION);
#if ALERT_LINE_OF_SIGHT_GIZMOS
					RuntimeGizmos.Get().Linecast(rayStart, rayStart - offset, hit, Color.green, Color.red);
#endif // ALERT_LINE_OF_SIGHT_GIZMOS

					if (hit.transform != null)
					{
						continue;
					}

					animal.alertPlayer(player, true);
				}
			}
		}

		/// <summary>
		/// Alerts any agents in the area.
		/// </summary>
		/// <param name="position">The position of the alert.</param>
		/// <param name="radius">The detection radius.</param>
		public static void alert(Vector3 position, float radius)
		{
			float sqrRadius = radius * radius;

			LogAlert("Position: {0} Radius: {1}", position, radius);

			if (LevelNavigation.checkNavigation(position))
			{
				zombiesInRadius.Clear();
				ZombieManager.getZombiesInRadius(position, sqrRadius, zombiesInRadius);

				for (int index = 0; index < zombiesInRadius.Count; index++)
				{
					Zombie zombie = zombiesInRadius[index];

					if (zombie.isDead)
					{
						continue;
					}

					zombie.alert(position, true);
				}
			}

			animalsInRadius.Clear();
			AnimalManager.getAnimalsInRadius(position, sqrRadius, animalsInRadius);

			for (int index = 0; index < animalsInRadius.Count; index++)
			{
				Animal animal = animalsInRadius[index];

				if (animal.isDead)
				{
					continue;
				}

				if (animal.asset == null)
				{
					continue;
				}

				if (animal.asset.behaviour == EAnimalBehaviour.DEFENSE)
				{
					// Run away from this point.
					animal.alertRunAwayFromPoint(position, true);
				}
				else if (animal.asset.behaviour == EAnimalBehaviour.OFFENSE)
				{
					// Investigate this point.
					animal.alertGoToPoint(position, true);
				}
			}
		}

		[System.Diagnostics.Conditional("LOG_ALERTS")]
		private static void LogAlert(string format, params object[] args)
		{
			UnturnedLog.info(format, args);
		}
	}
}
