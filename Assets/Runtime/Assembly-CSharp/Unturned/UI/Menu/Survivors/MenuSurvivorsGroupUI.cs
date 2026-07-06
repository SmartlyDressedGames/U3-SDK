////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuSurvivorsGroupUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static SteamGroup[] groups;

		//private static ISleekField nickField;
		private static ISleekBox markerColorBox;
		private static SleekColorPicker markerColorPicker;
		private static SleekButtonIcon groupButton;
		private static ISleekScrollView groupsBox;

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

			container.AnimateOutOfView(0, 1);
		}

		private static void onCharacterUpdated(byte index, Character character)
		{
			if (index == Characters.selected)
			{
				//nickField.text = character.nick;
				markerColorPicker.state = character.markerColor;

				for (int step = 0; step < groups.Length; step++)
				{
					if (groups[step].steamID == character.group)
					{
						groupButton.text = groups[step].name;
						groupButton.icon = groups[step].icon;
						return;
					}
				}

				groupButton.text = localization.format("Group_Box");
				groupButton.icon = null;
			}
		}

		private static void onTypedNickField(ISleekField field, string text)
		{
			Characters.renick(text);
		}

		private static void onPickedMarkerColor(SleekColorPicker picker, Color state)
		{
			Characters.paintMarkerColor(state);
		}

		private static void onClickedGroupButton(ISleekElement button)
		{
			Characters.group(groups[Mathf.FloorToInt(button.PositionOffset_Y / 40)].steamID);
		}

		private static void onClickedUngroupButton(ISleekElement button)
		{
			Characters.ungroup();
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuSurvivorsUI.open();
			close();
		}

		public void OnDestroy()
		{
			Characters.onCharacterUpdated -= onCharacterUpdated;
		}

		public MenuSurvivorsGroupUI()
		{
			localization = Localization.read("/Menu/Survivors/MenuSurvivorsGroup.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			groups = Provider.provider.communityService.getGroups();

			//nickField = Glazier.Get().CreateStringField();
			//nickField.positionOffset_X = -100;
			//nickField.positionOffset_Y = 100;
			//nickField.positionScale_X = 0.75f;
			//nickField.sizeOffset_X = 200;
			//nickField.sizeOffset_Y = 30;
			//nickField.maxLength = 32;
			//nickField.addLabel(localization.format("Nick_Field_Label"), ESleekSide.LEFT);
			//nickField.onTyped += onTypedNickField;
			//container.add(nickField);

			markerColorBox = Glazier.Get().CreateBox();
			markerColorBox.PositionOffset_X = -120;
			markerColorBox.PositionOffset_Y = 100;
			markerColorBox.PositionScale_X = 0.75f;
			markerColorBox.SizeOffset_X = 240;
			markerColorBox.SizeOffset_Y = 30;
			markerColorBox.Text = localization.format("Marker_Color_Box");
			container.AddChild(markerColorBox);

			markerColorPicker = new SleekColorPicker();
			markerColorPicker.PositionOffset_X = -120;
			markerColorPicker.PositionOffset_Y = 140;
			markerColorPicker.PositionScale_X = 0.75f;
			markerColorPicker.onColorPicked = onPickedMarkerColor;
			container.AddChild(markerColorPicker);

			groupButton = new SleekButtonIcon(null, 20);
			groupButton.PositionOffset_X = -120;
			groupButton.PositionOffset_Y = 270;
			groupButton.PositionScale_X = 0.75f;
			groupButton.SizeOffset_X = 240;
			groupButton.SizeOffset_Y = 30;
			groupButton.AddLabel(localization.format("Group_Box_Label"), ESleekSide.LEFT);
			groupButton.onClickedButton += onClickedUngroupButton;
			container.AddChild(groupButton);

			groupsBox = Glazier.Get().CreateScrollView();
			groupsBox.PositionOffset_X = -120;
			groupsBox.PositionOffset_Y = 310;
			groupsBox.PositionScale_X = 0.75f;
			groupsBox.SizeOffset_X = 270;
			groupsBox.SizeOffset_Y = -410;
			groupsBox.SizeScale_Y = 1;
			groupsBox.ScaleContentToWidth = true;
			groupsBox.ContentSizeOffset = new Vector2(0.0f, (groups.Length * 40) - 10);
			container.AddChild(groupsBox);

			for (int index = 0; index < groups.Length; index++)
			{
				SleekButtonIcon button = new SleekButtonIcon(groups[index].icon, 20);
				button.PositionOffset_Y = index * 40;
				button.SizeOffset_X = 240;
				button.SizeOffset_Y = 30;
				button.text = groups[index].name;
				button.onClickedButton += onClickedGroupButton;
				groupsBox.AddChild(button);
			}

			Characters.onCharacterUpdated += onCharacterUpdated;
			onCharacterUpdated(Characters.selected, Characters.list[Characters.selected]);

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);
		}
	}
}
