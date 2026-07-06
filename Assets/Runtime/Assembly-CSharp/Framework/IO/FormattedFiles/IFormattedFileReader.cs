////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Framework.IO.FormattedFiles
{
	public interface IFormattedFileReader
	{
		IEnumerable<string> getKeys();
		bool containsKey(string key);

		void readKey(string key);

		int readArrayLength(string key);
		int readArrayLength();

		void readArrayIndex(string key, int index);
		void readArrayIndex(int index);

		string readValue(string key);
		string readValue(int index);
		string readValue(string key, int index);
		string readValue();

		object readValue(Type type, string key);
		object readValue(Type type, int index);
		object readValue(Type type, string key, int index);
		object readValue(Type type);

		T readValue<T>(string key);
		T readValue<T>(int index);
		T readValue<T>(string key, int index);
		T readValue<T>();

		IFormattedFileReader readObject(string key);
		IFormattedFileReader readObject(int index);
		IFormattedFileReader readObject(string key, int index);
		IFormattedFileReader readObject();
	}
}
