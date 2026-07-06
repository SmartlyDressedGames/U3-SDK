////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.Utilities;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageTile : IFormattedFileReadable, IFormattedFileWritable
	{
		protected FoliageCoord _coord;
		public FoliageCoord coord
		{
			get => _coord;
			protected set
			{
				_coord = value;
				updateBounds();
			}
		}

		public Bounds worldBounds
		{
			get;
			protected set;
		}

		public Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instances;

		public bool hasUnsavedChanges;
		public bool isRelevantToViewer;

		[System.Obsolete]
		public void addCut(IShapeVolume cut)
		{
		}

		internal void AddCut(FoliageCut cut)
		{
			cuts.Add(cut);

			foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> pair in instances)
			{
				FoliageInstanceList list = pair.Value;

				for (int listIndex = 0; listIndex < list.matrices.Count; listIndex++)
				{
					List<Matrix4x4> matrixList = list.matrices[listIndex];
					List<bool> clearWhenBakedList = list.clearWhenBaked[listIndex];

					for (int matrixIndex = matrixList.Count - 1; matrixIndex >= 0; matrixIndex--)
					{
						if (cut.ContainsPoint(matrixList[matrixIndex].GetPosition()))
						{
							matrixList.RemoveAt(matrixIndex);
							clearWhenBakedList.RemoveAt(matrixIndex);
						}
					}
				}
			}
		}

		internal void RemoveCut(FoliageCut cut)
		{
			cuts.RemoveFast(cut);
		}

		public bool isInstanceCut(Vector3 point)
		{
			foreach (FoliageCut cut in cuts)
			{
				if (cut.ContainsPoint(point))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Does this tile contain any placed foliage?
		/// </summary>
		public bool isEmpty()
		{
			foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> list in instances)
			{
				if (!list.Value.IsListEmpty())
				{
					return false;
				}
			}

			return true;
		}

		public FoliageInstanceList getOrAddList(AssetReference<FoliageInstancedMeshInfoAsset> assetReference)
		{
			FoliageInstanceList list;
			if (!instances.TryGetValue(assetReference, out list))
			{
				list = PoolablePool<FoliageInstanceList>.claim();
				list.assetReference = assetReference;
				list.loadAsset();
				instances.Add(assetReference, list);
			}

			return list;
		}

		public void addInstance(FoliageInstanceGroup instance)
		{
			FoliageInstanceList list = getOrAddList(instance.assetReference);
			list.addInstanceRandom(instance);

			updateBounds();

			hasUnsavedChanges = true;
		}

		public void removeInstance(FoliageInstanceList list, int matricesIndex, int matrixIndex)
		{
			list.removeInstance(matricesIndex, matrixIndex);

			hasUnsavedChanges = true;
		}

		public void clearAndReleaseInstances()
		{
			if (instances.Count > 0)
			{
				foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> pair in instances)
				{
					PoolablePool<FoliageInstanceList>.release(pair.Value);
				}
			}
			instances.Clear();
		}

		public void clearGeneratedInstances()
		{
			foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> pair in instances)
			{
				FoliageInstanceList list = pair.Value;
				list.clearGeneratedInstances();
			}
		}

		public void applyScale()
		{
			foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> pair in instances)
			{
				FoliageInstanceList list = pair.Value;
				list.applyScale();
			}
		}

		public virtual void read(IFormattedFileReader reader)
		{
			reader = reader.readObject();
			coord = reader.readValue<FoliageCoord>("Coord");
		}

		public virtual void write(IFormattedFileWriter writer)
		{
			writer.beginObject();
			writer.writeValue("Coord", coord);
			writer.endObject();
		}

		public void updateBounds()
		{
			if (instances.Count > 0)
			{
				float min_y = Landscapes.Landscape.TILE_HEIGHT;
				float max_y = -Landscapes.Landscape.TILE_HEIGHT;

				foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> pair in instances)
				{
					FoliageInstanceList list = pair.Value;

					foreach (List<Matrix4x4> matrixList in list.matrices)
					{
						foreach (Matrix4x4 matrix in matrixList)
						{
							float y = matrix.m13;

							if (y < min_y)
							{
								min_y = y;
							}

							if (y > max_y)
							{
								max_y = y;
							}
						}
					}
				}

				float height = max_y - min_y;
				worldBounds = new Bounds(new Vector3((coord.x * FoliageSystem.TILE_SIZE) + (FoliageSystem.TILE_SIZE / 2), min_y + (height / 2), (coord.y * FoliageSystem.TILE_SIZE) + (FoliageSystem.TILE_SIZE / 2)),
										 new Vector3(FoliageSystem.TILE_SIZE, height, FoliageSystem.TILE_SIZE));
			}
			else
			{
				worldBounds = new Bounds(new Vector3((coord.x * FoliageSystem.TILE_SIZE) + (FoliageSystem.TILE_SIZE / 2), 0, (coord.y * FoliageSystem.TILE_SIZE) + (FoliageSystem.TILE_SIZE / 2)),
						 new Vector3(FoliageSystem.TILE_SIZE, Landscapes.Landscape.TILE_HEIGHT, FoliageSystem.TILE_SIZE));
			}
		}

		public FoliageTile(FoliageCoord newCoord)
		{
			instances = new Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList>();
			coord = newCoord;
			cuts = new List<FoliageCut>();
		}

		private List<FoliageCut> cuts;
	}
}
