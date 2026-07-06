////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class NPCZombieKillsCondition : INPCCondition
	{
		public ushort id
		{
			get;
			protected set;
		}

		public short value
		{
			get;
			protected set;
		}

		public EZombieSpeciality zombie
		{
			get;
			protected set;
		}

		/// <summary>
		/// Should zombie(s) of the required type be spawned when player enters the area?
		/// </summary>
		public bool spawn
		{
			get;
			protected set;
		}

		/// <summary>
		/// How many to spawn if spawning <see cref="spawn"/> is enabled.
		/// </summary>
		public int spawnQuantity
		{
			get;
			protected set;
		}

		/// <summary>
		/// If greater than zero, find this zombie type configured in the level editor. For example, if the level editor
		/// lists "0 Fire (4)", then 4 is the unique ID, and if assigned to this condition a zombie from the "Fire"
		/// table will spawn.
		/// </summary>
		public int LevelTableUniqueId
		{
			get;
			private set;
		}

		/// <summary>
		/// Navmesh index player must be within. If set to byte.MaxValue then anywhere on the map is eligible.
		/// </summary>
		public byte nav
		{
			get;
			protected set;
		}

		/// <summary>
		/// Only kills within this radius around the player are tracked.
		/// </summary>
		public float sqrRadius
		{
			get;
			protected set;
		}

		/// <summary>
		/// Only kills outside this radius around the player are tracked.
		/// Useful for quests incentivizing sniping.
		/// </summary>
		public float sqrMinRadius;

		/// <summary>
		/// If spawning is enabled, whether to use the timer between spawns.
		/// </summary>
		public bool usesBossInterval
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			short flag;
			if (player.quests.getFlag(id, out flag))
			{
				return flag >= value;
			}
			else
			{
				return false;
			}
		}

		public override void ApplyCondition(Player player)
		{
			if (!shouldReset)
			{
				return;
			}

			player.quests.sendRemoveFlag(id);
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.format("Condition_ZombieKills");
			}

			short flag;
			if (!player.quests.getFlag(id, out flag))
			{
				flag = 0;
			}

			return Local.FormatText(text, flag, value);
		}

		public override bool isAssociatedWithFlag(ushort flagID)
		{
			return flagID == id;
		}

		internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
		{
			associatedFlags.Add(id);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseUInt16("ID", out ushort _id))
			{
				id = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseInt16("Value", out short _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}

			if (p.data.TryParseEnum("Zombie", out EZombieSpeciality _zombie))
			{
				zombie = _zombie;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Zombie");
			}

			spawn = p.data.ParseBool("Spawn");
			spawnQuantity = p.data.ParseInt32("Spawn_Quantity", 1);
			nav = p.data.ParseUInt8("Nav", byte.MaxValue);
			sqrRadius = MathfEx.Square(p.data.ParseFloat("Radius", defaultValue: 512f));
			sqrMinRadius = MathfEx.Square(p.data.ParseFloat("MinRadius"));
			LevelTableUniqueId = p.data.ParseInt32("LevelTableOverride", defaultValue: -1);

			// boss zombie is the traditional case
			// Some modders implemented "hunt down X special zombies" quests and wants them to still spawn in easy mode,
			// so if >1 spawn is requested we just ensure that many zombies of that type exist
			usesBossInterval = spawnQuantity < 2;
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseUInt16(p.legacyPrefix + "_ID", out ushort _id))
			{
				id = _id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseInt16(p.legacyPrefix + "_Value", out short _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}

			if (p.data.TryParseEnum(p.legacyPrefix + "_Zombie", out EZombieSpeciality _zombie))
			{
				zombie = _zombie;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Zombie");
			}

			spawn = p.data.ContainsKey(p.legacyPrefix + "_Spawn");
			spawnQuantity = p.data.ParseInt32(p.legacyPrefix + "_Spawn_Quantity", 1);
			nav = p.data.ParseUInt8(p.legacyPrefix + "_Nav", byte.MaxValue);
			sqrRadius = MathfEx.Square(p.data.ParseFloat(p.legacyPrefix + "_Radius", defaultValue: 512f));
			sqrMinRadius = MathfEx.Square(p.data.ParseFloat(p.legacyPrefix + "_MinRadius"));
			LevelTableUniqueId = p.data.ParseInt32(p.legacyPrefix + "_LevelTableOverride", defaultValue: -1);

			// boss zombie is the traditional case
			// Some modders implemented "hunt down X special zombies" quests and wants them to still spawn in easy mode,
			// so if >1 spawn is requested we just ensure that many zombies of that type exist
			usesBossInterval = spawnQuantity < 2;
		}

		public NPCZombieKillsCondition() { }

		[System.Obsolete]
		public NPCZombieKillsCondition(ushort newID, short newValue, EZombieSpeciality newZombie, bool newSpawn, int newSpawnQuantity, byte newNav, float newRadius, float newMinRadius, int newLevelTableUniqueId, string newText, bool newShouldReset) : base(newText, newShouldReset)
		{
			id = newID;
			value = newValue;
			zombie = newZombie;
			spawn = newSpawn;
			spawnQuantity = newSpawnQuantity;
			nav = newNav;
			sqrRadius = MathfEx.Square(newRadius);
			sqrMinRadius = MathfEx.Square(newMinRadius);
			LevelTableUniqueId = newLevelTableUniqueId;

			// boss zombie is the traditional case
			// Some modders implemented "hunt down X special zombies" quests and wants them to still spawn in easy mode,
			// so if >1 spawn is requested we just ensure that many zombies of that type exist
			usesBossInterval = spawnQuantity < 2;
		}
	}
}
