////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class VolumeTypeButton : SleekWrapper
	{
		public VolumeTypeButton(EditorVolumesUI owner, VolumeManagerBase volumeType) : base()
		{
			this.owner = owner;
			this.volumeType = volumeType;

			visibilityButton = new SleekButtonState(new GUIContent("H", owner.localization.format("Visibility_Hidden")),
				new GUIContent("W", owner.localization.format("Visibility_Wireframe")),
				new GUIContent("S", owner.localization.format("Visibility_Solid")));
			visibilityButton.SizeOffset_X = 50;
			visibilityButton.SizeOffset_Y = 30;
			visibilityButton.UseContentTooltip = true;
			visibilityButton.onSwappedState += OnSwappedVisibility;
			RefreshVisibility();
			AddChild(visibilityButton);

			nameButton = Glazier.Get().CreateButton();
			nameButton.PositionOffset_X = 50;
			nameButton.SizeScale_X = 1.0f;
			nameButton.SizeScale_Y = 1.0f;
			nameButton.SizeOffset_X = -nameButton.PositionOffset_X;
			nameButton.Text = volumeType.FriendlyName;
			nameButton.OnClicked += OnTypeClicked;
			AddChild(nameButton);
		}

		public void RefreshVisibility()
		{
			visibilityButton.state = (int) volumeType.Visibility;
		}

		private void OnSwappedVisibility(SleekButtonState button, int state)
		{
			volumeType.Visibility = (ELevelVolumeVisibility) state;
			owner.RefreshSelectedVisibility();
		}

		private void OnTypeClicked(ISleekElement element)
		{
			owner.SetSelectedType(volumeType);
		}

		public EditorVolumesUI owner;
		public VolumeManagerBase volumeType;
		private SleekButtonState visibilityButton;
		private ISleekButton nameButton;
	}

	internal class EditorVolumesUI : SleekFullscreenBox
	{
		public void Open()
		{
			SyncSettings();
			AnimateIntoView();
			EditorInteract.instance.SetActiveTool(tool);
		}

		public void Close()
		{
			AnimateOutOfView(1.0f, 0.0f);

			DevkitSelectionToolOptions.save();

			EditorInteract.instance.SetActiveTool(null);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			GameObject newFocusedGameObject = DevkitSelectionManager.mostRecentGameObject;
			if (focusedGameObject == newFocusedGameObject)
				return;

			if (focusedItemMenu != null)
			{
				focusedItemScrollView.RemoveChild(focusedItemMenu);
				focusedItemMenu = null;
				focusedItemScrollView.IsVisible = false;
			}

			focusedGameObject = newFocusedGameObject;

			VolumeBase focusedVolume = focusedGameObject?.GetComponent<VolumeBase>();
			if (focusedVolume != null)
			{
				focusedItemMenu = focusedVolume.CreateMenu();
				if (focusedItemMenu != null)
				{
					// Prevent focus loss when dragging a slider outside foreground.
					ISleekBox backgroundElement = Glazier.Get().CreateBox();
					backgroundElement.BackgroundColor = ColorEx.WhiteZeroAlpha;
					backgroundElement.SizeScale_X = 1;
					backgroundElement.SizeScale_Y = 1;
					focusedItemMenu.AddChild(backgroundElement);
					backgroundElement.SetAsFirstSibling();

					focusedItemScrollView.ContentSizeOffset = new Vector2(0.0f, focusedItemMenu.SizeOffset_Y);
					focusedItemScrollView.AddChild(focusedItemMenu);
					focusedItemScrollView.IsVisible = true;
					focusedItemScrollView.ScrollToTop();
				}
			}
		}

		public EditorVolumesUI() : base()
		{
			DevkitSelectionToolOptions.load();

			tool = new VolumesEditor();

			localization = Localization.read("/Editor/EditorLevelVolumes.dat");
			Local objectsLocalization = Localization.read("/Editor/EditorLevelObjects.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorLevelObjects");

			List<VolumeManagerBase> volumeTypes = new List<VolumeManagerBase>(VolumeManagerBase.allManagers);
			volumeTypes.Sort((VolumeManagerBase lhs, VolumeManagerBase rhs) =>
			{
				return lhs.FriendlyName.CompareTo(rhs.FriendlyName);
			});

			const int leftColumnWidth = 200;
			float lowerLeftOffset = 0;

			surfaceMaskField = Glazier.Get().CreateUInt32Field();
			surfaceMaskField.PositionScale_Y = 1.0f;
			surfaceMaskField.SizeOffset_X = leftColumnWidth;
			surfaceMaskField.SizeOffset_Y = 30;
			lowerLeftOffset -= surfaceMaskField.SizeOffset_Y;
			surfaceMaskField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= 10;
			surfaceMaskField.AddLabel("Surface Mask (sorry this is not user-friendly at the moment)", ESleekSide.RIGHT);
			surfaceMaskField.OnValueChanged += OnSurfaceMaskTyped;
			AddChild(surfaceMaskField);

			coordinateButton = new SleekButtonState(new GUIContent(objectsLocalization.format("CoordinateButtonTextGlobal"), icons.load<Texture>("Global")), new GUIContent(objectsLocalization.format("CoordinateButtonTextLocal"), icons.load<Texture>("Local")));
			coordinateButton.PositionScale_Y = 1;
			coordinateButton.SizeOffset_X = leftColumnWidth;
			coordinateButton.SizeOffset_Y = 30;
			lowerLeftOffset -= coordinateButton.SizeOffset_Y;
			coordinateButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= 10;
			coordinateButton.tooltip = objectsLocalization.format("CoordinateButtonTooltip");
			coordinateButton.onSwappedState = OnSwappedStateCoordinate;
			AddChild(coordinateButton);

			scaleButton = new SleekButtonIcon(icons.load<Texture2D>("Scale"));
			scaleButton.PositionScale_Y = 1;
			scaleButton.SizeOffset_X = leftColumnWidth;
			scaleButton.SizeOffset_Y = 30;
			lowerLeftOffset -= scaleButton.SizeOffset_Y;
			scaleButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= 10;
			scaleButton.text = objectsLocalization.format("ScaleButtonText", ControlsSettings.tool_3);
			scaleButton.tooltip = objectsLocalization.format("ScaleButtonTooltip");
			scaleButton.onClickedButton += OnScaleClicked;
			AddChild(scaleButton);

			rotateButton = new SleekButtonIcon(icons.load<Texture2D>("Rotate"));
			rotateButton.PositionScale_Y = 1;
			rotateButton.SizeOffset_X = leftColumnWidth;
			rotateButton.SizeOffset_Y = 30;
			lowerLeftOffset -= rotateButton.SizeOffset_Y;
			rotateButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= 10;
			rotateButton.text = objectsLocalization.format("RotateButtonText", ControlsSettings.tool_1);
			rotateButton.tooltip = objectsLocalization.format("RotateButtonTooltip");
			rotateButton.onClickedButton += OnRotateClicked;
			AddChild(rotateButton);

			transformButton = new SleekButtonIcon(icons.load<Texture2D>("Transform"));
			transformButton.PositionScale_Y = 1;
			transformButton.SizeOffset_X = leftColumnWidth;
			transformButton.SizeOffset_Y = 30;
			lowerLeftOffset -= transformButton.SizeOffset_Y;
			transformButton.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= 10;
			transformButton.text = objectsLocalization.format("TransformButtonText", ControlsSettings.tool_0);
			transformButton.tooltip = objectsLocalization.format("TransformButtonTooltip");
			transformButton.onClickedButton += OnTransformClicked;
			AddChild(transformButton);

			snapRotationField = Glazier.Get().CreateFloat32Field();
			snapRotationField.PositionScale_Y = 1;
			snapRotationField.SizeOffset_X = leftColumnWidth;
			snapRotationField.SizeOffset_Y = 30;
			lowerLeftOffset -= snapRotationField.SizeOffset_Y;
			snapRotationField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= 10;
			snapRotationField.AddLabel(objectsLocalization.format("SnapRotationLabelText"), ESleekSide.RIGHT);
			snapRotationField.OnValueChanged += OnTypedSnapRotationField;
			AddChild(snapRotationField);

			snapTransformField = Glazier.Get().CreateFloat32Field();
			snapTransformField.PositionScale_Y = 1;
			snapTransformField.SizeOffset_X = leftColumnWidth;
			snapTransformField.SizeOffset_Y = 30;
			lowerLeftOffset -= snapTransformField.SizeOffset_Y;
			snapTransformField.PositionOffset_Y = lowerLeftOffset;
			lowerLeftOffset -= 10;
			snapTransformField.AddLabel(objectsLocalization.format("SnapTransformLabelText"), ESleekSide.RIGHT);
			snapTransformField.OnValueChanged += OnTypedSnapTransformField;
			AddChild(snapTransformField);

			focusedItemScrollView = Glazier.Get().CreateScrollView();
			focusedItemScrollView.SizeOffset_X = leftColumnWidth + 200 + 30;
			focusedItemScrollView.SizeOffset_Y = lowerLeftOffset;
			focusedItemScrollView.SizeScale_Y = 1.0f;
			focusedItemScrollView.IsVisible = false;
			focusedItemScrollView.AlignContentToBottom = true;
			AddChild(focusedItemScrollView);

			// Kinda hacked in here but it is the only per-volume-type visibility setting so far.
			enableUnderwaterEffectsToggle = Glazier.Get().CreateToggle();
			enableUnderwaterEffectsToggle.PositionOffset_X = 400;
			enableUnderwaterEffectsToggle.PositionOffset_Y = -40;
			enableUnderwaterEffectsToggle.PositionScale_Y = 1.0f;
			enableUnderwaterEffectsToggle.SizeOffset_X = 40;
			enableUnderwaterEffectsToggle.SizeOffset_Y = 40;
			enableUnderwaterEffectsToggle.AddLabel(localization.format("WantsUnderwaterEffects"), ESleekSide.RIGHT);
			enableUnderwaterEffectsToggle.Value = LevelLighting.EditorWantsUnderwaterEffects;
			enableUnderwaterEffectsToggle.IsVisible = false;
			enableUnderwaterEffectsToggle.OnValueChanged += OnUnderwaterEffectsToggled;
			AddChild(enableUnderwaterEffectsToggle);
			enableWaterSurfaceToggle = Glazier.Get().CreateToggle();
			enableWaterSurfaceToggle.PositionOffset_X = 400;
			enableWaterSurfaceToggle.PositionOffset_Y = -90;
			enableWaterSurfaceToggle.PositionScale_Y = 1.0f;
			enableWaterSurfaceToggle.SizeOffset_X = 40;
			enableWaterSurfaceToggle.SizeOffset_Y = 40;
			enableWaterSurfaceToggle.AddLabel(localization.format("WantsWaterSurface"), ESleekSide.RIGHT);
			enableWaterSurfaceToggle.Value = LevelLighting.EditorWantsWaterSurface;
			enableWaterSurfaceToggle.IsVisible = false;
			enableWaterSurfaceToggle.OnValueChanged += OnWaterSurfaceToggled;
			AddChild(enableWaterSurfaceToggle);
			refreshCullingVolumesButton = Glazier.Get().CreateButton();
			refreshCullingVolumesButton.PositionOffset_X = 400;
			refreshCullingVolumesButton.PositionOffset_Y = -30;
			refreshCullingVolumesButton.PositionScale_Y = 1.0f;
			refreshCullingVolumesButton.SizeOffset_X = 200;
			refreshCullingVolumesButton.SizeOffset_Y = 30;
			refreshCullingVolumesButton.Text = localization.format("RefreshCullingVolumes");
			refreshCullingVolumesButton.TooltipText = localization.format("RefreshCullingVolumes_Tooltip");
			refreshCullingVolumesButton.IsVisible = false;
			refreshCullingVolumesButton.OnClicked += OnRefreshCullingVolumesClicked;
			AddChild(refreshCullingVolumesButton);
			previewCullingToggle = Glazier.Get().CreateToggle();
			previewCullingToggle.PositionOffset_X = 400;
			previewCullingToggle.PositionOffset_Y = -80;
			previewCullingToggle.PositionScale_Y = 1.0f;
			previewCullingToggle.SizeOffset_X = 40;
			previewCullingToggle.SizeOffset_Y = 40;
			previewCullingToggle.AddLabel(localization.format("PreviewCulling"), ESleekSide.RIGHT);
			previewCullingToggle.Value = EditorWantsToPreviewCulling;
			previewCullingToggle.IsVisible = false;
			previewCullingToggle.OnValueChanged += OnPreviewCullingToggled;
			AddChild(previewCullingToggle);
			enableNoLightingPreviewToggle = Glazier.Get().CreateToggle();
			enableNoLightingPreviewToggle.PositionOffset_X = 400;
			enableNoLightingPreviewToggle.PositionOffset_Y = -40;
			enableNoLightingPreviewToggle.PositionScale_Y = 1.0f;
			enableNoLightingPreviewToggle.SizeOffset_X = 40;
			enableNoLightingPreviewToggle.SizeOffset_Y = 40;
			enableNoLightingPreviewToggle.AddLabel(localization.format("WantsNoLightingPreview"), ESleekSide.RIGHT);
			enableNoLightingPreviewToggle.Value = LevelLighting.EditorWantsNoLightingPreview;
			enableNoLightingPreviewToggle.IsVisible = false;
			enableNoLightingPreviewToggle.OnValueChanged += OnNoLightingPreviewToggled;
			AddChild(enableNoLightingPreviewToggle);

			const int rightColumnWidth = 300;
			float upperRightOffset = 0;

			selectedTypeBox = Glazier.Get().CreateBox();
			selectedTypeBox.PositionScale_X = 1.0f;
			selectedTypeBox.PositionOffset_Y = upperRightOffset;
			selectedTypeBox.SizeOffset_X = rightColumnWidth;
			selectedTypeBox.PositionOffset_X = -selectedTypeBox.SizeOffset_X;
			selectedTypeBox.SizeOffset_Y = 30;
			selectedTypeBox.AddLabel(localization.format("SelectedType_Label"), ESleekSide.LEFT);
			AddChild(selectedTypeBox);
			upperRightOffset += selectedTypeBox.SizeOffset_Y + 10;

			activeVisibilityButton = new SleekButtonState(new GUIContent(localization.format("Visibility_Hidden")), new GUIContent(localization.format("Visibility_Wireframe")), new GUIContent(localization.format("Visibility_Solid")));
			activeVisibilityButton.PositionScale_X = 1.0f;
			activeVisibilityButton.PositionOffset_Y = upperRightOffset;
			activeVisibilityButton.SizeOffset_X = rightColumnWidth;
			activeVisibilityButton.PositionOffset_X = -activeVisibilityButton.SizeOffset_X;
			activeVisibilityButton.SizeOffset_Y = 30;
			activeVisibilityButton.AddLabel(localization.format("ActiveVisibility_Label"), ESleekSide.LEFT);
			activeVisibilityButton.onSwappedState += OnSwappedActiveVisibility;
			activeVisibilityButton.IsVisible = false;
			AddChild(activeVisibilityButton);
			upperRightOffset += selectedTypeBox.SizeOffset_Y + 10;

			typeScrollView = Glazier.Get().CreateScrollView();
			typeScrollView.PositionScale_X = 1.0f;
			typeScrollView.SizeOffset_X = rightColumnWidth;
			typeScrollView.PositionOffset_X = -typeScrollView.SizeOffset_X;
			typeScrollView.PositionOffset_Y = upperRightOffset;
			typeScrollView.SizeOffset_Y = -upperRightOffset;
			typeScrollView.SizeScale_Y = 1.0f;
			typeScrollView.ScaleContentToWidth = true;
			AddChild(typeScrollView);

			int volumeIndex = 0;
			float typeOffset = 0;
			volumeButtons = new VolumeTypeButton[volumeTypes.Count];
			foreach (VolumeManagerBase type in volumeTypes)
			{
				VolumeTypeButton button = new VolumeTypeButton(this, type);
				button.PositionOffset_Y = typeOffset;
				button.SizeScale_X = 1.0f;
				button.SizeOffset_Y = 30;
				typeScrollView.AddChild(button);
				volumeButtons[volumeIndex] = button;
				typeOffset += button.SizeOffset_Y;
				++volumeIndex;
			}

			typeScrollView.ContentSizeOffset = new Vector2(0.0f, typeOffset);
		}

		internal void SetSelectedType(VolumeManagerBase type)
		{
			selectedTypeBox.Text = type.FriendlyName;
			tool.activeVolumeManager = type;
			activeVisibilityButton.state = (int) type.Visibility;
			activeVisibilityButton.IsVisible = true;
			enableUnderwaterEffectsToggle.IsVisible = type is SDG.Framework.Water.WaterVolumeManager;
			enableWaterSurfaceToggle.IsVisible = enableUnderwaterEffectsToggle.IsVisible;
#if !DEDICATED_SERVER
			refreshCullingVolumesButton.IsVisible = type is CullingVolumeManager;
			previewCullingToggle.IsVisible = refreshCullingVolumesButton.IsVisible;
#endif // !DEDICATED_SERVER
			enableNoLightingPreviewToggle.IsVisible = type is SDG.Framework.Devkit.AmbianceVolumeManager;
		}

		internal void RefreshSelectedVisibility()
		{
			if (tool.activeVolumeManager != null)
			{
				activeVisibilityButton.state = (int) tool.activeVolumeManager.Visibility;
			}
		}

		private void OnTypedSnapTransformField(ISleekFloat32Field field, float value)
		{
			DevkitSelectionToolOptions.instance.snapPosition = value;
		}

		private void OnTypedSnapRotationField(ISleekFloat32Field field, float value)
		{
			DevkitSelectionToolOptions.instance.snapRotation = value;
		}

		private void OnTransformClicked(ISleekElement button)
		{
			tool.mode = SelectionTool.ESelectionMode.POSITION;
		}

		private void OnRotateClicked(ISleekElement button)
		{
			tool.mode = SelectionTool.ESelectionMode.ROTATION;
		}

		private void OnScaleClicked(ISleekElement button)
		{
			tool.mode = SelectionTool.ESelectionMode.SCALE;
		}

		private void OnSwappedStateCoordinate(SleekButtonState button, int index)
		{
			DevkitSelectionToolOptions.instance.localSpace = index > 0;
		}

		private void OnSwappedActiveVisibility(SleekButtonState button, int state)
		{
			if (tool.activeVolumeManager != null)
			{
				tool.activeVolumeManager.Visibility = (ELevelVolumeVisibility) state;
			}

			foreach (VolumeTypeButton volumeButton in volumeButtons)
			{
				volumeButton.RefreshVisibility();
			}
		}

		private void OnUnderwaterEffectsToggled(ISleekToggle toggle, bool state)
		{
			LevelLighting.EditorWantsUnderwaterEffects = state;
		}

		private void OnWaterSurfaceToggled(ISleekToggle toggle, bool state)
		{
			LevelLighting.EditorWantsWaterSurface = state;
		}

		private void OnRefreshCullingVolumesClicked(ISleekElement button)
		{
#if !DEDICATED_SERVER
			// Clear all before refreshing all so that order is the same as if level just loaded.
			CullingVolumeManager.Get().ClearOverlappingObjects();
			CullingVolumeManager.Get().RefreshOverlappingObjects();
#endif // !DEDICATED_SERVER
		}

		private void OnPreviewCullingToggled(ISleekToggle toggle, bool state)
		{
			EditorWantsToPreviewCulling = state;
		}

		private void OnNoLightingPreviewToggled(ISleekToggle toggle, bool state)
		{
			LevelLighting.EditorWantsNoLightingPreview = state;
		}

		private void OnSurfaceMaskTyped(ISleekUInt32Field field, uint state)
		{
			DevkitSelectionToolOptions.instance.selectionMask = (ERayMask) state;
		}

		/// <summary>
		/// Other menus can modify DevkitSelectionToolOptions so we need to sync our menu when opened.
		/// </summary>
		private void SyncSettings()
		{
			surfaceMaskField.Value = (uint) DevkitSelectionToolOptions.instance.selectionMask;
			coordinateButton.state = DevkitSelectionToolOptions.instance.localSpace ? 1 : 0;
			snapRotationField.Value = DevkitSelectionToolOptions.instance.snapRotation;
			snapTransformField.Value = DevkitSelectionToolOptions.instance.snapPosition;
		}

		internal Local localization;
		private VolumesEditor tool;
		private GameObject focusedGameObject;
		private ISleekScrollView focusedItemScrollView;
		private ISleekElement focusedItemMenu;

		private ISleekFloat32Field snapTransformField;
		private ISleekFloat32Field snapRotationField;

		private SleekButtonIcon transformButton;
		private SleekButtonIcon rotateButton;
		private SleekButtonIcon scaleButton;
		private SleekButtonState coordinateButton;
		private ISleekUInt32Field surfaceMaskField;

		private ISleekBox selectedTypeBox;
		private SleekButtonState activeVisibilityButton;
		private ISleekScrollView typeScrollView;
		private VolumeTypeButton[] volumeButtons;

		private ISleekToggle enableUnderwaterEffectsToggle;
		private ISleekToggle enableWaterSurfaceToggle;
		private ISleekButton refreshCullingVolumesButton;
		private ISleekToggle previewCullingToggle;
		private ISleekToggle enableNoLightingPreviewToggle;

		internal static bool EditorWantsToPreviewCulling;
	}
}
