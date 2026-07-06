////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit.Tools;
using UnityEngine;

namespace SDG.Unturned
{
	internal class EditorTerrainHeightUI : SleekFullscreenBox
	{
		public void Open()
		{
			AnimateIntoView();

			TerrainEditor.toolMode = TerrainEditor.EDevkitLandscapeToolMode.HEIGHTMAP;
			EditorInteract.instance.SetActiveTool(EditorInteract.instance.terrainTool);

			if (SDG.Framework.Foliage.FoliageSystem.instance != null)
				SDG.Framework.Foliage.FoliageSystem.instance.hiddenByHeightEditor = true;
		}

		public void Close()
		{
			AnimateOutOfView(1.0f, 0.0f);

			DevkitLandscapeToolHeightmapOptions.save();

			EditorInteract.instance.SetActiveTool(null);

			if (SDG.Framework.Foliage.FoliageSystem.instance != null)
				SDG.Framework.Foliage.FoliageSystem.instance.hiddenByHeightEditor = false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			// These values need updating because they can be adjusted by hotkey.
			modeButton.state = (int) TerrainEditor.heightmapMode;
			brushRadiusField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
			brushFalloffField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
			brushStrengthField.Value = EditorInteract.instance.terrainTool.heightmapBrushStrength;
			flattenTargetField.Value = DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;

			if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.ADJUST)
			{
				hintLabel.Text = localization.format("Hint_Adjust", "Shift");
				hintLabel.IsVisible = true;
			}
			else if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.FLATTEN)
			{
				hintLabel.Text = localization.format("Hint_Flatten", "Alt");
				hintLabel.IsVisible = true;
			}
			else if (TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.RAMP)
			{
				hintLabel.Text = localization.format("Hint_Ramp", "R");
				hintLabel.IsVisible = true;
			}
			else
			{
				hintLabel.IsVisible = false;
			}

			UpdateLowerLeftOffset();
		}

		public EditorTerrainHeightUI() : base()
		{
			localization = Localization.read("/Editor/EditorTerrainHeight.dat");

			DevkitLandscapeToolHeightmapOptions.load();

			hintLabel = Glazier.Get().CreateLabel();
			hintLabel.PositionScale_Y = 1.0f;
			hintLabel.PositionOffset_Y = -30;
			hintLabel.SizeScale_X = 1.0f;
			hintLabel.SizeOffset_Y = 30;
			hintLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			hintLabel.IsVisible = false;
			AddChild(hintLabel);

			modeButton = new SleekButtonState(new GUIContent(localization.format("Mode_Adjust", "Q")),
				new GUIContent(localization.format("Mode_Flatten", "W")),
				new GUIContent(localization.format("Mode_Smooth", "E")),
				new GUIContent(localization.format("Mode_Ramp", "R")));
			modeButton.PositionScale_Y = 1.0f;
			modeButton.SizeOffset_X = 200;
			modeButton.SizeOffset_Y = 30;
			modeButton.AddLabel(localization.format("Mode_Label"), ESleekSide.RIGHT);
			modeButton.state = (int) TerrainEditor.heightmapMode;
			modeButton.onSwappedState += OnSwappedMode;
			AddChild(modeButton);

			brushRadiusField = Glazier.Get().CreateFloat32Field();
			brushRadiusField.PositionScale_Y = 1.0f;
			brushRadiusField.SizeOffset_X = 200;
			brushRadiusField.SizeOffset_Y = 30;
			brushRadiusField.AddLabel(localization.format("BrushRadius", "B"), ESleekSide.RIGHT);
			brushRadiusField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
			brushRadiusField.OnValueChanged += OnBrushRadiusTyped;
			AddChild(brushRadiusField);

			brushFalloffField = Glazier.Get().CreateFloat32Field();
			brushFalloffField.PositionScale_Y = 1.0f;
			brushFalloffField.SizeOffset_X = 200;
			brushFalloffField.SizeOffset_Y = 30;
			brushFalloffField.AddLabel(localization.format("BrushFalloff", "F"), ESleekSide.RIGHT);
			brushFalloffField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
			brushFalloffField.OnValueChanged += OnBrushFalloffTyped;
			AddChild(brushFalloffField);

			brushStrengthField = Glazier.Get().CreateFloat32Field();
			brushStrengthField.PositionScale_Y = 1.0f;
			brushStrengthField.SizeOffset_X = 200;
			brushStrengthField.SizeOffset_Y = 30;
			brushStrengthField.AddLabel(localization.format("BrushStrength", "V"), ESleekSide.RIGHT);
			brushStrengthField.Value = DevkitLandscapeToolHeightmapOptions.instance.brushStrength;
			brushStrengthField.OnValueChanged += OnBrushStrengthTyped;
			AddChild(brushStrengthField);

			smoothMethodButton = new SleekButtonState(new GUIContent(localization.format("SmoothMethod_BrushAverage")), new GUIContent(localization.format("SmoothMethod_PixelAverage")));
			smoothMethodButton.PositionScale_Y = 1.0f;
			smoothMethodButton.SizeOffset_X = 200;
			smoothMethodButton.SizeOffset_Y = 30;
			smoothMethodButton.AddLabel(localization.format("SmoothMethod_Label"), ESleekSide.RIGHT);
			smoothMethodButton.state = (int) DevkitLandscapeToolHeightmapOptions.instance.smoothMethod;
			smoothMethodButton.onSwappedState += OnSwappedSmoothMethod;
			AddChild(smoothMethodButton);

			flattenTargetField = Glazier.Get().CreateFloat32Field();
			flattenTargetField.PositionScale_Y = 1.0f;
			flattenTargetField.SizeOffset_X = 200;
			flattenTargetField.SizeOffset_Y = 30;
			flattenTargetField.AddLabel(localization.format("FlattenTarget", "Alt"), ESleekSide.RIGHT);
			flattenTargetField.Value = DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;
			flattenTargetField.OnValueChanged += OnFlattenTargetTyped;
			AddChild(flattenTargetField);

			flattenMethodButton = new SleekButtonState(new GUIContent(localization.format("FlattenMethod_Regular")), new GUIContent(localization.format("FlattenMethod_Min")), new GUIContent(localization.format("FlattenMethod_Max")));
			flattenMethodButton.PositionScale_Y = 1.0f;
			flattenMethodButton.SizeOffset_X = 200;
			flattenMethodButton.SizeOffset_Y = 30;
			flattenMethodButton.AddLabel(localization.format("FlattenMethod_Label"), ESleekSide.RIGHT);
			flattenMethodButton.state = (int) DevkitLandscapeToolHeightmapOptions.instance.flattenMethod;
			flattenMethodButton.onSwappedState += OnSwappedFlattenMethod;
			AddChild(flattenMethodButton);

			maxPreviewSamplesField = Glazier.Get().CreateUInt32Field();
			maxPreviewSamplesField.PositionScale_Y = 1.0f;
			maxPreviewSamplesField.SizeOffset_X = 200;
			maxPreviewSamplesField.SizeOffset_Y = 30;
			maxPreviewSamplesField.AddLabel(localization.format("MaxPreviewSamples"), ESleekSide.RIGHT);
			maxPreviewSamplesField.Value = DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples;
			maxPreviewSamplesField.OnValueChanged += OnMaxPreviewSamplesTyped;
			AddChild(maxPreviewSamplesField);

			UpdateLowerLeftOffset();
		}

		private void OnSwappedMode(SleekButtonState element, int index)
		{
			TerrainEditor.heightmapMode = (TerrainEditor.EDevkitLandscapeToolHeightmapMode) index;
		}

		private void OnBrushStrengthTyped(ISleekFloat32Field field, float state)
		{
			EditorInteract.instance.terrainTool.heightmapBrushStrength = state;
		}

		private void OnBrushFalloffTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolHeightmapOptions.instance.brushFalloff = state;
		}

		private void OnBrushRadiusTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolHeightmapOptions.instance.brushRadius = state;
		}

		private void OnFlattenTargetTyped(ISleekFloat32Field field, float state)
		{
			DevkitLandscapeToolHeightmapOptions.instance.flattenTarget = state;
		}

		private void OnMaxPreviewSamplesTyped(ISleekUInt32Field field, uint state)
		{
			DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples = state;
		}

		private void OnSwappedSmoothMethod(SleekButtonState element, int index)
		{
			DevkitLandscapeToolHeightmapOptions.instance.smoothMethod = (EDevkitLandscapeToolHeightmapSmoothMethod) index;
		}

		private void OnSwappedFlattenMethod(SleekButtonState element, int index)
		{
			DevkitLandscapeToolHeightmapOptions.instance.flattenMethod = (EDevkitLandscapeToolHeightmapFlattenMethod) index;
		}

		private void UpdateLowerLeftOffset()
		{
			float lowerLeftOffset = 0;
			const float lowerLeftPadding = 10;

			lowerLeftOffset -= modeButton.SizeOffset_Y;
			modeButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			lowerLeftOffset -= maxPreviewSamplesField.SizeOffset_Y;
			maxPreviewSamplesField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			smoothMethodButton.IsVisible = TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.SMOOTH;
			if (smoothMethodButton.IsVisible)
			{
				lowerLeftOffset -= smoothMethodButton.SizeOffset_Y;
				smoothMethodButton.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			flattenMethodButton.IsVisible = TerrainEditor.heightmapMode == TerrainEditor.EDevkitLandscapeToolHeightmapMode.FLATTEN;
			flattenTargetField.IsVisible = flattenMethodButton.IsVisible;
			if (flattenMethodButton.IsVisible)
			{
				lowerLeftOffset -= flattenMethodButton.SizeOffset_Y;
				flattenMethodButton.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;

				lowerLeftOffset -= flattenTargetField.SizeOffset_Y;
				flattenTargetField.PositionOffset_Y = lowerLeftOffset;
				lowerLeftOffset -= lowerLeftPadding;
			}

			lowerLeftOffset -= brushStrengthField.SizeOffset_Y;
			brushStrengthField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			lowerLeftOffset -= brushFalloffField.SizeOffset_Y;
			brushFalloffField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;

			lowerLeftOffset -= brushRadiusField.SizeOffset_Y;
			brushRadiusField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= lowerLeftPadding;
		}

		private Local localization;
		private ISleekLabel hintLabel;
		private SleekButtonState modeButton;
		private ISleekFloat32Field brushRadiusField;
		private ISleekFloat32Field brushFalloffField;
		private ISleekFloat32Field brushStrengthField;
		private ISleekFloat32Field flattenTargetField;
		private ISleekUInt32Field maxPreviewSamplesField;
		private SleekButtonState smoothMethodButton;
		private SleekButtonState flattenMethodButton;
	}
}
