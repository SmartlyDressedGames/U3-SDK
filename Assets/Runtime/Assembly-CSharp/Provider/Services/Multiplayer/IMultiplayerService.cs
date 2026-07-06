////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services.Multiplayer.Client;
using SDG.Provider.Services.Multiplayer.Server;

namespace SDG.Provider.Services.Multiplayer
{
	public interface IMultiplayerService : IService
	{
		/// <summary>
		/// Current client multiplayer implementation.
		/// </summary>
		IClientMultiplayerService clientMultiplayerService
		{
			get;
		}

		/// <summary>
		/// Current server multiplayer implementation.
		/// </summary>
		IServerMultiplayerService serverMultiplayerService
		{
			get;
		}
	}
}