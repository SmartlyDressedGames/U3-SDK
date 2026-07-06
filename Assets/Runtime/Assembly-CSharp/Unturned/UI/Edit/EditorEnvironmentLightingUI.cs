////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorEnvironmentLightingUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekScrollView lightingScrollBox;

		private static ISleekSlider azimuthSlider;
		private static ISleekSlider biasSlider;
		private static ISleekSlider fadeSlider;

		private static ISleekButton[] timeButtons;
		private static ISleekBox[] infoBoxes;
		private static SleekColorPicker[] colorPickers;
		private static ISleekSlider[] singleSliders;

		private static ELightingTime selectedTime;

		//private static SleekBox skyDawnHeaderBox;
		//private static SleekBox skyMiddayHeaderBox;
		//private static SleekBox skyDuskHeaderBox;
		//private static SleekBox skyMidnightHeaderBox;

		//private static SleekColorPicker skyDawnColorPicker;
		//private static SleekColorPicker skyMiddayColorPicker;
		//private static SleekColorPicker skyDuskColorPicker;
		//private static SleekColorPicker skyMidnightColorPicker;

		//private static SleekBox ambientDawnHeaderBox;
		//private static SleekBox ambientMiddayHeaderBox;
		//private static SleekBox ambientDuskHeaderBox;
		//private static SleekBox ambientMidnightHeaderBox;

		//private static SleekColorPicker ambientDawnColorPicker;
		//private static SleekColorPicker ambientMiddayColorPicker;
		//private static SleekColorPicker ambientDuskColorPicker;
		//private static SleekColorPicker ambientMidnightColorPicker;

		//private static SleekBox ambientDawnFogHeaderBox;
		//private static SleekBox ambientMiddayFogHeaderBox;
		//private static SleekBox ambientDuskFogHeaderBox;
		//private static SleekBox ambientMidnightFogHeaderBox;

		//private static SleekColorPicker ambientDawnFogColorPicker;
		//private static SleekColorPicker ambientMiddayFogColorPicker;
		//private static SleekColorPicker ambientDuskFogColorPicker;
		//private static SleekColorPicker ambientMidnightFogColorPicker;

		//private static SleekBox sunDawnHeaderBox;
		//private static SleekBox sunMiddayHeaderBox;
		//private static SleekBox sunDuskHeaderBox;
		//private static SleekBox sunMidnightHeaderBox;

		//private static SleekColorPicker sunDawnColorPicker;
		//private static SleekColorPicker sunMiddayColorPicker;
		//private static SleekColorPicker sunDuskColorPicker;
		//private static SleekColorPicker sunMidnightColorPicker;

		//private static SleekBox seaDawnHeaderBox;
		//private static SleekBox seaMiddayHeaderBox;
		//private static SleekBox seaDuskHeaderBox;
		//private static SleekBox seaMidnightHeaderBox;

		//private static SleekColorPicker seaDawnColorPicker;
		//private static SleekColorPicker seaMiddayColorPicker;
		//private static SleekColorPicker seaDuskColorPicker;
		//private static SleekColorPicker seaMidnightColorPicker;

		//private static ISleekSlider sunDawnIntensitySlider;
		//private static ISleekSlider sunMiddayIntensitySlider;
		//private static ISleekSlider sunDuskIntensitySlider;
		//private static ISleekSlider sunMidnightIntensitySlider;

		//private static ISleekSlider ambientDawnFogSlider;
		//private static ISleekSlider ambientMiddayFogSlider;
		//private static ISleekSlider ambientDuskFogSlider;
		//private static ISleekSlider ambientMidnightFogSlider;

		//private static ISleekSlider ambientDawnCloudsSlider;
		//private static ISleekSlider ambientMiddayCloudsSlider;
		//private static ISleekSlider ambientDuskCloudsSlider;
		//private static ISleekSlider ambientMidnightCloudsSlider;

		//private static ISleekSlider shadowDawnIntensitySlider;
		//private static ISleekSlider shadowMiddayIntensitySlider;
		//private static ISleekSlider shadowDuskIntensitySlider;
		//private static ISleekSlider shadowMidnightIntensitySlider;

		private static SleekValue seaLevelSlider;
		private static SleekValue snowLevelSlider;
		private static ISleekFloat32Field rainFreqField;
		private static ISleekFloat32Field rainDurField;
		private static ISleekFloat32Field snowFreqField;
		private static ISleekFloat32Field snowDurField;
		private static ISleekToggle rainToggle;
		private static ISleekToggle snowToggle;
		private static ISleekField weatherGuidField;
		private static ISleekButton previewWeatherButton;
		private static ISleekSlider moonSlider;
		private static ISleekSlider timeSlider;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(1, 0);
		}

		private static void onDraggedAzimuthSlider(ISleekSlider slider, float state)
		{
			LevelLighting.azimuth = state * 360;
		}

		private static void onDraggedBiasSlider(ISleekSlider slider, float state)
		{
			LevelLighting.bias = state;
			LevelLighting.MarkParticleCloudsNeedRestart();
		}

		private static void onDraggedFadeSlider(ISleekSlider slider, float state)
		{
			LevelLighting.fade = state;
			LevelLighting.MarkParticleCloudsNeedRestart();
		}

		private static void onValuedSeaLevelSlider(SleekValue slider, float state)
		{
			LevelLighting.seaLevel = state;
		}

		private static void onValuedSnowLevelSlider(SleekValue slider, float state)
		{
			LevelLighting.snowLevel = state;
		}

		private static void onToggledRainToggle(ISleekToggle toggle, bool state)
		{
			LevelLighting.canRain = state;
		}

		private static void onToggledSnowToggle(ISleekToggle toggle, bool state)
		{
			LevelLighting.canSnow = state;
		}

		private static void onTypedRainFreqField(ISleekFloat32Field field, float state)
		{
			LevelLighting.rainFreq = state;
		}

		private static void onTypedRainDurField(ISleekFloat32Field field, float state)
		{
			LevelLighting.rainDur = state;
		}

		private static void onTypedSnowFreqField(ISleekFloat32Field field, float state)
		{
			LevelLighting.snowFreq = state;
		}

		private static void onTypedSnowDurField(ISleekFloat32Field field, float state)
		{
			LevelLighting.snowDur = state;
		}

		private static void onDraggedMoonSlider(ISleekSlider slider, float state)
		{
			byte index = (byte) (state * LevelLighting.MOON_CYCLES);

			if (index >= LevelLighting.MOON_CYCLES)
			{
				index = (byte) (LevelLighting.MOON_CYCLES - 1);
			}

			LevelLighting.moon = index;
		}

		private static void onDraggedTimeSlider(ISleekSlider slider, float state)
		{
			LevelLighting.time = state;
			LevelLighting.MarkParticleCloudsNeedRestart();
		}

		private static void onClickedTimeButton(ISleekElement button)
		{
			int index;
			for (index = 0; index < timeButtons.Length; index++)
			{
				if (timeButtons[index] == button)
				{
					break;
				}
			}

			selectedTime = (ELightingTime) index;
			updateSelection();

			switch (selectedTime)
			{
				case ELightingTime.DAWN:
					LevelLighting.time = 0;
					break;
				case ELightingTime.MIDDAY:
					LevelLighting.time = LevelLighting.bias / 2f;
					break;
				case ELightingTime.DUSK:
					LevelLighting.time = LevelLighting.bias;
					break;
				case ELightingTime.MIDNIGHT:
					LevelLighting.time = 1f - ((1 - LevelLighting.bias) / 2f);
					break;
			}

			timeSlider.Value = LevelLighting.time;
			LevelLighting.MarkParticleCloudsNeedRestart();
		}

		private static void OnClickedPreviewWeather(ISleekElement button)
		{
			System.Guid weatherGuid;
			WeatherAssetBase weatherAsset;
			if (System.Guid.TryParse(weatherGuidField.Text, out weatherGuid))
			{
				weatherAsset = Assets.find(new AssetReference<WeatherAssetBase>(weatherGuid));
			}
			else
			{
				weatherAsset = null;
			}

			WeatherAssetBase activeWeatherAsset = LevelLighting.GetActiveWeatherAsset();
			if (activeWeatherAsset != null && (activeWeatherAsset == weatherAsset || activeWeatherAsset.GUID == weatherGuid))
			{
				LevelLighting.SetActiveWeatherAsset(null, 0.0f, new NetId());
			}
			else
			{
				LevelLighting.SetActiveWeatherAsset(weatherAsset, 0.0f, new NetId());
			}
		}

		private static void onPickedColorPicker(SleekColorPicker picker, Color state)
		{
			int index;
			for (index = 0; index < colorPickers.Length; index++)
			{
				if (colorPickers[index] == picker)
				{
					break;
				}
			}

			LevelLighting.times[(int) selectedTime].colors[index] = state;
			LevelLighting.updateLighting();
		}

		private static void onDraggedSingleSlider(ISleekSlider slider, float state)
		{
			int index;
			for (index = 0; index < singleSliders.Length; index++)
			{
				if (singleSliders[index] == slider)
				{
					break;
				}
			}

			LevelLighting.times[(int) selectedTime].singles[index] = state;
			LevelLighting.updateLighting();

			if (index == (int) ELightingSingle.CLOUDS)
			{
				LevelLighting.MarkParticleCloudsNeedRestart();
			}
		}

		private static void updateSelection()
		{
			for (int color = 0; color < colorPickers.Length; color++)
			{
				colorPickers[color].state = LevelLighting.times[(int) selectedTime].colors[color];
			}

			for (int single = 0; single < singleSliders.Length; single++)
			{
				singleSliders[single].Value = LevelLighting.times[(int) selectedTime].singles[single];
			}
		}

		public EditorEnvironmentLightingUI()
		{
			Local localization = Localization.read("/Editor/EditorEnvironmentLighting.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_X = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			EditorUI.window.AddChild(container);
			active = false;
			selectedTime = ELightingTime.DAWN;

			azimuthSlider = Glazier.Get().CreateSlider();
			azimuthSlider.PositionOffset_X = -230;
			azimuthSlider.PositionOffset_Y = 80;
			azimuthSlider.PositionScale_X = 1;
			azimuthSlider.SizeOffset_X = 230;
			azimuthSlider.SizeOffset_Y = 20;
			azimuthSlider.Value = LevelLighting.azimuth / 360f;
			azimuthSlider.Orientation = ESleekOrientation.HORIZONTAL;
			azimuthSlider.AddLabel(localization.format("AzimuthSliderLabelText"), ESleekSide.LEFT);
			azimuthSlider.OnValueChanged += onDraggedAzimuthSlider;
			container.AddChild(azimuthSlider);

			biasSlider = Glazier.Get().CreateSlider();
			biasSlider.PositionOffset_X = -230;
			biasSlider.PositionOffset_Y = 110;
			biasSlider.PositionScale_X = 1;
			biasSlider.SizeOffset_X = 230;
			biasSlider.SizeOffset_Y = 20;
			biasSlider.Value = LevelLighting.bias;
			biasSlider.Orientation = ESleekOrientation.HORIZONTAL;
			biasSlider.AddLabel(localization.format("BiasSliderLabelText"), ESleekSide.LEFT);
			biasSlider.OnValueChanged += onDraggedBiasSlider;
			container.AddChild(biasSlider);

			fadeSlider = Glazier.Get().CreateSlider();
			fadeSlider.PositionOffset_X = -230;
			fadeSlider.PositionOffset_Y = 140;
			fadeSlider.PositionScale_X = 1;
			fadeSlider.SizeOffset_X = 230;
			fadeSlider.SizeOffset_Y = 20;
			fadeSlider.Value = LevelLighting.fade;
			fadeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			fadeSlider.AddLabel(localization.format("FadeSliderLabelText"), ESleekSide.LEFT);
			fadeSlider.OnValueChanged += onDraggedFadeSlider;
			container.AddChild(fadeSlider);

			lightingScrollBox = Glazier.Get().CreateScrollView();
			lightingScrollBox.PositionOffset_X = -470;
			lightingScrollBox.PositionOffset_Y = 170;
			lightingScrollBox.PositionScale_X = 1;
			lightingScrollBox.SizeOffset_X = 470;
			lightingScrollBox.SizeOffset_Y = -170;
			lightingScrollBox.SizeScale_Y = 1;
			lightingScrollBox.ScaleContentToWidth = true;
			container.AddChild(lightingScrollBox);

			seaLevelSlider = new SleekValue();
			seaLevelSlider.PositionOffset_Y = -130;
			seaLevelSlider.PositionScale_Y = 1;
			seaLevelSlider.SizeOffset_X = 200;
			seaLevelSlider.SizeOffset_Y = 30;
			seaLevelSlider.state = LevelLighting.seaLevel;
			seaLevelSlider.AddLabel(localization.format("Sea_Level_Slider_Label"), ESleekSide.RIGHT);
			seaLevelSlider.onValued = onValuedSeaLevelSlider;
			container.AddChild(seaLevelSlider);

			snowLevelSlider = new SleekValue();
			snowLevelSlider.PositionOffset_Y = -90;
			snowLevelSlider.PositionScale_Y = 1;
			snowLevelSlider.SizeOffset_X = 200;
			snowLevelSlider.SizeOffset_Y = 30;
			snowLevelSlider.state = LevelLighting.snowLevel;
			snowLevelSlider.AddLabel(localization.format("Snow_Level_Slider_Label"), ESleekSide.RIGHT);
			snowLevelSlider.onValued = onValuedSnowLevelSlider;
			container.AddChild(snowLevelSlider);

			rainFreqField = Glazier.Get().CreateFloat32Field();
			rainFreqField.PositionOffset_Y = -370;
			rainFreqField.PositionScale_Y = 1;
			rainFreqField.SizeOffset_X = 100;
			rainFreqField.SizeOffset_Y = 30;
			rainFreqField.Value = LevelLighting.rainFreq;
			rainFreqField.AddLabel(localization.format("Rain_Freq_Label"), ESleekSide.RIGHT);
			rainFreqField.OnValueChanged += onTypedRainFreqField;
			container.AddChild(rainFreqField);

			rainDurField = Glazier.Get().CreateFloat32Field();
			rainDurField.PositionOffset_Y = -330;
			rainDurField.PositionScale_Y = 1;
			rainDurField.SizeOffset_X = 100;
			rainDurField.SizeOffset_Y = 30;
			rainDurField.Value = LevelLighting.rainDur;
			rainDurField.AddLabel(localization.format("Rain_Dur_Label"), ESleekSide.RIGHT);
			rainDurField.OnValueChanged += onTypedRainDurField;
			container.AddChild(rainDurField);

			snowFreqField = Glazier.Get().CreateFloat32Field();
			snowFreqField.PositionOffset_Y = -290;
			snowFreqField.PositionScale_Y = 1;
			snowFreqField.SizeOffset_X = 100;
			snowFreqField.SizeOffset_Y = 30;
			snowFreqField.Value = LevelLighting.snowFreq;
			snowFreqField.AddLabel(localization.format("Snow_Freq_Label"), ESleekSide.RIGHT);
			snowFreqField.OnValueChanged += onTypedSnowFreqField;
			container.AddChild(snowFreqField);

			snowDurField = Glazier.Get().CreateFloat32Field();
			snowDurField.PositionOffset_Y = -250;
			snowDurField.PositionScale_Y = 1;
			snowDurField.SizeOffset_X = 100;
			snowDurField.SizeOffset_Y = 30;
			snowDurField.Value = LevelLighting.snowDur;
			snowDurField.AddLabel(localization.format("Snow_Dur_Label"), ESleekSide.RIGHT);
			snowDurField.OnValueChanged += onTypedSnowDurField;
			container.AddChild(snowDurField);

			weatherGuidField = Glazier.Get().CreateStringField();
			weatherGuidField.PositionOffset_Y = -210;
			weatherGuidField.PositionScale_Y = 1.0f;
			weatherGuidField.SizeOffset_X = 200;
			weatherGuidField.SizeOffset_Y = 30;
			weatherGuidField.MaxLength = 32;
			container.AddChild(weatherGuidField);

			previewWeatherButton = Glazier.Get().CreateButton();
			previewWeatherButton.PositionOffset_X = 210;
			previewWeatherButton.PositionOffset_Y = -210;
			previewWeatherButton.PositionScale_Y = 1.0f;
			previewWeatherButton.SizeOffset_X = 200;
			previewWeatherButton.SizeOffset_Y = 30;
			previewWeatherButton.Text = localization.format("WeatherPreview_Label");
			previewWeatherButton.OnClicked += OnClickedPreviewWeather;
			container.AddChild(previewWeatherButton);

			rainToggle = Glazier.Get().CreateToggle();
			rainToggle.PositionOffset_Y = -175;
			rainToggle.PositionScale_Y = 1;
			rainToggle.SizeOffset_X = 40;
			rainToggle.SizeOffset_Y = 40;
			rainToggle.Value = LevelLighting.canRain;
			rainToggle.AddLabel(localization.format("Rain_Toggle_Label"), ESleekSide.RIGHT);
			rainToggle.OnValueChanged += onToggledRainToggle;
			container.AddChild(rainToggle);

			snowToggle = Glazier.Get().CreateToggle();
			snowToggle.PositionOffset_X = 110;
			snowToggle.PositionOffset_Y = -175;
			snowToggle.PositionScale_Y = 1;
			snowToggle.SizeOffset_X = 40;
			snowToggle.SizeOffset_Y = 40;
			snowToggle.Value = LevelLighting.canSnow;
			snowToggle.AddLabel(localization.format("Snow_Toggle_Label"), ESleekSide.RIGHT);
			snowToggle.OnValueChanged += onToggledSnowToggle;
			container.AddChild(snowToggle);

			moonSlider = Glazier.Get().CreateSlider();
			moonSlider.PositionOffset_Y = -50;
			moonSlider.PositionScale_Y = 1;
			moonSlider.SizeOffset_X = 200;
			moonSlider.SizeOffset_Y = 20;
			moonSlider.Value = LevelLighting.moon / (float) LevelLighting.MOON_CYCLES;
			moonSlider.Orientation = ESleekOrientation.HORIZONTAL;
			moonSlider.AddLabel(localization.format("MoonSliderLabelText"), ESleekSide.RIGHT);
			moonSlider.OnValueChanged += onDraggedMoonSlider;
			container.AddChild(moonSlider);

			timeSlider = Glazier.Get().CreateSlider();
			timeSlider.PositionOffset_Y = -20;
			timeSlider.PositionScale_Y = 1;
			timeSlider.SizeOffset_X = 200;
			timeSlider.SizeOffset_Y = 20;
			timeSlider.Value = LevelLighting.time;
			timeSlider.Orientation = ESleekOrientation.HORIZONTAL;
			timeSlider.AddLabel(localization.format("TimeSliderLabelText"), ESleekSide.RIGHT);
			timeSlider.OnValueChanged += onDraggedTimeSlider;
			container.AddChild(timeSlider);

			timeButtons = new ISleekButton[4];
			for (int index = 0; index < timeButtons.Length; index++)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_X = 240;
				button.PositionOffset_Y = index * 40;
				button.SizeOffset_X = 200;
				button.SizeOffset_Y = 30;
				button.Text = localization.format("Time_" + index);
				button.OnClicked += onClickedTimeButton;
				lightingScrollBox.AddChild(button);

				timeButtons[index] = button;
			}

			infoBoxes = new ISleekBox[12];
			colorPickers = new SleekColorPicker[infoBoxes.Length];
			singleSliders = new ISleekSlider[5];

			for (int index = 0; index < colorPickers.Length; index++)
			{
				ISleekBox box = Glazier.Get().CreateBox();
				box.PositionOffset_X = 240;
				box.PositionOffset_Y = (timeButtons.Length * 40) + (index * 170);
				box.SizeOffset_X = 200;
				box.SizeOffset_Y = 30;
				box.Text = localization.format("Color_" + index);
				lightingScrollBox.AddChild(box);

				infoBoxes[index] = box;

				SleekColorPicker picker = new SleekColorPicker();
				picker.PositionOffset_X = 200;
				picker.PositionOffset_Y = (timeButtons.Length * 40) + (index * 170) + 40;
				picker.onColorPicked += onPickedColorPicker;
				lightingScrollBox.AddChild(picker);

				colorPickers[index] = picker;
			}

			for (int index = 0; index < singleSliders.Length; index++)
			{
				ISleekSlider slider = Glazier.Get().CreateSlider();
				slider.PositionOffset_X = 240;
				slider.PositionOffset_Y = (timeButtons.Length * 40) + (colorPickers.Length * 170) + (index * 30);
				slider.SizeOffset_X = 200;
				slider.SizeOffset_Y = 20;
				slider.Orientation = ESleekOrientation.HORIZONTAL;
				slider.AddLabel(localization.format("Single_" + index), ESleekSide.LEFT);
				slider.OnValueChanged += onDraggedSingleSlider;
				lightingScrollBox.AddChild(slider);

				singleSliders[index] = slider;
			}

			lightingScrollBox.ContentSizeOffset = new Vector2(0.0f, (timeButtons.Length * 40) + (colorPickers.Length * 170) + (singleSliders.Length * 30) - 10);

			updateSelection();
		}
	}
}
