////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekDefaultServerListPresetButton : SleekWrapper
	{
		public SleekDefaultServerListPresetButton(ServerListFilters preset, Local localization, IconsBundle icons)
		{
			this.preset = preset;

			Texture2D icon;
			string text;
			string tooltip;
			if (preset.presetId == FilterSettings.defaultPresetInternet.presetId)
			{
				icon = icons.load<Texture2D>("List_Internet");
				text = localization.format("DefaultPreset_Internet_Label");
				tooltip = localization.format("List_Internet_Tooltip");
			}
			else if (preset.presetId == FilterSettings.defaultPresetLAN.presetId)
			{
				icon = icons.load<Texture2D>("List_LAN");
				text = localization.format("DefaultPreset_LAN_Label");
				tooltip = localization.format("List_LAN_Tooltip");
			}
			else if (preset.presetId == FilterSettings.defaultPresetHistory.presetId)
			{
				icon = icons.load<Texture2D>("List_History");
				text = localization.format("DefaultPreset_History_Label");
				tooltip = localization.format("List_History_Tooltip");
			}
			else if (preset.presetId == FilterSettings.defaultPresetFavorites.presetId)
			{
				icon = icons.load<Texture2D>("List_Favorites");
				text = localization.format("DefaultPreset_Favorites_Label");
				tooltip = localization.format("List_Favorites_Tooltip");
			}
			else if (preset.presetId == FilterSettings.defaultPresetFriends.presetId)
			{
				icon = icons.load<Texture2D>("List_Friends");
				text = localization.format("DefaultPreset_Friends_Label");
				tooltip = localization.format("List_Friends_Tooltip");
			}
			else
			{
				icon = null;
				text = $"unknown preset ({preset.presetId})";
				tooltip = text;
			}

			internalButton = new SleekButtonIcon(icon, 20);
			internalButton.SizeScale_X = 1.0f;
			internalButton.SizeScale_Y = 1.0f;
			internalButton.text = text;
			internalButton.tooltip = tooltip;
			internalButton.onClickedButton += OnClicked;
			internalButton.iconColor = ESleekTint.FOREGROUND;
			AddChild(internalButton);
		}

		private void OnClicked(ISleekElement button)
		{
			FilterSettings.activeFilters.CopyFrom(preset);
			if (preset.presetId == FilterSettings.defaultPresetInternet.presetId)
			{
				FilterSettings.activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("DefaultPreset_Internet_Label");
			}
			else if (preset.presetId == FilterSettings.defaultPresetLAN.presetId)
			{
				FilterSettings.activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("DefaultPreset_LAN_Label");
			}
			else if (preset.presetId == FilterSettings.defaultPresetHistory.presetId)
			{
				FilterSettings.activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("DefaultPreset_History_Label");
			}
			else if (preset.presetId == FilterSettings.defaultPresetFavorites.presetId)
			{
				FilterSettings.activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("DefaultPreset_Favorites_Label");
			}
			else if (preset.presetId == FilterSettings.defaultPresetFriends.presetId)
			{
				FilterSettings.activeFilters.presetName = MenuPlayUI.serverListUI.localization.format("DefaultPreset_Friends_Label");
			}
			else
			{
				FilterSettings.activeFilters.presetName = "unknown default preset";
			}
			FilterSettings.InvokeActiveFiltersReplaced();
		}

		private SleekButtonIcon internalButton;
		private ServerListFilters preset;
	}
}
