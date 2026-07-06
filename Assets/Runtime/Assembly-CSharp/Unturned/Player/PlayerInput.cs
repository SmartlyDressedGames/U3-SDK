////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
//#define LAGSWITCH
//#define LOG_ASK_INPUT
//#define LOG_INPUT_RESIMULATION

using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void PluginKeyTickHandler(Player player, uint simulation, byte key, bool state);

	public class PlayerInputPacket
	{
		/// <summary>
		/// Worst case scenario, maybe shotgun hit or fast spray SMG.
		/// </summary>
		internal const int MAX_CLIENTSIDE_INPUTS = 16;

		public struct ClientRaycast
		{
			public RaycastInfo info;
			public ERaycastInfoUsage usage;

			public ClientRaycast(RaycastInfo info, ERaycastInfoUsage usage)
			{
				this.info = info;
				this.usage = usage;
			}
		}

		public List<ClientRaycast> clientsideInputs; // list of raycasts sent this frame
		public Queue<InputInfo> serversideInputs;

		public uint clientSimulationFrameNumber;
		public int recov;
		public ushort keys;
		public EAttackInputFlags primaryAttack;
		public EAttackInputFlags secondaryAttack;
		public float yaw;
		public float pitch;

		private static List<Collider> tempColliders = new List<Collider>();
		private bool IsBarricadeHitWithinBounds(InputInfo info, BarricadeDrop barricade)
		{
			if (barricade.IsChildOfVehicle || barricade.asset.hasClipPrefab)
			{
				return (info.point - barricade.model.position).sqrMagnitude < 256;
			}

			// Static barricades with a 1:1 prefab (no server-side version) should have a valid collider hit.
			if (info.colliderTransform == null)
			{
				return false;
			}

			tempColliders.Clear();
			info.colliderTransform.GetComponents(tempColliders);
			foreach (Collider testCollider in tempColliders)
			{
				Bounds worldBounds = testCollider.bounds;
				worldBounds.Expand(0.1f);
				if (worldBounds.Contains(info.point))
				{
					return true;
				}
			}

			return false;
		}

		public virtual void read(SteamChannel channel, NetPakReader reader)
		{
			reader.ReadUInt32(out clientSimulationFrameNumber);
			reader.ReadInt32(out recov);
			reader.ReadUInt16(out keys);

			uint primaryAttackBits;
			reader.ReadBits(2, out primaryAttackBits);
			if ((primaryAttackBits & (uint) EAttackInputFlags.Start) == (uint) EAttackInputFlags.Start)
			{
				primaryAttack |= EAttackInputFlags.Start;
			}
			if ((primaryAttackBits & (uint) EAttackInputFlags.Stop) == (uint) EAttackInputFlags.Stop)
			{
				primaryAttack |= EAttackInputFlags.Stop;
			}

			uint secondaryAttackBits;
			reader.ReadBits(2, out secondaryAttackBits);
			if ((secondaryAttackBits & (uint) EAttackInputFlags.Start) == (uint) EAttackInputFlags.Start)
			{
				secondaryAttack |= EAttackInputFlags.Start;
			}
			if ((secondaryAttackBits & (uint) EAttackInputFlags.Stop) == (uint) EAttackInputFlags.Stop)
			{
				secondaryAttack |= EAttackInputFlags.Stop;
			}

			reader.ReadFloat(out yaw);
			reader.ReadFloat(out pitch);

			byte raycastCountByte;
			reader.ReadUInt8(out raycastCountByte);
			int raycastCount = raycastCountByte;
			if (raycastCount > 0)
			{
				raycastCount = Mathf.Min(raycastCount, MAX_CLIENTSIDE_INPUTS);
				serversideInputs = new Queue<InputInfo>(raycastCount);

				for (int raycastIndex = 0; raycastIndex < raycastCount; ++raycastIndex)
				{
					InputInfo info = new InputInfo();
					reader.ReadEnum(out info.type);

					switch (info.type)
					{
						case ERaycastInfoType.NONE:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadPhysicsMaterialName(out info.materialName);
#pragma warning disable
							info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
							break;
						case ERaycastInfoType.SKIP:
							info = null;
							break;
						case ERaycastInfoType.PLAYER:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.direction);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadEnum(out info.limb);
							CSteamID enemyID;
							reader.ReadSteamID(out enemyID);

							Player enemy = PlayerTool.getPlayer(enemyID);


							if (enemy != null)
							{
								bool isHitPositionValid = false;
								if (enemy.movement.getVehicle() != null)
								{
									float maxDist = 64.0f; // Same as vehicle.
									float maxDistMultiplier = enemy.movement.getVehicle().asset?.ValidHitDistanceMultiplier ?? 1f;
									maxDist *= maxDistMultiplier;
									float sqrMaxDist = maxDist * maxDist;

									if ((info.point - enemy.transform.position).sqrMagnitude < sqrMaxDist)
									{
										isHitPositionValid = true;
									}
								}
								else if (enemy.input.serverBoundsHistory != null
									&& enemy.input.serverBoundsHistory.ContainsPoint(enemy.movement.controller, info.point))
								{
									isHitPositionValid = true;
								}

								if (isHitPositionValid)
								{
									info.materialName = "Flesh_Dynamic";
#pragma warning disable
									info.material = EPhysicsMaterial.FLESH_DYNAMIC;
#pragma warning restore
									info.player = enemy;
									info.transform = enemy.transform;
								}
								else
								{
									info = null;
								}
							}
							else
							{
								info = null;
							}

							break;
						case ERaycastInfoType.ZOMBIE:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.direction);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadEnum(out info.limb);
							ushort zombieID;
							reader.ReadUInt16(out zombieID);

							Zombie zombie = ZombieManager.getZombie(info.point, zombieID);

							if (zombie != null)
							{
								// Only compare distance on the horizontal plane because for the most part zombies
								// are not moving vertically. Saw a video where a zombie managed to fall out of the
								// map and could not be shot because it was infinitely falling.
								Vector2 xzDelta = new Vector2(info.point.x - zombie.transform.position.x, info.point.z - zombie.transform.position.z);
								if (xzDelta.sqrMagnitude < 256)
								{
									if (zombie.isRadioactive)
									{
										info.materialName = "Alien_Dynamic";
#pragma warning disable
										info.material = EPhysicsMaterial.ALIEN_DYNAMIC;
#pragma warning restore
									}
									else
									{
										info.materialName = "Flesh_Dynamic";
#pragma warning disable
										info.material = EPhysicsMaterial.FLESH_DYNAMIC;
#pragma warning restore
									}
									info.zombie = zombie;
									info.transform = zombie.transform;
								}
								else
								{
									info = null;
								}
							}
							else
							{
								info = null;
							}

							break;
						case ERaycastInfoType.ANIMAL:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.direction);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadPhysicsMaterialName(out info.materialName);
							reader.ReadEnum(out info.limb);
							ushort animalIndex;
							reader.ReadUInt16(out animalIndex);

							Animal animal = AnimalManager.getAnimal(animalIndex);

							if (animal != null && (info.point - animal.transform.position).sqrMagnitude < 256)
							{
								if (string.IsNullOrEmpty(info.materialName))
								{
									info.materialName = "Flesh_Dynamic";
#pragma warning disable
									info.material = EPhysicsMaterial.FLESH_DYNAMIC;
#pragma warning restore
								}
								else
								{
#pragma warning disable
									info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
								}
								info.animal = animal;
								info.transform = animal.transform;
							}
							else
							{
								info = null;
							}

							break;
						case ERaycastInfoType.VEHICLE:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadPhysicsMaterialName(out info.materialName);
#pragma warning disable
							info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
							uint vehicleInstanceID;
							reader.ReadUInt32(out vehicleInstanceID);
							reader.ReadTransform(out info.colliderTransform);

							InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(vehicleInstanceID);

							// Experimented with calculating based on speed, but came back to a similar value
							// due to allowing hitting far away points on large vehicles like blimps.
							float maxDistance = 64.0f;
							float maxDistanceMultiplier = vehicle?.asset?.ValidHitDistanceMultiplier ?? 1f;
							maxDistance *= maxDistanceMultiplier;
							float sqrMaxDistance = maxDistance * maxDistance;
							if (vehicle != null && (vehicle == channel.owner.player.movement.getVehicle() || (info.point - vehicle.transform.position).sqrMagnitude < sqrMaxDistance))
							{
								info.vehicle = vehicle;
								info.transform = vehicle.transform;
							}
							else
							{
								info = null;
							}

							break;
						case ERaycastInfoType.BARRICADE:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadPhysicsMaterialName(out info.materialName);
#pragma warning disable
							info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
							NetId barricadeNetId;
							reader.ReadNetId(out barricadeNetId);
							reader.ReadTransform(out info.colliderTransform);

							BarricadeDrop barricade = NetIdRegistry.Get<BarricadeDrop>(barricadeNetId);
							if (barricade != null && barricade.asset != null && barricade.model != null)
							{
								info.transform = barricade.model;
								if (info.usage.RequiresBarricadeBoundsTest() && !IsBarricadeHitWithinBounds(info, barricade))
								{
									info = null;
								}
							}
							else
							{
								info = null;
							}

							break;
						case ERaycastInfoType.STRUCTURE:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.direction);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadPhysicsMaterialName(out info.materialName);
#pragma warning disable
							info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
							NetId structureNetId;
							reader.ReadNetId(out structureNetId);
							reader.ReadTransform(out info.colliderTransform);

							StructureDrop structure = NetIdRegistry.Get<StructureDrop>(structureNetId);
							if (structure != null)
							{
								Transform structureModel = structure.model;
								if (structureModel != null && (info.point - structureModel.position).sqrMagnitude < 256)
								{
									info.transform = structureModel;
								}
								else
								{
									info = null;
								}
							}
							else
							{
								info = null;
							}

							break;
						case ERaycastInfoType.RESOURCE:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.direction);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadPhysicsMaterialName(out info.materialName);
#pragma warning disable
							info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
							byte resource_x;
							reader.ReadUInt8(out resource_x);
							byte resource_y;
							reader.ReadUInt8(out resource_y);
							ushort resourceIndex;
							reader.ReadUInt16(out resourceIndex);
							reader.ReadTransform(out info.colliderTransform);

							Transform resource = ResourceManager.getResource(resource_x, resource_y, resourceIndex);

							if (resource != null && (info.point - resource.transform.position).sqrMagnitude < 256)
							{
								info.transform = resource;
							}
							else
							{
								info = null;
							}

							break;
						case ERaycastInfoType.OBJECT:
							reader.ReadEnum(out info.usage);
							reader.ReadClampedVector3(out info.point);
							reader.ReadNormalVector3(out info.direction);
							reader.ReadNormalVector3(out info.normal);
							reader.ReadPhysicsMaterialName(out info.materialName);
#pragma warning disable
							info.material = PhysicsTool.GetLegacyMaterialByName(info.materialName);
#pragma warning restore
							reader.ReadUInt8(out info.section);
							byte object_x;
							reader.ReadUInt8(out object_x);
							byte object_y;
							reader.ReadUInt8(out object_y);
							ushort objectIndex;
							reader.ReadUInt16(out objectIndex);
							reader.ReadTransform(out info.colliderTransform);

							LevelObject obj = ObjectManager.getObject(object_x, object_y, objectIndex);

							if (obj != null && obj.transform != null && (info.point - obj.transform.position).sqrMagnitude < 256)
							{
								info.transform = obj.transform;
							}
							else
							{
								info.type = ERaycastInfoType.NONE;
							}

							break;
					}

					if (info != null)
					{
						serversideInputs.Enqueue(info);
					}
				}
			}
		}

		public virtual void write(NetPakWriter writer)
		{
			writer.WriteUInt32(clientSimulationFrameNumber);
			writer.WriteInt32(recov);
			writer.WriteUInt16(keys);
			writer.WriteBits((uint) primaryAttack, 2);
			writer.WriteBits((uint) secondaryAttack, 2);

			writer.WriteFloat(yaw);
			writer.WriteFloat(pitch);

			if (clientsideInputs == null)
			{
				writer.WriteUInt8(0);
			}
			else
			{
				int numClientsideInputs = clientsideInputs.Count;
				if (numClientsideInputs > MAX_CLIENTSIDE_INPUTS)
				{
					UnturnedLog.warn("Discarding excessive hit inputs {0}/{1}", numClientsideInputs, MAX_CLIENTSIDE_INPUTS);
					numClientsideInputs = MAX_CLIENTSIDE_INPUTS;
				}

				writer.WriteUInt8((byte) numClientsideInputs);
				for (int clientsideInputIndex = 0; clientsideInputIndex < numClientsideInputs; ++clientsideInputIndex)
				{
					RaycastInfo info = clientsideInputs[clientsideInputIndex].info;
					ERaycastInfoUsage usage = clientsideInputs[clientsideInputIndex].usage;

					if (info.player != null)
					{
						writer.WriteEnum(ERaycastInfoType.PLAYER);
						writer.WriteEnum(usage);
						writer.WriteClampedVector3(info.point);
						writer.WriteNormalVector3(info.direction);
						writer.WriteNormalVector3(info.normal);
						writer.WriteEnum(info.limb);
						writer.WriteSteamID(info.player.channel.owner.playerID.steamID);
					}
					else if (info.zombie != null)
					{
						writer.WriteEnum(ERaycastInfoType.ZOMBIE);
						writer.WriteEnum(usage);
						writer.WriteClampedVector3(info.point);
						writer.WriteNormalVector3(info.direction);
						writer.WriteNormalVector3(info.normal);
						writer.WriteEnum(info.limb);
						writer.WriteUInt16(info.zombie.id);
					}
					else if (info.animal != null)
					{
						writer.WriteEnum(ERaycastInfoType.ANIMAL);
						writer.WriteEnum(usage);
						writer.WriteClampedVector3(info.point);
						writer.WriteNormalVector3(info.direction);
						writer.WriteNormalVector3(info.normal);
						writer.WritePhysicsMaterialName(info.materialName);
						writer.WriteEnum(info.limb);
						writer.WriteUInt16(info.animal.index);
					}
					else if (info.vehicle != null)
					{
						writer.WriteEnum(ERaycastInfoType.VEHICLE);
						writer.WriteEnum(usage);
						writer.WriteClampedVector3(info.point);
						writer.WriteNormalVector3(info.normal);
						writer.WritePhysicsMaterialName(info.materialName);
						writer.WriteUInt32(info.vehicle.instanceID);
						writer.WriteTransform(info.collider?.transform);
					}
					else if (info.transform != null)
					{
						if (info.transform.CompareTag("Barricade"))
						{
							writer.WriteEnum(ERaycastInfoType.BARRICADE);
							writer.WriteEnum(usage);

							info.transform = DamageTool.getBarricadeRootTransform(info.transform);
							BarricadeDrop barricade = BarricadeManager.FindBarricadeByRootTransform(info.transform);

							if (barricade != null)
							{
								writer.WriteClampedVector3(info.point);
								writer.WriteNormalVector3(info.normal);
								writer.WritePhysicsMaterialName(info.materialName);
								writer.WriteNetId(barricade.GetNetId());
							}
							else
							{
								writer.WriteClampedVector3(Vector3.zero);
								writer.WriteNormalVector3(Vector3.up);
								writer.WritePhysicsMaterialName(null);
								writer.WriteNetId(NetId.INVALID);
							}

							writer.WriteTransform(info.collider?.transform);
						}
						else if (info.transform.CompareTag("Structure"))
						{
							writer.WriteEnum(ERaycastInfoType.STRUCTURE);
							writer.WriteEnum(usage);

							info.transform = DamageTool.getStructureRootTransform(info.transform);
							StructureDrop structure = StructureManager.FindStructureByRootTransform(info.transform);

							if (structure != null)
							{
								writer.WriteClampedVector3(info.point);
								writer.WriteNormalVector3(info.direction);
								writer.WriteNormalVector3(info.normal);
								writer.WritePhysicsMaterialName(info.materialName);
								writer.WriteNetId(structure.GetNetId());
							}
							else
							{
								writer.WriteClampedVector3(Vector3.zero);
								writer.WriteNormalVector3(Vector3.up);
								writer.WriteNormalVector3(Vector3.up);
								writer.WritePhysicsMaterialName(null);
								writer.WriteNetId(NetId.INVALID);
							}

							writer.WriteTransform(info.collider?.transform);
						}
						else if (info.transform.CompareTag("Resource"))
						{
							writer.WriteEnum(ERaycastInfoType.RESOURCE);
							writer.WriteEnum(usage);

							byte x;
							byte y;
							ushort index;

							info.transform = DamageTool.getResourceRootTransform(info.transform);

							if (ResourceManager.tryGetRegion(info.transform, out x, out y, out index))
							{
								writer.WriteClampedVector3(info.point);
								writer.WriteNormalVector3(info.direction);
								writer.WriteNormalVector3(info.normal);
								writer.WritePhysicsMaterialName(info.materialName);
								writer.WriteUInt8(x);
								writer.WriteUInt8(y);
								writer.WriteUInt16(index);
							}
							else
							{
								writer.WriteClampedVector3(Vector3.zero);
								writer.WriteNormalVector3(Vector3.up);
								writer.WriteNormalVector3(Vector3.up);
								writer.WritePhysicsMaterialName(null);
								writer.WriteUInt8(byte.MinValue);
								writer.WriteUInt8(byte.MinValue);
								writer.WriteUInt16(ushort.MaxValue);
							}

							writer.WriteTransform(info.collider?.transform);
						}
						else if (info.transform.CompareTag("Small") || info.transform.CompareTag("Medium") || info.transform.CompareTag("Large"))
						{
							writer.WriteEnum(ERaycastInfoType.OBJECT);
							writer.WriteEnum(usage);

							byte x;
							byte y;
							ushort index;

							// Set section in-case caller code didn't already, otherwise server can't find the hit rubble.
							InteractableObjectRubble rubble = info.transform.GetComponentInParent<InteractableObjectRubble>();
							if (rubble != null)
							{
								info.transform = rubble.transform;
								info.section = rubble.getSection(info.collider.transform);
							}
							else
							{
								info.transform = info.transform.root;
							}

							if (ObjectManager.tryGetRegion(info.transform, out x, out y, out index))
							{
								writer.WriteClampedVector3(info.point);
								writer.WriteNormalVector3(info.direction);
								writer.WriteNormalVector3(info.normal);
								writer.WritePhysicsMaterialName(info.materialName);
								writer.WriteUInt8(info.section);
								writer.WriteUInt8(x);
								writer.WriteUInt8(y);
								writer.WriteUInt16(index);
							}
							else
							{
								writer.WriteClampedVector3(Vector3.zero);
								writer.WriteNormalVector3(Vector3.up);
								writer.WriteNormalVector3(Vector3.up);
								writer.WritePhysicsMaterialName(null);
								writer.WriteUInt8(byte.MaxValue);
								writer.WriteUInt8(byte.MinValue);
								writer.WriteUInt8(byte.MinValue);
								writer.WriteUInt16(ushort.MaxValue);
							}

							writer.WriteTransform(info.collider?.transform);
						}
						else if (info.transform.CompareTag("Ground") || info.transform.CompareTag("Environment"))
						{
							writer.WriteEnum(ERaycastInfoType.NONE);
							writer.WriteEnum(usage);
							writer.WriteClampedVector3(info.point);
							writer.WriteNormalVector3(info.normal);
							writer.WritePhysicsMaterialName(info.materialName);
						}
						else
						{
							writer.WriteEnum(ERaycastInfoType.SKIP);
						}
					}
					else
					{
						writer.WriteEnum(ERaycastInfoType.SKIP);
					}
				}
			}
		}
	}

	public class DrivingPlayerInputPacket : PlayerInputPacket
	{
		public Vector3 position;
		public Quaternion rotation;
		public float speed;
		public float forwardVelocity;
		public float steeringInput;
		public float velocityInput;

		internal InteractableVehicle vehicle;

		public override void read(SteamChannel channel, NetPakReader reader)
		{
			base.read(channel, reader);

			reader.ReadClampedVector3(out position, fracBitCount: VehicleManager.POSITION_FRAC_BIT_COUNT);
			reader.ReadQuaternion(out rotation, bitsPerComponent: VehicleManager.ROTATION_BIT_COUNT);
			reader.ReadUnsignedClampedFloat(VehicleManager.SPEED_INT_BIT_COUNT, VehicleManager.SPEED_FRAC_BIT_COUNT, out speed);
			reader.ReadClampedFloat(VehicleManager.FORWARD_VELOCITY_INT_BIT_COUNT, VehicleManager.FORWARD_VELOCITY_FRAC_BIT_COUNT, out forwardVelocity);
			reader.ReadSignedNormalizedFloat(VehicleManager.STEERING_BIT_COUNT, out steeringInput);
			reader.ReadClampedFloat(VehicleManager.FORWARD_VELOCITY_INT_BIT_COUNT, VehicleManager.FORWARD_VELOCITY_FRAC_BIT_COUNT, out velocityInput);

			if (vehicle != null && vehicle.asset != null)
			{
				if (vehicle.asset.replicatedWheelIndices != null)
				{
					foreach (int wheelIndex in vehicle.asset.replicatedWheelIndices)
					{
						Wheel wheel = vehicle.GetWheelAtIndex(wheelIndex);
						if (wheel == null)
						{
							UnturnedLog.error($"Missing wheel for replicated index: {wheelIndex}");
							reader.ReadUnsignedNormalizedFloat(4, out float _); // Read to avoid messing up offsets.
							reader.ReadPhysicsMaterialNetId(out PhysicsMaterialNetId _);
							continue;
						}

						if (reader.ReadUnsignedNormalizedFloat(4, out float state))
						{
							wheel.replicatedSuspensionState = state;
						}

						reader.ReadPhysicsMaterialNetId(out wheel.replicatedGroundMaterial);
					}
				}

				if (vehicle.asset.UsesEngineRpmAndGears)
				{
					reader.ReadBits(VehicleManager.GEAR_BIT_COUNT, out uint packedGear);
					int replicatedGear = ((int) packedGear) - 1;
					replicatedGear = Mathf.Clamp(replicatedGear, -1, vehicle.asset.forwardGearRatios.Length);
					vehicle.ChangeGears(replicatedGear);

					reader.ReadUnsignedNormalizedFloat(VehicleManager.ENGINE_RPM_BIT_COUNT, out float normalizedRpm);
					vehicle.ReplicatedEngineRpm = Mathf.Lerp(vehicle.asset.EngineIdleRpm, vehicle.asset.EngineMaxRpm, normalizedRpm);
				}
			}
		}

		public override void write(NetPakWriter writer)
		{
			base.write(writer);

			writer.WriteClampedVector3(position, fracBitCount: VehicleManager.POSITION_FRAC_BIT_COUNT);
			writer.WriteQuaternion(rotation, bitsPerComponent: VehicleManager.ROTATION_BIT_COUNT);
			writer.WriteUnsignedClampedFloat(speed, VehicleManager.SPEED_INT_BIT_COUNT, VehicleManager.SPEED_FRAC_BIT_COUNT);
			writer.WriteClampedFloat(forwardVelocity, VehicleManager.FORWARD_VELOCITY_INT_BIT_COUNT, VehicleManager.FORWARD_VELOCITY_FRAC_BIT_COUNT);
			writer.WriteSignedNormalizedFloat(steeringInput, VehicleManager.STEERING_BIT_COUNT);
			writer.WriteClampedFloat(velocityInput, VehicleManager.FORWARD_VELOCITY_INT_BIT_COUNT, VehicleManager.FORWARD_VELOCITY_FRAC_BIT_COUNT);

			if (vehicle != null && vehicle.asset != null)
			{
				if (vehicle.asset.replicatedWheelIndices != null)
				{
					foreach (int wheelIndex in vehicle.asset.replicatedWheelIndices)
					{
						Wheel wheel = vehicle.GetWheelAtIndex(wheelIndex);
						if (wheel == null)
						{
							UnturnedLog.error($"Missing wheel for replicated index: {wheelIndex}");
							writer.WriteUnsignedNormalizedFloat(0.0f, 4); // Write something to avoid messing up offsets.
							writer.WritePhysicsMaterialNetId(PhysicsMaterialNetId.NULL);
							continue;
						}

						writer.WriteUnsignedNormalizedFloat(wheel.replicatedSuspensionState, 4);
						writer.WritePhysicsMaterialNetId(wheel.replicatedGroundMaterial);
					}
				}

				if (vehicle.asset.UsesEngineRpmAndGears)
				{
					uint packedGear = (uint) (vehicle.GearNumber + 1);
					writer.WriteBits(packedGear, VehicleManager.GEAR_BIT_COUNT);

					float normalizedEngineRpm = Mathf.InverseLerp(vehicle.asset.EngineIdleRpm, vehicle.asset.EngineMaxRpm, vehicle.ReplicatedEngineRpm);
					writer.WriteUnsignedNormalizedFloat(normalizedEngineRpm, VehicleManager.ENGINE_RPM_BIT_COUNT);
				}
			}
		}

		public DrivingPlayerInputPacket(InteractableVehicle vehicle) : base()
		{
			this.vehicle = vehicle;
		}
	}

	internal struct ClientMovementInput
	{
		public uint frameNumber;
		public bool crouch;
		public bool prone;
		public bool sprint;
		public int input_x;
		public int input_y;
		public bool jump;

		// World space rotations to temporarily apply because movement uses them for forward vectors.
		public Quaternion rotation;
		public Quaternion aimRotation;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		public Vector3 debugPosition;
		public Vector3 debugVelocity;
		public bool debugIsGrounded;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
	}

	[NetEnum]
	public enum ERaycastInfoType
	{
		NONE,
		SKIP,
		OBJECT,
		PLAYER,
		ZOMBIE,
		ANIMAL,
		VEHICLE,
		BARRICADE,
		STRUCTURE,
		RESOURCE
	}

	/// <summary>
	/// Tags how client expects server to use a raycast input.
	/// For example, client may think they fired a gun while server thinks they dequipped the gun,
	/// so tagging the input prevents the server from handling it as a punch instead.
	/// </summary>
	[NetEnum]
	public enum ERaycastInfoUsage
	{
		Punch, // PlayerEquipment melee
		ConsumeableAid, // UseableConsumeable aid another player
		Melee, // UseableMelee
		Gun, // UseableGun
		Bayonet, // Bayonet attachment on gun
		Refill, // UseableRefill
		Tire, // UseableTire
		Battery, // UseableVehicleBattery
		Detonator, // UseableDetonator
		Carlockpick, // UseableCarlockpick
		Fuel, // UseableFuel
		Carjack, // UseableCarjack
		Grower, // UseableGrower
		ArrestStart, // UseableArrestStart
		ArrestEnd, // UseableArrestEnd
		Paint, // UseableVehiclePaint
	}

	public static class ERaycastInfoUsageEx
	{
		/// <summary>
		/// Nelson 2025-09-09: changing to only require bounds test when usage will
		/// apply damage because Detonator usage provides the barricade root transform
		/// which may not have a collider. (Public issue #5202.)
		/// </summary>
		public static bool RequiresBarricadeBoundsTest(this ERaycastInfoUsage usage)
		{
			switch (usage)
			{
				case ERaycastInfoUsage.Punch:
				case ERaycastInfoUsage.Melee:
				case ERaycastInfoUsage.Gun:
				case ERaycastInfoUsage.Bayonet:
					return true;

				default:
					return false;
			}
		}
	}

	public class WalkingPlayerInputPacket : PlayerInputPacket
	{
		public byte analog;

		/// <summary>
		/// Resulting transform.position immediately after movement.simulate was called.
		/// </summary>
		public Vector3 clientPosition;

		public override void read(SteamChannel channel, NetPakReader reader)
		{
			base.read(channel, reader);

			reader.ReadUInt8(out analog);
			reader.ReadClampedVector3(out clientPosition);
		}

		public override void write(NetPakWriter writer)
		{
			base.write(writer);

			writer.WriteUInt8(analog);
			writer.WriteClampedVector3(clientPosition);
		}
	}

	public class PlayerInput : PlayerCaller
	{
		public static readonly uint SAMPLES = 4;
		public static readonly float RATE = 0.08f;

		/// <summary>
		/// Calls to UseableGun.tock per second.
		/// </summary>
		public static readonly uint TOCK_PER_SECOND = 50; // (1 / RATE) * SAMPLES

		private const int VANILLA_DIGITAL_KEYS = 10;

		/// <summary>
		/// Called for every input packet received allowing plugins to listen for a few special
		/// keys they can display in chat/effect UIs.
		/// </summary>
		public static PluginKeyTickHandler onPluginKeyTick;

		private float _tick;
		public float tick => _tick;

		private uint buffer;
		private uint consumed;

		private uint count;

		private uint _simulation;
		public uint simulation => _simulation;

		/// <summary>
		/// Whether client is currently penalized for potentially using a lag switch. False positives are relatively
		/// likely when client framerate hitches (e.g. loading dense region), so we only modify their stats (e.g. reduce
		/// player damage) for a corresponding duration.
		/// </summary>
		public bool IsUnderFakeLagPenalty => fakeLagPenaltyFrames > 0;

		private uint _clock;
		public uint clock => _clock;

		public bool IsPluginKeyHeld(int index)
		{
			return keys[keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + index];
		}

		public bool[] keys
		{
			get;
			protected set;
		}

		private EAttackInputFlags pendingPrimaryAttackInput;
		private EAttackInputFlags pendingSecondaryAttackInput;

		private ushort[] flags;

		private Queue<InputInfo> inputs;

		/// <summary>
		/// Server tracks history of this player's bounding box to assist with validating hits.
		/// Some padding is added to reduce false positives sliding against walls (substep) and
		/// player movement inside vehicles.
		/// </summary>
		internal BoundsHistory serverBoundsHistory;

		public bool hasInputs()
		{
			return inputs != null && inputs.Count > 0;
		}

		public int getInputCount()
		{
			if (inputs == null)
			{
				return 0;
			}

			return inputs.Count;
		}


		public InputInfo getInput(bool doOcclusionCheck, ERaycastInfoUsage usage)
		{
			return getInput(doOcclusionCheck, usage, null);
		}

		/// <summary>
		/// Get the hit result of a raycast on the server. Until a generic way to address net objects is implemented
		/// this is how legacy features specify which player/animal/zombie/vehicle/etc they want to interact with.
		/// </summary>
		public InputInfo getInput(bool doOcclusionCheck, ERaycastInfoUsage usage, Vector3? rayOriginOverride)
		{
			if (inputs == null)
			{
				return null;
			}

			while (inputs.Count > 0)
			{
				InputInfo inputInfo = inputs.Dequeue();
				if (inputInfo == null)
				{
					return null;
				}

				if (inputInfo.usage != usage)
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					CommandWindow.LogWarningFormat("Input discarded because client usage {0} did not match server usage {1}", inputInfo.usage, usage);
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					continue;
				}

				if (doOcclusionCheck)
				{
					// Called when obstruction.transform is not null, so compare against inputInfo.transform 
					bool IsObstructionHitValid()
					{
						if (inputInfo.transform == null)
						{
							// Null is sent and allowed if ground or road (see sendRaycast). Kind of hacky.
							return obstruction.transform.CompareTag("Ground") || obstruction.transform.CompareTag("Environment");
						}
						else
						{
							// Input hit something specific like a vehicle, so server hit IS valid if related.
							return obstruction.transform.IsChildOf(inputInfo.transform);
						}
					}

					if (inputInfo != null)
					{
						Vector3 rayOrigin = rayOriginOverride.HasValue ? rayOriginOverride.Value : player.look.aim.position;

						Vector3 diff = inputInfo.point - rayOrigin;
						float dist = diff.magnitude;
						Vector3 norm = diff / dist;

						if (dist > 0.025f)
						{
							Physics.Raycast(new Ray(rayOrigin, norm), out obstruction, dist - 0.025f, RayMasks.DAMAGE_SERVER);

							if (obstruction.transform != null && !IsObstructionHitValid())
							{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
								CommandWindow.LogWarningFormat("Input discarded because forward ray hit {0}", obstruction.ToDebugString());
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
								return null;
							}
							else
							{
								Physics.Raycast(new Ray(rayOrigin + (norm * (dist - 0.025f)), -norm), out obstruction, dist - 0.025f, RayMasks.DAMAGE_SERVER);

								if (obstruction.transform != null && !IsObstructionHitValid())
								{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
									CommandWindow.LogWarningFormat("Input discarded because backward ray hit {0}", obstruction.ToDebugString());
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
									return null;
								}
							}
						}
					}
				}

				return inputInfo;
			}

			return null;
		}

		[System.Obsolete("Use the overload of getInput that takes an expected usage parameter instead.")]
		public InputInfo getInput(bool doOcclusionCheck)
		{
			return null;
		}

		private PlayerInputPacket clientPendingInput;
		private List<ClientMovementInput> clientInputHistory;
		private Queue<PlayerInputPacket> serversidePackets;

		/// <summary>
		/// Ideally simulation frame number would be signed, but there is a lot of code expecting unsigned.
		/// </summary>
		private uint serverLastReceivedSimulationFrameNumber = uint.MaxValue;

		public int recov;
		private RaycastHit obstruction;

		private float lastInputed;
		private bool hasInputed;
		private bool isDismissed;

#if UNITY_EDITOR
		/// <summary>
		/// Can be enabled in the unity inspector to test that discarding inputs recovers properly.
		/// </summary>
		public bool debugDiscardInputs;
#endif // UNITY_EDITOR

		public bool isRaycastInvalid(RaycastInfo info)
		{
			return info.player == null && info.zombie == null && info.animal == null && info.vehicle == null && info.transform == null;
		}

		public void sendRaycast(RaycastInfo info, ERaycastInfoUsage usage)
		{
			if (isRaycastInvalid(info))
			{
				return;
			}

			if (Provider.isServer)
			{
				InputInfo input = new InputInfo();
				input.usage = usage;
				input.animal = info.animal;
				input.direction = info.direction;
				input.limb = info.limb;
				input.materialName = info.materialName;
#pragma warning disable
				input.material = info.material;
#pragma warning restore
				input.normal = info.normal;
				input.player = info.player;
				input.point = info.point;
				input.transform = info.transform;
				input.colliderTransform = info.collider?.transform;
				input.vehicle = info.vehicle;
				input.zombie = info.zombie;
				input.section = info.section;

				if (input.player != null)
				{
					input.type = ERaycastInfoType.PLAYER;
				}
				else if (input.zombie != null)
				{
					input.type = ERaycastInfoType.ZOMBIE;
				}
				else if (input.animal != null)
				{
					input.type = ERaycastInfoType.ANIMAL;
				}
				else if (input.vehicle != null)
				{
					input.type = ERaycastInfoType.VEHICLE;
				}
				else if (input.transform != null)
				{
					if (input.transform.CompareTag("Barricade"))
					{
						input.type = ERaycastInfoType.BARRICADE;
					}
					else if (info.transform.CompareTag("Structure"))
					{
						input.type = ERaycastInfoType.STRUCTURE;
					}
					else if (info.transform.CompareTag("Resource"))
					{
						input.type = ERaycastInfoType.RESOURCE;
					}
					else if (input.transform.CompareTag("Small") || input.transform.CompareTag("Medium") || input.transform.CompareTag("Large"))
					{
						input.type = ERaycastInfoType.OBJECT;
					}
					else if (info.transform.CompareTag("Ground") || info.transform.CompareTag("Environment"))
					{
						input.type = ERaycastInfoType.NONE;
					}
					else
					{
						input = null;
					}
				}
				else
				{
					input = null;
				}

				if (input != null)
				{
					inputs.Enqueue(input);
				}
			}
			else
			{
				if (clientPendingInput.clientsideInputs == null) // no need for GC if no raycasts
				{
					clientPendingInput.clientsideInputs = new List<PlayerInputPacket.ClientRaycast>();
				}

				clientPendingInput.clientsideInputs.Add(new PlayerInputPacket.ClientRaycast(info, usage));
			}
		}

		/// <summary>
		/// askInput is always called the same number of times per second because it's run from FixedUpdate,
		/// but the spacing between calls can vary depending on network and whether client FPS is low.
		/// </summary>
		private static readonly float EXPECTED_ASKINPUT_PER_SECOND = 1 / RATE; // 12.5

		/// <summary>
		/// If average askInput calls per second exceeds this, we either ignore their request or flat-out kick them.
		/// </summary>
		private static readonly int MAX_ASKINPUT_PER_SECOND = (int) (EXPECTED_ASKINPUT_PER_SECOND + 3); // 15

		/// <summary>
		/// If average askInput calls per second exceeds this we silently kick them.
		/// </summary>
		private static readonly int KICK_ASKINPUT_PER_SECOND = (int) (EXPECTED_ASKINPUT_PER_SECOND * 5);

		/// <summary>
		/// Number of times askInput has been called by client.
		/// Even with huge packet loss, we know that 
		/// </summary>
		private int serversideAskInputCount = 0;

		/// <summary>
		/// Realtime that the first call to askInput was made by the client.
		/// </summary>
		private float initialServersideAskInputTime = -1;

		/// <summary>
		/// Realtime that the previous askInput kick test was performed.
		/// </summary>
		private float latestAskInputDismissTestTime = -1;

		private static readonly int ASKINPUT_WINDOW_LENGTH = 10;
		private int[] rollingWindow = new int[ASKINPUT_WINDOW_LENGTH];
		private int rollingWindowIndex = 0;

		/// <summary>
		/// Set rollingWindowIndex to newIndex, zeroing all input counts along the way.
		/// Important to zero the intermediary indexes in-case server stalled for more than one second.
		/// </summary>
		private void advanceRollingWindowIndex(int newIndex)
		{
			// Note that this is only called when newIndex has changed.
			do
			{
				// Increment first so that we don't zero the previous frame.
				++rollingWindowIndex;
				if (rollingWindowIndex >= rollingWindow.Length)
					rollingWindowIndex = 0;

				rollingWindow[rollingWindowIndex] = 0;
			}
			while (rollingWindowIndex != newIndex);
		}

		private void internalDismiss()
		{
			isDismissed = true;
			Provider.dismiss(channel.owner.playerID.steamID);
		}

		private bool clientHasPendingResimulation;
		private uint clientResimulationFrameNumber;
		private EPlayerStance clientResimulationStance;
		private Vector3 clientResimulationPosition;
		private Vector3 clientResimulationVelocity;
		private byte clientResimulationStamina;
		private int clientResimulationLastTireOffset;
		private int clientResimulationLastRestOffset;

		private void ClientRemoveInputHistory(uint frameNumber)
		{
			if (clientInputHistory.IsEmpty())
				return;

			uint oldestFrameNumber = clientInputHistory[0].frameNumber;
			if (oldestFrameNumber <= frameNumber)
			{
				// Originally this removed depending on the diff (frameNumber - oldestFrameNumber), but that would be
				// incorrect if input history was paused while in a vehicle, dead, or something else.
				int removeCount;
				for (removeCount = 1; removeCount < clientInputHistory.Count; ++removeCount)
				{
					if (clientInputHistory[removeCount].frameNumber > frameNumber)
						break;
				}

#if LOG_INPUT_RESIMULATION
				UnturnedLog.info($"Removing {removeCount} frame(s) from input history");
#endif // LOG_INPUT_RESIMULATION

				clientInputHistory.RemoveRange(0, removeCount);
			}
		}

		internal bool isResimulating;

		private void ClientResimulate()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			for (int index = 0; index < clientInputHistory.Count; ++index)
			{
				if (clientInputHistory[index].frameNumber == clientResimulationFrameNumber)
				{
					UnturnedLog.info($"Resimulating frame number {clientResimulationFrameNumber}");
					UnturnedLog.info($"Client position: {clientInputHistory[index].debugPosition} Server position: {clientResimulationPosition}");
					UnturnedLog.info($"Client velocity: {clientInputHistory[index].debugVelocity} Server velocity: {clientResimulationVelocity}");
					UnturnedLog.info($"Client grounded: {clientInputHistory[index].debugIsGrounded}");
					break;
				}
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			ClientRemoveInputHistory(clientResimulationFrameNumber);

			if (player.movement.getVehicle() != null)
			{
				// Don't resimulate walking movement after entering vehicle.
				return;
			}

			if (player.movement.hasPendingVehicleChange)
			{
				// Resimulating would do the vehicle change early and cause bad side effects.
				return;
			}

			if (!player.movement.controller.enabled)
			{
				// Currently dead.
				return;
			}

#if LOG_INPUT_RESIMULATION
			Vector3 oldPosition = transform.position;
#endif // LOG_INPUT_RESIMULATION

			isResimulating = true;

			player.stance.internalSetStance(clientResimulationStance);

			// Client updates rotation every frame rather than during simulation, so we just temporarily restore rotations.
			Quaternion clientRotation = transform.rotation;
			Quaternion clientAimRotation = player.look.aim.rotation;

			// Must disable and re-enable CharacterController component, otherwise it will use the old position.
			player.movement.controller.enabled = false;
			transform.position = clientResimulationPosition;
			player.movement.controller.enabled = true;

			player.movement.velocity = clientResimulationVelocity;

			player.life.internalSetStamina(clientResimulationStamina);
			player.life.lastTire = MathfEx.ClampLongToUInt(clientResimulationFrameNumber - clientResimulationLastTireOffset);
			player.life.lastRest = MathfEx.ClampLongToUInt(clientResimulationFrameNumber - clientResimulationLastRestOffset);

			foreach (ClientMovementInput input in clientInputHistory)
			{
				transform.rotation = input.rotation;
				player.look.aim.rotation = input.aimRotation;

				player.life.SimulateStaminaFrame(input.frameNumber);
				player.stance.simulate(input.frameNumber, input.crouch, input.prone, input.sprint);
				player.movement.simulate(input.frameNumber, 0, input.input_x, input.input_y, 0.0f, 0.0f, input.jump, false, RATE);
			}

			transform.rotation = clientRotation;
			player.look.aim.rotation = clientAimRotation;

			isResimulating = false;

#if LOG_INPUT_RESIMULATION
			UnturnedLog.info($@"Simulated {clientInputHistory.Count} frame(s) of input history
Position delta: {(transform.position - oldPosition).magnitude}");
#endif // LOG_INPUT_RESIMULATION
		}

		private static readonly ClientInstanceMethod<uint, EPlayerStance, Vector3, Vector3, byte, int, int> SendSimulateMispredictedInputs = ClientInstanceMethod<uint, EPlayerStance, Vector3, Vector3, byte, int, int>.Get(typeof(PlayerInput), nameof(ReceiveSimulateMispredictedInputs));
		/// <summary>
		/// Notify client there has been a prediction error, so movement needs to be re-simulated.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveSimulateMispredictedInputs(uint frameNumber, EPlayerStance stance, Vector3 position, Vector3 velocity, byte stamina, int lastTireOffset, int lastRestOffset)
		{
			clientHasPendingResimulation = true;
			clientResimulationFrameNumber = frameNumber;
			clientResimulationStance = stance;
			clientResimulationPosition = position;
			clientResimulationVelocity = velocity;
			clientResimulationStamina = stamina;
			clientResimulationLastTireOffset = lastTireOffset;
			clientResimulationLastRestOffset = lastRestOffset;
		}

		private static readonly ClientInstanceMethod<uint> SendAckGoodInputs = ClientInstanceMethod<uint>.Get(typeof(PlayerInput), nameof(ReceiveAckGoodInputs));
		/// <summary>
		/// Notify client old inputs can be discarded because they were predicted correctly.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveAckGoodInputs(uint frameNumber)
		{
			if (clientHasPendingResimulation)
			{
				if (frameNumber > clientResimulationFrameNumber)
				{
					// Good news? Somehow we were correct in the future!
					clientHasPendingResimulation = false;
				}
				else
				{
					return;
				}
			}

			ClientRemoveInputHistory(frameNumber);
		}

		private static readonly ServerInstanceMethod SendInputs = ServerInstanceMethod.Get(typeof(PlayerInput), nameof(ReceiveInputs));
		[System.Obsolete]
		public void askInput(CSteamID steamID)
		{ }

		/// <summary>
		/// Not using rate limit attribute because it internally keeps a rolling window limit.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER)]
		public void ReceiveInputs(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;

			if (isDismissed)
			{
				// In the process of kicking from server, likely for cheat spamming this function.
				context.LogWarning("ignoring because isDismissed");
				return;
			}

			if (serversideAskInputCount == 0)
			{
				initialServersideAskInputTime = Time.realtimeSinceStartup;
			}
			++serversideAskInputCount;

			float timeSinceInitialCall = Time.realtimeSinceStartup - initialServersideAskInputTime;
			int newWindowIndex = ((int) timeSinceInitialCall) % ASKINPUT_WINDOW_LENGTH;
			if (newWindowIndex != rollingWindowIndex)
			{
				// Advance prior to kick test in case server stalled for a long time.
				advanceRollingWindowIndex(newWindowIndex);

				// Rolling window helps protect against initial finickyness, e.g. logging suggests
				// it might be ~18 in first bucket, but since we ignore extra ones we should be safe
				// to wait a few seconds to give the client time to stabilize just in case.
				if (Provider.configData.Server.Enable_Kick_Input_Spam && timeSinceInitialCall > ASKINPUT_WINDOW_LENGTH)
				{
					// This seems like a rare case, but hosts are reporting unusual kicks,
					// so we protect here against the case where server stalled for >5 seconds.
					float timeSinceTest = Time.realtimeSinceStartup - latestAskInputDismissTestTime;
					if (timeSinceTest < ASKINPUT_WINDOW_LENGTH / 2)
					{
						int totalRecentCalls = 0;
						foreach (int numCalls in rollingWindow)
							totalRecentCalls += numCalls;
						float averageRecentCallsPerSecond = totalRecentCalls / ASKINPUT_WINDOW_LENGTH;
						if (averageRecentCallsPerSecond > KICK_ASKINPUT_PER_SECOND)
						{
							string percStr = Mathf.RoundToInt(averageRecentCallsPerSecond / EXPECTED_ASKINPUT_PER_SECOND * 100).ToString();
							UnturnedLog.warn("Received {0}% of expected input packets from {1} over the past {2} seconds, so we're dismissing them", percStr, channel.owner.playerID.steamID, ASKINPUT_WINDOW_LENGTH);
#if LOG_ASK_INPUT
							UnturnedLog.warn("Average recent inputs per second {0} exceeded limit of {1}", averageRecentCallsPerSecond, KICK_ASKINPUT_PER_SECOND);
#endif
							internalDismiss();
							return;
						}
					}

					latestAskInputDismissTestTime = Time.realtimeSinceStartup;
				}
			}

			++rollingWindow[rollingWindowIndex];
			if (rollingWindow[rollingWindowIndex] > MAX_ASKINPUT_PER_SECOND)
			{
#if LOG_ASK_INPUT
				UnturnedLog.info("Received {0}/{1} inputs this second", rollingWindow[rollingWindowIndex], EXPECTED_ASKINPUT_PER_SECOND);
#endif
				// We've tracked that that called askInput, but we don't proceed with processing it because they might be spamming.
				context.LogWarning($"ignoring because limit exceeded ({rollingWindow[rollingWindowIndex]} of {MAX_ASKINPUT_PER_SECOND})");
				return;
			}

			bool packetType;
			reader.ReadBit(out packetType);
			PlayerInputPacket packet = null;

			if (packetType)
			{
				packet = new DrivingPlayerInputPacket(player.movement.getVehicle());
			}
			else
			{
				packet = new WalkingPlayerInputPacket();
			}

			packet.read(channel, reader);

			if (serverLastReceivedSimulationFrameNumber != uint.MaxValue && packet.clientSimulationFrameNumber <= serverLastReceivedSimulationFrameNumber)
			{
				context.LogWarning($"out of order (client: {packet.clientSimulationFrameNumber} server: {serverLastReceivedSimulationFrameNumber})");
				return;
			}
			else
			{
				serverLastReceivedSimulationFrameNumber = packet.clientSimulationFrameNumber;
			}

			serversidePackets.Enqueue(packet);

			float timeSinceLastInput = Time.realtimeSinceStartup - lastInputed;
			if (hasInputed && timeSinceLastInput > MIN_FAKE_LAG_THRESHOLD_SECONDS && timeSinceLastInput > Provider.configData.Server.Fake_Lag_Threshold_Seconds)
			{
				if (Provider.configData.Server.Fake_Lag_Log_Warnings)
				{
					CommandWindow.LogWarning($"{timeSinceLastInput} seconds between inputs from \"{channel.owner.playerID.playerName}\" steamid: {channel.owner.playerID.steamID}");
				}

				float penaltyDuration = timeSinceLastInput - MIN_FAKE_LAG_THRESHOLD_SECONDS;
				fakeLagPenaltyFrames += Mathf.CeilToInt(penaltyDuration / RATE);
			}

			lastInputed = Time.realtimeSinceStartup;
			hasInputed = true;
		}

		/// <summary>
		/// Only bound on dedicated server.
		/// When dieing in a vehicle this prevents delay handling packets.
		/// </summary>
		private void onLifeUpdated(bool isDead)
		{
			serversidePackets.Clear();
		}

#if UNITY_EDITOR
		private void Update()
		{
// 			if (serverBoundsHistory != null)
// 			{
// 				serverBoundsHistory.DrawGizmos();
// 			}
		}
#endif // UNITY_EDITOR

		private void FixedUpdate()
		{
			if (isDismissed)
			{
				return;
			}

			if (Provider.isServer)
			{
				// Nelson 2025-06-17: adding here rather than the end is admittedly unintuitive,
				// but the body of this function has some early returns on the server and I don't
				// want to create hassle for plugin devs injecting code. The most up-to-date
				// character capsule is always included in hit validation regardless.
				serverBoundsHistory.AddCharacterControllerBounds(player.movement.controller);
			}

			if (channel.IsLocalPlayer)
			{
				if (count % SAMPLES == 0)
				{
					_tick = Time.realtimeSinceStartup;

					if (clientHasPendingResimulation)
					{
						clientHasPendingResimulation = false;
						ClientResimulate();
					}

					keys[0] = player.movement.jump;
					keys[1] = false; // Was primary attack, now available for something else.
					keys[2] = false; // Was secondary attack, now available for something else.
					keys[3] = player.stance.crouch;
					keys[4] = player.stance.prone;
					keys[5] = player.stance.sprint;
					keys[6] = player.animator.leanLeft;
					keys[7] = player.animator.leanRight;
					keys[8] = false; // Gun tactical input.
					keys[9] = player.stance.localWantsToSteadyAim;

					// Ignore input while rebinding a key.
					bool enablePluginKeys = !MenuConfigurationControlsUI.ShouldGameIgnoreInput;
					for (int pluginKeyIndex = 0; pluginKeyIndex < ControlsSettings.NUM_PLUGIN_KEYS; pluginKeyIndex++)
					{
						int packetKeyIndex = keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + pluginKeyIndex;
						keys[packetKeyIndex] = enablePluginKeys && InputEx.GetKey(ControlsSettings.getPluginKeyCode(pluginKeyIndex));
					}

					player.equipment.CaptureAttackInputs(out pendingPrimaryAttackInput, out pendingSecondaryAttackInput);

					UnityEngine.Profiling.Profiler.BeginSample("Life");
					player.life.simulate(simulation);
					UnityEngine.Profiling.Profiler.EndSample();

					bool inputCrouch = player.stance.crouch;
					bool inputProne = player.stance.prone;
					bool inputSprint = player.stance.sprint;

					UnityEngine.Profiling.Profiler.BeginSample("Stance");
					player.stance.simulate(simulation, inputCrouch, inputProne, inputSprint);
					UnityEngine.Profiling.Profiler.EndSample();

					int input_x = player.movement.horizontal - 1;
					int input_y = player.movement.vertical - 1;
					bool inputJump = player.movement.jump;

					UnityEngine.Profiling.Profiler.BeginSample("Move");
					player.movement.simulate(simulation, 0, input_x, input_y, player.look.look_x, player.look.look_y, inputJump, inputSprint, PlayerInput.RATE);
					UnityEngine.Profiling.Profiler.EndSample();

					if (Provider.isServer)
					{
						inputs.Clear();
					}
					else
					{
						if (player.stance.stance == EPlayerStance.DRIVING)
						{
							clientPendingInput = new DrivingPlayerInputPacket(player.movement.getVehicle());
						}
						else
						{
							WalkingPlayerInputPacket clientPendingWalkingInput = new WalkingPlayerInputPacket();
							clientPendingWalkingInput.analog = (byte) ((player.movement.horizontal << 4) | player.movement.vertical);
							clientPendingWalkingInput.clientPosition = transform.position;

							clientPendingInput = clientPendingWalkingInput;

							ClientMovementInput historyInput = new ClientMovementInput();
							historyInput.frameNumber = simulation;
							historyInput.crouch = inputCrouch;
							historyInput.prone = inputProne;
							historyInput.input_x = input_x;
							historyInput.input_y = input_y;
							historyInput.jump = inputJump;
							historyInput.sprint = inputSprint;
							historyInput.rotation = transform.rotation;
							historyInput.aimRotation = player.look.aim.rotation;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
							historyInput.debugPosition = transform.position;
							historyInput.debugVelocity = player.movement.velocity;
							historyInput.debugIsGrounded = player.movement.isGrounded;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
							clientInputHistory.Add(historyInput);
						}

						clientPendingInput.clientSimulationFrameNumber = simulation;
						clientPendingInput.recov = recov;
						clientPendingInput.pitch = player.look.pitch;
						clientPendingInput.yaw = player.look.yaw;
					}

					UnityEngine.Profiling.Profiler.BeginSample("Equipment");
					player.equipment.simulate(simulation, pendingPrimaryAttackInput, pendingSecondaryAttackInput, player.stance.localWantsToSteadyAim);
					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("Animator");
					player.animator.simulate(simulation, player.animator.leanLeft, player.animator.leanRight);
					UnityEngine.Profiling.Profiler.EndSample();

					buffer += SAMPLES;

					_simulation++;
				}

				if (consumed < buffer)
				{
					consumed++;

					player.equipment.tock(clock);

					_clock++;
				}

				if (consumed == buffer)
				{
					if (clientPendingInput != null)
					{
						if (!Provider.isServer)
						{
							ushort keyFlags = 0;
							for (byte index = 0; index < keys.Length; index++)
							{
								if (keys[index])
								{
									keyFlags = (ushort) (keyFlags | flags[index]);
								}
							}

							clientPendingInput.keys = keyFlags;
							clientPendingInput.primaryAttack = pendingPrimaryAttackInput;
							clientPendingInput.secondaryAttack = pendingSecondaryAttackInput;

							if (clientPendingInput is DrivingPlayerInputPacket)
							{
								DrivingPlayerInputPacket drivingPacket = clientPendingInput as DrivingPlayerInputPacket;
								InteractableVehicle drivenVehicle = player.movement.getVehicle();

								if (drivenVehicle != null)
								{
									drivingPacket.vehicle = drivenVehicle;

									Transform vehicleTransform = drivenVehicle.transform;

									if (drivenVehicle.asset.engine == EEngine.TRAIN)
									{
										drivingPacket.position = InteractableVehicle.PackRoadPosition(drivenVehicle.roadPosition);
									}
									else
									{
										drivingPacket.position = vehicleTransform.position;
									}
									drivingPacket.rotation = vehicleTransform.rotation;
									drivingPacket.speed = drivenVehicle.ReplicatedSpeed;
									drivingPacket.forwardVelocity = drivenVehicle.ReplicatedForwardVelocity;
									drivingPacket.steeringInput = drivenVehicle.ReplicatedSteeringInput;
									drivingPacket.velocityInput = drivenVehicle.ReplicatedVelocityInput;
								}
							}

#if UNITY_EDITOR
							// Hacked-together lag switch test.
							bool shouldSend = !InputEx.GetKey(KeyCode.LeftArrow);
#else // !UNITY_EDITOR
							bool shouldSend = true;
#endif // !UNITY_EDITOR
							shouldSend &= Provider.isConnected;

							if (shouldSend)
							{
								SendInputs.Invoke(GetNetId(), NetTransport.ENetReliability.Reliable, SendInputs_Write);
							}
						}
					}
				}

				count++;
			}
			else if (Provider.isServer)
			{
				if (serversidePackets.Count > 0)
				{
					PlayerInputPacket packet = serversidePackets.Peek(); // take a look at the next packet so we know whether to delay %SAMPLES

					if (packet is WalkingPlayerInputPacket || count % SAMPLES == 0)
					{
						if (simulation > (uint) ((Time.realtimeSinceStartup + 5.0f - tick) / RATE))
						{
							return;
						}

						packet = serversidePackets.Dequeue();

						if (packet == null)
						{
							return;
						}

						inputs = packet.serversideInputs;

						for (byte index = 0; index < keys.Length; index++)
						{
							keys[index] = (packet.keys & flags[index]) == flags[index];
						}
						pendingPrimaryAttackInput = packet.primaryAttack;
						pendingSecondaryAttackInput = packet.secondaryAttack;

						if (packet is DrivingPlayerInputPacket) // driving
						{
							DrivingPlayerInputPacket drivingPacket = packet as DrivingPlayerInputPacket;

							if (player.life.IsAlive)
							{
								UnityEngine.Profiling.Profiler.BeginSample("Life");
								player.life.simulate(simulation);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Look");
								player.look.simulate(drivingPacket.yaw, drivingPacket.pitch, RATE);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Stance");
								player.stance.simulate(simulation, false, false, false);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Move");
								player.movement.simulate(simulation, drivingPacket.recov, keys[0], keys[5], drivingPacket.position, drivingPacket.rotation, drivingPacket.speed, drivingPacket.forwardVelocity, drivingPacket.steeringInput, drivingPacket.velocityInput, RATE);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Equipment");
								player.equipment.simulate(simulation, pendingPrimaryAttackInput, pendingSecondaryAttackInput, keys[9]);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Animator");
								player.animator.simulate(simulation, false, false);
								UnityEngine.Profiling.Profiler.EndSample();
							}
						}
						else // walking
						{
							WalkingPlayerInputPacket walkingPacket = packet as WalkingPlayerInputPacket;

							byte analog = walkingPacket.analog;

							if (player.life.IsAlive)
							{
								UnityEngine.Profiling.Profiler.BeginSample("Life");
								player.life.simulate(simulation);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Look");
								player.look.simulate(walkingPacket.yaw, walkingPacket.pitch, RATE);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Stance");
								player.stance.simulate(simulation, keys[3], keys[4], keys[5]);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Move");
								int moveInputX = ((analog >> 4) & 15) - 1;
								int moveInputY = ((analog) & 15) - 1;
								bool moveInputJump = keys[0];
								bool moveInputSprint = keys[5];
								player.movement.simulate(simulation, walkingPacket.recov, moveInputX, moveInputY, 0, 0, moveInputJump, moveInputSprint, PlayerInput.RATE);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Equipment");
								player.equipment.simulate(simulation, pendingPrimaryAttackInput, pendingSecondaryAttackInput, keys[9]);
								UnityEngine.Profiling.Profiler.EndSample();

								UnityEngine.Profiling.Profiler.BeginSample("Animator");
								player.animator.simulate(simulation, keys[6], keys[7]);
								UnityEngine.Profiling.Profiler.EndSample();

								// We do not need to tell client to resimulate if we entered a vehicle on this frame.
								if (!player.movement.hasPendingVehicleChange && player.stance.stance != EPlayerStance.DRIVING && player.stance.stance != EPlayerStance.SITTING)
								{
									const float errorToleranceDistance = 0.02f; // 2cm
									const float sqrErrorToleranceDistance = errorToleranceDistance * errorToleranceDistance;
									Vector3 serverPosition = transform.position;
									if ((walkingPacket.clientPosition - serverPosition).sqrMagnitude > sqrErrorToleranceDistance)
									{
#if LOG_INPUT_RESIMULATION || UNITY_EDITOR || DEVELOPMENT_BUILD
										CommandWindow.LogWarning($"Movement misprediction! frame: {walkingPacket.clientSimulationFrameNumber} client: {walkingPacket.clientPosition} server: {serverPosition} distance: {(walkingPacket.clientPosition - serverPosition).magnitude} x: {moveInputX} y: {moveInputY} velocity: {player.movement.velocity} grounded: {player.movement.isGrounded} jump: {moveInputJump}");
#endif // LOG_INPUT_RESIMULATION || UNITY_EDITOR || DEVELOPMENT_BUILD

										// Server simulation frame may differ from client, so tell client to offset these
										// values from client's simulation frame number.
										int lastTireOffset = (int) (simulation - player.life.lastTire);
										int lastRestOffset = (int) (simulation - player.life.lastRest);

										SendSimulateMispredictedInputs.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GetOwnerTransportConnection(), walkingPacket.clientSimulationFrameNumber, player.stance.stance, serverPosition, player.movement.velocity, player.life.stamina, lastTireOffset, lastRestOffset);
									}
									else
									{
										SendAckGoodInputs.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GetOwnerTransportConnection(), walkingPacket.clientSimulationFrameNumber);
									}
								}
							}
						}

						if (onPluginKeyTick != null)
						{
							// Broadcast plugin hotkey states.
							for (byte pluginKeyIndex = 0; pluginKeyIndex < ControlsSettings.NUM_PLUGIN_KEYS; pluginKeyIndex++)
							{
								int packetKeyIndex = keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + pluginKeyIndex;
								onPluginKeyTick(player, simulation, pluginKeyIndex, keys[packetKeyIndex]);
							}
						}

						buffer += SAMPLES;

						_simulation++;

						while (consumed < buffer)
						{
							consumed++;

							if (player.life.IsAlive)
							{
								player.equipment.tock(clock);
							}

							_clock++;
						}

						// AFTER potentially updating weapon.
						fakeLagPenaltyFrames = Mathf.Max(0, fakeLagPenaltyFrames - 1);
					}

					count++;
				}
				else
				{
					UnityEngine.Profiling.Profiler.BeginSample("Move");
					player.movement.simulate();
					UnityEngine.Profiling.Profiler.EndSample();

					if (hasInputed && Time.realtimeSinceStartup - lastInputed > 20 && Provider.configData.Server.Enable_Kick_Input_Timeout)
					{
						UnturnedLog.warn("Haven't received input from {0} for the past 20 seconds, so we're dismissing them", channel.owner.playerID.steamID);
						internalDismiss();
					}
				}
			}
		}

		private void SendInputs_Write(NetPakWriter writer)
		{
			if (clientPendingInput is DrivingPlayerInputPacket)
			{
				writer.WriteBit(true);
			}
			else
			{
				writer.WriteBit(false);
			}

			clientPendingInput.write(writer);
		}

		internal void InitializePlayer()
		{
			_tick = Time.realtimeSinceStartup;
			_simulation = 0;
			_clock = 0;

			if (channel.IsLocalPlayer || Provider.isServer)
			{
				keys = new bool[VANILLA_DIGITAL_KEYS + ControlsSettings.NUM_PLUGIN_KEYS];
				flags = new ushort[VANILLA_DIGITAL_KEYS + ControlsSettings.NUM_PLUGIN_KEYS];

				for (byte index = 0; index < keys.Length; index++)
				{
					flags[index] = (ushort) (1 << index);
				}
			}

			if (channel.IsLocalPlayer && Provider.isServer)
			{
				inputs = new Queue<InputInfo>();
			}

			if (Provider.isServer)
			{
				serverBoundsHistory = new BoundsHistory();
				// Expansion should be enough to account for animations outside of capsule.
				// E.g., leaning, prone.
				serverBoundsHistory.Expansion = 0.75f;
			}

			if (channel.IsLocalPlayer)
			{
				clientPendingInput = null;
				clientInputHistory = new List<ClientMovementInput>();
			}
			else if (Provider.isServer)
			{
				serversidePackets = new Queue<PlayerInputPacket>();

				player.life.onLifeUpdated += onLifeUpdated;
			}

			recov = -1;
		}

		private const float MIN_FAKE_LAG_THRESHOLD_SECONDS = 1.0f;

		/// <summary>
		/// Counter of simulation frames before fake lag penalty is disabled.
		/// </summary>
		private int fakeLagPenaltyFrames;

		/// <summary>
		/// Player damage multiplier while under penalty for fake lag. (10%)
		/// </summary>
		internal const float FAKE_LAG_PENALTY_DAMAGE = 0.1f;
	}
}
