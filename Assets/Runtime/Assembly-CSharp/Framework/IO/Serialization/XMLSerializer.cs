////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SDG.Framework.IO.Serialization
{
	public class XMLSerializer : ISerializer
	{
		private static readonly XmlSerializerNamespaces XML_SERIALIZER_NAMESPACES = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
		private static readonly XmlWriterSettings XML_WRITER_SETTINGS_FORMATTED = new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true, Encoding = new System.Text.UTF8Encoding() };
		private static readonly XmlWriterSettings XML_WRITER_SETTINGS_UNFORMATTED = new XmlWriterSettings() { Indent = false, OmitXmlDeclaration = true, Encoding = new System.Text.UTF8Encoding() };

		public void serialize<T>(T instance, byte[] data, int index, out int size, bool isFormatted)
		{
			MemoryStream memoryStream = new MemoryStream(data, index, data.Length - index);
			serialize(instance, memoryStream, isFormatted);
			size = (int) memoryStream.Position;

			memoryStream.Close();
			memoryStream.Dispose();
		}

		public void serialize<T>(T instance, MemoryStream memoryStream, bool isFormatted)
		{
			XmlWriter xmlWriter = XmlWriter.Create(memoryStream, isFormatted ? XML_WRITER_SETTINGS_FORMATTED : XML_WRITER_SETTINGS_UNFORMATTED);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			try
			{
				xmlSerializer.Serialize(xmlWriter, instance, XML_SERIALIZER_NAMESPACES);

				xmlWriter.Flush();
			}
			finally
			{
				// Used to close memory stream here
			}
		}

		public void serialize<T>(T instance, string path, bool isFormatted)
		{
			StreamWriter streamWriter = new StreamWriter(path);
			XmlWriter xmlWriter = XmlWriter.Create(streamWriter, isFormatted ? XML_WRITER_SETTINGS_FORMATTED : XML_WRITER_SETTINGS_UNFORMATTED);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			try
			{
				xmlSerializer.Serialize(xmlWriter, instance, XML_SERIALIZER_NAMESPACES);

				xmlWriter.Flush();
			}
			finally
			{
				streamWriter.Close();
				streamWriter.Dispose();
			}
		}
	}
}
