////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Utilities
{
	/// <summary>
	/// For use with PoolablePool when no special construction is required.
	/// </summary>
	public interface IPoolable
	{
		/// <summary>
		/// Called when this instance is getting claimed.
		/// </summary>
		void poolClaim();

		/// <summary>
		/// Called when this instance is returned to the pool.
		/// </summary>
		void poolRelease();
	}
}
