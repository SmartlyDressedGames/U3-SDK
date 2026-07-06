#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(StructureManager))]
	public static class StructureManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(StructureManager.ReceiveDestroyStructure), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDestroyStructure_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			SDG.Unturned.NetId netId;
#if LOG_INVOKE_READ_ERRORS
			bool netId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out netId);
#if LOG_INVOKE_READ_ERRORS
			if (!netId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(netId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 ragdoll;
#if LOG_INVOKE_READ_ERRORS
			bool ragdoll_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out ragdoll);
#if LOG_INVOKE_READ_ERRORS
			if (!ragdoll_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(ragdoll));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean wasPickedUp;
#if LOG_INVOKE_READ_ERRORS
			bool wasPickedUp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out wasPickedUp);
#if LOG_INVOKE_READ_ERRORS
			if (!wasPickedUp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(wasPickedUp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			StructureManager.ReceiveDestroyStructure(context, netId, ragdoll, wasPickedUp);
		}
		[NetInvokableGeneratedMethod(nameof(StructureManager.ReceiveDestroyStructure), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDestroyStructure_Write(NetPakWriter writer, SDG.Unturned.NetId netId, UnityEngine.Vector3 ragdoll, System.Boolean wasPickedUp)
		{
			writer.WriteNetId(netId);
			writer.WriteClampedVector3(ragdoll);
			writer.WriteBit(wasPickedUp);
		}
		[NetInvokableGeneratedMethod(nameof(StructureManager.ReceiveClearRegionStructures), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClearRegionStructures_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			StructureManager.ReceiveClearRegionStructures(x, y);
		}
		[NetInvokableGeneratedMethod(nameof(StructureManager.ReceiveClearRegionStructures), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveClearRegionStructures_Write(NetPakWriter writer, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(StructureManager.ReceiveSingleStructure), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSingleStructure_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point, intBitCount: 13, fracBitCount: 11);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Quaternion rotation;
#if LOG_INVOKE_READ_ERRORS
			bool rotation_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSpecialYawOrQuaternion(out rotation, yawBitCount: 23);
#if LOG_INVOKE_READ_ERRORS
			if (!rotation_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rotation));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt64 owner;
#if LOG_INVOKE_READ_ERRORS
			bool owner_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt64(out owner);
#if LOG_INVOKE_READ_ERRORS
			if (!owner_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(owner));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt64 group;
#if LOG_INVOKE_READ_ERRORS
			bool group_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt64(out group);
#if LOG_INVOKE_READ_ERRORS
			if (!group_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(group));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.NetId netId;
#if LOG_INVOKE_READ_ERRORS
			bool netId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out netId);
#if LOG_INVOKE_READ_ERRORS
			if (!netId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(netId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			StructureManager.ReceiveSingleStructure(x, y, id, point, rotation, owner, group, netId);
		}
		[NetInvokableGeneratedMethod(nameof(StructureManager.ReceiveSingleStructure), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSingleStructure_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.Guid id, UnityEngine.Vector3 point, UnityEngine.Quaternion rotation, System.UInt64 owner, System.UInt64 group, SDG.Unturned.NetId netId)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteGuid(id);
			writer.WriteClampedVector3(point, intBitCount: 13, fracBitCount: 11);
			writer.WriteSpecialYawOrQuaternion(rotation, yawBitCount: 23);
			writer.WriteUInt64(owner);
			writer.WriteUInt64(group);
			writer.WriteNetId(netId);
		}
		// ReceiveMultipleStructures read will be called directly.
		// ReceiveMultipleStructures write will be called directly.
	}
}
