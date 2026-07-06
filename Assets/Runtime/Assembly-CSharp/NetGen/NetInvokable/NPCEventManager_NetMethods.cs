#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(NPCEventManager))]
	public static class NPCEventManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(NPCEventManager.ReceiveBroadcast), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBroadcast_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte channelId;
#if LOG_INVOKE_READ_ERRORS
			bool channelId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out channelId);
#if LOG_INVOKE_READ_ERRORS
			if (!channelId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(channelId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String eventId;
#if LOG_INVOKE_READ_ERRORS
			bool eventId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out eventId);
#if LOG_INVOKE_READ_ERRORS
			if (!eventId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(eventId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			NPCEventManager.ReceiveBroadcast(channelId, eventId);
		}
		[NetInvokableGeneratedMethod(nameof(NPCEventManager.ReceiveBroadcast), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBroadcast_Write(NetPakWriter writer, System.Byte channelId, System.String eventId)
		{
			writer.WriteUInt8(channelId);
			writer.WriteString(eventId);
		}
	}
}
