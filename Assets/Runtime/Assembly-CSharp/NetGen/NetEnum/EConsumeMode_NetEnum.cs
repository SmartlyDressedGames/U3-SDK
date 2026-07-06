using SDG.NetPak;
namespace SDG.Unturned
{
	public static class EConsumeMode_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out EConsumeMode value)
		{
			uint index;
			bool result = reader.ReadBits(1, out index);
			value = (EConsumeMode) index;
			return result;
		}
		public static bool WriteEnum(this NetPakWriter writer, EConsumeMode value)
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
