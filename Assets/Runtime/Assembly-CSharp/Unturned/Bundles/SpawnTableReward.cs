////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public struct SpawnTableRewardEnumerator : IEnumerable<ushort>, IEnumerator<ushort>
	{
		public SpawnTableRewardEnumerator(ushort tableID, int count)
		{
			this.tableID = tableID;
			assetID = 0;
			this.count = count;
			index = -1;
		}

		public ushort tableID;
		public ushort assetID;
		public int count;
		public int index;

		public ushort Current => assetID;

		object IEnumerator.Current => assetID;

		public void Dispose()
		{ }

		public IEnumerator<ushort> GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			while (++index < count)
			{
				assetID = SpawnTableTool.ResolveLegacyId(tableID, EAssetType.ITEM, OnGetSpawnTableErrorContext);
				if (assetID != 0)
				{
					return true;
				}
			}

			return false;
		}

		public void Reset()
		{
			index = -1;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		private string OnGetSpawnTableErrorContext()
		{
			return "consumable item";
		}
	}

	/// <summary>
	/// Nelson 2023-08-11: this probably should be rewritten a bit if used in the future
	/// because the error context currently assumes this is an item reward for consumables.
	/// </summary>
	public struct SpawnTableReward
	{
		public SpawnTableReward(ushort tableID, int min, int max)
		{
			this.tableID = tableID;
			this.min = min;
			this.max = max;
		}

		public ushort tableID;
		public int min;
		public int max;

		public int count()
		{
			return Random.Range(min, max + 1);
		}

		public int count(float multiplier)
		{
			return Mathf.CeilToInt(count() * multiplier);
		}

		/// <summary>
		/// Resolve table as items and grant random number to player.
		/// </summary>
		public void grantItems(Player player, EItemOrigin itemOrigin, bool shouldAutoEquip)
		{
			foreach (ushort itemID in spawn())
			{
				player.inventory.forceAddItem(new Item(itemID, itemOrigin), shouldAutoEquip, false);
			}
		}

		/// <summary>
		/// Resolve table as items and grant random number to player.
		/// </summary>
		public void grantItems(Player player, EItemOrigin itemOrigin, bool shouldAutoEquip, float countMultiplier)
		{
			foreach (ushort itemID in spawn(countMultiplier))
			{
				player.inventory.forceAddItem(new Item(itemID, itemOrigin), shouldAutoEquip, false);
			}
		}

		/// <summary>
		/// Enumerate random number of valid assetIDs.
		/// </summary>
		public SpawnTableRewardEnumerator spawn()
		{
			return new SpawnTableRewardEnumerator(tableID, count());
		}

		public SpawnTableRewardEnumerator spawn(float multiplier)
		{
			return new SpawnTableRewardEnumerator(tableID, count(multiplier));
		}
	}
}
