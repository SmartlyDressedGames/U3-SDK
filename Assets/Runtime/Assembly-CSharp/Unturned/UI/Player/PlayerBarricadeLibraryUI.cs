////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class PlayerBarricadeLibraryUI
	{
		private static SleekFullscreenBox container;
		private static Local localization;

		public static bool active;
		private static InteractableLibrary library;

		private static ISleekBox capacityBox; // 10/100
		private static ISleekBox walletBox; // 15
		private static ISleekUInt32Field amountField; // 15
		private static SleekButtonState transactionButton;
		private static ISleekBox taxBox; // Tax: 5
		private static ISleekBox netBox; // Net change: 10
		private static uint tax;
		private static uint net;

		private static ISleekButton yesButton;
		private static ISleekButton noButton;

		public static void open(InteractableLibrary newLibrary)
		{
			if (active)
			{
				return;
			}

			active = true;
			library = newLibrary;

			if (library != null)
			{
				capacityBox.Text = localization.format("Capacity_Text", library.amount, library.capacity);
				walletBox.Text = Player.LocalPlayer.skills.experience.ToString();
				amountField.Value = 0;
				updateTax();
			}

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			library = null;

			container.AnimateOutOfView(0, 1);
		}

		private static void updateTax()
		{
			if (library != null)
			{
				if (transactionButton.state == 0) // Deposit
				{
					tax = (uint) System.Math.Ceiling(amountField.Value * (library.tax / 100.0));
					net = amountField.Value - tax;

					yesButton.IsClickable = amountField.Value <= Player.LocalPlayer.skills.experience &&
						net + library.amount <= library.capacity;
				}
				else // Withdraw
				{
					tax = 0;
					net = amountField.Value - tax;

					yesButton.IsClickable = net <= library.amount;
				}

				ESleekTint textColor = yesButton.IsClickable ? ESleekTint.FONT : ESleekTint.BAD;
				amountField.TextColor = textColor;
				taxBox.TextColor = textColor;
				netBox.TextColor = textColor;
			}

			taxBox.Text = tax.ToString();
			netBox.Text = net.ToString();
		}

		private static void onTypedAmountField(ISleekUInt32Field field, uint state)
		{
			updateTax();
		}

		private static void onSwappedTransactionState(SleekButtonState button, int index)
		{
			updateTax();
		}

		private static void onClickedYesButton(ISleekElement button)
		{
			if (library != null)
			{
				if (transactionButton.state == 0)
				{
					if (amountField.Value > Player.LocalPlayer.skills.experience || net + library.amount > library.capacity)
					{
						return;
					}
				}
				else
				{
					if (net > library.amount)
					{
						return;
					}
				}

				if (net > 0)
				{
					library.ClientTransfer((byte) transactionButton.state, amountField.Value);
				}
			}

			PlayerLifeUI.open();
			close();
		}

		private static void onClickedNoButton(ISleekElement button)
		{
			PlayerLifeUI.open();
			close();
		}

		public PlayerBarricadeLibraryUI()
		{
			localization = Localization.read("/Player/PlayerBarricadeLibrary.dat");

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
			library = null;

			capacityBox = Glazier.Get().CreateBox();
			capacityBox.PositionOffset_X = -100;
			capacityBox.PositionOffset_Y = -135;
			capacityBox.PositionScale_X = 0.5f;
			capacityBox.PositionScale_Y = 0.5f;
			capacityBox.SizeOffset_X = 200;
			capacityBox.SizeOffset_Y = 30;
			capacityBox.AddLabel(localization.format("Capacity_Label"), ESleekSide.LEFT);
			container.AddChild(capacityBox);

			walletBox = Glazier.Get().CreateBox();
			walletBox.PositionOffset_X = -100;
			walletBox.PositionOffset_Y = -95;
			walletBox.PositionScale_X = 0.5f;
			walletBox.PositionScale_Y = 0.5f;
			walletBox.SizeOffset_X = 200;
			walletBox.SizeOffset_Y = 30;
			walletBox.AddLabel(localization.format("Wallet_Label"), ESleekSide.LEFT);
			container.AddChild(walletBox);

			amountField = Glazier.Get().CreateUInt32Field();
			amountField.PositionOffset_X = -100;
			amountField.PositionOffset_Y = -15;
			amountField.PositionScale_X = 0.5f;
			amountField.PositionScale_Y = 0.5f;
			amountField.SizeOffset_X = 200;
			amountField.SizeOffset_Y = 30;
			amountField.AddLabel(localization.format("Amount_Label"), ESleekSide.LEFT);
			amountField.OnValueChanged += onTypedAmountField;
			container.AddChild(amountField);

			transactionButton = new SleekButtonState(new GUIContent(localization.format("Deposit"), localization.format("Deposit_Tooltip")), new GUIContent(localization.format("Withdraw"), localization.format("Withdraw_Tooltip")));
			transactionButton.PositionOffset_X = -100;
			transactionButton.PositionOffset_Y = -55;
			transactionButton.PositionScale_X = 0.5f;
			transactionButton.PositionScale_Y = 0.5f;
			transactionButton.SizeOffset_X = 200;
			transactionButton.SizeOffset_Y = 30;
			transactionButton.AddLabel(localization.format("Transaction_Label"), ESleekSide.LEFT);
			transactionButton.onSwappedState = onSwappedTransactionState;
			container.AddChild(transactionButton);

			taxBox = Glazier.Get().CreateBox();
			taxBox.PositionOffset_X = -100;
			taxBox.PositionOffset_Y = 25;
			taxBox.PositionScale_X = 0.5f;
			taxBox.PositionScale_Y = 0.5f;
			taxBox.SizeOffset_X = 200;
			taxBox.SizeOffset_Y = 30;
			taxBox.AddLabel(localization.format("Tax_Label"), ESleekSide.LEFT);
			container.AddChild(taxBox);

			netBox = Glazier.Get().CreateBox();
			netBox.PositionOffset_X = -100;
			netBox.PositionOffset_Y = 65;
			netBox.PositionScale_X = 0.5f;
			netBox.PositionScale_Y = 0.5f;
			netBox.SizeOffset_X = 200;
			netBox.SizeOffset_Y = 30;
			netBox.AddLabel(localization.format("Net_Label"), ESleekSide.LEFT);
			container.AddChild(netBox);

			yesButton = Glazier.Get().CreateButton();
			yesButton.PositionOffset_X = -100;
			yesButton.PositionOffset_Y = 105;
			yesButton.PositionScale_X = 0.5f;
			yesButton.PositionScale_Y = 0.5f;
			yesButton.SizeOffset_X = 95;
			yesButton.SizeOffset_Y = 30;
			yesButton.Text = localization.format("Yes_Button");
			yesButton.TooltipText = localization.format("Yes_Button_Tooltip");
			yesButton.OnClicked += onClickedYesButton;
			container.AddChild(yesButton);

			noButton = Glazier.Get().CreateButton();
			noButton.PositionOffset_X = 5;
			noButton.PositionOffset_Y = 105;
			noButton.PositionScale_X = 0.5f;
			noButton.PositionScale_Y = 0.5f;
			noButton.SizeOffset_X = 95;
			noButton.SizeOffset_Y = 30;
			noButton.Text = localization.format("No_Button");
			noButton.TooltipText = localization.format("No_Button_Tooltip");
			noButton.OnClicked += onClickedNoButton;
			container.AddChild(noButton);
		}
	}
}
