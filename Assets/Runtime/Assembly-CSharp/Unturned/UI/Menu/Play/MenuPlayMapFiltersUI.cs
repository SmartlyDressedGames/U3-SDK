////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum EMenuPlayMapFiltersUIOpenContext
	{
		ServerList,
		Filters,
	}

	public class MenuPlayMapFiltersUI : SleekFullscreenBox
	{
		public Local localization;
		public IconsBundle icons;
		public bool active;

		private EMenuPlayMapFiltersUIOpenContext openContext;

		private LevelInfo[] levels;
		private SleekFilterLevel[] levelButtons;
		private int previousLayoutWidth = -1;

		private ISleekBox headerBox;
		private ISleekLabel titleLabel;
		private ISleekLabel filtersLabel;
		private ISleekButton resetButton;
		private ISleekScrollView levelScrollBox;
		private SleekButtonIcon backButton;

		public void open(EMenuPlayMapFiltersUIOpenContext openContext)
		{
			if (active)
			{
				return;
			}

			active = true;
			this.openContext = openContext;

			int layoutWidth = ScreenEx.GetWidthForLayout();
			if (levels == null || levels.Length < 1 || layoutWidth != previousLayoutWidth)
			{
				PopulateLevelButtons();
			}

			UpdateFiltersLabel();
			SynchronizeCheckBoxes();

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

		public void OpenPreviousMenu()
		{
			switch (openContext)
			{
				case EMenuPlayMapFiltersUIOpenContext.ServerList:
					MenuPlayUI.serverListUI.open(true); // Refresh because we may have modified the map.
					break;

				case EMenuPlayMapFiltersUIOpenContext.Filters:
					MenuPlayServersUI.serverListFiltersUI.open();
					break;
			}
		}

		public override void OnDestroy()
		{
			Level.onLevelsRefreshed -= OnLevelsRefreshed;

			base.OnDestroy();
		}

		private void UpdateFiltersLabel()
		{
			string displayText = FilterSettings.activeFilters.GetMapDisplayText();
			if (string.IsNullOrEmpty(displayText))
			{
				filtersLabel.Text = localization.format("MapFilter_Button_EmptyLabel");
				resetButton.IsClickable = false;
			}
			else
			{
				filtersLabel.Text = displayText;
				resetButton.IsClickable = true;
			}
		}

		private void OnClickedResetButton(ISleekElement button)
		{
			FilterSettings.activeFilters.ClearMaps();
			FilterSettings.MarkActiveFilterModified();
			UpdateFiltersLabel();
			SynchronizeCheckBoxes();
		}

		private void OnClickedBackButton(ISleekElement button)
		{
			OpenPreviousMenu();
			close();
		}

		private void OnClickedLevel(SleekLevel levelButton, byte index)
		{
			bool inFilter = FilterSettings.activeFilters.ToggleMap(levelButton.level);
			((SleekFilterLevel) levelButton).IsIncludedInFilter = inFilter;
			FilterSettings.MarkActiveFilterModified();
			UpdateFiltersLabel();
		}

		private void PopulateLevelButtons()
		{
			int layoutWidth = ScreenEx.GetWidthForLayout();
			previousLayoutWidth = layoutWidth;

			levelScrollBox.RemoveAllChildren();

			levels = Level.getLevels(ESingleplayerMapCategory.ALL);

			const int levelButtonWidth = 400;
			const int levelButtonHeight = 100;
			const int buttonSpacing = 10;

			int levelsBeforeLineWrap = Mathf.Max(1, (layoutWidth - 200) / (levelButtonWidth + buttonSpacing));
			int buttonIndex = 0;

			int verticalOffset = 0;

			levelButtons = new SleekFilterLevel[levels.Length];
			for (int index = 0; index < levels.Length; index++)
			{
				if (levels[index] != null)
				{
					SleekFilterLevel level = new SleekFilterLevel(levels[index]);
					level.PositionOffset_X = (buttonIndex % levelsBeforeLineWrap) * (levelButtonWidth + buttonSpacing);
					level.PositionOffset_Y = (buttonIndex / levelsBeforeLineWrap) * (levelButtonHeight + buttonSpacing);
					level.onClickedLevel += OnClickedLevel;
					levelScrollBox.AddChild(level);
					verticalOffset += (levelButtonHeight + buttonSpacing);

					levelButtons[index] = level;

					++buttonIndex;
				}
			}

			float listHeight = MathfEx.GetPageCount(buttonIndex, levelsBeforeLineWrap) * (levelButtonHeight + buttonSpacing);
			levelScrollBox.ContentSizeOffset = new Vector2(0.0f, listHeight - buttonSpacing);

			int totalWidth = (levelsBeforeLineWrap * (levelButtonWidth + buttonSpacing)) - buttonSpacing;

			headerBox.PositionOffset_X = -totalWidth / 2;
			headerBox.SizeOffset_X = totalWidth;
			resetButton.PositionOffset_X = -totalWidth / 2;
			resetButton.SizeOffset_X = totalWidth;
			levelScrollBox.PositionOffset_X = -totalWidth / 2;
			levelScrollBox.SizeOffset_X = totalWidth + 30;
		}

		private void SynchronizeCheckBoxes()
		{
			if (levelButtons == null)
				return;

			List<LevelInfo> levels = new List<LevelInfo>();
			FilterSettings.activeFilters.GetLevels(levels);

			foreach (SleekFilterLevel button in levelButtons)
			{
				bool inFilter = levels.Contains(button.level);
				button.IsIncludedInFilter = inFilter;
			}
		}

		private void OnLevelsRefreshed()
		{
			PopulateLevelButtons();
		}

		public MenuPlayMapFiltersUI(MenuPlayServersUI serverListUI)
		{
			localization = serverListUI.localization;
			icons = serverListUI.icons;

			active = false;

			headerBox = Glazier.Get().CreateBox();
			headerBox.PositionOffset_Y = 100;
			headerBox.PositionScale_X = 0.5f;
			headerBox.SizeOffset_Y = 100;
			headerBox.TooltipText = localization.format("MapFilter_Header_Tooltip");
			AddChild(headerBox);

			titleLabel = Glazier.Get().CreateLabel();
			titleLabel.SizeScale_X = 1.0f;
			titleLabel.SizeOffset_Y = 50;
			titleLabel.Text = localization.format("MapFilter_Header_Label");
			titleLabel.FontSize = ESleekFontSize.Medium;
			headerBox.AddChild(titleLabel);

			filtersLabel = Glazier.Get().CreateLabel();
			filtersLabel.SizeScale_X = 1.0f;
			filtersLabel.PositionOffset_Y = 50;
			filtersLabel.SizeOffset_Y = 50;
			headerBox.AddChild(filtersLabel);

			resetButton = Glazier.Get().CreateButton();
			resetButton.PositionOffset_Y = 210;
			resetButton.PositionScale_X = 0.5f;
			resetButton.SizeOffset_Y = 50;
			resetButton.Text = localization.format("MapFilter_ResetButton_Label");
			resetButton.TooltipText = localization.format("MapFilter_ResetButton_Tooltip");
			resetButton.FontSize = ESleekFontSize.Medium;
			resetButton.OnClicked += OnClickedResetButton;
			AddChild(resetButton);

			levelScrollBox = Glazier.Get().CreateScrollView();
			levelScrollBox.PositionOffset_Y = 270;
			levelScrollBox.PositionScale_X = 0.5f;
			levelScrollBox.SizeOffset_Y = -370;
			levelScrollBox.SizeScale_Y = 1;
			levelScrollBox.ScaleContentToWidth = true;
			AddChild(levelScrollBox);

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

			Level.onLevelsRefreshed += OnLevelsRefreshed;
		}
	}
}
