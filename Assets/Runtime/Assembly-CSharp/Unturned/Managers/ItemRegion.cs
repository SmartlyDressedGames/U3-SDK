////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class ItemRegion
	{
		private List<ItemDrop> _drops;
		public List<ItemDrop> drops => _drops;

		public List<ItemData> items;

		public bool isNetworked;
		internal bool isPendingDestroy;

		public ushort despawnItemIndex;
		public ushort respawnItemIndex;

		public float lastRespawn;

		[System.Obsolete("Renamed to DestroyAll")]
		public void destroy()
		{
			DestroyAll();
		}

		internal void DestroyTail()
		{
			ItemDrop item = drops.GetAndRemoveTail();
			Object.Destroy(item.model.gameObject);
		}

		public void DestroyAll()
		{
			for (ushort index = 0; index < drops.Count; index++)
			{
				Object.Destroy(drops[index].model.gameObject);
			}

			drops.Clear();
		}

		public ItemRegion()
		{
			_drops = new List<ItemDrop>();
			items = new List<ItemData>();

			isNetworked = false;
			isPendingDestroy = false;

			lastRespawn = Time.realtimeSinceStartup;

			despawnItemIndex = 0;
			respawnItemIndex = 0;
		}
	}
}
