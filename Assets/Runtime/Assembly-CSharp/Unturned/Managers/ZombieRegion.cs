////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void HyperUpdated(bool isHyper);
	public delegate void ZombieLifeUpdated(Zombie zombie);

	public class ZombieRegion
	{
		public HyperUpdated onHyperUpdated;
		public ZombieLifeUpdated onZombieLifeUpdated;

		private List<Zombie> _zombies;
		public List<Zombie> zombies => _zombies;

		public byte nav
		{
			get;
			protected set;
		}

		public FlagData flagData;

		public ushort updates;
		public ushort respawnZombieIndex;

		/// <summary>
		/// Number of alive zombies.
		/// </summary>
		public int alive;

		//public bool isMarked;
		public bool isNetworked;

		public float lastMega;
		public bool hasMega;

		private bool _hasBeacon;
		public bool hasBeacon
		{
			get => _hasBeacon;

			set
			{
				if (value != _hasBeacon)
				{
					_hasBeacon = value;

					onHyperUpdated?.Invoke(isHyper);
				}
			}
		}

		public bool isHyper => LightingManager.isFullMoon || hasBeacon;

		public bool HasInfiniteAgroRange => hasBeacon || (flagData != null && flagData.hyperAgro);

		public bool isRadioactive;

		private Zombie bossZombie;

		/// <summary>
		/// Last time a quest boss was spawned.
		/// </summary>
		private float lastBossTime = -1f;

		public int GetAliveBossZombieCount()
		{
			return aliveBossZombieCount;
		}

		/// <summary>
		/// Allow another quest to spawn a boss zombie immediately.
		/// </summary>
		public void resetQuestBossTimer()
		{
			lastBossTime = -1f;
		}

		/// <summary>
		/// Kills the boss zombie if nobody is around, if the boss was killed it calls UpdateBoss.
		/// </summary>
		public void UpdateRegion()
		{
			if (bossZombie == null)
			{
				return;
			}

			// separate bounds/nav prevents stuttering on edge
			bool hasPlayerInBounds = false;
			bool hasPlayerInNav = false;
			for (int index = 0; index < Provider.clients.Count; index++)
			{
				SteamPlayer player = Provider.clients[index];

				if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
				{
					continue;
				}

				if (player.player.movement.bound == nav)
				{
					hasPlayerInBounds = true;
				}

				if (player.player.movement.nav == nav)
				{
					hasPlayerInNav = true;
				}

				if (hasPlayerInBounds && hasPlayerInNav)
				{
					break;
				}
			}

			if (hasPlayerInBounds)
			{
				if (bossZombie.isDead)
				{
					bossZombie = null;

					if (hasPlayerInNav)
					{
						UpdateBoss();
					}
				}
			}
			else
			{
				EPlayerKill kill;
				uint xp;
				bossZombie.askDamage(50000, Vector3.up, out kill, out xp, false, false);

				// All players left without killing the boss (e.g. failed pirate quest), so allow another try immediately.
				resetQuestBossTimer();
			}
		}

		public Zombie FindBestZombieToRespawnDifferentSpeciality(EZombieSpeciality speciality)
		{
			// first try to find a dead zombie
			foreach (Zombie zombie in zombies)
			{
				if (zombie != null && zombie.isDead)
				{
					return zombie;
				}
			}

			// next try to find a non-wandering zombie, preferring normal ones
			if (speciality != EZombieSpeciality.NORMAL)
			{
				foreach (Zombie zombie in zombies)
				{
					if (zombie != null && !zombie.isHunting && zombie.speciality == EZombieSpeciality.NORMAL)
					{
						return zombie;
					}
				}
			}

			// next try to find a non-wandering zombie
			foreach (Zombie zombie in zombies)
			{
				if (zombie != null && !zombie.isHunting && zombie.speciality != speciality)
				{
					return zombie;
				}
			}

			// worst case grab any zombie that isn't already our type
			foreach (Zombie zombie in zombies)
			{
				if (zombie != null && zombie.speciality != speciality)
				{
					return zombie;
				}
			}

			return null;
		}

		/// <summary>
		/// Checks for players in the area with quests and spawns boss zombies accordingly.
		/// </summary>
		public void UpdateBoss()
		{
			if (bossZombie != null)
			{
				return;
			}

			bool canSpawnBoss = lastBossTime < 0 || (Time.time - lastBossTime > Provider.modeConfigData.Zombies.Quest_Boss_Respawn_Interval);

			for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
			{
				SteamPlayer player = Provider.clients[playerIndex];

				if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
				{
					continue;
				}

				if (player.player.movement.nav == nav)
				{
					for (int questIndex = 0; questIndex < player.player.quests.questsList.Count; questIndex++)
					{
						PlayerQuest quest = player.player.quests.questsList[questIndex];

						if (quest == null || quest.asset == null)
						{
							continue;
						}

						for (int conditionIndex = 0; conditionIndex < quest.asset.conditions.Length; conditionIndex++)
						{
							NPCZombieKillsCondition condition = quest.asset.conditions[conditionIndex] as NPCZombieKillsCondition;

							if (condition == null)
							{
								continue;
							}

							if (condition.nav == nav && condition.spawn && !condition.isConditionMet(player.player))
							{
								bool isBossZombie = condition.usesBossInterval;
								if (isBossZombie && !canSpawnBoss)
								{
									// Prevent players from abusing bosses to farm loot.
									continue;
								}

								int quantityToSpawn = Mathf.Min(zombies.Count, condition.spawnQuantity);

								// existing quantity counts alive zombies that match our desired speciality
								int existingQuantity = 0;
								foreach (Zombie zombie in zombies)
								{
									if (zombie != null && !zombie.isDead && zombie.speciality == condition.zombie)
									{
										existingQuantity++;
									}
								}

								int overrideTableIndex = LevelZombies.FindTableIndexByUniqueId(condition.LevelTableUniqueId);
								ZombieTable overrideTable = overrideTableIndex >= 0 ? LevelZombies.tables[overrideTableIndex] : null;

								// convert the remaining desired num zombies from either dead ones,
								// or alive ones that aren't already our desired speciality
								int spawnedQuantity = existingQuantity;
								while (spawnedQuantity < quantityToSpawn)
								{
									Zombie targetZombie = FindBestZombieToRespawnDifferentSpeciality(condition.zombie);
									if (targetZombie == null)
									{
										break;
									}

									Vector3 point = targetZombie.transform.position;

									// find a new point 
									if (targetZombie.isDead)
									{
										for (int attempt = 0; attempt < 10; attempt++)
										{
											ZombieSpawnpoint spawn = LevelZombies.zombies[nav][Random.Range(0, LevelZombies.zombies[nav].Count)];

											if (SafezoneManager.checkPointValid(spawn.point))
											{
												break;
											}

											point = spawn.point;
											point.y += 0.1f;
										}
									}

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

									++spawnedQuantity;
									targetZombie.sendRevive(spawnTableIndex, (byte) condition.zombie, spawnShirt, spawnPants, spawnHat, spawnGear, point, Random.Range(0f, 360f));
									if (isBossZombie)
									{
										bossZombie = targetZombie;
									}
								}

								UnturnedLog.info("Spawned " + spawnedQuantity + " " + condition.zombie + " zombies in nav " + nav + " for quest " + quest.id + ", isBoss " + isBossZombie + " boss = " + bossZombie);
							}
						}
					}
				}
			}
		}

		private void onMoonUpdated(bool isFullMoon)
		{
			onHyperUpdated?.Invoke(isHyper);
		}

		public void destroy()
		{
			for (ushort index = 0; index < zombies.Count; index++)
			{
				GameObject.Destroy(zombies[index].gameObject);
			}

			zombies.Clear();

			hasMega = false;
		}

		// needed to call after because of how onFullMoonUpdated is reset
		public void init()
		{
			LightingManager.onMoonUpdated += onMoonUpdated;
		}

		public ZombieRegion(byte newNav)
		{
			_zombies = new List<Zombie>();
			this.nav = newNav;

			if (nav < LevelNavigation.flagData.Count)
			{
				flagData = LevelNavigation.flagData[nav];
			}

			updates = 0;
			respawnZombieIndex = 0;
			alive = 0;

			//isMarked = false;
			isNetworked = false;

			lastMega = -1000;
			hasMega = false;
		}

		private bool isInRegionsWithPlayersSet;
		private int _playerCountInRegion;
		public int PlayerCountInRegion
		{
			get => _playerCountInRegion;
			internal set
			{
				if (value == _playerCountInRegion)
				{
					return;
				}

				_playerCountInRegion = value;

				if (_playerCountInRegion < 1)
				{
					if (isInRegionsWithPlayersSet)
					{
						isInRegionsWithPlayersSet = false;
						ZombieManager.regionsWithPlayers.Remove(nav);
					}
				}
				else
				{
					if (!isInRegionsWithPlayersSet)
					{
						isInRegionsWithPlayersSet = true;
						ZombieManager.regionsWithPlayers.Add(nav);
					}
				}
			}
		}

		internal int aliveBossZombieCount;
	}
}
