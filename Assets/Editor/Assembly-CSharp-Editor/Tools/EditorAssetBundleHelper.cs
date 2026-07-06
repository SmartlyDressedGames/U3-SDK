////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace SDG.Unturned.Tools
{
	public static partial class EditorAssetBundleHelper
	{
		public static string GetBuildTargetAssetBundleName(string assetBundleName, BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.StandaloneLinux64:
					return MasterBundleHelper.getLinuxAssetBundleName(assetBundleName);

				case BuildTarget.StandaloneOSX:
					return MasterBundleHelper.getMacAssetBundleName(assetBundleName);

				default:
					return assetBundleName;
			}
		}

		/// <summary>
		/// Build an asset bundle by name.
		/// </summary>
		/// <param name="assetBundleName">Name of an asset bundle registered in the editor.</param>
		/// <param name="outputPath">Absolute path to directory to contain built asset bundle.</param>
		/// <param name="multiplatform">Should mac and linux variants of asset bundle be built as well?</param>
		public static void Build(string assetBundleName, string outputPath, bool multiplatform)
		{
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
			if (assetPaths.Length < 1)
			{
				Debug.LogWarning("No assets in: " + assetBundleName);
				return;
			}

			// Saves some perf by disabling these unused loading options.
			// If changing remember to update the CI build process.
			// 2022-06-09: experimented with disabling compression to see if loading the asset bundle faster was worth
			//			   reading the larger file slower, but even on an SSD it was ~13% faster to enable compression.
			BuildAssetBundleOptions assetBundleOptions = BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

			BuildTarget[] targetPlatforms = new BuildTarget[3]
			{
				BuildTarget.StandaloneLinux64,
				BuildTarget.StandaloneOSX,
				BuildTarget.StandaloneWindows64,
			};

			BuildTarget editorBuildTarget;
			switch (Application.platform)
			{
				case RuntimePlatform.LinuxEditor:
					editorBuildTarget = BuildTarget.StandaloneLinux64;
					break;

				case RuntimePlatform.OSXEditor:
					editorBuildTarget = BuildTarget.StandaloneOSX;
					break;

				case RuntimePlatform.WindowsEditor:
					editorBuildTarget = BuildTarget.StandaloneWindows64;
					break;

				default:
					Debug.LogWarning("Unable to determine editor platform to restore to");
					return;
			}

			if (multiplatform)
			{
				foreach (BuildTarget target in targetPlatforms)
				{
					if (target == editorBuildTarget)
					{
						continue;
					}

					AssetBundleBuild[] builds = new AssetBundleBuild[1];
					builds[0].assetBundleName = GetBuildTargetAssetBundleName(assetBundleName, target);
					builds[0].assetNames = assetPaths;
					BuildPipeline.BuildAssetBundles(outputPath, builds, assetBundleOptions, target);
				}
			}

			AssetBundleBuild[] finalBuild = new AssetBundleBuild[1];
			finalBuild[0].assetBundleName = GetBuildTargetAssetBundleName(assetBundleName, editorBuildTarget);
			finalBuild[0].assetNames = assetPaths;
			BuildPipeline.BuildAssetBundles(outputPath, finalBuild, assetBundleOptions, editorBuildTarget);

			CleanupAfterBuildingAssetBundle(outputPath);
			HashAssetBundle(outputPath + '/' + assetBundleName);

#if GAME
			if (string.Equals(assetBundleName, "core.masterbundle"))
			{
				PostBuildCoreMasterBundle(outputPath);
			}
#endif
		}

		/// <summary>
		/// Unity (sometimes?) creates an empty bundle with the same name as the folder, so we delete it.
		/// </summary>
		public static void CleanupAfterBuildingAssetBundle(string outputPath)
		{
			string directoryName = Path.GetFileName(outputPath);
			string emptyBundlePath = Path.Combine(outputPath, directoryName);
			if (File.Exists(emptyBundlePath))
			{
				File.Delete(emptyBundlePath);
			}
			string emptyManifestPath = emptyBundlePath + ".manifest";
			if (File.Exists(emptyManifestPath))
			{
				File.Delete(emptyManifestPath);
			}
		}

		/// <summary>
		/// Combine per-platform hashes into a file for the server to load.
		/// </summary>
		public static void HashAssetBundle(string windowsFilePath)
		{
			string hashFilePath = MasterBundleHelper.getHashFileName(windowsFilePath);
			string linuxFilePath = MasterBundleHelper.getLinuxAssetBundleName(windowsFilePath);
			string macFilePath = MasterBundleHelper.getMacAssetBundleName(windowsFilePath);

			if (!File.Exists(linuxFilePath) || !File.Exists(macFilePath))
			{
				if (File.Exists(hashFilePath))
				{
					Debug.Log("Skipping hashing step and deleting existing hash file because multiplatform asset bundles have not been exported yet");
					File.Delete(hashFilePath);
				}
				else
				{
					Debug.Log("Skipping hashing step because multiplatform asset bundles have not been exported yet");
				}
				return;
			}

			SHA1CryptoServiceProvider hashAlgo = new SHA1CryptoServiceProvider();
			byte[] windowsHash = hashAlgo.ComputeHash(File.ReadAllBytes(windowsFilePath));
			byte[] linuxHash = hashAlgo.ComputeHash(File.ReadAllBytes(linuxFilePath));
			byte[] macHash = hashAlgo.ComputeHash(File.ReadAllBytes(macFilePath));

			byte[] hashes = new byte[61];
			hashes[0] = 2; // Version
			Array.Copy(windowsHash, 0, hashes, 1, 20);
			Array.Copy(linuxHash, 0, hashes, 21, 20);
			Array.Copy(macHash, 0, hashes, 41, 20);

			//Debug.LogFormat("Windows hash: {0}", Hash.toString(windowsHash));
			//Debug.LogFormat("Linux hash: {0}", Hash.toString(linuxHash));
			//Debug.LogFormat("Mac hash: {0}", Hash.toString(macHash));

			File.WriteAllBytes(hashFilePath, hashes);
		}
	}
}
