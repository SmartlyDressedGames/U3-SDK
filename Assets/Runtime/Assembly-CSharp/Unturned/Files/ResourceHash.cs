////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !DEDICATED_SERVER
using System.Collections.Generic;
using System.IO;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public class ResourceHash
	{
		public static byte[] localHash = new byte[20];

		public static void Initialize()
		{
			if (wasInitialized)
				return;

			wasInitialized = true;

			if (Dedicator.IsDedicatedServer)
				return;

			if (shouldSkipHashing)
			{
				UnturnedLog.info("Skipping resources hashing");
				return;
			}

			ResourceHashThreadState state = new ResourceHashThreadState();
			state.shouldLogVerbose = shouldLogHash;
			state.logMessages = new List<string>();
			if (state.shouldLogVerbose)
			{
				UnturnedLog.info("Queueing resources hashing work item...");
			}
			state.watch = System.Diagnostics.Stopwatch.StartNew();

			System.Threading.ThreadPool.QueueUserWorkItem(ThreadInitialize, state);
		}

		/// <param name="dataPath">Unturned_Data folder path</param>
		public static List<string> GatherFilePaths(string dataPath)
		{
			List<string> paths = new List<string>();
			GatherFilePathsInDirectory(dataPath, paths);

			string subDir = Path.Combine(dataPath, "Resources");
			GatherFilePathsInDirectory(subDir, paths);

			return paths;
		}

		private static async void ThreadInitialize(object voidState)
		{
			ResourceHashThreadState state = (ResourceHashThreadState) voidState;

#if UNITY_EDITOR
			string dataPath = PathEx.Join(UnityPaths.ProjectDirectory, "Builds", "Test", "Unturned_Data");
#else
			string dataPath = UnityPaths.GameDataDirectory.FullName;
#endif // !UNITY_EDITOR

			try
			{
				if (Directory.Exists(dataPath))
				{
					long previousElapsedMs = state.watch.ElapsedMilliseconds;

					List<string> filePaths = GatherFilePaths(dataPath);
					List<byte[]> hashes = new List<byte[]>();
					foreach (string path in filePaths)
					{
						using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
						using (SHA1Stream hashStream = new SHA1Stream(fileStream))
						using (MemoryStream memoryStream = new MemoryStream())
						{
							await hashStream.CopyToAsync(memoryStream);
							byte[] hash = hashStream.Hash;
							hashes.Add(hash);

							if (state.shouldLogVerbose)
							{
								long currentElapsedMs = state.watch.ElapsedMilliseconds;
								long individualFileMs = currentElapsedMs - previousElapsedMs;
								previousElapsedMs = currentElapsedMs;
								state.logMessages.Add($"Including {path} in resources hash: {Hash.toString(hash)} ({individualFileMs} ms)");
							}
						}
					}

					byte[] combinedHash = Hash.combine(hashes);
					if (state.shouldLogVerbose)
					{
						state.logMessages.Add($"Combined resources hash: {Hash.toString(combinedHash)}");
					}

					state.hash = combinedHash;
				}
				else
				{
					state.logMessages.Add($"Resources data path does not exist ({dataPath})");
				}
			}
			catch (System.Exception exception)
			{
				state.logMessages.Add($"Caught exception hashing resources: {exception.Message}");
			}

			GameThreadQueueUtil.QueueGameThreadWorkItem(OnHashReady, state);
		}

		private static void OnHashReady(object voidState)
		{
			ResourceHashThreadState state = (ResourceHashThreadState) voidState;

			state.watch.Stop();

			if (shouldLogHash)
			{
				UnturnedLog.info($"Hash resources: {state.watch.ElapsedMilliseconds} ms");
			}

			if (state.logMessages != null && state.logMessages.Count > 0)
			{
				foreach (string message in state.logMessages)
				{
					UnturnedLog.info(message);
				}
			}

			if (state.hash != null)
			{
				localHash = state.hash;
				UnturnedLog.info("Hashed resources");
			}
			else
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				UnturnedLog.info("Unable to hash player resources until test player is exported");
#else
				UnturnedLog.error("Hashing resources failed");
#endif
			}
		}

		private static void GatherFilePathsInDirectory(string directoryPath, List<string> filePaths)
		{
			List<string> fileNames = new List<string>();

			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
			foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
			{
				string name = fileInfo.Name;
				if (name.Equals("globalgamemanagers", System.StringComparison.Ordinal)
					|| name.Equals("unity default resources", System.StringComparison.Ordinal)
					|| name.Equals("unity_builtin_extra", System.StringComparison.Ordinal)
					|| name.StartsWith("level", System.StringComparison.Ordinal)
					|| name.EndsWith(".assets", System.StringComparison.Ordinal)
					|| name.EndsWith(".assets.resS", System.StringComparison.Ordinal))
				{
					fileNames.Add(name);
				}
			}

			fileNames.Sort(System.StringComparer.Ordinal);

			foreach (string fileName in fileNames)
			{
				filePaths.Add(Path.Combine(directoryPath, fileName));
			}
		}

		private class ResourceHashThreadState
		{
			public List<string> logMessages;
			public byte[] hash;
			public System.Diagnostics.Stopwatch watch;
			public bool shouldLogVerbose;
		}

		private static bool wasInitialized;

		/// <summary>
		/// Useful to check whether hashing is causing problems.
		/// </summary>
		private static CommandLineFlag shouldSkipHashing = new CommandLineFlag(false, "-SkipResourcesHashing");

		/// <summary>
		/// Useful to narrow down why a player is getting kicked for modified resource files when joining a server.
		/// </summary>
		private static CommandLineFlag shouldLogHash = new CommandLineFlag(false, "-LogResourcesHash");
	}
}
#endif // !DEDICATED_SERVER
