////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public enum ERefillWaterType
	{
		EMPTY,
		CLEAN, // somewhere safe
		SALTY, // from the ocean
		DIRTY // from an object
	}

	public class ItemRefillAsset : ItemAsset
	{
		protected AudioClip _use;
		public AudioClip use => _use;

		/// <summary>
		/// Kept for backwards compatibility with plugins.
		/// </summary>
		[System.Obsolete("Replaced by separate stats for each water type")]
		public byte water => MathfEx.RoundAndClampToByte(cleanWater);

		public float cleanHealth
		{
			get;
			protected set;
		}

		public float saltyHealth
		{
			get;
			protected set;
		}

		public float dirtyHealth
		{
			get;
			protected set;
		}

		public float cleanFood
		{
			get;
			protected set;
		}

		public float saltyFood
		{
			get;
			protected set;
		}

		public float dirtyFood
		{
			get;
			protected set;
		}

		public float cleanWater
		{
			get;
			protected set;
		}

		public float saltyWater
		{
			get;
			protected set;
		}

		public float dirtyWater
		{
			get;
			protected set;
		}

		public float cleanVirus
		{
			get;
			protected set;
		}

		public float saltyVirus
		{
			get;
			protected set;
		}

		public float dirtyVirus
		{
			get;
			protected set;
		}

		public float cleanStamina
		{
			get;
			protected set;
		}

		public float saltyStamina
		{
			get;
			protected set;
		}

		public float dirtyStamina
		{
			get;
			protected set;
		}

		public float cleanOxygen
		{
			get;
			protected set;
		}

		public float saltyOxygen
		{
			get;
			protected set;
		}

		public float dirtyOxygen
		{
			get;
			protected set;
		}

		public float GetRefillHealth(ERefillWaterType refillWaterType)
		{
			switch (refillWaterType)
			{
				case ERefillWaterType.CLEAN:
					return cleanHealth;

				case ERefillWaterType.SALTY:
					return saltyHealth;

				case ERefillWaterType.DIRTY:
					return dirtyHealth;

				default:
					return 0.0f;
			}
		}

		public float GetRefillFood(ERefillWaterType refillWaterType)
		{
			switch (refillWaterType)
			{
				case ERefillWaterType.CLEAN:
					return cleanFood;

				case ERefillWaterType.SALTY:
					return saltyFood;

				case ERefillWaterType.DIRTY:
					return dirtyFood;

				default:
					return 0.0f;
			}
		}

		public float GetRefillWater(ERefillWaterType refillWaterType)
		{
			switch (refillWaterType)
			{
				case ERefillWaterType.CLEAN:
					return cleanWater;

				case ERefillWaterType.SALTY:
					return saltyWater;

				case ERefillWaterType.DIRTY:
					return dirtyWater;

				default:
					return 0.0f;
			}
		}

		public float GetRefillVirus(ERefillWaterType refillWaterType)
		{
			switch (refillWaterType)
			{
				case ERefillWaterType.CLEAN:
					return cleanVirus;

				case ERefillWaterType.SALTY:
					return saltyVirus;

				case ERefillWaterType.DIRTY:
					return dirtyVirus;

				default:
					return 0.0f;
			}
		}

		public float GetRefillStamina(ERefillWaterType refillWaterType)
		{
			switch (refillWaterType)
			{
				case ERefillWaterType.CLEAN:
					return cleanStamina;

				case ERefillWaterType.SALTY:
					return saltyStamina;

				case ERefillWaterType.DIRTY:
					return dirtyStamina;

				default:
					return 0.0f;
			}
		}

		public float GetRefillOxygen(ERefillWaterType refillWaterType)
		{
			switch (refillWaterType)
			{
				case ERefillWaterType.CLEAN:
					return cleanOxygen;

				case ERefillWaterType.SALTY:
					return saltyOxygen;

				case ERefillWaterType.DIRTY:
					return dirtyOxygen;

				default:
					return 0.0f;
			}
		}

		public override byte[] getState(EItemOrigin origin)
		{
			byte[] state = new byte[1];

			if (origin == EItemOrigin.ADMIN)
			{
				state[0] = (byte) ERefillWaterType.CLEAN;
			}
			else
			{
				state[0] = (byte) ERefillWaterType.EMPTY;
			}

			return state;
		}

		public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
		{
			base.BuildDescription(builder, itemInstance);

			if (itemInstance == null)
				return;

			ERefillWaterType waterType = (ERefillWaterType) itemInstance.state[0];
			string waterKey;

			switch (waterType)
			{
				case ERefillWaterType.EMPTY:
					waterKey = "Empty";
					break;
				case ERefillWaterType.CLEAN:
					waterKey = "Clean";
					break;
				case ERefillWaterType.SALTY:
					waterKey = "Salty";
					break;
				case ERefillWaterType.DIRTY:
					waterKey = "Dirty";
					break;
				default:
					waterKey = "Full";
					break;
			}

			builder.Append(PlayerDashboardInventoryUI.localization.format("Refill", PlayerDashboardInventoryUI.localization.format(waterKey)), DescSort_Important);

			if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
				return;

			int roundedHealth = Mathf.RoundToInt(GetRefillHealth(waterType));
			if (roundedHealth > 0)
			{
				string healthText = PlayerDashboardInventoryUI.FormatStatColor(roundedHealth.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_HealthPositive", healthText), DescSort_RefillStat + DescSort_Beneficial);
			}
			else if (roundedHealth < 0)
			{
				string healthText = PlayerDashboardInventoryUI.FormatStatColor((-roundedHealth).ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_HealthNegative", healthText), DescSort_RefillStat + DescSort_Detrimental);
			}

			int roundedFood = Mathf.RoundToInt(GetRefillFood(waterType));
			if (roundedFood > 0)
			{
				string foodText = PlayerDashboardInventoryUI.FormatStatColor(roundedFood.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_FoodPositive", foodText), DescSort_RefillStat + DescSort_Beneficial);
			}
			else if (roundedFood < 0)
			{
				string foodText = PlayerDashboardInventoryUI.FormatStatColor((-roundedFood).ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_FoodNegative", foodText), DescSort_RefillStat + DescSort_Detrimental);
			}

			int roundedWater = Mathf.RoundToInt(GetRefillWater(waterType));
			if (roundedWater > 0)
			{
				string waterText = PlayerDashboardInventoryUI.FormatStatColor(roundedWater.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WaterPositive", waterText), DescSort_RefillStat + DescSort_Beneficial);
			}
			else if (roundedWater < 0)
			{
				string waterText = PlayerDashboardInventoryUI.FormatStatColor((-roundedWater).ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WaterNegative", waterText), DescSort_RefillStat + DescSort_Detrimental);
			}

			int roundedVirus = Mathf.RoundToInt(GetRefillVirus(waterType));
			if (roundedVirus > 0)
			{
				string virusText = PlayerDashboardInventoryUI.FormatStatColor(roundedVirus.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusPositive", virusText), DescSort_RefillStat + DescSort_Beneficial);
			}
			else if (roundedVirus < 0)
			{
				string virusText = PlayerDashboardInventoryUI.FormatStatColor((-roundedVirus).ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusNegative", virusText), DescSort_RefillStat + DescSort_Detrimental);
			}

			int roundedStamina = Mathf.RoundToInt(GetRefillStamina(waterType));
			if (roundedStamina > 0)
			{
				string staminaText = PlayerDashboardInventoryUI.FormatStatColor(roundedStamina.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_StaminaPositive", staminaText), DescSort_RefillStat + DescSort_Beneficial);
			}
			else if (roundedStamina < 0)
			{
				string staminaText = PlayerDashboardInventoryUI.FormatStatColor((-roundedStamina).ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_StaminaNegative", staminaText), DescSort_RefillStat + DescSort_Detrimental);
			}

			int roundedOxygen = Mathf.RoundToInt(GetRefillOxygen(waterType));
			if (roundedOxygen > 0)
			{
				string oxygenText = PlayerDashboardInventoryUI.FormatStatColor(roundedOxygen.ToString(), true);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_OxygenPositive", oxygenText), DescSort_RefillStat + DescSort_Beneficial);
			}
			else if (roundedOxygen < 0)
			{
				string oxygenText = PlayerDashboardInventoryUI.FormatStatColor((-roundedOxygen).ToString(), false);
				builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_OxygenNegative", oxygenText), DescSort_RefillStat + DescSort_Detrimental);
			}
		}

		public override void PopulateAsset(in PopulateAssetParameters p)
		{
			base.PopulateAsset(in p);

			_use = LoadRedirectableAsset<AudioClip>(p.bundle, "Use", p.data, "ConsumeAudioClip");

			float legacyWaterValue = p.data.ParseFloat("Water");
			const float defaultSaltyFactor = 0.25f;
			const float defaultDirtyFactor = 0.6f;

			cleanHealth = p.data.ParseFloat("Clean_Health", defaultValue: 0.0f);
			saltyHealth = p.data.ParseFloat("Salty_Health", defaultValue: cleanHealth * defaultSaltyFactor);
			dirtyHealth = p.data.ParseFloat("Dirty_Health", defaultValue: cleanHealth * defaultDirtyFactor);

			cleanFood = p.data.ParseFloat("Clean_Food", defaultValue: 0.0f);
			saltyFood = p.data.ParseFloat("Salty_Food", defaultValue: cleanFood * defaultSaltyFactor);
			dirtyFood = p.data.ParseFloat("Dirty_Food", defaultValue: cleanFood * defaultDirtyFactor);

			cleanWater = p.data.ParseFloat("Clean_Water", defaultValue: legacyWaterValue);
			saltyWater = p.data.ParseFloat("Salty_Water", defaultValue: cleanWater * defaultSaltyFactor);
			dirtyWater = p.data.ParseFloat("Dirty_Water", defaultValue: cleanWater * defaultDirtyFactor);

			cleanVirus = p.data.ParseFloat("Clean_Virus", defaultValue: 0.0f);
			saltyVirus = p.data.ParseFloat("Salty_Virus", defaultValue: cleanWater * (-1.0f + defaultSaltyFactor));
			dirtyVirus = p.data.ParseFloat("Dirty_Virus", defaultValue: cleanWater * (-1.0f + defaultDirtyFactor));

			cleanStamina = p.data.ParseFloat("Clean_Stamina", defaultValue: 0.0f);
			saltyStamina = p.data.ParseFloat("Salty_Stamina", defaultValue: cleanStamina * defaultSaltyFactor);
			dirtyStamina = p.data.ParseFloat("Dirty_Stamina", defaultValue: cleanStamina * defaultDirtyFactor);

			cleanOxygen = p.data.ParseFloat("Clean_Oxygen", defaultValue: 0.0f);
			saltyOxygen = p.data.ParseFloat("Salty_Oxygen", defaultValue: cleanOxygen * defaultSaltyFactor);
			dirtyOxygen = p.data.ParseFloat("Dirty_Oxygen", defaultValue: cleanOxygen * defaultDirtyFactor);
		}
	}
}
