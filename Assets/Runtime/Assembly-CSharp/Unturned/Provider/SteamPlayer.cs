////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_KILL_COUNTERS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Provider;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	[NetPak.NetEnum]
	public enum EClientPlatform
	{
		Windows,
		Mac,
		Linux,
	}

	public class SteamPlayer : SteamConnectedClientBase
	{
		public static System.Action<SteamPlayer, string, byte[]> OnSteamAuthTicketForWebApiReceived;

		private NetId _netId;
		public NetId GetNetId()
		{
			return _netId;
		}

		private SteamPlayerID _playerID;
		public SteamPlayerID playerID => _playerID;

		private Transform _model;
		public Transform model => _model;

		private Player _player;
		public Player player => _player;

		private bool _isPro;
		public bool isPro
		{
			get
			{
				if (OptionsSettings.ShouldAnonymizeMultiplayerDetails && playerID.steamID != Provider.user)
				{
					return false;
				}
				else
				{
					return _isPro;
				}
			}
		}

		private int _channel;
		public int channel => _channel;

#if WITH_THIRDPARTYAC
		/// <summary>
		/// Not an actual external ID, instead this is used to map player references to and from thirdparty anti-cheat systems.
		/// </summary>
		internal int thirdpartyAntiCheatId;
#endif // WITH_THIRDPARTYAC

		private bool _isAdmin;
		public bool isAdmin
		{
			get
			{
				if (OptionsSettings.ShouldAnonymizeMultiplayerDetails && playerID.steamID != Provider.user)
				{
					return false;
				}
				else
				{
					return _isAdmin;
				}
			}

			set => _isAdmin = value;
		}

		private float[] pings;
		private float _ping;
		public float ping => _ping;

		private float _joined;
		public float joined => _joined;

		public byte face;

		private byte _hair;
		public byte hair => _hair;

		private byte _beard;
		public byte beard => _beard;

		private Color _skin;
		public Color skin => _skin;

		private Color _color;
		public Color color => _color;

		private Color _markerColor;
		public Color markerColor => _markerColor;

		public Color BeardColor
		{
			get;
			set;
		}

		private bool _hand;
		public bool IsLeftHanded => _hand;

		[System.Obsolete("Renamed to IsLeftHanded")]
		public bool hand => _hand;

		public int shirtItem;
		public int pantsItem;
		public int hatItem;
		public int backpackItem;
		public int vestItem;
		public int maskItem;
		public int glassesItem;

		/// <summary>
		/// Steam itemdef IDs of equipped weapon and vehicle skins.
		/// </summary>
		public int[] skinItems;

		/// <summary>
		/// Unique per-item tags.
		/// Indices correspond to those in skinItems array.
		/// </summary>
		public string[] skinTags;

		/// <summary>
		/// Unique per-item dynamic properties.
		/// Indices correspond to those in skinItems array.
		/// </summary>
		public string[] skinDynamicProps;

		public Dictionary<ushort, int> itemSkins;
		[System.Obsolete("This will be removed in a future version!")]
		public Dictionary<ushort, int> vehicleSkins;
		private Dictionary<System.Guid, int> vehicleGuidToSkinItemDefId;
		/// <summary>
		/// Steam item def IDs of items with dynamic property updates.
		/// </summary>
		public HashSet<int> modifiedItems;
		private bool submittedModifiedItems;

		private EPlayerSkillset _skillset;
		public EPlayerSkillset skillset => _skillset;

		private string _language;
		public string language => _language;

		public CSteamID lobbyID
		{
			get;
			private set;
		}

		//private bool _isAnonymous;
		//public bool isAnonymous
		//{
		//	get { return _isAnonymous; }
		//}

		public float timeLastPacketWasReceivedFromClient;
		public float timeLastPingRequestWasSentToClient;
		public float lastChat;
		public float nextVote;

		public bool isVoiceChatLocallyMuted;
		public bool isTextChatLocallyMuted;

#if !DEDICATED_SERVER
		/// <summary>
		/// True for offline or listen server host.
		/// </summary>
		public bool IsLocalServerHost
		{
			get;
			private set;
		}

		public void SetVoiceChatLocallyMuted(bool newVoiceChatLocallyMuted)
		{
			if (isVoiceChatLocallyMuted != newVoiceChatLocallyMuted)
			{
				isVoiceChatLocallyMuted = newVoiceChatLocallyMuted;
				LocalPlayerBlocklist.SetVoiceChatMuted(playerID.steamID, isVoiceChatLocallyMuted);
			}
		}

		public void SetTextChatLocallyMuted(bool newTextChatLocallyMuted)
		{
			if (isTextChatLocallyMuted != newTextChatLocallyMuted)
			{
				isTextChatLocallyMuted = newTextChatLocallyMuted;
				LocalPlayerBlocklist.SetTextChatMuted(playerID.steamID, isTextChatLocallyMuted);
			}
		}
#endif // !DEDICATED_SERVER

		[System.Obsolete("This field should not have been externally used and will be removed in a future version.")]
		public float rpcCredits;
		public float lastReceivedPingRequestRealtime;

		/// <summary>
		/// Next time method is allowed to be called.
		/// </summary>
		public float[] rpcAllowedTimes = new float[NetReflection.rateLimitedMethodsCount];
		/// <summary>
		/// Number of times client has tried to invoke this method while rate-limited.
		/// </summary>
		internal int[] rpcHitCount = new int[NetReflection.rateLimitedMethodsCount];

		internal EClientPlatform clientPlatform;

		public bool getItemSkinItemDefID(ushort itemID, out int itemdefid)
		{
			itemdefid = 0;

			if (itemSkins == null)
			{
				return false;
			}

			return itemSkins.TryGetValue(itemID, out itemdefid);
		}

		[System.Obsolete("This will be removed in a future version!")]
		public bool getVehicleSkinItemDefID(ushort vehicleID, out int itemdefid)
		{
			itemdefid = 0;

			if (vehicleSkins == null)
			{
				return false;
			}

			return vehicleSkins.TryGetValue(vehicleID, out itemdefid);
		}

		/// <summary>
		/// Get Steam item definition ID equipped for given vehicle.
		/// </summary>
		/// <returns>True if a skin was available.</returns>
		public bool GetVehicleSkinItemDefId(InteractableVehicle vehicle, out int itemdefid)
		{
			itemdefid = 0;

			if (vehicle == null || vehicleGuidToSkinItemDefId == null)
			{
				return false;
			}

			VehicleAsset sharedSkinVehicleAsset = vehicle.asset.FindSharedSkinVehicleAsset();
			if (sharedSkinVehicleAsset == null)
			{
				return false;
			}

			return vehicleGuidToSkinItemDefId.TryGetValue(sharedSkinVehicleAsset.GUID, out itemdefid);
		}

		public bool getTagsAndDynamicPropsForItem(int item, out string tags, out string dynamic_props)
		{
			tags = string.Empty;
			dynamic_props = string.Empty;

			for (int itemIndex = 0; itemIndex < skinItems.Length; itemIndex++)
			{
				if (skinItems[itemIndex] == item)
				{
					if (itemIndex < skinTags.Length && itemIndex < skinDynamicProps.Length)
					{
						tags = skinTags[itemIndex];
						dynamic_props = skinDynamicProps[itemIndex];

						return true;
					}
					else
					{
						// Bad data?
						return false;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Build econ details struct from tags and dynamic_props.
		/// Note that details cannot be modified because it's a struct and has copies of the data.
		/// </summary>
		public bool getDynamicEconDetails(ushort itemID, out DynamicEconDetails details)
		{
			int itemdefid;
			if (!getItemSkinItemDefID(itemID, out itemdefid))
			{
				details = default;
				return false;
			}

			return getDynamicEconDetailsForItemDef(itemdefid, out details);
		}

		public bool getDynamicEconDetailsForItemDef(int itemdefid, out DynamicEconDetails details)
		{
			string tags;
			string dynamic_props;
			if (!getTagsAndDynamicPropsForItem(itemdefid, out tags, out dynamic_props))
			{
				details = default;
				return false;
			}

			details = new DynamicEconDetails(tags, dynamic_props);
			return true;
		}

		public bool getStatTrackerValue(ushort itemID, out EStatTrackerType type, out int kills)
		{
			DynamicEconDetails details;
			if (!getDynamicEconDetails(itemID, out details))
			{
				type = EStatTrackerType.NONE;
				kills = -1;
				return false;
			}

			return details.getStatTrackerValue(out type, out kills);
		}

		public bool getStatTrackerValueForItemDef(int itemDefId, out EStatTrackerType type, out int kills)
		{
			DynamicEconDetails details;
			if (!getDynamicEconDetailsForItemDef(itemDefId, out details))
			{
				type = EStatTrackerType.NONE;
				kills = -1;
				return false;
			}

			return details.getStatTrackerValue(out type, out kills);
		}

		public bool getRagdollEffect(ushort itemID, out ERagdollEffect effect)
		{
			DynamicEconDetails details;
			if (!getDynamicEconDetails(itemID, out details))
			{
				effect = ERagdollEffect.None;
				return false;
			}

			return details.getRagdollEffect(out effect);
		}

		public bool TryGetRagdollEffectForItemDef(int itemDefId, out ERagdollEffect effect)
		{
			if (!getDynamicEconDetailsForItemDef(itemDefId, out DynamicEconDetails details))
			{
				effect = ERagdollEffect.None;
				return false;
			}

			return details.getRagdollEffect(out effect);
		}

		public ushort getParticleEffectForItemDef(int itemdefid)
		{
			DynamicEconDetails details;
			if (getDynamicEconDetailsForItemDef(itemdefid, out details))
			{
				return details.getParticleEffect();
			}
			else
			{
				return 0;
			}
		}

		public void incrementStatTrackerValue(InteractableVehicle vehicle, EPlayerStat stat)
		{
			int itemdefid;
			if (!GetVehicleSkinItemDefId(vehicle, out itemdefid))
			{
#if LOG_KILL_COUNTERS
				UnturnedLog.info($"Not incrementing {vehicle.asset.FriendlyName} kills because no skin is equipped");
#endif
				return;
			}

			IncrementStatTrackerValue(itemdefid, stat);
		}

		public void incrementStatTrackerValue(ushort itemID, EPlayerStat stat)
		{
			int itemdefid;
			if (!getItemSkinItemDefID(itemID, out itemdefid))
			{
#if LOG_KILL_COUNTERS
				UnturnedLog.info($"Not incrementing item {itemID} kills because no skin is equipped");
#endif
				return;
			}

			IncrementStatTrackerValue(itemdefid, stat);
		}

		private void IncrementStatTrackerValue(int itemdefid, EPlayerStat stat)
		{
			string tags;
			string dynamic_props;
			if (!getTagsAndDynamicPropsForItem(itemdefid, out tags, out dynamic_props))
			{
#if LOG_KILL_COUNTERS
				UnturnedLog.info($"Not incrementing itemdefid {itemdefid} kills because it has no dynamic properties");
#endif
				return;
			}

			DynamicEconDetails details = new DynamicEconDetails(tags, dynamic_props);
			EStatTrackerType type;
			int kills;
			if (!details.getStatTrackerValue(out type, out kills))
			{
#if LOG_KILL_COUNTERS
				UnturnedLog.info($"Not incrementing itemdefid {itemdefid} kills because its dynamic properties do not contain a stat tracker");
#endif
				return;
			}

			switch (type)
			{
				case EStatTrackerType.TOTAL:
					if (stat != EPlayerStat.KILLS_ANIMALS && stat != EPlayerStat.KILLS_PLAYERS && stat != EPlayerStat.KILLS_ZOMBIES_MEGA && stat != EPlayerStat.KILLS_ZOMBIES_NORMAL)
					{
#if LOG_KILL_COUNTERS
						UnturnedLog.info($"Not incrementing itemdefid {itemdefid} kills because {stat} doesn't match {type} filter");
#endif
						return;
					}

					break;

				case EStatTrackerType.PLAYER:
					if (stat != EPlayerStat.KILLS_PLAYERS)
					{
#if LOG_KILL_COUNTERS
						UnturnedLog.info($"Not incrementing itemdefid {itemdefid} kills because {stat} doesn't match {type} filter");
#endif
						return;
					}

					break;

				default:
					return;
			}

			modifiedItems.Add(itemdefid);

			kills++;

#if LOG_KILL_COUNTERS
			UnturnedLog.info($"Incremented {type} to {kills} on {Provider.provider.economyService.getInventoryName(itemdefid)}");
#endif

			for (int itemIndex = 0; itemIndex < skinItems.Length; itemIndex++)
			{
				if (skinItems[itemIndex] == itemdefid)
				{
					if (itemIndex < skinDynamicProps.Length)
					{
						skinDynamicProps[itemIndex] = details.getPredictedDynamicPropsJsonForStatTracker(type, kills);
					}

					break;
				}
			}
		}

		public void commitModifiedDynamicProps()
		{
			if (modifiedItems.Count < 1 || submittedModifiedItems)
			{
				return;
			}
			submittedModifiedItems = true;

			SteamInventoryUpdateHandle_t handle = SteamInventory.StartUpdateProperties();
			int modificationCount = 0;
			foreach (int itemdefid in modifiedItems)
			{
				ulong instanceID;
				if (!Characters.TryGetEquippedSkinInstanceIdByItemDefId(itemdefid, out instanceID))
				{
					continue;
				}

				EStatTrackerType type;
				int kills;
				if (!getStatTrackerValueForItemDef(itemdefid, out type, out kills))
				{
					continue;
				}

				string propertyName = Provider.provider.economyService.getStatTrackerPropertyName(type);
				if (string.IsNullOrEmpty(propertyName))
				{
					continue;
				}

				SteamInventory.SetProperty(handle, new SteamItemInstanceID_t(instanceID), propertyName, kills);
				++modificationCount;
			}
			SteamInventory.SubmitUpdateProperties(handle, out Provider.provider.economyService.commitResult);
			UnturnedLog.info($"Submitted {modificationCount} item property update(s)");
		}

		/// <summary>
		/// Add a recent ping sample to the average ping window.
		/// Updates ping based on the average of several recent ping samples.
		/// </summary>
		/// <param name="value">Most recent ping value.</param>
		public void lag(float value)
		{
			value = Mathf.Clamp01(value);
			_ping = value;

			for (int index = pings.Length - 1; index > 0; index--)
			{
				pings[index] = pings[index - 1];

				if (pings[index] > 0.001f)
				{
					_ping += pings[index];
				}
			}

			_ping /= pings.Length;
			pings[0] = value;
		}

		/// <returns>True if both players exist, are both members of groups, and are both members of the same group.</returns>
		public bool isMemberOfSameGroupAs(Player other)
		{
			return player != null && other != null && player.quests.isMemberOfSameGroupAs(other);
		}

		/// <returns>True if both players exist, are both members of groups, and are both members of the same group.</returns>
		public bool isMemberOfSameGroupAs(SteamPlayer other)
		{
			return other != null && isMemberOfSameGroupAs(other.player);
		}

		/// <summary>
		/// Get real IPv4 address of remote player NOT the relay server.
		/// </summary>
		/// <returns>True if address was available, and not flagged as a relay server.</returns>
		public bool getIPv4Address(out uint address)
		{
			if (!ReferenceEquals(transportConnection, null))
			{
				return transportConnection.TryGetIPv4Address(out address);
			}
			else
			{
				address = 0;
				return false;
			}
		}

		/// <summary>
		/// See above, returns zero if failed.
		/// </summary>
		public uint getIPv4AddressOrZero()
		{
			uint address;
			getIPv4Address(out address);
			return address;
		}

		/// <summary>
		/// Get real address of remote player NOT a relay server.
		/// </summary>
		/// <returns>Null if address was unavailable.</returns>
		public System.Net.IPAddress getAddress()
		{
			if (transportConnection != null)
			{
				return transportConnection.GetAddress();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Get string representation of remote end point.
		/// </summary>
		/// <returns>Null if address was unavailable.</returns>
		public string getAddressString(bool withPort)
		{
			if (transportConnection != null)
			{
				return transportConnection.GetAddressString(withPort);
			}
			else
			{
				return null;
			}
		}

		public bool Equals(SteamPlayer otherClient)
		{
			return !ReferenceEquals(otherClient, null) && playerID.Equals(otherClient.playerID);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as SteamPlayer);
		}

		public override int GetHashCode()
		{
			return playerID.GetHashCode();
		}

		public override string ToString()
		{
			string result = string.Empty;

			if (!ReferenceEquals(playerID, null))
			{
				result = playerID.ToString();
			}

			if (!ReferenceEquals(transportConnection, null))
			{
				if (string.IsNullOrEmpty(result))
				{
					result = transportConnection.ToString();
				}
				else
				{
					result = $"{result} Connection: {transportConnection.ToString()}";
				}
			}

			if (string.IsNullOrEmpty(result))
			{
				result = "[invalid client]";
			}

			return result;
		}

		/// <summary>
		/// Players can set a "nickname" which is only shown to the members in their group.
		/// </summary>
		internal string GetLocalDisplayName()
		{
			if (!string.IsNullOrEmpty(playerID.nickName) &&
				playerID.steamID != Provider.client &&
				player != null &&
				player.quests != null &&
				Player.LocalPlayer != null && // local player
				player.quests.isMemberOfSameGroupAs(Player.LocalPlayer))
			{
				return playerID.nickName;
			}
			else
			{
				return playerID.characterName;
			}
		}

		/// <summary>
		/// Can be used by plugins to verify player is on a particular server.
		/// 
		/// OnSteamAuthTicketForWebApiReceived will be invoked when the response is received.
		/// Note that the client doesn't send anything if the request to Steam fails, so plugins may wish to kick
		/// players if a certain amount of time passes. (e.g., if a cheat is canceling the request)
		/// </summary>
		public void RequestSteamAuthTicketForWebApi(string identity)
		{
			if (string.IsNullOrWhiteSpace(identity))
			{
				throw new System.ArgumentException("cannot be null or empty", nameof(identity));
			}

			bool added = requestedSteamAuthTicketIdentities.Add(identity);
			if (!added)
			{
				// Already requested.
				return;
			}

			UnturnedLog.info($"Sending request to {transportConnection} for Steam auth ticket for web API identity \"{identity}\"");
			SendGetSteamAuthTicketForWebApiRequest.Invoke(ENetReliability.Reliable, transportConnection, identity);
		}

		private static readonly ClientStaticMethod<string> SendGetSteamAuthTicketForWebApiRequest = ClientStaticMethod<string>.Get(ReceiveGetSteamAuthTicketForWebApiRequest);
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public static void ReceiveGetSteamAuthTicketForWebApiRequest(string identity)
		{
			UnturnedLog.info($"Received request to get Steam auth ticket for web API identity \"{identity}\"");
			Provider.RequestSteamAuthTicketForWebApi(identity);
		}

		internal static readonly ServerStaticMethod SendGetSteamAuthTicketForWebApiResponse = ServerStaticMethod.Get(ReceiveGetSteamAuthTicketForWebApiResponse);
		[SteamCall(ESteamCallValidation.SERVERSIDE)]
		public static void ReceiveGetSteamAuthTicketForWebApiResponse(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;
			SteamPlayer player = context.GetCallingPlayer();

			if (!reader.ReadString(out string identity, lengthBitCount: 5))
			{
				context.Kick("Unable to read Steam auth ticket web API identity");
				return;
			}

			if (!player.requestedSteamAuthTicketIdentities.Contains(identity))
			{
				context.Kick("Server did not request Steam auth ticket for provided web API identity");
				return;
			}

			bool added = player.receivedSteamAuthTicketIdentities.Add(identity);
			if (!added)
			{
				context.Kick("Client sent duplicate Steam auth ticket for web API response");
				return;
			}

			ushort ticketLength;
			if (!reader.ReadUInt16(out ticketLength))
			{
				context.Kick("Unable to read Steam web API auth ticket length");
				return;
			}

			if (ticketLength > Steamworks.Constants.k_nCubTicketMaxLength)
			{
				context.Kick("Steam web API auth ticket longer than maximum");
				return;
			}

			byte[] ticket = new byte[ticketLength];
			if (!reader.ReadBytes(ticket))
			{
				context.Kick("Unable to read Steam web API auth ticket contents");
				return;
			}

			UnturnedLog.info($"Received response from {player.transportConnection} for Steam auth ticket for web API identity \"{identity}\" length: {ticketLength}");
			OnSteamAuthTicketForWebApiReceived?.TryInvoke("OnSteamAuthTicketForWebApiReceived", player, identity, ticket);
		}

		public SteamPlayer(ITransportConnection transportConnection, NetId netId, SteamPlayerID newPlayerID, Transform newModel, bool newPro, bool newAdmin, int newChannel, byte newFace, byte newHair, byte newBeard, Color newSkin, Color newColor, Color newMarkerColor, Color newBeardColor, bool newHand, int newShirtItem, int newPantsItem, int newHatItem, int newBackpackItem, int newVestItem, int newMaskItem, int newGlassesItem, int[] newSkinItems, string[] newSkinTags, string[] newSkinDynamicProps, EPlayerSkillset newSkillset, string newLanguage, CSteamID newLobbyID, EClientPlatform clientPlatform)
		{
			this.transportConnection = transportConnection;

			_netId = netId;
			NetIdRegistry.Assign(_netId, this);

#if !DEDICATED_SERVER
			// In offline the transport connection is a dummy struct, whereas in multiplayer it is null on the client.
			// Note that !DEDICATED_SERVER can be true for test builds running server.
			IsLocalServerHost = transportConnection != null && !Dedicator.IsDedicatedServer;

			bool isLocalPlayer = newPlayerID.steamID == Provider.client;
			if (!isLocalPlayer && !Dedicator.IsDedicatedServer)
			{
				LocalPlayerBlocklist.GetBlockStatus(newPlayerID.steamID, out isVoiceChatLocallyMuted, out isTextChatLocallyMuted);
			}
#endif // !DEDICATED_SERVER

			_playerID = newPlayerID;

			_model = newModel;
			model.name = playerID.characterName + " [" + playerID.playerName + "]";
			model.GetComponent<SteamChannel>().id = newChannel;
			model.GetComponent<SteamChannel>().owner = this;
#if !DEDICATED_SERVER
			model.GetComponent<SteamChannel>().IsLocalPlayer = isLocalPlayer;
#endif // !DEDICATED_SERVER
			model.GetComponent<SteamChannel>().setup();

			_player = model.GetComponent<Player>();
			_player.AssignNetIdBlock(_netId);

			_isPro = newPro;
			_channel = newChannel;
			isAdmin = newAdmin;

			face = newFace;
			_hair = newHair;
			_beard = newBeard;

			_skin = newSkin;
			_color = newColor;
			_markerColor = newMarkerColor;
			BeardColor = newBeardColor;

			_hand = newHand;
			_skillset = newSkillset;
			_language = newLanguage;
			//_isAnonymous = newAnonymous;

			shirtItem = newShirtItem;
			pantsItem = newPantsItem;
			hatItem = newHatItem;
			backpackItem = newBackpackItem;
			vestItem = newVestItem;
			maskItem = newMaskItem;
			glassesItem = newGlassesItem;
			skinItems = newSkinItems;
			skinTags = newSkinTags;
			skinDynamicProps = newSkinDynamicProps;

			//if(!Dedicator.IsDedicatedServer)
			//{
			itemSkins = new Dictionary<ushort, int>();
#pragma warning disable
			vehicleSkins = new Dictionary<ushort, int>();
#pragma warning restore
			vehicleGuidToSkinItemDefId = new Dictionary<System.Guid, int>();
			modifiedItems = new HashSet<int>();

			for (int index = 0; index < skinItems.Length; index++)
			{
				int item = skinItems[index];
				if (item == 0)
				{
					continue;
				}

				System.Guid itemGuid;
				System.Guid vehicleGuid;
				Provider.provider.economyService.getInventoryTargetID(item, out itemGuid, out vehicleGuid);

				ItemAsset itemAsset = Assets.find<ItemAsset>(itemGuid);
				VehicleAsset vehicleAsset = VehicleTool.FindVehicleByGuidAndHandleRedirects(vehicleGuid);

				if (itemAsset != null)
				{
					if (!itemSkins.ContainsKey(itemAsset.id))
					{
						itemSkins.Add(itemAsset.id, item);
					}
				}
				else if (vehicleAsset != null)
				{
#pragma warning disable
					if (!vehicleSkins.ContainsKey(vehicleAsset.id))
					{
						vehicleSkins.Add(vehicleAsset.id, item);
					}
#pragma warning restore
					vehicleGuidToSkinItemDefId[vehicleAsset.GUID] = item;
				}
			}
			//}

			pings = new float[4];

			timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
			lastChat = Time.realtimeSinceStartup;
			nextVote = Time.realtimeSinceStartup;
			lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;

			_joined = Time.realtimeSinceStartup;

			lobbyID = newLobbyID;
			this.clientPlatform = clientPlatform;
		}

#if WITH_NSB_LOGGING
		public float sentAnimalUpdate = Time.realtimeSinceStartup;
		public float sentPlayerUpdate = Time.realtimeSinceStartup;
		public float sentVehicleUpdate = Time.realtimeSinceStartup;
#endif // WITH_NSB_LOGGING

		internal HashSet<System.Guid> validatedGuids = new HashSet<System.Guid>();
		/// <summary>
		/// Since this isn't accessible to plugins it isn't necessarily up-to-date. For example, when a player
		/// teleports the culledPlayers list isn't updated until the next PlayerManager sync. If this becomes
		/// publicly accessible in some way it should be kept in sync.
		/// </summary>
		internal HashSet<CSteamID> culledPlayers = new HashSet<CSteamID>();
		private HashSet<string> requestedSteamAuthTicketIdentities = new HashSet<string>();
		private HashSet<string> receivedSteamAuthTicketIdentities = new HashSet<string>();
	}
}
