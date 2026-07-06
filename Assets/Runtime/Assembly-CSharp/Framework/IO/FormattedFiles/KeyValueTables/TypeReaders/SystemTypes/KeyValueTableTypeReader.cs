////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.SystemTypes
{
	public class KeyValueTableTypeReader : IFormattedTypeReader
	{
		public object read(IFormattedFileReader reader)
		{
			string assemblyQualifiedName = reader.readValue();
			if (string.IsNullOrEmpty(assemblyQualifiedName))
				return null;

			assemblyQualifiedName = KeyValueTableTypeRedirectorRegistry.chase(assemblyQualifiedName);
			if (assemblyQualifiedName.IndexOfAny(SDG.Unturned.DatValue.INVALID_TYPE_CHARS) >= 0)
			{
				return null;
			}

			return Type.GetType(assemblyQualifiedName);
		}
	}
}
