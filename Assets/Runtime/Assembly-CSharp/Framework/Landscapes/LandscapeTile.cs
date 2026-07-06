////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Debug;
using SDG.Framework.Foliage;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.Utilities;
using SDG.Unturned;
using System.IO;
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public class LandscapeTile : IFormattedFileReadable, IFormattedFileWritable, IFoliageSurface
	{
		public GameObject gameObject
		{
			get;
			protected set;
		}

		protected LandscapeCoord _coord;
		public LandscapeCoord coord
		{
			get => _coord;
			protected set
			{
				_coord = value;
				updateTransform();
			}
		}

		public Bounds localBounds => new Bounds(new Vector3(Landscape.TILE_SIZE / 2, 0, Landscape.TILE_SIZE / 2),
								  new Vector3(Landscape.TILE_SIZE, Landscape.TILE_HEIGHT, Landscape.TILE_SIZE));

		public Bounds worldBounds
		{
			get
			{
				Bounds bounds = localBounds;
				bounds.center += new Vector3(coord.x * Landscape.TILE_SIZE, 0, coord.y * Landscape.TILE_SIZE);
				return bounds;
			}
		}

		public float[,] heightmap;
		public float[,,] splatmap;

		/// <summary>
		/// True is solid and false is empty.
		/// </summary>
		public bool[,] holes;

		/// <summary>
		/// Marked true when level editor or legacy hole volumes modify hole data.
		/// Defaults to false in which case holes do not need to be saved.
		/// 
		/// Initially this was not going to be marked by hole volumes because they can re-generate the holes, but saving
		/// hole volume cuts is helpful when upgrading to remove hole volumes from a map.
		/// </summary>
		public bool hasAnyHolesData;

		/// <summary>
		/// If true, SetHeightsDelayLOD was called without calling SyncHeightmap yet.
		/// </summary>
		private bool isHeightsLodDataDirty;

		/// <summary>
		/// If true, SetHolesDelayLOD was called without calling SyncTexture yet.
		/// </summary>
		private bool isHolesLodDataDirty;

		public InspectableList<AssetReference<LandscapeMaterialAsset>> materials
		{
			get;
			protected set;
		}

		private TerrainLayer[] terrainLayers;

		public TerrainData data
		{
			get;
			protected set;
		}

		/// <summary>
		/// Heightmap-only data used in level editor. Refer to Landscape.DisableHoleColliders for explanation.
		/// </summary>
		public TerrainData dataWithoutHoles;

		public Terrain terrain
		{
			get;
			protected set;
		}

		public TerrainCollider collider
		{
			get;
			protected set;
		}

		public void SetHeightsDelayLOD()
		{
			data.SetHeightsDelayLOD(0, 0, heightmap);
			if (dataWithoutHoles != null)
			{
				dataWithoutHoles.SetHeightsDelayLOD(0, 0, heightmap);
			}
			isHeightsLodDataDirty = true;
		}

		public void SyncDelayedLOD()
		{
			if (isHeightsLodDataDirty)
			{
				isHeightsLodDataDirty = false;

				data.SyncHeightmap();
				if (dataWithoutHoles != null)
				{
					dataWithoutHoles.SyncHeightmap();
				}
			}

			if (isHolesLodDataDirty)
			{
				isHolesLodDataDirty = false;
				data.SyncTexture(TerrainData.HolesTextureName);
			}
		}

		public void SetHoles()
		{
			if (Landscape.ShouldUseSetHolesDelayLOD)
			{
				data.SetHolesDelayLOD(0, 0, holes);
				isHolesLodDataDirty = true;
			}
			else
			{
				data.SetHoles(0, 0, holes);
			}
		}

		public virtual void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();
			coord = reader.readValue<LandscapeCoord>("Coord");
			UpdateNames();

			bool useEditorAssetRedirector = Level.isEditor && EditorAssetRedirector.HasRedirects;

			int layerCount = reader.readArrayLength("Materials");
			for (int layer = 0; layer < layerCount; layer++)
			{
				AssetReference<LandscapeMaterialAsset> materialRef = reader.readValue<AssetReference<LandscapeMaterialAsset>>(layer);
				if (materialRef.isValid)
				{
					if (Level.shouldUseHolidayRedirects)
					{
						LandscapeMaterialAsset originalMaterial = materialRef.Find();
						if (originalMaterial != null)
						{
							AssetReference<LandscapeMaterialAsset> redirectedMaterialRef = originalMaterial.getHolidayRedirect();
							if (redirectedMaterialRef.isValid)
							{
								materialRef = redirectedMaterialRef;
							}
						}
					}
					else if (useEditorAssetRedirector)
					{
						LandscapeMaterialAsset redirect = EditorAssetRedirector.Redirect<LandscapeMaterialAsset>(materialRef.GUID);
						if (redirect != null)
						{
							materialRef = redirect.getReferenceTo<LandscapeMaterialAsset>();
						}
					}

					// If material is missing on the server then do not kick clients for missing it as well.
					LandscapeMaterialAsset material = materialRef.Find();
					if (material == null)
					{
						ClientAssetIntegrity.ServerAddKnownMissingAsset(materialRef.GUID, $"Landscape tile (x: {coord.x} y: {coord.y} layer: {layer})");
					}
				}

				materials[layer] = materialRef;
			}
			updatePrototypes();

			readHeightmaps();
			readSplatmaps(); // Call on server for hash.
			ReadHoles();
		}

		public virtual void readHeightmaps()
		{
			readHeightmap("_Source", heightmap);
			SetHeightsDelayLOD();
		}

		protected virtual void readHeightmap(string suffix, float[,] heightmap)
		{
			string fileName = "Tile_" + coord.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + '_' + coord.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + suffix + ".heightmap";
			string filePath = Level.info.path + "/Landscape/Heightmaps/" + fileName;

			if (!File.Exists(filePath))
			{
				UnturnedLog.warn("LandscapeTile missing heightmap file: {0}", filePath);
				return;
			}

			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (SHA1Stream hashStream = new SHA1Stream(stream))
			{
				for (int x = 0; x < Landscape.HEIGHTMAP_RESOLUTION; x++)
				{
					for (int y = 0; y < Landscape.HEIGHTMAP_RESOLUTION; y++)
					{
						ushort raw = (ushort) ((hashStream.ReadByte() << 8) | hashStream.ReadByte());
						float height = raw / (float) ushort.MaxValue;

						heightmap[x, y] = height;
					}
				}

				Level.includeHash(fileName, hashStream.Hash);
			}
		}

		public virtual void readSplatmaps()
		{
			readSplatmap("_Source", splatmap); // Call on server for hash.

			if (!Dedicator.IsDedicatedServer)
			{
				data.SetAlphamaps(0, 0, splatmap);
			}
		}

		protected virtual void readSplatmap(string suffix, float[,,] splatmap)
		{
			string fileName = "Tile_" + coord.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + '_' + coord.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + suffix + ".splatmap";
			string filePath = Level.info.path + "/Landscape/Splatmaps/" + fileName;

			if (!File.Exists(filePath))
			{
				UnturnedLog.warn("LandscapeTile missing splatmap file: {0}", filePath);
				return;
			}

			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (SHA1Stream hashStream = new SHA1Stream(stream))
			{
				for (int x = 0; x < Landscape.SPLATMAP_RESOLUTION; x++)
				{
					for (int y = 0; y < Landscape.SPLATMAP_RESOLUTION; y++)
					{
						for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
						{
							byte raw = (byte) hashStream.ReadByte();
							float splat = raw / (float) byte.MaxValue;

							splatmap[x, y, layer] = splat;
						}
					}
				}

				// Prevent players from painting the terrain black.
				Level.includeHash(fileName, hashStream.Hash);
			}
		}

		private void ReadHoles()
		{
			string fileName = "Tile_" + coord.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + '_' + coord.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".bin";
			string filePath = Level.info.path + "/Landscape/Holes/" + fileName;

			if (!File.Exists(filePath))
			{
				// Do not warn because old maps as well as maps without holes do not need this file.
				return;
			}

			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (SHA1Stream hashStream = new SHA1Stream(stream))
			{
				int version = hashStream.ReadByte();

				for (int x = 0; x < Landscape.HOLES_RESOLUTION; ++x)
				{
					for (int y = 0; y < Landscape.HOLES_RESOLUTION; y += 8)
					{
						byte value = (byte) hashStream.ReadByte();
						for (int offset = 0; offset < 8; ++offset)
						{
							holes[x, y + offset] = (value & (1 << offset)) > 0;
						}
					}
				}

				// Prevent players from painting the terrain black.
				Level.includeHash(fileName, hashStream.Hash);
			}

			SetHoles();
			hasAnyHolesData = true;
		}

		public virtual void write(IFormattedFileWriter writer)
		{
			writer.beginObject();

			writer.writeValue("Coord", coord);

			writer.beginArray("Materials");
			for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				writer.writeValue(materials[layer]);
			}
			writer.endArray();

			writer.endObject();

			writeHeightmaps();
			writeSplatmaps();

			if (hasAnyHolesData)
			{
				WriteHoles();
			}
		}

		public virtual void writeHeightmaps()
		{
			writeHeightmap("_Source", heightmap);
		}

		protected virtual void writeHeightmap(string suffix, float[,] heightmap)
		{
			string filePath = Level.info.path + "/Landscape/Heightmaps/Tile_" + coord.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + '_' + coord.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + suffix + ".heightmap";
			string directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
			{
				for (int x = 0; x < Landscape.HEIGHTMAP_RESOLUTION; x++)
				{
					for (int y = 0; y < Landscape.HEIGHTMAP_RESOLUTION; y++)
					{
						float height = heightmap[x, y];
						ushort raw = (ushort) Mathf.RoundToInt(height * ushort.MaxValue);

						stream.WriteByte((byte) ((raw >> 8) & 0xFF));
						stream.WriteByte((byte) (raw & 0xFF));
					}
				}
			}
		}

		public virtual void writeSplatmaps()
		{
			writeSplatmap("_Source", splatmap);
		}

		protected virtual void writeSplatmap(string suffix, float[,,] splatmap)
		{
			string filePath = Level.info.path + "/Landscape/Splatmaps/Tile_" + coord.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + '_' + coord.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + suffix + ".splatmap";
			string directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
			{
				for (int x = 0; x < Landscape.SPLATMAP_RESOLUTION; x++)
				{
					for (int y = 0; y < Landscape.SPLATMAP_RESOLUTION; y++)
					{
						for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
						{
							float splat = splatmap[x, y, layer];
							byte raw = (byte) Mathf.RoundToInt(splat * byte.MaxValue);

							stream.WriteByte(raw);
						}
					}
				}
			}
		}

		private void WriteHoles()
		{
			string filePath = Level.info.path + "/Landscape/Holes/Tile_" + coord.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + '_' + coord.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".bin";
			string directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
			{
				stream.WriteByte(1); // Version number just in case.

				for (int x = 0; x < Landscape.HOLES_RESOLUTION; ++x)
				{
					for (int y = 0; y < Landscape.HOLES_RESOLUTION; y += 8)
					{
						byte value = holes[x, y] ? (byte) 1 : (byte) 0;
						for (int offset = 1; offset < 8; ++offset)
						{
							value |= holes[x, y + offset] ? (byte) (1 << offset) : (byte) 0;
						}

						stream.WriteByte(value);
					}
				}
			}
		}

		/// <summary>
		/// Call this when done changing material references to grab their textures and pass them to the terrain renderer.
		/// </summary>
		public void updatePrototypes()
		{
			if (Dedicator.IsDedicatedServer)
			{
				// Linux headless server crashes when setting splat prototypes.
				return;
			}

			for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				AssetReference<LandscapeMaterialAsset> materialAssetRef = materials[layer];
				LandscapeMaterialAsset materialAsset = materialAssetRef.Find();

				// Prevent players from replacing with black textures. (if ref is not null, asset can be null e.g. player deleted it)
				// Server loads which terrain materials are used as well so it will not kick if an asset is known missing.
				if (materialAssetRef.isValid)
				{
					ClientAssetIntegrity.QueueRequest(materialAssetRef.GUID, materialAsset, $"Landscape tile (x: {coord.x} y: {coord.y} layer: {layer})");
				}

				if (materialAsset == null)
				{
					terrainLayers[layer] = null;
				}
				else
				{
					terrainLayers[layer] = materialAsset.getOrCreateLayer();
				}
			}

			data.terrainLayers = terrainLayers;
		}

		protected void updateTransform()
		{
			gameObject.transform.position = new Vector3(coord.x * Landscape.TILE_SIZE, -Landscape.TILE_HEIGHT / 2, coord.y * Landscape.TILE_SIZE);
		}

		public void convertLegacyHeightmap()
		{
			for (int x = 0; x < Landscape.HEIGHTMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.HEIGHTMAP_RESOLUTION; y++)
				{
					HeightmapCoord heightmapCoord = new HeightmapCoord(x, y);
					Vector3 worldPosition = Landscape.getWorldPosition(coord, heightmapCoord, heightmap[x, y]);

					float legacyHeight = LevelGround.getConversionHeight(worldPosition);
					legacyHeight /= Landscape.TILE_HEIGHT;
					legacyHeight += 0.5f; // account for the negative heights in the new system

					heightmap[x, y] = legacyHeight;
				}
			}

			data.SetHeights(0, 0, heightmap);
			if (dataWithoutHoles != null)
			{
				dataWithoutHoles.SetHeights(0, 0, heightmap);
			}
		}

		public void convertLegacySplatmap()
		{
			for (int x = 0; x < Landscape.SPLATMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.SPLATMAP_RESOLUTION; y++)
				{
					SplatmapCoord splatmapCoord = new SplatmapCoord(x, y);
					Vector3 worldPosition = Landscape.getWorldPosition(coord, splatmapCoord);

					for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
					{
						float legacyWeight = LevelGround.getConversionWeight(worldPosition, layer);
						splatmap[x, y, layer] = legacyWeight;
					}
				}
			}

			if (!Dedicator.IsDedicatedServer)
			{
				data.SetAlphamaps(0, 0, splatmap);
			}
		}

		public void resetHeightmap()
		{
			for (int x = 0; x < Landscape.HEIGHTMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.HEIGHTMAP_RESOLUTION; y++)
				{
					heightmap[x, y] = 0.5f;
				}
			}

			// Reconciling neighbors also calls SetHeightsDelayLOD afterward.
			Landscape.reconcileNeighbors(this);
			SyncDelayedLOD();
		}

		public void resetSplatmap()
		{
			for (int x = 0; x < Landscape.SPLATMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.SPLATMAP_RESOLUTION; y++)
				{
					splatmap[x, y, 0] = 1;
					for (int layer = 1; layer < Landscape.SPLATMAP_LAYERS; layer++)
					{
						splatmap[x, y, layer] = 0;
					}
				}
			}

			data.SetAlphamaps(0, 0, splatmap);
		}

		public void normalizeSplatmap()
		{
			for (int x = 0; x < Landscape.SPLATMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.SPLATMAP_RESOLUTION; y++)
				{
					float magnitude = 0;
					for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
					{
						magnitude += splatmap[x, y, layer];
					}

					for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
					{
						splatmap[x, y, layer] /= magnitude;
					}
				}
			}

			data.SetAlphamaps(0, 0, splatmap);
		}

		public void applyGraphicsSettings()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (GraphicsSettings.blend)
			{
				switch (GraphicsSettings.renderMode)
				{
					case ERenderMode.FORWARD:
						terrain.materialTemplate = Resources.Load<Material>("Materials/Landscapes/Landscape_Forward");
						break;
					case ERenderMode.DEFERRED:
						terrain.materialTemplate = Resources.Load<Material>("Materials/Landscapes/Landscape_Deferred");
						break;
					default:
						terrain.materialTemplate = null;
						UnturnedLog.error("Unknown render mode: " + GraphicsSettings.renderMode);
						break;
				}
			}
			else
			{
				terrain.materialTemplate = Resources.Load<Material>("Materials/Landscapes/Landscape_Classic");
			}

			terrain.basemapDistance = GraphicsSettings.terrainBasemapDistance;

			if (terrain.materialTemplate == null)
			{
				UnturnedLog.warn("LandscapeTile unable to load materialTemplate");
			}

			terrain.heightmapPixelError = GraphicsSettings.terrainHeightmapPixelError;
		}

		public bool IsValidFoliageSurface
		{
			get => true;
		}

		public FoliageBounds getFoliageSurfaceBounds()
		{
			return new FoliageBounds(new FoliageCoord(coord.x * Landscape.TILE_SIZE_INT / FoliageSystem.TILE_SIZE_INT, coord.y * Landscape.TILE_SIZE_INT / FoliageSystem.TILE_SIZE_INT),
									 new FoliageCoord(((coord.x + 1) * Landscape.TILE_SIZE_INT / FoliageSystem.TILE_SIZE_INT) - 1, ((coord.y + 1) * Landscape.TILE_SIZE_INT / FoliageSystem.TILE_SIZE_INT) - 1));
		}

		public bool getFoliageSurfaceInfo(Vector3 position, out Vector3 surfacePosition, out Vector3 surfaceNormal)
		{
			surfacePosition = position;
			surfacePosition.y = terrain.SampleHeight(position) - (Landscape.TILE_HEIGHT / 2);
			surfaceNormal = data.GetInterpolatedNormal((position.x - (coord.x * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE, (position.z - (coord.y * Landscape.TILE_SIZE)) / Landscape.TILE_SIZE);

			return !Landscape.IsPointInsideHole(surfacePosition);
		}

		public void bakeFoliageSurface(FoliageBakeSettings bakeSettings, FoliageTile foliageTile)
		{
			int min_splatmap_x = ((foliageTile.coord.y * FoliageSystem.TILE_SIZE_INT) - (coord.y * Landscape.TILE_SIZE_INT)) / FoliageSystem.TILE_SIZE_INT * FoliageSystem.SPLATMAP_RESOLUTION_PER_TILE;
			int max_splatmap_x = min_splatmap_x + FoliageSystem.SPLATMAP_RESOLUTION_PER_TILE;
			int min_splatmap_y = ((foliageTile.coord.x * FoliageSystem.TILE_SIZE_INT) - (coord.x * Landscape.TILE_SIZE_INT)) / FoliageSystem.TILE_SIZE_INT * FoliageSystem.SPLATMAP_RESOLUTION_PER_TILE;
			int max_splatmap_y = min_splatmap_y + FoliageSystem.SPLATMAP_RESOLUTION_PER_TILE;
			//int max_splatmap_x = Mathf.Clamp(Mathf.FloorToInt(((tileBounds.max.z - coord.y * Landscape.TILE_SIZE) / Landscape.TILE_SIZE) * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
			//int min_splatmap_y = Mathf.Clamp(Mathf.FloorToInt(((tileBounds.min.x - coord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE) * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
			//int max_splatmap_y = Mathf.Clamp(Mathf.FloorToInt(((tileBounds.max.x - coord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE) * Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);

			//if(max_splatmap_x < Landscape.SPLATMAP_RESOLUTION_MINUS_ONE)
			//{
			//	max_splatmap_x--;
			//}

			//if(max_splatmap_y < Landscape.SPLATMAP_RESOLUTION_MINUS_ONE)
			//{
			//	max_splatmap_y--;
			//}

			UnityEngine.Assertions.Assert.IsFalse(min_splatmap_x < 0 || max_splatmap_x > Landscape.SPLATMAP_RESOLUTION || min_splatmap_y < 0 || max_splatmap_y > Landscape.SPLATMAP_RESOLUTION,
												  "Invalid tile coordinates passed into landscape foliage bake");

			for (int splatmap_x = min_splatmap_x; splatmap_x < max_splatmap_x; splatmap_x++)
			{
				for (int splatmap_y = min_splatmap_y; splatmap_y < max_splatmap_y; splatmap_y++)
				{
					SplatmapCoord splatmapCoord = new SplatmapCoord(splatmap_x, splatmap_y);

					float world_x = (coord.x * Landscape.TILE_SIZE) + (splatmapCoord.y * Landscape.SPLATMAP_WORLD_UNIT);
					float world_z = (coord.y * Landscape.TILE_SIZE) + (splatmapCoord.x * Landscape.SPLATMAP_WORLD_UNIT);

					Bounds bounds = new Bounds();
					bounds.min = new Vector3(world_x, 0, world_z);
					bounds.max = new Vector3(world_x + Landscape.SPLATMAP_WORLD_UNIT, 0, world_z + Landscape.SPLATMAP_WORLD_UNIT);

					for (int layer = 0; layer < Landscape.SPLATMAP_LAYERS; layer++)
					{
						float weight = splatmap[splatmap_x, splatmap_y, layer];
						if (weight < 0.01f)
						{
							continue;
						}

						LandscapeMaterialAsset materialAsset = Assets.find(materials[layer]);
						if (materialAsset != null)
						{
							FoliageInfoCollectionAsset collectionAsset = Assets.find(materialAsset.foliage);
							if (collectionAsset != null)
							{
								collectionAsset.bakeFoliage(bakeSettings, this, bounds, weight);
							}
						}
					}
				}
			}
		}

		protected virtual void handleMaterialsInspectorChanged(IInspectableList list)
		{
			updatePrototypes();
		}

		public virtual void enable()
		{
			FoliageSystem.addSurface(this);
		}

		public virtual void disable()
		{
			FoliageSystem.removeSurface(this);
		}

		public LandscapeTile(LandscapeCoord newCoord)
		{
			gameObject = new GameObject();
			gameObject.tag = "Ground";
			gameObject.layer = LayerMasks.GROUND;
			gameObject.transform.rotation = MathUtility.IDENTITY_QUATERNION;
			gameObject.transform.localScale = Vector3.one;

			coord = newCoord;

			heightmap = new float[Landscape.HEIGHTMAP_RESOLUTION, Landscape.HEIGHTMAP_RESOLUTION];
			splatmap = new float[Landscape.SPLATMAP_RESOLUTION, Landscape.SPLATMAP_RESOLUTION, Landscape.SPLATMAP_LAYERS];
			holes = new bool[Landscape.HOLES_RESOLUTION, Landscape.HOLES_RESOLUTION];

			for (int x = 0; x < Landscape.HEIGHTMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.HEIGHTMAP_RESOLUTION; y++)
				{
					heightmap[x, y] = 0.5f;
				}
			}

			for (int x = 0; x < Landscape.SPLATMAP_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.SPLATMAP_RESOLUTION; y++)
				{
					splatmap[x, y, 0] = 1;
				}
			}

			for (int x = 0; x < Landscape.HOLES_RESOLUTION; x++)
			{
				for (int y = 0; y < Landscape.HOLES_RESOLUTION; y++)
				{
					holes[x, y] = true;
				}
			}

			materials = new InspectableList<AssetReference<LandscapeMaterialAsset>>(Landscape.SPLATMAP_LAYERS);
			materials.Add(DEFAULT_MATERIAL);
			for (int layer = 1; layer < Landscape.SPLATMAP_LAYERS; layer++)
			{
				materials.Add(AssetReference<LandscapeMaterialAsset>.invalid);
			}
			materials.canInspectorAdd = false;
			materials.canInspectorRemove = false;
			materials.inspectorChanged += handleMaterialsInspectorChanged;

			data = new TerrainData();

			if (!Dedicator.IsDedicatedServer)
			{
				// Linux headless server crashes when setting splat prototypes.
				terrainLayers = new TerrainLayer[Landscape.SPLATMAP_LAYERS];
				data.terrainLayers = terrainLayers;
			}

			data.heightmapResolution = Landscape.HEIGHTMAP_RESOLUTION;
			data.alphamapResolution = Landscape.SPLATMAP_RESOLUTION;
			data.baseMapResolution = Landscape.BASEMAP_RESOLUTION;
			data.size = new Vector3(Landscape.TILE_SIZE, Landscape.TILE_HEIGHT, Landscape.TILE_SIZE);
			data.SetHeightsDelayLOD(0, 0, heightmap);

			if (Landscape.ShouldUseSetHolesDelayLOD)
			{
				// Nelson 2025-03-10: SyncTexture(TerrainData.HolesTextureName) throws an InvalidOperationException
				// if holes texture is compressed. Perhaps this is the difference between editor and runtime responsible
				// for public issue #4851.
				data.enableHolesTextureCompression = false;
			}

			isHeightsLodDataDirty = true;
			if (!Dedicator.IsDedicatedServer)
			{
				data.SetAlphamaps(0, 0, splatmap);
			}
			data.wavingGrassTint = Color.white;

			terrain = gameObject.AddComponent<Terrain>();
			terrain.drawInstanced = SystemInfo.supportsInstancing;
			terrain.terrainData = data;
			terrain.heightmapPixelError = 200;
			terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			terrain.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			terrain.drawHeightmap = !Dedicator.IsDedicatedServer;
			terrain.drawTreesAndFoliage = false;
			terrain.collectDetailPatches = false;
			terrain.allowAutoConnect = false;
			terrain.groupingID = 1;
			terrain.Flush();

			if (Level.isEditor)
			{
				dataWithoutHoles = new TerrainData();
				dataWithoutHoles.heightmapResolution = data.heightmapResolution;
				dataWithoutHoles.size = data.size;
				dataWithoutHoles.SetHeightsDelayLOD(0, 0, heightmap);
			}

			collider = gameObject.AddComponent<TerrainCollider>();
			collider.terrainData = Level.isEditor && Landscape.DisableHoleColliders ? dataWithoutHoles : data;

			UpdateNames();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
		private void UpdateNames()
		{
			gameObject.name = $"Terrain ({coord.x}, {coord.y})";
			data.name = $"x: {coord.x} y: {coord.y}";

			if (dataWithoutHoles != null)
			{
				dataWithoutHoles.name = data.name + " (without holes)";
			}
		}

		[System.Obsolete]
		public void SyncHeightmap()
		{
			SyncDelayedLOD();
		}

		private static AssetReference<LandscapeMaterialAsset> DEFAULT_MATERIAL = new AssetReference<LandscapeMaterialAsset>("498ca625072d443a876b2a4f11896018"); // Fallback_Green
	}
}
