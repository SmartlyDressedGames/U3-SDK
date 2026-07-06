////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ClickedServer(SleekServer server, SteamServerAdvertisement info);

	public class SleekServer : SleekWrapper
	{
		private SteamServerAdvertisement info;

		private SleekButtonIcon favoriteButton;
		private ISleekButton button;
		private ISleekBox mapBox;
		private ISleekBox playersBox;
		private ISleekBox maxPlayersBox;
		private ISleekBox fullnessBox;
		private ISleekBox pingBox;
		private ISleekBox anticheatBox;
		private ISleekBox perspectiveBox;
		private ISleekBox combatBox;
		private ISleekBox passwordBox;
		private ISleekBox workshopBox;
		private ISleekBox goldBox;
		private ISleekBox cheatsBox;
		private ISleekBox monetizationBox;
		private ISleekBox pluginsBox;
		private SleekWebImage thumbnail;
		private ISleekLabel nameLabel;

		public ClickedServer onClickedServer;

		/// <summary>
		/// Is the server this widget represents currently favorited?
		/// Can be false on the favorites list.
		/// </summary>
		public bool isCurrentlyFavorited => Provider.GetServerIsFavorited(info.ip, info.queryPort);

		public void SynchronizeVisibleColumns()
		{
			const float spacing = 0;
			float horizontalOffset = 0;

			if (FilterSettings.columns.anticheat)
			{
				horizontalOffset -= anticheatBox.SizeOffset_X;
				anticheatBox.PositionOffset_X = horizontalOffset;
				anticheatBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				anticheatBox.IsVisible = false;
			}

			if (FilterSettings.columns.cheats)
			{
				horizontalOffset -= cheatsBox.SizeOffset_X;
				cheatsBox.PositionOffset_X = horizontalOffset;
				cheatsBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				cheatsBox.IsVisible = false;
			}

			if (FilterSettings.columns.plugins)
			{
				horizontalOffset -= pluginsBox.SizeOffset_X;
				pluginsBox.PositionOffset_X = horizontalOffset;
				pluginsBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				pluginsBox.IsVisible = false;
			}

			if (FilterSettings.columns.workshop)
			{
				horizontalOffset -= workshopBox.SizeOffset_X;
				workshopBox.PositionOffset_X = horizontalOffset;
				workshopBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				workshopBox.IsVisible = false;
			}

			if (FilterSettings.columns.monetization)
			{
				horizontalOffset -= monetizationBox.SizeOffset_X;
				monetizationBox.PositionOffset_X = horizontalOffset;
				monetizationBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				monetizationBox.IsVisible = false;
			}

			if (FilterSettings.columns.gold)
			{
				horizontalOffset -= goldBox.SizeOffset_X;
				goldBox.PositionOffset_X = horizontalOffset;
				goldBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				goldBox.IsVisible = false;
			}

			if (FilterSettings.columns.perspective)
			{
				horizontalOffset -= perspectiveBox.SizeOffset_X;
				perspectiveBox.PositionOffset_X = horizontalOffset;
				perspectiveBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				perspectiveBox.IsVisible = false;
			}

			if (FilterSettings.columns.combat)
			{
				horizontalOffset -= combatBox.SizeOffset_X;
				combatBox.PositionOffset_X = horizontalOffset;
				combatBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				combatBox.IsVisible = false;
			}

			if (FilterSettings.columns.password)
			{
				horizontalOffset -= passwordBox.SizeOffset_X;
				passwordBox.PositionOffset_X = horizontalOffset;
				passwordBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				passwordBox.IsVisible = false;
			}

			if (FilterSettings.columns.fullnessPercentage)
			{
				horizontalOffset -= fullnessBox.SizeOffset_X;
				fullnessBox.PositionOffset_X = horizontalOffset;
				fullnessBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				fullnessBox.IsVisible = false;
			}

			if (FilterSettings.columns.maxPlayers)
			{
				horizontalOffset -= maxPlayersBox.SizeOffset_X;
				maxPlayersBox.PositionOffset_X = horizontalOffset;
				maxPlayersBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				maxPlayersBox.IsVisible = false;
			}

			if (FilterSettings.columns.players)
			{
				if (FilterSettings.columns.maxPlayers)
				{
					playersBox.SizeOffset_X = 80;
					playersBox.Text = info.players.ToString();
				}
				else
				{
					playersBox.SizeOffset_X = 120;
					playersBox.Text = MenuPlayUI.serverListUI.localization.format("Server_Players", info.players, info.maxPlayers);
				}

				horizontalOffset -= playersBox.SizeOffset_X;
				playersBox.PositionOffset_X = horizontalOffset;
				playersBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				playersBox.IsVisible = false;
			}

			if (FilterSettings.columns.ping)
			{
				horizontalOffset -= pingBox.SizeOffset_X;
				pingBox.PositionOffset_X = horizontalOffset;
				pingBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				pingBox.IsVisible = false;
			}

			if (FilterSettings.columns.map)
			{
				horizontalOffset -= mapBox.SizeOffset_X;
				mapBox.PositionOffset_X = horizontalOffset;
				mapBox.IsVisible = true;
				horizontalOffset -= spacing;
			}
			else
			{
				mapBox.IsVisible = false;
			}

			horizontalOffset -= button.PositionOffset_X;
			button.SizeOffset_X = horizontalOffset;
		}

		private void onClickedFavoriteOffButton(ISleekElement button)
		{
			Provider.SetServerIsFavorited(info.ip, info.connectionPort, info.queryPort, !isCurrentlyFavorited);
			refreshFavoriteButton();
		}

		private void refreshFavoriteButton()
		{
			if (isCurrentlyFavorited)
			{
				button.IsClickable = true;

				favoriteButton.tooltip = MenuPlayUI.serverListUI.localization.format("Favorite_Off_Button_Tooltip");
				favoriteButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Favorite_Off");
			}
			else
			{
				button.IsClickable = false;

				favoriteButton.tooltip = MenuPlayUI.serverListUI.localization.format("Favorite_On_Button_Tooltip");
				favoriteButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Favorite_On");
			}
		}

		private void onClickedButton(ISleekElement button)
		{
			if (!Provider.isPro && info.isPro)
			{
				Provider.provider.storeService.open(new SteamworksProvider.Services.Store.SteamworksStorePackageID(Provider.PRO_ID.m_AppId));
			}
			else
			{
				onClickedServer?.Invoke(this, info);
			}
		}

		public SleekServer(ESteamServerList list, SteamServerAdvertisement newInfo) : base()
		{
			info = newInfo;

			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1;
			button.SizeScale_Y = 1;
			button.OnClicked += onClickedButton;

			if (info.deniedByRule != null)
			{
				button.TooltipText = MenuPlayUI.serverListUI.localization.format("BlockedByCurator_Tooltip",
					info.deniedByRule.owner.Name, info.deniedByRule.description);
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (!string.IsNullOrEmpty(button.TooltipText))
			{
				button.TooltipText += "\n";
			}
			button.TooltipText += info.GetUtilityScoreDebugText();
#endif

			if (info.isDeniedByServerCurationRule)
			{
				button.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
			}

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 45;
			nameLabel.SizeScale_X = 1;
			nameLabel.SizeOffset_X = -45;
			nameLabel.TextAlignment = TextAnchor.MiddleLeft;
			nameLabel.Text = info.name;
			button.AddChild(nameLabel);

			if (info.isDeniedByServerCurationRule)
			{
				nameLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
			}

			if (string.IsNullOrEmpty(info.descText) && string.IsNullOrEmpty(info.serverCurationLabels))
			{
				nameLabel.SizeOffset_Y = 40;
			}
			else
			{
				nameLabel.SizeOffset_Y = 30;
			}

			if (!string.IsNullOrEmpty(info.descText))
			{
				ISleekLabel descLabel = Glazier.Get().CreateLabel();
				descLabel.PositionOffset_X = 45;
				descLabel.PositionOffset_Y = 15;
				descLabel.SizeScale_X = 1;
				descLabel.SizeOffset_X = -50;
				descLabel.SizeOffset_Y = 30;
				descLabel.FontSize = ESleekFontSize.Small;
				descLabel.AllowRichText = true;

				if (info.isDeniedByServerCurationRule)
				{
					descLabel.TextColor = new SleekColor(ESleekTint.RICH_TEXT_DEFAULT, 0.5f);
				}
				else
				{
					descLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				}

				descLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				descLabel.TextAlignment = TextAnchor.MiddleLeft;
				descLabel.Text = info.descText;
				button.AddChild(descLabel);
			}

			if (!string.IsNullOrEmpty(info.serverCurationLabels))
			{
				ISleekLabel curationLabel = Glazier.Get().CreateLabel();
				curationLabel.PositionOffset_X = 45;
				curationLabel.PositionOffset_Y = 15;
				curationLabel.SizeScale_X = 1;
				curationLabel.SizeOffset_X = -50;
				curationLabel.SizeOffset_Y = 30;
				curationLabel.FontSize = ESleekFontSize.Small;
				curationLabel.AllowRichText = true;
				curationLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				curationLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				curationLabel.TextAlignment = TextAnchor.MiddleRight;
				curationLabel.Text = info.serverCurationLabels;
				button.AddChild(curationLabel);
			}

			mapBox = Glazier.Get().CreateBox();
			mapBox.PositionScale_X = 1;
			mapBox.SizeOffset_X = 153;
			mapBox.SizeScale_Y = 1;

			Texture2D mapIcon = LevelIconCache.GetOrLoadIcon(info.map);
			if (mapIcon != null)
			{
				ISleekImage mapImage = Glazier.Get().CreateImage(mapIcon);
				mapImage.PositionOffset_X = 5;
				mapImage.PositionOffset_Y = 5;
				mapImage.SizeOffset_X = 143;
				mapImage.SizeOffset_Y = 30;
				mapBox.AddChild(mapImage);

				ISleekLabel mapLabel = Glazier.Get().CreateLabel();
				mapLabel.SizeScale_X = 1f;
				mapLabel.SizeScale_Y = 1f;
				mapLabel.TextAlignment = TextAnchor.MiddleCenter;
				mapLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				mapLabel.Text = info.map;
				mapBox.AddChild(mapLabel);
			}
			else
			{
				mapBox.Text = info.map;
			}

			playersBox = Glazier.Get().CreateBox();
			playersBox.PositionScale_X = 1;
			playersBox.SizeOffset_X = 80;
			playersBox.SizeScale_Y = 1;

			maxPlayersBox = Glazier.Get().CreateBox();
			maxPlayersBox.PositionScale_X = 1;
			maxPlayersBox.SizeOffset_X = 80;
			maxPlayersBox.SizeScale_Y = 1;
			maxPlayersBox.Text = info.maxPlayers.ToString();

			fullnessBox = Glazier.Get().CreateBox();
			fullnessBox.PositionScale_X = 1;
			fullnessBox.SizeOffset_X = 80;
			fullnessBox.SizeScale_Y = 1;
			fullnessBox.Text = info.NormalizedPlayerCount.ToString("P0");
			fullnessBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Server_Players", info.players, info.maxPlayers);

			pingBox = Glazier.Get().CreateBox();
			pingBox.PositionScale_X = 1;
			pingBox.SizeOffset_X = 80;
			pingBox.SizeScale_Y = 1;
			if (info.anycastProxyMode != SteamServerAdvertisement.EAnycastProxyMode.None)
			{
				pingBox.Text = $"{info.PingMs} ms*";

				if (info.anycastProxyMode == SteamServerAdvertisement.EAnycastProxyMode.FlaggedByModerator)
				{
					pingBox.TextColor = ESleekTint.BAD;
				}

				// Shouldn't be null by this point but don't want to risk breaking stuff in the patch.
				if (MenuPlayServerInfoUI.localization != null)
				{
					pingBox.TooltipText = MenuPlayServerInfoUI.localization.format("HostBan_QueryPingWarning");
				}
			}
			else
			{
				pingBox.Text = $"{info.PingMs} ms";
			}

			anticheatBox = Glazier.Get().CreateBox();
			anticheatBox.PositionScale_X = 1;
			anticheatBox.SizeOffset_X = 80;
			anticheatBox.SizeScale_Y = 1;

#if WITH_THIRDPARTYAC
			ISleekImage thirdpartyAntiCheatIcon = Glazier.Get().CreateImage();
			thirdpartyAntiCheatIcon.PositionOffset_X = 15;
			thirdpartyAntiCheatIcon.PositionOffset_Y = 10;
			thirdpartyAntiCheatIcon.SizeOffset_X = 20;
			thirdpartyAntiCheatIcon.SizeOffset_Y = 20;
			thirdpartyAntiCheatIcon.TintColor = ESleekTint.FOREGROUND;
			anticheatBox.AddChild(thirdpartyAntiCheatIcon);

			if (info.IsThirdpartyAntiCheatEnabled)
			{
				thirdpartyAntiCheatIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>(ThirdpartyAntiCheat.IconName);
			}
			else
			{
				thirdpartyAntiCheatIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>(ThirdpartyAntiCheat.IconInsecureName);
			}
#endif

			ISleekImage vacIcon = Glazier.Get().CreateImage();
			vacIcon.PositionOffset_X = 45;
			vacIcon.PositionOffset_Y = 10;
			vacIcon.SizeOffset_X = 20;
			vacIcon.SizeOffset_Y = 20;
			vacIcon.TintColor = ESleekTint.FOREGROUND;
			anticheatBox.AddChild(vacIcon);

			if (info.IsVACSecure)
			{
				vacIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("VAC");
			}
			else
			{
				vacIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("VAC_Off");
			}

#if WITH_THIRDPARTYAC
			if (info.IsThirdpartyAntiCheatEnabled && info.IsVACSecure)
			{
				anticheatBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Anticheat_Column_Both_Tooltip");
			}
			else if (info.IsThirdpartyAntiCheatEnabled)
			{
				anticheatBox.TooltipText = MenuPlayUI.serverListUI.localization.format(ThirdpartyAntiCheat.ServerListAnticheatColumnTooltipKey);
			}
			else if (info.IsVACSecure)
#else
			if (info.IsVACSecure)
#endif
			{
				anticheatBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Anticheat_Column_VAC_Tooltip");
			}
			else
			{
				anticheatBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Anticheat_Column_None_Tooltip");
			}

			perspectiveBox = Glazier.Get().CreateBox();
			perspectiveBox.PositionScale_X = 1;
			perspectiveBox.SizeOffset_X = 40;
			perspectiveBox.SizeScale_Y = 1;

			ISleekImage perspectiveIcon = Glazier.Get().CreateImage();
			perspectiveIcon.PositionOffset_X = 10;
			perspectiveIcon.PositionOffset_Y = 10;
			perspectiveIcon.SizeOffset_X = 20;
			perspectiveIcon.SizeOffset_Y = 20;
			perspectiveIcon.TintColor = ESleekTint.FOREGROUND;
			switch (info.cameraMode)
			{
				case ECameraMode.FIRST:
				{
					perspectiveIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Perspective_FirstPerson");
					perspectiveBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Perspective_Column_First_Tooltip");
				}
				break;

				case ECameraMode.THIRD:
				{
					perspectiveIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Perspective_ThirdPerson");
					perspectiveBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Perspective_Column_Third_Tooltip");
				}
				break;

				case ECameraMode.BOTH:
				{
					perspectiveIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Perspective_Both");
					perspectiveBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Perspective_Column_Both_Tooltip");
				}
				break;

				case ECameraMode.VEHICLE:
				{
					perspectiveIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Perspective_Vehicle");
					perspectiveBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Perspective_Column_Vehicle_Tooltip");
				}
				break;
			}
			perspectiveBox.AddChild(perspectiveIcon);

			combatBox = Glazier.Get().CreateBox();
			combatBox.PositionScale_X = 1;
			combatBox.SizeOffset_X = 40;
			combatBox.SizeScale_Y = 1;

			if (info.isPvP)
			{
				combatBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Combat_Column_PvP_Tooltip");
			}
			else
			{
				combatBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Combat_Column_PvE_Tooltip");
			}

			ISleekImage combatIcon = Glazier.Get().CreateImage();
			combatIcon.PositionOffset_X = 10;
			combatIcon.PositionOffset_Y = 10;
			combatIcon.SizeOffset_X = 20;
			combatIcon.SizeOffset_Y = 20;
			combatIcon.TintColor = ESleekTint.FOREGROUND;
			combatIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>(info.isPvP ? "PvP" : "PvE");
			combatBox.AddChild(combatIcon);

			passwordBox = Glazier.Get().CreateBox();
			passwordBox.PositionScale_X = 1;
			passwordBox.SizeOffset_X = 40;
			passwordBox.SizeScale_Y = 1;
			ISleekImage passwordIcon = Glazier.Get().CreateImage();
			passwordIcon.PositionOffset_X = 10;
			passwordIcon.PositionOffset_Y = 10;
			passwordIcon.SizeOffset_X = 20;
			passwordIcon.SizeOffset_Y = 20;
			passwordIcon.TintColor = ESleekTint.FOREGROUND;
			passwordBox.AddChild(passwordIcon);
			if (info.isPassworded)
			{

				passwordIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("PasswordProtected");
				passwordBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Password_Column_Yes_Tooltip");
			}
			else
			{
				passwordIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("NotPasswordProtected");
				passwordBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Password_Column_No_Tooltip");
			}

			workshopBox = Glazier.Get().CreateBox();
			workshopBox.PositionScale_X = 1;
			workshopBox.SizeOffset_X = 40;
			workshopBox.SizeScale_Y = 1;
			ISleekImage workshopIcon = Glazier.Get().CreateImage();
			workshopIcon.PositionOffset_X = 10;
			workshopIcon.PositionOffset_Y = 10;
			workshopIcon.SizeOffset_X = 20;
			workshopIcon.SizeOffset_Y = 20;
			workshopIcon.TintColor = ESleekTint.FOREGROUND;
			workshopBox.AddChild(workshopIcon);
			if (info.isWorkshop)
			{
				workshopIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("HasMods");
				workshopBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Workshop_Column_Yes_Tooltip");
			}
			else
			{
				workshopIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("NoMods");
				workshopBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Workshop_Column_No_Tooltip");
			}

			goldBox = Glazier.Get().CreateBox();
			goldBox.PositionScale_X = 1;
			goldBox.SizeOffset_X = 40;
			goldBox.SizeScale_Y = 1;
			goldBox.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
			goldBox.TextColor = Palette.PRO;
			ISleekImage goldIcon = Glazier.Get().CreateImage();
			goldIcon.PositionOffset_X = 10;
			goldIcon.PositionOffset_Y = 10;
			goldIcon.SizeOffset_X = 20;
			goldIcon.SizeOffset_Y = 20;
			goldIcon.TintColor = Palette.PRO;
			goldBox.AddChild(goldIcon);
			if (info.isPro)
			{
				goldIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("GoldRequired");
				goldBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Gold_Column_Yes_Tooltip");
			}
			else
			{
				goldIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("GoldNotRequired");
				goldBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Gold_Column_No_Tooltip");
			}

			cheatsBox = Glazier.Get().CreateBox();
			cheatsBox.PositionScale_X = 1;
			cheatsBox.SizeOffset_X = 40;
			cheatsBox.SizeScale_Y = 1;
			ISleekImage cheatsIcon = Glazier.Get().CreateImage();
			cheatsIcon.PositionOffset_X = 10;
			cheatsIcon.PositionOffset_Y = 10;
			cheatsIcon.SizeOffset_X = 20;
			cheatsIcon.SizeOffset_Y = 20;
			cheatsIcon.TintColor = ESleekTint.FOREGROUND;
			cheatsBox.AddChild(cheatsIcon);
			if (info.hasCheats)
			{
				cheatsIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("CheatCodes");
				cheatsBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Cheats_Column_Yes_Tooltip");
			}
			else
			{
				cheatsIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("CheatCodes_None");
				cheatsBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Cheats_Column_No_Tooltip");
			}

			monetizationBox = Glazier.Get().CreateBox();
			monetizationBox.PositionScale_X = 1;
			monetizationBox.SizeOffset_X = 40;
			monetizationBox.SizeScale_Y = 1;
			ISleekImage monetizationIcon = Glazier.Get().CreateImage();
			monetizationIcon.PositionOffset_X = 10;
			monetizationIcon.PositionOffset_Y = 10;
			monetizationIcon.SizeOffset_X = 20;
			monetizationIcon.SizeOffset_Y = 20;
			monetizationIcon.TintColor = ESleekTint.FOREGROUND;
			monetizationBox.AddChild(monetizationIcon);
			switch (info.monetization)
			{
				case EServerMonetizationTag.Unspecified:
					monetizationIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Unknown");
					monetizationBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Monetization_Column_Unspecified_Tooltip");
					break;

				case EServerMonetizationTag.NonGameplay:
					monetizationIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("NonGameplayMonetization");
					monetizationBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Monetization_Column_NonGameplay_Tooltip");
					break;

				case EServerMonetizationTag.Monetized:
					monetizationIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Monetized");
					monetizationBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Monetization_Column_Monetized_Tooltip");
					break;

				case EServerMonetizationTag.None:
					monetizationIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Monetization_None");
					monetizationBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Monetization_Column_None_Tooltip");
					break;
			}

			pluginsBox = Glazier.Get().CreateBox();
			pluginsBox.PositionScale_X = 1;
			pluginsBox.SizeOffset_X = 40;
			pluginsBox.SizeScale_Y = 1;
			ISleekImage pluginsIcon = Glazier.Get().CreateImage();
			pluginsIcon.PositionOffset_X = 10;
			pluginsIcon.PositionOffset_Y = 10;
			pluginsIcon.SizeOffset_X = 20;
			pluginsIcon.SizeOffset_Y = 20;
			pluginsIcon.TintColor = ESleekTint.FOREGROUND;
			pluginsBox.AddChild(pluginsIcon);
			switch (info.pluginFramework)
			{
				case SteamServerAdvertisement.EPluginFramework.None:
					pluginsIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Plugins_None");
					pluginsBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Plugins_Column_None_Tooltip");
					break;

				case SteamServerAdvertisement.EPluginFramework.Rocket:
					pluginsIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("RocketMod");
					pluginsBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Plugins_Column_Rocket_Tooltip");
					break;

				case SteamServerAdvertisement.EPluginFramework.OpenMod:
					pluginsIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("OpenMod");
					pluginsBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Plugins_Column_OpenMod_Tooltip");
					break;

				case SteamServerAdvertisement.EPluginFramework.Unknown:
					pluginsIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("Unknown");
					pluginsBox.TooltipText = MenuPlayUI.serverListUI.localization.format("Plugins_Column_Unknown_Tooltip");
					break;
			}

			SynchronizeVisibleColumns();

			AddChild(button);
			AddChild(mapBox);
			AddChild(playersBox);
			AddChild(maxPlayersBox);
			AddChild(fullnessBox);
			AddChild(pingBox);
			AddChild(anticheatBox);
			AddChild(perspectiveBox);
			AddChild(combatBox);
			AddChild(passwordBox);
			AddChild(workshopBox);
			AddChild(goldBox);
			AddChild(cheatsBox);
			AddChild(monetizationBox);
			AddChild(pluginsBox);

			if (!string.IsNullOrEmpty(info.thumbnailURL))
			{
				thumbnail = new SleekWebImage();
				thumbnail.PositionOffset_X = 4;
				thumbnail.PositionOffset_Y = 4;
				thumbnail.SizeOffset_X = 32;
				thumbnail.SizeOffset_Y = 32;
				thumbnail.Refresh(info.thumbnailURL);

				if (info.isDeniedByServerCurationRule)
				{
					thumbnail.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
				}

				button.AddChild(thumbnail);
			}

			if (info.isPro && !Provider.isPro)
			{
				if (!info.isDeniedByServerCurationRule)
				{
					button.TextColor = Palette.PRO;
					button.TooltipText = MenuPlayUI.serverListUI.localization.format("Gold_Column_Yes_Tooltip");
				}

				ISleekImage lockIcon = Glazier.Get().CreateImage();
				lockIcon.PositionOffset_X = 10;
				lockIcon.PositionOffset_Y = 10;
				lockIcon.SizeOffset_X = 20;
				lockIcon.SizeOffset_Y = 20;
				lockIcon.TintColor = Palette.PRO;
				lockIcon.Texture = MenuPlayUI.serverListUI.icons.load<Texture2D>("GoldRequired");
				button.AddChild(lockIcon);
			}

			if (list == ESteamServerList.FAVORITES)
			{
				button.PositionOffset_X += 40;
				button.SizeOffset_X -= 40;

				favoriteButton = new SleekButtonIcon(null);
				favoriteButton.SizeOffset_X = 40;
				favoriteButton.SizeScale_Y = 1;
				favoriteButton.iconPositionOffset = 10;
				favoriteButton.iconColor = ESleekTint.FOREGROUND;
				favoriteButton.onClickedButton += onClickedFavoriteOffButton;
				AddChild(favoriteButton);

				refreshFavoriteButton();
			}
		}
	}
}
