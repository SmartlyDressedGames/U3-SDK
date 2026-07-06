////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define ENABLE_REPLICATED_WHEEL_GIZMOS
// #define ENABLE_WHEEL_PROFILING
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections.Generic;
using UnityEngine;
#if ENABLE_WHEEL_PROFILING
using UnityEngine.Profiling;
#endif

namespace SDG.Unturned
{
	public delegate void WheelAliveChangedHandler(Wheel wheel);

	/// <summary>
	/// When moving between physics materials we need to continue any previous tire kickup particles until they expire.
	/// This class manages the individual effect per-physics-material. Each wheel can have multiple at once. When the
	/// particles have despawned and the effect is no longer needed, the effect game object is returned to the effect
	/// pool and this class is returned to <see cref="Wheel.motionEffectInstancesPool"/>.
	/// </summary>
	internal class TireMotionEffectInstance
	{
		/// <summary>
		/// Name from <see cref="PhysicsTool.GetMaterialName"/>.
		/// </summary>
		public string materialName;
		/// <summary>
		/// Instantiated effect. Null after returning to pool.
		/// </summary>
		private GameObject gameObject;
		/// <summary>
		/// Effect's transform. Null after returning to pool.
		/// </summary>
		public Transform transform;
		/// <summary>
		/// Component on gameObject. Null after returning to pool.
		/// </summary>
		public ParticleSystem particleSystem;
		/// <summary>
		/// Whether this effect should be emitting particles. False stops the particle system immediately, whereas true
		/// only starts playing on the next frame to avoid filling a gap between positions, e.g., after a jump.
		/// </summary>
		public bool isReadyToPlay;
		/// <summary>
		/// Prevents repeated lookups if asset is null, while allowing asset to be looked up each time this effect
		/// becomes active so that it can be iterated on without restarting the game.
		/// </summary>
		public bool hasTriedToInstantiateEffect;

		public void StopParticleSystem()
		{
			if (particleSystem != null)
			{
				particleSystem.Stop();
			}
			isReadyToPlay = false;
		}

		public void DestroyEffect()
		{
			if (gameObject != null)
			{
				EffectManager.DestroyIntoPool(gameObject);
				gameObject = null;
				transform = null;
				particleSystem = null;
				isReadyToPlay = false;
			}
		}

		public void InstantiateEffect()
		{
			hasTriedToInstantiateEffect = true;
			AssetReference<EffectAsset> assetRef = PhysicMaterialCustomData.GetTireMotionEffect(materialName);
			EffectAsset asset = assetRef.Find();
			if (asset != null && asset.effect != null)
			{
				gameObject = EffectManager.InstantiateFromPool(asset);
				transform = gameObject.transform;
				particleSystem = gameObject.GetComponent<ParticleSystem>();
				isReadyToPlay = false;
			}
		}

		public void Reset()
		{
			gameObject = null;
			transform = null;
			particleSystem = null;
			isReadyToPlay = false;
			hasTriedToInstantiateEffect = false;
		}
	}

	public class Wheel
	{
		private InteractableVehicle _vehicle;
		public InteractableVehicle vehicle => _vehicle;

		public int index
		{
			get;
			private set;
		}

		private WheelCollider _wheel;
		public WheelCollider wheel => _wheel;

		private Transform colliderTransform;
		private Vector3 colliderLocalCenter;
		private float colliderRadius;
		private float colliderSuspensionDistance;

		public Transform model;
		public Quaternion rest;

		private ECrawlerTrackForwardMode crawlerTrackForwardMode;

		public bool isPowered;

		internal VehicleWheelConfiguration config;
		internal CrawlerTrackTilingMaterialInstance copyCrawlerTrack;

		/// <summary>
		/// Does this wheel affect brake torque?
		/// </summary>
		public bool hasBrakes;

		private bool _isGrounded;
		public bool isGrounded => _isGrounded;
		internal WheelHit mostRecentGroundHit;

		private bool _isAlive;
		public bool isAlive
		{
			get => _isAlive;
			set
			{
				if (isAlive == value)
				{
					return;
				}
				_isAlive = value;

				if (model != null)
				{
					model.gameObject.SetActive(isAlive);
				}

				UpdateColliderEnabled();

				triggerAliveChanged();
			}
		}

		public bool IsDead => !_isAlive;

		public float stiffnessTractionMultiplier = 0.25f;
		public float stiffnessSideways = 1.0f;
		public float stiffnessForward = 2.0f;
		public float motorTorqueMultiplier = 1.0f;
		public float motorTorqueClampMultiplier = 0.5f;
		public float brakeTorqueMultiplier = 1.0f;
		public float brakeTorqueTractionMultiplier = 0.5f;

		private void triggerAliveChanged()
		{
			aliveChanged?.Invoke(this);
		}

		private float latestLocalSteeringInput;
		private float latestLocalAccelerationInput;
		private bool latestLocalBrakingInput;

		private bool _isPhysical;
		/// <summary>
		/// Turn on/off physics as needed. Overridden by isAlive.
		/// </summary>
		public bool isPhysical
		{
			get => _isPhysical;
			set
			{
				_isPhysical = value;

				UpdateColliderEnabled();
			}
		}

		/// <summary>
		/// [0.0, 1.0] normalized position of wheel along suspension.
		/// </summary>
		internal float replicatedSuspensionState;
		/// <summary>
		/// [0.0, 1.0] normalized position animated toward replicatedSuspensionState.
		/// </summary>
		private float animatedSuspensionState;
		/// <summary>
		/// Model position interpolated toward animatedSuspensionState according to modelSuspensionSpeed.
		/// </summary>
		private float animatedModelSuspension;

		internal PhysicsMaterialNetId replicatedGroundMaterial;

		/// <summary>
		/// [0, 360] angle of rotation around wheel axle. Measured in degrees because Quaternion.AngleAxis takes degrees.
		/// 
		/// We track rather than using GetWorldPose so that we can alternate between using replicated and simulated
		/// results without snapping transforms.
		/// </summary>
		private float rollAngleDegrees;

		/// <summary>
		/// List is created if this wheel has a collider and uses collider pose. Null when vehicle is destroyed to
		/// prevent creation of more effects.
		/// </summary>
		private List<TireMotionEffectInstance> motionEffectInstances;
		/// <summary>
		/// Instance corresponding to current ground material. Doesn't necessarily mean the particle system is active.
		/// </summary>
		private TireMotionEffectInstance currentGroundEffect;
		private static List<TireMotionEffectInstance> motionEffectInstancesPool = new List<TireMotionEffectInstance>();

		public event WheelAliveChangedHandler aliveChanged;

		public void askRepair()
		{
			if (isAlive)
			{
				return;
			}

			isAlive = true;
			vehicle.sendTireAliveMaskUpdate();
		}

		public void askDamage()
		{
			if (!isAlive)
			{
				return;
			}

			isAlive = false;
			vehicle.sendTireAliveMaskUpdate();

			EffectAsset rubber_0 = Rubber_0_Ref.Find();
			if (rubber_0 != null)
			{
				TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(rubber_0);
				triggerEffectParameters.relevantDistance = EffectManager.SMALL;
				triggerEffectParameters.position = colliderTransform.position;
				triggerEffectParameters.SetDirection(colliderTransform.up);
				triggerEffectParameters.reliable = true;
				EffectManager.triggerEffect(triggerEffectParameters);
			}
		}

		private void UpdateColliderEnabled()
		{
			if (wheel != null)
			{
				wheel.gameObject.SetActive(isPhysical && isAlive);
			}
		}

		/// <summary>
		/// Called after construction and on all clients and server when a player stops driving.
		/// </summary>
		internal void Reset()
		{
			latestLocalSteeringInput = 0;
			latestLocalAccelerationInput = 0;
			latestLocalBrakingInput = false;

			if (wheel != null)
			{
				wheel.steerAngle = 0;
				wheel.motorTorque = 0;
				wheel.brakeTorque = vehicle.asset.brake * 0.25f;

				WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
				sidewaysFriction.stiffness = 0.25f;
				wheel.sidewaysFriction = sidewaysFriction;

				WheelFrictionCurve forwardFriction = wheel.forwardFriction;
				forwardFriction.stiffness = 0.25f;
				wheel.forwardFriction = forwardFriction;
			}
		}

		/// <summary>
		/// Called when vehicles explodes.
		/// </summary>
		internal void Explode()
		{
			if (model == null || IsDead)
				return;

			Collider modelCollider = model.GetComponent<Collider>();
			if (modelCollider == null)
				return;

			// Nelson 2024-12-06: Even if wheel can't "explode" we enable the model collider to account for the wheel
			// colider be turned off. This prevents the Explorer from falling through the ground somewhat.
			modelCollider.enabled = true;

			if (!config.canExplode)
				return;

			EffectManager.RegisterDebris(model.gameObject);
			model.transform.parent = null;

			Rigidbody rb = model.gameObject.GetOrAddComponent<Rigidbody>();
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			rb.drag = 0.5f;
			rb.angularDrag = 0.1f;

			Object.Destroy(model.gameObject, 8f);

			if (index % 2 == 0)
			{
				rb.AddForce((-model.right * 512) + (Vector3.up * 128));
			}
			else
			{
				rb.AddForce((model.right * 512) + (Vector3.up * 128));
			}
		}

		internal void UpdateGrounded()
		{
			if (wheel == null)
			{
				return;
			}

			_isGrounded = wheel.GetGroundHit(out mostRecentGroundHit);
			if (_isGrounded)
			{
				string materialName = PhysicsTool.GetMaterialName(mostRecentGroundHit);
				replicatedGroundMaterial = PhysicsMaterialNetTable.GetNetId(materialName);
			}
			else
			{
				replicatedGroundMaterial = PhysicsMaterialNetId.NULL;
			}
		}

		/// <summary>
		/// Called during FixedUpdate if vehicle is driven by the local player.
		/// </summary>
		internal void ClientSimulate(float input_x, float input_y, bool inputBrake, float delta, bool isTorqueBlocked)
		{
			if (wheel == null)
			{
				return;
			}

			latestLocalSteeringInput = input_x;
			latestLocalAccelerationInput = input_y;
			latestLocalBrakingInput = inputBrake;

			if (config.steeringMode == EWheelSteeringMode.CrawlerTrack && isTorqueBlocked)
			{
				// Hack to fix steering tank without fuel (#4825) and underwater (#4830). For acceleration input_y
				// is zero, so it seems fitting to do the same for steering.
				latestLocalSteeringInput = 0.0f;
			}

			UpdateGrounded();
		}

		internal void OnVehicleDestroyed()
		{
			if (motionEffectInstances != null)
			{
				foreach (TireMotionEffectInstance instance in motionEffectInstances)
				{
					instance.DestroyEffect();
					motionEffectInstancesPool.Add(instance);
				}
				motionEffectInstances.Clear();
				motionEffectInstances = null;
				currentGroundEffect = null;
			}

			// Nelson 2024-07-01: Now that wheel debris are moved out of the vehicle hierarchy they are not automatically
			// destroyed at the same time as the vehicle itself. We need to destroy them manually in this case because
			// they may share a material with the base vehicle. (public issue #4550)
			if (model != null && model.transform.parent == null)
			{
				Object.Destroy(model.gameObject);
			}
		}

		/// <summary>
		/// Calculate suspension state from GetWorldPose result.
		/// 
		/// Nelson 2024-03-25: Originally we used the result of GetWorldPose for the model transform and calculated
		/// the suspension state from it because I thought Unity was internally using the spring position that isn't
		/// (currently) exposed to the API. Whether or not it is, it seems fine to calculate the spring position using
		/// the ground hit point instead. We switched entirely away from GetWorldPose so that the wheel can retain
		/// its roll angle when transitioning between locally simulated and replicated.
		/// </summary>
		private float CalculateNormalizedSuspensionPosition(Vector3 worldPosePosition)
		{
			if (colliderSuspensionDistance > float.Epsilon)
			{
				Vector3 colliderWorldCenter = colliderTransform.TransformPoint(colliderLocalCenter);
				Vector3 colliderWorldDownward = -colliderTransform.up;
				float distanceAlongSuspension = Vector3.Dot(worldPosePosition - colliderWorldCenter, colliderWorldDownward);
				return Mathf.Clamp01(distanceAlongSuspension / colliderSuspensionDistance);
			}
			else
			{
				return 0.0f;
			}
		}

		private float CalculateNormalizedSuspensionPosition(float distanceAlongSuspension)
		{
			if (colliderSuspensionDistance > float.Epsilon)
			{
				return Mathf.Clamp01(distanceAlongSuspension / colliderSuspensionDistance);
			}
			else
			{
				return 0.0f;
			}
		}

		private TireMotionEffectInstance FindOrAddMotionEffect(string materialName)
		{
			foreach (TireMotionEffectInstance instance in motionEffectInstances)
			{
				if (instance.materialName == materialName)
				{
					return instance;
				}
			}

			TireMotionEffectInstance newInstance;
			if (motionEffectInstancesPool.Count > 0)
			{
				newInstance = motionEffectInstancesPool.GetAndRemoveTail();
				newInstance.Reset();
			}
			else
			{
				newInstance = new TireMotionEffectInstance();
			}
			newInstance.materialName = materialName;
			motionEffectInstances.Add(newInstance);
			return newInstance;
		}

		private void UpdateMotionEffect(Vector3 groundHitPosition, bool isVisualGrounded)
		{
			if (motionEffectInstances == null)
				return;

			string groundMaterialName = PhysicsMaterialNetTable.GetMaterialName(replicatedGroundMaterial);
			TireMotionEffectInstance newMotionEffect;
			if (string.IsNullOrEmpty(groundMaterialName))
			{
				newMotionEffect = null;
			}
			else
			{
				if (currentGroundEffect == null || currentGroundEffect.materialName != groundMaterialName)
				{
					newMotionEffect = FindOrAddMotionEffect(groundMaterialName);
				}
				else
				{
					// Current effect name matches, continue.
					newMotionEffect = currentGroundEffect;
				}
			}

			if (currentGroundEffect != newMotionEffect)
			{
				if (currentGroundEffect != null)
				{
					currentGroundEffect.StopParticleSystem();
				}

				currentGroundEffect = newMotionEffect;

				if (currentGroundEffect != null)
				{
					currentGroundEffect.hasTriedToInstantiateEffect = false;
				}
			}

			if (currentGroundEffect != null)
			{
				if (isVisualGrounded)
				{
					if (!currentGroundEffect.hasTriedToInstantiateEffect)
					{
						// Hold off on trying to instantiate effect if vehicle isn't actually moving. This saves on
						// instantiating effects for every single wheel in the level.
						if (!MathfEx.IsNearlyZero(vehicle.ReplicatedForwardVelocity, 0.1f))
						{
							currentGroundEffect.InstantiateEffect();
						}
					}

					if (currentGroundEffect.particleSystem != null)
					{
						float sign = Mathf.Sign(vehicle.AnimatedForwardVelocity);
						bool shouldPlay;
						switch (config.motionEffectsMode)
						{
							default:
							case EWheelMotionEffectsMode.BothDirections:
								shouldPlay = true;
								break;

							case EWheelMotionEffectsMode.ForwardOnly:
								shouldPlay = sign >= 0.0f;
								break;

							case EWheelMotionEffectsMode.BackwardOnly:
								shouldPlay = sign <= 0.0f;
								break;
						}

						if (shouldPlay)
						{
							Vector3 up = colliderTransform.up;
							Vector3 backward = colliderTransform.forward * -sign;
							// Aim particles upward at slow speeds and approach 45 degrees in opposite direction at high speeds.
							float blendWeight = vehicle.GetAnimatedForwardSpeedPercentageOfTargetSpeed() * 0.5f;
							Vector3 alignDirection = Vector3.Lerp(up, backward, blendWeight);
							Quaternion alignRotation = Quaternion.LookRotation(alignDirection);
							currentGroundEffect.transform.SetPositionAndRotation(groundHitPosition, alignRotation);

							// Important: We only Play the particle system the first frame after it becomes "active" so that
							// distance-based emission doesn't fill the gap, e.g., when going off a ramp.
							if (currentGroundEffect.isReadyToPlay && !currentGroundEffect.particleSystem.isPlaying)
							{
								currentGroundEffect.particleSystem.Play();
							}
							currentGroundEffect.isReadyToPlay = true;
						}
						else
						{
							currentGroundEffect.StopParticleSystem();
						}
					}
				}
				else
				{
					currentGroundEffect.StopParticleSystem();
				}
			}

			for (int removalIndex = motionEffectInstances.Count - 1; removalIndex >= 0; --removalIndex)
			{
				TireMotionEffectInstance instance = motionEffectInstances[removalIndex];
				if (instance == currentGroundEffect)
				{
					// Hold onto current effect even if particles are dead. Otherwise, we end up in a loop of creating
					// and destroying it because particles might not be getting emitted due to lack of motion.
					// (Intention is for effects to use "spawn by distance" mode.)
					continue;
				}

				// Nelson 2024-07-16: Experimented with Ring Buffer particle system option to ensure wheels continue
				// emitting particles at high speeds. Unfortunately, IsAlive() never returns false in that case, so
				// the instances don't get cleaned up. (public issue #4590)
				if (instance.particleSystem == null || !instance.particleSystem.IsAlive())
				{
					// UnturnedLog.info($"{instance.materialName} particles destroyed");
					instance.DestroyEffect();
					motionEffectInstances.RemoveAtFast(removalIndex);
					motionEffectInstancesPool.Add(instance);
				}
			}
		}

		/// <summary>
		/// Called during Update on dedicated server only if replicated suspension state is enabled.
		/// </summary>
		internal void UpdateServerSuspensionAndPhysicsMaterial()
		{
			if (_wheel != null)
			{
				float distanceAlongSuspension;
				_isGrounded = _wheel.GetGroundHit(out mostRecentGroundHit);
				if (_isGrounded)
				{
					Vector3 colliderWorldCenter = colliderTransform.TransformPoint(colliderLocalCenter);
					// Nelson 2024-07-30: Initially this used the collider's down vector, but as far as I can tell a
					// limitation of the physics engine is that all wheels share the same down vector. This came up
					// for the bicycle and dirtbike which have angled front axles.
					Vector3 colliderWorldDownward = -vehicle.transform.up;
					Vector3 hitWorldPosition = mostRecentGroundHit.point;
					float hitDistanceAlongSuspension = Vector3.Dot(hitWorldPosition - colliderWorldCenter, colliderWorldDownward);
					distanceAlongSuspension = hitDistanceAlongSuspension - colliderRadius;

					string materialName = PhysicsTool.GetMaterialName(mostRecentGroundHit);
					replicatedGroundMaterial = PhysicsMaterialNetTable.GetNetId(materialName);
				}
				else
				{
					distanceAlongSuspension = colliderSuspensionDistance;
					replicatedGroundMaterial = PhysicsMaterialNetId.NULL;
				}
				replicatedSuspensionState = CalculateNormalizedSuspensionPosition(distanceAlongSuspension);
			}
		}

		/// <summary>
		/// Set replicated suspension state AND animated suspension state when vehicle is first received.
		/// </summary>
		/// <param name="state"></param>
		internal void TeleportSuspensionState(float state)
		{
			replicatedSuspensionState = state;
			animatedSuspensionState = state;
			if (wheel != null)
			{
				animatedModelSuspension = state * colliderSuspensionDistance;
			}
		}

		/// <summary>
		/// Supported when locally simulated and on remote clients.
		/// </summary>
		internal float CalculateWheelSpeed()
		{
			if (_wheel != null && isPhysical)
			{
				float rotationsPerMinute = _wheel.rpm;
				float rotationsPerSecond = rotationsPerMinute / 60.0f;
				float wheelCircumference = 2.0f * Mathf.PI * colliderRadius;
				return rotationsPerSecond * wheelCircumference;
			}
			else
			{
				return 0.0f;
			}
		}

		/// <summary>
		/// Called during Update on client.
		/// </summary>
		internal void UpdateModel(float deltaTime)
		{
			if (config.modelUseColliderPose && _wheel != null)
			{
				Vector3 colliderWorldCenter = colliderTransform.TransformPoint(colliderLocalCenter);
				// Nelson 2024-07-30: Initially this used the collider's down vector, but as far as I can tell a
				// limitation of the physics engine is that all wheels share the same down vector. This came up
				// for the bicycle and dirtbike which have angled front axles.
				Vector3 colliderWorldDownward = -vehicle.transform.up;

				Vector3 visualWorldDownward = -colliderTransform.up;

				if (_isPhysical)
				{
					BeginSample("Physical");
					float distanceAlongSuspension;
					BeginSample("GetGroundHit");
					bool isGrounded = _wheel.GetGroundHit(out WheelHit groundHit);
					EndSample();
					if (isGrounded)
					{
						Vector3 hitWorldPosition = groundHit.point;
						float hitDistanceAlongSuspension = Vector3.Dot(hitWorldPosition - colliderWorldCenter, colliderWorldDownward);
						distanceAlongSuspension = hitDistanceAlongSuspension - colliderRadius;

						BeginSample("GetMaterialName");
						string materialName = PhysicsTool.GetMaterialName(groundHit);
						EndSample();
						replicatedGroundMaterial = PhysicsMaterialNetTable.GetNetId(materialName);
						BeginSample("UpdateMotionEffect");
						UpdateMotionEffect(hitWorldPosition, true);
						EndSample();
					}
					else
					{
						distanceAlongSuspension = colliderSuspensionDistance;

						replicatedGroundMaterial = PhysicsMaterialNetId.NULL;
						BeginSample("UpdateMotionEffect");
						UpdateMotionEffect(Vector3.zero, false);
						EndSample();
					}

					MoveModelSuspension(distanceAlongSuspension, deltaTime);
					Vector3 visualModelOffset = Vector3.Project(colliderWorldDownward * (animatedModelSuspension - config.modelSuspensionOffset), visualWorldDownward);
					Vector3 newModelWorldPosition = colliderWorldCenter + visualModelOffset;

					float rotationDeltaDegrees;
					if (copyCrawlerTrack != null)
					{
						float arcLength = copyCrawlerTrack.speed * deltaTime;
						float wheelCircumference = 2.0f * Mathf.PI * colliderRadius;
						float normalizedAngle = arcLength / wheelCircumference;
						rotationDeltaDegrees = normalizedAngle * 360.0f;
					}
					else
					{
						float rotationsPerMinute = _wheel.rpm;
						float rotationsPerSecond = rotationsPerMinute / 60.0f;
						rotationDeltaDegrees = rotationsPerSecond * 360.0f * deltaTime;
					}
					rollAngleDegrees += rotationDeltaDegrees;
					rollAngleDegrees = ((rollAngleDegrees % 360.0f) + 360.0f) % 360.0f;

					Quaternion newLocalRotation = rest;
					newLocalRotation = Quaternion.AngleAxis(rollAngleDegrees, Vector3.right) * newLocalRotation;
					newLocalRotation = Quaternion.AngleAxis(wheel.steerAngle, Vector3.up) * newLocalRotation;
					Quaternion newWorldRotation = model.parent.TransformRotation(newLocalRotation);

					BeginSample("SetModelPositionAndRotation");
					model.SetPositionAndRotation(newModelWorldPosition, newWorldRotation);
					EndSample();

					replicatedSuspensionState = CalculateNormalizedSuspensionPosition(distanceAlongSuspension);
					animatedSuspensionState = replicatedSuspensionState;
					EndSample(); // UpdatePhysicalWheel
				}
				else
				{
					BeginSample("NonPhysical");
					// 1 is 50% per second, 2 is 75%/s, 3 is 87.5%/s, etc.
					// Nelson 2024-12-03: The purpose of THIS blend is to smooth out the time between network updates,
					// whereas the animatedModelSuspension lerp is at a constant speed.
					const float BLEND_SPEED = 13.0f;
					float lerpWeight = 1.0f - Mathf.Pow(2.0f, -BLEND_SPEED * Time.deltaTime);
					animatedSuspensionState = Mathf.Lerp(animatedSuspensionState, replicatedSuspensionState, lerpWeight);
					float distanceAlongSuspension = animatedSuspensionState * colliderSuspensionDistance;

					MoveModelSuspension(distanceAlongSuspension, deltaTime);
					Vector3 visualModelOffset = Vector3.Project(colliderWorldDownward * (animatedModelSuspension - config.modelSuspensionOffset), visualWorldDownward);
					Vector3 newModelWorldPosition = colliderWorldCenter + visualModelOffset;

					if (colliderRadius > float.Epsilon)
					{
						float arcLength = vehicle.AnimatedForwardVelocity * deltaTime;
						float wheelCircumference = 2.0f * Mathf.PI * colliderRadius;
						float normalizedAngle = arcLength / wheelCircumference;
						float rotationDeltaDegrees = normalizedAngle * 360.0f;
						rollAngleDegrees += rotationDeltaDegrees;
						rollAngleDegrees = ((rollAngleDegrees % 360.0f) + 360.0f) % 360.0f;
					}

					Quaternion newLocalRotation = rest;
					newLocalRotation = Quaternion.AngleAxis(rollAngleDegrees, Vector3.right) * newLocalRotation;
					if (config.steeringMode == EWheelSteeringMode.SteeringAngle)
					{
						float displaySteeringAngle = vehicle.AnimatedSteeringAngle;
						displaySteeringAngle *= config.steeringAngleMultiplier;
						newLocalRotation = Quaternion.AngleAxis(displaySteeringAngle, Vector3.up) * newLocalRotation;
					}
					Quaternion newWorldRotation = model.parent.TransformRotation(newLocalRotation);

					BeginSample("SetModelPositionAndRotation");
					model.SetPositionAndRotation(newModelWorldPosition, newWorldRotation);
					EndSample();

					BeginSample("UpdateMotionEffect");
					if (animatedSuspensionState < 0.99f)
					{
						UpdateMotionEffect(colliderWorldCenter + colliderWorldDownward * (distanceAlongSuspension + colliderRadius), true);
					}
					else
					{
						UpdateMotionEffect(Vector3.zero, false);
					}
					EndSample(); // UpdateMotionEffect

#if ENABLE_REPLICATED_WHEEL_GIZMOS
					RuntimeGizmos gizmos = RuntimeGizmos.Get();
					gizmos.Line(colliderWorldCenter,
						colliderWorldCenter + colliderWorldDownward * _wheel.suspensionDistance,
						Color.gray);

					gizmos.Circle(colliderWorldCenter + colliderWorldDownward * (replicatedSuspensionState * wheel.suspensionDistance),
						_wheel.transform.forward,
						_wheel.transform.up,
						_wheel.radius,
						Color.blue);

					gizmos.Circle(newWorldPosition,
						_wheel.transform.forward,
						_wheel.transform.up,
						_wheel.radius,
						Color.red);
#endif // ENABLE_REPLICATED_WHEEL_GIZMOS

					EndSample(); // NonPhysical
				}
			}
			else
			{
				BeginSample("UpdateWithoutCollider");
				if (config.modelRadius > float.Epsilon)
				{
					if (_isPhysical && config.copyColliderRpmIndex >= 0)
					{
						Wheel copyWheel = vehicle.GetWheelAtIndex(config.copyColliderRpmIndex);
						if (copyWheel != null && copyWheel.wheel != null && copyWheel.colliderRadius > float.Epsilon)
						{
							float rotationsPerMinute = copyWheel.colliderRadius * copyWheel.wheel.rpm / config.modelRadius;
							float rotationsPerSecond = rotationsPerMinute / 60.0f;
							float rotationDeltaDegrees = rotationsPerSecond * 360.0f * deltaTime;
							rollAngleDegrees += rotationDeltaDegrees;
							rollAngleDegrees = ((rollAngleDegrees % 360.0f) + 360.0f) % 360.0f;
						}
					}
					else
					{
						float arcLength = vehicle.AnimatedForwardVelocity * deltaTime;
						float wheelCircumference = 2.0f * Mathf.PI * config.modelRadius;
						float normalizedAngle = arcLength / wheelCircumference;
						float rotationDeltaDegrees = normalizedAngle * 360.0f;
						rollAngleDegrees += rotationDeltaDegrees;
						rollAngleDegrees = ((rollAngleDegrees % 360.0f) + 360.0f) % 360.0f;
					}
				}
				else
				{
					// Nelson 2024-03-25: Yep rotating at 45 degrees per meter is weird, but maintains the old behavior.
					// (collider might not exist for this wheel)
					rollAngleDegrees += vehicle.AnimatedForwardVelocity * 45.0f * deltaTime;
					rollAngleDegrees = ((rollAngleDegrees % 360.0f) + 360.0f) % 360.0f;
				}

				model.localRotation = rest;
				if (config.isModelSteered)
				{
					float displaySteeringAngle = vehicle.AnimatedSteeringAngle;
					displaySteeringAngle *= config.steeringAngleMultiplier;
					model.Rotate(0, displaySteeringAngle, 0, Space.Self);
				}
				model.Rotate(rollAngleDegrees, 0, 0, Space.Self);
				EndSample();
			}
		}

		/// <summary>
		/// Called during Update if vehicle is driven by the local player.
		/// </summary>
		internal void UpdateLocallyDriven(float delta, float availableTorque)
		{
			if (wheel == null)
			{
				return;
			}

			if (config.steeringMode == EWheelSteeringMode.SteeringAngle)
			{
				// Nelson 2024-03-19: My instinct was to move all of this into the FixedUpdate code, but there are a
				// few important caveats: 1. ClientSimulate is not called every FixedUpdate *yet*, only on "simulation"
				// frames. 2. GetWorldPose doesn't seem to be interpolation the steering angle, so we'll need our own
				// fix for that.
				float maxSteeringAngleMagnitude = Mathf.Lerp(vehicle.asset.MaxSteeringAngle, vehicle.asset.MaxSteeringAngleAtFullSpeed,
					vehicle.GetReplicatedForwardSpeedPercentageOfTargetSpeed());
				float targetSteeringAngle = latestLocalSteeringInput * maxSteeringAngleMagnitude;
				targetSteeringAngle *= config.steeringAngleMultiplier;
				float steeringAngleMaxDelta = vehicle.asset.SteeringAngleTurnSpeed * delta;
				wheel.steerAngle = Mathf.MoveTowards(wheel.steerAngle, targetSteeringAngle, steeringAngleMaxDelta);
			}

			float targetMotorTorque;
			bool wantsToChangeDirections;
			bool wantsToIdle = false;
			if (latestLocalAccelerationInput > 0.01f)
			{
				if (vehicle.ReplicatedForwardVelocity > -0.05f)
				{
					if (vehicle.asset.UsesEngineRpmAndGears)
					{
						// Nelson 2024-07-16: Multiply by input to support "stamina" boost. (public issue #4589)
						targetMotorTorque = availableTorque * latestLocalAccelerationInput;
					}
					else
					{
						targetMotorTorque = vehicle.asset.TargetForwardVelocity * latestLocalAccelerationInput * motorTorqueMultiplier;
						if (vehicle.ReplicatedForwardVelocity > vehicle.asset.TargetForwardVelocity)
						{
							targetMotorTorque *= motorTorqueClampMultiplier;
						}
					}
					wantsToChangeDirections = false;
				}
				else
				{
					targetMotorTorque = 0.0f;
					wantsToChangeDirections = true;
				}
			}
			else if (latestLocalAccelerationInput < -0.01f)
			{
				if (vehicle.ReplicatedForwardVelocity < 0.05f)
				{
					if (vehicle.asset.UsesEngineRpmAndGears)
					{
						// Nelson 2024-07-16: Multiply by input to support "stamina" boost. (public issue #4589)
						targetMotorTorque = availableTorque * latestLocalAccelerationInput;
					}
					else
					{
						targetMotorTorque = vehicle.asset.TargetReverseVelocity * -latestLocalAccelerationInput * motorTorqueMultiplier;
						if (vehicle.ReplicatedForwardVelocity < vehicle.asset.TargetReverseVelocity)
						{
							targetMotorTorque *= motorTorqueClampMultiplier;
						}
					}
					wantsToChangeDirections = false;
				}
				else
				{
					targetMotorTorque = 0.0f;
					wantsToChangeDirections = true;
				}
			}
			else
			{
				targetMotorTorque = 0.0f;
				wantsToChangeDirections = false;
				wantsToIdle = true;
			}

			float newMotorTorque = isPowered ? targetMotorTorque : 0.0f;
			float sidewaysStiffnessMultiplier = 1.0f;
			if (config.steeringMode == EWheelSteeringMode.CrawlerTrack)
			{
				// Helps keep the vehicle under control as it speeds up by reducing steering influence.
				float speedMultiplier = Mathf.Lerp(1.0f, vehicle.asset.CrawlerTrackSteeringMaxSpeedScale,
					vehicle.GetReplicatedForwardSpeedPercentageOfTargetSpeed());

				if (!MathfEx.IsNearlyZero(latestLocalSteeringInput))
				{
					wantsToIdle = false;
					float targetStiffnessMultiplier = vehicle.asset.CrawlerTrackSteeringSidewaysFrictionMultiplier;
					float lerpWeight = Mathf.Abs(latestLocalSteeringInput) * speedMultiplier;
					sidewaysStiffnessMultiplier *= Mathf.Lerp(1.0f, targetStiffnessMultiplier, lerpWeight);
				}

				// Nelson 2024-12-02: By default, turning while driving backwards is inverted from how players would
				// expect from cars. I experimented with using the vehicle's forward velocity to detect when to invert,
				// but this created a feedback loop where the vehicle would alternate turning left/right. Using the
				// acceleration input seems like a good compromise to me.
				float direction = 1.0f;
				if (latestLocalAccelerationInput < -0.01f)
				{
					direction = -1.0f;
				}

				float maxSteeringTorque = vehicle.asset.CrawlerTrackSteeringTorque * speedMultiplier;

				switch (crawlerTrackForwardMode)
				{
					case ECrawlerTrackForwardMode.Clockwise:
						newMotorTorque += direction * latestLocalSteeringInput * maxSteeringTorque;
						break;

					case ECrawlerTrackForwardMode.CounterClockwise:
						newMotorTorque -= direction * latestLocalSteeringInput * maxSteeringTorque;
						break;
				}
			}
			wheel.motorTorque = newMotorTorque;

			if (hasBrakes && (wantsToChangeDirections || latestLocalBrakingInput))
			{
				float torqueMultiplier = Mathf.Lerp(1.0f, brakeTorqueTractionMultiplier, vehicle.slip);
				torqueMultiplier *= brakeTorqueMultiplier;
				wheel.brakeTorque = vehicle.asset.brake * torqueMultiplier;
			}
			else if (wantsToIdle)
			{
				// Nelson 2024-03-05: Added this very slight amount of braking while not trying to accelerate because
				// I found sometimes the car was slightly wiggling or vibrating while stopped.
				wheel.brakeTorque = 1.0f;
			}
			else
			{
				wheel.brakeTorque = 0.0f;
			}

			WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
			WheelFrictionCurve forwardFriction = wheel.forwardFriction;

			if (vehicle.asset.hasSleds)
			{
				sidewaysFriction.stiffness = Mathf.Lerp(wheel.sidewaysFriction.stiffness, 0.25f, 4 * delta);
				forwardFriction.stiffness = Mathf.Lerp(wheel.forwardFriction.stiffness, 0.25f, 4 * delta);
			}
			else
			{
				float stiffnessMultiplier = Mathf.Lerp(1.0f, stiffnessTractionMultiplier, vehicle.slip);
				sidewaysFriction.stiffness = Mathf.Lerp(wheel.sidewaysFriction.stiffness, stiffnessSideways * stiffnessMultiplier, 4 * delta);
				forwardFriction.stiffness = Mathf.Lerp(wheel.forwardFriction.stiffness, stiffnessForward * stiffnessMultiplier, 4 * delta);
			}
			sidewaysFriction.stiffness *= sidewaysStiffnessMultiplier;

			wheel.sidewaysFriction = sidewaysFriction;
			wheel.forwardFriction = forwardFriction;

			if (clEnableWheeledVehicleGizmos)
			{
				string debugText = $"M: {wheel.motorTorque:N1}\nB: {wheel.brakeTorque:N1}\n";
				debugText += $"RPM: {wheel.rpm:N1}\n";
				float debugCircumference = 2.0f * Mathf.PI * wheel.radius;
				float debugMetersPerMin = debugCircumference * wheel.rpm;
				float debugMetersPerHour = debugMetersPerMin * 60.0f;
				float debugKilometersPerHour = debugMetersPerHour / 1000.0f;
				debugText += $"KPH: {(debugKilometersPerHour):N1}";
				if (isGrounded)
				{
					debugText += $"\nSlip: {mostRecentGroundHit.forwardSlip:N1}";
				}
				RuntimeGizmos.Get().Label(wheel.transform.position, debugText);
			}
		}

		/// <summary>
		/// Called during Update on the server while vehicle is driven by player.
		/// </summary>
		internal void CheckForTraps()
		{
			RaycastHit hit;
			Physics.Raycast(new Ray(colliderTransform.position, -colliderTransform.up), out hit, colliderSuspensionDistance + colliderRadius, RayMasks.BARRICADE);
			if (hit.transform != null && hit.transform.CompareTag("Barricade"))
			{
				InteractableTrapDamageTires trap = hit.transform.GetComponent<InteractableTrapDamageTires>();
				if (trap != null)
				{
					askDamage();
				}
			}
		}

		internal Wheel(InteractableVehicle newVehicle, int newIndex, WheelCollider newWheel, Transform newModel, VehicleWheelConfiguration newConfiguration)
		{
			_vehicle = newVehicle;
			index = newIndex;
			_wheel = newWheel;
			model = newModel;
			config = newConfiguration;

			if (wheel != null)
			{
				// Nelson 2025-04-22: running UpdateModel on every wheel every frame was taking ~0.48 ms for 32 vehicles
				// on PEI on my PC. Caching these calls to the WheelCollider API saves ~0.15 ms!
				colliderTransform = _wheel.transform;
				colliderRadius = wheel.radius;
				colliderLocalCenter = wheel.center;
				colliderSuspensionDistance = wheel.suspensionDistance;

				if (config.wasAutomaticallyGenerated)
				{
					// Nelson 2024-03-05: Perhaps all the way back to Unity adding WheelCollider.forceAppPointDistance
					// this has overridden it to zero, but if wheels have been manually configured with wheel config
					// we should trust the creator has tuned this value accordingly.
					wheel.forceAppPointDistance = 0;
				}
				replicatedSuspensionState = wheel.suspensionSpring.targetPosition;
				animatedSuspensionState = replicatedSuspensionState;
				animatedModelSuspension = wheel.suspensionSpring.targetPosition * wheel.suspensionDistance;

				if (config.motionEffectsMode != EWheelMotionEffectsMode.None)
				{
					motionEffectInstances = new List<TireMotionEffectInstance>();
				}
				currentGroundEffect = null;
			}

			if (config.steeringMode == EWheelSteeringMode.CrawlerTrack)
			{
				switch (config.crawlerTrackForwardMode)
				{
					case ECrawlerTrackForwardMode.Auto:
					{
						if (newWheel != null)
						{
							Vector3 positionRelativeToVehicle = newVehicle.transform.InverseTransformPoint(newWheel.transform.position);
							if (positionRelativeToVehicle.x < 0.0f)
							{
								crawlerTrackForwardMode = ECrawlerTrackForwardMode.Clockwise;
							}
							else
							{
								crawlerTrackForwardMode = ECrawlerTrackForwardMode.CounterClockwise;
							}
						}
						else
						{
							Assets.ReportError(vehicle.asset, $"wheel at index {index} has Auto CrawlerTrackForwardMode without a collider");
						}
						break;
					}

					default:
					case ECrawlerTrackForwardMode.Clockwise:
					case ECrawlerTrackForwardMode.CounterClockwise:
						crawlerTrackForwardMode = config.crawlerTrackForwardMode;
						break;
				}
			}

			isPowered = config.isColliderPowered;
			hasBrakes = true; // Legacy

			isAlive = true;

			if (model != null)
			{
				rest = model.localRotation;
			}
		}

		private void MoveModelSuspension(float target, float deltaTime)
		{
			if (config.modelSuspensionSpeed < -0.01f)
			{
				animatedModelSuspension = target;
			}
			else
			{
				// Nelson 2024-12-11: Changed this to snap to target if target is less than animated. This fixes the
				// wheel from sinking through the ground (in singleplayer at least), while preventing the bad look of
				// the wheel teleporting downward when going off a ramp.
				if (target < animatedModelSuspension)
				{
					animatedModelSuspension = target;
				}
				else
				{
					float modelDelta = config.modelSuspensionSpeed * deltaTime;
					animatedModelSuspension = Mathf.MoveTowards(animatedModelSuspension, target, modelDelta);
				}
			}
		}

		internal static CommandLineFlag clEnableWheeledVehicleGizmos = new CommandLineFlag(false, "-EnableWheeledVehicleGizmos");
		private static readonly AssetReference<EffectAsset> Rubber_0_Ref = new AssetReference<EffectAsset>("a87c5007b22542dcbf3599ee3faceadd"); // (138)

		#region Obsolete
		[System.Obsolete("Should not have been public.")]
		public void checkForTraps()
		{
			CheckForTraps();
		}

		[System.Obsolete("Should not have been public.")]
		public void update(float delta)
		{
			UpdateLocallyDriven(delta, 0.0f);
		}

		[System.Obsolete("Should not have been public.")]
		public void simulate(float input_x, float input_y, bool inputBrake, float delta)
		{
			ClientSimulate(input_x, input_y, inputBrake, delta, false);
		}

		[System.Obsolete("Should not have been public.")]
		public void reset()
		{
			Reset();
		}
		#endregion Obsolete

		[System.Diagnostics.Conditional("ENABLE_WHEEL_PROFILING")]
		private void BeginSample(string name)
		{
#if ENABLE_WHEEL_PROFILING
			Profiler.BeginSample(name);
#endif
		}

		[System.Diagnostics.Conditional("ENABLE_WHEEL_PROFILING")]
		private void EndSample()
		{
#if ENABLE_WHEEL_PROFILING
			Profiler.EndSample();
#endif
		}
	}
}
