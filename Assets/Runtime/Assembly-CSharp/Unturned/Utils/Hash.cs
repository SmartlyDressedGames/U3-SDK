////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SDG.Unturned
{
	public class Hash
	{
		private static SHA1CryptoServiceProvider service = new SHA1CryptoServiceProvider();

		public static byte[] SHA1(byte[] bytes)
		{
			return service.ComputeHash(bytes);
		}

		public static byte[] SHA1(Stream stream)
		{
			return service.ComputeHash(stream);
		}

		public static byte[] SHA1(string text)
		{
			// Nelson 2024-04-30: Looks like UTF8.GetBytes throws an exception if input string is null, so I'm going
			// through and ensuring we never pass null to it.
			if (text == null)
			{
				text = string.Empty;
			}
			return SHA1(Encoding.UTF8.GetBytes(text));
		}

		public static byte[] SHA1(CSteamID steamID)
		{
			return SHA1(System.BitConverter.GetBytes(steamID.m_SteamID));
		}

		public static bool verifyHash(byte[] hash_0, byte[] hash_1)
		{
			if (hash_0.Length != 20 || hash_1.Length != 20)
			{
				return false;
			}

			for (int index = 0; index < hash_0.Length; index++)
			{
				if (hash_0[index] != hash_1[index])
				{
					return false;
				}
			}

			return true;
		}

		private static byte[] _40bytes = new byte[40];
		/// <summary>
		/// Combine two existing 20-byte hashes.
		/// </summary>
		public static byte[] combineSHA1Hashes(byte[] a, byte[] b)
		{
			if (a.Length != 20 || b.Length != 20)
				throw new Exception("both lengths should be 20");

			a.CopyTo(_40bytes, 0);
			b.CopyTo(_40bytes, 20);
			return SHA1(_40bytes);
		}

		public static byte[] combine(params byte[][] hashes)
		{
			byte[] bytes = new byte[hashes.Length * 20];

			for (int index = 0; index < hashes.Length; index++)
			{
				byte[] hash = hashes[index];

				hash.CopyTo(bytes, index * 20);
			}

			return SHA1(bytes);
		}

		public static byte[] combine(List<byte[]> hashes)
		{
			byte[] bytes = new byte[hashes.Count * 20];

			for (int index = 0; index < hashes.Count; index++)
			{
				byte[] hash = hashes[index];

				hash.CopyTo(bytes, index * 20);
			}

			return SHA1(bytes);
		}

		public static string toString(byte[] hash)
		{
			if (hash == null)
			{
				return "null";
			}
			else
			{
				string text = "";
				for (int index = 0; index < hash.Length; index++)
				{
					text += hash[index].ToString("X2");
				}
				return text;
			}
		}

		internal static string ToCodeString(byte[] hash)
		{
			StringBuilder sb = new StringBuilder(hash.Length * 6);

			sb.Append("0x");
			sb.Append(hash[0].ToString("X2"));

			for (int index = 1; index < hash.Length; ++index)
			{
				sb.Append(", 0x");
				sb.Append(hash[index].ToString("X2"));
			}

			return sb.ToString();
		}

		public static void log(byte[] hash)
		{
			if (hash == null || hash.Length != 20)
			{
				return;
			}

			string text = toString(hash);
			CommandWindow.Log(text);
		}
	}

	/// <summary>
	/// Utility to hash a stream of bytes over several frames.
	/// </summary>
	public class TimeSliceHash<T> : IDisposable where T : HashAlgorithm, new()
	{
		public TimeSliceHash(Stream stream)
		{
			algo = new T();
			algo.Initialize();
			this.stream = stream;
			buffer = new byte[8192];
		}

		/// <summary>
		/// [0, 1] percentage progress through the stream.
		/// </summary>
		public float progress => (float) (stream.Position / (double) stream.Length);

		/// <summary>
		/// Advance 1MB further into the stream.
		/// </summary>
		/// <returns>True if there is more data, false if complete.</returns>
		public bool advance()
		{
			for (int iter = 0; iter < 122; ++iter) // 122 * 8192 = 1MB
			{
				int bytesRead = stream.Read(buffer, 0, buffer.Length);
				if (bytesRead > 0)
				{
					algo.TransformBlock(buffer, 0, bytesRead, buffer, 0);
				}
				else
				{
					algo.TransformFinalBlock(buffer, 0, 0);
					return false;
				}
			}

			return true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				algo.Dispose();
			}

			disposed = true;
		}

		/// <summary>
		/// Get the computed hash after processing stream.
		/// </summary>
		public byte[] computeHash()
		{
			return algo.Hash;
		}

		private T algo;
		private Stream stream;
		private byte[] buffer;
		private bool disposed;
	}
}
