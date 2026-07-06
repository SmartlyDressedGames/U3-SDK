////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class EditorEnvironmentRoadsUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		/// <summary>
		/// Switches between "legacy" (per-level road textures bundle) and "assets" (using RoadAsset).
		/// </summary>
		private static SleekButtonState listModeButton;

		private static List<RoadAsset> searchAssets;

		private static ISleekElement legacyRoadMaterialsContainer;
		private static ISleekScrollView legacyScrollBox;
		private static ISleekBox legacySelectedBox;
		private static ISleekFloat32Field legacyWidthField;
		private static ISleekFloat32Field legacyHeightField;
		private static ISleekFloat32Field legacyDepthField;
		private static ISleekFloat32Field legacyOffset2Field;
		private static ISleekToggle legacyConcreteToggle;

		private static ISleekElement assetContainer;
		private static ISleekScrollView assetScrollView;
		private static SleekBoxIcon selectedAssetBox;
		private static ISleekToggle onlyUsedAssetsToggle;
		private static ISleekField searchField;

		private static SleekButtonIcon bakeRoadsButton;

		private static ISleekFloat32Field offsetField;
		private static ISleekToggle loopToggle;
		private static ISleekToggle ignoreTerrainToggle;
		private static SleekButtonState modeButton;
		private static ISleekBox roadIndexBox;
		private static SleekAssetField roadAssetField;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			EditorRoads.isPaving = true;

			EditorUI.message(EEditorMessage.ROADS);

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			EditorRoads.isPaving = false;

			container.AnimateOutOfView(1, 0);
		}

		public static void updateSelection(Road road, RoadJoint joint)
		{
			if (road != null && joint != null)
			{
				offsetField.Value = joint.offset;
				loopToggle.Value = road.isLoop;
				ignoreTerrainToggle.Value = joint.ignoreTerrain;
				modeButton.state = (int) joint.mode;
				roadIndexBox.Text = LevelRoads.getRoadIndex(road).ToString();
				roadAssetField.Value = road.RoadAssetRef;

				EditorRoads.selected = road.material;
				EditorRoads.selectedAssetRef = road.RoadAssetRef;
				RefreshAssetSelection();
				RefreshLegacySelection();

				int newListMode;
				if (EditorRoads.selectedAssetRef.IsAssigned)
				{
					newListMode = 1;
				}
				else
				{
					newListMode = 0;
				}
				if (newListMode != listModeButton.state)
				{
					listModeButton.state = newListMode;
					RefreshListMode();
				}
			}

			offsetField.IsVisible = road != null;
			loopToggle.IsVisible = road != null;
			ignoreTerrainToggle.IsVisible = road != null;
			modeButton.IsVisible = road != null;
			roadIndexBox.IsVisible = road != null;
			roadAssetField.IsVisible = road != null;
		}

		private static void RefreshLegacySelection()
		{
			if (EditorRoads.selected < LevelRoads.materials.Length)
			{
				RoadMaterial mat = LevelRoads.materials[EditorRoads.selected];

				legacySelectedBox.Text = mat.material.mainTexture.name;

				legacyWidthField.Value = mat.width;
				legacyHeightField.Value = mat.height;
				legacyDepthField.Value = mat.depth;
				legacyOffset2Field.Value = mat.offset;
				legacyConcreteToggle.Value = mat.isConcrete;
			}
		}

		private static void RefreshAssetSelection()
		{
			RoadAsset selectedAsset = EditorRoads.selectedAssetRef.Get<RoadAsset>();
			if (selectedAsset != null)
			{
				selectedAssetBox.icon = selectedAsset.RoadTexture;
				selectedAssetBox.text = selectedAsset.FriendlyName;
			}
			else
			{
				selectedAssetBox.icon = null;
				selectedAssetBox.text = string.Empty;
			}
		}

		private static void OnLegacyRoadMaterialClicked(ISleekElement button)
		{
			EditorRoads.selected = (byte) (button.Parent.PositionOffset_Y / 70);
			RefreshLegacySelection();

			if (EditorRoads.road != null)
			{
				EditorRoads.road.material = EditorRoads.selected;
				EditorRoads.road.RoadAssetRef = null;
				roadAssetField.Value = null;
			}
		}

		private static void OnLegacyWidthChanged(ISleekFloat32Field field, float state)
		{
			LevelRoads.materials[EditorRoads.selected].width = state;
		}

		private static void OnLegacyHeightChanged(ISleekFloat32Field field, float state)
		{
			LevelRoads.materials[EditorRoads.selected].height = state;
		}

		private static void OnLegacyDepthChanged(ISleekFloat32Field field, float state)
		{
			LevelRoads.materials[EditorRoads.selected].depth = state;
		}

		private static void OnLegacyMaterialOffsetChanged(ISleekFloat32Field field, float state)
		{
			LevelRoads.materials[EditorRoads.selected].offset = state;
		}

		private static void OnLegacyConcreteToggled(ISleekToggle toggle, bool state)
		{
			LevelRoads.materials[EditorRoads.selected].isConcrete = state;
		}

		private static void OnAssetClicked(ISleekElement button)
		{
			int index = assetScrollView.FindIndexOfChild(button);
			RoadAsset selectedAsset = (index >= 0 && index < searchAssets.Count) ? searchAssets[index] : null;
			EditorRoads.selectedAssetRef = selectedAsset;
			RefreshAssetSelection();

			if (EditorRoads.road != null)
			{
				EditorRoads.road.RoadAssetRef = EditorRoads.selectedAssetRef;
				roadAssetField.Value = EditorRoads.selectedAssetRef;
			}
		}

		private static void OnOnlyUsedAssetsToggled(ISleekToggle toggle, bool state)
		{
			RefreshAssets();
		}

		private static void OnNameFilterSubmitted(ISleekField field)
		{
			RefreshAssets();
		}

		private static void OnListModeChanged(SleekButtonState button, int state)
		{
			RefreshListMode();
		}

		private static void onClickedBakeRoadsButton(ISleekElement button)
		{
			LevelRoads.bakeRoads();
		}

		private static void onTypedOffsetField(ISleekFloat32Field field, float state)
		{
			EditorRoads.joint.offset = state;
			EditorRoads.road.updatePoints();
		}

		private static void OnRoadAssetChanged(SleekAssetField field)
		{
			if (EditorRoads.road != null)
			{
				EditorRoads.road.RoadAssetRef = field.Value;
			}
		}

		private static void onToggledLoopToggle(ISleekToggle toggle, bool state)
		{
			EditorRoads.road.isLoop = state;
		}

		private static void onToggledIgnoreTerrainToggle(ISleekToggle toggle, bool state)
		{
			EditorRoads.joint.ignoreTerrain = state;
			EditorRoads.road.updatePoints();
		}

		private static void onSwappedStateMode(SleekButtonState button, int index)
		{
			EditorRoads.joint.mode = (ERoadMode) index;
		}

		private static void RefreshListMode()
		{
			bool isUsingAssets = listModeButton.state == 1;
			legacyRoadMaterialsContainer.IsVisible = !isUsingAssets;
			assetContainer.IsVisible = isUsingAssets;

			listModeButton.SizeOffset_X = isUsingAssets ? 300 : 200;
			listModeButton.PositionOffset_X = -listModeButton.SizeOffset_X;

			bakeRoadsButton.SizeOffset_X = listModeButton.SizeOffset_X;
			bakeRoadsButton.PositionOffset_X = listModeButton.PositionOffset_X;

			if (isUsingAssets)
			{
				RefreshAssets();
			}
		}

		private static void RefreshAssets()
		{
			searchAssets.Clear();
			if (onlyUsedAssetsToggle.Value)
			{
				LevelRoads.GatherUniqueAssets(searchAssets);
			}
			else
			{
				Assets.find(searchAssets);
			}

			string searchText = searchField.Text;
			if (!string.IsNullOrEmpty(searchText))
			{
				searchAssets.RemoveSwap((RoadAsset asset) =>
				{
					return asset.FriendlyName.IndexOf(searchText, System.StringComparison.CurrentCultureIgnoreCase) == -1;
				});
			}

			searchAssets.Sort((RoadAsset lhs, RoadAsset rhs) =>
			{
				return lhs.FriendlyName.CompareTo(rhs.FriendlyName);
			});

			assetScrollView.RemoveAllChildren();
			float offset = 0;
			foreach (RoadAsset asset in searchAssets)
			{
				SleekButtonIcon button = new SleekButtonIcon(asset.RoadTexture, 64);
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

		public EditorEnvironmentRoadsUI()
		{
			Local localization = Localization.read("/Editor/EditorEnvironmentRoads.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorEnvironmentRoads");

			searchAssets = new List<RoadAsset>();

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

			listModeButton = new SleekButtonState(
				new GUIContent(localization.format("ListMode_Legacy_Label"), localization.format("ListMode_Legacy_Tooltip")),
				new GUIContent(localization.format("ListMode_RoadAssets_Label"), localization.format("ListMode_RoadAssets_Tooltip"))
				);
			listModeButton.PositionOffset_X = -200;
			listModeButton.PositionOffset_Y = 80;
			listModeButton.PositionScale_X = 1;
			listModeButton.SizeOffset_X = 200;
			listModeButton.SizeOffset_Y = 30;
			listModeButton.UseContentTooltip = true;
			listModeButton.AddLabel(localization.format("ListMode_Label"), ESleekSide.LEFT);
			listModeButton.onSwappedState += OnListModeChanged;
			container.AddChild(listModeButton);

			legacyRoadMaterialsContainer = Glazier.Get().CreateFrame();
			legacyRoadMaterialsContainer.PositionOffset_X = -200;
			legacyRoadMaterialsContainer.PositionOffset_Y = 120;
			legacyRoadMaterialsContainer.PositionScale_X = 1;
			legacyRoadMaterialsContainer.SizeOffset_X = 200;
			legacyRoadMaterialsContainer.SizeOffset_Y = -160;
			legacyRoadMaterialsContainer.SizeScale_Y = 1;
			container.AddChild(legacyRoadMaterialsContainer);

			legacySelectedBox = Glazier.Get().CreateBox();
			legacySelectedBox.SizeScale_X = 1;
			legacySelectedBox.SizeOffset_Y = 30;
			legacySelectedBox.AddLabel(localization.format("SelectionBoxLabelText"), ESleekSide.LEFT);
			legacyRoadMaterialsContainer.AddChild(legacySelectedBox);

			legacyScrollBox = Glazier.Get().CreateScrollView();
			legacyScrollBox.PositionOffset_X = -200;
			legacyScrollBox.PositionOffset_Y = 40;
			legacyScrollBox.SizeScale_Y = 1;
			legacyScrollBox.SizeOffset_X = 400; // Wider to accomodate labels on the left.
			legacyScrollBox.SizeOffset_Y = -40;
			legacyScrollBox.ScaleContentToWidth = true;
			legacyScrollBox.ContentSizeOffset = new Vector2(0.0f, (LevelRoads.materials.Length * 70) + 200);
			legacyRoadMaterialsContainer.AddChild(legacyScrollBox);

			for (int index = 0; index < LevelRoads.materials.Length; index++)
			{
				ISleekImage materialImage = Glazier.Get().CreateImage();
				materialImage.PositionOffset_X = 200;
				materialImage.PositionOffset_Y = index * 70;
				materialImage.SizeOffset_X = 64;
				materialImage.SizeOffset_Y = 64;
				materialImage.Texture = LevelRoads.materials[index].material.mainTexture;
				legacyScrollBox.AddChild(materialImage);

				ISleekButton nameBox = Glazier.Get().CreateButton();
				nameBox.PositionOffset_X = 70;
				nameBox.SizeOffset_X = 100;
				nameBox.SizeOffset_Y = 64;
				nameBox.Text = LevelRoads.materials[index].material.mainTexture.name;
				nameBox.OnClicked += OnLegacyRoadMaterialClicked;
				materialImage.AddChild(nameBox);
			}

			legacyWidthField = Glazier.Get().CreateFloat32Field();
			legacyWidthField.PositionOffset_X = 200;
			legacyWidthField.PositionOffset_Y = LevelRoads.materials.Length * 70;
			legacyWidthField.SizeOffset_X = 170;
			legacyWidthField.SizeOffset_Y = 30;
			legacyWidthField.AddLabel(localization.format("WidthFieldLabelText"), ESleekSide.LEFT);
			legacyWidthField.OnValueChanged += OnLegacyWidthChanged;
			legacyScrollBox.AddChild(legacyWidthField);

			legacyHeightField = Glazier.Get().CreateFloat32Field();
			legacyHeightField.PositionOffset_X = 200;
			legacyHeightField.PositionOffset_Y = (LevelRoads.materials.Length * 70) + 40;
			legacyHeightField.SizeOffset_X = 170;
			legacyHeightField.SizeOffset_Y = 30;
			legacyHeightField.AddLabel(localization.format("HeightFieldLabelText"), ESleekSide.LEFT);
			legacyHeightField.OnValueChanged += OnLegacyHeightChanged;
			legacyScrollBox.AddChild(legacyHeightField);

			legacyDepthField = Glazier.Get().CreateFloat32Field();
			legacyDepthField.PositionOffset_X = 200;
			legacyDepthField.PositionOffset_Y = (LevelRoads.materials.Length * 70) + 80;
			legacyDepthField.SizeOffset_X = 170;
			legacyDepthField.SizeOffset_Y = 30;
			legacyDepthField.AddLabel(localization.format("DepthFieldLabelText"), ESleekSide.LEFT);
			legacyDepthField.OnValueChanged += OnLegacyDepthChanged;
			legacyScrollBox.AddChild(legacyDepthField);

			legacyOffset2Field = Glazier.Get().CreateFloat32Field();
			legacyOffset2Field.PositionOffset_X = 200;
			legacyOffset2Field.PositionOffset_Y = (LevelRoads.materials.Length * 70) + 120;
			legacyOffset2Field.SizeOffset_X = 170;
			legacyOffset2Field.SizeOffset_Y = 30;
			legacyOffset2Field.AddLabel(localization.format("OffsetFieldLabelText"), ESleekSide.LEFT);
			legacyOffset2Field.OnValueChanged += OnLegacyMaterialOffsetChanged;
			legacyScrollBox.AddChild(legacyOffset2Field);

			legacyConcreteToggle = Glazier.Get().CreateToggle();
			legacyConcreteToggle.PositionOffset_X = 200;
			legacyConcreteToggle.PositionOffset_Y = (LevelRoads.materials.Length * 70) + 160;
			legacyConcreteToggle.SizeOffset_X = 40;
			legacyConcreteToggle.SizeOffset_Y = 40;
			legacyConcreteToggle.AddLabel(localization.format("ConcreteToggleLabelText"), ESleekSide.RIGHT);
			legacyConcreteToggle.OnValueChanged += OnLegacyConcreteToggled;
			legacyScrollBox.AddChild(legacyConcreteToggle);

			assetContainer = Glazier.Get().CreateFrame();
			assetContainer.PositionOffset_X = -300;
			assetContainer.PositionOffset_Y = 120;
			assetContainer.PositionScale_X = 1;
			assetContainer.SizeOffset_X = 300;
			assetContainer.SizeOffset_Y = -160;
			assetContainer.SizeScale_Y = 1;
			assetContainer.IsVisible = false;
			container.AddChild(assetContainer);

			selectedAssetBox = new SleekBoxIcon(null, 64);
			selectedAssetBox.SizeScale_X = 1f;
			selectedAssetBox.SizeOffset_Y = 74;
			selectedAssetBox.AddLabel(localization.format("SelectedAsset_Label"), ESleekSide.LEFT);
			assetContainer.AddChild(selectedAssetBox);

			onlyUsedAssetsToggle = Glazier.Get().CreateToggle();
			onlyUsedAssetsToggle.SizeOffset_X = 40;
			onlyUsedAssetsToggle.SizeOffset_Y = 40;
			onlyUsedAssetsToggle.PositionOffset_Y = 84;
			onlyUsedAssetsToggle.AddLabel(localization.format("OnlyUsedAssets_Label"), ESleekSide.RIGHT);
			onlyUsedAssetsToggle.OnValueChanged += OnOnlyUsedAssetsToggled;
			assetContainer.AddChild(onlyUsedAssetsToggle);
			
			searchField = Glazier.Get().CreateStringField();
			searchField.PositionOffset_Y = 124;
			searchField.SizeOffset_Y = 30;
			searchField.SizeScale_X = 1;
			searchField.PlaceholderText = localization.format("SearchHint");
			searchField.OnTextSubmitted += OnNameFilterSubmitted;
			assetContainer.AddChild(searchField);

			assetScrollView = Glazier.Get().CreateScrollView();
			assetScrollView.PositionOffset_Y = 154;
			assetScrollView.SizeScale_X = 1;
			assetScrollView.SizeScale_Y = 1;
			assetScrollView.SizeOffset_Y = -154;
			assetScrollView.ScaleContentToWidth = true;
			assetContainer.AddChild(assetScrollView);

			bakeRoadsButton = new SleekButtonIcon(icons.load<Texture2D>("Roads"));
			bakeRoadsButton.PositionOffset_X = -200;
			bakeRoadsButton.PositionOffset_Y = -30;
			bakeRoadsButton.PositionScale_X = 1;
			bakeRoadsButton.PositionScale_Y = 1;
			bakeRoadsButton.SizeOffset_X = 200;
			bakeRoadsButton.SizeOffset_Y = 30;
			bakeRoadsButton.text = localization.format("BakeRoadsButtonText");
			bakeRoadsButton.tooltip = localization.format("BakeRoadsButtonTooltip");
			bakeRoadsButton.onClickedButton += onClickedBakeRoadsButton;
			container.AddChild(bakeRoadsButton);

			offsetField = Glazier.Get().CreateFloat32Field();
			offsetField.PositionOffset_Y = -280;
			offsetField.PositionScale_Y = 1;
			offsetField.SizeOffset_X = 200;
			offsetField.SizeOffset_Y = 30;
			offsetField.AddLabel(localization.format("OffsetFieldLabelText"), ESleekSide.RIGHT);
			offsetField.OnValueChanged += onTypedOffsetField;
			container.AddChild(offsetField);
			offsetField.IsVisible = false;

			loopToggle = Glazier.Get().CreateToggle();
			loopToggle.PositionOffset_Y = -240;
			loopToggle.PositionScale_Y = 1;
			loopToggle.SizeOffset_X = 40;
			loopToggle.SizeOffset_Y = 40;
			loopToggle.AddLabel(localization.format("LoopToggleLabelText"), ESleekSide.RIGHT);
			loopToggle.OnValueChanged += onToggledLoopToggle;
			container.AddChild(loopToggle);
			loopToggle.IsVisible = false;

			ignoreTerrainToggle = Glazier.Get().CreateToggle();
			ignoreTerrainToggle.PositionOffset_Y = -190;
			ignoreTerrainToggle.PositionScale_Y = 1;
			ignoreTerrainToggle.SizeOffset_X = 40;
			ignoreTerrainToggle.SizeOffset_Y = 40;
			ignoreTerrainToggle.AddLabel(localization.format("IgnoreTerrainToggleLabelText"), ESleekSide.RIGHT);
			ignoreTerrainToggle.OnValueChanged += onToggledIgnoreTerrainToggle;
			container.AddChild(ignoreTerrainToggle);
			ignoreTerrainToggle.IsVisible = false;

			modeButton = new SleekButtonState(new GUIContent(localization.format("Mirror")), new GUIContent(localization.format("Aligned")), new GUIContent(localization.format("Free")));
			modeButton.PositionOffset_Y = -140;
			modeButton.PositionScale_Y = 1;
			modeButton.SizeOffset_X = 200;
			modeButton.SizeOffset_Y = 30;
			modeButton.tooltip = localization.format("ModeButtonTooltipText");
			modeButton.onSwappedState = onSwappedStateMode;
			container.AddChild(modeButton);
			modeButton.IsVisible = false;

			roadIndexBox = Glazier.Get().CreateBox();
			roadIndexBox.PositionOffset_Y = -100;
			roadIndexBox.PositionScale_Y = 1;
			roadIndexBox.SizeOffset_X = 200;
			roadIndexBox.SizeOffset_Y = 30;
			roadIndexBox.AddLabel(localization.format("RoadIndexLabelText"), ESleekSide.RIGHT);
			container.AddChild(roadIndexBox);
			roadIndexBox.IsVisible = false;

			roadAssetField = new SleekAssetField(typeof(RoadAsset));
			roadAssetField.PositionOffset_Y = -60;
			roadAssetField.PositionScale_Y = 1.0f;
			roadAssetField.SizeOffset_X = 200;
			roadAssetField.SizeOffset_Y = 60;
			roadAssetField.AddLabel(localization.format("RoadAsset_Label"), ESleekSide.RIGHT);
			roadAssetField.TooltipText = localization.format("RoadAsset_Tooltip");
			roadAssetField.OnValueChanged += OnRoadAssetChanged;
			roadAssetField.IsVisible = false;
			container.AddChild(roadAssetField);

			RefreshListMode();
			RefreshLegacySelection();
		}
	}
}
