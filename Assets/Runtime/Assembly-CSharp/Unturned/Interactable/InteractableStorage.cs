////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned
{
	public interface IManualOnDestroy
	{
		void ManualOnDestroy();
	}

	public class InteractableStorage : Interactable, IManualOnDestroy, IBarricadePlacedHandler
	{
		private CSteamID _owner;
		public CSteamID owner => _owner;

		private CSteamID _group;
		public CSteamID group => _group;

		private Items _items;
		public Items items => _items;

		private Transform gunLargeTransform;
		private Transform gunSmallTransform;
		private Transform meleeTransform;
		private Transform itemTransform;
		protected Transform displayModel;
		protected ItemAsset displayAsset;

		public Item displayItem;
		public ushort displaySkin;
		public ushort displayMythic;
		public string displayTags = string.Empty;
		public string displayDynamicProps = string.Empty;

		private Quaternion displayRotation;

		public byte rot_comp;
		public byte rot_x;
		public byte rot_y;
		public byte rot_z;

		public bool isOpen;
		public Player opener;

		private bool isLocked;

		private bool _isDisplay;
		public bool isDisplay => _isDisplay;

		public bool despawnWhenDestroyed;

		/// <summary>
		/// If player gets too far away from this storage while using it, should we close out?
		/// False by default for trunk storage because player is inside vehicle.
		/// Plugins needed to be able to set this to false for "virtual storage" plugins,
		/// so we default to false and set to true if asset enables it.
		/// </summary>
		public bool shouldCloseWhenOutsideRange = false;

		private bool canPlayersOpen = true;

		protected bool getDisplayStatTrackerValue(out EStatTrackerType type, out int kills)
		{
			DynamicEconDetails details = new DynamicEconDetails(displayTags, displayDynamicProps);
			return details.getStatTrackerValue(out type, out kills);
		}

		private void onStateUpdated()
		{
			if (isDisplay)
			{
				updateDisplay();

				if (Dedicator.IsDedicatedServer)
				{
					BarricadeManager.sendStorageDisplay(transform, displayItem, displaySkin, displayMythic, displayTags, displayDynamicProps);
				}

				refreshDisplay();
			}

			rebuildState();
		}

		public delegate void RebuiltStateHandler(InteractableStorage storage, byte[] state, int size);
		public RebuiltStateHandler onStateRebuilt;

		public void rebuildState()
		{
			if (items == null)
			{
				return;
			}

			Block block = new Block();

			block.write(owner, group, items.getItemCount());

			for (byte index = 0; index < items.getItemCount(); index++)
			{
				ItemJar jar = items.getItem(index);

				block.write(jar.x, jar.y, jar.rot, jar.item.id, jar.item.amount, jar.item.quality, jar.item.state);
			}

			if (isDisplay)
			{
				block.write(displaySkin);
				block.write(displayMythic);
				block.write(string.IsNullOrEmpty(displayTags) ? string.Empty : displayTags);
				block.write(string.IsNullOrEmpty(displayDynamicProps) ? string.Empty : displayDynamicProps);
				block.write(rot_comp);
			}

			int size;
			byte[] state = block.getBytes(out size);
			if (onStateRebuilt == null)
			{
				// Default implementation, saves state.
				BarricadeManager.updateState(transform, state, size);
			}
			else
			{
				// Plugin implementation for virtual storage to hook state.
				// Vanilla code does not use this.
				onStateRebuilt(this, state, size);
			}
		}

		private void updateDisplay()
		{
			if (items != null && items.getItemCount() > 0)
			{
				if (displayItem == null || items.getItem(0).item != displayItem)
				{
					if (displayItem != null)
					{
						displaySkin = 0;
						displayMythic = 0;
						displayTags = string.Empty;
						displayDynamicProps = string.Empty;
					}

					displayItem = items.getItem(0).item;

					if (opener != null)
					{
						ItemAsset displayItemAsset = displayItem.GetAsset();
						bool isSharedSkin = displayItemAsset != null && displayItemAsset.sharedSkinLookupID != displayItemAsset.id;
						ushort lookupId = isSharedSkin ? displayItemAsset.sharedSkinLookupID : displayItem.id;

						int item;
						if (opener.channel.owner.getItemSkinItemDefID(lookupId, out item))
						{
							if (!isSharedSkin || displayItemAsset.SharedSkinShouldApplyVisuals)
							{
								displaySkin = Provider.provider.economyService.getInventorySkinID(item);
							}
							else
							{
								displaySkin = 0;
							}
							displayMythic = Provider.provider.economyService.getInventoryMythicID(item);
							if (displayMythic == 0)
							{
								displayMythic = opener.channel.owner.getParticleEffectForItemDef(item);
							}
							opener.channel.owner.getTagsAndDynamicPropsForItem(item, out displayTags, out displayDynamicProps);
						}
					}
				}
			}
			else
			{
				displayItem = null;
				displaySkin = 0;
				displayMythic = 0;
				displayTags = string.Empty;
				displayDynamicProps = string.Empty;
			}
		}

		public void setDisplay(ushort id, byte quality, byte[] state, ushort skin, ushort mythic, string tags, string dynamicProps)
		{
			if (id == 0)
			{
				displayItem = null;
			}
			else
			{
				displayItem = new Item(id, 0, quality, state);
			}

			displaySkin = skin;
			displayMythic = mythic;
			displayTags = tags;
			displayDynamicProps = dynamicProps;

			refreshDisplay();
		}

		public byte getRotation(byte rot_x, byte rot_y, byte rot_z)
		{
			byte rotComp = (byte) ((rot_x << 4) | (rot_y << 2) | rot_z);
			return rotComp;
		}

		public void applyRotation(byte rotComp)
		{
			rot_comp = rotComp;
			rot_x = (byte) ((rotComp >> 4) & 3);
			rot_y = (byte) ((rotComp >> 2) & 3);
			rot_z = (byte) (rotComp & 3);

			displayRotation = Quaternion.Euler(rot_x * 90, rot_y * 90, rot_z * 90);
		}

		// used by networking, refrheses display
		public void setRotation(byte rotComp)
		{
			applyRotation(rotComp);
			refreshDisplay();
		}

		public virtual void refreshDisplay()
		{
			if (displayModel != null)
			{
				Destroy(displayModel.gameObject);
				displayModel = null;
				displayAsset = null;
			}

			if (displayItem == null)
			{
				return;
			}

			if (gunLargeTransform == null || gunSmallTransform == null || meleeTransform == null || itemTransform == null)
			{
				return;
			}

			displayAsset = displayItem.GetAsset();

			if (displayAsset == null)
			{
				return;
			}

			if (displaySkin != 0)
			{
				SkinAsset skinAsset = Assets.find(EAssetType.SKIN, displaySkin) as SkinAsset;

				if (skinAsset == null)
				{
					return;
				}

				displayModel = ItemTool.getItem(displayItem.id, displaySkin, displayItem.quality, displayItem.state, /*viewmodel*/ false, displayAsset, /*shouldDestroyColliders*/ true, getDisplayStatTrackerValue);

				if (displayMythic != 0)
				{
					ItemTool.ApplyMythicalEffect(displayModel, displayMythic, EEffectType.THIRD);
				}
			}
			else
			{
				displayModel = ItemTool.getItem(displayItem.id, 0, displayItem.quality, displayItem.state, /*viewmodel*/ false, displayAsset, /*shouldDestroyColliders*/ true, getDisplayStatTrackerValue);

				if (displayMythic != 0)
				{
					EEffectType effectType = displayAsset.type == EItemType.BACKPACK || displayAsset.type == EItemType.VEST
						? EEffectType.BODY_COSMETIC : EEffectType.HEAD_COSMETIC;
					ItemTool.ApplyMythicalEffect(displayModel, displayMythic, EEffectType.HEAD_COSMETIC);
				}
			}

			if (displayModel == null)
			{
				return;
			}

			if (displayAsset.type == EItemType.GUN)
			{
				if (displayAsset.slot == ESlotType.PRIMARY)
				{
					displayModel.parent = gunLargeTransform;
				}
				else
				{
					displayModel.parent = gunSmallTransform;
				}
			}
			else if (displayAsset.type == EItemType.MELEE)
			{
				displayModel.parent = meleeTransform;
			}
			else
			{
				displayModel.parent = itemTransform;
			}

			displayModel.localPosition = Vector3.zero;
			displayModel.localRotation = displayRotation;
			displayModel.localScale = Vector3.one;

			displayModel.DestroyRigidbody();
		}

		public bool checkRot(CSteamID enemyPlayer, CSteamID enemyGroup)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			return !isLocked || enemyPlayer == owner || (group != CSteamID.Nil && enemyGroup == group);
		}

		public bool checkStore(CSteamID enemyPlayer, CSteamID enemyGroup)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			return (!isLocked || enemyPlayer == owner || (group != CSteamID.Nil && enemyGroup == group)) && !isOpen;
		}

		public override void updateState(Asset asset, byte[] state)
		{
			gunLargeTransform = transform.FindChildRecursive("Gun_Large");
			gunSmallTransform = transform.FindChildRecursive("Gun_Small");
			meleeTransform = transform.FindChildRecursive("Melee");
			itemTransform = transform.FindChildRecursive("Item");

			isLocked = ((ItemBarricadeAsset) asset).isLocked;
			_isDisplay = ((ItemStorageAsset) asset).isDisplay;
			shouldCloseWhenOutsideRange = ((ItemStorageAsset) asset).shouldCloseWhenOutsideRange;
			canPlayersOpen = ((ItemStorageAsset) asset).CanPlayersOpen;
			despawnWhenDestroyed = ((ItemStorageAsset) asset).ShouldDeleteContainedItemsOnDestroy;

			if (Provider.isServer)
			{
				Block block = new Block(state);

				_owner = (CSteamID) block.read(Types.STEAM_ID_TYPE);
				_group = (CSteamID) block.read(Types.STEAM_ID_TYPE);

				_items = new Items(PlayerInventory.STORAGE);
				items.resize(((ItemStorageAsset) asset).storage_x, ((ItemStorageAsset) asset).storage_y);

				byte count = block.readByte();
				for (byte index = 0; index < count; index++)
				{
					if (BarricadeManager.version > 7)
					{
						object[] objects = block.read(Types.BYTE_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.UINT16_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.BYTE_ARRAY_TYPE);

						ItemAsset item = Assets.find(EAssetType.ITEM, (ushort) objects[3]) as ItemAsset;

						if (item != null)
						{
							items.loadItem((byte) objects[0], (byte) objects[1], (byte) objects[2], new Item((ushort) objects[3], (byte) objects[4], (byte) objects[5], (byte[]) objects[6]));
						}
					}
					else
					{
						object[] objects = block.read(Types.BYTE_TYPE, Types.BYTE_TYPE, Types.UINT16_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.BYTE_ARRAY_TYPE);

						ItemAsset item = Assets.find(EAssetType.ITEM, (ushort) objects[2]) as ItemAsset;

						if (item != null)
						{
							items.loadItem((byte) objects[0], (byte) objects[1], 0, new Item((ushort) objects[2], (byte) objects[3], (byte) objects[4], (byte[]) objects[5]));
						}
					}
				}

				if (isDisplay)
				{
					displaySkin = block.readUInt16();
					displayMythic = block.readUInt16();

					if (BarricadeManager.version > 12)
					{
						displayTags = block.readString();
						displayDynamicProps = block.readString();
					}
					else
					{
						displayTags = string.Empty;
						displayDynamicProps = string.Empty;
					}

					if (BarricadeManager.version > 8)
					{
						applyRotation(block.readByte());
					}
					else
					{
						applyRotation(0);
					}
				}

				items.onStateUpdated = onStateUpdated;

				if (isDisplay)
				{
					updateDisplay();

					refreshDisplay();
				}
			}
			else
			{
				Block block = new Block(state);

				_owner = new CSteamID((ulong) block.read(Types.UINT64_TYPE));
				_group = new CSteamID((ulong) block.read(Types.UINT64_TYPE));

				if (state.Length > 16)
				{
					object[] objects = block.read(Types.UINT16_TYPE, Types.BYTE_TYPE, Types.BYTE_ARRAY_TYPE, Types.UINT16_TYPE, Types.UINT16_TYPE, Types.STRING_TYPE, Types.STRING_TYPE, Types.BYTE_TYPE);
					applyRotation((byte) objects[7]);
					setDisplay((ushort) objects[0], (byte) objects[1], (byte[]) objects[2], (ushort) objects[3], (ushort) objects[4], (string) objects[5], (string) objects[6]);
				}
			}

			//			byte[] newState = new byte[16];
			//			for(byte index = 0; index < 16; index ++)
			//			{
			//				newState[index] = state[index];
			//			}
			//
			//			state = newState;
		}

		public override bool checkInteractable()
		{
			return canPlayersOpen;
		}

		public override bool checkUseable()
		{
			return checkStore(Provider.client, Player.LocalPlayer.quests.groupID) && !PlayerUI.window.showCursor;
		}

		public override void use()
		{
			ClientInteract(InputEx.GetKey(ControlsSettings.other));
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			text = "";
			color = Color.white;

			if (checkUseable())
			{
				message = EPlayerMessage.STORAGE;
			}
			else
			{
				message = EPlayerMessage.LOCKED;
			}

			return true;
		}

		private void Start()
		{
			if (!Provider.isServer)
			{
				return;
			}

			if (BarricadeManager.version < 13)
			{
				onStateUpdated();
			}
		}

		public void ManualOnDestroy()
		{
			if (isDisplay)
			{
				setDisplay(0, 0, null, 0, 0, string.Empty, string.Empty);
			}

			if (!Provider.isServer)
			{
				return;
			}

			items.onStateUpdated = null;

			if (!despawnWhenDestroyed)
			{
				for (byte index = 0; index < items.getItemCount(); index++)
				{
					ItemJar jar = items.getItem(index);

					ItemManager.dropItem(jar.item, transform.position, false, true, true);
				}
			}

			items.clear();
			_items = null;

			if (isOpen)
			{
				if (opener != null)
				{
					if (opener.inventory.isStoring)
					{
						// closeStorage sets our opener/isOpen, but we also set them just in case.
						opener.inventory.closeStorageAndNotifyClient();
					}

					opener = null;
				}

				isOpen = false;
			}
		}

		public void ClientInteract(bool quickGrab)
		{
			SendInteractRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, quickGrab);
		}

		private static readonly ServerInstanceMethod<bool> SendInteractRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableStorage), nameof(ReceiveInteractRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 4)]
		public void ReceiveInteractRequest(in ServerInvocationContext context, bool quickGrab)
		{
			Player player = context.GetPlayer();

			if (!canPlayersOpen)
			{
				context.LogWarning("players are not allowed to open this type of storage");
				return;
			}

			if (player == null)
			{
				context.LogWarning("null player");
				return;
			}

			if (player.life.isDead)
			{
				context.LogWarning("player is dead");
				return;
			}

			if (player.inventory.isStoring && player.inventory.isStorageTrunk)
			{
				// Trunk storage takes priority while in vehicle.
				context.LogWarning("already in trunk storage");
				return;
			}

			if (player.animator.gesture == EPlayerGesture.ARREST_START)
			{
				context.LogWarning("under arrest");
				return;
			}

			Vector3 storagePosition = transform.position;
			if ((storagePosition - player.transform.position).sqrMagnitude > 400)
			{
				context.LogWarning("too far away");
				return;
			}

			Vector3 viewPosition = player.look.getEyesPosition();
			bool bHitSomething = Physics.Linecast(viewPosition, storagePosition, RayMasks.BLOCK_BARRICADE_INTERACT_LOS, QueryTriggerInteraction.Ignore);
			if (bHitSomething)
			{
				context.LogWarning("obstructed");
				return;
			}

			// checkStore may fail if player is already using that storage, so we close current storage beforehand.
			if (player.inventory.isStoring)
			{
				player.inventory.closeStorage();
			}

			if (checkStore(player.channel.owner.playerID.steamID, player.quests.groupID))
			{
				bool shouldAllow = true;
				BarricadeManager.onOpenStorageRequested?.Invoke(player.channel.owner.playerID.steamID, this, ref shouldAllow);

				if (!shouldAllow)
				{
					context.LogWarning("rejected by plugin");
					return;
				}

				if (isDisplay && quickGrab)
				{
					if (displayItem != null)
					{
						player.inventory.forceAddItem(displayItem, true);
						displayItem = null;
						displaySkin = 0;
						displayMythic = 0;
						displayTags = string.Empty;
						displayDynamicProps = string.Empty;
						items.removeItem(0);
					}
				}
				else
				{
					player.inventory.openStorage(this);
				}
			}
			else
			{
				player.sendMessage(EPlayerMessage.BUSY);
			}
		}

		internal static readonly ClientInstanceMethod<ushort, byte, byte[], ushort, ushort, string, string> SendDisplay
			= ClientInstanceMethod<ushort, byte, byte[], ushort, ushort, string, string>.Get(typeof(InteractableStorage), nameof(ReceiveDisplay));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveDisplay(ushort id, byte quality, byte[] state, ushort skin, ushort mythic, string tags, string dynamicProps)
		{
			setDisplay(id, quality, state, skin, mythic, tags, dynamicProps);
		}

		public void ClientSetDisplayRotation(byte rotComp)
		{
			SendRotDisplayRequest.Invoke(GetNetId(), NetTransport.ENetReliability.Unreliable, rotComp);
		}

		private static readonly ClientInstanceMethod<byte> SendRotDisplay = ClientInstanceMethod<byte>.Get(typeof(InteractableStorage), nameof(ReceiveRotDisplay));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveRotDisplay(byte rotComp)
		{
			setRotation(rotComp);
		}

		private static readonly ServerInstanceMethod<byte> SendRotDisplayRequest = ServerInstanceMethod<byte>.Get(typeof(InteractableStorage), nameof(ReceiveRotDisplayRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveRotDisplayRequest(in ServerInvocationContext context, byte rotComp)
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

			if (checkRot(player.channel.owner.playerID.steamID, player.quests.groupID) && isDisplay)
			{
				SendRotDisplay.InvokeAndLoopback(GetNetId(), NetTransport.ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), rotComp);
				rebuildState();
			}
		}

		public void OnBarricadePlaced(BarricadeRegion region, BarricadeDrop barricade)
		{
			if (barricade?.asset is ItemStorageAsset storageAsset)
			{
				storageAsset.AddDefaultContainedItemsToStorage(this);
			}
		}
	}
}
