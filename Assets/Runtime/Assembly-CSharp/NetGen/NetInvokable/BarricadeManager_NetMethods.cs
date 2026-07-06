#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(BarricadeManager))]
	public static class BarricadeManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(BarricadeManager.ReceiveDestroyBarricade), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDestroyBarricade_Read(in ClientInvocationContext context)
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
			BarricadeManager.ReceiveDestroyBarricade(context, netId);
		}
		[NetInvokableGeneratedMethod(nameof(BarricadeManager.ReceiveDestroyBarricade), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDestroyBarricade_Write(NetPakWriter writer, SDG.Unturned.NetId netId)
		{
			writer.WriteNetId(netId);
		}
		[NetInvokableGeneratedMethod(nameof(BarricadeManager.ReceiveClearRegionBarricades), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClearRegionBarricades_Read(in ClientInvocationContext context)
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
			BarricadeManager.ReceiveClearRegionBarricades(x, y);
		}
		[NetInvokableGeneratedMethod(nameof(BarricadeManager.ReceiveClearRegionBarricades), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveClearRegionBarricades_Write(NetPakWriter writer, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(BarricadeManager.ReceiveSingleBarricade), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSingleBarricade_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			SDG.Unturned.NetId parentNetId;
#if LOG_INVOKE_READ_ERRORS
			bool parentNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out parentNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!parentNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(parentNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid assetId;
#if LOG_INVOKE_READ_ERRORS
			bool assetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetId);
#if LOG_INVOKE_READ_ERRORS
			if (!assetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
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
			BarricadeManager.ReceiveSingleBarricade(context, parentNetId, assetId, state, point, rotation, owner, group, netId);
		}
		[NetInvokableGeneratedMethod(nameof(BarricadeManager.ReceiveSingleBarricade), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSingleBarricade_Write(NetPakWriter writer, SDG.Unturned.NetId parentNetId, System.Guid assetId, System.Byte[] state, UnityEngine.Vector3 point, UnityEngine.Quaternion rotation, System.UInt64 owner, System.UInt64 group, SDG.Unturned.NetId netId)
		{
			writer.WriteNetId(parentNetId);
			writer.WriteGuid(assetId);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteClampedVector3(point, intBitCount: 13, fracBitCount: 11);
			writer.WriteSpecialYawOrQuaternion(rotation, yawBitCount: 23);
			writer.WriteUInt64(owner);
			writer.WriteUInt64(group);
			writer.WriteNetId(netId);
		}
		// ReceiveMultipleBarricades read will be called directly.
		// ReceiveMultipleBarricades write will be called directly.
	}
}
