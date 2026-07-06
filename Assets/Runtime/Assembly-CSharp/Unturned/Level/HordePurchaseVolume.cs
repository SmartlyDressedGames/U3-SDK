////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned
{
	public class HordePurchaseVolume : LevelVolume<HordePurchaseVolume, HordePurchaseVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		public ushort id;
		public uint cost;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			if (reader.containsKey("Item_ID"))
				id = reader.readValue<ushort>("Item_ID");

			if (reader.containsKey("Cost"))
				cost = reader.readValue<uint>("Cost");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Item_ID", id);
			writer.writeValue("Cost", cost);
		}

		private class Menu : SleekWrapper
		{
			public Menu(HordePurchaseVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 70;

				ISleekUInt16Field idField = Glazier.Get().CreateUInt16Field();
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 30;
				idField.Value = volume.id;
				idField.AddLabel("Item ID", ESleekSide.RIGHT);
				idField.OnValueChanged += OnIdChanged;
				AddChild(idField);

				ISleekUInt32Field costField = Glazier.Get().CreateUInt32Field();
				costField.PositionOffset_Y = 40;
				costField.SizeOffset_X = 200;
				costField.SizeOffset_Y = 30;
				costField.Value = volume.cost;
				costField.AddLabel("Cost", ESleekSide.RIGHT);
				costField.OnValueChanged += OnCostChanged;
				AddChild(costField);
			}

			private void OnIdChanged(ISleekUInt16Field field, ushort state)
			{
				volume.id = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnCostChanged(ISleekUInt32Field field, uint state)
			{
				volume.cost = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private HordePurchaseVolume volume;
		}
	}
}
