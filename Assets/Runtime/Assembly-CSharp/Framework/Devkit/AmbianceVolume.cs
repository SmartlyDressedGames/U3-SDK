////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit
{
	public enum EAmbianceVolumeFogOverrideMode
	{
		/// <summary>
		/// Volume doesn't override fog.
		/// </summary>
		None,

		/// <summary>
		/// Volume fog settings are the same at all times of day.
		/// </summary>
		Constant,

		/// <summary>
		/// Volume fog settings vary throughout the day.
		/// </summary>
		PerTimeOfDay,
	}

	[System.Serializable]
	internal struct AmbianceVolumeTimeOfDaySettings
	{
		public Color fogColor;
		public float fogIntensity;

		public AmbianceVolumeTimeOfDaySettings(IFormattedFileReader reader)
		{
			if (reader == null)
			{
				fogColor = Color.white;
				fogIntensity = 1;
				return;
			}

			fogColor = reader.readValue<Color>("Fog_Color");
			fogIntensity = reader.readValue<float>("Fog_Intensity");
		}

		public void Write(IFormattedFileWriter writer)
		{
			writer.writeValue("Fog_Color", fogColor);
			writer.writeValue("Fog_Intensity", fogIntensity);
		}
	}

	public class AmbianceVolume : LevelVolume<AmbianceVolume, AmbianceVolumeManager>, IAmbianceNode
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		[SerializeField]
		internal System.Guid _effectGuid;
		public System.Guid EffectGuid
		{
			get => _effectGuid;
			set
			{
				_effectGuid = value;
				cachedEffectAsset = null;
			}
		}

		/// <summary>
		/// Kept because lots of modders have been using this script in Unity,
		/// so removing legacy effect id would break their content.
		/// </summary>
		[SerializeField]
		protected ushort _id;
		public ushort id
		{
			[System.Obsolete]
			get => _id;
			set
			{
				_id = value;
				cachedEffectAsset = null;
			}
		}

		[SerializeField]
		protected bool _noWater;
		public bool noWater
		{
			get => _noWater;
			set => _noWater = value;
		}

		[SerializeField]
		protected bool _noLighting;
		public bool noLighting
		{
			get => _noLighting;
			set => _noLighting = value;
		}

		/// <summary>
		/// If per-weather mask AND is non zero the weather will blend in.
		/// </summary>
		[SerializeField]
		public uint weatherMask = 0xFFFFFFFF;

		[SerializeField]
		protected EAmbianceVolumeFogOverrideMode _fogOverrideMode;
		public EAmbianceVolumeFogOverrideMode FogOverrideMode
		{
			get => _fogOverrideMode;
			set => _fogOverrideMode = value;
		}

		/// <summary>
		/// Kept for backwards compatibility with fog volumes created in Unity / by mods.
		/// </summary>
		[SerializeField]
		protected bool _overrideFog;
		[System.Obsolete]
		public bool overrideFog
		{
			get => _overrideFog;
			set
			{
				// Doesn't set _overrideFog to value because we only used it to convert old volumes in Awake.
				_overrideFog = false;
				_fogOverrideMode = value ? EAmbianceVolumeFogOverrideMode.Constant : EAmbianceVolumeFogOverrideMode.PerTimeOfDay;
			}
		}

		[SerializeField]
		protected Color _fogColor = Color.white;
		public Color fogColor
		{
			get => _fogColor;
			set => _fogColor = value;
		}

		[SerializeField]
		protected float _fogIntensity;
		public float fogIntensity
		{
			get => _fogIntensity;
			set => _fogIntensity = value;
		}

		[SerializeField]
		internal AmbianceVolumeTimeOfDaySettings[] perTimeOfDaySettings;

		[SerializeField]
		public bool overrideAtmosphericFog;

		/// <summary>
		/// Distinguishes from zero falloff which may be useful deep in a cave.
		/// </summary>
		[SerializeField]
		public bool enableFalloff;

		/// <summary>
		/// Higher priority volumes override lower priority volumes.
		/// </summary>
		public int priority;

		/// <summary>
		/// When falloff is OFF, how long to fade in audio by time.
		/// </summary>
		public float audioFadeInDuration = 2.0f;

		/// <summary>
		/// When falloff is OFF, how long to fade out audio by time.
		/// </summary>
		public float audioFadeOutDuration = 2.0f;

		/// <summary>
		/// When falloff is OFF, how long to fade in audio by time.
		/// </summary>
		public float fogFadeInDuration = 20.0f;

		/// <summary>
		/// When falloff is OFF, how long to fade out audio by time.
		/// </summary>
		public float fogFadeOutDuration = 8.0f;

		/// <summary>
		/// When falloff is OFF, how long to fade in lighting by time.
		/// </summary>
		public float lightingFadeInDuration = 4.0f;

		/// <summary>
		/// When falloff is OFF, how long to fade out lighting by time.
		/// </summary>
		public float lightingFadeOutDuration = 4.0f;

		/// <summary>
		/// Used by lighting to get the currently active effect.
		/// </summary>
		public EffectAsset GetEffectAsset()
		{
			if (cachedEffectAsset == null || cachedEffectAsset.hasBeenReplaced)
			{
				cachedEffectAsset = Assets.FindEffectAssetByGuidOrLegacyId(_effectGuid, _id);
			}

			return cachedEffectAsset;
		}
		private EffectAsset cachedEffectAsset;

		internal void GetFogSettings(int blendKey, int currentKey, float timeAlpha, out Color overrideFogColor, out float overrideFogIntensity)
		{
			if (_fogOverrideMode != EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
			{
				overrideFogColor = fogColor;
				overrideFogIntensity = fogIntensity;
				return;
			}

			if (perTimeOfDaySettings == null || perTimeOfDaySettings.Length != 4)
			{
				overrideFogColor = Color.white;
				overrideFogIntensity = 0.0f;
				return;
			}

			ref AmbianceVolumeTimeOfDaySettings blendTo = ref perTimeOfDaySettings[currentKey];
			ref AmbianceVolumeTimeOfDaySettings blendFrom = ref perTimeOfDaySettings[blendKey == -1 ? currentKey : blendKey];
			overrideFogColor = Color.Lerp(blendFrom.fogColor, blendTo.fogColor, timeAlpha);
			overrideFogIntensity = Mathf.Lerp(blendFrom.fogIntensity, blendTo.fogIntensity, timeAlpha);
		}

		internal ref AmbianceVolumeTimeOfDaySettings GetFogSettings(ELightingTime time)
		{
			if (perTimeOfDaySettings == null || perTimeOfDaySettings.Length != 4)
			{
				perTimeOfDaySettings = new AmbianceVolumeTimeOfDaySettings[4];
				for (int index = 0; index < 4; ++index)
				{
					ref AmbianceVolumeTimeOfDaySettings settings = ref perTimeOfDaySettings[index];
					settings.fogColor = Color.white;
					settings.fogIntensity = 1;
				}
			}
			return ref perTimeOfDaySettings[(int) time];
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			string effectIdString = reader.readValue("Ambiance_ID");
			if (ushort.TryParse(effectIdString, out _id))
			{
				_effectGuid = System.Guid.Empty;
			}
			else if (System.Guid.TryParse(effectIdString, out _effectGuid))
			{
				_id = 0;
			}

			noWater = reader.readValue<bool>("No_Water");
			noLighting = reader.readValue<bool>("No_Lighting");

			if (reader.containsKey("Weather_Mask"))
			{
				weatherMask = reader.readValue<uint>("Weather_Mask");
			}
			else
			{
				weatherMask = uint.MaxValue;

				if (reader.containsKey("Can_Rain"))
				{
					bool legacyCanRain = reader.readValue<bool>("Can_Rain");
					if (!legacyCanRain)
					{
						weatherMask &= ~(1U << 0);
					}
				}

				if (reader.containsKey("Can_Snow"))
				{
					bool legacyCanSnow = reader.readValue<bool>("Can_Snow");
					if (!legacyCanSnow)
					{
						weatherMask &= ~(1U << 1);
					}
				}
			}

			if (reader.containsKey("Override_Fog_Mode"))
			{
				_fogOverrideMode = reader.readValue<EAmbianceVolumeFogOverrideMode>("Override_Fog_Mode");
				if (_fogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
				{
					perTimeOfDaySettings = new AmbianceVolumeTimeOfDaySettings[4];
					perTimeOfDaySettings[(int) ELightingTime.DAWN] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Dawn"));
					perTimeOfDaySettings[(int) ELightingTime.MIDDAY] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Midday"));
					perTimeOfDaySettings[(int) ELightingTime.DUSK] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Dusk"));
					perTimeOfDaySettings[(int) ELightingTime.MIDNIGHT] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Midnight"));
				}
			}
			else
			{
				bool legacyOverrideFog = reader.readValue<bool>("Override_Fog");
				if (legacyOverrideFog)
				{
					_fogOverrideMode = EAmbianceVolumeFogOverrideMode.Constant;
				}
			}

			fogColor = reader.readValue<Color>("Fog_Color");
			if (reader.containsKey("Fog_Intensity"))
			{
				fogIntensity = reader.readValue<float>("Fog_Intensity");
			}
			else
			{
				float fogHeight = reader.readValue<float>("Fog_Height");
				fogIntensity = Mathf.InverseLerp(-1024.0f, 1024.0f, fogHeight);
			}

			overrideAtmosphericFog = reader.readValue<bool>("Override_Atmospheric_Fog");
			enableFalloff = reader.readValue<bool>("Enable_Falloff");
			priority = reader.readValue<int>("Priority");

			if (reader.containsKey("Audio_FadeIn"))
			{
				audioFadeInDuration = reader.readValue<float>("Audio_FadeIn");
			}
			if (reader.containsKey("Audio_FadeOut"))
			{
				audioFadeOutDuration = reader.readValue<float>("Audio_FadeOut");
			}
			if (reader.containsKey("Fog_FadeIn"))
			{
				fogFadeInDuration = reader.readValue<float>("Fog_FadeIn");
			}
			if (reader.containsKey("Fog_FadeOut"))
			{
				fogFadeOutDuration = reader.readValue<float>("Fog_FadeOut");
			}
			if (reader.containsKey("Lighting_FadeIn"))
			{
				lightingFadeInDuration = reader.readValue<float>("Lighting_FadeIn");
			}
			if (reader.containsKey("Lighting_FadeOut"))
			{
				lightingFadeOutDuration = reader.readValue<float>("Lighting_FadeOut");
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			if (!_effectGuid.IsEmpty())
			{
				writer.writeValue("Ambiance_ID", _effectGuid);
			}
			else
			{
				writer.writeValue("Ambiance_ID", _id);
			}

			writer.writeValue("No_Water", noWater);
			writer.writeValue("No_Lighting", noLighting);

			writer.writeValue("Weather_Mask", weatherMask);

			writer.writeValue("Override_Fog_Mode", _fogOverrideMode);
			if (_fogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay
				&& perTimeOfDaySettings != null && perTimeOfDaySettings.Length == 4)
			{
				writer.beginObject("Dawn");
				perTimeOfDaySettings[(int) ELightingTime.DAWN].Write(writer);
				writer.endObject();

				writer.beginObject("Midday");
				perTimeOfDaySettings[(int) ELightingTime.MIDDAY].Write(writer);
				writer.endObject();

				writer.beginObject("Dusk");
				perTimeOfDaySettings[(int) ELightingTime.DUSK].Write(writer);
				writer.endObject();

				writer.beginObject("Midnight");
				perTimeOfDaySettings[(int) ELightingTime.MIDNIGHT].Write(writer);
				writer.endObject();
			}

			writer.writeValue("Fog_Color", fogColor);
			writer.writeValue("Fog_Intensity", fogIntensity);
			writer.writeValue("Override_Atmospheric_Fog", overrideAtmosphericFog);
			writer.writeValue("Enable_Falloff", enableFalloff);
			writer.writeValue("Priority", priority);

			writer.writeValue("Audio_FadeIn", audioFadeInDuration);
			writer.writeValue("Audio_FadeOut", audioFadeOutDuration);
			writer.writeValue("Fog_FadeIn", fogFadeInDuration);
			writer.writeValue("Fog_FadeOut", fogFadeOutDuration);
			writer.writeValue("Lighting_FadeIn", lightingFadeInDuration);
			writer.writeValue("Lighting_FadeOut", lightingFadeOutDuration);
		}

		protected override void Awake()
		{
			supportsFalloff = true;

			if (_overrideFog)
			{
				// Convert old setting loaded from asset bundle.
				_overrideFog = false;
				_fogOverrideMode = EAmbianceVolumeFogOverrideMode.Constant;
			}

			base.Awake();
		}

		private class Menu : SleekWrapper
		{
			public Menu(AmbianceVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;

				const int spacing = 10;
				float verticalOffset = 0;

				ISleekField idField = Glazier.Get().CreateStringField();
				idField.PositionOffset_Y = verticalOffset;
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 30;

				if (volume._effectGuid.IsEmpty())
				{
					idField.Text = volume._id.ToString();
				}
				else
				{
					idField.Text = volume._effectGuid.ToString("N");
				}

				idField.AddLabel("Effect ID", ESleekSide.RIGHT);
				idField.OnTextChanged += OnIdChanged;
				AddChild(idField);
				verticalOffset += idField.SizeOffset_Y + spacing;

				ISleekToggle noWaterToggle = Glazier.Get().CreateToggle();
				noWaterToggle.PositionOffset_Y = verticalOffset;
				noWaterToggle.SizeOffset_X = 40;
				noWaterToggle.SizeOffset_Y = 40;
				noWaterToggle.Value = volume.noWater;
				noWaterToggle.AddLabel("No Water", ESleekSide.RIGHT);
				noWaterToggle.OnValueChanged += OnNoWaterToggled;
				AddChild(noWaterToggle);
				verticalOffset += noWaterToggle.SizeOffset_Y + spacing;

				ISleekToggle noLightingToggle = Glazier.Get().CreateToggle();
				noLightingToggle.PositionOffset_Y = verticalOffset;
				noLightingToggle.SizeOffset_X = 40;
				noLightingToggle.SizeOffset_Y = 40;
				noLightingToggle.Value = volume.noLighting;
				noLightingToggle.AddLabel("No Lighting", ESleekSide.RIGHT);
				noLightingToggle.OnValueChanged += OnNoLightingToggled;
				AddChild(noLightingToggle);
				verticalOffset += noLightingToggle.SizeOffset_Y + spacing;

				ISleekUInt32Field weatherMaskField = Glazier.Get().CreateUInt32Field();
				weatherMaskField.PositionOffset_Y = verticalOffset;
				weatherMaskField.SizeOffset_X = 200;
				weatherMaskField.SizeOffset_Y = 30;
				weatherMaskField.Value = volume.weatherMask;
				weatherMaskField.AddLabel("Weather Mask", ESleekSide.RIGHT);
				weatherMaskField.OnValueChanged += OnWeatherMaskChanged;
				AddChild(weatherMaskField);
				verticalOffset += weatherMaskField.SizeOffset_Y + spacing;

				fogMode = new SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode>();
				fogMode.PositionOffset_Y = verticalOffset;
				fogMode.SizeOffset_X = 200;
				fogMode.SizeOffset_Y = 30;
				fogMode.SetEnum(volume.FogOverrideMode);
				fogMode.AddLabel("Override Fog", ESleekSide.RIGHT);
				fogMode.OnSwappedEnum += OnFogModeChanged;
				AddChild(fogMode);
				verticalOffset += fogMode.SizeOffset_Y + spacing;

				timeButton = new SleekButtonStateEnum<ELightingTime>();
				timeButton.PositionOffset_Y = verticalOffset;
				timeButton.SizeOffset_X = 200;
				timeButton.SizeOffset_Y = 30;
				timeButton.AddLabel("Fog Time", ESleekSide.RIGHT);
				timeButton.OnSwappedEnum += OnFogTimeChanged;
				AddChild(timeButton);
				verticalOffset += timeButton.SizeOffset_Y + spacing;

				fogColor = new SleekColorPicker();
				fogColor.PositionOffset_Y = verticalOffset;
				fogColor.onColorPicked += OnFogColorPicked;
				AddChild(fogColor);
				verticalOffset += fogColor.SizeOffset_Y + spacing;

				fogIntensityField = Glazier.Get().CreateFloat32Field();
				fogIntensityField.PositionOffset_Y = verticalOffset;
				fogIntensityField.SizeOffset_X = 200;
				fogIntensityField.SizeOffset_Y = 30;
				fogIntensityField.AddLabel("Fog Intensity", ESleekSide.RIGHT);
				fogIntensityField.OnValueChanged += OnFogIntensityChanged;
				AddChild(fogIntensityField);
				verticalOffset += fogIntensityField.SizeOffset_Y + spacing;

				ISleekToggle overrideAtmosphericFogToggle = Glazier.Get().CreateToggle();
				overrideAtmosphericFogToggle.PositionOffset_Y = verticalOffset;
				overrideAtmosphericFogToggle.SizeOffset_X = 40;
				overrideAtmosphericFogToggle.SizeOffset_Y = 40;
				overrideAtmosphericFogToggle.Value = volume.overrideAtmosphericFog;
				overrideAtmosphericFogToggle.AddLabel("Override Atmospheric Fog", ESleekSide.RIGHT);
				overrideAtmosphericFogToggle.OnValueChanged += OnOverrideAtmosphericFogToggled;
				AddChild(overrideAtmosphericFogToggle);
				verticalOffset += overrideAtmosphericFogToggle.SizeOffset_Y + spacing;

				ISleekToggle enableFalloffToggle = Glazier.Get().CreateToggle();
				enableFalloffToggle.PositionOffset_Y = verticalOffset;
				enableFalloffToggle.SizeOffset_X = 40;
				enableFalloffToggle.SizeOffset_Y = 40;
				enableFalloffToggle.Value = volume.enableFalloff;
				enableFalloffToggle.AddLabel("Use Falloff", ESleekSide.RIGHT);
				enableFalloffToggle.OnValueChanged += OnEnableFalloffToggled;
				AddChild(enableFalloffToggle);
				verticalOffset += enableFalloffToggle.SizeOffset_Y + spacing;

				ISleekInt32Field priorityField = Glazier.Get().CreateInt32Field();
				priorityField.PositionOffset_Y = verticalOffset;
				priorityField.SizeOffset_X = 200;
				priorityField.SizeOffset_Y = 30;
				priorityField.Value = volume.priority;
				priorityField.AddLabel("Priority", ESleekSide.RIGHT);
				priorityField.OnValueChanged += OnPriorityChanged;
				AddChild(priorityField);
				verticalOffset += priorityField.SizeOffset_Y + spacing;

				audioFadeInField = Glazier.Get().CreateFloat32Field();
				audioFadeInField.PositionOffset_Y = verticalOffset;
				audioFadeInField.SizeOffset_X = 200;
				audioFadeInField.SizeOffset_Y = 30;
				audioFadeInField.Value = volume.audioFadeInDuration;
				audioFadeInField.AddLabel("Audio Fade-In", ESleekSide.RIGHT);
				audioFadeInField.TooltipText = "Seconds for effect audio to fade in when distance falloff is disabled.";
				audioFadeInField.OnValueChanged += OnAudioFadeInDurationChanged;
				AddChild(audioFadeInField);
				verticalOffset += audioFadeInField.SizeOffset_Y + spacing;

				audioFadeOutField = Glazier.Get().CreateFloat32Field();
				audioFadeOutField.PositionOffset_Y = verticalOffset;
				audioFadeOutField.SizeOffset_X = 200;
				audioFadeOutField.SizeOffset_Y = 30;
				audioFadeOutField.Value = volume.audioFadeOutDuration;
				audioFadeOutField.AddLabel("Audio Fade-Out", ESleekSide.RIGHT);
				audioFadeOutField.TooltipText = "Seconds for effect audio to fade out when distance falloff is disabled.";
				audioFadeOutField.OnValueChanged += OnAudioFadeOutDurationChanged;
				AddChild(audioFadeOutField);
				verticalOffset += audioFadeOutField.SizeOffset_Y + spacing;

				fogFadeInField = Glazier.Get().CreateFloat32Field();
				fogFadeInField.PositionOffset_Y = verticalOffset;
				fogFadeInField.SizeOffset_X = 200;
				fogFadeInField.SizeOffset_Y = 30;
				fogFadeInField.Value = volume.fogFadeInDuration;
				fogFadeInField.AddLabel("Fog Fade-In", ESleekSide.RIGHT);
				fogFadeInField.TooltipText = "Seconds for fog to fade in when distance falloff is disabled.";
				fogFadeInField.OnValueChanged += OnFogFadeInDurationChanged;
				AddChild(fogFadeInField);
				verticalOffset += fogFadeInField.SizeOffset_Y + spacing;

				fogFadeOutField = Glazier.Get().CreateFloat32Field();
				fogFadeOutField.PositionOffset_Y = verticalOffset;
				fogFadeOutField.SizeOffset_X = 200;
				fogFadeOutField.SizeOffset_Y = 30;
				fogFadeOutField.Value = volume.fogFadeOutDuration;
				fogFadeOutField.AddLabel("Fog Fade-Out", ESleekSide.RIGHT);
				fogFadeOutField.TooltipText = "Seconds for fog to fade out when distance falloff is disabled.";
				fogFadeOutField.OnValueChanged += OnFogFadeOutDurationChanged;
				AddChild(fogFadeOutField);
				verticalOffset += fogFadeOutField.SizeOffset_Y + spacing;

				lightingFadeInField = Glazier.Get().CreateFloat32Field();
				lightingFadeInField.PositionOffset_Y = verticalOffset;
				lightingFadeInField.SizeOffset_X = 200;
				lightingFadeInField.SizeOffset_Y = 30;
				lightingFadeInField.Value = volume.lightingFadeInDuration;
				lightingFadeInField.AddLabel("Lighting Fade-In", ESleekSide.RIGHT);
				lightingFadeInField.TooltipText = "Seconds for lighting to fade in when distance falloff is disabled.";
				lightingFadeInField.OnValueChanged += OnLightingFadeInDurationChanged;
				AddChild(lightingFadeInField);
				verticalOffset += lightingFadeInField.SizeOffset_Y + spacing;

				lightingFadeOutField = Glazier.Get().CreateFloat32Field();
				lightingFadeOutField.PositionOffset_Y = verticalOffset;
				lightingFadeOutField.SizeOffset_X = 200;
				lightingFadeOutField.SizeOffset_Y = 30;
				lightingFadeOutField.Value = volume.lightingFadeOutDuration;
				lightingFadeOutField.AddLabel("Lighting Fade-Out", ESleekSide.RIGHT);
				lightingFadeOutField.TooltipText = "Seconds for lighting to fade out when distance falloff is disabled.";
				lightingFadeOutField.OnValueChanged += OnLightingFadeOutDurationChanged;
				AddChild(lightingFadeOutField);
				verticalOffset += lightingFadeOutField.SizeOffset_Y + spacing;

				UpdateFogSettings();
				UpdateFade();

				SizeOffset_Y = verticalOffset - spacing;
			}

			private void OnIdChanged(ISleekField field, string effectIdString)
			{
				if (ushort.TryParse(effectIdString, out volume._id))
				{
					volume._effectGuid = System.Guid.Empty;
				}
				else if (System.Guid.TryParse(effectIdString, out volume._effectGuid))
				{
					volume._id = 0;
				}
				else
				{
					volume._effectGuid = System.Guid.Empty;
					volume._id = 0;
				}
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnNoWaterToggled(ISleekToggle toggle, bool noWater)
			{
				volume.noWater = noWater;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnNoLightingToggled(ISleekToggle toggle, bool noLighting)
			{
				volume.noLighting = noLighting;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnWeatherMaskChanged(ISleekUInt32Field field, uint mask)
			{
				volume.weatherMask = mask;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnFogModeChanged(SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode> button, EAmbianceVolumeFogOverrideMode newFogMode)
			{
				volume.FogOverrideMode = newFogMode;
				UpdateFogSettings();
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnFogTimeChanged(SleekButtonStateEnum<ELightingTime> button, ELightingTime newTime)
			{
				UpdateFogSettings();
			}

			private void OnFogColorPicked(SleekColorPicker picker, Color color)
			{
				if (volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
				{
					ref AmbianceVolumeTimeOfDaySettings settings = ref volume.GetFogSettings(timeButton.GetEnum());
					settings.fogColor = color;
				}
				else
				{
					volume.fogColor = color;
				}

				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnFogIntensityChanged(ISleekFloat32Field field, float value)
			{
				if (volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
				{
					ref AmbianceVolumeTimeOfDaySettings settings = ref volume.GetFogSettings(timeButton.GetEnum());
					settings.fogIntensity = value;
				}
				else
				{
					volume.fogIntensity = value;
				}

				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnOverrideAtmosphericFogToggled(ISleekToggle toggle, bool overrideAtmosphericFog)
			{
				volume.overrideAtmosphericFog = overrideAtmosphericFog;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnEnableFalloffToggled(ISleekToggle toggle, bool enableFalloff)
			{
				volume.enableFalloff = enableFalloff;
				UpdateFade();
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnPriorityChanged(ISleekInt32Field field, int value)
			{
				volume.priority = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnAudioFadeInDurationChanged(ISleekFloat32Field field, float value)
			{
				volume.audioFadeInDuration = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnAudioFadeOutDurationChanged(ISleekFloat32Field field, float value)
			{
				volume.audioFadeOutDuration = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnFogFadeInDurationChanged(ISleekFloat32Field field, float value)
			{
				volume.fogFadeInDuration = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnFogFadeOutDurationChanged(ISleekFloat32Field field, float value)
			{
				volume.fogFadeOutDuration = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnLightingFadeInDurationChanged(ISleekFloat32Field field, float value)
			{
				volume.lightingFadeInDuration = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnLightingFadeOutDurationChanged(ISleekFloat32Field field, float value)
			{
				volume.lightingFadeOutDuration = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void UpdateFade()
			{
				bool useFadeDurations = !volume.enableFalloff;
				audioFadeInField.IsClickable = useFadeDurations;
				audioFadeOutField.IsClickable = useFadeDurations;
				fogFadeInField.IsClickable = useFadeDurations;
				fogFadeOutField.IsClickable = useFadeDurations;
				lightingFadeInField.IsClickable = useFadeDurations;
				lightingFadeOutField.IsClickable = useFadeDurations;
			}

			private void UpdateFogSettings()
			{
				timeButton.isInteractable = volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay;

				if (volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
				{
					ref AmbianceVolumeTimeOfDaySettings settings = ref volume.GetFogSettings(timeButton.GetEnum());
					fogColor.state = settings.fogColor;
					fogIntensityField.Value = settings.fogIntensity;
				}
				else
				{
					fogColor.state = volume.fogColor;
					fogIntensityField.Value = volume.fogIntensity;
				}
			}

			private AmbianceVolume volume;
			private SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode> fogMode;
			private SleekButtonStateEnum<ELightingTime> timeButton;
			private SleekColorPicker fogColor;
			private ISleekFloat32Field fogIntensityField;
			private ISleekFloat32Field audioFadeInField;
			private ISleekFloat32Field audioFadeOutField;
			private ISleekFloat32Field fogFadeInField;
			private ISleekFloat32Field fogFadeOutField;
			private ISleekFloat32Field lightingFadeInField;
			private ISleekFloat32Field lightingFadeOutField;
		}
	}
}
