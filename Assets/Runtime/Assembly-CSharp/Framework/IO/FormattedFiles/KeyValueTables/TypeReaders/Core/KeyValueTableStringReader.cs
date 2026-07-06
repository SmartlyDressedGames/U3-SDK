////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes
{
	public class KeyValueTableStringReader : IFormattedTypeReader
	{
		public object read(IFormattedFileReader reader)
		{
			return reader.readValue();
		}
	}
}
