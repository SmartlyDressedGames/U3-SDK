////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SDG.Unturned
{
	public class NetEnumGenerator
	{
		public void EvaluateType(Type enumType)
		{
			NetEnumAttribute attribute = enumType.GetCustomAttribute<NetEnumAttribute>();
			if (attribute == null)
				return;

			// To-do: does not handle enums with manually overridden values, e.g. Val = -300.
			Array enumValues = enumType.GetEnumValues();
			int maxEnumIndex = enumValues.Length - 1;
			int bitsRequired = NetPakConst.CountBits((uint) maxEnumIndex);
			uint maxBitsValue = (1U << bitsRequired) - 1;

			string path = Path.Combine(Application.dataPath, "Runtime", "Assembly-CSharp", "NetGen", "NetEnum", enumType.Name + "_NetEnum.cs");

			using (StreamWriter fileWriter = new StreamWriter(path))
			{
				NetGenWriter writer = new NetGenWriter(fileWriter);
				writer.WriteLine("using SDG.NetPak;");
				writer.WriteLine("namespace SDG.Unturned");
				writer.WriteLine('{');
				writer.Indent();
				writer.WriteLine($"public static class {enumType.Name}_NetEnum");
				writer.WriteLine('{');
				writer.Indent();

				writer.WriteLine($"public static bool ReadEnum(this NetPakReader reader, out {enumType.Name} value)");
				writer.WriteLine('{');
				writer.Indent();

				writer.WriteLine("uint index;");
				writer.WriteLine($"bool result = reader.ReadBits({bitsRequired}, out index);");

				if (maxEnumIndex == maxBitsValue)
				{
					// Index is guaranteed to be valid.
					writer.WriteLine($"value = ({enumType.Name}) index;");
					writer.WriteLine("return result;");
				}
				else
				{
					writer.WriteLine("// Casting out of range index to enum would throw an exception.");
					writer.WriteLine($"if (index <= {maxEnumIndex})");
					writer.WriteLine('{');
					writer.Indent();
					writer.WriteLine($"value = ({enumType.Name}) index;");
					writer.WriteLine("return result;");
					writer.Outdent();
					writer.WriteLine('}');
					writer.WriteLine("else");
					writer.WriteLine('{');
					writer.Indent();
					writer.WritePreprocessorLine("#if WITH_NETPAK_EXCEPTIONS");
					writer.WriteLine("throw new System.IndexOutOfRangeException();");
					writer.WritePreprocessorLine("#else");
					writer.WriteLine("value = default;");
					writer.WriteLine("return false;");
					writer.WritePreprocessorLine("#endif // WITH_NETPAK_EXCEPTIONS");
					writer.Outdent();
					writer.WriteLine('}');
				}

				writer.Outdent();
				writer.WriteLine('}'); // read

				writer.WriteLine($"public static bool WriteEnum(this NetPakWriter writer, {enumType.Name} value)");
				writer.WriteLine('{');
				writer.Indent();

				writer.WriteLine("uint index = (uint) value;");
				writer.WritePreprocessorLine("#if WITH_NETPAK_EXCEPTIONS");
				writer.WriteLine($"if (index > {maxEnumIndex})");
				writer.Indent();
				writer.WriteLine("throw new System.IndexOutOfRangeException();");
				writer.Outdent();
				writer.WritePreprocessorLine("#endif // WITH_NETPAK_EXCEPTIONS");
				writer.WriteLine($"return writer.WriteBits(index, {bitsRequired});");

				writer.Outdent();
				writer.WriteLine('}'); // write

				writer.Outdent();
				writer.WriteLine('}'); // class
				writer.Outdent();
				writer.WriteLine('}'); // namespace
			}
		}

	}
}
