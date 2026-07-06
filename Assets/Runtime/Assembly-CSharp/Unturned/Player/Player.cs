////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void PlayerCreated(Player player);
	public delegate void PlayerTeleported(Player player, Vector3 point);
	public delegate void PlayerSpyReady(CSteamID steamID, byte[] data);

	public struct PlayerSpotLightConfig
	{
		/// <summary>
		/// If true, light contributes to player spotlight. Defaults to true.
		/// 
		/// Can be set to false for modders with a custom light setup. For example, this was added
		/// for a modder who is using melee lights to toggle a lightsaber-style glow.
		/// </summary>
		public bool isEnabled;

		public float range;
		public float angle;
		public Color color;

		public void applyToLight(Light light)
		{
			if (light == null)
				return;

			light.range = range;
			light.spotAngle = angle;
			light.intensity = 1.0f; // Refer to constructor.
			light.color = color;
		}

		public override string ToString()
		{
			return $"(Enabled: {isEnabled} Range: {range}m Angle: {angle}° Color: {color})";
		}

		public PlayerSpotLightConfig(IDatDictionary data)
		{
			isEnabled = data.ParseBool("SpotLight_Enabled", defaultValue: true);
			range = data.ParseFloat("SpotLight_Range", defaultValue: 64.0f);
			angle = data.ParseFloat("SpotLight_Angle", defaultValue: 90.0f);
			// Nelson 2025-03-24: storing both intensity and color was problematic. (public issue #4960)
			// In that case, some lights had [0, 255] colors with a tiny intensity as opposed to the Color type's
			// [0, 1] color with near-1 intensity. When blending these two extremes together you got a medium (~0.5)
			// intensity with out-of-range (>1) color. (Very bright!)
			float intensity = data.ParseFloat("SpotLight_Intensity", defaultValue: 1.3f);
			color = data.LegacyParseColor("SpotLight_Color", new Color32(245, 223, 147, 255)) * intensity;
		}
	}

	/// <summary>
	/// 32-bit mask granting server plugins additional control over custom UIs.
	/// Only replicated to owner.
	/// </summary>
	[System.Flags]
	public enum EPluginWidgetFlags
	{
		None = 0,

		/// <summary>
		/// Enables cursor movement while not in a vanilla menu.
		/// </summary>
		Modal = 1 << 0,

		/// <summary>
		/// Disable background blur regardless of other UI state.
		/// </summary>
		NoBlur = 1 << 1,

		/// <summary>
		/// Enable background blur regardless of other UI state.
		/// Takes precedence over NoBlur.
		/// </summary>
		ForceBlur = 1 << 2,

		/// <summary>
		/// Enable title card while focusing a nearby player.
		/// </summary>
		ShowInteractWithEnemy = 1 << 3,

		/// <summary>
		/// Enable explanation and respawn buttons while dead.
		/// </summary>
		ShowDeathMenu = 1 << 4,

		/// <summary>
		/// Enable health meter in the HUD.
		/// </summary>
		ShowHealth = 1 << 5,

		/// <summary>
		/// Enable food meter in the HUD.
		/// </summary>
		ShowFood = 1 << 6,

		/// <summary>
		/// Enable water meter in the HUD.
		/// </summary>
		ShowWater = 1 << 7,

		/// <summary>
		/// Enable virus/radiation/infection meter in the HUD.
		/// </summary>
		ShowVirus = 1 << 8,

		/// <summary>
		/// Enable stamina meter in the HUD.
		/// </summary>
		ShowStamina = 1 << 9,

		/// <summary>
		/// Enable oxygen meter in the HUD.
		/// </summary>
		ShowOxygen = 1 << 10,

		/// <summary>
		/// Enable icons for bleeding, broken bones, temperature, starving, dehydrating, infected, drowning, full moon,
		/// safezone, and arrested status.
		/// </summary>
		ShowStatusIcons = 1 << 11,

		/// <summary>
		/// Enable UseableGun ammo and firemode in the HUD.
		/// </summary>
		ShowUseableGunStatus = 1 << 12,

		/// <summary>
		/// Enable vehicle fuel, speed, health, battery charge, and locked status in the HUD.
		/// </summary>
		ShowVehicleStatus = 1 << 13,

		/// <summary>
		/// Enable center dot when guns are not equipped.
		/// </summary>
		ShowCenterDot = 1 << 14,

		/// <summary>
		/// Enable popup when in-game rep is increased/decreased.
		/// </summary>
		ShowReputationChangeNotification = 1 << 15,

		ShowLifeMeters = ShowHealth | ShowFood | ShowWater | ShowVirus | ShowStamina | ShowOxygen,

		/// <summary>
		/// Default flags set when player spawns.
		/// </summary>
		Default = ShowInteractWithEnemy | ShowDeathMenu | ShowLifeMeters | ShowStatusIcons | ShowUseableGunStatus | ShowVehicleStatus | ShowCenterDot | ShowReputationChangeNotification,
	}

	/// <summary>
	/// 32-bit mask indicating to the server which admin powers are being used.
	/// Does not control which admin powers are available.
	/// </summary>
	[System.Flags]
	public enum EPlayerAdminUsageFlags
	{
		None = 0,

		/// <summary>
		/// Player is using spectator camera.
		/// </summary>
		Freecam = 1 << 0,

		/// <summary>
		/// Player is using barricade/structure transform tools.
		/// </summary>
		Workzone = 1 << 1,

		/// <summary>
		/// Player is using overlay showing player names and positions.
		/// </summary>
		SpectatorStatsOverlay = 1 << 2,
	}

	public delegate void AdminUsageFlagsChanged(Player player, EPlayerAdminUsageFlags oldFlags, EPlayerAdminUsageFlags newFlags);

	public class Player : MonoBehaviour, IDialogueTarget, IExplosionDamageable
	{
		public static readonly byte SAVEDATA_VERSION = 1;

		public static PlayerCreated onPlayerCreated;
		public static PlayerCreated onPlayerDestroyed;
		public PlayerTeleported onPlayerTeleported;
		public PlayerSpyReady onPlayerSpyReady;
		public static PlayerSpyReady onSpyReady;

		/// <summary>
		/// Per-player event invoked when admin usage flags change.
		/// </summary>
		public event AdminUsageFlagsChanged OnAdminUsageChanged;
		/// <summary>
		/// Event invoked when any player's admin usage flags change.
		/// </summary>
		public static event AdminUsageFlagsChanged OnAnyPlayerAdminUsageChanged;

		public delegate void PlayerStatIncremented(Player player, EPlayerStat stat);
		/// <summary>
		/// Used by plugins.
		/// </summary>
		public static event PlayerStatIncremented onPlayerStatIncremented;

		public delegate void PluginWidgetFlagsChanged(Player player, EPluginWidgetFlags oldFlags);
		/// <summary>
		/// Invoked on client when a plugin changes the widget flags. 
		/// </summary>
		public event PluginWidgetFlagsChanged onLocalPluginWidgetFlagsChanged;

		public static bool isLoadingInventory;
		public static bool isLoadingLife;
		public static bool isLoadingClothing;
		public static bool isLoading => isLoadingLife || isLoadingInventory || isLoadingClothing;

		public int agro;

		private static Player _localPlayer;
		public static Player LocalPlayer
		{
			get
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (Dedicator.IsDedicatedServer)
				{
					throw new System.NotSupportedException("LocalPlayer used on dedicated server!");
				}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				return _localPlayer;
			}
		}

		[System.Obsolete("Renamed to LocalPlayer to avoid confusion with PlayerCaller.player")]
		public static Player player => LocalPlayer;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public static Player instance => LocalPlayer;

		protected SteamChannel _channel;
		public SteamChannel channel => _channel;

		private PlayerAnimator _animator;
		public PlayerAnimator animator => _animator;

		private PlayerClothing _clothing;
		public PlayerClothing clothing => _clothing;

		private PlayerInventory _inventory;
		public PlayerInventory inventory => _inventory;

		private PlayerEquipment _equipment;
		public PlayerEquipment equipment => _equipment;

		private PlayerLife _life;
		public PlayerLife life => _life;

		private PlayerCrafting _crafting;
		public PlayerCrafting crafting => _crafting;

		private PlayerSkills _skills;
		public PlayerSkills skills => _skills;

		private PlayerMovement _movement;
		public PlayerMovement movement => _movement;

		private PlayerLook _look;
		public PlayerLook look => _look;

		private PlayerStance _stance;
		public PlayerStance stance => _stance;

		private PlayerInput _input;
		public PlayerInput input => _input;

		private PlayerVoice _voice;
		public PlayerVoice voice => _voice;

		private PlayerInteract _interact;
		public PlayerInteract interact => _interact;

		private PlayerWorkzone _workzone;
		public PlayerWorkzone workzone => _workzone;

		private PlayerQuests _quests;
		public PlayerQuests quests => _quests;

		private Transform _first;
		public Transform first => _first;

		private Transform _third;
		public Transform third => _third;

		private Transform _character;
		public Transform character => _character;

		private Transform firstSpot;
		private Transform thirdSpot;

		public bool isSpotOn => itemOn || headlampOn;

		private PlayerSpotLightConfig lightConfig
		{
			get
			{
				if (itemOn && headlampOn)
				{
					PlayerSpotLightConfig config = new PlayerSpotLightConfig();
					config.angle = Mathf.LerpAngle(itemLightConfig.angle, headlampLightConfig.angle, 0.5f);
					config.color = Color32.Lerp(itemLightConfig.color, headlampLightConfig.color, 0.5f);
					config.range = Mathf.Lerp(itemLightConfig.range, headlampLightConfig.range, 0.5f);
					return config;
				}
				else if (itemOn)
				{
					return itemLightConfig;
				}
				else if (headlampOn)
				{
					return headlampLightConfig;
				}
				else
				{
					return new PlayerSpotLightConfig();
				}
			}
		}

		private bool itemOn;
		private PlayerSpotLightConfig itemLightConfig;
		private bool headlampOn;
		private PlayerSpotLightConfig headlampLightConfig;

#if !DEDICATED_SERVER
		public OneShotAudioHandle PlayAudioReference(AudioReference audioReference)
		{
			if (Dedicator.IsDedicatedServer)
			{
				return default;
			}

			float volumeMultiplier;
			float pitchMultiplier;
			AudioClip clip = audioReference.LoadAudioClip(out volumeMultiplier, out pitchMultiplier);
			if (clip == null)
			{
				return default;
			}

			OneShotAudioParameters parameters = new OneShotAudioParameters(transform, clip);
			parameters.volume = volumeMultiplier;
			parameters.pitch = pitchMultiplier;
			parameters.SetLinearRolloff(1.0f, 32.0f);
			return parameters.Play();
		}
#endif // !DEDICATED_SERVER

		public OneShotAudioHandle playSound(AudioClip clip, float volume, float pitch, float deviation)
		{
#if !DEDICATED_SERVER
			if (clip == null || Dedicator.IsDedicatedServer)
			{
				return default;
			}

			deviation = Mathf.Clamp01(deviation);

			OneShotAudioParameters parameters = new OneShotAudioParameters(transform, clip);
			parameters.volume = volume;
			parameters.RandomizePitch(pitch * (1.0f - deviation), pitch * (1.0f + deviation));
			parameters.SetLinearRolloff(1.0f, 32.0f);
			return parameters.Play();
#else // DEDICATED_SERVER
			return default;
#endif // DEDICATED_SERVER
		}

		public OneShotAudioHandle playSound(AudioClip clip, float pitch, float deviation)
		{
			return playSound(clip, 1f, pitch, deviation);
		}

		public OneShotAudioHandle playSound(AudioClip clip, float volume)
		{
			return playSound(clip, volume, 1f, 0.1f);
		}

		public OneShotAudioHandle playSound(AudioClip clip)
		{
			return playSound(clip, 1f, 1f, 0.1f);
		}

		private int screenshotsExpected; // int incase someone spams take screenshot command
		private CSteamID screenshotsDestination;
		private Queue<PlayerSpyReady> screenshotsCallbacks = new Queue<PlayerSpyReady>();

		[System.Obsolete]
		public void tellScreenshotDestination(CSteamID steamID)
		{ }

		private static readonly ClientInstanceMethod SendScreenshotDestination = ClientInstanceMethod.Get(typeof(Player), nameof(ReceiveScreenshotDestination));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveScreenshotDestination(in ClientInvocationContext context)
		{
			NetPakReader reader = context.reader;
			ushort length;
			reader.ReadUInt16(out length);
			byte[] data = new byte[length];
			reader.ReadBytes(data);
			HandleScreenshotData(data);
		}

		private void HandleScreenshotData(byte[] data)
		{
			if (Dedicator.IsDedicatedServer)
			{
				ReadWrite.writeBytes(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Spy.jpg", false, false, data);
				ReadWrite.writeBytes(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + "/Spy/" + channel.owner.playerID.steamID.m_SteamID + ".jpg", false, false, data);

				onPlayerSpyReady?.Invoke(channel.owner.playerID.steamID, data);

				PlayerSpyReady callback = screenshotsCallbacks.Dequeue();
				callback?.Invoke(channel.owner.playerID.steamID, data);
			}
			else
			{
				ReadWrite.writeBytes("/Spy.jpg", false, true, data);

				onSpyReady?.Invoke(channel.owner.playerID.steamID, data);
			}
		}

		[System.Obsolete]
		public void tellScreenshotRelay(CSteamID steamID)
		{ }

		private static readonly ServerInstanceMethod SendScreenshotRelay = ServerInstanceMethod.Get(typeof(Player), nameof(ReceiveScreenshotRelay));
		/// <summary>
		/// Not rate limited because server tracks number of expected screenshots.
		/// </summary>
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER)]
		public void ReceiveScreenshotRelay(in ServerInvocationContext context)
		{
			NetPakReader reader = context.reader;

			if (screenshotsExpected < 1)
			{
				context.Kick("server was not expecting a screenshot");
				return;
			}

			screenshotsExpected--;

			ushort length;
			if (!reader.ReadUInt16(out length))
			{
				// kick?
				return;
			}

			byte[] data = new byte[length];
			reader.ReadBytes(data);

			if (screenshotsDestination != CSteamID.Nil)
			{
				ITransportConnection transportConnection = Provider.findTransportConnection(screenshotsDestination);
				if (transportConnection != null)
				{
					SendScreenshotDestination.Invoke(GetNetId(), ENetReliability.Reliable, transportConnection,
						SendScreenshotDestination_Write, length, data);
				}
			}

			HandleScreenshotData(data);
		}

		private void SendScreenshotDestination_Write(NetPakWriter writer, ushort length, byte[] data)
		{
			writer.WriteUInt16(length);
			writer.WriteBytes(data);
		}

		private Texture2D screenshotFinal;

		private IEnumerator takeScreenshot()
		{
			yield return new WaitForEndOfFrame();

			UnityEngine.Profiling.Profiler.BeginSample("Spy");

			// We could improve performance further by capturing to rendertexture and using async gpu read,
			// but texture handles upside-down flip for us.
			Texture2D screenCaptureTexture = ScreenCapture.CaptureScreenshotAsTexture();
			RenderTexture downsampleRenderTexture = RenderTexture.GetTemporary(640, 480, /*depthBuffer*/ 0, screenCaptureTexture.graphicsFormat);
			Graphics.Blit(screenCaptureTexture, downsampleRenderTexture);
			Destroy(screenCaptureTexture);

			if (screenshotFinal == null)
			{
				screenshotFinal = new Texture2D(640, 480, TextureFormat.RGB24, false);
				screenshotFinal.name = "Screenshot_Final";
				screenshotFinal.hideFlags = HideFlags.HideAndDontSave;
			}

			RenderTexture.active = downsampleRenderTexture;
			screenshotFinal.ReadPixels(new Rect(0, 0, screenshotFinal.width, screenshotFinal.height), 0, 0, /*recalculateMipMaps*/ false);
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(downsampleRenderTexture);

			byte[] data = screenshotFinal.EncodeToJPG(33);

			if (data.Length < 40000)
			{
				if (Provider.isServer)
				{
					HandleScreenshotData(data);
				}
				else
				{
					SendScreenshotRelay.Invoke(GetNetId(), ENetReliability.Reliable, SendScreenshotRelay_Write, data);
				}
			}
			else
			{
				UnturnedLog.warn($"Unable to send screenshot to server because size ({data.Length} bytes) exceeds limit");
			}

			UnityEngine.Profiling.Profiler.EndSample();
		}

		private void SendScreenshotRelay_Write(NetPakWriter writer, byte[] data)
		{
			ushort length = (ushort) data.Length;
			writer.WriteUInt16(length);
			writer.WriteBytes(data, length);
		}

		[System.Obsolete]
		public void askScreenshot(CSteamID steamID)
		{
			ReceiveTakeScreenshot();
		}

		private static readonly ClientInstanceMethod SendTakeScreenshot = ClientInstanceMethod.Get(typeof(Player), nameof(ReceiveTakeScreenshot));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askScreenshot))]
		public void ReceiveTakeScreenshot()
		{
			StartCoroutine(takeScreenshot());
		}

		public void sendScreenshot(CSteamID destination, PlayerSpyReady callback = null)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			screenshotsExpected++;
			screenshotsDestination = destination;
			screenshotsCallbacks.Enqueue(callback);
			SendTakeScreenshot.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection());
		}

		[System.Obsolete]
		public void askBrowserRequest(CSteamID steamID, string msg, string url)
		{
			ReceiveBrowserRequest(msg, url);
		}

		private static readonly ClientInstanceMethod<string, string> SendBrowserRequest = ClientInstanceMethod<string, string>.Get(typeof(Player), nameof(ReceiveBrowserRequest));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askBrowserRequest))]
		public void ReceiveBrowserRequest(string msg, string url)
		{
			// Nelson 2024-08-19: We don't send the parsed URL to the UI anymore so that the original link is shown
			// rather than the one redirected to Steam's link filter.
			if (!WebUtils.CanParseThirdPartyUrl(url))
			{
				UnturnedLog.warn("Ignoring potentially unsafe browser request \"{0}\" \"{1}\"", msg, url);
				return;
			}

			if (PlayerUI.instance != null)
			{
				PlayerUI.instance.browserRequestUI.open(msg, url);
				PlayerLifeUI.close();
			}
		}

		/// <summary>
		/// Request client to open a given URL.
		/// Allows plugins to open web browser, but also gives client the chance to ignore it.
		/// </summary>
		public void sendBrowserRequest(string msg, string url)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			SendBrowserRequest.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), msg, url);
		}

		private static readonly ClientInstanceMethod<string, float> SendHintMessage = ClientInstanceMethod<string, float>.Get(typeof(Player), nameof(ReceiveHintMessage));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveHintMessage(string message, float durationSeconds)
		{
			if (PlayerUI.instance != null)
			{
				ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref message);
				message = message.Replace("<name_char>", channel.owner.playerID.characterName);

				PlayerUI.message(EPlayerMessage.NPC_CUSTOM, message, durationSeconds);
			}
		}

		private static readonly ClientInstanceMethod<System.Guid, string, float> SendTranslatedHint = ClientInstanceMethod<System.Guid, string, float>.Get(typeof(Player), nameof(ReceiveTranslatedHint));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveTranslatedHint(System.Guid assetGuid, string translationKey, float durationSeconds)
		{
			if (PlayerUI.instance == null)
				return;

			Asset asset = Assets.find(assetGuid);
			if (asset == null)
			{
				UnturnedLog.warn($"Missing asset for replicated hint! GUID: {assetGuid:N} Translation Key: \"{translationKey}\"");
				return;
			}

			if (asset.Localization == null)
			{
				UnturnedLog.warn($"Missing translation data for replicated hint! Asset: {asset.FriendlyNameWithFriendlyType} Translation Key: \"{translationKey}\"");
				return;
			}

			string text = asset.Localization.FormatOrNull(translationKey);
			if (text == null)
			{
				UnturnedLog.warn($"Replicated hint text is empty! Asset: {asset.FriendlyNameWithFriendlyType} Translation Key: \"{translationKey}\"");
				return;
			}

			text = ItemTool.filterRarityRichText(text); // Matches INPCReward default text filtering.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			UnturnedLog.info($"Replicated hint: \"{text}\" Asset: {asset.FriendlyNameWithFriendlyType} Key: \"{translationKey}\"");
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
			ReceiveHintMessage(text, durationSeconds);
		}

		public void ServerShowHint(string message, float durationSeconds)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			SendHintMessage.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), message, durationSeconds);
		}

		public void ServerShowTranslatedHint(Asset asset, string translationKey, float durationSeconds)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			if (asset == null || string.IsNullOrEmpty(translationKey))
				return;
			SendTranslatedHint.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), asset.GUID, translationKey, durationSeconds);
		}

		[System.Obsolete]
		public void askRelayToServer(CSteamID steamID, uint ip, ushort port, string password, bool shouldShowMenu)
		{
			ReceiveRelayToServer(ip, port, CSteamID.Nil, password, shouldShowMenu);
		}

		private static readonly ClientInstanceMethod<uint, ushort, CSteamID, string, bool> SendRelayToServer = ClientInstanceMethod<uint, ushort, CSteamID, string, bool>.Get(typeof(Player), nameof(ReceiveRelayToServer));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
		public void ReceiveRelayToServer(uint ip, ushort port, CSteamID serverCode, string password, bool shouldShowMenu)
		{
			if (MenuPlayConnectUI.hasPendingServerRelay)
			{
				// Already received a relay request? (hasPendingServerRelay is reset at main menu)
				// Do not mess with our existing relay process.
				return;
			}

			if (Provider.isServer)
			{
				// A host actually sent in a log where a plugin was calling this causing server to switch to client
				// mode, so now we throw an exception.
				throw new System.NotSupportedException(string.Format("IP: {0} Port: {1} Server Code: {2}", Parser.getIPFromUInt32(ip), port, serverCode));
			}

			MenuPlayConnectUI.hasPendingServerRelay = true;
			MenuPlayConnectUI.serverRelayIP = ip;
			MenuPlayConnectUI.serverRelayPort = port;
			MenuPlayConnectUI.serverRelayServerCode = serverCode;
			MenuPlayConnectUI.serverRelayPassword = password;
			MenuPlayConnectUI.serverRelayWaitOnMenu = shouldShowMenu;

			Provider.RequestDisconnect($"Relaying to IP: {Parser.getIPFromUInt32(ip)} Port: {port} Code: {serverCode}");
		}

		/// <summary>
		/// Tell client to join a specific server.
		/// Disconnects client and sends them to the join server screen.
		/// Only used by plugins.
		/// </summary>
		public void sendRelayToServer(uint ip, ushort port, string password, bool shouldShowMenu = true)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			SendRelayToServer.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), ip, port, CSteamID.Nil, password, shouldShowMenu);
		}

		public void sendRelayToServer(CSteamID serverCode, string password, bool shouldShowMenu = true)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			SendRelayToServer.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), 0, 0, serverCode, password, shouldShowMenu);
		}

		public void sendRelayToServer(uint ip, ushort port, string password)
		{
			sendRelayToServer(ip, port, password, shouldShowMenu: true);
		}

		/// <summary>
		/// Is this player currently in a plugin's modal dialog?
		/// Enables cursor movement while not in a vanilla menu.
		/// </summary>
		public bool inPluginModal => isPluginWidgetFlagActive(EPluginWidgetFlags.Modal);

		public bool isPluginWidgetFlagActive(EPluginWidgetFlags flag)
		{
			return (pluginWidgetFlags & flag) == flag;
		}

		public EPluginWidgetFlags pluginWidgetFlags
		{
			get;
			protected set;
		} = EPluginWidgetFlags.Default;

		[System.Obsolete]
		public void clientsideSetPluginWidgetFlags(CSteamID steamID, uint newFlags)
		{
			ReceiveSetPluginWidgetFlags(newFlags);
		}

		private static readonly ClientInstanceMethod<uint> SendSetPluginWidgetFlags = ClientInstanceMethod<uint>.Get(typeof(Player), nameof(ReceiveSetPluginWidgetFlags));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(clientsideSetPluginWidgetFlags))]
		public void ReceiveSetPluginWidgetFlags(uint newFlags)
		{
			EPluginWidgetFlags oldFlags = pluginWidgetFlags;
			pluginWidgetFlags = (EPluginWidgetFlags) newFlags;
			onLocalPluginWidgetFlagsChanged?.Invoke(this, oldFlags);
		}

		public void setAllPluginWidgetFlags(EPluginWidgetFlags newFlags)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			if (pluginWidgetFlags == newFlags)
				return;

			SendSetPluginWidgetFlags.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), (uint) newFlags);
			pluginWidgetFlags = newFlags; // After RPC because we want to compare oldFlags with newFlags in singleplayer.
		}

		public void enablePluginWidgetFlag(EPluginWidgetFlags flag)
		{
			EPluginWidgetFlags desiredFlags = pluginWidgetFlags | flag;
			setAllPluginWidgetFlags(desiredFlags);
		}

		public void disablePluginWidgetFlag(EPluginWidgetFlags flag)
		{
			EPluginWidgetFlags desiredFlags = pluginWidgetFlags & ~flag;
			setAllPluginWidgetFlags(desiredFlags);
		}

		public void setPluginWidgetFlag(EPluginWidgetFlags flag, bool active)
		{
			if (active)
				enablePluginWidgetFlag(flag);
			else
				disablePluginWidgetFlag(flag);
		}

		/// <summary>
		/// Tell the client whether to be in plugin modal mode or not.
		/// Kept from prior to introduction of pluginWidgetFlags.
		/// </summary>
		[System.Obsolete]
		public void serversideSetPluginModal(bool enableModal)
		{
			setPluginWidgetFlag(EPluginWidgetFlags.Modal, enableModal);
		}

		/// <summary>
		/// If true, bypass player culling test as if freecam overlay were active.
		/// Enables plugins to implement a custom admin culling bypass switch. (Was requested.)
		/// Defaults to false.
		/// </summary>
		public bool ServerAllowKnowledgeOfAllClientPositions
		{
			get;
			set;
		}

		private EPlayerAdminUsageFlags _adminUsageFlags;
		/// <summary>
		/// Which admin powers are currently in use by the client.
		/// Reported to the server by the client.
		/// Does not control which admin powers are available.
		/// Note: Hacks can prevent this notification from being sent.
		/// </summary>
		public EPlayerAdminUsageFlags AdminUsageFlags
		{
			get => _adminUsageFlags;
		}

		private static readonly ServerInstanceMethod<uint> SendAdminUsageFlags = ServerInstanceMethod<uint>.Get(typeof(Player), nameof(ReceiveAdminUsageFlags));
		[SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 8)]
		public void ReceiveAdminUsageFlags(in ServerInvocationContext context, uint newFlagsBitmask)
		{
			EPlayerAdminUsageFlags newFlags;
			try
			{
				newFlags = (EPlayerAdminUsageFlags) newFlagsBitmask;
			}
			catch
			{
				context.Kick($"invalid admin usage flags");
				return;
			}

			if (_adminUsageFlags != newFlags)
			{
				EPlayerAdminUsageFlags oldFlags = _adminUsageFlags;
				_adminUsageFlags = newFlags;

				if (oldFlags.HasFlag(EPlayerAdminUsageFlags.Freecam) != newFlags.HasFlag(EPlayerAdminUsageFlags.Freecam))
				{
					if (newFlags.HasFlag(EPlayerAdminUsageFlags.Freecam))
					{
						UnturnedLog.info($"{channel.owner.playerID} entered freecam admin mode");

						if (!look.canUseFreecam)
						{
							context.Kick("freecam not allowed");
							return;
						}
					}
					else
					{
						UnturnedLog.info($"{channel.owner.playerID} exited freecam admin mode");
					}
				}

				if (oldFlags.HasFlag(EPlayerAdminUsageFlags.Workzone) != newFlags.HasFlag(EPlayerAdminUsageFlags.Workzone))
				{
					if (newFlags.HasFlag(EPlayerAdminUsageFlags.Workzone))
					{
						UnturnedLog.info($"{channel.owner.playerID} entered workzone admin mode");

						if (!look.canUseWorkzone)
						{
							context.Kick("workzone not allowed");
							return;
						}
					}
					else
					{
						UnturnedLog.info($"{channel.owner.playerID} exited workzone admin mode");
					}
				}

				if (oldFlags.HasFlag(EPlayerAdminUsageFlags.SpectatorStatsOverlay) != newFlags.HasFlag(EPlayerAdminUsageFlags.SpectatorStatsOverlay))
				{
					if (newFlags.HasFlag(EPlayerAdminUsageFlags.SpectatorStatsOverlay))
					{
						UnturnedLog.info($"{channel.owner.playerID} turned on spectator stats overlay admin mode");

						if (!look.canUseSpecStats)
						{
							context.Kick("specstats not allowed");
							return;
						}
					}
					else
					{
						UnturnedLog.info($"{channel.owner.playerID} turned off spectator stats overlay admin mode");
					}
				}

				OnAdminUsageChanged?.Invoke(this, oldFlags, newFlags);
				OnAnyPlayerAdminUsageChanged?.Invoke(this, oldFlags, newFlags);
			}
		}

		/// <summary>
		/// Called on the client to notify the server of admin usage changes (if any).
		/// </summary>
		private void ClientSetAdminUsageFlags(EPlayerAdminUsageFlags newFlags)
		{
			if (_adminUsageFlags != newFlags)
			{
				_adminUsageFlags = newFlags;
				SendAdminUsageFlags.Invoke(GetNetId(), ENetReliability.Reliable, (uint) _adminUsageFlags);
			}
		}

		/// <summary>
		/// Called on the client to notify the server of admin usage changes (if any).
		/// </summary>
		internal void ClientSetAdminUsageFlagActive(EPlayerAdminUsageFlags flag, bool active)
		{
			if (active)
			{
				ClientSetAdminUsageFlags(_adminUsageFlags | flag);
			}
			else
			{
				ClientSetAdminUsageFlags(_adminUsageFlags & ~flag);
			}
		}

#if WITH_THIRDPARTYAC
		/// <summary>
		/// Older versions had a console command to request enabling this. It's only kept in case plugins were setting it.
		/// </summary>
		public bool WantsThirdPartyAnticheatDebugMessages
		{
			get;
			set;
		}
#endif

		[System.Obsolete]
		public void tellTerminalRelay(CSteamID steamID, string internalMessage)
		{
			ReceiveTerminalRelay(internalMessage);
		}

		private static readonly ClientInstanceMethod<string> SendTerminalRelay = ClientInstanceMethod<string>.Get(typeof(Player), nameof(ReceiveTerminalRelay));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellTerminalRelay))]
		public void ReceiveTerminalRelay(string internalMessage)
		{
			UnturnedLog.info(internalMessage);
		}

		[System.Obsolete]
		public void sendTerminalRelay(string internalMessage, string internalCategory, string displayCategory)
		{
			sendTerminalRelay(internalMessage);
		}

		public void sendTerminalRelay(string internalMessage)
		{
			SendTerminalRelay.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), internalMessage);
		}

		internal void PostTeleport()
		{
			onPlayerTeleported?.Invoke(this, transform.position);

#if !DEDICATED_SERVER
			if (channel.IsLocalPlayer)
			{
				CullingVolumeManager.Get().OnPlayerTeleported();
			}
#endif // !DEDICATED_SERVER
		}

		[System.Obsolete]
		public void askTeleport(CSteamID steamID, Vector3 position, byte angle)
		{
			ReceiveTeleport(position, angle);
		}

		// If changing the offset you should adjust padding in teleportToLocation and hasTeleportClearanceAtPosition.
		internal const float TELEPORT_VERTICAL_OFFSET = 0.5f;

		private static readonly ClientInstanceMethod<Vector3, byte> SendTeleport = ClientInstanceMethod<Vector3, byte>.Get(typeof(Player), nameof(ReceiveTeleport));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askTeleport))]
		public void ReceiveTeleport(Vector3 position, byte angle)
		{
			bool wasControllerEnabled = false;
			if (movement.controller != null)
			{
				movement.controller.DisableDetectCollisionsUntilNextFrame();
				wasControllerEnabled = movement.controller.enabled;
				movement.controller.enabled = false;
			}

			transform.position = position + new Vector3(0.0f, TELEPORT_VERTICAL_OFFSET, 0.0f);
			transform.rotation = Quaternion.Euler(0, angle * 2, 0);

			if (wasControllerEnabled)
			{
				// Similar to PlayerInput rewind, suspicious that we might need to re-enable here to ensure controller
				// on server is using the new position after teleporting.
				// Edit 2021-11-24: yes this is definitely required because at first I forgot to actually disable movement. :(
				// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/2890
				movement.controller.enabled = true;
			}

			look.updateLook();
			movement.updateMovement();

			PostTeleport();

			if (Provider.isServer)
			{
				input.serverBoundsHistory.Clear();
				input.serverBoundsHistory.AddCharacterControllerBounds(movement.controller);
			}
		}

		public void sendTeleport(Vector3 position, byte angle)
		{
			CommandWindow.LogWarning("Please use teleportToPlayer or teleportToLocation rather than sendTeleport, as they check for error conditions and safe space");
			teleportToLocation(position, angle);
		}

		public bool teleportToPlayer(Player otherPlayer)
		{
			if (otherPlayer == null)
				return false;

			if (otherPlayer.movement.getVehicle() != null)
				return false;

			Vector3 position = otherPlayer.transform.position;
			float yaw = otherPlayer.transform.rotation.eulerAngles.y;
			return teleportToLocation(position, yaw);
		}

		public bool teleportToLocation(Vector3 position, float yaw)
		{
			// askTeleport offsets by 0.5 upward which I don't want to mess with, so we request that much padding.
			const float padding = 0.5f;

			if (!stance.wouldHaveHeightClearanceAtPosition(position, padding: padding))
				return false;

			teleportToLocationUnsafe(position, yaw);
			return true;
		}

		/// <summary>
		/// Teleport to a random player spawn designated in the level.
		/// </summary>
		public bool teleportToRandomSpawnPoint()
		{
			PlayerSpawnpoint spawnpoint = LevelPlayers.getSpawn(false);
			if (spawnpoint != null)
			{
				teleportToLocationUnsafe(spawnpoint.point + new Vector3(0, 0.5f, 0), spawnpoint.angle);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Teleport to bed, if player has set one.
		/// </summary>
		public bool teleportToBed()
		{
			Vector3 point;
			byte angle;
			if (BarricadeManager.tryGetBed(channel.owner.playerID.steamID, out point, out angle))
			{
				point.y += 0.5f; // Respawn uses this offset.
				float yaw = MeasurementTool.byteToAngle(angle);
				return teleportToLocation(point, yaw);
			}
			else
			{
				return false;
			}
		}

		public bool adjustStanceOrTeleportIfStuck()
		{
			return stance.adjustStanceOrTeleportIfStuck();
		}

		/// <summary>
		/// Teleport is always handled by owner and locally (loopback), but *not* by culled clients.
		/// </summary>
		private PooledTransportConnectionList GatherTeleportRemoteClientConnections(Vector3 destination)
		{
			// Prior to culling this was just Provider.GatherRemoteClientConnections.

			SteamPlayer teleportingClient = channel.owner;

			PooledTransportConnectionList list = TransportConnectionListPool.Get();
			foreach (SteamPlayer client in Provider._clients)
			{
#if !DEDICATED_SERVER
				if (client.IsLocalServerHost)
					continue;
#endif // !DEDICATED_SERVER

				if (client == teleportingClient)
				{
					// Always notify self of the teleport.
					list.Add(client.transportConnection);
					continue;
				}

				if (client.model == null) // error/bug
					continue;

				Vector3 recipientPosition = client.model.transform.position;
				// There are four cases:
				//
				// 1. Teleporting player was already culled by client and will remain culled:
				//    Nothing changes. We don't send the new position.
				//
				// 2. Teleporting player wasn't culled by client and won't be culled:
				//    New position is sent as normal.
				//
				// 3. Teleporting player was culled by client but the new position is visible:
				//    We send the new position and will remove from culled players in next PlayerManager update.
				//
				// 4. Teleporting player wasn't culled by client but the new position is culled:
				//    Don't send the new position and will add to culled players in next PlayerManager update.
				bool culled = PlayerManager.IsPlayerCulledAtPosition(teleportingClient, destination, client, recipientPosition);
				if (culled)
					continue;

				list.Add(client.transportConnection);
			}
			return list;
		}

		/// <summary>
		/// Get simulation center of mass.
		/// </summary>
		public Vector3 GetCapsuleCenter()
		{
			CharacterController cc = movement?.controller;
			float height = cc != null ? cc.height : PlayerMovement.HEIGHT_STAND;
			return transform.TransformPoint(0.0f, height * 0.5f, 0.0f);
		}

		public void teleportToLocationUnsafe(Vector3 position, float yaw)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			InteractableVehicle vehicle = movement.getVehicle();
			if (vehicle == null)
			{
				byte netAngle = MeasurementTool.angleToByte(yaw);
				if (movement.canAddSimulationResultsToUpdates)
				{
					SendTeleport.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, GatherTeleportRemoteClientConnections(position), position, netAngle);
				}
				else
				{
					// Not used in vanilla. Plugins want to hide the admin true position in "vanish" mode.
					SendTeleport.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), position, netAngle);
					ReceiveTeleport(position, netAngle);
				}
			}
			else
			{
				VehicleManager.removePlayerTeleportUnsafe(vehicle, this, position, yaw);
			}
		}

		[System.Obsolete]
		public void tellStat(CSteamID steamID, byte newStat)
		{
			ReceiveStat((EPlayerStat) newStat);
		}

		private static readonly ClientInstanceMethod<EPlayerStat> SendStat = ClientInstanceMethod<EPlayerStat>.Get(typeof(Player), nameof(ReceiveStat));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellStat))]
		public void ReceiveStat(EPlayerStat stat)
		{
			if (stat == EPlayerStat.NONE)
			{
				return;
			}

			trackStat(stat);

			if (stat == EPlayerStat.KILLS_PLAYERS)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Players", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Players", data + 1);
				}
			}
			else if (stat == EPlayerStat.KILLS_ZOMBIES_NORMAL)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Normal", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Zombies_Normal", data + 1);
				}
			}
			else if (stat == EPlayerStat.KILLS_ZOMBIES_MEGA)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Mega", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Zombies_Mega", data + 1);
				}
			}
			else if (stat == EPlayerStat.FOUND_ITEMS)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Items", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Items", data + 1);
				}
			}
			else if (stat == EPlayerStat.FOUND_RESOURCES)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Resources", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Resources", data + 1);
				}
			}
			else if (stat == EPlayerStat.KILLS_ANIMALS)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Animals", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Kills_Animals", data + 1);
				}
			}
			else if (stat == EPlayerStat.FOUND_CRAFTS)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Crafts", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Crafts", data + 1);
				}
			}
			else if (stat == EPlayerStat.FOUND_FISHES)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Fishes", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Fishes", data + 1);
				}
			}
			else if (stat == EPlayerStat.FOUND_PLANTS)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Plants", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Plants", data + 1);
				}
			}
			else if (stat == EPlayerStat.ARENA_WINS)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Arena_Wins", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Arena_Wins", data + 1);
				}
			}
			else if (stat == EPlayerStat.FOUND_BUILDABLES)
			{
				int data;
				if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Buildables", out data))
				{
					Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Buildables", data + 1);
				}
			}
		}

		[System.Obsolete]
		public void tellAchievementUnlocked(CSteamID steamID, string id)
		{
			ReceiveAchievementUnlocked(id);
		}

		private static readonly ClientInstanceMethod<string> SendAchievementUnlocked = ClientInstanceMethod<string>.Get(typeof(Player), nameof(ReceiveAchievementUnlocked));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(tellAchievementUnlocked))]
		public void ReceiveAchievementUnlocked(string id)
		{
			if (Provider.statusData.Achievements.canBeGrantedByNPC(id))
			{
				bool data;
				if (Provider.provider.achievementsService.getAchievement(id, out data) && !data)
				{
					Provider.provider.achievementsService.setAchievement(id);
				}
			}
			else
			{
				UnturnedLog.warn($"Achievement {id} cannot be unlocked by NPCs");
			}
		}

		protected void trackStat(EPlayerStat stat)
		{
			// Typically, only the first case here will happen. But if the player is driving a vehicle
			// with a turret in the pilot's seat then kills should probably count for both.

			if (equipment.HasValidUseable && equipment.IsEquipAnimationFinished && equipment.asset != null)
			{
				channel.owner.incrementStatTrackerValue(equipment.asset.sharedSkinLookupID, stat);
			}

			InteractableVehicle vehicle = movement.getVehicle();
			if (vehicle != null && movement.getSeat() == 0 && vehicle.asset != null)
			{
				channel.owner.incrementStatTrackerValue(vehicle, stat);
			}
		}

		public void sendStat(EPlayerKill kill)
		{
			if (kill == EPlayerKill.PLAYER)
			{
				sendStat(EPlayerStat.KILLS_PLAYERS);
			}
			else if (kill == EPlayerKill.ZOMBIE)
			{
				sendStat(EPlayerStat.KILLS_ZOMBIES_NORMAL);
			}
			else if (kill == EPlayerKill.MEGA)
			{
				sendStat(EPlayerStat.KILLS_ZOMBIES_MEGA);
			}
			else if (kill == EPlayerKill.ANIMAL)
			{
				sendStat(EPlayerStat.KILLS_ANIMALS);
			}
			else if (kill == EPlayerKill.RESOURCE)
			{
				sendStat(EPlayerStat.FOUND_RESOURCES);
			}
		}

		public void sendStat(EPlayerStat stat)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			if (!channel.IsLocalPlayer)
			{
				trackStat(stat);
			}

			onPlayerStatIncremented?.Invoke(this, stat);

			SendStat.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), stat);
		}

		public void sendAchievementUnlocked(string id)
		{
			ThreadUtil.ConditionalAssertIsGameThread();
			SendAchievementUnlocked.Invoke(GetNetId(), ENetReliability.Reliable, channel.GetOwnerTransportConnection(), id);
		}

		[System.Obsolete]
		public void askMessage(CSteamID steamID, byte message)
		{
			ReceiveUIMessage((EPlayerMessage) message);
		}

		private static readonly ClientInstanceMethod<EPlayerMessage> SendUIMessage = ClientInstanceMethod<EPlayerMessage>.Get(typeof(Player), nameof(ReceiveUIMessage));
		[SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = nameof(askMessage))]
		public void ReceiveUIMessage(EPlayerMessage message)
		{
			PlayerUI.message(message, string.Empty);
		}

		public void sendMessage(EPlayerMessage message)
		{
			SendUIMessage.Invoke(GetNetId(), ENetReliability.Unreliable, channel.GetOwnerTransportConnection(), message);
		}

		public void enableItemSpotLight(PlayerSpotLightConfig config)
		{
			itemLightConfig = config;
			itemOn = config.isEnabled;
			updateLights();
		}

		public void disableItemSpotLight()
		{
			itemOn = false;
			updateLights();
		}

		public void updateGlassesLights(bool on)
		{
			if (clothing.firstClothes != null && clothing.firstClothes.glassesModel != null)
			{
				Transform model = clothing.firstClothes.glassesModel.Find("Model_0");
				if (model != null)
				{
					Transform light = model.Find("Light");
					if (light != null)
					{
						light.gameObject.SetActive(on);
					}
				}
			}

			if (clothing.thirdClothes != null && clothing.thirdClothes.glassesModel != null)
			{
				Transform model = clothing.thirdClothes.glassesModel.Find("Model_0");
				if (model != null)
				{
					Transform light = model.Find("Light");
					if (light != null)
					{
						light.gameObject.SetActive(on);
					}
				}
			}

			if (clothing.characterClothes != null && clothing.characterClothes.glassesModel != null)
			{
				Transform model = clothing.characterClothes.glassesModel.Find("Model_0");
				if (model != null)
				{
					Transform light = model.Find("Light");
					if (light != null)
					{
						light.gameObject.SetActive(on);
					}
				}
			}
		}

		public void enableHeadlamp(PlayerSpotLightConfig config)
		{
			headlampLightConfig = config;
			headlampOn = config.isEnabled;
			updateLights();
		}

		public void disableHeadlamp()
		{
			headlampOn = false;
			updateLights();
		}

		private void updateLights()
		{
			if (!Dedicator.IsDedicatedServer)
			{
				if (channel.IsLocalPlayer)
				{
					firstSpot.gameObject.SetActive(isSpotOn && look.perspective == EPlayerPerspective.FIRST);
					thirdSpot.gameObject.SetActive(isSpotOn && look.perspective == EPlayerPerspective.THIRD);
				}
				else
				{
					thirdSpot.gameObject.SetActive(isSpotOn);
				}

				if (isSpotOn)
				{
					PlayerSpotLightConfig config = lightConfig; // Get blended config.

					if (firstSpot != null)
					{
						config.applyToLight(firstSpot.GetComponent<Light>());
					}

					if (thirdSpot != null)
					{
						config.applyToLight(thirdSpot.GetComponent<Light>());
					}
				}
			}
		}

		private void onPerspectiveUpdated(EPlayerPerspective newPerspective)
		{
			if (isSpotOn)
			{
				updateLights();
			}
		}

		/// <summary>
		/// How many calls to <see cref="tryToPerformRateLimitedAction"/> will succeed per second.
		/// </summary>
		public uint maxRateLimitedActionsPerSecond = 10;

		/// <summary>
		/// How many rate limited actions have been performed recently.
		/// Increased after performing each rate limited action, and decreased over time.
		/// Cannot perform actions when greater than one.
		/// </summary>
		public float rateLimitedActionsCredits
		{
			get;
			protected set;
		}

		/// <summary>
		/// Note: new official code should be using per-method rate limit attribute.
		/// This is kept for backwards compatibility with plugins however.
		/// 
		/// Call this method before any requests the client can spam to the server.
		/// </summary>
		/// <returns>Should your code proceed with the rate limited action?</returns>
		public bool tryToPerformRateLimitedAction()
		{
			bool canPerform = rateLimitedActionsCredits < 1.0f;
			if (canPerform)
			{
				rateLimitedActionsCredits += 1.0f / maxRateLimitedActionsPerSecond;
			}
			return canPerform;
		}

		/// <summary>
		/// Call every frame to cool down rate limiting.
		/// </summary>
		protected void updateRateLimiting()
		{
			rateLimitedActionsCredits -= Time.deltaTime;
			if (rateLimitedActionsCredits < 0.0f)
			{
				rateLimitedActionsCredits = 0.0f;
			}
		}

		private void Update()
		{
			if (Provider.isServer)
			{
				updateRateLimiting();
			}
		}

		/// <summary>
		/// This code was in the Start message, and should happen before other initialization.
		/// </summary>
		private void InitializePlayerStart()
		{
			if (channel.IsLocalPlayer)
			{
				_localPlayer = this;

				_first = transform.Find("First");
				_third = transform.Find("Third");

				//Destroy(transform.FindChild("Third").gameObject);
				first.gameObject.SetActive(true);
				third.gameObject.SetActive(true);
				//transform.FindChild("Third").gameObject.SetActive(true);

				string inspectPath =
#if WITH_NOREDIST
				"Characters_NoRedist/Inspect";
#else
				"Characters/Inspect";
#endif
				_character = Instantiate(Resources.Load<GameObject>(inspectPath)).transform;
				character.name = "Inspect";
				character.transform.position = new Vector3(256, -256, 0);
				character.transform.rotation = Quaternion.Euler(90, 0, 0);

				firstSpot = MainCamera.instance.transform.Find("Spot");
				firstSpot.localPosition = Vector3.zero; // Eh bit of a hack. Don't want to touch the prefab though.

				isLoadingInventory = true;
				isLoadingLife = true;
				isLoadingClothing = true;

				look.onPerspectiveUpdated += onPerspectiveUpdated;
			}
			else
			{
				_first = null; // redundant, just for clarity
				_third = transform.Find("Third");

				third.gameObject.SetActive(true);
			}

			thirdSpot = third.Find("Skeleton").Find("Spine").Find("Skull").Find("Spot");
		}

		internal void AssignNetIdBlock(NetId baseId)
		{
			_netId = ++baseId;
			NetIdRegistry.Assign(_netId, this);
			NetIdRegistry.AssignTransform(++baseId, transform);

			// Sorted alphabetically. If adding or removing entries please make sure to adjust block size.
			_animator.AssignNetId(++baseId);
			_clothing.AssignNetId(++baseId);
			_crafting.AssignNetId(++baseId);
			_equipment.AssignNetId(++baseId);
			_input.AssignNetId(++baseId);
			_interact.AssignNetId(++baseId);
			_inventory.AssignNetId(++baseId);
			_life.AssignNetId(++baseId);
			_look.AssignNetId(++baseId);
			_movement.AssignNetId(++baseId);
			_quests.AssignNetId(++baseId);
			_skills.AssignNetId(++baseId);
			_stance.AssignNetId(++baseId);
			_voice.AssignNetId(++baseId);
			// _workzone.SetAndRegisterNetId(++baseId); // Workzone component does not exist on server.
		}

		/// <summary>
		/// Hacky replacement for Start() that runs after net ids are assigned but before sending player state.
		/// </summary>
		internal void InitializePlayer()
		{
			// 2022-10-13 this is a workaround for thirdperson-only servers which reparent the PlayerUI component during initialization.
			PlayerUI playerUI = null;
			if (channel.IsLocalPlayer)
			{
				playerUI = transform.Find("First")?.Find("Camera")?.GetComponent<PlayerUI>();
			}

			// This is old fragile code that used to run in Start, and the components were in this order.
			InitializePlayerStart();
			clothing.InitializePlayer();
			inventory.InitializePlayer();
			life.InitializePlayer();
			skills.InitializePlayer();
			crafting.InitializePlayer();
			stance.InitializePlayer();
			movement.InitializePlayer();
			look.InitializePlayer();
			interact.InitializePlayer();
			animator.InitializePlayer();
			equipment.InitializePlayer();
			input.InitializePlayer();
			voice.InitializePlayer();
			if (workzone != null)
			{
				workzone.InitializePlayer();
			}
			quests.InitializePlayer();

			if (playerUI != null)
			{
				playerUI.InitializePlayer();
			}
		}

		internal void SendInitialPlayerState(SteamPlayer client)
		{
			clothing.SendInitialPlayerState(client);
			inventory.SendInitialPlayerState(client); // Only sent if owner.
			life.SendInitialPlayerState(client);
			skills.SendInitialPlayerState(client);
			stance.SendInitialPlayerState(client);
			quests.SendInitialPlayerState(client);
			equipment.SendInitialPlayerState(client);
			animator.SendInitialPlayerState(client);
		}

		internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
		{
			clothing.SendInitialPlayerState(transportConnections);
			life.SendInitialPlayerState(transportConnections);
			skills.SendInitialPlayerState(transportConnections);
			stance.SendInitialPlayerState(transportConnections);
			quests.SendInitialPlayerState(transportConnections);
			equipment.SendInitialPlayerState(transportConnections);
			animator.SendInitialPlayerState(transportConnections);
		}

		internal void ReleaseNetIdBlock()
		{
			NetIdRegistry.ReleaseTransform(_netId + 1, transform);
			NetIdRegistry.Release(_netId);
			_netId.Clear();

			// Sorted alphabetically. If adding or removing entries please make sure to adjust block size.
			_animator.ReleaseNetId();
			_clothing.ReleaseNetId();
			_crafting.ReleaseNetId();
			_equipment.ReleaseNetId();
			_input.ReleaseNetId();
			_interact.ReleaseNetId();
			_inventory.ReleaseNetId();
			_life.ReleaseNetId();
			_look.ReleaseNetId();
			_movement.ReleaseNetId();
			_quests.ReleaseNetId();
			_skills.ReleaseNetId();
			_stance.ReleaseNetId();
			_voice.ReleaseNetId();
		}

		private void Awake()
		{
			_channel = GetComponent<SteamChannel>();

			agro = 0;

			_animator = GetComponent<PlayerAnimator>();
			_clothing = GetComponent<PlayerClothing>();
			_inventory = GetComponent<PlayerInventory>();
			_equipment = GetComponent<PlayerEquipment>();
			_life = GetComponent<PlayerLife>();
			_crafting = GetComponent<PlayerCrafting>();
			_skills = GetComponent<PlayerSkills>();
			_movement = GetComponent<PlayerMovement>();
			_look = GetComponent<PlayerLook>();
			_stance = GetComponent<PlayerStance>();
			_input = GetComponent<PlayerInput>();
			_voice = GetComponent<PlayerVoice>();
			_interact = GetComponent<PlayerInteract>();
			_workzone = GetComponent<PlayerWorkzone>();
			_quests = GetComponent<PlayerQuests>();
		}

		/// <summary>
		/// Nelson 2024-11-11: Added to help narrow down if player is destroyed outside of Provider.removePlayer.
		/// (public issue #4760)
		/// </summary>
		internal bool isExpectingDestroy;

		private void OnDestroy()
		{
			// Nelson 2025-03-05: hosts reported this error happening when shutting down with players online. My bad!
			// That case doesn't go through the full player removal, rather, it notifies them the server is closing.
			if (!isExpectingDestroy && !Provider.isApplicationQuitting && Dedicator.IsDedicatedServer)
			{
				UnturnedLog.error("FATAL ERROR! Player game object destroyed outside of Provider.removePlayer!");
				if (channel != null)
				{
					if (channel.owner != null)
					{
						UnturnedLog.error("Logging destroyed player info to assist with debugging");
						UnturnedLog.error("e.g., to correlate with other recent log lines");
						UnturnedLog.error("(it's likely *NOT* the player's fault)");

						bool hasAnyInfo = false;

						if (!ReferenceEquals(channel.owner.playerID, null))
						{
							UnturnedLog.error($"Destroyed player ID: {channel.owner.playerID}");
							hasAnyInfo = true;
						}

						if (!ReferenceEquals(channel.owner.transportConnection, null))
						{
							UnturnedLog.error($"Destroyed player connection: {channel.owner.transportConnection}");
							hasAnyInfo = true;
						}

						if (!hasAnyInfo)
						{
							UnturnedLog.error("Unable to log destroyed player info because player ID and connection are null");
						}
					}
					else
					{
						UnturnedLog.info("Unable to log destroyed player info because channel's owner is null");
					}
				}
				else
				{
					UnturnedLog.info("Unable to log destroyed player info because channel component is null");
				}
			}

			if (screenshotFinal != null)
			{
				DestroyImmediate(screenshotFinal);
				screenshotFinal = null;
			}

			if (channel != null && channel.IsLocalPlayer)
			{
				isLoadingInventory = false;
				isLoadingLife = false;
				isLoadingClothing = false;

				channel.owner.commitModifiedDynamicProps();
			}

			try
			{
				onPlayerDestroyed?.Invoke(this);
			}
			catch (System.Exception e)
			{
				UnturnedLog.exception(e, "Caught exception in onPlayerDestroyed:");
			}
		}

		public void save()
		{
			savePositionAndRotation();
			clothing.save();
			inventory.save();
			life.save();
			skills.save();
			animator.save();
			quests.save();
		}

		protected void savePositionAndRotation()
		{
			bool shouldSave = life.IsAlive;
			Vector3 savePosition = transform.position;
			byte saveRotation = MeasurementTool.angleToByte(transform.rotation.eulerAngles.y);
			if (shouldSave)
			{
				InteractableVehicle vehicle = movement.getVehicle();
				if (vehicle != null)
				{
					byte seatIndex;
					if (vehicle.findPlayerSeat(this, out seatIndex))
					{
						Vector3 exitPosition;
						byte exitAngle;
						if (vehicle.tryGetExit(seatIndex, out exitPosition, out exitAngle))
						{
							savePosition = exitPosition;
							saveRotation = exitAngle;
						}
					}
				}

				shouldSave = savePosition.IsFinite();
			}

			if (shouldSave)
			{
				Block block = new Block();
				block.writeByte(SAVEDATA_VERSION);
				block.writeSingleVector3(savePosition);
				block.writeByte(saveRotation);

				PlayerSavedata.writeBlock(channel.owner.playerID, "/Player/Player.dat", block);
			}
			else
			{
				if (PlayerSavedata.fileExists(channel.owner.playerID, "/Player/Player.dat"))
				{
					PlayerSavedata.deleteFile(channel.owner.playerID, "/Player/Player.dat");
				}
			}
		}

		private NetId _netId;
		public NetId GetNetId()
		{
			return _netId;
		}

		#region IDialogueTarget
		public Vector3 GetDialogueTargetWorldPosition()
		{
			return transform.position;
		}

		public NetId GetDialogueTargetNetId()
		{
			return _netId + 1;
		}

		public bool ShouldServerApproveDialogueRequest(Player withPlayer)
		{
			// Nelson 2024-10-01: False to prevent cheat clients from requesting dialogue.
			// Plugins can still use ApproveTalkWithNpcRequest.
			return false;
		}

		public DialogueAsset FindStartingDialogueAsset()
		{
			// Nelson 2024-10-01: Null to prevent cheat clients from requesting dialogue.
			// Plugins can still use ApproveTalkWithNpcRequest.
			return null;
		}

		public string GetDialogueTargetDebugName()
		{
			return channel?.owner?.playerID?.ToString() ?? "invalid player";
		}

		public string GetDialogueTargetNameShownToPlayer(Player player)
		{
			if (Dedicator.IsDedicatedServer)
			{
				return GetDialogueTargetDebugName();
			}
			else
			{
				return channel?.owner?.GetLocalDisplayName() ?? "invalid player";
			}
		}

		public void SetFaceOverride(byte? faceOverride)
		{
			// N/A
		}

		public void SetIsTalkingWithLocalPlayer(bool isTalkingWithLocalPlayer)
		{
			// N/A
		}
		#endregion IDialogueTarget

		#region IExplosionDamageable
		public bool Equals(IExplosionDamageable obj)
		{
			return ReferenceEquals(this, obj);
		}

		public bool IsEligibleForExplosionDamage
		{
			get => life.IsAlive;
		}

		public Vector3 GetClosestPointToExplosion(Vector3 explosionCenter)
		{
			return CollisionUtil.ClosestPoint(gameObject, explosionCenter, false, DamageTool.EXPLOSION_CLOSEST_POINT_LAYER_MASK);
		}

		public void ApplyExplosionDamage(in ExplosionParameters explosionParameters, ref ExplosionDamageParameters damageParameters)
		{
			if (!damageParameters.shouldAffectPlayers)
			{
				return;
			}

			if (explosionParameters.damageType == EExplosionDamageType.ZOMBIE_FIRE)
			{
				if (clothing.shirtAsset != null && clothing.shirtAsset.proofFire && clothing.pantsAsset != null && clothing.pantsAsset.proofFire)
				{
					return;
				}
			}

			Vector3 offset = damageParameters.closestPoint - explosionParameters.point;
			float range = offset.magnitude;
			if (range > explosionParameters.damageRadius)
			{
				return;
			}

			Vector3 normal = offset / range;
			if (damageParameters.LineOfSightTest(explosionParameters.point, normal, range, out RaycastHit block))
			{
				if (block.transform != null && !block.transform.IsChildOf(transform))
				{
					return;
				}
			}

			if (damageParameters.canDealPlayerDamage)
			{
				if (explosionParameters.playImpactEffect)
				{
					EffectAsset fleshEffect = DamageTool.FleshDynamicRef.Find();
					if (fleshEffect != null)
					{
						TriggerEffectParameters triggerEffectParameters = new TriggerEffectParameters(fleshEffect);
						triggerEffectParameters.relevantDistance = EffectManager.SMALL;
						triggerEffectParameters.position = transform.position + Vector3.up;
						triggerEffectParameters.reliable = true;
						EffectManager.triggerEffect(triggerEffectParameters);

						// Spawn a second time pointing towards the damage.
						triggerEffectParameters.SetDirection(-normal);
						EffectManager.triggerEffect(triggerEffectParameters);
					}
				}

				float times = 1.0f - MathfEx.Square(range / explosionParameters.damageRadius);
				if (movement.getVehicle() != null && movement.getVehicle().asset != null)
				{
					times *= movement.getVehicle().asset.passengerExplosionArmor;
				}

				float armorMultiplier = DamageTool.getPlayerExplosionArmor(this);
				times *= armorMultiplier;

				DamageTool.damage(this, explosionParameters.cause, ELimb.SPINE, explosionParameters.killer, normal,
					explosionParameters.playerDamage, times, out EPlayerKill kill, trackKill: true, ragdollEffect: explosionParameters.ragdollEffect);

				// 2023-01-25: don't track blowing yourself up as a kill stat. Assuming EPlayerKill is only
				// used for tracking stats. (public issue #2692)
				if (kill != EPlayerKill.NONE && channel.owner.playerID.steamID != explosionParameters.killer)
				{
					damageParameters.kills.Add(kill);
				}
			}

			if (explosionParameters.launchSpeed > 0.01f)
			{
				Vector3 launchDirection = (transform.position + Vector3.up - explosionParameters.point).normalized;
				float multiplier = 1.0f - MathfEx.Square(range / explosionParameters.damageRadius);
				multiplier *= Provider.modeConfigData.Gameplay.Explosion_Launch_Speed_Multiplier;
				movement.pendingLaunchVelocity += launchDirection * explosionParameters.launchSpeed * multiplier;
			}
		}
		#endregion IExplosionDamageable
	}
}
