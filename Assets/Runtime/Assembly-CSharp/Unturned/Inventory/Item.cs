////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public enum EItemOrigin
	{
		WORLD,  // false
		ADMIN,  // true
		CRAFT,  // new: crafted, different because canteens/fuel shouldn't be filled from crafting them
		NATURE  // same as admin for now
	}

	public static class EItemOriginEx
	{
		public static string ToStringPascalCase(this EItemOrigin origin)
		{
			switch (origin)
			{
				case EItemOrigin.WORLD:
					return "World";

				case EItemOrigin.ADMIN:
					return "Admin";

				case EItemOrigin.CRAFT:
					return "Craft";

				case EItemOrigin.NATURE:
					return "Nature";

				default:
					return origin.ToString();
			}
		}
	}

	public class Item
	{
		private ushort _id;
		public ushort id => _id;

		public byte amount;
		public byte quality;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public byte durability
		{
			get => quality;
			set => quality = value;
		}

		public byte[] state;

		/// <summary>
		/// Exposed for Rocket transition to modules backwards compatibility.
		/// </summary>
		public byte[] metadata
		{
			get => state;
			set => state = value;
		}

		public ItemAsset GetAsset()
		{
			return Assets.find(EAssetType.ITEM, id) as ItemAsset;
		}

		public T GetAsset<T>() where T : ItemAsset
		{
			return Assets.find(EAssetType.ITEM, id) as T;
		}

		public Item(ushort newID, bool full) : this(newID, full ? EItemOrigin.ADMIN : EItemOrigin.WORLD)
		{ }

		/// <summary>
		/// Ideally in a future rewrite asset overload will become the default rather than the overload taking legacy ID.
		/// </summary>
		public Item(ItemAsset asset, EItemOrigin origin) : this(asset?.id ?? 0, origin)
		{ }

		public Item(ushort newID, EItemOrigin origin)
		{
			_id = newID;

			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;

			if (asset == null)
			{
				state = new byte[0]; // All code assumes state is not null.
				return;
			}

			if (origin != EItemOrigin.WORLD)
			{
				amount = MathfEx.Max(asset.MaxAmountAsByte, 1);
			}
			else
			{
				amount = MathfEx.Max(asset.count, 1);
			}

			if (origin != EItemOrigin.WORLD || ShouldItemTypeSpawnAtFullQuality(asset.type))
			{
				quality = 100;
			}
			else
			{
				quality = MathfEx.Clamp(asset.quality, 0, 100);
			}

			state = asset.getState(origin);
		}

		public Item(ushort newID, bool full, byte newQuality) : this(newID, full ? EItemOrigin.ADMIN : EItemOrigin.WORLD, newQuality)
		{ }

		public Item(ushort newID, EItemOrigin origin, byte newQuality)
		{
			_id = newID;
			quality = newQuality;

			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;

			if (asset == null)
			{
				state = new byte[0]; // All code assumes state is not null.
				return;
			}

			if (origin != EItemOrigin.WORLD)
			{
				amount = MathfEx.Max(asset.MaxAmountAsByte, 1);
			}
			else
			{
				amount = MathfEx.Max(asset.count, 1);
			}

			state = asset.getState(origin);
		}

		public Item(ushort newID, byte newAmount, byte newQuality)
		{
			_id = newID;
			amount = newAmount;
			quality = newQuality;

			ItemAsset asset = Assets.find(EAssetType.ITEM, id) as ItemAsset;

			if (asset == null)
			{
				state = new byte[0]; // All code assumes state is not null.
				return;
			}

			state = asset.getState();
		}

		public Item(ushort newID, byte newAmount, byte newQuality, byte[] newState)
		{
			_id = newID;

			amount = newAmount;
			quality = newQuality;
			state = newState != null ? newState : new byte[0]; // All code assumes state is not null.
		}

		public override string ToString()
		{
			return id + " " + amount + " " + quality + " " + state.Length;
		}

		/// <summary>
		/// If true, item has 100% quality. If false, item has a random quality.
		/// </summary>
		private static bool ShouldItemTypeSpawnAtFullQuality(EItemType type)
		{
			if (!Provider.modeConfigData.Items.Has_Durability)
			{
				return true;
			}

			switch (type)
			{
				case EItemType.HAT:
				case EItemType.PANTS:
				case EItemType.SHIRT:
				case EItemType.MASK:
				case EItemType.BACKPACK:
				case EItemType.VEST:
				case EItemType.GLASSES:
					return Provider.modeConfigData.Items.Clothing_Spawns_At_Full_Quality;

				case EItemType.FOOD:
					return Provider.modeConfigData.Items.Food_Spawns_At_Full_Quality;

				case EItemType.WATER:
					return Provider.modeConfigData.Items.Water_Spawns_At_Full_Quality;

				case EItemType.GUN:
				case EItemType.MELEE:
					return Provider.modeConfigData.Items.Weapons_Spawn_At_Full_Quality;

				default:
					return Provider.modeConfigData.Items.Default_Spawns_At_Full_Quality;
			}
		}
	}
}
