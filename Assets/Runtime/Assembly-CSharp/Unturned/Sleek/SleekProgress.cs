////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekProgress : SleekWrapper
	{
		public enum ERoundingMode
		{
			Round,
			Floor,
			Ceil,
		}
		
		private ISleekImage background;
		private ISleekImage foreground;
		private ISleekLabel label;

		// Not ideal. Replace with unit enum perhaps? e.g. Percentage vs Speed
		public string suffix;

		public ERoundingMode roundingMode = ERoundingMode.Round; 

		private float _state;
		public float state
		{
			get => _state;

			set
			{
				_state = Mathf.Clamp01(value);

				foreground.SizeScale_X = state;
				if (suffix.Length == 0)
				{
					int perc;
					switch (roundingMode)
					{
						default:
						case ERoundingMode.Round:
							perc = Mathf.RoundToInt(state * 100);
							break;

						case ERoundingMode.Floor:
							perc = Mathf.FloorToInt(state * 100);
							break;

						case ERoundingMode.Ceil:
							perc = Mathf.CeilToInt(state * 100);
							break;
					}

					label.Text = perc + "%";
				}
			}
		}

		public int measure
		{
			set
			{
				if (suffix.Length != 0)
				{
					label.Text = value + suffix;
				}
			}
		}

		public Color color
		{
			get => foreground.TintColor;

			set
			{
				Color c = value;
				c.a = 0.5f;
				background.TintColor = c;

				foreground.TintColor = value;
			}
		}

		public SleekProgress(string newSuffix) : base()
		{
			background = Glazier.Get().CreateImage();
			background.SizeScale_X = 1;
			background.SizeScale_Y = 1;
			background.Texture = GlazierResources.PixelTexture;
			AddChild(background);

			foreground = Glazier.Get().CreateImage();
			foreground.SizeScale_X = 1;
			foreground.SizeScale_Y = 1;
			foreground.Texture = GlazierResources.PixelTexture;
			AddChild(foreground);

			label = Glazier.Get().CreateLabel();
			label.SizeScale_X = 1;
			label.PositionScale_Y = 0.5f;
			label.PositionOffset_Y = -15;
			label.SizeOffset_Y = 30;
			label.TextColor = Color.white;
			label.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(label);

			suffix = newSuffix;
		}
	}
}
