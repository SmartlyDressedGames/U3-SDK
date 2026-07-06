////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekCustomServerListPresetButton : SleekWrapper
	{
		public SleekCustomServerListPresetButton(ServerListFilters preset)
		{
			this.preset = preset;

			internalButton = Glazier.Get().CreateButton();
			internalButton.SizeScale_X = 1.0f;
			internalButton.SizeScale_Y = 1.0f;
			internalButton.Text = preset.presetName;
			internalButton.OnClicked += OnClicked;
			AddChild(internalButton);
		}

		private void OnClicked(ISleekElement button)
		{
			FilterSettings.activeFilters.CopyFrom(preset);
			FilterSettings.InvokeActiveFiltersReplaced();
		}

		private ISleekButton internalButton;
		private ServerListFilters preset;
	}
}
