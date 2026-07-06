////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class BlueprintOutput
	{
		/// <summary>
		/// Note: if calling ItemRef.Get() please use FindItemAsset instead to avoid redundant asset lookups.
		/// </summary>
		public CachingBcAssetRef ItemRef
		{
			get => _itemRef;
			internal set => _itemRef = value;
		}
		private CachingBcAssetRef _itemRef;
		[System.Obsolete("Please use FindItemAsset instead for GUID support")]
		public ushort id => _itemRef.LegacyId;

		public int amount;
		public EItemOrigin origin;

		public ItemAsset FindItemAsset()
		{
			return _itemRef.Get<ItemAsset>();
		}

		public T FindItemAsset<T>() where T : ItemAsset
		{
			return _itemRef.Get<T>();
		}

		/// <summary>
		/// Does this blueprint output create the specified item?
		/// </summary>
		public bool IsItem(ItemAsset asset)
		{
			if (asset == null)
				return false;

			return _itemRef.IsReferenceTo(asset);
		}

		public BlueprintOutput(ushort newID, int newAmount, EItemOrigin newOrigin)
		{
			amount = newAmount;
			origin = newOrigin;
		}
	}
}
