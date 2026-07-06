////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ColorPicked(SleekColorPicker paint, Color color);

	public class SleekColorPicker : SleekWrapper
	{
		public ColorPicked onColorPicked;

		public Color state
		{
			get
			{
				ValidateNotDestroyed();
				return color;
			}

			set
			{
				ValidateNotDestroyed();
				color = value;

				updateColor();
				updateColorText();
				updateColorSlider();
			}
		}

		private void updateColor()
		{
			colorImage.TintColor = color;
		}

		private void updateColorText()
		{
			rField.Value = (byte) (color.r * 255);
			gField.Value = (byte) (color.g * 255);
			bField.Value = (byte) (color.b * 255);
			aField.Value = (byte) (color.a * 255);
		}

		private void updateColorSlider()
		{
			rSlider.Value = color.r;
			gSlider.Value = color.g;
			bSlider.Value = color.b;
			aSlider.Value = color.a;
		}

		private void onTypedRField(ISleekUInt8Field field, byte value)
		{
			color.r = value / 255f;
			updateColor();
			updateColorSlider();

			onColorPicked?.Invoke(this, color);
		}

		private void onTypedGField(ISleekUInt8Field field, byte value)
		{
			color.g = value / 255f;
			updateColor();
			updateColorSlider();

			onColorPicked?.Invoke(this, color);
		}

		private void onTypedBField(ISleekUInt8Field field, byte value)
		{
			color.b = value / 255f;
			updateColor();
			updateColorSlider();

			onColorPicked?.Invoke(this, color);
		}

		private void onTypedAField(ISleekUInt8Field field, byte value)
		{
			color.a = value / 255f;
			updateColor();
			updateColorSlider();

			onColorPicked?.Invoke(this, color);
		}

		private void onDraggedRSlider(ISleekSlider slider, float state)
		{
			color.r = state;
			updateColor();
			updateColorText();

			onColorPicked?.Invoke(this, color);
		}

		private void onDraggedGSlider(ISleekSlider slider, float state)
		{
			color.g = state;
			updateColor();
			updateColorText();

			onColorPicked?.Invoke(this, color);
		}

		private void onDraggedBSlider(ISleekSlider slider, float state)
		{
			color.b = state;
			updateColor();
			updateColorText();

			onColorPicked?.Invoke(this, color);
		}

		private void onDraggedASlider(ISleekSlider slider, float state)
		{
			color.a = state;
			updateColor();
			updateColorText();

			onColorPicked?.Invoke(this, color);
		}

		public void SetAllowAlpha(bool allowAlpha)
		{
			aField.IsVisible = allowAlpha;
			aSlider.IsVisible = allowAlpha;

			if (allowAlpha)
			{
				SizeOffset_Y = 150;

				rField.SizeOffset_X = 50;
				gField.PositionOffset_X = rField.PositionOffset_X + rField.SizeOffset_X;
				gField.SizeOffset_X = 50;
				bField.PositionOffset_X = gField.PositionOffset_X + gField.SizeOffset_X;
				bField.SizeOffset_X = 50;
				aField.PositionOffset_X = bField.PositionOffset_X + bField.SizeOffset_X;
			}
			else
			{
				SizeOffset_Y = 120;

				rField.SizeOffset_X = 60;
				gField.PositionOffset_X = rField.PositionOffset_X + rField.SizeOffset_X + 10;
				gField.SizeOffset_X = 60;
				bField.PositionOffset_X = gField.PositionOffset_X + gField.SizeOffset_X + 10;
				bField.SizeOffset_X = 60;
			}
		}

		public SleekColorPicker() : base()
		{
			color = Color.black;

			SizeOffset_X = 240;

			colorImage = Glazier.Get().CreateImage();
			colorImage.SizeOffset_X = 30;
			colorImage.SizeOffset_Y = 30;
			colorImage.Texture = GlazierResources.PixelTexture;
			AddChild(colorImage);

			rField = Glazier.Get().CreateUInt8Field();
			rField.PositionOffset_X = 40;
			rField.SizeOffset_Y = 30;
			rField.TextColor = Palette.COLOR_R;
			rField.OnValueChanged += onTypedRField;
			AddChild(rField);

			gField = Glazier.Get().CreateUInt8Field();
			gField.SizeOffset_Y = 30;
			gField.TextColor = Palette.COLOR_G;
			gField.OnValueChanged += onTypedGField;
			AddChild(gField);

			bField = Glazier.Get().CreateUInt8Field();
			bField.SizeOffset_Y = 30;
			bField.TextColor = Palette.COLOR_B;
			bField.OnValueChanged += onTypedBField;
			AddChild(bField);

			aField = Glazier.Get().CreateUInt8Field();
			aField.SizeOffset_X = 50;
			aField.SizeOffset_Y = 30;
			aField.TextColor = Palette.COLOR_W;
			aField.OnValueChanged += onTypedAField;
			aField.IsVisible = false;
			AddChild(aField);

			rSlider = Glazier.Get().CreateSlider();
			rSlider.PositionOffset_X = 40;
			rSlider.PositionOffset_Y = 40;
			rSlider.SizeOffset_X = 200;
			rSlider.SizeOffset_Y = 20;
			rSlider.Orientation = ESleekOrientation.HORIZONTAL;
			rSlider.AddLabel("R", Palette.COLOR_R, ESleekSide.LEFT);
			rSlider.OnValueChanged += onDraggedRSlider;
			AddChild(rSlider);

			gSlider = Glazier.Get().CreateSlider();
			gSlider.PositionOffset_X = 40;
			gSlider.PositionOffset_Y = 70;
			gSlider.SizeOffset_X = 200;
			gSlider.SizeOffset_Y = 20;
			gSlider.Orientation = ESleekOrientation.HORIZONTAL;
			gSlider.AddLabel("G", Palette.COLOR_G, ESleekSide.LEFT);
			gSlider.OnValueChanged += onDraggedGSlider;
			AddChild(gSlider);

			bSlider = Glazier.Get().CreateSlider();
			bSlider.PositionOffset_X = 40;
			bSlider.PositionOffset_Y = 100;
			bSlider.SizeOffset_X = 200;
			bSlider.SizeOffset_Y = 20;
			bSlider.Orientation = ESleekOrientation.HORIZONTAL;
			bSlider.AddLabel("B", Palette.COLOR_B, ESleekSide.LEFT);
			bSlider.OnValueChanged += onDraggedBSlider;
			AddChild(bSlider);

			aSlider = Glazier.Get().CreateSlider();
			aSlider.PositionOffset_X = 40;
			aSlider.PositionOffset_Y = 130;
			aSlider.SizeOffset_X = 200;
			aSlider.SizeOffset_Y = 20;
			aSlider.Orientation = ESleekOrientation.HORIZONTAL;
			aSlider.AddLabel("A", Palette.COLOR_W, ESleekSide.LEFT);
			aSlider.OnValueChanged += onDraggedASlider;
			aSlider.IsVisible = false;
			AddChild(aSlider);

			SetAllowAlpha(false);
		}

		private ISleekImage colorImage;
		private ISleekUInt8Field rField;
		private ISleekUInt8Field gField;
		private ISleekUInt8Field bField;
		private ISleekUInt8Field aField;
		private ISleekSlider rSlider;
		private ISleekSlider gSlider;
		private ISleekSlider bSlider;
		private ISleekSlider aSlider;

		private Color color;
	}
}
