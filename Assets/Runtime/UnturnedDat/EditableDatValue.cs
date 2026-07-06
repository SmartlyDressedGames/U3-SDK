////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// To be used with MetadataPreservingDatWriter.
	/// </summary>
	public interface IEditableDatValue : IDatValue, IEditableDatNode
	{
		/// <summary>
		/// Optional comment after value. Defaults to null. If set, value will written in quotes.
		/// </summary>
		public string InlineComment
		{
			get;
			set;
		}
	}

	public static class IEditableDatValueEx
	{
		/// <summary>
		/// Enables builder pattern for dat edits.
		/// </summary>
		public static TValueNode SetInlineComment<TValueNode>(this TValueNode valueNode, string inlineComment) where TValueNode : IEditableDatValue
		{
			valueNode.InlineComment = inlineComment;
			return valueNode;
		}
	}

	internal sealed class EditableDatValue : EditableDatNode<IDatValue, DatValue, EditableDatValue>, IEditableDatValue
	{
		public EditableDatValue(IDatValue node)
		{
			wrappedNode = node;
		}

		public string inlineComment;
		public bool hasAssignedInlineComment;
		public string InlineComment
		{
			get => inlineComment;
			set
			{
				inlineComment = value;
				hasAssignedInlineComment = true;
			}
		}

		#region IDatValue
		public string Value
		{
			get => wrappedNode.Value;
			set => wrappedNode.Value = value;
		}

		public bool TryGetParsedInlineComment(out string comment) => wrappedNode.TryGetParsedInlineComment(out comment);
		#endregion IDatValue

		#region IEditableDatValue
		public IEditableDatValue Edit()
		{
			return this;
		}
		#endregion

		#region IMetadataPreservingDatWriterCompatible
		public override string WriterGetInlineComment()
		{
			if (hasAssignedInlineComment)
			{
				return inlineComment;
			}
			else
			{
				wrappedNode.TryGetParsedInlineComment(out string metadataComment);
				return metadataComment;
			}
		}
		#endregion IMetadataPreservingDatWriterCompatible
	}
}
