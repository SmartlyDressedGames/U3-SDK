////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// True once per frame, false otherwise.
	/// </summary>
	public struct OncePerFrameGuard
	{
		public bool Consume()
		{
			int frameNumber = Time.frameCount;
			if (frameNumber > consumedFrameNumber)
			{
				consumedFrameNumber = frameNumber;
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool HasBeenConsumed => Time.frameCount == consumedFrameNumber;

		private int consumedFrameNumber;
	}
}
