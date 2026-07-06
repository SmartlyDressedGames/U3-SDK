////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCPlayerLifeFoodCondition : NPCLogicCondition
	{
		public int food
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			return doesLogicPass(player.life.food, food);
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			return Local.FormatText(text, player.life.food, food);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseInt32("Value", out int _value))
			{
				food = _value;
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
				food = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCPlayerLifeFoodCondition() { }

		[System.Obsolete]
		public NPCPlayerLifeFoodCondition(int newFood, ENPCLogicType newLogicType, string newText) : base(newLogicType, newText, false)
		{
			food = newFood;
		}
	}
}
