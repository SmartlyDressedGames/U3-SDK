////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Net;
using UnityEngine;
using Unturned.SystemEx;
using UnityEngine.Networking;

namespace SDG.Unturned
{
	public class MenuPlayConnectUI
	{
		public static IconsBundle icons;
		public static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		/// <summary>
		/// These server relay variables redirect the client to another server when the menu opens
		/// similar to how Steam sets the +connect string on game startup. Allows plugin to redirect
		/// player to another server on the same network.
		/// </summary>
		public static bool hasPendingServerRelay = false;
		public static uint serverRelayIP;
		public static ushort serverRelayPort;
		public static CSteamID serverRelayServerCode;
		public static string serverRelayPassword;
		public static bool serverRelayWaitOnMenu;

		private static SleekButtonIcon backButton;

		private static ISleekField hostField;
		private static ISleekUInt16Field portField;
		private static ISleekField passwordField;
		private static SleekButtonIcon connectButton;
		private static ISleekBox addressInfoBox;
		private static ISleekBox serverCodeInfoBox;
		private static ISleekImage serverCodeIcon;


		/// <param name="shouldAutoJoin">If true the server is immediately joined, otherwise show server details beforehand.</param>
		public static void connect(SteamConnectionInfo info, bool shouldAutoJoin, MenuPlayServerInfoUI.EServerInfoOpenContext openContext)
		{
			Provider.provider.matchmakingService.connect(info);
			Provider.provider.matchmakingService.autoJoinServerQuery = shouldAutoJoin;
			Provider.provider.matchmakingService.serverQueryContext = openContext;
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

		internal static bool TryParseHostString(string input, out IPv4Address address, out CSteamID steamId, out ushort queryPortOverride)
		{
			address = default;
			steamId = default;
			queryPortOverride = 0;

			if (string.IsNullOrEmpty(input))
			{
				UnturnedLog.info("Unable to parse empty host string");
				return false;
			}

			input = input.Trim();
			if (string.IsNullOrEmpty(input))
			{
				UnturnedLog.info("Unable to parse host string empty after trimming");
				return false;
			}

			// If input looks like a URL then we perform a GET request. e.g., to find the most recent Fake IP address
			// and port, or to allow hosts to automatically distribute players between game servers.
			//
			// Nelson 2024-06-24: If input contains a forward slash then it's almost definitely intended as a URL
			// because DNS names, individual IP addresses, and Steam IDs cannot contain them. In this case where we
			// think it's a URL it's OK to auto-prefix with https:// because hosts reported their players were
			// confused by putting links starting with http or https:// in the address field. For example, this input
			// would be interpreted as a URL now: api.example.com/server1
			bool autoPrefix = input.Contains('/');
			// Nelson 2024-08-12: This URL *doesn't* check filters because the player isn't opening this link.
			const bool mustPassUrlFilters = false;
			if (WebUtils.ParseThirdPartyUrl(input, out string url, autoPrefix: autoPrefix, useLinkFiltering: mustPassUrlFilters))
			{
				if (!Provider.allowWebRequests)
				{
					UnturnedLog.warn("Unable to request host details because web requests are disabled");
					return false;
				}

				UnturnedLog.info($"Requesting host details from {url}...");
				using (UnityWebRequest request = UnityWebRequest.Get(url))
				{
					request.timeout = 2; // Short timeout because I was lazy and didn't put this in a coroutine yet.
					request.SendWebRequest();
					while (!request.isDone)
					{ }

					if (request.result == UnityWebRequest.Result.Success)
					{
						string responseString = request.downloadHandler.text.Trim();
						if (string.IsNullOrEmpty(responseString))
						{
							UnturnedLog.info("Unable to parse empty host details response");
							return false;
						}

						int portDelimiterIndex = responseString.IndexOf(':');
						if (portDelimiterIndex < 0)
						{
							input = responseString;
						}
						else
						{
							input = responseString.Substring(0, portDelimiterIndex);
							string portSubstring = responseString.Substring(portDelimiterIndex + 1);
							ushort.TryParse(portSubstring, out queryPortOverride);
						}
						UnturnedLog.info($"Received host details ({input}:{queryPortOverride}) from {url}");
					}
					else
					{
						UnturnedLog.warn($"Network error requesting host details: \"{request.error}\"");
						return false;
					}
				}
			}

			if (input.Length >= 6 && ulong.TryParse(input, out ulong serverCode))
			{
				steamId = new CSteamID(serverCode);
				if (steamId.BGameServerAccount())
				{
					return true;
				}
				else
				{
					steamId = CSteamID.Nil;
				}
			}

			if (string.Equals(input, "localhost", System.StringComparison.OrdinalIgnoreCase))
			{
				address = new IPv4Address("127.0.0.1");
				return true;
			}
			
			// First try parsing as-is before using DNS lookup. DNS lookup *should* handle parsing an IPv4 address,
			// but it can also block the main thread. Maybe a non-issue. (public issue #4413)
			if (IPv4Address.TryParse(input, out address))
			{
				return true;
			}

			string resolvedAddress;
			try
			{
				IPAddress[] addresses = Dns.GetHostAddresses(input);
				if (addresses.Length > 0 && addresses[0] != null)
				{
					resolvedAddress = addresses[0].ToString();
				}
				else
				{
					resolvedAddress = null;
				}
			}
			catch (System.Exception exception)
			{
				resolvedAddress = input;
				UnturnedLog.exception(exception, $"Caught exception while resolving host string \"{input}\":");
			}

			if (string.IsNullOrEmpty(resolvedAddress))
			{
				UnturnedLog.info("Resolved address was empty");
				return false;
			}

			if (!IPv4Address.TryParse(resolvedAddress, out address))
			{
				UnturnedLog.info($"Unable to parse resolved address \"{resolvedAddress}\"");
				return false;
			}

			return true;
		}

		private static void onClickedConnectButton(ISleekElement button)
		{
			SplitHostIntoAddressAndPort();

			IPv4Address finalAddress;
			CSteamID steamId;
			ushort queryPortOverride;
			if (!TryParseHostString(hostField.Text, out finalAddress, out steamId, out queryPortOverride))
			{
				UnturnedLog.info("Cannot connect because unable to parse host string");
				return;
			}

			if (steamId.BGameServerAccount())
			{
				ServerConnectParameters connectParameters = new ServerConnectParameters(steamId, passwordField.Text);
				Provider.connect(connectParameters, null, null);
				return;
			}

			ushort queryPort = queryPortOverride > 0 ? queryPortOverride : portField.Value;
			if (queryPort == 0)
			{
				UnturnedLog.info("Cannot connect because port field is empty");
				return;
			}

			SteamConnectionInfo info = new SteamConnectionInfo(finalAddress.value, queryPort, passwordField.Text);

			connect(info, false, MenuPlayServerInfoUI.EServerInfoOpenContext.CONNECT);
		}

		private static void onTypedHostField(ISleekField field, string text)
		{
			PlaySettings.connectHost = text;
			addressInfoBox.IsVisible = false;
			RefreshServerCodeInfo();
		}

		private static void SplitHostIntoAddressAndPort()
		{
			string text = hostField.Text;
			int portDelimiterIndex = text.LastIndexOf(':');
			if (portDelimiterIndex < 0)
				return;

			ushort port;
			if (!ushort.TryParse(text.Substring(portDelimiterIndex + 1), out port))
				return;

			string prePortText = text.Substring(0, portDelimiterIndex);
			PlaySettings.connectHost = prePortText;
			PlaySettings.connectPort = port;
			hostField.Text = PlaySettings.connectHost;
			portField.Value = PlaySettings.connectPort;
		}

		private static void OnIpFieldCommitted(ISleekField field)
		{
			SplitHostIntoAddressAndPort();
			RefreshAddressInfo();
			RefreshServerCodeInfo();
		}

		private static void onTypedPortField(ISleekUInt16Field field, ushort state)
		{
			PlaySettings.connectPort = state;
		}

		private static void onTypedPasswordField(ISleekField field, string text)
		{
			PlaySettings.connectPassword = text;
		}

		private static void onAttemptUpdated(int attempt)
		{
			MenuUI.openAlert(localization.format("Connecting", attempt));
		}

		private static void onTimedOut()
		{
			if (Provider.connectionFailureInfo != ESteamConnectionFailureInfo.NONE)
			{
				ESteamConnectionFailureInfo info = Provider.connectionFailureInfo;
				Provider.resetConnectionFailure();

				if (info == ESteamConnectionFailureInfo.PRO_SERVER)
				{
					MenuUI.alert(localization.format("Pro_Server"));
				}
				else if (info == ESteamConnectionFailureInfo.PASSWORD)
				{
					MenuUI.alert(localization.format("Password"));
				}
				else if (info == ESteamConnectionFailureInfo.FULL)
				{
					MenuUI.alert(localization.format("Full"));
				}
				else if (info == ESteamConnectionFailureInfo.TIMED_OUT)
				{
					MenuUI.alert(localization.format("Timed_Out"));
				}
			}
		}

		private static void RefreshAddressInfo()
		{
			addressInfoBox.IsVisible = false;

			string inputIP = hostField.Text.ToLower();
			inputIP = inputIP.Trim();

			if (string.IsNullOrEmpty(inputIP))
				return;

			ulong steamId;
			if (inputIP.Length >= 6 && ulong.TryParse(inputIP, out steamId))
			{
				// Server code, not an address.
				return;
			}

			string parsedIP = null;
			if (inputIP == "localhost")
			{
				parsedIP = "127.0.0.1";
			}
			else
			{
				try
				{
					IPAddress[] address = Dns.GetHostAddresses(inputIP);
					if (address.Length > 0 && address[0] != null)
					{
						parsedIP = address[0].ToString();
					}
					else
					{
						parsedIP = null;
					}
				}
				catch (System.Exception exception)
				{
					UnturnedLog.exception(exception, $"Caught exception while resolving \"{inputIP}\" for address info box:");
					parsedIP = inputIP;
				}
			}

			if (string.IsNullOrEmpty(parsedIP))
				return;

			IPv4Address finalAddress;
			if (!IPv4Address.TryParse(parsedIP, out finalAddress))
				return;

			if (finalAddress.IsLoopback)
			{
				addressInfoBox.Text = localization.format("Address_Loopback_Label");
				addressInfoBox.TooltipText = localization.format("Address_Loopback_Tooltip");
				addressInfoBox.IsVisible = true;
			}
			else if (finalAddress.IsLocalPrivate)
			{
				addressInfoBox.Text = localization.format("Address_LocalPrivate_Label");
				addressInfoBox.TooltipText = localization.format("Address_LocalPrivate_Tooltip");
				addressInfoBox.IsVisible = true;
			}
		}

		private static void RefreshServerCodeInfo()
		{
			serverCodeInfoBox.IsVisible = false;
			portField.IsVisible = true;

			string inputIP = hostField.Text.ToLower();
			inputIP = inputIP.Trim();

			if (string.IsNullOrEmpty(inputIP) || inputIP.Length < 6)
				return;

			ulong serverCode;
			if (!ulong.TryParse(inputIP, out serverCode))
			{
				// Not a server code.
				return;
			}

			CSteamID steamId = new CSteamID(serverCode);
			if (steamId.BGameServerAccount())
			{
				serverCodeInfoBox.Text = localization.format("ServerCode_Valid_Label");
				serverCodeInfoBox.TooltipText = localization.format("ServerCode_Valid_Tooltip");
				serverCodeIcon.Texture = icons.load<Texture2D>("ValidServerCode");
				serverCodeIcon.TintColor = ESleekTint.FOREGROUND;
			}
			else if (steamId.BIndividualAccount())
			{
				serverCodeInfoBox.Text = localization.format("ServerCode_Invalid_Label");
				serverCodeInfoBox.TooltipText = localization.format("ServerCode_Friend_Tooltip");
				serverCodeIcon.Texture = icons.load<Texture2D>("InvalidServerCode");
				serverCodeIcon.TintColor = ESleekTint.BAD;
			}
			else
			{
				serverCodeInfoBox.Text = localization.format("ServerCode_Invalid_Label");
				serverCodeInfoBox.TooltipText = localization.format("ServerCode_Invalid_Tooltip");
				serverCodeIcon.Texture = icons.load<Texture2D>("InvalidServerCode");
				serverCodeIcon.TintColor = ESleekTint.BAD;
			}
			serverCodeInfoBox.IsVisible = true;
			portField.IsVisible = false;
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuPlayUI.open();
			close();
		}

		internal static void HandlePendingServerRelayRequest()
		{
			hasPendingServerRelay = false;

			UnturnedLog.info("Relay connect IP: {0} Port: {1} Code: {2} Password: \"{3}\"", Parser.getIPFromUInt32(serverRelayIP), serverRelayPort, serverRelayServerCode, serverRelayPassword);
			bool joinImmediately = serverRelayWaitOnMenu == false;

			if (serverRelayServerCode != CSteamID.Nil)
			{
				if (serverRelayServerCode.BGameServerAccount())
				{
					ServerConnectParameters connectParameters = new ServerConnectParameters(serverRelayServerCode, serverRelayPassword);
					Provider.connect(connectParameters, null, null);
				}
				else
				{
					UnturnedLog.warn($"Unable to join non-gameserver code ({serverRelayServerCode.GetEAccountType()})");
				}
			}
			else
			{
				SteamConnectionInfo info = new SteamConnectionInfo(serverRelayIP, serverRelayPort, serverRelayPassword);
				connect(info, joinImmediately, MenuPlayServerInfoUI.EServerInfoOpenContext.CONNECT);
			}
		}

		public void OnDestroy()
		{
			Provider.provider.matchmakingService.onAttemptUpdated -= onAttemptUpdated;
			Provider.provider.matchmakingService.onTimedOut -= onTimedOut;
		}

		public MenuPlayConnectUI()
		{
			localization = Localization.read("/Menu/Play/MenuPlayConnect.dat");
			icons = Bundles.getIconsBundle("UI/Menu/Icons/Play/MenuPlayConnect");

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

			hostField = Glazier.Get().CreateStringField();
			hostField.PositionOffset_X = -300;
			hostField.PositionOffset_Y = -75;
			hostField.PositionScale_X = 0.5f;
			hostField.PositionScale_Y = 0.5f;
			hostField.SizeOffset_X = 600;
			hostField.SizeOffset_Y = 30;
			hostField.MaxLength = 64;
			hostField.AddLabel(localization.format("Host_Field_Label"), ESleekSide.RIGHT);
			hostField.TooltipText = localization.format("Host_Field_Tooltip");
			hostField.Text = PlaySettings.connectHost;
			hostField.OnTextChanged += onTypedHostField;
			hostField.OnTextSubmitted += OnIpFieldCommitted;
			container.AddChild(hostField);

			addressInfoBox = Glazier.Get().CreateBox();
			addressInfoBox.PositionOffset_X = -410;
			addressInfoBox.PositionOffset_Y = -75;
			addressInfoBox.PositionScale_X = 0.5f;
			addressInfoBox.PositionScale_Y = 0.5f;
			addressInfoBox.SizeOffset_X = 100;
			addressInfoBox.SizeOffset_Y = 30;
			addressInfoBox.IsVisible = false;
			container.AddChild(addressInfoBox);

			serverCodeInfoBox = Glazier.Get().CreateBox();
			serverCodeInfoBox.PositionOffset_X = -300;
			serverCodeInfoBox.PositionOffset_Y = -35;
			serverCodeInfoBox.PositionScale_X = 0.5f;
			serverCodeInfoBox.PositionScale_Y = 0.5f;
			serverCodeInfoBox.SizeOffset_X = 600;
			serverCodeInfoBox.SizeOffset_Y = 30;
			serverCodeInfoBox.IsVisible = false;
			container.AddChild(serverCodeInfoBox);

			serverCodeIcon = Glazier.Get().CreateImage();
			serverCodeIcon.PositionOffset_X = 5;
			serverCodeIcon.PositionOffset_Y = 5;
			serverCodeIcon.SizeOffset_X = 20;
			serverCodeIcon.SizeOffset_Y = 20;
			serverCodeInfoBox.AddChild(serverCodeIcon);

			portField = Glazier.Get().CreateUInt16Field();
			portField.PositionOffset_X = -300;
			portField.PositionOffset_Y = -35;
			portField.PositionScale_X = 0.5f;
			portField.PositionScale_Y = 0.5f;
			portField.SizeOffset_X = 600;
			portField.SizeOffset_Y = 30;
			portField.AddLabel(localization.format("Port_Field_Label"), ESleekSide.RIGHT);
			portField.TooltipText = localization.format("Port_Field_Tooltip");
			portField.Value = PlaySettings.connectPort;
			portField.OnValueChanged += onTypedPortField;
			container.AddChild(portField);

			passwordField = Glazier.Get().CreateStringField();
			passwordField.PositionOffset_X = -300;
			passwordField.PositionOffset_Y = 5;
			passwordField.PositionScale_X = 0.5f;
			passwordField.PositionScale_Y = 0.5f;
			passwordField.SizeOffset_X = 600;
			passwordField.SizeOffset_Y = 30;
			passwordField.AddLabel(localization.format("Password_Field_Label"), ESleekSide.RIGHT);
			passwordField.IsPasswordField = true;
			passwordField.MaxLength = 0; // Disable
			passwordField.Text = PlaySettings.connectPassword;
			passwordField.OnTextChanged += onTypedPasswordField;
			container.AddChild(passwordField);

			connectButton = new SleekButtonIcon(icons.load<Texture2D>("Connect"));
			connectButton.PositionOffset_X = -300;
			connectButton.PositionOffset_Y = 45;
			connectButton.PositionScale_X = 0.5f;
			connectButton.PositionScale_Y = 0.5f;
			connectButton.SizeOffset_X = 600;
			connectButton.SizeOffset_Y = 30;
			connectButton.text = localization.format("Connect_Button");
			connectButton.tooltip = localization.format("Connect_Button_Tooltip");
			connectButton.iconColor = ESleekTint.FOREGROUND;
			connectButton.onClickedButton += onClickedConnectButton;
			container.AddChild(connectButton);

			RefreshAddressInfo();
			RefreshServerCodeInfo();

			Provider.provider.matchmakingService.onAttemptUpdated += onAttemptUpdated;
			Provider.provider.matchmakingService.onTimedOut += onTimedOut;

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
