////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Parses mb:X from input string and filters assets using X master bundle.
	/// </summary>
	public struct MasterBundleSearchFilter
	{
		public static MasterBundleSearchFilter? parse(string filter)
		{
			string value;
			if (SearchFilterUtil.parseKeyValue(filter, "mb:", out value) == false)
				return null;

			MasterBundleConfig masterBundle = Assets.findMasterBundleByName(value, matchExtension: false);
			if (masterBundle == null)
				return null;

			return new MasterBundleSearchFilter(masterBundle);
		}

		public bool ignores(Asset asset)
		{
			return asset == null || asset.originMasterBundle != masterBundle;
		}

		public bool matches(Asset asset)
		{
			return ignores(asset) == false;
		}

		public MasterBundleSearchFilter(MasterBundleConfig masterBundle)
		{
			this.masterBundle = masterBundle;
		}

		private MasterBundleConfig masterBundle;
	}
}
