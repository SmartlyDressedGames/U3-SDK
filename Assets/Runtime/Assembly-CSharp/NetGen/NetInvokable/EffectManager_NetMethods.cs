#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(EffectManager))]
	public static class EffectManager_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClearById), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectClearById_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
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
			EffectManager.ReceiveEffectClearById(id);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClearById), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectClearById_Write(NetPakWriter writer, System.UInt16 id)
		{
			writer.WriteUInt16(id);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClearByGuid), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectClearByGuid_Read(in ClientInvocationContext context)
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
			EffectManager.ReceiveEffectClearByGuid(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClearByGuid), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectClearByGuid_Write(NetPakWriter writer, System.Guid assetGuid)
		{
			writer.WriteGuid(assetGuid);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClearAll), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectClearAll_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			EffectManager.ReceiveEffectClearAll();
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClearAll), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectClearAll_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPointNormal_NonUniformScale), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPointNormal_NonUniformScale_Read(in ClientInvocationContext context)
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
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 normal;
#if LOG_INVOKE_READ_ERRORS
			bool normal_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNormalVector3(out normal, bitsPerComponent: 9);
#if LOG_INVOKE_READ_ERRORS
			if (!normal_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(normal));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 scale;
#if LOG_INVOKE_READ_ERRORS
			bool scale_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out scale);
#if LOG_INVOKE_READ_ERRORS
			if (!scale_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(scale));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPointNormal_NonUniformScale(assetGuid, point, normal, scale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPointNormal_NonUniformScale), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPointNormal_NonUniformScale_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 point, UnityEngine.Vector3 normal, UnityEngine.Vector3 scale)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(point);
			writer.WriteNormalVector3(normal, bitsPerComponent: 9);
			writer.WriteClampedVector3(scale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPointNormal_UniformScale), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPointNormal_UniformScale_Read(in ClientInvocationContext context)
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
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 normal;
#if LOG_INVOKE_READ_ERRORS
			bool normal_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNormalVector3(out normal, bitsPerComponent: 9);
#if LOG_INVOKE_READ_ERRORS
			if (!normal_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(normal));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single uniformScale;
#if LOG_INVOKE_READ_ERRORS
			bool uniformScale_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out uniformScale);
#if LOG_INVOKE_READ_ERRORS
			if (!uniformScale_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(uniformScale));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPointNormal_UniformScale(assetGuid, point, normal, uniformScale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPointNormal_UniformScale), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPointNormal_UniformScale_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 point, UnityEngine.Vector3 normal, System.Single uniformScale)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(point);
			writer.WriteNormalVector3(normal, bitsPerComponent: 9);
			writer.WriteFloat(uniformScale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPointNormal), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPointNormal_Read(in ClientInvocationContext context)
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
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 normal;
#if LOG_INVOKE_READ_ERRORS
			bool normal_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNormalVector3(out normal, bitsPerComponent: 9);
#if LOG_INVOKE_READ_ERRORS
			if (!normal_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(normal));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPointNormal(assetGuid, point, normal);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPointNormal), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPointNormal_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 point, UnityEngine.Vector3 normal)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(point);
			writer.WriteNormalVector3(normal, bitsPerComponent: 9);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPoint_NonUniformScale), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPoint_NonUniformScale_Read(in ClientInvocationContext context)
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
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 scale;
#if LOG_INVOKE_READ_ERRORS
			bool scale_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out scale);
#if LOG_INVOKE_READ_ERRORS
			if (!scale_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(scale));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPoint_NonUniformScale(assetGuid, point, scale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPoint_NonUniformScale), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPoint_NonUniformScale_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 point, UnityEngine.Vector3 scale)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(point);
			writer.WriteClampedVector3(scale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPoint_UniformScale), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPoint_UniformScale_Read(in ClientInvocationContext context)
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
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single uniformScale;
#if LOG_INVOKE_READ_ERRORS
			bool uniformScale_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out uniformScale);
#if LOG_INVOKE_READ_ERRORS
			if (!uniformScale_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(uniformScale));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPoint_UniformScale(assetGuid, point, uniformScale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPoint_UniformScale), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPoint_UniformScale_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 point, System.Single uniformScale)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(point);
			writer.WriteFloat(uniformScale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPoint), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPoint_Read(in ClientInvocationContext context)
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
			UnityEngine.Vector3 point;
#if LOG_INVOKE_READ_ERRORS
			bool point_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out point);
#if LOG_INVOKE_READ_ERRORS
			if (!point_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(point));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPoint(assetGuid, point);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPoint), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPoint_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 point)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(point);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPositionRotation_NonUniformScale), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPositionRotation_NonUniformScale_Read(in ClientInvocationContext context)
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
			UnityEngine.Quaternion rotation;
#if LOG_INVOKE_READ_ERRORS
			bool rotation_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadQuaternion(out rotation);
#if LOG_INVOKE_READ_ERRORS
			if (!rotation_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rotation));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Vector3 scale;
#if LOG_INVOKE_READ_ERRORS
			bool scale_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadClampedVector3(out scale);
#if LOG_INVOKE_READ_ERRORS
			if (!scale_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(scale));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPositionRotation_NonUniformScale(assetGuid, position, rotation, scale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPositionRotation_NonUniformScale), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPositionRotation_NonUniformScale_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Vector3 scale)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(position);
			writer.WriteQuaternion(rotation);
			writer.WriteClampedVector3(scale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPositionRotation_UniformScale), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPositionRotation_UniformScale_Read(in ClientInvocationContext context)
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
			UnityEngine.Quaternion rotation;
#if LOG_INVOKE_READ_ERRORS
			bool rotation_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadQuaternion(out rotation);
#if LOG_INVOKE_READ_ERRORS
			if (!rotation_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rotation));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Single uniformScale;
#if LOG_INVOKE_READ_ERRORS
			bool uniformScale_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadFloat(out uniformScale);
#if LOG_INVOKE_READ_ERRORS
			if (!uniformScale_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(uniformScale));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPositionRotation_UniformScale(assetGuid, position, rotation, uniformScale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPositionRotation_UniformScale), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPositionRotation_UniformScale_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, System.Single uniformScale)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(position);
			writer.WriteQuaternion(rotation);
			writer.WriteFloat(uniformScale);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPositionRotation), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectPositionRotation_Read(in ClientInvocationContext context)
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
			UnityEngine.Quaternion rotation;
#if LOG_INVOKE_READ_ERRORS
			bool rotation_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadQuaternion(out rotation);
#if LOG_INVOKE_READ_ERRORS
			if (!rotation_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rotation));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectPositionRotation(assetGuid, position, rotation);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectPositionRotation), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectPositionRotation_Write(NetPakWriter writer, System.Guid assetGuid, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteClampedVector3(position);
			writer.WriteQuaternion(rotation);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect0Args), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffect0Args_Read(in ClientInvocationContext context)
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
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffect0Args(assetGuid, key);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect0Args), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffect0Args_Write(NetPakWriter writer, System.Guid assetGuid, System.Int16 key)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteInt16(key);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect1Arg), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffect1Arg_Read(in ClientInvocationContext context)
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
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg0;
#if LOG_INVOKE_READ_ERRORS
			bool arg0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg0);
#if LOG_INVOKE_READ_ERRORS
			if (!arg0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffect1Arg(assetGuid, key, arg0);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect1Arg), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffect1Arg_Write(NetPakWriter writer, System.Guid assetGuid, System.Int16 key, System.String arg0)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteInt16(key);
			writer.WriteString(arg0);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect2Args), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffect2Args_Read(in ClientInvocationContext context)
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
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg0;
#if LOG_INVOKE_READ_ERRORS
			bool arg0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg0);
#if LOG_INVOKE_READ_ERRORS
			if (!arg0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg1;
#if LOG_INVOKE_READ_ERRORS
			bool arg1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg1);
#if LOG_INVOKE_READ_ERRORS
			if (!arg1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffect2Args(assetGuid, key, arg0, arg1);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect2Args), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffect2Args_Write(NetPakWriter writer, System.Guid assetGuid, System.Int16 key, System.String arg0, System.String arg1)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteInt16(key);
			writer.WriteString(arg0);
			writer.WriteString(arg1);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect3Args), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffect3Args_Read(in ClientInvocationContext context)
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
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg0;
#if LOG_INVOKE_READ_ERRORS
			bool arg0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg0);
#if LOG_INVOKE_READ_ERRORS
			if (!arg0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg1;
#if LOG_INVOKE_READ_ERRORS
			bool arg1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg1);
#if LOG_INVOKE_READ_ERRORS
			if (!arg1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg2;
#if LOG_INVOKE_READ_ERRORS
			bool arg2_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg2);
#if LOG_INVOKE_READ_ERRORS
			if (!arg2_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg2));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffect3Args(assetGuid, key, arg0, arg1, arg2);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect3Args), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffect3Args_Write(NetPakWriter writer, System.Guid assetGuid, System.Int16 key, System.String arg0, System.String arg1, System.String arg2)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteInt16(key);
			writer.WriteString(arg0);
			writer.WriteString(arg1);
			writer.WriteString(arg2);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect4Args), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffect4Args_Read(in ClientInvocationContext context)
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
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg0;
#if LOG_INVOKE_READ_ERRORS
			bool arg0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg0);
#if LOG_INVOKE_READ_ERRORS
			if (!arg0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg1;
#if LOG_INVOKE_READ_ERRORS
			bool arg1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg1);
#if LOG_INVOKE_READ_ERRORS
			if (!arg1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg2;
#if LOG_INVOKE_READ_ERRORS
			bool arg2_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg2);
#if LOG_INVOKE_READ_ERRORS
			if (!arg2_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg2));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String arg3;
#if LOG_INVOKE_READ_ERRORS
			bool arg3_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out arg3);
#if LOG_INVOKE_READ_ERRORS
			if (!arg3_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(arg3));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffect4Args(assetGuid, key, arg0, arg1, arg2, arg3);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffect4Args), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffect4Args_Write(NetPakWriter writer, System.Guid assetGuid, System.Int16 key, System.String arg0, System.String arg1, System.String arg2, System.String arg3)
		{
			writer.WriteGuid(assetGuid);
			writer.WriteInt16(key);
			writer.WriteString(arg0);
			writer.WriteString(arg1);
			writer.WriteString(arg2);
			writer.WriteString(arg3);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffectVisibility), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffectVisibility_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String childNameOrPath;
#if LOG_INVOKE_READ_ERRORS
			bool childNameOrPath_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out childNameOrPath);
#if LOG_INVOKE_READ_ERRORS
			if (!childNameOrPath_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(childNameOrPath));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean visible;
#if LOG_INVOKE_READ_ERRORS
			bool visible_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out visible);
#if LOG_INVOKE_READ_ERRORS
			if (!visible_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(visible));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffectVisibility(key, childNameOrPath, visible);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffectVisibility), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffectVisibility_Write(NetPakWriter writer, System.Int16 key, System.String childNameOrPath, System.Boolean visible)
		{
			writer.WriteInt16(key);
			writer.WriteString(childNameOrPath);
			writer.WriteBit(visible);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffectText), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffectText_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String childNameOrPath;
#if LOG_INVOKE_READ_ERRORS
			bool childNameOrPath_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out childNameOrPath);
#if LOG_INVOKE_READ_ERRORS
			if (!childNameOrPath_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(childNameOrPath));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String text;
#if LOG_INVOKE_READ_ERRORS
			bool text_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out text);
#if LOG_INVOKE_READ_ERRORS
			if (!text_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(text));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffectText(key, childNameOrPath, text);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffectText), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffectText_Write(NetPakWriter writer, System.Int16 key, System.String childNameOrPath, System.String text)
		{
			writer.WriteInt16(key);
			writer.WriteString(childNameOrPath);
			writer.WriteString(text);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffectImageURL), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUIEffectImageURL_Read(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.Int16 key;
#if LOG_INVOKE_READ_ERRORS
			bool key_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadInt16(out key);
#if LOG_INVOKE_READ_ERRORS
			if (!key_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(key));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String childNameOrPath;
#if LOG_INVOKE_READ_ERRORS
			bool childNameOrPath_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out childNameOrPath);
#if LOG_INVOKE_READ_ERRORS
			if (!childNameOrPath_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(childNameOrPath));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String url;
#if LOG_INVOKE_READ_ERRORS
			bool url_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out url);
#if LOG_INVOKE_READ_ERRORS
			if (!url_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(url));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean shouldCache;
#if LOG_INVOKE_READ_ERRORS
			bool shouldCache_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out shouldCache);
#if LOG_INVOKE_READ_ERRORS
			if (!shouldCache_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(shouldCache));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean forceRefresh;
#if LOG_INVOKE_READ_ERRORS
			bool forceRefresh_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out forceRefresh);
#if LOG_INVOKE_READ_ERRORS
			if (!forceRefresh_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(forceRefresh));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveUIEffectImageURL(key, childNameOrPath, url, shouldCache, forceRefresh);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveUIEffectImageURL), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUIEffectImageURL_Write(NetPakWriter writer, System.Int16 key, System.String childNameOrPath, System.String url, System.Boolean shouldCache, System.Boolean forceRefresh)
		{
			writer.WriteInt16(key);
			writer.WriteString(childNameOrPath);
			writer.WriteString(url);
			writer.WriteBit(shouldCache);
			writer.WriteBit(forceRefresh);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClicked), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectClicked_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.String buttonName;
#if LOG_INVOKE_READ_ERRORS
			bool buttonName_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out buttonName);
#if LOG_INVOKE_READ_ERRORS
			if (!buttonName_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(buttonName));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectClicked(context, buttonName);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectClicked), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectClicked_Write(NetPakWriter writer, System.String buttonName)
		{
			writer.WriteString(buttonName);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectTextCommitted), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveEffectTextCommitted_Read(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			System.String inputFieldName;
#if LOG_INVOKE_READ_ERRORS
			bool inputFieldName_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out inputFieldName);
#if LOG_INVOKE_READ_ERRORS
			if (!inputFieldName_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(inputFieldName));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.String text;
#if LOG_INVOKE_READ_ERRORS
			bool text_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out text);
#if LOG_INVOKE_READ_ERRORS
			if (!text_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(text));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			EffectManager.ReceiveEffectTextCommitted(context, inputFieldName, text);
		}
		[NetInvokableGeneratedMethod(nameof(EffectManager.ReceiveEffectTextCommitted), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveEffectTextCommitted_Write(NetPakWriter writer, System.String inputFieldName, System.String text)
		{
			writer.WriteString(inputFieldName);
			writer.WriteString(text);
		}
	}
}
