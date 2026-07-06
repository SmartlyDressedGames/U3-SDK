////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void ApplyingDefaultSkillsHandler(Player forPlayer, Skill[][] newSkills);
	public delegate void ExperienceUpdated(uint newExperience);
	public delegate void ReputationUpdated(int newReputation);
	public delegate void BoostUpdated(EPlayerBoost newBoost);
	public delegate void SkillsUpdated();

	public class SpecialitySkillPair
	{
		public int speciality
		{
			get;
			private set;
		}

		public int skill
		{
			get;
			private set;
		}

		public SpecialitySkillPair(int newSpeciality, int newSkill)
		{
			speciality = newSpeciality;
			skill = newSkill;
		}
	}

	public class PlayerSkills : PlayerCaller
	{
		public static readonly SpecialitySkillPair[][] SKILLSETS = new SpecialitySkillPair[][]
		{
			new SpecialitySkillPair[] {}, // none
			new SpecialitySkillPair[] // fire
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.CARDIO),
				new SpecialitySkillPair((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.STRENGTH)
			},
			new SpecialitySkillPair[] // police
			{
				 new SpecialitySkillPair((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.EXERCISE),
				new SpecialitySkillPair((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.TOUGHNESS)
			},
			new SpecialitySkillPair[] // army
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.SHARPSHOOTER),
				new SpecialitySkillPair((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.DEXTERITY)
			},
			new SpecialitySkillPair[] // farm
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.AGRICULTURE),
				new SpecialitySkillPair((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SURVIVAL)
			},
			new SpecialitySkillPair[] // fishing
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.FISHING),
				new SpecialitySkillPair((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.DIVING)
			},
			new SpecialitySkillPair[] // camp
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.WARMBLOODED),
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.OUTDOORS)
			},
			new SpecialitySkillPair[] // worker
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.CRAFTING),
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.ENGINEER),
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.MECHANIC)
			},
			new SpecialitySkillPair[] // chef
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.COOKING),
				new SpecialitySkillPair((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.VITALITY)
			},
			new SpecialitySkillPair[] // thief
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.OFFENSE, (int) EPlayerOffense.PARKOUR),
				new SpecialitySkillPair((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.SNEAKYBEAKY)
			},
			new SpecialitySkillPair[] // doctor
			{
				new SpecialitySkillPair((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.IMMUNITY),
				new SpecialitySkillPair((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.HEALING)
			}
		};

		public static readonly byte SAVEDATA_VERSION = 7;

		public static readonly byte SPECIALITIES = 3;

		public static readonly byte BOOST_COUNT = 4;
		public static readonly uint BOOST_COST = 25;

		public static event ApplyingDefaultSkillsHandler onApplyingDefaultSkills;

		/// <summary>
		/// Invoked after any player's experience value changes (not including loading).
		/// </summary>
		public static event System.Action<PlayerSkills, uint> OnExperienceChanged_Global;
		/// <summary>
		/// Invoked after any player's reputation value changes (not including loading).
		/// </summary>
		public static event System.Action<PlayerSkills, int> OnReputationChanged_Global;

		public ExperienceUpdated onExperienceUpdated;
		public ReputationUpdated onReputationUpdated;
		public BoostUpdated onBoostUpdated;
		public SkillsUpdated onSkillsUpdated;

		//		public EPlayerSpeciality speciality
		//		{
		//			get
		//			{
		//				return EPlayerSpeciality.OFFENSE;//channel.owner.speciality;
		//			}
		//		}

		private Skill[][] _skills;
		public Skill[][] skills => _skills;

		private EPlayerBoost _boost;
		public EPlayerBoost boost => _boost;

		private uint _experience;
		public uint experience => _experience;

		private int _reputation;
		public int reputation => _reputation;

		private bool wasLoaded;

		public bool doesLevelAllowSkills
		{
			get
			{
				if (Level.info != null && Level.info.configData != null)
				{
					return Level.info.configData.Allow_Skills;
				}
				else
				{
					return true;
				}
			}
		}

		public float GetSharpshooterRecoilMultiplierForLevel(int level)
		{
			Skill skill = skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.SHARPSHOOTER];
			return 1.0f - skill.NormalizeLevel(level) * 0.4f;
		}

		public float GetSharpshooterRecoilMultiplier()
		{
			Skill skill = skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.SHARPSHOOTER];
			return GetSharpshooterRecoilMultiplierForLevel(skill.level);
		}

		/// <summary>
		/// Hack to parse both the speciality enum and per-speciality skill enum given the name.
		/// </summary>
		public static bool TryParseIndices(string input, out int specialityIndex, out int skillIndex)
		{
			EPlayerOffense offense;
			if (System.Enum.TryParse(input, true, out offense))
			{
				specialityIndex = (int) EPlayerSpeciality.OFFENSE;
				skillIndex = (int) offense;
				return true;
			}

			EPlayerDefense defense;
			if (System.Enum.TryParse(input, true, out defense))
			{
				specialityIndex = (int) EPlayerSpeciality.DEFENSE;
				skillIndex = (int) defense;
				return true;
			}

			EPlayerSupport support;
			if (System.Enum.TryParse(input, true, out support))
			{
				specialityIndex = (int) EPlayerSpeciality.SUPPORT;
				skillIndex = (int) support;
				return true;
			}

			specialityIndex = -1;
			skillIndex = -1;
			return false;
		}

		[System.Obsolete]
		public void tellExperience(CSteamID steamID, uint newExperience)
		{
			ReceiveExperience(newExperience);
		}

		private static readonly ClientInstanceMethod<uint> SendExperience = ClientInstanceMethod<uint>.Get(typeof(PlayerSkills), nameof(ReceiveExperience));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellExperience))]
		public void ReceiveExperience(uint newExperience)
		{
			uint oldExperience = _experience;

			if (channel.IsLocalPlayer && newExperience > experience && Level.info.type != ELevelType.HORDE)
			{
				if (wasLoaded)
				{
					int data;
					if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Experience", out data))
					{
						Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Experience", data + (int) (newExperience - experience));
					}

					PlayerUI.message(EPlayerMessage.EXPERIENCE, (newExperience - experience).ToString());
				}
			}

			_experience = newExperience;

			onExperienceUpdated?.Invoke(experience);

			// Invoked here in case plugins are manually calling tellExperience.
			OnExperienceChanged_Global?.Invoke(this, oldExperience);
		}

		[System.Obsolete]
		public void tellReputation(CSteamID steamID, int newReputation)
		{
			ReceiveReputation(newReputation);
		}

		private static readonly ClientInstanceMethod<int> SendReputation = ClientInstanceMethod<int>.Get(typeof(PlayerSkills), nameof(ReceiveReputation));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellReputation))]
		public void ReceiveReputation(int newReputation)
		{
			int oldReputation = _reputation;

			if (channel.IsLocalPlayer && newReputation != reputation && Level.info.type != ELevelType.HORDE)
			{
				if (wasLoaded)
				{
					if (newReputation <= -200)
					{
						bool data;
						if (Provider.provider.achievementsService.getAchievement("Villain", out data) && !data)
						{
							Provider.provider.achievementsService.setAchievement("Villain");
						}
					}
					else if (newReputation >= 200)
					{
						bool data;
						if (Provider.provider.achievementsService.getAchievement("Paragon", out data) && !data)
						{
							Provider.provider.achievementsService.setAchievement("Paragon");
						}
					}

					if (player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowReputationChangeNotification))
					{
						string text = (newReputation - reputation).ToString();
						if (newReputation > reputation)
						{
							text = '+' + text;
						}

						PlayerUI.message(EPlayerMessage.REPUTATION, text);
					}
				}
			}

			_reputation = newReputation;

			onReputationUpdated?.Invoke(reputation);

			// Invoked here in case plugins are manually calling tellReputation.
			OnReputationChanged_Global?.Invoke(this, oldReputation);
		}

		[System.Obsolete]
		public void tellBoost(CSteamID steamID, byte newBoost)
		{
			ReceiveBoost((EPlayerBoost) newBoost);
		}

		private static readonly ClientInstanceMethod<EPlayerBoost> SendBoost = ClientInstanceMethod<EPlayerBoost>.Get(typeof(PlayerSkills), nameof(ReceiveBoost));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellBoost))]
		public void ReceiveBoost(EPlayerBoost newBoost)
		{
			_boost = newBoost;

			onBoostUpdated?.Invoke(boost);

			wasLoaded = true;
		}

		[System.Obsolete]
		public void tellSkill(CSteamID steamID, byte speciality, byte index, byte level)
		{
			ReceiveSingleSkillLevel(speciality, index, level);
		}

		private static readonly ClientInstanceMethod<byte, byte, byte> SendSingleSkillLevel = ClientInstanceMethod<byte, byte, byte>.Get(typeof(PlayerSkills), nameof(ReceiveSingleSkillLevel));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSkill))]
		public void ReceiveSingleSkillLevel(byte speciality, byte index, byte level)
		{
			if (index >= skills[speciality].Length)
			{
				return;
			}

			skills[speciality][index].level = level;

			if (channel.IsLocalPlayer)
			{
				bool offense = true;
				bool defense = true;
				bool support = true;

				for (int check = 0; check < skills[(int) EPlayerSpeciality.OFFENSE].Length; check++)
				{
					if (skills[(int) EPlayerSpeciality.OFFENSE][check].level < skills[(int) EPlayerSpeciality.OFFENSE][check].max)
					{
						offense = false;
						break;
					}
				}

				for (int check = 0; check < skills[(int) EPlayerSpeciality.DEFENSE].Length; check++)
				{
					if (skills[(int) EPlayerSpeciality.DEFENSE][check].level < skills[(int) EPlayerSpeciality.DEFENSE][check].max)
					{
						defense = false;
						break;
					}
				}

				for (int check = 0; check < skills[(int) EPlayerSpeciality.SUPPORT].Length; check++)
				{
					if (skills[(int) EPlayerSpeciality.SUPPORT][check].level < skills[(int) EPlayerSpeciality.SUPPORT][check].max)
					{
						support = false;
						break;
					}
				}

				if (offense)
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement("Offense", out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement("Offense");
					}
				}

				if (defense)
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement("Defense", out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement("Defense");
					}
				}

				if (support)
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement("Support", out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement("Support");
					}
				}

				if (offense && defense && support)
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement("Mastermind", out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement("Mastermind");
					}
				}
			}

			Profiler.BeginSample("Invoke onSkillsUpdated");
			onSkillsUpdated?.Invoke();
			Profiler.EndSample();
		}

		public float mastery(int speciality, int index)
		{
			return skills[speciality][index].mastery;
		}

		public uint cost(int speciality, int index)
		{
			uint finalCost = skills[speciality][index].cost;

			if (Provider.modeConfigData?.Players?.Skillset_Reduces_Skill_Cost ?? true)
			{
				if (Level.info != null && Level.info.type != ELevelType.ARENA)
				{
					for (byte search = 0; search < SKILLSETS[(byte) channel.owner.skillset].Length; search++)
					{
						SpecialitySkillPair pair = SKILLSETS[(byte) channel.owner.skillset][search];

						if (speciality == pair.speciality && index == pair.skill)
						{
							finalCost /= 2;
						}
					}
				}
			}

			float multiplier = Provider.modeConfigData?.Players?.Skill_Cost_Multiplier ?? 1;
			if (multiplier != 1)
			{
				finalCost = MathfEx.RoundAndClampToUInt(finalCost * multiplier);
			}

			return finalCost;
		}

		public void askSpend(uint cost)
		{
			if (channel.IsLocalPlayer)
			{
				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience - cost);
			}
			else
			{
				uint oldExperience = _experience;
				_experience -= cost;

				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience);

				OnExperienceChanged_Global?.Invoke(this, oldExperience);
			}
		}

		public void askAward(uint award)
		{
			if (channel.IsLocalPlayer)
			{
				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience + award);
			}
			else
			{
				uint oldExperience = _experience;
				_experience += award;

				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience);

				OnExperienceChanged_Global?.Invoke(this, oldExperience);
			}
		}

		public void ServerSetExperience(uint newExperience)
		{
			if (newExperience > _experience)
			{
				askAward(newExperience - _experience);
			}
			else if (newExperience < _experience)
			{
				askSpend(_experience - newExperience);
			}
		}

		public void ServerModifyExperience(int delta)
		{
			if (delta > 0)
			{
				askAward((uint) delta);
			}
			else if (delta < 0)
			{
				uint adjustedDelta = (uint) -delta;
				adjustedDelta = MathfEx.Min(adjustedDelta, _experience);
				askSpend(adjustedDelta);
			}
		}

		public void askRep(int rep)
		{
			SendReputation.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), reputation + rep);
			// Do not invoke OnReputationChanged here because tellReputation does.
		}

		public void askPay(uint pay)
		{
			if (pay == 0)
				return;

			pay = (uint) (pay * Provider.modeConfigData.Players.Experience_Multiplier);

			if (channel.IsLocalPlayer)
			{
				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience + pay);
			}
			else
			{
				uint oldExperience = _experience;
				_experience += pay;

				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience);

				OnExperienceChanged_Global?.Invoke(this, oldExperience);
			}
		}

		public void modRep(int rep)
		{
			int oldReputation = _reputation;

			_reputation += rep;

			onReputationUpdated?.Invoke(reputation);

			OnReputationChanged_Global?.Invoke(this, oldReputation);
		}

		public void modXp(uint xp)
		{
			uint oldExperience = _experience;
			_experience += xp;

			onExperienceUpdated?.Invoke(experience);

			OnExperienceChanged_Global?.Invoke(this, oldExperience);
		}

		public void modXp2(uint xp)
		{
			uint oldExperience = _experience;
			_experience -= xp;

			onExperienceUpdated?.Invoke(experience);

			OnExperienceChanged_Global?.Invoke(this, oldExperience);
		}

		public static event System.Action<PlayerSkills, byte, byte, byte> OnSkillUpgraded_Global;

		[System.Obsolete]
		public void askUpgrade(CSteamID steamID, byte speciality, byte index, bool force)
		{
			ReceiveUpgradeRequest(speciality, index, force);
		}

		private static readonly ServerInstanceMethod<byte, byte, bool> SendUpgradeRequest = ServerInstanceMethod<byte, byte, bool>.Get(typeof(PlayerSkills), nameof(ReceiveUpgradeRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = nameof(askUpgrade))]
		public void ReceiveUpgradeRequest(byte speciality, byte index, bool force)
		{
			if (doesLevelAllowSkills == false)
				return;

			if (speciality >= SPECIALITIES)
			{
				return;
			}

			if (index >= skills[speciality].Length)
			{
				return;
			}

			Skill skill = skills[speciality][index];
			byte oldLevel = skill.level;
			uint oldExperience = _experience;

			while (true)
			{
				if (experience >= cost(speciality, index) && skill.level < skill.GetClampedMaxUnlockableLevel())
				{
					_experience -= cost(speciality, index);
					skill.level++;
				}
				else
				{
					break;
				}

				if (!force)
				{
					break;
				}
			}

			if (skill.level > oldLevel)
			{
				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience);
				SendSingleSkillLevel.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), speciality, index, skill.level);

				if (_experience != oldExperience && OnExperienceChanged_Global != null)
				{
					OnExperienceChanged_Global.Invoke(this, oldExperience);
				}

				OnSkillUpgraded_Global.TryInvoke("OnSkillUpgraded_Global", this, speciality, index, oldLevel);
			}
		}

		public bool ServerSetSkillLevel(int specialityIndex, int skillIndex, int newLevel)
		{
			if (specialityIndex >= skills.Length)
				throw new System.ArgumentOutOfRangeException(nameof(specialityIndex));

			if (skillIndex >= skills[specialityIndex].Length)
				throw new System.ArgumentOutOfRangeException(nameof(skillIndex));

			Skill skill = skills[specialityIndex][skillIndex];
			if (newLevel > skill.max)
				throw new System.ArgumentOutOfRangeException(nameof(newLevel));

			if (skill.level != newLevel)
			{
				skill.level = (byte) newLevel;
				SendSingleSkillLevel.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), (byte) specialityIndex, (byte) skillIndex, skill.level);
				return true;
			}
			else
			{
				return false;
			}
		}

		[System.Obsolete]
		public void askBoost(CSteamID steamID)
		{
			ReceiveBoostRequest();
		}

		private static readonly ServerInstanceMethod SendBoostRequest = ServerInstanceMethod.Get(typeof(PlayerSkills), nameof(ReceiveBoostRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = nameof(askBoost))]
		public void ReceiveBoostRequest()
		{
			if (doesLevelAllowSkills == false)
				return;

			if (experience >= BOOST_COST)
			{
				uint oldExperience = _experience;
				_experience -= BOOST_COST;

				byte newBoost;
				do
				{
					newBoost = (byte) Random.Range(1, BOOST_COUNT + 1);
				}
				while (newBoost == (byte) boost);
				_boost = (EPlayerBoost) newBoost;

				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience);
				SendBoost.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), boost);

				OnExperienceChanged_Global?.Invoke(this, oldExperience);
			}
		}

		[System.Obsolete]
		public void askPurchase(CSteamID steamID, byte index)
		{
		}

		private static readonly ServerInstanceMethod<NetId> SendPurchaseRequest = ServerInstanceMethod<NetId>.Get(typeof(PlayerSkills), nameof(ReceivePurchaseRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = nameof(askPurchase))]
		public void ReceivePurchaseRequest(NetId volumeNetId)
		{
			HordePurchaseVolume node = NetIdRegistry.Get<HordePurchaseVolume>(volumeNetId);
			if (node == null)
				return;

			if (experience >= node.cost)
			{
				uint oldExperience = _experience;
				_experience -= node.cost;

				SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), experience);

				ItemAsset purchaseAsset = Assets.find(EAssetType.ITEM, node.id) as ItemAsset;
				if (purchaseAsset.type == EItemType.GUN && player.inventory.HasItemByAsset(purchaseAsset))
				{
					player.inventory.tryAddItem(new Item(((ItemGunAsset) purchaseAsset).GetDefaultMagazineLegacyId(), EItemOrigin.ADMIN), true);
				}
				else
				{
					player.inventory.tryAddItem(new Item(node.id, EItemOrigin.ADMIN), true);
				}

				OnExperienceChanged_Global?.Invoke(this, oldExperience);
			}
		}

		public void sendUpgrade(byte speciality, byte index, bool force)
		{
			SendUpgradeRequest.Invoke(GetNetId(), ENetReliability.Unreliable, speciality, index, force);
		}

		public void sendBoost()
		{
			SendBoostRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
		}

		public void sendPurchase(HordePurchaseVolume node)
		{
			SendPurchaseRequest.Invoke(GetNetId(), ENetReliability.Unreliable, node.GetNetIdFromInstanceId());
		}

		[System.Obsolete]
		public void tellSkills(CSteamID steamID, byte speciality, byte[] newLevels)
		{ }

		private static readonly ClientInstanceMethod SendMultipleSkillLevels = ClientInstanceMethod.Get(typeof(PlayerSkills), nameof(ReceiveMultipleSkillLevels));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveMultipleSkillLevels(in ClientInvocationContext context)
		{
			if (skills == null) // Server plugin probably sent this packet too early.
				return;

			NetPakReader reader = context.reader;
			for (int specialityIndex = 0; specialityIndex < skills.Length; ++specialityIndex)
			{
				Skill[] specialitySkills = skills[specialityIndex];
				for (int skillIndex = 0; skillIndex < specialitySkills.Length; ++skillIndex)
				{
					reader.ReadUInt8(out specialitySkills[skillIndex].level);
				}
			}

			Profiler.BeginSample("Invoke onSkillsUpdate");
			onSkillsUpdated?.Invoke();
			Profiler.EndSample();
		}

		private void WriteSkillLevels(NetPakWriter writer)
		{
			for (int specialityIndex = 0; specialityIndex < skills.Length; ++specialityIndex)
			{
				Skill[] specialitySkills = skills[specialityIndex];
				for (int skillIndex = 0; skillIndex < specialitySkills.Length; ++skillIndex)
				{
					writer.WriteUInt8(specialitySkills[skillIndex].level);
				}
			}
		}

		[System.Obsolete]
		public void askSkills(CSteamID steamID)
		{ }

		internal void SendInitialPlayerState(SteamPlayer client)
		{
			SendMultipleSkillLevels.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, WriteSkillLevels);
			SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, experience);
			SendReputation.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, reputation);
			SendBoost.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, boost);
		}

		internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
		{
			SendMultipleSkillLevels.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, WriteSkillLevels);
			SendExperience.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, experience);
			SendReputation.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, reputation);
			SendBoost.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, boost);
		}

		/// <summary>
		/// Set every level to max and replicate.
		/// </summary>
		public void ServerUnlockAllSkills()
		{
			foreach (Skill[] specialitySkills in skills)
			{
				foreach (Skill skill in specialitySkills)
				{
					skill.setLevelToMax();
				}
			}

			SendMultipleSkillLevels.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), WriteSkillLevels);
		}

		private void onLifeUpdated(bool isDead)
		{
			if (isDead)
			{
				if (Provider.isServer)
				{
					if (Level.info == null || Level.info.type == ELevelType.SURVIVAL)
					{
						bool modifiedAnySkillLevels = false;

						float loseSkills = player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Skills_PvP : Provider.modeConfigData.Players.Lose_Skills_PvE;
						if (loseSkills < 0.999f)
						{
							for (int specialityIndex = 0; specialityIndex < skills.Length; specialityIndex++)
							{
								Skill[] specialitySkills = skills[specialityIndex];
								for (int skillIndex = 0; skillIndex < specialitySkills.Length; skillIndex++)
								{
									if (CanDecreaseLevelOfSkill(specialityIndex, skillIndex))
									{
										byte newLevel = (byte) (specialitySkills[skillIndex].level * loseSkills);
										modifiedAnySkillLevels |= (specialitySkills[skillIndex].level != newLevel);
										specialitySkills[skillIndex].level = newLevel;
									}
								}
							}
						}

						uint numberOfSkillLevelsToLose = player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Skill_Levels_PvP : Provider.modeConfigData.Players.Lose_Skill_Levels_PvE;
						if (numberOfSkillLevelsToLose > 0)
						{
							LoseNumberOfSkills(numberOfSkillLevelsToLose, ref modifiedAnySkillLevels);
						}

						if (modifiedAnySkillLevels)
						{
							SendMultipleSkillLevels.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), WriteSkillLevels);
						}

						float loseXp = player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Experience_PvP : Provider.modeConfigData.Players.Lose_Experience_PvE;
						_experience = (uint) (experience * loseXp);
					}
					else
					{
						for (byte speciality = 0; speciality < skills.Length; speciality++)
						{
							for (byte index = 0; index < skills[speciality].Length; index++)
							{
								skills[speciality][index].level = 0;
							}
						}

						applyDefaultSkills();

						SendMultipleSkillLevels.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), WriteSkillLevels);

						if (Level.info.type == ELevelType.ARENA)
						{
							_experience = 0;
						}
						else
						{
							_experience = (uint) (experience * 0.75f);
						}
					}

					_boost = EPlayerBoost.NONE;

					SendExperience.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), experience);
					SendBoost.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), boost);

					// Do not invoke OnExperienceChanged here because tellExperience is routed to all.
				}
			}
		}

		internal void InitializePlayer()
		{
			_skills = new Skill[SPECIALITIES][];

			//OFFENSE
			skills[(int) EPlayerSpeciality.OFFENSE] = new Skill[7]; // If changing remember to update LevelAsset.skillRules.
			skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.OVERKILL] = new Skill(0, 7, 10, 1.0f);
			skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.SHARPSHOOTER] = new Skill(0, 7, 10, 1.0f);
			skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.DEXTERITY] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.CARDIO] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.EXERCISE] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.DIVING] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.OFFENSE][(int) EPlayerOffense.PARKOUR] = new Skill(0, 5, 20, 0.5f);

			//DEFENSE
			skills[(int) EPlayerSpeciality.DEFENSE] = new Skill[7]; // If changing remember to update LevelAsset.skillRules.
			skills[(int) EPlayerSpeciality.DEFENSE][(int) EPlayerDefense.SNEAKYBEAKY] = new Skill(0, 7, 10, 1.0f);
			skills[(int) EPlayerSpeciality.DEFENSE][(int) EPlayerDefense.VITALITY] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.DEFENSE][(int) EPlayerDefense.IMMUNITY] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.DEFENSE][(int) EPlayerDefense.TOUGHNESS] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.DEFENSE][(int) EPlayerDefense.STRENGTH] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.DEFENSE][(int) EPlayerDefense.WARMBLOODED] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.DEFENSE][(int) EPlayerDefense.SURVIVAL] = new Skill(0, 5, 10, 0.5f);

			//SUPPORT
			skills[(int) EPlayerSpeciality.SUPPORT] = new Skill[8]; // If changing remember to update LevelAsset.skillRules.
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.HEALING] = new Skill(0, 7, 10, 1.0f);
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.CRAFTING] = new Skill(0, 3, 20, 1.5f);
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.OUTDOORS] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.COOKING] = new Skill(0, 3, 20, 1.5f);
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.FISHING] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.AGRICULTURE] = new Skill(0, 7, 10, 1.0f);
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.MECHANIC] = new Skill(0, 5, 10, 0.5f);
			skills[(int) EPlayerSpeciality.SUPPORT][(int) EPlayerSupport.ENGINEER] = new Skill(0, 3, 20, 1.5f);

			LevelAsset levelAsset = Level.getAsset();
			if (levelAsset != null && levelAsset.skillRules != null && !(Provider.modeConfigData?.Players?.Prevent_Level_Skill_Overrides ?? false))
			{
				for (int specialityIndex = 0; specialityIndex < skills.Length; ++specialityIndex)
				{
					for (int skillIndex = 0; skillIndex < skills[specialityIndex].Length; ++skillIndex)
					{
						LevelAsset.SkillRule skillRule = levelAsset.skillRules[specialityIndex][skillIndex];
						if (skillRule != null)
						{
							Skill skill = skills[specialityIndex][skillIndex];
							if (skillRule.maxUnlockableLevel > -1)
							{
								skill.maxUnlockableLevel = skillRule.maxUnlockableLevel;
							}
							skill.costMultiplier = skillRule.costMultiplier;
							if (skillRule.baseCostOverride > -1)
							{
								skill.baseCost = skillRule.baseCostOverride;
							}
							if (skillRule.perLevelCostIncreaseOverride > -1)
							{
								skill.perLevelCostIncrease = skillRule.perLevelCostIncreaseOverride;
							}
						}
					}
				}
			}

			if (Provider.isServer)
			{
				load();

				player.life.onLifeUpdated += onLifeUpdated;
			}
			else
			{
				_experience = uint.MaxValue;
				_reputation = 0;
			}
		}

		private bool wasLoadCalled;

		public void load()
		{
			wasLoadCalled = true;

			if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Skills.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				Block block = PlayerSavedata.readBlock(channel.owner.playerID, "/Player/Skills.dat", 0);
				byte version = block.readByte();

				if (version > 4)
				{
					_experience = block.readUInt32();

					if (version >= 7)
					{
						_reputation = block.readInt32();
					}
					else
					{
						_reputation = 0;
					}

					_boost = (EPlayerBoost) block.readByte();

					if (version >= 6)
					{
						for (byte special = 0; special < skills.Length; special++)
						{
							if (skills[special] != null)
							{
								for (byte index = 0; index < skills[special].Length; index++)
								{
									skills[special][index].level = block.readByte();

									if (skills[special][index].level > skills[special][index].max)
									{
										skills[special][index].level = skills[special][index].max;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				applyDefaultSkills();
			}
		}

		public void save()
		{
			if (!wasLoadCalled)
				return;

			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION);

			block.writeUInt32(experience);
			block.writeInt32(reputation);
			block.writeByte((byte) boost);

			for (byte special = 0; special < skills.Length; special++)
			{
				if (skills[special] != null)
				{
					for (byte index = 0; index < skills[special].Length; index++)
					{
						block.writeByte(skills[special][index].level);
					}
				}
			}

			PlayerSavedata.writeBlock(channel.owner.playerID, "/Player/Skills.dat", block);
		}

		/// <summary>
		/// Serverside only.
		/// Called when skills weren't loaded (no save, or in arena mode), as well as when reseting skills after death.
		/// </summary>
		private void applyDefaultSkills()
		{
			if (Provider.modeConfigData.Players.Spawn_With_Max_Skills)
			{
				for (byte speciality = 0; speciality < skills.Length; speciality++)
				{
					Skill[] specialitySkills = skills[speciality];
					for (byte skillIndex = 0; skillIndex < specialitySkills.Length; skillIndex++)
					{
						specialitySkills[skillIndex].setLevelToMax();
					}
				}
			}
			else // No point entering this branch if all skills were already maxed
			{
				LevelAsset levelAsset = Level.getAsset();
				if (levelAsset != null && levelAsset.skillRules != null && !(Provider.modeConfigData?.Players?.Prevent_Level_Skill_Overrides ?? false))
				{
					for (int specialityIndex = 0; specialityIndex < skills.Length; ++specialityIndex)
					{
						for (int skillIndex = 0; skillIndex < skills[specialityIndex].Length; ++skillIndex)
						{
							LevelAsset.SkillRule skillRule = levelAsset.skillRules[specialityIndex][skillIndex];
							if (skillRule != null)
							{
								skills[specialityIndex][skillIndex].level = (byte) skillRule.defaultLevel;
							}
						}
					}
				}

				if (Provider.modeConfigData.Players.Spawn_With_Stamina_Skills)
				{
					skills[(byte) EPlayerSpeciality.OFFENSE][(byte) EPlayerOffense.CARDIO].setLevelToMax();
					skills[(byte) EPlayerSpeciality.OFFENSE][(byte) EPlayerOffense.DIVING].setLevelToMax();
					skills[(byte) EPlayerSpeciality.OFFENSE][(byte) EPlayerOffense.EXERCISE].setLevelToMax();
					skills[(byte) EPlayerSpeciality.OFFENSE][(byte) EPlayerOffense.PARKOUR].setLevelToMax();
				}
			}

			onApplyingDefaultSkills?.Invoke(player, skills);
		}

		private bool CanDecreaseLevelOfSkill(int specialityIndex, int skillIndex)
		{
			if (Provider.modeConfigData?.Players?.Skillset_Prevents_Skill_Loss ?? true)
			{
				int ownerSkillsetIndex = (int) channel.owner.skillset;
				for (int searchIndex = 0; searchIndex < SKILLSETS[ownerSkillsetIndex].Length; searchIndex++)
				{
					SpecialitySkillPair pair = SKILLSETS[ownerSkillsetIndex][searchIndex];
					if (specialityIndex == pair.speciality && skillIndex == pair.skill)
					{
						return false;
					}
				}
			}

			return true;
		}

		private static List<System.Tuple<int, int>> availableSkillsToLoseLevels = new List<System.Tuple<int, int>>();
		private void LoseNumberOfSkills(uint numberOfSkillsToLose, ref bool modifiedAnySkillLevels)
		{
			availableSkillsToLoseLevels.Clear();

			for (int specialityIndex = 0; specialityIndex < skills.Length; ++specialityIndex)
			{
				Skill[] specialitySkills = skills[specialityIndex];
				for (int skillIndex = 0; skillIndex < specialitySkills.Length; ++skillIndex)
				{
					if (CanDecreaseLevelOfSkill(specialityIndex, skillIndex) && specialitySkills[skillIndex].level > 0)
					{
						availableSkillsToLoseLevels.Add(new System.Tuple<int, int>(specialityIndex, skillIndex));
					}
				}
			}

			while (numberOfSkillsToLose > 0 && availableSkillsToLoseLevels.Count > 0)
			{
				int removeIndex = availableSkillsToLoseLevels.GetRandomIndex();
				System.Tuple<int, int> coord = availableSkillsToLoseLevels[removeIndex];
				availableSkillsToLoseLevels.RemoveAtFast(removeIndex);

				Skill skill = skills[coord.Item1][coord.Item2];
				--skill.level;
				modifiedAnySkillLevels = true;

				--numberOfSkillsToLose;
			}
		}
	}
}
