////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Parses -X=Y from command-line.
	/// Ideally we could do "where T : TryParse" but for the meantime there are specialized subclasses.
	/// </summary>
	public abstract class CommandLineValue<T>
	{
		public string key
		{
			get;
			protected set;
		}

		public bool hasValue
		{
			get;
			protected set;
		}

		public T value
		{
			get;
			protected set;
		}

		protected abstract bool tryParse(string stringValue);

		public CommandLineValue(string key)
		{
			this.key = key;
			hasValue = false;
			value = default;

			string stringValue;
			if (CommandLine.TryParseValue(key, out stringValue))
			{
				if (string.IsNullOrWhiteSpace(stringValue))
				{
					UnturnedLog.warn("Expected non-empty value for '{0}' on command-line", key);
				}
				else
				{
					if (tryParse(stringValue))
					{
						hasValue = true;
						UnturnedLog.info("Parsed '{0}' as '{1}' from command-line", key, value);
					}
					else
					{
						UnturnedLog.warn("Unable to parse '{0}' as '{1}' from command-line", key, stringValue);
					}
				}
			}
		}
	}

	public class CommandLineInt : CommandLineValue<int>
	{
		public CommandLineInt(string key) : base(key) { }

		protected override bool tryParse(string stringValue)
		{
			int parsedValue;
			if (int.TryParse(stringValue, out parsedValue))
			{
				value = parsedValue;
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public class CommandLineFloat : CommandLineValue<float>
	{
		public CommandLineFloat(string key) : base(key) { }

		protected override bool tryParse(string stringValue)
		{
			float parsedValue;
			if (float.TryParse(stringValue, out parsedValue))
			{
				value = parsedValue;
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public class CommandLineString : CommandLineValue<string>
	{
		public CommandLineString(string key) : base(key) { }

		protected override bool tryParse(string stringValue)
		{
			value = stringValue;
			return true;
		}
	}

	public class CommandLineBool : CommandLineValue<bool>
	{
		public CommandLineBool(string key) : base(key) { }

		protected override bool tryParse(string stringValue)
		{
			if (stringValue.Equals("y", System.StringComparison.InvariantCultureIgnoreCase)
				|| stringValue.Equals("yes", System.StringComparison.InvariantCultureIgnoreCase)
				|| stringValue == "1"
				|| stringValue.Equals("true", System.StringComparison.InvariantCultureIgnoreCase))
			{
				value = true;
				return true;
			}
			else if (stringValue.Equals("n", System.StringComparison.InvariantCultureIgnoreCase)
				|| stringValue.Equals("no", System.StringComparison.InvariantCultureIgnoreCase)
				|| stringValue == "0"
				|| stringValue.Equals("false", System.StringComparison.InvariantCultureIgnoreCase))
			{
				value = false;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
