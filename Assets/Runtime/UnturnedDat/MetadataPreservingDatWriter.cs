////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal interface IMetadataPreservingDatWriterCompatible
	{
		EDatNodeType NodeType { get; }
		DatComment? WriterGetPrefixComment();
		/// <summary>
		/// Only called for value nodes.
		/// </summary>
		string WriterGetInlineComment();
		/// <summary>
		/// Get line number earliest in the file associated with this node if available, otherwise zero.
		/// 
		/// For example, prefix comment line number is earlier in the file than node declaration, and key line number
		/// in dictionary is earlier in the file than list or dictionary contents.
		/// </summary>
		int WriterGetEarliestLineNumber();
		/// <summary>
		/// Get ending line number in file if available, otherwise zero.
		/// </summary>
		int WriterGetLatestLineNumber();
		/// <param name="lineNumber">Same as earliest line number.</param>
		/// <param name="sortOrder">For comparison when line numbers are the same.</param>
		void WriterGetSortingParameters(out int lineNumber, out int sortOrder);
		void WriterGetMargins(out int upperMargin, out int lowerMargin);
	}

	/// <summary>
	/// Uses a DatWriter to write parsed dat back to file, preserving formatting as much as possible.
	/// </summary>
	public sealed class MetadataPreservingDatWriter
	{
		public static IEditableDatDictionary CreateRoot()
		{
			return new EditableDatDictionary(new DatDictionary());
		}

		public void WriteRootDictionary(IDatDictionary rootDictionary, DatWriter writer)
		{
			if (rootDictionary == null)
			{
				throw new System.ArgumentNullException(nameof(rootDictionary));
			}

			if (writer == null)
			{
				throw new System.ArgumentNullException(nameof(writer));
			}

			if (!(rootDictionary is IMetadataPreservingDatWriterCompatible))
			{
				throw new System.ArgumentException("not compatible", nameof(rootDictionary));
			}

			output = writer;
			WriteDictionary(rootDictionary);
		}

		private void WriteDictionary(IDatDictionary dictionary)
		{
			List<KeyValuePair<string, IDatNode>> sortedDictionary;
			if (kvpPool.TryPop(out sortedDictionary))
			{
				sortedDictionary.Clear();
				if (dictionary.Count > sortedDictionary.Capacity)
				{
					sortedDictionary.Capacity = dictionary.Count;
				}
			}
			else
			{
				sortedDictionary = new List<KeyValuePair<string, IDatNode>>(dictionary.Count);
			}
			sortedDictionary.AddRange(dictionary);
			sortedDictionary.Sort(DictionaryLineNumberComparer);

			int previousElementFinalLineNumber = 0;
			int previousElementMargin = 0;
			bool isFirstNode = true;
			foreach (KeyValuePair<string, IDatNode> kvp in sortedDictionary)
			{
				IMetadataPreservingDatWriterCompatible node = (IMetadataPreservingDatWriterCompatible) kvp.Value;
				WriteSpacingAndPrefixComment(node, !isFirstNode, ref previousElementFinalLineNumber, ref previousElementMargin);
				isFirstNode = false;

				switch (node.NodeType)
				{
					case EDatNodeType.Value:
					{
						IDatValue valueNode = (IDatValue) node;
						output.WriteKeyValue(kvp.Key, valueNode.Value, node.WriterGetInlineComment());
						break;
					}

					case EDatNodeType.Dictionary:
					{
						IDatDictionary childDictionary = (IDatDictionary) node;
						output.WriteDictionaryStart(kvp.Key);
						WriteDictionary(childDictionary);
						output.WriteDictionaryEnd();
						break;
					}

					case EDatNodeType.List:
					{
						IDatList childList = (IDatList) node;
						output.WriteListStart(kvp.Key);
						WriteList(childList);
						output.WriteListEnd();
						break;
					}
				}
			}

			kvpPool.Push(sortedDictionary);
		}

		private void WriteList(IDatList list)
		{
			List<IDatNode> sortedList;
			if (listPool.TryPop(out sortedList))
			{
				sortedList.Clear();
				if (list.Count > sortedList.Capacity)
				{
					sortedList.Capacity = list.Count;
				}
			}
			else
			{
				sortedList = new List<IDatNode>(list.Count);
			}
			sortedList.AddRange(list);
			sortedList.Sort(ListLineNumberComparer);

			int previousElementFinalLineNumber = 0;
			int previousElementMargin = 0;
			bool isFirstNode = true;
			foreach (IDatNode node2 in sortedList)
			{
				IMetadataPreservingDatWriterCompatible node = (IMetadataPreservingDatWriterCompatible) node2;
				WriteSpacingAndPrefixComment(node, !isFirstNode, ref previousElementFinalLineNumber, ref previousElementMargin);
				isFirstNode = false;

				switch (node.NodeType)
				{
					case EDatNodeType.Value:
					{
						IDatValue valueNode = (IDatValue) node;
						output.WriteValue(valueNode.Value, node.WriterGetInlineComment());
						break;
					}

					case EDatNodeType.Dictionary:
					{
						IDatDictionary childDictionary = (IDatDictionary) node;
						output.WriteDictionaryStart();
						WriteDictionary(childDictionary);
						output.WriteDictionaryEnd();
						break;
					}

					case EDatNodeType.List:
					{
						IDatList childList = (IDatList) node;
						output.WriteListStart();
						WriteList(childList);
						output.WriteListEnd();
						break;
					}
				}
			}

			listPool.Push(sortedList);
		}

		private void WriteSpacingAndPrefixComment(IMetadataPreservingDatWriterCompatible node,
			bool allowSpacing,
			ref int previousElementLatestLineNumber,
			ref int previousElementMargin)
		{
			int earliestLineNumber = node.WriterGetEarliestLineNumber();
			int latestLineNumber = node.WriterGetLatestLineNumber();
			node.WriterGetMargins(out int upperMargin, out int lowerMargin);

			int emptyLines = 0;
			if (earliestLineNumber > 0 && previousElementLatestLineNumber > 0)
			{
				emptyLines = earliestLineNumber - previousElementLatestLineNumber - 1;
			}
			int margin = Mathf.Max(previousElementMargin, upperMargin);
			emptyLines = Mathf.Max(emptyLines, margin);
			previousElementLatestLineNumber = latestLineNumber;
			previousElementMargin = lowerMargin;

			if (allowSpacing)
			{
				while (emptyLines > 0)
				{
					output.WriteEmptyLine();
					--emptyLines;
				}
			}

			DatComment? comment = node.WriterGetPrefixComment();
			if (comment.HasValue && !comment.Value.AreMessageLinesNullOrEmpty)
			{
				foreach (string line in comment.Value.MessageLines)
				{
					output.WriteComment(line);
				}
			}
		}

		private int DictionaryLineNumberComparer(KeyValuePair<string, IDatNode> lhs, KeyValuePair<string, IDatNode> rhs)
		{
			return ListLineNumberComparer(lhs.Value, rhs.Value);
		}

		private int ListLineNumberComparer(IDatNode baseLhs, IDatNode baseRhs)
		{
			IMetadataPreservingDatWriterCompatible lhs = (IMetadataPreservingDatWriterCompatible) baseLhs;
			IMetadataPreservingDatWriterCompatible rhs = (IMetadataPreservingDatWriterCompatible) baseRhs;
			lhs.WriterGetSortingParameters(out int lhsLineNumber, out int lhsSortOrder);
			rhs.WriterGetSortingParameters(out int rhsLineNumber, out int rhsSortOrder);
			
			// Simplest case: both have different line numbers from parsing.
			if (lhsLineNumber > 0 && rhsLineNumber > 0 && lhsLineNumber != rhsLineNumber)
			{
				return lhsLineNumber.CompareTo(rhsLineNumber);
			}

			// Sort order for parsed items is zero and defaults to 1+ for edited items.
			// This ensures edited items sort after parsed items by default.
			return lhsSortOrder.CompareTo(rhsSortOrder);
		}

		private Stack<List<KeyValuePair<string, IDatNode>>> kvpPool = new Stack<List<KeyValuePair<string, IDatNode>>>();
		private Stack<List<IDatNode>> listPool = new Stack<List<IDatNode>>();
		private DatWriter output;
	}
}
