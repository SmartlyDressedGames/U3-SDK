////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void ShirtUpdated(ushort newShirt, byte newShirtQuality, byte[] newShirtState);
	public delegate void PantsUpdated(ushort newPants, byte newPantsQuality, byte[] newPantsState);
	public delegate void HatUpdated(ushort newHat, byte newHatQuality, byte[] newHatState);
	public delegate void BackpackUpdated(ushort newBackpack, byte newBackpackQuality, byte[] newBackpackState);
	public delegate void VestUpdated(ushort newVest, byte newVestQuality, byte[] newVestState);
	public delegate void MaskUpdated(ushort newMask, byte newMaskQuality, byte[] newMaskState);
	public delegate void GlassesUpdated(ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState);
	public delegate void VisualToggleChanged(PlayerClothing sender);

	[NetEnum]
	public enum EVisualToggleType
	{
		COSMETIC,
		SKIN,
		MYTHIC
	}

	public class PlayerClothing : PlayerCaller
	{
		public static readonly byte SAVEDATA_VERSION = 7;

		public ShirtUpdated onShirtUpdated;
		public PantsUpdated onPantsUpdated;
		public HatUpdated onHatUpdated;
		public BackpackUpdated onBackpackUpdated;
		public VestUpdated onVestUpdated;
		public MaskUpdated onMaskUpdated;
		public GlassesUpdated onGlassesUpdated;

		/// <summary>
		/// Called when the player clicks the cosmetic, visual or skin toggle buttons.
		/// </summary>
		public event VisualToggleChanged VisualToggleChanged;

		/// <summary>
		/// Invoked after any player's shirt values change (not including loading).
		/// </summary>
		public static event System.Action<PlayerClothing> OnShirtChanged_Global;
		/// <summary>
		/// Invoked after any player's shirt values change (not including loading).
		/// </summary>
		public static event System.Action<PlayerClothing> OnPantsChanged_Global;
		/// <summary>
		/// Invoked after any player's hat values change (not including loading).
		/// </summary>
		public static event System.Action<PlayerClothing> OnHatChanged_Global;
		/// <summary>
		/// Invoked after any player's backpack values change (not including loading).
		/// </summary>
		public static event System.Action<PlayerClothing> OnBackpackChanged_Global;
		/// <summary>
		/// Invoked after any player's backpack values change (not including loading).
		/// </summary>
		public static event System.Action<PlayerClothing> OnVestChanged_Global;
		/// <summary>
		/// Invoked after any player's backpack values change (not including loading).
		/// </summary>
		public static event System.Action<PlayerClothing> OnMaskChanged_Global;
		/// <summary>
		/// Invoked after any player's glasses values change (not including loading).
		/// </summary>
		public static event System.Action<PlayerClothing> OnGlassesChanged_Global;

		public HumanClothes firstClothes
		{
			get;
			private set;
		}

		public HumanClothes thirdClothes
		{
			get;
			private set;
		}

		public HumanClothes characterClothes
		{
			get;
			private set;
		}

		public bool isVisual => thirdClothes.isVisual;
		public bool isSkinned
		{
			get;
			private set;
		}
		public bool isMythic => thirdClothes.isMythic;

		public ItemShirtAsset shirtAsset => thirdClothes.shirtAsset;

		public ItemPantsAsset pantsAsset => thirdClothes.pantsAsset;

		public ItemHatAsset hatAsset => thirdClothes.hatAsset;

		public ItemBackpackAsset backpackAsset => thirdClothes.backpackAsset;

		public ItemVestAsset vestAsset => thirdClothes.vestAsset;

		public ItemMaskAsset maskAsset => thirdClothes.maskAsset;

		public ItemGlassesAsset glassesAsset => thirdClothes.glassesAsset;

		public byte shirtQuality;
		public byte pantsQuality;
		public byte hatQuality;
		public byte backpackQuality;
		public byte vestQuality;
		public byte maskQuality;
		public byte glassesQuality;

		public byte[] shirtState;
		public byte[] pantsState;
		public byte[] hatState;
		public byte[] backpackState;
		public byte[] vestState;
		public byte[] maskState;
		public byte[] glassesState;

		public int visualShirt => thirdClothes.visualShirt;

		public int visualPants => thirdClothes.visualPants;

		public int visualHat => thirdClothes.visualHat;

		public int visualBackpack => thirdClothes.visualBackpack;

		public int visualVest => thirdClothes.visualVest;

		public int visualMask => thirdClothes.visualMask;

		public int visualGlasses => thirdClothes.visualGlasses;

		public ushort shirt => thirdClothes.shirt;

		public ushort pants => thirdClothes.pants;

		public ushort hat => thirdClothes.hat;

		public ushort backpack => thirdClothes.backpack;

		public ushort vest => thirdClothes.vest;

		public ushort mask => thirdClothes.mask;

		public ushort glasses => thirdClothes.glasses;

		public byte face => thirdClothes.face;

		public byte hair => thirdClothes.hair;

		public byte beard => thirdClothes.beard;

		public Color skin => thirdClothes.skin;

		public Color color => thirdClothes.color;
		public Color BeardColor => thirdClothes.BeardColor;

		[System.Obsolete]
		public void tellUpdateShirtQuality(CSteamID steamID, byte quality)
		{
			ReceiveShirtQuality(quality);
		}

		private static readonly ClientInstanceMethod<byte> SendShirtQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveShirtQuality));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUpdateShirtQuality))]
		public void ReceiveShirtQuality(byte quality)
		{
			shirtQuality = quality;

			onShirtUpdated?.Invoke(shirt, shirtQuality, shirtState);
		}

		public void sendUpdateShirtQuality()
		{
			SendShirtQuality.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), shirtQuality);
		}

		[System.Obsolete]
		public void tellUpdatePantsQuality(CSteamID steamID, byte quality)
		{
			ReceivePantsQuality(quality);
		}

		private static readonly ClientInstanceMethod<byte> SendPantsQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceivePantsQuality));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUpdatePantsQuality))]
		public void ReceivePantsQuality(byte quality)
		{
			pantsQuality = quality;

			onPantsUpdated?.Invoke(pants, pantsQuality, pantsState);
		}

		public void sendUpdatePantsQuality()
		{
			SendPantsQuality.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), pantsQuality);
		}

		[System.Obsolete]
		public void tellUpdateHatQuality(CSteamID steamID, byte quality)
		{
			ReceiveHatQuality(quality);
		}

		private static readonly ClientInstanceMethod<byte> SendHatQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveHatQuality));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUpdateHatQuality))]
		public void ReceiveHatQuality(byte quality)
		{
			hatQuality = quality;

			onHatUpdated?.Invoke(hat, hatQuality, hatState);
		}

		public void sendUpdateHatQuality()
		{
			SendHatQuality.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), hatQuality);
		}

		[System.Obsolete]
		public void tellUpdateBackpackQuality(CSteamID steamID, byte quality)
		{
			ReceiveBackpackQuality(quality);
		}

		private static readonly ClientInstanceMethod<byte> SendBackpackQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveBackpackQuality));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUpdateBackpackQuality))]
		public void ReceiveBackpackQuality(byte quality)
		{
			backpackQuality = quality;

			onBackpackUpdated?.Invoke(backpack, backpackQuality, backpackState);
		}

		public void sendUpdateBackpackQuality()
		{
			SendBackpackQuality.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), backpackQuality);
		}

		[System.Obsolete]
		public void tellUpdateVestQuality(CSteamID steamID, byte quality)
		{
			ReceiveVestQuality(quality);
		}

		private static readonly ClientInstanceMethod<byte> SendVestQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveVestQuality));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUpdateVestQuality))]
		public void ReceiveVestQuality(byte quality)
		{
			vestQuality = quality;

			onVestUpdated?.Invoke(vest, vestQuality, vestState);
		}

		public void sendUpdateVestQuality()
		{
			SendVestQuality.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), vestQuality);
		}

		[System.Obsolete]
		public void tellUpdateMaskQuality(CSteamID steamID, byte quality)
		{
			ReceiveMaskQuality(quality);
		}

		private static readonly ClientInstanceMethod<byte> SendMaskQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveMaskQuality));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUpdateMaskQuality))]
		public void ReceiveMaskQuality(byte quality)
		{
			maskQuality = quality;

			onMaskUpdated?.Invoke(mask, maskQuality, maskState);
		}

		public void sendUpdateMaskQuality()
		{
			SendMaskQuality.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), maskQuality);
		}

		public void updateMaskQuality()
		{
			onMaskUpdated?.Invoke(mask, maskQuality, maskState);
		}

		[System.Obsolete]
		public void tellUpdateGlassesQuality(CSteamID steamID, byte quality)
		{
			ReceiveGlassesQuality(quality);
		}

		private static readonly ClientInstanceMethod<byte> SendGlassesQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveGlassesQuality));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellUpdateGlassesQuality))]
		public void ReceiveGlassesQuality(byte quality)
		{
			glassesQuality = quality;

			onGlassesUpdated?.Invoke(glasses, glassesQuality, glassesState);
		}

		public void sendUpdateGlassesQuality()
		{
			SendGlassesQuality.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), glassesQuality);
		}

		[System.Obsolete]
		public void tellWearShirt(CSteamID steamID, ushort id, byte quality, byte[] state)
		{
			Asset asset = Assets.find(EAssetType.ITEM, id);
			ReceiveWearShirt(asset?.GUID ?? System.Guid.Empty, quality, state, false);
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, byte[], bool> SendWearShirt = ClientInstanceMethod<System.Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), nameof(ReceiveWearShirt));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWearShirt))]
		public void ReceiveWearShirt(System.Guid id, byte quality, byte[] state, bool playEffect)
		{
			if (thirdClothes == null)
			{
				return;
			}

			thirdClothes.shirtGuid = id;
			shirtQuality = quality;
			shirtState = state;
			thirdClothes.apply();

			if (firstClothes != null)
			{
				firstClothes.shirtGuid = id;
				firstClothes.apply();
			}

			if (characterClothes != null)
			{
				characterClothes.shirtGuid = id;
				characterClothes.apply();

				Characters.active.shirt = shirt;
			}

			UpdateStatModifiers();

			onShirtUpdated?.Invoke(shirt, quality, state);

			OnShirtChanged_Global?.Invoke(this);

			if (channel.IsLocalPlayer && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.shirtAsset);
			}

#if !DEDICATED_SERVER
			if (playEffect && thirdClothes.shirtAsset != null)
			{
				player.PlayAudioReference(thirdClothes.shirtAsset.wearAudio);
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askSwapShirt(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveSwapShirtRequest(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapShirtRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapShirtRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapShirt))]
		public void ReceiveSwapShirtRequest(byte page, byte x, byte y)
		{
			if (player.equipment.checkSelection(PlayerInventory.SHIRT))
			{
				if (player.equipment.isBusy)
				{
					return;
				}

				player.equipment.dequip();
			}

			if (page == 255)
			{
				if (shirtAsset == null)
				{
					return;
				}

				askWearShirt(0, 0, new byte[0], true);
			}
			else
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index == 255)
				{
					return;
				}

				ItemJar jar = player.inventory.getItem(page, index);
				ItemAsset asset = jar.GetAsset();

				if (asset != null && asset.type == EItemType.SHIRT)
				{
					player.inventory.removeItem(page, index);

					askWearShirt(jar.item.id, jar.item.quality, jar.item.state, true);
				}
			}
		}

		public void askWearShirt(ushort id, byte quality, byte[] state, bool playEffect)
		{
			ItemShirtAsset asset = Assets.find(EAssetType.ITEM, id) as ItemShirtAsset;
			askWearShirt(asset, quality, state, playEffect);
		}

		public void askWearShirt(ItemShirtAsset asset, byte quality, byte[] state, bool playEffect)
		{
			ushort currentID = shirt;
			byte currentQuality = shirtQuality;
			byte[] currentState = shirtState;

			SendWearShirt.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), asset?.GUID ?? System.Guid.Empty, quality, state, playEffect);

			if (currentID != 0)
			{
				player.inventory.forceAddItem(new Item(currentID, 1, currentQuality, currentState), false);
			}
		}

		public void sendSwapShirt(byte page, byte x, byte y)
		{
			if (page == 255 && shirtAsset == null)
			{
				// Client wants to remove shirt, but we are already not wearing shirt.
				return;
			}

			SendSwapShirtRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
		}

		[System.Obsolete]
		public void tellWearPants(CSteamID steamID, ushort id, byte quality, byte[] state)
		{
			Asset asset = Assets.find(EAssetType.ITEM, id);
			ReceiveWearPants(asset?.GUID ?? System.Guid.Empty, quality, state, false);
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, byte[], bool> SendWearPants = ClientInstanceMethod<System.Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), nameof(ReceiveWearPants));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWearPants))]
		public void ReceiveWearPants(System.Guid id, byte quality, byte[] state, bool playEffect)
		{
			if (thirdClothes == null)
			{
				return;
			}

			thirdClothes.pantsGuid = id;
			pantsQuality = quality;
			pantsState = state;
			thirdClothes.apply();

			if (characterClothes != null)
			{
				characterClothes.pantsGuid = id;
				characterClothes.apply();

				Characters.active.pants = pants;
			}

			UpdateStatModifiers();

			onPantsUpdated?.Invoke(pants, quality, state);

			OnPantsChanged_Global?.Invoke(this);

			if (channel.IsLocalPlayer && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.pantsAsset);
			}

#if !DEDICATED_SERVER
			if (playEffect && thirdClothes.pantsAsset != null)
			{
				player.PlayAudioReference(thirdClothes.pantsAsset.wearAudio);
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askSwapPants(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveSwapPantsRequest(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapPantsRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapPantsRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapPants))]
		public void ReceiveSwapPantsRequest(byte page, byte x, byte y)
		{
			if (player.equipment.checkSelection(PlayerInventory.PANTS))
			{
				if (player.equipment.isBusy)
				{
					return;
				}

				player.equipment.dequip();
			}

			if (page == 255)
			{
				if (pantsAsset == null)
				{
					return;
				}

				askWearPants(0, 0, new byte[0], true);
			}
			else
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index == 255)
				{
					return;
				}

				ItemJar jar = player.inventory.getItem(page, index);
				ItemAsset asset = jar.GetAsset();

				if (asset != null && asset.type == EItemType.PANTS)
				{
					player.inventory.removeItem(page, index);

					askWearPants(jar.item.id, jar.item.quality, jar.item.state, true);
				}
			}
		}

		public void askWearPants(ushort id, byte quality, byte[] state, bool playEffect)
		{
			ItemPantsAsset asset = Assets.find(EAssetType.ITEM, id) as ItemPantsAsset;
			askWearPants(asset, quality, state, playEffect);
		}

		public void askWearPants(ItemPantsAsset asset, byte quality, byte[] state, bool playEffect)
		{
			ushort currentID = pants;
			byte currentQuality = pantsQuality;
			byte[] currentState = pantsState;

			SendWearPants.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), asset?.GUID ?? System.Guid.Empty, quality, state, playEffect);

			if (currentID != 0)
			{
				player.inventory.forceAddItem(new Item(currentID, 1, currentQuality, currentState), false);
			}
		}

		public void sendSwapPants(byte page, byte x, byte y)
		{
			if (page == 255 && pantsAsset == null)
			{
				// Client wants to remove pants, but we are already not wearing pants.
				return;
			}

			SendSwapPantsRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
		}

		[System.Obsolete]
		public void tellWearHat(CSteamID steamID, ushort id, byte quality, byte[] state)
		{
			Asset asset = Assets.find(EAssetType.ITEM, id);
			ReceiveWearHat(asset?.GUID ?? System.Guid.Empty, quality, state, false);
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, byte[], bool> SendWearHat = ClientInstanceMethod<System.Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), nameof(ReceiveWearHat));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWearHat))]
		public void ReceiveWearHat(System.Guid id, byte quality, byte[] state, bool playEffect)
		{
			if (thirdClothes == null)
			{
				return;
			}

			thirdClothes.hatGuid = id;
			hatQuality = quality;
			hatState = state;
			thirdClothes.apply();

			if (characterClothes != null)
			{
				characterClothes.hatGuid = id;
				characterClothes.apply();

				Characters.active.hat = hat;
			}

			UpdateStatModifiers();

			onHatUpdated?.Invoke(hat, quality, state);

			OnHatChanged_Global?.Invoke(this);

			if (channel.IsLocalPlayer && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.hatAsset);
			}

#if !DEDICATED_SERVER
			if (playEffect && thirdClothes.hatAsset != null)
			{
				player.PlayAudioReference(thirdClothes.hatAsset.wearAudio);
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askSwapHat(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveSwapHatRequest(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapHatRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapHatRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapHat))]
		public void ReceiveSwapHatRequest(byte page, byte x, byte y)
		{
			if (page == 255)
			{
				if (hatAsset == null)
				{
					return;
				}

				askWearHat(0, 0, new byte[0], true);
			}
			else
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index == 255)
				{
					return;
				}

				ItemJar jar = player.inventory.getItem(page, index);
				ItemAsset asset = jar.GetAsset();

				if (asset != null && asset.type == EItemType.HAT)
				{
					player.inventory.removeItem(page, index);

					askWearHat(jar.item.id, jar.item.quality, jar.item.state, true);
				}
			}
		}

		public void askWearHat(ushort id, byte quality, byte[] state, bool playEffect)
		{
			ItemHatAsset asset = Assets.find(EAssetType.ITEM, id) as ItemHatAsset;
			askWearHat(asset, quality, state, playEffect);
		}

		public void askWearHat(ItemHatAsset asset, byte quality, byte[] state, bool playEffect)
		{
			ushort currentID = hat;
			byte currentQuality = hatQuality;
			byte[] currentState = hatState;

			SendWearHat.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), asset?.GUID ?? System.Guid.Empty, quality, state, playEffect);

			if (currentID != 0)
			{
				player.inventory.forceAddItem(new Item(currentID, 1, currentQuality, currentState), false);
			}
		}

		public void sendSwapHat(byte page, byte x, byte y)
		{
			if (page == 255 && hatAsset == null)
			{
				// Client wants to remove hat, but we are already not wearing hat.
				return;
			}

			if (Provider.isServer)
			{
				ReceiveSwapHatRequest(page, x, y);
			}
			else
			{
				SendSwapHatRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
			}
		}

		[System.Obsolete]
		public void tellWearBackpack(CSteamID steamID, ushort id, byte quality, byte[] state)
		{
			Asset asset = Assets.find(EAssetType.ITEM, id);
			ReceiveWearBackpack(asset?.GUID ?? System.Guid.Empty, quality, state, false);
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, byte[], bool> SendWearBackpack = ClientInstanceMethod<System.Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), nameof(ReceiveWearBackpack));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWearBackpack))]
		public void ReceiveWearBackpack(System.Guid id, byte quality, byte[] state, bool playEffect)
		{
			if (thirdClothes == null)
			{
				return;
			}

			thirdClothes.backpackGuid = id;
			backpackQuality = quality;
			backpackState = state;
			thirdClothes.apply();

			if (characterClothes != null)
			{
				characterClothes.backpackGuid = id;
				characterClothes.apply();

				Characters.active.backpack = backpack;
			}

			UpdateStatModifiers();

			onBackpackUpdated?.Invoke(backpack, quality, state);

			OnBackpackChanged_Global?.Invoke(this);

			if (channel.IsLocalPlayer && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.backpackAsset);
			}

#if !DEDICATED_SERVER
			if (playEffect && thirdClothes.backpackAsset != null)
			{
				player.PlayAudioReference(thirdClothes.backpackAsset.wearAudio);
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askSwapBackpack(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveSwapBackpackRequest(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapBackpackRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapBackpackRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapBackpack))]
		public void ReceiveSwapBackpackRequest(byte page, byte x, byte y)
		{
			if (player.equipment.checkSelection(PlayerInventory.BACKPACK))
			{
				if (player.equipment.isBusy)
				{
					return;
				}

				player.equipment.dequip();
			}

			if (page == 255)
			{
				if (backpackAsset == null)
				{
					return;
				}

				askWearBackpack(0, 0, new byte[0], true);
			}
			else
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index == 255)
				{
					return;
				}

				ItemJar jar = player.inventory.getItem(page, index);
				ItemAsset asset = jar.GetAsset();

				if (asset != null && asset.type == EItemType.BACKPACK)
				{
					player.inventory.removeItem(page, index);

					askWearBackpack(jar.item.id, jar.item.quality, jar.item.state, true);
				}
			}
		}

		public void askWearBackpack(ushort id, byte quality, byte[] state, bool playEffect)
		{
			ItemBackpackAsset asset = Assets.find(EAssetType.ITEM, id) as ItemBackpackAsset;
			askWearBackpack(asset, quality, state, playEffect);
		}

		public void askWearBackpack(ItemBackpackAsset asset, byte quality, byte[] state, bool playEffect)
		{
			ushort currentID = backpack;
			byte currentQuality = backpackQuality;
			byte[] currentState = backpackState;

			SendWearBackpack.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), asset?.GUID ?? System.Guid.Empty, quality, state, playEffect);

			if (currentID != 0)
			{
				player.inventory.forceAddItem(new Item(currentID, 1, currentQuality, currentState), false);
			}
		}

		public void sendSwapBackpack(byte page, byte x, byte y)
		{
			if (page == 255 && backpackAsset == null)
			{
				// Client wants to remove backpack, but we are already not wearing backpack.
				return;
			}

			SendSwapBackpackRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
		}

		[System.Obsolete]
		public void tellVisualToggle(CSteamID steamID, byte index, bool toggle)
		{
			ReceiveVisualToggleState((EVisualToggleType) index, toggle);
		}

		private static readonly ClientInstanceMethod<EVisualToggleType, bool> SendVisualToggleState = ClientInstanceMethod<EVisualToggleType, bool>.Get(typeof(PlayerClothing), nameof(ReceiveVisualToggleState));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVisualToggle))]
		public void ReceiveVisualToggleState(EVisualToggleType type, bool toggle)
		{
			switch (type)
			{
				case EVisualToggleType.COSMETIC:
					if (thirdClothes != null)
					{
						thirdClothes.isVisual = toggle;
						thirdClothes.apply();
					}

					if (firstClothes != null)
					{
						firstClothes.isVisual = toggle;
						firstClothes.apply();
					}

					if (characterClothes != null)
					{
						characterClothes.isVisual = toggle;
						characterClothes.apply();
					}
					break;

				case EVisualToggleType.SKIN:
					isSkinned = toggle;

					if (player.equipment != null)
					{
						player.equipment.applySkinVisual();
						player.equipment.applyMythicVisual();
					}
					break;

				case EVisualToggleType.MYTHIC:
					if (thirdClothes != null)
					{
						thirdClothes.isMythic = toggle;
						thirdClothes.apply();
					}

					if (firstClothes != null)
					{
						firstClothes.isMythic = toggle;
						firstClothes.apply();
					}

					if (characterClothes != null)
					{
						characterClothes.isMythic = toggle;
						characterClothes.apply();
					}

					if (player.equipment != null)
					{
						player.equipment.applyMythicVisual();
					}
					break;
			}

			VisualToggleChanged?.Invoke(this);
		}

		public void ServerSetVisualToggleState(EVisualToggleType type, bool isVisible)
		{
			SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), type, isVisible);
		}

		[System.Obsolete]
		public void askVisualToggle(CSteamID steamID, byte index)
		{
			if (index > 2)
				return;

			EVisualToggleType type = (EVisualToggleType) index;
			ReceiveVisualToggleRequest(type);
		}

		private static readonly ServerInstanceMethod<EVisualToggleType> SendVisualToggleRequest = ServerInstanceMethod<EVisualToggleType>.Get(typeof(PlayerClothing), nameof(ReceiveVisualToggleRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askVisualToggle))]
		public void ReceiveVisualToggleRequest(EVisualToggleType type)
		{
			switch (type)
			{
				case EVisualToggleType.COSMETIC:
					SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), type, !isVisual);
					break;
				case EVisualToggleType.SKIN:
					SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), type, !isSkinned);
					break;
				case EVisualToggleType.MYTHIC:
					SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), type, !isMythic);
					break;
			}
		}

		public void sendVisualToggle(EVisualToggleType type)
		{
			SendVisualToggleRequest.Invoke(GetNetId(), ENetReliability.Unreliable, type);
		}

		[System.Obsolete]
		public void tellWearVest(CSteamID steamID, ushort id, byte quality, byte[] state)
		{
			Asset asset = Assets.find(EAssetType.ITEM, id);
			ReceiveWearVest(asset?.GUID ?? System.Guid.Empty, quality, state, false);
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, byte[], bool> SendWearVest = ClientInstanceMethod<System.Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), nameof(ReceiveWearVest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWearVest))]
		public void ReceiveWearVest(System.Guid id, byte quality, byte[] state, bool playEffect)
		{
			if (thirdClothes == null)
			{
				return;
			}

			thirdClothes.vestGuid = id;
			vestQuality = quality;
			vestState = state;
			thirdClothes.apply();

			if (characterClothes != null)
			{
				characterClothes.vestGuid = id;
				characterClothes.apply();

				Characters.active.vest = vest;
			}

			UpdateStatModifiers();

			onVestUpdated?.Invoke(vest, quality, state);

			OnVestChanged_Global?.Invoke(this);

			if (channel.IsLocalPlayer && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.vestAsset);
			}

#if !DEDICATED_SERVER
			if (playEffect && thirdClothes.vestAsset != null)
			{
				player.PlayAudioReference(thirdClothes.vestAsset.wearAudio);
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askSwapVest(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveSwapVestRequest(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapVestRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapVestRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapVest))]
		public void ReceiveSwapVestRequest(byte page, byte x, byte y)
		{
			if (player.equipment.checkSelection(PlayerInventory.VEST))
			{
				if (player.equipment.isBusy)
				{
					return;
				}

				player.equipment.dequip();
			}

			if (page == 255)
			{
				if (vestAsset == null)
				{
					return;
				}

				askWearVest(0, 0, new byte[0], true);
			}
			else
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index == 255)
				{
					return;
				}

				ItemJar jar = player.inventory.getItem(page, index);
				ItemAsset asset = jar.GetAsset();

				if (asset != null && asset.type == EItemType.VEST)
				{
					player.inventory.removeItem(page, index);

					askWearVest(jar.item.id, jar.item.quality, jar.item.state, true);
				}
			}
		}

		public void askWearVest(ushort id, byte quality, byte[] state, bool playEffect)
		{
			ItemVestAsset asset = Assets.find(EAssetType.ITEM, id) as ItemVestAsset;
			askWearVest(asset, quality, state, playEffect);
		}

		public void askWearVest(ItemVestAsset asset, byte quality, byte[] state, bool playEffect)
		{
			ushort currentID = vest;
			byte currentQuality = vestQuality;
			byte[] currentState = vestState;

			SendWearVest.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), asset?.GUID ?? System.Guid.Empty, quality, state, playEffect);

			if (currentID != 0)
			{
				player.inventory.forceAddItem(new Item(currentID, 1, currentQuality, currentState), false);
			}
		}

		public void sendSwapVest(byte page, byte x, byte y)
		{
			if (page == 255 && vestAsset == null)
			{
				// Client wants to remove vest, but we are already not wearing vest.
				return;
			}

			SendSwapVestRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
		}

		[System.Obsolete]
		public void tellWearMask(CSteamID steamID, ushort id, byte quality, byte[] state)
		{
			Asset asset = Assets.find(EAssetType.ITEM, id);
			ReceiveWearMask(asset?.GUID ?? System.Guid.Empty, quality, state, false);
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, byte[], bool> SendWearMask = ClientInstanceMethod<System.Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), nameof(ReceiveWearMask));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWearMask))]
		public void ReceiveWearMask(System.Guid id, byte quality, byte[] state, bool playEffect)
		{
			if (thirdClothes == null)
			{
				return;
			}

			thirdClothes.maskGuid = id;
			maskQuality = quality;
			maskState = state;
			thirdClothes.apply();

			if (characterClothes != null)
			{
				characterClothes.maskGuid = id;
				characterClothes.apply();

				Characters.active.mask = mask;
			}

			UpdateStatModifiers();

			onMaskUpdated?.Invoke(mask, quality, state);

			OnMaskChanged_Global?.Invoke(this);

			if (channel.IsLocalPlayer && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.maskAsset);
			}

#if !DEDICATED_SERVER
			if (playEffect && thirdClothes.maskAsset != null)
			{
				player.PlayAudioReference(thirdClothes.maskAsset.wearAudio);
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askSwapMask(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveSwapMaskRequest(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapMaskRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapMaskRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapMask))]
		public void ReceiveSwapMaskRequest(byte page, byte x, byte y)
		{
			if (page == 255)
			{
				if (maskAsset == null)
				{
					return;
				}

				askWearMask(0, 0, new byte[0], true);
			}
			else
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index == 255)
				{
					return;
				}

				ItemJar jar = player.inventory.getItem(page, index);
				ItemAsset asset = jar.GetAsset();

				if (asset != null && asset.type == EItemType.MASK)
				{
					player.inventory.removeItem(page, index);

					askWearMask(jar.item.id, jar.item.quality, jar.item.state, true);
				}
			}
		}

		public void askWearMask(ushort id, byte quality, byte[] state, bool playEffect)
		{
			ItemMaskAsset asset = Assets.find(EAssetType.ITEM, id) as ItemMaskAsset;
			askWearMask(asset, quality, state, playEffect);
		}

		public void askWearMask(ItemMaskAsset asset, byte quality, byte[] state, bool playEffect)
		{
			ushort currentID = mask;
			byte currentQuality = maskQuality;
			byte[] currentState = maskState;

			SendWearMask.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), asset?.GUID ?? System.Guid.Empty, quality, state, playEffect);

			if (currentID != 0)
			{
				player.inventory.forceAddItem(new Item(currentID, 1, currentQuality, currentState), false);
			}
		}

		public void sendSwapMask(byte page, byte x, byte y)
		{
			if (page == 255 && maskAsset == null)
			{
				// Client wants to remove mask, but we are already not wearing mask.
				return;
			}

			SendSwapMaskRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
		}

		[System.Obsolete]
		public void tellWearGlasses(CSteamID steamID, ushort id, byte quality, byte[] state)
		{
			Asset asset = Assets.find(EAssetType.ITEM, id);
			ReceiveWearGlasses(asset?.GUID ?? System.Guid.Empty, quality, state, false);
		}

		private static readonly ClientInstanceMethod<System.Guid, byte, byte[], bool> SendWearGlasses = ClientInstanceMethod<System.Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), nameof(ReceiveWearGlasses));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellWearGlasses))]
		public void ReceiveWearGlasses(System.Guid id, byte quality, byte[] state, bool playEffect)
		{
			if (thirdClothes == null)
			{
				return;
			}

			thirdClothes.glassesGuid = id;
			glassesQuality = quality;
			glassesState = state;
			thirdClothes.apply();

			if (characterClothes != null)
			{
				characterClothes.glassesGuid = id;
				characterClothes.apply();

				Characters.active.glasses = glasses;
			}

			onGlassesUpdated?.Invoke(glasses, quality, state);

			UpdateStatModifiers();

			OnGlassesChanged_Global?.Invoke(this);

			if (channel.IsLocalPlayer && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.glassesAsset);
			}

#if !DEDICATED_SERVER
			if (playEffect && thirdClothes.glassesAsset != null)
			{
				player.PlayAudioReference(thirdClothes.glassesAsset.wearAudio);
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askSwapGlasses(CSteamID steamID, byte page, byte x, byte y)
		{
			ReceiveSwapGlassesRequest(page, x, y);
		}

		private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapGlassesRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapGlassesRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapGlasses))]
		public void ReceiveSwapGlassesRequest(byte page, byte x, byte y)
		{
			if (page == 255)
			{
				if (glassesAsset == null)
				{
					return;
				}

				askWearGlasses(0, 0, new byte[0], true);
			}
			else
			{
				byte index = player.inventory.getIndex(page, x, y);

				if (index == 255)
				{
					return;
				}

				ItemJar jar = player.inventory.getItem(page, index);
				ItemAsset asset = jar.GetAsset();

				if (asset != null && asset.type == EItemType.GLASSES)
				{
					player.inventory.removeItem(page, index);

					askWearGlasses(jar.item.id, jar.item.quality, jar.item.state, true);
				}
			}
		}

		public void askWearGlasses(ushort id, byte quality, byte[] state, bool playEffect)
		{
			ItemGlassesAsset asset = Assets.find(EAssetType.ITEM, id) as ItemGlassesAsset;
			askWearGlasses(asset, quality, state, playEffect);
		}

		public void askWearGlasses(ItemGlassesAsset asset, byte quality, byte[] state, bool playEffect)
		{
			ushort currentID = glasses;
			byte currentQuality = glassesQuality;
			byte[] currentState = glassesState;

			SendWearGlasses.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), asset?.GUID ?? System.Guid.Empty, quality, state, playEffect);

			if (currentID != 0)
			{
				player.inventory.forceAddItem(new Item(currentID, 1, currentQuality, currentState), false);
			}
		}

		public void sendSwapGlasses(byte page, byte x, byte y)
		{
			if (page == 255 && glassesAsset == null)
			{
				// Client wants to remove glasses, but we are already not wearing glasses.
				return;
			}

			SendSwapGlassesRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
		}

		[System.Obsolete]
		public void tellClothing(CSteamID steamID, ushort newShirt, byte newShirtQuality, byte[] newShirtState, ushort newPants, byte newPantsQuality, byte[] newPantsState, ushort newHat, byte newHatQuality, byte[] newHatState, ushort newBackpack, byte newBackpackQuality, byte[] newBackpackState, ushort newVest, byte newVestQuality, byte[] newVestState, ushort newMask, byte newMaskQuality, byte[] newMaskState, ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState, bool newVisual, bool newSkinned, bool newMythic)
		{ }

		private static readonly ClientInstanceMethod SendClothingState = ClientInstanceMethod.Get(typeof(PlayerClothing), nameof(ReceiveClothingState));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveClothingState(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Guid newShirt;
			reader.ReadGuid(out newShirt);
			byte newShirtQuality;
			reader.ReadUInt8(out newShirtQuality);
			byte[] newShirtState;
			reader.ReadStateArray(out newShirtState);
			System.Guid newPants;
			reader.ReadGuid(out newPants);
			byte newPantsQuality;
			reader.ReadUInt8(out newPantsQuality);
			byte[] newPantsState;
			reader.ReadStateArray(out newPantsState);
			System.Guid newHat;
			reader.ReadGuid(out newHat);
			byte newHatQuality;
			reader.ReadUInt8(out newHatQuality);
			byte[] newHatState;
			reader.ReadStateArray(out newHatState);
			System.Guid newBackpack;
			reader.ReadGuid(out newBackpack);
			byte newBackpackQuality;
			reader.ReadUInt8(out newBackpackQuality);
			byte[] newBackpackState;
			reader.ReadStateArray(out newBackpackState);
			System.Guid newVest;
			reader.ReadGuid(out newVest);
			byte newVestQuality;
			reader.ReadUInt8(out newVestQuality);
			byte[] newVestState;
			reader.ReadStateArray(out newVestState);
			System.Guid newMask;
			reader.ReadGuid(out newMask);
			byte newMaskQuality;
			reader.ReadUInt8(out newMaskQuality);
			byte[] newMaskState;
			reader.ReadStateArray(out newMaskState);
			System.Guid newGlasses;
			reader.ReadGuid(out newGlasses);
			byte newGlassesQuality;
			reader.ReadUInt8(out newGlassesQuality);
			byte[] newGlassesState;
			reader.ReadStateArray(out newGlassesState);
			bool newVisual;
			reader.ReadBit(out newVisual);
			bool newSkinned;
			reader.ReadBit(out newSkinned);
			bool newMythic;
			reader.ReadBit(out newMythic);

			Profiler.BeginSample("tellClothing");
			player.animator.NotifyClothingIsVisible();

			if (channel.IsLocalPlayer)
			{
				Player.isLoadingClothing = false;
			}

			if (thirdClothes != null)
			{
				Profiler.BeginSample("Update Third-Person Clothes");
				thirdClothes.face = channel.owner.face;
				thirdClothes.hair = channel.owner.hair;
				thirdClothes.beard = channel.owner.beard;

				thirdClothes.skin = channel.owner.skin;
				thirdClothes.color = channel.owner.color;
				thirdClothes.BeardColor = channel.owner.BeardColor;

				thirdClothes.shirtGuid = newShirt;
				shirtQuality = newShirtQuality;
				shirtState = newShirtState;
				thirdClothes.pantsGuid = newPants;
				pantsQuality = newPantsQuality;
				pantsState = newPantsState;
				thirdClothes.hatGuid = newHat;
				hatQuality = newHatQuality;
				hatState = newHatState;
				thirdClothes.backpackGuid = newBackpack;
				backpackQuality = newBackpackQuality;
				backpackState = newBackpackState;
				thirdClothes.vestGuid = newVest;
				vestQuality = newVestQuality;
				vestState = newVestState;
				thirdClothes.maskGuid = newMask;
				maskQuality = newMaskQuality;
				maskState = newMaskState;
				thirdClothes.glassesGuid = newGlasses;
				glassesQuality = newGlassesQuality;
				glassesState = newGlassesState;

				thirdClothes.isVisual = newVisual;
				thirdClothes.isMythic = newMythic;

				thirdClothes.apply();
				Profiler.EndSample();
			}

			if (firstClothes != null)
			{
				Profiler.BeginSample("Update First-Person Clothes");
				firstClothes.skin = channel.owner.skin;
				firstClothes.shirtGuid = newShirt;

				firstClothes.isVisual = newVisual;
				firstClothes.isMythic = newMythic;

				firstClothes.apply();
				Profiler.EndSample();
			}

			if (characterClothes != null)
			{
				Profiler.BeginSample("Update Preview Clothes");
				characterClothes.face = channel.owner.face;
				characterClothes.hair = channel.owner.hair;
				characterClothes.beard = channel.owner.beard;

				characterClothes.skin = channel.owner.skin;
				characterClothes.color = channel.owner.color;
				characterClothes.BeardColor = channel.owner.BeardColor;

				characterClothes.shirtGuid = newShirt;
				characterClothes.pantsGuid = newPants;
				characterClothes.hatGuid = newHat;
				characterClothes.backpackGuid = newBackpack;
				characterClothes.vestGuid = newVest;
				characterClothes.maskGuid = newMask;
				characterClothes.glassesGuid = newGlasses;

				characterClothes.isVisual = newVisual;
				characterClothes.isMythic = newMythic;

				characterClothes.apply();

				Characters.active.shirt = shirt;
				Characters.active.pants = pants;
				Characters.active.hat = hat;
				Characters.active.backpack = backpack;
				Characters.active.vest = vest;
				Characters.active.mask = mask;
				Characters.active.glasses = glasses;
				Characters.hasPlayed = true;
				Profiler.EndSample();
			}

			isSkinned = newSkinned;
			player.equipment.applySkinVisual();
			player.equipment.applyMythicVisual();

			UpdateStatModifiers();

			Profiler.BeginSample("Invoke Events");
			onShirtUpdated?.Invoke(shirt, newShirtQuality, newShirtState);
			OnShirtChanged_Global?.Invoke(this);

			onPantsUpdated?.Invoke(pants, newPantsQuality, newPantsState);
			OnPantsChanged_Global?.Invoke(this);

			onHatUpdated?.Invoke(hat, newHatQuality, newHatState);
			OnHatChanged_Global?.Invoke(this);

			onBackpackUpdated?.Invoke(backpack, newBackpackQuality, newBackpackState);
			OnBackpackChanged_Global?.Invoke(this);

			onVestUpdated?.Invoke(vest, newVestQuality, newVestState);
			OnVestChanged_Global?.Invoke(this);

			onMaskUpdated?.Invoke(mask, newMaskQuality, newMaskState);
			OnMaskChanged_Global?.Invoke(this);

			onGlassesUpdated?.Invoke(glasses, newGlassesQuality, newGlassesState);
			OnGlassesChanged_Global?.Invoke(this);

			Profiler.EndSample();
			Profiler.EndSample();

			if (channel.IsLocalPlayer && thirdClothes != null && !Provider.isServer)
			{
				ClientAssetIntegrity.QueueRequest(thirdClothes.shirtAsset);
				ClientAssetIntegrity.QueueRequest(thirdClothes.pantsAsset);
				ClientAssetIntegrity.QueueRequest(thirdClothes.hatAsset);
				ClientAssetIntegrity.QueueRequest(thirdClothes.backpackAsset);
				ClientAssetIntegrity.QueueRequest(thirdClothes.vestAsset);
				ClientAssetIntegrity.QueueRequest(thirdClothes.maskAsset);
				ClientAssetIntegrity.QueueRequest(thirdClothes.glassesAsset);
			}
		}

		public void updateClothes(ushort newShirt, byte newShirtQuality, byte[] newShirtState, ushort newPants, byte newPantsQuality, byte[] newPantsState, ushort newHat, byte newHatQuality, byte[] newHatState, ushort newBackpack, byte newBackpackQuality, byte[] newBackpackState, ushort newVest, byte newVestQuality, byte[] newVestState, ushort newMask, byte newMaskQuality, byte[] newMaskState, ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState)
		{
			Asset newShirtAsset = Assets.find(EAssetType.ITEM, newShirt);
			Asset newPantsAsset = Assets.find(EAssetType.ITEM, newPants);
			Asset newHatAsset = Assets.find(EAssetType.ITEM, newHat);
			Asset newBackpackAsset = Assets.find(EAssetType.ITEM, newBackpack);
			Asset newVestAsset = Assets.find(EAssetType.ITEM, newVest);
			Asset newMaskAsset = Assets.find(EAssetType.ITEM, newMask);
			Asset newGlassesAsset = Assets.find(EAssetType.ITEM, newGlasses);

			// If changing this remember to change WriteClothingState.
			SendClothingState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), (NetPakWriter writer) =>
			{
				writer.WriteGuid(newShirtAsset?.GUID ?? System.Guid.Empty);
				writer.WriteUInt8(newShirtQuality);
				writer.WriteStateArray(newShirtState);
				writer.WriteGuid(newPantsAsset?.GUID ?? System.Guid.Empty);
				writer.WriteUInt8(newPantsQuality);
				writer.WriteStateArray(newPantsState);
				writer.WriteGuid(newHatAsset?.GUID ?? System.Guid.Empty);
				writer.WriteUInt8(newHatQuality);
				writer.WriteStateArray(newHatState);
				writer.WriteGuid(newBackpackAsset?.GUID ?? System.Guid.Empty);
				writer.WriteUInt8(newBackpackQuality);
				writer.WriteStateArray(newBackpackState);
				writer.WriteGuid(newVestAsset?.GUID ?? System.Guid.Empty);
				writer.WriteUInt8(newVestQuality);
				writer.WriteStateArray(newVestState);
				writer.WriteGuid(newMaskAsset?.GUID ?? System.Guid.Empty);
				writer.WriteUInt8(newMaskQuality);
				writer.WriteStateArray(newMaskState);
				writer.WriteGuid(newGlassesAsset?.GUID ?? System.Guid.Empty);
				writer.WriteUInt8(newGlassesQuality);
				writer.WriteStateArray(newGlassesState);
				writer.WriteBit(isVisual);
				writer.WriteBit(isSkinned);
				writer.WriteBit(isMythic);
			});
		}

		[System.Obsolete]
		public void askClothing(CSteamID steamID)
		{ }

		private void WriteClothingState(NetPakWriter writer)
		{
			// If changing this remember to change updateClothes.
			writer.WriteGuid(shirtAsset?.GUID ?? System.Guid.Empty);
			writer.WriteUInt8(shirtQuality);
			writer.WriteStateArray(shirtState);
			writer.WriteGuid(pantsAsset?.GUID ?? System.Guid.Empty);
			writer.WriteUInt8(pantsQuality);
			writer.WriteStateArray(pantsState);
			writer.WriteGuid(hatAsset?.GUID ?? System.Guid.Empty);
			writer.WriteUInt8(hatQuality);
			writer.WriteStateArray(hatState);
			writer.WriteGuid(backpackAsset?.GUID ?? System.Guid.Empty);
			writer.WriteUInt8(backpackQuality);
			writer.WriteStateArray(backpackState);
			writer.WriteGuid(vestAsset?.GUID ?? System.Guid.Empty);
			writer.WriteUInt8(vestQuality);
			writer.WriteStateArray(vestState);
			writer.WriteGuid(maskAsset?.GUID ?? System.Guid.Empty);
			writer.WriteUInt8(maskQuality);
			writer.WriteStateArray(maskState);
			writer.WriteGuid(glassesAsset?.GUID ?? System.Guid.Empty);
			writer.WriteUInt8(glassesQuality);
			writer.WriteStateArray(glassesState);
			writer.WriteBit(isVisual);
			writer.WriteBit(isSkinned);
			writer.WriteBit(isMythic);
		}

		internal void SendInitialPlayerState(SteamPlayer client)
		{
			SendClothingState.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, WriteClothingState);
		}

		internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
		{
			SendClothingState.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, WriteClothingState);
		}

		[System.Obsolete]
		public void tellSwapFace(CSteamID steamID, byte index)
		{
			ReceiveFaceState(index);
		}

		private static readonly ClientInstanceMethod<byte> SendFaceState = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveFaceState));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellSwapFace))]
		public void ReceiveFaceState(byte index)
		{
			channel.owner.face = index;

			if (thirdClothes != null)
			{
				thirdClothes.face = channel.owner.face;
				thirdClothes.apply();
			}

			if (characterClothes != null)
			{
				characterClothes.face = channel.owner.face;
				characterClothes.apply();
			}
		}

		public bool ServerSetFace(byte index)
		{
			if (index >= Customization.FACES_FREE + Customization.FACES_PRO)
			{
				return false;
			}

			if (!channel.owner.isPro && index >= Customization.FACES_FREE)
			{
				return false;
			}

			SendFaceState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), index);
			return true;
		}

		[System.Obsolete]
		public void askSwapFace(CSteamID steamID, byte index)
		{
			ReceiveSwapFaceRequest(index);
		}

		private static readonly ServerInstanceMethod<byte> SendSwapFaceRequest = ServerInstanceMethod<byte>.Get(typeof(PlayerClothing), nameof(ReceiveSwapFaceRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = nameof(askSwapFace))]
		public void ReceiveSwapFaceRequest(byte index)
		{
			ServerSetFace(index);
		}

		public void sendSwapFace(byte index)
		{
			SendSwapFaceRequest.Invoke(GetNetId(), ENetReliability.Unreliable, index);
		}

		private void onStanceUpdated()
		{
			if (thirdClothes == null)
			{
				return;
			}

			if (player.movement.getVehicle() != null)
			{
				thirdClothes.hasBackpack = player.movement.getVehicle().passengers[player.movement.getSeat()].obj == null;
			}
			else
			{
				thirdClothes.hasBackpack = true;
			}
		}

		private void onLifeUpdated(bool isDead)
		{
			if (isDead)
			{
				if (Provider.isServer)
				{
					bool loseClothes = player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Clothes_PvP : Provider.modeConfigData.Players.Lose_Clothes_PvE;

					if (loseClothes)
					{
						if (shirtAsset != null && shirtAsset.shouldDropOnDeath)
						{
							ItemManager.dropItem(new Item(shirt, 1, shirtQuality, shirtState), transform.position, false, true, true);
						}

						if (pantsAsset != null && pantsAsset.shouldDropOnDeath)
						{
							ItemManager.dropItem(new Item(pants, 1, pantsQuality, pantsState), transform.position, false, true, true);
						}

						if (hatAsset != null && hatAsset.shouldDropOnDeath)
						{
							ItemManager.dropItem(new Item(hat, 1, hatQuality, hatState), transform.position, false, true, true);
						}

						if (backpackAsset != null && backpackAsset.shouldDropOnDeath)
						{
							ItemManager.dropItem(new Item(backpack, 1, backpackQuality, backpackState), transform.position, false, true, true);
						}

						if (vestAsset != null && vestAsset.shouldDropOnDeath)
						{
							ItemManager.dropItem(new Item(vest, 1, vestQuality, vestState), transform.position, false, true, true);
						}

						if (maskAsset != null && maskAsset.shouldDropOnDeath)
						{
							ItemManager.dropItem(new Item(mask, 1, maskQuality, maskState), transform.position, false, true, true);
						}

						if (glassesAsset != null && glassesAsset.shouldDropOnDeath)
						{
							ItemManager.dropItem(new Item(glasses, 1, glassesQuality, glassesState), transform.position, false, true, true);
						}

						thirdClothes.shirtAsset = null;
						shirtQuality = 0;
						thirdClothes.pantsAsset = null;
						pantsQuality = 0;
						thirdClothes.hatAsset = null;
						hatQuality = 0;
						thirdClothes.backpackAsset = null;
						backpackQuality = 0;
						thirdClothes.vestAsset = null;
						vestQuality = 0;
						thirdClothes.maskAsset = null;
						maskQuality = 0;
						thirdClothes.glassesAsset = null;
						glassesQuality = 0;

						shirtState = new byte[0];
						pantsState = new byte[0];
						hatState = new byte[0];
						backpackState = new byte[0];
						vestState = new byte[0];
						maskState = new byte[0];
						glassesState = new byte[0];

						SendClothingState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), WriteClothingState);
					}
				}
			}
		}

		internal void InitializePlayer()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				player.stance.onStanceUpdated += onStanceUpdated;
			}

			if (channel.IsLocalPlayer)
			{
				if (player.first != null)
				{
					firstClothes = player.first.Find("Camera").Find("Viewmodel").GetComponent<HumanClothes>();
					firstClothes.isMine = true;
					firstClothes.ShouldHairOverridesUseFallbackColor = !player.channel.owner.isPro;
				}

				if (player.third != null)
				{
					thirdClothes = player.third.GetComponent<HumanClothes>();
					thirdClothes.ShouldHairOverridesUseFallbackColor = !player.channel.owner.isPro;
				}

				if (player.character != null)
				{
					characterClothes = player.character.GetComponent<HumanClothes>();
					characterClothes.ShouldHairOverridesUseFallbackColor = !player.channel.owner.isPro;
				}
			}
			else
			{
				if (player.third != null)
				{
					thirdClothes = player.third.GetComponent<HumanClothes>();
				}
			}

			if (firstClothes != null)
			{
				firstClothes.visualShirt = channel.owner.shirtItem;
				firstClothes.hand = channel.owner.IsLeftHanded;
			}

			if (thirdClothes != null)
			{
				thirdClothes.visualShirt = channel.owner.shirtItem;
				thirdClothes.visualPants = channel.owner.pantsItem;
				thirdClothes.visualHat = channel.owner.hatItem;
				thirdClothes.visualBackpack = channel.owner.backpackItem;
				thirdClothes.visualVest = channel.owner.vestItem;
				thirdClothes.visualMask = channel.owner.maskItem;
				thirdClothes.visualGlasses = channel.owner.glassesItem;
				thirdClothes.hand = channel.owner.IsLeftHanded;
			}

			if (characterClothes != null)
			{
				characterClothes.visualShirt = channel.owner.shirtItem;
				characterClothes.visualPants = channel.owner.pantsItem;
				characterClothes.visualHat = channel.owner.hatItem;
				characterClothes.visualBackpack = channel.owner.backpackItem;
				characterClothes.visualVest = channel.owner.vestItem;
				characterClothes.visualMask = channel.owner.maskItem;
				characterClothes.visualGlasses = channel.owner.glassesItem;
				characterClothes.hand = channel.owner.IsLeftHanded;
			}

			isSkinned = true;

			if (Provider.isServer)
			{
				load();

				player.life.onLifeUpdated += onLifeUpdated;
			}
		}

		private bool wasLoadCalled;

		public void load()
		{
			wasLoadCalled = true;

			thirdClothes.visualShirt = channel.owner.shirtItem;
			thirdClothes.visualPants = channel.owner.pantsItem;
			thirdClothes.visualHat = channel.owner.hatItem;
			thirdClothes.visualBackpack = channel.owner.backpackItem;
			thirdClothes.visualVest = channel.owner.vestItem;
			thirdClothes.visualMask = channel.owner.maskItem;
			thirdClothes.visualGlasses = channel.owner.glassesItem;

			if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Clothing.dat") && Level.info.type == ELevelType.SURVIVAL)
			{
				Block block = PlayerSavedata.readBlock(channel.owner.playerID, "/Player/Clothing.dat", 0);
				byte version = block.readByte();

				if (version > 1)
				{
					if (version > 6)
					{
						thirdClothes.shirtGuid = block.readGUID();
					}
					else
					{
						thirdClothes.shirt = block.readUInt16();
					}

					shirtQuality = block.readByte();

					if (version > 6)
					{
						thirdClothes.pantsGuid = block.readGUID();
					}
					else
					{
						thirdClothes.pants = block.readUInt16();
					}

					pantsQuality = block.readByte();

					if (version > 6)
					{
						thirdClothes.hatGuid = block.readGUID();
					}
					else
					{
						thirdClothes.hat = block.readUInt16();
					}

					hatQuality = block.readByte();

					if (version > 6)
					{
						thirdClothes.backpackGuid = block.readGUID();
					}
					else
					{
						thirdClothes.backpack = block.readUInt16();
					}

					backpackQuality = block.readByte();

					if (version > 6)
					{
						thirdClothes.vestGuid = block.readGUID();
					}
					else
					{
						thirdClothes.vest = block.readUInt16();
					}

					vestQuality = block.readByte();

					if (version > 6)
					{
						thirdClothes.maskGuid = block.readGUID();
					}
					else
					{
						thirdClothes.mask = block.readUInt16();
					}

					maskQuality = block.readByte();

					if (version > 6)
					{
						thirdClothes.glassesGuid = block.readGUID();
					}
					else
					{
						thirdClothes.glasses = block.readUInt16();
					}

					glassesQuality = block.readByte();

					if (version > 2)
					{
						thirdClothes.isVisual = block.readBoolean();
					}

					if (version > 5)
					{
						isSkinned = block.readBoolean();
						thirdClothes.isMythic = block.readBoolean();
					}
					else
					{
						isSkinned = true;
						thirdClothes.isMythic = true;
					}

					if (version > 4)
					{
						shirtState = block.readByteArray();
						pantsState = block.readByteArray();
						hatState = block.readByteArray();
						backpackState = block.readByteArray();
						vestState = block.readByteArray();
						maskState = block.readByteArray();
						glassesState = block.readByteArray();
					}
					else
					{
						shirtState = new byte[0];
						pantsState = new byte[0];
						hatState = new byte[0];
						backpackState = new byte[0];
						vestState = new byte[0];
						maskState = new byte[0];
						glassesState = new byte[0];

						if (glasses == 334)
						{
							glassesState = new byte[1];
						}
					}

					thirdClothes.apply();
					UpdateStatModifiers();

					return;
				}
			}

			thirdClothes.shirtAsset = null;
			shirtQuality = 0;
			thirdClothes.pantsAsset = null;
			pantsQuality = 0;
			thirdClothes.hatAsset = null;
			hatQuality = 0;
			thirdClothes.backpackAsset = null;
			backpackQuality = 0;
			thirdClothes.vestAsset = null;
			vestQuality = 0;
			thirdClothes.maskAsset = null;
			maskQuality = 0;
			thirdClothes.glassesAsset = null;
			glassesQuality = 0;
			shirtState = new byte[0];
			pantsState = new byte[0];
			hatState = new byte[0];
			backpackState = new byte[0];
			vestState = new byte[0];
			maskState = new byte[0];
			glassesState = new byte[0];

			thirdClothes.apply();
			UpdateStatModifiers();
		}

		public void save()
		{
			if (!wasLoadCalled)
				return;

			bool loseClothes = player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Clothes_PvP : Provider.modeConfigData.Players.Lose_Clothes_PvE;

			if ((player.life.isDead && loseClothes) || thirdClothes == null)
			{
				if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Clothing.dat"))
				{
					PlayerSavedata.deleteFile(channel.owner.playerID, "/Player/Clothing.dat");
				}
			}
			else
			{
				Block block = new Block();
				block.writeByte(SAVEDATA_VERSION);

				block.writeGUID(thirdClothes.shirtGuid);
				block.writeByte(shirtQuality);
				block.writeGUID(thirdClothes.pantsGuid);
				block.writeByte(pantsQuality);
				block.writeGUID(thirdClothes.hatGuid);
				block.writeByte(hatQuality);
				block.writeGUID(thirdClothes.backpackGuid);
				block.writeByte(backpackQuality);
				block.writeGUID(thirdClothes.vestGuid);
				block.writeByte(vestQuality);
				block.writeGUID(thirdClothes.maskGuid);
				block.writeByte(maskQuality);
				block.writeGUID(thirdClothes.glassesGuid);
				block.writeByte(glassesQuality);

				block.writeBoolean(isVisual);
				block.writeBoolean(isSkinned);
				block.writeBoolean(isMythic);

				block.writeByteArray(shirtState);
				block.writeByteArray(pantsState);
				block.writeByteArray(hatState);
				block.writeByteArray(backpackState);
				block.writeByteArray(vestState);
				block.writeByteArray(maskState);
				block.writeByteArray(glassesState);

				PlayerSavedata.writeBlock(channel.owner.playerID, "/Player/Clothing.dat", block);
			}
		}

		internal float movementSpeedMultiplier = 1.0f;
		internal float fallingDamageMultiplier = 1.0f;
		internal bool preventsFallingBrokenBones = false;

		private void UpdateStatModifiers()
		{
			movementSpeedMultiplier = 1.0f;
			fallingDamageMultiplier = 1.0f;
			preventsFallingBrokenBones = false;

			if (thirdClothes != null)
			{
				movementSpeedMultiplier *= thirdClothes.shirtAsset?.movementSpeedMultiplier ?? 1.0f;
				movementSpeedMultiplier *= thirdClothes.pantsAsset?.movementSpeedMultiplier ?? 1.0f;
				movementSpeedMultiplier *= thirdClothes.hatAsset?.movementSpeedMultiplier ?? 1.0f;
				movementSpeedMultiplier *= thirdClothes.backpackAsset?.movementSpeedMultiplier ?? 1.0f;
				movementSpeedMultiplier *= thirdClothes.vestAsset?.movementSpeedMultiplier ?? 1.0f;
				movementSpeedMultiplier *= thirdClothes.maskAsset?.movementSpeedMultiplier ?? 1.0f;
				movementSpeedMultiplier *= thirdClothes.glassesAsset?.movementSpeedMultiplier ?? 1.0f;

				fallingDamageMultiplier *= thirdClothes.shirtAsset?.fallingDamageMultiplier ?? 1.0f;
				fallingDamageMultiplier *= thirdClothes.pantsAsset?.fallingDamageMultiplier ?? 1.0f;
				fallingDamageMultiplier *= thirdClothes.hatAsset?.fallingDamageMultiplier ?? 1.0f;
				fallingDamageMultiplier *= thirdClothes.backpackAsset?.fallingDamageMultiplier ?? 1.0f;
				fallingDamageMultiplier *= thirdClothes.vestAsset?.fallingDamageMultiplier ?? 1.0f;
				fallingDamageMultiplier *= thirdClothes.maskAsset?.fallingDamageMultiplier ?? 1.0f;
				fallingDamageMultiplier *= thirdClothes.glassesAsset?.fallingDamageMultiplier ?? 1.0f;

				preventsFallingBrokenBones |= thirdClothes.shirtAsset?.preventsFallingBrokenBones ?? false;
				preventsFallingBrokenBones |= thirdClothes.pantsAsset?.preventsFallingBrokenBones ?? false;
				preventsFallingBrokenBones |= thirdClothes.hatAsset?.preventsFallingBrokenBones ?? false;
				preventsFallingBrokenBones |= thirdClothes.backpackAsset?.preventsFallingBrokenBones ?? false;
				preventsFallingBrokenBones |= thirdClothes.vestAsset?.preventsFallingBrokenBones ?? false;
				preventsFallingBrokenBones |= thirdClothes.maskAsset?.preventsFallingBrokenBones ?? false;
				preventsFallingBrokenBones |= thirdClothes.glassesAsset?.preventsFallingBrokenBones ?? false;
			}
		}
	}
}
