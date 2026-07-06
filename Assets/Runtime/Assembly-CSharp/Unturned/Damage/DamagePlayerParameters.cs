////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Payload for the DamageTool.damagePlayer function.
	/// </summary>
	public struct DamagePlayerParameters
	{
		public DamagePlayerParameters(Player player)
		{
			this.player = player;
			cause = EDeathCause.SUICIDE;
			limb = ELimb.SPINE;
			killer = Steamworks.CSteamID.Nil;
			direction = Vector3.up;
			damage = 0.0f;
			times = 1.0f;
			respectArmor = false;
			applyGlobalArmorMultiplier = true;
			trackKill = false;
			ragdollEffect = ERagdollEffect.None;
			bleedingModifier = Bleeding.Default;
			bonesModifier = Bones.None;
			foodModifier = 0;
			waterModifier = 0;
			virusModifier = 0;
			hallucinationModifier = 0;
		}

		public Player player;
		public EDeathCause cause;
		public ELimb limb;
		public Steamworks.CSteamID killer;
		public Vector3 direction;
		public float damage;
		public float times;

		/// <summary>
		/// Should armor worn on matching limb be factored in?
		/// </summary>
		public bool respectArmor;

		/// <summary>
		/// Should game mode config damage multiplier be factored in?
		/// </summary>
		public bool applyGlobalArmorMultiplier;

		/// <summary>
		/// If player dies should it count towards quests?
		/// </summary>
		public bool trackKill;

		/// <summary>
		/// Effect to apply to ragdoll if dead.
		/// </summary>
		public ERagdollEffect ragdollEffect;

		public enum Bleeding
		{
			Default,
			Always,
			Never,
			Heal,
		}

		public Bleeding bleedingModifier;

		public enum Bones
		{
			None,
			Always,
			Heal
		}

		public Bones bonesModifier;

		public float foodModifier;
		public float waterModifier;
		public float virusModifier;
		public float hallucinationModifier;

		public static DamagePlayerParameters make(Player player, EDeathCause cause, Vector3 direction, IDamageMultiplier multiplier, ELimb limb)
		{
			DamagePlayerParameters parameters = new DamagePlayerParameters(player);
			parameters.cause = cause;
			parameters.limb = limb;
			parameters.direction = direction;
			parameters.damage = multiplier.multiply(limb);
			return parameters;
		}
	}
}
