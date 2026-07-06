////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_OBJECT_DELTA

using System.Reflection;

namespace SDG.Framework.Devkit.Transactions
{
	public struct TransactionFieldDelta : ITransactionDelta
	{
		public FieldInfo field;
		public object before;
		public object after;

		public void undo(object instance)
		{
			field.SetValue(instance, before);
		}

		public void redo(object instance)
		{
			field.SetValue(instance, after);
		}

		public TransactionFieldDelta(FieldInfo newField) : this(newField, null, null)
		{ }

		public TransactionFieldDelta(FieldInfo newField, object newBefore, object newAfter)
		{
			field = newField;
			before = newBefore;
			after = newAfter;
		}
	}
}
