#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerInput))]
	public static class PlayerInput_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerInput.ReceiveSimulateMispredictedInputs), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSimulateMispredictedInputs_Read(in ClientInvocationContext context)
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
			PlayerInput netObj = voidNetObj as PlayerInput;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInput, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 frameNumber;
#if LOG_INVOKE_READ_ERRORS
			bool frameNumber_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out frameNumber);
#if LOG_INVOKE_READ_ERRORS
			if (!frameNumber_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(frameNumber));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.EPlayerStance stance;
#if LOG_INVOKE_READ_ERRORS
			bool stance_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out stance);
#if LOG_INVOKE_READ_ERRORS
			if (!stance_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(stance));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
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
			UnityEngine.Vector3 velocity;
#if LOG_INVOKE_READ_ERRORS
			bool velocity_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out velocity);
#if LOG_INVOKE_READ_ERRORS
			if (!velocity_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(velocity));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte stamina;
#if LOG_INVOKE_READ_ERRORS
			bool stamina_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out stamina);
#if LOG_INVOKE_READ_ERRORS
			if (!stamina_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(stamina));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Int32 lastTireOffset;
#if LOG_INVOKE_READ_ERRORS
			bool lastTireOffset_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt32(out lastTireOffset);
#if LOG_INVOKE_READ_ERRORS
			if (!lastTireOffset_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(lastTireOffset));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Int32 lastRestOffset;
#if LOG_INVOKE_READ_ERRORS
			bool lastRestOffset_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt32(out lastRestOffset);
#if LOG_INVOKE_READ_ERRORS
			if (!lastRestOffset_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(lastRestOffset));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSimulateMispredictedInputs(frameNumber, stance, position, velocity, stamina, lastTireOffset, lastRestOffset);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInput.ReceiveSimulateMispredictedInputs), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSimulateMispredictedInputs_Write(NetPakWriter writer, System.UInt32 frameNumber, SDG.Unturned.EPlayerStance stance, UnityEngine.Vector3 position, UnityEngine.Vector3 velocity, System.Byte stamina, System.Int32 lastTireOffset, System.Int32 lastRestOffset)
		{
			writer.WriteUInt32(frameNumber);
			writer.WriteEnum(stance);
			writer.WriteClampedVector3(position);
			writer.WriteClampedVector3(velocity);
			writer.WriteUInt8(stamina);
			writer.WriteInt32(lastTireOffset);
			writer.WriteInt32(lastRestOffset);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInput.ReceiveAckGoodInputs), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAckGoodInputs_Read(in ClientInvocationContext context)
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
			PlayerInput netObj = voidNetObj as PlayerInput;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInput, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.UInt32 frameNumber;
#if LOG_INVOKE_READ_ERRORS
			bool frameNumber_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt32(out frameNumber);
#if LOG_INVOKE_READ_ERRORS
			if (!frameNumber_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(frameNumber));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveAckGoodInputs(frameNumber);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInput.ReceiveAckGoodInputs), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAckGoodInputs_Write(NetPakWriter writer, System.UInt32 frameNumber)
		{
			writer.WriteUInt32(frameNumber);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInput.ReceiveInputs), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveInputs_Read(in ServerInvocationContext context)
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
			PlayerInput netObj = voidNetObj as PlayerInput;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInput, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveInputs(context);
		}
		// ReceiveInputs write will be called directly.
	}
}
