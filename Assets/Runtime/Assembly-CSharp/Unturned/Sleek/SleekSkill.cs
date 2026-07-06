////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekSkill : SleekWrapper
	{
		public event ClickedButton onClickedButton;

		public SleekSkill(byte speciality, byte index, Skill skill) : base()
		{
			uint cost = Player.LocalPlayer.skills.cost(speciality, index);

			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			button.TooltipText = PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index + "_Tooltip");
			button.OnClicked += onClickedInternalButton;
			button.IsClickable = Player.LocalPlayer.skills.experience >= cost && skill.level < skill.GetClampedMaxUnlockableLevel();
			AddChild(button);

			for (byte barIndex = 0; barIndex < skill.GetClampedMaxUnlockableLevel(); barIndex++)
			{
				ISleekImage bar = Glazier.Get().CreateImage();
				bar.PositionOffset_X = -20 - (barIndex * 20);
				bar.PositionOffset_Y = 10;
				bar.PositionScale_X = 1;
				bar.SizeOffset_X = 10;
				bar.SizeOffset_Y = -10;
				bar.SizeScale_Y = 0.5f;

				if (barIndex < skill.level)
				{
					bar.Texture = PlayerDashboardSkillsUI.icons.load<Texture2D>("Unlocked");
				}
				else
				{
					bar.Texture = PlayerDashboardSkillsUI.icons.load<Texture2D>("Locked");
				}

				AddChild(bar);
			}

			ISleekLabel infoLabel = Glazier.Get().CreateLabel();
			infoLabel.PositionOffset_X = 5;
			infoLabel.PositionOffset_Y = 5;
			infoLabel.SizeOffset_X = -10;
			infoLabel.SizeOffset_Y = 30;
			infoLabel.SizeScale_X = 0.5f;
			infoLabel.TextAlignment = TextAnchor.UpperLeft;
			infoLabel.Text = PlayerDashboardSkillsUI.localization.format("Skill", PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index), PlayerDashboardSkillsUI.localization.format("Level_" + skill.level));
			infoLabel.FontSize = ESleekFontSize.Medium;
			AddChild(infoLabel);

			ISleekImage skillsetIcon = Glazier.Get().CreateImage();
			skillsetIcon.PositionOffset_X = 10;
			skillsetIcon.PositionOffset_Y = -10;
			skillsetIcon.PositionScale_Y = 0.5f;
			skillsetIcon.SizeOffset_X = 20;
			skillsetIcon.SizeOffset_Y = 20;
			skillsetIcon.TintColor = ESleekTint.FOREGROUND;

			for (byte search = 0; search < PlayerSkills.SKILLSETS.Length; search++)
			{
				for (byte search2 = 0; search2 < PlayerSkills.SKILLSETS[search].Length; search2++)
				{
					SpecialitySkillPair pair = PlayerSkills.SKILLSETS[search][search2];

					if (speciality == pair.speciality && index == pair.skill)
					{
						skillsetIcon.Texture = MenuSurvivorsCharacterUI.icons.load<Texture2D>("Skillset_" + search);
						break;
					}
				}
			}

			AddChild(skillsetIcon);

			ISleekLabel descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = 5;
			descriptionLabel.PositionOffset_Y = -35;
			descriptionLabel.PositionScale_Y = 1.0f;
			descriptionLabel.SizeOffset_X = -10;
			descriptionLabel.SizeOffset_Y = 30;
			descriptionLabel.SizeScale_X = 0.5f;
			descriptionLabel.TextAlignment = TextAnchor.LowerLeft;
			descriptionLabel.Text = PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index + "_Tooltip");
			AddChild(descriptionLabel);

			if (skill.level > 0)
			{
				ISleekLabel bonus = Glazier.Get().CreateLabel();
				bonus.PositionOffset_X = 5;
				bonus.PositionOffset_Y = 5;
				bonus.PositionScale_X = 0.25f;
				bonus.SizeOffset_X = -10;
				bonus.SizeOffset_Y = -10;
				bonus.SizeScale_X = 0.5f;
				bonus.SizeScale_Y = 0.5f;
				bonus.TextAlignment = TextAnchor.MiddleCenter;
				bonus.Text = PlayerDashboardSkillsUI.localization.format("Bonus_Current", FormatLevelString(speciality, index, skill.level));
				AddChild(bonus);
			}

			if (skill.level < skill.GetClampedMaxUnlockableLevel())
			{
				ISleekLabel next = Glazier.Get().CreateLabel();
				next.PositionOffset_X = 5;
				next.PositionOffset_Y = 5;
				next.PositionScale_X = 0.25f;
				next.PositionScale_Y = 0.5f;
				next.SizeOffset_X = -10;
				next.SizeOffset_Y = -10;
				next.SizeScale_X = 0.5f;
				next.SizeScale_Y = 0.5f;
				next.TextAlignment = TextAnchor.MiddleCenter;
				next.Text = PlayerDashboardSkillsUI.localization.format("Bonus_Next", FormatLevelString(speciality, index, skill.level + 1));
				AddChild(next);
			}

			ISleekLabel costLabel = Glazier.Get().CreateLabel();
			costLabel.PositionOffset_X = 5;
			costLabel.PositionOffset_Y = -35;
			costLabel.PositionScale_X = 0.5f;
			costLabel.PositionScale_Y = 1f;
			costLabel.SizeOffset_X = -10;
			costLabel.SizeOffset_Y = 30;
			costLabel.SizeScale_X = 0.5f;
			costLabel.TextAlignment = TextAnchor.LowerRight;

			if (skill.level < skill.GetClampedMaxUnlockableLevel())
			{
				costLabel.Text = PlayerDashboardSkillsUI.localization.format("Cost", cost);
			}
			else
			{
				costLabel.Text = PlayerDashboardSkillsUI.localization.format("Full");
			}

			AddChild(costLabel);
		}

		private string FormatLevelString(byte speciality, byte index, int level)
		{
			// Nelson 2024-12-16: Hacked in because per-level benefits were hardcoded in translation file. :(
			if (speciality == (byte) EPlayerSpeciality.OFFENSE && index == (byte) EPlayerOffense.SHARPSHOOTER)
			{
				float multiplier = 1.0f - Player.LocalPlayer.skills.GetSharpshooterRecoilMultiplierForLevel(level);
				// Doesn't use P0 format for consistency with other skills.
				string multiplierText = $"{Mathf.RoundToInt(multiplier * 100.0f)}%";
				return PlayerDashboardSkillsUI.localization.format("Speciality_0_Skill_1_Levels_V2", multiplierText, multiplierText);
			}
			else
			{
				return PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index + "_Level_" + level);
			}
		}

		private void onClickedInternalButton(ISleekElement internalButton)
		{
			onClickedButton?.Invoke(this);
		}

		private ISleekButton button;
	}
}
