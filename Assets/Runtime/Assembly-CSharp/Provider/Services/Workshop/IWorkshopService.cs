////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services.Workshop
{
	public interface IWorkshopService : IService
	{
		/// <summary>
		/// Whether the user has their overlay enabled.
		/// </summary>
		bool canOpenWorkshop
		{
			get;
		}

		void open(Steamworks.PublishedFileId_t id);
	}
}