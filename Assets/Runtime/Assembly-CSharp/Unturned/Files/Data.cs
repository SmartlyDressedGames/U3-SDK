////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SDG.Unturned
{
	public class Data
	{
		private Dictionary<string, string> data;

		private byte[] _hash;
		public byte[] hash => _hash;

		public bool isCSV;

		public bool isEmpty => data.Count == 0;

		public List<string> errors
		{
			get;
			protected set;
		}

		public bool TryReadString(string key, out string value)
		{
			return data.TryGetValue(key, out value);
		}

		public string readString(string key, string defaultValue = default)
		{
			string value;

			if (!data.TryGetValue(key, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public T readEnum<T>(string key, T defaultValue = default) where T : struct
		{
			string value;
			if (data.TryGetValue(key, out value))
			{
				// Future versions of .NET have Enum.TryParse, but for now we have this ask-forgiveness method:
				try
				{
					const bool ignoreCase = true;
					T result = (T) Enum.Parse(typeof(T), value, ignoreCase);
					return result;
				}
				catch
				{
					return defaultValue;
				}
			}
			else
			{
				return defaultValue;
			}
		}

		public bool readBoolean(string key, bool defaultValue = false)
		{
			string stringValue;
			if (data.TryGetValue(key, out stringValue))
			{
				return stringValue.Equals("y", StringComparison.InvariantCultureIgnoreCase) || stringValue == "1" || stringValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			}
			else
			{
				return defaultValue;
			}
		}

		public byte readByte(string key, byte defaultValue = 0)
		{
			byte value;

			if (!byte.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public sbyte readSByte(string key, sbyte defaultValue = 0)
		{
			sbyte value;

			if (!sbyte.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public byte[] readByteArray(string key)
		{
			string value = readString(key);

			return Encoding.UTF8.GetBytes(value);
		}

		public short readInt16(string key, short defaultValue = 0)
		{
			short value;

			if (!short.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public ushort readUInt16(string key, ushort defaultValue = 0)
		{
			ushort value;

			if (!ushort.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public int readInt32(string key, int defaultValue = 0)
		{
			int value;

			if (!int.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public uint readUInt32(string key, uint defaultValue = 0)
		{
			uint value;

			if (!uint.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public long readInt64(string key, long defaultValue = 0)
		{
			long value;

			if (!long.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public ulong readUInt64(string key, ulong defaultValue = 0)
		{
			ulong value;

			if (!ulong.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public float readSingle(string key, float defaultValue = 0.0f)
		{
			float value;

			if (!float.TryParse(readString(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public Vector3 readVector3(string key)
		{
			return new Vector3(readSingle(key + "_X"), readSingle(key + "_Y"), readSingle(key + "_Z"));
		}

		public Quaternion readQuaternion(string key)
		{
			return Quaternion.Euler(readByte(key + "_X") * 2, readByte(key + "_Y"), readByte(key + "_Z"));
		}

		public Color readColor(string key)
		{
			return readColor(key, Color.black);
		}

		public Color readColor(string key, Color defaultColor)
		{
			return new Color(readSingle(key + "_R", defaultColor.r), readSingle(key + "_G", defaultColor.g), readSingle(key + "_B", defaultColor.b));
		}

		/// <summary>
		/// Read 8-bit per channel color excluding alpha.
		/// </summary>
		public Color32 ReadColor32RGB(string key, Color32 defaultValue)
		{
			return new Color32(readByte(key + "_R", defaultValue.r), readByte(key + "_G", defaultValue.g), readByte(key + "_B", defaultValue.b), byte.MaxValue);
		}

		public CSteamID readSteamID(string key)
		{
			return new CSteamID(readUInt64(key));
		}

		public Guid readGUID(string key)
		{
			string value = readString(key);
			if (string.IsNullOrEmpty(value) || (value.Length == 1 && value[0] == '0'))
			{
				// Allows null asset references.
				return Guid.Empty;
			}
			else
			{
				return new Guid(value);
			}
		}

		public void ReadGuidOrLegacyId(string key, out Guid guid, out ushort legacyId)
		{
			string value;
			if (data.TryGetValue(key, out value) && !string.IsNullOrEmpty(value) && (value.Length != 1 || value[0] != '0'))
			{
				// ushort comes first because it will fail for large guid numbers.
				if (ushort.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out legacyId))
				{
					guid = Guid.Empty;
					return;
				}
				else if (Guid.TryParse(value, out guid))
				{
					legacyId = 0;
					return;
				}
			}

			guid = Guid.Empty;
			legacyId = 0;
		}

		/// <summary>
		/// Intended as a drop-in replacement for existing assets with property uint16s.
		/// </summary>
		public ushort ReadGuidOrLegacyId(string key, out Guid guid)
		{
			ushort legacyId;
			ReadGuidOrLegacyId(key, out guid, out legacyId);
			return legacyId;
		}

		public AssetReference<T> readAssetReference<T>(string key) where T : Asset
		{
			if (has(key))
			{
				return new AssetReference<T>(readGUID(key));
			}
			else
			{
				return AssetReference<T>.invalid;
			}
		}

		public AssetReference<T> readAssetReference<T>(string key, in AssetReference<T> defaultValue) where T : Asset
		{
			if (has(key))
			{
				return new AssetReference<T>(readGUID(key));
			}
			else
			{
				return defaultValue;
			}
		}

		private void ParseMasterBundleReference(string key, string value, out string name, out string path)
		{
			int delimiterIndex = value.IndexOf(':');
			if (delimiterIndex < 0)
			{
				if (Assets.currentMasterBundle != null)
				{
					name = Assets.currentMasterBundle.assetBundleName;
				}
				else
				{
					name = string.Empty;
					AddError($"MasterBundleRef \"{key}\" is not associated with a master bundle nor does it specify one");
				}
				path = value;
			}
			else
			{
				name = value.Substring(0, delimiterIndex);
				path = value.Substring(delimiterIndex + 1);

				if (string.IsNullOrEmpty(name))
				{
					AddError($"MasterBundleRef \"{key}\" specified asset bundle name is empty");
				}
				if (string.IsNullOrEmpty(path))
				{
					AddError($"MasterBundleRef \"{key}\" specified asset path is empty");
				}
			}
		}

		public MasterBundleReference<T> readMasterBundleReference<T>(string key) where T : UnityEngine.Object
		{
			string value;
			if (TryReadString(key, out value))
			{
				string name;
				string path;
				ParseMasterBundleReference(key, value, out name, out path);
				return new MasterBundleReference<T>(name, path);
			}
			else
			{
				return MasterBundleReference<T>.invalid;
			}
		}

		public AudioReference ReadAudioReference(string key)
		{
			string value;
			if (TryReadString(key, out value))
			{
				string name;
				string path;
				ParseMasterBundleReference(key, value, out name, out path);
				return new AudioReference(name, path);
			}
			else
			{
				return default;
			}
		}

		public void writeString(string key, string value)
		{
			data.Add(key, value);
		}

		public void writeBoolean(string key, bool value)
		{
			data.Add(key, value ? "y" : "n");
		}

		public void writeByte(string key, byte value)
		{
			data.Add(key, value.ToString());
		}

		public void writeByteArray(string key, byte[] value)
		{
			data.Add(key, Encoding.UTF8.GetString(value));
		}

		public void writeInt16(string key, short value)
		{
			data.Add(key, value.ToString());
		}

		public void writeUInt16(string key, ushort value)
		{
			data.Add(key, value.ToString());
		}

		public void writeInt32(string key, int value)
		{
			data.Add(key, value.ToString());
		}

		public void writeUInt32(string key, uint value)
		{
			data.Add(key, value.ToString());
		}

		public void writeInt64(string key, long value)
		{
			data.Add(key, value.ToString());
		}

		public void writeUInt64(string key, ulong value)
		{
			data.Add(key, value.ToString());
		}

		public void writeSingle(string key, float value)
		{
			data.Add(key, (Mathf.Floor(value * 100) / 100).ToString());
		}

		public void writeVector3(string key, Vector3 value)
		{
			writeSingle(key + "_X", value.x);
			writeSingle(key + "_Y", value.y);
			writeSingle(key + "_Z", value.z);
		}

		public void writeQuaternion(string key, Quaternion value)
		{
			Vector3 angles = value.eulerAngles;

			writeByte(key + "_X", MeasurementTool.angleToByte(angles.x));
			writeByte(key + "_Y", MeasurementTool.angleToByte(angles.y));
			writeByte(key + "_Z", MeasurementTool.angleToByte(angles.z));
		}

		public void writeColor(string key, Color value)
		{
			writeSingle(key + "_R", value.r);
			writeSingle(key + "_G", value.g);
			writeSingle(key + "_B", value.b);
		}

		public void writeSteamID(string key, CSteamID value)
		{
			writeUInt64(key, value.m_SteamID);
		}

		public void writeGUID(string key, Guid value)
		{
			writeString(key, value.ToString("N"));
		}

		public string getFile()
		{
			string file = "";
			char delimiter = isCSV ? ',' : ' ';

			foreach (KeyValuePair<string, string> content in data)
			{
				file += content.Key + delimiter + content.Value + "\n";
			}

			return file;
		}

		public string[] getLines()
		{
			string[] lines = new string[data.Count];
			char delimiter = isCSV ? ',' : ' ';

			int index = 0;
			foreach (KeyValuePair<string, string> content in data)
			{
				lines[index] = content.Key + delimiter + content.Value;
				index++;
			}

			return lines;
		}

		public KeyValuePair<string, string>[] getContents()
		{
			KeyValuePair<string, string>[] contents = new KeyValuePair<string, string>[data.Count];

			int index = 0;
			foreach (KeyValuePair<string, string> content in data)
			{
				contents[index] = content;
				index++;
			}

			return contents;
		}

		public string[] getValuesWithKey(string key)
		{
			List<string> values = new List<string>();

			foreach (KeyValuePair<string, string> content in data)
			{
				if (content.Key == key)
				{
					values.Add(content.Value);
				}
			}

			return values.ToArray();
		}

		public string[] getKeysWithValue(string value)
		{
			List<string> keys = new List<string>();

			foreach (KeyValuePair<string, string> content in data)
			{
				if (content.Value == value)
				{
					keys.Add(content.Key);
				}
			}

			return keys.ToArray();
		}

		public bool has(string key)
		{
			return data.ContainsKey(key);
		}

		public void reset()
		{
			data.Clear();
		}

		public void log()
		{
			foreach (KeyValuePair<string, string> content in data)
			{
				UnturnedLog.info("{0} = {1}", content.Key, content.Value);
			}
		}

		internal Data(StreamReader streamReader, SHA1Stream hashStream)
		{
			data = new Dictionary<string, string>();

			string line = string.Empty;
			while ((line = streamReader.ReadLine()) != null)
			{
				ParseLine(line);
			}

			_hash = hashStream?.Hash;
		}

		public Data(string content)
		{
			data = new Dictionary<string, string>();

			StringReader reader = null;

			try
			{
				reader = new StringReader(content);
				string line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					ParseLine(line);
				}

				_hash = Hash.SHA1(content);
			}
			catch (Exception exception)
			{
				do
				{
					AddError($"Caught exception: \"{exception.Message}\"\n{exception.StackTrace}");
					exception = exception.InnerException;
				}
				while (exception != null);

				data.Clear();
				_hash = null;
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}
		}

		public Data()
		{
			data = new Dictionary<string, string>();
			_hash = null;
		}

		private void AddError(string message)
		{
			if (errors == null)
			{
				errors = new List<string>();
			}
			errors.Add(message);
		}

		private void ParseLine(string line)
		{
			if (line.Length < 1 || (line.Length > 1 && line[0] == '/' && line[1] == '/'))
			{
				return;
			}

			int split = line.IndexOf(' ');

			string key;
			string value;

			if (split != -1)
			{
				key = line.Substring(0, split);
				value = line.Substring(split + 1, line.Length - split - 1);
			}
			else
			{
				key = line;
				value = string.Empty;
			}

			string existingValue;
			if (data.TryGetValue(key, out existingValue))
			{
				AddError($"Duplicate key: \"{key}\" Old value: {existingValue} New value: {value}");
				data[key] = value;
			}
			else
			{
				data.Add(key, value);
			}
		}
	}
}
