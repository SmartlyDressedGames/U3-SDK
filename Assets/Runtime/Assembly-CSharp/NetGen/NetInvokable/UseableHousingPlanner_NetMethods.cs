#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableHousingPlanner))]
	public static class UseableHousingPlanner_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableHousingPlanner.ReceivePlaceHousingItemResult), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlaceHousingItemResult_Read(in ClientInvocationContext context)
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
			UseableHousingPlanner netObj = voidNetObj as UseableHousingPlanner;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableHousingPlanner, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean success;
#if LOG_INVOKE_READ_ERRORS
			bool success_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out success);
#if LOG_INVOKE_READ_ERRORS
			if (!success_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(success));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlaceHousingItemResult(success);
		}
		[NetInvokableGeneratedMethod(nameof(UseableHousingPlanner.ReceivePlaceHousingItemResult), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlaceHousingItemResult_Write(NetPakWriter writer, System.Boolean success)
		{
			writer.WriteBit(success);
		}
		[NetInvokableGeneratedMethod(nameof(UseableHousingPlanner.ReceivePlaceHousingItem), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlaceHousingItem_Read(in ServerInvocationContext context)
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
			UseableHousingPlanner netObj = voidNetObj as UseableHousingPlanner;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableHousingPlanner, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 position;
#if LOG_INVOKE_READ_ERRORS
			bool position_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out position, intBitCount: 13, fracBitCount: 11);
#if LOG_INVOKE_READ_ERRORS
			if (!position_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(position));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single yaw;
#if LOG_INVOKE_READ_ERRORS
			bool yaw_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out yaw);
#if LOG_INVOKE_READ_ERRORS
			if (!yaw_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(yaw));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid blueprintGuid;
#if LOG_INVOKE_READ_ERRORS
			bool blueprintGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out blueprintGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!blueprintGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(blueprintGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte blueprintIndex;
#if LOG_INVOKE_READ_ERRORS
			bool blueprintIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out blueprintIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!blueprintIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(blueprintIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlaceHousingItem(context, assetGuid, position, yaw, blueprintGuid, blueprintIndex);
		}
		[NetInvokableGeneratedMethod(nameof(UseableHousingPlanner.ReceivePlaceHousingItem), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlaceHousingItem_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 position, System.Single yaw, System.Guid blueprintGuid, System.Byte blueprintIndex)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(position, intBitCount: 13, fracBitCount: 11);
			writer.WriteFloat(yaw);
			writer.WriteGuid(blueprintGuid);
			writer.WriteUInt8(blueprintIndex);
		}
	}
}
