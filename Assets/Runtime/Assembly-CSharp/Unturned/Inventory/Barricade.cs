////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class Barricade
	{
		public bool isDead => health == 0;

		public bool isRepaired => health == asset.health;

		[System.Obsolete]
		public ushort id => asset.id;

		public ushort health;
		public byte[] state;

		public ItemBarricadeAsset asset
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
		public Barricade(ushort newID)
		{
			asset = Assets.find(EAssetType.ITEM, newID) as ItemBarricadeAsset;

			if (asset == null)
			{
				health = 0;
				state = new byte[0];
				return;
			}

			health = asset.health;
			state = asset.getState();
		}

		[System.Obsolete]
		public Barricade(ushort newID, ushort newHealth, byte[] newState, ItemBarricadeAsset newAsset)
		{
			health = newHealth;
			state = newState;
			asset = newAsset;
		}

		public Barricade(ItemBarricadeAsset newAsset)
		{
			asset = newAsset;
			if (asset != null)
			{
				health = asset.health;
				state = asset.getState();
			}
			else
			{
				health = 0;
				state = new byte[0];
			}
		}

		public Barricade(ItemBarricadeAsset newAsset, ushort newHealth, byte[] newState)
		{
			asset = newAsset;
			health = newHealth;
			state = newState;
		}

		public override string ToString()
		{
			return asset + " " + health + " " + state.Length;
		}
	}
}
