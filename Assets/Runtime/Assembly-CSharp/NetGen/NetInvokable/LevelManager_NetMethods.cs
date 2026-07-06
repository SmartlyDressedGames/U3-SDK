#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(LevelManager))]
	public static class LevelManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveArenaOrigin), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveArenaOrigin_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			UnityEngine.Vector3 newArenaCurrentCenter;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaCurrentCenter_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newArenaCurrentCenter);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaCurrentCenter_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaCurrentCenter));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newArenaCurrentRadius;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaCurrentRadius_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newArenaCurrentRadius);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaCurrentRadius_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaCurrentRadius));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 newArenaOriginCenter;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaOriginCenter_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newArenaOriginCenter);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaOriginCenter_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaOriginCenter));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newArenaOriginRadius;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaOriginRadius_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newArenaOriginRadius);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaOriginRadius_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaOriginRadius));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 newArenaTargetCenter;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaTargetCenter_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newArenaTargetCenter);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaTargetCenter_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaTargetCenter));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newArenaTargetRadius;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaTargetRadius_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newArenaTargetRadius);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaTargetRadius_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaTargetRadius));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single newArenaCompactorSpeed;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaCompactorSpeed_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out newArenaCompactorSpeed);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaCompactorSpeed_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaCompactorSpeed));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte delay;
#if LOG_INVOKE_READ_ERRORS
			bool delay_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out delay);
#if LOG_INVOKE_READ_ERRORS
			if (!delay_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(delay));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LevelManager.ReceiveArenaOrigin(newArenaCurrentCenter, newArenaCurrentRadius, newArenaOriginCenter, newArenaOriginRadius, newArenaTargetCenter, newArenaTargetRadius, newArenaCompactorSpeed, delay);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveArenaOrigin), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveArenaOrigin_Write(NetPakWriter writer, UnityEngine.Vector3 newArenaCurrentCenter, System.Single newArenaCurrentRadius, UnityEngine.Vector3 newArenaOriginCenter, System.Single newArenaOriginRadius, UnityEngine.Vector3 newArenaTargetCenter, System.Single newArenaTargetRadius, System.Single newArenaCompactorSpeed, System.Byte delay)
		{
			writer.WriteClampedVector3(newArenaCurrentCenter);
			writer.WriteFloat(newArenaCurrentRadius);
			writer.WriteClampedVector3(newArenaOriginCenter);
			writer.WriteFloat(newArenaOriginRadius);
			writer.WriteClampedVector3(newArenaTargetCenter);
			writer.WriteFloat(newArenaTargetRadius);
			writer.WriteFloat(newArenaCompactorSpeed);
			writer.WriteUInt8(delay);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveArenaMessage), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveArenaMessage_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			SDG.Unturned.EArenaMessage newArenaMessage;
#if LOG_INVOKE_READ_ERRORS
			bool newArenaMessage_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newArenaMessage);
#if LOG_INVOKE_READ_ERRORS
			if (!newArenaMessage_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newArenaMessage));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LevelManager.ReceiveArenaMessage(newArenaMessage);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveArenaMessage), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveArenaMessage_Write(NetPakWriter writer, SDG.Unturned.EArenaMessage newArenaMessage)
		{
			writer.WriteEnum(newArenaMessage);
		}
		// ReceiveArenaPlayer read will be called directly.
		// ReceiveArenaPlayer write will be called directly.
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveLevelNumber), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLevelNumber_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte newLevelNumber;
#if LOG_INVOKE_READ_ERRORS
			bool newLevelNumber_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newLevelNumber);
#if LOG_INVOKE_READ_ERRORS
			if (!newLevelNumber_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newLevelNumber));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LevelManager.ReceiveLevelNumber(newLevelNumber);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveLevelNumber), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLevelNumber_Write(NetPakWriter writer, System.Byte newLevelNumber)
		{
			writer.WriteUInt8(newLevelNumber);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveLevelTimer), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveLevelTimer_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte newTimerCount;
#if LOG_INVOKE_READ_ERRORS
			bool newTimerCount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newTimerCount);
#if LOG_INVOKE_READ_ERRORS
			if (!newTimerCount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newTimerCount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LevelManager.ReceiveLevelTimer(newTimerCount);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveLevelTimer), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveLevelTimer_Write(NetPakWriter writer, System.Byte newTimerCount)
		{
			writer.WriteUInt8(newTimerCount);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveAirdropState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAirdropState_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			UnityEngine.Vector3 position;
#if LOG_INVOKE_READ_ERRORS
			bool position_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out position, intBitCount: 14, fracBitCount: 9);
#if LOG_INVOKE_READ_ERRORS
			if (!position_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(position));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 velocity;
#if LOG_INVOKE_READ_ERRORS
			bool velocity_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadVector3AsYawMagnitude(out velocity, yawBitCount: 24);
#if LOG_INVOKE_READ_ERRORS
			if (!velocity_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(velocity));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LevelManager.ReceiveAirdropState(position, velocity);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveAirdropState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAirdropState_Write(NetPakWriter writer, UnityEngine.Vector3 position, UnityEngine.Vector3 velocity)
		{
			writer.WriteClampedVector3(position, intBitCount: 14, fracBitCount: 9);
			writer.WriteVector3AsYawMagnitude(velocity, yawBitCount: 24);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveSpawnCarepackage), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSpawnCarepackage_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
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
			System.Single constantForce;
#if LOG_INVOKE_READ_ERRORS
			bool constantForce_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out constantForce);
#if LOG_INVOKE_READ_ERRORS
			if (!constantForce_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(constantForce));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			LevelManager.ReceiveSpawnCarepackage(position, constantForce);
		}
		[NetInvokableGeneratedMethod(nameof(LevelManager.ReceiveSpawnCarepackage), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSpawnCarepackage_Write(NetPakWriter writer, UnityEngine.Vector3 position, System.Single constantForce)
		{
			writer.WriteClampedVector3(position);
			writer.WriteFloat(constantForce);
		}
	}
}
