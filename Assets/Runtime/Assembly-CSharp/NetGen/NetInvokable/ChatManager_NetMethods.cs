#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(ChatManager))]
	public static class ChatManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteStart), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVoteStart_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			Steamworks.CSteamID origin;
#if LOG_INVOKE_READ_ERRORS
			bool origin_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out origin);
#if LOG_INVOKE_READ_ERRORS
			if (!origin_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(origin));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Steamworks.CSteamID target;
#if LOG_INVOKE_READ_ERRORS
			bool target_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out target);
#if LOG_INVOKE_READ_ERRORS
			if (!target_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(target));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte votesNeeded;
#if LOG_INVOKE_READ_ERRORS
			bool votesNeeded_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out votesNeeded);
#if LOG_INVOKE_READ_ERRORS
			if (!votesNeeded_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(votesNeeded));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveVoteStart(origin, target, votesNeeded);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteStart), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVoteStart_Write(NetPakWriter writer, Steamworks.CSteamID origin, Steamworks.CSteamID target, System.Byte votesNeeded)
		{
			writer.WriteSteamID(origin);
			writer.WriteSteamID(target);
			writer.WriteUInt8(votesNeeded);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteUpdate), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVoteUpdate_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte voteYes;
#if LOG_INVOKE_READ_ERRORS
			bool voteYes_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out voteYes);
#if LOG_INVOKE_READ_ERRORS
			if (!voteYes_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(voteYes));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte voteNo;
#if LOG_INVOKE_READ_ERRORS
			bool voteNo_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out voteNo);
#if LOG_INVOKE_READ_ERRORS
			if (!voteNo_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(voteNo));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveVoteUpdate(voteYes, voteNo);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteUpdate), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVoteUpdate_Write(NetPakWriter writer, System.Byte voteYes, System.Byte voteNo)
		{
			writer.WriteUInt8(voteYes);
			writer.WriteUInt8(voteNo);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteStop), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVoteStop_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			SDG.Unturned.EVotingMessage message;
#if LOG_INVOKE_READ_ERRORS
			bool message_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out message);
#if LOG_INVOKE_READ_ERRORS
			if (!message_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(message));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveVoteStop(message);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteStop), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVoteStop_Write(NetPakWriter writer, SDG.Unturned.EVotingMessage message)
		{
			writer.WriteEnum(message);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteMessage), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVoteMessage_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			SDG.Unturned.EVotingMessage message;
#if LOG_INVOKE_READ_ERRORS
			bool message_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out message);
#if LOG_INVOKE_READ_ERRORS
			if (!message_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(message));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveVoteMessage(message);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveVoteMessage), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVoteMessage_Write(NetPakWriter writer, SDG.Unturned.EVotingMessage message)
		{
			writer.WriteEnum(message);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveSubmitVoteRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSubmitVoteRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Boolean vote;
#if LOG_INVOKE_READ_ERRORS
			bool vote_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out vote);
#if LOG_INVOKE_READ_ERRORS
			if (!vote_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(vote));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveSubmitVoteRequest(context, vote);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveSubmitVoteRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSubmitVoteRequest_Write(NetPakWriter writer, System.Boolean vote)
		{
			writer.WriteBit(vote);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveCallVoteRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveCallVoteRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			Steamworks.CSteamID target;
#if LOG_INVOKE_READ_ERRORS
			bool target_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out target);
#if LOG_INVOKE_READ_ERRORS
			if (!target_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(target));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveCallVoteRequest(context, target);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveCallVoteRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveCallVoteRequest_Write(NetPakWriter writer, Steamworks.CSteamID target)
		{
			writer.WriteSteamID(target);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveChatEntry), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChatEntry_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			Steamworks.CSteamID owner;
#if LOG_INVOKE_READ_ERRORS
			bool owner_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out owner);
#if LOG_INVOKE_READ_ERRORS
			if (!owner_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(owner));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String iconURL;
#if LOG_INVOKE_READ_ERRORS
			bool iconURL_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out iconURL);
#if LOG_INVOKE_READ_ERRORS
			if (!iconURL_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(iconURL));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.EChatMode mode;
#if LOG_INVOKE_READ_ERRORS
			bool mode_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out mode);
#if LOG_INVOKE_READ_ERRORS
			if (!mode_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(mode));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Color color;
#if LOG_INVOKE_READ_ERRORS
			bool color_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadColor32RGB(out color);
#if LOG_INVOKE_READ_ERRORS
			if (!color_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(color));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean rich;
#if LOG_INVOKE_READ_ERRORS
			bool rich_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out rich);
#if LOG_INVOKE_READ_ERRORS
			if (!rich_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rich));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String text;
#if LOG_INVOKE_READ_ERRORS
			bool text_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out text);
#if LOG_INVOKE_READ_ERRORS
			if (!text_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(text));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveChatEntry(owner, iconURL, mode, color, rich, text);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveChatEntry), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChatEntry_Write(NetPakWriter writer, Steamworks.CSteamID owner, System.String iconURL, SDG.Unturned.EChatMode mode, UnityEngine.Color color, System.Boolean rich, System.String text)
		{
			writer.WriteSteamID(owner);
			writer.WriteString(iconURL);
			writer.WriteEnum(mode);
			writer.WriteColor32RGB(color);
			writer.WriteBit(rich);
			writer.WriteString(text);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveChatRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChatRequest_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte flags;
#if LOG_INVOKE_READ_ERRORS
			bool flags_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out flags);
#if LOG_INVOKE_READ_ERRORS
			if (!flags_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(flags));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String text;
#if LOG_INVOKE_READ_ERRORS
			bool text_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out text);
#if LOG_INVOKE_READ_ERRORS
			if (!text_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(text));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ChatManager.ReceiveChatRequest(context, flags, text);
		}
		[NetInvokableGeneratedMethod(nameof(ChatManager.ReceiveChatRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChatRequest_Write(NetPakWriter writer, System.Byte flags, System.String text)
		{
			writer.WriteUInt8(flags);
			writer.WriteString(text);
		}
	}
}
