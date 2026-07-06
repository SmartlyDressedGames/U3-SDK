using SDG.NetPak;
namespace SDG.Unturned
{
	public static class EPlayerStance_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out EPlayerStance value)
		{
			uint index;
			bool result = reader.ReadBits(3, out index);
			value = (EPlayerStance) index;
			return result;
		}
		public static bool WriteEnum(this NetPakWriter writer, EPlayerStance value)
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
