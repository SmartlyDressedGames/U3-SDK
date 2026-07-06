////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;

namespace SDG.Unturned
{
	public class ConsoleWriterProxy : TextWriter
	{
		public ConsoleWriterProxy(StreamWriter streamWriter, TextWriter defaultConsoleWriter)
		{
			customWriter = streamWriter;
			this.defaultConsoleWriter = defaultConsoleWriter;
		}

		public override void Close()
		{
			customWriter.Close();
			defaultConsoleWriter.Close();
		}

		public override Encoding Encoding => defaultConsoleWriter.Encoding;

		public override void Flush()
		{
			customWriter.Flush();
			defaultConsoleWriter.Flush();
		}

		public override IFormatProvider FormatProvider => defaultConsoleWriter.FormatProvider;

		public override string NewLine
		{
			get => defaultConsoleWriter.NewLine;
			set => defaultConsoleWriter.NewLine = value;
		}

		/// <summary>
		/// This is the only /required/ override of text writer.
		/// </summary>
		public override void Write(char value)
		{
			customWriter.Write(value);
			defaultConsoleWriter.Write(value);
		}

		public override void WriteLine()
		{
			customWriter.WriteLine();
			defaultConsoleWriter.WriteLine();
		}

		public override void WriteLine(string value)
		{
			customWriter.WriteLine(value);
			defaultConsoleWriter.WriteLine(value);
		}

		protected TextWriter defaultConsoleWriter;
		private StreamWriter customWriter;
	}
}
