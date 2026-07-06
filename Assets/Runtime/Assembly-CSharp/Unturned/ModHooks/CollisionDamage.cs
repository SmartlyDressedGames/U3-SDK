////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if GAME
using Steamworks;
#endif
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Can be added to any GameObject with a Trigger collider.
	/// Ensure that Layer will detect player overlaps. Trap is a good candidate.
	/// </summary>
	[AddComponentMenu("Unturned/Collision Damage")]
	public class CollisionDamage : MonoBehaviour
	{
		public bool CanDamagePlayersInPvEMode = false;
		public EDeathCause DeathCause = EDeathCause.BURNING;
		public float PlayerDamage = 1.0f;
		public bool ApplyGlobalArmorMultiplier = true;

		public enum EDirectionMode
		{
			Forward,
			Radial,
		}
		public EDirectionMode DirectionMode;

#if GAME
		private void OnTriggerEnter(Collider other)
		{
			if (!Provider.isServer)
				return;

			GameObject otherGameObject = other?.gameObject;
			if (otherGameObject == null)
				return;

			if (otherGameObject.CompareTag("Player"))
			{
				if (!Provider.isPvP && !CanDamagePlayersInPvEMode)
				{
					return;
				}

				Player player = DamageTool.getPlayer(other.transform);
				if (player != null)
				{
					CSteamID killer = CSteamID.Nil;
					if (DamageTool.TryFindOwnership(transform, out ulong ownerUser, out ulong ownerGroup))
					{
						killer = new CSteamID(ownerUser);
					}

					Vector3 direction;
					switch (DirectionMode)
					{
						default:
						case EDirectionMode.Forward:
						{
							direction = transform.forward;
							break;
						}

						case EDirectionMode.Radial:
						{
							direction = (player.GetCapsuleCenter() - transform.position).normalized;
							break;
						}
					}

					DamagePlayerParameters parameters = new DamagePlayerParameters(player);
					parameters.cause = DeathCause;
					parameters.limb = ELimb.SPINE;
					parameters.killer = killer;
					parameters.direction = direction;
					parameters.damage = PlayerDamage;
					parameters.applyGlobalArmorMultiplier = ApplyGlobalArmorMultiplier;
					DamageTool.damagePlayer(parameters, out EPlayerKill kill);

					DamageTool.ServerSpawnLegacyImpact(player.GetCapsuleCenter(),
						-direction,
						"Flesh",
						null,
						Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
				}
			}
		}
#endif // GAME
	}
}
