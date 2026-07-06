////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCHintReward : INPCReward
	{
		/// <summary>
		/// How many seconds message should popup.
		/// </summary>
		private float duration;

		public override void GrantReward(Player player)
		{
			if (hintTextAsset.Get() != null && !string.IsNullOrEmpty(hintTextLocKey))
			{
				player.ServerShowTranslatedHint(hintTextAsset.Get(), hintTextLocKey, duration);
			}
			else
			{
				player.ServerShowHint(text, duration);
			}
		}

		private CachingAssetRef hintTextAsset;
		private string hintTextLocKey;

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (string.IsNullOrEmpty(text))
			{
				text = p.data.GetString("Text");
			}
			else
			{
				// Only enable replicated hint if asset kept localization loaded.
				Asset asset = p.errorContext as Asset;
				if (asset != null && asset.Localization != null)
				{
					hintTextAsset = asset;
					hintTextLocKey = p.data.GetString("TextId");
				}
			}

			duration = p.data.ParseFloat("Duration", 2.0f);
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (string.IsNullOrEmpty(text))
			{
				text = p.data.GetString(p.legacyPrefix + "_Text");
			}
			else
			{
				// Only enable replicated hint if asset kept localization loaded.
				Asset asset = p.errorContext as Asset;
				if (asset != null && asset.Localization != null)
				{
					hintTextAsset = p.errorContext as Asset;
					hintTextLocKey = p.legacyPrefix;
				}
			}

			duration = p.data.ParseFloat(p.legacyPrefix + "_Duration", 2.0f);
		}

		public NPCHintReward() { }

		[System.Obsolete]
		public NPCHintReward(float newDuration, string newText) : base(newText)
		{
			duration = newDuration;
		}
	}
}
