////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// 2023-01-25: fixing killing self with explosive to track kill under
	/// the assumption that this is only used for tracking stats. (public issue #2692)
	/// </summary>
	public enum EPlayerKill
	{
		NONE,
		PLAYER,
		ZOMBIE,
		MEGA,
		ANIMAL,
		RESOURCE,
		OBJECT
	}
}
