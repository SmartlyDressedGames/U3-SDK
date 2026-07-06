////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableRainBarrel : Interactable
	{
		private bool _isFull;
		public bool isFull => _isFull;

		public void updateFull(bool newFull)
		{
			_isFull = newFull;
		}

		public override void updateState(Asset asset, byte[] state)
		{
			_isFull = state[0] == 1;
		}

		public override bool checkUseable()
		{
			return isFull;
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			message = EPlayerMessage.VOLUME_WATER;
			text = "";
			color = Color.white;
			return true;
		}

		private void onRainUpdated(ELightingRain rain)
		{
			if (rain != ELightingRain.POST_DRIZZLE)
			{
				return;
			}

			if (Physics.Raycast(transform.position + Vector3.up, Vector3.up, 32f, RayMasks.BLOCK_WIND))
			{
				return;
			}

			_isFull = true;

			if (Provider.isServer)
			{
				BarricadeManager.updateRainBarrel(transform, isFull, false);
			}
		}

		private void OnEnable()
		{
			LightingManager.onRainUpdated += onRainUpdated;
		}

		private void OnDisable()
		{
			LightingManager.onRainUpdated -= onRainUpdated;
		}

		internal static readonly ClientInstanceMethod<bool> SendFull = ClientInstanceMethod<bool>.Get(typeof(InteractableRainBarrel), nameof(ReceiveFull));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveFull(bool newFull)
		{
			updateFull(newFull);
		}
	}
}
