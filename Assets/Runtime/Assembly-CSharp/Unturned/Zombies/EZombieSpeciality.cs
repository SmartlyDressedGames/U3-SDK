////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	[NetEnum]
	public enum EZombieSpeciality
	{
		NONE,
		NORMAL,
		MEGA,
		CRAWLER,
		SPRINTER,
		FLANKER_FRIENDLY,
		FLANKER_STALK,
		BURNER,
		ACID,
		BOSS_ELECTRIC,
		BOSS_WIND,
		BOSS_FIRE,
		BOSS_ALL,
		BOSS_MAGMA,
		SPIRIT,
		BOSS_SPIRIT,
		BOSS_NUCLEAR,

		/// <summary>
		/// Crossover from Dying Light. Only spawns during night. Explodes into fire at dawn.
		/// </summary>
		DL_RED_VOLATILE,

		/// <summary>
		/// Crossover from Dying Light. Only spawns during night. Explodes into fire at dawn.
		/// </summary>
		DL_BLUE_VOLATILE,

		/// <summary>
		/// Elver endgame boss with reduced bullet damage and wind zombie stomping attacks.
		/// </summary>
		BOSS_ELVER_STOMPER,

		/// <summary>
		/// Kuwait final boss with increased rock throwing, damage players inside vehicle (turrets), and flashbangs.
		/// </summary>
		BOSS_KUWAIT,

		/// <summary>
		/// Buak boss types have a red-eyed flashbang effect.
		/// </summary>
		BOSS_BUAK_ELECTRIC,
		BOSS_BUAK_WIND,
		BOSS_BUAK_FIRE,
		BOSS_BUAK_FINAL,

		// Nelson 2025-09-25: if changing number of specialities, ZombieDifficultyAsset health override
		// parsing needs to be updated. :P
	}

	public static class ZombieSpecialityExtension
	{
		/// <summary>
		/// Is this one of the Dying Light volatile zombies? Only spawns during night. Explodes into fire at dawn.
		/// </summary>
		public static bool IsDLVolatile(this EZombieSpeciality speciality)
		{
			return (speciality == EZombieSpeciality.DL_RED_VOLATILE) | (speciality == EZombieSpeciality.DL_BLUE_VOLATILE);
		}

		/// <summary>
		/// Does this have the BOSS_* prefix?
		/// </summary>
		public static bool IsBoss(this EZombieSpeciality speciality)
		{
			switch (speciality)
			{
				case EZombieSpeciality.BOSS_ELECTRIC:
				case EZombieSpeciality.BOSS_WIND:
				case EZombieSpeciality.BOSS_FIRE:
				case EZombieSpeciality.BOSS_MAGMA:
				case EZombieSpeciality.BOSS_SPIRIT:
				case EZombieSpeciality.BOSS_NUCLEAR:
				case EZombieSpeciality.BOSS_ELVER_STOMPER:
				case EZombieSpeciality.BOSS_KUWAIT:
				case EZombieSpeciality.BOSS_BUAK_FIRE:
				case EZombieSpeciality.BOSS_BUAK_ELECTRIC:
				case EZombieSpeciality.BOSS_BUAK_WIND:
				case EZombieSpeciality.BOSS_BUAK_FINAL:
					return true;

				default:
					return false;
			}
		}

		public static bool IsFromBuakMap(this EZombieSpeciality speciality)
		{
			switch (speciality)
			{
				case EZombieSpeciality.BOSS_BUAK_FIRE:
				case EZombieSpeciality.BOSS_BUAK_ELECTRIC:
				case EZombieSpeciality.BOSS_BUAK_WIND:
				case EZombieSpeciality.BOSS_BUAK_FINAL:
					return true;

				default:
					return false;
			}
		}
	}
}
