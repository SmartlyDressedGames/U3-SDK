////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables
{
	public class KeyValueTableTypeWriterRegistry
	{
		private static Dictionary<Type, IFormattedTypeWriter> writers = new Dictionary<Type, IFormattedTypeWriter>();

		public static void write<T>(IFormattedFileWriter output, T value)
		{
			IFormattedTypeWriter writer;
			if (writers.TryGetValue(typeof(T), out writer))
			{
				writer.write(output, value);
			}
			else
			{
				output.writeValue(value.ToString());
			}
		}

		public static void write(IFormattedFileWriter output, object value)
		{
			IFormattedTypeWriter writer;
			if (writers.TryGetValue(value.GetType(), out writer))
			{
				writer.write(output, value);
			}
			else
			{
				output.writeValue(value.ToString());
			}
		}

		public static void add<T>(IFormattedTypeWriter writer)
		{
			add(typeof(T), writer);
		}

		public static void add(Type type, IFormattedTypeWriter writer)
		{
			writers.Add(type, writer);
		}

		public static void remove<T>()
		{
			remove(typeof(T));
		}

		public static void remove(Type type)
		{
			writers.Remove(type);
		}
	}
}
