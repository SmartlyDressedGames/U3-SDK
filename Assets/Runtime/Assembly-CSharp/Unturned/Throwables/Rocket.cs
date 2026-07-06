////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class Rocket : MonoBehaviour, IOwnershipInfo
	{
		public Steamworks.CSteamID killer;
		public float range;
		public float playerDamage;
		public float zombieDamage;
		public float animalDamage;
		public float barricadeDamage;
		public float structureDamage;
		public float vehicleDamage;
		public float resourceDamage;
		public float objectDamage;
		public System.Guid explosionEffectGuid;
		/// <summary>
		/// Kept because lots of modders have been using this script in Unity,
		/// so removing legacy effect id would break their content.
		/// </summary>
		public ushort explosion;
		public bool penetrateBuildables;
		public Transform ignoreTransform;
		public ERagdollEffect ragdollEffect = ERagdollEffect.None;
		public float explosionLaunchSpeed;

		private bool isExploded;
		private Vector3 lastPos;
		private Vector3 secondLastPos;

		#region IOwnershipInfo
		public bool TryGetOwnership(out ulong ownerUser, out ulong ownerGroup)
		{
			if (killer != Steamworks.CSteamID.Nil)
			{
				ownerUser = killer.m_SteamID;
				ownerGroup = 0;
				return true;
			}
			else
			{
				ownerUser = 0;
				ownerGroup = 0;
				return false;
			}
		}
		#endregion IOwnershipInfo

		private void OnTriggerEnter(Collider other)
		{
			if (isExploded)
			{
				return;
			}

			if (other.isTrigger)
			{
				return;
			}

			if (ignoreTransform != null && (other.transform == ignoreTransform || other.transform.IsChildOf(ignoreTransform)))
			{
				return;
			}

			isExploded = true;

			if (Provider.isServer)
			{
				CSteamID explosionKiller = killer;
				if (explosionKiller == CSteamID.Nil)
				{
					// Nelson 2025-10-06: ensures instigator tracking works for nested rocket/grenade scripts.
					// For example, some mods have rockets with nested rocket components which should find the
					// instigator from the outer rocket.
					if (DamageTool.TryFindOwnership(transform.parent, out ulong ownerUser, out ulong ownerGroup))
					{
						explosionKiller = new CSteamID(ownerUser);
					}
				}

				List<EPlayerKill> kills;
				ExplosionParameters explosionParameters = new ExplosionParameters(secondLastPos, range, EDeathCause.MISSILE, explosionKiller);
				explosionParameters.playerDamage = playerDamage;
				explosionParameters.zombieDamage = zombieDamage;
				explosionParameters.animalDamage = animalDamage;
				explosionParameters.barricadeDamage = barricadeDamage;
				explosionParameters.structureDamage = structureDamage;
				explosionParameters.vehicleDamage = vehicleDamage;
				explosionParameters.resourceDamage = resourceDamage;
				explosionParameters.objectDamage = objectDamage;
				explosionParameters.damageOrigin = EDamageOrigin.Rocket_Explosion;
				explosionParameters.penetrateBuildables = penetrateBuildables;
				explosionParameters.ragdollEffect = ragdollEffect;
				explosionParameters.launchSpeed = explosionLaunchSpeed;
				DamageTool.explode(explosionParameters, out kills);

				TriggerEffectParameters effectParams = new TriggerEffectParameters(Assets.FindEffectAssetByGuidOrLegacyId(explosionEffectGuid, explosion));
				effectParams.position = secondLastPos;
				effectParams.relevantDistance = EffectManager.LARGE;
				effectParams.wasInstigatedByPlayer = true;
				effectParams.reliable = true;
				EffectManager.triggerEffect(effectParams);

				// Credit killer's stats:
				Player player = PlayerTool.getPlayer(explosionKiller);
				if (player != null)
				{
					foreach (EPlayerKill explosionKill in kills)
					{
						player.sendStat(explosionKill);
					}
				}
			}

			Destroy(gameObject);
		}

		private void FixedUpdate()
		{
			secondLastPos = lastPos;
			lastPos = transform.position;
		}

		private void Awake()
		{
			lastPos = transform.position;
			secondLastPos = transform.position;
		}
	}
}
