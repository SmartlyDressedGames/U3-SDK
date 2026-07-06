using SDG.NetPak;
namespace SDG.Unturned
{
	public static class EGameMode_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out EGameMode value)
		{
			uint index;
			bool result = reader.ReadBits(3, out index);
			// Casting out of range index to enum would throw an exception.
			if (index <= 4)
			{
				value = (EGameMode) index;
				return result;
			}
			else
			{
#if WITH_NETPAK_EXCEPTIONS
				throw new System.IndexOutOfRangeException();
#else
				value = default;
				return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}
		}
		public static bool WriteEnum(this NetPakWriter writer, EGameMode value)
		{
			uint index = (uint) value;
#if WITH_NETPAK_EXCEPTIONS
			if (index > 4)
				throw new System.IndexOutOfRangeException();
#endif // WITH_NETPAK_EXCEPTIONS
			return writer.WriteBits(index, 3);
		}
	}
}
