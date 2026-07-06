////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.SystemTypes
{
	public class KeyValueTableGUIDReader : IFormattedTypeReader
	{
		public object read(IFormattedFileReader reader)
		{
			string value = reader.readValue();
			if (string.IsNullOrEmpty(value) || value.Equals("0"))
			{
				// Allows null asset references.
				return Guid.Empty;
			}
			else
			{
				return new Guid(value);
			}
		}
	}
}
