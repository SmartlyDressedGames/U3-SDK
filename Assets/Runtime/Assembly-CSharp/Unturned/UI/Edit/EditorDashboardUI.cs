////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorDashboardUI
	{
		private static SleekFullscreenBox container;
		public static Local localization;

		private static SleekButtonIcon terrainButton;
		private static SleekButtonIcon environmentButton;
		private static SleekButtonIcon spawnsButton;
		private static SleekButtonIcon levelButton;

		private void onClickedTerrainButton(ISleekElement button)
		{
			terrainMenu.open();
			EditorEnvironmentUI.close();
			EditorSpawnsUI.close();
			EditorLevelUI.close();
		}

		private void onClickedEnvironmentButton(ISleekElement button)
		{
			terrainMenu.close();
			EditorEnvironmentUI.open();
			EditorSpawnsUI.close();
			EditorLevelUI.close();
		}

		private void onClickedSpawnsButton(ISleekElement button)
		{
			terrainMenu.close();
			EditorEnvironmentUI.close();
			EditorSpawnsUI.open();
			EditorLevelUI.close();
		}

		private void onClickedLevelButton(ISleekElement button)
		{
			terrainMenu.close();
			EditorEnvironmentUI.close();
			EditorSpawnsUI.close();
			EditorLevelUI.open();
		}

		public void OnDestroy()
		{
			environmentUI.OnDestroy();
			levelUI.OnDestroy();
		}

		public EditorDashboardUI()
		{
			localization = Localization.read("/Editor/EditorDashboard.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorDashboard");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			EditorUI.window.AddChild(container);

			terrainButton = new SleekButtonIcon(icons.load<Texture2D>("Terrain"));
			terrainButton.SizeOffset_X = -5;
			terrainButton.SizeOffset_Y = 30;
			terrainButton.SizeScale_X = 0.25f;
			terrainButton.text = localization.format("TerrainButtonText");
			terrainButton.tooltip = localization.format("TerrainButtonTooltip");
			terrainButton.onClickedButton += onClickedTerrainButton;
			container.AddChild(terrainButton);

			environmentButton = new SleekButtonIcon(icons.load<Texture2D>("Environment"));
			environmentButton.PositionOffset_X = 5;
			environmentButton.PositionScale_X = 0.25f;
			environmentButton.SizeOffset_X = -10;
			environmentButton.SizeOffset_Y = 30;
			environmentButton.SizeScale_X = 0.25f;
			environmentButton.text = localization.format("EnvironmentButtonText");
			environmentButton.tooltip = localization.format("EnvironmentButtonTooltip");
			environmentButton.onClickedButton += onClickedEnvironmentButton;
			container.AddChild(environmentButton);

			spawnsButton = new SleekButtonIcon(icons.load<Texture2D>("Spawns"));
			spawnsButton.PositionOffset_X = 5;
			spawnsButton.PositionScale_X = 0.5f;
			spawnsButton.SizeOffset_X = -10;
			spawnsButton.SizeOffset_Y = 30;
			spawnsButton.SizeScale_X = 0.25f;
			spawnsButton.text = localization.format("SpawnsButtonText");
			spawnsButton.tooltip = localization.format("SpawnsButtonTooltip");
			spawnsButton.onClickedButton += onClickedSpawnsButton;
			container.AddChild(spawnsButton);

			levelButton = new SleekButtonIcon(icons.load<Texture2D>("Level"));
			levelButton.PositionOffset_X = 5;
			levelButton.PositionScale_X = 0.75f;
			levelButton.SizeOffset_X = -5;
			levelButton.SizeOffset_Y = 30;
			levelButton.SizeScale_X = 0.25f;
			levelButton.text = localization.format("LevelButtonText");
			levelButton.tooltip = localization.format("LevelButtonTooltip");
			levelButton.onClickedButton += onClickedLevelButton;
			container.AddChild(levelButton);

			new EditorPauseUI();
			terrainMenu = new EditorTerrainUI();
			environmentUI = new EditorEnvironmentUI();
			new EditorSpawnsUI();
			levelUI = new EditorLevelUI();
		}

		internal EditorTerrainUI terrainMenu;
		private EditorEnvironmentUI environmentUI;
		private EditorLevelUI levelUI;
	}
}
