#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(GroupManager))]
	public static class GroupManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(GroupManager.ReceiveGroupInfo), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveGroupInfo_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			Steamworks.CSteamID groupID;
#if LOG_INVOKE_READ_ERRORS
			bool groupID_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out groupID);
#if LOG_INVOKE_READ_ERRORS
			if (!groupID_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(groupID));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String name;
#if LOG_INVOKE_READ_ERRORS
			bool name_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out name);
#if LOG_INVOKE_READ_ERRORS
			if (!name_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(name));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt32 members;
#if LOG_INVOKE_READ_ERRORS
			bool members_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out members);
#if LOG_INVOKE_READ_ERRORS
			if (!members_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(members));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			GroupManager.ReceiveGroupInfo(groupID, name, members);
		}
		[NetInvokableGeneratedMethod(nameof(GroupManager.ReceiveGroupInfo), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveGroupInfo_Write(NetPakWriter writer, Steamworks.CSteamID groupID, System.String name, System.UInt32 members)
		{
			writer.WriteSteamID(groupID);
			writer.WriteString(name);
			writer.WriteUInt32(members);
		}
	}
}
