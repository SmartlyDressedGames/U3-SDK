////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemDrop
	{
		private Transform _model;
		public Transform model => _model;

		private InteractableItem _interactableItem;
		public InteractableItem interactableItem => _interactableItem;

		private uint _instanceID;
		public uint instanceID => _instanceID;

		public ItemDrop(Transform newModel, InteractableItem newInteractableItem, uint newInstanceID)
		{
			_model = newModel;
			_interactableItem = newInteractableItem;
			_instanceID = newInstanceID;
		}
	}
}