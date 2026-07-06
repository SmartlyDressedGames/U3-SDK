////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekItemIcon : SleekWrapper
	{
		/// <summary>
		/// Hide existing icon until refresh.
		/// Experimented with doing this for every refresh, but it looks bad in particular for hotbar.
		/// </summary>
		public void Clear()
		{
			ValidateNotDestroyed();
			internalImage.Texture = null;
		}

		public void Refresh(ushort id, byte quality, byte[] state)
		{
			ValidateNotDestroyed();
			expectedHandle = ItemTool.getIcon(id, quality, state, OnIconReady);
		}

		public void Refresh(ushort id, byte quality, byte[] state, ItemAsset itemAsset)
		{
			ValidateNotDestroyed();
			expectedHandle = ItemTool.getIcon(id, quality, state, itemAsset, OnIconReady);
		}

		public void Refresh(ushort id, byte quality, byte[] state, ItemAsset itemAsset, int widthOverride, int heightOverride)
		{
			ValidateNotDestroyed();
			expectedHandle = ItemTool.getIcon(id, quality, state, itemAsset, widthOverride, heightOverride, OnIconReady);
		}

		public void Refresh(ItemAsset itemAsset, int widthOverride, int heightOverride)
		{
			ValidateNotDestroyed();
			expectedHandle = ItemTool.getIcon(itemAsset.id, 100, itemAsset.getState(), itemAsset, widthOverride, heightOverride, OnIconReady);
		}

		public byte rot
		{
			set
			{
				ValidateNotDestroyed();
				internalImage.RotationAngle = value * 90;
			}
		}

		public bool isAngled
		{
			set
			{
				ValidateNotDestroyed();
				internalImage.CanRotate = value;
			}
		}

		public SleekColor color
		{
			get
			{
				ValidateNotDestroyed();
				return internalImage.TintColor;
			}
			set
			{
				ValidateNotDestroyed();
				internalImage.TintColor = value;
			}
		}

		public override void OnDestroy()
		{
			// Clear reference so callback does not modify potentially released image.
			internalImage = null;
		}

		public SleekItemIcon() : base()
		{
			internalImage = Glazier.Get().CreateImage();
			internalImage.SizeScale_X = 1.0f;
			internalImage.SizeScale_Y = 1.0f;
			AddChild(internalImage);
		}

		private void OnIconReady(int handle, Texture2D texture)
		{
			if (internalImage != null && (handle == -1 || handle == expectedHandle))
			{
				internalImage.Texture = texture;
			}
		}

		private ISleekImage internalImage;
		private int expectedHandle;
	}
}
