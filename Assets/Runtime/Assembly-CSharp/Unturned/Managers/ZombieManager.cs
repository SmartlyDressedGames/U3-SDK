////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void WaveUpdated(bool newWaveReady, int newWaveIndex);

	public class ZombieManager : SteamCaller
	{
		public static AudioClip[] roars;
		public static AudioClip[] groans;
		public static AudioClip[] spits;

		public static AudioClip[] dl_attacks;
		public static AudioClip[] dl_deaths;
		public static AudioClip[] dl_enemy_spotted;
		public static AudioClip[] dl_taunt;

		private static ZombieManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static ZombieManager instance => manager;

		private static ZombieRegion[] _regions;
		public static ZombieRegion[] regions => _regions;
		internal static HashSet<int> regionsWithPlayers;
		
		private static Dictionary<string, double> cooldowns = new Dictionary<string, double>(System.StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// False if time since this was last called with same cooldownId is less than duration.
		/// True otherwise.
		/// </summary>
		public static bool CheckCustomCooldown(string cooldownId, double duration)
		{
			if (string.IsNullOrEmpty(cooldownId) || duration <= 0.0)
			{
				return false;
			}

			double now = Time.timeAsDouble;
			if (cooldowns.TryGetValue(cooldownId, out double lastRun) && now - lastRun < duration)
			{
				return false;
			}

			cooldowns[cooldownId] = now;
			return true;
		}

		public static int wanderingCount;
		private static int tickIndex;
		private static List<Zombie> _tickingZombies;
		public static List<Zombie> tickingZombies => _tickingZombies;

		public static List<Zombie> AllZombies
		{
			get;
			private set;
		}

		public static bool canSpareWanderer => wanderingCount < 8 && tickingZombies.Count < 50;

		private static byte respawnZombiesBound;
		private static float lastWave;

		private static bool _waveReady;
		public static bool waveReady => _waveReady;

		private static int _waveIndex;
		public static int waveIndex => _waveIndex;

		private static int _waveRemaining;
		public static int waveRemaining => _waveRemaining;

		private static float lastTick;

		public static WaveUpdated onWaveUpdated;

		public static void getZombiesInRadius(Vector3 center, float sqrRadius, List<Zombie> result)
		{
			if (regions == null)
			{
				return;
			}

			byte nav;
			if (!LevelNavigation.tryGetNavigation(center, out nav))
			{
				return;
			}

			if (regions[nav] == null || regions[nav].zombies == null)
			{
				return;
			}

			for (int index = 0; index < regions[nav].zombies.Count; index++)
			{
				Zombie zombie = regions[nav].zombies[index];

				if (zombie == null)
				{
					continue;
				}

				Vector3 offset = zombie.transform.position - center;

				if (offset.sqrMagnitude < sqrRadius)
				{
					result.Add(zombie);
				}
			}
		}

		[System.Obsolete]
		public void tellBeacon(CSteamID steamID, byte reference, bool hasBeacon)
		{
			ReceiveBeacon(reference, hasBeacon);
		}

		private static readonly ClientStaticMethod<byte, bool> SendBeacon = ClientStaticMethod<byte, bool>.Get(ReceiveBeacon);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellBeacon))]
		public static void ReceiveBeacon(byte reference, bool hasBeacon)
		{
			if (regions == null || reference >= regions.Length)
			{
				return;
			}

			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			regions[reference].hasBeacon = hasBeacon;
		}

		[System.Obsolete]
		public void tellWave(CSteamID steamID, bool newWaveReady, int newWave)
		{
			ReceiveWave(newWaveReady, newWave);
		}

		private static readonly ClientStaticMethod<bool, int> SendWave = ClientStaticMethod<bool, int>.Get(ReceiveWave);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWave))]
		public static void ReceiveWave(bool newWaveReady, int newWave)
		{
			_waveReady = newWaveReady;
			_waveIndex = newWave;

			onWaveUpdated?.Invoke(waveReady, waveIndex);
		}

		[System.Obsolete]
		public void askWave(CSteamID steamID)
		{ }

		internal static void SendInitialGlobalState(SteamPlayer client)
		{
			if (Level.info.type == ELevelType.HORDE)
			{
				SendWave.Invoke(ENetReliability.Reliable, client.transportConnection, waveReady, waveIndex);
			}
		}

		[System.Obsolete]
		public void tellZombieAlive(CSteamID steamID, byte reference, ushort id, byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
		{
			ReceiveZombieAlive(reference, id, newType, newSpeciality, newShirt, newPants, newHat, newGear, newPosition, newAngle);
		}

		private static readonly ClientStaticMethod<byte, ushort, byte, byte, byte, byte, byte, byte, Vector3, byte> SendZombieAlive =
			ClientStaticMethod<byte, ushort, byte, byte, byte, byte, byte, byte, Vector3, byte>.Get(ReceiveZombieAlive);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellZombieAlive))]
		public static void ReceiveZombieAlive(byte reference, ushort id, byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].tellAlive(newType, newSpeciality, newShirt, newPants, newHat, newGear, newPosition, newAngle);
		}

		[System.Obsolete]
		public void tellZombieDead(CSteamID steamID, byte reference, ushort id, Vector3 newRagdoll, byte newRagdollEffect)
		{
			ReceiveZombieDead(reference, id, newRagdoll, (ERagdollEffect) newRagdollEffect);
		}

		private static readonly ClientStaticMethod<byte, ushort, Vector3, ERagdollEffect> SendZombieDead =
			ClientStaticMethod<byte, ushort, Vector3, ERagdollEffect>.Get(ReceiveZombieDead);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellZombieDead))]
		public static void ReceiveZombieDead(byte reference, ushort id, Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].tellDead(newRagdoll, newRagdollEffect);
		}

		private static uint seq;

		[System.Obsolete]
		public void tellZombieStates(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendZombieStates = ClientStaticMethod.Get(ReceiveZombieStates);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveZombieStates(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;

			byte reference;
			reader.ReadUInt8(out reference);
			if (reference >= regions.Length)
			{
				context.IndexOutOfRange(nameof(reference), reference, regions.Length);
				return;
			}

			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					context.LogWarning($"region {reference} not received yet");
					return;
				}
			}

			uint newSeq;
			reader.ReadUInt32(out newSeq);
			if (newSeq <= seq)
			{
				context.LogWarning($"sequence {newSeq} older than {seq}");
				return;
			}
			seq = newSeq;

			ushort count;
			reader.ReadUInt16(out count);
			if (count < 1)
			{
				context.LogWarning($"empty");
				return;
			}

			for (ushort index = 0; index < count; ++index)
			{
				ushort zombieIndex;
				reader.ReadUInt16(out zombieIndex);
				Vector3 position;
				reader.ReadClampedVector3(out position);
				float yaw;
				reader.ReadDegrees(out yaw);

				if (zombieIndex >= regions[reference].zombies.Count)
				{
					context.IndexOutOfRange(nameof(zombieIndex), zombieIndex, regions[reference].zombies.Count);
					continue;
				}

				regions[reference].zombies[zombieIndex].tellState(position, yaw);
			}
		}

		[System.Obsolete]
		public void tellZombieSpeciality(CSteamID steamID, byte reference, ushort id, byte speciality)
		{
			ReceiveZombieSpeciality(reference, id, (EZombieSpeciality) speciality);
		}

		private static readonly ClientStaticMethod<byte, ushort, EZombieSpeciality> SendZombieSpeciality =
			ClientStaticMethod<byte, ushort, EZombieSpeciality>.Get(ReceiveZombieSpeciality);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellZombieSpeciality))]
		public static void ReceiveZombieSpeciality(byte reference, ushort id, EZombieSpeciality speciality)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].tellSpeciality(speciality);
		}

		[System.Obsolete]
		public void askZombieThrow(CSteamID steamID, byte reference, ushort id)
		{
			ReceiveZombieThrow(reference, id);
		}

		private static readonly ClientStaticMethod<byte, ushort> SendZombieThrow = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieThrow);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieThrow))]
		public static void ReceiveZombieThrow(byte reference, ushort id)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askThrow();
		}

		[System.Obsolete]
		public void askZombieBoulder(CSteamID steamID, byte reference, ushort id, Vector3 origin, Vector3 direction)
		{
			ReceiveZombieBoulder(reference, id, origin, direction);
		}

		private static readonly ClientStaticMethod<byte, ushort, Vector3, Vector3> SendZombieBoulder = ClientStaticMethod<byte, ushort, Vector3, Vector3>.Get(ReceiveZombieBoulder);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieBoulder))]
		public static void ReceiveZombieBoulder(byte reference, ushort id, Vector3 origin, [NetPakNormal] Vector3 direction)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askBoulder(origin, direction);
		}

		[System.Obsolete]
		public void askZombieSpit(CSteamID steamID, byte reference, ushort id)
		{
			ReceiveZombieSpit(reference, id);
		}

		private static readonly ClientStaticMethod<byte, ushort> SendZombieSpit = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieSpit);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieSpit))]
		public static void ReceiveZombieSpit(byte reference, ushort id)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askSpit();
		}

		[System.Obsolete]
		public void askZombieCharge(CSteamID steamID, byte reference, ushort id)
		{
			ReceiveZombieCharge(reference, id);
		}

		private static readonly ClientStaticMethod<byte, ushort> SendZombieCharge = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieCharge);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieCharge))]
		public static void ReceiveZombieCharge(byte reference, ushort id)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askCharge();
		}

		[System.Obsolete]
		public void askZombieStomp(CSteamID steamID, byte reference, ushort id)
		{
			ReceiveZombieStomp(reference, id);
		}

		private static readonly ClientStaticMethod<byte, ushort> SendZombieStomp = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieStomp);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieStomp))]
		public static void ReceiveZombieStomp(byte reference, ushort id)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askStomp();
		}

		[System.Obsolete]
		public void askZombieBreath(CSteamID steamID, byte reference, ushort id)
		{
			ReceiveZombieBreath(reference, id);
		}

		private static readonly ClientStaticMethod<byte, ushort> SendZombieBreath = ClientStaticMethod<byte, ushort>.Get(ReceiveZombieBreath);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieBreath))]
		public static void ReceiveZombieBreath(byte reference, ushort id)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askBreath();
		}

		[System.Obsolete]
		public void askZombieAcid(CSteamID steamID, byte reference, ushort id, Vector3 origin, Vector3 direction)
		{
			ReceiveZombieAcid(reference, id, origin, direction);
		}

		private static readonly ClientStaticMethod<byte, ushort, Vector3, Vector3> SendZombieAcid = ClientStaticMethod<byte, ushort, Vector3, Vector3>.Get(ReceiveZombieAcid);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieAcid))]
		public static void ReceiveZombieAcid(byte reference, ushort id, Vector3 origin, [NetPakNormal] Vector3 direction)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askAcid(origin, direction);
		}

		[System.Obsolete]
		public void askZombieSpark(CSteamID steamID, byte reference, ushort id, Vector3 target)
		{
			ReceiveZombieSpark(reference, id, target);
		}

		private static readonly ClientStaticMethod<byte, ushort, Vector3> SendZombieSpark = ClientStaticMethod<byte, ushort, Vector3>.Get(ReceiveZombieSpark);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieSpark))]
		public static void ReceiveZombieSpark(byte reference, ushort id, Vector3 target)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askSpark(target);
		}

		[System.Obsolete]
		public void askZombieAttack(CSteamID steamID, byte reference, ushort id, byte attack)
		{
			ReceiveZombieAttack(reference, id, attack);
		}

		private static readonly ClientStaticMethod<byte, ushort, byte> SendZombieAttack = ClientStaticMethod<byte, ushort, byte>.Get(ReceiveZombieAttack);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieAttack))]
		public static void ReceiveZombieAttack(byte reference, ushort id, byte attack)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askAttack(attack);
		}

		[System.Obsolete]
		public void askZombieStartle(CSteamID steamID, byte reference, ushort id, byte startle)
		{
			ReceiveZombieStartle(reference, id, startle);
		}

		private static readonly ClientStaticMethod<byte, ushort, byte> SendZombieStartle = ClientStaticMethod<byte, ushort, byte>.Get(ReceiveZombieStartle);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieStartle))]
		public static void ReceiveZombieStartle(byte reference, ushort id, byte startle)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askStartle(startle);
		}

		[System.Obsolete]
		public void askZombieStun(CSteamID steamID, byte reference, ushort id, byte stun)
		{
			ReceiveZombieStun(reference, id, stun);
		}

		private static readonly ClientStaticMethod<byte, ushort, byte> SendZombieStun = ClientStaticMethod<byte, ushort, byte>.Get(ReceiveZombieStun);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askZombieStun))]
		public static void ReceiveZombieStun(byte reference, ushort id, byte stun)
		{
			if (!Provider.isServer)
			{
				if (!regions[reference].isNetworked)
				{
					return;
				}
			}

			if (id >= regions[reference].zombies.Count)
			{
				return;
			}

			regions[reference].zombies[id].askStun(stun);
		}

		[System.Obsolete]
		public void tellZombies(CSteamID steamID)
		{ }

		private static readonly ClientStaticMethod SendZombies = ClientStaticMethod.Get(ReceiveZombies);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveZombies(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			byte reference;
			reader.ReadUInt8(out reference);
			if (regions[reference].isNetworked)
			{
				return;
			}

			regions[reference].isNetworked = true;

			bool hasBeacon;
			reader.ReadBit(out hasBeacon);
			ushort count;
			reader.ReadUInt16(out count);
			for (ushort index = 0; index < count; ++index)
			{
				byte type;
				reader.ReadUInt8(out type);
				byte speciality;
				reader.ReadUInt8(out speciality);
				byte shirt;
				reader.ReadUInt8(out shirt);
				byte pants;
				reader.ReadUInt8(out pants);
				byte hat;
				reader.ReadUInt8(out hat);
				byte gear;
				reader.ReadUInt8(out gear);
				byte move;
				reader.ReadUInt8(out move);
				byte idle;
				reader.ReadUInt8(out idle);
				Vector3 position;
				reader.ReadClampedVector3(out position);
				float yaw;
				reader.ReadDegrees(out yaw);
				bool isDead;
				reader.ReadBit(out isDead);

				manager.addZombie(reference, type, speciality, shirt, pants, hat, gear, move, idle, position, yaw, isDead);
			}

			regions[reference].hasBeacon = hasBeacon;
		}

		[System.Obsolete]
		public void askZombies(CSteamID steamID, byte bound)
		{ }

		private void SendZombiesToPlayer(ITransportConnection transportConnection, byte bound)
		{
			SendZombies.Invoke(ENetReliability.Reliable, transportConnection, SendZombies_Write, bound);
		}

		private static void SendZombies_Write(NetPakWriter writer, byte bound)
		{
			ZombieRegion region = regions[bound];
			writer.WriteUInt8(bound);
			writer.WriteBit(region.hasBeacon);
			writer.WriteUInt16((ushort) region.zombies.Count);
			for (ushort index = 0; index < region.zombies.Count; ++index)
			{
				Zombie zombie = region.zombies[index];
				writer.WriteUInt8(zombie.type);
				writer.WriteUInt8((byte) zombie.speciality);
				writer.WriteUInt8(zombie.shirt);
				writer.WriteUInt8(zombie.pants);
				writer.WriteUInt8(zombie.hat);
				writer.WriteUInt8(zombie.gear);
				writer.WriteUInt8(zombie.move);
				writer.WriteUInt8(zombie.idle);
				writer.WriteClampedVector3(zombie.transform.position);
				writer.WriteDegrees(zombie.transform.eulerAngles.y);
				writer.WriteBit(zombie.isDead);
			}
		}

		public static void sendZombieAlive(Zombie zombie, byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
		{
			SendZombieAlive.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, newType, newSpeciality, newShirt, newPants, newHat, newGear, newPosition, newAngle);

			regions[zombie.bound].onZombieLifeUpdated?.Invoke(zombie);
		}

		public static void sendZombieDead(Zombie zombie, Vector3 newRagdoll, ERagdollEffect newRagdollEffect = ERagdollEffect.None)
		{
			SendZombieDead.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, newRagdoll, newRagdollEffect);

			regions[zombie.bound].onZombieLifeUpdated?.Invoke(zombie);
		}

		public static void sendZombieSpeciality(Zombie zombie, EZombieSpeciality speciality)
		{
			SendZombieSpeciality.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, speciality);
		}

		public static void sendZombieThrow(Zombie zombie)
		{
			SendZombieThrow.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
		}

		public static void sendZombieSpit(Zombie zombie)
		{
			SendZombieSpit.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
		}

		public static void sendZombieCharge(Zombie zombie)
		{
			SendZombieCharge.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
		}

		public static void sendZombieStomp(Zombie zombie)
		{
			SendZombieStomp.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
		}

		public static void sendZombieBreath(Zombie zombie)
		{
			SendZombieBreath.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id);
		}

		public static void sendZombieBoulder(Zombie zombie, Vector3 origin, Vector3 direction)
		{
			SendZombieBoulder.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, origin, direction);
		}

		public static void sendZombieAcid(Zombie zombie, Vector3 origin, Vector3 direction)
		{
			SendZombieAcid.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, origin, direction);
		}

		public static void sendZombieSpark(Zombie zombie, Vector3 target)
		{
			SendZombieSpark.Invoke(ENetReliability.Unreliable, GatherClientConnections(zombie.bound), zombie.bound, zombie.id, target);
		}

		public static void sendZombieAttack(Zombie zombie, byte attack)
		{
			SendZombieAttack.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, attack);
		}

		public static void sendZombieStartle(Zombie zombie, byte startle)
		{
			SendZombieStartle.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, startle);
		}

		public static void sendZombieStun(Zombie zombie, byte stun)
		{
			SendZombieStun.InvokeAndLoopback(ENetReliability.Unreliable, GatherRemoteClientConnections(zombie.bound), zombie.bound, zombie.id, stun);
		}

		public static void dropLoot(Zombie zombie)
		{
			int drops;
			if (zombie.isBoss || zombie.speciality == EZombieSpeciality.BOSS_ALL)
			{
				drops = Random.Range((int) Provider.modeConfigData.Zombies.Min_Boss_Drops, (int) Provider.modeConfigData.Zombies.Max_Boss_Drops + 1);
			}
			else if (zombie.isMega)
			{
				drops = Random.Range((int) Provider.modeConfigData.Zombies.Min_Mega_Drops, (int) Provider.modeConfigData.Zombies.Max_Mega_Drops + 1);
			}
			else
			{
				drops = Random.Range((int) Provider.modeConfigData.Zombies.Min_Drops, (int) Provider.modeConfigData.Zombies.Max_Drops + 1);
			}
			// Prevent players from crashing themselves with huge numbers of items.
			drops = Mathf.Clamp(drops, 0, 100);

			if (LevelZombies.tables[zombie.type].isMega)
			{
				regions[zombie.bound].lastMega = Time.realtimeSinceStartup;
				regions[zombie.bound].hasMega = false;
			}

			if (drops > 1 || Random.value < Provider.modeConfigData.Zombies.Loot_Chance)
			{
				if (LevelZombies.tables[zombie.type].lootID != 0)
				{
					for (int drop = 0; drop < drops; drop++)
					{
						ushort id = SpawnTableTool.ResolveLegacyId(LevelZombies.tables[zombie.type].lootID, EAssetType.ITEM, OnGetZombieLootSpawnTableErrorContext);

						if (id != 0)
						{
							Item item = new Item(id, EItemOrigin.WORLD);
							ItemManager.dropItem(item, zombie.transform.position, false, Dedicator.IsDedicatedServer, true);
						}
					}
				}
				else if (LevelZombies.tables[zombie.type].lootIndex < LevelItems.tables.Count)
				{
					for (int drop = 0; drop < drops; drop++)
					{
						ushort id = LevelItems.getItem(LevelZombies.tables[zombie.type].lootIndex);

						if (id != 0)
						{
							Item item = new Item(id, EItemOrigin.WORLD);
							ItemManager.dropItem(item, zombie.transform.position, false, Dedicator.IsDedicatedServer, true);
						}
					}
				}
			}
		}

		private static string OnGetZombieLootSpawnTableErrorContext()
		{
			return "zombie loot";
		}

		private static StaticResourceRef<GameObject> dedicatedZombiePrefab = new StaticResourceRef<GameObject>("Characters/Zombie_Dedicated");
		private static StaticResourceRef<GameObject> serverZombiePrefab = new StaticResourceRef<GameObject>("Characters/Zombie_Server");
		private static StaticResourceRef<GameObject> clientZombiePrefab = new StaticResourceRef<GameObject>("Characters/Zombie_Client");

		public void addZombie(byte bound, byte type, byte speciality, byte shirt, byte pants, byte hat, byte gear, byte move, byte idle, Vector3 position, float angle, bool isDead)
		{
			Quaternion rotation = Quaternion.Euler(0, angle, 0);

			GameObject zombiePrefab;
			if (Dedicator.IsDedicatedServer)
			{
				zombiePrefab = dedicatedZombiePrefab;
			}
			else if (Provider.isServer)
			{
				zombiePrefab = serverZombiePrefab;
			}
			else
			{
				zombiePrefab = clientZombiePrefab;
			}

			GameObject zombieGameObject = Instantiate(zombiePrefab, position, rotation);
			zombieGameObject.name = "Zombie";

			Zombie character = zombieGameObject.GetComponent<Zombie>();
			character.id = (ushort) regions[bound].zombies.Count;
			character.speciality = (EZombieSpeciality) speciality;
			character.bound = bound;
			character.zombieRegion = regions[bound];
			character.type = type;
			character.shirt = shirt;
			character.pants = pants;
			character.hat = hat;
			character.gear = gear;
			character.move = move;
			character.idle = idle;
			character.isDead = isDead;
			character.init();

			regions[bound].zombies.Add(character);
			AllZombies.Add(character);
		}

		public static Zombie getZombie(Vector3 point, ushort id)
		{
			byte bound;

			if (LevelNavigation.tryGetBounds(point, out bound))
			{
				if (id >= regions[bound].zombies.Count)
				{
					return null;
				}

				if (regions[bound].zombies[id].isDead)
				{
					return null;
				}

				return regions[bound].zombies[id];
			}

			return null;
		}

		/// <summary>
		/// Find difficulty asset (if valid) for navigation bound index.
		/// </summary>
		public static ZombieDifficultyAsset getDifficultyInBound(byte bound)
		{
			if (bound < LevelNavigation.flagData.Count)
			{
				return LevelNavigation.flagData[bound].resolveDifficulty();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Allows level to override whether per-table or per-navmesh difficulty asset takes priority.
		/// </summary>
		public static ZombieDifficultyAsset GetDifficultyInBoundForTable(byte bound, ZombieTable table, bool forSpawnOverrides)
		{
			ZombieDifficultyAsset result;

			switch (Level.getAsset()?.ZombieDifficultyAssetPrioritization ?? EZombieDifficultyAssetPrioritization.NavmeshOverridesTable)
			{
				default:
				case EZombieDifficultyAssetPrioritization.NavmeshOverridesTable:
				{
					result = getDifficultyInBound(bound);
					if (result == null || (forSpawnOverrides && !result.Overrides_Spawn_Chance))
					{
						result = table.resolveDifficulty();
					}

					break;
				}

				case EZombieDifficultyAssetPrioritization.TableOverridesNavmesh:
				{
					result = table.resolveDifficulty();
					if (result == null || (forSpawnOverrides && !result.Overrides_Spawn_Chance))
					{
						result = getDifficultyInBound(bound);
					}

					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Could potentially be reused generically.
		/// </summary>
		private class ZombieSpecialityWeightedRandom : System.Collections.Generic.IComparer<ZombieSpecialityWeightedRandom.Entry>
		{
			public struct Entry
			{
				public EZombieSpeciality value;
				public float weight;

				public Entry(EZombieSpeciality value, float weight)
				{
					this.value = value;
					this.weight = weight;
				}
			}

			private List<Entry> entries;
			public float totalWeight
			{
				get;
				private set;
			}

			public void clear()
			{
				entries.Clear();
				totalWeight = 0.0f;
			}

			public void add(EZombieSpeciality value, float weight)
			{
				weight = Mathf.Max(weight, 0.0f);
				Entry entry = new Entry(value, weight);
				int index = entries.BinarySearch(entry, this);
				if (index < 0)
				{
					index = ~index;
				}
				entries.Insert(index, entry);
				totalWeight += weight;
			}

			public EZombieSpeciality get()
			{
				if (entries.Count < 1)
				{
					// List is empty.
					return default;
				}

				float random = Random.value * totalWeight;

				foreach (Entry entry in entries)
				{
					if (random < entry.weight)
					{
						return entry.value;
					}

					// e.g. [0] is 10, [1] is 5, and random is 12
					// subtract 10 so random is 2 and will select [1]
					random -= entry.weight;
				}

				// Maybe edge case with small numbers at end of list? Default to highest weight.
				return entries[0].value;
			}

			public void log()
			{
				UnturnedLog.info("Entries: {0} Total Weight: {1}", entries.Count, totalWeight);
				foreach (Entry x in entries)
				{
					UnturnedLog.info("{0}: {1}", x.value, x.weight);
				}
			}

			public int Compare(Entry lhs, Entry rhs)
			{
				// Default CompareTo uses less than, so we negate to put highest weights at the front of the list.
				return -lhs.weight.CompareTo(rhs.weight);
			}

			public ZombieSpecialityWeightedRandom()
			{
				entries = new List<Entry>();
				totalWeight = 0.0f;
			}
		}

		private static ZombieSpecialityWeightedRandom zombieSpecialityTable = new ZombieSpecialityWeightedRandom();

		private static EZombieSpeciality generateZombieSpeciality(byte bound, ZombieTable table)
		{
			zombieSpecialityTable.clear();

			ZombieDifficultyAsset asset = GetDifficultyInBoundForTable(bound, table, true);
			if (asset != null && asset.Overrides_Spawn_Chance)
			{
				zombieSpecialityTable.add(EZombieSpeciality.CRAWLER, asset.Crawler_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.SPRINTER, asset.Sprinter_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.FLANKER_FRIENDLY, asset.Flanker_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BURNER, asset.Burner_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.ACID, asset.Acid_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELECTRIC, asset.Boss_Electric_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_WIND, asset.Boss_Wind_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_FIRE, asset.Boss_Fire_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.SPIRIT, asset.Spirit_Chance);

				// Check Level isLoaded otherwise lighting day/night might not have loaded yet.
				// Only spawn volatiles at nighttime, otherwise they explode immediately.
				if (Level.isLoaded && LightingManager.isNighttime)
				{
					zombieSpecialityTable.add(EZombieSpeciality.DL_RED_VOLATILE, asset.DL_Red_Volatile_Chance);
					zombieSpecialityTable.add(EZombieSpeciality.DL_BLUE_VOLATILE, asset.DL_Blue_Volatile_Chance);
				}

				zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELVER_STOMPER, asset.Boss_Elver_Stomper_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_KUWAIT, asset.Boss_Kuwait_Chance);
			}
			else
			{
				zombieSpecialityTable.add(EZombieSpeciality.CRAWLER, Provider.modeConfigData.Zombies.Crawler_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.SPRINTER, Provider.modeConfigData.Zombies.Sprinter_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.FLANKER_FRIENDLY, Provider.modeConfigData.Zombies.Flanker_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BURNER, Provider.modeConfigData.Zombies.Burner_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.ACID, Provider.modeConfigData.Zombies.Acid_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELECTRIC, Provider.modeConfigData.Zombies.Boss_Electric_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_WIND, Provider.modeConfigData.Zombies.Boss_Wind_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_FIRE, Provider.modeConfigData.Zombies.Boss_Fire_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.SPIRIT, Provider.modeConfigData.Zombies.Spirit_Chance);

				// Check Level isLoaded otherwise lighting day/night might not have loaded yet.
				// Only spawn volatiles at nighttime, otherwise they explode immediately.
				if (Level.isLoaded && LightingManager.isNighttime)
				{
					zombieSpecialityTable.add(EZombieSpeciality.DL_RED_VOLATILE, Provider.modeConfigData.Zombies.DL_Red_Volatile_Chance);
					zombieSpecialityTable.add(EZombieSpeciality.DL_BLUE_VOLATILE, Provider.modeConfigData.Zombies.DL_Blue_Volatile_Chance);
				}

				zombieSpecialityTable.add(EZombieSpeciality.BOSS_ELVER_STOMPER, Provider.modeConfigData.Zombies.Boss_Elver_Stomper_Chance);
				zombieSpecialityTable.add(EZombieSpeciality.BOSS_KUWAIT, Provider.modeConfigData.Zombies.Boss_Kuwait_Chance);
			}

			// Not ideal, but many configurations exist assuming normal is the default with all chances adding up to 100%.
			zombieSpecialityTable.add(EZombieSpeciality.NORMAL, 1.0f - zombieSpecialityTable.totalWeight);

			return zombieSpecialityTable.get();
		}

		/// <summary>
		/// When zombie falls outside the map it needs a replacement spawnpoint within the same navmesh area.
		/// </summary>
		private static ZombieSpawnpoint getReplacementSpawnpointInBound(byte bound)
		{
			if (bound < LevelZombies.zombies.Length) // Should always be valid, but just in case...
			{
				List<ZombieSpawnpoint> eligibleSpawnpoints = LevelZombies.zombies[bound];
				if (eligibleSpawnpoints.Count > 0) // Should have been >0 to spawn a zombie in the first place, but just in case...
				{
					int randomIndex = Random.Range(0, eligibleSpawnpoints.Count);
					return eligibleSpawnpoints[randomIndex];
				}
				else
				{
					UnturnedLog.warn("Unable to replace zombie because spawns are empty in bound {0}", bound);
				}
			}
			else
			{
				UnturnedLog.warn("Unable to replace zombie because bound {0} is out of range", bound);
			}

			return null;
		}

		/// <summary>
		/// Find replacement spawnpoint for a zombie and teleport it there.
		/// </summary>
		public static void teleportZombieBackIntoMap(Zombie zombie)
		{
			ZombieSpawnpoint spawnpoint = getReplacementSpawnpointInBound(zombie.bound);
			if (spawnpoint != null)
			{
				EffectAsset souls_1 = Souls_1_Ref.Find();
				if (souls_1 != null)
				{
					TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(souls_1);
					triggerEffectParameters.relevantDistance = 16.0f;
					triggerEffectParameters.position = zombie.transform.position + Vector3.up;
					EffectManager.triggerEffect(triggerEffectParameters);
				}

				Vector3 position = spawnpoint.point + Vector3.up;
				zombie.transform.position = position;
			}
		}

		internal static readonly AssetReference<EffectAsset> Souls_1_Ref = new AssetReference<EffectAsset>("c17b00f2a58646c8a9ea728f6d72e54e"); // (125)

		public void generateZombies(byte bound)
		{
			if (LevelNavigation.bounds.Count == 0 || LevelZombies.zombies.Length == 0 || LevelNavigation.bounds.Count != LevelZombies.zombies.Length)
			{
				return;
			}

			List<ZombieSpawnpoint> levelSpawnpoints = LevelZombies.zombies[bound];
			if (levelSpawnpoints.Count > 0)
			{
				ZombieRegion region = regions[bound];
				region.alive = 0;

				List<ZombieSpawnpoint> eligibleSpawnpoints = new List<ZombieSpawnpoint>();
				foreach (ZombieSpawnpoint spawnpoint in levelSpawnpoints)
				{
					if (SafezoneManager.checkPointValid(spawnpoint.point))
					{
						eligibleSpawnpoints.Add(spawnpoint);
					}
				}

				int regionMaxZombies;
				int regionMaxBossZombies;
				if (Level.info.type == ELevelType.HORDE)
				{
					regionMaxZombies = 40;
					regionMaxBossZombies = -1;
				}
				else
				{
					int serverMaxZombies = Mathf.CeilToInt(levelSpawnpoints.Count * Provider.modeConfigData.Zombies.Spawn_Chance);
					int flagMaxZombies = LevelNavigation.flagData[bound].maxZombies;
					regionMaxZombies = Mathf.Min(flagMaxZombies, serverMaxZombies);
					regionMaxBossZombies = LevelNavigation.flagData[bound].maxBossZombies;
				}

				while (eligibleSpawnpoints.Count > 0 && region.zombies.Count < regionMaxZombies)
				{
					int randomSpawnpointIndex = Random.Range(0, eligibleSpawnpoints.Count);
					ZombieSpawnpoint spawn = eligibleSpawnpoints[randomSpawnpointIndex];
					eligibleSpawnpoints.RemoveAt(randomSpawnpointIndex);

					byte spawnType = spawn.type;
					ZombieTable table = LevelZombies.tables[spawnType];

					if (canRegionSpawnZombiesFromTable(region, table))
					{
						EZombieSpeciality speciality = EZombieSpeciality.NORMAL;

						if (table.isMega)
						{
							region.lastMega = Time.realtimeSinceStartup;
							region.hasMega = true;

							speciality = EZombieSpeciality.MEGA;
						}
						else if (Level.info.type == ELevelType.SURVIVAL)
						{
							speciality = generateZombieSpeciality(bound, table);
						}

						if (regionMaxBossZombies >= 0 && speciality.IsBoss() && region.aliveBossZombieCount >= regionMaxBossZombies)
						{
							// Reached max boss zombie limit.
							continue;
						}

						if (region.hasBeacon)
						{
							BeaconManager.checkBeacon(bound).spawnRemaining();
						}

						byte shirt = 255;
						if (table.slots[0].table.Count > 0 && Random.value < table.slots[0].chance)
						{
							shirt = (byte) Random.Range(0, table.slots[0].table.Count);
						}

						byte pants = 255;
						if (table.slots[1].table.Count > 0 && Random.value < table.slots[1].chance)
						{
							pants = (byte) Random.Range(0, table.slots[1].table.Count);
						}

						byte hat = 255;
						if (table.slots[2].table.Count > 0 && Random.value < table.slots[2].chance)
						{
							hat = (byte) Random.Range(0, table.slots[2].table.Count);
						}

						byte gear = 255;
						if (table.slots[3].table.Count > 0 && Random.value < table.slots[3].chance)
						{
							gear = (byte) Random.Range(0, table.slots[3].table.Count);
						}

						byte move = (byte) Random.Range(0, 4);
						byte idle = (byte) Random.Range(0, 3);

						Vector3 point = spawn.point;
						point += new Vector3(0, 0.5f, 0);

						addZombie(bound, spawnType, (byte) speciality, shirt, pants, hat, gear, move, idle, point, Random.Range(0f, 360f), !LevelNavigation.flagData[bound].spawnZombies || Level.info.type == ELevelType.HORDE);
					}
				}
			}
		}

		private bool canRegionSpawnZombiesFromTable(ZombieRegion region, ZombieTable table)
		{
			if (region.hasBeacon)
			{
				return !table.isMega;
			}
			else
			{
				return !table.isMega || (!region.hasMega && Time.realtimeSinceStartup - region.lastMega > 600);
			}
		}

		public void respawnZombies()
		{
			ZombieRegion region = regions[respawnZombiesBound];

			if (Level.info.type == ELevelType.HORDE)
			{
				if (waveRemaining > 0 || region.alive > 0)
				{
					lastWave = Time.realtimeSinceStartup;
				}

				if (waveRemaining == 0)
				{
					if (region.alive > 0)
					{
						return;
					}

					if (Time.realtimeSinceStartup - lastWave > 10f || waveIndex == 0)
					{
						if (!waveReady)
						{
							_waveReady = true;
							_waveIndex++;
							_waveRemaining = (int) Mathf.Ceil(Mathf.Pow(waveIndex + 5, 1.5f));

							SendWave.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), waveReady, waveIndex);
						}
					}
					else
					{
						if (waveReady)
						{
							_waveReady = false;

							SendWave.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), waveReady, waveIndex);
						}

						return;
					}
				}
			}

			if (!LevelNavigation.flagData[respawnZombiesBound].spawnZombies)
			{
				return;
			}

			if (region.zombies.Count > 0)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					if (!region.hasBeacon && Level.info.type != ELevelType.HORDE)
					{
						return;
					}
				}

				if (region.hasBeacon)
				{
					if (BeaconManager.checkBeacon(respawnZombiesBound).getRemaining() == 0)
					{
						return;
					}
				}

				if (region.respawnZombieIndex >= region.zombies.Count)
				{
					region.respawnZombieIndex = (ushort) (region.zombies.Count - 1);
				}

				Zombie zombie = region.zombies[region.respawnZombieIndex];

				region.respawnZombieIndex++;

				if (region.respawnZombieIndex >= region.zombies.Count)
				{
					region.respawnZombieIndex = 0;
				}

				if (!zombie.isDead)
				{
					return;
				}

				float respawnInterval = Provider.modeConfigData.Zombies.Respawn_Day_Time;
				if (region.hasBeacon)
				{
					respawnInterval = Provider.modeConfigData.Zombies.Respawn_Beacon_Time;
				}
				else if (LightingManager.isFullMoon)
				{
					respawnInterval = Provider.modeConfigData.Zombies.Respawn_Night_Time;
				}

				if (Time.realtimeSinceStartup - zombie.lastDead > respawnInterval)
				{
					ZombieSpawnpoint spawn = LevelZombies.zombies[respawnZombiesBound][Random.Range(0, LevelZombies.zombies[respawnZombiesBound].Count)];

					if (!SafezoneManager.checkPointValid(spawn.point))
					{
						return;
					}

					for (ushort index = 0; index < region.zombies.Count; index++)
					{
						if (!region.zombies[index].isDead && (region.zombies[index].transform.position - spawn.point).sqrMagnitude < 4)
						{
							return;
						}
					}

					byte spawnType = spawn.type;
					ZombieTable table = LevelZombies.tables[spawnType];

					if (canRegionSpawnZombiesFromTable(region, table))
					{
						EZombieSpeciality speciality = EZombieSpeciality.NORMAL;

						if (region.hasBeacon ? BeaconManager.checkBeacon(respawnZombiesBound).getRemaining() == 1 : table.isMega)
						{
							if (!table.isMega)
							{
								for (byte tableIndex = 0; tableIndex < LevelZombies.tables.Count; tableIndex++)
								{
									ZombieTable searchTable = LevelZombies.tables[tableIndex];

									if (searchTable.isMega)
									{
										spawnType = tableIndex;
										table = searchTable;
										break;
									}
								}
							}

							region.lastMega = Time.realtimeSinceStartup;
							region.hasMega = true;

							speciality = EZombieSpeciality.MEGA;
						}
						else if (Level.info.type == ELevelType.SURVIVAL)
						{
							speciality = generateZombieSpeciality(respawnZombiesBound, table);
						}

						int regionMaxBossZombies = LevelNavigation.flagData[respawnZombiesBound].maxBossZombies;
						if (regionMaxBossZombies >= 0 && speciality.IsBoss() && region.aliveBossZombieCount >= regionMaxBossZombies)
						{
							// Reached max boss zombie limit.
							return;
						}

						if (region.hasBeacon)
						{
							BeaconManager.checkBeacon(respawnZombiesBound).spawnRemaining();
						}

						byte shirt;
						byte pants;
						byte hat;
						byte gear;
						table.GetSpawnClothingParameters(out shirt, out pants, out hat, out gear);

						Vector3 point = spawn.point;
						point += new Vector3(0, 0.5f, 0);

						zombie.sendRevive(spawnType, (byte) speciality, shirt, pants, hat, gear, point, Random.Range(0f, 360f));

						if (Level.info.type == ELevelType.HORDE)
						{
							_waveRemaining--;
						}
					}
				}
			}
		}

		private void onBoundUpdated(Player player, byte oldBound, byte newBound)
		{
			if (player.channel.IsLocalPlayer)
			{
				if (LevelNavigation.checkSafe(oldBound) && regions[oldBound].isNetworked)
				{
					regions[oldBound].destroy();

					regions[oldBound].isNetworked = false;
				}
			}

			if (Provider.isServer)
			{
				if (LevelNavigation.checkSafe(oldBound))
				{
					if (player.movement.loadedBounds[oldBound].isZombiesLoaded)
					{
						player.movement.loadedBounds[oldBound].isZombiesLoaded = false;
					}

					// Max just to be safe.
					regions[oldBound].PlayerCountInRegion = Mathf.Max(0, regions[oldBound].PlayerCountInRegion - 1);
				}

				if (LevelNavigation.checkSafe(newBound))
				{
					if (!player.movement.loadedBounds[newBound].isZombiesLoaded)
					{
						if (player.channel.IsLocalPlayer)
						{
							generateZombies(newBound);

							regions[newBound].isNetworked = true;
						}
						else
						{
							SendZombiesToPlayer(player.channel.owner.transportConnection, newBound);
						}

						player.movement.loadedBounds[newBound].isZombiesLoaded = true;
					}

					regions[newBound].PlayerCountInRegion += 1;
				}
			}
		}

		private void onPlayerCreated(Player player)
		{
			if (Provider.isServer)
			{
				if (player.movement.bound < regions.Length)
				{
					regions[player.movement.bound].PlayerCountInRegion += 1;
				}
			}

			player.movement.onBoundUpdated += onBoundUpdated;
		}

		private void onPlayerDestroyed(Player player)
		{
			if (Provider.isServer)
			{
				if (player.movement.bound < regions.Length)
				{
					// Max just to be safe.
					regions[player.movement.bound].PlayerCountInRegion = Mathf.Max(0, regions[player.movement.bound].PlayerCountInRegion - 1);
				}
			}

			player.movement.onBoundUpdated -= onBoundUpdated;
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_MENU)
			{
				seq = 0;

				if (LevelNavigation.bounds == null)
				{
					return;
				}

				_regions = new ZombieRegion[LevelNavigation.bounds.Count];
				regionsWithPlayers = new HashSet<int>();
				for (byte regionIndex = 0; regionIndex < regions.Length; regionIndex++)
				{
					regions[regionIndex] = new ZombieRegion(regionIndex);
					Vector3 center = LevelNavigation.bounds[regionIndex].center;
					regions[regionIndex].isRadioactive = SDG.Framework.Devkit.DeadzoneVolumeManager.Get().IsNavmeshCenterInsideAnyVolume(center);
				}

				cooldowns.Clear();

				wanderingCount = 0;
				tickIndex = 0;
				_tickingZombies = new List<Zombie>();
				AllZombies = new List<Zombie>();

				respawnZombiesBound = 0;

				_waveReady = false;
				_waveIndex = 0;
				_waveRemaining = 0;

				onWaveUpdated = null;

				if (Dedicator.IsDedicatedServer)
				{
					if (LevelNavigation.bounds.Count == 0 || LevelZombies.zombies.Length == 0 || LevelNavigation.bounds.Count != LevelZombies.zombies.Length)
					{
						return;
					}

					for (byte bound = 0; bound < LevelNavigation.bounds.Count; bound++)
					{
						generateZombies(bound);
					}
				}
			}

			if (level > Level.BUILD_INDEX_SETUP)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					ZombieClothing.build();
				}
			}
		}

		/// <summary>
		/// Kills night-only zombies at dawn. 
		/// </summary>
		private void onDayNightUpdated(bool isDaytime)
		{
			if (isDaytime == false)
				return;

			foreach (ZombieRegion region in regions)
			{
				foreach (Zombie zombie in region.zombies)
				{
					if (zombie.speciality.IsDLVolatile())
					{
						zombie.killWithFireExplosion();
					}
				}
			}
		}


		private void onPostLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_MENU)
			{
				if (regions == null)
				{
					return;
				}

				for (int index = 0; index < regions.Length; index++)
				{
					regions[index].init();

					if (Provider.isServer)
					{
						InteractableBeacon beacon = BeaconManager.checkBeacon((byte) index);
						if (beacon != null)
						{
							beacon.init(regions[index].alive);
						}

						regions[index].hasBeacon = beacon != null;
					}
				}

				// Delegate is reset during level load.
				LightingManager.onDayNightUpdated += onDayNightUpdated;
			}
		}

		private void onBeaconUpdated(byte nav, bool hasBeacon)
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (regions == null || nav >= regions.Length)
			{
				return;
			}

			if (hasBeacon)
			{
				InteractableBeacon beacon = BeaconManager.checkBeacon(nav);
				beacon.init(regions[nav].alive);
			}

			SendBeacon.InvokeAndLoopback(ENetReliability.Reliable, GatherRemoteClientConnections(nav), nav, hasBeacon);
		}

		private void updateRegionsAndSendZombieStates()
		{
			for (byte regionIndex = 0; regionIndex < regions.Length; ++regionIndex)
			{
				ZombieRegion region = regions[regionIndex];
				region.UpdateRegion();

				if (region.updates > 0)
				{
					if (Dedicator.IsDedicatedServer)
					{
						seq++;
						SendZombieStates.Invoke(ENetReliability.Unreliable, GatherRemoteClientConnections(regionIndex),
							SendZombieStates_Write, regionIndex);

						region.updates = 0;
					}
					else
					{
						foreach (Zombie updated in region.zombies)
						{
							if (updated.isUpdated)
							{
								updated.isUpdated = false;
							}
						}

						region.updates = 0;
					}
				}
			}
		}

		private void SendZombieStates_Write(NetPakWriter writer, byte regionIndex)
		{
			ZombieRegion region = regions[regionIndex];
			writer.WriteUInt8(regionIndex);
			writer.WriteUInt32(seq);
			writer.WriteUInt16(region.updates);
			foreach (Zombie updated in region.zombies)
			{
				if (updated.isUpdated)
				{
					updated.isUpdated = false;
					writer.WriteUInt16(updated.id);
					writer.WriteClampedVector3(updated.transform.position);
					writer.WriteDegrees(updated.transform.eulerAngles.y);
				}
			}
		}

		private void Update()
		{
			if (!Level.isLoaded)
			{
				return;
			}

			if (!Provider.isServer && AllZombies != null)
			{
				foreach (Zombie zombie in AllZombies)
				{
					try
					{
						// We *shouldn't* have null zombies but we don't want to take risks when this hack is right before a major update.
						zombie?.OnUpdate();
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, $"Caught exception updating zombie:");
					}
				}

				return;
			}

			if (LevelNavigation.bounds == null || LevelNavigation.bounds.Count == 0 || LevelZombies.zombies == null || LevelZombies.zombies.Length == 0 || LevelNavigation.bounds.Count != LevelZombies.zombies.Length)
			{
				return;
			}

			if (regions == null || tickingZombies == null)
			{
				return;
			}

			int start;
			int end;

			if (Dedicator.IsDedicatedServer)
			{
				if (tickIndex >= tickingZombies.Count)
				{
					tickIndex = 0;
				}

				start = tickIndex;
				end = start + 50;
				if (end >= tickingZombies.Count)
				{
					end = tickingZombies.Count;
				}

				tickIndex = end;
			}
			else
			{
				start = 0;
				end = tickingZombies.Count;
			}

			UnityEngine.Profiling.Profiler.BeginSample("TickZombies");
			for (int index = end - 1; index >= start; index--)
			{
				Zombie zombie = tickingZombies[index];

				if (zombie == null)
				{
					UnturnedLog.error("Missing zombie " + index);
					continue;
				}

				zombie.tick();
			}
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("TickZombiesInRegionsWithPlayers");
			foreach (int updateRegionIndex in regionsWithPlayers)
			{
				ZombieRegion region = regions[updateRegionIndex];
				foreach (Zombie zombie in region.zombies)
				{
					try
					{
						// We *shouldn't* have null zombies but we don't want to take risks when this hack is right before a major update.
						zombie?.OnUpdate();
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, $"Caught exception updating zombie:");
					}
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();

			if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
			{
				lastTick += Provider.UPDATE_TIME;
				if (Time.realtimeSinceStartup - lastTick > Provider.UPDATE_TIME)
				{
					lastTick = Time.realtimeSinceStartup;
				}

				UnityEngine.Profiling.Profiler.BeginSample("ZombieManager.updateRegionsAndSendZombieStates()");
				updateRegionsAndSendZombieStates();
				UnityEngine.Profiling.Profiler.EndSample();
			}

			UnityEngine.Profiling.Profiler.BeginSample("ZombieManager.respawnZombies()");
			respawnZombies();
			UnityEngine.Profiling.Profiler.EndSample();

			respawnZombiesBound++;

			if (respawnZombiesBound >= LevelZombies.zombies.Length)
			{
				respawnZombiesBound = 0;
			}
		}

		private void Start()
		{
			manager = this;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;

			Level.onLevelLoaded += onLevelLoaded;
			Level.onPostLevelLoaded += onPostLevelLoaded;
			Player.onPlayerCreated += onPlayerCreated;
			Player.onPlayerDestroyed += onPlayerDestroyed;
			BeaconManager.onBeaconUpdated += onBeaconUpdated;

			if (!Dedicator.IsDedicatedServer)
			{
				Assets.onAssetsRefreshed += OnAssetsRefreshed;
			}
		}

		private void OnAssetsRefreshed()
		{
			Assets.onAssetsRefreshed -= OnAssetsRefreshed;

			MasterBundleConfig core = Assets.findMasterBundleByName("core.masterbundle");
			if (core == null)
			{
				UnturnedLog.warn("Unable to load default zombie sounds");
				return;
			}

			roars = new AudioClip[16];
			for (int index = 0; index < roars.Length; index++)
			{
				roars[index] = core.assetBundle.LoadAsset<AudioClip>($"Assets/CoreMasterBundle/Sounds/Zombies/Roars/Roar_{index}.mp3");
			}

			groans = new AudioClip[5];
			for (int index = 0; index < groans.Length; index++)
			{
				groans[index] = core.assetBundle.LoadAsset<AudioClip>($"Assets/CoreMasterBundle/Sounds/Zombies/Groans/Groan_{index}.mp3");
			}

			spits = new AudioClip[4];
			for (int index = 0; index < spits.Length; index++)
			{
				spits[index] = core.assetBundle.LoadAsset<AudioClip>($"Assets/CoreMasterBundle/Sounds/Zombies/Spits/Spit_{index}.mp3");
			}

			dl_attacks = new AudioClip[6];
			for (int index = 0; index < dl_attacks.Length; index++)
			{
				dl_attacks[index] = core.assetBundle.LoadAsset<AudioClip>($"Assets/CoreMasterBundle/Sounds/Zombies/DL_Volatile/volatile00_attack_0{index}.wav");
			}

			dl_deaths = new AudioClip[4];
			for (int index = 0; index < dl_deaths.Length; index++)
			{
				dl_deaths[index] = core.assetBundle.LoadAsset<AudioClip>($"Assets/CoreMasterBundle/Sounds/Zombies/DL_Volatile/volatile00_death_0{index}.wav");
			}

			dl_enemy_spotted = new AudioClip[4];
			for (int index = 0; index < dl_enemy_spotted.Length; index++)
			{
				dl_enemy_spotted[index] = core.assetBundle.LoadAsset<AudioClip>($"Assets/CoreMasterBundle/Sounds/Zombies/DL_Volatile/volatile00_enemy_spotted_0{index}.wav");
			}

			dl_taunt = new AudioClip[4];
			for (int index = 0; index < dl_taunt.Length; index++)
			{
				dl_taunt[index] = core.assetBundle.LoadAsset<AudioClip>($"Assets/CoreMasterBundle/Sounds/Zombies/DL_Volatile/volatile_taunt_0{index}.wav");
			}
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Zombie regions: {regions.Length}");
			int zombies = 0;
			int aliveZombies = 0;
			int aliveBossZombies = 0;
			foreach (ZombieRegion region in regions)
			{
				zombies += region.zombies?.Count ?? 0;
				aliveZombies += region.alive;
				aliveBossZombies += region.aliveBossZombieCount;
			}
			if (zombies != AllZombies.Count)
			{
				UnturnedLog.error($"AllZombies doesn't match per-region count! (AllZombies: {AllZombies.Count}, Total: {zombies})");
			}
			results.Add($"Zombies: {zombies}");
			results.Add($"Alive zombies: {aliveZombies}");
			results.Add($"Alive boss zombies: {aliveBossZombies}");

			results.Add($"Ticking zombies: {tickingZombies.Count}");
		}

		public static PooledTransportConnectionList GatherClientConnections(byte bound)
		{
			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client.player != null && client.player.movement.bound == bound)
				{
					list.Add(client.transportConnection);
				}
			}
			return list;
		}

		[System.Obsolete("Replaced by GatherClientConnections")]
		public static IEnumerable<ITransportConnection> EnumerateClients(byte bound)
		{
			return GatherClientConnections(bound);
		}

		public static PooledTransportConnectionList GatherRemoteClientConnections(byte bound)
		{
			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			foreach (SteamPlayer client in Provider.clients)
			{
#if !DEDICATED_SERVER
				if (client.IsLocalServerHost)
					continue;
#endif // !DEDICATED_SERVER

				if (client.player != null && client.player.movement.bound == bound)
				{
					list.Add(client.transportConnection);
				}
			}
			return list;
		}

		[System.Obsolete("Replaced by GatherRemoteClientConnections")]
		public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte bound)
		{
			return GatherRemoteClientConnections(bound);
		}
	}
}
