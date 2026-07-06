////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Runtime.InteropServices;

namespace SDG.NetPak
{
	/// <summary>
	/// Packs bits into a 32-bit buffer value, and from there into a byte array. GafferOnGames recommends this approach
	/// rather than "farting across a buffer at byte level like it's 1985".
	/// </summary>
	public class NetPakWriter
	{
		public byte[] buffer;
		private ulong scratch;
		public int writeByteIndex;
		public int scratchBitCount;

		/// <summary>
		/// Lightweight error when exceptions are disabled. Bitwise OR to prevent different errors from clobbering each other. 
		/// </summary>
		[System.Flags]
		public enum EErrorFlags
		{
			None = 0,
			BufferOverflow = 1 << 0,
		}
		public EErrorFlags errors;

		public void Reset()
		{
			scratch = 0;
			writeByteIndex = 0;
			scratchBitCount = 0;
			errors = EErrorFlags.None;
		}

		public bool WriteBit(bool value)
		{
			if (value)
			{
				scratch |= 1UL << scratchBitCount;
			}
			++scratchBitCount;
			if (scratchBitCount >= 32)
			{
				return FlushLowBits();
			}
			else
			{
				return true;
			}
		}

		public bool WriteBits(uint value, int valueBitCount)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (valueBitCount < 0 || valueBitCount > 32)
				throw new System.ArgumentOutOfRangeException("valueBitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			ulong mask = (1UL << valueBitCount) - 1;
			scratch |= (value & mask) << scratchBitCount;
			scratchBitCount += valueBitCount;
			if (scratchBitCount >= 32)
			{
				return FlushLowBits();
			}
			else
			{
				return true;
			}
		}

		public bool Flush()
		{
			if (scratchBitCount < 1)
			{
				// Nothing to flush.
				return true;
			}

			int bytesNeeded = ((scratchBitCount - 1) / 8) + 1; // Round up division by eight.
			int bytesAvailable = buffer.Length - writeByteIndex;
			if (bytesNeeded > bytesAvailable)
			{
#if WITH_NETPAK_EXCEPTIONS
				throw new System.Exception("Wrote past end of buffer");
#else
				errors |= EErrorFlags.BufferOverflow;
				return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}

			switch (bytesNeeded)
			{
#if WITH_NETPAK_EXCEPTIONS
				default:
				case 0:
					throw new System.Exception("bytesNeeded should not be zero");
#endif // WITH_NETPAK_EXCEPTIONS
				case 1:
					buffer[writeByteIndex] = (byte) scratch;
					break;

				case 2:
					buffer[writeByteIndex] = (byte) scratch;
					buffer[writeByteIndex + 1] = (byte) (scratch >> 8);
					break;

				case 3:
					buffer[writeByteIndex] = (byte) scratch;
					buffer[writeByteIndex + 1] = (byte) (scratch >> 8);
					buffer[writeByteIndex + 2] = (byte) (scratch >> 16);
					break;

				case 4:
					buffer[writeByteIndex] = (byte) scratch;
					buffer[writeByteIndex + 1] = (byte) (scratch >> 8);
					buffer[writeByteIndex + 2] = (byte) (scratch >> 16);
					buffer[writeByteIndex + 3] = (byte) (scratch >> 24);
					break;
			}

			writeByteIndex += bytesNeeded;
			scratch = 0;
			scratchBitCount = 0;
			return true;
		}

		public bool AlignToByte()
		{
			int bitAlignment = scratchBitCount % 8; // e.g. 30 % 8 = 6
			if (bitAlignment != 0)
			{
				return WriteBits(0, 8 - bitAlignment);
			}
			else
			{
				// Already aligned.
				return true;
			}
		}

		public bool WriteBytes(byte[] bytes)
		{
			return WriteBytes(bytes, bytes.Length);
		}

		public bool WriteBytes(byte[] bytes, int length)
		{
			return WriteBytes(bytes, 0, length);
		}

		public bool WriteBytes(byte[] bytes, int offset, int length)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (bytes == null)
				throw new System.ArgumentNullException(nameof(bytes));
			if (offset < 0 || offset + length > bytes.Length)
				throw new System.ArgumentOutOfRangeException(nameof(length));
#endif // #if WITH_NETPAK_EXCEPTIONS

			if (length < 1)
			{
				// Do not waste time/bits aligning to byte for zero length.
				return true;
			}

			if (!AlignToByte())
				return false;

			if (!Flush())
				return false;

			if (writeByteIndex + length > buffer.Length)
			{
#if WITH_NETPAK_EXCEPTIONS
				throw new System.Exception("Would overrun buffer");
#else
				errors |= EErrorFlags.BufferOverflow;
				return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}

			unsafe
			{
				fixed (byte* bytesPtr = bytes)
				fixed (byte* bufferPtr = buffer)
				{
					byte* sourcePtr = bytesPtr + offset;
					byte* destinationPtr = bufferPtr + writeByteIndex;
					long destinationSizeInBytes = buffer.Length - writeByteIndex;
					System.Buffer.MemoryCopy(sourcePtr, destinationPtr, destinationSizeInBytes, length);
					writeByteIndex += length;
				}
			}

			return true;
		}

		public bool WriteBytes(System.IntPtr bytesPtr, int length)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (bytesPtr == System.IntPtr.Zero)
				throw new System.ArgumentNullException("bytes");
			if (length < 0)
				throw new System.ArgumentOutOfRangeException("length");
#endif // #if WITH_NETPAK_EXCEPTIONS

			if (length < 1)
			{
				// Do not waste time/bits aligning to byte for zero length.
				return true;
			}

			if (!AlignToByte())
				return false;

			if (!Flush())
				return false;

			if (writeByteIndex + length > buffer.Length)
			{
#if WITH_NETPAK_EXCEPTIONS
				throw new System.Exception("Would overrun buffer");
#else
				errors |= EErrorFlags.BufferOverflow;
				return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}

			Marshal.Copy(bytesPtr, buffer, writeByteIndex, length);
			writeByteIndex += length;

			return true;
		}

		private bool FlushLowBits()
		{
#if WITH_NETPAK_EXCEPTIONS
			if (scratchBitCount < 32)
				throw new System.Exception("Should only be called with a full 32-bits");
#endif // #if WITH_NETPAK_EXCEPTIONS

			int availableSpace = buffer.Length - writeByteIndex;
			if (availableSpace < 4)
			{
#if WITH_NETPAK_EXCEPTIONS
				throw new System.Exception("Wrote low bits past end of buffer");
#else
				errors |= EErrorFlags.BufferOverflow;
				return false;
#endif
			}

			buffer[writeByteIndex] = (byte) scratch;
			buffer[writeByteIndex + 1] = (byte) (scratch >> 8);
			buffer[writeByteIndex + 2] = (byte) (scratch >> 16);
			buffer[writeByteIndex + 3] = (byte) (scratch >> 24);
			writeByteIndex += 4;
			scratch >>= 32;
			scratchBitCount -= 32;
			return true;
		}
	}
}
