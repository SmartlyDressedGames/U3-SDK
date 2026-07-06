////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Landscapes;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class TerrainTileLayer : SleekWrapper
	{
		public void UpdateSelectedTile()
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile != null)
			{
				AssetReference<LandscapeMaterialAsset> assetRef = tile.materials[layerIndex];
				LandscapeMaterialAsset asset = assetRef.Find();
				if (asset != null)
				{
					nameButton.Text = asset.FriendlyName;
				}
				else if (assetRef.isNull)
				{
					nameButton.Text = EditorTerrainTilesUI.localization.format("LayerNull");
				}
				else
				{
					nameButton.Text = assetRef.GUID.ToString("N");
				}
			}
			else
			{
				nameButton.Text = string.Empty;
			}
		}

		public TerrainTileLayer(EditorTerrainTilesUI owner, int layerIndex) : base()
		{
			this.owner = owner;
			this.layerIndex = layerIndex;

			layerBox = Glazier.Get().CreateBox();
			layerBox.SizeOffset_X = 30;
			layerBox.SizeScale_Y = 1.0f;
			layerBox.Text = layerIndex.ToString();
			AddChild(layerBox);

			nameButton = Glazier.Get().CreateButton();
			nameButton.PositionOffset_X = 30;
			nameButton.SizeScale_X = 1.0f;
			nameButton.SizeScale_Y = 1.0f;
			nameButton.SizeOffset_X = -30;
			nameButton.OnClicked += OnClicked;
			AddChild(nameButton);

			UpdateSelectedTile();
		}

		private void OnClicked(ISleekElement element)
		{
			owner.SetSelectedLayerIndex(layerIndex);
		}

		private EditorTerrainTilesUI owner;
		private int layerIndex;
		private ISleekBox layerBox;
		private ISleekButton nameButton;
	}

	internal class EditorTerrainTilesUI : SleekFullscreenBox
	{
		public void Open()
		{
			AnimateIntoView();

			TerrainEditor.toolMode = TerrainEditor.EDevkitLandscapeToolMode.TILE;
			EditorInteract.instance.SetActiveTool(EditorInteract.instance.terrainTool);
		}

		public void Close()
		{
			AnimateOutOfView(1.0f, 0.0f);

			TerrainEditor.selectedTile = null;
			EditorInteract.instance.SetActiveTool(null);
		}

		public override void OnDestroy()
		{
			TerrainEditor.selectedTileChanged -= OnSelectedTileChanged;
		}

		public EditorTerrainTilesUI() : base()
		{
			localization = Localization.read("/Editor/EditorTerrainTiles.dat");

			TerrainEditor.selectedTileChanged += OnSelectedTileChanged;

			hintLabel = Glazier.Get().CreateLabel();
			hintLabel.PositionScale_Y = 1.0f;
			hintLabel.PositionOffset_Y = -30;
			hintLabel.SizeScale_X = 1.0f;
			hintLabel.SizeOffset_Y = 30;
			hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			hintLabel.Text = localization.format("Hint_Remove", "Delete");
			AddChild(hintLabel);

			float layersColumnOffset = 0;

			repairEdgesButton = Glazier.Get().CreateButton();
			repairEdgesButton.PositionScale_X = 1.0f;
			repairEdgesButton.PositionScale_Y = 1.0f;
			repairEdgesButton.PositionOffset_X = -560;
			repairEdgesButton.SizeOffset_X = 250;
			repairEdgesButton.SizeOffset_Y = 30;
			layersColumnOffset -= repairEdgesButton.SizeOffset_Y;
			repairEdgesButton.PositionOffset_Y = layersColumnOffset;
			layersColumnOffset -= 10;
			repairEdgesButton.Text = localization.format("RepairEdges_Label");
			repairEdgesButton.OnClicked += OnClickedRepairEdges;
			repairEdgesButton.TooltipText = localization.format("RepairEdges_Tooltip");
			AddChild(repairEdgesButton);

			applyToAllTilesButton = new SleekButtonIconConfirm(null, localization.format("ApplyToAllTiles_Confirm_Label"), localization.format("ApplyToAllTiles_Confirm_Tooltip"),
				localization.format("ApplyToAllTiles_Deny_Label"), localization.format("ApplyToAllTiles_Deny_Tooltip"));
			applyToAllTilesButton.PositionScale_X = 1.0f;
			applyToAllTilesButton.PositionScale_Y = 1.0f;
			applyToAllTilesButton.PositionOffset_X = -560;
			applyToAllTilesButton.SizeOffset_X = 250;
			applyToAllTilesButton.SizeOffset_Y = 30;
			layersColumnOffset -= applyToAllTilesButton.SizeOffset_Y;
			applyToAllTilesButton.PositionOffset_Y = layersColumnOffset;
			layersColumnOffset -= 10;
			applyToAllTilesButton.text = localization.format("ApplyToAllTiles_Label");
			applyToAllTilesButton.tooltip = localization.format("ApplyToAllTiles_Tooltip");
			applyToAllTilesButton.onConfirmed += OnApplyToAllTiles;
			AddChild(applyToAllTilesButton);

			resetSplatmapButton = new SleekButtonIconConfirm(null, localization.format("ResetSplatmap_Confirm_Label"), localization.format("ResetSplatmap_Confirm_Tooltip"),
				localization.format("ResetSplatmap_Deny_Label"), localization.format("ResetSplatmap_Deny_Tooltip"));
			resetSplatmapButton.PositionScale_X = 1.0f;
			resetSplatmapButton.PositionScale_Y = 1.0f;
			resetSplatmapButton.PositionOffset_X = -560;
			resetSplatmapButton.SizeOffset_X = 250;
			resetSplatmapButton.SizeOffset_Y = 30;
			layersColumnOffset -= resetSplatmapButton.SizeOffset_Y;
			resetSplatmapButton.PositionOffset_Y = layersColumnOffset;
			layersColumnOffset -= 10;
			resetSplatmapButton.text = localization.format("ResetSplatmap_Label");
			resetSplatmapButton.tooltip = localization.format("ResetSplatmap_Tooltip");
			resetSplatmapButton.onConfirmed += OnResetSplatmap;
			AddChild(resetSplatmapButton);

			resetHeightmapButton = new SleekButtonIconConfirm(null, localization.format("ResetHeightmap_Confirm_Label"), localization.format("ResetHeightmap_Confirm_Tooltip"),
				localization.format("ResetHeightmap_Deny_Label"), localization.format("ResetHeightmap_Deny_Tooltip"));
			resetHeightmapButton.PositionScale_X = 1.0f;
			resetHeightmapButton.PositionScale_Y = 1.0f;
			resetHeightmapButton.PositionOffset_X = -560;
			resetHeightmapButton.SizeOffset_X = 250;
			resetHeightmapButton.SizeOffset_Y = 30;
			layersColumnOffset -= resetHeightmapButton.SizeOffset_Y;
			resetHeightmapButton.PositionOffset_Y = layersColumnOffset;
			layersColumnOffset -= 10;
			resetHeightmapButton.text = localization.format("ResetHeightmap_Label");
			resetHeightmapButton.tooltip = localization.format("ResetHeightmap_Tooltip");
			resetHeightmapButton.onConfirmed += OnResetHeightmap;
			AddChild(resetHeightmapButton);

			layers = new TerrainTileLayer[Landscape.SPLATMAP_LAYERS];
			for (int layerIndex = layers.Length - 1; layerIndex >= 0; --layerIndex)
			{
				TerrainTileLayer element = new TerrainTileLayer(this, layerIndex);
				layers[layerIndex] = element;
				element.PositionScale_X = 1.0f;
				element.PositionScale_Y = 1.0f;
				element.PositionOffset_X = -560;
				element.SizeOffset_X = 250;
				element.SizeOffset_Y = 30;
				layersColumnOffset -= element.SizeOffset_Y;
				element.PositionOffset_Y = layersColumnOffset;
				AddChild(element);
			}

			int rightColumnWidth = 300;
			float upperRightOffset = 0;

			selectedLayerBox = new SleekBoxIcon(null, 64);
			selectedLayerBox.SizeOffset_X = rightColumnWidth;
			selectedLayerBox.PositionOffset_X = -selectedLayerBox.SizeOffset_X;
			selectedLayerBox.SizeOffset_Y = 74;
			selectedLayerBox.PositionScale_X = 1.0f;
			selectedLayerBox.AddLabel(localization.format("SelectedLayer"), ESleekSide.LEFT);
			AddChild(selectedLayerBox);
			upperRightOffset += selectedLayerBox.SizeOffset_Y + 10;

			layerGuidField = Glazier.Get().CreateStringField();
			layerGuidField.SizeOffset_X = rightColumnWidth;
			layerGuidField.PositionOffset_X = -layerGuidField.SizeOffset_X;
			layerGuidField.PositionOffset_Y = upperRightOffset;
			layerGuidField.SizeOffset_Y = 30;
			layerGuidField.PositionScale_X = 1.0f;
			layerGuidField.MaxLength = 32;
			layerGuidField.AddLabel(localization.format("LayerGuid"), ESleekSide.LEFT);
			layerGuidField.OnTextSubmitted += OnLayerGuidEntered;
			AddChild(layerGuidField);
			upperRightOffset += layerGuidField.SizeOffset_Y + 10;

			resetAssetButton = Glazier.Get().CreateButton();
			resetAssetButton.SizeOffset_X = rightColumnWidth;
			resetAssetButton.PositionOffset_X = -resetAssetButton.SizeOffset_X;
			resetAssetButton.PositionOffset_Y = upperRightOffset;
			resetAssetButton.SizeOffset_Y = 30;
			resetAssetButton.PositionScale_X = 1.0f;
			resetAssetButton.Text = localization.format("ResetAsset");
			resetAssetButton.OnClicked += OnClickedResetAsset;
			AddChild(resetAssetButton);
			upperRightOffset += resetAssetButton.SizeOffset_Y + 10;

			searchAssets = new List<LandscapeMaterialAsset>();
			assetScrollView = Glazier.Get().CreateScrollView();
			assetScrollView.PositionOffset_Y = upperRightOffset;
			assetScrollView.PositionScale_X = 1.0f;
			assetScrollView.SizeOffset_X = rightColumnWidth;
			assetScrollView.SizeOffset_Y = -upperRightOffset;
			assetScrollView.PositionOffset_X = -assetScrollView.SizeOffset_X;
			assetScrollView.SizeScale_Y = 1.0f;
			assetScrollView.ScaleContentToWidth = true;
			AddChild(assetScrollView);
			RefreshAssets();
		}

		private void OnSelectedTileChanged(LandscapeTile oldSelectedTile, LandscapeTile newSelectedTile)
		{
			foreach (TerrainTileLayer element in layers)
			{
				element.UpdateSelectedTile();
			}
			SetSelectedLayerIndex(-1);
		}

		public void SetSelectedLayerIndex(int layerIndex)
		{
			selectedLayerIndex = layerIndex;

			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile != null && layerIndex >= 0)
			{
				AssetReference<LandscapeMaterialAsset> assetRef = tile.materials[layerIndex];
				LandscapeMaterialAsset asset = assetRef.Find();
				if (asset != null)
				{
					selectedLayerBox.icon = Assets.load(asset.texture);
					selectedLayerBox.text = asset.FriendlyName;
				}
				else if (assetRef.isNull)
				{
					selectedLayerBox.icon = null;
					selectedLayerBox.text = localization.format("LayerNull");
				}
				else
				{
					selectedLayerBox.icon = null;
					selectedLayerBox.text = localization.format("LayerMissing");
				}

				layerGuidField.Text = assetRef.ToString();
			}
			else
			{
				selectedLayerBox.icon = null;
				selectedLayerBox.text = string.Empty;
				layerGuidField.Text = string.Empty;
			}
		}

		private void OnLayerGuidEntered(ISleekField field)
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile == null || selectedLayerIndex < 0)
				return;

			AssetReference<LandscapeMaterialAsset> value;
			AssetReference<LandscapeMaterialAsset>.TryParse(field.Text, out value);
			tile.materials[selectedLayerIndex] = value;
			tile.updatePrototypes();
			layers[selectedLayerIndex].UpdateSelectedTile();
			SetSelectedLayerIndex(selectedLayerIndex);
			SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
		}

		private void OnClickedResetAsset(ISleekElement button)
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile == null || selectedLayerIndex < 0)
				return;

			tile.materials[selectedLayerIndex] = AssetReference<LandscapeMaterialAsset>.invalid;
			tile.updatePrototypes();
			layers[selectedLayerIndex].UpdateSelectedTile();
			SetSelectedLayerIndex(selectedLayerIndex);
			SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
		}

		private void OnAssetClicked(ISleekElement button)
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile == null || selectedLayerIndex < 0)
				return;

			int assetIndex = assetScrollView.FindIndexOfChild(button);
			tile.materials[selectedLayerIndex] = searchAssets[assetIndex].getReferenceTo<LandscapeMaterialAsset>();
			tile.updatePrototypes();
			layers[selectedLayerIndex].UpdateSelectedTile();
			SetSelectedLayerIndex(selectedLayerIndex);
			SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
		}

		private void RefreshAssets()
		{
			searchAssets.Clear();
			assetScrollView.RemoveAllChildren();

			float offset = 0;

			Assets.find(searchAssets);
			searchAssets.Sort((LandscapeMaterialAsset lhs, LandscapeMaterialAsset rhs) =>
			{
				return lhs.name.CompareTo(rhs.name);
			});

			foreach (LandscapeMaterialAsset asset in searchAssets)
			{
				SleekButtonIcon button = new SleekButtonIcon(Assets.load(asset.texture), 64);
				button.PositionOffset_Y = offset;
				button.SizeScale_X = 1.0f;
				button.SizeOffset_Y = 74;
				button.text = asset.FriendlyName;
				button.onClickedButton += OnAssetClicked;
				assetScrollView.AddChild(button);
				offset += button.SizeOffset_Y;
			}

			assetScrollView.ContentSizeOffset = new Vector2(0.0f, offset);
		}

		private void OnResetHeightmap(SleekButtonIconConfirm button)
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile == null)
				return;

			tile.resetHeightmap();
			SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
		}

		private void OnResetSplatmap(SleekButtonIconConfirm button)
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile == null)
				return;

			tile.resetSplatmap();
			SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
		}

		private void OnApplyToAllTiles(SleekButtonIconConfirm button)
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile == null)
				return;

			Landscape.CopyLayersToAllTiles(tile);
			SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
		}

		private void OnClickedRepairEdges(ISleekElement button)
		{
			LandscapeTile tile = TerrainEditor.selectedTile;
			if (tile == null)
				return;

			Landscape.reconcileNeighbors(tile);
			SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
		}

		internal static Local localization;
		private ISleekLabel hintLabel;
		private TerrainTileLayer[] layers;
		private SleekButtonIconConfirm resetHeightmapButton;
		private SleekButtonIconConfirm resetSplatmapButton;
		private SleekButtonIconConfirm applyToAllTilesButton;
		private ISleekButton repairEdgesButton;
		internal int selectedLayerIndex;
		private List<LandscapeMaterialAsset> searchAssets;
		private SleekBoxIcon selectedLayerBox;
		private ISleekField layerGuidField;
		private ISleekButton resetAssetButton;
		private ISleekScrollView assetScrollView;
	}
}
