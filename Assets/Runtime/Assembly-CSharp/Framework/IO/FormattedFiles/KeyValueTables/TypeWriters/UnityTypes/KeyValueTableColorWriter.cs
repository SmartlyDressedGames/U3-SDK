////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes
{
	public class KeyValueTableColorWriter : IFormattedTypeWriter
	{
		public void write(IFormattedFileWriter writer, object value)
		{
			writer.beginObject();

			Color32 vector = (Color32) (Color) value;
			writer.writeValue("R", vector.r);
			writer.writeValue("G", vector.g);
			writer.writeValue("B", vector.b);
			writer.writeValue("A", vector.a);

			writer.endObject();
		}
	}
}
