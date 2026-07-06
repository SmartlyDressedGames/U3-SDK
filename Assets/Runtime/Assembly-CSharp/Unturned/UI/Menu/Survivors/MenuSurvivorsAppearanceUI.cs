////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuSurvivorsAppearanceUI
	{
		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon backButton;

		private static ISleekScrollView customizationBox;

		private static ISleekBox faceBox;
		private static ISleekBox hairBox;
		private static ISleekBox beardBox;

		private static ISleekButton[] faceButtons;
		private static ISleekButton[] hairButtons;
		private static ISleekButton[] beardButtons;

		private static ISleekBox skinBox;
		private static ISleekBox hairColorBox;
		private static ISleekBox beardColorBox;

		private static ISleekButton[] skinButtons;
		private static ISleekButton[] colorButtons;

		private static SleekColorPicker skinColorPicker;
		private static SleekColorPicker hairColorPicker;
		private static SleekColorPicker beardColorPicker;

		private static SleekButtonState handState;
		private static ISleekSlider characterSlider;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			Characters.RefreshPreviewCharacterModel();

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			Characters.RefreshPreviewCharacterModel();

			container.AnimateOutOfView(0, 1);
		}

		private static void updateFaces(Color color)
		{
			for (int index = 0; index < faceButtons.Length; index++)
			{
				((ISleekImage) faceButtons[index].GetChildAtIndex(0)).TintColor = color;
			}
		}

		private static void UpdateHairButtonTint(Color color)
		{
			for (int index = 1; index < hairButtons.Length; index++)
			{
				((ISleekImage) hairButtons[index].GetChildAtIndex(0)).TintColor = color;
			}
		}

		private static void UpdateBeardButtonTint(Color color)
		{
			for (int index = 1; index < beardButtons.Length; index++)
			{
				((ISleekImage) beardButtons[index].GetChildAtIndex(0)).TintColor = color;
			}
		}

		private static void onCharacterUpdated(byte index, Character character)
		{
			if (index == Characters.selected)
			{
				skinColorPicker.state = character.skin;
				hairColorPicker.state = character.color;
				beardColorPicker.state = character.BeardColor;

				updateFaces(character.skin);
				UpdateHairButtonTint(character.color);
				UpdateBeardButtonTint(character.BeardColor);

				handState.state = character.hand ? 1 : 0;
			}
		}

		private static void onClickedFaceButton(ISleekElement button)
		{
			int index = Mathf.FloorToInt((button.PositionOffset_X / 50) + ((button.PositionOffset_Y - 40) / 50 * 5));

			Characters.growFace((byte) index);
		}

		private static void onClickedHairButton(ISleekElement button)
		{
			int index = Mathf.FloorToInt((button.PositionOffset_X / 50) + ((button.PositionOffset_Y - 40) / 50 * 5));

			Characters.growHair((byte) index);
		}

		private static void onClickedBeardButton(ISleekElement button)
		{
			int index = Mathf.FloorToInt((button.PositionOffset_X / 50) + ((button.PositionOffset_Y - 40) / 50 * 5));

			Characters.growBeard((byte) index);
		}

		private static void onClickedSkinButton(ISleekElement button)
		{
			int index = Mathf.FloorToInt((button.PositionOffset_X / 50) + ((button.PositionOffset_Y - 40) / 50 * 5));
			Color color = Customization.SKINS[index];

			Characters.paintSkin(color);
			skinColorPicker.state = color;

			updateFaces(color);
		}

		private static void onSkinColorPicked(SleekColorPicker picker, Color color)
		{
			Characters.paintSkin(color);

			updateFaces(color);
		}

		private static void OnClickedHairColorButton(ISleekElement button)
		{
			int index = Mathf.FloorToInt((button.PositionOffset_X / 50) + ((button.PositionOffset_Y - 40) / 50 * 5));
			Color color = Customization.COLORS[index];

			Characters.paintColor(color);
			hairColorPicker.state = color;
			UpdateHairButtonTint(color);

			Characters.ChangeBeardColor(color);
			beardColorPicker.state = color;
			UpdateBeardButtonTint(color);
		}

		private static void OnHairColorPicked(SleekColorPicker picker, Color color)
		{
			Characters.paintColor(color);

			UpdateHairButtonTint(color);
		}

		private static void OnBeardColorPicked(SleekColorPicker picker, Color color)
		{
			Characters.ChangeBeardColor(color);
			UpdateBeardButtonTint(color);
		}

		private static void onSwappedHandState(SleekButtonState button, int index)
		{
			Characters.hand(index != 0);
		}

		private static void onDraggedCharacterSlider(ISleekSlider slider, float state)
		{
			Characters.characterYaw = state * 360;
		}

		private static void onClickedBackButton(ISleekElement button)
		{
			MenuSurvivorsUI.open();
			close();
		}

		public void OnDestroy()
		{
			Characters.onCharacterUpdated -= onCharacterUpdated;
		}

		public MenuSurvivorsAppearanceUI()
		{
			localization = Localization.read("/Menu/Survivors/MenuSurvivorsAppearance.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			customizationBox = Glazier.Get().CreateScrollView();
			customizationBox.PositionOffset_X = -140;
			customizationBox.PositionOffset_Y = 100;
			customizationBox.PositionScale_X = 0.75f;
			customizationBox.SizeOffset_X = 270;
			customizationBox.SizeOffset_Y = -270;
			customizationBox.SizeScale_Y = 1;
			container.AddChild(customizationBox);

			float verticalOffset = 0;
			const float spacing = 10;

			faceBox = Glazier.Get().CreateBox();
			faceBox.PositionOffset_Y = verticalOffset;
			faceBox.SizeOffset_X = 240;
			faceBox.SizeOffset_Y = 30;
			faceBox.Text = localization.format("Face_Box");
			faceBox.TooltipText = localization.format("Face_Box_Tooltip");
			customizationBox.AddChild(faceBox);
			verticalOffset += faceBox.SizeOffset_Y + spacing;

			faceButtons = new ISleekButton[Customization.FACES_FREE + Customization.FACES_PRO];
			for (int index = 0; index < faceButtons.Length; index++)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_X = index % 5 * 50;
				button.PositionOffset_Y = 40 + (Mathf.FloorToInt(index / 5f) * 50);
				button.SizeOffset_X = 40;
				button.SizeOffset_Y = 40;
				faceBox.AddChild(button);

				ISleekImage skin = Glazier.Get().CreateImage();
				skin.PositionOffset_X = 10;
				skin.PositionOffset_Y = 10;
				skin.SizeOffset_X = 20;
				skin.SizeOffset_Y = 20;
				skin.Texture = GlazierResources.PixelTexture;
				button.AddChild(skin);

				ISleekImage icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = 2;
				icon.PositionOffset_Y = 2;
				icon.SizeOffset_X = 16;
				icon.SizeOffset_Y = 16;
				icon.Texture = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Faces/" + index + "/Texture.png");
				skin.AddChild(icon);

				if (index >= Customization.FACES_FREE)
				{
					if (Provider.isPro)
					{
						button.OnClicked += onClickedFaceButton;
					}
					else
					{
						button.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);

						IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

						ISleekImage pro = Glazier.Get().CreateImage();
						pro.PositionOffset_X = -10;
						pro.PositionOffset_Y = -10;
						pro.PositionScale_X = 0.5f;
						pro.PositionScale_Y = 0.5f;
						pro.SizeOffset_X = 20;
						pro.SizeOffset_Y = 20;
						pro.Texture = pros.load<Texture2D>("Lock_Small");
						button.AddChild(pro);
					}
				}
				else
				{
					button.OnClicked += onClickedFaceButton;
				}

				faceButtons[index] = button;
			}
			verticalOffset += MathfEx.GetPageCount(faceButtons.Length, 5) * 50;

			hairBox = Glazier.Get().CreateBox();
			hairBox.PositionOffset_Y = verticalOffset;
			hairBox.SizeOffset_X = 240;
			hairBox.SizeOffset_Y = 30;
			hairBox.Text = localization.format("Hair_Box");
			hairBox.TooltipText = localization.format("Hair_Box_Tooltip");
			customizationBox.AddChild(hairBox);
			verticalOffset += hairBox.SizeOffset_Y + spacing;

			hairButtons = new ISleekButton[Customization.HAIRS_FREE + Customization.HAIRS_PRO];
			for (int index = 0; index < hairButtons.Length; index++)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_X = index % 5 * 50;
				button.PositionOffset_Y = 40 + (Mathf.FloorToInt(index / 5f) * 50);
				button.SizeOffset_X = 40;
				button.SizeOffset_Y = 40;
				hairBox.AddChild(button);

				ISleekImage icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = 10;
				icon.PositionOffset_Y = 10;
				icon.SizeOffset_X = 20;
				icon.SizeOffset_Y = 20;
				icon.Texture = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Hairs/" + index + "/Texture.png");
				button.AddChild(icon);

				if (index >= Customization.HAIRS_FREE)
				{
					if (Provider.isPro)
					{
						button.OnClicked += onClickedHairButton;
					}
					else
					{
						button.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);

						IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

						ISleekImage pro = Glazier.Get().CreateImage();
						pro.PositionOffset_X = -10;
						pro.PositionOffset_Y = -10;
						pro.PositionScale_X = 0.5f;
						pro.PositionScale_Y = 0.5f;
						pro.SizeOffset_X = 20;
						pro.SizeOffset_Y = 20;
						pro.Texture = pros.load<Texture2D>("Lock_Small");
						button.AddChild(pro);
					}
				}
				else
				{
					button.OnClicked += onClickedHairButton;
				}

				hairButtons[index] = button;
			}
			verticalOffset += MathfEx.GetPageCount(hairButtons.Length, 5) * 50;

			beardBox = Glazier.Get().CreateBox();
			beardBox.PositionOffset_Y = verticalOffset;
			beardBox.SizeOffset_X = 240;
			beardBox.SizeOffset_Y = 30;
			beardBox.Text = localization.format("Beard_Box");
			beardBox.TooltipText = localization.format("Beard_Box_Tooltip");
			customizationBox.AddChild(beardBox);
			verticalOffset += beardBox.SizeOffset_Y + spacing;

			beardButtons = new ISleekButton[Customization.BEARDS_FREE + Customization.BEARDS_PRO];
			for (int index = 0; index < beardButtons.Length; index++)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_X = index % 5 * 50;
				button.PositionOffset_Y = 40 + (Mathf.FloorToInt(index / 5f) * 50);
				button.SizeOffset_X = 40;
				button.SizeOffset_Y = 40;
				beardBox.AddChild(button);

				ISleekImage icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = 10;
				icon.PositionOffset_Y = 10;
				icon.SizeOffset_X = 20;
				icon.SizeOffset_Y = 20;
				icon.Texture = Assets.coreMasterBundle.LoadAsset<Texture2D>("Items/Beards/" + index + "/Texture.png");
				button.AddChild(icon);

				if (index >= Customization.BEARDS_FREE)
				{
					if (Provider.isPro)
					{
						button.OnClicked += onClickedBeardButton;
					}
					else
					{
						button.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);

						IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

						ISleekImage pro = Glazier.Get().CreateImage();
						pro.PositionOffset_X = -10;
						pro.PositionOffset_Y = -10;
						pro.PositionScale_X = 0.5f;
						pro.PositionScale_Y = 0.5f;
						pro.SizeOffset_X = 20;
						pro.SizeOffset_Y = 20;
						pro.Texture = pros.load<Texture2D>("Lock_Small");
						button.AddChild(pro);
					}
				}
				else
				{
					button.OnClicked += onClickedBeardButton;
				}

				beardButtons[index] = button;
			}
			verticalOffset += MathfEx.GetPageCount(beardButtons.Length, 5) * 50;

			skinBox = Glazier.Get().CreateBox();
			skinBox.PositionOffset_Y = verticalOffset;
			skinBox.SizeOffset_X = 240;
			skinBox.SizeOffset_Y = 30;
			skinBox.Text = localization.format("Skin_Box");
			skinBox.TooltipText = localization.format("Skin_Box_Tooltip");
			customizationBox.AddChild(skinBox);
			verticalOffset += skinBox.SizeOffset_Y + 10;

			skinButtons = new ISleekButton[Customization.SKINS.Length];
			for (int index = 0; index < skinButtons.Length; index++)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_X = index % 5 * 50;
				button.PositionOffset_Y = 40 + (Mathf.FloorToInt(index / 5f) * 50);
				button.SizeOffset_X = 40;
				button.SizeOffset_Y = 40;
				button.OnClicked += onClickedSkinButton;
				skinBox.AddChild(button);

				ISleekImage icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = 10;
				icon.PositionOffset_Y = 10;
				icon.SizeOffset_X = 20;
				icon.SizeOffset_Y = 20;
				icon.Texture = GlazierResources.PixelTexture;
				icon.TintColor = Customization.SKINS[index];
				button.AddChild(icon);

				skinButtons[index] = button;
			}
			verticalOffset += MathfEx.GetPageCount(skinButtons.Length, 5) * 50;

			skinColorPicker = new SleekColorPicker();
			skinColorPicker.PositionOffset_Y = verticalOffset;
			customizationBox.AddChild(skinColorPicker);
			verticalOffset += skinColorPicker.SizeOffset_Y + spacing;

			if (Provider.isPro)
			{
				skinColorPicker.onColorPicked = onSkinColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				skinColorPicker.AddChild(pro);
			}

			hairColorBox = Glazier.Get().CreateBox();
			hairColorBox.PositionOffset_Y = verticalOffset;
			hairColorBox.SizeOffset_X = 240;
			hairColorBox.SizeOffset_Y = 30;
			hairColorBox.Text = localization.format("Color_Box");
			hairColorBox.TooltipText = localization.format("Color_Box_Tooltip");
			customizationBox.AddChild(hairColorBox);
			verticalOffset += hairColorBox.SizeOffset_Y + spacing;

			colorButtons = new ISleekButton[Customization.COLORS.Length];
			for (int index = 0; index < colorButtons.Length; index++)
			{
				ISleekButton button = Glazier.Get().CreateButton();
				button.PositionOffset_X = index % 5 * 50;
				button.PositionOffset_Y = 40 + (Mathf.FloorToInt(index / 5f) * 50);
				button.SizeOffset_X = 40;
				button.SizeOffset_Y = 40;
				button.OnClicked += OnClickedHairColorButton;
				hairColorBox.AddChild(button);

				ISleekImage icon = Glazier.Get().CreateImage();
				icon.PositionOffset_X = 10;
				icon.PositionOffset_Y = 10;
				icon.SizeOffset_X = 20;
				icon.SizeOffset_Y = 20;
				icon.Texture = GlazierResources.PixelTexture;
				icon.TintColor = Customization.COLORS[index];
				button.AddChild(icon);

				colorButtons[index] = button;
			}
			verticalOffset += MathfEx.GetPageCount(colorButtons.Length, 5) * 50;

			hairColorPicker = new SleekColorPicker();
			hairColorPicker.PositionOffset_Y = verticalOffset;
			customizationBox.AddChild(hairColorPicker);
			verticalOffset += hairColorPicker.SizeOffset_Y + spacing;

			if (Provider.isPro)
			{
				hairColorPicker.onColorPicked = OnHairColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				hairColorPicker.AddChild(pro);
			}

			beardColorBox = Glazier.Get().CreateBox();
			beardColorBox.PositionOffset_Y = verticalOffset;
			beardColorBox.SizeOffset_X = 240;
			beardColorBox.SizeOffset_Y = 30;
			beardColorBox.Text = localization.format("Beard_Color_Box");
			beardColorBox.TooltipText = localization.format("Beard_Color_Box_Tooltip");
			customizationBox.AddChild(beardColorBox);
			verticalOffset += beardColorBox.SizeOffset_Y + spacing;

			beardColorPicker = new SleekColorPicker();
			beardColorPicker.PositionOffset_Y = verticalOffset;
			customizationBox.AddChild(beardColorPicker);
			verticalOffset += beardColorPicker.SizeOffset_Y + spacing;

			if (Provider.isPro)
			{
				beardColorPicker.onColorPicked = OnBeardColorPicked;
			}
			else
			{
				IconsBundle pros = Bundles.getIconsBundle("UI/Menu/Icons/Pro");

				ISleekImage pro = Glazier.Get().CreateImage();
				pro.PositionOffset_X = -40;
				pro.PositionOffset_Y = -40;
				pro.PositionScale_X = 0.5f;
				pro.PositionScale_Y = 0.5f;
				pro.SizeOffset_X = 80;
				pro.SizeOffset_Y = 80;
				pro.Texture = pros.load<Texture2D>("Lock_Large");
				beardColorPicker.AddChild(pro);
			}

			customizationBox.ScaleContentToWidth = true;
			customizationBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset - spacing);

			handState = new SleekButtonState(new GUIContent(localization.format("Right")), new GUIContent(localization.format("Left")));
			handState.PositionOffset_X = -140;
			handState.PositionOffset_Y = -160;
			handState.PositionScale_X = 0.75f;
			handState.PositionScale_Y = 1;
			handState.SizeOffset_X = 240;
			handState.SizeOffset_Y = 30;
			handState.onSwappedState = onSwappedHandState;
			container.AddChild(handState);

			characterSlider = Glazier.Get().CreateSlider();
			characterSlider.PositionOffset_X = -140;
			characterSlider.PositionOffset_Y = -120;
			characterSlider.PositionScale_X = 0.75f;
			characterSlider.PositionScale_Y = 1;
			characterSlider.SizeOffset_X = 240;
			characterSlider.SizeOffset_Y = 20;
			characterSlider.Orientation = ESleekOrientation.HORIZONTAL;
			characterSlider.OnValueChanged += onDraggedCharacterSlider;
			container.AddChild(characterSlider);

			Characters.onCharacterUpdated += onCharacterUpdated;
			onCharacterUpdated(Characters.selected, Characters.list[Characters.selected]);

			backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
			backButton.PositionOffset_Y = -50;
			backButton.PositionScale_Y = 1f;
			backButton.SizeOffset_X = 200;
			backButton.SizeOffset_Y = 50;
			backButton.text = MenuDashboardUI.localization.format("BackButtonText");
			backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
			backButton.onClickedButton += onClickedBackButton;
			backButton.fontSize = ESleekFontSize.Medium;
			backButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(backButton);
		}
	}
}
