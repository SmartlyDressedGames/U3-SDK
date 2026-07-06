////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class MaterialPaletteAsset : Asset
	{

		public List<ContentReference<Material>> materials;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.TryGetList("Materials", out IDatList materialsList))
			{
				materials = materialsList.ParseListOfStructs<ContentReference<Material>>();
			}
		}

		public MaterialPaletteAsset() : base()
		{
			materials = new List<ContentReference<Material>>();
		}
	}
}
