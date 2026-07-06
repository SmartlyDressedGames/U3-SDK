////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class OutfitAsset : Asset
	{
		public AssetReference<ItemAsset>[] itemAssets;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.TryGetList("Items", out IDatList itemNodes))
			{
				itemAssets = itemNodes.ParseArrayOfStructs<AssetReference<ItemAsset>>();
			}
			else
			{
				itemAssets = new AssetReference<ItemAsset>[0];
			}
		}
	}
}
