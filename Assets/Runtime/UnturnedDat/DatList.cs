////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public interface IDatList : IDatNode, IEnumerable<IDatNode>
	{
		/// <summary>
		/// Number of nodes in this list.
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// Returns true if index is within bounds and node is not null.
		/// </summary>
		public bool TryGetNode(int index, out IDatNode node);

		/// <summary>
		/// Enumerate IDatValue nodes in this list.
		/// </summary>
		public DatListValueEnumerable GetValues();

		int IndexOf(IDatNode node);

		IDatNode this[int index]
		{
			get;
		}

		/// <summary>
		/// Get editing interface, if available.
		/// </summary>
		public IEditableDatList Edit();
	}

	public sealed class DatList : List<IDatNode>, IDatList
	{
		public DatList() : base()
		{ }

		public EDatNodeType NodeType => EDatNodeType.List;

		public bool TryGetNode(int index, out IDatNode node)
		{
			node = index >= 0 && index < Count ? this[index] : null;
			return node != null;
		}

		public DatListValueEnumerable GetValues()
		{
			return new DatListValueEnumerable(this);
		}

		public void DebugDumpToStringBuilder(System.Text.StringBuilder output, int indentationLevel = 0)
		{
			output.Append('[');
			if (TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber))
			{
				output.Append(" (lines ");
				output.Append(startingLineNumber);
				output.Append('-');
				output.Append(endingLineNumber);
				output.Append(')');
			}
			output.AppendLine();

			for (int index = 0; index < Count; ++index)
			{
				IDatNode item = this[index];

				if (item.TryGetParsedComment(out DatComment comment))
				{
					comment.DebugDumpToStringBuilder(output, indentationLevel);
				}

				for (int tabIndex = 0; tabIndex < indentationLevel + 1; ++tabIndex)
					output.Append('\t');
				output.Append(index);
				output.Append(" = ");

				if (item != null)
				{
					item.DebugDumpToStringBuilder(output, indentationLevel + 1);
				}
				else
				{
					output.AppendLine("null");
				}
			}

			for (int tabIndex = 0; tabIndex < indentationLevel; ++tabIndex)
				output.Append('\t');
			output.AppendLine("]");
		}

		public bool IsMetadataAvailable => false;

		public bool TryGetParsedComment(out DatComment comment)
		{
			comment = default;
			return false;
		}

		public bool TryGetParentNode(out IDatNode parentNode)
		{
			parentNode = null;
			return false;
		}

		public bool TryGetParsedLineNumber(out int lineNumber)
		{
			lineNumber = 0;
			return false;
		}

		public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
		{
			startingLineNumber = 0;
			endingLineNumber = 0;
			return false;
		}

		public IEditableDatList Edit()
		{
			return null;
		}
	}
}
