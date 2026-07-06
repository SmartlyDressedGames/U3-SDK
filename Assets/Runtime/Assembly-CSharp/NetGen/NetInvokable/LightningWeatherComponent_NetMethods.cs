#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(LightningWeatherComponent))]
	public static class LightningWeatherComponent_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(LightningWeatherComponent.ReceiveLightningStrike), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLightningStrike_Read(in ClientInvocationContext context)
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
			LightningWeatherComponent netObj = voidNetObj as LightningWeatherComponent;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type LightningWeatherComponent, but was {voidNetObj.GetType().Name}");
				return;
			}
			UnityEngine.Vector3 hitPosition;
#if LOG_INVOKE_READ_ERRORS
			bool hitPosition_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out hitPosition);
#if LOG_INVOKE_READ_ERRORS
			if (!hitPosition_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hitPosition));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveLightningStrike(hitPosition);
		}
		[NetInvokableGeneratedMethod(nameof(LightningWeatherComponent.ReceiveLightningStrike), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLightningStrike_Write(NetPakWriter writer, UnityEngine.Vector3 hitPosition)
		{
			writer.WriteClampedVector3(hitPosition);
		}
	}
}
