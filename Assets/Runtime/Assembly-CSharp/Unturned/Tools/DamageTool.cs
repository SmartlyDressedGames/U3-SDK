////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define WITH_EXPLOSION_GIZMOS
// #define WITH_BULLET_IMPACT_GIZMOS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Cryptography;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	internal class ExplosionRangeComparator : IComparer<ExplosionDamageCandidate>
	{
		public Vector3 explosionCenter;

		public int Compare(ExplosionDamageCandidate lhs, ExplosionDamageCandidate rhs)
		{
			return Mathf.RoundToInt(((lhs.closestPoint - explosionCenter).sqrMagnitude - (rhs.closestPoint - explosionCenter).sqrMagnitude) * 100);
		}
	}

	public delegate void DamageToolPlayerDamagedHandler(Player player, ref EDeathCause cause, ref ELimb limb, ref CSteamID killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);
	public delegate void DamageToolZombieDamagedHandler(Zombie zombie, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);
	public delegate void DamageToolAnimalDamagedHandler(Animal animal, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);

	/// <summary>
	/// Implemented by components to support taking damage from explosions.
	/// Not intended for external use (yet?) and may need to change. 
	/// </summary>
	public interface IExplosionDamageable
		// Implements equatable for HashSet<IExplosionDamageable>
		: System.IEquatable<IExplosionDamageable>
	{
		/// <summary>
		/// Used to exclude dead entities from further evaluation.
		/// </summary>
		public bool IsEligibleForExplosionDamage
		{
			get;
		}

		/// <summary>
		/// Used to sort damage from nearest to furthest.
		/// </summary>
		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter);

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters);
	}

	/// <summary>
	/// Intended for internal use only.
	/// </summary>
	public struct ExplosionDamageParameters
	{
		// Minimum distance between explosion center and closest point to trigger a line-of-sight check.
		// Prior to 2023-02-28 this was 50cm, but the reasoning behind that is unclear. One possible explanation
		// was to compensate for previously not using the closest point. For example if the pivot of a barricade
		// is slightly behind another barricade when there would be a closer point to do a LoS test to. Reduced
		// down to 1cm because it was pointed out it broke "child armor multiplier". (public issue #3728)
		private const float MIN_LINE_OF_SIGHT_DISTANCE = 0.01f;

		internal bool LineOfSightTest(Vector3 explosionCenter, Vector3 direction, float distance, out RaycastHit hit)
		{
			if (distance > MIN_LINE_OF_SIGHT_DISTANCE)
			{
				Ray ray = new Ray(explosionCenter, direction);
				float maxDistance = distance - MIN_LINE_OF_SIGHT_DISTANCE;
				bool hitAnything = Physics.Raycast(ray, out hit, maxDistance, obstructionMask, QueryTriggerInteraction.Ignore);
				//UnturnedLog.info($"Explosion LoS test From: {ray.origin} To: {ray.origin + ray.direction * maxDistance} Hit: {hitAnything} {hit.ToDebugString()}");

#if WITH_EXPLOSION_GIZMOS
				const float explosionGizmoLifespan = 20.0f;
				RuntimeGizmos.Get().Cube(ray.origin + ray.direction * distance, 0.1f, Color.red, lifespan: explosionGizmoLifespan);
				RuntimeGizmos.Get().Raycast(ray, maxDistance, hit, Color.green, Color.red, lifespan: explosionGizmoLifespan);
#endif
				return hitAnything;
			}
			else
			{
				hit = default;
				return false;
			}
		}

		public Vector3 closestPoint;
		public List<EPlayerKill> kills;
		public uint xp;
		public int obstructionMask;
		public bool shouldAffectStructures;
		public bool shouldAffectTrees;
		public bool shouldAffectObjects;
		public bool shouldAffectBarricades;
		public bool canDealPlayerDamage;
		public bool shouldAffectPlayers;
		public bool shouldAffectZombies;
		public bool shouldAffectAnimals;
		public bool shouldAffectVehicles;
	}

	internal struct ExplosionDamageCandidate
	{
		public IExplosionDamageable target;
		public Vector3 closestPoint;
	}

	/// <summary>
	/// Data that we pool to reduce allocations, but needs to be separate per-invocation of explosion in case it's
	/// invoked recursively. (for example, by blowing up a vehicle)
	/// </summary>
	internal struct ExplosionPoolData
	{
		public List<ExplosionDamageCandidate> damageCandidates;
		public List<EPlayerKill> kills;
	}

	public class DamageTool
	{
		[System.Obsolete("Use damagePlayerRequested")]
		public static DamageToolPlayerDamagedHandler playerDamaged;
		[System.Obsolete("Use damageZombieRequested")]
		public static DamageToolZombieDamagedHandler zombieDamaged;

		[System.Obsolete("Use damageAnimalRequested")]
		public static DamageToolAnimalDamagedHandler animalDamaged;

		public delegate void DamagePlayerHandler(ref DamagePlayerParameters parameters, ref bool shouldAllow);

		/// <summary>
		/// Replacement for playerDamaged.
		/// </summary>
		public static event DamagePlayerHandler damagePlayerRequested;

		public delegate void DamageZombieHandler(ref DamageZombieParameters parameters, ref bool shouldAllow);

		/// <summary>
		/// Replacement for zombieDamaged.
		/// </summary>
		public static event DamageZombieHandler damageZombieRequested;

		public delegate void DamageAnimalHandler(ref DamageAnimalParameters parameters, ref bool shouldAllow);

		/// <summary>
		/// Replacement for animalDamaged.
		/// </summary>
		public static event DamageAnimalHandler damageAnimalRequested;

		/// <summary>
		/// Refer to ExplosionPoolData for pooling explanation.
		/// </summary>
		private static List<ExplosionPoolData> explosionDataPool = new List<ExplosionPoolData>();
		private static ExplosionRangeComparator explosionRangeComparator = new ExplosionRangeComparator();

		internal const int EXPLOSION_CLOSEST_POINT_LAYER_MASK = (RayMasks.ALL & ~RayMasks.NAVMESH);

		public static ELimb getLimb(Transform limb)
		{
			if (limb.CompareTag("Player") || limb.CompareTag("Enemy") || limb.CompareTag("Zombie") || limb.CompareTag("Animal"))
			{
				switch (limb.name)
				{
					case "Left_Foot":
						return ELimb.LEFT_FOOT;
					case "Left_Leg":
						return ELimb.LEFT_LEG;
					case "Right_Foot":
						return ELimb.RIGHT_FOOT;
					case "Right_Leg":
						return ELimb.RIGHT_LEG;
					case "Left_Hand":
						return ELimb.LEFT_HAND;
					case "Left_Arm":
						return ELimb.LEFT_ARM;
					case "Right_Hand":
						return ELimb.RIGHT_HAND;
					case "Right_Arm":
						return ELimb.RIGHT_ARM;
					case "Left_Back":
						return ELimb.LEFT_BACK;
					case "Right_Back":
						return ELimb.RIGHT_BACK;
					case "Left_Front":
						return ELimb.LEFT_FRONT;
					case "Right_Front":
						return ELimb.RIGHT_FRONT;
					case "Spine":
						return ELimb.SPINE;
					case "Skull":
						return ELimb.SKULL;
				}
			}

			return ELimb.SPINE;
		}

		public static Player getPlayer(Transform limb)
		{
			Player player = limb.GetComponentInParent<Player>();

			if (player != null && player.life.isDead)
			{
				player = null;
			}

			return player;
		}

		public static Zombie getZombie(Transform limb)
		{
			Zombie zombie = limb.GetComponentInParent<Zombie>();

			if (zombie != null && zombie.isDead)
			{
				zombie = null;
			}

			return zombie;
		}

		public static Animal getAnimal(Transform limb)
		{
			Animal animal = limb.GetComponentInParent<Animal>();

			if (animal != null && animal.isDead)
			{
				animal = null;
			}

			return animal;
		}

		public static InteractableVehicle getVehicle(Transform model)
		{
			if (model == null)
			{
				return null;
			}

			model = model.root;

			InteractableVehicle vehicle = model.GetComponent<InteractableVehicle>();
			if (vehicle != null)
			{
				return vehicle;
			}

			VehicleRef vehicleRef = model.GetComponent<VehicleRef>();
			if (vehicleRef != null)
			{
				return vehicleRef.vehicle;
			}

			return null;
		}

		public static Transform getBarricadeRootTransform(Transform barricadeTransform)
		{
			Transform node = barricadeTransform;
			while (true)
			{
				Transform parent = node.parent;
				if (parent == null)
				{
					// Not attached to a vehicle.
					return node;
				}
				else if (parent.CompareTag("Vehicle"))
				{
					// Is attached to a vehicle.
					return node;
				}
				else
				{
					node = parent;
				}
			}
		}

		/// <summary>
		/// Was necessary when structures were children of level transform.
		/// </summary>
		public static Transform getStructureRootTransform(Transform structureTransform)
		{
			return structureTransform.root;
		}

		/// <summary>
		/// Was necessary when trees were children of ground transform.
		/// </summary>
		public static Transform getResourceRootTransform(Transform resourceTransform)
		{
			return resourceTransform.root;
		}

		/// <summary>
		/// Somewhat hacked-together to find owner of a vehicle, barricade, or structure descendant.
		/// Checks each IOwnershipInfo up the hierarchy until one returns true.
		/// </summary>
		public static bool TryFindOwnership(Transform transform, out ulong ownerUser, out ulong ownerGroup)
		{
			// GetComponentInParent includes input game object.
			IOwnershipInfo ownershipInfo = transform?.GetComponentInParent<IOwnershipInfo>();
			while (ownershipInfo != null)
			{
				if (ownershipInfo.TryGetOwnership(out ownerUser, out ownerGroup))
				{
					return true;
				}

				// Try next level up, if possible.
				if (ownershipInfo is Component component)
				{
					ownershipInfo = component.transform.parent?.GetComponentInParent<IOwnershipInfo>();
				}
				else
				{
					break;
				}
			}

			ownerUser = 0;
			ownerGroup = 0;
			return false;
		}

		public static void damagePlayer(DamagePlayerParameters parameters, out EPlayerKill kill)
		{
			if (parameters.player == null || parameters.player.life.isDead)
			{
				kill = EPlayerKill.NONE;
				return;
			}

			bool shouldAllow = true;
			damagePlayerRequested?.Invoke(ref parameters, ref shouldAllow);
#pragma warning disable
			if (playerDamaged != null)
			{
				playerDamaged(parameters.player, ref parameters.cause, ref parameters.limb, ref parameters.killer, ref parameters.direction, ref parameters.damage, ref parameters.times, ref shouldAllow);
			}
#pragma warning restore

			if (!shouldAllow)
			{
				kill = EPlayerKill.NONE;
				return;
			}

			if (parameters.respectArmor)
			{
				parameters.times *= getPlayerArmor(parameters.limb, parameters.player);
			}

			if (parameters.applyGlobalArmorMultiplier)
			{
				parameters.times *= Provider.modeConfigData.Players.Armor_Multiplier;
			}

			int roundedDamage = Mathf.FloorToInt(parameters.damage * parameters.times);
			if (roundedDamage == 0)
			{
				kill = EPlayerKill.NONE;
				return;
			}

			byte amount = (byte) Mathf.Min(byte.MaxValue, roundedDamage);

			// Safezone and respawn safety checks.
			bool canAttackPlayer = parameters.player.life.InternalCanDamage();

			bool canCauseBleeding;
			switch (parameters.bleedingModifier)
			{
				default:
				case DamagePlayerParameters.Bleeding.Default:
					canCauseBleeding = true;
					break;

				case DamagePlayerParameters.Bleeding.Always:
					canCauseBleeding = false;
					if (canAttackPlayer)
					{
						parameters.player.life.serverSetBleeding(true);
					}
					break;

				case DamagePlayerParameters.Bleeding.Never:
					canCauseBleeding = false;
					break;

				case DamagePlayerParameters.Bleeding.Heal:
					canCauseBleeding = false;
					parameters.player.life.serverSetBleeding(false);
					break;
			}

			parameters.player.life.askDamage(amount, parameters.direction * amount, parameters.cause, parameters.limb, parameters.killer, out kill, trackKill: parameters.trackKill, newRagdollEffect: parameters.ragdollEffect, canCauseBleeding: canCauseBleeding);

			switch (parameters.bonesModifier)
			{
				default:
				case DamagePlayerParameters.Bones.None:
					break;

				case DamagePlayerParameters.Bones.Always:
					if (canAttackPlayer)
					{
						parameters.player.life.serverSetLegsBroken(true);
					}
					break;

				case DamagePlayerParameters.Bones.Heal:
					parameters.player.life.serverSetLegsBroken(false);
					break;
			}

			if (parameters.foodModifier > 0 || canAttackPlayer)
			{
				parameters.player.life.serverModifyFood(parameters.foodModifier);
			}

			if (parameters.waterModifier > 0 || canAttackPlayer)
			{
				parameters.player.life.serverModifyWater(parameters.waterModifier);
			}

			if (parameters.virusModifier > 0 || canAttackPlayer)
			{
				parameters.player.life.serverModifyVirus(parameters.virusModifier);
			}

			// Negative hallucination modifier is beneficial. (Decreases duration of any current hallucination effect.)
			if (parameters.hallucinationModifier < 0 || canAttackPlayer)
			{
				parameters.player.life.serverModifyHallucination(parameters.hallucinationModifier);
			}
		}

		public static void damage(Player player, EDeathCause cause, ELimb limb, CSteamID killer, Vector3 direction, float damage, float times, out EPlayerKill kill, bool applyGlobalArmorMultiplier = true, bool trackKill = false, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			DamagePlayerParameters parameters = new DamagePlayerParameters(player);
			parameters.cause = cause;
			parameters.limb = limb;
			parameters.killer = killer;
			parameters.direction = direction;
			parameters.damage = damage;
			parameters.times = times;
			parameters.applyGlobalArmorMultiplier = applyGlobalArmorMultiplier;
			parameters.trackKill = trackKill;
			parameters.ragdollEffect = ragdollEffect;
			damagePlayer(parameters, out kill);
		}

		/// <summary>
		/// Get average explosionArmor of player's equipped clothing.
		/// </summary>
		public static float getPlayerExplosionArmor(Player player)
		{
			if (player == null)
				return 1.0f;

			// Originally this was the actual number of items equipped, but that meant a single high-armor value item
			// would have higher explosion protection while naked.
			// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/3061
			const int numArmor = 4;

			float totalArmor = 0.0f; // Total of explosionArmor values.

			totalArmor += player.clothing.pantsAsset?.explosionArmor ?? 1.0f;
			totalArmor += player.clothing.shirtAsset?.explosionArmor ?? 1.0f;
			totalArmor += player.clothing.vestAsset?.explosionArmor ?? 1.0f;
			totalArmor += player.clothing.hatAsset?.explosionArmor ?? 1.0f;

			return totalArmor / numArmor;
		}

		public static float getPlayerArmor(ELimb limb, Player player)
		{
			if (limb == ELimb.LEFT_FOOT || limb == ELimb.LEFT_LEG || limb == ELimb.RIGHT_FOOT || limb == ELimb.RIGHT_LEG)
			{
				ItemClothingAsset asset = player.clothing.pantsAsset;
				if (asset != null)
				{
					if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
					{
						if (player.clothing.pantsQuality > 0)
						{
							player.clothing.pantsQuality--;

							player.clothing.sendUpdatePantsQuality();
						}
					}

					return asset.armor + ((1f - asset.armor) * (1f - (player.clothing.pantsQuality / 100f)));
				}
			}
			else if (limb == ELimb.LEFT_HAND || limb == ELimb.LEFT_ARM || limb == ELimb.RIGHT_HAND || limb == ELimb.RIGHT_ARM)
			{
				ItemClothingAsset asset = player.clothing.shirtAsset;
				if (asset != null)
				{
					if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
					{
						if (player.clothing.shirtQuality > 0)
						{
							player.clothing.shirtQuality--;

							player.clothing.sendUpdateShirtQuality();
						}
					}

					return asset.armor + ((1f - asset.armor) * (1f - (player.clothing.shirtQuality / 100f))); // ie 0.9f + 0.1f * 0.5f
				}
			}
			else if (limb == ELimb.SPINE)
			{
				float armor = 1.0f;

				if (player.clothing.vestAsset != null)
				{
					ItemClothingAsset asset = player.clothing.vestAsset;

					if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
					{
						if (player.clothing.vestQuality > 0)
						{
							player.clothing.vestQuality--;

							player.clothing.sendUpdateVestQuality();
						}
					}

					armor *= asset.armor + ((1f - asset.armor) * (1f - (player.clothing.vestQuality / 100f)));
				}

				if (player.clothing.shirtAsset != null)
				{
					ItemClothingAsset asset = player.clothing.shirtAsset;

					if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
					{
						if (player.clothing.shirtQuality > 0)
						{
							player.clothing.shirtQuality--;

							player.clothing.sendUpdateShirtQuality();
						}
					}

					armor *= asset.armor + ((1f - asset.armor) * (1f - (player.clothing.shirtQuality / 100f)));
				}

				return armor;
			}
			else if (limb == ELimb.SKULL)
			{
				ItemClothingAsset asset = player.clothing.hatAsset;
				if (asset != null)
				{
					if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
					{
						if (player.clothing.hatQuality > 0)
						{
							player.clothing.hatQuality--;

							player.clothing.sendUpdateHatQuality();
						}
					}

					return asset.armor + ((1f - asset.armor) * (1f - (player.clothing.hatQuality / 100f)));
				}
			}

			return 1f;
		}

		/// <summary>
		/// Refer to getPlayerExplosionArmor for explanation of total/average.
		/// </summary>
		public static float GetZombieExplosionArmor(Zombie zombie)
		{
			if (zombie.type < LevelZombies.tables.Count)
			{
				// 0 Shirt
				// 1 Pants
				// 2 Hat
				// 3 Gear

				const int numArmor = 4;
				float totalArmor = 0.0f; // Total of explosionArmor values.

				if (zombie.pants != 255 && zombie.pants < LevelZombies.tables[zombie.type].slots[1].table.Count)
				{
					ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[1].table[zombie.pants].item) as ItemClothingAsset;
					totalArmor += asset?.explosionArmor ?? 1.0f;
				}
				else
				{
					totalArmor += 1.0f;
				}

				if (zombie.shirt != 255 && zombie.shirt < LevelZombies.tables[zombie.type].slots[0].table.Count)
				{
					ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[0].table[zombie.shirt].item) as ItemClothingAsset;
					totalArmor += asset?.explosionArmor ?? 1.0f;
				}
				else
				{
					totalArmor += 1.0f;
				}

				if (zombie.gear != 255 && zombie.gear < LevelZombies.tables[zombie.type].slots[3].table.Count)
				{
					ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[3].table[zombie.gear].item) as ItemClothingAsset;
					totalArmor += asset?.explosionArmor ?? 1.0f;
				}
				else
				{
					totalArmor += 1.0f;
				}

				if (zombie.hat != 255 && zombie.hat < LevelZombies.tables[zombie.type].slots[2].table.Count)
				{
					ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[2].table[zombie.hat].item) as ItemClothingAsset;
					totalArmor += asset?.explosionArmor ?? 1.0f;
				}
				else
				{
					totalArmor += 1.0f;
				}

				return totalArmor / numArmor;
			}

			return 1f;
		}

		public static float getZombieArmor(ELimb limb, Zombie zombie)
		{
			if (zombie.type < LevelZombies.tables.Count)
			{
				// 0 Shirt
				// 1 Pants
				// 2 Hat
				// 3 Gear

				if (limb == ELimb.LEFT_FOOT || limb == ELimb.LEFT_LEG || limb == ELimb.RIGHT_FOOT || limb == ELimb.RIGHT_LEG)
				{
					if (zombie.pants != 255 && zombie.pants < LevelZombies.tables[zombie.type].slots[1].table.Count)
					{
						ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[1].table[zombie.pants].item) as ItemClothingAsset;

						if (asset != null)
						{
							return asset.armor;
						}
					}
				}
				else if (limb == ELimb.LEFT_HAND || limb == ELimb.LEFT_ARM || limb == ELimb.RIGHT_HAND || limb == ELimb.RIGHT_ARM)
				{
					if (zombie.shirt != 255 && zombie.shirt < LevelZombies.tables[zombie.type].slots[0].table.Count)
					{
						ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[0].table[zombie.shirt].item) as ItemClothingAsset;

						if (asset != null)
						{
							return asset.armor;
						}
					}
				}
				else if (limb == ELimb.SPINE)
				{
					float armor = 1.0f;

					if (zombie.gear != 255 && zombie.gear < LevelZombies.tables[zombie.type].slots[3].table.Count)
					{
						ItemAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[3].table[zombie.gear].item) as ItemAsset;

						if (asset != null && asset.type == EItemType.VEST)
						{
							armor *= ((ItemClothingAsset) asset).armor;
						}
					}

					if (zombie.shirt != 255 && zombie.shirt < LevelZombies.tables[zombie.type].slots[0].table.Count)
					{
						ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[0].table[zombie.shirt].item) as ItemClothingAsset;

						if (asset != null)
						{
							armor *= asset.armor;
						}
					}

					return armor;
				}
				else if (limb == ELimb.SKULL)
				{
					if (zombie.hat != 255 && zombie.hat < LevelZombies.tables[zombie.type].slots[2].table.Count)
					{
						ItemClothingAsset asset = Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[2].table[zombie.hat].item) as ItemClothingAsset;

						if (asset != null)
						{
							return asset.armor;
						}
					}
				}
			}

			return 1f;
		}

		public static void damage(Player player, EDeathCause cause, ELimb limb, CSteamID killer, Vector3 direction, IDamageMultiplier multiplier, float times, bool armor, out EPlayerKill kill, bool trackKill = false, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			DamagePlayerParameters parameters = DamagePlayerParameters.make(player, cause, direction, multiplier, limb);
			parameters.killer = killer;
			parameters.times = times;
			parameters.respectArmor = armor;
			parameters.trackKill = trackKill;
			parameters.ragdollEffect = ragdollEffect;
			damagePlayer(parameters, out kill);
		}

		/// <summary>
		/// Do damage to a zombie.
		/// </summary>
		public static void damageZombie(DamageZombieParameters parameters, out EPlayerKill kill, out uint xp)
		{
			if (parameters.zombie == null || parameters.zombie.isDead)
			{
				kill = EPlayerKill.NONE;
				xp = 0;

				return;
			}

			// Moved from the alternative damage function which determined this from "armor" parameter.
			if (parameters.respectArmor)
			{
				parameters.times *= getZombieArmor(parameters.limb, parameters.zombie);
			}

			// Moved from the alternative damage function which determined this from "armor" parameter.
			if (parameters.allowBackstab)
			{
				if (Vector3.Dot(parameters.zombie.transform.forward, parameters.direction) > 0.5)
				{
					parameters.times *= Provider.modeConfigData.Zombies.Backstab_Multiplier;

					if (Provider.modeConfigData.Zombies.Only_Critical_Stuns && parameters.zombieStunOverride == EZombieStunOverride.None)
					{
						parameters.zombieStunOverride = EZombieStunOverride.Always;
					}
				}
			}

			bool shouldAllow = true;
			damageZombieRequested?.Invoke(ref parameters, ref shouldAllow);
#pragma warning disable
			if (zombieDamaged != null)
			{
				zombieDamaged(parameters.zombie, ref parameters.direction, ref parameters.damage, ref parameters.times, ref shouldAllow);
			}
#pragma warning restore

			if (!shouldAllow)
			{
				kill = EPlayerKill.NONE;
				xp = 0;
				return;
			}

			if (parameters.applyGlobalArmorMultiplier)
			{
				if (parameters.limb == ELimb.SKULL)
				{
					parameters.times *= Provider.modeConfigData.Zombies.Armor_Multiplier;
				}
				else
				{
					parameters.times *= Provider.modeConfigData.Zombies.NonHeadshot_Armor_Multiplier;
				}
			}

			int roundedDamage = Mathf.FloorToInt(parameters.damage * parameters.times);
			if (roundedDamage == 0)
			{
				kill = EPlayerKill.NONE;
				xp = 0;
				return;
			}

			ushort amount = (ushort) Mathf.Min(ushort.MaxValue, roundedDamage);
			Vector3 ragdoll = parameters.direction * amount * parameters.RagdollForceMultiplier;
			parameters.zombie.askDamage(amount, ragdoll, out kill, out xp, stunOverride: parameters.zombieStunOverride, ragdollEffect: parameters.ragdollEffect);

			if (parameters.AlertPosition.HasValue)
			{
				parameters.zombie.alert(parameters.AlertPosition.Value, true);
			}
		}

		/// <summary>
		/// Legacy function replaced by damageZombie.
		/// </summary>
		public static void damage(Zombie zombie, Vector3 direction, float damage, float times, out EPlayerKill kill, out uint xp, EZombieStunOverride zombieStunOverride = EZombieStunOverride.None, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			DamageZombieParameters parameters = new DamageZombieParameters(zombie, direction, damage);
			parameters.times = times;
			parameters.zombieStunOverride = zombieStunOverride;
			parameters.ragdollEffect = ragdollEffect;
			damageZombie(parameters, out kill, out xp);
		}

		/// <summary>
		/// Legacy function replaced by damageZombie.
		/// </summary>
		public static void damage(Zombie zombie, ELimb limb, Vector3 direction, IDamageMultiplier multiplier, float times, bool armor, out EPlayerKill kill, out uint xp, EZombieStunOverride zombieStunOverride = EZombieStunOverride.None, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			DamageZombieParameters parameters = DamageZombieParameters.make(zombie, direction, multiplier, limb);
			parameters.legacyArmor = armor;
			parameters.times = times;
			parameters.zombieStunOverride = zombieStunOverride;
			parameters.ragdollEffect = ragdollEffect;
			damageZombie(parameters, out kill, out xp);
		}

		/// <summary>
		/// Do damage to an animal.
		/// </summary>
		public static void damageAnimal(DamageAnimalParameters parameters, out EPlayerKill kill, out uint xp)
		{
			if (parameters.animal == null || parameters.animal.isDead)
			{
				kill = EPlayerKill.NONE;
				xp = 0;

				return;
			}

			bool shouldAllow = true;
			damageAnimalRequested?.Invoke(ref parameters, ref shouldAllow);
#pragma warning disable
			if (animalDamaged != null)
			{
				animalDamaged(parameters.animal, ref parameters.direction, ref parameters.damage, ref parameters.times, ref shouldAllow);
			}
#pragma warning restore

			if (!shouldAllow)
			{
				kill = EPlayerKill.NONE;
				xp = 0;
				return;
			}

			if (parameters.applyGlobalArmorMultiplier)
			{
				parameters.times *= Provider.modeConfigData.Animals.Armor_Multiplier;
			}

			int roundedDamage = Mathf.FloorToInt(parameters.damage * parameters.times);
			if (roundedDamage == 0)
			{
				kill = EPlayerKill.NONE;
				xp = 0;
				return;
			}

			ushort amount = (ushort) Mathf.Min(ushort.MaxValue, roundedDamage);
			parameters.animal.askDamage(amount, parameters.direction * amount, out kill, out xp, ragdollEffect: parameters.ragdollEffect);

			if (parameters.AlertPosition.HasValue)
			{
				parameters.animal.alertDamagedFromPoint(parameters.AlertPosition.Value);
			}
		}

		/// <summary>
		/// Legacy function replaced by damageAnimal.
		/// </summary>
		public static void damage(Animal animal, Vector3 direction, float damage, float times, out EPlayerKill kill, out uint xp, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			DamageAnimalParameters parameters = new DamageAnimalParameters(animal, direction, damage);
			parameters.times = times;
			parameters.ragdollEffect = ragdollEffect;
			damageAnimal(parameters, out kill, out xp);
		}

		/// <summary>
		/// Legacy function replaced by damageAnimal.
		/// </summary>
		public static void damage(Animal animal, ELimb limb, Vector3 direction, IDamageMultiplier multiplier, float times, out EPlayerKill kill, out uint xp, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			DamageAnimalParameters parameters = DamageAnimalParameters.make(animal, direction, multiplier, limb);
			parameters.times = times;
			parameters.ragdollEffect = ragdollEffect;
			damageAnimal(parameters, out kill, out xp);
		}

		public static void damage(InteractableVehicle vehicle, bool damageTires, Vector3 position, bool isRepairing, float vehicleDamage, float times, bool canRepair, out EPlayerKill kill, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			kill = EPlayerKill.NONE;

			if (vehicle == null)
			{
				return;
			}

			if (isRepairing)
			{
				if (!vehicle.isExploded && !vehicle.isRepaired)
				{
					VehicleManager.repair(vehicle, vehicleDamage, times, instigatorSteamID: instigatorSteamID);
				}
			}
			else
			{
				if (!vehicle.isDead)
				{
					VehicleManager.damage(vehicle, vehicleDamage, times, canRepair, instigatorSteamID: instigatorSteamID, damageOrigin: damageOrigin);
				}

				if (damageTires && !vehicle.isExploded)
				{
					int tireIndex = vehicle.getHitTireIndex(position);
					if (tireIndex != -1)
					{
						VehicleManager.damageTire(vehicle, tireIndex, instigatorSteamID: instigatorSteamID, damageOrigin: damageOrigin);
					}
				}
			}
		}

		public static void damage(Transform barricade, bool isRepairing, float barricadeDamage, float times, out EPlayerKill kill, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			kill = EPlayerKill.NONE;

			if (barricade == null)
			{
				return;
			}

			if (isRepairing)
			{
				BarricadeManager.repair(barricade, barricadeDamage, times, instigatorSteamID);
			}
			else
			{
				BarricadeManager.damage(barricade, barricadeDamage, times, true, instigatorSteamID: instigatorSteamID, damageOrigin: damageOrigin);
			}
		}

		public static void damage(Transform structure, bool isRepairing, Vector3 direction, float structureDamage, float times, out EPlayerKill kill, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			kill = EPlayerKill.NONE;

			if (structure == null)
			{
				return;
			}

			if (isRepairing)
			{
				StructureManager.repair(structure, structureDamage, times, instigatorSteamID);
			}
			else
			{
				StructureManager.damage(structure, direction, structureDamage, times, true, instigatorSteamID: instigatorSteamID, damageOrigin: damageOrigin);
			}
		}

		public static void damage(Transform resource, Vector3 direction, float resourceDamage, float times, float drops, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			if (resource == null)
			{
				kill = EPlayerKill.NONE;
				xp = 0;

				return;
			}

			ResourceManager.damage(resource, direction, resourceDamage, times, drops, out kill, out xp, instigatorSteamID: instigatorSteamID, damageOrigin: damageOrigin);
		}

		public static void damage(Transform obj, Vector3 direction, byte section, float objectDamage, float times, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = new CSteamID(), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
		{
			if (obj == null)
			{
				kill = EPlayerKill.NONE;
				xp = 0;

				return;
			}

			ObjectManager.damage(obj, direction, section, objectDamage, times, out kill, out xp, instigatorSteamID: instigatorSteamID, damageOrigin: damageOrigin, trackKill: true);
		}

		/// <summary>
		/// This unwieldy mess is the original explode function, but should be maintained for backwards compatibility with plugins.
		/// </summary>
		public static void explode(Vector3 point, float damageRadius, EDeathCause cause, CSteamID killer, float playerDamage, float zombieDamage, float animalDamage, float barricadeDamage, float structureDamage, float vehicleDamage, float resourceDamage, float objectDamage, out List<EPlayerKill> kills, EExplosionDamageType damageType = EExplosionDamageType.CONVENTIONAL, float alertRadius = 32.0f, bool playImpactEffect = true, bool penetrateBuildables = false, EDamageOrigin damageOrigin = EDamageOrigin.Unknown, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			ExplosionParameters parameters = new ExplosionParameters(point, damageRadius, cause, killer);

			parameters.playerDamage = playerDamage;
			parameters.zombieDamage = zombieDamage;
			parameters.animalDamage = animalDamage;
			parameters.barricadeDamage = barricadeDamage;
			parameters.structureDamage = structureDamage;
			parameters.vehicleDamage = vehicleDamage;
			parameters.resourceDamage = resourceDamage;
			parameters.objectDamage = objectDamage;

			parameters.damageType = damageType;
			parameters.alertRadius = alertRadius;
			parameters.playImpactEffect = playImpactEffect;
			parameters.penetrateBuildables = penetrateBuildables;
			parameters.damageOrigin = damageOrigin;
			parameters.ragdollEffect = ragdollEffect;
			parameters.launchSpeed = playerDamage * 0.1f;

			explode(parameters, out kills);
		}

		private static Collider[] explosionColliders = new Collider[256];
		private static HashSet<IExplosionDamageable> explosionOverlaps = new HashSet<IExplosionDamageable>();

		/// <summary>
		/// Used if explosion won't damage anything.
		/// </summary>
		private static List<EPlayerKill> emptyKillsList = new List<EPlayerKill>();

		/// <summary>
		/// Do radial damage.
		/// </summary>
		public static void explode(ExplosionParameters parameters, out List<EPlayerKill> kills)
		{
			ThreadUtil.assertIsGameThread();

			emptyKillsList.Clear(); // Just in case caller modified it for some reason.
			kills = emptyKillsList;

#if WITH_EXPLOSION_GIZMOS
			const float explosionGizmoLifespan = 20.0f;
			RuntimeGizmos.Get().Box(parameters.point, new Vector3(0.01f, 0.01f, 0.01f), Color.red, explosionGizmoLifespan);
			RuntimeGizmos.Get().Sphere(parameters.point, parameters.damageRadius, Color.red, explosionGizmoLifespan);
#endif

			bool shouldAffectStructures = parameters.structureDamage > 0.5f;
			bool shouldAffectTrees = parameters.resourceDamage > 0.5f;
			bool shouldAffectObjects = parameters.objectDamage > 0.5f;
			bool shouldAffectBarricades = parameters.barricadeDamage > 0.5f;
			bool canDealPlayerDamage = (Provider.isPvP || parameters.damageType == EExplosionDamageType.ZOMBIE_ACID || parameters.damageType == EExplosionDamageType.ZOMBIE_FIRE || parameters.damageType == EExplosionDamageType.ZOMBIE_ELECTRIC) && parameters.playerDamage > 0.5f;
			bool shouldAffectPlayers = canDealPlayerDamage || parameters.launchSpeed > 0.01f;
			bool shouldAffectZombies = parameters.damageType == EExplosionDamageType.ZOMBIE_FIRE || parameters.zombieDamage > 0.5f;
			bool shouldAffectAnimals = parameters.animalDamage > 0.5f;
			bool shouldAffectVehicles = parameters.vehicleDamage > 0.5f;

			int overlapLayerMask = 0;
			overlapLayerMask |= (shouldAffectStructures ? RayMasks.STRUCTURE : 0);
			overlapLayerMask |= (shouldAffectTrees ? RayMasks.RESOURCE : 0);
			overlapLayerMask |= (shouldAffectObjects ? (RayMasks.LARGE | RayMasks.MEDIUM | RayMasks.SMALL) : 0);
			overlapLayerMask |= (shouldAffectBarricades ? (RayMasks.BARRICADE | RayMasks.RESOURCE) : 0); // Resource because barricades on vehicles. :|
			overlapLayerMask |= (shouldAffectPlayers ? (RayMasks.PLAYER | RayMasks.ENEMY) : 0);
			overlapLayerMask |= (shouldAffectZombies ? RayMasks.AGENT : 0);
			overlapLayerMask |= (shouldAffectAnimals ? RayMasks.AGENT : 0);
			overlapLayerMask |= (shouldAffectVehicles ? RayMasks.VEHICLE : 0);

			if (overlapLayerMask == 0)
			{
				return;
			}

			// Nelson 2024-11-12: Note that explosionColliders does not need to be pooled the same way the data struct
			// is because its use will be finished by the time explode is potentially called recursively.
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
			int overlapCount = Physics.OverlapSphereNonAlloc(parameters.point, parameters.damageRadius, explosionColliders, overlapLayerMask, queryTriggerInteraction);
			if (overlapCount < 1)
			{
				return;
			}

			if (overlapCount == explosionColliders.Length)
			{
				// Nelson 2024-11-12: It's quite plausible this limit will be reached by modded weapons like "the nuke"
				// mod, so try not to spam the log and use the array allocated by Unity as our new buffer.
				UnturnedLog.warn($"Explosion overlap reached non-alloc collider limit ({overlapCount})! (Radius: {parameters.damageRadius})");
				explosionColliders = Physics.OverlapSphere(parameters.point, parameters.damageRadius, overlapLayerMask, queryTriggerInteraction);
				overlapCount = explosionColliders.Length;
				UnturnedLog.warn($"New explosion collider limit: {overlapCount}");
			}

			ExplosionPoolData data;
			if (explosionDataPool.Count > 0)
			{
				data = explosionDataPool.GetAndRemoveTail();
			}
			else
			{
				data = new ExplosionPoolData()
				{
					damageCandidates = new List<ExplosionDamageCandidate>(),
					kills = new List<EPlayerKill>(),
				};
			}

			data.damageCandidates.Clear();
			data.kills.Clear();

			explosionOverlaps.Clear();

			try
			{
				for (int colliderIndex = 0; colliderIndex < overlapCount; ++colliderIndex)
				{
					Collider hitCollider = explosionColliders[colliderIndex];
					if (hitCollider == null)
					{
						// How'd this happen? Anyway...
						continue;
					}

					Transform hitTransform = hitCollider.transform;
					if (hitTransform == null)
					{
						// Same as above. :P
						continue;
					}

					IExplosionDamageable damageable = hitTransform.GetComponentInParent<IExplosionDamageable>();
					if (damageable != null && damageable.IsEligibleForExplosionDamage)
					{
						bool added = explosionOverlaps.Add(damageable);
						if (added)
						{
							Vector3 closestPoint = damageable.GetClosestPointToExplosion(parameters.point);
							ExplosionDamageCandidate candidate = new ExplosionDamageCandidate()
							{
								target = damageable,
								closestPoint = closestPoint,
							};
							data.damageCandidates.Add(candidate);
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception while evaluating explosion damage candidates:");
			}

			if (data.damageCandidates.IsEmpty())
			{
				explosionDataPool.Add(data);
				return;
			}

			explosionRangeComparator.explosionCenter = parameters.point;
			data.damageCandidates.Sort(explosionRangeComparator);

			int obstructionMask;
			if (parameters.penetrateBuildables)
			{
				obstructionMask = RayMasks.BLOCK_EXPLOSION_PENETRATE_BUILDABLES;
			}
			else
			{
				obstructionMask = RayMasks.BLOCK_EXPLOSION;
			}

			ExplosionDamageParameters damageParameters = new ExplosionDamageParameters()
			{
				kills = data.kills,
				xp = 0,
				obstructionMask = obstructionMask,
				shouldAffectStructures = shouldAffectStructures,
				shouldAffectTrees = shouldAffectTrees,
				shouldAffectObjects = shouldAffectObjects,
				shouldAffectBarricades = shouldAffectBarricades,
				canDealPlayerDamage = canDealPlayerDamage,
				shouldAffectPlayers = shouldAffectPlayers,
				shouldAffectZombies = shouldAffectZombies,
				shouldAffectAnimals = shouldAffectAnimals,
				shouldAffectVehicles = shouldAffectVehicles,
			};

			try
			{
				foreach (ExplosionDamageCandidate candidate in data.damageCandidates)
				{
					if (candidate.target == null || !candidate.target.IsEligibleForExplosionDamage)
					{
						// May have been destroyed be a recursive explosion?
						continue;
					}

					damageParameters.closestPoint = candidate.closestPoint;
					candidate.target.ApplyExplosionDamage(parameters, ref damageParameters);
				}
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception while applying explosion damage:");
			}

			AlertTool.alert(parameters.point, parameters.alertRadius);

			kills = data.kills;
			explosionDataPool.Add(data);
		}

		[System.Obsolete("Physics material enum replaced by string names")]
		public static EPhysicsMaterial getMaterial(Vector3 point, Transform transform, Collider collider)
		{
			return PhysicsTool.GetLegacyMaterialByName(PhysicsTool.GetMaterialName(point, transform, collider));
		}

		/// <summary>
		/// Server spawn impact effect for all players within range.
		/// </summary>
		[System.Obsolete("Replaced by separate melee and bullet impact methods")]
		public static void impact(Vector3 point, Vector3 normal, EPhysicsMaterial material, bool forceDynamic)
		{
			impact(point, normal, material, forceDynamic, CSteamID.Nil, point);
		}

		/// <summary>
		/// Server spawn impact effect for all players within range. Optional "spectator" receives effect regardless of distance.
		/// </summary>
		[System.Obsolete("Replaced by separate melee and bullet impact methods")]
		public static void impact(Vector3 point, Vector3 normal, EPhysicsMaterial material, bool forceDynamic, CSteamID spectatorID, Vector3 spectatorPoint)
		{
			if (material == EPhysicsMaterial.NONE)
			{
				return;
			}

			ushort id = 0;

			if (material == EPhysicsMaterial.CLOTH_DYNAMIC || material == EPhysicsMaterial.TILE_DYNAMIC || material == EPhysicsMaterial.CONCRETE_DYNAMIC)
			{
				id = 38;
			}
			else if (material == EPhysicsMaterial.CLOTH_STATIC || material == EPhysicsMaterial.TILE_STATIC || material == EPhysicsMaterial.CONCRETE_STATIC)
			{
				id = forceDynamic ? (ushort) 38 : (ushort) 13;
			}
			else if (material == EPhysicsMaterial.FLESH_DYNAMIC)
			{
				id = 5;
			}
			else if (material == EPhysicsMaterial.GRAVEL_DYNAMIC)
			{
				id = 44;
			}
			else if (material == EPhysicsMaterial.GRAVEL_STATIC || material == EPhysicsMaterial.SAND_STATIC)
			{
				id = forceDynamic ? (ushort) 44 : (ushort) 14;
			}
			else if (material == EPhysicsMaterial.METAL_DYNAMIC)
			{
				id = 18;
			}
			else if (material == EPhysicsMaterial.METAL_STATIC || material == EPhysicsMaterial.METAL_SLIP)
			{
				id = forceDynamic ? (ushort) 18 : (ushort) 12;
			}
			else if (material == EPhysicsMaterial.WOOD_DYNAMIC)
			{
				id = 17;
			}
			else if (material == EPhysicsMaterial.WOOD_STATIC)
			{
				id = forceDynamic ? (ushort) 17 : (ushort) 2;
			}
			else if (material == EPhysicsMaterial.FOLIAGE_STATIC || material == EPhysicsMaterial.FOLIAGE_DYNAMIC)
			{
				id = 15;
			}
			else if (material == EPhysicsMaterial.SNOW_STATIC || material == EPhysicsMaterial.ICE_STATIC)
			{
				id = 41;
			}
			else if (material == EPhysicsMaterial.WATER_STATIC)
			{
				id = 16;
			}
			else if (material == EPhysicsMaterial.ALIEN_DYNAMIC)
			{
				id = 95;
			}

			impact(point, normal, id, spectatorID, spectatorPoint);
		}

		/// <summary>
		/// Server spawn effect by ID for all players within range. Optional "spectator" receives effect regardless of distance.
		/// </summary>
		[System.Obsolete("Replaced by ServerTriggerImpactEffectForMagazinesV2")]
		public static void impact(Vector3 point, Vector3 normal, ushort id, CSteamID spectatorID, Vector3 spectatorPoint)
		{
			if (id == 0)
			{
				return;
			}

			EffectAsset asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
			ServerTriggerImpactEffectForMagazinesV2(asset, point, normal, PlayerTool.getSteamPlayer(spectatorID));
		}

		/// <summary>
		/// Server spawn effect for all players within range and instigator receives effect regardless of distance.
		/// </summary>
		public static void ServerTriggerImpactEffectForMagazinesV2(EffectAsset asset, Vector3 position, Vector3 normal, SteamPlayer instigatingClient)
		{
			if (asset == null)
				return;

			// This is weird for new code but matches old `impact` method behavior.
			position += normal * Random.Range(0.04f, 0.06f);

			TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(asset);
			triggerEffectParameters.position = position;
			triggerEffectParameters.SetDirection(normal);
			triggerEffectParameters.relevantDistance = EffectManager.SMALL;

			if (instigatingClient != null && instigatingClient.player != null && instigatingClient.player.channel != null)
			{
				triggerEffectParameters.SetRelevantTransportConnections(instigatingClient.player.channel.GatherOwnerAndClientConnectionsWithinSphere(position, EffectManager.SMALL));
			}

			EffectManager.triggerEffect(triggerEffectParameters);
		}

#if !DEDICATED_SERVER
		/// <summary>
		/// parent should only be set if that system also calls ClearAttachments, otherwise attachedEffects will leak memory.
		/// </summary>
		internal static void LocalSpawnBulletImpactEffect(Vector3 position, Vector3 normal, string materialName, Transform parent)
		{
			AssetReference<EffectAsset> assetRef = PhysicMaterialCustomData.WipDoNotUseTemp_GetBulletImpactEffect(materialName);
			EffectAsset asset = assetRef.Find();
			if (asset != null)
			{
				EffectManager.internalSpawnEffect(asset, position, normal, Vector3.one, false, parent);
			}
		}

		private static void PlayBulletImpactAudio(Vector3 position, string materialName, bool wasInstigatedByLocalPlayer)
		{
			OneShotAudioDefinition audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, "BulletImpact");
			if (audioDef == null)
				return;

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return;

			OneShotAudioParameters parameters = new OneShotAudioParameters(position, audioClip);
			parameters.volume = 0.6f * audioDef.volumeMultiplier;
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
			parameters.SetLinearRolloff(1.0f, 16.0f);
			parameters.spatialBlend = wasInstigatedByLocalPlayer ? 0.9f : 1.0f; // Our own shots are slightly 2D to provide more hit feedback.
			parameters.Play();
		}

		internal static void PlayMeleeImpactAudio(Vector3 position, string materialName)
		{
			OneShotAudioDefinition audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, "MeleeImpact");
			if (audioDef == null)
			{
				audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, "LegacyImpact");
				if (audioDef == null)
					return;
			}

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return;

			OneShotAudioParameters parameters = new OneShotAudioParameters(position, audioClip);
			parameters.volume = 0.6f * audioDef.volumeMultiplier;
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
			parameters.SetLinearRolloff(1.0f, 16.0f);
			parameters.Play();
		}

		private static void PlayLegacyImpactAudio(Vector3 position, string materialName)
		{
			OneShotAudioDefinition audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, "LegacyImpact");
			if (audioDef == null)
			{
				audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, "MeleeImpact");
				if (audioDef == null)
					return;
			}

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return;

			OneShotAudioParameters parameters = new OneShotAudioParameters(position, audioClip);
			parameters.volume = 0.6f * audioDef.volumeMultiplier;
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);
			parameters.SetLinearRolloff(1.0f, 16.0f);
			parameters.Play();
		}
#endif // !DEDICATED_SERVER

		private static ClientStaticMethod<Vector3, Vector3, string, Transform, NetId> SendSpawnBulletImpact = ClientStaticMethod<Vector3, Vector3, string, Transform, NetId>.Get(ReceiveSpawnBulletImpact);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveSpawnBulletImpact(Vector3 position, [NetPakNormal] Vector3 normal, string materialName, Transform colliderTransform, NetId instigatorNetId)
		{
#if !DEDICATED_SERVER
			bool wasInstigatedByLocalPlayer = Player.LocalPlayer != null && instigatorNetId == Player.LocalPlayer.channel.owner.GetNetId();
			LocalSpawnBulletImpactEffect(position, normal, materialName, colliderTransform);
			PlayBulletImpactAudio(position, materialName, wasInstigatedByLocalPlayer);
#if WITH_BULLET_IMPACT_GIZMOS
			RuntimeGizmos.Get().Arrow(position, normal, 1.0f, Color.red, lifespan: 1.0f, layer: EGizmoLayer.Foreground);
#endif // WITH_BULLET_IMPACT_GIZMOS
#endif // !DEDICATED_SERVER
		}

		internal static void ServerSpawnBulletImpact(Vector3 position, Vector3 normal, string materialName, Transform colliderTransform, SteamPlayer instigatingClient, List<ITransportConnection> transportConnections)
		{
			// Old code offsets position as well.
			position += normal * Random.Range(0.04f, 0.06f);

			NetId instigatorNetId = instigatingClient?.GetNetId() ?? NetId.INVALID;
			SendSpawnBulletImpact.Invoke(ENetReliability.Unreliable, transportConnections, position, normal, materialName, colliderTransform, instigatorNetId);
		}

		private static ClientStaticMethod<Vector3, Vector3, string, Transform> SendSpawnLegacyImpact = ClientStaticMethod<Vector3, Vector3, string, Transform>.Get(ReceiveSpawnLegacyImpact);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveSpawnLegacyImpact(Vector3 position, [NetPakNormal] Vector3 normal, string materialName, Transform colliderTransform)
		{
#if !DEDICATED_SERVER
			LocalSpawnBulletImpactEffect(position, normal, materialName, colliderTransform);
			PlayLegacyImpactAudio(position, materialName);
#endif // !DEDICATED_SERVER
		}

		internal static void ServerSpawnLegacyImpact(Vector3 position, Vector3 normal, string materialName, Transform colliderTransform, List<ITransportConnection> transportConnections)
		{
			// Old code offsets position as well.
			position += normal * Random.Range(0.04f, 0.06f);

			SendSpawnLegacyImpact.Invoke(ENetReliability.Unreliable, transportConnections, position, normal, materialName, colliderTransform);
		}

		public static RaycastInfo raycast(Ray ray, float range, int mask)
		{
			return raycast(ray, range, mask, ignorePlayer: null);
		}

		public static RaycastInfo raycast(Ray ray, float range, int mask, Player ignorePlayer = null)
		{
			RaycastHit hit;
			Physics.Raycast(ray, out hit, range, mask);

			RaycastInfo info = new RaycastInfo(hit);
			info.direction = ray.direction;
			info.limb = ELimb.SPINE; // Default before getLimb was moved into Enemy/Zombie/Animal branches.

			if (info.transform != null)
			{
				if (info.transform.CompareTag("Barricade"))
				{
					info.transform = getBarricadeRootTransform(info.transform);
				}
				else if (info.transform.CompareTag("Structure"))
				{
					info.transform = getStructureRootTransform(info.transform);
				}
				else if (info.transform.CompareTag("Resource"))
				{
					info.transform = getResourceRootTransform(info.transform);
				}
				else if (info.transform.CompareTag("Enemy"))
				{
					info.player = getPlayer(info.transform);
					if (info.player == ignorePlayer)
					{
						// Todo: in the future it would be nice to do another raycast.
						info.player = null;
					}
					info.limb = getLimb(info.transform);
				}
				else if (info.transform.CompareTag("Zombie"))
				{
					info.zombie = getZombie(info.transform);
					info.limb = getLimb(info.transform);
				}
				else if (info.transform.CompareTag("Animal"))
				{
					info.animal = getAnimal(info.transform);
					info.limb = getLimb(info.transform);
				}
				else if (info.transform.CompareTag("Vehicle"))
				{
					info.vehicle = getVehicle(info.transform);
				}

				if (info.zombie != null && info.zombie.isRadioactive)
				{
					info.materialName = "Alien_Dynamic";
#pragma warning disable
					info.material = EPhysicsMaterial.ALIEN_DYNAMIC;
#pragma warning restore
				}
				else
				{
					info.materialName = PhysicsTool.GetMaterialName(hit);
#pragma warning disable
					info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
				}
			}

			return info;
		}

		public delegate void PlayerAllowedToDamagePlayerHandler(Player instigator, Player victim, ref bool isAllowed);

		public static event PlayerAllowedToDamagePlayerHandler onPlayerAllowedToDamagePlayer;

		public static bool isPlayerAllowedToDamagePlayer(Player instigator, Player victim)
		{
			bool isAllowed;
			if (Provider.isPvP)
			{
				if (Provider.modeConfigData.Gameplay.Friendly_Fire)
				{
					isAllowed = true;
				}
				else
				{
					// Friendly fire is disabled, so we test group affiliation.
					isAllowed = instigator.quests.isMemberOfSameGroupAs(victim) == false;
				}
			}
			else
			{
				// PvE server, never allowed to damage each other.
				isAllowed = false;
			}

			if (!instigator.movement.canAddSimulationResultsToUpdates)
			{
				// Prevent damage while in "vanish" mode.
				isAllowed = false;
			}

			if (onPlayerAllowedToDamagePlayer != null)
			{
				try
				{
					onPlayerAllowedToDamagePlayer(instigator, victim, ref isAllowed);
				}
				catch (System.Exception e)
				{
					UnturnedLog.warn("Plugin raised an exception from onPlayerAllowedToDamagePlayer:");
					UnturnedLog.exception(e);
				}
			}

			return isAllowed;
		}

		internal static readonly AssetReference<EffectAsset> FleshDynamicRef = new AssetReference<EffectAsset>("cea791255ba74b43a20e511a52ebcbec"); // Old fleshy punch impact (5)
		internal static readonly AssetReference<EffectAsset> AlienDynamicRef = new AssetReference<EffectAsset>("67a4addd45174d7e9ca5c8ec24f8010f"); // Radioactive zombie punch impact (95)
	}
}
