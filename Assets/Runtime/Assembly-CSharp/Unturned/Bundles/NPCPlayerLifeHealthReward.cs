////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCPlayerLifeHealthReward : INPCReward
	{
		public int value
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			player.life.serverModifyHealth(value);
		}

		public override string formatReward(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Health");
			}

			return Local.FormatText(text, value);
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

		public NPCPlayerLifeHealthReward() { }

		[System.Obsolete]
		public NPCPlayerLifeHealthReward(int newValue, string newText) : base(newText)
		{
			value = newValue;
		}
	}
}
