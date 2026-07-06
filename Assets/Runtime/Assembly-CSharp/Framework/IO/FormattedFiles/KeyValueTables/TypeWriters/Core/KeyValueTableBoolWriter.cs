////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.CoreTypes
{
	public class KeyValueTableBoolWriter : IFormattedTypeWriter
	{
		public void write(IFormattedFileWriter writer, object value)
		{
			bool state = (bool) value;
			writer.writeValue(state ? "true" : "false");
		}
	}
}
