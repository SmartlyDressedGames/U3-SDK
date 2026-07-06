////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class Animal : MonoBehaviour, IExplosionDamageable
	{
		private Animation animator;
		private Transform skeleton;
		private Renderer renderer_0;
		private Renderer renderer_1;

		private double lastEat;
		private double lastGlance;
		private double startleAnimationCompletionTime;
		private double lastWander;
		private double lastStuck;
		private double lastTarget;
		private double lastAttack;
		private double lastRegen;

		private float eatTime;
		private float glanceTime;
		private float attackDuration;
		private float attackInterval;

		private double startedRoar;
		private double startedPanic;

		private float eatDelay;
		private float glanceDelay;
		private float wanderDelay;

		private bool isPlayingEat;
		private bool isPlayingGlance;
		private bool isPlayingStartleAnimation;
		private bool isPlayingAttack;

		private Player currentTargetPlayer;
		public Vector3 target
		{
			get;
			private set;
		}

		private Vector3 lastUpdatePos;
		private float lastUpdateAngle;
		private NetworkSnapshotBuffer<YawSnapshotInfo> nsb;

		private bool isMoving;
		private bool isRunning;

		#region IExplosionDamageable
		public bool Equals(IExplosionDamageable obj)
		{
			return ReferenceEquals(this, obj);
		}

		public bool IsEligibleForExplosionDamage
		{
			get => IsAlive;
		}

		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
		{
			return CollisionUtil.ClosestPoint(gameObject, explosionCenter, false, DamageTool.EXPLOSION_CLOSEST_POINT_LAYER_MASK);
		}

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
		{
			if (!damageParameters.shouldAffectAnimals)
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
				EffectAsset fleshEffect = DamageTool.FleshDynamicRef.Find();
				if (fleshEffect != null)
				{
					TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(fleshEffect);
					triggerEffectParameters.relevantDistance = EffectManager.SMALL;
					triggerEffectParameters.position = transform.position + Vector3.up + Vector3.up;
					triggerEffectParameters.reliable = true;
					EffectManager.triggerEffect(triggerEffectParameters);

					// Spawn a second time pointing towards the damage.
					triggerEffectParameters.SetDirection(-normal);
					EffectManager.triggerEffect(triggerEffectParameters);
				}
			}

			DamageTool.damage(this, normal, explosionParameters.animalDamage, 1.0f - (range / explosionParameters.damageRadius),
				out EPlayerKill kill, out uint xp, ragdollEffect: explosionParameters.ragdollEffect);

			if (kill != EPlayerKill.NONE)
			{
				damageParameters.kills.Add(kill);
			}
			damageParameters.xp += xp;
		}
		#endregion IExplosionDamageable

		private bool isTicking;
		private void updateTicking()
		{
			if (isFleeing || isWandering || isHunting)
			{
				if (isTicking)
				{
					return;
				}
				isTicking = true;

				AnimalManager.tickingAnimals.Add(this);
				lastTick = Time.timeAsDouble;
			}
			else
			{
				if (!isTicking)
				{
					return;
				}
				isTicking = false;

				AnimalManager.tickingAnimals.RemoveFast(this);
			}
		}

		private bool _isFleeing;
		public bool isFleeing => _isFleeing;

		private bool isWandering;
		public bool isHunting
		{
			get;
			private set;
		}

		private bool isStuck;
		private bool isAttacking;

		private float _lastDead;
		public float lastDead => _lastDead;

		public bool isDead;

		public bool IsAlive => !isDead;

		public ushort index;
		public ushort id;
		public PackInfo pack;
		private ushort health;
		private Vector3 ragdoll;

		private AnimalAsset _asset;
		public AnimalAsset asset => _asset;

		private CharacterController controller;

		/// <summary>
		/// Whether this animal was updated in this network tick and needs to be resent.
		/// </summary>
		public bool isUpdated;

		public float GetHealth()
		{
			return health;
		}

		public Player GetTargetPlayer()
		{
			return currentTargetPlayer;
		}

		public void askEat()
		{
			if (isDead)
			{
				return;
			}

			lastEat = Time.timeAsDouble;
			eatDelay = Random.Range(4f, 8f);
			isPlayingEat = true;

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				string clipName;
				if (asset.eatAnimVariantsCount == 1)
				{
					clipName = "Eat"; // Legacy
				}
				else
				{
					int animIndex = Random.Range(0, asset.eatAnimVariantsCount);
					clipName = "Eat_" + animIndex;
				}

				AnimationClip clip = animator?.GetClip(clipName);
				if (clip != null)
				{
					eatTime = clip.length;
					animator.Play(clipName);
				}
				else if (Assets.shouldValidateAssets)
				{
					Assets.ReportError(asset, $"missing AnimationClip \"{clipName}\"");
				}
			}
		}

		public void askGlance()
		{
			if (isDead)
			{
				return;
			}

			lastGlance = Time.timeAsDouble;
			glanceDelay = Random.Range(4f, 8f);
			isPlayingGlance = true;

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				int animIndex = Random.Range(0, asset.glanceAnimVariantsCount);
				string clipName = "Glance_" + animIndex;

				AnimationClip clip = animator?.GetClip(clipName);
				if (clip != null)
				{
					glanceTime = clip.length;
					animator.Play(clipName);
				}
				else if (Assets.shouldValidateAssets)
				{
					Assets.ReportError(asset, $"missing AnimationClip \"{clipName}\"");
				}
			}
		}

		public void PlayStartleAnimation(byte animationIndex)
		{
			if (isDead)
			{
				return;
			}

			startleAnimationCompletionTime = Time.timeAsDouble + 0.5; // Placeholder in case animation is missing.
			isPlayingStartleAnimation = true;

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				string clipName;
				if (asset.startleAnimVariantsCount == 1)
				{
					clipName = "Startle"; // Legacy
				}
				else
				{
					clipName = "Startle_" + animationIndex;
				}

				AnimationClip clip = animator?.GetClip(clipName);
				if (clip != null)
				{
					startleAnimationCompletionTime = Time.timeAsDouble + clip.length;
					animator.Play(clipName);
				}
				else if (Assets.shouldValidateAssets)
				{
					Assets.ReportError(asset, $"missing AnimationClip \"{clipName}\"");
				}
			}
		}

		public void askAttack(byte animationIndex)
		{
			if (isDead)
			{
				return;
			}

			lastAttack = Time.timeAsDouble;
			isPlayingAttack = true;

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				string clipName;
				if (asset.attackAnimVariantsCount == 1)
				{
					clipName = "Attack"; // Legacy
				}
				else
				{
					clipName = "Attack_" + animationIndex;
				}

				AnimationClip clip = animator?.GetClip(clipName);
				if (clip != null)
				{
					attackDuration = clip.length;
					attackInterval = Mathf.Max(attackDuration, asset.attackInterval);
					animator.Play(clipName);
				}
				else if (Assets.shouldValidateAssets)
				{
					Assets.ReportError(asset, $"missing AnimationClip \"{clipName}\"");
				}

				if (asset != null && asset.roars != null && asset.roars.Length > 0 && Time.timeAsDouble - startedRoar > 1)
				{
					startedRoar = Time.timeAsDouble;

#if !DEDICATED_SERVER
					AudioClip roarClip = asset.roars[Random.Range(0, asset.roars.Length)];
					OneShotAudioParameters roarParams = new OneShotAudioParameters(transform, roarClip);
					roarParams.volume = 0.5f;
					roarParams.RandomizePitch(0.9f, 1.1f);
					roarParams.SetLinearRolloff(1.0f, 32.0f);
					roarParams.Play();
#endif // !DEDICATED_SERVER
				}
			}
		}

		public void askPanic()
		{
			if (isDead)
			{
				return;
			}

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				if (asset != null && asset.panics != null && asset.panics.Length > 0 && Time.timeAsDouble - startedPanic > 1)
				{
					startedPanic = Time.timeAsDouble;

#if !DEDICATED_SERVER
					AudioClip panicClip = asset.panics[Random.Range(0, asset.panics.Length)];
					OneShotAudioParameters panicParams = new OneShotAudioParameters(transform, panicClip);
					panicParams.volume = 0.5f;
					panicParams.RandomizePitch(0.9f, 1.1f);
					panicParams.SetLinearRolloff(1.0f, 32.0f);
					panicParams.Play();
#endif // !DEDICATED_SERVER
				}
			}
		}

		public void askDamage(ushort amount, Vector3 newRagdoll, out EPlayerKill kill, out uint xp, bool trackKill = true, bool dropLoot = true, ERagdollEffect ragdollEffect = ERagdollEffect.None)
		{
			kill = EPlayerKill.NONE;
			xp = 0;

			if (amount == 0 || isDead)
			{
				return;
			}

			if (IsAlive)
			{
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
					kill = EPlayerKill.ANIMAL;
					if (asset != null)
					{
						xp = asset.rewardXP;
					}

					if (dropLoot)
					{
						AnimalManager.dropLoot(this);
					}

					AnimalManager.sendAnimalDead(this, ragdoll, ragdollEffect);

					if (trackKill)
					{
						for (int playerIndex = 0; playerIndex < Provider.clients.Count; playerIndex++)
						{
							SteamPlayer player = Provider.clients[playerIndex];

							if (player.player == null || player.player.movement == null || player.player.life == null || player.player.life.isDead)
							{
								continue;
							}

							if ((player.player.transform.position - transform.position).sqrMagnitude < 262144) // 512 meters is the max draw distance
							{
								player.player.quests.trackAnimalKill(this);
							}
						}
					}
				}
				else
				{
					if (asset != null && asset.panics != null && asset.panics.Length > 0)
					{
						AnimalManager.sendAnimalPanic(this);
					}
				}

				lastRegen = Time.timeAsDouble;
			}
		}

		public void sendRevive(Vector3 position, float angle)
		{
			AnimalManager.sendAnimalAlive(this, position, MeasurementTool.angleToByte(angle));
		}

		private bool checkTargetValid(Vector3 point)
		{
			if (!Level.checkSafeIncludingClipVolumes(point))
			{
				return false;
			}

			float height = LevelGround.getHeight(point);
			return !SDG.Framework.Water.WaterUtility.isPointUnderwater(new Vector3(point.x, height - 1, point.z));
		}

		private Vector3 getFleeTarget(Vector3 normal)
		{
			// Try furthest away from danger.
			Vector3 furthestAway = transform.position + (normal * 64f) + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
			if (checkTargetValid(furthestAway))
			{
				return furthestAway;
			}

			// Try away from danger, but slightly closer.
			Vector3 point = transform.position + (normal * 32f) + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
			if (checkTargetValid(point))
			{
				return point;
			}

			// Try toward danger in the hopes we can run past it and escape.
			point = transform.position + (normal * -32) + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
			if (checkTargetValid(point))
			{
				return point;
			}

			// Try toward danger, but slightly closer. 
			point = transform.position + (normal * -16f) + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
			if (checkTargetValid(point))
			{
				return point;
			}

			// Nothing nearby is valid, but standing here would be dumb, so fallback to furthest away from danger.
			return furthestAway;
		}

		private void getWanderTarget()
		{
			Vector3 point;

			if (isStuck) // stuck is highest priority because this animal could be trying to get back, but stuck in a room
			{
				point = transform.position + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));

				if (!checkTargetValid(point))
				{
					return;
				}
			}
			// Nelson 2024-04-01: in vanilla pack should always be valid, but I saw a log file running into an exception
			// with null pack, so we handle that just in case.
			else if (pack != null)
			{
				if ((transform.position - pack.getAverageAnimalPoint()).sqrMagnitude > 256)
				{
					point = pack.getAverageAnimalPoint() + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
				}
				else
				{
					Vector3 normal = pack.getWanderDirection();

					point = transform.position + (normal * Random.Range(6.0f, 8.0f)) + new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));

					if (!checkTargetValid(point))
					{
						point = transform.position - (normal * Random.Range(6.0f, 8.0f)) + new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));

						if (!checkTargetValid(point))
						{
							return;
						}

						pack.wanderAngle += Random.Range(160.0f, 200.0f);
					}
					else
					{
						pack.wanderAngle += Random.Range(-20.0f, 20.0f);
					}
				}
			}
			else
			{
				point = transform.position + new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
			}

			target = point;
			isWandering = true;
			updateTicking();
		}

		public bool checkAlert(Player potentialTargetPlayer)
		{
			return currentTargetPlayer != potentialTargetPlayer;
		}

		public void alertPlayer(Player potentialTargetPlayer, bool sendToPack)
		{
			if (sendToPack && pack != null)
			{
				for (int index = 0; index < pack.animals.Count; index++)
				{
					Animal animal = pack.animals[index];

					if (animal == null || animal == this)
					{
						continue;
					}

					animal.alertPlayer(potentialTargetPlayer, false);
				}
			}

			if (isDead)
			{
				return;
			}

			if (currentTargetPlayer == potentialTargetPlayer)
			{
				return;
			}

			if (!isHunting && asset.startleAnimVariantsCount > 0)
			{
				int animIndex = Random.Range(0, asset.startleAnimVariantsCount);
				AnimalManager.sendAnimalStartle(this, MathfEx.ClampToByte(animIndex));
			}

			if (currentTargetPlayer == null)
			{
				_isFleeing = false;
				isWandering = false;
				isHunting = true;
				updateTicking();

				lastStuck = Time.timeAsDouble;

				currentTargetPlayer = potentialTargetPlayer;
			}
			else
			{
				if ((potentialTargetPlayer.transform.position - transform.position).sqrMagnitude < (currentTargetPlayer.transform.position - transform.position).sqrMagnitude)
				{
					_isFleeing = false;
					isWandering = false;
					isHunting = true;
					updateTicking();

					currentTargetPlayer = potentialTargetPlayer;
				}
			}
		}

		/// <summary>
		/// Alert this animal that it was damaged from a given position.
		/// Offensive animals investigate the position, whereas other animals run away.
		/// </summary>
		public void alertDamagedFromPoint(Vector3 point)
		{
			if (asset != null && asset.behaviour == EAnimalBehaviour.OFFENSE)
			{
				alertGoToPoint(point, true);
			}
			else
			{
				alertRunAwayFromPoint(point, true);
			}
		}

		/// <summary>
		/// Alerts this animal that it needs to run away.
		/// </summary>
		/// <param name="newPosition">The position to run away from.</param>
		public void alertRunAwayFromPoint(Vector3 newPosition, bool sendToPack)
		{
			alertDirection((transform.position - newPosition).normalized, sendToPack);
		}

		/// <summary>
		/// Keep for plugin backwards compatibility.
		/// </summary>
		[System.Obsolete("Clarified with alertRunAwayFromPoint, alertGoToPoint and alertDamagedFromPoint.")]
		public void alertPoint(Vector3 newPosition, bool sendToPack)
		{
			alertRunAwayFromPoint(newPosition, sendToPack);
		}

		public void alertDirection(Vector3 newDirection, bool sendToPack)
		{
			if (sendToPack && pack != null)
			{
				for (int index = 0; index < pack.animals.Count; index++)
				{
					Animal animal = pack.animals[index];

					if (animal == null || animal == this)
					{
						continue;
					}

					animal.alertDirection(newDirection, false);
				}
			}

			if (isDead)
			{
				return;
			}

			if (isStuck) // no distracting when trying to get away
			{
				return;
			}

			if (isHunting)
			{
				return;
			}

			if (!isFleeing && asset.startleAnimVariantsCount > 0)
			{
				int animIndex = Random.Range(0, asset.startleAnimVariantsCount);
				AnimalManager.sendAnimalStartle(this, MathfEx.ClampToByte(animIndex));
			}

			_isFleeing = true;
			isWandering = false;
			isHunting = false;
			updateTicking();
			target = getFleeTarget(newDirection);
		}

		public void alertGoToPoint(Vector3 point, bool sendToPack)
		{
			if (sendToPack && pack != null)
			{
				for (int index = 0; index < pack.animals.Count; index++)
				{
					Animal animal = pack.animals[index];

					if (animal == null || animal == this)
					{
						continue;
					}

					animal.alertGoToPoint(point, false);
				}
			}

			if (isDead)
			{
				return;
			}

			if (isFleeing || isHunting)
			{
				return;
			}

			lastWander = Time.timeAsDouble;
			_isFleeing = false;
			isWandering = true;
			isHunting = false;
			target = point;
			updateTicking();
		}

		private void stop()
		{
			isMoving = false;
			isRunning = false;
			_isFleeing = false;
			isWandering = false;
			isHunting = false;
			updateTicking();
			isStuck = false;

			currentTargetPlayer = null;
			target = transform.position;
		}

		public void tellAlive(Vector3 newPosition, byte newAngle)
		{
			isDead = false;

			transform.position = newPosition;
			transform.rotation = Quaternion.Euler(0, newAngle * 2, 0);

			updateLife();
			updateStates();

			reset();
		}

		public void tellDead(Vector3 newRagdoll, ERagdollEffect ragdollEffect)
		{
			isDead = true;
			_lastDead = Time.realtimeSinceStartup;

			updateLife();

			if (!Dedicator.IsDedicatedServer)
			{
				ragdoll = newRagdoll;

				RagdollTool.ragdollAnimal(transform.position, transform.rotation, skeleton, ragdoll, id, ragdollEffect);
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

		public void tellState(Vector3 newPosition, float newAngle)
		{
			lastUpdatePos = newPosition;
			lastUpdateAngle = newAngle;

			if (nsb != null)
			{
				nsb.addNewSnapshot(new YawSnapshotInfo(newPosition, newAngle));
			}
#if WITH_NSB_LOGGING
			else if(!Provider.isServer)
			{
				NsbLog.WarningFormat("animal tellState null buffer ({0})", index);
			}
#endif // WITH_NSB_LOGGING

			if (isPlayingEat || isPlayingGlance)
			{
				isPlayingEat = false;
				isPlayingGlance = false;

				animator?.Stop();
			}
		}

		private void updateLife()
		{
			if (controller != null)
			{
				controller.SetDetectCollisionsDeferred(IsAlive);
			}

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				if (renderer_0 != null)
				{
					renderer_0.enabled = IsAlive;
				}

				if (renderer_1 != null)
				{
					renderer_1.enabled = IsAlive;
				}

				if (skeleton != null)
				{
					skeleton.gameObject.SetActive(IsAlive);
				}
			}

			Collider rootCollider = GetComponent<Collider>();
			if (rootCollider != null)
			{
				rootCollider.enabled = IsAlive;
			}
		}

		public void updateStates()
		{
			//position = transform.position;
			//angle = transform.rotation.eulerAngles.y;
			lastUpdatePos = transform.position;
			lastUpdateAngle = transform.rotation.eulerAngles.y;

			if (nsb != null)
			{
				nsb.updateLastSnapshot(new YawSnapshotInfo(transform.position, transform.rotation.eulerAngles.y));
			}
#if WITH_NSB_LOGGING
			else if(!Provider.isServer)
			{
				NsbLog.WarningFormat("animal updateStates null buffer ({0})", index);
			}
#endif // WITH_NSB_LOGGING
		}

		private void reset()
		{
			target = transform.position;

			lastWander = Time.timeAsDouble;
			lastStuck = Time.timeAsDouble;

			isPlayingEat = false;
			isPlayingGlance = false;
			isPlayingStartleAnimation = false;

			isMoving = false;
			isRunning = false;
			_isFleeing = false;
			isWandering = false;
			isHunting = false;
			updateTicking();
			isStuck = false;

			health = asset.health;
		}

		private void move(float delta)
		{
			Vector3 horizontalDirectionToTarget = (target - transform.position).GetHorizontal();
			float horizontalDistanceToTarget = horizontalDirectionToTarget.magnitude;

			bool preventMoving = isPlayingStartleAnimation && asset.ShouldPreventMoveDuringStartle;
			bool newIsMoving = horizontalDistanceToTarget > 0.75f && !preventMoving;
			if ((!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer) && newIsMoving && !isMoving)
			{
				if (isPlayingEat)
				{
					animator?.Stop();
					isPlayingEat = false;
				}

				if (isPlayingGlance)
				{
					animator?.Stop();
					isPlayingGlance = false;
				}
			}
			isMoving = newIsMoving;

			isRunning = isMoving && (isFleeing || isHunting);

			float nearTargetSlowdown = Mathf.Clamp01(horizontalDistanceToTarget / 0.6f);

			Vector3 forward = transform.forward;
			float forwardAlignment = Vector3.Dot(horizontalDirectionToTarget.normalized, forward);
			float newSpeed = (isRunning ? asset.speedRun : asset.speedWalk) * Mathf.Max(forwardAlignment, 0.05f) * nearTargetSlowdown;

			if (Time.deltaTime > 0)
			{
				newSpeed = Mathf.Clamp(newSpeed, 0, horizontalDistanceToTarget / (Time.deltaTime * 2));
			}

			Vector3 newVelocity = forward * newSpeed;
			newVelocity.y = Physics.gravity.y * 2;

			if (!isMoving && !preventMoving)
			{
				newVelocity.x = 0;
				newVelocity.z = 0;

				if (!isStuck)
				{
					_isFleeing = false;
					isWandering = false;
					updateTicking();
				}
			}
			else
			{
				Quaternion rot = transform.rotation;
				Quaternion toTarget = Quaternion.LookRotation(horizontalDirectionToTarget);

				rot = Quaternion.Slerp(rot, toTarget, 8f * delta);
				Vector3 euler = rot.eulerAngles;
				euler.z = 0;
				euler.x = 0;
				rot = Quaternion.Euler(euler);

				transform.rotation = rot;
			}

			UnityEngine.Profiling.Profiler.BeginSample("CharacterController.Move");
			if (!preventMoving && newVelocity.sqrMagnitude > float.Epsilon)
			{
				controller?.Move(newVelocity * delta);
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		private void Update()
		{
			if (isDead)
			{
				return;
			}

			if (Provider.isServer)
			{
				if (!isUpdated)
				{
					if (Mathf.Abs(lastUpdatePos.x - transform.position.x) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatePos.y - transform.position.y) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdatePos.z - transform.position.z) > Provider.UPDATE_DISTANCE || Mathf.Abs(lastUpdateAngle - transform.rotation.eulerAngles.y) > 1)
					{
						lastUpdatePos = transform.position;
						lastUpdateAngle = transform.rotation.eulerAngles.y;

						isUpdated = true;
						AnimalManager.updates++;

						if (isStuck && Time.timeAsDouble - lastStuck > 0.5f)
						{
							isStuck = false;
							lastStuck = Time.timeAsDouble;
						}
					}
					else
					{
						if (isMoving)
						{
							if (Time.timeAsDouble - lastStuck > 0.125f)
							{
								isStuck = true;
							}
						}
						else
						{
							isStuck = false;
							lastStuck = Time.timeAsDouble;
						}
					}
				}
			}
			else
			{
				if (Mathf.Abs(lastUpdatePos.x - transform.position.x) > 0.01f || Mathf.Abs(lastUpdatePos.y - transform.position.y) > 0.01f || Mathf.Abs(lastUpdatePos.z - transform.position.z) > 0.01f)
				{
					if (!isMoving)
					{
						if (isPlayingEat)
						{
							animator?.Stop();
							isPlayingEat = false;
						}

						if (isPlayingGlance)
						{
							animator?.Stop();
							isPlayingGlance = false;
						}
					}
					isMoving = true;

					isRunning = (lastUpdatePos - transform.position).sqrMagnitude > 1; // high freq
				}
				else
				{
					isMoving = false;
					isRunning = false;
				}

				if (nsb != null)
				{
					YawSnapshotInfo info = nsb.getCurrentSnapshot();

					transform.position = info.pos;
					transform.rotation = Quaternion.Euler(0, info.yaw, 0);
				}
#if WITH_NSB_LOGGING
				else if(!Provider.isServer)
				{
					NsbLog.WarningFormat("animal Update null buffer ({0})", index);
				}
#endif // WITH_NSB_LOGGING
			}

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				if (!isMoving && !isPlayingEat && !isPlayingGlance && !isPlayingAttack && !isPlayingStartleAnimation)
				{
					// Nelson 2023-08-18: checking time since last attack is a placeholder-ish fix
					// to prevent idle animations immediately playing after an attack. (public issue #4073)
					if (Time.timeAsDouble - lastAttack > attackInterval + 0.5)
					{
						if (asset.eatAnimVariantsCount > 0 && Time.timeAsDouble - lastEat > eatDelay)
						{
							askEat();
						}
						else if (asset.glanceAnimVariantsCount > 0 && Time.timeAsDouble - lastGlance > glanceDelay)
						{
							askGlance();
						}
					}
				}
			}

			if (Provider.isServer)
			{
				if (isStuck)
				{
					if (Time.timeAsDouble - lastStuck > 0.75f)
					{
						lastStuck = Time.timeAsDouble;

						getWanderTarget();
					}
				}
				else if (!isFleeing && !isHunting)
				{
					if (Time.timeAsDouble - lastWander > wanderDelay)
					{
						lastWander = Time.timeAsDouble;
						wanderDelay = Random.Range(8f, 16f);

						getWanderTarget();
					}
				}
				else
				{
					lastWander = Time.timeAsDouble;
				}
			}

			if (isPlayingEat)
			{
				if (Time.timeAsDouble - lastEat > eatTime)
				{
					isPlayingEat = false;
				}
			}
			else if (isPlayingGlance)
			{
				if (Time.timeAsDouble - lastGlance > glanceTime)
				{
					isPlayingGlance = false;
				}
			}
			else if (isPlayingStartleAnimation)
			{
				if (Time.timeAsDouble > startleAnimationCompletionTime)
				{
					isPlayingStartleAnimation = false;
				}
			}
			else if (isPlayingAttack)
			{
				if (Time.timeAsDouble - lastAttack > attackDuration)
				{
					isPlayingAttack = false;
				}
			}
			else
			{
				if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
				{
					if (isRunning && hasRunAnimation)
					{
						animator?.Play("Run");
					}
					else if (isMoving && hasWalkAnimation)
					{
						animator?.Play("Walk");
					}
					else if (hasIdleAnimation)
					{
						animator?.Play("Idle");
					}
				}
			}

			if (Provider.isServer)
			{
				if (health < asset.health && Time.timeAsDouble - lastRegen > asset.regen)
				{
					lastRegen = Time.timeAsDouble;
					health++; // Potential timing issues, but not big deal. In most cases the server framerate is higher than this update rate.
				}
			}
		}

		/// <summary>
		/// Reduces frequency of UndergroundAllowlist checks because it can be expensive with lots of entities and volumes. 
		/// </summary>
		private float undergroundTestTimer = 10.0f;

		private double lastTick;
		public void tick()
		{
			float delta = (float) (Time.timeAsDouble - lastTick);
			lastTick = Time.timeAsDouble;

			undergroundTestTimer -= delta;
			if (undergroundTestTimer < 0.0f)
			{
				undergroundTestTimer = Random.Range(30.0f, 60.0f);

				UnityEngine.Profiling.Profiler.BeginSample("Underground Test");
				bool isPositionValid = UndergroundAllowlist.IsPositionWithinValidHeight(transform.position);
				UnityEngine.Profiling.Profiler.EndSample();
				if (!isPositionValid)
				{
					AnimalManager.TeleportAnimalBackIntoMap(this);
					return;
				}
			}

			if (isHunting)
			{
				if (currentTargetPlayer != null && currentTargetPlayer.life.IsAlive && currentTargetPlayer.stance.stance != EPlayerStance.SWIM)
				{
					target = currentTargetPlayer.transform.position;

					float distance = MathfEx.HorizontalDistanceSquared(target, transform.position);
					float height = Mathf.Abs(target.y - transform.position.y);

					if (distance < (currentTargetPlayer.movement.getVehicle() != null ? asset.horizontalVehicleAttackRangeSquared : asset.horizontalAttackRangeSquared) && height < asset.verticalAttackRange)
					{
						if (Time.timeAsDouble - lastTarget > (Dedicator.IsDedicatedServer ? 0.3f : 0.1f))
						{
							if (isAttacking)
							{
								if (Time.timeAsDouble - lastAttack > attackDuration * 0.5f)
								{
									isAttacking = false;

									byte damage = asset.damage;
									damage = (byte) (damage * Provider.modeConfigData.Animals.Damage_Multiplier);

									if (currentTargetPlayer.movement.getVehicle() != null)
									{
										if (currentTargetPlayer.movement.getVehicle().asset != null && currentTargetPlayer.movement.getVehicle().asset.isVulnerableToEnvironment)
										{
											VehicleManager.damage(currentTargetPlayer.movement.getVehicle(), damage, 1, true, damageOrigin: EDamageOrigin.Animal_Attack);
										}
									}
									else
									{
										EPlayerKill kill;
										DamagePlayerParameters damagePlayerParameters = new DamagePlayerParameters(currentTargetPlayer);
										damagePlayerParameters.cause = EDeathCause.ANIMAL;
										damagePlayerParameters.killer = Provider.server;
										damagePlayerParameters.direction = (target - transform.position).normalized;
										damagePlayerParameters.damage = damage;
										damagePlayerParameters.respectArmor = true;
										DamageTool.damagePlayer(damagePlayerParameters, out kill);
									}
								}
							}
							else
							{
								if (asset.attackAnimVariantsCount > 0 && Time.timeAsDouble - lastAttack > attackInterval)
								{
									isAttacking = true;

									int animIndex = Random.Range(0, asset.attackAnimVariantsCount);
									AnimalManager.sendAnimalAttack(this, MathfEx.ClampToByte(animIndex));
								}
							}
						}
					}
					else if (distance > 4096)
					{
						currentTargetPlayer = null;
						isHunting = false;
						updateTicking();
					}
					else
					{
						lastTarget = Time.timeAsDouble;
						isAttacking = false;
					}
				}
				else
				{
					currentTargetPlayer = null;
					isHunting = false;
					updateTicking();
				}

				lastWander = Time.timeAsDouble;
			}

			UnityEngine.Profiling.Profiler.BeginSample("Animal.Move");
			move(delta);
			UnityEngine.Profiling.Profiler.EndSample();
		}

		public void init()
		{
			_asset = Assets.find(EAssetType.ANIMAL, id) as AnimalAsset;

			attackDuration = 0.5f;
			attackInterval = asset.attackInterval;
			eatTime = 0.5f;
			glanceTime = 0.5f;

			if (!Dedicator.IsDedicatedServer || asset.shouldPlayAnimsOnDedicatedServer)
			{
				Transform characterTransform = transform.Find("Character");
				if (characterTransform != null)
				{
					animator = characterTransform.GetComponent<Animation>();
					if (animator != null)
					{
						hasIdleAnimation = animator.GetClip("Idle") != null;
						hasRunAnimation = animator.GetClip("Run") != null;
						hasWalkAnimation = animator.GetClip("Walk") != null;

						if (Dedicator.IsDedicatedServer)
						{
							animator.cullingType = AnimationCullingType.AlwaysAnimate;
						}
					}
					else
					{
						_asset.ReportAssetError("missing Animation component on child Character transform");
					}

					skeleton = characterTransform.Find("Skeleton");
					if (skeleton == null)
					{
						_asset.ReportAssetError("missing Skeleton transform child of Character transform");
					}

					renderer_0 = characterTransform.Find("Model_0")?.GetComponent<Renderer>();
					renderer_1 = characterTransform.Find("Model_1")?.GetComponent<Renderer>();
				}
				else
				{
					_asset.ReportAssetError("missing child Character transform with Animation component");
				}
			}

			if (Provider.isServer)
			{
				controller = GetComponent<CharacterController>();
				if (controller != null)
				{
					controller.enableOverlapRecovery = false; // Refer to PlayerMovement's comment.
				}
				else
				{
					Assets.ReportError(asset, "missing CharacterController component");
				}
			}
			else
			{
				nsb = new NetworkSnapshotBuffer<YawSnapshotInfo>(Provider.UPDATE_TIME, Provider.UPDATE_DELAY);
			}

			reset();

			lastEat = Time.timeAsDouble + Random.Range(4f, 16f);
			lastGlance = Time.timeAsDouble + Random.Range(4f, 16f);
			lastWander = Time.timeAsDouble + Random.Range(8f, 32f);

			eatDelay = Random.Range(4f, 8f);
			glanceDelay = Random.Range(4f, 8f);
			wanderDelay = Random.Range(8f, 16f);

			updateLife();
			updateStates();
		}

		// Necessary otherwise playing missing animation will spam log file.
		private bool hasIdleAnimation;
		private bool hasRunAnimation;
		private bool hasWalkAnimation;

		[System.Obsolete("Renamed to PlayStartleAnimation")]
		public void askStartle()
		{
			PlayStartleAnimation(0);
		}
	}
}
