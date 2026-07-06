////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorEnvironmentNavigationUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekSlider widthSlider;
		private static ISleekSlider heightSlider;
		private static ISleekBox navBox;
		private static ISleekField difficultyGUIDField;
		private static ISleekUInt8Field maxZombiesField;
		private static ISleekInt32Field maxBossZombiesField;
		private static ISleekToggle spawnZombiesToggle;
		private static ISleekToggle hyperAgroToggle;
		private static SleekButtonIcon bakeNavigationButton;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			EditorNavigation.isPathfinding = true;

			EditorUI.message(EEditorMessage.NAVIGATION);

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;
			EditorNavigation.isPathfinding = false;

			container.AnimateOutOfView(1, 0);
		}

		public static void updateSelection(Flag flag)
		{
			if (flag != null)
			{
				widthSlider.Value = flag.width;
				heightSlider.Value = flag.height;
				navBox.Text = flag.EditorFlagInterface.GraphIndexForUI.ToString();
				difficultyGUIDField.Text = flag.data.difficultyGUID;
				maxZombiesField.Value = flag.data.maxZombies;
				maxBossZombiesField.Value = flag.data.maxBossZombies;
				spawnZombiesToggle.Value = flag.data.spawnZombies;
				hyperAgroToggle.Value = flag.data.hyperAgro;
			}

			widthSlider.IsVisible = flag != null;
			heightSlider.IsVisible = flag != null;
			navBox.IsVisible = flag != null;
			difficultyGUIDField.IsVisible = flag != null;
			maxZombiesField.IsVisible = flag != null;
			maxBossZombiesField.IsVisible = flag != null;
			spawnZombiesToggle.IsVisible = flag != null;
			hyperAgroToggle.IsVisible = flag != null;
			bakeNavigationButton.IsVisible = flag != null;
		}

		private static void onDraggedWidthSlider(ISleekSlider slider, float state)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.width = state;
				EditorNavigation.flag.buildMesh();
			}
		}

		private static void onDraggedHeightSlider(ISleekSlider slider, float state)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.height = state;
				EditorNavigation.flag.buildMesh();
			}
		}

		private static void onDifficultyGUIDFieldTyped(ISleekField field, string state)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.data.difficultyGUID = state;
			}
		}

		private static void onMaxZombiesFieldTyped(ISleekUInt8Field field, byte state)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.data.maxZombies = state;
			}
		}

		private static void onMaxBossZombiesFieldTyped(ISleekInt32Field field, int state)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.data.maxBossZombies = state;
			}
		}

		private static void onToggledSpawnZombiesToggle(ISleekToggle toggle, bool state)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.data.spawnZombies = state;
			}
		}

		private static void onToggledHyperAgroToggle(ISleekToggle toggle, bool state)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.data.hyperAgro = state;
			}
		}

		private static void onClickedBakeNavigationButton(ISleekElement button)
		{
			if (EditorNavigation.flag != null)
			{
				EditorNavigation.flag.bakeNavigation();
			}
		}

		public EditorEnvironmentNavigationUI()
		{
			Local localization = Localization.read("/Editor/EditorEnvironmentNavigation.dat");
			IconsBundle icons = Bundles.getIconsBundle("UI/Edit/Icons/EditorEnvironmentNavigation");

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

			widthSlider = Glazier.Get().CreateSlider();
			widthSlider.PositionOffset_X = -200;
			widthSlider.PositionOffset_Y = 80;
			widthSlider.PositionScale_X = 1;
			widthSlider.SizeOffset_X = 200;
			widthSlider.SizeOffset_Y = 20;
			widthSlider.Orientation = ESleekOrientation.HORIZONTAL;
			widthSlider.AddLabel(localization.format("Width_Label"), ESleekSide.LEFT);
			widthSlider.OnValueChanged += onDraggedWidthSlider;
			container.AddChild(widthSlider);
			widthSlider.IsVisible = false;

			heightSlider = Glazier.Get().CreateSlider();
			heightSlider.PositionOffset_X = -200;
			heightSlider.PositionOffset_Y = 110;
			heightSlider.PositionScale_X = 1;
			heightSlider.SizeOffset_X = 200;
			heightSlider.SizeOffset_Y = 20;
			heightSlider.Orientation = ESleekOrientation.HORIZONTAL;
			heightSlider.AddLabel(localization.format("Height_Label"), ESleekSide.LEFT);
			heightSlider.OnValueChanged += onDraggedHeightSlider;
			container.AddChild(heightSlider);
			heightSlider.IsVisible = false;

			navBox = Glazier.Get().CreateBox();
			navBox.PositionOffset_X = -200;
			navBox.PositionOffset_Y = 140;
			navBox.PositionScale_X = 1;
			navBox.SizeOffset_X = 200;
			navBox.SizeOffset_Y = 30;
			navBox.AddLabel(localization.format("Nav_Label"), ESleekSide.LEFT);
			container.AddChild(navBox);
			navBox.IsVisible = false;

			difficultyGUIDField = Glazier.Get().CreateStringField();
			difficultyGUIDField.PositionOffset_X = -200;
			difficultyGUIDField.PositionOffset_Y = 180;
			difficultyGUIDField.PositionScale_X = 1;
			difficultyGUIDField.SizeOffset_X = 200;
			difficultyGUIDField.SizeOffset_Y = 30;
			difficultyGUIDField.MaxLength = 32;
			difficultyGUIDField.OnTextChanged += onDifficultyGUIDFieldTyped;
			difficultyGUIDField.AddLabel(localization.format("Difficulty_GUID_Field_Label"), ESleekSide.LEFT);
			container.AddChild(difficultyGUIDField);
			difficultyGUIDField.IsVisible = false;

			maxZombiesField = Glazier.Get().CreateUInt8Field();
			maxZombiesField.PositionOffset_X = -200;
			maxZombiesField.PositionOffset_Y = 220;
			maxZombiesField.PositionScale_X = 1;
			maxZombiesField.SizeOffset_X = 200;
			maxZombiesField.SizeOffset_Y = 30;
			maxZombiesField.OnValueChanged += onMaxZombiesFieldTyped;
			maxZombiesField.AddLabel(localization.format("Max_Zombies_Field_Label"), ESleekSide.LEFT);
			container.AddChild(maxZombiesField);
			maxZombiesField.IsVisible = false;

			maxBossZombiesField = Glazier.Get().CreateInt32Field();
			maxBossZombiesField.PositionOffset_X = -200;
			maxBossZombiesField.PositionOffset_Y = 260;
			maxBossZombiesField.PositionScale_X = 1;
			maxBossZombiesField.SizeOffset_X = 200;
			maxBossZombiesField.SizeOffset_Y = 30;
			maxBossZombiesField.OnValueChanged += onMaxBossZombiesFieldTyped;
			maxBossZombiesField.AddLabel(localization.format("Max_Boss_Zombies_Field_Label"), ESleekSide.LEFT);
			container.AddChild(maxBossZombiesField);
			maxBossZombiesField.IsVisible = false;

			spawnZombiesToggle = Glazier.Get().CreateToggle();
			spawnZombiesToggle.PositionOffset_X = -200;
			spawnZombiesToggle.PositionOffset_Y = 300;
			spawnZombiesToggle.PositionScale_X = 1;
			spawnZombiesToggle.SizeOffset_X = 40;
			spawnZombiesToggle.SizeOffset_Y = 40;
			spawnZombiesToggle.OnValueChanged += onToggledSpawnZombiesToggle;
			spawnZombiesToggle.AddLabel(localization.format("Spawn_Zombies_Toggle_Label"), ESleekSide.RIGHT);
			container.AddChild(spawnZombiesToggle);
			spawnZombiesToggle.IsVisible = false;

			hyperAgroToggle = Glazier.Get().CreateToggle();
			hyperAgroToggle.PositionOffset_X = -200;
			hyperAgroToggle.PositionOffset_Y = 350;
			hyperAgroToggle.PositionScale_X = 1;
			hyperAgroToggle.SizeOffset_X = 40;
			hyperAgroToggle.SizeOffset_Y = 40;
			hyperAgroToggle.OnValueChanged += onToggledHyperAgroToggle;
			hyperAgroToggle.AddLabel(localization.format("Hyper_Agro_Label"), ESleekSide.RIGHT);
			container.AddChild(hyperAgroToggle);
			hyperAgroToggle.IsVisible = false;

			bakeNavigationButton = new SleekButtonIcon(icons.load<Texture2D>("Navigation"));
			bakeNavigationButton.PositionOffset_X = -200;
			bakeNavigationButton.PositionOffset_Y = -30;
			bakeNavigationButton.PositionScale_X = 1;
			bakeNavigationButton.PositionScale_Y = 1;
			bakeNavigationButton.SizeOffset_X = 200;
			bakeNavigationButton.SizeOffset_Y = 30;
			bakeNavigationButton.text = localization.format("Bake_Navigation");
			bakeNavigationButton.tooltip = localization.format("Bake_Navigation_Tooltip");
			bakeNavigationButton.onClickedButton += onClickedBakeNavigationButton;
			container.AddChild(bakeNavigationButton);
			bakeNavigationButton.IsVisible = false;
		}
	}
}
