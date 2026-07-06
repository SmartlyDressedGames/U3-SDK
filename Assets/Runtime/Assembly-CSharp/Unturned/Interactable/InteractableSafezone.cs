////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableSafezone : InteractablePower
	{
		private bool _isPowered; // on/off
		public bool isPowered => _isPowered;

		private Transform engine;
		private SafezoneBubble bubble;

		private void UpdateEngine()
		{
			if (engine != null)
			{
				engine.gameObject.SetActive(isWired && isPowered);
			}
		}

		protected override void updateWired()
		{
			UpdateEngine();
			updateBubble();
		}

		public void updatePowered(bool newPowered)
		{
			if (_isPowered != newPowered)
			{
				_isPowered = newPowered;
				UpdateEngine();
				updateBubble();
			}
		}

		private void updateBubble()
		{
			if (isWired && isPowered)
			{
				registerBubble();
			}
			else
			{
				deregisterBubble();
			}
		}

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			_isPowered = state[0] == 1;

			// In the past Engine was client-only, but mod developers want it on the server too.
			engine = transform.Find("Engine");

			RefreshIsConnectedToPowerWithoutNotify();
			UpdateEngine();
			updateBubble();
		}

		public override void use()
		{
			ClientToggle();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (isPowered)
			{
				message = EPlayerMessage.SPOT_OFF;
			}
			else
			{
				message = EPlayerMessage.SPOT_ON;
			}

			text = "";
			color = Color.white;
			return true;
		}

		private void registerBubble()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (bubble != null)
			{
				return;
			}

			if (IsChildOfVehicle) // no system for moving bubble (yet?) depends if that's balanced
			{
				return;
			}

			bubble = SafezoneManager.registerBubble(transform.position, 24.0f);
		}

		private void deregisterBubble()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (bubble == null)
			{
				return;
			}

			SafezoneManager.deregisterBubble(bubble);
			bubble = null;
		}

		private void OnDisable()
		{
			// Returning to pool.
			deregisterBubble();
		}

		internal static readonly ClientInstanceMethod<bool> SendPowered = ClientInstanceMethod<bool>.Get(typeof(InteractableSafezone), nameof(ReceivePowered));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceivePowered(bool newPowered)
		{
			updatePowered(newPowered);
		}

		public void ClientToggle()
		{
			SendToggleRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, !isPowered);
		}

		private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableSafezone), nameof(ReceiveToggleRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveToggleRequest(in ServerInvocationContext context, bool desiredPowered)
		{
			if (isPowered == desiredPowered)
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

			BarricadeManager.ServerSetSafezonePoweredInternal(this, x, y, plant, region, !isPowered);
			EffectManager.TriggerFiremodeEffect(transform.position);
		}
	}
}
