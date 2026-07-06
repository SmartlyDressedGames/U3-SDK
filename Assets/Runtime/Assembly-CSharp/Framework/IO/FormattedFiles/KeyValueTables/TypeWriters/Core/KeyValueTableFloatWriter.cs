////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.CoreTypes
{
	public class KeyValueTableFloatWriter : IFormattedTypeWriter
	{
		public void write(IFormattedFileWriter writer, object value)
		{
			float state = (float) value;
			string asString = state.ToString(System.Globalization.CultureInfo.InvariantCulture);
			writer.writeValue(asString);
		}
	}
}
