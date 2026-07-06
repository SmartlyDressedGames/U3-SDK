////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class Local
	{
		public string read(string key)
		{
			// 2022-05-27: I converted this method to just call format(key), but this method has slightly different expected behaviour in
			// that it can return empty/null.
			if (data != null)
			{
				return data.GetString(key);
			}
			else
			{
				return null;
			}
		}

		public string format(string key)
		{
			string text;
			if (TryReadString(key, out text))
			{
				return text;
			}
			else
			{
				return key;
			}
		}

		/// <summary>
		/// Unlike format, this returns null if key doesn't exist.
		/// </summary>
		public string FormatOrNull(string key)
		{
			if (TryReadString(key, out string text))
			{
				return text;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Unlike format, this returns string.Empty if key doesn't exist.
		/// </summary>
		public string FormatOrEmpty(string key)
		{
			if (TryReadString(key, out string text))
			{
				return text;
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// If active language is not English always reads from English fallback.
		/// </summary>
		public string FormatEnglishOrEmpty(string key)
		{
			IDatDictionary englishData = Provider.languageIsEnglish ? data : fallbackData;
			if (englishData != null && englishData.TryGetString(key, out string text))
			{
				return text;
			}
			else
			{
				return string.Empty;
			}
		}

		public string format(string key, object arg0)
		{
			string text;
			if (TryReadString(key, out text))
			{
				try
				{
					return string.Format(text, arg0);
				}
				catch
				{
					UnturnedLog.error($"Caught localization string formatting exception (key: \"{key}\" text: \"{text}\" arg0: \"{arg0}\")");
					return key;
				}
			}
			else
			{
				return key;
			}
		}

		internal static string FormatText(string text, object arg0)
		{
			try
			{
				return string.Format(text, arg0);
			}
			catch
			{
				UnturnedLog.error($"Caught localization string formatting exception (text: \"{text}\" arg0: \"{arg0}\")");
				return text;
			}
		}

		public string format(string key, object arg0, object arg1)
		{
			string text;
			if (TryReadString(key, out text))
			{
				try
				{
					return string.Format(text, arg0, arg1);
				}
				catch
				{
					UnturnedLog.error($"Caught localization string formatting exception (key: \"{key}\" text: \"{text}\" arg0: \"{arg0}\" arg1: \"{arg1}\")");
					return key;
				}
			}
			else
			{
				return key;
			}
		}

		internal static string FormatText(string text, object arg0, object arg1)
		{
			try
			{
				return string.Format(text, arg0, arg1);
			}
			catch
			{
				UnturnedLog.error($"Caught localization string formatting exception (text: \"{text}\" arg0: \"{arg0}\" arg1: \"{arg1}\")");
				return text;
			}
		}

		public string format(string key, object arg0, object arg1, object arg2)
		{
			string text;
			if (TryReadString(key, out text))
			{
				try
				{
					return string.Format(text, arg0, arg1, arg2);
				}
				catch
				{
					UnturnedLog.error($"Caught localization string formatting exception (key: \"{key}\" text: \"{text}\" arg0: \"{arg0}\" arg1: \"{arg1}\" arg2: \"{arg2}\")");
					return key;
				}
			}
			else
			{
				return key;
			}
		}

		public static string FormatText(string text, object arg0, object arg1, object arg2)
		{
			try
			{
				return string.Format(text, arg0, arg1, arg2);
			}
			catch
			{
				UnturnedLog.error($"Caught localization string formatting exception (text: \"{text}\" arg0: \"{arg0}\" arg1: \"{arg1}\" arg2: \"{arg2}\")");
				return text;
			}
		}

		public string format(string key, params object[] args)
		{
			string text;
			if (TryReadString(key, out text))
			{
				try
				{
					return string.Format(text, args);
				}
				catch
				{
					string argsDump = string.Empty;
					for (int argumentIndex = 0; argumentIndex < args.Length; ++argumentIndex)
					{
						if (argsDump.Length > 0)
						{
							argsDump += ' ';
						}
						argsDump += $"arg{argumentIndex}: \"{args[argumentIndex]}\"";
					}
					UnturnedLog.error($"Caught localization string formatting exception (key: \"{key}\" text: \"{text}\" {argsDump})");
					return key;
				}
			}
			else
			{
				return key;
			}
		}

		public bool has(string key)
		{
			// Only checks data (not fallbackData) because this is used for text versioning.
			if (data != null)
			{
				return data.ContainsKey(key);
			}
			else
			{
				return false;
			}
		}

		public Local(IDatDictionary newData) : this(newData, null)
		{ }

		public Local(IDatDictionary data, IDatDictionary fallbackData)
		{
			this.data = data;
			this.fallbackData = fallbackData;
		}

		public Local()
		{
			data = null;
		}

		private bool TryReadString(string key, out string text)
		{
			text = null;
			return (data != null && data.TryGetString(key, out text) && !string.IsNullOrEmpty(text)) || (fallbackData != null && fallbackData.TryGetString(key, out text) && !string.IsNullOrEmpty(text));
		}

		private IDatDictionary data;
		private IDatDictionary fallbackData;
	}
}
