////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Entry in the MenuPlayServerCurationUI list.
	/// </summary>
	public class SleekServerCurationItem : SleekWrapper
	{
		internal event System.Action<ServerCurationItem> OnClickedItem;
		internal event System.Action<ServerCurationItem> OnDeletedItem;
		internal event System.Action<ServerCurationItem, int> OnMovedItem;

		public void SynchronizeBlockCount()
		{
			int latestBlockedServerCount = item.LatestBlockedServerCount;
			if (latestBlockedServerCount > 0)
			{
				blockCountLabel.Text = localization.format("BlockCount", latestBlockedServerCount);
				blockCountLabel.IsVisible = true;
			}
			else
			{
				blockCountLabel.IsVisible = false;
			}
		}

		private void OnActiveToggled(ISleekToggle toggle, bool value)
		{
			item.IsActive = value;
			RefreshIsActive();
		}

		private void OnClickedButton(ISleekElement button)
		{
			OnClickedItem?.Invoke(item);
		}

		private void OnClickedRemoveButton(ISleekElement button)
		{
			OnDeletedItem?.Invoke(item);
		}

		private void OnClickedMoveUpButton(ISleekElement button)
		{
			OnMovedItem?.Invoke(item, -1);
		}

		private void OnClickedMoveDownButton(ISleekElement button)
		{
			OnMovedItem?.Invoke(item, 1);
		}

		private void RefreshIsActive()
		{
			if (item.IsActive)
			{
				nameLabel.TextColor = ESleekTint.FONT;
				originLabel.TextColor = ESleekTint.FONT;
				icon.TintColor = ESleekTint.NONE;
				webIcon.internalImage.TintColor = ESleekTint.NONE;
				toggle.TooltipText = localization.format("Deactivate_Tooltip");
			}
			else
			{
				nameLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
				originLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
				icon.TintColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
				webIcon.internalImage.TintColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
				toggle.TooltipText = localization.format("Activate_Tooltip");
			}
		}

		private void SynchronizeSortOrder()
		{
			bool canMoveUp = !item.IsAtFrontOfList;
			moveUpButton.isClickable = canMoveUp;
			moveUpButton.iconColor = new SleekColor(ESleekTint.FOREGROUND, canMoveUp ? 1.0f : 0.5f);

			bool canMoveDown = !item.IsAtBackOfList;
			moveDownButton.isClickable = canMoveDown;
			moveDownButton.iconColor = new SleekColor(ESleekTint.FOREGROUND, canMoveDown ? 1.0f : 0.5f);
		}

		private void SynchronizeDetails()
		{
			// Leaking implementation details into the UI! :|
			ServerCurationItem_Web webItem = item as ServerCurationItem_Web;

			string errorMessage;
			if (webItem != null && !Provider.allowWebRequests)
			{
				errorMessage = localization.format("NoWebRequests");
			}
			else
			{
				errorMessage = item.ErrorMessage;
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				errorLabel.Text = errorMessage;
				nameLabel.IsVisible = false;
				errorLabel.IsVisible = true;
			}
			else
			{
				nameLabel.IsVisible = true;
				errorLabel.IsVisible = false;
			}

			if (webItem != null && webItem.isWaitingForResponse)
			{
				nameLabel.Text = localization.format("WebItemPending");
			}
			else
			{
				nameLabel.Text = item.DisplayName;
			}
			originLabel.Text = item.DisplayOrigin;

			if (item.Icon != null)
			{
				icon.IsVisible = true;
				icon.Texture = item.Icon;
				webIcon.IsVisible = false;
			}
			else if (!string.IsNullOrEmpty(item.IconUrl))
			{
				icon.IsVisible = false;
				webIcon.Refresh(item.IconUrl);
				webIcon.IsVisible = true;
			}
			else
			{
				icon.IsVisible = false;
				webIcon.IsVisible = false;
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			item.OnSortOrderChanged -= SynchronizeSortOrder;
			item.OnDataChanged -= SynchronizeDetails;
		}

		internal SleekServerCurationItem(Local localization, IconsBundle icons, ServerCurationItem item) : base()
		{
			this.localization = localization;
			this.item = item;
			item.OnSortOrderChanged += SynchronizeSortOrder;
			item.OnDataChanged += SynchronizeDetails;

			toggle = Glazier.Get().CreateToggle();
			toggle.SizeOffset_X = 40;
			toggle.SizeOffset_Y = 40;
			toggle.Value = item.IsActive;
			toggle.OnValueChanged += OnActiveToggled;
			AddChild(toggle);

			button = Glazier.Get().CreateButton();
			button.PositionOffset_X = 40;
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			button.OnClicked += OnClickedButton;

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 45;
			nameLabel.SizeScale_X = 1;
			nameLabel.SizeOffset_X = -45;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.SizeOffset_Y = 30;
			button.AddChild(nameLabel);

			errorLabel = Glazier.Get().CreateLabel();
			errorLabel.PositionOffset_X = 45;
			errorLabel.SizeScale_X = 1;
			errorLabel.SizeOffset_X = -45;
			errorLabel.TextAlignment = TextAnchor.MiddleLeft;
			errorLabel.SizeOffset_Y = 30;
			errorLabel.TextColor = ESleekTint.BAD;
			button.AddChild(errorLabel);

			originLabel = Glazier.Get().CreateLabel();
			originLabel.PositionOffset_X = 45;
			originLabel.PositionOffset_Y = 15;
			originLabel.SizeScale_X = 1;
			originLabel.SizeOffset_X = -45;
			originLabel.SizeOffset_Y = 30;
			originLabel.FontSize = ESleekFontSize.Small;
			originLabel.TextAlignment = TextAnchor.MiddleLeft;
			button.AddChild(originLabel);

			icon = Glazier.Get().CreateImage();
			icon.PositionOffset_X = 4;
			icon.PositionOffset_Y = 4;
			icon.SizeOffset_X = 32;
			icon.SizeOffset_Y = 32;
			icon.IsVisible = false;
			button.AddChild(icon);

			webIcon = new SleekWebImage();
			webIcon.PositionOffset_X = 4;
			webIcon.PositionOffset_Y = 4;
			webIcon.SizeOffset_X = 32;
			webIcon.SizeOffset_Y = 32;
			webIcon.IsVisible = false;
			button.AddChild(webIcon);

			blockCountLabel = Glazier.Get().CreateLabel();
			blockCountLabel.PositionOffset_X = 5;
			blockCountLabel.PositionOffset_Y = 15;
			blockCountLabel.SizeScale_X = 1;
			blockCountLabel.SizeOffset_X = -10;
			blockCountLabel.SizeOffset_Y = 30;
			blockCountLabel.FontSize = ESleekFontSize.Small;
			blockCountLabel.TextAlignment = TextAnchor.MiddleRight;
			button.AddChild(blockCountLabel);

			if (item.IsDeletable)
			{
				button.SizeOffset_X = -160;

				SleekButtonIcon removeButton = new SleekButtonIcon(icons.load<Texture2D>("DeletePreset"), 20);
				removeButton.PositionScale_X = 1.0f;
				removeButton.PositionOffset_X = -120;
				removeButton.SizeOffset_X = 40;
				removeButton.SizeScale_Y = 1;
				removeButton.iconPositionOffset = 10;
				removeButton.iconColor = ESleekTint.FOREGROUND;
				removeButton.onClickedButton += OnClickedRemoveButton;
				removeButton.tooltip = localization.format("Remove_Tooltip");
				AddChild(removeButton);
			}
			else
			{
				button.SizeOffset_X = -120;
			}

			moveUpButton = new SleekButtonIcon(icons.load<Texture2D>("MoveCurationItemUp"), 20);
			moveUpButton.PositionScale_X = 1.0f;
			moveUpButton.PositionOffset_X = -80;
			moveUpButton.SizeOffset_X = 40;
			moveUpButton.SizeScale_Y = 1;
			moveUpButton.iconPositionOffset = 10;
			moveUpButton.iconColor = ESleekTint.FOREGROUND;
			moveUpButton.onClickedButton += OnClickedMoveUpButton;
			moveUpButton.tooltip = localization.format("MoveUp_Tooltip");
			AddChild(moveUpButton);

			moveDownButton = new SleekButtonIcon(icons.load<Texture2D>("MoveCurationItemDown"), 20);
			moveDownButton.PositionScale_X = 1.0f;
			moveDownButton.PositionOffset_X = -40;
			moveDownButton.SizeOffset_X = 40;
			moveDownButton.SizeScale_Y = 1;
			moveDownButton.iconPositionOffset = 10;
			moveDownButton.iconColor = ESleekTint.FOREGROUND;
			moveDownButton.onClickedButton += OnClickedMoveDownButton;
			moveDownButton.tooltip = localization.format("MoveDown_Tooltip");
			AddChild(moveDownButton);

			SynchronizeBlockCount();

			SynchronizeSortOrder();
			SynchronizeDetails();

			RefreshIsActive();

			AddChild(button);
		}

		private Local localization;
		private ServerCurationItem item;

		private ISleekToggle toggle;
		private ISleekButton button;
		private ISleekImage icon;
		private SleekWebImage webIcon;
		private SleekButtonIcon moveUpButton;
		private SleekButtonIcon moveDownButton;
		private ISleekLabel nameLabel;
		private ISleekLabel errorLabel;
		private ISleekLabel originLabel;
		private ISleekLabel blockCountLabel;
	}
}
