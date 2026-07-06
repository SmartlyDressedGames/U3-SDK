////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System.IO;

namespace SDG.Framework.IO.Deserialization
{
	public class JSONDeserializer : IDeserializer
	{
		public T deserialize<T>(byte[] data, int offset)
		{
			MemoryStream memoryStream = new MemoryStream(data, offset, data.Length - offset);
			T instance = deserialize<T>(memoryStream);

			memoryStream.Close();
			memoryStream.Dispose();

			return instance;
		}

		public T deserialize<T>(MemoryStream memoryStream)
		{
			T instance = default;

			StreamReader streamReader = new StreamReader(memoryStream);
			JsonReader jsonReader = new JsonTextReader(streamReader);
			JsonSerializer jsonDeserializer = new JsonSerializer();

			try
			{
				instance = jsonDeserializer.Deserialize<T>(jsonReader);
			}
			finally
			{
				jsonReader.Close();

				streamReader.Close();
				streamReader.Dispose();
			}

			return instance;
		}

		public T deserialize<T>(string path)
		{
			T instance = default;

			StreamReader streamReader = new StreamReader(path);
			JsonReader jsonReader = new JsonTextReader(streamReader);
			JsonSerializer jsonDeserializer = new JsonSerializer();

			try
			{
				instance = jsonDeserializer.Deserialize<T>(jsonReader);
			}
			finally
			{
				jsonReader.Close();

				streamReader.Close();
				streamReader.Dispose();
			}

			return instance;
		}
	}
}
