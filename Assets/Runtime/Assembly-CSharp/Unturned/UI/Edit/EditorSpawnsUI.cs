////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorSpawnsUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon animalsButton;
		private static SleekButtonIcon itemsButton;
		private static SleekButtonIcon zombiesButton;
		private static SleekButtonIcon vehiclesButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			EditorSpawnsItemsUI.close();
			EditorSpawnsAnimalsUI.close();
			EditorSpawnsZombiesUI.close();
			EditorSpawnsVehiclesUI.close();

			container.AnimateOutOfView(1, 0);
		}

		private static void onClickedAnimalsButton(ISleekElement button)
		{
			EditorSpawnsItemsUI.close();
			EditorSpawnsZombiesUI.close();
			EditorSpawnsVehiclesUI.close();

			EditorSpawnsAnimalsUI.open();
		}

		private static void onClickItemsButton(ISleekElement button)
		{
			EditorSpawnsAnimalsUI.close();
			EditorSpawnsZombiesUI.close();
			EditorSpawnsVehiclesUI.close();

			EditorSpawnsItemsUI.open();
		}

		private static void onClickedZombiesButton(ISleekElement button)
		{
			EditorSpawnsAnimalsUI.close();
			EditorSpawnsItemsUI.close();
			EditorSpawnsVehiclesUI.close();

			EditorSpawnsZombiesUI.open();
		}

		private static void onClickedVehiclesButton(ISleekElement button)
		{
			EditorSpawnsAnimalsUI.close();
			EditorSpawnsItemsUI.close();
			EditorSpawnsZombiesUI.close();

			EditorSpawnsVehiclesUI.open();
		}

		public EditorSpawnsUI()
		{
			Local localization = Localization.read("/Editor/EditorSpawns.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorSpawns");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_X = 1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			EditorUI.window.AddChild(container);
			active = false;

			animalsButton = new SleekButtonIcon(icons.load<Texture2D>("Animals"));
			animalsButton.PositionOffset_Y = 40;
			animalsButton.SizeOffset_X = -5;
			animalsButton.SizeOffset_Y = 30;
			animalsButton.SizeScale_X = 0.25f;
			animalsButton.text = localization.format("AnimalsButtonText");
			animalsButton.tooltip = localization.format("AnimalsButtonTooltip");
			animalsButton.onClickedButton += onClickedAnimalsButton;
			container.AddChild(animalsButton);

			itemsButton = new SleekButtonIcon(icons.load<Texture2D>("Items"));
			itemsButton.PositionOffset_X = 5;
			itemsButton.PositionOffset_Y = 40;
			itemsButton.PositionScale_X = 0.25f;
			itemsButton.SizeOffset_X = -10;
			itemsButton.SizeOffset_Y = 30;
			itemsButton.SizeScale_X = 0.25f;
			itemsButton.text = localization.format("ItemsButtonText");
			itemsButton.tooltip = localization.format("ItemsButtonTooltip");
			itemsButton.onClickedButton += onClickItemsButton;
			container.AddChild(itemsButton);

			zombiesButton = new SleekButtonIcon(icons.load<Texture2D>("Zombies"));
			zombiesButton.PositionOffset_X = 5;
			zombiesButton.PositionOffset_Y = 40;
			zombiesButton.PositionScale_X = 0.5f;
			zombiesButton.SizeOffset_X = -10;
			zombiesButton.SizeOffset_Y = 30;
			zombiesButton.SizeScale_X = 0.25f;
			zombiesButton.text = localization.format("ZombiesButtonText");
			zombiesButton.tooltip = localization.format("ZombiesButtonTooltip");
			zombiesButton.onClickedButton += onClickedZombiesButton;
			container.AddChild(zombiesButton);

			vehiclesButton = new SleekButtonIcon(icons.load<Texture2D>("Vehicles"));
			vehiclesButton.PositionOffset_X = 5;
			vehiclesButton.PositionOffset_Y = 40;
			vehiclesButton.PositionScale_X = 0.75f;
			vehiclesButton.SizeOffset_X = -5;
			vehiclesButton.SizeOffset_Y = 30;
			vehiclesButton.SizeScale_X = 0.25f;
			vehiclesButton.text = localization.format("VehiclesButtonText");
			vehiclesButton.tooltip = localization.format("VehiclesButtonTooltip");
			vehiclesButton.onClickedButton += onClickedVehiclesButton;
			container.AddChild(vehiclesButton);

			new EditorSpawnsAnimalsUI();
			new EditorSpawnsItemsUI();
			new EditorSpawnsZombiesUI();
			new EditorSpawnsVehiclesUI();
		}
	}
}
