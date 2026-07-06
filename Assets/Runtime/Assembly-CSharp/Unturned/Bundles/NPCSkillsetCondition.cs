////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCSkillsetCondition : NPCLogicCondition
	{
		public EPlayerSkillset skillset
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			return doesLogicPass(player.channel.owner.skillset, skillset);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseEnum("Value", out EPlayerSkillset _value))
			{
				skillset = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseEnum(p.legacyPrefix + "_Value", out EPlayerSkillset _value))
			{
				skillset = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCSkillsetCondition() { }

		[System.Obsolete]
		public NPCSkillsetCondition(EPlayerSkillset newSkillset, ENPCLogicType newLogicType, string newText) : base(newLogicType, newText, false)
		{
			skillset = newSkillset;
		}
	}
}
