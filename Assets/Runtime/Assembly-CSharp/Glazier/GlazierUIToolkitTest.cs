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
	/// Used in a test scene to quickly test UI Toolkit implementation.
	/// </summary>
	internal class GlazierUIToolkitTest : MonoBehaviour
	{
		private void Awake()
		{
			Glazier.instance = Glazier_UIToolkit.CreateGlazier();

			SleekWindow window = new SleekWindow();
			Glazier.Get().Root = window;
			
			ISleekLabel label = Glazier.Get().CreateLabel();
			label.PositionOffset_X = 10;
			label.PositionOffset_Y = 10;
			label.SizeOffset_X = 200;
			label.SizeOffset_Y = 30;
			label.Text = "Label 1";
			window.AddChild(label);

			ISleekButton button = Glazier.Get().CreateButton();
			button.PositionOffset_X = 10;
			button.PositionOffset_Y = 40;
			button.SizeOffset_X = 200;
			button.SizeOffset_Y = 30;
			button.Text = "Button 1";
			window.AddChild(button);

			ISleekImage image = Glazier.Get().CreateImage();
			image.PositionOffset_X = 10;
			image.PositionOffset_Y = 70;
			image.SizeOffset_X = 40;
			image.SizeOffset_Y = 40;
			image.Texture = Resources.Load<Texture>("Bundles/Textures/Player/Icons/PlayerLife/Acid");
			window.AddChild(image);

			ISleekSprite sprite = Glazier.Get().CreateSprite();
			sprite.PositionOffset_X = 50;
			sprite.PositionOffset_Y = 70;
			sprite.SizeOffset_X = 20;
			sprite.SizeOffset_Y = 20;
			sprite.Sprite = Resources.Load<Sprite>("Bundles/Textures/Menu/Icons/MenuDashboard/External_Link_Sprite");
			window.AddChild(sprite);

			ISleekField stringField = Glazier.Get().CreateStringField();
			stringField.PositionOffset_X = 10;
			stringField.PositionOffset_Y = 110;
			stringField.SizeOffset_X = 200;
			stringField.SizeOffset_Y = 30;
			window.AddChild(stringField);

			ISleekToggle toggle = Glazier.Get().CreateToggle();
			toggle.PositionOffset_X = 10;
			toggle.PositionOffset_Y = 140;
			toggle.SizeOffset_X = 40;
			toggle.SizeOffset_Y = 40;
			window.AddChild(toggle);

			ISleekSlider slider = Glazier.Get().CreateSlider();
			slider.PositionOffset_X = 200;
			slider.PositionOffset_Y = 10;
			slider.SizeOffset_X = 20;
			slider.SizeOffset_Y = 200;
			window.AddChild(slider);
		}
	}
}
#endif // UNITY_EDITOR
