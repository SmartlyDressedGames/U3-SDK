////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	[NetEnum]
	public enum ENPCHoliday
	{
		NONE,
		HALLOWEEN,
		CHRISTMAS,
		APRIL_FOOLS,
		VALENTINES,
		PRIDE_MONTH,
		LUNAR_NEW_YEAR,

		/// <summary>
		/// July 7th!
		/// </summary>
		UNTURNED_ANNIVERSARY,

		MAX, // Used to determine number of holidays.
	}

	public class NPCHolidayCondition : NPCLogicCondition
	{
		public ENPCHoliday holiday
		{
			get;
			protected set;
		}

		public override bool isConditionMet(Player player)
		{
			return doesLogicPass(HolidayUtil.getActiveHoliday(), holiday);
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseEnum("Value", out ENPCHoliday _value))
			{
				holiday = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseEnum(p.legacyPrefix + "_Value", out ENPCHoliday _value))
			{
				holiday = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCHolidayCondition() { }

		[System.Obsolete]
		public NPCHolidayCondition(ENPCHoliday newHoliday, ENPCLogicType newLogicType) : base(newLogicType, null, false)
		{
			holiday = newHoliday;
		}
	}
}
