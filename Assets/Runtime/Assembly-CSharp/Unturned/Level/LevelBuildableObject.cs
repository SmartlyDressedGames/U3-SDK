////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class LevelBuildableObject
	{
		private static List<Collider> colliders = new List<Collider>();

		public Vector3 point;
		public Quaternion rotation;

		private Transform _transform;
		public Transform transform => _transform;

		private ushort _id;
		public ushort id => _id;

		private ItemAsset _asset;
		public ItemAsset asset => _asset;

		public bool isEnabled
		{
			get;
			private set;
		}

		public void enable()
		{
			isEnabled = true;

			if (transform != null)
			{
				transform.gameObject.SetActive(true);
			}
		}

		public void disable()
		{
			isEnabled = false;

			if (transform != null)
			{
				transform.gameObject.SetActive(false);
			}
		}

		public void destroy()
		{
			if (transform != null)
			{
				GameObject.Destroy(transform.gameObject);
			}
		}

		public LevelBuildableObject(Vector3 newPoint, Quaternion newRotation, ushort newID)
		{
			point = newPoint;
			rotation = newRotation;
			_id = newID;

			_asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
			if (asset == null || asset.id != id)
			{
				_asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;

				if (asset == null)
				{
					return;
				}
			}

			if (Level.isEditor)
			{
				ItemBarricadeAsset barricadeAsset = asset as ItemBarricadeAsset;
				ItemStructureAsset structureAsset = asset as ItemStructureAsset;

				GameObject prefab = null;
				if (barricadeAsset != null)
				{
					prefab = barricadeAsset.barricade;
				}
				else if (structureAsset != null)
				{
					prefab = structureAsset.structure;
				}

				if (prefab != null)
				{
					GameObject gameObject = Object.Instantiate(prefab, newPoint, newRotation);
					_transform = gameObject.transform;
					gameObject.name = id.ToString();

					Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
					if (rigidbody == null)
					{
						rigidbody = transform.gameObject.AddComponent<Rigidbody>();
						rigidbody.useGravity = false;
						rigidbody.isKinematic = true;
					}

					transform.gameObject.SetActive(false);

					colliders.Clear();
					transform.GetComponentsInChildren(true, colliders);
					for (int index = 0; index < colliders.Count; index++)
					{
						if (colliders[index].gameObject.layer == LayerMasks.BARRICADE || colliders[index].gameObject.layer == LayerMasks.STRUCTURE)
						{
							continue;
						}

						Object.Destroy(colliders[index].gameObject);
					}
				}
			}
		}
	}
}
