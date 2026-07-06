////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using Unturned.SystemEx;
using Steamworks;

namespace SDG.Unturned
{
	internal class SleekServerCurationRuleTester : SleekWrapper
	{
		public event System.Action OnInputChanged;

		public void TestRules(List<ServerListCurationRule> rules, bool mergedRules)
		{
			if (rules == null && !mergedRules)
			{
				matchBox.Text = localization.format("Test_NoMatch");
				return;
			}

			string testName = nameField.Text;
			if (string.IsNullOrWhiteSpace(testName))
			{
				matchBox.Text = localization.format("Test_NameInvalid");
				return;
			}

			IPv4Address testAddress = IPv4Address.Zero;
			ushort testQueryPort = 0;
			if (!IPv4Address.TryParseWithOptionalPort(addressField.Text, out testAddress, out ushort? optionalPort) || !optionalPort.HasValue)
			{
				matchBox.Text = localization.format("Test_AddressInvalid");
				return;
			}
			testQueryPort = optionalPort.Value;

			CSteamID testSteamId = CSteamID.Nil;
			if (!string.IsNullOrWhiteSpace(serverIdField.Text))
			{
				if (!ulong.TryParse(serverIdField.Text, out testSteamId.m_SteamID))
				{
					matchBox.Text = localization.format("Test_ServerIdInvalid");
					return;
				}
			}

			ServerListCurationInput curationInput = new ServerListCurationInput(testName, testAddress, testQueryPort, testSteamId);
			ServerListCurationOutput curationOutput = default;
			curationOutput.matchedRules = matchedRules;
			ServerListCuration curation = ServerListCuration.Get();
			if (mergedRules)
			{
				curation.RefreshIfDirty();
				curation.MergeRulesIfDirty();
				curation.EvaluateMergedRules(curationInput, ref curationOutput);
			}
			else
			{
				curation.Evaluate(rules, curationInput, ref curationOutput);
			}

			if (curationOutput.matchedAnyRules)
			{
				string matchCount = localization.format("Test_Match_Count", curationOutput.matchedRules.Count);
				string matchAllowed = localization.format(curationOutput.allowed ? "Test_Match_Allowed" : "Test_Match_Denied");
				string matchLabels;
				if (!string.IsNullOrEmpty(curationOutput.labels))
				{
					matchLabels = localization.format("Test_Match_HasLabels", curationOutput.labels);
				}
				else
				{
					matchLabels = localization.format("Test_Match_NoLabels");
				}

				string matchText = localization.format("Test_Match_Format", matchCount, matchAllowed, matchLabels);
				if (curationOutput.allowOrDenyRule != null)
				{
					string matchRule = localization.format("Test_Match_Rule", curationOutput.allowOrDenyRule.description, curationOutput.allowOrDenyRule.owner.Name);
					matchText = $"{matchText}\n{matchRule}";
				}

				matchBox.Text = matchText;
			}
			else
			{
				matchBox.Text = localization.format("Test_NoMatch");
			}
		}

		internal SleekServerCurationRuleTester(Local localization) : base()
		{
			this.localization = localization;

			nameField = Glazier.Get().CreateStringField();
			nameField.SizeOffset_X = -200;
			nameField.SizeOffset_Y = 30;
			nameField.SizeScale_X = 0.333f;
			nameField.AddLabel(localization.format("Test_Input_Name_Label"), ESleekSide.RIGHT);
			nameField.TooltipText = localization.format("Test_Input_Name_Tooltip");
			nameField.OnTextChanged += OnTextChanged;
			AddChild(nameField);

			addressField = Glazier.Get().CreateStringField();
			addressField.PositionScale_X = 0.333f;
			addressField.SizeOffset_X = -200;
			addressField.SizeOffset_Y = 30;
			addressField.SizeScale_X = 0.333f;
			addressField.Text = "127.0.0.1:27015";
			addressField.AddLabel(localization.format("Test_Input_Address_Label"), ESleekSide.RIGHT);
			addressField.TooltipText = localization.format("Test_Input_Address_Tooltip");
			addressField.OnTextChanged += OnTextChanged;
			AddChild(addressField);

			serverIdField = Glazier.Get().CreateStringField();
			serverIdField.PositionScale_X = 0.666f;
			serverIdField.SizeOffset_X = -200;
			serverIdField.SizeOffset_Y = 30;
			serverIdField.SizeScale_X = 0.333f;
			serverIdField.AddLabel(localization.format("Test_Input_ServerID_Label"), ESleekSide.RIGHT);
			serverIdField.TooltipText = localization.format("Test_Input_ServerID_Tooltip");
			serverIdField.OnTextChanged += OnTextChanged;
			AddChild(serverIdField);

			matchBox = Glazier.Get().CreateBox();
			matchBox.PositionOffset_Y = 30;
			matchBox.SizeOffset_Y = 50;
			matchBox.SizeScale_X = 1.0f;
			matchBox.AllowRichText = true;
			matchBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			matchBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			AddChild(matchBox);
		}

		private void OnTextChanged(ISleekField field, string text)
		{
			OnInputChanged?.Invoke();
		}

		private void OnInputTypeChanged(SleekButtonState button, int state)
		{
			OnInputChanged?.Invoke();
		}

		private Local localization;
		private ISleekField nameField;
		private ISleekField addressField;
		private ISleekField serverIdField;
		private ISleekLabel matchBox;
		private List<ServerListCurationRule> matchedRules = new List<ServerListCurationRule>();
	}
}
