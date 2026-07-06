////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes
{
	public class KeyValueTableQuaternionWriter : IFormattedTypeWriter
	{
		public void write(IFormattedFileWriter writer, object value)
		{
			writer.beginObject();

			Quaternion quaternion = (Quaternion) value;
			writer.writeValue("X", quaternion.x);
			writer.writeValue("Y", quaternion.y);
			writer.writeValue("Z", quaternion.z);
			writer.writeValue("W", quaternion.w);

			writer.endObject();
		}
	}
}
