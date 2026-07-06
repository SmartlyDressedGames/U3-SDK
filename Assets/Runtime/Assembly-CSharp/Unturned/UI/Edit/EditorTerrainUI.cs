////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorTerrainUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		public void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			heightV2.Close();
			materialsV2.Close();
			detailsV2.Close();
			tiles.Close();

			container.AnimateOutOfView(1, 0);
		}

		public void GoToHeightsTab()
		{
			detailsV2.Close();
			materialsV2.Close();
			tiles.Close();

			heightV2.Open();
		}

		public void GoToMaterialsTab()
		{
			heightV2.Close();
			detailsV2.Close();
			tiles.Close();

			materialsV2.Open();
		}

		public void GoToFoliageTab()
		{
			heightV2.Close();
			materialsV2.Close();
			tiles.Close();

			detailsV2.Open();
		}

		public void GoToTilesTab()
		{
			heightV2.Close();
			materialsV2.Close();
			detailsV2.Close();

			tiles.Open();
		}

		private void onClickedHeightButton(ISleekElement button)
		{
			GoToHeightsTab();
		}

		private void onClickedMaterialsButton(ISleekElement button)
		{
			GoToMaterialsTab();
		}

		private void onClickedDetailsButton(ISleekElement button)
		{
			GoToFoliageTab();
		}

		private void OnClickedTilesButton(ISleekElement button)
		{
			GoToTilesTab();
		}

		public EditorTerrainUI()
		{
			Local localization = Localization.read("/Editor/EditorTerrain.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorTerrain");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_X = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			EditorUI.window.AddChild(container);
			active = false;

			SleekButtonIcon heightButton = new SleekButtonIcon(icons.load<Texture2D>("Height"));
			heightButton.PositionOffset_Y = 40;
			heightButton.SizeOffset_X = -5;
			heightButton.SizeOffset_Y = 30;
			heightButton.SizeScale_X = 0.25f;
			heightButton.text = localization.format("HeightButtonText") + " [1]";
			heightButton.tooltip = localization.format("HeightButtonTooltip");
			heightButton.onClickedButton += onClickedHeightButton;
			container.AddChild(heightButton);

			SleekButtonIcon materialsButton = new SleekButtonIcon(icons.load<Texture2D>("Materials"));
			materialsButton.PositionOffset_X = 5;
			materialsButton.PositionOffset_Y = 40;
			materialsButton.PositionScale_X = 0.25f;
			materialsButton.SizeOffset_X = -10;
			materialsButton.SizeOffset_Y = 30;
			materialsButton.SizeScale_X = 0.25f;
			materialsButton.text = localization.format("MaterialsButtonText") + " [2]";
			materialsButton.tooltip = localization.format("MaterialsButtonTooltip");
			materialsButton.onClickedButton += onClickedMaterialsButton;
			container.AddChild(materialsButton);

			SleekButtonIcon detailsButton = new SleekButtonIcon(icons.load<Texture2D>("Details"));
			detailsButton.PositionOffset_X = 5;
			detailsButton.PositionOffset_Y = 40;
			detailsButton.PositionScale_X = 0.5f;
			detailsButton.SizeOffset_X = -10;
			detailsButton.SizeOffset_Y = 30;
			detailsButton.SizeScale_X = 0.25f;
			detailsButton.text = localization.format("DetailsButtonText") + " [3]";
			detailsButton.tooltip = localization.format("DetailsButtonTooltip");
			detailsButton.onClickedButton += onClickedDetailsButton;
			container.AddChild(detailsButton);

			ISleekButton tilesButton = Glazier.Get().CreateButton();
			tilesButton.PositionOffset_X = 5;
			tilesButton.PositionOffset_Y = 40;
			tilesButton.PositionScale_X = 0.75f;
			tilesButton.SizeOffset_X = -5;
			tilesButton.SizeOffset_Y = 30;
			tilesButton.SizeScale_X = 0.25f;
			tilesButton.Text = localization.format("TilesButton_Label") + " [4]";
			tilesButton.TooltipText = localization.format("TilesButton_Tooltip");
			tilesButton.OnClicked += OnClickedTilesButton;
			container.AddChild(tilesButton);

			heightV2 = new EditorTerrainHeightUI();
			heightV2.PositionOffset_X = 10;
			heightV2.PositionOffset_Y = 90;
			heightV2.PositionScale_X = 1.0f;
			heightV2.SizeOffset_X = -20;
			heightV2.SizeOffset_Y = -100;
			heightV2.SizeScale_X = 1.0f;
			heightV2.SizeScale_Y = 1.0f;
			EditorUI.window.AddChild(heightV2);

			materialsV2 = new EditorTerrainMaterialsUI();
			materialsV2.PositionOffset_X = 10;
			materialsV2.PositionOffset_Y = 90;
			materialsV2.PositionScale_X = 1.0f;
			materialsV2.SizeOffset_X = -20;
			materialsV2.SizeOffset_Y = -100;
			materialsV2.SizeScale_X = 1.0f;
			materialsV2.SizeScale_Y = 1.0f;
			EditorUI.window.AddChild(materialsV2);

			detailsV2 = new EditorTerrainDetailsUI();
			detailsV2.PositionOffset_X = 10;
			detailsV2.PositionOffset_Y = 90;
			detailsV2.PositionScale_X = 1.0f;
			detailsV2.SizeOffset_X = -20;
			detailsV2.SizeOffset_Y = -100;
			detailsV2.SizeScale_X = 1.0f;
			detailsV2.SizeScale_Y = 1.0f;
			EditorUI.window.AddChild(detailsV2);

			tiles = new EditorTerrainTilesUI();
			tiles.PositionOffset_X = 10;
			tiles.PositionOffset_Y = 90;
			tiles.PositionScale_X = 1.0f;
			tiles.SizeOffset_X = -20;
			tiles.SizeOffset_Y = -100;
			tiles.SizeScale_X = 1.0f;
			tiles.SizeScale_Y = 1.0f;
			EditorUI.window.AddChild(tiles);
		}

		private EditorTerrainHeightUI heightV2;
		private EditorTerrainMaterialsUI materialsV2;
		private EditorTerrainDetailsUI detailsV2;
		private EditorTerrainTilesUI tiles;
	}
}
