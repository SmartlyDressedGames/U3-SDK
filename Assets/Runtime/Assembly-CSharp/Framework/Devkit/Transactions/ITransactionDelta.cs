////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_OBJECT_DELTA

namespace SDG.Framework.Devkit.Transactions
{
	public interface ITransactionDelta
	{
		void undo(object instance);
		void redo(object instance);
	}
}
