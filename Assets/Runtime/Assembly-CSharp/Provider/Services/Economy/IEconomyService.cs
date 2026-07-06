////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Economy
{
	public interface IEconomyService : IService
	{
		/// <summary>
		/// Whether the user has their overlay enabled.
		/// </summary>
		bool canOpenInventory
		{
			get;
		}

		/// <summary>
		/// Requests the user's inventory.
		/// </summary>
		/// <param name="economyRequestReadyCallback">Called when the request is completed.</param>
		/// <returns>Handle for checking the owner of the callback.</returns>
		IEconomyRequestHandle requestInventory(EconomyRequestReadyCallback economyRequestReadyCallback);

		/// <summary>
		/// Requests a check for promotional items.
		/// </summary>
		/// <param name="economyRequestReadyCallback">Called when the request is completed.</param>
		/// <returns>Handle for checking the owner of the callback.</returns>
		IEconomyRequestHandle requestPromo(EconomyRequestReadyCallback economyRequestReadyCallback);

		/// <summary>
		/// Converts the input items into the output items.
		/// </summary>
		/// <param name="inputItemInstanceIDs">Items to be converted from.</param>
		/// <param name="inputItemQuantities">Item amounts to be consumed.</param>
		/// <param name="outputItemDefinitionIDs">Items to be converted to.</param>
		/// <param name="outputItemQuantities">Item amounts to be generated.</param>
		/// <param name="economyRequestReadyCallback">Called when the exchange is completed.</param>
		IEconomyRequestHandle exchangeItems(IEconomyItemInstance[] inputItemInstanceIDs, uint[] inputItemQuantities, IEconomyItemDefinition[] outputItemDefinitionIDs, uint[] outputItemQuantities, EconomyRequestReadyCallback economyRequestReadyCallback);

		void open(ulong id);
	}
}