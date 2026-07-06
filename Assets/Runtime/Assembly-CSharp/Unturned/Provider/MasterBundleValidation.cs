////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned
{
	/// <summary>
	/// Hashes for Windows, Linux, and Mac asset bundles.
	/// Only loaded on the dedicated server. Null otherwise.
	/// </summary>
	internal class MasterBundleHash
	{
		public byte[] windowsHash;
		public byte[] macHash;
		public byte[] linuxHash;

		public byte[] GetPlatformHash(EClientPlatform clientPlatform)
		{
			switch (clientPlatform)
			{
				default:
					return null;

				case EClientPlatform.Windows:
					return windowsHash;

				case EClientPlatform.Mac:
					return macHash;

				case EClientPlatform.Linux:
					return linuxHash;
			}
		}

		/// <summary>
		/// Does given hash match any of the platform hashes?
		/// </summary>
		public bool DoesAnyHashMatch(byte[] hash)
		{
			if (windowsHash == null || macHash == null || linuxHash == null)
			{
				// Should not happen anyway, but in this case the hashes were not available.
				return true;
			}

			return Hash.verifyHash(hash, windowsHash) || Hash.verifyHash(hash, macHash) || Hash.verifyHash(hash, linuxHash);
		}

		public bool DoesPlatformHashMatch(byte[] hash, EClientPlatform clientPlatform)
		{
			byte[] platformHash = GetPlatformHash(clientPlatform);
			if (platformHash == null)
			{
				// Should not happen anyway, but in this case the hashes were not available.
				return true;
			}
			else
			{
				return Hash.verifyHash(hash, platformHash);
			}
		}
	}

	/// <summary>
	/// Compares client asset bundle hash with server known hashes.
	/// </summary>
	internal static class MasterBundleValidation
	{
		/// <summary>
		/// Called by asset startup to cache which bundles are eligible for hashing.
		/// </summary>
		public static void initialize(List<MasterBundleConfig> allMasterBundles)
		{
			if (!Dedicator.IsDedicatedServer)
			{
				throw new System.NotSupportedException("MasterBundleValidation should only be used on dedicated server!");
			}

			foreach (MasterBundleConfig bundle in allMasterBundles)
			{
				if (!bundle.doesHashFileExist)
				{
					UnturnedLog.info($"Asset bundle \"{bundle.assetBundleNameWithoutExtension}\" does not have server hashes file");
					continue;
				}

				bundle.serverHashes = loadHashForBundle(bundle);
				if (bundle.serverHashes != null)
				{
					// Validity check that the loaded asset bundle matches one of the hashes in the file.
					if (!bundle.serverHashes.DoesAnyHashMatch(bundle.hash))
					{
						bundle.serverHashes = null;
						UnturnedLog.warn("Master bundle hash file does not match loaded: {0}", bundle.assetBundleName);
					}
				}
			}
		}

		private static MasterBundleHash loadHashForBundle(MasterBundleConfig bundle)
		{
			if (bundle.sourceConfig != null)
			{
				bundle = bundle.sourceConfig;
			}

			string filePath = bundle.getHashFilePath();
			if (!File.Exists(filePath))
			{
				// We checked that it exists at startup, so it got deleted?
				UnturnedLog.warn("Master bundle hashes file was deleted: {0}", filePath);
				return null;
			}

			byte[] hashBytes = File.ReadAllBytes(filePath);
			if (hashBytes.Length < 1)
			{
				UnturnedLog.warn("Master bundle hashes file is empty: {0}", filePath);
				return null;
			}

			int version;
			if (hashBytes.Length == 60)
			{
				version = 1;
			}
			else
			{
				version = hashBytes[0];

				if (version != 2)
				{
					UnturnedLog.warn("Master bundle hash file is an unknown version ({0}): {1}", version, filePath);
					return null;
				}

				if (hashBytes.Length != 61)
				{
					UnturnedLog.warn("Master bundle hash file is the wrong size ({0}): {1}", hashBytes.Length, filePath);
					return null;
				}
			}

			MasterBundleHash container = new MasterBundleHash();
			container.windowsHash = new byte[20];
			container.macHash = new byte[20];
			container.linuxHash = new byte[20];

			int hashBytesOffset = 0;
			if (version > 1)
			{
				hashBytesOffset = 1;
			}
			System.Array.Copy(hashBytes, hashBytesOffset, container.windowsHash, 0, 20);
			hashBytesOffset += 20;
			System.Array.Copy(hashBytes, hashBytesOffset, container.linuxHash, 0, 20);
			hashBytesOffset += 20;
			System.Array.Copy(hashBytes, hashBytesOffset, container.macHash, 0, 20);

			return container;
		}
	}
}
