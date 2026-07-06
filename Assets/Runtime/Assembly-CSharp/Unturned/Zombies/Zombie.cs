////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public enum EHuntType
	{
		PLAYER,
		POINT
	}

	/// <summary>
	/// Used when damaging zombies to override in which situations they are stunned.
	/// </summary>
	public enum EZombieStunOverride
	{
		/// <summary>
		/// Default stun behaviour determined by damage dealt.
		/// </summary>
		None,
		/// <summary>
		/// Don't stun even if damage is over threshold.
		/// </summary>
		Never,
		/// <summary>
		/// Stun regardless of damage.
		/// </summary>
		Always
	}

	public class Zombie : MonoBehaviour, IExplosionDamageable
	{
		private static List<RegionCoordinate> regionsInRadius = new List<RegionCoordinate>(4);
		private static List<Transform> structuresInRadius = new List<Transform>();
		private static List<InteractableVehicle> vehiclesInRadius = new List<InteractableVehicle>();
		private static List<Transform> barricadesInRadius = new List<Transform>();
		private static List<Transform> objectsInRadius = new List<Transform>();

		private static readonly float ATTACK_BARRICADE = 16;
		private static readonly float ATTACK_VEHICLE = 16;
		private static readonly float ATTACK_PLAYER = 2;

		private Transform skeleton;
		private Transform rightHook;
		private SkinnedMeshRenderer renderer_0;
		private SkinnedMeshRenderer renderer_1;
		private Transform eyes;
		private Transform radiation;
		private Transform burner;
		private Transform acid;
		private Transform acidNuclear;
		private Transform electric;
		private ParticleSystem sparkSystem;
		private ParticleSystem fireSystem;
		private AudioSource fireAudio;

		private Material skinMaterial;
		private Transform attachmentModel_0;
		private Transform attachmentModel_1;
		private Material attachmentMaterial_0;
		private Material attachmentMaterial_1;

		public ushort id;
		public byte bound;
		public byte type;
		public EZombieSpeciality speciality;
		public byte shirt;
		public byte pants;
		public byte hat;
		public byte gear;

		/// <summary>
		/// Overrides hat item from zombie table with a specific item ID.
		/// </summary>
		private ushort hatID => speciality.IsDLVolatile() ? (ushort) 960 : (ushort) 0;

		/// <summary>
		/// Overrides gear item from zombie table with a specific item ID.
		/// </summary>
		private ushort gearID => speciality.IsDLVolatile() ? (ushort) 961 : (ushort) 0;

		private byte _move;
		public byte move
		{
			get => _move;

			set
			{
				_move = value;
				moveAnim = "Move_" + move;
			}
		}
		private string moveAnim;

		private byte _idle;
		public byte idle
		{
			get => _idle;

			set
			{
				_idle = value;
				idleAnim = "Idle_" + idle;
			}
		}

		public string idleAnim;
		public bool isUpdated;

		private IUnturnedPathfindingMovementComponentInterface seeker;
		private Player player;

		/// <summary>
		/// If zombie is stuck this was a nearby barricade potentially blocking our path.
		/// </summary>
		private Transform targetBarricade;

		/// <summary>
		/// If zombie is stuck this was a nearby structure potentially blocking our path.
		/// </summary>
		private Transform targetStructure;

		/// <summary>
		/// If zombie is stuck this was a nearby vehicle potentially blocking our path.
		/// </summary>
		private InteractableVehicle targetObstructionVehicle;

		/// <summary>
		/// If target player is passenger in a vehicle this is their vehicle.
		/// </summary>
		private InteractableVehicle targetPassengerVehicle;

		/// <summary>
		/// If zombie is stuck this was a nearby object potentially blocking our path.
		/// </summary>
		private InteractableObjectRubble targetObject;

		private Transform target;
		private Animation animator;

		private float lastHunted;
		private float lastTarget;
		private float lastLeave;
		private float lastSpecial;
		private float lastAttack;
		private float lastStartle;
		private float lastStun;
		private float lastGroan;
		private float lastRegen;
		private float lastStuck;
		/// <summary>
		/// Incremented while stuck. Prevents doing overlap test too frequently.
		/// </summary>
		private float stuckSearchTimer;

		private Vector3 cameFrom;
		private bool isPulled;
		private float lastPull;
		private float pullDelay;

		private float groanDelay;

		private float leaveTime;
		private float throwTime;
		private float boulderTime;
		private float spitTime;
		private float acidTime;
		private float chargeTime;
		private float sparkTime;
		private float windTime;
		private float fireTime;
		private float attackTime;
		private float startleTime;
		private float stunTime;

		private bool isThrowingBoulder;
		private bool isSpittingAcid;
		private bool isChargingSpark;
		private bool isStompingWind;
		private bool isBreathingFire;
		private bool isPlayingBoulder;
		private bool isPlayingSpit;
		private bool isPlayingCharge;
		private bool isPlayingWind;
		private bool isPlayingFire;
		private bool isPlayingAttack;
		private bool isPlayingStartle;
		private bool isPlayingStun;

		private Vector3 lastUpdatedPos;
		private float lastUpdatedAngle;
		private Vector3 interpPositionTarget;
		private float interpYawTarget;
		private bool isMoving;
		private bool isAttacking;
		private bool isVisible;

		private bool isWandering;
		private bool isTicking;

		#region IExplosionDamageable
		public bool Equals(IExplosionDamageable obj)
		{
			return ReferenceEquals(this, obj);
		}

		public bool IsEligibleForExplosionDamage
		{
			get => !isDead;
		}

		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
		{
			return CollisionUtil.ClosestPoint(gameObject, explosionCenter, false, DamageTool.EXPLOSION_CLOSEST_POINT_LAYER_MASK);
		}

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
		{
			if (explosionParameters.damageType == EExplosionDamageType.ZOMBIE_FIRE)
			{
				if (speciality == EZombieSpeciality.NORMAL)
				{
					ZombieManager.sendZombieSpeciality(this, EZombieSpeciality.BURNER);
				}
				return;
			}

			if (!damageParameters.shouldAffectZombies)
			{
				return;
			}

			Vector3 offset = damageParameters.closestPoint - explosionParameters.point;
			float range = offset.magnitude;
			if (range > explosionParameters.damageRadius)
			{
				return;
			}

			Vector3 normal = offset / range;
			if (damageParameters.LineOfSightTest(explosionParameters.point, normal, range, out RaycastHit block))
			{
				if (block.transform != null && !block.transform.IsChildOf(transform))
				{
					return;
				}
			}

			if (explosionParameters.playImpactEffect)
			{
				EffectAsset fleshEffect = isRadioactive ? DamageTool.AlienDynamicRef.Find() : DamageTool.FleshDynamicRef.Find();
				if (fleshEffect != null)
				{
					TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(fleshEffect);
					triggerEffectParameters.relevantDistance = EffectManager.SMALL;
					triggerEffectParameters.position = transform.position + Vector3.up;
					triggerEffectParameters.reliable = true;
					EffectManager.triggerEffect(triggerEffectParameters);

					// Spawn a second time pointing towards the damage.
					triggerEffectParameters.SetDirection(-normal);
					EffectManager.triggerEffect(triggerEffectParameters);
				}
			}

			float times = 1.0f - (range / explosionParameters.damageRadius);
			float armorMultiplier = DamageTool.GetZombieExplosionArmor(this);
			times *= armorMultiplier;

			DamageTool.damage(this, normal, explosionParameters.zombieDamage, times, out EPlayerKill kill, out uint xp,
				ragdollEffect: explosionParameters.ragdollEffect);

			if (kill != EPlayerKill.NONE)
			{
				damageParameters.kills.Add(kill);
			}
			damageParameters.xp += xp;
		}
		#endregion IExplosionDamageable

		/// <summary>
		/// Add or remove from ticking list if needed.
		/// Separated from updateTicking in order to move once after first spawn.
		/// </summary>
		private void setTicking(bool wantsToTick)
		{
			if (wantsToTick)
			{
				if (isTicking == false)
				{
					isTicking = true;
					ZombieManager.tickingZombies.Add(this);
					lastTick = Time.time;
				}
			}
			else
			{
				if (isTicking)
				{
					isTicking = false;
					ZombieManager.tickingZombies.RemoveFast(this);
				}
			}
		}

		private bool _isHunting;
		public bool isHunting
		{
			get => _isHunting;

			set
			{
				if (value != isHunting)
				{
					_isHunting = value;

					if (isHunting)
					{
						needsTickForPlacement = false; // Ticking for real purpose, so do not do the placement return.
						setTicking(true);

						if (speciality == EZombieSpeciality.FLANKER_FRIENDLY)
						{
							ZombieManager.sendZombieSpeciality(this, EZombieSpeciality.FLANKER_STALK);
						}
					}
					else
					{
						if (needsTickForPlacement == false)
						{
							setTicking(false);
						}

						if (isWandering)
						{
							isWandering = false;
							ZombieManager.wanderingCount--;
						}

						if (speciality == EZombieSpeciality.FLANKER_STALK)
						{
							ZombieManager.sendZombieSpeciality(this, EZombieSpeciality.FLANKER_FRIENDLY);
						}
					}
				}
			}
		}
		private EHuntType huntType;

		private bool isLeaving;
		private bool isStunned;
		private bool isStuck;
		private Vector3 leaveTo;

		private float _lastDead;
		public float lastDead => _lastDead;

		public bool isDead;

		private ushort health;
		private ushort maxHealth;
		private Vector3 ragdoll;
		private EZombiePath path;

		public float GetHealth()
		{
			// At the time of writing health is a ushort, but ideally it should be changed to float eventually.
			return health;
		}

		public float GetMaxHealth()
		{
			// At the time of writing maxHealth is a ushort, but ideally it should be changed to float eventually.
			return maxHealth;
		}

		public bool isHyper => zombieRegion.isHyper && speciality != EZombieSpeciality.BOSS_ALL && speciality != EZombieSpeciality.BOSS_BUAK_FINAL;

		public bool isRadioactive => zombieRegion.isRadioactive;

		public bool isBoss => speciality.IsBoss();

		/// <summary>
		/// Boss zombies are considered mega as well.
		/// </summary>
		public bool isMega => speciality == EZombieSpeciality.MEGA || isBoss || speciality == EZombieSpeciality.BOSS_ALL;

		public bool isCutesy => speciality == EZombieSpeciality.SPIRIT;

		private float GetHorizontalAttackRangeSquared()
		{
			if (targetBarricade != null)
			{
				return ATTACK_BARRICADE * (isMega ? 2 : 1);
			}
			else if (targetObstructionVehicle != null)
			{
				return ATTACK_VEHICLE * (isMega ? 2 : 1);
			}
			// We also check target player's vehicle here because Kuwait boss does not target vehicles directly.
			else if (targetPassengerVehicle != null || player?.movement.getVehicle() != null)
			{
				return ATTACK_VEHICLE * (isMega ? 2 : 1);
			}
			else
			{
				return ATTACK_PLAYER * (Dedicator.IsDedicatedServer && speciality == EZombieSpeciality.NORMAL ? 0.5f : 1f) * (isMega ? 2 : 1);
			}
		}

		private float GetVerticalAttackRange()
		{
			// 2023-01-31: slightly increased non-hyper attack range from 2.0 to 2.1 to make it easier
			// for zombies (with capsule height 2.0) to attack players on their heads.
			return (isHyper ? 3.5f : 2.1f) * (isMega ? 1.5f : 1.0f);
		}

		public void tellAlive(byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
		{
			type = newType;
			speciality = (EZombieSpeciality) newSpeciality;
			shirt = newShirt;
			pants = newPants;
			hat = newHat;
			gear = newGear;

			isDead = false;
			SetCountedAsAliveInZombieRegion(true);
			SetCountedAsAliveBossInZombieRegion(isBoss);

			transform.position = newPosition;
			transform.rotation = Quaternion.Euler(0, newAngle * 2, 0);

			updateDifficulty();
			updateLife();
			apply();
			updateEffects();
			updateVisibility(speciality != EZombieSpeciality.FLANKER_STALK && speciality != EZombieSpeciality.SPIRIT && speciality != EZombieSpeciality.BOSS_SPIRIT, false);
			updateStates();

			if (Provider.isServer)
			{
				reset();
			}
		}

		public void tellDead(Vector3 newRagdoll, ERagdollEffect ragdollEffect)
		{
			isDead = true;
			SetCountedAsAliveInZombieRegion(false);
			SetCountedAsAliveBossInZombieRegion(false);
			if (zombieRegion.hasBeacon && Provider.isServer)
			{
				BeaconManager.checkBeacon(bound).despawnAlive();
			}
			_lastDead = Time.realtimeSinceStartup;

			updateLife();

			if (!Dedicator.IsDedicatedServer)
			{
				ragdoll = newRagdoll;

				Transform ragdollModel = RagdollTool.ragdollZombie(transform.position, transform.rotation, skeleton, ragdoll, type, shirt, pants, hat, gear, hatID, gearID, isMega, ragdollEffect);
				// Nelson 2024-08-12: Feels hacky to check ragdoll effect when overriding this material, but at the
				// moment all ragdoll effects override material - wasn't working on volatiles. (public issue #4635)
				if (ragdollModel != null && speciality.IsDLVolatile() && ragdollEffect == ERagdollEffect.None)
				{
					SkinnedMeshRenderer ragdollRenderer_1 = ragdollModel.Find("Model_1").GetComponent<SkinnedMeshRenderer>();
					if (ragdollRenderer_1 != null)
					{
						if (speciality == EZombieSpeciality.DL_RED_VOLATILE)
						{
							ragdollRenderer_1.sharedMaterial = Resources.Load<Material>("Characters/M_Volatile_Red");
						}
						else if (speciality == EZombieSpeciality.DL_BLUE_VOLATILE)
						{
							ragdollRenderer_1.sharedMaterial = Resources.Load<Material>("Characters/M_Volatile_Blue");
						}
					}
				}

				if (radiation != null && isRadioactive)
				{
					EffectAsset zombie_0 = Assets.find(Zombie_0_Ref);
					if (zombie_0 != null)
					{
						EffectManager.effect(zombie_0, radiation.position, Vector3.up);
					}
				}

				if (burner != null && (speciality == EZombieSpeciality.BURNER || speciality == EZombieSpeciality.BOSS_FIRE || speciality == EZombieSpeciality.BOSS_MAGMA || speciality == EZombieSpeciality.BOSS_BUAK_FIRE))
				{
					EffectAsset zombie_2 = Assets.find(Zombie_2_Ref);
					if (zombie_2 != null)
					{
						EffectManager.effect(zombie_2, burner.position, Vector3.up);
					}
				}

				if (speciality.IsDLVolatile())
				{
					PlayOneShot(ZombieManager.dl_deaths);
				}
			}

			if (Provider.isServer)
			{
				stop();
			}
		}

		[System.Obsolete]
		public void tellState(Vector3 newPosition, byte newAngle)
		{
			tellState(newPosition, newAngle * 2.0f);
		}

		public void tellState(Vector3 newPosition, float newYaw)
		{
			lastUpdatedPos = newPosition;
			lastUpdatedAngle = newYaw;

			interpPositionTarget = newPosition;
			interpYawTarget = newYaw;
		}

		public void tellSpeciality(EZombieSpeciality newSpeciality)
		{
			speciality = newSpeciality;
			SetCountedAsAliveBossInZombieRegion(!isDead && isBoss);

			updateEffects();
			updateVisibility(speciality != EZombieSpeciality.FLANKER_STALK && speciality != EZombieSpeciality.SPIRIT && speciality != EZombieSpeciality.BOSS_SPIRIT, true);
		}

		private float specialStartleDelay;
		private float specialAttackDelay;
		private float specialUseDelay;

		/// <summary>
		/// Yeah it seems kinda ugly to pollute all zombies with this code... zombie rewrite eventually please.
		/// </summary>
		private float flashbangDelay;
		private float lastFlashbang;

		private Transform boulderItem;

		public void askThrow()
		{
			if (isDead)
			{
				return;
			}

			lastSpecial = Time.time;
			isThrowingBoulder = true;
			isPlayingBoulder = true;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Boulder_0");

#if !DEDICATED_SERVER
				AudioClip roarClip = ZombieManager.roars[Random.Range(0, 16)];
				OneShotAudioParameters roarParams = new OneShotAudioParameters(transform, roarClip);
				roarParams.volume = 0.5f;
				roarParams.pitch = GetRandomPitch();
				roarParams.SetLinearRolloff(1.0f, 32.0f);
				roarParams.Play();
#endif // !DEDICATED_SERVER
			}

			InstantiateParameters instantiateParameters = new InstantiateParameters()
			{
				parent = rightHook,
				worldSpace = false,
			};
			boulderItem = Instantiate(Resources.Load<GameObject>("Characters/Mega_Boulder_Item"), Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 90.0f), instantiateParameters).transform;
			boulderItem.name = "Boulder";
			boulderItem.localScale = Vector3.one;
			Destroy(boulderItem.gameObject, 2.0f);
		}

		private void StopThrowingBoulder()
		{
			if (!isThrowingBoulder)
				return;

			isThrowingBoulder = false;

			if (boulderItem != null)
			{
				Destroy(boulderItem.gameObject);
			}

			if (Provider.isServer)
			{
				seeker.CanMove = true;
			}
		}

		public void askBoulder(Vector3 origin, Vector3 direction)
		{
			if (isDead)
			{
				return;
			}

			Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(Random.Range(0, 2) * 180.0f, Random.Range(0, 2) * 180.0f, Random.Range(0, 2) * 180.0f);
			Transform boulderProjectile = Instantiate(Resources.Load<GameObject>(Dedicator.IsDedicatedServer ? "Characters/Mega_Boulder_Projectile_Server" : "Characters/Mega_Boulder_Projectile_Client"), origin, rotation).transform;
			boulderProjectile.name = "Boulder";
			EffectManager.RegisterDebris(boulderProjectile.gameObject);
			boulderProjectile.localScale = Vector3.one * 1.75f;
			boulderProjectile.GetComponent<Rigidbody>().AddForce(direction * 1500);
			boulderProjectile.GetComponent<Rigidbody>().AddRelativeTorque(Random.Range(-500.0f, 500.0f), Random.Range(-500.0f, 500.0f), Random.Range(-500.0f, 500.0f), ForceMode.Force);
			boulderProjectile.Find("Trap").gameObject.AddComponent<Boulder>();
			Destroy(boulderProjectile.gameObject, 8.0f);
		}

		public void askSpit()
		{
			if (isDead)
			{
				return;
			}

			lastSpecial = Time.time;
			isSpittingAcid = true;
			isPlayingSpit = true;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Acid_0");
			}
		}

		private void StopSpittingAcid()
		{
			if (!isSpittingAcid)
				return;

			isSpittingAcid = false;

			if (Provider.isServer)
			{
				seeker.CanMove = true;
			}
		}

		public void askAcid(Vector3 origin, Vector3 direction)
		{
			if (isDead)
			{
				return;
			}

			if (!Dedicator.IsDedicatedServer)
			{
				PlayOneShot(ZombieManager.spits);
			}

			Quaternion rotation = Quaternion.LookRotation(direction);
			Transform acidProjectile = Instantiate(Resources.Load<GameObject>(Dedicator.IsDedicatedServer ? "Characters/Acid_Projectile_Server" : (speciality == EZombieSpeciality.BOSS_NUCLEAR ? "Characters/Acid_Projectile_Client_Nuclear" : "Characters/Acid_Projectile_Client")), origin, rotation).transform;
			acidProjectile.name = "Acid";
			EffectManager.RegisterDebris(acidProjectile.gameObject);
			acidProjectile.GetComponent<Rigidbody>().AddForce(direction * 1000);
			acidProjectile.Find("Trap").gameObject.AddComponent<Acid>().effectGuid = (speciality == EZombieSpeciality.BOSS_NUCLEAR ? Zombie_7_Ref : Zombie_3_Ref).GUID;
			Destroy(acidProjectile.gameObject, 8.0f);
		}

		public void askCharge()
		{
			if (isDead)
			{
				return;
			}

			lastSpecial = Time.time;
			isChargingSpark = true;
			isPlayingCharge = true;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Electric_0");

				if (sparkSystem != null)
				{
					sparkSystem.Play();
				}
			}
		}

		private void StopChargingSpark()
		{
			if (!isChargingSpark)
				return;

			isChargingSpark = false;

			if (Provider.isServer)
			{
				seeker.CanMove = true;
			}
		}

		public void askSpark(Vector3 target)
		{
			if (isDead)
			{
				return;
			}

			Vector3 offset = target - sparkSystem.transform.position;
			Vector3 normal = offset.normalized;

			EffectAsset zombie_4 = Assets.find(Zombie_4_Ref);
			if (zombie_4 != null)
			{
				Transform effect = EffectManager.effect(zombie_4, sparkSystem.transform.position + (normal * 2.0f), normal);
				if (effect != null)
				{
					ParticleSystem.MainModule main = effect.GetComponent<ParticleSystem>().main;
					main.startLifetime = (offset.magnitude - 2.0f) / 128.0f;
				}
			}

			EffectAsset zombie_6 = Assets.find(Zombie_6_Ref);
			if (zombie_6 != null)
			{
				EffectManager.effect(zombie_6, target, -normal);
			}
		}

		public void askStomp()
		{
			if (isDead)
			{
				return;
			}

			lastSpecial = Time.time;
			isStompingWind = true;
			isPlayingWind = true;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Wind_0");

				EffectAsset zombie_5 = Assets.find(Zombie_5_Ref);
				if (zombie_5 != null)
				{
					EffectManager.effect(zombie_5, transform.position, Vector3.up);
				}
			}
		}

		private void StopStompingWind()
		{
			if (!isStompingWind)
				return;

			isStompingWind = false;

			if (Provider.isServer)
			{
				seeker.CanMove = true;
			}
		}

		private float fireDamage;

		public void askBreath()
		{
			if (isDead)
			{
				return;
			}

			lastSpecial = Time.time;
			isBreathingFire = true;
			isPlayingFire = true;
			fireDamage = 0.0f;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Fire_0");

				if (fireSystem != null)
				{
					ParticleSystem.EmissionModule module = fireSystem.emission;
					module.enabled = true;
					fireSystem.Play();
				}

				if (fireAudio != null)
				{
					fireAudio.pitch = Random.Range(0.95f, 1.05f);
					fireAudio.Play();
				}
			}
		}

		private void StopBreathingFire()
		{
			if (!isBreathingFire)
				return;

			isBreathingFire = false;

			if (fireSystem != null)
			{
				ParticleSystem.EmissionModule module = fireSystem.emission;
				module.enabled = false;
			}

			if (Provider.isServer)
			{
				seeker.CanMove = true;
			}
		}

		public void askAttack(byte id)
		{
			if (isDead)
			{
				return;
			}

			lastAttack = Time.time;
			specialAttackDelay = Random.Range(2.0f, 4.0f);
			isPlayingAttack = true;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Attack_" + id);

				AudioClip[] clips = speciality.IsDLVolatile() ? ZombieManager.dl_attacks : ZombieManager.roars;
				PlayOneShot(clips);
			}

			if (speciality == EZombieSpeciality.FLANKER_FRIENDLY || speciality == EZombieSpeciality.FLANKER_STALK)
			{
				updateVisibility(true, true);
			}
		}

		public void askStartle(byte id)
		{
			if (isDead)
			{
				return;
			}

			lastStartle = Time.time;
			specialStartleDelay = Random.Range(1.0f, 2.0f);
			isPlayingStartle = true;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Startle_" + id);

				AudioClip[] clips = speciality.IsDLVolatile() ? ZombieManager.dl_enemy_spotted : ZombieManager.roars;
				PlayOneShot(clips);
			}
		}

		public void askStun(byte id)
		{
			if (isDead)
			{
				return;
			}

			lastStun = Time.time;
			isPlayingStun = true;

			if (!Dedicator.IsDedicatedServer)
			{
				animator.Play("Stun_" + id);
			}
		}

		/// <summary>
		/// If damage exceeds this value, stun the zombie.
		/// </summary>
		public int getStunDamageThreshold()
		{
			if (isMega)
			{
				int overrideValue = difficulty != null ? difficulty.Mega_Stun_Threshold : -1;
				if (overrideValue > 0)
				{
					return overrideValue;
				}
				else
				{
					return 150;
				}
			}
			else
			{
				int overrideValue = difficulty != null ? difficulty.Normal_Stun_Threshold : -1;
				if (overrideValue > 0)
				{
					return overrideValue;
				}
				else
				{
					return 20;
				}
			}
		}

		/// <summary>
		/// Used to kill night-only zombies at dawn.
		/// </summary>
		public void killWithFireExplosion()
		{
			if (isDead)
				return;

			EPlayerKill kill;
			uint xp;
			DamageTool.damageZombie(DamageZombieParameters.makeInstakill(this), out kill, out xp);

			if (!isDead)
				return; // Somehow survived?

			if (burner != null)
			{
				EffectAsset zombie_2 = Assets.find(Zombie_2_Ref);
				if (zombie_2 != null)
				{
					TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(zombie_2);
					triggerEffectParameters.position = burner.position;
					triggerEffectParameters.relevantDistance = EffectManager.MEDIUM;
					triggerEffectParameters.reliable = true;
					EffectManager.triggerEffect(triggerEffectParameters);
				}

				List<EPlayerKill> kills;
				DamageTool.explode(transform.position + new Vector3(0.0f, 0.25f, 0.0f), 4.0f, EDeathCause.BURNER, Steamworks.CSteamID.Nil, 40.0f, 0.0f, 40.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, out kills, EExplosionDamageType.ZOMBIE_FIRE, 4.0f, damageOrigin: EDamageOrigin.Flamable_Zombie_Explosion);
			}
		}

		public void askDamage(ushort amount, Vector3 newRagdoll, out EPlayerKill kill, out uint xp, bool trackKill = true, bool dropLoot = true, EZombieStunOverride stunOverride = EZombieStunOverride.None, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			kill = EPlayerKill.NONE;
			xp = 0;

			if (amount == 0 || isDead)
			{
				return;
			}

			if (!isDead)
			{
				if (zombieRegion.hasBeacon)
				{
					amount = MathfEx.CeilToUShort(amount / (Mathf.Max(1, BeaconManager.checkBeacon(bound).initialParticipants) * 1.5f));
				}

				if (amount >= health)
				{
					health = 0;
				}
				else
				{
					health -= amount;
				}

				ragdoll = newRagdoll;

				if (health == 0)
				{
					if (isMega)
					{
						kill = EPlayerKill.MEGA;
					}
					else
					{
						kill = EPlayerKill.ZOMBIE;
					}
					xp = LevelZombies.tables[type].xp;

					if (zombieRegion.hasBeacon)
					{
						xp = (uint) (xp * Provider.modeConfigData.Zombies.Beacon_Experience_Multiplier);
					}
					else
					{
						if (LightingManager.isFullMoon)
						{
							xp = (uint) (xp * Provider.modeConfigData.Zombies.Full_Moon_Experience_Multiplier);
						}

						if (dropLoot)
						{
							ZombieManager.dropLoot(this);
						}
					}

					ZombieManager.sendZombieDead(this, ragdoll, newRagdollEffect: ragdollEffect);

					if (isRadioactive)
					{
						List<EPlayerKill> kills;
						DamageTool.explode(transform.position + new Vector3(0.0f, 0.25f, 0.0f), 2.0f, EDeathCause.ACID, Steamworks.CSteamID.Nil, 20.0f, 0.0f, 20.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, out kills, EExplosionDamageType.ZOMBIE_ACID, 2.0f, damageOrigin: EDamageOrigin.Radioactive_Zombie_Explosion);
					}

					if (speciality == EZombieSpeciality.BURNER || speciality == EZombieSpeciality.BOSS_FIRE || speciality == EZombieSpeciality.BOSS_MAGMA || speciality == EZombieSpeciality.BOSS_BUAK_FIRE)
					{
						List<EPlayerKill> kills;
						DamageTool.explode(transform.position + new Vector3(0.0f, 0.25f, 0.0f), 4.0f, EDeathCause.BURNER, Steamworks.CSteamID.Nil, 40.0f, 0.0f, 40.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, out kills, EExplosionDamageType.ZOMBIE_FIRE, 4.0f, damageOrigin: EDamageOrigin.Flamable_Zombie_Explosion);
					}

					if (trackKill)
					{
						for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
						{
							SteamPlayer player = Provider.clients[playerIndex];

							if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
							{
								continue;
							}

							if (player.player.movement.bound == bound || player.player.movement.bound == byte.MaxValue)
							{
								player.player.quests.trackZombieKill(this);
							}
						}
					}
				}
				else
				{
					if (Provider.modeConfigData.Zombies.Can_Stun)
					{
						// When Only_Critical_Stuns is enabled the calling code will set stunOverride appropriately.
						if (stunOverride == EZombieStunOverride.None && Provider.modeConfigData.Zombies.Only_Critical_Stuns == false)
						{
							if (amount > getStunDamageThreshold())
							{
								stun();
							}
						}
						else if (stunOverride == EZombieStunOverride.Always)
						{
							stun();
						}
					}
				}

				lastRegen = Time.time;
			}
		}

		public void sendRevive(byte type, byte speciality, byte shirt, byte pants, byte hat, byte gear, Vector3 position, float angle)
		{
			ZombieManager.sendZombieAlive(this, type, speciality, shirt, pants, hat, gear, position, MeasurementTool.angleToByte(angle));
		}

		public bool checkAlert(Player newPlayer)
		{
			return player != newPlayer;
		}

		public void alert(Player newPlayer)
		{
			if (isDead)
			{
				return;
			}

			if (player == newPlayer)
			{
				return;
			}

			if (player == null)
			{
				if (!isHunting)
				{
					if (!isLeaving)
					{
						if (speciality == EZombieSpeciality.CRAWLER)
						{
							float random = Random.value;

							if (random < 0.5f)
							{
								ZombieManager.sendZombieStartle(this, 3);
							}
							else
							{
								ZombieManager.sendZombieStartle(this, 6);
							}
						}
						else if (speciality == EZombieSpeciality.SPRINTER)
						{
							float random = Random.value;

							if (random < 0.5f)
							{
								ZombieManager.sendZombieStartle(this, 4);
							}
							else
							{
								ZombieManager.sendZombieStartle(this, 5);
							}
						}
						else
						{
							ZombieManager.sendZombieStartle(this, (byte) Random.Range(0, 3));
						}
					}
				}

				isHunting = true;
				huntType = EHuntType.PLAYER;
				isPulled = true;
				lastPull = Time.time;

				if (isWandering)
				{
					isWandering = false;
					ZombieManager.wanderingCount--;
				}

				isLeaving = false;
				isMoving = false;
				isStuck = false;

				lastHunted = Time.time;
				lastStuck = Time.time;
				stuckSearchTimer = 0.0f;

				player = newPlayer;
				target.position = player.transform.position;
				seeker.CanSearch = true;
				seeker.CanMove = true;

				if (isMega)
				{
					path = EZombiePath.RUSH;
				}
				else if (speciality == EZombieSpeciality.FLANKER_FRIENDLY || speciality == EZombieSpeciality.FLANKER_STALK)
				{
					if (Random.value < 0.5)
					{
						path = EZombiePath.LEFT_FLANK;
					}
					else
					{
						path = EZombiePath.RIGHT_FLANK;
					}
				}
				else
				{
					int attention = player.agro % 3;

					if (attention == 0)
					{
						path = EZombiePath.RUSH;
					}
					else
					{
						if (Random.value < 0.5)
						{
							path = EZombiePath.LEFT;
						}
						else
						{
							path = EZombiePath.RIGHT;
						}
					}
				}

				player.agro++;
			}
			else
			{
				if ((newPlayer.transform.position - transform.position).sqrMagnitude < (player.transform.position - transform.position).sqrMagnitude)
				{
					player.agro--;

					player = newPlayer;
					target.position = player.transform.position;

					if (isMega)
					{
						path = EZombiePath.RUSH;
					}
					else
					{
						int attention = player.agro % 3;

						if (attention == 0)
						{
							path = EZombiePath.RUSH;
						}
						else
						{
							if (Random.value < 0.5)
							{
								path = EZombiePath.LEFT;
							}
							else
							{
								path = EZombiePath.RIGHT;
							}
						}
					}

					player.agro++;
				}
			}
		}

		public void alert(Vector3 newPosition, bool isStartling)
		{
			if (isDead)
			{
				return;
			}

			if (player == null)
			{
				if (!isHunting)
				{
					if (isStartling)
					{
						if (speciality == EZombieSpeciality.CRAWLER)
						{
							float random = Random.value;

							if (random < 0.5f)
							{
								ZombieManager.sendZombieStartle(this, 3);
							}
							else
							{
								ZombieManager.sendZombieStartle(this, 6);
							}
						}
						else if (speciality == EZombieSpeciality.SPRINTER)
						{
							float random = Random.value;

							if (random < 0.5f)
							{
								ZombieManager.sendZombieStartle(this, 4);
							}
							else
							{
								ZombieManager.sendZombieStartle(this, 5);
							}
						}
						else
						{
							ZombieManager.sendZombieStartle(this, (byte) Random.Range(0, 3));
						}

						isPulled = true;
						lastPull = Time.time;

						if (isWandering)
						{
							isWandering = false;
							ZombieManager.wanderingCount--;
						}
					}

					isHunting = true;
					huntType = EHuntType.POINT;

					isLeaving = false;
					isMoving = false;
					isStuck = false;

					lastHunted = Time.time;
					lastStuck = Time.time;
					stuckSearchTimer = 0.0f;

					target.position = newPosition;
					seeker.CanSearch = true;
					seeker.CanMove = true;
				}
				else
				{
					if ((newPosition - transform.position).sqrMagnitude < (target.position - transform.position).sqrMagnitude)
					{
						target.position = newPosition;
					}
				}
			}
		}

		public void updateStates()
		{
			lastUpdatedPos = transform.position;
			lastUpdatedAngle = transform.rotation.eulerAngles.y;

			interpPositionTarget = lastUpdatedPos;
			interpYawTarget = lastUpdatedAngle;
		}

		private void stop()
		{
			isMoving = false;
			isAttacking = false;
			isHunting = false;
			isStuck = false;

			lastStuck = Time.time;
			stuckSearchTimer = 0.0f;

			if (player != null)
			{
				player.agro--;
			}

			player = null;
			targetBarricade = null;
			targetStructure = null;
			targetObstructionVehicle = null;
			targetPassengerVehicle = null;
			targetObject = null;
			seeker.CanSearch = false;
			seeker.CanMove = false;

			target.position = transform.position;
		}

		private void stun()
		{
			isStunned = true;

			isAttacking = false; // Cancel in-progress attack damage.
			StopBreathingFire();
			StopThrowingBoulder();
			StopSpittingAcid();
			StopStompingWind();
			StopChargingSpark();

			isMoving = false;
			seeker.CanMove = false;

			if (speciality == EZombieSpeciality.CRAWLER)
			{
				float random = Random.value;

				if (random < 0.33f)
				{
					ZombieManager.sendZombieStun(this, 5);
				}
				else if (random < 0.66f)
				{
					ZombieManager.sendZombieStun(this, 7);
				}
				else
				{
					ZombieManager.sendZombieStun(this, 8);
				}
			}
			else if (speciality == EZombieSpeciality.SPRINTER)
			{
				float random = Random.value;

				if (random < 0.33f)
				{
					ZombieManager.sendZombieStun(this, 6);
				}
				else if (random < 0.66f)
				{
					ZombieManager.sendZombieStun(this, 9);
				}
				else
				{
					ZombieManager.sendZombieStun(this, 10);
				}
			}
			else
			{
				ZombieManager.sendZombieStun(this, (byte) Random.Range(0, 5));
			}
		}

		private void leave(bool quick)
		{
			isLeaving = true;
			lastLeave = Time.time;

			if (quick)
			{
				leaveTime = Random.Range(0.5f, 1.0f);
			}
			else
			{
				leaveTime = Random.Range(3f, 6f);
			}

			leaveTo = transform.position - (16 * (target.position - transform.position).normalized) + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));

			if (!LevelNavigation.checkNavigation(leaveTo))
			{
				leaveTo = transform.position + (16 * (target.position - transform.position).normalized) + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
			}

			if (!LevelNavigation.checkNavigation(leaveTo))
			{
				leaveTo = transform.position;
			}

			stop();
		}

		private void updateEffects()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (burner != null)
			{
				burner.gameObject.SetActive(speciality == EZombieSpeciality.BURNER || speciality == EZombieSpeciality.BOSS_FIRE || speciality == EZombieSpeciality.BOSS_MAGMA || speciality == EZombieSpeciality.BOSS_BUAK_FIRE);
			}

			if (acid != null)
			{
				acid.gameObject.SetActive(speciality == EZombieSpeciality.ACID);
			}

			if (acidNuclear != null)
			{
				acidNuclear.gameObject.SetActive(speciality == EZombieSpeciality.BOSS_NUCLEAR);
			}

			if (electric != null)
			{
				electric.gameObject.SetActive(speciality == EZombieSpeciality.BOSS_ELECTRIC || speciality == EZombieSpeciality.BOSS_BUAK_ELECTRIC);
			}

			if (fireSystem != null)
			{
				ParticleSystem.EmissionModule module = fireSystem.emission;
				module.enabled = false;

				fireSystem.gameObject.SetActive(speciality == EZombieSpeciality.BOSS_FIRE || speciality == EZombieSpeciality.BOSS_MAGMA || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FIRE || speciality == EZombieSpeciality.BOSS_BUAK_FINAL);
			}

			if (sparkSystem != null)
			{
				sparkSystem.gameObject.SetActive(speciality == EZombieSpeciality.BOSS_ELECTRIC || speciality == EZombieSpeciality.BOSS_BUAK_ELECTRIC || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL);
			}
		}

		public float getBulletResistance()
		{
			switch (speciality)
			{
				case EZombieSpeciality.SPIRIT:
				case EZombieSpeciality.BOSS_SPIRIT: // Nelson 2024-06-11: Wasn't previously resistant, public issue #4489.
				case EZombieSpeciality.BOSS_ELVER_STOMPER:
					return 0.1f;

				default:
					return 1.0f;
			}
		}

		private bool hasUpdateVisibilityBeenCalledYet;
		private void updateVisibility(bool newVisible, bool playEffect)
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (hasUpdateVisibilityBeenCalledYet)
			{
				if (newVisible == isVisible)
					return;
			}
			hasUpdateVisibilityBeenCalledYet = true;
			isVisible = newVisible;

			if (isVisible)
			{
				if (attachmentModel_0 != null && attachmentMaterial_0 != null)
				{
					Material ghostMaterial;
					HighlighterTool.rematerialize(attachmentModel_0, attachmentMaterial_0, out ghostMaterial);
				}

				if (attachmentModel_1 != null && attachmentMaterial_1 != null)
				{
					Material ghostMaterial;
					HighlighterTool.rematerialize(attachmentModel_1, attachmentMaterial_1, out ghostMaterial);
				}

				if (renderer_0 != null && skinMaterial != null)
				{
					renderer_0.sharedMaterial = skinMaterial;
				}

				if (renderer_1 != null && skinMaterial != null)
				{
					renderer_1.sharedMaterial = skinMaterial;
				}

				attachmentMaterial_0 = null;
				attachmentMaterial_1 = null;
				skinMaterial = null;
			}
			else
			{
				Material newMaterial = (speciality == EZombieSpeciality.SPIRIT || speciality == EZombieSpeciality.BOSS_SPIRIT) ? ZombieClothing.ghostSpiritMaterial : ZombieClothing.ghostMaterial;

				if (attachmentModel_0 != null)
				{
					HighlighterTool.rematerialize(attachmentModel_0, newMaterial, out attachmentMaterial_0);
				}

				if (attachmentModel_1 != null)
				{
					HighlighterTool.rematerialize(attachmentModel_1, newMaterial, out attachmentMaterial_1);
				}

				if (renderer_0 != null)
				{
					skinMaterial = renderer_0.sharedMaterial;

					renderer_0.sharedMaterial = newMaterial;
				}

				if (renderer_1 != null)
				{
					if (skinMaterial == null)
					{
						skinMaterial = renderer_1.sharedMaterial;
					}

					renderer_1.sharedMaterial = newMaterial;
				}
			}

			if (playEffect)
			{
				EffectAsset zombie_1 = Assets.find(Zombie_1_Ref);
				if (zombie_1 != null)
				{
					EffectManager.effect(zombie_1, radiation.position, Vector3.up);
				}
			}
		}

		private void apply()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				ZombieClothing.EApplyFlags clothingFlags = isMega ? ZombieClothing.EApplyFlags.Mega : ZombieClothing.EApplyFlags.None;
				ZombieClothing.apply(animator.transform, clothingFlags, renderer_0, renderer_1, type, shirt, pants, hat, gear, hatID, gearID, out attachmentModel_0, out attachmentModel_1);

				Material skinOverride = null;
				switch (speciality)
				{
					case EZombieSpeciality.BOSS_MAGMA:
						skinOverride = Resources.Load<Material>("Characters/Magma_Material");
						break;

					case EZombieSpeciality.DL_RED_VOLATILE:
						skinOverride = Resources.Load<Material>("Characters/M_Volatile_Red");
						break;

					case EZombieSpeciality.DL_BLUE_VOLATILE:
						skinOverride = Resources.Load<Material>("Characters/M_Volatile_Blue");
						break;
				}

				if (skinOverride != null)
				{
					if (renderer_0 != null)
					{
						renderer_0.sharedMaterial = skinOverride;
					}
					if (renderer_1 != null)
					{
						renderer_1.sharedMaterial = skinOverride;
					}
				}
			}

			if (isMega)
			{
				if (!Dedicator.IsDedicatedServer)
				{
					animator.transform.localScale = Vector3.one * Random.Range(1.45f, 1.55f);
				}

				SetCapsuleRadiusAndHeight(0.75f, 2.0f);

				if (Provider.isServer)
				{
					seeker.Speed = 6f;
				}
			}
			else
			{
				if (!Dedicator.IsDedicatedServer)
				{
					animator.transform.localScale = Vector3.one * Random.Range(0.95f, 1.05f);
				}

				SetCapsuleRadiusAndHeight(0.4f, 2.0f);

				if (Provider.isServer)
				{
					if (speciality == EZombieSpeciality.CRAWLER)
					{
						if (Provider.modeConfigData.Zombies.Slow_Movement)
						{
							seeker.Speed = 2.5f;
						}
						else
						{
							seeker.Speed = 3.0f;// 2.75f;
						}
					}
					else if (speciality == EZombieSpeciality.SPRINTER || speciality.IsDLVolatile())
					{
						if (Provider.modeConfigData.Zombies.Slow_Movement)
						{
							seeker.Speed = 6.0f;// 6.25f;
						}
						else
						{
							seeker.Speed = 6.5f;// 6.25f;
						}
					}
					else if (speciality == EZombieSpeciality.FLANKER_FRIENDLY || speciality == EZombieSpeciality.FLANKER_STALK)
					{
						if (Provider.modeConfigData.Zombies.Slow_Movement)
						{
							seeker.Speed = 5.5f;// 6.25f;
						}
						else
						{
							seeker.Speed = 6f;// 6.25f;
						}
					}
					else
					{
						if (Provider.modeConfigData.Zombies.Slow_Movement)
						{
							seeker.Speed = 4.5f;// 4.75f;
						}
						else
						{
							seeker.Speed = 5.5f;// 4.75f;
						}
					}
				}
			}
		}

		public ZombieDifficultyAsset difficulty
		{
			get;
			private set;
		}

		/// <summary>
		/// Cache difficulty asset (if any) for this zombie's current type and bound.
		/// Allows difficulty assets to override certain zombie behaviors.
		/// Called after bound/type is initialized, and after type changes during respawn.
		/// </summary>
		private void updateDifficulty()
		{
			if (Provider.isServer == false)
				return;

			ZombieTable spawnTable = type < LevelZombies.tables.Count ? LevelZombies.tables[type] : null;
			if (spawnTable != null)
			{
				difficulty = ZombieManager.GetDifficultyInBoundForTable(bound, spawnTable, false);
			}
			else
			{
				difficulty = ZombieManager.getDifficultyInBound(bound);
			}

			// Good place to test is Cologne in the Germany map.
			// UnturnedLog.info("Zombie in bound {0} of type {1} using difficulty '{2}'", bound, type, difficulty != null ? difficulty.name : "none");
		}

		private void updateLife()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (renderer_0 != null)
				{
					renderer_0.enabled = !isDead;
				}

				if (renderer_1 != null)
				{
					renderer_1.enabled = !isDead;
				}

				skeleton.gameObject.SetActive(!isDead);

				if (eyes != null)
				{
					eyes.gameObject.SetActive(isHyper);
				}

				if (radiation != null)
				{
					radiation.gameObject.SetActive(isRadioactive);
				}
			}

			CharacterController cc = GetComponent<CharacterController>();
			if (cc != null)
			{
				cc.SetDetectCollisionsDeferred(!isDead);
			}

			GetComponent<Collider>().enabled = !isDead;
		}

		private bool needsTickForPlacement = false;

		private void reset()
		{
			target.position = transform.position;

			lastTarget = Time.time;
			lastLeave = Time.time;
			lastSpecial = Time.time;
			lastAttack = Time.time;
			lastStartle = Time.time;
			lastStun = Time.time;
			lastStuck = Time.time;
			stuckSearchTimer = 0.0f;

			cameFrom = transform.position;
			isPulled = false;
			pullDelay = Random.Range(24.0f, 96.0f); // bump this value up

			specialStartleDelay = Random.Range(1.0f, 2.0f);
			specialAttackDelay = Random.Range(2.0f, 4.0f);
			specialUseDelay = Random.Range(4.0f, 8.0f);
			flashbangDelay = 10.0f;

			isThrowingBoulder = false;
			isSpittingAcid = false;
			isChargingSpark = false;
			isStompingWind = false;
			isBreathingFire = false;
			isPlayingBoulder = false;
			isPlayingSpit = false;
			isPlayingCharge = false;
			isPlayingWind = false;
			isPlayingFire = false;
			isPlayingAttack = false;
			isPlayingStartle = false;
			isPlayingStun = false;

			isMoving = false;
			isAttacking = false;
			isHunting = false;
			isLeaving = false;
			isStunned = false;
			isStuck = false;
			leaveTo = transform.position;

			if (player != null)
			{
				player.agro--;
			}

			player = null;
			targetBarricade = null;
			targetStructure = null;
			targetObstructionVehicle = null;
			targetPassengerVehicle = null;
			targetObject = null;
			seeker.CanSearch = false;
			seeker.CanMove = false;

			health = LevelZombies.tables[type].health;

			if (speciality == EZombieSpeciality.CRAWLER || speciality.IsDLVolatile())
			{
				health = (ushort) (health * 1.5f);
			}
			else if (speciality == EZombieSpeciality.SPRINTER)
			{
				health = (ushort) (health * 0.5f);
			}
			else if (speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_MAGMA)
			{
#if UNITY_EDITOR
				health = 300;
#else
				health = 12000;
#endif
			}
			else if (speciality == EZombieSpeciality.BOSS_ELVER_STOMPER)
			{
				// ~23 hits with a 200 damage melee weapon.
				health = 4600;
			}
			else if (speciality == EZombieSpeciality.BOSS_KUWAIT)
			{
				health = 60000;
			}
			else if (speciality == EZombieSpeciality.BOSS_BUAK_WIND)
			{
				health = 6000;
			}
			else if (speciality == EZombieSpeciality.BOSS_BUAK_FIRE)
			{
				health = 6200;
			}
			else if (speciality == EZombieSpeciality.BOSS_BUAK_ELECTRIC)
			{
				health = 6400;
			}
			else if (speciality == EZombieSpeciality.BOSS_BUAK_FINAL)
			{
				health = 7000;
			}
			else if (isBoss)
			{
#if UNITY_EDITOR
				health = 200;
#else
				health = 6000;
#endif
			}

			if (difficulty != null && difficulty.SpecialityHealthOverrideMode != EZombieDifficultyHealthOverrideMode.None
				&& difficulty.SpecialityHealthOverrides != null)
			{
				if (difficulty.SpecialityHealthOverrides.TryGetValue(speciality, out float healthOverride))
				{
					switch (difficulty.SpecialityHealthOverrideMode)
					{
						case EZombieDifficultyHealthOverrideMode.MultiplyEditorHealth:
						{
							health = MathfEx.RoundAndClampToUShort(LevelZombies.tables[type].health * healthOverride);
							break;
						}

						case EZombieDifficultyHealthOverrideMode.MultiplyDefaultHealth:
						{
							health = MathfEx.RoundAndClampToUShort(health * healthOverride);
							break;
						}

						case EZombieDifficultyHealthOverrideMode.Replace:
						{
							health = MathfEx.RoundAndClampToUShort(healthOverride);
							break;
						}
					}
				}
			}

			if (Level.info.type == ELevelType.HORDE)
			{
				health += (ushort) (Mathf.Min(ZombieManager.waveIndex - 1, 20) * 10);
			}

			maxHealth = health;

			// Tick once after spawn to place on ground properly.
			needsTickForPlacement = true;
			setTicking(true);
		}

		/// <summary>
		/// Called when zombie does not have a target, but has been stuck for a period.
		/// </summary>
		private void findTargetWhileStuck()
		{
			bool canTargetStructures = Provider.modeConfigData.Zombies.Can_Target_Structures;
			bool canTargetBarricades = Provider.modeConfigData.Zombies.Can_Target_Barricades;
			bool canTargetVehicles = Provider.modeConfigData.Zombies.Can_Target_Vehicles;
			bool canTargetObjects = Provider.modeConfigData.Zombies.Can_Target_Objects;

			// Kuwait boss cannot damage vehicles because the final bossfight uses them as stationary turrets / cannons
			// which should not be destroyed.
			canTargetVehicles &= speciality != EZombieSpeciality.BOSS_KUWAIT;

			if (canTargetStructures || canTargetBarricades || canTargetObjects)
			{
				regionsInRadius.Clear();
				Regions.getRegionsInRadius(transform.position, 4.0f, regionsInRadius);
			}

			if (canTargetStructures)
			{
				structuresInRadius.Clear();
				StructureManager.getStructuresInRadius(transform.position, 16.0f, regionsInRadius, structuresInRadius);

				if (structuresInRadius.Count > 0)
				{
					foreach (Transform testStructureTransform in structuresInRadius)
					{
						StructureDrop structure = StructureDrop.FindByRootFast(testStructureTransform);
						if (structure != null && structure.asset != null && structure.asset.CanZombiesTarget)
						{
							targetStructure = testStructureTransform;
							return;
						}
					}
				}
			}

			if (canTargetVehicles)
			{
				vehiclesInRadius.Clear();
				VehicleManager.getVehiclesInRadius(transform.position, 16.0f, vehiclesInRadius);

				if (vehiclesInRadius.Count > 0 && vehiclesInRadius[0].asset != null && vehiclesInRadius[0].asset.isVulnerableToEnvironment)
				{
					targetObstructionVehicle = vehiclesInRadius[0];
					return;
				}
			}

			if (canTargetBarricades)
			{
				barricadesInRadius.Clear();
				BarricadeManager.getBarricadesInRadius(transform.position, 16.0f, regionsInRadius, barricadesInRadius);

				if (barricadesInRadius.Count > 0)
				{
					foreach (Transform testBarricadeTransform in barricadesInRadius)
					{
						BarricadeDrop barricade = BarricadeDrop.FindByRootFast(testBarricadeTransform);
						if (barricade != null && barricade.asset != null && barricade.asset.CanZombiesTarget)
						{
							targetBarricade = testBarricadeTransform;
							return;
						}
					}
				}
			}

			if (canTargetObjects)
			{
				objectsInRadius.Clear();
				ObjectManager.getObjectsInRadius(transform.position, 16.0f, regionsInRadius, objectsInRadius);

				if (objectsInRadius.Count > 0)
				{
					foreach (Transform obj in objectsInRadius)
					{
						InteractableObjectRubble rubble = obj.GetComponent<InteractableObjectRubble>();
						if (rubble != null && rubble.asset.RubbleCanZombiesDamage && !rubble.isAllDead())
						{
							targetObject = rubble;
						}
					}
				}
			}
		}

		/// <summary>
		/// Reduces frequency of UndergroundAllowlist checks because it can be expensive with lots of entities and volumes. 
		/// </summary>
		private float undergroundTestTimer = 10.0f;

		private float lastTick;
		public void tick()
		{
			if (needsTickForPlacement)
			{
				needsTickForPlacement = false;
				setTicking(false);
				GetComponent<CharacterController>().Move(Vector3.down);
				return;
			}

			float delta = Time.time - lastTick;
			lastTick = Time.time;
			lastPull = Time.time;

			//lastWander = Time.realtimeSinceStartup;

			if (isStunned)
			{
				return;
			}

			undergroundTestTimer -= delta;
			if (undergroundTestTimer < 0.0f)
			{
				undergroundTestTimer = Random.Range(30.0f, 60.0f);

				if (!UndergroundAllowlist.IsPositionWithinValidHeight(transform.position))
				{
					ZombieManager.teleportZombieBackIntoMap(this);
					return;
				}
			}

			if (huntType == EHuntType.PLAYER)
			{
				if (player == null)
				{
					stop();
					return;
				}
			}
			else if (huntType == EHuntType.POINT)
			{
				if ((!isMoving || isStuck) && Time.time - lastHunted > 3f)
				{
					stop();
					return;
				}
			}

			if (player != null)
			{
				if (player.life.isDead)
				{
					leave(false);
					return;
				}
				else if (player.movement.nav == 255 || (player.stance.stance == EPlayerStance.SWIM && !SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position)))
				{
					leave(true);
					return;
				}
			}

			if (targetObstructionVehicle != null)
			{
				if (targetObstructionVehicle.isDead)
				{
					targetObstructionVehicle = null;
				}
			}

			if (targetPassengerVehicle != null)
			{
				if (targetPassengerVehicle.isDead)
				{
					targetPassengerVehicle = null;
				}
			}

			if (targetObject != null && targetObject.isAllDead())
			{
				targetObject = null;
			}

			if (isStuck)
			{
				float timeSinceStuck = Time.time - lastStuck;
				if (timeSinceStuck > 0.75f)
				{
					stuckSearchTimer += delta;
					if (stuckSearchTimer > 0.25f)
					{
						stuckSearchTimer = 0.0f;

						if (targetBarricade == null && targetStructure == null && targetObstructionVehicle == null
							&& targetPassengerVehicle == null && targetObject == null)
						{
							findTargetWhileStuck();
						}
					}
				}
				else
				{
					stuckSearchTimer = 0.0f;
				}

				if (timeSinceStuck > 5.0f && zombieRegion.hasBeacon && Time.time - lastAttack > 10.0f)
				{
					lastStuck = Time.time;
					stuckSearchTimer = 0.0f;
					// Zombie has been stuck for a while during horde beacon which prevents it from being completed,
					// though only if not attacking recently because the zombie might be stuck while attacking buildable.
					ZombieManager.teleportZombieBackIntoMap(this);
					return;
				}
			}

			// Nelson 2024-10-16: Clear target if it's moved out of range or is inactive. There was a bug with zombies
			// continuing to attack pooled barricades. The detection radius is 4 m so 8 m should be safe for clearing.
			if (targetBarricade != null)
			{
				if (!targetBarricade.gameObject.activeInHierarchy
					|| (targetBarricade.transform.position - transform.position).sqrMagnitude > 64.0f)
				{
					targetBarricade = null;
				}
			}
			if (targetStructure != null)
			{
				if (!targetStructure.gameObject.activeInHierarchy
					|| (targetStructure.transform.position - transform.position).sqrMagnitude > 64.0f)
				{
					targetStructure = null;
				}
			}

			float sqrHorizontalDistanceFromTarget;
			float verticalDistanceFromTarget;
			if (targetBarricade != null)
			{
				sqrHorizontalDistanceFromTarget = MathfEx.HorizontalDistanceSquared(targetBarricade.position, transform.position);
				verticalDistanceFromTarget = Mathf.Abs(targetBarricade.position.y - transform.position.y);
				target.position = targetBarricade.position;

				seeker.CanTurn = false;
				seeker.TargetDirection = targetBarricade.position - transform.position;
			}
			else if (targetStructure != null)
			{
				sqrHorizontalDistanceFromTarget = 0.0f;
				verticalDistanceFromTarget = 0.0f;
				target.position = transform.position;

				seeker.CanTurn = false;
				seeker.TargetDirection = targetStructure.position - transform.position;
			}
			else if (targetObstructionVehicle != null)
			{
				sqrHorizontalDistanceFromTarget = MathfEx.HorizontalDistanceSquared(targetObstructionVehicle.transform.position, transform.position);
				verticalDistanceFromTarget = Mathf.Abs(targetObstructionVehicle.transform.position.y - transform.position.y);
				target.position = targetObstructionVehicle.transform.position;

				seeker.CanTurn = false;
				seeker.TargetDirection = targetObstructionVehicle.transform.position - transform.position;
			}
			else if (targetObject != null)
			{
				sqrHorizontalDistanceFromTarget = 0.0f;
				verticalDistanceFromTarget = 0.0f;
				target.position = transform.position;

				seeker.CanTurn = false;
				seeker.TargetDirection = targetObject.transform.position - transform.position;
			}
			else if (player != null)
			{
				// Kuwait boss prioritizes damaging passenger rather than the vehicle itself because the final bossfight
				// uses vehicles as stationary turrets / cannons which should not be destroyed.
				targetPassengerVehicle = speciality != EZombieSpeciality.BOSS_KUWAIT ? player.movement.getVehicle() : null;

				if (targetPassengerVehicle != null)
				{
					if (targetPassengerVehicle.isDead)
					{
						targetPassengerVehicle = null;
					}
					else if (targetPassengerVehicle.asset == null || !targetPassengerVehicle.asset.isVulnerableToEnvironment)
					{
						targetPassengerVehicle = null;
					}
				}

				if (targetPassengerVehicle != null)
				{
					sqrHorizontalDistanceFromTarget = MathfEx.HorizontalDistanceSquared(targetPassengerVehicle.transform.position, transform.position);
					verticalDistanceFromTarget = Mathf.Abs(targetPassengerVehicle.transform.position.y - transform.position.y);
					target.position = targetPassengerVehicle.transform.position;

					// Nelson 2024-12-11: Previously, canTurn was false here. This prevented zombies from pathfinding
					// to the vehicle. (public issue #4805) I'm unsure what the original reasoning behind turning off
					// steering was. :/
					seeker.CanTurn = true;
				}
				else
				{
					sqrHorizontalDistanceFromTarget = MathfEx.HorizontalDistanceSquared(player.transform.position, transform.position);
					verticalDistanceFromTarget = Mathf.Abs(player.transform.position.y - transform.position.y);

					target.position = player.transform.position;

					if (path == EZombiePath.LEFT_FLANK)
					{
						// step #1: if far away aim for point far to the side
						// step #2: start aiming for diagonal from behind
						// step #3: directly attack 
						if (sqrHorizontalDistanceFromTarget > 100)
						{
							seeker.CanTurn = true;
							target.position += (player.transform.right * 9.0f) + (player.transform.forward * -4.0f);
						}
						else if (sqrHorizontalDistanceFromTarget > 20 || Vector3.Dot((transform.position - player.transform.position).normalized, player.transform.forward) > 0.0f)
						{
							seeker.CanTurn = true;
							target.position += (player.transform.right * 3.0f) + (player.transform.forward * -3.0f);
						}
						else
						{
							if (sqrHorizontalDistanceFromTarget > 4)
							{
								seeker.CanTurn = true;
								target.position -= player.transform.forward;
							}
							else
							{
								seeker.CanTurn = false;
								seeker.TargetDirection = player.transform.position - transform.position;
							}
						}
					}
					else if (path == EZombiePath.RIGHT_FLANK)
					{
						if (sqrHorizontalDistanceFromTarget > 100)
						{
							seeker.CanTurn = true;
							target.position += (player.transform.right * -9.0f) + (player.transform.forward * -4.0f);
						}
						else if (sqrHorizontalDistanceFromTarget > 20 || Vector3.Dot((transform.position - player.transform.position).normalized, player.transform.forward) > 0.0f)
						{
							seeker.CanTurn = true;
							target.position += (player.transform.right * -3.0f) + (player.transform.forward * -3.0f);
						}
						else
						{
							if (sqrHorizontalDistanceFromTarget > 4)
							{
								seeker.CanTurn = true;
								target.position -= player.transform.forward;
							}
							else
							{
								seeker.CanTurn = false;
								seeker.TargetDirection = player.transform.position - transform.position;
							}
						}
					}
					else if (path == EZombiePath.LEFT)
					{
						if (sqrHorizontalDistanceFromTarget > 4)
						{
							seeker.CanTurn = true;
							target.position -= transform.right;
						}
						else
						{
							seeker.CanTurn = false;
							seeker.TargetDirection = player.transform.position - transform.position;
						}
					}
					else if (path == EZombiePath.RIGHT)
					{
						if (sqrHorizontalDistanceFromTarget > 4)
						{
							seeker.CanTurn = true;
							target.position += transform.right;
						}
						else
						{
							seeker.CanTurn = false;
							seeker.TargetDirection = player.transform.position - transform.position;
						}
					}
					else if (path == EZombiePath.RUSH)
					{
						if (sqrHorizontalDistanceFromTarget > 4)
						{
							seeker.CanTurn = true;
							target.position -= transform.forward;
						}
						else
						{
							seeker.CanTurn = false;
							seeker.TargetDirection = player.transform.position - transform.position;
						}
					}

					if (!Dedicator.IsDedicatedServer)
					{
						if (speciality == EZombieSpeciality.SPRINTER)
						{
							target.position -= transform.forward * 0.15f;
						}
					}
				}
			}
			else
			{
				sqrHorizontalDistanceFromTarget = MathfEx.HorizontalDistanceSquared(target.position, transform.position);
				verticalDistanceFromTarget = Mathf.Abs(target.position.y - transform.position.y);

				seeker.CanTurn = true;
			}

			isMoving = sqrHorizontalDistanceFromTarget > 3;

			if (!isWandering)
			{
				if (sqrHorizontalDistanceFromTarget > 4096 && (player == null || !zombieRegion.HasInfiniteAgroRange))
				{
					leave(false);

					return;
				}
			}

			if (player != null || targetBarricade != null || targetStructure != null || targetObstructionVehicle != null || targetPassengerVehicle != null || targetObject != null)
			{
				if (player != null && Time.time - lastStartle > specialStartleDelay && Time.time - lastAttack > specialAttackDelay && Time.time - lastSpecial > specialUseDelay)
				{
					availableAbilityChoices.Clear();

					if ((speciality == EZombieSpeciality.MEGA || speciality == EZombieSpeciality.BOSS_KUWAIT || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL))
					{
						if (sqrHorizontalDistanceFromTarget > 20)
						{
							availableAbilityChoices.Add(EAbilityChoice.ThrowBoulder);
						}
					}

					if ((speciality == EZombieSpeciality.ACID || speciality == EZombieSpeciality.BOSS_NUCLEAR || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL))
					{
						availableAbilityChoices.Add(EAbilityChoice.SpitAcid);
					}

					if ((speciality == EZombieSpeciality.BOSS_WIND || speciality == EZombieSpeciality.BOSS_BUAK_WIND || speciality == EZombieSpeciality.BOSS_ELVER_STOMPER || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL))
					{
						if (sqrHorizontalDistanceFromTarget < 144)
						{
							availableAbilityChoices.Add(EAbilityChoice.Stomp);
						}
					}

					if ((speciality == EZombieSpeciality.BOSS_FIRE || speciality == EZombieSpeciality.BOSS_MAGMA || speciality == EZombieSpeciality.BOSS_BUAK_FIRE || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL))
					{
						if (sqrHorizontalDistanceFromTarget < 529)
						{
							availableAbilityChoices.Add(EAbilityChoice.BreatheFire);
						}
					}

					if ((speciality == EZombieSpeciality.BOSS_ELECTRIC || speciality == EZombieSpeciality.BOSS_BUAK_ELECTRIC || speciality == EZombieSpeciality.BOSS_ALL || speciality == EZombieSpeciality.BOSS_BUAK_FINAL))
					{
						if (sqrHorizontalDistanceFromTarget > 4 && sqrHorizontalDistanceFromTarget < 4096)
						{
							availableAbilityChoices.Add(EAbilityChoice.ElectricShock);
						}
					}

					if ((speciality == EZombieSpeciality.BOSS_KUWAIT || speciality.IsFromBuakMap()) && Time.time - lastFlashbang > flashbangDelay)
					{
						if (sqrHorizontalDistanceFromTarget > 4 && sqrHorizontalDistanceFromTarget < 32 * 32)
						{
							availableAbilityChoices.Add(EAbilityChoice.Flashbang);
						}
					}

					if (availableAbilityChoices.Count > 0)
					{
						lastSpecial = Time.time;

						EAbilityChoice abilityChoice = availableAbilityChoices.RandomOrDefault();
						if (abilityChoice == EAbilityChoice.ThrowBoulder)
						{
							specialUseDelay = Random.Range(6.0f, 12.0f);
							if (speciality == EZombieSpeciality.BOSS_KUWAIT || speciality == EZombieSpeciality.BOSS_BUAK_FINAL)
							{
								specialUseDelay *= 0.5f;
							}

							seeker.CanMove = false;
							ZombieManager.sendZombieThrow(this);
						}
						else if (abilityChoice == EAbilityChoice.SpitAcid)
						{
							specialUseDelay = Random.Range(4.0f, 8.0f);

							seeker.CanMove = false;
							ZombieManager.sendZombieSpit(this);
						}
						else if (abilityChoice == EAbilityChoice.Stomp)
						{
							specialUseDelay = Random.Range(4.0f, 8.0f);

							seeker.CanMove = false;
							ZombieManager.sendZombieStomp(this);
						}
						else if (abilityChoice == EAbilityChoice.BreatheFire)
						{
							specialUseDelay = Random.Range(4.0f, 8.0f);

							seeker.CanMove = false;
							ZombieManager.sendZombieBreath(this);
						}
						else if (abilityChoice == EAbilityChoice.ElectricShock)
						{
							specialUseDelay = Random.Range(4.0f, 8.0f);

							seeker.CanMove = false;
							ZombieManager.sendZombieCharge(this);
						}
						else if (abilityChoice == EAbilityChoice.Flashbang)
						{
							specialUseDelay = Random.Range(1.0f, 2.0f);

							lastFlashbang = Time.time;
							flashbangDelay = Random.Range(30.0f, 45.0f);

							EffectAsset flashbangEffect = speciality == EZombieSpeciality.BOSS_KUWAIT ? KuwaitBossFlashbangRef.Find() : BuakBossFlashbangRef.Find();
							if (flashbangEffect != null)
							{
								TriggerEffectParameters effectParameters = new TriggerEffectParameters(flashbangEffect);
								effectParameters.reliable = true;
								effectParameters.position = transform.position + new Vector3(0.0f, 5.0f, 0.0f);
								EffectManager.triggerEffect(effectParameters);
							}
							else
							{
								UnturnedLog.warn("Missing built-in zombie flashbang effect");
							}
						}
					}
				}

				if ((targetStructure != null || sqrHorizontalDistanceFromTarget < GetHorizontalAttackRangeSquared()) && verticalDistanceFromTarget < GetVerticalAttackRange())
				{
					if (speciality == EZombieSpeciality.SPRINTER || Time.time - lastTarget > (Dedicator.IsDedicatedServer ? 0.5f : 0.1f))
					{
						if (isAttacking)
						{
							if (Time.time - lastAttack > attackTime / 2)
							{
								isAttacking = false;
								byte damage = (byte) (LevelZombies.tables[type].damage * (isHyper ? 1.5f : 1));
								damage = (byte) (damage * Provider.modeConfigData.Zombies.Damage_Multiplier);

								if (speciality == EZombieSpeciality.CRAWLER)
								{
									damage = (byte) (damage * 2f);
								}
								else if (speciality == EZombieSpeciality.SPRINTER)
								{
									damage = (byte) (damage * 0.75f);
								}

								if (targetStructure != null)
								{
									StructureManager.damage(targetStructure, (target.position - transform.position).normalized * damage, damage, 1, true, damageOrigin: EDamageOrigin.Zombie_Swipe);

									if (targetStructure == null || !targetStructure.CompareTag("Structure"))
									{
										targetStructure = null;

										isStuck = false;

										lastStuck = Time.time;
										stuckSearchTimer = 0.0f;
									}
								}
								else if (targetBarricade != null)
								{
									BarricadeManager.damage(targetBarricade, damage, 1, true, damageOrigin: EDamageOrigin.Zombie_Swipe);
								}
								else if (targetObstructionVehicle != null)
								{
									VehicleManager.damage(targetObstructionVehicle, damage, 1, true, damageOrigin: EDamageOrigin.Zombie_Swipe);
								}
								else if (targetPassengerVehicle != null)
								{
									VehicleManager.damage(targetPassengerVehicle, damage, 1, true, damageOrigin: EDamageOrigin.Zombie_Swipe);
								}
								else if (targetObject != null)
								{
									if (targetObject.TryGetRandomAliveSectionIndex(out byte sectionIndex))
									{
										float times = targetObject.asset.RubbleZombieDamageMultiplier;

										Vector3 dir = (target.position - transform.position).normalized * damage;
										ObjectManager.damage(targetObject.transform, dir, sectionIndex, damage, times,
											out EPlayerKill kill, out uint xp, damageOrigin: EDamageOrigin.Zombie_Swipe, trackKill: false);
									}
								}
								else if (player != null)
								{
									if (player.skills.boost == EPlayerBoost.HARDENED)
									{
										damage = (byte) (damage * 0.75f);
									}

									if (isMega)
									{
										if (player.clothing.hatAsset != null)
										{
											ItemClothingAsset asset = player.clothing.hatAsset;

											if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
											{
												if (player.clothing.hatQuality > 0)
												{
													player.clothing.hatQuality--;

													player.clothing.sendUpdateHatQuality();
												}
											}

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.hatQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
										else if (player.clothing.vestAsset != null)
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

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.vestQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
										else if (player.clothing.shirtAsset != null)
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

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.shirtQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
									}
									else if (speciality == EZombieSpeciality.NORMAL)
									{
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

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.vestQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
										else if (player.clothing.shirtAsset != null)
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

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.shirtQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
									}
									else if (speciality == EZombieSpeciality.CRAWLER)
									{
										if (player.clothing.pantsAsset != null)
										{
											ItemClothingAsset asset = player.clothing.pantsAsset;

											if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
											{
												if (player.clothing.pantsQuality > 0)
												{
													player.clothing.pantsQuality--;

													player.clothing.sendUpdatePantsQuality();
												}
											}

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.pantsQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
									}
									else if (speciality == EZombieSpeciality.SPRINTER)
									{
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

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.vestQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
										else if (player.clothing.shirtAsset != null)
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

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.shirtQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
										else if (player.clothing.pantsAsset != null)
										{
											ItemClothingAsset asset = player.clothing.pantsAsset;

											if (Provider.modeConfigData.Items.ShouldClothingTakeDamage)
											{
												if (player.clothing.pantsQuality > 0)
												{
													player.clothing.pantsQuality--;

													player.clothing.sendUpdatePantsQuality();
												}
											}

											float multiplier = asset.armor + ((1f - asset.armor) * (1f - (player.clothing.pantsQuality / 100f)));
											damage = (byte) (damage * multiplier);
										}
									}

									EPlayerKill kill;
									DamageTool.damage(player, EDeathCause.ZOMBIE, ELimb.SKULL, Provider.server, (target.position - transform.position).normalized, damage, 1.0f, out kill);

									player.life.askInfect((byte) (damage / 3 * (1f - (player.skills.mastery((int) EPlayerSpeciality.DEFENSE, (int) EPlayerDefense.IMMUNITY) * 0.5f))));
								}
							}
						}
						else
						{
							if (Time.time - lastAttack > 1)
							{
								isAttacking = true;

								if (speciality == EZombieSpeciality.CRAWLER)
								{
									ZombieManager.sendZombieAttack(this, 5);
								}
								else if (speciality == EZombieSpeciality.SPRINTER)
								{
									ZombieManager.sendZombieAttack(this, (byte) Random.Range(6, 9));
								}
								else
								{
									ZombieManager.sendZombieAttack(this, (byte) Random.Range(0, 5));
								}
							}
						}
					}
				}
				else
				{
					lastTarget = Time.time;
					isAttacking = false;
				}
			}

			if (seeker != null)
			{
				UnityEngine.Profiling.Profiler.BeginSample("AIPath");
				seeker.Move(delta);
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		/// <summary>
		/// 2026-04-24: this *was* Unity's Update monobehavior message, but for maps with large numbers of zombies
		/// (2500+) it turned out to take a significant chunk of time. As a last-minute hack we now only update
		/// zombies in regions with players in them.
		/// </summary>
		internal void OnUpdate()
		{
			if (isDead)
			{
				return;
			}

			UnityEngine.Profiling.Profiler.BeginSample("Snapshot");

			if (Provider.isServer)
			{
				if (!isUpdated)
				{
					if (Mathf.Abs(lastUpdatedPos.x - transform.position.x) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.y - transform.position.y) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedPos.z - transform.position.z) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatedAngle - transform.rotation.eulerAngles.y) > 1)
					{
						lastUpdatedPos = transform.position;
						lastUpdatedAngle = transform.rotation.eulerAngles.y;

						isUpdated = true;
						zombieRegion.updates++;

						isStuck = false;

						lastStuck = Time.time;
						stuckSearchTimer = 0.0f;
					}
					else if (!isStuck)
					{
						if (isMoving)
						{
							isStuck = true;
						}
						else if (zombieRegion.HasInfiniteAgroRange)
						{
							if (player != null && (player.transform.position - transform.position).sqrMagnitude > 4)
							{
								isStuck = true;
							}
						}
					}
				}

				if (isPulled && Time.time - lastPull > pullDelay)
				{
					lastPull = Time.time;
					pullDelay = Random.Range(24.0f, 96.0f); // bump this value up

					if (!isLeaving && ZombieManager.canSpareWanderer)
					{
						float angle = Random.value * Mathf.PI * 2.0f;
						float dist = Random.Range(0.5f, 1.0f);

						isWandering = true;
						ZombieManager.wanderingCount++;

						isPulled = false;
						alert(cameFrom + new Vector3(Mathf.Cos(angle) * dist, 0.0f, Mathf.Sin(angle) * dist), false);
					}
				}
			}
			else
			{
				if (Mathf.Abs(lastUpdatedPos.x - transform.position.x) > 0.01f || Mathf.Abs(lastUpdatedPos.y - transform.position.y) > 0.01f || Mathf.Abs(lastUpdatedPos.z - transform.position.z) > 0.01f)
				{
					isMoving = true;
				}
				else
				{
					isMoving = false;
				}

				transform.position = Vector3.Lerp(transform.position, interpPositionTarget, Time.deltaTime * Provider.INTERP_SPEED);
				transform.rotation = Quaternion.Euler(0.0f, Mathf.LerpAngle(transform.rotation.eulerAngles.y, interpYawTarget, Time.deltaTime * Provider.INTERP_SPEED), 0.0f);
			}

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("Animation");

			if (isThrowingBoulder || isSpittingAcid || isBreathingFire || isChargingSpark)
			{
				if (Provider.isServer)
				{
					if (player != null)
					{
						Vector3 dir = (player.transform.position - transform.position).normalized;
						dir.y = 0;
						Quaternion rotation = Quaternion.LookRotation(dir);

						if (Dedicator.IsDedicatedServer)
						{
							transform.rotation = rotation;
						}
						else
						{
							transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 4 * Time.deltaTime);
						}
					}
				}
			}

			if (isThrowingBoulder)
			{
				if (Time.time - lastSpecial > throwTime)
				{
					StopThrowingBoulder();

					if (Provider.isServer)
					{
						if (player != null)
						{
							Vector3 offset = player.transform.position - transform.position;
							float distance = offset.magnitude;
							offset += Vector3.up * distance * 0.1f;

							// Somewhat take into account the direction the player is moving.
							// todo: during the projectile rewrite we can make this more precise
							float playerSpeed = player.movement.velocity.magnitude;
							if (playerSpeed > 0.1f)
							{
								Vector3 playerDirection = player.movement.velocity / playerSpeed;
								offset += playerDirection * distance * Random.Range(0.1f, 0.2f);
							}

							Vector3 normal = offset / distance;

							ZombieManager.sendZombieBoulder(this, transform.position + (Vector3.up * transform.localScale.y * 1.9f), normal);
						}
						else
						{
							ZombieManager.sendZombieBoulder(this, transform.position + (Vector3.up * transform.localScale.y * 1.9f), Vector3.forward);
						}
					}
				}
			}

			if (isSpittingAcid)
			{
				if (Time.time - lastSpecial > acidTime)
				{
					StopSpittingAcid();

					if (Provider.isServer)
					{
						if (player != null)
						{
							Vector3 offset = player.transform.position - transform.position;
							float distance = offset.magnitude;
							offset += Vector3.up * distance * 0.25f;

							ZombieManager.sendZombieAcid(this, transform.position + (Vector3.up * transform.localScale.y * 1.75f), offset.normalized);
						}
						else
						{
							ZombieManager.sendZombieAcid(this, transform.position + (Vector3.up * transform.localScale.y * 1.75f), Vector3.forward);
						}
					}
				}
			}

			if (isChargingSpark)
			{
				if (Time.time - lastSpecial > sparkTime)
				{
					StopChargingSpark();

					if (Provider.isServer)
					{
						if (player != null)
						{
							Vector3 target = player.look.aim.position;
							Vector3 offset = target - (transform.position + new Vector3(0, 2, 0));

							RaycastHit obstruction;
							if (Physics.Raycast(new Ray(transform.position + new Vector3(0, 2, 0), offset), out obstruction, offset.magnitude - 0.025f, RayMasks.BLOCK_SENTRY))
							{
								target = obstruction.point + obstruction.normal;
							}

							// 2023-06-15: was pointed out that if targeting is disabled then damage should probably be disabled. (public issue #3952)
							float barricadeDamage = Provider.modeConfigData.Zombies.Can_Target_Barricades ? 250.0f : 0.0f;
							float structureDamage = Provider.modeConfigData.Zombies.Can_Target_Structures ? 250.0f : 0.0f;
							float vehicleDamage = Provider.modeConfigData.Zombies.Can_Target_Vehicles ? 250.0f : 0.0f;

							List<EPlayerKill> kills;
							DamageTool.explode(target, 5.0f, EDeathCause.SPARK, Steamworks.CSteamID.Nil, 25.0f, 0.0f, 0.0f, barricadeDamage, structureDamage, vehicleDamage, 250.0f, 250.0f, out kills, EExplosionDamageType.ZOMBIE_ELECTRIC, 4.0f, damageOrigin: EDamageOrigin.Zombie_Electric_Shock);

							ZombieManager.sendZombieSpark(this, target);
						}
					}
				}
			}

			if (isStompingWind)
			{
				if (Time.time - lastSpecial > windTime)
				{
					StopStompingWind();

					if (Provider.isServer)
					{
						// 2023-06-15: was pointed out that if targeting is disabled then damage should probably be disabled. (public issue #3952)
						float barricadeDamage = Provider.modeConfigData.Zombies.Can_Target_Barricades ? 500.0f : 0.0f;
						float structureDamage = Provider.modeConfigData.Zombies.Can_Target_Structures ? 500.0f : 0.0f;
						float vehicleDamage = Provider.modeConfigData.Zombies.Can_Target_Vehicles ? 500.0f : 0.0f;

						List<EPlayerKill> kills;
						DamageTool.explode(transform.position + new Vector3(0.0f, 1.5f, 0.0f), 10.0f, EDeathCause.BOULDER, Steamworks.CSteamID.Nil, 60.0f, 0.0f, 0.0f, barricadeDamage, structureDamage, vehicleDamage, 500.0f, 500.0f, out kills, EExplosionDamageType.ZOMBIE_ACID, 32.0f, damageOrigin: EDamageOrigin.Zombie_Stomp);

						EffectAsset metal_2 = Boulder.Metal_2_Ref.Find();
						if (metal_2 != null)
						{
							TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(metal_2);
							triggerEffectParameters.relevantDistance = EffectManager.MEDIUM;
							triggerEffectParameters.position = transform.position;
							EffectManager.triggerEffect(triggerEffectParameters);
						}
					}
				}
			}

			if (isBreathingFire)
			{
				if (Provider.isServer)
				{
					if (isBreathingFire)
					{
						fireDamage += Time.deltaTime * 50.0f;

						if (fireDamage > 1.0f)
						{
							float playerDamage = fireDamage;
							float otherDamage = fireDamage * 10.0f;

							fireDamage = 0.0f;

							// 2023-06-15: was pointed out that if targeting is disabled then damage should probably be disabled. (public issue #3952)
							float barricadeDamage = Provider.modeConfigData.Zombies.Can_Target_Barricades ? otherDamage : 0.0f;
							float structureDamage = Provider.modeConfigData.Zombies.Can_Target_Structures ? otherDamage : 0.0f;
							float vehicleDamage = Provider.modeConfigData.Zombies.Can_Target_Vehicles ? otherDamage : 0.0f;

							List<EPlayerKill> kills;
							DamageTool.explode(transform.position + new Vector3(0.0f, 1.25f, 0.0f) + (transform.forward * 3.0f), 2.0f, EDeathCause.BURNER, Steamworks.CSteamID.Nil, playerDamage, 0.0f, 0.0f, barricadeDamage, structureDamage, vehicleDamage, otherDamage, otherDamage, out kills, EExplosionDamageType.ZOMBIE_FIRE, 4.0f, false, damageOrigin: EDamageOrigin.Zombie_Fire_Breath);
							DamageTool.explode(transform.position + new Vector3(0.0f, 1.25f, 0.0f) + (transform.forward * 7.0f), 3.0f, EDeathCause.BURNER, Steamworks.CSteamID.Nil, playerDamage, 0.0f, 0.0f, barricadeDamage, structureDamage, vehicleDamage, otherDamage, otherDamage, out kills, EExplosionDamageType.ZOMBIE_FIRE, 4.0f, false, damageOrigin: EDamageOrigin.Zombie_Fire_Breath);
							DamageTool.explode(transform.position + new Vector3(0.0f, 1.25f, 0.0f) + (transform.forward * 12.0f), 4.0f, EDeathCause.BURNER, Steamworks.CSteamID.Nil, playerDamage, 0.0f, 0.0f, barricadeDamage, structureDamage, vehicleDamage, otherDamage, otherDamage, out kills, EExplosionDamageType.ZOMBIE_FIRE, 4.0f, false, damageOrigin: EDamageOrigin.Zombie_Fire_Breath);
						}
					}
				}

				if (Time.time - lastSpecial > fireTime)
				{
					StopBreathingFire();
				}
			}

			if (isPlayingBoulder)
			{
				if (Time.time - lastSpecial > boulderTime)
				{
					isPlayingBoulder = false;
				}
			}
			else if (isPlayingSpit)
			{
				if (Time.time - lastSpecial > spitTime)
				{
					isPlayingSpit = false;
				}
			}
			else if (isPlayingCharge)
			{
				if (Time.time - lastSpecial > chargeTime)
				{
					isPlayingCharge = false;
				}
			}
			else if (isPlayingWind)
			{
				if (Time.time - lastSpecial > windTime)
				{
					isPlayingWind = false;
				}
			}
			else if (isPlayingFire)
			{
				if (Time.time - lastSpecial > fireTime)
				{
					isPlayingFire = false;
				}
			}
			else if (isPlayingAttack)
			{
				if (Time.time - lastAttack > attackTime)
				{
					if (speciality == EZombieSpeciality.FLANKER_FRIENDLY || speciality == EZombieSpeciality.FLANKER_STALK)
					{
						updateVisibility(false, true);
					}

					isPlayingAttack = false;
				}
			}
			else if (isPlayingStartle)
			{
				if (Time.time - lastStartle > startleTime)
				{
					isPlayingStartle = false;
				}
			}
			else if (isPlayingStun)
			{
				if (Time.time - lastStun > stunTime)
				{
					isPlayingStun = false;
				}
			}
			else
			{
				if (!Dedicator.IsDedicatedServer)
				{
					if (isMoving && (!Provider.isServer || !isStuck))
					{
						if (speciality == EZombieSpeciality.CRAWLER)
						{
							animator.CrossFade("Move_4", CharacterAnimator.BLEND);
						}
						else if (speciality == EZombieSpeciality.SPRINTER)
						{
							animator.CrossFade("Move_5", CharacterAnimator.BLEND);
						}
						else
						{
							animator.CrossFade(moveAnim, CharacterAnimator.BLEND);
						}
					}
					else
					{
						if (speciality == EZombieSpeciality.CRAWLER)
						{
							animator.CrossFade("Idle_3", CharacterAnimator.BLEND);
						}
						else if (speciality == EZombieSpeciality.SPRINTER)
						{
							animator.CrossFade("Idle_4", CharacterAnimator.BLEND);
						}
						else
						{
							animator.CrossFade(idleAnim, CharacterAnimator.BLEND);
						}
					}
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("Regen");

			if (Provider.isServer)
			{
				if (health < maxHealth && Time.time - lastRegen > LevelZombies.tables[type].regen)
				{
					lastRegen = Time.time;
					health++; // Potential timing issues, but not big deal. In most cases the server framerate is higher than this update rate.
				}
			}

			UnityEngine.Profiling.Profiler.EndSample();
			if (!Dedicator.IsDedicatedServer)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Groan");

				if (Time.time - lastGroan > groanDelay)
				{
					lastGroan = Time.time;

					if (isVisible)
					{
						UnityEngine.Profiling.Profiler.BeginSample("Delay");
						if (isMega)
						{
							groanDelay = Random.Range(2f, 4f);
						}
						else
						{
							groanDelay = Random.Range(4f, 8f);
						}
						UnityEngine.Profiling.Profiler.EndSample();

						if (!isMoving)
						{
							UnityEngine.Profiling.Profiler.BeginSample("Standing");

							if (Random.value > 0.8)
							{
								PlayOneShot(ZombieManager.groans);
							}

							UnityEngine.Profiling.Profiler.EndSample();
						}
						else
						{
							UnityEngine.Profiling.Profiler.BeginSample("Moving");

							AudioClip[] clips = speciality.IsDLVolatile() ? ZombieManager.dl_taunt : ZombieManager.roars;
							PlayOneShot(clips);

							UnityEngine.Profiling.Profiler.EndSample();
						}
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();

#if !DEDICATED_SERVER
				UnityEngine.Profiling.Profiler.BeginSample("Movement Audio");
				if (isMoving && OptionsSettings._zombieFootstepsVolume > 0.0001f)
				{
					bool isSilent;
					switch (speciality)
					{
						case EZombieSpeciality.FLANKER_STALK:
						case EZombieSpeciality.SPIRIT:
						case EZombieSpeciality.BOSS_SPIRIT:
							isSilent = true;
							break;

						default:
							isSilent = false;
							break;
					}

					if (!isSilent && (transform.position - lastFootstepPosition).sqrMagnitude > 2.0f)
					{
						lastFootstepPosition = transform.position;
						UpdateFootsteps();
					}
				}
				UnityEngine.Profiling.Profiler.EndSample();
#endif // !DEDICATED_SERVER
			}
			if (Provider.isServer)
			{
				if (isStunned)
				{
					if (Time.time - lastStun > 1)
					{
						lastTarget = Time.time;
						lastStuck = Time.time;
						stuckSearchTimer = 0.0f;

						isStunned = false;
						seeker.CanMove = true;
					}
					else
					{
						return;
					}
				}

				if (isLeaving && Time.time - lastLeave > leaveTime)
				{
					alert(leaveTo, false);

					isLeaving = false;
				}
			}
		}

		private void onHyperUpdated(bool isHyper)
		{
			if (eyes != null)
			{
				eyes.gameObject.SetActive(isHyper);
			}
		}

		public void init()
		{
			awake();
			start();
			SetCountedAsAliveInZombieRegion(!isDead);
			SetCountedAsAliveBossInZombieRegion(!isDead && isBoss);
		}

		internal ZombieRegion zombieRegion;

		private void start()
		{
			updateDifficulty();

			if (Provider.isServer)
			{
				seeker = UnturnedPathfinding.Get().CreateMovementComponentForZombie(this);

				CharacterController controller = GetComponent<CharacterController>();
				controller.enableOverlapRecovery = false; // Refer to PlayerMovement's comment.

				target = transform.Find("Target");
				target.parent = null;

				seeker.TargetTransform = target;

				reset();
			}
			else
			{
				lastUpdatedPos = transform.position;
				lastUpdatedAngle = transform.rotation.eulerAngles.y;
				interpPositionTarget = lastUpdatedPos;
				interpYawTarget = lastUpdatedAngle;
			}

			lastGroan = Time.time + Random.Range(4f, 16f);

			if (isMega)
			{
				groanDelay = Random.Range(2f, 4f);
			}
			else
			{
				groanDelay = Random.Range(4f, 8f);
			}

			updateLife();
			apply();
			updateEffects();
			updateVisibility(speciality != EZombieSpeciality.FLANKER_STALK && speciality != EZombieSpeciality.SPIRIT && speciality != EZombieSpeciality.BOSS_SPIRIT, false);
			updateStates();

			if (!Dedicator.IsDedicatedServer)
			{
				zombieRegion.onHyperUpdated += onHyperUpdated;
			}
		}

		private void awake()
		{
			throwTime = 1.0f;
			acidTime = 1.0f;
			windTime = 0.9f;
			fireTime = 2.75f;
			chargeTime = 1.8f;
			sparkTime = 1.2f;

			if (Dedicator.IsDedicatedServer)
			{
				boulderTime = 1.0f;
				spitTime = 1.0f;
				attackTime = 0.5f;
				startleTime = 0.5f;
				stunTime = 0.5f;
			}
			else
			{
				animator = transform.Find("Character").GetComponent<Animation>();
				skeleton = animator.transform.Find("Skeleton");
				rightHook = skeleton.Find("Spine").Find("Right_Shoulder").Find("Right_Arm").Find("Right_Hand").Find("Right_Hook");
				renderer_0 = animator.transform.Find("Model_0").GetComponent<SkinnedMeshRenderer>();
				renderer_1 = animator.transform.Find("Model_1").GetComponent<SkinnedMeshRenderer>();
				eyes = skeleton.Find("Spine").Find("Skull").Find("Eyes");
				radiation = skeleton.Find("Spine").Find("Radiation");
				burner = skeleton.Find("Spine").Find("Burner");
				acid = skeleton.Find("Spine").Find("Skull").Find("Acid");
				acidNuclear = skeleton.Find("Spine").Find("Skull").Find("Acid_Nuclear");
				electric = skeleton.Find("Spine").Find("Electric");
				sparkSystem = rightHook.Find("Spark").GetComponent<ParticleSystem>();
				fireSystem = skeleton.Find("Spine").Find("Skull").Find("Fire").GetComponent<ParticleSystem>();
				fireAudio = skeleton.Find("Spine").Find("Skull").Find("Fire").GetComponent<AudioSource>();

				boulderTime = animator["Boulder_0"].clip.length;
				spitTime = animator["Acid_0"].clip.length;
				attackTime = animator["Attack_0"].clip.length;
				startleTime = animator["Startle_0"].clip.length;
				stunTime = animator["Stun_0"].clip.length;
			}
		}

		private void OnDestroy()
		{
			if (Provider.isServer)
			{
				isHunting = false;
			}

			if (!Dedicator.IsDedicatedServer)
			{
				zombieRegion.onHyperUpdated -= onHyperUpdated;
			}

			// Prevent target from leaking because it is parented to world.
			if (target != null && target.parent != this)
			{
				Destroy(target.gameObject);
			}

			ZombieManager.AllZombies?.RemoveFast(this);
		}

		private void PlayOneShot(AudioClip[] clips)
		{
#if !DEDICATED_SERVER
			if (clips == null || clips.Length < 1)
			{
				return;
			}

			AudioClip randomClip = clips[Random.Range(0, clips.Length)];
			OneShotAudioParameters audioParams = new OneShotAudioParameters(transform, randomClip);
			audioParams.volume = 0.5f;
			audioParams.pitch = GetRandomPitch();
			audioParams.SetLinearRolloff(1.0f, 32.0f);
			audioParams.Play();
#endif // !DEDICATED_SERVER
		}

		private float GetRandomPitch()
		{
			float pitch;

			if (isMega)
			{
				pitch = Random.Range(0.5f, 0.7f);
			}
			else if (isCutesy)
			{
				pitch = Random.Range(1.3f, 1.4f);
			}
			else
			{
				pitch = Random.Range(0.9f, 1.1f);
			}

			if (isHyper)
			{
				pitch *= 0.9f;
			}

			return pitch;
		}

		/// <summary>
		/// Helper to prevent mistakes or plugins from breaking alive zombie count.
		/// </summary>
		private void SetCountedAsAliveInZombieRegion(bool newValue)
		{
			if (isCountedAsAliveInZombieRegion != newValue)
			{
				isCountedAsAliveInZombieRegion = newValue;
				if (newValue)
				{
					++zombieRegion.alive;
				}
				else
				{
					--zombieRegion.alive;
				}
			}
		}

		/// <summary>
		/// Helper to prevent mistakes or plugins from breaking alive boss zombie count.
		/// </summary>
		private void SetCountedAsAliveBossInZombieRegion(bool newValue)
		{
			if (isCountedAsAliveBossInZombieRegion != newValue)
			{
				isCountedAsAliveBossInZombieRegion = newValue;
				if (newValue)
				{
					++zombieRegion.aliveBossZombieCount;
				}
				else
				{
					--zombieRegion.aliveBossZombieCount;
				}
			}
		}

		/// <summary>
		/// 2023-01-31: set height to 2 rather than adjusting per-zombie-type. Tall zombies (megas) couldn't
		/// get through doorways, and short zombies (crawlers) could get underneath objects they shouldn't
		/// like gas tanks. Zombies were also stacking on top of eachother a bit too much.
		/// </summary>
		private void SetCapsuleRadiusAndHeight(float radius, float height)
		{
			if (Provider.isServer)
			{
				CharacterController characterController = GetComponent<CharacterController>();
				if (characterController != null)
				{
					characterController.radius = radius;
					characterController.center = new Vector3(0.0f, height * 0.5f, 0.0f);
					characterController.height = height;
				}
			}
			else
			{
				CapsuleCollider capsule = GetComponent<CapsuleCollider>();
				if (capsule != null)
				{
					capsule.radius = radius;
					capsule.center = new Vector3(0.0f, height * 0.5f, 0.0f);
					capsule.height = height;
				}
			}
		}

#if !DEDICATED_SERVER
		private Vector3 lastFootstepPosition;
		private bool wasGrounded;
		private void UpdateFootsteps()
		{
			int mask = RayMasks.BLOCK_COLLISION;
			Ray ray = new Ray(transform.position + new Vector3(0.0f, 0.1f, 0.0f), Vector3.down);
			bool isGrounded = Physics.Raycast(ray, out RaycastHit groundHit, 0.35f, mask, QueryTriggerInteraction.Ignore);
			string materialName = isGrounded ? PhysicsTool.GetMaterialName(groundHit) : null;

			if (isGrounded && !string.IsNullOrEmpty(materialName))
			{
				bool playedLand = false;
				if (!wasGrounded)
				{
					playedLand = PlayLandAudioClip(materialName);
				}

				if (!playedLand)
				{
					PlayFootstepAudioClip(materialName);
				}
			}
			wasGrounded = isGrounded;
		}

		/// <summary>
		/// Very similar to <see cref="PlayerMovement.PlayFootstepAudioClip"/>.
		/// </summary>
		private void PlayFootstepAudioClip(string materialName)
		{
			bool isRunning;
			switch (speciality)
			{
				case EZombieSpeciality.SPRINTER:
				case EZombieSpeciality.FLANKER_FRIENDLY:
				case EZombieSpeciality.FLANKER_STALK:
				case EZombieSpeciality.DL_BLUE_VOLATILE:
				case EZombieSpeciality.DL_RED_VOLATILE:
					isRunning = true;
					break;

				default:
					isRunning = false;
					break;
			}

			string key = isMega ? "MegaZombieFootstep" : (isRunning ? "ZombieFootstepRun" : "ZombieFootstepWalk");

			bool isVanillaHumanAudio = false;
			OneShotAudioDefinition audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, key);
			if (audioDef == null)
			{
				isVanillaHumanAudio = true;
				key = isRunning ? "FootstepRun" : "FootstepWalk";
				audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, key);
				if (audioDef == null)
				{
					return;
				}
			}

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return;

			float volume = 0.125f;

			OneShotAudioParameters parameters = new OneShotAudioParameters(transform, audioClip);
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);

			if (isVanillaHumanAudio)
			{
				// If not zombie-specific audio, help slightly differentiate from players.
				parameters.pitch *= 0.85f;

				if (isMega)
				{
					volume *= 1.5f;
					parameters.pitch *= 0.85f;
				}
			}

			parameters.volume = volume * audioDef.volumeMultiplier;
			parameters.SetLinearRolloff(1.0f, 32.0f);
			parameters.outputAudioMixerGroup = UnturnedAudioMixer.GetZombieFootstepsGroup();
			parameters.Play();
		}

		/// <summary>
		/// Very similar to <see cref="PlayerMovement.PlayLandAudioClip"/>.
		/// </summary>
		/// <returns>True if sound played.</returns>
		private bool PlayLandAudioClip(string materialName)
		{
			string key = isMega ? "MegaZombieLand" : "ZombieBipedLand";
			bool isVanillaHumanAudio = false;
			OneShotAudioDefinition audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, key);
			if (audioDef == null)
			{
				isVanillaHumanAudio = true;
				audioDef = PhysicMaterialCustomData.GetAudioDef(materialName, "BipedLand");
				if (audioDef == null)
				{
					return false;
				}
			}

			AudioClip audioClip = audioDef.GetRandomClip();
			if (audioClip == null)
				return false;

			float volume = 0.15f;

			OneShotAudioParameters parameters = new OneShotAudioParameters(transform, audioClip);
			parameters.RandomizePitch(audioDef.minPitch, audioDef.maxPitch);

			if (isVanillaHumanAudio)
			{
				// If not zombie-specific audio, help slightly differentiate from players.
				parameters.pitch *= 0.85f;

				if (isMega)
				{
					volume *= 1.5f;
					parameters.pitch *= 0.85f;
				}
			}

			parameters.volume = volume * audioDef.volumeMultiplier;
			parameters.SetLinearRolloff(1.0f, 24.0f);
			parameters.outputAudioMixerGroup = UnturnedAudioMixer.GetZombieFootstepsGroup();
			parameters.Play();

			return true;
		}
#endif // !DEDICATED_SERVER

		private bool isCountedAsAliveInZombieRegion;
		private bool isCountedAsAliveBossInZombieRegion;

		private static readonly AssetReference<EffectAsset> KuwaitBossFlashbangRef = new AssetReference<EffectAsset>("5436f56485c841a7bbec8e79a163ad19");
		private static readonly AssetReference<EffectAsset> BuakBossFlashbangRef = new AssetReference<EffectAsset>("b7acfd045ceb40c1b84788cb9159d0f2");
		private static readonly AssetReference<EffectAsset> Zombie_0_Ref = new AssetReference<EffectAsset>("000f550dc3d44586b7fc0f6e5b2530d9"); // Radioactive Explosion (94)
		private static readonly AssetReference<EffectAsset> Zombie_1_Ref = new AssetReference<EffectAsset>("f2f0d31897024317b32b58c00c1f78dd"); // Purple Wisps of Smoke (118)
		private static readonly AssetReference<EffectAsset> Zombie_2_Ref = new AssetReference<EffectAsset>("469414f0a1b047c58732bb6076b0c035"); // Fiery Explosion (119)
		private static readonly AssetReference<EffectAsset> Zombie_3_Ref = new AssetReference<EffectAsset>("ae477aac40b64d3c8ce8e538daffecf5"); // Regular Acid (121)
		private static readonly AssetReference<EffectAsset> Zombie_4_Ref = new AssetReference<EffectAsset>("9fd759eda4b746dfb9f2599bf8f27219"); // Electric Source Sparks (127)
		private static readonly AssetReference<EffectAsset> Zombie_5_Ref = new AssetReference<EffectAsset>("50872061be8e411ea28780fcb7aa7cef"); // Ground Pounder Ring (128)
		private static readonly AssetReference<EffectAsset> Zombie_6_Ref = new AssetReference<EffectAsset>("23363b069ad740819f1d7131656f8ca7"); // Electric Impact Sparks (129)
		private static readonly AssetReference<EffectAsset> Zombie_7_Ref = new AssetReference<EffectAsset>("36b272f5be8c4427b0fdd0625f361c15"); // Nuclear Acid (149)

		private enum EAbilityChoice
		{
			ThrowBoulder,
			SpitAcid,
			Stomp,
			BreatheFire,
			ElectricShock,
			Flashbang,
		}

		private static List<EAbilityChoice> availableAbilityChoices = new List<EAbilityChoice>();
	}
}
