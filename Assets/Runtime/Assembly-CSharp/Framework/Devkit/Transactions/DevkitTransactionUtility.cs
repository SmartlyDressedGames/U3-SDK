////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Devkit.Transactions
{
	public class DevkitTransactionUtility
	{
		public static void beginGenericTransaction()
		{
			DevkitTransactionManager.beginTransaction("Generic");
		}

		public static void endGenericTransaction()
		{
			DevkitTransactionManager.endTransaction();
		}

		public static void recordInstantiation(GameObject go)
		{
			DevkitTransactionManager.recordTransaction(new DevkitGameObjectInstantiationTransaction(go));
		}

		/// <summary>
		/// Save the state of all the fields and properties on this object to the current transaction group so that they can be checked for changes once the transaction has ended.
		/// </summary>
		public static void recordObjectDelta(object instance)
		{
			DevkitTransactionManager.recordTransaction(new DevkitObjectDeltaTransaction(instance));
		}

		public static void recordDestruction(GameObject go)
		{
			DevkitTransactionManager.recordTransaction(new DevkitGameObjectDestructionTransaction(go));
		}

		public static void recordTransformChangeParent(Transform transform, Transform parent)
		{
			DevkitTransactionManager.recordTransaction(new DevkitTransformChangeParentTransaction(transform, parent));
		}
	}
}
