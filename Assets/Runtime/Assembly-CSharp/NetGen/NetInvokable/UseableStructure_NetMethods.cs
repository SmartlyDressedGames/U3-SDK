#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableStructure))]
	public static class UseableStructure_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableStructure.ReceiveBuildStructure), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBuildStructure_Read(in ServerInvocationContext context)
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
			UseableStructure netObj = voidNetObj as UseableStructure;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableStructure, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			UnityEngine.Vector3 newPoint;
#if LOG_INVOKE_READ_ERRORS
			bool newPoint_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newPoint, intBitCount: 13, fracBitCount: 11);
#if LOG_INVOKE_READ_ERRORS
			if (!newPoint_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPoint));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newAngle;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newAngle);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBuildStructure(context, newPoint, newAngle);
		}
		[NetInvokableGeneratedMethod(nameof(UseableStructure.ReceiveBuildStructure), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBuildStructure_Write(NetPakWriter writer, UnityEngine.Vector3 newPoint, System.Single newAngle)
		{
			writer.WriteClampedVector3(newPoint, intBitCount: 13, fracBitCount: 11);
			writer.WriteFloat(newAngle);
		}
		[NetInvokableGeneratedMethod(nameof(UseableStructure.ReceivePlayConstruct), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayConstruct_Read(in ClientInvocationContext context)
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
			UseableStructure netObj = voidNetObj as UseableStructure;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableStructure, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayConstruct();
		}
		[NetInvokableGeneratedMethod(nameof(UseableStructure.ReceivePlayConstruct), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayConstruct_Write(NetPakWriter writer)
		{
		}
	}
}
