////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned
{
	public class NPCOverlapVolume : LevelVolume<NPCOverlapVolume, NPCOverlapVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		[SerializeField]
		private string _id;
		public string id
		{
			get => _id;
			set
			{
				if (!string.Equals(_id, value))
				{
					GetVolumeManager().RemoveVolumeFromIdDictionary(this);
					_id = value;
					GetVolumeManager().AddVolumeToIdDictionary(this);
				}
			}
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);
			id = reader.readValue<string>("ID");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);
			writer.writeValue("ID", id);
		}

		private class Menu : SleekWrapper
		{
			public Menu(NPCOverlapVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 30;

				ISleekField idField = Glazier.Get().CreateStringField();
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 30;
				idField.Text = volume.id;
				idField.AddLabel("ID", ESleekSide.RIGHT);
				idField.OnTextChanged += OnNameChanged;
				AddChild(idField);
			}

			private void OnNameChanged(ISleekField field, string id)
			{
				volume.id = id;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private NPCOverlapVolume volume;
		}
	}
}
