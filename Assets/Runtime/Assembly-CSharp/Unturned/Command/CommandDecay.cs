////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;

namespace SDG.Unturned
{
	public class CommandDecay : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length != 2)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			uint barricadeDecay;
			if (!uint.TryParse(components[0], out barricadeDecay))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			uint structureDecay;
			if (!uint.TryParse(components[1], out structureDecay))
			{
				CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
				return;
			}

			//BarricadeManager.barricadeDecay = barricadeDecay;
			//StructureManager.structureDecay = structureDecay;

			CommandWindow.Log(localization.format("DecayText"));
		}

		public CommandDecay(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("DecayCommandText");
			_info = localization.format("DecayInfoText");
			_help = localization.format("DecayHelpText");
		}
	}
}