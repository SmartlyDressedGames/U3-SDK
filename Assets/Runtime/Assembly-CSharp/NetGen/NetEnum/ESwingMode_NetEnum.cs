using SDG.NetPak;
namespace SDG.Unturned
{
	public static class ESwingMode_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out ESwingMode value)
		{
			uint index;
			bool result = reader.ReadBits(1, out index);
			value = (ESwingMode) index;
			return result;
		}
		public static bool WriteEnum(this NetPakWriter writer, ESwingMode value)
		{
			uint index = (uint) value;
#if WITH_NETPAK_EXCEPTIONS
			if (index > 1)
				throw new System.IndexOutOfRangeException();
#endif // WITH_NETPAK_EXCEPTIONS
			return writer.WriteBits(index, 1);
		}
	}
}
