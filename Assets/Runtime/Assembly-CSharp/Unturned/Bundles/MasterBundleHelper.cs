////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace SDG.Unturned
{
	public static class MasterBundleHelper
	{
		public static string getConfigPath(string absoluteDirectory)
		{
			return absoluteDirectory + "/MasterBundle.dat";
		}

		public static bool containsMasterBundle(string absoluteDirectory)
		{
			return File.Exists(getConfigPath(absoluteDirectory));
		}

		/// <summary>
		/// Append suffix to name, or if name contains a '.' insert it before.
		/// </summary>
		public static string insertAssetBundleNameSuffix(string name, string suffix)
		{
			int index = name.IndexOf('.');
			if (index < 0)
			{
				return name + suffix;
			}
			else
			{
				return name.Insert(index, suffix);
			}
		}

		public static string getLinuxAssetBundleName(string name)
		{
			return insertAssetBundleNameSuffix(name, "_linux");
		}

		public static string getMacAssetBundleName(string name)
		{
			return insertAssetBundleNameSuffix(name, "_mac");
		}

		public static string getHashFileName(string name)
		{
			return name + ".hash";
		}
	}
}
