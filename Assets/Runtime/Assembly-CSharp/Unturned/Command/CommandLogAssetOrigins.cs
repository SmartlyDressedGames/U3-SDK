////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Steamworks;

namespace SDG.Unturned
{
	public class CommandLogAssetOrigins : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (!Provider.hasCheats)
				return;

			foreach (AssetOrigin origin in Assets.assetOrigins)
			{
				UnturnedLog.info(origin.name + " " + origin.workshopFileId);
			}
		}

		public CommandLogAssetOrigins(Local newLocalization)
		{
			localization = newLocalization;
			_command = "LogAssetOrigins";
			_info = string.Empty;
			_help = string.Empty;
		}
	}
}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
