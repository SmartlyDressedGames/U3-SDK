////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned
{
	internal sealed class DatDictionaryWithMetadata : DatNodeWithMetadata<DatDictionary, EditableDatDictionary>, IDatDictionary
	{
		public DatDictionaryWithMetadata(DatDictionary dictionary)
			: base(dictionary)
		{}

		/// <summary>
		/// Line number the opening '{' bracket was declared on, or zero if unavailable. (e.g., if created by code)
		/// </summary>
		public int openingLineNumber;

		/// <summary>
		/// Line number the closing '}' bracket was declared on, or zero if unavailable. (e.g., if created by code)
		/// </summary>
		public int closingLineNumber;

		#region IDatNode
		public bool TryGetParsedLineNumber(out int lineNumber)
		{
			lineNumber = openingLineNumber;
			return true;
		}

		public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
		{
			startingLineNumber = openingLineNumber;
			endingLineNumber = closingLineNumber;
			return true;
		}
		#endregion IDatNode

		#region IDatDictionary
		public int Count => underlyingNode.Count;
		public bool ContainsKey(string key) => underlyingNode.ContainsKey(key);
		public bool TryGetNode(string key, out IDatNode node) => underlyingNode.TryGetNode(key, out node);
		public bool TryGetKeyLineNumber(string key, out int lineNumber) => underlyingNode.TryGetKeyLineNumber(key, out lineNumber);
		public IDatNode this[string index]
		{
			get => underlyingNode[index];
			set => underlyingNode[index] = value;
		}
		public IEditableDatDictionary Edit()
		{
			if (editable == null)
			{
				editable = new EditableDatDictionary(this);
			}
			return editable;
		}
		IEnumerator<KeyValuePair<string, IDatNode>> IEnumerable<KeyValuePair<string, IDatNode>>.GetEnumerator() => underlyingNode.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => underlyingNode.GetEnumerator();
		#endregion IDatDictionary

		#region IMetadataPreservingDatWriterCompatible
		public override string WriterGetInlineComment()
		{
			return null;
		}
		#endregion IMetadataPreservingDatWriterCompatible
	}
}
