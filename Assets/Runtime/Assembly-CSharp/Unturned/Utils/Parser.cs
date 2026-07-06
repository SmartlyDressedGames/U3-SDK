////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class Parser
	{
		public static bool trySplitStart(string serial, out string start, out string end)
		{
			start = "";
			end = "";

			int index = serial.IndexOf(' ');

			if (index == -1)
			{
				return false;
			}

			start = serial.Substring(0, index);
			end = serial.Substring(index + 1, serial.Length - index - 1);

			return true;
		}

		public static bool trySplitEnd(string serial, out string start, out string end)
		{
			start = "";
			end = "";

			int index = serial.LastIndexOf(' ');

			if (index == -1)
			{
				return false;
			}

			start = serial.Substring(0, index);
			end = serial.Substring(index + 1, serial.Length - index - 1);

			return true;
		}

		public static string[] getComponentsFromSerial(string serial, char delimiter)
		{
			List<string> components = new List<string>();
			int index = 0;

			while (index < serial.Length)
			{
				int next = serial.IndexOf(delimiter, index);

				if (next == -1)
				{
					components.Add(serial.Substring(index, serial.Length - index));
					break;
				}
				else
					components.Add(serial.Substring(index, next - index));

				index = next + 1;
			}

			return components.ToArray();
		}

		public static string getSerialFromComponents(char delimiter, params object[] components)
		{
			string serial = "";

			for (int index = 0; index < components.Length; index++)
			{
				serial += components[index].ToString();

				if (index < components.Length - 1)
				{
					serial += delimiter;
				}
			}

			return serial;
		}

		public static bool checkIP(string ip)
		{
			int a = ip.IndexOf('.');

			if (a == -1)
			{
				return false;
			}

			int b = ip.IndexOf('.', a + 1);

			if (b == -1)
			{
				return false;
			}

			int c = ip.IndexOf('.', b + 1);

			if (c == -1)
			{
				return false;
			}

			int d = ip.IndexOf('.', c + 1);

			if (d != -1)
			{
				return false;
			}

			return true;
		}

		public static bool TryGetUInt32FromIP(string ip, out uint value)
		{
			// Not the smartest implementation. Moved from getUInt32FromIP in order to keep any weird legacy behaviour
			// while allowing us to differentiate between bad input and 0.0.0.0.
			value = 0;

			if (string.IsNullOrWhiteSpace(ip))
				return false;

			string[] components = ip.Split('.');
			if (components.Length != 4)
				return false;

			uint tempValue;

			if (uint.TryParse(components[0], out tempValue))
			{
				value |= (tempValue & 0xFF) << 24;
			}
			else
			{
				return false;
			}

			if (uint.TryParse(components[1], out tempValue))
			{
				value |= (tempValue & 0xFF) << 16;
			}
			else
			{
				return false;
			}

			if (uint.TryParse(components[2], out tempValue))
			{
				value |= (tempValue & 0xFF) << 8;
			}
			else
			{
				return false;
			}

			if (uint.TryParse(components[3], out tempValue))
			{
				value |= tempValue & 0xFF;
			}
			else
			{
				return false;
			}

			return true;
		}

		public static uint getUInt32FromIP(string ip)
		{
			uint value;
			TryGetUInt32FromIP(ip, out value);
			return value;
		}

		public static string getIPFromUInt32(uint ip)
		{
			return ((ip >> 24) & 0xFF) + "." + ((ip >> 16) & 0xFF) + "." + ((ip >> 8) & 0xFF) + "." + (ip & 0xFF);
		}
	}
}
