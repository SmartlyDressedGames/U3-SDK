////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum ESleekScrollbarVisibility
	{
		Default, // todo
		Hidden, // Not visible, regardless of content size.
	}

	public interface ISleekScrollView : ISleekElement
	{
		/// <summary>
		/// Should content size match width of this element?
		/// </summary>
		bool ScaleContentToWidth
		{
			get;
			set;
		}

		/// <summary>
		/// Should content size match height of this element?
		/// </summary>
		bool ScaleContentToHeight
		{
			get;
			set;
		}

		/// <summary>
		/// Used by map view.
		/// </summary>
		float ContentScaleFactor
		{
			get;
			set;
		}

		/// <summary>
		/// Used by the server list and item store to keep columns aligned.
		/// </summary>
		bool ReduceWidthWhenScrollbarVisible
		{
			get;
			set;
		}

		/// <summary>
		/// Used by chat to hide vertical scrollbar while history is closed.
		/// </summary>
		ESleekScrollbarVisibility VerticalScrollbarVisibility
		{
			get;
			set;
		}

		/// <summary>
		/// Content size in pixels.
		/// </summary>
		Vector2 ContentSizeOffset
		{
			get;
			set;
		}

		/// <summary>
		/// Only supported by uGUI at the moment. Used by map menu to preserve zoom position.
		/// </summary>
		Vector2 NormalizedStateCenter
		{
			get;
			set;
		}

		/// <summary>
		/// Scroll wheel is disabled for map menu and SleekItems.
		/// </summary>
		bool HandleScrollWheel
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
		/// Handle color. Not supported by IMGUI.
		/// </summary>
		SleekColor ForegroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Invoked when scroll position changes. Value is normalized.
		/// </summary>
		event System.Action<Vector2> OnNormalizedValueChanged;

		/// <summary>
		/// Get normalized position of vertical scrollbar.
		/// </summary>
		float NormalizedVerticalPosition
		{ get; }

		/// <summary>
		/// Get viewport height as a percentage of content height.
		/// </summary>
		float NormalizedViewportHeight
		{ get; }

		/// <summary>
		/// Defaults to true. Depends on the Glazier's SupportsAutomaticLayout property.
		/// </summary>
		bool ContentUseManualLayout
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to false. If true, when content is smaller than viewport, it is aligned to bottom rather than top.
		/// Used by chat to build upward from the text field.
		/// </summary>
		public bool AlignContentToBottom
		{
			get;
			set;
		}

		/// <summary>
		/// When false the mouse can click content behind the scroll view's content.
		/// Defaults to true. Making it a raycast target is useful for clicking and dragging the content itself.
		/// </summary>
		bool IsRaycastTarget
		{
			get;
			set;
		}

		void ScrollToTop();
		void ScrollToBottom();
	}
}
