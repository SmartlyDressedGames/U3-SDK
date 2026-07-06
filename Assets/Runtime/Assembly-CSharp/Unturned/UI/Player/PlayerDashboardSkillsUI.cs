////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerDashboardSkillsUI
	{
		public static Local localization;
		public static IconsBundle icons;
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekBox backdropBox;

		private static Skill[] skills;
		private static ISleekScrollView skillsScrollBox;
		private static SleekBoost boostButton;
		private static ISleekBox experienceBox;

		private static byte selectedSpeciality;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			updateSelection(selectedSpeciality);

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

		private static void updateSelection(byte specialityIndex)
		{
			skills = Player.LocalPlayer.skills.skills[specialityIndex];

			skillsScrollBox.RemoveAllChildren();
			skillsScrollBox.ContentSizeOffset = new Vector2(0.0f, (skills.Length * 90) - 10);

			for (byte skillIndex = 0; skillIndex < skills.Length; skillIndex++)
			{
				Skill skill = skills[skillIndex];

				SleekSkill skillButton = new SleekSkill(specialityIndex, skillIndex, skill);
				skillButton.PositionOffset_Y = skillIndex * 90;
				skillButton.SizeOffset_Y = 80;
				skillButton.SizeScale_X = 1;
				skillButton.onClickedButton += onClickedSkillButton;
				skillsScrollBox.AddChild(skillButton);
			}

			if (boostButton != null)
			{
				backdropBox.RemoveChild(boostButton);
			}

			boostButton = new SleekBoost((byte) Player.LocalPlayer.skills.boost);
			boostButton.PositionOffset_X = 5;
			boostButton.PositionOffset_Y = -90;
			boostButton.PositionScale_X = 0.5f;
			boostButton.PositionScale_Y = 1;
			boostButton.SizeOffset_X = -15;
			boostButton.SizeOffset_Y = 80;
			boostButton.SizeScale_X = 0.5f;
			boostButton.onClickedButton += onClickedBoostButton;
			backdropBox.AddChild(boostButton);

			selectedSpeciality = specialityIndex;
		}

		private static void onClickedSpecialityButton(ISleekElement button)
		{
			byte specialityIndex = (byte) ((button.PositionOffset_X + 85) / 60);

			updateSelection(specialityIndex);
		}

		private static void onClickedBoostButton(ISleekElement button)
		{
			if (Player.LocalPlayer.skills.experience >= PlayerSkills.BOOST_COST)
			{
				Player.LocalPlayer.skills.sendBoost();
			}
		}

		private static void onClickedSkillButton(ISleekElement button)
		{
			byte index = (byte) (button.PositionOffset_Y / 90);

			if (skills[index].level < skills[index].GetClampedMaxUnlockableLevel())
			{
				if (Player.LocalPlayer.skills.experience >= Player.LocalPlayer.skills.cost(selectedSpeciality, index))
				{
					Player.LocalPlayer.skills.sendUpgrade(selectedSpeciality, index, InputEx.GetKey(ControlsSettings.other));
				}
			}
		}

		private static void onExperienceUpdated(uint newExperience)
		{
			experienceBox.Text = localization.format("Experience", newExperience.ToString());
		}

		private static void onBoostUpdated(EPlayerBoost newBoost)
		{
			if (!active || !PlayerDashboardUI.active)
				return;

			updateSelection(selectedSpeciality);
		}

		private static void onSkillsUpdated()
		{
			if (!active || !PlayerDashboardUI.active)
				return;

			updateSelection(selectedSpeciality);
		}

		public PlayerDashboardSkillsUI()
		{
			localization = Localization.read("/Player/PlayerDashboardSkills.dat");
			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerDashboardSkills");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;
			selectedSpeciality = 255;

			backdropBox = Glazier.Get().CreateBox();
			backdropBox.PositionOffset_Y = 60;
			backdropBox.SizeOffset_Y = -60;
			backdropBox.SizeScale_X = 1;
			backdropBox.SizeScale_Y = 1;
			backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
			container.AddChild(backdropBox);

			experienceBox = Glazier.Get().CreateBox();
			experienceBox.PositionOffset_X = 10;
			experienceBox.PositionOffset_Y = -90;
			experienceBox.PositionScale_Y = 1;
			experienceBox.SizeOffset_X = -15;
			experienceBox.SizeOffset_Y = 80;
			experienceBox.SizeScale_X = 0.5f;
			experienceBox.FontSize = ESleekFontSize.Medium;
			backdropBox.AddChild(experienceBox);

			for (int specialityIndex = 0; specialityIndex < PlayerSkills.SPECIALITIES; specialityIndex++)
			{
				SleekButtonIcon specialityButton = new SleekButtonIcon(icons.load<Texture2D>("Speciality_" + specialityIndex));
				specialityButton.PositionOffset_X = -85 + (specialityIndex * 60);
				specialityButton.PositionOffset_Y = 10;
				specialityButton.PositionScale_X = 0.5f;
				specialityButton.SizeOffset_X = 50;
				specialityButton.SizeOffset_Y = 50;
				specialityButton.tooltip = localization.format("Speciality_" + specialityIndex + "_Tooltip");
				specialityButton.iconColor = ESleekTint.FOREGROUND;
				specialityButton.onClickedButton += onClickedSpecialityButton;
				backdropBox.AddChild(specialityButton);
			}

			skillsScrollBox = Glazier.Get().CreateScrollView();
			skillsScrollBox.PositionOffset_X = 10;
			skillsScrollBox.PositionOffset_Y = 70;
			skillsScrollBox.SizeOffset_X = -20;
			skillsScrollBox.SizeOffset_Y = -170;
			skillsScrollBox.SizeScale_X = 1;
			skillsScrollBox.SizeScale_Y = 1;
			skillsScrollBox.ScaleContentToWidth = true;
			backdropBox.AddChild(skillsScrollBox);

			boostButton = null;

			updateSelection(0);

			Player.LocalPlayer.skills.onExperienceUpdated += onExperienceUpdated;
			onExperienceUpdated(Player.LocalPlayer.skills.experience);
			Player.LocalPlayer.skills.onBoostUpdated += onBoostUpdated;
			Player.LocalPlayer.skills.onSkillsUpdated += onSkillsUpdated;
		}
	}
}
