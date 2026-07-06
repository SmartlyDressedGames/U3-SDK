////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class LevelVisibility
	{
		public static readonly byte SAVEDATA_VERSION = 2;

		public static readonly byte OBJECT_REGIONS = 4;

		private static bool _roadsVisible;
		public static bool roadsVisible
		{
			get => _roadsVisible;

			set
			{
				_roadsVisible = value;

				LevelRoads.setEnabled(roadsVisible);
			}
		}

		private static bool _navigationVisible;
		public static bool navigationVisible
		{
			get => _navigationVisible;

			set
			{
				_navigationVisible = value;

				LevelNavigation.setEnabled(navigationVisible);
			}
		}

		public static bool nodesVisible
		{
			get => SDG.Framework.Devkit.SpawnpointSystemV2.Get().IsVisible;

			set => SDG.Framework.Devkit.SpawnpointSystemV2.Get().IsVisible = value;
		}

		private static bool _itemsVisible;
		public static bool itemsVisible
		{
			get => _itemsVisible;

			set
			{
				_itemsVisible = value;

				LevelItems.setEnabled(itemsVisible);
			}
		}

		private static bool _playersVisible;
		public static bool playersVisible
		{
			get => _playersVisible;

			set
			{
				_playersVisible = value;

				LevelPlayers.setEnabled(playersVisible);
			}
		}

		private static bool _zombiesVisible;
		public static bool zombiesVisible
		{
			get => _zombiesVisible;

			set
			{
				_zombiesVisible = value;

				LevelZombies.setEnabled(zombiesVisible);
			}
		}

		private static bool _vehiclesVisible;
		public static bool vehiclesVisible
		{
			get => _vehiclesVisible;

			set
			{
				_vehiclesVisible = value;

				LevelVehicles.setEnabled(vehiclesVisible);
			}
		}

		private static bool _borderVisible;
		public static bool borderVisible
		{
			get => _borderVisible;

			set
			{
				_borderVisible = value;

				Level.setEnabled(borderVisible);
			}
		}

		private static bool _animalsVisible;
		public static bool animalsVisible
		{
			get => _animalsVisible;

			set
			{
				_animalsVisible = value;

				LevelAnimals.setEnabled(animalsVisible);
			}
		}

		public static void load()
		{
			if (Level.isEditor)
			{
				if (ReadWrite.fileExists(Level.info.path + "/Level/Visibility.dat", false, false))
				{
					River river = new River(Level.info.path + "/Level/Visibility.dat", false);
					byte version = river.readByte();

					if (version > 0)
					{
						roadsVisible = river.readBoolean();
						navigationVisible = river.readBoolean();
						river.readBoolean(); // old nodesVisible
						itemsVisible = river.readBoolean();
						playersVisible = river.readBoolean();
						zombiesVisible = river.readBoolean();
						vehiclesVisible = river.readBoolean();
						borderVisible = river.readBoolean();

						if (version > 1)
						{
							animalsVisible = river.readBoolean();
						}
						else
						{
							_animalsVisible = true;
						}

						river.closeRiver();
					}
				}
				else
				{
					_roadsVisible = true;
					_navigationVisible = true;
					_itemsVisible = true;
					_playersVisible = true;
					_zombiesVisible = true;
					_vehiclesVisible = true;
					_borderVisible = true;
					_animalsVisible = true;
				}
			}
		}

		public static void save()
		{
			River river = new River(Level.info.path + "/Level/Visibility.dat", false);
			river.writeByte(SAVEDATA_VERSION);

			river.writeBoolean(roadsVisible);
			river.writeBoolean(navigationVisible);
			river.writeBoolean(nodesVisible);
			river.writeBoolean(itemsVisible);
			river.writeBoolean(playersVisible);
			river.writeBoolean(zombiesVisible);
			river.writeBoolean(vehiclesVisible);
			river.writeBoolean(borderVisible);
			river.writeBoolean(animalsVisible);

			river.closeRiver();
		}
	}
}
