////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public sealed class DatWriter : System.IDisposable
	{
		public DatWriter()
		{ }

		public DatWriter(System.IO.TextWriter output)
		{
			SetOutput(output);
		}

		public void SetOutput(System.IO.TextWriter output)
		{
			this.output = output;
			stack.Clear();
			indentationDepth = 0;
		}

		public void Dispose()
		{
			CloseStack();
		}

		/// <summary>
		/// Write corresponding ends to any open dictionaries/lists.
		/// </summary>
		public void CloseStack()
		{
			while (stack.Count > 0)
			{
				switch (stack.Peek())
				{
					case EWriterToken.Dictionary:
						WriteDictionaryEnd();
						break;

					case EWriterToken.List:
						WriteListEnd();
						break;

					case EWriterToken.Key:
						stack.Pop();
						break;
				}
			}
		}

		public void WriteEmptyLine()
		{
			output.WriteLine();
		}

		public void WriteKey(string key)
		{
			if (key == null)
			{
				throw new System.ArgumentNullException(nameof(key));
			}

			if (key.Length < 1)
			{
				throw new System.ArgumentException("cannot be empty", nameof(key));
			}

			if (stack.Count > 0 && stack.Peek() != EWriterToken.Dictionary)
			{
				throw new System.Exception($"Cannot write key (\"{key}\") into {stack.Peek()}");
			}

			WriteIndentation();
			output.Write(key);
			stack.Push(EWriterToken.Key);
		}

		public void WriteValue(string value, string comment = null)
		{
			if (stack.Count < 1)
			{
				throw new System.Exception($"Writing value into empty slot");
			}

			if (stack.Peek() == EWriterToken.Dictionary)
			{
				throw new System.Exception($"Cannot write value (\"{value}\") into {stack.Peek()}");
			}

			if (string.IsNullOrEmpty(value)) // Probably key is treated as a flag.
			{
				if (stack.Peek() == EWriterToken.Key)
				{
					stack.Pop();
				}
				if (!string.IsNullOrEmpty(comment))
				{
					output.Write(" // ");
					output.Write(comment);
				}
				output.WriteLine();
				return;
			}

			if (stack.Peek() == EWriterToken.Key)
			{
				output.Write(' ');
				stack.Pop();
			}
			else
			{
				WriteIndentation();
			}

			// If it starts with quotation mark it will need to be within quotes to prevent reading as quoted string.
			bool quoted = value[0] == '"' || !string.IsNullOrEmpty(comment);

			if (quoted)
			{
				output.Write('"');
			}

			foreach (char c in value)
			{
				switch (c)
				{
					case '\n': // line feed
						output.Write("\\n");
						break;

					case '\r': // carriage return
						// Nelson 2025-09-29: we skip '\r' in output because we skip it while reading. (public issue #5226)
						// (The game only uses '\n' for newlines in dat strings.)
						//output.Write("\\r");
						break;

					case '\t': // horizontal tab
						output.Write("\\t");
						break;

					case '\\': // backslash
						output.Write("\\\\");
						break;

					case '"': // double quote
						output.Write(quoted ? "\\\"" : "\"");
						break;

					default:
						output.Write(c);
						break;
				}
			}

			if (quoted)
			{
				output.Write('"');
			}

			if (!string.IsNullOrEmpty(comment))
			{
				output.Write(" // ");
				output.Write(comment);
			}

			output.WriteLine();
		}

		public void WriteValue(sbyte value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(byte value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(short value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(ushort value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(int value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(uint value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(long value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(ulong value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(float value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValue(double value, string comment = null)
		{
			WriteValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture), comment);
		}

		public void WriteValueEnumString<T>(T value, string comment = null) where T : struct
		{
			WriteValue(value.ToString(), comment);
		}

		public void WriteValue(bool value, string comment = null)
		{
			WriteValue(value ? "true" : "false", comment);
		}

		public void WriteValue(System.Guid value, string comment = null)
		{
			WriteValue(value.ToString("N"), comment);
		}

		public void WriteValue(System.DateTime value, string comment = null)
		{
			if (value.Hour == 0 && value.Minute == 0 && value.Second == 0)
			{
				WriteValue(value.ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.InvariantCulture), comment);
			}
			else
			{
				WriteValue(value.ToString("yyyy'-'MM'-'dd HH':'mm':'ss", System.Globalization.CultureInfo.InvariantCulture), comment);
			}
		}

		public void WriteKeyValue(string key, string value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, sbyte value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, byte value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, short value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, ushort value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, int value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, uint value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, long value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, ulong value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, float value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, double value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValueEnumString<T>(string key, T value, string comment = null) where T : struct
		{
			WriteKey(key);
			WriteValueEnumString<T>(value, comment);
		}

		public void WriteKeyValue(string key, bool value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, System.Guid value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteKeyValue(string key, System.DateTime value, string comment = null)
		{
			WriteKey(key);
			WriteValue(value, comment);
		}

		public void WriteDictionaryStart()
		{
			if (stack.Count < 1)
			{
				throw new System.Exception($"Cannot write dictionary into root without a key");
			}

			if (stack.Peek() == EWriterToken.Dictionary)
			{
				throw new System.Exception($"Cannot write dictionary into dictionary without a key");
			}

			if (stack.Peek() == EWriterToken.Key)
			{
				output.WriteLine();
				stack.Pop();
			}

			WriteIndentation();
			output.WriteLine('{');
			stack.Push(EWriterToken.Dictionary);
			++indentationDepth;
		}

		public void WriteDictionaryStart(string key)
		{
			WriteKey(key);
			WriteDictionaryStart();
		}

		public void WriteDictionaryEnd()
		{
			if (stack.Count < 1)
			{
				throw new System.Exception("Reached end of stack");
			}

			if (stack.Peek() != EWriterToken.Dictionary)
			{
				throw new System.Exception($"Current element ({stack.Peek()}) is not a dictionary");
			}

			--indentationDepth;
			stack.Pop();
			WriteIndentation();
			output.WriteLine('}');
		}

		public void WriteListStart()
		{
			if (stack.Count < 1)
			{
				throw new System.Exception($"Cannot write dictionary into root without a key name");
			}

			if (stack.Peek() == EWriterToken.Dictionary)
			{
				throw new System.Exception($"Cannot write list into dictionary without a key");
			}

			if (stack.Peek() == EWriterToken.Key)
			{
				output.WriteLine();
				stack.Pop();
			}

			WriteIndentation();
			output.WriteLine('[');
			stack.Push(EWriterToken.List);
			++indentationDepth;
		}

		public void WriteListStart(string key)
		{
			WriteKey(key);
			WriteListStart();
		}

		public void WriteListEnd()
		{
			if (stack.Count < 1)
			{
				throw new System.Exception("Reached end of stack");
			}

			if (stack.Peek() != EWriterToken.List)
			{
				throw new System.Exception($"Current element ({stack.Peek()}) is not a list");
			}

			--indentationDepth;
			stack.Pop();
			WriteIndentation();
			output.WriteLine(']');
		}

		public void WriteComment(string message)
		{
			WriteIndentation();
			output.Write("// ");
			output.WriteLine(message);
		}

		public void WriteNode(IDatNode node)
		{
			if (node == null)
			{
				throw new System.ArgumentNullException(nameof(node));
			}

			switch (node.NodeType)
			{
				case EDatNodeType.Value:
					WriteValue(((IDatValue) node).Value);
					break;

				case EDatNodeType.Dictionary:
					WriteDictionary((IDatDictionary) node);
					break;

				case EDatNodeType.List:
					WriteList((IDatList) node);
					break;
			}
		}

		public void WriteDictionary(IDatDictionary dictionary)
		{
			if (dictionary == null)
			{
				throw new System.ArgumentNullException(nameof(dictionary));
			}

			bool isRoot = stack.Count < 1;

			if (!isRoot)
			{
				WriteDictionaryStart();
			}

			foreach (KeyValuePair<string, IDatNode> pair in dictionary)
			{
				WriteKey(pair.Key);
				if (pair.Value != null)
				{
					WriteNode(pair.Value);
				}
				else
				{
					WriteValue(null);
				}
			}

			if (!isRoot)
			{
				WriteDictionaryEnd();
			}
		}

		public void WriteList(IDatList list)
		{
			if (list == null)
			{
				throw new System.ArgumentNullException(nameof(list));
			}

			WriteListStart();
			foreach (IDatNode node in list)
			{
				if (node != null)
				{
					WriteNode(node);
				}
			}
			WriteListEnd();
		}

		private void WriteIndentation()
		{
			for (int tabIndex = 0; tabIndex < indentationDepth; ++tabIndex)
			{
				output.Write('\t');
			}
		}

		private enum EWriterToken
		{
			Dictionary,
			List,
			Key,
		}

		private Stack<EWriterToken> stack = new Stack<EWriterToken>();
		private System.IO.TextWriter output;
		private int indentationDepth;
	}
}
