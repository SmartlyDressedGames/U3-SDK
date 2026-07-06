////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Modules;

namespace SDG.Unturned
{
	public class UnturnedNexus : IModuleNexus
	{
		/// <summary>
		/// Register all built-in asset and useable types.
		/// </summary>
		public void initialize()
		{
			Assets.assetTypes.addType("Hat", typeof(ItemHatAsset));
			Assets.assetTypes.addType("Pants", typeof(ItemPantsAsset));
			Assets.assetTypes.addType("Shirt", typeof(ItemShirtAsset));
			Assets.assetTypes.addType("Backpack", typeof(ItemBackpackAsset));
			Assets.assetTypes.addType("Vest", typeof(ItemVestAsset));
			Assets.assetTypes.addType("Mask", typeof(ItemMaskAsset));
			Assets.assetTypes.addType("Glasses", typeof(ItemGlassesAsset));
			Assets.assetTypes.addType("Gun", typeof(ItemGunAsset));
			Assets.assetTypes.addType("Sight", typeof(ItemSightAsset));
			Assets.assetTypes.addType("Tactical", typeof(ItemTacticalAsset));
			Assets.assetTypes.addType("Grip", typeof(ItemGripAsset));
			Assets.assetTypes.addType("Barrel", typeof(ItemBarrelAsset));
			Assets.assetTypes.addType("Magazine", typeof(ItemMagazineAsset));
			Assets.assetTypes.addType("Food", typeof(ItemFoodAsset));
			Assets.assetTypes.addType("Water", typeof(ItemWaterAsset));
			Assets.assetTypes.addType("Medical", typeof(ItemMedicalAsset));
			Assets.assetTypes.addType("Melee", typeof(ItemMeleeAsset));
			Assets.assetTypes.addType("Fuel", typeof(ItemFuelAsset));
			Assets.assetTypes.addType("Tool", typeof(ItemToolAsset));
			Assets.assetTypes.addType("Vehicle_Repair_Tool", typeof(ItemVehicleRepairToolAsset));
			Assets.assetTypes.addType("Barricade", typeof(ItemBarricadeAsset));
			Assets.assetTypes.addType("Storage", typeof(ItemStorageAsset));
			Assets.assetTypes.addType("Tank", typeof(ItemTankAsset));
			Assets.assetTypes.addType("Generator", typeof(ItemGeneratorAsset));
			Assets.assetTypes.addType("Beacon", typeof(ItemBeaconAsset));
			Assets.assetTypes.addType("Farm", typeof(ItemFarmAsset));
			Assets.assetTypes.addType("Trap", typeof(ItemTrapAsset));
			Assets.assetTypes.addType("Structure", typeof(ItemStructureAsset));
			Assets.assetTypes.addType("Supply", typeof(ItemSupplyAsset));
			Assets.assetTypes.addType("Throwable", typeof(ItemThrowableAsset));
			Assets.assetTypes.addType("Grower", typeof(ItemGrowerAsset));
			Assets.assetTypes.addType("Optic", typeof(ItemOpticAsset));
			Assets.assetTypes.addType("Refill", typeof(ItemRefillAsset));
			Assets.assetTypes.addType("Fisher", typeof(ItemFisherAsset));
			Assets.assetTypes.addType("Cloud", typeof(ItemCloudAsset));
			Assets.assetTypes.addType("Map", typeof(ItemMapAsset));
			Assets.assetTypes.addType("Compass", typeof(ItemMapAsset));
			Assets.assetTypes.addType("Key", typeof(ItemKeyAsset));
			Assets.assetTypes.addType("Box", typeof(ItemBoxAsset));
			Assets.assetTypes.addType("Arrest_Start", typeof(ItemArrestStartAsset));
			Assets.assetTypes.addType("Arrest_End", typeof(ItemArrestEndAsset));
			Assets.assetTypes.addType("Detonator", typeof(ItemDetonatorAsset));
			Assets.assetTypes.addType("Charge", typeof(ItemChargeAsset));
			Assets.assetTypes.addType("Library", typeof(ItemLibraryAsset));
			Assets.assetTypes.addType("Filter", typeof(ItemFilterAsset));
			Assets.assetTypes.addType("Sentry", typeof(ItemSentryAsset));
			Assets.assetTypes.addType("Tire", typeof(ItemTireAsset));
			Assets.assetTypes.addType("Oil_Pump", typeof(ItemOilPumpAsset));
			Assets.assetTypes.addType("Vehicle_Paint_Tool", typeof(ItemVehiclePaintToolAsset));
			Assets.assetTypes.addType("Vehicle_Lockpick_Tool", typeof(ItemVehicleLockpickToolAsset));

			Assets.assetTypes.addType("Effect", typeof(EffectAsset));
			Assets.assetTypes.addType("Large", typeof(ObjectAsset));
			Assets.assetTypes.addType("Medium", typeof(ObjectAsset));
			Assets.assetTypes.addType("Small", typeof(ObjectAsset));
			Assets.assetTypes.addType("NPC", typeof(ObjectNPCAsset));
			Assets.assetTypes.addType("Decal", typeof(ObjectAsset));
			Assets.assetTypes.addType("Resource", typeof(ResourceAsset));
			Assets.assetTypes.addType("Vehicle", typeof(VehicleAsset));
			Assets.assetTypes.addType("Animal", typeof(AnimalAsset));
			Assets.assetTypes.addType("Mythic", typeof(MythicAsset));
			Assets.assetTypes.addType("Skin", typeof(SkinAsset));
			Assets.assetTypes.addType("Spawn", typeof(SpawnAsset));
			Assets.assetTypes.addType("Dialogue", typeof(DialogueAsset));
			Assets.assetTypes.addType("Quest", typeof(QuestAsset));
			Assets.assetTypes.addType("Vendor", typeof(VendorAsset));
			Assets.assetTypes.addType("RewardsList", typeof(NPCRewardsAsset));
			Assets.assetTypes.addType("Redirector", typeof(RedirectorAsset));
			Assets.assetTypes.addType("ServerCuration", typeof(ServerListCurationAsset));
			Assets.assetTypes.addType("Tag", typeof(TagAsset));
			Assets.assetTypes.addType("Road", typeof(RoadAsset));

			Assets.useableTypes.addType("Barricade", typeof(UseableBarricade));
			Assets.useableTypes.addType("Battery_Vehicle", typeof(UseableVehicleBattery));
			Assets.useableTypes.addType("Carjack", typeof(UseableCarjack));
			Assets.useableTypes.addType("Clothing", typeof(UseableClothing));
			Assets.useableTypes.addType("Consumeable", typeof(UseableConsumeable));
			Assets.useableTypes.addType("Fisher", typeof(UseableFisher));
			Assets.useableTypes.addType("Fuel", typeof(UseableFuel));
			Assets.useableTypes.addType("Grower", typeof(UseableGrower));
			Assets.useableTypes.addType("Gun", typeof(UseableGun));
			Assets.useableTypes.addType("Melee", typeof(UseableMelee));
			Assets.useableTypes.addType("Optic", typeof(UseableOptic));
			Assets.useableTypes.addType("Refill", typeof(UseableRefill));
			Assets.useableTypes.addType("Structure", typeof(UseableStructure));
			Assets.useableTypes.addType("Throwable", typeof(UseableThrowable));
			Assets.useableTypes.addType("Tire", typeof(UseableTire));
			Assets.useableTypes.addType("Cloud", typeof(UseableCloud));
			Assets.useableTypes.addType("Arrest_Start", typeof(UseableArrestStart));
			Assets.useableTypes.addType("Arrest_End", typeof(UseableArrestEnd));
			Assets.useableTypes.addType("Detonator", typeof(UseableDetonator));
			Assets.useableTypes.addType("Filter", typeof(UseableFilter));
			Assets.useableTypes.addType("Carlockpick", typeof(UseableCarlockpick));
			Assets.useableTypes.addType("Walkie_Talkie", typeof(UseableWalkieTalkie));
			Assets.useableTypes.addType("Housing_Planner", typeof(UseableHousingPlanner));
			Assets.useableTypes.addType("Vehicle_Paint", typeof(UseableVehiclePaint));
		}

		public void shutdown()
		{
			Assets.assetTypes.removeType("Hat");
			Assets.assetTypes.removeType("Pants");
			Assets.assetTypes.removeType("Shirt");
			Assets.assetTypes.removeType("Backpack");
			Assets.assetTypes.removeType("Vest");
			Assets.assetTypes.removeType("Mask");
			Assets.assetTypes.removeType("Glasses");
			Assets.assetTypes.removeType("Gun");
			Assets.assetTypes.removeType("Sight");
			Assets.assetTypes.removeType("Tactical");
			Assets.assetTypes.removeType("Grip");
			Assets.assetTypes.removeType("Barrel");
			Assets.assetTypes.removeType("Magazine");
			Assets.assetTypes.removeType("Food");
			Assets.assetTypes.removeType("Water");
			Assets.assetTypes.removeType("Medical");
			Assets.assetTypes.removeType("Melee");
			Assets.assetTypes.removeType("Fuel");
			Assets.assetTypes.removeType("Tool");
			Assets.assetTypes.removeType("Barricade");
			Assets.assetTypes.removeType("Storage");
			Assets.assetTypes.removeType("Tank");
			Assets.assetTypes.removeType("Generator");
			Assets.assetTypes.removeType("Beacon");
			Assets.assetTypes.removeType("Farm");
			Assets.assetTypes.removeType("Trap");
			Assets.assetTypes.removeType("Structure");
			Assets.assetTypes.removeType("Supply");
			Assets.assetTypes.removeType("Throwable");
			Assets.assetTypes.removeType("Grower");
			Assets.assetTypes.removeType("Optic");
			Assets.assetTypes.removeType("Refill");
			Assets.assetTypes.removeType("Fisher");
			Assets.assetTypes.removeType("Cloud");
			Assets.assetTypes.removeType("Map");
			Assets.assetTypes.removeType("Compass");
			Assets.assetTypes.removeType("Key");
			Assets.assetTypes.removeType("Box");
			Assets.assetTypes.removeType("Arrest_Start");
			Assets.assetTypes.removeType("Arrest_End");
			Assets.assetTypes.removeType("Detonator");
			Assets.assetTypes.removeType("Charge");
			Assets.assetTypes.removeType("Library");
			Assets.assetTypes.removeType("Filter");
			Assets.assetTypes.removeType("Sentry");

			Assets.assetTypes.removeType("Effect");
			Assets.assetTypes.removeType("Large");
			Assets.assetTypes.removeType("Medium");
			Assets.assetTypes.removeType("Small");
			Assets.assetTypes.removeType("NPC");
			Assets.assetTypes.removeType("Resource");
			Assets.assetTypes.removeType("Vehicle");
			Assets.assetTypes.removeType("Animal");
			Assets.assetTypes.removeType("Mythic");
			Assets.assetTypes.removeType("Skin");
			Assets.assetTypes.removeType("Spawn");
			Assets.assetTypes.removeType("Dialogue");
			Assets.assetTypes.removeType("Quest");
			Assets.assetTypes.removeType("Vendor");
			Assets.assetTypes.removeType("RewardsList");
			Assets.assetTypes.removeType("Redirector");
			Assets.assetTypes.removeType("ServerCuration");
			Assets.assetTypes.removeType("Tag");
			Assets.assetTypes.removeType("Road");

			Assets.useableTypes.removeType("Barricade");
			Assets.useableTypes.removeType("Carjack");
			Assets.useableTypes.removeType("Clothing");
			Assets.useableTypes.removeType("Consumeable");
			Assets.useableTypes.removeType("Fisher");
			Assets.useableTypes.removeType("Fuel");
			Assets.useableTypes.removeType("Grower");
			Assets.useableTypes.removeType("Gun");
			Assets.useableTypes.removeType("Melee");
			Assets.useableTypes.removeType("Optic");
			Assets.useableTypes.removeType("Refill");
			Assets.useableTypes.removeType("Structure");
			Assets.useableTypes.removeType("Throwable");
			Assets.useableTypes.removeType("Cloud");
			Assets.useableTypes.removeType("Arrest_Start");
			Assets.useableTypes.removeType("Arrest_End");
			Assets.useableTypes.removeType("Detonator");
			Assets.useableTypes.removeType("Filter");
			Assets.useableTypes.removeType("Carlockpick");
			Assets.useableTypes.removeType("Walkie_Talkie");
		}
	}
}
