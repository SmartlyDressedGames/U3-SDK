////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorCopy
	{
		private Vector3 _position;
		public Vector3 position => _position;

		private Quaternion _rotation;
		public Quaternion rotation => _rotation;

		private Vector3 _scale;
		public Vector3 scale => _scale;

		private ObjectAsset _objectAsset;
		public ObjectAsset objectAsset => _objectAsset;

		private ItemAsset _itemAsset;
		public ItemAsset itemAsset => _itemAsset;

		public EditorCopy(Vector3 newPosition, Quaternion newRotation, Vector3 newScale, ObjectAsset newObjectAsset, ItemAsset newItemAsset)
		{
			_position = newPosition;
			_rotation = newRotation;
			_scale = newScale;
			_objectAsset = newObjectAsset;
			_itemAsset = newItemAsset;
		}
	}
}