////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Currently used by plugins to identify what damaged a buildable.
	/// </summary>
	public enum EDamageOrigin
	{
		Unknown,
		Mega_Zombie_Boulder,
		Vehicle_Bumper,
		Horde_Beacon_Self_Destruct, // Self-destructs when all players or zombies are dead
		Trap_Wear_And_Tear, // Trap self-damages with every hit
		Carepackage_Timeout,
		Plant_Harvested,
		Charge_Self_Destruct, // Self-destructs in-case the explosion damage doesn't destroy itself
		Zombie_Swipe,
		Grenade_Explosion,
		Rocket_Explosion,
		Food_Explosion,
		Vehicle_Explosion,
		Charge_Explosion,
		Trap_Explosion,
		Bullet_Explosion,
		Radioactive_Zombie_Explosion,
		Flamable_Zombie_Explosion,
		Zombie_Electric_Shock,
		Zombie_Stomp,
		Zombie_Fire_Breath,
		Sentry,
		Useable_Gun,
		Useable_Melee,
		Punch,
		Animal_Attack,
		Kill_Volume,
		Vehicle_Collision_Self_Damage, // Bumper damages itself during collison.
		Lightning,
		VehicleDecay, // Take damage after being neglected for a long time.
		/// <summary>
		/// Explosion instigated by <see cref="ExplosionSpawner"/>.
		/// </summary>
		ExplosionSpawnerComponent,
	}
}
