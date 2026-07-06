////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCItemCondition : NPCLogicCondition
	{
		private static InventorySearchQualityAscendingComparator qualityAscendingComparator = new InventorySearchQualityAscendingComparator();

		public CachingBcAssetRef ItemAssetRef
		{
			get => _itemAssetRef;
		}
		private CachingBcAssetRef _itemAssetRef;

		public ushort amount
		{
			get;
			protected set;
		}

		public ItemAsset GetItemAsset()
		{
			return _itemAssetRef.Get<ItemAsset>();
		}

		public override bool isConditionMet(Player player)
		{
			ItemAsset asset = GetItemAsset();
			if (asset == null)
			{
				return false;
			}

			using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
			{
				player.inventory.FindItemsByAsset(scope.PooledResults, asset, false, true);

				ushort total = 0;
				foreach (PlayerInventorySearchResultV2 searchResult in scope.PooledResults)
				{
					total += searchResult.Jar.item.amount;
				}

				return doesLogicPass(total, amount);
			}
		}

		public override void ApplyCondition(Player player)
		{
			if (!shouldReset)
			{
				return;
			}

			ItemAsset asset = GetItemAsset();
			if (asset == null)
			{
				return;
			}

			using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
			{
				player.inventory.FindItemsByAsset(scope.PooledResults, asset, false, true);
				scope.PooledResults.Sort(qualityAscendingComparator);

				uint total = amount;
				foreach (PlayerInventorySearchResultV2 searchResult in scope.PooledResults)
				{
					uint amountDeleted = searchResult.DeleteAmount(player, total);
					total -= amountDeleted;
					if (total == 0)
					{
						break;
					}
				}
			}
		}

		public override string formatCondition(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = PlayerNPCQuestUI.localization.format("Condition_Item");
			}

			ItemAsset asset = GetItemAsset();
			if (asset != null)
			{
				string format = "<color=" + Palette.hex(ItemTool.getRarityColorUI(asset.rarity)) + ">" + asset.itemName + "</color>";

				using (ScopedPlayerInventorySearchResultPool scope = new ScopedPlayerInventorySearchResultPool())
				{
					player.inventory.FindItemsByAsset(scope.PooledResults, asset, false, true);

					int total = 0;
					foreach (PlayerInventorySearchResultV2 searchResult in scope.PooledResults)
					{
						total += searchResult.Jar.item.amount;
					}

					return Local.FormatText(text, total, amount, format);
				}
			}
			else
			{
				return Local.FormatText(text, 0, amount, "?");
			}
		}

		public override ISleekElement createUI(Player player, Texture2D icon)
		{
			string text = formatCondition(player);

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			ItemAsset itemAsset = GetItemAsset();
			if (itemAsset == null)
			{
				return null;
			}

			ISleekBox conditionBox = Glazier.Get().CreateBox();

			if (itemAsset.size_y == 1)
			{
				conditionBox.SizeOffset_Y = (itemAsset.size_y * 50) + 10;
			}
			else
			{
				conditionBox.SizeOffset_Y = (itemAsset.size_y * 25) + 10;
			}

			conditionBox.SizeScale_X = 1;

			if (icon != null)
			{
				ISleekImage iconImage = Glazier.Get().CreateImage(icon);
				iconImage.PositionOffset_X = 5;
				iconImage.PositionOffset_Y = -10;
				iconImage.PositionScale_Y = 0.5f;
				iconImage.SizeOffset_X = 20;
				iconImage.SizeOffset_Y = 20;
				conditionBox.AddChild(iconImage);
			}

			SleekItemIcon itemImage = new SleekItemIcon();

			if (icon != null)
			{
				itemImage.PositionOffset_X = 30;
			}
			else
			{
				itemImage.PositionOffset_X = 5;
			}

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

			conditionBox.AddChild(itemImage);

			itemImage.Refresh(itemAsset.id, 100, itemAsset.getState(false), itemAsset, Mathf.RoundToInt(itemImage.SizeOffset_X), Mathf.RoundToInt(itemImage.SizeOffset_Y));

			ISleekLabel conditionLabel = Glazier.Get().CreateLabel();

			if (icon != null)
			{
				conditionLabel.PositionOffset_X = 35 + itemImage.SizeOffset_X;
				conditionLabel.SizeOffset_X = -40 - itemImage.SizeOffset_X;
			}
			else
			{
				conditionLabel.PositionOffset_X = 10 + itemImage.SizeOffset_X;
				conditionLabel.SizeOffset_X = -15 - itemImage.SizeOffset_X;
			}

			conditionLabel.SizeScale_X = 1;
			conditionLabel.SizeScale_Y = 1;
			conditionLabel.TextAlignment = TextAnchor.MiddleLeft;
			conditionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			conditionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			conditionLabel.AllowRichText = true;
			conditionLabel.Text = text;
			conditionBox.AddChild(conditionLabel);

			return conditionBox;
		}

		internal override void PopulateV2(in PopulateConditionParameters p)
		{
			base.PopulateV2(p);

			if (!p.data.TryParseBcAssetRef("ID", EAssetType.ITEM, out _itemAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseUInt16("Amount", out ushort _amount))
			{
				amount = _amount;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Amount");
			}

			if (shouldReset && logicType != ENPCLogicType.GREATER_THAN_OR_EQUAL_TO)
			{
				logicType = ENPCLogicType.GREATER_THAN_OR_EQUAL_TO;
				p.ReportError("Resetting item condition only compatible with >= comparison");
			}
		}

		internal override void PopulateLegacy(in PopulateConditionParameters p)
		{
			base.PopulateLegacy(p);

			if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.ITEM, out _itemAssetRef))
			{
				p.ReportRequiredOptionInvalid("ID");
			}

			if (p.data.TryParseUInt16(p.legacyPrefix + "_Amount", out ushort _amount))
			{
				amount = _amount;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Amount");
			}

			if (shouldReset && logicType != ENPCLogicType.GREATER_THAN_OR_EQUAL_TO)
			{
				logicType = ENPCLogicType.GREATER_THAN_OR_EQUAL_TO;
				p.ReportError("Resetting item condition only compatible with >= comparison");
			}
		}

		public NPCItemCondition() { }

		[System.Obsolete]
		public NPCItemCondition(System.Guid newItemGuid, ushort newID, ushort newAmount, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newLogicType, newText, newShouldReset)
		{
			amount = newAmount;
		}

		[System.Obsolete]
		public ushort id => ItemAssetRef.LegacyId;
	}
}
