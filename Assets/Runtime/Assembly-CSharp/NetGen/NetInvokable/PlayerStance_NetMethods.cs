#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerStance))]
	public static class PlayerStance_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerStance.ReceiveClimbRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClimbRequest_Read(in ServerInvocationContext context)
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
			PlayerStance netObj = voidNetObj as PlayerStance;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerStance, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			UnityEngine.Vector3 direction;
#if LOG_INVOKE_READ_ERRORS
			bool direction_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNormalVector3(out direction, bitsPerComponent: 9);
#if LOG_INVOKE_READ_ERRORS
			if (!direction_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(direction));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveClimbRequest(context, direction);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerStance.ReceiveClimbRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveClimbRequest_Write(NetPakWriter writer, UnityEngine.Vector3 direction)
		{
			writer.WriteNormalVector3(direction, bitsPerComponent: 9);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerStance.ReceiveStance), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveStance_Read(in ClientInvocationContext context)
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
			PlayerStance netObj = voidNetObj as PlayerStance;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerStance, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EPlayerStance newStance;
#if LOG_INVOKE_READ_ERRORS
			bool newStance_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newStance);
#if LOG_INVOKE_READ_ERRORS
			if (!newStance_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newStance));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveStance(newStance);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerStance.ReceiveStance), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveStance_Write(NetPakWriter writer, SDG.Unturned.EPlayerStance newStance)
		{
			writer.WriteEnum(newStance);
		}
	}
}
