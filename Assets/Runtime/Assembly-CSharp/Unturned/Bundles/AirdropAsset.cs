////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class AirdropAsset : Asset
	{
		public static AssetReference<AirdropAsset> defaultAirdrop = new AssetReference<AirdropAsset>("229440c249dc490ba26ce71e8a59d5c6");

		/// <summary>
		/// Interactable storage barricade to spawn at the drop position.
		/// </summary>
		public AssetReference<ItemBarricadeAsset> barricadeRef;

		/// <summary>
		/// Prefab to spawn falling from the aircraft.
		/// </summary>
		public MasterBundleReference<GameObject> model;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			barricadeRef = p.data.ParseStruct<AssetReference<ItemBarricadeAsset>>("Landed_Barricade");
			model = p.data.ParseStruct<MasterBundleReference<GameObject>>("Carepackage_Prefab");
		}
	}
}
