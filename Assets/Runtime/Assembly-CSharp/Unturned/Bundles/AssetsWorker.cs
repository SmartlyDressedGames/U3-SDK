////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define WITH_ASSETS_PROFILING
// #define LOG_ASSETS_WORKER
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	/// <summary>
	/// Responsible for loading asset definitions on a separate thread.
	/// </summary>
	internal class AssetsWorker
	{
		public enum EResultType
		{
			MasterBundle,
			Asset,
			Exception,
		}

		public abstract class ResultItem
		{
			public abstract EResultType ResultType { get; }
		}

		public sealed class MasterBundle : ResultItem
		{
			public override EResultType ResultType => EResultType.MasterBundle;

			public MasterBundleConfig config;
			public byte[] assetBundleData;
			public byte[] hash;
		}

		public sealed class AssetDefinition : ResultItem
		{
			public override EResultType ResultType => EResultType.Asset;

			public string path;
			public byte[] hash;
			public IDatDictionary assetData;
			public IDatDictionary translationData;
			public IDatDictionary fallbackTranslationData;

			/// <summary>
			/// Parser error messages, if any.
			/// </summary>
			public List<string> assetErrors;

			/// <summary>
			/// Warning: on worker thread this only acts as handle. Do not access.
			/// </summary>
			public AssetOrigin origin;
		}

		public sealed class ExceptionDetails : ResultItem
		{
			public override EResultType ResultType => EResultType.Exception;

			public string message;
			public System.Exception exception;
		}

		/// <summary>
		/// Used on main thread to determine when all queued tasks have finished.
		/// </summary>
		public bool IsWorking
		{
			get => isWorking;
		}

		public void Initialize()
		{
			// Make a copy of language information so worker knows what translation files to look for.
			language = Provider.language;
			languageIsEnglish = Provider.languageIsEnglish;
			UnityEngine.Debug.Assert(!string.IsNullOrEmpty(language));

			shouldWorkerThreadsContinue = 1;

			resultItems = new ConcurrentQueue<ResultItem>();
		}

		public void Shutdown()
		{
			Interlocked.Exchange(ref shouldWorkerThreadsContinue, 0);
		}

		public void RequestSearch(string path, AssetOrigin origin)
		{
			++totalSearchLocationRequests;

			WorkerThreadState state = new WorkerThreadState()
			{
				owner = this,
				rootPath = path,
				searchPaths = new Queue<string>(),
				readerWorkItems = new ConcurrentQueue<WorkerThreadState.ReaderWorkItem>(),
				datParser = new DatParser(),
				origin = origin,
			};
			state.datParser.EnableMetadata = Assets.shouldParseMetadata;
			ThreadPool.QueueUserWorkItem(SearcherThreadMain, state);
			ThreadPool.QueueUserWorkItem(ReaderThreadMain, state);

#if LOG_ASSETS_WORKER
			UnturnedLog.info("Main thread path queued: " + path);
#endif // LOG_ASSETS_WORKER

			isWorking = true;
		}

		public bool TryDequeueResult(out ResultItem result)
		{
			return resultItems.TryDequeue(out result);
		}

		public void Update()
		{
#if LOG_ASSETS_WORKER
			foreach (SearchLocation path in mtSearchLocations)
			{
				UnturnedLog.info("Main thread submitting path to worker thread: " + path.path);
			}
#endif // LOG_ASSETS_WORKER

			isWorking = totalSearchLocationRequests > totalSearchLocationsFinishedReading || resultItems.Count > 0;
		}

		/// <summary>
		/// Loop searching directories recursively for asset bundle and asset definition files.
		/// </summary>
		private void SearcherThreadMain(object untypedState)
		{
			WorkerThreadState state = (WorkerThreadState) untypedState;

			state.searchPaths.Enqueue(state.rootPath);

			while (shouldWorkerThreadsContinue > 0 && state.searchPaths.Count > 0)
			{
#if WITH_ASSETS_PROFILING
				workSampler.Begin();
#endif // WITH_ASSETS_PROFILING
				string dirPath = state.searchPaths.Dequeue();

				string mbConfigPath = Path.Combine(dirPath, "MasterBundle.dat");
				try
				{
					if (File.Exists(mbConfigPath))
					{
						Interlocked.Increment(ref totalMasterBundlesFound);
						state.readerWorkItems.Enqueue(new WorkerThreadState.ReaderWorkItem()
						{
							filePath = mbConfigPath,
							itemType = WorkerThreadState.ReaderWorkItem.EItemType.MasterBundle,
						});
					}
				}
				catch (System.Exception e)
				{
					state.AddException(e, $"Caught exception reading master bundle config at: \"{mbConfigPath}\"");
				}

				state.FindAssets(dirPath, state.origin);

				try
				{
					foreach (string subdirectoryPath in Directory.EnumerateDirectories(dirPath))
					{
						state.searchPaths.Enqueue(subdirectoryPath);
					}
				}
				catch (System.Exception e)
				{
					state.AddException(e, $"Caught exception finding asset subdirectories in: \"{dirPath}\"");
				}
#if WITH_ASSETS_PROFILING
				workSampler.End();
#endif // WITH_ASSETS_PROFILING

				// We don't sleep while we have work to do otherwise this thread becomes the bottleneck.
			}

			Interlocked.Exchange(ref state.isFinishedSearching, 1);
			Interlocked.Increment(ref totalSearchLocationsFinishedSearching);
		}

		private async void ReaderThreadMain(object untypedState)
		{
			WorkerThreadState state = (WorkerThreadState) untypedState;

			while (shouldWorkerThreadsContinue > 0)
			{
				// Hack? We check this before checking for a new work item to prevent a race condition if a work item
				// is added after dequeuing work item but before checking if finished.
				bool finishedSearching = state.isFinishedSearching > 0;

				WorkerThreadState.ReaderWorkItem workItem;
				if (state.readerWorkItems.TryDequeue(out workItem))
				{
					if (workItem.itemType == WorkerThreadState.ReaderWorkItem.EItemType.MasterBundle)
					{
						try
						{
							IDatDictionary configData = state.ReadFileWithoutHash(workItem.filePath);
							string dirPath = Path.GetDirectoryName(workItem.filePath);
							MasterBundleConfig config = new MasterBundleConfig(dirPath, configData, state.origin);

							byte[] data;
							byte[] hash;

							using (FileStream fileStream = new FileStream(config.getAssetBundlePath(), FileMode.Open, FileAccess.Read, FileShare.Read))
							using (SHA1Stream hashStream = new SHA1Stream(fileStream))
							using (MemoryStream memoryStream = new MemoryStream())
							{
								await hashStream.CopyToAsync(memoryStream);
								data = memoryStream.ToArray();
								hash = hashStream.Hash;
							}

							Interlocked.Increment(ref totalMasterBundlesRead);
							state.owner.resultItems.Enqueue(new MasterBundle
							{
								config = config,
								assetBundleData = data,
								hash = hash,
							});
						}
						catch (System.Exception e)
						{
							state.AddException(e, $"Caught exception reading master bundle config at: \"{workItem.filePath}\"");
						}
					}
					else if (workItem.itemType == WorkerThreadState.ReaderWorkItem.EItemType.Asset)
					{
						try
						{
							state.AddFoundAsset(workItem.filePath, true /* To-do: Why was this parameter Added? */);
						}
						catch (System.Exception e)
						{
							state.AddException(e, $"Caught exception reading asset definition at: \"{workItem.filePath}\"");
						}
					}

					continue;
				}

				if (finishedSearching)
				{
					// No more files to load and searcher is finished, so we can stop.
					break;
				}

				// We don't sleep while we have work to do otherwise this thread becomes the bottleneck.
			}

			Interlocked.Increment(ref totalSearchLocationsFinishedReading);
		}

		private class WorkerThreadState
		{
			public struct ReaderWorkItem
			{
				public enum EItemType
				{
					MasterBundle,
					Asset,
				}

				public string filePath;
				public EItemType itemType;
			}

			public AssetsWorker owner;
			public DatParser datParser;
			public string rootPath;
			public Queue<string> searchPaths;
			public ConcurrentQueue<ReaderWorkItem> readerWorkItems;
			public int isFinishedSearching;

			/// <summary>
			/// Warning: on worker thread this only acts as handle. Do not access.
			/// </summary>
			public AssetOrigin origin;

			public IDatDictionary ReadFileWithoutHash(string path)
			{
				using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (StreamReader streamReader = new StreamReader(fileStream))
				{
					return datParser.Parse(streamReader);
				}
			}

			public void FindAssets(string dirPath, AssetOrigin origin)
			{
				string folderName = Path.GetFileName(dirPath);
				string testPath = Path.Combine(dirPath, folderName + ".asset");

				try
				{
					if (File.Exists(testPath))
					{
						Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
						readerWorkItems.Enqueue(new ReaderWorkItem()
						{
							filePath = testPath,
							itemType = ReaderWorkItem.EItemType.Asset,
						});
						return;
					}
				}
				catch (System.Exception e)
				{
					AddException(e, $"Caught exception checking for assets at: \"{testPath}\"");
					return;
				}

				testPath = Path.Combine(dirPath, folderName + ".dat");
				try
				{
					if (File.Exists(testPath))
					{
						Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
						readerWorkItems.Enqueue(new ReaderWorkItem()
						{
							filePath = testPath,
							itemType = ReaderWorkItem.EItemType.Asset,
						});
						return;
					}
				}
				catch (System.Exception e)
				{
					AddException(e, $"Caught exception checking for assets at: \"{testPath}\"");
					return;
				}

				testPath = Path.Combine(dirPath, "Asset.dat");
				try
				{
					if (File.Exists(testPath))
					{
						Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
						readerWorkItems.Enqueue(new ReaderWorkItem()
						{
							filePath = testPath,
							itemType = ReaderWorkItem.EItemType.Asset,
						});
						return;
					}
				}
				catch (System.Exception e)
				{
					AddException(e, $"Caught exception checking for assets at: \"{testPath}\"");
					return;
				}

				try
				{
					foreach (string filePath in Directory.EnumerateFiles(dirPath, "*.asset"))
					{
						Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
						readerWorkItems.Enqueue(new ReaderWorkItem()
						{
							filePath = filePath,
							itemType = ReaderWorkItem.EItemType.Asset,
						});
					}
				}
				catch (System.Exception e)
				{
					AddException(e, $"Caught exception checking for assets in: \"{dirPath}\"");
					return;
				}
			}

			public void AddFoundAsset(string filePath, bool checkForTranslations)
			{
				string dirPath = Path.GetDirectoryName(filePath);

				IDatDictionary rootData;
				byte[] dataHash;

				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (SHA1Stream hashStream = new SHA1Stream(fileStream))
				using (StreamReader streamReader = new StreamReader(hashStream))
				{
					rootData = datParser.Parse(streamReader);
					List<string> assetErrors = datParser.HasError ? new List<string>(datParser.ErrorMessages) : null;
					dataHash = hashStream.Hash;

					IDatDictionary translationData = null;
					IDatDictionary fallbackTranslationData = null;

					if (checkForTranslations)
					{
						string languageFilePath = Path.Combine(dirPath, owner.language + ".dat");
						string englishFilePath = Path.Combine(dirPath, "English.dat");

						if (File.Exists(languageFilePath))
						{
							translationData = ReadFileWithoutHash(languageFilePath);
							if (!owner.languageIsEnglish && File.Exists(englishFilePath))
							{
								fallbackTranslationData = ReadFileWithoutHash(englishFilePath);
							}
						}
						else if (File.Exists(englishFilePath))
						{
							translationData = ReadFileWithoutHash(englishFilePath);
						}
					}

					Interlocked.Increment(ref owner.totalAssetDefinitionsRead);
					owner.resultItems.Enqueue(new AssetDefinition()
					{
						path = filePath,
						assetData = rootData,
						assetErrors = assetErrors,
						hash = dataHash,
						translationData = translationData,
						fallbackTranslationData = fallbackTranslationData,
						origin = origin,
					});
				}
			}

			public void AddException(System.Exception exception, string message)
			{
				owner.resultItems.Enqueue(new ExceptionDetails()
				{
					message = message,
					exception = exception,
				});
			}
		}

		private int shouldWorkerThreadsContinue;

		private ConcurrentQueue<ResultItem> resultItems;
		internal int totalMasterBundlesFound;
		internal int totalMasterBundlesRead;
		internal int totalAssetDefinitionsFound;
		internal int totalAssetDefinitionsRead;
		internal int totalSearchLocationRequests;
		internal int totalSearchLocationsFinishedSearching;
		internal int totalSearchLocationsFinishedReading;
		private bool isWorking;

		private string language;
		private bool languageIsEnglish;

#if WITH_ASSETS_PROFILING
		private CustomSampler workSampler = CustomSampler.Create("AssetsWorker.Work");
#endif // WITH_ASSETS_PROFILING
	}
}
