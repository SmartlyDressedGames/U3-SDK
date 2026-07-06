////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public interface IDatValue : IDatNode
	{
		public string Value
		{
			get;
			set;
		}

		public bool TryGetParsedInlineComment(out string inlineComment);

		/// <summary>
		/// Get editing interface, if available.
		/// </summary>
		public IEditableDatValue Edit();
	}

	public sealed class DatValue : IDatValue
	{
		public string value;

		public EDatNodeType NodeType => EDatNodeType.Value;

		public string Value
		{
			get => value;
			set => this.value = value;
		}

		public DatValue()
		{
			value = null;
		}

		public DatValue(string value)
		{
			this.value = value;
		}

		public void DebugDumpToStringBuilder(System.Text.StringBuilder output, int indentationLevel = 0)
		{
			if (Value != null)
			{
				output.Append('"');
				output.Append(Value);
				output.Append('"');
			}
			else
			{
				output.Append("value(null)");
			}

			TryGetParsedLineNumber(out int lineNumber);

			if (TryGetParsedInlineComment(out string inlineComment) && !string.IsNullOrEmpty(inlineComment))
			{
				output.Append(" // ");
				output.Append(inlineComment);
			}

			if (lineNumber > 0)
			{
				output.Append(" (line ");
				output.Append(lineNumber);
				output.Append(')');
			}

			output.AppendLine();
		}

		public bool IsMetadataAvailable => false;

		public bool TryGetParsedComment(out DatComment comment)
		{
			comment = default;
			return false;
		}

		public bool TryGetParsedInlineComment(out string comment)
		{
			comment = null;
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

		public IEditableDatValue Edit()
		{
			return null;
		}

		/// <summary>
		/// CyberAndrii reported an exploit that Type.GetType will try to load requested assemblies
		/// if a file path is provided, and that newer Mono versions prevent path separator characters.
		/// </summary>
		public static readonly char[] INVALID_TYPE_CHARS = new char[]
		{
			'\\',
			':',
			'/',
		};
	}
}
