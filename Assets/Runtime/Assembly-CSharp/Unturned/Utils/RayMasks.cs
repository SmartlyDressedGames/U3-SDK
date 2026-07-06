////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	[Flags]
	public enum ERayMask
	{
		DEFAULT = 1 << ELayerMask.DEFAULT,
		TRANSPARENT_FX = 1 << ELayerMask.TRANSPARENT_FX,
		IGNORE_RAYCAST = 1 << ELayerMask.IGNORE_RAYCAST,
		BUILTIN_3 = 1 << ELayerMask.BUILTIN_3,
		WATER = 1 << ELayerMask.WATER,
		UI = 1 << ELayerMask.UI,
		BUILTIN_6 = 1 << ELayerMask.BUILTIN_6,
		BUILTIN_7 = 1 << ELayerMask.BUILTIN_7,
		LOGIC = 1 << ELayerMask.LOGIC,
		PLAYER = 1 << ELayerMask.PLAYER,
		ENEMY = 1 << ELayerMask.ENEMY,
		VIEWMODEL = 1 << ELayerMask.VIEWMODEL,
		DEBRIS = 1 << ELayerMask.DEBRIS,
		ITEM = 1 << ELayerMask.ITEM,
		RESOURCE = 1 << ELayerMask.RESOURCE,
		LARGE = 1 << ELayerMask.LARGE,
		MEDIUM = 1 << ELayerMask.MEDIUM,
		SMALL = 1 << ELayerMask.SMALL,
		SKY = 1 << ELayerMask.SKY,
		ENVIRONMENT = 1 << ELayerMask.ENVIRONMENT,
		GROUND = 1 << ELayerMask.GROUND,
		CLIP = 1 << ELayerMask.CLIP,
		NAVMESH = 1 << ELayerMask.NAVMESH,
		ENTITY = 1 << ELayerMask.ENTITY,
		AGENT = 1 << ELayerMask.AGENT,
		LADDER = 1 << ELayerMask.LADDER,
		VEHICLE = 1 << ELayerMask.VEHICLE,
		BARRICADE = 1 << ELayerMask.BARRICADE,
		STRUCTURE = 1 << ELayerMask.STRUCTURE,
		TIRE = 1 << ELayerMask.TIRE,
		TRAP = 1 << ELayerMask.TRAP,
		GROUND2 = 1 << ELayerMask.GROUND2
	}

	public class RayMasks
	{
		public const int DEFAULT = 1 << LayerMasks.DEFAULT;
		public const int TRANSPARENT_FX = 1 << LayerMasks.TRANSPARENT_FX;
		public const int IGNORE_RAYCAST = 1 << LayerMasks.IGNORE_RAYCAST;
		public const int WATER = 1 << LayerMasks.WATER;
		public const int UI = 1 << LayerMasks.UI;
		public const int LOGIC = 1 << LayerMasks.LOGIC;
		public const int PLAYER = 1 << LayerMasks.PLAYER;
		public const int ENEMY = 1 << LayerMasks.ENEMY;
		public const int VIEWMODEL = 1 << LayerMasks.VIEWMODEL;
		public const int DEBRIS = 1 << LayerMasks.DEBRIS;
		public const int ITEM = 1 << LayerMasks.ITEM;
		public const int RESOURCE = 1 << LayerMasks.RESOURCE;
		public const int LARGE = 1 << LayerMasks.LARGE;
		public const int MEDIUM = 1 << LayerMasks.MEDIUM;
		public const int SMALL = 1 << LayerMasks.SMALL;
		public const int SKY = 1 << LayerMasks.SKY;
		public const int ENVIRONMENT = 1 << LayerMasks.ENVIRONMENT;
		public const int GROUND = 1 << LayerMasks.GROUND;
		public const int CLIP = 1 << LayerMasks.CLIP;
		public const int NAVMESH = 1 << LayerMasks.NAVMESH;
		public const int ENTITY = 1 << LayerMasks.ENTITY;
		public const int AGENT = 1 << LayerMasks.AGENT;
		public const int LADDER = 1 << LayerMasks.LADDER;
		public const int VEHICLE = 1 << LayerMasks.VEHICLE;
		public const int BARRICADE = 1 << LayerMasks.BARRICADE;
		public const int STRUCTURE = 1 << LayerMasks.STRUCTURE;
		public const int TIRE = 1 << LayerMasks.TIRE;
		public const int TRAP = 1 << LayerMasks.TRAP;
		public const int GROUND2 = 1 << LayerMasks.GROUND2;
		public const int ALL = ~0;

		public static readonly int REFLECTION = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND;
		public static readonly int CHART = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND;
		public static readonly int FOLIAGE_FOCUS = GROUND | GROUND2 | LARGE | MEDIUM;

		public static readonly int POWER_INTERACT = BARRICADE | LARGE | MEDIUM | SMALL | RESOURCE;
		public static readonly int BARRICADE_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE | VEHICLE;
		public static readonly int STRUCTURE_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | STRUCTURE;
		public static readonly int ROOFS_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | STRUCTURE | GROUND2;
		public static readonly int CORNERS_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | STRUCTURE | SKY;
		public static readonly int WALLS_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | STRUCTURE | UI;
		public static readonly int LADDERS_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | STRUCTURE | VEHICLE | TRANSPARENT_FX;
		public static readonly int SLOTS_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | LADDER | VEHICLE | BARRICADE | STRUCTURE | LOGIC;
		public static readonly int LADDER_INTERACT = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | LADDER | VEHICLE | BARRICADE | STRUCTURE;
		public static readonly int CLOTHING_INTERACT = PLAYER | ENEMY | ITEM;
		public static readonly int PLAYER_INTERACT = ENEMY | ITEM | RESOURCE | LARGE | MEDIUM | SMALL | ENVIRONMENT | GROUND | VEHICLE | BARRICADE | STRUCTURE | LADDER | DEFAULT;
		public static readonly int EDITOR_INTERACT = LARGE | MEDIUM | SMALL | BARRICADE | STRUCTURE;
		public static readonly int EDITOR_WORLD = RESOURCE | LARGE | MEDIUM | SMALL | GROUND | GROUND2 | BARRICADE | STRUCTURE;
		public static readonly int EDITOR_LOGIC = LOGIC | SKY;
		public static readonly int EDITOR_VR = RESOURCE | LARGE | MEDIUM | GROUND | GROUND2;
		public static readonly int EDITOR_BUILDABLE = RESOURCE | BARRICADE | STRUCTURE;

		public static readonly int BLOCK_LADDER = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | VEHICLE | BARRICADE | STRUCTURE | CLIP;
		public static readonly int BLOCK_PICKUP = MEDIUM | LARGE | BARRICADE | STRUCTURE;
		public static readonly int BLOCK_LASER = ENEMY | ITEM | RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | ENTITY | VEHICLE | BARRICADE | STRUCTURE;
		public static readonly int BLOCK_RESOURCE = RESOURCE | LARGE | MEDIUM | ENVIRONMENT;
		public static readonly int BLOCK_ITEM = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE; // what items will fall directly to when dropped
		public static readonly int BLOCK_VEHICLE = LARGE | MEDIUM | ENVIRONMENT | GROUND;

		/// <summary>
		/// Used to test whether player can fit in a space.
		/// Includes terrain because tested capsule could be slightly underground, and clip to prevent exploits at sky limit.
		/// </summary>
		public static readonly int BLOCK_STANCE = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | VEHICLE | BARRICADE | STRUCTURE | GROUND | CLIP;

		public static readonly int BLOCK_NAVMESH = NAVMESH | RESOURCE | ENVIRONMENT;
		public static readonly int BLOCK_KILLCAM = LARGE | RESOURCE | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE | VEHICLE;
		public static readonly int BLOCK_PLAYERCAM = LARGE | RESOURCE | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE | VEHICLE;
		public static readonly int BLOCK_PLAYERCAM_1P = LARGE | RESOURCE | ENVIRONMENT | BARRICADE | STRUCTURE | VEHICLE; // Removes ground because we use spherecast, and terrain cannot curve outward.

		/// <summary>
		/// Used for third-person camera in vehicle.
		/// Does not include resource layer because attached barricades are put on that layer.
		/// Barricades layer itself is included to prevent looking inside player bases.
		/// </summary>
		public static readonly int BLOCK_VEHICLECAM = LARGE | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE;

		public static readonly int BLOCK_VISION = LARGE | MEDIUM;
		public static readonly int BLOCK_COLLISION = CLIP | GROUND | ENVIRONMENT | MEDIUM | LARGE | RESOURCE | VEHICLE | BARRICADE | STRUCTURE;
		public static readonly int BLOCK_GRASS = LARGE | MEDIUM | ENVIRONMENT;
		public static readonly int BLOCK_LEAN = BLOCK_STANCE;

		/// <summary>
		/// Used to test whether player can enter a vehicle.
		/// Does not include resource layer because attached barricades are put on that layer.
		/// </summary>
		public static readonly int BLOCK_ENTRY = CLIP | LARGE | MEDIUM | GROUND | BARRICADE | STRUCTURE;

		public static readonly int BLOCK_EXIT = CLIP | LARGE | MEDIUM | ENVIRONMENT | RESOURCE | GROUND | BARRICADE | STRUCTURE;
		public static readonly int BLOCK_EXIT_FIND_GROUND = CLIP | LARGE | MEDIUM | ENVIRONMENT | RESOURCE | GROUND | BARRICADE | STRUCTURE | VEHICLE;
		public static readonly int BLOCK_BARRICADE_INTERACT_LOS = LARGE | MEDIUM | STRUCTURE;
		public static readonly int BLOCK_TIRE = CLIP | GROUND | ENVIRONMENT | MEDIUM | LARGE | RESOURCE;
		public static readonly int BLOCK_BARRICADE = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | BARRICADE | STRUCTURE | PLAYER | ENEMY;
		public static readonly int BLOCK_DOOR_OPENING = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | BARRICADE | STRUCTURE | PLAYER | ENEMY;
		public static readonly int BLOCK_BED_LOS = BLOCK_ITEM;

		[System.Obsolete]
		public static readonly int BLOCK_STRUCTURE = VEHICLE | BARRICADE | STRUCTURE | PLAYER | ENEMY;

		public static readonly int BLOCK_EXPLOSION = LARGE | MEDIUM | GROUND | BARRICADE | STRUCTURE | RESOURCE;
		public static readonly int BLOCK_EXPLOSION_PENETRATE_BUILDABLES = LARGE | MEDIUM | GROUND | RESOURCE;
		public static readonly int BLOCK_WIND = LARGE | BARRICADE | STRUCTURE;
		public static readonly int BLOCK_FRAME = BARRICADE | IGNORE_RAYCAST;
		public static readonly int BLOCK_WINDOW = BARRICADE | IGNORE_RAYCAST;
		public static readonly int BLOCK_SENTRY = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | VEHICLE | BARRICADE | STRUCTURE;
		public static readonly int BLOCK_CHAR_BUILDABLE_OVERLAP = PLAYER; // kept separate incase enemy is considered, but the local player doesn't know about their own enemy size (boxes disabled)
		public static readonly int BLOCK_CHAR_BUILDABLE_OVERLAP_NOT_ON_VEHICLE = PLAYER | VEHICLE;
		public static readonly int BLOCK_CHAR_HINGE_OVERLAP = PLAYER | VEHICLE;
		public static readonly int BLOCK_CHAR_HINGE_OVERLAP_ON_VEHICLE = PLAYER;
		public static readonly int BLOCK_TRAIN = LARGE | MEDIUM | BARRICADE | STRUCTURE | VEHICLE;
		public static readonly int WAYPOINT = LARGE | MEDIUM | RESOURCE | BARRICADE | STRUCTURE | ENVIRONMENT | GROUND;

		//public static readonly int DAMAGE_WORLD = ENEMY | ZOMBIE | RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | VEHICLE | BARRICADE | STRUCTURE;
		public static readonly int DAMAGE_PHYSICS = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | VEHICLE | BARRICADE | STRUCTURE;
		public static readonly int DAMAGE_CLIENT = ENEMY | ENTITY | RESOURCE | LARGE | MEDIUM | SMALL | ENVIRONMENT | GROUND | VEHICLE | BARRICADE | STRUCTURE;
		public static readonly int DAMAGE_SERVER = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE; // RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE;
		public static readonly int DAMAGE_ZOMBIE = VEHICLE | BARRICADE | STRUCTURE;
		//public static readonly int DAMAGE_EXPLOSION = PLAYER |  AGENT | BARRICADE | STRUCTURE | VEHICLE | RESOURCE;

		[System.Obsolete("Replaced by EFFECT_SPLATTER to make const")]
		public static readonly int SPLATTER = LARGE | MEDIUM | ENVIRONMENT | GROUND;

		/// <summary>
		/// 2023-02-02: adding more layers since splatter can be attached to them now.
		/// parent should only be set if that system also calls ClearAttachments, otherwise attachedEffects will leak memory.
		/// </summary>
		public const int EFFECT_SPLATTER = LARGE | MEDIUM | ENVIRONMENT | GROUND | BARRICADE | STRUCTURE | VEHICLE;

		/// <summary>
		/// Layer mask for CharacterController overlap test.
		/// </summary>
		public const int CHARACTER_CONTROLLER_MOVE = RESOURCE | LARGE | MEDIUM | ENVIRONMENT | GROUND | CLIP | BARRICADE | STRUCTURE;

		/// <summary>
		/// Layer mask for CharacterController overlap test while inside landscape hole volume.
		/// </summary>
		public const int CHARACTER_CONTROLLER_MOVE_IGNORE_GROUND = CHARACTER_CONTROLLER_MOVE & ~GROUND;

		/// <summary>
		/// Lightning strike raycasts from sky to ground using this layer mask.
		/// </summary>
		public const int LIGHTNING = LARGE | MEDIUM | RESOURCE | VEHICLE | BARRICADE | STRUCTURE | ENVIRONMENT | GROUND;
	}
}
