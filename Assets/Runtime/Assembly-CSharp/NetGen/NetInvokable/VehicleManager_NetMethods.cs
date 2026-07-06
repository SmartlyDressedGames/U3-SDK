#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(VehicleManager))]
	public static class VehicleManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleLockState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleLockState_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Steamworks.CSteamID owner;
#if LOG_INVOKE_READ_ERRORS
			bool owner_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out owner);
#if LOG_INVOKE_READ_ERRORS
			if (!owner_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(owner));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Steamworks.CSteamID group;
#if LOG_INVOKE_READ_ERRORS
			bool group_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out group);
#if LOG_INVOKE_READ_ERRORS
			if (!group_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(group));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean locked;
#if LOG_INVOKE_READ_ERRORS
			bool locked_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out locked);
#if LOG_INVOKE_READ_ERRORS
			if (!locked_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(locked));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleLockState(instanceID, owner, group, locked);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleLockState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleLockState_Write(NetPakWriter writer, System.UInt32 instanceID, Steamworks.CSteamID owner, Steamworks.CSteamID group, System.Boolean locked)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteSteamID(owner);
			writer.WriteSteamID(group);
			writer.WriteBit(locked);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleSkin), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleSkin_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 skinID;
#if LOG_INVOKE_READ_ERRORS
			bool skinID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out skinID);
#if LOG_INVOKE_READ_ERRORS
			if (!skinID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(skinID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 mythicID;
#if LOG_INVOKE_READ_ERRORS
			bool mythicID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out mythicID);
#if LOG_INVOKE_READ_ERRORS
			if (!mythicID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(mythicID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleSkin(instanceID, skinID, mythicID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleSkin), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleSkin_Write(NetPakWriter writer, System.UInt32 instanceID, System.UInt16 skinID, System.UInt16 mythicID)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt16(skinID);
			writer.WriteUInt16(mythicID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleSirens), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleSirens_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean on;
#if LOG_INVOKE_READ_ERRORS
			bool on_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out on);
#if LOG_INVOKE_READ_ERRORS
			if (!on_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(on));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleSirens(instanceID, on);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleSirens), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleSirens_Write(NetPakWriter writer, System.UInt32 instanceID, System.Boolean on)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteBit(on);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleBlimp), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleBlimp_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean on;
#if LOG_INVOKE_READ_ERRORS
			bool on_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out on);
#if LOG_INVOKE_READ_ERRORS
			if (!on_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(on));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleBlimp(instanceID, on);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleBlimp), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleBlimp_Write(NetPakWriter writer, System.UInt32 instanceID, System.Boolean on)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteBit(on);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleHeadlights), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleHeadlights_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean on;
#if LOG_INVOKE_READ_ERRORS
			bool on_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out on);
#if LOG_INVOKE_READ_ERRORS
			if (!on_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(on));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleHeadlights(instanceID, on);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleHeadlights), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleHeadlights_Write(NetPakWriter writer, System.UInt32 instanceID, System.Boolean on)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteBit(on);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleHorn), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleHorn_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleHorn(instanceID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleHorn), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleHorn_Write(NetPakWriter writer, System.UInt32 instanceID)
		{
			writer.WriteUInt32(instanceID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleFuel), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleFuel_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 newFuel;
#if LOG_INVOKE_READ_ERRORS
			bool newFuel_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out newFuel);
#if LOG_INVOKE_READ_ERRORS
			if (!newFuel_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFuel));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleFuel(instanceID, newFuel);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleFuel), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleFuel_Write(NetPakWriter writer, System.UInt32 instanceID, System.UInt16 newFuel)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt16(newFuel);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleBatteryCharge), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleBatteryCharge_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 newBatteryCharge;
#if LOG_INVOKE_READ_ERRORS
			bool newBatteryCharge_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out newBatteryCharge);
#if LOG_INVOKE_READ_ERRORS
			if (!newBatteryCharge_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newBatteryCharge));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleBatteryCharge(instanceID, newBatteryCharge);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleBatteryCharge), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleBatteryCharge_Write(NetPakWriter writer, System.UInt32 instanceID, System.UInt16 newBatteryCharge)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt16(newBatteryCharge);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleTireAliveMask), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleTireAliveMask_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newTireAliveMask;
#if LOG_INVOKE_READ_ERRORS
			bool newTireAliveMask_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newTireAliveMask);
#if LOG_INVOKE_READ_ERRORS
			if (!newTireAliveMask_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newTireAliveMask));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleTireAliveMask(instanceID, newTireAliveMask);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleTireAliveMask), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleTireAliveMask_Write(NetPakWriter writer, System.UInt32 instanceID, System.Byte newTireAliveMask)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt8(newTireAliveMask);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleExploded), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleExploded_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleExploded(instanceID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleExploded), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleExploded_Write(NetPakWriter writer, System.UInt32 instanceID)
		{
			writer.WriteUInt32(instanceID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleHealth), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleHealth_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 newHealth;
#if LOG_INVOKE_READ_ERRORS
			bool newHealth_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out newHealth);
#if LOG_INVOKE_READ_ERRORS
			if (!newHealth_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newHealth));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleHealth(instanceID, newHealth);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleHealth), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleHealth_Write(NetPakWriter writer, System.UInt32 instanceID, System.UInt16 newHealth)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt16(newHealth);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleRecov), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVehicleRecov_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 newPosition;
#if LOG_INVOKE_READ_ERRORS
			bool newPosition_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newPosition);
#if LOG_INVOKE_READ_ERRORS
			if (!newPosition_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPosition));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Int32 newRecov;
#if LOG_INVOKE_READ_ERRORS
			bool newRecov_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt32(out newRecov);
#if LOG_INVOKE_READ_ERRORS
			if (!newRecov_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newRecov));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveVehicleRecov(instanceID, newPosition, newRecov);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveVehicleRecov), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVehicleRecov_Write(NetPakWriter writer, System.UInt32 instanceID, UnityEngine.Vector3 newPosition, System.Int32 newRecov)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteClampedVector3(newPosition);
			writer.WriteInt32(newRecov);
		}
		// ReceiveVehicleStates read will be called directly.
		// ReceiveVehicleStates write will be called directly.
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveDestroySingleVehicle), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDestroySingleVehicle_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveDestroySingleVehicle(instanceID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveDestroySingleVehicle), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDestroySingleVehicle_Write(NetPakWriter writer, System.UInt32 instanceID)
		{
			writer.WriteUInt32(instanceID);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveDestroyAllVehicles), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDestroyAllVehicles_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			VehicleManager.ReceiveDestroyAllVehicles();
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveDestroyAllVehicles), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDestroyAllVehicles_Write(NetPakWriter writer)
		{
		}
		// ReceiveSingleVehicle read will be called directly.
		// ReceiveSingleVehicle write will be called directly.
		// ReceiveMultipleVehicles read will be called directly.
		// ReceiveMultipleVehicles write will be called directly.
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveEnterVehicle), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEnterVehicle_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte seat;
#if LOG_INVOKE_READ_ERRORS
			bool seat_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out seat);
#if LOG_INVOKE_READ_ERRORS
			if (!seat_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(seat));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Steamworks.CSteamID player;
#if LOG_INVOKE_READ_ERRORS
			bool player_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out player);
#if LOG_INVOKE_READ_ERRORS
			if (!player_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(player));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveEnterVehicle(instanceID, seat, player);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveEnterVehicle), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEnterVehicle_Write(NetPakWriter writer, System.UInt32 instanceID, System.Byte seat, Steamworks.CSteamID player)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt8(seat);
			writer.WriteSteamID(player);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveExitVehicle), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveExitVehicle_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte seat;
#if LOG_INVOKE_READ_ERRORS
			bool seat_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out seat);
#if LOG_INVOKE_READ_ERRORS
			if (!seat_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(seat));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
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
			System.Boolean forceUpdate;
#if LOG_INVOKE_READ_ERRORS
			bool forceUpdate_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out forceUpdate);
#if LOG_INVOKE_READ_ERRORS
			if (!forceUpdate_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(forceUpdate));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveExitVehicle(instanceID, seat, point, angle, forceUpdate);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveExitVehicle), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveExitVehicle_Write(NetPakWriter writer, System.UInt32 instanceID, System.Byte seat, UnityEngine.Vector3 point, System.Byte angle, System.Boolean forceUpdate)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt8(seat);
			writer.WriteClampedVector3(point);
			writer.WriteUInt8(angle);
			writer.WriteBit(forceUpdate);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveSwapVehicleSeats), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapVehicleSeats_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte fromSeat;
#if LOG_INVOKE_READ_ERRORS
			bool fromSeat_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out fromSeat);
#if LOG_INVOKE_READ_ERRORS
			if (!fromSeat_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(fromSeat));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte toSeat;
#if LOG_INVOKE_READ_ERRORS
			bool toSeat_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out toSeat);
#if LOG_INVOKE_READ_ERRORS
			if (!toSeat_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(toSeat));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveSwapVehicleSeats(instanceID, fromSeat, toSeat);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveSwapVehicleSeats), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapVehicleSeats_Write(NetPakWriter writer, System.UInt32 instanceID, System.Byte fromSeat, System.Byte toSeat)
		{
			writer.WriteUInt32(instanceID);
			writer.WriteUInt8(fromSeat);
			writer.WriteUInt8(toSeat);
		}
		// ReceiveVehicleLockRequest read will be called directly.
		// ReceiveVehicleLockRequest write will be called directly.
		// ReceiveVehicleSkinRequest read will be called directly.
		// ReceiveVehicleSkinRequest write will be called directly.
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveToggleVehicleHeadlights), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveToggleVehicleHeadlights_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Boolean wantsHeadlightsOn;
#if LOG_INVOKE_READ_ERRORS
			bool wantsHeadlightsOn_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out wantsHeadlightsOn);
#if LOG_INVOKE_READ_ERRORS
			if (!wantsHeadlightsOn_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(wantsHeadlightsOn));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveToggleVehicleHeadlights(context, wantsHeadlightsOn);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveToggleVehicleHeadlights), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveToggleVehicleHeadlights_Write(NetPakWriter writer, System.Boolean wantsHeadlightsOn)
		{
			writer.WriteBit(wantsHeadlightsOn);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveUseVehicleBonus), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUseVehicleBonus_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte bonusType;
#if LOG_INVOKE_READ_ERRORS
			bool bonusType_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out bonusType);
#if LOG_INVOKE_READ_ERRORS
			if (!bonusType_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(bonusType));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveUseVehicleBonus(context, bonusType);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveUseVehicleBonus), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUseVehicleBonus_Write(NetPakWriter writer, System.Byte bonusType)
		{
			writer.WriteUInt8(bonusType);
		}
		// ReceiveStealVehicleBattery read will be called directly.
		// ReceiveStealVehicleBattery write will be called directly.
		// ReceiveVehicleHornRequest read will be called directly.
		// ReceiveVehicleHornRequest write will be called directly.
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveEnterVehicleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEnterVehicleRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 instanceID;
#if LOG_INVOKE_READ_ERRORS
			bool instanceID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out instanceID);
#if LOG_INVOKE_READ_ERRORS
			if (!instanceID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instanceID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] hash;
			byte hash_Length;
			reader.ReadUInt8(out hash_Length);
			hash = new byte[hash_Length];
			reader.ReadBytes(hash);
			System.Byte[] physicsProfileHash;
			byte physicsProfileHash_Length;
			reader.ReadUInt8(out physicsProfileHash_Length);
			physicsProfileHash = new byte[physicsProfileHash_Length];
			reader.ReadBytes(physicsProfileHash);
			System.Byte engine;
#if LOG_INVOKE_READ_ERRORS
			bool engine_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out engine);
#if LOG_INVOKE_READ_ERRORS
			if (!engine_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(engine));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveEnterVehicleRequest(context, instanceID, hash, physicsProfileHash, engine);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveEnterVehicleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEnterVehicleRequest_Write(NetPakWriter writer, System.UInt32 instanceID, System.Byte[] hash, System.Byte[] physicsProfileHash, System.Byte engine)
		{
			writer.WriteUInt32(instanceID);
			byte hash_Length = (byte) hash.Length;
			writer.WriteUInt8(hash_Length);
			writer.WriteBytes(hash, hash_Length);
			byte physicsProfileHash_Length = (byte) physicsProfileHash.Length;
			writer.WriteUInt8(physicsProfileHash_Length);
			writer.WriteBytes(physicsProfileHash, physicsProfileHash_Length);
			writer.WriteUInt8(engine);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveExitVehicleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveExitVehicleRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			UnityEngine.Vector3 velocity;
#if LOG_INVOKE_READ_ERRORS
			bool velocity_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out velocity);
#if LOG_INVOKE_READ_ERRORS
			if (!velocity_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(velocity));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveExitVehicleRequest(context, velocity);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveExitVehicleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveExitVehicleRequest_Write(NetPakWriter writer, UnityEngine.Vector3 velocity)
		{
			writer.WriteClampedVector3(velocity);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveSwapVehicleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapVehicleRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte toSeat;
#if LOG_INVOKE_READ_ERRORS
			bool toSeat_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out toSeat);
#if LOG_INVOKE_READ_ERRORS
			if (!toSeat_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(toSeat));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			VehicleManager.ReceiveSwapVehicleRequest(context, toSeat);
		}
		[NetInvokableGeneratedMethod(nameof(VehicleManager.ReceiveSwapVehicleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapVehicleRequest_Write(NetPakWriter writer, System.Byte toSeat)
		{
			writer.WriteUInt8(toSeat);
		}
	}
}
