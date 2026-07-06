#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerInventory))]
	public static class PlayerInventory_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveDragItem), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDragItem_Read(in ServerInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page_0;
#if LOG_INVOKE_READ_ERRORS
			bool page_0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page_0);
#if LOG_INVOKE_READ_ERRORS
			if (!page_0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page_0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x_0;
#if LOG_INVOKE_READ_ERRORS
			bool x_0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x_0);
#if LOG_INVOKE_READ_ERRORS
			if (!x_0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x_0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y_0;
#if LOG_INVOKE_READ_ERRORS
			bool y_0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y_0);
#if LOG_INVOKE_READ_ERRORS
			if (!y_0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y_0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte page_1;
#if LOG_INVOKE_READ_ERRORS
			bool page_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page_1);
#if LOG_INVOKE_READ_ERRORS
			if (!page_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x_1;
#if LOG_INVOKE_READ_ERRORS
			bool x_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x_1);
#if LOG_INVOKE_READ_ERRORS
			if (!x_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y_1;
#if LOG_INVOKE_READ_ERRORS
			bool y_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y_1);
#if LOG_INVOKE_READ_ERRORS
			if (!y_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte rot_1;
#if LOG_INVOKE_READ_ERRORS
			bool rot_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out rot_1);
#if LOG_INVOKE_READ_ERRORS
			if (!rot_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rot_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveDragItem(page_0, x_0, y_0, page_1, x_1, y_1, rot_1);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveDragItem), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDragItem_Write(NetPakWriter writer, System.Byte page_0, System.Byte x_0, System.Byte y_0, System.Byte page_1, System.Byte x_1, System.Byte y_1, System.Byte rot_1)
		{
			writer.WriteUInt8(page_0);
			writer.WriteUInt8(x_0);
			writer.WriteUInt8(y_0);
			writer.WriteUInt8(page_1);
			writer.WriteUInt8(x_1);
			writer.WriteUInt8(y_1);
			writer.WriteUInt8(rot_1);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveSwapItem), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSwapItem_Read(in ServerInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			System.Byte page_0;
#if LOG_INVOKE_READ_ERRORS
			bool page_0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page_0);
#if LOG_INVOKE_READ_ERRORS
			if (!page_0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page_0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x_0;
#if LOG_INVOKE_READ_ERRORS
			bool x_0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x_0);
#if LOG_INVOKE_READ_ERRORS
			if (!x_0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x_0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y_0;
#if LOG_INVOKE_READ_ERRORS
			bool y_0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y_0);
#if LOG_INVOKE_READ_ERRORS
			if (!y_0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y_0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte rot_0;
#if LOG_INVOKE_READ_ERRORS
			bool rot_0_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out rot_0);
#if LOG_INVOKE_READ_ERRORS
			if (!rot_0_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rot_0));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte page_1;
#if LOG_INVOKE_READ_ERRORS
			bool page_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out page_1);
#if LOG_INVOKE_READ_ERRORS
			if (!page_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(page_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte x_1;
#if LOG_INVOKE_READ_ERRORS
			bool x_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out x_1);
#if LOG_INVOKE_READ_ERRORS
			if (!x_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(x_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte y_1;
#if LOG_INVOKE_READ_ERRORS
			bool y_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out y_1);
#if LOG_INVOKE_READ_ERRORS
			if (!y_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(y_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte rot_1;
#if LOG_INVOKE_READ_ERRORS
			bool rot_1_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out rot_1);
#if LOG_INVOKE_READ_ERRORS
			if (!rot_1_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rot_1));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSwapItem(page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveSwapItem), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSwapItem_Write(NetPakWriter writer, System.Byte page_0, System.Byte x_0, System.Byte y_0, System.Byte rot_0, System.Byte page_1, System.Byte x_1, System.Byte y_1, System.Byte rot_1)
		{
			writer.WriteUInt8(page_0);
			writer.WriteUInt8(x_0);
			writer.WriteUInt8(y_0);
			writer.WriteUInt8(rot_0);
			writer.WriteUInt8(page_1);
			writer.WriteUInt8(x_1);
			writer.WriteUInt8(y_1);
			writer.WriteUInt8(rot_1);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveDropItem), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveDropItem_Read(in ServerInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
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
			netObj.ReceiveDropItem(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveDropItem), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveDropItem_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveUpdateAmount), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpdateAmount_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
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
			System.Byte amount;
#if LOG_INVOKE_READ_ERRORS
			bool amount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out amount);
#if LOG_INVOKE_READ_ERRORS
			if (!amount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(amount));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveUpdateAmount(page, index, amount);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveUpdateAmount), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpdateAmount_Write(NetPakWriter writer, System.Byte page, System.Byte index, System.Byte amount)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(index);
			writer.WriteUInt8(amount);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveUpdateQuality), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpdateQuality_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
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
			netObj.ReceiveUpdateQuality(page, index, quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveUpdateQuality), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpdateQuality_Write(NetPakWriter writer, System.Byte page, System.Byte index, System.Byte quality)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(index);
			writer.WriteUInt8(quality);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveUpdateInvState), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveUpdateInvState_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
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
			System.Byte[] state;
			byte state_Length;
			reader.ReadUInt8(out state_Length);
			state = new byte[state_Length];
			reader.ReadBytes(state);
			netObj.ReceiveUpdateInvState(page, index, state);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveUpdateInvState), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveUpdateInvState_Write(NetPakWriter writer, System.Byte page, System.Byte index, System.Byte[] state)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(index);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveItemAdd), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveItemAdd_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
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
			System.Byte rot;
#if LOG_INVOKE_READ_ERRORS
			bool rot_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out rot);
#if LOG_INVOKE_READ_ERRORS
			if (!rot_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(rot));
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
			System.Byte amount;
#if LOG_INVOKE_READ_ERRORS
			bool amount_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out amount);
#if LOG_INVOKE_READ_ERRORS
			if (!amount_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(amount));
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
			netObj.ReceiveItemAdd(page, x, y, rot, id, amount, quality, state);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveItemAdd), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveItemAdd_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y, System.Byte rot, System.UInt16 id, System.Byte amount, System.Byte quality, System.Byte[] state)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			writer.WriteUInt8(rot);
			writer.WriteUInt16(id);
			writer.WriteUInt8(amount);
			writer.WriteUInt8(quality);
			byte state_Length = (byte) state.Length;
			writer.WriteUInt8(state_Length);
			writer.WriteBytes(state, state_Length);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveItemRemove), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveItemRemove_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
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
			netObj.ReceiveItemRemove(page, x, y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveItemRemove), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveItemRemove_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveSize), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveSize_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
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
			System.Byte newWidth;
#if LOG_INVOKE_READ_ERRORS
			bool newWidth_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newWidth);
#if LOG_INVOKE_READ_ERRORS
			if (!newWidth_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newWidth));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.Byte newHeight;
#if LOG_INVOKE_READ_ERRORS
			bool newHeight_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out newHeight);
#if LOG_INVOKE_READ_ERRORS
			if (!newHeight_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newHeight));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveSize(page, newWidth, newHeight);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveSize), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveSize_Write(NetPakWriter writer, System.Byte page, System.Byte newWidth, System.Byte newHeight)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(newWidth);
			writer.WriteUInt8(newHeight);
		}
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveStoraging), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveStoraging_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveStoraging(context);
		}
		// ReceiveStoraging write will be called directly.
		[NetInvokableGeneratedMethod(nameof(PlayerInventory.ReceiveInventory), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveInventory_Read(in ClientInvocationContext context)
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
			PlayerInventory netObj = voidNetObj as PlayerInventory;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type PlayerInventory, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceiveInventory(context);
		}
		// ReceiveInventory write will be called directly.
	}
}
