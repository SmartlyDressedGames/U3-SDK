////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned
{
	public class CommandEffectUI : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
				return;

			if (executorID == CSteamID.Nil)
			{
				// Executed from the server console.
				if (Provider.clients.Count > 0)
				{
					executorID = Provider.clients[0].playerID.steamID;
				}
			}

			ITransportConnection tc = Provider.findTransportConnection(executorID);
			if (tc == null)
				return;

			string[] splitParams = parameter.Split('/');
			string effectParam = splitParams.Length > 0 ? splitParams[0] : parameter;

			if (effectParam.Equals("clearall", System.StringComparison.InvariantCultureIgnoreCase))
			{
				UnturnedLog.info("Clearing all effects");
				EffectManager.askEffectClearAll();
				return;
			}

			EffectAsset asset;
			if (System.Guid.TryParse(effectParam, out System.Guid parsedGuid))
			{
				asset = Assets.find(parsedGuid) as EffectAsset;
			}
			else if (ushort.TryParse(effectParam, out ushort parsedLegacyId))
			{
				asset = Assets.find(EAssetType.EFFECT, parsedLegacyId) as EffectAsset;
			}
			else
			{
				return;
			}

			if (splitParams.Length < 2)
			{
				EffectManager.SendUIEffect(asset, 1, tc, true);
			}
			else if (splitParams.Length == 2)
			{
				if (splitParams[1].Equals("clearbyid", System.StringComparison.InvariantCulture))
				{
					UnturnedLog.info("Clearing UI effects with GUID {0}", asset.GUID);
					EffectManager.ClearEffectByGuid(asset.GUID, tc);
				}
				else
				{
					EffectManager.SendUIEffect(asset, 1, tc, true, splitParams[1]);
				}
			}
			else if (splitParams.Length == 3)
			{
				EffectManager.SendUIEffect(asset, 1, tc, true, splitParams[1], splitParams[2]);
			}
			else if (splitParams.Length == 4)
			{
				EffectManager.SendUIEffect(asset, 1, tc, true, splitParams[1], splitParams[2], splitParams[3]);
			}
			else if (splitParams.Length == 5)
			{
				EffectManager.SendUIEffect(asset, 1, tc, true, splitParams[1], splitParams[2], splitParams[3], splitParams[4]);
			}
		}

		public CommandEffectUI(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("EffectCommandText");
			_info = localization.format("EffectInfoText");
			_help = localization.format("EffectHelpText");
		}
	}
}
