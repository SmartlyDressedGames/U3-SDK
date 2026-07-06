////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuSurvivorsCharacterUI
	{
		public static Local localization;
		public static IconsBundle icons;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static ISleekScrollView characterBox;
		private static SleekCharacter[] characterButtons;

		private static ISleekField nameField;
		private static ISleekField nickField;
		private static SleekBoxIcon skillsetBox;
		//private static SleekToggle anonymousToggle;
		private static ISleekScrollView skillsetsBox;

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
				nameField.Text = character.name;
				nickField.Text = character.nick;
				skillsetBox.icon = character.skillset > 0 ? icons.load<Texture2D>("Skillset_" + (int) character.skillset) : null;
				skillsetBox.text = localization.format("Skillset_" + (byte) character.skillset);
			}

			characterButtons[index].updateCharacter(character);
		}

		private static void onTypedNameField(ISleekField field, string text)
		{
			Characters.rename(text);
		}

		private static void onTypedNickField(ISleekField field, string text)
		{
			Characters.renick(text);
		}

		private static void onClickedCharacter(SleekCharacter character, byte index)
		{
			Characters.selected = index;
		}

		private static void onClickedSkillset(ISleekElement button)
		{
			Characters.skillify((EPlayerSkillset) (button.PositionOffset_Y / 40));
		}

		//private static void onToggledAnonymousToggle(SleekToggle toggle, bool state)
		//{
		//	Characters.active.isAnonymous = state;
		//}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuSurvivorsUI.open();
			close();
		}

		public void OnDestroy()
		{
			Characters.onCharacterUpdated -= onCharacterUpdated;
		}

		public MenuSurvivorsCharacterUI()
		{
			localization = Localization.read("/Menu/Survivors/MenuSurvivorsCharacter.dat");
			icons = Bundles.getIconsBundle("UI/Menu/Icons/Survivors/MenuSurvivorsCharacter");

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

			characterBox = Glazier.Get().CreateScrollView();
			characterBox.PositionOffset_X = -100;
			characterBox.PositionOffset_Y = 45;
			characterBox.PositionScale_X = 0.75f;
			characterBox.PositionScale_Y = 0.5f;
			characterBox.SizeOffset_X = 230;
			characterBox.SizeOffset_Y = -145;
			characterBox.SizeScale_Y = 0.5f;
			characterBox.ScaleContentToWidth = true;
			characterBox.ContentSizeOffset = new Vector2(0.0f, ((Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS) * 80) - 10);
			container.AddChild(characterBox);

			characterButtons = new SleekCharacter[Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS];
			for (byte index = 0; index < characterButtons.Length; index++)
			{
				SleekCharacter character = new SleekCharacter(index);
				character.PositionOffset_Y = index * 80;
				character.SizeOffset_X = 200;
				character.SizeOffset_Y = 70;
				character.onClickedCharacter = onClickedCharacter;
				characterBox.AddChild(character);

				characterButtons[index] = character;
			}

			nameField = Glazier.Get().CreateStringField();
			nameField.PositionOffset_X = -100;
			nameField.PositionOffset_Y = 100;
			nameField.PositionScale_X = 0.75f;
			nameField.SizeOffset_X = 200;
			nameField.SizeOffset_Y = 30;
			nameField.MaxLength = 32;
			nameField.AddLabel(localization.format("Name_Field_Label"), ESleekSide.LEFT);
			nameField.OnTextChanged += onTypedNameField;
			container.AddChild(nameField);

			nickField = Glazier.Get().CreateStringField();
			nickField.PositionOffset_X = -100;
			nickField.PositionOffset_Y = 140;
			nickField.PositionScale_X = 0.75f;
			nickField.SizeOffset_X = 200;
			nickField.SizeOffset_Y = 30;
			nickField.MaxLength = 32;
			nickField.AddLabel(localization.format("Nick_Field_Label"), ESleekSide.LEFT);
			nickField.OnTextChanged += onTypedNickField;
			container.AddChild(nickField);

			skillsetBox = new SleekBoxIcon(null);
			skillsetBox.PositionOffset_X = -100;
			skillsetBox.PositionOffset_Y = 180;
			skillsetBox.PositionScale_X = 0.75f;
			skillsetBox.SizeOffset_X = 200;
			skillsetBox.SizeOffset_Y = 30;
			skillsetBox.iconColor = ESleekTint.FOREGROUND;
			skillsetBox.AddLabel(localization.format("Skillset_Box_Label"), ESleekSide.LEFT);
			container.AddChild(skillsetBox);

			//anonymousToggle = Glazier.Get().CreateToggle();
			//anonymousToggle.positionOffset_X = -100;
			//anonymousToggle.positionOffset_Y = -140;
			//anonymousToggle.positionScale_X = 0.75f;
			//anonymousToggle.positionScale_Y = 1.0f;
			//anonymousToggle.sizeOffset_X = 40;
			//anonymousToggle.sizeOffset_Y = 40;
			//anonymousToggle.onToggled += onToggledAnonymousToggle;
			//anonymousToggle.addLabel(localization.format("Anonymous"), ESleekSide.RIGHT);
			//container.add(anonymousToggle);

			skillsetsBox = Glazier.Get().CreateScrollView();
			skillsetsBox.PositionOffset_X = -100;
			skillsetsBox.PositionOffset_Y = 220;
			skillsetsBox.PositionScale_X = 0.75f;
			skillsetsBox.SizeOffset_X = 230;
			skillsetsBox.SizeOffset_Y = -185;
			skillsetsBox.SizeScale_Y = 0.5f;
			skillsetsBox.ScaleContentToWidth = true;
			skillsetsBox.ContentSizeOffset = new Vector2(0.0f, (Customization.SKILLSETS * 40) - 10);
			container.AddChild(skillsetsBox);

			for (int index = 0; index < Customization.SKILLSETS; index++)
			{
				SleekButtonIcon button = new SleekButtonIcon(index > 0 ? icons.load<Texture2D>("Skillset_" + index) : null);
				button.PositionOffset_Y = index * 40;
				button.SizeOffset_X = 200;
				button.SizeOffset_Y = 30;
				button.text = localization.format("Skillset_" + index);
				button.iconColor = ESleekTint.FOREGROUND;
				button.onClickedButton += onClickedSkillset;
				skillsetsBox.AddChild(button);
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
