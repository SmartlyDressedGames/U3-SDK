////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if GAME
using SDG.NetPak;
#endif

namespace SDG.Unturned
{
#if GAME
	[NetEnum]
#endif
	public enum EDeathCause
	{
		BLEEDING,
		BONES,
		FREEZING,
		BURNING,
		FOOD,
		WATER,
		GUN,
		MELEE,
		ZOMBIE,
		ANIMAL,
		SUICIDE,
		KILL,
		INFECTION,
		PUNCH,
		BREATH,
		ROADKILL,
		VEHICLE,
		GRENADE,
		SHRED,
		LANDMINE,
		ARENA,
		MISSILE,
		CHARGE,
		SPLASH,
		SENTRY,
		ACID,
		BOULDER,
		BURNER,
		SPIT,
		SPARK
	}
}
