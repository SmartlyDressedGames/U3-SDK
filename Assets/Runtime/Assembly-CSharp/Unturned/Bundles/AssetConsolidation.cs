////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if WITH_ASSET_CONSOLIDATION
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned
{
	public static class AssetConsolidation
	{
		public static void OnLoadingPath(string pathWithinAssetBundle)
		{
			if(IsDuplicate(pathWithinAssetBundle))
			{
				assetFilePaths.Add(Assets.currentFilePath);
			}
		}

		public static string[] ReadAssetFilePaths()
		{
			return File.ReadAllLines(AssetUsageFilePath);
		}

		public static void WriteAssetFilePaths()
		{
			if(assetFilePaths.Count > 0)
			{
				File.WriteAllLines(AssetUsageFilePath, assetFilePaths.ToArray());
			}
		}
		
		private static bool IsDuplicate(string pathWithinAssetBundle)
		{
			foreach(string filePath in duplicateFilePaths)
			{
				if(filePath.EndsWith(pathWithinAssetBundle))
				{
					return true;
				}
			}

			return false;
		}

		public static string[] ReadDuplicateReport()
		{
			duplicateFilePaths = File.ReadAllLines(DuplicateReportFilePath);
			return duplicateFilePaths;
		}

		public static void WriteDuplicateReport(string[] filePaths)
		{
			File.WriteAllLines(DuplicateReportFilePath, filePaths);
		}

		private static string AssetUsageFilePath
		{
			get { return Path.Combine(GameProject.PROJECT_PATH, "Temp", "AssetUsage.csv"); }
		}

		private static string DuplicateReportFilePath
		{
			get { return Path.Combine(GameProject.PROJECT_PATH, "Temp", "DuplicateReport.csv"); }
		}

		private static string[] duplicateFilePaths;
		private static List<string> assetFilePaths = new List<string>();
	}
}
#endif // WITH_ASSET_CONSOLIDATION
