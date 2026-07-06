////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Modules
{
	public enum EModuleRole
	{
		None,
		Client, // Only load on client
		Server, // Only load on dedicated server
		Both_Optional, // Load on client and server, doesn't matter if one is missing it
		Both_Required // Load on client and server, refuse connection if either is missing it
	}
}