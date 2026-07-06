#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerMovement))]
	public static class PlayerMovement_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerMovement.ReceivePluginGravityMultiplier), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePluginGravityMultiplier_Read(in ClientInvocationContext context)
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
			PlayerMovement netObj = voidNetObj as PlayerMovement;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerMovement, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Single newPluginGravityMultiplier;
#if LOG_INVOKE_READ_ERRORS
			bool newPluginGravityMultiplier_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newPluginGravityMultiplier);
#if LOG_INVOKE_READ_ERRORS
			if (!newPluginGravityMultiplier_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPluginGravityMultiplier));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePluginGravityMultiplier(newPluginGravityMultiplier);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerMovement.ReceivePluginGravityMultiplier), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePluginGravityMultiplier_Write(NetPakWriter writer, System.Single newPluginGravityMultiplier)
		{
			writer.WriteFloat(newPluginGravityMultiplier);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerMovement.ReceivePluginJumpMultiplier), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePluginJumpMultiplier_Read(in ClientInvocationContext context)
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
			PlayerMovement netObj = voidNetObj as PlayerMovement;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerMovement, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Single newPluginJumpMultiplier;
#if LOG_INVOKE_READ_ERRORS
			bool newPluginJumpMultiplier_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newPluginJumpMultiplier);
#if LOG_INVOKE_READ_ERRORS
			if (!newPluginJumpMultiplier_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPluginJumpMultiplier));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePluginJumpMultiplier(newPluginJumpMultiplier);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerMovement.ReceivePluginJumpMultiplier), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePluginJumpMultiplier_Write(NetPakWriter writer, System.Single newPluginJumpMultiplier)
		{
			writer.WriteFloat(newPluginJumpMultiplier);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerMovement.ReceivePluginSpeedMultiplier), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePluginSpeedMultiplier_Read(in ClientInvocationContext context)
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
			PlayerMovement netObj = voidNetObj as PlayerMovement;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerMovement, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Single newPluginSpeedMultiplier;
#if LOG_INVOKE_READ_ERRORS
			bool newPluginSpeedMultiplier_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newPluginSpeedMultiplier);
#if LOG_INVOKE_READ_ERRORS
			if (!newPluginSpeedMultiplier_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPluginSpeedMultiplier));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePluginSpeedMultiplier(newPluginSpeedMultiplier);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerMovement.ReceivePluginSpeedMultiplier), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePluginSpeedMultiplier_Write(NetPakWriter writer, System.Single newPluginSpeedMultiplier)
		{
			writer.WriteFloat(newPluginSpeedMultiplier);
		}
	}
}
