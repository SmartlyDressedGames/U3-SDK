////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class AssetNameAscendingComparator : IComparer<Asset>
	{
		public int Compare(Asset a, Asset b)
		{
			return a.name.CompareTo(b.name);
		}
	}

	public class EditorLevelObjectsUI : SleekFullscreenBox
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static List<ObjectAsset> tempObjectAssets = new List<ObjectAsset>();
		private static List<Asset> assets;
		private static AssetNameAscendingComparator comparator = new AssetNameAscendingComparator();

		private static SleekList<Asset> assetsScrollBox;
		private static ISleekBox selectedBox;
		private static ISleekField searchField;
		private static ISleekButton searchButton;
		private static ISleekToggle largeToggle;
		private static ISleekToggle mediumToggle;
		private static ISleekToggle smallToggle;
		private static ISleekToggle barricadesToggle;
		private static ISleekToggle structuresToggle;
		private static ISleekToggle npcsToggle;

		private static ISleekImage dragBox;

		private static ISleekToggle isOwnedCullingVolumeAllowedToggle;
		private static ISleekField materialPaletteOverrideField;
		private static ISleekInt32Field materialIndexOverrideField;

		private static ISleekFloat32Field snapTransformField;
		private static ISleekFloat32Field snapRotationField;

		private static SleekButtonIcon transformButton;
		private static SleekButtonIcon rotateButton;
		private static SleekButtonIcon scaleButton;
		public static SleekButtonState coordinateButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			EditorObjects.isBuilding = true;

			EditorUI.message(EEditorMessage.OBJECTS);

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			EditorObjects.isBuilding = false;

			container.AnimateOutOfView(1, 0);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			GameObject newFocusedGameObject = EditorObjects.GetMostRecentSelectedGameObject();
			if (focusedGameObject == newFocusedGameObject)
				return;

			focusedGameObject = newFocusedGameObject;
			focusedLevelObject = LevelObjects.FindLevelObject(focusedGameObject);

			if (focusedLevelObject != null)
			{
#if !DEDICATED_SERVER
				if (focusedLevelObject.asset != null && focusedLevelObject.asset.lod != EObjectLOD.NONE)
				{
					isOwnedCullingVolumeAllowedToggle.IsVisible = true;
					isOwnedCullingVolumeAllowedToggle.Value = focusedLevelObject.isOwnedCullingVolumeAllowed;
				}
				else
				{
					isOwnedCullingVolumeAllowedToggle.IsVisible = false;
				}
#endif // !DEDICATED_SERVER

				materialPaletteOverrideField.IsVisible = true;
				materialIndexOverrideField.IsVisible = true;
				materialPaletteOverrideField.Text = focusedLevelObject.customMaterialOverride.ToString();
				materialIndexOverrideField.Value = focusedLevelObject.materialIndexOverride;
			}
			else
			{
				isOwnedCullingVolumeAllowedToggle.IsVisible = false;
				materialPaletteOverrideField.IsVisible = false;
				materialIndexOverrideField.IsVisible = false;
			}
		}

		private static void updateSelection(string search, bool large, bool medium, bool small, bool barricades, bool structures, bool npcs)
		{
			if (assets == null)
			{
				return;
			}
			assets.Clear();

			EditorObjectSearchFilter filter = EditorObjectSearchFilter.parse(search);

			if (large || medium || small || npcs)
			{
				tempObjectAssets.Clear();
				Assets.find(tempObjectAssets);
				foreach (ObjectAsset objectAsset in tempObjectAssets)
				{
					if (!large && objectAsset.type == EObjectType.LARGE)
					{
						continue;
					}

					if (!medium && objectAsset.type == EObjectType.MEDIUM)
					{
						continue;
					}

					if (!small && (objectAsset.type == EObjectType.SMALL || objectAsset.type == EObjectType.DECAL))
					{
						continue;
					}

					if (!npcs && objectAsset.type == EObjectType.NPC)
					{
						continue;
					}

					if (filter != null)
					{
						if (filter.ignores(objectAsset))
						{
							continue;
						}
					}

					assets.Add(objectAsset);
				}
			}

			if (barricades || structures)
			{
				List<ItemAsset> itemAssets = new List<ItemAsset>();
				Assets.find(itemAssets);
				foreach (ItemAsset itemAsset in itemAssets)
				{
					if (itemAsset is ItemBarricadeAsset)
					{
						if (!barricades)
						{
							continue;
						}
					}
					else if (itemAsset is ItemStructureAsset)
					{
						if (!structures)
						{
							continue;
						}
					}
					else
					{
						continue;
					}

					if (filter != null)
					{
						if (filter.ignores(itemAsset))
						{
							continue;
						}
					}

					assets.Add(itemAsset);
				}
			}

			assets.Sort(comparator);

			assetsScrollBox.NotifyDataChanged();
		}

		private static void onAssetsRefreshed()
		{
			updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
		}

		private static ISleekElement onCreateAssetButton(Asset item)
		{
			string name = string.Empty;

			ObjectAsset objectAsset = item as ObjectAsset;
			ItemAsset itemAsset = item as ItemAsset;

			if (objectAsset != null)
			{
				name = objectAsset.objectName;
			}
			else if (itemAsset != null)
			{
				name = itemAsset.itemName;
			}

			ISleekButton assetButton = Glazier.Get().CreateButton();
			assetButton.Text = name;
			assetButton.OnClicked += onClickedAssetButton;
			return assetButton;
		}

		private static void onClickedAssetButton(ISleekElement button)
		{
			int index = Mathf.FloorToInt(button.PositionOffset_Y / 40);

			EditorObjects.selectedObjectAsset = assets[index] as ObjectAsset;
			EditorObjects.selectedItemAsset = assets[index] as ItemAsset;

			if (EditorObjects.selectedObjectAsset != null)
			{
				selectedBox.Text = EditorObjects.selectedObjectAsset.objectName;
			}
			else if (EditorObjects.selectedItemAsset != null)
			{
				selectedBox.Text = EditorObjects.selectedItemAsset.itemName;
			}
		}

		private static void onDragStarted(Vector2 minViewportPoint, Vector2 maxViewportPoint)
		{
			Vector2 minPosition = EditorUI.window.ViewportToNormalizedPosition(minViewportPoint);
			Vector2 maxPosition = EditorUI.window.ViewportToNormalizedPosition(maxViewportPoint);
			if (maxPosition.y < minPosition.y)
			{
				float temp = maxPosition.y;
				maxPosition.y = minPosition.y;
				minPosition.y = temp;
			}

			dragBox.PositionScale_X = minPosition.x;
			dragBox.PositionScale_Y = minPosition.y;
			dragBox.SizeScale_X = maxPosition.x - minPosition.x;
			dragBox.SizeScale_Y = maxPosition.y - minPosition.y;

			dragBox.IsVisible = true;
		}

		private static void onDragStopped()
		{
			dragBox.IsVisible = false;
		}

		private static void onEnteredSearchField(ISleekField field)
		{
			updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
		}

		private static void onClickedSearchButton(ISleekElement button)
		{
			updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
		}

		private static void onToggledLargeToggle(ISleekToggle toggle, bool state)
		{
			updateSelection(searchField.Text, state, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
		}

		private static void onToggledMediumToggle(ISleekToggle toggle, bool state)
		{
			updateSelection(searchField.Text, largeToggle.Value, state, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
		}

		private static void onToggledSmallToggle(ISleekToggle toggle, bool state)
		{
			updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, state, barricadesToggle.Value, structuresToggle.Value, npcsToggle.Value);
		}

		private static void onToggledBarricadesToggle(ISleekToggle toggle, bool state)
		{
			updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, state, structuresToggle.Value, npcsToggle.Value);
		}

		private static void onToggledStructuresToggle(ISleekToggle toggle, bool state)
		{
			updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, state, npcsToggle.Value);
		}

		private static void onToggledNPCsToggle(ISleekToggle toggle, bool state)
		{
			updateSelection(searchField.Text, largeToggle.Value, mediumToggle.Value, smallToggle.Value, barricadesToggle.Value, structuresToggle.Value, state);
		}

		private static void OnIsOwnedCullingVolumeAllowedChanged(ISleekToggle toggle, bool value)
		{
			foreach (GameObject gameObject in EditorObjects.EnumerateSelectedGameObjects())
			{
				LevelObject levelObject = LevelObjects.FindLevelObject(gameObject);
				if (levelObject != null)
				{
					levelObject.isOwnedCullingVolumeAllowed = value;
					levelObject.ReapplyOwnedCullingVolumeAllowed();
				}
			}
		}

		private static void OnTypedMaterialPaletteOverride(ISleekField field, string value)
		{
			AssetReference<MaterialPaletteAsset> assetRef = new AssetReference<MaterialPaletteAsset>(value);
			foreach (GameObject gameObject in EditorObjects.EnumerateSelectedGameObjects())
			{
				LevelObject levelObject = LevelObjects.FindLevelObject(gameObject);
				if (levelObject != null)
				{
					levelObject.customMaterialOverride = assetRef;
					levelObject.ReapplyMaterialOverrides();
				}
			}
		}

		private static void OnTypedMaterialIndexOverride(ISleekInt32Field field, int value)
		{
			foreach (GameObject gameObject in EditorObjects.EnumerateSelectedGameObjects())
			{
				LevelObject levelObject = LevelObjects.FindLevelObject(gameObject);
				if (levelObject != null)
				{
					levelObject.materialIndexOverride = value;
					levelObject.ReapplyMaterialOverrides();
				}
			}
		}

		private static void onTypedSnapTransformField(ISleekFloat32Field field, float value)
		{
			EditorObjects.snapTransform = value;
		}

		private static void onTypedSnapRotationField(ISleekFloat32Field field, float value)
		{
			EditorObjects.snapRotation = value;
		}

		private static void onClickedTransformButton(ISleekElement button)
		{
			EditorObjects.dragMode = EDragMode.TRANSFORM;
		}

		private static void onClickedRotateButton(ISleekElement button)
		{
			EditorObjects.dragMode = EDragMode.ROTATE;
		}

		private static void onClickedScaleButton(ISleekElement button)
		{
			EditorObjects.dragMode = EDragMode.SCALE;
		}

		private static void onSwappedStateCoordinate(SleekButtonState button, int index)
		{
			EditorObjects.dragCoordinate = (EDragCoordinate) index;
		}

		public override void OnDestroy()
		{
			Assets.onAssetsRefreshed -= onAssetsRefreshed;
		}

		public EditorLevelObjectsUI()
		{
			Local localization = Localization.read("/Editor/EditorLevelObjects.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorLevelObjects");

			container = this;
			active = false;

			assets = new List<Asset>();

			selectedBox = Glazier.Get().CreateBox();
			selectedBox.PositionOffset_X = -230;
			selectedBox.PositionScale_X = 1;
			selectedBox.SizeOffset_X = 230;
			selectedBox.SizeOffset_Y = 30;
			selectedBox.AddLabel(localization.format("SelectionBoxLabelText"), ESleekSide.LEFT);
			AddChild(selectedBox);

			searchField = Glazier.Get().CreateStringField();
			searchField.PositionOffset_X = -230;
			searchField.PositionOffset_Y = 40;
			searchField.PositionScale_X = 1;
			searchField.SizeOffset_X = 160;
			searchField.SizeOffset_Y = 30;
			searchField.PlaceholderText = localization.format("Search_Field_Hint");
			searchField.OnTextSubmitted += onEnteredSearchField;
			AddChild(searchField);

			searchButton = Glazier.Get().CreateButton();
			searchButton.PositionOffset_X = -60;
			searchButton.PositionOffset_Y = 40;
			searchButton.PositionScale_X = 1;
			searchButton.SizeOffset_X = 60;
			searchButton.SizeOffset_Y = 30;
			searchButton.Text = localization.format("Search");
			searchButton.TooltipText = localization.format("Search_Tooltip");
			searchButton.OnClicked += onClickedSearchButton;
			AddChild(searchButton);

			largeToggle = Glazier.Get().CreateToggle();
			largeToggle.PositionOffset_X = -230;
			largeToggle.PositionOffset_Y = 80;
			largeToggle.PositionScale_X = 1;
			largeToggle.SizeOffset_X = 40;
			largeToggle.SizeOffset_Y = 40;
			largeToggle.AddLabel(localization.format("LargeLabel"), ESleekSide.RIGHT);
			largeToggle.Value = true;
			largeToggle.OnValueChanged += onToggledLargeToggle;
			AddChild(largeToggle);

			mediumToggle = Glazier.Get().CreateToggle();
			mediumToggle.PositionOffset_X = -230;
			mediumToggle.PositionOffset_Y = 130;
			mediumToggle.PositionScale_X = 1;
			mediumToggle.SizeOffset_X = 40;
			mediumToggle.SizeOffset_Y = 40;
			mediumToggle.AddLabel(localization.format("MediumLabel"), ESleekSide.RIGHT);
			mediumToggle.Value = true;
			mediumToggle.OnValueChanged += onToggledMediumToggle;
			AddChild(mediumToggle);

			smallToggle = Glazier.Get().CreateToggle();
			smallToggle.PositionOffset_X = -230;
			smallToggle.PositionOffset_Y = 180;
			smallToggle.PositionScale_X = 1;
			smallToggle.SizeOffset_X = 40;
			smallToggle.SizeOffset_Y = 40;
			smallToggle.AddLabel(localization.format("SmallLabel"), ESleekSide.RIGHT);
			smallToggle.Value = true;
			smallToggle.OnValueChanged += onToggledSmallToggle;
			AddChild(smallToggle);

			barricadesToggle = Glazier.Get().CreateToggle();
			barricadesToggle.PositionOffset_X = -130;
			barricadesToggle.PositionOffset_Y = 80;
			barricadesToggle.PositionScale_X = 1;
			barricadesToggle.SizeOffset_X = 40;
			barricadesToggle.SizeOffset_Y = 40;
			barricadesToggle.AddLabel(localization.format("BarricadesLabel"), ESleekSide.RIGHT);
			barricadesToggle.Value = false;
			barricadesToggle.OnValueChanged += onToggledBarricadesToggle;
			AddChild(barricadesToggle);

			structuresToggle = Glazier.Get().CreateToggle();
			structuresToggle.PositionOffset_X = -130;
			structuresToggle.PositionOffset_Y = 130;
			structuresToggle.PositionScale_X = 1;
			structuresToggle.SizeOffset_X = 40;
			structuresToggle.SizeOffset_Y = 40;
			structuresToggle.AddLabel(localization.format("StructuresLabel"), ESleekSide.RIGHT);
			structuresToggle.Value = false;
			structuresToggle.OnValueChanged += onToggledStructuresToggle;
			AddChild(structuresToggle);

			npcsToggle = Glazier.Get().CreateToggle();
			npcsToggle.PositionOffset_X = -130;
			npcsToggle.PositionOffset_Y = 180;
			npcsToggle.PositionScale_X = 1;
			npcsToggle.SizeOffset_X = 40;
			npcsToggle.SizeOffset_Y = 40;
			npcsToggle.AddLabel(localization.format("NPCsLabel"), ESleekSide.RIGHT);
			npcsToggle.Value = false;
			npcsToggle.OnValueChanged += onToggledNPCsToggle;
			AddChild(npcsToggle);

			assetsScrollBox = new SleekList<Asset>();
			assetsScrollBox.PositionOffset_X = -230;
			assetsScrollBox.PositionOffset_Y = 230;
			assetsScrollBox.PositionScale_X = 1;
			assetsScrollBox.SizeOffset_X = 230;
			assetsScrollBox.SizeOffset_Y = -230;
			assetsScrollBox.SizeScale_Y = 1;
			assetsScrollBox.itemHeight = 30;
			assetsScrollBox.itemPadding = 10;
			assetsScrollBox.onCreateElement = onCreateAssetButton;
			assetsScrollBox.SetData(assets);
			AddChild(assetsScrollBox);

			EditorObjects.selectedObjectAsset = null;
			EditorObjects.selectedItemAsset = null;

			EditorObjects.onDragStarted = onDragStarted;
			EditorObjects.onDragStopped = onDragStopped;

			dragBox = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			dragBox.TintColor = new Color(1.0f, 1.0f, 0.0f, 0.2f);
			EditorUI.window.AddChild(dragBox);
			dragBox.IsVisible = false;

			isOwnedCullingVolumeAllowedToggle = Glazier.Get().CreateToggle();
			isOwnedCullingVolumeAllowedToggle.PositionOffset_Y = -350;
			isOwnedCullingVolumeAllowedToggle.PositionScale_Y = 1.0f;
			isOwnedCullingVolumeAllowedToggle.AddLabel(localization.format("IsOwnedCullingVolumeAllowed_Label"), ESleekSide.RIGHT);
			isOwnedCullingVolumeAllowedToggle.TooltipText = localization.format("IsOwnedCullingVolumeAllowed_Tooltip");
			isOwnedCullingVolumeAllowedToggle.OnValueChanged += OnIsOwnedCullingVolumeAllowedChanged;
			isOwnedCullingVolumeAllowedToggle.IsVisible = false;
			AddChild(isOwnedCullingVolumeAllowedToggle);

			materialPaletteOverrideField = Glazier.Get().CreateStringField();
			materialPaletteOverrideField.PositionOffset_Y = -310;
			materialPaletteOverrideField.PositionScale_Y = 1.0f;
			materialPaletteOverrideField.SizeOffset_X = 200;
			materialPaletteOverrideField.SizeOffset_Y = 30;
			materialPaletteOverrideField.AddLabel(localization.format("MaterialPaletteOverride_Label"), ESleekSide.RIGHT);
			materialPaletteOverrideField.TooltipText = localization.format("MaterialPaletteOverride_Tooltip");
			materialPaletteOverrideField.OnTextChanged += OnTypedMaterialPaletteOverride;
			materialPaletteOverrideField.IsVisible = false;
			AddChild(materialPaletteOverrideField);

			materialIndexOverrideField = Glazier.Get().CreateInt32Field();
			materialIndexOverrideField.PositionOffset_Y = -270;
			materialIndexOverrideField.PositionScale_Y = 1.0f;
			materialIndexOverrideField.SizeOffset_X = 200;
			materialIndexOverrideField.SizeOffset_Y = 30;
			materialIndexOverrideField.AddLabel(localization.format("MaterialIndexOverride_Label"), ESleekSide.RIGHT);
			materialIndexOverrideField.TooltipText = localization.format("MaterialIndexOverride_Tooltip");
			materialIndexOverrideField.OnValueChanged += OnTypedMaterialIndexOverride;
			materialIndexOverrideField.IsVisible = false;
			AddChild(materialIndexOverrideField);

			snapTransformField = Glazier.Get().CreateFloat32Field();
			snapTransformField.PositionOffset_Y = -230;
			snapTransformField.PositionScale_Y = 1;
			snapTransformField.SizeOffset_X = 200;
			snapTransformField.SizeOffset_Y = 30;
			snapTransformField.Value = EditorObjects.snapTransform;
			snapTransformField.AddLabel(localization.format("SnapTransformLabelText"), ESleekSide.RIGHT);
			snapTransformField.OnValueChanged += onTypedSnapTransformField;
			AddChild(snapTransformField);

			snapRotationField = Glazier.Get().CreateFloat32Field();
			snapRotationField.PositionOffset_Y = -190;
			snapRotationField.PositionScale_Y = 1;
			snapRotationField.SizeOffset_X = 200;
			snapRotationField.SizeOffset_Y = 30;
			snapRotationField.Value = EditorObjects.snapRotation;
			snapRotationField.AddLabel(localization.format("SnapRotationLabelText"), ESleekSide.RIGHT);
			snapRotationField.OnValueChanged += onTypedSnapRotationField;
			AddChild(snapRotationField);

			transformButton = new SleekButtonIcon(icons.load<Texture2D>("Transform"));
			transformButton.PositionOffset_Y = -150;
			transformButton.PositionScale_Y = 1;
			transformButton.SizeOffset_X = 200;
			transformButton.SizeOffset_Y = 30;
			transformButton.text = localization.format("TransformButtonText", ControlsSettings.tool_0);
			transformButton.tooltip = localization.format("TransformButtonTooltip");
			transformButton.onClickedButton += onClickedTransformButton;
			AddChild(transformButton);

			rotateButton = new SleekButtonIcon(icons.load<Texture2D>("Rotate"));
			rotateButton.PositionOffset_Y = -110;
			rotateButton.PositionScale_Y = 1;
			rotateButton.SizeOffset_X = 200;
			rotateButton.SizeOffset_Y = 30;
			rotateButton.text = localization.format("RotateButtonText", ControlsSettings.tool_1);
			rotateButton.tooltip = localization.format("RotateButtonTooltip");
			rotateButton.onClickedButton += onClickedRotateButton;
			AddChild(rotateButton);

			scaleButton = new SleekButtonIcon(icons.load<Texture2D>("Scale"));
			scaleButton.PositionOffset_Y = -70;
			scaleButton.PositionScale_Y = 1;
			scaleButton.SizeOffset_X = 200;
			scaleButton.SizeOffset_Y = 30;
			scaleButton.text = localization.format("ScaleButtonText", ControlsSettings.tool_3);
			scaleButton.tooltip = localization.format("ScaleButtonTooltip");
			scaleButton.onClickedButton += onClickedScaleButton;
			AddChild(scaleButton);

			coordinateButton = new SleekButtonState(new GUIContent(localization.format("CoordinateButtonTextGlobal"), icons.load<Texture>("Global")), new GUIContent(localization.format("CoordinateButtonTextLocal"), icons.load<Texture>("Local")));
			coordinateButton.PositionOffset_Y = -30;
			coordinateButton.PositionScale_Y = 1;
			coordinateButton.SizeOffset_X = 200;
			coordinateButton.SizeOffset_Y = 30;
			coordinateButton.tooltip = localization.format("CoordinateButtonTooltip");
			coordinateButton.onSwappedState = onSwappedStateCoordinate;
			AddChild(coordinateButton);

			onAssetsRefreshed();
			Assets.onAssetsRefreshed += onAssetsRefreshed;
		}

		private GameObject focusedGameObject;
		private static LevelObject focusedLevelObject;
	}
}
