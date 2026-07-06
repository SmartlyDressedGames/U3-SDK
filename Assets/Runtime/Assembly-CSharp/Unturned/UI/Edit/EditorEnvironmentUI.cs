////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorEnvironmentUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon lightingButton;
		private static SleekButtonIcon roadsButton;
		private static SleekButtonIcon navigationButton;
		private static SleekButtonIcon nodesButton;

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

			EditorEnvironmentLightingUI.close();
			EditorEnvironmentRoadsUI.close();
			EditorEnvironmentNavigationUI.close();
			nodesUI.Close();

			container.AnimateOutOfView(1, 0);
		}

		private static void onClickedLightingButton(ISleekElement button)
		{
			EditorEnvironmentRoadsUI.close();
			EditorEnvironmentNavigationUI.close();
			nodesUI.Close();

			EditorEnvironmentLightingUI.open();
		}

		private static void onClickedRoadsButton(ISleekElement button)
		{
			EditorEnvironmentLightingUI.close();
			EditorEnvironmentNavigationUI.close();
			nodesUI.Close();

			EditorEnvironmentRoadsUI.open();
		}

		private static void onClickedNavigationButton(ISleekElement button)
		{
			EditorEnvironmentLightingUI.close();
			EditorEnvironmentRoadsUI.close();
			nodesUI.Close();

			EditorEnvironmentNavigationUI.open();
		}

		private static void onClickedNodesButton(ISleekElement button)
		{
			EditorEnvironmentLightingUI.close();
			EditorEnvironmentRoadsUI.close();
			EditorEnvironmentNavigationUI.close();

			nodesUI.Open();
		}

		public void OnDestroy()
		{
			nodesUI.OnDestroy();
		}

		public EditorEnvironmentUI()
		{
			Local localization = Localization.read("/Editor/EditorEnvironment.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorEnvironment");

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

			lightingButton = new SleekButtonIcon(icons.load<Texture2D>("Lighting"));
			lightingButton.PositionOffset_Y = 40;
			lightingButton.SizeOffset_X = -5;
			lightingButton.SizeOffset_Y = 30;
			lightingButton.SizeScale_X = 0.25f;
			lightingButton.text = localization.format("LightingButtonText");
			lightingButton.tooltip = localization.format("LightingButtonTooltip");
			lightingButton.onClickedButton += onClickedLightingButton;
			container.AddChild(lightingButton);

			roadsButton = new SleekButtonIcon(icons.load<Texture2D>("Roads"));
			roadsButton.PositionOffset_X = 5;
			roadsButton.PositionOffset_Y = 40;
			roadsButton.PositionScale_X = 0.25f;
			roadsButton.SizeOffset_X = -10;
			roadsButton.SizeOffset_Y = 30;
			roadsButton.SizeScale_X = 0.25f;
			roadsButton.text = localization.format("RoadsButtonText");
			roadsButton.tooltip = localization.format("RoadsButtonTooltip");
			roadsButton.onClickedButton += onClickedRoadsButton;
			container.AddChild(roadsButton);

			navigationButton = new SleekButtonIcon(icons.load<Texture2D>("Navigation"));
			navigationButton.PositionOffset_X = 5;
			navigationButton.PositionOffset_Y = 40;
			navigationButton.PositionScale_X = 0.5f;
			navigationButton.SizeOffset_X = -10;
			navigationButton.SizeOffset_Y = 30;
			navigationButton.SizeScale_X = 0.25f;
			navigationButton.text = localization.format("NavigationButtonText");
			navigationButton.tooltip = localization.format("NavigationButtonTooltip");
			navigationButton.onClickedButton += onClickedNavigationButton;
			container.AddChild(navigationButton);

			nodesButton = new SleekButtonIcon(icons.load<Texture2D>("Nodes"));
			nodesButton.PositionOffset_X = 5;
			nodesButton.PositionOffset_Y = 40;
			nodesButton.PositionScale_X = 0.75f;
			nodesButton.SizeOffset_X = -5;
			nodesButton.SizeOffset_Y = 30;
			nodesButton.SizeScale_X = 0.25f;
			nodesButton.text = localization.format("NodesButtonText");
			nodesButton.tooltip = localization.format("NodesButtonTooltip");
			nodesButton.onClickedButton += onClickedNodesButton;
			container.AddChild(nodesButton);

			new EditorEnvironmentLightingUI();
			new EditorEnvironmentRoadsUI();
			new EditorEnvironmentNavigationUI();
			nodesUI = new EditorEnvironmentNodesUI();
			nodesUI.PositionOffset_X = 10;
			nodesUI.PositionOffset_Y = 90;
			nodesUI.PositionScale_X = 1.0f;
			nodesUI.SizeOffset_X = -20;
			nodesUI.SizeOffset_Y = -100;
			nodesUI.SizeScale_X = 1.0f;
			nodesUI.SizeScale_Y = 1.0f;
			EditorUI.window.AddChild(nodesUI);
		}

		private static EditorEnvironmentNodesUI nodesUI;
	}
}
