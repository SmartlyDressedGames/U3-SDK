////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class Glazier_UIToolkit : GlazierBase, IGlazier
	{
		public ISleekBox CreateBox()
		{
			GlazierBox_UIToolkit element = new GlazierBox_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekButton CreateButton()
		{
			GlazierButton_UIToolkit element = new GlazierButton_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekElement CreateFrame()
		{
			GlazierEmpty_UIToolkit element = new GlazierEmpty_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekConstraintFrame CreateConstraintFrame()
		{
			GlazierConstraintFrame_UIToolkit element = new GlazierConstraintFrame_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekImage CreateImage()
		{
			return CreateImage(null);
		}

		public ISleekImage CreateImage(Texture texture)
		{
			GlazierImage_UIToolkit element = new GlazierImage_UIToolkit(this);
			liveElements.Add(element);
			element.Texture = texture;
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekSprite CreateSprite()
		{
			return CreateSprite(null);
		}

		public ISleekSprite CreateSprite(Sprite sprite)
		{
			GlazierSprite_UIToolkit element = new GlazierSprite_UIToolkit(this);
			liveElements.Add(element);
			element.Sprite = sprite;
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekLabel CreateLabel()
		{
			GlazierLabel_UIToolkit element = new GlazierLabel_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekScrollView CreateScrollView()
		{
			GlazierScrollView_UIToolkit element = new GlazierScrollView_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekSlider CreateSlider()
		{
			GlazierSlider_UIToolkit element = new GlazierSlider_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekField CreateStringField()
		{
			GlazierStringField_UIToolkit element = new GlazierStringField_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekToggle CreateToggle()
		{
			GlazierToggle_UIToolkit element = new GlazierToggle_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekUInt8Field CreateUInt8Field()
		{
			GlazierUInt8Field_UIToolkit element = new GlazierUInt8Field_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekUInt16Field CreateUInt16Field()
		{
			GlazierUInt16Field_UIToolkit element = new GlazierUInt16Field_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekUInt32Field CreateUInt32Field()
		{
			GlazierUInt32Field_UIToolkit element = new GlazierUInt32Field_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekInt32Field CreateInt32Field()
		{
			GlazierInt32Field_UIToolkit element = new GlazierInt32Field_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekFloat32Field CreateFloat32Field()
		{
			GlazierFloat32Field_UIToolkit element = new GlazierFloat32Field_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekFloat64Field CreateFloat64Field()
		{
			GlazierFloat64Field_UIToolkit element = new GlazierFloat64Field_UIToolkit(this);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public ISleekProxyImplementation CreateProxyImplementation(SleekWrapper owner)
		{
			GlazierProxy_UIToolkit element = new GlazierProxy_UIToolkit(this, owner);
			liveElements.Add(element);
			element.SynchronizeColors();
			ValidateNewElement(element);
			return element;
		}

		public override bool ShouldGameProcessKeyDown
		{
			get
			{
				if (!base.ShouldGameProcessKeyDown)
				{
					// uGUI text field has focus.
					return false;
				}

				return !(focusController.focusedElement is TextField);
			}
		}

		public bool SupportsDepth => true;

		public bool SupportsRichTextAlpha => true;

		public bool SupportsAutomaticLayout => true;
		public bool SupportsTilingSprite => false;

		private SleekWindow _root;
		private GlazierElementBase_UIToolkit rootImpl;
		public SleekWindow Root
		{
			get { return _root; }
			set
			{
				if (_root == value)
					return;

				if (rootImpl != null)
				{
					gameLayer.Remove(rootImpl.visualElement);
				}

				_root = value;

				if (_root != null)
				{
					rootImpl = _root.AttachmentRoot as GlazierElementBase_UIToolkit;
					if (rootImpl != null)
					{
						gameLayer.Add(rootImpl.visualElement);
					}
					else
					{
						UnturnedLog.warn("Root must be a UIToolkit element: {0}", _root.GetType().Name);
					}
				}
				else
				{
					rootImpl = null;
				}
			}
		}

		public static Glazier_UIToolkit CreateGlazier()
		{
			GameObject gameObject = new GameObject("Glazier");
			DontDestroyOnLoad(gameObject);
			Glazier_UIToolkit component = gameObject.AddComponent<Glazier_UIToolkit>();
			return component;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			OptionsSettings.OnThemeChanged += OnThemeChanged;
			OptionsSettings.OnCustomColorsChanged += OnCustomColorsChanged;

			document = gameObject.AddComponent<UIDocument>();
			document.panelSettings = Resources.Load<PanelSettings>("UI/Glazier_UIToolkit/PanelSettings");
			document.panelSettings.themeStyleSheet = GlazierResources_UIToolkit.Theme;
			document.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/Glazier_UIToolkit/DefaultVisualTree");
			focusController = document.rootVisualElement.focusController;

			gameLayer = new VisualElement();
			gameLayer.AddToClassList("unturned-glazier-layer");
			gameLayer.pickingMode = PickingMode.Ignore;
			document.rootVisualElement.Add(gameLayer);

			overlayLayer = new VisualElement();
			overlayLayer.AddToClassList("unturned-glazier-layer");
			overlayLayer.pickingMode = PickingMode.Ignore;
			document.rootVisualElement.Add(overlayLayer);

			CreateDebugLabel();
			CreateCursor();
			CreateTooltip();
		}

		protected void LateUpdate()
		{
			float scaleFactor = GraphicsSettings.userInterfaceScale;
			if (MathfEx.IsNearlyEqual(scaleFactor, 1.0f, 0.001f))
			{
				document.panelSettings.scale = 1.0f;
			}
			else
			{
				document.panelSettings.scale = scaleFactor;
			}

			if (rootImpl != null)
			{
				rootImpl.Update();

				bool cursorLocked = _root.isCursorLocked;
				if (wasCursorLocked != cursorLocked)
				{
					wasCursorLocked = cursorLocked;
					if (cursorLocked)
					{
						// Prevent Submit/Cancel events from being routed to button selected prior to locking cursor.
						// For example, this fixes pressing Tab when closing the dashboard.
						UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
						if (focusController.focusedElement != null)
						{
							focusController.focusedElement.Blur();
						}
					}
				}

				rootImpl.IsVisible = Root.isEnabled;
			}

			UpdateDebugLabel();
			UpdateCursor();
			UpdateTooltip();
		}

		internal void RemoveDestroyedElement(GlazierElementBase_UIToolkit element)
		{
			liveElements.Remove(element);
		}

		/// <summary>
		/// Sanity check all returned elements have a gameObject.
		/// </summary>
		[System.Diagnostics.Conditional("VALIDATE_GLAZIER_USE_AFTER_DESTROY")]
		private void ValidateNewElement(GlazierElementBase_UIToolkit element)
		{
			if (element.visualElement == null)
			{
				throw new System.Exception("UIToolkit element constructed with null visual element");
			}
		}

		/// <summary>
		/// Create software cursor visual element.
		/// </summary>
		private void CreateCursor()
		{
			cursorImage = new Image();
			cursorImage.AddToClassList("unturned-cursor");
			cursorImage.pickingMode = PickingMode.Ignore; // Don't need to click on the cursor image. :)
			overlayLayer.Add(cursorImage);
		}

		/// <summary>
		/// Create green label in the upper-left.
		/// </summary>
		private void CreateDebugLabel()
		{
			debugLabel = new Label();
			debugLabel.AddToClassList("unturned-debug");
			debugLabel.pickingMode = PickingMode.Ignore;
			debugLabel.visible = false;
			GlazierUtils_UIToolkit.ApplyTextContrast(debugLabel.style, ETextContrastContext.ColorfulBackdrop, 1.0f);
			overlayLayer.Add(debugLabel);
		}

		/// <summary>
		/// Create tooltip visual element.
		/// </summary>
		private void CreateTooltip()
		{
			tooltipLabel = new Label();
			tooltipLabel.AddToClassList("unturned-tooltip");
			tooltipLabel.pickingMode = PickingMode.Ignore;
			tooltipLabel.visible = false;
			GlazierUtils_UIToolkit.ApplyTextContrast(debugLabel.style, ETextContrastContext.Tooltip, 1.0f);
			overlayLayer.Add(tooltipLabel);

			SynchronizeTooltipShadowColor();
		}

		/// <summary>
		/// Update upper-left green text.
		/// </summary>
		private void UpdateDebugLabel()
		{
			if (OptionsSettings.debug && _root != null && (_root.isEnabled || _root.drawCursorWhileDisabled))
			{
				UpdateDebugStats();
				UpdateDebugString();

				debugLabel.style.color = debugStringColor;
				debugLabel.text = debugString;
				debugLabel.visible = true;
			}
			else
			{
				debugLabel.visible = false;
			}
		}

		/// <summary>
		/// Update software cursor visual element.
		/// </summary>
		private void UpdateCursor()
		{
			cursorImage.visible = Root.ShouldDrawCursor;
			cursorImage.style.unityBackgroundImageTintColor = SleekCustomization.cursorColor;

			// Percentages aren't normalized 0-1, rather 100 is 100%.
			Vector2 normalizedMousePosition = InputEx.NormalizedMousePosition;
			normalizedMousePosition.y = 1.0f - normalizedMousePosition.y;
			cursorImage.style.left = Length.Percent(normalizedMousePosition.x * 100.0f);
			cursorImage.style.top = Length.Percent(normalizedMousePosition.y * 100.0f);
		}

		/// <summary>
		/// Find hovered element and update tooltip visibility/text.
		/// </summary>
		private void UpdateTooltip()
		{
			Vector2 screenPosition = Input.mousePosition;
			screenPosition.y = Screen.height - screenPosition.y;
			IPanel rootPanel = document.rootVisualElement.panel;
			Vector2 panelRelativePosition = RuntimePanelUtils.ScreenToPanel(rootPanel, screenPosition);
			VisualElement topmostElement = rootPanel.Pick(panelRelativePosition);

			object topmostUserData;
			if (topmostElement != null)
			{
				topmostUserData = topmostElement.userData;
				if (topmostUserData == null)
				{
					topmostUserData = topmostElement.FindAncestorUserData();
				}
			}
			else
			{
				topmostUserData = null;
			}

			GlazierElementBase_UIToolkit tooltipElement = topmostUserData as GlazierElementBase_UIToolkit;
			if (tooltipElement != previousTooltipElement)
			{
				previousTooltipElement = tooltipElement;
				tooltipFocusTimer = 0.0f;
			}

			if (tooltipElement != null)
			{
				tooltipFocusTimer += Time.unscaledDeltaTime;
			}

			string tooltipText;
			Color tooltipColor;
			if (Root.ShouldDrawTooltip
				&& tooltipElement != null
				&& tooltipFocusTimer >= 0.5f
				&& tooltipElement.GetTooltipParameters(out tooltipText, out tooltipColor)
				&& !string.IsNullOrEmpty(tooltipText))
			{
				// Percentages aren't normalized 0-1, rather 100 is 100%.
				Vector2 normalizedMousePosition = InputEx.NormalizedMousePosition;
				normalizedMousePosition.y = 1.0f - normalizedMousePosition.y;
				tooltipLabel.style.top = Length.Percent(normalizedMousePosition.y * 100.0f);

				if (normalizedMousePosition.x > 0.7f)
				{
					tooltipLabel.style.left = StyleKeyword.Null; // Remove existing "left" inline value (if any).
					tooltipLabel.style.right = Length.Percent((1.0f - normalizedMousePosition.x) * 100.0f);
					tooltipLabel.style.marginLeft = 0;
					tooltipLabel.style.marginRight = 10;
					tooltipLabel.style.unityTextAlign = TextAnchor.UpperRight;
				}
				else
				{
					tooltipLabel.style.left = Length.Percent(normalizedMousePosition.x * 100.0f);
					tooltipLabel.style.right = StyleKeyword.Null; // Remove existing "right" inline value (if any).
					tooltipLabel.style.marginLeft = 30; // Push to the right 30px. (cursor is 20px wide)
					tooltipLabel.style.marginRight = 0;
					tooltipLabel.style.unityTextAlign = TextAnchor.UpperLeft;
				}

				tooltipLabel.text = tooltipText;
				tooltipLabel.style.color = tooltipColor;
				tooltipLabel.visible = true;
			}
			else
			{
				tooltipLabel.visible = false;
			}
		}

		private void SynchronizeTooltipShadowColor()
		{
			Color tooltipShadowColor = SleekCustomization.shadowColor;
			tooltipShadowColor.a = 0.5f;
			tooltipLabel.style.unityBackgroundImageTintColor = tooltipShadowColor;
		}

		private void OnThemeChanged()
		{
			document.panelSettings.themeStyleSheet = GlazierResources_UIToolkit.Theme;
		}

		private void OnCustomColorsChanged()
		{
			foreach (GlazierElementBase_UIToolkit element in liveElements)
			{
				element.SynchronizeColors();
			}

			SynchronizeTooltipShadowColor();
		}

		private UIDocument document;
		private FocusController focusController;

		private HashSet<GlazierElementBase_UIToolkit> liveElements = new HashSet<GlazierElementBase_UIToolkit>();

		/// <summary>
		/// Container for SleekWindow element.
		/// </summary>
		private VisualElement gameLayer;

		/// <summary>
		/// Container for top-level visual elements.
		/// </summary>
		private VisualElement overlayLayer;

		private Label debugLabel;
		private Image cursorImage;
		private Label tooltipLabel;

		/// <summary>
		/// Element under the cursor on the previous frame.
		/// </summary>
		private GlazierElementBase_UIToolkit previousTooltipElement;

		/// <summary>
		/// Duration in seconds the cursor has been over the element.
		/// </summary>
		private float tooltipFocusTimer;

		private bool wasCursorLocked;
	}
}
