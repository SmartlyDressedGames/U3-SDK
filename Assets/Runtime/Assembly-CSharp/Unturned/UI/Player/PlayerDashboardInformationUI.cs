////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerDashboardInformationUI
	{
		private class SleekInviteButton : SleekWrapper
		{
			public CSteamID groupID
			{
				get;
				protected set;
			}

			private void handleJoinButtonClicked(ISleekElement button)
			{
				Player.LocalPlayer.quests.SendAcceptGroupInvitation(groupID);
			}

			private void handleIgnoreButtonClicked(ISleekElement button)
			{
				Player.LocalPlayer.quests.SendDeclineGroupInvitation(groupID);
			}

			public SleekInviteButton(CSteamID newGroupID)
			{
				groupID = newGroupID;

				GroupInfo group = GroupManager.getGroupInfo(groupID);
				string name = group != null ? group.name : groupID.ToString();

				ISleekBox inviteBox = Glazier.Get().CreateBox();
				inviteBox.SizeOffset_X = -140;
				inviteBox.SizeScale_X = 1;
				inviteBox.SizeScale_Y = 1;
				inviteBox.Text = name;
				AddChild(inviteBox);

				ISleekButton joinButton = Glazier.Get().CreateButton();
				joinButton.PositionScale_X = 1;
				joinButton.SizeOffset_X = 60;
				joinButton.SizeScale_Y = 1;
				joinButton.Text = localization.format("Group_Join");
				joinButton.TooltipText = localization.format("Group_Join_Tooltip");
				joinButton.OnClicked += handleJoinButtonClicked;
				inviteBox.AddChild(joinButton);

				ISleekButton ignoreButton = Glazier.Get().CreateButton();
				ignoreButton.PositionOffset_X = 60;
				ignoreButton.PositionScale_X = 1;
				ignoreButton.SizeOffset_X = 80;
				ignoreButton.SizeScale_Y = 1;
				ignoreButton.Text = localization.format("Group_Ignore");
				ignoreButton.TooltipText = localization.format("Group_Ignore_Tooltip");
				ignoreButton.OnClicked += handleIgnoreButtonClicked;
				inviteBox.AddChild(ignoreButton);
			}
		}

		private static readonly List<SteamPlayer> sortedClients = new List<SteamPlayer>();

		public static Local localization;
		public static IconsBundle icons;
		private static SleekFullscreenBox container;

		public static bool active;
		private static int zoomMultiplier;
		private static int maxZoomMultiplier;

		private static ISleekBox backdropBox;

		private static ISleekElement mapInspect;
		private static ISleekScrollView mapBox;
		private static ISleekImage mapImage;

		/// <summary>
		/// Labels for named locations.
		/// </summary>
		private static ISleekElement mapLocationsContainer;

		/// <summary>
		/// Contains arena outer circle and inner target points.
		/// </summary>
		private static ISleekElement mapArenaContainer;
		private static ISleekElement mapMarkersContainer;
		private static ISleekElement mapRemotePlayersContainer;

		private static List<ISleekImage> markerImages;
		private static List<ISleekImage> arenaTargetPoints;

		/// <summary>
		/// Player avatars.
		/// </summary>
		private static List<ISleekImage> remotePlayerImages;

		/// <summary>
		/// Arrow oriented with the local player.
		/// </summary>
		private static ISleekImage localPlayerImage;

		private static ISleekImage arenaAreaCurrentOverlay;
		private static ISleekImage arenaAreaCurrentLeftOverlay;
		private static ISleekImage arenaAreaCurrentRightOverlay;
		private static ISleekImage arenaAreaCurrentUpOverlay;
		private static ISleekImage arenaAreaCurrentDownOverlay;

		private static ISleekToggle showMarkersToggle;
		private static ISleekToggle showPlayerNamesToggle;
		private static ISleekToggle showPlayerAvatarsToggle;

		private static SleekButtonIcon zoomInButton;
		private static SleekButtonIcon zoomOutButton;
		private static SleekButtonIcon centerButton;
		private static SleekButtonState mapButtonState;
		public static ISleekLabel noLabel;

		private static ISleekElement headerButtonsContainer;
		private static SleekButtonIcon questsButton;
		private static SleekButtonIcon groupsButton;
		private static SleekButtonIcon playersButton;
		private static ISleekScrollView questsBox;
		private static ISleekScrollView groupsBox;
		private static ISleekElement playersBox;
		private static SleekButtonState playerSortButton;
		private static SleekList<SteamPlayer> playersList;

		private static ISleekFloat64Field radioFrequencyField;
		private static ISleekField groupNameField;

		private static bool hasChart;
		private static bool hasGPS;

		private enum EInfoTab
		{
			QUESTS,
			GROUPS,
			PLAYERS
		}

		private static EInfoTab tab;

		private static Texture2D mapTexture;
		private static Texture2D chartTexture;
		private static Texture2D staticTexture;

		private static void synchronizeMapVisibility(int view)
		{
			if (view == 0) // chart
			{
				if (chartTexture != null && !PlayerUI.isBlindfolded && hasChart)
				{
					mapImage.Texture = chartTexture;
					noLabel.IsVisible = false;
				}
				else
				{
					mapImage.Texture = staticTexture;

					noLabel.Text = localization.format("No_Chart");
					noLabel.IsVisible = true;
				}
			}
			else // satellite
			{
				if (mapTexture != null && !PlayerUI.isBlindfolded && hasGPS)
				{
					mapImage.Texture = mapTexture;
					noLabel.IsVisible = false;
				}
				else
				{
					mapImage.Texture = staticTexture;

					noLabel.Text = localization.format("No_GPS");
					noLabel.IsVisible = true;
				}
			}

			bool isMapVisible = !noLabel.IsVisible;
			mapLocationsContainer.IsVisible = isMapVisible;

			// Arena circle, player positions, and group positions are hidden in hard mode.
			bool isDynamicMapVisible = isMapVisible && Provider.modeConfigData.Gameplay.Group_Map;

			mapMarkersContainer.IsVisible = isDynamicMapVisible && showMarkersToggle.Value;
			mapArenaContainer.IsVisible = isDynamicMapVisible && LevelManager.levelType == ELevelType.ARENA;
			mapRemotePlayersContainer.IsVisible = isDynamicMapVisible && (showPlayerNamesToggle.Value || showPlayerAvatarsToggle.Value);
			localPlayerImage.IsVisible = isDynamicMapVisible;
		}

		private static void updateMarkers()
		{
			int visibleMarkerCount = 0;
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
					continue; // no marker
				}

				ISleekImage markerImage;
				if (visibleMarkerCount < markerImages.Count)
				{
					markerImage = markerImages[visibleMarkerCount];
					markerImage.IsVisible = true;
				}
				else
				{
					markerImage = Glazier.Get().CreateImage(icons.load<Texture2D>("Marker"));
					markerImage.PositionOffset_X = -10;
					markerImage.PositionOffset_Y = -10;
					markerImage.SizeOffset_X = 20;
					markerImage.SizeOffset_Y = 20;
					markerImage.AddLabel(string.Empty, ESleekSide.RIGHT);
					mapMarkersContainer.AddChild(markerImage);
					markerImages.Add(markerImage);
				}
				++visibleMarkerCount;

				Vector2 mapPosition = ProjectWorldPositionToMap(quests.markerPosition);
				markerImage.PositionScale_X = mapPosition.x;
				markerImage.PositionScale_Y = mapPosition.y;
				markerImage.TintColor = player.markerColor;

				string markerText = quests.markerTextOverride;
				if (string.IsNullOrEmpty(markerText))
				{
					if (string.IsNullOrEmpty(player.playerID.nickName))
					{
						markerText = player.playerID.characterName;
					}
					else
					{
						markerText = player.playerID.nickName;
					}
				}
				markerImage.UpdateLabel(markerText);
			}

			for (int index = markerImages.Count - 1; index >= visibleMarkerCount; --index)
			{
				markerImages[index].IsVisible = false;
			}
		}

		private static void updateArenaCircle()
		{
			int pointCount = 0;
			// when arena compactor pause is disabled target radius is set to 0.5,
			// but this config value isn't replicated to the client so we assume that it's disabled
			// if that's the case we don't add the target overlay because it's ugly covering the entire map in yellow
			if (Mathf.Abs(LevelManager.arenaTargetRadius - 0.5f) > 0.01)
			{
				float interpCount = Mathf.Lerp(10.0f, 64.0f, LevelManager.arenaTargetRadius / 2000.0f); // Lerp T is clamped
				pointCount = Mathf.RoundToInt(interpCount);
				pointCount *= zoomMultiplier;
				if (pointCount > 1)
				{
					float animValue = Time.time / 100.0f; // 1 revolution per 100 seconds
					animValue -= Mathf.Floor(animValue); // 0 to 1 angle

					for (int pointIndex = 0; pointIndex < pointCount; ++pointIndex)
					{
						float pointAngle = (((float) pointIndex / pointCount) + animValue) * Mathf.PI * 2.0f;
						float pointX = Mathf.Cos(pointAngle);
						float pointY = Mathf.Sin(pointAngle);

						Vector3 levelPoint = LevelManager.arenaTargetCenter + new Vector3(pointX * LevelManager.arenaTargetRadius, 0.0f, pointY * LevelManager.arenaTargetRadius);
						Vector2 mapPoint = ProjectWorldPositionToMap(levelPoint);

						ISleekImage point;
						if (pointIndex < arenaTargetPoints.Count)
						{
							point = arenaTargetPoints[pointIndex];
							point.IsVisible = true;
						}
						else
						{
							point = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
							point.SizeOffset_X = 2;
							point.SizeOffset_Y = 2;
							point.TintColor = new Color(1, 1, 0, 1);
							arenaTargetPoints.Add(point);
							mapArenaContainer.AddChild(point);
						}

						point.PositionScale_X = mapPoint.x;
						point.PositionScale_Y = mapPoint.y;
					}
				}
			}

			for (int index = arenaTargetPoints.Count - 1; index >= pointCount; --index)
			{
				arenaTargetPoints[index].IsVisible = false;
			}

			Vector2 arenaCenterMapPosition = ProjectWorldPositionToMap(LevelManager.arenaCurrentCenter);
			float levelSize = Level.size - (Level.border * 2.0f);
			float arenaMapRadius = LevelManager.arenaCurrentRadius / levelSize;
			float arenaMapDiameter = arenaMapRadius * 2.0f;

			arenaAreaCurrentOverlay.PositionScale_X = arenaCenterMapPosition.x - arenaMapRadius;
			arenaAreaCurrentOverlay.PositionScale_Y = arenaCenterMapPosition.y - arenaMapRadius;
			arenaAreaCurrentOverlay.SizeScale_X = arenaMapDiameter;
			arenaAreaCurrentOverlay.SizeScale_Y = arenaMapDiameter;

			arenaAreaCurrentLeftOverlay.PositionScale_Y = arenaAreaCurrentOverlay.PositionScale_Y;
			arenaAreaCurrentLeftOverlay.SizeScale_X = arenaAreaCurrentOverlay.PositionScale_X;
			arenaAreaCurrentLeftOverlay.SizeScale_Y = arenaAreaCurrentOverlay.SizeScale_Y;

			arenaAreaCurrentRightOverlay.PositionScale_X = arenaAreaCurrentOverlay.PositionScale_X + arenaAreaCurrentOverlay.SizeScale_X;
			arenaAreaCurrentRightOverlay.PositionScale_Y = arenaAreaCurrentOverlay.PositionScale_Y;
			arenaAreaCurrentRightOverlay.SizeScale_X = 1.0f - arenaAreaCurrentOverlay.PositionScale_X - arenaAreaCurrentOverlay.SizeScale_X;
			arenaAreaCurrentRightOverlay.SizeScale_Y = arenaAreaCurrentOverlay.SizeScale_Y;

			arenaAreaCurrentUpOverlay.SizeScale_Y = arenaAreaCurrentOverlay.PositionScale_Y;

			arenaAreaCurrentDownOverlay.PositionScale_Y = arenaAreaCurrentOverlay.PositionScale_Y + arenaAreaCurrentOverlay.SizeScale_Y;
			arenaAreaCurrentDownOverlay.SizeScale_Y = 1.0f - arenaAreaCurrentOverlay.PositionScale_Y - arenaAreaCurrentOverlay.SizeScale_Y;
		}

		private static void updateRemotePlayerAvatars()
		{
			int visibleAvatarCount = 0;
			bool isLocalPlayerSpectating = Player.LocalPlayer.look.areSpecStatsVisible;
			foreach (SteamPlayer player in Provider.clients)
			{
				if (player.model == null)
					continue;

				if (player.playerID.steamID == Provider.client)
					continue; // We show a directional arrow for our local client instead.

				bool inSameGroupAsLocalPlayer = player.player.quests.isMemberOfSameGroupAs(Player.LocalPlayer);
				if (!(isLocalPlayerSpectating || inSameGroupAsLocalPlayer))
					continue;

				ISleekImage avatarImage;
				if (visibleAvatarCount < remotePlayerImages.Count)
				{
					avatarImage = remotePlayerImages[visibleAvatarCount];
					avatarImage.IsVisible = true;
				}
				else
				{
					avatarImage = Glazier.Get().CreateImage();
					avatarImage.PositionOffset_X = -10;
					avatarImage.PositionOffset_Y = -10;
					avatarImage.SizeOffset_X = 20;
					avatarImage.SizeOffset_Y = 20;
					avatarImage.AddLabel(string.Empty, ESleekSide.RIGHT);
					mapRemotePlayersContainer.AddChild(avatarImage);
					remotePlayerImages.Add(avatarImage);
				}
				++visibleAvatarCount;

				Vector2 mapPosition = ProjectWorldPositionToMap(player.player.transform.position);
				avatarImage.PositionScale_X = mapPosition.x;
				avatarImage.PositionScale_Y = mapPosition.y;

				if (OptionsSettings.ShouldAnonymizeMultiplayerDetails || !showPlayerAvatarsToggle.Value)
				{
					avatarImage.Texture = icons.load<Texture2D>("RemotePlayer");
					avatarImage.TintColor = player.markerColor;
					avatarImage.SizeOffset_X = 4;
					avatarImage.SizeOffset_Y = 4;
				}
				else
				{
					avatarImage.Texture = Provider.provider.communityService.getIcon(player.playerID.steamID, true);
					avatarImage.TintColor = Color.white;
					avatarImage.SizeOffset_X = 20;
					avatarImage.SizeOffset_Y = 20;
				}
				avatarImage.PositionOffset_X = avatarImage.SizeOffset_X / -2;
				avatarImage.PositionOffset_Y = avatarImage.SizeOffset_Y / -2;

				if (showPlayerNamesToggle.Value)
				{
					// Name text can change when switching spectator mode or groups, so update here.
					if (inSameGroupAsLocalPlayer && !string.IsNullOrEmpty(player.playerID.nickName))
					{
						avatarImage.UpdateLabel(player.playerID.nickName);
					}
					else
					{
						avatarImage.UpdateLabel(player.playerID.characterName);
					}
				}
				else
				{
					avatarImage.UpdateLabel(string.Empty);
				}
			}

			for (int index = remotePlayerImages.Count - 1; index >= visibleAvatarCount; --index)
			{
				remotePlayerImages[index].IsVisible = false;
			}
		}

		public static void updateDynamicMap()
		{
			if (mapMarkersContainer.IsVisible)
			{
				updateMarkers();
			}

			if (mapArenaContainer.IsVisible)
			{
				updateArenaCircle();
			}

			if (mapRemotePlayersContainer.IsVisible)
			{
				updateRemotePlayerAvatars();
			}

			if (localPlayerImage.IsVisible && Player.LocalPlayer != null)
			{
				Vector2 mapPosition = ProjectWorldPositionToMap(Player.LocalPlayer.transform.position);
				localPlayerImage.PositionScale_X = mapPosition.x;
				localPlayerImage.PositionScale_Y = mapPosition.y;
				localPlayerImage.RotationAngle = ProjectWorldRotationToMap(Player.LocalPlayer.transform.rotation.eulerAngles.y);
			}
		}

		protected static void searchForMapsInInventory(ref bool enableChart, ref bool enableMap)
		{
			if (enableChart && enableMap) // We already have both, so there's no purpose searching.
				return;

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
					if (map != null)
					{
						enableChart |= map.enablesChart;
						enableMap |= map.enablesMap;
					}

					if (enableChart && enableMap) // We found one of each, so our search is complete.
						return;
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

			hasChart = Provider.modeConfigData.Gameplay.Chart || Level.info.type != ELevelType.SURVIVAL;
			hasGPS = Provider.modeConfigData.Gameplay.Satellite || Level.info.type != ELevelType.SURVIVAL;
			searchForMapsInInventory(ref hasChart, ref hasGPS);

			if (hasChart && !hasGPS)
			{
				mapButtonState.state = 0;
			}

			if (hasGPS && !hasChart)
			{
				mapButtonState.state = 1;
			}

			synchronizeMapVisibility(mapButtonState.state);
			updateDynamicMap();

			RefreshQuestsButtonLabel();

			if (OptionsSettings.ShouldAnonymizeMultiplayerDetails)
			{
				playersButton.text = localization.format("Streamer");
			}
			else
			{
				playersButton.text = localization.format("Players", Provider.clients.Count, Provider.maxPlayers);
			}

			switch (tab)
			{
				case EInfoTab.GROUPS:
					openGroups();
					break;

				case EInfoTab.QUESTS:
					openQuests();
					break;

				case EInfoTab.PLAYERS:
					openPlayers();
					break;
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

			container.AnimateOutOfView(0, 1);
		}

		private static List<PlayerQuest> displayedQuests = new List<PlayerQuest>();

		private static void RefreshQuestsButtonLabel()
		{
			questsButton.text = localization.format("Quests", Player.LocalPlayer.quests.countValidQuests());
		}

		private static void RefreshQuests()
		{
			questsBox.RemoveAllChildren();

			displayedQuests.Clear();
			float verticalOffset = 0;
			foreach (PlayerQuest quest in Player.LocalPlayer.quests.questsList)
			{
				if (quest == null || quest.asset == null)
					continue;

				displayedQuests.Add(quest);
				bool isComplete = quest.asset.areConditionsMet(Player.LocalPlayer);

				ISleekButton questButton = Glazier.Get().CreateButton();
				questButton.PositionOffset_Y = verticalOffset;
				questButton.SizeOffset_Y = 50;
				questButton.SizeScale_X = 1;
				questButton.OnClicked += onClickedQuestButton;
				questsBox.AddChild(questButton);

				ISleekImage iconImage = Glazier.Get().CreateImage(icons.load<Texture2D>(isComplete ? "Complete" : "Incomplete"));
				iconImage.PositionOffset_X = 5;
				iconImage.PositionOffset_Y = 5;
				iconImage.SizeOffset_X = 40;
				iconImage.SizeOffset_Y = 40;
				questButton.AddChild(iconImage);

				ISleekLabel nameLabel = Glazier.Get().CreateLabel();
				nameLabel.PositionOffset_X = 50;
				nameLabel.SizeOffset_X = -55;
				nameLabel.SizeScale_X = 1;
				nameLabel.SizeScale_Y = 1;
				nameLabel.TextAlignment = TextAnchor.MiddleLeft;
				nameLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
				nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				nameLabel.AllowRichText = true;
				nameLabel.FontSize = ESleekFontSize.Medium;
				nameLabel.Text = quest.asset.questName;
				questButton.AddChild(nameLabel);

				verticalOffset += questButton.SizeOffset_Y + 10;
			}

			questsBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10);
		}

		public static void openQuests()
		{
			tab = EInfoTab.QUESTS;
			RefreshQuests();
			updateTabs();
		}

		private static void onClickedTuneButton(ISleekElement button)
		{
			uint newRadioFrequency = (uint) (radioFrequencyField.Value * 1000);
			if (newRadioFrequency < 300000)
			{
				newRadioFrequency = 300000;
			}
			else if (newRadioFrequency > 900000)
			{
				newRadioFrequency = 900000;
			}
			radioFrequencyField.Value = newRadioFrequency / 1000.0;
			Player.LocalPlayer.quests.sendSetRadioFrequency(newRadioFrequency);
		}

		private static void onClickedResetButton(ISleekElement button)
		{
			radioFrequencyField.Value = PlayerQuests.DEFAULT_RADIO_FREQUENCY / 1000.0;
			onClickedTuneButton(button);
		}

		private static void onClickedRenameButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.sendRenameGroup(groupNameField.Text);
		}

		private static void onClickedMainGroupButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.SendAcceptGroupInvitation(Characters.active.group);
		}

		private static void onClickedLeaveGroupButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.sendLeaveGroup();
		}

		private static void onClickedDeleteGroupButton(SleekButtonIconConfirm button)
		{
			Player.LocalPlayer.quests.sendDeleteGroup();
		}

		private static void onClickedCreateGroupButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.sendCreateGroup();
		}

		private static void refreshGroups()
		{
			if (!active)
			{
				return;
			}

			groupsBox.RemoveAllChildren();

			int offset = 0;

			ISleekBox tuneLabel = Glazier.Get().CreateBox();
			tuneLabel.PositionOffset_Y = offset;
			tuneLabel.SizeOffset_X = 125;
			tuneLabel.SizeOffset_Y = 30;
			tuneLabel.Text = localization.format("Radio_Frequency_Label");
			groupsBox.AddChild(tuneLabel);

			radioFrequencyField = Glazier.Get().CreateFloat64Field();
			radioFrequencyField.PositionOffset_X = 125;
			radioFrequencyField.SizeOffset_X = -225;
			radioFrequencyField.PositionOffset_Y = offset;
			radioFrequencyField.SizeOffset_Y = 30;
			radioFrequencyField.SizeScale_X = 1;
			radioFrequencyField.Value = Player.LocalPlayer.quests.radioFrequency / 1000.0;
			groupsBox.AddChild(radioFrequencyField);

			ISleekButton tuneButton = Glazier.Get().CreateButton();
			tuneButton.PositionOffset_X = -100;
			tuneButton.PositionScale_X = 1;
			tuneButton.SizeOffset_X = 50;
			tuneButton.SizeOffset_Y = 30;
			tuneButton.Text = localization.format("Radio_Frequency_Tune");
			tuneButton.TooltipText = localization.format("Radio_Frequency_Tune_Tooltip");
			tuneButton.OnClicked += onClickedTuneButton;
			groupsBox.AddChild(tuneButton);

			ISleekButton resetButton = Glazier.Get().CreateButton();
			resetButton.PositionOffset_X = -50;
			resetButton.PositionScale_X = 1;
			resetButton.SizeOffset_X = 50;
			resetButton.SizeOffset_Y = 30;
			resetButton.Text = localization.format("Radio_Frequency_Reset");
			resetButton.TooltipText = localization.format("Radio_Frequency_Reset_Tooltip");
			resetButton.OnClicked += onClickedResetButton;
			groupsBox.AddChild(resetButton);

			offset += 30;

			PlayerQuests quests = Player.LocalPlayer.quests;
			if (quests.isMemberOfAGroup)
			{
				if (Characters.active.@group == quests.groupID)
				{
					SteamGroup group = Provider.provider.communityService.getCachedGroup(Characters.active.group);
					if (group != null)
					{
						SleekBoxIcon mainGroupBox = new SleekBoxIcon(group.icon, 40);
						mainGroupBox.PositionOffset_Y = offset;
						mainGroupBox.SizeOffset_Y = 50;
						mainGroupBox.SizeScale_X = 1;
						mainGroupBox.text = group.name;
						groupsBox.AddChild(mainGroupBox);
						offset += 50;
					}
				}
				else
				{
					GroupInfo group = GroupManager.getGroupInfo(quests.groupID);
					string name = group != null ? group.name : quests.groupID.ToString();

					if (quests.groupRank == EPlayerGroupRank.OWNER)
					{
						groupNameField = Glazier.Get().CreateStringField();
						groupNameField.PositionOffset_Y = offset;
						groupNameField.MaxLength = 32;
						groupNameField.Text = name;
						groupNameField.SizeOffset_X = -100;
						groupNameField.SizeOffset_Y = 30;
						groupNameField.SizeScale_X = 1;
						groupsBox.AddChild(groupNameField);

						ISleekButton renameButton = Glazier.Get().CreateButton();
						renameButton.PositionScale_X = 1;
						renameButton.PositionOffset_X = -100;
						renameButton.PositionOffset_Y = offset;
						renameButton.SizeOffset_X = 100;
						renameButton.SizeOffset_Y = 30;
						renameButton.Text = localization.format("Group_Rename");
						renameButton.TooltipText = localization.format("Group_Rename_Tooltip");
						renameButton.OnClicked += onClickedRenameButton;
						groupsBox.AddChild(renameButton);
					}
					else
					{
						ISleekBox nameBox = Glazier.Get().CreateBox();
						nameBox.PositionOffset_Y = offset;
						nameBox.SizeOffset_Y = 30;
						nameBox.SizeScale_X = 1;
						nameBox.Text = name;
						groupsBox.AddChild(nameBox);
					}
					offset += 30;

					if (quests.useMaxGroupMembersLimit)
					{
						ISleekBox membersBox = Glazier.Get().CreateBox();
						membersBox.PositionOffset_Y = offset;
						membersBox.SizeOffset_Y = 30;
						membersBox.SizeScale_X = 1;
						membersBox.Text = localization.format("Group_Members", group.members, Provider.modeConfigData.Gameplay.Max_Group_Members);
						groupsBox.AddChild(membersBox);
						offset += 30;
					}
				}

				if (quests.hasPermissionToLeaveGroup)
				{
					SleekButtonIcon leaveButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
					leaveButton.PositionOffset_Y = offset;
					leaveButton.SizeOffset_Y = 30;
					leaveButton.SizeScale_X = 1;
					leaveButton.text = localization.format("Group_Leave");
					leaveButton.tooltip = localization.format("Group_Leave_Tooltip");
					leaveButton.onClickedButton += onClickedLeaveGroupButton;
					groupsBox.AddChild(leaveButton);
					offset += 30;
				}

				if (quests.hasPermissionToDeleteGroup)
				{
					SleekButtonIconConfirm deleteButton = new SleekButtonIconConfirm(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"),
																						localization.format("Group_Delete_Confirm"),
																						localization.format("Group_Delete_Confirm_Tooltip"),
																						localization.format("Group_Delete_Deny"),
																						localization.format("Group_Delete_Deny_Tooltip"));
					deleteButton.PositionOffset_Y = offset;
					deleteButton.SizeOffset_Y = 30;
					deleteButton.SizeScale_X = 1;
					deleteButton.text = localization.format("Group_Delete");
					deleteButton.tooltip = localization.format("Group_Delete_Tooltip");
					deleteButton.onConfirmed += onClickedDeleteGroupButton;
					groupsBox.AddChild(deleteButton);
					offset += 30;
				}

				foreach (SteamPlayer player in Provider.clients)
				{
					if (player.player == null || !player.player.quests.isMemberOfSameGroupAs(Player.LocalPlayer))
					{
						continue;
					}

					SleekPlayer playerButton = new SleekPlayer(player, true, SleekPlayer.ESleekPlayerDisplayContext.GROUP_ROSTER);
					playerButton.PositionOffset_Y = offset;
					playerButton.SizeOffset_Y = 50;
					playerButton.SizeScale_X = 1;
					groupsBox.AddChild(playerButton);
					offset += 50;
				}
			}
			else
			{
				if (Characters.active.group != CSteamID.Nil && Provider.modeConfigData.Gameplay.Allow_Static_Groups)
				{
					SteamGroup group = Provider.provider.communityService.getCachedGroup(Characters.active.group);
					if (group != null)
					{
						SleekButtonIcon mainGroupButton = new SleekButtonIcon(group.icon, 40);
						mainGroupButton.PositionOffset_Y = offset;
						mainGroupButton.SizeOffset_Y = 50;
						mainGroupButton.SizeScale_X = 1;
						mainGroupButton.text = group.name;
						mainGroupButton.onClickedButton += onClickedMainGroupButton;
						groupsBox.AddChild(mainGroupButton);
						offset += 50;
					}
				}

				foreach (CSteamID invite in quests.groupInvites)
				{
					SleekInviteButton inviteButton = new SleekInviteButton(invite);
					inviteButton.PositionOffset_Y = offset;
					inviteButton.SizeOffset_Y = 30;
					inviteButton.SizeScale_X = 1;
					groupsBox.AddChild(inviteButton);
					offset += 30;
				}

				if (Player.LocalPlayer.quests.hasPermissionToCreateGroup)
				{
					SleekButtonIcon createButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
					createButton.PositionOffset_Y = offset;
					createButton.SizeOffset_Y = 30;
					createButton.SizeScale_X = 1;
					createButton.text = localization.format("Group_Create");
					createButton.tooltip = localization.format("Group_Create_Tooltip");
					createButton.onClickedButton += onClickedCreateGroupButton;
					groupsBox.AddChild(createButton);
					offset += 30;
				}
			}

			groupsBox.ContentSizeOffset = new Vector2(0.0f, offset);
		}

		//private static void handleGroupIDChanged(PlayerQuests quests, CSteamID oldGroupID, CSteamID newGroupID)
		//{
		//	refreshGroups();
		//}

		//private static void handleGroupRankChanged(PlayerQuests quests, EPlayerGroupRank oldGroupRank, EPlayerGroupRank newGroupRank)
		//{
		//	refreshGroups();
		//}

		//private static void handleGroupInvitesChanged(PlayerQuests quests)
		//{
		//	if(quests.isMemberOfAGroup)
		//	{
		//		return; // Invites aren't shown while in a group so don't bother refreshing
		//	}

		//	refreshGroups();
		//}

		private static void handleGroupUpdated(PlayerQuests sender)
		{
			refreshGroups();
		}

		private static void handleGroupInfoReady(GroupInfo group)
		{
			refreshGroups();
		}

		private static void OnLocalPlayerQuestsChanged(ushort legacyId)
		{
			if (active)
			{
				RefreshQuestsButtonLabel();
				if (tab == EInfoTab.QUESTS)
				{
					RefreshQuests();
				}
			}
		}

		public static void openGroups()
		{
			tab = EInfoTab.GROUPS;
			refreshGroups();
			updateTabs();
		}

		public static void openPlayers()
		{
			tab = EInfoTab.PLAYERS;
			SortAndRebuildPlayers();
			updateTabs();
		}

		private static void SortAndRebuildPlayers()
		{
			sortedClients.Clear();
			sortedClients.AddRange(Provider.clients);

			int sortMode;
			if (Provider.modeConfigData.Gameplay.Group_Player_List)
			{
				sortMode = playerSortButton.state;
			}
			else
			{
				// If group connections are off we only allow name sort.
				sortMode = 0;
			}

			if (sortMode == 0) // by name
			{
				playersList.onCreateElement = OnCreatePlayerEntry;
				sortedClients.Sort((SteamPlayer lhs, SteamPlayer rhs) =>
				{
					return lhs.GetLocalDisplayName().CompareTo(rhs.GetLocalDisplayName());
				});
			}
			else // by group
			{
				playersList.onCreateElement = OnCreatePlayerEntryWithGrouping;
				sortedClients.Sort((SteamPlayer lhs, SteamPlayer rhs) =>
				{
					int groupComparison = lhs.player.quests.groupID.CompareTo(rhs.player.quests.groupID);
					if (groupComparison != 0)
					{
						return groupComparison;
					}
					else
					{
						// Fallback to sorting by name within group.
						return lhs.GetLocalDisplayName().CompareTo(rhs.GetLocalDisplayName());
					}
				});
			}

			// ForceRebuildElements rather than NotifyDataChanged because SleekPlayer does not update itself.
			playersList.ForceRebuildElements();
		}

		private static void updateTabs()
		{
			questsBox.IsVisible = tab == EInfoTab.QUESTS;
			groupsBox.IsVisible = tab == EInfoTab.GROUPS;
			playersBox.IsVisible = tab == EInfoTab.PLAYERS;
		}

		private static void updateZoom()
		{
			mapBox.ContentScaleFactor = zoomMultiplier;
		}

		public static void focusPoint(Vector3 point)
		{
			Vector2 mapPosition = ProjectWorldPositionToMap(point);
			mapBox.NormalizedStateCenter = mapPosition;
		}

		private static float ProjectWorldRotationToMap(float yaw)
		{
			CartographyVolume cartographyVolume = CartographyVolumeManager.Get().GetMainVolume();
			if (cartographyVolume != null)
			{
				return yaw - cartographyVolume.transform.eulerAngles.y;
			}
			else
			{
				return yaw;
			}
		}

		/// <summary>
		/// Convert level-space 3D position into normalized 2D position.
		/// </summary>
		private static Vector2 ProjectWorldPositionToMap(Vector3 worldPosition)
		{
			CartographyVolume cartographyVolume = CartographyVolumeManager.Get().GetMainVolume();
			if (cartographyVolume != null)
			{
				Vector3 localPosition = cartographyVolume.transform.InverseTransformPoint(worldPosition);
				return new Vector2(localPosition.x + 0.5f, 0.5f - localPosition.z);
			}
			else
			{
				float levelSize = Level.size - (Level.border * 2.0f);
				Vector2 mapPosition = new Vector2((worldPosition.x / levelSize) + 0.5f,
					0.5f - (worldPosition.z / levelSize));
				return mapPosition;
			}
		}

		/// <summary>
		/// Convert normalized 2D position into level-space 3D position.
		/// </summary>
		private static Vector3 DeprojectMapToWorld(Vector2 mapPosition)
		{
			CartographyVolume cartographyVolume = CartographyVolumeManager.Get().GetMainVolume();
			if (cartographyVolume != null)
			{
				Vector3 localPosition = new Vector3(mapPosition.x - 0.5f, 0.0f, 0.5f - mapPosition.y);
				Vector3 worldPosition = cartographyVolume.transform.TransformPoint(localPosition);
				return worldPosition.GetHorizontal();
			}
			else
			{
				float levelSize = Level.size - (Level.border * 2.0f);
				Vector3 worldPosition = new Vector3((mapPosition.x - 0.5f) * levelSize,
					0.0f,
					(0.5f - mapPosition.y) * levelSize);
				return worldPosition;
			}
		}

		private static void onRightClickedMap()
		{
			bool newIsMarkerPlaced;
			Vector2 newMarkerMapPosition = mapImage.GetNormalizedCursorPosition();
			Vector3 newMarkerLevelPosition = DeprojectMapToWorld(newMarkerMapPosition);

			PlayerQuests quests = Player.LocalPlayer.quests;
			if (quests.isMarkerPlaced)
			{
				Vector2 oldMarkerMapPosition = ProjectWorldPositionToMap(quests.markerPosition);

				float distance = Vector2.Distance(oldMarkerMapPosition, newMarkerMapPosition);
				distance *= mapBox.ContentSizeOffset.x;

				newIsMarkerPlaced = distance > 15;
			}
			else
			{
				newIsMarkerPlaced = true;
			}

			quests.sendSetMarker(newIsMarkerPlaced, newMarkerLevelPosition);
		}

		private static void OnShowMarkersToggled(ISleekToggle toggle, bool value)
		{
			synchronizeMapVisibility(mapButtonState.state);
			updateDynamicMap();
		}

		private static void onClickedZoomInButton(ISleekElement button)
		{
			if (zoomMultiplier < maxZoomMultiplier)
			{
				zoomMultiplier++;

				Vector2 normalizedState = mapBox.NormalizedStateCenter;
				updateZoom();
				mapBox.NormalizedStateCenter = normalizedState;
			}
		}

		private static void onClickedZoomOutButton(ISleekElement button)
		{
			if (zoomMultiplier > 1)
			{
				zoomMultiplier--;

				Vector2 normalizedState = mapBox.NormalizedStateCenter;
				updateZoom();
				mapBox.NormalizedStateCenter = normalizedState;
			}
		}

		private static void onClickedCenterButton(ISleekElement button)
		{
			focusPoint(Player.LocalPlayer.transform.position);
		}

		private static void onSwappedMapState(SleekButtonState button, int index)
		{
			synchronizeMapVisibility(index);
			updateDynamicMap();
		}

		private static void onClickedQuestButton(ISleekElement button)
		{
			int index = questsBox.FindIndexOfChild(button);
			if (index < 0 || index >= displayedQuests.Count)
			{
				UnturnedLog.warn("Cannot find clicked quest");
				return;
			}

			PlayerQuest quest = displayedQuests[index];

			PlayerDashboardUI.close();
			PlayerNPCQuestUI.open(quest.asset, null, null, null, EQuestViewMode.DETAILS);
		}

		private static void onClickedQuestsButton(ISleekElement button)
		{
			openQuests();
		}

		private static void onClickedGroupsButton(ISleekElement button)
		{
			openGroups();
		}

		private static void onClickedPlayersButton(ISleekElement button)
		{
			openPlayers();
		}

		private static void handleIsBlindfoldedChanged()
		{
			if (active)
			{
				synchronizeMapVisibility(mapButtonState.state);
				updateDynamicMap();
			}
		}

		private static void onPlayerTeleported(Player player, Vector3 point)
		{
			focusPoint(point);
		}

		private void createLocationNameLabels()
		{
			Local mapLocalization = Level.info?.getLocalization();

			// regardless of mode we can still see names of locs
			foreach (LocationDevkitNode node in LocationDevkitNodeSystem.Get().GetAllNodes())
			{
				if (!node.isVisibleOnMap)
				{
					// 2023-02-16: added to replace empty localization name workaround because
					// that seems to have broken at some point when English fallbacks were added.
					continue;
				}

				string nodeName = node.locationName;
				if (string.IsNullOrWhiteSpace(nodeName))
				{
					// This was likely a misclick in the map editor, misplacing an empty location.
					continue;
				}

				string localizationKey = nodeName.Replace(' ', '_');
				if (mapLocalization != null && mapLocalization.has(localizationKey))
				{
					nodeName = mapLocalization.format(localizationKey);
				}

				Vector2 mapPosition = ProjectWorldPositionToMap(node.transform.position);
				ISleekLabel location = Glazier.Get().CreateLabel();
				location.PositionOffset_X = -200;
				location.PositionOffset_Y = -30;
				location.PositionScale_X = mapPosition.x;
				location.PositionScale_Y = mapPosition.y;
				location.SizeOffset_X = 400;
				location.SizeOffset_Y = 60;
				location.Text = nodeName;
				location.TextColor = ESleekTint.FONT;
				location.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				mapLocationsContainer.AddChild(location);
			}
		}

		private void OnSwappedPlayerSortState(SleekButtonState state, int index)
		{
			ConvenientSavedata.get().write(playerListSortKey, index);
			SortAndRebuildPlayers();
		}

		private static ISleekElement OnCreatePlayerEntry(SteamPlayer player)
		{
			return new SleekPlayer(player, true, SleekPlayer.ESleekPlayerDisplayContext.PLAYER_LIST);
		}

		private static ISleekElement OnCreatePlayerEntryWithGrouping(SteamPlayer player)
		{
			SleekPlayer element = new SleekPlayer(player, true, SleekPlayer.ESleekPlayerDisplayContext.PLAYER_LIST);

			int index = playersList.IndexOfCreateElementItem; // Hacky...
			int nextIndex = index + 1;
			if (nextIndex < sortedClients.Count && player.player.quests.isMemberOfSameGroupAs(sortedClients[nextIndex].player))
			{
				ISleekImage group = Glazier.Get().CreateImage(icons.load<Texture2D>("Group"));
				group.PositionOffset_X = 21;
				group.PositionOffset_Y = 47;
				group.SizeOffset_X = 8;
				group.SizeOffset_Y = 16;
				group.TintColor = ESleekTint.FOREGROUND;
				element.AddChild(group);
			}

			return element;
		}

		private static void OnGameplayConfigReceived()
		{
			SyncPlayerSortButtonVisible();
		}

		private static void SyncPlayerSortButtonVisible()
		{
			bool showSort = Provider.modeConfigData.Gameplay.Group_Player_List;
			playerSortButton.IsVisible = showSort;
			playersList.PositionOffset_Y = showSort ? 30 : 0;
			playersList.SizeOffset_Y = showSort ? -30 : 0;
		}

		/// <summary>
		/// Temporary to unbind events because this class is static for now. (sigh)
		/// </summary>
		public void OnDestroy()
		{
			PlayerUI.isBlindfoldedChanged -= handleIsBlindfoldedChanged;

			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.onPlayerTeleported -= onPlayerTeleported;
				Player.LocalPlayer.quests.OnLocalPlayerQuestsChanged -= OnLocalPlayerQuestsChanged;
			}

			PlayerQuests.groupUpdated -= handleGroupUpdated;
			GroupManager.groupInfoReady -= handleGroupInfoReady;

			ClientMessageHandler_Accepted.OnGameplayConfigReceived -= OnGameplayConfigReceived;
		}

		public PlayerDashboardInformationUI()
		{
			localization = Localization.read("/Player/PlayerDashboardInformation.dat");
			icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerDashboardInformation");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;
			zoomMultiplier = 1;
			tab = EInfoTab.PLAYERS;

			backdropBox = Glazier.Get().CreateBox();
			backdropBox.PositionOffset_Y = 60;
			backdropBox.SizeOffset_Y = -60;
			backdropBox.SizeScale_X = 1;
			backdropBox.SizeScale_Y = 1;
			backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
			container.AddChild(backdropBox);

			mapInspect = Glazier.Get().CreateFrame();
			mapInspect.PositionOffset_X = 10;
			mapInspect.PositionOffset_Y = 10;
			mapInspect.SizeOffset_X = -15;
			mapInspect.SizeOffset_Y = -20;
			mapInspect.SizeScale_X = 0.6f;
			mapInspect.SizeScale_Y = 1f;
			backdropBox.AddChild(mapInspect);

			ISleekConstraintFrame scrollConstraint = Glazier.Get().CreateConstraintFrame();
			scrollConstraint.SizeOffset_Y = -80;
			scrollConstraint.SizeScale_X = 1;
			scrollConstraint.SizeScale_Y = 1;
			scrollConstraint.Constraint = ESleekConstraint.FitInParent;
			mapInspect.AddChild(scrollConstraint);

			CartographyVolume cartographyVolume = CartographyVolumeManager.Get().GetMainVolume();
			if (cartographyVolume != null)
			{
				Bounds localBounds = cartographyVolume.CalculateLocalBounds();
				Vector3 boundsSize = localBounds.size;
				scrollConstraint.AspectRatio = boundsSize.x / boundsSize.z;

				maxZoomMultiplier = Mathf.CeilToInt(Mathf.Max(boundsSize.x, boundsSize.z) / 1024.0f) + 1;
			}
			else
			{
				maxZoomMultiplier = (Level.size / 1024) + 1;
			}

			mapBox = Glazier.Get().CreateScrollView();
			mapBox.SizeScale_X = 1;
			mapBox.SizeScale_Y = 1;
			mapBox.HandleScrollWheel = false;
			mapBox.ScaleContentToWidth = true;
			mapBox.ScaleContentToHeight = true;
			scrollConstraint.AddChild(mapBox);

			mapImage = Glazier.Get().CreateImage();
			mapImage.SizeScale_X = 1.0f;
			mapImage.SizeScale_Y = 1.0f;
			mapImage.OnRightClicked += onRightClickedMap;
			mapBox.AddChild(mapImage);

			mapLocationsContainer = Glazier.Get().CreateFrame();
			mapLocationsContainer.SizeScale_X = 1;
			mapLocationsContainer.SizeScale_Y = 1;
			mapImage.AddChild(mapLocationsContainer);

			createLocationNameLabels();

			mapArenaContainer = Glazier.Get().CreateFrame();
			mapArenaContainer.SizeScale_X = 1;
			mapArenaContainer.SizeScale_Y = 1;
			mapImage.AddChild(mapArenaContainer);

			mapMarkersContainer = Glazier.Get().CreateFrame();
			mapMarkersContainer.SizeScale_X = 1;
			mapMarkersContainer.SizeScale_Y = 1;
			mapImage.AddChild(mapMarkersContainer);

			mapRemotePlayersContainer = Glazier.Get().CreateFrame();
			mapRemotePlayersContainer.SizeScale_X = 1;
			mapRemotePlayersContainer.SizeScale_Y = 1;
			mapImage.AddChild(mapRemotePlayersContainer);

			arenaTargetPoints = new List<ISleekImage>();
			markerImages = new List<ISleekImage>();
			remotePlayerImages = new List<ISleekImage>();

			localPlayerImage = Glazier.Get().CreateImage();
			localPlayerImage.PositionOffset_X = -10;
			localPlayerImage.PositionOffset_Y = -10;
			localPlayerImage.SizeOffset_X = 20;
			localPlayerImage.SizeOffset_Y = 20;
			localPlayerImage.CanRotate = true;
			localPlayerImage.Texture = icons.load<Texture2D>("Player");
			localPlayerImage.TintColor = ESleekTint.FOREGROUND;
			if (string.IsNullOrEmpty(Characters.active.nick))
			{
				localPlayerImage.AddLabel(Characters.active.name, ESleekSide.RIGHT);
			}
			else
			{
				localPlayerImage.AddLabel(Characters.active.nick, ESleekSide.RIGHT);
			}
			mapImage.AddChild(localPlayerImage);

			arenaAreaCurrentOverlay = Glazier.Get().CreateImage(icons.load<Texture2D>("Arena_Area"));
			mapArenaContainer.AddChild(arenaAreaCurrentOverlay);

			arenaAreaCurrentLeftOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			arenaAreaCurrentLeftOverlay.SizeOffset_X = 1;
			mapArenaContainer.AddChild(arenaAreaCurrentLeftOverlay);

			arenaAreaCurrentRightOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			arenaAreaCurrentRightOverlay.PositionOffset_X = -1;
			arenaAreaCurrentRightOverlay.SizeOffset_X = 1;
			mapArenaContainer.AddChild(arenaAreaCurrentRightOverlay);

			arenaAreaCurrentUpOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			arenaAreaCurrentUpOverlay.SizeOffset_Y = 1;
			arenaAreaCurrentUpOverlay.SizeScale_X = 1.0f;
			mapArenaContainer.AddChild(arenaAreaCurrentUpOverlay);

			arenaAreaCurrentDownOverlay = Glazier.Get().CreateImage(GlazierResources.PixelTexture);
			arenaAreaCurrentDownOverlay.PositionOffset_Y = -1;
			arenaAreaCurrentDownOverlay.SizeOffset_Y = 1;
			arenaAreaCurrentDownOverlay.SizeScale_X = 1.0f;
			mapArenaContainer.AddChild(arenaAreaCurrentDownOverlay);

			noLabel = Glazier.Get().CreateLabel();
			noLabel.SizeOffset_Y = -80;
			noLabel.SizeScale_X = 1;
			noLabel.SizeScale_Y = 1;
			noLabel.TextColor = Color.black;
			noLabel.FontSize = ESleekFontSize.Large;
			noLabel.FontStyle = FontStyle.Bold;
			mapInspect.AddChild(noLabel);
			noLabel.IsVisible = false;

			updateZoom();

			showMarkersToggle = Glazier.Get().CreateToggle();
			showMarkersToggle.PositionOffset_Y = -70;
			showMarkersToggle.PositionScale_Y = 1.0f;
			showMarkersToggle.AddLabel(localization.format("ShowMarkersToggle_Label"), ESleekSide.RIGHT);
			showMarkersToggle.TooltipText = localization.format("ShowMarkersToggle_Tooltip");
			showMarkersToggle.Value = true;
			showMarkersToggle.OnValueChanged += OnShowMarkersToggled;
			mapInspect.AddChild(showMarkersToggle);

			showPlayerNamesToggle = Glazier.Get().CreateToggle();
			showPlayerNamesToggle.PositionOffset_Y = -70;
			showPlayerNamesToggle.PositionScale_X = 0.25f;
			showPlayerNamesToggle.PositionScale_Y = 1.0f;
			showPlayerNamesToggle.AddLabel(localization.format("ShowPlayerNamesToggle_Label"), ESleekSide.RIGHT);
			showPlayerNamesToggle.TooltipText = localization.format("ShowPlayerNamesToggle_Tooltip");
			showPlayerNamesToggle.Value = true;
			showPlayerNamesToggle.OnValueChanged += OnShowMarkersToggled;
			mapInspect.AddChild(showPlayerNamesToggle);

			showPlayerAvatarsToggle = Glazier.Get().CreateToggle();
			showPlayerAvatarsToggle.PositionOffset_Y = -70;
			showPlayerAvatarsToggle.PositionScale_X = 0.5f;
			showPlayerAvatarsToggle.PositionScale_Y = 1.0f;
			showPlayerAvatarsToggle.AddLabel(localization.format("ShowPlayerAvatarsToggle_Label"), ESleekSide.RIGHT);
			showPlayerAvatarsToggle.TooltipText = localization.format("ShowPlayerAvatarsToggle_Tooltip");
			showPlayerAvatarsToggle.Value = true;
			showPlayerAvatarsToggle.OnValueChanged += OnShowMarkersToggled;
			mapInspect.AddChild(showPlayerAvatarsToggle);

			zoomInButton = new SleekButtonIcon(icons.load<Texture2D>("Zoom_In"));
			zoomInButton.PositionOffset_Y = -30;
			zoomInButton.PositionScale_Y = 1;
			zoomInButton.SizeOffset_X = -5;
			zoomInButton.SizeOffset_Y = 30;
			zoomInButton.SizeScale_X = 0.25f;
			zoomInButton.text = localization.format("Zoom_In_Button");
			zoomInButton.tooltip = localization.format("Zoom_In_Button_Tooltip");
			zoomInButton.iconColor = ESleekTint.FOREGROUND;
			zoomInButton.onClickedButton += onClickedZoomInButton;
			mapInspect.AddChild(zoomInButton);

			zoomOutButton = new SleekButtonIcon(icons.load<Texture2D>("Zoom_Out"));
			zoomOutButton.PositionOffset_X = 5;
			zoomOutButton.PositionOffset_Y = -30;
			zoomOutButton.PositionScale_X = 0.25f;
			zoomOutButton.PositionScale_Y = 1;
			zoomOutButton.SizeOffset_X = -10;
			zoomOutButton.SizeOffset_Y = 30;
			zoomOutButton.SizeScale_X = 0.25f;
			zoomOutButton.text = localization.format("Zoom_Out_Button");
			zoomOutButton.tooltip = localization.format("Zoom_Out_Button_Tooltip");
			zoomOutButton.iconColor = ESleekTint.FOREGROUND;
			zoomOutButton.onClickedButton += onClickedZoomOutButton;
			mapInspect.AddChild(zoomOutButton);

			centerButton = new SleekButtonIcon(icons.load<Texture2D>("Center"));
			centerButton.PositionOffset_X = 5;
			centerButton.PositionOffset_Y = -30;
			centerButton.PositionScale_X = 0.5f;
			centerButton.PositionScale_Y = 1;
			centerButton.SizeOffset_X = -10;
			centerButton.SizeOffset_Y = 30;
			centerButton.SizeScale_X = 0.25f;
			centerButton.text = localization.format("Center_Button");
			centerButton.tooltip = localization.format("Center_Button_Tooltip");
			centerButton.iconColor = ESleekTint.FOREGROUND;
			centerButton.onClickedButton += onClickedCenterButton;
			mapInspect.AddChild(centerButton);

			mapButtonState = new SleekButtonState(new GUIContent(localization.format("Chart")), new GUIContent(localization.format("Satellite")));
			mapButtonState.PositionOffset_X = 5;
			mapButtonState.PositionOffset_Y = -30;
			mapButtonState.PositionScale_X = 0.75f;
			mapButtonState.PositionScale_Y = 1;
			mapButtonState.SizeOffset_X = -5;
			mapButtonState.SizeOffset_Y = 30;
			mapButtonState.SizeScale_X = 0.25f;
			mapButtonState.onSwappedState = onSwappedMapState;
			mapInspect.AddChild(mapButtonState);

			headerButtonsContainer = Glazier.Get().CreateFrame();
			headerButtonsContainer.PositionOffset_X = 5;
			headerButtonsContainer.PositionOffset_Y = 10;
			headerButtonsContainer.PositionScale_X = 0.6f;
			headerButtonsContainer.SizeOffset_X = -15;
			headerButtonsContainer.SizeOffset_Y = 50;
			headerButtonsContainer.SizeScale_X = 0.4f;
			backdropBox.AddChild(headerButtonsContainer);

			questsButton = new SleekButtonIcon(icons.load<Texture2D>("Quests"));
			questsButton.SizeOffset_X = -5;
			questsButton.SizeScale_X = 0.333f;
			questsButton.SizeScale_Y = 1.0f;
			questsButton.fontSize = ESleekFontSize.Medium;
			questsButton.tooltip = localization.format("Quests_Tooltip");
			questsButton.onClickedButton += onClickedQuestsButton;
			headerButtonsContainer.AddChild(questsButton);

			groupsButton = new SleekButtonIcon(icons.load<Texture2D>("Groups"));
			groupsButton.PositionOffset_X = 5;
			groupsButton.PositionScale_X = 0.333f;
			groupsButton.SizeOffset_X = -10;
			groupsButton.SizeScale_X = 0.334f;
			groupsButton.SizeScale_Y = 1.0f;
			groupsButton.fontSize = ESleekFontSize.Medium;
			groupsButton.text = localization.format("Groups");
			groupsButton.tooltip = localization.format("Groups_Tooltip");
			groupsButton.onClickedButton += onClickedGroupsButton;
			headerButtonsContainer.AddChild(groupsButton);

			playersButton = new SleekButtonIcon(icons.load<Texture2D>("Players"));
			playersButton.PositionOffset_X = 5;
			playersButton.PositionScale_X = 0.667f;
			playersButton.SizeOffset_X = -5;
			playersButton.SizeScale_X = 0.333f;
			playersButton.SizeScale_Y = 1.0f;
			playersButton.fontSize = ESleekFontSize.Medium;
			playersButton.tooltip = localization.format("Players_Tooltip");
			playersButton.onClickedButton += onClickedPlayersButton;
			headerButtonsContainer.AddChild(playersButton);

			questsBox = Glazier.Get().CreateScrollView();
			questsBox.PositionOffset_X = 5;
			questsBox.PositionOffset_Y = 70;
			questsBox.PositionScale_X = 0.6f;
			questsBox.SizeOffset_X = -15;
			questsBox.SizeOffset_Y = -80;
			questsBox.SizeScale_X = 0.4f;
			questsBox.SizeScale_Y = 1;
			questsBox.ScaleContentToWidth = true;
			backdropBox.AddChild(questsBox);
			questsBox.IsVisible = false;

			groupsBox = Glazier.Get().CreateScrollView();
			groupsBox.PositionOffset_X = 5;
			groupsBox.PositionOffset_Y = 70;
			groupsBox.PositionScale_X = 0.6f;
			groupsBox.SizeOffset_X = -15;
			groupsBox.SizeOffset_Y = -80;
			groupsBox.SizeScale_X = 0.4f;
			groupsBox.SizeScale_Y = 1;
			groupsBox.ScaleContentToWidth = true;
			backdropBox.AddChild(groupsBox);
			groupsBox.IsVisible = false;

			playersBox = Glazier.Get().CreateFrame();
			playersBox.PositionOffset_X = 5;
			playersBox.PositionOffset_Y = 70;
			playersBox.PositionScale_X = 0.6f;
			playersBox.SizeOffset_X = -15;
			playersBox.SizeOffset_Y = -80;
			playersBox.SizeScale_X = 0.4f;
			playersBox.SizeScale_Y = 1;
			backdropBox.AddChild(playersBox);
			playersBox.IsVisible = true;

			playerSortButton = new SleekButtonState(new GUIContent(localization.format("SortPlayers_Name")), new GUIContent(localization.format("SortPlayers_Group")));
			playerSortButton.SizeScale_X = 1.0f;
			playerSortButton.SizeOffset_Y = 30;
			playerSortButton.onSwappedState = OnSwappedPlayerSortState;
			long preferredSort;
			if (ConvenientSavedata.get().read(playerListSortKey, out preferredSort))
			{
				playerSortButton.state = MathfEx.ClampLongToInt(preferredSort, 0, 1);
			}
			playersBox.AddChild(playerSortButton);

			playersList = new SleekList<SteamPlayer>();
			playersList.SizeScale_X = 1.0f;
			playersList.SizeScale_Y = 1.0f;
			playersList.itemHeight = 50;
			playersList.itemPadding = 10;
			playersBox.AddChild(playersList);
			sortedClients.Clear();
			playersList.SetData(sortedClients);

			PlayerUI.isBlindfoldedChanged += handleIsBlindfoldedChanged;
			Player.LocalPlayer.onPlayerTeleported += onPlayerTeleported;
			Player.LocalPlayer.quests.OnLocalPlayerQuestsChanged += OnLocalPlayerQuestsChanged;
			PlayerQuests.groupUpdated += handleGroupUpdated;
			GroupManager.groupInfoReady += handleGroupInfoReady;

			ClientMessageHandler_Accepted.OnGameplayConfigReceived += OnGameplayConfigReceived;
			SyncPlayerSortButtonVisible();

			onPlayerTeleported(Player.LocalPlayer, Player.LocalPlayer.transform.position);

			string absoluteChartPath = Level.info != null ? Level.info.path + "/Chart.png" : null;
			if (absoluteChartPath != null && ReadWrite.fileExists(absoluteChartPath, false, false))
			{
				chartTexture = ReadWrite.readTextureFromFile(absoluteChartPath);
			}
			else
			{
				chartTexture = null;
			}

			string absoluteSatellitePath = Level.info != null ? Level.info.path + "/Map.png" : null;
			if (absoluteSatellitePath != null && ReadWrite.fileExists(absoluteSatellitePath, false, false))
			{
				mapTexture = ReadWrite.readTextureFromFile(absoluteSatellitePath);
			}
			else
			{
				mapTexture = null;
			}

			staticTexture = Resources.Load<Texture2D>("Level/Map");
		}

		private const string playerListSortKey = "PlayerListSortMode";
	}
}
