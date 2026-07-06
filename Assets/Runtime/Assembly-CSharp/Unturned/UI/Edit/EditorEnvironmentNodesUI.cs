////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using UnityEngine;

namespace SDG.Unturned
{
	internal class EditorEnvironmentNodesUI : SleekFullscreenBox
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
				RemoveChild(focusedItemMenu);
				focusedItemMenu = null;
			}

			focusedGameObject = newFocusedGameObject;

			TempNodeBase focusedNode = focusedGameObject?.GetComponent<TempNodeBase>();
			if (focusedNode != null)
			{
				focusedItemMenu = focusedNode.CreateMenu();
				if (focusedItemMenu != null)
				{
					focusedItemMenu.PositionOffset_Y = snapTransformField.PositionOffset_Y - 10 - focusedItemMenu.SizeOffset_Y;
					focusedItemMenu.PositionScale_Y = 1.0f;
					AddChild(focusedItemMenu);
				}
			}
		}

		public EditorEnvironmentNodesUI()
		{
			DevkitSelectionToolOptions.load();

			tool = new NodesEditor();

			localization = Localization.read("/Editor/EditorLevelNodes.dat");
			Local objectsLocalization = Localization.read("/Editor/EditorLevelObjects.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorLevelObjects");

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

			float upperRightOffset = 0;
			ISleekElement typeContainer = Glazier.Get().CreateFrame();
			typeContainer.PositionScale_X = 1.0f;
			typeContainer.SizeOffset_X = 200;
			typeContainer.PositionOffset_X = -typeContainer.SizeOffset_X;
			typeContainer.SizeScale_Y = 1.0f;
			AddChild(typeContainer);

			ISleekButton airdropButton = Glazier.Get().CreateButton();
			airdropButton.PositionOffset_Y = upperRightOffset;
			airdropButton.SizeScale_X = 1.0f;
			airdropButton.SizeOffset_Y = 30;
			airdropButton.Text = "Airdrop Marker";
			airdropButton.OnClicked += (ISleekElement button) =>
			{
				tool.activeNodeSystem = AirdropDevkitNodeSystem.Get();
			};
			typeContainer.AddChild(airdropButton);
			upperRightOffset += airdropButton.SizeOffset_Y;

			ISleekButton locationButton = Glazier.Get().CreateButton();
			locationButton.PositionOffset_Y = upperRightOffset;
			locationButton.SizeScale_X = 1.0f;
			locationButton.SizeOffset_Y = 30;
			locationButton.Text = "Named Location";
			locationButton.OnClicked += (ISleekElement button) =>
			{
				tool.activeNodeSystem = LocationDevkitNodeSystem.Get();
			};
			typeContainer.AddChild(locationButton);
			upperRightOffset += locationButton.SizeOffset_Y;

			ISleekButton spawnpointButton = Glazier.Get().CreateButton();
			spawnpointButton.PositionOffset_Y = upperRightOffset;
			spawnpointButton.SizeScale_X = 1.0f;
			spawnpointButton.SizeOffset_Y = 30;
			spawnpointButton.Text = "Spawnpoint";
			spawnpointButton.OnClicked += (ISleekElement button) =>
			{
				tool.activeNodeSystem = SpawnpointSystemV2.Get();
			};
			typeContainer.AddChild(spawnpointButton);
			upperRightOffset += spawnpointButton.SizeOffset_Y;
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

		private void OnSurfaceMaskTyped(ISleekUInt32Field field, uint state)
		{
			DevkitSelectionToolOptions.instance.selectionMask = (ERayMask) state;
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

		internal Local localization;
		private NodesEditor tool;
		private GameObject focusedGameObject;
		private ISleekElement focusedItemMenu;

		private ISleekFloat32Field snapTransformField;
		private ISleekFloat32Field snapRotationField;

		private SleekButtonIcon transformButton;
		private SleekButtonIcon rotateButton;
		private SleekButtonIcon scaleButton;
		private SleekButtonState coordinateButton;
		private ISleekUInt32Field surfaceMaskField;
	}
}
