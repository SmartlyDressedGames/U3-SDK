////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public interface IDatSerializable
	{
		/// <summary>
		/// Nelson 2025-07-23: not super happy with this approach. (Don't rely on it.)
		/// Added initially for <see cref="BrowserConfigData.Link"/>.
		/// Future improvement should support:
		/// • Serializing into existing object. I.e., patching user-configured dat file?
		/// • Not being specific to pre-created dictionaries.
		/// </summary>
		public void SerializeIntoDictionary(IEditableDatDictionary dictionary);
	}
}
