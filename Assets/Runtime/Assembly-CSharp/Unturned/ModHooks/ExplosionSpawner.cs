////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if GAME
using Steamworks;
#endif // GAME
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allow Unity events to apply damage in a sphere. (doesn't have any visual effects)
	/// Intended to replace unsupported/unintentional use of "Grenade.cs" and "Rocket.cs" scripts.
	/// </summary>
	[AddComponentMenu("Unturned/Explosion Spawner")]
	public class ExplosionSpawner : MonoBehaviour
	{
		public float DamageRadius;
		public EDeathCause Cause = EDeathCause.GRENADE;

		/// <summary>
		/// Zombie explosion types have slight variations e.g. lighting zombies on fire.
		/// </summary>
		public EExplosionDamageType DamageType = EExplosionDamageType.CONVENTIONAL;

		/// <summary>
		/// If greater than -0.5, overrides default radius zombies and animals will be alerted within.
		/// </summary>
		public float AlertRadiusOverride = -1.0f;

		/// <summary>
		/// If true, per-surface effects like blood splatter are created.
		/// </summary>
		public bool PlayImpactEffects = true;

		/// <summary>
		/// If true, explosion damage passes through Barricades and Structures.
		/// </summary>
		public bool PenetrateBuildables = true;

		public float PlayerDamage;
		public float ZombieDamage;
		public float AnimalDamage;
		public float BarricadeDamage;
		public float StructureDamage;
		public float VehicleDamage;
		public float ResourceDamage;
		public float ObjectDamage;

		/// <summary>
		/// Speed to launch players away from blast position.
		/// </summary>
		public float LaunchSpeed;

		public void Explode()
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

			ExplosionParameters explosionParameters = new ExplosionParameters(transform.position, DamageRadius, Cause, CSteamID.Nil);
			explosionParameters.damageType = DamageType;

			if (AlertRadiusOverride > -0.5f)
			{
				explosionParameters.alertRadius = AlertRadiusOverride;
			}

			explosionParameters.playImpactEffect = PlayImpactEffects;
			explosionParameters.penetrateBuildables = PenetrateBuildables;

			explosionParameters.playerDamage = PlayerDamage;
			explosionParameters.zombieDamage = ZombieDamage;
			explosionParameters.animalDamage = AnimalDamage;
			explosionParameters.barricadeDamage = BarricadeDamage;
			explosionParameters.structureDamage = StructureDamage;
			explosionParameters.vehicleDamage = VehicleDamage;
			explosionParameters.resourceDamage = ResourceDamage;
			explosionParameters.objectDamage = ObjectDamage;
			explosionParameters.damageOrigin = EDamageOrigin.ExplosionSpawnerComponent;
			explosionParameters.launchSpeed = LaunchSpeed;
			DamageTool.explode(explosionParameters, out List<EPlayerKill> _kills);
#endif // GAME
		}
	}
}
