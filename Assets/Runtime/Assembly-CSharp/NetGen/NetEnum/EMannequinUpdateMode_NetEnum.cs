using SDG.NetPak;
namespace SDG.Unturned
{
	public static class EMannequinUpdateMode_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out EMannequinUpdateMode value)
		{
			uint index;
			bool result = reader.ReadBits(2, out index);
			value = (EMannequinUpdateMode) index;
			return result;
		}
		public static bool WriteEnum(this NetPakWriter writer, EMannequinUpdateMode value)
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
