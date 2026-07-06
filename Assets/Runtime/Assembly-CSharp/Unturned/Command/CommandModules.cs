////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Modules;
using Steamworks;

namespace SDG.Unturned
{
	public class CommandModules : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (ModuleHook.modules.Count == 0)
			{
				CommandWindow.LogError(localization.format("NoModulesErrorText"));
				return;
			}

			CommandWindow.Log(localization.format("ModulesText"));
			CommandWindow.Log(localization.format("SeparatorText"));
			for (int index = 0; index < ModuleHook.modules.Count; index++)
			{
				Module module = ModuleHook.modules[index];
				if (module == null)
				{
					continue;
				}

				ModuleConfig config = module.config;
				if (config == null)
				{
					continue;
				}

				Local localization2 = Localization.tryRead(config.DirectoryPath, false);

				CommandWindow.Log(localization2.format("Name"));
				CommandWindow.Log(localization.format("Version", config.Version));
				CommandWindow.Log(localization2.format("Description"));

				CommandWindow.Log(localization.format("SeparatorText"));
			}
		}

		public CommandModules(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ModulesCommandText");
			_info = localization.format("ModulesInfoText");
			_help = localization.format("ModulesHelpText");
		}
	}
}