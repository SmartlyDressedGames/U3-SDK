////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.IO.FormattedFiles
{
	public interface IFormattedFileWriter
	{
		void writeKey(string key);
		void writeValue(string key, string value);
		void writeValue(string value);

		void writeValue(string key, object value);
		void writeValue(object value);

		void writeValue<T>(string key, T value);
		void writeValue<T>(T value);

		void beginObject();
		void beginObject(string key);
		void endObject();

		void beginArray(string key);
		void beginArray();
		void endArray();
	}
}
