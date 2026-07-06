#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerCrafting))]
	public static class PlayerCrafting_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerCrafting.ReceiveStripAttachments), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveStripAttachments_Read(in ServerInvocationContext context)
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
			PlayerCrafting netObj = voidNetObj as PlayerCrafting;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerCrafting, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveStripAttachments(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerCrafting.ReceiveStripAttachments), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveStripAttachments_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerCrafting.ReceiveRefreshCrafting), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveRefreshCrafting_Read(in ClientInvocationContext context)
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
			PlayerCrafting netObj = voidNetObj as PlayerCrafting;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerCrafting, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveRefreshCrafting();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerCrafting.ReceiveRefreshCrafting), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveRefreshCrafting_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerCrafting.ReceiveCraft), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveCraft_Read(in ServerInvocationContext context)
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
			PlayerCrafting netObj = voidNetObj as PlayerCrafting;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerCrafting, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
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
			System.Byte index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean asManyAsPossible;
#if LOG_INVOKE_READ_ERRORS
			bool asManyAsPossible_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out asManyAsPossible);
#if LOG_INVOKE_READ_ERRORS
			if (!asManyAsPossible_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(asManyAsPossible));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveCraft(context, assetGuid, index, asManyAsPossible);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerCrafting.ReceiveCraft), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveCraft_Write(NetPakWriter writer, System.Guid assetGuid, System.Byte index, System.Boolean asManyAsPossible)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteUInt8(index);
			writer.WriteBit(asManyAsPossible);
		}
	}
}
