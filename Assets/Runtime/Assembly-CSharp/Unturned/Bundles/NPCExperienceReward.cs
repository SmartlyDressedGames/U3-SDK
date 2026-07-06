////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCExperienceReward : INPCReward
	{
		public uint value
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			player.skills.askAward(value);
		}

		public override string formatReward(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Experience");
			}

			string format = "+" + value;
			return Local.FormatText(text, format);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseUInt32("Value", out uint _value))
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

			if (p.data.TryParseUInt32(p.legacyPrefix + "_Value", out uint _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCExperienceReward() { }

		[System.Obsolete]
		public NPCExperienceReward(uint newValue, string newText) : base(newText)
		{
			value = newValue;
		}
	}
}
