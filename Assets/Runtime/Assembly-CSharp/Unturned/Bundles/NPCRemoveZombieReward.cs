////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCRemoveZombieReward : INPCReward
	{
		/// <summary>
		/// If not none, only remove zombies of this type.
		/// </summary>
		public EZombieSpeciality ZombieSpeciality
		{
			get;
			set;
		}

		/// <summary>
		/// If greater than zero, only remove zombies matching this table unique ID.
		/// </summary>
		public int LevelTableUniqueId
		{
			get;
			set;
		}

		/// <summary>
		/// Navmesh index to remove zombies within. If set to byte.MaxValue then zombies are removed everywhere.
		/// </summary>
		public byte NavmeshIndex
		{
			get;
			set;
		}

		public override void GrantReward(Player player)
		{
			if (ZombieManager.regions == null)
			{
				return;
			}

			if (NavmeshIndex == byte.MaxValue)
			{
				foreach (ZombieRegion region in ZombieManager.regions)
				{
					ApplyToRegion(region);
				}
			}
			else if (NavmeshIndex < ZombieManager.regions.Length)
			{
				ApplyToRegion(ZombieManager.regions[NavmeshIndex]);
			}
		}

		private void ApplyToRegion(ZombieRegion region)
		{
			EPlayerKill kill;
			uint xp;
			foreach (Zombie zombie in region.zombies)
			{
				if (zombie == null || zombie.isDead)
				{
					continue;
				}

				if (LevelTableUniqueId > 0 && zombie.type != LevelTableUniqueId)
				{
					continue;
				}

				if (ZombieSpeciality != EZombieSpeciality.NONE && zombie.speciality != ZombieSpeciality)
				{
					continue;
				}

				zombie.askDamage(65000, Vector3.up, out kill, out xp, false, false);
			}
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			ZombieSpeciality = p.data.ParseEnum("Zombie", EZombieSpeciality.NONE);
			LevelTableUniqueId = p.data.ParseInt32("LevelTable", defaultValue: -1);
			NavmeshIndex = p.data.ParseUInt8("Nav", byte.MaxValue);
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			ZombieSpeciality = p.data.ParseEnum(p.legacyPrefix + "_Zombie", EZombieSpeciality.NONE);
			LevelTableUniqueId = p.data.ParseInt32(p.legacyPrefix + "_LevelTable", defaultValue: -1);
			NavmeshIndex = p.data.ParseUInt8(p.legacyPrefix + "_Nav", byte.MaxValue);
		}

		public NPCRemoveZombieReward() { }
	}
}
