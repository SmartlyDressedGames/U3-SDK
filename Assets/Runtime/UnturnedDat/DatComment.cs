////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Represents a prefix comment associated with a node.
	/// </summary>
	public struct DatComment
	{
		/// <summary>
		/// Comment text. Shouldn't be null or empty if this comment was associated with a node.
		/// </summary>
		public string[] MessageLines
		{
			get;
			set;
		}

		/// <summary>
		/// First line starting with //. Same as endingLineNumber if this is a single line comment.
		/// </summary>
		public int StartingLineNumber
		{
			get;
			set;
		}

		/// <summary>
		/// Last line starting with //. Same as startingLineNumber if this is a single line comment.
		/// </summary>
		public int EndingLineNumber
		{
			get;
			set;
		}

		/// <summary>
		/// Shouldn't be the case for parsed comments.
		/// </summary>
		public bool AreMessageLinesNullOrEmpty
		{
			get
			{
				if (MessageLines == null || MessageLines.Length < 1)
				{
					return true;
				}

				if (MessageLines.Length == 1)
				{
					return string.IsNullOrEmpty(MessageLines[0]);
				}
				else
				{
					return false;
				}
			}
		}

		public string JoinLines(char separator)
		{
			if (MessageLines == null || MessageLines.Length < 1)
			{
				return null;
			}

			if (MessageLines.Length == 1)
			{
				return MessageLines[0];
			}

			return string.Join(separator, MessageLines);
		}

		public string JoinLines(string separator)
		{
			if (MessageLines == null || MessageLines.Length < 1)
			{
				return null;
			}

			if (MessageLines.Length == 1)
			{
				return MessageLines[0];
			}

			return string.Join(separator, MessageLines);
		}

		/// <summary>
		/// Formats multi-line string with '\n' line breaks.
		/// </summary>
		public string MessageWithLineBreaks
		{
			get
			{
				return JoinLines('\n');
			}

			set
			{
				if (string.IsNullOrEmpty(value))
				{
					MessageLines = null;
					return;
				}

				MessageLines = value.Split(messageLineBreaks, System.StringSplitOptions.None);
			}
		}
		private static string[] messageLineBreaks = new string[2] { "\r\n", "\n" };

		/// <summary>
		/// Indents each line and adds // before message text.
		/// </summary>
		public void DebugDumpToStringBuilder(System.Text.StringBuilder output, int indentationLevel = 0)
		{
			if (MessageLines == null || MessageLines.Length < 1)
			{
				return;
			}

			foreach (string line in MessageLines)
			{
				for (int tabIndex = 0; tabIndex < indentationLevel + 1; ++tabIndex)
					output.Append('\t');

				if (!string.IsNullOrEmpty(line))
				{
					output.Append("// ");
					output.AppendLine(line);
				}
				else
				{
					output.AppendLine("//");
				}
			}
		}

		public override string ToString()
		{
			if (StartingLineNumber == EndingLineNumber)
			{
				return $"(Line: {StartingLineNumber} Message: \"{MessageWithLineBreaks}\")";
			}
			else
			{
				return $"(Lines: {StartingLineNumber}-{EndingLineNumber} Message: \"{MessageWithLineBreaks}\")";
			}
		}

		public DatComment(string message)
		{
			MessageLines = null;
			StartingLineNumber = 0;
			EndingLineNumber = 0;
			MessageWithLineBreaks = message;
		}
	}
}
