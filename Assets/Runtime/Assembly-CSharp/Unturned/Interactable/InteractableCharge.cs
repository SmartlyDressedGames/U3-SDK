////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public partial class InteractableCharge : Interactable
	{
		public bool hasOwnership => OwnershipTool.checkToggle(owner, group);

		public ulong owner;
		public ulong group;

		private float range2;
		private float playerDamage;
		private float zombieDamage;
		private float animalDamage;
		private float barricadeDamage;
		private float structureDamage;
		private float vehicleDamage;
		private float resourceDamage;
		private float objectDamage;
		public System.Guid detonationEffectGuid;
		/// <summary>
		/// Kept because lots of modders have been these scripts in Unity,
		/// so removing legacy effect id would break their content.
		/// Note: unsure about this one because it is private and not serialized.
		/// </summary>
		private ushort explosion2;
		private float explosionLaunchSpeed;

		public override void updateState(Asset asset, byte[] state)
		{
			range2 = ((ItemChargeAsset) asset).range2;
			playerDamage = ((ItemChargeAsset) asset).playerDamage;
			zombieDamage = ((ItemChargeAsset) asset).zombieDamage;
			animalDamage = ((ItemChargeAsset) asset).animalDamage;
			barricadeDamage = ((ItemChargeAsset) asset).barricadeDamage;
			structureDamage = ((ItemChargeAsset) asset).structureDamage;
			vehicleDamage = ((ItemChargeAsset) asset).vehicleDamage;
			resourceDamage = ((ItemChargeAsset) asset).resourceDamage;
			objectDamage = ((ItemChargeAsset) asset).objectDamage;
			detonationEffectGuid = ((ItemChargeAsset) asset).DetonationEffectGuid;
#pragma warning disable
			explosion2 = ((ItemChargeAsset) asset).explosion2;
#pragma warning restore
			explosionLaunchSpeed = ((ItemChargeAsset) asset).explosionLaunchSpeed;

			// Pool cleanup:
			isSelected = false;
			isTargeted = false;
			unhighlight();
		}

		public override bool checkInteractable()
		{
			return false;
		}

		public void detonate(CSteamID killer)
		{
			Player instigatingPlayer = PlayerTool.getPlayer(killer);
			Detonate(instigatingPlayer);
		}

		public void Detonate(Player instigatingPlayer)
		{
			EffectAsset detonationEffectAsset = Assets.FindEffectAssetByGuidOrLegacyId(detonationEffectGuid, explosion2);
			if (detonationEffectAsset != null)
			{
				TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(detonationEffectAsset);
				triggerEffectParameters.relevantDistance = EffectManager.LARGE;
				triggerEffectParameters.position = transform.position;
				triggerEffectParameters.reliable = true;
				EffectManager.triggerEffect(triggerEffectParameters);
			}

			List<EPlayerKill> kills;
			ExplosionParameters parameters = new ExplosionParameters(transform.position, range2, EDeathCause.CHARGE);

			parameters.playerDamage = playerDamage;
			parameters.zombieDamage = zombieDamage;
			parameters.animalDamage = animalDamage;
			parameters.barricadeDamage = barricadeDamage;
			parameters.structureDamage = structureDamage;
			parameters.vehicleDamage = vehicleDamage;
			parameters.resourceDamage = resourceDamage;
			parameters.objectDamage = objectDamage;
			parameters.damageOrigin = EDamageOrigin.Charge_Explosion;
			parameters.launchSpeed = explosionLaunchSpeed;

			if (instigatingPlayer != null)
			{
				parameters.killer = instigatingPlayer.channel.owner.playerID.steamID;
				parameters.ragdollEffect = instigatingPlayer.equipment.getUseableRagdollEffect();
			}

			DamageTool.explode(parameters, out kills);

			if (instigatingPlayer != null)
			{
				foreach (EPlayerKill explosionKill in kills)
				{
					instigatingPlayer.sendStat(explosionKill);
				}
			}

			BarricadeManager.damage(transform, 5.0f, 1.0f, false, instigatorSteamID: parameters.killer, damageOrigin: EDamageOrigin.Charge_Self_Destruct);
		}

		//
		// Clientside targeting highlight features
		//

		public bool isSelected
		{
			get;
			private set;
		}

		public bool isTargeted
		{
			get;
			private set;
		}

		public void select()
		{
			if (isSelected)
			{
				return;
			}
			isSelected = true;

			updateHighlight();
		}

		public void deselect()
		{
			if (!isSelected)
			{
				return;
			}
			isSelected = false;

			updateHighlight();
		}

		public void target()
		{
			if (isTargeted)
			{
				return;
			}
			isTargeted = true;

			updateHighlight();
		}

		public void untarget()
		{
			if (!isTargeted)
			{
				return;
			}
			isTargeted = false;

			updateHighlight();
		}

		public void highlight()
		{
			PartialEnableHighlight();
		}

		public void unhighlight()
		{
			PartialDisableHighlight();
		}

		partial void updateHighlight();
		partial void PartialEnableHighlight();
		partial void PartialDisableHighlight();
	}
}
