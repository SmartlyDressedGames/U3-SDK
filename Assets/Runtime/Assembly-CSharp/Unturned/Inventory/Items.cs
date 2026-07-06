////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace SDG.Unturned
{
	public delegate void ItemsResized(byte page, byte newWidth, byte newHeight);
	public delegate void ItemUpdated(byte page, byte index, ItemJar jar);
	public delegate void ItemAdded(byte page, byte index, ItemJar jar);
	public delegate void ItemRemoved(byte page, byte index, ItemJar jar);
	public delegate void ItemDiscarded(byte page, byte index, ItemJar jar); // when outside bounds
	public delegate void StateUpdated();

	public class Items
	{
		public ItemsResized onItemsResized;
		public ItemUpdated onItemUpdated;
		public ItemAdded onItemAdded;

		/// <summary>
		/// Nelson 2025-02-24: Warning that this is invoked *before* the item is actually removed from the items list.
		/// (public issue #4894)
		/// </summary>
		public ItemRemoved onItemRemoved;

		public ItemDiscarded onItemDiscarded;
		public StateUpdated onStateUpdated;

		private byte _page;
		public byte page => _page;

		private byte _width;
		public byte width => _width;

		private byte _height;
		public byte height => _height;

		private bool[,] slots;
		public List<ItemJar> items
		{
			get;
			protected set;
		}

		public void updateAmount(byte index, byte newAmount)
		{
			if (index < 0 || index >= items.Count)
			{
				return;
			}

			items[index].item.amount = newAmount;

			onItemUpdated?.Invoke(page, index, items[index]);

			onStateUpdated?.Invoke();
		}

		public void updateQuality(byte index, byte newQuality)
		{
			if (index < 0 || index >= items.Count)
			{
				return;
			}

			items[index].item.quality = newQuality;

			onItemUpdated?.Invoke(page, index, items[index]);

			onStateUpdated?.Invoke();
		}

		public void updateState(byte index, byte[] newState)
		{
			if (index < 0 || index >= items.Count)
			{
				return;
			}

			items[index].item.state = newState;

			onItemUpdated?.Invoke(page, index, items[index]);

			onStateUpdated?.Invoke();
		}

		public byte getItemCount()
		{
			return (byte) items.Count;
		}

		public bool containsItem(ItemJar jar)
		{
			return items.Contains(jar);
		}

		public ItemJar getItem(byte index)
		{
			if (index < 0 || index >= items.Count)
			{
				return null;
			}

			return items[index];
		}

		public byte getIndex(byte x, byte y)
		{
			if (page < PlayerInventory.SLOTS)
			{
				return 0;
			}

			if (x < 0 || y < 0 || x >= width || y >= height)
			{
				return 255;
			}

			for (byte index = 0; index < items.Count; index++)
			{
				if (items[index].x == x && items[index].y == y)
				{
					return index;
				}
			}

			return 255;
		}

		internal int FindIndexOfJar(ItemJar jar)
		{
			for (int index = 0; index < items.Count; ++index)
			{
				if (items[index] == jar)
				{
					return index;
				}
			}

			return -1;
		}

		public byte findIndex(byte x, byte y, out byte find_x, out byte find_y)
		{
			find_x = 255;
			find_y = 255;

			if (x < 0 || y < 0 || x >= width || y >= height)
			{
				return 255;
			}

			for (byte index = 0; index < items.Count; index++)
			{
				// if the item's top left is less than our search its size might fit our point
				if (items[index].x <= x && items[index].y <= y)// && items[index].x + items[index].size_x > x && items[index].y + items[index].size_y > y)
				{
					byte size_x = items[index].size_x;
					byte size_y = items[index].size_y;
					if (items[index].rot % 2 == 1)
					{
						size_x = items[index].size_y;
						size_y = items[index].size_x;
					}

					if (items[index].x + size_x > x && items[index].y + size_y > y)
					{
						find_x = items[index].x;
						find_y = items[index].y;

						return index;
					}
				}
			}

			return 255;
		}

		public void SearchContents(in PlayerInventorySearchParameters parameters)
		{
			foreach (ItemJar jar in items)
			{
				Item item = jar?.item;
				if (item == null)
				{
					// Hopefully not the case, but this is old code and I don't trust it. ;)
					continue;
				}

				if (ReferenceEquals(jar, parameters.ItemToIgnore))
				{
					// If ItemToIgnore is null this was already handled above, or we are skipping the specified item.
					continue;
				}

				if (item.amount < 1 && !parameters.IncludeEmpty)
				{
					continue;
				}

				if (item.quality >= 100 && !parameters.IncludeMaxQuality)
				{
					continue;
				}

				ItemAsset asset = item.GetAsset();
				if (asset == null)
				{
					continue;
				}

				if (parameters.ExcludeFullAmount && item.amount >= asset.MaxAmount)
				{
					continue;
				}

				if (parameters.ItemType.HasValue && asset.type != parameters.ItemType.Value)
				{
					continue;
				}

				if (parameters.AssetRef.HasValue && !parameters.AssetRef.Value.IsReferenceTo(asset))
				{
					continue;
				}

				if (parameters.CaliberId.HasValue || parameters.AnyCaliberIds != null)
				{
					if (!(asset is ItemCaliberAsset caliberAsset))
					{
						continue;
					}

					if (caliberAsset.calibers == null || caliberAsset.calibers.Length < 1)
					{
						if (!parameters.IncludeUnspecifiedCaliber)
						{
							continue;
						}
					}
					else
					{
						if (parameters.CaliberId.HasValue)
						{
							if (!caliberAsset.CalibersContainId(parameters.CaliberId.Value))
							{
								continue;
							}
						}
						else
						{
							if (!caliberAsset.CalibersContainAnyOfIds(parameters.AnyCaliberIds))
							{
								continue;
							}
						}
					}
				}

				// Purpose of this is to double-check the results are properly pooled after it's been called a few
				// times. For example, that crafting refresh is not allocating memory here.
				Profiler.BeginSample("Items.SearchContents.AddResult");
				// Yay! All criteria were met.
				parameters.Results.Add(new PlayerInventorySearchResultV2(this, jar));
				Profiler.EndSample(); // Items.SearchContents.AddResult

				if (parameters.MaxResultsCount > 0 && parameters.Results.Count >= parameters.MaxResultsCount)
				{
					return;
				}
			}
		}

		internal void GatherUniqueItems(HashSet<ItemAsset> results)
		{
			foreach (ItemJar jar in items)
			{
				ItemAsset asset = jar?.GetAsset();
				if (asset != null)
				{
					results.Add(asset);
				}
			}
		}

		public void loadItem(byte x, byte y, byte rot, Item item)
		{
			ItemJar jar = new ItemJar(x, y, rot, item);

			fillSlot(jar, true);
			items.Add(jar);
		}

		public void addItem(byte x, byte y, byte rot, Item item)
		{
			ItemJar jar = new ItemJar(x, y, rot, item);

			fillSlot(jar, true);
			items.Add(jar);

			try
			{
				onItemAdded?.Invoke(page, (byte) (items.Count - 1), jar);
			}
			catch (System.Exception ex)
			{
				UnturnedLog.exception(ex, $"Caught exception during addItem (x: {x} y: {y} rot: {rot} item: {item?.id}):");
			}

			onStateUpdated?.Invoke();
		}

		public bool tryAddItem(Item item)
		{
			return tryAddItem(item, true);
		}

		public bool tryAddItem(Item item, bool isStateUpdatable)
		{
			if (getItemCount() >= 200)
			{
				return false;
			}

			byte x;
			byte y;
			byte rot;

			ItemJar jar = new ItemJar(item);

			if (!tryFindSpace(jar.size_x, jar.size_y, out x, out y, out rot))
			{
				return false;
			}

			jar.x = x;
			jar.y = y;
			jar.rot = rot;

			fillSlot(jar, true);
			items.Add(jar);

			try
			{
				onItemAdded?.Invoke(page, (byte) (items.Count - 1), jar);
			}
			catch (System.Exception ex)
			{
				UnturnedLog.exception(ex, $"Caught exception during tryAddItem ({item?.id}):");
			}

			if (isStateUpdatable)
			{
				onStateUpdated?.Invoke();
			}

			return true;
		}

		public void removeItem(byte index)
		{
			if (index < 0 || index >= items.Count)
			{
				return;
			}

			fillSlot(items[index], false);

			onItemRemoved?.Invoke(page, index, items[index]);

			items.RemoveAt(index);

			onStateUpdated?.Invoke();
		}

		public void clear()
		{
			items.Clear();
		}

		public void loadSize(byte newWidth, byte newHeight)
		{
			_width = newWidth;
			_height = newHeight;

			slots = new bool[width, height];
			for (byte x = 0; x < width; x++)
			{
				for (byte y = 0; y < height; y++)
				{
					slots[x, y] = false;
				}
			}

			List<ItemJar> newItems = new List<ItemJar>();

			if (items != null)
			{
				for (byte index = 0; index < items.Count; index++)
				{
					ItemJar jar = items[index];

					byte size_x = jar.size_x;
					byte size_y = jar.size_y;
					if (jar.rot % 2 == 1)
					{
						size_x = jar.size_y;
						size_y = jar.size_x;
					}

					if (width == 0 || height == 0 || (page >= PlayerInventory.SLOTS && (jar.x + size_x > width || jar.y + size_y > height)))
					{
						onItemDiscarded?.Invoke(page, index, jar);

						onStateUpdated?.Invoke();

						continue;
					}

					fillSlot(jar, true);
					newItems.Add(jar);
				}
			}

			items = newItems;
		}

		public void resize(byte newWidth, byte newHeight)
		{
			loadSize(newWidth, newHeight);

			onItemsResized?.Invoke(page, newWidth, newHeight);

			onStateUpdated?.Invoke();
		}

		/// checks whether a space contains any filled slots
		public bool checkSpaceEmpty(byte pos_x, byte pos_y, byte size_x, byte size_y, byte rot)
		{
			if (page < PlayerInventory.SLOTS)
			{
				return items.Count == 0;
			}

			if (rot % 2 == 1) // flipped
			{
				byte temp = size_x;
				size_x = size_y;
				size_y = temp;
			}

			for (byte grid_x = pos_x; grid_x < pos_x + size_x; grid_x++)
			{
				for (byte grid_y = pos_y; grid_y < pos_y + size_y; grid_y++)
				{
					if (grid_x >= width || grid_y >= height)
					{
						return false;
					}

					if (slots[grid_x, grid_y])
					{
						return false;
					}
				}
			}

			return true;
		}

		/// checks whether an item can be dragged and takes into account if the item overlaps its old self
		public bool checkSpaceDrag(byte old_x, byte old_y, byte oldRot, byte new_x, byte new_y, byte newRot, byte size_x, byte size_y, bool checkSame)
		{
			if (page < PlayerInventory.SLOTS)
			{
				return items.Count == 0 || checkSame;
			}

			byte oldSize_x = size_x;
			byte oldSize_y = size_y;
			if (oldRot % 2 == 1)
			{
				oldSize_x = size_y;
				oldSize_y = size_x;
			}

			byte newSize_x = size_x;
			byte newSize_y = size_y;
			if (newRot % 2 == 1)
			{
				newSize_x = size_y;
				newSize_y = size_x;
			}

			for (byte grid_x = new_x; grid_x < new_x + newSize_x; grid_x++)
			{
				for (byte grid_y = new_y; grid_y < new_y + newSize_y; grid_y++)
				{
					if (grid_x >= width || grid_y >= height)
					{
						return false;
					}

					if (slots[grid_x, grid_y]) // this spot is taken!
					{
						// find distance to old item
						int offset_x = grid_x - old_x;
						int offset_y = grid_y - old_y;

						// if we're not allowed to place in the same spot
						// OR
						// the offset fits into the old item space
						if (!checkSame || !(offset_x >= 0 && offset_y >= 0 && offset_x < oldSize_x && offset_y < oldSize_y))
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// checks whether the spot currently used by the old item is big enough to fit the new item
		/// </summary>
		public bool checkSpaceSwap(byte x, byte y, byte oldSize_X, byte oldSize_Y, byte oldRot, byte newSize_X, byte newSize_Y, byte newRot)
		{
			if (page < PlayerInventory.SLOTS)
			{
				return true;
			}

			if (oldRot % 2 == 1)
			{
				byte temp = oldSize_X;
				oldSize_X = oldSize_Y;
				oldSize_Y = temp;
			}

			if (newRot % 2 == 1)
			{
				byte temp = newSize_X;
				newSize_X = newSize_Y;
				newSize_Y = temp;
			}

			for (byte grid_x = x; grid_x < x + newSize_X; grid_x++)
			{
				for (byte grid_y = y; grid_y < y + newSize_Y; grid_y++)
				{
					if (grid_x >= width || grid_y >= height)
					{
						return false;
					}

					if (slots[grid_x, grid_y]) // this spot is taken!
					{
						int offset_x = grid_x - x;
						int offset_y = grid_y - y;

						// if the offset fits into the old item space
						if (!(offset_x >= 0 && offset_y >= 0 && offset_x < oldSize_X && offset_y < oldSize_Y))
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		public bool tryFindSpace(byte size_x, byte size_y, out byte x, out byte y, out byte rot)
		{
			x = 255;
			y = 255;
			rot = 0;

			if (page < PlayerInventory.SLOTS)
			{
				x = 0;
				y = 0;
				rot = 0;

				return items.Count == 0;
			}

			for (byte grid_y = 0; grid_y < height - size_y + 1; grid_y++)
			{
				for (byte grid_x = 0; grid_x < width - size_x + 1; grid_x++)
				{
					bool searched = false;

					for (byte offset_y = 0; offset_y < size_y; offset_y++)
					{
						if (searched)
						{
							break;
						}

						for (byte offset_x = 0; offset_x < size_x; offset_x++)
						{
							if (slots[grid_x + offset_x, grid_y + offset_y])
							{
								searched = true;
								break;
							}

							if (offset_x == size_x - 1 && offset_y == size_y - 1)
							{
								x = grid_x;
								y = grid_y;
								rot = 0;
								return true;
							}
						}
					}
				}
			}

			for (byte grid_y = 0; grid_y < height - size_x + 1; grid_y++)
			{
				for (byte grid_x = 0; grid_x < width - size_y + 1; grid_x++)
				{
					bool searched = false;

					for (byte offset_y = 0; offset_y < size_x; offset_y++)
					{
						if (searched)
						{
							break;
						}

						for (byte offset_x = 0; offset_x < size_y; offset_x++)
						{
							if (slots[grid_x + offset_x, grid_y + offset_y])
							{
								searched = true;
								break;
							}

							if (offset_x == size_y - 1 && offset_y == size_x - 1)
							{
								x = grid_x;
								y = grid_y;
								rot = 1;
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		private void fillSlot(ItemJar jar, bool isOccupied)
		{
			byte size_x = jar.size_x;
			byte size_y = jar.size_y;
			if (jar.rot % 2 == 1)
			{
				size_x = jar.size_y;
				size_y = jar.size_x;
			}

			for (byte x = 0; x < size_x; x++)
			{
				for (byte y = 0; y < size_y; y++)
				{
					if (jar.x + x < width && jar.y + y < height)
					{
						slots[jar.x + x, jar.y + y] = isOccupied;
					}
				}
			}
		}

		public Items(byte newPage)
		{
			_page = newPage;

			items = new List<ItemJar>();
		}

		#region OBSOLETE
		[System.Obsolete]
		public List<InventorySearch> search(List<InventorySearch> search, EItemType type, ushort caliber)
		{
			return this.search(search, type, caliber, true);
		}

		/// <summary>
		/// Please use SearchContents instead! To perform an equivalent search:
		/// • Set ItemType to type.
		/// • Set IncludeEmpty to false.
		/// • Set IncludeMaxQuality to true.
		/// </summary>
		[System.Obsolete("Please use new search API instead! Refer to this method's comment for more information.")]
		public List<InventorySearch> search(List<InventorySearch> search, EItemType type)
		{
			for (byte index = 0; index < items.Count; index++)
			{
				ItemJar jar = items[index];

				if (jar.item.amount > 0)
				{
					ItemAsset asset = jar.GetAsset();
					if (asset != null && asset.type == type)
					{
						search.Add(new InventorySearch(page, jar));
					}
				}
			}

			return search;
		}

		/// <summary>
		/// Please use SearchContents instead! To perform an equivalent search:
		/// • Set ItemType to type.
		/// • Set IncludeEmpty to false.
		/// • Set IncludeMaxQuality to true.
		/// • Set CaliberId to caliber.
		/// • Set IncludeUnspecifiedCaliber to allowZeroCaliber.
		/// </summary>
		[System.Obsolete("Please use new search API instead! Refer to this method's comment for more information.")]
		public List<InventorySearch> search(List<InventorySearch> search, EItemType type, ushort caliber, bool allowZeroCaliber)
		{
			for (byte index = 0; index < items.Count; index++)
			{
				ItemJar jar = items[index];

				if (jar.item.amount > 0)
				{
					bool alreadyContains = false;
					for (int checkIndex = 0; checkIndex < search.Count; checkIndex++)
					{
						if (search[checkIndex].page == page && search[checkIndex].jar.x == jar.x && search[checkIndex].jar.y == jar.y)
						{
							alreadyContains = true;
							break;
						}
					}

					if (!alreadyContains)
					{
						ItemAsset asset = jar.GetAsset();
						if (asset != null && asset.type == type)
						{
							if (((ItemCaliberAsset) asset).calibers.Length == 0)
							{
								if (allowZeroCaliber)
								{
									search.Add(new InventorySearch(page, jar));
								}
							}
							else
							{
								for (byte step = 0; step < ((ItemCaliberAsset) asset).calibers.Length; step++)
								{
									if (((ItemCaliberAsset) asset).calibers[step] == caliber)
									{
										search.Add(new InventorySearch(page, jar));
										break;
									}
								}
							}
						}
					}
				}
			}

			return search;
		}

		/// <summary>
		/// Please use SearchContents instead! To perform an equivalent search:
		/// • Set AssetRef to id.
		/// • Set IncludeEmpty to findEmpty.
		/// • Set IncludeMaxQuality to findHealthy.
		/// </summary>
		[System.Obsolete("Please use new search API instead! Refer to this method's comment for more information.")]
		public List<InventorySearch> search(List<InventorySearch> search, ushort id, bool findEmpty, bool findHealthy)
		{
			for (byte index = 0; index < items.Count; index++)
			{
				ItemJar jar = items[index];

				if (findEmpty || jar.item.amount > 0)
				{
					if (findHealthy || jar.item.quality < 100)
					{
						if (jar.item.id == id)
						{
							search.Add(new InventorySearch(page, jar));
						}
					}
				}
			}

			return search;
		}

		/// <summary>
		/// Please use SearchContents instead! To perform an equivalent search:
		/// • Set MaxResultsCount to 1.
		/// • Set AssetRef to id.
		/// • Set IncludeEmpty to false.
		/// • Set IncludeMaxQuality to true.
		/// </summary>
		[System.Obsolete("Please use new search API instead! Refer to this method's comment for more information.")]
		public InventorySearch has(ushort id)
		{
			for (byte index = 0; index < items.Count; index++)
			{
				ItemJar jar = items[index];

				if (jar.item.amount > 0)
				{
					if (jar.item.id == id)
					{
						return new InventorySearch(page, jar);
					}
				}
			}

			return null;
		}
		#endregion OBSOLETE
	}
}
