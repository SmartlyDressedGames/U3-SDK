////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemJar
	{
		public byte x;
		public byte y;
		public byte rot;

		public byte size_x;
		public byte size_y;

		private Item _item;
		public Item item => _item;

		public InteractableItem interactableItem;

		public ItemAsset GetAsset()
		{
			return _item != null ? _item.GetAsset() : null;
		}

		public T GetAsset<T>() where T : ItemAsset
		{
			return _item != null ? _item.GetAsset<T>() : null;
		}

		public ItemJar(Item newItem)
		{
			_item = newItem;

			ItemAsset asset = item.GetAsset();

			if (asset == null)
			{
				return;
			}

			size_x = asset.size_x;
			size_y = asset.size_y;
		}

		public ItemJar(byte new_x, byte new_y, byte newRot, Item newItem)
		{
			x = new_x;
			y = new_y;
			rot = newRot;

			_item = newItem;

			ItemAsset asset = item.GetAsset();

			if (asset == null)
			{
				return;
			}

			size_x = asset.size_x;
			size_y = asset.size_y;
		}
	}
}
