////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum ETextContrastContext
	{
		/// <summary>
		/// Text on a default white/black box.
		/// </summary>
		Default,

		/// <summary>
		/// Text is against a plain backdrop, typically white or black.
		/// </summary>
		InconspicuousBackdrop,

		/// <summary>
		/// Text is against an in-game or image backdrop.
		/// </summary>
		ColorfulBackdrop,

		/// <summary>
		/// Special case for tooltip text.
		/// </summary>
		Tooltip,
	}

	public enum ETextContrastStyle
	{
		/// <summary>
		/// No shadow or outline.
		/// </summary>
		None,

		/// <summary>
		/// Outline in all directions.
		/// </summary>
		Outline,

		/// <summary>
		/// Shadow down and to the right.
		/// </summary>
		Shadow,

		/// <summary>
		/// Special case for tooltip text.
		/// </summary>
		Tooltip,
	}
}
