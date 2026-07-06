////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEditor;
using UnityEngine;

public class UnturnedAssetPostprocessor : AssetPostprocessor
{
	public override uint GetVersion()
	{
		return 1;
	}

	private void OnPreprocessTexture()
	{
		if (assetPath.StartsWith("Assets/CoreMasterBundle/Items"))
		{
			string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
			if (fileName == "Shirt" || fileName == "Pants")
			{
				TextureImporter textureImporter = (TextureImporter) assetImporter;
				textureImporter.isReadable = false;
				textureImporter.mipmapEnabled = true;
				textureImporter.wrapMode = TextureWrapMode.Clamp;
				textureImporter.filterMode = FilterMode.Point;
			}
		}
		else
		{
			if (!assetPath.StartsWith("Assets/Resources/Economy/CosmeticPreviews")
				&& !assetPath.StartsWith("Assets/Resources/Economy/Item"))
			{
				return;
			}

			TextureImporter textureImporter = (TextureImporter) assetImporter;
			textureImporter.textureType = TextureImporterType.GUI;
			textureImporter.filterMode = FilterMode.Bilinear;
		}
	}
}
