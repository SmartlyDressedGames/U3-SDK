////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace SDG.Unturned
{
	public class VolumeTeleporter : MonoBehaviour
	{
		public string achievement;
		public Transform target;

		public ushort teleportEffect;
		public Transform effectHook;

		private Player playerTeleported;

		private IEnumerator teleport()
		{
			yield return new WaitForSeconds(3.0f);

			if (target != null && playerTeleported != null && playerTeleported.life.IsAlive)
			{
				playerTeleported.teleportToLocation(target.position, target.rotation.eulerAngles.y);

				if (playerTeleported.equipment.HasValidUseable)
				{
					playerTeleported.equipment.dequip();
				}
				playerTeleported.equipment.canEquip = true;
			}

			playerTeleported = null;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!Dedicator.IsDedicatedServer && !string.IsNullOrEmpty(achievement)) // clientside grant achievement
			{
				if (other.transform.CompareTag("Player") && other.transform == Player.LocalPlayer.transform)
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement(achievement, out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement(achievement);
					}
				}
			}

			if (Provider.isServer && other.transform.CompareTag("Player"))
			{
				if (playerTeleported == null)
				{
#pragma warning disable
					// Did not both converting these because the hardcoded per-level scripts need upgrading anyway.
					EffectManager.sendEffect(teleportEffect, 16, effectHook.position);
#pragma warning restore

					playerTeleported = DamageTool.getPlayer(other.transform);
					StartCoroutine("teleport");
				}
			}
		}
	}
}
