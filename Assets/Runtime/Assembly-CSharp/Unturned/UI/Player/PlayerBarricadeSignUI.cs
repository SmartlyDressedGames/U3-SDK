////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class PlayerBarricadeSignUI
	{
		private static SleekFullscreenBox container;

		public static bool active;
		private static InteractableSign sign;

		private static ISleekField textField;
		private static ISleekBox textBox;

		private static ISleekButton yesButton;
		private static ISleekButton noButton;

		public static void open(string newText)
		{
			if (active)
			{
				return;
			}

			active = true;
			sign = null;

			yesButton.IsVisible = false;
			yesButton.IsClickable = true;
			noButton.PositionOffset_X = -200;
			noButton.SizeOffset_X = 400;

			string text = newText;
			ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref text);

			// Same as NPC.
			text = text.Replace("<name_char>", Player.LocalPlayer.channel.owner.playerID.characterName);

			textBox.Text = text;
			textField.IsVisible = false;
			textBox.IsVisible = true;

			container.AnimateIntoView();
		}

		public static void open(InteractableSign newSign)
		{
			if (active)
			{
				close();
				return;
			}

			active = true;
			sign = newSign;

			yesButton.IsVisible = true;
			yesButton.IsClickable = true;
			noButton.PositionOffset_X = 5;
			noButton.SizeOffset_X = 195;

			textField.Text = sign.DisplayText;
			textField.IsVisible = true;
			textBox.IsVisible = false;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			sign = null;

			// Workaround for IMGUI because mouse needed to be clicked after exiting sign to allow keyboard input.
			textField.ClearFocus();

			container.AnimateOutOfView(0, 1);
		}

		private static void onTypedSignText(ISleekField field, string text)
		{
			// Must in case sign was destroyed while editing.
			if (sign != null)
			{
				string trimmedText = sign.trimText(text);
				yesButton.IsClickable = sign.isTextValid(trimmedText);
			}
			else
			{
				yesButton.IsClickable = false;
			}
		}

		private static void onClickedYesButton(ISleekElement button)
		{
			if (sign != null)
			{
				string trimmedText = sign.trimText(textField.Text);
				sign.ClientSetText(trimmedText);
			}

			PlayerLifeUI.open();
			close();
		}

		private static void onClickedNoButton(ISleekElement button)
		{
			PlayerLifeUI.open();
			close();
		}

		public PlayerBarricadeSignUI()
		{
			Local localization = Localization.read("/Player/PlayerBarricadeSign.dat");

			container = new SleekFullscreenBox();
			container.PositionScale_Y = 1;
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			PlayerUI.container.AddChild(container);
			active = false;
			sign = null;

			textField = Glazier.Get().CreateStringField();
			textField.PositionOffset_X = -200;
			textField.PositionScale_X = 0.5f;
			textField.PositionScale_Y = 0.1f;
			textField.SizeOffset_X = 400;
			textField.SizeScale_Y = 0.8f;
			textField.MaxLength = 200;
			textField.IsMultiline = true;
			textField.OnTextChanged += onTypedSignText;
			container.AddChild(textField);

			textBox = Glazier.Get().CreateBox();
			textBox.PositionOffset_X = -200;
			textBox.PositionScale_X = 0.5f;
			textBox.PositionScale_Y = 0.1f;
			textBox.SizeOffset_X = 400;
			textBox.SizeScale_Y = 0.8f;
			textBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
			textBox.AllowRichText = true;
			container.AddChild(textBox);

			yesButton = Glazier.Get().CreateButton();
			yesButton.PositionOffset_X = -200;
			yesButton.PositionOffset_Y = 5;
			yesButton.PositionScale_X = 0.5f;
			yesButton.PositionScale_Y = 0.9f;
			yesButton.SizeOffset_X = 195;
			yesButton.SizeOffset_Y = 30;
			yesButton.Text = localization.format("Yes_Button");
			yesButton.TooltipText = localization.format("Yes_Button_Tooltip");
			yesButton.OnClicked += onClickedYesButton;
			container.AddChild(yesButton);

			noButton = Glazier.Get().CreateButton();
			noButton.PositionOffset_X = 5;
			noButton.PositionOffset_Y = 5;
			noButton.PositionScale_X = 0.5f;
			noButton.PositionScale_Y = 0.9f;
			noButton.SizeOffset_X = 195;
			noButton.SizeOffset_Y = 30;
			noButton.Text = localization.format("No_Button");
			noButton.TooltipText = localization.format("No_Button_Tooltip");
			noButton.OnClicked += onClickedNoButton;
			container.AddChild(noButton);
		}
	}
}
