////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuConfigurationGraphicsUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;
		private static ISleekButton defaultButton;

		private static ISleekScrollView graphicsBox;

		private static ISleekToggle ambientOcclusionToggle;
		private static ISleekToggle bloomToggle;
		private static ISleekToggle chromaticAberrationToggle;
		private static ISleekToggle filmGrainToggle;
		private static ISleekToggle blendToggle;
		private static ISleekToggle grassDisplacementToggle;
		private static ISleekToggle windToggle;
		private static ISleekToggle foliageFocusToggle;
		private static ISleekToggle ragdollsToggle;
		private static ISleekToggle debrisToggle;
		private static ISleekToggle blastToggle;
		private static ISleekToggle puddleToggle;
		private static ISleekToggle glitterToggle;
		private static ISleekToggle triplanarToggle;
		private static ISleekToggle skyboxReflectionToggle;
		private static ISleekToggle itemIconAntiAliasingToggle;
		private static ISleekToggle clutterToggle;

		private static ISleekSlider farClipDistanceSlider;
		private static ISleekSlider distanceSlider;
		private static ISleekSlider landmarkSlider;

		private static SleekButtonState landmarkButton;
		public static SleekButtonState antiAliasingButton;
		public static SleekButtonState anisotropicFilteringButton;
		private static SleekButtonState effectButton;
		private static SleekBoxIcon foliagePerf;
		private static SleekButtonState foliageButton;
		private static SleekButtonState sunShaftsButton;
		private static SleekButtonState lightingButton;
		private static SleekButtonState ambientOcclusionButton;
		private static SleekButtonState reflectionButton;
		private static SleekButtonState planarReflectionButton;
		private static SleekButtonState waterButton;
		private static SleekBoxIcon waterPerf;
		private static SleekButtonState scopeButton;
		private static SleekBoxIcon scopePerf;
		private static ISleekToggle scopeDarkPeripheralToggle;
		private static SleekButtonState outlineButton;
		private static SleekButtonState terrainButton;
		private static SleekButtonState renderButton;

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

			container.AnimateOutOfView(0, 1);
		}

		private static void onToggledAmbientOcclusion(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.isAmbientOcclusionEnabled = state;
			GraphicsSettings.apply("changed ambient occlusion");
		}

		private static void onToggledBloomToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.bloom = state;
			GraphicsSettings.apply("changed bloom");
		}

		private static void onToggledChromaticAberrationToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.chromaticAberration = state;
			GraphicsSettings.apply("changed chromatic aberration");
		}

		private static void onToggledFilmGrainToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.filmGrain = state;
			GraphicsSettings.apply("changed film grain");
		}

		private static void onToggledBlendToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.blend = state;
			GraphicsSettings.apply("changed blend");
		}

		private static void onToggledGrassDisplacementToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.grassDisplacement = state;
			GraphicsSettings.apply("changed grass displacement");
		}

		private static void onToggledWindToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.IsWindEnabled = state;
			GraphicsSettings.apply("changed wind");
		}

		private static void onToggledFoliageFocusToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.foliageFocus = state;
			GraphicsSettings.apply("changed foliage focus");
		}

		private static void onSwappedLandmarkState(SleekButtonState button, int index)
		{
			GraphicsSettings.landmarkQuality = (EGraphicQuality) index;
			GraphicsSettings.apply("changed landmark quality");
			updatePerfWarnings();
		}

		private static void onToggledRagdollsToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.ragdolls = state;
			GraphicsSettings.apply("changed ragdolls");
		}

		private static void onToggledDebrisToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.debris = state;
			GraphicsSettings.apply("changed debris");
		}

		private static void onToggledBlastToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.blast = state;
			GraphicsSettings.apply("changed blastmarks");
		}

		private static void onToggledPuddleToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.puddle = state;
			GraphicsSettings.apply("changed puddles");
		}

		private static void onToggledGlitterToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.glitter = state;
			GraphicsSettings.apply("changed glitter");
		}

		private static void onToggledTriplanarToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.triplanar = state;
			GraphicsSettings.apply("changed triplanar");
		}

		private static void onToggledSkyboxReflectionToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.skyboxReflection = state;
			GraphicsSettings.apply("changed skybox reflection");
		}

		private static void onToggledItemIconAntiAliasingToggle(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.IsItemIconAntiAliasingEnabled = state;
			GraphicsSettings.apply("changed item icon anti-aliasing");
		}

		private static void OnToggledClutter(ISleekToggle toggle, bool state)
		{
			GraphicsSettings.IsClutterEnabled = state;
			GraphicsSettings.apply("changed clutter");
		}

		private static void OnDraggedFarClipDistanceSlider(ISleekSlider slider, float state)
		{
			GraphicsSettings.NormalizedFarClipDistance = state;
			GraphicsSettings.apply("changed far clip distance");

			farClipDistanceSlider.UpdateLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(state * 150)));
		}

		private static void onDraggedDistanceSlider(ISleekSlider slider, float state)
		{
			GraphicsSettings.normalizedDrawDistance = state;
			GraphicsSettings.apply("changed draw distance");

			distanceSlider.UpdateLabel(localization.format("Distance_Slider_Label", 25 + (int) (state * 75)));
		}

		private static void onDraggedLandmarkSlider(ISleekSlider slider, float state)
		{
			GraphicsSettings.normalizedLandmarkDrawDistance = state;
			GraphicsSettings.apply("changed landmark draw distance");

			landmarkSlider.UpdateLabel(localization.format("Landmark_Slider_Label", (int) (state * 100)));
		}

		private static void onSwappedAntiAliasingState(SleekButtonState button, int index)
		{
			GraphicsSettings.antiAliasingType = (EAntiAliasingType) index;
			GraphicsSettings.apply("changed anti-aliasing type");
		}

		private static void onSwappedAnisotropicFilteringState(SleekButtonState button, int index)
		{
			GraphicsSettings.anisotropicFilteringMode = (EAnisotropicFilteringMode) index;
			GraphicsSettings.apply("changed anisotropic filtering mode");
		}

		private static void onSwappedEffectState(SleekButtonState button, int index)
		{
			GraphicsSettings.effectQuality = (EGraphicQuality) (index + 1);
			GraphicsSettings.apply("changed effect quality");
		}

		private static void onSwappedFoliageState(SleekButtonState button, int index)
		{
			GraphicsSettings.foliageQuality = (EGraphicQuality) index;
			GraphicsSettings.apply("changed foliage quality");
			updatePerfWarnings();
		}

		private static void onSwappedSunShaftsState(SleekButtonState button, int index)
		{
			GraphicsSettings.sunShaftsQuality = (EGraphicQuality) index;
			GraphicsSettings.apply("changed sun shafts quality");
		}

		private static void onSwappedLightingState(SleekButtonState button, int index)
		{
			GraphicsSettings.lightingQuality = (EGraphicQuality) index;
			GraphicsSettings.apply("changed lighting quality");
		}

		private static void onSwappedReflectionState(SleekButtonState button, int index)
		{
			GraphicsSettings.reflectionQuality = (EGraphicQuality) index;
			GraphicsSettings.apply("changed reflection quality");
		}

		private static void onSwappedPlanarReflectionState(SleekButtonState button, int index)
		{
			GraphicsSettings.planarReflectionQuality = (EGraphicQuality) (index + 1);
			GraphicsSettings.apply("changed planar reflection quality");
			updatePerfWarnings();
		}

		private static void onSwappedWaterState(SleekButtonState button, int index)
		{
			GraphicsSettings.waterQuality = (EGraphicQuality) (index + 1);
			GraphicsSettings.apply("changed water quality");
			updatePerfWarnings();
		}

		private static void onSwappedScopeState(SleekButtonState button, int index)
		{
			GraphicsSettings.scopeQuality = (EGraphicQuality) index;
			GraphicsSettings.apply("changed scope quality");
			updatePerfWarnings();
		}

		private static void OnToggledDarkScopePeripheral(ISleekToggle toggle, bool value)
		{
			GraphicsSettings.WantsDarkScopePeripheral = value;
		}

		private static void onSwappedOutlineState(SleekButtonState button, int index)
		{
			GraphicsSettings.outlineQuality = (EGraphicQuality) (index + 1);
			GraphicsSettings.apply("changed outline quality");
		}

		private static void onSwappedTerrainState(SleekButtonState button, int index)
		{
			GraphicsSettings.terrainQuality = (EGraphicQuality) (index + 1);
			GraphicsSettings.apply("changed terrain quality");
		}

		private static void onSwappedRenderState(SleekButtonState button, int index)
		{
			GraphicsSettings.renderMode = (ERenderMode) index;
			GraphicsSettings.apply("changed render mode");
			updatePerfWarnings();

			//aaHint.isVisible = GraphicsSettings.renderMode == ERenderMode.DEFERRED && (GraphicsSettings.antiAliasingType == EAntiAliasingType.MSAA2 || GraphicsSettings.antiAliasingType == EAntiAliasingType.MSAA4 || GraphicsSettings.antiAliasingType == EAntiAliasingType.MSAA8);
		}

		private static void onClickedBackButton(ISleekElement button)
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

		private static void onClickedDefaultButton(ISleekElement button)
		{
			GraphicsSettings.restoreDefaults();

			updateAll();
		}

		private static void updateAll()
		{
			ambientOcclusionToggle.Value = GraphicsSettings.isAmbientOcclusionEnabled;
			bloomToggle.Value = GraphicsSettings.bloom;
			chromaticAberrationToggle.Value = GraphicsSettings.chromaticAberration;
			filmGrainToggle.Value = GraphicsSettings.filmGrain;
			grassDisplacementToggle.Value = GraphicsSettings.grassDisplacement;
			windToggle.Value = GraphicsSettings.IsWindEnabled;
			foliageFocusToggle.Value = GraphicsSettings.foliageFocus;
			landmarkButton.state = (int) GraphicsSettings.landmarkQuality;
			ragdollsToggle.Value = GraphicsSettings.ragdolls;
			debrisToggle.Value = GraphicsSettings.debris;
			blastToggle.Value = GraphicsSettings.blast;
			puddleToggle.Value = GraphicsSettings.puddle;
			glitterToggle.Value = GraphicsSettings.glitter;
			triplanarToggle.Value = GraphicsSettings.triplanar;
			skyboxReflectionToggle.Value = GraphicsSettings.skyboxReflection;
			itemIconAntiAliasingToggle.Value = GraphicsSettings.IsItemIconAntiAliasingEnabled;
			clutterToggle.Value = GraphicsSettings.IsClutterEnabled;

			farClipDistanceSlider.Value = GraphicsSettings.NormalizedFarClipDistance;
			farClipDistanceSlider.UpdateLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(GraphicsSettings.NormalizedFarClipDistance * 150)));

			distanceSlider.Value = GraphicsSettings.normalizedDrawDistance;
			distanceSlider.UpdateLabel(localization.format("Distance_Slider_Label", 25 + (int) (GraphicsSettings.normalizedDrawDistance * 75)));

			landmarkSlider.Value = GraphicsSettings.normalizedLandmarkDrawDistance;
			landmarkSlider.UpdateLabel(localization.format("Landmark_Slider_Label", (int) (GraphicsSettings.normalizedLandmarkDrawDistance * 100)));

			antiAliasingButton.state = (int) GraphicsSettings.antiAliasingType;
			anisotropicFilteringButton.state = (int) GraphicsSettings.anisotropicFilteringMode;
			effectButton.state = ((int) GraphicsSettings.effectQuality) - 1;
			foliageButton.state = (int) GraphicsSettings.foliageQuality;
			sunShaftsButton.state = (int) GraphicsSettings.sunShaftsQuality;
			lightingButton.state = (int) GraphicsSettings.lightingQuality;
			reflectionButton.state = (int) GraphicsSettings.reflectionQuality;
			planarReflectionButton.state = (int) GraphicsSettings.planarReflectionQuality - 1;
			waterButton.state = ((int) GraphicsSettings.waterQuality) - 1;
			scopeButton.state = (int) GraphicsSettings.scopeQuality;
			scopeDarkPeripheralToggle.Value = GraphicsSettings.WantsDarkScopePeripheral; 
			outlineButton.state = ((int) GraphicsSettings.outlineQuality) - 1;
			terrainButton.state = ((int) GraphicsSettings.terrainQuality) - 1;
			renderButton.state = (int) GraphicsSettings.renderMode;

			updatePerfWarnings();
		}

		private static void updatePerfWarnings()
		{
			landmarkSlider.IsInteractable = GraphicsSettings.landmarkQuality != EGraphicQuality.OFF;

			foliagePerf.IsVisible = !SystemInfo.supportsInstancing;
			grassDisplacementToggle.IsInteractable = GraphicsSettings.foliageQuality != EGraphicQuality.OFF;
			foliageFocusToggle.IsInteractable = GraphicsSettings.foliageQuality != EGraphicQuality.OFF;

			// Planar reflections enabled on Ultra.
			waterPerf.IsVisible = GraphicsSettings.waterQuality == EGraphicQuality.ULTRA;
			planarReflectionButton.isInteractable = GraphicsSettings.waterQuality == EGraphicQuality.ULTRA;

			scopePerf.IsVisible = GraphicsSettings.scopeQuality != EGraphicQuality.OFF;

			// Not supported by forward renderer.
			reflectionButton.isInteractable = GraphicsSettings.renderMode == ERenderMode.DEFERRED;
			blastToggle.IsInteractable = GraphicsSettings.renderMode == ERenderMode.DEFERRED;

			scopeDarkPeripheralToggle.IsInteractable = GraphicsSettings.scopeQuality == EGraphicQuality.OFF;
		}

		public MenuConfigurationGraphicsUI()
		{
			localization = Localization.read("/Menu/Configuration/MenuConfigurationGraphics.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Menu/Icons/Configuration/MenuConfigurationGraphics");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;

			if (Provider.isConnected)
			{
				PlayerUI.container.AddChild(container);
			}
			else if (Level.isEditor)
			{
				// Yep this is messy and ideally needs to be cleaned up.
				EditorUI.window.AddChild(container);
			}
			else
			{
				MenuUI.container.AddChild(container);
			}

			Color32 tooltipHeaderColor = new Color32(240, 240, 240, byte.MaxValue);
			Color32 tooltipBodyColor = new Color32(180, 180, 180, byte.MaxValue);

			active = false;

			graphicsBox = Glazier.Get().CreateScrollView();
			graphicsBox.PositionOffset_X = -425;
			graphicsBox.PositionOffset_Y = 100;
			graphicsBox.PositionScale_X = 0.5f;
			graphicsBox.SizeOffset_X = 680;
			graphicsBox.SizeOffset_Y = -200;
			graphicsBox.SizeScale_Y = 1;
			graphicsBox.ScaleContentToWidth = true;
			container.AddChild(graphicsBox);
			int verticalOffset = 0;

			farClipDistanceSlider = Glazier.Get().CreateSlider();
			farClipDistanceSlider.PositionOffset_X = 205;
			farClipDistanceSlider.PositionOffset_Y = verticalOffset;
			farClipDistanceSlider.SizeOffset_X = 200;
			farClipDistanceSlider.SizeOffset_Y = 20;
			farClipDistanceSlider.Orientation = ESleekOrientation.HORIZONTAL;
			farClipDistanceSlider.AddLabel(localization.format("Far_Clip_Slider_Label", 50 + Mathf.RoundToInt(GraphicsSettings.NormalizedFarClipDistance * 150)), ESleekSide.RIGHT);
			farClipDistanceSlider.OnValueChanged += OnDraggedFarClipDistanceSlider;
			graphicsBox.AddChild(farClipDistanceSlider);
			verticalOffset += 30;
			farClipDistanceSlider.SideLabel.SizeOffset_X += 100; // Hack because the default label is too narrow.

			distanceSlider = Glazier.Get().CreateSlider();
			distanceSlider.PositionOffset_X = 205;
			distanceSlider.PositionOffset_Y = verticalOffset;
			distanceSlider.SizeOffset_X = 200;
			distanceSlider.SizeOffset_Y = 20;
			distanceSlider.Orientation = ESleekOrientation.HORIZONTAL;
			distanceSlider.AddLabel(localization.format("Distance_Slider_Label", 25 + (int) (GraphicsSettings.normalizedDrawDistance * 75)), ESleekSide.RIGHT);
			distanceSlider.OnValueChanged += onDraggedDistanceSlider;
			graphicsBox.AddChild(distanceSlider);
			verticalOffset += 30;
			distanceSlider.SideLabel.SizeOffset_X += 100; // Hack because the default label is too narrow.

			landmarkButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			landmarkButton.PositionOffset_X = 205;
			landmarkButton.PositionOffset_Y = verticalOffset;
			landmarkButton.SizeOffset_X = 200;
			landmarkButton.SizeOffset_Y = 30;
			landmarkButton.AddLabel(localization.format("Landmark_Button_Label"), ESleekSide.RIGHT);
			landmarkButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Landmark_Button_Tooltip"), tooltipHeaderColor)
				+ RichTextUtil.wrapWithColor("\n" + localization.format("Landmark_Low", localization.format("Low"))
				+ "\n" + localization.format("Landmark_Medium", localization.format("Medium"))
				+ "\n" + localization.format("Landmark_High", localization.format("High"))
				+ "\n" + localization.format("Landmark_Ultra", localization.format("Ultra")), tooltipBodyColor);
			landmarkButton.onSwappedState = onSwappedLandmarkState;
			graphicsBox.AddChild(landmarkButton);
			verticalOffset += 40;

			landmarkSlider = Glazier.Get().CreateSlider();
			landmarkSlider.PositionOffset_X = 205;
			landmarkSlider.PositionOffset_Y = verticalOffset;
			landmarkSlider.SizeOffset_X = 200;
			landmarkSlider.SizeOffset_Y = 20;
			landmarkSlider.Orientation = ESleekOrientation.HORIZONTAL;
			landmarkSlider.AddLabel(localization.format("Landmark_Slider_Label", 25 + (int) (GraphicsSettings.normalizedLandmarkDrawDistance * 75)), ESleekSide.RIGHT);
			landmarkSlider.OnValueChanged += onDraggedLandmarkSlider;
			graphicsBox.AddChild(landmarkSlider);
			verticalOffset += 30;
			landmarkSlider.SideLabel.SizeOffset_X += 100; // Hack because the default label is too narrow.

			clutterToggle = Glazier.Get().CreateToggle();
			clutterToggle.PositionOffset_X = 205;
			clutterToggle.PositionOffset_Y = verticalOffset;
			clutterToggle.SizeOffset_X = 40;
			clutterToggle.SizeOffset_Y = 40;
			clutterToggle.AddLabel(localization.format("Clutter_Label"), ESleekSide.RIGHT);
			clutterToggle.TooltipText = localization.format("Clutter_Tooltip");
			clutterToggle.OnValueChanged += OnToggledClutter;
			graphicsBox.AddChild(clutterToggle);
			verticalOffset += 50;

			ragdollsToggle = Glazier.Get().CreateToggle();
			ragdollsToggle.PositionOffset_X = 205;
			ragdollsToggle.PositionOffset_Y = verticalOffset;
			ragdollsToggle.SizeOffset_X = 40;
			ragdollsToggle.SizeOffset_Y = 40;
			ragdollsToggle.AddLabel(localization.format("Ragdolls_Toggle_Label"), ESleekSide.RIGHT);
			ragdollsToggle.TooltipText = localization.format("Ragdolls_Tooltip");
			ragdollsToggle.OnValueChanged += onToggledRagdollsToggle;
			graphicsBox.AddChild(ragdollsToggle);
			verticalOffset += 50;

			debrisToggle = Glazier.Get().CreateToggle();
			debrisToggle.PositionOffset_X = 205;
			debrisToggle.PositionOffset_Y = verticalOffset;
			debrisToggle.SizeOffset_X = 40;
			debrisToggle.SizeOffset_Y = 40;
			debrisToggle.AddLabel(localization.format("Debris_Toggle_Label"), ESleekSide.RIGHT);
			debrisToggle.TooltipText = localization.format("Debris_Tooltip");
			debrisToggle.OnValueChanged += onToggledDebrisToggle;
			graphicsBox.AddChild(debrisToggle);
			verticalOffset += 50;

			ambientOcclusionToggle = Glazier.Get().CreateToggle();
			ambientOcclusionToggle.PositionOffset_X = 205;
			ambientOcclusionToggle.PositionOffset_Y = verticalOffset;
			ambientOcclusionToggle.SizeOffset_X = 40;
			ambientOcclusionToggle.SizeOffset_Y = 40;
			ambientOcclusionToggle.AddLabel(localization.format("Ambient_Occlusion_Label"), ESleekSide.RIGHT);
			ambientOcclusionToggle.TooltipText = localization.format("Ambient_Occlusion_Tooltip");
			ambientOcclusionToggle.OnValueChanged += onToggledAmbientOcclusion;
			graphicsBox.AddChild(ambientOcclusionToggle);
			verticalOffset += 50;

			bloomToggle = Glazier.Get().CreateToggle();
			bloomToggle.PositionOffset_X = 205;
			bloomToggle.PositionOffset_Y = verticalOffset;
			bloomToggle.SizeOffset_X = 40;
			bloomToggle.SizeOffset_Y = 40;
			bloomToggle.AddLabel(localization.format("Bloom_Toggle_Label"), ESleekSide.RIGHT);
			bloomToggle.TooltipText = localization.format("Bloom_Tooltip");
			bloomToggle.OnValueChanged += onToggledBloomToggle;
			graphicsBox.AddChild(bloomToggle);
			verticalOffset += 50;

			filmGrainToggle = Glazier.Get().CreateToggle();
			filmGrainToggle.PositionOffset_X = 205;
			filmGrainToggle.PositionOffset_Y = verticalOffset;
			filmGrainToggle.SizeOffset_X = 40;
			filmGrainToggle.SizeOffset_Y = 40;
			filmGrainToggle.AddLabel(localization.format("Film_Grain_Toggle_Label"), ESleekSide.RIGHT);
			filmGrainToggle.TooltipText = localization.format("Film_Grain_Tooltip");
			filmGrainToggle.OnValueChanged += onToggledFilmGrainToggle;
			graphicsBox.AddChild(filmGrainToggle);
			verticalOffset += 50;

			blendToggle = Glazier.Get().CreateToggle();
			blendToggle.PositionOffset_X = 205;
			blendToggle.PositionOffset_Y = verticalOffset;
			blendToggle.SizeOffset_X = 40;
			blendToggle.SizeOffset_Y = 40;
			blendToggle.AddLabel(localization.format("Blend_Toggle_Label"), ESleekSide.RIGHT);
			blendToggle.TooltipText = localization.format("Blend_Tooltip");
			blendToggle.Value = GraphicsSettings.blend;
			blendToggle.OnValueChanged += onToggledBlendToggle;
			graphicsBox.AddChild(blendToggle);
			verticalOffset += 50;

			grassDisplacementToggle = Glazier.Get().CreateToggle();
			grassDisplacementToggle.PositionOffset_X = 205;
			grassDisplacementToggle.PositionOffset_Y = verticalOffset;
			grassDisplacementToggle.SizeOffset_X = 40;
			grassDisplacementToggle.SizeOffset_Y = 40;
			grassDisplacementToggle.AddLabel(localization.format("Grass_Displacement_Toggle_Label"), ESleekSide.RIGHT);
			grassDisplacementToggle.TooltipText = localization.format("Grass_Displacement_Tooltip");
			grassDisplacementToggle.OnValueChanged += onToggledGrassDisplacementToggle;
			graphicsBox.AddChild(grassDisplacementToggle);
			verticalOffset += 50;

			windToggle = Glazier.Get().CreateToggle();
			windToggle.PositionOffset_X = 205;
			windToggle.PositionOffset_Y = verticalOffset;
			windToggle.SizeOffset_X = 40;
			windToggle.SizeOffset_Y = 40;
			windToggle.AddLabel(localization.format("Wind_Toggle_Label"), ESleekSide.RIGHT);
			windToggle.TooltipText = localization.format("Wind_Tooltip");
			windToggle.OnValueChanged += onToggledWindToggle;
			graphicsBox.AddChild(windToggle);
			verticalOffset += 50;

			foliageFocusToggle = Glazier.Get().CreateToggle();
			foliageFocusToggle.PositionOffset_X = 205;
			foliageFocusToggle.PositionOffset_Y = verticalOffset;
			foliageFocusToggle.SizeOffset_X = 40;
			foliageFocusToggle.SizeOffset_Y = 40;
			foliageFocusToggle.AddLabel(localization.format("Foliage_Focus_Toggle_Label"), ESleekSide.RIGHT);
			foliageFocusToggle.OnValueChanged += onToggledFoliageFocusToggle;
			foliageFocusToggle.TooltipText = localization.format("Foliage_Focus_Tooltip");
			graphicsBox.AddChild(foliageFocusToggle);
			verticalOffset += 50;

			blastToggle = Glazier.Get().CreateToggle();
			blastToggle.PositionOffset_X = 205;
			blastToggle.PositionOffset_Y = verticalOffset;
			blastToggle.SizeOffset_X = 40;
			blastToggle.SizeOffset_Y = 40;
			blastToggle.AddLabel(localization.format("Blast_Toggle_Label"), ESleekSide.RIGHT);
			blastToggle.TooltipText = localization.format("Blast_Toggle_Tooltip");
			blastToggle.OnValueChanged += onToggledBlastToggle;
			graphicsBox.AddChild(blastToggle);
			verticalOffset += 50;

			puddleToggle = Glazier.Get().CreateToggle();
			puddleToggle.PositionOffset_X = 205;
			puddleToggle.PositionOffset_Y = verticalOffset;
			puddleToggle.SizeOffset_X = 40;
			puddleToggle.SizeOffset_Y = 40;
			puddleToggle.AddLabel(localization.format("Puddle_Toggle_Label"), ESleekSide.RIGHT);
			puddleToggle.TooltipText = localization.format("Puddle_Tooltip");
			puddleToggle.OnValueChanged += onToggledPuddleToggle;
			graphicsBox.AddChild(puddleToggle);
			verticalOffset += 50;

			glitterToggle = Glazier.Get().CreateToggle();
			glitterToggle.PositionOffset_X = 205;
			glitterToggle.PositionOffset_Y = verticalOffset;
			glitterToggle.SizeOffset_X = 40;
			glitterToggle.SizeOffset_Y = 40;
			glitterToggle.AddLabel(localization.format("Glitter_Toggle_Label"), ESleekSide.RIGHT);
			glitterToggle.TooltipText = localization.format("Glitter_Tooltip");
			glitterToggle.OnValueChanged += onToggledGlitterToggle;
			graphicsBox.AddChild(glitterToggle);
			verticalOffset += 50;

			triplanarToggle = Glazier.Get().CreateToggle();
			triplanarToggle.PositionOffset_X = 205;
			triplanarToggle.PositionOffset_Y = verticalOffset;
			triplanarToggle.SizeOffset_X = 40;
			triplanarToggle.SizeOffset_Y = 40;
			triplanarToggle.AddLabel(localization.format("Triplanar_Toggle_Label"), ESleekSide.RIGHT);
			triplanarToggle.TooltipText = localization.format("Triplanar_Tooltip");
			triplanarToggle.OnValueChanged += onToggledTriplanarToggle;
			graphicsBox.AddChild(triplanarToggle);
			verticalOffset += 50;

			skyboxReflectionToggle = Glazier.Get().CreateToggle();
			skyboxReflectionToggle.PositionOffset_X = 205;
			skyboxReflectionToggle.PositionOffset_Y = verticalOffset;
			skyboxReflectionToggle.SizeOffset_X = 40;
			skyboxReflectionToggle.SizeOffset_Y = 40;
			skyboxReflectionToggle.AddLabel(localization.format("Skybox_Reflection_Label"), ESleekSide.RIGHT);
			skyboxReflectionToggle.TooltipText = localization.format("Skybox_Reflection_Tooltip");
			skyboxReflectionToggle.OnValueChanged += onToggledSkyboxReflectionToggle;
			graphicsBox.AddChild(skyboxReflectionToggle);
			verticalOffset += 50;

			itemIconAntiAliasingToggle = Glazier.Get().CreateToggle();
			itemIconAntiAliasingToggle.PositionOffset_X = 205;
			itemIconAntiAliasingToggle.PositionOffset_Y = verticalOffset;
			itemIconAntiAliasingToggle.SizeOffset_X = 40;
			itemIconAntiAliasingToggle.SizeOffset_Y = 40;
			itemIconAntiAliasingToggle.AddLabel(localization.format("Item_Icon_Anti_Aliasing_Label"), ESleekSide.RIGHT);
			itemIconAntiAliasingToggle.TooltipText = localization.format("Item_Icon_Anti_Aliasing_Tooltip");
			itemIconAntiAliasingToggle.OnValueChanged += onToggledItemIconAntiAliasingToggle;
			graphicsBox.AddChild(itemIconAntiAliasingToggle);
			verticalOffset += 50;

			chromaticAberrationToggle = Glazier.Get().CreateToggle();
			chromaticAberrationToggle.PositionOffset_X = 205;
			chromaticAberrationToggle.PositionOffset_Y = verticalOffset;
			chromaticAberrationToggle.SizeOffset_X = 40;
			chromaticAberrationToggle.SizeOffset_Y = 40;
			chromaticAberrationToggle.AddLabel(localization.format("Chromatic_Aberration_Toggle_Label"), ESleekSide.RIGHT);
			chromaticAberrationToggle.TooltipText = localization.format("Chromatic_Aberration_Tooltip");
			chromaticAberrationToggle.OnValueChanged += onToggledChromaticAberrationToggle;
			graphicsBox.AddChild(chromaticAberrationToggle);
			verticalOffset += 50;

			antiAliasingButton = new SleekButtonState(new GUIContent(localization.format("Off")),
				new GUIContent(localization.format("FXAA")),
				new GUIContent(localization.format("TAA")),
				new GUIContent(localization.format("SMAA"))
				);
			antiAliasingButton.PositionOffset_X = 205;
			antiAliasingButton.PositionOffset_Y = verticalOffset;
			antiAliasingButton.SizeOffset_X = 200;
			antiAliasingButton.SizeOffset_Y = 30;
			antiAliasingButton.AddLabel(localization.format("Anti_Aliasing_Button_Label"), ESleekSide.RIGHT);
			antiAliasingButton.tooltip = localization.format("Anti_Aliasing_Button_Tooltip");
			antiAliasingButton.onSwappedState = onSwappedAntiAliasingState;
			graphicsBox.AddChild(antiAliasingButton);
			verticalOffset += 40;

			anisotropicFilteringButton = new SleekButtonState(new GUIContent(localization.format("AF_Disabled")), new GUIContent(localization.format("AF_Per_Texture")), new GUIContent(localization.format("AF_Forced_On")));
			anisotropicFilteringButton.PositionOffset_X = 205;
			anisotropicFilteringButton.PositionOffset_Y = verticalOffset;
			anisotropicFilteringButton.SizeOffset_X = 200;
			anisotropicFilteringButton.SizeOffset_Y = 30;
			anisotropicFilteringButton.AddLabel(localization.format("Anisotropic_Filtering_Button_Label"), ESleekSide.RIGHT);
			anisotropicFilteringButton.tooltip = localization.format("Anisotropic_Filtering_Button_Tooltip");
			anisotropicFilteringButton.onSwappedState = onSwappedAnisotropicFilteringState;
			graphicsBox.AddChild(anisotropicFilteringButton);
			verticalOffset += 40;

			effectButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			effectButton.PositionOffset_X = 205;
			effectButton.PositionOffset_Y = verticalOffset;
			effectButton.SizeOffset_X = 200;
			effectButton.SizeOffset_Y = 30;
			effectButton.AddLabel(localization.format("Effect_Button_Label"), ESleekSide.RIGHT);
			effectButton.tooltip = localization.format("Effect_Button_Tooltip");
			effectButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Effect_Button_Tooltip"), tooltipHeaderColor)
				+ RichTextUtil.wrapWithColor("\n" + localization.format("Effect_Tier", localization.format("Low"), GraphicsSettings.EFFECT_LOW)
				+ "\n" + localization.format("Effect_Tier", localization.format("Medium"), GraphicsSettings.EFFECT_MEDIUM)
				+ "\n" + localization.format("Effect_Tier", localization.format("High"), GraphicsSettings.EFFECT_HIGH)
				+ "\n" + localization.format("Effect_Tier", localization.format("Ultra"), GraphicsSettings.EFFECT_ULTRA), tooltipBodyColor);
			effectButton.onSwappedState = onSwappedEffectState;
			graphicsBox.AddChild(effectButton);
			verticalOffset += 40;

			foliageButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			foliageButton.PositionOffset_X = 205;
			foliageButton.PositionOffset_Y = verticalOffset;
			foliageButton.SizeOffset_X = 200;
			foliageButton.SizeOffset_Y = 30;
			foliageButton.AddLabel(localization.format("Foliage_Button_Label"), ESleekSide.RIGHT);
			foliageButton.tooltip = localization.format("Foliage_Button_Tooltip");
			foliageButton.onSwappedState = onSwappedFoliageState;
			graphicsBox.AddChild(foliageButton);

			foliagePerf = new SleekBoxIcon(icons.load<Texture2D>("Perf"));
			foliagePerf.PositionOffset_X = 175;
			foliagePerf.PositionOffset_Y = verticalOffset;
			foliagePerf.SizeOffset_X = 30;
			foliagePerf.SizeOffset_Y = 30;
			foliagePerf.iconColor = ESleekTint.FOREGROUND;
			foliagePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Foliage_Instancing_Not_Supported"), new Color(1, 0.5f, 0));
			graphicsBox.AddChild(foliagePerf);
			verticalOffset += 40;

			sunShaftsButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			sunShaftsButton.PositionOffset_X = 205;
			sunShaftsButton.PositionOffset_Y = verticalOffset;
			sunShaftsButton.SizeOffset_X = 200;
			sunShaftsButton.SizeOffset_Y = 30;
			sunShaftsButton.AddLabel(localization.format("Sun_Shafts_Button_Label"), ESleekSide.RIGHT);
			sunShaftsButton.tooltip = localization.format("Sun_Shafts_Button_Tooltip");
			sunShaftsButton.onSwappedState = onSwappedSunShaftsState;
			graphicsBox.AddChild(sunShaftsButton);
			verticalOffset += 40;

			lightingButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			lightingButton.PositionOffset_X = 205;
			lightingButton.PositionOffset_Y = verticalOffset;
			lightingButton.SizeOffset_X = 200;
			lightingButton.SizeOffset_Y = 30;
			lightingButton.AddLabel(localization.format("Lighting_Button_Label"), ESleekSide.RIGHT);
			lightingButton.tooltip = localization.format("Lighting_Button_Tooltip");
			lightingButton.onSwappedState = onSwappedLightingState;
			graphicsBox.AddChild(lightingButton);
			verticalOffset += 40;

			reflectionButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			reflectionButton.PositionOffset_X = 205;
			reflectionButton.PositionOffset_Y = verticalOffset;
			reflectionButton.SizeOffset_X = 200;
			reflectionButton.SizeOffset_Y = 30;
			reflectionButton.AddLabel(localization.format("Reflection_Button_Label"), ESleekSide.RIGHT);
			reflectionButton.tooltip = localization.format("Reflection_Button_Tooltip");
			reflectionButton.onSwappedState = onSwappedReflectionState;
			graphicsBox.AddChild(reflectionButton);
			verticalOffset += 40;

			waterButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			waterButton.PositionOffset_X = 205;
			waterButton.PositionOffset_Y = verticalOffset;
			waterButton.SizeOffset_X = 200;
			waterButton.SizeOffset_Y = 30;
			waterButton.AddLabel(localization.format("Water_Button_Label"), ESleekSide.RIGHT);
			waterButton.tooltip = localization.format("Water_Button_Tooltip");
			waterButton.onSwappedState = onSwappedWaterState;
			graphicsBox.AddChild(waterButton);

			waterPerf = new SleekBoxIcon(icons.load<Texture2D>("Perf"));
			waterPerf.PositionOffset_X = 175;
			waterPerf.PositionOffset_Y = verticalOffset;
			waterPerf.SizeOffset_X = 30;
			waterPerf.SizeOffset_Y = 30;
			waterPerf.iconColor = ESleekTint.FOREGROUND;
			waterPerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Water_Reflections"), new Color(1, 0.5f, 0));
			graphicsBox.AddChild(waterPerf);
			verticalOffset += 40;

			planarReflectionButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			planarReflectionButton.PositionOffset_X = 205;
			planarReflectionButton.PositionOffset_Y = verticalOffset;
			planarReflectionButton.SizeOffset_X = 200;
			planarReflectionButton.SizeOffset_Y = 30;
			planarReflectionButton.AddLabel(localization.format("Planar_Reflection_Button_Label"), ESleekSide.RIGHT);
			planarReflectionButton.tooltip = RichTextUtil.wrapWithColor(localization.format("Planar_Reflection_Button_Tooltip"), tooltipHeaderColor)
				+ RichTextUtil.wrapWithColor("\n" + localization.format("Planar_Reflection_Low", localization.format("Low"))
				+ "\n" + localization.format("Planar_Reflection_Medium", localization.format("Medium"))
				+ "\n" + localization.format("Planar_Reflection_High", localization.format("High"))
				+ "\n" + localization.format("Planar_Reflection_Ultra", localization.format("Ultra")), tooltipBodyColor);
			planarReflectionButton.onSwappedState = onSwappedPlanarReflectionState;
			graphicsBox.AddChild(planarReflectionButton);
			verticalOffset += 40;

			scopeButton = new SleekButtonState(new GUIContent(localization.format("Off")), new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			scopeButton.PositionOffset_X = 205;
			scopeButton.PositionOffset_Y = verticalOffset;
			scopeButton.SizeOffset_X = 200;
			scopeButton.SizeOffset_Y = 30;
			scopeButton.AddLabel(localization.format("Scope_Button_Label"), ESleekSide.RIGHT);
			scopeButton.tooltip = localization.format("Scope_Button_Tooltip");
			scopeButton.onSwappedState = onSwappedScopeState;
			graphicsBox.AddChild(scopeButton);

			scopePerf = new SleekBoxIcon(icons.load<Texture2D>("Perf"));
			scopePerf.PositionOffset_X = 175;
			scopePerf.PositionOffset_Y = verticalOffset;
			scopePerf.SizeOffset_X = 30;
			scopePerf.SizeOffset_Y = 30;
			scopePerf.iconColor = ESleekTint.FOREGROUND;
			scopePerf.tooltip = RichTextUtil.wrapWithColor(localization.format("Perf_Dual_Render_Scope"), new Color(1, 0.5f, 0));
			graphicsBox.AddChild(scopePerf);
			verticalOffset += 40;

			scopeDarkPeripheralToggle = Glazier.Get().CreateToggle();
			scopeDarkPeripheralToggle.PositionOffset_X = 205;
			scopeDarkPeripheralToggle.PositionOffset_Y = verticalOffset;
			scopeDarkPeripheralToggle.SizeOffset_X = 40;
			scopeDarkPeripheralToggle.SizeOffset_Y = 40;
			scopeDarkPeripheralToggle.AddLabel(localization.format("DarkScopePeripheral_Label"), ESleekSide.RIGHT);
			scopeDarkPeripheralToggle.TooltipText = localization.format("DarkScopePeripheral_Tooltip");
			scopeDarkPeripheralToggle.OnValueChanged += OnToggledDarkScopePeripheral;
			graphicsBox.AddChild(scopeDarkPeripheralToggle);
			verticalOffset += 50;

			outlineButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			outlineButton.PositionOffset_X = 205;
			outlineButton.PositionOffset_Y = verticalOffset;
			outlineButton.SizeOffset_X = 200;
			outlineButton.SizeOffset_Y = 30;
			outlineButton.AddLabel(localization.format("Outline_Button_Label"), ESleekSide.RIGHT);
			outlineButton.tooltip = localization.format("Outline_Button_Tooltip");
			outlineButton.onSwappedState = onSwappedOutlineState;
			graphicsBox.AddChild(outlineButton);
			verticalOffset += 40;

			terrainButton = new SleekButtonState(new GUIContent(localization.format("Low")), new GUIContent(localization.format("Medium")), new GUIContent(localization.format("High")), new GUIContent(localization.format("Ultra")));
			terrainButton.PositionOffset_X = 205;
			terrainButton.PositionOffset_Y = verticalOffset;
			terrainButton.SizeOffset_X = 200;
			terrainButton.SizeOffset_Y = 30;
			terrainButton.AddLabel(localization.format("Terrain_Button_Label"), ESleekSide.RIGHT);
			terrainButton.tooltip = localization.format("Terrain_Button_Tooltip");
			terrainButton.onSwappedState = onSwappedTerrainState;
			graphicsBox.AddChild(terrainButton);
			verticalOffset += 40;

			renderButton = new SleekButtonState(new GUIContent(localization.format("Deferred")), new GUIContent(localization.format("Forward")));
			renderButton.PositionOffset_X = 205;
			renderButton.PositionOffset_Y = verticalOffset;
			renderButton.SizeOffset_X = 200;
			renderButton.SizeOffset_Y = 30;
			renderButton.AddLabel(localization.format("Render_Mode_Button_Label"), ESleekSide.RIGHT);
			renderButton.tooltip = localization.format("Render_Mode_Button_Tooltip");
			renderButton.onSwappedState = onSwappedRenderState;
			graphicsBox.AddChild(renderButton);
			verticalOffset += 40;

			graphicsBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10);

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
			container.AddChild(backButton);

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
			container.AddChild(defaultButton);

			updateAll();
		}
	}
}
