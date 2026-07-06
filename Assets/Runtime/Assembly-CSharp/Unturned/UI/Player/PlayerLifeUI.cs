////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Provider;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public struct HitmarkerInfo
	{
		public float aliveTime;
		public Vector3 worldPosition;
		public bool shouldFollowWorldPosition;
		public SleekHitmarker sleekElement;
	}

	public partial class PlayerLifeUI
	{
		public static Local localization;
		public static IconsBundle icons;

		private static SleekFullscreenBox _container;
		public static SleekFullscreenBox container => _container;

		public static bool active;
		public static bool chatting;
		public static bool gesturing;
		public static IDialogueTarget npc;

		public static bool isVoteMessaged;
		public static float lastVoteMessage;

		// Chat V1 fixed-height, no auto layout, IMGUI-only
		private static ISleekScrollView chatHistoryBoxV1;
		private static SleekChatEntryV1[] chatHistoryLabelsV1; // Labels in scroll box for reviewing previous messages
		private static SleekChatEntryV1[] chatPreviewLabelsV1; // Labels always visible outside scroll box

		// Chat V2 auto layout, uGUI & UITK
		private static ISleekScrollView chatScrollViewV2;
		private static Queue<SleekChatEntryV2> chatEntriesV2;

		public static ISleekField chatField;
		private static SleekButtonState chatModeButton;
		private static SleekButtonIcon sendChatButton;
		public static ISleekBox voteBox;
		private static ISleekLabel voteInfoLabel;
		private static ISleekLabel votesNeededLabel;
		private static ISleekLabel voteYesLabel;
		private static ISleekLabel voteNoLabel;
		private static SleekBoxIcon voiceBox;
		private static ISleekImage voiceOutboundOffIcon;

		private static ISleekLabel trackedQuestTitle;
		private static ISleekImage trackedQuestBar;

		private static ISleekBox levelTextBox;
		private static ISleekBox levelNumberBox;

		public static ISleekBox compassBox;
		private static ISleekElement compassLabelsContainer;
		private static ISleekElement compassMarkersContainer;
		private static List<ISleekImage> compassMarkers;
		private static int compassMarkersVisibleCount;
		private static ISleekLabel[] compassLabels;

		private static ISleekLabel getCompassLabelByAngle(int angle)
		{
			return compassLabels[angle / 5];
		}

		private static ISleekElement hotbarContainer;
		private static SleekHotbarEntry[] hotbarItems;
		private static int previousEquippedHotbarIndex;

		public static ISleekLabel statTrackerLabel;

		private static ISleekButton[] faceButtons;
		private static ISleekButton surrenderButton;
		private static ISleekButton pointButton;
		private static ISleekButton waveButton;
		private static ISleekButton saluteButton;
		private static ISleekButton restButton;
		private static ISleekButton facepalmButton;
		private static ISleekButton tPoseButton;

		public static SleekScopeOverlay scopeOverlay;
		public static ISleekImage binocularsOverlay;

		public static Crosshair crosshair;

		private static ISleekBox lifeBox;
		private static ISleekImage healthIcon;
		private static SleekProgress healthProgress;

		private static ISleekImage foodIcon;
		private static SleekProgress foodProgress;

		private static ISleekImage waterIcon;
		private static SleekProgress waterProgress;

		private static ISleekImage virusIcon;
		private static SleekProgress virusProgress;

		private static ISleekImage staminaIcon;
		private static SleekProgress staminaProgress;

		private static ISleekLabel waveLabel;
		private static ISleekLabel scoreLabel;

		//private static SleekBoxIcon oxygenBox;
		private static ISleekImage oxygenIcon;
		private static SleekProgress oxygenProgress;

		private static ISleekBox vehicleBox;
		private static ISleekImage fuelIcon;
		private static SleekProgress fuelProgress;
		private static ISleekLabel vehicleLockedLabel;
		private static ISleekLabel vehicleEngineLabel;
		private static bool vehicleVisibleByDefault;

		private static ISleekBox gasmaskBox;
		private static SleekItemIcon gasmaskIcon;
		private static SleekProgress gasmaskProgress;

		private static ISleekImage speedIcon;
		private static SleekProgress speedProgress;

		private static ISleekImage batteryChargeIcon;
		private static SleekProgress batteryChargeProgress;

		private static ISleekImage hpIcon;
		private static SleekProgress hpProgress;

		private static ISleekElement statusIconsContainer;
		private static SleekBoxIcon bleedingBox;
		private static SleekBoxIcon brokenBox;
		private static SleekBoxIcon temperatureBox;
		private static SleekBoxIcon starvedBox;
		private static SleekBoxIcon dehydratedBox;
		private static SleekBoxIcon infectedBox;
		private static SleekBoxIcon drownedBox;
		private static SleekBoxIcon asphyxiatingBox;
		private static SleekBoxIcon moonBox;
		private static SleekBoxIcon radiationBox;
		private static SleekBoxIcon safeBox;
		private static SleekBoxIcon arrestBox;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			if (npc != null)
			{
				npc.SetIsTalkingWithLocalPlayer(false);
				npc = null;
			}

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			closeChat();
			closeGestures();

			if (container != null)
			{
				container.AnimateOutOfView(0, 1);
			}
		}

		public static void openChat()
		{
			if (chatting)
			{
				return;
			}

			chatting = true;

			chatField.Text = string.Empty;
			chatField.AnimatePositionOffset(100, chatField.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20);

			chatModeButton.state = (int) PlayerUI.chat;

			if (chatEntriesV2 != null)
			{
				chatScrollViewV2.VerticalScrollbarVisibility = ESleekScrollbarVisibility.Default;
				chatScrollViewV2.IsRaycastTarget = true;

				foreach (SleekChatEntryV2 chatEntry in chatEntriesV2)
				{
					chatEntry.forceVisibleWhileBrowsingChatHistory = true;
				}

				chatScrollViewV2.ScrollToBottom();
			}
			else if (chatHistoryBoxV1 != null)
			{
				chatHistoryBoxV1.IsVisible = true;
				chatHistoryBoxV1.ScrollToBottom(); // Scroll to most recent message
				for (int index = 0; index < chatPreviewLabelsV1.Length; index++)
				{
					chatPreviewLabelsV1[index].IsVisible = false;
				}
			}
		}

		public static void closeChat()
		{
			if (!chatting)
			{
				return;
			}

			chatting = false;
			repeatChatIndex = -1;

			if (chatField != null)
			{
				chatField.Text = string.Empty;
				chatField.AnimatePositionOffset(-chatField.SizeOffset_X - 50, chatField.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20);
			}

			if (chatEntriesV2 != null)
			{
				chatScrollViewV2.VerticalScrollbarVisibility = ESleekScrollbarVisibility.Hidden;
				chatScrollViewV2.IsRaycastTarget = false; // Fixes public issue #4142.

				foreach (SleekChatEntryV2 chatEntry in chatEntriesV2)
				{
					chatEntry.forceVisibleWhileBrowsingChatHistory = false;
				}

				chatScrollViewV2.ScrollToBottom();
			}
			else if (chatHistoryBoxV1 != null)
			{
				chatHistoryBoxV1.IsVisible = false;
				for (int index = 0; index < chatPreviewLabelsV1.Length; index++)
				{
					chatPreviewLabelsV1[index].IsVisible = true;
				}
			}
		}

		public static void SendChatAndClose()
		{
			if (!string.IsNullOrEmpty(chatField.Text))
			{
				ChatManager.sendChat(PlayerUI.chat, chatField.Text);
			}

			closeChat();
		}

		/// <summary>
		/// Reset to -1 when not chatting. If player presses up/down we get index 0 (most recent).
		/// </summary>
		private static int repeatChatIndex = -1;

		/// <summary>
		/// Fill chat field with previous sent message.
		/// Useful for repeating commands with minor changes.
		/// </summary>
		public static void repeatChat(int delta)
		{
			if (chatField == null)
				return;

			int testChatIndex = Mathf.Max(repeatChatIndex + delta, 0);
			string message = ChatManager.getRecentlySentMessage(testChatIndex);
			if (string.IsNullOrEmpty(message))
				return;

			chatField.Text = message;
			repeatChatIndex = testChatIndex;
		}

		private static void OnChatFieldEscaped(ISleekField field)
		{
			// Important! IMGUI and uGUI have different workarounds here because uGUI does not fire OnEscaped yet.
			if (chatting)
			{
				closeChat();
			}
		}

		private static void OnSwappedChatModeState(SleekButtonState button, int index)
		{
			PlayerUI.chat = (EChatMode) index;
		}

		private static void OnSendChatButtonClicked(ISleekElement button)
		{
			if (chatting)
			{
				SendChatAndClose();
			}
		}

		public static void openGestures()
		{
			if (gesturing)
			{
				return;
			}

			gesturing = true;

			for (int index = 0; index < faceButtons.Length; index++)
			{
				faceButtons[index].IsVisible = true;
			}

			bool canGesture = !Player.LocalPlayer.equipment.HasValidUseable && Player.LocalPlayer.stance.stance != EPlayerStance.PRONE && Player.LocalPlayer.stance.stance != EPlayerStance.DRIVING && Player.LocalPlayer.stance.stance != EPlayerStance.SITTING;

			surrenderButton.IsVisible = canGesture;
			pointButton.IsVisible = canGesture;
			waveButton.IsVisible = canGesture;
			saluteButton.IsVisible = canGesture;
			restButton.IsVisible = canGesture;
			facepalmButton.IsVisible = canGesture;
			tPoseButton.IsVisible = canGesture;
		}

		public static void closeGestures()
		{
			if (!gesturing)
			{
				return;
			}

			gesturing = false;

			if (faceButtons == null)
				return;

			for (int index = 0; index < faceButtons.Length; index++)
			{
				faceButtons[index].IsVisible = false;
			}

			surrenderButton.IsVisible = false;
			pointButton.IsVisible = false;
			waveButton.IsVisible = false;
			saluteButton.IsVisible = false;
			restButton.IsVisible = false;
			facepalmButton.IsVisible = false;
			tPoseButton.IsVisible = false;
		}

		private static void OnLocalPluginWidgetFlagsChanged(Player player, EPluginWidgetFlags oldFlags)
		{
			EPluginWidgetFlags newFlags = player.pluginWidgetFlags;

			if ((oldFlags & EPluginWidgetFlags.ShowStatusIcons) != (newFlags & EPluginWidgetFlags.ShowStatusIcons))
			{
				updateIcons();
			}

			if ((oldFlags & EPluginWidgetFlags.ShowLifeMeters) != (newFlags & EPluginWidgetFlags.ShowLifeMeters))
			{
				updateLifeBoxVisibility();
			}

			if ((oldFlags & EPluginWidgetFlags.ShowVehicleStatus) != (newFlags & EPluginWidgetFlags.ShowVehicleStatus))
			{
				UpdateVehicleBoxVisibility();
			}

			if (crosshair != null)
			{
				crosshair.SetPluginAllowsCenterDotVisible(newFlags.HasFlag(EPluginWidgetFlags.ShowCenterDot));
			}
		}

		private static void onDamaged(byte damage)
		{
			if (damage > 5)
			{
				PlayerUI.pain(Mathf.Clamp(damage / 40f, 0f, 1f));
			}
		}

		private static void updateHotbarItem(ref float offset, ref float maxHeight, ItemJar jar, byte index)
		{
			SleekHotbarEntry entry = hotbarItems[index];
			entry.UpdateItem(jar);
			if (entry.IsVisible)
			{
				entry.PositionOffset_X = offset;
				offset += entry.SizeOffset_X;
				offset += 5;
				maxHeight = Mathf.Max(maxHeight, entry.SizeOffset_Y);
			}
		}

		private static int cachedHotbarSearch;
		/// <summary>
		/// Use the latest hotbar items in the UI.
		/// </summary>
		public static void updateHotbar()
		{
			if (hotbarContainer == null || Player.LocalPlayer == null)
				return;

			hotbarContainer.IsVisible = !PlayerUI.messageBox.IsVisible && !PlayerUI.messageBox2.IsVisible && OptionsSettings.showHotbar;
			if (!hotbarContainer.IsVisible)
				return;

			int equippedHotkeyButtonIndex = Player.LocalPlayer.equipment.FindEquippedHotkeyButton();
			if (previousEquippedHotbarIndex != equippedHotkeyButtonIndex)
			{
				if (previousEquippedHotbarIndex >= 0)
				{
					hotbarItems[previousEquippedHotbarIndex].IsEquipped = false;
				}

				previousEquippedHotbarIndex = equippedHotkeyButtonIndex;

				if (equippedHotkeyButtonIndex >= 0)
				{
					hotbarItems[equippedHotkeyButtonIndex].IsEquipped = true;
				}
			}

			if (!Player.LocalPlayer.inventory.doesSearchNeedRefresh(ref cachedHotbarSearch))
			{
				if (equippedHotkeyButtonIndex >= 0)
				{
					hotbarItems[equippedHotkeyButtonIndex].UpdateQuality();
				}
				return;
			}
			
			float offset = 0;
			float maxHeight = 0;

			updateHotbarItem(ref offset, ref maxHeight, Player.LocalPlayer.inventory.getItem(0, 0), 0);
			updateHotbarItem(ref offset, ref maxHeight, Player.LocalPlayer.inventory.getItem(1, 0), 1);

			for (byte hotkeyIndex = 0; hotkeyIndex < Player.LocalPlayer.equipment.hotkeys.Length; hotkeyIndex++)
			{
				HotkeyInfo hotkeyInfo = Player.LocalPlayer.equipment.hotkeys[hotkeyIndex];
				ItemJar jar = null;

				if (hotkeyInfo.id != 0)
				{
					byte index = Player.LocalPlayer.inventory.getIndex(hotkeyInfo.page, hotkeyInfo.x, hotkeyInfo.y);
					jar = Player.LocalPlayer.inventory.getItem(hotkeyInfo.page, index);

					if (jar != null && jar.item.id != hotkeyInfo.id)
					{
						jar = null;
					}
				}

				updateHotbarItem(ref offset, ref maxHeight, jar, (byte) (hotkeyIndex + 2));
			}

			hotbarContainer.SizeOffset_X = Mathf.Max(0, offset - 5);
			hotbarContainer.SizeOffset_Y = maxHeight;

			hotbarContainer.PositionOffset_X = hotbarContainer.SizeOffset_X / -2;
			hotbarContainer.PositionOffset_Y = -80 - hotbarContainer.SizeOffset_Y;
		}

		public static void updateStatTracker()
		{
			EStatTrackerType type;
			int kills;
			statTrackerLabel.IsVisible = Player.LocalPlayer.equipment.getUseableStatTrackerValue(out type, out kills);
			if (statTrackerLabel.IsVisible)
			{
				statTrackerLabel.TextColor = Provider.provider.economyService.getStatTrackerColor(type);
				statTrackerLabel.Text = localization.format(type == EStatTrackerType.TOTAL ? "Stat_Tracker_Total_Kills" : "Stat_Tracker_Player_Kills", kills.ToString("D7"));
			}
		}

		private static void onHotkeysUpdated()
		{
			cachedHotbarSearch = -1; // Force a refresh next repaint.
		}

		/// <summary>
		/// Moves legacy image effect dependency out of SDK release.
		/// </summary>
		static partial void UpdateGrayscaleEffectPartial();

		public static void updateGrayscale()
		{
			UpdateGrayscaleEffectPartial();
		}

		private static void onPerspectiveUpdated(EPlayerPerspective newPerspective)
		{
			updateGrayscale();
		}

		private static void onHealthUpdated(byte newHealth)
		{
			healthProgress.state = newHealth / 100f;
			onPerspectiveUpdated(Player.LocalPlayer.look.perspective);
		}

		private static void onFoodUpdated(byte newFood)
		{
			updateIcons();
			foodProgress.state = newFood / 100f;
		}

		private static void onWaterUpdated(byte newWater)
		{
			updateIcons();
			waterProgress.state = newWater / 100f;
		}

		private static void onVirusUpdated(byte newVirus)
		{
			updateIcons();
			virusProgress.state = newVirus / 100f;
		}

		private static void onStaminaUpdated(byte newStamina)
		{
			staminaProgress.state = newStamina / 100f;
		}

		private static void onOxygenUpdated(byte newOxygen)
		{
			updateIcons();
			oxygenProgress.state = newOxygen / 100f;
		}

		private static void OnIsAsphyxiatingChanged()
		{
			updateIcons();
		}

		private static void updateCompassElement(ISleekElement element, float viewAngle, float elementAngle, out float alpha)
		{
			float difference = Mathf.DeltaAngle(viewAngle, elementAngle) / 22.5f;

			element.PositionScale_X = (difference / 2) + 0.5f; // range -1 to 1, divide by 2 for -0.5f to 0.5 and add 0.5 to center 0 to 1
			element.IsVisible = Mathf.Abs(difference) < 1;
			alpha = 1.0f - MathfEx.Square(Mathf.Abs(difference));
		}

		private static int cachedCompassSearch;
		private static bool cachedHasCompass;
		protected static bool hasCompassInInventory()
		{
			if (!Player.LocalPlayer.inventory.doesSearchNeedRefresh(ref cachedCompassSearch))
				return cachedHasCompass;

			cachedHasCompass = false;
			for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
			{
				Items list = Player.LocalPlayer.inventory.items[page];
				if (list == null)
					continue;

				foreach (ItemJar jar in list.items)
				{
					if (jar == null)
						continue;

					ItemMapAsset map = jar.GetAsset<ItemMapAsset>();
					if (map != null && map.enablesCompass)
					{
						cachedHasCompass = true;
						return cachedHasCompass;
					}
				}
			}

			return cachedHasCompass;
		}

		public static void updateCompass()
		{
			if (Provider.modeConfigData.Gameplay.Compass || (Level.info != null && Level.info.type == ELevelType.ARENA))
			{
				compassBox.IsVisible = true;
			}
			else
			{
				compassBox.IsVisible = hasCompassInInventory();
			}

			if (!compassBox.IsVisible)
			{
				return;
			}

			UnityEngine.Profiling.Profiler.BeginSample("UpdateCompass");

			Transform cameraTransform = MainCamera.instance.transform;
			Vector3 cameraPosition = cameraTransform.position;
			float viewAngle = cameraTransform.rotation.eulerAngles.y;

			for (int compassIndex = 0; compassIndex < compassLabels.Length; compassIndex++)
			{
				float compassAngle = compassIndex * 5; // [0, 350]

				ISleekLabel compassLabel = compassLabels[compassIndex];
				Color textColor = compassLabel.TextColor;
				updateCompassElement(compassLabel, viewAngle, compassAngle, out textColor.a);
				compassLabel.TextColor = textColor;
			}

			int compassMarkerIndex = 0;
			foreach (SteamPlayer player in Provider.clients)
			{
				if (player.model == null)
				{
					continue;
				}

				PlayerQuests quests = player.player.quests;
				if (player.playerID.steamID != Provider.client && !quests.isMemberOfSameGroupAs(Player.LocalPlayer))
				{
					continue; // pass on them if they're not us and they're not in our group
				}

				if (!quests.isMarkerPlaced)
				{
					continue; // no marker :(
				}
				ISleekImage compassMarkerImage;
				if (compassMarkerIndex < compassMarkers.Count)
				{
					compassMarkerImage = compassMarkers[compassMarkerIndex];
				}
				else
				{
					compassMarkerImage = Glazier.Get().CreateImage(icons.load<Texture2D>("Marker"));
					compassMarkerImage.PositionOffset_X = -10;
					compassMarkerImage.PositionOffset_Y = -5;
					compassMarkerImage.SizeOffset_X = 20;
					compassMarkerImage.SizeOffset_Y = 20;
					compassMarkersContainer.AddChild(compassMarkerImage);
					compassMarkers.Add(compassMarkerImage);
				}
				++compassMarkerIndex;

				float markerAngle = Mathf.Atan2(quests.markerPosition.x - cameraPosition.x,
												quests.markerPosition.z - cameraPosition.z);
				markerAngle *= Mathf.Rad2Deg;

				Color markerColor = player.markerColor;
				updateCompassElement(compassMarkerImage, viewAngle, markerAngle, out markerColor.a);
				compassMarkerImage.TintColor = markerColor;
			}

			for (int index = compassMarkersVisibleCount - 1; index >= compassMarkerIndex; --index)
			{
				compassMarkers[index].IsVisible = false;
			}
			compassMarkersVisibleCount = compassMarkerIndex;

			UnityEngine.Profiling.Profiler.EndSample();
		}

		private static void updateIcons()
		{
			Player player = Player.LocalPlayer;
			bool showAny = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowStatusIcons);

			int offset = 0;

			bleedingBox.IsVisible = player.life.isBleeding && showAny;
			if (bleedingBox.IsVisible)
			{
				offset += 60;
			}

			brokenBox.PositionOffset_X = offset;
			brokenBox.IsVisible = player.life.isBroken && showAny;
			if (brokenBox.IsVisible)
			{
				offset += 60;
			}

			temperatureBox.PositionOffset_X = offset;
			temperatureBox.IsVisible = player.life.temperature != EPlayerTemperature.NONE && showAny;
			if (temperatureBox.IsVisible)
			{
				offset += 60;
			}

			starvedBox.PositionOffset_X = offset;
			starvedBox.IsVisible = player.life.food == 0 && showAny;
			if (starvedBox.IsVisible)
			{
				offset += 60;
			}

			dehydratedBox.PositionOffset_X = offset;
			dehydratedBox.IsVisible = player.life.water == 0 && showAny;
			if (dehydratedBox.IsVisible)
			{
				offset += 60;
			}

			infectedBox.PositionOffset_X = offset;
			infectedBox.IsVisible = player.life.virus == 0 && showAny;
			if (infectedBox.IsVisible)
			{
				offset += 60;
			}

			drownedBox.PositionOffset_X = offset;
			drownedBox.IsVisible = player.life.oxygen == 0 && showAny;
			if (drownedBox.IsVisible)
			{
				offset += 60;
			}

			asphyxiatingBox.PositionOffset_X = offset;
			asphyxiatingBox.IsVisible = !drownedBox.IsVisible && player.life.isAsphyxiating && showAny;
			if (asphyxiatingBox.IsVisible)
			{
				offset += 60;
			}

			moonBox.PositionOffset_X = offset;
			moonBox.IsVisible = LightingManager.isFullMoon && showAny;
			if (moonBox.IsVisible)
			{
				offset += 60;
			}

			radiationBox.PositionOffset_X = offset;
			radiationBox.IsVisible = player.movement.isRadiated && showAny;
			if (radiationBox.IsVisible)
			{
				offset += 60;
			}

			safeBox.PositionOffset_X = offset;
			safeBox.IsVisible = player.movement.isSafe && showAny;
			if (safeBox.IsVisible)
			{
				offset += 60;
			}

			arrestBox.PositionOffset_X = offset;
			arrestBox.IsVisible = player.animator.gesture == EPlayerGesture.ARREST_START && showAny;
			if (arrestBox.IsVisible)
			{
				offset += 60;
			}

			statusIconsContainer.SizeOffset_X = offset - 10;
			statusIconsContainer.IsVisible = offset > 0;
		}

		private static void updateLifeBoxVisibility()
		{
			Player player = Player.LocalPlayer;
			bool healthVisible = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowHealth);
			bool foodVisible = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowFood);
			bool waterVisible = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowWater);
			bool virusVisible = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowVirus);
			bool staminaVisible = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowStamina);
			bool oxygenVisible = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowOxygen);
			bool hordeInfoVisible = false;

			if (Level.info != null)
			{
				if (Level.info.configData != null)
				{
					healthVisible &= Level.info.configData.PlayerUI_HealthVisible;
					foodVisible &= Level.info.configData.PlayerUI_FoodVisible;
					waterVisible &= Level.info.configData.PlayerUI_WaterVisible;
					virusVisible &= Level.info.configData.PlayerUI_VirusVisible;
					staminaVisible &= Level.info.configData.PlayerUI_StaminaVisible;
					oxygenVisible &= Level.info.configData.PlayerUI_OxygenVisible;
				}

				if (Level.info.type == ELevelType.ARENA)
				{
					levelTextBox.IsVisible = true;
					levelNumberBox.IsVisible = true;

					compassBox.PositionOffset_Y = 60;
				}

				if (Level.info.type != ELevelType.SURVIVAL)
				{
					foodVisible = false;
					waterVisible = false;
					virusVisible = false;

					if (Level.info.type == ELevelType.HORDE)
					{
						oxygenVisible = false;
						hordeInfoVisible = true;
					}
				}
			}

			int lifeBoxOffset = 5;

			healthIcon.IsVisible = healthVisible;
			healthProgress.IsVisible = healthVisible;
			if (healthVisible)
			{
				healthIcon.PositionOffset_Y = lifeBoxOffset;
				healthProgress.PositionOffset_Y = lifeBoxOffset + 5;
				lifeBoxOffset += 30;
			}

			foodIcon.IsVisible = foodVisible;
			foodProgress.IsVisible = foodVisible;
			if (foodVisible)
			{
				foodIcon.PositionOffset_Y = lifeBoxOffset;
				foodProgress.PositionOffset_Y = lifeBoxOffset + 5;
				lifeBoxOffset += 30;
			}

			waterIcon.IsVisible = waterVisible;
			waterProgress.IsVisible = waterVisible;
			if (waterVisible)
			{
				waterIcon.PositionOffset_Y = lifeBoxOffset;
				waterProgress.PositionOffset_Y = lifeBoxOffset + 5;
				lifeBoxOffset += 30;
			}

			virusIcon.IsVisible = virusVisible;
			virusProgress.IsVisible = virusVisible;
			if (virusVisible)
			{
				virusIcon.PositionOffset_Y = lifeBoxOffset;
				virusProgress.PositionOffset_Y = lifeBoxOffset + 5;
				lifeBoxOffset += 30;
			}

			staminaIcon.IsVisible = staminaVisible;
			staminaProgress.IsVisible = staminaVisible;
			if (staminaVisible)
			{
				staminaIcon.PositionOffset_Y = lifeBoxOffset;
				staminaProgress.PositionOffset_Y = lifeBoxOffset + 5;
				lifeBoxOffset += 30;
			}

			waveLabel.IsVisible = hordeInfoVisible;
			scoreLabel.IsVisible = hordeInfoVisible;
			if (hordeInfoVisible)
			{
				waveLabel.PositionOffset_Y = lifeBoxOffset;
				scoreLabel.PositionOffset_Y = lifeBoxOffset;
				lifeBoxOffset += 30;
			}

			oxygenIcon.IsVisible = oxygenVisible;
			oxygenProgress.IsVisible = oxygenVisible;
			if (oxygenVisible)
			{
				oxygenIcon.PositionOffset_Y = lifeBoxOffset;
				oxygenProgress.PositionOffset_Y = lifeBoxOffset + 5;
				lifeBoxOffset += 30;
			}

			lifeBox.SizeOffset_Y = lifeBoxOffset - 5;
			lifeBox.PositionOffset_Y = -lifeBox.SizeOffset_Y;
			lifeBox.IsVisible = lifeBox.SizeOffset_Y > 0;
			statusIconsContainer.PositionOffset_Y = lifeBox.PositionOffset_Y - 60;
		}

		private static void UpdateVehicleBoxVisibility()
		{
			bool visible = vehicleVisibleByDefault;
			visible &= Player.LocalPlayer.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowVehicleStatus);
			vehicleBox.IsVisible = visible;
		}

		private static void onBleedingUpdated(bool newBleeding)
		{
			updateIcons();
		}

		private static void onBrokenUpdated(bool newBroken)
		{
			updateIcons();
		}

		private static void onTemperatureUpdated(EPlayerTemperature newTemperature)
		{
			switch (newTemperature)
			{
				case EPlayerTemperature.FREEZING:
					temperatureBox.icon = icons.load<Texture2D>("Freezing");
					break;
				case EPlayerTemperature.COLD:
					temperatureBox.icon = icons.load<Texture2D>("Cold");
					break;
				case EPlayerTemperature.WARM:
					temperatureBox.icon = icons.load<Texture2D>("Warm");
					break;
				case EPlayerTemperature.BURNING:
					temperatureBox.icon = icons.load<Texture2D>("Burning");
					break;
				case EPlayerTemperature.COVERED:
					temperatureBox.icon = icons.load<Texture2D>("Covered");
					break;
				case EPlayerTemperature.ACID:
					temperatureBox.icon = icons.load<Texture2D>("Acid");
					break;
				default:
					temperatureBox.icon = null;
					break;
			}

			updateIcons();
		}

		private static void onMoonUpdated(bool isFullMoon)
		{
			updateIcons();
		}

		private static void onExperienceUpdated(uint newExperience)
		{
			scoreLabel.Text = localization.format("Score", newExperience.ToString());
		}

		private static void onWaveUpdated(bool newWaveReady, int newWaveIndex)
		{
			waveLabel.Text = localization.format("Round", newWaveIndex);

			if (newWaveReady)
			{
				PlayerUI.message(EPlayerMessage.WAVE_ON, "");
			}
			else
			{
				PlayerUI.message(EPlayerMessage.WAVE_OFF, "");
			}
		}

		private static void onSeated(bool isDriver, bool inVehicle, bool wasVehicle, InteractableVehicle oldVehicle, InteractableVehicle newVehicle)
		{
			if (isDriver && inVehicle)
			{
				int offset = 5;

				// Fuel shows stamina for bikes, but we hide it for electric vehicles.
				bool fuelVisible = newVehicle.usesFuel || newVehicle.asset.isStaminaPowered;
				fuelIcon.IsVisible = fuelVisible;
				fuelProgress.IsVisible = fuelVisible;
				if (fuelVisible)
				{
					fuelIcon.PositionOffset_Y = offset;
					fuelProgress.PositionOffset_Y = offset + 5;
					offset += 30;
				}

				speedIcon.PositionOffset_Y = offset;
				speedProgress.PositionOffset_Y = offset + 5;
				offset += 30;

				hpIcon.IsVisible = newVehicle.usesHealth;
				hpProgress.IsVisible = newVehicle.usesHealth;
				if (newVehicle.usesHealth)
				{
					hpIcon.PositionOffset_Y = offset;
					hpProgress.PositionOffset_Y = offset + 5;
					offset += 30;
				}

				batteryChargeIcon.IsVisible = newVehicle.usesBattery;
				batteryChargeProgress.IsVisible = newVehicle.usesBattery;
				if (newVehicle.usesBattery)
				{
					batteryChargeIcon.PositionOffset_Y = offset;
					batteryChargeProgress.PositionOffset_Y = offset + 5;
					offset += 30;
				}

				vehicleEngineLabel.IsVisible = newVehicle.asset.UsesEngineRpmAndGears && newVehicle.asset.AllowsEngineRpmAndGearsInHud;
				if (vehicleEngineLabel.IsVisible)
				{
					vehicleEngineLabel.PositionOffset_Y = offset - 5;
					offset += 30;
				}

				vehicleBox.SizeOffset_Y = offset - 5;
				vehicleBox.PositionOffset_Y = -vehicleBox.SizeOffset_Y;

				if (newVehicle.passengers[Player.LocalPlayer.movement.getSeat()].turret != null)
				{
					vehicleBox.PositionOffset_Y -= 80;
				}

				vehicleVisibleByDefault = true;
			}
			else
			{
				vehicleVisibleByDefault = false;
			}

			UpdateVehicleBoxVisibility();
		}

		private static void onVehicleUpdated(bool isDriveable, ushort newFuel, ushort maxFuel, float newSpeed, float minSpeed, float maxSpeed, ushort newHealth, ushort maxHealth, ushort newBatteryCharge)
		{
			if (isDriveable)
			{
				fuelProgress.state = newFuel / (float) maxFuel;

				float speed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
				if (speed > 0)
				{
					speed /= maxSpeed;
				}
				else
				{
					speed /= minSpeed;
				}

				speedProgress.state = speed;

				if (OptionsSettings.metric)
				{
					speedProgress.measure = (int) MeasurementTool.speedToKPH(Mathf.Abs(newSpeed));
				}
				else
				{
					speedProgress.measure = (int) MeasurementTool.KPHToMPH(MeasurementTool.speedToKPH(Mathf.Abs(newSpeed)));
				}

				batteryChargeProgress.state = newBatteryCharge / 10000f;
				hpProgress.state = newHealth / (float) maxHealth;

				InteractableVehicle vehicle = Player.LocalPlayer.movement.getVehicle();
				if (vehicle.asset != null && vehicle.asset.canBeLocked)
				{
					vehicleLockedLabel.Text = localization.format(vehicle.isLocked ? "Vehicle_Locked" : "Vehicle_Unlocked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
					vehicleLockedLabel.IsVisible = true;
				}
				else
				{
					vehicleLockedLabel.IsVisible = false;
				}

				if (vehicleEngineLabel.IsVisible)
				{
					string gearText;
					if (vehicle.GearNumber < 0)
					{
						gearText = localization.format("VehicleGear_Reverse");
					}
					else if (vehicle.GearNumber == 0)
					{
						gearText = localization.format("VehicleGear_Neutral");
					}
					else
					{
						gearText = vehicle.GearNumber.ToString();
					}
					vehicleEngineLabel.Text = localization.format("VehicleEngineStatus", gearText, Mathf.RoundToInt(vehicle.AnimatedEngineRpm));
				}
			}

			vehicleVisibleByDefault = isDriveable;
			UpdateVehicleBoxVisibility();
		}

		private static void updateGasmask()
		{
			if (Player.LocalPlayer.movement.isRadiated)
			{
				ItemMaskAsset asset = Player.LocalPlayer.clothing.maskAsset;
				if (asset != null && asset.proofRadiation)
				{
					gasmaskIcon.Refresh(asset.id, Player.LocalPlayer.clothing.maskQuality, Player.LocalPlayer.clothing.maskState, asset);

					gasmaskProgress.state = Player.LocalPlayer.clothing.maskQuality / 100.0f;
					gasmaskProgress.color = ItemTool.getQualityColor(Player.LocalPlayer.clothing.maskQuality / 100.0f);

					gasmaskBox.IsVisible = true;
				}
				else
				{
					gasmaskBox.IsVisible = false;
				}
			}
			else
			{
				gasmaskBox.IsVisible = false;
			}
		}

		private static void onMaskUpdated(ushort newMask, byte newMaskQuality, byte[] newMaskState)
		{
			updateGasmask();
		}

		private static void onSafetyUpdated(bool isSafe)
		{
			updateIcons();

			if (isSafe)
			{
				PlayerUI.message(EPlayerMessage.SAFEZONE_ON, "");
			}
			else
			{
				PlayerUI.message(EPlayerMessage.SAFEZONE_OFF, "");
			}
		}

		private static void onRadiationUpdated(bool isRadiated)
		{
			updateIcons();

			if (isRadiated)
			{
				PlayerUI.message(EPlayerMessage.DEADZONE_ON, "");
			}
			else
			{
				PlayerUI.message(EPlayerMessage.DEADZONE_OFF, "");
			}

			updateGasmask();
		}

		private static void onGestureUpdated(EPlayerGesture gesture)
		{
			updateIcons();
		}

		private void OnCustomAllowTalkingChanged(PlayerVoice voice)
		{
			SynchronizeOutboundVoiceChatVisible();
		}

		private static void onTalked(bool isTalking)
		{
			voiceBox.IsVisible = isTalking;
		}

		internal static void UpdateTrackedQuest()
		{
			QuestAsset asset = Player.LocalPlayer.quests.GetTrackedQuest();

			if (asset == null)
			{
				trackedQuestTitle.IsVisible = false;
				trackedQuestBar.IsVisible = false;

				return;
			}

			trackedQuestTitle.Text = asset.questName;

			bool areAllConditionsMet = true;
			if (asset.conditions != null)
			{
				trackedQuestBar.RemoveAllChildren();

				areConditionsMet.Clear();
				foreach (INPCCondition condition in asset.conditions)
				{
					areConditionsMet.Add(condition.isConditionMet(Player.LocalPlayer));
				}

				int offset = 5;
				for (int conditionIndex = 0; conditionIndex < asset.conditions.Length; conditionIndex++)
				{
					INPCCondition condition = asset.conditions[conditionIndex];
					if (areConditionsMet[conditionIndex])
					{
						// This condition is already complete and should not be shown.
						continue;
					}

					if (!condition.AreUIRequirementsMet(areConditionsMet))
					{
						// This condition should not yet be visible.
						continue;
					}

					string text = condition.formatCondition(Player.LocalPlayer);

					if (string.IsNullOrEmpty(text))
					{
						continue;
					}

					ISleekLabel trackedQuestCondition = Glazier.Get().CreateLabel();
					trackedQuestCondition.PositionOffset_X = -300;
					trackedQuestCondition.PositionOffset_Y = offset;
					trackedQuestCondition.SizeOffset_X = 500;
					trackedQuestCondition.SizeOffset_Y = 30;
					trackedQuestCondition.AllowRichText = true;
					trackedQuestCondition.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
					trackedQuestCondition.TextAlignment = TextAnchor.MiddleRight;
					trackedQuestCondition.Text = text;
					trackedQuestCondition.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
					trackedQuestBar.AddChild(trackedQuestCondition);

					offset += 20;

					areAllConditionsMet = false;
				}
			}

			trackedQuestTitle.IsVisible = !areAllConditionsMet;
			trackedQuestBar.IsVisible = trackedQuestTitle.IsVisible;
		}

		private static void OnTrackedQuestUpdated(PlayerQuests quests)
		{
			UpdateTrackedQuest();
		}

		private static void OnChatMessageReceived()
		{
			if (chatScrollViewV2 != null && ChatManager.receivedChatHistory.Count > 0)
			{
				if (chatEntriesV2.Count >= Provider.preferenceData.Chat.History_Length)
				{
					SleekChatEntryV2 oldEntry = chatEntriesV2.Dequeue();
					chatScrollViewV2.RemoveChild(oldEntry);
				}

				SleekChatEntryV2 chatEntry = new SleekChatEntryV2();
				chatEntry.shouldFadeOutWithAge = Glazier.Get().SupportsRichTextAlpha;
				chatEntry.forceVisibleWhileBrowsingChatHistory = chatting;
				chatEntry.representingChatMessage = ChatManager.receivedChatHistory[0];
				chatScrollViewV2.AddChild(chatEntry);
				chatEntriesV2.Enqueue(chatEntry);

				if (!chatting)
				{
					chatScrollViewV2.ScrollToBottom();
				}
			}
			else if (chatHistoryBoxV1 != null)
			{
				int numHistoryMessages = 0;
				for (int historyIndex = 0; historyIndex < ChatManager.receivedChatHistory.Count; historyIndex++)
				{
					if (historyIndex >= chatHistoryLabelsV1.Length)
						break; // Never happen? Just to be safe.

					// History index 0 is at the top of the list, history index n-1 is at the bottom
					// we want the top message to be the oldest, so start at the end of the chat history and work backwards
					int messageIndex = ChatManager.receivedChatHistory.Count - 1 - historyIndex;

					chatHistoryLabelsV1[historyIndex].representingChatMessage = ChatManager.receivedChatHistory[messageIndex];
					numHistoryMessages++;
				}

				// Resize and push down history so that it lines up with preview labels until we have enough history messages
				int historyHeight = numHistoryMessages * 40;
				int previewHeight = chatPreviewLabelsV1.Length * 40;
				chatHistoryBoxV1.SizeOffset_Y = Mathf.Min(historyHeight, previewHeight);
				chatHistoryBoxV1.PositionOffset_Y = Mathf.Max(0, previewHeight - chatHistoryBoxV1.SizeOffset_Y);
				chatHistoryBoxV1.ContentSizeOffset = new Vector2(0.0f, historyHeight);

				for (int previewIndex = 0; previewIndex < chatPreviewLabelsV1.Length; previewIndex++)
				{
					// Preview index 0 is at the top of the screen, preview index n-1 is lower below that
					// we want the newest message at the bottom (which is received chat index 0),
					// so the message index is flipped 0 = n - 1 - 0
					int messageIndex = chatPreviewLabelsV1.Length - 1 - previewIndex;
					if (messageIndex >= ChatManager.receivedChatHistory.Count)
						continue;

					chatPreviewLabelsV1[previewIndex].representingChatMessage = ChatManager.receivedChatHistory[messageIndex];
				}
			}
		}

		private static void onVotingStart(SteamPlayer origin, SteamPlayer target, byte votesNeeded)
		{
			isVoteMessaged = false;

			voteBox.Text = "";
			voteBox.IsVisible = true;

			voteInfoLabel.IsVisible = true;
			votesNeededLabel.IsVisible = true;
			voteYesLabel.IsVisible = true;
			voteNoLabel.IsVisible = true;

			voteInfoLabel.Text = localization.format("Vote_Kick", origin.playerID.characterName, origin.playerID.playerName, target.playerID.characterName, target.playerID.playerName);
			votesNeededLabel.Text = localization.format("Votes_Needed", votesNeeded);

			voteYesLabel.Text = localization.format("Vote_Yes", KeyCode.F1, 0);
			voteNoLabel.Text = localization.format("Vote_No", KeyCode.F2, 0);
		}

		private static void onVotingUpdate(byte voteYes, byte voteNo)
		{
			voteYesLabel.Text = localization.format("Vote_Yes", KeyCode.F1, voteYes);
			voteNoLabel.Text = localization.format("Vote_No", KeyCode.F2, voteNo);
		}

		private static void onVotingStop(EVotingMessage message)
		{
			voteInfoLabel.IsVisible = false;
			votesNeededLabel.IsVisible = false;
			voteYesLabel.IsVisible = false;
			voteNoLabel.IsVisible = false;

			if (message == EVotingMessage.PASS)
			{
				voteBox.Text = localization.format("Vote_Pass");
			}
			else if (message == EVotingMessage.FAIL)
			{
				voteBox.Text = localization.format("Vote_Fail");
			}

			isVoteMessaged = true;
			lastVoteMessage = Time.realtimeSinceStartup;
		}

		private static void onVotingMessage(EVotingMessage message)
		{
			voteBox.IsVisible = true;

			voteInfoLabel.IsVisible = false;
			votesNeededLabel.IsVisible = false;
			voteYesLabel.IsVisible = false;
			voteNoLabel.IsVisible = false;

			if (message == EVotingMessage.OFF)
			{
				voteBox.Text = localization.format("Vote_Off");
			}
			else if (message == EVotingMessage.DELAY)
			{
				voteBox.Text = localization.format("Vote_Delay");
			}
			else if (message == EVotingMessage.PLAYERS)
			{
				voteBox.Text = localization.format("Vote_Players");
			}

			isVoteMessaged = true;
			lastVoteMessage = Time.realtimeSinceStartup;
		}

		private static void onArenaMessageUpdated(EArenaMessage newArenaMessage)
		{
			switch (newArenaMessage)
			{
				case EArenaMessage.LOBBY:
					levelTextBox.Text = localization.format("Arena_Lobby");
					return;
				case EArenaMessage.WARMUP:
					levelTextBox.Text = localization.format("Arena_Warm_Up");
					return;
				case EArenaMessage.PLAY:
					levelTextBox.Text = localization.format("Arena_Play");
					return;
				case EArenaMessage.LOSE:
					levelTextBox.Text = localization.format("Arena_Lose");
					return;
				case EArenaMessage.INTERMISSION:
					levelTextBox.Text = localization.format("Arena_Intermission");
					return;
			}
		}

		private static void onArenaPlayerUpdated(ulong[] playerIDs, EArenaMessage newArenaMessage)
		{
			List<SteamPlayer> steamPlayers = new List<SteamPlayer>();
			for (int index = 0; index < playerIDs.Length; index++)
			{
				SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(playerIDs[index]);

				if (steamPlayer == null)
				{
					continue;
				}

				steamPlayers.Add(steamPlayer);
			}

			if (steamPlayers.Count == 0)
			{
				return;
			}

			string listPlayers = "";
			for (int index = 0; index < steamPlayers.Count; index++)
			{
				SteamPlayer steamPlayer = steamPlayers[index];

				if (index == 0)
				{
					listPlayers += steamPlayer.playerID.characterName;
				}
				else if (index == steamPlayers.Count - 1)
				{
					listPlayers += localization.format("List_Joint_1") + steamPlayer.playerID.characterName;
				}
				else
				{
					listPlayers += localization.format("List_Joint_0") + steamPlayer.playerID.characterName;
				}
			}

			switch (newArenaMessage)
			{
				case EArenaMessage.DIED:
					levelTextBox.Text = localization.format("Arena_Died", listPlayers);
					return;
				case EArenaMessage.ABANDONED:
					levelTextBox.Text = localization.format("Arena_Abandoned", listPlayers);
					return;
				case EArenaMessage.WIN:
					levelTextBox.Text = localization.format("Arena_Win", listPlayers);
					return;
			}
		}

		private static void onLevelNumberUpdated(int newLevelNumber)
		{
			levelNumberBox.Text = newLevelNumber.ToString();
		}

		private static void onClickedFaceButton(ISleekElement button)
		{
			byte index;
			for (index = 0; index < faceButtons.Length; index++)
			{
				if (faceButtons[index] == button)
				{
					break;
				}
			}

			Player.LocalPlayer.clothing.sendSwapFace(index);
			closeGestures();
		}

		private static void onClickedSurrenderButton(ISleekElement button)
		{
			if (Player.LocalPlayer.animator.gesture == EPlayerGesture.SURRENDER_START)
			{
				Player.LocalPlayer.animator.sendGesture(EPlayerGesture.SURRENDER_STOP, true);
			}
			else
			{
				Player.LocalPlayer.animator.sendGesture(EPlayerGesture.SURRENDER_START, true);
			}

			closeGestures();
		}

		private static void onClickedPointButton(ISleekElement button)
		{
			Player.LocalPlayer.animator.sendGesture(EPlayerGesture.POINT, true);

			closeGestures();
		}

		private static void onClickedWaveButton(ISleekElement button)
		{
			Player.LocalPlayer.animator.sendGesture(EPlayerGesture.WAVE, true);

			closeGestures();
		}

		private static void onClickedSaluteButton(ISleekElement button)
		{
			Player.LocalPlayer.animator.sendGesture(EPlayerGesture.SALUTE, true);

			closeGestures();
		}

		private static void onClickedRestButton(ISleekElement button)
		{
			if (Player.LocalPlayer.animator.gesture == EPlayerGesture.REST_START)
			{
				Player.LocalPlayer.animator.sendGesture(EPlayerGesture.REST_STOP, true);
			}
			else
			{
				Player.LocalPlayer.animator.sendGesture(EPlayerGesture.REST_START, true);
			}

			closeGestures();
		}

		private static void onClickedFacepalmButton(ISleekElement button)
		{
			Player.LocalPlayer.animator.sendGesture(EPlayerGesture.FACEPALM, true);

			closeGestures();
		}

		private static void onClickedTPoseButton(ISleekElement button)
		{
			if (Player.LocalPlayer.animator.gesture == EPlayerGesture.T_POSE_START)
			{
				Player.LocalPlayer.animator.sendGesture(EPlayerGesture.T_POSE_STOP, true);
			}
			else
			{
				Player.LocalPlayer.animator.sendGesture(EPlayerGesture.T_POSE_START, true);
			}

			closeGestures();
		}

		private void SynchronizeOutboundVoiceChatVisible()
		{
			bool visible = false;
			if (!Provider.isServer)
			{
				visible |= (!OptionsSettings.EnableOutboundVoiceChat && OptionsSettings.ShowOutboundVoiceChatOffHint);
				visible |= !Player.LocalPlayer.voice.GetCustomAllowTalking();
			}

			voiceOutboundOffIcon.IsVisible = visible;
		}

		private void OnUnitSystemChanged()
		{
			speedProgress.suffix = OptionsSettings.metric ? " kph" : " mph";
		}

		public void OnDestroy()
		{
			ChatManager.onChatMessageReceived -= OnChatMessageReceived;
			ChatManager.onVotingStart -= onVotingStart;
			ChatManager.onVotingUpdate -= onVotingUpdate;
			ChatManager.onVotingStop -= onVotingStop;
			ChatManager.onVotingMessage -= onVotingMessage;

			LevelManager.onArenaMessageUpdated -= onArenaMessageUpdated;
			LevelManager.onArenaPlayerUpdated -= onArenaPlayerUpdated;
			LevelManager.onLevelNumberUpdated -= onLevelNumberUpdated;

			OptionsSettings.OnShowOutboundVoiceChatOffHintChanged -= SynchronizeOutboundVoiceChatVisible;
			OptionsSettings.OnEnableOutboundVoiceChatChanged -= SynchronizeOutboundVoiceChatVisible;
			OptionsSettings.OnUnitSystemChanged -= OnUnitSystemChanged;

			Player.LocalPlayer.life.OnIsAsphyxiatingChanged -= OnIsAsphyxiatingChanged;
		}

		public PlayerLifeUI()
		{
			localization = Localization.read("/Player/PlayerLife.dat");
			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerLife");

			_container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = true;
			chatting = false;

			if (Provider.preferenceData.Chat.History_Length > ChatPreferenceData.DEFAULT_HISTORY_LENGTH)
			{
				// Nelson 2025-08-12: if raised very high (e.g., 5k entries) this can lead to performance issues.
				// Initially I wanted to clamp it, but from initial feedback that's unpopular. At least logging it
				// we can catch by requesting Client.log even if we forget. (public issue #4177)
				UnturnedLog.warn($"Chat history length ({Provider.preferenceData.Chat.History_Length}) is higher than the default ({ChatPreferenceData.DEFAULT_HISTORY_LENGTH}) and may be the cause of performance issues if significantly higher");
			}

			if (Glazier.Get().SupportsAutomaticLayout)
			{
				chatScrollViewV2 = Glazier.Get().CreateScrollView();
				chatScrollViewV2.SizeOffset_X = 630;
				chatScrollViewV2.SizeOffset_Y = Provider.preferenceData.Chat.Preview_Length * 40;
				chatScrollViewV2.ScaleContentToWidth = true;
				chatScrollViewV2.ContentUseManualLayout = false;
				chatScrollViewV2.AlignContentToBottom = true;
				chatScrollViewV2.VerticalScrollbarVisibility = ESleekScrollbarVisibility.Hidden;
				chatScrollViewV2.IsRaycastTarget = false;
				container.AddChild(chatScrollViewV2);

				chatEntriesV2 = new Queue<SleekChatEntryV2>(Provider.preferenceData.Chat.History_Length);
			}
			else
			{
				chatHistoryBoxV1 = Glazier.Get().CreateScrollView();
				chatHistoryBoxV1.SizeOffset_X = 630;
				chatHistoryBoxV1.ScaleContentToWidth = true;
				container.AddChild(chatHistoryBoxV1);
				chatHistoryBoxV1.IsVisible = false;

				chatHistoryLabelsV1 = new SleekChatEntryV1[Provider.preferenceData.Chat.History_Length];
				for (int index = 0; index < chatHistoryLabelsV1.Length; index++)
				{
					SleekChatEntryV1 chat = new SleekChatEntryV1();
					chat.PositionOffset_Y = index * 40;
					chat.SizeOffset_X = chatHistoryBoxV1.SizeOffset_X - 30;
					chat.SizeOffset_Y = 40;
					chat.shouldFadeOutWithAge = false;
					chatHistoryBoxV1.AddChild(chat);

					chatHistoryLabelsV1[index] = chat;
				}

				bool chatPreviewFadeOutWithAge = Glazier.Get().SupportsRichTextAlpha;
				chatPreviewLabelsV1 = new SleekChatEntryV1[Provider.preferenceData.Chat.Preview_Length];
				for (int index = 0; index < chatPreviewLabelsV1.Length; index++)
				{
					SleekChatEntryV1 chat = new SleekChatEntryV1();
					chat.PositionOffset_Y = index * 40;
					chat.SizeOffset_X = chatHistoryBoxV1.SizeOffset_X - 30;
					chat.SizeOffset_Y = 40;
					chat.shouldFadeOutWithAge = chatPreviewFadeOutWithAge;
					container.AddChild(chat);

					chatPreviewLabelsV1[index] = chat;
				}
			}

			chatField = Glazier.Get().CreateStringField();
			chatField.PositionOffset_Y = (Provider.preferenceData.Chat.Preview_Length * 40) + 10;
			chatField.SizeOffset_X = 500;
			chatField.PositionOffset_X = -chatField.SizeOffset_X - 50;
			chatField.SizeOffset_Y = 30;
			chatField.TextAlignment = TextAnchor.MiddleLeft;
			chatField.MaxLength = ChatManager.MAX_MESSAGE_LENGTH;
			chatField.OnTextEscaped += OnChatFieldEscaped;
			container.AddChild(chatField);

			chatModeButton = new SleekButtonState();
			chatModeButton.UseContentTooltip = true;
			chatModeButton.setContent(
				new GUIContent(localization.format("Mode_Global"), localization.format("Mode_Global_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.global))),
				new GUIContent(localization.format("Mode_Local"), localization.format("Mode_Local_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.local))),
				new GUIContent(localization.format("Mode_Group"), localization.format("Mode_Group_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.group)))
				);
			chatModeButton.PositionOffset_X = -100;
			chatModeButton.SizeOffset_X = 100;
			chatModeButton.SizeOffset_Y = 30;
			chatModeButton.onSwappedState = OnSwappedChatModeState;
			chatField.AddChild(chatModeButton);

			sendChatButton = new SleekButtonIcon(icons.load<Texture2D>("SendChat"));
			sendChatButton.PositionScale_X = 1.0f;
			sendChatButton.SizeOffset_X = 30;
			sendChatButton.SizeOffset_Y = 30;
			sendChatButton.tooltip = localization.format("SendChat_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Return));
			sendChatButton.iconColor = ESleekTint.FOREGROUND;
			sendChatButton.onClickedButton += OnSendChatButtonClicked;
			chatField.AddChild(sendChatButton);

			voteBox = Glazier.Get().CreateBox();
			voteBox.PositionOffset_X = -430;
			voteBox.PositionScale_X = 1.0f;
			voteBox.SizeOffset_X = 430;
			voteBox.SizeOffset_Y = 90;
			container.AddChild(voteBox);
			voteBox.IsVisible = false;

			voteInfoLabel = Glazier.Get().CreateLabel();
			voteInfoLabel.SizeOffset_Y = 30;
			voteInfoLabel.SizeScale_X = 1.0f;
			voteBox.AddChild(voteInfoLabel);

			votesNeededLabel = Glazier.Get().CreateLabel();
			votesNeededLabel.PositionOffset_Y = 30;
			votesNeededLabel.SizeOffset_Y = 30;
			votesNeededLabel.SizeScale_X = 1.0f;
			voteBox.AddChild(votesNeededLabel);

			voteYesLabel = Glazier.Get().CreateLabel();
			voteYesLabel.PositionOffset_Y = 60;
			voteYesLabel.SizeOffset_Y = 30;
			voteYesLabel.SizeScale_X = 0.5f;
			voteBox.AddChild(voteYesLabel);

			voteNoLabel = Glazier.Get().CreateLabel();
			voteNoLabel.PositionOffset_Y = 60;
			voteNoLabel.PositionScale_X = 0.5f;
			voteNoLabel.SizeOffset_Y = 30;
			voteNoLabel.SizeScale_X = 0.5f;
			voteBox.AddChild(voteNoLabel);

			voiceBox = new SleekBoxIcon(icons.load<Texture2D>("Voice"));
			voiceBox.PositionOffset_Y = chatField.PositionOffset_Y + 40;
			voiceBox.SizeOffset_X = 50;
			voiceBox.SizeOffset_Y = 50;
			voiceBox.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(voiceBox);
			voiceBox.IsVisible = false;

			voiceOutboundOffIcon = Glazier.Get().CreateImage(icons.load<Texture2D>("VoiceOutboundOff"));
			voiceOutboundOffIcon.PositionOffset_X = 60;
			voiceOutboundOffIcon.PositionOffset_Y = chatField.PositionOffset_Y + 40;
			voiceOutboundOffIcon.SizeOffset_X = 40;
			voiceOutboundOffIcon.SizeOffset_Y = 40;
			voiceOutboundOffIcon.TintColor = new SleekColor(ESleekTint.FOREGROUND, 0.5f);
			container.AddChild(voiceOutboundOffIcon);
			SynchronizeOutboundVoiceChatVisible();

			trackedQuestTitle = Glazier.Get().CreateLabel();
			trackedQuestTitle.PositionOffset_X = -500;
			trackedQuestTitle.PositionOffset_Y = 200;
			trackedQuestTitle.PositionScale_X = 1.0f;
			trackedQuestTitle.SizeOffset_X = 500;
			trackedQuestTitle.SizeOffset_Y = 35;
			trackedQuestTitle.AllowRichText = true;
			trackedQuestTitle.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			trackedQuestTitle.FontSize = ESleekFontSize.Medium;
			trackedQuestTitle.TextAlignment = TextAnchor.LowerRight;
			trackedQuestTitle.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			container.AddChild(trackedQuestTitle);

			trackedQuestBar = Glazier.Get().CreateImage();
			trackedQuestBar.PositionOffset_X = -200;
			trackedQuestBar.PositionOffset_Y = 240;
			trackedQuestBar.PositionScale_X = 1.0f;
			trackedQuestBar.SizeOffset_X = 200;
			trackedQuestBar.SizeOffset_Y = 3;
			trackedQuestBar.Texture = GlazierResources.PixelTexture;
			trackedQuestBar.TintColor = ESleekTint.FOREGROUND;
			container.AddChild(trackedQuestBar);

			levelTextBox = Glazier.Get().CreateBox();
			levelTextBox.PositionOffset_X = -180;
			levelTextBox.PositionScale_X = 0.5f;
			levelTextBox.SizeOffset_X = 300;
			levelTextBox.SizeOffset_Y = 50;
			levelTextBox.FontSize = ESleekFontSize.Medium;
			container.AddChild(levelTextBox);
			levelTextBox.IsVisible = false;

			levelNumberBox = Glazier.Get().CreateBox();
			levelNumberBox.PositionOffset_X = 130;
			levelNumberBox.PositionScale_X = 0.5f;
			levelNumberBox.SizeOffset_X = 50;
			levelNumberBox.SizeOffset_Y = 50;
			levelNumberBox.FontSize = ESleekFontSize.Medium;
			container.AddChild(levelNumberBox);
			levelNumberBox.IsVisible = false;

			cachedCompassSearch = -1;
			cachedHasCompass = false;

			compassBox = Glazier.Get().CreateBox();
			compassBox.PositionOffset_X = -180;
			compassBox.PositionScale_X = 0.5f;
			compassBox.SizeOffset_X = 360;
			compassBox.SizeOffset_Y = 50;
			compassBox.FontSize = ESleekFontSize.Medium;
			container.AddChild(compassBox);
			compassBox.IsVisible = false;

			compassLabelsContainer = Glazier.Get().CreateFrame();
			compassLabelsContainer.PositionOffset_X = 10;
			compassLabelsContainer.PositionOffset_Y = 10;
			compassLabelsContainer.SizeOffset_X = -20;
			compassLabelsContainer.SizeOffset_Y = -20;
			compassLabelsContainer.SizeScale_X = 1;
			compassLabelsContainer.SizeScale_Y = 1;
			compassBox.AddChild(compassLabelsContainer);

			compassMarkersContainer = Glazier.Get().CreateFrame();
			compassMarkersContainer.PositionOffset_X = 10;
			compassMarkersContainer.PositionOffset_Y = 10;
			compassMarkersContainer.SizeOffset_X = -20;
			compassMarkersContainer.SizeOffset_Y = -20;
			compassMarkersContainer.SizeScale_X = 1;
			compassMarkersContainer.SizeScale_Y = 1;
			compassBox.AddChild(compassMarkersContainer);
			compassMarkers = new List<ISleekImage>();
			compassMarkersVisibleCount = 0;

			compassLabels = new ISleekLabel[72];
			for (int compassIndex = 0; compassIndex < compassLabels.Length; compassIndex++)
			{
				ISleekLabel compassLabel = Glazier.Get().CreateLabel();
				compassLabel.PositionOffset_X = -25;
				compassLabel.SizeOffset_X = 50;
				compassLabel.SizeOffset_Y = 30;
				compassLabel.Text = (compassIndex * 5).ToString();
				compassLabel.TextColor = new Color(0.75f, 0.75f, 0.75f);
				compassLabelsContainer.AddChild(compassLabel);

				compassLabels[compassIndex] = compassLabel;
			}

			ISleekLabel north = getCompassLabelByAngle(0);
			north.FontSize = ESleekFontSize.Large;
			north.Text = "N";
			north.TextColor = Palette.COLOR_R;

			ISleekLabel northEast = getCompassLabelByAngle(45);
			northEast.FontSize = ESleekFontSize.Medium;
			northEast.Text = "NE";
			northEast.TextColor = new Color(1, 1, 1);

			ISleekLabel east = getCompassLabelByAngle(90);
			east.FontSize = ESleekFontSize.Large;
			east.Text = "E";
			east.TextColor = new Color(1, 1, 1);

			ISleekLabel southEast = getCompassLabelByAngle(135);
			southEast.FontSize = ESleekFontSize.Medium;
			southEast.Text = "SE";
			southEast.TextColor = new Color(1, 1, 1);

			ISleekLabel south = getCompassLabelByAngle(180);
			south.FontSize = ESleekFontSize.Large;
			south.Text = "S";
			south.TextColor = new Color(1, 1, 1);

			ISleekLabel southWest = getCompassLabelByAngle(225);
			southWest.FontSize = ESleekFontSize.Medium;
			southWest.Text = "SW";
			southWest.TextColor = new Color(1, 1, 1);

			ISleekLabel west = getCompassLabelByAngle(270);
			west.FontSize = ESleekFontSize.Large;
			west.Text = "W";
			west.TextColor = new Color(1, 1, 1);

			ISleekLabel northWest = getCompassLabelByAngle(315);
			northWest.FontSize = ESleekFontSize.Medium;
			northWest.Text = "NW";
			northWest.TextColor = new Color(1, 1, 1);

			hotbarContainer = Glazier.Get().CreateFrame();
			hotbarContainer.PositionScale_X = 0.5f;
			hotbarContainer.PositionScale_Y = 1.0f;
			hotbarContainer.PositionOffset_Y = -200;
			container.AddChild(hotbarContainer);
			hotbarContainer.IsVisible = false;

			cachedHotbarSearch = -1;
			previousEquippedHotbarIndex = -1;
			hotbarItems = new SleekHotbarEntry[10];
			for (int hotbarIndex = 0; hotbarIndex < hotbarItems.Length; hotbarIndex++)
			{
				SleekHotbarEntry hotbarEntry = new SleekHotbarEntry(hotbarIndex);
				hotbarContainer.AddChild(hotbarEntry);
				hotbarItems[hotbarIndex] = hotbarEntry;
				hotbarEntry.IsVisible = false;
			}

			statTrackerLabel = Glazier.Get().CreateLabel();
			statTrackerLabel.PositionOffset_X = -100;
			statTrackerLabel.PositionOffset_Y = -30;
			statTrackerLabel.PositionScale_X = 0.5f;
			statTrackerLabel.PositionScale_Y = 1.0f;
			statTrackerLabel.SizeOffset_X = 200;
			statTrackerLabel.SizeOffset_Y = 30;
			statTrackerLabel.TextAlignment = TextAnchor.LowerCenter;
			statTrackerLabel.FontStyle = FontStyle.Italic;
			statTrackerLabel.FontSize = ESleekFontSize.Default;
			container.AddChild(statTrackerLabel);
			statTrackerLabel.IsVisible = false;

			IconsBundle overlayBundle = Bundles.getIconsBundle("UI/Player/Overlay");

			scopeOverlay = new SleekScopeOverlay(overlayBundle);
			scopeOverlay.SizeScale_X = 1.0f;
			scopeOverlay.SizeScale_Y = 1.0f;
			scopeOverlay.IsVisible = false;
			PlayerUI.window.AddChild(scopeOverlay);

			binocularsOverlay = Glazier.Get().CreateImage(overlayBundle.load<Texture2D>("Binoculars"));
			binocularsOverlay.SizeScale_X = 1;
			binocularsOverlay.SizeScale_Y = 1;
			PlayerUI.window.AddChild(binocularsOverlay);
			binocularsOverlay.IsVisible = false;

			faceButtons = new ISleekButton[Customization.FACES_FREE + Customization.FACES_PRO];
			for (int index = 0; index < faceButtons.Length; index++)
			{
				float angle = Mathf.PI * 4 * (index / (float) faceButtons.Length);
				float distance = 210.0f;
				if (index >= faceButtons.Length / 2)
				{
					angle += Mathf.PI / (faceButtons.Length / 2);
					distance += 30.0f;
				}

				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_X = (int) (Mathf.Cos(angle) * distance) - 20;
				button.PositionOffset_Y = (int) (Mathf.Sin(angle) * distance) - 20;
				button.PositionScale_X = 0.5f;
				button.PositionScale_Y = 0.5f;
				button.SizeOffset_X = 40;
				button.SizeOffset_Y = 40;
				container.AddChild(button);
				button.IsVisible = false;

				ISleekImage skin = Glazier.Get().CreateImage();
				skin.PositionOffset_X = 10;
				skin.PositionOffset_Y = 10;
				skin.SizeOffset_X = 20;
				skin.SizeOffset_Y = 20;
				skin.Texture = GlazierResources.PixelTexture;
				skin.TintColor = Characters.active.skin;
				button.AddChild(skin);

				ISleekImage icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = 2;
				icon.PositionOffset_Y = 2;
				icon.SizeOffset_X = 16;
				icon.SizeOffset_Y = 16;
				icon.Texture = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Faces/" + index + "/Texture.png");
				skin.AddChild(icon);

				if (index >= Customization.FACES_FREE)
				{
					if (Provider.isPro)
					{
						button.OnClicked += onClickedFaceButton;
					}
					else
					{
						button.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);

						IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

						ISleekImage pro = Glazier.Get().CreateImage();
						pro.PositionOffset_X = -10;
						pro.PositionOffset_Y = -10;
						pro.PositionScale_X = 0.5f;
						pro.PositionScale_Y = 0.5f;
						pro.SizeOffset_X = 20;
						pro.SizeOffset_Y = 20;
						pro.Texture = pros.load<Texture2D>("Lock_Small");
						button.AddChild(pro);
					}
				}
				else
				{
					button.OnClicked += onClickedFaceButton;
				}

				faceButtons[index] = button;
			}

			surrenderButton = Glazier.Get().CreateButton();
			surrenderButton.PositionOffset_X = -160;
			surrenderButton.PositionOffset_Y = -15;
			surrenderButton.PositionScale_X = 0.5f;
			surrenderButton.PositionScale_Y = 0.5f;
			surrenderButton.SizeOffset_X = 150;
			surrenderButton.SizeOffset_Y = 30;
			surrenderButton.Text = localization.format("Surrender");
			surrenderButton.OnClicked += onClickedSurrenderButton;
			container.AddChild(surrenderButton);
			surrenderButton.IsVisible = false;

			pointButton = Glazier.Get().CreateButton();
			pointButton.PositionOffset_X = 10;
			pointButton.PositionOffset_Y = -15;
			pointButton.PositionScale_X = 0.5f;
			pointButton.PositionScale_Y = 0.5f;
			pointButton.SizeOffset_X = 150;
			pointButton.SizeOffset_Y = 30;
			pointButton.Text = localization.format("Point");
			pointButton.OnClicked += onClickedPointButton;
			container.AddChild(pointButton);
			pointButton.IsVisible = false;

			waveButton = Glazier.Get().CreateButton();
			waveButton.PositionOffset_X = -75;
			waveButton.PositionOffset_Y = -55;
			waveButton.PositionScale_X = 0.5f;
			waveButton.PositionScale_Y = 0.5f;
			waveButton.SizeOffset_X = 150;
			waveButton.SizeOffset_Y = 30;
			waveButton.Text = localization.format("Wave");
			waveButton.OnClicked += onClickedWaveButton;
			container.AddChild(waveButton);
			waveButton.IsVisible = false;

			saluteButton = Glazier.Get().CreateButton();
			saluteButton.PositionOffset_X = -75;
			saluteButton.PositionOffset_Y = 25;
			saluteButton.PositionScale_X = 0.5f;
			saluteButton.PositionScale_Y = 0.5f;
			saluteButton.SizeOffset_X = 150;
			saluteButton.SizeOffset_Y = 30;
			saluteButton.Text = localization.format("Salute");
			saluteButton.OnClicked += onClickedSaluteButton;
			container.AddChild(saluteButton);
			saluteButton.IsVisible = false;

			restButton = Glazier.Get().CreateButton();
			restButton.PositionOffset_X = -160;
			restButton.PositionOffset_Y = 65;
			restButton.PositionScale_X = 0.5f;
			restButton.PositionScale_Y = 0.5f;
			restButton.SizeOffset_X = 150;
			restButton.SizeOffset_Y = 30;
			restButton.Text = localization.format("Rest");
			restButton.OnClicked += onClickedRestButton;
			container.AddChild(restButton);
			restButton.IsVisible = false;

			facepalmButton = Glazier.Get().CreateButton();
			facepalmButton.PositionOffset_X = 10;
			facepalmButton.PositionOffset_Y = -95;
			facepalmButton.PositionScale_X = 0.5f;
			facepalmButton.PositionScale_Y = 0.5f;
			facepalmButton.SizeOffset_X = 150;
			facepalmButton.SizeOffset_Y = 30;
			facepalmButton.Text = localization.format("Facepalm");
			facepalmButton.OnClicked += onClickedFacepalmButton;
			container.AddChild(facepalmButton);
			facepalmButton.IsVisible = false;

			tPoseButton = Glazier.Get().CreateButton();
			tPoseButton.PositionOffset_X = 10;
			tPoseButton.PositionOffset_Y = 65;
			tPoseButton.PositionScale_X = 0.5f;
			tPoseButton.PositionScale_Y = 0.5f;
			tPoseButton.SizeOffset_X = 150;
			tPoseButton.SizeOffset_Y = 30;
			tPoseButton.Text = localization.format("Gesture_TPose");
			tPoseButton.OnClicked += onClickedTPoseButton;
			container.AddChild(tPoseButton);
			tPoseButton.IsVisible = false;

			activeHitmarkers = new List<HitmarkerInfo>(16);
			hitmarkersPool = new List<SleekHitmarker>(16);
			for (int hitmarkerIndex = 0; hitmarkerIndex < 16; ++hitmarkerIndex)
			{
				// Prewarm the pool.
				ReleaseHitmarker(NewHitmarker());
			}

			crosshair = new Crosshair(icons);
			crosshair.SizeScale_X = 1.0f;
			crosshair.SizeScale_Y = 1.0f;
			container.AddChild(crosshair);
			crosshair.SetPluginAllowsCenterDotVisible(Player.LocalPlayer.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowCenterDot));

			lifeBox = Glazier.Get().CreateBox();
			lifeBox.PositionScale_Y = 1.0f;
			lifeBox.SizeScale_X = 0.2f;
			container.AddChild(lifeBox);

			statusIconsContainer = Glazier.Get().CreateFrame();
			statusIconsContainer.PositionOffset_Y = -60;
			statusIconsContainer.PositionScale_Y = 1.0f;
			statusIconsContainer.SizeScale_X = 0.2f;
			statusIconsContainer.SizeOffset_Y = 50;
			container.AddChild(statusIconsContainer);

			healthIcon = Glazier.Get().CreateImage();
			healthIcon.PositionOffset_X = 5;
			healthIcon.SizeOffset_X = 20;
			healthIcon.SizeOffset_Y = 20;
			healthIcon.Texture = icons.load<Texture2D>("Health");
			lifeBox.AddChild(healthIcon);

			healthProgress = new SleekProgress("");
			healthProgress.PositionOffset_X = 30;
			healthProgress.SizeOffset_X = -40;
			healthProgress.SizeOffset_Y = 10;
			healthProgress.SizeScale_X = 1;
			healthProgress.color = Palette.COLOR_R;
			lifeBox.AddChild(healthProgress);

			foodIcon = Glazier.Get().CreateImage();
			foodIcon.PositionOffset_X = 5;
			foodIcon.SizeOffset_X = 20;
			foodIcon.SizeOffset_Y = 20;
			foodIcon.Texture = icons.load<Texture2D>("Food");
			lifeBox.AddChild(foodIcon);

			foodProgress = new SleekProgress("");
			foodProgress.PositionOffset_X = 30;
			foodProgress.SizeOffset_X = -40;
			foodProgress.SizeOffset_Y = 10;
			foodProgress.SizeScale_X = 1;
			foodProgress.color = Palette.COLOR_O;
			lifeBox.AddChild(foodProgress);

			waterIcon = Glazier.Get().CreateImage();
			waterIcon.PositionOffset_X = 5;
			waterIcon.SizeOffset_X = 20;
			waterIcon.SizeOffset_Y = 20;
			waterIcon.Texture = icons.load<Texture2D>("Water");
			lifeBox.AddChild(waterIcon);

			waterProgress = new SleekProgress("");
			waterProgress.PositionOffset_X = 30;
			waterProgress.SizeOffset_X = -40;
			waterProgress.SizeOffset_Y = 10;
			waterProgress.SizeScale_X = 1;
			waterProgress.color = Palette.COLOR_B;
			lifeBox.AddChild(waterProgress);

			virusIcon = Glazier.Get().CreateImage();
			virusIcon.PositionOffset_X = 5;
			virusIcon.SizeOffset_X = 20;
			virusIcon.SizeOffset_Y = 20;
			virusIcon.Texture = icons.load<Texture2D>("Virus");
			lifeBox.AddChild(virusIcon);

			virusProgress = new SleekProgress("");
			virusProgress.PositionOffset_X = 30;
			virusProgress.SizeOffset_X = -40;
			virusProgress.SizeOffset_Y = 10;
			virusProgress.SizeScale_X = 1;
			virusProgress.color = Palette.COLOR_G;
			lifeBox.AddChild(virusProgress);

			staminaIcon = Glazier.Get().CreateImage();
			staminaIcon.PositionOffset_X = 5;
			staminaIcon.SizeOffset_X = 20;
			staminaIcon.SizeOffset_Y = 20;
			staminaIcon.Texture = icons.load<Texture2D>("Stamina");
			lifeBox.AddChild(staminaIcon);

			staminaProgress = new SleekProgress("");
			staminaProgress.PositionOffset_X = 30;
			staminaProgress.SizeOffset_X = -40;
			staminaProgress.SizeOffset_Y = 10;
			staminaProgress.SizeScale_X = 1;
			staminaProgress.color = Palette.COLOR_Y;
			lifeBox.AddChild(staminaProgress);

			waveLabel = Glazier.Get().CreateLabel();
			waveLabel.SizeOffset_Y = 30;
			waveLabel.SizeScale_X = 0.5f;
			lifeBox.AddChild(waveLabel);

			scoreLabel = Glazier.Get().CreateLabel();
			scoreLabel.PositionScale_X = 0.5f;
			scoreLabel.SizeOffset_Y = 30;
			scoreLabel.SizeScale_X = 0.5f;
			lifeBox.AddChild(scoreLabel);

			oxygenIcon = Glazier.Get().CreateImage();
			oxygenIcon.PositionOffset_X = 5;
			oxygenIcon.SizeOffset_X = 20;
			oxygenIcon.SizeOffset_Y = 20;
			oxygenIcon.Texture = icons.load<Texture2D>("Oxygen");
			lifeBox.AddChild(oxygenIcon);

			oxygenProgress = new SleekProgress("");
			oxygenProgress.PositionOffset_X = 30;
			oxygenProgress.SizeOffset_X = -40;
			oxygenProgress.SizeOffset_Y = 10;
			oxygenProgress.SizeScale_X = 1;
			oxygenProgress.color = Palette.COLOR_W;
			lifeBox.AddChild(oxygenProgress);

			vehicleBox = Glazier.Get().CreateBox();
			vehicleBox.PositionOffset_Y = -120;
			vehicleBox.PositionScale_X = 0.8f;
			vehicleBox.PositionScale_Y = 1;
			vehicleBox.SizeOffset_Y = 120;
			vehicleBox.SizeScale_X = 0.2f;
			container.AddChild(vehicleBox);
			vehicleVisibleByDefault = false;

			fuelIcon = Glazier.Get().CreateImage();
			fuelIcon.PositionOffset_X = 5;
			fuelIcon.PositionOffset_Y = 5;
			fuelIcon.SizeOffset_X = 20;
			fuelIcon.SizeOffset_Y = 20;
			fuelIcon.Texture = icons.load<Texture2D>("Fuel");
			vehicleBox.AddChild(fuelIcon);

			fuelProgress = new SleekProgress("");
			fuelProgress.PositionOffset_X = 30;
			fuelProgress.PositionOffset_Y = 10;
			fuelProgress.SizeOffset_X = -40;
			fuelProgress.SizeOffset_Y = 10;
			fuelProgress.SizeScale_X = 1;
			fuelProgress.color = Palette.COLOR_Y;
			vehicleBox.AddChild(fuelProgress);

			speedIcon = Glazier.Get().CreateImage();
			speedIcon.PositionOffset_X = 5;
			speedIcon.PositionOffset_Y = 35;
			speedIcon.SizeOffset_X = 20;
			speedIcon.SizeOffset_Y = 20;
			speedIcon.Texture = icons.load<Texture2D>("Speed");
			vehicleBox.AddChild(speedIcon);

			speedProgress = new SleekProgress(OptionsSettings.metric ? " kph" : " mph");
			speedProgress.PositionOffset_X = 30;
			speedProgress.PositionOffset_Y = 40;
			speedProgress.SizeOffset_X = -40;
			speedProgress.SizeOffset_Y = 10;
			speedProgress.SizeScale_X = 1;
			speedProgress.color = Palette.COLOR_P;
			vehicleBox.AddChild(speedProgress);

			hpIcon = Glazier.Get().CreateImage();
			hpIcon.PositionOffset_X = 5;
			hpIcon.PositionOffset_Y = 65;
			hpIcon.SizeOffset_X = 20;
			hpIcon.SizeOffset_Y = 20;
			hpIcon.Texture = icons.load<Texture2D>("Health");
			vehicleBox.AddChild(hpIcon);

			hpProgress = new SleekProgress("");
			hpProgress.PositionOffset_X = 30;
			hpProgress.PositionOffset_Y = 70;
			hpProgress.SizeOffset_X = -40;
			hpProgress.SizeOffset_Y = 10;
			hpProgress.SizeScale_X = 1;
			hpProgress.color = Palette.COLOR_R;
			vehicleBox.AddChild(hpProgress);

			batteryChargeIcon = Glazier.Get().CreateImage();
			batteryChargeIcon.PositionOffset_X = 5;
			batteryChargeIcon.PositionOffset_Y = 95;
			batteryChargeIcon.SizeOffset_X = 20;
			batteryChargeIcon.SizeOffset_Y = 20;
			batteryChargeIcon.Texture = icons.load<Texture2D>("Stamina");
			vehicleBox.AddChild(batteryChargeIcon);

			batteryChargeProgress = new SleekProgress("");
			batteryChargeProgress.PositionOffset_X = 30;
			batteryChargeProgress.PositionOffset_Y = 100;
			batteryChargeProgress.SizeOffset_X = -40;
			batteryChargeProgress.SizeOffset_Y = 10;
			batteryChargeProgress.SizeScale_X = 1;
			batteryChargeProgress.color = Palette.COLOR_Y;
			vehicleBox.AddChild(batteryChargeProgress);

			vehicleLockedLabel = Glazier.Get().CreateLabel();
			vehicleLockedLabel.PositionOffset_Y = -25;
			vehicleLockedLabel.SizeScale_X = 1;
			vehicleLockedLabel.SizeOffset_Y = 30;
			vehicleLockedLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			vehicleBox.AddChild(vehicleLockedLabel);

			vehicleEngineLabel = Glazier.Get().CreateLabel();
			vehicleEngineLabel.SizeScale_X = 1;
			vehicleEngineLabel.SizeOffset_Y = 30;
			vehicleBox.AddChild(vehicleEngineLabel);

			gasmaskBox = Glazier.Get().CreateBox();
			gasmaskBox.PositionOffset_X = -200;
			gasmaskBox.PositionOffset_Y = -60;
			gasmaskBox.PositionScale_X = 0.5f;
			gasmaskBox.PositionScale_Y = 1;
			gasmaskBox.SizeOffset_X = 400;
			gasmaskBox.SizeOffset_Y = 60;
			container.AddChild(gasmaskBox);
			gasmaskBox.IsVisible = false;

			gasmaskIcon = new SleekItemIcon();
			gasmaskIcon.PositionOffset_X = 5;
			gasmaskIcon.PositionOffset_Y = 5;
			gasmaskIcon.SizeOffset_X = 50;
			gasmaskIcon.SizeOffset_Y = 50;
			gasmaskBox.AddChild(gasmaskIcon);

			gasmaskProgress = new SleekProgress("");
			gasmaskProgress.PositionOffset_X = 60;
			gasmaskProgress.PositionOffset_Y = 10;
			gasmaskProgress.SizeOffset_X = -70;
			gasmaskProgress.SizeOffset_Y = 40;
			gasmaskProgress.SizeScale_X = 1;
			gasmaskBox.AddChild(gasmaskProgress);

			bleedingBox = new SleekBoxIcon(icons.load<Texture2D>("Bleeding"));
			bleedingBox.SizeOffset_X = 50;
			bleedingBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(bleedingBox);
			bleedingBox.IsVisible = false;

			brokenBox = new SleekBoxIcon(icons.load<Texture2D>("Broken"));
			brokenBox.SizeOffset_X = 50;
			brokenBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(brokenBox);
			brokenBox.IsVisible = false;

			temperatureBox = new SleekBoxIcon(null);
			temperatureBox.SizeOffset_X = 50;
			temperatureBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(temperatureBox);
			temperatureBox.IsVisible = false;

			starvedBox = new SleekBoxIcon(icons.load<Texture2D>("Starved"));
			starvedBox.SizeOffset_X = 50;
			starvedBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(starvedBox);
			starvedBox.IsVisible = false;

			dehydratedBox = new SleekBoxIcon(icons.load<Texture2D>("Dehydrated"));
			dehydratedBox.SizeOffset_X = 50;
			dehydratedBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(dehydratedBox);
			dehydratedBox.IsVisible = false;

			infectedBox = new SleekBoxIcon(icons.load<Texture2D>("Infected"));
			infectedBox.SizeOffset_X = 50;
			infectedBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(infectedBox);
			infectedBox.IsVisible = false;

			drownedBox = new SleekBoxIcon(icons.load<Texture2D>("Drowned"));
			drownedBox.SizeOffset_X = 50;
			drownedBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(drownedBox);
			drownedBox.IsVisible = false;

			asphyxiatingBox = new SleekBoxIcon(icons.load<Texture2D>("AsphyxiatingStatus"));
			asphyxiatingBox.SizeOffset_X = 50;
			asphyxiatingBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(asphyxiatingBox);
			asphyxiatingBox.IsVisible = false;

			moonBox = new SleekBoxIcon(icons.load<Texture2D>("Moon"));
			moonBox.SizeOffset_X = 50;
			moonBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(moonBox);
			moonBox.IsVisible = false;

			radiationBox = new SleekBoxIcon(icons.load<Texture2D>("Deadzone"));
			radiationBox.SizeOffset_X = 50;
			radiationBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(radiationBox);
			radiationBox.IsVisible = false;

			safeBox = new SleekBoxIcon(icons.load<Texture2D>("Safe"));
			safeBox.SizeOffset_X = 50;
			safeBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(safeBox);
			safeBox.IsVisible = false;

			arrestBox = new SleekBoxIcon(icons.load<Texture2D>("Arrest"));
			arrestBox.SizeOffset_X = 50;
			arrestBox.SizeOffset_Y = 50;
			statusIconsContainer.AddChild(arrestBox);
			arrestBox.IsVisible = false;

			updateIcons();
			updateLifeBoxVisibility();
			UpdateVehicleBoxVisibility();

			OptionsSettings.OnEnableOutboundVoiceChatChanged += SynchronizeOutboundVoiceChatVisible;
			OptionsSettings.OnShowOutboundVoiceChatOffHintChanged += SynchronizeOutboundVoiceChatVisible;
			OptionsSettings.OnUnitSystemChanged += OnUnitSystemChanged;

			Player.LocalPlayer.onLocalPluginWidgetFlagsChanged += OnLocalPluginWidgetFlagsChanged;
			Player.LocalPlayer.life.onDamaged += onDamaged;
			Player.LocalPlayer.life.onHealthUpdated = onHealthUpdated;
			Player.LocalPlayer.life.onFoodUpdated = onFoodUpdated;
			Player.LocalPlayer.life.onWaterUpdated = onWaterUpdated;
			Player.LocalPlayer.life.onVirusUpdated = onVirusUpdated;
			Player.LocalPlayer.life.onStaminaUpdated = onStaminaUpdated;
			Player.LocalPlayer.life.onOxygenUpdated = onOxygenUpdated;
			Player.LocalPlayer.life.OnIsAsphyxiatingChanged += OnIsAsphyxiatingChanged;
			Player.LocalPlayer.life.onBleedingUpdated = onBleedingUpdated;
			Player.LocalPlayer.life.onBrokenUpdated = onBrokenUpdated;
			Player.LocalPlayer.life.onTemperatureUpdated = onTemperatureUpdated;

			Player.LocalPlayer.look.onPerspectiveUpdated += onPerspectiveUpdated;

			Player.LocalPlayer.movement.onSeated += onSeated;
			Player.LocalPlayer.movement.onVehicleUpdated += onVehicleUpdated;
			Player.LocalPlayer.movement.onSafetyUpdated += onSafetyUpdated;
			Player.LocalPlayer.movement.onRadiationUpdated += onRadiationUpdated;

			Player.LocalPlayer.animator.onGestureUpdated += onGestureUpdated;
			Player.LocalPlayer.equipment.onHotkeysUpdated += onHotkeysUpdated;

			Player.LocalPlayer.voice.OnCustomAllowTalkingChanged += OnCustomAllowTalkingChanged;
			Player.LocalPlayer.voice.onTalkingChanged += onTalked;

			// Note: do not update tracked quest yet because conditions may depend on NPCQuestUI.
			Player.LocalPlayer.quests.TrackedQuestUpdated += OnTrackedQuestUpdated;

			Player.LocalPlayer.skills.onExperienceUpdated += onExperienceUpdated;
			LightingManager.onMoonUpdated += onMoonUpdated;
			ZombieManager.onWaveUpdated += onWaveUpdated;

			Player.LocalPlayer.clothing.onMaskUpdated += onMaskUpdated;

			OnChatMessageReceived();
			ChatManager.onChatMessageReceived += OnChatMessageReceived;
			ChatManager.onVotingStart += onVotingStart;
			ChatManager.onVotingUpdate += onVotingUpdate;
			ChatManager.onVotingStop += onVotingStop;
			ChatManager.onVotingMessage += onVotingMessage;

			LevelManager.onArenaMessageUpdated += onArenaMessageUpdated;
			LevelManager.onArenaPlayerUpdated += onArenaPlayerUpdated;
			LevelManager.onLevelNumberUpdated += onLevelNumberUpdated;
		}

		private static SleekHitmarker NewHitmarker()
		{
			SleekHitmarker hitmarker = new SleekHitmarker();
			hitmarker.PositionOffset_X = -64;
			hitmarker.PositionOffset_Y = -64;
			hitmarker.SizeOffset_X = 128;
			hitmarker.SizeOffset_Y = 128;
			PlayerUI.window.AddChild(hitmarker);
			return hitmarker;
		}

		internal static SleekHitmarker ClaimHitmarker()
		{
			if (hitmarkersPool.Count > 0)
			{
				return hitmarkersPool.GetAndRemoveTail();
			}
			else
			{
				return NewHitmarker();
			}
		}

		internal static void ReleaseHitmarker(SleekHitmarker hitmarker)
		{
			hitmarker.IsVisible = false;
			hitmarkersPool.Add(hitmarker);
		}

		internal static List<HitmarkerInfo> activeHitmarkers;
		private static List<SleekHitmarker> hitmarkersPool;
		private static List<bool> areConditionsMet = new List<bool>(8);
	}
}
