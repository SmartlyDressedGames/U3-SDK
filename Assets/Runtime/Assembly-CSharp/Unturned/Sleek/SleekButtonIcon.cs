////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekButtonIcon : SleekWrapper
	{
		public Texture2D icon
		{
			set
			{
				ValidateNotDestroyed();
				iconImage.Texture = value;

				if (iconSize == 0 && !iconFillsButton && iconImage.Texture != null)
				{
					iconImage.SizeOffset_X = iconImage.Texture.width;
					iconImage.SizeOffset_Y = iconImage.Texture.height;
				}

				if (label != null)
				{
					UpdateLabelAlignment();
				}
			}
		}

		public event ClickedButton onClickedButton;
		public event ClickedButton onRightClickedButton;

		public string text
		{
			get
			{
				ValidateNotDestroyed();
				return label != null ? label.Text : button.Text;
			}
			set
			{
				ValidateNotDestroyed();
				if (label != null)
				{
					label.Text = value;
				}
				else
				{
					button.Text = value;
				}
			}
		}

		public TextAnchor TextAlignment
		{
			get
			{
				ValidateNotDestroyed();
				return label != null ? label.TextAlignment : button.TextAlignment;
			}

			set
			{
				ValidateNotDestroyed();
				if (label != null)
				{
					label.TextAlignment = value;
				}
				else
				{
					button.TextAlignment = value;
				}
			}
		}

		public string tooltip
		{
			get
			{
				ValidateNotDestroyed();
				return button.TooltipText;
			}
			set
			{
				ValidateNotDestroyed();
				button.TooltipText = value;
			}
		}

		public ESleekFontSize fontSize
		{
			get
			{
				ValidateNotDestroyed();
				return button.FontSize;
			}
			set
			{
				ValidateNotDestroyed();
				button.FontSize = value;
				if (label != null)
				{
					label.FontSize = value;
				}
			}
		}

		public ETextContrastContext shadowStyle
		{
			get
			{
				ValidateNotDestroyed();
				return button.TextContrastContext;
			}
			set
			{
				ValidateNotDestroyed();
				button.TextContrastContext = value;
				if (label != null)
				{
					label.TextContrastContext = value;
				}
			}
		}

		public SleekColor backgroundColor
		{
			get
			{
				ValidateNotDestroyed();
				return button.BackgroundColor;
			}
			set
			{
				ValidateNotDestroyed();
				button.BackgroundColor = value;
			}
		}

		public SleekColor textColor
		{
			get
			{
				ValidateNotDestroyed();
				return button.TextColor;
			}
			set
			{
				ValidateNotDestroyed();
				button.TextColor = value;
				if (label != null)
				{
					label.TextColor = value;
				}
			}
		}

		public bool enableRichText
		{
			get
			{
				ValidateNotDestroyed();
				return button.AllowRichText;
			}
			set
			{
				ValidateNotDestroyed();
				button.AllowRichText = value;
				if (label != null)
				{
					label.AllowRichText = value;
				}
			}
		}

		public int iconPositionOffset
		{
			set
			{
				ValidateNotDestroyed();
				iconImage.PositionOffset_X = value;
				iconImage.PositionOffset_Y = value;

				if (label != null)
				{
					UpdateLabelAlignment();
				}
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

		public bool isClickable
		{
			get
			{
				ValidateNotDestroyed();
				return button.IsClickable;
			}
			set
			{
				ValidateNotDestroyed();
				button.IsClickable = value;
			}
		}

		public SleekButtonIcon(Texture2D newIcon, int newSize, bool newScale) : base()
		{
			iconSize = newSize;
			iconFillsButton = newScale;

			button = Glazier.Get().CreateButton();
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			button.BackgroundColor = ESleekTint.BACKGROUND;
			button.OnClicked += onClickedInternalButton;
			button.OnRightClicked += onRightClickedInternalButton;
			AddChild(button);

			iconImage = Glazier.Get().CreateImage();
			iconImage.Texture = newIcon;
			iconPositionOffset = 5;

			if (iconFillsButton)
			{
				iconImage.SizeOffset_X = -10;
				iconImage.SizeOffset_Y = -10;
				iconImage.SizeScale_X = 1f;
				iconImage.SizeScale_Y = 1f;
			}
			else
			{
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
			AddChild(iconImage);

			button.TextAlignment = TextAnchor.MiddleCenter;
			button.FontSize = ESleekFontSize.Default;
		}

		public SleekButtonIcon(Texture2D newIcon, int newSize) : this(newIcon, newSize, false)
		{ }

		public SleekButtonIcon(Texture2D newIcon) : this(newIcon, 0, false)
		{ }

		public SleekButtonIcon(Texture2D newIcon, bool newScale) : this(newIcon, 0, newScale)
		{ }

		private ISleekButton button;
		private int iconSize;
		private bool iconFillsButton;
		private ISleekImage iconImage;
		private ISleekLabel label;

		private void onClickedInternalButton(ISleekElement internalButton)
		{
			onClickedButton?.Invoke(this);
		}

		private void onRightClickedInternalButton(ISleekElement internalButton)
		{
			onRightClickedButton?.Invoke(this);
		}

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
