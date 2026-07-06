////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables
{
	public class KeyValueTableTypeReaderRegistry
	{
		private static Dictionary<Type, IFormattedTypeReader> readers = new Dictionary<Type, IFormattedTypeReader>();

		public static T read<T>(IFormattedFileReader input)
		{
			IFormattedTypeReader reader;
			if (readers.TryGetValue(typeof(T), out reader))
			{
				object value = reader.read(input);
				if (value == null)
				{
					return default;
				}
				else
				{
					return (T) value;
				}
			}
			else if (typeof(T).IsEnum)
			{
				string value = input.readValue();
				if (string.IsNullOrEmpty(value))
				{
					return default;
				}
				else
				{
					return (T) Enum.Parse(typeof(T), value, true);
				}
			}

			SDG.Unturned.UnturnedLog.error("Failed to find reader for: " + typeof(T));
			return default;
		}

		public static object read(IFormattedFileReader input, Type type)
		{
			IFormattedTypeReader reader;
			if (readers.TryGetValue(type, out reader))
			{
				object value = reader.read(input);
				if (value == null)
				{
					return type.getDefaultValue();
				}
				else
				{
					return value;
				}
			}
			else if (type.IsEnum)
			{
				string value = input.readValue();
				if (string.IsNullOrEmpty(value))
				{
					return type.getDefaultValue();
				}
				else
				{
					return Enum.Parse(type, value, true);
				}
			}

			SDG.Unturned.UnturnedLog.error("Failed to find reader for: " + type);
			return type.getDefaultValue();
		}

		public static void add<T>(IFormattedTypeReader reader)
		{
			add(typeof(T), reader);
		}

		public static void add(Type type, IFormattedTypeReader reader)
		{
			readers.Add(type, reader);
		}

		public static void remove<T>()
		{
			remove(typeof(T));
		}

		public static void remove(Type type)
		{
			readers.Remove(type);
		}
	}
}
