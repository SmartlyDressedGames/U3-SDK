////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class CommandGive : Command
	{
		protected override void execute(CSteamID executorID, string parameter)
		{
			if (!Provider.isServer)
			{
				CommandWindow.LogError(localization.format("NotRunningErrorText"));
				return;
			}

			if (!Provider.hasCheats)
			{
				CommandWindow.LogError(localization.format("CheatsErrorText"));
				return;
			}

			string[] components = Parser.getComponentsFromSerial(parameter, '/');

			if (components.Length < 1 || components.Length > 3)
			{
				CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
				return;
			}

			SteamPlayer player;
			bool isMe = false;
			if (!PlayerTool.tryGetSteamPlayer(components[0], out player))
			{
				player = PlayerTool.getSteamPlayer(executorID);

				if (player == null)
				{
					CommandWindow.LogError(localization.format("NoPlayerErrorText", components[0]));
					return;
				}
				else
				{
					isMe = true;
				}
			}

			uint amount = 1;
			if (isMe)
			{
				if (components.Length > 1)
				{
					if (!uint.TryParse(components[1], out amount))
					{
						CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[1]));
						return;
					}
				}
			}
			else
			{
				if (components.Length > 2)
				{
					if (!uint.TryParse(components[2], out amount))
					{
						CommandWindow.LogError(localization.format("InvalidNumberErrorText", components[2]));
						return;
					}
				}
			}

			string itemString = components[isMe ? 0 : 1];
			if (System.Guid.TryParse(itemString, out System.Guid parsedGuid))
			{
				Asset foundAsset = Assets.find(parsedGuid);
				GiveAsset(player, foundAsset, amount);
			}
			else if (ushort.TryParse(itemString, out ushort legacyId))
			{
				giveItem(player, legacyId, (byte) amount);
			}
			else
			{
				Asset foundAsset = FindByString(itemString);
				GiveAsset(player, foundAsset, amount);
			}
		}

		private void GiveAsset(SteamPlayer player, Asset asset, uint amount)
		{
			if (asset is ItemAsset)
			{
				giveItem(player, asset.id, (byte) amount);
			}
			else if (asset is ItemCurrencyAsset currency)
			{
				currency.grantValue(player.player, amount);
			}
		}

		private void giveItem(SteamPlayer player, ushort itemID, byte amount)
		{
			if (!ItemTool.tryForceGiveItem(player.player, itemID, amount))
			{
				CommandWindow.LogError(localization.format("NoItemIDErrorText", itemID));
				return;
			}

			CommandWindow.Log(localization.format("GiveText", player.playerID.playerName, itemID, amount));
		}

		private Asset FindByString(string input)
		{
			input = input.Trim();
			if (string.IsNullOrEmpty(input))
				return null;

			List<ItemAsset> allItemAssets = new List<ItemAsset>();
			Assets.find(allItemAssets);

			// File name matches take priority because they tend to be more specific than the display name.
			// For example, "Ambulance_German" is just called "Ambulance" in-game.

			// Find exact matches by file name.
			foreach (ItemAsset testAsset in allItemAssets)
			{
				if (string.Equals(input, testAsset.name, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			// Find exact matches by display name.
			foreach (ItemAsset testAsset in allItemAssets)
			{
				if (string.Equals(input, testAsset.itemName, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			// Find partial matches by file name.
			foreach (ItemAsset testAsset in allItemAssets)
			{
				if (testAsset.name.Contains(input, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			// Find partial matches by display name.
			foreach (ItemAsset testAsset in allItemAssets)
			{
				if (testAsset.itemName.Contains(input, System.StringComparison.InvariantCultureIgnoreCase))
				{
					return testAsset;
				}
			}

			return null;
		}

		public CommandGive(Local newLocalization)
		{
			localization = newLocalization;
			_command = localization.format("GiveCommandText");
			_info = localization.format("GiveInfoText");
			_help = localization.format("GiveHelpText");
		}
	}
}
