#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerClothing))]
	public static class PlayerClothing_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveShirtQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveShirtQuality_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveShirtQuality(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveShirtQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveShirtQuality_Write(NetPakWriter writer, System.Byte quality)
		{
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceivePantsQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePantsQuality_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePantsQuality(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceivePantsQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePantsQuality_Write(NetPakWriter writer, System.Byte quality)
		{
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveHatQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveHatQuality_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveHatQuality(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveHatQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveHatQuality_Write(NetPakWriter writer, System.Byte quality)
		{
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveBackpackQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveBackpackQuality_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveBackpackQuality(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveBackpackQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveBackpackQuality_Write(NetPakWriter writer, System.Byte quality)
		{
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveVestQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVestQuality_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveVestQuality(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveVestQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVestQuality_Write(NetPakWriter writer, System.Byte quality)
		{
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveMaskQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveMaskQuality_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveMaskQuality(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveMaskQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveMaskQuality_Write(NetPakWriter writer, System.Byte quality)
		{
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveGlassesQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveGlassesQuality_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveGlassesQuality(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveGlassesQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveGlassesQuality_Write(NetPakWriter writer, System.Byte quality)
		{
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearShirt), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWearShirt_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.Boolean playEffect;
#if LOG_INVOKE_READ_ERRORS
			bool playEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out playEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!playEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(playEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWearShirt(id, quality, state, playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearShirt), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWearShirt_Write(NetPakWriter writer, System.Guid id, System.Byte quality, System.Byte[] state, System.Boolean playEffect)
		{
			writer.WriteGuid(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteBit(playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapShirtRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapShirtRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapShirtRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapShirtRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapShirtRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearPants), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWearPants_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.Boolean playEffect;
#if LOG_INVOKE_READ_ERRORS
			bool playEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out playEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!playEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(playEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWearPants(id, quality, state, playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearPants), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWearPants_Write(NetPakWriter writer, System.Guid id, System.Byte quality, System.Byte[] state, System.Boolean playEffect)
		{
			writer.WriteGuid(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteBit(playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapPantsRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapPantsRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapPantsRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapPantsRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapPantsRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearHat), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWearHat_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.Boolean playEffect;
#if LOG_INVOKE_READ_ERRORS
			bool playEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out playEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!playEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(playEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWearHat(id, quality, state, playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearHat), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWearHat_Write(NetPakWriter writer, System.Guid id, System.Byte quality, System.Byte[] state, System.Boolean playEffect)
		{
			writer.WriteGuid(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteBit(playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapHatRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapHatRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapHatRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapHatRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapHatRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearBackpack), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWearBackpack_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.Boolean playEffect;
#if LOG_INVOKE_READ_ERRORS
			bool playEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out playEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!playEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(playEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWearBackpack(id, quality, state, playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearBackpack), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWearBackpack_Write(NetPakWriter writer, System.Guid id, System.Byte quality, System.Byte[] state, System.Boolean playEffect)
		{
			writer.WriteGuid(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteBit(playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapBackpackRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapBackpackRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapBackpackRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapBackpackRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapBackpackRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveVisualToggleState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVisualToggleState_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			SDG.Unturned.EVisualToggleType type;
#if LOG_INVOKE_READ_ERRORS
			bool type_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out type);
#if LOG_INVOKE_READ_ERRORS
			if (!type_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(type));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Boolean toggle;
#if LOG_INVOKE_READ_ERRORS
			bool toggle_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out toggle);
#if LOG_INVOKE_READ_ERRORS
			if (!toggle_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(toggle));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveVisualToggleState(type, toggle);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveVisualToggleState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVisualToggleState_Write(NetPakWriter writer, SDG.Unturned.EVisualToggleType type, System.Boolean toggle)
		{
			writer.WriteEnum(type);
			writer.WriteBit(toggle);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveVisualToggleRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveVisualToggleRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			SDG.Unturned.EVisualToggleType type;
#if LOG_INVOKE_READ_ERRORS
			bool type_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out type);
#if LOG_INVOKE_READ_ERRORS
			if (!type_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(type));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveVisualToggleRequest(type);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveVisualToggleRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveVisualToggleRequest_Write(NetPakWriter writer, SDG.Unturned.EVisualToggleType type)
		{
			writer.WriteEnum(type);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearVest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWearVest_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.Boolean playEffect;
#if LOG_INVOKE_READ_ERRORS
			bool playEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out playEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!playEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(playEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWearVest(id, quality, state, playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearVest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWearVest_Write(NetPakWriter writer, System.Guid id, System.Byte quality, System.Byte[] state, System.Boolean playEffect)
		{
			writer.WriteGuid(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteBit(playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapVestRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapVestRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapVestRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapVestRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapVestRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearMask), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWearMask_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.Boolean playEffect;
#if LOG_INVOKE_READ_ERRORS
			bool playEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out playEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!playEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(playEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWearMask(id, quality, state, playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearMask), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWearMask_Write(NetPakWriter writer, System.Guid id, System.Byte quality, System.Byte[] state, System.Boolean playEffect)
		{
			writer.WriteGuid(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteBit(playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapMaskRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapMaskRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapMaskRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapMaskRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapMaskRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearGlasses), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveWearGlasses_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Guid id;
#if LOG_INVOKE_READ_ERRORS
			bool id_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadGuid(out id);
#if LOG_INVOKE_READ_ERRORS
			if (!id_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(id));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte quality;
#if LOG_INVOKE_READ_ERRORS
			bool quality_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out quality);
#if LOG_INVOKE_READ_ERRORS
			if (!quality_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(quality));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			System.Boolean playEffect;
#if LOG_INVOKE_READ_ERRORS
			bool playEffect_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out playEffect);
#if LOG_INVOKE_READ_ERRORS
			if (!playEffect_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(playEffect));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveWearGlasses(id, quality, state, playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveWearGlasses), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveWearGlasses_Write(NetPakWriter writer, System.Guid id, System.Byte quality, System.Byte[] state, System.Boolean playEffect)
		{
			writer.WriteGuid(id);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
			writer.WriteBit(playEffect);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapGlassesRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapGlassesRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page;
#if LOG_INVOKE_READ_ERRORS
			bool page_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page);
#if LOG_INVOKE_READ_ERRORS
			if (!page_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x;
#if LOG_INVOKE_READ_ERRORS
			bool x_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x);
#if LOG_INVOKE_READ_ERRORS
			if (!x_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y;
#if LOG_INVOKE_READ_ERRORS
			bool y_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y);
#if LOG_INVOKE_READ_ERRORS
			if (!y_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapGlassesRequest(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapGlassesRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapGlassesRequest_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveClothingState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveClothingState_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveClothingState(context);
		}
		// ReceiveClothingState write will be called directly.
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveFaceState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveFaceState_Read(in ClientInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveFaceState(index);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveFaceState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveFaceState_Write(NetPakWriter writer, System.Byte index)
		{
			writer.WriteUInt8(index);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapFaceRequest), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapFaceRequest_Read(in ServerInvocationContext context)
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
			PlayerClothing netObj = voidNetObj as PlayerClothing;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerClothing, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte index;
#if LOG_INVOKE_READ_ERRORS
			bool index_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out index);
#if LOG_INVOKE_READ_ERRORS
			if (!index_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(index));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapFaceRequest(index);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerClothing.ReceiveSwapFaceRequest), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapFaceRequest_Write(NetPakWriter writer, System.Byte index)
		{
			writer.WriteUInt8(index);
		}
	}
}
