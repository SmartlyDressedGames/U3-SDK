////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Extensions to the built-in Input class.
	/// </summary>
	public static class InputEx
	{
		/// <summary>
		/// Wrapper for Input.GetKey, but returns false while typing in a uGUI text field.
		/// </summary>
		public static bool GetKey(KeyCode key)
		{
			return Input.GetKey(key) && !MenuConfigurationControlsUI.ShouldGameIgnoreInput && Glazier.Get().ShouldGameProcessKeyDown;
		}

		/// <summary>
		/// Wrapper for Input.GetKeyDown, but returns false while typing in a uGUI text field.
		/// </summary>
		public static bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key) && !MenuConfigurationControlsUI.ShouldGameIgnoreInput && Glazier.Get().ShouldGameProcessKeyDown;
		}

		/// <summary>
		/// Wrapper for Input.GetKeyUp, but returns false while typing in a uGUI text field.
		/// </summary>
		public static bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key) && !MenuConfigurationControlsUI.ShouldGameIgnoreInput && Glazier.Get().ShouldGameProcessKeyDown;
		}

		/// <summary>
		/// Should be used anywhere that Input.GetKeyDown opens a UI.
		///
		/// Each frame one input event can be consumed. This is a hack to prevent multiple UI-related key presses from
		/// interfering during the same frame. Only the first input event proceeds, while the others are ignored.
		/// </summary>
		/// <returns>True if caller should proceed, false otherwise.</returns>
		public static bool ConsumeKeyDown(KeyCode key)
		{
			return GetKeyDown(key) && keyGuard.Consume();
		}

		/// <summary>
		/// Get mouse position in viewport coordinates where zero is the bottom left and one is the top right.
		/// </summary>
		public static Vector2 NormalizedMousePosition
		{
			get
			{
				Vector2 mousePosition = Input.mousePosition;
				mousePosition.x /= Screen.width;
				mousePosition.y /= Screen.height;
				return mousePosition;
			}
		}

		private static OncePerFrameGuard keyGuard;
	}
}
