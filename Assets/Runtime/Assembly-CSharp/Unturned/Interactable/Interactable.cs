////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Interactable : MonoBehaviour
	{
		public bool IsChildOfVehicle => transform.parent != null && transform.parent.CompareTag("Vehicle");

		public virtual void updateState(Asset asset, byte[] state)
		{

		}

		public virtual bool checkInteractable()
		{
			return true;
		}

		public virtual bool checkUseable()
		{
			return true;
		}

		public virtual bool checkHighlight(out Color color)
		{
			color = Color.white;

			return false;
		}

		public virtual bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			message = EPlayerMessage.NONE;
			text = "";
			color = Color.white;

			return false;
		}

		public virtual void use()
		{

		}

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

		private NetId _netId;


		[System.Obsolete("Renamed to IsChildOfVehicle")]
		public bool isPlant => IsChildOfVehicle;
	}
}
