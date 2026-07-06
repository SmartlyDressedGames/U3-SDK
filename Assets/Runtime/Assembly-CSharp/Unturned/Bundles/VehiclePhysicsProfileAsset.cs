////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LOG_VEHICLE_PHYSICS_PROFILE

using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Overrides vehicle physics values in bulk without building asset bundles.
	/// </summary>
	public class VehiclePhysicsProfileAsset : Asset
	{
		public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Boat = new AssetReference<VehiclePhysicsProfileAsset>(new System.Guid("47258d0dcad14cb8be26e24c1ef3449e"));
		public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Car = new AssetReference<VehiclePhysicsProfileAsset>(new System.Guid("6b91a94f01b6472eaca31d9420ec2367"));
		public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Helicopter = new AssetReference<VehiclePhysicsProfileAsset>(new System.Guid("bb9f9f0204c4462ca7d976b87d1336d4"));
		public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Plane = new AssetReference<VehiclePhysicsProfileAsset>(new System.Guid("93a47d6d40454335b4784e803628ac54"));

		public float? rootMassOverride
		{
			get;
			protected set;
		}

		public float? rootMassMultiplier
		{
			get;
			protected set;
		}

		public float? rootDragMultiplier
		{
			get;
			protected set;
		}

		public float? rootAngularDragMultiplier
		{
			get;
			protected set;
		}

		public float? carjackForceMultiplier
		{
			get;
			protected set;
		}

		public float? wheelMassOverride
		{
			get;
			protected set;
		}

		public float? wheelMassMultiplier
		{
			get;
			protected set;
		}

		public float? wheelDampingRate
		{
			get;
			protected set;
		}

		public float? wheelStiffnessTractionMultiplier
		{
			get;
			protected set;
		}

		public float? wheelSuspensionForce
		{
			get;
			protected set;
		}

		public float? wheelSuspensionDamper
		{
			get;
			protected set;
		}

		public struct Friction
		{
			public float extremumSlip;
			public float extremumValue;
			public float asymptoteSlip;
			public float asymptoteValue;
			public float stiffness;

			public void applyTo(ref WheelFrictionCurve frictionCurve)
			{
				frictionCurve.extremumSlip = extremumSlip;
				frictionCurve.extremumValue = extremumValue;
				frictionCurve.asymptoteSlip = frictionCurve.asymptoteSlip;
				frictionCurve.asymptoteValue = frictionCurve.asymptoteValue;
			}
		}

		public Friction? forwardFriction
		{
			get;
			protected set;
		}

		public Friction? sidewaysFriction
		{
			get;
			protected set;
		}

		public float? motorTorqueMultiplier
		{
			get;
			protected set;
		}

		public float? motorTorqueClampMultiplier
		{
			get;
			protected set;
		}

		public float? brakeTorqueMultiplier
		{
			get;
			protected set;
		}

		public float? brakeTorqueTractionMultiplier
		{
			get;
			protected set;
		}

		public enum EDriveModel
		{
			Front,
			Rear,
			All,
		}

		public EDriveModel? wheelDriveModel
		{
			get;
			protected set;
		}

		public EDriveModel? wheelBrakeModel
		{
			get;
			protected set;
		}

		protected Friction? readFriction(IDatDictionary data, string key)
		{
			if (data.ContainsKey(key))
			{
				IDatDictionary frictionReader = data.GetDictionary(key);
				Friction friction = new Friction();
				friction.extremumSlip = frictionReader.ParseFloat("Extremum_Slip");
				friction.extremumValue = frictionReader.ParseFloat("Extremum_Value");
				friction.asymptoteSlip = frictionReader.ParseFloat("Asymptote_Slip");
				friction.asymptoteValue = frictionReader.ParseFloat("Asymptote_Value");
				friction.stiffness = frictionReader.ParseFloat("Stiffness");
				return friction;
			}
			else
			{
				return null;
			}
		}

		public void applyTo(InteractableVehicle vehicle)
		{
			Rigidbody rootRigidbody = vehicle.GetComponent<Rigidbody>();
			if (rootRigidbody != null)
			{
				if (rootMassOverride.HasValue)
				{
					log(vehicle, "changing root mass from {0} to {1}", rootRigidbody.mass, rootMassOverride.Value);
					rootRigidbody.mass = rootMassOverride.Value;
				}
				else if (rootMassMultiplier.HasValue)
				{
					log(vehicle, "multiplying root mass by {0} from {1} to {2}", rootMassMultiplier.Value, rootRigidbody.mass, rootRigidbody.mass * rootMassMultiplier.Value);
					rootRigidbody.mass *= rootMassMultiplier.Value;
				}

				if (rootDragMultiplier.HasValue)
				{
					log(vehicle, "multiplying root drag by {0} from {1} to {2}", rootDragMultiplier.Value, rootRigidbody.drag, rootRigidbody.drag * rootDragMultiplier.Value);
					rootRigidbody.drag *= rootDragMultiplier.Value;
				}

				if (rootAngularDragMultiplier.HasValue)
				{
					log(vehicle, "multiplying root angular drag by {0} from {1} to {2}", rootAngularDragMultiplier.Value, rootRigidbody.angularDrag, rootRigidbody.angularDrag * rootAngularDragMultiplier.Value);
					rootRigidbody.angularDrag *= rootAngularDragMultiplier.Value;
				}
			}

			bool shouldOverrideWheelMass = wheelMassOverride.HasValue && vehicle.asset.wheelColliderMassOverride.HasValue == false;
			bool shouldOverrideSpring = wheelSuspensionForce.HasValue || wheelSuspensionDamper.HasValue;

			foreach (Wheel tire in vehicle.tires)
			{
				if (tire.wheel == null)
					continue; // Null for purely visual wheels.

				if (wheelStiffnessTractionMultiplier.HasValue)
				{
					log(vehicle, "changing stiffness traction from {0} to {1}", tire.stiffnessTractionMultiplier, wheelStiffnessTractionMultiplier.Value);
					tire.stiffnessTractionMultiplier = wheelStiffnessTractionMultiplier.Value;
				}

				if (wheelDampingRate.HasValue)
				{
					log(vehicle, "changing wheel damping rate from {0} to {1}", tire.wheel.wheelDampingRate, wheelDampingRate.Value);
					tire.wheel.wheelDampingRate = wheelDampingRate.Value;
				}

				if (shouldOverrideSpring)
				{
					JointSpring jointSpring = tire.wheel.suspensionSpring;
					if (wheelSuspensionForce.HasValue)
					{
						log(vehicle, "changing suspension spring from {0} to {1}", jointSpring.spring, wheelSuspensionForce.Value);
						jointSpring.spring = wheelSuspensionForce.Value;
					}
					if (wheelSuspensionDamper.HasValue)
					{
						log(vehicle, "changing suspension damper from {0} to {1}", jointSpring.damper, wheelSuspensionDamper.Value);
						jointSpring.damper = wheelSuspensionDamper.Value;
					}
					tire.wheel.suspensionSpring = jointSpring;
				}

				if (sidewaysFriction.HasValue)
				{
					log(vehicle, "changing sideways stiffness from {0} to {1}", tire.stiffnessSideways, sidewaysFriction.Value.stiffness);
					tire.stiffnessSideways = sidewaysFriction.Value.stiffness;

					WheelFrictionCurve colliderSidewaysFriction = tire.wheel.sidewaysFriction;
					sidewaysFriction.Value.applyTo(ref colliderSidewaysFriction);
					tire.wheel.sidewaysFriction = colliderSidewaysFriction;
				}

				if (forwardFriction.HasValue)
				{
					log(vehicle, "changing forward stiffness from {0} to {1}", tire.stiffnessForward, forwardFriction.Value.stiffness);
					tire.stiffnessForward = forwardFriction.Value.stiffness;

					WheelFrictionCurve colliderForwardFriction = tire.wheel.forwardFriction;
					forwardFriction.Value.applyTo(ref colliderForwardFriction);
					tire.wheel.forwardFriction = colliderForwardFriction;
				}

				if (shouldOverrideWheelMass)
				{
					log(vehicle, "changing wheel mass from {0} to {1}", tire.wheel.mass, wheelMassOverride.Value);
					tire.wheel.mass = wheelMassOverride.Value;
				}
				else if (wheelMassMultiplier.HasValue)
				{
					log(vehicle, "multiplying wheel mass by {0} from {1} to {2}", wheelMassMultiplier.Value, tire.wheel.mass, tire.wheel.mass * wheelMassMultiplier.Value);
					tire.wheel.mass *= wheelMassMultiplier.Value;
				}

				if (motorTorqueMultiplier.HasValue)
				{
					log(vehicle, "changing motor torque multiplier from {0} to {1}", tire.motorTorqueMultiplier, motorTorqueMultiplier.Value);
					tire.motorTorqueMultiplier = motorTorqueMultiplier.Value;
				}

				if (motorTorqueClampMultiplier.HasValue)
				{
					log(vehicle, "changing motor torque clamp multiplier from {0} to {1}", tire.motorTorqueClampMultiplier, motorTorqueClampMultiplier.Value);
					tire.motorTorqueClampMultiplier = motorTorqueClampMultiplier.Value;
				}

				if (brakeTorqueMultiplier.HasValue)
				{
					log(vehicle, "changing brake torque multiplier from {0} to {1}", tire.brakeTorqueMultiplier, brakeTorqueMultiplier.Value);
					tire.brakeTorqueMultiplier = brakeTorqueMultiplier.Value;
				}

				if (brakeTorqueTractionMultiplier.HasValue)
				{
					log(vehicle, "changing brake torque traction multiplier from {0} to {1}", tire.brakeTorqueTractionMultiplier, brakeTorqueTractionMultiplier.Value);
					tire.brakeTorqueTractionMultiplier = brakeTorqueTractionMultiplier.Value;
				}

				if (wheelDriveModel.HasValue && tire.index >= 0)
				{
					log(vehicle, "changing wheel drive model to {0}", wheelDriveModel);

					switch (wheelDriveModel.Value)
					{
						case EDriveModel.Front:
							tire.isPowered = tire.index < 2;
							break;

						default: // Legacy
						case EDriveModel.Rear:
							tire.isPowered = tire.index >= 2;
							break;

						case EDriveModel.All:
							tire.isPowered = true;
							break;
					}
				}

				if (wheelBrakeModel.HasValue && tire.index >= 0)
				{
					log(vehicle, "changing wheel brake model to {0}", wheelBrakeModel);

					switch (wheelBrakeModel.Value)
					{
						case EDriveModel.Front:
							tire.hasBrakes = tire.index < 2;
							break;

						case EDriveModel.Rear:
							tire.hasBrakes = tire.index >= 2;
							break;

						default: // Legacy
						case EDriveModel.All:
							tire.hasBrakes = true;
							break;
					}
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.ContainsKey("Root_Mass"))
			{
				rootMassOverride = p.data.ParseFloat("Root_Mass");
			}

			if (p.data.ContainsKey("Root_Mass_Multiplier"))
			{
				rootMassMultiplier = p.data.ParseFloat("Root_Mass_Multiplier");
			}

			if (p.data.ContainsKey("Root_Drag_Multiplier"))
			{
				rootDragMultiplier = p.data.ParseFloat("Root_Drag_Multiplier");
			}

			if (p.data.ContainsKey("Root_Angular_Drag_Multiplier"))
			{
				rootAngularDragMultiplier = p.data.ParseFloat("Root_Angular_Drag_Multiplier");
			}

			if (p.data.ContainsKey("Carjack_Force_Multiplier"))
			{
				carjackForceMultiplier = p.data.ParseFloat("Carjack_Force_Multiplier");
			}

			if (p.data.ContainsKey("Wheel_Mass"))
			{
				wheelMassOverride = p.data.ParseFloat("Wheel_Mass");
			}

			if (p.data.ContainsKey("Wheel_Mass_Multiplier"))
			{
				wheelMassMultiplier = p.data.ParseFloat("Wheel_Mass_Multiplier");
			}

			if (p.data.ContainsKey("Wheel_Damping_Rate"))
			{
				wheelDampingRate = p.data.ParseFloat("Wheel_Damping_Rate");
			}

			if (p.data.ContainsKey("Wheel_Stiffness_Traction_Multiplier"))
			{
				wheelStiffnessTractionMultiplier = p.data.ParseFloat("Wheel_Stiffness_Traction_Multiplier");
			}

			if (p.data.ContainsKey("Wheel_Suspension_Force"))
			{
				wheelSuspensionForce = p.data.ParseFloat("Wheel_Suspension_Force");
			}

			if (p.data.ContainsKey("Wheel_Suspension_Damper"))
			{
				wheelSuspensionDamper = p.data.ParseFloat("Wheel_Suspension_Damper");
			}

			sidewaysFriction = readFriction(p.data, "Wheel_Friction_Sideways");
			forwardFriction = readFriction(p.data, "Wheel_Friction_Forward");

			if (p.data.ContainsKey("Motor_Torque_Multiplier"))
			{
				motorTorqueMultiplier = p.data.ParseFloat("Motor_Torque_Multiplier");
			}

			if (p.data.ContainsKey("Motor_Torque_Clamp_Multiplier"))
			{
				motorTorqueClampMultiplier = p.data.ParseFloat("Motor_Torque_Clamp_Multiplier");
			}

			if (p.data.ContainsKey("Brake_Torque_Multiplier"))
			{
				brakeTorqueMultiplier = p.data.ParseFloat("Brake_Torque_Multiplier");
			}

			if (p.data.ContainsKey("Brake_Torque_Traction_Multiplier"))
			{
				brakeTorqueTractionMultiplier = p.data.ParseFloat("Brake_Torque_Traction_Multiplier");
			}

			if (p.data.ContainsKey("Wheel_Drive_Model"))
			{
				wheelDriveModel = p.data.ParseEnum<EDriveModel>("Wheel_Drive_Model");
			}

			if (p.data.ContainsKey("Wheel_Brake_Model"))
			{
				wheelBrakeModel = p.data.ParseEnum<EDriveModel>("Wheel_Brake_Model");
			}
		}

		[System.Diagnostics.Conditional("LOG_VEHICLE_PHYSICS_PROFILE")]
		private void log(InteractableVehicle vehicle, string format, params object[] args)
		{
			UnturnedLog.info(vehicle.asset.name + ": " + format, args);
		}
	}
}
