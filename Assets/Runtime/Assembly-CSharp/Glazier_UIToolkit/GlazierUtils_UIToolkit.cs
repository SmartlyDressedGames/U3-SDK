////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal static class GlazierUtils_UIToolkit
	{
		/// <summary>
		/// By default, clickable only responds to LeftMouse without the Control modifier.
		/// Unturned (currently) filters left/right mouse and modifiers outside Glazier,
		/// so add activators for left/right and control modifier to all clickables.
		/// </summary>
		public static void AddClickableActivators(Clickable clickable)
		{
			clickable.activators.Add(new ManipulatorActivationFilter() { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
			clickable.activators.Add(new ManipulatorActivationFilter() { button = MouseButton.RightMouse });
			clickable.activators.Add(new ManipulatorActivationFilter() { button = MouseButton.RightMouse, modifiers = EventModifiers.Control });
		}

		public static void ApplyTextContrast(IStyle style, ETextContrastContext contrastContext, float alpha)
		{
			ETextContrastStyle contrastStyle = SleekShadowStyle.ContextToStyle(contrastContext);

			switch (contrastStyle)
			{
				default:
				case ETextContrastStyle.None:
					style.textShadow = StyleKeyword.Null;
					style.unityTextOutlineColor = StyleKeyword.Null;
					style.unityTextOutlineWidth = StyleKeyword.Null;
					break;

				case ETextContrastStyle.Outline:
					style.textShadow = new TextShadow() { color = SleekCustomization.shadowColor.WithAlpha(alpha), offset = new Vector2(0.0f, 0.0f), blurRadius = 1.5f };
					style.unityTextOutlineColor = StyleKeyword.Null;
					style.unityTextOutlineWidth = StyleKeyword.Null;
					break;

				case ETextContrastStyle.Shadow:
					style.textShadow = new TextShadow() { color = SleekCustomization.shadowColor.WithAlpha(alpha), offset = new Vector2(0.0f, 1.0f), blurRadius = 1.0f };
					style.unityTextOutlineColor = StyleKeyword.Null;
					style.unityTextOutlineWidth = StyleKeyword.Null;
					break;

				case ETextContrastStyle.Tooltip:
					style.textShadow = new TextShadow() { color = Color.black, offset = new Vector2(0.0f, 1.0f), blurRadius = 2.0f };
					style.unityTextOutlineColor = SleekCustomization.shadowColor.WithAlpha(alpha * 0.25f);
					style.unityTextOutlineWidth = 0.25f;
					break;
			}
		}

		public static StyleLength GetFontSize(ESleekFontSize fontSize)
		{
			switch (fontSize)
			{
				case ESleekFontSize.Tiny:
					return 8;

				case ESleekFontSize.Small:
					return 10;

				// If changing the default font size, please also update the USS font size!
				// The default is applied by the stylesheet rather than inline style.
				default:
				case ESleekFontSize.Default:
					return StyleKeyword.Null; // 12

				case ESleekFontSize.Medium:
					return 14;

				case ESleekFontSize.Large:
					return 20;

				case ESleekFontSize.Title:
					return 50;
			}
		}

		/// <summary>
		/// USS best practices mentions inline styles have a higher memory overhead, so we
		/// only apply an inline value if it doesn't match the default :root font style.
		/// </summary>
		public static StyleEnum<FontStyle> GetFontStyle(FontStyle fontStyle)
		{
			switch (fontStyle)
			{
				case FontStyle.Normal:
					return StyleKeyword.Null;

				default:
					return fontStyle;
			}
		}

		/// <summary>
		/// USS best practices mentions inline styles have a higher memory overhead, so we
		/// only apply an inline value if it doesn't match the default :root text alignment.
		/// </summary>
		public static StyleEnum<TextAnchor> GetTextAlignment(TextAnchor textAlignment)
		{
			switch (textAlignment)
			{
				case TextAnchor.MiddleCenter:
					return StyleKeyword.Null;

				default:
					return textAlignment;
			}
		}
	}
}
