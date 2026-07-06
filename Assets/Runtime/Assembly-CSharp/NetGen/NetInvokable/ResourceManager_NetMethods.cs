#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(ResourceManager))]
	public static class ResourceManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveClearRegionResources), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClearRegionResources_Read(in ClientInvocationContext context)
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
			ResourceManager.ReceiveClearRegionResources(x, y);
		}
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveClearRegionResources), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveClearRegionResources_Write(NetPakWriter writer, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveForageRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveForageRequest_Read(in ServerInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ResourceManager.ReceiveForageRequest(context, x, y, index);
		}
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveForageRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveForageRequest_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
		}
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveResourceDead), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveResourceDead_Read(in ClientInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
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
			ResourceManager.ReceiveResourceDead(x, y, index, ragdoll);
		}
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveResourceDead), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveResourceDead_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index, UnityEngine.Vector3 ragdoll)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
			writer.WriteClampedVector3(ragdoll);
		}
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveResourceAlive), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveResourceAlive_Read(in ClientInvocationContext context)
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
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ResourceManager.ReceiveResourceAlive(x, y, index);
		}
		[NetInvokableGeneratedMethod(nameof(ResourceManager.ReceiveResourceAlive), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveResourceAlive_Write(NetPakWriter writer, System.Byte x, System.Byte y, System.UInt16 index)
		{
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt16(index);
		}
		// ReceiveResources read will be called directly.
		// ReceiveResources write will be called directly.
	}
}
