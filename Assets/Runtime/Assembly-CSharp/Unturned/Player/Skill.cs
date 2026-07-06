////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Skill
	{
		public byte level;

		/// <summary>
		/// Vanilla maximum level.
		/// </summary>
		public byte max;

		/// <summary>
		/// If set, maximum skill level attainable through gameplay.
		/// </summary>
		public int maxUnlockableLevel = -1;

		/// <summary>
		/// Multiplier for XP upgrade cost.
		/// </summary>
		public float costMultiplier = 1.0f;

		/// <summary>
		/// Get maximum level, or maxUnlockableLevel if set.
		/// </summary>
		/// <returns></returns>
		public int GetClampedMaxUnlockableLevel()
		{
			return maxUnlockableLevel > -1 ? Mathf.Min(max, maxUnlockableLevel) : max;
		}

		/// <summary>
		/// 0.0 when <= 0, 1.0 when >= max, otherwise returns percentage between 0 and maximum level.
		/// </summary>
		public float NormalizeLevel(int inputLevel)
		{
			if (inputLevel <= 0)
			{
				return 0.0f;
			}
			else if (inputLevel >= max)
			{
				return 1.0f;
			}
			else
			{
				return inputLevel / (float) max;
			}
		}

		public void setLevelToMax()
		{
			level = max;
		}

		public float mastery
		{
			get
			{
				if (level == 0)
				{
					return 0f;
				}
				else if (level >= max)
				{
					return 1f;
				}
				else
				{
					return level / (float) max;
				}
			}
		}

		internal int baseCost;
		internal int perLevelCostIncrease;
		public uint cost => MathfEx.RoundAndClampToUInt((baseCost + level * perLevelCostIncrease) * costMultiplier);

		public Skill(byte newLevel, byte newMax, uint newCost, float newDifficulty)
		{
			level = newLevel;
			max = newMax;

			// Nelson 2025-09-11: previously, upgrade cost equation was:
			// _cost * ((level * difficulty) + 1) * costMultiplier
			// For example with cost = 20, difficulty = 1.5:
			// level 0: 20
			// level 1: 50
			// level 2: 80
			baseCost = (int) newCost;
			perLevelCostIncrease = Mathf.RoundToInt(baseCost * newDifficulty);
		}
	}
}
