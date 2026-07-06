////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned.Tools
{
	public static partial class EditorAssetBundleHelper
	{
		/// <summary>
		/// Copy manifest along with asset bundle.
		/// </summary>
		private static void CopyAssetBundle(string sourceFilePath, string destDirectoryPath)
		{
			Directory.CreateDirectory(destDirectoryPath);

			if (File.Exists(sourceFilePath))
			{
				string sourceFileName = Path.GetFileName(sourceFilePath);
				string destFilePath = Path.Combine(destDirectoryPath, sourceFileName);

				const bool overwrite = true;
				File.Copy(sourceFilePath, destFilePath, overwrite);
				File.Copy(sourceFilePath + ".manifest", destFilePath + ".manifest", overwrite);
			}
		}

		/// <summary>
		/// Callback when core.masterbundle is exported.
		/// </summary>
		public static void PostBuildCoreMasterBundle(string sourcePath)
		{
			string winPath = Path.Combine(sourcePath, "core.masterbundle");
			string macPath = Path.Combine(sourcePath, "core_mac.masterbundle");
			string linuxPath = Path.Combine(sourcePath, "core_linux.masterbundle");

			string buildsPath = PathEx.Join(UnityPaths.ProjectDirectory, "Builds");
			CopyAssetBundle(winPath, Path.Combine(buildsPath, "Windows32", "Bundles"));
			CopyAssetBundle(winPath, Path.Combine(buildsPath, "Windows64", "Bundles"));
			CopyAssetBundle(winPath, Path.Combine(buildsPath, "Windows64_Headless", "Bundles"));
			CopyAssetBundle(macPath, Path.Combine(buildsPath, "OSX64", "Bundles"));
			CopyAssetBundle(linuxPath, Path.Combine(buildsPath, "Linux64", "Bundles"));
			CopyAssetBundle(linuxPath, Path.Combine(buildsPath, "Linux64_Headless", "Bundles"));

			// Originally we deleted the source file after copying, but that interferes with incremental builds because
			// Unity uses the manifests to calculate what changed.

			string sourceHashPath = Path.Combine(sourcePath, "core.masterbundle.hash");
			if (File.Exists(sourceHashPath))
			{
				string destHashPath = Path.Combine(buildsPath, "Shared", "Bundles", "core.masterbundle.hash");
				File.Copy(sourceHashPath, destHashPath, /*overwrite*/ true);
			}
		}
	}
}
