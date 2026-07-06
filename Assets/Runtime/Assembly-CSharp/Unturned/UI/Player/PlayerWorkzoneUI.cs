////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerWorkzoneUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekImage dragBox;

		private static ISleekFloat32Field snapTransformField;
		private static ISleekFloat32Field snapRotationField;

		private static SleekButtonIcon transformButton;
		private static SleekButtonIcon rotateButton;
		//private static SleekButtonIcon scaleButton;
		public static SleekButtonState coordinateButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			Player.LocalPlayer.workzone.isBuilding = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			Player.LocalPlayer.workzone.isBuilding = false;

			container.AnimateOutOfView(0, 1);
		}

		private static void onDragStarted(Vector2 minViewportPoint, Vector2 maxViewportPoint)
		{
			Vector2 minPosition = PlayerUI.window.ViewportToNormalizedPosition(minViewportPoint);
			Vector2 maxPosition = PlayerUI.window.ViewportToNormalizedPosition(maxViewportPoint);
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

		private static void onTypedSnapTransformField(ISleekFloat32Field field, float value)
		{
			Player.LocalPlayer.workzone.snapTransform = value;
		}

		private static void onTypedSnapRotationField(ISleekFloat32Field field, float value)
		{
			Player.LocalPlayer.workzone.snapRotation = value;
		}

		private static void onClickedTransformButton(ISleekElement button)
		{
			Player.LocalPlayer.workzone.dragMode = EDragMode.TRANSFORM;
		}

		private static void onClickedRotateButton(ISleekElement button)
		{
			Player.LocalPlayer.workzone.dragMode = EDragMode.ROTATE;
		}

		//private static void onClickedScaleButton(SleekButton button)
		//{
		//	Player.player.workzone.dragMode = EDragMode.SCALE;
		//}

		private static void onSwappedStateCoordinate(SleekButtonState button, int index)
		{
			Player.LocalPlayer.workzone.dragCoordinate = (EDragCoordinate) index;
		}

		public PlayerWorkzoneUI()
		{
			Local localization = Localization.read("/Editor/EditorLevelObjects.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorLevelObjects");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_X = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.window.AddChild(container);
			active = false;

			Player.LocalPlayer.workzone.onDragStarted = onDragStarted;
			Player.LocalPlayer.workzone.onDragStopped = onDragStopped;

			dragBox = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			dragBox.TintColor = new Color(1.0f, 1.0f, 0.0f, 0.2f);
			PlayerUI.window.AddChild(dragBox);
			dragBox.IsVisible = false;

			snapTransformField = Glazier.Get().CreateFloat32Field();
			snapTransformField.PositionOffset_Y = -190;
			snapTransformField.PositionScale_Y = 1;
			snapTransformField.SizeOffset_X = 200;
			snapTransformField.SizeOffset_Y = 30;
			snapTransformField.Value = Player.LocalPlayer.workzone.snapTransform;
			snapTransformField.AddLabel(localization.format("SnapTransformLabelText"), ESleekSide.RIGHT);
			snapTransformField.OnValueChanged += onTypedSnapTransformField;
			container.AddChild(snapTransformField);

			snapRotationField = Glazier.Get().CreateFloat32Field();
			snapRotationField.PositionOffset_Y = -150;
			snapRotationField.PositionScale_Y = 1;
			snapRotationField.SizeOffset_X = 200;
			snapRotationField.SizeOffset_Y = 30;
			snapRotationField.Value = Player.LocalPlayer.workzone.snapRotation;
			snapRotationField.AddLabel(localization.format("SnapRotationLabelText"), ESleekSide.RIGHT);
			snapRotationField.OnValueChanged += onTypedSnapRotationField;
			container.AddChild(snapRotationField);

			transformButton = new SleekButtonIcon(icons.load<Texture2D>("Transform"));
			transformButton.PositionOffset_Y = -110;
			transformButton.PositionScale_Y = 1;
			transformButton.SizeOffset_X = 200;
			transformButton.SizeOffset_Y = 30;
			transformButton.text = localization.format("TransformButtonText", ControlsSettings.tool_0);
			transformButton.tooltip = localization.format("TransformButtonTooltip");
			transformButton.onClickedButton += onClickedTransformButton;
			container.AddChild(transformButton);

			rotateButton = new SleekButtonIcon(icons.load<Texture2D>("Rotate"));
			rotateButton.PositionOffset_Y = -70;
			rotateButton.PositionScale_Y = 1;
			rotateButton.SizeOffset_X = 200;
			rotateButton.SizeOffset_Y = 30;
			rotateButton.text = localization.format("RotateButtonText", ControlsSettings.tool_1);
			rotateButton.tooltip = localization.format("RotateButtonTooltip");
			rotateButton.onClickedButton += onClickedRotateButton;
			container.AddChild(rotateButton);

			//scaleButton = new SleekButtonIcon(icons.load<Texture2D>("Scale"));
			//scaleButton.positionOffset_Y = -70;
			//scaleButton.positionScale_Y = 1;
			//scaleButton.sizeOffset_X = 200;
			//scaleButton.sizeOffset_Y = 30;
			//scaleButton.text = localization.format("ScaleButtonText", ControlsSettings.tool_3);
			//scaleButton.tooltip = localization.format("ScaleButtonTooltip");
			//scaleButton.onClickedButton += onClickedScaleButton;
			//container.add(scaleButton);

			coordinateButton = new SleekButtonState(new GUIContent(localization.format("CoordinateButtonTextGlobal"), icons.load<Texture>("Global")), new GUIContent(localization.format("CoordinateButtonTextLocal"), icons.load<Texture>("Local")));
			coordinateButton.PositionOffset_Y = -30;
			coordinateButton.PositionScale_Y = 1;
			coordinateButton.SizeOffset_X = 200;
			coordinateButton.SizeOffset_Y = 30;
			coordinateButton.tooltip = localization.format("CoordinateButtonTooltip");
			coordinateButton.onSwappedState = onSwappedStateCoordinate;
			container.AddChild(coordinateButton);
		}
	}
}
