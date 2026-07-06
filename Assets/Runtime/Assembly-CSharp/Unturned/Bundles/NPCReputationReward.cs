////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCReputationReward : INPCReward
	{
		public int value
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			player.skills.askRep(value);
		}

		public override string formatReward(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Reputation");
			}

			string format = value.ToString();
			if (value > 0)
			{
				format = "+" + format;
			}

			return Local.FormatText(text, format);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseInt32("Value", out int _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseInt32(p.legacyPrefix + "_Value", out int _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCReputationReward() { }

		[System.Obsolete]
		public NPCReputationReward(int newValue, string newText) : base(newText)
		{
			value = newValue;
		}
	}
}
