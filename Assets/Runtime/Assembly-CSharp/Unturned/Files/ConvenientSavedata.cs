////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public interface IConvenientSavedata
	{
		bool read(string key, out string value);
		void write(string key, string value);

		bool read(string key, out DateTime value);
		void write(string key, DateTime value);

		bool read(string key, out bool value);
		void write(string key, bool value);

		bool read(string key, out long value);
		void write(string key, long value);

		bool hasFlag(string flag);
		void setFlag(string flag);

		/// <returns>true if key existed and was removed.</returns>
		bool DeleteBool(string key);
		/// <returns>true if key existed and was removed.</returns>
		bool DeleteInteger(string key);
	}

	/// <summary>
	/// Unturned equivalent of unity's PlayerPrefs.
	/// Convenient for saving one-off key-value pairs.
	/// </summary>
	public static class ConvenientSavedata
	{
		public static IConvenientSavedata get()
		{
			if (instance == null)
			{
				load();
			}

			return instance;
		}

		private static void load()
		{
			if (ReadWrite.fileExists(RELATIVE_PATH, false, true))
			{
				try
				{
					instance = ReadWrite.deserializeJSON<ConvenientSavedataImplementation>(RELATIVE_PATH, false, true);
				}
				catch (Exception e)
				{
					UnturnedLog.exception(e, "Unable to parse {0}! consider validating with a JSON linter", RELATIVE_PATH);
					instance = null;
				}

				if (instance == null)
				{
					instance = new ConvenientSavedataImplementation();
				}
			}
			else
			{
				instance = new ConvenientSavedataImplementation();
			}
		}

		public static void save()
		{
			if (instance == null)
			{
				// Perhaps nobody ever called get/load, in which case do not clobber existing file.
				UnturnedLog.info("Skipped saving convenient data");
			}
			else
			{
				instance.isDirty = false;

				// Catch exception because if IO fails (e.g. if user marked file read-only) we do not want to break. 
				try
				{
					ReadWrite.serializeJSON(RELATIVE_PATH, false, true, instance);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, "Caught exception serializing convenient data:");
				}

				UnturnedLog.info("Saved convenient data");
			}
		}

		public static void SaveIfDirty()
		{
			if (instance != null && instance.isDirty)
			{
				instance.isDirty = false;

				// Catch exception because if IO fails (e.g. if user marked file read-only) we do not want to break. 
				try
				{
					ReadWrite.serializeJSON(RELATIVE_PATH, false, true, instance);
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, "Caught exception serializing convenient data:");
				}

				UnturnedLog.info("Saved convenient data (dirty)");
			}
		}

		private static ConvenientSavedataImplementation instance = null;
		private static readonly string RELATIVE_PATH = "/Cloud/ConvenientSavedata.json";
	}

	internal class ConvenientSavedataImplementation : IConvenientSavedata
	{
		public Dictionary<string, string> Strings = new Dictionary<string, string>();

		public bool read(string key, out string value)
		{
			return Strings.TryGetValue(key, out value);
		}

		public void write(string key, string value)
		{
			Strings[key] = value;
			isDirty = true;
		}

		public Dictionary<string, DateTime> DateTimes = new Dictionary<string, DateTime>();

		public bool read(string key, out DateTime value)
		{
			return DateTimes.TryGetValue(key, out value);
		}

		public void write(string key, DateTime value)
		{
			DateTimes[key] = value;
			isDirty = true;
		}

		public Dictionary<string, bool> Booleans = new Dictionary<string, bool>();

		public bool read(string key, out bool value)
		{
			return Booleans.TryGetValue(key, out value);
		}

		public void write(string key, bool value)
		{
			Booleans[key] = value;
			isDirty = true;
		}

		public Dictionary<string, long> Integers = new Dictionary<string, long>();

		public bool read(string key, out long value)
		{
			return Integers.TryGetValue(key, out value);
		}

		public void write(string key, long value)
		{
			Integers[key] = value;
			isDirty = true;
		}

		public HashSet<string> Flags = new HashSet<string>();

		public bool hasFlag(string flag)
		{
			return Flags.Contains(flag);
		}

		public void setFlag(string flag)
		{
			Flags.Add(flag);
			isDirty = true;
		}

		public bool DeleteBool(string key)
		{
			return Booleans.Remove(key);
		}

		public bool DeleteInteger(string key)
		{
			return Integers.Remove(key);
		}

		[Newtonsoft.Json.JsonIgnore]
		public bool isDirty = false;
	}
}
