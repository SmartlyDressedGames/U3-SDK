////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Framework.IO.Deserialization
{
	public interface IDeserializer
	{
		T deserialize<T>(byte[] data, int offset);
		T deserialize<T>(MemoryStream memoryStream);
		T deserialize<T>(string path);
	}
}
