////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned
{
	internal class EditorInteract : MonoBehaviour
	{
		public static readonly byte SAVEDATA_VERSION = 1;

		private static bool _isFlying;
		public static bool isFlying => _isFlying;

		private static Ray _ray;
		public static Ray ray => _ray;

		private static RaycastHit _worldHit;
		public static RaycastHit worldHit => _worldHit;

		private static RaycastHit _objectHit;
		public static RaycastHit objectHit => _objectHit;

		private static RaycastHit _logicHit;
		public static RaycastHit logicHit => _logicHit;

		public static EditorInteract instance;
		private IDevkitTool activeTool;
		public TerrainEditor terrainTool;

		public void SetActiveTool(IDevkitTool tool)
		{
			if (activeTool != null)
			{
				activeTool.dequip();
			}

			activeTool = tool;

			if (activeTool != null)
			{
				activeTool.equip();
			}
		}

		private void Update()
		{
			if (Glazier.Get().ShouldGameProcessInput)
			{
				_isFlying = InputEx.GetKey(ControlsSettings.secondary);
			}
			else
			{
				_isFlying = false;
			}

			_ray = MainCamera.instance.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray, out _worldHit, 2048, RayMasks.EDITOR_WORLD);
			Physics.Raycast(ray, out _objectHit, 2048, RayMasks.EDITOR_INTERACT);
			Physics.Raycast(ray, out _logicHit, 2048, RayMasks.EDITOR_LOGIC);

			if (InputEx.GetKeyDown(KeyCode.S) && InputEx.GetKey(KeyCode.LeftControl))
			{
				Level.save();
			}

			if (InputEx.GetKeyDown(KeyCode.F1))
			{
				LevelVisibility.roadsVisible = !LevelVisibility.roadsVisible;
				EditorLevelVisibilityUI.roadsToggle.Value = LevelVisibility.roadsVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F2))
			{
				LevelVisibility.navigationVisible = !LevelVisibility.navigationVisible;
				EditorLevelVisibilityUI.navigationToggle.Value = LevelVisibility.navigationVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F3))
			{
				LevelVisibility.nodesVisible = !LevelVisibility.nodesVisible;
				EditorLevelVisibilityUI.nodesToggle.Value = LevelVisibility.nodesVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F4))
			{
				LevelVisibility.itemsVisible = !LevelVisibility.itemsVisible;
				EditorLevelVisibilityUI.itemsToggle.Value = LevelVisibility.itemsVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F5))
			{
				LevelVisibility.playersVisible = !LevelVisibility.playersVisible;
				EditorLevelVisibilityUI.playersToggle.Value = LevelVisibility.playersVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F6))
			{
				LevelVisibility.zombiesVisible = !LevelVisibility.zombiesVisible;
				EditorLevelVisibilityUI.zombiesToggle.Value = LevelVisibility.zombiesVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F7))
			{
				LevelVisibility.vehiclesVisible = !LevelVisibility.vehiclesVisible;
				EditorLevelVisibilityUI.vehiclesToggle.Value = LevelVisibility.vehiclesVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F8))
			{
				LevelVisibility.borderVisible = !LevelVisibility.borderVisible;
				EditorLevelVisibilityUI.borderToggle.Value = LevelVisibility.borderVisible;
			}

			if (InputEx.GetKeyDown(KeyCode.F9))
			{
				LevelVisibility.animalsVisible = !LevelVisibility.animalsVisible;
				EditorLevelVisibilityUI.animalsToggle.Value = LevelVisibility.animalsVisible;
			}

			if (activeTool != null)
			{
				activeTool.update();

				// 2022-05-19 only process "devkit" undo/redo while devkit tool is equipped. Regular editor handles undo/redo separately.
				if (InputEx.GetKeyDown(KeyCode.Z))
				{
#if !UNITY_EDITOR
				if(InputEx.GetKey(KeyCode.LeftControl))
				{
#endif
					if (InputEx.GetKey(KeyCode.LeftShift))
					{
						SDG.Framework.Devkit.Transactions.DevkitTransactionManager.redo();
					}
					else
					{
						SDG.Framework.Devkit.Transactions.DevkitTransactionManager.undo();
					}
#if !UNITY_EDITOR
				}
#endif
				}
			}
		}

		private void OnDisable()
		{
			// Ensure tool is cleaned up for example no more drawing to viewport.
			SetActiveTool(null);
		}

		private void Start()
		{
			load();

			instance = this;
			terrainTool = new TerrainEditor();
		}

		public static void load()
		{
			if (ReadWrite.fileExists(Level.info.path + "/Editor/Camera.dat", false, false))
			{
				Block block = ReadWrite.readBlock(Level.info.path + "/Editor/Camera.dat", false, false, 1);

				MainCamera.instance.transform.parent.position = block.readSingleVector3();
				MainCamera.instance.transform.localRotation = Quaternion.Euler(block.readSingle(), 0, 0);
				MainCamera.instance.transform.parent.rotation = Quaternion.Euler(0, block.readSingle(), 0);
			}
			else
			{
				MainCamera.instance.transform.parent.position = new Vector3(0, Level.TERRAIN, 0);
				MainCamera.instance.transform.parent.rotation = Quaternion.identity;
				MainCamera.instance.transform.localRotation = Quaternion.identity;
			}
		}

		public static void save()
		{
			Block block = new Block();
			block.writeByte(SAVEDATA_VERSION);

			block.writeSingleVector3(MainCamera.instance.transform.position);
			block.writeSingle(EditorLook.pitch);
			block.writeSingle(EditorLook.yaw);

			ReadWrite.writeBlock(Level.info.path + "/Editor/Camera.dat", false, false, block);
		}
	}
}
