////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Store
{
	public interface IStoreService : IService
	{
		/// <summary>
		/// View a package on the store.
		/// </summary>
		/// <param name="packageID">Package to view.</param>
		void open(IStorePackageID packageID);
	}
}
