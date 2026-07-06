////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCRewardsListAssetReward : INPCReward
	{
		public AssetReference<Asset> assetRef
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			Asset asset = assetRef.Find();
			if (asset == null)
			{
				UnturnedLog.warn($"Rewards list asset reward unable to resolve guid ({assetRef})");
				return;
			}

			if (asset is SpawnAsset spawnAsset)
			{
				asset = SpawnTableTool.Resolve(spawnAsset, OnGetSpawnTableErrorContext);
				if (asset == null)
				{
					// SpawnTableTool.Resolve already logged an error.
					return;
				}
			}

			if (asset is NPCRewardsAsset rewardsAsset)
			{
				if (rewardsAsset.AreConditionsMet(player))
				{
					rewardsAsset.ApplyConditions(player);
					rewardsAsset.GrantRewards(player);
				}
			}
			else
			{
				UnturnedLog.warn($"Rewards list asset reward unable to grant \"{asset.FriendlyName}\" because type is incompatible ({asset.GetTypeFriendlyName()})");
			}
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseGuid("GUID", out System.Guid _guid))
			{
				assetRef = new AssetReference<Asset>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseGuid(p.legacyPrefix + "_GUID", out System.Guid _guid))
			{
				assetRef = new AssetReference<Asset>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}
		}

		public NPCRewardsListAssetReward() { }

		[System.Obsolete]
		public NPCRewardsListAssetReward(AssetReference<Asset> newAssetRef, string newText) : base(newText)
		{
			assetRef = newAssetRef;
		}

		private string OnGetSpawnTableErrorContext()
		{
			return "NPC rewards list asset reward";
		}
	}
}
