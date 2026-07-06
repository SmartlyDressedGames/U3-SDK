////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandArmor : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length != 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			float barricadeArmor;
			if (!float.TryParse(components[0], out barricadeArmor))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			float structureArmor;
			if (!float.TryParse(components[1], out structureArmor))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			//BarricadeManager.barricadeArmor = barricadeArmor;
			//StructureManager.structureArmor = structureArmor;

			CommandWindow.Log(localization.format("ArmorText"));
		}

		public CommandArmor(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ArmorCommandText");
			_info = localization.format("ArmorInfoText");
			_help = localization.format("ArmorHelpText");
		}
	}
}