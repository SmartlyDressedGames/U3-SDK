////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Payload for the DamageTool.damageAnimal function.
	/// </summary>
	public struct DamageAnimalParameters
	{
		public DamageAnimalParameters(Animal animal, Vector3 direction, float damage)
		{
			this.animal = animal;
			this.direction = direction;
			this.damage = damage;

			applyGlobalArmorMultiplier = true;
			limb = ELimb.SPINE;
			times = 1.0f;
			ragdollEffect = ERagdollEffect.None;

			AlertPosition = null;
			instigator = null;
		}

		public Animal animal;
		public Vector3 direction;
		public float damage;

		/// <summary>
		/// Should game mode config damage multiplier be factored in?
		/// </summary>
		public bool applyGlobalArmorMultiplier;

		public ELimb limb;
		public float times;
		public ERagdollEffect ragdollEffect;

		/// <summary>
		/// If not null and damage is applied, <see cref="Animal.alertDamagedFromPoint"/> is called with this position.
		/// </summary>
		public Vector3? AlertPosition
		{
			get;
			set;
		}

		/// <summary>
		/// Object responsible for creating this AnimalDamageParameters.
		/// However, can be null if calling code didn't assign one.
		/// Example types as of 2025-10-30:
		/// - Kill Volume
		/// - Bumper (vehicle impact)
		/// - Interactable Sentry
		/// - Interactable Trap
		/// - Barrier (legacy per-object kill volume)
		/// - Player Equipment (punch)
		/// - Useable Gun
		/// - Useable Melee
		/// </summary>
		public object instigator;

		public static DamageAnimalParameters makeInstakill(Animal animal)
		{
			return new DamageAnimalParameters(animal, Vector3.up, 65000)
			{
				applyGlobalArmorMultiplier = false,
			};
		}

		public static DamageAnimalParameters make(Animal animal, Vector3 direction, IDamageMultiplier multiplier, ELimb limb)
		{
			float limbDamage = multiplier.multiply(limb);
			DamageAnimalParameters parameters = new DamageAnimalParameters(animal, direction, limbDamage);
			parameters.limb = limb;
			return parameters;
		}
	}
}
