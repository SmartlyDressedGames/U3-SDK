////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetTransport.Loopback
{
	/// <summary>
	/// Dummy connection used in singleplayer.
	/// </summary>
	public struct TransportConnection_Loopback : ITransportConnection
	{
		public static readonly ITransportConnection DedicatedServer = DedicatedServerLoopback;

		public static TransportConnection_Loopback Create()
		{
			return new TransportConnection_Loopback(++counter);
		}

		public bool TryGetIPv4Address(out uint address)
		{
			address = default;
			return false;
		}

		public bool TryGetPort(out ushort port)
		{
			port = default;
			return false;
		}

		public bool TryGetSteamId(out ulong steamId)
		{
			steamId = default;
			return false;
		}

		public System.Net.IPAddress GetAddress()
		{
			return null;
		}

		public string GetAddressString(bool withPort)
		{
			return null;
		}

		public void CloseConnection()
		{

		}

		public void Send(byte[] buffer, long size, ENetReliability reliability)
		{
			throw new System.NotSupportedException();
		}

		public override bool Equals(object obj)
		{
			return obj is TransportConnection_Loopback && id == ((TransportConnection_Loopback) obj).id;
		}

		public bool Equals(TransportConnection_Loopback other)
		{
			return id == other.id;
		}

		public bool Equals(ITransportConnection other)
		{
			return other is TransportConnection_Loopback && id == ((TransportConnection_Loopback) other).id;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override string ToString()
		{
			if (this == DedicatedServerLoopback)
			{
				return "DedicatedServerLoopback";
			}
			else
			{
				return string.Format("Loopback_{0}", id.ToString());
			}
		}

		public static bool operator ==(TransportConnection_Loopback lhs, TransportConnection_Loopback rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(TransportConnection_Loopback lhs, TransportConnection_Loopback rhs)
		{
			return !(lhs == rhs);
		}

		private TransportConnection_Loopback(int id)
		{
			this.id = id;
		}

		private int id;
		private static int counter;

		private static readonly TransportConnection_Loopback DedicatedServerLoopback = Create();
	}
}
