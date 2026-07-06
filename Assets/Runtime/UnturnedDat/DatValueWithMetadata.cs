////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	internal sealed class DatValueWithMetadata : DatNodeWithMetadata<DatValue, EditableDatValue>, IDatValue
	{
		/// <summary>
		/// Line number this value was declared on, or zero if unavailable. (e.g., if created by code)
		/// </summary>
		public int lineNumber;

		/// <summary>
		/// Optional comment after value. Defaults to null. If set, value will written in quotes.
		/// </summary>
		public string inlineComment = null;

		public bool TryGetParsedInlineComment(out string inlineComment)
		{
			inlineComment = this.inlineComment;
			return true;
		}

		public bool TryGetParsedLineNumber(out int lineNumber)
		{
			lineNumber = this.lineNumber;
			return true;
		}

		public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
		{
			startingLineNumber = lineNumber;
			endingLineNumber = lineNumber;
			return true;
		}

		public DatValueWithMetadata(DatValue valueNode, int lineNumber, string inlineComment, DatComment? prefixComment)
			: base(valueNode)
		{
			this.lineNumber = lineNumber;
			this.inlineComment = inlineComment;
			this.prefixComment = prefixComment;
		}

		#region IDatValue
		public string Value
		{
			get => underlyingNode.value;
			set => underlyingNode.value = value;
		}

		public IEditableDatValue Edit()
		{
			if (editable == null)
			{
				editable = new EditableDatValue(this);
			}
			return editable;
		}
		#endregion IDatValue

		#region IMetadataPreservingDatWriterCompatible
		public override string WriterGetInlineComment()
		{
			if (editable != null && editable.hasAssignedInlineComment)
			{
				return editable.inlineComment;
			}
			else
			{
				return inlineComment;
			}
		}
		#endregion IMetadataPreservingDatWriterCompatible
	}
}
