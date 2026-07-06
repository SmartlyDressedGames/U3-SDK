////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableConsumeable : Useable
	{
		public delegate void PerformingAidHandler(Player instigator, Player target, ItemConsumeableAsset asset, ref bool shouldAllow);

		/// <summary>
		/// Broadcasts for plugins before applying consumeable stats to another player.
		/// </summary>
		public static event PerformingAidHandler onPerformingAid;

		public delegate void PerformedAidHandler(Player instigator, Player target);

		/// <summary>
		/// Broadcasts for plugins after applying consumeable stats to another player.
		/// </summary>
		public static event PerformedAidHandler onPerformedAid;

		public delegate void ConsumeRequestedHandler(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset, ref bool shouldAllow);

		/// <summary>
		/// Broadcasts for plugins before applying consumeable stats to self.
		/// </summary>
		public static event ConsumeRequestedHandler onConsumeRequested;

		public delegate void ConsumePerformedHandler(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset);

		/// <summary>
		/// Broadcasts for plugins after applying consumeable stats to self.
		/// </summary>
		public static event ConsumePerformedHandler onConsumePerformed;

		private bool invokeConsumeRequested(ItemConsumeableAsset asset)
		{
			if (onConsumeRequested != null)
			{
				bool shouldAllow = true;
				onConsumeRequested.Invoke(player, asset, ref shouldAllow);
				return shouldAllow;
			}
			else
			{
				return true;
			}
		}

		private void invokeConsumePerformed(ItemConsumeableAsset asset)
		{
			onConsumePerformed?.Invoke(player, asset);
		}

		private float startedUse;
		private float useTime;
		private float aidTime;

		private bool isUsing;
		private EConsumeMode consumeMode;

		private Player enemy;
		private bool hasAid;

		private bool isUseable
		{
			get
			{
				if (consumeMode == EConsumeMode.USE)
				{
					return Time.realtimeSinceStartup - startedUse > useTime;
				}
				else if (consumeMode == EConsumeMode.AID)
				{
					return Time.realtimeSinceStartup - startedUse > aidTime;
				}

				return false;
			}
		}

		private void consume()
		{
			if (consumeMode == EConsumeMode.USE)
			{
				player.animator.play("Use", false);
			}
			else if (consumeMode == EConsumeMode.AID && hasAid)
			{
				player.animator.play("Aid", false);
			}

			if (!Dedicator.IsDedicatedServer)
			{
				ItemConsumeableAsset asset = (ItemConsumeableAsset) player.equipment.asset;
				float defaultPitchDeviation = 0.1f;
				if (!asset.ShouldRandomizeUseAudioPitch)
				{
					defaultPitchDeviation = 0.0f;
				}
				player.playSound(asset.use, 0.5f, 1f, defaultPitchDeviation);
			}

			if (Provider.isServer)
			{
				AlertTool.alert(transform.position, 8);
			}
		}

		[System.Obsolete]
		public void askConsume(CSteamID steamID, byte mode)
		{
			ReceivePlayConsume((EConsumeMode) mode);
		}

		private static readonly ClientInstanceMethod<EConsumeMode> SendPlayConsume = ClientInstanceMethod<EConsumeMode>.Get(typeof(UseableConsumeable), nameof(ReceivePlayConsume));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askConsume))]
		public void ReceivePlayConsume(EConsumeMode mode)
		{
			if (player.equipment.IsEquipAnimationFinished)
			{
				consumeMode = mode;

				consume();
			}
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy || player.quests.IsCutsceneModeActive())
			{
				return false;
			}

			player.equipment.isBusy = true;
			startedUse = Time.realtimeSinceStartup;
			isUsing = true;

			consumeMode = EConsumeMode.USE;
			consume();

			if (Provider.isServer)
			{
				SendPlayConsume.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner(), consumeMode);
			}

			return true;
		}

		public override bool startSecondary()
		{
			if (player.equipment.isBusy || player.quests.IsCutsceneModeActive())
			{
				return false;
			}

			if (!hasAid)
			{
				return false;
			}

			if (channel.IsLocalPlayer)
			{
				Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
				RaycastInfo info = DamageTool.raycast(ray, 3f, RayMasks.DAMAGE_CLIENT);

				player.input.sendRaycast(info, ERaycastInfoUsage.ConsumeableAid);

				if (!Provider.isServer)
				{
					if (info.player != null)
					{
						player.equipment.isBusy = true;
						startedUse = Time.realtimeSinceStartup;
						isUsing = true;

						consumeMode = EConsumeMode.AID;
						consume();
					}
				}
			}

			if (Provider.isServer)
			{
				if (!player.input.hasInputs())
				{
					return false;
				}

				InputInfo info = player.input.getInput(true, ERaycastInfoUsage.ConsumeableAid);

				if (info == null)
				{
					return false;
				}

				if (info.type == ERaycastInfoType.PLAYER)
				{
					if (info.player != null)
					{
						enemy = info.player;

						player.equipment.isBusy = true;
						startedUse = Time.realtimeSinceStartup;
						isUsing = true;

						consumeMode = EConsumeMode.AID;
						consume();

						SendPlayConsume.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GatherRemoteClientConnectionsExcludingOwner(), consumeMode);
					}
				}
			}

			return true;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			hasAid = ((ItemConsumeableAsset) player.equipment.asset).hasAid;

			useTime = player.animator.GetAnimationLength("Use");

			if (hasAid)
			{
				aidTime = player.animator.GetAnimationLength("Aid");
			}
		}

		protected void performHealth(Player target, byte delta)
		{
			if (delta == 0)
				return;

			float instigatorHealingSkillMultiplier = 1f + (player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.HEALING) * 0.5f);
			int roundedDelta = Mathf.RoundToInt(delta * instigatorHealingSkillMultiplier);

			const bool healBleeding = false;
			const bool healBrokenBones = false;
			target.life.askHeal((byte) roundedDelta, healBleeding, healBrokenBones);
		}

		protected void performBleeding(Player target, ItemConsumeableAsset.Bleeding bleedingModifier)
		{
			switch (bleedingModifier)
			{
				case ItemConsumeableAsset.Bleeding.None:
					return;

				case ItemConsumeableAsset.Bleeding.Heal:
					target.life.serverSetBleeding(false);
					return;

				case ItemConsumeableAsset.Bleeding.Cut:
					target.life.serverSetBleeding(true);
					return;
			}
		}

		protected void performBrokenBones(Player target, ItemConsumeableAsset.Bones bonesModifier)
		{
			switch (bonesModifier)
			{
				case ItemConsumeableAsset.Bones.None:
					return;

				case ItemConsumeableAsset.Bones.Heal:
					target.life.serverSetLegsBroken(false);
					return;

				case ItemConsumeableAsset.Bones.Break:
					target.life.serverSetLegsBroken(true);
					return;
			}
		}

		/// <summary>
		/// Called serverside when using consumeable on another player.
		/// </summary>
		protected void performAid(ItemConsumeableAsset asset)
		{
			bool shouldAllow = true;
			onPerformingAid?.Invoke(player, enemy, asset, ref shouldAllow);

			if (shouldAllow == false)
			{
				player.equipment.dequip();
				return;
			}

			asset.GrantQuestRewards(enemy);
			asset.itemRewards.grantItems(enemy, EItemOrigin.CRAFT, false);

			byte preHealth = enemy.life.health;
			byte preVirus = enemy.life.virus;
			bool preBleeding = enemy.life.isBleeding;
			bool preBroken = enemy.life.isBroken;

			float qualityMultiplier = player.equipment.quality / 100f;

			performHealth(enemy, asset.health);
			performBleeding(enemy, asset.bleedingModifier);
			performBrokenBones(enemy, asset.bonesModifier);

			byte preEat = enemy.life.food;
			enemy.life.askEat((byte) (asset.food * qualityMultiplier));
			byte postEat = enemy.life.food;

			byte water = (byte) (asset.water * qualityMultiplier);
			if (asset.foodConstrainsWater)
			{
				water = (byte) Mathf.Min(water, postEat - preEat);
			}

			enemy.life.askDrink(water);

			float enemyImmunitySkillMultiplier = 1f - (enemy.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.IMMUNITY) * 0.5f);
			enemy.life.askInfect((byte) (asset.virus * enemyImmunitySkillMultiplier));

			float enemyHealingSkillMultiplier = 1f + (enemy.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.HEALING) * 0.5f);
			enemy.life.askDisinfect((byte) (asset.disinfectant * enemyHealingSkillMultiplier));

			if (player.equipment.quality < 50)
			{
				enemy.life.askInfect((byte) ((asset.food + asset.water) * 0.5f * (1f - (player.equipment.quality / 50f)) * enemyImmunitySkillMultiplier));
			}

			byte postHealth = enemy.life.health;
			byte postVirus = enemy.life.virus;
			bool postBleeding = enemy.life.isBleeding;
			bool postBroken = enemy.life.isBroken;

			uint xp = 0;
			int rep = 0;

			if (postHealth > preHealth)
			{
				xp += (uint) Mathf.RoundToInt((postHealth - preHealth) / 2.0f);
				rep++;
			}

			if (postVirus > preVirus)
			{
				xp += (uint) Mathf.RoundToInt((postVirus - preVirus) / 2.0f);
				rep++;
			}

			if (preBleeding && !postBleeding)
			{
				xp += 15;
				rep++;
			}

			if (preBroken && !postBroken)
			{
				xp += 15;
				rep++;
			}

			if (xp > 0)
			{
				player.skills.askPay(xp);
			}

			if (rep > 0)
			{
				player.skills.askRep(rep);
			}

			if (asset.vision != 0) // Otherwise colors/effects are modified. (public issue #5356)
			{
				byte newVision = (byte) (asset.vision * (1f - enemy.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.IMMUNITY)));
				enemy.life.serverModifyHallucination(newVision);
			}

			enemy.life.serverModifyStamina(asset.energy);
			enemy.life.serverModifyWarmth(asset.warmth);
			enemy.skills.ServerModifyExperience(asset.experience);

			onPerformedAid?.Invoke(player, enemy);

			if (asset.shouldDeleteAfterUse)
				player.equipment.use();
			else
				player.equipment.dequip();
		}

		/// <summary>
		/// Called by owner and server when using consumeable on self.
		/// </summary>
		protected void performUseOnSelf(ItemConsumeableAsset asset)
		{
			// Owner and Server
			{
				player.life.askRest(asset.energy);

				if (asset.vision != 0) // Otherwise colors/effects are modified. (public issue #5356)
				{
					byte currentVision = player.life.vision;
					byte newVision = (byte) (asset.vision * (1f - player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.IMMUNITY)));
					player.life.askView((byte) Mathf.Max(currentVision, newVision));
				}

				if (channel.IsLocalPlayer && asset.vision > 0)
				{
					bool data;
					if (Provider.provider.achievementsService.getAchievement("Berries", out data) && !data)
					{
						Provider.provider.achievementsService.setAchievement("Berries");
					}
				}

				player.life.simulatedModifyOxygen(asset.oxygen);
				player.life.simulatedModifyWarmth((short) asset.warmth);
			}

			if (Provider.isServer)
			{
				// We only allow the consume events for the serverside portion.
				if (invokeConsumeRequested(asset) == false)
				{
					player.equipment.dequip();
					return;
				}

				asset.GrantQuestRewards(player);
				asset.itemRewards.grantItems(player, EItemOrigin.CRAFT, false);

				Vector3 explosionPosition = transform.position + Vector3.up;

				performHealth(player, asset.health);
				performBleeding(player, asset.bleedingModifier);
				performBrokenBones(player, asset.bonesModifier);
				player.skills.ServerModifyExperience(asset.experience);

				byte preEat = player.life.food;
				player.life.askEat((byte) (asset.food * (player.equipment.quality / 100f)));
				byte postEat = player.life.food;

				byte water = (byte) (asset.water * (player.equipment.quality / 100f));
				if (asset.foodConstrainsWater)
				{
					water = (byte) Mathf.Min(water, postEat - preEat);
				}

				player.life.askDrink(water);

				player.life.askInfect((byte) (asset.virus * (1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.IMMUNITY) * 0.5f))));
				player.life.askDisinfect((byte) (asset.disinfectant * (1f + (player.skills.mastery((int) EPlayerSpeciality.SUPPORT, (int) EPlayerSupport.HEALING) * 0.5f))));

				if (player.equipment.quality < 50)
				{
					player.life.askInfect((byte) ((asset.food + asset.water) * 0.5f * (1f - (player.equipment.quality / 50f)) * (1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.IMMUNITY) * 0.5f))));
				}

				invokeConsumePerformed(asset);

				if (asset.shouldDeleteAfterUse)
					player.equipment.use();
				else
					player.equipment.dequip();

				if (asset.IsExplosive)
				{
					EffectAsset explosionEffect = asset.FindExplosionEffectAsset();
					if (explosionEffect != null)
					{
						TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(explosionEffect);
						triggerEffectParameters.relevantDistance = EffectManager.LARGE;
						triggerEffectParameters.position = explosionPosition;
						triggerEffectParameters.reliable = true;
						EffectManager.triggerEffect(triggerEffectParameters);
					}

					List<EPlayerKill> kills;
					DamageTool.explode(explosionPosition, asset.range, EDeathCause.CHARGE, channel.owner.playerID.steamID, asset.playerDamageMultiplier.damage, asset.zombieDamageMultiplier.damage, asset.animalDamageMultiplier.damage, asset.barricadeDamage, asset.structureDamage, asset.vehicleDamage, asset.resourceDamage, asset.objectDamage, out kills, damageOrigin: EDamageOrigin.Food_Explosion);

					if (asset.playerDamageMultiplier.damage > 0.5f)
					{
						EPlayerKill kill;
						player.life.askDamage(101, Vector3.up, EDeathCause.CHARGE, ELimb.SPINE, channel.owner.playerID.steamID, out kill);
					}
				}
			}
		}

		public override void simulate(uint simulation, bool inputSteady)
		{
			if (isUsing && isUseable)
			{
				player.equipment.isBusy = false;
				isUsing = false;

				ItemConsumeableAsset asset = (ItemConsumeableAsset) player.equipment.asset;
				if (asset == null)
				{
					player.equipment.dequip();
					return;
				}

				if (consumeMode == EConsumeMode.AID)
				{
					if (Provider.isServer)
					{
						if (enemy != null)
						{
							performAid(asset);
						}
					}
				}
				else
				{
					performUseOnSelf(asset);
				}
			}
		}
	}
}
