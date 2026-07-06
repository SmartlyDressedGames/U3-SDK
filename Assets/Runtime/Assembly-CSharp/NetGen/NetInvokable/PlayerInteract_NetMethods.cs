#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerInteract))]
	public static class PlayerInteract_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerInteract.ReceiveSalvageTimeOverride), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSalvageTimeOverride_Read(in ClientInvocationContext context)
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
			PlayerInteract netObj = voidNetObj as PlayerInteract;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInteract, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Single overrideValue;
#if LOG_INVOKE_READ_ERRORS
			bool overrideValue_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out overrideValue);
#if LOG_INVOKE_READ_ERRORS
			if (!overrideValue_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(overrideValue));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSalvageTimeOverride(overrideValue);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInteract.ReceiveSalvageTimeOverride), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSalvageTimeOverride_Write(NetPakWriter writer, System.Single overrideValue)
		{
			writer.WriteFloat(overrideValue);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInteract.ReceiveInspectRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveInspectRequest_Read(in ServerInvocationContext context)
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
			PlayerInteract netObj = voidNetObj as PlayerInteract;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInteract, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveInspectRequest();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInteract.ReceiveInspectRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveInspectRequest_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInteract.ReceivePlayInspect), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayInspect_Read(in ClientInvocationContext context)
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
			PlayerInteract netObj = voidNetObj as PlayerInteract;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInteract, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayInspect();
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInteract.ReceivePlayInspect), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayInspect_Write(NetPakWriter writer)
		{
		}
	}
}
