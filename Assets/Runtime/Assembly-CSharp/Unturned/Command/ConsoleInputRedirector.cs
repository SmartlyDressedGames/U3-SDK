////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace SDG.Unturned
{
	public class ConsoleInputRedirector
	{
		public void enable()
		{
			if (defaultInputReader != null)
				return; // Already enabled.

			defaultInputReader = Console.In;

			standardInputStream = Console.OpenStandardInput();
			standardInputReader = new StreamReader(standardInputStream, Console.InputEncoding);
			Console.SetIn(standardInputReader);
		}

		public void disable()
		{
			if (standardInputReader != null)
			{
				standardInputReader.Close();
				standardInputReader.Dispose();
				standardInputReader = null;
			}

			if (standardInputStream != null)
			{
				standardInputStream.Close();
				standardInputStream.Dispose();
				standardInputStream = null;
			}

			if (defaultInputReader != null)
			{
				Console.SetIn(defaultInputReader);
				defaultInputReader = null;
			}
		}

		private Stream standardInputStream;
		private StreamReader standardInputReader;

		private TextReader defaultInputReader;
	}
}
