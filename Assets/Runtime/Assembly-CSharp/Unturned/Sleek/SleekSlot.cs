////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekSlot : SleekWrapper
	{
		public SelectedItem onSelectedItem;
		public GrabbedItem onGrabbedItem;
		public PlacedItem onPlacedItem;

		private ISleekSprite image;

		private SleekItem _item;
		public SleekItem item => _item;

		private byte _page;
		public byte page => _page;

		private bool _isItemEnabled = true;
		public bool isItemEnabled
		{
			get
			{
				ValidateNotDestroyed();
				return _isItemEnabled;
			}
			set
			{
				ValidateNotDestroyed();
				_isItemEnabled = value;
				if (item != null)
				{
					item.setEnabled(value);
				}
			}
		}

		public bool isImageRaycastTarget
		{
			get
			{
				ValidateNotDestroyed();
				return image.IsRaycastTarget;
			}
			set
			{
				ValidateNotDestroyed();
				image.IsRaycastTarget = value;
			}
		}

		public void select()
		{
			ValidateNotDestroyed();
			onPlacedItem?.Invoke(page, 0, 0);
		}

		public void updateItem(ItemJar jar)
		{
			ValidateNotDestroyed();
			if (item == null)
			{
				return;
			}

			item.updateItem(jar);
		}

		public void applyItem(ItemJar jar)
		{
			ValidateNotDestroyed();
			if (item != null)
			{
				image.RemoveChild(item);
				_item = null;
			}

			if (jar != null)
			{
				_item = new SleekItem(jar);
				item.PositionOffset_X = -jar.size_x * 25;
				item.PositionOffset_Y = -jar.size_y * 25;
				item.PositionScale_X = 0.5f;
				item.PositionScale_Y = 0.5f;
				item.updateHotkey(page);
				item.onClickedItem = onClickedItem;
				item.onDraggedItem = onDraggedItem;
				item.setEnabled(_isItemEnabled);
				image.AddChild(item); // Must be child of sprite for IMGUI click handling.
			}
		}

		private void onClickedItem(SleekItem item)
		{
			onSelectedItem?.Invoke(page, 0, 0);
		}

		private void onDraggedItem(SleekItem item)
		{
			onGrabbedItem?.Invoke(page, 0, 0, item);
		}

		public SleekSlot(byte newPage) : base()
		{
			_page = newPage;

			SizeOffset_X = 250;
			SizeOffset_Y = 150;

			image = Glazier.Get().CreateSprite();
			image.SizeScale_X = 1;
			image.SizeScale_Y = 1;
			image.DrawMethod = ESleekSpriteType.Sliced;
			image.Sprite = PlayerDashboardInventoryUI.icons.load<Sprite>("Slot_Sprite");
			image.TintColor = ESleekTint.FOREGROUND;
			image.OnClicked += select;
			AddChild(image);
		}
	}
}
