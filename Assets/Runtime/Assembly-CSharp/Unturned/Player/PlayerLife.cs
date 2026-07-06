////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void PlayerLifeUpdated(Player player);
	public delegate void LifeUpdated(bool isDead);
	public delegate void HealthUpdated(byte newHealth);
	public delegate void FoodUpdated(byte newFood);
	public delegate void WaterUpdated(byte newWater);
	public delegate void VirusUpdated(byte newVirus);
	public delegate void StaminaUpdated(byte newStamina);
	public delegate void VisionUpdated(bool isViewing);
	public delegate void OxygenUpdated(byte newOxygen);
	public delegate void BleedingUpdated(bool newBleeding);
	public delegate void BrokenUpdated(bool newBroken);
	public delegate void TemperatureUpdated(EPlayerTemperature newTemperature);
	public delegate void Damaged(byte damage);
	public delegate void Hurt(Player player, byte damage, Vector3 force, EDeathCause cause, ELimb limb, CSteamID killer);

	public class PlayerLife : PlayerCaller
	{
		public static readonly byte SAVEDATA_VERSION_LATEST = 3;
		public static readonly byte SAVEDATA_VERSION_WITH_OXYGEN = 3;

		[System.Obsolete("Future version numbers for all systems will specify what changed.")]
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_LATEST;

		private static readonly float COMBAT_COOLDOWN = 30.0f;

		public static PlayerLifeUpdated onPlayerLifeUpdated;

		/// <summary>
		/// Invoked prior to built-in death logic.
		/// </summary>
		public static event System.Action<PlayerLife> OnPreDeath;

		public delegate void PlayerDiedCallback(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator);

		/// <summary>
		/// Event for plugins when player dies.
		/// </summary>
		public static event PlayerDiedCallback onPlayerDied;

		private static void broadcastPlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
		{
			try
			{
				onPlayerDied?.Invoke(sender, cause, limb, instigator);
			}
			catch (System.Exception e)
			{
				UnturnedLog.warn("Plugin raised an exception from onPlayerDied:");
				UnturnedLog.exception(e);
			}
		}

		public static System.Action<PlayerLife> OnTellHealth_Global;
		public static System.Action<PlayerLife> OnTellFood_Global;
		public static System.Action<PlayerLife> OnTellWater_Global;
		public static System.Action<PlayerLife> OnTellVirus_Global;
		public static System.Action<PlayerLife> OnTellBleeding_Global;
		public static System.Action<PlayerLife> OnTellBroken_Global;

		public static System.Action<PlayerLife, EDeathCause, ELimb, CSteamID> RocketLegacyOnDeath;

		/// <summary>
		/// Invoked after player finishes respawning.
		/// </summary>
		public static System.Action<PlayerLife> OnRevived_Global;

		public delegate void RespawnPointSelector(PlayerLife sender, bool wantsToSpawnAtHome, ref Vector3 position, ref float yaw);
		public static event RespawnPointSelector OnSelectingRespawnPoint;

		public LifeUpdated onLifeUpdated;
		public HealthUpdated onHealthUpdated;
		public FoodUpdated onFoodUpdated;
		public WaterUpdated onWaterUpdated;
		public VirusUpdated onVirusUpdated;
		public StaminaUpdated onStaminaUpdated;
		public VisionUpdated onVisionUpdated;
		public OxygenUpdated onOxygenUpdated;
		public BleedingUpdated onBleedingUpdated;
		public BrokenUpdated onBrokenUpdated;
		public TemperatureUpdated onTemperatureUpdated;
		public Damaged onDamaged;
		public event Hurt onHurt;

		public bool wasPvPDeath
		{
			get;
			private set;
		}

		private static EDeathCause _deathCause;
		public static EDeathCause deathCause => _deathCause;

		private static ELimb _deathLimb;
		public static ELimb deathLimb => _deathLimb;

		private static CSteamID _deathKiller;
		public static CSteamID deathKiller => _deathKiller;

		private CSteamID recentKiller;
		private float lastTimeAggressive;
		private float lastTimeTookDamage;
		private float lastTimeCausedDamage;

		public bool isAggressor => Time.realtimeSinceStartup - lastTimeAggressive < COMBAT_COOLDOWN;

		/// <summary>
		/// Tracks this player as an aggressor if they were recently an aggressor or if they haven't been attacked recently.
		/// </summary>
		/// <param name="force">Ignores rules and just make aggressive.</param>
		/// <param name="spreadToGroup">Whether to call markAggressive on group members.</param>
		public void markAggressive(bool force, bool spreadToGroup = true)
		{
			if (force || Time.realtimeSinceStartup - lastTimeAggressive < COMBAT_COOLDOWN)
			{
				lastTimeAggressive = Time.realtimeSinceStartup;
			}
			else
			{
				// If they haven't been attacked or they were last attacked a while ago and I haven't done damage recently they were probably aggressive
				if (recentKiller == CSteamID.Nil || Time.realtimeSinceStartup - lastTimeTookDamage > COMBAT_COOLDOWN)
				{
					lastTimeAggressive = Time.realtimeSinceStartup;
				}
			}

			if (spreadToGroup && player.quests.isMemberOfAGroup)
			{
				for (int index = 0; index < Provider.clients.Count; index++)
				{
					if (Provider.clients[index].playerID.steamID != channel.owner.playerID.steamID && player.quests.isMemberOfSameGroupAs(Provider.clients[index].player))
					{
						if (Provider.clients[index].player != null)
						{
							Provider.clients[index].player.life.markAggressive(force, false);
						}
					}
				}
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public bool enableGodMode;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		private bool _isDead;
		public bool isDead => _isDead;

		public bool IsAlive => !_isDead;

		private byte lastHealth;
		private byte _health;
		public byte health => _health;

		private byte _food;
		public byte food => _food;

		private byte _water;
		public byte water => _water;

		private byte _virus;
		public byte virus => _virus;

		private byte _vision;
		public byte vision => _vision;

		private uint _warmth;
		public uint warmth => _warmth;

		private byte _stamina;
		public byte stamina => _stamina;

		private byte _oxygen;
		public byte oxygen => _oxygen;

		private bool _isBleeding;
		public bool isBleeding => _isBleeding;

		private bool _isBroken;
		public bool isBroken => _isBroken;

		private EPlayerTemperature _temperature;
		public EPlayerTemperature temperature => _temperature;

		private uint lastStarve;
		private uint lastDehydrate;
		private uint lastUncleaned;
		private uint lastView;
		internal uint lastTire;
		private uint lastSuffocate;
		internal uint lastRest;
		private uint lastBreath;
		private uint lastInfect;
		private uint lastBleed;
		private uint lastBleeding;
		private uint lastBroken;
		private uint lastFreeze;
		private uint lastWarm;
		private uint lastBurn;
		private uint lastCovered;
		private uint lastRegenerate;
		private uint lastRadiate;
		private uint lastOutsideDeadzoneFrame;
		private float pendingDeadzoneDamage;
		private float pendingDeadzoneRadiation;
		private float pendingDeadzoneMaskFilterQualityLoss;

		private bool wasWarm;
		private bool wasCovered;

		private float _lastRespawn = -1.0f;
		public float lastRespawn => _lastRespawn;

		private float _lastDeath;
		public float lastDeath => _lastDeath;
		private float lastSuicide;

		private float lastAlive;

		private Vector3 ragdoll;
		private ERagdollEffect ragdollEffect;

		private PlayerSpawnpoint spawnpoint;

		[System.Obsolete]
		public void tellDeath(CSteamID steamID, byte newCause, byte newLimb, CSteamID newKiller)
		{
			ReceiveDeath((EDeathCause) newCause, (ELimb) newLimb, newKiller);
		}

		private static readonly ClientInstanceMethod<EDeathCause, ELimb, CSteamID> SendDeath = ClientInstanceMethod<EDeathCause, ELimb, CSteamID>.Get(typeof(PlayerLife), nameof(ReceiveDeath));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellDeath))]
		public void ReceiveDeath(EDeathCause newCause, ELimb newLimb, CSteamID newKiller)
		{
			_deathCause = newCause;
			_deathLimb = newLimb;
			_deathKiller = newKiller;

			if (channel.IsLocalPlayer) // always true?
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Deaths_Players", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Deaths_Players", data + 1);
				}
			}
		}

		[System.Obsolete]
		public void tellDead(CSteamID steamID, Vector3 newRagdoll, byte newRagdollEffect)
		{
			ReceiveDead(newRagdoll, (ERagdollEffect) newRagdollEffect);
		}

		private static readonly ClientInstanceMethod<Vector3, ERagdollEffect> SendDead = ClientInstanceMethod<Vector3, ERagdollEffect>.Get(typeof(PlayerLife), nameof(ReceiveDead));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellDead))]
		public void ReceiveDead(Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
		{
			Profiler.BeginSample("tellDead");
			_isDead = true;
			_lastDeath = Time.realtimeSinceStartup;

			ragdoll = newRagdoll;
			ragdollEffect = newRagdollEffect;

			if (!Dedicator.IsDedicatedServer)
			{
				Profiler.BeginSample("Ragdoll");
				RagdollTool.ragdollPlayer(transform.position, transform.rotation, player.animator.thirdSkeleton, ragdoll, player.clothing, ragdollEffect);
				Profiler.EndSample();
			}

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				player.movement.UpdateCharacterControllerEnabled();
			}

			if (onLifeUpdated != null)
			{
				Profiler.BeginSample("Invoke onLifeUpdated");
				onLifeUpdated(isDead);
				Profiler.EndSample();
			}

			if (onPlayerLifeUpdated != null)
			{
				Profiler.BeginSample("Invoke onPlayerLifeUpdated");
				onPlayerLifeUpdated(player);
				Profiler.EndSample();
			}
			Profiler.EndSample();
		}

		[System.Obsolete]
		public void tellRevive(CSteamID steamID, Vector3 position, byte angle)
		{
			ReceiveRevive(position, angle);
		}

		private static readonly ClientInstanceMethod<Vector3, byte> SendRevive = ClientInstanceMethod<Vector3, byte>.Get(typeof(PlayerLife), nameof(ReceiveRevive));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellRevive))]
		public void ReceiveRevive(Vector3 position, byte angle)
		{
			_isDead = false;
			_lastRespawn = Time.realtimeSinceStartup;

			player.ReceiveTeleport(position, angle);

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				player.movement.UpdateCharacterControllerEnabled();
			}

			onLifeUpdated?.Invoke(isDead);

			onPlayerLifeUpdated?.Invoke(player);

			try
			{
				OnRevived_Global?.Invoke(this);
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Plugin threw an exception during OnRevived_Global:");
			}
		}

		[System.Obsolete("Prior to saving/loading oxygen the client assumed it started at 100, but now needs the exact value.")]
		public void tellLife(CSteamID steamID, byte newHealth, byte newFood, byte newWater, byte newVirus, bool newBleeding, bool newBroken)
		{
			tellLifeWithOxygen(steamID, newHealth, newFood, newWater, newVirus, 100, newBleeding, newBroken);
		}

		[System.Obsolete]
		public void tellLifeWithOxygen(CSteamID steamID, byte newHealth, byte newFood, byte newWater, byte newVirus, byte newOxygen, bool newBleeding, bool newBroken)
		{
			ReceiveLifeStats(newHealth, newFood, newWater, newVirus, newOxygen, newBleeding, newBroken);
		}

		private static readonly ClientInstanceMethod<byte, byte, byte, byte, byte, bool, bool> SendLifeStats = ClientInstanceMethod<byte, byte, byte, byte, byte, bool, bool>.Get(typeof(PlayerLife), nameof(ReceiveLifeStats));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellLifeWithOxygen))]
		public void ReceiveLifeStats(byte newHealth, byte newFood, byte newWater, byte newVirus, byte newOxygen, bool newBleeding, bool newBroken)
		{
			Player.isLoadingLife = false;

			ReceiveHealth(newHealth);
			ReceiveFood(newFood);
			ReceiveWater(newWater);
			ReceiveVirus(newVirus);
			ReceiveBleeding(newBleeding);
			ReceiveBroken(newBroken);

			_stamina = 100;
			_oxygen = newOxygen;
			_vision = 0;
			_warmth = 0;

			_temperature = EPlayerTemperature.NONE;
			wasWarm = false;
			wasCovered = false;

			onVisionUpdated?.Invoke(false);

			onStaminaUpdated?.Invoke(stamina);

			onOxygenUpdated?.Invoke(oxygen);

			onTemperatureUpdated?.Invoke(temperature);

			lastAlive = Time.realtimeSinceStartup;
		}

		[System.Obsolete]
		public void askLife(CSteamID steamID)
		{ }

		internal void SendInitialPlayerState(SteamPlayer client)
		{
			if (channel.owner == client)
			{
				SendLifeStats.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, health, food, water, virus, oxygen, isBleeding, isBroken);
			}
			else if (isDead)
			{
				SendDead.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, ragdoll, ragdollEffect);
			}
		}

		internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
		{
			if (isDead)
			{
				SendDead.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, ragdoll, ragdollEffect);
			}
		}

		[System.Obsolete]
		public void tellHealth(CSteamID steamID, byte newHealth)
		{
			ReceiveHealth(newHealth);
		}

		private static readonly ClientInstanceMethod<byte> SendHealth = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), nameof(ReceiveHealth));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellHealth))]
		public void ReceiveHealth(byte newHealth)
		{
			_health = newHealth;

			onHealthUpdated?.Invoke(health);

			if (newHealth < lastHealth - 3)
			{
				onDamaged?.Invoke((byte) (lastHealth - newHealth));
			}

			lastHealth = newHealth;

			// Invoked here in case plugins are manually calling tellHealth.
			OnTellHealth_Global?.Invoke(this);
		}

		private static readonly ClientInstanceMethod<byte, Vector3> SendDamagedEvent = ClientInstanceMethod<byte, Vector3>.Get(typeof(PlayerLife), nameof(ReceiveDamagedEvent));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveDamagedEvent(byte damageAmount, Vector3 damageDirection)
		{
			player.look.FlinchFromDamage(damageAmount, damageDirection);
		}

		[System.Obsolete]
		public void tellFood(CSteamID steamID, byte newFood)
		{
			ReceiveFood(newFood);
		}

		private static readonly ClientInstanceMethod<byte> SendFood = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), nameof(ReceiveFood));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellFood))]
		public void ReceiveFood(byte newFood)
		{
			_food = newFood;

			onFoodUpdated?.Invoke(food);

			// Invoked here in case plugins are manually calling tellFood.
			OnTellFood_Global?.Invoke(this);
		}

		[System.Obsolete]
		public void tellWater(CSteamID steamID, byte newWater)
		{
			ReceiveWater(newWater);
		}

		private static readonly ClientInstanceMethod<byte> SendWater = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), nameof(ReceiveWater));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWater))]
		public void ReceiveWater(byte newWater)
		{
			_water = newWater;

			onWaterUpdated?.Invoke(water);

			// Invoked here in case plugins are manually calling tellWater.
			OnTellWater_Global?.Invoke(this);
		}

		[System.Obsolete]
		public void tellVirus(CSteamID steamID, byte newVirus)
		{
			ReceiveVirus(newVirus);
		}

		private static readonly ClientInstanceMethod<byte> SendVirus = ClientInstanceMethod<byte>.Get(typeof(PlayerLife), nameof(ReceiveVirus));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVirus))]
		public void ReceiveVirus(byte newVirus)
		{
			_virus = newVirus;

			onVirusUpdated?.Invoke(virus);

			// Invoked here in case plugins are manually calling tellVirus.
			OnTellVirus_Global?.Invoke(this);
		}

		[System.Obsolete]
		public void tellBleeding(CSteamID steamID, bool newBleeding)
		{
			ReceiveBleeding(newBleeding);
		}

		private static readonly ClientInstanceMethod<bool> SendBleeding = ClientInstanceMethod<bool>.Get(typeof(PlayerLife), nameof(ReceiveBleeding));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellBleeding))]
		public void ReceiveBleeding(bool newBleeding)
		{
			_isBleeding = newBleeding;

			onBleedingUpdated?.Invoke(isBleeding);

			// Invoked here in case plugins are manually calling tellBleeding.
			OnTellBleeding_Global?.Invoke(this);
		}

		[System.Obsolete]
		public void tellBroken(CSteamID steamID, bool newBroken)
		{
			ReceiveBroken(newBroken);
		}

		private static readonly ClientInstanceMethod<bool> SendBroken = ClientInstanceMethod<bool>.Get(typeof(PlayerLife), nameof(ReceiveBroken));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellBroken))]
		public void ReceiveBroken(bool newBroken)
		{
			_isBroken = newBroken;

			onBrokenUpdated?.Invoke(isBroken);

			// Invoked here in case plugins are manually calling tellBroken.
			OnTellBroken_Global?.Invoke(this);
		}

		public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill)
		{
			askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill: false, newRagdollEffect: ERagdollEffect.None, canCauseBleeding: true);
		}

		public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.None)
		{
			askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill: trackKill, newRagdollEffect: newRagdollEffect, canCauseBleeding: true);
		}

		public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.None, bool canCauseBleeding = true)
		{
			askDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill: trackKill, newRagdollEffect: newRagdollEffect, canCauseBleeding: canCauseBleeding, bypassSafezone: false);
		}

		/// <param name="bypassSafezone">Should damage be dealt even while inside safezone?</param>
		public void askDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.None, bool canCauseBleeding = true, bool bypassSafezone = false)
		{
			kill = EPlayerKill.NONE;

			if (!bypassSafezone && !InternalCanDamage())
			{
				return;
			}

			doDamage(amount, newRagdoll, newCause, newLimb, newKiller, out kill, trackKill: trackKill, newRagdollEffect: newRagdollEffect, canCauseBleeding: canCauseBleeding);
		}

		internal bool InternalCanDamage()
		{
			if (player.movement.isSafe && player.movement.isSafeInfo.noIncomingDamage)
			{
				return false;
			}

			// Half a second of damage immunity after respawn.
			if (lastRespawn > 0.0f && Time.realtimeSinceStartup - lastRespawn < 0.5f)
			{
				return false;
			}

			return true;
		}

		private void doDamage(byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, out EPlayerKill kill, bool trackKill = false, ERagdollEffect newRagdollEffect = ERagdollEffect.None, bool canCauseBleeding = true)
		{
			kill = EPlayerKill.NONE;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (enableGodMode)
				return;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= health)
				{
					_health = 0;
				}
				else
				{
					_health -= amount;
				}

				ragdoll = newRagdoll;
				ragdollEffect = newRagdollEffect;

				// This threshold of 3 corresponds to the older onDamaged event threshold.
				if (_health > 0 && amount > 3)
				{
					SendDamagedEvent.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), amount, newRagdoll.normalized);
				}

				SendHealth.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), health);
				OnTellHealth_Global?.Invoke(this);

				if (newCause == EDeathCause.GUN || newCause == EDeathCause.MELEE || newCause == EDeathCause.PUNCH || newCause == EDeathCause.ROADKILL || newCause == EDeathCause.GRENADE || newCause == EDeathCause.MISSILE || newCause == EDeathCause.CHARGE)
				{
					recentKiller = newKiller;
					lastTimeTookDamage = Time.realtimeSinceStartup;

					Player killer = PlayerTool.getPlayer(recentKiller);
					if (killer != null)
					{
						killer.life.lastTimeCausedDamage = Time.realtimeSinceStartup;

						// If they were just being aggressive this is probably a continuation of that, so reset their timer to now
						if (Time.realtimeSinceStartup - killer.life.lastTimeAggressive < COMBAT_COOLDOWN)
						{
							killer.life.markAggressive(true);
						}
						else
						{
							// If they haven't been attacked or they were last attacked a while ago and I haven't done damage recently they were probably aggressive
							if ((killer.life.recentKiller == CSteamID.Nil || Time.realtimeSinceStartup - killer.life.lastTimeTookDamage > COMBAT_COOLDOWN) && Time.realtimeSinceStartup - lastTimeCausedDamage > COMBAT_COOLDOWN)
							{
								killer.life.markAggressive(true);
							}
						}
					}
				}

				if (health == 0)
				{
					try
					{
						// Nelson 2024-10-04: Calling here rather than OnLifeUpdated so the player is mostly alive still.
						player.quests.InterruptDelayedQuestRewards(EDelayedQuestRewardsInterruption.Death);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, "Caught exception interrupting delayed quest rewards on death:");
					}

					if (recentKiller != CSteamID.Nil && recentKiller != channel.owner.playerID.steamID && Time.realtimeSinceStartup - lastTimeTookDamage < COMBAT_COOLDOWN)
					{
						Player killer = PlayerTool.getPlayer(recentKiller);
						if (killer != null)
						{
							//int rep = player.skills.reputation;
							//if(rep == 0)
							//{
							//	rep = 1;
							//}
							//else
							//{
							//	rep = Mathf.Clamp(rep, -20, 20);
							//	rep = -rep;
							//}

							//if(isKillerAggressor)
							//{
							//	rep = -Mathf.Abs(rep);
							//}

							int rep = Mathf.Abs(player.skills.reputation);
							rep = Mathf.Clamp(rep, 1, 25);

							if (killer.life.isAggressor)
							{
								rep = -rep;
							}

							killer.skills.askRep(rep);
						}
					}

					kill = EPlayerKill.PLAYER;

					wasPvPDeath = newCause == EDeathCause.GUN || newCause == EDeathCause.MELEE || newCause == EDeathCause.PUNCH || newCause == EDeathCause.ROADKILL || newCause == EDeathCause.GRENADE || newCause == EDeathCause.MISSILE || newCause == EDeathCause.CHARGE || newCause == EDeathCause.SENTRY; // || newCause == EDeathCause.SHRED || newCause == EDeathCause.LANDMINE

					OnPreDeath.TryInvoke("OnPreDeath", this);

					// Prior to 2020-04-27 this was handled by PlayerMovement.onLifeUpdated, but I suspect plugins might
					// be teleporting player or doing something unexpected during onLifeUpdated messing with death.
					player.movement.forceRemoveFromVehicle();

					RocketLegacyOnDeath.TryInvoke("RocketLegacyOnDeath", this, newCause, newLimb, newKiller);

					try
					{
						SendDeath.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), newCause, newLimb, newKiller);
						SendDead.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), ragdoll, ragdollEffect);
					}
					catch (System.Exception exception)
					{
						// Considering tellDead calls the onLifeUpdated event which so much code including plugins hooks we catch any exceptions.
						UnturnedLog.warn("Exception during tellDeath or tellDead:");
						UnturnedLog.exception(exception);
					}

					if (spawnpoint == null || (newCause != EDeathCause.SUICIDE && newCause != EDeathCause.BREATH) || Time.realtimeSinceStartup - lastSuicide > 60.0f)
					{
						spawnpoint = LevelPlayers.getSpawn(false);
					}

					if (newCause == EDeathCause.SUICIDE || newCause == EDeathCause.BREATH)
					{
						lastSuicide = Time.realtimeSinceStartup;
					}

					if (trackKill)
					{
						const float maxTrackDistance = 300 * 300;
						for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
						{
							SteamPlayer enemyPlayer = Provider.clients[playerIndex];

							if (enemyPlayer.player == null || enemyPlayer.player.movement == null || enemyPlayer.player.life == null || enemyPlayer.player.life.isDead)
							{
								continue;
							}

							if (enemyPlayer == channel.owner)
							{
								// We shouldn't be able to kill ourself to progress quests!
								continue;
							}

							if ((enemyPlayer.player.transform.position - transform.position).sqrMagnitude < maxTrackDistance)
							{
								enemyPlayer.player.quests.trackPlayerKill(player);
							}
						}
					}

					broadcastPlayerDied(this, newCause, newLimb, newKiller);

					if (CommandWindow.shouldLogDeaths)
					{
						if (newCause == EDeathCause.BLEEDING)
						{
							CommandWindow.Log(Provider.localization.format("Bleeding", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.BONES)
						{
							CommandWindow.Log(Provider.localization.format("Bones", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.FREEZING)
						{
							CommandWindow.Log(Provider.localization.format("Freezing", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.BURNING)
						{
							CommandWindow.Log(Provider.localization.format("Burning", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.FOOD)
						{
							CommandWindow.Log(Provider.localization.format("Food", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.WATER)
						{
							CommandWindow.Log(Provider.localization.format("Water", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.GUN || newCause == EDeathCause.MELEE || newCause == EDeathCause.PUNCH || newCause == EDeathCause.ROADKILL || newCause == EDeathCause.GRENADE || newCause == EDeathCause.MISSILE || newCause == EDeathCause.CHARGE || newCause == EDeathCause.SPLASH)
						{
							SteamPlayer killer = PlayerTool.getSteamPlayer(newKiller);

							string characterName;
							string playerName;

							if (killer != null)
							{
								characterName = killer.playerID.characterName;
								playerName = killer.playerID.playerName;
							}
							else
							{
								characterName = "?";
								playerName = "?";
							}

							string limb = "";
							if (newLimb == ELimb.LEFT_FOOT || newLimb == ELimb.LEFT_LEG || newLimb == ELimb.RIGHT_FOOT || newLimb == ELimb.RIGHT_LEG)
							{
								limb = Provider.localization.format("Leg");
							}
							else if (newLimb == ELimb.LEFT_HAND || newLimb == ELimb.LEFT_ARM || newLimb == ELimb.RIGHT_HAND || newLimb == ELimb.RIGHT_ARM)
							{
								limb = Provider.localization.format("Arm");
							}
							else if (newLimb == ELimb.SPINE)
							{
								limb = Provider.localization.format("Spine");
							}
							else if (newLimb == ELimb.SKULL)
							{
								limb = Provider.localization.format("Skull");
							}

							if (newCause == EDeathCause.GUN)
							{
								CommandWindow.Log(Provider.localization.format("Gun", channel.owner.playerID.characterName, channel.owner.playerID.playerName, limb, characterName, playerName));
							}
							else if (newCause == EDeathCause.MELEE)
							{
								CommandWindow.Log(Provider.localization.format("Melee", channel.owner.playerID.characterName, channel.owner.playerID.playerName, limb, characterName, playerName));
							}
							else if (newCause == EDeathCause.PUNCH)
							{
								CommandWindow.Log(Provider.localization.format("Punch", channel.owner.playerID.characterName, channel.owner.playerID.playerName, limb, characterName, playerName));
							}
							else if (newCause == EDeathCause.ROADKILL)
							{
								CommandWindow.Log(Provider.localization.format("Roadkill", channel.owner.playerID.characterName, channel.owner.playerID.playerName, characterName, playerName));
							}
							else if (newCause == EDeathCause.GRENADE)
							{
								CommandWindow.Log(Provider.localization.format("Grenade", channel.owner.playerID.characterName, channel.owner.playerID.playerName, characterName, playerName));
							}
							else if (newCause == EDeathCause.MISSILE)
							{
								CommandWindow.Log(Provider.localization.format("Missile", channel.owner.playerID.characterName, channel.owner.playerID.playerName, characterName, playerName));
							}
							else if (newCause == EDeathCause.CHARGE)
							{
								CommandWindow.Log(Provider.localization.format("Charge", channel.owner.playerID.characterName, channel.owner.playerID.playerName, characterName, playerName));
							}
							else if (newCause == EDeathCause.SPLASH)
							{
								CommandWindow.Log(Provider.localization.format("Splash", channel.owner.playerID.characterName, channel.owner.playerID.playerName, characterName, playerName));
							}
						}
						else if (newCause == EDeathCause.ZOMBIE)
						{
							CommandWindow.Log(Provider.localization.format("Zombie", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.ANIMAL)
						{
							CommandWindow.Log(Provider.localization.format("Animal", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.SUICIDE)
						{
							CommandWindow.Log(Provider.localization.format("Suicide", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.INFECTION)
						{
							CommandWindow.Log(Provider.localization.format("Infection", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.BREATH)
						{
							CommandWindow.Log(Provider.localization.format("Breath", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.ZOMBIE)
						{
							CommandWindow.Log(Provider.localization.format("Zombie", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.VEHICLE)
						{
							CommandWindow.Log(Provider.localization.format("Vehicle", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.SHRED)
						{
							CommandWindow.Log(Provider.localization.format("Shred", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.LANDMINE)
						{
							CommandWindow.Log(Provider.localization.format("Landmine", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.ARENA)
						{
							CommandWindow.Log(Provider.localization.format("Arena", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.SENTRY)
						{
							CommandWindow.Log(Provider.localization.format("Sentry", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.ACID)
						{
							CommandWindow.Log(Provider.localization.format("Acid", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.BOULDER)
						{
							CommandWindow.Log(Provider.localization.format("Boulder", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.BURNER)
						{
							CommandWindow.Log(Provider.localization.format("Burner", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.SPIT)
						{
							CommandWindow.Log(Provider.localization.format("Spit", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
						else if (newCause == EDeathCause.SPARK)
						{
							CommandWindow.Log(Provider.localization.format("Spark", channel.owner.playerID.characterName, channel.owner.playerID.playerName));
						}
					}
				}
				else
				{
					if (Provider.modeConfigData.Players.Can_Start_Bleeding && canCauseBleeding)
					{
						if (amount >= 20)
						{
							serverSetBleeding(true);
						}
					}
				}

				onHurt?.Invoke(player, amount, newRagdoll, newCause, newLimb, newKiller);
			}
		}

		public void askHeal(byte amount, bool healBleeding, bool healBroken)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= 100 - health)
				{
					_health = 100;
				}
				else
				{
					_health += amount;
				}

				SendHealth.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), health);
				OnTellHealth_Global?.Invoke(this);

				if (isBleeding && healBleeding)
				{
					serverSetBleeding(false);
				}

				if (isBroken && healBroken)
				{
					serverSetLegsBroken(false);
				}
			}
		}

		/// <summary>
		/// Set bleeding state and replicate to owner if changed.
		/// </summary>
		public void serverSetBleeding(bool newBleeding)
		{
			if (newBleeding)
			{
				// Reset timer.
				lastBleeding = player.input.simulation;
				lastBleed = player.input.simulation;
			}

			if (isBleeding != newBleeding)
			{
				_isBleeding = newBleeding;
				SendBleeding.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), isBleeding);
				OnTellBleeding_Global?.Invoke(this);
			}
		}

		/// <summary>
		/// Set legs broken state and replicate to owner if changed.
		/// </summary>
		public void serverSetLegsBroken(bool newLegsBroken)
		{
			if (newLegsBroken)
			{
				// Reset timer.
				lastBroken = player.input.simulation;
			}

			if (isBroken != newLegsBroken)
			{
				_isBroken = newLegsBroken;
				SendBroken.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), isBroken);
				OnTellBroken_Global?.Invoke(this);
			}
		}

		public void askStarve(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= food)
				{
					_food = 0;
				}
				else
				{
					_food -= amount;
				}

				SendFood.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), food);
				OnTellFood_Global?.Invoke(this);
			}
		}

		public void askEat(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= 100 - food)
				{
					_food = 100;
				}
				else
				{
					_food += amount;
				}

				SendFood.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), food);
				OnTellFood_Global?.Invoke(this);
			}
		}

		public void askDehydrate(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= water)
				{
					_water = 0;
				}
				else
				{
					_water -= amount;
				}

				SendWater.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), water);
				OnTellWater_Global?.Invoke(this);
			}
		}

		public void askDrink(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= 100 - water)
				{
					_water = 100;
				}
				else
				{
					_water += amount;
				}

				SendWater.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), water);
				OnTellWater_Global?.Invoke(this);
			}
		}

		public void askInfect(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= virus)
				{
					_virus = 0;
				}
				else
				{
					_virus -= amount;
				}

				SendVirus.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), virus);
				OnTellVirus_Global?.Invoke(this);
			}
		}

		public void askRadiate(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= virus)
				{
					_virus = 0;
				}
				else
				{
					_virus -= amount;
				}

				onVirusUpdated?.Invoke(virus);

				// Nelson 2024-03-13: askRadiate doesn't call SendVirus because it's called on both client and server,
				// but that means plugins don't currently have a way to listen for this event. Adding the call to
				// OnTellVirus_Global to match other calls to SendVirus. (public issue #4373)
				OnTellVirus_Global?.Invoke(this);
			}
		}

		public void askDisinfect(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= 100 - virus)
				{
					_virus = 100;
				}
				else
				{
					_virus += amount;
				}

				SendVirus.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), virus);
				OnTellVirus_Global?.Invoke(this);
			}
		}

		internal void internalSetStamina(byte value)
		{
			_stamina = value;
			onStaminaUpdated?.Invoke(stamina);
		}

		public void askTire(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				lastTire = player.input.simulation;

				if (amount >= stamina)
				{
					_stamina = 0;
				}
				else
				{
					_stamina -= amount;
				}

				onStaminaUpdated?.Invoke(stamina);
			}
		}

		public void askRest(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= 100 - stamina)
				{
					_stamina = 100;
				}
				else
				{
					_stamina += amount;
				}

				onStaminaUpdated?.Invoke(stamina);
			}
		}

		/// <summary>
		/// Add to or subtract from stamina level.
		/// Does not replicate the change.
		/// </summary>
		public void simulatedModifyStamina(short delta)
		{
			if (delta > 0)
			{
				askRest((byte) delta);
			}
			else if (delta < 0)
			{
				askTire((byte) -delta);
			}
		}

		/// <summary>
		/// Add to or subtract from stamina level.
		/// Does not replicate the change.
		/// </summary>
		public void simulatedModifyStamina(float delta)
		{
			simulatedModifyStamina(MathfEx.RoundAndClampToShort(delta));
		}

		[System.Obsolete]
		public void clientModifyStamina(CSteamID senderId, short delta)
		{
			ReceiveModifyStamina(delta);
		}

		private static readonly ClientInstanceMethod<short> SendModifyStamina = ClientInstanceMethod<short>.Get(typeof(PlayerLife), nameof(ReceiveModifyStamina));
		/// <summary>
		/// Called from the server to modify stamina.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(clientModifyStamina))]
		public void ReceiveModifyStamina(short delta)
		{
			simulatedModifyStamina(delta);
		}

		/// <summary>
		/// Add to or subtract from stamina level on the client and server.
		/// </summary>
		public void serverModifyStamina(float delta)
		{
			short roundedDelta = MathfEx.RoundAndClampToShort(delta);
			if (roundedDelta != 0)
			{
				simulatedModifyStamina(roundedDelta);

				if (channel.IsLocalPlayer == false)
				{
					SendModifyStamina.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), roundedDelta);
				}
			}
		}

		public void askView(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				lastView = player.input.simulation;
				_vision = amount;

				onVisionUpdated?.Invoke(true);
			}
		}

		[System.Obsolete]
		public void clientModifyHallucination(CSteamID senderId, short delta)
		{
			ReceiveModifyHallucination(delta);
		}

		private static readonly ClientInstanceMethod<short> SendModifyHallucination = ClientInstanceMethod<short>.Get(typeof(PlayerLife), nameof(ReceiveModifyHallucination));
		/// <summary>
		/// Called from the server to induce a hallucination.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(clientModifyHallucination))]
		public void ReceiveModifyHallucination(short delta)
		{
			if (delta > 0)
			{
				askView((byte) delta);
			}
			else if (delta < 0)
			{
				askBlind((byte) -delta);
			}
		}

		/// <summary>
		/// Add to or subtract from hallucination level on the client.
		/// </summary>
		public void serverModifyHallucination(float delta)
		{
			short roundedDelta = MathfEx.RoundAndClampToShort(delta);
			if (roundedDelta != 0)
			{
				SendModifyHallucination.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), roundedDelta);
			}
		}

		[System.Obsolete("Use serverModifyHallucination instead.")]
		public void tellHallucinate(CSteamID senderId, byte amount)
		{
			clientModifyHallucination(senderId, amount);
		}

		[System.Obsolete("Use serverModifyHallucination instead.")]
		public void sendHallucination(byte amount)
		{
			serverModifyHallucination(amount);
		}

		/// <summary>
		/// Add to or subtract from warmth level.
		/// Does not replicate the change.
		/// </summary>
		public void simulatedModifyWarmth(short delta)
		{
			if (delta == 0 || isDead)
			{
				return;
			}

			if (delta > 0)
			{
				_warmth = (uint) (_warmth + delta);
			}
			else if (delta < 0)
			{
				_warmth = (uint) Mathf.Max(0, (int) _warmth + delta);
			}
		}

		[System.Obsolete]
		public void clientModifyWarmth(CSteamID senderId, short delta)
		{
			ReceiveModifyWarmth(delta);
		}

		private static readonly ClientInstanceMethod<short> SendModifyWarmth = ClientInstanceMethod<short>.Get(typeof(PlayerLife), nameof(ReceiveModifyWarmth));
		/// <summary>
		/// Called from the server to modify warmth.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(clientModifyWarmth))]
		public void ReceiveModifyWarmth(short delta)
		{
			simulatedModifyWarmth(delta);
		}

		/// <summary>
		/// Add to or subtract from warmth level on the client and server.
		/// </summary>
		public void serverModifyWarmth(float delta)
		{
			short roundedDelta = MathfEx.RoundAndClampToShort(delta);
			if (roundedDelta != 0)
			{
				simulatedModifyWarmth(roundedDelta);

				if (channel.IsLocalPlayer == false)
				{
					SendModifyWarmth.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), roundedDelta);
				}
			}
		}

		public void askBlind(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= vision)
				{
					_vision = 0;
				}
				else
				{
					_vision -= amount;
				}

				if (vision == 0)
				{
					onVisionUpdated?.Invoke(false);
				}
			}
		}

		public void askSuffocate(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				lastSuffocate = player.input.simulation;

				if (amount >= oxygen)
				{
					_oxygen = 0;
				}
				else
				{
					_oxygen -= amount;
				}

				onOxygenUpdated?.Invoke(oxygen);
			}
		}

		public void askBreath(byte amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
				if (amount >= 100 - oxygen)
				{
					_oxygen = 100;
				}
				else
				{
					_oxygen += amount;
				}

				onOxygenUpdated?.Invoke(oxygen);
			}
		}

		/// <summary>
		/// Add to or subtract from oxygen level.
		/// Does not replicate the change.
		/// </summary>
		public void simulatedModifyOxygen(sbyte delta)
		{
			if (delta > 0)
			{
				byte amount = (byte) delta;
				askBreath(amount);
			}
			else if (delta < 0)
			{
				byte amount = (byte) -delta;
				askSuffocate(amount);
			}
		}

		public void simulatedModifyOxygen(float delta)
		{
			simulatedModifyOxygen(MathfEx.RoundAndClampToSByte(delta));
		}

		/// <summary>
		/// Add to or subtract from health level.
		/// Replicates change to owner.
		/// </summary>
		public void serverModifyHealth(float delta)
		{
			if (delta > 0)
			{
				byte amount = MathfEx.RoundAndClampToByte(delta);
				askHeal(amount, false, false);
			}
			else
			{
				byte amount = MathfEx.RoundAndClampToByte(-delta);
				EPlayerKill kill;
				// Nelson 2024-09-23: Changed this to not cause bleeding. (public issue #4670)
				askDamage(amount, Vector3.up, EDeathCause.SUICIDE, ELimb.SPINE, CSteamID.Nil, out kill, trackKill: false,
					newRagdollEffect: ERagdollEffect.None,
					canCauseBleeding: false);
			}
		}

		/// <summary>
		/// Add to or subtract from food level.
		/// Replicates change to owner.
		/// </summary>
		public void serverModifyFood(float delta)
		{
			if (delta > 0)
			{
				byte amount = MathfEx.RoundAndClampToByte(delta);
				askEat(amount);
			}
			else
			{
				byte amount = MathfEx.RoundAndClampToByte(-delta);
				askStarve(amount);
			}
		}

		/// <summary>
		/// Add to or subtract from water level.
		/// Replicates change to owner.
		/// </summary>
		public void serverModifyWater(float delta)
		{
			if (delta > 0)
			{
				byte amount = MathfEx.RoundAndClampToByte(delta);
				askDrink(amount);
			}
			else
			{
				byte amount = MathfEx.RoundAndClampToByte(-delta);
				askDehydrate(amount);
			}
		}

		/// <summary>
		/// Add to or subtract from virus level.
		/// Replicates change to owner.
		/// </summary>
		public void serverModifyVirus(float delta)
		{
			if (delta > 0)
			{
				byte amount = MathfEx.RoundAndClampToByte(delta);
				askDisinfect(amount);
			}
			else
			{
				byte amount = MathfEx.RoundAndClampToByte(-delta);
				askInfect(amount);
			}
		}

		[System.Obsolete]
		public void askRespawn(CSteamID steamID, bool atHome)
		{
			ReceiveRespawnRequest(atHome);
		}

		/// <summary>
		/// Used by plugins to respawn the player bypassing timers. Issue #2701
		/// </summary>
		public void ServerRespawn(bool atHome)
		{
			if (IsAlive)
				return;

			sendRevive();

			Vector3 point;
			byte angle;

			if (!atHome || !BarricadeManager.tryGetBed(channel.owner.playerID.steamID, out point, out angle))
			{
				if (spawnpoint == null)
				{
					// Should have been set during death.
					spawnpoint = LevelPlayers.getSpawn(false);
				}

				if (spawnpoint == null)
				{
					// Still unable to get one, so don't teleport.
					point = transform.position;
					angle = 0;
				}
				else
				{
					point = spawnpoint.point;
					angle = MeasurementTool.angleToByte(spawnpoint.angle);
				}

				string npcSpawnId = player.quests.npcSpawnId;
				if (!string.IsNullOrEmpty(npcSpawnId))
				{
					SDG.Framework.Devkit.Spawnpoint devkitSpawnpoint = SDG.Framework.Devkit.SpawnpointSystemV2.Get().FindFirstSpawnpoint(npcSpawnId);
					if (devkitSpawnpoint != null)
					{
						point = devkitSpawnpoint.transform.position;
						angle = MeasurementTool.angleToByte(devkitSpawnpoint.transform.rotation.eulerAngles.y);
					}
					else
					{
						LocationDevkitNode locationNode = LocationDevkitNodeSystem.Get().FindByName(npcSpawnId);
						if (locationNode != null)
						{
							point = locationNode.transform.position;
							angle = MeasurementTool.angleToByte(Random.Range(0.0f, 360.0f));
						}
						else
						{
							player.quests.npcSpawnId = null;
							UnturnedLog.warn($"Unable to find spawnpoint or location matching NpcSpawnId \"{npcSpawnId}\"");
						}
					}
				}
			}

			if (OnSelectingRespawnPoint != null)
			{
				float yaw = MeasurementTool.byteToAngle(angle);
				OnSelectingRespawnPoint.Invoke(this, atHome, ref point, ref yaw);
				angle = MeasurementTool.angleToByte(yaw);
			}

			point += new Vector3(0, 0.5f, 0);

			SendReviveTeleport(point, angle);
		}

		/// <summary>
		/// Very similar to VehicleManager.sendExitVehicle. Please refer to that for comments.
		/// </summary>
		private void SendReviveTeleport(Vector3 point, byte packedAngle)
		{
			SteamPlayer teleportingClient = channel.owner;

			// Prior to player culling this used Provider.GatherRemoteClientConnections().
			// Now, we send a fake position to clients who shouldn't know the new position.
			// Please refer to Player.GatherTeleportRemoteClientConnections for more info.
			PooledTransportConnectionList visibleClients = TransportConnectionListPool.Get();
			PooledTransportConnectionList culledClients = TransportConnectionListPool.Get();

			foreach (SteamPlayer client in Provider._clients)
			{
#if !DEDICATED_SERVER
				if (client.IsLocalServerHost)
					continue;
#endif // !DEDICATED_SERVER

				if (client == teleportingClient)
				{
					// Always notify self of the teleport.
					visibleClients.Add(client.transportConnection);
					continue;
				}

				if (client.model == null) // error/bug?
				{
					visibleClients.Add(client.transportConnection);
					continue;
				}

				Vector3 recipientPosition = client.model.transform.position;
				bool culled = PlayerManager.IsPlayerCulledAtPosition(teleportingClient, point, client, recipientPosition);
				if (culled)
				{
					culledClients.Add(client.transportConnection);
				}
				else
				{
					visibleClients.Add(client.transportConnection);
				}
			}

			// Always invoke (even if empty) so loopback is called.
			SendRevive.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, visibleClients, point, packedAngle);

			if (culledClients.Count > 0)
			{
				// No loopback.
				SendRevive.Invoke(GetNetId(), ENetReliability.Reliable, culledClients, PlayerManager.CulledPosition, packedAngle);
			}
		}

		private static readonly ServerInstanceMethod<bool> SendRespawnRequest = ServerInstanceMethod<bool>.Get(typeof(PlayerLife), nameof(ReceiveRespawnRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askRespawn))]
		public void ReceiveRespawnRequest(bool atHome)
		{
			if (!Provider.isServer)
				return;

			if (IsAlive)
			{
				// Perhaps they spam-clicked the respawn button and we already respawned them.
				return;
			}

			// First ensure that enough time has passed:
			if (atHome)
			{
				if (Dedicator.IsDedicatedServer && Provider.isPvP)
				{
					// Extended home timer is not used in singleplayer/offline.
					if (Time.realtimeSinceStartup - lastDeath < Provider.modeConfigData.Gameplay.Timer_Home)
						return;
				}
				else
				{
					if (Time.realtimeSinceStartup - lastRespawn < Provider.modeConfigData.Gameplay.Timer_Respawn)
						return;
				}
			}
			else
			{
				if (Time.realtimeSinceStartup - lastRespawn < Provider.modeConfigData.Gameplay.Timer_Respawn)
					return;
			}

			ServerRespawn(atHome);
		}

		[System.Obsolete]
		public void askSuicide(CSteamID steamID)
		{
			ReceiveSuicideRequest();
		}

		private static readonly ServerInstanceMethod SendSuicideRequest = ServerInstanceMethod.Get(typeof(PlayerLife), nameof(ReceiveSuicideRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSuicide))]
		public void ReceiveSuicideRequest()
		{
			if (IsAlive)
			{
				if ((Level.info != null && Level.info.type == ELevelType.SURVIVAL) || !(player.movement.isSafe && player.movement.isSafeInfo.noIncomingDamage))
				{
					if (Provider.modeConfigData.Gameplay.Can_Suicide)
					{
						EPlayerKill kill;
						doDamage(100, Vector3.up * 10, EDeathCause.SUICIDE, ELimb.SKULL, channel.owner.playerID.steamID, out kill);
					}
				}
			}
		}

		/// <summary>
		/// Used to refill all client stats like stamina
		/// </summary>
		public void sendRevive()
		{
			_health = (byte) Provider.modeConfigData.Players.Health_Default;
			_food = (byte) Provider.modeConfigData.Players.Food_Default;
			_water = (byte) Provider.modeConfigData.Players.Water_Default;
			_virus = (byte) Provider.modeConfigData.Players.Virus_Default;
			_stamina = 100;
			_oxygen = 100;
			_vision = 0;
			_warmth = 0;
			_isBleeding = false;
			_isBroken = false;

			_temperature = EPlayerTemperature.NONE;
			wasWarm = false;
			wasCovered = false;

			lastStarve = player.input.simulation;
			lastDehydrate = player.input.simulation;
			lastUncleaned = player.input.simulation;
			lastTire = player.input.simulation;
			lastRest = player.input.simulation;
			lastRadiate = player.input.simulation; // Hack to prevent virus damage after respawning from deadzone.
			lastOutsideDeadzoneFrame = player.input.simulation; // Similar to lastRadiate, prevent damage until in deadzone for a brief period.
			pendingDeadzoneDamage = 0.0f;
			pendingDeadzoneRadiation = 0.0f;
			pendingDeadzoneMaskFilterQualityLoss = 0.0f;

			recentKiller = CSteamID.Nil;
			lastTimeAggressive = -100;
			lastTimeTookDamage = -100;
			lastTimeCausedDamage = -100;

			SendLifeStats.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), health, food, water, virus, oxygen, isBleeding, isBroken);
		}

		public void sendRespawn(bool atHome)
		{
			SendRespawnRequest.Invoke(GetNetId(), ENetReliability.Unreliable, atHome);
		}

		public void sendSuicide()
		{
			SendSuicideRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
		}

		internal void SimulateStaminaFrame(uint simulation)
		{
			if ((player.stance.stance == EPlayerStance.SPRINT || (player.stance.stance == EPlayerStance.DRIVING && player.movement.getVehicle() != null && player.movement.getVehicle().isBoosting))
				&& simulation - lastTire > 1 + player.skills.skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.EXERCISE].level)
			{
				lastTire = simulation;

				askTire(1);
			}

			if (stamina < 100)
			{
				if (simulation - lastTire > 32 * (1f - (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.CARDIO) * 0.5f)) && simulation - lastRest > 1)
				{
					lastRest = simulation;

					askRest((byte) (1f + (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.CARDIO) * 2f)));
				}
			}
		}

		/// <summary>
		/// Used by UI. True when underwater or inside non-breathable oxygen volume.
		/// </summary>
		internal bool isAsphyxiating;
		private void SetIsAsphyxiating(bool newIsAsphyxiating)
		{
			if (isAsphyxiating != newIsAsphyxiating)
			{
				isAsphyxiating = newIsAsphyxiating;
				OnIsAsphyxiatingChanged?.Invoke();
			}
		}

		internal event System.Action OnIsAsphyxiatingChanged;

		private void SimulateOxygenFrame(uint simulation)
		{
			Vector3 position = transform.position;

			// Measure of how breathable the current surroundings are.
			// -1 depletes oxygen, whereas +1 refills oxygen, and the neutral zone is slow or stagnant.
			float breathability;
			if (OxygenManager.checkPointBreathable(position))
			{
				// Oxygenator bypasses all other cases.
				breathability = 1.0f;
			}
			else
			{
				if (player.stance.isSubmerged)
				{
					breathability = -1.0f;
				}
				else
				{
					if (Level.info != null && Level.info.type == ELevelType.SURVIVAL)
					{
						if (Level.info.configData != null && Level.info.configData.Use_Legacy_Oxygen_Height)
						{
							float water = LevelLighting.getWaterSurfaceElevation(defaultValue: 0.0f);
							float height = Mathf.Clamp01((position.y - water) / (Level.HEIGHT - water));
							breathability = Mathf.Lerp(1.0f, -1.0f, height);
						}
						else
						{
							breathability = 1.0f;
						}
					}
					else
					{
						breathability = 1.0f;
					}
				}

				if (breathability > -0.9999f)
				{
					float alpha;
					if (OxygenVolumeManager.Get().IsPositionInsideNonBreathableVolume(position, out alpha))
					{
						breathability = Mathf.Lerp(breathability, -1.0f, alpha);
					}
				}

				if (breathability < 0.9999f)
				{
					float alpha;
					if (OxygenVolumeManager.Get().IsPositionInsideBreathableVolume(position, out alpha))
					{
						breathability = Mathf.Lerp(breathability, 1.0f, alpha);
					}
				}
			}

			if (breathability > 0.0f)
			{
				SetIsAsphyxiating(false);
				if (oxygen < 100)
				{
					// breathability closer to 1 = more safe

					if (simulation - lastBreath > (uint) (1 + Mathf.CeilToInt(10 * (1.0f - breathability))))
					{
						lastBreath = simulation;

						askBreath((byte) (1f + (player.skills.mastery((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.CARDIO) * 2f)));
					}
				}
			}
			else if (breathability < 0.0f)
			{
				SetIsAsphyxiating(true);
				if (oxygen > 0)
				{
					// This interval is the same as the old underwater interval which has -1.0 breathability.
					uint suffocationInterval = (uint) (1 + player.skills.skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.DIVING].level);

					// Anything other than -1.0 breathability extends the suffocation interval.
					suffocationInterval += (uint) Mathf.CeilToInt((breathability + 1.0f) * 10);

					bool hasUnderwaterBreathingApparatus = player.clothing.backpackAsset != null && player.clothing.backpackAsset.proofWater &&
						((player.clothing.glassesAsset != null && player.clothing.glassesAsset.proofWater) || (player.clothing.maskAsset != null && player.clothing.maskAsset.proofWater));
					if (hasUnderwaterBreathingApparatus)
					{
						suffocationInterval *= 10;
					}

					if (simulation - lastSuffocate > suffocationInterval)
					{
						lastSuffocate = simulation;

						askSuffocate(1);
					}
				}
				else
				{
					if (simulation - lastSuffocate > 10)
					{
						lastSuffocate = simulation;

						if (Provider.isServer)
						{
							EPlayerKill kill;
							doDamage(10, Vector3.up, EDeathCause.BREATH, ELimb.SPINE, Provider.server, out kill);
						}
					}
				}
			}
		}

		public void simulate(uint simulation)
		{
			if (Provider.isServer)
			{
				if (Level.info.type == ELevelType.SURVIVAL)
				{
					UnityEngine.Profiling.Profiler.BeginSample("Starve");
					if (food > 0)
					{
						if (simulation - lastStarve > Provider.modeConfigData.Players.Food_Use_Ticks * (1f + (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SURVIVAL) * 0.25f)) * (player.movement.inSnow ? 0.5f + (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.WARMBLOODED) * 0.5f) : 1f))
						{
							lastStarve = simulation;

							askStarve(1);
						}
					}
					else
					{
						if (simulation - lastStarve > Provider.modeConfigData.Players.Food_Damage_Ticks)
						{
							lastStarve = simulation;

							EPlayerKill kill;
							askDamage(1, Vector3.up, EDeathCause.FOOD, ELimb.SPINE, Provider.server, out kill);
						}
					}
					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("Dehydrate");
					if (water > 0)
					{
						if (simulation - lastDehydrate > Provider.modeConfigData.Players.Water_Use_Ticks * (1f + (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SURVIVAL) * 0.25f)))
						{
							lastDehydrate = simulation;

							askDehydrate(1);
						}
					}
					else
					{
						if (simulation - lastDehydrate > Provider.modeConfigData.Players.Water_Damage_Ticks)
						{
							lastDehydrate = simulation;

							EPlayerKill kill;
							askDamage(1, Vector3.up, EDeathCause.WATER, ELimb.SPINE, Provider.server, out kill);
						}
					}
					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("Infect");
					if (virus == 0)
					{
						if (simulation - lastInfect > Provider.modeConfigData.Players.Virus_Damage_Ticks)
						{
							lastInfect = simulation;

							EPlayerKill kill;
							askDamage(1, Vector3.up, EDeathCause.INFECTION, ELimb.SPINE, Provider.server, out kill);
						}
					}
					else if (virus < Provider.modeConfigData.Players.Virus_Infect)
					{
						if (simulation - lastUncleaned > Provider.modeConfigData.Players.Virus_Use_Ticks)
						{
							lastUncleaned = simulation;

							askInfect(1);
						}
					}
					UnityEngine.Profiling.Profiler.EndSample();
				}

				UnityEngine.Profiling.Profiler.BeginSample("Bleed/Regen");
				if (isBleeding)
				{
					if (simulation - lastBleed > Provider.modeConfigData.Players.Bleed_Damage_Ticks)
					{
						lastBleed = simulation;

						EPlayerKill kill;
						askDamage(1, Vector3.up, EDeathCause.BLEEDING, ELimb.SPINE, Provider.server, out kill);
					}
				}
				else
				{
					if (health < 100 && food > Provider.modeConfigData.Players.Health_Regen_Min_Food && water > Provider.modeConfigData.Players.Health_Regen_Min_Water && simulation - lastRegenerate > Provider.modeConfigData.Players.Health_Regen_Ticks * (1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.VITALITY) * 0.5f)))
					{
						lastRegenerate = simulation;

						askHeal(1, false, false);
					}
				}
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Recover");
				if (Provider.modeConfigData.Players.Can_Stop_Bleeding)
				{
					if (isBleeding && simulation - lastBleeding > Provider.modeConfigData.Players.Bleed_Regen_Ticks * (1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.STRENGTH) * 0.5f)))
					{
						serverSetBleeding(false);
					}
				}

				if (Provider.modeConfigData.Players.Can_Fix_Legs)
				{
					if (isBroken && simulation - lastBroken > Provider.modeConfigData.Players.Leg_Regen_Ticks * (1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.STRENGTH) * 0.5f)))
					{
						serverSetLegsBroken(false);
					}
				}
				UnityEngine.Profiling.Profiler.EndSample();
			}

			if (channel.IsLocalPlayer)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Vision");
				if (vision > 0)
				{
					if (simulation - lastView > 12)
					{
						lastView = simulation;

						askBlind(1);
					}
				}
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Econ Heartbeat");
				if (IsAlive)
				{
					Provider.provider.economyService.updateInventory();
				}
				UnityEngine.Profiling.Profiler.EndSample();
			}

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				SimulateStaminaFrame(simulation);

				UnityEngine.Profiling.Profiler.BeginSample("Swim");
				SimulateOxygenFrame(simulation);
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Radiate");
				if (player.movement.isRadiated)
				{
					bool hasRadiationProtection = player.clothing.maskAsset != null && player.clothing.maskAsset.proofRadiation && player.clothing.maskQuality > 0;
					if (player.movement.ActiveDeadzone.DeadzoneType == EDeadzoneType.FullSuitRadiation)
					{
						hasRadiationProtection &= player.clothing.shirtAsset != null && player.clothing.shirtAsset.proofRadiation;
						hasRadiationProtection &= player.clothing.pantsAsset != null && player.clothing.pantsAsset.proofRadiation;
					}

					if (hasRadiationProtection)
					{
						if (simulation - lastOutsideDeadzoneFrame > 2)
						{
							pendingDeadzoneDamage += player.movement.ActiveDeadzone.ProtectedDamagePerSecond * PlayerInput.RATE;

							// Nelson 2024-06-10: Previously, one quality was removed every 31st simulation frame.
							// i.e., if (frame - lastTime > 30) {  lastTime = frame; quality--; }
							// At the moment, each frame is 0.08 seconds (12.5 TPS), so with 1 damage every 2.48
							// seconds the quality loss per second is ~0.4. (Using this as the equivalent value).
							float qualityLossRate = player.movement.ActiveDeadzone.MaskFilterDamagePerSecond;
							qualityLossRate *= player.clothing.maskAsset.FilterDegradationRateMultiplier;
							pendingDeadzoneMaskFilterQualityLoss += qualityLossRate * PlayerInput.RATE;
							int flooredPendingQualityLoss = Mathf.FloorToInt(pendingDeadzoneMaskFilterQualityLoss);
							if (flooredPendingQualityLoss > 0)
							{
								pendingDeadzoneMaskFilterQualityLoss -= flooredPendingQualityLoss;
								lastRadiate = simulation;
								player.clothing.maskQuality--;
								player.clothing.updateMaskQuality();
							}
						}
					}
					else
					{
						if (simulation - lastOutsideDeadzoneFrame > 2)
						{
							pendingDeadzoneDamage += player.movement.ActiveDeadzone.UnprotectedDamagePerSecond * PlayerInput.RATE;
						}

						if (virus > 0)
						{
							// Nelson 2024-06-10: Previously, one radiation was added every 2nd simulation frame.
							// i.e., if (frame - lastTime > 1) {  lastTime = frame; askRadiate(1); }
							// At the moment, each frame is 0.08 seconds (12.5 TPS), so with 1 radiation every 0.16
							// seconds the radiation per second is 6.25. (Using this as the equivalent value).
							if (simulation - lastRadiate > 1)
							{
								lastRadiate = simulation;
							}

							if (simulation - lastOutsideDeadzoneFrame > 2)
							{
								pendingDeadzoneRadiation += player.movement.ActiveDeadzone.UnprotectedRadiationPerSecond * PlayerInput.RATE;
							}

							int flooredPendingDeadzoneRadiation = Mathf.FloorToInt(pendingDeadzoneRadiation);
							if (flooredPendingDeadzoneRadiation > 0)
							{
								pendingDeadzoneRadiation -= flooredPendingDeadzoneRadiation;
								askRadiate(MathfEx.ClampToByte(flooredPendingDeadzoneRadiation));
							}
						}
						else
						{
							if (Provider.isServer)
							{
								if (simulation - lastRadiate > 10)
								{
									lastRadiate = simulation;

									EPlayerKill kill;
									askDamage(10, Vector3.up, EDeathCause.INFECTION, ELimb.SPINE, Provider.server, out kill);
								}
							}
						}
					}

					if (!isDead && Provider.isServer)
					{
						int flooredPendingDeadzoneDamage = Mathf.FloorToInt(pendingDeadzoneDamage);
						if (flooredPendingDeadzoneDamage > 0)
						{
							pendingDeadzoneDamage -= flooredPendingDeadzoneDamage;
							EPlayerKill kill;
							askDamage(MathfEx.ClampToByte(flooredPendingDeadzoneDamage), Vector3.up, EDeathCause.INFECTION, ELimb.SPINE, Provider.server, out kill);
						}
					}
				}
				else
				{
					lastRadiate = simulation;
					lastOutsideDeadzoneFrame = simulation;
					pendingDeadzoneDamage = 0.0f;
					pendingDeadzoneRadiation = 0.0f;
					pendingDeadzoneMaskFilterQualityLoss = 0.0f;
				}
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Temperature");
				if (warmth > 0)
				{
					simulatedModifyWarmth(-1);
				}

				bool proofFire = false;
				if (player.clothing.shirtAsset != null && player.clothing.shirtAsset.proofFire && player.clothing.pantsAsset != null && player.clothing.pantsAsset.proofFire)
				{
					proofFire = true;
				}

				EPlayerTemperature newTemperature = temperature;
				EPlayerTemperature areaTemperature = TemperatureManager.checkPointTemperature(transform.position, proofFire);

				if (areaTemperature == EPlayerTemperature.ACID)
				{
					newTemperature = EPlayerTemperature.ACID;

					if (Provider.isServer)
					{
						if (simulation - lastBurn > 10)
						{
							lastBurn = simulation;

							EPlayerKill kill;
							askDamage(10, Vector3.up, EDeathCause.SPIT, ELimb.SPINE, Provider.server, out kill);
						}
					}
				}
				else if (areaTemperature == EPlayerTemperature.BURNING)
				{
					newTemperature = EPlayerTemperature.BURNING;

					if (Provider.isServer)
					{
						if (simulation - lastBurn > 10)
						{
							lastBurn = simulation;

							EPlayerKill kill;
							askDamage(10, Vector3.up, EDeathCause.BURNING, ELimb.SPINE, Provider.server, out kill);
						}
					}

					lastWarm = simulation;
					wasWarm = true;
				}
				else if (areaTemperature == EPlayerTemperature.WARM || warmth > 0)// || (player.movement.getVehicle() != null && player.movement.getVehicle().fuel > 0))
				{
					newTemperature = EPlayerTemperature.WARM;

					lastWarm = simulation;
					wasWarm = true;
				}
				else if (player.movement.inSnow && Level.info != null && Level.info.configData.Snow_Affects_Temperature)
				{
					if (player.stance.stance == EPlayerStance.SWIM)
					{
						newTemperature = EPlayerTemperature.FREEZING;

						if (Provider.isServer)
						{
							if (simulation - lastFreeze > 25)
							{
								lastFreeze = simulation;

								byte damage = 8;

								if (player.clothing.shirtAsset != null || player.clothing.vestAsset != null)
								{
									damage -= 2;
								}

								if (player.clothing.pantsAsset != null)
								{
									damage -= 2;
								}

								if (player.clothing.hatAsset != null)
								{
									damage -= 2;
								}

								EPlayerKill kill;
								askDamage(damage, Vector3.up, EDeathCause.FREEZING, ELimb.SPINE, Provider.server, out kill);
							}
						}
					}
					else
					{
						if (!wasWarm || simulation - lastWarm > 250 * (1f + player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.WARMBLOODED)))
						{
							bool isCovered = (player.movement.getVehicle() != null && !player.movement.getVehicle().asset.hasZip && !player.movement.getVehicle().asset.hasBicycle) || Physics.Raycast(transform.position + Vector3.up, Quaternion.Euler(45, LevelLighting.wind, 0) * -Vector3.forward, 32f, RayMasks.BLOCK_WIND);

							if (isCovered)
							{
								newTemperature = EPlayerTemperature.COVERED;

								lastCovered = simulation;
								wasCovered = true;
							}
							else
							{
								byte trapped = 1;

								if (player.clothing.shirtAsset != null || player.clothing.vestAsset != null)
								{
									trapped += 1;
								}

								if (player.clothing.pantsAsset != null)
								{
									trapped += 1;
								}

								if (player.clothing.hatAsset != null)
								{
									trapped += 1;
								}

								if (!wasCovered || simulation - lastCovered > 50 * trapped)
								{
									newTemperature = EPlayerTemperature.FREEZING;

									if (Provider.isServer)
									{
										if (simulation - lastFreeze > 75)
										{
											lastFreeze = simulation;

											byte damage = 4;

											if (player.clothing.shirtAsset != null || player.clothing.vestAsset != null)
											{
												damage -= 1;
											}

											if (player.clothing.pantsAsset != null)
											{
												damage -= 1;
											}

											if (player.clothing.hatAsset != null)
											{
												damage -= 1;
											}

											EPlayerKill kill;
											askDamage(damage, Vector3.up, EDeathCause.FREEZING, ELimb.SPINE, Provider.server, out kill);
										}
									}
								}
								else
								{
									newTemperature = EPlayerTemperature.COLD;
								}
							}
						}
						else
						{
							newTemperature = EPlayerTemperature.COLD;

							lastCovered = simulation;
							wasCovered = true;
						}
					}
				}
				else
				{
					newTemperature = EPlayerTemperature.NONE;
				}

				if (newTemperature != temperature)
				{
					_temperature = newTemperature;

					onTemperatureUpdated?.Invoke(temperature);
				}
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		public void breakLegs()
		{
			if (!isBroken)
			{
				EffectAsset bones = BonesRef.Find();
				if (bones != null)
				{
					TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(bones);
					triggerEffectParameters.relevantDistance = EffectManager.SMALL;
					triggerEffectParameters.position = transform.position;
					triggerEffectParameters.reliable = true;
					EffectManager.triggerEffect(triggerEffectParameters);
				}
			}

			serverSetLegsBroken(true);
		}

		private static readonly AssetReference<EffectAsset> BonesRef = new AssetReference<EffectAsset>("663158e0a71346068947b29978818ef7"); // Breaking bone sound (31)

		public delegate void FallDamageRequestHandler(PlayerLife component, float velocity, ref float damage, ref bool shouldBreakLegs);
		public event FallDamageRequestHandler OnFallDamageRequested;

		private void onLanded(float velocity)
		{
			LevelAsset levelAsset = Level.getAsset();
			float threshold = levelAsset != null && levelAsset.fallDamageSpeedThreshold > 0.01f ? levelAsset.fallDamageSpeedThreshold : 22.0f;

			// ~7.1m/s is typical for a jump, ~17.5m/s falling off the farm roof.
			if (velocity < -threshold && player.movement.totalGravityMultiplier > 0.67f)
			{
				Transform objectTransform = player.movement.ground.transform;
				ObjectAsset groundObjectAsset = objectTransform != null ? LevelObjects.getAsset(objectTransform) : null;
				bool causesFallDamage = groundObjectAsset == null || groundObjectAsset.causesFallDamage;
				if (!causesFallDamage)
					return;

				if (objectTransform != null)
				{
					FallDamageOverride overrideComponent = objectTransform.gameObject.GetComponentInParent<FallDamageOverride>();
					if (overrideComponent != null)
					{
						switch (overrideComponent.Mode)
						{
							case FallDamageOverride.EMode.None:
								break;

							case FallDamageOverride.EMode.PreventFallDamage:
								return; // Cancel damage.

							default:
								UnturnedLog.warn("Unknown fall damage override: {0}", overrideComponent.GetSceneHierarchyPath());
								break;
						}
					}
				}

				float baseDamage = Mathf.Min(101.0f, Mathf.Abs(velocity));
				float skillMultiplier = 1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.STRENGTH) * 0.75f);
				float damage = baseDamage * skillMultiplier;
				damage *= player.clothing.fallingDamageMultiplier;
				if (!Provider.modeConfigData.Players.Can_Hurt_Legs)
				{
					damage = 0.0f;
				}
				bool shouldBreakLegs = Provider.modeConfigData.Players.Can_Break_Legs;
				shouldBreakLegs &= !player.clothing.preventsFallingBrokenBones;

				if (OnFallDamageRequested != null)
				{
					try
					{
						OnFallDamageRequested.Invoke(this, velocity, ref damage, ref shouldBreakLegs);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, "Caught exception during OnFallDamageRequested:");
					}
				}

				byte roundedDamage = MathfEx.RoundAndClampToByte(damage);
				if (roundedDamage > 0)
				{
					EPlayerKill kill;
					askDamage(roundedDamage, Vector3.down, EDeathCause.BONES, ELimb.SPINE, Provider.server, out kill);
				}

				if (shouldBreakLegs)
				{
					breakLegs();
				}
			}
		}

		internal void InitializePlayer()
		{
			if (Provider.isServer)
			{
				player.movement.onLanded += onLanded;

				load();
			}
		}

		private bool wasLoadCalled;

		public void load()
		{
			wasLoadCalled = true;
			_isDead = false;

			if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Life.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				Block block = PlayerSavedata.readBlock(channel.owner.playerID, "/Player/Life.dat", 0);
				byte version = block.readByte();

				if (version > 1)
				{
					_health = block.readByte();
					_food = block.readByte();
					_water = block.readByte();
					_virus = block.readByte();
					_stamina = 100;

					if (version < SAVEDATA_VERSION_WITH_OXYGEN)
						_oxygen = 100;
					else
						_oxygen = block.readByte();

					_isBleeding = block.readBoolean();
					_isBroken = block.readBoolean();

					_temperature = EPlayerTemperature.NONE;
					wasWarm = false;
					wasCovered = false;

					return;
				}
			}

			_health = (byte) Provider.modeConfigData.Players.Health_Default;
			_food = (byte) Provider.modeConfigData.Players.Food_Default;
			_water = (byte) Provider.modeConfigData.Players.Water_Default;
			_virus = (byte) Provider.modeConfigData.Players.Virus_Default;
			_stamina = 100;
			_oxygen = 100;
			_isBleeding = false;
			_isBroken = false;

			_temperature = EPlayerTemperature.NONE;
			wasWarm = false;
			wasCovered = false;

			recentKiller = CSteamID.Nil;
			lastTimeAggressive = -100;
			lastTimeTookDamage = -100;
			lastTimeCausedDamage = -100;
		}

		public void save()
		{
			if (!wasLoadCalled)
				return;

			if (player.life.isDead)
			{
				if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Life.dat"))
				{
					PlayerSavedata.deleteFile(channel.owner.playerID, "/Player/Life.dat");
				}
			}
			else
			{
				Block block = new Block();
				block.writeByte(SAVEDATA_VERSION_LATEST);

				block.writeByte(health);
				block.writeByte(food);
				block.writeByte(water);
				block.writeByte(virus);
				block.writeByte(oxygen);
				block.writeBoolean(isBleeding);
				block.writeBoolean(isBroken);

				PlayerSavedata.writeBlock(channel.owner.playerID, "/Player/Life.dat", block);
			}
		}
	}
}
