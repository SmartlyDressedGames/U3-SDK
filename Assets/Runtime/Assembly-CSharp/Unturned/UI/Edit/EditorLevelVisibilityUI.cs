////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorLevelVisibilityUI
	{
		private static readonly byte DEBUG_SIZE = 7;

		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static List<MeshFilter> meshes = new List<MeshFilter>();

		public static ISleekToggle roadsToggle;
		public static ISleekToggle navigationToggle;
		public static ISleekToggle nodesToggle;
		public static ISleekToggle itemsToggle;
		public static ISleekToggle playersToggle;
		public static ISleekToggle zombiesToggle;
		public static ISleekToggle vehiclesToggle;
		public static ISleekToggle borderToggle;
		public static ISleekToggle animalsToggle;
		public static ISleekToggle decalsToggle;

		private static ISleekLabel[] regionLabels;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;
			update(Editor.editor.area.region_x, Editor.editor.area.region_y);

			EditorUI.message(EEditorMessage.VISIBILITY);

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			for (int index = 0; index < regionLabels.Length; index++)
			{
				ISleekLabel regionLabel = regionLabels[index];
				regionLabel.IsVisible = false;
			}

			container.AnimateOutOfView(1, 0);
		}

		private static void onToggledRoadsToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.roadsVisible = state;
		}

		private static void onToggledNavigationToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.navigationVisible = state;
		}

		private static void onToggledNodesToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.nodesVisible = state;
		}

		private static void onToggledItemsToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.itemsVisible = state;
		}

		private static void onToggledPlayersToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.playersVisible = state;
		}

		private static void onToggledZombiesToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.zombiesVisible = state;
		}

		private static void onToggledVehiclesToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.vehiclesVisible = state;
		}

		private static void onToggledBorderToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.borderVisible = state;
		}

		private static void onToggledAnimalsToggle(ISleekToggle toggle, bool state)
		{
			LevelVisibility.animalsVisible = state;
		}

		private static void onToggledDecalsToggle(ISleekToggle toggle, bool state)
		{
			DecalSystem.IsVisible = state;
		}

		private static void onRegionUpdated(byte old_x, byte old_y, byte new_x, byte new_y)
		{
			if (!active)
			{
				return;
			}

			update(new_x, new_y);
		}

		private static void update(int x, int y)
		{
			for (int offset_x = 0; offset_x < DEBUG_SIZE; offset_x++)
			{
				for (int offset_y = 0; offset_y < DEBUG_SIZE; offset_y++)
				{
					int index = (offset_x * DEBUG_SIZE) + offset_y;
					int region_x = x - (DEBUG_SIZE / 2) + offset_x;
					int region_y = y - (DEBUG_SIZE / 2) + offset_y;

					ISleekLabel regionLabel = regionLabels[index];

					if (Regions.checkSafe(region_x, region_y))
					{
						int count = LevelObjects.objects[region_x, region_y].Count + LevelGround.GetTreeCountInRegion(new Vector2Int(region_x, region_y));
						int total = LevelObjects.total + LevelGround.total;
						double density = System.Math.Round(count / (double) total * 1000) / 10.0;

						int tris = 0;
						for (int objectIndex = 0; objectIndex < LevelObjects.objects[region_x, region_y].Count; objectIndex++)
						{
							LevelObject obj = LevelObjects.objects[region_x, region_y][objectIndex];

							if (!obj.transform)
							{
								continue;
							}

							obj.transform.GetComponents(meshes);
							if (meshes.Count == 0)
							{
								Transform model_0 = obj.transform.Find("Model_0");

								if (model_0)
								{
									model_0.GetComponentsInChildren(true, meshes);
								}
							}

							if (meshes.Count == 0)
							{
								continue;
							}

							for (int meshIndex = 0; meshIndex < meshes.Count; meshIndex++)
							{
								Mesh mesh = meshes[meshIndex].sharedMesh;

								if (!mesh)
								{
									continue;
								}

								tris += mesh.triangles.Length;
							}
						}
						List<ResourceSpawnpoint> trees = LevelGround.GetTreesOrNullInRegion(new Vector2Int(region_x, region_y));
						if (trees != null)
						{
							foreach (ResourceSpawnpoint tree in trees)
							{
								if (!tree.model)
								{
									continue;
								}

								tree.model.GetComponents(meshes);
								if (meshes.Count == 0)
								{
									Transform model_0 = tree.model.Find("Model_0");

									if (model_0)
									{
										model_0.GetComponentsInChildren(true, meshes);
									}
								}

								if (meshes.Count == 0)
								{
									continue;
								}

								for (int meshIndex = 0; meshIndex < meshes.Count; meshIndex++)
								{
									Mesh mesh = meshes[meshIndex].sharedMesh;

									if (!mesh)
									{
										continue;
									}
									tris += mesh.triangles.Length;
								}
							}
						}

						long score = count * (long) tris;
						float complexity = Mathf.Clamp01((float) (1.0 - (score / 50000000.0)));

						regionLabel.Text = localization.format("Point", region_x, region_y);
						regionLabel.Text += "\n" + localization.format("Objects", count, density);
						regionLabel.Text += "\n" + localization.format("Triangles", tris);

						if (count == 0 && tris == 0)
						{
							regionLabel.TextColor = Color.white;
						}
						else
						{
							regionLabel.TextColor = ItemTool.getQualityColor(complexity);
						}
					}
				}
			}
		}

		public static void update()
		{
			for (int offset_x = 0; offset_x < DEBUG_SIZE; offset_x++)
			{
				for (int offset_y = 0; offset_y < DEBUG_SIZE; offset_y++)
				{
					int index = (offset_x * DEBUG_SIZE) + offset_y;
					int region_x = Editor.editor.area.region_x - (DEBUG_SIZE / 2) + offset_x;
					int region_y = Editor.editor.area.region_y - (DEBUG_SIZE / 2) + offset_y;

					ISleekLabel regionLabel = regionLabels[index];

					Vector3 worldPosition;
					if (Regions.tryGetPoint(region_x, region_y, out worldPosition))
					{
						Vector3 viewportPoint = MainCamera.instance.WorldToViewportPoint(worldPosition + new Vector3(Regions.REGION_SIZE / 2, 0, Regions.REGION_SIZE / 2));

						if (viewportPoint.z > 0)
						{
							Vector2 normalizedPosition = container.ViewportToNormalizedPosition(viewportPoint);
							regionLabel.PositionScale_X = normalizedPosition.x;
							regionLabel.PositionScale_Y = normalizedPosition.y;
							regionLabel.IsVisible = true;
						}
						else
						{
							regionLabel.IsVisible = false;
						}
					}
					else
					{
						regionLabel.IsVisible = false;
					}
				}
			}
		}

		public EditorLevelVisibilityUI()
		{
			localization = Localization.read("/Editor/EditorLevelVisibility.dat");

			container = new SleekFullscreenBox();
			container.PositionScale_X = 1;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			EditorUI.window.AddChild(container);
			active = false;

			roadsToggle = Glazier.Get().CreateToggle();
			roadsToggle.PositionOffset_X = -210;
			roadsToggle.PositionOffset_Y = 90;
			roadsToggle.PositionScale_X = 1;
			roadsToggle.SizeOffset_X = 40;
			roadsToggle.SizeOffset_Y = 40;
			roadsToggle.Value = LevelVisibility.roadsVisible;
			roadsToggle.AddLabel(localization.format("Roads_Label"), ESleekSide.RIGHT);
			roadsToggle.OnValueChanged += onToggledRoadsToggle;
			container.AddChild(roadsToggle);

			navigationToggle = Glazier.Get().CreateToggle();
			navigationToggle.PositionOffset_X = -210;
			navigationToggle.PositionOffset_Y = 140;
			navigationToggle.PositionScale_X = 1;
			navigationToggle.SizeOffset_X = 40;
			navigationToggle.SizeOffset_Y = 40;
			navigationToggle.Value = LevelVisibility.navigationVisible;
			navigationToggle.AddLabel(localization.format("Navigation_Label"), ESleekSide.RIGHT);
			navigationToggle.OnValueChanged += onToggledNavigationToggle;
			container.AddChild(navigationToggle);

			nodesToggle = Glazier.Get().CreateToggle();
			nodesToggle.PositionOffset_X = -210;
			nodesToggle.PositionOffset_Y = 190;
			nodesToggle.PositionScale_X = 1;
			nodesToggle.SizeOffset_X = 40;
			nodesToggle.SizeOffset_Y = 40;
			nodesToggle.Value = LevelVisibility.nodesVisible;
			nodesToggle.AddLabel(localization.format("Nodes_Label"), ESleekSide.RIGHT);
			nodesToggle.OnValueChanged += onToggledNodesToggle;
			container.AddChild(nodesToggle);

			itemsToggle = Glazier.Get().CreateToggle();
			itemsToggle.PositionOffset_X = -210;
			itemsToggle.PositionOffset_Y = 240;
			itemsToggle.PositionScale_X = 1;
			itemsToggle.SizeOffset_X = 40;
			itemsToggle.SizeOffset_Y = 40;
			itemsToggle.Value = LevelVisibility.itemsVisible;
			itemsToggle.AddLabel(localization.format("Items_Label"), ESleekSide.RIGHT);
			itemsToggle.OnValueChanged += onToggledItemsToggle;
			container.AddChild(itemsToggle);

			playersToggle = Glazier.Get().CreateToggle();
			playersToggle.PositionOffset_X = -210;
			playersToggle.PositionOffset_Y = 290;
			playersToggle.PositionScale_X = 1;
			playersToggle.SizeOffset_X = 40;
			playersToggle.SizeOffset_Y = 40;
			playersToggle.Value = LevelVisibility.playersVisible;
			playersToggle.AddLabel(localization.format("Players_Label"), ESleekSide.RIGHT);
			playersToggle.OnValueChanged += onToggledPlayersToggle;
			container.AddChild(playersToggle);

			zombiesToggle = Glazier.Get().CreateToggle();
			zombiesToggle.PositionOffset_X = -210;
			zombiesToggle.PositionOffset_Y = 340;
			zombiesToggle.PositionScale_X = 1;
			zombiesToggle.SizeOffset_X = 40;
			zombiesToggle.SizeOffset_Y = 40;
			zombiesToggle.Value = LevelVisibility.zombiesVisible;
			zombiesToggle.AddLabel(localization.format("Zombies_Label"), ESleekSide.RIGHT);
			zombiesToggle.OnValueChanged += onToggledZombiesToggle;
			container.AddChild(zombiesToggle);

			vehiclesToggle = Glazier.Get().CreateToggle();
			vehiclesToggle.PositionOffset_X = -210;
			vehiclesToggle.PositionOffset_Y = 390;
			vehiclesToggle.PositionScale_X = 1;
			vehiclesToggle.SizeOffset_X = 40;
			vehiclesToggle.SizeOffset_Y = 40;
			vehiclesToggle.Value = LevelVisibility.vehiclesVisible;
			vehiclesToggle.AddLabel(localization.format("Vehicles_Label"), ESleekSide.RIGHT);
			vehiclesToggle.OnValueChanged += onToggledVehiclesToggle;
			container.AddChild(vehiclesToggle);

			borderToggle = Glazier.Get().CreateToggle();
			borderToggle.PositionOffset_X = -210;
			borderToggle.PositionOffset_Y = 440;
			borderToggle.PositionScale_X = 1;
			borderToggle.SizeOffset_X = 40;
			borderToggle.SizeOffset_Y = 40;
			borderToggle.Value = LevelVisibility.borderVisible;
			borderToggle.AddLabel(localization.format("Border_Label"), ESleekSide.RIGHT);
			borderToggle.OnValueChanged += onToggledBorderToggle;
			container.AddChild(borderToggle);

			animalsToggle = Glazier.Get().CreateToggle();
			animalsToggle.PositionOffset_X = -210;
			animalsToggle.PositionOffset_Y = 490;
			animalsToggle.PositionScale_X = 1;
			animalsToggle.SizeOffset_X = 40;
			animalsToggle.SizeOffset_Y = 40;
			animalsToggle.Value = LevelVisibility.animalsVisible;
			animalsToggle.AddLabel(localization.format("Animals_Label"), ESleekSide.RIGHT);
			animalsToggle.OnValueChanged += onToggledAnimalsToggle;
			container.AddChild(animalsToggle);

			decalsToggle = Glazier.Get().CreateToggle();
			decalsToggle.PositionOffset_X = -210;
			decalsToggle.PositionOffset_Y = 540;
			decalsToggle.PositionScale_X = 1;
			decalsToggle.SizeOffset_X = 40;
			decalsToggle.SizeOffset_Y = 40;
			decalsToggle.Value = DecalSystem.IsVisible;
			decalsToggle.AddLabel(localization.format("Decals_Label"), ESleekSide.RIGHT);
			decalsToggle.OnValueChanged += onToggledDecalsToggle;
			container.AddChild(decalsToggle);

			regionLabels = new ISleekLabel[DEBUG_SIZE * DEBUG_SIZE];
			for (int index = 0; index < regionLabels.Length; index++)
			{
				ISleekLabel regionLabel = Glazier.Get().CreateLabel();
				regionLabel.PositionOffset_X = -100;
				regionLabel.PositionOffset_Y = -25;
				regionLabel.SizeOffset_X = 200;
				regionLabel.SizeOffset_Y = 50;
				regionLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
				regionLabels[index] = regionLabel;
				container.AddChild(regionLabel);
			}

			Editor.editor.area.onRegionUpdated += onRegionUpdated;
		}
	}
}
