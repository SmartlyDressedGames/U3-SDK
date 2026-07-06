#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(UseableGun))]
	public static class UseableGun_NetMethods
	{
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveChangeFiremode), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveChangeFiremode_Read(in ServerInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
				return;
			}
			if (!context.IsOwnerOf(netObj.channel))
			{
				context.Kick($"not owner of {netObj}");
				return;
			}
			SDG.Unturned.EFiremode newFiremode;
#if LOG_INVOKE_READ_ERRORS
			bool newFiremode_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadEnum(out newFiremode);
#if LOG_INVOKE_READ_ERRORS
			if (!newFiremode_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newFiremode));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceiveChangeFiremode(newFiremode);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveChangeFiremode), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveChangeFiremode_Write(NetPakWriter writer, SDG.Unturned.EFiremode newFiremode)
		{
			writer.WriteEnum(newFiremode);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayProject), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayProject_Read(in ClientInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
				return;
			}
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
			System.UInt16 barrelId;
#if LOG_INVOKE_READ_ERRORS
			bool barrelId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out barrelId);
#if LOG_INVOKE_READ_ERRORS
			if (!barrelId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(barrelId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			System.UInt16 magazineId;
#if LOG_INVOKE_READ_ERRORS
			bool magazineId_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt16(out magazineId);
#if LOG_INVOKE_READ_ERRORS
			if (!magazineId_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(magazineId));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlayProject(origin, direction, barrelId, magazineId);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayProject), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayProject_Write(NetPakWriter writer, UnityEngine.Vector3 origin, UnityEngine.Vector3 direction, System.UInt16 barrelId, System.UInt16 magazineId)
		{
			writer.WriteClampedVector3(origin);
			writer.WriteNormalVector3(direction, bitsPerComponent: 9);
			writer.WriteUInt16(barrelId);
			writer.WriteUInt16(magazineId);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayShoot), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayShoot_Read(in ClientInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayShoot();
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayShoot), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayShoot_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachSight), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAttachSight_Read(in ServerInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
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
			System.Byte[] hash;
			byte hash_Length;
			reader.ReadUInt8(out hash_Length);
			hash = new byte[hash_Length];
			reader.ReadBytes(hash);
			netObj.ReceiveAttachSight(page, x, y, hash);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachSight), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAttachSight_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y, System.Byte[] hash)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			byte hash_Length = (byte) hash.Length;
			writer.WriteUInt8(hash_Length);
			writer.WriteBytes(hash, hash_Length);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachTactical), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAttachTactical_Read(in ServerInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
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
			System.Byte[] hash;
			byte hash_Length;
			reader.ReadUInt8(out hash_Length);
			hash = new byte[hash_Length];
			reader.ReadBytes(hash);
			netObj.ReceiveAttachTactical(page, x, y, hash);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachTactical), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAttachTactical_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y, System.Byte[] hash)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			byte hash_Length = (byte) hash.Length;
			writer.WriteUInt8(hash_Length);
			writer.WriteBytes(hash, hash_Length);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachGrip), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAttachGrip_Read(in ServerInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
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
			System.Byte[] hash;
			byte hash_Length;
			reader.ReadUInt8(out hash_Length);
			hash = new byte[hash_Length];
			reader.ReadBytes(hash);
			netObj.ReceiveAttachGrip(page, x, y, hash);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachGrip), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAttachGrip_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y, System.Byte[] hash)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			byte hash_Length = (byte) hash.Length;
			writer.WriteUInt8(hash_Length);
			writer.WriteBytes(hash, hash_Length);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachBarrel), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAttachBarrel_Read(in ServerInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
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
			System.Byte[] hash;
			byte hash_Length;
			reader.ReadUInt8(out hash_Length);
			hash = new byte[hash_Length];
			reader.ReadBytes(hash);
			netObj.ReceiveAttachBarrel(page, x, y, hash);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachBarrel), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAttachBarrel_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y, System.Byte[] hash)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			byte hash_Length = (byte) hash.Length;
			writer.WriteUInt8(hash_Length);
			writer.WriteBytes(hash, hash_Length);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachMagazine), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceiveAttachMagazine_Read(in ServerInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
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
			System.Byte[] hash;
			byte hash_Length;
			reader.ReadUInt8(out hash_Length);
			hash = new byte[hash_Length];
			reader.ReadBytes(hash);
			netObj.ReceiveAttachMagazine(context, page, x, y, hash);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceiveAttachMagazine), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceiveAttachMagazine_Write(NetPakWriter writer, System.Byte page, System.Byte x, System.Byte y, System.Byte[] hash)
		{
			writer.WriteUInt8(page);
			writer.WriteUInt8(x);
			writer.WriteUInt8(y);
			byte hash_Length = (byte) hash.Length;
			writer.WriteUInt8(hash_Length);
			writer.WriteBytes(hash, hash_Length);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayReload), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayReload_Read(in ClientInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Boolean newHammer;
#if LOG_INVOKE_READ_ERRORS
			bool newHammer_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadBit(out newHammer);
#if LOG_INVOKE_READ_ERRORS
			if (!newHammer_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(newHammer));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlayReload(newHammer);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayReload), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayReload_Write(NetPakWriter writer, System.Boolean newHammer)
		{
			writer.WriteBit(newHammer);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayChamberJammed), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayChamberJammed_Read(in ClientInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
				return;
			}
			System.Byte correctedAmmo;
#if LOG_INVOKE_READ_ERRORS
			bool correctedAmmo_ReadSuccess =
#endif // LOG_INVOKE_READ_ERRORS
			reader.ReadUInt8(out correctedAmmo);
#if LOG_INVOKE_READ_ERRORS
			if (!correctedAmmo_ReadSuccess)
			{
				context.ReadParameterFailed(nameof(correctedAmmo));
				return;
			}
#endif // LOG_INVOKE_READ_ERRORS
			netObj.ReceivePlayChamberJammed(correctedAmmo);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayChamberJammed), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayChamberJammed_Write(NetPakWriter writer, System.Byte correctedAmmo)
		{
			writer.WriteUInt8(correctedAmmo);
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayAimStart), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayAimStart_Read(in ClientInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayAimStart();
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayAimStart), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayAimStart_Write(NetPakWriter writer)
		{
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayAimStop), ENetInvokableGeneratedMethodPurpose.Read)]
		public static void ReceivePlayAimStop_Read(in ClientInvocationContext context)
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
			UseableGun netObj = voidNetObj as UseableGun;
			if (netObj == null)
			{
				context.LogWarning($"expected target instance with net id {netId} to be type UseableGun, but was {voidNetObj.GetType().Name}");
				return;
			}
			netObj.ReceivePlayAimStop();
		}
		[NetInvokableGeneratedMethod(nameof(UseableGun.ReceivePlayAimStop), ENetInvokableGeneratedMethodPurpose.Write)]
		public static void ReceivePlayAimStop_Write(NetPakWriter writer)
		{
		}
	}
}
