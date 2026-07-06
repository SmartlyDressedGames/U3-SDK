////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ReunObjectRemove : IReun
	{
		public int step
		{
			get;
			private set;
		}

		private Transform model;
		private ObjectAsset objectAsset;
		private ItemAsset itemAsset;
		private Vector3 position;
		private Quaternion rotation;
		private Vector3 scale;

		public Transform redo()
		{
			if (model != null)
			{
				if (objectAsset != null)
				{
					LevelObjects.removeObject(model);
					model = null;
				}
				else if (itemAsset != null)
				{
					LevelObjects.removeBuildable(model);
					model = null;
				}
			}

			return null;
		}

		public void undo()
		{
			if (model == null)
			{
				if (objectAsset != null)
				{
					model = LevelObjects.addObject(position, rotation, scale, objectAsset.id, objectAsset.GUID, ELevelObjectPlacementOrigin.MANUAL);
				}
				else if (itemAsset != null)
				{
					model = LevelObjects.addBuildable(position, rotation, itemAsset.id);
				}
			}
		}

		public ReunObjectRemove(int newStep, Transform newModel, ObjectAsset newObjectAsset, ItemAsset newItemAsset, Vector3 newPosition, Quaternion newRotation, Vector3 newScale)
		{
			step = newStep;
			model = newModel;
			objectAsset = newObjectAsset;
			itemAsset = newItemAsset;
			position = newPosition;
			rotation = newRotation;
			scale = newScale;
		}
	}
}
