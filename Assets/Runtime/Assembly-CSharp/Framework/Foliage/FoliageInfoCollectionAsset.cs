////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageInfoCollectionAsset : Asset
	{
		public struct FoliageInfoCollectionElement : IDatParseable
		{
			public AssetReference<FoliageInfoAsset> asset;
			public float weight;

			public bool TryParse(IDatNode node)
			{
				if (node is IDatDictionary dictionary)
				{
					asset = dictionary.ParseStruct<AssetReference<FoliageInfoAsset>>("Asset");
					weight = dictionary.ParseFloat("Weight", defaultValue: 1.0f);
					return true;
				}
				else
				{
					return false;
				}

			}
		}

		public List<FoliageInfoCollectionElement> elements;

		public virtual void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float weight)
		{
			foreach (FoliageInfoCollectionElement element in elements)
			{
				FoliageInfoAsset asset = Assets.find(element.asset);
				if (asset != null)
				{
					asset.bakeFoliage(bakeSettings, surface, bounds, weight, element.weight);
				}
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.TryGetList("Foliage", out IDatList foliageList))
			{
				elements = foliageList.ParseListOfStructs<FoliageInfoCollectionElement>();
			}
		}

		public FoliageInfoCollectionAsset() : base()
		{
			elements = new List<FoliageInfoCollectionElement>();
		}
	}
}
