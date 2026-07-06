////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Special asset type that isn't (shouldn't be) returned by asset Find methods. Instead, if found when resolving
	/// an asset legacy ID or GUID, Find is called again with the target specified by this asset.
	/// </summary>
	public class RedirectorAsset : Asset
	{
		public override EAssetType assetCategory => assetCategoryOverride;

		protected EAssetType assetCategoryOverride;

		private System.Guid _targetGuid;
		public System.Guid TargetGuid
		{
			get => _targetGuid;
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			assetCategoryOverride = p.data.ParseEnum("AssetCategory", defaultValue: EAssetType.NONE);
			if (id > 0 && assetCategoryOverride == EAssetType.NONE)
			{
				Assets.ReportError(this, "legacy ID was assigned but AssetCategory was not");
			}

			if (!p.data.TryParseGuid("TargetAsset", out _targetGuid))
			{
				Assets.ReportError(this, "unable to parse TargetAsset");
			}
		}
	}
}
