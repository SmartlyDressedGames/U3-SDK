////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class PackInfo
	{
		public List<AnimalSpawnpoint> spawns
		{
			get;
			private set;
		}

		public List<Animal> animals
		{
			get;
			private set;
		}

		private Vector3 wanderNormal;
		private float _wanderAngle;
		public float wanderAngle
		{
			get => _wanderAngle;

			set
			{
				_wanderAngle = value;

				wanderNormal = new Vector3(Mathf.Cos(Mathf.Deg2Rad * wanderAngle), 0.0f, Mathf.Sin(Mathf.Deg2Rad * wanderAngle));
			}
		}

		public Vector3 getWanderDirection()
		{
			return wanderNormal;
		}

		private Vector3? avgSpawnPoint;

		public Vector3 getAverageSpawnPoint()
		{
			if (!avgSpawnPoint.HasValue)
			{
				avgSpawnPoint = Vector3.zero;
				for (int spawnIndex = 0; spawnIndex < spawns.Count; spawnIndex++)
				{
					AnimalSpawnpoint spawn = spawns[spawnIndex];

					if (spawn == null)
					{
						continue;
					}

					avgSpawnPoint += spawn.point;
				}
				avgSpawnPoint /= spawns.Count;
			}

			return avgSpawnPoint.Value;
		}

		private int avgAnimalPointRecalculation;
		private Vector3 avgAnimalPoint;

		public Vector3 getAverageAnimalPoint()
		{
			if (Time.frameCount > avgAnimalPointRecalculation)
			{
				avgAnimalPoint = Vector3.zero;
				for (int animalIndex = 0; animalIndex < animals.Count; animalIndex++)
				{
					Animal animal = animals[animalIndex];

					if (animal == null)
					{
						continue;
					}

					avgAnimalPoint += animal.transform.position;
				}
				avgAnimalPoint /= animals.Count;

				avgAnimalPointRecalculation = Time.frameCount;
			}

			return avgAnimalPoint;
		}

		public PackInfo()
		{
			this.spawns = new List<AnimalSpawnpoint>();
			this.animals = new List<Animal>();

			this.wanderAngle = Random.Range(0.0f, 360.0f);
		}
	}

	public class AnimalManager : SteamCaller
	{
		private class ValidAnimalSpawnsInfo
		{
			public List<AnimalSpawnpoint> spawns;
			public PackInfo pack;
		}

		//private static readonly float RESPAWN = 180f;

		private static AnimalManager manager;

		private static List<Animal> _animals;
		public static List<Animal> animals => _animals;

		private static List<PackInfo> _packs;
		public static List<PackInfo> packs => _packs;

		private static int tickIndex;
		private static List<Animal> _tickingAnimals;
		public static List<Animal> tickingAnimals => _tickingAnimals;

		public static ushort updates;
		private static ushort respawnPackIndex;
		//private static float lastFixedThink;
		private static float lastTick;

		public static uint maxInstances
		{
			get
			{
				switch (Level.info.size)
				{
					case ELevelSize.TINY:
						return Provider.modeConfigData.Animals.Max_Instances_Tiny;

					case ELevelSize.SMALL:
						return Provider.modeConfigData.Animals.Max_Instances_Small;

					case ELevelSize.MEDIUM:
						return Provider.modeConfigData.Animals.Max_Instances_Medium;

					case ELevelSize.LARGE:
						return Provider.modeConfigData.Animals.Max_Instances_Large;

					case ELevelSize.INSANE:
						return Provider.modeConfigData.Animals.Max_Instances_Insane;

					default:
						return 0;
				}
			}
		}

		public static bool giveAnimal(Player player, ushort id)
		{
			AnimalAsset asset = Assets.find(EAssetType.ANIMAL, id) as AnimalAsset;

			if (asset != null)
			{
				Vector3 point = player.transform.position + (player.transform.forward * 6);

				RaycastHit hit;
				Physics.Raycast(point + (Vector3.up * 16), Vector3.down, out hit, 32, RayMasks.BLOCK_VEHICLE);

				if (hit.collider != null)
				{
					point = hit.point;
				}

				spawnAnimal(id, point, player.transform.rotation);

				return true;
			}

			return false;
		}

		public static void spawnAnimal(ushort id, Vector3 point, Quaternion angle)
		{
			// Try to find a dead animal with the same ID and just respawn that
			foreach (Animal existingAnimal in animals)
			{
				if (existingAnimal.id == id && existingAnimal.isDead)
				{
					existingAnimal.sendRevive(point, Random.Range(0, 360f));
					return;
				}
			}

			// Didn't find a dead animal, so spawn a new one
			AnimalAsset asset = Assets.find(EAssetType.ANIMAL, id) as AnimalAsset;
			if (asset != null)
			{
				Animal animal = manager.addAnimal(id, point, angle.eulerAngles.y, false);
				AnimalSpawnpoint spawnpoint = new AnimalSpawnpoint(0, point); // Temporary hack to fix packs, but not priority to fix because it's only for testing.

				PackInfo animalPack = new PackInfo();
				animal.pack = animalPack;
				animalPack.animals.Add(animal);
				animalPack.spawns.Add(spawnpoint);
				packs.Add(animalPack);

				SendSingleAnimal.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), WriteSingleAnimal, animal);
			}
		}

		public static void getAnimalsInRadius(Vector3 center, float sqrRadius, List<Animal> result)
		{
			if (animals == null)
			{
				return;
			}

			for (int index = 0; index < animals.Count; index++)
			{
				Animal animal = animals[index];
				Vector3 offset = animal.transform.position - center;

				if (offset.sqrMagnitude < sqrRadius)
				{
					result.Add(animal);
				}
			}
		}

		[System.Obsolete]
		public void tellAnimalAlive(CSteamID steamID, ushort index, Vector3 newPosition, byte newAngle)
		{
			ReceiveAnimalAlive(index, newPosition, newAngle);
		}

		private static readonly ClientStaticMethod<ushort, Vector3, byte> SendAnimalAlive = ClientStaticMethod<ushort, Vector3, byte>.Get(ReceiveAnimalAlive);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellAnimalAlive))]
		public static void ReceiveAnimalAlive(ushort index, Vector3 newPosition, byte newAngle)
		{
			if (index >= animals.Count)
			{
				return;
			}

			animals[index].tellAlive(newPosition, newAngle);
		}

		[System.Obsolete]
		public void tellAnimalDead(CSteamID steamID, ushort index, Vector3 newRagdoll, byte newRagdollEffect)
		{
			ReceiveAnimalDead(index, newRagdoll, (ERagdollEffect) newRagdollEffect);
		}

		private static readonly ClientStaticMethod<ushort, Vector3, ERagdollEffect> SendAnimalDead = ClientStaticMethod<ushort, Vector3, ERagdollEffect>.Get(ReceiveAnimalDead);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellAnimalDead))]
		public static void ReceiveAnimalDead(ushort index, Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
		{
			if (index >= animals.Count)
			{
				return;
			}

			animals[index].tellDead(newRagdoll, newRagdollEffect);
		}

		private static uint seq;

		[System.Obsolete]
		public void tellAnimalStates(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendAnimalStates = ClientStaticMethod.Get(ReceiveAnimalStates);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveAnimalStates(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			uint newSeq;
			reader.ReadUInt32(out newSeq);
			if (newSeq <= seq)
				return;
			seq = newSeq;

			ushort count;
			reader.ReadUInt16(out count);
			if (count < 1)
				return;

			for (ushort index = 0; index < count; index++)
			{
				ushort animalIndex;
				reader.ReadUInt16(out animalIndex);
				Vector3 position;
				reader.ReadClampedVector3(out position);
				float yaw;
				reader.ReadDegrees(out yaw);

				if (animalIndex >= animals.Count)
				{
					context.IndexOutOfRange(nameof(animalIndex), animalIndex, animals.Count);
					continue;
				}

				animals[animalIndex].tellState(position, yaw);
			}
		}

		[System.Obsolete]
		public void askAnimalStartle(CSteamID steamID, ushort index)
		{
			ReceiveAnimalStartle(index, 0);
		}

		private static readonly ClientStaticMethod<ushort, byte> SendAnimalStartle = ClientStaticMethod<ushort, byte>.Get(ReceiveAnimalStartle);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askAnimalStartle))]
		public static void ReceiveAnimalStartle(ushort index, byte animationIndex)
		{
			if (index >= animals.Count)
			{
				return;
			}

			animals[index].PlayStartleAnimation(animationIndex);
		}

		[System.Obsolete]
		public void askAnimalAttack(CSteamID steamID, ushort index)
		{
			ReceiveAnimalAttack(index, 0);
		}

		private static readonly ClientStaticMethod<ushort, byte> SendAnimalAttack = ClientStaticMethod<ushort, byte>.Get(ReceiveAnimalAttack);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askAnimalAttack))]
		public static void ReceiveAnimalAttack(ushort index, byte animationIndex)
		{
			if (index >= animals.Count)
			{
				return;
			}

			animals[index].askAttack(animationIndex);
		}

		[System.Obsolete]
		public void askAnimalPanic(CSteamID steamID, ushort index)
		{
			ReceiveAnimalPanic(index);
		}

		private static readonly ClientStaticMethod<ushort> SendAnimalPanic = ClientStaticMethod<ushort>.Get(ReceiveAnimalPanic);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askAnimalPanic))]
		public static void ReceiveAnimalPanic(ushort index)
		{
			if (index >= animals.Count)
			{
				return;
			}

			animals[index].askPanic();
		}

		[System.Obsolete]
		public void tellAnimals(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendMultipleAnimals = ClientStaticMethod.Get(ReceiveMultipleAnimals);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveMultipleAnimals(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			ushort count;
			reader.ReadUInt16(out count);
			for (ushort index = 0; index < count; ++index)
			{
				ReadSingleAnimal(reader);
			}
		}

		private static void ReadSingleAnimal(NetPakReader reader)
		{
			ushort assetId;
			reader.ReadUInt16(out assetId);
			Vector3 position;
			reader.ReadClampedVector3(out position);
			float yaw;
			reader.ReadDegrees(out yaw);
			bool isAlive;
			reader.ReadBit(out isAlive);

			manager.addAnimal(assetId, position, yaw, isAlive);
		}

		[System.Obsolete]
		public void tellAnimal(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendSingleAnimal = ClientStaticMethod.Get(ReceiveSingleAnimal);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveSingleAnimal(in ClientInvocationContext context)
		{
			ReadSingleAnimal(context.reader);
		}

		[System.Obsolete]
		public void askAnimals(CSteamID steamID)
		{ }

		internal static void SendInitialGlobalState(ITransportConnection transportConnection)
		{
			SendMultipleAnimals.Invoke(ENetReliability.Reliable, transportConnection, SendMultipleAnimals_Write);
		}

		private static void SendMultipleAnimals_Write(NetPakWriter writer)
		{
			writer.WriteUInt16((ushort) animals.Count);
			for (ushort index = 0; index < animals.Count; ++index)
			{
				Animal animal = animals[index];
				WriteSingleAnimal(writer, animal);
			}
		}

		[System.Obsolete]
		public void sendAnimal(Animal animal, NetPakWriter writer)
		{ }

		private static void WriteSingleAnimal(NetPakWriter writer, Animal animal)
		{
			writer.WriteUInt16(animal.id);
			writer.WriteClampedVector3(animal.transform.position);
			writer.WriteDegrees(animal.transform.eulerAngles.y);
			writer.WriteBit(animal.isDead);
		}

		public static void sendAnimalAlive(Animal animal, Vector3 newPosition, byte newAngle)
		{
			SendAnimalAlive.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), animal.index, newPosition, newAngle);
		}

		public static void sendAnimalDead(Animal animal, Vector3 newRagdoll, ERagdollEffect newRagdollEffect = ERagdollEffect.None)
		{
			SendAnimalDead.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), animal.index, newRagdoll, newRagdollEffect);
		}

		public static void sendAnimalStartle(Animal animal, byte animationIndex)
		{
			SendAnimalStartle.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), animal.index, animationIndex);
		}

		public static void sendAnimalAttack(Animal animal, byte animationIndex)
		{
			SendAnimalAttack.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), animal.index, animationIndex);
		}

		public static void sendAnimalPanic(Animal animal)
		{
			SendAnimalPanic.InvokeAndLoopback(ENetReliability.Unreliable, Provider.GatherRemoteClientConnections(), animal.index);
		}

		public static void dropLoot(Animal animal)
		{
			if (animal == null || animal.asset == null || animal.transform == null)
				return; // Silent because other exceptions have been thrown if it's this broken.

			if (animal.asset.rewardID != 0)
			{
				int rewards = Random.Range(animal.asset.rewardMin, animal.asset.rewardMax + 1);
				// Prevent players from crashing themselves with huge numbers of items.
				rewards = Mathf.Clamp(rewards, 0, 100);
				for (int reward = 0; reward < rewards; reward++)
				{
					ushort id = SpawnTableTool.ResolveLegacyId(animal.asset.rewardID, EAssetType.ITEM, animal.asset.OnGetRewardSpawnTableErrorContext);

					if (id != 0)
					{
						ItemManager.dropItem(new Item(id, EItemOrigin.NATURE), animal.transform.position, false, Dedicator.IsDedicatedServer, true);
					}
				}
			}
			else
			{
				if (animal.asset.meat != 0)
				{
					int drops = Random.Range(2, 5);
					for (int step = 0; step < drops; step++)
					{
						ItemManager.dropItem(new Item(animal.asset.meat, EItemOrigin.NATURE), animal.transform.position, false, Dedicator.IsDedicatedServer, true);
					}
				}

				if (animal.asset.pelt != 0)
				{
					int drops = Random.Range(2, 5);
					for (int step = 0; step < drops; step++)
					{
						ItemManager.dropItem(new Item(animal.asset.pelt, EItemOrigin.NATURE), animal.transform.position, false, Dedicator.IsDedicatedServer, true);
					}
				}
			}
		}

		/// <summary>
		/// Spawns an animal into the world.
		/// </summary>
		/// <param name="id">The ID of the animal.</param>
		/// <param name="point">Position to spawn the animal.</param>
		/// <param name="angle">Angle to spawn the animal.</param>
		/// <param name="isDead">Whether the animal is dead or not.</param>
		private Animal addAnimal(ushort id, Vector3 point, float angle, bool isDead)
		{
			ThreadUtil.ConditionalAssertIsGameThread();

			Animal character = null;

			AnimalAsset asset = Assets.find(EAssetType.ANIMAL, id) as AnimalAsset;
			if (asset != null)
			{
				try
				{
					GameObject animalPrefab;
					if (Dedicator.IsDedicatedServer)
					{
						animalPrefab = asset.dedicated;
					}
					else if (Provider.isServer)
					{
						animalPrefab = asset.server;
					}
					else
					{
						animalPrefab = asset.client;
					}

					Quaternion rotation = Quaternion.Euler(0, angle, 0);
					GameObject animalGameObject = Instantiate(animalPrefab, point, rotation);
					animalGameObject.name = id.ToString(); // Older systems might rely on name.

					character = animalGameObject.AddComponent<Animal>();
					character.index = (ushort) animals.Count;
					character.id = id;
					character.isDead = isDead;
					character.init();

					animals.Add(character);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught an exception instantiating animal \"{asset.FriendlyName}\" ({asset.GUID:N})");
				}
			}

			return character;
		}

		/// <summary>
		/// Gets the animal at a specific index.
		/// </summary>
		/// <param name="index">The index of the animal.</param>
		/// <returns></returns>
		public static Animal getAnimal(ushort index)
		{
			if (index >= animals.Count)
			{
				return null;
			}

			return animals[index];
		}

		/// <summary>
		/// Find replacement spawnpoint for an animal and teleport it there.
		/// </summary>
		public static void TeleportAnimalBackIntoMap(Animal animal)
		{
			Vector3? newPosition = null;

			if (animal.pack != null)
			{
				if (animal.pack.animals != null)
				{
					foreach (Animal friendAnimal in animal.pack.animals)
					{
						if (animal == friendAnimal || friendAnimal.isDead)
							continue;

						Vector3 friendPosition = friendAnimal.transform.position;
						if (!UndergroundAllowlist.IsPositionWithinValidHeight(friendPosition))
							continue;

						newPosition = friendPosition;
						break;
					}
				}

				if (!newPosition.HasValue)
				{
					if (animal.pack.spawns != null && animal.pack.spawns.Count > 0)
					{
						newPosition = animal.pack.spawns[animal.pack.spawns.GetRandomIndex()].point;
					}
				}
			}

			if (!newPosition.HasValue)
			{
				// This shouldn't happen because pack is assigned by vanilla code, but just in case for plugins...
				if (LevelAnimals.spawns != null && LevelAnimals.spawns.Count > 0)
				{
					// Random spawn anywhere in the level.
					newPosition = LevelAnimals.spawns[LevelAnimals.spawns.GetRandomIndex()].point;
				}
				else
				{
					Vector3 adjustedPosition = animal.transform.position;
					adjustedPosition.y = Level.HEIGHT - 10.0f;
					newPosition = adjustedPosition;
				}
			}

			EffectAsset souls_1 = ZombieManager.Souls_1_Ref.Find();
			if (souls_1 != null)
			{
				TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(souls_1);
				triggerEffectParameters.relevantDistance = 16.0f;
				triggerEffectParameters.position = animal.transform.position + Vector3.up;
				EffectManager.triggerEffect(triggerEffectParameters);
			}

			animal.transform.position = newPosition.Value + Vector3.up;
		}

		/// <summary>
		/// Used in arena mode to reset all animals to dead.
		/// </summary>
		public static void askClearAllAnimals()
		{
			foreach (Animal animal in animals)
			{
				EPlayerKill kill;
				uint xp;
				animal.askDamage(ushort.MaxValue, Vector3.up, out kill, out xp, trackKill: false, dropLoot: false);
			}
		}

		private void respawnAnimals()
		{
			if (Level.info == null || Level.info.type == ELevelType.ARENA)
			{
				return;
			}

			if (respawnPackIndex >= packs.Count)
			{
				respawnPackIndex = (ushort) (packs.Count - 1);
			}

			PackInfo pack = packs[respawnPackIndex];

			respawnPackIndex++;
			if (respawnPackIndex >= packs.Count)
			{
				respawnPackIndex = 0;
			}

			if (pack == null)
			{
				return;
			}

			for (int animalIndex = 0; animalIndex < pack.animals.Count; animalIndex++)
			{
				Animal animal = pack.animals[animalIndex];

				if (animal == null || animal.IsAlive || Time.realtimeSinceStartup - animal.lastDead < Provider.modeConfigData.Animals.Respawn_Time)
				{
					return; // cancel because at least part of the pack is still alive, we only respawn when it's entirely dead
				}
			}

			List<AnimalSpawnpoint> valid = new List<AnimalSpawnpoint>();
			for (int spawnIndex = 0; spawnIndex < pack.spawns.Count; spawnIndex++)
			{
				valid.Add(pack.spawns[spawnIndex]);
			}

			for (int animalIndex = 0; animalIndex < pack.animals.Count; animalIndex++)
			{
				Animal animal = pack.animals[animalIndex];

				if (animal == null)
				{
					continue; // unneccessary check because above it returns when null animal, but just incase someone changes that
				}

				int spawnIndex = Random.Range(0, valid.Count);
				AnimalSpawnpoint spawn = valid[spawnIndex];
				valid.RemoveAt(spawnIndex);

				Vector3 point = spawn.point;
				point.y += 0.1f;

				animal.sendRevive(point, Random.Range(0, 360f));
			}
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				seq = 0;

				_animals = new List<Animal>();
				_packs = null;
				updates = 0;

				tickIndex = 0;
				_tickingAnimals = new List<Animal>();

				if (Provider.isServer)
				{
					_packs = new List<PackInfo>();

					if (LevelAnimals.spawns.Count > 0 && Level.info != null && Level.info.type != ELevelType.ARENA)
					{
						for (int spawnIndex = 0; spawnIndex < LevelAnimals.spawns.Count; spawnIndex++)
						{
							AnimalSpawnpoint spawn = LevelAnimals.spawns[spawnIndex];

							int foundIndex = -1;
							for (int packIndex = packs.Count - 1; packIndex >= 0; packIndex--)
							{
								List<AnimalSpawnpoint> group = packs[packIndex].spawns;

								for (int animalIndex = 0; animalIndex < group.Count; animalIndex++)
								{
									AnimalSpawnpoint other = group[animalIndex];

									if ((other.point - spawn.point).sqrMagnitude < 256) // less than 16 meters from this other spawn
									{
										if (foundIndex == -1) // we haven't found anyone nearby yet, so we'll add ourselves to this one
										{
											group.Add(spawn);
										}
										else // we already found someone nearby, so let's move that entire group into this one
										{
											List<AnimalSpawnpoint> others = packs[foundIndex].spawns;
											for (int otherIndex = 0; otherIndex < others.Count; otherIndex++)
											{
												group.Add(others[otherIndex]);
											}

											packs.RemoveAtFast(foundIndex);
										}

										foundIndex = packIndex;
										break;
									}
								}
							}

							if (foundIndex == -1) // no group found, we'll start our own!
							{
								PackInfo pack = new PackInfo();
								pack.spawns.Add(spawn);

								packs.Add(pack);
							}
						}

						List<ValidAnimalSpawnsInfo> valid = new List<ValidAnimalSpawnsInfo>();
						for (int packIndex = 0; packIndex < packs.Count; packIndex++)
						{
							PackInfo pack = packs[packIndex];

							List<AnimalSpawnpoint> spawns = new List<AnimalSpawnpoint>();
							for (int spawnIndex = 0; spawnIndex < pack.spawns.Count; spawnIndex++)
							{
								spawns.Add(pack.spawns[spawnIndex]);
							}

							ValidAnimalSpawnsInfo info = new ValidAnimalSpawnsInfo();
							info.spawns = spawns;
							info.pack = pack;
							valid.Add(info);
						}

						while (animals.Count < maxInstances && valid.Count > 0)
						{
							int groupIndex = Random.Range(0, valid.Count);
							ValidAnimalSpawnsInfo info = valid[groupIndex];

							int animalIndex = Random.Range(0, info.spawns.Count);
							AnimalSpawnpoint spawn = info.spawns[animalIndex];
							info.spawns.RemoveAt(animalIndex);
							if (info.spawns.Count == 0)
							{
								valid.RemoveAt(groupIndex);
							}

							Vector3 point = spawn.point;
							point.y += 0.1f;

							ushort id;
							if (info.pack.animals.Count > 0)
							{
								id = info.pack.animals[0].id;
							}
							else
							{
								id = LevelAnimals.getAnimal(spawn);
							}

							Animal animal = addAnimal(id, point, Random.Range(0f, 360f), false);
							if (animal != null)
							{
								animal.pack = info.pack;
								info.pack.animals.Add(animal);
							}
						}

						for (int packIndex = packs.Count - 1; packIndex >= 0; packIndex--)
						{
							PackInfo pack = packs[packIndex];

							if (pack.animals.Count > 0)
							{
								continue;
							}

							packs.RemoveAt(packIndex);
						}
					}
				}
			}
		}

		private void OnDrawGizmos()
		{
			if (packs == null)
			{
				return;
			}

			for (int packIndex = 0; packIndex < packs.Count; packIndex++)
			{
				PackInfo pack = packs[packIndex];

				if (pack == null || pack.spawns == null || pack.animals == null)
				{
					continue;
				}

				Vector3 avgSpawnPoint = pack.getAverageSpawnPoint();
				Vector3 avgAnimalPoint = pack.getAverageAnimalPoint();
				Vector3 direction = pack.getWanderDirection();

				Gizmos.color = Color.gray;
				for (int spawnIndex = 0; spawnIndex < pack.spawns.Count; spawnIndex++)
				{
					AnimalSpawnpoint spawn = pack.spawns[spawnIndex];

					if (spawn == null)
					{
						continue;
					}

					Gizmos.DrawLine(avgSpawnPoint, spawn.point);
				}

				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(avgSpawnPoint, avgAnimalPoint);

				for (int animalIndex = 0; animalIndex < pack.animals.Count; animalIndex++)
				{
					Animal animal = pack.animals[animalIndex];

					if (animal == null)
					{
						continue;
					}

					Gizmos.color = animal.isDead ? Color.red : Color.green;
					Gizmos.DrawLine(avgAnimalPoint, animal.transform.position);

					if (animal.IsAlive)
					{
						Gizmos.color = Color.magenta;
						Gizmos.DrawLine(animal.transform.position, animal.target);
					}
				}

				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(avgAnimalPoint, avgAnimalPoint + (direction * 4.0f));
			}
		}

		private List<Animal> animalsToSend = new List<Animal>();

		private void sendAnimalStates()
		{
			//	// PACKET SIZE
			//	//
			//	// index		2
			//	// pos		  8 (12 without compression)
			//	// angle		1
			//	//
			//	// TOTAL		11 bytes per animal

			seq++;

			for (int clientIndex = 0; clientIndex < Provider.clients.Count; clientIndex++)
			{
				SteamPlayer client = Provider.clients[clientIndex];

				if (client == null || client.player == null)
				{
					continue;
				}

				ushort updateCount = 0;
				animalsToSend.Clear();

				for (int animalIndex = 0; animalIndex < animals.Count; animalIndex++)
				{
					Animal animal = animals[animalIndex];

					if (animal == null || !animal.isUpdated)
					{
						continue;
					}

					animalsToSend.Add(animal);
					updateCount++;
				}

				if (updateCount == 0)
				{
					continue;
				}

				SendAnimalStates.Invoke(ENetReliability.Unreliable, client.transportConnection, SendAnimalStates_Write, updateCount);

#if WITH_NSB_LOGGING
				client.sentAnimalUpdate = Time.realtimeSinceStartup;
				sentAnyAnimalUpdate = Time.realtimeSinceStartup;
#endif // WITH_NSB_LOGGING
			}

			for (int animalIndex = 0; animalIndex < animals.Count; animalIndex++)
			{
				Animal animal = animals[animalIndex];

				if (animal == null)
				{
					continue;
				}

				animal.isUpdated = false;
			}

#if WITH_NSB_LOGGING
			float timeSinceAnyUpdate = Time.realtimeSinceStartup - sentAnyAnimalUpdate;
			if(NsbLog.isEnabledOnServer && timeSinceAnyUpdate < 10)
			{
				foreach(SteamPlayer client in Provider.clients)
				{
					if(client == null)
						continue;

					float timeSinceUpdate = Time.realtimeSinceStartup - client.sentAnimalUpdate;
					if(timeSinceUpdate > 10)
					{
						client.sentAnimalUpdate = Time.realtimeSinceStartup; // Prevent warning spam.
						NsbLog.WarningFormat("{0}s since we sent tellAnimalStates to {1}", timeSinceUpdate, client.playerID);
					}
				}
			}
#endif // WITH_NSB_LOGGING
		}

		private void SendAnimalStates_Write(NetPakWriter writer, ushort updateCount)
		{
			writer.WriteUInt32(seq);
			writer.WriteUInt16(updateCount);
			foreach (Animal animal in animalsToSend)
			{
				writer.WriteUInt16(animal.index);
				writer.WriteClampedVector3(animal.transform.position);
				writer.WriteDegrees(animal.transform.eulerAngles.y);
			}
		}

#if WITH_NSB_LOGGING
		private float sentAnyAnimalUpdate;
#endif // WITH_NSB_LOGGING

		private void Update()
		{
			if (!Provider.isServer || !Level.isLoaded)
			{
				return;
			}

			if (animals == null || animals.Count == 0)
			{
				return;
			}

			if (tickingAnimals == null)
			{
				return;
			}

			int start;
			int end;

			if (Dedicator.IsDedicatedServer)
			{
				if (tickIndex >= tickingAnimals.Count)
				{
					tickIndex = 0;
				}

				start = tickIndex;
				end = start + 25;
				if (end >= tickingAnimals.Count)
				{
					end = tickingAnimals.Count;
				}

				tickIndex = end;
			}
			else
			{
				start = 0;
				end = tickingAnimals.Count;
			}

			UnityEngine.Profiling.Profiler.BeginSample("Tick");
			for (int index = end - 1; index >= start; index--)
			{
				Animal animal = tickingAnimals[index];

				if (animal == null)
				{
					UnturnedLog.error("Missing animal " + index);
					continue;
				}

				animal.tick();
			}
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Packet");
			if (Dedicator.IsDedicatedServer && Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
			{
				lastTick += Provider.UPDATE_TIME;
				if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
				{
					lastTick = Time.realtimeSinceStartup;
				}

				sendAnimalStates();
			}
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Respawn");
			respawnAnimals();
			UnityEngine.Profiling.Profiler.EndSample();
		}

		private void Start()
		{
			manager = this;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			Level.onLevelLoaded += onLevelLoaded;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Animals: {animals?.Count}");
			results.Add($"Animal packs: {packs?.Count}");
			results.Add($"Ticking animals: {tickingAnimals?.Count}");
		}

		[System.Obsolete]
		public static void sendAnimalStartle(Animal animal)
		{
			sendAnimalStartle(animal, 0);
		}

		[System.Obsolete]
		public static void sendAnimalAttack(Animal animal)
		{
			sendAnimalAttack(animal, 0);
		}
	}
}
