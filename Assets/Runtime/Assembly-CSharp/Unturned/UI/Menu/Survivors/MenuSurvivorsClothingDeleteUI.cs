////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public enum EDeleteMode
	{
		DELETE, // Default
		SALVAGE, // Scrapping item
		TAG_TOOL_ADD, // Stat counter or ragdoll effect
		TAG_TOOL_REMOVE, // Removing stat counter or ragdoll effect
	}

	public class MenuSurvivorsClothingDeleteUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static int item;
		private static ulong instance;
		private static ushort quantity;
		private static EDeleteMode mode;
		private static ulong instigator;

		private static ISleekConstraintFrame inventory;
		private static ISleekBox deleteBox;
		private static ISleekLabel intentLabel;
		private static ISleekLabel warningLabel;
		private static ISleekLabel confirmLabel;
		private static ISleekField confirmField;
		private static ISleekButton yesButton;
		private static ISleekButton noButton;

		private static ISleekLabel quantityLabel;
		private static ISleekUInt16Field quantityField;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(0, 1);
		}

		/// <summary>
		/// Note: inventory service does not support exchanging multiple items simultaneously.
		/// </summary>
		public static void salvageItem(int itemID, ulong instanceID)
		{
			int targetItem = Provider.provider.economyService.getScrapExchangeItem(itemID);
			if (targetItem < 1)
			{
				UnturnedLog.warn("Unable to find exchange target for salvaging itemdef {0} ({1})", itemID, instanceID);
				return;
			}

			Provider.provider.economyService.exchangeInventory(targetItem, new List<EconExchangePair>() { new EconExchangePair(instanceID, 1) });
		}

		public static void applyTagTool(int itemID, ulong targetID, ulong toolID)
		{
			List<EconExchangePair> destroy = new List<EconExchangePair>()
			{
				new EconExchangePair(targetID, 1),
				new EconExchangePair(toolID, 1)
			};
			Provider.provider.economyService.exchangeInventory(itemID, destroy);
		}

		public static void viewItem(int newItem, ulong newInstance, ushort newQuantity, EDeleteMode newMode, ulong newInstigator)
		{
			item = newItem;
			instance = newInstance;
			quantity = newQuantity;
			mode = newMode;
			instigator = newInstigator;

			deleteBox.SizeOffset_Y = 130;

			yesButton.TooltipText = localization.format(mode == EDeleteMode.SALVAGE ? "Yes_Salvage_Tooltip" : (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE) ? "Yes_Tag_Tool_Tooltip" : "Yes_Delete_Tooltip");
			if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
			{
				int instigatorItem = Provider.provider.economyService.getInventoryItem(instigator);
				intentLabel.Text = localization.format("Intent_Tag_Tool", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(instigatorItem)) + ">" + Provider.provider.economyService.getInventoryName(instigatorItem) + "</color>", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryName(item) + "</color>");
			}
			else
			{
				intentLabel.Text = localization.format(mode == EDeleteMode.SALVAGE ? "Intent_Salvage" : "Intent_Delete", "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(item)) + ">" + Provider.provider.economyService.getInventoryName(item) + "</color>");
			}

			confirmLabel.Text = localization.format("Confirm", localization.format(mode == EDeleteMode.SALVAGE ? "Salvage" : "Delete"));
			confirmLabel.IsVisible = mode != EDeleteMode.TAG_TOOL_ADD && mode != EDeleteMode.TAG_TOOL_REMOVE;
			confirmField.PlaceholderText = localization.format(mode == EDeleteMode.SALVAGE ? "Salvage" : "Delete");
			confirmField.Text = string.Empty;
			confirmField.IsVisible = mode != EDeleteMode.TAG_TOOL_ADD && mode != EDeleteMode.TAG_TOOL_REMOVE;

			if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
			{
				yesButton.PositionOffset_X = -65;
				yesButton.PositionScale_X = 0.5f;
				noButton.PositionOffset_X = 5;
				noButton.PositionScale_X = 0.5f;
			}
			else
			{
				yesButton.PositionOffset_X = -135;
				yesButton.PositionScale_X = 1;
				noButton.PositionOffset_X = -65;
				noButton.PositionScale_X = 1;
			}

			if (mode == EDeleteMode.TAG_TOOL_ADD)
			{
				warningLabel.Text = localization.format("Warning_UndoableWithTool");
			}
			else
			{
				warningLabel.Text = localization.format("Warning");
			}

			quantityField.Value = 1;
			quantityField.MaxValue = quantity;
			if (mode == EDeleteMode.DELETE && quantity > 1)
			{
				quantityLabel.Text = localization.format("Quantity", quantity);
				deleteBox.SizeOffset_Y += 50;
				quantityLabel.IsVisible = true;
				quantityField.IsVisible = true;
			}
			else
			{
				quantityLabel.IsVisible = false;
				quantityField.IsVisible = false;
			}
		}

		private static void onClickedYesButton(ISleekElement button)
		{
			if (mode == EDeleteMode.SALVAGE)
			{
				if (confirmField.Text != localization.format("Salvage"))
				{
					return;
				}

				salvageItem(item, instance);
			}
			else if (mode == EDeleteMode.DELETE)
			{
				if (confirmField.Text != localization.format("Delete"))
				{
					return;
				}

				Provider.provider.economyService.consumeItem(instance, MathfEx.Min(quantityField.Value, quantity));
			}

			MenuSurvivorsClothingUI.open();
			close();

			if (mode == EDeleteMode.TAG_TOOL_ADD || mode == EDeleteMode.TAG_TOOL_REMOVE)
			{
				MenuSurvivorsClothingUI.prepareForCraftResult();
				applyTagTool(item, instance, instigator);
			}
		}

		private static void onClickedNoButton(ISleekElement button)
		{
			MenuSurvivorsClothingItemUI.open();
			close();
		}

		public MenuSurvivorsClothingDeleteUI()
		{
			localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothingDelete.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			inventory = Glazier.Get().CreateConstraintFrame();
			inventory.PositionScale_X = 0.5f;
			inventory.PositionOffset_Y = 10;
			inventory.SizeScale_X = 0.5f;
			inventory.SizeScale_Y = 1;
			inventory.SizeOffset_Y = -20;
			inventory.Constraint = ESleekConstraint.FitInParent;
			container.AddChild(inventory);

			deleteBox = Glazier.Get().CreateBox();
			deleteBox.PositionOffset_Y = -65;
			deleteBox.PositionScale_Y = 0.5f;
			deleteBox.SizeOffset_Y = 130;
			deleteBox.SizeScale_X = 1;
			inventory.AddChild(deleteBox);

			intentLabel = Glazier.Get().CreateLabel();
			intentLabel.AllowRichText = true;
			intentLabel.PositionOffset_X = 5;
			intentLabel.PositionOffset_Y = 0;
			intentLabel.SizeOffset_X = -10;
			intentLabel.SizeOffset_Y = 30;
			intentLabel.SizeScale_X = 1;
			intentLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			intentLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			deleteBox.AddChild(intentLabel);

			warningLabel = Glazier.Get().CreateLabel();
			warningLabel.PositionOffset_X = 5;
			warningLabel.PositionOffset_Y = 20;
			warningLabel.SizeOffset_X = -10;
			warningLabel.SizeOffset_Y = 30;
			warningLabel.SizeScale_X = 1;
			deleteBox.AddChild(warningLabel);

			confirmLabel = Glazier.Get().CreateLabel();
			confirmLabel.PositionOffset_X = 5;
			confirmLabel.PositionOffset_Y = 40;
			confirmLabel.SizeOffset_X = -10;
			confirmLabel.SizeOffset_Y = 30;
			confirmLabel.SizeScale_X = 1;
			deleteBox.AddChild(confirmLabel);

			confirmField = Glazier.Get().CreateStringField();
			confirmField.PositionOffset_X = 5;
			confirmField.PositionOffset_Y = 75;
			confirmField.SizeOffset_X = -150;
			confirmField.SizeOffset_Y = 50;
			confirmField.SizeScale_X = 1;
			confirmField.FontSize = ESleekFontSize.Medium;
			deleteBox.AddChild(confirmField);

			yesButton = Glazier.Get().CreateButton();
			yesButton.PositionOffset_X = -135;
			yesButton.PositionOffset_Y = 75;
			yesButton.PositionScale_X = 1;
			yesButton.SizeOffset_X = 60;
			yesButton.SizeOffset_Y = 50;
			yesButton.FontSize = ESleekFontSize.Medium;
			yesButton.Text = localization.format("Yes");
			yesButton.OnClicked += onClickedYesButton;
			deleteBox.AddChild(yesButton);

			noButton = Glazier.Get().CreateButton();
			noButton.PositionOffset_X = -65;
			noButton.PositionOffset_Y = 75;
			noButton.PositionScale_X = 1;
			noButton.SizeOffset_X = 60;
			noButton.SizeOffset_Y = 50;
			noButton.FontSize = ESleekFontSize.Medium;
			noButton.Text = localization.format("No");
			noButton.TooltipText = localization.format("No_Tooltip");
			noButton.OnClicked += onClickedNoButton;
			deleteBox.AddChild(noButton);

			quantityLabel = Glazier.Get().CreateLabel();
			quantityLabel.PositionOffset_X = 5;
			quantityLabel.PositionOffset_Y = -35;
			quantityLabel.PositionScale_Y = 1.0f;
			quantityLabel.SizeOffset_X = -10;
			quantityLabel.SizeOffset_Y = 30;
			quantityLabel.SizeScale_X = 0.75f;
			quantityLabel.TextAlignment = UnityEngine.TextAnchor.MiddleRight;
			quantityLabel.IsVisible = false;
			deleteBox.AddChild(quantityLabel);

			quantityField = Glazier.Get().CreateUInt16Field();
			quantityField.PositionOffset_X = 5;
			quantityField.PositionOffset_Y = -35;
			quantityField.PositionScale_X = 0.75f;
			quantityField.PositionScale_Y = 1.0f;
			quantityField.SizeOffset_X = -10;
			quantityField.SizeOffset_Y = 30;
			quantityField.SizeScale_X = 0.25f;
			quantityField.Value = 1;
			quantityField.MinValue = 1;
			quantityField.IsVisible = false;
			deleteBox.AddChild(quantityField);
		}
	}
}
