////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Allows underlying UI system (IMGUI, uGUI, UIElements) to be transparently swapped out.
	/// </summary>
	public interface IGlazier
	{
		ISleekBox CreateBox();
		ISleekButton CreateButton();
		ISleekElement CreateFrame();
		ISleekConstraintFrame CreateConstraintFrame();
		ISleekImage CreateImage();
		ISleekImage CreateImage(Texture texture);
		ISleekSprite CreateSprite();
		ISleekSprite CreateSprite(Sprite sprite);
		ISleekLabel CreateLabel();
		ISleekScrollView CreateScrollView();
		ISleekSlider CreateSlider();
		ISleekField CreateStringField();
		ISleekToggle CreateToggle();

		ISleekUInt8Field CreateUInt8Field();
		ISleekUInt16Field CreateUInt16Field();
		ISleekUInt32Field CreateUInt32Field();
		ISleekInt32Field CreateInt32Field();
		ISleekFloat32Field CreateFloat32Field();
		ISleekFloat64Field CreateFloat64Field();

		/// <summary>
		/// Used by SleekWrapper to create a frame with debug info matching the subclass type.
		/// </summary>
		ISleekProxyImplementation CreateProxyImplementation(SleekWrapper owner);

		/// <summary>
		/// All visible elements should be children of root.
		/// </summary>
		SleekWindow Root { get; set; }

		/// <summary>
		/// Wraps code that previously checked GUIUtility.hotControl == 0.
		/// </summary>
		bool ShouldGameProcessInput { get; }

		/// <summary>
		/// IMGUI prevented Input.GetKeyDown/Up from being called while a text field was active, whereas uGUI unfortunately
		/// does not prevent that. Any hotkeys that might be used while typing should check this.
		/// </summary>
		bool ShouldGameProcessKeyDown { get; }

		/// <summary>
		/// Can elements be layered on top of each other with focus routed to the topmost layer?
		/// False for IMGUI, true for uGUI and UIToolkit.
		/// </summary>
		bool SupportsDepth { get; }

		/// <summary>
		/// Is a rich text label with color tags multiplied by the label's color?
		/// False for IMGUI, true for uGUI and UIToolkit.
		/// </summary>
		bool SupportsRichTextAlpha { get; }

		/// <summary>
		/// If true, CreateVerticalLayout can be used.
		/// </summary>
		bool SupportsAutomaticLayout { get; }

		/// <summary>
		/// If true, ESleekSpriteType works properly. Added because UITK doesn't (as of 2024-12-12) have a good way to
		/// support tiling sprites, so we need to turn off news feed background. (public issue #4800)
		/// </summary>
		bool SupportsTilingSprite { get; }
	}

	public static class Glazier
	{
		public static IGlazier Get()
		{
			return instance;
		}

		public static IGlazier instance;
	}
}
