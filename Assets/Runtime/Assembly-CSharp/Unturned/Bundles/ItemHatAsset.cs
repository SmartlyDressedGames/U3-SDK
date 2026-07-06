////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemHatAsset : ItemGearAsset
	{
		protected GameObject _hat;
		public GameObject hat => _hat;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (!Dedicator.IsDedicatedServer)
			{
				_hat = loadRequiredAsset<GameObject>(p.bundle, "Hat");

				if (Assets.shouldValidateAssets)
				{
					AssetValidation.ValidateLayersEqual(this, _hat, LayerMasks.ENEMY);
					AssetValidation.ValidateClothComponents(this, _hat);
				}
			}
		}

		internal override GameObject ClothingPrefab => hat;
	}
}
