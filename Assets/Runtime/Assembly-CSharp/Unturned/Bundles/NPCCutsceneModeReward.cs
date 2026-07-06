////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCCutsceneModeReward : INPCReward
	{
		private bool value;

		public override void GrantReward(Player player)
		{
			player.quests.ServerSetCutsceneModeActive(value);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
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

		internal override void PopulateLegacy(in PopulateRewardParameters p)
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

		public NPCCutsceneModeReward() { }

		[System.Obsolete]
		public NPCCutsceneModeReward(bool newValue, string newText) : base(newText)
		{
			value = newValue;
		}
	}
}
