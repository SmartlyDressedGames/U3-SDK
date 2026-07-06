using SDG.NetPak;
namespace SDG.Unturned
{
	public static class EFiremode_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out EFiremode value)
		{
			uint index;
			bool result = reader.ReadBits(2, out index);
			value = (EFiremode) index;
			return result;
		}
		public static bool WriteEnum(this NetPakWriter writer, EFiremode value)
		{
			uint index = (uint) value;
#if WITH_NETPAK_EXCEPTIONS
			if (index > 3)
				throw new System.IndexOutOfRangeException();
#endif // WITH_NETPAK_EXCEPTIONS
			return writer.WriteBits(index, 2);
		}
	}
}
