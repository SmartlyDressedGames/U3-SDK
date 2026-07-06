////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.NetPak
{
	/// <summary>
	/// Unpacks bits from a byte array into a 32-bit buffer value. GafferOnGames recommends this approach rather than
	/// "farting across a buffer at byte level like it's 1985".
	/// </summary>
	public class NetPakReader
	{
		private byte[] buffer;
		private ulong scratch;
		private int bufferLength;
		public int readByteIndex;
		public int scratchBitCount;

		/// <summary>
		/// Lightweight error when exceptions are disabled. Bitwise OR to prevent different errors from clobbering each other. 
		/// </summary>
		[System.Flags]
		public enum EErrorFlags
		{
			None = 0,

			/// <summary>
			/// Call to ReadBits or ReadBytes would have overflowed our buffer.
			/// </summary>
			SourceBufferOverflow = 1 << 0,

			/// <summary>
			/// Buffer passed into ReadBytes would have overflowed.
			/// </summary>
			DestinationBufferOverflow = 1 << 1,

			/// <summary>
			/// AlignToByte bits should be zero.
			/// </summary>
			AlignmentPadding = 1 << 2,

			/// <summary>
			/// Buffer passed into SaveState would have overflowed.
			/// </summary>
			SaveStateBufferOverflow = 1 << 3,
		}
		public EErrorFlags errors;

		/// <summary>
		/// Imprecise because sent byte length is rounded up from bit length, but should help find particularly
		/// egregious reading errors.
		/// </summary>
		public bool ReachedEndOfSegment => readByteIndex == bufferLength;

		/// <summary>
		/// Number of bytes until end of segment is reached.
		/// </summary>
		public int RemainingSegmentLength => bufferLength - readByteIndex;

		/// <summary>
		/// Save remaining data to resume reading later. Used by net invokables to defer invocation.
		/// </summary>
		public bool SaveState(out uint scratch, out int scratchBitCount, byte[] buffer)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (buffer == null)
				throw new System.ArgumentNullException(nameof(buffer));

			if (this.scratch > uint.MaxValue || this.scratchBitCount > 32)
				throw new System.Exception($"Scratch {this.scratch} ({this.scratchBitCount} bits) outside uint32 range");
#endif // WITH_NETPAK_EXCEPTIONS

			long sourceBytesToCopy = RemainingSegmentLength;
			if (sourceBytesToCopy > buffer.Length)
			{
#if WITH_NETPAK_EXCEPTIONS
				throw new System.Exception("Would overrun buffer");
#else
				scratch = 0;
				scratchBitCount = 0;
				return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}

			scratch = (uint) this.scratch;
			scratchBitCount = this.scratchBitCount;

			if (sourceBytesToCopy > 0)
			{
				unsafe
				{
					fixed (byte* bufferPtr = this.buffer)
					fixed (byte* destinationPtr = buffer)
					{
						byte* sourcePtr = bufferPtr + readByteIndex;
						long destinationSizeInBytes = buffer.LongLength;
						System.Buffer.MemoryCopy(sourcePtr, destinationPtr, destinationSizeInBytes, sourceBytesToCopy);
					}
				}
			}

			readByteIndex = bufferLength; // Move to end to avoid ReachedEndOfSegment warning.
			return true;
		}

		public void LoadState(uint scratch, int scratchBitCount, byte[] buffer, int bufferLength)
		{
			this.scratch = scratch;
			readByteIndex = 0;
			this.scratchBitCount = scratchBitCount;
			errors = EErrorFlags.None;
			this.buffer = buffer;
			this.bufferLength = bufferLength;
		}

		public void Reset()
		{
			scratch = 0;
			readByteIndex = 0;
			scratchBitCount = 0;
			errors = EErrorFlags.None;
		}

		/// <summary>
		/// Used by invocation messages to show more error context rather than the default.
		/// </summary>
		public void ResetErrors()
		{
			errors = EErrorFlags.None;
			readByteIndex = bufferLength; // ReachedEndOfSegment
		}

		public int GetBufferSegmentLength()
		{
			return bufferLength;
		}

		public void SetBuffer(byte[] buffer)
		{
			this.buffer = buffer;
			bufferLength = buffer.Length;
		}

		public void SetBufferSegment(byte[] buffer, int bufferLength)
		{
			this.buffer = buffer;
			this.bufferLength = bufferLength;
		}

		/// <summary>
		/// Used by NetInvokable loopback to copy buffer from writer to reader.
		/// </summary>
		public void SetBufferSegmentCopy(byte[] sourceBuffer, byte[] destinationBuffer, int bufferLength)
		{
			buffer = destinationBuffer;
			this.bufferLength = bufferLength;

			unsafe
			{
				fixed (byte* sourcePtr = sourceBuffer)
				fixed (byte* destinationPtr = destinationBuffer)
				{
					System.Buffer.MemoryCopy(sourcePtr, destinationPtr, destinationBuffer.Length, bufferLength);
				}
			}
		}

		public bool ReadBit(out bool value)
		{
			// Unlike the writer, reading has no meaningful performance gain from a special implementation.
			uint bit;
			bool result = ReadBits(1, out bit);
			value = bit == 1;
			return result;
		}

		public bool ReadBits(int valueBitCount, out uint value)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (valueBitCount < 0 || valueBitCount > 32)
				throw new System.ArgumentOutOfRangeException("valueBitCount");
#endif // WITH_NETPAK_EXCEPTIONS

			if (valueBitCount > scratchBitCount)
			{
				int availableBytes = bufferLength - readByteIndex;

				ulong readWord;
				switch (availableBytes)
				{
					case 0:
						value = 0;
#if WITH_NETPAK_EXCEPTIONS
						throw new System.Exception("Already at the end of stream");
#else
						errors |= EErrorFlags.SourceBufferOverflow;
						return false;
#endif // WITH_NETPAK_EXCEPTIONS

					case 1:
						readWord = buffer[readByteIndex];
						break;

					case 2:
						readWord = buffer[readByteIndex]
							| (((ulong) buffer[readByteIndex + 1]) << 8);
						break;

					case 3:
						readWord = buffer[readByteIndex]
							| (((ulong) buffer[readByteIndex + 1]) << 8)
							| (((ulong) buffer[readByteIndex + 2]) << 16);
						break;

					case 4:
						readWord = buffer[readByteIndex]
							| (((ulong) buffer[readByteIndex + 1]) << 8)
							| (((ulong) buffer[readByteIndex + 2]) << 16)
							| (((ulong) buffer[readByteIndex + 3]) << 24);
						break;

					default: // >4
						availableBytes = 4;
						readWord = buffer[readByteIndex]
							| (((ulong) buffer[readByteIndex + 1]) << 8)
							| (((ulong) buffer[readByteIndex + 2]) << 16)
							| (((ulong) buffer[readByteIndex + 3]) << 24);
						break;
				}

				readWord <<= scratchBitCount;
				scratch |= readWord;
				scratchBitCount += availableBytes * 8;
				readByteIndex += availableBytes;

				if (valueBitCount > scratchBitCount)
				{
#if WITH_NETPAK_EXCEPTIONS
					throw new System.Exception("Read past end of stream");
#else
					value = 0;
					errors |= EErrorFlags.SourceBufferOverflow;
					return false;
#endif // WITH_NETPAK_EXCEPTIONS
				}
			}

			ulong mask = (1UL << valueBitCount) - 1;
			value = (uint) (scratch & mask);
			scratch >>= valueBitCount;
			scratchBitCount -= valueBitCount;
			return true;
		}

		public bool AlignToByte()
		{
			int bitAlignment = scratchBitCount % 8; // e.g. 30 % 8 = 6
			if (bitAlignment != 0)
			{
				uint value;
				bool result = ReadBits(bitAlignment, out value) && value == 0;
#if WITH_NETPAK_EXCEPTIONS
				if(!result)
				{
					throw new System.Exception("Padding was not zero: " + value);
				}
				if (scratchBitCount != 0 && scratchBitCount != 8 && scratchBitCount != 16 && scratchBitCount != 24)
				{
					// Should not be 32 either because getting the padding bits should not trigger a read.
					throw new System.Exception(string.Format("Bit count ({0}) should have been aligned to byte", scratchBitCount));
				}
#endif // WITH_NETPAK_EXCEPTIONS
				errors |= value != 0 ? EErrorFlags.AlignmentPadding : 0;
				return result;
			}
			else
			{
				// Already aligned.
				return true;
			}
		}

		/// <summary>
		/// Assumes length is greater than zero!
		/// Moves reader forward according to length.
		/// </summary>
		public bool ReadBytesPtr(int length, out byte[] source, out int bufferOffset)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (length < 1)
				throw new System.ArgumentOutOfRangeException("length");
#endif // WITH_NETPAK_EXCEPTIONS

			if (!AlignToByte())
			{
				source = null;
				bufferOffset = 0;
				return false;
			}

			// Originally we shifted scratchBits to get the first few bytes, but that was a waste of time because the
			// original bytes are still available slightly backward in the buffer.
			int availableScratchBytes = scratchBitCount / 8;
			bufferOffset = readByteIndex - availableScratchBytes;

			if (bufferOffset + length > bufferLength)
			{
#if WITH_NETPAK_EXCEPTIONS
				throw new System.Exception("Would overrun buffer");
#else
				source = null;
				errors |= EErrorFlags.SourceBufferOverflow;
				return false;
#endif // WITH_NETPAK_EXCEPTIONS
			}

			if (length >= availableScratchBytes)
			{
				readByteIndex = bufferOffset + length;
				scratch = 0;
				scratchBitCount = 0;
			}
			else
			{
				int lengthBitCount = length * 8;
				scratch >>= lengthBitCount;
				scratchBitCount -= lengthBitCount;
			}

			source = buffer;
			return true;
		}

		public bool ReadBytes(byte[] destination)
		{
			return ReadBytes(destination, destination.Length);
		}

		public bool ReadBytes(byte[] destination, int length)
		{
#if WITH_NETPAK_EXCEPTIONS
			if (destination == null)
				throw new System.ArgumentNullException(nameof(destination));
			if (length < 0 || length > destination.Length)
				throw new System.ArgumentOutOfRangeException(nameof(length));
#else
			if (length > destination.Length)
			{
				errors |= EErrorFlags.DestinationBufferOverflow;
				return false;
			}
#endif // WITH_NETPAK_EXCEPTIONS

			if (length < 1)
			{
				// Do not waste time/bits aligning to byte for zero length.
				return true;
			}

			byte[] sourceBuffer;
			int bufferOffset;
			if (ReadBytesPtr(length, out sourceBuffer, out bufferOffset))
			{
				unsafe
				{
					fixed (byte* bufferPtr = sourceBuffer)
					fixed (byte* destinationPtr = destination)
					{
						byte* sourcePtr = bufferPtr + bufferOffset;
						long destinationSizeInBytes = destination.LongLength;
						long sourceBytesToCopy = length;
						System.Buffer.MemoryCopy(sourcePtr, destinationPtr, destinationSizeInBytes, sourceBytesToCopy);
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
