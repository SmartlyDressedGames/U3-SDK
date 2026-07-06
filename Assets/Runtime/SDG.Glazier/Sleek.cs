////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum ESleekChildLayout
	{
		None,
		Vertical,
		Horizontal,
	}

	public enum ESleekChildPerpendicularAlignment
	{
		Center,
		Top,
		Bottom,
	}

	public interface ISleekElement
	{
		bool IsVisible
		{
			get;
			set;
		}

		ISleekElement Parent { get; }

		ISleekLabel SideLabel
		{
			get;
		}

		float PositionOffset_X
		{
			get;
			set;
		}

		float PositionOffset_Y
		{
			get;
			set;
		}

		float PositionScale_X
		{
			get;
			set;
		}

		float PositionScale_Y
		{
			get;
			set;
		}

		float SizeOffset_X
		{
			get;
			set;
		}

		float SizeOffset_Y
		{
			get;
			set;
		}

		float SizeScale_X
		{
			get;
			set;
		}

		float SizeScale_Y
		{
			get;
			set;
		}

		/// <summary>
		/// Ideally will clean up? This is used by uGUI to properly attach SleekWrapper proxies.
		/// </summary>
		ISleekElement AttachmentRoot
		{
			get;
		}

		bool IsAnimatingTransform
		{
			get;
		}

		/// <summary>
		/// If true, position/size properties are relative to the parent rect.
		/// Otherwise, only SizeOffset_X/Y are supported, and depend on Width/Height layout override.
		/// Defaults to true. Depends on the Glazier's SupportsAutomaticLayout property.
		/// </summary>
		bool UseManualLayout
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to false. If true, SizeOffset_X overrides the auto layout.
		/// </summary>
		bool UseWidthLayoutOverride
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to false. If true, SizeOffset_Y overrides the auto layout.
		/// </summary>
		bool UseHeightLayoutOverride
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to None.
		/// </summary>
		public ESleekChildLayout UseChildAutoLayout
		{
			get;
			set;
		}

		public ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to false. If true, children are expanded along the primary layout axis.
		/// </summary>
		public bool ExpandChildren
		{
			get;
			set;
		}

		/// <summary>
		/// Defaults to false. If true, and contained in an auto-layout, the auto-layout will be bypassed.
		/// </summary>
		public bool IgnoreLayout
		{
			get;
			set;
		}

		/// <summary>
		/// Hack to add padding around main menu news feed contents. (public issue #4261)
		/// Only implemented by uGUI for now.
		/// </summary>
		public float ChildAutoLayoutPadding
		{
			get;
			set;
		}

		/// <summary>
		/// New code should probably not be calling this, instead it should be called by the RemoveChild implementation.
		/// </summary>
		void InternalDestroy();

		void AnimatePositionOffset(float newPositionOffset_X, float newPositionOffset_Y, ESleekLerp lerp, float time);
		void AnimatePositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time);
		void AnimateSizeOffset(float newSizeOffset_X, float newSizeOffset_Y, ESleekLerp lerp, float time);
		void AnimateSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time);

		/// <summary>
		/// Can also be used to change the parent of a child, i.e. if child has a different parent then it will be
		/// re-parented to this widget. Used by the inventory dashboard when switching two-pane mode.
		/// </summary>
		void AddChild(ISleekElement child);

		void AddLabel(string text, ESleekSide side);
		void AddLabel(string text, Color color, ESleekSide side);
		void UpdateLabel(string text);
		int FindIndexOfChild(ISleekElement sleek);

		/// <summary>
		/// Perhaps not ideal, however legacy users were often indexing directly into the children list.
		/// </summary>
		ISleekElement GetChildAtIndex(int index);

		int GetChildCount();

		/// <summary>
		/// Called during LateUpdate so that latest transform values (e.g. tracking a 3D object) are used.
		/// </summary>
		void Update();

		void RemoveChild(ISleekElement child);
		void RemoveAllChildren();

		/// <summary>
		/// Convert normalized viewport position where zero is the bottom left of the screen and one is the top right
		/// into a local normalized position where zero is the top left of this rectangle and one is the bottom right.
		/// </summary>
		Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition);

		/// <summary>
		/// Get cursor position relative to the upper left of this transform, normalized to the transform size.
		/// </summary>
		Vector2 GetNormalizedCursorPosition();

		/// <summary>
		/// Get display size on screen.
		/// Useful to create render texture matching display resolution.
		/// </summary>
		Vector2 GetAbsoluteSize();

		/// <summary>
		/// Move this element in the UI hierarchy such that it is drawn before its siblings.
		/// Useful for automatic layout to bring an element to the top of a list.
		/// </summary>
		void SetAsFirstSibling();

		void ForceLayoutUpdate();
	}
}
