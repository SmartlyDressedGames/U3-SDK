////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.Unturned
{
	public class MenuUI : MonoBehaviour
	{
		public static SleekWindow window;
		public static SleekFullscreenBox container;

		private static ISleekBox alertBox;
		private static ISleekLabel originLabel;
		private static ISleekButton dismissNotificationButton;
		internal static SleekButtonIcon copyNotificationButton;
		internal static SleekButtonIcon markContentCorruptButton;
		private static SleekInventory[] itemAlerts;
		private static bool isAlerting;

		private Transform titleCameraTransform;
		private Transform playCameraTransform;
		private Transform survivorsCameraTransform;
		private Transform configurationCameraTransform;
		private Transform workshopCameraTransform;
		private Transform targetCameraTransform;

		private static bool hasHandledCommandLineConnectionRequests;
		private static bool hasReachedTitleCameraTransform;

		/// <summary>
		/// Remove any existing item alert widgets.
		/// </summary>
		private static void removeItemAlerts()
		{
			if (itemAlerts == null)
				return;

			foreach (SleekInventory alert in itemAlerts)
			{
				alertBox.RemoveChild(alert);
			}

			itemAlerts = null;
		}

		private static void alertText()
		{
			if (alertBox == null || originLabel == null)
			{
				return;
			}

			alertBox.PositionOffset_Y = -50;
			alertBox.SizeOffset_Y = 100;
			copyNotificationButton.IsVisible = true;
			markContentCorruptButton.IsVisible = false;

			originLabel.IsVisible = false;
			removeItemAlerts();
		}

		private static void alertItem()
		{
			if (alertBox == null || originLabel == null)
			{
				return;
			}

			alertBox.Text = "";
			alertBox.PositionOffset_Y = -150;
			alertBox.SizeOffset_Y = 300;
			copyNotificationButton.IsVisible = false;
			markContentCorruptButton.IsVisible = false;

			originLabel.IsVisible = true;
		}

		private static void internalOpenAlert()
		{
			if (alertBox != null)
			{
				alertBox.AnimatePositionScale(0, 0.5f, ESleekLerp.EXPONENTIAL, 20);
			}

			if (container != null)
			{
				container.AnimateOutOfView(-1, 0);
			}
		}

		private static void updateDismissButton(bool canBeDismissed)
		{
			if (dismissNotificationButton != null)
			{
				// Messy, but this is how escapeMenu handles it at the moment too...
				if (Provider.provider.matchmakingService.isAttemptingServerQuery)
				{
					dismissNotificationButton.Text = MenuPlayConnectUI.localization.format("Cancel_Attempt_Label");
					dismissNotificationButton.TooltipText = MenuPlayConnectUI.localization.format("Cancel_Attempt_Tooltip");
				}
				else
				{
					dismissNotificationButton.Text = MenuDashboardUI.localization.format("Dismiss_Notification_Label");
					dismissNotificationButton.TooltipText = MenuDashboardUI.localization.format("Dismiss_Notification_Tooltip");
				}
				dismissNotificationButton.IsVisible = canBeDismissed;
			}
		}

		public static void openAlert(string message, bool canBeDismissed = true)
		{
			alertText();

			if (alertBox != null)
			{
				alertBox.Text = message;
			}

			updateDismissButton(canBeDismissed);
			internalOpenAlert();
		}

		public static void closeAlert()
		{
			removeItemAlerts(); // Otherwise large number of purchased items overflow onto screen.

			if (alertBox != null)
			{
				alertBox.AnimatePositionScale(1, 0.5f, ESleekLerp.EXPONENTIAL, 20);
			}

			if (container != null)
			{
				container.AnimateIntoView();
			}
		}

		public static void alert(string message)
		{
			openAlert(message);
			isAlerting = true;
		}

		public static void alert(string origin, ulong instanceId, int itemDefId, ushort quantity)
		{
			SteamItemDetails_t details = new SteamItemDetails_t();
			details.m_itemId.m_SteamItemInstanceID = instanceId;
			details.m_iDefinition.m_SteamItemDef = itemDefId;
			details.m_unQuantity = quantity;
			alertNewItems(origin, new List<SteamItemDetails_t> { details });
		}

		/// <summary>
		/// Open fullscreen alert showcasing newly granted items.
		/// Uses first item for title color, so items should be sorted by priority.
		/// </summary>
		public static void alertNewItems(string origin, List<SteamItemDetails_t> grantedItems)
		{
			if (originLabel != null)
			{
				originLabel.Text = origin;
				originLabel.TextColor = Provider.provider.economyService.getInventoryColor(grantedItems[0].m_iDefinition.m_SteamItemDef);
			}

			removeItemAlerts();

			itemAlerts = new SleekInventory[grantedItems.Count];
			int horizontalOffset = -100;
			for (int index = 0; index < grantedItems.Count; ++index)
			{
				SteamItemDetails_t item = grantedItems[index];
				bool isFullSize = index == 0; // Primary item is centered and full size.
				int alertSize = isFullSize ? 200 : 100;

				SleekInventory alert = new SleekInventory();
				alert.PositionOffset_X = horizontalOffset;
				alert.PositionOffset_Y = isFullSize ? 75 : 125;
				alert.PositionScale_X = 0.5f;
				alert.SizeOffset_X = alertSize;
				alert.SizeOffset_Y = alertSize;
				alertBox.AddChild(alert);

				alert.updateInventory(item.m_itemId.m_SteamItemInstanceID, item.m_iDefinition.m_SteamItemDef, item.m_unQuantity, false, isFullSize);
				itemAlerts[index] = alert;

				horizontalOffset += alertSize + 5;
			}

			alertItem();
			updateDismissButton(true);
			internalOpenAlert();

			isAlerting = true;
		}

		/// <summary>
		/// Open fullscreen alert showcasing newly granted items.
		/// </summary>
		public static void alertPurchasedItems(string origin, List<SteamItemDetails_t> grantedItems)
		{
			if (originLabel != null)
			{
				originLabel.Text = origin;
				originLabel.TextColor = ItemStore.PremiumColor;
			}

			removeItemAlerts();

			itemAlerts = new SleekInventory[grantedItems.Count];
			int horizontalOffset = grantedItems.Count * -100;
			for (int index = 0; index < grantedItems.Count; ++index)
			{
				SteamItemDetails_t item = grantedItems[index];

				SleekInventory alert = new SleekInventory();
				alert.PositionOffset_X = horizontalOffset;
				alert.PositionOffset_Y = 75;
				alert.PositionScale_X = 0.5f;
				alert.SizeOffset_X = 200;
				alert.SizeOffset_Y = 200;
				alertBox.AddChild(alert);

				alert.updateInventory(item.m_itemId.m_SteamItemInstanceID, item.m_iDefinition.m_SteamItemDef, item.m_unQuantity, false, true);
				itemAlerts[index] = alert;

				horizontalOffset += 200;
			}

			alertItem();
			updateDismissButton(true);
			internalOpenAlert();

			isAlerting = true;
		}

		private static void onClickedDismissNotification(ISleekElement button)
		{
			// Some of this logic is based on escapeMenu, ideally tidied up eventually.
			if (Provider.provider.matchmakingService.isAttemptingServerQuery)
			{
				Provider.provider.matchmakingService.cancel();

				// Also dismiss the "failed to find server" notification:
				closeAlert();
				isAlerting = false;
			}
			else if (MenuSurvivorsClothingUI.isCrafting)
			{
				// Cannot cancel while waiting for exchange result.
				return;
			}
			else
			{
				closeAlert();
				isAlerting = false;
			}
		}

		private static void OnClickedCopyNotification(ISleekElement button)
		{
			GUIUtility.systemCopyBuffer = alertBox.Text;
		}

		private static void OnClickedMarkContentCorrupt(ISleekElement button)
		{
			UnturnedLog.info("Marketing Steam content corrupt (will verify/validate game files)");
			SteamApps.MarkContentCorrupt(/*bMissingFilesOnly*/ false);
			Provider.QuitGame("clicked mark content corrupt button");
		}

		public static void closeAll()
		{
			MenuPauseUI.close();
			MenuCreditsUI.close();
			MenuTitleUI.close();
			MenuDashboardUI.close();

			MenuPlayUI.close();
			MenuPlaySingleplayerUI.close();
			MenuPlayLobbiesUI.close();
			MenuPlayConnectUI.close();
			MenuPlayServersUI.serverListFiltersUI.close();
			MenuPlayServersUI.serverCurationUI.close();
			MenuPlayServersUI.serverCurationUI.rulesUI.close();
			MenuPlayServersUI.mapFiltersUI.close();
			MenuPlayUI.serverListUI.close();
			MenuPlayUI.serverBookmarksUI.close();
			MenuPlayUI.onlineSafetyUI.close();
			MenuPlayServerInfoUI.close();
			MenuServerPasswordUI.close();
			MenuPlayConfigUI.close();

			MenuSurvivorsUI.close();
			ItemStoreBundleContentsMenu.instance.Close();
			ItemStoreDetailsMenu.instance.Close();
			ItemStoreCartMenu.instance.Close();
			ItemStoreMenu.instance.Close();
			MenuSurvivorsCharacterUI.close();
			MenuSurvivorsAppearanceUI.close();
			MenuSurvivorsClothingUI.close();
			MenuSurvivorsGroupUI.close();
			MenuSurvivorsClothingBoxUI.close();
			MenuSurvivorsClothingDeleteUI.close();
			MenuSurvivorsClothingInspectUI.close();
			MenuSurvivorsClothingItemUI.close();

			MenuConfigurationUI.close();
			MenuConfigurationOptionsUI.close();
			MenuConfigurationDisplayUI.close();
			MenuConfigurationGraphicsUI.close();
			MenuConfigurationControlsUI.close();
			MenuConfigurationUI.audioMenu.close();

			MenuWorkshopUI.close();
			MenuWorkshopEditorUI.close();
			MenuWorkshopSubmitUI.close();
		}

		internal static MenuUI instance;

		private void OnEnable()
		{
			instance = this;
			useGUILayout = false;
		}

		internal void Menu_OnGUI()
		{
			if (window != null)
			{
				Glazier.Get().Root = window;
			}
		}

		private void OnGUI()
		{
			MenuConfigurationControlsUI.bindOnGUI();
		}

		/// <summary>
		/// Handle esc/back key press.
		/// Still really messy, but this used to be inside a huge nested if/elseif in Update.
		/// </summary>
		private void escapeMenu()
		{
			if (Provider.provider.matchmakingService.isAttemptingServerQuery)
			{
				Provider.provider.matchmakingService.cancel();

				// Also dismiss the "failed to find server" notification:
				closeAlert();
				isAlerting = false;
				return;
			}

			if (MenuSurvivorsClothingUI.isCrafting)
			{
				// Cannot cancel while waiting for exchange result.
				return;
			}

			if (isAlerting)
			{
				closeAlert();
				isAlerting = false;
				return;
			}

			if (MenuPauseUI.active)
			{
				MenuPauseUI.close();
				MenuDashboardUI.open();
				MenuTitleUI.open();
				return;
			}

			if (MenuCreditsUI.active)
			{
				MenuCreditsUI.close();
				MenuPauseUI.open();
				return;
			}

			if (MenuTitleUI.active)
			{
				MenuPauseUI.open();
				MenuDashboardUI.close();
				MenuTitleUI.close();
				return;
			}

			if (MenuPlayConfigUI.active)
			{
				MenuPlayConfigUI.close();
				MenuPlaySingleplayerUI.open();
				return;
			}

			if (MenuServerPasswordUI.isActive)
			{
				MenuServerPasswordUI.close();
				MenuPlayServerInfoUI.OpenWithoutRefresh();
				return;
			}

			if (MenuPlayServerInfoUI.active)
			{
				MenuPlayServerInfoUI.close();

				switch (MenuPlayServerInfoUI.openContext)
				{
					case MenuPlayServerInfoUI.EServerInfoOpenContext.CONNECT:
						MenuPlayConnectUI.open();
						break;

					case MenuPlayServerInfoUI.EServerInfoOpenContext.SERVERS:
						MenuPlayUI.serverListUI.open(false);
						break;

					case MenuPlayServerInfoUI.EServerInfoOpenContext.BOOKMARKS:
						MenuPlayUI.serverBookmarksUI.open();
						break;

					default:
						UnturnedLog.info("Unknown server info open context: {0}", MenuPlayServerInfoUI.openContext);
						break;
				}

				return;
			}

			if (MenuPlayServersUI.mapFiltersUI.active)
			{
				MenuPlayServersUI.mapFiltersUI.close();
				MenuPlayServersUI.mapFiltersUI.OpenPreviousMenu();
				return;
			}

			if (MenuPlayServersUI.serverListFiltersUI.active)
			{
				MenuPlayServersUI.serverListFiltersUI.close();
				MenuPlayUI.serverListUI.open(true);
				return;
			}

			if (MenuPlayServersUI.serverCurationUI.active)
			{
				MenuPlayServersUI.serverCurationUI.close();
				MenuPlayUI.serverListUI.open(true);
				return;
			}

			if (MenuPlayServersUI.serverCurationUI.rulesUI.active)
			{
				MenuPlayServersUI.serverCurationUI.rulesUI.close();
				MenuPlayServersUI.serverCurationUI.open();
				return;
			}

			if (MenuPlayConnectUI.active || MenuPlayUI.serverListUI.active || MenuPlaySingleplayerUI.active || MenuPlayLobbiesUI.active || MenuPlayUI.serverBookmarksUI.active || MenuPlayUI.onlineSafetyUI.active)
			{
				MenuPlayConnectUI.close();
				MenuPlayUI.serverListUI.close();
				MenuPlaySingleplayerUI.close();
				MenuPlayLobbiesUI.close();
				MenuPlayUI.serverBookmarksUI.close();
				MenuPlayUI.onlineSafetyUI.close();

				MenuPlayUI.open();
				return;
			}

			if (ItemStoreCartMenu.instance.IsOpen)
			{
				ItemStoreCartMenu.instance.Close();
				ItemStoreMenu.instance.Open();
				return;
			}

			if (ItemStoreBundleContentsMenu.instance.IsOpen)
			{
				ItemStoreBundleContentsMenu.instance.Close();
				ItemStoreDetailsMenu.instance.OpenCurrentListing();
				return;
			}

			if (ItemStoreDetailsMenu.instance.IsOpen)
			{
				ItemStoreDetailsMenu.instance.Close();
				ItemStoreMenu.instance.Open();
				return;
			}

			if (ItemStoreMenu.instance.IsOpen)
			{
				ItemStoreMenu.instance.Close();
				MenuSurvivorsClothingUI.open();
				return;
			}

			if (MenuSurvivorsClothingItemUI.active)
			{
				MenuSurvivorsClothingItemUI.close();
				MenuSurvivorsClothingUI.open();
				return;
			}

			if (MenuSurvivorsClothingBoxUI.active)
			{
				if (MenuSurvivorsClothingBoxUI.isUnboxing)
				{
					MenuSurvivorsClothingBoxUI.skipAnimation();
				}
				else
				{
					MenuSurvivorsClothingBoxUI.close();
					MenuSurvivorsClothingItemUI.open();
				}
				return;
			}

			if (MenuSurvivorsClothingInspectUI.active)
			{
				MenuSurvivorsClothingInspectUI.close();
				MenuSurvivorsClothingInspectUI.OpenPreviousMenu();
				return;
			}

			if (MenuSurvivorsClothingDeleteUI.active)
			{
				MenuSurvivorsClothingDeleteUI.close();
				MenuSurvivorsClothingItemUI.open();
				return;
			}

			if (MenuSurvivorsCharacterUI.active || MenuSurvivorsAppearanceUI.active || MenuSurvivorsGroupUI.active || MenuSurvivorsClothingUI.active)
			{
				MenuSurvivorsCharacterUI.close();
				MenuSurvivorsAppearanceUI.close();
				MenuSurvivorsGroupUI.close();
				MenuSurvivorsClothingUI.close();

				MenuSurvivorsUI.open();
				return;
			}

			if (MenuConfigurationOptionsUI.active || MenuConfigurationControlsUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationUI.audioMenu.active)
			{
				MenuConfigurationOptionsUI.close();
				MenuConfigurationControlsUI.close();
				MenuConfigurationGraphicsUI.close();
				MenuConfigurationDisplayUI.close();
				MenuConfigurationUI.audioMenu.close();

				MenuConfigurationUI.open();
				return;
			}

			if (MenuWorkshopSubmitUI.active || MenuWorkshopEditorUI.active || MenuWorkshopErrorUI.active || MenuWorkshopLocalizationUI.active || MenuWorkshopSpawnsUI.active || MenuWorkshopSubscriptionsUI.active)
			{
				MenuWorkshopSubmitUI.close();
				MenuWorkshopEditorUI.close();
				MenuWorkshopErrorUI.close();
				MenuWorkshopLocalizationUI.close();
				MenuWorkshopSpawnsUI.close();
				MenuWorkshopSubscriptionsUI.instance.close();

				MenuWorkshopUI.open();
				return;
			}

			MenuPlayUI.close();
			MenuSurvivorsUI.close();
			MenuConfigurationUI.close();
			MenuWorkshopUI.close();

			MenuDashboardUI.open();
			MenuTitleUI.open();
		}

		private void tickInput()
		{
			if (InputEx.GetKeyDown(KeyCode.F1))
			{
				MenuWorkshopUI.toggleIconTools();
			}

			if (InputEx.ConsumeKeyDown(KeyCode.Escape))
			{
				escapeMenu();
			}

			if (window != null)
			{
				if (InputEx.GetKeyDown(ControlsSettings.screenshot))
				{
					Provider.RequestScreenshot();
				}

				if (InputEx.GetKeyDown(ControlsSettings.hud))
				{
					window.isEnabled = !window.isEnabled;
					window.drawCursorWhileDisabled = false;
				}

				if (InputEx.GetKeyDown(ControlsSettings.terminal))
				{
					// debug menu?
				}
			}

			if (InputEx.GetKeyDown(ControlsSettings.refreshAssets))
			{
				Assets.RequestReloadAllAssets();
			}

			if (InputEx.GetKeyDown(ControlsSettings.clipboardDebug))
			{
				if (MenuSurvivorsAppearanceUI.active)
				{
					string export = string.Empty;

					export += "Face " + Characters.active.face;
					export += "\nHair " + Characters.active.hair;
					export += "\nBeard " + Characters.active.beard;

					export += "\nColor_Skin " + Palette.hex(Characters.active.skin);
					export += "\nColor_Hair " + Palette.hex(Characters.active.color);

					if (Characters.active.hand)
					{
						export += "\nBackward";
					}

					GUIUtility.systemCopyBuffer = export;
				}
				else if (MenuPlayServerInfoUI.active)
				{
					GUIUtility.systemCopyBuffer = MenuPlayServerInfoUI.GetClipboardData();
				}
			}
		}

		private void Update()
		{
			if (window == null)
			{
				return;
			}

			MenuConfigurationControlsUI.bindUpdate();
			MenuSurvivorsClothingBoxUI.update();
			tickInput();

			window.showCursor = true;

			if (MenuPlayUI.active || MenuPlayConnectUI.active || MenuPlayUI.serverListUI.active || MenuPlayServersUI.serverListFiltersUI.active || MenuPlayServersUI.serverCurationUI.active || MenuPlayServersUI.serverCurationUI.rulesUI.active || MenuPlayServersUI.mapFiltersUI.active || MenuPlayServerInfoUI.active || MenuServerPasswordUI.isActive || MenuPlaySingleplayerUI.active || MenuPlayLobbiesUI.active || MenuPlayConfigUI.active || MenuPlayUI.serverBookmarksUI.active || MenuPlayUI.onlineSafetyUI.active)
			{
				targetCameraTransform = playCameraTransform;
			}
			else if (MenuSurvivorsUI.active || MenuSurvivorsCharacterUI.active || MenuSurvivorsAppearanceUI.active
				|| MenuSurvivorsGroupUI.active || MenuSurvivorsClothingUI.active || MenuSurvivorsClothingItemUI.active
				|| MenuSurvivorsClothingInspectUI.active || MenuSurvivorsClothingDeleteUI.active
				|| MenuSurvivorsClothingBoxUI.active || ItemStoreMenu.instance.IsOpen
				|| ItemStoreCartMenu.instance.IsOpen || ItemStoreDetailsMenu.instance.IsOpen
				|| ItemStoreBundleContentsMenu.instance.IsOpen)
			{
				targetCameraTransform = survivorsCameraTransform;
			}
			else if (MenuConfigurationUI.active || MenuConfigurationOptionsUI.active || MenuConfigurationControlsUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationUI.audioMenu.active)
			{
				targetCameraTransform = configurationCameraTransform;
			}
			else if (MenuWorkshopUI.active || MenuWorkshopSubmitUI.active || MenuWorkshopEditorUI.active || MenuWorkshopErrorUI.active || MenuWorkshopLocalizationUI.active || MenuWorkshopSpawnsUI.active || MenuWorkshopSubscriptionsUI.active)
			{
				targetCameraTransform = workshopCameraTransform;
			}
			else
			{
				targetCameraTransform = titleCameraTransform;
			}

			if (targetCameraTransform == titleCameraTransform)
			{
				if (hasReachedTitleCameraTransform)
				{
					transform.position = Vector3.Lerp(transform.position, targetCameraTransform.position, Time.deltaTime * 4);
					transform.rotation = Quaternion.Lerp(transform.rotation, targetCameraTransform.rotation, Time.deltaTime * 4);
				}
				else
				{
					transform.position = Vector3.Lerp(transform.position, targetCameraTransform.position, Time.deltaTime);
					transform.rotation = Quaternion.Lerp(transform.rotation, targetCameraTransform.rotation, Time.deltaTime);
				}
			}
			else
			{
				hasReachedTitleCameraTransform = true;

				transform.position = Vector3.Lerp(transform.position, targetCameraTransform.position, Time.deltaTime * 4);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetCameraTransform.rotation, Time.deltaTime * 4);
			}
		}

		/// <summary>
		/// Despite being newer code, this is obviously not ideal. Previously the news request was using the Steam HTTP
		/// API which might have been the cause of some crashes, so it was quickly converted to Unity web request instead.
		/// </summary>
		internal System.Collections.IEnumerator requestSteamNews()
		{
			int announcementsCount = Provider.statusData.News.Announcements_Count;
			if (announcementsCount < 1)
			{
				UnturnedLog.warn("Not requesting Steam community announcements because count is zero");
				yield break;
			}
			else if (announcementsCount > 10)
			{
				announcementsCount = 10;
				UnturnedLog.warn("Clamping Steam community announcements to {0}", announcementsCount);
			}

			if (!Provider.allowWebRequests)
			{
				UnturnedLog.warn("Not requesting Steam community announcements because web requests are disabled");
				yield break;
			}

			string uri = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v0002?appid=304930&count={0}&feeds=steam_community_announcements";
			uri = string.Format(uri, announcementsCount.ToString("D"));
			using (UnityWebRequest request = UnityWebRequest.Get(uri))
			{
				request.timeout = 15;
				UnturnedLog.info("Requesting {0} Steam community announcements", announcementsCount);

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					UnturnedLog.warn("Error requesting news: {0}", request.error);
				}
				else
				{
					try
					{
						UnturnedLog.info("Received Steam community announcements");
						MenuDashboardUI.receiveSteamNews(request.downloadHandler.text);
					}
					catch (System.Exception exception)
					{
						UnturnedLog.exception(exception, "News web query handled improperly!");
					}
				}
			}
		}

		internal System.Collections.IEnumerator CheckForUpdates(System.Action<string, bool> callback)
		{
			if (Application.isEditor)
				yield break;

			if (!Provider.allowWebRequests)
			{
				UnturnedLog.warn("Not checking for updates because web requests are disabled");
				yield break;
			}

			string betaName;
			if (!SteamApps.GetCurrentBetaName(out betaName, 64) || string.IsNullOrWhiteSpace(betaName))
			{
				UnturnedLog.warn("Unable to get current Steam beta name, defaulting to \"public\"");
				betaName = "public";
			}

			UnturnedLog.info($"Checking for updates on Steam beta branch \"{betaName}\"...");
			string url = $"https://smartlydressedgames.com/unturned-steam-versions/{betaName}.txt";

			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				request.timeout = 30;
				yield return request.SendWebRequest();

				if (request.result == UnityWebRequest.Result.Success)
				{
					string versionString = request.downloadHandler.text;
					uint packedVersion;
					if (Parser.TryGetUInt32FromIP(versionString, out packedVersion))
					{
						if (packedVersion != Provider.APP_VERSION_PACKED)
						{
							if (packedVersion > Provider.APP_VERSION_PACKED)
							{
								UnturnedLog.info($"Detected newer game version: {versionString}");
							}
							else
							{
								UnturnedLog.info($"Detected rollback to older game version: {versionString}");
							}

							bool isRollback = packedVersion < Provider.APP_VERSION_PACKED;
							callback(versionString, isRollback);
						}
						else
						{
							UnturnedLog.info("Game version is up-to-date");
						}
					}
					else
					{
						UnturnedLog.info($"Unable to parse newest game version \"{versionString}\"");
					}
				}
				else
				{
					UnturnedLog.warn($"Network error checking for updates: \"{request.error}\"");
				}
			}
		}

		internal System.Collections.IEnumerator HandleCommandLineConnectionRequests()
		{
			// Wait until frame after menu finishes loading before starting connection process.
			yield return null;

			uint connectIP;
			ushort connectPort;
			string connectPassword;
			CSteamID connectServerCode;

			if (CommandLine.TryGetSteamConnect(CommandLine.Get(), out connectIP, out connectPort, out connectPassword, out connectServerCode))
			{
				if (connectServerCode.IsValid())
				{
					if (connectServerCode.BGameServerAccount())
					{
						ServerConnectParameters connectParameters = new ServerConnectParameters(connectServerCode, connectPassword);
						Provider.connect(connectParameters, null, null);
						UnturnedLog.info($"Command-line connect server code: {connectServerCode} Password: \"{connectPassword}\"");
					}
					else
					{
						UnturnedLog.warn($"Unable to join +connect non-gameserver code ({connectServerCode.GetEAccountType()})");
					}
				}
				else
				{
					SteamConnectionInfo info = new SteamConnectionInfo(connectIP, connectPort, connectPassword);
					UnturnedLog.info("Command-line connect IP: {0} Port: {1} Password: '{2}'", Parser.getIPFromUInt32(info.ip), info.port, info.password);

					MenuPlayConnectUI.connect(info, false, MenuPlayServerInfoUI.EServerInfoOpenContext.CONNECT);
				}
			}
			else // Prioritize connect over lobby
			{
				ulong lobby;

				if (CommandLine.tryGetLobby(CommandLine.Get(), out lobby))
				{
					UnturnedLog.info("Lobby: " + lobby);
					Lobbies.joinLobby(new CSteamID(lobby));
				}
			}
		}

		internal System.Collections.IEnumerator HandlePendingServerRelayRequest()
		{
			// Wait until frame after menu finishes loading before starting connection process.
			yield return null;

			MenuPlayConnectUI.HandlePendingServerRelayRequest();
		}

		internal void customStart()
		{
			// Reset pause in case player exited through the singleplayer escape menu.
			Time.timeScale = 1f;
			AudioListener.pause = false;

			if (!Dedicator.IsDedicatedServer)
			{
				titleCameraTransform = transform.parent.Find("Title");
				playCameraTransform = transform.parent.Find("Play");
				survivorsCameraTransform = transform.parent.Find("Survivors");
				configurationCameraTransform = transform.parent.Find("Configuration");
				workshopCameraTransform = transform.parent.Find("Workshop");

				window = new SleekWindow();

				container = new SleekFullscreenBox();
				container.SizeScale_X = 1;
				container.SizeScale_Y = 1;
				window.AddChild(container);

				alertBox = Glazier.Get().CreateBox();
				alertBox.PositionOffset_X = 10;
				alertBox.PositionOffset_Y = -25;
				alertBox.PositionScale_X = 1;
				alertBox.PositionScale_Y = 0.5f;
				alertBox.SizeScale_X = 1;
				alertBox.SizeOffset_X = -20;
				alertBox.SizeOffset_Y = 50;
				alertBox.FontSize = ESleekFontSize.Medium;
				window.AddChild(alertBox);

				originLabel = Glazier.Get().CreateLabel();
				originLabel.SizeOffset_Y = 50;
				originLabel.SizeScale_X = 1;
				originLabel.FontSize = ESleekFontSize.Large;
				alertBox.AddChild(originLabel);
				originLabel.IsVisible = false;

				dismissNotificationButton = Glazier.Get().CreateButton();
				dismissNotificationButton.PositionOffset_X = -100;
				dismissNotificationButton.PositionOffset_Y = 10;
				dismissNotificationButton.PositionScale_X = 0.5f;
				dismissNotificationButton.PositionScale_Y = 1.0f;
				dismissNotificationButton.SizeOffset_X = 200;
				dismissNotificationButton.SizeOffset_Y = 30;
				dismissNotificationButton.OnClicked += onClickedDismissNotification;
				dismissNotificationButton.FontSize = ESleekFontSize.Medium;
				alertBox.AddChild(dismissNotificationButton);

				copyNotificationButton = new SleekButtonIcon(null, 20);
				copyNotificationButton.PositionOffset_X = -100;
				copyNotificationButton.PositionOffset_Y = 50;
				copyNotificationButton.PositionScale_X = 0.5f;
				copyNotificationButton.PositionScale_Y = 1.0f;
				copyNotificationButton.SizeOffset_X = 200;
				copyNotificationButton.SizeOffset_Y = 30;
				copyNotificationButton.onClickedButton += OnClickedCopyNotification;
				copyNotificationButton.fontSize = ESleekFontSize.Medium;
				alertBox.AddChild(copyNotificationButton);

				markContentCorruptButton = new SleekButtonIcon(null, 40);
				markContentCorruptButton.PositionOffset_X = -200;
				markContentCorruptButton.PositionOffset_Y = 90;
				markContentCorruptButton.PositionScale_X = 0.5f;
				markContentCorruptButton.PositionScale_Y = 1.0f;
				markContentCorruptButton.SizeOffset_X = 400;
				markContentCorruptButton.SizeOffset_Y = 50;
				markContentCorruptButton.onClickedButton += OnClickedMarkContentCorrupt;
				markContentCorruptButton.fontSize = ESleekFontSize.Medium;
				alertBox.AddChild(markContentCorruptButton);

				itemAlerts = null;

				OptionsSettings.apply();
				GraphicsSettings.apply("loaded menu");

				dashboard = new MenuDashboardUI();

				if (hasReachedTitleCameraTransform && titleCameraTransform != null)
				{
					transform.position = titleCameraTransform.position;
					transform.rotation = titleCameraTransform.rotation;
				}

				MenuOverridableObjects.OnMenuOverridesApplied += OnMenuOverridesApplied;

				if (!hasHandledCommandLineConnectionRequests)
				{
					hasHandledCommandLineConnectionRequests = true;
					StartCoroutine(HandleCommandLineConnectionRequests());
				}
				else if (MenuPlayConnectUI.hasPendingServerRelay)
				{
					StartCoroutine(HandlePendingServerRelayRequest());
				}

				UnturnedLog.info("Menu UI ready");
			}
		}

		private void OnMenuOverridesApplied(MenuOverridableObjects source)
		{
			Vector3 newPosition;
			Quaternion newRotation;
			if (hasReachedTitleCameraTransform)
			{
				// Assuming additive level is loaded within a few ms
				titleCameraTransform.GetPositionAndRotation(out newPosition, out newRotation);
			}
			else
			{
				source.initialCamera.transform.GetPositionAndRotation(out newPosition, out newRotation);
			}
			transform.SetPositionAndRotation(newPosition, newRotation);
		}

		private void OnDestroy()
		{
			if (window == null)
			{
				return;
			}

			MenuOverridableObjects.OnMenuOverridesApplied -= OnMenuOverridesApplied;

			if (dashboard != null)
			{
				dashboard.OnDestroy();
			}

			if (!Provider.isApplicationQuitting) // Cleanup during shutdown is a waste of time.
			{
				window.InternalDestroy();
			}
			window = null;
		}

		private MenuDashboardUI dashboard;
	}
}
