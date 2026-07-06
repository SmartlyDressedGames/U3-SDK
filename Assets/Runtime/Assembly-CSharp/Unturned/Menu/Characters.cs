////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void CharacterUpdated(byte index, Character character);

	public class Characters : MonoBehaviour
	{
		public const byte SAVEDATA_VERSION_INITIAL = 21;
		public const byte SAVEDATA_VERSION_ADDED_BEARD_COLOR = 22;
		private const byte SAVEDATA_VERSION_NEWEST = SAVEDATA_VERSION_ADDED_BEARD_COLOR;
		public static readonly byte SAVEDATA_VERSION = SAVEDATA_VERSION_NEWEST;

		private static bool hasLoaded;
		private static bool initialApply;
		public static bool hasPlayed;
		private static bool hasDropped;

		public static CharacterUpdated onCharacterUpdated;

		private static byte _selected;
		public static byte selected
		{
			get => _selected;

			set
			{
				_selected = value;

				onCharacterUpdated?.Invoke(selected, active);

				RefreshPreviewCharacterModel();
			}
		}

		public static Character active => list[selected];

		public static Character[] list
		{
			get;
			private set;
		}

		private static Transform character;
		//private static CharacterAnimator animator;
		public static HumanClothes clothes;
		private static Transform[] slots;
		private static Transform primaryMeleeSlot;
		private static Transform primaryLargeGunSlot;
		private static Transform primarySmallGunSlot;
		private static Transform secondaryMeleeSlot;
		private static Transform secondaryGunSlot;

		private static List<ulong> _packageSkins;
		public static List<ulong> packageSkins => _packageSkins;

		/// <summary>
		/// If set, this item is prioritized over equipped cosmetics. Used by item inspect menu.
		/// Admittedly, this is very hacked-together. Hopefully rewriting this file someday?
		/// </summary>
		internal static int previewItemDefId;
		internal static bool previewItemSolo;

		private static bool wasRefreshCharacterRequested;

		public static void rename(string name)
		{
			active.name = name;

			onCharacterUpdated?.Invoke(selected, active);
		}

		public static void skillify(EPlayerSkillset skillset)
		{
			active.skillset = skillset;

			onCharacterUpdated?.Invoke(selected, active);

			active.applyHero();
			RefreshPreviewCharacterModel();
		}

		public static void growFace(byte face)
		{
			active.face = face;
			RefreshPreviewCharacterModel();
		}

		public static void growHair(byte hair)
		{
			active.hair = hair;
			RefreshPreviewCharacterModel();
		}

		public static void growBeard(byte beard)
		{
			active.beard = beard;
			RefreshPreviewCharacterModel();
		}

		public static void paintSkin(Color color)
		{
			active.skin = color;
			RefreshPreviewCharacterModel();
		}

		public static void paintColor(Color color)
		{
			active.color = color;
			RefreshPreviewCharacterModel();
		}

		public static void renick(string nick)
		{
			active.nick = nick;

			onCharacterUpdated?.Invoke(selected, active);
		}

		public static void paintMarkerColor(Color color)
		{
			active.markerColor = color;
		}

		public static void ChangeBeardColor(Color color)
		{
			active.BeardColor = color;
			RefreshPreviewCharacterModel();
		}

		public static void group(CSteamID group)
		{
			if (active.group == group)
			{
				active.group = CSteamID.Nil;
			}
			else
			{
				active.group = group;
			}

			onCharacterUpdated?.Invoke(selected, active);
		}

		public static void ungroup()
		{
			active.group = CSteamID.Nil;

			onCharacterUpdated?.Invoke(selected, active);
		}

		public static void hand(bool state)
		{
			active.hand = state;
			RefreshPreviewCharacterModel();

			onCharacterUpdated?.Invoke(selected, active);
		}

		public static bool isSkinEquipped(ulong instance)
		{
			if (instance == 0)
			{
				return false;
			}

			return packageSkins.IndexOf(instance) != -1;
		}

		public static bool isCosmeticEquipped(ulong instance)
		{
			if (instance == 0)
			{
				return false;
			}

			return active.packageBackpack == instance || active.packageGlasses == instance || active.packageHat == instance || active.packageMask == instance || active.packagePants == instance || active.packageShirt == instance || active.packageVest == instance;
		}

		/// <summary>
		/// Is cosmetic or skin equipped?
		/// </summary>
		public static bool isEquipped(ulong instanceID)
		{
			return isSkinEquipped(instanceID) || isCosmeticEquipped(instanceID);
		}

		public static void ToggleEquipItemByInstanceId(ulong itemInstanceId)
		{
			int itemDefId = Provider.provider.economyService.getInventoryItem(itemInstanceId);

			if (itemDefId == 0)
			{
				return;
			}

			System.Guid itemGuid;
			System.Guid vehicleGuid;
			Provider.provider.economyService.getInventoryTargetID(itemDefId, out itemGuid, out vehicleGuid);

			if (itemGuid == default && vehicleGuid == default)
			{
				return;
			}

			ItemAsset itemAsset = Assets.find<ItemAsset>(itemGuid);

			if (itemAsset == null || itemAsset.proPath == null || itemAsset.proPath.Length == 0)
			{
				ushort skinID = Provider.provider.economyService.getInventorySkinID(itemDefId);

				if (skinID == 0)
				{
					return;
				}

				if (!packageSkins.Remove(itemInstanceId))
				{
					for (int index = 0; index < packageSkins.Count; index++)
					{
						ulong otherPackage = packageSkins[index];
						if (otherPackage == 0)
						{
							continue;
						}

						int otherItem = Provider.provider.economyService.getInventoryItem(otherPackage);
						if (otherItem == 0)
						{
							continue;
						}

						System.Guid otherItemGuid;
						System.Guid otherVehicleGuid;
						Provider.provider.economyService.getInventoryTargetID(otherItem, out otherItemGuid, out otherVehicleGuid);
						if ((itemGuid != default && itemGuid == otherItemGuid) || (vehicleGuid != default && vehicleGuid == otherVehicleGuid))
						{
							packageSkins.RemoveAt(index); // found skin competing for this slot
							break;
						}
					}

					packageSkins.Add(itemInstanceId);
				}
			}

			if (itemAsset != null)
			{
				if (itemAsset.type == EItemType.SHIRT)
				{
					if (active.packageShirt == itemInstanceId)
					{
						active.packageShirt = 0;
					}
					else
					{
						active.packageShirt = itemInstanceId;
					}
				}
				else if (itemAsset.type == EItemType.PANTS)
				{
					if (active.packagePants == itemInstanceId)
					{
						active.packagePants = 0;
					}
					else
					{
						active.packagePants = itemInstanceId;
					}
				}
				else if (itemAsset.type == EItemType.HAT)
				{
					if (active.packageHat == itemInstanceId)
					{
						active.packageHat = 0;
					}
					else
					{
						active.packageHat = itemInstanceId;
					}
				}
				else if (itemAsset.type == EItemType.BACKPACK)
				{
					if (active.packageBackpack == itemInstanceId)
					{
						active.packageBackpack = 0;
					}
					else
					{
						active.packageBackpack = itemInstanceId;
					}
				}
				else if (itemAsset.type == EItemType.VEST)
				{
					if (active.packageVest == itemInstanceId)
					{
						active.packageVest = 0;
					}
					else
					{
						active.packageVest = itemInstanceId;
					}
				}
				else if (itemAsset.type == EItemType.MASK)
				{
					if (active.packageMask == itemInstanceId)
					{
						active.packageMask = 0;
					}
					else
					{
						active.packageMask = itemInstanceId;
					}
				}
				else if (itemAsset.type == EItemType.GLASSES)
				{
					if (active.packageGlasses == itemInstanceId)
					{
						active.packageGlasses = 0;
					}
					else
					{
						active.packageGlasses = itemInstanceId;
					}
				}
			}

			RefreshPreviewCharacterModel();

			onCharacterUpdated?.Invoke(selected, active);
		}

		private static float characterOffset;
		private static float _characterYaw;
		public static float characterYaw;

		public static bool getPackageForItemID(ushort itemID, out ulong itemInstanceId)
		{
			itemInstanceId = 0;

			ItemAsset targetItemAsset = Assets.find(EAssetType.ITEM, itemID) as ItemAsset;
			if (targetItemAsset == null)
			{
				return false;
			}

			for (int index = 0; index < packageSkins.Count; index++)
			{
				itemInstanceId = packageSkins[index];
				if (itemInstanceId == 0)
				{
					continue;
				}

				int itemDefId = Provider.provider.economyService.getInventoryItem(itemInstanceId);
				if (itemDefId == 0)
				{
					continue;
				}

				System.Guid otherGuid = Provider.provider.economyService.getInventoryItemGuid(itemDefId);
				if (otherGuid == default)
				{
					continue;
				}

				if (targetItemAsset.GUID == otherGuid)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Given the itemdefid of an equipped skin, try to find its corresponding Steam unique instance ID.
		/// (A.K.A. "Package" in older code?)
		/// </summary>
		public static bool TryGetEquippedSkinInstanceIdByItemDefId(int itemDefId, out ulong itemInstanceId)
		{
			foreach (ulong equippedInstanceId in packageSkins)
			{
				if (Provider.provider.economyService.getInventoryItem(equippedInstanceId) == itemDefId)
				{
					itemInstanceId = equippedInstanceId;
					return true;
				}
			}

			itemInstanceId = 0;
			return false;
		}

		private static bool getSlot0StatTrackerValue(out EStatTrackerType type, out int kills)
		{
			type = EStatTrackerType.NONE;
			kills = -1;

			ulong package;
			if (!getPackageForItemID(active.primaryItem, out package))
			{
				return false;
			}

			return Provider.provider.economyService.getInventoryStatTrackerValue(package, out type, out kills);
		}

		private static bool getSlot1StatTrackerValue(out EStatTrackerType type, out int kills)
		{
			type = EStatTrackerType.NONE;
			kills = -1;

			ulong package;
			if (!getPackageForItemID(active.secondaryItem, out package))
			{
				return false;
			}

			return Provider.provider.economyService.getInventoryStatTrackerValue(package, out type, out kills);
		}

		private static void apply(byte slot, bool showItems)
		{
			if (slots[slot] != null)
			{
				Destroy(slots[slot].gameObject);
			}

			if (!showItems)
			{
				return;
			}

			ushort itemID = 0;
			byte[] state = null;

			if (slot == 0)
			{
				itemID = active.primaryItem;
				state = active.primaryState;
			}
			else if (slot == 1)
			{
				itemID = active.secondaryItem;
				state = active.secondaryState;
			}

			if (itemID == 0)
			{
				return;
			}

			ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemID) as ItemAsset;

			if (itemAsset != null)
			{
				ushort skinID = 0;
				ushort mythicID = 0;
				bool isSharedSkin = itemAsset.sharedSkinLookupID != itemID;
				System.Guid skinLookupId = itemAsset.GUID;
				if (isSharedSkin)
				{
					Asset sharedAsset = Assets.find(EAssetType.ITEM, itemAsset.sharedSkinLookupID);
					if (sharedAsset != null)
					{
						skinLookupId = sharedAsset.GUID;
					}
				}

				for (int index = 0; index < packageSkins.Count; index++)
				{
					ulong package = packageSkins[index];
					if (package == 0)
					{
						continue;
					}

					int itemDefId = Provider.provider.economyService.getInventoryItem(package);
					if (itemDefId == 0)
					{
						continue;
					}

					System.Guid otherGuid = Provider.provider.economyService.getInventoryItemGuid(itemDefId);
					if (otherGuid == default)
					{
						continue;
					}

					if (skinLookupId == otherGuid)
					{
						skinID = Provider.provider.economyService.getInventorySkinID(itemDefId);
						mythicID = Provider.provider.economyService.getInventoryMythicID(itemDefId);
						if (mythicID == 0)
						{
							mythicID = Provider.provider.economyService.getInventoryParticleEffect(package);
						}
						break;
					}
				}

				GetStatTrackerValueHandler statTrackerCallback = null;
				if (slot == 0)
				{
					statTrackerCallback = getSlot0StatTrackerValue;
				}
				else if (slot == 1)
				{
					statTrackerCallback = getSlot1StatTrackerValue;
				}

				Transform model = ItemTool.getItem(itemID, skinID, 100, state, /*viewmodel*/ false, itemAsset, statTrackerCallback);

				if (slot == 0)
				{
					if (itemAsset.type == EItemType.MELEE)
					{
						model.transform.parent = primaryMeleeSlot;
					}
					else
					{
						if (itemAsset.slot == ESlotType.PRIMARY)
						{
							model.transform.parent = primaryLargeGunSlot;
						}
						else
						{
							model.transform.parent = primarySmallGunSlot;
						}
					}
				}
				else if (slot == 1)
				{
					if (itemAsset.type == EItemType.MELEE)
					{
						model.transform.parent = secondaryMeleeSlot;
					}
					else
					{
						model.transform.parent = secondaryGunSlot;
					}
				}

				model.localPosition = Vector3.zero;
				model.localRotation = Quaternion.Euler(0, 0, 90);
				model.localScale = Vector3.one;
				Destroy(model.GetComponent<Collider>());

				if (mythicID != 0)
				{
					ItemTool.ApplyMythicalEffect(model, mythicID, EEffectType.THIRD);
				}

				slots[slot] = model;
			}
		}

		public static void RefreshPreviewCharacterModel()
		{
			if (active == null)
			{
				UnturnedLog.error("Failed to find an active character.");
				return;
			}

			if (clothes == null)
			{
				UnturnedLog.error("Failed to find character clothes.");
				return;
			}

			wasRefreshCharacterRequested = true;
		}

		private static void applyInternal()
		{
			// Nelson 2024-09-24: Believe it or not, this mess is now a *little* bit better than it was before. :|
			bool isEditingAppearance = MenuSurvivorsAppearanceUI.active;
			bool isEditingEquippedCosmetics = MenuSurvivorsClothingUI.active || MenuSurvivorsClothingBoxUI.active
				|| MenuSurvivorsClothingDeleteUI.active || MenuSurvivorsClothingInspectUI.active
				|| MenuSurvivorsClothingItemUI.active;
			if (ItemStoreMenu.instance?.IsOpen ?? false)
			{
				isEditingEquippedCosmetics = true;
			}
			if (ItemStoreCartMenu.instance?.IsOpen ?? false)
			{
				isEditingEquippedCosmetics = true;
			}
			if (ItemStoreDetailsMenu.instance?.IsOpen ?? false)
			{
				isEditingEquippedCosmetics = true;
			}
			if (ItemStoreBundleContentsMenu.instance?.IsOpen ?? false)
			{
				isEditingEquippedCosmetics = true;
			}

			bool showItems = !isEditingAppearance && !isEditingEquippedCosmetics && !previewItemSolo;
			bool showCosmetics = !isEditingAppearance && !previewItemSolo;

			character.localScale = new Vector3(active.hand ? -1 : 1, 1, 1);

			if (showItems)
			{
				clothes.shirt = active.shirt;
				clothes.pants = active.pants;
				clothes.hat = active.hat;
				clothes.backpack = active.backpack;
				clothes.vest = active.vest;
				clothes.mask = active.mask;
				clothes.glasses = active.glasses;
			}
			else
			{
				clothes.shirt = 0;
				clothes.pants = 0;
				clothes.hat = 0;
				clothes.backpack = 0;
				clothes.vest = 0;
				clothes.mask = 0;
				clothes.glasses = 0;
			}

			if (showCosmetics)
			{
				if (active.packageShirt != 0)
				{
					clothes.visualShirt = Provider.provider.economyService.getInventoryItem(active.packageShirt);
				}
				else
				{
					clothes.visualShirt = 0;
				}

				if (active.packagePants != 0)
				{
					clothes.visualPants = Provider.provider.economyService.getInventoryItem(active.packagePants);
				}
				else
				{
					clothes.visualPants = 0;
				}

				if (active.packageHat != 0)
				{
					clothes.visualHat = Provider.provider.economyService.getInventoryItem(active.packageHat);
				}
				else
				{
					clothes.visualHat = 0;
				}

				if (active.packageBackpack != 0)
				{
					clothes.visualBackpack = Provider.provider.economyService.getInventoryItem(active.packageBackpack);
				}
				else
				{
					clothes.visualBackpack = 0;
				}

				if (active.packageVest != 0)
				{
					clothes.visualVest = Provider.provider.economyService.getInventoryItem(active.packageVest);
				}
				else
				{
					clothes.visualVest = 0;
				}

				if (active.packageMask != 0)
				{
					clothes.visualMask = Provider.provider.economyService.getInventoryItem(active.packageMask);
				}
				else
				{
					clothes.visualMask = 0;
				}

				if (active.packageGlasses != 0)
				{
					clothes.visualGlasses = Provider.provider.economyService.getInventoryItem(active.packageGlasses);
				}
				else
				{
					clothes.visualGlasses = 0;
				}
			}
			else
			{
				clothes.visualShirt = 0;
				clothes.visualPants = 0;
				clothes.visualHat = 0;
				clothes.visualBackpack = 0;
				clothes.visualVest = 0;
				clothes.visualMask = 0;
				clothes.visualGlasses = 0;
			}

			if (previewItemDefId > 0)
			{
				ItemAsset gameAsset = Assets.find<ItemAsset>(Provider.provider.economyService.getInventoryItemGuid(previewItemDefId));
				if (gameAsset != null)
				{
					switch (gameAsset.type)
					{
						case EItemType.HAT:
							clothes.visualHat = previewItemDefId;
							break;

						case EItemType.PANTS:
							clothes.visualPants = previewItemDefId;
							break;

						case EItemType.SHIRT:
							clothes.visualShirt = previewItemDefId;
							break;

						case EItemType.MASK:
							clothes.visualMask = previewItemDefId;
							break;

						case EItemType.BACKPACK:
							clothes.visualBackpack = previewItemDefId;
							break;

						case EItemType.VEST:
							clothes.visualVest = previewItemDefId;
							break;

						case EItemType.GLASSES:
							clothes.visualGlasses = previewItemDefId;
							break;
					}
				}
			}

			clothes.face = active.face;
			clothes.hair = active.hair;
			clothes.beard = active.beard;
			clothes.skin = active.skin;
			clothes.color = active.color;
			clothes.BeardColor = active.BeardColor;
			clothes.hand = active.hand;

			clothes.apply();

			for (byte slot = 0; slot < slots.Length; slot++)
			{
				apply(slot, showItems);
			}
		}

		private static void onInventoryRefreshed()
		{
			if (clothes != null && list != null && packageSkins != null)
			{
				for (int index = packageSkins.Count - 1; index >= 0; index--)
				{
					ulong package = packageSkins[index];

					if (package != 0 && Provider.provider.economyService.getInventoryItem(package) == 0)
					{
						packageSkins.RemoveAt(index);
					}
				}

				for (int index = 0; index < list.Length; index++)
				{
					Character character = list[index];

					if (character == null)
					{
						continue;
					}

					if (character.packageShirt != 0 && Provider.provider.economyService.getInventoryItem(character.packageShirt) == 0)
					{
						character.packageShirt = 0;
					}

					if (character.packagePants != 0 && Provider.provider.economyService.getInventoryItem(character.packagePants) == 0)
					{
						character.packagePants = 0;
					}

					if (character.packageHat != 0 && Provider.provider.economyService.getInventoryItem(character.packageHat) == 0)
					{
						character.packageHat = 0;
					}

					if (character.packageBackpack != 0 && Provider.provider.economyService.getInventoryItem(character.packageBackpack) == 0)
					{
						character.packageBackpack = 0;
					}

					if (character.packageVest != 0 && Provider.provider.economyService.getInventoryItem(character.packageVest) == 0)
					{
						character.packageVest = 0;
					}

					if (character.packageMask != 0 && Provider.provider.economyService.getInventoryItem(character.packageMask) == 0)
					{
						character.packageMask = 0;
					}

					if (character.packageGlasses != 0 && Provider.provider.economyService.getInventoryItem(character.packageGlasses) == 0)
					{
						character.packageGlasses = 0;
					}
				}

				if (!initialApply)
				{
					initialApply = true;
					RefreshPreviewCharacterModel();
				}
			}

			if (hasDropped)
			{
				return;
			}

			hasDropped = true;

			if (hasPlayed)
			{
#if !DEV
				Provider.provider.economyService.dropInventory();
#endif

#if !DEDICATED_SERVER
				LiveConfig.Refresh();
#endif // !DEDICATED_SERVER
			}
		}

		private void Update()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (character == null)
			{
				return;
			}

			_characterYaw = Mathf.Lerp(_characterYaw, characterOffset + characterYaw, 4 * Time.deltaTime);
			character.transform.rotation = Quaternion.Euler(90, _characterYaw, 0);

			if (wasRefreshCharacterRequested)
			{
				wasRefreshCharacterRequested = false;
				try
				{
					applyInternal();
				}
				catch (System.Exception e)
				{
					UnturnedLog.exception(e);
				}
			}
		}

		private void ResetCharacterYaw(MenuOverridableObjects source)
		{
			characterOffset = character.transform.eulerAngles.y;
			_characterYaw = characterOffset;
			characterYaw = 0;
		}

		private void OnDestroy()
		{
			MenuOverridableObjects.OnMenuOverridesApplied -= ResetCharacterYaw;
		}

		internal void customStart()
		{
			character = GameObject.Find("Hero").transform;

			clothes = character.GetComponent<HumanClothes>();
			clothes.isView = true;
			clothes.ShouldHairOverridesUseFallbackColor = !Provider.isPro;
			slots = new Transform[PlayerInventory.SLOTS];
			primaryMeleeSlot = character.Find("Skeleton").Find("Spine").Find("Primary_Melee");
			primaryLargeGunSlot = character.Find("Skeleton").Find("Spine").Find("Primary_Large_Gun");
			primarySmallGunSlot = character.Find("Skeleton").Find("Spine").Find("Primary_Small_Gun");
			secondaryMeleeSlot = character.Find("Skeleton").Find("Right_Hip").Find("Right_Leg").Find("Secondary_Melee");
			secondaryGunSlot = character.Find("Skeleton").Find("Right_Hip").Find("Right_Leg").Find("Secondary_Gun");

			MenuOverridableObjects.OnMenuOverridesApplied += ResetCharacterYaw;
			ResetCharacterYaw(null);

			previewItemDefId = 0;
			previewItemSolo = false;

			hasDropped = false;

			if (!hasLoaded)
			{
				Provider.provider.economyService.onInventoryRefreshed += onInventoryRefreshed;
			}

			load();
		}

		public static void load()
		{
			initialApply = false;
			Provider.provider.economyService.refreshInventory();

			if (list != null)
			{
				for (byte index = 0; index < list.Length; index++)
				{
					if (list[index] == null)
					{
						continue;
					}

					onCharacterUpdated?.Invoke(index, list[index]);
				}

				return;
			}

			list = new Character[Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS];
			_packageSkins = new List<ulong>();

			if (ReadWrite.fileExists("/Characters.dat", true))
			{
				Block block = ReadWrite.readBlock("/Characters.dat", true, 0);

				if (block != null)
				{
					byte version = block.readByte();

					if (version >= 12)
					{
						if (version >= 14)
						{
							ushort count = block.readUInt16();
							for (ushort index = 0; index < count; index++)
							{
								ulong package = block.readUInt64();

								if (package != 0)
								{
									packageSkins.Add(package);
								}
							}
						}

						_selected = block.readByte();

						if (_selected >= list.Length || (!Provider.isPro && selected >= Customization.FREE_CHARACTERS))
						{
							_selected = 0;
						}

						for (byte index = 0; index < list.Length; index++)
						{
							ushort shirt = block.readUInt16();
							ushort pants = block.readUInt16();
							ushort hat = block.readUInt16();
							ushort backpack = block.readUInt16();
							ushort vest = block.readUInt16();
							ushort mask = block.readUInt16();
							ushort glasses = block.readUInt16();

							ulong packageShirt = block.readUInt64();
							ulong packagePants = block.readUInt64();
							ulong packageHat = block.readUInt64();
							ulong packageBackpack = block.readUInt64();
							ulong packageVest = block.readUInt64();
							ulong packageMask = block.readUInt64();
							ulong packageGlasses = block.readUInt64();

							ushort primaryItem = block.readUInt16();
							byte[] primaryState = block.readByteArray();

							ushort secondaryItem = block.readUInt16();
							byte[] secondaryState = block.readByteArray();

							byte face = block.readByte();
							byte hair = block.readByte();
							byte beard = block.readByte();

							Color skin = block.readColor();
							Color color = block.readColor();

							Color markerColor;
							if (version > 20)
							{
								markerColor = block.readColor();
							}
							else
							{
								markerColor = Customization.MARKER_COLORS[Random.Range(0, Customization.MARKER_COLORS.Length)];
							}

							Color beardColor;
							if (version >= SAVEDATA_VERSION_ADDED_BEARD_COLOR)
							{
								beardColor = block.readColor();
							}
							else
							{
								beardColor = color;
							}

							bool hand = block.readBoolean();

							string name = block.readString();
							if (version < 19)
							{
								name = Provider.clientName;// "New Survivor #" + Random.Range(1, 100001);
							}

							string nick = block.readString();
							CSteamID group = block.readSteamID();
							byte skillset = block.readByte();

							if (!Provider.provider.communityService.checkGroup(group))
							{
								group = CSteamID.Nil;
							}

							if (skillset >= Customization.SKILLSETS)
							{
								skillset = 0;
							}

							if (version < 16)
							{
								skillset = (byte) Random.Range(1, Customization.SKILLSETS);
							}

							if (version > 16 && version < 20)
							{
								block.readBoolean();
							}

							if (!Provider.isPro)
							{
								if (index >= Customization.FREE_CHARACTERS)
								{
									// Reset name because a player reported that they had a name they did not want listed,
									// but could not change it because they do not have gold.
									name = Provider.clientName;
									nick = Provider.clientName;
								}

								if (face >= Customization.FACES_FREE)
								{
									face = (byte) Random.Range(0, Customization.FACES_FREE);
								}

								if (hair >= Customization.HAIRS_FREE)
								{
									hair = (byte) Random.Range(0, Customization.HAIRS_FREE);
								}

								if (beard >= Customization.BEARDS_FREE)
								{
									beard = 0;
								}

								if (!Customization.checkSkin(skin))
								{
									skin = Customization.SKINS[Random.Range(0, Customization.SKINS.Length)];
								}

								if (!Customization.checkColor(color))
								{
									color = Customization.COLORS[Random.Range(0, Customization.COLORS.Length)];
								}

								if (!Customization.checkColor(beardColor))
								{
									beardColor = color;
								}
							}

							list[index] = new Character(shirt, pants, hat, backpack, vest, mask, glasses, packageShirt, packagePants, packageHat, packageBackpack, packageVest, packageMask, packageGlasses, primaryItem, primaryState, secondaryItem, secondaryState, face, hair, beard, skin, color, markerColor, beardColor, hand, name, nick, group, (EPlayerSkillset) skillset);

							onCharacterUpdated?.Invoke(index, list[index]);
						}
					}
					else
					{
						for (byte index = 0; index < list.Length; index++)
						{
							list[index] = new Character();

							onCharacterUpdated?.Invoke(index, list[index]);
						}
					}
				}
			}
			else
			{
				_selected = 0;
			}

			for (byte index = 0; index < list.Length; index++)
			{
				if (list[index] == null)
				{
					list[index] = new Character();

					onCharacterUpdated?.Invoke(index, list[index]);
				}
			}

			RefreshPreviewCharacterModel();

			hasLoaded = true;
			UnturnedLog.info("Loaded characters");
		}

		public static void save()
		{
			if (!hasLoaded)
			{
				return;
			}

			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION_NEWEST);

			block.writeUInt16((ushort) packageSkins.Count);
			for (ushort index = 0; index < packageSkins.Count; index++)
			{
				ulong package = packageSkins[index];

				block.writeUInt64(package);
			}

			block.writeByte(selected);
			for (byte index = 0; index < list.Length; index++)
			{
				Character character = list[index];

				if (character == null)
				{
					character = new Character();
				}

				block.writeUInt16(character.shirt);
				block.writeUInt16(character.pants);
				block.writeUInt16(character.hat);
				block.writeUInt16(character.backpack);
				block.writeUInt16(character.vest);
				block.writeUInt16(character.mask);
				block.writeUInt16(character.glasses);

				block.writeUInt64(character.packageShirt);
				block.writeUInt64(character.packagePants);
				block.writeUInt64(character.packageHat);
				block.writeUInt64(character.packageBackpack);
				block.writeUInt64(character.packageVest);
				block.writeUInt64(character.packageMask);
				block.writeUInt64(character.packageGlasses);

				block.writeUInt16(character.primaryItem);
				block.writeByteArray(character.primaryState);

				block.writeUInt16(character.secondaryItem);
				block.writeByteArray(character.secondaryState);

				block.writeByte(character.face);
				block.writeByte(character.hair);
				block.writeByte(character.beard);

				block.writeColor(character.skin);
				block.writeColor(character.color);
				block.writeColor(character.markerColor);
				block.writeColor(character.BeardColor);

				block.writeBoolean(character.hand);

				block.writeString(character.name);
				block.writeString(character.nick);
				block.writeSteamID(character.group);
				block.writeByte((byte) character.skillset);
			}

			ReadWrite.writeBlock("/Characters.dat", true, block);
		}
	}
}
