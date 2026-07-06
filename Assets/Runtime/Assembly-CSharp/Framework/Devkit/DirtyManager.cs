////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Framework.Devkit
{
	public delegate void MarkedDirtyHandler(IDirtyable item);
	public delegate void MarkedCleanHandler(IDirtyable item);
	public delegate void SaveableChangedHandler(IDirtyable item, bool isSaveable);
	public delegate void DirtySaved();

	public class DirtyManager
	{
		protected static List<IDirtyable> _dirty = new List<IDirtyable>();
		public static List<IDirtyable> dirty => _dirty;

		public static HashSet<IDirtyable> _notSaveable = new HashSet<IDirtyable>();
		public static HashSet<IDirtyable> notSaveable => _notSaveable;

		public static event MarkedDirtyHandler markedDirty;
		public static event MarkedCleanHandler markedClean;
		public static event SaveableChangedHandler saveableChanged;
		public static event DirtySaved saved;

		protected static bool isSaving;

		public static void markDirty(IDirtyable item)
		{
			dirty.Add(item);

			triggerMarkedDirty(item);
		}

		public static void markClean(IDirtyable item)
		{
			if (isSaving)
			{
				return;
			}

			dirty.Remove(item);

			triggerMarkedClean(item);
		}

		public static bool checkSaveable(IDirtyable item)
		{
			return !notSaveable.Contains(item);
		}

		public static void toggleSaveable(IDirtyable item)
		{
			if (!notSaveable.Remove(item))
			{
				notSaveable.Add(item);

				triggerSaveableChanged(item, true);
			}
			else
			{
				triggerSaveableChanged(item, false);
			}
		}

		public static void save()
		{
			isSaving = true;

			for (int dirtyIndex = dirty.Count - 1; dirtyIndex >= 0; dirtyIndex--)
			{
				IDirtyable item = dirty[dirtyIndex];

				if (notSaveable.Contains(item))
				{
					continue;
				}

				item.save();
				item.isDirty = false;
				dirty.RemoveAt(dirtyIndex);
			}

			isSaving = false;

			triggerSaved();
		}

		protected static void triggerMarkedDirty(IDirtyable item)
		{
			markedDirty?.Invoke(item);
		}

		protected static void triggerMarkedClean(IDirtyable item)
		{
			markedClean?.Invoke(item);
		}

		protected static void triggerSaveableChanged(IDirtyable item, bool isSaveable)
		{
			saveableChanged?.Invoke(item, isSaveable);
		}

		protected static void triggerSaved()
		{
			saved?.Invoke();
		}
	}
}
