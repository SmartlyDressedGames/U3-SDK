////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Security.Cryptography;

namespace SDG.Unturned
{
	/// <summary>
	/// Run hash algorithm for all data passing through a stream.
	/// </summary>
	public class HashStream : Stream
	{
		public HashStream(Stream underlyingStream, HashAlgorithm hashAlgo)
		{
			this.underlyingStream = underlyingStream;
			this.hashAlgo = hashAlgo;
		}

		public byte[] Hash
		{
			get
			{
				hashAlgo.TransformFinalBlock(new byte[0], 0, 0);
				return hashAlgo.Hash;
			}
		}

		public override bool CanRead => underlyingStream.CanRead;

		public override bool CanSeek => underlyingStream.CanSeek;

		public override bool CanWrite => underlyingStream.CanWrite;

		public override long Length => underlyingStream.Length;

		public override long Position
		{
			get => underlyingStream.Position;
			set
			{
				if (value == 0)
				{
					hashAlgo.Initialize();
				}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				else
				{
					UnityEngine.Debug.LogErrorFormat("Unsupported hash stream position change? {0}", value);
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				underlyingStream.Position = value;
			}
		}

		public override void Flush()
		{
			underlyingStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int numBytesRead = underlyingStream.Read(buffer, offset, count);
			hashAlgo.TransformBlock(buffer, offset, numBytesRead, buffer, offset);
			return numBytesRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			// Unity asset bundle seems to seek to the beginning of the stream after reading partial data, so we reset the hash algorithm.
			if (origin == SeekOrigin.Begin && offset == 0)
			{
				hashAlgo.Initialize();
			}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			else
			{
				bool seekingToCurrentPosition = origin == SeekOrigin.Begin && offset == Position;
				if (!seekingToCurrentPosition)
				{
					UnityEngine.Debug.LogErrorFormat("Unsupported hash stream seek from {0} to {1} ({2})", Position, offset, origin);
				}
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			return underlyingStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			underlyingStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			underlyingStream.Write(buffer, offset, count);
			hashAlgo.TransformBlock(buffer, offset, count, buffer, offset);
		}

		private Stream underlyingStream;
		private HashAlgorithm hashAlgo;
	}

	public class SHA1Stream : HashStream
	{
		public SHA1Stream(Stream underlyingStream) : base(underlyingStream, new SHA1Managed())
		{ }
	}
}
