////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	public class DismissableTimeSpan
	{
		public DismissableTimeSpan(DateTime start, DateTime end, string key)
		{
			range = new DateTimeRange(start, end);
			this.key = key;
		}

		/// <summary>
		/// Is current UTC time within this time span, and player has not dismissed?
		/// </summary>
		public bool isRelevant()
		{
			if (isNowWithinSpan())
			{
				if (hasDismissedSpan())
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Has the current time span been dismissed?
		/// For example, player may have dismissed a previous event but not this current one.
		/// </summary>
		public bool hasDismissedSpan()
		{
			DateTime dismissedTime;
			bool hasEverDismissed = getDismissedTime(out dismissedTime);
			if (hasEverDismissed == false)
			{
				// Player has never dismissed.
				return false;
			}

			return dismissedTime >= range.start;
		}

		/// <summary>
		/// Is current UTC time within this time span?
		/// </summary>
		public bool isNowWithinSpan()
		{
			return range.isNowWithinRange();
		}

		public bool getDismissedTime(out DateTime dismissedTime)
		{
			return ConvenientSavedata.get().read(key, out dismissedTime);
		}

		public void dismiss()
		{
			DateTime now = DateTime.UtcNow;
			ConvenientSavedata.get().write(key, now);
		}

		private DateTimeRange range;
		private string key;
	}
}
