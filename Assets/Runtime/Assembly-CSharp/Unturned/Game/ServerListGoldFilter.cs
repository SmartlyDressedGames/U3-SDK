////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Filter for whether the Permanent Gold Upgrade DLC is required to join a server.
	/// </summary>
	public enum EServerListGoldFilter
	{
		/// <summary>
		/// All servers pass the filter.
		/// </summary>
		Any,

		/// <summary>
		/// Only non-gold servers pass the filter.
		/// </summary>
		DoesNotRequireGold,

		/// <summary>
		/// Only gold servers pass the filter.
		/// </summary>
		RequiresGold,
	}
}
