////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class AnimalDamageMultiplier : IDamageMultiplier
	{
		public static readonly float MULTIPLIER_EASY = 1.25f;
		public static readonly float MULTIPLIER_HARD = 0.75f;


		public float damage;


		public float leg;


		public float spine;


		public float skull;

		public float multiply(ELimb limb)
		{
			switch (limb)
			{
				case ELimb.LEFT_BACK:
					return damage * leg;
				case ELimb.RIGHT_BACK:
					return damage * leg;
				case ELimb.LEFT_FRONT:
					return damage * leg;
				case ELimb.RIGHT_FRONT:
					return damage * leg;
				case ELimb.SPINE:
					return damage * spine;
				case ELimb.SKULL:
					return damage * skull;
			}

			return damage;
		}

		public AnimalDamageMultiplier(float newDamage, float newLeg, float newSpine, float newSkull)
		{
			damage = newDamage;
			leg = newLeg;
			spine = newSpine;
			skull = newSkull;
		}
	}
}
