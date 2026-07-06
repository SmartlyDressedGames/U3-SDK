////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	/// <summary>
	/// Legacy implementation of foliage storage, with one file per tile.
	/// </summary>
	public class FoliageStorageV1 : IFoliageStorage
	{
		public void Initialize()
		{ }

		public void Shutdown()
		{ }

		public void TileBecameRelevantToViewer(FoliageTile tile)
		{
			if (hasAllTilesInMemory == false)
			{
				pendingLoad.AddLast(tile);
			}
		}

		public void TileNoLongerRelevantToViewer(FoliageTile tile)
		{
			if (hasAllTilesInMemory == false)
			{
				pendingLoad.Remove(tile);
				tile.clearAndReleaseInstances();
			}
		}

		public void Update()
		{
			if (pendingLoad.Count > 0)
			{
				FoliageTile tileToLoad = pendingLoad.First.Value;
				pendingLoad.RemoveFirst();
				readInstances(tileToLoad);
			}
		}

		public void EditorLoadAllTiles(IEnumerable<FoliageTile> tiles)
		{
			hasAllTilesInMemory = true;
			foreach (FoliageTile tile in tiles)
			{
				readInstances(tile);
			}
		}

		public void EditorSaveAllTiles(IEnumerable<FoliageTile> tiles)
		{
			foreach (FoliageTile tile in tiles)
			{
				if (tile.hasUnsavedChanges)
				{
					tile.hasUnsavedChanges = false;
					writeInstances(tile);
				}
			}
		}

		protected string formatTilePath(FoliageTile tile)
		{
			string x = tile.coord.x.ToString(System.Globalization.CultureInfo.InvariantCulture);
			string y = tile.coord.y.ToString(System.Globalization.CultureInfo.InvariantCulture);
			return SDG.Unturned.Level.info.path + "/Foliage/Tile_" + x + "_" + y + ".foliage";
		}

		protected void readInstances(FoliageTile tile)
		{
			string filePath = formatTilePath(tile);
			if (File.Exists(filePath))
			{
				using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					BinaryReader reader = new BinaryReader(stream);

					int version = reader.ReadInt32();
					int instanceCount = reader.ReadInt32();
					for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
					{
						GuidBuffer buffer = new GuidBuffer();
						stream.Read(GUID_BUFFER, 0, 16);
						buffer.Read(GUID_BUFFER, 0);
						AssetReference<FoliageInstancedMeshInfoAsset> assetReference = new AssetReference<FoliageInstancedMeshInfoAsset>(buffer.GUID);

						FoliageInstanceList list = tile.getOrAddList(assetReference);

						int matrixCount = reader.ReadInt32();
						for (int matrixIndex = 0; matrixIndex < matrixCount; matrixIndex++)
						{
							Matrix4x4 matrix = new Matrix4x4();
							for (int elementIndex = 0; elementIndex < 16; elementIndex++)
							{
								matrix[elementIndex] = reader.ReadSingle();
							}

							bool clearWhenBaked;
							if (version > 2)
							{
								clearWhenBaked = reader.ReadBoolean();
							}
							else
							{
								clearWhenBaked = true;
							}

							if (!tile.isInstanceCut(matrix.GetPosition()))
							{
								list.addInstanceAppend(new FoliageInstanceGroup(assetReference, matrix, clearWhenBaked));
							}
						}
					}
				}
			}

			tile.updateBounds();
		}

		public void writeInstances(FoliageTile tile)
		{
			string filePath = formatTilePath(tile);
			string directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
			{
				BinaryWriter writer = new BinaryWriter(stream);

				writer.Write(FOLIAGE_FILE_VERSION);
				writer.Write(tile.instances.Count);
				foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> list in tile.instances)
				{
					GuidBuffer buffer = new GuidBuffer(list.Key.GUID);
					buffer.Write(GUID_BUFFER, 0);
					stream.Write(GUID_BUFFER, 0, 16);

					int matrixCount = 0;
					foreach (List<Matrix4x4> matrixList in list.Value.matrices)
					{
						matrixCount += matrixList.Count;
					}

					writer.Write(matrixCount);
					for (int listIndex = 0; listIndex < list.Value.matrices.Count; listIndex++)
					{
						List<Matrix4x4> matrixList = list.Value.matrices[listIndex];
						List<bool> clearWhenBakedList = list.Value.clearWhenBaked[listIndex];

						for (int matrixIndex = 0; matrixIndex < matrixList.Count; matrixIndex++)
						{
							Matrix4x4 matrix = matrixList[matrixIndex];
							for (int elementIndex = 0; elementIndex < 16; elementIndex++)
							{
								writer.Write(matrix[elementIndex]);
							}

							bool clearWhenBaked = clearWhenBakedList[matrixIndex];
							writer.Write(clearWhenBaked);
						}
					}
				}
			}
		}

		protected bool hasAllTilesInMemory = false;
		protected LinkedList<FoliageTile> pendingLoad = new LinkedList<FoliageTile>();

		private readonly int FOLIAGE_FILE_VERSION = 3;
		private byte[] GUID_BUFFER = new byte[16];
	}
}
