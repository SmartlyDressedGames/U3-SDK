////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class GameMode
	{
		public virtual GameObject getPlayerGameObject(SteamPlayerID playerID)
		{
			if (Dedicator.IsDedicatedServer)
			{
				return Object.Instantiate(Resources.Load<GameObject>("Characters/Player_Dedicated"));
			}
			else
			{
				if (playerID.steamID == Provider.client)
				{
					string path =
#if WITH_NOREDIST
				"Characters_NoRedist/Player_Server";
#else
				"Characters/Player_Server";
#endif
					return Object.Instantiate(Resources.Load<GameObject>(path));
				}
				else
				{
					return Object.Instantiate(Resources.Load<GameObject>("Characters/Player_Client"));
				}
			}
		}

		public GameMode()
		{
			UnturnedLog.info(this);
		}
	}
}
