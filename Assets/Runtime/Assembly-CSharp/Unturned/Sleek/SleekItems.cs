////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void SelectedItem(byte page, byte x, byte y);
	public delegate void GrabbedItem(byte page, byte x, byte y, SleekItem item);
	public delegate void PlacedItem(byte page, byte x, byte y);

	public class SleekItems : SleekWrapper
	{
		public SelectedItem onSelectedItem;
		public GrabbedItem onGrabbedItem;
		public PlacedItem onPlacedItem;

		private ISleekElement itemsPanel;
		private ISleekSprite grid;
		private ISleekScrollView horizontalScrollView;

		private byte _page;
		public byte page => _page;

		private byte _width;
		public byte width => _width;

		private byte _height;
		public byte height => _height;

		private List<SleekItem> _items;
		public List<SleekItem> items => _items;

		/// <summary>
		/// Rather than creating all SleekItems as once we create a few per frame.
		/// </summary>
		private List<ItemJar> pendingItems;

		private bool _areItemsEnabled = true;
		public bool areItemsEnabled
		{
			get
			{
				ValidateNotDestroyed();
				return _areItemsEnabled;
			}
			set
			{
				ValidateNotDestroyed();
				_areItemsEnabled = value;
				foreach (SleekItem item in _items)
				{
					item.setEnabled(_areItemsEnabled);
				}
			}
		}

		public bool isGridRaycastTarget
		{
			get
			{
				ValidateNotDestroyed();
				return grid.IsRaycastTarget;
			}
			set
			{
				ValidateNotDestroyed();
				grid.IsRaycastTarget = value;
			}
		}

		/// <summary>
		/// Reset all items hotkey label.
		/// </summary>
		public void resetHotkeyDisplay()
		{
			ValidateNotDestroyed();
			foreach (SleekItem item in _items)
			{
				if (item.hotkey != byte.MaxValue)
				{
					item.updateHotkey(byte.MaxValue);
				}
			}
		}

		public void updateHotkey(ItemJar jar, byte button)
		{
			ValidateNotDestroyed();
			int index = indexOfItemElement(jar);
			if (index >= 0)
			{
				items[index].updateHotkey(button);
			}
			else
			{
				int queueIndex = pendingItems.IndexOf(jar);
				if (queueIndex >= 0)
				{
					pendingItems.RemoveAtFast(queueIndex);
					SleekItem item = createElementForItem(jar);
					item.updateHotkey(button);
				}
				else
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					UnturnedLog.error("Item hotkeyed to {0} has no element and not in the queue", button);
#endif
				}
			}
		}

		public void resize(byte newWidth, byte newHeight)
		{
			ValidateNotDestroyed();
			_width = newWidth;
			_height = newHeight;

			horizontalScrollView.ContentSizeOffset = new Vector2(width * 50.0f, height * 50.0f);
			SizeOffset_Y = (height * 50) + 30;
			grid.TileRepeatHintForUITK = new Vector2Int(newWidth, newHeight);
		}

		public void clear()
		{
			ValidateNotDestroyed();
			items.Clear();
			itemsPanel.RemoveAllChildren();
			pendingItems.Clear();
		}

		public void updateItem(ItemJar jar)
		{
			ValidateNotDestroyed();
			// Item may still be in the queue.
			int index = indexOfItemElement(jar);
			if (index >= 0)
			{
				items[index].updateItem(jar);
			}
		}

		public void addItem(ItemJar jar)
		{
			ValidateNotDestroyed();
			pendingItems.Add(jar);
		}

		public void removeItem(ItemJar jar)
		{
			ValidateNotDestroyed();
			int index = indexOfItemElement(jar);
			if (index >= 0)
			{
				itemsPanel.RemoveChild(items[index]);
				items.RemoveAtFast(index);
			}
			else
			{
				// Element for this item does not exist yet.
				pendingItems.RemoveFast(jar);
			}
		}

		public override void OnUpdate()
		{
			const int elementsPerUpdate = 5;
			int endIndex = Mathf.Max(0, pendingItems.Count - elementsPerUpdate);
			for (int index = pendingItems.Count - 1; index >= endIndex; --index)
			{
				ItemJar jar = pendingItems[index];
				pendingItems.RemoveAt(index);
				createElementForItem(jar);
			}
		}

		private int indexOfItemElement(ItemJar jar)
		{
			int match_x = jar.x * 50;
			int match_y = jar.y * 50;
			for (int index = 0; index < items.Count; index++)
			{
				// One would think that we could compare item.jar == jar, but this is really old code,
				// so be careful...
				if (items[index].PositionOffset_X == match_x && items[index].PositionOffset_Y == match_y)
				{
					return index;
				}
			}

			return -1;
		}

		private SleekItem createElementForItem(ItemJar jar)
		{
			SleekItem itemBox = new SleekItem(jar);
			itemBox.PositionOffset_X = jar.x * 50;
			itemBox.PositionOffset_Y = jar.y * 50;
			itemBox.onClickedItem = onClickedItem;
			itemBox.onDraggedItem = onDraggedItem;
			itemsPanel.AddChild(itemBox);
			itemBox.setEnabled(_areItemsEnabled);
			items.Add(itemBox);
			return itemBox;
		}

		private void onClickedItem(SleekItem item)
		{
			onSelectedItem?.Invoke(page, (byte) (item.PositionOffset_X / 50), (byte) (item.PositionOffset_Y / 50));
		}

		private void onDraggedItem(SleekItem item)
		{
			onGrabbedItem?.Invoke(page, (byte) (item.PositionOffset_X / 50), (byte) (item.PositionOffset_Y / 50), item);
		}

		private void onClickedGrid()
		{
			Vector2 cursorPosition = grid.GetNormalizedCursorPosition();

			byte x = (byte) (cursorPosition.x * width);
			byte y = (byte) (cursorPosition.y * height);

			onPlacedItem?.Invoke(page, x, y);
		}

		public SleekItems(byte newPage) : base()
		{
			_page = newPage;
			_items = new List<SleekItem>();
			pendingItems = new List<ItemJar>();

			SizeScale_X = 1.0f; // Horizontal view takes up entire width.

			horizontalScrollView = Glazier.Get().CreateScrollView();
			horizontalScrollView.SizeScale_X = 1.0f;
			horizontalScrollView.SizeScale_Y = 1.0f;
			horizontalScrollView.HandleScrollWheel = false;
			AddChild(horizontalScrollView);

			grid = Glazier.Get().CreateSprite();
			grid.SizeScale_X = 1;
			grid.SizeScale_Y = 1;
			grid.Sprite = PlayerDashboardInventoryUI.icons.load<Sprite>("Grid_Sprite");
			grid.OnClicked += onClickedGrid;
			grid.TintColor = ESleekTint.FOREGROUND;
			horizontalScrollView.AddChild(grid);

			itemsPanel = Glazier.Get().CreateFrame();
			itemsPanel.SizeScale_X = 1.0f;
			itemsPanel.SizeScale_Y = 1.0f;
			grid.AddChild(itemsPanel); // Must be child of sprite for IMGUI click handling.
		}
	}
}
