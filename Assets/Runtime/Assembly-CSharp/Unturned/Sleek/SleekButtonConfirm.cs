////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void Confirm(SleekButtonIconConfirm button);
	public delegate void Deny(SleekButtonIconConfirm button);

	public class SleekButtonIconConfirm : SleekWrapper
	{
		public Confirm onConfirmed;
		public Deny onDenied;

		public string text
		{
			get
			{
				ValidateNotDestroyed();
				return mainButton.text;
			}
			set
			{
				ValidateNotDestroyed();
				mainButton.text = value;
			}
		}

		public string tooltip
		{
			get
			{
				ValidateNotDestroyed();
				return mainButton.tooltip;
			}
			set
			{
				ValidateNotDestroyed();
				mainButton.tooltip = value;
			}
		}

		public ESleekFontSize fontSize
		{
			get
			{
				ValidateNotDestroyed();
				return mainButton.fontSize;
			}
			set
			{
				ValidateNotDestroyed();
				mainButton.fontSize = value;
			}
		}

		public SleekColor iconColor
		{
			get
			{
				ValidateNotDestroyed();
				return mainButton.iconColor;
			}
			set
			{
				ValidateNotDestroyed();
				mainButton.iconColor = value;
			}
		}

		public bool isClickable
		{
			get
			{
				ValidateNotDestroyed();
				return mainButton.isClickable;
			}
			set
			{
				ValidateNotDestroyed();
				mainButton.isClickable = value;
			}
		}

		public void reset()
		{
			ValidateNotDestroyed();
			mainButton.IsVisible = true;
			confirmButton.IsVisible = false;
			denyButton.IsVisible = false;
		}

		private void onClickedConfirmButton(ISleekElement button)
		{
			reset();

			onConfirmed?.Invoke(this);
		}

		private void onClickedDenyButton(ISleekElement button)
		{
			reset();

			onDenied?.Invoke(this);
		}

		private void onClickedMainButton(ISleekElement button)
		{
			mainButton.IsVisible = false;
			confirmButton.IsVisible = true;
			denyButton.IsVisible = true;
		}

		public SleekButtonIconConfirm(Texture2D newIcon, string newConfirm, string newConfirmTooltip, string newDeny, string newDenyTooltip) : this(newIcon, newConfirm, newConfirmTooltip, newDeny, newDenyTooltip, 0)
		{ }

		public SleekButtonIconConfirm(Texture2D newIcon, string newConfirm, string newConfirmTooltip, string newDeny, string newDenyTooltip, int iconSize) : base()
		{
			mainButton = new SleekButtonIcon(newIcon, iconSize);
			mainButton.SizeScale_X = 1.0f;
			mainButton.SizeScale_Y = 1.0f;
			mainButton.onClickedButton += onClickedMainButton;
			AddChild(mainButton);

			confirmButton = Glazier.Get().CreateButton();
			confirmButton.SizeOffset_X = -5;
			confirmButton.SizeScale_X = 0.5f;
			confirmButton.SizeScale_Y = 1;
			confirmButton.Text = newConfirm;
			confirmButton.TooltipText = newConfirmTooltip;
			confirmButton.OnClicked += onClickedConfirmButton;
			AddChild(confirmButton);
			confirmButton.IsVisible = false;

			denyButton = Glazier.Get().CreateButton();
			denyButton.PositionOffset_X = 5;
			denyButton.PositionScale_X = 0.5f;
			denyButton.SizeOffset_X = -5;
			denyButton.SizeScale_X = 0.5f;
			denyButton.SizeScale_Y = 1;
			denyButton.Text = newDeny;
			denyButton.TooltipText = newDenyTooltip;
			denyButton.OnClicked += onClickedDenyButton;
			AddChild(denyButton);
			denyButton.IsVisible = false;
		}

		private SleekButtonIcon mainButton;
		private ISleekButton confirmButton;
		private ISleekButton denyButton;
	}
}
