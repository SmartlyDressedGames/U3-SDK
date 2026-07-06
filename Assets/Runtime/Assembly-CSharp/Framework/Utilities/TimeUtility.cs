////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace SDG.Framework.Utilities
{
	public delegate void UpdateHandler();

	public class TimeUtility : MonoBehaviour
	{
		/// <summary>
		/// Equivalent to MonoBehaviour.Update
		/// </summary>
		public static event UpdateHandler updated;

		/// <summary>
		/// Equivalent to MonoBehaviour.FixedUpdate
		/// </summary>
		public static event UpdateHandler physicsUpdated;

		/// <summary>
		/// Useful when caller is not a MonoBehaviour, or coroutine should not be owned by a component which might get
		/// deactivated. For example attached effects destroy timer should happen regardless of parent deactivation.
		/// </summary>
		public static Coroutine InvokeAfterDelay(System.Action callback, float timeSeconds)
		{
			return singleton.StartCoroutine(singleton.InternalInvokeAfterDelay(callback, timeSeconds));
		}

		/// <summary>
		/// Stop a coroutine started by InvokeAfterDelay.
		/// </summary>
		public static void StaticStopCoroutine(Coroutine routine)
		{
			// Nelson 2024-06-05: singleton can be null if this was called from OnDestroy.
			if (singleton != null)
			{
				singleton.StopCoroutine(routine);
			}
		}

		protected virtual void triggerUpdated()
		{
			updated?.Invoke();
		}

		protected virtual void Update()
		{
			updated?.Invoke();
		}

		protected virtual void FixedUpdate()
		{
			physicsUpdated?.Invoke();
		}

		private IEnumerator InternalInvokeAfterDelay(System.Action callback, float timeSeconds)
		{
			yield return new WaitForSeconds(timeSeconds);
			callback();
		}

		private void Awake()
		{
			singleton = this;
		}

		private static TimeUtility singleton;
	}
}
