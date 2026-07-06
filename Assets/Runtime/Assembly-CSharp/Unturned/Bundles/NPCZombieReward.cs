////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCZombieReward : INPCReward
	{
		/// <summary>
		/// Spawned zombie will be changed to this speciality type.
		/// </summary>
		public EZombieSpeciality ZombieSpeciality
		{
			get;
			set;
		}

		/// <summary>
		/// Zombie(s) will be spawned at a Spawnpoint node matching this ID.
		/// If multiple Spawnpoints match this ID a random spawnpoint is chosen for each zombie.
		/// </summary>
		public string SpawnpointId
		{
			get;
			set;
		}

		/// <summary>
		/// If greater than zero, find this zombie type configured in the level editor. For example, if the level editor
		/// lists "0 Fire (4)", then 4 is the unique ID, and if assigned to this reward a zombie from the "Fire"
		/// table will spawn.
		/// </summary>
		public int LevelTableUniqueId
		{
			get;
			set;
		}

		/// <summary>
		/// Number of zombies to spawn.
		/// </summary>
		public int SpawnQuantity
		{
			get;
			set;
		}

		/// <summary>
		/// If set, zombies will not spawn unless CooldownDuration seconds have passed since last run.
		/// </summary>
		public string CooldownId
		{
			get;
			set;
		}

		public float CooldownDuration
		{
			get;
			set;
		}

		private static List<Spawnpoint> spawnpointsWorkingCopy = new List<Spawnpoint>();
		public override void GrantReward(Player player)
		{
			if (SpawnQuantity < 1)
			{
				return;
			}

			if (!string.IsNullOrEmpty(CooldownId))
			{
				if (!ZombieManager.CheckCustomCooldown(CooldownId, CooldownDuration))
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					UnturnedLog.info($"NPC zombie reward skipped because custom cooldown \"{CooldownId}\" still pending");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					return;
				}
			}

			List<Spawnpoint> potentialSpawnpoints;
			if (!SpawnpointSystemV2.Get().idToSpawnpoints.TryGetValue(SpawnpointId, out potentialSpawnpoints)
				|| potentialSpawnpoints.Count < 1)
			{
				UnturnedLog.error($"No spawnpoints for NPC zombie reward matching ID \"{SpawnpointId}\"");
				return;
			}

			spawnpointsWorkingCopy.Clear();
			spawnpointsWorkingCopy.AddRange(potentialSpawnpoints);

			int overrideTableIndex = LevelZombies.FindTableIndexByUniqueId(LevelTableUniqueId);
			ZombieTable overrideTable = overrideTableIndex >= 0 ? LevelZombies.tables[overrideTableIndex] : null;

			int remainingToSpawn = SpawnQuantity;
			do
			{
				int randomSpawnpointIndex = spawnpointsWorkingCopy.GetRandomIndex();
				Spawnpoint randomSpawnpoint = spawnpointsWorkingCopy[randomSpawnpointIndex];
				spawnpointsWorkingCopy.RemoveAtFast(randomSpawnpointIndex);

				Vector3 position;
				Quaternion rotation;
				randomSpawnpoint.transform.GetPositionAndRotation(out position, out rotation);

				byte navIndex;
				if (!LevelNavigation.tryGetNavigation(position, out navIndex))
				{
					UnturnedLog.error($"Spawnpoint for NPC zombie reward \"{SpawnpointId}\" at {position} isn't within a navmesh");
					continue;
				}

				if (!SafezoneManager.checkPointValid(position))
				{
					// Cannot use this spot because players built a Safezone Radiator.
					continue;
				}

				if (ZombieManager.regions == null || navIndex >= ZombieManager.regions.Length)
				{
					// Somehow got called while level is not loaded? :S
					break;
				}

				ZombieRegion region = ZombieManager.regions[navIndex];

				Zombie targetZombie = region.FindBestZombieToRespawnDifferentSpeciality(ZombieSpeciality);
				if (targetZombie == null)
				{
					UnturnedLog.info($"Unable to spawn all zombies for NPC zombie reward \"{SpawnpointId}\" because we ran out of candidates");
					break;
				}

				position += new Vector3(0.0f, 0.1f, 0.0f); // Matches regular zombie spawn offset.
				float yaw = rotation.eulerAngles.y;

				byte spawnTableIndex = targetZombie.type;
				byte spawnShirt = targetZombie.shirt;
				byte spawnPants = targetZombie.pants;
				byte spawnHat = targetZombie.hat;
				byte spawnGear = targetZombie.gear;
				if (overrideTable != null)
				{
					spawnTableIndex = (byte) overrideTableIndex;
					overrideTable.GetSpawnClothingParameters(out spawnShirt, out spawnPants, out spawnHat, out spawnGear);
				}
				targetZombie.sendRevive(spawnTableIndex, (byte) ZombieSpeciality, spawnShirt, spawnPants, spawnHat, spawnGear, position, yaw);

				--remainingToSpawn;
			}
			while (remainingToSpawn > 0 && spawnpointsWorkingCopy.Count > 0);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			UnturnedLog.info($"NPC zombie reward \"{SpawnpointId}\" had {remainingToSpawn} remaining to spawn");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseEnum("Zombie", out EZombieSpeciality _zombieType))
			{
				ZombieSpeciality = _zombieType;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Zombie");
			}

			if (p.data.TryGetString("Spawnpoint", out string _spawnpoint))
			{
				SpawnpointId = _spawnpoint;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Spawnpoint");
			}

			LevelTableUniqueId = p.data.ParseInt32("LevelTableOverride", defaultValue: -1);
			SpawnQuantity = p.data.ParseInt32("SpawnQuantity", defaultValue: 1);
			CooldownId = p.data.GetString("CooldownId");
			CooldownDuration = p.data.ParseFloat("CooldownDuration", -1.0f);
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseEnum(p.legacyPrefix + "_Zombie", out EZombieSpeciality _zombieType))
			{
				ZombieSpeciality = _zombieType;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Zombie");
			}

			if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out string _spawnpoint))
			{
				SpawnpointId = _spawnpoint;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Spawnpoint");
			}

			LevelTableUniqueId = p.data.ParseInt32(p.legacyPrefix + "_LevelTableOverride", defaultValue: -1);
			SpawnQuantity = p.data.ParseInt32(p.legacyPrefix + "_SpawnQuantity", defaultValue: 1);
			CooldownId = p.data.GetString(p.legacyPrefix + "_CooldownId");
			CooldownDuration = p.data.ParseFloat(p.legacyPrefix + "_CooldownDuration", -1.0f);
		}

		public NPCZombieReward() { }
	}
}
