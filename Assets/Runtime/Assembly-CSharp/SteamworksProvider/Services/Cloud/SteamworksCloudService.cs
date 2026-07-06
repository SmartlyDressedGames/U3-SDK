////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider.Services;
using SDG.Provider.Services.Cloud;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Cloud
{
	public class SteamworksCloudService : Service, ICloudService
	{
		public bool read(string path, byte[] data)
		{
			if (path == null)
			{
				throw new System.ArgumentNullException("path");
			}

			if (data == null)
			{
				throw new System.ArgumentNullException("data");
			}

			int size = SteamRemoteStorage.GetFileSize(path);

			if (data.Length < size)
			{
				return false;
			}

			int read = SteamRemoteStorage.FileRead(path, data, size);

			if (read != size)
			{
				return false;
			}

			return true;
		}

		public bool write(string path, byte[] data, int size)
		{
			if (path == null)
			{
				throw new System.ArgumentNullException("path");
			}

			if (data == null)
			{
				throw new System.ArgumentNullException("data");
			}

			return SteamRemoteStorage.FileWrite(path, data, size);
		}

		public bool getSize(string path, out int size)
		{
			if (path == null)
			{
				throw new System.ArgumentNullException("path");
			}

			size = SteamRemoteStorage.GetFileSize(path);
			return true;
		}

		public bool exists(string path, out bool exists)
		{
			if (path == null)
			{
				throw new System.ArgumentNullException("path");
			}

			exists = SteamRemoteStorage.FileExists(path);
			return true;
		}

		public bool delete(string path)
		{
			if (path == null)
			{
				throw new System.ArgumentNullException("path");
			}

			return SteamRemoteStorage.FileDelete(path);
		}
	}
}