////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	[NetEnum]
	public enum EMannequinUpdateMode
	{
		COSMETICS,
		ADD,
		REMOVE,
		SWAP
	}

	public class InteractableMannequin : Interactable, IManualOnDestroy
	{
		private CSteamID _owner;
		public CSteamID owner => _owner;

		private CSteamID _group;
		public CSteamID group => _group;

		private bool isLocked;

		public byte pose_comp;
		public bool mirror;
		public byte pose;

		public bool isUpdatable => Time.realtimeSinceStartup - updated > 0.5f;

		private float updated;

		private Animation anim;

		public HumanClothes clothes
		{
			get;
			private set;
		}

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

		public int visualShirt => clothes.visualShirt;

		public int visualPants => clothes.visualPants;

		public int visualHat => clothes.visualHat;

		public int visualBackpack => clothes.visualBackpack;

		public int visualVest => clothes.visualVest;

		public int visualMask => clothes.visualMask;

		public int visualGlasses => clothes.visualGlasses;

		public ushort shirt => clothes.shirt;

		public ushort pants => clothes.pants;

		public ushort hat => clothes.hat;

		public ushort backpack => clothes.backpack;

		public ushort vest => clothes.vest;

		public ushort mask => clothes.mask;

		public ushort glasses => clothes.glasses;

		/// <summary>
		/// Are any players standing on the mannequin?
		/// Used to prevent exploiting pose switches to push through objects.
		/// </summary>
		public bool isObstructedByPlayers()
		{
			// Vanilla mannequin pivots are centered.
			const float halfHeight = 1.0f;
			const float radius = 0.4f;
			Vector3 center = transform.position;
			Vector3 p0 = center + new Vector3(0.0f, -halfHeight + radius, 0.0f);
			Vector3 p1 = center + new Vector3(0.0f, halfHeight - radius, 0.0f);
			int mask = IsChildOfVehicle ? RayMasks.BLOCK_CHAR_HINGE_OVERLAP_ON_VEHICLE : RayMasks.BLOCK_CHAR_HINGE_OVERLAP;
			int numCollisions = Physics.OverlapCapsuleNonAlloc(p0, p1, radius, InteractableDoor.checkColliders, mask, QueryTriggerInteraction.Ignore);
			return numCollisions > 0;
		}

		public bool checkUpdate(CSteamID enemyPlayer, CSteamID enemyGroup)
		{
			if (Provider.isServer && !Dedicator.IsDedicatedServer) // sp, temp, remove this
			{
				return true;
			}

			return !isLocked || enemyPlayer == owner || (group != CSteamID.Nil && enemyGroup == group);
		}

		public byte getComp(bool mirror, byte pose)
		{
			byte mirrorComp = (byte) (mirror ? 1 : 0);
			byte poseComp = (byte) ((mirrorComp << 7) | pose);
			return poseComp;
		}

		public void applyPose(byte poseComp)
		{
			pose_comp = poseComp;
			mirror = ((poseComp >> 7) & 1) == 1;
			pose = (byte) (poseComp & 127);
		}

		// used by networking, refrheses display
		public void setPose(byte poseComp)
		{
			applyPose(poseComp);
			updatePose();
		}

		public void rebuildState()
		{
			Block block = new Block();

			block.write(owner, group);

			block.writeInt32(visualShirt);
			block.writeInt32(visualPants);
			block.writeInt32(visualHat);
			block.writeInt32(visualBackpack);
			block.writeInt32(visualVest);
			block.writeInt32(visualMask);
			block.writeInt32(visualGlasses);

			block.writeUInt16(clothes.shirt);
			block.writeByte(shirtQuality);
			block.writeUInt16(clothes.pants);
			block.writeByte(pantsQuality);
			block.writeUInt16(clothes.hat);
			block.writeByte(hatQuality);
			block.writeUInt16(clothes.backpack);
			block.writeByte(backpackQuality);
			block.writeUInt16(clothes.vest);
			block.writeByte(vestQuality);
			block.writeUInt16(clothes.mask);
			block.writeByte(maskQuality);
			block.writeUInt16(clothes.glasses);
			block.writeByte(glassesQuality);

			block.writeByteArray(shirtState);
			block.writeByteArray(pantsState);
			block.writeByteArray(hatState);
			block.writeByteArray(backpackState);
			block.writeByteArray(vestState);
			block.writeByteArray(maskState);
			block.writeByteArray(glassesState);

			block.writeByte(pose_comp);

			int size;
			byte[] state = block.getBytes(out size);
			BarricadeManager.updateState(transform, state, size);

			updated = Time.realtimeSinceStartup;
		}

		public void updateVisuals(int newVisualShirt, int newVisualPants, int newVisualHat, int newVisualBackpack, int newVisualVest, int newVisualMask, int newVisualGlasses)
		{
			clothes.visualShirt = newVisualShirt;
			clothes.visualPants = newVisualPants;
			clothes.visualHat = newVisualHat;
			clothes.visualBackpack = newVisualBackpack;
			clothes.visualVest = newVisualVest;
			clothes.visualMask = newVisualMask;
			clothes.visualGlasses = newVisualGlasses;
		}

		public void clearVisuals()
		{
			updateVisuals(0, 0, 0, 0, 0, 0, 0);
		}

		public void updateClothes(ushort newShirt, byte newShirtQuality, byte[] newShirtState, ushort newPants, byte newPantsQuality, byte[] newPantsState, ushort newHat, byte newHatQuality, byte[] newHatState, ushort newBackpack, byte newBackpackQuality, byte[] newBackpackState, ushort newVest, byte newVestQuality, byte[] newVestState, ushort newMask, byte newMaskQuality, byte[] newMaskState, ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState)
		{
			clothes.shirt = newShirt;
			shirtQuality = newShirtQuality;
			shirtState = newShirtState;
			clothes.pants = newPants;
			pantsQuality = newPantsQuality;
			pantsState = newPantsState;
			clothes.hat = newHat;
			hatQuality = newHatQuality;
			hatState = newHatState;
			clothes.backpack = newBackpack;
			backpackQuality = newBackpackQuality;
			backpackState = newBackpackState;
			clothes.vest = newVest;
			vestQuality = newVestQuality;
			vestState = newVestState;
			clothes.mask = newMask;
			maskQuality = newMaskQuality;
			maskState = newMaskState;
			clothes.glasses = newGlasses;
			glassesQuality = newGlassesQuality;
			glassesState = newGlassesState;
		}

		public void dropClothes()
		{
			if (shirt != 0)
			{
				ItemManager.dropItem(new Item(shirt, 1, shirtQuality, shirtState), transform.position, false, true, true);
			}

			if (pants != 0)
			{
				ItemManager.dropItem(new Item(pants, 1, pantsQuality, pantsState), transform.position, false, true, true);
			}

			if (hat != 0)
			{
				ItemManager.dropItem(new Item(hat, 1, hatQuality, hatState), transform.position, false, true, true);
			}

			if (backpack != 0)
			{
				ItemManager.dropItem(new Item(backpack, 1, backpackQuality, backpackState), transform.position, false, true, true);
			}

			if (vest != 0)
			{
				ItemManager.dropItem(new Item(vest, 1, vestQuality, vestState), transform.position, false, true, true);
			}

			if (mask != 0)
			{
				ItemManager.dropItem(new Item(mask, 1, maskQuality, maskState), transform.position, false, true, true);
			}

			if (glasses != 0)
			{
				ItemManager.dropItem(new Item(glasses, 1, glassesQuality, glassesState), transform.position, false, true, true);
			}

			clearClothes();
		}

		public void clearClothes()
		{
			updateClothes(0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0]);
		}

		public void updatePose()
		{
			string clip;
			switch (pose)
			{
				case 0:
					clip = "T";
					break;
				case 1:
					clip = "Classic";
					break;
				case 2:
					clip = "Lie";
					break;
				default:
					return;
			}

			if (anim != null)
			{
				anim.transform.localScale = new Vector3(mirror ? -1 : 1, 1, 1);
				anim.Play(clip);
			}
		}

		public void updateState(byte[] state)
		{
			Profiler.BeginSample("InteractableMannequin.ParseBytes");
			Block block = new Block(state);

			_owner = new CSteamID((ulong) block.read(Types.UINT64_TYPE));
			_group = new CSteamID((ulong) block.read(Types.UINT64_TYPE));

			clothes.skin = new Color32(210, 210, 210, 255);
			clothes.color = clothes.skin; // Hair color. Needed for hats with hair meshes.
			clothes.BeardColor = clothes.skin;

			clothes.visualShirt = block.readInt32();
			clothes.visualPants = block.readInt32();
			clothes.visualHat = block.readInt32();
			clothes.visualBackpack = block.readInt32();
			clothes.visualVest = block.readInt32();
			clothes.visualMask = block.readInt32();
			clothes.visualGlasses = block.readInt32();

			clothes.shirt = block.readUInt16();
			shirtQuality = block.readByte();
			clothes.pants = block.readUInt16();
			pantsQuality = block.readByte();
			clothes.hat = block.readUInt16();
			hatQuality = block.readByte();
			clothes.backpack = block.readUInt16();
			backpackQuality = block.readByte();
			clothes.vest = block.readUInt16();
			vestQuality = block.readByte();
			clothes.mask = block.readUInt16();
			maskQuality = block.readByte();
			clothes.glasses = block.readUInt16();
			glassesQuality = block.readByte();

			shirtState = block.readByteArray();
			pantsState = block.readByteArray();
			hatState = block.readByteArray();
			backpackState = block.readByteArray();
			vestState = block.readByteArray();
			maskState = block.readByteArray();
			glassesState = block.readByteArray();
			Profiler.EndSample();

			Profiler.BeginSample("InteractableMannequin.ApplyClothes");
			clothes.apply();
			Profiler.EndSample();

			setPose(block.readByte());
		}

		public override void updateState(Asset asset, byte[] state)
		{
			isLocked = ((ItemBarricadeAsset) asset).isLocked;

			Transform root = transform.Find("Root");
			anim = root.GetComponent<Animation>();
			clothes = root.GetOrAddComponent<HumanClothes>();
			clothes.ShouldHairOverridesUseFallbackColor = true;

			updateState(state);
		}

		public override bool checkUseable()
		{
			return checkUpdate(Provider.client, Player.LocalPlayer.quests.groupID) && !PlayerUI.window.showCursor;
		}

		public override void use()
		{
			if (InputEx.GetKey(ControlsSettings.other))
			{
				if (Player.LocalPlayer.equipment.useable is UseableClothing)
				{
					ClientRequestUpdate(EMannequinUpdateMode.ADD);
				}
				else
				{
					ClientRequestUpdate(EMannequinUpdateMode.REMOVE);
				}
			}
			else
			{
				PlayerUI.instance.mannequinUI.open(this);
				PlayerLifeUI.close();
			}
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

		public void ManualOnDestroy()
		{
			if (!Provider.isServer)
			{
				return;
			}

			dropClothes();
		}

		internal static readonly ClientInstanceMethod<byte> SendPose = ClientInstanceMethod<byte>.Get(typeof(InteractableMannequin), nameof(ReceivePose));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceivePose(byte poseComp)
		{
			setPose(poseComp);
		}

		internal void ClientSetPose(byte poseComp)
		{
			SendPoseRequest.Invoke(GetNetId(), ENetReliability.Unreliable, poseComp);
		}

		private static readonly ServerInstanceMethod<byte> SendPoseRequest = ServerInstanceMethod<byte>.Get(typeof(InteractableMannequin), nameof(ReceivePoseRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceivePoseRequest(in ServerInvocationContext context, byte poseComp)
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

			if (checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
			{
				if (isObstructedByPlayers())
				{
					context.LogWarning("obstructed by players");
					return;
				}

				byte x;
				byte y;
				ushort plant;
				BarricadeRegion region;
				if (BarricadeManager.tryGetRegion(transform, out x, out y, out plant, out region))
				{
					BarricadeManager.InternalSetMannequinPose(this, x, y, plant, poseComp);
				}
				else
				{
					context.LogWarning("invalid region");
				}
			}
		}

		private static readonly ClientInstanceMethod<byte[]> SendUpdate = ClientInstanceMethod<byte[]>.Get(typeof(InteractableMannequin), nameof(ReceiveUpdate));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
		public void ReceiveUpdate(byte[] state)
		{
			updateState(state);
		}

		internal void ClientRequestUpdate(EMannequinUpdateMode updateMode)
		{
			SendUpdateRequest.Invoke(GetNetId(), ENetReliability.Unreliable, updateMode);
		}

		private static readonly ServerInstanceMethod<EMannequinUpdateMode> SendUpdateRequest = ServerInstanceMethod<EMannequinUpdateMode>.Get(typeof(InteractableMannequin), nameof(ReceiveUpdateRequest));
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
		public void ReceiveUpdateRequest(in ServerInvocationContext context, EMannequinUpdateMode updateMode)
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

			if (player.equipment.isBusy)
			{
				return;
			}

			if (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished)
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

			if (isUpdatable && checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
			{
				switch (updateMode)
				{
					case EMannequinUpdateMode.COSMETICS:
						updateVisuals(player.clothing.visualShirt, player.clothing.visualPants, player.clothing.visualHat, player.clothing.visualBackpack, player.clothing.visualVest, player.clothing.visualMask, player.clothing.visualGlasses);

						if (shirt != 0)
						{
							player.inventory.forceAddItem(new Item(shirt, 1, shirtQuality, shirtState), false);
						}

						if (pants != 0)
						{
							player.inventory.forceAddItem(new Item(pants, 1, pantsQuality, pantsState), false);
						}

						if (hat != 0)
						{
							player.inventory.forceAddItem(new Item(hat, 1, hatQuality, hatState), false);
						}

						if (backpack != 0)
						{
							player.inventory.forceAddItem(new Item(backpack, 1, backpackQuality, backpackState), false);
						}

						if (vest != 0)
						{
							player.inventory.forceAddItem(new Item(vest, 1, vestQuality, vestState), false);
						}

						if (mask != 0)
						{
							player.inventory.forceAddItem(new Item(mask, 1, maskQuality, maskState), false);
						}

						if (glasses != 0)
						{
							player.inventory.forceAddItem(new Item(glasses, 1, glassesQuality, glassesState), false);
						}

						clearClothes();
						break;
					case EMannequinUpdateMode.ADD:
						if (!player.equipment.HasValidUseable || !player.equipment.IsEquipAnimationFinished || player.equipment.isBusy || player.equipment.asset == null || !(player.equipment.useable is UseableClothing))
						{
							return;
						}

						ItemJar item = player.inventory.getItem(player.equipment.equippedPage, player.inventory.getIndex(player.equipment.equippedPage, player.equipment.equipped_x, player.equipment.equipped_y));
						if (item == null || item.item == null)
						{
							return;
						}

						clearVisuals();

						switch (player.equipment.asset.type)
						{
							case EItemType.SHIRT:
								if (shirt != 0)
								{
									player.inventory.forceAddItem(new Item(shirt, 1, shirtQuality, shirtState), false);
								}
								clothes.shirt = item.item.id;
								shirtQuality = item.item.quality;
								shirtState = item.item.state;
								break;
							case EItemType.PANTS:
								if (pants != 0)
								{
									player.inventory.forceAddItem(new Item(pants, 1, pantsQuality, pantsState), false);
								}
								clothes.pants = item.item.id;
								pantsQuality = item.item.quality;
								pantsState = item.item.state;
								break;
							case EItemType.HAT:
								if (hat != 0)
								{
									player.inventory.forceAddItem(new Item(hat, 1, hatQuality, hatState), false);
								}
								clothes.hat = item.item.id;
								hatQuality = item.item.quality;
								hatState = item.item.state;
								break;
							case EItemType.BACKPACK:
								if (backpack != 0)
								{
									player.inventory.forceAddItem(new Item(backpack, 1, backpackQuality, backpackState), false);
								}
								clothes.backpack = item.item.id;
								backpackQuality = item.item.quality;
								backpackState = item.item.state;
								break;
							case EItemType.VEST:
								if (vest != 0)
								{
									player.inventory.forceAddItem(new Item(vest, 1, vestQuality, vestState), false);
								}
								clothes.vest = item.item.id;
								vestQuality = item.item.quality;
								vestState = item.item.state;
								break;
							case EItemType.MASK:
								if (mask != 0)
								{
									player.inventory.forceAddItem(new Item(mask, 1, maskQuality, maskState), false);
								}
								clothes.mask = item.item.id;
								maskQuality = item.item.quality;
								maskState = item.item.state;
								break;
							case EItemType.GLASSES:
								if (glasses != 0)
								{
									player.inventory.forceAddItem(new Item(glasses, 1, glassesQuality, glassesState), false);
								}
								clothes.glasses = item.item.id;
								glassesQuality = item.item.quality;
								glassesState = item.item.state;
								break;
							default:
								return;
						}

						player.equipment.use();
						break;
					case EMannequinUpdateMode.REMOVE:
						clearVisuals();

						if (shirt != 0)
						{
							player.inventory.forceAddItem(new Item(shirt, 1, shirtQuality, shirtState), true, false);
						}

						if (pants != 0)
						{
							player.inventory.forceAddItem(new Item(pants, 1, pantsQuality, pantsState), true, false);
						}

						if (hat != 0)
						{
							player.inventory.forceAddItem(new Item(hat, 1, hatQuality, hatState), true, false);
						}

						if (backpack != 0)
						{
							player.inventory.forceAddItem(new Item(backpack, 1, backpackQuality, backpackState), true, false);
						}

						if (vest != 0)
						{
							player.inventory.forceAddItem(new Item(vest, 1, vestQuality, vestState), true, false);
						}

						if (mask != 0)
						{
							player.inventory.forceAddItem(new Item(mask, 1, maskQuality, maskState), true, false);
						}

						if (glasses != 0)
						{
							player.inventory.forceAddItem(new Item(glasses, 1, glassesQuality, glassesState), true, false);
						}

						clearClothes();
						break;
					case EMannequinUpdateMode.SWAP:
						clearVisuals();

						ushort newShirt = player.clothing.shirt;
						byte newShirtQuality = player.clothing.shirtQuality;
						byte[] newShirtState = player.clothing.shirtState;
						ushort newPants = player.clothing.pants;
						byte newPantsQuality = player.clothing.pantsQuality;
						byte[] newPantsState = player.clothing.pantsState;
						ushort newHat = player.clothing.hat;
						byte newHatQuality = player.clothing.hatQuality;
						byte[] newHatState = player.clothing.hatState;
						ushort newBackpack = player.clothing.backpack;
						byte newBackpackQuality = player.clothing.backpackQuality;
						byte[] newBackpackState = player.clothing.backpackState;
						ushort newVest = player.clothing.vest;
						byte newVestQuality = player.clothing.vestQuality;
						byte[] newVestState = player.clothing.vestState;
						ushort newMask = player.clothing.mask;
						byte newMaskQuality = player.clothing.maskQuality;
						byte[] newMaskState = player.clothing.maskState;
						ushort newGlasses = player.clothing.glasses;
						byte newGlassesQuality = player.clothing.glassesQuality;
						byte[] newGlassesState = player.clothing.glassesState;

						player.clothing.updateClothes(shirt, shirtQuality, shirtState, pants, pantsQuality, pantsState, hat, hatQuality, hatState, backpack, backpackQuality, backpackState, vest, vestQuality, vestState, mask, maskQuality, maskState, glasses, glassesQuality, glassesState);
						updateClothes(newShirt, newShirtQuality, newShirtState, newPants, newPantsQuality, newPantsState, newHat, newHatQuality, newHatState, newBackpack, newBackpackQuality, newBackpackState, newVest, newVestQuality, newVestState, newMask, newMaskQuality, newMaskState, newGlasses, newGlassesQuality, newGlassesState);
						break;
					default:
						return;
				}

				rebuildState();

				byte[] newState = region.FindBarricadeByRootFast(transform).serversideData.barricade.state; // todo
				SendUpdate.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), newState);

				EffectAsset sleeveEffect = SleeveRef.Find();
				if (sleeveEffect != null)
				{
					TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(sleeveEffect);
					triggerEffectParameters.position = transform.position;
					triggerEffectParameters.relevantDistance = EffectManager.SMALL;
					EffectManager.triggerEffect(triggerEffectParameters);
				}
			}
		}

		private static readonly AssetReference<EffectAsset> SleeveRef = new AssetReference<EffectAsset>("704906b407fe4cb9b4a193ab7447d784"); // Clothing sound (9)
	}
}
