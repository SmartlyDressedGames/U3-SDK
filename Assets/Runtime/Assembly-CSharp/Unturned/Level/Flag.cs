////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Flag
	{
		public static readonly float MIN_SIZE = 32;
		public static readonly float MAX_SIZE = 1024;

		public float width;
		public float height;

		private Vector3 _point;
		public Vector3 point => _point;

		private Transform _model;
		public Transform model => _model;

		public MeshFilter VisualizationMeshFilter
		{
			get;
			private set;
		}

		private LineRenderer _area;
		public LineRenderer area => _area;

		private LineRenderer _bounds;
		public LineRenderer bounds => _bounds;

		public IUnturnedNavmeshInterface navmeshInterface
		{
			get;
			private set;
		}

		public IUnturnedPerNavmeshEditorInterface EditorFlagInterface
		{
			get;
			private set;
		}

		public FlagData data
		{
			get;
			private set;
		}

		public bool needsNavigationSave;

		public void move(Vector3 newPoint)
		{
			_point = newPoint;
			model.position = point;
			VisualizationMeshFilter.transform.position = Vector3.zero;
		}

		public void setEnabled(bool isEnabled)
		{
			model.gameObject.SetActive(isEnabled);
		}

		public void buildMesh()
		{
			float x = MIN_SIZE + (width * (MAX_SIZE - MIN_SIZE));
			float y = MIN_SIZE + (height * (MAX_SIZE - MIN_SIZE));

			area.SetPosition(0, new Vector3(-x / 2, 0, -y / 2));
			area.SetPosition(1, new Vector3(x / 2, 0, -y / 2));
			area.SetPosition(2, new Vector3(x / 2, 0, y / 2));
			area.SetPosition(3, new Vector3(-x / 2, 0, y / 2));
			area.SetPosition(4, new Vector3(-x / 2, 0, -y / 2));

			x += LevelNavigation.BOUNDS_SIZE.x;
			y += LevelNavigation.BOUNDS_SIZE.z;

			bounds.SetPosition(0, new Vector3(-x / 2, 0, -y / 2));
			bounds.SetPosition(1, new Vector3(x / 2, 0, -y / 2));
			bounds.SetPosition(2, new Vector3(x / 2, 0, y / 2));
			bounds.SetPosition(3, new Vector3(-x / 2, 0, y / 2));
			bounds.SetPosition(4, new Vector3(-x / 2, 0, -y / 2));
		}

		public void remove()
		{
			EditorFlagInterface.OnDestroy();
			Object.Destroy(model.gameObject);
		}

		public Bounds CalculateBakingBounds()
		{
			float x = MIN_SIZE + (width * (MAX_SIZE - MIN_SIZE));
			float y = MIN_SIZE + (height * (MAX_SIZE - MIN_SIZE));

			Vector3 forcedBoundsCenter;
			Vector3 forcedBoundsSize;
			if (Level.info.configData.Use_Legacy_Water && LevelLighting.seaLevel < 0.99f && !Level.info.configData.Allow_Underwater_Features)
			{
				forcedBoundsCenter = new Vector3(point.x, (LevelLighting.seaLevel * Level.TERRAIN) + ((Level.TERRAIN - (LevelLighting.seaLevel * Level.TERRAIN)) / 2f) - 0.625f, point.z);
				forcedBoundsSize = new Vector3(x, Level.TERRAIN - (LevelLighting.seaLevel * Level.TERRAIN) + 1.25f, y);
			}
			else
			{
				forcedBoundsCenter = new Vector3(point.x, 0, point.z);
				forcedBoundsSize = new Vector3(x, SDG.Framework.Landscapes.Landscape.TILE_HEIGHT, y);
			}

			return new Bounds(forcedBoundsCenter, forcedBoundsSize);
		}

		public void bakeNavigation()
		{
#if !DEDICATED_SERVER
			CullingVolumeManager.Get().ImmediatelySyncAllVolumes();
#endif // !DEDICATED_SERVER
			LevelObjects.ImmediatelySyncRegionalVisibility();
			LevelRoads.ImmediatelySyncRegionalVisibility();

			EditorFlagInterface.Bake();

			LevelNavigation.updateBounds();
		}

		public Flag(Vector3 newPoint, IUnturnedNavmeshInterface newNavmesh, FlagData newData)
		{
			_point = newPoint;

			_model = (Object.Instantiate(Resources.Load<GameObject>("Edit/Flag"))).transform;
			model.name = "Flag";
			model.position = point;
			_area = model.Find("Area").GetComponent<LineRenderer>();
			_bounds = model.Find("Bounds").GetComponent<LineRenderer>();

			VisualizationMeshFilter = model.Find("Navmesh").GetComponent<MeshFilter>();

			width = 0;
			height = 0;

			navmeshInterface = newNavmesh;
			data = newData;

			buildMesh();

			EditorFlagInterface = UnturnedPathfinding.Get().CreateFlag(this);
		}

		public Flag(Vector3 newPoint, float newWidth, float newHeight, IUnturnedNavmeshInterface newNavmesh, FlagData newData)
		{
			_point = newPoint;

			_model = (Object.Instantiate(Resources.Load<GameObject>("Edit/Flag"))).transform;
			model.name = "Flag";
			model.position = point;
			_area = model.Find("Area").GetComponent<LineRenderer>();
			_bounds = model.Find("Bounds").GetComponent<LineRenderer>();

			VisualizationMeshFilter = model.Find("Navmesh").GetComponent<MeshFilter>();

			width = newWidth;
			height = newHeight;

			navmeshInterface = newNavmesh;
			data = newData;

			buildMesh();

			EditorFlagInterface = UnturnedPathfinding.Get().CreateFlag(this);
		}
	}
}
