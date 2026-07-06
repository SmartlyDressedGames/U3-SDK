////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	[NetEnum]
	public enum EPlayerMessage // REMINDER: ADD NEW ONES TO THE BOTTOM OR TUTORIAL PROBABLY BREAKS
	{
		NONE,
		SPACE,
		ITEM,
		VEHICLE_ENTER,
		VEHICLE_EXIT,
		VEHICLE_SWAP,
		RELOAD,
		SAFETY,
		LIGHT,
		LASER,
		RANGEFINDER,
		ENEMY,
		DOOR_OPEN,
		DOOR_CLOSE,
		LOCKED,
		BLOCKED,
		PILLAR,
		POST,
		ROOF,
		WALL,
		CORNER,
		GROUND,
		DOORWAY,
		GARAGE,
		WINDOW,
		BED_ON,
		BED_OFF,
		BED_CLAIMED,
		BOUNDS,
		EXPERIENCE,
		STORAGE,
		FARM,
		GROW,
		SOIL,
		FIRE_ON,
		FIRE_OFF,
		FORAGE,
		GENERATOR_ON,
		GENERATOR_OFF,
		SPOT_ON,
		SPOT_OFF,
		EMPTY,
		FULL,
		MOON_ON,
		MOON_OFF,
		SAFEZONE_ON,
		SAFEZONE_OFF,
		PURCHASE,
		WAVE_ON,
		WAVE_OFF,
		POWER,
		USE,
		SALVAGE,
		TUTORIAL_MOVE,
		TUTORIAL_LOOK,
		TUTORIAL_JUMP,
		TUTORIAL_PERSPECTIVE,
		TUTORIAL_RUN,
		TUTORIAL_INVENTORY,
		TUTORIAL_SURVIVAL,
		TUTORIAL_GUN,
		TUTORIAL_LADDER,
		TUTORIAL_CRAFT,
		TUTORIAL_SKILLS,
		TUTORIAL_SWIM,
		TUTORIAL_MEDICAL,
		TUTORIAL_VEHICLE,
		TUTORIAL_CROUCH,
		TUTORIAL_PRONE,
		TUTORIAL_EDUCATED,
		TUTORIAL_HARVEST,
		TUTORIAL_FISH,
		TUTORIAL_BUILD,
		TUTORIAL_HORN,
		TUTORIAL_LIGHTS,
		TUTORIAL_SIRENS,
		TUTORIAL_FARM,
		TUTORIAL_POWER,
		TUTORIAL_FIRE,
		CLAIM,
		DEADZONE_ON,
		DEADZONE_OFF,
		UNDERWATER,
		NAV,
		SPAWN,
		MOBILE,
		OIL,
		VOLUME_WATER,
		VOLUME_FUEL,
		BUSY,
		TRAPDOOR,
		FUEL,
		CLEAN,
		SALTY,
		DIRTY,
		TALK,
		REPUTATION,
		CONDITION,

		/// <summary>
		/// Poorly named. Specific to InteractableObjectQuest.
		/// </summary>
		INTERACT,

		SAFEZONE,
		BAYONET,
		VEHICLE_LOCKED,
		VEHICLE_UNLOCKED,

		/// <summary>
		/// Directly uses input string for custom message popups.
		/// </summary>
		NPC_CUSTOM,

		/// <summary>
		/// Player cannot build on a vehicle with occupied seats.
		/// </summary>
		BUILD_ON_OCCUPIED_VEHICLE,

		/// <summary>
		/// Horde beacon cannot be built here.
		/// </summary>
		NOT_ALLOWED_HERE,

		/// <summary>
		/// Item type is not allowed on vehicles.
		/// </summary>
		CANNOT_BUILD_ON_VEHICLE,

		/// <summary>
		/// Item must be placed closer to vehicle hull.
		/// </summary>
		TOO_FAR_FROM_HULL,

		/// <summary>
		/// Player cannot build while seated in a vehicle because some vehicles are abusable to stick the camera through a wall.
		/// </summary>
		CANNOT_BUILD_WHILE_SEATED,

		/// <summary>
		/// Interacting with ladder.
		/// </summary>
		CLIMB,

		/// <summary>
		/// Popup when equipping housing planner "press T to show items"
		/// </summary>
		HOUSING_PLANNER_TUTORIAL,

		/// <summary>
		/// Popup when structure is blocked by something named we can format into the message.
		/// </summary>
		PLACEMENT_OBSTRUCTED_BY,

		/// <summary>
		/// Notice that freeform buildables are blocked by <see cref="GameplayConfigData.Allow_Freeform_Buildables">Allow_Freeform_Buildables</see>.
		/// </summary>
		FREEFORM_BUILDABLE_NOT_ALLOWED,

		/// <summary>
		/// Popup when structure is blocked by terrain.
		/// </summary>
		PLACEMENT_OBSTRUCTED_BY_GROUND,

		/// <summary>
		/// Vehicle doesn't support spray paints.
		/// </summary>
		NOT_PAINTABLE,

		/// <summary>
		/// Explaining that some blueprints require workstations.
		/// </summary>
		TUTORIAL_WORKSTATION,

		/// <summary>
		/// Barricades are allowed here but not structures.
		/// </summary>
		INSIDE_NO_STRUCTURES_VOLUME,

		/// <summary>
		/// Alternative to VOLUME_WATER or VOLUME_FUEL shown when object is destroyed.
		/// </summary>
		VOLUME_DESTROYED,
	}
}
