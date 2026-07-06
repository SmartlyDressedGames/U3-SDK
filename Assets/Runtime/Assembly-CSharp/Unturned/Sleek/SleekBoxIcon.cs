////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekBoxIcon : SleekWrapper
	{
		public Texture2D icon
		{
			set
			{
				ValidateNotDestroyed();
				iconImage.Texture = value;

				if (iconSize == 0 && iconImage.Texture != null)
				{
					iconImage.SizeOffset_X = iconImage.Texture.width;
					iconImage.SizeOffset_Y = iconImage.Texture.height;
				}

				UpdateLabelAlignment();
			}
		}

		public string text
		{
			get
			{
				ValidateNotDestroyed();
				return label.Text;
			}
			set
			{
				ValidateNotDestroyed();
				label.Text = value;
			}
		}

		public string tooltip
		{
			get
			{
				ValidateNotDestroyed();
				return box.TooltipText;
			}
			set
			{
				ValidateNotDestroyed();
				box.TooltipText = value;
			}
		}

		public ESleekFontSize fontSize
		{
			get
			{
				ValidateNotDestroyed();
				return label.FontSize;
			}
			set
			{
				ValidateNotDestroyed();
				label.FontSize = value;
			}
		}

		public SleekColor iconColor
		{
			get
			{
				ValidateNotDestroyed();
				return iconImage.TintColor;
			}
			set
			{
				ValidateNotDestroyed();
				iconImage.TintColor = value;
			}
		}

		public SleekBoxIcon(Texture2D newIcon, int newSize) : base()
		{
			iconSize = newSize;

			box = Glazier.Get().CreateBox();
			box.SizeScale_X = 1.0f;
			box.SizeScale_Y = 1.0f;
			AddChild(box);

			iconImage = Glazier.Get().CreateImage();
			iconImage.PositionOffset_X = 5;
			iconImage.PositionOffset_Y = 5;
			iconImage.Texture = newIcon;
			AddChild(iconImage);

			if (iconSize == 0)
			{
				if (iconImage.Texture != null)
				{
					iconImage.SizeOffset_X = iconImage.Texture.width;
					iconImage.SizeOffset_Y = iconImage.Texture.height;
				}
			}
			else
			{
				iconImage.SizeOffset_X = iconSize;
				iconImage.SizeOffset_Y = iconSize;
			}

			label = Glazier.Get().CreateLabel();
			label.SizeScale_X = 1.0f;
			label.SizeScale_Y = 1.0f;
			UpdateLabelAlignment();
			AddChild(label);
		}

		public SleekBoxIcon(Texture2D newIcon) : this(newIcon, 0)
		{ }

		private ISleekBox box;
		private ISleekImage iconImage;
		private int iconSize;
		private ISleekLabel label;

		private void UpdateLabelAlignment()
		{
			if (iconImage.Texture != null)
			{
				label.PositionOffset_X = iconImage.SizeOffset_X + (iconImage.PositionOffset_X * 2);
				label.SizeOffset_X = -label.PositionOffset_X - iconImage.PositionOffset_X;
			}
			else
			{
				label.PositionOffset_X = 5;
				label.SizeOffset_X = -10;
			}
		}
	}
}
