////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// To be used with MetadataPreservingDatWriter.
	/// </summary>
	public interface IEditableDatDictionary : IDatDictionary, IEditableDatNode
	{
		public IEditableDatValue AddValue(string key);
		public IEditableDatList AddList(string key);
		public IEditableDatDictionary AddDictionary(string key);
		public IEditableDatValue ReplaceWithValue(string key);
		public IEditableDatList ReplaceWithList(string key);
		public IEditableDatDictionary ReplaceWithDictionary(string key);
		public bool Remove(string key);
		public bool Remove(string key, out IDatNode node);
	}

	internal sealed class EditableDatDictionary : EditableDatNode<IDatDictionary, DatDictionary, EditableDatDictionary>, IEditableDatDictionary
	{
		public EditableDatDictionary(IDatDictionary node)
		{
			wrappedNode = node;
		}

		#region IDatDictionary
		public int Count => wrappedNode.Count;
		public bool ContainsKey(string key) => wrappedNode.ContainsKey(key);
		public bool TryGetNode(string key, out IDatNode node) => wrappedNode.TryGetNode(key, out node);
		public bool TryGetKeyLineNumber(string key, out int lineNumber) => wrappedNode.TryGetKeyLineNumber(key, out lineNumber);
		public IDatNode this[string index]
		{
			get => wrappedNode[index];
		}
		public IEditableDatDictionary Edit() { return this; }
		IEnumerator<KeyValuePair<string, IDatNode>> IEnumerable<KeyValuePair<string, IDatNode>>.GetEnumerator() => wrappedNode.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => wrappedNode.GetEnumerator();
		#endregion IDatDictionary
		#region IEditableDatDictionary
		public IEditableDatValue AddValue(string key)
		{
			EditableDatValue edit = new EditableDatValue(new DatValue());
			edit.creationId = ++nodeCreationCounter;
			edit.parentNode = this;
			GetUnderlyingNode().Add(key, edit);
			return edit;
		}

		public IEditableDatList AddList(string key)
		{
			EditableDatList edit = new EditableDatList(new DatList());
			edit.creationId = ++nodeCreationCounter;
			edit.parentNode = this;
			GetUnderlyingNode().Add(key, edit);
			return edit;
		}

		public IEditableDatDictionary AddDictionary(string key)
		{
			EditableDatDictionary edit = new EditableDatDictionary(new DatDictionary());
			edit.creationId = ++nodeCreationCounter;
			edit.parentNode = this;
			GetUnderlyingNode().Add(key, edit);
			return edit;
		}

		public IEditableDatValue ReplaceWithValue(string key)
		{
			bool removed = Remove(key, out IDatNode oldNode);
			IEditableDatValue value = AddValue(key);
			if (removed && oldNode != null && oldNode.TryGetParsedLineNumber(out int oldLineNumber))
			{
				value.PreferredLineNumber = oldLineNumber;
			}
			return value;
		}

		public IEditableDatList ReplaceWithList(string key)
		{
			bool removed = Remove(key, out IDatNode oldNode);
			IEditableDatList list = AddList(key);
			if (removed && oldNode != null && oldNode.TryGetParsedLineNumber(out int oldLineNumber))
			{
				list.PreferredLineNumber = oldLineNumber;
			}
			return list;
		}

		public IEditableDatDictionary ReplaceWithDictionary(string key)
		{
			bool removed = Remove(key, out IDatNode oldNode);
			IEditableDatDictionary dictionary = AddDictionary(key);
			if (removed && oldNode != null && oldNode.TryGetParsedLineNumber(out int oldLineNumber))
			{
				dictionary.PreferredLineNumber = oldLineNumber;
			}
			return dictionary;
		}

		public bool Remove(string key) => GetUnderlyingNode().Remove(key);
		public bool Remove(string key, out IDatNode node) => GetUnderlyingNode().Remove(key, out node);
		#endregion IEditableDatDictionary

		#region IMetadataPreservingDatWriterCompatible
		public override string WriterGetInlineComment()
		{
			return null;
		}
		#endregion IMetadataPreservingDatWriterCompatible

		private int nodeCreationCounter;
	}
}
