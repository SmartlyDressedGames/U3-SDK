////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Payload for the DamageTool.damageZombie function.
	/// </summary>
	public struct DamageZombieParameters
	{
		public DamageZombieParameters(Zombie zombie, Vector3 direction, float damage)
		{
			this.zombie = zombie;
			this.direction = direction;
			this.damage = damage;

			respectArmor = false;
			allowBackstab = false;
			applyGlobalArmorMultiplier = true;
			limb = ELimb.SPINE;

			times = 1;
			zombieStunOverride = EZombieStunOverride.None;
			ragdollEffect = ERagdollEffect.None;
			RagdollForceMultiplier = 1.0f;

			AlertPosition = null;
			instigator = null;
		}

		public Zombie zombie;
		public Vector3 direction;
		public float damage;

		public bool respectArmor;

		/// <summary>
		/// Should game mode config damage multiplier be factored in?
		/// </summary>
		public bool applyGlobalArmorMultiplier;

		public bool allowBackstab;
		public ELimb limb;

		/// <summary>
		/// Equivalent to the "armor" parameter of the legacy damage function.
		/// </summary>
		public bool legacyArmor
		{
			set
			{
				respectArmor = value;
				allowBackstab = value;
			}
		}

		public float times;
		public EZombieStunOverride zombieStunOverride;
		public ERagdollEffect ragdollEffect;

		/// <summary>
		/// Defaults to 1.
		/// </summary>
		public float RagdollForceMultiplier
		{
			get;
			set;
		}

		/// <summary>
		/// If not null and damage is applied, <see cref="Zombie.alert"/> is called with this position (startle: true).
		/// </summary>
		public Vector3? AlertPosition
		{
			get;
			set;
		}

		public object instigator;

		public static DamageZombieParameters makeInstakill(Zombie zombie)
		{
			return new DamageZombieParameters(zombie, Vector3.up, 65000)
			{
				applyGlobalArmorMultiplier = false,
			};
		}

		public static DamageZombieParameters make(Zombie zombie, Vector3 direction, IDamageMultiplier multiplier, ELimb limb)
		{
			float limbDamage = multiplier.multiply(limb);
			DamageZombieParameters parameters = new DamageZombieParameters(zombie, direction, limbDamage);
			parameters.limb = limb;
			return parameters;
		}
	}
}
