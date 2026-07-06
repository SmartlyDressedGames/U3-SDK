////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;

namespace SDG.Unturned
{
	[NetEnum]
	public enum EPlayerGesture
	{
		NONE,
		INVENTORY_START,
		INVENTORY_STOP,
		PICKUP,
		PUNCH_LEFT,
		PUNCH_RIGHT,
		SURRENDER_START,
		SURRENDER_STOP,
		POINT,
		WAVE,
		SALUTE,
		ARREST_START,
		ARREST_STOP,
		REST_START,
		REST_STOP,
		FACEPALM,
		T_POSE_START,
		T_POSE_STOP,
	}
}
