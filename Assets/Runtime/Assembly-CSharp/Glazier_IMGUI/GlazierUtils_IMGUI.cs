////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal static class GlazierUtils_IMGUI
	{
		public static bool allowInput = true;

		public static int getScaledFontSize(int originalFontSize)
		{
			return Mathf.CeilToInt(originalFontSize * GraphicsSettings.userInterfaceScale);
		}

		public static void drawAngledImageTexture(Rect area, Texture texture, float angle, Color color)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Angled Image Texture");

			if (texture != null)
			{
				if (!GUI.enabled)
				{
					color.a *= 0.5f;
				}

				GUI.color = color;
				Matrix4x4 matrix = GUI.matrix;
				GUIUtility.RotateAroundPivot(angle, area.center);
				GUI.DrawTexture(area, texture, ScaleMode.StretchToFill);
				GUI.matrix = matrix;
				GUI.color = Color.white;
			}

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static void drawImageTexture(Rect area, Texture texture, Color color)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Image Texture");

			if (texture != null)
			{
				if (!GUI.enabled)
				{
					color.a *= 0.5f;
				}

				GUI.color = color;
				GUI.DrawTexture(area, texture, ScaleMode.StretchToFill);
				GUI.color = Color.white;
			}

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static void drawTile(Rect area, Texture texture, Color color)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Tile");

			if (texture != null)
			{
				if (!GUI.enabled)
				{
					color.a *= 0.5f;
				}

				GUI.color = color;

				float layoutScale = GraphicsSettings.userInterfaceScale;
				GUI.DrawTextureWithTexCoords(area, texture, new Rect(0, 0, area.width / texture.width / layoutScale, area.height / texture.height / layoutScale));
				GUI.color = Color.white;
			}

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static void drawSliced(Rect area, Texture texture, Color color, GUIStyle style)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Sliced");

			if (texture != null)
			{
				if (!GUI.enabled)
				{
					color.a *= 0.5f;
				}

				GUI.backgroundColor = color;
				GUI.Box(area, string.Empty, style);
				GUI.color = Color.white;
			}

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static bool drawToggle(Rect area, Color color, bool state, GUIContent content)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Toggle");

			GUI.backgroundColor = color;
			state = GUI.Toggle(area, state, content);

			UnityEngine.Profiling.Profiler.EndSample();
			return state;
		}

		public static bool drawButton(Rect area, Color color)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Button");

			if (allowInput)
			{
				GUI.backgroundColor = color;
				bool state = GUI.Button(area, "");

				UnityEngine.Profiling.Profiler.EndSample();
				return state;
			}
			else
			{
				drawBox(area, color);

				UnityEngine.Profiling.Profiler.EndSample();
				return false;
			}
		}

		public static void drawBox(Rect area, Color color)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Box");

			GUI.backgroundColor = color;
			GUI.Box(area, "");

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static void drawLabel(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, GUIContent shadowContent, Color color, GUIContent content, ETextContrastContext shadowStyle)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Label");

			GUI.skin.label.fontStyle = fontStyle;
			GUI.skin.label.alignment = fontAlignment;
			GUI.skin.label.fontSize = getScaledFontSize(fontSize);

			bool wasRich = GUI.skin.label.richText;
			GUI.skin.label.richText = shadowContent != null;

			if (shadowContent == null)
			{
				drawLabelOutline(area, content, SleekShadowStyle.ContextToStyle(shadowStyle), color.a);
			}
			else
			{
				drawLabelOutline(area, shadowContent, SleekShadowStyle.ContextToStyle(shadowStyle), color.a);
			}

			GUI.contentColor = color;
			GUI.Label(area, content);

			GUI.skin.label.richText = wasRich;

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static void drawLabel(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, bool isRich, Color color, string text, ETextContrastContext shadowStyle)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Label");

			GUI.skin.label.fontStyle = fontStyle;
			GUI.skin.label.alignment = fontAlignment;
			GUI.skin.label.fontSize = getScaledFontSize(fontSize);

			if (isRich)
			{
				bool wasRich = GUI.skin.label.richText;
				GUI.skin.label.richText = isRich;

				GUI.Label(area, text);

				GUI.skin.label.richText = wasRich;
			}
			else
			{
				drawLabelOutline(area, text, SleekShadowStyle.ContextToStyle(shadowStyle), color.a);

				GUI.contentColor = color;
				GUI.Label(area, text);
			}

			UnityEngine.Profiling.Profiler.EndSample();
		}

		public static string drawField(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, Color color_0, Color color_1, string text, int maxLength, bool multiline, ETextContrastContext shadowStyle)
		{
			return DrawTextInputField(area, fontStyle, fontAlignment, fontSize, color_0, color_1, text, maxLength, string.Empty, multiline, shadowStyle);
		}

		public static string DrawTextInputField(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, Color color_0, Color color_1, string text, int maxLength, string hint, bool multiline, ETextContrastContext shadowStyle)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Field");

			GUI.skin.textArea.fontStyle = fontStyle;
			GUI.skin.textArea.alignment = fontAlignment;
			GUI.skin.textArea.fontSize = getScaledFontSize(fontSize);
			GUI.skin.textField.fontStyle = fontStyle;
			GUI.skin.textField.alignment = fontAlignment;
			GUI.skin.textField.fontSize = getScaledFontSize(fontSize);

			GUI.backgroundColor = color_0;
			GUI.contentColor = color_1;

			if (allowInput)
			{
				if (text == null)
				{
					text = string.Empty;
				}

				if (maxLength > 0)
				{
					if (multiline)
					{
						text = GUI.TextArea(area, text, maxLength);
					}
					else
					{
						text = GUI.TextField(area, text, maxLength);
					}
				}
				else
				{
					if (multiline)
					{
						text = GUI.TextArea(area, text);
					}
					else
					{
						text = GUI.TextField(area, text);
					}
				}

				if (text.Length < 1)
				{
					drawLabel(area, fontStyle, fontAlignment, fontSize, false, color_1 * 0.5f, hint, shadowStyle);
				}

				UnityEngine.Profiling.Profiler.EndSample();
				return text;
			}
			else
			{
				drawBox(area, color_0);
				drawLabel(area, fontStyle, fontAlignment, fontSize, false, color_1, text, shadowStyle);

				UnityEngine.Profiling.Profiler.EndSample();
				return text;
			}
		}

		public static string DrawPasswordField(Rect area, FontStyle fontStyle, TextAnchor fontAlignment, int fontSize, Color color_0, Color color_1, string text, int maxLength, string hint, char replace, ETextContrastContext shadowStyle)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Field");

			GUI.skin.textField.fontStyle = fontStyle;
			GUI.skin.textField.alignment = fontAlignment;
			GUI.skin.textField.fontSize = getScaledFontSize(fontSize);

			GUI.backgroundColor = color_0;
			GUI.contentColor = color_1;

			if (allowInput)
			{
				if (text == null)
				{
					text = string.Empty;
				}

				if (maxLength > 0)
				{
					text = GUI.PasswordField(area, text, replace, maxLength);
				}
				else
				{
					text = GUI.PasswordField(area, text, replace);
				}

				if (text.Length < 1)
				{
					drawLabel(area, fontStyle, fontAlignment, fontSize, false, color_1 * 0.5f, hint, shadowStyle);
				}

				UnityEngine.Profiling.Profiler.EndSample();
				return text;
			}
			else
			{
				drawBox(area, color_0);

				string replacement = string.Empty;
				if (text != null)
				{
					for (int index = 0; index < text.Length; index++)
					{
						replacement += replace;
					}
				}

				drawLabel(area, fontStyle, fontAlignment, fontSize, false, color_1, replacement, shadowStyle);

				UnityEngine.Profiling.Profiler.EndSample();
				return text;
			}
		}

		public static float drawSlider(Rect area, ESleekOrientation orientation, float state, float size, Color color)
		{
			UnityEngine.Profiling.Profiler.BeginSample("Draw Slider");

			GUI.backgroundColor = color;
			if (orientation == ESleekOrientation.HORIZONTAL)
			{
				state = GUI.HorizontalScrollbar(area, state, size, 0, 1);
			}
			else
			{
				state = GUI.VerticalScrollbar(area, state, size, 0, 1);
			}

			UnityEngine.Profiling.Profiler.EndSample();
			return state;
		}

		private static Vector2[] outlineOffsets_4way = new Vector2[4]
		{
			new Vector2(0, 1), // N
			new Vector2(1, 0), // E
			new Vector2(0, -1), // S
			new Vector2(-1, 0), // W
		};

		private static Vector2[] outlineOffsets_8way = new Vector2[8]
		{
			// normalize(1, 1) = (0.707, 0.707)
			new Vector2(0, 1), // N
			new Vector2(0.707f, 0.707f), // NE
			new Vector2(1, 0), // E
			new Vector2(0.707f, -0.707f), // SE
			new Vector2(0, -1), // S
			new Vector2(-0.707f, -0.707f), // SW
			new Vector2(-1, 0), // W
			new Vector2(-0.707f, 0.707f), // NW
		};

		private static void drawLabelOutline(Rect area, GUIContent content, Vector2[] offsets, float magnitude)
		{
			foreach (Vector2 offset in offsets)
			{
				Rect outlineRect = new Rect(area.position + (offset * magnitude), area.size);
				GUI.Label(outlineRect, content);
			}
		}

		/// <summary>
		/// Helper for drawing label outline/shadow so that we can easily change it.
		/// </summary>
		private static void drawLabelOutline(Rect area, GUIContent content, ETextContrastStyle shadowStyle, float alpha)
		{
			Color shadowColor = SleekCustomization.shadowColor;

			switch (shadowStyle)
			{
				case ETextContrastStyle.None:
					return;

				case ETextContrastStyle.Shadow:
					shadowColor.a = 0.5f * alpha;
					GUI.contentColor = shadowColor;
					area.x++;
					area.y++;
					GUI.Label(area, content);
					break;

				case ETextContrastStyle.Outline:
					shadowColor.a = 0.5f * alpha;
					GUI.contentColor = shadowColor;
					drawLabelOutline(area, content, outlineOffsets_4way, 1.0f);
					break;

				case ETextContrastStyle.Tooltip:
					shadowColor.a = 0.5f * alpha;
					GUI.contentColor = shadowColor;
					drawLabelOutline(area, content, outlineOffsets_8way, 2.0f);
					shadowColor.a = 1.0f * alpha;
					GUI.contentColor = shadowColor;
					drawLabelOutline(area, content, outlineOffsets_8way, 1.0f);
					break;
			}
		}

		private static void drawLabelOutline(Rect area, string text, Vector2[] offsets, float magnitude)
		{
			foreach (Vector2 offset in offsets)
			{
				Rect outlineRect = new Rect(area.position + (offset * magnitude), area.size);
				GUI.Label(outlineRect, text);
			}
		}

		/// <summary>
		/// Helper for drawing label outline/shadow so that we can easily change it.
		/// </summary>
		private static void drawLabelOutline(Rect area, string text, ETextContrastStyle shadowStyle, float alpha)
		{
			Color shadowColor = SleekCustomization.shadowColor;

			switch (shadowStyle)
			{
				case ETextContrastStyle.None:
					return;

				case ETextContrastStyle.Shadow:
					shadowColor.a = 0.5f * alpha;
					GUI.contentColor = shadowColor;
					area.x++;
					area.y++;
					GUI.Label(area, text);
					break;

				case ETextContrastStyle.Outline:
					shadowColor.a = 0.5f * alpha;
					GUI.contentColor = shadowColor;
					drawLabelOutline(area, text, outlineOffsets_4way, 1.0f);
					break;

				case ETextContrastStyle.Tooltip:
					shadowColor.a = 0.5f * alpha;
					GUI.contentColor = shadowColor;
					drawLabelOutline(area, text, outlineOffsets_8way, 2.0f);
					shadowColor.a = 1.0f * alpha;
					GUI.contentColor = shadowColor;
					drawLabelOutline(area, text, outlineOffsets_8way, 1.0f);
					break;
			}
		}

		public static int GetFontSize(ESleekFontSize fontSize)
		{
			switch (fontSize)
			{
				case ESleekFontSize.Tiny:
					return 8;

				case ESleekFontSize.Small:
					return 10;

				default:
				case ESleekFontSize.Default:
					return 12;

				case ESleekFontSize.Medium:
					return 14;

				case ESleekFontSize.Large:
					return 20;

				case ESleekFontSize.Title:
					return 50;
			}
		}

		public static string CreateUniqueControlName()
		{
			++controlNameCounter;
			return "Glazier" + controlNameCounter;
		}

		private static int controlNameCounter = -1;
	}
}
