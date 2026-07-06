////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{

	public struct DatListValueEnumerator : IEnumerator<IDatValue>
	{
		public DatListValueEnumerator(IDatList list)
		{
			this.list = list;
			index = -1;
			current = null;
		}

		public IDatValue Current => current;

		object System.Collections.IEnumerator.Current => current;

		public bool MoveNext()
		{
			while (++index < list.Count)
			{
				current = list[index] as IDatValue;
				if (current != null)
				{
					return true;
				}
			}

			return false;
		}

		public void Reset()
		{
			index = -1;
			current = null;
		}

		public void Dispose()
		{ }

		private IDatList list;
		private int index;
		private IDatValue current;
	}

	public struct DatListValueEnumerable : IEnumerable<IDatValue>
	{
		public DatListValueEnumerable(IDatList list)
		{
			this.list = list;
		}

		public DatListValueEnumerator GetEnumerator()
		{
			return new DatListValueEnumerator(list);
		}

		IEnumerator<IDatValue> IEnumerable<IDatValue>.GetEnumerator()
		{
			return new DatListValueEnumerator(list);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new DatListValueEnumerator(list);
		}

		private IDatList list;
	}
}
