////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace Unturned.SteamCmd
{
	public class VdfWriter
	{
		public void WriteStartBlock(object key)
		{
			textWriter.WriteLine(indentationPrefix + key.ToString());
			textWriter.WriteLine(indentationPrefix + '{');
			indentationPrefix += '\t';
		}

		public void WriteKeyValue(object key, object value)
		{
			string line = value != null ?
				$"\"{key}\" \"{value}\"" :
				$"\"{key}\" \"\"";
			textWriter.WriteLine(indentationPrefix + line);
		}

		public void WriteKeyValue(object key, bool value)
		{
			int valueInt = value ? 1 : 0;
			string line = $"\"{key}\" \"{valueInt}\"";
			textWriter.WriteLine(indentationPrefix + line);
		}

		public void WriteEndBlock()
		{
			indentationPrefix = indentationPrefix.Remove(indentationPrefix.Length - 1, 1);
			textWriter.WriteLine(indentationPrefix + '}');
		}

		public VdfWriter(TextWriter textWriter)
		{
			this.textWriter = textWriter;
			indentationPrefix = string.Empty;
		}

		private TextWriter textWriter;
		private string indentationPrefix;
	}
}
