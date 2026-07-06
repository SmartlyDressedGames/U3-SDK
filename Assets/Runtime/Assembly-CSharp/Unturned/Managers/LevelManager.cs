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
	public enum EArenaState
	{
		LOBBY, // waiting for players, goes to CLEAR
		CLEAR, // clear old dropped items, goes to WARMUP
		WARMUP, // warning that it's starting, goes to LOBBY if empty or SPAWN if ready
		SPAWN, // teleport players and spawn items, goes to PLAY
		PLAY, // watch for deaths and leaving players, goes to FINALE
		FINALE, // announce winner! goes to RESTART
		RESTART, // teleport players to origin
		INTERMISSION // wait between matches
	}

	[NetEnum]
	public enum EArenaMessage
	{
		LOBBY, // "waiting for players"
		WARMUP, // "starting:"
		PLAY, // "let the battle begin!"
		DIED, // "X died"
		ABANDONED, // "X abandoned"
		WIN, // "X wins!"
		LOSE, // "everyone loses!"
		INTERMISSION, // "intermission: "
	}

	public class ArenaPlayer
	{
		private SteamPlayer _steamPlayer;
		public SteamPlayer steamPlayer => _steamPlayer;

		private bool _hasDied;
		public bool hasDied => _hasDied;

		/// <summary>
		/// Time.time damage was last dealt so that damage is applied once per second.
		/// </summary>
		public float lastAreaDamage;

		/// <summary>
		/// Timer increased while taking damage, and reset to zero while inside zone.
		/// </summary>
		public float timeOutsideArea;

		private void onLifeUpdated(bool isDead)
		{
			if (isDead)
			{
				_hasDied = true;
			}
		}

		public ArenaPlayer(SteamPlayer newSteamPlayer)
		{
			_steamPlayer = newSteamPlayer;
			_hasDied = false;

			steamPlayer.player.life.onLifeUpdated += onLifeUpdated;
		}
	}

	public class AirdropInfo
	{
		public Transform model;

		[System.Obsolete("Replaced by CargoSpawnTableRef which is only set on the server")]
		public ushort id;

		/// <summary>
		/// Current position.
		/// </summary>
		public Vector3 state;

		[System.Obsolete("Replaced by Velocity property")]
		public Vector3 direction;

		[System.Obsolete("Replaced by Velocity property")]
		public float speed;

		[System.Obsolete("Replaced by ServerTimeUntilDrop which is only set on the server")]
		public float delay;

		[System.Obsolete("Replaced by ServerConstantForce which is only set on the server")]
		public float force;

		[System.Obsolete("Replaced by ServerHasDeployedCarepackage which is only set on the server")]
		public bool dropped;

		[System.Obsolete("Replaced by ServerDropPosition which is only set on the server")]
		public Vector3 dropPosition;

		public Vector3 Velocity
		{
			get;
			set;
		}

		public CachingAssetRef ServerCargoSpawnTableRef
		{
			get;
			set;
		}

		public float ServerConstantForce
		{
#pragma warning disable
			get => force;
			set => force = value;
#pragma warning restore
		}

		public Vector3 ServerDropPosition
		{
#pragma warning disable
			get => dropPosition;
			set => dropPosition = value;
#pragma warning restore
		}

		public bool ServerHasDeployedCarepackage
		{
#pragma warning disable
			get => dropped;
			set => dropped = value;
#pragma warning restore
		}

		public float ServerTimeUntilDrop
		{
#pragma warning disable
			get => delay;
			set => delay = value;
#pragma warning restore
		}
	}

	public delegate void ArenaMessageUpdated(EArenaMessage newArenaMessage);
	public delegate void ArenaPlayerUpdated(ulong[] playerIDs, EArenaMessage newArenaMessage);
	public delegate void LevelNumberUpdated(int newLevelNumber);

	public class LevelManager : SteamCaller
	{
		public static readonly byte SAVEDATA_VERSION = 1;

		private static LevelManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static LevelManager instance => manager;

		private static bool isInit;
		private static ELevelType _levelType;
		public static ELevelType levelType => _levelType;

		/// <summary>
		/// Is the active level an Arena mode map?
		/// </summary>
		public static bool isArenaMode => levelType == ELevelType.ARENA;


		private static AudioClip timerClip;
		private static AudioClip GetOrLoadTimerClip()
		{
			if (timerClip == null)
			{
				timerClip = new AudioReference("core.masterbundle", "Sounds/Timer.mp3").LoadAudioClip();
			}

			return timerClip;
		}

		private static float lastFinaleMessage;
		private static float lastTimerMessage;
		private static float nextAreaModify;
		private static int countTimerMessages;
		public static EArenaState arenaState;
		public static EArenaMessage arenaMessage;
		private static int nonGroups;
		public static List<CSteamID> arenaGroups;
		public static List<ArenaPlayer> arenaPlayers;

		private static Vector3 _arenaCurrentCenter;
		public static Vector3 arenaCurrentCenter => _arenaCurrentCenter;

		private static Vector3 _arenaOriginCenter;
		public static Vector3 arenaOriginCenter => _arenaOriginCenter;

		private static Vector3 _arenaTargetCenter;
		public static Vector3 arenaTargetCenter => _arenaTargetCenter;

		private static float _arenaCurrentRadius;
		public static float arenaCurrentRadius => _arenaCurrentRadius;

		private static float _arenaOriginRadius;
		public static float arenaOriginRadius => _arenaOriginRadius;

		private static float _arenaTargetRadius;
		public static float arenaTargetRadius => _arenaTargetRadius;

		private static float _arenaCompactorSpeed;
		public static float arenaCompactorSpeed => _arenaCompactorSpeed;

		private static float arenaSqrRadius;
		private static Transform arenaCurrentArea;
		private static Transform arenaTargetArea;

		public static ArenaMessageUpdated onArenaMessageUpdated;
		public static ArenaPlayerUpdated onArenaPlayerUpdated;
		public static LevelNumberUpdated onLevelNumberUpdated;

		private static uint minPlayers
		{
			get
			{
				if (Dedicator.IsDedicatedServer)
				{
					return Provider.modeConfigData.Events.Arena_Min_Players;
				}
				else
				{
					return 1;
				}
			}
		}

		public static float compactorSpeed
		{
			get
			{
				switch (Level.info.size)
				{
					case ELevelSize.TINY:
						return Provider.modeConfigData.Events.Arena_Compactor_Speed_Tiny;

					case ELevelSize.SMALL:
						return Provider.modeConfigData.Events.Arena_Compactor_Speed_Small;

					case ELevelSize.MEDIUM:
						return Provider.modeConfigData.Events.Arena_Compactor_Speed_Medium;

					case ELevelSize.LARGE:
						return Provider.modeConfigData.Events.Arena_Compactor_Speed_Large;

					case ELevelSize.INSANE:
						return Provider.modeConfigData.Events.Arena_Compactor_Speed_Insane;

					default:
						return 0;
				}
			}
		}

		public static bool isPlayerInArena(Player player)
		{
			if (arenaState == EArenaState.CLEAR || arenaState == EArenaState.PLAY || arenaState == EArenaState.FINALE || arenaState == EArenaState.RESTART)
			{
				foreach (ArenaPlayer arenaPlayer in arenaPlayers)
				{
					if (arenaPlayer.steamPlayer != null && arenaPlayer.steamPlayer.player == player)
					{
						return true;
					}
				}
			}

			return false;
		}

		private void findGroups()
		{
			nonGroups = 0;
			arenaGroups.Clear();

			for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
			{
				SteamPlayer steamPlayer = Provider.clients[playerIndex];

				if (steamPlayer == null || steamPlayer.player == null || steamPlayer.player.life.isDead)
				{
					continue;
				}

				if (!steamPlayer.player.quests.isMemberOfAGroup)
				{
					nonGroups++;
				}
				else if (!arenaGroups.Contains(steamPlayer.player.quests.groupID))
				{
					arenaGroups.Add(steamPlayer.player.quests.groupID);
				}
			}
		}

		private void updateGroups(SteamPlayer steamPlayer)
		{
			if (!steamPlayer.player.quests.isMemberOfAGroup)
			{
				nonGroups--;
			}
			else
			{
				for (int index = arenaPlayers.Count - 1; index >= 0; index--)
				{
					ArenaPlayer arenaPlayer = arenaPlayers[index];

					if (arenaPlayer.steamPlayer.player.quests.isMemberOfSameGroupAs(steamPlayer.player))
					{
						return; // if there is a surviving team member then cancel
					}
				}

				arenaGroups.Remove(steamPlayer.player.quests.groupID); // remove group when everyone is dead
			}
		}

		private void arenaLobby()
		{
			findGroups();

			if (nonGroups + arenaGroups.Count < minPlayers)
			{
				if (arenaMessage != EArenaMessage.LOBBY)
				{
					SendArenaMessage.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), EArenaMessage.LOBBY);
				}

				return;
			}

			arenaState = EArenaState.CLEAR;
		}

		/// <summary>
		/// Find a new smaller circle within the old circle and clamp it to the playable level area.
		/// </summary>
		private void getArenaTarget(Vector3 currentCenter, float currentRadius, out Vector3 targetCenter, out float targetRadius)
		{
			targetCenter = currentCenter;
			targetRadius = currentRadius * Provider.modeConfigData.Events.Arena_Compactor_Shrink_Factor;

			float offsetAngleInRadians = Random.Range(0, Mathf.PI * 2);
			float offsetDirection_X = Mathf.Cos(offsetAngleInRadians);
			float offsetDirection_Z = Mathf.Sin(offsetAngleInRadians);

			// If currentRadius is 100 and shrink factor is 0.4 then targetRadius is 40
			// we can move the targetCenter by up to 100 - 40 = 60 in any direction while still being inside the circle
			float offsetDistance = Random.Range(0, currentRadius - targetRadius);

			targetCenter += new Vector3(offsetDirection_X * offsetDistance, 0, offsetDirection_Z * offsetDistance);

			// If targetCenter.x is 300 and targetRadius is 1000 with a level border of -500 we're -200 past the limit
			// If targetCenter.x is -400 and targetRadius is 250 with a level border of -500 we're -150 past the limit
			if (targetCenter.x - targetRadius < (-Level.size / 2) + Level.border)
			{
				targetRadius = targetCenter.x - ((-Level.size / 2) + Level.border);
			}

			// If targetCenter.x is 300 and targetRadius is 400 with a level edge of 500 we're 200 over the limit, subtract 200 and we're safe
			if (targetCenter.x + targetRadius > (Level.size / 2) - Level.border)
			{
				targetRadius = (Level.size / 2) - Level.border - targetCenter.x;
			}

			if (targetCenter.z - targetRadius < (-Level.size / 2) + Level.border)
			{
				targetRadius = targetCenter.z - ((-Level.size / 2) + Level.border);
			}

			if (targetCenter.z + targetRadius > (Level.size / 2) - Level.border)
			{
				targetRadius = (Level.size / 2) - Level.border - targetCenter.z;
			}
		}

		private void arenaClear()
		{
			AnimalManager.askClearAllAnimals();
			VehicleManager.askVehicleDestroyAll();
			BarricadeManager.askClearAllBarricades();
			StructureManager.askClearAllStructures();
			ItemManager.askClearAllItems();
			EffectManager.askEffectClearAll();
			ObjectManager.askClearAllObjects();
			ResourceManager.askClearAllResources();
			arenaPlayers.Clear();

			Vector3 newArenaCurrentCenter = Vector3.zero;
			float newArenaCurrentRadius = Level.size / 2.0f;
			if (Level.info.configData.Use_Arena_Compactor)
			{
				ArenaCompactorVolume node = ArenaCompactorVolumeManager.Get().GetRandomVolumeOrNull();
				if (node != null)
				{
					newArenaCurrentCenter = node.transform.position;
					newArenaCurrentCenter.y = 0;
					newArenaCurrentRadius = node.GetSphereRadius();
				}
			}
			else
			{
				newArenaCurrentRadius = 16384;
			}
			float newArenaCompactorSpeed = compactorSpeed;

			Vector3 newArenaTargetCenter;
			float newArenaTargetRadius;

			if (Level.info.configData.Use_Arena_Compactor)
			{
				if (Provider.modeConfigData.Events.Arena_Use_Compactor_Pause)
				{
					getArenaTarget(newArenaCurrentCenter, newArenaCurrentRadius, out newArenaTargetCenter, out newArenaTargetRadius);
				}
				else
				{
					newArenaTargetCenter = newArenaCurrentCenter;
					newArenaTargetRadius = 0.5f;
				}
			}
			else
			{
				newArenaTargetCenter = newArenaCurrentCenter;
				newArenaTargetRadius = newArenaCurrentRadius;
			}

			SendArenaOrigin.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), newArenaCurrentCenter, newArenaCurrentRadius, newArenaCurrentCenter, newArenaCurrentRadius, newArenaTargetCenter, newArenaTargetRadius, newArenaCompactorSpeed, (byte) (Provider.modeConfigData.Events.Arena_Clear_Timer + Provider.modeConfigData.Events.Arena_Compactor_Delay_Timer));

			arenaState = EArenaState.WARMUP;
			SendLevelTimer.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), (byte) Provider.modeConfigData.Events.Arena_Clear_Timer);
		}

		private void arenaWarmUp()
		{
			if (arenaMessage != EArenaMessage.WARMUP)
			{
				SendArenaMessage.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), EArenaMessage.WARMUP);
			}

			if (countTimerMessages >= 0)
			{
				return;
			}
			else
			{
				findGroups();

				if (nonGroups + arenaGroups.Count < minPlayers)
				{
					arenaState = EArenaState.LOBBY;
				}
				else
				{
					arenaState = EArenaState.SPAWN;
				}
			}
		}

		private void arenaSpawn()
		{
			for (byte x = 0; x < Regions.WORLD_SIZE; x++)
			{
				for (byte y = 0; y < Regions.WORLD_SIZE; y++)
				{
					if (LevelItems.spawns[x, y].Count > 0)
					{
						for (int index = 0; index < LevelItems.spawns[x, y].Count; index++)
						{
							ItemSpawnpoint itemSpawn = LevelItems.spawns[x, y][index];
							ushort id = LevelItems.getItem(itemSpawn);

							if (id != 0)
							{
								Item item = new Item(id, EItemOrigin.ADMIN);
								ItemManager.dropItem(item, itemSpawn.point, false, false, false);
							}
						}
					}
				}
			}

			List<VehicleSpawnpoint> vehicleSpawns = LevelVehicles.spawns;

			for (int vehicleIndex = 0; vehicleIndex < vehicleSpawns.Count; vehicleIndex++)
			{
				VehicleSpawnpoint vehicleSpawn = vehicleSpawns[vehicleIndex];
				Asset vehicleAsset = LevelVehicles.GetRandomAssetForSpawnpoint(vehicleSpawn);
				
				if (vehicleAsset != null)
				{
					Vector3 point = vehicleSpawn.point;
					point.y++;

					InteractableVehicle spawnedVehicle = VehicleManager.spawnVehicleV2(vehicleAsset, point, Quaternion.Euler(0, vehicleSpawn.angle, 0));
					if (spawnedVehicle != null)
					{
						spawnedVehicle.WasNaturallySpawned = true;
					}
				}
			}

			// Spawn animals at all available spawn points
			List<AnimalSpawnpoint> animalSpawns = LevelAnimals.spawns;
			foreach (AnimalSpawnpoint animalSpawn in animalSpawns)
			{
				ushort id = LevelAnimals.getAnimal(animalSpawn);
				if (id == 0)
					continue;

				Vector3 position = animalSpawn.point;
				position.y += 0.1f;

				AnimalManager.spawnAnimal(id, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
			}

			List<PlayerSpawnpoint> playerSpawns = LevelPlayers.getAltSpawns();

			float removeSqrRadius = arenaCurrentRadius - SafezoneNode.MIN_SIZE;
			removeSqrRadius *= removeSqrRadius;

			for (int spawnIndex = playerSpawns.Count - 1; spawnIndex >= 0; spawnIndex--)
			{
				PlayerSpawnpoint playerSpawn = playerSpawns[spawnIndex];

				float distance = MathfEx.HorizontalDistanceSquared(playerSpawn.point, arenaCurrentCenter);
				if (distance > removeSqrRadius)
				{
					playerSpawns.RemoveAt(spawnIndex);
				}
			}

			List<SteamPlayer> potentialPlayers = new List<SteamPlayer>(Provider.clients); // Copy clients list.
			while (playerSpawns.Count > 0)
			{
				if (potentialPlayers.Count == 0)
					break; // Ran out of players to spawn.

				// Pick a random player for this spawnpoint so that servers with more players than spawns
				// get a chance to play somewhat evenly.
				int playerIndex = Random.Range(0, potentialPlayers.Count);
				SteamPlayer steamPlayer = potentialPlayers[playerIndex];
				potentialPlayers.RemoveAtFast(playerIndex);

				if (steamPlayer == null || steamPlayer.player == null || steamPlayer.player.life.isDead)
					continue;

				int spawnIndex = Random.Range(0, playerSpawns.Count);
				PlayerSpawnpoint playerSpawn = playerSpawns[spawnIndex];
				playerSpawns.RemoveAt(spawnIndex);

				ArenaPlayer arenaPlayer = new ArenaPlayer(steamPlayer);
				arenaPlayer.steamPlayer.player.life.sendRevive();
				arenaPlayer.steamPlayer.player.teleportToLocationUnsafe(playerSpawn.point, playerSpawn.angle);
				arenaPlayers.Add(arenaPlayer);

				foreach (ArenaLoadout loadout in Level.info.configData.Arena_Loadouts)
				{
					for (ushort amount = 0; amount < loadout.Amount; amount++)
					{
						ushort itemID = SpawnTableTool.ResolveLegacyId(loadout.Table_ID, EAssetType.ITEM, OnGetArenaLoadoutsSpawnTableErrorContext);
						if (itemID != 0)
						{
							arenaPlayer.steamPlayer.player.inventory.forceAddItemAuto(new Item(itemID, true), true, false, true, false);
						}
					}
				}
			}

			arenaAirdrop();

			arenaState = EArenaState.PLAY;
			SendLevelNumber.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), (byte) arenaPlayers.Count);
		}

		private string OnGetArenaLoadoutsSpawnTableErrorContext()
		{
			return "level config arena loadout";
		}

		private void arenaAirdrop()
		{
			if (!Provider.modeConfigData.Events.Use_Airdrops)
				return;

			AirdropDevkitNode node = GetRandomArenaAirdropNode();
			if (node != null)
			{
				SpawnAirdropAtNode(node);
			}
		}

		private void arenaPlay()
		{
			if (arenaMessage != EArenaMessage.PLAY)
			{
				SendArenaMessage.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), EArenaMessage.PLAY);
			}

			if (nonGroups + arenaGroups.Count < minPlayers)
			{
				arenaState = EArenaState.FINALE;
				lastFinaleMessage = Time.realtimeSinceStartup;

				if (arenaPlayers.Count > 0)
				{
					ulong[] playersIDs = new ulong[arenaPlayers.Count];
					for (int index = 0; index < arenaPlayers.Count; index++)
					{
						playersIDs[index] = arenaPlayers[index].steamPlayer.playerID.steamID.m_SteamID;
					}

					arenaMessage = EArenaMessage.LOSE;
					SendArenaPlayer.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(),
						SendArenaPlayer_Write, playersIDs, EArenaMessage.WIN);
				}
				else
				{
					SendArenaMessage.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), EArenaMessage.LOSE);
				}
			}
			else
			{
				float currentTime = Time.time;
				float deltaTime = Time.deltaTime;
				for (int index = arenaPlayers.Count - 1; index >= 0; index--)
				{
					ArenaPlayer arenaPlayer = arenaPlayers[index];

					if (arenaPlayer.steamPlayer == null || arenaPlayer.steamPlayer.player == null)
					{
						ulong[] playersIDs = new ulong[1];
						playersIDs[0] = arenaPlayer.steamPlayer.playerID.steamID.m_SteamID;

						SendArenaPlayer.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(),
							SendArenaPlayer_Write, playersIDs, EArenaMessage.ABANDONED);

						arenaPlayers.RemoveAt(index);
						updateGroups(arenaPlayer.steamPlayer);
						SendLevelNumber.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), (byte) arenaPlayers.Count);
					}
					else
					{
						float distance = MathfEx.HorizontalDistanceSquared(arenaPlayer.steamPlayer.player.transform.position, arenaCurrentCenter);
						bool outsideArea = distance > arenaSqrRadius || arenaCurrentRadius < 1.0f;

						if (outsideArea)
						{
							float timeSinceLastDamage = currentTime - arenaPlayer.lastAreaDamage;
							if (timeSinceLastDamage > 1.0f)
							{
								// Players in creative servers can spam medical items to escape to the safezone,
								// in which case the safezone protection should be ignored to eventually kill them.
								const bool bypassSafezone = true;

								float extraDamage = Provider.modeConfigData.Events.Arena_Compactor_Extra_Damage_Per_Second * arenaPlayer.timeOutsideArea;
								float totalDamage = Provider.modeConfigData.Events.Arena_Compactor_Damage + extraDamage;
								byte roundedDamage = MathfEx.RoundAndClampToByte(totalDamage);

								EPlayerKill kill;
								arenaPlayer.steamPlayer.player.life.askDamage(roundedDamage, Vector3.up * 10, EDeathCause.ARENA, ELimb.SPINE, CSteamID.Nil, out kill, bypassSafezone: bypassSafezone);

								arenaPlayer.lastAreaDamage = currentTime;
							}

							arenaPlayer.timeOutsideArea += deltaTime;
						}
						else
						{
							arenaPlayer.timeOutsideArea = 0.0f;
						}

						if (arenaPlayer.hasDied)
						{
							ulong[] playersIDs = new ulong[1];
							playersIDs[0] = arenaPlayer.steamPlayer.playerID.steamID.m_SteamID;

							SendArenaPlayer.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(),
								SendArenaPlayer_Write, playersIDs, EArenaMessage.DIED);
							//arenaPlayer.steamPlayer.player.skills.askAward(25); // 25 XP for dying
							arenaPlayers.RemoveAt(index);
							updateGroups(arenaPlayer.steamPlayer);
							SendLevelNumber.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), (byte) arenaPlayers.Count);
						}
					}
				}
			}
		}

		private void arenaFinale()
		{
			if (Time.realtimeSinceStartup - lastFinaleMessage > Provider.modeConfigData.Events.Arena_Finale_Timer)
			{
				arenaState = EArenaState.RESTART;
			}
		}

		private void arenaRestart()
		{
			arenaState = EArenaState.INTERMISSION;
			SendLevelTimer.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), (byte) Provider.modeConfigData.Events.Arena_Restart_Timer);

			for (int index = arenaPlayers.Count - 1; index >= 0; --index)
			{
				ArenaPlayer arenaPlayer = arenaPlayers[index];

				if (arenaPlayer.hasDied || arenaPlayer.steamPlayer == null || arenaPlayer.steamPlayer.player == null)
				{
					continue;
				}

				arenaPlayer.steamPlayer.player.sendStat(EPlayerStat.ARENA_WINS);

				// Players in creative servers can spam medical items to escape to the safezone,
				// in which case the safezone protection should be ignored to properly reset.
				const bool bypassSafezone = true;

				EPlayerKill kill;
				arenaPlayer.steamPlayer.player.life.askDamage(101, Vector3.up * 101, EDeathCause.ARENA, ELimb.SPINE, CSteamID.Nil, out kill, bypassSafezone: bypassSafezone);
			}
		}

		private void arenaIntermission()
		{
			if (arenaMessage != EArenaMessage.INTERMISSION)
			{
				SendArenaMessage.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), EArenaMessage.INTERMISSION);
			}

			if (countTimerMessages >= 0)
			{
				return;
			}
			else
			{
				arenaState = EArenaState.LOBBY;
			}
		}

		private void arenaTick()
		{
			if (Time.realtimeSinceStartup > nextAreaModify)
			{
				_arenaCurrentRadius = arenaCurrentRadius - (Time.deltaTime * arenaCompactorSpeed);
				if (arenaCurrentRadius < arenaTargetRadius)
				{
					_arenaCurrentRadius = arenaTargetRadius;

					if (Provider.isServer && Level.info.configData.Use_Arena_Compactor && Provider.modeConfigData.Events.Arena_Use_Compactor_Pause)
					{
						float newArenaCompactorSpeed = compactorSpeed;

						Vector3 newArenaTargetCenter;
						float newArenaTargetRadius;
						getArenaTarget(arenaTargetCenter, arenaTargetRadius, out newArenaTargetCenter, out newArenaTargetRadius);

						SendArenaOrigin.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), arenaTargetCenter, arenaTargetRadius, arenaTargetCenter, arenaTargetRadius, newArenaTargetCenter, newArenaTargetRadius, newArenaCompactorSpeed, (byte) Provider.modeConfigData.Events.Arena_Compactor_Pause_Timer);
					}
				}

				arenaSqrRadius = arenaCurrentRadius * arenaCurrentRadius;

				float alpha = Mathf.InverseLerp(arenaTargetRadius, arenaOriginRadius, arenaCurrentRadius);
				_arenaCurrentCenter = Vector3.Lerp(arenaTargetCenter, arenaOriginCenter, alpha);
			}

			if (!Dedicator.IsDedicatedServer)
			{
				// 2022-03-14: this was equal to Level.HEIGHT (1024), but after the far clip plane distance reduction
				// the top of the wall gets cut off so I decided to reduce height to something reasonable for most maps.
				const float wallHeight = 300.0f;

				if (arenaCurrentArea != null)
				{
					arenaCurrentArea.position = arenaCurrentCenter;
					arenaCurrentArea.localScale = new Vector3(arenaCurrentRadius, wallHeight, arenaCurrentRadius);
				}

				if (arenaTargetArea != null)
				{
					arenaTargetArea.position = arenaTargetCenter;
					arenaTargetArea.localScale = new Vector3(arenaTargetRadius, wallHeight, arenaTargetRadius);
				}
			}

			if (countTimerMessages >= 0)
			{
				if (Time.realtimeSinceStartup - lastTimerMessage > 1.0f)
				{
					onLevelNumberUpdated?.Invoke(countTimerMessages);

					lastTimerMessage = Time.realtimeSinceStartup;
					countTimerMessages--;

					if (arenaMessage == EArenaMessage.WARMUP)
					{
						if (!Dedicator.IsDedicatedServer && MainCamera.instance != null && OptionsSettings.timer)
						{
							MainCamera.instance.GetComponent<AudioSource>().PlayOneShot(GetOrLoadTimerClip(), 1.0f);
						}
					}
				}
			}

			if (Provider.isServer)
			{
				switch (arenaState)
				{
					case EArenaState.LOBBY:
						arenaLobby();
						break;
					case EArenaState.CLEAR:
						arenaClear();
						break;
					case EArenaState.WARMUP:
						arenaWarmUp();
						break;
					case EArenaState.SPAWN:
						arenaSpawn();
						break;
					case EArenaState.PLAY:
						arenaPlay();
						break;
					case EArenaState.FINALE:
						arenaFinale();
						break;
					case EArenaState.RESTART:
						arenaRestart();
						break;
					case EArenaState.INTERMISSION:
						arenaIntermission();
						break;
				}
			}
		}

		private void arenaInit()
		{
			_arenaCurrentCenter = Vector3.zero;
			_arenaTargetCenter = Vector3.zero;
			_arenaCurrentRadius = 16384;
			_arenaTargetRadius = 16384;
			_arenaCompactorSpeed = 0;

			if (!Dedicator.IsDedicatedServer && !Level.isEditor)
			{
				arenaCurrentArea = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Arena_Area_Current"))).transform;
				arenaCurrentArea.name = "Arena_Area_Current";
				arenaCurrentArea.parent = Level.clips;

				arenaTargetArea = ((GameObject) GameObject.Instantiate(Resources.Load("Level/Arena_Area_Target"))).transform;
				arenaTargetArea.name = "Arena_Area_Target";
				arenaTargetArea.parent = Level.clips;
			}

			if (Provider.isServer)
			{
				arenaState = EArenaState.LOBBY;
				arenaGroups = new List<CSteamID>();
				arenaPlayers = new List<ArenaPlayer>();
			}
		}

		[System.Obsolete]
		public void tellArenaOrigin(CSteamID steamID, Vector3 newArenaCurrentCenter, float newArenaCurrentRadius, Vector3 newArenaOriginCenter, float newArenaOriginRadius, Vector3 newArenaTargetCenter, float newArenaTargetRadius, float newArenaCompactorSpeed, byte delay)
		{
			ReceiveArenaOrigin(newArenaCurrentCenter, newArenaCurrentRadius, newArenaOriginCenter, newArenaOriginRadius, newArenaTargetCenter, newArenaTargetRadius, newArenaCompactorSpeed, delay);
		}

		private static readonly ClientStaticMethod<Vector3, float, Vector3, float, Vector3, float, float, byte> SendArenaOrigin
			= ClientStaticMethod<Vector3, float, Vector3, float, Vector3, float, float, byte>.Get(ReceiveArenaOrigin);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellArenaOrigin))]
		public static void ReceiveArenaOrigin(Vector3 newArenaCurrentCenter, float newArenaCurrentRadius, Vector3 newArenaOriginCenter, float newArenaOriginRadius, Vector3 newArenaTargetCenter, float newArenaTargetRadius, float newArenaCompactorSpeed, byte delay)
		{
			_arenaCurrentCenter = newArenaCurrentCenter;
			_arenaCurrentRadius = newArenaCurrentRadius;
			arenaSqrRadius = arenaCurrentRadius * arenaCurrentRadius;
			_arenaOriginCenter = newArenaOriginCenter;
			_arenaOriginRadius = newArenaOriginRadius;
			_arenaTargetCenter = newArenaTargetCenter;
			_arenaTargetRadius = newArenaTargetRadius;
			_arenaCompactorSpeed = newArenaCompactorSpeed;

			if (delay == 0)
			{
				nextAreaModify = 0;
			}
			else
			{
				nextAreaModify = Time.realtimeSinceStartup + delay;
			}
		}

		[System.Obsolete]
		public void tellArenaMessage(CSteamID steamID, byte newArenaMessage)
		{
			ReceiveArenaMessage((EArenaMessage) newArenaMessage);
		}

		private static readonly ClientStaticMethod<EArenaMessage> SendArenaMessage = ClientStaticMethod<EArenaMessage>.Get(ReceiveArenaMessage);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellArenaMessage))]
		public static void ReceiveArenaMessage(EArenaMessage newArenaMessage)
		{
			arenaMessage = newArenaMessage;

			onArenaMessageUpdated?.Invoke(arenaMessage);
		}

		[System.Obsolete]
		public void tellArenaPlayer(CSteamID steamID, ulong[] newPlayerIDs, byte newArenaMessage)
		{
			onArenaPlayerUpdated?.Invoke(newPlayerIDs, (EArenaMessage) newArenaMessage);
		}

		private static readonly ClientStaticMethod SendArenaPlayer = ClientStaticMethod.Get(ReceiveArenaPlayer);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveArenaPlayer(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			byte newPlayerIDsLength;
			reader.ReadUInt8(out newPlayerIDsLength);
			ulong[] newPlayerIDs = new ulong[newPlayerIDsLength];
			for (byte index = 0; index < newPlayerIDsLength; ++index)
			{
				reader.ReadUInt64(out newPlayerIDs[index]);
			}
			EArenaMessage newArenaMessage;
			reader.ReadEnum(out newArenaMessage);

			onArenaPlayerUpdated?.Invoke(newPlayerIDs, newArenaMessage);
		}

		private static void SendArenaPlayer_Write(NetPakWriter writer, ulong[] newPlayerIDs, EArenaMessage newArenaMessage)
		{
			byte newPlayerIDsLength = (byte) newPlayerIDs.Length;
			writer.WriteUInt8(newPlayerIDsLength);
			for (byte index = 0; index < newPlayerIDsLength; ++index)
			{
				writer.WriteUInt64(newPlayerIDs[index]);
			}
			writer.WriteEnum(newArenaMessage);
		}

		[System.Obsolete]
		public void tellLevelNumber(CSteamID steamID, byte newLevelNumber)
		{
			ReceiveLevelNumber(newLevelNumber);
		}

		private static readonly ClientStaticMethod<byte> SendLevelNumber = ClientStaticMethod<byte>.Get(ReceiveLevelNumber);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLevelNumber))]
		public static void ReceiveLevelNumber(byte newLevelNumber)
		{
			countTimerMessages = -1;

			onLevelNumberUpdated?.Invoke(newLevelNumber);
		}

		[System.Obsolete]
		public void tellLevelTimer(CSteamID steamID, byte newTimerCount)
		{
			ReceiveLevelTimer(newTimerCount);
		}

		private static readonly ClientStaticMethod<byte> SendLevelTimer = ClientStaticMethod<byte>.Get(ReceiveLevelTimer);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLevelTimer))]
		public static void ReceiveLevelTimer(byte newTimerCount)
		{
			countTimerMessages = newTimerCount;
		}

		[System.Obsolete]
		public void askArenaState(CSteamID steamID)
		{ }

		private static List<AirdropDevkitNode> airdropNodes;
		private static List<AirdropInfo> airdrops;

		public static uint airdropFrequency;
		private static bool _hasAirdrop;
		public static bool hasAirdrop => _hasAirdrop;
		private static float lastAirdrop;

		public static void SpawnAirdrop(Vector3 dropPosition, SpawnAsset cargoSpawnTable)
		{
			float speed = Provider.modeConfigData.Events.Airdrop_Speed;
			InternalSpawnAirdrop(dropPosition, cargoSpawnTable, speed);
		}

		private static AirdropDevkitNode GetRandomArenaAirdropNode()
		{
			Vector3 airdropCenter = arenaTargetCenter;
			float airdropRadius = arenaTargetRadius;
			float sqrAirdropRadius = airdropRadius * airdropRadius;

			List<AirdropDevkitNode> validAirdropNodes = new List<AirdropDevkitNode>();
			foreach (AirdropDevkitNode validNode in airdropNodes)
			{
				if ((validNode.transform.position - airdropCenter).sqrMagnitude < sqrAirdropRadius)
				{
					validAirdropNodes.Add(validNode);
				}
			}

			if (validAirdropNodes.Count == 0)
			{
				return null;
			}

			return validAirdropNodes[Random.Range(0, validAirdropNodes.Count)];
		}

		/// <summary>
		/// Pick a random airdrop node appropriate for the game mode.
		/// </summary>
		public static AirdropDevkitNode GetRandomAirdropNode()
		{
			if (airdropNodes == null || airdropNodes.Count < 1)
			{
				return null;
			}

			if (_levelType == ELevelType.ARENA)
			{
				return GetRandomArenaAirdropNode();
			}
			else
			{
				return airdropNodes[Random.Range(0, airdropNodes.Count)];
			}
		}

		public static void SpawnAirdropAtNode(AirdropDevkitNode node)
		{
			if (node == null)
				throw new System.ArgumentNullException(nameof(node));

			SpawnAsset cargoSpawnTable = node.GetCargoSpawnTableOrLogWarning();
			if (cargoSpawnTable != null)
			{
				SpawnAirdrop(node.transform.position, cargoSpawnTable);
			}
		}

		public static void airdrop(Vector3 point, ushort id, float speed)
		{
			if (id == 0)
			{
				return;
			}

			SpawnAsset spawnAsset = Assets.find(EAssetType.SPAWN, id) as SpawnAsset;
			if (spawnAsset == null)
			{
				return;
			}

			InternalSpawnAirdrop(point, spawnAsset, speed);
		}

		private static void InternalSpawnAirdrop(Vector3 dropPosition, SpawnAsset cargoSpawnTable, float speed)
		{
			if (cargoSpawnTable == null)
				throw new System.ArgumentNullException(nameof(cargoSpawnTable));

			Vector3 startingPosition = Vector3.zero;
			if (Random.value < 0.5f) // horizontal approach e.g. from East to West
			{
				startingPosition.x = Level.size / 2 * -Mathf.Sign(dropPosition.x);
				startingPosition.z = Random.Range(0, Level.size / 2) * -Mathf.Sign(dropPosition.z);
			}
			else // vertical approach e.g. from North to South
			{
				startingPosition.x = Random.Range(0, Level.size / 2) * -Mathf.Sign(dropPosition.x);
				startingPosition.z = Level.size / 2 * -Mathf.Sign(dropPosition.z);
			}

			float flightHeight = dropPosition.y + Random.Range(450.0f, 475.0f);
			dropPosition.y = 0;
			Vector3 direction = (dropPosition - startingPosition).normalized;
			startingPosition += direction * -2048.0f;
			float timeUntilDrop = (dropPosition - startingPosition).magnitude / speed; // delay is calculated here because we don't send the drop coordinate
			startingPosition.y = flightHeight;
			dropPosition.y = flightHeight;
			Vector3 velocity = direction * speed;

			SendAirdropState.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), startingPosition, velocity);

			AirdropInfo airdropInfo = airdrops.Count > 0 ? airdrops[airdrops.Count - 1] : null;
			if (airdropInfo == null)
			{
				UnturnedLog.error("Adding AirdropInfo failed");
				return;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Vector3 replicatedDropPosition = airdropInfo.state + airdropInfo.Velocity * timeUntilDrop;
			float horizontalError = (replicatedDropPosition - dropPosition).GetHorizontalMagnitude();
			if (horizontalError >= 0.1f)
			{
				UnturnedLog.warn($"Significant discrepency between client and server airdrop trajectory: {horizontalError}");
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

#pragma warning disable
			// Backwards compatibility in case mods are using it.
			airdropInfo.direction = direction;
			airdropInfo.speed = speed;
#pragma warning restore

			airdropInfo.ServerHasDeployedCarepackage = false;
			airdropInfo.ServerCargoSpawnTableRef = cargoSpawnTable;
			airdropInfo.ServerConstantForce = Provider.modeConfigData.Events.Airdrop_Force;
			airdropInfo.ServerDropPosition = dropPosition;
			airdropInfo.ServerTimeUntilDrop = timeUntilDrop;
		}

		private void AirdropUpdate()
		{
			float deltaTime = Time.deltaTime;
			for (int index = airdrops.Count - 1; index >= 0; index--)
			{
				AirdropInfo info = airdrops[index];

				info.state += info.Velocity * deltaTime;

				if (info.model != null)
				{
					info.model.position = info.state;
				}

				if (Provider.isServer && !info.ServerHasDeployedCarepackage)
				{
					info.ServerTimeUntilDrop -= deltaTime;
					if (info.ServerTimeUntilDrop <= 0)
					{
						info.ServerHasDeployedCarepackage = true;

						Vector3 dropPosition = info.ServerDropPosition;
						SpawnAsset spawnTable = info.ServerCargoSpawnTableRef.Get<SpawnAsset>();
						float constantForce = info.ServerConstantForce;

						SpawnCarepackage(dropPosition, spawnTable, constantForce);
						SendSpawnCarepackage.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), dropPosition, constantForce);

						if (Dedicator.IsDedicatedServer)
						{
							// Dedicated server does not spawn a model for the airplane.
							airdrops.RemoveAt(index);
							continue;
						}
					}
				}

				// For listen server (singleplayer) and clients we remove the airplane model after it leaves the level.
				//
				// For example, if plane starts at -3000 and has direction.x = 0.5 then -3000*1 = -3000
				// or, if plane starts at 3000 and has direction.x = -0.5 then 2000 * -1 = -2000
				// only once it passes to the other side will it be greater than threshold.
				float dir_x = Mathf.Sign(info.Velocity.x);
				float dir_z = Mathf.Sign(info.Velocity.z);
				if (info.state.x * dir_x > (Level.size / 2) + 2048 || info.state.z * dir_z > (Level.size / 2) + 2048)
				{
					if (info.model != null)
					{
						Destroy(info.model.gameObject);
					}

					airdrops.RemoveAt(index);
				}
			}

			if (Provider.isServer && levelType == ELevelType.SURVIVAL && Provider.modeConfigData.Events.Use_Airdrops)
			{
				if (airdropNodes.Count > 0)
				{
					if (!hasAirdrop)
					{
						airdropFrequency = (uint) (Random.Range(Provider.modeConfigData.Events.Airdrop_Frequency_Min, Provider.modeConfigData.Events.Airdrop_Frequency_Max) * LightingManager.cycle);

						_hasAirdrop = true;
						lastAirdrop = Time.realtimeSinceStartup;
					}

					if (airdropFrequency > 0)
					{
						if (Time.realtimeSinceStartup - lastAirdrop > 1.0f)
						{
							airdropFrequency--;
							lastAirdrop = Time.realtimeSinceStartup;
						}
					}
					else
					{
						AirdropDevkitNode node = airdropNodes[Random.Range(0, airdropNodes.Count)];
						SpawnAirdropAtNode(node);

						_hasAirdrop = false;
					}
				}
			}
		}

		private void airdropInit()
		{
			lastAirdrop = Time.realtimeSinceStartup;
			airdrops = new List<AirdropInfo>();

			if (Provider.isServer)
			{
				airdropNodes = new List<AirdropDevkitNode>();
				foreach (AirdropDevkitNode node in AirdropDevkitNodeSystem.Get().GetAllNodes())
				{
					if (node.CargoSpawnTableRef.IsAssigned)
					{
						airdropNodes.Add(node);
					}
				}

				load();
			}
		}

		private void AddAirdropInfo(Vector3 position, Vector3 velocity)
		{
			AirdropInfo info = new AirdropInfo();
			info.state = position;
			info.Velocity = velocity;

			if (!Dedicator.IsDedicatedServer)
			{
				LevelAsset levelAsset = Level.getAsset();
				MasterBundleReference<GameObject> dropshipPrefab = levelAsset != null ? levelAsset.dropshipPrefab : new MasterBundleReference<GameObject>();
				if (dropshipPrefab.isNull)
				{
					dropshipPrefab = new MasterBundleReference<GameObject>("core.masterbundle", "Level/Dropship.prefab");
				}

				Quaternion spawnRotation = Quaternion.LookRotation(velocity) * Quaternion.Euler(-90.0f, 180.0f, 0.0f);
				Transform model = Instantiate(dropshipPrefab.loadAsset(), position, spawnRotation).transform;
				model.name = "Dropship";
				info.model = model;
			}

			airdrops.Add(info);
		}

		private static readonly ClientStaticMethod<Vector3, Vector3> SendAirdropState = ClientStaticMethod<Vector3, Vector3>.Get(ReceiveAirdropState);
		/// <summary>
		/// Nelson 2025-04-01: default position intBitCount of 13 has range of [-4096, 4096), but on "insane" size maps
		/// the aircraft starts 2 km outside that range. This causes the care package to spawn at the wrong position.
		/// Bumping intBitCount to 14 enables a range of [-8192, 8192). (public issue #4972)
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveAirdropState([NetPakVector3(intBitCount: 14)] Vector3 position, [NetPakVectorAsYaw(yawBitCount: 24)] Vector3 velocity)
		{
			manager.AddAirdropInfo(position, velocity);
		}

		private static readonly ClientStaticMethod<Vector3, float> SendSpawnCarepackage = ClientStaticMethod<Vector3, float>.Get(ReceiveSpawnCarepackage);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveSpawnCarepackage(Vector3 position, float constantForce)
		{
			SpawnCarepackage(position, null, constantForce);
		}

		private static void SpawnCarepackage(Vector3 position, SpawnAsset cargoSpawnTable, float constantForce)
		{
			LevelAsset levelAsset = Level.getAsset();
			AssetReference<AirdropAsset> airdropRef = levelAsset != null ? levelAsset.airdropRef : AssetReference<AirdropAsset>.invalid;
			if (airdropRef.isNull)
			{
				airdropRef = AirdropAsset.defaultAirdrop;
			}

			AirdropAsset airdrop = airdropRef.Find();

			MasterBundleReference<GameObject> carepackagePrefab = airdrop != null ? airdrop.model : MasterBundleReference<GameObject>.invalid;
			if (carepackagePrefab.isNull)
			{
				carepackagePrefab = new MasterBundleReference<GameObject>("core.masterbundle", "Level/Carepackage.prefab");
			}

			Transform carepackage = Instantiate(carepackagePrefab.loadAsset(), position, Quaternion.identity).transform;
			carepackage.name = "Carepackage";

			// On clients the behavior destroys the airdrop visual, whereas on server it spawns the barricade.
			Carepackage carepackageBehavior = carepackage.GetOrAddComponent<Carepackage>();
			carepackageBehavior.cargoSpawnTable = cargoSpawnTable;

			if (airdrop != null)
			{
				carepackageBehavior.barricadeAsset = airdrop.barricadeRef.Find();
			}

			ConstantForce cf = carepackage.GetComponent<ConstantForce>();
			if (cf != null)
			{
				cf.force = new Vector3(0, constantForce, 0);
			}
		}

		[System.Obsolete]
		public void askAirdropState(CSteamID steamID)
		{ }

		internal static void SendInitialGlobalState(SteamPlayer client)
		{
			if (Level.info.type == ELevelType.ARENA)
			{
				SendArenaOrigin.Invoke(ENetReliability.Reliable, client.transportConnection, arenaCurrentCenter, arenaCurrentRadius, arenaOriginCenter, arenaOriginRadius, arenaTargetCenter, arenaTargetRadius, arenaCompactorSpeed, 0);
				SendArenaMessage.Invoke(ENetReliability.Reliable, client.transportConnection, arenaMessage);

				if (countTimerMessages > 0)
				{
					SendLevelTimer.Invoke(ENetReliability.Reliable, client.transportConnection, (byte) countTimerMessages);
				}
				else
				{
					SendLevelNumber.Invoke(ENetReliability.Reliable, client.transportConnection, (byte) arenaPlayers.Count);
				}
			}

			for (int index = 0; index < airdrops.Count; index++)
			{
				AirdropInfo info = airdrops[index];

				SendAirdropState.Invoke(ENetReliability.Reliable, client.transportConnection, info.state, info.Velocity);
			}
		}

		private void onLevelLoaded(int level)
		{
			isInit = false;

			if (level > Level.BUILD_INDEX_SETUP)
			{
				if (Level.info != null)
				{
					isInit = true;
					_levelType = Level.info.type;

					if (levelType == ELevelType.ARENA)
					{
						arenaInit();
					}

					if (levelType != ELevelType.HORDE)
					{
						airdropInit();
					}
				}
			}
		}

		private void Update()
		{
			if (!isInit)
			{
				return;
			}

			if (levelType == ELevelType.ARENA)
			{
				arenaTick();
			}

			if (levelType != ELevelType.HORDE)
			{
				AirdropUpdate();
			}
		}

		private void Start()
		{
			manager = this;

			Level.onLevelLoaded += onLevelLoaded;
		}

		public static void load()
		{
			bool useDefaults = true;

			if (LevelSavedata.fileExists("/Events.dat"))
			{
				River river = LevelSavedata.openRiver("/Events.dat", true);
				byte version = river.readByte();

				if (version > 0)
				{
					airdropFrequency = river.readUInt32();
					_hasAirdrop = river.readBoolean();

					useDefaults = false;
				}

				river.closeRiver();
			}

			if (useDefaults)
			{
				_hasAirdrop = false;
			}
		}

		public static void save()
		{
			River river = LevelSavedata.openRiver("/Events.dat", false);
			river.writeByte(SAVEDATA_VERSION);
			river.writeUInt32(airdropFrequency);
			river.writeBoolean(hasAirdrop);
			river.closeRiver();
		}
	}
}
