////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Achievement : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (Dedicator.IsDedicatedServer || !other.transform.CompareTag("Player") || other.transform != Player.LocalPlayer.transform)
			{
				return;
			}

			bool data;
			if (Provider.provider.achievementsService.getAchievement(transform.name, out data) && !data)
			{
				Provider.provider.achievementsService.setAchievement(transform.name);
			}
		}
	}
}