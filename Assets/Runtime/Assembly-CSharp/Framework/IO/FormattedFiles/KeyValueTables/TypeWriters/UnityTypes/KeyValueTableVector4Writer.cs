////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes
{
	public class KeyValueTableVector4Writer : IFormattedTypeWriter
	{
		public void write(IFormattedFileWriter writer, object value)
		{
			writer.beginObject();

			Vector4 vector = (Vector4) value;
			writer.writeValue("X", vector.x);
			writer.writeValue("Y", vector.y);
			writer.writeValue("Z", vector.z);
			writer.writeValue("W", vector.w);

			writer.endObject();
		}
	}
}
