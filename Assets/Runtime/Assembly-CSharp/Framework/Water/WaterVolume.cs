////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Water
{
	public partial class WaterVolume : LevelVolume<WaterVolume, WaterVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		public override ELevelVolumeShape Shape
		{
			get => base.Shape;
			set
			{
				base.Shape = value;
				SyncWaterPlaneActive();
			}
		}

		public static readonly int WATER_SURFACE_TILE_SIZE = 1024;

		public GameObject waterPlane;
		/// <summary>
		/// All water tiles and the planar reflection component reference this material.
		/// </summary>
		public Material sharedMaterial;

		[SerializeField]
		protected bool _isSurfaceVisible = true;
		protected bool _editorIsSufaceVisible = true;
		public bool isSurfaceVisible
		{
			get => _isSurfaceVisible;
			set
			{
				_isSurfaceVisible = value;
				SyncWaterPlaneActive();
			}
		}

		[SerializeField]
		protected bool _isReflectionVisible;
		public bool isReflectionVisible
		{
			get => _isReflectionVisible;
			set
			{
				_isReflectionVisible = value;
				SyncPlanarReflectionEnabled();
			}
		}

		[SerializeField]
		protected bool _isSeaLevel;
		/// <summary>
		/// If true rain will be occluded below the surface on the Y axis.
		/// </summary>
		public bool isSeaLevel
		{
			get => _isSeaLevel;
			set
			{
				_isSeaLevel = value;

				if (isSeaLevel)
				{
					WaterVolumeManager.seaLevelVolume = this;
				}
			}
		}

		/// <summary>
		/// Not using CachingAssetRef so it works with copy-paste. (Can we implement serialization for CachingAssetRef?)
		/// </summary>
		[SerializeField]
		internal System.Guid _fishSpawnTableGuid;

		public CachingAssetRef FishSpawnTableRef
		{
			get => new CachingAssetRef(_fishSpawnTableGuid);
			set
			{
				_fishSpawnTableGuid = value.Guid;
			}
		}

		public SpawnAsset GetFishSpawnTable()
		{
			return FishSpawnTableRef.Get<SpawnAsset>();
		}

		[SerializeField]
		private float _fishingMinimumDepthOverride = -1;
		/// <summary>
		/// If set, depth at bobber must exceed this to catch fish.
		/// </summary>
		public float FishingMinimumDepthOverride
		{
			get => _fishingMinimumDepthOverride;
			set => _fishingMinimumDepthOverride = value;
		}

		/// <summary>
		/// Flag for legacy sea level.
		/// </summary>
		internal bool isManagedByLighting;

		public ERefillWaterType waterType = ERefillWaterType.SALTY;

		public override void UpdateEditorVisibility(ELevelVolumeVisibility visibility)
		{
			base.UpdateEditorVisibility(visibility);

			// In hidden mode we do not want to see water, and in solid mode we use the editor mesh.
			_editorIsSufaceVisible = visibility != ELevelVolumeVisibility.Solid && LevelLighting.EditorWantsWaterSurface;

			SyncWaterPlaneActive();
		}

		internal void SyncWaterQuality()
		{
			if (sharedMaterial == null)
				return;

			EGraphicQuality waterQuality = GraphicsSettings.waterQuality;
			if (waterQuality == EGraphicQuality.LOW)
			{
				sharedMaterial.shader.maximumLOD = 201;
			}
			else if (waterQuality == EGraphicQuality.MEDIUM)
			{
				sharedMaterial.shader.maximumLOD = 301;
			}
			else if (waterQuality == EGraphicQuality.HIGH || waterQuality == EGraphicQuality.ULTRA)
			{
				sharedMaterial.shader.maximumLOD = 501;
			}
		}

		partial void SyncPlanarReflectionEnabledPartial();

		internal void SyncPlanarReflectionEnabled()
		{
			SyncPlanarReflectionEnabledPartial();
		}

		private void SyncWaterPlaneActive()
		{
			if (waterPlane != null)
			{
				waterPlane.SetActive(isSurfaceVisible && _editorIsSufaceVisible && Shape == ELevelVolumeShape.Box);
			}
		}

		partial void CreateUnity4Water();

		private void CreateFallbackWaterPlanes()
		{
			waterPlane = new GameObject("Plane");
			waterPlane.transform.parent = transform;
			waterPlane.transform.SetLocalPositionAndRotation(new Vector3(0f, 0.5f, 0f), Quaternion.identity);
			waterPlane.transform.localScale = Vector3.one;

			int size_x = Mathf.Max(1, Mathf.FloorToInt(transform.localScale.x / WATER_SURFACE_TILE_SIZE));
			int size_y = Mathf.Max(1, Mathf.FloorToInt(transform.localScale.z / WATER_SURFACE_TILE_SIZE));

			float scale_x = 1f / size_x;
			float scale_y = 1f / size_y;

			GameObject waterTilePrefab = Resources.Load<GameObject>("Level/Water_Fallback");
			InstantiateParameters instantiateParameters = new InstantiateParameters()
			{
				parent = waterPlane.transform,
				worldSpace = false,
			};
			for (int tile_x = 0; tile_x < size_x; tile_x++)
			{
				for (int tile_y = 0; tile_y < size_y; tile_y++)
				{
					Vector3 position = new Vector3(-0.5f + (scale_x / 2) + (tile_x * scale_x),
																	 0,
																	 -0.5f + (scale_y / 2) + (tile_y * scale_y));

					GameObject waterTile = Instantiate(waterTilePrefab, position, Quaternion.identity, instantiateParameters);
					waterTile.name = "Tile_" + tile_x + "_" + tile_y;
					waterTile.transform.localScale = new Vector3(0.1f * scale_x, 1.0f, 0.1f * scale_y);
					if (sharedMaterial == null)
					{
						sharedMaterial = waterTile.GetComponent<Renderer>().material;
					}
					else
					{
						waterTile.GetComponent<Renderer>().material = sharedMaterial;
					}
				}
			}
		}

		protected void createWaterPlanes()
		{
			if (!Dedicator.IsDedicatedServer && waterPlane == null)
			{
				CreateUnity4Water();
				// Enables us to fallback without preprocessor code specific to SDK release.
				if (waterPlane == null)
				{
					CreateFallbackWaterPlanes();
				}

				SyncWaterQuality();
				SyncPlanarReflectionEnabled();
				SyncWaterPlaneActive();
				LevelLighting.updateLighting();
			}
		}

		public void beginCollision(Collider collider)
		{
			if (collider == null)
			{
				return;
			}

			IWaterVolumeInteractionHandler handler = collider.gameObject.GetComponent<IWaterVolumeInteractionHandler>();
			if (handler != null)
			{
				handler.waterBeginCollision(this);
			}
		}

		public void endCollision(Collider collider)
		{
			if (collider == null)
			{
				return;
			}

			IWaterVolumeInteractionHandler handler = collider.gameObject.GetComponent<IWaterVolumeInteractionHandler>();
			if (handler != null)
			{
				handler.waterEndCollision(this);
			}
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			isSurfaceVisible = reader.readValue<bool>("Is_Surface_Visible");
			isReflectionVisible = reader.readValue<bool>("Is_Reflection_Visible");
			isSeaLevel = reader.readValue<bool>("Is_Sea_Level");
			waterType = reader.readValue<ERefillWaterType>("Water_Type");
			_fishSpawnTableGuid = reader.readValue<System.Guid>("Fish_Spawn_Table");
			if (reader.containsKey("Fishing_Minimum_Depth_Override"))
			{
				_fishingMinimumDepthOverride = reader.readValue<float>("Fishing_Minimum_Depth_Override");
			}

			createWaterPlanes();
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Is_Surface_Visible", isSurfaceVisible);
			writer.writeValue("Is_Reflection_Visible", isReflectionVisible);
			writer.writeValue("Is_Sea_Level", isSeaLevel);
			writer.writeValue("Water_Type", waterType);
			writer.writeValue("Fish_Spawn_Table", _fishSpawnTableGuid);
			if (_fishingMinimumDepthOverride > -0.5f)
			{
				writer.writeValue("Fishing_Minimum_Depth_Override", _fishingMinimumDepthOverride);
			}
		}

		public override bool ShouldSave => !isManagedByLighting;
		public override bool CanBeSelected => !isManagedByLighting;

		public void OnTriggerEnter(Collider other)
		{
			beginCollision(other);
		}

		public void OnTriggerExit(Collider other)
		{
			endCollision(other);
		}

		protected override void Awake()
		{
			forceShouldAddCollider = true; // Used by gameplay for bullet casing particle sound effects.
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			createWaterPlanes();
		}

		public string OnGetFishErrorContext()
		{
			return $"water volume at {transform.position}";
		}

		private class Menu : SleekWrapper
		{
			public Menu(WaterVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 230;

				ISleekToggle isSurfaceVisibleToggle = Glazier.Get().CreateToggle();
				isSurfaceVisibleToggle.PositionOffset_Y = -30;
				isSurfaceVisibleToggle.SizeOffset_X = 40;
				isSurfaceVisibleToggle.SizeOffset_Y = 40;
				isSurfaceVisibleToggle.Value = volume.isSurfaceVisible;
				isSurfaceVisibleToggle.AddLabel("Surface Visible", ESleekSide.RIGHT);
				isSurfaceVisibleToggle.OnValueChanged += OnIsSurfaceVisibleToggled;
				AddChild(isSurfaceVisibleToggle);

				ISleekToggle isReflectionVisibleToggle = Glazier.Get().CreateToggle();
				isReflectionVisibleToggle.PositionOffset_Y = 10;
				isReflectionVisibleToggle.SizeOffset_X = 40;
				isReflectionVisibleToggle.SizeOffset_Y = 40;
				isReflectionVisibleToggle.Value = volume.isReflectionVisible;
				isReflectionVisibleToggle.AddLabel("Reflection Visible", ESleekSide.RIGHT);
				isReflectionVisibleToggle.OnValueChanged += OnIsReflectionVisibleToggled;
				AddChild(isReflectionVisibleToggle);

				ISleekToggle isSeaLevelToggle = Glazier.Get().CreateToggle();
				isSeaLevelToggle.PositionOffset_Y = 50;
				isSeaLevelToggle.SizeOffset_X = 40;
				isSeaLevelToggle.SizeOffset_Y = 40;
				isSeaLevelToggle.Value = volume.isSeaLevel;
				isSeaLevelToggle.AddLabel("Sea Level", ESleekSide.RIGHT);
				isSeaLevelToggle.OnValueChanged += OnIsSeaLevelToggled;
				AddChild(isSeaLevelToggle);

				SleekButtonState refillButton = new SleekButtonState(new GUIContent("Clean"), new GUIContent("Salty"), new GUIContent("Dirty"));
				refillButton.PositionOffset_Y = 90;
				refillButton.SizeOffset_X = 200;
				refillButton.SizeOffset_Y = 30;
				refillButton.AddLabel("Refill Type", ESleekSide.RIGHT);
				refillButton.state = (int) volume.waterType - 1;
				refillButton.onSwappedState += OnSwappedWaterType;
				AddChild(refillButton);

				SleekAssetField fishField = new SleekAssetField();
				fishField.PositionOffset_Y = 130;
				fishField.SizeOffset_X = 200;
				fishField.SizeOffset_Y = 60;
				fishField.Value = volume.FishSpawnTableRef;
				fishField.AddLabel("Fish", ESleekSide.RIGHT);
				fishField.OnValueChanged += OnFishSpawnTableChanged;
				AddChild(fishField);

				ISleekFloat32Field depthField = Glazier.Get().CreateFloat32Field();
				depthField.PositionOffset_Y = 200;
				depthField.SizeOffset_X = 200;
				depthField.SizeOffset_Y = 30;
				depthField.Value = volume.FishingMinimumDepthOverride;
				depthField.AddLabel("Fishing Minimum Depth Override", ESleekSide.RIGHT);
				depthField.OnValueChanged += OnMinimumDepthChanged;
				AddChild(depthField);
			}

			private void OnIsSurfaceVisibleToggled(ISleekToggle toggle, bool state)
			{
				volume.isSurfaceVisible = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnIsReflectionVisibleToggled(ISleekToggle toggle, bool state)
			{
				volume.isReflectionVisible = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnIsSeaLevelToggled(ISleekToggle toggle, bool state)
			{
				volume.isSeaLevel = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnSwappedWaterType(SleekButtonState button, int state)
			{
				volume.waterType = (ERefillWaterType) (state + 1);
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnFishSpawnTableChanged(SleekAssetField field)
			{
				volume.FishSpawnTableRef = field.Value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnMinimumDepthChanged(ISleekFloat32Field field, float value)
			{
				volume.FishingMinimumDepthOverride = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private WaterVolume volume;
		}
	}
}
