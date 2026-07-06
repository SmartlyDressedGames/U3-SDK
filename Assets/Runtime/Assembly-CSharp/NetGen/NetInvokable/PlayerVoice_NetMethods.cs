#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerVoice))]
	public static class PlayerVoice_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerVoice.ReceivePermissions), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePermissions_Read(in ClientInvocationContext context)
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
			PlayerVoice netObj = voidNetObj as PlayerVoice;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerVoice, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean allowTalkingWhileDead;
#if LOG_INVOKE_READ_ERRORS
			bool allowTalkingWhileDead_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out allowTalkingWhileDead);
#if LOG_INVOKE_READ_ERRORS
			if (!allowTalkingWhileDead_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(allowTalkingWhileDead));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean customAllowTalking;
#if LOG_INVOKE_READ_ERRORS
			bool customAllowTalking_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out customAllowTalking);
#if LOG_INVOKE_READ_ERRORS
			if (!customAllowTalking_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(customAllowTalking));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePermissions(allowTalkingWhileDead, customAllowTalking);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerVoice.ReceivePermissions), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePermissions_Write(NetPakWriter writer, System.Boolean allowTalkingWhileDead, System.Boolean customAllowTalking)
		{
			writer.WriteBit(allowTalkingWhileDead);
			writer.WriteBit(customAllowTalking);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerVoice.ReceiveVoiceChatRelay), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVoiceChatRelay_Read(in ServerInvocationContext context)
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
			PlayerVoice netObj = voidNetObj as PlayerVoice;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerVoice, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveVoiceChatRelay(context);
		}
		// ReceiveVoiceChatRelay write will be called directly.
		[NetInvokableGeneratedMethod(nameof(PlayerVoice.ReceivePlayVoiceChat), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayVoiceChat_Read(in ClientInvocationContext context)
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
			PlayerVoice netObj = voidNetObj as PlayerVoice;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerVoice, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayVoiceChat(context);
		}
		// ReceivePlayVoiceChat write will be called directly.
	}
}
