////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_KVT_READER

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables
{
	public class KeyValueTableReader : IFormattedFileReader
	{
		protected static StringBuilder builder = new StringBuilder();

		public Dictionary<string, object> table
		{
			get;
			protected set;
		}

		protected string key;
		protected int index;

		protected string dictionaryKey;
		protected bool dictionaryInQuotes;
		protected bool dictionaryIgnoreNextChar;

		protected bool listInQuotes = false;
		protected bool listIgnoreNextChar = false;

		public virtual IEnumerable<string> getKeys()
		{
			return table.Keys;
		}

		public virtual bool containsKey(string key)
		{
			return table.ContainsKey(key);
		}

		public virtual void readKey(string key)
		{
			this.key = key;
			index = -1;
		}

		public virtual int readArrayLength(string key)
		{
			readKey(key);
			return readArrayLength();
		}

		public virtual int readArrayLength()
		{
			object values;
			if (table.TryGetValue(key, out values))
			{
				return (values as List<object>).Count;
			}
			else
			{
				return 0;
			}
		}

		public virtual void readArrayIndex(string key, int index)
		{
			readKey(key);
			readArrayIndex(index);
		}

		public virtual void readArrayIndex(int index)
		{
			this.index = index;
		}

		public virtual string readValue(string key)
		{
			readKey(key);
			return readValue();
		}

		public virtual string readValue(int index)
		{
			readArrayIndex(index);
			return readValue();
		}

		public virtual string readValue(string key, int index)
		{
			readKey(key);
			readArrayIndex(index);
			return readValue();
		}

		public virtual string readValue()
		{
			if (index == -1) // non-array
			{
				object value;
				if (!table.TryGetValue(key, out value))
				{
					return default;
				}

				return (string) value;
			}
			else // array
			{
				object values;
				if (table.TryGetValue(key, out values))
				{
					object value = (values as List<object>)[index];
					return (string) value;
				}
				else
				{
					return default;
				}
			}
		}

		public virtual object readValue(Type type, string key)
		{
			readKey(key);
			return readValue(type);
		}

		public virtual object readValue(Type type, int index)
		{
			readArrayIndex(index);
			return readValue(type);
		}

		public virtual object readValue(Type type, string key, int index)
		{
			readKey(key);
			readArrayIndex(index);
			return readValue(type);
		}

		public virtual object readValue(Type type)
		{
			if (typeof(IFormattedFileReadable).IsAssignableFrom(type))
			{
				IFormattedFileReadable readable = System.Activator.CreateInstance(type) as IFormattedFileReadable;
				readable.read(this);
				return readable;
			}
			else
			{
				return KeyValueTableTypeReaderRegistry.read(this, type);
			}
		}

		public virtual T readValue<T>(string key)
		{
			readKey(key);
			return readValue<T>();
		}

		public virtual T readValue<T>(int index)
		{
			readArrayIndex(index);
			return readValue<T>();
		}

		public virtual T readValue<T>(string key, int index)
		{
			readKey(key);
			readArrayIndex(index);
			return readValue<T>();
		}

		public virtual T readValue<T>()
		{
			if (typeof(IFormattedFileReadable).IsAssignableFrom(typeof(T)))
			{
				IFormattedFileReadable readable = System.Activator.CreateInstance<T>() as IFormattedFileReadable;
				readable.read(this);
				return (T) readable;
			}
			else
			{
				return KeyValueTableTypeReaderRegistry.read<T>(this);
			}
		}

		public virtual IFormattedFileReader readObject(string key)
		{
			readKey(key);
			return readObject();
		}

		public virtual IFormattedFileReader readObject(int index)
		{
			readArrayIndex(index);
			return readObject();
		}

		public virtual IFormattedFileReader readObject(string key, int index)
		{
			readKey(key);
			readArrayIndex(index);
			return readObject();
		}

		public virtual IFormattedFileReader readObject()
		{
			if (index == -1) // non-array
			{
				object value;
				if (table.TryGetValue(key, out value))
				{
					return value as IFormattedFileReader;
				}
				else
				{
					return null;
				}
			}
			else
			{
				object values;
				if (table.TryGetValue(key, out values))
				{
					return (values as List<object>)[index] as IFormattedFileReader;
				}
				else
				{
					return null;
				}
			}
		}

		protected virtual bool canContinueReadDictionary(StreamReader input, Dictionary<string, object> scope)
		{
			return true;
		}

		public virtual void readDictionary(StreamReader input, Dictionary<string, object> scope)
		{
			dictionaryKey = null;
			dictionaryInQuotes = false;
			dictionaryIgnoreNextChar = false;

			while (!input.EndOfStream)
			{
				char character = (char) input.Read();

				if (dictionaryIgnoreNextChar)
				{
					builder.Append(character);
					dictionaryIgnoreNextChar = false;
					continue;
				}

				if (character == '\\')
				{
					dictionaryIgnoreNextChar = true;
					continue;
				}

				if (character == '"')
				{
					if (dictionaryInQuotes)
					{
						dictionaryInQuotes = false;

						if (string.IsNullOrEmpty(dictionaryKey))
						{
							dictionaryKey = builder.ToString();
						}
						else
						{
							string value = builder.ToString();
							if (!scope.ContainsKey(dictionaryKey))
							{
								scope.Add(dictionaryKey, value);
#if LOG_KVT_READER
								UnturnedLog.info(key + "=" + value);
#endif
							}

							if (!canContinueReadDictionary(input, scope))
							{
								return;
							}
							dictionaryKey = null;
						}
					}
					else
					{
						dictionaryInQuotes = true;

						builder.Length = 0;
					}
				}
				else if (dictionaryInQuotes)
				{
					builder.Append(character);
				}
				else
				{
					if (character == '{')
					{
#if LOG_KVT_READER
						UnturnedLog.info("entering object " + key);
#endif
						object subtable;
						if (scope.TryGetValue(dictionaryKey, out subtable))
						{
							KeyValueTableReader reader = (KeyValueTableReader) subtable;
							reader.readDictionary(input, reader.table);
						}
						else
						{
							KeyValueTableReader reader = new KeyValueTableReader(input);
							subtable = reader;
							scope.Add(dictionaryKey, reader);
						}
#if LOG_KVT_READER
						UnturnedLog.info(key + "=" + subtable);
#endif
						if (!canContinueReadDictionary(input, scope))
						{
							return;
						}
						dictionaryKey = null;
					}
					else if (character == '}')
					{
#if LOG_KVT_READER
						UnturnedLog.info("exiting object");
#endif
						return;
					}
					else if (character == '[')
					{
#if LOG_KVT_READER
						UnturnedLog.info("entering list " + key);
#endif
						object sublist;
						if (!scope.TryGetValue(dictionaryKey, out sublist))
						{
							sublist = new List<object>();
							scope.Add(dictionaryKey, sublist);
						}
						readList(input, (List<object>) sublist);
#if LOG_KVT_READER
						UnturnedLog.info(key + "=" + sublist);
#endif
						if (!canContinueReadDictionary(input, scope))
						{
							return;
						}
						dictionaryKey = null;
					}
				}
			}
		}

		public virtual void readList(StreamReader input, List<object> scope)
		{
			listInQuotes = false;
			listIgnoreNextChar = false;

			while (!input.EndOfStream)
			{
				char character = (char) input.Read();

				if (listIgnoreNextChar)
				{
					builder.Append(character);
					listIgnoreNextChar = false;
					continue;
				}

				if (character == '\\')
				{
					listIgnoreNextChar = true;
					continue;
				}

				if (character == '"')
				{
					if (listInQuotes)
					{
						listInQuotes = false;

						string element = builder.ToString();
						scope.Add(element);
#if LOG_KVT_READER
						UnturnedLog.info(element + " @" + scope.Count);
#endif
					}
					else
					{
						listInQuotes = true;

						builder.Length = 0;
					}
				}
				else if (listInQuotes)
				{
					builder.Append(character);
				}
				else
				{
					if (character == '{')
					{
#if LOG_KVT_READER
						UnturnedLog.info("entering object");
#endif
						KeyValueTableReader element = new KeyValueTableReader(input);
						scope.Add(element);
#if LOG_KVT_READER
						UnturnedLog.info(element + " @" + scope.Count);
#endif
					}
					// sublists are tricky, not sure what to do about them
					//					else if(character == '[')
					//					{
					//#if LOG_KVT_READER
					//						UnturnedLog.info("entering list");
					//#endif
					//						List<object> element = readList(input);
					//						scope.Add(element);
					//#if LOG_KVT_READER
					//						UnturnedLog.info(element + " @" + scope.Count);
					//#endif
					//					}
					else if (character == ']')
					{
#if LOG_KVT_READER
						UnturnedLog.info("exiting list");
#endif
						return;
					}
				}
			}
		}

		public KeyValueTableReader()
		{
			table = new Dictionary<string, object>();
		}

		public KeyValueTableReader(StreamReader input)
		{
			table = new Dictionary<string, object>();
			readDictionary(input, table);
		}
	}
}
