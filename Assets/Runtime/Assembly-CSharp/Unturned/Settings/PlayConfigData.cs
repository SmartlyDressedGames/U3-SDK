////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Reflection;
using Unturned.SystemEx;

namespace SDG.Unturned
{
	/// <summary>
	/// When this attribute is enabled a warning is shown in the server lobby.
	/// Applied to options which reduce accessibility.
	/// </summary>
	public class ConfigWarnIfTrueAttribute : System.Attribute
	{

	}

	public class ConfigData
	{
		public BrowserConfigData Browser;
		public ServerConfigData Server;
		public UnityEventConfigData UnityEvents;

		[System.Obsolete("Please update code to no longer reference this directly! The intention is to stop storing all three side-by-side in the future.")]
		public ModeConfigData Easy;
		[System.Obsolete("Please update code to no longer reference this directly! The intention is to stop storing all three side-by-side in the future.")]
		public ModeConfigData Normal;
		[System.Obsolete("Please update code to no longer reference this directly! The intention is to stop storing all three side-by-side in the future.")]
		public ModeConfigData Hard;

		private ConfigData()
		{
			Browser = new BrowserConfigData();
			Server = new ServerConfigData();
			UnityEvents = new UnityEventConfigData();
#pragma warning disable
			Easy = new ModeConfigData(EGameMode.EASY);
			Normal = new ModeConfigData(EGameMode.NORMAL);
			Hard = new ModeConfigData(EGameMode.HARD);
#pragma warning restore
		}

		public void InitSingleplayerDefaults()
		{
#pragma warning disable
			Easy.InitSingleplayerDefaults();
			Normal.InitSingleplayerDefaults();
			Hard.InitSingleplayerDefaults();
#pragma warning restore
		}

		public void InitDedicatedServerDefaults()
		{

		}

		public ModeConfigData getModeConfig(EGameMode mode)
		{
#pragma warning disable
			switch (mode)
			{
				case EGameMode.EASY:
					return Easy;
				case EGameMode.NORMAL:
					return Normal;
				case EGameMode.HARD:
					return Hard;
				default:
					return null;
			}
#pragma warning restore
		}

		public static ConfigData CreateDefault(bool singleplayer)
		{
			ConfigData instance = new ConfigData();
			if (singleplayer)
			{
				instance.InitSingleplayerDefaults();
			}
			else
			{
				instance.InitDedicatedServerDefaults();
			}
			return instance;
		}
	}

	public class BrowserConfigData
	{
		/// <summary>
		/// URL of a 64x64 image shown in the upper-left of the server lobby menu.
		/// </summary>
		public string Icon;

		/// <summary>
		/// URL of a 32x32 image shown in the server list.
		/// </summary>
		public string Thumbnail;

		/// <summary>
		/// Short description underneath the server name in the server lobby menu.
		/// </summary>
		public string Desc_Hint;

		/// <summary>
		/// Long description in the lower-right of the server lobby menu.
		/// </summary>
		public string Desc_Full;

		/// <summary>
		/// Short description underneath the server name in the server list.
		/// </summary>
		public string Desc_Server_List;

		/// <summary>
		/// Documentation: https://docs.smartlydressedgames.com/en/stable/servers/game-server-login-tokens.html
		/// To generate a new token visit: https://steamcommunity.com/dev/managegameservers
		/// </summary>
		public string Login_Token;

		/// <summary>
		/// IP address, DNS name, or a web address (to perform GET request) to advertise.
		///
		/// Servers not using Fake IP can specify just a DNS entry. This way if server's IP changes clients can rejoin.
		/// For example, if you own the "example.com" domain you could add an A record "myunturnedserver" pointing at
		/// your game server IP and set that record here "myunturnedserver.example.com".
		/// 
		/// Servers using Fake IP are assigned random ports at startup, but can implement a web API endpoint to return
		/// the IP and port. Clients perform a GET request if this string starts with http:// or https://. The returned
		/// text can be an IP address or DNS name with optional query port override. (e.g., "127.0.0.1:27015")
		///
		/// Documentation: https://docs.smartlydressedgames.com/en/stable/servers/bookmark-host.html
		/// </summary>
		public string BookmarkHost;

		/// <summary>
		/// If true, the server lobby warns that in-game ping may be higher than shown.
		/// </summary>
		public bool Is_Using_Anycast_Proxy;

		/// <summary>
		/// How the server is monetized (if at all).
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public EServerMonetizationTag Monetization;

		public struct Link : IDatParseable, IDatSerializable
		{
			public string Message;
			public string Url;

			/// <summary>
			/// Used to find overrides in json file.
			/// </summary>
			public override bool Equals(object other)
			{
				if (ReferenceEquals(other, null) || other is not Link otherLink)
				{
					return false;
				}

				return string.Equals(Message, otherLink.Message) && string.Equals(Url, otherLink.Url);
			}

			public override int GetHashCode()
			{
				return System.HashCode.Combine(Message, Url);
			}

			public bool TryParse(IDatNode node)
			{
				if (node is IDatDictionary dictionary)
				{
					Message = dictionary.GetString("Message");
					Url = dictionary.GetString("URL");
					return true;
				}

				return false;
			}

			public void SerializeIntoDictionary(IEditableDatDictionary dictionary)
			{
				dictionary.AddValue("Message").SetString(Message);
				dictionary.AddValue("URL").SetString(Url);
			}
		}

		/// <summary>
		/// Buttons shown in the server lobby menu. For example:
		/// ` Links
		/// ` [
		/// `     {
		/// `         Message Visit our website!
		/// `         URL https://smartlydressedgames.com/
		/// `     }
		/// ` ]
		/// </summary>
		public Link[] Links;

		public BrowserConfigData()
		{
			Icon = string.Empty;
			Thumbnail = string.Empty;
			Desc_Hint = string.Empty;
			Desc_Full = string.Empty;
			Desc_Server_List = string.Empty;
			Login_Token = string.Empty;
			BookmarkHost = string.Empty;
			Monetization = EServerMonetizationTag.Unspecified;
			Links = null;
		}
	}

	public partial class ServerConfigData
	{
		/// <summary>
		/// Whether to enable Valve Anti-Cheat.
		/// </summary>
		public bool VAC_Secure;

		/// <summary>
		/// Players with a ping higher than this are kicked.
		/// </summary>
		public uint Max_Ping_Milliseconds;

		/// <summary>
		/// Players in the pre-join queue we haven't heard from in this past number of seconds are kicked.
		/// </summary>
		public float Timeout_Queue_Seconds;

		/// <summary>
		/// Players in the server we haven't heard from in this past number of seconds are kicked.
		/// </summary>
		public float Timeout_Game_Seconds;

		[System.Obsolete]
		[System.NonSerialized]
		public float Max_Packets_Per_Second;

		/// <summary>
		/// If ready-to-connect messages are received more than twice from the same client in less than this many
		/// seconds they will be kicked.
		/// </summary>
		public float Join_Rate_Limit_Window_Seconds;

		/// <summary>
		/// If bad packets (that *may* be legitimate) are received more than threshold times within this many seconds
		/// of each other, reject the calling connection.
		/// </summary>
		public float Bad_Packet_Rate_Limit_Window_Seconds;

		/// <summary>
		/// If more than this many bad packets (that *may* be legitimate) are received within window seconds of each
		/// other, reject the calling connection.
		/// </summary>
		public int Bad_Packet_Rate_Limit_Threshold;

		/// <summary>
		/// If a rate-limited method is called this many times within cooldown window the client will be kicked.
		/// For example a value of 1 means the client will be kicked the first time they call the method off-cooldown. (not recommended)
		/// </summary>
		public int Rate_Limit_Kick_Threshold;

		/// <summary>
		/// Only applicable when Fake IP is off. When a client is connecting, if their connection would push the number
		/// of simultaneous connections from the same IP address past this number, they are prevented from joining.
		///
		/// May be useful to prevent against fake join requests coming from a single source IP. (public issue #5001)
		/// 
		/// Defaults to a high value because some regions will have many more clients with the same IPv4 address than
		/// others. For example, due to Carrier-grade NAT (CGNAT).
		/// </summary>
		public int Max_Clients_With_Same_IP_Address = 64;

		/// <summary>
		/// Whether rejections for Max_Clients_With_Same_IP_Address should log to command output. Useful for checking
		/// if the limit is appropriate.
		/// </summary>
		public bool Max_Clients_With_Same_IP_Address_Log_Warnings = true;

		/// <summary>
		/// Ordinarily the server should be receiving multiple input packets per second from a client. If more than this
		/// amount of time passes between input packets we flag the client as potentially using a lag switch, and modify
		/// their stats (e.g. reduce player damage) for a corresponding duration.
		/// </summary>
		public float Fake_Lag_Threshold_Seconds;

		/// <summary>
		/// Whether fake lag detection should log to command output. False positives are relatively likely when client
		/// framerate hitches (e.g. loading dense region), so this is best used for tuning threshold rather than bans.
		/// </summary>
		public bool Fake_Lag_Log_Warnings;

		/// <summary>
		/// PvP damage multiplier while under fake lag penalty.
		/// </summary>
		public float Fake_Lag_Damage_Penalty_Multiplier;

		/// <summary>
		/// Should we kick players after detecting spammed calls to askInput?
		/// </summary>
		public bool Enable_Kick_Input_Spam;

		/// <summary>
		/// Should we kick players if they do not submit inputs for a long time?
		/// </summary>
		public bool Enable_Kick_Input_Timeout;

		/// <summary>
		/// Should the server automatically shutdown at a configured time?
		/// </summary>
		public bool Enable_Scheduled_Shutdown;

		/// <summary>
		/// When the server should shutdown if Enable_Scheduled_Shutdown is true.
		/// </summary>
		public string Scheduled_Shutdown_Time = "1:30 am";

		/// <summary>
		/// Broadcast "shutting down for scheduled maintenance" warnings at these intervals.
		/// Format is a list of hours:minutes:seconds, for example to warn only 5 seconds before:
		/// ` Scheduled_Shutdown_Warnings
		/// ` [
		///	`     00:00:05
		/// ` ]
		/// Default starts at 30 minutes and counts down.
		/// </summary>
		public string[] Scheduled_Shutdown_Warnings = new string[]
		{
			"00:30:00",
			"00:15:00",
			"00:05:00",
			"00:01:00",
			"00:00:30",
			"00:00:15",
			"00:00:03",
			"00:00:02",
			"00:00:01",
		};

		/// <summary>
		/// Should the server automatically shutdown when a new version is detected?
		/// </summary>
		public bool Enable_Update_Shutdown;

		/// <summary>
		/// If Enable_Update_Shutdown is true, we check for updates to this branch of the game.
		/// (Unfortunately the server does not have a way to automatically determine the current beta branch.)
		/// </summary>
		public string Update_Steam_Beta_Name = "public";

		/// <summary>
		/// Broadcast "shutting down for update" warnings at these intervals.
		/// Refer to Scheduled_Shutdown_Warnings for an explanation of the format.
		/// Default starts at 3 minutes and counts down.
		/// </summary>
		public string[] Update_Shutdown_Warnings = new string[]
		{
			"00:03:00",
			"00:01:00",
			"00:00:30",
			"00:00:15",
			"00:00:03",
			"00:00:02",
			"00:00:01",
		};

		/// <summary>
		/// Should vanilla text chat messages always use rich text?
		/// Servers with plugins may want to enable because IMGUI does not fade out rich text.
		/// Kept because plugins might be setting this directly, but it no longer does anything.
		/// </summary>
		[System.Obsolete("uGUI supports rich text fade out.")]
		[System.NonSerialized]
		public bool Chat_Always_Use_Rich_Text;

		/// <summary>
		/// Should the EconInfo.json hash be checked by the server?
		/// </summary>
		public bool Validate_EconInfo_Hash;

		/// <summary>
		/// Documentation: https://docs.smartlydressedgames.com/en/stable/servers/fake-ip.html
		/// </summary>
		public bool Use_FakeIP;

		/// <summary>
		/// If greater than zero, vehicles with XZ position outside this threshold are saved in the center of the map.
		/// By default, vehicles outside ±40 km are teleported into the map.
		/// Intended to help with physics issues caused by vehicles far out in space. (public issue #4465)
		/// </summary>
		public float Reset_Vehicles_Outside_Horizontal_Distance = 40000.0f;

		public ServerConfigData()
		{
			VAC_Secure = true;

			Max_Ping_Milliseconds = 750;
			Timeout_Queue_Seconds = 15;
			Timeout_Game_Seconds = 30;
			Join_Rate_Limit_Window_Seconds = 40.0f;
			Bad_Packet_Rate_Limit_Window_Seconds = 2.5f;
			Bad_Packet_Rate_Limit_Threshold = 10;
			Rate_Limit_Kick_Threshold = 10;
			Fake_Lag_Threshold_Seconds = 3.0f;
			Fake_Lag_Damage_Penalty_Multiplier = 0.1f;

			Enable_Kick_Input_Spam = false;
			Enable_Kick_Input_Timeout = false;

			Validate_EconInfo_Hash = true;
		}

		internal float GetClampedTimeoutQueueSeconds()
		{
			return UnityEngine.Mathf.Clamp(Timeout_Queue_Seconds, 1.0f, MAX_TIMEOUT_QUEUE_SECONDS);
		}

		/// <summary>
		/// Limit max queue timeout duration so that if server encounters an error or doesn't
		/// process the request the client can timeout locally.
		/// </summary>
		internal const float MAX_TIMEOUT_QUEUE_SECONDS = 25.0f;
		/// <summary>
		/// Longer than server timeout so that ideally more context is logged on the server
		/// rather than just "client disconnected."
		/// </summary>
		internal const float CLIENT_TIMEOUT_QUEUE_SECONDS = MAX_TIMEOUT_QUEUE_SECONDS + 5.0f;
	}

	public class ModeConfigData
	{
		public ItemsConfigData Items;
		public VehiclesConfigData Vehicles;
		public ZombiesConfigData Zombies;
		public AnimalsConfigData Animals;
		public BarricadesConfigData Barricades;
		public StructuresConfigData Structures;
		public PlayersConfigData Players;
		public ObjectConfigData Objects;
		public EventsConfigData Events;
		public GameplayConfigData Gameplay;

		public ModeConfigData(EGameMode mode)
		{
			Items = new ItemsConfigData(mode);
			Vehicles = new VehiclesConfigData(mode);
			Zombies = new ZombiesConfigData(mode);
			Animals = new AnimalsConfigData(mode);
			Barricades = new BarricadesConfigData(mode);
			Structures = new StructuresConfigData(mode);
			Players = new PlayersConfigData(mode);
			Objects = new ObjectConfigData(mode);
			Events = new EventsConfigData(mode);
			Gameplay = new GameplayConfigData(mode);
		}

		public void InitSingleplayerDefaults()
		{
			Players.InitSingleplayerDefaults();
			Gameplay.InitSingleplayerDefaults();
		}

		public static ModeConfigData CreateDefault(EGameMode mode, bool singleplayer)
		{
			ModeConfigData modeConfigData = new ModeConfigData(mode);
			if (singleplayer)
			{
				modeConfigData.InitSingleplayerDefaults();
			}
			return modeConfigData;
		}
	}

	public class ItemsConfigData
	{
		/// <summary>
		/// Percentage [0 to 1] of item spawns to use.
		/// For example, if set to 0.2 and level has 100 item spawns, max 20 items will spawn at a time.
		/// </summary>
		public float Spawn_Chance;

		/// <summary>
		/// How long (in seconds) before an item dropped by a player is despawned.
		/// </summary>
		public float Despawn_Dropped_Time;

		/// <summary>
		/// How long (in seconds) before a spawned item is despawned.
		/// (For example, an item nobody wants to pick up.)
		/// </summary>
		public float Despawn_Natural_Time;

		/// <summary>
		/// When less than the target amount of items are dropped (determined by Spawn_Chance), a new
		/// item is spawned approximately this often (in seconds).
		/// </summary>
		public float Respawn_Time;

		/// <summary>
		/// Percentage [0 to 1] probability of item spawning at max quality.
		/// </summary>
		public float Quality_Full_Chance;

		/// <summary>
		/// When an item spawns without max quality, the random quality is scaled by this factor.
		/// For example, 0.5 halves the initial quality.
		/// </summary>
		public float Quality_Multiplier;

		/// <summary>
		/// Percentage [0 to 1] probability of gun spawning with full ammo.
		/// </summary>
		public float Gun_Bullets_Full_Chance;

		/// <summary>
		/// When a gun spawns without full ammo, the random amount is scaled by this factor.
		/// </summary>
		public float Gun_Bullets_Multiplier;

		/// <summary>
		/// Percentage [0 to 1] probability of magazines spawning with full ammo.
		/// </summary>
		public float Magazine_Bullets_Full_Chance;

		/// <summary>
		/// When a magazine spawns without full ammo, the random amount is scaled by this factor.
		/// </summary>
		public float Magazine_Bullets_Multiplier;

		/// <summary>
		/// Percentage [0 to 1] probability of non-magazines spawning with full amount.
		/// (E.g., ammo boxes.)
		/// </summary>
		public float Crate_Bullets_Full_Chance;

		/// <summary>
		/// When a non-magazine spawns without full amount, the random amount is scaled by this factor.
		/// (E.g., ammo boxes.)
		/// </summary>
		public float Crate_Bullets_Multiplier;

		/// <summary>
		/// Original option for disabling item quality. If false, items spawn at 100% quality and
		/// their quality doesn't decrease. For backwards compatibility, the newer per-item-type
		/// durability options are ignored if this is off.
		/// </summary>
		public bool Has_Durability;

		/// <summary>
		/// Food-specific replacement for Has_Durability. If true, food spawns at 100% quality.
		/// </summary>
		public bool Food_Spawns_At_Full_Quality;

		/// <summary>
		/// Water-specific replacement for Has_Durability. If true, water spawns at 100% quality.
		/// </summary>
		public bool Water_Spawns_At_Full_Quality;

		/// <summary>
		/// Clothing-specific replacement for Has_Durability. If true, clothing spawns at 100% quality.
		/// </summary>
		public bool Clothing_Spawns_At_Full_Quality;

		/// <summary>
		/// Weapon-specific replacement for Has_Durability. If true, weapons spawns at 100% quality.
		/// </summary>
		public bool Weapons_Spawn_At_Full_Quality;

		/// <summary>
		/// Fallback used when spawning an item that doesn't fit into one of the other quality/durability settings.
		/// If true, items spawn at 100% quality.
		/// </summary>
		public bool Default_Spawns_At_Full_Quality;

		/// <summary>
		/// Clothing-specific replacement for Has_Durability. If false, clothing quality
		/// doesn't decrease when damaged.
		/// </summary>
		public bool Clothing_Has_Durability;

		/// <summary>
		/// Melee and gun replacement for Has_Durability. If false, weapons quality
		/// doesn't decrease when used.
		/// </summary>
		public bool Weapons_Have_Durability;

		public ItemsConfigData(EGameMode mode)
		{
			Despawn_Dropped_Time = 600.0f;
			Despawn_Natural_Time = 900.0f;

			switch (mode)
			{
				case EGameMode.EASY:
					Spawn_Chance = 0.35f;
					Respawn_Time = 50.0f;

					Quality_Full_Chance = 0.1f;
					Quality_Multiplier = 1.0f;

					Gun_Bullets_Full_Chance = 0.1f;
					Gun_Bullets_Multiplier = 1.0f;
					Magazine_Bullets_Full_Chance = 0.1f;
					Magazine_Bullets_Multiplier = 1.0f;
					Crate_Bullets_Full_Chance = 0.1f;
					Crate_Bullets_Multiplier = 1.0f;
					break;
				case EGameMode.NORMAL:
					Spawn_Chance = 0.35f;
					Respawn_Time = 100.0f;

					Quality_Full_Chance = 0.1f;
					Quality_Multiplier = 1.0f;

					Gun_Bullets_Full_Chance = 0.05f;
					Gun_Bullets_Multiplier = 0.25f;
					Magazine_Bullets_Full_Chance = 0.05f;
					Magazine_Bullets_Multiplier = 0.5f;
					Crate_Bullets_Full_Chance = 0.05f;
					Crate_Bullets_Multiplier = 1.0f;
					break;
				case EGameMode.HARD:
					Spawn_Chance = 0.15f;
					Respawn_Time = 150.0f;

					Quality_Full_Chance = 0.01f;
					Quality_Multiplier = 1.0f;

					Gun_Bullets_Full_Chance = 0.025f;
					Gun_Bullets_Multiplier = 0.1f;
					Magazine_Bullets_Full_Chance = 0.025f;
					Magazine_Bullets_Multiplier = 0.25f;
					Crate_Bullets_Full_Chance = 0.025f;
					Crate_Bullets_Multiplier = 0.75f;
					break;
				default:
					Spawn_Chance = 1.0f;
					Respawn_Time = 1000000.0f;

					Quality_Full_Chance = 1.0f;
					Quality_Multiplier = 1.0f;

					Gun_Bullets_Full_Chance = 1.0f;
					Gun_Bullets_Multiplier = 1.0f;
					Magazine_Bullets_Full_Chance = 1.0f;
					Magazine_Bullets_Multiplier = 1.0f;
					Crate_Bullets_Full_Chance = 1.0f;
					Crate_Bullets_Multiplier = 1.0f;
					break;
			}

			switch (mode)
			{
				case EGameMode.EASY:
					Has_Durability = false;
					Food_Spawns_At_Full_Quality = true;
					Water_Spawns_At_Full_Quality = true;
					Clothing_Spawns_At_Full_Quality = true;
					Weapons_Spawn_At_Full_Quality = true;
					Default_Spawns_At_Full_Quality = true;
					Clothing_Has_Durability = false;
					Weapons_Have_Durability = false;
					break;

				default:
					Has_Durability = true;
					Food_Spawns_At_Full_Quality = false;
					Water_Spawns_At_Full_Quality = false;
					Clothing_Spawns_At_Full_Quality = false;
					Weapons_Spawn_At_Full_Quality = false;
					Default_Spawns_At_Full_Quality = false;
					Clothing_Has_Durability = true;
					Weapons_Have_Durability = true;
					break;
			}
		}

		internal bool ShouldClothingTakeDamage
		{
			get
			{
				if (!Has_Durability)
				{
					return false;
				}

				return Clothing_Has_Durability;
			}
		}

		internal bool ShouldWeaponTakeDamage
		{
			get
			{
				if (!Has_Durability)
				{
					return false;
				}

				return Weapons_Have_Durability;
			}
		}
	}

	public class VehiclesConfigData
	{
		/// <summary>
		/// Seconds vehicle can be neglected before it begins taking damage.
		/// </summary>
		public float Decay_Time;

		/// <summary>
		/// After vehicle has been neglected for more than Decay_Time seconds it will begin taking this much damage per second.
		/// </summary>
		public float Decay_Damage_Per_Second;

		/// <summary>
		/// Percentage [0 to 1] probability of spawning with a battery.
		/// </summary>
		public float Has_Battery_Chance;

		/// <summary>
		/// Percentage [0 to 1] minimum initial charge if spawning with a battery.
		/// </summary>
		public float Min_Battery_Charge;

		/// <summary>
		/// Percentage [0 to 1] maximum initial charge if spawning with a battery.
		/// </summary>
		public float Max_Battery_Charge;

		/// <summary>
		/// Percentage [0 to 1] probability of spawning with a tire per-wheel.
		/// </summary>
		public float Has_Tire_Chance;

		/// <summary>
		/// How long (in seconds) after vehicle explodes or gets stuck underwater before it despawns.
		/// </summary>
		public float Respawn_Time;

		/// <summary>
		/// How long (in seconds) a locked vehicle can sit empty in the safezone before it is
		/// automatically unlocked.
		/// </summary>
		public float Unlocked_After_Seconds_In_Safezone;

		/// <summary>
		/// Scales the amount of damage taken by vehicles.
		/// For example, 0.5 halves the amount of damage dealt to vehicles.
		/// </summary>
		public float Armor_Multiplier;

		/// <summary>
		/// Scales damage to the vehicle when an attached barricade obstructions an explosion.
		/// For example, 0.5 halves the explosion damage when blocked by a barricade.
		/// </summary>
		public float Child_Explosion_Armor_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by vehicles from non-"Heavy Weapon" guns.
		/// For example, 2.0 doubles the amount of damage dealt to vehicles by non-"Heavy Weapon" guns.
		/// </summary>
		public float Gun_Lowcal_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by vehicles from "Heavy Weapon" guns.
		/// For example, 2.0 doubles the amount of damage dealt to vehicles by "Heavy Weapon" guns.
		/// </summary>
		public float Gun_Highcal_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by vehicles from melee weapons and fists.
		/// For example, 2.0 doubles the amount of damage dealt to vehicles by melee.
		/// </summary>
		public float Melee_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of HP restored by melee items like the Blowtorch.
		/// For example, 2.0 doubles the amount of health restored by melee items.
		/// </summary>
		public float Melee_Repair_Multiplier;

		/// <summary>
		/// Maximum number of naturally-spawned vehicles on "Tiny" size levels.
		/// </summary>
		public uint Max_Instances_Tiny;

		/// <summary>
		/// Maximum number of naturally-spawned vehicles on "Small" size levels.
		/// </summary>
		public uint Max_Instances_Small;

		/// <summary>
		/// Maximum number of naturally-spawned vehicles on "Medium" size levels.
		/// </summary>
		public uint Max_Instances_Medium;

		/// <summary>
		/// Maximum number of naturally-spawned vehicles on "Large" size levels.
		/// </summary>
		public uint Max_Instances_Large;

		/// <summary>
		/// Maximum number of naturally-spawned vehicles on "Insane" size levels.
		/// </summary>
		public uint Max_Instances_Insane;

		/// <summary>
		/// Vehicles are considered "natural" if they were spawned by the level as opposed to players or vendors.
		/// If less than this many natural vehicles exist in the level, more will be spawned. The minimum of this or
		/// Max_Instances is used. (I.e., if this value is higher than max instances the max instances value is used
		/// instead.)
		/// </summary>
		public uint Min_Natural_Vehicles;

		public VehiclesConfigData(EGameMode mode)
		{
			Decay_Time = 604800.0f; // 60 * 60 * 24 * 7
			Decay_Damage_Per_Second = 0.1f;

			Has_Battery_Chance = 0.8f;
			Min_Battery_Charge = 0.5f;
			Max_Battery_Charge = 0.75f;

			switch (mode)
			{
				case EGameMode.EASY:
					Has_Battery_Chance = 1;
					Min_Battery_Charge = 0.8f;
					Max_Battery_Charge = 1;
					Has_Tire_Chance = 1;
					break;

				case EGameMode.NORMAL:
					Has_Battery_Chance = 0.8f;
					Min_Battery_Charge = 0.5f;
					Max_Battery_Charge = 0.75f;
					Has_Tire_Chance = 0.85f;
					break;

				case EGameMode.HARD:
					Has_Battery_Chance = 0.25f;
					Min_Battery_Charge = 0.1f;
					Max_Battery_Charge = 0.3f;
					Has_Tire_Chance = 0.7f;
					break;

				default:
					Has_Battery_Chance = 1;
					Min_Battery_Charge = 1;
					Max_Battery_Charge = 1;
					Has_Tire_Chance = 1;
					break;
			}

			Respawn_Time = 300.0f;
			Unlocked_After_Seconds_In_Safezone = 3600.0f;
			Armor_Multiplier = 1.0f;
			Child_Explosion_Armor_Multiplier = 1.0f;
			Gun_Lowcal_Damage_Multiplier = 1.0f;
			Gun_Highcal_Damage_Multiplier = 1.0f;
			Melee_Damage_Multiplier = 1.0f;
			Melee_Repair_Multiplier = 1.0f;

			Max_Instances_Tiny = 4;
			Max_Instances_Small = 8;
			Max_Instances_Medium = 16;
			Max_Instances_Large = 32;
			Max_Instances_Insane = 64;
			Min_Natural_Vehicles = 16;
		}
	}

	public class ZombiesConfigData
	{
		/// <summary>
		/// Percentage [0 to 1] of zombie spawns to use.
		/// For example, if set to 0.2 and an area has 100 zombie spawns, max 20 zombies will spawn at a time.
		/// </summary>
		public float Spawn_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie dropping an item except when dropping more than one item.
		/// </summary>
		public float Loot_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a crawler.
		/// </summary>
		public float Crawler_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a sprinter.
		/// </summary>
		public float Sprinter_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a flanker.
		/// </summary>
		public float Flanker_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a burner.
		/// </summary>
		public float Burner_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as an acid spitter.
		/// </summary>
		public float Acid_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as an electric boss.
		/// </summary>
		public float Boss_Electric_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a ground-pounding boss.
		/// </summary>
		public float Boss_Wind_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a fire-breathing boss.
		/// </summary>
		public float Boss_Fire_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a ghost.
		/// </summary>
		public float Spirit_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a Dying Light Volatile (crossover).
		/// </summary>
		public float DL_Red_Volatile_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as a Dying Light Volatile (crossover).
		/// </summary>
		public float DL_Blue_Volatile_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as the Elver final boss.
		/// </summary>
		public float Boss_Elver_Stomper_Chance;

		/// <summary>
		/// Percentage [0 to 1] chance of zombie spawning as the Kuwait final boss.
		/// </summary>
		public float Boss_Kuwait_Chance;

		/// <summary>
		/// How long (in seconds) before a dead zombie respawns by default.
		/// </summary>
		public float Respawn_Day_Time;

		/// <summary>
		/// How long (in seconds) before a dead zombie respawns during a full moon.
		/// </summary>
		public float Respawn_Night_Time;

		/// <summary>
		/// How long (in seconds) before a dead zombie respawns during a horde beacon.
		/// </summary>
		public float Respawn_Beacon_Time;

		/// <summary>
		/// Minimum seconds between boss zombie spawns for players doing quests.
		/// Players were abusing the spawns to farm boss tier loot.
		/// </summary>
		public float Quest_Boss_Respawn_Interval;

		/// <summary>
		/// Scales the amount of damage dealt by zombies.
		/// For example, 2.0 doubles the amount of damage from zombie attacks.
		/// </summary>
		public float Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by zombies.
		/// For example, 0.5 halves the amount of damage dealt to zombies.
		/// </summary>
		public float Armor_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by zombies when attacked from behind.
		/// Only certain weapons quality for this modifier.
		/// </summary>
		public float Backstab_Multiplier;

		/// <summary>
		/// Weapon damage multiplier against body, arms, legs. Useful for headshot-only mode.
		/// </summary>
		public float NonHeadshot_Armor_Multiplier;

		/// <summary>
		/// Scales amount of XP gained for killing a zombie during a horde beacon.
		/// </summary>
		public float Beacon_Experience_Multiplier;

		/// <summary>
		/// Scales amount of XP gained for killing a zombie during the full moon.
		/// </summary>
		public float Full_Moon_Experience_Multiplier;

		/// <summary>
		/// Minimum number of loot drops from non-mega non-boss zombies.
		/// Loot_Chance applies if the rolled number of drops between [min, max] is one.
		/// </summary>
		public uint Min_Drops;

		/// <summary>
		/// Maximum number of loot drops from non-mega non-boss zombies.
		/// </summary>
		public uint Max_Drops;

		/// <summary>
		/// Minimum number of loot drops from non-boss mega zombies.
		/// Loot_Chance applies if the rolled number of drops between [min, max] is one.
		/// </summary>
		public uint Min_Mega_Drops;

		/// <summary>
		/// Maximum number of loot drops from non-boss mega zombies.
		/// </summary>
		public uint Max_Mega_Drops;

		/// <summary>
		/// Minimum number of loot drops from boss zombies.
		/// Loot_Chance applies if the rolled number of drops between [min, max] is one.
		/// </summary>
		public uint Min_Boss_Drops;

		/// <summary>
		/// Maximum number of loot drops from boss zombies.
		/// </summary>
		public uint Max_Boss_Drops;

		/// <summary>
		/// If true, all zombies are a bit slower, making it easier to escape them.
		/// </summary>
		public bool Slow_Movement;

		/// <summary>
		/// If false, nothing can stun zombies, making combat harder.
		/// </summary>
		public bool Can_Stun;

		/// <summary>
		/// If true, only certain weapons and attacks can stun zombie (e.g., backstabs).
		/// Not applicable if Can_Stun is false.
		/// </summary>
		public bool Only_Critical_Stuns;

		/// <summary>
		/// If true, attacking a zombie uses the weapon's PvP damage values rather than zombie-specific damage.
		/// </summary>
		public bool Weapons_Use_Player_Damage;

		/// <summary>
		/// If true, zombies will attack barricades obstructing their movement.
		/// </summary>
		public bool Can_Target_Barricades;

		/// <summary>
		/// If true, zombies will attack structures obstructing their movement.
		/// </summary>
		public bool Can_Target_Structures;

		/// <summary>
		/// If true, zombies will attack vehicles obstructing their movement.
		/// </summary>
		public bool Can_Target_Vehicles;

		/// <summary>
		/// If true, zombies will attack level objects (e.g., fences) obstructing their movement.
		/// </summary>
		public bool Can_Target_Objects;

		/// <summary>
		/// If greater than zero, maximum number of items a horde beacon can drop.
		/// Useful to clamp the number of drops when a large number of players participate.
		/// </summary>
		public uint Beacon_Max_Rewards;

		/// <summary>
		/// If greater than zero, maximum player count for horde beacon loot scaling.
		/// Useful to clamp the number of drops when a large number of players participate.
		/// </summary>
		public uint Beacon_Max_Participants;

		/// <summary>
		/// Scales total number of horde beacon loot drops, applied before Beacon_Max_Rewards.
		/// </summary>
		public float Beacon_Rewards_Multiplier;

		public ZombiesConfigData(EGameMode mode)
		{
			Respawn_Day_Time = 360.0f;
			Respawn_Night_Time = 30.0f;
			Respawn_Beacon_Time = 0.0f;
			Quest_Boss_Respawn_Interval = 600f;

			switch (mode)
			{
				case EGameMode.EASY:
					Spawn_Chance = 0.2f;
					Loot_Chance = 0.55f;
					Crawler_Chance = 0.0f;
					Sprinter_Chance = 0.0f;
					Flanker_Chance = 0.0f;
					Burner_Chance = 0.0f;
					Acid_Chance = 0.0f;
					break;
				case EGameMode.NORMAL:
					Spawn_Chance = 0.25f;
					Loot_Chance = 0.5f;
					Crawler_Chance = 0.15f;
					Sprinter_Chance = 0.15f;
					Flanker_Chance = 0.025f;
					Burner_Chance = 0.025f;
					Acid_Chance = 0.025f;
					break;
				case EGameMode.HARD:
					Spawn_Chance = 0.3f;
					Loot_Chance = 0.3f;
					Crawler_Chance = 0.125f;
					Sprinter_Chance = 0.175f;
					Flanker_Chance = 0.05f;
					Burner_Chance = 0.05f;
					Acid_Chance = 0.05f;
					break;
				default:
					Spawn_Chance = 1.0f;
					Loot_Chance = 0.0f;
					Crawler_Chance = 0.0f;
					Sprinter_Chance = 0.0f;
					Flanker_Chance = 0.0f;
					Burner_Chance = 0.0f;
					Acid_Chance = 0.0f;
					break;
			}

			Boss_Electric_Chance = 0.0f;
			Boss_Wind_Chance = 0.0f;
			Boss_Fire_Chance = 0.0f;
			Spirit_Chance = 0.0f;
			DL_Red_Volatile_Chance = 0.0f;
			DL_Blue_Volatile_Chance = 0.0f;
			Boss_Elver_Stomper_Chance = 0.0f;
			Boss_Kuwait_Chance = 0.0f;

			switch (mode)
			{
				case EGameMode.EASY:
					Damage_Multiplier = 0.75f;
					Armor_Multiplier = 1.25f;
					break;
				case EGameMode.HARD:
					Damage_Multiplier = 1.5f;
					Armor_Multiplier = 0.75f;
					break;
				default:
					Damage_Multiplier = 1.0f;
					Armor_Multiplier = 1.0f;
					break;
			}

			Backstab_Multiplier = 1.25f;
			NonHeadshot_Armor_Multiplier = 1.0f;

			Beacon_Experience_Multiplier = 1.0f;
			Full_Moon_Experience_Multiplier = 2.0f;

			Min_Drops = 1;
			Max_Drops = 1;
			Min_Mega_Drops = 5;
			Max_Mega_Drops = 5;
			Min_Boss_Drops = 8;
			Max_Boss_Drops = 10;

			Slow_Movement = mode == EGameMode.EASY;
			Can_Stun = mode != EGameMode.HARD;
			Only_Critical_Stuns = mode == EGameMode.HARD;
			Weapons_Use_Player_Damage = mode == EGameMode.HARD;

			Can_Target_Barricades = true;
			Can_Target_Structures = true;
			Can_Target_Vehicles = true;
			Can_Target_Objects = true;

			Beacon_Max_Rewards = 0;
			Beacon_Max_Participants = 0;
			Beacon_Rewards_Multiplier = 1.0f;
		}
	}

	public class AnimalsConfigData
	{
		/// <summary>
		/// How long (in seconds) before a dead animal respawns.
		/// </summary>
		public float Respawn_Time;

		/// <summary>
		/// Scales the amount of damage dealt by animals.
		/// For example, 2.0 doubles the amount of damage from animal attacks.
		/// </summary>
		public float Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by animals.
		/// For example, 0.5 halves the amount of damage dealt to animals.
		/// </summary>
		public float Armor_Multiplier;

		/// <summary>
		/// Maximum number of animals on "Tiny" size levels.
		/// </summary>
		public uint Max_Instances_Tiny;

		/// <summary>
		/// Maximum number of animals on "Small" size levels.
		/// </summary>
		public uint Max_Instances_Small;

		/// <summary>
		/// Maximum number of animals on "Medium" size levels.
		/// </summary>
		public uint Max_Instances_Medium;

		/// <summary>
		/// Maximum number of animals on "Large" size levels.
		/// </summary>
		public uint Max_Instances_Large;

		/// <summary>
		/// Maximum number of animals on "Insane" size levels.
		/// </summary>
		public uint Max_Instances_Insane;

		/// <summary>
		/// If true, attacking an animal uses the weapon's PvP damage values rather than animal-specific damage.
		/// </summary>
		public bool Weapons_Use_Player_Damage;

		public AnimalsConfigData(EGameMode mode)
		{
			Respawn_Time = 180.0f;

			switch (mode)
			{
				case EGameMode.EASY:
					Damage_Multiplier = 0.75f;
					Armor_Multiplier = 1.25f;
					break;
				case EGameMode.HARD:
					Damage_Multiplier = 1.5f;
					Armor_Multiplier = 0.75f;
					break;
				default:
					Damage_Multiplier = 1.0f;
					Armor_Multiplier = 1.0f;
					break;
			}

			Max_Instances_Tiny = 4;
			Max_Instances_Small = 8;
			Max_Instances_Medium = 16;
			Max_Instances_Large = 32;
			Max_Instances_Insane = 64;

			Weapons_Use_Player_Damage = mode == EGameMode.HARD;
		}
	}

	public class BarricadesConfigData
	{
		/// <summary>
		/// How long (in seconds) since the barricade owner/group last played before the barricade won't be saved.
		/// If the server is offline for more than half the Decay_Time, all decay timers are reset.
		/// </summary>
		public uint Decay_Time;

		/// <summary>
		/// Scales the amount of damage taken by "Armor Tier: Low" barricades.
		/// For example, 0.5 halves the amount of damage dealt to barricades.
		/// </summary>
		public float Armor_Lowtier_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by "Armor Tier: High" barricades.
		/// For example, 0.5 halves the amount of damage dealt to barricades.
		/// </summary>
		public float Armor_Hightier_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by barricades from non-"Heavy Weapon" guns.
		/// For example, 2.0 doubles the amount of damage dealt to barricades by non-"Heavy Weapon" guns.
		/// </summary>
		public float Gun_Lowcal_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by barricades from "Heavy Weapon" guns.
		/// For example, 2.0 doubles the amount of damage dealt to barricades by "Heavy Weapon" guns.
		/// </summary>
		public float Gun_Highcal_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by barricades from melee weapons and fists.
		/// For example, 2.0 doubles the amount of damage dealt to barricades by melee.
		/// </summary>
		public float Melee_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of HP restored by melee items like the Blowtorch.
		/// For example, 2.0 doubles the amount of health restored by melee items.
		/// </summary>
		public float Melee_Repair_Multiplier;

		/// <summary>
		/// Should players be allowed to build on their vehicles?
		/// </summary>
		public bool Allow_Item_Placement_On_Vehicle;

		/// <summary>
		/// Should players be allowed to build traps (e.g. barbed wire) on their vehicles?
		/// </summary>
		public bool Allow_Trap_Placement_On_Vehicle;

		/// <summary>
		/// Furthest away from colliders a player can build an item onto their vehicle.
		/// </summary>
		public float Max_Item_Distance_From_Hull;

		/// <summary>
		/// Furthest away from colliders a player can build a trap (e.g. barbed wire) onto their vehicle.
		/// </summary>
		public float Max_Trap_Distance_From_Hull;

		public float getArmorMultiplier(EArmorTier armorTier)
		{
			switch (armorTier)
			{
				default:
				case EArmorTier.LOW:
					return Armor_Lowtier_Multiplier;

				case EArmorTier.HIGH:
					return Armor_Hightier_Multiplier;
			}
		}

		public BarricadesConfigData(EGameMode mode)
		{
			Decay_Time = 60 * 60 * 24 * 7;

			Armor_Lowtier_Multiplier = 1.0f;
			Armor_Hightier_Multiplier = 0.5f;
			Gun_Lowcal_Damage_Multiplier = 1.0f;
			Gun_Highcal_Damage_Multiplier = 1.0f;
			Melee_Damage_Multiplier = 1.0f;
			Melee_Repair_Multiplier = 1.0f;

			Allow_Item_Placement_On_Vehicle = true;
			Allow_Trap_Placement_On_Vehicle = true;
			Max_Item_Distance_From_Hull = 64f;
			Max_Trap_Distance_From_Hull = 16f;
		}
	}

	public class StructuresConfigData
	{
		/// <summary>
		/// How long (in seconds) since the structure owner/group last played before the structure won't be saved.
		/// If the server is offline for more than half the Decay_Time, all decay timers are reset.
		/// </summary>
		public uint Decay_Time;

		/// <summary>
		/// Scales the amount of damage taken by "Armor Tier: Low" structures.
		/// For example, 0.5 halves the amount of damage dealt to structures.
		/// </summary>
		public float Armor_Lowtier_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by "Armor Tier: High" structures.
		/// For example, 0.5 halves the amount of damage dealt to structures.
		/// </summary>
		public float Armor_Hightier_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by structures from non-"Heavy Weapon" guns.
		/// For example, 2.0 doubles the amount of damage dealt to structures by non-"Heavy Weapon" guns.
		/// </summary>
		public float Gun_Lowcal_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by structures from "Heavy Weapon" guns.
		/// For example, 2.0 doubles the amount of damage dealt to structures by "Heavy Weapon" guns.
		/// </summary>
		public float Gun_Highcal_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of damage taken by structures from melee weapons and fists.
		/// For example, 2.0 doubles the amount of damage dealt to structures by melee.
		/// </summary>
		public float Melee_Damage_Multiplier;

		/// <summary>
		/// Scales the amount of HP restored by melee items like the Blowtorch.
		/// For example, 2.0 doubles the amount of health restored by melee items.
		/// </summary>
		public float Melee_Repair_Multiplier;

		public float getArmorMultiplier(EArmorTier armorTier)
		{
			switch (armorTier)
			{
				default:
				case EArmorTier.LOW:
					return Armor_Lowtier_Multiplier;

				case EArmorTier.HIGH:
					return Armor_Hightier_Multiplier;
			}
		}

		public StructuresConfigData(EGameMode mode)
		{
			Decay_Time = 60 * 60 * 24 * 7;

			Armor_Lowtier_Multiplier = 1.0f;
			Armor_Hightier_Multiplier = 0.5f;
			Gun_Lowcal_Damage_Multiplier = 1.0f;
			Gun_Highcal_Damage_Multiplier = 1.0f;
			Melee_Damage_Multiplier = 1.0f;
			Melee_Repair_Multiplier = 1.0f;
		}
	}

	public class PlayersConfigData
	{
		/// <summary>
		/// Amount of health players spawn with. [0 to 100]
		/// </summary>
		public uint Health_Default;

		/// <summary>
		/// Player must have more than this amount of food to begin regenerating health.
		/// </summary>
		public uint Health_Regen_Min_Food;

		/// <summary>
		/// Player must have more than this amount of water to begin regenerating health.
		/// </summary>
		public uint Health_Regen_Min_Water;

		/// <summary>
		/// How quickly players health regenerates with sufficient food and water.
		/// Lower values regenerate health faster, higher values regenerate health slower.
		/// </summary>
		public uint Health_Regen_Ticks;

		/// <summary>
		/// Amount of food players spawn with. [0 to 100]
		/// </summary>
		public uint Food_Default;

		/// <summary>
		/// How quickly players food meter depletes.
		/// Lower values burn food faster, higher values burn food slower.
		/// </summary>
		public uint Food_Use_Ticks;

		/// <summary>
		/// How quickly players starve to death.
		/// Lower values kill the player faster, higher values kill the player slower.
		/// </summary>
		public uint Food_Damage_Ticks;

		/// <summary>
		/// Amount of water players spawn with. [0 to 100]
		/// </summary>
		public uint Water_Default;

		/// <summary>
		/// How quickly players water meter depletes.
		/// Lower values lose water faster, higher values lose water slower.
		/// </summary>
		public uint Water_Use_Ticks;

		/// <summary>
		/// How quickly players dehydrate to death.
		/// Lower values kill the player faster, higher values kill the player slower.
		/// </summary>
		public uint Water_Damage_Ticks;

		/// <summary>
		/// Amount of immunity players spawn with. [0 to 100]
		/// </summary>
		public uint Virus_Default;

		/// <summary>
		/// When immunity is below this amount it will gradually begin depleting.
		/// </summary>
		public uint Virus_Infect;

		/// <summary>
		/// How quickly players immunity depletes when below Virus_Infect.
		/// Lower values deplete faster, higher values deplete slower.
		/// </summary>
		public uint Virus_Use_Ticks;

		/// <summary>
		/// How quickly players die at zero immunity.
		/// Lower values kill the player faster, higher values kill the player slower.
		/// </summary>
		public uint Virus_Damage_Ticks;

		/// <summary>
		/// How quickly broken legs heal automatically.
		/// Depends on Can_Fix_Legs.
		/// Lower values heal faster, higher values heal slower.
		/// </summary>
		public uint Leg_Regen_Ticks;

		/// <summary>
		/// How frequently players lose health while bleeding.
		/// Lower values kill the player faster, higher values kill the player slower.
		/// </summary>
		public uint Bleed_Damage_Ticks;

		/// <summary>
		/// How quickly bleeding heals automatically.
		/// Depends on Can_Stop_Bleeding.
		/// Lower values heal faster, higher values heal slower.
		/// </summary>
		public uint Bleed_Regen_Ticks;

		/// <summary>
		/// Scales the amount of damage taken by players.
		/// For example, 0.5 halves the amount of damage dealt to players.
		/// </summary>
		public float Armor_Multiplier;

		/// <summary>
		/// Scales the amount of XP gained from all activities.
		/// </summary>
		public float Experience_Multiplier;

		/// <summary>
		/// Scales the radius within zombies and animals will detect the player.
		/// </summary>
		public float Detect_Radius_Multiplier;

		/// <summary>
		/// How close an attack is to a player to be considered aggressive.
		/// For example, when a bullet passes within this distance of a player the shooter is
		/// considered the aggressor.
		/// </summary>
		public float Ray_Aggressor_Distance;

		/// <summary>
		/// Percentage [0 to 1] of skill levels to retain when killed by another player.
		/// </summary>
		public float Lose_Skills_PvP;
		/// <summary>
		/// Percentage [0 to 1] of skill levels to retain when killed by the environment (e.g., zombies).
		/// </summary>
		public float Lose_Skills_PvE;
		/// <summary>
		/// Number of skill levels to remove when killed by another player.
		/// </summary>
		public uint Lose_Skill_Levels_PvP;
		/// <summary>
		/// Number of skill levels to remove when killed by the environment (e.g., zombies).
		/// </summary>
		public uint Lose_Skill_Levels_PvE;
		/// <summary>
		/// Percentage [0 to 1] of XP to retain when killed by another player.
		/// </summary>
		public float Lose_Experience_PvP;
		/// <summary>
		/// Percentage [0 to 1] of XP to retain when killed by the environment (e.g., zombies).
		/// </summary>
		public float Lose_Experience_PvE;

		/// <summary>
		/// Scales XP cost to purchase/upgrade skills.
		/// </summary>
		public float Skill_Cost_Multiplier;

		/// <summary>
		/// Percentage [0 to 1] chance to lose each inventory item when killed by another player.
		/// Depends on Lose_Clothes_PvP because losing storage will drop contained items.
		/// </summary>
		public float Lose_Items_PvP;

		/// <summary>
		/// Percentage [0 to 1] chance to lose each inventory item when killed by the environment (e.g., zombies).
		/// Depends on Lose_Clothes_PvE because losing storage will drop contained items.
		/// </summary>
		public float Lose_Items_PvE;

		/// <summary>
		/// If true, drop all clothing items when killed by another player.
		/// </summary>
		public bool Lose_Clothes_PvP;

		/// <summary>
		/// If true, drop all clothing items when killed by the environment (e.g., zombies).
		/// </summary>
		public bool Lose_Clothes_PvE;

		/// <summary>
		/// If true, drop primary and secondary weapon when killed by another player.
		/// </summary>
		public bool Lose_Weapons_PvP;

		/// <summary>
		/// If true, drop primary and secondary weapon when killed by the environment (e.g., zombies).
		/// </summary>
		public bool Lose_Weapons_PvE;

		/// <summary>
		/// If false, players have no health loss from falling long distances.
		/// </summary>
		public bool Can_Hurt_Legs;

		/// <summary>
		/// If false, players cannot break their leg when falling long distances.
		/// </summary>
		public bool Can_Break_Legs;

		/// <summary>
		/// If false, broken legs cannot automatically heal themselves after Leg_Regen_Ticks.
		/// </summary>
		public bool Can_Fix_Legs;

		/// <summary>
		/// If false, damage cannot cause players to bleed.
		/// </summary>
		public bool Can_Start_Bleeding;

		/// <summary>
		/// If false, bleeding cannot automatically heal itself after Bleed_Regen_Ticks.
		/// </summary>
		public bool Can_Stop_Bleeding;

		/// <summary>
		/// Should all skills default to max level?
		/// </summary>
		public bool Spawn_With_Max_Skills;

		/// <summary>
		/// Should cardio, diving, exercise, and parkour default to max level?
		/// </summary>
		public bool Spawn_With_Stamina_Skills;

		/// <summary>
		/// If true, skills related to player's skillset/speciality are half cost.
		/// </summary>
		public bool Skillset_Reduces_Skill_Cost = true;

		/// <summary>
		/// If true, skills related to player's skillset/speciality cannot lose levels on death.
		/// </summary>
		public bool Skillset_Prevents_Skill_Loss = true;

		/// <summary>
		/// If true, prevent levels from modifying skill starting levels, costs, and max levels.
		/// </summary>
		public bool Prevent_Level_Skill_Overrides;

		/// <summary>
		/// Should guns with Instakill Headshots (snipers) bypass armor?
		/// </summary>
		public bool Allow_Instakill_Headshots;

		/// <summary>
		/// Should each character slot have separate savedata?
		/// </summary>
		public bool Allow_Per_Character_Saves;

		/// <summary>
		/// If true, players will be kicked if their skin color is too similar to one of the level's terrain colors.
		/// </summary>
		public bool Enable_Terrain_Color_Kick = true;

		public PlayersConfigData(EGameMode mode)
		{
			Health_Default = 100;
			Health_Regen_Min_Food = 90;
			Health_Regen_Min_Water = 90;
			Health_Regen_Ticks = 60;

			Food_Damage_Ticks = 15;
			Water_Damage_Ticks = 20;

			Virus_Default = 100;
			Virus_Infect = 50;
			Virus_Use_Ticks = 125;
			Virus_Damage_Ticks = 25;

			Leg_Regen_Ticks = 750;
			Bleed_Damage_Ticks = 10;
			Bleed_Regen_Ticks = 750;

			switch (mode)
			{
				case EGameMode.HARD:
					Food_Default = 85;
					Water_Default = 85;
					break;
				default:
					Food_Default = 100;
					Water_Default = 100;
					break;
			}

			switch (mode)
			{
				case EGameMode.EASY:
					Food_Use_Ticks = 350;
					Water_Use_Ticks = 320;
					break;
				case EGameMode.HARD:
					Food_Use_Ticks = 250;
					Water_Use_Ticks = 220;
					break;
				default:
					Food_Use_Ticks = 300;
					Water_Use_Ticks = 270;
					break;
			}

			switch (mode)
			{
				case EGameMode.EASY:
					Experience_Multiplier = 1.5f;
					break;
				case EGameMode.NORMAL:
					Experience_Multiplier = 1.0f;
					break;
				case EGameMode.HARD:
					Experience_Multiplier = 1.5f;
					break;
				default:
					Experience_Multiplier = 10.0f;
					break;
			}

			switch (mode)
			{
				case EGameMode.EASY:
					Detect_Radius_Multiplier = 0.5f;
					break;
				case EGameMode.HARD:
					Detect_Radius_Multiplier = 1.25f;
					break;
				default:
					Detect_Radius_Multiplier = 1.0f;
					break;
			}

			Ray_Aggressor_Distance = 8.0f;

			Armor_Multiplier = 1.0f;

			Lose_Skills_PvP = 1.0f;
			Lose_Skills_PvE = 1.0f;
			Lose_Skill_Levels_PvP = 1;
			Lose_Skill_Levels_PvE = 1;
			Lose_Experience_PvP = 0.5f;
			Lose_Experience_PvE = 0.5f;
			Skill_Cost_Multiplier = 1.0f;
			Lose_Items_PvP = 1.0f;
			Lose_Items_PvE = 1.0f;
			Lose_Clothes_PvP = true;
			Lose_Clothes_PvE = true;
			Lose_Weapons_PvP = true;
			Lose_Weapons_PvE = true;

			Can_Hurt_Legs = true;

			switch (mode)
			{
				case EGameMode.EASY:
					Can_Break_Legs = false;
					Can_Start_Bleeding = false;
					Lose_Skill_Levels_PvP = 0;
					Lose_Skill_Levels_PvE = 0;
					break;
				default:
					Can_Break_Legs = true;
					Can_Start_Bleeding = true;
					break;
			}

			switch (mode)
			{
				case EGameMode.HARD:
					Can_Fix_Legs = false;
					Can_Stop_Bleeding = false;
					Lose_Skill_Levels_PvP = 2;
					Lose_Skill_Levels_PvE = 2;
					break;
				default:
					Can_Fix_Legs = true;
					Can_Stop_Bleeding = true;
					break;
			}

			Spawn_With_Max_Skills = false;
			Spawn_With_Stamina_Skills = false;
			Allow_Instakill_Headshots = mode == EGameMode.HARD;
			Allow_Per_Character_Saves = false;
		}

		public void InitSingleplayerDefaults()
		{
			Allow_Per_Character_Saves = true;
		}
	}

	public class ObjectConfigData
	{
		/// <summary>
		/// Scales how long before interactables like fridges automatically close.
		/// </summary>
		public float Binary_State_Reset_Multiplier;

		/// <summary>
		/// Scales how long before sources of fuel in the world are automatically partially refilled.
		/// </summary>
		public float Fuel_Reset_Multiplier;

		/// <summary>
		/// Scales how long before sources of water in the world are automatically partially refilled.
		/// </summary>
		public float Water_Reset_Multiplier;

		/// <summary>
		/// Scales how long before trees, rocks, and bushes in the world grow back.
		/// </summary>
		public float Resource_Reset_Multiplier;

		/// <summary>
		/// Scales number of items dropped by resources like trees and rocks.
		/// </summary>
		public float Resource_Drops_Multiplier;

		/// <summary>
		/// Scales how long before destructible objects (e.g., fences) automatically repair.
		/// </summary>
		public float Rubble_Reset_Multiplier;

		/// <summary>
		/// Should holiday-specific objects be able to drop special items?
		/// For example, whether christmas presents contain guns.
		/// </summary>
		public bool Allow_Holiday_Drops;

		/// <summary>
		/// Should barricades placed on tree stumps prevent the tree from growing back
		/// while the server is running?
		/// </summary>
		public bool Items_Obstruct_Tree_Respawns;

		public ObjectConfigData(EGameMode mode)
		{
			Binary_State_Reset_Multiplier = 1.0f;
			Fuel_Reset_Multiplier = 1.0f;
			Water_Reset_Multiplier = 1.0f;
			Resource_Reset_Multiplier = 1.0f;
			Resource_Drops_Multiplier = 1.0f;
			Rubble_Reset_Multiplier = 1.0f;
			Allow_Holiday_Drops = true;
			Items_Obstruct_Tree_Respawns = true;
		}
	}

	public class ArenaLoadout
	{
		public ushort Table_ID;
		public ushort Amount;
	}

	public class EventsConfigData
	{
		/// <summary>
		/// Minimum number of in-game days between legacy rain events. 
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Rain_Frequency_Min;

		/// <summary>
		/// Maximum number of in-game days between legacy rain events. 
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Rain_Frequency_Max;

		/// <summary>
		/// Minimum number of in-game days a legacy rain event lasts. Zero turns off legacy rain.
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Rain_Duration_Min;

		/// <summary>
		/// Maximum number of in-game days a legacy rain event lasts. Zero turns off legacy rain.
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Rain_Duration_Max;

		/// <summary>
		/// Minimum number of in-game days between legacy snow events. 
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Snow_Frequency_Min;

		/// <summary>
		/// Maximum number of in-game days between legacy snow events. 
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Snow_Frequency_Max;

		/// <summary>
		/// Minimum number of in-game days a legacy snow event lasts. Zero turns off legacy snow.
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Snow_Duration_Min;

		/// <summary>
		/// Maximum number of in-game days a legacy snow event lasts. Zero turns off legacy snow.
		/// Only applicable for backwards compatibility with levels using the legacy weather features.
		/// </summary>
		public float Snow_Duration_Max;

		/// <summary>
		/// Scales number of in-game days between weather events. (Levels using the newer weather
		/// features can have multiple weather types with different frequencies.) If this was
		/// accidentally set to a high value you can use the "/weather 0" command to reschedule
		/// the next weather event.
		///
		/// Lower values cause more frequent weather, higher values cause less frequent weather.
		/// (Misnomer, sorry!)
		/// </summary>
		public float Weather_Frequency_Multiplier;

		/// <summary>
		/// Scales number of in-game days a weather event lasts. (Levels using the newer weather
		/// features can have multiple weather types with different durations.)
		/// Zero turns off weather entirely.
		/// </summary>
		public float Weather_Duration_Multiplier;

		/// <summary>
		/// Minimum number of in-game days between airdrops. Depends on Use_Airdrops.
		/// </summary>
		public float Airdrop_Frequency_Min;

		/// <summary>
		/// Maximum number of in-game days between airdrops. Depends on Use_Airdrops.
		/// </summary>
		public float Airdrop_Frequency_Max;

		/// <summary>
		/// How fast (in meters per second) the airdrop plane flies across the level.
		/// Lower values give players more time to react and chase the airplane.
		/// </summary>
		public float Airdrop_Speed;

		/// <summary>
		/// Amount of upward force applied to the carepackage, resisting gravity.
		/// Higher values require players to wait longer for the carepackage.
		/// (This isn't intuitive, sorry!)
		/// </summary>
		public float Airdrop_Force;

		/// <summary>
		/// Minimum number of teams needed to start an arena match.
		/// </summary>
		public uint Arena_Min_Players;

		/// <summary>
		/// Base damage per second while standing outside the arena field.
		/// </summary>
		public uint Arena_Compactor_Damage;

		/// <summary>
		/// Accumulating additional damage per second while standing outside the arena field.
		/// </summary>
		public float Arena_Compactor_Extra_Damage_Per_Second;

		/// <summary>
		/// How long (in seconds) between match ready and teleporting players into the arena.
		/// </summary>
		public uint Arena_Clear_Timer;

		/// <summary>
		/// How long (in seconds) after a winner is announced to wait before restarting.
		/// </summary>
		public uint Arena_Finale_Timer;

		/// <summary>
		/// How long (in seconds) to wait in intermission before starting the next match.
		/// </summary>
		public uint Arena_Restart_Timer;

		/// <summary>
		/// How long (in seconds) before first arena circle starts shrinking.
		/// </summary>
		public uint Arena_Compactor_Delay_Timer;

		/// <summary>
		/// How long (in seconds) after arena circle finishes shrinking to start shrinking again.
		/// </summary>
		public uint Arena_Compactor_Pause_Timer;

		/// <summary>
		/// Should airplanes fly over the level dropping carepackages?
		/// </summary>
		public bool Use_Airdrops;

		/// <summary>
		/// If true, arena selects multiple smaller circles within the initial circle.
		/// Otherwise, arena cricle shrinks toward its initial center.
		/// </summary>
		public bool Arena_Use_Compactor_Pause;

		/// <summary>
		/// How quickly (in meters per second) the arena radius shrinks on "Tiny" size levels.
		/// </summary>
		public float Arena_Compactor_Speed_Tiny;

		/// <summary>
		/// How quickly (in meters per second) the arena radius shrinks on "Small" size levels.
		/// </summary>
		public float Arena_Compactor_Speed_Small;

		/// <summary>
		/// How quickly (in meters per second) the arena radius shrinks on "Medium" size levels.
		/// </summary>
		public float Arena_Compactor_Speed_Medium;

		/// <summary>
		/// How quickly (in meters per second) the arena radius shrinks on "Large" size levels.
		/// </summary>
		public float Arena_Compactor_Speed_Large;

		/// <summary>
		/// How quickly (in meters per second) the arena radius shrinks on "Insane" size levels.
		/// </summary>
		public float Arena_Compactor_Speed_Insane;

		/// <summary>
		/// Percentage [0 to 1] of arena circle radius retained when selecting next smaller circle.
		/// Depends on Arena_Use_Compactor_Pause.
		/// </summary>
		public float Arena_Compactor_Shrink_Factor;

		//public List<ArenaLoadout> Arena_Loadouts;

		public EventsConfigData(EGameMode mode)
		{
			Rain_Frequency_Min = 2.3f;
			Rain_Frequency_Max = 5.6f;
			Rain_Duration_Min = 0.05f;
			Rain_Duration_Max = 0.15f;

			Snow_Frequency_Min = 1.3f;
			Snow_Frequency_Max = 4.6f;
			Snow_Duration_Min = 0.2f;
			Snow_Duration_Max = 0.5f;

			Weather_Frequency_Multiplier = 1.0f;
			Weather_Duration_Multiplier = 1.0f;

			Airdrop_Frequency_Min = 0.8f;
			Airdrop_Frequency_Max = 6.5f;
			Airdrop_Speed = 128.0f;
			Airdrop_Force = 9.5f;

			Arena_Clear_Timer = 5;
			Arena_Finale_Timer = 10;
			Arena_Restart_Timer = 15;
			Arena_Compactor_Delay_Timer = 1;
			Arena_Compactor_Pause_Timer = 5;

			Arena_Min_Players = 2;
			Arena_Compactor_Damage = 9;
			Arena_Compactor_Extra_Damage_Per_Second = 1.0f;
			Use_Airdrops = true;
			Arena_Use_Compactor_Pause = true;

			Arena_Compactor_Speed_Tiny = 0.5f;
			Arena_Compactor_Speed_Small = 1.5f;
			Arena_Compactor_Speed_Medium = 3.0f;
			Arena_Compactor_Speed_Large = 4.5f;
			Arena_Compactor_Speed_Insane = 6.0f;
			Arena_Compactor_Shrink_Factor = 0.5f;

			//Arena_Loadouts = new List<ArenaLoadout>();
		}
	}

	public class UnityEventConfigData
	{
		/// <summary>
		/// Should ServerTextChatMessenger be allowed to broadcast?
		/// </summary>
		public bool Allow_Server_Messages;

		/// <summary>
		/// Should ServerTextChatMessenger be allowed to execute commands?
		/// </summary>
		public bool Allow_Server_Commands;

		/// <summary>
		/// Should ClientTextChatMessenger be allowed to broadcast?
		/// </summary>
		public bool Allow_Client_Messages;

		/// <summary>
		/// Should ClientTextChatMessenger be allowed to execute commands?
		/// </summary>
		public bool Allow_Client_Commands;
	}

	public class GameplayConfigData
	{
		/// <summary>
		/// Blueprints requiring a repair skill level higher than this cannot be crafted.
		/// Restricts players from repairing higher-tier items.
		/// </summary>
		public uint Repair_Level_Max;

		/// <summary>
		/// Should a hit confirmation be shown when players deal damage?
		/// </summary>
		public bool Hitmarkers;

		/// <summary>
		/// Should a crosshair be visible while holding a gun?
		/// </summary>
		public bool Crosshair;

		/// <summary>
		/// Should bullets be affected by gravity and travel time?
		/// </summary>
		public bool Ballistics;

		/// <summary>
		/// Should the player have permanent access to a "paper" map of the level even when they
		/// don't have the associated in-game item?
		/// </summary>
		public bool Chart;

		/// <summary>
		/// Should the player have permanent access to a GPS map of the level even when they
		/// don't have the associated in-game item?
		/// </summary>
		public bool Satellite;

		/// <summary>
		/// Should the player have permanent access to their compass heading HUD even when they
		/// don't have the associated in-game item?
		/// </summary>
		public bool Compass;

		/// <summary>
		/// Should group members and similar info be visible on the in-game map?
		/// </summary>
		public bool Group_Map;

		/// <summary>
		/// Should group member names be visible through walls?
		/// </summary>
		public bool Group_HUD;

		/// <summary>
		/// Should group connections be shown on player list?
		/// </summary>
		public bool Group_Player_List;

		/// <summary>
		/// Should Steam clans/groups be enables as in-game groups?
		/// </summary>
		public bool Allow_Static_Groups;

		/// <summary>
		/// Should players be allowed to create in-game groups and invite members of the server?
		/// </summary>
		public bool Allow_Dynamic_Groups;

		/// <summary>
		/// If true, allow automatically creating an in-game group for members of your Steam lobby.
		/// Requires Allow_Dynamic_Groups to be enabled as well.
		/// </summary>
		public bool Allow_Lobby_Groups;

		/// <summary>
		/// Should the third-person camera extend out to the side?
		/// If false, the third-person camera is centered over your character.
		/// </summary>
		public bool Allow_Shoulder_Camera;

		/// <summary>
		/// Should players be allowed to kill themselves from the pause menu?
		/// </summary>
		public bool Can_Suicide;

		/// <summary>
		/// Is friendly-fire within groups allowed?
		/// </summary>
		public bool Friendly_Fire;

		/// <summary>
		/// Are sentry guns and beds allowed on vehicles?
		/// </summary>
		public bool Bypass_Buildable_Mobility;

		/// <summary>
		/// If true, buildables can be placed in "no building" zones.
		/// </summary>
		public bool Bypass_No_Building_Zones;

		/// <summary>
		/// If true, buildables can be placed in safezones.
		/// </summary>
		public bool Bypass_Building_In_Safezones;

		/// <summary>
		/// Should holiday (Halloween and Christmas) content like NPC outfits and decorations be loaded?
		/// </summary>
		public bool Allow_Holidays = true;

		/// <summary>
		/// Can "freeform" barricades be placed in the world?
		/// </summary>
		public bool Allow_Freeform_Buildables;

		/// <summary>
		/// Can "freeform" barricades be placed on vehicles?
		/// </summary>
		public bool Allow_Freeform_Buildables_On_Vehicles;

		/// <summary>
		/// If true, aim flinches away from center when damaged.
		/// </summary>
		public bool Enable_Damage_Flinch;

		/// <summary>
		/// If true, camera will shake near explosions. Can also be toned down client-side in Options menu.
		/// </summary>
		public bool Enable_Explosion_Camera_Shake;

		/// <summary>
		/// If true, crafting blueprints can require nearby workstations.
		/// If false, only the backwards-compatibility "Heat Source" vanilla crafting tag can be required. This
		/// functions identically to the cooking-skill-also-requires-heat behavior from before.
		/// </summary>
		public bool Enable_Workstation_Requirements;

		/// <summary>
		/// If true, client-side options like damage flinch, explosion camera shake, viewmodel bob are ignored.
		/// </summary>
		[ConfigWarnIfTrue]
		public bool Disable_Motion_Sickness_Options;

		/// <summary>
		/// If true, minimum foliage density of "Low" is enforced.
		/// </summary>
		[ConfigWarnIfTrue]
		public bool Disable_Foliage_Off;

		/// <summary>
		/// If true, hide viewmodel while aiming a dual-render scope and show a 2D overlay instead.
		/// Useful for backwards compatibility with modded scopes that have a small enough
		/// dual-render surface to zoom-*out* when aiming in.
		/// </summary>
		public bool Use_2D_Scope_Overlay;

		/// <summary>
		/// If true, a challenge must be completed before catching a fish.
		/// Only applicable for supported maps and fishing rods. (I.e., not older custom maps.)
		/// </summary>
		public bool Enable_Fishing_Catch_Challenge;

		internal const uint MAX_TIMER_EXIT = 60;

		/// <summary>
		/// How long (in seconds) before a player can leave the server through the pause menu.
		/// </summary>
		public uint Timer_Exit;

		/// <summary>
		/// How long (in seconds) after death before a player can respawn.
		/// </summary>
		public uint Timer_Respawn;

		/// <summary>
		/// How long (in seconds) after death before a player can respawn at their bed.
		/// </summary>
		public uint Timer_Home;

		/// <summary>
		/// How long (in seconds) after a player requests to leave an in-game "dynamic" group
		/// before they are actually removed. Gives group members time to take cover.
		/// </summary>
		public uint Timer_Leave_Group;

		/// <summary>
		/// Maximum number of players invitable to an in-game "dynamic" group.
		/// Depends on Allow_Dynamic_Groups.
		/// </summary>
		public uint Max_Group_Members;

		/// <summary>
		/// Scales velocity added to players by explosion knock-back.
		/// </summary>
		public float Explosion_Launch_Speed_Multiplier = 1.0f;

		/// <summary>
		/// Scales midair input change in player direction.
		/// </summary>
		public float AirStrafing_Acceleration_Multiplier = 1.0f;

		/// <summary>
		/// Scales midair decrease in speed while faster than max walk speed.
		/// </summary>
		public float AirStrafing_Deceleration_Multiplier = 1.0f;

		/// <summary>
		/// Scales magnitude of recoil while using first-person perspective.
		/// </summary>
		public float FirstPerson_RecoilMultiplier = 1.0f;

		/// <summary>
		/// Scales magnitude of recoil while aiming in first-person perspective.
		/// </summary>
		public float FirstPerson_AimingRecoilMultiplier = 1.0f;

		/// <summary>
		/// Scales magnitude of recoil inversely with zoom level while aiming in first-person perspective.
		/// </summary>
		public float FirstPerson_AimingZoomRecoilReduction = 0.0f;

		/// <summary>
		/// Scales magnitude of recoil while using third-person perspective.
		/// </summary>
		public float ThirdPerson_RecoilMultiplier = 2.0f;

		/// <summary>
		/// Scales magnitude of bullet inaccuracy while using third-person perspective.
		/// </summary>
		public float ThirdPerson_SpreadMultiplier = 2.0f;

		/// <summary>
		/// [0 to 1] Scales how much the first-person move up and down while jumping/landing.
		/// </summary>
		public float Viewmodel_AimingJumpLandMultiplier = 1.0f;

		/// <summary>
		/// [0 to 1] Scales how much the first-person arms move while ADS.
		/// </summary>
		public float Viewmodel_AimingMisalignmentMultiplier = 1.0f;

		/// <summary>
		/// Shortest amount of time before a fish takes the bait.
		/// </summary>
		public float Min_Fishing_Bite_Interval;

		/// <summary>
		/// Longest amount of time before a fish takes the bait.
		/// </summary>
		public float Max_Fishing_Bite_Interval;

		/// <summary>
		/// Multiplier for fishing bite interval when casting strength bar is full.
		/// </summary>
		public float Fishing_MaxStrength_Bite_Interval_Multiplier = 0.3f;

		public GameplayConfigData(EGameMode mode)
		{
			Repair_Level_Max = 3;

			switch (mode)
			{
				case EGameMode.HARD:
					Hitmarkers = false;
					Crosshair = false;
					break;
				default:
					Hitmarkers = true;
					Crosshair = true;
					break;
			}

			switch (mode)
			{
				case EGameMode.EASY:
					Ballistics = false;
					break;
				default:
					Ballistics = true;
					break;
			}

			switch (mode)
			{
				case EGameMode.EASY:
					ThirdPerson_RecoilMultiplier = 1.0f;
					ThirdPerson_SpreadMultiplier = 1.0f;
					Viewmodel_AimingMisalignmentMultiplier = 0.2f;
					FirstPerson_AimingZoomRecoilReduction = 0.25f;
					break;

				case EGameMode.NORMAL:
					Viewmodel_AimingMisalignmentMultiplier = 0.5f;
					break;

				case EGameMode.HARD:
					Viewmodel_AimingMisalignmentMultiplier = 1.0f;
					break;
			}

			Chart = mode == EGameMode.EASY;
			Satellite = false;
			Compass = false;
			Group_Map = mode != EGameMode.HARD;
			Group_HUD = true;
			Group_Player_List = true;
			Allow_Static_Groups = true;
			Allow_Dynamic_Groups = true;
			Allow_Lobby_Groups = true;
			Allow_Shoulder_Camera = true;
			Can_Suicide = true;
			Friendly_Fire = false;
			Bypass_Buildable_Mobility = false;

			Timer_Exit = 10;
			Timer_Respawn = 10;
			Timer_Home = 30;
			Timer_Leave_Group = 30;
			Max_Group_Members = 0;

			Allow_Freeform_Buildables = true;
			Allow_Freeform_Buildables_On_Vehicles = true;
			Enable_Damage_Flinch = true;
			Enable_Explosion_Camera_Shake = true;
			Enable_Workstation_Requirements = true;
			Enable_Fishing_Catch_Challenge = true;

			switch (mode)
			{
				case EGameMode.TUTORIAL:
				{
					Min_Fishing_Bite_Interval = 15f;
					Max_Fishing_Bite_Interval = 25f;
					break;
				}

				case EGameMode.EASY:
				{
					Min_Fishing_Bite_Interval = 35f;
					Max_Fishing_Bite_Interval = 48f;
					break;
				}

				default:
				{
					Min_Fishing_Bite_Interval = 48f;
					Max_Fishing_Bite_Interval = 60f;
					break;
				}
			}
		}

		public void InitSingleplayerDefaults()
		{
			Bypass_Buildable_Mobility = true;
			Bypass_Building_In_Safezones = true;
		}

		// Moved from PlayerMovement because it cannot be within MonoBehaviour.
		internal static CommandLineFlag _forceTrustClient = new CommandLineFlag(false, "-ForceTrustClient");
	}

	internal static class PlayConfigUtils
	{
		private static ConfigData configDefaults = ConfigData.CreateDefault(false);
		
		private static string GetModeFileName(EGameMode mode)
		{
			switch (mode)
			{
				case EGameMode.EASY:
					return "EasyDifficulty";
					
				case EGameMode.NORMAL:
					return "NormalDifficulty";
					
				case EGameMode.HARD:
					return "HardDifficulty";
					
				default:
					throw new System.NotImplementedException(mode.ToString());
			}
		}

		/// <summary>
		/// Each generated comment line is prefixed with this string.
		/// </summary>
		public const string COMMENT_PREFIX = "> ";

		/// <summary>
		/// Format absolute path to newer txt (UnturnedDat) config file.
		/// </summary>
		public static string GetSingleplayerConfigPathV2(int characterSlot, EGameMode singleplayerMode)
		{
			string modeName = GetModeFileName(singleplayerMode);
			return PathEx.Join(UnturnedPaths.RootDirectory, "Worlds", $"Singleplayer_{characterSlot}", $"Config_{modeName}.txt");
		}

		/// <summary>
		/// Format absolute path to older json serialized config file.
		/// </summary>
		public static string GetSingleplayerConfigPathV1(int characterSlot)
		{
			return PathEx.Join(UnturnedPaths.RootDirectory, "Worlds", $"Singleplayer_{characterSlot}", "Config.json");
		}

		/// <summary>
		/// Config path used for new servers.
		/// </summary>
		public static string GetServerConfigPathV2(string serverId)
		{
			return PathEx.Join(UnturnedPaths.RootDirectory, "Servers", serverId, "Config.txt");
		}

		/// <summary>
		/// Config path used for conversion from Config.json.
		/// </summary>
		public static string GetServerConfigPathV2(string serverId, EGameMode serverMode)
		{
			string modeName = GetModeFileName(serverMode);
			return PathEx.Join(UnturnedPaths.RootDirectory, "Servers", serverId, $"Config_{modeName}.txt");
		}

		public static string GetFieldPath(FieldInfo field)
		{
			string name = field.DeclaringType.Name;
			if (name.EndsWith("ConfigData", System.StringComparison.Ordinal))
			{
				name = name.Substring(0, name.Length - "ConfigData".Length);
			}
			return $"{name}.{field.Name}";
		}

		/// <summary>
		/// Fill server-related sections of config from dat file.
		/// </summary>
		public static void ParseServerConfig(IDatDictionary rootDictionary, ConfigData config)
		{
			if (rootDictionary.TryGetDictionary("Browser", out IDatDictionary browser))
			{
				ParseCategory(browser, config.Browser, null);
			}
			if (rootDictionary.TryGetDictionary("Server", out IDatDictionary server))
			{
				ParseCategory(server, config.Server, null);
			}
			if (rootDictionary.TryGetDictionary("UnityEvents", out IDatDictionary unityEvents))
			{
				ParseCategory(unityEvents, config.UnityEvents, null);
			}
		}

		/// <summary>
		/// Fill mode-related sections of config from dat file and gather overrides.
		/// (for servers and singleplayer)
		/// </summary>
		public static void ParseModeConfig(IDatDictionary rootDictionary, ModeConfigData config, Dictionary<FieldInfo, object> overrides)
		{
			System.Type configType = typeof(ModeConfigData);
			FieldInfo[] categoryFields = configType.GetFields();
			foreach (FieldInfo categoryField in categoryFields)
			{
				if (!rootDictionary.TryGetDictionary(categoryField.Name, out IDatDictionary categoryDictionary))
				{
					continue;
				}

				object category = categoryField.GetValue(config);
				ParseCategory(categoryDictionary, category, overrides);
			}
		}

		/// <summary>
		/// Parses dictionary keys according to reflected fields in targetObject.
		/// If overrides is valid, gathers which values were set. (used for mode config)
		/// </summary>
		private static void ParseCategory(IDatDictionary dictionary, object targetObject, Dictionary<FieldInfo, object> overrides)
		{
			FieldInfo[] configFields = targetObject.GetType().GetFields();
			foreach (FieldInfo configField in configFields)
			{
				if (!dictionary.TryGetNode(configField.Name, out IDatNode node))
				{
					// Not added yet.
					continue;
				}

				if (configField.FieldType.IsArray)
				{
					if (node is IDatList listNode)
					{
						if (TryParseArrayField(configField, listNode, out System.Array overrideValues))
						{
							configField.SetValue(targetObject, overrideValues);
							if (overrides != null)
							{
								overrides.Add(configField, overrideValues);
							}
						}
					}
					else
					{
						if (node is IDatValue valueNode)
						{
							// Default is empty value node, so if user specified a value warn it's misconfigured
							if (!valueNode.IsValueNullOrEmpty())
							{
								node.TryGetParsedLineNumber(out int lineNumber);
								CommandWindow.LogWarning($"Server config: expected {configField.Name} on line {lineNumber} to be a List, but found a Value");
							}
						}
						else if (node is IDatDictionary dictionaryNode)
						{
							node.TryGetParsedLineNumber(out int lineNumber);
							CommandWindow.LogWarning($"Server config: expected {configField.Name} on line {lineNumber} to be a List, but found a Dictionary");
						}
					}
				}
				else
				{
					if (node is IDatValue valueNode)
					{
						// Default is empty.
						if (!string.IsNullOrEmpty(valueNode.Value))
						{
							if (TryParseValueField(configField, valueNode, out object overrideValue))
							{
								configField.SetValue(targetObject, overrideValue);
								if (overrides != null)
								{
									overrides.Add(configField, overrideValue);
								}
							}
						}
					}
					else
					{
						node.TryGetParsedLineNumber(out int lineNumber);
						CommandWindow.LogWarning($"Server config: expected {configField.Name} on line {lineNumber} to be a Value, but found {node.NodeType}");
					}
				}
			}
		}

		/// <summary>
		/// Attempt to parse user-supplied value from dat file according to field's reflected type.
		/// </summary>
		private static bool TryParseValueField(FieldInfo fieldInfo, IDatValue valueNode, out object overrideValue)
		{
			// At this point we know value is non-empty, so failure to parse is misconfig

			System.Type valueType = fieldInfo.FieldType;
			if (valueType == typeof(bool))
			{
				if (valueNode.TryParseBool(out bool value))
				{
					overrideValue = value;
					return true;
				}
				else
				{
					CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as bool (true/false) from \"{valueNode.Value}\"");
				}
			}
			else if (valueType == typeof(float))
			{
				if (valueNode.TryParseFloat(out float value))
				{
					overrideValue = value;
					return true;
				}
				else
				{
					CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as decimal number from \"{valueNode.Value}\"");
				}
			}
			else if (valueType == typeof(int))
			{
				if (valueNode.TryParseInt32(out int value))
				{
					overrideValue = value;
					return true;
				}
				else
				{
					CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as integer number from \"{valueNode.Value}\"");
				}
			}
			else if (valueType == typeof(uint))
			{
				if (valueNode.TryParseUInt32(out uint value))
				{
					overrideValue = value;
					return true;
				}
				else
				{
					CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as non-negative integer number from \"{valueNode.Value}\"");
				}
			}
			else if (valueType == typeof(string))
			{
				overrideValue = valueNode.Value;
				return true;
			}
			else if (valueType.IsEnum)
			{
				if (valueNode.TryParseEnum(valueType, out overrideValue))
				{
					return true;
				}
				else
				{
					CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as {valueType.Name} from \"{valueNode.Value}\"");
				}
			}
			else
			{
				throw new System.NotImplementedException(valueType.ToString());
			}

			overrideValue = null;
			return false;
		}

		/// <summary>
		/// Attempt to parse user-supplied list from dat file according to field's reflected type.
		/// </summary>
		private static bool TryParseArrayField(FieldInfo fieldInfo, IDatList listNode, out System.Array overrideValues)
		{
			System.Type elementType = fieldInfo.FieldType.GetElementType(); // e.g. string rather than string[]
			System.Array tempOverrideValues = System.Array.CreateInstance(elementType, listNode.Count);

			if (elementType == typeof(string))
			{
				int valueIndex = -1;
				for (int nodeIndex = 0; nodeIndex < listNode.Count; ++nodeIndex)
				{
					IDatNode node = listNode[nodeIndex];
					if (node is not IDatValue valueNode)
					{
						CommandWindow.LogWarning($"Server config: expected {fieldInfo.Name} on line {node.GetParsedLineNumber()} to be a Value, but found a {node.NodeType}");
						continue;
					}

					++valueIndex;
					tempOverrideValues.SetValue(valueNode.Value, valueIndex);
				}

				int actualValueCount = valueIndex + 1;
				overrideValues = System.Array.CreateInstance(elementType, actualValueCount);
				if (actualValueCount > 0)
				{
					System.Array.Copy(tempOverrideValues, overrideValues, actualValueCount);
				}
				return true;
			}
			else if (typeof(IDatParseable).IsAssignableFrom(elementType))
			{
				int valueIndex = -1;
				for (int nodeIndex = 0; nodeIndex < listNode.Count; ++nodeIndex)
				{
					IDatNode node = listNode[nodeIndex];
					IDatParseable structInstance = (IDatParseable) System.Activator.CreateInstance(elementType);
					if (!structInstance.TryParse(node))
					{
						CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {node.GetParsedLineNumber()} as {elementType.Name}");
						continue;
					}

					++valueIndex;
					tempOverrideValues.SetValue(structInstance, valueIndex);
				}

				int actualValueCount = valueIndex + 1;
				overrideValues = System.Array.CreateInstance(elementType, actualValueCount);
				if (actualValueCount > 0)
				{
					System.Array.Copy(tempOverrideValues, overrideValues, actualValueCount);
				}
				return true;
			}
			else
			{
				throw new System.NotImplementedException(elementType.ToString());
			}
		}

		/// <summary>
		/// WARNING: This is called on a worker thread.
		/// 
		/// Add empty dat values (if not yet added), and include code documentation
		/// in their comments prefixed with COMMENT_PREFIX. User-supplied comments are preserved.
		/// </summary>
		public static void PopulateConfigFilePropertiesAndComments(IEditableDatDictionary rootDictionary)
		{
			UnturnedCodeDocsHelper codeDocsHelper = new UnturnedCodeDocsHelper();

			IEditableDatDictionary browserDictionary = rootDictionary.GetOrAddDictionary("Browser");
			if (!browserDictionary.IsMetadataAvailable)
			{
				browserDictionary.TopMargin = 1;
			}
			PopulateConfigFilePropertiesAndComments(codeDocsHelper, browserDictionary, typeof(BrowserConfigData), null, configDefaults.Browser, null);

			IEditableDatDictionary serverDictionary = rootDictionary.GetOrAddDictionary("Server");
			if (!serverDictionary.IsMetadataAvailable)
			{
				serverDictionary.TopMargin = 1;
			}
			PopulateConfigFilePropertiesAndComments(codeDocsHelper, serverDictionary, typeof(ServerConfigData), null, configDefaults.Server, null);

			IEditableDatDictionary unityEventsDictionary = rootDictionary.GetOrAddDictionary("UnityEvents");
			if (!unityEventsDictionary.IsMetadataAvailable)
			{
				unityEventsDictionary.TopMargin = 1;
			}
			PopulateConfigFilePropertiesAndComments(codeDocsHelper, unityEventsDictionary, typeof(UnityEventConfigData), null, configDefaults.UnityEvents, null);

			System.Type configType = typeof(ModeConfigData);
			FieldInfo[] categoryFields = configType.GetFields();
			foreach (FieldInfo categoryField in categoryFields)
			{
#pragma warning disable
				object defaultConfigGroupEasy = categoryField.GetValue(configDefaults.Easy);
				object defaultConfigGroupNormal = categoryField.GetValue(configDefaults.Normal);
				object defaultConfigGroupHard = categoryField.GetValue(configDefaults.Hard);
#pragma warning restore

				IEditableDatDictionary categoryDictionary = rootDictionary.GetOrAddDictionary(categoryField.Name);
				if (!categoryDictionary.IsMetadataAvailable)
				{
					categoryDictionary.TopMargin = 1;
				}

				PopulateConfigFilePropertiesAndComments(codeDocsHelper, categoryDictionary, categoryField.FieldType, defaultConfigGroupEasy, defaultConfigGroupNormal, defaultConfigGroupHard);
			}
		}

		/// <summary>
		/// Add empty dat values for every field in category (if not yet added), and include code documentation
		/// in their comments prefixed with COMMENT_PREFIX. User-supplied comments are preserved.
		/// 
		/// In categories without easy/normal/hard split (server config), only normalObject is set.
		/// </summary>
		private static void PopulateConfigFilePropertiesAndComments(UnturnedCodeDocsHelper codeDocsHelper, IEditableDatDictionary dictionary, System.Type categoryType, object easyObject, object normalObject, object hardObject)
		{
			FieldInfo[] configFields = categoryType.GetFields();
			foreach (FieldInfo configField in configFields)
			{
				IEditableDatNode editableNode = null;
				if (dictionary.TryGetNode(configField.Name, out IDatNode node))
				{
					if (node is IDatValue valueNode)
					{
						editableNode = valueNode.Edit();
					}
					else if (node is IDatList listNode)
					{
						editableNode = listNode.Edit();
					}
				}

				if (editableNode == null)
				{
					// Even for arrays, add an empty value (so empty list doesn't override default)
					editableNode = dictionary.AddValue(configField.Name);
				}

				if (!editableNode.IsMetadataAvailable)
				{
					editableNode.TopMargin = 1;
				}

				object defaultValueEasy = easyObject != null ? configField.GetValue(easyObject) : null;
				object defaultValueNormal = normalObject != null ? configField.GetValue(normalObject) : null;
				object defaultValueHard = hardObject != null ? configField.GetValue(hardObject) : null;
				string summary = codeDocsHelper.GetSummary(categoryType.Name, configField.Name);
				UpdateFieldComment(configField, editableNode, summary, defaultValueEasy, defaultValueNormal, defaultValueHard);
			}
		}

		/// <summary>
		/// For conversion from json file. Server-only.
		/// </summary>
		public static void ApplyServerConfigOverrides(IEditableDatDictionary rootDictionary, Dictionary<FieldInfo, object> overrides)
		{
			ApplyOverridesInCategory(rootDictionary.GetOrAddDictionary("Browser"), typeof(BrowserConfigData), overrides);
			ApplyOverridesInCategory(rootDictionary.GetOrAddDictionary("Server"), typeof(ServerConfigData), overrides);
			ApplyOverridesInCategory(rootDictionary.GetOrAddDictionary("UnityEvents"), typeof(UnityEventConfigData), overrides);
		}

		/// <summary>
		/// For conversion from json file.
		/// </summary>
		public static void ApplyModeConfigOverrides(IEditableDatDictionary rootDictionary, Dictionary<FieldInfo, object> overrides)
		{
			System.Type configType = typeof(ModeConfigData);
			FieldInfo[] categoryFields = configType.GetFields();
			foreach (FieldInfo categoryField in categoryFields)
			{
				IEditableDatDictionary categoryDictionary = rootDictionary.GetOrAddDictionary(categoryField.Name);
				System.Type categoryType = categoryField.FieldType;
				ApplyOverridesInCategory(categoryDictionary, categoryType, overrides);
			}
		}

		/// <summary>
		/// Set dat values for every field in category that has an override specified.
		/// (Will not add values if not overridden.)
		/// </summary>
		private static void ApplyOverridesInCategory(IEditableDatDictionary dictionary, System.Type categoryType, Dictionary<FieldInfo, object> overrides)
		{
			FieldInfo[] configFields = categoryType.GetFields();
			foreach (FieldInfo configField in configFields)
			{
				bool hasOverride = overrides.TryGetValue(configField, out object overrideValue) && overrideValue != null;
				if (!hasOverride)
				{
					continue;
				}

				if (configField.FieldType.IsArray)
				{
					IEditableDatList listNode = dictionary.GetOrAddList(configField.Name);
					ApplyArrayFieldOverride(configField, listNode, (System.Array) overrideValue);
				}
				else
				{
					IEditableDatValue valueNode = dictionary.GetOrAddValue(configField.Name);
					ApplyValueFieldOverride(configField, valueNode, overrideValue);
				}
			}
		}

		private static void ApplyValueFieldOverride(FieldInfo fieldInfo, IEditableDatValue valueNode, object overrideValue)
		{
			System.Type valueType = fieldInfo.FieldType;
			if (valueType == typeof(bool))
			{
				valueNode.SetBool((bool) overrideValue);
			}
			else if (valueType == typeof(float))
			{
				valueNode.SetFloat((float) overrideValue);
			}
			else if (valueType == typeof(uint))
			{
				valueNode.SetUInt32((uint) overrideValue);
			}
			else if (valueType == typeof(int))
			{
				valueNode.SetInt32((int) overrideValue);
			}
			else if (valueType.IsEnum)
			{
				valueNode.Value = overrideValue.ToString();
			}
			else if (valueType == typeof(string))
			{
				valueNode.Value = (string) overrideValue;
			}
			else
			{
				throw new System.NotImplementedException(valueType.ToString());
			}
		}

		private static void ApplyArrayFieldOverride(FieldInfo fieldInfo, IEditableDatList listNode, System.Array overrideValues)
		{
			System.Type valueType = fieldInfo.FieldType;
			if (valueType == typeof(string[]))
			{
				foreach (object value in overrideValues)
				{
					listNode.AddValue().SetString((string) value);
				}
			}
			else if (typeof(IDatSerializable).IsAssignableFrom(valueType.GetElementType()))
			{
				foreach (object value in overrideValues)
				{
					IDatSerializable datSerializable = (IDatSerializable) value;
					IEditableDatDictionary dictionary = listNode.AddDictionary();
					datSerializable.SerializeIntoDictionary(dictionary);
				}
			}
			else
			{
				throw new System.NotImplementedException(valueType.ToString());
			}
		}

		/// <summary>
		/// For conversion from json file. Find fields different from default in the server-related categories.
		/// </summary>
		public static void GatherServerModifiedFields(ConfigData baseConfig, ConfigData currentConfig, Dictionary<FieldInfo, object> results)
		{
			GatherModifiedFields(baseConfig.Server, currentConfig.Server, results);
			GatherModifiedFields(baseConfig.Browser, currentConfig.Browser, results);
			GatherModifiedFields(baseConfig.UnityEvents, currentConfig.UnityEvents, results);
		}

		/// <summary>
		/// For conversion from json file. Find fields different from defaults in one of easy/normal/hard mode.
		/// </summary>
		public static void GatherModifiedFields(ModeConfigData baseConfig, ModeConfigData currentConfig, Dictionary<FieldInfo, object> results)
		{
			System.Type configType = typeof(ModeConfigData);
			FieldInfo[] categoryFields = configType.GetFields();
			foreach (FieldInfo categoryField in categoryFields)
			{
				object baseCategory = categoryField.GetValue(baseConfig);
				object currentCategory = categoryField.GetValue(currentConfig);
				GatherModifiedFields(baseCategory, currentCategory, results);
			}
		}

		private static void GatherModifiedFields(object baseObject, object currentObject, Dictionary<FieldInfo, object> results)
		{
			System.Type objectType = baseObject.GetType();
			FieldInfo[] fields = objectType.GetFields();
			foreach (FieldInfo field in fields)
			{
				object baseValue = field.GetValue(baseObject);
				object currentValue = field.GetValue(currentObject);
				System.Type fieldType = field.FieldType;
				if (fieldType == typeof(bool))
				{
					bool currentBool = (bool) currentValue;
					bool baseBool = (bool) baseValue;
					if (currentBool != baseBool)
					{
						results.Add(field, currentValue);
					}
				}
				else if (fieldType == typeof(float))
				{
					float currentFloat = (float) currentValue;
					float baseFloat = (float) baseValue;
					if (!MathfEx.IsNearlyEqual(currentFloat, baseFloat, tolerance: 0.0001f))
					{
						results.Add(field, currentValue);
					}
				}
				else if (fieldType == typeof(int) || fieldType.IsEnum)
				{
					// Also enum
					int currentInt = (int) currentValue;
					int baseInt = (int) baseValue;
					if (currentInt != baseInt)
					{
						results.Add(field, currentValue);
					}
				}
				else if (fieldType == typeof(uint))
				{
					uint currentInt = (uint) currentValue;
					uint baseInt = (uint) baseValue;
					if (currentInt != baseInt)
					{
						results.Add(field, currentValue);
					}
				}
				else if (fieldType == typeof(string))
				{
					string currentString = (string) currentValue;
					string baseString = (string) baseValue;
					if (!string.Equals(currentString, baseString))
					{
						results.Add(field, currentValue);
					}
				}
				else
				{
					if (ReferenceEquals(baseValue, null) != ReferenceEquals(currentValue, null))
					{
						results.Add(field, currentValue);
					}
					else if (baseValue != null && currentValue != null)
					{
						if (fieldType.IsArray)
						{
							System.Array currentArray = (System.Array) currentValue;
							System.Array baseArray = (System.Array) baseValue;
							if (currentArray.Length != baseArray.Length)
							{
								results.Add(field, currentValue);
							}
							else
							{
								for (int index = 0; index < currentArray.Length; ++index)
								{
									if (!Equals(currentArray.GetValue(index), baseArray.GetValue(index)))
									{
										results.Add(field, currentValue);
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		public static void RemoveEmptyValues(IEditableDatDictionary dictionary)
		{
			List<string> keysToRemove = new List<string>();
			foreach (KeyValuePair<string, IDatNode> kvp in dictionary)
			{
				switch (kvp.Value.NodeType)
				{
					case EDatNodeType.Value:
					{
						if (((IDatValue) kvp.Value).IsValueNullOrEmpty())
						{
							keysToRemove.Add(kvp.Key);
						}
						break;
					}

					case EDatNodeType.Dictionary:
					{
						IEditableDatDictionary edit = ((IDatDictionary) kvp.Value).Edit();
						if (edit != null)
						{
							RemoveEmptyValues(edit);
						}
						if (((IDatDictionary) kvp.Value).Count < 1)
						{
							keysToRemove.Add(kvp.Key);
						}
						break;
					}

					case EDatNodeType.List:
					{
						IEditableDatList edit = ((IDatList) kvp.Value).Edit();
						if (edit != null)
						{
							RemoveEmptyValues(edit);
						}
						if (((IDatList) kvp.Value).Count < 1)
						{
							keysToRemove.Add(kvp.Key);
						}
						break;
					}
				}
			}

			foreach (string key in keysToRemove)
			{
				dictionary.Remove(key);
			}
		}

		private static void RemoveEmptyValues(IEditableDatList list)
		{
			for (int index = list.Count - 1; index >= 0; --index)
			{
				IDatNode node = list[index];
				switch (node.NodeType)
				{
					case EDatNodeType.Value:
					{
						if (((IDatValue) node).IsValueNullOrEmpty())
						{
							list.RemoveAt(index);
						}
						break;
					}

					case EDatNodeType.Dictionary:
					{
						IEditableDatDictionary edit = ((IDatDictionary) node).Edit();
						if (edit != null)
						{
							RemoveEmptyValues(edit);
						}
						if (((IDatDictionary) node).Count < 1)
						{
							list.RemoveAt(index);
						}
						break;
					}

					case EDatNodeType.List:
					{
						IEditableDatList edit = ((IDatList) node).Edit();
						if (edit != null)
						{
							RemoveEmptyValues(edit);
						}
						if (((IDatList) node).Count < 1)
						{
							list.RemoveAt(index);
						}
						break;
					}
				}
			}
		}

		public static void RemoveGeneratedComments(IEditableDatNode node)
		{
			node.MergeGeneratedComment<IEditableDatNode, string[]>(COMMENT_PREFIX, null, commentStringBuilder, tempParsedLines);

			switch (node.NodeType)
			{
				case EDatNodeType.Dictionary:
				{
					foreach (KeyValuePair<string, IDatNode> kvp in ((IDatDictionary) node))
					{
						RemoveGeneratedCommentsWrapper(kvp.Value);
					}
					break;
				};

				case EDatNodeType.List:
				{
					IDatList list = (IDatList) node;
					foreach (IDatNode childNode in list)
					{
						RemoveGeneratedCommentsWrapper(childNode);
					}
					break;
				}
			}
		}

		private static void RemoveGeneratedCommentsWrapper(IDatNode node)
		{
			switch (node.NodeType)
			{
				case EDatNodeType.Dictionary:
				{
					RemoveGeneratedComments(((IDatDictionary) node).Edit());
					break;
				}

				case EDatNodeType.List:
				{
					RemoveGeneratedComments(((IDatList) node).Edit());
					break;
				}

				case EDatNodeType.Value:
				{
					RemoveGeneratedComments(((IDatValue) node).Edit());
					break;
				}
			}
		}

		private static string GetDefaultValueComment(object defaultValue)
		{
			if (defaultValue == null)
			{
				return null;
			}

			if (defaultValue is string defaultString)
			{
				if (defaultString.Length < 1)
				{
					// No comment for empty string.
					return null;
				}

				return defaultString;
			}

			if (defaultValue.GetType().IsArray)
			{
				// No single-line comment for arrays.
				return null;
			}

			return defaultValue.ToString();
		}

		private static void UpdateFieldComment(FieldInfo fieldInfo, IEditableDatNode node, string summary, object easy, object normal, object hard)
		{
			commentStringBuilder.Clear();

			generatedLines.Clear();
			if (!string.IsNullOrEmpty(summary))
			{
				string[] summaryLines = summary.SplitLinesIncludingEmpty();
				int lineCount = summaryLines.Length;
				if (string.IsNullOrWhiteSpace(summaryLines[lineCount - 1]))
				{
					lineCount--;
				}

				for (int lineIndex = 0; lineIndex < lineCount; ++lineIndex)
				{
					generatedLines.Add(summaryLines[lineIndex].Trim());
				}
			}

			if (fieldInfo.FieldType.IsEnum)
			{
				string[] enumNames = fieldInfo.FieldType.GetEnumNames();

				commentStringBuilder.Clear();
				commentStringBuilder.Append("Options: ");

				bool isFirst = true;
				foreach (string name in enumNames)
				{
					if (!isFirst)
					{
						commentStringBuilder.Append(", ");
					}
					commentStringBuilder.Append(name);
					isFirst = false;
				}

				generatedLines.Add(commentStringBuilder.ToString());
			}

			if (easy == null || hard == null || (normal.Equals(easy) && normal.Equals(hard)))
			{
				string normalComment = GetDefaultValueComment(normal);
				if (!string.IsNullOrEmpty(normalComment))
				{
					commentStringBuilder.Clear();
					commentStringBuilder.Append("Default: ");
					commentStringBuilder.Append(normalComment);
					generatedLines.Add(commentStringBuilder.ToString());
				}
			}
			else
			{
				commentStringBuilder.Clear();
				commentStringBuilder.Append("Easy: ");
				commentStringBuilder.Append(GetDefaultValueComment(easy));
				commentStringBuilder.Append("    Normal: ");
				commentStringBuilder.Append(GetDefaultValueComment(normal));
				commentStringBuilder.Append("    Hard: ");
				commentStringBuilder.Append(GetDefaultValueComment(hard));
				generatedLines.Add(commentStringBuilder.ToString());
			}

			node.MergeGeneratedComment(COMMENT_PREFIX, generatedLines, commentStringBuilder, tempParsedLines);
		}

		private static List<string> generatedLines = new List<string>();
		private static List<string> tempParsedLines = new List<string>();
		private static System.Text.StringBuilder commentStringBuilder = new System.Text.StringBuilder();
	}
}
