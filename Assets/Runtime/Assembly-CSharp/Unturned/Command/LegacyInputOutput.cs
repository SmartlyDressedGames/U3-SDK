////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	/// <summary>
	/// Matches the console behavior prior to command IO refactor.
	/// </summary>
	public class LegacyInputOutput : ICommandInputOutput
	{
		public virtual void initialize(CommandWindow commandWindow)
		{ }

		public virtual void shutdown(CommandWindow commandWindow)
		{ }

		public virtual void update()
		{ }

#pragma warning disable
		public event CommandInputHandler inputCommitted;
#pragma warning restore

		public virtual void outputInformation(string information)
		{
			outputToConsole(information, ConsoleColor.White);
		}

		public virtual void outputWarning(string warning)
		{
			outputToConsole(warning, ConsoleColor.Yellow);
		}

		public virtual void outputError(string error)
		{
			outputToConsole(error, ConsoleColor.Red);
		}

		protected virtual void outputToConsole(string value, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(value);
		}
	}
}
