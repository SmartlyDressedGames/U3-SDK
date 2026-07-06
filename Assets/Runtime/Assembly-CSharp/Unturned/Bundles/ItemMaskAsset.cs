////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemMaskAsset : ItemGearAsset
	{
		protected GameObject _mask;
		public GameObject mask => _mask;

		private bool _isEarpiece;
		public bool isEarpiece => _isEarpiece;

		/// <summary>
		/// Multiplier for how quickly deadzones deplete a gasmask's filter quality.
		/// e.g., 2 is faster (2x) and 0.5 is slower.
		/// </summary>
		public float FilterDegradationRateMultiplier
		{
			get;
			protected set;
		} = 1.0f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		/// <summary>
		/// Hack for previewing the "aura" cosmetic items.
		/// </summary>
		public ushort cosmeticPreviewMythicId;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			if (FilterDegradationRateMultiplier != 1.0f)
			{
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FilterDegradationRateMultiplier", PlayerDashboardInventoryUI.FormatStatModifier(FilterDegradationRateMultiplier, true, false)), DescSort_ClothingStat + DescSort_LowerIsBeneficial(FilterDegradationRateMultiplier));
			}

			if (isEarpiece)
			{
				builder.Append(PlayerDashboardInventoryUI.FormatStatColor(PlayerDashboardInventoryUI.localization.format("ItemDescription_Clothing_Earpiece"), true), DescSort_ClothingStat + DescSort_Beneficial);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			FilterDegradationRateMultiplier = p.data.ParseFloat("FilterDegradationRateMultiplier", defaultValue: 1.0f);

			if (!Dedicator.IsDedicatedServer)
			{
				_mask = loadRequiredAsset<GameObject>(p.bundle, "Mask");

				if (Assets.shouldValidateAssets)
				{
					AssetValidation.ValidateLayersEqual(this, _mask, LayerMasks.ENEMY);
					AssetValidation.ValidateClothComponents(this, _mask);
				}
			}

			if (isPro)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				cosmeticPreviewMythicId = p.data.ParseUInt16("CosmeticPreviewMythicId");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}
			else
			{
				_isEarpiece = p.data.ContainsKey("Earpiece");
			}
		}

		internal override void BuildCargoData(CargoBuilder builder)
		{
			base.BuildCargoData(builder);

			// https://unturned.wiki.gg/wiki/Special:CargoTables/Gear
			// Game data for Gear Item assets.
			CargoDeclaration data = builder.GetOrAddDeclaration("Gear");
			data.Append("GUID", GUID); // Key

			data.Append("FilterDegradationRateMultiplier", FilterDegradationRateMultiplier);
			data.Append("Earpiece", isEarpiece);
		}

		internal override GameObject ClothingPrefab => mask;
	}
}
