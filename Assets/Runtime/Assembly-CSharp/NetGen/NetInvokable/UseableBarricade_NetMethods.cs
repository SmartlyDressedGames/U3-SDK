#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableBarricade))]
	public static class UseableBarricade_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableBarricade.ReceiveBarricadeVehicle), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBarricadeVehicle_Read(in ServerInvocationContext context)
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
			UseableBarricade netObj = voidNetObj as UseableBarricade;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableBarricade, but was {voidNetObj.GetType().Name}");
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
			System.Single newAngle_X;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_X_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newAngle_X);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_X_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle_X));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newAngle_Y;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_Y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newAngle_Y);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_Y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle_Y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newAngle_Z;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_Z_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newAngle_Z);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_Z_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle_Z));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.NetId regionNetId;
#if LOG_INVOKE_READ_ERRORS
			bool regionNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out regionNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!regionNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(regionNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBarricadeVehicle(context, newPoint, newAngle_X, newAngle_Y, newAngle_Z, regionNetId);
		}
		[NetInvokableGeneratedMethod(nameof(UseableBarricade.ReceiveBarricadeVehicle), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBarricadeVehicle_Write(NetPakWriter writer, UnityEngine.Vector3 newPoint, System.Single newAngle_X, System.Single newAngle_Y, System.Single newAngle_Z, SDG.Unturned.NetId regionNetId)
		{
			writer.WriteClampedVector3(newPoint, intBitCount: 13, fracBitCount: 11);
			writer.WriteFloat(newAngle_X);
			writer.WriteFloat(newAngle_Y);
			writer.WriteFloat(newAngle_Z);
			writer.WriteNetId(regionNetId);
		}
		[NetInvokableGeneratedMethod(nameof(UseableBarricade.ReceiveBarricadeNone), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBarricadeNone_Read(in ServerInvocationContext context)
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
			UseableBarricade netObj = voidNetObj as UseableBarricade;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableBarricade, but was {voidNetObj.GetType().Name}");
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
			System.Single newAngle_X;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_X_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newAngle_X);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_X_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle_X));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newAngle_Y;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_Y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newAngle_Y);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_Y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle_Y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newAngle_Z;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_Z_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newAngle_Z);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_Z_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle_Z));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBarricadeNone(context, newPoint, newAngle_X, newAngle_Y, newAngle_Z);
		}
		[NetInvokableGeneratedMethod(nameof(UseableBarricade.ReceiveBarricadeNone), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBarricadeNone_Write(NetPakWriter writer, UnityEngine.Vector3 newPoint, System.Single newAngle_X, System.Single newAngle_Y, System.Single newAngle_Z)
		{
			writer.WriteClampedVector3(newPoint, intBitCount: 13, fracBitCount: 11);
			writer.WriteFloat(newAngle_X);
			writer.WriteFloat(newAngle_Y);
			writer.WriteFloat(newAngle_Z);
		}
		[NetInvokableGeneratedMethod(nameof(UseableBarricade.ReceivePlayBuild), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayBuild_Read(in ClientInvocationContext context)
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
			UseableBarricade netObj = voidNetObj as UseableBarricade;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableBarricade, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayBuild();
		}
		[NetInvokableGeneratedMethod(nameof(UseableBarricade.ReceivePlayBuild), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayBuild_Write(NetPakWriter writer)
		{
		}
	}
}
