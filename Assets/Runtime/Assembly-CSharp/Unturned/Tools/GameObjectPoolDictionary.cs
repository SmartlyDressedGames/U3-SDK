////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class GameObjectPoolDictionary
	{
		internal Dictionary<GameObject, GameObjectPool> pools;

		public PoolReference Instantiate(GameObject prefab)
		{
			return Instantiate(prefab, Vector3.zero, Quaternion.identity);
		}

		public PoolReference Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			GameObjectPool pool;
			if (!pools.TryGetValue(prefab, out pool))
			{
				pool = new GameObjectPool(prefab);
				pools.Add(prefab, pool);
			}

			return pool.Instantiate(position, rotation);
		}

		public void Instantiate(GameObject prefab, string name, int count)
		{
			GameObjectPool pool;
			if (!pools.TryGetValue(prefab, out pool))
			{
				pool = new GameObjectPool(prefab, count);
				pools.Add(prefab, pool);
			}

			GameObject[] temp = new GameObject[count];
			for (int index = 0; index < count; index++)
			{
				GameObject element = pool.Instantiate().gameObject;
				element.name = name;

				temp[index] = element;
			}

			for (int index = 0; index < count; index++)
			{
				pool.Destroy(temp[index].GetComponent<PoolReference>());
			}
		}

		public void Destroy(GameObject element)
		{
			if (element == null)
			{
				return;
			}

			PoolReference reference = element.GetComponent<PoolReference>();
			if (reference == null || reference.pool == null)
			{
				if (element.transform.parent != null)
				{
					EffectManager.UnregisterAttachment(element);
					element.transform.parent = null;
				}

				Object.Destroy(element);
				return;
			}

			reference.pool.Destroy(reference);
		}

		public void Destroy(GameObject element, float t)
		{
			if (element == null)
			{
				return;
			}

			PoolReference reference = element.GetComponent<PoolReference>();
			if (reference == null || reference.pool == null)
			{
				if (element.transform.parent != null)
				{
					EffectManager.UnregisterAttachment(element);
					element.transform.parent = null;
				}

				Object.Destroy(element);
				return;
			}

			reference.DestroyIntoPool(t);
		}

		public void DestroyAll()
		{
			foreach (GameObjectPool pool in pools.Values)
			{
				pool.DestroyAll();
			}
		}

		public void DestroyAllMatchingPrefab(GameObject prefab)
		{
			GameObjectPool pool;
			if (pools.TryGetValue(prefab, out pool))
			{
				pool.DestroyAll();
			}
		}

		public GameObjectPoolDictionary()
		{
			pools = new Dictionary<GameObject, GameObjectPool>();
		}
	}
}
