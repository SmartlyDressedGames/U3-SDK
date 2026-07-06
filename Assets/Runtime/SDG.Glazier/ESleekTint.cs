////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum ESleekTint
	{
		NONE,

		/// <summary>
		/// OptionsSettings.backgroundColor
		/// </summary>
		BACKGROUND,

		/// <summary>
		/// OptionsSettings.foregroundColor
		/// </summary>
		FOREGROUND,

		/// <summary>
		/// OptionsSettings.fontColor
		/// </summary>
		FONT,

		/// <summary>
		/// Use a medium gray for rich text labels.
		/// </summary>
		RICH_TEXT_DEFAULT,

		/// <summary>
		/// OptionsSettings.backgroundColor if using the light theme, otherwise specified color.
		/// Useful for rarity and gold backdrops that are overwhelming if using the rarity color.
		/// </summary>
		BACKGROUND_IF_LIGHT,

		/// <summary>
		/// OptionsSettings.badColor rather than red.
		/// </summary>
		BAD,
	}
}
