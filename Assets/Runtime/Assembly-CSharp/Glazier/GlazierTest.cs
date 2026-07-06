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
	/// Used in a test scene to quickly test all Glazier features.
	/// </summary>
	internal class GlazierTest : MonoBehaviour
	{
		private void Awake()
		{
			GlazierFactory.Create();

			window = new SleekWindow();
			Glazier.Get().Root = window;

			ISleekScrollView container = Glazier.Get().CreateScrollView();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1.0f;
			container.SizeScale_Y = 1.0f;
			container.ScaleContentToWidth = true;
			window.AddChild(container);

			int verticalOffset = 0;

			SleekButtonState themeButton = new SleekButtonState(new GUIContent("Light Theme"),
				new GUIContent("Dark Theme"));
			themeButton.PositionOffset_Y = verticalOffset;
			themeButton.SizeOffset_X = 200;
			themeButton.SizeOffset_Y = 30;
			themeButton.onSwappedState = onSwappedThemeState;
			container.AddChild(themeButton);
			verticalOffset += 40;

			ISleekBox box = Glazier.Get().CreateBox();
			box.PositionOffset_Y = verticalOffset;
			box.SizeOffset_X = 220;
			box.SizeOffset_Y = 90;
			box.TooltipText = "Box Tooltip";
			container.AddChild(box);
			verticalOffset += 100;

			ISleekLabel label = Glazier.Get().CreateLabel();
			label.PositionOffset_X = 10;
			label.PositionOffset_Y = 10;
			label.SizeOffset_X = -20;
			label.SizeOffset_Y = 30;
			label.SizeScale_X = 1.0f;
			label.Text = "Label";
			box.AddChild(label);

			ISleekButton button = Glazier.Get().CreateButton();
			button.PositionOffset_X = 10;
			button.PositionOffset_Y = 50;
			button.SizeOffset_X = -20;
			button.SizeOffset_Y = 30;
			button.SizeScale_X = 1.0f;
			button.Text = "Button";
			button.TooltipText = "Button Tooltip";
			button.OnClicked += onClickedNormalButton;
			button.OnRightClicked += onRightClickedNormalButton;
			box.AddChild(button);

			ISleekButton raycastButton = Glazier.Get().CreateButton();
			raycastButton.PositionOffset_X = 10;
			raycastButton.PositionOffset_Y = 35;
			raycastButton.PositionScale_X = 0.5f;
			raycastButton.SizeOffset_X = -20;
			raycastButton.SizeOffset_Y = 30;
			raycastButton.SizeScale_X = 1.0f;
			raycastButton.Text = "Raycast Button";
			raycastButton.TooltipText = "Raycast Button Tooltip";
			raycastButton.OnClicked += onClickedRaycastButton;
			box.AddChild(raycastButton);

			ISleekButton noRaycastButton = Glazier.Get().CreateButton();
			noRaycastButton.PositionOffset_X = 10;
			noRaycastButton.PositionOffset_Y = 65;
			noRaycastButton.PositionScale_X = 0.5f;
			noRaycastButton.SizeOffset_X = -20;
			noRaycastButton.SizeOffset_Y = 30;
			noRaycastButton.SizeScale_X = 1.0f;
			noRaycastButton.Text = "Non-Raycast Button";
			noRaycastButton.TooltipText = "Non-Raycast Button Tooltip";
			noRaycastButton.OnClicked += onClickedNonRaycastButton;
			noRaycastButton.IsRaycastTarget = false;
			box.AddChild(noRaycastButton);

			ISleekButton nonInteractiveButton = Glazier.Get().CreateButton();
			nonInteractiveButton.PositionOffset_X = 10;
			nonInteractiveButton.PositionOffset_Y = 65;
			nonInteractiveButton.PositionScale_X = 0.0f;
			nonInteractiveButton.SizeOffset_X = -20;
			nonInteractiveButton.SizeOffset_Y = 30;
			nonInteractiveButton.SizeScale_X = 0.5f;
			nonInteractiveButton.Text = "Non-Interative Button";
			nonInteractiveButton.TooltipText = "Non-Interactive Button Tooltip";
			nonInteractiveButton.OnClicked += onClickedNonRaycastButton;
			nonInteractiveButton.IsClickable = false;
			box.AddChild(nonInteractiveButton);

			ISleekBox colorfulBox = Glazier.Get().CreateBox();
			colorfulBox.PositionOffset_X = 400;
			colorfulBox.PositionOffset_Y = box.PositionOffset_Y;
			colorfulBox.SizeOffset_X = 220;
			colorfulBox.SizeOffset_Y = 90;
			colorfulBox.BackgroundColor = Color.blue;
			colorfulBox.TextColor = Color.yellow;
			colorfulBox.Text = "Colorful Box";
			colorfulBox.FontStyle = FontStyle.Bold;
			colorfulBox.TooltipText = "Colorful Box Tooltip";
			container.AddChild(colorfulBox);

			ISleekButton colorfulButton = Glazier.Get().CreateButton();
			colorfulButton.PositionOffset_X = 620;
			colorfulButton.PositionOffset_Y = box.PositionOffset_Y;
			colorfulButton.SizeOffset_X = 220;
			colorfulButton.SizeOffset_Y = 90;
			colorfulButton.BackgroundColor = Color.blue;
			colorfulButton.TextColor = Color.yellow;
			colorfulButton.Text = "Colorful Button";
			colorfulButton.FontStyle = FontStyle.Bold;
			colorfulButton.TooltipText = "Colorful Button Tooltip";
			container.AddChild(colorfulButton);

			ISleekBox verticalLayoutBox = Glazier.Get().CreateBox();
			verticalLayoutBox.PositionOffset_X = 850;
			verticalLayoutBox.PositionOffset_Y = box.PositionOffset_Y;
			verticalLayoutBox.SizeOffset_X = 200;
			verticalLayoutBox.SizeOffset_Y = 100;
			verticalLayoutBox.UseChildAutoLayout = ESleekChildLayout.Vertical;
			container.AddChild(verticalLayoutBox);

			ISleekBox verticalLayoutItemBox = Glazier.Get().CreateBox();
			verticalLayoutItemBox.UseManualLayout = false;
			verticalLayoutItemBox.Text = "Box";
			verticalLayoutItemBox.UseChildAutoLayout = ESleekChildLayout.Horizontal;
			verticalLayoutBox.AddChild(verticalLayoutItemBox);

			ISleekButton verticalLayoutItemButton = Glazier.Get().CreateButton();
			verticalLayoutItemButton.UseManualLayout = false;
			verticalLayoutItemButton.Text = "Button";
			verticalLayoutItemButton.UseChildAutoLayout = ESleekChildLayout.Horizontal;
			verticalLayoutBox.AddChild(verticalLayoutItemButton);

			ISleekLabel verticalLayoutItemLabel = Glazier.Get().CreateLabel();
			verticalLayoutItemLabel.UseManualLayout = false;
			verticalLayoutItemLabel.Text = "Label";
			verticalLayoutBox.AddChild(verticalLayoutItemLabel);

			ISleekToggle defaultFalseToggle = Glazier.Get().CreateToggle();
			defaultFalseToggle.PositionOffset_X = 0;
			defaultFalseToggle.PositionOffset_Y = verticalOffset;
			defaultFalseToggle.Value = false;
			defaultFalseToggle.OnValueChanged += onDefaultFalseToggled;
			defaultFalseToggle.TooltipText = "False Tooltip";
			container.AddChild(defaultFalseToggle);

			ISleekToggle defaultTrueToggle = Glazier.Get().CreateToggle();
			defaultTrueToggle.PositionOffset_X = 50;
			defaultTrueToggle.PositionOffset_Y = verticalOffset;
			defaultTrueToggle.Value = true;
			defaultTrueToggle.OnValueChanged += onDefaultTrueToggled;
			defaultTrueToggle.TooltipText = "True Tooltip";
			container.AddChild(defaultTrueToggle);

			ISleekToggle colorfulToggle = Glazier.Get().CreateToggle();
			colorfulToggle.PositionOffset_X = 100;
			colorfulToggle.PositionOffset_Y = verticalOffset;
			colorfulToggle.Value = true;
			colorfulToggle.TooltipText = "Colorful";
			colorfulToggle.BackgroundColor = Color.blue;
			colorfulToggle.ForegroundColor = Color.yellow;
			container.AddChild(colorfulToggle);

			ISleekToggle nonInteractiveToggle = Glazier.Get().CreateToggle();
			nonInteractiveToggle.PositionOffset_X = 150;
			nonInteractiveToggle.PositionOffset_Y = verticalOffset;
			nonInteractiveToggle.Value = true;
			nonInteractiveToggle.TooltipText = "Not Interactable";
			nonInteractiveToggle.IsInteractable = false;
			container.AddChild(nonInteractiveToggle);

			verticalOffset += 50;

			stringField = Glazier.Get().CreateStringField();
			stringField.PositionOffset_X = 100;
			stringField.PositionOffset_Y = verticalOffset;
			stringField.SizeOffset_X = 200;
			stringField.SizeOffset_Y = 30;
			stringField.PlaceholderText = "Placeholder";
			stringField.AddLabel("Left", ESleekSide.LEFT);
			stringField.AddLabel("Right", ESleekSide.RIGHT);
			stringField.TooltipText = "Field Tooltip";
			stringField.OnTextChanged += OnTyped;
			stringField.OnTextSubmitted += OnEntered;
			stringField.OnTextEscaped += OnEscaped;
			container.AddChild(stringField);
			verticalOffset += 40;

			ISleekButton focusStringFieldButton = Glazier.Get().CreateButton();
			focusStringFieldButton.PositionOffset_X = 400;
			focusStringFieldButton.PositionOffset_Y = stringField.PositionOffset_Y;
			focusStringFieldButton.SizeOffset_X = 100;
			focusStringFieldButton.SizeOffset_Y = 30;
			focusStringFieldButton.Text = "Focus";
			focusStringFieldButton.TooltipText = "Focus string field";
			focusStringFieldButton.OnClicked += OnFocusStringFieldClicked;
			container.AddChild(focusStringFieldButton);

			ISleekField passwordField = Glazier.Get().CreateStringField();
			passwordField.PositionOffset_X = 100;
			passwordField.PositionOffset_Y = verticalOffset;
			passwordField.SizeOffset_X = 200;
			passwordField.SizeOffset_Y = 30;
			passwordField.IsPasswordField = true;
			passwordField.Text = "Default Text";
			passwordField.AddLabel("Password", ESleekSide.LEFT);
			passwordField.TooltipText = "Password Field Tooltip";
			container.AddChild(passwordField);
			verticalOffset += 40;

			ISleekField multilineField = Glazier.Get().CreateStringField();
			multilineField.PositionOffset_X = 100;
			multilineField.PositionOffset_Y = verticalOffset;
			multilineField.SizeOffset_X = 200;
			multilineField.SizeOffset_Y = 70;
			multilineField.IsMultiline = true;
			multilineField.Text = "First Line\nSecond Line";
			multilineField.TooltipText = "Multiline Field Tooltip";
			container.AddChild(multilineField);
			verticalOffset += 80;

			ISleekFloat32Field float32Field = Glazier.Get().CreateFloat32Field();
			float32Field.PositionOffset_X = 100;
			float32Field.PositionOffset_Y = verticalOffset;
			float32Field.SizeOffset_X = 200;
			float32Field.SizeOffset_Y = 30;
			float32Field.Value = 2.5f;
			float32Field.AddLabel("Float32:", ESleekSide.LEFT);
			float32Field.OnValueChanged += onTypedFloat32;
			container.AddChild(float32Field);
			verticalOffset += 40;

			ISleekInt32Field int32Field = Glazier.Get().CreateInt32Field();
			int32Field.PositionOffset_X = 100;
			int32Field.PositionOffset_Y = verticalOffset;
			int32Field.SizeOffset_X = 200;
			int32Field.SizeOffset_Y = 30;
			int32Field.Value = -32;
			int32Field.AddLabel("Int32:", ESleekSide.LEFT);
			int32Field.OnValueChanged += onTypedInt32;
			container.AddChild(int32Field);
			verticalOffset += 40;

			ISleekUInt16Field uint16Field = Glazier.Get().CreateUInt16Field();
			uint16Field.PositionOffset_X = 100;
			uint16Field.PositionOffset_Y = verticalOffset;
			uint16Field.SizeOffset_X = 200;
			uint16Field.SizeOffset_Y = 30;
			uint16Field.Value = 16;
			uint16Field.AddLabel("UInt16:", ESleekSide.LEFT);
			uint16Field.OnValueChanged += onTypedUInt16;
			container.AddChild(uint16Field);
			verticalOffset += 40;

			ISleekUInt32Field uint32Field = Glazier.Get().CreateUInt32Field();
			uint32Field.PositionOffset_X = 100;
			uint32Field.PositionOffset_Y = verticalOffset;
			uint32Field.SizeOffset_X = 200;
			uint32Field.SizeOffset_Y = 30;
			uint32Field.Value = 32;
			uint32Field.AddLabel("UInt32:", ESleekSide.LEFT);
			uint32Field.OnValueChanged += onTypedUInt32;
			container.AddChild(uint32Field);
			verticalOffset += 40;

			ISleekFloat64Field float64Field = Glazier.Get().CreateFloat64Field();
			float64Field.PositionOffset_X = 100;
			float64Field.PositionOffset_Y = verticalOffset;
			float64Field.SizeOffset_X = 200;
			float64Field.SizeOffset_Y = 30;
			float64Field.Value = 5.1f;
			float64Field.AddLabel("Float64:", ESleekSide.LEFT);
			float64Field.OnValueChanged += onTypedFloat64;
			container.AddChild(float64Field);
			verticalOffset += 40;

			ISleekUInt8Field uint8Field = Glazier.Get().CreateUInt8Field();
			uint8Field.PositionOffset_X = 100;
			uint8Field.PositionOffset_Y = verticalOffset;
			uint8Field.SizeOffset_X = 200;
			uint8Field.SizeOffset_Y = 30;
			uint8Field.Value = 8;
			uint8Field.AddLabel("UInt8:", ESleekSide.LEFT);
			uint8Field.OnValueChanged += onTypedUInt8;
			container.AddChild(uint8Field);
			verticalOffset += 40;

			ISleekSlider verticalSlider = Glazier.Get().CreateSlider();
			verticalSlider.PositionOffset_Y = verticalOffset;
			verticalSlider.SizeOffset_X = 20;
			verticalSlider.SizeOffset_Y = 200;
			verticalSlider.OnValueChanged += onDraggedVerticalSlider;
			container.AddChild(verticalSlider);
			ISleekSlider horizontalSlider = Glazier.Get().CreateSlider();
			horizontalSlider.PositionOffset_X = 30;
			horizontalSlider.PositionOffset_Y = verticalOffset;
			horizontalSlider.SizeOffset_X = 200;
			horizontalSlider.SizeOffset_Y = 20;
			horizontalSlider.OnValueChanged += onDraggedHorizontalSlider;
			horizontalSlider.Orientation = ESleekOrientation.HORIZONTAL;
			container.AddChild(horizontalSlider);

			ISleekScrollView scrollView2D = Glazier.Get().CreateScrollView();
			scrollView2D.PositionOffset_X = 240;
			scrollView2D.PositionOffset_Y = verticalOffset;
			scrollView2D.SizeOffset_X = 200;
			scrollView2D.SizeOffset_Y = 200;
			scrollView2D.ScaleContentToWidth = true;
			scrollView2D.ScaleContentToHeight = true;
			scrollView2D.ContentScaleFactor = 2.0f;
			scrollView2D.HandleScrollWheel = false;
			container.AddChild(scrollView2D);
			verticalOffset += 210;

			ISleekBox mapBox = Glazier.Get().CreateBox();
			mapBox.SizeScale_X = 1.0f;
			mapBox.SizeScale_Y = 1.0f;
			mapBox.Text = "This is a map";
			scrollView2D.AddChild(mapBox);

			ISleekScrollView scrollView = Glazier.Get().CreateScrollView();
			scrollView.PositionOffset_Y = verticalOffset;
			scrollView.SizeOffset_X = 200;
			scrollView.SizeOffset_Y = 200;
			container.AddChild(scrollView);

			for (int scrollViewItemIndex = 0; scrollViewItemIndex < 20; ++scrollViewItemIndex)
			{
				ISleekBox item = Glazier.Get().CreateBox();
				item.PositionOffset_Y = scrollViewItemIndex * 35;
				item.SizeScale_X = 1.0f;
				item.SizeOffset_Y = 30;
				item.Text = "Item #" + (scrollViewItemIndex + 1);
				scrollView.AddChild(item);
			}
			scrollView.ScaleContentToWidth = true;
			scrollView.ContentSizeOffset = new Vector2(0.0f, (20 * 35.0f) - 5.0f);

			verticalOffset += 210;

			ISleekScrollView verticalLayoutScrollView = Glazier.Get().CreateScrollView();
			verticalLayoutScrollView.PositionOffset_Y = verticalOffset;
			verticalLayoutScrollView.SizeOffset_X = 200;
			verticalLayoutScrollView.SizeOffset_Y = 200;
			verticalLayoutScrollView.ContentUseManualLayout = false;
			container.AddChild(verticalLayoutScrollView);

			for (int scrollViewItemIndex = 0; scrollViewItemIndex < 20; ++scrollViewItemIndex)
			{
				ISleekBox autoLayoutBox = Glazier.Get().CreateBox();
				autoLayoutBox.UseManualLayout = false;
				autoLayoutBox.UseChildAutoLayout = ESleekChildLayout.Vertical;
				verticalLayoutScrollView.AddChild(autoLayoutBox);

				ISleekLabel item = Glazier.Get().CreateLabel();
				item.Text = "Vertical Layout Item #" + (scrollViewItemIndex + 1);
				item.UseManualLayout = false;
				autoLayoutBox.AddChild(item);

				ISleekButton autolayoutButton = Glazier.Get().CreateButton();
				autolayoutButton.Text = "Button #" + (scrollViewItemIndex + 1);
				autolayoutButton.UseManualLayout = false;
				autolayoutButton.UseChildAutoLayout = ESleekChildLayout.Horizontal;
				autoLayoutBox.AddChild(autolayoutButton);

				ISleekImage autoLayoutImage = Glazier.Get().CreateImage();
				autoLayoutImage.Texture = Resources.Load<Texture2D>("Bundles/Textures/Menu/Icons/MenuDashboard/Clipboard");
				autoLayoutImage.SizeOffset_X = 40;
				autoLayoutImage.SizeOffset_Y = 40;
				autoLayoutImage.UseManualLayout = false;
				autoLayoutImage.UseWidthLayoutOverride = true;
				autoLayoutImage.UseHeightLayoutOverride = true;
				autoLayoutBox.AddChild(autoLayoutImage);
			}
			verticalLayoutScrollView.ScaleContentToWidth = true;

			verticalOffset += 210;

			ISleekScrollView chatScrollView = Glazier.Get().CreateScrollView();
			chatScrollView.PositionOffset_Y = verticalOffset;
			chatScrollView.SizeOffset_X = 200;
			chatScrollView.SizeOffset_Y = 200;
			chatScrollView.ContentUseManualLayout = false;
			chatScrollView.ScaleContentToWidth = true;
			chatScrollView.AlignContentToBottom = true;
			container.AddChild(chatScrollView);

			List<ReceivedChatMessage> chatMessages = new List<ReceivedChatMessage>();
			chatMessages.Add(new ReceivedChatMessage()
			{
				mode = EChatMode.GLOBAL,
				contents = "Hello, world!",
				color = Color.white,
			});
			chatMessages.Add(new ReceivedChatMessage()
			{
				mode = EChatMode.GLOBAL,
				contents = "<b>Rich</b> <i>text</i> <color=red>red</color>",
				color = Color.white,
				useRichTextFormatting = true,
			});
			chatMessages.Add(new ReceivedChatMessage()
			{
				mode = EChatMode.GLOBAL,
				contents = "Line #1\nLine #2\nLine #3",
				color = Color.white,
			});

			foreach (ReceivedChatMessage message in chatMessages)
			{
				SleekChatEntryV2 chatEntryV2 = new SleekChatEntryV2();
				chatEntryV2.representingChatMessage = message;
				chatScrollView.AddChild(chatEntryV2);
			}
			
			verticalOffset += 210;

			List<int> intList = new List<int>()
			{
				1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765,
			};

			List<ListItem> refTypesList = new List<ListItem>(intList.Count);
			foreach (int value in intList)
			{
				refTypesList.Add(new ListItem(value));
			}

			SleekList<ListItem> list = new SleekList<ListItem>();
			list.PositionOffset_Y = verticalOffset;
			list.SizeOffset_X = 200;
			list.SizeOffset_Y = 200;
			list.itemHeight = 40;
			list.itemPadding = 15;
			list.onCreateElement = onCreateListElement;
			list.SetData(refTypesList);
			container.AddChild(list);
			verticalOffset += 210;

			ISleekButton createButton = Glazier.Get().CreateButton();
			createButton.PositionOffset_Y = verticalOffset;
			createButton.SizeOffset_X = 200;
			createButton.SizeOffset_Y = 30;
			createButton.Text = "Create 1000 Buttons";
			createButton.OnClicked += onClickedCreateButton;
			container.AddChild(createButton);
			verticalOffset += 30;

			ISleekButton destroyButton = Glazier.Get().CreateButton();
			destroyButton.PositionOffset_Y = verticalOffset;
			destroyButton.SizeOffset_X = 200;
			destroyButton.SizeOffset_Y = 30;
			destroyButton.Text = "Destroy 1000 Buttons";
			destroyButton.OnClicked += onClickedDestroyButton;
			container.AddChild(destroyButton);
			verticalOffset += 40;

			image = Glazier.Get().CreateImage();
			image.PositionOffset_Y = verticalOffset;
			image.SizeOffset_X = 20;
			image.SizeOffset_Y = 20;
			image.OnClicked += onClickedImage;
			image.Texture = Resources.Load<Texture2D>("Reputation/Neutral");
			container.AddChild(image);

			ISleekLabel redImagelabel = Glazier.Get().CreateLabel();
			redImagelabel.PositionOffset_X = image.PositionOffset_X + 30;
			redImagelabel.PositionOffset_Y = image.PositionOffset_Y;
			redImagelabel.SizeOffset_X = 100;
			redImagelabel.SizeOffset_Y = 30;
			redImagelabel.Text = "Red image:";
			redImagelabel.TextColor = Color.red;
			container.AddChild(redImagelabel);

			ISleekImage redImage = Glazier.Get().CreateImage();
			redImage.PositionOffset_X = redImagelabel.PositionOffset_X + redImagelabel.SizeOffset_X + 10;
			redImage.PositionOffset_Y = redImagelabel.PositionOffset_Y;
			redImage.SizeOffset_X = image.SizeOffset_X;
			redImage.SizeOffset_Y = image.SizeOffset_Y;
			redImage.Texture = image.Texture;
			redImage.TintColor = Color.red;
			container.AddChild(redImage);

			verticalOffset += 30;

			ISleekImage stretchedImage = Glazier.Get().CreateImage();
			stretchedImage.PositionOffset_Y = verticalOffset;
			stretchedImage.SizeOffset_X = 40;
			stretchedImage.SizeOffset_Y = 20;
			stretchedImage.Texture = image.Texture;
			container.AddChild(stretchedImage);
			verticalOffset += 30;

			ISleekImage angledImage = Glazier.Get().CreateImage();
			angledImage.PositionOffset_Y = verticalOffset;
			angledImage.SizeOffset_X = 20;
			angledImage.SizeOffset_Y = 20;
			angledImage.Texture = image.Texture;
			angledImage.CanRotate = true;
			angledImage.RotationAngle = 30.0f;
			container.AddChild(angledImage);
			verticalOffset += 30;

			rotatingImage = Glazier.Get().CreateImage();
			rotatingImage.PositionOffset_X = angledImage.PositionOffset_X + 30;
			rotatingImage.PositionOffset_Y = angledImage.PositionOffset_Y;
			rotatingImage.SizeOffset_X = 20;
			rotatingImage.SizeOffset_Y = 20;
			rotatingImage.Texture = image.Texture;
			rotatingImage.CanRotate = true;
			container.AddChild(rotatingImage);

			ISleekSprite tiledSprite = Glazier.Get().CreateSprite();
			tiledSprite.PositionOffset_X = image.PositionOffset_X + 300;
			tiledSprite.PositionOffset_Y = image.PositionOffset_Y;
			tiledSprite.SizeOffset_X = 200;
			tiledSprite.SizeOffset_Y = 100;
			tiledSprite.Sprite = Resources.Load<Sprite>("Bundles/Textures/Player/Icons/PlayerDashboardInventory/Grid_Sprite");
			tiledSprite.DrawMethod = ESleekSpriteType.Tiled;
			tiledSprite.TileRepeatHintForUITK = new Vector2Int(4, 2);
			container.AddChild(tiledSprite);

			ISleekSprite slicedSprite = Glazier.Get().CreateSprite();
			slicedSprite.PositionOffset_X = image.PositionOffset_X + 500;
			slicedSprite.PositionOffset_Y = image.PositionOffset_Y;
			slicedSprite.SizeOffset_X = 200;
			slicedSprite.SizeOffset_Y = 100;
			slicedSprite.Sprite = Resources.Load<Sprite>("Bundles/Textures/Player/Icons/PlayerDashboardInventory/Slot_Sprite");
			slicedSprite.DrawMethod = ESleekSpriteType.Sliced;
			container.AddChild(slicedSprite);

			ISleekImage invisibleImage = Glazier.Get().CreateImage();
			invisibleImage.PositionOffset_X = 100;
			invisibleImage.PositionOffset_Y = verticalOffset;
			invisibleImage.SizeOffset_X = 20;
			invisibleImage.SizeOffset_Y = 30;
			invisibleImage.OnClicked += onClickedInvisibleImage;
			invisibleImage.Texture = null;
			invisibleImage.AddLabel("Invisible: [", ESleekSide.LEFT);
			invisibleImage.AddLabel("]", ESleekSide.RIGHT);
			container.AddChild(invisibleImage);
			verticalOffset += 40;

			ISleekLabel richTextLabel = Glazier.Get().CreateLabel();
			richTextLabel.PositionOffset_Y = verticalOffset;
			richTextLabel.SizeOffset_X = 200;
			richTextLabel.SizeOffset_Y = 100;
			richTextLabel.Text = "Rich Text\n<b>Bold</b>\n<i>Italic</i>";
			richTextLabel.AllowRichText = true;
			richTextLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			richTextLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			container.AddChild(richTextLabel);
			testFontLabels.Add(richTextLabel);
			verticalOffset += 110;

			ISleekBox testFontBox = Glazier.Get().CreateBox();
			testFontBox.PositionOffset_Y = verticalOffset;
			testFontBox.SizeScale_X = 1.0f;
			testFontBox.SizeOffset_Y = 180;
			container.AddChild(testFontBox);
			verticalOffset += 190;

			ESleekFontSize[] testFontSizes = new ESleekFontSize[]
			{
				ESleekFontSize.Tiny,
				ESleekFontSize.Small,
				ESleekFontSize.Default,
				ESleekFontSize.Medium,
				ESleekFontSize.Large,
			};

			string[] testFontStrings = new string[]
			{
				"Hello World",
				"你好，世界", // Chinese
				"こんにちは世界", // Japanese
				"안녕하세요 세계", // Korean
				"สวัสดีชาวโลก", // Thai
			};

			float testFontWidth = 1.0f / testFontStrings.Length;
			float testFontHeight = 1.0f / testFontSizes.Length;
			for (int stringIndex = 0; stringIndex < testFontStrings.Length; ++stringIndex)
			{
				for (int sizeIndex = 0; sizeIndex < testFontSizes.Length; ++sizeIndex)
				{
					ISleekLabel testFontLabel = Glazier.Get().CreateLabel();
					testFontLabel.PositionScale_X = stringIndex * testFontWidth;
					testFontLabel.PositionScale_Y = sizeIndex * testFontHeight;
					testFontLabel.SizeScale_X = testFontWidth;
					testFontLabel.SizeScale_Y = testFontHeight;
					testFontLabel.Text = testFontStrings[stringIndex];
					testFontLabel.FontSize = testFontSizes[sizeIndex];
					testFontBox.AddChild(testFontLabel);
					testFontLabels.Add(testFontLabel);
				}
			}

			SleekButtonState shadowButton = new SleekButtonState(new GUIContent("DefaultBackdrop"),
				new GUIContent("InconspicuousBackdrop"),
				new GUIContent("ColorfulBackdrop"),
				new GUIContent("Tooltip"));
			shadowButton.PositionOffset_X = -200;
			shadowButton.PositionOffset_Y = -30;
			shadowButton.PositionScale_X = 1.0f;
			shadowButton.SizeOffset_X = 200;
			shadowButton.SizeOffset_Y = 30;
			shadowButton.onSwappedState = onSwappedTestFontState;
			testFontBox.AddChild(shadowButton);

			ISleekLabel opaqueRichTextLabel = Glazier.Get().CreateLabel();
			opaqueRichTextLabel.PositionOffset_Y = verticalOffset;
			opaqueRichTextLabel.SizeOffset_X = 300;
			opaqueRichTextLabel.SizeOffset_Y = 30;
			opaqueRichTextLabel.Text = "Opaque rich text colored label: <color=red>Red</color>";
			opaqueRichTextLabel.AllowRichText = true;
			opaqueRichTextLabel.TextColor = Color.white;
			opaqueRichTextLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			container.AddChild(opaqueRichTextLabel);
			verticalOffset += 30;

			ISleekLabel transparentRichTextLabel = Glazier.Get().CreateLabel();
			transparentRichTextLabel.PositionOffset_Y = verticalOffset;
			transparentRichTextLabel.SizeOffset_X = 300;
			transparentRichTextLabel.SizeOffset_Y = 30;
			transparentRichTextLabel.Text = "Nearly transparent rich text colored label: <color=red>Red</color>";
			transparentRichTextLabel.AllowRichText = true;
			transparentRichTextLabel.TextColor = new Color(1.0f, 1.0f, 1.0f, 0.2f);
			transparentRichTextLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
			container.AddChild(transparentRichTextLabel);
			verticalOffset += 30;

			container.ContentSizeOffset = new Vector2(0.0f, verticalOffset - 10);
		}

		private void OnFocusStringFieldClicked(ISleekElement button)
		{
			stringField.FocusControl();
		}

		private void onClickedNormalButton(ISleekElement button)
		{
			UnturnedLog.info("Clicked normal button");
		}

		private void onRightClickedNormalButton(ISleekElement button)
		{
			UnturnedLog.info("Right-clicked normal button");
		}

		private void onClickedRaycastButton(ISleekElement button)
		{
			UnturnedLog.info("Clicked raycast button");
		}

		private void onClickedNonRaycastButton(ISleekElement button)
		{
			UnturnedLog.warn("Clicked non-raycast button");
		}

		private void onDefaultFalseToggled(ISleekToggle toggle, bool state)
		{
			UnturnedLog.info("Default false toggle state changed {0}", state);
		}

		private void onDefaultTrueToggled(ISleekToggle toggle, bool state)
		{
			UnturnedLog.info("Default true toggle state changed {0}", state);
		}

		private void OnTyped(ISleekField field, string value)
		{
			UnturnedLog.info("Typed {0}", value);
		}

		private void OnEntered(ISleekField field)
		{
			UnturnedLog.info("Entered {0}", field.Text);
		}

		private void OnEscaped(ISleekField field)
		{
			UnturnedLog.info("Escaped field");
		}

		private void onTypedFloat32(ISleekFloat32Field field, float state)
		{
			UnturnedLog.info("Float32 state changed {0}", state);
		}

		private void onTypedInt32(ISleekInt32Field field, int state)
		{
			UnturnedLog.info("Int32 state changed {0}", state);
		}

		private void onTypedUInt16(ISleekUInt16Field field, ushort state)
		{
			UnturnedLog.info("UInt16 state changed {0}", state);
		}

		private void onTypedUInt32(ISleekUInt32Field field, uint state)
		{
			UnturnedLog.info("UInt32 state changed {0}", state);
		}

		private void onTypedFloat64(ISleekFloat64Field field, double state)
		{
			UnturnedLog.info("Float64 state changed {0}", state);
		}

		private void onTypedUInt8(ISleekUInt8Field field, byte state)
		{
			UnturnedLog.info("UInt8 state changed {0}", state);
		}

		private void onDraggedVerticalSlider(ISleekSlider slider, float state)
		{
			UnturnedLog.info("Vertical slider state changed {0} (top should be 0, bottom should be 1)", state);
		}

		private void onDraggedHorizontalSlider(ISleekSlider slider, float state)
		{
			UnturnedLog.info("Horizontal slider state changed {0} (left should be 0, right should be 1)", state);
		}

		private void onClickedImage()
		{
			Vector2 cursorPosition = image.GetNormalizedCursorPosition();
			UnturnedLog.info("Clicked image @ {0}", cursorPosition);
		}

		private void onClickedInvisibleImage()
		{
			UnturnedLog.info("Clicked invisible image");
		}

		private List<ISleekButton> buttons = new List<ISleekButton>();

		private void onClickedCreateButton(ISleekElement clickedButton)
		{
			for (int i = 0; i < 1000; ++i)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionScale_X = Random.value * 0.5f;
				button.PositionScale_Y = Random.value * 0.5f;
				button.Text = i.ToString();
				button.SizeOffset_X = 100;
				button.SizeOffset_Y = 100;
				window.AddChild(button);
				buttons.Add(button);
			}
		}

		private void onClickedDestroyButton(ISleekElement clickedButton)
		{
			foreach (ISleekButton button in buttons)
			{
				window.RemoveChild(button);
			}
			buttons.Clear();
		}

		private ISleekElement onCreateListElement(ListItem item)
		{
			ISleekBox box = Glazier.Get().CreateBox();
			box.Text = item.value.ToString("D");
			return box;
		}

		/// <summary>
		/// Reference type for testing SleekList.
		/// </summary>
		private class ListItem
		{
			public ListItem(int value)
			{
				this.value = value;
			}

			public int value;
		}

		private ISleekField stringField;
		private ISleekImage image;
		private ISleekImage rotatingImage;

		private void Update()
		{
			rotatingImage.RotationAngle = Time.time * 15.0f;
		}

		private void onSwappedThemeState(SleekButtonState button, int index)
		{
			OptionsSettings.proUI = index > 0;
		}

		private void onSwappedTestFontState(SleekButtonState button, int index)
		{
			foreach (ISleekLabel label in testFontLabels)
			{
				label.TextContrastContext = (ETextContrastContext) index;
			}
		}

		private List<ISleekLabel> testFontLabels = new List<ISleekLabel>();
		private SleekWindow window;
	}
}
#endif // UNITY_EDITOR
