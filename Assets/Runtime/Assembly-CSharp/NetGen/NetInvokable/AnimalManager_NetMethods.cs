#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(AnimalManager))]
	public static class AnimalManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalAlive), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAnimalAlive_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 newPosition;
#if LOG_INVOKE_READ_ERRORS
			bool newPosition_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newPosition);
#if LOG_INVOKE_READ_ERRORS
			if (!newPosition_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPosition));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newAngle;
#if LOG_INVOKE_READ_ERRORS
			bool newAngle_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newAngle);
#if LOG_INVOKE_READ_ERRORS
			if (!newAngle_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newAngle));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			AnimalManager.ReceiveAnimalAlive(index, newPosition, newAngle);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalAlive), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAnimalAlive_Write(NetPakWriter writer, System.UInt16 index, UnityEngine.Vector3 newPosition, System.Byte newAngle)
		{
			writer.WriteUInt16(index);
			writer.WriteClampedVector3(newPosition);
			writer.WriteUInt8(newAngle);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalDead), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAnimalDead_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 newRagdoll;
#if LOG_INVOKE_READ_ERRORS
			bool newRagdoll_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out newRagdoll);
#if LOG_INVOKE_READ_ERRORS
			if (!newRagdoll_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newRagdoll));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.ERagdollEffect newRagdollEffect;
#if LOG_INVOKE_READ_ERRORS
			bool newRagdollEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newRagdollEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!newRagdollEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newRagdollEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			AnimalManager.ReceiveAnimalDead(index, newRagdoll, newRagdollEffect);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalDead), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAnimalDead_Write(NetPakWriter writer, System.UInt16 index, UnityEngine.Vector3 newRagdoll, SDG.Unturned.ERagdollEffect newRagdollEffect)
		{
			writer.WriteUInt16(index);
			writer.WriteClampedVector3(newRagdoll);
			writer.WriteEnum(newRagdollEffect);
		}
		// ReceiveAnimalStates read will be called directly.
		// ReceiveAnimalStates write will be called directly.
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalStartle), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAnimalStartle_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte animationIndex;
#if LOG_INVOKE_READ_ERRORS
			bool animationIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out animationIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!animationIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(animationIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			AnimalManager.ReceiveAnimalStartle(index, animationIndex);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalStartle), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAnimalStartle_Write(NetPakWriter writer, System.UInt16 index, System.Byte animationIndex)
		{
			writer.WriteUInt16(index);
			writer.WriteUInt8(animationIndex);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalAttack), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAnimalAttack_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte animationIndex;
#if LOG_INVOKE_READ_ERRORS
			bool animationIndex_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out animationIndex);
#if LOG_INVOKE_READ_ERRORS
			if (!animationIndex_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(animationIndex));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			AnimalManager.ReceiveAnimalAttack(index, animationIndex);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalAttack), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAnimalAttack_Write(NetPakWriter writer, System.UInt16 index, System.Byte animationIndex)
		{
			writer.WriteUInt16(index);
			writer.WriteUInt8(animationIndex);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalPanic), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAnimalPanic_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.UInt16 index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			AnimalManager.ReceiveAnimalPanic(index);
		}
		[NetInvokableGeneratedMethod(nameof(AnimalManager.ReceiveAnimalPanic), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAnimalPanic_Write(NetPakWriter writer, System.UInt16 index)
		{
			writer.WriteUInt16(index);
		}
		// ReceiveMultipleAnimals read will be called directly.
		// ReceiveMultipleAnimals write will be called directly.
		// ReceiveSingleAnimal read will be called directly.
		// ReceiveSingleAnimal write will be called directly.
	}
}
