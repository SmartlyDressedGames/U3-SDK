////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Context for the Assets.ReportError methods.
	/// Nelson 2024-11-20: Converted from directly using asset to this interface so that asset-related features can
	/// more easily log warnings to the in-game menu.
	/// </summary>
	public interface IAssetErrorContext
	{
		/// <summary>
		/// Format text to prefix any errors reported in this context. (e.g., this asset's name and ID)
		/// </summary>
		public string AssetErrorPrefix
		{
			get;
		}

		void ReportAssetError(string message);
	}
}
