////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierImage_IMGUI : GlazierElementBase_IMGUI, ISleekImage
	{
		public Texture Texture
		{
			get;
			set;
		}

		public float RotationAngle
		{
			get;
			set;
		}

		public bool CanRotate
		{
			get;
			set;
		}

		public bool ShouldDestroyTexture
		{
			get;
			set;
		}

		public SleekColor TintColor
		{
			get;
			set;
		} = ESleekTint.NONE;

		public event System.Action OnClicked;
		public event System.Action OnRightClicked;

		public void UpdateTexture(Texture2D newTexture)
		{
			ValidateNotDestroyed();
			Texture = newTexture;
		}

		public void SetTextureAndShouldDestroy(Texture2D texture, bool shouldDestroyTexture)
		{
			ValidateNotDestroyed();
			if (this.Texture != null && this.ShouldDestroyTexture)
			{
				Object.Destroy(this.Texture);
			}

			this.Texture = texture;
			this.ShouldDestroyTexture = shouldDestroyTexture;
		}

		public override void InternalDestroy()
		{
			if (ShouldDestroyTexture && Texture != null)
			{
				Object.DestroyImmediate(Texture);
				Texture = null;
			}

			base.InternalDestroy();
		}

		public override void OnGUI()
		{
			if (CanRotate)
			{
				GlazierUtils_IMGUI.drawAngledImageTexture(drawRect, Texture, RotationAngle, TintColor);
			}
			else
			{
				GlazierUtils_IMGUI.drawImageTexture(drawRect, Texture, TintColor);
			}

			ChildrenOnGUI();

			if (OnClicked != null || OnRightClicked != null)
			{
				// IMGUI does not support depth, so we only enable our invisible button if a child was not clicked.
				// Drawing regardless of enabled is important to route IMGUI input properly. (Issue #2241)
				GUI.enabled = Event.current.type != EventType.Repaint && Event.current.type != EventType.Used;

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
					else if (Event.current.button == 1)
					{
						OnRightClicked?.Invoke();
					}
				}
			}
		}

		public GlazierImage_IMGUI(Texture texture) : base()
		{
			this.Texture = texture;
		}
	}
}
