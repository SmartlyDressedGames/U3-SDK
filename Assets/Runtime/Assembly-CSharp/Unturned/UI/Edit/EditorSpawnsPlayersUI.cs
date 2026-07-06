////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorLevelPlayersUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekToggle altToggle;
		private static ISleekSlider radiusSlider;
		private static ISleekSlider rotationSlider;
		private static SleekButtonIcon addButton;
		private static SleekButtonIcon removeButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			EditorSpawns.isSpawning = true;
			EditorSpawns.spawnMode = ESpawnMode.ADD_PLAYER;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			EditorSpawns.isSpawning = false;

			container.AnimateOutOfView(1, 0);
		}

		private static void onToggledAltToggle(ISleekToggle toggle, bool state)
		{
			EditorSpawns.selectedAlt = state;
		}

		private static void onDraggedRadiusSlider(ISleekSlider slider, float state)
		{
			EditorSpawns.radius = (byte) (EditorSpawns.MIN_REMOVE_SIZE + (state * EditorSpawns.MAX_REMOVE_SIZE));
		}

		private static void onDraggedRotationSlider(ISleekSlider slider, float state)
		{
			EditorSpawns.rotation = state * 360;
		}

		private static void onClickedAddButton(ISleekElement button)
		{
			EditorSpawns.spawnMode = ESpawnMode.ADD_PLAYER;
		}

		private static void onClickedRemoveButton(ISleekElement button)
		{
			EditorSpawns.spawnMode = ESpawnMode.REMOVE_PLAYER;
		}

		public EditorLevelPlayersUI()
		{
			Local localization = Localization.read("/Editor/EditorLevelPlayers.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorLevelPlayers");

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

			altToggle = Glazier.Get().CreateToggle();
			altToggle.PositionOffset_Y = -180;
			altToggle.PositionScale_Y = 1;
			altToggle.SizeOffset_X = 40;
			altToggle.SizeOffset_Y = 40;
			altToggle.Value = EditorSpawns.selectedAlt;
			altToggle.AddLabel(localization.format("AltLabelText"), ESleekSide.RIGHT);
			altToggle.OnValueChanged += onToggledAltToggle;
			container.AddChild(altToggle);

			radiusSlider = Glazier.Get().CreateSlider();
			radiusSlider.PositionOffset_Y = -130;
			radiusSlider.PositionScale_Y = 1;
			radiusSlider.SizeOffset_X = 200;
			radiusSlider.SizeOffset_Y = 20;
			radiusSlider.Value = (EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float) EditorSpawns.MAX_REMOVE_SIZE;
			radiusSlider.Orientation = ESleekOrientation.HORIZONTAL;
			radiusSlider.AddLabel(localization.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
			radiusSlider.OnValueChanged += onDraggedRadiusSlider;
			container.AddChild(radiusSlider);

			rotationSlider = Glazier.Get().CreateSlider();
			rotationSlider.PositionOffset_Y = -100;
			rotationSlider.PositionScale_Y = 1;
			rotationSlider.SizeOffset_X = 200;
			rotationSlider.SizeOffset_Y = 20;
			rotationSlider.Value = EditorSpawns.rotation / 360f;
			rotationSlider.Orientation = ESleekOrientation.HORIZONTAL;
			rotationSlider.AddLabel(localization.format("RotationSliderLabelText"), ESleekSide.RIGHT);
			rotationSlider.OnValueChanged += onDraggedRotationSlider;
			container.AddChild(rotationSlider);

			addButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
			addButton.PositionOffset_Y = -70;
			addButton.PositionScale_Y = 1;
			addButton.SizeOffset_X = 200;
			addButton.SizeOffset_Y = 30;
			addButton.text = localization.format("AddButtonText", ControlsSettings.tool_0);
			addButton.tooltip = localization.format("AddButtonTooltip");
			addButton.onClickedButton += onClickedAddButton;
			container.AddChild(addButton);

			removeButton = new SleekButtonIcon(icons.load<Texture2D>("Remove"));
			removeButton.PositionOffset_Y = -30;
			removeButton.PositionScale_Y = 1;
			removeButton.SizeOffset_X = 200;
			removeButton.SizeOffset_Y = 30;
			removeButton.text = localization.format("RemoveButtonText", ControlsSettings.tool_1);
			removeButton.tooltip = localization.format("RemoveButtonTooltip");
			removeButton.onClickedButton += onClickedRemoveButton;
			container.AddChild(removeButton);
		}
	}
}
