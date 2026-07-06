////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Provider.Services
{
	public interface IService
	{
		/// <summary>
		/// Initialize this service's external API. Should be called before using.
		/// </summary>
		void initialize();

		/// <summary>
		/// Update this service's external API. Should be called every frame.
		/// </summary>
		void update();

		/// <summary>
		/// Shutdown this service's external API. Should be called before closing the program.
		/// </summary>
		void shutdown();
	}
}