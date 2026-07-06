////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class LookAtLocalPlayer : MonoBehaviour
	{
#if GAME && !DEDICATED_SERVER
		private void LateUpdate()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (Player.LocalPlayer != null)
			{
				transform.LookAt(Player.LocalPlayer.look.aim);
			}
		}
#endif // GAME && !DEDICATED_SERVER
	}
}
