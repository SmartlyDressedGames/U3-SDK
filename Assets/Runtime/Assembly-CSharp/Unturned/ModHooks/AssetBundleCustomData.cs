////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	[CreateAssetMenu(fileName = "AssetBundleCustomData", menuName = "Unturned/Asset Bundle Custom Data")]
	public class AssetBundleCustomData : ScriptableObject
	{
		/// <summary>
		/// If Unturned is loading this asset bundle from a Steam workshop file but the file ID does not match then
		/// loading will be canceled. Makes it somewhat harder to copy/re-upload without permission.
		/// </summary>
		public ulong ownerWorkshopFileId;

		/// <summary>
		/// Same as ownerWorkshopFileId for cases where the asset bundle is allowed in multiple uploads.
		///
		/// Uploading the same asset bundle multiple times is not ideal because the game doesn't handle
		/// multiple of them with the same name well, and Unity logs an error if an asset bundle with the
		/// same files is already loaded. That being said, the game doesn't handle dependencies between
		/// workshop files well either (as of 2023-01-12), so this is perhaps the lesser of two evils.
		/// 
		/// My understanding is that some mod creators license their work to multiple servers that upload
		/// the files and this property will make it easier so it doesn't need to be re-exported multiple times.
		/// </summary>
		public List<ulong> ownerWorkshopFileIds;
	}
}
