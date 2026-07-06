////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned
{
	public class OxygenVolume : LevelVolume<OxygenVolume, OxygenVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		[SerializeField]
		private bool _isBreathable = true;
		/// <summary>
		/// If true oxygen is restored while in this volume, otherwise if false oxygen is depleted.
		/// </summary>
		public bool isBreathable
		{
			get => _isBreathable;
			set
			{
				if (!enabled)
				{
					_isBreathable = value;
					return;
				}

				GetVolumeManager().RemoveVolume(this);
				_isBreathable = value;
				GetVolumeManager().AddVolume(this);
			}
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			isBreathable = reader.readValue<bool>("Is_Breathable");
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Is_Breathable", isBreathable);
		}

		protected override void Awake()
		{
			supportsFalloff = true;
			base.Awake();
		}

		private class Menu : SleekWrapper
		{
			public Menu(OxygenVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 40;

				ISleekToggle hasOxygenToggle = Glazier.Get().CreateToggle();
				hasOxygenToggle.SizeOffset_X = 40;
				hasOxygenToggle.SizeOffset_Y = 40;
				hasOxygenToggle.Value = volume.isBreathable;
				hasOxygenToggle.AddLabel("Is Breathable", ESleekSide.RIGHT);
				hasOxygenToggle.OnValueChanged += OnHasOxygenToggled;
				AddChild(hasOxygenToggle);
			}

			private void OnHasOxygenToggled(ISleekToggle toggle, bool state)
			{
				volume.isBreathable = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private OxygenVolume volume;
		}
	}
}
