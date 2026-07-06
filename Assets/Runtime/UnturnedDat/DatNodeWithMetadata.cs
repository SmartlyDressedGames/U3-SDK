////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal abstract class DatNodeWithMetadataBase
	{
		public IDatNode parentNode;
	}

	internal abstract class DatNodeWithMetadata<TNode, TEditable> : DatNodeWithMetadataBase, IMetadataPreservingDatWriterCompatible
		where TNode : IDatNode
		where TEditable : EditableDatNodeBase
	{
		public TNode underlyingNode;
		public TEditable editable;
		public DatComment? prefixComment;

		public DatNodeWithMetadata(TNode underlyingNode)
		{
			this.underlyingNode = underlyingNode;
		}

		#region IDatNode
		public EDatNodeType NodeType => underlyingNode.NodeType;

		public void DebugDumpToStringBuilder(System.Text.StringBuilder output, int indentationLevel = 0)
			=> underlyingNode.DebugDumpToStringBuilder(output, indentationLevel);

		public bool IsMetadataAvailable => true;

		public bool TryGetParentNode(out IDatNode parentNode)
		{
			parentNode = this.parentNode;
			return true;
		}

		public bool TryGetParsedComment(out DatComment comment)
		{
			if (prefixComment.HasValue)
			{
				comment = prefixComment.Value;
				return true;
			}
			else
			{
				comment = default;
				return false;
			}
		}
		#endregion IDatNode

		#region IMetadataPreservingDatWriterCompatible
		public DatComment? WriterGetPrefixComment()
		{
			if (editable != null && editable.hasAssignedComment)
			{
				return new DatComment(editable.Comment);
			}
			else
			{
				return prefixComment;
			}
		}

		public abstract string WriterGetInlineComment();

		public int WriterGetEarliestLineNumber()
		{
			if (prefixComment.HasValue && prefixComment.Value.StartingLineNumber > 0)
			{
				return prefixComment.Value.StartingLineNumber;
			}
			else
			{
				((IDatNode) this).TryGetParsedLineNumber(out int earliestLineNumber);
				if (NodeType != EDatNodeType.Value && parentNode != null && parentNode.NodeType == EDatNodeType.Dictionary)
				{
					--earliestLineNumber;
				}
				return earliestLineNumber;
			}
		}

		public int WriterGetLatestLineNumber()
		{
			((IDatNode) this).TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber);
			return endingLineNumber;
		}

		public void WriterGetSortingParameters(out int lineNumber, out int sortOrder)
		{
			lineNumber = WriterGetEarliestLineNumber();
			sortOrder = 0;
		}

		public void WriterGetMargins(out int topMargin, out int bottomMargin)
		{
			if (editable != null)
			{
				topMargin = editable.TopMargin;
				bottomMargin = editable.BottomMargin;
			}
			else
			{
				topMargin = 0;
				bottomMargin = 0;
			}
		}
		#endregion IMetadataPreservingDatWriterCompatible
	}
}
