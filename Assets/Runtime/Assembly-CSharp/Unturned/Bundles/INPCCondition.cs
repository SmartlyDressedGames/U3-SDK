////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal readonly struct PopulateConditionParameters
	{
		public readonly ENPCConditionType conditionType;
		public readonly IDatDictionary data;
		public readonly Local localization;
		public readonly IAssetErrorContext errorContext;

		public readonly string errorAdditionalInfo;

		/// <summary>
		/// Should only be used by <see cref="INPCCondition.PopulateLegacy(in PopulateConditionParameters)"/>.
		/// For example: "Condition_##" where ## is an index.
		/// </summary>
		public readonly string legacyPrefix;

		/// <summary>
		/// Nelson 2025-03-11: not *super* happy about having this in here. Needed for UI_Requirements.
		/// </summary>
		public readonly int conditionIndex;

		/// <summary>
		/// Nelson 2025-03-11: not *super* happy about having this in here. Needed for UI_Requirements.
		/// </summary>
		public readonly int conditionsLength;

		public void ReportError(string message)
		{
			if (!string.IsNullOrEmpty(errorAdditionalInfo))
			{
				errorContext.ReportAssetError($"{errorAdditionalInfo} ({conditionType} condition) {message}");
			}
			else if (!string.IsNullOrEmpty(legacyPrefix))
			{
				errorContext.ReportAssetError($"{legacyPrefix} ({conditionType} condition) {message}");
			}
			else
			{
				errorContext.ReportAssetError($"({conditionType} condition) {message}");
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

		public PopulateConditionParameters(ENPCConditionType conditionType, IDatDictionary data, Local localization,
			IAssetErrorContext errorContext, string errorInfo, string legacyPrefix, int conditionIndex, int conditionsLength)
		{
			this.conditionType = conditionType;
			this.data = data;
			this.localization = localization;
			this.errorContext = errorContext;
			this.errorAdditionalInfo = errorInfo;
			this.legacyPrefix = legacyPrefix;
			this.conditionIndex = conditionIndex;
			this.conditionsLength = conditionsLength;
		}
	}

	public class INPCCondition
	{
		protected string text;
		protected bool shouldReset;

		/// <summary>
		/// If set, only show this condition in the UI when conditions with these indices are met.
		/// For example don't show "arrest the criminal (name)" until "investigate crime" is completed.
		/// </summary>
		internal List<int> uiRequirementIndices;

		// do they have # item?
		// is key/value set?
		public virtual bool isConditionMet(Player player)
		{
			return false;
		}

		// remove items
		public virtual void ApplyCondition(Player player)
		{ }

		public virtual string formatCondition(Player player)
		{
			return string.IsNullOrEmpty(text) ? null : text;
		}

		public virtual ISleekElement createUI(Player player, Texture2D icon)
		{
			string text = formatCondition(player);

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			ISleekBox conditionBox = Glazier.Get().CreateBox();
			conditionBox.SizeOffset_Y = 30;
			conditionBox.SizeScale_X = 1;

			if (icon != null)
			{
				ISleekImage iconImage = Glazier.Get().CreateImage(icon);
				iconImage.PositionOffset_X = 5;
				iconImage.PositionOffset_Y = 5;
				iconImage.SizeOffset_X = 20;
				iconImage.SizeOffset_Y = 20;
				conditionBox.AddChild(iconImage);
			}

			ISleekLabel conditionLabel = Glazier.Get().CreateLabel();

			if (icon != null)
			{
				conditionLabel.PositionOffset_X = 30;
				conditionLabel.SizeOffset_X = -35;
			}
			else
			{
				conditionLabel.PositionOffset_X = 5;
				conditionLabel.SizeOffset_X = -10;
			}

			conditionLabel.SizeScale_X = 1;
			conditionLabel.SizeScale_Y = 1;
			conditionLabel.TextAlignment = TextAnchor.MiddleLeft;
			conditionLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			conditionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			conditionLabel.AllowRichText = true;
			conditionLabel.Text = text;
			conditionBox.AddChild(conditionLabel);

			return conditionBox;
		}

		/// <summary>
		/// Is this condition influenced by a given quest flag?
		/// Used by level objects to determine if local player's flag change may affect visibility.
		/// </summary>
		public virtual bool isAssociatedWithFlag(ushort flagID)
		{
			return false;
		}

		/// <summary>
		/// Replacement for isAssociatedWithFlag to fix quest conditions and somewhat improve perf.
		/// </summary>
		internal virtual void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
		{
		}

		/// <summary>
		/// Intended to replace filling data from constructor.
		/// </summary>
		internal virtual void PopulateV2(in PopulateConditionParameters p)
		{
			string textId = p.data.GetString("TextId");
			if (!string.IsNullOrEmpty(textId))
			{
				string conditionText = p.localization.FormatOrEmpty(textId);
				if (!string.IsNullOrEmpty(conditionText))
				{
					conditionText = ItemTool.filterRarityRichText(conditionText);
					text = conditionText;
				}
				else
				{
					p.ReportError($"no text for condition text ID \"{textId}\"");
				}
			}

			shouldReset = p.data.ParseBool("Reset");

			if (p.data.TryGetString("UI_Requirements", out string uiRequirements))
			{
				ParseUIRequirements(in p, uiRequirements);
			}
		}

		/// <summary>
		/// Intended to replace filling data from constructor. Legacy is for backwards compatibility with Condition_#_Key
		/// format, whereas V2 uses the list and dictionary features.
		/// </summary>
		internal virtual void PopulateLegacy(in PopulateConditionParameters p)
		{
			// Nelson 2024-03-20: It's important that conditionText can be null/empty here (i.e., not formatted with
			// the default KEY_NAME text) so that UI creation is skipped. (public issue #4388)
			string conditionText = p.localization.FormatOrEmpty(p.legacyPrefix);
			conditionText = ItemTool.filterRarityRichText(conditionText);
			text = conditionText;

			shouldReset = p.data.ContainsKey(p.legacyPrefix + "_Reset");

			if (p.data.TryGetString(p.legacyPrefix + "_UI_Requirements", out string uiRequirements))
			{
				ParseUIRequirements(in p, uiRequirements);
			}
		}

		private void ParseUIRequirements(in PopulateConditionParameters p, string uiRequirements)
		{
			string[] splitUiRequirements = uiRequirements.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
			if (splitUiRequirements == null || splitUiRequirements.Length < 1)
			{
				p.ReportError("UI_Requirements are empty");
			}
			else
			{
				List<int> tempRequirementIndices = new List<int>(splitUiRequirements.Length);
				foreach (string item in splitUiRequirements)
				{
					if (!int.TryParse(item, out int uiRequirementIndex))
					{
						p.ReportError($"unable to parse UI Requirement index from \"{item}\"");
						continue;
					}

					if (uiRequirementIndex < 0 || uiRequirementIndex >= p.conditionsLength)
					{
						p.ReportError($"UI Requirement index {uiRequirementIndex} out of bounds");
						continue;
					}

					if (uiRequirementIndex == p.conditionIndex)
					{
						p.ReportError("UI Requirement depends on itself");

						continue;
					}

					tempRequirementIndices.Add(uiRequirementIndex);
				}

				if (tempRequirementIndices.Count > 0)
				{
					uiRequirementIndices = tempRequirementIndices;
				}
			}
		}

		public bool AreUIRequirementsMet(List<bool> areConditionsMet)
		{
			if (uiRequirementIndices == null || uiRequirementIndices.Count < 1)
			{
				// UI requirements are unused.
				return true;
			}

			foreach (int requirementIndex in uiRequirementIndices)
			{
				if (requirementIndex < 0 || requirementIndex >= areConditionsMet.Count)
				{
					// Should have been caught during parsing, or areConditionsMet parameter is bad.
					continue;
				}

				if (!areConditionsMet[requirementIndex])
				{
					return false;
				}
			}

			return true;
		}

		public virtual string GetTypeFriendlyName()
		{
			string friendlyName = GetType().Name;
			if (friendlyName.StartsWith("NPC", System.StringComparison.Ordinal))
			{
				friendlyName = friendlyName.Substring("NPC".Length);
			}
			if (friendlyName.EndsWith("Condition", System.StringComparison.Ordinal))
			{
				friendlyName = friendlyName.Substring(0, friendlyName.Length - "Condition".Length);
			}

			System.Text.StringBuilder sb = new System.Text.StringBuilder(32);
			for (int letterIndex = 0; letterIndex < friendlyName.Length; ++letterIndex)
			{
				char letter = friendlyName[letterIndex];
				if (letterIndex > 0 && char.IsUpper(letter) && !char.IsUpper(friendlyName[letterIndex - 1]))
				{
					sb.Append(' ');
				}
				sb.Append(letter);
			}

			return sb.ToString();
		}

		public virtual void DebugDumpToStringBuilder(Player player, System.Text.StringBuilder sb)
		{
			sb.Append(GetTypeFriendlyName());
		}

		public INPCCondition() { }

		[System.Obsolete]
		public INPCCondition(string newText, bool newShouldReset)
		{
			text = newText;
			shouldReset = newShouldReset;
		}

		[System.Obsolete("Removed shouldSend parameter because ApplyCondition is only called on the server now")]
		public virtual void applyCondition(Player player, bool shouldSend)
		{
			ApplyCondition(player);
		}
	}
}
