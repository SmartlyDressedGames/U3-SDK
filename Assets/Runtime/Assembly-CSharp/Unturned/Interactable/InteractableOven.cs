////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableOven : InteractablePower
	{
		private bool _isLit;
		public bool isLit => _isLit;

		private Transform fire;

		private void UpdateVisual()
		{
			if (fire != null)
			{
				fire.gameObject.SetActive(isWired && isLit);
			}
		}

		protected override void updateWired()
		{
			UpdateVisual();
		}

		public void updateLit(bool newLit)
		{
			if (_isLit != newLit)
			{
				_isLit = newLit;
				UpdateVisual();
			}
		}

		public override void updateState(Asset asset, byte[] state)
		{
			_isLit = state[0] == 1;

			if (fire == null)
			{
				fire = transform.Find("Fire");
				LightLODTool.applyLightLOD(fire);
			}

			RefreshIsConnectedToPowerWithoutNotify();
			UpdateVisual();
		}

		public override void use()
		{
			ClientToggle();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (isLit)
			{
				message = EPlayerMessage.FIRE_OFF;
			}
			else
			{
				message = EPlayerMessage.FIRE_ON;
			}

			text = "";
			color = Color.white;
			return true;
		}

		internal static readonly ClientInstanceMethod<bool> SendLit = ClientInstanceMethod<bool>.Get(typeof(InteractableOven), nameof(ReceiveLit));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveLit(bool newLit)
		{
			updateLit(newLit);
		}

		public void ClientToggle()
		{
			SendToggleRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, !isLit);
		}

		private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableOven), nameof(ReceiveToggleRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveToggleRequest(in ServerInvocationContext context, bool desiredLit)
		{
			if (isLit == desiredLit)
				return;

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			Player player = context.GetPlayer();

			if (player == null)
			{
				return;
			}

			if (player.life.isDead)
			{
				return;
			}

			if ((transform.position - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			BarricadeManager.ServerSetOvenLitInternal(this, x, y, plant, region, !isLit);
		}
	}
}
