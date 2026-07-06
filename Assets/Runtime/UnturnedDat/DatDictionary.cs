////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public interface IDatDictionary : IDatNode, IEnumerable<KeyValuePair<string, IDatNode>>
	{
		/// <summary>
		/// Number of key-value pairs in the dictionary.
		/// </summary>
		public int Count { get; }

		public bool ContainsKey(string key);

		/// <summary>
		/// Returns true if key exists and node is not null.
		/// </summary>
		public bool TryGetNode(string key, out IDatNode node);

		/// <summary>
		/// If available, get line number key was declared on.
		/// </summary>
		/// <returns>True if metadata (such as line number) is available for specified key.</returns>
		public bool TryGetKeyLineNumber(string key, out int lineNumber);

		IDatNode this[string key]
		{
			get;
		}

		/// <summary>
		/// Get editing interface, if available.
		/// </summary>
		public IEditableDatDictionary Edit();
	}

	public sealed class DatDictionary : Dictionary<string, IDatNode>, IDatDictionary
	{
		public DatDictionary() : base(System.StringComparer.OrdinalIgnoreCase)
		{ }

		public EDatNodeType NodeType => EDatNodeType.Dictionary;

		public bool TryGetNode(string key, out IDatNode node)
		{
			return TryGetValue(key, out node);
		}

		public void DebugDumpToStringBuilder(System.Text.StringBuilder output, int indentationLevel = 0)
		{
			output.Append('{');
			if (TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber))
			{
				output.Append(" (lines ");
				output.Append(startingLineNumber);
				output.Append('-');
				output.Append(endingLineNumber);
				output.Append(')');
			}
			output.AppendLine();

			foreach (KeyValuePair<string, IDatNode> pair in this)
			{
				if (pair.Value.TryGetParsedComment(out DatComment comment))
				{
					comment.DebugDumpToStringBuilder(output, indentationLevel);
				}

				for (int tabIndex = 0; tabIndex < indentationLevel + 1; ++tabIndex)
					output.Append('\t');
				output.Append($"\"{pair.Key}\"");
				output.Append(" = ");

				if (pair.Value != null)
				{
					pair.Value.DebugDumpToStringBuilder(output, indentationLevel + 1);
				}
				else
				{
					output.AppendLine("null");
				}
			}

			for (int tabIndex = 0; tabIndex < indentationLevel; ++tabIndex)
				output.Append('\t');
			output.AppendLine("}");
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

		/// <summary>
		/// If available, get line number key was declared on.
		/// </summary>
		/// <returns>True if metadata (such as line number) is available for specified key.</returns>
		public bool TryGetKeyLineNumber(string key, out int lineNumber)
		{
			lineNumber = 0;
			return false;
		}

		public IEditableDatDictionary Edit()
		{
			return null;
		}
	}
}
