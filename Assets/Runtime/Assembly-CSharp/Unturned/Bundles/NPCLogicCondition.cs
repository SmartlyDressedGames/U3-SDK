////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;

namespace SDG.Unturned
{
	public class NPCLogicCondition : INPCCondition
	{
		public ENPCLogicType logicType
		{
			get;
			protected set;
		}

		protected bool doesLogicPass<T>(T a, T b) where T : IComparable
		{
			return NPCTool.doesLogicPass(logicType, a, b);
		}

		public override void DebugDumpToStringBuilder(Player player, StringBuilder sb)
		{
			base.DebugDumpToStringBuilder(player, sb);
			sb.Append(", Op: ");
			sb.Append(logicType.ToCharAbbr());
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			ENPCLogicType defaultLogicType = GetDefaultLogicMode(p.conditionType);
			if (p.data.TryGetValue("Logic", out IDatValue logicNode))
			{
				if (TryParseLogic(logicNode, out ENPCLogicType parsedLogicType))
				{
					logicType = parsedLogicType;
				}
				else
				{
					p.ReportRequiredOptionInvalid("Logic");
					logicType = defaultLogicType;
				}
			}
			else
			{
				logicType = defaultLogicType;
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			ENPCLogicType defaultLogicType = GetDefaultLogicMode(p.conditionType);
			if (p.data.TryGetValue(p.legacyPrefix + "_Logic", out IDatValue logicNode))
			{
				if (TryParseLogic(logicNode, out ENPCLogicType parsedLogicType))
				{
					logicType = parsedLogicType;
				}
				else
				{
					p.ReportRequiredOptionInvalid("Logic");
					logicType = defaultLogicType;
				}
			}
			else
			{
				logicType = defaultLogicType;
			}
		}

		private bool TryParseLogic(IDatValue node, out ENPCLogicType parsedValue)
		{
			if (node.TryParseEnum(out parsedValue))
			{
				return true;
			}

			if (!string.IsNullOrEmpty(node.Value))
			{
				if (node.Value == "<")
				{
					parsedValue = ENPCLogicType.LESS_THAN;
				}
				else if (node.Value == "<=" || node.Value == "≤")
				{
					parsedValue = ENPCLogicType.LESS_THAN_OR_EQUAL_TO;
				}
				else if (node.Value == "==" || node.Value == "=")
				{
					parsedValue = ENPCLogicType.EQUAL;
				}
				else if (node.Value == "!=" || node.Value == "≠")
				{
					parsedValue = ENPCLogicType.NOT_EQUAL;
				}
				else if (node.Value == ">=" || node.Value == "≥")
				{
					parsedValue = ENPCLogicType.GREATER_THAN_OR_EQUAL_TO;
				}
				else if (node.Value == ">")
				{
					parsedValue = ENPCLogicType.GREATER_THAN;
				}
				else
				{
					return false;
				}
				return true;
			}

			return false;
		}

		private ENPCLogicType GetDefaultLogicMode(ENPCConditionType conditionType)
		{
			switch (conditionType)
			{
				case ENPCConditionType.ITEM:
					// Item condition previously assumed >=, but now supports other comparions.
					return ENPCLogicType.GREATER_THAN_OR_EQUAL_TO;
					
				case ENPCConditionType.HOLIDAY:
					// Holiday condition previously assumed ==, but now supports != as well.
					return ENPCLogicType.EQUAL;
					
				default:
					return ENPCLogicType.NONE;
			}
		}

		public NPCLogicCondition() { }

		[System.Obsolete]
		public NPCLogicCondition(ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newText, newShouldReset)
		{
			logicType = newLogicType;
		}
	}
}
