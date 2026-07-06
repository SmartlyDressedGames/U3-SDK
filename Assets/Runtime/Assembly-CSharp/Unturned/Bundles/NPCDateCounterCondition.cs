////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCDateCounterCondition : NPCLogicCondition
	{
		protected long value;
		protected long divisor;

		public override bool isConditionMet(Player player)
		{
			long remainder = LightingManager.DateCounter % divisor;
			return NPCTool.doesLogicPass(logicType, remainder, value);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseInt64("Value", out long _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}

			if (p.data.TryParseInt64("Divisor", out long _divisor))
			{
				divisor = _divisor;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Divisor");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseInt64(p.legacyPrefix + "_Value", out long _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}

			if (p.data.TryParseInt64(p.legacyPrefix + "_Divisor", out long _divisor))
			{
				divisor = _divisor;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Divisor");
			}
		}

		public NPCDateCounterCondition() { }

		[System.Obsolete]
		public NPCDateCounterCondition(long newValue, long newDivisor, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newLogicType, newText, newShouldReset)
		{
			value = newValue;
			divisor = newDivisor;
		}
	}
}
