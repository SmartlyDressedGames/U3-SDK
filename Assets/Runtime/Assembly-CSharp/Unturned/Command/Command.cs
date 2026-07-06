////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class Command : System.IComparable<Command>
	{
		protected Local localization;

		protected string _command;
		public string command => _command;

		protected string _info;
		public string info => _info;

		protected string _help;
		public string help => _help;

		protected virtual void execute(CSteamID executorID, string parameter)
		{ }

		public virtual bool check(CSteamID executorID, string method, string parameter)
		{
			if (method.ToLower() == command.ToLower())
			{
				execute(executorID, parameter);

				return true;
			}

			return false;
		}

		public int CompareTo(Command other)
		{
			return command.CompareTo(other.command);
		}
	}
}