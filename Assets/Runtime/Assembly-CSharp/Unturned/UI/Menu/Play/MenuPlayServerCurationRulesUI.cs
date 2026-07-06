////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class MenuPlayServerCurationRulesUI : SleekFullscreenBox
	{
		public Local localization;
		public Bundle icons;
		public bool active;

		public void open(ServerCurationItem item)
		{
			if (active)
			{
				return;
			}

			active = true;
			BindAssetsReloaded();

			SetItem(item);

			AnimateIntoView();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			UnbindAssetsReloaded();
			UnbindDataChanged();

			AnimateOutOfView(0, 1);
		}

		private ISleekElement OnCreateListElement(ServerListCurationRule rule)
		{
			SleekServerCurationRule element = new SleekServerCurationRule(this, rule);
			return element;
		}

		private void OnClickedActivationButton(ISleekElement button)
		{
			if (item != null)
			{
				item.IsActive = !item.IsActive;
				SynchronizeActivationButton();
			}
		}

		private void OnClickedReloadButton(ISleekElement button)
		{
			if (item != null)
			{
				item.Reload();
			}
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			MenuPlayServersUI.serverCurationUI.open();
			close();
		}

		private void SynchronizeActivationButton()
		{
			if (item.IsActive)
			{
				activationButton.Text = localization.format("Deactivate_Label");
				activationButton.TooltipText = localization.format("Deactivate_Tooltip");
			}
			else
			{
				activationButton.Text = localization.format("Activate_Label");
				activationButton.TooltipText = localization.format("Activate_Tooltip");
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			UnbindAssetsReloaded();
		}

		private void BindAssetsReloaded()
		{
			if (!isAssetsReloadedBound)
			{
				isAssetsReloadedBound = true;
				Assets.OnNewAssetsFinishedLoading += OnNewAssetsFinishedLoading;
			}
		}

		private void UnbindAssetsReloaded()
		{
			if (isAssetsReloadedBound)
			{
				isAssetsReloadedBound = false;
				Assets.OnNewAssetsFinishedLoading -= OnNewAssetsFinishedLoading;
			}
		}

		private void UnbindDataChanged()
		{
			if (isDataChangedBound)
			{
				isDataChangedBound = false;
				item.OnDataChanged -= OnDataChanged;
			}
		}

		private void SetItem(ServerCurationItem newItem)
		{
			UnbindDataChanged();

			item = newItem;

			if (item == null)
			{
				nameLabel.Text = "null";
				nameLabel.IsVisible = true;
				errorLabel.IsVisible = false;
				originLabel.Text = "null";
				icon.IsVisible = false;
				webIcon.IsVisible = false;
				list.SetData(null);
				return;
			}
			else
			{
				item.OnDataChanged += OnDataChanged;
				OnDataChanged();
			}

			SynchronizeActivationButton();
		}

		private void OnNewAssetsFinishedLoading()
		{
			// If asset changed OnDataChanged should be called.
			ServerListCuration.Get().RefreshIfDirty();
		}

		private void OnDataChanged()
		{
			// Leaking implementation details into the UI! :|
			ServerCurationItem_Web webItem = item as ServerCurationItem_Web;

			string errorMessage;
			if (webItem != null && !Provider.allowWebRequests)
			{
				errorMessage = MenuPlayServersUI.serverCurationUI.localization.format("NoWebRequests");
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
				nameLabel.Text = MenuPlayServersUI.serverCurationUI.localization.format("WebItemPending");
			}
			else
			{
				nameLabel.Text = item.DisplayName;
			}
			originLabel.Text = item.DisplayOrigin;
			if (item.Icon != null)
			{
				icon.Texture = item.Icon;
				icon.IsVisible = true;
				webIcon.IsVisible = false;
			}
			else if (!string.IsNullOrEmpty(item.IconUrl))
			{
				webIcon.Refresh(item.IconUrl);
				webIcon.IsVisible = true;
				icon.IsVisible = false;
			}
			else
			{
				icon.IsVisible = false;
				webIcon.IsVisible = false;
			}

			list.SetData(item.GetRules());

			RefreshRulesTester();

			list.ForEachElement<SleekServerCurationRule>(element => element.SynchronizeBlockCount());
		}

		private void RefreshRulesTester()
		{
			if (item != null && testerVisibleToggle.Value)
			{
				rulesTester.TestRules(item.GetRules(), false);
			}
		}

		private void OnTesterVisibleToggled(ISleekToggle toggle, bool value)
		{
			list.SizeOffset_Y = value ? -200.0f : -110.0f;
			rulesTester.IsVisible = value;
			RefreshRulesTester();
		}

		public MenuPlayServerCurationRulesUI(MenuPlayServerCurationUI curationUI)
		{
			localization = curationUI.localization;

			active = false;

			ISleekBox headerBox = Glazier.Get().CreateBox();
			headerBox.SizeScale_X = 1.0f;
			headerBox.SizeOffset_Y = 40;
			headerBox.SizeOffset_X = -420.0f;

			ISleekButton reloadButton = Glazier.Get().CreateButton();
			reloadButton.SizeOffset_X = 200.0f;
			reloadButton.SizeOffset_Y = 40;
			reloadButton.PositionScale_X = 1.0f;
			reloadButton.PositionOffset_X = -410;
			reloadButton.Text = localization.format("Reload_Label");
			reloadButton.TooltipText = localization.format("Reload_Tooltip");
			reloadButton.OnClicked += OnClickedReloadButton;
			AddChild(reloadButton);

			activationButton = Glazier.Get().CreateButton();
			activationButton.SizeOffset_X = 200.0f;
			activationButton.SizeOffset_Y = 40;
			activationButton.PositionScale_X = 1.0f;
			activationButton.PositionOffset_X = -200.0f;
			activationButton.OnClicked += OnClickedActivationButton;
			AddChild(activationButton);

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 45;
			nameLabel.SizeScale_X = 1;
			nameLabel.SizeOffset_X = -45;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.SizeOffset_Y = 30;
			headerBox.AddChild(nameLabel);

			errorLabel = Glazier.Get().CreateLabel();
			errorLabel.PositionOffset_X = 45;
			errorLabel.SizeScale_X = 1;
			errorLabel.SizeOffset_X = -45;
			errorLabel.TextAlignment = TextAnchor.MiddleLeft;
			errorLabel.SizeOffset_Y = 30;
			errorLabel.TextColor = ESleekTint.BAD;
			headerBox.AddChild(errorLabel);

			originLabel = Glazier.Get().CreateLabel();
			originLabel.PositionOffset_X = 45;
			originLabel.PositionOffset_Y = 15;
			originLabel.SizeScale_X = 1;
			originLabel.SizeOffset_X = -45;
			originLabel.SizeOffset_Y = 30;
			originLabel.FontSize = ESleekFontSize.Small;
			originLabel.AllowRichText = true;
			originLabel.TextAlignment = TextAnchor.MiddleLeft;
			headerBox.AddChild(originLabel);

			icon = Glazier.Get().CreateImage();
			icon.PositionOffset_X = 4;
			icon.PositionOffset_Y = 4;
			icon.SizeOffset_X = 32;
			icon.SizeOffset_Y = 32;
			headerBox.AddChild(icon);

			webIcon = new SleekWebImage();
			webIcon.PositionOffset_X = 4;
			webIcon.PositionOffset_Y = 4;
			webIcon.SizeOffset_X = 32;
			webIcon.SizeOffset_Y = 32;
			headerBox.AddChild(webIcon);

			AddChild(headerBox);

			list = new SleekList<ServerListCurationRule>();
			list.PositionOffset_Y = 50;
			list.SizeOffset_Y = -120;
			list.SizeScale_X = 1;
			list.SizeScale_Y = 1;
			list.itemHeight = 40;
			list.onCreateElement = OnCreateListElement;
			AddChild(list);

			rulesTester = new SleekServerCurationRuleTester(localization);
			rulesTester.PositionOffset_Y = -140;
			rulesTester.PositionScale_Y = 1.0f;
			rulesTester.SizeScale_X = 1.0f;
			rulesTester.SizeOffset_Y = 80;
			rulesTester.OnInputChanged += RefreshRulesTester;
			rulesTester.IsVisible = false;
			AddChild(rulesTester);

			testerVisibleToggle = Glazier.Get().CreateToggle();
			testerVisibleToggle.PositionOffset_X = -245;
			testerVisibleToggle.PositionScale_X = 1.0f;
			testerVisibleToggle.PositionScale_Y = 1.0f;
			testerVisibleToggle.PositionOffset_Y = -45;
			testerVisibleToggle.AddLabel(localization.format("Test_Visible_Label"), ESleekSide.RIGHT);
			testerVisibleToggle.TooltipText = localization.format("Test_Visible_Tooltip");
			testerVisibleToggle.Value = false;
			testerVisibleToggle.OnValueChanged += OnTesterVisibleToggled;
			AddChild(testerVisibleToggle);

			SleekButtonIcon backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += OnClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			AddChild(backButton);
		}

		private ServerCurationItem item;
		private SleekList<ServerListCurationRule> list;
		private bool isAssetsReloadedBound;
		private bool isDataChangedBound;

		private ISleekLabel nameLabel;
		private ISleekLabel errorLabel;
		private ISleekLabel originLabel;
		private ISleekImage icon;
		private SleekWebImage webIcon;
		private ISleekButton activationButton;
		private SleekServerCurationRuleTester rulesTester;
		private ISleekToggle testerVisibleToggle;
	}
}
