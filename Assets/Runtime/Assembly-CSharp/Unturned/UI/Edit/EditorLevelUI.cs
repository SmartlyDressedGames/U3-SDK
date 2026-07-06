////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorLevelUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon objectsButton;
		private static SleekButtonIcon visibilityButton;
		private static SleekButtonIcon playersButton;
		private static SleekButtonIcon volumesButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			EditorLevelObjectsUI.close();
			EditorLevelVisibilityUI.close();
			EditorLevelPlayersUI.close();
			volumesUI.Close();

			container.AnimateOutOfView(1, 0);
		}

		private void onClickedObjectsButton(ISleekElement button)
		{
			EditorLevelObjectsUI.open();
			EditorLevelVisibilityUI.close();
			EditorLevelPlayersUI.close();
			volumesUI.Close();
		}

		private void onClickedVisibilityButton(ISleekElement button)
		{
			EditorLevelObjectsUI.close();
			EditorLevelVisibilityUI.open();
			EditorLevelPlayersUI.close();
			volumesUI.Close();
		}

		private void onClickedPlayersButton(ISleekElement button)
		{
			EditorLevelObjectsUI.close();
			EditorLevelVisibilityUI.close();
			EditorLevelPlayersUI.open();
			volumesUI.Close();
		}

		private void OnClickedVolumesButton(ISleekElement button)
		{
			EditorLevelObjectsUI.close();
			EditorLevelVisibilityUI.close();
			EditorLevelPlayersUI.close();
			volumesUI.Open();
		}

		public void OnDestroy()
		{
			objectsUI.OnDestroy();
			volumesUI.OnDestroy();
		}

		public EditorLevelUI()
		{
			Local localization = Localization.read("/Editor/EditorLevel.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorLevel");

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

			objectsButton = new SleekButtonIcon(icons.load<Texture2D>("Objects"));
			objectsButton.PositionOffset_Y = 40;
			objectsButton.SizeOffset_X = -5;
			objectsButton.SizeOffset_Y = 30;
			objectsButton.SizeScale_X = 0.25f;
			objectsButton.text = localization.format("ObjectsButtonText");
			objectsButton.tooltip = localization.format("ObjectsButtonTooltip");
			objectsButton.onClickedButton += onClickedObjectsButton;
			container.AddChild(objectsButton);

			visibilityButton = new SleekButtonIcon(icons.load<Texture2D>("Visibility"));
			visibilityButton.PositionOffset_X = 5;
			visibilityButton.PositionOffset_Y = 40;
			visibilityButton.PositionScale_X = 0.25f;
			visibilityButton.SizeOffset_X = -10;
			visibilityButton.SizeOffset_Y = 30;
			visibilityButton.SizeScale_X = 0.25f;
			visibilityButton.text = localization.format("VisibilityButtonText");
			visibilityButton.tooltip = localization.format("VisibilityButtonTooltip");
			visibilityButton.onClickedButton += onClickedVisibilityButton;
			container.AddChild(visibilityButton);

			playersButton = new SleekButtonIcon(icons.load<Texture2D>("Players"));
			playersButton.PositionOffset_Y = 40;
			playersButton.PositionOffset_X = 5;
			playersButton.PositionScale_X = 0.5f;
			playersButton.SizeOffset_X = -10;
			playersButton.SizeOffset_Y = 30;
			playersButton.SizeScale_X = 0.25f;
			playersButton.text = localization.format("PlayersButtonText");
			playersButton.tooltip = localization.format("PlayersButtonTooltip");
			playersButton.onClickedButton += onClickedPlayersButton;
			container.AddChild(playersButton);

			volumesButton = new SleekButtonIcon(null);
			volumesButton.PositionOffset_Y = 40;
			volumesButton.PositionOffset_X = 5;
			volumesButton.PositionScale_X = 0.75f;
			volumesButton.SizeOffset_X = -5;
			volumesButton.SizeOffset_Y = 30;
			volumesButton.SizeScale_X = 0.25f;
			volumesButton.text = localization.format("VolumesButtonText");
			volumesButton.tooltip = localization.format("VolumesButtonTooltip");
			volumesButton.onClickedButton += OnClickedVolumesButton;
			container.AddChild(volumesButton);

			objectsUI = new EditorLevelObjectsUI();
			objectsUI.PositionOffset_X = 10;
			objectsUI.PositionOffset_Y = 90;
			objectsUI.PositionScale_X = 1.0f;
			objectsUI.SizeOffset_X = -20;
			objectsUI.SizeOffset_Y = -100;
			objectsUI.SizeScale_X = 1.0f;
			objectsUI.SizeScale_Y = 1.0f;
			EditorUI.window.AddChild(objectsUI);

			new EditorLevelVisibilityUI();
			new EditorLevelPlayersUI();

			volumesUI = new EditorVolumesUI();
			volumesUI.PositionOffset_X = 10;
			volumesUI.PositionOffset_Y = 90;
			volumesUI.PositionScale_X = 1.0f;
			volumesUI.SizeOffset_X = -20;
			volumesUI.SizeOffset_Y = -100;
			volumesUI.SizeScale_X = 1.0f;
			volumesUI.SizeScale_Y = 1.0f;
			EditorUI.window.AddChild(volumesUI);
		}

		private EditorLevelObjectsUI objectsUI;
		private static EditorVolumesUI volumesUI;
	}
}
