using SDG.NetPak;
namespace SDG.Unturned
{
	public static class EArenaMessage_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out EArenaMessage value)
		{
			uint index;
			bool result = reader.ReadBits(3, out index);
			value = (EArenaMessage) index;
			return result;
		}
		public static bool WriteEnum(this NetPakWriter writer, EArenaMessage value)
		{
			uint index = (uint) value;
#if WITH_NETPAK_EXCEPTIONS
			if (index > 7)
				throw new System.IndexOutOfRangeException();
#endif // WITH_NETPAK_EXCEPTIONS
			return writer.WriteBits(index, 3);
		}
	}
}
