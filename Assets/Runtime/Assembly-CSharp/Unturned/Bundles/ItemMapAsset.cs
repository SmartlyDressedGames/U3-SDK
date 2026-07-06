////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemMapAsset : ItemAsset
	{
		/// <summary>
		/// Does having this item show the compass?
		/// </summary>
		public bool enablesCompass
		{
			get;
			protected set;
		}

		/// <summary>
		/// Does having this item show the chart?
		/// </summary>
		public bool enablesChart
		{
			get;
			protected set;
		}

		/// <summary>
		/// Does having this item show the satellite?
		/// </summary>
		public bool enablesMap
		{
			get;
			protected set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			enablesCompass = p.data.ContainsKey("Enables_Compass");
			enablesChart = p.data.ContainsKey("Enables_Chart");
			enablesMap = p.data.ContainsKey("Enables_Map");
		}
	}
}
