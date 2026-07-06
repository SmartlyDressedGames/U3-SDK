////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerDeathUI
	{
		private static SleekFullscreenBox container;
		public static Local localization;
		public static bool active;

		private static ISleekBox causeBox;
		public static SleekButtonIcon homeButton;
		public static SleekButtonIcon respawnButton;

		/// <summary>
		/// Has the contained been animated into visibility on-screen?
		/// Used to disable animating out if disabled.
		/// </summary>
		private static bool containerOnScreen;

#if !DEDICATED_SERVER
		private static OneShotAudioHandle deathMusicHandle;
#endif

		public static void open(bool fromDeath)
		{
			if (active)
			{
				return;
			}

			active = true;

			synchronizeDeathCause();

			if (fromDeath && PlayerLife.deathCause != EDeathCause.SUICIDE)
			{
				if (OptionsSettings.deathMusicVolume > 0.0f)
				{
					// Only play death music is singleplayer
					// Lots of servers use custom FX after death, and when playing multiplayer it can get tiring if still enabled
					if (Provider.isServer)
					{
#if !DEDICATED_SERVER
						LevelAsset levelAsset = Level.getAsset();
						MasterBundleReference<AudioClip> deathMusicRef = levelAsset != null ? levelAsset.DeathMusicRef
								: LevelAsset.DefaultDeathMusicRef;
						// Can be null if level chose to turn off death music.
						if (deathMusicRef.isValid)
						{
							AudioClip deathMusic = deathMusicRef.loadAsset();
							if (deathMusic != null)
							{
								OneShotAudioParameters parameters = new OneShotAudioParameters(deathMusic); // defaults to 2D
								parameters.outputAudioMixerGroup = UnturnedAudioMixer.GetMusicGroup();
								parameters.volume = OptionsSettings.deathMusicVolume;
								deathMusicHandle = parameters.Play();
							}
							else
							{
								UnturnedLog.warn($"Unable to find death music \"{deathMusicRef}\"");
							}
						}
#endif // !DEDICATED_SERVER
					}
				}
			}

			if (Player.LocalPlayer.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowDeathMenu))
			{
				if (containerOnScreen == false)
				{
					containerOnScreen = true;
					container.AnimateIntoView();
				}
			}
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

#if !DEDICATED_SERVER
			deathMusicHandle.Stop();
#endif

			if (containerOnScreen)
			{
				containerOnScreen = false;
				container.AnimateOutOfView(0, 1);
			}
		}

		private static void synchronizeDeathCause()
		{
			if (PlayerLife.deathCause == EDeathCause.BLEEDING)
			{
				causeBox.Text = localization.format("Bleeding");
			}
			else if (PlayerLife.deathCause == EDeathCause.BONES)
			{
				causeBox.Text = localization.format("Bones");
			}
			else if (PlayerLife.deathCause == EDeathCause.FREEZING)
			{
				causeBox.Text = localization.format("Freezing");
			}
			else if (PlayerLife.deathCause == EDeathCause.BURNING)
			{
				causeBox.Text = localization.format("Burning");
			}
			else if (PlayerLife.deathCause == EDeathCause.FOOD)
			{
				causeBox.Text = localization.format("Food");
			}
			else if (PlayerLife.deathCause == EDeathCause.WATER)
			{
				causeBox.Text = localization.format("Water");
			}
			else if (PlayerLife.deathCause == EDeathCause.GUN || PlayerLife.deathCause == EDeathCause.MELEE || PlayerLife.deathCause == EDeathCause.PUNCH || PlayerLife.deathCause == EDeathCause.ROADKILL || PlayerLife.deathCause == EDeathCause.GRENADE || PlayerLife.deathCause == EDeathCause.MISSILE || PlayerLife.deathCause == EDeathCause.CHARGE || PlayerLife.deathCause == EDeathCause.SPLASH)
			{
				SteamPlayer player = PlayerTool.getSteamPlayer(PlayerLife.deathKiller);

				string characterName;
				string playerName;

				if (player != null)
				{
					characterName = player.playerID.characterName;
					playerName = player.playerID.playerName;
				}
				else
				{
					characterName = "?";
					playerName = "?";
				}

				string limb = "";
				if (PlayerLife.deathLimb == ELimb.LEFT_FOOT || PlayerLife.deathLimb == ELimb.LEFT_LEG || PlayerLife.deathLimb == ELimb.RIGHT_FOOT || PlayerLife.deathLimb == ELimb.RIGHT_LEG)
				{
					limb = localization.format("Leg");
				}
				else if (PlayerLife.deathLimb == ELimb.LEFT_HAND || PlayerLife.deathLimb == ELimb.LEFT_ARM || PlayerLife.deathLimb == ELimb.RIGHT_HAND || PlayerLife.deathLimb == ELimb.RIGHT_ARM)
				{
					limb = localization.format("Arm");
				}
				else if (PlayerLife.deathLimb == ELimb.SPINE)
				{
					limb = localization.format("Spine");
				}
				else if (PlayerLife.deathLimb == ELimb.SKULL)
				{
					limb = localization.format("Skull");
				}

				if (PlayerLife.deathCause == EDeathCause.GUN)
				{
					causeBox.Text = localization.format("Gun", limb, characterName, playerName);
				}
				else if (PlayerLife.deathCause == EDeathCause.MELEE)
				{
					causeBox.Text = localization.format("Melee", limb, characterName, playerName);
				}
				else if (PlayerLife.deathCause == EDeathCause.PUNCH)
				{
					causeBox.Text = localization.format("Punch", limb, characterName, playerName);
				}
				else if (PlayerLife.deathCause == EDeathCause.ROADKILL)
				{
					causeBox.Text = localization.format("Roadkill", characterName, playerName);
				}
				else if (PlayerLife.deathCause == EDeathCause.GRENADE)
				{
					causeBox.Text = localization.format("Grenade", characterName, playerName);
				}
				else if (PlayerLife.deathCause == EDeathCause.MISSILE)
				{
					causeBox.Text = localization.format("Missile", characterName, playerName);
				}
				else if (PlayerLife.deathCause == EDeathCause.CHARGE)
				{
					causeBox.Text = localization.format("Charge", characterName, playerName);
				}
				else if (PlayerLife.deathCause == EDeathCause.SPLASH)
				{
					causeBox.Text = localization.format("Splash", characterName, playerName);
				}
			}
			else if (PlayerLife.deathCause == EDeathCause.ZOMBIE)
			{
				causeBox.Text = localization.format("Zombie");
			}
			else if (PlayerLife.deathCause == EDeathCause.ANIMAL)
			{
				causeBox.Text = localization.format("Animal");
			}
			else if (PlayerLife.deathCause == EDeathCause.SUICIDE)
			{
				causeBox.Text = localization.format("Suicide");
			}
			else if (PlayerLife.deathCause == EDeathCause.KILL)
			{
				causeBox.Text = localization.format("Kill");
			}
			else if (PlayerLife.deathCause == EDeathCause.INFECTION)
			{
				causeBox.Text = localization.format("Infection");
			}
			else if (PlayerLife.deathCause == EDeathCause.BREATH)
			{
				causeBox.Text = localization.format("Breath");
			}
			else if (PlayerLife.deathCause == EDeathCause.ZOMBIE)
			{
				causeBox.Text = localization.format("Zombie");
			}
			else if (PlayerLife.deathCause == EDeathCause.VEHICLE)
			{
				causeBox.Text = localization.format("Vehicle");
			}
			else if (PlayerLife.deathCause == EDeathCause.SHRED)
			{
				causeBox.Text = localization.format("Shred");
			}
			else if (PlayerLife.deathCause == EDeathCause.LANDMINE)
			{
				causeBox.Text = localization.format("Landmine");
			}
			else if (PlayerLife.deathCause == EDeathCause.ARENA)
			{
				causeBox.Text = localization.format("Arena");
			}
			else if (PlayerLife.deathCause == EDeathCause.SENTRY)
			{
				causeBox.Text = localization.format("Sentry");
			}
			else if (PlayerLife.deathCause == EDeathCause.ACID)
			{
				causeBox.Text = localization.format("Acid");
			}
			else if (PlayerLife.deathCause == EDeathCause.BOULDER)
			{
				causeBox.Text = localization.format("Boulder");
			}
			else if (PlayerLife.deathCause == EDeathCause.BURNER)
			{
				causeBox.Text = localization.format("Burner");
			}
			else if (PlayerLife.deathCause == EDeathCause.SPIT)
			{
				causeBox.Text = localization.format("Spit");
			}
			else if (PlayerLife.deathCause == EDeathCause.SPARK)
			{
				causeBox.Text = localization.format("Spark");
			}
		}

		private static void onClickedHomeButton(ISleekElement button)
		{
			if (!Provider.isServer && Provider.isPvP)
			{
				if (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastDeath < Provider.modeConfigData.Gameplay.Timer_Home)
				{
					return;
				}
			}
			else
			{
				if (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastRespawn < Provider.modeConfigData.Gameplay.Timer_Respawn)
				{
					return;
				}
			}

			Player.LocalPlayer.life.sendRespawn(true);
		}

		private static void onClickedRespawnButton(ISleekElement button)
		{
			if (Time.realtimeSinceStartup - Player.LocalPlayer.life.lastRespawn < Provider.modeConfigData.Gameplay.Timer_Respawn)
			{
				return;
			}

			Player.LocalPlayer.life.sendRespawn(false);
		}

		public PlayerDeathUI()
		{
			localization = Localization.read("/Player/PlayerDeath.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Player/Icons/PlayerDeath");

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
			containerOnScreen = false;

			causeBox = Glazier.Get().CreateBox();
			causeBox.PositionOffset_Y = -25;
			causeBox.PositionScale_Y = 0.8f;
			causeBox.SizeOffset_Y = 50;
			causeBox.SizeScale_X = 1f;
			container.AddChild(causeBox);

			homeButton = new SleekButtonIcon(icons.load<Texture2D>("Home"));
			homeButton.PositionOffset_X = -205;
			homeButton.PositionOffset_Y = 35;
			homeButton.PositionScale_X = 0.5f;
			homeButton.PositionScale_Y = 0.8f;
			homeButton.SizeOffset_X = 200;
			homeButton.SizeOffset_Y = 30;
			homeButton.text = localization.format("Home_Button");
			homeButton.tooltip = localization.format("Home_Button_Tooltip");
			homeButton.iconColor = ESleekTint.FOREGROUND;
			homeButton.onClickedButton += onClickedHomeButton;
			container.AddChild(homeButton);

			respawnButton = new SleekButtonIcon(icons.load<Texture2D>("Respawn"));
			respawnButton.PositionOffset_X = 5;
			respawnButton.PositionOffset_Y = 35;
			respawnButton.PositionScale_X = 0.5f;
			respawnButton.PositionScale_Y = 0.8f;
			respawnButton.SizeOffset_X = 200;
			respawnButton.SizeOffset_Y = 30;
			respawnButton.text = localization.format("Respawn_Button");
			respawnButton.tooltip = localization.format("Respawn_Button_Tooltip");
			respawnButton.iconColor = ESleekTint.FOREGROUND;
			respawnButton.onClickedButton += onClickedRespawnButton;
			container.AddChild(respawnButton);
		}
	}
}
