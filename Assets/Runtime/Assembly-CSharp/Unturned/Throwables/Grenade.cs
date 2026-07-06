////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public interface IExplodableThrowable
	{
		void Explode();
	}

	internal class LocallyPredictImpactDestroyThrowable : MonoBehaviour, IExplodableThrowable
	{
		public void Explode()
		{
			Destroy(gameObject);
		}
	}

	public class Grenade : MonoBehaviour, IExplodableThrowable
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
		public float fuseLength = 2.5f;
		public float explosionLaunchSpeed;

		/// <summary>
		/// Hack for modders using grenade component as a way to deal radial damage. Not a good long term solution but
		/// widely requested for the meantime until I get the chance to rewrite some of the health stuff.
		/// </summary>
		public bool shouldDestroySelf = true;

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

		public void Explode()
		{
			if (shouldDestroySelf)
			{
				Destroy(gameObject);
			}

			if (!Provider.isServer)
			{
				// In the base game this component is only attached on the server, but it seems some
				// mods are attaching it to client prefabs. Destroying itself should work fine on
				// client but damage/effect are server-only. (public issue #3750)
				return;
			}

			Steamworks.CSteamID explosionKiller = killer;
			if (explosionKiller == Steamworks.CSteamID.Nil)
			{
				// Nelson 2025-10-06: ensures instigator tracking works for nested rocket/grenade scripts.
				// For example, some mods have rockets with nested rocket components which should find the
				// instigator from the outer rocket.
				if (DamageTool.TryFindOwnership(transform.parent, out ulong ownerUser, out ulong ownerGroup))
				{
					explosionKiller = new Steamworks.CSteamID(ownerUser);
				}
			}

			List<EPlayerKill> kills;
			ExplosionParameters explosionParameters = new ExplosionParameters(transform.position, range, EDeathCause.GRENADE, explosionKiller);
			explosionParameters.playerDamage = playerDamage;
			explosionParameters.zombieDamage = zombieDamage;
			explosionParameters.animalDamage = animalDamage;
			explosionParameters.barricadeDamage = barricadeDamage;
			explosionParameters.structureDamage = structureDamage;
			explosionParameters.vehicleDamage = vehicleDamage;
			explosionParameters.resourceDamage = resourceDamage;
			explosionParameters.objectDamage = objectDamage;
			explosionParameters.damageOrigin = EDamageOrigin.Grenade_Explosion;
			explosionParameters.launchSpeed = explosionLaunchSpeed;
			DamageTool.explode(explosionParameters, out kills);

			EffectAsset explosionEffect = Assets.FindEffectAssetByGuidOrLegacyId(explosionEffectGuid, explosion);
			if (explosionEffect != null)
			{
				TriggerEffectParameters effectParams = new TriggerEffectParameters(explosionEffect);
				effectParams.position = transform.position;
				effectParams.relevantDistance = EffectManager.LARGE;
				effectParams.wasInstigatedByPlayer = true;
				effectParams.reliable = true;
				EffectManager.triggerEffect(effectParams);
			}

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

		private void Start()
		{
			if (fuseLength >= 0.0f)
			{
				Invoke("Explode", fuseLength);
			}
		}
	}
}
