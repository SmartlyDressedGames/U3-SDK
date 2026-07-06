#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerLife))]
	public static class PlayerLife_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveDeath), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDeath_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EDeathCause newCause;
#if LOG_INVOKE_READ_ERRORS
			bool newCause_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newCause);
#if LOG_INVOKE_READ_ERRORS
			if (!newCause_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newCause));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.ELimb newLimb;
#if LOG_INVOKE_READ_ERRORS
			bool newLimb_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newLimb);
#if LOG_INVOKE_READ_ERRORS
			if (!newLimb_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newLimb));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Steamworks.CSteamID newKiller;
#if LOG_INVOKE_READ_ERRORS
			bool newKiller_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out newKiller);
#if LOG_INVOKE_READ_ERRORS
			if (!newKiller_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newKiller));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDeath(newCause, newLimb, newKiller);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveDeath), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDeath_Write(NetPakWriter writer, SDG.Unturned.EDeathCause newCause, SDG.Unturned.ELimb newLimb, Steamworks.CSteamID newKiller)
		{
			writer.WriteEnum(newCause);
			writer.WriteEnum(newLimb);
			writer.WriteSteamID(newKiller);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveDead), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDead_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			UnityEngine.Vector3 newRagdoll;
#if LOG_INVOKE_READ_ERRORS
			bool newRagdoll_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newRagdoll);
#if LOG_INVOKE_READ_ERRORS
			if (!newRagdoll_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newRagdoll));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.ERagdollEffect newRagdollEffect;
#if LOG_INVOKE_READ_ERRORS
			bool newRagdollEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newRagdollEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!newRagdollEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newRagdollEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDead(newRagdoll, newRagdollEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveDead), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDead_Write(NetPakWriter writer, UnityEngine.Vector3 newRagdoll, SDG.Unturned.ERagdollEffect newRagdollEffect)
		{
			writer.WriteClampedVector3(newRagdoll);
			writer.WriteEnum(newRagdollEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveRevive), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRevive_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			UnityEngine.Vector3 position;
#if LOG_INVOKE_READ_ERRORS
			bool position_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out position);
#if LOG_INVOKE_READ_ERRORS
			if (!position_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(position));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte angle;
#if LOG_INVOKE_READ_ERRORS
			bool angle_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out angle);
#if LOG_INVOKE_READ_ERRORS
			if (!angle_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(angle));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRevive(position, angle);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveRevive), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRevive_Write(NetPakWriter writer, UnityEngine.Vector3 position, System.Byte angle)
		{
			writer.WriteClampedVector3(position);
			writer.WriteUInt8(angle);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveLifeStats), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLifeStats_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newHealth;
#if LOG_INVOKE_READ_ERRORS
			bool newHealth_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newHealth);
#if LOG_INVOKE_READ_ERRORS
			if (!newHealth_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newHealth));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newFood;
#if LOG_INVOKE_READ_ERRORS
			bool newFood_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newFood);
#if LOG_INVOKE_READ_ERRORS
			if (!newFood_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFood));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newWater;
#if LOG_INVOKE_READ_ERRORS
			bool newWater_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newWater);
#if LOG_INVOKE_READ_ERRORS
			if (!newWater_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newWater));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newVirus;
#if LOG_INVOKE_READ_ERRORS
			bool newVirus_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newVirus);
#if LOG_INVOKE_READ_ERRORS
			if (!newVirus_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newVirus));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newOxygen;
#if LOG_INVOKE_READ_ERRORS
			bool newOxygen_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newOxygen);
#if LOG_INVOKE_READ_ERRORS
			if (!newOxygen_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOxygen));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean newBleeding;
#if LOG_INVOKE_READ_ERRORS
			bool newBleeding_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newBleeding);
#if LOG_INVOKE_READ_ERRORS
			if (!newBleeding_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newBleeding));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean newBroken;
#if LOG_INVOKE_READ_ERRORS
			bool newBroken_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newBroken);
#if LOG_INVOKE_READ_ERRORS
			if (!newBroken_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newBroken));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveLifeStats(newHealth, newFood, newWater, newVirus, newOxygen, newBleeding, newBroken);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveLifeStats), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLifeStats_Write(NetPakWriter writer, System.Byte newHealth, System.Byte newFood, System.Byte newWater, System.Byte newVirus, System.Byte newOxygen, System.Boolean newBleeding, System.Boolean newBroken)
		{
			writer.WriteUInt8(newHealth);
			writer.WriteUInt8(newFood);
			writer.WriteUInt8(newWater);
			writer.WriteUInt8(newVirus);
			writer.WriteUInt8(newOxygen);
			writer.WriteBit(newBleeding);
			writer.WriteBit(newBroken);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveHealth), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveHealth_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newHealth;
#if LOG_INVOKE_READ_ERRORS
			bool newHealth_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newHealth);
#if LOG_INVOKE_READ_ERRORS
			if (!newHealth_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newHealth));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveHealth(newHealth);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveHealth), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveHealth_Write(NetPakWriter writer, System.Byte newHealth)
		{
			writer.WriteUInt8(newHealth);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveDamagedEvent), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDamagedEvent_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte damageAmount;
#if LOG_INVOKE_READ_ERRORS
			bool damageAmount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out damageAmount);
#if LOG_INVOKE_READ_ERRORS
			if (!damageAmount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(damageAmount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 damageDirection;
#if LOG_INVOKE_READ_ERRORS
			bool damageDirection_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out damageDirection);
#if LOG_INVOKE_READ_ERRORS
			if (!damageDirection_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(damageDirection));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDamagedEvent(damageAmount, damageDirection);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveDamagedEvent), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDamagedEvent_Write(NetPakWriter writer, System.Byte damageAmount, UnityEngine.Vector3 damageDirection)
		{
			writer.WriteUInt8(damageAmount);
			writer.WriteClampedVector3(damageDirection);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveFood), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveFood_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newFood;
#if LOG_INVOKE_READ_ERRORS
			bool newFood_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newFood);
#if LOG_INVOKE_READ_ERRORS
			if (!newFood_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFood));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFood(newFood);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveFood), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveFood_Write(NetPakWriter writer, System.Byte newFood)
		{
			writer.WriteUInt8(newFood);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveWater), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWater_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newWater;
#if LOG_INVOKE_READ_ERRORS
			bool newWater_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newWater);
#if LOG_INVOKE_READ_ERRORS
			if (!newWater_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newWater));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWater(newWater);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveWater), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWater_Write(NetPakWriter writer, System.Byte newWater)
		{
			writer.WriteUInt8(newWater);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveVirus), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVirus_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newVirus;
#if LOG_INVOKE_READ_ERRORS
			bool newVirus_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newVirus);
#if LOG_INVOKE_READ_ERRORS
			if (!newVirus_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newVirus));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveVirus(newVirus);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveVirus), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVirus_Write(NetPakWriter writer, System.Byte newVirus)
		{
			writer.WriteUInt8(newVirus);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveBleeding), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBleeding_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newBleeding;
#if LOG_INVOKE_READ_ERRORS
			bool newBleeding_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newBleeding);
#if LOG_INVOKE_READ_ERRORS
			if (!newBleeding_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newBleeding));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBleeding(newBleeding);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveBleeding), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBleeding_Write(NetPakWriter writer, System.Boolean newBleeding)
		{
			writer.WriteBit(newBleeding);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveBroken), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBroken_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newBroken;
#if LOG_INVOKE_READ_ERRORS
			bool newBroken_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newBroken);
#if LOG_INVOKE_READ_ERRORS
			if (!newBroken_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newBroken));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBroken(newBroken);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveBroken), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBroken_Write(NetPakWriter writer, System.Boolean newBroken)
		{
			writer.WriteBit(newBroken);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveModifyStamina), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveModifyStamina_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Int16 delta;
#if LOG_INVOKE_READ_ERRORS
			bool delta_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out delta);
#if LOG_INVOKE_READ_ERRORS
			if (!delta_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(delta));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveModifyStamina(delta);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveModifyStamina), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveModifyStamina_Write(NetPakWriter writer, System.Int16 delta)
		{
			writer.WriteInt16(delta);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveModifyHallucination), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveModifyHallucination_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Int16 delta;
#if LOG_INVOKE_READ_ERRORS
			bool delta_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out delta);
#if LOG_INVOKE_READ_ERRORS
			if (!delta_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(delta));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveModifyHallucination(delta);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveModifyHallucination), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveModifyHallucination_Write(NetPakWriter writer, System.Int16 delta)
		{
			writer.WriteInt16(delta);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveModifyWarmth), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveModifyWarmth_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Int16 delta;
#if LOG_INVOKE_READ_ERRORS
			bool delta_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out delta);
#if LOG_INVOKE_READ_ERRORS
			if (!delta_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(delta));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveModifyWarmth(delta);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveModifyWarmth), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveModifyWarmth_Write(NetPakWriter writer, System.Int16 delta)
		{
			writer.WriteInt16(delta);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveRespawnRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRespawnRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Boolean atHome;
#if LOG_INVOKE_READ_ERRORS
			bool atHome_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out atHome);
#if LOG_INVOKE_READ_ERRORS
			if (!atHome_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(atHome));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRespawnRequest(atHome);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveRespawnRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRespawnRequest_Write(NetPakWriter writer, System.Boolean atHome)
		{
			writer.WriteBit(atHome);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveSuicideRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSuicideRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			NetId netId;
			if (!reader.ReadNetId(out netId))
			{
				context.LogWarning("unable to read target instance net id");
				return;
			}

			object voidNetObj = NetIdRegistry.Get(netId);
			if (voidNetObj == null)
					return;
			PlayerLife netObj = voidNetObj as PlayerLife;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerLife, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveSuicideRequest();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerLife.ReceiveSuicideRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSuicideRequest_Write(NetPakWriter writer)
		{
		}
	}
}
