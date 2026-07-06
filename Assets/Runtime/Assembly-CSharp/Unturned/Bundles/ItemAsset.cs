////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public interface ISkinableAsset
	{
		Texture albedoBase
		{
			get;
		}

		Texture metallicBase
		{
			get;
		}

		Texture emissionBase
		{
			get;
		}
	}

	public struct ItemDescriptionLine : System.IComparable<ItemDescriptionLine>
	{
		public string text;
		public int sortOrder;

		public int CompareTo(ItemDescriptionLine other)
		{
			if (sortOrder == other.sortOrder)
			{
				return text.CompareTo(other.text);
			}
			else
			{
				return sortOrder.CompareTo(other.sortOrder);
			}
		}
	}

	internal class ItemDescriptionBuilderUtils
	{
		public static System.Text.StringBuilder descriptionStringBuilder = new System.Text.StringBuilder(512);
		public static List<ItemDescriptionLine> lines = new List<ItemDescriptionLine>();

		public static string FormatLines()
		{
			lines.Sort();

			int previousSortOrder = 0;
			if (lines.Count > 0)
			{
				previousSortOrder = lines[0].sortOrder;
			}

			descriptionStringBuilder.Clear();
			foreach (ItemDescriptionLine line in lines)
			{
				if (line.sortOrder - previousSortOrder > 100)
				{
					descriptionStringBuilder.AppendLine();
				}
				descriptionStringBuilder.AppendLine(line.text);
				previousSortOrder = line.sortOrder;
			}
			return descriptionStringBuilder.ToString();
		}

		public static ItemDescriptionBuilder CreateForUI(ItemAsset itemAsset)
		{
			ItemDescriptionBuilder descriptionBuilder = new ItemDescriptionBuilder();

			descriptionStringBuilder.Clear();
			descriptionBuilder.stringBuilder = descriptionStringBuilder;

			if (!Glazier.Get().SupportsAutomaticLayout)
			{
				descriptionBuilder.flags = EItemDescriptionFlags.LegacyContent;
			}
			else
			{
				descriptionBuilder.flags = itemAsset.PreferredDescriptionFlags;
			}
			lines.Clear();

			descriptionBuilder.lines = lines;

			return descriptionBuilder;
		}
	}

	public struct ItemDescriptionBuilder
	{
		public EItemDescriptionFlags flags;
		public List<ItemDescriptionLine> lines;

		public bool HasFlag(EItemDescriptionFlags flag)
		{
			return flags.HasFlag(flag);
		}

		/// <summary>
		/// BuildDescription implementations can use this to concatenate longer strings.
		/// </summary>
		public System.Text.StringBuilder stringBuilder;

		public void Append(string text, int sortOrder)
		{
			lines.Add(new ItemDescriptionLine() { text = text, sortOrder = sortOrder });
		}
	}

	/// <summary>
	/// Determines which info is automatically added to the item description.
	/// </summary>
	[System.Flags]
	public enum EItemDescriptionFlags
	{
		/// <summary>
		/// Do not add any of the newer info to the description.
		/// Equivalent to Use_Auto_Stat_Descriptions false.
		/// Also applicable when using IMGUI.
		/// </summary>
		LegacyContent = 0,

		/// <summary>
		/// Include names of gun's attachments in the description.
		/// </summary>
		GunAttachments = 1 << 0,

		/// <summary>
		/// Include any other info without its own flag.
		/// 
		/// This only exists because description flags are retrofitted over an all-or-nothing
		/// option (Use_Auto_Stat_Description).
		/// </summary>
		Uncategorized = 1 << 1,

		/// <summary>
		/// Add as much info to the description as possible.
		/// Equivalent to Use_Auto_Stat_Descriptions true.
		/// </summary>
		All = GunAttachments | Uncategorized,
	}

	/// <summary>
	/// Which parent to use when attaching an equipped/useable item to the player.
	/// </summary>
	public enum EEquipableModelParent
	{
		RightHook,
		LeftHook,
		Spine,
		SpineHook,
	}

	public class ItemAsset : Asset, ISkinableAsset, IBlueprintOwner
	{
		/// <summary>
		/// Helper for plugins that want item prefabs server-side.
		/// e.g. Allows item icons to be captured on dedicated server.
		/// </summary>
		public static CommandLineFlag shouldAlwaysLoadItemPrefab = new CommandLineFlag(false, "-AlwaysLoadItemPrefab");

		protected bool _shouldVerifyHash;
		public bool shouldVerifyHash => _shouldVerifyHash;

		internal override bool ShouldVerifyHash => _shouldVerifyHash;
		public override string FriendlyName
		{
			get
			{
				if (!string.IsNullOrEmpty(_itemName))
				{
					return _itemName;
				}
				else
				{
					return name;
				}
			}
		}

		protected string _itemName;
		public string itemName => _itemName;

		protected string _itemDescription;
		public string itemDescription => _itemDescription;


		public EItemType type;


		public EItemRarity rarity;

		/// <summary>
		/// Item name wrapped in color rich text tags according to rarity.
		/// </summary>
		public string RarityRichTextName => $"<color={Palette.hex(ItemTool.getRarityColorUI(rarity))}>{FriendlyName}</color>";

		protected string _proPath;
		public string proPath => _proPath;

		/// <summary>
		/// Hack for Kuwait aura icons.
		/// </summary>
		public bool econIconUseId;


		public bool isPro;


		public string useable;

		/// <summary>
		/// Useable subclass.
		/// </summary>
		public System.Type useableType
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can this useable be equipped by players?
		/// True for most items, but allows modders to create sentry-only weapons.
		/// </summary>
		public bool canPlayerEquip
		{
			get;
			protected set;
		}

		[System.Obsolete("Renamed to canPlayerEquip")]
		public bool isUseable
		{
			get => canPlayerEquip;
			set => canPlayerEquip = value;
		}


		public ESlotType slot;

		/// <summary>
		/// Can this useable be equipped while underwater?
		/// </summary>
		public bool canUseUnderwater
		{
			get;
			protected set;
		}

		public byte[] getState()
		{
			return getState(false);
		}

		public byte[] getState(bool isFull)
		{
			return getState(isFull ? EItemOrigin.ADMIN : EItemOrigin.WORLD);
		}

		public virtual byte[] getState(EItemOrigin origin)
		{
			return new byte[0];
		}

		public virtual void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			string rarityDesc = PlayerDashboardInventoryUI.localization.format("Rarity_" + (int) rarity);
			string typeDesc = PlayerDashboardInventoryUI.localization.format("Type_" + (int) type);

			builder.Append("<color=" + Palette.hex(ItemTool.getRarityColorUI(rarity)) + ">" + PlayerDashboardInventoryUI.localization.format("Rarity_Type_Label", rarityDesc, typeDesc) + "</color>", DescSort_RarityAndType);

			if (!string.IsNullOrEmpty(_itemDescription))
			{
				builder.Append(_itemDescription, DescSort_LoreText);
			}

			if (showQuality && itemInstance != null)
			{
				Color32 color = ItemTool.getQualityColor(itemInstance.quality / 100.0f);
				builder.Append("<color=" + Palette.hex(color) + ">" + PlayerDashboardInventoryUI.localization.format("Quality", itemInstance.quality) + "</color>", DescSort_QualityAndAmount);
			}

			if (MaxAmount > 1)
			{
				if (itemInstance != null)
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AmountWithCapacity", itemInstance.amount, MaxAmount), DescSort_QualityAndAmount);
				}
				else
				{
					builder.Append(PlayerDashboardInventoryUI.localization.format("Amount", MaxAmount), DescSort_QualityAndAmount);
				}
			}

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (equipableMovementSpeedMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_EquipableMovementSpeedModifier", PlayerDashboardInventoryUI.FormatStatModifier(equipableMovementSpeedMultiplier, true, true)), DescSort_ItemStat + DescSort_HigherIsBeneficial(equipableMovementSpeedMultiplier));
			}
		}

		public override string GetTypeFriendlyName()
		{
			string friendlyName = base.GetTypeFriendlyName();
			if (friendlyName.StartsWith("Item "))
			{
				friendlyName = friendlyName.Substring(5) + " Item";
			}
			return friendlyName;
		}

		public byte size_x;


		public byte size_y;

		/// <summary>
		/// Vertical half size of icon camera.
		/// Values less than zero are disabled.
		/// </summary>
		public float iconCameraOrthographicSize;

		/// <summary>
		/// Vertical half size of economy icon camera.
		/// </summary>
		public float econIconCameraOrthographicSize;

		/// <summary>
		/// Should the newer automatic placement and orthographic size for axis-aligned icon cameras be used?
		/// Enabled by default, but optionally disabled for manual adjustment.
		/// </summary>
		public bool isEligibleForAutomaticIconMeasurements;

		public byte amount;

		/// <summary>
		/// Nelson 2025-04-10: adding this for semantics because amount isn't an obvious name.
		/// </summary>
		public int MaxAmount => amount;
		public byte MaxAmountAsByte => amount;

		/// <summary>
		/// If true, item should be removed when "amount" reaches zero.
		/// Defaults to true except for magazines.
		/// </summary>
		public bool ShouldDeleteAtZeroAmount
		{
			get;
			protected set;
		}

		public byte countMin;

		public byte countMax;

		public byte count
		{
			get
			{
				float fullChance;
				float multiplier;

				if (Provider.modeConfigData != null)
				{
					if (type == EItemType.MAGAZINE)
					{
						fullChance = Provider.modeConfigData.Items.Magazine_Bullets_Full_Chance;
						multiplier = Provider.modeConfigData.Items.Magazine_Bullets_Multiplier;
					}
					else
					{
						fullChance = Provider.modeConfigData.Items.Crate_Bullets_Full_Chance;
						multiplier = Provider.modeConfigData.Items.Crate_Bullets_Multiplier;
					}
				}
				else
				{
					fullChance = 0.9f;
					multiplier = 1.0f;
				}

				if (Random.value < fullChance)
				{
					return MaxAmountAsByte;
				}
				else
				{
					return (byte) Mathf.CeilToInt(Random.Range(countMin, countMax + 1) * multiplier);
				}
			}
		}


		public byte qualityMin;


		public byte qualityMax;

		public byte quality
		{
			get
			{
				if (Random.value < (Provider.modeConfigData != null ? Provider.modeConfigData.Items.Quality_Full_Chance : 0.9f))
				{
					return 100;
				}
				else
				{
					return (byte) Mathf.CeilToInt(Random.Range(qualityMin, qualityMax + 1) * (Provider.modeConfigData != null ? Provider.modeConfigData.Items.Quality_Multiplier : 1.0f));
				}
			}
		}

		/// <summary>
		/// Which parent to use when attaching an equipped/useable item to the player.
		/// </summary>
		public EEquipableModelParent EquipableModelParent
		{
			get;
			protected set;
		}

		/// <summary>
		/// If true, equipable prefab is a child of the left hand rather than the right.
		/// Defaults to false.
		/// </summary>
		[System.Obsolete("Replaced by EquipableModelParent property's LeftHook option.")]
		public bool ShouldAttachEquippedModelToLeftHand
		{
			get => EquipableModelParent == EEquipableModelParent.LeftHook;
		}

		[System.Obsolete("Renamed to ShouldAttachEquippedModelToLeftHand")]
		public bool isBackward;

		/// <summary>
		/// Whether viewmodel should procedurally animate inertia of equipped item.
		/// Useful for low-quality older animations, but modders may wish to disable for high-quality newer animations.
		/// </summary>
		public bool shouldProcedurallyAnimateInertia;

		/// <summary>
		/// Defaults to true. If false, the equipped item model is flipped to counteract the flipped character.
		/// </summary>
		public bool shouldLeftHandedCharactersMirrorEquippedItem;

		public EItemDescriptionFlags PreferredDescriptionFlags
		{
			get;
			set;
		}

		/// <summary>
		/// If true, stats like damage, accuracy, health, etc. are automatically appended to the description.
		/// Defaults to true.
		/// </summary>
		[System.Obsolete("Replaced by DescriptionFlags. Will be removed in a future update.")]
		public bool isEligibleForAutoStatDescriptions;

		protected GameObject _item;
		/// <summary>
		/// Nelson 2024-12-11: This can now be null for cosmetic items (<see cref="isPro"/>). For those items it wasn't
		/// used outside of the main menu 3D item preview, in which case the clothing prefab is typically a better
		/// visualization.
		/// </summary>
		public GameObject item => _item;

		/// <summary>
		/// Optional alternative item prefab specifically for the PlayerEquipment prefab spawned.
		/// </summary>
		public GameObject equipablePrefab;

		/// <summary>
		/// Name to use when instantiating item prefab.
		/// By default the asset legacy id is used, but it can be overridden because some
		/// modders rely on the name for Unity's legacy animation component. Some maps
		/// had a lot of duplicate animations to work around the id naming, in which
		/// case overriding the name simplified animation.
		/// </summary>
		public string instantiatedItemName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Movement speed multiplier while the item is equipped in the hands.
		/// </summary>
		public float equipableMovementSpeedMultiplier = 1.0f;

		protected AudioClip _equip;
		public AudioClip equip => _equip;

		protected AnimationClip[] _animations;
		public AnimationClip[] animations => _animations;

		/// <summary>
		/// Sound to play when inspecting the equipped item.
		/// </summary>
		public AudioReference inspectAudio;

		/// <summary>
		/// Sound to play when moving or rotating the item in the inventory.
		/// </summary>
		public AudioReference inventoryAudio;

#if !DEDICATED_SERVER
		public void PlayInventoryAudio2D()
		{
			if (inventoryAudio.IsNullOrEmpty)
			{
				return;
			}

			OneShotAudioParameters parameters = new OneShotAudioParameters(inventoryAudio);
			parameters.volume *= 0.2f;
			parameters.Play();
		}

#endif // !DEDICATED_SERVER

		protected List<Blueprint> _blueprints;
		public List<Blueprint> blueprints => _blueprints;

		protected List<Action> _actions;
		public List<Action> actions => _actions;

		public bool overrideShowQuality
		{
			get;
			protected set;
		}

		public virtual bool showQuality => overrideShowQuality;

		/// <summary>
		/// When a player dies with this item, should an item drop be spawned?
		/// </summary>
		public bool shouldDropOnDeath
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can player click the drop button on this item?
		/// </summary>
		public bool allowManualDrop
		{
			get;
			protected set;
		}

		protected Texture2D _albedoBase;
		public Texture albedoBase => _albedoBase;

		protected Texture2D _metallicBase;
		public Texture metallicBase => _metallicBase;

		protected Texture2D _emissionBase;
		public Texture emissionBase => _emissionBase;

		public void applySkinBaseTextures(Material material)
		{
			if (sharedSkinLookupID > 0 && sharedSkinLookupID != id)
			{
				ItemAsset baseItem = Assets.find(EAssetType.ITEM, sharedSkinLookupID) as ItemAsset;
				if (baseItem != null)
				{
					baseItem.applySkinBaseTextures(material);
					return;
				}
			}

			material.SetTexture("_AlbedoBase", albedoBase);
			material.SetTexture("_MetallicBase", metallicBase);
			material.SetTexture("_EmissionBase", emissionBase);
		}

		/// <summary>
		/// If this item is compatible with skins for another item, lookup that item's ID instead.
		/// </summary>
		public ushort sharedSkinLookupID
		{
			get;
			protected set;
		}

		/// <summary>
		/// Defaults to true. If false, skin material and mesh are not applied when <see cref="sharedSkinLookupID"/> is
		/// set. For example, a custom axe can transfer the kill counter and ragdoll effect from a vanilla item's skin
		/// without also transferring the material and mesh.
		/// </summary>
		public bool SharedSkinShouldApplyVisuals
		{
			get;
			protected set;
		}

		/// <summary>
		/// Should friendly-mode sentry guns target a player who has this item equipped?
		/// </summary>
		public virtual bool shouldFriendlySentryTargetUser => false;

		/// <summary>
		/// Kept in case any plugins refer to it.
		/// Renamed to shouldFriendlySentryTargetUser.
		/// </summary>
		[System.Obsolete]
		public virtual bool isDangerous => shouldFriendlySentryTargetUser;

		[System.Obsolete("canBeUsedInSafezone now has special cases for admins")]
		public virtual bool canBeUsedInSafezone(SafezoneNode safezone)
		{
			return canBeUsedInSafezone(safezone, /*byAdmin*/ false);
		}

		/// <summary>
		/// Should players be allowed to start primary/secondary use of this item while inside given safezone?
		/// If returns false the primary/secondary inputs are set to false.
		/// </summary>
		public virtual bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
		{
			if (safezone.noWeapons)
			{
				return shouldFriendlySentryTargetUser == false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Should this item be deleted when using and quality hits zero?
		/// e.g. final melee hit shatters the weapon.
		/// </summary>
		public bool shouldDeleteAtZeroQuality
		{
			get;
			protected set;
		}

		private CachingAssetRef _deletedAtZeroQualityEffectRef;
		public CachingAssetRef DeletedAtZeroQualityEffectRef
		{
			get => _deletedAtZeroQualityEffectRef;
			set => _deletedAtZeroQualityEffectRef = value;
		}

		public EffectAsset FindDeletedAtZeroQualityEffect()
		{
			return _deletedAtZeroQualityEffectRef.Get<EffectAsset>();
		}

		private NPCRewardsList _deletedAtZeroQualityRewards;
		public NPCRewardsList DeletedAtZeroQualityRewards
		{
			get => _deletedAtZeroQualityRewards;
			set => _deletedAtZeroQualityRewards = value;
		}

		/// <summary>
		/// Should the game destroy all child colliders on the item when requested?
		/// Physics items in the world and on character preview don't request destroy,
		/// but items attached to the character do. Mods might be using colliders
		/// in unexpected ways (e.g., riot shield) so they can disable this default.
		/// </summary>
		public bool shouldDestroyItemColliders
		{
			get;
			protected set;
		}

		public override EAssetType assetCategory => EAssetType.ITEM;

		/// <summary>
		/// Are there any official skins for this item type?
		/// Skips checking for base textures if item cannot have skins.
		/// </summary>
		protected virtual bool doesItemTypeHaveSkins => false;

		/// <summary>
		/// Defaults to null.
		/// Allows items (specifically fish) to override fishing settings.
		/// </summary>
		public FishingCatchableProperties FishingCatchable
		{
			get;
			set;
		}

		/// <summary>
		/// Find useableType by useable name.
		/// </summary>
		private void updateUseableType(IDatDictionary data)
		{
			useable = data.GetString("Useable");
			if (string.IsNullOrEmpty(useable))
			{
				useableType = null;
				return;
			}

			// Nelson 2024-11-11: Default to original behavior of looking up registered type by a short "user-friendly"
			// name like "Gun", then fallback to c# type lookup. (public issue #4752) Similar to Asset's Type property.
			useableType = Assets.useableTypes.getType(useable);
			if (useableType == null)
			{
				useableType = data.ParseType("Useable");
				if (useableType == null)
				{
					Assets.ReportError(this, "cannot find useable type \"{0}\"", useable);
				}
			}

			if (useableType != null && !typeof(Useable).IsAssignableFrom(useableType))
			{
				Assets.ReportError(this, "type \"{0}\" is not useable", useableType);
				useableType = null;
			}
		}

		public ItemAsset() : base()
		{
			_animations = new AnimationClip[0];

			_blueprints = new List<Blueprint>();
			_actions = new List<Action>();
		}

		private static List<AnimationClip> tempAnimations = new List<AnimationClip>();
		private void initAnimations(GameObject anim)
		{
			Animation animation = anim.GetComponent<Animation>();
			if (animation == null)
			{
				Assets.ReportError(this, "missing Animation component on 'Animations' GameObject");
				return;
			}

			if (animation.GetClipCount() < 1)
			{
				Assets.ReportError(this, "animation clips list is empty");
				return;
			}

			tempAnimations.Clear();

			bool hasNullClips = false;
			bool hasIdleClip = false;

			foreach (AnimationState state in animation)
			{
				AnimationClip clip = state.clip;
				if (clip == null)
				{
					hasNullClips = true;
					continue;
				}

				hasIdleClip = hasIdleClip || clip.name == "Equip";
				tempAnimations.Add(clip);
			}

			if (hasNullClips)
			{
				Assets.ReportError(this, "has invalid animation clip references");
			}

			if (!hasIdleClip)
			{
				Assets.ReportError(this, "missing 'Equip' animation clip");
			}

			if (tempAnimations.Count < 1)
			{
				// Null warning above will have been logged.
				return;
			}

			_animations = tempAnimations.ToArray();
		}

		public Asset GetBlueprintOwnerAsset()
		{
			return this;
		}

		public List<Blueprint> GetBlueprints()
		{
			return _blueprints;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			isPro = p.data.ContainsKey("Pro");

			if (id < 2000 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 2000");
			}

			// 2022-12-02: previously localization was skipped for `isPro` items because Steam
			// item localization is handled elsewhere. This was changed for issue #3546 because
			// they are using items as a clever hack for vendor icons.
			// Nelson 2024-06-11: Previously, if Name wasn't specified in the localization file, _itemName defaulted to
			// "Name." To hide this default from parts of the UI (e.g., gun's attachments description) I want to check
			// if the string is default. Some parts of the game assume itemName isn't null, and .read() returns null
			// if excluded, so we default to an empty string. (public issue #4494)
			_itemName = p.localization.FormatOrEmpty("Name");

			if (Assets.shouldValidateAssets && (_itemName.Trim().Length != _itemName.Length))
			{
				Assets.ReportError(this, "Display name has leading or trailing whitespace");
			}
			_itemDescription = p.localization.format("Description");
			_itemDescription = ItemTool.filterRarityRichText(itemDescription);
			RichTextUtil.replaceNewlineMarkup(ref _itemDescription);

			instantiatedItemName = p.data.GetString("Instantiated_Item_Name_Override", defaultValue: id.ToString());

			type = (EItemType) System.Enum.Parse(typeof(EItemType), p.data.GetString("Type"), true);

			if (p.data.ContainsKey("Rarity"))
			{
				rarity = (EItemRarity) System.Enum.Parse(typeof(EItemRarity), p.data.GetString("Rarity"), true);
			}
			else
			{
				rarity = EItemRarity.COMMON;
			}

			if (isPro)
			{
				econIconUseId = p.data.ParseBool("Econ_Icon_Use_Id");

				if (type == EItemType.SHIRT)
				{
					_proPath = "/Shirts";
				}
				else if (type == EItemType.PANTS)
				{
					_proPath = "/Pants";
				}
				else if (type == EItemType.HAT)
				{
					_proPath = "/Hats";
				}
				else if (type == EItemType.BACKPACK)
				{
					_proPath = "/Backpacks";
				}
				else if (type == EItemType.VEST)
				{
					_proPath = "/Vests";
				}
				else if (type == EItemType.MASK)
				{
					_proPath = "/Masks";
				}
				else if (type == EItemType.GLASSES)
				{
					_proPath = "/Glasses";
				}
				else if (type == EItemType.KEY)
				{
					_proPath = "/Keys";
				}
				else if (type == EItemType.BOX)
				{
					_proPath = "/Boxes";
				}

				_proPath += "/" + name;
			}

			size_x = p.data.ParseUInt8("Size_X");
			if (size_x < 1)
			{
				size_x = 1;
			}

			size_y = p.data.ParseUInt8("Size_Y");
			if (size_y < 1)
			{
				size_y = 1;
			}

			iconCameraOrthographicSize = p.data.ParseFloat("Size_Z", defaultValue: -1.0f);
			isEligibleForAutomaticIconMeasurements = p.data.ParseBool("Use_Auto_Icon_Measurements", defaultValue: true);
			econIconCameraOrthographicSize = p.data.ParseFloat("Size2_Z", defaultValue: -1.0f);

			sharedSkinLookupID = p.data.ParseUInt16("Shared_Skin_Lookup_ID", defaultValue: id);
			SharedSkinShouldApplyVisuals = p.data.ParseBool("Shared_Skin_Apply_Visuals", defaultValue: true);

			amount = p.data.ParseUInt8("Amount");
			if (amount < 1)
			{
				amount = 1;
			}

			countMin = p.data.ParseUInt8("Count_Min");
			if (countMin < 1)
			{
				countMin = 1;
			}

			countMax = p.data.ParseUInt8("Count_Max");
			if (countMax < 1)
			{
				countMax = 1;
			}

			if (p.data.ContainsKey("Quality_Min"))
			{
				qualityMin = p.data.ParseUInt8("Quality_Min");
			}
			else
			{
				qualityMin = 10;
			}

			if (p.data.ContainsKey("Quality_Max"))
			{
				qualityMax = p.data.ParseUInt8("Quality_Max");
			}
			else
			{
				qualityMax = 90;
			}

			if (p.data.TryParseEnum("EquipableModelParent", out EEquipableModelParent empValue))
			{
				EquipableModelParent = empValue;
			}
			else if (p.data.ContainsKey("Backward"))
			{
				EquipableModelParent = EEquipableModelParent.LeftHook;
			}
			else
			{
				EquipableModelParent = EEquipableModelParent.RightHook;
			}

			shouldLeftHandedCharactersMirrorEquippedItem = p.data.ParseBool("Left_Handed_Characters_Mirror_Equipable", defaultValue: true);

#pragma warning disable
			isEligibleForAutoStatDescriptions = p.data.ParseBool("Use_Auto_Stat_Descriptions", defaultValue: true);
			if (isEligibleForAutoStatDescriptions)
			{
				EItemDescriptionFlags descriptionFlags = EItemDescriptionFlags.All;
				if (p.data.TryParseEnum("Description_Excludes", out EItemDescriptionFlags removeFlags))
				{
					descriptionFlags &= ~removeFlags;
				}
				PreferredDescriptionFlags = descriptionFlags;
			}
			else
			{
				EItemDescriptionFlags descriptionFlags = EItemDescriptionFlags.LegacyContent;
				if (p.data.TryParseEnum("Description_Includes", out EItemDescriptionFlags addFlags))
				{
					descriptionFlags |= addFlags;
				}
				PreferredDescriptionFlags = descriptionFlags;
			}
#pragma warning restore

			shouldProcedurallyAnimateInertia = p.data.ParseBool("Procedurally_Animate_Inertia", defaultValue: true);

			updateUseableType(p.data);

			bool defaultCanPlayerEquip = useableType != null;
			canPlayerEquip = p.data.ParseBool("Can_Player_Equip", defaultValue: defaultCanPlayerEquip);

			// This is always loaded regardless of canPlayerEquip because gun attachments affect equipable speed multiplier.
			equipableMovementSpeedMultiplier = p.data.ParseFloat("Equipable_Movement_Speed_Multiplier", defaultValue: 1.0f);

			if (canPlayerEquip)
			{
				_equip = LoadRedirectableAsset<AudioClip>(p.bundle, "Equip", p.data, "EquipAudioClip");
				inspectAudio = p.data.ReadAudioReference("InspectAudioDef", p.bundle);

				MasterBundleReference<GameObject> equipablePrefabRef = p.data.readMasterBundleReference<GameObject>("EquipablePrefab", p.bundle);
				if (equipablePrefabRef.isValid)
				{
					equipablePrefab = equipablePrefabRef.loadAsset();
				}

				if (!isPro)
				{
					GameObject anim = p.bundle.load<GameObject>("Animations");

					if (anim != null)
					{
						initAnimations(anim);
					}
					else
					{
						_animations = new AnimationClip[0];
					}
				}
			}

			if (p.data.ContainsKey("InventoryAudio"))
			{
				inventoryAudio = p.data.ReadAudioReference("InventoryAudio", p.bundle);
			}
			else
			{
				inventoryAudio = GetDefaultInventoryAudio();
			}

			slot = p.data.ParseEnum("Slot", defaultValue: ESlotType.NONE);

			bool defaultCanUseUnderwater = slot != ESlotType.PRIMARY;
			canUseUnderwater = p.data.ParseBool("Can_Use_Underwater", defaultValue: defaultCanUseUnderwater);

			if (!Dedicator.IsDedicatedServer || type == EItemType.GUN || type == EItemType.MELEE || shouldAlwaysLoadItemPrefab)
			{
				_item = p.bundle.load<GameObject>("Item");

				if (item == null)
				{
					// Nelson 2025-08-25: hackily removing this shirt/pants check for unobtainable zip/oversize fallback shirt.
					//bool isPremiumAndDoesntNeedItem = isPro && type != EItemType.SHIRT && type != EItemType.PANTS;
					if (!isPro)
					{
						throw new System.NotSupportedException($"missing \"Item\" GameObject (expected at {p.bundle.WhereLoadLookedToString("Item")})");
					}
				}
				else
				{
					if (item.transform.Find("Icon") != null)
					{
						if (item.transform.Find("Icon").GetComponent<Camera>() != null)
						{
							throw new System.NotSupportedException("'Icon' has a camera attached!");
						}
					}

					if (id < 2000 && (type == EItemType.GUN || type == EItemType.MELEE))
					{
						if (item.transform.Find("Stat_Tracker") == null)
						{
							Assets.ReportError(this, "missing 'Stat_Tracker' Transform");
						}
					}

					AssetValidation.searchGameObjectForErrors(this, item);

					if (Assets.shouldValidateAssets)
					{
						MeshCollider mc = _item.GetComponentInChildren<MeshCollider>();
						if (mc != null && !mc.convex)
						{
							ReportAssetError($"contains non-convex MeshCollider at {mc.GetSceneHierarchyPath()}");
						}
					}
				}
			}

			// Nelson 2025-05-16: oversight when Add_Default_Actions was moved ahead of parsing blueprints for legacy
			// conversion. We were checking actions.Count == 0, but actions depend on blueprints. :P
			bool hasNoActionsDefined = !p.data.ContainsKey("Actions");
			bool shouldAddDefaultActions = p.data.ParseBool("Add_Default_Actions", defaultValue: hasNoActionsDefined);

			bool parsedLegacyBlueprints = false;
			if (p.data.TryGetNode("Blueprints", out IDatNode blueprintsNode))
			{
				if (blueprintsNode is IDatValue blueprintsCountNode)
				{
					if (blueprintsCountNode.TryParseUInt8(out byte blueprintCount))
					{
						parsedLegacyBlueprints = true;
						PopulateBlueprintsLegacy(p.data, blueprintCount, p.localization, shouldAddDefaultActions);
					}
					else
					{
						ReportAssetError($"unable to parse Blueprints count");
					}
				}
				else if (blueprintsNode is IDatList blueprintsListNode)
				{
					_blueprints = PopulateBlueprintsV2(blueprintsListNode, p.localization, this);
				}
			}
			if (_blueprints == null)
			{
				// Nelson 2025-03-20: probably wouldn't do it this way nowadays, but existing code expects blueprints
				// to be empty rather than null.
				_blueprints = new List<Blueprint>();
			}
			else if (_blueprints.Count > byte.MaxValue)
			{
				ReportAssetError($"has more than {byte.MaxValue} Blueprints which breaks some assumptions");
			}

			if (p.data.TryGetNode("Actions", out IDatNode actionsNode))
			{
				if (actionsNode is IDatValue actionsCountNode)
				{
					if (actionsCountNode.TryParseUInt8(out byte actionCount))
					{
						PopulateActionsLegacy(p.data, actionCount, p.localization);
					}
					else
					{
						ReportAssetError($"unable to parse Actions count");
					}
				}
				else if (actionsNode is IDatList actionsListNode)
				{
					PopulateActionsV2(actionsListNode, p.localization);
				}
			}
			if (_actions == null)
			{
				// Nelson 2025-04-08: probably wouldn't do it this way nowadays, but existing code expects actions
				// to be empty rather than null.
				_actions = new List<Action>();
			}

			if (shouldAddDefaultActions)
			{
				bool hasRefillBlueprint = false;
				bool hasCreatedRepairAction = false;
				bool hasCreatedSalvageAction = false;

				for (byte index = 0; index < blueprints.Count; index++)
				{
					Blueprint blueprint = blueprints[index];

					if (blueprint.Operation == EBlueprintOperation.RepairTargetItem)
					{
						if (!hasCreatedRepairAction)
						{
							if (parsedLegacyBlueprints)
							{
								blueprint.Name = "Repair";
							}

							hasCreatedRepairAction = true;
							Action action = new Action(0, EActionType.BLUEPRINT, new ActionBlueprint[] { new ActionBlueprint(index, true) }, null, null, "Repair");
							action.blueprintOwnerRef = this;
							actions.Insert(0, action);
						}
					}
					else if (blueprint.Operation == EBlueprintOperation.FillTargetItem)
					{
						hasRefillBlueprint = true;
					}
					else if (blueprint.supplies.Length == 1 && blueprint.supplies[0].IsItem(this))
					{
						if (!hasCreatedSalvageAction)
						{
							if (parsedLegacyBlueprints)
							{
								blueprint.Name = "Salvage";
							}

							hasCreatedSalvageAction = true;
							Action action = new Action(0, EActionType.BLUEPRINT, new ActionBlueprint[] { new ActionBlueprint(index, type == EItemType.GUN || type == EItemType.MELEE) }, null, null, "Salvage");
							action.blueprintOwnerRef = this;
							actions.Add(action);
						}
					}
				}

				if (hasRefillBlueprint)
				{
					List<ActionBlueprint> acBlueprints = new List<ActionBlueprint>();
					for (byte index = 0; index < blueprints.Count; index++)
					{
						Blueprint blueprint = blueprints[index];

						if (blueprint.Operation == EBlueprintOperation.FillTargetItem)
						{
							ActionBlueprint acBlueprint = new ActionBlueprint(index, true);
							acBlueprints.Add(acBlueprint);
						}
					}

					Action action = new Action(0, EActionType.BLUEPRINT, acBlueprints.ToArray(), null, null, "Refill");
					action.blueprintOwnerRef = this;
					actions.Add(action);
				}
			}

			_shouldVerifyHash = !p.data.ContainsKey("Bypass_Hash_Verification");
			overrideShowQuality = p.data.ContainsKey("Override_Show_Quality");
			shouldDropOnDeath = p.data.ParseBool("Should_Drop_On_Death", defaultValue: true);
			allowManualDrop = p.data.ParseBool("Allow_Manual_Drop", defaultValue: true);
			shouldDeleteAtZeroQuality = p.data.ParseBool("Should_Delete_At_Zero_Quality", defaultValue: false);
			if (shouldDeleteAtZeroQuality)
			{
				_deletedAtZeroQualityEffectRef = p.data.ParseAssetRef("Deleted_At_Zero_Quality_Effect");
				_deletedAtZeroQualityRewards.Parse(p.data, p.localization, this, "Deleted_At_Zero_Quality_Rewards",
					"Deleted_At_Zero_Quality_Reward_");
			}

			shouldDestroyItemColliders = p.data.ParseBool("Destroy_Item_Colliders", defaultValue: true);

			ShouldDeleteAtZeroAmount = p.data.ParseBool("Should_Delete_At_Zero_Amount", type != EItemType.MAGAZINE);
			if (type == EItemType.MAGAZINE && p.data.ContainsKey("Delete_Empty"))
			{
				// Backwards compatibility.
				ShouldDeleteAtZeroAmount = true;
			}

			// Only official content has skins, so we only check the official ID range of [1, 2000).
			if (!Dedicator.IsDedicatedServer && id < 2000 && doesItemTypeHaveSkins)
			{
				_albedoBase = p.bundle.load<Texture2D>("Albedo_Base");
				_metallicBase = p.bundle.load<Texture2D>("Metallic_Base");
				_emissionBase = p.bundle.load<Texture2D>("Emission_Base");
			}

			if (p.data.TryGetDictionary("Fishing_Catchable", out IDatDictionary dict))
			{
				FishingCatchable = new FishingCatchableProperties();
				FishingCatchable.Parse(dict);
				//UnturnedLog.info($"{itemName} fishing properties: {FishingCatchable}");
			}
		}

		/// <summary>
		/// V2 is for newer dat list features.
		/// </summary>
		internal static List<Blueprint> PopulateBlueprintsV2(IDatList blueprintsList, Local localization, Asset assetContext)
		{
			List<Blueprint> _blueprints = new List<Blueprint>(blueprintsList.Count);
			int nodeIndex = -1;
			foreach (IDatNode blueprintNode in blueprintsList)
			{
				nodeIndex += 1;
				if (!(blueprintNode is IDatDictionary blueprintDict))
				{
					continue;
				}

#pragma warning disable
				bool hasLegacyType = blueprintDict.TryParseEnum("Type", out EBlueprintType legacyType);
#pragma warning restore

				if (!blueprintDict.TryParseEnum("Operation", out EBlueprintOperation operation) && hasLegacyType)
				{
#pragma warning disable
					switch (legacyType)
					{
						case EBlueprintType.AMMO:
							operation = EBlueprintOperation.FillTargetItem;
							break;

						case EBlueprintType.REPAIR:
							operation = EBlueprintOperation.RepairTargetItem;
							break;
					}
#pragma warning restore
				}

				bool inputsDefaultCritical = blueprintDict.ParseBool("InputItems_Critical");

				BlueprintSupply[] supplies;
				if (blueprintDict.TryGetNode("InputItems", out IDatNode inputItemsNode))
				{
					if (inputItemsNode is IDatList inputItemsList)
					{
						List<BlueprintSupply> inputItems = new List<BlueprintSupply>(inputItemsList.Count);
						foreach (IDatNode inputItemNode in inputItemsList)
						{
							BlueprintSupply inputItem = ParseBlueprintInputItem(operation, inputItemNode, inputsDefaultCritical, assetContext);
							if (inputItem != null)
							{
								inputItems.Add(inputItem);
							}
							else
							{
								if (inputItemNode.TryGetNodePath(out string nodePath))
								{
									inputItemNode.TryGetParsedLineNumber(out int lineNumber);
									assetContext.ReportAssetError($"unable to parse blueprint input item {nodePath} on line {lineNumber}");
								}
								else
								{
									assetContext.ReportAssetError($"unable to parse blueprint {nodeIndex} input item");
								}
							}
						}
						supplies = inputItems.ToArray();
					}
					else
					{
						BlueprintSupply supply = ParseBlueprintInputItem(operation, inputItemsNode, inputsDefaultCritical, assetContext);
						if (supply != null)
						{
							supplies = new BlueprintSupply[] { supply };
						}
						else
						{
							assetContext.ReportAssetError("unable to parse InputItems");
							supplies = new BlueprintSupply[0];
						}
					}
				}
				else
				{
					supplies = new BlueprintSupply[0];
				}

				BlueprintOutput[] outputs;
				if (blueprintDict.TryGetNode("OutputItems", out IDatNode outputItemsNode))
				{
					if (outputItemsNode is IDatList outputItemsList)
					{
						List<BlueprintOutput> outputItems = new List<BlueprintOutput>(outputItemsList.Count);
						foreach (IDatNode outputItemNode in outputItemsList)
						{
							BlueprintOutput outputItem = ParseBlueprintOutputItem(outputItemNode, assetContext);
							if (outputItem != null)
							{
								outputItems.Add(outputItem);
							}
							else
							{
								if (outputItemNode.TryGetNodePath(out string nodePath))
								{
									outputItemNode.TryGetParsedLineNumber(out int lineNumber);
									assetContext.ReportAssetError($"unable to parse blueprint output item {nodePath} on line {lineNumber}");
								}
								else
								{
									assetContext.ReportAssetError($"unable to parse blueprint {nodeIndex} output item");
								}
							}
						}
						outputs = outputItems.ToArray();
					}
					else
					{
						BlueprintOutput outputItem = ParseBlueprintOutputItem(outputItemsNode, assetContext);
						if (outputItem != null)
						{
							outputs = new BlueprintOutput[] { outputItem };
						}
						else
						{
							assetContext.ReportAssetError("unable to parse OutputItems");
							outputs = new BlueprintOutput[0];
						}
					}
				}
				else
				{
					outputs = new BlueprintOutput[0];
				}

				CachingBcAssetRef effectAssetRef = blueprintDict.ParseGuidOrLegacyIdV2("Effect", EAssetType.EFFECT);

				byte level = blueprintDict.ParseUInt8("Skill_Level");
				int skillSpecialityIndex = -1;
				int skillIndex = -1;
				if (level > 0)
				{
					if (blueprintDict.TryParseEnum("Skill", out EBlueprintSkill legacySkill))
					{
						legacySkill.ToSkillIndices(out skillSpecialityIndex, out skillIndex);
					}
					else
					{
						string skillValue = blueprintDict.GetString("Skill");
						if (!PlayerSkills.TryParseIndices(skillValue, out skillSpecialityIndex, out skillIndex))
						{
							assetContext.ReportAssetError($"unable to parse blueprint Skill \"{skillValue}\"");
						}
					}
				}

				bool transferState = blueprintDict.ParseBool("StateTransfer");
				bool withoutAttachments = blueprintDict.ParseBool("StateTransfer_DeleteAttachments");
				string map = blueprintDict.GetString("Map");

				NPCConditionsList questConditionsList = new NPCConditionsList();
				questConditionsList.Parse(blueprintDict, localization, assetContext, "Conditions");

				NPCRewardsList questRewardsList = new NPCRewardsList();
				questRewardsList.Parse(blueprintDict, localization, assetContext, "Rewards");

				byte blueprintIndex = (byte) _blueprints.Count;
				Blueprint blueprint = new Blueprint(blueprintIndex, supplies, outputs,
					level, EBlueprintSkill.NONE, transferState, withoutAttachments, map, questConditionsList,
					questRewardsList);
				blueprint.Owner = assetContext as IBlueprintOwner;
				blueprint.effectAssetRef = effectAssetRef;
				blueprint.canBeVisibleWhenSearchedWithoutRequiredItems = blueprintDict.ParseBool("Searchable", defaultValue: true);
				blueprint.CanBeVisibleWithUnmetConditions = blueprintDict.ParseBool("VisibleWithUnmetConditions");
				blueprint.Name = blueprintDict.GetString("Name");
				blueprint.RequiresNearbyCraftingTags = blueprintDict.ParseArrayOfStructs<CachingAssetRef>("RequiresNearbyCraftingTags");
				blueprint.RequiresStaticTags = blueprintDict.ParseArrayOfStructs<CachingAssetRef>("RequiresStaticTags");
				blueprint._operation = operation;
				blueprint.SkillSpecialityIndex = skillSpecialityIndex;
				blueprint.SkillIndex = skillIndex;

				if (blueprint.Operation != EBlueprintOperation.None)
				{
					CachingBcAssetRef targetItemRef;
					if (blueprintDict.TryGetString("TargetItem", out string targetItemString))
					{
						if (!ParseItemString(targetItemString, out targetItemRef, out int _amount, assetContext))
						{
							targetItemRef = assetContext;
						}
					}
					else
					{
						targetItemRef = assetContext;
					}

					if (operation == EBlueprintOperation.RepairTargetItem)
					{
						const bool isCritical = true; // Need target item for it to be relevant.
						const bool countEmptyAsOne = false; // Legacy behaviour was to ignore empty items.
						BlueprintSupply targetItem = new BlueprintSupply(0, isCritical, 1, countEmptyAsOne,
							ECraftingInputPrioritization.LowestQuality); // Prioritize most-damaged item to repair.
						targetItem.ItemRef = targetItemRef;
						targetItem.ShouldIncludeMaxQuality = false; // Don't repair a full-quality item!
						targetItem.ShouldConsume = false; // Don't delete the repaired item.
						blueprint.TargetItem = targetItem;
					}
					else if (operation == EBlueprintOperation.FillTargetItem)
					{
						const bool isCritical = true; // Need target item for it to be relevant.
						const bool countEmptyAsOne = true;
						BlueprintSupply targetItem = new BlueprintSupply(0, isCritical, 1, countEmptyAsOne,
							ECraftingInputPrioritization.HighestAmount); // Prioritize most-full item to fill.
						targetItem.ItemRef = targetItemRef;
						targetItem.ShouldConsume = false; // Don't delete the refilled item.
						targetItem.ShouldExcludeFullAmount = true; // Don't fill a full item!
						blueprint.TargetItem = targetItem;

						if (blueprint.supplies.Length > 0)
						{
							// First input to refill target item should be critical because without it there's nothing to do.
							blueprint.supplies[0]._isCritical = true;
						}
					}
				}

				if (!blueprintDict.TryParseAssetRef("CategoryTag", out blueprint._categoryTagRef) && hasLegacyType)
				{
					blueprint._categoryTagRef = legacyType.GetCategoryTagRef();
				}

				if (blueprint.RequiresNearbyCraftingTags == null)
				{
					bool requiresHeat = blueprintDict.ParseBool("RequiresHeat", blueprint.GetLegacyBlueprintSkill() == EBlueprintSkill.COOK);
					if (requiresHeat)
					{
						blueprint.RequiresNearbyCraftingTags = new CachingAssetRef[1]
						{
							PowerTool.VanillaCraftingHeatTag,
						};
					}
				}

				_blueprints.Add(blueprint);
			}

			return _blueprints;
		}

		private static BlueprintSupply ParseBlueprintInputItem(EBlueprintOperation operation, IDatNode inputItemNode, bool defaultIsCritical, Asset assetContext)
		{
			ECraftingInputPrioritization defaultPrioritization = operation == EBlueprintOperation.FillTargetItem
				? ECraftingInputPrioritization.LowestAmount : ECraftingInputPrioritization.LowestQuality;
			ECraftingInputCountingMethod defaultCountingMethod = operation == EBlueprintOperation.FillTargetItem
				? ECraftingInputCountingMethod.TotalAmount : ECraftingInputCountingMethod.TotalItems;

			if (inputItemNode is IDatDictionary inputItemDict)
			{
				if (!inputItemDict.TryParseBcAssetRef("ID", EAssetType.ITEM, out CachingBcAssetRef itemRef))
				{
					if (string.Equals(inputItemDict.GetString("ID"), "this", System.StringComparison.InvariantCultureIgnoreCase))
					{
						itemRef = assetContext;
					}
					else
					{
						assetContext.ReportAssetError($"Unable to parse blueprint input item ID: \"{inputItemDict.GetString("ID")}\"");
						return null;
					}
				}
				bool critical = inputItemDict.ParseBool("Critical", defaultIsCritical);
				bool treatEmptyAsOne = inputItemDict.ParseBool("CountEmptyAsOne");
				bool allowEmpty = inputItemDict.ParseBool("AllowEmpty", treatEmptyAsOne);
				bool shouldConsume = inputItemDict.ParseBool("Delete", true);
				if (!shouldConsume)
				{
					// e.g. tools
					defaultCountingMethod = ECraftingInputCountingMethod.TotalItems;
				}

				ECraftingInputPrioritization prioritization = inputItemDict.ParseEnum("Prioritization", defaultPrioritization);
				ECraftingInputCountingMethod countingMethod = inputItemDict.ParseEnum("CountingMethod", defaultCountingMethod);

				int amount = inputItemDict.ParseInt32("Amount", 1);
				if (amount < 1)
				{
					amount = 1;
				}

				BlueprintSupply inputItem = new BlueprintSupply(0, critical, amount, treatEmptyAsOne, prioritization);
				inputItem.ShouldConsume = shouldConsume;
				inputItem.ShouldIncludeEmptyAmount = allowEmpty;
				inputItem.ShouldExcludeFullAmount = !inputItemDict.ParseBool("AllowFull", true);
				inputItem.ShouldIncludeMaxQuality = inputItemDict.ParseBool("AllowMaxQuality", true);
				inputItem.CountingMethod = countingMethod;
				inputItem.ItemRef = itemRef;
				return inputItem;
			}
			else if (inputItemNode is IDatValue inputItemValue)
			{
				CachingBcAssetRef itemRef;
				int amount;
				if (!ParseItemString(inputItemValue.Value, out itemRef, out amount, assetContext))
				{
					return null;
				}

				BlueprintSupply inputItem = new BlueprintSupply(0, defaultIsCritical, amount, false, defaultPrioritization);
				inputItem.ShouldConsume = true;
				inputItem.ItemRef = itemRef;
				inputItem.CountingMethod = defaultCountingMethod;
				return inputItem;
			}

			return null;
		}

		private static BlueprintOutput ParseBlueprintOutputItem(IDatNode outputItemNode, Asset assetContext)
		{
			if (outputItemNode is IDatDictionary outputItemDict)
			{
				if (!outputItemDict.TryParseBcAssetRef("ID", EAssetType.ITEM, out CachingBcAssetRef itemRef))
				{
					if (string.Equals(outputItemDict.GetString("ID"), "this", System.StringComparison.InvariantCultureIgnoreCase))
					{
						itemRef = assetContext;
					}
					else
					{
						assetContext.ReportAssetError($"Unable to parse blueprint output item ID: \"{outputItemDict.GetString("ID")}\"");
					}
				}

				int amount = outputItemDict.ParseInt32("Amount", 1);
				if (amount < 1)
				{
					amount = 1;
				}

				EItemOrigin origin = outputItemDict.ParseEnum("Origin", defaultValue: EItemOrigin.CRAFT);
				BlueprintOutput outputItem = new BlueprintOutput(0, amount, origin);
				outputItem.ItemRef = itemRef;
				return outputItem;
			}
			else if (outputItemNode is IDatValue outputItemValue)
			{
				CachingBcAssetRef itemRef;
				int amount;
				if (!ParseItemString(outputItemValue.Value, out itemRef, out amount, assetContext))
				{
					return null;
				}

				BlueprintOutput outputItem = new BlueprintOutput(0, amount, EItemOrigin.CRAFT);
				outputItem.ItemRef = itemRef;
				return outputItem;
			}

			return null;
		}

		private static bool ParseItemString(string input, out CachingBcAssetRef assetRef, out int amount, Asset assetContext)
		{
			assetRef = CachingBcAssetRef.Empty;
			string idString;
			amount = 1;
			int amountDelimiterIndex = input.IndexOf('x');
			if (amountDelimiterIndex < 0)
			{
				idString = input;
			}
			else
			{
				idString = input.Substring(0, amountDelimiterIndex);
				string amountString = input.Substring(amountDelimiterIndex + 1);
				if (!int.TryParse(amountString, out amount))
				{
					assetContext.ReportAssetError($"Unable to parse blueprint input item amount: \"{amountString}\"");
					return false;
				}
				if (amount < 1)
				{
					amount = 1;
				}
			}

			if (!CachingBcAssetRef.TryParse(idString, EAssetType.ITEM, out assetRef))
			{
				if (string.Equals(idString.Trim(), "this", System.StringComparison.InvariantCultureIgnoreCase))
				{
					assetRef = assetContext;
				}
				else
				{
					assetContext.ReportAssetError($"Unable to parse blueprint input item ID: \"{idString}\"");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Legacy is for backwards compatibility with Blueprint_# format.
		/// </summary>
		private void PopulateBlueprintsLegacy(IDatDictionary data, byte blueprintCount, Local localization, bool shouldAddDefaultActions)
		{
			_blueprints = new List<Blueprint>(blueprintCount);
			for (byte index = 0; index < blueprintCount; index++)
			{
				if (!data.ContainsKey("Blueprint_" + index + "_Type"))
				{
					throw new System.NotSupportedException("Missing blueprint type");
				}

				EBlueprintOperation operation = EBlueprintOperation.None;

#pragma warning disable
				EBlueprintType bpType = (EBlueprintType) System.Enum.Parse(typeof(EBlueprintType), data.GetString("Blueprint_" + index + "_Type"), true);
				switch (bpType)
				{
					case EBlueprintType.AMMO:
						operation = EBlueprintOperation.FillTargetItem;
						break;

					case EBlueprintType.REPAIR:
						operation = EBlueprintOperation.RepairTargetItem;
						break;
				}
#pragma warning restore

				// Nelson 2025-03-03: Default prioritization matches behavior prior to adding the option.
				// Note: if changing defaults here please also update legacy format conversion.
				ECraftingInputPrioritization defaultPrioritization = operation == EBlueprintOperation.FillTargetItem
					? ECraftingInputPrioritization.LowestAmount : ECraftingInputPrioritization.LowestQuality;
				ECraftingInputCountingMethod defaultCountingMethod = operation == EBlueprintOperation.FillTargetItem
					? ECraftingInputCountingMethod.TotalAmount : ECraftingInputCountingMethod.TotalItems;

				byte recipes = data.ParseUInt8("Blueprint_" + index + "_Supplies");
				if (recipes < 1)
				{
					recipes = 1;
				}

				tempBlueprintSupplies.Clear();
				for (byte step = 0; step < recipes; step++)
				{
					ushort supply;
					string supplyKey = "Blueprint_" + index + "_Supply_" + step + "_ID";
					if (!data.TryParseUInt16(supplyKey, out supply))
					{
						if (string.Equals(data.GetString(supplyKey), "this", System.StringComparison.InvariantCultureIgnoreCase))
						{
							supply = id;
						}
						else
						{
							Assets.ReportError(this, $"Unable to parse {supplyKey}: \"{data.GetString(supplyKey)}\"");
						}
					}
					bool critical = data.ContainsKey("Blueprint_" + index + "_Supply_" + step + "_Critical");

					// First input to refill target item should be critical because without it there's nothing to do.
					critical |= (operation == EBlueprintOperation.FillTargetItem && step == 0);

					bool treatEmptyAsOne = data.ParseBool("Blueprint_" + index + "_Supply_" + step + "_AllowEmpty");

					ECraftingInputPrioritization prioritization = data.ParseEnum("Blueprint_" + index + "_Supply_" + step + "_Prioritization", defaultPrioritization);

					int number = data.ParseInt32("Blueprint_" + index + "_Supply_" + step + "_Amount");
					if (number < 1)
					{
						number = 1;
					}

					BlueprintSupply inputItem = new BlueprintSupply(0, critical, number, treatEmptyAsOne, prioritization);
					inputItem.ItemRef = new CachingBcAssetRef(EAssetType.ITEM, supply);
					inputItem.CountingMethod = defaultCountingMethod;
					tempBlueprintSupplies.Add(inputItem);
				}

				ushort tool = data.ParseUInt16("Blueprint_" + index + "_Tool");
				bool toolCritical = data.ContainsKey("Blueprint_" + index + "_Tool_Critical");
				if (tool != 0)
				{
					BlueprintSupply toolInputItem = new BlueprintSupply(0, toolCritical, 1, false, ECraftingInputPrioritization.LowestQuality);
					toolInputItem.ItemRef = new CachingBcAssetRef(EAssetType.ITEM, tool);
					toolInputItem.ShouldConsume = false;
					tempBlueprintSupplies.Add(toolInputItem);
					// Don't use default counting method here because tool isn't ammo. :P
				}

				BlueprintOutput[] outputs;
				if (operation != EBlueprintOperation.None)
				{
					// EBlueprintType.Repair and EBlueprintType.Ammo didn't support output items.
					outputs = new BlueprintOutput[0];
				}
				else
				{
					byte outputCount = data.ParseUInt8("Blueprint_" + index + "_Outputs");
					if (outputCount > 0)
					{
						outputs = new BlueprintOutput[outputCount];

						for (byte step = 0; step < outputs.Length; step++)
						{
							ushort supply = data.ParseUInt16("Blueprint_" + index + "_Output_" + step + "_ID");
							int number = data.ParseInt32("Blueprint_" + index + "_Output_" + step + "_Amount");
							if (number < 1)
							{
								number = 1;
							}

							EItemOrigin origin = data.ParseEnum<EItemOrigin>("Blueprint_" + index + "_Output_" + step + "_Origin", defaultValue: EItemOrigin.CRAFT);

							outputs[step] = new BlueprintOutput(0, number, origin);
							outputs[step].ItemRef = new CachingBcAssetRef(EAssetType.ITEM, supply);
						}
					}
					else
					{
						outputs = new BlueprintOutput[1];

						ushort product = data.ParseUInt16("Blueprint_" + index + "_Product");
						if (product == 0)
						{
							product = id;
						}

						byte products = data.ParseUInt8("Blueprint_" + index + "_Products");
						if (products < 1)
						{
							products = 1;
						}

						EItemOrigin origin = data.ParseEnum<EItemOrigin>("Blueprint_" + index + "_Origin", defaultValue: EItemOrigin.CRAFT);

						outputs[0] = new BlueprintOutput(0, products, origin);
						outputs[0].ItemRef = new CachingBcAssetRef(EAssetType.ITEM, product);
					}
				}

				CachingBcAssetRef effectAssetRef = data.ParseGuidOrLegacyIdV2("Blueprint_" + index + "_Build", EAssetType.EFFECT);

				byte level = data.ParseUInt8("Blueprint_" + index + "_Level");

				EBlueprintSkill skill = EBlueprintSkill.NONE;
				if (level > 0)
				{
					skill = (EBlueprintSkill) System.Enum.Parse(typeof(EBlueprintSkill), data.GetString("Blueprint_" + index + "_Skill"), true);
				}

				bool transferState = data.ContainsKey("Blueprint_" + index + "_State_Transfer");
				bool withoutAttachments = data.ParseBool("Blueprint_" + index + "_State_Transfer_Delete_Attachments");
				string map = data.GetString("Blueprint_" + index + "_Map");

				NPCConditionsList questConditionsList = new NPCConditionsList();
				questConditionsList.Parse(data, localization, this, "Blueprint_" + index + "_Conditions", "Blueprint_" + index + "_Condition_");

				NPCRewardsList questRewardsList = new NPCRewardsList();
				questRewardsList.Parse(data, localization, this, "Blueprint_" + index + "_Rewards", "Blueprint_" + index + "_Reward_");

				Blueprint blueprint = new Blueprint(index, tempBlueprintSupplies.ToArray(), outputs,
					level, skill, transferState, withoutAttachments, map,
					questConditionsList, questRewardsList);
				blueprint.Owner = this;
				blueprint.effectAssetRef = effectAssetRef;
				blueprint.canBeVisibleWhenSearchedWithoutRequiredItems = data.ParseBool($"Blueprint_{index}_Searchable", defaultValue: true);
				blueprint._operation = operation;

				int specialityIndex;
				int skillIndex;
				skill.ToSkillIndices(out specialityIndex, out skillIndex);
				blueprint.SkillSpecialityIndex = specialityIndex;
				blueprint.SkillIndex = skillIndex;

				CachingAssetRef categoryTagRef;
				if (shouldAddDefaultActions && operation == EBlueprintOperation.None
					&& tempBlueprintSupplies.Count == 1 && tempBlueprintSupplies[0].IsItem(this))
				{
					// This matches existing automatic "salvage" action creations.
					// Nelson 2025-04-17: adding this new category as part of conversion from old to new format.
					// Considering that we already create a salvage action I think it will be helpful to merge all of
					// those blueprints into a new category, tidying up the other categories.
					categoryTagRef = EBlueprintTypeEx.salvageCategoryTagRef;
				}
				else
				{
					categoryTagRef = bpType.GetCategoryTagRef();
				}
				blueprint._categoryTagRef = categoryTagRef;

				if (operation == EBlueprintOperation.RepairTargetItem)
				{
					const bool isCritical = true; // Need target item for it to be relevant.
					const bool countEmptyAsOne = false; // Legacy behaviour was to ignore empty items.
					BlueprintSupply targetItem = new BlueprintSupply(0, isCritical, 1, countEmptyAsOne,
						ECraftingInputPrioritization.LowestQuality); // Prioritize most-damaged item to repair.
					targetItem.ItemRef = this;
					targetItem.ShouldIncludeMaxQuality = false; // Don't repair a full-quality item!
					targetItem.ShouldConsume = false; // Don't delete the repaired item.
					blueprint.TargetItem = targetItem;
				}
				else if (operation == EBlueprintOperation.FillTargetItem)
				{
					const bool isCritical = true; // Need target item for it to be relevant.
					const bool countEmptyAsOne = true;
					BlueprintSupply targetItem = new BlueprintSupply(0, isCritical, 1, countEmptyAsOne,
						ECraftingInputPrioritization.HighestAmount); // Prioritize most-full item to fill.
					targetItem.ItemRef = this;
					targetItem.ShouldConsume = false; // Don't delete the refilled item.
					targetItem.ShouldExcludeFullAmount = true; // Don't fill a full item!
					blueprint.TargetItem = targetItem;
				}

				if (skill == EBlueprintSkill.COOK)
				{
					blueprint.RequiresNearbyCraftingTags = new CachingAssetRef[1]
					{
						PowerTool.VanillaCraftingHeatTag,
					};
				}

				blueprints.Add(blueprint);
			}
		}

		/// <summary>
		/// V2 is for newer dat list features.
		/// </summary>
		private void PopulateActionsV2(IDatList actionsList, Local localization)
		{
			_actions = new List<Action>(actionsList.Count);
			int nodeIndex = -1;
			foreach (IDatNode actionNode in actionsList)
			{
				nodeIndex += 1;
				if (!(actionNode is IDatDictionary actionDict))
				{
					continue;
				}

				if (!actionDict.TryParseEnum("Type", out EActionType actionType))
				{
					ReportAssetError($"unable to parse action Type \"{actionDict.GetString("Type")}\"");
					continue;
				}

				if (!actionDict.TryGetString("BlueprintName", out string blueprintName))
				{
					ReportAssetError("action requires BlueprintName property");
					continue;
				}

				CachingAssetRef blueprintOwnerRef;
				if (!actionDict.TryParseAssetRef("BlueprintOwner", out blueprintOwnerRef))
				{
					blueprintOwnerRef = this;
				}

				bool isLink = actionDict.ParseBool("BlueprintLink");

				string textOverride = null;
				string tooltipOverride = null;
				string commonTextId = actionDict.GetString("CommonTextId");
				if (string.IsNullOrEmpty(commonTextId))
				{
					if (actionDict.TryGetString("TextId", out string textId))
					{
						textOverride = localization.format(textId);
					}
					if (actionDict.TryGetString("TooltipId", out string tooltipId))
					{
						tooltipOverride = localization.format(tooltipId);
					}
				}

				ActionBlueprint actionBlueprint = new ActionBlueprint(-1, isLink);
				actionBlueprint.blueprintName = blueprintName;

				// Nelson 2025-04-08: I'm not sure anywhere uses more than 1 blueprint?
				ActionBlueprint[] blueprints = new ActionBlueprint[1]
				{
					actionBlueprint
				};

				Action action = new Action(0, actionType, blueprints, textOverride, tooltipOverride, commonTextId);
				action.blueprintOwnerRef = blueprintOwnerRef;
				actions.Add(action);
			}
		}

		/// <summary>
		/// Legacy is for backwards compatibility with Action_# format.
		/// </summary>
		private void PopulateActionsLegacy(IDatDictionary data, byte actionCount, Local localization)
		{
			_actions = new List<Action>(actionCount);
			for (byte index = 0; index < actionCount; index++)
			{
				if (!data.ContainsKey("Action_" + index + "_Type"))
				{
					throw new System.NotSupportedException("Missing action type");
				}

				EActionType acType = (EActionType) System.Enum.Parse(typeof(EActionType), data.GetString("Action_" + index + "_Type"), true);

				string text;
				string tooltip;
				string key = data.GetString("Action_" + index + "_Key");
				if (string.IsNullOrEmpty(key))
				{
					string textKey = "Action_" + index + "_Text";
					if (localization.has(textKey))
					{
						text = localization.format(textKey);
					}
					else
					{
						text = data.GetString(textKey);
					}

					string tooltipKey = "Action_" + index + "_Tooltip";
					if (localization.has(tooltipKey))
					{
						tooltip = localization.format(tooltipKey);
					}
					else
					{
						tooltip = data.GetString(tooltipKey);
					}
				}
				else
				{
					text = string.Empty;
					tooltip = string.Empty;
				}

				string sourceItemKey = "Action_" + index + "_Source";
				CachingBcAssetRef sourceAssetRef;
				if (!data.TryParseBcAssetRef(sourceItemKey, EAssetType.ITEM, out sourceAssetRef))
				{
					sourceAssetRef = this;
				}

				byte recipes = data.ParseUInt8("Action_" + index + "_Blueprints");
				if (recipes < 1)
				{
					recipes = 1;
				}

				ActionBlueprint[] supplies = new ActionBlueprint[recipes];

				Action action = new Action(0, acType, supplies, text, tooltip, key);
				action.blueprintOwnerRef = sourceAssetRef;

				for (byte step = 0; step < supplies.Length; step++)
				{
					int blueprintIndex = -1;
					string blueprintName;
					string bpNameKey = "Action_" + index + "_Blueprint_" + step + "_Name";
					if (!data.TryGetString(bpNameKey, out blueprintName))
					{
						blueprintIndex = data.ParseUInt8("Action_" + index + "_Blueprint_" + step + "_Index");
					}
					bool isLink = data.ContainsKey("Action_" + index + "_Blueprint_" + step + "_Link");

					ActionBlueprint ab = new ActionBlueprint(blueprintIndex, isLink);
					ab.blueprintName = blueprintName;
					supplies[step] = ab;
				}

				actions.Add(action);
			}
		}

		internal override void PreResaveAsset(IDatDictionary data)
		{
			base.PreResaveAsset(data);

			if (data.ParseUInt8("Blueprints") > 0)
			{
				bool canConvertBlueprints = true;
				foreach (Blueprint blueprint in blueprints)
				{
					if (blueprint.questConditions != null || blueprint.questRewards != null)
					{
						canConvertBlueprints = false;
						break;
					}
				}
				if (canConvertBlueprints)
				{
					ConvertLegacyBlueprintsFormat(data);
				}
				else
				{
					UnturnedLog.info($"Cannot automatically convert {FriendlyNameWithFriendlyType} Blueprints (yet) because they use conditions/rewards");
				}
			}
		}

		private void ConvertLegacyBlueprintsFormat(IDatDictionary data)
		{
			IEditableDatDictionary editableData = data.Edit();
			UnturnedLog.info($"Converting {FriendlyNameWithFriendlyType} Blueprints format");
			List<string> keysToRemove = new List<string>();
			foreach (KeyValuePair<string, IDatNode> kvp in data)
			{
				if (kvp.Key.StartsWith("Blueprint_", System.StringComparison.InvariantCultureIgnoreCase))
				{
					keysToRemove.Add(kvp.Key);
				}
			}
			UnturnedLog.info($"Removing keys: {string.Join(", ", keysToRemove)}");
			foreach (string key in keysToRemove)
			{
				editableData.Remove(key);
			}
			IEditableDatList blueprintsDatList = editableData.ReplaceWithList("Blueprints");
			blueprintsDatList.SetMargins(1); // It's a big list, so margin here helps distinguish adjacent keys.
			foreach (Blueprint blueprint in blueprints)
			{
				IEditableDatDictionary blueprintDict = blueprintsDatList.AddDictionary();
				if (!string.IsNullOrEmpty(blueprint.Name))
				{
					blueprintDict.AddValue("Name").SetString(blueprint.Name);
				}
				if (blueprint.CategoryTagRef.IsAssigned)
				{
					blueprintDict.AddValue("CategoryTag").SetAssetRefWithInlineComment(blueprint.CategoryTagRef);
				}
				if (blueprint.Operation != EBlueprintOperation.None)
				{
					blueprintDict.AddValue("Operation").SetEnumString(blueprint.Operation);
				}

				ECraftingInputPrioritization defaultPrioritization = blueprint.Operation == EBlueprintOperation.FillTargetItem
					? ECraftingInputPrioritization.LowestAmount : ECraftingInputPrioritization.LowestQuality;
				ECraftingInputCountingMethod defaultCountingMethod = blueprint.Operation == EBlueprintOperation.FillTargetItem
					? ECraftingInputCountingMethod.TotalAmount : ECraftingInputCountingMethod.TotalItems;

				if (!blueprint.supplies.IsNullOrEmpty())
				{
					bool shouldExpand = false;
					for (int index = 0; index < blueprint.supplies.Length; ++index)
					{
						BlueprintSupply inputItem = blueprint.supplies[index];

						bool defaultCritical = (index == 0 && blueprint.Operation == EBlueprintOperation.FillTargetItem);
						if (inputItem.isCritical != defaultCritical)
						{
							shouldExpand = true;
							break;
						}

						if (!inputItem.ShouldConsume)
						{
							shouldExpand = true;
							break;
						}

						if (inputItem.Prioritization != defaultPrioritization)
						{
							shouldExpand = true;
							break;
						}

						if (inputItem.ShouldCountEmptyAsOne)
						{
							shouldExpand = true;
							break;
						}
					}

					if (shouldExpand)
					{
						IEditableDatList inputItems = blueprintDict.AddList("InputItems");
						foreach (BlueprintSupply inputItem in blueprint.supplies)
						{
							IEditableDatDictionary inputItemDict = inputItems.AddDictionary();
							if (inputItem.ItemRef.IsReferenceTo(this))
							{
								inputItemDict.AddValue("ID").SetString("this");
							}
							else
							{
								inputItemDict.AddValue("ID").SetAssetRefWithInlineComment(inputItem.ItemRef);
							}

							if (inputItem.amount != 1)
							{
								inputItemDict.AddValue("Amount").SetInt32(inputItem.amount);
							}

							if (inputItem.isCritical)
							{
								inputItemDict.AddValue("Critical").SetBool(true);
							}

							if (!inputItem.ShouldConsume)
							{
								inputItemDict.AddValue("Delete").SetBool(false);
							}

							if (inputItem.Prioritization != defaultPrioritization)
							{
								inputItemDict.AddValue("Prioritization").SetEnumString(inputItem.Prioritization);
							}

							if (inputItem.ShouldCountEmptyAsOne)
							{
								inputItemDict.AddValue("CountEmptyAsOne").SetBool(true);
							}
						}
					}
					else
					{
						if (blueprint.supplies.Length > 1)
						{
							IEditableDatList inputItems = blueprintDict.AddList("InputItems");
							foreach (BlueprintSupply inputItem in blueprint.supplies)
							{
								IEditableDatValue inputItemValueNode = inputItems.AddValue();
								ConvertSimpleItemRefAndAmount(inputItem.ItemRef, inputItem.amount, inputItemValueNode);
							}
						}
						else
						{
							BlueprintSupply inputItem = blueprint.supplies[0];
							ConvertSimpleItemRefAndAmount(inputItem.ItemRef, inputItem.amount, blueprintDict.AddValue("InputItems"));
						}
					}
				}

				if (!blueprint.outputs.IsNullOrEmpty())
				{
					bool shouldExpand = false;
					foreach (BlueprintOutput outputItem in blueprint.outputs)
					{
						shouldExpand |= outputItem.origin != EItemOrigin.CRAFT;
					}
					if (shouldExpand)
					{
						IEditableDatList outputItems = blueprintDict.AddList("OutputItems");
						foreach (BlueprintOutput outputItem in blueprint.outputs)
						{
							IEditableDatDictionary outputItemDict = outputItems.AddDictionary();
							if (outputItem.ItemRef.IsReferenceTo(this))
							{
								outputItemDict.AddValue("ID").SetString("this");
							}
							else
							{
								outputItemDict.AddValue("ID").SetAssetRefWithInlineComment(outputItem.ItemRef);
							}

							if (outputItem.amount != 1)
							{
								outputItemDict.AddValue("Amount").SetInt32(outputItem.amount);
							}

							if (outputItem.origin != EItemOrigin.CRAFT)
							{
								outputItemDict.AddValue("Origin").SetString(outputItem.origin.ToStringPascalCase());
							}
						}
					}
					else
					{
						if (blueprint.outputs.Length > 1)
						{
							IEditableDatList outputItems = blueprintDict.AddList("OutputItems");
							foreach (BlueprintOutput outputItem in blueprint.outputs)
							{
								IEditableDatValue outputItemValueNode = outputItems.AddValue();
								ConvertSimpleItemRefAndAmount(outputItem.ItemRef, outputItem.amount, outputItemValueNode);
							}
						}
						else
						{
							BlueprintOutput outputItem = blueprint.outputs[0];
							ConvertSimpleItemRefAndAmount(outputItem.ItemRef, outputItem.amount, blueprintDict.AddValue("OutputItems"));
						}
					}
				}

				EBlueprintSkill legacySkill = blueprint.GetLegacyBlueprintSkill();
				if (legacySkill != EBlueprintSkill.NONE)
				{
					blueprintDict.AddValue("Skill").SetString(legacySkill.ToStringPascalCase());
					blueprintDict.AddValue("Skill_Level").SetInt32(blueprint.level);
				}

				if (blueprint.transferState)
				{
					blueprintDict.AddValue("StateTransfer").SetBool(true);
					if (blueprint.withoutAttachments)
					{
						blueprintDict.AddValue("StateTransfer_DeleteAttachments").SetBool(true);
					}
				}

				if (!string.IsNullOrEmpty(blueprint.map))
				{
					blueprintDict.AddValue("Map").SetString(blueprint.map);
				}

				if (!blueprint.canBeVisibleWhenSearchedWithoutRequiredItems)
				{
					blueprintDict.AddValue("Searchable").SetBool(false);
				}

				if (blueprint.RequiresNearbyCraftingTags != null && blueprint.RequiresNearbyCraftingTags.Length > 0)
				{
					IEditableDatList craftingTagsList = blueprintDict.AddList("RequiresNearbyCraftingTags");
					foreach (CachingAssetRef tagRef in blueprint.RequiresNearbyCraftingTags)
					{
						craftingTagsList.AddValue().SetAssetRefWithInlineComment(tagRef);
					}
				}

				if (blueprint.RequiresStaticTags != null && blueprint.RequiresStaticTags.Length > 0)
				{
					IEditableDatList craftingTagsList = blueprintDict.AddList("RequiresStaticTags");
					foreach (CachingAssetRef tagRef in blueprint.RequiresStaticTags)
					{
						craftingTagsList.AddValue().SetAssetRefWithInlineComment(tagRef);
					}
				}

				if (blueprint.effectAssetRef.IsAssigned)
				{
					blueprintDict.AddValue("Effect").SetAssetRefWithInlineComment(blueprint.effectAssetRef);
				}
			}
		}

		private void ConvertSimpleItemRefAndAmount(CachingBcAssetRef itemRef, int amount, IEditableDatValue valueNode)
		{
			if (itemRef.IsReferenceTo(this))
			{
				if (amount > 1)
				{
					valueNode.SetString($"this x {amount}");
				}
				else
				{
					valueNode.SetString("this");
				}
			}
			else
			{
				valueNode.SetAssetRefWithInlineComment(itemRef);
				if (amount > 1)
				{
					valueNode.Value += $" x {amount}";
				}
			}
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Locale_Item
			// Localization for Item assets.
			CargoDeclaration en = builder.GetOrAddDeclaration("Locale_Item");
			en.Append("GUID", GUID); // PFK
			en.Append("Name", FriendlyName);
			en.Append("Description", itemDescription);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Item
			// Game data for Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Item");
			data.Append("GUID", GUID); // PFK

			data.Append("Actions", actions.Count);
			data.Append("Allow_Manual_Drop", allowManualDrop);
			data.Append("Amount", MaxAmount);
			data.Append("Blueprints", (object) blueprints.Count);
			data.Append("Can_Player_Equip", canPlayerEquip);
			data.Append("Can_Use_Underwater", canUseUnderwater);
			data.Append("Count_Max", countMax);
			data.Append("Count_Min", countMin);
			data.Append("Destroy_Item_Colliders", shouldDestroyItemColliders);
			data.Append("Equipable_Movement_Speed_Multiplier", equipableMovementSpeedMultiplier);
			data.Append("EquipableModelParent", EquipableModelParent);
			data.Append("EquipAudioClip", (object) equip);
			data.Append("Instantiated_Item_Name_Override", instantiatedItemName);
			data.Append("Left_Handed_Characters_Mirror_Equipable", shouldLeftHandedCharactersMirrorEquippedItem);
			data.Append("Pro", isPro);
			data.Append("Quality_Max", qualityMax);
			data.Append("Quality_Min", qualityMin);
			data.Append("Shared_Skin_Lookup_ID", sharedSkinLookupID);
			data.Append("Shared_Skin_Apply_Visuals", SharedSkinShouldApplyVisuals);
			data.Append("Should_Delete_At_Zero_Quality", shouldDeleteAtZeroQuality);
			data.Append("Should_Drop_On_Death", shouldDropOnDeath);
			data.Append("Size_Z", iconCameraOrthographicSize);
			data.Append("Size2_Z", econIconCameraOrthographicSize);
			data.Append("Rarity", rarity);
			data.Append("Size_X", size_x);
			data.Append("Size_Y", size_y);
			data.Append("Slot", slot);
			data.Append("Useable", (object) useable);
			data.Append("Use_Auto_Icon_Measurements", isEligibleForAutomaticIconMeasurements);
#pragma warning disable
			data.Append("Use_Auto_Stat_Descriptions", isEligibleForAutoStatDescriptions);
#pragma warning restore

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Item_Blueprint
			// Child table for item blueprints. (Refer to Blueprint.cs)
			// Our composite key is formed from the item's GUID and blueprintIndex.
			//
			// Molt 2024-12-03: All of our columns (besides Item_GUID) are non-unique, so we use blueprintIndex as it's the least likely to change. Blueprints could use GUIDs in the future, which would be unique and should replace blueprintIndex entirely if/when that happens. E.g., we can simplify JOINs on Item_Blueprint_Supply and Item_Blueprint_Output.
			for (byte bpIndex = 0; bpIndex < blueprints.Count; bpIndex++)
			{
				CargoDeclaration bp = builder.AddDeclaration("Item_Blueprint");
				bp.Append("GUID", GUID); // FK (non-unique)
				bp.Append("blueprintIndex", bpIndex); // non-unique

#pragma warning disable
				bp.Append("Build", blueprints[bpIndex].build); // This property accepts GUIDs, but actually looks for the legacy ID still.
#pragma warning restore
				bp.Append("Level", blueprints[bpIndex].level);
				bp.Append("Map", blueprints[bpIndex].map);
				bp.Append("Outputs", blueprints[bpIndex].outputs.Length); // Get original value.
				bp.Append("Searchable", blueprints[bpIndex].canBeVisibleWhenSearchedWithoutRequiredItems);
				bp.Append("Skill", blueprints[bpIndex].GetLegacyBlueprintSkill());
				bp.Append("State_Transfer", blueprints[bpIndex].transferState);
				bp.Append("State_Transfer_Delete_Attachments", blueprints[bpIndex].withoutAttachments);
				bp.Append("Supplies", blueprints[bpIndex].supplies.Length); // Get original value.
#pragma warning disable
				bp.Append("Type", blueprints[bpIndex].type);
#pragma warning restore

				// https://unturned.wiki.gg/wiki/Special:CargoTables/Item_Blueprint_Supply
				// Child table for item blueprints' supplies. (Refer to BlueprintSupply.cs)
				// Our composite key is formed from the item's GUID, blueprintIndex, and supplyIndex.
				for (byte sIndex = 0; sIndex < blueprints[bpIndex].supplies.Length; sIndex++)
				{
					CargoDeclaration s = builder.AddDeclaration("Item_Blueprint_Supply");
					s.Append("GUID", GUID); // FK (non-unique)
					s.Append("blueprintIndex", bpIndex); // FK (non-unique)
					s.Append("supplyIndex", sIndex); // non-unique

					s.Append("ID", blueprints[bpIndex].supplies[sIndex].ItemRef.LegacyId);
					s.Append("ItemGUID", blueprints[bpIndex].supplies[sIndex].ItemRef.Guid);
					s.Append("Critical", blueprints[bpIndex].supplies[sIndex].isCritical);
					s.Append("Delete", blueprints[bpIndex].supplies[sIndex].ShouldConsume);
					s.Append("Amount", blueprints[bpIndex].supplies[sIndex].amount);
				}

				// https://unturned.wiki.gg/wiki/Special:CargoTables/Item_Blueprint_Output
				// Child table for item blueprints' outputs. (Refer to BlueprintOutput.cs)
				// Our composite key is formed from the item's GUID, blueprintIndex, and outputIndex.
				for (byte oIndex = 0; oIndex < blueprints[bpIndex].outputs.Length; oIndex++)
				{
					CargoDeclaration o = builder.AddDeclaration("Item_Blueprint_Output");
					o.Append("GUID", GUID); // FK (non-unique)
					o.Append("blueprintIndex", bpIndex); // FK (non-unique)
					o.Append("outputIndex", oIndex); // non-unique

					o.Append("ID", blueprints[bpIndex].outputs[oIndex].ItemRef.LegacyId);
					o.Append("ItemGUID", blueprints[bpIndex].outputs[oIndex].ItemRef.Guid);
					o.Append("Amount", blueprints[bpIndex].outputs[oIndex].amount);
					o.Append("Origin", blueprints[bpIndex].outputs[oIndex].origin);
				}
			}
		}

		protected virtual AudioReference GetDefaultInventoryAudio()
		{
			if (size_x < 2 && size_y < 2)
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/LightGrab.asset");
			}
			else
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/RoughGrab.asset");
			}
		}

		protected void ValidateEquipableHasAnimation(string name)
		{
			if (_animations != null)
			{
				foreach (AnimationClip clip in _animations)
				{
					if (clip != null && string.Equals(clip.name, name, System.StringComparison.Ordinal))
					{
						return;
					}
				}
			}

			ReportAssetError($"Equipable missing \"{name}\" animation in Animations.prefab");
		}

		//////////////////////////////////////////////////////////////////////////////////
		/// sortOrder values for description lines.
		/// Difference in value greater than 100 creates an empty line.
		//////////////////////////////////////////////////////////////////////////////////
		internal const int DescSort_RarityAndType = 0;
		internal const int DescSort_LoreText = 200;
		internal const int DescSort_QualityAndAmount = 400;
		internal const int DescSort_Important = 2000;
		internal const int DescSort_ItemStat = 10000;
		internal const int DescSort_ClothingStat = DescSort_ItemStat;
		internal const int DescSort_ConsumeableStat = DescSort_ItemStat;
		internal const int DescSort_GunStat = DescSort_ItemStat;
		internal const int DescSort_GunAttachmentStat = DescSort_ItemStat;
		internal const int DescSort_MeleeStat = DescSort_ItemStat;
		internal const int DescSort_RefillStat = DescSort_ItemStat;
		/// <summary>
		/// Properties common to Gun and Melee.
		/// </summary>
		internal const int DescSort_Weapon_NonExplosive_Common = DescSort_ItemStat;
		internal const int DescSort_TrapKeyword = 10001;
		internal const int DescSort_TrapStat = 10002;
		internal const int DescSort_FarmableText = 15000;
		/// <summary>
		/// Properties common to Barricade and Structure.
		/// </summary>
		internal const int DescSort_BuildableCommon = 20000;
		internal const int DescSort_CraftingTags = 25000;
		internal const int DescSort_ExplosiveBulletDamage = 30000;
		internal const int DescSort_ExplosiveChargeDamage = 30000;
		internal const int DescSort_ExplosiveTrapDamage = 30000;
		/// <summary>
		/// Properties common to Gun, Consumable, and Throwable.
		/// </summary>
		internal const int DescSort_Weapon_Explosive_RangeAndDamage = 30000;
		/// <summary>
		/// Properties common to Gun and Melee.
		/// </summary>
		internal const int DescSort_Weapon_NonExplosive_PlayerDamage = 30000;
		/// <summary>
		/// Properties common to Gun and Melee.
		/// </summary>
		internal const int DescSort_Weapon_NonExplosive_ZombieDamage = 31000;
		/// <summary>
		/// Properties common to Gun and Melee.
		/// </summary>
		internal const int DescSort_Weapon_NonExplosive_AnimalDamage = 32000;
		/// <summary>
		/// Properties common to Gun and Melee.
		/// </summary>
		internal const int DescSort_Weapon_NonExplosive_OtherDamage = 33000;

		internal const int DescSort_Beneficial = -1;
		internal const int DescSort_Detrimental = 1;

		protected int DescSort_HigherIsBeneficial(float value)
		{
			return value > 1.0f ? DescSort_Beneficial : DescSort_Detrimental;
		}

		protected int DescSort_LowerIsBeneficial(float value)
		{
			return value < 1.0f ? DescSort_Beneficial : DescSort_Detrimental;
		}

		private static List<BlueprintSupply> tempBlueprintSupplies = new List<BlueprintSupply>();
	}
}
