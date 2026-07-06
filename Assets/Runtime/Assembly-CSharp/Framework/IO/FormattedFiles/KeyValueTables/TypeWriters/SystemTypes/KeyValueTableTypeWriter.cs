////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.SystemTypes
{
	public class KeyValueTableTypeWriter : IFormattedTypeWriter
	{
		public void write(IFormattedFileWriter writer, object value)
		{
			Type type = value as Type;
			writer.writeValue(type.AssemblyQualifiedName);
		}
	}
}
