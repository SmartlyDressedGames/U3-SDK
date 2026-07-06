#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerAnimator))]
	public static class PlayerAnimator_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerAnimator.ReceiveLean), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLean_Read(in ClientInvocationContext context)
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
			PlayerAnimator netObj = voidNetObj as PlayerAnimator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerAnimator, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte newLean;
#if LOG_INVOKE_READ_ERRORS
			bool newLean_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newLean);
#if LOG_INVOKE_READ_ERRORS
			if (!newLean_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newLean));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveLean(newLean);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerAnimator.ReceiveLean), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLean_Write(NetPakWriter writer, System.Byte newLean)
		{
			writer.WriteUInt8(newLean);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerAnimator.ReceiveGesture), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveGesture_Read(in ClientInvocationContext context)
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
			PlayerAnimator netObj = voidNetObj as PlayerAnimator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerAnimator, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EPlayerGesture newGesture;
#if LOG_INVOKE_READ_ERRORS
			bool newGesture_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newGesture);
#if LOG_INVOKE_READ_ERRORS
			if (!newGesture_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGesture));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveGesture(newGesture);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerAnimator.ReceiveGesture), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveGesture_Write(NetPakWriter writer, SDG.Unturned.EPlayerGesture newGesture)
		{
			writer.WriteEnum(newGesture);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerAnimator.ReceiveGestureRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveGestureRequest_Read(in ServerInvocationContext context)
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
			PlayerAnimator netObj = voidNetObj as PlayerAnimator;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerAnimator, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			SDG.Unturned.EPlayerGesture newGesture;
#if LOG_INVOKE_READ_ERRORS
			bool newGesture_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newGesture);
#if LOG_INVOKE_READ_ERRORS
			if (!newGesture_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGesture));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveGestureRequest(newGesture);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerAnimator.ReceiveGestureRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveGestureRequest_Write(NetPakWriter writer, SDG.Unturned.EPlayerGesture newGesture)
		{
			writer.WriteEnum(newGesture);
		}
	}
}
