////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum EBoxItemOrigin
	{
		Unbox,
		Unwrap,
	}

	public enum EBoxProbabilityModel
	{
		/// <summary>
		/// Each quality tier has different rarities.
		/// Legendary: 5% Epic: 20% Rare: 75%
		/// </summary>
		Original,

		/// <summary>
		/// Each item has an equal chance regardless of quality.
		/// </summary>
		Equalized,
	}

	public class ItemBoxAsset : ItemAsset
	{
		protected int _generate;
		public int generate => _generate;

		protected int _destroy;
		public int destroy => _destroy;

		protected int[] _drops;
		public int[] drops => _drops;

		public EBoxItemOrigin itemOrigin
		{
			get;
			protected set;
		}

		public EBoxProbabilityModel probabilityModel
		{
			get;
			protected set;
		}

		public bool containsBonusItems
		{
			get;
			protected set;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_generate = p.data.ParseInt32("Generate");
			_destroy = p.data.ParseInt32("Destroy");

			_drops = new int[p.data.ParseInt32("Drops")];
			for (int index = 0; index < drops.Length; index++)
			{
				drops[index] = p.data.ParseInt32("Drop_" + index);
			}

			itemOrigin = p.data.ParseEnum("Item_Origin", defaultValue: EBoxItemOrigin.Unbox);
			probabilityModel = p.data.ParseEnum("Probability_Model", defaultValue: EBoxProbabilityModel.Original);
			containsBonusItems = p.data.ParseBool("Contains_Bonus_Items");
		}
	}
}
