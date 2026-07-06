#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(InteractableObjectQuestEventHook))]
	public static class InteractableObjectQuestEventHook_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(InteractableObjectQuestEventHook.ReceiveUsedNotification), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUsedNotification_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			UnityEngine.Transform eventHookTransform;
#if LOG_INVOKE_READ_ERRORS
			bool eventHookTransform_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadTransform(out eventHookTransform);
#if LOG_INVOKE_READ_ERRORS
			if (!eventHookTransform_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(eventHookTransform));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			InteractableObjectQuestEventHook.ReceiveUsedNotification(eventHookTransform);
		}
		[NetInvokableGeneratedMethod(nameof(InteractableObjectQuestEventHook.ReceiveUsedNotification), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUsedNotification_Write(NetPakWriter writer, UnityEngine.Transform eventHookTransform)
		{
			writer.WriteTransform(eventHookTransform);
		}
	}
}
