////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableLibrary : Interactable
	{
		private CSteamID _owner;
		public CSteamID owner => _owner;

		private CSteamID _group;
		public CSteamID group => _group;

		private uint _amount;
		public uint amount => _amount;

		private uint _capacity;
		public uint capacity => _capacity;

		private byte _tax;
		public byte tax => _tax;

		private bool isLocked;

		public bool checkTransfer(CSteamID enemyPlayer, CSteamID enemyGroup)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			return !isLocked || enemyPlayer == owner || (group != CSteamID.Nil && enemyGroup == group);
		}

		public void updateAmount(uint newAmount)
		{
			_amount = newAmount;
		}

		public override void updateState(Asset asset, byte[] state)
		{
			isLocked = ((ItemBarricadeAsset) asset).isLocked;
			_capacity = ((ItemLibraryAsset) asset).capacity;
			_tax = ((ItemLibraryAsset) asset).tax;

			_owner = new CSteamID(System.BitConverter.ToUInt64(state, 0));
			_group = new CSteamID(System.BitConverter.ToUInt64(state, 8));
			_amount = System.BitConverter.ToUInt32(state, 16);
		}

		public override bool checkUseable()
		{
			return checkTransfer(Provider.client, Player.LocalPlayer.quests.groupID) && !PlayerUI.window.showCursor;
		}

		public override void use()
		{
			PlayerBarricadeLibraryUI.open(this);

			PlayerLifeUI.close();
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			if (checkUseable())
			{
				message = EPlayerMessage.USE;
			}
			else
			{
				message = EPlayerMessage.LOCKED;
			}

			text = "";
			color = Color.white;
			return !PlayerUI.window.showCursor;
		}

		private static readonly ClientInstanceMethod<uint> SendAmount = ClientInstanceMethod<uint>.Get(typeof(InteractableLibrary), nameof(ReceiveAmount));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveAmount(uint newAmount)
		{
			updateAmount(newAmount);
		}

		public void ClientTransfer(byte transaction, uint delta)
		{
			SendTransferLibraryRequest.Invoke(GetNetId(), ENetReliability.Unreliable, transaction, delta);
		}

		private static readonly ServerInstanceMethod<byte, uint> SendTransferLibraryRequest = ServerInstanceMethod<byte, uint>.Get(typeof(InteractableLibrary), nameof(ReceiveTransferLibraryRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveTransferLibraryRequest(in ServerInvocationContext context, byte transaction, uint delta)
		{
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

			byte x;
			byte y;
			ushort plant;
			BarricadeRegion region;
			if (!BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
			{
				context.LogWarning("invalid region");
				return;
			}

			if (checkTransfer(player.channel.owner.playerID.steamID, player.quests.groupID))
			{
				uint newAmount;

				if (transaction == 0)
				{
					uint taxAmt = (uint) System.Math.Ceiling(delta * (tax / 100.0));
					uint net = delta - taxAmt;

					if (delta > player.skills.experience || net + amount > capacity)
					{
						return;
					}

					newAmount = amount + net;
					player.skills.askSpend(delta);
				}
				else
				{
					if (delta > amount)
					{
						return;
					}

					newAmount = amount - delta;
					player.skills.askAward(delta);
				}

				SendAmount.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), newAmount);

				BarricadeDrop barricade = region.FindBarricadeByRootFast(transform);
				System.Buffer.BlockCopy(System.BitConverter.GetBytes(newAmount), 0, barricade.serversideData.barricade.state, 16, 4);
			}
		}
	}
}
