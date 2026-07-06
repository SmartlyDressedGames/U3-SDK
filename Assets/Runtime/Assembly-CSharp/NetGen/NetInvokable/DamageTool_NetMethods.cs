#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(DamageTool))]
	public static class DamageTool_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(DamageTool.ReceiveSpawnBulletImpact), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSpawnBulletImpact_Read(in ClientInvocationContext context)
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
			System.String materialName;
#if LOG_INVOKE_READ_ERRORS
			bool materialName_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out materialName);
#if LOG_INVOKE_READ_ERRORS
			if (!materialName_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(materialName));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Transform colliderTransform;
#if LOG_INVOKE_READ_ERRORS
			bool colliderTransform_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadTransform(out colliderTransform);
#if LOG_INVOKE_READ_ERRORS
			if (!colliderTransform_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(colliderTransform));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			SDG.Unturned.NetId instigatorNetId;
#if LOG_INVOKE_READ_ERRORS
			bool instigatorNetId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadNetId(out instigatorNetId);
#if LOG_INVOKE_READ_ERRORS
			if (!instigatorNetId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(instigatorNetId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			DamageTool.ReceiveSpawnBulletImpact(position, normal, materialName, colliderTransform, instigatorNetId);
		}
		[NetInvokableGeneratedMethod(nameof(DamageTool.ReceiveSpawnBulletImpact), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSpawnBulletImpact_Write(NetPakWriter writer, UnityEngine.Vector3 position, UnityEngine.Vector3 normal, System.String materialName, UnityEngine.Transform colliderTransform, SDG.Unturned.NetId instigatorNetId)
		{
			writer.WriteClampedVector3(position);
			writer.WriteNormalVector3(normal, bitsPerComponent: 9);
			writer.WriteString(materialName);
			writer.WriteTransform(colliderTransform);
			writer.WriteNetId(instigatorNetId);
		}
		[NetInvokableGeneratedMethod(nameof(DamageTool.ReceiveSpawnLegacyImpact), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSpawnLegacyImpact_Read(in ClientInvocationContext context)
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
			System.String materialName;
#if LOG_INVOKE_READ_ERRORS
			bool materialName_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadString(out materialName);
#if LOG_INVOKE_READ_ERRORS
			if (!materialName_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(materialName));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			UnityEngine.Transform colliderTransform;
#if LOG_INVOKE_READ_ERRORS
			bool colliderTransform_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadTransform(out colliderTransform);
#if LOG_INVOKE_READ_ERRORS
			if (!colliderTransform_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(colliderTransform));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			DamageTool.ReceiveSpawnLegacyImpact(position, normal, materialName, colliderTransform);
		}
		[NetInvokableGeneratedMethod(nameof(DamageTool.ReceiveSpawnLegacyImpact), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSpawnLegacyImpact_Write(NetPakWriter writer, UnityEngine.Vector3 position, UnityEngine.Vector3 normal, System.String materialName, UnityEngine.Transform colliderTransform)
		{
			writer.WriteClampedVector3(position);
			writer.WriteNormalVector3(normal, bitsPerComponent: 9);
			writer.WriteString(materialName);
			writer.WriteTransform(colliderTransform);
		}
	}
}
