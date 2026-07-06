////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetPak
{
	public struct NetLength
	{
		public NetLength(uint valueInclusive)
		{
			value = valueInclusive;
			bitCount = NetPakConst.CountBits(value);
		}

		public uint Clamp(uint otherValue)
		{
			return otherValue > value ? value : otherValue;
		}

		public uint Clamp(int otherValue)
		{
			return otherValue > value ? value : (uint) otherValue;
		}

		public uint value;
		public int bitCount;
	}
}
