////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEDICATED_SERVER
namespace SDG.Unturned
{
	/// <summary>
	/// Used by the server to validate client Unity player files (assemblies and resources).
	/// </summary>
	public class PlayerHashValidation
	{
		internal static bool IsAssemblyHashValid(byte[] hash, EClientPlatform clientPlatform)
		{
			if (!hasLoadedHashes)
			{
				hasLoadedHashes = true;
				LoadHashes();
			}

			if (!areHashesAvailable)
			{
				return true;
			}

			switch (clientPlatform)
			{
				case EClientPlatform.Windows:
					return Hash.verifyHash(hash, winAssemblyHash);

				case EClientPlatform.Mac:
					return Hash.verifyHash(hash, macAssemblyHash);

				case EClientPlatform.Linux:
					return Hash.verifyHash(hash, linuxAssemblyHash);
			}

#if UNITY_EDITOR
			return true; // fixes joining editor from test build
#elif DEVELOPMENT_BUILD
			return bypassAssemblyHash;
#else // !DEVELOPMENT_BUILD
			return false;
#endif // !DEVELOPMENT_BUILD
		}

		internal static bool IsResourcesHashValid(byte[] hash, EClientPlatform clientPlatform)
		{
			if (!hasLoadedHashes)
			{
				hasLoadedHashes = true;
				LoadHashes();
			}

			if (!areHashesAvailable)
			{
				return true;
			}

			switch (clientPlatform)
			{
				case EClientPlatform.Windows:
					return Hash.verifyHash(hash, winResourcesHash);

				case EClientPlatform.Mac:
					return Hash.verifyHash(hash, macResourcesHash);

				case EClientPlatform.Linux:
					return Hash.verifyHash(hash, linuxResourcesHash);
			}

#if UNITY_EDITOR
			return true; // fixes joining editor from test build
#elif DEVELOPMENT_BUILD
			return bypassResourcesHash;
#else // !DEVELOPMENT_BUILD
			return false;
#endif // !DEVELOPMENT_BUILD
		}

		private static void LoadHashes()
		{
			try
			{
				Block applog = ReadWrite.readBlock("/Extras/Sources/Animation/appout.log", false, 0);
				winAssemblyHash = applog.readByteArray();
				macAssemblyHash = applog.readByteArray();
				linuxAssemblyHash = applog.readByteArray();
				winResourcesHash = applog.readByteArray();
				macResourcesHash = applog.readByteArray();
				linuxResourcesHash = applog.readByteArray();
				areHashesAvailable = true;
			}
			catch (System.Exception exception)
			{
				UnturnedLog.exception(exception, "Caught exception loading Unity player hashes:");
				areHashesAvailable = false;
			}
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		/// <summary>
		/// Should players be allowed to join this server regardless of whether their DLL hash matches ours?
		/// Useful to allow players to join debug mode servers.
		/// </summary>
		private static CommandLineFlag bypassAssemblyHash = new CommandLineFlag(false, "-BypassAssemblyHash");

		/// <summary>
		/// Should players be allowed to join this server regardless of whether their resources hash matches ours?
		/// Useful to allow players to join debug mode servers.
		/// </summary>
		private static CommandLineFlag bypassResourcesHash = new CommandLineFlag(false, "-BypassResourcesHash");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		private static bool hasLoadedHashes = false;
		private static bool areHashesAvailable = false;
		private static byte[] winAssemblyHash;
		private static byte[] macAssemblyHash;
		private static byte[] linuxAssemblyHash;
		private static byte[] winResourcesHash;
		private static byte[] macResourcesHash;
		private static byte[] linuxResourcesHash;
	}
}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEDICATED_SERVER
