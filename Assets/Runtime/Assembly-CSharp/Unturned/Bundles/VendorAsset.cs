////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class VendorAsset : Asset
	{
		public string vendorName
		{
			get;
			protected set;
		}

		public override string FriendlyName
		{
			get
			{
				// Nelson 2024-11-14: Vendor assets often contain rich text we don't want in file names. (public issue #4783)
				return RichTextUtil.replaceColorTags(vendorName);
			}
		}

		public string vendorDescription
		{
			get;
			protected set;
		}

		public VendorBuying[] buying
		{
			get;
			protected set;
		}

		public VendorSellingBase[] selling
		{
			get;
			protected set;
		}

		/// <summary>
		/// Should the buying and selling lists be alphabetically sorted?
		/// </summary>
		public bool enableSorting
		{
			get;
			protected set;
		}

		public AssetReference<ItemCurrencyAsset> currency
		{
			get;
			protected set;
		}

		public byte? faceOverride
		{
			get;
			private set;
		}

		public override EAssetType assetCategory => EAssetType.NPC;

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			if (id < 2000 && !OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
			{
				throw new System.NotSupportedException("ID < 2000");
			}

			vendorName = p.localization.format("Name");
			vendorName = ItemTool.filterRarityRichText(vendorName);

			string description = p.localization.format("Description");
			description = ItemTool.filterRarityRichText(description);
			RichTextUtil.replaceNewlineMarkup(ref description);
			vendorDescription = description;

			if (p.data.ContainsKey("FaceOverride"))
			{
				faceOverride = p.data.ParseUInt8("FaceOverride");
			}
			else
			{
				faceOverride = null;
			}

			buying = new VendorBuying[p.data.ParseUInt8("Buying")];
			for (byte buyingIndex = 0; buyingIndex < buying.Length; buyingIndex++)
			{
				string buyingDescription = p.localization.FormatOrNull($"Buying_{buyingIndex}_Description");
				if (!string.IsNullOrEmpty(buyingDescription))
				{
					buyingDescription = ItemTool.filterRarityRichText(buyingDescription);
				}

				System.Guid buyingGuid;
				ushort buyingLegacyId;
				p.data.ParseGuidOrLegacyId("Buying_" + buyingIndex + "_ID", out buyingGuid, out buyingLegacyId);
				uint buyingCost = p.data.ParseUInt32("Buying_" + buyingIndex + "_Cost");

				NPCConditionsList buyingConditionsList = new NPCConditionsList();
				buyingConditionsList.Parse(p.data, p.localization, this, "Buying_" + buyingIndex + "_Conditions", "Buying_" + buyingIndex + "_Condition_");

				NPCRewardsList buyingRewardsList = new NPCRewardsList();
				buyingRewardsList.Parse(p.data, p.localization, this, "Buying_" + buyingIndex + "_Rewards", "Buying_" + buyingIndex + "_Reward_");

				buying[buyingIndex] = new VendorBuying(this, buyingIndex, buyingGuid, buyingLegacyId, buyingCost, buyingConditionsList, buyingRewardsList, buyingDescription);
			}

			selling = new VendorSellingBase[p.data.ParseUInt8("Selling")];
			for (byte sellingIndex = 0; sellingIndex < selling.Length; sellingIndex++)
			{
				string sellingType = null;
				if (p.data.ContainsKey("Selling_" + sellingIndex + "_Type"))
				{
					sellingType = p.data.GetString("Selling_" + sellingIndex + "_Type");
				}

				string sellingDescription = p.localization.FormatOrNull($"Selling_{sellingIndex}_Description");
				if (!string.IsNullOrEmpty(sellingDescription))
				{
					sellingDescription = ItemTool.filterRarityRichText(sellingDescription);
				}

				System.Guid sellingGuid;
				ushort sellingLegacyId;
				p.data.ParseGuidOrLegacyId("Selling_" + sellingIndex + "_ID", out sellingGuid, out sellingLegacyId);
				uint sellingCost = p.data.ParseUInt32("Selling_" + sellingIndex + "_Cost");

				NPCConditionsList sellingConditionsList = new NPCConditionsList();
				sellingConditionsList.Parse(p.data, p.localization, this, "Selling_" + sellingIndex + "_Conditions", "Selling_" + sellingIndex + "_Condition_");

				NPCRewardsList sellingRewardsList = new NPCRewardsList();
				sellingRewardsList.Parse(p.data, p.localization, this, "Selling_" + sellingIndex + "_Rewards", "Selling_" + sellingIndex + "_Reward_");

				if (sellingType == null || sellingType.Equals("Item", System.StringComparison.InvariantCultureIgnoreCase))
				{
					int sight = p.data.ParseInt32("Selling_" + sellingIndex + "_Sight", defaultValue: -1);
					int tactical = p.data.ParseInt32("Selling_" + sellingIndex + "_Tactical", defaultValue: -1);
					int grip = p.data.ParseInt32("Selling_" + sellingIndex + "_Grip", defaultValue: -1);
					int barrel = p.data.ParseInt32("Selling_" + sellingIndex + "_Barrel", defaultValue: -1);
					int magazine = p.data.ParseInt32("Selling_" + sellingIndex + "_Magazine", defaultValue: -1);
					int ammo = p.data.ParseInt32("Selling_" + sellingIndex + "_Ammo", defaultValue: -1);

					selling[sellingIndex] = new VendorSellingItem(this, sellingIndex, sellingGuid, sellingLegacyId, sellingCost, sellingConditionsList, sellingRewardsList, sellingDescription, sight, tactical, grip, barrel, magazine, ammo);
				}
				else if (sellingType.Equals("Vehicle", System.StringComparison.InvariantCultureIgnoreCase))
				{
					string spawnpointKey = "Selling_" + sellingIndex + "_Spawnpoint";
					string spawnpoint = p.data.GetString(spawnpointKey);
					if (string.IsNullOrEmpty(spawnpoint))
					{
						Assets.ReportError(this, $"missing \"{spawnpointKey}\" for vehicle");
					}

					Color32? paintColor = null;
					if (p.data.TryParseColor32RGB("Selling_" + sellingIndex + "_PaintColor", out Color32 paintColorOverride))
					{
						paintColor = paintColorOverride;
					}

					selling[sellingIndex] = new VendorSellingVehicle(this, sellingIndex, sellingGuid, sellingLegacyId, sellingCost, spawnpoint, paintColor, sellingConditionsList, sellingRewardsList, sellingDescription);
				}
				else
				{
					throw new System.NotSupportedException("unknown selling type: '" + sellingType + "'");
				}
			}

			// Modders requested ability to disable the alphabetic sorting, but it's enabled by default
			enableSorting = !p.data.ContainsKey("Disable_Sorting");

			currency = p.data.readAssetReference<ItemCurrencyAsset>("Currency");
		}
	}
}
