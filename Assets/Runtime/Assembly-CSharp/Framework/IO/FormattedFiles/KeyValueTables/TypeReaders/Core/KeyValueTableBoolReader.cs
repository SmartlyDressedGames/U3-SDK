////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes
{
	public class KeyValueTableBoolReader : IFormattedTypeReader
	{
		public object read(IFormattedFileReader reader)
		{
			string value = reader.readValue();
			if (value == null)
			{
				return default(bool);
			}

			if (value.Equals("false", System.StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			else if (value.Equals("true", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (value.Equals("0", System.StringComparison.OrdinalIgnoreCase) || value.Equals("no", System.StringComparison.OrdinalIgnoreCase) || value.Equals("n", System.StringComparison.OrdinalIgnoreCase) || value.Equals("f", System.StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			else if (value.Equals("1", System.StringComparison.OrdinalIgnoreCase) || value.Equals("yes", System.StringComparison.OrdinalIgnoreCase) || value.Equals("y", System.StringComparison.OrdinalIgnoreCase) || value.Equals("t", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else
			{
				return default(bool);
			}
		}
	}
}
