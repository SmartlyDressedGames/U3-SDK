////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	public delegate void ChatMessageReceivedHandler();
	public delegate void ServerSendingChatMessageHandler(ref string text,
		ref Color color,
		SteamPlayer fromPlayer,
		SteamPlayer toPlayer,
		EChatMode mode,
		ref string iconURL,
		ref bool useRichTextFormatting);
	public delegate void ServerFormattingChatMessageHandler(SteamPlayer speaker, EChatMode mode, ref string text);

	public delegate void Chatted(SteamPlayer player, EChatMode mode, ref Color chatted, ref bool isRich, string text, ref bool isVisible);
	public delegate void CheckPermissions(SteamPlayer player, string text, ref bool shouldExecuteCommand, ref bool shouldList);
	public delegate void VotingStart(SteamPlayer origin, SteamPlayer target, byte votesNeeded);
	public delegate void VotingUpdate(byte voteYes, byte voteNo);
	public delegate void VotingStop(EVotingMessage message);
	public delegate void VotingMessage(EVotingMessage message);

	[NetEnum]
	public enum EVotingMessage
	{
		OFF, // disabled on server
		DELAY, // cooldown
		PLAYERS, // not enough players
		PASS, // vote passed
		FAIL // vote failed
	}

	public class ChatManager : SteamCaller
	{
		public static readonly int MAX_MESSAGE_LENGTH = 512;

		/// <summary>
		/// Called on the client after a new message is inserted to the front of the list.
		/// </summary>
		public static ChatMessageReceivedHandler onChatMessageReceived;

		/// <summary>
		/// Called on the server when preparing a message to be sent to a player.
		/// Allows controlling how %SPEAKER% is formatted for the receiving player.
		/// </summary>
		public static ServerSendingChatMessageHandler onServerSendingMessage;

		/// <summary>
		/// Called on the server when formatting a player's message before sending to anyone.
		/// Allows structuring the message and where the player's name is, for example: '[CustomPluginRoleThing] %SPEAKER% - OriginalMessageText'
		/// </summary>
		public static ServerFormattingChatMessageHandler onServerFormattingMessage;

		public static Chatted onChatted;
		public static CheckPermissions onCheckPermissions;
		public static VotingStart onVotingStart;
		public static VotingUpdate onVotingUpdate;
		public static VotingStop onVotingStop;
		public static VotingMessage onVotingMessage;

		public delegate void ClientUnityEventPermissionsHandler(SteamPlayer player, string command, ref bool shouldExecuteCommand, ref bool shouldList);
		public static event ClientUnityEventPermissionsHandler onCheckUnityEventPermissions;

		public static string welcomeText = "";
		public static Color welcomeColor = Palette.SERVER;
		public static float chatrate = 0.25f;

		public static bool voteAllowed = false;
		public static float votePassCooldown = 5.0f;
		public static float voteFailCooldown = 60.0f;
		public static float voteDuration = 15.0f;
		public static float votePercentage = 0.75f;
		public static byte votePlayers = 3;

		private static float lastVote;
		private static bool isVoting;
		private static bool needsVote;
		private static bool hasVote;
		private static byte voteYes;
		private static byte voteNo;
		private static byte votesPossible;
		private static byte votesNeeded;
		private static SteamPlayer voteOrigin;
		private static CSteamID voteTarget;
		private static uint voteIP;
		private static List<CSteamID> votes;

		private static ChatManager manager;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static ChatManager instance => manager;

		private static List<ReceivedChatMessage> _receivedChatHistory = new List<ReceivedChatMessage>();
		public static List<ReceivedChatMessage> receivedChatHistory => _receivedChatHistory;

		/// <summary>
		/// Add a newly received chat message to the front of the list,
		/// and remove an old message if necessary.
		/// </summary>
		public static void receiveChatMessage(CSteamID speakerSteamID, string iconURL, EChatMode mode, Color color, bool isRich, string text)
		{
			text = text.Trim(); // Some server plugins might accidentally insert extra padding.

			// Allow server plugins to show their hotkeys in chat messages.
			// Players can be silly putting the hotkeys into their messages, but not a big deal.
			ControlsSettings.formatPluginHotkeysIntoText(ref text);

			ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref text);

			if (OptionsSettings.ShouldAnonymizeMultiplayerDetails)
			{
				color = Color.white;
			}

			SteamPlayer speaker;
			if (speakerSteamID == CSteamID.Nil)
			{
				// Message was sent by alert or plugin.
				speaker = null;
			}
			else
			{
				if (!OptionsSettings.chatText && speakerSteamID != Provider.client)
					return; // If chat is disabled we only list our own messages.

				speaker = PlayerTool.getSteamPlayer(speakerSteamID);
				if (speaker.isTextChatLocallyMuted)
					return;
			}

			ReceivedChatMessage message = new ReceivedChatMessage(speaker, iconURL, mode, color, isRich, text);

			receivedChatHistory.Insert(0, message);
			if (receivedChatHistory.Count > Provider.preferenceData.Chat.History_Length)
			{
				receivedChatHistory.RemoveAt(receivedChatHistory.Count - 1);
			}

			onChatMessageReceived?.Invoke();
		}

		public static bool process(SteamPlayer player, string cmd)
		{
			return process(player, cmd, false);
		}

		public static bool process(SteamPlayer player, string cmd, bool fromUnityEvent)
		{
			bool shouldExecuteCommand = false;
			bool shouldList = true;

			string type = cmd.Substring(0, 1);
			if (type == "@" || type == "/")
			{
				if (!Dedicator.IsDedicatedServer || player.isAdmin)
				{
					shouldExecuteCommand = true;
					shouldList = false;
				}

				if (Dedicator.IsDedicatedServer && fromUnityEvent && !Provider.configData.UnityEvents.Allow_Client_Commands)
				{
					shouldExecuteCommand = false;
					shouldList = false;
				}
			}

			onCheckPermissions?.Invoke(player, cmd, ref shouldExecuteCommand, ref shouldList);

			if (fromUnityEvent)
			{
				onCheckUnityEventPermissions?.Invoke(player, cmd, ref shouldExecuteCommand, ref shouldList);
			}

			if (shouldExecuteCommand)
			{
				Commander.execute(player.playerID.steamID, cmd.Substring(1));
			}

			return shouldList;
		}

		[System.Obsolete]
		public void tellVoteStart(CSteamID steamID, CSteamID origin, CSteamID target, byte votesNeeded)
		{
			ReceiveVoteStart(origin, target, votesNeeded);
		}

		private static readonly ClientStaticMethod<CSteamID, CSteamID, byte> SendVoteStart = ClientStaticMethod<CSteamID, CSteamID, byte>.Get(ReceiveVoteStart);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVoteStart))]
		public static void ReceiveVoteStart(CSteamID origin, CSteamID target, byte votesNeeded)
		{
			SteamPlayer player = PlayerTool.getSteamPlayer(origin);

			if (player == null)
			{
				return;
			}

			SteamPlayer enemy = PlayerTool.getSteamPlayer(target);

			if (enemy == null)
			{
				return;
			}

			needsVote = true;
			hasVote = false;

			onVotingStart?.Invoke(player, enemy, votesNeeded);
		}

		[System.Obsolete]
		public void tellVoteUpdate(CSteamID steamID, byte voteYes, byte voteNo)
		{
			ReceiveVoteUpdate(voteYes, voteNo);
		}

		private static readonly ClientStaticMethod<byte, byte> SendVoteUpdate = ClientStaticMethod<byte, byte>.Get(ReceiveVoteUpdate);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVoteMessage))]
		public static void ReceiveVoteUpdate(byte voteYes, byte voteNo)
		{
			onVotingUpdate?.Invoke(voteYes, voteNo);
		}

		[System.Obsolete]
		public void tellVoteStop(CSteamID steamID, byte message)
		{
			ReceiveVoteStop((EVotingMessage) message);
		}

		private static readonly ClientStaticMethod<EVotingMessage> SendVoteStop = ClientStaticMethod<EVotingMessage>.Get(ReceiveVoteStop);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVoteStop))]
		public static void ReceiveVoteStop(EVotingMessage message)
		{
			needsVote = false;

			onVotingStop?.Invoke(message);
		}

		[System.Obsolete]
		public void tellVoteMessage(CSteamID steamID, byte message)
		{
			ReceiveVoteMessage((EVotingMessage) message);
		}

		private static readonly ClientStaticMethod<EVotingMessage> SendVoteMessage = ClientStaticMethod<EVotingMessage>.Get(ReceiveVoteMessage);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellVoteMessage))]
		public static void ReceiveVoteMessage(EVotingMessage message)
		{
			onVotingMessage?.Invoke(message);
		}

		[System.Obsolete]
		public void askVote(CSteamID steamID, bool vote)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveSubmitVoteRequest(context, vote);
		}

		private static readonly ServerStaticMethod<bool> SendSubmitVoteRequest = ServerStaticMethod<bool>.Get(ReceiveSubmitVoteRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askVote))]
		public static void ReceiveSubmitVoteRequest(in ServerInvocationContext context, bool vote)
		{
			SteamPlayer player = context.GetCallingPlayer();

			if (player == null)
			{
				return;
			}

			if (!isVoting)
			{
				return;
			}

			if (votes.Contains(player.playerID.steamID))
			{
				return;
			}

			votes.Add(player.playerID.steamID);

			if (vote)
			{
				voteYes++;
			}
			else
			{
				voteNo++;
			}

			SendVoteUpdate.Invoke(ENetReliability.Reliable, Provider.GatherClientConnections(), voteYes, voteNo);
		}

		[System.Obsolete]
		public void askCallVote(CSteamID steamID, CSteamID target)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveCallVoteRequest(context, target);
		}

		private static readonly ServerStaticMethod<CSteamID> SendCallVoteRequest = ServerStaticMethod<CSteamID>.Get(ReceiveCallVoteRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2, legacyName = nameof(askCallVote))]
		public static void ReceiveCallVoteRequest(in ServerInvocationContext context, CSteamID target)
		{
			if (isVoting)
			{
				return;
			}

			SteamPlayer player = context.GetCallingPlayer();

			if (player == null || Time.realtimeSinceStartup < player.nextVote)
			{
				SendVoteMessage.Invoke(ENetReliability.Reliable, context.GetTransportConnection(), EVotingMessage.DELAY);
				return;
			}

			if (!voteAllowed)
			{
				SendVoteMessage.Invoke(ENetReliability.Reliable, context.GetTransportConnection(), EVotingMessage.OFF);
				return;
			}

			SteamPlayer enemy = PlayerTool.getSteamPlayer(target);

			if (enemy == null || enemy.isAdmin)
			{
				return;
			}

			if (Provider.clients.Count < votePlayers)
			{
				SendVoteMessage.Invoke(ENetReliability.Reliable, context.GetTransportConnection(), EVotingMessage.PLAYERS);
				return;
			}

			CommandWindow.Log(Provider.localization.format("Vote_Kick", player.playerID.characterName, player.playerID.playerName, enemy.playerID.characterName, enemy.playerID.playerName));

			lastVote = Time.realtimeSinceStartup;
			isVoting = true;
			voteYes = 0;
			voteNo = 0;
			votesPossible = (byte) Provider.clients.Count;
			votesNeeded = (byte) Mathf.Ceil(votesPossible * votePercentage);
			voteOrigin = player;
			voteTarget = target;
			votes = new List<CSteamID>();
			voteIP = enemy.getIPv4AddressOrZero();

			SendVoteStart.Invoke(ENetReliability.Reliable, Provider.GatherClientConnections(), player.playerID.steamID, target, votesNeeded);
		}

		public static void sendVote(bool vote)
		{
			SendSubmitVoteRequest.Invoke(ENetReliability.Reliable, vote);
		}

		public static void sendCallVote(CSteamID target)
		{
			SendCallVoteRequest.Invoke(ENetReliability.Unreliable, target);
		}

		[System.Obsolete]
		public void tellChat(CSteamID steamID, CSteamID owner, string iconURL, byte mode, Color color, bool rich, string text)
		{
			ReceiveChatEntry(owner, iconURL, (EChatMode) mode, color, rich, text);
		}

		private static readonly ClientStaticMethod<CSteamID, string, EChatMode, Color, bool, string> SendChatEntry = ClientStaticMethod<CSteamID, string, EChatMode, Color, bool, string>.Get(ReceiveChatEntry);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellChat))]
		public static void ReceiveChatEntry(CSteamID owner, string iconURL, EChatMode mode, Color color, bool rich, string text)
		{
			receiveChatMessage(owner, iconURL, mode, color, rich, text);
		}

		[System.Obsolete]
		public void askChat(CSteamID steamID, byte flags, string text)
		{
			ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
			ReceiveChatRequest(context, flags, text);
		}

		private static readonly ServerStaticMethod<byte, string> SendChatRequest = ServerStaticMethod<byte, string>.Get(ReceiveChatRequest);
		[SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 15, legacyName = nameof(askChat))]
		public static void ReceiveChatRequest(in ServerInvocationContext context, byte flags, string text)
		{
			SteamPlayer player = context.GetCallingPlayer();

			if (player == null || player.player == null)
			{
				return;
			}

			if (Time.realtimeSinceStartup - player.lastChat < chatrate)
			{
				return;
			}

			player.lastChat = Time.realtimeSinceStartup;

			EChatMode send = (EChatMode) (flags & 0x7F);
			bool fromUnityEvent = (flags & 0x80) > 0;

			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			if (Dedicator.IsDedicatedServer && fromUnityEvent && !Provider.configData.UnityEvents.Allow_Client_Messages)
			{
				context.LogWarning($"Blocking ClientTextChatMessenger component from sending message \"{text}\" because UnityEvents.Allow_Client_Messages is off");
				return;
			}

			text = text.Trim();
			if (text.Length < 1)
			{
				// If text was spaces-only, then it might be empty after trim.
				return;
			}
			else if (text.ContainsNewLine())
			{
				// Prevent players from sending messages spanning dozens of lines.
				return;
			}
			else if (text.Length > MAX_MESSAGE_LENGTH)
			{
				text = text.Substring(0, MAX_MESSAGE_LENGTH);
			}

			if (send == EChatMode.GLOBAL)
			{
				if (CommandWindow.shouldLogChat)
				{
					CommandWindow.Log(Provider.localization.format("Global", player.playerID.characterName, player.playerID.playerName, text));
				}
			}
			else if (send == EChatMode.LOCAL)
			{
				if (CommandWindow.shouldLogChat)
				{
					CommandWindow.Log(Provider.localization.format("Local", player.playerID.characterName, player.playerID.playerName, text));
				}
			}
			else if (send == EChatMode.GROUP)
			{
				if (CommandWindow.shouldLogChat)
				{
					CommandWindow.Log(Provider.localization.format("Group", player.playerID.characterName, player.playerID.playerName, text));
				}
			}
			else
			{
				return;
			}

			if (fromUnityEvent)
			{
				UnturnedLog.info("UnityEventMsg {0}: '{1}'", player.playerID.steamID, text);
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (player != null)
			{
				if (text == "fly")
				{
					player.player.movement.enableFly = !player.player.movement.enableFly;
				}
				else if (text == "god")
				{
					player.player.life.enableGodMode = !player.player.life.enableGodMode;
				}
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			Color color = Color.white;
			if (player.isAdmin && !Provider.hideAdmins)
			{
				color = Palette.ADMIN;
			}
			else if (player.isPro)
			{
				color = Palette.PRO;
			}

			bool isRich = false;
			bool isShown = true;

			onChatted?.Invoke(player, send, ref color, ref isRich, text, ref isShown);

			if (ProfanityFilter.NaiveContainsHardcodedBannedWord(text))
			{
				// Prefer filtering hate speech on server because client cannot remove name from message.
				// (server allows plugins to override name in messages)
				//
				// 2023-06-12: moved after onChatted event so that plugins can do their own moderation
				// filtering before message is discarded, for example banning players who use racial slurs.
				// (public issue #3934)
				return;
			}

			if (process(player, text, fromUnityEvent) && isShown)
			{
				if (onServerFormattingMessage != null)
				{
					onServerFormattingMessage(player, send, ref text);
				}
				else
				{
					text = "%SPEAKER%: " + text;

					switch (send)
					{
						case EChatMode.LOCAL:
							text = "[A] " + text;
							break;

						case EChatMode.GROUP:
							text = "[G] " + text;
							break;
					}
				}

				if (send == EChatMode.GLOBAL)
				{
					serverSendMessage(text, color, fromPlayer: player, mode: EChatMode.GLOBAL, useRichTextFormatting: isRich);
				}
				else if (send == EChatMode.LOCAL)
				{
					float sqrHearingDistance = 128 * 128;
					foreach (SteamPlayer client in Provider.clients)
					{
						if (client.player == null)
							continue;

						if ((client.player.transform.position - player.player.transform.position).sqrMagnitude < sqrHearingDistance)
						{
							serverSendMessage(text, color, fromPlayer: player, toPlayer: client, mode: EChatMode.LOCAL, useRichTextFormatting: isRich);
						}
					}
				}
				else if (send == EChatMode.GROUP && player.player.quests.groupID != CSteamID.Nil)
				{
					foreach (SteamPlayer client in Provider.clients)
					{
						if (client.player == null)
							continue;

						if (!client.player.quests.isMemberOfSameGroupAs(player.player))
							continue;

						serverSendMessage(text, color, fromPlayer: player, toPlayer: client, mode: EChatMode.GROUP, useRichTextFormatting: isRich);
					}
				}
			}
		}

		/// <summary>
		/// Previous messages sent to server from this client.
		/// Newest at the front, oldest at the back. Used to repeat chat commands.
		/// </summary>
		private static string[] recentlySentMessages = new string[10];

		public static string getRecentlySentMessage(int index)
		{
			if (index >= 0 && index < recentlySentMessages.Length)
			{
				return recentlySentMessages[index];
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Send a request to chat from the client to the server.
		/// </summary>
		public static void sendChat(EChatMode mode, string text)
		{
			for (int index = recentlySentMessages.Length - 1; index > 0; --index)
			{
				recentlySentMessages[index] = recentlySentMessages[index - 1];
			}
			recentlySentMessages[0] = text;

#if !DEDICATED_SERVER
			if (string.Equals(text, "/copycameratransform", System.StringComparison.InvariantCultureIgnoreCase))
			{
				CopyCameraTransform();
				return;
			}
			else if (string.Equals(text, "/freezecameratransform", System.StringComparison.InvariantCultureIgnoreCase))
			{
				ToggleFreezeCameraTransform();
				return;
			}
			else if (string.Equals(text, "/logmemoryusage"))
			{
				CommandLogMemoryUsage.ExecuteAndCopyToClipboard();
			}
			else if (string.Equals(text, "/drawaudioreverbzones", System.StringComparison.InvariantCultureIgnoreCase))
			{
				// OK at this rate we really need to add a local command handler
				DrawAudioReverbZones();
				return;
			}
#endif // !DEDICATED_SERVER

			SendChatRequest.Invoke(ENetReliability.Reliable, (byte) mode, text);
		}

		/// <summary>
		/// Allows Unity events to send text chat messages from the client, for example to execute commands.
		/// Messenger context is logged to help track down assets using it in inappropriate ways.
		/// </summary>
		public static void clientSendMessage_UnityEvent(EChatMode mode, string text, ClientTextChatMessenger messenger)
		{
			if (messenger == null)
				throw new System.ArgumentNullException("messenger");

			UnturnedLog.info("UnityEventMsg {0}: '{1}'", messenger.gameObject.GetSceneHierarchyPath(), text);
			SendChatRequest.Invoke(ENetReliability.Reliable, (byte) ((int) mode | 0x80), text);
		}

		/// <summary>
		/// Allows Unity events to broadcast text chat messages from the server.
		/// Messenger context is logged to help track down assets using it in inappropriate ways.
		/// </summary>
		public static void serverSendMessage_UnityEvent(string text, Color color, string iconURL, bool useRichTextFormatting, ServerTextChatMessenger messenger)
		{
			if (messenger == null)
				throw new System.ArgumentNullException("messenger");

			if (Dedicator.IsDedicatedServer && !Provider.configData.UnityEvents.Allow_Server_Messages)
			{
				UnturnedLog.info($"Blocking ServerTextChatMessenger component at {messenger.gameObject.GetSceneHierarchyPath()} from sending message \"{text}\" because UnityEvents.Allow_Server_Messages is off");
				return;
			}

			UnturnedLog.info("UnityEventMsg {0}: '{1}'", messenger.gameObject.GetSceneHierarchyPath(), text);
			serverSendMessage(text,
				color,
				toPlayer: null,
				fromPlayer: null,
				mode: EChatMode.SAY,
				iconURL: iconURL,
				useRichTextFormatting: useRichTextFormatting);
		}

		/// <summary>
		/// Server send message to specific player.
		/// Used in vanilla for the welcome message.
		/// Should not be removed because plugins may depend on it.
		/// </summary>
		public static void say(CSteamID target, string text, Color color, bool isRich = false)
		{
			say(target, text, color, EChatMode.WELCOME, isRich);
		}

		/// <summary>
		/// Server send message to specific player.
		/// Used in vanilla by help command to tell player about command options.
		/// Should not be removed because plugins may depend on it.
		/// </summary>
		public static void say(CSteamID target, string text, Color color, EChatMode mode, bool isRich = false)
		{
			SteamPlayer toPlayer = PlayerTool.getSteamPlayer(target);
			if (toPlayer == null)
				return;

			serverSendMessage(text, color, toPlayer: toPlayer, useRichTextFormatting: isRich);
		}

		/// <summary>
		/// Server send message to all players.
		/// Used in vanilla by some alerts and broadcast command.
		/// Should not be removed because plugins may depend on it.
		/// </summary>
		public static void say(string text, Color color, bool isRich = false)
		{
			serverSendMessage(text, color, useRichTextFormatting: isRich);
		}

		/// <summary>
		/// Serverside send a chat message to all players, or a specific player.
		/// </summary>
		/// <param name="text">Contents to display.</param>
		/// <param name="color">Default text color unless rich formatting overrides it.</param>
		/// <param name="fromPlayer">Player who sent the message (used for avatar), or null if send by a plugin.</param>
		/// <param name="toPlayer">Send message to only this player, or all players if null.</param>
		/// <param name="mode">Mostly deprecated, but global/local/group may be displayed.</param>
		/// <param name="iconURL">URL to a 32x32 .png to show rather than a player avatar, or null/empty.</param>
		/// <param name="useRichTextFormatting">Enable rich tags e.g., bold, italics in the message contents.</param>
		public static void serverSendMessage(string text,
			Color color,
			SteamPlayer fromPlayer = null,
			SteamPlayer toPlayer = null,
			EChatMode mode = EChatMode.SAY,
			string iconURL = null,
			bool useRichTextFormatting = false)
		{
			if (!Provider.isServer)
			{
				throw new System.Exception("Tried to send server message, but currently a client! Text: " + text);
			}

#if WITH_GAME_THREAD_ASSERTIONS
			ThreadUtil.assertIsGameThread();
#endif

			onServerSendingMessage?.Invoke(ref text, ref color, fromPlayer, toPlayer, mode, ref iconURL, ref useRichTextFormatting);
			// not else-if in-case onServerSendingMessage did not format %SPEAKER%
			if (fromPlayer != null && toPlayer != null)
			{
				string fromName;
				if (!string.IsNullOrEmpty(fromPlayer.playerID.nickName) && // has nickname
					fromPlayer != toPlayer && // show character name to self
					toPlayer.player != null &&
					fromPlayer.player != null &&
					fromPlayer.player.quests.isMemberOfSameGroupAs(toPlayer.player))
				{
					fromName = fromPlayer.playerID.nickName;
				}
				else
				{
					fromName = fromPlayer.playerID.characterName;
				}

				text = text.Replace("%SPEAKER%", fromName);
			}

			if (iconURL == null)
			{
				iconURL = string.Empty;
			}

			CSteamID fromPlayerID = fromPlayer == null ? CSteamID.Nil : fromPlayer.playerID.steamID;

			if (toPlayer == null)
			{
				// Expand toPlayer to each client to allow per-client message formatting
				foreach (SteamPlayer client in Provider.clients)
				{
					if (client == null)
						continue; // Could potentially cause recursive send message

					serverSendMessage(text, color, fromPlayer, client, mode, iconURL, useRichTextFormatting);
				}
			}
			else
			{
				SendChatEntry.Invoke(ENetReliability.Reliable, toPlayer.transportConnection, fromPlayerID, iconURL, mode, color, useRichTextFormatting, text);
			}
		}

		private void onLevelLoaded(int level)
		{
			if (level > Level.BUILD_INDEX_SETUP)
			{
				receivedChatHistory.Clear();
			}
		}

		private void onServerConnected(CSteamID steamID)
		{
			if (Provider.isServer)
			{
				if (welcomeText != "")
				{
					SteamPlayer player = PlayerTool.getSteamPlayer(steamID);
					say(player.playerID.steamID, string.Format(welcomeText, player.playerID.characterName), welcomeColor);
				}
			}
		}

		private void Update()
		{
			if (isVoting && (Time.realtimeSinceStartup - lastVote > voteDuration || voteYes >= votesNeeded || voteNo > votesPossible - votesNeeded))
			{
				isVoting = false;

				if (voteYes >= votesNeeded)
				{
					if (voteOrigin != null)
					{
						voteOrigin.nextVote = Time.realtimeSinceStartup + votePassCooldown;
					}

					CommandWindow.Log(Provider.localization.format("Vote_Pass"));
					SendVoteStop.Invoke(ENetReliability.Reliable, Provider.GatherClientConnections(), EVotingMessage.PASS);

					SteamBlacklist.ban(voteTarget, voteIP, null, CSteamID.Nil, "you were vote kicked", SteamBlacklist.TEMPORARY);
				}
				else
				{
					if (voteOrigin != null)
					{
						voteOrigin.nextVote = Time.realtimeSinceStartup + voteFailCooldown;
					}

					CommandWindow.Log(Provider.localization.format("Vote_Fail"));
					SendVoteStop.Invoke(ENetReliability.Reliable, Provider.GatherClientConnections(), EVotingMessage.FAIL);
				}
			}

			if (needsVote && !hasVote)
			{
				if (InputEx.GetKeyDown(KeyCode.F1))
				{
					needsVote = false;
					hasVote = true;

					//if(onVotingStop != null)
					//{
					//	onVotingStop();
					//}

					sendVote(true);
				}
				else if (InputEx.GetKeyDown(KeyCode.F2))
				{
					needsVote = false;
					hasVote = true;

					//if(onVotingStop != null)
					//{
					//	onVotingStop();
					//}

					sendVote(false);
				}
			}
		}

		/// <summary>
		/// Nelson 2024-10-14: We might want to elaborate on this with "client-side chat commands" in the future, but
		/// for the meantime I've hacked in this one command.
		/// </summary>
		internal static void CopyCameraTransform()
		{
			Camera mainCamera = MainCamera.instance;
			if (mainCamera == null)
			{
				UnturnedLog.warn("Unable to copy camera transform because there is no active main camera");
				return;
			}

			Vector3 eulerAngles = mainCamera.transform.rotation.eulerAngles;
			float pitch = eulerAngles.x;
			float yaw = eulerAngles.y;

			GUIUtility.systemCopyBuffer = $"{mainCamera.transform.position}:{pitch}, {yaw}";
		}

		internal static void ToggleFreezeCameraTransform()
		{
			Camera mainCamera = MainCamera.instance;
			if (mainCamera == null)
			{
				UnturnedLog.warn("Unable to freeze camera transform because there is no active main camera");
				return;
			}

			if (!Player.LocalPlayer.channel.owner.isAdmin)
			{
				UnturnedLog.warn("Unable to freeze camera transform without admin permissions");
				return;
			}

			MainCamera.IsPositionFrozen = !MainCamera.IsPositionFrozen;
		}

#if !DEDICATED_SERVER
		internal static void DrawAudioReverbZones()
		{
			if (!Player.LocalPlayer.channel.owner.isAdmin)
			{
				UnturnedLog.warn("Unable to draw audio reverb zones without admin permissions");
				return;
			}

			bool addedToAny = false;
			List<ReverbGizmoComponent> gizmoComponents = new List<ReverbGizmoComponent>();

			AudioReverbZone[] zones = FindObjectsByType<AudioReverbZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			if (!zones.IsNullOrEmpty())
			{
				foreach (AudioReverbZone zone in zones)
				{
					ReverbGizmoComponent gizmo = zone.GetComponent<ReverbGizmoComponent>();
					if (gizmo == null)
					{
						gizmo = zone.gameObject.AddComponent<ReverbGizmoComponent>();
						gizmo.zone = zone;
						addedToAny = true;
					}
					gizmoComponents.Add(gizmo);
				}
			}

			if (!addedToAny) // Toggle off
			{
				foreach (ReverbGizmoComponent gizmo in gizmoComponents)
				{
					Destroy(gizmo);
				}
			}
		}
#endif // !DEDICATED_SERVER

		private void Start()
		{
			manager = this;

			Level.onLevelLoaded += onLevelLoaded;
			Provider.onServerConnected += onServerConnected;
		}
	}
}
