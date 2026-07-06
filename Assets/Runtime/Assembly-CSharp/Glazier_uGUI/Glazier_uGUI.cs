////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace SDG.Unturned
{
	internal class Glazier_uGUI : GlazierBase, IGlazier
	{
		public ISleekBox CreateBox()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateBox");
			GlazierBox_uGUI box = new GlazierBox_uGUI(this);
			elements.Add(box);
			GlazierBox_uGUI.BoxPoolData poolData = ClaimElementFromPool(boxPool);
			if (poolData == null)
			{
				box.ConstructNew();
			}
			else
			{
				box.ConstructFromBoxPool(poolData);
			}
			box.SynchronizeTheme();
			box.SynchronizeColors();
			ValidateNewElement(box);
			Profiler.EndSample();
			return box;
		}

		public ISleekButton CreateButton()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateButton");
			GlazierButton_uGUI button = new GlazierButton_uGUI(this);
			elements.Add(button);
			GlazierButton_uGUI.ButtonPoolData poolData = ClaimElementFromPool(buttonPool);
			if (poolData == null)
			{
				button.ConstructNew();
			}
			else
			{
				button.ConstructFromButtonPool(poolData);
			}
			button.SynchronizeTheme();
			button.SynchronizeColors();
			ValidateNewElement(button);
			Profiler.EndSample();
			return button;
		}

		public ISleekElement CreateFrame()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateFrame");
			GlazierEmpty_uGUI frame = new GlazierEmpty_uGUI(this);
			elements.Add(frame);
			GlazierElementBase_uGUI.PoolData poolData = ClaimElementFromPool(framePool);
			if (poolData == null)
			{
				frame.ConstructNew();
			}
			else
			{
				frame.ConstructFromPool(poolData);
			}
			ValidateNewElement(frame);
			Profiler.EndSample();
			return frame;
		}

		public ISleekConstraintFrame CreateConstraintFrame()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateConstraintFrame");
			GlazierConstraintFrame_uGUI frame = new GlazierConstraintFrame_uGUI(this);
			frame.ConstructNew();
			elements.Add(frame);
			ValidateNewElement(frame);
			Profiler.EndSample();
			return frame;
		}

		public ISleekImage CreateImage()
		{
			return CreateImage(null);
		}

		public ISleekImage CreateImage(Texture texture)
		{
			Profiler.BeginSample("Glazier_uGUI.CreateImage");
			GlazierImage_uGUI image = new GlazierImage_uGUI(this);
			elements.Add(image);
			GlazierImage_uGUI.ImagePoolData poolData = ClaimElementFromPool(imagePool);
			if (poolData == null)
			{
				image.ConstructNew();
			}
			else
			{
				image.ConstructFromImagePool(poolData);
			}
			image.Texture = texture;
			image.SynchronizeColors();
			ValidateNewElement(image);
			Profiler.EndSample();
			return image;
		}

		public ISleekSprite CreateSprite()
		{
			return CreateSprite(null);
		}

		public ISleekSprite CreateSprite(Sprite sprite)
		{
			Profiler.BeginSample("Glazier_uGUI.CreateSprite");
			GlazierSprite_uGUI image = new GlazierSprite_uGUI(this, sprite);
			image.ConstructNew();
			elements.Add(image);
			image.Sprite = sprite;
			image.SynchronizeColors();
			ValidateNewElement(image);
			Profiler.EndSample();
			return image;
		}

		public ISleekLabel CreateLabel()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateLabel");
			GlazierLabel_uGUI label = new GlazierLabel_uGUI(this);
			elements.Add(label);
			GlazierLabel_uGUI.LabelPoolData poolData = ClaimElementFromPool(labelPool);
			if (poolData == null)
			{
				label.ConstructNew();
			}
			else
			{
				label.ConstructFromLabelPool(poolData);
			}
			label.SynchronizeColors();
			ValidateNewElement(label);
			Profiler.EndSample();
			return label;
		}

		public ISleekScrollView CreateScrollView()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateScrollView");
			GlazierScrollView_uGUI scrollView = new GlazierScrollView_uGUI(this);
			scrollView.ConstructNew();
			elements.Add(scrollView);
			scrollView.SynchronizeTheme();
			scrollView.SynchronizeColors();
			ValidateNewElement(scrollView);
			Profiler.EndSample();
			return scrollView;
		}

		public ISleekSlider CreateSlider()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateSlider");
			GlazierSlider_uGUI slider = new GlazierSlider_uGUI(this);
			slider.ConstructNew();
			elements.Add(slider);
			slider.SynchronizeTheme();
			slider.SynchronizeColors();
			ValidateNewElement(slider);
			Profiler.EndSample();
			return slider;
		}

		public ISleekField CreateStringField()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateStringField");
			GlazierStringField_uGUI field = new GlazierStringField_uGUI(this);
			field.ConstructNew();
			elements.Add(field);
			field.SynchronizeTheme();
			field.SynchronizeColors();
			ValidateNewElement(field);
			Profiler.EndSample();
			return field;
		}

		public ISleekToggle CreateToggle()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateToggle");
			GlazierToggle_uGUI toggle = new GlazierToggle_uGUI(this);
			toggle.ConstructNew();
			elements.Add(toggle);
			toggle.SynchronizeTheme();
			toggle.SynchronizeColors();
			ValidateNewElement(toggle);
			Profiler.EndSample();
			return toggle;
		}

		public ISleekUInt8Field CreateUInt8Field()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateUInt8Field");
			GlazierUInt8Field_uGUI field = new GlazierUInt8Field_uGUI(this);
			field.ConstructNew();
			elements.Add(field);
			field.SynchronizeTheme();
			field.SynchronizeColors();
			ValidateNewElement(field);
			Profiler.EndSample();
			return field;
		}

		public ISleekUInt16Field CreateUInt16Field()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateUInt16Field");
			GlazierUInt16Field_uGUI field = new GlazierUInt16Field_uGUI(this);
			field.ConstructNew();
			elements.Add(field);
			field.SynchronizeTheme();
			field.SynchronizeColors();
			ValidateNewElement(field);
			Profiler.EndSample();
			return field;
		}

		public ISleekUInt32Field CreateUInt32Field()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateUInt32Field");
			GlazierUInt32Field_uGUI field = new GlazierUInt32Field_uGUI(this);
			field.ConstructNew();
			elements.Add(field);
			field.SynchronizeTheme();
			field.SynchronizeColors();
			ValidateNewElement(field);
			Profiler.EndSample();
			return field;
		}

		public ISleekInt32Field CreateInt32Field()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateInt32Field");
			GlazierInt32Field_uGUI field = new GlazierInt32Field_uGUI(this);
			field.ConstructNew();
			elements.Add(field);
			field.SynchronizeTheme();
			field.SynchronizeColors();
			ValidateNewElement(field);
			Profiler.EndSample();
			return field;
		}

		public ISleekFloat32Field CreateFloat32Field()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateFloat32Field");
			GlazierFloat32Field_uGUI field = new GlazierFloat32Field_uGUI(this);
			field.ConstructNew();
			elements.Add(field);
			field.SynchronizeTheme();
			field.SynchronizeColors();
			ValidateNewElement(field);
			Profiler.EndSample();
			return field;
		}

		public ISleekFloat64Field CreateFloat64Field()
		{
			Profiler.BeginSample("Glazier_uGUI.CreateFloat64Field");
			GlazierFloat64Field_uGUI field = new GlazierFloat64Field_uGUI(this);
			field.ConstructNew();
			elements.Add(field);
			field.SynchronizeTheme();
			field.SynchronizeColors();
			ValidateNewElement(field);
			Profiler.EndSample();
			return field;
		}

		public ISleekProxyImplementation CreateProxyImplementation(SleekWrapper owner)
		{
			Profiler.BeginSample("Glazier_uGUI.CreateProxyImplementation");
			GlazierProxy_uGUI implementation = new GlazierProxy_uGUI(this);
			elements.Add(implementation);
			GlazierElementBase_uGUI.PoolData poolData = ClaimElementFromPool(framePool);
			if (poolData == null)
			{
				implementation.ConstructNew();
			}
			else
			{
				implementation.ConstructFromPool(poolData);
			}
			implementation.InitOwner(owner);
			ValidateNewElement(implementation);
			Profiler.EndSample();
			return implementation;
		}

		public bool SupportsDepth => true;

		public bool SupportsRichTextAlpha => true;

		public bool SupportsAutomaticLayout => true;
		public bool SupportsTilingSprite => true;

		private SleekWindow _root;
		private GlazierElementBase_uGUI rootImpl;
		public SleekWindow Root
		{
			get => _root;
			set
			{
				if (_root == value)
					return;

				// Old root gameObject may have been destroyed e.g. EditorUI destroyed when returning to main menu.
				if (rootImpl != null && rootImpl.transform != null)
				{
					rootImpl.transform.SetParent(null, false);
				}

				_root = value;

				if (_root != null)
				{
					rootImpl = _root.AttachmentRoot as GlazierElementBase_uGUI;
					if (rootImpl != null)
					{
						rootImpl.transform.SetParent(transform, false);
						rootImpl.transform.SetAsFirstSibling(); // Sort underneath cursor, tooltip, etc.
					}
					else
					{
						UnturnedLog.warn("Root must be a uGUI element: {0}", _root.GetType().Name);
					}
				}
				else
				{
					rootImpl = null;
				}

				// Hack to workaround plugins visible over loading screen.
				canvas.sortingOrder = _root != null && _root.hackSortOrder ?
					UnturnedCanvasSortOrders.LoadingScreen :
					UnturnedCanvasSortOrders.Glazier;
			}
		}

		public static Glazier_uGUI CreateGlazier()
		{
			GameObject gameObject = new GameObject("Glazier");
			DontDestroyOnLoad(gameObject);
			Glazier_uGUI component = gameObject.AddComponent<Glazier_uGUI>();

			return component;
		}

		internal void ReleaseBoxToPool(GlazierBox_uGUI.BoxPoolData poolData)
		{
			ValidateGameObjectReturnedToPool(poolData.gameObject);
			boxPool.Add(poolData);
		}

		internal void ReleaseButtonToPool(GlazierButton_uGUI.ButtonPoolData poolData)
		{
			ValidateGameObjectReturnedToPool(poolData.gameObject);
			buttonPool.Add(poolData);
		}

		internal void ReleaseEmptyToPool(GlazierElementBase_uGUI.PoolData poolData)
		{
			ValidateGameObjectReturnedToPool(poolData.gameObject);
			framePool.Add(poolData);
		}

		internal void ReleaseImageToPool(GlazierImage_uGUI.ImagePoolData poolData)
		{
			ValidateGameObjectReturnedToPool(poolData.gameObject);
			imagePool.Add(poolData);
		}

		internal void ReleaseLabelToPool(GlazierLabel_uGUI.LabelPoolData poolData)
		{
			ValidateGameObjectReturnedToPool(poolData.gameObject);
			labelPool.Add(poolData);
		}

		internal Material GetFontMaterial(ETextContrastStyle shadowStyle)
		{
			switch (shadowStyle)
			{
				default:
				case ETextContrastStyle.None:
					return fontMaterial_Default;
				case ETextContrastStyle.Shadow:
					return fontMaterial_Shadow;
				case ETextContrastStyle.Outline:
					return fontMaterial_Outline;
				case ETextContrastStyle.Tooltip:
					return fontMaterial_Tooltip;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			OptionsSettings.OnCustomColorsChanged += OnCustomColorsChanged;
			OptionsSettings.OnThemeChanged += OnThemeChanged;

			canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = UnturnedCanvasSortOrders.Glazier;
			if (clPixelPerfect.hasValue)
			{
				canvas.pixelPerfect = clPixelPerfect.value > 0;
			}
			// Cannot set additionalShaderChannels because TextMeshPro uses them.
			gameObject.AddComponent<GraphicRaycaster>();

			CreateFontMaterials();
			CreateDebugText();
			CreateCursor();
			CreateTooltip();
		}

		private void OnDestroy()
		{
			DestroyFontMaterials();
		}

		private void CreateDebugText()
		{
			GameObject debugGameObject = new GameObject("Debug", typeof(RectTransform));
			RectTransform debugTransform = debugGameObject.GetRectTransform();
			debugTransform.SetParent(transform, false);
			debugTransform.anchorMin = new Vector2(0.0f, 1.0f);
			debugTransform.anchorMax = new Vector2(0.0f, 1.0f);
			debugTransform.pivot = new Vector2(0.0f, 1.0f);
			debugTransform.sizeDelta = new Vector2(800.0f, 30.0f);
			debugTransform.anchoredPosition = Vector2.zero;

			ETextContrastStyle debugShadowStyle = SleekShadowStyle.ContextToStyle(ETextContrastContext.ColorfulBackdrop);
			debugTextComponent = debugGameObject.AddComponent<TextMeshProUGUI>();
			debugTextComponent.font = GlazierResources_uGUI.Font;
			debugTextComponent.fontSharedMaterial = GetFontMaterial(debugShadowStyle);
			debugTextComponent.characterSpacing = GlazierUtils_uGUI.GetCharacterSpacing(debugShadowStyle);
			debugTextComponent.fontSize = GlazierUtils_uGUI.GetFontSize(ESleekFontSize.Default);
			debugTextComponent.fontStyle = GlazierUtils_uGUI.GetFontStyleFlags(GlazierConst.DefaultLabelFontStyle);
			debugTextComponent.raycastTarget = false;
			debugTextComponent.alignment = TextAlignmentOptions.TopLeft;
			debugTextComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
			debugTextComponent.extraPadding = GlazierConst_uGUI.DefaultExtraPadding;
		}

		private void CreateFontMaterials()
		{
			fontMaterial_Default = Resources.Load<Material>("UI/Glazier_uGUI/Font_Default");
			fontMaterial_Outline = Instantiate(Resources.Load<Material>("UI/Glazier_uGUI/Font_Outline"));
			fontMaterial_Shadow = Instantiate(Resources.Load<Material>("UI/Glazier_uGUI/Font_Shadow"));
			fontMaterial_Tooltip = Instantiate(Resources.Load<Material>("UI/Glazier_uGUI/Font_Tooltip"));
			SynchronizeFontMaterials();
		}

		private void SynchronizeFontMaterials()
		{
			Color shadowColor = SleekCustomization.shadowColor;

			Color outlineOutlineColor = shadowColor;
			outlineOutlineColor.a = 0.25f;
			Color outlineUnderlayColor = shadowColor;
			outlineUnderlayColor.a = 0.75f;
			fontMaterial_Outline.SetColor("_OutlineColor", outlineOutlineColor);
			fontMaterial_Outline.SetColor("_UnderlayColor", outlineUnderlayColor);

			Color shadowUnderlayColor = shadowColor;
			shadowUnderlayColor.a = 0.75f;
			fontMaterial_Shadow.SetColor("_UnderlayColor", outlineUnderlayColor);

			Color tooltipOutlineColor = shadowColor;
			outlineOutlineColor.a = 1.0f;
			Color tooltipUnderlayColor = shadowColor;
			tooltipUnderlayColor.a = 1.0f;
			fontMaterial_Tooltip.SetColor("_OutlineColor", tooltipOutlineColor);
			fontMaterial_Tooltip.SetColor("_UnderlayColor", tooltipUnderlayColor);
		}

		private void DestroyFontMaterials()
		{
			Destroy(fontMaterial_Outline);
			Destroy(fontMaterial_Shadow);
			Destroy(fontMaterial_Tooltip);
		}

		private void CreateCursor()
		{
			GameObject cursorGameObject = new GameObject("Cursor", typeof(RectTransform));
			cursorTransform = cursorGameObject.GetRectTransform();
			cursorTransform.SetParent(transform, false);
			cursorTransform.anchorMin = Vector2.zero;
			cursorTransform.anchorMax = Vector2.zero;
			cursorTransform.pivot = new Vector2(0.0f, 1.0f);
			cursorTransform.sizeDelta = new Vector2(20.0f, 20.0f);
			cursorImage = cursorGameObject.AddComponent<RawImage>();
			cursorImage.texture = defaultCursor;
			cursorImage.raycastTarget = false;
			Canvas cursorDepthOverride = cursorGameObject.AddComponent<Canvas>();
			cursorDepthOverride.overrideSorting = true;
			cursorDepthOverride.sortingOrder = UnturnedCanvasSortOrders.Cursor;
		}

		private void CreateTooltip()
		{
			tooltipGameObject = new GameObject("Tooltip", typeof(RectTransform));
			tooltipTransform = tooltipGameObject.GetRectTransform();
			tooltipTransform.SetParent(transform, false);

			VerticalLayoutGroup verticalLayout = tooltipGameObject.AddComponent<VerticalLayoutGroup>();
			verticalLayout.childControlWidth = true;
			verticalLayout.childControlHeight = true;
			verticalLayout.childForceExpandWidth = false;
			verticalLayout.childForceExpandHeight = false;
			verticalLayout.padding = new RectOffset(5, 5, 5, 5);

			ContentSizeFitter contentSizeFitter = tooltipGameObject.AddComponent<ContentSizeFitter>();
			contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			tooltipShadowImage = tooltipGameObject.AddComponent<Image>();
			tooltipShadowImage.raycastTarget = false;
			tooltipShadowImage.type = Image.Type.Sliced;
			tooltipShadowImage.sprite = GlazierResources_uGUI.TooltipShadowSprite;
			SynchronizeTooltipShadowColor();

			GameObject textGameObject = new GameObject("Text", typeof(RectTransform));
			RectTransform textTransform = textGameObject.GetRectTransform();
			textTransform.SetParent(tooltipTransform, false);

			ETextContrastStyle tooltipShadowStyle = SleekShadowStyle.ContextToStyle(ETextContrastContext.Tooltip);
			tooltipTextComponent = textGameObject.AddComponent<TextMeshProUGUI>();
			tooltipTextComponent.font = GlazierResources_uGUI.Font;
			tooltipTextComponent.fontSharedMaterial = GetFontMaterial(tooltipShadowStyle);
			tooltipTextComponent.characterSpacing = GlazierUtils_uGUI.GetCharacterSpacing(tooltipShadowStyle);
			tooltipTextComponent.fontSize = GlazierUtils_uGUI.GetFontSize(ESleekFontSize.Default);
			tooltipTextComponent.fontStyle = GlazierUtils_uGUI.GetFontStyleFlags(GlazierConst.DefaultLabelFontStyle);
			tooltipTextComponent.raycastTarget = false;
			tooltipTextComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
			tooltipTextComponent.extraPadding = GlazierConst_uGUI.DefaultExtraPadding;
		}

		private void SynchronizeTooltipShadowColor()
		{
			Color tooltipShadowColor = SleekCustomization.shadowColor;
			tooltipShadowColor.a = 0.5f;
			tooltipShadowImage.color = tooltipShadowColor;
		}

		private void UpdateDebug()
		{
			if (OptionsSettings.debug && _root != null && (_root.isEnabled || _root.drawCursorWhileDisabled))
			{
				UpdateDebugStats();
				UpdateDebugString();

				debugTextComponent.color = debugStringColor;
				debugTextComponent.text = debugString;
				debugTextComponent.enabled = true;
			}
			else
			{
				debugTextComponent.enabled = false;
			}
		}

		private void UpdateCursor()
		{
			bool cursorVisible = Root.ShouldDrawCursor;
			if (cursorVisible != wasCursorVisible)
			{
				wasCursorVisible = cursorVisible;
				cursorImage.gameObject.SetActive(cursorVisible);
			}

			cursorImage.color = SleekCustomization.cursorColor;

			Vector2 normalizedMousePosition = InputEx.NormalizedMousePosition;
			cursorTransform.anchorMin = normalizedMousePosition;
			cursorTransform.anchorMax = normalizedMousePosition;
			cursorTransform.anchoredPosition = Vector2.zero;
			cursorImage.texture = defaultCursor;
		}

		private void UpdateTooltip()
		{
			bool tooltipVisible = Root.ShouldDrawTooltip;

			GlazieruGUITooltip activeTooltip = GlazieruGUITooltip.GetTooltip();
			if (activeTooltip != lastTooltip)
			{
				lastTooltip = activeTooltip;
				startedTooltip = Time.realtimeSinceStartup;
			}

			if (tooltipVisible
				&& activeTooltip != null
				&& !string.IsNullOrEmpty(activeTooltip.text)
				&& Time.realtimeSinceStartup - startedTooltip > 0.5f)
			{
				Vector2 normalizedMousePosition = InputEx.NormalizedMousePosition;
				tooltipTransform.anchorMin = normalizedMousePosition;
				tooltipTransform.anchorMax = normalizedMousePosition;

				if (normalizedMousePosition.x > 0.7f)
				{
					tooltipTransform.anchoredPosition = new Vector2(-10.0f, 0.0f);
					tooltipTransform.pivot = new Vector2(1.0f, 1.0f);
					tooltipTextComponent.alignment = TextAlignmentOptions.TopRight;
				}
				else
				{
					tooltipTransform.anchoredPosition = new Vector2(30.0f, 0.0f);
					tooltipTransform.pivot = new Vector2(0.0f, 1.0f);
					tooltipTextComponent.alignment = TextAlignmentOptions.TopLeft;
				}

				tooltipTextComponent.text = activeTooltip.text;
				tooltipTextComponent.color = activeTooltip.color;
				tooltipGameObject.SetActive(true);
			}
			else
			{
				tooltipGameObject.SetActive(false);
			}
		}

		private void OnCustomColorsChanged()
		{
			foreach (GlazierElementBase_uGUI element in EnumerateLiveElements())
			{
				element.SynchronizeColors();
			}

			SynchronizeFontMaterials();
			SynchronizeTooltipShadowColor();
		}

		private void OnThemeChanged()
		{
			foreach (GlazierElementBase_uGUI element in EnumerateLiveElements())
			{
				element.SynchronizeTheme();
				element.SynchronizeColors(); // Necessary for ESleekTint.BACKGROUND_IF_LIGHT
			}
		}

		/// <summary>
		/// Enumerate elements that are not in the pool.
		/// </summary>
		private IEnumerable<GlazierElementBase_uGUI> EnumerateLiveElements()
		{
			for (int index = elements.Count - 1; index >= 0; --index)
			{
				GlazierElementBase_uGUI item = elements[index];
				if (item.gameObject == null) // Has been destroyed! We should handle this better.
				{
					elements.RemoveAtFast(index);
					continue;
				}

				yield return item;
			}
		}

		/// <summary>
		/// Sanity check all returned elements have a gameObject.
		/// </summary>
		[System.Diagnostics.Conditional("VALIDATE_GLAZIER_USE_AFTER_DESTROY")]
		private void ValidateNewElement(GlazierElementBase_uGUI element)
		{
			if (element.gameObject == null)
			{
				throw new System.Exception("uGUI element constructed with null gameObject");
			}

			if (element.transform == null)
			{
				throw new System.Exception("uGUI element constructed with null transform");
			}

			// Nelson 2023-10-06: Hooray! This method came in useful. With auto layout some
			// components were not getting destroyed before returning to the pool.
			if (element.gameObject.GetComponent<LayoutElement>() != null)
			{
				throw new System.Exception("uGUI GameObject has a LayoutElement component, likely not removed before returning to the pool");
			}
			if (element.gameObject.GetComponent<LayoutGroup>() != null)
			{
				throw new System.Exception("uGUI GameObject has a LayoutGroup component, likely not removed before returning to the pool");
			}
		}

		[System.Diagnostics.Conditional("VALIDATE_GLAZIER_USE_AFTER_DESTROY")]
		private void ValidateGameObjectReturnedToPool(GameObject gameObject)
		{
			if (gameObject == null)
			{
				throw new System.Exception("uGUI element returned null gameObject to pool");
			}

			if (gameObject.GetComponent<LayoutElement>() != null)
			{
				throw new System.Exception("uGUI GameObject has a LayoutElement component, should have been removed before returning to the pool");
			}
			if (gameObject.GetComponent<LayoutGroup>() != null)
			{
				throw new System.Exception("uGUI GameObject has a LayoutGroup component, should have been removed before returning to the pool");
			}
		}

		private T ClaimElementFromPool<T>(List<T> pool) where T : GlazierElementBase_uGUI.PoolData
		{
			while (pool.Count > 0)
			{
				// Random index rather than a stack because we are more likely to catch pooling cleanup issues this way.
				int index = Random.Range(0, pool.Count - 1);
				T element = pool[index];
				pool.RemoveAtFast(index);
				if (element == null || element.gameObject == null)
				{
					// Someone else destroyed this element without our permission. Hard to track down.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					UnturnedLog.warn("uGUI element ({0}) was destroyed while in pool", typeof(T).Name);
#endif
					continue;
				}

				return element;
			}

			return null;
		}

		private void LateUpdate()
		{
			Profiler.BeginSample("Glazier_uGUI.UpdateCanvasScaler");
			float scaleFactor = GraphicsSettings.userInterfaceScale;
			if (MathfEx.IsNearlyEqual(scaleFactor, 1.0f, 0.001f))
			{
				if (canvasScaler != null)
				{
					Destroy(canvasScaler);
					canvasScaler = null;
				}
			}
			else
			{
				if (canvasScaler == null)
				{
					canvasScaler = gameObject.AddComponent<CanvasScaler>();
					canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
				}

				canvasScaler.scaleFactor = scaleFactor;
			}
			Profiler.EndSample();

			if (rootImpl != null)
			{
				Profiler.BeginSample("Glazier_uGUI.UpdateRoot");
				rootImpl.Update();
				Profiler.EndSample();

				bool cursorLocked = _root.isCursorLocked;
				if (wasCursorLocked != cursorLocked)
				{
					wasCursorLocked = cursorLocked;
					if (cursorLocked)
					{
						// Prevent Submit/Cancel events from being routed to button selected prior to locking cursor.
						EventSystem.current.SetSelectedGameObject(null);
					}
				}

				rootImpl.gameObject.SetActive(_root.isEnabled);
			}

			Profiler.BeginSample("Glazier_uGUI.UpdateDebug()");
			UpdateDebug();
			Profiler.EndSample();

			Profiler.BeginSample("Glazier_uGUI.UpdateCursor()");
			UpdateCursor();
			Profiler.EndSample();

			Profiler.BeginSample("Glazier_uGUI.UpdateTooltip()");
			UpdateTooltip();
			Profiler.EndSample();
		}

		private List<GlazierElementBase_uGUI> elements = new List<GlazierElementBase_uGUI>();
		private List<GlazierBox_uGUI.BoxPoolData> boxPool = new List<GlazierBox_uGUI.BoxPoolData>();
		private List<GlazierElementBase_uGUI.PoolData> framePool = new List<GlazierElementBase_uGUI.PoolData>();
		private List<GlazierButton_uGUI.ButtonPoolData> buttonPool = new List<GlazierButton_uGUI.ButtonPoolData>();
		private List<GlazierImage_uGUI.ImagePoolData> imagePool = new List<GlazierImage_uGUI.ImagePoolData>();
		private List<GlazierLabel_uGUI.LabelPoolData> labelPool = new List<GlazierLabel_uGUI.LabelPoolData>();

		private Canvas canvas;
		private CanvasScaler canvasScaler;
		private TextMeshProUGUI debugTextComponent;
		private RectTransform cursorTransform;
		private RawImage cursorImage;
		private GameObject tooltipGameObject;
		private RectTransform tooltipTransform;
		private TextMeshProUGUI tooltipTextComponent;
		private Image tooltipShadowImage;

		private GlazieruGUITooltip lastTooltip;
		private float startedTooltip;

		private bool wasCursorVisible;
		private bool wasCursorLocked;

		private Material fontMaterial_Default;
		private Material fontMaterial_Outline;
		private Material fontMaterial_Shadow;
		private Material fontMaterial_Tooltip;

		private static StaticResourceRef<Texture2D> defaultCursor = new StaticResourceRef<Texture2D>("UI/Glazier_uGUI/Cursor");
		private static CommandLineInt clPixelPerfect = new CommandLineInt("-uGUIPixelPerfect");
	}
}
