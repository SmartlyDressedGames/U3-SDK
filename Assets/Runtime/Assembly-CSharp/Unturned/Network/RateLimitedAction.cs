////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	///	Note: new official code should be using per-method rate limit attribute.
	/// This is kept for backwards compatibility with plugins however.
	///
	/// Timestamp for server-side rate limiting.
	/// </summary>
	public struct RateLimitedAction
	{
		/// <summary>
		/// Realtime this action was performed.
		/// </summary>
		public float performedRealtime;

		/// <summary>
		/// Realtime since performedRealtime.
		/// </summary>
		public float realtimeSincePerformed => Time.realtimeSinceStartup - performedRealtime;

		public bool hasIntervalPassed(float interval)
		{
			return realtimeSincePerformed > interval;
		}

		/// <summary>
		/// if(myRateLimit.throttle(1.0))
		///		return; // less than 1s passed
		/// </summary>
		public bool throttle(float interval)
		{
			if (hasIntervalPassed(interval))
			{
				performedRealtime = Time.realtimeSinceStartup;
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
