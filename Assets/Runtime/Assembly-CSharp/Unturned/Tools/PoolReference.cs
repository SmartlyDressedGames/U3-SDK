////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Utilities;
using UnityEngine;
using System.Collections.Generic;

namespace SDG.Unturned
{
	public class PoolReference : MonoBehaviour
	{
		public GameObjectPool pool;
		public bool inPool;

		/// <summary>
		/// Enabled for effects held by guns and sentries.
		/// </summary>
		public bool excludeFromDestroyAll;

		public void DestroyIntoPool(float t)
		{
			CancelDestroyTimer();

			if (pool == null)
			{
				Destroy(gameObject, t);
				return;
			}

			if (gameObject.activeInHierarchy)
			{
				// Used rather than StartCoroutine because we want callback to happen even if parent is deactivated.
				// (parent deactivating stops component owned coroutines)
				invokeAfterDelayCoroutine = TimeUtility.InvokeAfterDelay(DestroyIntoPoolCallback, t);
			}
			else
			{
				// Parent is disabled? Cannot start coroutine without a Unity warning.
				pool.Destroy(this);
			}
		}

		internal void CancelDestroyTimer()
		{
			if (invokeAfterDelayCoroutine != null)
			{
				TimeUtility.StaticStopCoroutine(invokeAfterDelayCoroutine);
				invokeAfterDelayCoroutine = null;
			}
		}

		private void DestroyIntoPoolCallback()
		{
			// Reset coroutine reference to prevent CancelDestroyTimer from wasting
			// time looking up the coroutine on the singleton gameobject.
			invokeAfterDelayCoroutine = null;

			if (pool == null)
			{
				Destroy(gameObject);
			}
			else
			{
				pool.Destroy(this);
			}
		}

		/// <summary>
		/// Listen for OnDestroy callback because mods may be destroying themselves in unexpected ways (e.g., Grenade
		/// component) and still need to be cleaned up.
		/// </summary>
		private void OnDestroy()
		{
			// Nelson 2024-06-05: Important to cancel timer, otherwise DestroyIntoPoolCallback was being called and an
			// exception thrown because the gameObject was destroyed.
			CancelDestroyTimer();

			if (Level.isExiting)
			{
				// Don't worry about cleanup when pool will be rebuilt for next level anyway.
				return;
			}

			if (transform.parent != null)
			{
				EffectManager.UnregisterAttachment(gameObject);
			}

			// If inPool it will be cleaned up while popping all effects in queue.
			if (pool != null && !inPool)
			{
				pool.active.RemoveFast(this);
				pool = null;
			}
		}

		private Coroutine invokeAfterDelayCoroutine;
	}
}
