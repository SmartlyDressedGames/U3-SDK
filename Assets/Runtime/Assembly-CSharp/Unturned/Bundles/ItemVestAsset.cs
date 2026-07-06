////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemVestAsset : ItemBagAsset
	{
		protected GameObject _vest;
		public GameObject vest => _vest;

		/// <summary>
		/// If true and player has no shirt equipped, use fallback shirt as equipped shirt.
		/// Used by oversize vest and zip-up vest so they are visible without a shirt equipped.
		/// </summary>
		internal bool hasFallbackShirt;
		internal CachingAssetRef fallbackShirt;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (!Dedicator.IsDedicatedServer)
			{
				_vest = loadRequiredAsset<GameObject>(p.bundle, "Vest");

				if (Assets.shouldValidateAssets)
				{
					AssetValidation.ValidateLayersEqual(this, _vest, LayerMasks.ENEMY);
					AssetValidation.ValidateClothComponents(this, _vest);
				}

				hasFallbackShirt = p.data.ParseBool("Has_Fallback_Shirt");
				if (hasFallbackShirt)
				{
					fallbackShirt = p.data.ParseAssetRef("Fallback_Shirt");
				}
			}
		}

		internal override GameObject ClothingPrefab => vest;
	}
}
