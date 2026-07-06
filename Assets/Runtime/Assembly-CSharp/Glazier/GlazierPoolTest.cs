////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Used in a test scene to quickly test whether pooled components are cleaned up.
	/// </summary>
	internal class GlazierPoolTest : MonoBehaviour
	{
		private void Awake()
		{
			GlazierFactory.Create();

			window = new SleekWindow();
			Glazier.Get().Root = window;

			ISleekButton createButton = Glazier.Get().CreateButton();
			createButton.PositionOffset_X = 10;
			createButton.PositionOffset_Y = 10;
			createButton.SizeOffset_X = 200;
			createButton.SizeOffset_Y = 30;
			createButton.Text = "Create";
			createButton.OnClicked += OnClickedCreateButton;
			window.AddChild(createButton);

			ISleekButton randomizeButton = Glazier.Get().CreateButton();
			randomizeButton.PositionOffset_X = 220;
			randomizeButton.PositionOffset_Y = 10;
			randomizeButton.SizeOffset_X = 200;
			randomizeButton.SizeOffset_Y = 30;
			randomizeButton.Text = "Randomize";
			randomizeButton.OnClicked += OnClickedRandomizeButton;
			window.AddChild(randomizeButton);

			ISleekButton destroyButton = Glazier.Get().CreateButton();
			destroyButton.PositionOffset_X = 430;
			destroyButton.PositionOffset_Y = 10;
			destroyButton.SizeOffset_X = 200;
			destroyButton.SizeOffset_Y = 30;
			destroyButton.Text = "Destroy";
			destroyButton.OnClicked += OnClickedDestroyButton;
			window.AddChild(destroyButton);

			container = Glazier.Get().CreateScrollView();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 50;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -60;
			container.SizeScale_X = 1.0f;
			container.SizeScale_Y = 1.0f;
			container.ScaleContentToWidth = true;
			window.AddChild(container);

			containerHeight = 0;
		}

		private void OnClickedCreateButton(ISleekElement button)
		{
			ISleekBox newBox = Glazier.Get().CreateBox();
			newBox.PositionOffset_Y = containerHeight;
			newBox.SizeOffset_X = 200;
			newBox.SizeOffset_Y = 30;
			containerHeight += newBox.SizeOffset_Y;
			boxes.Add(newBox);
			container.AddChild(newBox);

			ISleekButton newButton = Glazier.Get().CreateButton();
			newButton.PositionOffset_Y = containerHeight;
			newButton.SizeOffset_X = 200;
			newButton.SizeOffset_Y = 30;
			containerHeight += newButton.SizeOffset_Y;
			buttons.Add(newButton);
			container.AddChild(newButton);

			ISleekImage newImage = Glazier.Get().CreateImage();
			newImage.PositionOffset_Y = containerHeight;
			newImage.SizeOffset_X = 200;
			newImage.SizeOffset_X = 50;
			newImage.SizeOffset_Y = 50;
			containerHeight += newImage.SizeOffset_Y;
			images.Add(newImage);
			container.AddChild(newImage);

			ISleekLabel newLabel = Glazier.Get().CreateLabel();
			newLabel.PositionOffset_Y = containerHeight;
			newLabel.SizeOffset_X = 200;
			newLabel.SizeOffset_Y = 30;
			containerHeight += newLabel.SizeOffset_Y;
			labels.Add(newLabel);
			container.AddChild(newLabel);

			containerHeight += 10;
			container.ContentSizeOffset = new Vector2(0.0f, containerHeight);
		}

		private void RandomizeElement(ISleekElement testElement)
		{
			testElement.IsVisible = Random.value < 0.5f;
		}

		private void RandomizeLabel(ISleekLabel testLabel)
		{
			testLabel.Text = "Text " + Random.value.ToString();
			testLabel.FontStyle = (FontStyle) Random.Range(0, 4);
			testLabel.TextAlignment = (TextAnchor) Random.Range(0, 10);
			testLabel.FontSize = ESleekFontSize.Medium;
			testLabel.TextContrastContext = (ETextContrastContext) Random.Range(0, 4);
			testLabel.TextColor = Random.ColorHSV();
			testLabel.AllowRichText = Random.value < 0.5f;
		}

		private Texture LoadRandomTexture()
		{
			int index = Random.Range(1, 11);
			return Resources.Load<Texture>("Bundles/Textures/Menu/Icons/Survivors/MenuSurvivorsCharacter/Skillset_" + index);
		}

		private void OnClickedRandomizeButton(ISleekElement button)
		{
			foreach (ISleekBox testBox in boxes)
			{
				RandomizeElement(testBox);
				RandomizeLabel(testBox);

				testBox.TooltipText = "Box Tooltip " + Random.value.ToString();

				// ISleekBox
				testBox.BackgroundColor = Random.ColorHSV();
			}

			foreach (ISleekButton testButton in buttons)
			{
				RandomizeElement(testButton);
				RandomizeLabel(testButton);

				testButton.TooltipText = "Button Tooltip " + Random.value.ToString();

				// ISleekButton
				testButton.BackgroundColor = Random.ColorHSV();
				testButton.IsClickable = Random.value < 0.5f;
				testButton.IsRaycastTarget = Random.value < 0.5f;
			}

			foreach (ISleekImage testImage in images)
			{
				RandomizeElement(testImage);

				// ISleekImage
				testImage.Texture = LoadRandomTexture();
				if (Random.value < 0.5f)
				{
					testImage.RotationAngle = Random.Range(0.0f, 360.0f);
					testImage.CanRotate = true;
				}
				testImage.TintColor = Random.ColorHSV();
			}

			foreach (ISleekLabel testLabel in labels)
			{
				RandomizeElement(testLabel);
				RandomizeLabel(testLabel);
			}
		}

		private void OnClickedDestroyButton(ISleekElement button)
		{
			container.RemoveAllChildren();
			boxes.Clear();
			buttons.Clear();
			images.Clear();
			labels.Clear();
			containerHeight = 0;
		}

		private SleekWindow window;
		private ISleekScrollView container;
		private float containerHeight;

		private List<ISleekBox> boxes = new List<ISleekBox>();
		private List<ISleekButton> buttons = new List<ISleekButton>();
		private List<ISleekImage> images = new List<ISleekImage>();
		private List<ISleekLabel> labels = new List<ISleekLabel>();
	}
}
#endif // UNITY_EDITOR
