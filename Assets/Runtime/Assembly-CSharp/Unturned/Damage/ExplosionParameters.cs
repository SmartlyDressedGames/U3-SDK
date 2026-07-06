////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Payload for the DamageTool.explode function.
	/// Moved into struct because the amount of arguments to that function were insane, but now is not the time to completely refactor damage.
	/// </summary>
	public struct ExplosionParameters
	{
		public ExplosionParameters(Vector3 point, float damageRadius, EDeathCause cause, CSteamID killer)
		{
			this.point = point;
			this.damageRadius = damageRadius;
			this.cause = cause;
			this.killer = killer;

			damageType = EExplosionDamageType.CONVENTIONAL;
			alertRadius = 32.0f;
			playImpactEffect = true;
			penetrateBuildables = false;
			damageOrigin = EDamageOrigin.Unknown;
			ragdollEffect = ERagdollEffect.None;

			playerDamage = 0.0f;
			zombieDamage = 0.0f;
			animalDamage = 0.0f;
			barricadeDamage = 0.0f;
			structureDamage = 0.0f;
			vehicleDamage = 0.0f;
			resourceDamage = 0.0f;
			objectDamage = 0.0f;
			launchSpeed = 0.0f;
		}

		public ExplosionParameters(Vector3 point, float damageRadius, EDeathCause cause)
			: this(point, damageRadius, cause, CSteamID.Nil)
		{ }

		public Vector3 point;
		public float damageRadius;
		public EDeathCause cause;
		public CSteamID killer;

		public EExplosionDamageType damageType;
		public float alertRadius;
		public bool playImpactEffect;
		public bool penetrateBuildables;
		public EDamageOrigin damageOrigin;
		public ERagdollEffect ragdollEffect;

		public float playerDamage;
		public float zombieDamage;
		public float animalDamage;
		public float barricadeDamage;
		public float structureDamage;
		public float vehicleDamage;
		public float resourceDamage;
		public float objectDamage;

		/// <summary>
		/// Speed to launch players away from blast position.
		/// </summary>
		public float launchSpeed;
	}
}
