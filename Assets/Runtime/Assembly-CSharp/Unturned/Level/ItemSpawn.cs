////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class ItemSpawn
	{
		private ushort _item;
		public ushort item => _item;

		public ItemSpawn(ushort newItem)
		{
			_item = newItem;
		}
	}
}