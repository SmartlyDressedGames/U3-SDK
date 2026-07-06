////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemBackpackAsset : ItemBagAsset
	{
		protected GameObject _backpack;
		public GameObject backpack => _backpack;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (!Dedicator.IsDedicatedServer)
			{
				_backpack = loadRequiredAsset<GameObject>(p.bundle, "Backpack");

				if (Assets.shouldValidateAssets)
				{
					AssetValidation.ValidateLayersEqual(this, _backpack, LayerMasks.ENEMY);
					AssetValidation.ValidateClothComponents(this, _backpack);
				}
			}
		}

		protected override AudioReference GetDefaultInventoryAudio()
		{
			// Remember width and height are the storage dimensions.
			if (width <= 3 || height <= 3)
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/LightMetalEquipment.asset");
			}
			else if (width <= 6 || height <= 6)
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/MediumMetalEquipment.asset");
			}
			else
			{
				return new AudioReference("core.masterbundle", "Sounds/Inventory/HeavyMetalEquipment.asset");
			}
		}

		internal override GameObject ClothingPrefab => backpack;
	}
}
