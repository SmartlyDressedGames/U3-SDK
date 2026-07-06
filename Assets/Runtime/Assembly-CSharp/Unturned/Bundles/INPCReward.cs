////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal readonly struct PopulateRewardParameters
	{
		public readonly ENPCRewardType rewardType;
		public readonly IDatDictionary data;
		public readonly Local localization;
		public readonly IAssetErrorContext errorContext;

		public readonly string errorAdditionalInfo;

		/// <summary>
		/// Should only be used by <see cref="INPCReward.PopulateLegacy(in PopulateRewardParameters)"/>.
		/// For example: "Condition_##" where ## is an index.
		/// </summary>
		public readonly string legacyPrefix;

		public void ReportError(string message)
		{
			if (!string.IsNullOrEmpty(errorAdditionalInfo))
			{
				errorContext.ReportAssetError($"{errorAdditionalInfo} ({rewardType} reward) {message}");
			}
			else if (!string.IsNullOrEmpty(legacyPrefix))
			{
				errorContext.ReportAssetError($"{legacyPrefix} ({rewardType} reward) {message}");
			}
			else
			{
				errorContext.ReportAssetError($"({rewardType} reward) {message}");
			}
		}

		public void ReportRequiredOptionInvalid(string key)
		{
			if (!string.IsNullOrEmpty(legacyPrefix))
			{
				key = $"{legacyPrefix}_{key}";
			}

			if (data.ContainsKey(key))
			{
				ReportError($"unable to parse {key} from \"{data.GetString(key)}\"");
			}
			else
			{
				ReportError($"requires {key}");
			}
		}

		public PopulateRewardParameters(ENPCRewardType rewardType, IDatDictionary data, Local localization,
			IAssetErrorContext errorContext, string errorInfo, string legacyPrefix)
		{
			this.rewardType = rewardType;
			this.data = data;
			this.localization = localization;
			this.errorContext = errorContext;
			this.errorAdditionalInfo = errorInfo;
			this.legacyPrefix = legacyPrefix;
		}
	}

	public class INPCReward
	{
		/// <summary>
		/// If >0 the game will start a coroutine to grant the reward after waiting.
		/// </summary>
		public float grantDelaySeconds = -1.0f;

		/// <summary>
		/// If true and player has this reward pending when they die or disconnect it will be granted.
		/// </summary>
		public bool grantDelayApplyWhenInterrupted = false;

		protected string text;

		// give xp, items, rep, etc
		public virtual void GrantReward(Player player)
		{ }

		public virtual string formatReward(Player player)
		{
			return string.IsNullOrEmpty(text) ? null : text;
		}

		public virtual ISleekElement createUI(Player player)
		{
			string text = formatReward(player);

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			ISleekBox rewardBox = Glazier.Get().CreateBox();
			rewardBox.SizeOffset_Y = 30;
			rewardBox.SizeScale_X = 1;

			ISleekLabel rewardLabel = Glazier.Get().CreateLabel();
			rewardLabel.PositionOffset_X = 5;
			rewardLabel.SizeOffset_X = -10;
			rewardLabel.SizeScale_X = 1;
			rewardLabel.SizeScale_Y = 1;
			rewardLabel.TextAlignment = TextAnchor.MiddleLeft;
			rewardLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			rewardLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			rewardLabel.AllowRichText = true;
			rewardLabel.Text = text;
			rewardBox.AddChild(rewardLabel);

			return rewardBox;
		}

		/// <summary>
		/// Intended to replace filling data from constructor.
		/// </summary>
		internal virtual void PopulateV2(in PopulateRewardParameters p)
		{
			string textId = p.data.GetString("TextId");
			if (!string.IsNullOrEmpty(textId))
			{
				string rewardText = p.localization.FormatOrEmpty(textId);
				if (!string.IsNullOrEmpty(rewardText))
				{
					rewardText = ItemTool.filterRarityRichText(rewardText);
					text = rewardText;
				}
				else
				{
					p.ReportError($"no text for reward text ID \"{textId}\"");
				}
			}

			grantDelaySeconds = p.data.ParseFloat("GrantDelaySeconds", defaultValue: -1.0f);
			if (grantDelaySeconds > 0.0f)
			{
				grantDelayApplyWhenInterrupted = p.data.ParseBool("GrantDelayApplyWhenInterrupted", defaultValue: false);
			}
		}

		/// <summary>
		/// Intended to replace filling data from constructor. Legacy is for backwards compatibility with Reward_#_Key
		/// format, whereas V2 uses the list and dictionary features.
		/// </summary>
		internal virtual void PopulateLegacy(in PopulateRewardParameters p)
		{
			// Nelson 2024-03-20: It's important that rewardText can be null/empty here (i.e., not formatted with
			// the default KEY_NAME text) so that UI creation is skipped. (public issue #4388)
			string rewardText = p.localization.FormatOrEmpty(p.legacyPrefix);
			rewardText = ItemTool.filterRarityRichText(rewardText);
			text = rewardText;

			grantDelaySeconds = p.data.ParseFloat(p.legacyPrefix + "_GrantDelaySeconds", defaultValue: -1.0f);
			if (grantDelaySeconds > 0.0f)
			{
				grantDelayApplyWhenInterrupted = p.data.ParseBool(p.legacyPrefix + "_GrantDelayApplyWhenInterrupted", defaultValue: false);
			}
		}

		public INPCReward() { }

		[System.Obsolete]
		public INPCReward(string newText)
		{
			text = newText;
		}

		[System.Obsolete("Removed shouldSend parameter because GrantReward is only called on the server now")]
		public virtual void grantReward(Player player, bool shouldSend)
		{
			GrantReward(player);
		}
	}
}
