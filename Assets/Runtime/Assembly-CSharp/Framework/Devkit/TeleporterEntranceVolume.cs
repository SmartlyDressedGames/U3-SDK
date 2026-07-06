////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class TeleporterEntranceVolume : LevelVolume<TeleporterEntranceVolume, TeleporterEntranceVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		[SerializeField]
		public string pairId;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);
			pairId = reader.readValue<string>("Pair_Id");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);
			writer.writeValue("Pair_Id", pairId);
		}

		public void OnTriggerEnter(Collider other)
		{
			TeleporterExitVolume destination = TeleporterExitVolumeManager.Get().FindExitVolume(pairId);
			if (destination != null)
			{
				PlayerMovement movement = other.gameObject.GetComponent<PlayerMovement>();
				if (movement != null && movement.CanEnterTeleporter)
				{
					movement.EnterTeleporterVolume(this, destination);
				}
			}
		}

		protected override void Awake()
		{
			forceShouldAddCollider = true; // Client movement also needs to teleport.
			base.Awake();
		}

		private class Menu : SleekWrapper
		{
			public Menu(TeleporterEntranceVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 30;

				ISleekField idField = Glazier.Get().CreateStringField();
				idField.SizeOffset_X = 200;
				idField.SizeOffset_Y = 30;
				idField.Text = volume.pairId;
				idField.AddLabel("Pair ID", ESleekSide.RIGHT);
				idField.OnTextChanged += OnIdChanged;
				AddChild(idField);
			}

			private void OnIdChanged(ISleekField field, string id)
			{
				volume.pairId = id;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private TeleporterEntranceVolume volume;
		}
	}
}
