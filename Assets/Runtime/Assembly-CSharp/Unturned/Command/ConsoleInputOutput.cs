////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_CONSOLE_IO

using System;

namespace SDG.Unturned
{
	/// <summary>
	/// Read commands from standard input, and write logs to standard output.
	/// </summary>
	public class ConsoleInputOutput : ConsoleInputOutputBase
	{
		public override void update()
		{
			base.update();

			inputFromConsole();
		}

		protected void clearLine()
		{
			Console.CursorLeft = 0;
			Console.Write(new string(' ', Console.BufferWidth));
			Console.CursorTop--;
			Console.CursorLeft = 0;
		}

		protected void redrawInputLine()
		{
			if (Console.CursorLeft > 0)
			{
				clearLine();
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(pendingInput);
		}

		protected override void outputToConsole(string value, ConsoleColor color)
		{
			if (Console.CursorLeft != 0)
				clearLine();

			base.outputToConsole(value, color);

			redrawInputLine();
		}

		/// <summary>
		/// Each Update we consume a key press from the console buffer if available.
		/// Unfortunately ReadLine is not an option without blocking output, so we maintain our own pending input.
		/// </summary>
		protected virtual void inputFromConsole()
		{
			if (Console.KeyAvailable)
			{
				ConsoleKeyInfo keyInfo = Console.ReadKey();

#if LOG_CONSOLE_IO
				UnturnedLog.info("LOG_CONSOLE_IO Key: {0} Char: {1} Modifiers: {2}", keyInfo.Key, keyInfo.KeyChar, keyInfo.Modifiers);
#endif

				onConsoleInputKey(keyInfo);
			}
		}

		protected virtual void onConsoleInputKey(ConsoleKeyInfo keyInfo)
		{
			ConsoleKey key = keyInfo.Key;

			if (key == ConsoleKey.Enter)
			{
				onConsoleInputEnter();
			}
			else if (key == ConsoleKey.Backspace)
			{
				onConsoleInputBackspace();
			}
			else if (key == ConsoleKey.Escape)
			{
				onConsoleInputEscape();
			}
			else if (keyInfo.KeyChar != '\u0000')
			{
				pendingInput += keyInfo.KeyChar;
				redrawInputLine();
			}
		}

		protected virtual void onConsoleInputEnter()
		{
#if LOG_CONSOLE_IO
			UnturnedLog.info("LOG_CONSOLE_IO Enter");
#endif

			// Output redraws input, so we clear beforehand.
			string input = pendingInput;
			pendingInput = string.Empty;

			clearLine();
			outputInformation('>' + input);
			notifyInputCommitted(input);
		}

		protected virtual void onConsoleInputBackspace()
		{
#if LOG_CONSOLE_IO
			UnturnedLog.info("LOG_CONSOLE_IO Backspace");
#endif

			int length = pendingInput.Length;
			switch (length)
			{
				case 0:
					return;

				case 1:
					pendingInput = string.Empty;
					break;

				default:
					pendingInput = pendingInput.Substring(0, length - 1);
					break;
			}

			redrawInputLine();
		}

		protected virtual void onConsoleInputEscape()
		{
#if LOG_CONSOLE_IO
			UnturnedLog.info("LOG_CONSOLE_IO Escape");
#endif

			if (pendingInput.Length < 1)
				return;

			pendingInput = string.Empty;
			clearLine();
		}

		protected string pendingInput = string.Empty;
	}
}
