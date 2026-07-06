////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System.IO;

namespace SDG.Framework.IO.Serialization
{
	public class JsonTextWriterFormatted : JsonTextWriter
	{
		public override void WriteStartArray()
		{
			base.Formatting = Formatting.None;
			base.WriteIndent();
			base.WriteStartArray();
			base.Formatting = Formatting.Indented;
		}

		public JsonTextWriterFormatted(TextWriter textWriter) : base(textWriter)
		{
			base.IndentChar = '\t';
			base.Indentation = 1;
		}
	}
}
