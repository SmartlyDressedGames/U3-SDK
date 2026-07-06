////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuSurvivorsClothingBoxUI
	{
		/// <summary>
		/// Internal struct menu uses to sort items in box.
		/// </summary>
		private struct BoxEntry
		{
			/// <summary>
			/// Item definition id.
			/// </summary>
			public int id;

			/// <summary>
			/// Rarity used to sort mythical > legendary > epic > rare.
			/// </summary>
			public EItemRarity rarity;

			/// <summary>
			/// [0, 1] calculated chance of this item being unboxed.
			/// Shown to player in item tooltips.
			/// </summary>
			public float probability;
		}

		/// <summary>
		/// Sorts box entries from highest to lowest rarity.
		/// </summary>
		private class BoxEntryComparer : Comparer<BoxEntry>
		{
			public override int Compare(BoxEntry x, BoxEntry y)
			{
				int rarityComparison = x.rarity.CompareTo(y.rarity);
				if (rarityComparison == 0)
				{
					string name_x = Provider.provider.economyService.getInventoryName(x.id);
					string name_y = Provider.provider.economyService.getInventoryName(y.id);
					return -name_x.CompareTo(name_y); // Negative to match website view.
				}
				else
				{
					return -rarityComparison; // Negative to match website view.
				}
			}
		}

		private static Dictionary<EItemRarity, float> qualityRarities = new Dictionary<EItemRarity, float>
		{
			{ EItemRarity.RARE, 0.75f },
			{ EItemRarity.EPIC, 0.20f },
			{ EItemRarity.LEGENDARY, 0.05f },
			{ EItemRarity.MYTHICAL, 0.03f },
		};
		private static readonly float BONUS_ITEM_RARITY = 0.1f;

		/// <summary>
		/// Format qualityRarities as ##.#
		/// Does not use 'P' format because localized strings unfortunately already had % sign.
		/// </summary>
		private static string formatQualityRarity(EItemRarity rarity)
		{
			return (qualityRarities[rarity] * 100.0f).ToString("0.0");
		}

		private static IconsBundle icons;
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		public static bool isUnboxing;
		private static float lastUnbox; // started unboxing
		private static float lastAngle; // last angle change
		private static float angle; // visual angle
		private static int lastRotation; // last rotation value
		private static int rotation; // current rotation value
		private static int target; // target rotation, -1 until dropped

		private static int item;
		private static ulong instance;

		/// <summary>
		/// Items server told us we unboxed, but we wait for the animation to finish before showing.
		/// Typically one, but some newer boxes have bonus items occassionally.
		/// </summary>
		private static List<SteamItemDetails_t> unboxedItems;

		/// <summary>
		/// Is one of the unboxed items mythical rarity?
		/// </summary>
		private static bool didUnboxMythical;

		/// <summary>
		/// Items in the box.
		/// </summary>
		private static List<BoxEntry> boxEntries;
		private static int numBoxEntries;

		private static ItemBoxAsset boxAsset;
		private static ItemKeyAsset keyAsset;

		private static float size;

		private static ISleekConstraintFrame inventory;

		private static ISleekBox finalBox;
		private static SleekInventory boxButton;
		private static SleekButtonIcon keyButton;
		private static SleekButtonIcon unboxButton;
		private static ISleekBox disabledBox;

		private static ISleekLabel rareLabel;
		private static ISleekLabel epicLabel;
		private static ISleekLabel legendaryLabel;
		private static ISleekLabel mythicalLabel;
		private static ISleekLabel equalizedLabel;
		private static ISleekLabel bonusLabel;

		private static SleekInventory[] dropButtons;

		/// <summary>
		/// Skip unboxing animation.
		/// Initial call rotates to just before the item, next call skips entirely.
		/// </summary>
		public static void skipAnimation()
		{
			if (isUnboxing == false)
				return;

			if (target == -1)
				return; // Target rotation is unknown.

			if (rotation == target)
			{
				// Waiting for timer to elapse (0.5s), so we advance time.
				lastAngle -= 1.0f;
			}
			else
			{
				float skipThreshold = Mathf.PI / 2.0f;
				float targetAngle = target / (float) numBoxEntries * Mathf.PI * 2.0f;
				if (angle > targetAngle - skipThreshold)
				{
					// Almost at target, so we skip the reached item animation.
					rotation = target;
					lastAngle -= 1.0f;
				}
				else
				{
					// Skip close to final angle. Player can press escape again to skip entirely.
					angle = targetAngle - Random.Range(skipThreshold / 2.0f, skipThreshold);
				}
			}
		}

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(0, 1);
		}

		public static void viewItem(int newItem, ushort newQuantity, ulong newInstance)
		{
			item = newItem;
			instance = newInstance;

			unboxedItems = null;
			didUnboxMythical = false;

			angle = 0;
			lastRotation = 0;
			rotation = 0;
			target = -1;

			disabledBox.IsVisible = false;
			keyButton.IsVisible = true;
			unboxButton.IsVisible = true;
			//finalBox.isVisible = false;

			boxButton.updateInventory(instance, item, newQuantity, false, true);

			boxAsset = Assets.find<ItemBoxAsset>(Provider.provider.economyService.getInventoryItemGuid(item));

			if (boxAsset != null)
			{
				organizeBoxEntries();
				synchronizeTotalProbabilities();

				string textKey = null;
				string tooltipKey = null;
				switch (boxAsset.itemOrigin)
				{
					case EBoxItemOrigin.Unbox:
						textKey = "Unbox_Text";
						tooltipKey = "Unbox_Tooltip";
						break;
					case EBoxItemOrigin.Unwrap:
						textKey = "Unwrap_Text";
						tooltipKey = "Unwrap_Tooltip";
						break;
				}

				if (!Provider.provider.economyService.doesCountryAllowRandomItems)
				{
					if (Provider.provider.economyService.hasCountryDetails)
					{
						disabledBox.IsVisible = true;
						disabledBox.Text = localization.format("Region_Disabled", Provider.provider.economyService.getCountryWarningId());
					}
					else
					{
						// Show nothing. The player might simply be offline.
						disabledBox.IsVisible = false;
					}

					unboxButton.IsVisible = false;
					keyButton.IsVisible = false;
				}
				else if (boxAsset.destroy == 0)
				{
					keyButton.IsVisible = false;

					unboxButton.icon = null;
					unboxButton.PositionOffset_X = 0;
					unboxButton.PositionScale_X = 0.3f;
					unboxButton.SizeOffset_X = 0;
					unboxButton.SizeScale_X = 0.4f;
					unboxButton.text = localization.format(textKey);
					unboxButton.tooltip = localization.format(tooltipKey);
					unboxButton.IsVisible = true;

					keyAsset = null;
				}
				else
				{
					keyButton.IsVisible = true;

					unboxButton.icon = icons.load<Texture2D>("Unbox");
					unboxButton.PositionOffset_X = 5;
					unboxButton.PositionScale_X = 0.5f;
					unboxButton.SizeOffset_X = -5;
					unboxButton.SizeScale_X = 0.2f;
					unboxButton.text = localization.format(textKey);
					unboxButton.tooltip = localization.format(tooltipKey);
					unboxButton.IsVisible = true;

					keyAsset = Assets.find<ItemKeyAsset>(Provider.provider.economyService.getInventoryItemGuid(boxAsset.destroy));
					if (keyAsset != null)
					{
						keyButton.icon = Provider.provider.economyService.LoadItemIcon(boxAsset.destroy);
					}
				}

				size = Mathf.PI * 2f / numBoxEntries / 2.75f;

				finalBox.PositionScale_Y = 0.5f - (size / 2f);
				finalBox.SizeScale_X = size;
				finalBox.SizeScale_Y = size;

				if (dropButtons != null)
				{
					for (int index = 0; index < dropButtons.Length; index++)
					{
						inventory.RemoveChild(dropButtons[index]);
					}
				}

				dropButtons = new SleekInventory[numBoxEntries];

				for (int index = 0; index < numBoxEntries; index++)
				{
					BoxEntry entry = boxEntries[index];
					float offset = (Mathf.PI * 2f * index / numBoxEntries) + Mathf.PI;

					SleekInventory button = new SleekInventory();
					button.PositionScale_X = 0.5f + (Mathf.Cos(-offset) * (0.5f - (size / 2f))) - (size / 2f);
					button.PositionScale_Y = 0.5f + (Mathf.Sin(-offset) * (0.5f - (size / 2f))) - (size / 2f);
					button.SizeScale_X = size;
					button.SizeScale_Y = size;

					if (entry.probability > -0.5f)
					{
						// Show probability of getting this item as a percentage.
						button.extraTooltip = entry.probability.ToString("P");
					}

					button.updateInventory(0, entry.id, 1, false, false);
					inventory.AddChild(button);

					dropButtons[index] = button;
				}
			}

			Color itemColor = Provider.provider.economyService.getInventoryColor(item);
			keyButton.backgroundColor = SleekColor.BackgroundIfLight(itemColor);
			keyButton.textColor = itemColor;
			unboxButton.backgroundColor = keyButton.backgroundColor;
			unboxButton.textColor = itemColor;
		}

		private static void synchronizeTotalProbabilities()
		{
			rareLabel.IsVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Original;
			epicLabel.IsVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Original;
			legendaryLabel.IsVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Original;
			equalizedLabel.IsVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Equalized;
			bonusLabel.IsVisible = boxAsset.containsBonusItems;
		}

		private static void organizeBoxEntries()
		{
			int[] potentialItemDefIds = boxAsset.drops;
			int numItems = potentialItemDefIds.Length;
			numBoxEntries = numItems;
			boxEntries = new List<BoxEntry>(numBoxEntries);
			Dictionary<EItemRarity, int> numOfEachRarity = new Dictionary<EItemRarity, int>()
			{
				{ EItemRarity.RARE, 0 },
				{ EItemRarity.EPIC, 0 },
				{ EItemRarity.LEGENDARY, 0 },
				{ EItemRarity.MYTHICAL, 0 },
			};

			for (int index = 0; index < numItems; ++index)
			{
				EItemRarity rarity;
				int itemdefid = potentialItemDefIds[index];

				if (itemdefid < 0)
				{
					rarity = EItemRarity.MYTHICAL;
				}
				else
				{
					rarity = Provider.provider.economyService.getGameRarity(itemdefid);
				}

				numOfEachRarity[rarity]++;

				BoxEntry entry = new BoxEntry
				{
					id = itemdefid,
					rarity = rarity,
					probability = -1.0f
				};
				boxEntries.Add(entry);
			}

			float totalProbability = 0.0f;
			for (int index = 0; index < numItems; ++index)
			{
				BoxEntry entry = boxEntries[index];
				if (entry.rarity == EItemRarity.MYTHICAL)
				{
					// Does not contribute to total because mythical is affected by probability of legendary/epic/rare.
					entry.probability = qualityRarities[EItemRarity.MYTHICAL];
				}
				else
				{
					if (boxAsset.probabilityModel == EBoxProbabilityModel.Original)
					{
						int numOfRarity = numOfEachRarity[entry.rarity];
						float rarityProbability = qualityRarities[entry.rarity];
						float individualProbability = rarityProbability / numOfRarity;
						entry.probability = individualProbability;
					}
					else
					{
						int numItemsExcludingMythical = numItems - 1;
						entry.probability = 1.0f / numItemsExcludingMythical;
					}

					totalProbability += entry.probability;
				}
				boxEntries[index] = entry;
			}

			if (Mathf.Abs(totalProbability - 1.0f) > 0.01f) // Is total not approximately 100%?
			{
				UnturnedLog.warn("Unable to guess box probabilities ({0})", totalProbability);

				// We hide probabilities below zero.
				for (int index = 0; index < numItems; ++index)
				{
					BoxEntry entry = boxEntries[index];
					entry.probability = -1.0f;
					boxEntries[index] = entry;
				}
			}

			boxEntries.Sort(new BoxEntryComparer());
		}

		private static void onClickedKeyButton(ISleekElement button)
		{
			ItemStore.Get().ViewItem(boxAsset.destroy);
		}

		private static void onClickedUnboxButton(ISleekElement button)
		{
			if (boxAsset.destroy == 0)
			{
				Provider.provider.economyService.exchangeInventory(boxAsset.generate, new List<EconExchangePair>() { new EconExchangePair(instance, 1) });
			}
			else
			{
				ulong destroyPackage = Provider.provider.economyService.getInventoryPackage(boxAsset.destroy);

				if (destroyPackage == 0)
				{
					return;
				}

				List<EconExchangePair> destroy = new List<EconExchangePair>()
				{
					new EconExchangePair(instance, 1),
					new EconExchangePair(destroyPackage, 1)
				};

				Provider.provider.economyService.exchangeInventory(boxAsset.generate, destroy);
			}

			isUnboxing = true;
			backButton.IsVisible = false;
			lastUnbox = Time.realtimeSinceStartup;
			lastAngle = Time.realtimeSinceStartup;

			keyButton.IsVisible = false;
			unboxButton.IsVisible = false;
			//finalBox.isVisible = true;
		}

		/// <summary>
		/// Does client know about all the granted items?
		/// If not, either something is bad in the econ config (uh oh!) or client is out of date.
		/// </summary>
		private static bool hasAssetsForGrantedItems(List<SteamItemDetails_t> grantedItems)
		{
			foreach (SteamItemDetails_t item in grantedItems)
			{
				System.Guid item_guid;
				System.Guid vehicle_guid;
				Provider.provider.economyService.getInventoryTargetID(item.m_iDefinition.m_SteamItemDef, out item_guid, out vehicle_guid);

				ItemAsset itemAsset = Assets.find<ItemAsset>(item_guid);
				VehicleAsset vehicleAsset = VehicleTool.FindVehicleByGuidAndHandleRedirects(vehicle_guid);

				if (itemAsset == null && vehicleAsset == null)
				{
					return false;
				}
			}

			return true;
		}

		private static bool wasGrantedMythical(List<SteamItemDetails_t> grantedItems)
		{
			foreach (SteamItemDetails_t item in grantedItems)
			{
				ushort mythic_id = Provider.provider.economyService.getInventoryMythicID(item.m_iDefinition.m_SteamItemDef);
				if (mythic_id != 0)
				{
					return true;
				}
			}

			return false;
		}

		private static int getIndexOfGrantedItemInDrops(List<SteamItemDetails_t> grantedItems)
		{
			for (int dropIndex = 1; dropIndex < numBoxEntries; ++dropIndex)
			{
				int dropItemDefId = boxEntries[dropIndex].id;
				foreach (SteamItemDetails_t item in grantedItems)
				{
					if (item.m_iDefinition.m_SteamItemDef == dropItemDefId)
					{
						return dropIndex;
					}
				}
			}

			return -1;
		}

		private static void exchangeErrorAlert(string message)
		{
			isUnboxing = false;
			backButton.IsVisible = true;

			MenuUI.alert(message);

			MenuSurvivorsClothingUI.open();
			close();
		}

		private static void onInventoryExchanged(List<SteamItemDetails_t> grantedItems)
		{
			if (!isUnboxing)
			{
				return;
			}

			bool hasAssets = hasAssetsForGrantedItems(grantedItems);
			if (hasAssets == false)
			{
				exchangeErrorAlert(localization.format("Exchange_Missing_Assets"));
				return;
			}

			MenuSurvivorsClothingUI.updatePage();

			int indexInDrops;
			if (wasGrantedMythical(grantedItems))
			{
				didUnboxMythical = true;
				indexInDrops = 0;
			}
			else
			{
				didUnboxMythical = false;
				indexInDrops = getIndexOfGrantedItemInDrops(grantedItems);

				if (indexInDrops < 0)
				{
					exchangeErrorAlert(localization.format("Exchange_Not_In_Drops"));
					return;
				}
			}

			// We sort mythical (if any) into front of list.
			unboxedItems = grantedItems;
			unboxedItems.Sort(new EconItemRarityComparer());

			if (rotation < numBoxEntries * 2)
			{
				target = (numBoxEntries * 3) + indexInDrops;
			}
			else
			{
				target = (((int) (rotation / (float) numBoxEntries) + 2) * numBoxEntries) + indexInDrops;
			}
		}

		public static void update()
		{
			if (!isUnboxing)
			{
				return;
			}

			if (Time.realtimeSinceStartup - lastUnbox > Provider.CLIENT_TIMEOUT)
			{
				isUnboxing = false;
				backButton.IsVisible = true;

				MenuUI.alert(localization.format("Exchange_Timed_Out"));

				MenuSurvivorsClothingUI.open();
				close();

				return;
			}

			if (rotation == target)
			{
				if (Time.realtimeSinceStartup - lastAngle > 0.5f)
				{
					isUnboxing = false;
					backButton.IsVisible = true;

					string originKey = null;
					switch (boxAsset.itemOrigin)
					{
						case EBoxItemOrigin.Unbox:
							originKey = "Origin_Unbox";
							break;
						case EBoxItemOrigin.Unwrap:
							originKey = "Origin_Unwrap";
							break;
					}
					MenuUI.alertNewItems(localization.format(originKey), unboxedItems);

					SteamItemDetails_t primaryItem = unboxedItems[0];
					MenuSurvivorsClothingItemUI.viewItem(primaryItem.m_iDefinition.m_SteamItemDef, primaryItem.m_unQuantity, primaryItem.m_itemId.m_SteamItemInstanceID);
					MenuSurvivorsClothingItemUI.open();
					close();

					string unboxSoundPath = didUnboxMythical ? "Economy/Sounds/Mythical" : "Economy/Sounds/Unbox";
					MainCamera.instance.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>(unboxSoundPath), 0.66f);
				}
			}
			else
			{
				if (rotation < target - numBoxEntries || target == -1)
				{
					if (angle < Mathf.PI * 4f)
					{
						angle += (Time.realtimeSinceStartup - lastAngle) * size * Mathf.Lerp(80f, 20f, angle / (Mathf.PI * 4f));
					}
					else
					{
						angle += (Time.realtimeSinceStartup - lastAngle) * size * 20f;
					}
				}
				else
				{
					angle += (Time.realtimeSinceStartup - lastAngle) * Mathf.Max((target - (angle / (Mathf.PI * 2f / numBoxEntries))) / numBoxEntries, 0.05f) * size * 20f;
				}

				lastAngle = Time.realtimeSinceStartup;
				rotation = (int) (angle / (Mathf.PI * 2f / numBoxEntries));

				if (rotation == target)
				{
					angle = rotation * (Mathf.PI * 2f / numBoxEntries);
				}

				for (int index = 0; index < numBoxEntries; index++)
				{
					float offset = (Mathf.PI * 2f * +index / numBoxEntries) + Mathf.PI;

					dropButtons[index].PositionScale_X = 0.5f + (Mathf.Cos(angle - offset) * (0.5f - (size / 2f))) - (size / 2f);
					dropButtons[index].PositionScale_Y = 0.5f + (Mathf.Sin(angle - offset) * (0.5f - (size / 2f))) - (size / 2f);
				}

				if (rotation != lastRotation)
				{
					lastRotation = rotation;

					boxButton.PositionScale_Y = 0.25f;
					boxButton.AnimatePositionScale(0.3f, 0.3f, ESleekLerp.EXPONENTIAL, 20);
					finalBox.PositionOffset_X = -20;
					finalBox.PositionOffset_Y = -20;
					finalBox.SizeOffset_X = 40;
					finalBox.SizeOffset_Y = 40;
					finalBox.AnimatePositionOffset(-10, -10, ESleekLerp.EXPONENTIAL, 1);
					finalBox.AnimateSizeOffset(20, 20, ESleekLerp.EXPONENTIAL, 1);

					boxButton.updateInventory(0, boxEntries[rotation % numBoxEntries].id, 1, false, true);

					if (rotation == target)
					{
						MainCamera.instance.GetComponent<AudioSource>().PlayOneShot((AudioClip) Resources.Load("Economy/Sounds/Drop"), 0.33f);
					}
					else
					{
						MainCamera.instance.GetComponent<AudioSource>().PlayOneShot((AudioClip) Resources.Load("Economy/Sounds/Tick"), 0.33f);
					}
				}
			}
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuSurvivorsClothingItemUI.open();
			close();
		}

		public void OnDestroy()
		{
			Provider.provider.economyService.onInventoryExchanged -= onInventoryExchanged;
		}

		public MenuSurvivorsClothingBoxUI()
		{
			localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothingBox.dat");

			icons = Bundles.getIconsBundle("UI/Menu/Icons/Survivors/MenuSurvivorsClothingBox");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			inventory = Glazier.Get().CreateConstraintFrame();
			inventory.PositionScale_X = 0.5f;
			inventory.PositionOffset_Y = 10;
			inventory.SizeScale_X = 0.5f;
			inventory.SizeScale_Y = 1;
			inventory.SizeOffset_Y = -20;
			inventory.Constraint = ESleekConstraint.FitInParent;
			container.AddChild(inventory);

			finalBox = Glazier.Get().CreateBox();
			finalBox.PositionOffset_X = -10;
			finalBox.PositionOffset_Y = -10;
			finalBox.SizeOffset_X = 20;
			finalBox.SizeOffset_Y = 20;
			//finalBox.backgroundColor.a = 0.5f;
			inventory.AddChild(finalBox);
			//finalBox.isVisible = false;

			boxButton = new SleekInventory();
			boxButton.PositionOffset_Y = -30;
			boxButton.PositionScale_X = 0.3f;
			boxButton.PositionScale_Y = 0.3f;
			boxButton.SizeScale_X = 0.4f;
			boxButton.SizeScale_Y = 0.4f;
			inventory.AddChild(boxButton);

			keyButton = new SleekButtonIcon(null, 40);
			keyButton.PositionOffset_Y = -20;
			keyButton.PositionScale_X = 0.3f;
			keyButton.PositionScale_Y = 0.7f;
			keyButton.SizeOffset_X = -5;
			keyButton.SizeOffset_Y = 50;
			keyButton.SizeScale_X = 0.2f;
			keyButton.text = localization.format("Key_Text");
			keyButton.tooltip = localization.format("Key_Tooltip");
			keyButton.onClickedButton += onClickedKeyButton;
			keyButton.fontSize = ESleekFontSize.Medium;
			keyButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
			inventory.AddChild(keyButton);
			keyButton.IsVisible = false;

			unboxButton = new SleekButtonIcon(null);
			unboxButton.PositionOffset_X = 5;
			unboxButton.PositionOffset_Y = -20;
			unboxButton.PositionScale_X = 0.5f;
			unboxButton.PositionScale_Y = 0.7f;
			unboxButton.SizeOffset_X = -5;
			unboxButton.SizeOffset_Y = 50;
			unboxButton.SizeScale_X = 0.2f;
			unboxButton.text = localization.format("Unbox_Text");
			unboxButton.tooltip = localization.format("Unbox_Tooltip");
			unboxButton.onClickedButton += onClickedUnboxButton;
			unboxButton.fontSize = ESleekFontSize.Medium;
			unboxButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
			inventory.AddChild(unboxButton);
			unboxButton.IsVisible = false;

			disabledBox = Glazier.Get().CreateBox();
			disabledBox.PositionOffset_Y = -20;
			disabledBox.PositionScale_X = 0.3f;
			disabledBox.PositionScale_Y = 0.7f;
			disabledBox.SizeOffset_Y = 50;
			disabledBox.SizeScale_X = 0.4f;
			inventory.AddChild(disabledBox);
			disabledBox.IsVisible = false;

			rareLabel = Glazier.Get().CreateLabel();
			rareLabel.PositionOffset_X = 50;
			rareLabel.PositionOffset_Y = 50;
			rareLabel.SizeOffset_X = 200;
			rareLabel.SizeOffset_Y = 30;
			rareLabel.Text = localization.format("Rarity_Rare", formatQualityRarity(EItemRarity.RARE));
			rareLabel.TextColor = ItemTool.getRarityColorUI(EItemRarity.RARE);
			rareLabel.TextAlignment = TextAnchor.MiddleLeft;
			rareLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			container.AddChild(rareLabel);

			epicLabel = Glazier.Get().CreateLabel();
			epicLabel.PositionOffset_X = 50;
			epicLabel.PositionOffset_Y = 70;
			epicLabel.SizeOffset_X = 200;
			epicLabel.SizeOffset_Y = 30;
			epicLabel.Text = localization.format("Rarity_Epic", formatQualityRarity(EItemRarity.EPIC));
			epicLabel.TextColor = ItemTool.getRarityColorUI(EItemRarity.EPIC);
			epicLabel.TextAlignment = TextAnchor.MiddleLeft;
			epicLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			container.AddChild(epicLabel);

			legendaryLabel = Glazier.Get().CreateLabel();
			legendaryLabel.PositionOffset_X = 50;
			legendaryLabel.PositionOffset_Y = 90;
			legendaryLabel.SizeOffset_X = 200;
			legendaryLabel.SizeOffset_Y = 30;
			legendaryLabel.Text = localization.format("Rarity_Legendary", formatQualityRarity(EItemRarity.LEGENDARY));
			legendaryLabel.TextColor = ItemTool.getRarityColorUI(EItemRarity.LEGENDARY);
			legendaryLabel.TextAlignment = TextAnchor.MiddleLeft;
			legendaryLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			container.AddChild(legendaryLabel);

			mythicalLabel = Glazier.Get().CreateLabel();
			mythicalLabel.PositionOffset_X = 50;
			mythicalLabel.PositionOffset_Y = 110;
			mythicalLabel.SizeOffset_X = 200;
			mythicalLabel.SizeOffset_Y = 30;
			mythicalLabel.Text = localization.format("Rarity_Mythical", formatQualityRarity(EItemRarity.MYTHICAL));
			mythicalLabel.TextColor = ItemTool.getRarityColorUI(EItemRarity.MYTHICAL);
			mythicalLabel.TextAlignment = TextAnchor.MiddleLeft;
			mythicalLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			container.AddChild(mythicalLabel);

			equalizedLabel = Glazier.Get().CreateLabel();
			equalizedLabel.PositionOffset_X = 50;
			equalizedLabel.PositionOffset_Y = 50;
			equalizedLabel.SizeOffset_X = 200;
			equalizedLabel.SizeOffset_Y = 30;
			equalizedLabel.Text = localization.format("Rarity_Equalized");
			equalizedLabel.TextAlignment = TextAnchor.MiddleLeft;
			equalizedLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			container.AddChild(equalizedLabel);

			bonusLabel = Glazier.Get().CreateLabel();
			bonusLabel.PositionOffset_X = 50;
			bonusLabel.PositionOffset_Y = 130;
			bonusLabel.SizeOffset_X = 200;
			bonusLabel.SizeOffset_Y = 30;
			bonusLabel.Text = localization.format("Rarity_Bonus_Items", (BONUS_ITEM_RARITY * 100.0f).ToString("0.0"));
			bonusLabel.TextAlignment = TextAnchor.MiddleLeft;
			bonusLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			container.AddChild(bonusLabel);

			dropButtons = null;

			Provider.provider.economyService.onInventoryExchanged += onInventoryExchanged;

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);
		}
	}
}
