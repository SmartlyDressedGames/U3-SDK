////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public delegate void CommandWindowInputted(string text, ref bool shouldExecuteCommand);
	public delegate void CommandWindowOutputted(object text, ConsoleColor color);
	public delegate void CommandWindowTitleChanged(string title);

	public class CommandWindow
	{
		public static CommandWindowInputted onCommandWindowInputted;
		public static CommandWindowOutputted onCommandWindowOutputted;

		/// <summary>
		/// Broadcasts after dedicated server name changes.
		/// Command IO interface binds to this rather than having a title-specific method.
		/// </summary>
		public event CommandWindowTitleChanged onTitleChanged;

		/// <summary>
		/// Should the default console I/O handler be created?
		/// Plugins can disable on the command line when overriding handler.
		/// </summary>
		public static CommandLineFlag shouldCreateDefaultConsole = new CommandLineFlag(true, "-NoDefaultConsole");

		/// <summary>
		/// Should the legacy blocking (game thread) console be created?
		/// </summary>
		private static CommandLineFlag shouldCreateLegacyConsole = new CommandLineFlag(false, "-LegacyConsole");

		private string _title;
		public string title
		{
			get => _title;

			set
			{
				_title = value;
				onTitleChanged?.Invoke(_title);
			}
		}

		public static bool shouldLogChat = true;
		public static bool shouldLogJoinLeave = true;
		public static bool shouldLogDeaths = true;
		public static bool shouldLogAnticheat = false;

		/// <summary>
		/// Log white information.
		/// </summary>
		public static void Log(object text)
		{
			if (text == null)
			{
				// Ignore, don't call ToString(). (public issue #4222)
				return;
			}

			if (insideExplicitLogging)
			{
				// Are one of our static CommandWindow.* functions being called right now?
				// They also call UnturnedLog so that command output is available in log files,
				// but we do not want to write that callback to the console a second time.
				return;
			}

			try
			{
				// Notify logging system of this message so that it will be written to disk,
				// however we will get a callback so we guard against logging twice with insideExplicitLogging.
				insideExplicitLogging = true;
				UnturnedLog.info(text);
				Dedicator.commandWindow?.internalLogInformation(text.ToString());
			}
			finally
			{
				insideExplicitLogging = false;
			}
		}

		public static void LogFormat(string format, params object[] args)
		{
			Log(string.Format(format, args));
		}

		/// <summary>
		/// Log yellow warning.
		/// </summary>
		public static void LogWarning(object text)
		{
			if (text == null)
			{
				// Ignore, don't call ToString(). (public issue #4222)
				return;
			}

			if (insideExplicitLogging)
			{
				// Are one of our static CommandWindow.* functions being called right now?
				// They also call UnturnedLog so that command output is available in log files,
				// but we do not want to write that callback to the console a second time.
				return;
			}

			try
			{
				// Notify logging system of this message so that it will be written to disk,
				// however we will get a callback so we guard against logging twice with insideExplicitLogging.
				insideExplicitLogging = true;
				UnturnedLog.warn(text);
				Dedicator.commandWindow?.internalLogWarning(text.ToString());
			}
			finally
			{
				insideExplicitLogging = false;
			}
		}

		public static void LogWarningFormat(string format, params object[] args)
		{
			LogWarning(string.Format(format, args));
		}

		/// <summary>
		/// Log red error.
		/// </summary>
		public static void LogError(object text)
		{
			if (text == null)
			{
				// Ignore, don't call ToString(). (public issue #4222)
				return;
			}

			if (insideExplicitLogging)
			{
				// Are one of our static CommandWindow.* functions being called right now?
				// They also call UnturnedLog so that command output is available in log files,
				// but we do not want to write that callback to the console a second time.
				return;
			}

			try
			{
				// Notify logging system of this message so that it will be written to disk,
				// however we will get a callback so we guard against logging twice with insideExplicitLogging.
				insideExplicitLogging = true;
				UnturnedLog.error(text);
				Dedicator.commandWindow?.internalLogError(text.ToString());
			}
			finally
			{
				insideExplicitLogging = false;
			}
		}

		public static void LogErrorFormat(string format, params object[] args)
		{
			LogError(string.Format(format, args));
		}

		private static bool insideExplicitLogging = false;

		/// <summary>
		/// Print white message to console.
		/// </summary>
		private void internalLogInformation(string information)
		{
			try
			{
				onCommandWindowOutputted?.Invoke(information, ConsoleColor.White);
			}
			catch (Exception e)
			{
				HandleException("Plugin threw an exception from info onCommandWindowOutputted:", e);
			}

			foreach (ICommandInputOutput handler in ioHandlers)
			{
				try
				{
					handler.outputInformation(information);
				}
				catch (Exception e)
				{
					HandleException($"Command IO handler {handler} threw an exception from outputInformation:", e);
				}
			}
		}

		/// <summary>
		/// Print yellow message to console.
		/// </summary>
		private void internalLogWarning(string warning)
		{
			try
			{
				onCommandWindowOutputted?.Invoke(warning, ConsoleColor.Yellow);
			}
			catch (Exception e)
			{
				HandleException("Plugin threw an exception from warning onCommandWindowOutputted:", e);
			}

			foreach (ICommandInputOutput handler in ioHandlers)
			{
				try
				{
					handler.outputWarning(warning);
				}
				catch (Exception e)
				{
					HandleException($"Command IO handler {handler} threw an exception from outputWarning:", e);
				}
			}
		}

		/// <summary>
		/// Print red message to console.
		/// </summary>
		private void internalLogError(string error)
		{
			try
			{
				onCommandWindowOutputted?.Invoke(error, ConsoleColor.Red);
			}
			catch (Exception e)
			{
				HandleException("Plugin threw an exception from error onCommandWindowOutputted:", e);
			}

			foreach (ICommandInputOutput handler in ioHandlers)
			{
				try
				{
					handler.outputError(error);
				}
				catch (Exception e)
				{
					HandleException($"Command IO handler {handler} threw an exception from outputError:", e);
				}
			}
		}

		private void onInputCommitted(string input)
		{
			bool shouldExecuteCommand = true;

			try
			{
				onCommandWindowInputted?.Invoke(input, ref shouldExecuteCommand);
			}
			catch (Exception e)
			{
				HandleException("Plugin threw an exception from onCommandWindowInputted:", e);
			}

			if (shouldExecuteCommand)
			{
				if (!Commander.execute(CSteamID.Nil, input))
				{
					LogErrorFormat("Unable to match \"{0}\" with any built-in commands", input);
				}
			}
		}

		/// <summary>
		/// Cannot use UnturnedLog here because it may recursively call CommandWindow if another exception is thrown.
		/// </summary>
		private void HandleException(string message, Exception exception)
		{
			Logs.printLine(message);
			do
			{
				Logs.printLine(exception.Message);
				Logs.printLine(exception.StackTrace);
				exception = exception.InnerException;
			}
			while (exception != null);
		}

		/// <summary>
		/// Called during Unity Update loop.
		/// </summary>
		public void update()
		{
			foreach (ICommandInputOutput handler in ioHandlers)
			{
				try
				{
					handler.update();
				}
				catch (Exception e)
				{
					HandleException($"Command IO handler {handler} threw an exception from update:", e);
				}
			}
		}

		private void initializeIOHandler(ICommandInputOutput handler)
		{
			try
			{
				handler.initialize(this);
			}
			catch (Exception initializationException)
			{
				UnturnedLog.exception(initializationException);
			}
			handler.inputCommitted += onInputCommitted;
		}

		private void shutdownIOHandler(ICommandInputOutput handler)
		{
			handler.inputCommitted -= onInputCommitted;
			try
			{
				handler.shutdown(this);
			}
			catch (Exception shutdownException)
			{
				UnturnedLog.exception(shutdownException);
			}
		}

		/// <summary>
		/// Called during OnApplicationQuit.
		/// </summary>
		public void shutdown()
		{
			// Clear beforehand to prevent logging exceptions to shutdown handlers.
			List<ICommandInputOutput> oldHandlers = new List<ICommandInputOutput>(ioHandlers);
			ioHandlers.Clear();

			foreach (ICommandInputOutput handler in oldHandlers)
			{
				shutdownIOHandler(handler);
			}
		}

		/// <summary>
		/// Helper for plugins that want to replace the default without the shouldCreateDefaultConsole flag.
		/// </summary>
		public void removeDefaultIOHandler()
		{
			if (defaultIOHandler != null)
			{
				removeIOHandler(defaultIOHandler);
				defaultIOHandler = null;
			}
		}

		public void removeIOHandler(ICommandInputOutput handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");

			// Remove beforehand to prevent logging exception to our shutdown handler.
			ioHandlers.RemoveFast(handler);
			shutdownIOHandler(handler);
		}

		public void addIOHandler(ICommandInputOutput handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");

			if (ioHandlers.Contains(handler))
				throw new NotSupportedException("handler already registered");

			ioHandlers.Add(handler);
			initializeIOHandler(handler);
		}

		[System.Obsolete("Use addIOHandler instead (multiple simultaneous handlers now supported)")]
		public void setIOHandler(ICommandInputOutput newHandler)
		{
			addIOHandler(newHandler);
		}

		protected ICommandInputOutput createDefaultIOHandler()
		{
			if (shouldCreateDefaultConsole == false)
				return null;

#if UNITY_STANDALONE_WIN
			if (shouldCreateLegacyConsole)
			{
				return new WindowsConsoleInputOutput();
			}
			else
			{
				return new ThreadedWindowsConsoleInputOutput();
			}
#elif UNITY_STANDALONE_LINUX
			if(shouldCreateLegacyConsole)
			{
				return new LegacyInputOutput();
			}
			else
			{
				return new ThreadedConsoleInputOutput();
			}
#else
			return null;
#endif
		}

		public CommandWindow()
		{
			defaultIOHandler = createDefaultIOHandler();
			if (defaultIOHandler != null)
			{
				addIOHandler(defaultIOHandler);
			}
		}

		private List<ICommandInputOutput> ioHandlers = new List<ICommandInputOutput>();
		private ICommandInputOutput defaultIOHandler = null;
	}
}
