using SDG.NetPak;
namespace SDG.Unturned
{
	public static class EPlayerGesture_NetEnum
	{
		public static bool ReadEnum(this NetPakReader reader, out EPlayerGesture value)
		{
			uint index;
			bool result = reader.ReadBits(5, out index);
			// Casting out of range index to enum would throw an exception.
			if (index <= 17)
			{
				value = (EPlayerGesture) index;
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
		public static bool WriteEnum(this NetPakWriter writer, EPlayerGesture value)
		{
			uint index = (uint) value;
#if WITH_NETPAK_EXCEPTIONS
			if (index > 17)
				throw new System.IndexOutOfRangeException();
#endif // WITH_NETPAK_EXCEPTIONS
			return writer.WriteBits(index, 5);
		}
	}
}
