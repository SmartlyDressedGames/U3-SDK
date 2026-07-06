////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum EDatNodeType
	{
		Value,
		Dictionary,
		List,
	}

	public interface IDatNode
	{
		public EDatNodeType NodeType { get; }
		public void DebugDumpToStringBuilder(System.Text.StringBuilder output, int indentationLevel = 0);

		/// <summary>
		/// If true, this node supports metadata like prefix comments, inline comments, and line numbers.
		/// Note: line numbers are intended for analyzing parsed dat files, not setting by code creating dat hierarchies.
		/// </summary>
		public bool IsMetadataAvailable { get; }

		/// <summary>
		/// Comment line(s) immediately proceeding a node are associated with it.
		/// 
		/// Single line comment example associated with a dictionary value:
		///
		///		// This is a value
		///		Key Value
		///
		/// Two line comment example associated with a dictionary value:
		///
		///		// This is a value description
		///		// spread across multiple lines
		///		Key Value
		///
		/// Single line comment example associated with a list value:
		///
		///		List
		///		[
		///			// This is a value
		///			Value
		///		]
		///
		/// Two line comment example associated with a list value:
		///
		///		List
		///		[
		///			// This is a value description
		///			// spread across multiple lines
		///			Value
		///		]
		/// </summary>
		/// <returns>True if parsed comment is available and assigned. (I.e., not empty.)</returns>
		public bool TryGetParsedComment(out DatComment comment);

		/// <summary>
		/// Get parent if available. Depends whether parser had metadata enabled OR was edited.
		/// </summary>
		/// <returns>True if parent is available. Can be null if this is the root dictionary.</returns>
		public bool TryGetParentNode(out IDatNode parentNode);

		/// <summary>
		/// If available, get line number this node was declared on.
		/// For root dictionary this is line 1.
		/// For non-root dictionaries this is the opening '{' line number.
		/// For lists this is the opening '[' line number.
		/// For values this is the first line of text. (I.e., if spanning multiple lines, this is the first line number.)
		/// </summary>
		/// <returns>True if metadata (such as line number) is available.</returns>
		public bool TryGetParsedLineNumber(out int lineNumber);

		/// <summary>
		/// If available, get line numbers this node starts and ends on.
		/// For root dictionary this is the entire file range.
		/// For non-root dictionaries this is the opening '{' to closing '}' (inclusive).
		/// For lists this is the opening '[' to closing ']' (inclusive).
		/// For values this is a single line. (todo? values in quotes can span multiple lines)
		/// </summary>
		public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber);
	}
}
