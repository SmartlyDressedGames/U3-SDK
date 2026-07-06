#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(Player))]
	public static class Player_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveScreenshotDestination), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveScreenshotDestination_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveScreenshotDestination(context);
		}
		// ReceiveScreenshotDestination write will be called directly.
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveScreenshotRelay), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveScreenshotRelay_Read(in ServerInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveScreenshotRelay(context);
		}
		// ReceiveScreenshotRelay write will be called directly.
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTakeScreenshot), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTakeScreenshot_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveTakeScreenshot();
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTakeScreenshot), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTakeScreenshot_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveBrowserRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBrowserRequest_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.String msg;
#if LOG_INVOKE_READ_ERRORS
			bool msg_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out msg);
#if LOG_INVOKE_READ_ERRORS
			if (!msg_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(msg));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String url;
#if LOG_INVOKE_READ_ERRORS
			bool url_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out url);
#if LOG_INVOKE_READ_ERRORS
			if (!url_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(url));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBrowserRequest(msg, url);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveBrowserRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBrowserRequest_Write(NetPakWriter writer, System.String msg, System.String url)
		{
			writer.WriteString(msg);
			writer.WriteString(url);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveHintMessage), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveHintMessage_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.String message;
#if LOG_INVOKE_READ_ERRORS
			bool message_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out message);
#if LOG_INVOKE_READ_ERRORS
			if (!message_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(message));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single durationSeconds;
#if LOG_INVOKE_READ_ERRORS
			bool durationSeconds_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out durationSeconds);
#if LOG_INVOKE_READ_ERRORS
			if (!durationSeconds_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(durationSeconds));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveHintMessage(message, durationSeconds);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveHintMessage), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveHintMessage_Write(NetPakWriter writer, System.String message, System.Single durationSeconds)
		{
			writer.WriteString(message);
			writer.WriteFloat(durationSeconds);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTranslatedHint), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTranslatedHint_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid assetGuid;
#if LOG_INVOKE_READ_ERRORS
			bool assetGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out assetGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!assetGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(assetGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String translationKey;
#if LOG_INVOKE_READ_ERRORS
			bool translationKey_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out translationKey);
#if LOG_INVOKE_READ_ERRORS
			if (!translationKey_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(translationKey));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single durationSeconds;
#if LOG_INVOKE_READ_ERRORS
			bool durationSeconds_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out durationSeconds);
#if LOG_INVOKE_READ_ERRORS
			if (!durationSeconds_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(durationSeconds));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTranslatedHint(assetGuid, translationKey, durationSeconds);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTranslatedHint), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTranslatedHint_Write(NetPakWriter writer, System.Guid assetGuid, System.String translationKey, System.Single durationSeconds)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteString(translationKey);
			writer.WriteFloat(durationSeconds);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveRelayToServer), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRelayToServer_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 ip;
#if LOG_INVOKE_READ_ERRORS
			bool ip_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out ip);
#if LOG_INVOKE_READ_ERRORS
			if (!ip_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(ip));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 port;
#if LOG_INVOKE_READ_ERRORS
			bool port_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out port);
#if LOG_INVOKE_READ_ERRORS
			if (!port_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(port));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			Steamworks.CSteamID serverCode;
#if LOG_INVOKE_READ_ERRORS
			bool serverCode_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadSteamID(out serverCode);
#if LOG_INVOKE_READ_ERRORS
			if (!serverCode_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(serverCode));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String password;
#if LOG_INVOKE_READ_ERRORS
			bool password_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out password);
#if LOG_INVOKE_READ_ERRORS
			if (!password_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(password));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean shouldShowMenu;
#if LOG_INVOKE_READ_ERRORS
			bool shouldShowMenu_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out shouldShowMenu);
#if LOG_INVOKE_READ_ERRORS
			if (!shouldShowMenu_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(shouldShowMenu));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveRelayToServer(ip, port, serverCode, password, shouldShowMenu);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveRelayToServer), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRelayToServer_Write(NetPakWriter writer, System.UInt32 ip, System.UInt16 port, Steamworks.CSteamID serverCode, System.String password, System.Boolean shouldShowMenu)
		{
			writer.WriteUInt32(ip);
			writer.WriteUInt16(port);
			writer.WriteSteamID(serverCode);
			writer.WriteString(password);
			writer.WriteBit(shouldShowMenu);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveSetPluginWidgetFlags), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSetPluginWidgetFlags_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 newFlags;
#if LOG_INVOKE_READ_ERRORS
			bool newFlags_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newFlags);
#if LOG_INVOKE_READ_ERRORS
			if (!newFlags_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFlags));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSetPluginWidgetFlags(newFlags);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveSetPluginWidgetFlags), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSetPluginWidgetFlags_Write(NetPakWriter writer, System.UInt32 newFlags)
		{
			writer.WriteUInt32(newFlags);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveAdminUsageFlags), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAdminUsageFlags_Read(in ServerInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.UInt32 newFlagsBitmask;
#if LOG_INVOKE_READ_ERRORS
			bool newFlagsBitmask_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newFlagsBitmask);
#if LOG_INVOKE_READ_ERRORS
			if (!newFlagsBitmask_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFlagsBitmask));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAdminUsageFlags(context, newFlagsBitmask);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveAdminUsageFlags), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAdminUsageFlags_Write(NetPakWriter writer, System.UInt32 newFlagsBitmask)
		{
			writer.WriteUInt32(newFlagsBitmask);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTerminalRelay), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTerminalRelay_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.String internalMessage;
#if LOG_INVOKE_READ_ERRORS
			bool internalMessage_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out internalMessage);
#if LOG_INVOKE_READ_ERRORS
			if (!internalMessage_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(internalMessage));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTerminalRelay(internalMessage);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTerminalRelay), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTerminalRelay_Write(NetPakWriter writer, System.String internalMessage)
		{
			writer.WriteString(internalMessage);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTeleport), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveTeleport_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			UnityEngine.Vector3 position;
#if LOG_INVOKE_READ_ERRORS
			bool position_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out position);
#if LOG_INVOKE_READ_ERRORS
			if (!position_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(position));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte angle;
#if LOG_INVOKE_READ_ERRORS
			bool angle_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out angle);
#if LOG_INVOKE_READ_ERRORS
			if (!angle_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(angle));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveTeleport(position, angle);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveTeleport), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveTeleport_Write(NetPakWriter writer, UnityEngine.Vector3 position, System.Byte angle)
		{
			writer.WriteClampedVector3(position);
			writer.WriteUInt8(angle);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveStat), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveStat_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EPlayerStat stat;
#if LOG_INVOKE_READ_ERRORS
			bool stat_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out stat);
#if LOG_INVOKE_READ_ERRORS
			if (!stat_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(stat));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveStat(stat);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveStat), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveStat_Write(NetPakWriter writer, SDG.Unturned.EPlayerStat stat)
		{
			writer.WriteEnum(stat);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveAchievementUnlocked), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAchievementUnlocked_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.String id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAchievementUnlocked(id);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveAchievementUnlocked), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAchievementUnlocked_Write(NetPakWriter writer, System.String id)
		{
			writer.WriteString(id);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveUIMessage), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIMessage_Read(in ClientInvocationContext context)
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
			Player netObj = voidNetObj as Player;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type Player, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EPlayerMessage message;
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
			netObj.ReceiveUIMessage(message);
		}
		[NetInvokableGeneratedMethod(nameof(Player.ReceiveUIMessage), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIMessage_Write(NetPakWriter writer, SDG.Unturned.EPlayerMessage message)
		{
			writer.WriteEnum(message);
		}
	}
}
