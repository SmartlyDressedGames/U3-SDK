////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Simplifies glazier implementation by restricting the set of usable font sizes.
	/// </summary>
	public enum ESleekFontSize
	{
		/// <summary>
		/// IMGUI: 8
		/// </summary>
		Tiny,

		/// <summary>
		/// Ideal for 20px height.
		/// IMGUI: 10.
		/// </summary>
		Small,

		/// <summary>
		/// Default for all UI text, ideal for 30px height.
		/// IMGUI: 12
		/// 
		/// If changing the default font size, please also update the USS font size!
		/// (The default is applied by the stylesheet rather than inline style.)
		/// </summary>
		Default,

		/// <summary>
		/// Ideal for 50px height.
		/// IMGUI: 14
		/// </summary>
		Medium,

		/// <summary>
		/// Ideal for 70px height.
		/// IMGUI: 20
		/// </summary>
		Large,

		/// <summary>
		/// Only used by the main menu "Unturned" text.
		/// </summary>
		Title,
	}
}
