////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemVehiclePaintToolAsset : ItemToolAsset
	{
		public Color32 PaintColor
		{
			get;
			protected set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (p.data.TryParseColor32RGB("PaintColor", out Color32 color))
			{
				PaintColor = color;
			}
			else
			{
				Assets.ReportError(this, "missing PaintColor");
			}
		}
	}
}
