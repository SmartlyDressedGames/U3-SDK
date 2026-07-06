////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCBoolFlagCondition : NPCFlagCondition
	{
		public bool value
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			short flag;
			if (player.quests.getFlag(id, out flag))
			{
				return doesLogicPass(flag == 1, value);
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

			return Local.FormatText(text, isConditionMet(player) ? 1 : 0);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseBool("Value", out bool _value))
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

			if (p.data.TryParseBool(p.legacyPrefix + "_Value", out bool _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCBoolFlagCondition() { }

		[System.Obsolete]
		public NPCBoolFlagCondition(ushort newID, bool newValue, bool newAllowUnset, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newID, newAllowUnset, newLogicType, newText, newShouldReset)
		{
			value = newValue;
		}
	}
}
