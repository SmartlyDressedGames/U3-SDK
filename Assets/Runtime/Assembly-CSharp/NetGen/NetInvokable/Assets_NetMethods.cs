#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(Assets))]
	public static class Assets_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(Assets.ReceiveKickForInvalidGuid), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveKickForInvalidGuid_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Guid guid;
#if LOG_INVOKE_READ_ERRORS
			bool guid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out guid);
#if LOG_INVOKE_READ_ERRORS
			if (!guid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(guid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Assets.ReceiveKickForInvalidGuid(guid);
		}
		[NetInvokableGeneratedMethod(nameof(Assets.ReceiveKickForInvalidGuid), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveKickForInvalidGuid_Write(NetPakWriter writer, System.Guid guid)
		{
			writer.WriteGuid(guid);
		}
		[NetInvokableGeneratedMethod(nameof(Assets.ReceiveKickForHashMismatch), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveKickForHashMismatch_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Guid guid;
#if LOG_INVOKE_READ_ERRORS
			bool guid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out guid);
#if LOG_INVOKE_READ_ERRORS
			if (!guid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(guid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String serverName;
#if LOG_INVOKE_READ_ERRORS
			bool serverName_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out serverName);
#if LOG_INVOKE_READ_ERRORS
			if (!serverName_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(serverName));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String serverFriendlyName;
#if LOG_INVOKE_READ_ERRORS
			bool serverFriendlyName_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out serverFriendlyName);
#if LOG_INVOKE_READ_ERRORS
			if (!serverFriendlyName_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(serverFriendlyName));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] serverHash;
			byte serverHash_Length;
			reader.ReadUInt8(out serverHash_Length);
			serverHash = new byte[serverHash_Length];
			reader.ReadBytes(serverHash);
			System.String serverAssetBundleNameWithoutExtension;
#if LOG_INVOKE_READ_ERRORS
			bool serverAssetBundleNameWithoutExtension_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out serverAssetBundleNameWithoutExtension);
#if LOG_INVOKE_READ_ERRORS
			if (!serverAssetBundleNameWithoutExtension_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(serverAssetBundleNameWithoutExtension));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String serverAssetOrigin;
#if LOG_INVOKE_READ_ERRORS
			bool serverAssetOrigin_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out serverAssetOrigin);
#if LOG_INVOKE_READ_ERRORS
			if (!serverAssetOrigin_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(serverAssetOrigin));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Assets.ReceiveKickForHashMismatch(guid, serverName, serverFriendlyName, serverHash, serverAssetBundleNameWithoutExtension, serverAssetOrigin);
		}
		[NetInvokableGeneratedMethod(nameof(Assets.ReceiveKickForHashMismatch), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveKickForHashMismatch_Write(NetPakWriter writer, System.Guid guid, System.String serverName, System.String serverFriendlyName, System.Byte[] serverHash, System.String serverAssetBundleNameWithoutExtension, System.String serverAssetOrigin)
		{
			writer.WriteGuid(guid);
			writer.WriteString(serverName);
			writer.WriteString(serverFriendlyName);
			byte serverHash_Length = (byte) serverHash.Length;
			writer.WriteUInt8(serverHash_Length);
			writer.WriteBytes(serverHash, serverHash_Length);
			writer.WriteString(serverAssetBundleNameWithoutExtension);
			writer.WriteString(serverAssetOrigin);
		}
	}
}
