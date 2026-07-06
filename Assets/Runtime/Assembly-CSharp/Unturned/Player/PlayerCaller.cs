////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class PlayerCaller : SteamCaller
	{
		protected Player _player;
		public Player player => _player;

		internal NetId _netId;
		public NetId GetNetId()
		{
			return _netId;
		}

		internal void AssignNetId(NetId netId)
		{
			_netId = netId;
			NetIdRegistry.Assign(netId, this);
		}

		internal void ReleaseNetId()
		{
			NetIdRegistry.Release(_netId);
			_netId.Clear();
		}

		private void Awake()
		{
			_channel = GetComponent<SteamChannel>();
			_player = GetComponent<Player>();
		}
	}
}
