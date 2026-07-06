////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_FOLIAGESTORAGE_THREADS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	/// <summary>
	/// Replacement foliage storage with all tiles in a single file.
	/// 
	/// In the level editor all tiles are loaded into memory, whereas during gameplay the relevant tiles
	/// are loaded as-needed by a worker thread.
	/// </summary>
	public class FoliageStorageV2 : IFoliageStorage
	{
		public void Initialize()
		{
			tileBlobOffsets.Clear();
			tileBlobHeaderOffset = 0;
			assetsHeader.Clear();
			loadedFileVersion = 0;

			string filePath = SDG.Unturned.Level.info.path + "/Foliage.blob";
			if (File.Exists(filePath))
			{
				// Allow ReadWrite sharing for saving in editor.
				readerStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				reader = new BinaryReader(readerStream);

				using (SHA1Stream hashStream = new SHA1Stream(readerStream))
				using (BinaryReader hashReader = new BinaryReader(hashStream))
				{
					loadedFileVersion = hashReader.ReadInt32();

					int tileCount = hashReader.ReadInt32();
					UnturnedLog.info("Found {0} foliage v2 tiles", tileCount);
					for (int tileIndex = 0; tileIndex < tileCount; ++tileIndex)
					{
						int x = hashReader.ReadInt32();
						int y = hashReader.ReadInt32();
						long offset = hashReader.ReadInt64();
						tileBlobOffsets.Add(new FoliageCoord(x, y), offset);
					}

					if (loadedFileVersion >= FOLIAGE_FILE_VERSION_ADDED_ASSET_LIST_HEADER)
					{
						bool useEditorAssetRedirector = Level.isEditor && EditorAssetRedirector.HasRedirects;

						int assetCount = hashReader.ReadInt32();
						UnturnedLog.info("Found {0} foliage used assets in header", assetCount);
						assetsHeader.Capacity = assetCount;
						for (int assetIndex = 0; assetIndex < assetCount; ++assetIndex)
						{
							GuidBuffer buffer = new GuidBuffer();
							hashReader.Read(GUID_BUFFER, 0, 16);
							buffer.Read(GUID_BUFFER, 0);
							AssetReference<FoliageInstancedMeshInfoAsset> assetRef = new AssetReference<FoliageInstancedMeshInfoAsset>(buffer.GUID);
							if (useEditorAssetRedirector)
							{
								FoliageInstancedMeshInfoAsset redirect = EditorAssetRedirector.Redirect<FoliageInstancedMeshInfoAsset>(assetRef.GUID);
								if (redirect != null)
								{
									assetRef = redirect.getReferenceTo<FoliageInstancedMeshInfoAsset>();
								}
							}

							assetsHeader.Add(assetRef);
							if (assetRef.Find() == null)
							{
								// If foliage is missing on the server then do not kick clients for missing it as well.
								ClientAssetIntegrity.ServerAddKnownMissingAsset(assetRef.GUID, $"Foliage asset {assetIndex + 1} of {assetCount}");
							}
						}
					}

					tileBlobHeaderOffset = readerStream.Position;

					Level.includeHash("Foliage", hashStream.Hash);
				}

				if (Level.isEditor && loadedFileVersion < FOLIAGE_FILE_VERSION_NEWEST)
				{
					SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
				}

				// Dedicated server loads the foliage v2 header to calculate hash and then discards.
				if (!Level.isEditor && !Dedicator.IsDedicatedServer)
				{
					shouldWorkerThreadContinue = true;
					lockObject = new object();
					workerThread = new Thread(new ThreadStart(WorkerThreadMain));
					workerThread.Name = "Foliage Storage Thread";
					workerThread.Start();
				}
			}
		}

		public void Shutdown()
		{
			if (workerThread != null)
			{
				shouldWorkerThreadContinue = false;
			}
			else
			{
				CloseReader();
			}
		}

		public void TileBecameRelevantToViewer(FoliageTile tile)
		{
			if (hasAllTilesInMemory == false)
			{
				if (!mainThreadTilesWithRelevancyChanges.Contains(tile))
				{
					mainThreadTilesWithRelevancyChanges.Add(tile);
				}
			}
		}

		public void TileNoLongerRelevantToViewer(FoliageTile tile)
		{
			if (hasAllTilesInMemory == false)
			{
				// Clear any data if already loaded on main thread.
				tile.clearAndReleaseInstances();

				if (!mainThreadTilesWithRelevancyChanges.Contains(tile))
				{
					mainThreadTilesWithRelevancyChanges.Add(tile);
				}
			}
		}

		public void Update()
		{
			UnityEngine.Debug.Assert(hasAllTilesInMemory ^ (workerThread != null));
			if (workerThread == null)
				return;

			TileData tileDataToLoad = null;

			// If the worker thread is blocked e.g. busy reading from slow harddrive then this
			// lock could be entered multiple times before worker thread enters it again. In that case
			// we may have loaded multiple tiles.
			lock (lockObject)
			{
				// This list is cleared after the lock.
				foreach (FoliageTile tile in mainThreadTilesWithRelevancyChanges)
				{
					if (tile.isRelevantToViewer)
					{
						if (!workerThreadTileQueue.Contains(tile.coord))
						{
							// to-do: if this tile is currently in the worker-to-main queue then
							// it will get loaded a second time, but the main thread clears existing
							// data if received twice
							workerThreadTileQueue.AddLast(tile.coord);
#if LOG_FOLIAGESTORAGE_THREADS
							UnturnedLog.info($"Tile {tile.coord} data requested");
#endif // LOG_FOLIAGESTORAGE_THREADS
						}
					}
					else
					{
						bool wasRemoved = workerThreadTileQueue.Remove(tile.coord);
						if (wasRemoved)
						{
#if LOG_FOLIAGESTORAGE_THREADS
							UnturnedLog.info($"Tile {tile.coord} requested canceled");
#endif // LOG_FOLIAGESTORAGE_THREADS
						}
					}
				}

				if (tileDataFromWorkerThread.Count > 0)
				{
					tileDataToLoad = tileDataFromWorkerThread.Dequeue();
				}

				if (mainThreadTileDataFromPreviousUpdate != null)
				{
					tileDataFromMainThread.Add(mainThreadTileDataFromPreviousUpdate);
#if LOG_FOLIAGESTORAGE_THREADS
					UnturnedLog.info($"Returning {mainThreadTileDataFromPreviousUpdate.coord} to pool ({tileDataFromMainThread.Count} in queue)");
#endif // LOG_FOLIAGESTORAGE_THREADS
					mainThreadTileDataFromPreviousUpdate = null;
				}
			}
			mainThreadTilesWithRelevancyChanges.Clear();

			if (tileDataToLoad != null)
			{
				FoliageTile tile = FoliageSystem.getTile(tileDataToLoad.coord);
				if (tile != null)
				{
					if (tile.isRelevantToViewer)
					{
#if LOG_FOLIAGESTORAGE_THREADS
						UnturnedLog.info($"Tile {tile.coord} data received");
#endif // LOG_FOLIAGESTORAGE_THREADS

						// to-do: if tile is added to loading queue while in the worker-to-main queue
						// then it will get loaded a second time, so clear instances as a workaround
						tile.clearAndReleaseInstances();

						DeserializeTileOnMainThreadUsingDataFromWorkerThread(tile, tileDataToLoad);
					}
					else
					{
#if LOG_FOLIAGESTORAGE_THREADS
						UnturnedLog.info($"Tile {tile.coord} is no longer relevant, ignoring data");
#endif // LOG_FOLIAGESTORAGE_THREADS
					}
				}
				else
				{
#if LOG_FOLIAGESTORAGE_THREADS
					UnturnedLog.error($"Unable to find tile {tileDataToLoad.coord} loaded by worker thread");
#endif // LOG_FOLIAGESTORAGE_THREADS
				}
				mainThreadTileDataFromPreviousUpdate = tileDataToLoad;
			}
		}

		public void EditorLoadAllTiles(IEnumerable<FoliageTile> tiles)
		{
			UnityEngine.Debug.Assert(workerThread == null);

			hasAllTilesInMemory = true;
			foreach (FoliageTile tile in tiles)
			{
				DeserializeTileOnMainThread(tile);
			}

			CloseReader();
		}

		public void EditorSaveAllTiles(IEnumerable<FoliageTile> tiles)
		{
			UnityEngine.Debug.Assert(workerThread == null);

			string filePath = SDG.Unturned.Level.info.path + "/Foliage.blob";
			if (File.Exists(filePath) && loadedFileVersion >= FOLIAGE_FILE_VERSION_NEWEST)
			{
				// Only do dirty check if level has already been upgraded.
				bool hasAnyUnsavedChanges = false;
				foreach (FoliageTile tile in tiles)
				{
					if (tile.hasUnsavedChanges)
					{
						hasAnyUnsavedChanges = true;
						break;
					}
				}

				if (hasAnyUnsavedChanges == false)
				{
					// No point saving out the entire blob without changes.
					return;
				}
			}

			List<byte[]> blobs = new List<byte[]>();
			Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int> assetRefToIndex = new Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int>();
			tileBlobOffsets.Clear();
			assetsHeader.Clear();
			long totalOffset = 0;

			foreach (FoliageTile tile in tiles)
			{
				if (tile.isEmpty())
					continue;

				byte[] blob = SerializeTileOnMainThread(tile, assetRefToIndex);
				if (blob != null && blob.Length > 0)
				{
					blobs.Add(blob);
					tileBlobOffsets.Add(tile.coord, totalOffset);
					totalOffset += blob.LongLength;
				}
			}

			if (blobs.Count != tileBlobOffsets.Count)
			{
				UnturnedLog.error("Foliage blob count ({0}) does not match offset count ({1})", blobs.Count, tileBlobOffsets.Count);
				return;
			}

			// Nelson 2025-04-01: this was using FileMode.OpenOrCreate which prevented the file from getting smaller
			// if less data was written. (E.g., after removing some foliage.) (public issue #4980)
			using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			{
				BinaryWriter writer = new BinaryWriter(stream);

				writer.Write(FOLIAGE_FILE_VERSION_NEWEST);
				writer.Write(tileBlobOffsets.Count);
				foreach (KeyValuePair<FoliageCoord, long> offset in tileBlobOffsets)
				{
					writer.Write(offset.Key.x);
					writer.Write(offset.Key.y);
					writer.Write(offset.Value);
				}

				UnturnedLog.info($"Foliage saving with {assetsHeader.Count} assets in header");
				writer.Write(assetsHeader.Count);
				foreach (AssetReference<FoliageInstancedMeshInfoAsset> assetRef in assetsHeader)
				{
					GuidBuffer buffer = new GuidBuffer(assetRef.GUID);
					buffer.Write(GUID_BUFFER, 0);
					writer.Write(GUID_BUFFER, 0, 16);
				}

				foreach (byte[] blob in blobs)
				{
					writer.Write(blob);
				}
			}
		}

		private TileData GetTileDataOnWorkerThread(FoliageCoord coord)
		{
			TileData tileData;
			if (tileDataPool.Count > 0)
			{
				tileData = tileDataPool.Pop();
			}
			else
			{
				tileData = new TileData();
				tileData.perAssetData = new List<TilePerAssetData>();
			}
			tileData.coord = coord;

			long offset;
			if (tileBlobOffsets.TryGetValue(coord, out offset))
			{
				readerStream.Position = tileBlobHeaderOffset + offset;

				int instanceCount = reader.ReadInt32();
				tileData.perAssetData.Capacity = Mathf.Max(tileData.perAssetData.Capacity, instanceCount);
				for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
				{
					AssetReference<FoliageInstancedMeshInfoAsset> assetRef;
					if (loadedFileVersion >= FOLIAGE_FILE_VERSION_ADDED_ASSET_LIST_HEADER)
					{
						int assetIndex = reader.ReadInt32();
						if (assetIndex >= 0 && assetIndex < assetsHeader.Count)
						{
							assetRef = assetsHeader[assetIndex];
						}
						else
						{
							// Should never happen, so we log an error for null refs back on the game thread.
							assetRef = AssetReference<FoliageInstancedMeshInfoAsset>.invalid;
						}
					}
					else
					{
						GuidBuffer buffer = new GuidBuffer();
						readerStream.Read(GUID_BUFFER, 0, 16);
						buffer.Read(GUID_BUFFER, 0);
						assetRef = new AssetReference<FoliageInstancedMeshInfoAsset>(buffer.GUID);
					}

					int matrixCount = reader.ReadInt32();

					TilePerAssetData perAssetData;
					if (perAssetDataPool.Count > 0)
					{
						perAssetData = perAssetDataPool.Pop();
						perAssetData.matrices.Capacity = Mathf.Max(perAssetData.matrices.Capacity, matrixCount);
						perAssetData.clearWhenBaked.Capacity = Mathf.Max(perAssetData.clearWhenBaked.Capacity, matrixCount);
					}
					else
					{
						perAssetData = new TilePerAssetData();
						perAssetData.matrices = new List<Matrix4x4>(matrixCount);
						perAssetData.clearWhenBaked = new List<bool>(matrixCount);
					}
					perAssetData.assetRef = assetRef;

					for (int matrixIndex = 0; matrixIndex < matrixCount; matrixIndex++)
					{
						Matrix4x4 matrix = new Matrix4x4();
						for (int elementIndex = 0; elementIndex < 16; elementIndex++)
						{
							matrix[elementIndex] = reader.ReadSingle();
						}
						perAssetData.matrices.Add(matrix);

						bool clearWhenBaked = reader.ReadBoolean();
						perAssetData.clearWhenBaked.Add(clearWhenBaked);
					}

					if (matrixCount > 0)
					{
						tileData.perAssetData.Add(perAssetData);
					}
				}
			}

			return tileData;
		}

		private void DeserializeTileOnMainThreadUsingDataFromWorkerThread(FoliageTile tile, TileData tileData)
		{
			foreach (TilePerAssetData perAssetData in tileData.perAssetData)
			{
				if (perAssetData.assetRef.isNull)
				{
					UnturnedLog.error($"Foliage loaded invalid asset ref for tile {tile.coord}");
					continue;
				}

				FoliageInstanceList list = tile.getOrAddList(perAssetData.assetRef);
				for (int matrixIndex = 0; matrixIndex < perAssetData.matrices.Count; ++matrixIndex)
				{
					Matrix4x4 matrix = perAssetData.matrices[matrixIndex];
					bool clearWhenBaked = perAssetData.clearWhenBaked[matrixIndex];
					if (!tile.isInstanceCut(matrix.GetPosition()))
					{
						list.addInstanceAppend(new FoliageInstanceGroup(perAssetData.assetRef, matrix, clearWhenBaked));
					}
				}
			}
		}

		private void DeserializeTileOnMainThread(FoliageTile tile)
		{
			long offset;
			if (tileBlobOffsets.TryGetValue(tile.coord, out offset))
			{
				readerStream.Position = tileBlobHeaderOffset + offset;

				int instanceCount = reader.ReadInt32();
				for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
				{
					AssetReference<FoliageInstancedMeshInfoAsset> assetRef;

					// We continue reading so that other valid instance types can be added.
					bool shouldAddInstances;

					if (loadedFileVersion >= FOLIAGE_FILE_VERSION_ADDED_ASSET_LIST_HEADER)
					{
						int assetIndex = reader.ReadInt32();
						if (assetIndex >= 0 && assetIndex < assetsHeader.Count)
						{
							assetRef = assetsHeader[assetIndex];
							shouldAddInstances = !assetRef.isNull;
						}
						else
						{
							assetRef = AssetReference<FoliageInstancedMeshInfoAsset>.invalid;
							UnturnedLog.error($"Foliage loaded invalid asset index {assetIndex} for tile {tile.coord}");
							shouldAddInstances = false;
						}
					}
					else
					{
						GuidBuffer buffer = new GuidBuffer();
						readerStream.Read(GUID_BUFFER, 0, 16);
						buffer.Read(GUID_BUFFER, 0);
						assetRef = new AssetReference<FoliageInstancedMeshInfoAsset>(buffer.GUID);
						shouldAddInstances = !assetRef.isNull;
					}

					FoliageInstanceList list = tile.getOrAddList(assetRef);

					int matrixCount = reader.ReadInt32();
					for (int matrixIndex = 0; matrixIndex < matrixCount; matrixIndex++)
					{
						Matrix4x4 matrix = new Matrix4x4();
						for (int elementIndex = 0; elementIndex < 16; elementIndex++)
						{
							matrix[elementIndex] = reader.ReadSingle();
						}

						bool clearWhenBaked = reader.ReadBoolean();

						if (shouldAddInstances && !tile.isInstanceCut(matrix.GetPosition()))
						{
							list.addInstanceAppend(new FoliageInstanceGroup(assetRef, matrix, clearWhenBaked));
						}
					}
				}
			}

			tile.updateBounds();
		}

		private List<KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList>> tileInstanceListsToSave = new List<KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList>>();

		private int GetOrAddAssetIndex(AssetReference<FoliageInstancedMeshInfoAsset> assetRef, Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int> assetRefToIndex)
		{
			int existingIndex;
			if (assetRefToIndex.TryGetValue(assetRef, out existingIndex))
			{
				return existingIndex;
			}

			int newIndex = assetsHeader.Count;
			assetsHeader.Add(assetRef);
			assetRefToIndex.Add(assetRef, newIndex);
			return newIndex;
		}

		private byte[] SerializeTileOnMainThread(FoliageTile tile, Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int> assetRefToIndex)
		{
			tileInstanceListsToSave.Clear();
			foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> list in tile.instances)
			{
				if (list.Key.isNull)
					continue;

				if (list.Value.IsListEmpty())
					continue;

				if (!LevelObjects.preserveMissingAssets && list.Key.Find() == null)
				{
					// Invalid assets are being removed from level and this foliage type is missing.
					UnturnedLog.info($"Discarding missing foliage asset {list.Key} from tile {tile.coord}");
					continue;
				}

				tileInstanceListsToSave.Add(list);
			}

			if (tileInstanceListsToSave.Count < 1)
				return null;

			using (MemoryStream stream = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(stream);

				writer.Write(tileInstanceListsToSave.Count);
				foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> list in tileInstanceListsToSave)
				{
					int assetIndex = GetOrAddAssetIndex(list.Key, assetRefToIndex);
					writer.Write(assetIndex);

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

				return stream.ToArray();
			}
		}

		private void CloseReader()
		{
			if (reader != null)
			{
				reader.Close();
				reader.Dispose();
				reader = null;
			}

			if (readerStream != null)
			{
				readerStream.Close();
				readerStream.Dispose();
				readerStream = null;
			}
		}

		/// <summary>
		/// Entry point for worker thread loop.
		/// </summary>
		private void WorkerThreadMain()
		{
			perAssetDataPool = new Stack<TilePerAssetData>();
			tileDataPool = new Stack<TileData>();

			// Tile ready to be shared with the main thread during the next lock.
			TileData workerThreadNewlyLoadedData = null;

			while (shouldWorkerThreadContinue)
			{
				FoliageCoord requestedCoord = default;
				bool hasRequest = false;

				// If the main thread is blocked e.g. loading a dense barricade/structure region then
				// lock could be entered multiple times before main thread enters it again. In that case
				// we may have read multiple tiles.
				lock (lockObject)
				{
					if (workerThreadTileQueue.Count > 0)
					{
						requestedCoord = workerThreadTileQueue.First.Value;
						workerThreadTileQueue.RemoveFirst();
						hasRequest = true;
					}

					// Share newly read tile with main thread.
					if (workerThreadNewlyLoadedData != null)
					{
						tileDataFromWorkerThread.Enqueue(workerThreadNewlyLoadedData);
						workerThreadNewlyLoadedData = null;
					}

					// Release used tiles into pool.
					foreach (TileData tileData in tileDataFromMainThread)
					{
						foreach (TilePerAssetData perAssetData in tileData.perAssetData)
						{
							perAssetData.matrices.Clear();
							perAssetData.clearWhenBaked.Clear();
							perAssetDataPool.Push(perAssetData);
						}

						tileData.perAssetData.Clear();
						tileDataPool.Push(tileData);
					}
					tileDataFromMainThread.Clear();
				}

				if (hasRequest)
				{
					workerThreadNewlyLoadedData = GetTileDataOnWorkerThread(requestedCoord);
				}

				Thread.Sleep(10);
			}

			CloseReader();
		}

		private const int FOLIAGE_FILE_VERSION_INITIAL = 1;
		private const int FOLIAGE_FILE_VERSION_ADDED_ASSET_LIST_HEADER = 2;
		private const int FOLIAGE_FILE_VERSION_NEWEST = FOLIAGE_FILE_VERSION_ADDED_ASSET_LIST_HEADER;
		private byte[] GUID_BUFFER = new byte[16];

		private FileStream readerStream;
		private BinaryReader reader;

		private bool hasAllTilesInMemory = false;

		/// <summary>
		/// Order is important because TileBecameRelevant is called from the closest tile outward.
		/// </summary>
		private List<FoliageTile> mainThreadTilesWithRelevancyChanges = new List<FoliageTile>();

		/// <summary>
		/// Offsets into blob for per-tile data.
		/// </summary>
		private Dictionary<FoliageCoord, long> tileBlobOffsets = new Dictionary<FoliageCoord, long>();

		/// <summary>
		/// Tiles save an index into this list rather than guid.
		/// </summary>
		private List<AssetReference<FoliageInstancedMeshInfoAsset>> assetsHeader = new List<AssetReference<FoliageInstancedMeshInfoAsset>>();

		private int loadedFileVersion;

		/// <summary>
		/// Offset from header data.
		/// </summary>
		private long tileBlobHeaderOffset = 0;

		private Thread workerThread;
		private bool shouldWorkerThreadContinue;

		/// <summary>
		/// Data-only FoliageInstanceList shared between threads.
		/// </summary>
		private class TilePerAssetData
		{
			public AssetReference<FoliageInstancedMeshInfoAsset> assetRef;
			public List<Matrix4x4> matrices;
			public List<bool> clearWhenBaked;
		}

		/// <summary>
		/// Data-only FoliageTile shared between threads.
		/// </summary>
		private class TileData
		{
			public FoliageCoord coord;
			public List<TilePerAssetData> perAssetData;
		}

		/// <summary>
		/// Ready to be released to the worker thread during the next lock.
		/// </summary>
		private TileData mainThreadTileDataFromPreviousUpdate;

		/// <summary>
		/// Mutex lock. Only used in the main thread Update loop and worker thread loop.
		/// </summary>
		private object lockObject;

		/// <summary>
		/// SHARED BY BOTH THREADS!
		/// Coordinates requested by main thread for worker thread to read.
		/// This is a list because while main thread is busy the worker thread can continue reading.
		/// </summary>
		private LinkedList<FoliageCoord> workerThreadTileQueue = new LinkedList<FoliageCoord>();

		/// <summary>
		/// SHARED BY BOTH THREADS!
		/// Tiles read by worker thread ready to be copied into actual foliage tiles on main thread.
		/// </summary>
		private Queue<TileData> tileDataFromWorkerThread = new Queue<TileData>();

		/// <summary>
		/// SHARED BY BOTH THREADS!
		/// Main thread has finished using this tile data and it can be released back to the pool on the worker thread.
		/// This is a list because main thread could have populated multiple foliage tiles while the worker thread was busy reading.
		/// </summary>
		private List<TileData> tileDataFromMainThread = new List<TileData>();

		/// <summary>
		/// Lifecycle:
		/// 1. Worker thread claims or allocates data.
		/// 2. Worker thread passes data to main thread.
		/// 3. Main thread copies data over to actual foliage tile.
		/// 4. Main thread passes data back to worker thread.
		/// 5. Worker thread releases data back to pool.
		/// </summary>
		private Stack<TilePerAssetData> perAssetDataPool;
		private Stack<TileData> tileDataPool;
	}
}
