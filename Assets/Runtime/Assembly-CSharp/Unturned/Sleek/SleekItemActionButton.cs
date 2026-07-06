////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

namespace SDG.Unturned
{
	internal class SleekItemActionButton : SleekWrapper
	{
		public SleekItemActionButton(Action action)
		{
			this.action = action;

			Local localization = PlayerDashboardInventoryUI.localization;

			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;

			string actionTooltip;
			if (!string.IsNullOrEmpty(action.key))
			{
				button.Text = localization.format(action.key + "_Button");
				actionTooltip = localization.format(action.key + "_Button_Tooltip");
			}
			else
			{
				button.Text = action.text;
				actionTooltip = action.tooltip;
			}

			if (action.type == EActionType.BLUEPRINT && !string.IsNullOrEmpty(actionTooltip))
			{
				actionTooltip += "\n\n";

				if (action.IsAnyBlueprintLink)
				{
					actionTooltip += localization.format("ActionBlueprint_SkipCraftingTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.SkipActionCraftingMenu));
					actionTooltip += '\n';
				}

				actionTooltip += localization.format("ActionBlueprint_CraftAllTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
			}

			button.TooltipText = actionTooltip;
			button.OnClicked += OnClickedButton;
			AddChild(button);

			IBlueprintOwner asset = action.FindBlueprintOwnerAsset() as IBlueprintOwner;
			if (asset == null)
			{
				UnturnedLog.warn($"Unable to find item action blueprint owner {action}");
				button.IsClickable = false;
				return;
			}

			tempBlueprints.Clear();
			foreach (ActionBlueprint actionBlueprint in action.blueprints)
			{
				Blueprint foundBlueprint = actionBlueprint.FindBlueprint(asset);
				if (foundBlueprint == null)
				{
					UnturnedLog.warn($"Unable to find item action's blueprint {actionBlueprint}");
					continue;
				}

				tempBlueprints.Add(foundBlueprint);
			}

			if (tempBlueprints.Count > 0)
			{
				relatedBlueprints = tempBlueprints.ToArray();

				PlayerDashboardCraftingUI.filteredBlueprintsOverride = relatedBlueprints;
				bool allCraftable = PlayerDashboardCraftingUI.UpdateFilteredBlueprintsAndGetAreAllCraftable();
				PlayerDashboardCraftingUI.filteredBlueprintsOverride = null;
				if (!allCraftable)
				{
					button.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
				}
			}
			else
			{
				button.IsClickable = false;
				UnturnedLog.warn($"Item action has no blueprints {action}");
			}
		}

		private void OnClickedButton(ISleekElement element)
		{
			if (relatedBlueprints == null)
			{
				return;
			}

			bool isLink = action.IsAnyBlueprintLink;
			isLink &= !InputEx.GetKey(ControlsSettings.SkipActionCraftingMenu);

			PlayerDashboardCraftingUI.filteredBlueprintsOverride = relatedBlueprints;

			if (!isLink) // not linking, so let's check if we can craft it
			{
				if (Player.LocalPlayer.equipment.isBusy)
				{
					isLink = true;
				}
				else
				{
					bool allCraftable = PlayerDashboardCraftingUI.UpdateFilteredBlueprintsAndGetAreAllCraftable();
					isLink = !allCraftable;
				}
			}

			if (isLink) // either is linked or we can't craft it
			{
				PlayerDashboardInventoryUI.close();
				PlayerDashboardCraftingUI.open();
			}
			else // we CAN craft it, so let's do this!
			{
				bool asManyAsPossible = InputEx.GetKey(ControlsSettings.other);
				foreach (Blueprint blueprint in relatedBlueprints)
				{
					Player.LocalPlayer.crafting.SendRequestToCraft(blueprint, asManyAsPossible);
				}

				PlayerDashboardCraftingUI.filteredBlueprintsOverride = null;
				PlayerDashboardInventoryUI.closeSelection();
			}
		}

		private Action action;
		private ISleekButton button;
		private Blueprint[] relatedBlueprints;
		private static List<Blueprint> tempBlueprints = new List<Blueprint>();
	}
}
