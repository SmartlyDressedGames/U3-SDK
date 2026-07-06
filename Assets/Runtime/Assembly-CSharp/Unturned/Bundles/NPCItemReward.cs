////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCItemReward : INPCReward
	{
		public CachingBcAssetRef ItemAssetRef
		{
			get => _itemAssetRef;
		}
		private CachingBcAssetRef _itemAssetRef;

		public byte amount
		{
			get;
			protected set;
		}

		public bool shouldAutoEquip
		{
			get;
			protected set;
		}

		public int sight
		{
			get;
			protected set;
		}

		public int tactical
		{
			get;
			protected set;
		}

		public int grip
		{
			get;
			protected set;
		}

		public int barrel
		{
			get;
			protected set;
		}

		public int magazine
		{
			get;
			protected set;
		}

		public int ammo
		{
			get;
			protected set;
		}

		public EItemOrigin origin
		{
			get;
			protected set;
		}

		public ItemAsset GetItemAsset()
		{
			return _itemAssetRef.Get<ItemAsset>();
		}

		public override void GrantReward(Player player)
		{
			ItemAsset asset = GetItemAsset();
			if (asset == null)
				return;

			// At one point there was a bug because the created Item was shared
			// between each iteration, rather than unique instances.
			for (byte number = 0; number < amount; number++)
			{
				Item item;
				if (sight > -1 || tactical > -1 || grip > -1 || barrel > -1 || magazine > -1 || ammo > -1)
				{
					ItemGunAsset gunAsset = asset as ItemGunAsset;
					if (gunAsset != null)
					{
						ushort sightId = sight > -1 ? MathfEx.ClampToUShort(sight) : gunAsset.sightID;
						ushort tacticalId = tactical > -1 ? MathfEx.ClampToUShort(tactical) : gunAsset.tacticalID;
						ushort gripId = grip > -1 ? MathfEx.ClampToUShort(grip) : gunAsset.gripID;
						ushort barrelId = barrel > -1 ? MathfEx.ClampToUShort(barrel) : gunAsset.barrelID;
						ushort magazineId = magazine > -1 ? MathfEx.ClampToUShort(magazine) : gunAsset.GetDefaultMagazineLegacyId();
						byte spawnAmmo = ammo > -1 ? MathfEx.ClampToByte(ammo) : gunAsset.ammoMax;
						byte[] state = gunAsset.getState(sightId, tacticalId, gripId, barrelId, magazineId, spawnAmmo);
						item = new Item(asset.id, 1, 100, state);
					}
					else
					{
						// Gun properties were specified for non-gun item, but this may have happened if
						// the asset was created in third-party tools which were setting them to zero
						// for all item rewards regardless. (public issue #3898)
						item = new Item(asset.id, origin);
					}
				}
				else
				{
					item = new Item(asset.id, origin);
				}

				player.inventory.forceAddItem(item, shouldAutoEquip, false);
			}
		}

		public override string formatReward(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Item");
			}

			string format;

			ItemAsset asset = GetItemAsset();
			if (asset != null)
			{
				format = "<color=" + Palette.hex(ItemTool.getRarityColorUI(asset.rarity)) + ">" + asset.itemName + "</color>";
			}
			else
			{
				format = "?";
			}

			return Local.FormatText(text, amount, format);
		}

		public override ISleekElement createUI(Player player)
		{
			string text = formatReward(player);

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			ItemAsset itemAsset = GetItemAsset();
			if (itemAsset == null)
			{
				return null;
			}

			ISleekBox rewardBox = Glazier.Get().CreateBox();

			if (itemAsset.size_y == 1)
			{
				rewardBox.SizeOffset_Y = (itemAsset.size_y * 50) + 10;
			}
			else
			{
				rewardBox.SizeOffset_Y = (itemAsset.size_y * 25) + 10;
			}

			rewardBox.SizeScale_X = 1;

			SleekItemIcon itemImage = new SleekItemIcon();
			itemImage.PositionOffset_X = 5;
			itemImage.PositionOffset_Y = 5;

			if (itemAsset.size_y == 1)
			{
				itemImage.SizeOffset_X = itemAsset.size_x * 50;
				itemImage.SizeOffset_Y = itemAsset.size_y * 50;
			}
			else
			{
				itemImage.SizeOffset_X = itemAsset.size_x * 25;
				itemImage.SizeOffset_Y = itemAsset.size_y * 25;
			}

			rewardBox.AddChild(itemImage);

			itemImage.Refresh(itemAsset.id, 100, itemAsset.getState(false), itemAsset, Mathf.RoundToInt(itemImage.SizeOffset_X), Mathf.RoundToInt(itemImage.SizeOffset_Y));

			ISleekLabel rewardLabel = Glazier.Get().CreateLabel();
			rewardLabel.PositionOffset_X = 10 + itemImage.SizeOffset_X;
			rewardLabel.SizeOffset_X = -15 - itemImage.SizeOffset_X;
			rewardLabel.SizeScale_X = 1;
			rewardLabel.SizeScale_Y = 1;
			rewardLabel.TextAlignment = TextAnchor.MiddleLeft;
			rewardLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			rewardLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			rewardLabel.AllowRichText = true;
			rewardLabel.Text = text;
			rewardBox.AddChild(rewardLabel);

			return rewardBox;
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (!p.data.TryParseBcAssetRef("ID", EAssetType.ITEM, out _itemAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseUInt8("Amount", out byte _amount))
			{
				amount = _amount;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Amount");
			}

			shouldAutoEquip = p.data.ParseBool("Auto_Equip");
			origin = p.data.ParseEnum("Origin", EItemOrigin.CRAFT);

			sight = p.data.ParseInt32("Sight", defaultValue: -1);
			tactical = p.data.ParseInt32("Tactical", defaultValue: -1);
			grip = p.data.ParseInt32("Grip", defaultValue: -1);
			barrel = p.data.ParseInt32("Barrel", defaultValue: -1);
			magazine = p.data.ParseInt32("Magazine", defaultValue: -1);
			ammo = p.data.ParseInt32("Ammo", defaultValue: -1);
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.ITEM, out _itemAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseUInt8(p.legacyPrefix + "_Amount", out byte _amount))
			{
				amount = _amount;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Amount");
			}

			shouldAutoEquip = p.data.ParseBool(p.legacyPrefix + "_Auto_Equip");
			origin = p.data.ParseEnum(p.legacyPrefix + "_Origin", EItemOrigin.CRAFT);

			sight = p.data.ParseInt32(p.legacyPrefix + "_Sight", defaultValue: -1);
			tactical = p.data.ParseInt32(p.legacyPrefix + "_Tactical", defaultValue: -1);
			grip = p.data.ParseInt32(p.legacyPrefix + "_Grip", defaultValue: -1);
			barrel = p.data.ParseInt32(p.legacyPrefix + "_Barrel", defaultValue: -1);
			magazine = p.data.ParseInt32(p.legacyPrefix + "_Magazine", defaultValue: -1);
			ammo = p.data.ParseInt32(p.legacyPrefix + "_Ammo", defaultValue: -1);
		}

		public NPCItemReward() { }

		[System.Obsolete]
		public NPCItemReward(System.Guid newItemGuid, ushort newID, byte newAmount, bool newShouldAutoEquip, int newSight, int newTactical, int newGrip, int newBarrel, int newMagazine, int newAmmo, EItemOrigin origin, string newText) : base(newText)
		{
			_itemAssetRef = new CachingBcAssetRef(newItemGuid, EAssetType.ITEM, newID);
			amount = newAmount;
			shouldAutoEquip = newShouldAutoEquip;

			sight = newSight;
			tactical = newTactical;
			grip = newGrip;
			barrel = newBarrel;
			magazine = newMagazine;
			ammo = newAmmo;
			this.origin = origin;
		}

		[System.Obsolete]
		public System.Guid itemGuid => ItemAssetRef.Guid;

		[System.Obsolete]
		public ushort id => ItemAssetRef.LegacyId;
	}
}
