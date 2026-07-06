////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemData
	{
		private Item _item;
		public Item item => _item;

		private uint _instanceID;
		public uint instanceID => _instanceID;

		private Vector3 _point;
		public Vector3 point => _point;

		private bool _isDropped;
		public bool isDropped => _isDropped;

		private float _lastDropped;
		public float lastDropped => _lastDropped;

		public ItemData(Item newItem, uint newInstanceID, Vector3 newPoint, bool newDropped)
		{
			_item = newItem;
			_instanceID = newInstanceID;
			_point = newPoint;
			_isDropped = newDropped;

			_lastDropped = Time.realtimeSinceStartup;
		}
	}
}