////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	public struct SleekColor
	{
		public SleekColor(ESleekTint tint)
		{
			this.tint = tint;
			customColor = Color.white;
		}

		public SleekColor(ESleekTint tint, float alpha)
		{
			this.tint = tint;
			customColor = Color.white;
			customColor.a = alpha;
		}

		public SleekColor(Color customColor)
		{
			tint = ESleekTint.NONE;
			this.customColor = customColor;
		}

		public SleekColor(Color32 customColor)
		{
			tint = ESleekTint.NONE;
			this.customColor = customColor;
		}

		public static SleekColor BackgroundIfLight(Color customColor)
		{
			return new SleekColor(ESleekTint.BACKGROUND_IF_LIGHT, customColor);
		}

		public static SleekColor BackgroundIfLight(float alpha)
		{
			return new SleekColor(ESleekTint.BACKGROUND_IF_LIGHT, alpha);
		}

		public Color Get()
		{
			switch (tint)
			{
				default:
					return Color.white;

				case ESleekTint.NONE:
					return customColor;

				case ESleekTint.BACKGROUND:
				{
					Color color = SleekCustomization.backgroundColor;
					color.a = customColor.a;
					return color;
				}

				case ESleekTint.FOREGROUND:
				{
					Color color = SleekCustomization.foregroundColor;
					color.a = customColor.a;
					return color;
				}

				case ESleekTint.FONT:
				{
					Color color = SleekCustomization.fontColor;
					color.a = customColor.a;
					return color;
				}

				case ESleekTint.RICH_TEXT_DEFAULT:
				{
					Color color = new Color32(180, 180, 180, byte.MaxValue);
					color.a = customColor.a;
					return color;
				}

				case ESleekTint.BACKGROUND_IF_LIGHT:
					if (SleekCustomization.darkTheme)
					{
						return customColor;
					}
					else
					{
						Color color = SleekCustomization.backgroundColor;
						color.a = customColor.a;
						return color;
					}

				case ESleekTint.BAD:
				{
					Color color = SleekCustomization.badColor;
					color.a = customColor.a;
					return color;
				}
			}
		}

		public StyleColor GetStyleColor()
		{
			return new StyleColor(Get());
		}

		public void SetAlpha(float alpha)
		{
			customColor.a = alpha;
		}

		public static implicit operator SleekColor(ESleekTint tint)
		{
			return new SleekColor(tint);
		}

		public static implicit operator SleekColor(Color customColor)
		{
			return new SleekColor(customColor);
		}

		public static implicit operator SleekColor(Color32 customColor)
		{
			return new SleekColor(customColor);
		}

		public static implicit operator Color(SleekColor color)
		{
			return color.Get();
		}

		public static implicit operator StyleColor(SleekColor color)
		{
			return color.GetStyleColor();
		}

		/// <summary>
		/// Private because some tint/color combinations are unsupported.
		/// </summary>
		private SleekColor(ESleekTint tint, Color customColor)
		{
			this.tint = tint;
			this.customColor = customColor;
		}

		private ESleekTint tint;
		private Color customColor;
	}
}
