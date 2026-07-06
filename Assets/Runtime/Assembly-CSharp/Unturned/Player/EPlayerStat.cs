////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	[NetEnum]
	public enum EPlayerStat
	{
		NONE,                  // 0
		KILLS_ZOMBIES_NORMAL,   // 1
		KILLS_PLAYERS,        // 2
		FOUND_ITEMS,            // 3
		FOUND_RESOURCES,        // 4
		FOUND_EXPERIENCE,      // 5
		KILLS_ZOMBIES_MEGA,  // 6
		DEATHS_PLAYERS,      // 7
		KILLS_ANIMALS,        // 8
		FOUND_CRAFTS,          // 9
		FOUND_FISHES,          // 10
		FOUND_PLANTS,          // 11
		ACCURACY,              // 12
		HEADSHOTS,            // 13
		TRAVEL_FOOT,            // 14
		TRAVEL_VEHICLE,      // 15
		ARENA_WINS,          // 16
		FOUND_BUILDABLES,      // 17
		FOUND_THROWABLES        // 18
	}
}
