////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEngine;

namespace SDG.Unturned
{
	public static class AssetImporterEx
	{
		public static T GetAtPath<T>(string path) where T : AssetImporter
		{
			return AssetImporter.GetAtPath(path) as T;
		}

		public static AssetImporter GetForAsset(Object assetObject)
		{
			return AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(assetObject));
		}

		public static T GetForAsset<T>(Object assetObject) where T : AssetImporter
		{
			return GetForAsset(assetObject) as T;
		}
	}
}
