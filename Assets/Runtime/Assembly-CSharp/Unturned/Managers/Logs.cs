////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SDG.Unturned
{
	public class LogFile
	{
		/// <summary>
		/// Absolute path to *.log file.
		/// </summary>
		public string path
		{
			get;
			private set;
		}

		public LogFile(string path)
		{
			this.path = path;

			// Read share permission is important for external applications that watch our log files,
			// but write permission may be used while running multiple clients.
			stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

			// UTF8 encoding with fallback exceptions disabled because bad strings were causing the server to lock up:
			// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2494
			Encoding encoding = Encoding.GetEncoding(/*UTF8*/ 65001, new EncoderReplacementFallback(), new DecoderReplacementFallback());

			writer = new StreamWriter(stream, encoding);
			writer.AutoFlush = true;
		}

		public void writeLine(string line)
		{
			writer.WriteLine(line);
		}

		public void close()
		{
			if (writer != null)
			{
				writer.Flush();
				writer.Close();
				writer.Dispose();
				writer = null;
			}

			if (stream != null)
			{
				stream.Close();
				stream.Dispose();
				stream = null;
			}
		}

		private FileStream stream;
		private StreamWriter writer;
	}

	/// <summary>
	/// Responsible for the per-process .log file in the Logs directory.
	/// Kept multiple log files in the past, but now consolidates all information
	/// into a single file named Client.log or Server_{Identifier}.log.
	/// </summary>
	public class Logs : MonoBehaviour
	{
		/// <summary>
		/// Should setup of the default *.log file be disabled?
		/// </summary>
		public static CommandLineFlag noDefaultLog = new CommandLineFlag(false, "-NoDefaultLog");

		private static CommandLineFlag shouldRedactLogs = new CommandLineFlag(true, "-UnredactedLogs");

		/// <summary>
		/// If true, information like IP addresses and login tokens should be censored in vanilla logs.
		/// (public issue #4740)
		/// </summary>
		public static bool ShouldRedactLogs => shouldRedactLogs.value;

		/// <summary>
		/// Text to replace with if <see cref="shouldRedactLogs"/> is enabled.
		/// </summary>
		public static string RedactionReplacement
		{
			get;
			set;
		} = "[redacted]";

		/// <summary>
		/// *ATTEMPTS* to replace IPv4 address(es) with <see cref="RedactionReplacement"/>.
		/// Should only be called if <see cref="ShouldRedactLogs"/> is enabled.
		/// Case-by-case redaction should be preferred for performance reasons over using this function. This function
		/// is intended for third-party messages (e.g., anti-cheat) that we don't have control over.
		/// </summary>
		/// <returns>True if message was modified.</returns>
		public static bool RedactIPv4Addresses(ref string message)
		{
			// Don't create string builder until needed.
			System.Text.StringBuilder sb = null;

			int redactionEndIndex = 0;
			int addressStartedIndex = -1;
			int prevDotCount = 0;
			int dotCount = 0;
			int numCount = 0;
			for (int charIndex = 0; charIndex < message.Length; ++charIndex)
			{
				if (char.IsDigit(message, charIndex))
				{
					if (addressStartedIndex < 0)
					{
						addressStartedIndex = charIndex;
						prevDotCount = 0;
						dotCount = 0;
						numCount = 1;
					}
					else
					{
						if (dotCount != prevDotCount)
						{
							prevDotCount = dotCount;
							++numCount;
						}
					}
				}
				else if (message[charIndex] == '.')
				{
					++dotCount;
				}
				else
				{
					if (addressStartedIndex >= 0)
					{
						if (numCount == 4)
						{
							if (sb == null)
							{
								sb = new StringBuilder(message.Length * 2);
							}

							sb.Append(message.Substring(redactionEndIndex, addressStartedIndex - redactionEndIndex));
							sb.Append(RedactionReplacement);

							if (dotCount > 3)
							{
								// IP ended with a period/fullstop.
								sb.Append('.');
							}

							redactionEndIndex = charIndex;
						}

						addressStartedIndex = -1;
					}
				}
			}

			if (addressStartedIndex >= 0)
			{
				// Was partway through an IP before end of text?
				if (numCount == 4)
				{
					if (sb == null)
					{
						sb = new StringBuilder(message.Length * 2);
					}

					sb.Append(message.Substring(redactionEndIndex, addressStartedIndex - redactionEndIndex));
					sb.Append(RedactionReplacement);

					if (dotCount > 3)
					{
						// IP ended with a period/fullstop.
						sb.Append('.');
					}

					redactionEndIndex = message.Length;
				}
			}

			if (redactionEndIndex < message.Length && sb != null)
			{
				// Append any remaining text.
				sb.Append(message.Substring(redactionEndIndex));
			}

			if (sb != null)
			{
				message = sb.ToString();
				return true;
			}
			else
			{
				return false;
			}
		}

		private static LogFile debugLog = null;

		public static void printLine(string message)
		{
			if (debugLog != null && !string.IsNullOrEmpty(message))
			{
				// Ignore message that is only newline.
				string trimmedMessage = message.Trim();
				if (!string.IsNullOrEmpty(trimmedMessage))
				{
					// Explicit ISO 8601 usage to make log files consistent between locales.
					string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
					debugLog.writeLine(string.Format("[{0}] {1}", time, trimmedMessage));
				}
			}
		}

		/// <summary>
		/// Get logging to path.
		/// </summary>
		public static string getLogFilePath()
		{
			return debugLog != null ? debugLog.path : null;
		}

		/// <summary>
		/// Set path to log to.
		/// </summary>
		public static void setLogFilePath(string logFilePath)
		{
			if (logFilePath.EndsWith(".log") == false)
			{
				throw new ArgumentException("should be a .log file", "logFilePath");
			}

			closeLogFile(); // Close if plugin is changing log file after startup.

			try
			{
				string logsDirectoryPath = Path.GetDirectoryName(logFilePath);
				if (!Directory.Exists(logsDirectoryPath))
				{
					Directory.CreateDirectory(logsDirectoryPath);
				}
			}
			catch (System.Exception exception)
			{
				Debug.LogException(exception);
			}

			// We catch exceptions here because there was a report of a player getting permission
			// errors from anti-virus breaking the log system and from there a bunch of startup issues.
			try
			{
				if (File.Exists(logFilePath))
				{
					string prev = logFilePath.Insert(logFilePath.Length - 4, "_Prev");

					// Move rather than Copy is faster if log file was insanely big. (e.g. from error spam)

					if (File.Exists(prev))
					{
						File.Delete(prev);
					}

					File.Move(logFilePath, prev);
				}
			}
			catch (System.Exception exception)
			{
				Debug.LogException(exception);
			}

			try
			{
				debugLog = new LogFile(logFilePath);
			}
			catch (System.Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		/// <summary>
		/// Close current log file.
		/// </summary>
		public static void closeLogFile()
		{
			if (debugLog != null)
			{
				debugLog.close();
				debugLog = null;
			}
		}

		public void awake()
		{
			if (noDefaultLog)
			{
				// Do not create default log file.
				// Plugin might create a custom setup.
				return;
			}

			string logFilePath = ReadWrite.PATH;
			if (Dedicator.IsDedicatedServer)
			{
				logFilePath += "/Logs/Server_" + Dedicator.serverID.Replace(' ', '_') + ".log";
			}
			else
			{
				logFilePath += "/Logs/Client.log";
			}

			double startTime = Time.realtimeSinceStartupAsDouble;

			setLogFilePath(logFilePath);

			double elapsedTime = Time.realtimeSinceStartupAsDouble - startTime;
			if (elapsedTime > 0.1)
			{
				// Narrow down if logging is making startup hang. One player had a multi-gigabyte
				// log file from error spam so my guess is that copying to the _Prev file took a while.
				UnturnedLog.info($"Initializing logging took {elapsedTime}s");
			}

			NetReflection.SetLogCallback(UnturnedLog.info);
		}

		private void OnDestroy()
		{
			closeLogFile();
		}
	}
}
