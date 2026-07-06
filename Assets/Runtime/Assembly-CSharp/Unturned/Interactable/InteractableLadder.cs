////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define ENABLE_LADDER_INTERACT_GIZMOS
using UnityEngine;

namespace SDG.Unturned
{
	public class InteractableLadder : Interactable
	{
		public override bool checkUseable()
		{
			return true;
		}

		public override void use()
		{
			// 2022-04-25: this check is unfortunately not redundant because for some reason use() is called even
			// if checkHint() returns false
			if (CanClimb(Player.LocalPlayer))
			{
				Vector3 direction = (PlayerInteract.hit.point - Player.LocalPlayer.look.aim.position).normalized;
				PlayerStance.SendClimbRequest.Invoke(Player.LocalPlayer.stance.GetNetId(), NetTransport.ENetReliability.Reliable, direction);
			}
		}

		public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
		{
			text = "";
			color = Color.white;

			// 2022-04-19: ideally we should rewrite interactable to pass in who is interacting with this. :(
			if (CanClimb(Player.LocalPlayer))
			{
				message = EPlayerMessage.CLIMB;
				return true;
			}
			else
			{
				message = EPlayerMessage.NONE;
				return false;
			}
		}

		private bool CanClimb(Player player)
		{
			if (player == null || player.stance.stance == EPlayerStance.CLIMB)
				return false;

			if (!player.stance.canCurrentStanceTransitionToClimbing)
				return false;

			if (!player.stance.isAllowedToStartClimbing)
				return false;

			Vector3 direction = (PlayerInteract.hit.point - player.look.aim.position).normalized;

			// Sphere cast prevents climbing through a tiny gap.
			Ray ray = new Ray(player.look.aim.position, direction);
			RaycastHit climbHit;
			Physics.SphereCast(ray, PlayerStance.RADIUS, out climbHit, PlayerStance.LADDER_INTERACT_RANGE, RayMasks.LADDER_INTERACT);

#if ENABLE_LADDER_INTERACT_GIZMOS
			GizmosUtil.Get().Spherecast(ray, PlayerStance.RADIUS, PlayerStance.LADDER_INTERACT_RANGE, climbHit, Color.red, Color.green);
#endif // ENABLE_LADDER_INTERACT_GIZMOS

			if (climbHit.collider == null || !climbHit.collider.CompareTag("Ladder"))
				return false;

			RaycastHit rayHit;
			Physics.Raycast(new Ray(player.look.aim.position, direction), out rayHit, PlayerStance.LADDER_INTERACT_RANGE, RayMasks.LADDER_INTERACT);

#if ENABLE_LADDER_INTERACT_GIZMOS
			GizmosUtil.Get().Raycast(ray, PlayerStance.LADDER_INTERACT_RANGE, rayHit, Color.red, Color.green);
#endif // ENABLE_LADDER_INTERACT_GIZMOS

			if (rayHit.collider == null || !rayHit.collider.CompareTag("Ladder"))
				return false;

			// Ladder movement code does this check as well to see if we hit the front/back of the ladder.
			float forwardAlignment = Vector3.Dot(rayHit.normal, rayHit.collider.transform.up);
			if (Mathf.Abs(forwardAlignment) <= 0.9f)
				return false;

			// Prevent climbing angled ladders. Only "mostly" up/down ladders.
			float worldUpAlignment = Vector3.Dot(Vector3.up, rayHit.collider.transform.up);
			if (Mathf.Abs(worldUpAlignment) > 0.1f)
				return false;

			// Teleport adds vertical offset, but ladder checks cast from 0.5m above feet.
			// Ladder forward ray is 0.75m, so we move slightly less than that away from the ladder.
			Vector3 climbPoint = new Vector3(rayHit.collider.transform.position.x, rayHit.point.y - Player.TELEPORT_VERTICAL_OFFSET - 0.5f - 0.1f, rayHit.collider.transform.position.z) + (rayHit.normal * PlayerStance.LADDER_INTERACT_TELEPORT_OFFSET);
			float testHeight = PlayerMovement.HEIGHT_STAND + 0.1f + Player.TELEPORT_VERTICAL_OFFSET;

			// Test first hit has line-of-sight to center of climbing capsule, otherwise
			// player may have angled ladder to place capsule on the other side of a thin wall.
			RaycastHit losHit;
			Vector3 losCenter = climbPoint + new Vector3(0.0f, testHeight * 0.5f, 0.0f);
			bool hasLos = !Physics.Linecast(rayHit.point, losCenter, out losHit, RayMasks.BLOCK_STANCE, QueryTriggerInteraction.Collide);

#if ENABLE_LADDER_INTERACT_GIZMOS
			GizmosUtil.Get().Linecast(rayHit.point, losCenter, losHit, Color.green, Color.red);
#endif // ENABLE_LADDER_INTERACT_GIZMOS

			if (!hasLos)
				return false;

			bool hasClearance = PlayerStance.hasHeightClearanceAtPosition(climbPoint, testHeight);

#if ENABLE_LADDER_INTERACT_GIZMOS
			PlayerStance.drawCapsule(climbPoint, testHeight, hasClearance ? Color.green : Color.red);
#endif // ENABLE_LADDER_INTERACT_GIZMOS

			return hasClearance;
		}
	}
}
