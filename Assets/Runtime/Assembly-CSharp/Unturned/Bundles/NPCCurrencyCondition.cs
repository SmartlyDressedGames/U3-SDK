////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class NPCCurrencyCondition : NPCLogicCondition
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

		public override bool isConditionMet(Player player)
		{
			ItemCurrencyAsset currencyAsset = currency.Find();
			if (currencyAsset == null)
			{
				return false;
			}
			else
			{
				uint totalValue = currencyAsset.getInventoryValue(player);
				return doesLogicPass(totalValue, value);
			}
		}

		public override void ApplyCondition(Player player)
		{
			if (!shouldReset)
			{
				return;
			}

			ItemCurrencyAsset currencyAsset = currency.Find();
			if (currencyAsset == null)
			{
				return;
			}

			currencyAsset.spendValue(player, value);
		}

		public override string formatCondition(Player player)
		{
			ItemCurrencyAsset currencyAsset = currency.Find();
			if (currencyAsset == null)
			{
				return "?";
			}

			if (string.IsNullOrEmpty(text))
			{
				if (!string.IsNullOrEmpty(currencyAsset.defaultConditionFormat))
				{
					text = currencyAsset.defaultConditionFormat;
				}
				else
				{
					text = PlayerNPCQuestUI.localization.format("Condition_Currency");
				}
			}

			uint inventoryValue = currencyAsset.getInventoryValue(player);
			return Local.FormatText(text, inventoryValue, value);
		}
		
		internal override void PopulateV2(in PopulateConditionParameters p)
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

		internal override void PopulateLegacy(in PopulateConditionParameters p)
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

		public NPCCurrencyCondition() { }

		[System.Obsolete]
		public NPCCurrencyCondition(AssetReference<ItemCurrencyAsset> newCurrency, uint newValue, ENPCLogicType newLogicType, string newText, bool newShouldReset) : base(newLogicType, newText, newShouldReset)
		{
			currency = newCurrency;
			value = newValue;
		}
	}
}
