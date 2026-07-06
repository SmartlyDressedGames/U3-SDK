////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class MenuPlayConfigUI
	{
		public static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;
		private static ISleekButton defaultButton;

		private static ISleekScrollView configBox;

		private static ModeConfigData defaultModeConfigData;
		private static Dictionary<FieldInfo, SleekConfigProperty> propertyWidgets;
		private static Dictionary<FieldInfo, object> propertyOverrides;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			if (propertyWidgets == null)
			{
				CreatePropertyWidgets();
			}

			defaultModeConfigData = ModeConfigData.CreateDefault(PlaySettings.singleplayerMode, true);
			propertyOverrides.Clear();

			string v2FilePath = PlayConfigUtils.GetSingleplayerConfigPathV2(Characters.selected, PlaySettings.singleplayerMode);
			if (File.Exists(v2FilePath))
			{
				IDatDictionary parsedRootDictionary = null;
				try
				{
					using (FileStream fileStream = new FileStream(v2FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					using (StreamReader streamReader = new StreamReader(fileStream))
					{
						DatParser parser = new DatParser();
						parser.EnableMetadata = true;
						parsedRootDictionary = parser.Parse(streamReader);

						if (parser.HasError)
						{
							CommandWindow.LogWarning("Error(s) parsing gameplay config:");
							foreach (string message in parser.ErrorMessages)
							{
								CommandWindow.LogWarning(message);
							}
						}
					}
				}
				catch (System.Exception e)
				{
					UnturnedLog.exception(e, $"Caught exception parsing v2 gameplay config for menu:");
				}

				try
				{
					ModeConfigData parsedModeConfigData = ModeConfigData.CreateDefault(PlaySettings.singleplayerMode, true);
					PlayConfigUtils.ParseModeConfig(parsedRootDictionary, parsedModeConfigData, propertyOverrides);
				}
				catch (System.Exception e)
				{
					// Likely NotImplementedException if we messed handling a particular type
					UnturnedLog.exception(e, $"Caught exception parsing mode config for menu:");
				}
			}
			else
			{
				ConfigData configData = ConfigData.CreateDefault(true);
				string path = "/Worlds/Singleplayer_" + Characters.selected + "/Config.json";
				if (ReadWrite.fileExists(path, false))
				{
					try
					{
						ReadWrite.populateJSON(path, configData, usePath: true);
					}
					catch (Exception e)
					{
						UnturnedLog.error("Exception while parsing singleplayer config json for menu:");
						UnturnedLog.exception(e);
					}
				}

				try
				{
					PlayConfigUtils.GatherModifiedFields(defaultModeConfigData, configData.getModeConfig(PlaySettings.singleplayerMode), propertyOverrides);
					foreach (KeyValuePair<FieldInfo, object> pair in propertyOverrides)
					{
						CommandWindow.Log($"Config menu converted {PlayConfigUtils.GetFieldPath(pair.Key)} = \"{pair.Value}\"");
					}
				}
				catch (System.Exception e)
				{
					// Likely NotImplementedException if we messed handling a particular type
					UnturnedLog.exception(e, $"Caught exception gathering modified json fields for menu:");
				}
			}

			SyncPropertyWidgetValues();

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			IEditableDatDictionary dictionaryToWrite = MetadataPreservingDatWriter.CreateRoot();

			try
			{
				PlayConfigUtils.ApplyModeConfigOverrides(dictionaryToWrite, propertyOverrides);
			}
			catch (System.Exception e)
			{
				// Likely NotImplementedException if we messed handling a particular type
				UnturnedLog.exception(e, $"Caught exception applying modified fields for config menu:");
			}

			string v2FilePath = PlayConfigUtils.GetSingleplayerConfigPathV2(Characters.selected, PlaySettings.singleplayerMode);
			try
			{
				string dirname = Path.GetDirectoryName(v2FilePath);
				if (!Directory.Exists(dirname))
				{
					Directory.CreateDirectory(dirname);
				}

				const bool append = false;
				using (StreamWriter fileStream = new StreamWriter(v2FilePath, append, System.Text.Encoding.UTF8))
				{
					DatWriter datWriter = new DatWriter(fileStream);
					MetadataPreservingDatWriter metadataPreservingDatWriter = new MetadataPreservingDatWriter();
					metadataPreservingDatWriter.WriteRootDictionary(dictionaryToWrite, datWriter);
				}
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, $"Caught exception writing updated config file to: \"{v2FilePath}\"");
			}

			container.AnimateOutOfView(0, 1);
		}

		public static string sanitizeName(string fieldName)
		{
			if (localization.has(fieldName))
			{
				return localization.format(fieldName);
			}
			else
			{
				return fieldName.Replace('_', ' ');
			}
		}

		/// <summary>
		/// Creating all these elements is a bit slow, so we only do it once the menu is first opened.
		/// </summary>
		private static void CreatePropertyWidgets()
		{
			UnturnedCodeDocsHelper codeDocsHelper = new UnturnedCodeDocsHelper();
			propertyWidgets = new Dictionary<FieldInfo, SleekConfigProperty>();

			System.Text.StringBuilder tooltipSb = new System.Text.StringBuilder();

			float configOffset = 0;

			Type configType = typeof(ModeConfigData);
			FieldInfo[] categoryFields = configType.GetFields();
			foreach (FieldInfo categoryField in categoryFields)
			{
				ISleekBox box = Glazier.Get().CreateBox();
				box.PositionOffset_X = 100;
				box.PositionOffset_Y = configOffset;
				box.SizeOffset_Y = 30;
				box.SizeOffset_X = -100;
				box.SizeScale_X = 1;
				box.Text = sanitizeName(categoryField.Name);
				configBox.AddChild(box);

				float boxOffset = 40;
				configOffset += 40;

				Type categoryType = categoryField.FieldType;
				FieldInfo[] configFields = categoryType.GetFields();
				foreach (FieldInfo configField in configFields)
				{
					string docTooltip = codeDocsHelper.GetSummary(categoryField.FieldType.Name, configField.Name);
					if (!string.IsNullOrEmpty(docTooltip))
					{
						string[] summaryLines = docTooltip.SplitLinesIncludingEmpty();
						int lineCount = summaryLines.Length;
						if (string.IsNullOrWhiteSpace(summaryLines[lineCount - 1]))
						{
							lineCount--;
						}

						if (lineCount == 1)
						{
							docTooltip = summaryLines[0].Trim();
						}
						else
						{
							tooltipSb.Clear();
							for (int lineIndex = 0; lineIndex < lineCount; ++lineIndex)
							{
								if (lineIndex > 0)
								{
									tooltipSb.AppendLine();
								}
								tooltipSb.Append(summaryLines[lineIndex].Trim());
							}
							docTooltip = tooltipSb.ToString();
						}
					}

					SleekConfigProperty property = new SleekConfigProperty(configField, docTooltip);
					property.SizeScale_X = 1;
					property.PositionOffset_Y = boxOffset;
					property.OnValueChanged += OnPropertyOverrideChanged;
					box.AddChild(property);
					propertyWidgets.Add(configField, property);

					boxOffset += property.SizeOffset_Y + 10;
					configOffset += property.SizeOffset_Y + 10;
				}

				configOffset += 40;
			}

			configBox.ContentSizeOffset = new Vector2(0.0f, configOffset - 50);
		}

		private static void SyncPropertyWidgetValues()
		{
			Type configType = typeof(ModeConfigData);
			FieldInfo[] categoryFields = configType.GetFields();
			foreach (FieldInfo categoryField in categoryFields)
			{
				object category = categoryField.GetValue(defaultModeConfigData);

				Type categoryType = categoryField.FieldType;
				FieldInfo[] configFields = categoryType.GetFields();
				foreach (FieldInfo configField in configFields)
				{
					object defaultValue = configField.GetValue(category);
					bool hasOverride = propertyOverrides.TryGetValue(configField, out object overrideValue);

					SleekConfigProperty widget = propertyWidgets[configField];
					widget.defaultValue = configField.GetValue(category);
					widget.SetOverrideState(hasOverride, overrideValue);
				}
			}
		}

		private static void OnPropertyOverrideChanged(SleekConfigProperty widget, bool hasOverride, object overrideValue)
		{
			if (hasOverride)
			{
				propertyOverrides[widget.fieldInfo] = overrideValue;
				UnturnedLog.info($"Set {widget.fieldInfo.Name} override {overrideValue}");
			}
			else
			{
				propertyOverrides.Remove(widget.fieldInfo);
				UnturnedLog.info($"Remove {widget.fieldInfo.Name} override");
			}
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuPlaySingleplayerUI.open();
			close();
		}

		private static void onClickedDefaultButton(ISleekElement button)
		{
			propertyOverrides.Clear();
			SyncPropertyWidgetValues();
		}

		public MenuPlayConfigUI()
		{
			localization = Localization.read("/Menu/Play/MenuPlayConfig.dat");

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

			configBox = Glazier.Get().CreateScrollView();
			configBox.PositionOffset_X = -300;
			configBox.PositionOffset_Y = 100;
			configBox.PositionScale_X = 0.5f;
			configBox.SizeOffset_X = 530;
			configBox.SizeOffset_Y = -200;
			configBox.SizeScale_Y = 1;
			configBox.ScaleContentToWidth = true;
			container.AddChild(configBox);

			propertyWidgets = null;
			propertyOverrides = new Dictionary<FieldInfo, object>();

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

			defaultButton = Glazier.Get().CreateButton();
			defaultButton.PositionOffset_X = -200;
			defaultButton.PositionOffset_Y = -50;
			defaultButton.PositionScale_X = 1f;
			defaultButton.PositionScale_Y = 1f;
			defaultButton.SizeOffset_X = 200;
			defaultButton.SizeOffset_Y = 50;
			defaultButton.Text = localization.format("Default");
			defaultButton.TooltipText = localization.format("Default_Tooltip");
			defaultButton.OnClicked += onClickedDefaultButton;
			defaultButton.FontSize = ESleekFontSize.Medium;
			container.AddChild(defaultButton);
		}
	}
}
