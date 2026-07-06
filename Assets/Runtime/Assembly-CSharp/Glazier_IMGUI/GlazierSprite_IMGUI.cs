////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierSprite_IMGUI : GlazierElementBase_IMGUI, ISleekSprite
	{
		public Sprite Sprite
		{
			get;
			set;
		}

		public SleekColor TintColor
		{
			get;
			set;
		} = ESleekTint.NONE;

		public ESleekSpriteType DrawMethod
		{
			get;
			set;
		} = ESleekSpriteType.Tiled;

		public bool IsRaycastTarget
		{
			get;
			set;
		} = true;

		public Vector2Int TileRepeatHintForUITK
		{
			get;
			set;
		}

		public event System.Action OnClicked;

		public override void OnGUI()
		{
			if (Sprite != null)
			{
				switch (DrawMethod)
				{
					case ESleekSpriteType.Tiled:
						GlazierUtils_IMGUI.drawTile(drawRect, Sprite.texture, TintColor);
						break;

					case ESleekSpriteType.Sliced:
					{
						if (style == null)
						{
							style = new GUIStyle();
							style.normal.background = Sprite.texture;
							style.border = new RectOffset(20, 20, 20, 20);
						}
						GlazierUtils_IMGUI.drawSliced(drawRect, Sprite.texture, TintColor, style);
					}
					break;

					case ESleekSpriteType.Regular:
						GlazierUtils_IMGUI.drawImageTexture(drawRect, Sprite.texture, TintColor);
						break;
				}
			}

			ChildrenOnGUI();

			if (OnClicked != null)
			{
				// IMGUI does not support depth, so we only enable our invisible button if a child was not clicked.
				// Drawing regardless of enabled is important to route IMGUI input properly. (Issue #2241)
				GUI.enabled = IsRaycastTarget && Event.current.type != EventType.Repaint && Event.current.type != EventType.Used;

				Color restoreBackgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = ColorEx.BlackZeroAlpha;
				bool wasClicked = GUI.Button(drawRect, string.Empty);
				GUI.enabled = true;
				GUI.backgroundColor = restoreBackgroundColor;
				if (wasClicked)
				{
					if (Event.current.button == 0)
					{
						OnClicked?.Invoke();
					}
				}
			}
		}

		public GlazierSprite_IMGUI(Sprite sprite) : base()
		{
			this.Sprite = sprite;
		}

		private GUIStyle style;
	}
}
