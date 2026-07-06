////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned
{
	internal sealed class DatListWithMetadata : DatNodeWithMetadata<DatList, EditableDatList>, IDatList
	{
		public DatListWithMetadata(DatList node)
			: base(node) { }

		/// <summary>
		/// Line number the opening '[' bracket was declared on, or zero if unavailable. (e.g., if created by code)
		/// </summary>
		public int openingLineNumber;

		/// <summary>
		/// Line number the closing ']' bracket was declared on, or zero if unavailable. (e.g., if created by code)
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

		#region IDatList
		public int Count => underlyingNode.Count;
		public bool TryGetNode(int index, out IDatNode node) => underlyingNode.TryGetNode(index, out node);
		public DatListValueEnumerable GetValues() => underlyingNode.GetValues();
		public int IndexOf(IDatNode node) => underlyingNode.IndexOf(node);
		public IDatNode this[int index]
		{
			get => underlyingNode[index];
			set => underlyingNode[index] = value;
		}
		IEnumerator<IDatNode> IEnumerable<IDatNode>.GetEnumerator() => underlyingNode.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => underlyingNode.GetEnumerator();

		public IEditableDatList Edit()
		{
			if (editable == null)
			{
				editable = new EditableDatList(this);
			}
			return editable;
		}
		#endregion IDatList

		#region IMetadataPreservingDatWriterCompatible
		public override string WriterGetInlineComment()
		{
			return null;
		}
		#endregion IMetadataPreservingDatWriterCompatible
	}
}
