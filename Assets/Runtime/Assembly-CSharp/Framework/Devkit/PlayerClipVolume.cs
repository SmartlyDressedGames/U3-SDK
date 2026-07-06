////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class PlayerClipVolume : LevelVolume<PlayerClipVolume, PlayerClipVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		[SerializeField]
		protected bool _blockPlayer = true;
		public bool blockPlayer
		{
			get => _blockPlayer;
			set
			{
				_blockPlayer = value;
				if (!Level.isEditor)
				{
					volumeCollider.enabled = value;
				}
			}
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			if (reader.containsKey("Block_Player"))
			{
				blockPlayer = reader.readValue<bool>("Block_Player");
			}
			else
			{
				blockPlayer = true;
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("Block_Player", blockPlayer);
		}

		protected override void Awake()
		{
			forceShouldAddCollider = true; // Needed in gameplay for player collisions.
			base.Awake();
			volumeCollider.isTrigger = false;
			gameObject.layer = LayerMasks.CLIP;
		}

		private class Menu : SleekWrapper
		{
			public Menu(PlayerClipVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 40;

				ISleekToggle blockPlayerToggle = Glazier.Get().CreateToggle();
				blockPlayerToggle.SizeOffset_X = 40;
				blockPlayerToggle.SizeOffset_Y = 40;
				blockPlayerToggle.Value = volume.blockPlayer;
				blockPlayerToggle.AddLabel("Block Player", ESleekSide.RIGHT);
				blockPlayerToggle.OnValueChanged += OnBlockPlayerToggled;
				AddChild(blockPlayerToggle);
			}

			private void OnBlockPlayerToggled(ISleekToggle toggle, bool state)
			{
				volume.blockPlayer = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private PlayerClipVolume volume;
		}
	}
}
