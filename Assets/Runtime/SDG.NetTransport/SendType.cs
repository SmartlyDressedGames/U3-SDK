////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetTransport
{
	/// <summary>
	/// Ideally this will become purely "unreliable", and layer on top of transport will handle message building.
	/// </summary>
	[System.Obsolete]
	public enum ESendType
	{
		RELIABLE,
		RELIABLE_NODELAY,
		UNRELIABLE,
		UNRELIABLE_NODELAY
	}

	public enum ENetReliability
	{
		Reliable,
		Unreliable
	}
}
