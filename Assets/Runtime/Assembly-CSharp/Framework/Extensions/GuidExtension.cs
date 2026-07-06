////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Runtime.InteropServices;

namespace System
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct GuidBuffer
	{
		public static readonly byte[] GUID_BUFFER = new byte[16];

		[FieldOffset(0)]
		private fixed ulong buffer[2];

		[FieldOffset(0)]
		public Guid GUID;

		public GuidBuffer(Guid GUID) : this()
		{
			this.GUID = GUID;
		}

		public void Read(byte[] source, int offset)
		{
			if (offset + 16 > source.Length)
			{
				throw new ArgumentException("Source buffer is too small!");
			}

			fixed (byte* sourcePointer = source)
			fixed (ulong* dest = buffer)
			{
				byte* sourcePointerOffset = sourcePointer + offset;
				ulong* src = (ulong*) sourcePointerOffset;

				dest[0] = src[0];
				dest[1] = src[1];
			}
		}

		public void Write(byte[] destination, int offset)
		{
			if (offset + 16 > destination.Length)
			{
				throw new ArgumentException("Destination buffer is too small!");
			}

			fixed (byte* destinationPointer = destination)
			fixed (ulong* src = buffer)
			{
				byte* destinationPointerOffset = destinationPointer + offset;
				ulong* dest = (ulong*) destinationPointerOffset;

				dest[0] = src[0];
				dest[1] = src[1];
			}
		}
	}
}
