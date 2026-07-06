////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define USE_UNITY_LOG
#endif

using UnityEngine;

namespace SDG.Unturned
{
	public enum ELogType
	{
		Info,
		Warn,
		Error,
	}

	/// <summary>
	/// Unturned wrapper for Debug.Log, Debug.LogWarning, Debug.LogError, etc.
	/// </summary>
	public static class UnturnedLog
	{
		public static void info(string message)
		{
			if (insideLog)
			{
				// Prevent recursion, e.g. we called CommandWindow and now CommandWindow called us.
				return;
			}

			try
			{
				insideLog = true;

#if USE_UNITY_LOG
				Debug.Log(message);
#endif

				Logs.printLine(message);
			}
			finally
			{
				insideLog = false;
			}
		}

		public static void warn(string message)
		{
			if (insideLog)
			{
				// Prevent recursion, e.g. we called CommandWindow and now CommandWindow called us.
				return;
			}

			try
			{
				insideLog = true;

#if USE_UNITY_LOG
				Debug.LogWarning(message);
#endif

				Logs.printLine(message);
				ContinuousIntegration.reportFailure(message);
			}
			finally
			{
				insideLog = false;
			}
		}

		public static void error(string message)
		{
			if (insideLog)
			{
				// Prevent recursion, e.g. we called CommandWindow and now CommandWindow called us.
				return;
			}

			try
			{
				insideLog = true;

#if USE_UNITY_LOG
				Debug.LogError(message);
#endif

				Logs.printLine(message);
				CommandWindow.LogError(message);
				ContinuousIntegration.reportFailure(message);
			}
			finally
			{
				insideLog = false;
			}
		}

		public static void exception(System.Exception e)
		{
			if (e == null)
			{
				error("UnturnedLog.exception called with null argument");
				return;
			}

			if (insideLog)
			{
				// Prevent recursion, e.g. we called CommandWindow and now CommandWindow called us.
				return;
			}

			try
			{
				insideLog = true;

#if USE_UNITY_LOG
				Debug.LogException(e);
#endif

				internalException(e);
			}
			finally
			{
				insideLog = false;
			}
		}

		/// <summary>
		/// Log an exception with message providing context.
		/// </summary>
		public static void exception(System.Exception e, string message)
		{
			error(message);
			exception(e);
		}

		/// <summary>
		/// Recursively logs inner exception.
		/// 
		/// Should only be called by itself and exception because notifications
		/// to CommandWindow would otherwise get re-sent here as errors.
		/// </summary>
		private static void internalException(System.Exception e)
		{
			string message = e.Message;
			if (string.IsNullOrEmpty(message))
			{
				message = "(empty exception message)";
			}

			string stackTrace = e.StackTrace;
			if (string.IsNullOrEmpty(stackTrace))
			{
				stackTrace = "(empty stack trace)";
			}

			Logs.printLine(message);
			Logs.printLine(stackTrace);

			CommandWindow.LogError(message);
			CommandWindow.LogError(stackTrace);

			ContinuousIntegration.reportFailure(message);
			ContinuousIntegration.reportFailure(stackTrace);

			if (e.InnerException != null)
			{
				internalException(e.InnerException);
			}
		}

		private static bool insideLog = false;

		/// <summary>
		/// This is the ONLY place Unturned should be binding logMessageReceived.
		///
		/// This gives us greater control over how logging is handled. In particular, Unity's
		/// headless builds route logs (including stack traces) through stdout which is undesirable
		/// for dedicated servers, so we only call Debug.Log* in the editor and development builds. 
		/// </summary>
		private static void onBuiltinUnityLogMessageReceived(string text, string stack, LogType type)
		{
			if (insideLog)
			{
				// Was called by info/warn/error and should not be resent to Logs or CI.
				return;
			}

			Logs.printLine(text);

			switch (type)
			{
				case LogType.Assert:
				case LogType.Error:
				case LogType.Exception:
				case LogType.Warning:
					ContinuousIntegration.reportFailure(text);
					Logs.printLine(stack);
					break;
			}
		}

		static UnturnedLog()
		{
			// This is the ONLY place Unturned should be binding logMessageReceived.
			Application.logMessageReceived += onBuiltinUnityLogMessageReceived;
		}

		#region object helpers
		public static void info(object message)
		{
			if (message != null)
			{
				info(message.ToString());
			}
		}

		public static void warn(object message)
		{
			if (message != null)
			{
				warn(message.ToString());
			}
		}

		public static void error(object message)
		{
			if (message != null)
			{
				error(message.ToString());
			}
		}
		#endregion object
		#region string.Format helpers
		public static void info(string format, params object[] args)
		{
			info(string.Format(format, args));
		}

		public static void warn(string format, params object[] args)
		{
			warn(string.Format(format, args));
		}

		public static void error(string format, params object[] args)
		{
			error(string.Format(format, args));
		}

		/// <summary>
		/// Log an exception with message providing context.
		/// </summary>
		public static void exception(System.Exception e, string format, params object[] args)
		{
			try
			{
				error(string.Format(format, args));
			}
			catch
			{
				// Ignore. There was an actual bug where logging exception threw another
				// exception and broke most of the main menu. (private issue #1909)
			}
			exception(e);
		}
		#endregion
	}
}
