////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Devkit.Transactions
{
	public interface IDevkitTransaction
	{
		/// <summary>
		/// If false this transaction is ignored. If there were no changes at all in the group it's discarded.
		/// </summary>
		bool delta
		{
			get;
		}

		void undo();
		void redo();

		void begin();
		void end();

		/// <summary>
		/// Called when history buffer is too long so this transaction is discarded.
		/// </summary>
		void forget();
	}
}
