////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ZombieDamageMultiplier : IDamageMultiplier
	{

		public float damage;


		public float leg;


		public float arm;


		public float spine;


		public float skull;

		public float multiply(ELimb limb)
		{
			switch (limb)
			{
				case ELimb.LEFT_FOOT:
					return damage * leg;
				case ELimb.LEFT_LEG:
					return damage * leg;
				case ELimb.RIGHT_FOOT:
					return damage * leg;
				case ELimb.RIGHT_LEG:
					return damage * leg;
				case ELimb.LEFT_HAND:
					return damage * arm;
				case ELimb.LEFT_ARM:
					return damage * arm;
				case ELimb.RIGHT_HAND:
					return damage * arm;
				case ELimb.RIGHT_ARM:
					return damage * arm;
				case ELimb.SPINE:
					return damage * spine;
				case ELimb.SKULL:
					return damage * skull;
			}

			return damage;
		}

		public ZombieDamageMultiplier(float newDamage, float newLeg, float newArm, float newSpine, float newSkull)
		{
			damage = newDamage;
			leg = newLeg;
			arm = newArm;
			spine = newSpine;
			skull = newSkull;
		}
	}
}
