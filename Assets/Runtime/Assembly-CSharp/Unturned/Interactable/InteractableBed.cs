////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableBed : Interactable
	{
		private CSteamID _owner;
		public CSteamID owner => _owner;

		public bool isClaimed => owner != CSteamID.Nil;

		public bool isClaimable => Time.realtimeSinceStartup - claimed > 0.75f;

		private float claimed;

		public bool checkClaim(CSteamID enemy)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			if (isClaimed)
			{
				return enemy == owner;
			}
			else
			{
				return true;
			}
		}

		public void updateClaim(CSteamID newOwner)
		{
			claimed = Time.realtimeSinceStartup;
			_owner = newOwner;
		}

		public override void updateState(Asset asset, byte[] state)
		{
			_owner = new CSteamID(System.BitConverter.ToUInt64(state, 0));
		}

		public override bool checkUseable()
		{
			return checkClaim(Provider.client);
		}

		public override void use()
		{
			ClientClaim();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			text = "";
			color = Color.white;

			if (checkUseable())
			{
				if (isClaimed)
				{
					message = EPlayerMessage.BED_OFF;
				}
				else
				{
					message = EPlayerMessage.BED_ON;
				}
			}
			else
			{
				message = EPlayerMessage.BED_CLAIMED;
			}

			return true;
		}

		internal static readonly ClientInstanceMethod<CSteamID> SendClaim = ClientInstanceMethod<CSteamID>.Get(typeof(InteractableBed), nameof(ReceiveClaim));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveClaim(CSteamID newOwner)
		{
			updateClaim(newOwner);
		}

		public void ClientClaim()
		{
			SendClaimRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable);
		}

		private static readonly ServerInstanceMethod SendClaimRequest = ServerInstanceMethod.Get(typeof(InteractableBed), nameof(ReceiveClaimRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 1)]
		public void ReceiveClaimRequest(in ServerInvocationContext context)
		{
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

			if (isClaimable && checkClaim(player.channel.owner.playerID.steamID))
			{
				if (isClaimed)
				{
					BarricadeManager.ServerSetBedOwnerInternal(this, x, y, plant, region, CSteamID.Nil);
				}
				else
				{
					BarricadeManager.unclaimBeds(player.channel.owner.playerID.steamID);
					BarricadeManager.ServerSetBedOwnerInternal(this, x, y, plant, region, player.channel.owner.playerID.steamID);
				}
			}
		}
	}
}
