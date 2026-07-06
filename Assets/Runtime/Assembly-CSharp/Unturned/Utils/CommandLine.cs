////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public delegate void GetCommands(List<string> commands);

	public class CommandLine
	{
		/// <summary>
		/// Full argument string. Defaults to Environment.CommandLine.
		/// 
		/// Nelson 2025-06-17: By default, Steam shows a warning nowadays when the game is launched with externally-provided
		/// command-line arguments. For example, when joining a friend via rich presence. The solution is to use the arg
		/// string provided by SteamApps.GetLaunchCommandLine, which also supports *changing* the arguments while the app is
		/// running. If the environment-provided command-line doesn't contain it, the game will append Steam's launch options.
		///
		/// Note: Steam override isn't applied until Steam is initialized. (after Dedicator and ModuleManager) Please refer to
		/// Setup.cs for the full initialization order.
		/// </summary>
		public static string Get() => commandLineOverride;

		public static GetCommands onGetCommands;

		/// <summary>
		/// Nelson 2025-06-16: Steam doesn't handle "server code" connect URL, but we now support
		/// it for rich presence joins via server code for easier inviting friends to private servers.
		/// 
		/// When Steam parses a steam://connect/ip:port URL it requires the query port (e.g. 27015).
		/// </summary>
		public static bool TryGetSteamConnect(string line, out uint ip, out ushort queryPort, out string pass, out CSteamID serverCode)
		{
			ip = 0;
			queryPort = 0;
			pass = "";
			serverCode = CSteamID.Nil;

			TryParseValue(line, "+password", out pass);

			if (!TryParseValue(line, "+connect", out string connectString))
			{
				return false;
			}

			if (ulong.TryParse(connectString, out serverCode.m_SteamID))
			{
				return true;
			}

			if (IPv4Address.TryParseWithOptionalPort(connectString, out ip, out ushort? optionalPort)
				&& optionalPort.HasValue)
			{
				queryPort = optionalPort.Value;
				return true;
			}

			return false;
		}

		public static bool tryGetLobby(string line, out ulong lobby)
		{
			lobby = 0;

			int connect = line.ToLower().IndexOf("+connect_lobby ");
			if (connect != -1)
			{
				int space = line.IndexOf(' ', connect + 15);
				if (space == -1) // end of commandline
				{
					return ulong.TryParse(line.Substring(connect + 15, line.Length - connect - 15), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lobby);
				}
				else // partway through
				{
					return ulong.TryParse(line.Substring(connect + 15, space - connect - 15), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lobby);
				}
			}

			return false;
		}

		public static bool tryGetLanguage(out string local, out string path)
		{
			local = "";
			path = "";

			string[] arguments = System.Environment.GetCommandLineArgs();

			for (int index = 0; index < arguments.Length; index++)
			{
				string argLang = null;

				if (arguments[index].Length > 6 && (arguments[index].StartsWith("-Lang=", System.StringComparison.InvariantCultureIgnoreCase) || arguments[index].StartsWith("+Lang=", System.StringComparison.InvariantCultureIgnoreCase)))
				{
					argLang = arguments[index].Substring(6);
				}
				else if (arguments[index].Length > 5 && (arguments[index].StartsWith("-Loc=", System.StringComparison.InvariantCultureIgnoreCase) || arguments[index].StartsWith("+Loc=", System.StringComparison.InvariantCultureIgnoreCase)))
				{
					argLang = arguments[index].Substring(5);
				}
				else if (arguments[index].Length > 1 && arguments[index].StartsWith("+"))
				{
					if (arguments[index].IndexOf('/') >= 0)
					{
						// Actually a server launch argument.
						continue;
					}

					if (arguments[index].StartsWith("+connect") || arguments[index].StartsWith("+password"))
					{
						// Steam connection info.
						continue;
					}

					// Original way to set language, e.g. '+Russian'
					argLang = arguments[index].Substring(1);
				}

				if (string.IsNullOrEmpty(argLang))
					continue;

				if (Provider.provider.workshopService.ugc != null)
				{
					for (int check = 0; check < Provider.provider.workshopService.ugc.Count; check++)
					{
						SteamContent content = Provider.provider.workshopService.ugc[check];

						if (content.type == ESteamUGCType.LOCALIZATION)
						{
							if (ReadWrite.folderExists(content.path + "/" + argLang, false))
							{
								local = argLang;
								path = content.path + "/";
								UnturnedLog.info("Parsed language '{0}' on command-line, and found in workshop item {1}", argLang, content.publishedFileID);
								return true;
							}
						}
					}
				}

				if (ReadWrite.folderExists("/Localization/" + argLang))
				{
					local = argLang;
					path = ReadWrite.PATH + "/Localization/";
					UnturnedLog.info("Parsed language '{0}' on command-line, and found in root Localization directory", argLang);
					return true;
				}

				if (ReadWrite.folderExists("/Sandbox/" + argLang))
				{
					local = argLang;
					path = ReadWrite.PATH + "/Sandbox/";
					UnturnedLog.info("Parsed language '{0}' on command-line, and found in Sandbox directory", argLang);
					return true;
				}

				UnturnedLog.warn("Parsed language '{0}' on command-line, but unable to find related files", argLang);
			}

			return false;
		}

		public static bool tryGetServer(out ESteamServerVisibility visibility, out string id)
		{
			visibility = ESteamServerVisibility.LAN;
			id = "";

			string line = Get();
			int secureServer = line.ToLower().IndexOf("+secureserver", System.StringComparison.OrdinalIgnoreCase);

			if (secureServer != -1)
			{
				visibility = ESteamServerVisibility.Internet;
				id = line.Substring(secureServer + 14, line.Length - secureServer - 14);

				if (id == "Singleplayer")
				{
					return false;
				}

				return true;
			}

			int insecureServer = line.ToLower().IndexOf("+insecureserver", System.StringComparison.OrdinalIgnoreCase);

			if (insecureServer != -1)
			{
				visibility = ESteamServerVisibility.Internet;
				id = line.Substring(insecureServer + 16, line.Length - insecureServer - 16);

				if (id == "Singleplayer")
				{
					return false;
				}

				return true;
			}

			int internetServer = line.ToLower().IndexOf("+internetserver", System.StringComparison.OrdinalIgnoreCase);

			if (internetServer != -1)
			{
				visibility = ESteamServerVisibility.Internet;
				id = line.Substring(internetServer + 16, line.Length - internetServer - 16);

				if (id == "Singleplayer")
				{
					return false;
				}

				return true;
			}

			int lanServer = line.ToLower().IndexOf("+lanserver", System.StringComparison.OrdinalIgnoreCase);

			if (lanServer != -1)
			{
				visibility = ESteamServerVisibility.LAN;
				id = line.Substring(lanServer + 11, line.Length - lanServer - 11);

				if (id == "Singleplayer")
				{
					return false;
				}

				return true;
			}

			return false;
		}

		public static string[] getCommands()
		{
			string[] arguments = System.Environment.GetCommandLineArgs();
			List<string> commands = new List<string>();

			onGetCommands?.Invoke(commands);

			bool skip = false;

			for (int index = 0; index < arguments.Length; index++)
			{
				if (arguments[index].Substring(0, 1) == "+")
				{
					skip = true;
				}
				else if (arguments[index].Substring(0, 1) == "-")
				{
					commands.Add(arguments[index].Substring(1, arguments[index].Length - 1));
					skip = false;
				}
				else if (commands.Count > 0 && !skip)
				{
					commands[commands.Count - 1] += " " + arguments[index];
				}
			}

			return commands.ToArray();
		}

		/// <summary>
		/// Handles these cases:
		/// key value -> value
		/// key=value -> value
		/// key = value -> value
		/// key  =  value -> value
		/// key "value with spaces" -> value with spaces
		/// key "value with \" quotation marks" -> value with " quotation marks
		///
		/// Tested in CommandLineTests.cs
		/// </summary>
		public static bool TryParseValue(string input, string key, out string value)
		{
			value = null;
			if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(key))
			{
				return false;
			}

			int keySearchStartIndex = 0;
			while (keySearchStartIndex < input.Length)
			{
				int keyIndex = input.IndexOf(key, keySearchStartIndex, System.StringComparison.InvariantCultureIgnoreCase);
				if (keyIndex < 0)
				{
					// Input string does not contain key.
					return false;
				}

				int charAfterKeyIndex = keyIndex + key.Length;
				if (charAfterKeyIndex >= input.Length)
				{
					// String ends after key name.
					return false;
				}

				char charAfterKey = input[charAfterKeyIndex];
				if (charAfterKey != '=' && !char.IsWhiteSpace(charAfterKey))
				{
					// Key might be a substring of another key.
					// For example if key is "-w" we might have found "-width",
					// so continue search for key from here.
					keySearchStartIndex = charAfterKeyIndex;
					continue;
				}

				int valueStartIndex = charAfterKeyIndex + 1;
				while (true)
				{
					if (valueStartIndex >= input.Length)
					{
						// Reached the end of the string without finding a value.
						return false;
					}

					char nextChar = input[valueStartIndex];
					if (nextChar == '=' || char.IsWhiteSpace(nextChar))
					{
						++valueStartIndex;
					}
					else
					{
						break;
					}
				}

				if (input[valueStartIndex] != '"')
				{
					// Not inside quotation marks.
					int delimiterIndex = input.IndexOf(' ', valueStartIndex);
					if (delimiterIndex < 0)
					{
						// Value continues to the end of the string.
						value = input.Substring(valueStartIndex);
					}
					else
					{
						int valueLength = delimiterIndex - valueStartIndex;
						value = input.Substring(valueStartIndex, valueLength);
					}
					return true;
				}

				// Inside quotation marks, so we need to find a non-escaped closing quotation mark.
				++valueStartIndex;
				int valueEndIndex = valueStartIndex; // Equal to start because it could be an empty string.
				bool isNextCharEscaped = false;

				// Build value up char-by-char because we might need to skip escape characters.
				value = string.Empty;

				while (true)
				{
					if (valueEndIndex >= input.Length)
					{
						// Reached the end of the string without finding a closing quotation mark.
						return false;
					}

					char nextChar = input[valueEndIndex];
					if (nextChar == '\\')
					{
						++valueEndIndex;
						isNextCharEscaped = true;
					}
					else if (nextChar == '"' && !isNextCharEscaped)
					{
						// Found our closing quotation mark!
						return true;
					}
					else
					{
						value += nextChar;
						++valueEndIndex;
						isNextCharEscaped = false;
					}
				}
			}

			return false;
		}

		public static bool TryParseValue(string key, out string value)
		{
			return TryParseValue(Get(), key, out value);
		}

		static CommandLine()
		{
			// Refer to CommandLine.Get comment for reasoning.
			commandLineOverride = System.Environment.CommandLine;
		}
		internal static string commandLineOverride;
	}
}
