using SDG.NetPak;
namespace SDG.Unturned
{
	public static class ERaycastInfoUsage_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out ERaycastInfoUsage value)
		{
			uint index;
			bool result = reader.ReadBits(4, out index);
			value = (ERaycastInfoUsage) index;
			return result;
		}
		public static bool WriteEnum(this NetPakWriter writer, ERaycastInfoUsage value)
		{
			uint index = (uint) value;
#if WITH_NETPAK_EXCEPTIONS
			if (index > 15)
				throw new System.IndexOutOfRangeException();
#endif // WITH_NETPAK_EXCEPTIONS
			return writer.WriteBits(index, 4);
		}
	}
}
