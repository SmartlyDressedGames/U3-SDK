////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekBoost : SleekWrapper
	{
		public event ClickedButton onClickedButton;

		public SleekBoost(byte boost) : base()
		{
			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			button.TooltipText = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Tooltip");
			button.OnClicked += onClickedInternalButton;
			button.IsClickable = Player.LocalPlayer.skills.experience >= PlayerSkills.BOOST_COST;
			AddChild(button);

			infoLabel = Glazier.Get().CreateLabel();
			infoLabel.PositionOffset_X = 5;
			infoLabel.PositionOffset_Y = 5;
			infoLabel.SizeOffset_X = -10;
			infoLabel.SizeOffset_Y = -5;
			infoLabel.SizeScale_X = 0.5f;
			infoLabel.SizeScale_Y = 0.5f;
			infoLabel.TextAlignment = TextAnchor.MiddleLeft;
			infoLabel.Text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost);
			infoLabel.FontSize = ESleekFontSize.Medium;
			AddChild(infoLabel);

			descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = 5;
			descriptionLabel.PositionOffset_Y = 5;
			descriptionLabel.PositionScale_Y = 0.5f;
			descriptionLabel.SizeOffset_X = -10;
			descriptionLabel.SizeOffset_Y = -5;
			descriptionLabel.SizeScale_X = 0.5f;
			descriptionLabel.SizeScale_Y = 0.5f;
			descriptionLabel.TextAlignment = TextAnchor.MiddleLeft;
			descriptionLabel.Text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Tooltip");
			AddChild(descriptionLabel);

			if (boost > 0)
			{
				ISleekLabel bonus = Glazier.Get().CreateLabel();
				bonus.PositionOffset_X = 5;
				bonus.PositionOffset_Y = 5;
				bonus.PositionScale_X = 0.25f;
				bonus.SizeOffset_X = -10;
				bonus.SizeOffset_Y = -10;
				bonus.SizeScale_X = 0.5f;
				bonus.SizeScale_Y = 1;
				bonus.TextAlignment = TextAnchor.MiddleCenter;
				bonus.Text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Bonus");
				AddChild(bonus);
			}

			costLabel = Glazier.Get().CreateLabel();
			costLabel.PositionOffset_X = 5;
			costLabel.PositionOffset_Y = 5;
			costLabel.PositionScale_X = 0.5f;
			costLabel.SizeOffset_X = -10;
			costLabel.SizeOffset_Y = -10;
			costLabel.SizeScale_X = 0.5f;
			costLabel.SizeScale_Y = 1f;
			costLabel.TextAlignment = TextAnchor.MiddleRight;
			costLabel.Text = PlayerDashboardSkillsUI.localization.format("Cost", PlayerSkills.BOOST_COST);
			AddChild(costLabel);
		}

		private void onClickedInternalButton(ISleekElement internalButton)
		{
			onClickedButton?.Invoke(this);
		}

		private ISleekButton button;
		private ISleekLabel infoLabel;
		private ISleekLabel descriptionLabel;
		private ISleekLabel costLabel;
	}
}
