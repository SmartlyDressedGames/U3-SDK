////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace SDG.Unturned
{
	/// <summary>
	/// Read commands from standard input, and write logs to standard output.
	/// </summary>
	public class ConsoleInputOutputBase : ICommandInputOutput
	{
		public static CommandLineFlag defaultShouldRedirectInput = new CommandLineFlag(true, "-NoRedirectConsoleInput");
		public static CommandLineFlag defaultShouldRedirectOutput = new CommandLineFlag(true, "-NoRedirectConsoleOutput");
		public static CommandLineFlag defaultShouldProxyRedirectedOutput = new CommandLineFlag(false, "-ProxyRedirectedConsoleOutput");

		public ConsoleInputOutputBase()
		{
			shouldRedirectInput = defaultShouldRedirectInput;
			shouldRedirectOutput = defaultShouldRedirectOutput;
			shouldProxyRedirectedOutput = defaultShouldProxyRedirectedOutput;
		}

		public virtual void initialize(CommandWindow commandWindow)
		{
			desiredTitle = string.Empty;
			commandWindow.onTitleChanged += onTitleChanged;
			onTitleChanged(commandWindow.title);
			Console.CancelKeyPress += handleCancelEvent;

			if (shouldRedirectInput)
			{
				inputRedirector = new ConsoleInputRedirector();
				inputRedirector.enable();
			}

			if (shouldRedirectOutput)
			{
				outputRedirector = new ConsoleOutputRedirector();
				outputRedirector.enable(shouldProxyRedirectedOutput);
			}

			UnturnedLog.info("Console output encoding: {0}", Console.OutputEncoding?.EncodingName);

			// System.Text.Encoding.UTF8 has byte order mark (BOM) enabled by default,
			// but we don't want those two bytes displaying as gibberish in the console.
			const bool encoderShouldEmitUTF8Identifier = false; // Disable BOM
			Console.OutputEncoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier);
		}

		public virtual void shutdown(CommandWindow commandWindow)
		{
			commandWindow.onTitleChanged -= onTitleChanged;
			Console.CancelKeyPress -= handleCancelEvent;

			if (inputRedirector != null)
			{
				inputRedirector.disable();
				inputRedirector = null;
			}

			if (outputRedirector != null)
			{
				outputRedirector.disable();
				outputRedirector = null;
			}
		}

		public virtual void update()
		{
			if (wantsToTerminate > 0 && !isTerminating)
			{
				isTerminating = true;
				handleTermination();
			}
		}

		public event CommandInputHandler inputCommitted;

		public virtual void outputInformation(string information)
		{
			outputToConsole(information, ConsoleColor.Gray);
		}

		public virtual void outputWarning(string warning)
		{
			outputToConsole(warning, ConsoleColor.Yellow);
		}

		public virtual void outputError(string error)
		{
			outputToConsole(error, ConsoleColor.Red);
		}

		public bool shouldRedirectInput;
		public bool shouldRedirectOutput;
		public bool shouldProxyRedirectedOutput;

		protected virtual void outputToConsole(string value, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(value);
		}

		/// <summary>
		/// Broadcast the inputCommited event.
		/// </summary>
		protected void notifyInputCommitted(string input)
		{
			inputCommitted?.Invoke(input);
		}

		/// <summary>
		/// Synchronize console's title bar text.
		/// Virtual because at one point Win32 SetTitleText was required.
		/// </summary>
		protected virtual void synchronizeTitle(string title)
		{
			Console.Title = desiredTitle;
		}

		protected virtual void onTitleChanged(string title)
		{
			desiredTitle = title;

			if (string.IsNullOrEmpty(desiredTitle) == false)
			{
				synchronizeTitle(desiredTitle);
			}
		}

		/// <summary>
		/// Intercept the Ctrl-C or Ctrl-Break termination.
		/// </summary>
		protected virtual void handleCancelEvent(object sender, ConsoleCancelEventArgs args)
		{
			// Setting to true allows the application to continue executing.
			// Unturned will terminate gracefully via the shutdown system.
			args.Cancel = true;

			// Windows calls this event from a thread pool thread, so we need to wait
			// until back on the main thread before running the shutdown routine.
			System.Threading.Interlocked.Exchange(ref wantsToTerminate, 1);
		}

		/// <summary>
		/// Handle Ctrl-C or Ctrl-Break on the game thread.
		/// </summary>
		protected virtual void handleTermination()
		{
			CommandWindow.Log("Handling SIGINT or SIGBREAK by requesting a graceful shutdown");
			Provider.shutdown();
		}

		/// <summary>
		/// Has Ctrl-C or Ctrl-Break signal been received?
		/// </summary>
		protected int wantsToTerminate;

		/// <summary>
		/// Is the Ctrl-C or Ctrl-Break signal being handled?
		/// </summary>
		protected bool isTerminating;

		protected string desiredTitle;

		private ConsoleInputRedirector inputRedirector;
		private ConsoleOutputRedirector outputRedirector;
	}
}
