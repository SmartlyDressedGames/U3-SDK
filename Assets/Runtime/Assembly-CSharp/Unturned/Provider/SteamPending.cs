////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Provider;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Some new code common to SteamPending and SteamPlayer.
	/// </summary>
	public class SteamConnectedClientBase
	{
		/// <summary>
		/// Realtime the first ping request was received.
		/// </summary>
		private float firstPingRequestRealtime;

		/// <summary>
		/// Number of ping requests the server has received from this client.
		/// </summary>
		public int numPingRequestsReceived
		{
			get;
			private set;
		}

		/// <summary>
		/// Called when a ping request is received from this client.
		/// </summary>
		public void incrementNumPingRequestsReceived()
		{
			if (numPingRequestsReceived == 0)
			{
				firstPingRequestRealtime = Time.realtimeSinceStartup;
			}

			numPingRequestsReceived++;
		}

		/// <summary>
		/// Realtime passed since the first ping request was received from this client.
		/// </summary>
		public float realtimeSinceFirstPingRequest => Time.realtimeSinceStartup - firstPingRequestRealtime;

		/// <summary>
		/// Average number of ping requests received from this client per second.
		/// Begins tracking 10 seconds after the first ping request was received, or -1 if average is unknown yet.
		/// </summary>
		public float averagePingRequestsReceivedPerSecond
		{
			get
			{
				if (numPingRequestsReceived < 1)
					return -1;

				float deltaTime = realtimeSinceFirstPingRequest;
				if (deltaTime < 10.0f)
					return -1;

				return numPingRequestsReceived / deltaTime;
			}
		}

		/// <summary>
		/// Only set on server. Associates player with their connection.
		/// </summary>
		public ITransportConnection transportConnection
		{
			get;
			protected set;
		}
	}

	public class SteamPending : SteamConnectedClientBase
	{
		private SteamPlayerID _playerID;
		public SteamPlayerID playerID => _playerID;

		private bool _isPro;
		public bool isPro => _isPro;

		private byte _face;
		public byte face => _face;

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

		public ulong packageShirt;
		public ulong packagePants;
		public ulong packageHat;
		public ulong packageBackpack;
		public ulong packageVest;
		public ulong packageMask;
		public ulong packageGlasses;
		public ulong[] packageSkins;

		public SteamInventoryResult_t inventoryResult = SteamInventoryResult_t.Invalid;
		public SteamItemDetails_t[] inventoryDetails;
		public Dictionary<ulong, DynamicEconDetails> dynamicInventoryDetails = new Dictionary<ulong, DynamicEconDetails>();

		public bool assignedPro;
		public bool assignedAdmin;

		public bool hasAuthentication;
		public bool hasProof;
		public bool hasGroup;

		public bool canAcceptYet => hasAuthentication && hasProof && hasGroup;

		private EPlayerSkillset _skillset;
		public EPlayerSkillset skillset => _skillset;

		private string _language;
		public string language => _language;

		//private bool _isAnonymous;
		//public bool isAnonymous
		//{
		//	get { return _isAnonymous; }
		//}

		public float lastReceivedPingRequestRealtime;

		private double sentVerifyPacketRealtime;
		private bool _hasSentVerifyPacket;

		public bool hasSentVerifyPacket => _hasSentVerifyPacket;

		public float realtimeSinceSentVerifyPacket => (float) (Time.realtimeSinceStartupAsDouble - sentVerifyPacketRealtime);

		public void sendVerifyPacket()
		{
			if (hasSentVerifyPacket)
				return;

			sentVerifyPacketRealtime = Time.realtimeSinceStartupAsDouble;
			_hasSentVerifyPacket = true;

			if (!playerID.steamID.IsValid())
				return; // Kind of hacky, CommandQueue allows adding dummy players to test the queue who have invalid ids.

			UnturnedLog.info($"Sending verification request to queued player {playerID}");
			NetMessages.SendMessageToClient(EClientMessage.Verify, ENetReliability.Reliable, transportConnection, (NetPakWriter writer) => { });
		}

		public CSteamID lobbyID
		{
			get;
			private set;
		}

		internal EClientPlatform clientPlatform;

		public void inventoryDetailsReady()
		{
			shirtItem = getInventoryItem(packageShirt);
			pantsItem = getInventoryItem(packagePants);
			hatItem = getInventoryItem(packageHat);
			backpackItem = getInventoryItem(packageBackpack);
			vestItem = getInventoryItem(packageVest);
			maskItem = getInventoryItem(packageMask);
			glassesItem = getInventoryItem(packageGlasses);

			List<int> skinItemsList = new List<int>();
			List<string> skinTagsList = new List<string>();
			List<string> skinDynamicPropsList = new List<string>();
			for (int index = 0; index < packageSkins.Length; index++)
			{
				ulong package = packageSkins[index];

				if (package != 0)
				{
					int item = getInventoryItem(package);

					if (item != 0)
					{
						skinItemsList.Add(item);

						DynamicEconDetails details;
						if (dynamicInventoryDetails.TryGetValue(package, out details))
						{
							skinTagsList.Add(details.tags);
							skinDynamicPropsList.Add(details.dynamic_props);
						}
						else
						{
							skinTagsList.Add(string.Empty);
							skinDynamicPropsList.Add(string.Empty);
						}
					}
				}
			}

			skinItems = skinItemsList.ToArray();
			skinTags = skinTagsList.ToArray();
			skinDynamicProps = skinDynamicPropsList.ToArray();

			hasProof = true;

			if (canAcceptYet)
			{
				SDG.Unturned.Provider.accept(this);
			}
		}

		public int getInventoryItem(ulong package)
		{
			if (inventoryDetails != null)
			{
				for (int index = 0; index < inventoryDetails.Length; index++)
				{
					if (inventoryDetails[index].m_itemId.m_SteamItemInstanceID == package)
					{
						return inventoryDetails[index].m_iDefinition.m_SteamItemDef;
					}
				}
			}

			return 0;
		}

		public SteamPending(ITransportConnection transportConnection, SteamPlayerID newPlayerID, bool newPro, byte newFace, byte newHair, byte newBeard, Color newSkin, Color newColor, Color newMarkerColor, Color newBeardColor, bool newHand, ulong newPackageShirt, ulong newPackagePants, ulong newPackageHat, ulong newPackageBackpack, ulong newPackageVest, ulong newPackageMask, ulong newPackageGlasses, ulong[] newPackageSkins, EPlayerSkillset newSkillset, string newLanguage, CSteamID newLobbyID, EClientPlatform clientPlatform)
		{
			this.transportConnection = transportConnection;

			_playerID = newPlayerID;
			_isPro = newPro;

			_face = newFace;
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

			packageShirt = newPackageShirt;
			packagePants = newPackagePants;
			packageHat = newPackageHat;
			packageBackpack = newPackageBackpack;
			packageVest = newPackageVest;
			packageMask = newPackageMask;
			packageGlasses = newPackageGlasses;
			packageSkins = newPackageSkins;

			lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
			sentVerifyPacketRealtime = -1.0f;

			lobbyID = newLobbyID;
			this.clientPlatform = clientPlatform;
		}

		public SteamPending()
		{
			_playerID = new SteamPlayerID(CSteamID.Nil, 0, "Player Name", "Character Name", "Nick Name", CSteamID.Nil);

			lastReceivedPingRequestRealtime = Time.realtimeSinceStartup;
			sentVerifyPacketRealtime = -1.0f;
		}

		/// <summary>
		/// Used when kicking player in queue to log what backend system might be failing.
		/// </summary>
		internal string GetQueueStateDebugString()
		{
			if (hasSentVerifyPacket)
			{
				if (canAcceptYet)
				{
					return $"ready to accept from queue, elapsed: {realtimeSinceSentVerifyPacket}s";
				}
				else
				{
					return $"hasAuthentication: {hasAuthentication} hasProof: {hasProof} hasGroup: {hasGroup} elapsed: {realtimeSinceSentVerifyPacket}s";
				}
			}
			else
			{
				return "normal waiting in queue";
			}
		}

		internal int lastNotifiedQueuePosition;
	}
}
