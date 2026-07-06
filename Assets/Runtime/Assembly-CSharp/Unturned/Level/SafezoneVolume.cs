////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned
{
	public class SafezoneVolume : LevelVolume<SafezoneVolume, SafezoneVolumeManager>
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		/// <summary>
		/// If true, players inside the safezone cannot use items categorized as "weapons" (/hostile).
		/// </summary>
		public bool noWeapons = true;

		public bool noBuildables = true;

		/// <summary>
		/// If true, players inside the safezone cannot take damage. (Unless damage's bypassSafezone parameter is true.)
		/// For backwards compatibility this is true if noWeapons was true.
		/// </summary>
		public bool noIncomingDamage = true;

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			if (reader.containsKey("No_Weapons"))
				noWeapons = reader.readValue<bool>("No_Weapons");

			if (reader.containsKey("No_Buildables"))
				noBuildables = reader.readValue<bool>("No_Buildables");

			if (reader.containsKey("No_Incoming_Damage"))
			{
				noIncomingDamage = reader.readValue<bool>("No_Incoming_Damage");
			}
			else
			{
				noIncomingDamage = noWeapons;
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);

			writer.writeValue("No_Weapons", noWeapons);
			writer.writeValue("No_Buildables", noBuildables);
			writer.writeValue("No_Incoming_Damage", noIncomingDamage);
		}

		protected override void Start()
		{
			base.Start();
			backwardsCompatibilityNode = new SafezoneNode(transform.position, SafezoneNode.CalculateNormalizedRadiusFromRadius(GetSphereRadius()), false, noWeapons, noBuildables);
			backwardsCompatibilityNode.noIncomingDamage = noIncomingDamage;
		}

		internal SafezoneNode backwardsCompatibilityNode;

		private class Menu : SleekWrapper
		{
			public Menu(SafezoneVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;
				SizeOffset_Y = 140;

				ISleekToggle noWeaponsToggle = Glazier.Get().CreateToggle();
				noWeaponsToggle.SizeOffset_X = 40;
				noWeaponsToggle.SizeOffset_Y = 40;
				noWeaponsToggle.Value = volume.noWeapons;
				noWeaponsToggle.AddLabel("No Weapons", ESleekSide.RIGHT);
				noWeaponsToggle.OnValueChanged += OnWeaponsToggled;
				AddChild(noWeaponsToggle);

				ISleekToggle noBuildablesToggle = Glazier.Get().CreateToggle();
				noBuildablesToggle.PositionOffset_Y = 50;
				noBuildablesToggle.SizeOffset_X = 40;
				noBuildablesToggle.SizeOffset_Y = 40;
				noBuildablesToggle.Value = volume.noBuildables;
				noBuildablesToggle.AddLabel("No Buildables", ESleekSide.RIGHT);
				noBuildablesToggle.OnValueChanged += OnBuildablesToggled;
				AddChild(noBuildablesToggle);

				ISleekToggle noIncomingDamageToggle = Glazier.Get().CreateToggle();
				noIncomingDamageToggle.PositionOffset_Y = 100;
				noIncomingDamageToggle.SizeOffset_X = 40;
				noIncomingDamageToggle.SizeOffset_Y = 40;
				noIncomingDamageToggle.Value = volume.noIncomingDamage;
				noIncomingDamageToggle.AddLabel("No Incoming Damage", ESleekSide.RIGHT);
				noIncomingDamageToggle.OnValueChanged += OnIncomingDamageToggled;
				AddChild(noIncomingDamageToggle);
			}

			private void OnWeaponsToggled(ISleekToggle toggle, bool state)
			{
				volume.noWeapons = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnBuildablesToggled(ISleekToggle toggle, bool state)
			{
				volume.noBuildables = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnIncomingDamageToggled(ISleekToggle toggle, bool state)
			{
				volume.noIncomingDamage = state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private SafezoneVolume volume;
		}
	}
}
