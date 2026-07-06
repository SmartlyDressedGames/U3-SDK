////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableTank : Interactable
	{
		private ETankSource _source;
		public ETankSource source => _source;

		private ushort _amount;
		public ushort amount => _amount;

		private ushort _capacity;
		public ushort capacity => _capacity;

		public bool isRefillable => amount < capacity;

		public bool isSiphonable => amount > 0;

		public void updateAmount(ushort newAmount)
		{
			_amount = newAmount;
		}

		public override void updateState(Asset asset, byte[] state)
		{
			_amount = System.BitConverter.ToUInt16(state, 0);
			_capacity = ((ItemTankAsset) asset).resource;
			_source = ((ItemTankAsset) asset).source;
		}

		public override bool checkUseable()
		{
			return amount > 0;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (source == ETankSource.WATER)
			{
				message = EPlayerMessage.VOLUME_WATER;
				text = amount + "/" + capacity;
			}
			else
			{
				message = EPlayerMessage.VOLUME_FUEL;
				text = "";
			}

			color = Color.white;
			return true;
		}

		private static readonly ClientInstanceMethod<ushort> SendAmount = ClientInstanceMethod<ushort>.Get(typeof(InteractableTank), nameof(ReceiveAmount));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveAmount(ushort newAmount)
		{
			updateAmount(newAmount);
		}

		public void ServerSetAmount(ushort newAmount)
		{
			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;

			if (BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				SendAmount.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), newAmount);

				BarricadeDrop barricade = region.FindBarricadeByRootFast(transform);
				byte[] state = System.BitConverter.GetBytes(newAmount);
				barricade.serversideData.barricade.state[0] = state[0];
				barricade.serversideData.barricade.state[1] = state[1];
			}
			else
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				CommandWindow.LogWarning("Fuel tank ServerSetAmount invalid region");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}
		}
	}
}
