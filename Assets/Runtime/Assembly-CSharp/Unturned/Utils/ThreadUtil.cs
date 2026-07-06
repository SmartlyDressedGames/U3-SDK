////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Threading;
using UnityEngine;

namespace SDG.Unturned
{
	public static class ThreadUtil
	{
		public static Thread gameThread
		{
			get;
			private set;
		}

		/// <summary>
		/// Called once by Setup.
		/// </summary>
		public static void setupGameThread()
		{
			if (gameThread == null)
			{
				gameThread = Thread.CurrentThread;
			}
			else
			{
				throw new Exception("gameThread has already been setup");
			}

			GameThreadQueueUtil.Setup();
		}

		/// <summary>
		/// Extension method for Thread class. Plugins use this.
		/// </summary>
		public static bool IsGameThread(this Thread thread)
		{
			return thread == gameThread;
		}

		/// <summary>
		/// Throw an exception if current thread is not the game thread.
		/// </summary>
		public static void assertIsGameThread()
		{
			if (Thread.CurrentThread != gameThread)
			{
				throw new NotSupportedException("This function should only be called from the game thread. (e.g. from Unity's Update)");
			}
		}

		/// <summary>
		/// Only on dedicated server: throw an exception if current thread is not the game thread.
		/// </summary>
		[System.Diagnostics.Conditional("WITH_GAME_THREAD_ASSERTIONS")]
		internal static void ConditionalAssertIsGameThread()
		{
			if (Thread.CurrentThread != gameThread)
			{
				throw new NotSupportedException("This function should only be called from the game thread. (e.g. from Unity's Update)");
			}
		}
	}

	internal class GameThreadQueueUtil : MonoBehaviour
	{
		internal static void Setup()
		{
			GameObject gameObject = new GameObject("ThreadUtil");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			instance = gameObject.AddComponent<GameThreadQueueUtil>();
		}

		internal static void QueueGameThreadWorkItem(WaitCallback callback, object state)
		{
			instance.workItems.Enqueue(new WorkItem() { callback = callback, state = state });
		}

		private void Update()
		{
			if (workItems.TryDequeue(out WorkItem workItem) && workItem.callback != null)
			{
				workItem.callback.Invoke(workItem.state);
			}
		}

		private struct WorkItem
		{
			public WaitCallback callback;
			public object state;
		}

		private System.Collections.Concurrent.ConcurrentQueue<WorkItem> workItems = new System.Collections.Concurrent.ConcurrentQueue<WorkItem>();
		private static GameThreadQueueUtil instance;
	}
}
