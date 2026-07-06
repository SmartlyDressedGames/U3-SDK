////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Landscapes;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal class EditorTerrainMaterialsUI : SleekFullscreenBox
	{
		public void Open()
		{
			AnimateIntoView();

			TerrainEditor.toolMode = TerrainEditor.EDevkitLandscapeToolMode.SPLATMAP;
			EditorInteract.instance.SetActiveTool(EditorInteract.instance.terrainTool);

			if (SDG.Framework.Foliage.FoliageSystem.instance != null)
				SDG.Framework.Foliage.FoliageSystem.instance.hiddenByMaterialEditor = true;

			RefreshAssets();
		}

		public void Close()
		{
			AnimateOutOfView(1.0f, 0.0f);

			DevkitLandscapeToolSplatmapOptions.save();

			EditorInteract.instance.SetActiveTool(null);

			if (SDG.Framework.Foliage.FoliageSystem.instance != null)
				SDG.Framework.Foliage.FoliageSystem.instance.hiddenByMaterialEditor = false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			// These values need updating because they can be adjusted by hotkey.
			modeButton.state = (int) TerrainEditor.splatmapMode;
			brushRadiusField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
			brushFalloffField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
			brushStrengthField.Value = EditorInteract.instance.terrainTool.splatmapBrushStrength;
			weightTargetField.Value = DevkitLandscapeToolSplatmapOptions.instance.weightTarget;

			LandscapeMaterialAsset materialAsset = TerrainEditor.splatmapMaterialTarget.Find();
			if (selectedMaterialAsset != materialAsset)
			{
				selectedMaterialAsset = materialAsset;

				if (selectedMaterialAsset != null)
				{
					selectedAssetBox.icon = Assets.load(selectedMaterialAsset.texture);
					selectedAssetBox.text = selectedMaterialAsset.FriendlyName;
				}
				else
				{
					selectedAssetBox.icon = null;
					selectedAssetBox.text = string.Empty;
				}
			}

			if (TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT)
			{
				hintLabel.Text = localization.format("Hint_Paint", "Shift", "Ctrl", "Alt");
				hintLabel.IsVisible = true;
			}
			else if (TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.CUT)
			{
				hintLabel.Text = localization.format("Hint_Cut", "Shift");
				hintLabel.IsVisible = true;
			}
			else
			{
				hintLabel.IsVisible = false;
			}

			UpdateLowerLeftOffset();
		}

		public EditorTerrainMaterialsUI() : base()
		{
			localization = Localization.read("/Editor/EditorTerrainMaterials.dat");

			DevkitLandscapeToolSplatmapOptions.load();

			searchAssets = new List<LandscapeMaterialAsset>();

			hintLabel = Glazier.Get().CreateLabel();
			hintLabel.PositionScale_Y = 1.0f;
			hintLabel.PositionOffset_Y = -30;
			hintLabel.SizeScale_X = 1.0f;
			hintLabel.SizeOffset_Y = 30;
			hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			hintLabel.IsVisible = false;
			AddChild(hintLabel);

			modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Paint", "Q")),
				new GUIContent(localization.format("Mode_Auto", "W")),
				new GUIContent(localization.format("Mode_Smooth", "E")),
				new GUIContent(localization.format("Mode_Cut", "R")));
			modeButton.PositionScale_Y = 1.0f;
			modeButton.SizeOffset_X = 200;
			modeButton.SizeOffset_Y = 30;
			modeButton.AddLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
			modeButton.state = (int) TerrainEditor.splatmapMode;
			modeButton.onSwappedState += OnSwappedMode;
			AddChild(modeButton);

			brushRadiusField = Glazier.Get().CreateFloat32Field();
			brushRadiusField.PositionScale_Y = 1.0f;
			brushRadiusField.SizeOffset_X = 200;
			brushRadiusField.SizeOffset_Y = 30;
			brushRadiusField.AddLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
			brushRadiusField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
			brushRadiusField.OnValueChanged += OnBrushRadiusTyped;
			AddChild(brushRadiusField);

			brushFalloffField = Glazier.Get().CreateFloat32Field();
			brushFalloffField.PositionScale_Y = 1.0f;
			brushFalloffField.SizeOffset_X = 200;
			brushFalloffField.SizeOffset_Y = 30;
			brushFalloffField.AddLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
			brushFalloffField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
			brushFalloffField.OnValueChanged += OnBrushFalloffTyped;
			AddChild(brushFalloffField);

			brushStrengthField = Glazier.Get().CreateFloat32Field();
			brushStrengthField.PositionScale_Y = 1.0f;
			brushStrengthField.SizeOffset_X = 200;
			brushStrengthField.SizeOffset_Y = 30;
			brushStrengthField.AddLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
			brushStrengthField.Value = DevkitLandscapeToolSplatmapOptions.instance.brushStrength;
			brushStrengthField.OnValueChanged += OnBrushStrengthTyped;
			AddChild(brushStrengthField);

			smoothMethodButton = new SleekButtonState(new GUIContent(localization.format("SmoothMethod_BrushAverage")), new GUIContent(localization.format("SmoothMethod_PixelAverage")));
			smoothMethodButton.PositionScale_Y = 1.0f;
			smoothMethodButton.SizeOffset_X = 200;
			smoothMethodButton.SizeOffset_Y = 30;
			smoothMethodButton.AddLabel(localization.format("SmoothMethod_Label"), ESleekSide.RIGHT);
			smoothMethodButton.state = (int) DevkitLandscapeToolSplatmapOptions.instance.smoothMethod;
			smoothMethodButton.onSwappedState += OnSwappedSmoothMethod;
			AddChild(smoothMethodButton);

			autoRayMaskField = Glazier.Get().CreateUInt32Field();
			autoRayMaskField.PositionScale_Y = 1.0f;
			autoRayMaskField.SizeOffset_X = 200;
			autoRayMaskField.SizeOffset_Y = 30;
			autoRayMaskField.AddLabel("Ray Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
			autoRayMaskField.Value = (uint) DevkitLandscapeToolSplatmapOptions.instance.autoRayMask;
			autoRayMaskField.OnValueChanged += OnAutoRayMaskTyped;
			AddChild(autoRayMaskField);

			autoRayLengthField = Glazier.Get().CreateFloat32Field();
			autoRayLengthField.PositionScale_Y = 1.0f;
			autoRayLengthField.SizeOffset_X = 200;
			autoRayLengthField.SizeOffset_Y = 30;
			autoRayLengthField.AddLabel(localization.format("AutoRayLength"), ESleekSide.RIGHT);
			autoRayLengthField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoRayLength;
			autoRayLengthField.OnValueChanged += OnAutoRayLengthTyped;
			AddChild(autoRayLengthField);

			autoRayRadiusField = Glazier.Get().CreateFloat32Field();
			autoRayRadiusField.PositionScale_Y = 1.0f;
			autoRayRadiusField.SizeOffset_X = 200;
			autoRayRadiusField.SizeOffset_Y = 30;
			autoRayRadiusField.AddLabel(localization.format("AutoRayRadius"), ESleekSide.RIGHT);
			autoRayRadiusField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoRayRadius;
			autoRayRadiusField.OnValueChanged += OnAutoRayRadiusTyped;
			AddChild(autoRayRadiusField);

			useAutoFoundationToggle = Glazier.Get().CreateToggle();
			useAutoFoundationToggle.PositionScale_Y = 1.0f;
			useAutoFoundationToggle.SizeOffset_X = 40;
			useAutoFoundationToggle.SizeOffset_Y = 40;
			useAutoFoundationToggle.Value = DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation;
			useAutoFoundationToggle.OnValueChanged += OnClickedUseAutoFoundation;
			useAutoFoundationToggle.AddLabel(localization.format("UseAutoFoundation"), ESleekSide.RIGHT);
			AddChild(useAutoFoundationToggle);

			autoMaxAngleBeginField = Glazier.Get().CreateFloat32Field();
			autoMaxAngleBeginField.PositionScale_Y = 1.0f;
			autoMaxAngleBeginField.SizeOffset_X = 100;
			autoMaxAngleBeginField.SizeOffset_Y = 30;
			autoMaxAngleBeginField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin;
			autoMaxAngleBeginField.OnValueChanged += OnAutoMaxAngleBeginTyped;
			AddChild(autoMaxAngleBeginField);
			autoMaxAngleEndField = Glazier.Get().CreateFloat32Field();
			autoMaxAngleEndField.PositionOffset_X = 100;
			autoMaxAngleEndField.PositionScale_Y = 1.0f;
			autoMaxAngleEndField.SizeOffset_X = 100;
			autoMaxAngleEndField.SizeOffset_Y = 30;
			autoMaxAngleEndField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd;
			autoMaxAngleEndField.OnValueChanged += OnAutoMaxAngleEndTyped;
			autoMaxAngleEndField.AddLabel(localization.format("MaxAngleRange"), ESleekSide.RIGHT);
			AddChild(autoMaxAngleEndField);

			autoMinAngleBeginField = Glazier.Get().CreateFloat32Field();
			autoMinAngleBeginField.PositionScale_Y = 1.0f;
			autoMinAngleBeginField.SizeOffset_X = 100;
			autoMinAngleBeginField.SizeOffset_Y = 30;
			autoMinAngleBeginField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin;
			autoMinAngleBeginField.OnValueChanged += OnAutoMinAngleBeginTyped;
			AddChild(autoMinAngleBeginField);
			autoMinAngleEndField = Glazier.Get().CreateFloat32Field();
			autoMinAngleEndField.PositionOffset_X = 100;
			autoMinAngleEndField.PositionScale_Y = 1.0f;
			autoMinAngleEndField.SizeOffset_X = 100;
			autoMinAngleEndField.SizeOffset_Y = 30;
			autoMinAngleEndField.Value = DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd;
			autoMinAngleEndField.OnValueChanged += OnAutoMinAngleEndTyped;
			autoMinAngleEndField.AddLabel(localization.format("MinAngleRange"), ESleekSide.RIGHT);
			AddChild(autoMinAngleEndField);

			useAutoSlopeToggle = Glazier.Get().CreateToggle();
			useAutoSlopeToggle.PositionScale_Y = 1.0f;
			useAutoSlopeToggle.SizeOffset_X = 40;
			useAutoSlopeToggle.SizeOffset_Y = 40;
			useAutoSlopeToggle.Value = DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope;
			useAutoSlopeToggle.OnValueChanged += OnClickedUseAutoSlope;
			useAutoSlopeToggle.AddLabel(localization.format("UseAutoSlope"), ESleekSide.RIGHT);
			AddChild(useAutoSlopeToggle);

			useWeightTargetToggle = Glazier.Get().CreateToggle();
			useWeightTargetToggle.PositionScale_Y = 1.0f;
			useWeightTargetToggle.SizeOffset_X = 40;
			useWeightTargetToggle.SizeOffset_Y = 40;
			useWeightTargetToggle.Value = DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget;
			useWeightTargetToggle.OnValueChanged += OnClickedUseWeightTarget;
			AddChild(useWeightTargetToggle);
			weightTargetField = Glazier.Get().CreateFloat32Field();
			weightTargetField.PositionOffset_X = 40;
			weightTargetField.PositionScale_Y = 1.0f;
			weightTargetField.SizeOffset_X = 160;
			weightTargetField.SizeOffset_Y = 30;
			weightTargetField.Value = DevkitLandscapeToolSplatmapOptions.instance.weightTarget;
			weightTargetField.AddLabel(localization.format("WeightTarget", "G"), ESleekSide.RIGHT);
			weightTargetField.OnValueChanged += OnWeightTargetTyped;
			AddChild(weightTargetField);

			maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
			maxPreviewSamplesField.PositionScale_Y = 1.0f;
			maxPreviewSamplesField.SizeOffset_X = 200;
			maxPreviewSamplesField.SizeOffset_Y = 30;
			maxPreviewSamplesField.AddLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
			maxPreviewSamplesField.Value = DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples;
			maxPreviewSamplesField.OnValueChanged += OnMaxPreviewSamplesTyped;
			AddChild(maxPreviewSamplesField);

			previewMethodButton = new SleekButtonState(new GUIContent(localization.format("PreviewMethod_BrushAlpha")), new GUIContent(localization.format("PreviewMethod_Weight")));
			previewMethodButton.PositionScale_Y = 1.0f;
			previewMethodButton.SizeOffset_X = 200;
			previewMethodButton.SizeOffset_Y = 30;
			previewMethodButton.AddLabel(localization.format("PreviewMethod_Label"), ESleekSide.RIGHT);
			previewMethodButton.state = (int) DevkitLandscapeToolSplatmapOptions.instance.previewMethod;
			previewMethodButton.onSwappedState += OnSwappedPreviewMethod;
			AddChild(previewMethodButton);

			highlightHolesToggle = Glazier.Get().CreateToggle();
			highlightHolesToggle.PositionScale_Y = 1.0f;
			highlightHolesToggle.SizeOffset_X = 40;
			highlightHolesToggle.SizeOffset_Y = 40;
			highlightHolesToggle.OnValueChanged += OnClickedHighlightHoles;
			highlightHolesToggle.IsVisible = false;
			highlightHolesToggle.AddLabel(localization.format("HighlightHoles_Label"), ESleekSide.RIGHT);
			AddChild(highlightHolesToggle);

			UpdateLowerLeftOffset();

			const int rightColumnWidth = 300;
			float upperRightOffset = 0;

			selectedAssetBox = new SleekBoxIcon(null, 64);
			selectedAssetBox.PositionScale_X = 1.0f;
			selectedAssetBox.SizeOffset_X = rightColumnWidth;
			selectedAssetBox.PositionOffset_X = -selectedAssetBox.SizeOffset_X;
			selectedAssetBox.SizeOffset_Y = 74;
			selectedAssetBox.AddLabel(localization.format("SelectedAsset", "Alt"), ESleekSide.LEFT);
			AddChild(selectedAssetBox);
			upperRightOffset += selectedAssetBox.SizeOffset_Y + 10;

			onlyUsedMaterialsToggle = Glazier.Get().CreateToggle();
			onlyUsedMaterialsToggle.PositionScale_X = 1.0f;
			onlyUsedMaterialsToggle.SizeOffset_X = 40;
			onlyUsedMaterialsToggle.PositionOffset_X = -rightColumnWidth;
			onlyUsedMaterialsToggle.SizeOffset_Y = 40;
			onlyUsedMaterialsToggle.PositionOffset_Y = upperRightOffset;
			onlyUsedMaterialsToggle.AddLabel(localization.format("OnlyUsedMaterials"), ESleekSide.RIGHT);
			onlyUsedMaterialsToggle.Value = true;
			onlyUsedMaterialsToggle.OnValueChanged += OnClickedOnlyUsedMaterials;
			AddChild(onlyUsedMaterialsToggle);
			upperRightOffset += onlyUsedMaterialsToggle.SizeOffset_Y + 10;

			searchField = Glazier.Get().CreateStringField();
			searchField.PositionOffset_X = -rightColumnWidth;
			searchField.PositionOffset_Y = upperRightOffset;
			searchField.PositionScale_X = 1.0f;
			searchField.SizeOffset_X = rightColumnWidth;
			searchField.SizeOffset_Y = 30;
			searchField.PlaceholderText = localization.format("SearchHint");
			searchField.OnTextSubmitted += OnNameFilterEntered;
			AddChild(searchField);
			upperRightOffset += searchField.SizeOffset_Y + 10;

			assetScrollView = Glazier.Get().CreateScrollView();
			assetScrollView.PositionScale_X = 1.0f;
			assetScrollView.SizeOffset_X = rightColumnWidth;
			assetScrollView.PositionOffset_X = -assetScrollView.SizeOffset_X;
			assetScrollView.PositionOffset_Y = upperRightOffset;
			assetScrollView.SizeOffset_Y = -upperRightOffset;
			assetScrollView.SizeScale_Y = 1.0f;
			assetScrollView.ScaleContentToWidth = true;
			AddChild(assetScrollView);
		}

		private void OnSwappedMode(SleekButtonState element, int index)
		{
			TerrainEditor.splatmapMode = (TerrainEditor.EDevkitLandscapeToolSplatmapMode) index;
		}

		private void OnBrushStrengthTyped(ISleekFloat32Field field, float state)
		{
			EditorInteract.instance.terrainTool.splatmapBrushStrength = state;
		}

		private void OnBrushFalloffTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.brushFalloff = state;
		}

		private void OnBrushRadiusTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.brushRadius = state;
		}

		private void OnMaxPreviewSamplesTyped(ISleekUInt32Field field, uint state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples = state;
		}

		private void OnClickedUseWeightTarget(ISleekToggle toggle, bool state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget = state;
		}

		private void OnWeightTargetTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.weightTarget = state;
		}

		private void OnSwappedSmoothMethod(SleekButtonState element, int index)
		{
			DevkitLandscapeToolSplatmapOptions.instance.smoothMethod = (EDevkitLandscapeToolSplatmapSmoothMethod) index;
		}

		private void OnSwappedPreviewMethod(SleekButtonState element, int index)
		{
			DevkitLandscapeToolSplatmapOptions.instance.previewMethod = (EDevkitLandscapeToolSplatmapPreviewMethod) index;
		}

		private void OnClickedUseAutoSlope(ISleekToggle toggle, bool state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope = state;
		}

		private void OnAutoMinAngleBeginTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin = state;
		}

		private void OnAutoMinAngleEndTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd = state;
		}

		private void OnAutoMaxAngleBeginTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin = state;
		}

		private void OnAutoMaxAngleEndTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd = state;
		}

		private void OnClickedUseAutoFoundation(ISleekToggle toggle, bool state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation = state;
		}

		private void OnAutoRayRadiusTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.autoRayRadius = state;
		}

		private void OnAutoRayLengthTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.autoRayLength = state;
		}

		private void OnAutoRayMaskTyped(ISleekUInt32Field field, uint state)
		{
			DevkitLandscapeToolSplatmapOptions.instance.autoRayMask = (ERayMask) state;
		}

		private void OnClickedHighlightHoles(ISleekToggle toggle, bool state)
		{
			Landscape.HighlightHoles = state;
		}

		private void OnClickedOnlyUsedMaterials(ISleekToggle toggle, bool state)
		{
			RefreshAssets();
		}

		private void OnNameFilterEntered(ISleekField field)
		{
			RefreshAssets();
		}

		private void OnAssetClicked(ISleekElement button)
		{
			int index = assetScrollView.FindIndexOfChild(button);
			TerrainEditor.splatmapMaterialTarget = new AssetReference<LandscapeMaterialAsset>(searchAssets[index].GUID);
		}

		private void RefreshAssets()
		{
			searchAssets.Clear();
			assetScrollView.RemoveAllChildren();

			float offset = 0;

			if (onlyUsedMaterialsToggle.Value)
			{
				Landscape.GetUniqueMaterials(searchAssets);
			}
			else
			{
				Assets.find(searchAssets);
			}

			string searchText = searchField.Text;
			if (!string.IsNullOrEmpty(searchText))
			{
				searchAssets.RemoveSwap((LandscapeMaterialAsset asset) =>
				{
					return asset.FriendlyName.IndexOf(searchText, System.StringComparison.CurrentCultureIgnoreCase) == -1;
				});
			}

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

		private void UpdateLowerLeftOffset()
		{
			float lowerLeftOffset = 0;
			const float lowerLeftPadding = 10;

			lowerLeftOffset -= modeButton.SizeOffset_Y;
			modeButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			lowerLeftOffset -= previewMethodButton.SizeOffset_Y;
			previewMethodButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			lowerLeftOffset -= maxPreviewSamplesField.SizeOffset_Y;
			maxPreviewSamplesField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			smoothMethodButton.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.SMOOTH;
			if (smoothMethodButton.IsVisible)
			{
				lowerLeftOffset -= smoothMethodButton.SizeOffset_Y;
				smoothMethodButton.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			autoRayRadiusField.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT
				&& DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation;
			autoRayLengthField.IsVisible = autoRayRadiusField.IsVisible;
			autoRayMaskField.IsVisible = autoRayRadiusField.IsVisible;
			if (autoRayRadiusField.IsVisible)
			{
				lowerLeftOffset -= autoRayMaskField.SizeOffset_Y;
				autoRayMaskField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= autoRayLengthField.SizeOffset_Y;
				autoRayLengthField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= autoRayRadiusField.SizeOffset_Y;
				autoRayRadiusField.PositionOffset_Y = lowerLeftOffset;
			}

			useAutoFoundationToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT;
			if (useAutoFoundationToggle.IsVisible)
			{
				lowerLeftOffset -= useAutoFoundationToggle.SizeOffset_Y;
				useAutoFoundationToggle.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			autoMinAngleBeginField.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT
				&& DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope;
			autoMinAngleEndField.IsVisible = autoMinAngleBeginField.IsVisible;
			autoMaxAngleBeginField.IsVisible = autoMinAngleBeginField.IsVisible;
			autoMaxAngleEndField.IsVisible = autoMinAngleBeginField.IsVisible;
			if (autoMinAngleBeginField.IsVisible)
			{
				lowerLeftOffset -= autoMaxAngleBeginField.SizeOffset_Y;
				autoMaxAngleBeginField.PositionOffset_Y = lowerLeftOffset;
				autoMaxAngleEndField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= autoMinAngleBeginField.SizeOffset_Y;
				autoMinAngleBeginField.PositionOffset_Y = lowerLeftOffset;
				autoMinAngleEndField.PositionOffset_Y = lowerLeftOffset;
			}

			useAutoSlopeToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT;
			if (useAutoSlopeToggle.IsVisible)
			{
				lowerLeftOffset -= useAutoSlopeToggle.SizeOffset_Y;
				useAutoSlopeToggle.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			useWeightTargetToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.PAINT;
			weightTargetField.IsVisible = useWeightTargetToggle.IsVisible;
			if (useWeightTargetToggle.IsVisible)
			{
				lowerLeftOffset -= useWeightTargetToggle.SizeOffset_Y;
				useWeightTargetToggle.PositionOffset_Y = lowerLeftOffset;
				weightTargetField.PositionOffset_Y = lowerLeftOffset + 5;
				lowerLeftOffset -= lowerLeftPadding;
			}

			brushStrengthField.IsVisible = TerrainEditor.splatmapMode != TerrainEditor.EDevkitLandscapeToolSplatmapMode.CUT;
			brushFalloffField.IsVisible = brushStrengthField.IsVisible;
			if (brushStrengthField.IsVisible)
			{
				lowerLeftOffset -= brushStrengthField.SizeOffset_Y;
				brushStrengthField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;

				lowerLeftOffset -= brushFalloffField.SizeOffset_Y;
				brushFalloffField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			lowerLeftOffset -= brushRadiusField.SizeOffset_Y;
			brushRadiusField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			highlightHolesToggle.IsVisible = TerrainEditor.splatmapMode == TerrainEditor.EDevkitLandscapeToolSplatmapMode.CUT;
			if (highlightHolesToggle.IsVisible)
			{
				lowerLeftOffset -= highlightHolesToggle.SizeOffset_Y;
				highlightHolesToggle.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}
		}

		private Local localization;
		private LandscapeMaterialAsset selectedMaterialAsset;
		private List<LandscapeMaterialAsset> searchAssets;
		private ISleekLabel hintLabel;
		private SleekButtonState modeButton;
		private ISleekFloat32Field brushRadiusField;
		private ISleekFloat32Field brushFalloffField;
		private ISleekFloat32Field brushStrengthField;
		private ISleekToggle useWeightTargetToggle;
		private ISleekFloat32Field weightTargetField;
		private ISleekUInt32Field maxPreviewSamplesField;
		private SleekButtonState smoothMethodButton;
		private SleekButtonState previewMethodButton;
		private ISleekToggle highlightHolesToggle;
		private ISleekToggle useAutoSlopeToggle;
		private ISleekFloat32Field autoMinAngleBeginField;
		private ISleekFloat32Field autoMinAngleEndField;
		private ISleekFloat32Field autoMaxAngleBeginField;
		private ISleekFloat32Field autoMaxAngleEndField;
		private ISleekToggle useAutoFoundationToggle;
		private ISleekFloat32Field autoRayRadiusField;
		private ISleekFloat32Field autoRayLengthField;
		private ISleekUInt32Field autoRayMaskField;
		private SleekBoxIcon selectedAssetBox;
		private ISleekToggle onlyUsedMaterialsToggle;
		private ISleekField searchField;
		private ISleekScrollView assetScrollView;
	}
}
