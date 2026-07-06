////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class NPCCompareFlagsCondition : NPCLogicCondition
	{
		public ushort flag_A_ID
		{
			get;
			protected set;
		}

		public bool allowFlag_A_Unset
		{
			get;
			protected set;
		}

		public ushort flag_B_ID;

		public bool allowFlag_B_Unset
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			short flag_A_Value;
			short flag_B_Value;

			if (!player.quests.getFlag(flag_A_ID, out flag_A_Value) && !allowFlag_A_Unset)
			{
				return false;
			}

			if (!player.quests.getFlag(flag_B_ID, out flag_B_Value) && !allowFlag_B_Unset)
			{
				return false;
			}

			return doesLogicPass(flag_A_Value, flag_B_Value);
		}

		public override void ApplyCondition(Player player)
		{
			if (!shouldReset)
			{
				return;
			}

			player.quests.sendRemoveFlag(flag_A_ID);
			player.quests.sendRemoveFlag(flag_B_ID);
		}
		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			return text;
		}

		public override bool isAssociatedWithFlag(ushort flagID)
		{
			return flagID == flag_A_ID || flagID == flag_B_ID;
		}

		internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
		{
			associatedFlags.Add(flag_A_ID);
			associatedFlags.Add(flag_B_ID);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseUInt16("A_ID", out ushort _a_id))
			{
				flag_A_ID = _a_id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("A_ID");
			}

			if (p.data.TryParseUInt16("B_ID", out ushort _b_id))
			{
				flag_B_ID = _b_id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("B_ID");
			}

			allowFlag_A_Unset = p.data.ParseBool("Allow_A_Unset");
			allowFlag_B_Unset = p.data.ParseBool("Allow_B_Unset");
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseUInt16(p.legacyPrefix + "_A_ID", out ushort _a_id))
			{
				flag_A_ID = _a_id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("A_ID");
			}

			if (p.data.TryParseUInt16(p.legacyPrefix + "_B_ID", out ushort _b_id))
			{
				flag_B_ID = _b_id;
			}
			else
			{
				p.ReportRequiredOptionInvalid("B_ID");
			}

			allowFlag_A_Unset = p.data.ContainsKey(p.legacyPrefix + "_Allow_A_Unset");
			allowFlag_B_Unset = p.data.ContainsKey(p.legacyPrefix + "_Allow_B_Unset");
		}

		public NPCCompareFlagsCondition() { }

		[System.Obsolete]
		public NPCCompareFlagsCondition(ushort newFlag_A_ID, ushort newFlag_B_ID, bool newAllowFlag_A_Unset, bool newAllowFlag_B_Unset, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newLogicType, newText, newShouldReset)
		{
			flag_A_ID = newFlag_A_ID;
			allowFlag_A_Unset = newAllowFlag_A_Unset;

			flag_B_ID = newFlag_B_ID;
			allowFlag_B_Unset = newAllowFlag_B_Unset;
		}
	}
}
