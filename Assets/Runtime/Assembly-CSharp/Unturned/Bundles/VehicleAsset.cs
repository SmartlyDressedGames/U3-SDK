////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public class TurretInfo
	{
		public byte seatIndex;
		public ushort itemID;
		public float yawMin;
		public float yawMax;
		public float pitchMin;
		public float pitchMax;
		public bool useAimCamera;
	}

	public enum EBatteryMode
	{
		None,
		Burn,
		Charge
	}

	/// <summary>
	/// Controls whether vehicle allows barricades to be attached to it.
	/// </summary>
	public enum EVehicleBuildablePlacementRule
	{
		/// <summary>
		/// Vehicle does not override placement. This means, by default, that barricades can be placed on the vehicle
		/// unless the barricade sets Allow_Placement_On_Vehicle to false. (e.g., beds and sentry guns) Note that
		/// gameplay config Bypass_Buildable_Mobility, if true, takes priority.
		/// </summary>
		None,

		/// <summary>
		/// Vehicle allows any barricade to be placed on it, regardless of the barricade's Allow_Placement_On_Vehicle
		/// setting. The legacy option for this was the Supports_Mobile_Buildables flag. Vanilla trains originally
		/// used this option, but it was exploited to move beds into tunnel walls.
		/// </summary>
		AlwaysAllow,

		/// <summary>
		/// Vehicle prevents any barricade from being placed on it. Note that gameplay config Bypass_Buildable_Mobility,
		/// if true, takes priority.
		/// </summary>
		Block,
	}

	public struct PaintableVehicleSection : IDatParseable
	{
		/// <summary>
		/// Scene hierarchy path relative to vehicle root.
		/// </summary>
		public string path;

		/// <summary>
		/// Index in renderer's materials array.
		/// </summary>
		public int materialIndex;

		/// <summary>
		/// If true, apply to every item in renderer's materials array.
		/// </summary>
		public bool allMaterials;

		public bool TryParse(IDatNode node)
		{
			if (node is DatValue pathValue)
			{
				path = pathValue.value;
				materialIndex = -1;
				allMaterials = true;
				return !string.IsNullOrEmpty(path);
			}
			else if (node is IDatDictionary dictionary)
			{
				path = dictionary.GetString("Path");
				materialIndex = dictionary.ParseInt32("MaterialIndex");
				allMaterials = dictionary.ParseBool("AllMaterials");
				return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Controls how vehicle's default paint color (if applicable) is chosen.
	/// </summary>
	internal enum EVehicleDefaultPaintColorMode
	{
		/// <summary>
		/// Not configured.
		/// </summary>
		None,

		/// <summary>
		/// Pick from the DefaultPaintColors list.
		/// </summary>
		List,

		/// <summary>
		/// Pick a random HSV using VehicleRandomPaintColorConfiguration.
		/// </summary>
		RandomHueOrGrayscale,
	}

	internal enum EWheelSteeringMode
	{
		/// <summary>
		/// Wheel does not affect steering.
		/// </summary>
		None,

		/// <summary>
		/// Set steering angle according to <see cref="VehicleAsset.MaxSteeringAngleAtFullSpeed"/> and <see cref="VehicleAsset.MaxSteeringAngle"/>.
		/// </summary>
		SteeringAngle,

		/// <summary>
		/// Increase or decrease motor torque to rotate vehicle in-place. (Tanks)
		/// </summary>
		CrawlerTrack,
	}

	/// <summary>
	/// For <see cref="EWheelSteeringMode.CrawlerTrack"/>, indicates how a positive motor torque (forward) rotates
	/// the vehicle.
	/// </summary>
	internal enum ECrawlerTrackForwardMode
	{
		/// <summary>
		/// Wheels on the left side are Clockwise and wheels on the right side are Counter-Clockwise.
		/// </summary>
		Auto,

		/// <summary>
		/// Positive motor torque on this wheel rotates the vehicle clockwise.
		/// </summary>
		Clockwise,

		/// <summary>
		/// Positive motor torque on this wheel rotates the vehicle counter-clockwise.
		/// </summary>
		CounterClockwise,
	}

	/// <summary>
	/// Controls whether wheel creates particle kickup effects for the ground surface material underneath.
	/// </summary>
	internal enum EWheelMotionEffectsMode
	{
		/// <summary>
		/// Turn off motion effects. Default for wheels not using collider pose.
		/// </summary>
		None,

		/// <summary>
		/// Enable motion effects. Default for wheels using collider pose.
		/// </summary>
		BothDirections,

		/// <summary>
		/// Enable motion effects, but turn them off while moving backward.
		/// </summary>
		ForwardOnly,

		/// <summary>
		/// Enable motion effects, but turn them off while moving forward.
		/// </summary>
		BackwardOnly,
	}

	internal class VehicleRandomPaintColorConfiguration : IDatParseable
	{
		public float minSaturation;
		public float maxSaturation;
		public float minValue;
		public float maxValue;
		/// <summary>
		/// [0, 1] color will have zero saturation if random value is less than this. For example, 0.2 means 20% of
		/// vehicles will be grayscale.
		/// </summary>
		public float grayscaleChance;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dictionary)
			{
				bool success = dictionary.TryParseFloat("MinSaturation", out minSaturation);
				success &= dictionary.TryParseFloat("MaxSaturation", out maxSaturation);
				success &= dictionary.TryParseFloat("MinValue", out minValue);
				success &= dictionary.TryParseFloat("MaxValue", out maxValue);
				success &= dictionary.TryParseFloat("GrayscaleChance", out grayscaleChance);
				return success;
			}

			return false;
		}
	}

	internal class VehicleWheelConfiguration : IDatParseable
	{
		/// <summary>
		/// If true, this configuration was created by <see cref="InteractableVehicle.BuildAutomaticWheelConfiguration"/>.
		/// Otherwise, this configuration was loaded from the vehicle asset file.
		/// </summary>
		public bool wasAutomaticallyGenerated;

		/// <summary>
		/// Transform path relative to Vehicle prefab with WheelCollider component.
		/// </summary>
		public string wheelColliderPath;

		/// <summary>
		/// If true, WheelCollider's motorTorque is set according to accelerator input.
		/// </summary>
		public bool isColliderPowered;

		/// <summary>
		/// Transform path relative to Vehicle prefab. Animated to match WheelCollider state.
		/// </summary>
		public string modelPath;

		/// <summary>
		/// If true, model is animated according to steering input.
		/// Only kept for backwards compatibility. Prior to wheel configurations, only certain WheelColliders actually
		/// received steering input, while multiple models would appear to steer. For example, the APC's front 4 wheels
		/// appeared to rotate but only the front 2 actually affected physics.
		/// </summary>
		public bool isModelSteered;

		/// <summary>
		/// If true, model ignores isModelSteered and instead uses WheelCollider.GetWorldPose when simulating or the
		/// replicated state from the server when not simulating. Defaults to false.
		/// </summary>
		public bool modelUseColliderPose;

		/// <summary>
		/// If greater than zero, visual-only wheels (without a collider) like the extra wheels of the Snowmobile use
		/// this radius to calculate their rolling speed.
		/// </summary>
		public float modelRadius = -1.0f;

		/// <summary>
		/// If set, visual-only wheels without a collider (like the back wheels of the snowmobile) can copy RPM from
		/// a wheel that does have a collider. Requires modelRadius to also be set.
		/// </summary>
		public int copyColliderRpmIndex = -1;

		/// <summary>
		/// If set, wheel model uses this crawler track's speed (average RPM of wheels). Prevents wheel model from
		/// spinning out of sync with overall track.
		/// </summary>
		public int copyCrawlerTrackSpeedIndex = -1;

		/// <summary>
		/// Target steering angle is multiplied by this value. For example, can be set to a negative number for
		/// rear-wheel steering. Defaults to 1.
		/// </summary>
		public float steeringAngleMultiplier = 1.0f;

		/// <summary>
		/// Vertical offset of model from simulated suspension position.
		/// </summary>
		public float modelSuspensionOffset;

		/// <summary>
		/// How quickly to interpolate model toward suspension position in meters per second.
		/// If negative, position teleports immediately.
		/// </summary>
		public float modelSuspensionSpeed = -1.0f;

		/// <summary>
		/// Nelson 2024-12-06: Initially implemented as a minimum and maximum percentage of normalized forward velocity,
		/// but think this is more practical. I can't think of why we would use values other than -1, 0, +1 for that,
		/// and if we did we'd probably want some tuning for the angle particles are emitted at.
		/// </summary>
		public EWheelMotionEffectsMode motionEffectsMode;

		/// <summary>
		/// If true, wheel should fly off when vehicle explodes. Defaults to true.
		/// Used to simplify destroying vehicles with crawler tracks.
		/// </summary>
		public bool canExplode = true;

		public EWheelSteeringMode steeringMode;
		public ECrawlerTrackForwardMode crawlerTrackForwardMode;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dictionary)
			{
				wheelColliderPath = dictionary.GetString("WheelColliderPath");
				isColliderPowered = dictionary.ParseBool("IsColliderPowered");
				modelPath = dictionary.GetString("ModelPath");
				isModelSteered = dictionary.ParseBool("IsModelSteered");
				modelUseColliderPose = dictionary.ParseBool("ModelUseColliderPose");
				modelRadius = dictionary.ParseFloat("ModelRadius", defaultValue: -1.0f);
				copyColliderRpmIndex = dictionary.ParseInt32("CopyColliderRpmIndex", defaultValue: -1);
				copyCrawlerTrackSpeedIndex = dictionary.ParseInt16("CopyCrawlerTrackSpeedIndex", defaultValue: -1);
				steeringAngleMultiplier = dictionary.ParseFloat("SteeringAngleMultiplier", defaultValue: 1.0f);
				modelSuspensionOffset = dictionary.ParseFloat("ModelSuspensionOffset");
				modelSuspensionSpeed = dictionary.ParseFloat("ModelSuspensionSpeed", defaultValue: -1.0f);

				EWheelMotionEffectsMode defaultMotionEffectsMode = modelUseColliderPose ?
					EWheelMotionEffectsMode.BothDirections : EWheelMotionEffectsMode.None;
				motionEffectsMode = dictionary.ParseEnum("MotionEffects", defaultMotionEffectsMode);

				canExplode = dictionary.ParseBool("CanExplode", true);

				if (dictionary.ParseBool("IsColliderSteered"))
				{
					steeringMode = EWheelSteeringMode.SteeringAngle;
				}
				else
				{
					steeringMode = dictionary.ParseEnum<EWheelSteeringMode>("SteeringMode");
				}

				crawlerTrackForwardMode = dictionary.ParseEnum<ECrawlerTrackForwardMode>("CrawlerTrackForwardMode");
				return true;
			}

			return false;
		}
	}

// 	internal struct VehicleGearConfiguration : IDatParseable
// 	{
// 		public float gearRatio;
// 		public float upThreshold;
// 		public float downThreshold;
//
// 		public bool TryParse(IDatNode node)
// 		{
// 			if (node is DatDictionary dictionary)
// 			{
// 				gearRatio = dictionary.ParseFloat("GearRatio");
// 				upThreshold = dictionary.ParseFloat("UpThreshold", 0.9f);
// 				downThreshold = dictionary.ParseFloat("DownThreshold", 0.1f);
// 				return true;
// 			}
//
// 			return false;
// 		}
// 	}

	internal enum EVehicleEngineSoundType
	{
		/// <summary>
		/// Default.
		/// </summary>
		Legacy,

		/// <summary>
		/// Set pitch and volume of a single clip according to engine RPM.
		/// </summary>
		EngineRPMSimple,
	}

	/// <summary>
	/// Offsets a crawler track's material UV offset in sync with wheels rolling.
	/// </summary>
	internal struct CrawlerTrackTilingMaterial : IDatParseable
	{
		/// <summary>
		/// Scene hierarchy path relative to vehicle root.
		/// </summary>
		public string path;

		/// <summary>
		/// Index in renderer's materials array.
		/// </summary>
		public int materialIndex;

		/// <summary>
		/// Indices of wheels to copy RPM from.
		/// </summary>
		public int[] wheelIndices;

		/// <summary>
		/// How far to travel to offset UV 1x. (1/x)
		///
		/// You can calculate RepeatDistance by selecting an edge parallel to the crawler track and dividing the UV
		/// distance by the physical 3D distance. For example, if the UV length is 2 and the 3D length is 1.5 m then
		/// the texture repeats 1.33 UV/m.
		/// </summary>
		public float repeatDistance;

		/// <summary>
		/// UV mainTextureOffset per distance traveled.
		/// </summary>
		public Vector2 uvDirection;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dictionary)
			{
				repeatDistance = dictionary.ParseFloat("RepeatDistance");

				if (dictionary.TryGetList("WheelIndices", out IDatList wheelIndicesDatList))
				{
					List<int> tempWheelIndices = new List<int>(wheelIndicesDatList.Count);
					foreach (IDatNode indexNode in wheelIndicesDatList)
					{
						if (indexNode is IDatValue indexValue && indexValue.TryParseInt32(out int wheelIndex))
						{
							tempWheelIndices.Add(wheelIndex);
						}
					}
					if (tempWheelIndices.Count > 0)
					{
						wheelIndices = tempWheelIndices.ToArray();
					}
				}

				if (wheelIndices == null || wheelIndices.Length < 1)
				{
					Assets.ReportError(Assets.currentAsset, $"crawler track tiling material\"{path}\" has no WheelIndices");
					return false;
				}

				path = dictionary.GetString("Path");
				materialIndex = dictionary.ParseInt32("MaterialIndex");
				uvDirection = dictionary.ParseVector2("UV_Direction");

				return true;
			}

			return false;
		}
	}

	internal class RpmEngineSoundConfiguration : IDatParseable
	{
		public float idlePitch;
		public float idleVolume;
		public float maxPitch;
		public float maxVolume;

		public bool TryParse(IDatNode node)
		{
			if (node is IDatDictionary dictionary)
			{
				idlePitch = dictionary.ParseFloat("IdlePitch");
				idleVolume = dictionary.ParseFloat("IdleVolume");
				maxPitch = dictionary.ParseFloat("MaxPitch");
				maxVolume = dictionary.ParseFloat("MaxVolume");
				return true;
			}

			return false;
		}
	}

	public class VehicleAsset : Asset, ISkinableAsset, IArmorFalloff
	{
		#region IArmorFalloff
		public float ArmorFalloffMaxRange { get; set; }
		public float ArmorFalloffRange { get; set; }
		public float ArmorFalloffMultiplier { get; set; }
		#endregion IArmorFalloff

		protected bool _shouldVerifyHash;
		public bool shouldVerifyHash => _shouldVerifyHash;

		internal override bool ShouldVerifyHash => _shouldVerifyHash;

		protected string _vehicleName;
		public string vehicleName => _vehicleName;

		public override string FriendlyName => _vehicleName;

		protected float _size2_z;
		public float size2_z => _size2_z;

		protected string _sharedSkinName;
		public string sharedSkinName => _sharedSkinName;

		private System.Guid _sharedSkinLookupGuid;
		/// <summary>
		/// Please refer to: <seealso cref="FindSharedSkinVehicleAsset"/>
		/// </summary>
		public System.Guid SharedSkinLookupGuid => _sharedSkinLookupGuid;

		protected ushort _sharedSkinLookupID;
		/// <summary>
		/// Please refer to: <seealso cref="FindSharedSkinVehicleAsset"/>
		/// </summary>
		[System.Obsolete]
		public ushort sharedSkinLookupID => _sharedSkinLookupID;

		/// <summary>
		/// Supports redirects by VehicleRedirectorAsset.
		///
		/// "Shared Skins" were implemented when there were several asset variants of each vehicle. For example,
		/// Off_Roader_Orange, Off_Roader_Purple, Off_Roader_Green, etc. Each vehicle had their "shared skin" set to
		/// the same ID, and the skin asset had its target ID set to the shared ID. This isn't as necessary after
		/// merging vanilla vehicle variants, but some mods may rely on it, and it needed GUID support now that the
		/// target vehicle might not have a legacy ID.
		/// </summary>
		public VehicleAsset FindSharedSkinVehicleAsset()
		{
			Asset asset = Assets.FindBaseVehicleAssetByGuidOrLegacyId(_sharedSkinLookupGuid, _sharedSkinLookupID);
			if (asset is VehicleRedirectorAsset redirectorAsset)
			{
				asset = redirectorAsset.TargetVehicle.Find();
			}
			return asset as VehicleAsset;
		}

		protected EEngine _engine;
		public EEngine engine => _engine;

		protected EItemRarity _rarity;
		public EItemRarity rarity => _rarity;

		private GameObject loadedModel;
		/// <summary>
		/// Prevents calling getOrLoad redundantly if asset does not exist.
		/// </summary>
		private bool hasLoadedModel;
		private IDeferredAsset<GameObject> clientModel;
		private IDeferredAsset<GameObject> legacyServerModel;

		public GameObject GetOrLoadModel()
		{
			if (!hasLoadedModel)
			{
				hasLoadedModel = true;

				if (legacyServerModel != null)
				{
					loadedModel = legacyServerModel.getOrLoad();
					if (loadedModel == null)
					{
						loadedModel = clientModel.getOrLoad();
					}
				}
				else
				{
					loadedModel = clientModel.getOrLoad();
				}
			}

			return loadedModel;
		}

		protected void onModelLoaded(GameObject asset)
		{
			Transform seatsTransform = asset.transform.Find("Seats");
			if (seatsTransform == null)
			{
				Assets.ReportError(this, "missing 'Seats' Transform");
			}
			else if (seatsTransform.childCount < 1)
			{
				Assets.ReportError(this, "empty 'Seats' Transform has zero children");
			}

			// Adds up the number of "Seat_#" GameObjects that are children of "Seats".
			// This value is only used when generating Cargo data.
			else
			{
				for (int index = 0; index <= seatsTransform.childCount; index++)
				{
					if (seatsTransform.Find($"Seat_{index}"))
					{
						_numSeats++;
					}
				}
			}

			Rigidbody rootRigidbody = asset.GetComponent<Rigidbody>();
			if (rootRigidbody == null)
			{
				Assets.ReportError(this, "missing root Rigidbody");
			}
			else if (physicsProfileRef.isNull)
			{
				if (MathfEx.IsNearlyEqual(rootRigidbody.mass, 1.0f))
				{
					bool useDefaultPhysicsProfile = true;

					Transform tires = asset.transform.Find("Tires");
					if (tires != null)
					{
						for (int index = 0; index < tires.childCount; ++index)
						{
							Transform child = tires.GetChild(index);
							if (child == null)
								continue;

							WheelCollider wheel = child.GetComponent<WheelCollider>();
							if (wheel == null)
								continue;

							if (MathfEx.IsNearlyEqual(wheel.mass, 1.0f) == false)
							{
								useDefaultPhysicsProfile = false;
								break;
							}
						}
					}

					if (useDefaultPhysicsProfile)
					{
						// Blimp and train do not have default physics profiles.
						// Mostly because blimp is fine, and train does not use physics for the most part.
						switch (engine)
						{
							case EEngine.BOAT:
								physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Boat;
								break;

							case EEngine.CAR:
								physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Car;
								break;

							case EEngine.HELICOPTER:
								physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Helicopter;
								break;

							case EEngine.PLANE:
								physicsProfileRef = VehiclePhysicsProfileAsset.defaultProfile_Plane;
								break;
						}
					}
				}
			}

			// Required for hit detection which uses CompareTag.
			asset.SetTagIfUntaggedRecursively("Vehicle");

			if (wheelConfiguration == null)
			{
				BuildAutomaticWheelConfiguration(asset);
			}
		}

		/// <summary>
		/// Clip.prefab
		/// </summary>
		protected void OnServerModelLoaded(GameObject asset)
		{
			if (asset == null)
			{
				Assets.ReportError(this, "missing \"Clip\" GameObject, loading \"Vehicle\" GameObject instead");
				return;
			}

			// Server no longer loads the client vehicle, so we assume we have these.
			_hasHeadlights = true;
			_hasSirens = true;
			_hasHook = true;

			onModelLoaded(asset);
		}

		/// <summary>
		/// Vehicle.prefab
		/// </summary>
		protected void OnClientModelLoaded(GameObject asset)
		{
			if (asset == null)
			{
				Assets.ReportError(this, "missing \"Vehicle\" GameObject");
				return;
			}

			AssetValidation.searchGameObjectForErrors(this, asset);

			_hasHeadlights = asset.transform.Find("Headlights") != null;
			_hasSirens = asset.transform.Find("Sirens") != null;
			_hasHook = asset.transform.Find("Hook") != null;

			if (_pitchIdle < 0)
			{
				_pitchIdle = 0.5f;

				AudioSource source = asset.GetComponent<AudioSource>();

				if (source != null)
				{
					AudioClip track = source.clip;

					if (track != null)
					{
						if (track.name == "Engine_Large")
						{
							_pitchIdle = 0.625f;
						}
						else if (track.name == "Engine_Small")
						{
							_pitchIdle = 0.75f;
						}
					}
				}
			}

			if (_pitchDrive < 0)
			{
				if (engine == EEngine.HELICOPTER)
				{
					// Prior to 2021-02-22 this was hardcoded at 0.03.
					_pitchDrive = 0.03f;
				}
				else if (engine == EEngine.BLIMP)
				{
					// Prior to 2021-02-22 this was hardcoded at 0.1.
					_pitchDrive = 0.1f;
				}
				else
				{
					_pitchDrive = 0.05f;

					AudioSource source = asset.GetComponent<AudioSource>();

					if (source != null)
					{
						AudioClip track = source.clip;

						if (track != null)
						{
							if (track.name == "Engine_Large")
							{
								_pitchDrive = 0.025f;
							}
							else if (track.name == "Engine_Small")
							{
								_pitchDrive = 0.075f;
							}
						}
					}
				}
			}

			onModelLoaded(asset);

			if (Dedicator.IsDedicatedServer)
			{
				// Optimize client prefab for server usage.
				ServerPrefabUtil.RemoveClientComponents(asset, this);
			}
		}

		public void DebugDumpWheelConfigurationToStringBuilder(System.Text.StringBuilder output)
		{
			output.Append(vehicleName);
			if (wheelConfiguration == null || wheelConfiguration.Length < 1)
			{
				output.AppendLine(" wheel configuration(s): N/A");
				return;
			}

			output.AppendLine(" wheel configuration(s):");
			for (int wheelIndex = 0; wheelIndex < wheelConfiguration.Length; ++wheelIndex)
			{
				output.Append(wheelIndex);
				output.AppendLine(":");

				VehicleWheelConfiguration configuration = wheelConfiguration[wheelIndex];

				output.Append("Wheel collider path: \"");
				output.Append(configuration.wheelColliderPath);
				output.AppendLine("\"");

				output.Append("Is collider steered: ");
				output.Append(configuration.steeringMode == EWheelSteeringMode.SteeringAngle);
				output.AppendLine();

				output.Append("Is collider powered: ");
				output.Append(configuration.isColliderPowered);
				output.AppendLine();

				output.Append("Model path: \"");
				output.Append(configuration.modelPath);
				output.AppendLine("\"");

				output.Append("Is model steered: ");
				output.Append(configuration.isModelSteered);
				output.AppendLine();
			}
		}

		public string DebugDumpWheelConfigurationToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			DebugDumpWheelConfigurationToStringBuilder(sb);
			return sb.ToString();
		}

		private void LogWheelConfigurationDatConversion()
		{
			string result;

			using (System.IO.StringWriter stringWriter = new System.IO.StringWriter())
			using (DatWriter datWriter = new DatWriter(stringWriter))
			{
				datWriter.WriteListStart("WheelConfigurations");

				foreach (VehicleWheelConfiguration configuration in wheelConfiguration)
				{
					datWriter.WriteDictionaryStart();
					datWriter.WriteKeyValue("WheelColliderPath", configuration.wheelColliderPath);
					datWriter.WriteKeyValue("IsColliderPowered", configuration.isColliderPowered);
					datWriter.WriteKeyValue("ModelPath", configuration.modelPath);
					datWriter.WriteKeyValue("IsModelSteered", configuration.isModelSteered);
					datWriter.WriteKeyValue("SteeringMode", configuration.steeringMode.ToString());
					datWriter.WriteDictionaryEnd();
				}

				datWriter.WriteListEnd();
				result = stringWriter.ToString();
			}

			UnturnedLog.info($"Converted \"{FriendlyName}\" wheel configuration:");
			UnturnedLog.info(result);
		}

		/// <summary>
		/// Nelson 2024-02-28: Prior to the VehicleWheelConfiguration class, most of the wheel configuration was
		/// inferred during InteractableVehicle initialization from the children of the "Tires" and "Wheels" transforms.
		/// Confusingly, "Tires" only contains WheelColliders and "Wheels" only contains the visual models. Rather than
		/// keeping the old behavior in InteractableVehicle alongside the newer more configurable one, we match the old
		/// behavior here to generate an equivalent configuration.
		///
		/// Note that <see cref="steeringTireIndices"/> must be initialized before this is called (by loading model).
		/// </summary>
		private void BuildAutomaticWheelConfiguration(GameObject vehicleGameObject)
		{
			Transform vehicleTransform = vehicleGameObject.transform;

			List<VehicleWheelConfiguration> pendingWheelConfigurations = new List<VehicleWheelConfiguration>();

			Transform wheelCollidersParentTransform = vehicleTransform.Find("Tires");
			if (wheelCollidersParentTransform != null)
			{
				for (int index = 0; index < wheelCollidersParentTransform.childCount; index++)
				{
					string wheelColliderName = "Tire_" + index;
					Transform wheelColliderTransform = wheelCollidersParentTransform.Find(wheelColliderName);
					if (wheelColliderTransform == null)
					{
						Assets.ReportError(this, "missing \"{0}\" Transform", wheelColliderName);
						continue;
					}

					WheelCollider wheelCollider = wheelColliderTransform.GetComponent<WheelCollider>();
					if (wheelCollider == null)
					{
						Assets.ReportError(this, "missing \"{0}\" WheelCollider", wheelColliderName);
						continue;
					}

					VehicleWheelConfiguration configuration = new VehicleWheelConfiguration();
					configuration.wasAutomaticallyGenerated = true;
					configuration.wheelColliderPath = "Tires/" + wheelColliderName;
					configuration.steeringMode = index < 2 ? EWheelSteeringMode.SteeringAngle : EWheelSteeringMode.None;
					configuration.isColliderPowered = index >= wheelCollidersParentTransform.childCount - 2;
					pendingWheelConfigurations.Add(configuration);
				}
			}

			Transform wheelModelsParentTransform = vehicleTransform.Find("Wheels");
			if (wheelModelsParentTransform != null)
			{
				// Try to find wheel models based on distance from wheel collider first.
				foreach (VehicleWheelConfiguration configuration in pendingWheelConfigurations)
				{
					Transform wheelColliderTransform = vehicleTransform.Find(configuration.wheelColliderPath);
					if (wheelColliderTransform == null)
					{
						// We just assigned this a moment ago! :(
						Debug.Assert(false);
						continue;
					}

					int bestMatchChildIndex = -1;
					float bestMatchSqrDistance = 16.0f;
					for (int searchIndex = 0; searchIndex < wheelModelsParentTransform.childCount; ++searchIndex)
					{
						Transform modelTransform = wheelModelsParentTransform.GetChild(searchIndex);
						float sqrDistance = (wheelColliderTransform.position - modelTransform.position).sqrMagnitude;
						if (sqrDistance < bestMatchSqrDistance)
						{
							bestMatchChildIndex = searchIndex;
							bestMatchSqrDistance = sqrDistance;
						}
					}

					if (bestMatchChildIndex != -1)
					{
						Transform modelTransform = wheelModelsParentTransform.GetChild(bestMatchChildIndex);
						if (modelTransform.childCount < 1)
						{
							// Actual model exists elsewhere in the hierarchy.
							// (just maintaining backwards compatibility)
							Transform modelTransformSomewhereElse = vehicleTransform.FindChildRecursive("Wheel_" + bestMatchChildIndex);
							if (modelTransformSomewhereElse != null)
							{
								modelTransform = modelTransformSomewhereElse;
							}
						}

						string modelPath = modelTransform.name;

						Transform parent = modelTransform.parent;
						while (parent != vehicleTransform)
						{
							modelPath = parent.name + "/" + modelPath;
							parent = parent.parent;
						}

						configuration.modelPath = modelPath;
					}
				}

				// Add visual-only configurations for any models without a wheel collider.
				foreach (Transform modelTransform in wheelModelsParentTransform)
				{
					if (modelTransform.childCount < 1)
					{
						continue;
					}

					bool isUsedByAnyWheelConfiguration = false;
					foreach (VehicleWheelConfiguration configuration in pendingWheelConfigurations)
					{
						if (string.IsNullOrEmpty(configuration.modelPath))
						{
							continue;
						}

						Transform otherModelTransform = vehicleTransform.Find(configuration.modelPath);
						if (otherModelTransform == null)
						{
							// We just assigned this a moment ago! :(
							Debug.Assert(false);
							continue;
						}

						if (otherModelTransform == modelTransform)
						{
							isUsedByAnyWheelConfiguration = true;
							break;
						}
					}

					if (isUsedByAnyWheelConfiguration)
					{
						continue;
					}

					VehicleWheelConfiguration newConfiguration = new VehicleWheelConfiguration();
					newConfiguration.wasAutomaticallyGenerated = true;
					newConfiguration.modelPath = "Wheels/" + modelTransform.name;
					pendingWheelConfigurations.Add(newConfiguration);
				}

#pragma warning disable
				if (steeringTireIndices != null)
				{
					foreach (int animatedTireIndex in steeringTireIndices)
#pragma warning restore
					{
						string modelName = "Wheel_" + animatedTireIndex;
						Transform modelTransform = wheelModelsParentTransform.Find(modelName);
						if (modelTransform == null)
						{
							modelTransform = vehicleTransform.FindChildRecursive(modelName);
							if (modelTransform == null && animatedTireIndex < wheelModelsParentTransform.childCount)
							{
								modelTransform = wheelModelsParentTransform.GetChild(animatedTireIndex);
							}
						}

						if (modelTransform == null)
						{
							// Not reported because vehicles like helicopters and boats default to 1 steering tire.
							continue;
						}

						VehicleWheelConfiguration foundConfiguration = null;
						foreach (VehicleWheelConfiguration configuration in pendingWheelConfigurations)
						{
							if (string.IsNullOrEmpty(configuration.modelPath))
							{
								continue;
							}

							Transform otherModelTransform = vehicleTransform.Find(configuration.modelPath);
							if (otherModelTransform == null)
							{
								// We just assigned this a moment ago! :(
								Debug.Assert(false);
								continue;
							}

							if (otherModelTransform == modelTransform)
							{
								foundConfiguration = configuration;
								break;
							}
						}

						if (foundConfiguration != null)
						{
							foundConfiguration.isModelSteered = true;
						}
						else
						{
							Assets.ReportError(this, "unable to match physical tire with steering tire model {0}", animatedTireIndex);
						}
					}
				}
			}

			wheelConfiguration = pendingWheelConfigurations.ToArray();

			if (clLogWheelConfiguration)
			{
				LogWheelConfigurationDatConversion();
			}
		}

		protected AudioClip _ignition;
		public AudioClip ignition => _ignition;

		protected AudioClip _horn;
		public AudioClip horn => _horn;

		public bool hasHorn
		{
			get;
			protected set;
		}

		protected float _pitchIdle;
		public float pitchIdle => _pitchIdle;

		protected float _pitchDrive;
		public float pitchDrive => _pitchDrive;

		internal EVehicleEngineSoundType engineSoundType;
		internal RpmEngineSoundConfiguration engineSoundConfiguration;

		/// <summary>
		/// Maximum (negative) velocity to aim for while accelerating backward.
		/// </summary>
		public float TargetReverseVelocity
		{
			get;
			private set;
		}

		/// <summary>
		/// Maximum speed to aim for while accelerating backward.
		/// </summary>
		public float TargetReverseSpeed => Mathf.Abs(TargetReverseVelocity);

		/// <summary>
		/// Maximum velocity to aim for while accelerating forward.
		/// </summary>
		public float TargetForwardVelocity
		{
			get;
			private set;
		}

		/// <summary>
		/// Maximum speed to aim for while accelerating forward.
		/// </summary>
		public float TargetForwardSpeed => Mathf.Abs(TargetForwardVelocity);

		protected float _steerMin;
		/// <summary>
		/// Steering angle range at target maximum speed (for the current forward/backward direction).
		/// Reducing steering range at higher speeds keeps the vehicle controlable with digital (non-analog) input.
		/// </summary>
		public float MaxSteeringAngleAtFullSpeed
		{
			get => _steerMin;
			set => _steerMin = value;
		}

		protected float _steerMax;
		/// <summary>
		/// Steering angle range at zero speed (idle/parked).
		/// For example, 45 means the wheels connected to steering can rotate ±45 degrees.
		/// </summary>
		public float MaxSteeringAngle
		{
			get => _steerMax;
			set => _steerMax = value;
		}

		/// <summary>
		/// Steering angle rotation change in degrees per second.
		/// </summary>
		public float SteeringAngleTurnSpeed
		{
			get;
			private set;
		}

		/// <summary>
		/// Added or subtracted from wheel motor torque in <see cref="EWheelSteeringMode.CrawlerTrack"/> mode.
		/// </summary>
		public float CrawlerTrackSteeringTorque
		{
			get;
			private set;
		}

		/// <summary>
		/// When a wheel is in <see cref="EWheelSteeringMode.CrawlerTrack"/> mode and a steering input is applied the
		/// <see cref="WheelCollider.sidewaysFriction"/> stiffness is multiplied by this factor. This allows the vehicle
		/// to rotate in-place with a lower steering torque, which helps prevent the vehicle from going out of control
		/// while turning and accelerating.
		/// </summary>
		public float CrawlerTrackSteeringSidewaysFrictionMultiplier
		{
			get;
			private set;
		}

		/// <summary>
		/// Multiplier for <see cref="CrawlerTrackSteeringTorque"/> and <see cref="CrawlerTrackSteeringSidewaysFrictionMultiplier"/>
		/// while at target maximum speed (for the current forward/backward direction).
		/// </summary>
		public float CrawlerTrackSteeringMaxSpeedScale
		{
			get;
			private set;
		}

		/// <summary>
		/// Torque on Z axis applied according to steering input for bikes and motorcycles.
		/// </summary>
		internal float steeringLeaningForceMultiplier;

		/// <summary>
		/// If true, leaning force is multiplied by normalized speed to the power of steeringLeaningForceSpeedExponent.
		/// Defaults to false.
		/// </summary>
		internal bool steeringLeaningForceShouldScaleWithSpeed;
		/// <summary>
		/// Refer to steeringLeaningForceShouldScaleWithSpeed.
		/// </summary>
		internal float steeringLeaningForceSpeedExponent;

		protected float _brake;
		public float brake => _brake;

		protected float _lift;
		public float lift => _lift;

		protected ushort _fuelMin;
		public ushort fuelMin => _fuelMin;

		protected ushort _fuelMax;
		public ushort fuelMax => _fuelMax;

		protected ushort _fuel;
		public ushort fuel => _fuel;

		protected ushort _healthMin;
		public ushort healthMin => _healthMin;

		protected ushort _healthMax;
		public ushort healthMax => _healthMax;

		protected ushort _health;
		public ushort health => _health;

		private System.Guid _explosionEffectGuid;
		public System.Guid ExplosionEffectGuid => _explosionEffectGuid;

		protected ushort _explosion;
		public ushort explosion
		{
			[System.Obsolete]
			get => _explosion;
		}

		public bool IsExplosionEffectRefNull()
		{
#pragma warning disable
			return _explosion == 0 && _explosionEffectGuid.IsEmpty();
#pragma warning restore
		}

		public EffectAsset FindExplosionEffectAsset()
		{
#pragma warning disable
			return Assets.FindEffectAssetByGuidOrLegacyId(_explosionEffectGuid, _explosion);
#pragma warning restore
		}

		public Vector3 minExplosionForce
		{
			get;
			set;
		}

		public Vector3 maxExplosionForce
		{
			get;
			set;
		}

		[System.Obsolete("Separated into ShouldExplosionCauseDamage and ShouldExplosionBurnMaterials.")]
		public bool isExplosive => !IsExplosionEffectRefNull();

		/// <summary>
		/// If true, explosion will damage nearby entities and kill passengers.
		/// </summary>
		public bool ShouldExplosionCauseDamage
		{
			get;
			protected set;
		}

		public bool ShouldExplosionBurnMaterials
		{
			get;
			protected set;
		}

		/// <summary>
		/// Only used if ShouldExplosionBurnMaterials. Optional. Allows specifying which renderers to burn.
		/// </summary>
		internal PaintableVehicleSection[] explosionBurnMaterialSections;

		protected bool _hasHeadlights;
		public bool hasHeadlights => _hasHeadlights;

		protected bool _hasSirens;
		public bool hasSirens => _hasSirens;

		protected bool _hasHook;
		public bool hasHook => _hasHook;

		protected int _numSeats;
		public int numSeats => _numSeats;

		protected bool _hasZip;
		public bool hasZip => _hasZip;

		protected bool _hasBicycle;
		/// <summary>
		/// When true the bicycle animation is used and extra speed is stamina powered.
		/// Bad way to implement it.
		/// </summary>
		public bool hasBicycle => _hasBicycle;

		/// <summary>
		/// Can this vehicle ever spawn with a charged battery?
		/// Uses game mode battery stats when true, or overrides by preventing battery spawn when false.
		/// </summary>
		public bool canSpawnWithBattery
		{
			get;
			protected set;
		}

		/// <summary>
		/// Battery charge when first spawning in is multiplied by this [0, 1] number.
		/// </summary>
		public float batterySpawnChargeMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Battery decrease per second.
		/// </summary>
		public float batteryBurnRate
		{
			get;
			protected set;
		}

		/// <summary>
		/// Battery increase per second.
		/// </summary>
		public float batteryChargeRate
		{
			get;
			protected set;
		}

		public EBatteryMode batteryDriving
		{
			get;
			protected set;
		}

		public EBatteryMode batteryEmpty
		{
			get;
			protected set;
		}

		public EBatteryMode batteryHeadlights
		{
			get;
			protected set;
		}

		public EBatteryMode batterySirens
		{
			get;
			protected set;
		}

		/// <summary>
		/// Battery item given to the player when a specific battery hasn't been manually
		/// installed yet. Defaults to the vanilla car battery (098b13be34a7411db7736b7f866ada69).
		/// </summary>
		public System.Guid defaultBatteryGuid
		{
			get;
			protected set;
		}

		/// <summary>
		/// Fuel decrease per second.
		/// </summary>
		public float fuelBurnRate
		{
			get;
			protected set;
		}

		public bool isReclined
		{
			get;
			protected set;
		}

		protected bool _hasLockMouse;
		public bool hasLockMouse => _hasLockMouse;

		protected bool _hasTraction;
		public bool hasTraction => _hasTraction;

		protected bool _hasSleds;
		public bool hasSleds => _hasSleds;

		protected float _exit;
		public float exit => _exit;

		protected float _sqrDelta;
		public float sqrDelta => _sqrDelta;

		/// <summary>
		/// Client sends physics simulation results to server. If upward (+Y) speed exceeds this, mark the move invalid.
		/// </summary>
		public float validSpeedUp
		{
			get;
			protected set;
		}

		/// <summary>
		/// Client sends physics simulation results to server. If downward (-Y) speed exceeds this, mark the move invalid.
		/// </summary>
		public float validSpeedDown
		{
			get;
			protected set;
		}

		/// <summary>
		/// If distance between client-submitted hit position and vehicle pivot point is too high the hit will be
		/// marked invalid. This multiplies the distance threshold, useful for very fast vehicles.
		/// </summary>
		public float ValidHitDistanceMultiplier
		{
			get;
			protected set;
		}

		protected float _camFollowDistance;
		public float camFollowDistance => _camFollowDistance;

		/// <summary>
		/// Vertical first-person view translation.
		/// </summary>
		public float camDriverOffset
		{
			get;
			protected set;
		}

		/// <summary>
		/// Vertical first-person view translation.
		/// </summary>
		public float camPassengerOffset
		{
			get;
			protected set;
		}

		protected float _bumperMultiplier;
		public float bumperMultiplier => _bumperMultiplier;

		/// <summary>
		/// Base damage to players when traveling at 1 m/s. Defaults to 10.
		/// </summary>
		public float BumperPlayerDamage
		{
			get;
			set;
		}

		/// <summary>
		/// Base damage to zombies when traveling at 1 m/s. Defaults to 15.
		/// </summary>
		public float BumperZombieDamage
		{
			get;
			set;
		}

		/// <summary>
		/// Base damage to animals when traveling at 1 m/s. Defaults to 15.
		/// </summary>
		public float BumperAnimalDamage
		{
			get;
			set;
		}

		/// <summary>
		/// Base damage to objects when traveling at 1 m/s. Defaults to 30.
		/// </summary>
		public float BumperObjectDamage
		{
			get;
			set;
		}

		/// <summary>
		/// Base damage to trees when traveling at 1 m/s. Defaults to 85.
		/// </summary>
		public float BumperResourceDamage
		{
			get;
			set;
		}

		/// <summary>
		/// If speed multiplied by <see cref="bumperMultiplier"/> is less than this, no damage is applied.
		/// Defaults to 3.
		/// </summary>
		public float BumperSpeedDamageThreshold
		{
			get;
			set;
		}

		/// <summary>
		/// Multiplier for damage from crashing into things.
		/// Not applicable if <see cref="isVulnerableToBumper"/> is false.
		/// Defaults to 1.
		/// </summary>
		public float BumperSelfDamageMultiplier
		{
			get;
			set;
		}

		protected float _passengerExplosionArmor;
		public float passengerExplosionArmor => _passengerExplosionArmor;

		protected TurretInfo[] _turrets;
		public TurretInfo[] turrets => _turrets;

		protected Texture2D _albedoBase;
		public Texture albedoBase => _albedoBase;

		protected Texture2D _metallicBase;
		public Texture metallicBase => _metallicBase;

		protected Texture2D _emissionBase;
		public Texture emissionBase => _emissionBase;

		/// <summary>
		/// To non-explosions.
		/// </summary>
		public bool isVulnerable;
		public bool isVulnerableToExplosions;
		/// <summary>
		/// Mega zombie rocks, zombies, animals.
		/// </summary>
		public bool isVulnerableToEnvironment;
		/// <summary>
		/// Crashing into stuff.
		/// </summary>
		public bool isVulnerableToBumper;
		public bool canTiresBeDamaged;

		public bool CanDecay
		{
			get;
			private set;
		}

		/// <summary>
		/// Can this vehicle be repaired by a seated player?
		/// </summary>
		public bool canRepairWhileSeated
		{
			get;
			protected set;
		}

		public float childExplosionArmorMultiplier
		{
			get;
			protected set;
		}

		public float airTurnResponsiveness
		{
			get;
			protected set;
		}

		public float airSteerMin
		{
			get;
			protected set;
		}

		public float airSteerMax
		{
			get;
			protected set;
		}

		public float bicycleAnimSpeed
		{
			get;
			protected set;
		}

		public float trainTrackOffset
		{
			get;
			protected set;
		}

		public float trainWheelOffset
		{
			get;
			protected set;
		}

		public float trainCarLength
		{
			get;
			protected set;
		}

		public float staminaBoost
		{
			get;
			protected set;
		}

		public bool useStaminaBoost
		{
			get;
			protected set;
		}

		public bool isStaminaPowered
		{
			get;
			protected set;
		}

		public bool isBatteryPowered
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can mobile barricades e.g. bed or sentry guns be placed on this vehicle?
		/// </summary>
		[System.Obsolete("Replaced by BuildablePlacementRule")]
		public bool supportsMobileBuildables
		{
			get => BuildablePlacementRule == EVehicleBuildablePlacementRule.AlwaysAllow;
		}

		public EVehicleBuildablePlacementRule BuildablePlacementRule
		{
			get;
			protected set;
		}

		/// <summary>
		/// Should capsule colliders be added to seat transforms?
		/// Useful to prevent bikes from leaning into walls.
		/// </summary>
		public bool shouldSpawnSeatCapsules
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can players lock the vehicle to their clan/group?
		/// True by default, but mods want to be able to disable.
		/// </summary>
		public bool canBeLocked
		{
			get;
			protected set;
		}

		/// <summary>
		/// Can players steal the battery?
		/// </summary>
		public bool canStealBattery
		{
			get;
			protected set;
		}

		public byte trunkStorage_X
		{
			get;
			set;
		}

		public byte trunkStorage_Y
		{
			get;
			set;
		}

		/// <summary>
		/// Spawn table to drop items from on death.
		/// </summary>
		public ushort dropsTableId
		{
			get;
			protected set;
		}

		/// <summary>
		/// Minimum number of items to drop on death.
		/// </summary>
		public byte dropsMin
		{
			get;
			protected set;
		}

		/// <summary>
		/// Maximum number of items to drop on death.
		/// </summary>
		public byte dropsMax
		{
			get;
			protected set;
		}

		/// <summary>
		/// Item ID of compatible tire.
		/// </summary>
		public ushort tireID
		{
			get;
			protected set;
		}

		/// <summary>
		/// If greater than zero, torque is applied on the local Z axis multiplied by this factor.
		/// Note that <see cref="rollAngularVelocityDamping"/> is critical for damping this force.
		/// </summary>
		internal float wheelBalancingForceMultiplier = -1.0f;

		/// <summary>
		/// Exponent on the [0, 1] factor representing how aligned the vehicle is with the ground up vector.
		/// </summary>
		internal float wheelBalancingUprightExponent;

		/// <summary>
		/// If greater than zero, an acceleration is applied to angular velocity on Z axis toward zero.
		/// </summary>
		internal float rollAngularVelocityDamping = -1.0f;

		internal VehicleWheelConfiguration[] wheelConfiguration;
		/// <summary>
		/// Indices of wheels using replicated collider pose (if any).
		/// Null if not configured or no wheels using this feature.
		/// Allows client and server to replicate only the suspension value without other context.
		/// </summary>
		internal int[] replicatedWheelIndices;
		/// <summary>
		/// Indices of wheels with motor torque applied (if any).
		/// Used for engine RPM calculation.
		/// </summary>
		internal int[] poweredWheelIndices;

		internal float reverseGearRatio;
		internal float[] forwardGearRatios;
		internal bool UsesEngineRpmAndGears => forwardGearRatios != null && forwardGearRatios.Length > 0;

		/// <summary>
		/// If false, only grounded wheels are included when calculating wheel RPM.
		/// </summary>
		public bool ShouldIncludeAirbornWheelsInAverageRpm
		{
			get;
			set;
		} = true;

		/// <summary>
		/// If this and UsesEngineRpmAndGears are true, HUD will show RPM and gear number.
		/// </summary>
		public bool AllowsEngineRpmAndGearsInHud
		{
			get;
			protected set;
		}

		/// <summary>
		/// When engine RPM dips below this value shift to the next lower gear if available.
		/// </summary>
		public float GearShiftDownThresholdRpm
		{
			get;
			private set;
		}
		/// <summary>
		/// When engine RPM exceeds this value shift to the next higher gear if available.
		/// </summary>
		public float GearShiftUpThresholdRpm
		{
			get;
			private set;
		}
		/// <summary>
		/// How long after changing gears before throttle is engaged again.
		/// </summary>
		public float GearShiftDuration
		{
			get;
			private set;
		}
		/// <summary>
		/// How long between changing gears to allow another automatic gear change.
		/// </summary>
		public float GearShiftInterval
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, engine can skip from (for example) 1st to 3rd gear if it keeps RPM within
		/// the acceptable range.
		/// </summary>
		public bool GearShiftAllowSkippingGears
		{
			get;
			set;
		} = true;

		/// <summary>
		/// Minimum engine RPM.
		/// </summary>
		public float EngineIdleRpm
		{
			get;
			private set;
		}
		/// <summary>
		/// Maximum engine RPM.
		/// </summary>
		public float EngineMaxRpm
		{
			get;
			private set;
		}
		/// <summary>
		/// How quickly RPM can increase in RPM/s.
		/// e.g., 1000 will take 2 seconds to go from 2000 to 4000 RPM.
		/// Defaults to -1 which instantly changes RPM.
		/// </summary>
		public float EngineRpmIncreaseRate
		{
			get;
			private set;
		}
		/// <summary>
		/// How quickly RPM can decrease in RPM/s.
		/// e.g., 1000 will take 2 seconds to go from 4000 to 2000 RPM.
		/// Defaults to -1 which instantly changes RPM.
		/// </summary>
		public float EngineRpmDecreaseRate
		{
			get;
			private set;
		}
		/// <summary>
		/// Maximum torque (multiplied by output of torque curve).
		/// </summary>
		public float EngineMaxTorque
		{
			get;
			private set;
		}

		/// <summary>
		/// If true, wheel RPM is reduced according to the difference between expected and actual
		/// wheel RPM divided by torque reduction threshold.
		/// </summary>
		public bool EngineRpmMismatchTorqueReductionEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// If torque reduction is enabled, torque is reduced to zero when difference between
		/// expected and actual RPM is greater than this threshold.
		/// </summary>
		public float EngineRpmMismatchTorqueReductionThreshold
		{
			get;
			set;
		}

		/// <summary>
		/// If true, prevent changing gears when the difference between expected and actual
		/// wheel RPM exceeds threshold.
		/// </summary>
		public bool EngineRpmMismatchGearShiftPreventShifting
		{
			get;
			set;
		}

		/// <summary>
		/// If prevent shifting is enabled, prevent changing gears up when the difference between
		/// expected and actual wheel RPM is less than this threshold.
		/// I.e., if (expected - actual < min) it cannot shift up.
		/// </summary>
		public float EngineRpmMismatchGearShiftUpMinThreshold
		{
			get;
			set;
		}

		/// <summary>
		/// If prevent shifting is enabled, prevent changing gears up when the difference between
		/// expected and actual wheel RPM is greater than this threshold.
		/// I.e., if (expected - actual > max) it cannot shift up.
		/// </summary>
		public float EngineRpmMismatchGearShiftUpMaxThreshold
		{
			get;
			set;
		}

		/// <summary>
		/// If prevent shifting is enabled, prevent changing gears down when the difference between
		/// expected and actual wheel RPM is less than this threshold.
		/// I.e., if (expected - actual < min) it cannot shift down.
		/// </summary>
		public float EngineRpmMismatchGearShiftDownMinThreshold
		{
			get;
			set;
		}

		/// <summary>
		/// If prevent shifting is enabled, prevent changing gears down when the difference between
		/// expected and actual wheel RPM is greater than this threshold.
		/// I.e., if (expected - actual > max) it cannot shift down.
		/// </summary>
		public float EngineRpmMismatchGearShiftDownMaxThreshold
		{
			get;
			set;
		}

		/// <summary>
		/// Was a center of mass specified in the .dat?
		/// </summary>
		public bool hasCenterOfMassOverride
		{
			get;
			protected set;
		}

		/// <summary>
		/// If hasCenterOfMassOverride, use this value.
		/// </summary>
		public Vector3 centerOfMass
		{
			get;
			protected set;
		}

		public float carjackForceMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// Multiplier for otherwise not-yet-configurable plane/heli/boat forces.
		/// Nelson 2024-03-06: Required for increasing mass of vehicles without significantly messing with behavior.
		/// </summary>
		public float engineForceMultiplier
		{
			get;
			protected set;
		}

		/// <summary>
		/// If set, override the wheel collider mass with this value.
		/// </summary>
		public float? wheelColliderMassOverride
		{
			get;
			protected set;
		}

		public AssetReference<VehiclePhysicsProfileAsset> physicsProfileRef
		{
			get;
			protected set;
		}

		/// <summary>
		/// Null if vehicle doesn't support paint color.
		/// </summary>
		public PaintableVehicleSection[] PaintableVehicleSections
		{
			get;
			protected set;
		}

		/// <summary>
		/// List of transforms to register with DynamicWaterTransparentSort.
		/// </summary>
		internal PaintableVehicleSection[] extraTransparentSections;

		internal CrawlerTrackTilingMaterial[] crawlerTrackTilingMaterials;

		internal EVehicleDefaultPaintColorMode defaultPaintColorMode;

		/// <summary>
		/// Null if vehicle doesn't support paint color.
		/// </summary>
		public List<Color32> DefaultPaintColors
		{
			get;
			protected set;
		}

		/// <summary>
		/// Null if <see cref="defaultPaintColorMode"/> isn't <see cref="EVehicleDefaultPaintColorMode.RandomHueOrGrayscale"/>.
		/// </summary>
		internal VehicleRandomPaintColorConfiguration randomPaintColorConfiguration;

		/// <summary>
		/// Pick a random paint color according to <see cref="defaultPaintColorMode"/>. Null if unsupported or not configured.
		/// </summary>
		public Color32? GetRandomDefaultPaintColor()
		{
			if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.List)
			{
				if (DefaultPaintColors != null && DefaultPaintColors.Count > 0)
				{
					return DefaultPaintColors.RandomOrDefault();
				}
			}
			else if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.RandomHueOrGrayscale)
			{
				if (randomPaintColorConfiguration != null)
				{
					if (Random.value < randomPaintColorConfiguration.grayscaleChance)
					{
						float value = Random.Range(randomPaintColorConfiguration.minValue, randomPaintColorConfiguration.maxValue);
						return new Color(value, value, value, 1.0f);
					}
					else
					{
						float hue = Random.value;
						float saturation = Random.Range(randomPaintColorConfiguration.minSaturation, randomPaintColorConfiguration.maxSaturation);
						float value = Random.Range(randomPaintColorConfiguration.minValue, randomPaintColorConfiguration.maxValue);
						return Color.HSVToRGB(hue, saturation, value);
					}
				}
			}

			return null;
		}

		public bool SupportsPaintColor
		{
			get
			{
				return PaintableVehicleSections != null && PaintableVehicleSections.Length > 0;
			}
		}

		/// <summary>
		/// If true, Vehicle Paint items can be used on this vehicle.
		/// Always false if <see cref="SupportsPaintColor"/> is false.
		///
		/// Certain vehicles may support paint colors without also being paintable by players. For example, the creator
		/// of a vehicle may want to use color variants without also allowing players to make it bright pink.
		/// </summary>
		public bool IsPaintable
		{
			get;
			protected set;
		}

		/// <summary>
		/// Returns reverseGearRatio for negative gears, actual value for valid gear number, otherwise zero.
		/// Exposed for plugin use.
		/// </summary>
		public float GetEngineGearRatio(int gearNumber)
		{
			if (gearNumber < 0)
			{
				return reverseGearRatio;
			}

			int gearIndex = gearNumber - 1;
			if (forwardGearRatios != null && gearIndex >= 0 && gearIndex < forwardGearRatios.Length)
			{
				return forwardGearRatios[gearIndex];
			}

			return 0.0f;
		}

		/// <summary>
		/// Get number of reverse gear ratios.
		/// Exposed for plugin use.
		/// </summary>
		public int ReverseGearsCount => 1;

		/// <summary>
		/// Get number of forward gear ratios.
		/// Exposed for plugin use.
		/// </summary>
		public int ForwardGearsCount => forwardGearRatios?.Length ?? 0;

		public override EAssetType assetCategory => EAssetType.VEHICLE;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_vehicleName = p.localization.format("Name");

			// -1 values are resolved once model is loaded.
			_pitchIdle = p.data.ParseFloat("Pitch_Idle", defaultValue: -1);
			_pitchDrive = p.data.ParseFloat("Pitch_Drive", defaultValue: -1);

			// Engine checks for relevant subobjects once model is loaded.
			_engine = p.data.ParseEnum("Engine", defaultValue: EEngine.CAR);

			// If null then model loading may reassign.
			physicsProfileRef = p.data.readAssetReference<VehiclePhysicsProfileAsset>("Physics_Profile");

#pragma warning disable
			int defaultNumSteeringTires = engine == EEngine.CAR ? 2 : 1;
			_hasCrawler = p.data.ContainsKey("Crawler");
			if (hasCrawler)
				defaultNumSteeringTires = 0;
			numSteeringTires = p.data.ParseInt32("Num_Steering_Tires", defaultValue: defaultNumSteeringTires);
			steeringTireIndices = new int[numSteeringTires];
			for (int index = 0; index < numSteeringTires; ++index)
			{
				steeringTireIndices[index] = p.data.ParseInt32("Steering_Tire_" + index, defaultValue: index);
			}
#pragma warning restore

			if (Dedicator.IsDedicatedServer && p.data.ParseBool("Has_Clip_Prefab", defaultValue: true))
			{
				p.bundle.loadDeferred("Clip", out legacyServerModel, OnServerModelLoaded);
			}
			// Since we will not know until later whether Clip.prefab exists - and it is considered legacy/deprecated anyway,
			// we always load the client vehicle just in case.
			p.bundle.loadDeferred("Vehicle", out clientModel, OnClientModelLoaded);

			_size2_z = p.data.ParseFloat("Size2_Z");
			_sharedSkinName = p.data.GetString("Shared_Skin_Name");
			if (p.data.ContainsKey("Shared_Skin_Lookup_ID"))
			{
#pragma warning disable
				_sharedSkinLookupID = p.data.ParseGuidOrLegacyId("Shared_Skin_Lookup_ID", out _sharedSkinLookupGuid);
#pragma warning restore
			}
			else
			{
				_sharedSkinLookupGuid = GUID;
#pragma warning disable
				_sharedSkinLookupID = id;
#pragma warning restore
			}

			if (p.data.ContainsKey("Rarity"))
			{
				_rarity = (EItemRarity) System.Enum.Parse(typeof(EItemRarity), p.data.GetString("Rarity"), true);
			}
			else
			{
				_rarity = EItemRarity.COMMON;
			}

			_hasZip = p.data.ContainsKey("Zip");
			_hasBicycle = p.data.ContainsKey("Bicycle");
			isReclined = p.data.ContainsKey("Reclined");
			_hasLockMouse = p.data.ContainsKey("LockMouse");
			_hasTraction = p.data.ContainsKey("Traction");
			_hasSleds = p.data.ContainsKey("Sleds");

			canSpawnWithBattery = !p.data.ContainsKey("Cannot_Spawn_With_Battery");
			if (p.data.ContainsKey("Battery_Spawn_Charge_Multiplier"))
				batterySpawnChargeMultiplier = p.data.ParseFloat("Battery_Spawn_Charge_Multiplier");
			else
				batterySpawnChargeMultiplier = 1;

			if (p.data.ContainsKey("Battery_Burn_Rate"))
				batteryBurnRate = p.data.ParseFloat("Battery_Burn_Rate");
			else
				batteryBurnRate = 20;

			if (p.data.ContainsKey("Battery_Charge_Rate"))
				batteryChargeRate = p.data.ParseFloat("Battery_Charge_Rate");
			else
				batteryChargeRate = 20;

			batteryDriving = p.data.ParseEnum("BatteryMode_Driving", defaultValue: EBatteryMode.Charge);
			batteryEmpty = p.data.ParseEnum("BatteryMode_Empty", defaultValue: EBatteryMode.None);
			batteryHeadlights = p.data.ParseEnum("BatteryMode_Headlights", defaultValue: EBatteryMode.Burn);
			batterySirens = p.data.ParseEnum("BatteryMode_Sirens", defaultValue: EBatteryMode.Burn);
			defaultBatteryGuid = p.data.ParseGuid("Default_Battery", defaultValue: VANILLA_BATTERY_ITEM);

			// Original values were every 6 sim ticks for car, 3 otherwise at 12.5 sim ticks
			// 12.5 / 6 = 2.083, 12.5 / 3 = 4.166
			float defaultFuelBurnRate = engine == EEngine.CAR ? 2.05f : 4.2f;
			fuelBurnRate = p.data.ParseFloat("Fuel_Burn_Rate", defaultValue: defaultFuelBurnRate);

			_ignition = LoadRedirectableAsset<AudioClip>(p.bundle, "Ignition", p.data, "IgnitionAudioClip");

			// Check for horn audio clip on dedicated server because Has_Horn is optional.
			_horn = LoadRedirectableAsset<AudioClip>(p.bundle, "Horn", p.data, "HornAudioClip");
			hasHorn = p.data.ParseBool("Has_Horn", defaultValue: _horn != null);

			TargetReverseVelocity = p.data.ParseFloat("Speed_Min");
			TargetForwardVelocity = p.data.ParseFloat("Speed_Max");

			if (engine != EEngine.TRAIN)
			{
				TargetForwardVelocity *= 1.25f;
			}

			// Nelson 2025-09-29: finally renaming this property to remove old multiplication by 75%.
			if (!p.data.TryParseFloat("Steering_Angle_Max", out _steerMax))
			{
				_steerMax = p.data.ParseFloat("Steer_Max") * 0.75f;
			}

			if (p.data.TryParseFloat("Steering_Angle_FullSpeed_Factor", out float factor))
			{
				_steerMin = _steerMax * factor;
			}
			else
			{
				_steerMin = p.data.ParseFloat("Steer_Min");
			}

			CrawlerTrackSteeringTorque = p.data.ParseFloat("CrawlerTrackSteering_Torque");
			CrawlerTrackSteeringSidewaysFrictionMultiplier = p.data.ParseFloat("CrawlerTrackSteering_SidewaysFrictionMultiplier", defaultValue: 1.0f);
			CrawlerTrackSteeringMaxSpeedScale = p.data.ParseFloat("CrawlerTrackSteering_MaxSpeedScale", defaultValue: 1.0f);

			SteeringAngleTurnSpeed = p.data.ParseFloat("Steering_Angle_Turn_Speed", _steerMax * 5.0f);
			steeringLeaningForceMultiplier = p.data.ParseFloat("Steering_LeaningForceMultiplier", defaultValue: -1.0f);
			steeringLeaningForceShouldScaleWithSpeed = p.data.ParseBool("Steering_LeaningForce_ScaleWithSpeed", false);
			if (steeringLeaningForceShouldScaleWithSpeed)
			{
				steeringLeaningForceSpeedExponent = p.data.ParseFloat("Steering_LeaningForce_SpeedExponent", defaultValue: 1.0f);
			}

			_brake = p.data.ParseFloat("Brake");
			_lift = p.data.ParseFloat("Lift");

			_fuelMin = p.data.ParseUInt16("Fuel_Min");
			_fuelMax = p.data.ParseUInt16("Fuel_Max");
			_fuel = p.data.ParseUInt16("Fuel");

			_healthMin = p.data.ParseUInt16("Health_Min");
			_healthMax = p.data.ParseUInt16("Health_Max");
			_health = p.data.ParseUInt16("Health");

			_explosion = p.data.ParseGuidOrLegacyId("Explosion", out _explosionEffectGuid);

			bool hasExplosion = !IsExplosionEffectRefNull();
			ShouldExplosionCauseDamage = p.data.ParseBool("ShouldExplosionCauseDamage", defaultValue: hasExplosion);
			ShouldExplosionBurnMaterials = p.data.ParseBool("ShouldExplosionBurnMaterials", defaultValue: hasExplosion);
			if (ShouldExplosionBurnMaterials)
			{
				explosionBurnMaterialSections = p.data.ParseArrayOfStructs<PaintableVehicleSection>("ExplosionBurnMaterialSections");
			}

			float explosionForceMultiplier = p.data.ParseFloat("Explosion_Force_Multiplier", defaultValue: 1.0f);

			// Nelson 2024-03-25: TryParseVector3 before LegacyParseVector3 is sort of redundant because it internally
			// calls TryParseVector3, but I've done it this way to fix using the newer format while maintaining the old
			// behavior. (public issue #4399)
			if (p.data.TryParseVector3("Explosion_Min_Force", out Vector3 tempMinExplosionForce))
			{
				minExplosionForce = tempMinExplosionForce * explosionForceMultiplier;
			}
			else
			{
				if (p.data.ContainsKey("Explosion_Min_Force_Y"))
				{
					minExplosionForce = p.data.LegacyParseVector3("Explosion_Min_Force") * explosionForceMultiplier;
				}
				else
				{
					minExplosionForce = new Vector3(0.0f, 1024 * explosionForceMultiplier, 0.0f);
				}
			}
			if (p.data.TryParseVector3("Explosion_Max_Force", out Vector3 tempMaxExplosionForce))
			{
				maxExplosionForce = tempMaxExplosionForce * explosionForceMultiplier;
			}
			else
			{
				if (p.data.ContainsKey("Explosion_Max_Force_Y"))
				{
					maxExplosionForce = p.data.LegacyParseVector3("Explosion_Max_Force") * explosionForceMultiplier;
				}
				else
				{
					maxExplosionForce = new Vector3(0.0f, 1024 * explosionForceMultiplier, 0.0f);
				}
			}

			if (p.data.ContainsKey("Exit"))
			{
				_exit = p.data.ParseFloat("Exit");
			}
			else
			{
				_exit = 2f;
			}

			if (p.data.ContainsKey("Cam_Follow_Distance"))
			{
				_camFollowDistance = p.data.ParseFloat("Cam_Follow_Distance");
			}
			else
			{
				_camFollowDistance = 5.5f;
			}

			camDriverOffset = p.data.ParseFloat("Cam_Driver_Offset");
			camPassengerOffset = p.data.ParseFloat("Cam_Passenger_Offset");

			if (p.data.ContainsKey("Bumper_Multiplier"))
			{
				_bumperMultiplier = p.data.ParseFloat("Bumper_Multiplier");
			}
			else
			{
				_bumperMultiplier = 1.0f;
			}

			BumperSpeedDamageThreshold = p.data.ParseFloat("Bumper_SpeedDamageThreshold", 3f);
			BumperPlayerDamage = p.data.ParseFloat("Bumper_PlayerDamage", defaultValue: 10f);
			BumperZombieDamage = p.data.ParseFloat("Bumper_ZombieDamage", defaultValue: 15f);
			BumperAnimalDamage = p.data.ParseFloat("Bumper_AnimalDamage", defaultValue: 15f);
			BumperObjectDamage = p.data.ParseFloat("Bumper_ObjectDamage", defaultValue: 30f);
			BumperResourceDamage = p.data.ParseFloat("Bumper_ResourceDamage", defaultValue: 85f);
			BumperSelfDamageMultiplier = p.data.ParseFloat("Bumper_SelfDamageMultiplier", 1f);

			if (p.data.ContainsKey("Passenger_Explosion_Armor"))
			{
				_passengerExplosionArmor = p.data.ParseFloat("Passenger_Explosion_Armor");
			}
			else
			{
				_passengerExplosionArmor = 1.0f;
			}

			if (engine == EEngine.HELICOPTER || engine == EEngine.BLIMP)
			{
				_sqrDelta = MathfEx.Square(TargetForwardVelocity * 0.125f);
			}
			else
			{
				_sqrDelta = MathfEx.Square(TargetForwardVelocity * 0.1f);
			}
			if (p.data.ContainsKey("Valid_Speed_Horizontal"))
			{
				float validSpeedHorizontal = p.data.ParseFloat("Valid_Speed_Horizontal");
				float validHorizontalDistancePerInput = validSpeedHorizontal * PlayerInput.RATE;
				_sqrDelta = MathfEx.Square(validHorizontalDistancePerInput);
			}

			float defaultValidSpeedUp;
			float defaultValidSpeedDown;
			switch (engine)
			{
				case EEngine.CAR:
					defaultValidSpeedUp = 12.5f;
					defaultValidSpeedDown = 25.0f;
					break;

				case EEngine.BOAT:
					defaultValidSpeedUp = 3.25f;
					defaultValidSpeedDown = 25.0f;
					break;

				default:
					defaultValidSpeedUp = 100;
					defaultValidSpeedDown = 100;
					break;
			}

			validSpeedUp = p.data.ParseFloat("Valid_Speed_Up", defaultValue: defaultValidSpeedUp);
			validSpeedDown = p.data.ParseFloat("Valid_Speed_Down", defaultValue: defaultValidSpeedDown);
			ValidHitDistanceMultiplier = p.data.ParseFloat("Valid_Hit_Distance_Multiplier", 1.0f);

			_turrets = new TurretInfo[p.data.ParseUInt8("Turrets")];
			for (byte turretIndex = 0; turretIndex < turrets.Length; turretIndex++)
			{
				TurretInfo info = new TurretInfo();
				info.seatIndex = p.data.ParseUInt8("Turret_" + turretIndex + "_Seat_Index");
				info.itemID = p.data.ParseUInt16("Turret_" + turretIndex + "_Item_ID");
				info.yawMin = p.data.ParseFloat("Turret_" + turretIndex + "_Yaw_Min");
				info.yawMax = p.data.ParseFloat("Turret_" + turretIndex + "_Yaw_Max");
				info.pitchMin = p.data.ParseFloat("Turret_" + turretIndex + "_Pitch_Min");
				info.pitchMax = p.data.ParseFloat("Turret_" + turretIndex + "_Pitch_Max");
				info.useAimCamera = !p.data.ContainsKey("Turret_" + turretIndex + "_Ignore_Aim_Camera");

				_turrets[turretIndex] = info;
			}

			isVulnerable = !p.data.ContainsKey("Invulnerable");
			isVulnerableToExplosions = !p.data.ContainsKey("Explosions_Invulnerable");
			isVulnerableToEnvironment = !p.data.ContainsKey("Environment_Invulnerable");
			isVulnerableToBumper = !p.data.ContainsKey("Bumper_Invulnerable");
			canTiresBeDamaged = !p.data.ContainsKey("Tires_Invulnerable");
			canRepairWhileSeated = p.data.ParseBool("Can_Repair_While_Seated", defaultValue: false);

			childExplosionArmorMultiplier = p.data.ParseFloat("Child_Explosion_Armor_Multiplier", defaultValue: 0.2f);

			if (p.data.ContainsKey("Air_Turn_Responsiveness"))
			{
				airTurnResponsiveness = p.data.ParseFloat("Air_Turn_Responsiveness");
			}
			else
			{
				airTurnResponsiveness = 2;
			}

			if (p.data.ContainsKey("Air_Steer_Min"))
			{
				airSteerMin = p.data.ParseFloat("Air_Steer_Min");
			}
			else
			{
				airSteerMin = MaxSteeringAngleAtFullSpeed;
			}

			if (p.data.ContainsKey("Air_Steer_Max"))
			{
				airSteerMax = p.data.ParseFloat("Air_Steer_Max");
			}
			else
			{
				airSteerMax = MaxSteeringAngle;
			}

			bicycleAnimSpeed = p.data.ParseFloat("Bicycle_Anim_Speed");
			staminaBoost = p.data.ParseFloat("Stamina_Boost");
			useStaminaBoost = p.data.ContainsKey("Stamina_Boost");
			isStaminaPowered = p.data.ContainsKey("Stamina_Powered");
			isBatteryPowered = p.data.ContainsKey("Battery_Powered");

			if (p.data.TryParseEnum("Buildable_Placement_Rule", out EVehicleBuildablePlacementRule rule))
			{
				BuildablePlacementRule = rule;
			}
			else if (p.data.ContainsKey("Supports_Mobile_Buildables"))
			{
				BuildablePlacementRule = EVehicleBuildablePlacementRule.AlwaysAllow;
			}
			else
			{
				BuildablePlacementRule = EVehicleBuildablePlacementRule.None;
			}

			shouldSpawnSeatCapsules = p.data.ParseBool("Should_Spawn_Seat_Capsules");

			canBeLocked = p.data.ParseBool("Can_Be_Locked", defaultValue: true);
			canStealBattery = p.data.ParseBool("Can_Steal_Battery", defaultValue: true);

			trunkStorage_X = p.data.ParseUInt8("Trunk_Storage_X");
			trunkStorage_Y = p.data.ParseUInt8("Trunk_Storage_Y");

			dropsTableId = p.data.ParseUInt16("Drops_Table_ID", defaultValue: 962); // Destroyed_Vehicle_Default
			dropsMin = p.data.ParseUInt8("Drops_Min", defaultValue: 3);
			dropsMax = p.data.ParseUInt8("Drops_Max", defaultValue: 7);

			tireID = p.data.ParseUInt16("Tire_ID", defaultValue: 1451);

			hasCenterOfMassOverride = p.data.ParseBool("Override_Center_Of_Mass");
			if (hasCenterOfMassOverride)
			{
				centerOfMass = p.data.LegacyParseVector3("Center_Of_Mass");
			}

			carjackForceMultiplier = p.data.ParseFloat("Carjack_Force_Multiplier", defaultValue: 1.0f);
			engineForceMultiplier = p.data.ParseFloat("Engine_Force_Multiplier", defaultValue: 1.0f);

			if (p.data.ContainsKey("Wheel_Collider_Mass_Override"))
			{
				wheelColliderMassOverride = p.data.ParseFloat("Wheel_Collider_Mass_Override", defaultValue: 1.0f);
			}
			else
			{
				wheelColliderMassOverride = null;
			}

			trainTrackOffset = p.data.ParseFloat("Train_Track_Offset");
			trainWheelOffset = p.data.ParseFloat("Train_Wheel_Offset");
			trainCarLength = p.data.ParseFloat("Train_Car_Length");

			_shouldVerifyHash = !p.data.ContainsKey("Bypass_Hash_Verification");

			// Only official content has skins, so we only check the official ID range of [1, 2000).
			if (!Dedicator.IsDedicatedServer && id < 2000)
			{
				_albedoBase = p.bundle.load<Texture2D>("Albedo_Base");
				_metallicBase = p.bundle.load<Texture2D>("Metallic_Base");
				_emissionBase = p.bundle.load<Texture2D>("Emission_Base");
			}

			CanDecay = engine != EEngine.TRAIN && (isVulnerable | isVulnerableToExplosions | isVulnerableToEnvironment | isVulnerableToBumper);

			PaintableVehicleSections = p.data.ParseArrayOfStructs<PaintableVehicleSection>("PaintableSections");
			if (SupportsPaintColor)
			{
				IsPaintable = p.data.ParseBool("IsPaintable", defaultValue: true);
			}
			else
			{
				IsPaintable = false;
			}

			crawlerTrackTilingMaterials = p.data.ParseArrayOfStructs<CrawlerTrackTilingMaterial>("CrawlerTrackTilingMaterials");

			if (p.data.TryGetList("AdditionalTransparentSections", out IDatList datList))
			{
				extraTransparentSections = datList.ParseArrayOfStructs<PaintableVehicleSection>();
			}

			bool hasDefaultPaintColorsList = p.data.TryGetList("DefaultPaintColors", out IDatList paintColorsList);
			defaultPaintColorMode = p.data.ParseEnum("DefaultPaintColor_Mode", hasDefaultPaintColorsList ?
					EVehicleDefaultPaintColorMode.List : EVehicleDefaultPaintColorMode.None);
			if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.List)
			{
				DefaultPaintColors = new List<Color32>(paintColorsList.Count);
				foreach (IDatNode node in paintColorsList)
				{
					if (node is IDatValue value && value.TryParseColor32RGB(out Color32 color))
					{
						DefaultPaintColors.Add(color);
					}
				}
			}
			else if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.RandomHueOrGrayscale)
			{
				randomPaintColorConfiguration = new VehicleRandomPaintColorConfiguration();
				if (p.data.TryGetDictionary("DefaultPaintColor_Configuration", out IDatDictionary paintDat))
				{
					if (!randomPaintColorConfiguration.TryParse(paintDat))
					{
						Assets.ReportError(this, "unable to parse DefaultPaintColor_Configuration");
					}
				}
				else
				{
					Assets.ReportError(this, "missing DefaultPaintColor_Configuration");
				}
			}

			wheelBalancingForceMultiplier = p.data.ParseFloat("WheelBalancing_ForceMultiplier", defaultValue: -1.0f);
			wheelBalancingUprightExponent = p.data.ParseFloat("WheelBalancing_UprightExponent", defaultValue: 1.5f);
			rollAngularVelocityDamping = p.data.ParseFloat("RollAngularVelocityDamping", defaultValue: -1.0f);
			ShouldIncludeAirbornWheelsInAverageRpm = p.data.ParseBool("Include_Airborn_Wheels_In_Average_RPM", defaultValue: true);

			if (p.data.TryGetList("WheelConfigurations", out IDatList wheelConfigurationsList))
			{
				List<VehicleWheelConfiguration> pendingWheelConfigurations = new List<VehicleWheelConfiguration>();
				List<int> pendingReplicatedWheelIndices = new List<int>();
				List<int> pendingPoweredWheelIndices = new List<int>();
				foreach (IDatNode node in wheelConfigurationsList)
				{
					VehicleWheelConfiguration wheelConfiguration = new VehicleWheelConfiguration();
					if (wheelConfiguration.TryParse(node))
					{
						if (wheelConfiguration.modelUseColliderPose)
						{
							int index = pendingWheelConfigurations.Count;
							pendingReplicatedWheelIndices.Add(index);
						}
						if (wheelConfiguration.isColliderPowered)
						{
							int index = pendingWheelConfigurations.Count;
							pendingPoweredWheelIndices.Add(index);
						}
						pendingWheelConfigurations.Add(wheelConfiguration);
					}
					else
					{
						Assets.reportError($"Unable to parse entry in WheelConfigurations list: {node.DebugDumpToString()}");
					}
				}
				wheelConfiguration = pendingWheelConfigurations.ToArray();
				if (pendingReplicatedWheelIndices.Count > 0)
				{
					replicatedWheelIndices = pendingReplicatedWheelIndices.ToArray();
				}
				if (pendingPoweredWheelIndices.Count > 0)
				{
					poweredWheelIndices = pendingPoweredWheelIndices.ToArray();
				}
			}

			reverseGearRatio = p.data.ParseFloat("ReverseGearRatio", defaultValue: 1.0f);
			if (p.data.TryGetList("ForwardGearRatios", out IDatList gearRatiosList))
			{
				List<float> pendingGearRatios = new List<float>();
				foreach (IDatNode node in gearRatiosList)
				{
					if (node is IDatValue value && value.TryParseFloat(out float ratio))
					{
						pendingGearRatios.Add(ratio);
					}
				}
				if (pendingGearRatios.Count > 0)
				{
					forwardGearRatios = pendingGearRatios.ToArray();
					AllowsEngineRpmAndGearsInHud = p.data.ParseBool("GearShift_VisibleInHUD", defaultValue: true);
				}
			}
			GearShiftDownThresholdRpm = p.data.ParseFloat("GearShift_DownThresholdRPM", defaultValue: 1500.0f);
			GearShiftUpThresholdRpm = p.data.ParseFloat("GearShift_UpThresholdRPM", defaultValue: 5500.0f);
			GearShiftDuration = p.data.ParseFloat("GearShift_Duration", defaultValue: 0.5f);
			GearShiftInterval = p.data.ParseFloat("GearShift_Interval", defaultValue: 1.0f);
			GearShiftAllowSkippingGears = p.data.ParseBool("GearShift_AllowSkippingGears", defaultValue: true);

			EngineIdleRpm = p.data.ParseFloat("EngineIdleRPM", 1000.0f);
			EngineMaxRpm = p.data.ParseFloat("EngineMaxRPM", 7000.0f);
			EngineRpmIncreaseRate = p.data.ParseFloat("EngineRPM_IncreaseRate", -1f);
			EngineRpmDecreaseRate = p.data.ParseFloat("EngineRPM_DecreaseRate", -1f);
			EngineMaxTorque = p.data.ParseFloat("EngineMaxTorque", 1.0f);

			EngineRpmMismatchTorqueReductionEnabled = p.data.ParseBool("EngineRPMMismatch_TorqueReduction_Enabled");
			EngineRpmMismatchTorqueReductionThreshold = p.data.ParseFloat("EngineRPMMismatch_TorqueReduction_Threshold");

			EngineRpmMismatchGearShiftPreventShifting = p.data.ParseBool("EngineRPMMismatch_GearShift_PreventShifting");
			if (EngineRpmMismatchGearShiftPreventShifting)
			{
				EngineRpmMismatchGearShiftUpMinThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_UpMinThreshold");
				EngineRpmMismatchGearShiftUpMaxThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_UpMaxThreshold");
				EngineRpmMismatchGearShiftDownMinThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_DownMinThreshold");
				EngineRpmMismatchGearShiftDownMaxThreshold = p.data.ParseFloat("EngineRpmMismatch_GearShift_DownMaxThreshold");
			}

			engineSoundType = p.data.ParseEnum("EngineSound_Type", EVehicleEngineSoundType.Legacy);
			if (engineSoundType == EVehicleEngineSoundType.EngineRPMSimple)
			{
				engineSoundConfiguration = new RpmEngineSoundConfiguration();
				if (p.data.TryGetDictionary("EngineSound", out IDatDictionary engineSoundNode))
				{
					engineSoundConfiguration.TryParse(engineSoundNode);
				}
			}

			if (UsesEngineRpmAndGears && Assets.shouldValidateAssets)
			{
				GameObject vehicleRoot = GetOrLoadModel();
				if (vehicleRoot != null)
				{
					EngineCurvesComponent curvesComponent = vehicleRoot.GetComponent<EngineCurvesComponent>();
					if (curvesComponent == null)
					{
						Assets.ReportError(this, "needs EngineCurvesComponent on vehicle prefab for engine RPM and gearbox to work properly");
					}
				}
			}

			this.PopulateArmorFalloff(in p); // this. is necessary, at least in current C# version.
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Locale_Vehicle
			CargoDeclaration en = builder.GetOrAddDeclaration("Locale_Vehicle");
			en.Append("GUID", GUID); // PFK
			en.Append("Name", FriendlyName);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Vehicle
			CargoDeclaration data = builder.GetOrAddDeclaration("Vehicle");
			data.Append("GUID", GUID); // PFK

			data.Append("Air_Steer_Max", airSteerMax);
			data.Append("Air_Steer_Min", airSteerMin);
			data.Append("Air_Turn_Responsiveness", airTurnResponsiveness);
			data.Append("BatteryMode_Driving", batteryDriving);
			data.Append("BatteryMode_Empty", batteryEmpty);
			data.Append("BatteryMode_Headlights", batteryHeadlights);
			data.Append("BatteryMode_Sirens", batterySirens);
			data.Append("Battery_Burn_Rate", batteryBurnRate);
			data.Append("Battery_Charge_Rate", batteryChargeRate);
			data.Append("Battery_Powered", isBatteryPowered);
			data.Append("Battery_Spawn_Charge_Multiplier", batterySpawnChargeMultiplier);
			data.Append("Bicycle", hasBicycle);
			data.Append("Buildable_Placement_Rule", BuildablePlacementRule);
			data.Append("isVulnerableToBumper", isVulnerableToBumper); // Derived from "Bumper_Invulnerable".
			data.Append("Bumper_Multiplier", bumperMultiplier);
			data.Append("Brake", brake);
			data.Append("Can_Be_Locked", canBeLocked);
			data.Append("Can_Repair_While_Seated", canRepairWhileSeated);
			data.Append("Can_Steal_Battery", canStealBattery);
			data.Append("canSpawnWithBattery", canSpawnWithBattery); // Derived from "Cannot_Spawn_With_Battery".
			data.Append("Carjack_Force_Multiplier", carjackForceMultiplier);
			data.Append("Child_Explosion_Armor_Multiplier", childExplosionArmorMultiplier);
			data.Append("Crawler", _hasCrawler);
			data.Append("DefaultPaintColor_Mode", defaultPaintColorMode);
			data.Append("Default_Battery", defaultBatteryGuid);
			data.Append("Drops_Max", dropsMax);
			data.Append("Drops_Min", dropsMin);
			data.Append("Drops_Table_ID", dropsTableId);
			data.Append("Engine", engine);
			data.Append("EngineIdleRPM", EngineIdleRpm);
			data.Append("EngineMaxRPM", EngineMaxRpm);
			data.Append("EngineMaxTorque", EngineMaxTorque);
			data.Append("EngineRPM_DecreaseRate", EngineRpmDecreaseRate);
			data.Append("EngineRPM_IncreaseRate", EngineRpmIncreaseRate);
			data.Append("Engine_Force_Multiplier", engineForceMultiplier);
			data.Append("isVulnerableToEnvironment", isVulnerableToEnvironment); // Derived from "Environment_Invulnerable".
			data.Append("isVulnerableToExplosions", isVulnerableToExplosions); // Derived from "Explosions_Invulnerable".
			data.Append("Fuel", fuel);
			data.Append("Fuel_Burn_Rate", fuelBurnRate);
			data.Append("Fuel_Max", fuelMax);
			data.Append("Fuel_Min", fuelMin);
			data.Append("Has_Horn", hasHorn);
			data.Append("Health", health);
			data.Append("Health_Max", healthMax);
			data.Append("Health_Min", healthMin);
			data.Append("HornAudioClip", (object) horn);
			data.Append("isVulnerable", isVulnerable); // Derived from "Invulnerable".
			data.Append("IsPaintable", IsPaintable);
			data.Append("Lift", lift);
			data.Append("LockMouse", hasLockMouse);
			data.Append("Passenger_Explosion_Armor", passengerExplosionArmor);
			data.Append("Physics_Profile", physicsProfileRef);
			data.Append("Rarity", rarity);
			data.Append("ShouldExplosionCauseDamage", ShouldExplosionCauseDamage);
			data.Append("Should_Spawn_Seat_Capsules", shouldSpawnSeatCapsules);
			data.Append("Sleds", hasSleds);
			data.Append("TargetForwardVelocity", TargetForwardVelocity); // Derived from "Speed_Max".
			data.Append("TargetReverseVelocity", TargetReverseVelocity); // Derived from "Speed_Min".
			data.Append("Stamina_Boost", staminaBoost);
			data.Append("Stamina_Powered", isStaminaPowered);
			data.Append("steerMax", MaxSteeringAngle); // Derived from "Steer_Max".
			data.Append("steerMin", MaxSteeringAngleAtFullSpeed); // Derived from "Steer_Min".
			data.Append("Steering_Angle_Turn_Speed", SteeringAngleTurnSpeed);
			data.Append("Steering_LeaningForceMultiplier", steeringLeaningForceMultiplier);
			data.Append("Tire_ID", tireID);
			data.Append("canTiresBeDamaged", canTiresBeDamaged); // Derived from "Tires_Invulnerable".
			data.Append("Traction", hasTraction);
			data.Append("Trunk_Storage_X", trunkStorage_X);
			data.Append("Trunk_Storage_Y", trunkStorage_Y);
			data.Append("Turrets", turrets.Length); // Get original value of "Turrets".
			data.Append("Valid_Speed_Down", validSpeedDown);
			data.Append("Valid_Speed_Up", validSpeedUp);
			data.Append("Valid_Hit_Distance_Multiplier", ValidHitDistanceMultiplier);

			// Appending Unity characteristics.
			data.Append("hasSirens", hasSirens);
			data.Append("hasHook", hasHook);
			data.Append("hasHeadlights", hasHeadlights);
			data.Append("numSeats", numSeats);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Vehicle_VehicleRandomPaintColorConfiguration
			// Child table for the "DefaultPaintColor_Configuration" dictionary.
			if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.RandomHueOrGrayscale)
			{
				CargoDeclaration hue = builder.GetOrAddDeclaration("Vehicle_VehicleRandomPaintColorConfiguration");
				hue.Append("GUID", GUID); // FK

				hue.Append("MinSaturation", randomPaintColorConfiguration.minSaturation);
				hue.Append("MaxSaturation", randomPaintColorConfiguration.maxSaturation);
				hue.Append("MinValue", randomPaintColorConfiguration.minValue);
				hue.Append("MaxValue", randomPaintColorConfiguration.maxValue);
				hue.Append("GrayscaleChance", randomPaintColorConfiguration.grayscaleChance);
			}

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Vehicle_DefaultPaintColors
			// Keyless child table for the "DefaultPaintColors" list.
			//
			// Molt 2024-12-02: Normally, we wouldn't want to have keyless tables. Although we could make a composite key from array indices, there's no benefit this. Since array indices could change, relying on them would be more error-prone than not, as we wouldn't query a single row unless we already knew both column values anyway. Every query should want to fetch all relevant rows.
			if (defaultPaintColorMode == EVehicleDefaultPaintColorMode.List)
			{
				for (byte index = 0; index < DefaultPaintColors.Count; index++) {
					CargoDeclaration color = builder.AddDeclaration("Vehicle_DefaultPaintColors");
					color.Append("GUID", GUID); // FK
					color.Append("Color", DefaultPaintColors[index]);
				}
			}

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Vehicle_TurretInfo
			// Child table for TurretInfo class data.
			// Our composite key is formed from the vehicle's GUID and turretIndex.
			//
			// Molt 2024-12-02: All of our columns (besides GUID) are non-unique, so we use turretIndex as it's the least likely to change. seatIndex was another option, but we should avoid querying columns for unexpected reasons. Turrets could use GUIDs in the future, which would be unique and should replace turretIndex entirely if/when that happens.
			for (byte turretIndex = 0; turretIndex < turrets.Length; turretIndex++) {
				CargoDeclaration turret = builder.AddDeclaration("Vehicle_TurretInfo");
				turret.Append("GUID", GUID); // FK
				turret.Append("turretIndex", turretIndex); // non-unique, and susceptible to change (e.g., if a vehicle has a turret added/removed)

				turret.Append("useAimCamera", turrets[turretIndex].useAimCamera); // Derived from "Ignore_Aim_Camera".
				turret.Append("Item_ID", turrets[turretIndex].itemID);
				turret.Append("Pitch_Max", turrets[turretIndex].pitchMax);
				turret.Append("Pitch_Min", turrets[turretIndex].pitchMin);
				turret.Append("Seat_Index", turrets[turretIndex].seatIndex);
				turret.Append("Yaw_Max", turrets[turretIndex].yawMax);
				turret.Append("Yaw_Min", turrets[turretIndex].yawMin);
			}
		}

		private static readonly System.Guid VANILLA_BATTERY_ITEM = new System.Guid("098b13be34a7411db7736b7f866ada69");

		#region Obsolete
		/// <summary>
		/// Number of tire visuals to rotate with steering wheel.
		/// </summary>
		[System.Obsolete("Replaced by VehicleWheelConfiguration. Only used for backwards compatibility.")]
		public int numSteeringTires
		{
			get;
			protected set;
		}

		[System.Obsolete("Replaced by VehicleWheelConfiguration. Only used for backwards compatibility.")]
		public int[] steeringTireIndices
		{
			get;
			protected set;
		}

		protected bool _hasCrawler;
		[System.Obsolete("Replaced by VehicleWheelConfiguration. Only used for backwards compatibility.")]
		public bool hasCrawler => _hasCrawler;

		[System.Obsolete("Renamed to TargetReverseVelocity.")]
		public float speedMin => TargetReverseVelocity;
		[System.Obsolete("Renamed to TargetForwardVelocity.")]
		public float speedMax => TargetForwardVelocity;

		[System.Obsolete("Renamed to MaxSteeringAngle")]
		public float steerMax => _steerMax;

		[System.Obsolete("Renamed to MaxSteeringAngleAtFullSpeed")]
		public float steerMin => _steerMin;
		#endregion Obsolete

		private static CommandLineFlag clLogWheelConfiguration = new CommandLineFlag(false, "-LogVehicleWheelConfigurations");
	}
}
