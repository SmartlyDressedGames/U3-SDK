////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandLoadout : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length < 1)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			byte skillsetID;
			if (!byte.TryParse(components[0], out skillsetID) || (skillsetID != 255 && skillsetID > 10))
			{
				CommandWindow.LogError(localization.format("InvalidSkillsetIDErrorText", components[0]));
				return;
			}

			ushort[] itemIDs = new ushort[components.Length - 1];

			for (int index = 1; index < components.Length; index++)
			{
				ushort itemID;
				if (!ushort.TryParse(components[index], out itemID))
				{
					CommandWindow.LogError(localization.format("InvalidItemIDErrorText", components[index]));
					return;
				}

				itemIDs[index - 1] = itemID;
			}

			if (skillsetID == 255)
			{
				PlayerInventory.loadout = itemIDs;
			}
			else
			{
				PlayerInventory.skillsets[skillsetID] = itemIDs;
			}

			CommandWindow.Log(localization.format("LoadoutText", skillsetID));
		}

		public CommandLoadout(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("LoadoutCommandText");
			_info = localization.format("LoadoutInfoText");
			_help = localization.format("LoadoutHelpText");
		}
	}
}