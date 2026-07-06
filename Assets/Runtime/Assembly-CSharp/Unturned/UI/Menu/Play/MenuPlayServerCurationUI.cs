////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuPlayServerCurationUI : SleekFullscreenBox
	{
		public Local localization;
		public IconsBundle icons;
		public bool active;

		private SleekButtonIcon backButton;

		public void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			SynchronizeSortedItems();

			AnimateIntoView();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			AnimateOutOfView(0, 1);
		}

		private void SynchronizeSortedItems()
		{
			ServerListCuration curation = ServerListCuration.Get();
			curation.RefreshIfDirty();
			list.NotifyDataChanged();

			tutorialBox.IsVisible = curation.GetItems().Count < 1;
			RefreshRulesTester();

			list.ForEachElement<SleekServerCurationItem>(element => element.SynchronizeBlockCount());
		}

		private void OnUrlSubmitted(ISleekField field)
		{
			string input = field.Text;
			if (!WebUtils.ParseThirdPartyUrl(input, out string link, useLinkFiltering: false))
			{
				UnturnedLog.info($"Unable to parse curation URL \"{input}\"");
				return;
			}

			urlField.Text = string.Empty;

			const string fileDetailsString = "steamcommunity.com/sharedfiles/filedetails/?id=";
			int fileDetailsIndex = input.IndexOf(fileDetailsString);
			if (fileDetailsIndex != -1)
			{
				int idStartIndex = fileDetailsIndex + fileDetailsString.Length;
				int parameterDelimiterIndex = input.IndexOf('&', idStartIndex + 1);

				string idParameter;
				if (parameterDelimiterIndex > 0)
				{
					idParameter = input.Substring(idStartIndex, parameterDelimiterIndex - idStartIndex);
				}
				else
				{
					idParameter = input.Substring(idStartIndex);
				}

				if (!ulong.TryParse(idParameter, out ulong workshopFileId))
				{
					UnturnedLog.error($"Unable to parse ID parameter \"{idParameter}\" from workshop file URL \"{input}\"");
					return;
				}

				UnturnedLog.info($"Adding server curation workshop file ID {workshopFileId}");
				Provider.provider.workshopService.setSubscribed(workshopFileId, true);
				return;
			}

			ServerListCuration.Get().AddUrl(link, 0);
			SynchronizeSortedItems();
		}

		private void OnAddUrlButtonClicked(ISleekElement button)
		{
			OnUrlSubmitted(urlField);
		}

		private void OnClickedItem(ServerCurationItem item)
		{
			rulesUI.open(item);
			close();
		}

		private void OnDeletedItem(ServerCurationItem item)
		{
			item.Delete();
			list.NotifyDataChanged();
			RefreshRulesTester();
		}

		private void OnMovedItem(ServerCurationItem item, int direction)
		{
			ServerListCuration.Get().MoveItem(item, direction);
			list.NotifyDataChanged();
			RefreshRulesTester();
		}

		private ISleekElement OnCreateListElement(ServerCurationItem item)
		{
			SleekServerCurationItem element = new SleekServerCurationItem(localization, icons, item);
			element.OnClickedItem += OnClickedItem;
			element.OnDeletedItem += OnDeletedItem;
			element.OnMovedItem += OnMovedItem;
			return element;
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			MenuPlayUI.serverListUI.open(true);
			close();
		}

		private void OnChangedDefaultBehavior(SleekButtonState button, int value)
		{
			ServerListCuration.Get().DefaultBehavior = (EServerListCurationDefaultBehavior) value;
		}

		private void OnChangedDenyMode(SleekButtonState button, int value)
		{
			ServerListCuration.Get().DenyMode = (EServerListCurationDenyMode) value;
		}

		private void RefreshRulesTester()
		{
			if (testerVisibleToggle.Value)
			{
				rulesTester.TestRules(null, true);
			}
		}

		private void OnTesterVisibleToggled(ISleekToggle toggle, bool value)
		{
			list.SizeOffset_Y = value ? -260.0f : -170.0f;
			rulesTester.IsVisible = value;
			RefreshRulesTester();
		}

		public MenuPlayServerCurationUI(MenuPlayServersUI serverListUI)
		{
			localization = Localization.read("/Menu/Play/MenuPlayServerCuration.dat");
			icons = serverListUI.icons;

			active = false;

			rulesUI = new MenuPlayServerCurationRulesUI(this);
			rulesUI.PositionOffset_X = 10;
			rulesUI.PositionOffset_Y = 10;
			rulesUI.PositionScale_Y = 1;
			rulesUI.SizeOffset_X = -20;
			rulesUI.SizeOffset_Y = -20;
			rulesUI.SizeScale_X = 1;
			rulesUI.SizeScale_Y = 1;
			MenuUI.container.AddChild(rulesUI);

			ISleekBox headerBox = Glazier.Get().CreateBox();
			headerBox.SizeScale_X = 1.0f;
			headerBox.SizeOffset_Y = 60;
			AddChild(headerBox);

			ISleekLabel titleLabel = Glazier.Get().CreateLabel();
			titleLabel.SizeScale_X = 1;
			titleLabel.SizeOffset_Y = 40;
			titleLabel.Text = localization.format("Title");
			titleLabel.FontSize = ESleekFontSize.Large;
			headerBox.AddChild(titleLabel);

			ISleekLabel titleInfoLabel = Glazier.Get().CreateLabel();
			titleInfoLabel.PositionOffset_Y = 20;
			titleInfoLabel.SizeScale_X = 1;
			titleInfoLabel.SizeOffset_Y = 40;
			titleInfoLabel.Text = localization.format("TitleInfo");
			headerBox.AddChild(titleInfoLabel);

			urlField = Glazier.Get().CreateStringField();
			urlField.PositionOffset_Y = 70;
			urlField.SizeOffset_X = -200;
			urlField.SizeScale_X = 1.0f;
			urlField.SizeOffset_Y = 30;
			urlField.PlaceholderText = localization.format("URL_Placeholder");
			urlField.TooltipText = localization.format("URL_Tooltip");
			urlField.OnTextSubmitted += OnUrlSubmitted;
			AddChild(urlField);

			SleekButtonIcon addUrlButton = new SleekButtonIcon(icons.load<Texture2D>("NewPreset"), 20);
			addUrlButton.PositionOffset_Y = 70;
			addUrlButton.PositionOffset_X = -200;
			addUrlButton.PositionScale_X = 1.0f;
			addUrlButton.SizeOffset_X = 200;
			addUrlButton.SizeOffset_Y = 30;
			addUrlButton.text = localization.format("AddURLButton_Label");
			addUrlButton.tooltip = localization.format("AddURLButton_Tooltip");
			addUrlButton.onClickedButton += OnAddUrlButtonClicked;
			AddChild(addUrlButton);

			list = new SleekList<ServerCurationItem>();
			list.PositionOffset_Y = 110;
			list.SizeOffset_Y = -180;
			list.SizeScale_X = 1;
			list.SizeScale_Y = 1;
			list.itemHeight = 40;
			list.onCreateElement = OnCreateListElement;
			list.SetData(ServerListCuration.Get().GetItems());
			AddChild(list);

			tutorialBox = Glazier.Get().CreateBox();
			tutorialBox.SizeOffset_Y = 80;
			tutorialBox.SizeScale_X = 0.8f;
			tutorialBox.PositionScale_X = 0.1f;
			tutorialBox.PositionScale_Y = 0.5f;
			tutorialBox.PositionOffset_Y = -40;
			tutorialBox.Text = localization.format("Tutorial");
			tutorialBox.FontSize = ESleekFontSize.Medium;
			tutorialBox.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(tutorialBox);
			tutorialBox.IsVisible = false;

			defaultBehaviorButton = new SleekButtonState(
				new GUIContent(localization.format("DefaultBehavior_Show_Label", "DefaultBehavior_Show_Tooltip")),
				new GUIContent(localization.format("DefaultBehavior_Hide_Label", "DefaultBehavior_Hide_Tooltip")),
				new GUIContent(localization.format("DefaultBehavior_MoveToBottom_Label", "DefaultBehavior_MoveToBottom_Label"))
				);
			defaultBehaviorButton.PositionOffset_X = -650;
			defaultBehaviorButton.PositionOffset_Y = -50;
			defaultBehaviorButton.PositionScale_X = 1.0f;
			defaultBehaviorButton.PositionScale_Y = 1.0f;
			defaultBehaviorButton.SizeOffset_X = 200;
			defaultBehaviorButton.SizeOffset_Y = 25;
			defaultBehaviorButton.AddLabel(localization.format("DefaultBehavior_Label"), ESleekSide.RIGHT);
			defaultBehaviorButton.state = (int) ServerListCuration.Get().DefaultBehavior;
			defaultBehaviorButton.onSwappedState += OnChangedDefaultBehavior;
			AddChild(defaultBehaviorButton);

			denyModeButton = new SleekButtonState(
				new GUIContent(localization.format("DenyMode_Hide_Label", "DenyMode_Hide_Tooltip")),
				new GUIContent(localization.format("DenyMode_MoveToBottom_Label", "DenyMode_MoveToBottom_Label"))
				);
			denyModeButton.PositionOffset_X = -650;
			denyModeButton.PositionOffset_Y = -25;
			denyModeButton.PositionScale_X = 1.0f;
			denyModeButton.PositionScale_Y = 1.0f;
			denyModeButton.SizeOffset_X = 200;
			denyModeButton.SizeOffset_Y = 25;
			denyModeButton.AddLabel(localization.format("DenyMode_Label"), ESleekSide.RIGHT);
			denyModeButton.state = (int) ServerListCuration.Get().DenyMode;
			denyModeButton.onSwappedState += OnChangedDenyMode;
			AddChild(denyModeButton);

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

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
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

		internal MenuPlayServerCurationRulesUI rulesUI;

		private ISleekField urlField;

		private SleekList<ServerCurationItem> list;
		private ISleekLabel tutorialBox;
		private SleekButtonState defaultBehaviorButton;
		private SleekButtonState denyModeButton;
		private SleekServerCurationRuleTester rulesTester;
		private ISleekToggle testerVisibleToggle;
	}
}
