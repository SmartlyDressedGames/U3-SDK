////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void Escaped(ISleekField field);
	public delegate void Entered(ISleekField field);
	public delegate void Typed(ISleekField field, string text);

	public interface ISleekField : ISleekElement, ISleekLabel, ISleekWithTooltip
	{
		/// <summary>
		/// Invoked after return key is pressed while typing.
		/// </summary>
		event Entered OnTextSubmitted;

		/// <summary>
		/// Invoked after text changes.
		/// </summary>
		event Typed OnTextChanged;

		/// <summary>
		/// Invoked after escape key is pressed while typing.
		/// Hack for IMGUI chat.
		/// </summary>
		event Escaped OnTextEscaped;

		/// <summary>
		/// If true, visual representation of text is replaced with asterisks.
		/// </summary>
		bool IsPasswordField
		{
			get;
			set;
		}

		string PlaceholderText
		{
			get;
			set;
		}

		bool IsMultiline
		{
			get;
			set;
		}

		int MaxLength
		{
			get;
			set;
		}

		SleekColor BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// When false the field is disabled and greyed out.
		/// </summary>
		bool IsClickable
		{
			get;
			set;
		}

		/// <summary>
		/// Set input focus to this field.
		/// </summary>
		void FocusControl();

		/// <summary>
		/// If this field has input focus, cancel.
		/// </summary>
		void ClearFocus();
	}
}
