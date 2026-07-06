////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// To be used with MetadataPreservingDatWriter.
	/// </summary>
	public interface IEditableDatList : IDatList, IEditableDatNode
	{
		IEditableDatValue AddValue();
		IEditableDatList AddList();
		IEditableDatDictionary AddDictionary();
		public void RemoveAt(int index);
	}

	internal sealed class EditableDatList : EditableDatNode<IDatList, DatList, EditableDatList>, IEditableDatList
	{
		public EditableDatList(IDatList list)
		{
			wrappedNode = list;
		}

		#region IDatList
		public int Count => wrappedNode.Count;
		public bool TryGetNode(int index, out IDatNode node) => wrappedNode.TryGetNode(index, out node);
		public DatListValueEnumerable GetValues() => wrappedNode.GetValues();
		public int IndexOf(IDatNode node) => wrappedNode.IndexOf(node);
		public IDatNode this[int index]
		{
			get => wrappedNode[index];
		}
		public IEditableDatList Edit() { return this; }
		IEnumerator<IDatNode> IEnumerable<IDatNode>.GetEnumerator() => wrappedNode.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => wrappedNode.GetEnumerator();
		#endregion IDatList

		#region IEditableDatList
		public IEditableDatValue AddValue()
		{
			EditableDatValue edit = new EditableDatValue(new DatValue());
			edit.creationId = ++nodeCreationCounter;
			edit.parentNode = this;
			GetUnderlyingNode().Add(edit);
			return edit;
		}

		public IEditableDatList AddList()
		{
			EditableDatList edit = new EditableDatList(new DatList());
			edit.creationId = ++nodeCreationCounter;
			edit.parentNode = this;
			GetUnderlyingNode().Add(edit);
			return edit;
		}

		public IEditableDatDictionary AddDictionary()
		{
			EditableDatDictionary edit = new EditableDatDictionary(new DatDictionary());
			edit.creationId = ++nodeCreationCounter;
			edit.parentNode = this;
			GetUnderlyingNode().Add(edit);
			return edit;
		}

		public void RemoveAt(int index) => GetUnderlyingNode().RemoveAt(index);
		#endregion IEditableDatList

		#region IMetadataPreservingDatWriterCompatible
		public override string WriterGetInlineComment()
		{
			return null;
		}
		#endregion IMetadataPreservingDatWriterCompatible

		private int nodeCreationCounter;
	}
}
