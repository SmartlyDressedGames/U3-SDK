////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public struct NPCConditionsList
	{
		/// <summary>
		/// Exposed for plugins. Can be null. Please do not modify.
		/// </summary>
		public INPCCondition[] GetConditions()
		{
			return conditions;
		}

		public bool IsEmpty => conditions == null || conditions.Length < 1;

		public INPCCondition GetFirstUnmetCondition(Player player)
		{
			if (conditions != null)
			{
				foreach (INPCCondition condition in conditions)
				{
					if (!condition.isConditionMet(player))
					{
						return condition;
					}
				}
			}

			return null;
		}

		public bool AreConditionsMet(Player player)
		{
			if (conditions != null)
			{
				foreach (INPCCondition condition in conditions)
				{
					if (!condition.isConditionMet(player))
					{
						return false;
					}
				}
			}

			return true;
		}

		public void ApplyConditions(Player player)
		{
			if (conditions != null)
			{
				foreach (INPCCondition condition in conditions)
				{
					condition.ApplyCondition(player);
				}
			}
		}

		/// <summary>
		/// This overload supports legacy Condition_# format.
		/// </summary>
		public void Parse(IDatDictionary data, Local localization, Asset assetContext, string countKey, string legacyPrefixKey)
		{
			if (!data.TryGetNode(countKey, out IDatNode node))
				return;

			if (node is IDatValue valueNode)
			{
				int count = valueNode.ParseInt32();
				if (count > 0)
				{
					conditions = new INPCCondition[count];
#pragma warning disable
					NPCTool.readConditions(data, localization, legacyPrefixKey, conditions, assetContext);
#pragma warning restore
				}
			}
			else if (node is IDatList listNode)
			{
				Parse(localization, assetContext, listNode, countKey);
			}
		}

		/// <summary>
		/// This overload doesn't support legacy Condition_# format.
		/// </summary>
		public void Parse(IDatDictionary data, Local localization, Asset assetContext, string key)
		{
			if (data.TryGetList(key, out IDatList listNode))
			{
				Parse(localization, assetContext, listNode, key);
			}
		}

		private void Parse(Local localization, Asset assetContext, IDatList listNode, string countKey)
		{
			tempConditions.Clear();

			int conditionIndex = -1;
			foreach (IDatNode conditionNode in listNode)
			{
				++conditionIndex;
				if (!(conditionNode is IDatDictionary conditionData))
					continue;

				string errorInfo = $"{countKey}[{conditionIndex}]";

				if (!conditionData.TryParseEnum("Type", out ENPCConditionType conditionType))
				{
					if (conditionData.ContainsKey("Type"))
					{
						assetContext.ReportAssetError($"{errorInfo} missing condition Type");
					}
					else
					{
						assetContext.ReportAssetError($"{errorInfo} unable to parse condition type \"{conditionData.GetString("Type")}\"");
					}
					continue;
				}

				Type underlyingType = NPCTool.conditionTypes[(int) conditionType];
				if (underlyingType == null)
				{
					assetContext.ReportAssetError($"{errorInfo} unable to create type {conditionType}");
					continue;
				}

				INPCCondition condition;
				try
				{
					condition = Activator.CreateInstance(underlyingType) as INPCCondition;
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception instantiating {underlyingType}:");
					assetContext.ReportAssetError($"{errorInfo} error creating type {conditionType}");
					continue;
				}

				PopulateConditionParameters p = new PopulateConditionParameters(conditionType, conditionData,
					localization, assetContext, errorInfo, null, conditionIndex, listNode.Count);
				try
				{
					condition.PopulateV2(p);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception populating {errorInfo} {underlyingType}:");
					continue;
				}

				tempConditions.Add(condition);
			}

			if (tempConditions.Count > 0)
			{
				conditions = tempConditions.ToArray();
			}
		}

		public void DebugDumpToStringBuilder(System.Text.StringBuilder output)
		{
			output.AppendLine($"{conditions?.Length} conditions(s)");
			if (conditions != null)
			{
				for (int index = 0; index < conditions.Length; ++index)
				{
					output.AppendLine($"[{index}]: {conditions[index]}");
				}
			}
		}

		public void DebugDumpToStringBuilder(Player player, System.Text.StringBuilder output)
		{
			if (conditions != null)
			{
				for (int index = 0; index < conditions.Length; ++index)
				{
					output.Append("[");
					output.Append(index);
					output.Append("] ");
					INPCCondition condition = conditions[index];
					condition.DebugDumpToStringBuilder(player, output);
					output.AppendLine();
				}
			}
		}

		public string DebugDumpToString()
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			DebugDumpToStringBuilder(output);
			return output.ToString();
		}

		public string DebugDumpToString(Player player)
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			DebugDumpToStringBuilder(player, output);
			return output.ToString();
		}

		internal INPCCondition[] conditions;
		private static List<INPCCondition> tempConditions = new List<INPCCondition>();
	}
}
