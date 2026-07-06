////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCCurrencyReward : INPCReward
	{
		public AssetReference<ItemCurrencyAsset> currency
		{
			get;
			protected set;
		}

		public uint value
		{
			get;
			protected set;
		}

		public override void GrantReward(Player player)
		{
			ItemCurrencyAsset currencyAsset = currency.Find();
			if (currencyAsset == null)
				return;

			currencyAsset.grantValue(player, value);
		}

		public override string formatReward(Player player)
		{
			if (string.IsNullOrEmpty(text))
			{
				ItemCurrencyAsset currencyAsset = currency.Find();
				if (currencyAsset != null && string.IsNullOrEmpty(currencyAsset.valueFormat) == false)
				{
					text = currencyAsset.valueFormat;
				}
				else
				{
					text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Currency");
				}
			}

			return Local.FormatText(text, value);
		}

		internal override void PopulateV2(in PopulateRewardParameters p)
		{
			base.PopulateV2(p);

			if (p.data.TryParseGuid("GUID", out System.Guid _guid))
			{
				currency = new AssetReference<ItemCurrencyAsset>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}

			if (p.data.TryParseUInt32("Value", out uint _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		internal override void PopulateLegacy(in PopulateRewardParameters p)
		{
			base.PopulateLegacy(p);

			if (p.data.TryParseGuid(p.legacyPrefix + "_GUID", out System.Guid _guid))
			{
				currency = new AssetReference<ItemCurrencyAsset>(_guid);
			}
			else
			{
				p.ReportRequiredOptionInvalid("GUID");
			}

			if (p.data.TryParseUInt32(p.legacyPrefix + "_Value", out uint _value))
			{
				value = _value;
			}
			else
			{
				p.ReportRequiredOptionInvalid("Value");
			}
		}

		public NPCCurrencyReward() { }

		[System.Obsolete]
		public NPCCurrencyReward(AssetReference<ItemCurrencyAsset> newCurrency, uint newValue, string newText) : base(newText)
		{
			currency = newCurrency;
			value = newValue;
		}
	}
}
