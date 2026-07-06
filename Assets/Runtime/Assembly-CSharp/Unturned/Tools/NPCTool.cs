////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCTool
	{
		/// <summary>
		/// Was redirected to HolidayUtil but kept for plugin backwards compatibility.
		/// Refer to HolidayUtil for explanation of this weird situation.
		/// </summary>
		public static ENPCHoliday getActiveHoliday()
		{
			return Provider.authorityHoliday;
		}

		/// <summary>
		/// Was redirected to HolidayUtil but kept for plugin backwards compatibility.
		/// Refer to HolidayUtil for explanation of this weird situation.
		/// </summary>
		public static bool isHolidayActive(ENPCHoliday holiday)
		{
			return holiday == Provider.authorityHoliday;
		}

		public static bool doesLogicPass<T>(ENPCLogicType logicType, T a, T b) where T : IComparable
		{
			int result = a.CompareTo(b);

			switch (logicType)
			{
				case ENPCLogicType.LESS_THAN:
					return result < 0;
				case ENPCLogicType.LESS_THAN_OR_EQUAL_TO:
					return result <= 0;
				case ENPCLogicType.EQUAL:
					return result == 0;
				case ENPCLogicType.NOT_EQUAL:
					return result != 0;
				case ENPCLogicType.GREATER_THAN_OR_EQUAL_TO:
					return result >= 0;
				case ENPCLogicType.GREATER_THAN:
					return result > 0;
			}

			return false;
		}

		[System.Obsolete("NPCConditionsList.Parse should be used instead")]
		public static void readConditions(IDatDictionary data, Local localization, string prefix, INPCCondition[] conditions, Asset assetContext)
		{
			for (int conditionIndex = 0; conditionIndex < conditions.Length; ++conditionIndex)
			{
				string conditionPrefix = prefix + conditionIndex;

				string typeKey = conditionPrefix + "_Type";
				if (!data.ContainsKey(typeKey))
				{
					throw new System.NotSupportedException("Missing condition " + typeKey);
				}

				ENPCConditionType conditionType = data.ParseEnum<ENPCConditionType>(typeKey);
				if (conditionType == ENPCConditionType.NONE)
				{
					assetContext.ReportAssetError($"{typeKey} unknown type");
					continue;
				}

				Type underlyingType = conditionTypes[(int) conditionType];
				if (underlyingType == null)
				{
					assetContext.ReportAssetError($"{typeKey} unable to create type");
					return;
				}

				INPCCondition condition;
				try
				{
					condition = Activator.CreateInstance(underlyingType) as INPCCondition;
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception instantiating {underlyingType}:");
					assetContext.ReportAssetError($"{typeKey} error creating type");
					return;
				}

				PopulateConditionParameters p = new PopulateConditionParameters(conditionType, data, localization,
					assetContext, null, conditionPrefix, conditionIndex, conditions.Length);
				try
				{
					condition.PopulateLegacy(p);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception populating condition {underlyingType}:");
				}

				conditions[conditionIndex] = condition;
			}
		}

		[System.Obsolete("NPCRewardsList.Parse should be used instead")]
		public static void readRewards(IDatDictionary data, Local localization, string prefix, INPCReward[] rewards, Asset assetContext)
		{
			for (int rewardIndex = 0; rewardIndex < rewards.Length; rewardIndex++)
			{
				string rewardPrefix = prefix + rewardIndex;

				string typeKey = rewardPrefix + "_Type";
				if (!data.ContainsKey(typeKey))
				{
					throw new System.NotSupportedException("Missing reward " + typeKey);
				}

				ENPCRewardType rewardType = data.ParseEnum<ENPCRewardType>(typeKey);
				if (rewardType == ENPCRewardType.NONE)
				{
					assetContext.ReportAssetError($"{typeKey} unknown type");
					continue;
				}

				Type underlyingType = rewardTypes[(int) rewardType];
				if (underlyingType == null)
				{
					assetContext.ReportAssetError($"{typeKey} unable to create type");
					return;
				}

				INPCReward reward;
				try
				{
					reward = Activator.CreateInstance(underlyingType) as INPCReward;
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception instantiating {underlyingType}:");
					assetContext.ReportAssetError($"{typeKey} error creating type");
					return;
				}

				PopulateRewardParameters p = new PopulateRewardParameters(rewardType, data, localization,
					assetContext, null, rewardPrefix);
				try
				{
					reward.PopulateLegacy(p);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception populating reward {underlyingType}:");
				}

				rewards[rewardIndex] = reward;
			}
		}

		static NPCTool()
		{
			conditionTypes = new Type[]
			{
				null, // NONE,
				typeof(NPCExperienceCondition), // EXPERIENCE,
				typeof(NPCReputationCondition), // REPUTATION,
				typeof(NPCBoolFlagCondition), // FLAG_BOOL,
				typeof(NPCShortFlagCondition), // FLAG_SHORT,
				typeof(NPCQuestCondition), // QUEST,
				typeof(NPCSkillsetCondition), // SKILLSET,
				typeof(NPCItemCondition), // ITEM,
				typeof(NPCZombieKillsCondition), // KILLS_ZOMBIE,
				typeof(NPCHordeKillsCondition), // KILLS_HORDE,
				typeof(NPCAnimalKillsCondition), // KILLS_ANIMAL,
				typeof(NPCCompareFlagsCondition), // COMPARE_FLAGS,
				typeof(NPCTimeOfDayCondition), // TIME_OF_DAY,
				typeof(NPCPlayerLifeHealthCondition), // PLAYER_LIFE_HEALTH,
				typeof(NPCPlayerLifeFoodCondition), // PLAYER_LIFE_FOOD,
				typeof(NPCPlayerLifeWaterCondition), // PLAYER_LIFE_WATER,
				typeof(NPCPlayerLifeVirusCondition), // PLAYER_LIFE_VIRUS,
				typeof(NPCHolidayCondition), // HOLIDAY,
				typeof(NPCPlayerKillsCondition), // KILLS_PLAYER,
				typeof(NPCObjectKillsCondition), // KILLS_OBJECT,
				typeof(NPCCurrencyCondition), // CURRENCY,
				typeof(NPCTreeKillsCondition), // KILLS_TREE,
				typeof(NPCWeatherStatusCondition), // WEATHER_STATUS,
				typeof(NPCWeatherBlendAlphaCondition), // WEATHER_BLEND_ALPHA,
				typeof(NPCIsFullMoonCondition),// IS_FULL_MOON,
				typeof(NPCDateCounterCondition), // DATE_COUNTER,
				typeof(NPCPlayerLifeStaminaCondition), // PLAYER_LIFE_STAMINA,
				typeof(NPCVolumeOverlapCondition), // VOLUME_OVERLAP,
			};

			rewardTypes = new Type[]
			{
				null, // NONE,
				typeof(NPCExperienceReward), // EXPERIENCE,
				typeof(NPCReputationReward), // REPUTATION,
				typeof(NPCBoolFlagReward), // FLAG_BOOL,
				typeof(NPCShortFlagReward), // FLAG_SHORT,
				typeof(NPCRandomShortFlagReward), // FLAG_SHORT_RANDOM,
				typeof(NPCQuestReward), // QUEST,
				typeof(NPCItemReward), // ITEM,
				typeof(NPCRandomItemReward), // ITEM_RANDOM,
				typeof(NPCAchievementReward), // ACHIEVEMENT,
				typeof(NPCVehicleReward), // VEHICLE,
				typeof(NPCTeleportReward), // TELEPORT,
				typeof(NPCEventReward), // EVENT,
				typeof(NPCFlagMathReward), // FLAG_MATH,
				typeof(NPCCurrencyReward), // CURRENCY,
				typeof(NPCHintReward), // HINT,
				typeof(NPCPlayerSpawnpointReward), // PLAYER_SPAWNPOINT,
				typeof(NPCPlayerLifeHealthReward), // PLAYER_LIFE_HEALTH,
				typeof(NPCPlayerLifeFoodReward), // PLAYER_LIFE_FOOD,
				typeof(NPCPlayerLifeWaterReward), // PLAYER_LIFE_WATER,
				typeof(NPCPlayerLifeVirusReward), // PLAYER_LIFE_VIRUS,
				typeof(NPCRewardsListAssetReward), // REWARDS_LIST_ASSET,
				typeof(NPCCutsceneModeReward), // CUTSCENE_MODE,
				typeof(NPCPlayerLifeStaminaReward), // PLAYER_LIFE_STAMINA,
				typeof(NPCEffectReward), // EFFECT,
				typeof(NPCAirdropReward), // AIRDROP,
				typeof(NPCZombieReward), // ZOMBIE,
				typeof(NPCRemoveZombieReward), // REMOVE_ZOMBIE,
			};
		}

		internal static Type[] conditionTypes;
		internal static Type[] rewardTypes;
	}
}
