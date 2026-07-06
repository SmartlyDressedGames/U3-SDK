#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(LightingManager))]
	public static class LightingManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveInitialLightingState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveInitialLightingState_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 serverTime;
#if LOG_INVOKE_READ_ERRORS
			bool serverTime_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out serverTime);
#if LOG_INVOKE_READ_ERRORS
			if (!serverTime_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(serverTime));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt32 newCycle;
#if LOG_INVOKE_READ_ERRORS
			bool newCycle_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newCycle);
#if LOG_INVOKE_READ_ERRORS
			if (!newCycle_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newCycle));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt32 newOffset;
#if LOG_INVOKE_READ_ERRORS
			bool newOffset_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newOffset);
#if LOG_INVOKE_READ_ERRORS
			if (!newOffset_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOffset));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte moon;
#if LOG_INVOKE_READ_ERRORS
			bool moon_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out moon);
#if LOG_INVOKE_READ_ERRORS
			if (!moon_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(moon));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte wind;
#if LOG_INVOKE_READ_ERRORS
			bool wind_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out wind);
#if LOG_INVOKE_READ_ERRORS
			if (!wind_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(wind));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Guid activeWeatherGuid;
#if LOG_INVOKE_READ_ERRORS
			bool activeWeatherGuid_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out activeWeatherGuid);
#if LOG_INVOKE_READ_ERRORS
			if (!activeWeatherGuid_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(activeWeatherGuid));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single activeWeatherBlendAlpha;
#if LOG_INVOKE_READ_ERRORS
			bool activeWeatherBlendAlpha_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out activeWeatherBlendAlpha);
#if LOG_INVOKE_READ_ERRORS
			if (!activeWeatherBlendAlpha_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(activeWeatherBlendAlpha));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.NetId activeWeatherNetId;
#if LOG_INVOKE_READ_ERRORS
			bool activeWeatherNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out activeWeatherNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!activeWeatherNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(activeWeatherNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Int32 newDateCounter;
#if LOG_INVOKE_READ_ERRORS
			bool newDateCounter_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt32(out newDateCounter);
#if LOG_INVOKE_READ_ERRORS
			if (!newDateCounter_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newDateCounter));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LightingManager.ReceiveInitialLightingState(serverTime, newCycle, newOffset, moon, wind, activeWeatherGuid, activeWeatherBlendAlpha, activeWeatherNetId, newDateCounter);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveInitialLightingState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveInitialLightingState_Write(NetPakWriter writer, System.UInt32 serverTime, System.UInt32 newCycle, System.UInt32 newOffset, System.Byte moon, System.Byte wind, System.Guid activeWeatherGuid, System.Single activeWeatherBlendAlpha, SDG.Unturned.NetId activeWeatherNetId, System.Int32 newDateCounter)
		{
			writer.WriteUInt32(serverTime);
			writer.WriteUInt32(newCycle);
			writer.WriteUInt32(newOffset);
			writer.WriteUInt8(moon);
			writer.WriteUInt8(wind);
			writer.WriteGuid(activeWeatherGuid);
			writer.WriteFloat(activeWeatherBlendAlpha);
			writer.WriteNetId(activeWeatherNetId);
			writer.WriteInt32(newDateCounter);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingCycle), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLightingCycle_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 newScale;
#if LOG_INVOKE_READ_ERRORS
			bool newScale_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newScale);
#if LOG_INVOKE_READ_ERRORS
			if (!newScale_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newScale));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LightingManager.ReceiveLightingCycle(newScale);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingCycle), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLightingCycle_Write(NetPakWriter writer, System.UInt32 newScale)
		{
			writer.WriteUInt32(newScale);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingOffset), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLightingOffset_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt32 newOffset;
#if LOG_INVOKE_READ_ERRORS
			bool newOffset_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out newOffset);
#if LOG_INVOKE_READ_ERRORS
			if (!newOffset_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newOffset));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LightingManager.ReceiveLightingOffset(newOffset);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingOffset), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLightingOffset_Write(NetPakWriter writer, System.UInt32 newOffset)
		{
			writer.WriteUInt32(newOffset);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingWind), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLightingWind_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte newWind;
#if LOG_INVOKE_READ_ERRORS
			bool newWind_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newWind);
#if LOG_INVOKE_READ_ERRORS
			if (!newWind_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newWind));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LightingManager.ReceiveLightingWind(newWind);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingWind), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLightingWind_Write(NetPakWriter writer, System.Byte newWind)
		{
			writer.WriteUInt8(newWind);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveDateCounter), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDateCounter_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Int64 newValue;
#if LOG_INVOKE_READ_ERRORS
			bool newValue_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt64(out newValue);
#if LOG_INVOKE_READ_ERRORS
			if (!newValue_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newValue));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LightingManager.ReceiveDateCounter(newValue);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveDateCounter), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDateCounter_Write(NetPakWriter writer, System.Int64 newValue)
		{
			writer.WriteInt64(newValue);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingActiveWeather), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLightingActiveWeather_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
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
			System.Single blendAlpha;
#if LOG_INVOKE_READ_ERRORS
			bool blendAlpha_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out blendAlpha);
#if LOG_INVOKE_READ_ERRORS
			if (!blendAlpha_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(blendAlpha));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.NetId netId;
#if LOG_INVOKE_READ_ERRORS
			bool netId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out netId);
#if LOG_INVOKE_READ_ERRORS
			if (!netId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(netId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LightingManager.ReceiveLightingActiveWeather(assetGuid, blendAlpha, netId);
		}
		[NetInvokableGeneratedMethod(nameof(LightingManager.ReceiveLightingActiveWeather), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLightingActiveWeather_Write(NetPakWriter writer, System.Guid assetGuid, System.Single blendAlpha, SDG.Unturned.NetId netId)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteFloat(blendAlpha);
			writer.WriteNetId(netId);
		}
	}
}
