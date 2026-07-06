////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class Glazier_IMGUI : GlazierBase, IGlazier
	{
		public ISleekBox CreateBox()
		{
			return new GlazierBox_IMGUI();
		}

		public ISleekButton CreateButton()
		{
			return new GlazierButton_IMGUI();
		}

		public ISleekElement CreateFrame()
		{
			return new GlazierElementBase_IMGUI();
		}

		public ISleekConstraintFrame CreateConstraintFrame()
		{
			return new GlazierConstraintFrame_IMGUI();
		}

		public ISleekImage CreateImage()
		{
			return new GlazierImage_IMGUI(null);
		}

		public ISleekImage CreateImage(Texture texture)
		{
			return new GlazierImage_IMGUI(texture);
		}

		public ISleekSprite CreateSprite()
		{
			return new GlazierSprite_IMGUI(null);
		}

		public ISleekSprite CreateSprite(Sprite sprite)
		{
			return new GlazierSprite_IMGUI(sprite);
		}

		public ISleekLabel CreateLabel()
		{
			return new GlazierLabel_IMGUI();
		}

		public ISleekScrollView CreateScrollView()
		{
			return new GlazierScrollView_IMGUI();
		}

		public ISleekSlider CreateSlider()
		{
			return new GlazierSlider_IMGUI();
		}

		public ISleekField CreateStringField()
		{
			return new GlazierStringField_IMGUI();
		}

		public ISleekToggle CreateToggle()
		{
			return new GlazierToggle_IMGUI();
		}

		public ISleekUInt8Field CreateUInt8Field()
		{
			return new GlazierUInt8Field_IMGUI();
		}

		public ISleekUInt16Field CreateUInt16Field()
		{
			return new GlazierUInt16Field_IMGUI();
		}

		public ISleekUInt32Field CreateUInt32Field()
		{
			return new GlazierUInt32Field_IMGUI();
		}

		public ISleekInt32Field CreateInt32Field()
		{
			return new GlazierInt32Field_IMGUI();
		}

		public ISleekFloat32Field CreateFloat32Field()
		{
			return new GlazierFloat32Field_IMGUI();
		}

		public ISleekFloat64Field CreateFloat64Field()
		{
			return new GlazierFloat64Field_IMGUI();
		}

		public ISleekProxyImplementation CreateProxyImplementation(SleekWrapper owner)
		{
			return new GlazierProxy_IMGUI(owner);
		}

		public bool SupportsDepth => false;

		public bool SupportsRichTextAlpha => false;

		public bool SupportsAutomaticLayout => false;
		public bool SupportsTilingSprite => true;

		private SleekWindow _root;
		private GlazierElementBase_IMGUI rootImpl;
		public SleekWindow Root
		{
			get => _root;
			set
			{
				if (_root == value)
					return;

				_root = value;

				if (_root != null)
				{
					rootImpl = _root.AttachmentRoot as GlazierElementBase_IMGUI;
					if (rootImpl != null)
					{
						rootImpl.isTransformDirty = true;
					}
					else
					{
						UnturnedLog.warn("Root must be an IMGUI element: {0}", _root.GetType().Name);
					}
				}
				else
				{
					rootImpl = null;
				}
			}
		}

		public static Glazier_IMGUI CreateGlazier()
		{
			GameObject gameObject = new GameObject("Glazier");
			DontDestroyOnLoad(gameObject);
			Glazier_IMGUI component = gameObject.AddComponent<Glazier_IMGUI>();
			return component;
		}

		private void LateUpdate()
		{
			if (rootImpl != null)
			{
				if (Screen.width != cachedScreenWidth || Screen.height != cachedScreenHeight)
				{
					cachedScreenWidth = Screen.width;
					cachedScreenHeight = Screen.height;
					rootImpl.isTransformDirty = true;
				}

				rootImpl.Update();
			}

			if (OptionsSettings.debug)
			{
				UpdateDebugStats();
				UpdateDebugString();
			}
		}

		private void CursorOnGUI()
		{
			if (!Root.ShouldDrawCursor)
				return;

			Rect cursorRect = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 20.0f, 20.0f);

			GUI.color = SleekCustomization.cursorColor;
			GUI.DrawTexture(cursorRect, defaultCursor);
			GUI.color = Color.white;
		}

		private void TooltipOnGUI()
		{
			if (!Root.ShouldDrawTooltip)
				return;

			string activeTooltip = GUI.tooltip;

			if (activeTooltip != lastTooltip)
			{
				lastTooltip = activeTooltip;
				startedTooltip = Time.realtimeSinceStartup;

				tooltipContent = new GUIContent(activeTooltip);
				tooltipShadowContent = RichTextUtil.makeShadowContent(tooltipContent);
			}

			if (!string.IsNullOrWhiteSpace(activeTooltip) && Time.realtimeSinceStartup - startedTooltip > 0.5f)
			{
				Rect tooltipRect = new Rect(0.0f, Screen.height - Input.mousePosition.y, 400.0f, 200.0f);
				Color tooltipColor = OptionsSettings.fontColor;

				if (Input.mousePosition.x > Screen.width - tooltipRect.width - 30)
				{
					tooltipRect.x = Input.mousePosition.x - 30 - tooltipRect.width;
					GlazierUtils_IMGUI.drawLabel(tooltipRect, FontStyle.Bold, TextAnchor.UpperRight, 12, tooltipShadowContent, tooltipColor, tooltipContent, ETextContrastContext.Tooltip);
				}
				else
				{
					tooltipRect.x = Input.mousePosition.x + 30;
					GlazierUtils_IMGUI.drawLabel(tooltipRect, FontStyle.Bold, TextAnchor.UpperLeft, 12, tooltipShadowContent, tooltipColor, tooltipContent, ETextContrastContext.Tooltip);
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			useGUILayout = false;
		}

		private void OnGUI()
		{
			GUI.skin = GlazierResources_IMGUI.ActiveSkin;

			if (_root != null && _root.isEnabled && rootImpl != null)
			{
				// Tried GUI.depth here, but that only works from separate MonoBehaviours.
				rootImpl.OnGUI();
			}

			if (Event.current.type == EventType.Repaint)
			{
				if (OptionsSettings.debug && _root != null && (_root.isEnabled || _root.drawCursorWhileDisabled))
				{
					Rect debugRect = new Rect(0, 0, 800, 30);
					GlazierUtils_IMGUI.drawLabel(debugRect, FontStyle.Normal, TextAnchor.UpperLeft, 12, false, debugStringColor, debugString, ETextContrastContext.ColorfulBackdrop);
				}

				CursorOnGUI();
				TooltipOnGUI();
			}
		}

		private int cachedScreenWidth = -1;
		private int cachedScreenHeight = -1;

		private string lastTooltip;
		private float startedTooltip;
		private GUIContent tooltipContent;
		private GUIContent tooltipShadowContent;

		private static StaticResourceRef<Texture2D> defaultCursor = new StaticResourceRef<Texture2D>("UI/Glazier_IMGUI/Cursor");
	}
}
