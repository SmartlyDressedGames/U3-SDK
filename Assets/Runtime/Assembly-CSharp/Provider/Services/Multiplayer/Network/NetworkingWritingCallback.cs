////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Provider.Services.Multiplayer
{
	public delegate void NetworkingWritingCallback(MemoryStream bufferStream, BinaryWriter bufferWriter);
}
