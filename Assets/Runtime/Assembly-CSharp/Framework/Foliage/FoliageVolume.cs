////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public class FoliageVolume : LevelVolume<FoliageVolume, FoliageVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		public enum EFoliageVolumeMode
		{
			ADDITIVE,
			SUBTRACTIVE
		}

		[SerializeField]
		protected EFoliageVolumeMode _mode = EFoliageVolumeMode.SUBTRACTIVE;
		public EFoliageVolumeMode mode
		{
			get => _mode;
			set
			{
				if (!enabled)
				{
					_mode = value;
					return;
				}

				GetVolumeManager().RemoveVolume(this);
				_mode = value;
				GetVolumeManager().AddVolume(this);
			}
		}

		public bool instancedMeshes = true;
		public bool resources = true;
		public bool objects = true;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			mode = reader.readValue<EFoliageVolumeMode>("Mode");

			if (reader.containsKey("Instanced_Meshes"))
				instancedMeshes = reader.readValue<bool>("Instanced_Meshes");

			if (reader.containsKey("Resources"))
				resources = reader.readValue<bool>("Resources");

			if (reader.containsKey("Objects"))
				objects = reader.readValue<bool>("Objects");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Mode", mode);
			writer.writeValue("Instanced_Meshes", instancedMeshes);
			writer.writeValue("Resources", resources);
			writer.writeValue("Objects", objects);
		}

		private class Menu : SleekWrapper
		{
			public Menu(FoliageVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 160;

				SleekButtonState modeButton = new SleekButtonState(new GUIContent("Additive"), new GUIContent("Subtractive"));
				modeButton.SizeOffset_X = 200;
				modeButton.SizeOffset_Y = 30;
				modeButton.AddLabel("Mode", ESleekSide.RIGHT);
				modeButton.state = volume.mode == EFoliageVolumeMode.ADDITIVE ? 0 : 1;
				modeButton.onSwappedState += OnSwappedMode;
				AddChild(modeButton);

				ISleekToggle instancedMeshesToggle = Glazier.Get().CreateToggle();
				instancedMeshesToggle.PositionOffset_Y = 40;
				instancedMeshesToggle.SizeOffset_X = 40;
				instancedMeshesToggle.SizeOffset_Y = 40;
				instancedMeshesToggle.Value = volume.instancedMeshes;
				instancedMeshesToggle.AddLabel("Instanced Meshes", ESleekSide.RIGHT);
				instancedMeshesToggle.OnValueChanged += OnInstancedMeshesToggled;
				AddChild(instancedMeshesToggle);

				ISleekToggle resourcesToggle = Glazier.Get().CreateToggle();
				resourcesToggle.PositionOffset_Y = 80;
				resourcesToggle.SizeOffset_X = 40;
				resourcesToggle.SizeOffset_Y = 40;
				resourcesToggle.Value = volume.resources;
				resourcesToggle.AddLabel("Resources", ESleekSide.RIGHT);
				resourcesToggle.OnValueChanged += OnResourcesToggled;
				AddChild(resourcesToggle);

				ISleekToggle objectsToggle = Glazier.Get().CreateToggle();
				objectsToggle.PositionOffset_Y = 120;
				objectsToggle.SizeOffset_X = 40;
				objectsToggle.SizeOffset_Y = 40;
				objectsToggle.Value = volume.objects;
				objectsToggle.AddLabel("Objects", ESleekSide.RIGHT);
				objectsToggle.OnValueChanged += OnObjectsToggled;
				AddChild(objectsToggle);
			}

			private void OnSwappedMode(SleekButtonState button, int state)
			{
				volume.mode = state == 0 ? EFoliageVolumeMode.ADDITIVE : EFoliageVolumeMode.SUBTRACTIVE;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnInstancedMeshesToggled(ISleekToggle toggle, bool state)
			{
				volume.instancedMeshes = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnResourcesToggled(ISleekToggle toggle, bool state)
			{
				volume.resources = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnObjectsToggled(ISleekToggle toggle, bool state)
			{
				volume.objects = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private FoliageVolume volume;
		}
	}
}
