////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.Devkit.Transactions
{
	public class DevkitTransactionGroup
	{
		public string name
		{
			get;
			protected set;
		}

		public List<IDevkitTransaction> transactions
		{
			get;
			protected set;
		}

		public void record(IDevkitTransaction transaction)
		{
			transaction.begin();
			transactions.Add(transaction);
		}

		public bool delta
		{
			get
			{
				for (int index = transactions.Count - 1; index >= 0; index--)
				{
					if (!transactions[index].delta)
					{
						transactions.RemoveAt(index);
					}
				}

				return transactions.Count > 0;
			}
		}

		public void undo()
		{
			for (int index = 0; index < transactions.Count; index++)
			{
				transactions[index].undo();
			}
		}

		public void redo()
		{
			for (int index = 0; index < transactions.Count; index++)
			{
				transactions[index].redo();
			}
		}

		public void end()
		{
			for (int index = 0; index < transactions.Count; index++)
			{
				transactions[index].end();
			}
		}

		public void forget()
		{
			for (int index = 0; index < transactions.Count; index++)
			{
				transactions[index].forget();
			}
		}

		public DevkitTransactionGroup(string newName)
		{
			name = newName;
			transactions = new List<IDevkitTransaction>();
		}
	}
}
