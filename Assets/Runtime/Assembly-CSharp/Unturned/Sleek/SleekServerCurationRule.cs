////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Entry in the MenuPlayServerCurationRulesUI list.
	/// </summary>
	public class SleekServerCurationRule : SleekWrapper
	{
		public void SynchronizeBlockCount()
		{
			if (rule.latestBlockedServerCount > 0)
			{
				blockCountLabel.Text = localization.format("BlockCount", rule.latestBlockedServerCount);
				blockCountLabel.IsVisible = true;
			}
			else
			{
				blockCountLabel.IsVisible = false;
			}
		}

		internal SleekServerCurationRule(MenuPlayServerCurationRulesUI rulesUI, ServerListCurationRule rule) : base()
		{
			localization = rulesUI.localization;
			this.rule = rule;

			ISleekBox box = Glazier.Get().CreateBox();
			box.SizeScale_X = 1;
			box.SizeScale_Y = 1;

			string action;
			switch (rule.action)
			{
				case EServerListCurationAction.Label:
					action = localization.format("Rule_Action_Label");
					break;

				case EServerListCurationAction.Allow:
					action = localization.format("Rule_Action_Allow");
					break;

				case EServerListCurationAction.Deny:
					action = localization.format("Rule_Action_Deny");
					break;

				default:
					action = $"Unknown ({rule.action})";
					break;
			}

			string type;
			string value;
			switch (rule.ruleType)
			{
				case EServerListCurationRuleType.Name:
					type = localization.format("Rule_Type_Name");
					value = GetValueString(rule.regexes);
					break;

				case EServerListCurationRuleType.IPv4:
					type = localization.format("Rule_Type_IPv4");
					value = GetValueString(rule.ipv4Filters);
					break;

				case EServerListCurationRuleType.ServerID:
					type = localization.format("Rule_Type_ServerID");
					value = GetValueString(rule.steamIds);
					break;

				default:
					type = $"Unknown ({rule.ruleType})";
					value = string.Empty;
					break;
			}

			string formatKey = rule.inverted ? "Rule_Inverted_Format" : "Rule_NotInverted_Format";
			
			ISleekLabel descriptionLabel = Glazier.Get().CreateLabel();
			descriptionLabel.PositionOffset_X = 5;
			descriptionLabel.SizeScale_X = 1;
			descriptionLabel.SizeOffset_X = -10;
			descriptionLabel.TextAlignment = TextAnchor.MiddleLeft;
			descriptionLabel.Text = rule.description;
			descriptionLabel.SizeOffset_Y = 30;
			box.AddChild(descriptionLabel);

			ISleekLabel ruleLabel = Glazier.Get().CreateLabel();
			ruleLabel.PositionOffset_X = 5;
			ruleLabel.PositionOffset_Y = 15;
			ruleLabel.SizeScale_X = 1;
			ruleLabel.SizeOffset_X = -10;
			ruleLabel.SizeOffset_Y = 30;
			ruleLabel.FontSize = ESleekFontSize.Small;
			ruleLabel.TextAlignment = TextAnchor.MiddleLeft;
			ruleLabel.Text = localization.format(formatKey, action, type, value);
			box.AddChild(ruleLabel);

			if (!string.IsNullOrEmpty(rule.label))
			{
				ISleekLabel labelLabel = Glazier.Get().CreateLabel();
				labelLabel.PositionOffset_X = 5;
				labelLabel.SizeScale_X = 1;
				labelLabel.SizeOffset_X = -10;
				labelLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				labelLabel.AllowRichText = true;
				labelLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				labelLabel.TextAlignment = TextAnchor.MiddleRight;
				labelLabel.Text = localization.format("Rule_ApplyLabel", rule.label);
				labelLabel.SizeOffset_Y = 30;
				box.AddChild(labelLabel);
			}

			blockCountLabel = Glazier.Get().CreateLabel();
			blockCountLabel.PositionOffset_X = 5;
			blockCountLabel.PositionOffset_Y = 15;
			blockCountLabel.SizeScale_X = 1;
			blockCountLabel.SizeOffset_X = -10;
			blockCountLabel.SizeOffset_Y = 30;
			blockCountLabel.FontSize = ESleekFontSize.Small;
			blockCountLabel.TextAlignment = TextAnchor.MiddleRight;
			box.AddChild(blockCountLabel);

			SynchronizeBlockCount();

			AddChild(box);
		}

		private string GetValueString<T>(T[] values)
		{
			string result = string.Empty;
			if (values != null && values.Length > 0)
			{
				result += values[0].ToString();

				for (int index = 1; index < values.Length; ++index)
				{
					result += ' ';
					result += values[index].ToString();
				}
			}

			return result;
		}

		private Local localization;
		private ServerListCurationRule rule;
		private ISleekLabel blockCountLabel;
	}
}
