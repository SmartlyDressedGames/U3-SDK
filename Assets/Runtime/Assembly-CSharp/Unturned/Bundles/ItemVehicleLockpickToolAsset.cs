////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemVehicleLockpickToolAsset : ItemToolAsset
	{
		private CachingBcAssetRef _failureEffectRef;
		public CachingBcAssetRef FailureEffect
		{
			get => _failureEffectRef;
			set => _failureEffectRef = value;
		}
		public EffectAsset FindFailureEffect() => _failureEffectRef.Get<EffectAsset>();

		/// <summary>
		/// If greater than zero, picking the lock can fail.
		/// </summary>
		public float FailureProbability
		{
			get;
			set;
		}

		public bool CanFail => FailureProbability > 0.00001f;

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (CanFail)
			{
				string failText = PlayerDashboardInventoryUI.FormatStatColor(FailureProbability.ToString("P0"), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_LockpickFailureProbability", failText), DescSort_ItemStat);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			p.data.TryParseBcAssetRef("FailureEffect", EAssetType.EFFECT, out _failureEffectRef);
			FailureProbability = p.data.ParseFloat("FailureProbability", 0.0f);
		}
	}
}
