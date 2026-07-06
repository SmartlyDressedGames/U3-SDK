#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableMelee))]
	public static class UseableMelee_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceiveSpawnMeleeImpact), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSpawnMeleeImpact_Read(in ClientInvocationContext context)
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
			UseableMelee netObj = voidNetObj as UseableMelee;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableMelee, but was {voidNetObj.GetType().Name}");
				return;
			}
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
			netObj.ReceiveSpawnMeleeImpact(position, normal, materialName, colliderTransform);
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceiveSpawnMeleeImpact), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSpawnMeleeImpact_Write(NetPakWriter writer, UnityEngine.Vector3 position, UnityEngine.Vector3 normal, System.String materialName, UnityEngine.Transform colliderTransform)
		{
			writer.WriteClampedVector3(position);
			writer.WriteNormalVector3(normal, bitsPerComponent: 9);
			writer.WriteString(materialName);
			writer.WriteTransform(colliderTransform);
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceiveInteractMelee), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveInteractMelee_Read(in ServerInvocationContext context)
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
			UseableMelee netObj = voidNetObj as UseableMelee;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableMelee, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			netObj.ReceiveInteractMelee();
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceiveInteractMelee), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveInteractMelee_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceivePlaySwingStart), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlaySwingStart_Read(in ClientInvocationContext context)
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
			UseableMelee netObj = voidNetObj as UseableMelee;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableMelee, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlaySwingStart();
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceivePlaySwingStart), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlaySwingStart_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceivePlaySwingStop), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlaySwingStop_Read(in ClientInvocationContext context)
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
			UseableMelee netObj = voidNetObj as UseableMelee;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableMelee, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlaySwingStop();
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceivePlaySwingStop), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlaySwingStop_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceivePlaySwing), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlaySwing_Read(in ClientInvocationContext context)
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
			UseableMelee netObj = voidNetObj as UseableMelee;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableMelee, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.ESwingMode mode;
#if LOG_INVOKE_READ_ERRORS
			bool mode_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out mode);
#if LOG_INVOKE_READ_ERRORS
			if (!mode_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(mode));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlaySwing(mode);
		}
		[NetInvokableGeneratedMethod(nameof(UseableMelee.ReceivePlaySwing), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlaySwing_Write(NetPakWriter writer, SDG.Unturned.ESwingMode mode)
		{
			writer.WriteEnum(mode);
		}
	}
}
