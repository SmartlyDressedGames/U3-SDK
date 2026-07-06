////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SteamCaller : MonoBehaviour
	{
		protected SteamChannel _channel;
		public SteamChannel channel => _channel;

		private void Awake()
		{
			_channel = GetComponent<SteamChannel>();
		}
	}
}
