////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Keep all the uGUI Canvas sort orders in the same place.
	/// </summary>
	public static class UnturnedCanvasSortOrders
	{
		/// <summary>
		/// Manually created canvas in the Menu scene.
		/// </summary>
		public const int MenuNewsFeed = 0;

		/// <summary>
		/// Devkit canvas in the Setup scene.
		/// </summary>
		public const int Devkit = 1;

		/// <summary>
		/// Dropdowns, drag-drop content, tab destinations, etc.
		/// </summary>
		public const int DevkitOverlay = 5;

		/// <summary>
		/// Devkit tooltips should be visible over all other devkit content.
		/// </summary>
		public const int DevkitTooltip = 10;

		/// <summary>
		/// uGUI glazier contains tooltips and cursor regardless of mode (e.g. devkit), so takes absolute priority.
		/// </summary>
		public const int Glazier = 15;

		/// <summary>
		/// Plugins were spawning canvases with high sort orders that showed over the loading screen, so as a hacky
		/// workaround we put the uGUI loading screen on a higher sort order than normal glazier.
		/// </summary>
		public const int LoadingScreen = 29000;

		/// <summary>
		/// uGUI cursor needs to show above plugin canvas.
		/// Unity exposes sort order as an int32, but it is actually an int16, so this value is slightly below the 32767 max.
		/// </summary>
		public const int Cursor = 30000;
	}
}
