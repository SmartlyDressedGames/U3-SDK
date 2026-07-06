////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.Devkit.Transactions
{
	public delegate void DevkitTransactionPerformedHandler(DevkitTransactionGroup group);
	public delegate void DevkitTransactionsChangedHandler();

	public class DevkitTransactionManager
	{
		private static uint _historyLength = 25;
		public static uint historyLength
		{
			get => _historyLength;
			set
			{
				_historyLength = value;

				SDG.Unturned.UnturnedLog.info("Set history_length to: " + historyLength);
			}
		}

		public static event DevkitTransactionPerformedHandler transactionPerformed;

		protected static void triggerTransactionPerformed(DevkitTransactionGroup group)
		{
			transactionPerformed?.Invoke(group);
		}

		public static event DevkitTransactionsChangedHandler transactionsChanged;

		protected static void triggerTransactionsChanged()
		{
			transactionsChanged?.Invoke();
		}

		protected static LinkedList<DevkitTransactionGroup> undoable = new LinkedList<DevkitTransactionGroup>();
		protected static Stack<DevkitTransactionGroup> redoable = new Stack<DevkitTransactionGroup>();

		protected static DevkitTransactionGroup pendingGroup;
		protected static int transactionDepth;

		public static bool canUndo => undoable.Count > 0;

		public static bool canRedo => redoable.Count > 0;

		public static IEnumerable<DevkitTransactionGroup> getUndoable()
		{
			return undoable;
		}

		public static IEnumerable<DevkitTransactionGroup> getRedoable()
		{
			return redoable;
		}

		public static DevkitTransactionGroup undo()
		{
			if (!canUndo)
			{
				return null;
			}

			DevkitTransactionGroup group = popUndo();
			group.undo();
			pushRedo(group);

			triggerTransactionPerformed(group);
			return group;
		}

		public static DevkitTransactionGroup redo()
		{
			if (!canRedo)
			{
				return null;
			}

			DevkitTransactionGroup group = popRedo();
			group.redo();
			pushUndo(group);

			triggerTransactionPerformed(group);
			return group;
		}

		/// <summary>
		/// Open a new transaction group which stores multiple undo/redoable actions, for example this would be called before moving an object.
		/// </summary>
		public static void beginTransaction(string name)
		{
			if (transactionDepth == 0)
			{
				clearRedo();
				pendingGroup = new DevkitTransactionGroup(name);
			}

			transactionDepth++;
		}

		public static void recordTransaction(IDevkitTransaction transaction)
		{
			if (pendingGroup == null)
			{
				return;
			}

			pendingGroup.record(transaction);
		}

		/// <summary>
		/// Close the pending transaction and finalize any change checks.
		/// </summary>
		public static void endTransaction()
		{
			if (transactionDepth == 0)
			{
				return;
			}

			transactionDepth--;
			if (transactionDepth == 0)
			{
				pendingGroup.end();

				if (pendingGroup.delta)
				{
					pushUndo(pendingGroup);
				}
				else
				{
					pendingGroup.forget();
				}

				pendingGroup = null;

				triggerTransactionsChanged();
			}
		}

		/// <summary>
		/// Clear the undo/redo queues.
		/// </summary>
		public static void resetTransactions()
		{
			clearUndo();
			clearRedo();

			pendingGroup = null;
			transactionDepth = 0;
		}

		protected static void pushUndo(DevkitTransactionGroup group)
		{
			if (undoable.Count >= historyLength)
			{
				undoable.First.Value.forget();
				undoable.RemoveFirst();
			}

			undoable.AddLast(group);
		}

		protected static DevkitTransactionGroup popUndo()
		{
			DevkitTransactionGroup group = undoable.Last.Value;
			undoable.RemoveLast();

			return group;
		}

		protected static void clearUndo()
		{
			while (undoable.Count > 0)
			{
				DevkitTransactionGroup group = undoable.Last.Value;
				undoable.RemoveLast();
				group.forget();
			}

			undoable.Clear();
		}

		protected static void pushRedo(DevkitTransactionGroup group)
		{
			redoable.Push(group);
		}

		protected static DevkitTransactionGroup popRedo()
		{
			return redoable.Pop();
		}

		protected static void clearRedo()
		{
			while (redoable.Count > 0)
			{
				DevkitTransactionGroup group = redoable.Pop();
				group.forget();
			}

			redoable.Clear();
		}
	}
}
