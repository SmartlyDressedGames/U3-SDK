////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableOptic : Useable
	{
		private bool isZoomed;

		public override bool startPrimary()
		{
			if (channel.IsLocalPlayer && isZoomed)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				if (Physics.Raycast(ray, out RaycastHit hit, 2048.0f, RayMasks.DAMAGE_CLIENT))
				{
					player.quests.sendSetMarker(true, hit.point);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public override bool startSecondary()
		{
			if (channel.IsLocalPlayer && !isZoomed && player.look.perspective == EPlayerPerspective.FIRST)
			{
				isZoomed = true;

				startZoom();
				return true;
			}

			return false;
		}

		public override void stopSecondary()
		{
			if (channel.IsLocalPlayer && isZoomed)
			{
				isZoomed = false;

				stopZoom();
			}
		}

		private void startZoom()
		{
			player.animator.viewmodelCameraLocalPositionOffset = Vector3.up;
			player.animator.viewmodelSwayMultiplier = 0;

			player.look.enableZoom(((ItemOpticAsset) player.equipment.asset).zoom, false);
			player.look.shouldUseZoomFactorForSensitivity = true;

			PlayerUI.updateBinoculars(true);
			//isEnabled = PlayerUI.window.isEnabled;
			//PlayerUI.window.isEnabled = false;
		}

		private void stopZoom()
		{
			player.animator.viewmodelCameraLocalPositionOffset = Vector3.zero;
			player.animator.viewmodelSwayMultiplier = 1f;

			player.look.disableZoom();
			player.look.shouldUseZoomFactorForSensitivity = false;

			PlayerUI.updateBinoculars(false);
			//PlayerUI.window.isEnabled = isEnabled;
		}

		private void onPerspectiveUpdated(EPlayerPerspective newPerspective)
		{
			if (isZoomed && newPerspective == EPlayerPerspective.THIRD)
			{
				stopZoom();
			}
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			if (channel.IsLocalPlayer)
			{
				player.look.onPerspectiveUpdated += onPerspectiveUpdated;
			}
		}

		public override void dequip()
		{
			if (channel.IsLocalPlayer)
			{
				if (isZoomed)
				{
					stopZoom();
				}

				player.look.onPerspectiveUpdated -= onPerspectiveUpdated;
			}
		}
	}
}
