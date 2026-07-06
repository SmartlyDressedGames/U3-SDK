////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class SteamConnectionInfo
	{
		public uint _ip;
		public uint ip => _ip;

		public ushort _port;
		public ushort port => _port;

		public string _password;
		public string password => _password;

		public SteamConnectionInfo(uint newIP, ushort newPort, string newPassword)
		{
			_ip = newIP;
			_port = newPort;
			_password = newPassword;
		}

		public SteamConnectionInfo(string newIP, ushort newPort, string newPassword)
		{
			_ip = Parser.getUInt32FromIP(newIP);
			_port = newPort;
			_password = newPassword;
		}

		public SteamConnectionInfo(string newIPPort, string newPassword)
		{
			string[] components = Parser.getComponentsFromSerial(newIPPort, ':');

			_ip = Parser.getUInt32FromIP(components[0]);
			_port = ushort.Parse(components[1]);
			_password = newPassword;
		}
	}
}