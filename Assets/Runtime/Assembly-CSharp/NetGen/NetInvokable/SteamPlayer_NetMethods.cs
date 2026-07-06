#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(SteamPlayer))]
	public static class SteamPlayer_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(SteamPlayer.ReceiveGetSteamAuthTicketForWebApiRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveGetSteamAuthTicketForWebApiRequest_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.String identity;
#if LOG_INVOKE_READ_ERRORS
			bool identity_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out identity);
#if LOG_INVOKE_READ_ERRORS
			if (!identity_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(identity));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SteamPlayer.ReceiveGetSteamAuthTicketForWebApiRequest(identity);
		}
		[NetInvokableGeneratedMethod(nameof(SteamPlayer.ReceiveGetSteamAuthTicketForWebApiRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveGetSteamAuthTicketForWebApiRequest_Write(NetPakWriter writer, System.String identity)
		{
			writer.WriteString(identity);
		}
		// ReceiveGetSteamAuthTicketForWebApiResponse read will be called directly.
		// ReceiveGetSteamAuthTicketForWebApiResponse write will be called directly.
	}
}
