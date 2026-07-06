////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerTool
	{
		private static string getRepKey(int rep)
		{
			string key = "";

			if (rep <= -200)
			{
				key = "Villain";
			}
			else if (rep <= -100)
			{
				key = "Bandit";
			}
			else if (rep <= -33)
			{
				key = "Gangster";
			}
			else if (rep <= -8)
			{
				key = "Outlaw";
			}
			else if (rep < 0)
			{
				key = "Thug";
			}
			else if (rep >= 200)
			{
				key = "Paragon";
			}
			else if (rep >= 100)
			{
				key = "Sheriff";
			}
			else if (rep >= 33)
			{
				key = "Deputy";
			}
			else if (rep >= 8)
			{
				key = "Constable";
			}
			else if (rep > 0)
			{
				key = "Vigilante";
			}
			else if (rep == 0)
			{
				key = "Neutral";
			}

			return key;
		}

		public static Texture2D getRepTexture(int rep)
		{
			IconsBundle bundle = Bundles.getIconsBundle("UI/Player/Icons/Reputation");
			return bundle.load<Texture2D>(getRepKey(rep));
		}

		public static string getRepTitle(int rep)
		{
			return PlayerDashboardInformationUI.localization.format("Rep", PlayerDashboardInformationUI.localization.format("Rep_" + getRepKey(rep)), rep);
		}

		public static Color getRepColor(int rep)
		{
			if (rep == 0)
			{
				return Color.white;
			}
			else if (rep < 0)
			{
				float blend = Mathf.Min(Mathf.Abs(rep), 200) / 200.0f;

				if (blend < 0.5f)
				{
					return Color.Lerp(Color.white, Palette.COLOR_Y, blend * 2.0f);
				}
				else
				{
					return Color.Lerp(Palette.COLOR_Y, Palette.COLOR_R, (blend - 0.5f) * 2.0f);
				}
			}
			else if (rep > 0)
			{
				float blend = Mathf.Min(Mathf.Abs(rep), 200) / 200.0f;

				return Color.Lerp(Color.white, Palette.COLOR_G, blend);
			}

			return Color.white;
		}

		public static void getPlayersInRadius(Vector3 center, float sqrRadius, List<Player> result)
		{
			for (int index = 0; index < Provider.clients.Count; index++)
			{
				Player player = Provider.clients[index].player;

				if (player == null)
				{
					continue;
				}

				Vector3 offset = player.transform.position - center;

				if (offset.sqrMagnitude < sqrRadius)
				{
					result.Add(player);
				}
			}
		}

		public static Player GetNearestPlayerInRadius(Vector3 center, float sqrRadius)
		{
			Player nearestPlayer = null;
			float sqrNearestRadius = sqrRadius + 1.0f;

			foreach (SteamPlayer client in Provider.clients)
			{
				if (client == null)
					continue;

				Player player = client.player;
				if (player == null)
					continue;

				float sqrDistance = (player.transform.position - center).sqrMagnitude;
				if (sqrDistance < sqrNearestRadius)
				{
					nearestPlayer = player;
					sqrNearestRadius = sqrDistance;
				}
			}

			return nearestPlayer;
		}

		public static SteamPlayer[] getSteamPlayers()
		{
			return Provider.clients.ToArray();
		}

		public static SteamPlayer getSteamPlayer(string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;

			for (int index = 0; index < Provider.clients.Count; index++)
			{
				if (NameTool.checkNames(name, Provider.clients[index].playerID.playerName) || NameTool.checkNames(name, Provider.clients[index].playerID.characterName))
				{
					return Provider.clients[index];
				}
			}

			return null;
		}

		public static SteamPlayer getSteamPlayer(ulong steamID)
		{
			for (int index = 0; index < Provider.clients.Count; index++)
			{
				if (Provider.clients[index].playerID.steamID.m_SteamID == steamID)
				{
					return Provider.clients[index];
				}
			}

			return null;
		}

		public static SteamPlayer getSteamPlayer(CSteamID steamID)
		{
			for (int index = 0; index < Provider.clients.Count; index++)
			{
				if (Provider.clients[index].playerID.steamID == steamID)
				{
					return Provider.clients[index];
				}
			}

			return null;
		}

		/// <summary>
		/// Find client with given RPC channel ID.
		/// </summary>
		public static SteamPlayer findSteamPlayerByChannel(int channel)
		{
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client != null && client.channel == channel)
					return client;
			}

			return null;
		}

		public static Transform getPlayerModel(CSteamID steamID)
		{
			SteamPlayer player = getSteamPlayer(steamID);

			if (player != null && player.model != null)
			{
				return player.model;
			}

			return null;
		}

		public static Player getPlayer(CSteamID steamID)
		{
			SteamPlayer player = getSteamPlayer(steamID);

			if (player != null && player.player != null)
			{
				return player.player;
			}

			return null;
		}

		public static Transform getPlayerModel(string name)
		{
			SteamPlayer player = getSteamPlayer(name);

			if (player != null && player.model != null)
			{
				return player.model;
			}

			return null;
		}

		public static Player getPlayer(string name)
		{
			SteamPlayer player = getSteamPlayer(name);

			if (player != null && player.player != null)
			{
				return player.player;
			}

			return null;
		}

		public static bool tryGetSteamPlayer(string input, out SteamPlayer player)
		{
			player = null;

			if (string.IsNullOrEmpty(input))
				return false;

			ulong steamID;
			if (ulong.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out steamID))
			{
				player = getSteamPlayer(steamID);

				return player != null;
			}

			player = getSteamPlayer(input);

			return player != null;
		}

		public static bool tryGetSteamID(string input, out CSteamID steamID)
		{
			steamID = CSteamID.Nil;

			if (string.IsNullOrEmpty(input))
				return false;

			ulong id;
			if (ulong.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out id))
			{
				steamID = new CSteamID(id);

				return true;
			}

			SteamPlayer player = getSteamPlayer(input);
			if (player != null)
			{
				steamID = player.playerID.steamID;

				return true;
			}

			return false;
		}

		public static IEnumerable<Player> EnumeratePlayers()
		{
			foreach (SteamPlayer client in Provider.clients)
			{
				if (client.player != null)
				{
					yield return client.player;
				}
			}
		}
	}
}
