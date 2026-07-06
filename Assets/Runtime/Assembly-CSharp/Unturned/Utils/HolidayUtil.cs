////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	public static class HolidayUtil
	{
		public static bool isHolidayActive(ENPCHoliday holiday)
		{
			return holiday == Provider.authorityHoliday;
		}

		public static ENPCHoliday getActiveHoliday()
		{
			return Provider.authorityHoliday;
		}

		internal static ENPCHoliday GetScheduledHoliday()
		{
			if (holidayOverride != ENPCHoliday.NONE)
			{
				return holidayOverride;
			}

			DateTime now = DateTime.UtcNow;
			for (int holidayIndex = (int) ENPCHoliday.NONE + 1; holidayIndex < (int) ENPCHoliday.MAX; ++holidayIndex)
			{
				DateTimeRange schedule = scheduledHolidays[holidayIndex];
				if (schedule != null && schedule.isWithinRange(now))
				{
					return (ENPCHoliday) holidayIndex;
				}
			}

			return ENPCHoliday.NONE;
		}

		private static void scheduleHoliday(ENPCHoliday holiday, DateTime start, DateTime end)
		{
			DateTime utcStart = start.ToUniversalTime();
			DateTime utcEnd = end.ToUniversalTime();
			UnturnedLog.info($"Scheduled {holiday} from {start} to {end} local time ({utcStart} to {utcEnd} UTC)");
			scheduledHolidays[(int) holiday] = new DateTimeRange(utcStart, utcEnd);
		}

		public static void scheduleHolidays(HolidayStatusData data)
		{
			System.DateTime localNow = System.DateTime.Now;
			int localYear = localNow.Year;
			int localMonth = localNow.Month;

			int christmasStartYear = localMonth > 6 ? localYear : localYear - 1;
			scheduleHoliday(ENPCHoliday.CHRISTMAS,
				new DateTime(christmasStartYear, /*month*/ 12, /*day*/ 7, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local),
				new DateTime(christmasStartYear + 1, /*month*/ 1, /*day*/ 2, /*hour*/ 12, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local));

			scheduleHoliday(ENPCHoliday.HALLOWEEN,
				new DateTime(localYear, /*month*/ 10, /*day*/ 20, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local),
				new DateTime(localYear, /*month*/ 11, /*day*/ 1, /*hour*/ 12, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local));

			scheduleHoliday(ENPCHoliday.VALENTINES,
				new DateTime(localYear, /*month*/ 2, /*day*/ 14, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local),
				new DateTime(localYear, /*month*/ 2, /*day*/ 14, /*hour*/ 23, /*minute*/ 59, /*second*/ 59, DateTimeKind.Local));

			scheduleHoliday(ENPCHoliday.APRIL_FOOLS,
				new DateTime(localYear, /*month*/ 4, /*day*/ 1, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local),
				new DateTime(localYear, /*month*/ 4, /*day*/ 1, /*hour*/ 23, /*minute*/ 59, /*second*/ 59, DateTimeKind.Local));

			scheduleHoliday(ENPCHoliday.PRIDE_MONTH,
				new DateTime(localYear, /*month*/ 6, /*day*/ 1, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local),
				new DateTime(localYear, /*month*/ 6, /*day*/ 30, /*hour*/ 23, /*minute*/ 59, /*second*/ 59, DateTimeKind.Local));

			if (data.LunarNewYear_StartOverride.Ticks > 0 && data.LunarNewYear_EndOverride.Ticks > 0)
			{
				scheduleHoliday(ENPCHoliday.LUNAR_NEW_YEAR, data.LunarNewYear_StartOverride, data.LunarNewYear_EndOverride);
			}
			else
			{
				// Credit to Dai CC BY-SA 4.0 on Stack Overflow: https://stackoverflow.com/a/30719491
				// Revised slightly.
				System.Globalization.ChineseLunisolarCalendar chineseLunisolarCalendar = new System.Globalization.ChineseLunisolarCalendar();
				DateTime lunarNewYearStartDate = chineseLunisolarCalendar.ToDateTime(localYear, 1, 1, 0, 0, 0, 0);
				DateTime dayBeforeLNY = lunarNewYearStartDate.AddDays(-1);
				DateTime lnyEnd = lunarNewYearStartDate.AddDays(data.LunarNewYear_Days);
				scheduleHoliday(ENPCHoliday.LUNAR_NEW_YEAR,
					new DateTime(localYear, dayBeforeLNY.Month, dayBeforeLNY.Day, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local),
					new DateTime(localYear, lnyEnd.Month, lnyEnd.Day, /*hour*/ 23, /*minute*/ 59, /*second*/ 59, DateTimeKind.Local));
			}

			scheduleHoliday(ENPCHoliday.UNTURNED_ANNIVERSARY,
				new DateTime(localYear, /*month*/ 7, /*day*/ 7, /*hour*/ 0, /*minute*/ 0, /*second*/ 0, DateTimeKind.Local),
				new DateTime(localYear, /*month*/ 7, /*day*/ 7, /*hour*/ 23, /*minute*/ 59, /*second*/ 59, DateTimeKind.Local));
		}

		static HolidayUtil()
		{
			scheduledHolidays = new DateTimeRange[(int) ENPCHoliday.MAX];

			holidayOverride = ENPCHoliday.NONE;
			if (clHolidayOverride.hasValue)
			{
				string value = clHolidayOverride.value;
				if (string.Equals(value, "Halloween", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "HW", StringComparison.OrdinalIgnoreCase))
				{
					holidayOverride = ENPCHoliday.HALLOWEEN;
				}
				else if (string.Equals(value, "Christmas", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "XMAS", StringComparison.OrdinalIgnoreCase))
				{
					holidayOverride = ENPCHoliday.CHRISTMAS;
				}
				else if (string.Equals(value, "AprilFools", StringComparison.OrdinalIgnoreCase))
				{
					holidayOverride = ENPCHoliday.APRIL_FOOLS;
				}
				else if (string.Equals(value, "Valentines", StringComparison.OrdinalIgnoreCase))
				{
					holidayOverride = ENPCHoliday.VALENTINES;
				}
				else if (string.Equals(value, "PrideMonth", StringComparison.OrdinalIgnoreCase))
				{
					holidayOverride = ENPCHoliday.PRIDE_MONTH;
				}
				else if (string.Equals(value, "LunarNewYear", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "LNY", StringComparison.OrdinalIgnoreCase))
				{
					holidayOverride = ENPCHoliday.LUNAR_NEW_YEAR;
				}
				else if (string.Equals(value, "UnturnedAnniversary", StringComparison.OrdinalIgnoreCase))
				{
					holidayOverride = ENPCHoliday.UNTURNED_ANNIVERSARY;
				}
				else
				{
					UnturnedLog.warn("Unknown holiday \"{0}\" requested by command-line override", value);
				}
			}
		}

		private static DateTimeRange[] scheduledHolidays;
		private static CommandLineString clHolidayOverride = new CommandLineString("-Holiday");
		private static ENPCHoliday holidayOverride;
	}
}
