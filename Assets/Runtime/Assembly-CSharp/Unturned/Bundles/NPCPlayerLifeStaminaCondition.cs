////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCPlayerLifeStaminaCondition : NPCLogicCondition
	{
		public int stamina
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			return doesLogicPass(player.life.stamina, stamina);
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			return Local.FormatText(text, player.life.stamina, stamina);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseInt32("Value", out int _value))
			{
				stamina = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseInt32(p.legacyPrefix + "_Value", out int _value))
			{
				stamina = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCPlayerLifeStaminaCondition() { }

		[System.Obsolete]
		public NPCPlayerLifeStaminaCondition(int newStamina, ENPCLogicType newLogicType, string newText) : base(newLogicType, newText, false)
		{
			stamina = newStamina;
		}
	}
}
