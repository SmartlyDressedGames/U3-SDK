////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCShortFlagCondition : NPCFlagCondition
	{
		public short value
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			short flag;
			if (player.quests.getFlag(id, out flag))
			{
				return doesLogicPass(flag, value);
			}
			else
			{
				return allowUnset;
			}
		}

		public override void ApplyCondition(Player player)
		{
			if (!shouldReset)
			{
				return;
			}

			player.quests.sendRemoveFlag(id);
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			short flag;
			if (!player.quests.getFlag(id, out flag))
			{
				if (allowUnset)
				{
					flag = value;
				}
				else
				{
					flag = 0;
				}
			}

			return Local.FormatText(text, flag, value);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseInt16("Value", out short _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseInt16(p.legacyPrefix + "_Value", out short _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCShortFlagCondition() { }

		[System.Obsolete]
		public NPCShortFlagCondition(ushort newID, short newValue, bool newAllowUnset, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newID, newAllowUnset, newLogicType, newText, newShouldReset)
		{
			value = newValue;
		}
	}
}
