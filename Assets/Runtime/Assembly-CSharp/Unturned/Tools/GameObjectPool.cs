////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class GameObjectPool
	{
		private GameObject prefab;
		internal Stack<GameObject> pool;
		internal List<PoolReference> active;

		public PoolReference Instantiate()
		{
			return Instantiate(Vector3.zero, Quaternion.identity);
		}

		public PoolReference Instantiate(Vector3 position, Quaternion rotation)
		{
			while (pool.Count > 0)
			{
				GameObject oldElement = pool.Pop();
				if (oldElement == null)
				{
					// Can happen if something else destroyed it.
					// For example, modded effects doing something unexpected.
					continue;
				}
				
				oldElement.transform.parent = null;
				oldElement.transform.position = position;
				oldElement.transform.rotation = rotation;
				oldElement.transform.localScale = Vector3.one;
				oldElement.SetActive(true);

				PoolReference oldReference = oldElement.GetComponent<PoolReference>();
				oldReference.inPool = false;
				oldReference.excludeFromDestroyAll = false;
				active.Add(oldReference);

				return oldReference;
			}

			GameObject newElement = Object.Instantiate(prefab, position, rotation);
			PoolReference reference = newElement.AddComponent<PoolReference>();
			reference.pool = this;
			reference.inPool = false;
			active.Add(reference);
			return reference;
		}

		public void Destroy(PoolReference reference)
		{
			if (reference == null || reference.inPool || reference.pool != this)
			{
				return;
			}

			reference.CancelDestroyTimer();

			GameObject element = reference.gameObject;
			element.SetActive(false);

			// Restore parent to prevent effect from being destroyed if it was attached.
			if (element.transform.parent != null)
			{
				EffectManager.UnregisterAttachment(element);
				element.transform.parent = null;
			}

			pool.Push(element);
			active.RemoveFast(reference);

			reference.inPool = true;
			reference.excludeFromDestroyAll = false;
		}

		public void DestroyAll()
		{
			for (int index = active.Count - 1; index >= 0; --index)
			{
				PoolReference reference = active[index];
				if (reference == null || reference.gameObject == null)
				{
					// May have been destroyed when parent was destroyed, or something else went wrong...
					active.RemoveAtFast(index);
					continue;
				}

				if (reference.excludeFromDestroyAll)
				{
					// Skip object and do not remove from active list.
					continue;
				}

				// Effects were getting returned to the pool with their lifespan timer still running.
				// (public issue #3662)
				reference.CancelDestroyTimer();

				GameObject element = reference.gameObject;
				element.SetActive(false);

				// Restore parent to prevent effect from being destroyed if it was attached.
				if (element.transform.parent != null)
				{
					EffectManager.UnregisterAttachment(element);
					element.transform.parent = null;
				}

				pool.Push(element);
				active.RemoveAtFast(index);
				reference.inPool = true;
			}
		}

		public GameObjectPool(GameObject prefab) : this(prefab, 1)
		{ }

		public GameObjectPool(GameObject prefab, int count)
		{
			this.prefab = prefab;
			pool = new Stack<GameObject>(count);
			active = new List<PoolReference>(count);
		}
	}
}
