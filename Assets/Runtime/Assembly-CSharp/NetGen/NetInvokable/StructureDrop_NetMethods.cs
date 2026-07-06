#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(StructureDrop))]
	public static class StructureDrop_NetMethods
	{
		private static void ReceiveHealth_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Byte hp;
#if LOG_INVOKE_READ_ERRORS
			bool hp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out hp);
#if LOG_INVOKE_READ_ERRORS
			if (!hp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveHealth(hp);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveHealth), ENetInvokableGeneratedMethodPurpose.Read)]
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
			{
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveHealth_DeferredRead);
				return;
			}
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte hp;
#if LOG_INVOKE_READ_ERRORS
			bool hp_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out hp);
#if LOG_INVOKE_READ_ERRORS
			if (!hp_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hp));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveHealth(hp);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveHealth), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveHealth_Write(NetPakWriter writer, System.Byte hp)
		{
			writer.WriteUInt8(hp);
		}
		private static void ReceiveTransform_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.Byte old_x;
#if LOG_INVOKE_READ_ERRORS
			bool old_x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out old_x);
#if LOG_INVOKE_READ_ERRORS
			if (!old_x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(old_x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte old_y;
#if LOG_INVOKE_READ_ERRORS
			bool old_y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out old_y);
#if LOG_INVOKE_READ_ERRORS
			if (!old_y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(old_y));
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
			netObj.ReceiveTransform(context, old_x, old_y, point, rotation);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveTransform), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTransform_Read(in ClientInvocationContext context)
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
			{
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveTransform_DeferredRead);
				return;
			}
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte old_x;
#if LOG_INVOKE_READ_ERRORS
			bool old_x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out old_x);
#if LOG_INVOKE_READ_ERRORS
			if (!old_x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(old_x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte old_y;
#if LOG_INVOKE_READ_ERRORS
			bool old_y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out old_y);
#if LOG_INVOKE_READ_ERRORS
			if (!old_y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(old_y));
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
			netObj.ReceiveTransform(context, old_x, old_y, point, rotation);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveTransform), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTransform_Write(NetPakWriter writer, System.Byte old_x, System.Byte old_y, UnityEngine.Vector3 point, UnityEngine.Quaternion rotation)
		{
			writer.WriteUInt8(old_x);
			writer.WriteUInt8(old_y);
			writer.WriteClampedVector3(point, intBitCount: 13, fracBitCount: 11);
			writer.WriteSpecialYawOrQuaternion(rotation, yawBitCount: 23);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveTransformRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTransformRequest_Read(in ServerInvocationContext context)
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
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
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
			netObj.ReceiveTransformRequest(context, point, rotation);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveTransformRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTransformRequest_Write(NetPakWriter writer, UnityEngine.Vector3 point, UnityEngine.Quaternion rotation)
		{
			writer.WriteClampedVector3(point, intBitCount: 13, fracBitCount: 11);
			writer.WriteSpecialYawOrQuaternion(rotation, yawBitCount: 23);
		}
		private static void ReceiveOwnerAndGroup_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.UInt64 newOwner;
#if LOG_INVOKE_READ_ERRORS
			bool newOwner_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt64(out newOwner);
#if LOG_INVOKE_READ_ERRORS
			if (!newOwner_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOwner));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt64 newGroup;
#if LOG_INVOKE_READ_ERRORS
			bool newGroup_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt64(out newGroup);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroup_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroup));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveOwnerAndGroup(newOwner, newGroup);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveOwnerAndGroup), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveOwnerAndGroup_Read(in ClientInvocationContext context)
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
			{
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveOwnerAndGroup_DeferredRead);
				return;
			}
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt64 newOwner;
#if LOG_INVOKE_READ_ERRORS
			bool newOwner_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt64(out newOwner);
#if LOG_INVOKE_READ_ERRORS
			if (!newOwner_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOwner));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt64 newGroup;
#if LOG_INVOKE_READ_ERRORS
			bool newGroup_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt64(out newGroup);
#if LOG_INVOKE_READ_ERRORS
			if (!newGroup_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGroup));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveOwnerAndGroup(newOwner, newGroup);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveOwnerAndGroup), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveOwnerAndGroup_Write(NetPakWriter writer, System.UInt64 newOwner, System.UInt64 newGroup)
		{
			writer.WriteUInt64(newOwner);
			writer.WriteUInt64(newGroup);
		}
		[NetInvokableGeneratedMethod(nameof(StructureDrop.ReceiveSalvageRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSalvageRequest_Read(in ServerInvocationContext context)
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
			StructureDrop netObj = voidNetObj as StructureDrop;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type StructureDrop, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveSalvageRequest(context);
		}
		// ReceiveSalvageRequest write will be called directly.
	}
}
