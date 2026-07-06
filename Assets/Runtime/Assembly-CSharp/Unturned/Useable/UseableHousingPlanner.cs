////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetTransport;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class UseableHousingPlanner : Useable
	{
		public override bool isUseableShowingMenu => isItemSelectionMenuOpen;

		private static MasterBundleReference<OneShotAudioDefinition> popupAudioRef = new MasterBundleReference<OneShotAudioDefinition>("core.masterbundle", "Sounds/Popup/Popup.asset");
		private static MasterBundleReference<AudioClip> errorClipRef = new MasterBundleReference<AudioClip>("core.masterbundle", "Sounds/Error.wav");

		private static readonly ClientInstanceMethod<bool> SendPlaceHousingItemResult = ClientInstanceMethod<bool>.Get(typeof(UseableHousingPlanner), nameof(ReceivePlaceHousingItemResult));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceivePlaceHousingItemResult(bool success)
		{
			if (success)
			{
				OneShotAudioDefinition audioDef = popupAudioRef.loadAsset();
				if (audioDef == null)
				{
					UnturnedLog.warn("Missing built-in housing planner success audio");
				}
				else
				{
					player.playSound(audioDef.GetRandomClip(), 0.5f * audioDef.volumeMultiplier, Random.Range(audioDef.minPitch, audioDef.maxPitch), 0.0f);
				}
			}
			else
			{
				AudioClip clip = errorClipRef.loadAsset();
				if (clip == null)
				{
					UnturnedLog.warn("Missing built-in housing planner error audio");
				}
				else
				{
					player.playSound(clip, 0.5f, 1.0f, 0.025f);
				}
			}
		}

		private bool ReceivePlaceHousingItemInternal(in ServerInvocationContext context, System.Guid assetGuid, Vector3 position, float yaw, System.Guid blueprintGuid, byte blueprintIndex)
		{
			if ((position - player.look.aim.position).sqrMagnitude > HousingConnections.MAX_PLACEMENT_SQR_DISTANCE)
			{
				context.LogWarning("out of range");
				return false;
			}

			if (!UseableHousingUtils.IsPendingPositionValid(player, position))
			{
				context.LogWarning("position invalid");
				return false;
			}

			ItemStructureAsset asset = Assets.find(assetGuid) as ItemStructureAsset;
			if (asset == null)
			{
				context.LogWarning("invalid asset");
				return false;
			}

			if (!player.inventory.FindFirstItemByAsset(asset, out PlayerInventorySearchResultV2 searchResult))
			{
				Asset blueprintOwnerAsset = Assets.find(blueprintGuid);
				if (blueprintOwnerAsset == null)
				{
					context.LogWarning("no blueprint to fallback to");
					return false;
				}

				IBlueprintOwner blueprintOwner = blueprintOwnerAsset as IBlueprintOwner;
				if (blueprintOwner == null)
				{
					context.LogWarning($"Requested blueprint {blueprintOwnerAsset} is not a blueprint owner");
					return false;
				}

				Blueprint blueprint = blueprintOwner.GetBlueprintByIndex(blueprintIndex);
				if (blueprint == null)
				{
					context.LogWarning($"Index ({blueprintIndex}) does not correspond to a blueprint");
					return false;
				}

				if (!blueprint.DoesOutputCreateItem(asset))
				{
					context.LogWarning($"Requested blueprint does not create placed item");
					return false;
				}

				bool crafted = player.crafting.HandleCraftRequestInternal(in context, blueprint, /*asManyAsPossible*/ false, /*playEffect*/ false, bypassWorkstationRequirements);
				if (!crafted)
				{
					context.LogWarning("does not own item and was unable to craft");
					return false;
				}

				if (!player.inventory.FindFirstItemByAsset(asset, out searchResult))
				{
					context.LogWarning("unable to find crafting result");
					return false;
				}
			}

			string obstructionHint = string.Empty;
			EHousingPlacementResult result = UseableHousingUtils.ValidatePendingPlacement(asset, ref position, yaw, ref obstructionHint);
			if (result != EHousingPlacementResult.Success)
			{
				context.LogWarning($"housing link result invalid: {result} \"{obstructionHint}\"");
				return false;
			}

			bool wasPlaced = StructureManager.dropStructure(new Structure(asset, asset.health), position, 0, yaw, 0, channel.owner.playerID.steamID.m_SteamID, player.quests.groupID.m_SteamID);
			if (wasPlaced)
			{
				player.sendStat(EPlayerStat.FOUND_BUILDABLES);
				searchResult.DeleteAmount(player, 1);
			}
			else
			{
				context.LogWarning("instantiation failed");
			}
			return wasPlaced;
		}

		private static readonly ServerInstanceMethod<System.Guid, Vector3, float, System.Guid, byte> SendPlaceHousingItem = ServerInstanceMethod<System.Guid, Vector3, float, System.Guid, byte>.Get(typeof(UseableHousingPlanner), nameof(ReceivePlaceHousingItem));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
		public void ReceivePlaceHousingItem(in ServerInvocationContext context, System.Guid assetGuid, [NetPakVector3(fracBitCount: StructureManager.POSITION_FRAC_BIT_COUNT)] Vector3 position, float yaw, System.Guid blueprintGuid, byte blueprintIndex)
		{
			bool success = ReceivePlaceHousingItemInternal(context, assetGuid, position, yaw, blueprintGuid, blueprintIndex);
			SendPlaceHousingItemResult.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GetOwnerTransportConnection(), success);
		}

		public override bool startPrimary()
		{
			if (player.equipment.isBusy)
			{
				return false;
			}

			if (channel.IsLocalPlayer && HasSelection && UpdatePendingPlacement())
			{
				int availableAmount;
				itemAmounts.TryGetValue(selectedOption.asset.id, out availableAmount);
				System.Guid blueprintAssetGuid = default;
				byte blueprintIndex = 0;
				if (availableAmount < 1 && selectedOption.craftable.status?.blueprint != null)
				{
					// Blueprint should be available regardless, otherwise selection should've been cleared.
					Asset blueprintAsset = selectedOption.craftable.status.blueprint.GetOwnerAsset();
					if (blueprintAsset != null)
					{
						blueprintAssetGuid = blueprintAsset.GUID;
						blueprintIndex = selectedOption.craftable.status.blueprint.Index;
					}
				}

				SendPlaceHousingItem.Invoke(GetNetId(), ENetReliability.Reliable, selectedOption.asset.GUID, pendingPlacementPosition, pendingPlacementYaw + customRotationOffset, blueprintAssetGuid, blueprintIndex);
				return true;
			}

			return false;
		}

		public override bool startSecondary()
		{
			if (channel.IsLocalPlayer && HasSelection)
			{
				EConstruct construct = selectedOption.asset.construct;
				if (construct == EConstruct.FLOOR_POLY || construct == EConstruct.ROOF_POLY)
				{
					// Disable rotating triangles because their pivot is off-center.
					return false;
				}

				float delta;
				if (construct == EConstruct.FLOOR || construct == EConstruct.ROOF)
				{
					delta = 90.0f;
				}
				else if (construct == EConstruct.RAMPART || construct == EConstruct.WALL)
				{
					delta = 180.0f;
				}
				else
				{
					delta = 30.0f;
				}
				if (InputEx.GetKey(KeyCode.LeftShift))
				{
					delta *= -1.0f;
				}
				customRotationOffset += delta;
				return true;
			}

			return false;
		}

		public override void equip()
		{
			player.animator.play("Equip", true);

			if (channel.IsLocalPlayer)
			{
				relevantBlueprints = new List<RelevantBlueprint>();
				craftableBlueprints = new Dictionary<ItemStructureAsset, CraftableBlueprint>();
				blueprintStatusPool = new Stack<BlueprintStatus>();

				itemSearchResults = new List<PlayerInventorySearchResultV2>();
				floors = new List<ItemOption>();
				roofs = new List<ItemOption>();
				walls = new List<ItemOption>();
				pillars = new List<ItemOption>();
				itemAmounts = new Dictionary<ushort, int>();

				selectedItemBox = Glazier.Get().CreateBox();
				selectedItemBox.PositionOffset_Y = -50;
				selectedItemBox.PositionScale_X = 0.7f;
				selectedItemBox.PositionScale_Y = 1;
				selectedItemBox.SizeOffset_Y = 50;
				selectedItemBox.SizeScale_X = 0.3f;
				selectedItemBox.IsVisible = false;
				PlayerLifeUI.container.AddChild(selectedItemBox);

				selectedItemNameLabel = Glazier.Get().CreateLabel();
				selectedItemNameLabel.PositionOffset_X = 10;
				selectedItemNameLabel.SizeScale_X = 1.0f;
				selectedItemNameLabel.SizeScale_Y = 1.0f;
				selectedItemNameLabel.SizeOffset_X = -20;
				selectedItemNameLabel.TextAlignment = TextAnchor.MiddleRight;
				selectedItemNameLabel.FontSize = ESleekFontSize.Large;
				selectedItemBox.AddChild(selectedItemNameLabel);
				selectedItemNameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;

				selectedItemAvailableAmountLabel = Glazier.Get().CreateLabel();
				selectedItemAvailableAmountLabel.PositionOffset_X = 10;
				selectedItemAvailableAmountLabel.SizeScale_X = 1.0f;
				selectedItemAvailableAmountLabel.SizeOffset_X = -20;
				selectedItemAvailableAmountLabel.SizeOffset_Y = 30;
				selectedItemAvailableAmountLabel.TextAlignment = TextAnchor.MiddleLeft;
				selectedItemAvailableAmountLabel.FontSize = ESleekFontSize.Medium;
				selectedItemBox.AddChild(selectedItemAvailableAmountLabel);
				selectedItemAvailableAmountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;

				selectedItemCraftableAmountLabel = Glazier.Get().CreateLabel();
				selectedItemCraftableAmountLabel.PositionOffset_X = 10;
				selectedItemCraftableAmountLabel.PositionOffset_Y = 20;
				selectedItemCraftableAmountLabel.SizeScale_X = 1.0f;
				selectedItemCraftableAmountLabel.SizeOffset_X = -20;
				selectedItemCraftableAmountLabel.SizeOffset_Y = 30;
				selectedItemCraftableAmountLabel.TextAlignment = TextAnchor.MiddleLeft;
				selectedItemCraftableAmountLabel.FontSize = ESleekFontSize.Medium;
				selectedItemBox.AddChild(selectedItemCraftableAmountLabel);
				selectedItemCraftableAmountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;

				localization = Localization.read("/Player/Useable/PlayerUseableHousingPlanner.dat");
				IconsBundle icons = Bundles.getIconsBundle("UI/Player/Icons/Useable/PlayerUseableHousingPlanner");
				Texture radialMenuImage = icons.load<Texture>("RadialMenu");

				itemSelectionContainer = Glazier.Get().CreateFrame();
				itemSelectionContainer.SizeScale_X = 1.0f;
				itemSelectionContainer.SizeScale_Y = 1.0f;
				itemSelectionContainer.IsVisible = false;
				PlayerUI.container.AddChild(itemSelectionContainer);

				ISleekImage floorsBackdrop = Glazier.Get().CreateImage(radialMenuImage);
				floorsBackdrop.PositionScale_X = 0.5f;
				floorsBackdrop.PositionScale_Y = 0.5f;
				floorsBackdrop.PositionOffset_X = MENU_PADDING;
				floorsBackdrop.PositionOffset_Y = -MENU_SIZE - MENU_PADDING;
				floorsBackdrop.SizeOffset_X = MENU_SIZE;
				floorsBackdrop.SizeOffset_Y = MENU_SIZE;
				floorsBackdrop.TintColor = SleekColor.BackgroundIfLight(new Color(0.0f, 0.0f, 0.0f, RADIAL_BACKDROP_ALPHA));
				itemSelectionContainer.AddChild(floorsBackdrop);

				floorsLabel = Glazier.Get().CreateLabel();
				floorsLabel.PositionScale_X = 0.5f;
				floorsLabel.PositionScale_Y = 0.5f;
				floorsLabel.PositionOffset_X = MENU_PADDING;
				floorsLabel.PositionOffset_Y = -MENU_SIZE - MENU_PADDING;
				floorsLabel.SizeOffset_X = MENU_SIZE;
				floorsLabel.SizeOffset_Y = MENU_SIZE;
				floorsLabel.FontSize = ESleekFontSize.Large;
				floorsLabel.Text = localization.format("Floors");
				itemSelectionContainer.AddChild(floorsLabel);
				floorsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				noFloorItemsLabel = Glazier.Get().CreateLabel();
				noFloorItemsLabel.PositionScale_X = 0.5f;
				noFloorItemsLabel.PositionScale_Y = 0.5f;
				noFloorItemsLabel.PositionOffset_X = MENU_PADDING;
				noFloorItemsLabel.PositionOffset_Y = -MENU_SIZE - MENU_PADDING + 20;
				noFloorItemsLabel.SizeOffset_X = MENU_SIZE;
				noFloorItemsLabel.SizeOffset_Y = MENU_SIZE;
				noFloorItemsLabel.FontSize = ESleekFontSize.Medium;
				noFloorItemsLabel.TextColor = ESleekTint.BAD;
				noFloorItemsLabel.Text = localization.format("NoItems");
				noFloorItemsLabel.IsVisible = false;
				itemSelectionContainer.AddChild(noFloorItemsLabel);
				noFloorItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				ISleekImage roofsBackdrop = Glazier.Get().CreateImage(radialMenuImage);
				roofsBackdrop.PositionScale_X = 0.5f;
				roofsBackdrop.PositionScale_Y = 0.5f;
				roofsBackdrop.PositionOffset_X = MENU_PADDING;
				roofsBackdrop.PositionOffset_Y = MENU_PADDING;
				roofsBackdrop.SizeOffset_X = MENU_SIZE;
				roofsBackdrop.SizeOffset_Y = MENU_SIZE;
				roofsBackdrop.TintColor = SleekColor.BackgroundIfLight(new Color(0.0f, 0.0f, 0.0f, RADIAL_BACKDROP_ALPHA));
				itemSelectionContainer.AddChild(roofsBackdrop);

				roofsLabel = Glazier.Get().CreateLabel();
				roofsLabel.PositionScale_X = 0.5f;
				roofsLabel.PositionScale_Y = 0.5f;
				roofsLabel.PositionOffset_X = MENU_PADDING;
				roofsLabel.PositionOffset_Y = MENU_PADDING;
				roofsLabel.SizeOffset_X = MENU_SIZE;
				roofsLabel.SizeOffset_Y = MENU_SIZE;
				roofsLabel.FontSize = ESleekFontSize.Large;
				roofsLabel.Text = localization.format("Roofs");
				itemSelectionContainer.AddChild(roofsLabel);
				roofsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				noRoofItemsLabel = Glazier.Get().CreateLabel();
				noRoofItemsLabel.PositionScale_X = 0.5f;
				noRoofItemsLabel.PositionScale_Y = 0.5f;
				noRoofItemsLabel.PositionOffset_X = MENU_PADDING;
				noRoofItemsLabel.PositionOffset_Y = MENU_PADDING + 20;
				noRoofItemsLabel.SizeOffset_X = MENU_SIZE;
				noRoofItemsLabel.SizeOffset_Y = MENU_SIZE;
				noRoofItemsLabel.FontSize = ESleekFontSize.Medium;
				noRoofItemsLabel.TextColor = ESleekTint.BAD;
				noRoofItemsLabel.Text = localization.format("NoItems");
				noRoofItemsLabel.IsVisible = false;
				itemSelectionContainer.AddChild(noRoofItemsLabel);
				noRoofItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				ISleekImage wallsBackdrop = Glazier.Get().CreateImage(radialMenuImage);
				wallsBackdrop.PositionScale_X = 0.5f;
				wallsBackdrop.PositionScale_Y = 0.5f;
				wallsBackdrop.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				wallsBackdrop.PositionOffset_Y = -MENU_SIZE - MENU_PADDING;
				wallsBackdrop.SizeOffset_X = MENU_SIZE;
				wallsBackdrop.SizeOffset_Y = MENU_SIZE;
				wallsBackdrop.TintColor = SleekColor.BackgroundIfLight(new Color(0.0f, 0.0f, 0.0f, RADIAL_BACKDROP_ALPHA));
				itemSelectionContainer.AddChild(wallsBackdrop);

				wallsLabel = Glazier.Get().CreateLabel();
				wallsLabel.PositionScale_X = 0.5f;
				wallsLabel.PositionScale_Y = 0.5f;
				wallsLabel.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				wallsLabel.PositionOffset_Y = -MENU_SIZE - MENU_PADDING;
				wallsLabel.SizeOffset_X = MENU_SIZE;
				wallsLabel.SizeOffset_Y = MENU_SIZE;
				wallsLabel.FontSize = ESleekFontSize.Large;
				wallsLabel.Text = localization.format("Walls");
				itemSelectionContainer.AddChild(wallsLabel);
				wallsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				noWallItemsLabel = Glazier.Get().CreateLabel();
				noWallItemsLabel.PositionScale_X = 0.5f;
				noWallItemsLabel.PositionScale_Y = 0.5f;
				noWallItemsLabel.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				noWallItemsLabel.PositionOffset_Y = -MENU_SIZE - MENU_PADDING + 20;
				noWallItemsLabel.SizeOffset_X = MENU_SIZE;
				noWallItemsLabel.SizeOffset_Y = MENU_SIZE;
				noWallItemsLabel.FontSize = ESleekFontSize.Medium;
				noWallItemsLabel.TextColor = ESleekTint.BAD;
				noWallItemsLabel.Text = localization.format("NoItems");
				noWallItemsLabel.IsVisible = false;
				itemSelectionContainer.AddChild(noWallItemsLabel);
				noWallItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				ISleekImage pillarsBackdrop = Glazier.Get().CreateImage(radialMenuImage);
				pillarsBackdrop.PositionScale_X = 0.5f;
				pillarsBackdrop.PositionScale_Y = 0.5f;
				pillarsBackdrop.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				pillarsBackdrop.PositionOffset_Y = MENU_PADDING;
				pillarsBackdrop.SizeOffset_X = MENU_SIZE;
				pillarsBackdrop.SizeOffset_Y = MENU_SIZE;
				pillarsBackdrop.TintColor = SleekColor.BackgroundIfLight(new Color(0.0f, 0.0f, 0.0f, RADIAL_BACKDROP_ALPHA));
				itemSelectionContainer.AddChild(pillarsBackdrop);

				pillarsLabel = Glazier.Get().CreateLabel();
				pillarsLabel.PositionScale_X = 0.5f;
				pillarsLabel.PositionScale_Y = 0.5f;
				pillarsLabel.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				pillarsLabel.PositionOffset_Y = MENU_PADDING;
				pillarsLabel.SizeOffset_X = MENU_SIZE;
				pillarsLabel.SizeOffset_Y = MENU_SIZE;
				pillarsLabel.FontSize = ESleekFontSize.Large;
				pillarsLabel.Text = localization.format("Pillars");
				itemSelectionContainer.AddChild(pillarsLabel);
				pillarsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				noPillarItemsLabel = Glazier.Get().CreateLabel();
				noPillarItemsLabel.PositionScale_X = 0.5f;
				noPillarItemsLabel.PositionScale_Y = 0.5f;
				noPillarItemsLabel.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				noPillarItemsLabel.PositionOffset_Y = MENU_PADDING + 20;
				noPillarItemsLabel.SizeOffset_X = MENU_SIZE;
				noPillarItemsLabel.SizeOffset_Y = MENU_SIZE;
				noPillarItemsLabel.FontSize = ESleekFontSize.Medium;
				noPillarItemsLabel.TextColor = ESleekTint.BAD;
				noPillarItemsLabel.Text = localization.format("NoItems");
				noPillarItemsLabel.IsVisible = false;
				itemSelectionContainer.AddChild(noPillarItemsLabel);
				noPillarItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;

				floorsMenu = new SleekCircularContainer(MENU_RADIUS, Mathf.PI * 0.75f);
				floorsMenu.PositionScale_X = 0.5f;
				floorsMenu.PositionScale_Y = 0.5f;
				floorsMenu.PositionOffset_X = MENU_PADDING;
				floorsMenu.PositionOffset_Y = -MENU_SIZE - MENU_PADDING;
				floorsMenu.SizeOffset_X = MENU_SIZE;
				floorsMenu.SizeOffset_Y = MENU_SIZE;
				itemSelectionContainer.AddChild(floorsMenu);

				roofsMenu = new SleekCircularContainer(MENU_RADIUS, Mathf.PI * 1.25f);
				roofsMenu.PositionScale_X = 0.5f;
				roofsMenu.PositionScale_Y = 0.5f;
				roofsMenu.PositionOffset_X = MENU_PADDING;
				roofsMenu.PositionOffset_Y = MENU_PADDING;
				roofsMenu.SizeOffset_X = MENU_SIZE;
				roofsMenu.SizeOffset_Y = MENU_SIZE;
				itemSelectionContainer.AddChild(roofsMenu);

				wallsMenu = new SleekCircularContainer(MENU_RADIUS, Mathf.PI * 0.25f);
				wallsMenu.PositionScale_X = 0.5f;
				wallsMenu.PositionScale_Y = 0.5f;
				wallsMenu.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				wallsMenu.PositionOffset_Y = -MENU_SIZE - MENU_PADDING;
				wallsMenu.SizeOffset_X = MENU_SIZE;
				wallsMenu.SizeOffset_Y = MENU_SIZE;
				itemSelectionContainer.AddChild(wallsMenu);

				pillarsMenu = new SleekCircularContainer(MENU_RADIUS, Mathf.PI * 1.75f);
				pillarsMenu.PositionScale_X = 0.5f;
				pillarsMenu.PositionScale_Y = 0.5f;
				pillarsMenu.PositionOffset_X = -MENU_SIZE - MENU_PADDING;
				pillarsMenu.PositionOffset_Y = MENU_PADDING;
				pillarsMenu.SizeOffset_X = MENU_SIZE;
				pillarsMenu.SizeOffset_Y = MENU_SIZE;
				itemSelectionContainer.AddChild(pillarsMenu);

				PlayerUI.message(EPlayerMessage.HOUSING_PLANNER_TUTORIAL, "");
			}
		}

		public override void dequip()
		{
			if (channel.IsLocalPlayer)
			{
				SetItemSelectionMenuOpen(false);
				DestroyPlacementPreview();

				PlayerLifeUI.container.RemoveChild(selectedItemBox);
				PlayerUI.container.RemoveChild(itemSelectionContainer);
			}
		}

		public override void tick()
		{
			if (channel.IsLocalPlayer)
			{
				if (player.inventory.doesSearchNeedRefresh(ref cachedSearchIndex))
				{
					RefreshAvailableItemsAndSelectedBlueprint();
				}

				if (Assets.HasCurrentAssetMappingChanged(ref cachedAssetListChangeCounter))
				{
					RefreshRelevantBlueprints();
				}

				if (InputEx.GetKeyUp(ControlsSettings.attach))
				{
					SetItemSelectionMenuOpen(false);
				}
				else if (!PlayerUI.window.showCursor && InputEx.ConsumeKeyDown(ControlsSettings.attach))
				{
					SetItemSelectionMenuOpen(true);
				}

				if (placementPreviewTransform != null)
				{
					bool isPlacementValid = UpdatePendingPlacement();
					if (isPlacementPreviewValid != isPlacementValid)
					{
						isPlacementPreviewValid = isPlacementValid;
						HighlighterTool.help(placementPreviewTransform, isPlacementPreviewValid);
					}

					float scrollWheelInput = Glazier.Get().ShouldGameProcessInput ? Input.GetAxis("mouse_z") : 0.0f;
					foundationPositionOffset = Mathf.Clamp(foundationPositionOffset + (scrollWheelInput * UseableHousingUtils.FOUNDATION_MOUSE_SCROLL_MULTIPLIER), UseableHousingUtils.FOUNDATION_MIN_OFFSET, UseableHousingUtils.FOUNDATION_MAX_OFFSET);
					animatedRotationOffset = Mathf.Lerp(animatedRotationOffset, customRotationOffset, 8 * Time.deltaTime);
					placementPreviewTransform.position = pendingPlacementPosition;
					placementPreviewTransform.rotation = Quaternion.Euler(-90, pendingPlacementYaw + animatedRotationOffset, 0);
				}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (HasSelection && selectedOption.craftable.status != null)
				{
					if (!selectedOption.craftable.status.blueprint.DoesOutputCreateItem(selectedOption.asset))
					{
						UnturnedLog.error($"Housing planner selected blueprint does not create selected item! (Bug!)\nItem: {selectedOption.asset.FriendlyName}\nBlueprint: {selectedOption.craftable.status.blueprint}");
					}
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			}
		}

		private void SetItemSelectionMenuOpen(bool isOpen)
		{
			if (isItemSelectionMenuOpen == isOpen)
				return;

			isItemSelectionMenuOpen = isOpen;

			PlayerUI.isLocked = isOpen;

			if (isOpen)
			{
				PlayerLifeUI.close();
			}
			else
			{
				PlayerLifeUI.open();
			}

			itemSelectionContainer.IsVisible = isOpen;

			if (isOpen)
			{
				RefreshAllCraftableBlueprints();

				floors.Clear();
				roofs.Clear();
				walls.Clear();
				pillars.Clear();

				foreach (PlayerInventorySearchResultV2 item in itemSearchResults)
				{
					ItemStructureAsset asset = item.GetAsset<ItemStructureAsset>();
					CraftableBlueprint craftableBlueprint;
					craftableBlueprints.TryGetValue(asset, out craftableBlueprint);
					switch (asset.construct)
					{
						case EConstruct.FLOOR:
						case EConstruct.FLOOR_POLY:
							floors.Add(new ItemOption(asset, craftableBlueprint));
							break;

						case EConstruct.ROOF:
						case EConstruct.ROOF_POLY:
							roofs.Add(new ItemOption(asset, craftableBlueprint));
							break;

						case EConstruct.WALL:
						case EConstruct.RAMPART:
							walls.Add(new ItemOption(asset, craftableBlueprint));
							break;

						case EConstruct.PILLAR:
						case EConstruct.POST:
							pillars.Add(new ItemOption(asset, craftableBlueprint));
							break;
					}
				}

				foreach (KeyValuePair<ItemStructureAsset, CraftableBlueprint> kvp in craftableBlueprints)
				{
					ItemStructureAsset asset = kvp.Key;
					if (itemAmounts.ContainsKey(asset.id))
					{
						// Will already have been added by available items above.
						continue;
					}

					switch (asset.construct)
					{
						case EConstruct.FLOOR:
						case EConstruct.FLOOR_POLY:
							floors.Add(new ItemOption(asset, kvp.Value));
							break;

						case EConstruct.ROOF:
						case EConstruct.ROOF_POLY:
							roofs.Add(new ItemOption(asset, kvp.Value));
							break;

						case EConstruct.WALL:
						case EConstruct.RAMPART:
							walls.Add(new ItemOption(asset, kvp.Value));
							break;

						case EConstruct.PILLAR:
						case EConstruct.POST:
							pillars.Add(new ItemOption(asset, kvp.Value));
							break;
					}
				}

				floors.Sort(CompareItemNames);
				roofs.Sort(CompareItemNames);
				walls.Sort(CompareItemNames);
				pillars.Sort(CompareItemNames);

				noFloorItemsLabel.IsVisible = floors.Count < 1;
				noRoofItemsLabel.IsVisible = roofs.Count < 1;
				noWallItemsLabel.IsVisible = walls.Count < 1;
				noPillarItemsLabel.IsVisible = pillars.Count < 1;

				PopulateCircularMenu(floorsMenu, floors);
				PopulateCircularMenu(roofsMenu, roofs);
				PopulateCircularMenu(wallsMenu, walls);
				PopulateCircularMenu(pillarsMenu, pillars);
			}
		}

		private void PopulateCircularMenu(SleekCircularContainer container, List<ItemOption> options)
		{
			container.RemoveAllChildren();

			foreach (ItemOption option in options)
			{
				SleekHousingPlannerOption optionWidget = new SleekHousingPlannerOption(this, option);
				container.AddChild(optionWidget);
			}

			container.UpdateLayout();
		}

		private void DestroyPlacementPreview()
		{
			if (placementPreviewTransform != null)
			{
				Destroy(placementPreviewTransform.gameObject);
				placementPreviewTransform = null;
			}
		}

		private void ClearSelectedOption()
		{
			SetSelectedOption(default);
		}

		private void SetSelectedOption(ItemOption selectedOption)
		{
			this.selectedOption = selectedOption;

			DestroyPlacementPreview();

			isPlacementPreviewValid = false;
			foundationPositionOffset = 0.0f;
			customRotationOffset = 0.0f;
			animatedRotationOffset = 0.0f;

			if (HasSelection)
			{
				placementPreviewTransform = UseableHousingUtils.InstantiatePlacementPreview(selectedOption.asset);
				selectedItemNameLabel.Text = selectedOption.asset.itemName;
				selectedItemNameLabel.TextColor = ItemTool.getRarityColorUI(selectedOption.asset.rarity);

				int availableAmount;
				itemAmounts.TryGetValue(selectedOption.asset.id, out availableAmount);
				int craftableAmount = selectedOption.craftable.status?.EstimateOutputMaxAmount(selectedOption.craftable.structureOutputIndex) ?? 0;
				selectedItemAvailableAmountLabel.Text = localization.format("AvailableAmount", availableAmount);
				selectedItemCraftableAmountLabel.Text = localization.format("CraftableAmount", craftableAmount);
			}

			selectedItemBox.IsVisible = HasSelection;
		}

		private bool UpdatePendingPlacement()
		{
			if (!UseableHousingUtils.FindPlacement(selectedOption.asset, player, customRotationOffset, foundationPositionOffset, out pendingPlacementPosition, out pendingPlacementYaw))
			{
				return false;
			}

			if (!UseableHousingUtils.IsPendingPositionValid(player, pendingPlacementPosition))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Search loaded assets for blueprints that output a single structure item and are
		/// available on the current map.
		/// </summary>
		private void RefreshRelevantBlueprints()
		{
			if (!Level.IsCraftingAllowedByLevel)
			{
				return;
			}

			PlayerCrafting crafting = player.crafting;
			relevantBlueprints.Clear();
			foreach (IBlueprintOwner asset in PlayerDashboardCraftingUI.GetBlueprintOwners())
			{
				foreach (Blueprint blueprint in asset.GetBlueprints())
				{
					if (blueprint.outputs == null || blueprint.outputs.Length < 1)
						continue;

					int structureOutputIndex = -1;
					for (int index = 0; index < blueprint.outputs.Length; ++index)
					{
						BlueprintOutput output = blueprint.outputs[index];
						ItemStructureAsset outputItem = output.FindItemAsset<ItemStructureAsset>();
						if (outputItem != null)
						{
							structureOutputIndex = index;
							break;
						}
					}

					if (structureOutputIndex < 0)
					{
						continue;
					}

					if (crafting.IsBlueprintPermanentlyDisabled(blueprint))
						continue;

					relevantBlueprints.Add(new RelevantBlueprint(blueprint, structureOutputIndex));
				}
			}
		}

		/// <summary>
		/// Update status of all relevant blueprints.
		/// </summary>
		private void RefreshAllCraftableBlueprints()
		{
			foreach (CraftableBlueprint craftableBlueprint in craftableBlueprints.Values)
			{
				blueprintStatusPool.Push(craftableBlueprint.status);
			}
			craftableBlueprints.Clear();

			PlayerCrafting crafting = player.crafting;
			foreach (RelevantBlueprint relevantBlueprint in relevantBlueprints)
			{
				Blueprint blueprint = relevantBlueprint.blueprint;
				ItemStructureAsset outputItem = blueprint.outputs[relevantBlueprint.structureOutputIndex].FindItemAsset<ItemStructureAsset>();
				if (outputItem == null)
				{
					// Shouldn't have happened!
					continue;
				}

				if (craftableBlueprints.ContainsKey(outputItem))
				{
					// Already found a recipe for this item.
					continue;
				}

				BlueprintStatus status = CreateBlueprintStatus();
				status.blueprint = blueprint;

				UpdateBlueprintStatusParameters p = new UpdateBlueprintStatusParameters()
				{
					status = status,
					shouldExitEarly = true, // Uncraftable blueprints are not shown.
				};

				crafting.UpdateBlueprintStaticStatus(in p, bypassWorkstationRequirements);
				if (!status.IsCraftable)
				{
					blueprintStatusPool.Push(status);
					continue;
				}

				crafting.UpdateBlueprintDynamicStatus(in p);
				if (!status.IsCraftable)
				{
					blueprintStatusPool.Push(status);
					continue;
				}

				craftableBlueprints.Add(outputItem, new CraftableBlueprint(status, relevantBlueprint.structureOutputIndex));
			}

			if (HasSelection)
			{
				// Restore blueprint after it was returned to pool, if possible.
				// (player might have available amount, but no longer craftable)
				craftableBlueprints.TryGetValue(selectedOption.asset, out selectedOption.craftable);

				int craftableAmount = selectedOption.craftable.status?.EstimateOutputMaxAmount(selectedOption.craftable.structureOutputIndex) ?? 0;
				selectedItemCraftableAmountLabel.Text = localization.format("CraftableAmount", craftableAmount);
			}
		}

		/// <summary>
		/// Currently saved craftableBlueprint for asset may have become uncraftable,
		/// in which case we try finding a craftable replacement.
		/// </summary>
		private void RefreshCraftableBlueprint(ItemStructureAsset forAsset)
		{
			// Assume existing craftableBlueprint is no longer craftable. (If it was, it will be re-added.)
			if (craftableBlueprints.TryGetValue(forAsset, out CraftableBlueprint previousCraftable))
			{
				blueprintStatusPool.Push(previousCraftable.status);
				craftableBlueprints.Remove(forAsset);
			}

			PlayerCrafting crafting = player.crafting;
			foreach (RelevantBlueprint relevantBlueprint in relevantBlueprints)
			{
				Blueprint blueprint = relevantBlueprint.blueprint;
				ItemStructureAsset outputItem = blueprint.outputs[relevantBlueprint.structureOutputIndex].FindItemAsset<ItemStructureAsset>();
				if (outputItem != forAsset)
				{
					continue;
				}

				BlueprintStatus status = CreateBlueprintStatus();
				status.blueprint = blueprint;

				UpdateBlueprintStatusParameters p = new UpdateBlueprintStatusParameters()
				{
					status = status,
					shouldExitEarly = true, // Uncraftable blueprints are not shown.
				};

				crafting.UpdateBlueprintStaticStatus(in p, bypassWorkstationRequirements);
				if (!status.IsCraftable)
				{
					blueprintStatusPool.Push(status);
					continue;
				}

				crafting.UpdateBlueprintDynamicStatus(in p);
				if (!status.IsCraftable)
				{
					blueprintStatusPool.Push(status);
					continue;
				}

				// Found one! :)
				craftableBlueprints.Add(forAsset, new CraftableBlueprint(status, relevantBlueprint.structureOutputIndex));
				break;
			}

			// Note: doesn't update selected option. Caller does that ATM.
		}

		/// <summary>
		/// Get a blank status from the pool or construct a new one.
		/// </summary>
		private BlueprintStatus CreateBlueprintStatus()
		{
			BlueprintStatus blueprintStatus;
			if (blueprintStatusPool.TryPop(out blueprintStatus))
			{
				blueprintStatus.Reset();
			}
			else
			{
				blueprintStatus = new BlueprintStatus();
			}
			return blueprintStatus;
		}

		/// <summary>
		/// Search inventory for housing items, count the quantity of each, and remove
		/// duplicate entries from the list because it is used for the UI.
		/// </summary>
		private void RefreshAvailableItemsAndSelectedBlueprint()
		{
			itemSearchResults.Clear();
			itemAmounts.Clear();
			player.inventory.FindItemsByType(itemSearchResults, EItemType.STRUCTURE);

			for (int index = itemSearchResults.Count - 1; index >= 0; --index)
			{
				PlayerInventorySearchResultV2 item = itemSearchResults[index];

				int amount;
				if (itemAmounts.TryGetValue(item.Jar.item.id, out amount))
				{
					// Remove duplicate item types from the UI.
					itemSearchResults.RemoveAtFast(index);
				}

				itemAmounts[item.Jar.item.id] = amount + item.Jar.item.amount;
			}

			if (HasSelection)
			{
				int availableAmount;
				itemAmounts.TryGetValue(selectedOption.asset.id, out availableAmount);

				// Nelson 2025-07-01: we re-find an appropriate blueprint every time inventory changes. At first this
				// was only done if the recipe became uncraftable, but the benefit of doing it all times is it will
				// re-select a non-unstacking blueprint if a stacking blueprint was selected before.
				RefreshCraftableBlueprint(selectedOption.asset);
				craftableBlueprints.TryGetValue(selectedOption.asset, out selectedOption.craftable);
				int craftableAmount = selectedOption.craftable.status?.EstimateOutputMaxAmount(selectedOption.craftable.structureOutputIndex) ?? 0;

				if (availableAmount > 0 || craftableAmount > 0)
				{
					selectedItemAvailableAmountLabel.Text = localization.format("AvailableAmount", availableAmount);
					selectedItemCraftableAmountLabel.Text = localization.format("CraftableAmount", craftableAmount);
				}
				else
				{
					// Player must select a new item because they used up all of the old item.
					ClearSelectedOption();
				}
			}
		}

		private int CompareItemNames(ItemOption lhs, ItemOption rhs)
		{
			if (lhs.asset != null && rhs.asset != null)
			{
				return lhs.asset.itemName.CompareTo(rhs.asset.itemName);
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Stripped-down version of structure prefab for previewing where the structure will be spawned.
		/// </summary>
		private Transform placementPreviewTransform;

		/// <summary>
		/// Whether preview object is currently highlighted positively.
		/// </summary>
		private bool isPlacementPreviewValid;

		/// <summary>
		/// Position the item should be spawned at.
		/// </summary>
		private Vector3 pendingPlacementPosition;

		/// <summary>
		/// Rotation the item should be spawned at.
		/// </summary>
		private float pendingPlacementYaw;

		/// <summary>
		/// Interpolated toward customRotationOffset.
		/// </summary>
		private float animatedRotationOffset;

		/// <summary>
		/// Allows players to flip walls.
		/// </summary>
		private float customRotationOffset;

		/// <summary>
		/// Vertical offset using scroll wheel.
		/// </summary>
		private float foundationPositionOffset;

		private ItemOption selectedOption;

		private bool isItemSelectionMenuOpen;

		private ISleekElement itemSelectionContainer;

		private SleekCircularContainer floorsMenu;
		private SleekCircularContainer roofsMenu;
		private SleekCircularContainer wallsMenu;
		private SleekCircularContainer pillarsMenu;

		private ISleekLabel floorsLabel;
		private ISleekLabel noFloorItemsLabel;
		private ISleekLabel roofsLabel;
		private ISleekLabel noRoofItemsLabel;
		private ISleekLabel wallsLabel;
		private ISleekLabel noWallItemsLabel;
		private ISleekLabel pillarsLabel;
		private ISleekLabel noPillarItemsLabel;

		/// <summary>
		/// Box in the HUD with selected item name and quantity.
		/// </summary>
		private ISleekBox selectedItemBox;
		private ISleekLabel selectedItemNameLabel;
		private ISleekLabel selectedItemAvailableAmountLabel;
		private ISleekLabel selectedItemCraftableAmountLabel;

		/// <summary>
		/// Blueprints which create a structure item.
		/// </summary>
		private List<RelevantBlueprint> relevantBlueprints;
		/// <summary>
		/// One craftable blueprint per potential structure item.
		/// </summary>
		private Dictionary<ItemStructureAsset, CraftableBlueprint> craftableBlueprints;
		/// <summary>
		/// Recycled blueprint statuses.
		/// </summary>
		private Stack<BlueprintStatus> blueprintStatusPool;

		private List<PlayerInventorySearchResultV2> itemSearchResults;
		private List<ItemOption> floors;
		private List<ItemOption> roofs;
		private List<ItemOption> walls;
		private List<ItemOption> pillars;
		private Dictionary<ushort, int> itemAmounts;
		private int cachedSearchIndex = -1;
		private int cachedAssetListChangeCounter = -1;

		private bool HasSelection => selectedOption.asset != null;

		private const float MENU_RADIUS = 128.0f;
		private const int MENU_SIZE = 256;
		private const int MENU_PADDING = 50;
		private const float RADIAL_BACKDROP_ALPHA = 0.2f;

		private struct RelevantBlueprint
		{
			public Blueprint blueprint;
			public int structureOutputIndex;

			public RelevantBlueprint(Blueprint blueprint, int structureOutputIndex)
			{
				this.blueprint = blueprint;
				this.structureOutputIndex = structureOutputIndex;
			}
		}

		private struct CraftableBlueprint
		{
			public BlueprintStatus status;
			public int structureOutputIndex;

			public CraftableBlueprint(BlueprintStatus status, int structureOutputIndex)
			{
				this.status = status;
				this.structureOutputIndex = structureOutputIndex;
			}
		}

		private struct ItemOption
		{
			public ItemStructureAsset asset;
			public CraftableBlueprint craftable;

			public ItemOption(ItemStructureAsset asset, CraftableBlueprint craftable)
			{
				this.asset = asset;
				this.craftable = craftable;
			}
		}

		private class SleekHousingPlannerOption : SleekWrapper
		{
			public SleekHousingPlannerOption(UseableHousingPlanner useable, ItemOption option) : base()
			{
				this.useable = useable;
				this.option = option;
				ItemStructureAsset asset = option.asset;

				SizeOffset_X = asset.size_x * 50;
				SizeOffset_Y = asset.size_y * 50;

				Color rarityColor = ItemTool.getRarityColorUI(asset.rarity);
				button = Glazier.Get().CreateButton();
				button.SizeScale_X = 1;
				button.SizeScale_Y = 1;
				button.BackgroundColor = SleekColor.BackgroundIfLight(rarityColor);
				button.TextColor = rarityColor;
				button.TooltipText = asset.itemName;
				button.OnClicked += OnClicked;
				AddChild(button);

				icon = new SleekItemIcon();
				icon.SizeScale_X = 1;
				icon.SizeScale_Y = 1;
				icon.Refresh(asset, Mathf.RoundToInt(SizeOffset_X), Mathf.RoundToInt(SizeOffset_Y));
				AddChild(icon);

				amountLabel = Glazier.Get().CreateLabel();
				amountLabel.PositionScale_Y = 1;
				amountLabel.SizeOffset_Y = 30;
				amountLabel.SizeScale_X = 1;
				if (asset.size_x == 1 || asset.size_y == 1)
				{
					amountLabel.PositionOffset_X = 0;
					amountLabel.PositionOffset_Y = -30;
					amountLabel.SizeOffset_X = 0;
					amountLabel.FontSize = ESleekFontSize.Small;
				}
				else
				{
					amountLabel.PositionOffset_X = 5;
					amountLabel.PositionOffset_Y = -35;
					amountLabel.SizeOffset_X = -10;
					amountLabel.FontSize = ESleekFontSize.Default;
				}

				int availableAmount;
				useable.itemAmounts.TryGetValue(asset.id, out availableAmount);
				int craftableAmount = option.craftable.status?.EstimateOutputMaxAmount(option.craftable.structureOutputIndex) ?? 0;
				amountLabel.Text = $"{availableAmount}+{craftableAmount}";
				amountLabel.TextAlignment = TextAnchor.LowerLeft;
				amountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				AddChild(amountLabel);
			}

			private void OnClicked(ISleekElement button)
			{
				useable.SetSelectedOption(option);
			}

			private UseableHousingPlanner useable;
			private ItemOption option;
			private ISleekButton button;
			private SleekItemIcon icon;
			private ISleekLabel amountLabel;
		}

		private static Local localization;
		private const bool bypassWorkstationRequirements = true;
	}
}
