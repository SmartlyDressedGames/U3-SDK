////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	/// <summary>
	/// To be used with MetadataPreservingDatWriter.
	/// </summary>
	public interface IEditableDatNode : IDatNode
	{
		/// <summary>
		/// Optional prefix comment before value. Defaults to null.
		/// </summary>
		public string Comment
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to zero. If set, the writer will attempt to preserve this position.
		/// Useful for maintaining file layout when replacing data.
		/// </summary>
		public int PreferredLineNumber
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to zero. If set, the writer will ensure at least this many blank lines before this node.
		/// Note: there is always zero margin between opening '{' or '[' and first node.
		/// </summary>
		public int TopMargin
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to zero. If set, the writer will ensure at least this many blank lines after this node.
		/// Note: there is always zero margin between closing '}' or ']' and last node.
		/// </summary>
		public int BottomMargin
		{
			get;
			set;
		}

		public enum ESortingPreference
		{
			/// <summary>
			/// This node prefers to go after nodes with the same line number.
			/// </summary>
			TowardBack,

			/// <summary>
			/// This node prefers to go before nodes with the same line number.
			/// </summary>
			TowardFront,
		}

		/// <summary>
		/// Controls how nodes with the same preferred line number are sorted.
		/// </summary>
		public ESortingPreference SortingPreference
		{
			get;
			set;
		}
	}

	public static class IEditableDatNodeEx
	{
		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TNode SetComment<TNode>(this TNode node, string comment) where TNode : IEditableDatNode
		{
			node.Comment = comment;
			return node;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TNode SetMargins<TNode>(this TNode node, int margins) where TNode : IEditableDatNode
		{
			node.TopMargin = margins;
			node.BottomMargin = margins;
			return node;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TNode SetMargins<TNode>(this TNode node, int topMargin, int bottomMargin) where TNode : IEditableDatNode
		{
			node.TopMargin = topMargin;
			node.BottomMargin = bottomMargin;
			return node;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TNode SetTopMargin<TNode>(this TNode node, int topMargin) where TNode : IEditableDatNode
		{
			node.TopMargin = topMargin;
			return node;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TNode SetBottomMargin<TNode>(this TNode node, int bottomMargin) where TNode : IEditableDatNode
		{
			node.BottomMargin = bottomMargin;
			return node;
		}

		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TNode SetSortingPreference<TNode>(this TNode node, IEditableDatNode.ESortingPreference sortingPreference) where TNode : IEditableDatNode
		{
			node.SortingPreference = sortingPreference;
			return node;
		}

		/// <summary>
		/// Helper for merging user comments with code-generated comments.
		/// Used by the new server config file. Moved here for unit testing.
		/// Memory is supplied by calling code to make it thread-safe.
		/// </summary>
		public static TNode MergeGeneratedComment<TNode, TEnumerable>(this TNode node, string prefix, TEnumerable generatedLines, System.Text.StringBuilder stringBuilder, List<string> parsedLines)
			where TNode : IEditableDatNode
			where TEnumerable : IEnumerable<string> // Avoid struct boxing.
		{
			string trimmedPrefix = prefix.TrimEnd(); // If prefix is e.g. "> " we want to treat ">" as generated, too, in case editor has trim trailing whitespace enabled.
			parsedLines.Clear();
			int preGeneratedLineCount = 0;
			if (node.TryGetParsedComment(out DatComment existingComment))
			{
				bool hasFoundGeneratedLines = false;
				foreach (string line in existingComment.MessageLines)
				{
					if (line != null && line.StartsWith(trimmedPrefix))
					{
						if (!hasFoundGeneratedLines)
						{
							hasFoundGeneratedLines = true;
							preGeneratedLineCount = parsedLines.Count;
						}
						continue;
					}

					parsedLines.Add(line);
				}
			}

			stringBuilder.Clear();
			bool isFirstLine = true;
			for (int index = 0; index < preGeneratedLineCount; ++index)
			{
				if (!isFirstLine)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(parsedLines[index]);
				isFirstLine = false;
			}
			if (generatedLines != null)
			{
				foreach (string line in generatedLines)
				{
					if (!isFirstLine)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(prefix);
					stringBuilder.Append(line);
					isFirstLine = false;
				}
			}
			for (int index = preGeneratedLineCount; index < parsedLines.Count; ++index)
			{
				if (!isFirstLine)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(parsedLines[index]);
				isFirstLine = false;
			}

			return node.SetComment(stringBuilder.ToString());
		}

		public static TNode MergeGeneratedCommentAlloc<TNode, TEnumerable>(this TNode node, string prefix, TEnumerable generatedLines)
			where TNode : IEditableDatNode
			where TEnumerable : IEnumerable<string> // Avoid struct boxing.
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			List<string> parsedLines = new List<string>();
			return MergeGeneratedComment(node, prefix, generatedLines, sb, parsedLines);
		}
	}

	internal abstract class EditableDatNodeBase
	{
		/// <summary>
		/// Creation ID within container for sorting.
		/// </summary>
		public int creationId;

		/// <summary>
		/// Only set for *added* nodes. If this edit wraps a WithMetadata class it uses that parent.
		/// </summary>
		public IDatNode parentNode;

		public string comment;
		public bool hasAssignedComment;
		public string Comment
		{
			get => comment;
			set
			{
				comment = value;
				hasAssignedComment = true;
			}
		}

		public int TopMargin
		{
			get;
			set;
		}

		public int BottomMargin
		{
			get;
			set;
		}

		public IEditableDatNode.ESortingPreference SortingPreference
		{
			get;
			set;
		} = IEditableDatNode.ESortingPreference.TowardBack;
	}

	internal abstract class EditableDatNode<TInterface, TNode, TEditable> : EditableDatNodeBase, IMetadataPreservingDatWriterCompatible
		where TInterface : IDatNode
		where TNode : TInterface
		where TEditable : EditableDatNodeBase
	{
		/// <summary>
		/// Can be a "with metadata" type if editing an existing value, or a regular type if adding new data.
		/// </summary>
		public TInterface wrappedNode;

		public int PreferredLineNumber
		{
			get;
			set;
		}

		public TNode GetUnderlyingNode()
		{
			if (wrappedNode is DatNodeWithMetadata<TNode, TEditable> metadataNode)
			{
				return metadataNode.underlyingNode;
			}
			else
			{
				return (TNode) wrappedNode;
			}
		}

		#region IDatNode
		public EDatNodeType NodeType => wrappedNode.NodeType;

		public void DebugDumpToStringBuilder(System.Text.StringBuilder output, int indentationLevel = 0)
			=> wrappedNode.DebugDumpToStringBuilder(output, indentationLevel);

		public bool IsMetadataAvailable => wrappedNode.IsMetadataAvailable;

		public bool TryGetParentNode(out IDatNode parentNode)
		{
			if (wrappedNode.IsMetadataAvailable)
			{
				return wrappedNode.TryGetParentNode(out parentNode);
			}
			else
			{
				parentNode = this.parentNode;
				return true;
			}
		}

		public bool TryGetParsedComment(out DatComment comment) => wrappedNode.TryGetParsedComment(out comment);

		public bool TryGetParsedLineNumber(out int lineNumber) => wrappedNode.TryGetParsedLineNumber(out lineNumber);

		public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
			=> wrappedNode.TryGetParsedLineNumberRange(out startingLineNumber, out endingLineNumber);
		#endregion IDatNode

		#region IMetadataPreservingDatWriterCompatible
		public DatComment? WriterGetPrefixComment()
		{
			if (hasAssignedComment)
			{
				return new DatComment(comment);
			}
			else
			{
				return wrappedNode.TryGetParsedComment(out DatComment result) ? result : null;
			}
		}

		public abstract string WriterGetInlineComment();

		public int WriterGetEarliestLineNumber()
		{
			if (PreferredLineNumber > 0)
			{
				return PreferredLineNumber;
			}
			else
			{
				wrappedNode.TryGetParsedLineNumber(out int earliestLineNumber);
				if (NodeType != EDatNodeType.Value && TryGetParentNode(out IDatNode underlyingParent) && underlyingParent != null
					&& underlyingParent.NodeType == EDatNodeType.Dictionary)
				{
					--earliestLineNumber;
				}
				return earliestLineNumber;
			}
		}

		public int WriterGetLatestLineNumber()
		{
			wrappedNode.TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber);
			return endingLineNumber;
		}

		public void WriterGetSortingParameters(out int lineNumber, out int sortOrder)
		{
			lineNumber = WriterGetEarliestLineNumber();
			sortOrder = SortingPreference == IEditableDatNode.ESortingPreference.TowardBack ? creationId : -creationId;
		}

		public void WriterGetMargins(out int topMargin, out int bottomMargin)
		{
			topMargin = TopMargin;
			bottomMargin = BottomMargin;
		}
		#endregion IMetadataPreservingDatWriterCompatible
	}
}
