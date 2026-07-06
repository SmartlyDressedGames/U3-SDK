////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Transactions;

namespace SDG.Unturned
{
	public class ScopedObjectUndo : System.IDisposable
	{
		public ScopedObjectUndo(object modifiedObject)
		{
			DevkitTransactionUtility.beginGenericTransaction();
			DevkitTransactionUtility.recordObjectDelta(modifiedObject);
		}

		public void Dispose()
		{
			DevkitTransactionUtility.endGenericTransaction();
		}
	}
}
