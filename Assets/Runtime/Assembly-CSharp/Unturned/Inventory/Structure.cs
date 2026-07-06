////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class Structure
	{
		public bool isDead => health == 0;

		public bool isRepaired => health == asset.health;

		[System.Obsolete]
		public ushort id => asset.id;

		public ushort health;

		public ItemStructureAsset asset
		{
			get;
			private set;
		}

		public void askDamage(ushort amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (amount >= health)
			{
				health = 0;
			}
			else
			{
				health -= amount;
			}
		}

		public void askRepair(ushort amount)
		{
			if (amount == 0 || isDead)
			{
				return;
			}

			if (amount >= asset.health - health)
			{
				health = asset.health;
			}
			else
			{
				health += amount;
			}
		}

		[System.Obsolete]
		public Structure(ushort newID)
		{
			asset = Assets.find(EAssetType.ITEM, newID) as ItemStructureAsset;
			health = asset.health;
		}

		[System.Obsolete]
		public Structure(ushort newID, ushort newHealth, ItemStructureAsset newAsset)
		{
			health = newHealth;
			asset = newAsset;
		}

		public Structure(ItemStructureAsset newAsset, ushort newHealth)
		{
			asset = newAsset;
			health = newHealth;
		}

		public override string ToString()
		{
			return asset + " " + health;
		}
	}
}
