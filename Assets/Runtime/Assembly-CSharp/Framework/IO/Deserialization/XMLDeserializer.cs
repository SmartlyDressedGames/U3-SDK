////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Xml.Serialization;

namespace SDG.Framework.IO.Deserialization
{
	public class XMLDeserializer : IDeserializer
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

			XmlSerializer xmlDeserializer = new XmlSerializer(typeof(T));

			try
			{
				instance = (T) xmlDeserializer.Deserialize(memoryStream);
			}
			finally
			{
				// Used to close memory stream here
			}

			return instance;
		}

		public T deserialize<T>(string path)
		{
			T instance = default;

			XmlSerializer xmlDeserializer = new XmlSerializer(typeof(T));
			StreamReader streamReader = new StreamReader(path);

			try
			{
				instance = (T) xmlDeserializer.Deserialize(streamReader);
			}
			finally
			{
				streamReader.Close();
				streamReader.Dispose();
			}

			return instance;
		}
	}
}
