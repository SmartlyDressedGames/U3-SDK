////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuConfigurationAudioUI : SleekFullscreenBox
	{
		public Local localization;
		public bool active;

		private SleekButtonIcon backButton;
		private ISleekButton defaultButton;

		private ISleekScrollView audioBox;

		private ISleekSlider masterVolumeSlider;
		private ISleekSlider unfocusedVolumeSlider;
		private ISleekSlider musicMasterVolumeSlider;
		private ISleekSlider loadingScreenMusicVolumeSlider;
		private ISleekSlider deathMusicVolumeSlider;
		private ISleekSlider mainMenuMusicVolumeSlider;
		private ISleekSlider ambientMusicVolumeSlider;
		private ISleekSlider gameVolumeSlider;
		private ISleekSlider voiceVolumeSlider;
		private ISleekSlider atmosphereVolumeSlider;
		private ISleekSlider zombieFootstepsVolumeSlider;

		public void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			AnimateIntoView();
		}

		public void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			OptionsSettings.save();

			AnimateOutOfView(0, 1);
		}

		private void OnVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.volume = state;
			OptionsSettings.apply();

			masterVolumeSlider.UpdateLabel(localization.format("Volume_Slider_Label", OptionsSettings.volume.ToString("P0")));
		}

		private void OnVoiceVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.voiceVolume = state;
			voiceVolumeSlider.UpdateLabel(localization.format("Voice_Slider_Label", OptionsSettings.voiceVolume.ToString("P0")));
		}

		private void OnAtmosphereVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.AtmosphereVolume = state;
			atmosphereVolumeSlider.UpdateLabel(localization.format("Atmosphere_Volume_Slider_Label", OptionsSettings.AtmosphereVolume.ToString("P0")));
		}

		private void OnZombieFootstepsVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.ZombieFootstepsVolume = state;
			zombieFootstepsVolumeSlider.UpdateLabel(localization.format("Zombie_Footsteps_Volume_Slider_Label", OptionsSettings.ZombieFootstepsVolume.ToString("P0")));
		}

		private void OnGameVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.gameVolume = state;
			gameVolumeSlider.UpdateLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")));
		}

		private void OnUnfocusedVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.UnfocusedVolume = state;
			unfocusedVolumeSlider.UpdateLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.UnfocusedVolume.ToString("P0")));
		}

		private void OnMusicMasterVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.MusicMasterVolume = state;
			musicMasterVolumeSlider.UpdateLabel(localization.format("Music_Master_Volume_Slider_Label", OptionsSettings.MusicMasterVolume.ToString("P0")));
		}

		private void OnLoadingScreenMusicVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.loadingScreenMusicVolume = state;
			loadingScreenMusicVolumeSlider.UpdateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")));
		}

		private void OnDeathMusicVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.deathMusicVolume = state;
			deathMusicVolumeSlider.UpdateLabel(localization.format("Death_Music_Volume_Slider_Label", OptionsSettings.deathMusicVolume.ToString("P0")));
		}

		private void OnMainMenuMusicVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.MainMenuMusicVolume = state;
			mainMenuMusicVolumeSlider.UpdateLabel(localization.format("Main_Menu_Music_Volume_Slider_Label", OptionsSettings.MainMenuMusicVolume.ToString("P0")));
		}

		private void OnAmbientMusicVolumeSliderDragged(ISleekSlider slider, float state)
		{
			OptionsSettings.ambientMusicVolume = state;
			ambientMusicVolumeSlider.UpdateLabel(localization.format("Ambient_Music_Volume_Slider_Label", OptionsSettings.ambientMusicVolume.ToString("P0")));
		}

		private void onClickedBackButton(ISleekElement button)
		{
			if (Player.LocalPlayer != null)
			{
				PlayerPauseUI.open();
			}
			else if (Level.isEditor)
			{
				EditorPauseUI.open();
			}
			else
			{
				MenuConfigurationUI.open();
			}

			close();
		}

		private void onClickedDefaultButton(ISleekElement button)
		{
			OptionsSettings.RestoreAudioDefaults();

			updateAll();
		}

		private void updateAll()
		{
			masterVolumeSlider.Value = OptionsSettings.volume;
			masterVolumeSlider.UpdateLabel(localization.format("Volume_Slider_Label", OptionsSettings.volume.ToString("P0")));
			unfocusedVolumeSlider.Value = OptionsSettings.UnfocusedVolume;
			unfocusedVolumeSlider.UpdateLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.UnfocusedVolume.ToString("P0")));
			musicMasterVolumeSlider.Value = OptionsSettings.MusicMasterVolume;
			musicMasterVolumeSlider.UpdateLabel(localization.format("Music_Master_Volume_Slider_Label", OptionsSettings.MusicMasterVolume.ToString("P0")));
			loadingScreenMusicVolumeSlider.Value = OptionsSettings.loadingScreenMusicVolume;
			loadingScreenMusicVolumeSlider.UpdateLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")));
			deathMusicVolumeSlider.Value = OptionsSettings.deathMusicVolume;
			deathMusicVolumeSlider.UpdateLabel(localization.format("Death_Music_Volume_Slider_Label", OptionsSettings.deathMusicVolume.ToString("P0")));
			mainMenuMusicVolumeSlider.Value = OptionsSettings.MainMenuMusicVolume;
			mainMenuMusicVolumeSlider.UpdateLabel(localization.format("Main_Menu_Music_Volume_Slider_Label", OptionsSettings.MainMenuMusicVolume.ToString("P0")));
			ambientMusicVolumeSlider.Value = OptionsSettings.ambientMusicVolume;
			ambientMusicVolumeSlider.UpdateLabel(localization.format("Ambient_Music_Volume_Slider_Label", OptionsSettings.ambientMusicVolume.ToString("P0")));
			voiceVolumeSlider.Value = OptionsSettings.voiceVolume;
			voiceVolumeSlider.UpdateLabel(localization.format("Voice_Slider_Label", OptionsSettings.voiceVolume.ToString("P0")));
			gameVolumeSlider.Value = OptionsSettings.gameVolume;
			gameVolumeSlider.UpdateLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")));
			atmosphereVolumeSlider.Value = OptionsSettings.AtmosphereVolume;
			atmosphereVolumeSlider.UpdateLabel(localization.format("Atmosphere_Volume_Slider_Label", OptionsSettings.AtmosphereVolume.ToString("P0")));
			zombieFootstepsVolumeSlider.Value = OptionsSettings.ZombieFootstepsVolume;
			zombieFootstepsVolumeSlider.UpdateLabel(localization.format("Zombie_Footsteps_Volume_Slider_Label", OptionsSettings.ZombieFootstepsVolume.ToString("P0")));
		}

		public MenuConfigurationAudioUI()
		{
			localization = Localization.read("/Menu/Configuration/MenuConfigurationAudio.dat");

			Color32 tooltipHeaderColor = new Color32(240, 240, 240, byte.MaxValue);
			Color32 tooltipBodyColor = new Color32(180, 180, 180, byte.MaxValue);

			active = false;

			audioBox = Glazier.Get().CreateScrollView();
			audioBox.PositionOffset_X = -200;
			audioBox.PositionOffset_Y = 100;
			audioBox.PositionScale_X = 0.5f;
			audioBox.SizeOffset_X = 430;
			audioBox.SizeOffset_Y = -200;
			audioBox.SizeScale_Y = 1;
			audioBox.ScaleContentToWidth = true;
			AddChild(audioBox);

			int verticalOffset = 0;

			masterVolumeSlider = Glazier.Get().CreateSlider();
			masterVolumeSlider.PositionOffset_Y = verticalOffset;
			masterVolumeSlider.SizeOffset_X = 200;
			masterVolumeSlider.SizeOffset_Y = 20;
			masterVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			masterVolumeSlider.AddLabel(localization.format("Volume_Slider_Label", OptionsSettings.volume.ToString("P0")), ESleekSide.RIGHT);
			masterVolumeSlider.OnValueChanged += OnVolumeSliderDragged;
			audioBox.AddChild(masterVolumeSlider);
			verticalOffset += 30;

			gameVolumeSlider = Glazier.Get().CreateSlider();
			gameVolumeSlider.PositionOffset_Y = verticalOffset;
			gameVolumeSlider.SizeOffset_X = 200;
			gameVolumeSlider.SizeOffset_Y = 20;
			gameVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			gameVolumeSlider.AddLabel(localization.format("Game_Volume_Slider_Label", OptionsSettings.gameVolume.ToString("P0")), ESleekSide.RIGHT);
			gameVolumeSlider.OnValueChanged += OnGameVolumeSliderDragged;
			audioBox.AddChild(gameVolumeSlider);
			verticalOffset += 30;

			unfocusedVolumeSlider = Glazier.Get().CreateSlider();
			unfocusedVolumeSlider.PositionOffset_Y = verticalOffset;
			unfocusedVolumeSlider.SizeOffset_X = 200;
			unfocusedVolumeSlider.SizeOffset_Y = 20;
			unfocusedVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			unfocusedVolumeSlider.AddLabel(localization.format("Unfocused_Volume_Slider_Label", OptionsSettings.UnfocusedVolume.ToString("P0")), ESleekSide.RIGHT);
			unfocusedVolumeSlider.OnValueChanged += OnUnfocusedVolumeSliderDragged;
			audioBox.AddChild(unfocusedVolumeSlider);
			verticalOffset += 30;

			voiceVolumeSlider = Glazier.Get().CreateSlider();
			voiceVolumeSlider.PositionOffset_Y = verticalOffset;
			voiceVolumeSlider.SizeOffset_X = 200;
			voiceVolumeSlider.SizeOffset_Y = 20;
			voiceVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			voiceVolumeSlider.AddLabel(localization.format("Voice_Slider_Label", OptionsSettings.voiceVolume.ToString("P0")), ESleekSide.RIGHT);
			voiceVolumeSlider.OnValueChanged += OnVoiceVolumeSliderDragged;
			audioBox.AddChild(voiceVolumeSlider);
			verticalOffset += 30;

			atmosphereVolumeSlider = Glazier.Get().CreateSlider();
			atmosphereVolumeSlider.PositionOffset_Y = verticalOffset;
			atmosphereVolumeSlider.SizeOffset_X = 200;
			atmosphereVolumeSlider.SizeOffset_Y = 20;
			atmosphereVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			atmosphereVolumeSlider.AddLabel(localization.format("Atmosphere_Volume_Slider_Label", OptionsSettings.AtmosphereVolume.ToString("P0")), ESleekSide.RIGHT);
			atmosphereVolumeSlider.OnValueChanged += OnAtmosphereVolumeSliderDragged;
			audioBox.AddChild(atmosphereVolumeSlider);
			verticalOffset += 30;

			zombieFootstepsVolumeSlider = Glazier.Get().CreateSlider();
			zombieFootstepsVolumeSlider.PositionOffset_Y = verticalOffset;
			zombieFootstepsVolumeSlider.SizeOffset_X = 200;
			zombieFootstepsVolumeSlider.SizeOffset_Y = 20;
			zombieFootstepsVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			zombieFootstepsVolumeSlider.AddLabel(localization.format("Zombie_Footsteps_Volume_Slider_Label", OptionsSettings.ZombieFootstepsVolume.ToString("P0")), ESleekSide.RIGHT);
			zombieFootstepsVolumeSlider.OnValueChanged += OnZombieFootstepsVolumeSliderDragged;
			audioBox.AddChild(zombieFootstepsVolumeSlider);
			verticalOffset += 30;

			ISleekBox musicHeader = Glazier.Get().CreateBox();
			musicHeader.PositionOffset_Y = verticalOffset;
			musicHeader.SizeOffset_X = 400;
			musicHeader.SizeOffset_Y = 30;
			musicHeader.Text = localization.format("Music_Header");
			audioBox.AddChild(musicHeader);
			verticalOffset += 40;

			musicMasterVolumeSlider = Glazier.Get().CreateSlider();
			musicMasterVolumeSlider.PositionOffset_Y = verticalOffset;
			musicMasterVolumeSlider.SizeOffset_X = 200;
			musicMasterVolumeSlider.SizeOffset_Y = 20;
			musicMasterVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			musicMasterVolumeSlider.AddLabel(localization.format("Music_Master_Volume_Slider_Label", OptionsSettings.MusicMasterVolume.ToString("P0")), ESleekSide.RIGHT);
			musicMasterVolumeSlider.OnValueChanged += OnMusicMasterVolumeSliderDragged;
			audioBox.AddChild(musicMasterVolumeSlider);
			verticalOffset += 30;

			loadingScreenMusicVolumeSlider = Glazier.Get().CreateSlider();
			loadingScreenMusicVolumeSlider.PositionOffset_Y = verticalOffset;
			loadingScreenMusicVolumeSlider.SizeOffset_X = 200;
			loadingScreenMusicVolumeSlider.SizeOffset_Y = 20;
			loadingScreenMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			loadingScreenMusicVolumeSlider.AddLabel(localization.format("Loading_Screen_Music_Volume_Slider_Label", OptionsSettings.loadingScreenMusicVolume.ToString("P0")), ESleekSide.RIGHT);
			loadingScreenMusicVolumeSlider.OnValueChanged += OnLoadingScreenMusicVolumeSliderDragged;
			audioBox.AddChild(loadingScreenMusicVolumeSlider);
			verticalOffset += 30;

			deathMusicVolumeSlider = Glazier.Get().CreateSlider();
			deathMusicVolumeSlider.PositionOffset_Y = verticalOffset;
			deathMusicVolumeSlider.SizeOffset_X = 200;
			deathMusicVolumeSlider.SizeOffset_Y = 20;
			deathMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			deathMusicVolumeSlider.AddLabel(localization.format("Death_Music_Volume_Slider_Label", OptionsSettings.deathMusicVolume.ToString("P0")), ESleekSide.RIGHT);
			deathMusicVolumeSlider.OnValueChanged += OnDeathMusicVolumeSliderDragged;
			audioBox.AddChild(deathMusicVolumeSlider);
			verticalOffset += 30;

			mainMenuMusicVolumeSlider = Glazier.Get().CreateSlider();
			mainMenuMusicVolumeSlider.PositionOffset_Y = verticalOffset;
			mainMenuMusicVolumeSlider.SizeOffset_X = 200;
			mainMenuMusicVolumeSlider.SizeOffset_Y = 20;
			mainMenuMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			mainMenuMusicVolumeSlider.AddLabel(localization.format("Main_Menu_Music_Volume_Slider_Label", OptionsSettings.MainMenuMusicVolume.ToString("P0")), ESleekSide.RIGHT);
			mainMenuMusicVolumeSlider.OnValueChanged += OnMainMenuMusicVolumeSliderDragged;
			audioBox.AddChild(mainMenuMusicVolumeSlider);
			verticalOffset += 30;

			ambientMusicVolumeSlider = Glazier.Get().CreateSlider();
			ambientMusicVolumeSlider.PositionOffset_Y = verticalOffset;
			ambientMusicVolumeSlider.SizeOffset_X = 200;
			ambientMusicVolumeSlider.SizeOffset_Y = 20;
			ambientMusicVolumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			ambientMusicVolumeSlider.AddLabel(localization.format("Ambient_Music_Volume_Slider_Label", OptionsSettings.ambientMusicVolume.ToString("P0")), ESleekSide.RIGHT);
			ambientMusicVolumeSlider.OnValueChanged += OnAmbientMusicVolumeSliderDragged;
			audioBox.AddChild(ambientMusicVolumeSlider);
			verticalOffset += 30;

			audioBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10);

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
			AddChild(backButton);

			defaultButton = Glazier.Get().CreateButton();
			defaultButton.PositionOffset_X = -200;
			defaultButton.PositionOffset_Y = -50;
			defaultButton.PositionScale_X = 1f;
			defaultButton.PositionScale_Y = 1f;
			defaultButton.SizeOffset_X = 200;
			defaultButton.SizeOffset_Y = 50;
			defaultButton.Text = MenuPlayConfigUI.localization.format("Default");
			defaultButton.TooltipText = MenuPlayConfigUI.localization.format("Default_Tooltip");
			defaultButton.OnClicked += onClickedDefaultButton;
			defaultButton.FontSize = ESleekFontSize.Medium;
			AddChild(defaultButton);

			updateAll();
		}
	}
}
