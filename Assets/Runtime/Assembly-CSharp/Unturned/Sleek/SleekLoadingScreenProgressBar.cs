////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class SleekLoadingScreenProgressBar : SleekWrapper
	{
		private float _progressPercentage;
		public float ProgressPercentage
		{
			get => _progressPercentage;
			set
			{
				_progressPercentage = value;
				foregroundImage.PositionScale_X = value;
				foregroundImage.SizeScale_X = 1.0f - value;
				percentageLabel.Text = value.ToString("P");
			}
		}

		public string DescriptionText
		{
			get => label.Text;
			set => label.Text = value;
		}

		public SleekLoadingScreenProgressBar() : base()
		{
			backgroundImage = Glazier.Get().CreateImage();
			backgroundImage.SizeScale_X = 1.0f;
			backgroundImage.SizeScale_Y = 1.0f;
			backgroundImage.Texture = GlazierResources.PixelTexture;
			backgroundImage.TintColor = ESleekTint.FOREGROUND;
			AddChild(backgroundImage);

			foregroundImage = Glazier.Get().CreateImage();
			foregroundImage.SizeScale_X = 1.0f;
			foregroundImage.SizeScale_Y = 1.0f;
			foregroundImage.Texture = GlazierResources.PixelTexture;
			foregroundImage.TintColor = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.75f);
			backgroundImage.AddChild(foregroundImage);

			label = Glazier.Get().CreateLabel();
			label.PositionOffset_X = 10;
			label.PositionOffset_Y = -15;
			label.PositionScale_Y = 0.5f;
			label.SizeOffset_X = -20;
			label.SizeOffset_Y = 30;
			label.SizeScale_X = 1.0f;
			label.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(label);

			percentageLabel = Glazier.Get().CreateLabel();
			percentageLabel.PositionOffset_X = -100;
			percentageLabel.PositionOffset_Y = -15;
			percentageLabel.PositionScale_X = 1.0f;
			percentageLabel.PositionScale_Y = 0.5f;
			percentageLabel.SizeOffset_X = 100;
			percentageLabel.SizeOffset_Y = 30;
			percentageLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
			AddChild(percentageLabel);
		}

		private ISleekImage backgroundImage;
		private ISleekImage foregroundImage;
		private ISleekLabel label;
		private ISleekLabel percentageLabel;
	}
}
