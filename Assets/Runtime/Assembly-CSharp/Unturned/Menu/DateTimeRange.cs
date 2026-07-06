////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	public class DateTimeRange
	{
		public DateTimeRange(DateTime start, DateTime end)
		{
			this.start = start;
			this.end = end;

			if (start.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException("DateTimeRange kind should be UTC", "start");
			}

			if (end.Kind != DateTimeKind.Utc)
			{
				throw new ArgumentException("DateTimeRange kind should be UTC", "end");
			}

			if (start > end)
			{
				throw new ArgumentException("DateTimeRange start and end are out of order");
			}
		}

		public bool isWithinRange(DateTime dateTime)
		{
			return dateTime >= start && dateTime <= end;
		}

		/// <summary>
		/// Is client UTC time within this time range?
		/// </summary>
		public bool isNowWithinRange()
		{
			DateTime now = DateTime.UtcNow;
			return now >= start && now <= end;
		}

		/// <summary>
		/// Is server UTC time within this time range?
		/// </summary>
		public bool isBackendNowWithinRange()
		{
			DateTime now = Provider.backendRealtimeDate;
			return now >= start && now <= end;
		}

		public DateTime start;
		public DateTime end;
	}
}
