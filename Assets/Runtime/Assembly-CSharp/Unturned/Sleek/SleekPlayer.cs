////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekPlayer : SleekWrapper
	{
		public enum ESleekPlayerDisplayContext
		{
			NONE,
			GROUP_ROSTER,
			PLAYER_LIST
		}

		private ISleekElement box;
		private ISleekImage avatarImage;
		private ISleekImage repImage;
		private ISleekLabel nameLabel;
		private ISleekLabel repLabel;
		//private ISleekLabel nameLabel;
		private ISleekImage icon;
		private ISleekImage voice;
		private ISleekImage skillset;
		private ISleekButton muteVoiceChatButton;
		private ISleekButton muteTextChatButton;

		public SteamPlayer player
		{
			get;
			private set;
		}

		private ESleekPlayerDisplayContext context;

		private void onClickedPlayerButton(ISleekElement button)
		{
			Provider.provider.browserService.open("https://steamcommunity.com/profiles/" + player.playerID.steamID);
		}

		private void OnMuteVoiceChatClicked(ISleekElement button)
		{
#if !DEDICATED_SERVER
			player.SetVoiceChatLocallyMuted(!player.isVoiceChatLocallyMuted);
#endif
			UpdateMuteVoiceChatLabel();
		}

		private void OnMuteTextChatClicked(ISleekElement button)
		{
#if !DEDICATED_SERVER
			player.SetTextChatLocallyMuted(!player.isTextChatLocallyMuted);
#endif
			UpdateMuteTextChatLabel();
		}

		private void onClickedPromoteButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.sendPromote(player.playerID.steamID);
		}

		private void onClickedDemoteButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.sendDemote(player.playerID.steamID);
		}

		private void onClickedKickButton(ISleekElement button)
		{
			if (context == ESleekPlayerDisplayContext.GROUP_ROSTER)
			{
				Player.LocalPlayer.quests.sendKickFromGroup(player.playerID.steamID);
			}
			else if (context == ESleekPlayerDisplayContext.PLAYER_LIST)
			{
				ChatManager.sendCallVote(player.playerID.steamID);

				PlayerDashboardUI.close();
				PlayerLifeUI.open();
			}
		}

		private void onClickedInviteButton(ISleekElement button)
		{
			Player.LocalPlayer.quests.sendAskAddGroupInvite(player.playerID.steamID);
		}

		private void onClickedSpyButton(ISleekElement button)
		{
			ChatManager.sendChat(EChatMode.GLOBAL, "/spy " + player.playerID.steamID);
		}

		private void onTalked(bool isTalking)
		{
			voice.IsVisible = isTalking;
		}

		public override void OnDestroy()
		{
			if (player != null)
			{
				player.player.voice.onTalkingChanged -= onTalked;
			}
		}

		private void UpdateMuteVoiceChatLabel()
		{
			muteVoiceChatButton.Text = player.isVoiceChatLocallyMuted ? PlayerDashboardInformationUI.localization.format("UnmuteVoiceChat_Label") : PlayerDashboardInformationUI.localization.format("MuteVoiceChat_Label");
		}

		private void UpdateMuteTextChatLabel()
		{
			muteTextChatButton.Text = player.isTextChatLocallyMuted ? PlayerDashboardInformationUI.localization.format("UnmuteTextChat_Label") : PlayerDashboardInformationUI.localization.format("MuteTextChat_Label");
		}

		public SleekPlayer(SteamPlayer newPlayer, bool isButton, ESleekPlayerDisplayContext context) : base()
		{
			player = newPlayer;
			this.context = context;

			Texture2D avatar;
			if (OptionsSettings.ShouldAnonymizeMultiplayerDetails)
			{
				avatar = null;
			}
			else
			{
				if (Provider.isServer)
				{
					avatar = Provider.provider.communityService.getIcon(Provider.user);
				}
				else
				{
					avatar = Provider.provider.communityService.getIcon(player.playerID.steamID);
				}
			}

			SleekColor backgroundColor = ESleekTint.BACKGROUND;
			SleekColor textColor = ESleekTint.FOREGROUND;
			if (player.isAdmin && !Provider.isServer)
			{
				backgroundColor = SleekColor.BackgroundIfLight(Palette.ADMIN);
				textColor = Palette.ADMIN;
			}
			else if (player.isPro)
			{
				backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
				textColor = Palette.PRO;
			}

			if (isButton)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.SizeScale_X = 1;
				button.SizeScale_Y = 1;
				button.TooltipText = player.playerID.playerName;
				button.FontSize = ESleekFontSize.Medium;
				button.BackgroundColor = backgroundColor;
				button.TextColor = textColor;
				button.OnClicked += onClickedPlayerButton;
				AddChild(button);

				box = button;
			}
			else
			{
				ISleekBox button = Glazier.Get().CreateBox();
				button.SizeScale_X = 1;
				button.SizeScale_Y = 1;
				button.TooltipText = player.playerID.playerName;
				button.FontSize = ESleekFontSize.Medium;
				button.BackgroundColor = backgroundColor;
				button.TextColor = textColor;
				AddChild(button);

				box = button;
			}

			avatarImage = Glazier.Get().CreateImage();
			avatarImage.PositionOffset_X = 9;
			avatarImage.PositionOffset_Y = 9;
			avatarImage.SizeOffset_X = 32;
			avatarImage.SizeOffset_Y = 32;
			avatarImage.Texture = avatar;
			avatarImage.ShouldDestroyTexture = true;
			box.AddChild(avatarImage);

			if (player.player != null && player.player.skills != null)
			{
				repImage = Glazier.Get().CreateImage();
				repImage.PositionOffset_X = 46;
				repImage.PositionOffset_Y = 9;
				repImage.SizeOffset_X = 32;
				repImage.SizeOffset_Y = 32;
				repImage.Texture = PlayerTool.getRepTexture(player.player.skills.reputation);
				repImage.TintColor = PlayerTool.getRepColor(player.player.skills.reputation);
				box.AddChild(repImage);
			}

			nameLabel = Glazier.Get().CreateLabel();
			nameLabel.PositionOffset_X = 83;
			nameLabel.SizeOffset_X = -113;
			nameLabel.SizeOffset_Y = 30;
			nameLabel.SizeScale_X = 1;
			nameLabel.Text = player.GetLocalDisplayName();
			nameLabel.FontSize = ESleekFontSize.Medium;
			box.AddChild(nameLabel);

			if (player.player != null && player.player.skills != null)
			{
				repLabel = Glazier.Get().CreateLabel();
				repLabel.PositionOffset_X = 83;
				repLabel.PositionOffset_Y = 20;
				repLabel.SizeOffset_X = -113;
				repLabel.SizeOffset_Y = 30;
				repLabel.SizeScale_X = 1;
				repLabel.TextColor = repImage.TintColor;
				repLabel.Text = PlayerTool.getRepTitle(player.player.skills.reputation);
				repLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				box.AddChild(repLabel);
			}

			if (context == ESleekPlayerDisplayContext.GROUP_ROSTER)
			{
				nameLabel.PositionOffset_Y = -5;
				repLabel.PositionOffset_Y = 10;

				ISleekLabel rankLabel = Glazier.Get().CreateLabel();
				rankLabel.PositionOffset_X = 83;
				rankLabel.PositionOffset_Y = 25;
				rankLabel.SizeOffset_X = -113;
				rankLabel.SizeOffset_Y = 30;
				rankLabel.SizeScale_X = 1;
				rankLabel.TextColor = repImage.TintColor;
				rankLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
				box.AddChild(rankLabel);

				switch (player.player.quests.groupRank)
				{
					case EPlayerGroupRank.MEMBER:
						rankLabel.Text = PlayerDashboardInformationUI.localization.format("Group_Rank_Member");
						break;

					case EPlayerGroupRank.ADMIN:
						rankLabel.Text = PlayerDashboardInformationUI.localization.format("Group_Rank_Admin");
						break;

					case EPlayerGroupRank.OWNER:
						rankLabel.Text = PlayerDashboardInformationUI.localization.format("Group_Rank_Owner");
						break;
				}
			}

			voice = Glazier.Get().CreateImage();
			voice.PositionOffset_X = 15;
			voice.PositionOffset_Y = 15;
			voice.SizeOffset_X = 20;
			voice.SizeOffset_Y = 20;
			voice.Texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Voice");
			box.AddChild(voice);

			skillset = Glazier.Get().CreateImage();
			skillset.PositionOffset_X = -25;
			skillset.PositionOffset_Y = 25;
			skillset.PositionScale_X = 1;
			skillset.SizeOffset_X = 20;
			skillset.SizeOffset_Y = 20;
			skillset.Texture = MenuSurvivorsCharacterUI.icons.load<Texture2D>("Skillset_" + (int) player.skillset);
			skillset.TintColor = ESleekTint.FOREGROUND;
			box.AddChild(skillset);

			if (player.isAdmin && !Provider.isServer)
			{
				nameLabel.TextColor = Palette.ADMIN;
				nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;

				icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = -25;
				icon.PositionOffset_Y = 5;
				icon.PositionScale_X = 1;
				icon.SizeOffset_X = 20;
				icon.SizeOffset_Y = 20;
				icon.Texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Admin");
				box.AddChild(icon);
			}
			else if (player.isPro)
			{
				nameLabel.TextColor = Palette.PRO;
				nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;

				icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = -25;
				icon.PositionOffset_Y = 5;
				icon.PositionScale_X = 1;
				icon.SizeOffset_X = 20;
				icon.SizeOffset_Y = 20;
				icon.Texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Pro");
				box.AddChild(icon);
			}

			if (context == ESleekPlayerDisplayContext.GROUP_ROSTER)
			{
				int offset = 0;

				if (!player.player.channel.IsLocalPlayer) // Only show rank and kick buttons for other players
				{
					if (Player.LocalPlayer.quests.hasPermissionToChangeRank)
					{
						if (player.player.quests.groupRank < EPlayerGroupRank.OWNER)
						{
							ISleekButton promoteButton = Glazier.Get().CreateButton();
							promoteButton.PositionOffset_X = offset;
							promoteButton.PositionScale_X = 1.0f;
							promoteButton.SizeOffset_X = 80;
							promoteButton.SizeScale_Y = 1.0f;
							promoteButton.Text = PlayerDashboardInformationUI.localization.format("Group_Promote");
							promoteButton.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Promote_Tooltip");
							promoteButton.OnClicked += onClickedPromoteButton;
							box.AddChild(promoteButton);
							offset += 80;
						}

						if (player.player.quests.groupRank == EPlayerGroupRank.ADMIN) // No case where it makes sense to demote owner
						{
							ISleekButton demoteButton = Glazier.Get().CreateButton();
							demoteButton.PositionOffset_X = offset;
							demoteButton.PositionScale_X = 1.0f;
							demoteButton.SizeOffset_X = 80;
							demoteButton.SizeScale_Y = 1.0f;
							demoteButton.Text = PlayerDashboardInformationUI.localization.format("Group_Demote");
							demoteButton.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Demote_Tooltip");
							demoteButton.OnClicked += onClickedDemoteButton;
							box.AddChild(demoteButton);
							offset += 80;
						}
					}

					if (Player.LocalPlayer.quests.hasPermissionToKickMembers && player.player.quests.canBeKickedFromGroup)
					{
						ISleekButton kickButton = Glazier.Get().CreateButton();
						kickButton.PositionOffset_X = offset;
						kickButton.PositionScale_X = 1.0f;
						kickButton.SizeOffset_X = 50;
						kickButton.SizeScale_Y = 1.0f;
						kickButton.Text = PlayerDashboardInformationUI.localization.format("Group_Kick");
						kickButton.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Kick_Tooltip");
						kickButton.OnClicked += onClickedKickButton;
						box.AddChild(kickButton);
						offset += 50;
					}
				}

				box.SizeOffset_X = -offset;
			}
			else if (context == ESleekPlayerDisplayContext.PLAYER_LIST)
			{
				int offset = 0;

				if (!player.player.channel.IsLocalPlayer)
				{
					muteVoiceChatButton = Glazier.Get().CreateButton();
					muteVoiceChatButton.PositionScale_X = 1.0f;
					muteVoiceChatButton.SizeOffset_X = 100;
					muteVoiceChatButton.SizeScale_Y = 0.5f;
					UpdateMuteVoiceChatLabel();
					muteVoiceChatButton.TooltipText = PlayerDashboardInformationUI.localization.format("Mute_Tooltip");
					muteVoiceChatButton.OnClicked += OnMuteVoiceChatClicked;
					box.AddChild(muteVoiceChatButton);

					muteTextChatButton = Glazier.Get().CreateButton();
					muteTextChatButton.PositionScale_X = 1.0f;
					muteTextChatButton.PositionScale_Y = 0.5f;
					muteTextChatButton.SizeOffset_X = 100;
					muteTextChatButton.SizeScale_Y = 0.5f;
					UpdateMuteTextChatLabel();
					muteTextChatButton.TooltipText = PlayerDashboardInformationUI.localization.format("Mute_Tooltip");
					muteTextChatButton.OnClicked += OnMuteTextChatClicked;
					box.AddChild(muteTextChatButton);

					offset += 100;
				}

				if (!player.player.channel.IsLocalPlayer && !player.isAdmin)
				{
					ISleekButton kickButton = Glazier.Get().CreateButton();
					kickButton.PositionOffset_X = offset;
					kickButton.PositionScale_X = 1.0f;
					kickButton.SizeOffset_X = 50;
					kickButton.SizeScale_Y = 1.0f;
					kickButton.Text = PlayerDashboardInformationUI.localization.format("Vote_Kick");
					kickButton.TooltipText = PlayerDashboardInformationUI.localization.format("Vote_Kick_Tooltip");
					kickButton.OnClicked += onClickedKickButton;
					box.AddChild(kickButton);
					offset += 50;
				}

				if (Player.LocalPlayer != null)
				{
					if (!player.player.channel.IsLocalPlayer)
					{
						if (Player.LocalPlayer.quests.isMemberOfAGroup && Player.LocalPlayer.quests.hasPermissionToInviteMembers && Player.LocalPlayer.quests.hasSpaceForMoreMembersInGroup && !player.player.quests.isMemberOfAGroup)
						{
							ISleekButton inviteButton = Glazier.Get().CreateButton();
							inviteButton.PositionOffset_X = offset;
							inviteButton.PositionScale_X = 1.0f;
							inviteButton.SizeOffset_X = 60;
							inviteButton.SizeScale_Y = 1.0f;
							inviteButton.Text = PlayerDashboardInformationUI.localization.format("Group_Invite");
							inviteButton.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Invite_Tooltip");
							inviteButton.OnClicked += onClickedInviteButton;
							box.AddChild(inviteButton);
							offset += 60;
						}
					}

					if (Player.LocalPlayer.channel.owner.isAdmin)
					{
						ISleekButton spyButton = Glazier.Get().CreateButton();
						spyButton.PositionOffset_X = offset;
						spyButton.PositionScale_X = 1.0f;
						spyButton.SizeOffset_X = 50;
						spyButton.SizeScale_Y = 1.0f;
						spyButton.Text = PlayerDashboardInformationUI.localization.format("Spy");
						spyButton.TooltipText = PlayerDashboardInformationUI.localization.format("Spy_Tooltip");
						spyButton.OnClicked += onClickedSpyButton;
						box.AddChild(spyButton);
						offset += 50;
					}
				}

				box.SizeOffset_X = -offset;
			}

			if (player != null)
			{
				player.player.voice.onTalkingChanged += onTalked;
				onTalked(player.player.voice.isTalking);
			}
		}
	}
}
