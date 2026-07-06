////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCReputationCondition : NPCLogicCondition
	{
		public int reputation
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			return doesLogicPass(player.skills.reputation, reputation);
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.FormatOrEmpty("Condition_Reputation");
			}

			string format_0 = player.skills.reputation.ToString();
			if (player.skills.reputation > 0)
			{
				format_0 = "+" + format_0;
			}

			string format_1 = reputation.ToString();
			if (reputation > 0)
			{
				format_1 = "+" + format_1;
			}

			return Local.FormatText(text, format_0, format_1);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseInt32("Value", out int value))
			{
				reputation = value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseInt32(p.legacyPrefix + "_Value", out int value))
			{
				reputation = value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCReputationCondition() { }

		[System.Obsolete]
		public NPCReputationCondition(int newReputation, ENPCLogicType newLogicType, string newText) : base(newLogicType, newText, false)
		{
			reputation = newReputation;
		}
	}
}
