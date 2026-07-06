////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableSpot : InteractablePower
	{
		private bool _isPowered; // on/off
		public bool isPowered => _isPowered;

		private Material material;
		private Transform spot;

		private void updateLights()
		{
			bool on = isWired && isPowered;

			if (material != null)
			{
				material.SetColor("_EmissionColor", on ? new Color(2f, 2f, 2f) : Color.black);
			}

			if (spot != null)
			{
				spot.gameObject.SetActive(on);
			}
		}

		protected override void updateWired()
		{
			updateLights();
		}

		public void updatePowered(bool newPowered)
		{
			if (_isPowered != newPowered)
			{
				_isPowered = newPowered;
				updateLights();
			}
		}

		public override void updateState(Asset asset, byte[] state)
		{
			base.updateState(asset, state);

			_isPowered = state[0] == 1;

			if (!Dedicator.IsDedicatedServer)
			{
				if (material == null)
				{
					material = HighlighterTool.getMaterialInstance(transform);
				}

				if (spot == null)
				{
					spot = transform.Find("Spots");
					LightLODTool.applyLightLOD(spot);
				}
			}

			RefreshIsConnectedToPowerWithoutNotify();
			updateLights();
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

		private void OnDestroy()
		{
			if (material != null)
			{
				DestroyImmediate(material);
				material = null;
			}
		}

		internal static readonly ClientInstanceMethod<bool> SendPowered = ClientInstanceMethod<bool>.Get(typeof(InteractableSpot), nameof(ReceivePowered));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceivePowered(bool newPowered)
		{
			updatePowered(newPowered);
		}

		public void ClientToggle()
		{
			SendToggleRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, !isPowered);
		}

		private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableSpot), nameof(ReceiveToggleRequest));
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

			BarricadeManager.ServerSetSpotPoweredInternal(this, x, y, plant, region, !isPowered);
			EffectManager.TriggerFiremodeEffect(transform.position);
		}
	}
}
