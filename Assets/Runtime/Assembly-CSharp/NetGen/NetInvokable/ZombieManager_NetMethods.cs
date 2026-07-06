#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(ZombieManager))]
	public static class ZombieManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveBeacon), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBeacon_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean hasBeacon;
#if LOG_INVOKE_READ_ERRORS
			bool hasBeacon_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out hasBeacon);
#if LOG_INVOKE_READ_ERRORS
			if (!hasBeacon_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(hasBeacon));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveBeacon(reference, hasBeacon);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveBeacon), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBeacon_Write(NetPakWriter writer, System.Byte reference, System.Boolean hasBeacon)
		{
			writer.WriteUInt8(reference);
			writer.WriteBit(hasBeacon);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveWave), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWave_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Boolean newWaveReady;
#if LOG_INVOKE_READ_ERRORS
			bool newWaveReady_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newWaveReady);
#if LOG_INVOKE_READ_ERRORS
			if (!newWaveReady_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newWaveReady));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Int32 newWave;
#if LOG_INVOKE_READ_ERRORS
			bool newWave_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt32(out newWave);
#if LOG_INVOKE_READ_ERRORS
			if (!newWave_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newWave));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveWave(newWaveReady, newWave);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveWave), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWave_Write(NetPakWriter writer, System.Boolean newWaveReady, System.Int32 newWave)
		{
			writer.WriteBit(newWaveReady);
			writer.WriteInt32(newWave);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieAlive), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieAlive_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newType;
#if LOG_INVOKE_READ_ERRORS
			bool newType_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newType);
#if LOG_INVOKE_READ_ERRORS
			if (!newType_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newType));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newSpeciality;
#if LOG_INVOKE_READ_ERRORS
			bool newSpeciality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newSpeciality);
#if LOG_INVOKE_READ_ERRORS
			if (!newSpeciality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newSpeciality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newShirt;
#if LOG_INVOKE_READ_ERRORS
			bool newShirt_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newShirt);
#if LOG_INVOKE_READ_ERRORS
			if (!newShirt_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newShirt));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newPants;
#if LOG_INVOKE_READ_ERRORS
			bool newPants_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newPants);
#if LOG_INVOKE_READ_ERRORS
			if (!newPants_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newPants));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newHat;
#if LOG_INVOKE_READ_ERRORS
			bool newHat_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newHat);
#if LOG_INVOKE_READ_ERRORS
			if (!newHat_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newHat));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newGear;
#if LOG_INVOKE_READ_ERRORS
			bool newGear_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newGear);
#if LOG_INVOKE_READ_ERRORS
			if (!newGear_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newGear));
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
			ZombieManager.ReceiveZombieAlive(reference, id, newType, newSpeciality, newShirt, newPants, newHat, newGear, newPosition, newAngle);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieAlive), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieAlive_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, System.Byte newType, System.Byte newSpeciality, System.Byte newShirt, System.Byte newPants, System.Byte newHat, System.Byte newGear, UnityEngine.Vector3 newPosition, System.Byte newAngle)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteUInt8(newType);
			writer.WriteUInt8(newSpeciality);
			writer.WriteUInt8(newShirt);
			writer.WriteUInt8(newPants);
			writer.WriteUInt8(newHat);
			writer.WriteUInt8(newGear);
			writer.WriteClampedVector3(newPosition);
			writer.WriteUInt8(newAngle);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieDead), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieDead_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
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
			ZombieManager.ReceiveZombieDead(reference, id, newRagdoll, newRagdollEffect);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieDead), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieDead_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, UnityEngine.Vector3 newRagdoll, SDG.Unturned.ERagdollEffect newRagdollEffect)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteClampedVector3(newRagdoll);
			writer.WriteEnum(newRagdollEffect);
		}
		// ReceiveZombieStates read will be called directly.
		// ReceiveZombieStates write will be called directly.
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieSpeciality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieSpeciality_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.EZombieSpeciality speciality;
#if LOG_INVOKE_READ_ERRORS
			bool speciality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out speciality);
#if LOG_INVOKE_READ_ERRORS
			if (!speciality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(speciality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieSpeciality(reference, id, speciality);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieSpeciality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieSpeciality_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, SDG.Unturned.EZombieSpeciality speciality)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteEnum(speciality);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieThrow), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieThrow_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieThrow(reference, id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieThrow), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieThrow_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieBoulder), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieBoulder_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 origin;
#if LOG_INVOKE_READ_ERRORS
			bool origin_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out origin);
#if LOG_INVOKE_READ_ERRORS
			if (!origin_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(origin));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 direction;
#if LOG_INVOKE_READ_ERRORS
			bool direction_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNormalVector3(out direction, bitsPerComponent: 9);
#if LOG_INVOKE_READ_ERRORS
			if (!direction_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(direction));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieBoulder(reference, id, origin, direction);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieBoulder), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieBoulder_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, UnityEngine.Vector3 origin, UnityEngine.Vector3 direction)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteClampedVector3(origin);
			writer.WriteNormalVector3(direction, bitsPerComponent: 9);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieSpit), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieSpit_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieSpit(reference, id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieSpit), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieSpit_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieCharge), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieCharge_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieCharge(reference, id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieCharge), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieCharge_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieStomp), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieStomp_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieStomp(reference, id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieStomp), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieStomp_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieBreath), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieBreath_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieBreath(reference, id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieBreath), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieBreath_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieAcid), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieAcid_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 origin;
#if LOG_INVOKE_READ_ERRORS
			bool origin_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out origin);
#if LOG_INVOKE_READ_ERRORS
			if (!origin_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(origin));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 direction;
#if LOG_INVOKE_READ_ERRORS
			bool direction_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNormalVector3(out direction, bitsPerComponent: 9);
#if LOG_INVOKE_READ_ERRORS
			if (!direction_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(direction));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieAcid(reference, id, origin, direction);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieAcid), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieAcid_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, UnityEngine.Vector3 origin, UnityEngine.Vector3 direction)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteClampedVector3(origin);
			writer.WriteNormalVector3(direction, bitsPerComponent: 9);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieSpark), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieSpark_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 target;
#if LOG_INVOKE_READ_ERRORS
			bool target_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out target);
#if LOG_INVOKE_READ_ERRORS
			if (!target_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(target));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieSpark(reference, id, target);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieSpark), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieSpark_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, UnityEngine.Vector3 target)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteClampedVector3(target);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieAttack), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieAttack_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte attack;
#if LOG_INVOKE_READ_ERRORS
			bool attack_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out attack);
#if LOG_INVOKE_READ_ERRORS
			if (!attack_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(attack));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieAttack(reference, id, attack);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieAttack), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieAttack_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, System.Byte attack)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteUInt8(attack);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieStartle), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieStartle_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte startle;
#if LOG_INVOKE_READ_ERRORS
			bool startle_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out startle);
#if LOG_INVOKE_READ_ERRORS
			if (!startle_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(startle));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieStartle(reference, id, startle);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieStartle), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieStartle_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, System.Byte startle)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteUInt8(startle);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieStun), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveZombieStun_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Byte reference;
#if LOG_INVOKE_READ_ERRORS
			bool reference_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out reference);
#if LOG_INVOKE_READ_ERRORS
			if (!reference_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(reference));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte stun;
#if LOG_INVOKE_READ_ERRORS
			bool stun_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out stun);
#if LOG_INVOKE_READ_ERRORS
			if (!stun_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(stun));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			ZombieManager.ReceiveZombieStun(reference, id, stun);
		}
		[NetInvokableGeneratedMethod(nameof(ZombieManager.ReceiveZombieStun), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveZombieStun_Write(NetPakWriter writer, System.Byte reference, System.UInt16 id, System.Byte stun)
		{
			writer.WriteUInt8(reference);
			writer.WriteUInt16(id);
			writer.WriteUInt8(stun);
		}
		// ReceiveZombies read will be called directly.
		// ReceiveZombies write will be called directly.
	}
}
