////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public static class SleekShadowStyle
	{
		public static ETextContrastStyle ContextToStyle(ETextContrastContext context)
		{
			return contextToStyleLookupTable[(int) context];
		}

		private static ETextContrastStyle UncachedContextToStyle(ETextContrastContext context)
		{
			switch (context)
			{
				default:
				case ETextContrastContext.Default:
					switch (SleekCustomization.defaultTextContrast)
					{
						default:
						case ETextContrastPreference.Default:
						case ETextContrastPreference.None:
							return ETextContrastStyle.None;

						case ETextContrastPreference.Shadow:
							return ETextContrastStyle.Shadow;

						case ETextContrastPreference.Outline:
							return ETextContrastStyle.Outline;
					}

				case ETextContrastContext.InconspicuousBackdrop:
					switch (SleekCustomization.inconspicuousTextContrast)
					{
						case ETextContrastPreference.None:
							return ETextContrastStyle.None;

						default:
						case ETextContrastPreference.Default:
						case ETextContrastPreference.Shadow:
							return ETextContrastStyle.Shadow;

						case ETextContrastPreference.Outline:
							return ETextContrastStyle.Outline;
					}

				case ETextContrastContext.ColorfulBackdrop:
					switch (SleekCustomization.colorfulTextContrast)
					{
						case ETextContrastPreference.None:
							return ETextContrastStyle.None;

						case ETextContrastPreference.Shadow:
							return ETextContrastStyle.Shadow;

						default:
						case ETextContrastPreference.Default:
						case ETextContrastPreference.Outline:
							return ETextContrastStyle.Outline;
					}

				case ETextContrastContext.Tooltip:
					return ETextContrastStyle.Tooltip;
			}
		}

		static SleekShadowStyle()
		{
			contextToStyleLookupTable = new ETextContrastStyle[4];
			for (int index = 0; index < 4; ++index)
			{
				ETextContrastStyle style = UncachedContextToStyle((ETextContrastContext) index);
				contextToStyleLookupTable[index] = style;
			}
		}

		private static ETextContrastStyle[] contextToStyleLookupTable = null;
	}
}
