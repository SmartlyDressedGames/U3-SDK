////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCIsFullMoonCondition : INPCCondition
	{
		public bool value
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			return LightingManager.isFullMoon == value;
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

		public NPCIsFullMoonCondition() { }

		[System.Obsolete]
		public NPCIsFullMoonCondition(bool newValue, string newText) : base(newText, false)
		{
			value = newValue;
		}
	}
}
