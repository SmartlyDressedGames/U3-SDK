#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableSign))]
	public static class InteractableSign_NetMethods
	{
		private static void ReceiveChangeText_DeferredRead(object voidNetObj, in ClientInvocationContext context)
		{
			InteractableSign netObj = voidNetObj as InteractableSign;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance to be type InteractableSign, but was {voidNetObj.GetType().Name}");
				return;
			}
			NetPakReader reader = context.reader;
			System.String newText;
#if LOG_INVOKE_READ_ERRORS
			bool newText_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out newText);
#if LOG_INVOKE_READ_ERRORS
			if (!newText_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newText));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChangeText(newText);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSign.ReceiveChangeText), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChangeText_Read(in ClientInvocationContext context)
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
			{
				NetInvocationDeferralRegistry.Defer(netId, context, ReceiveChangeText_DeferredRead);
				return;
			}
			InteractableSign netObj = voidNetObj as InteractableSign;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableSign, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.String newText;
#if LOG_INVOKE_READ_ERRORS
			bool newText_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out newText);
#if LOG_INVOKE_READ_ERRORS
			if (!newText_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newText));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChangeText(newText);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSign.ReceiveChangeText), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChangeText_Write(NetPakWriter writer, System.String newText)
		{
			writer.WriteString(newText);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSign.ReceiveChangeTextRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChangeTextRequest_Read(in ServerInvocationContext context)
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
			InteractableSign netObj = voidNetObj as InteractableSign;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type InteractableSign, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.String newText;
#if LOG_INVOKE_READ_ERRORS
			bool newText_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out newText);
#if LOG_INVOKE_READ_ERRORS
			if (!newText_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newText));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChangeTextRequest(context, newText);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableSign.ReceiveChangeTextRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChangeTextRequest_Write(NetPakWriter writer, System.String newText)
		{
			writer.WriteString(newText);
		}
	}
}
