////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned
{
	public enum EMenuSurvivorsClothingInspectUIOpenContext
	{
		OwnedItem,
		ItemStoreDetailsMenu,
		ItemStoreBundleContents,
	}

	public class MenuSurvivorsClothingInspectUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static ISleekConstraintFrame inventory;
		private static SleekCameraImage image;
		private static ISleekSlider slider;

		private static ISleekToggle previewOnCharacterToggle;
		private static ISleekToggle previewSoloToggle;

		private static int item;
		private static ulong instance;
		private static EMenuSurvivorsClothingInspectUIOpenContext openContext;

		private static Transform inspect;
		private static Transform model;
		private static ItemLook look;
		private static Camera camera;

		public static void open(EMenuSurvivorsClothingInspectUIOpenContext openContext)
		{
			if (active)
			{
				return;
			}

			active = true;
			MenuSurvivorsClothingInspectUI.openContext = openContext;

			camera.gameObject.SetActive(true);
			look._yaw = Characters.characterYaw;
			look.yaw = Characters.characterYaw;
			slider.Value = Characters.characterYaw / 360.0f;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			Characters.previewItemDefId = 0;
			Characters.previewItemSolo = false;
			Characters.RefreshPreviewCharacterModel();

			camera.gameObject.SetActive(false);

			container.AnimateOutOfView(0, 1);
		}

		public static void OpenPreviousMenu()
		{
			switch (openContext)
			{
				case EMenuSurvivorsClothingInspectUIOpenContext.OwnedItem:
					MenuSurvivorsClothingItemUI.open();
					break;

				case EMenuSurvivorsClothingInspectUIOpenContext.ItemStoreDetailsMenu:
					ItemStoreDetailsMenu.instance.OpenCurrentListing();
					break;

				case EMenuSurvivorsClothingInspectUIOpenContext.ItemStoreBundleContents:
					ItemStoreBundleContentsMenu.instance.OpenCurrentListing();
					break;
			}
		}

		private static bool getInspectedItemStatTrackerValue(out EStatTrackerType type, out int kills)
		{
			return Provider.provider.economyService.getInventoryStatTrackerValue(instance, out type, out kills);
		}

		public static void viewItem(int newItem, ulong newInstance)
		{
			item = newItem;
			instance = newInstance;

			if (model != null)
			{
				Object.Destroy(model.gameObject);
				model = null;
			}

			System.Guid itemGuid;
			System.Guid vehicleGuid;
			Provider.provider.economyService.getInventoryTargetID(item, out itemGuid, out vehicleGuid);
			ushort skinID = Provider.provider.economyService.getInventorySkinID(item);
			ushort mythicID = Provider.provider.economyService.getInventoryMythicID(item);
			if (mythicID == 0 && instance != 0)
			{
				mythicID = Provider.provider.economyService.getInventoryParticleEffect(instance);
			}
			ItemAsset itemAsset = Assets.find<ItemAsset>(itemGuid);
			VehicleAsset vehicleAsset = VehicleTool.FindVehicleByGuidAndHandleRedirects(vehicleGuid);

			if (itemAsset is ItemClothingAsset)
			{
				previewOnCharacterToggle.IsVisible = true;
				previewSoloToggle.IsVisible = true;
				ApplyPreview();
			}
			else
			{
				previewOnCharacterToggle.IsVisible = false;
				previewSoloToggle.IsVisible = false;
				Characters.previewItemDefId = 0;
				Characters.previewItemSolo = false;
				Characters.RefreshPreviewCharacterModel();
			}

			if (itemAsset == null && vehicleAsset == null)
			{
				return;
			}

			// Nelson 2024-12-11: Yeah this feels messy. Sorry!
			bool isModelClothingPrefab = false;

			if (skinID != 0)
			{
				SkinAsset skinAsset = Assets.find(EAssetType.SKIN, skinID) as SkinAsset;

				if (vehicleAsset != null)
				{
					model = VehicleTool.getVehicle(vehicleAsset.id, skinAsset.id, mythicID, vehicleAsset, skinAsset);
				}
				else
				{
					model = ItemTool.getItem(itemAsset.id, skinID, 100, itemAsset.getState(), false, itemAsset, skinAsset, getInspectedItemStatTrackerValue);

					if (mythicID != 0)
					{
						ItemTool.ApplyMythicalEffect(model, mythicID, EEffectType.THIRD);
					}
				}
			}
			else
			{
				// Nelson 2024-12-11: Some cosmetics' Item prefab isn't ideal for viewing in this window, so I'm making
				// it optional and fall back to the actual clothing model instead.
				if (itemAsset.item == null && itemAsset is ItemClothingAsset clothingAsset)
				{
					switch (clothingAsset.type)
					{
						case EItemType.SHIRT:
						{
							// Orange Hoodie
							model = ItemTool.getItem(3, 0, 100, itemAsset.getState(), false, getInspectedItemStatTrackerValue);
							ItemShirtAsset shirtAsset = (ItemShirtAsset) itemAsset;
							Material shirtMaterial = new Material(Shader.Find("Standard"));
							shirtMaterial.mainTexture = shirtAsset.shirt;
							shirtMaterial.EnableKeyword("_ALPHATEST_ON");
							shirtMaterial.SetFloat("_Mode", 1);

							if (shirtAsset.metallic != null)
							{
								shirtMaterial.EnableKeyword("_METALLICGLOSSMAP");
								shirtMaterial.SetTexture("_MetallicGlossMap", shirtAsset.metallic);
								shirtMaterial.SetFloat("_Glossiness", 1f);
							}
							else
							{
								shirtMaterial.SetFloat("_Glossiness", 0f);
							}

							if (shirtAsset.emission != null)
							{
								shirtMaterial.EnableKeyword("_EMISSION");
								shirtMaterial.SetTexture("_EmissionMap", shirtAsset.emission);
								shirtMaterial.SetColor("_EmissionColor", new Color(2f, 2f, 2f));
							}

							model.GetComponent<Renderer>().material = shirtMaterial;
							model.gameObject.AddComponent<DestroyMaterialOnDestroy>().instantiatedMaterial = shirtMaterial;
							break; 
						}

						case EItemType.PANTS:
						{
							// Work Jeans
							model = ItemTool.getItem(2, 0, 100, itemAsset.getState(), false, getInspectedItemStatTrackerValue);

							ItemPantsAsset pantsAsset = (ItemPantsAsset) itemAsset;
							Material pantsMaterial = new Material(Shader.Find("Standard"));
							pantsMaterial.mainTexture = pantsAsset.pants;
							pantsMaterial.EnableKeyword("_ALPHATEST_ON");
							pantsMaterial.SetFloat("_Mode", 1);

							if (pantsAsset.metallic != null)
							{
								pantsMaterial.EnableKeyword("_METALLICGLOSSMAP");
								pantsMaterial.SetTexture("_MetallicGlossMap", pantsAsset.metallic);
								pantsMaterial.SetFloat("_Glossiness", 1f);
							}
							else
							{
								pantsMaterial.SetFloat("_Glossiness", 0f);
							}

							if (pantsAsset.emission != null)
							{
								pantsMaterial.EnableKeyword("_EMISSION");
								pantsMaterial.SetTexture("_EmissionMap", pantsAsset.emission);
								pantsMaterial.SetColor("_EmissionColor", new Color(2f, 2f, 2f));
							}

							model.GetComponent<Renderer>().material = pantsMaterial;
							model.gameObject.AddComponent<DestroyMaterialOnDestroy>().instantiatedMaterial = pantsMaterial;
							break;
						}

						default:
						{
							GameObject prefab = clothingAsset.ClothingPrefab;
							if (prefab != null)
							{
								GameObject clothingInstance = Object.Instantiate(prefab);
								if (clothingInstance != null)
								{
									model = clothingInstance.transform;
									isModelClothingPrefab = true;
								}
							}
							break;
						}
					}
				}

				if (model == null)
				{
					model = ItemTool.getItem(itemAsset.id, 0, 100, itemAsset.getState(), false, itemAsset, getInspectedItemStatTrackerValue);
				}

				if (mythicID != 0)
				{
					EEffectType effectType = itemAsset.type == EItemType.BACKPACK || itemAsset.type == EItemType.VEST
						? EEffectType.BODY_COSMETIC : EEffectType.HEAD_COSMETIC;
					ItemTool.ApplyMythicalEffect(model, mythicID, effectType);
				}
			}

			model.parent = inspect;
			model.localPosition = Vector3.zero;

			if (isModelClothingPrefab)
			{
				model.localRotation = Quaternion.Euler(0, 180, -90);
			}
			else if (vehicleAsset != null)
			{
				model.localRotation = Quaternion.identity;
			}
			else if (itemAsset != null && itemAsset.type == EItemType.MELEE)
			{
				model.localRotation = Quaternion.Euler(0, -90, 90);
			}
			else
			{
				model.localRotation = Quaternion.Euler(-90, 0, 0);
			}

			look.target = model.gameObject;
		}

		private static void ApplyPreview()
		{
			Characters.previewItemDefId = previewOnCharacterToggle.Value ? item : 0;
			Characters.previewItemSolo = previewOnCharacterToggle.Value && previewSoloToggle.Value;
			Characters.RefreshPreviewCharacterModel();
		}

		private static void OnPreviewOnCharacterToggled(ISleekToggle toggle, bool value)
		{
			previewSoloToggle.IsInteractable = value;
			ApplyPreview();
		}

		private static void OnPreviewSoloToggled(ISleekToggle toggle, bool value)
		{
			ApplyPreview();
		}

		private static void onDraggedSlider(ISleekSlider slider, float state)
		{
			look.yaw = state * 360f;
			Characters.characterYaw = look.yaw;
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			OpenPreviousMenu();
			close();
		}

		public MenuSurvivorsClothingInspectUI()
		{
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
			inventory.PositionScale_Y = 0.125f;
			inventory.SizeScale_X = 0.5f;
			inventory.SizeScale_Y = 0.75f;
			inventory.SizeOffset_Y = -20;
			inventory.Constraint = ESleekConstraint.FitInParent;
			container.AddChild(inventory);

			ISleekBox imageBox = Glazier.Get().CreateBox();
			imageBox.SizeScale_X = 1;
			imageBox.SizeScale_Y = 1;
			inventory.AddChild(imageBox);

			image = new SleekCameraImage();
			image.SizeScale_X = 1;
			image.SizeScale_Y = 1;
			imageBox.AddChild(image);

			previewOnCharacterToggle = Glazier.Get().CreateToggle();
			previewOnCharacterToggle.PositionOffset_X = 10;
			previewOnCharacterToggle.PositionOffset_Y = 10;
			previewOnCharacterToggle.AddLabel(MenuSurvivorsClothingUI.localization.format("PreviewOnCharacterLabel"), ESleekSide.RIGHT);
			previewOnCharacterToggle.OnValueChanged += OnPreviewOnCharacterToggled;
			inventory.AddChild(previewOnCharacterToggle);

			previewSoloToggle = Glazier.Get().CreateToggle();
			previewSoloToggle.PositionOffset_X = 10;
			previewSoloToggle.PositionOffset_Y = 60;
			previewSoloToggle.AddLabel(MenuSurvivorsClothingUI.localization.format("PreviewSoloLabel"), ESleekSide.RIGHT);
			previewSoloToggle.OnValueChanged += OnPreviewSoloToggled;
			previewSoloToggle.IsInteractable = false;
			inventory.AddChild(previewSoloToggle);

			slider = Glazier.Get().CreateSlider();
			slider.PositionOffset_Y = 10;
			slider.PositionScale_Y = 1;
			slider.SizeOffset_Y = 20;
			slider.SizeScale_X = 1;
			slider.Orientation = ESleekOrientation.HORIZONTAL;
			slider.OnValueChanged += onDraggedSlider;
			imageBox.AddChild(slider);

			inspect = GameObject.Find("Inspect").transform;
			look = inspect.GetComponent<ItemLook>();
			camera = look.inspectCamera;
			image.SetCamera(camera);

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
