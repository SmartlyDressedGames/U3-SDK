////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;
using System.IO;

namespace SDG.Unturned
{
	public class CommandReload : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			Guid parsedGUID;
			if (Guid.TryParse(parameter, out parsedGUID))
			{
				Asset foundAsset = Assets.find(parsedGUID);
				if (foundAsset == null)
					return;

				string directory = Path.GetDirectoryName(foundAsset.absoluteOriginFilePath);
				Assets.reload(directory);
			}
			else if (Directory.Exists(parameter))
			{
				string sanitizedPath = Path.GetFullPath(parameter);
				Assets.reload(sanitizedPath);
			}
		}

		public CommandReload(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("ReloadCommandText");
			_info = localization.format("ReloadInfoText");
			_help = localization.format("ReloadHelpText");
		}
	}
}
