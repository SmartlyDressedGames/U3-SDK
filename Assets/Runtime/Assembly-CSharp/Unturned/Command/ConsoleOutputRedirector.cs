////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace SDG.Unturned
{
	public class ConsoleOutputRedirector
	{
		public void enable(bool shouldProxy)
		{
			if (defaultOutputWriter != null)
				return; // Already enabled.

			defaultOutputWriter = Console.Out;

			standardOutputStream = Console.OpenStandardOutput();
			standardOutputWriter = new StreamWriter(standardOutputStream, Console.OutputEncoding);
			standardOutputWriter.AutoFlush = true;

			if (shouldProxy)
			{
				proxyWriter = new ConsoleWriterProxy(standardOutputWriter, defaultOutputWriter);
				Console.SetOut(proxyWriter);
			}
			else
			{
				Console.SetOut(standardOutputWriter);
			}
		}

		public void disable()
		{
			if (proxyWriter != null)
			{
				proxyWriter.Close();
				proxyWriter.Dispose();
				proxyWriter = null;
			}

			if (standardOutputWriter != null)
			{
				standardOutputWriter.Close();
				standardOutputWriter.Dispose();
				standardOutputWriter = null;
			}

			if (standardOutputStream != null)
			{
				standardOutputStream.Close();
				standardOutputStream.Dispose();
				standardOutputStream = null;
			}

			if (defaultOutputWriter != null)
			{
				Console.SetOut(defaultOutputWriter);
				defaultOutputWriter = null;
			}
		}

		private Stream standardOutputStream;
		private StreamWriter standardOutputWriter;
		private ConsoleWriterProxy proxyWriter;

		private TextWriter defaultOutputWriter;
	}
}
