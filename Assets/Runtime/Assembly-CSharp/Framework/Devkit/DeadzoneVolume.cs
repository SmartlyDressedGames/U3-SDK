////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////

using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit
{
	public class DeadzoneVolume : LevelVolume<DeadzoneVolume, DeadzoneVolumeManager>, IDeadzoneNode
	{
		public override ISleekElement CreateMenu()
		{
			ISleekElement menu = new Menu(this);
			AppendBaseMenu(menu);
			return menu;
		}

		[SerializeField]
		private EDeadzoneType _deadzoneType;
		public EDeadzoneType DeadzoneType
		{
			get => _deadzoneType;
			set => _deadzoneType = value;
		}

		[SerializeField]
		private float _unprotectedDamagePerSecond;
		public float UnprotectedDamagePerSecond
		{
			get => _unprotectedDamagePerSecond;
			set => _unprotectedDamagePerSecond = value;
		}

		[SerializeField]
		private float _protectedDamagePerSecond;
		public float ProtectedDamagePerSecond
		{
			get => _protectedDamagePerSecond;
			set => _protectedDamagePerSecond = value;
		}

		[SerializeField]
		private float _unprotectedRadiationPerSecond = 6.25f;
		public float UnprotectedRadiationPerSecond
		{
			get => _unprotectedRadiationPerSecond;
			set => _unprotectedRadiationPerSecond = value;
		}

		[SerializeField]
		private float _maskFilterDamagePerSecond = 0.4f;
		public float MaskFilterDamagePerSecond
		{
			get => _maskFilterDamagePerSecond;
			set => _maskFilterDamagePerSecond = value;
		}

		protected override void readHierarchyItem(IFormattedFileReader reader)
		{
			base.readHierarchyItem(reader);

			if (reader.containsKey("Deadzone_Type"))
			{
				_deadzoneType = reader.readValue<EDeadzoneType>("Deadzone_Type");
			}
			else
			{
				_deadzoneType = EDeadzoneType.DefaultRadiation;
			}

			_unprotectedDamagePerSecond = reader.readValue<float>("UnprotectedDamagePerSecond");
			_protectedDamagePerSecond = reader.readValue<float>("ProtectedDamagePerSecond");
			if (reader.containsKey("UnprotectedRadiationPerSecond"))
			{
				_unprotectedRadiationPerSecond = reader.readValue<float>("UnprotectedRadiationPerSecond");
			}
			else
			{
				_unprotectedRadiationPerSecond = 6.25f;
			}
			if (reader.containsKey("MaskFilterDamagePerSecond"))
			{
				_maskFilterDamagePerSecond = reader.readValue<float>("MaskFilterDamagePerSecond");
			}
			else
			{
				_maskFilterDamagePerSecond = 0.4f;
			}
		}

		protected override void writeHierarchyItem(IFormattedFileWriter writer)
		{
			base.writeHierarchyItem(writer);
			writer.writeValue("Deadzone_Type", _deadzoneType);
			writer.writeValue("UnprotectedDamagePerSecond", _unprotectedDamagePerSecond);
			writer.writeValue("ProtectedDamagePerSecond", _protectedDamagePerSecond);
			writer.writeValue("UnprotectedRadiationPerSecond", _unprotectedRadiationPerSecond);
			writer.writeValue("MaskFilterDamagePerSecond", _maskFilterDamagePerSecond);
		}

		private class Menu : SleekWrapper
		{
			public Menu(DeadzoneVolume volume)
			{
				this.volume = volume;

				SizeOffset_X = 400;

				float offset = 0;

				SleekButtonState deadzoneTypeButton = new SleekButtonState(new GUIContent("Default Radiation"), new GUIContent("Full Suit Radiation"));
				deadzoneTypeButton.PositionOffset_Y = offset;
				deadzoneTypeButton.SizeOffset_X = 200;
				deadzoneTypeButton.SizeOffset_Y = 30;
				deadzoneTypeButton.state = (int) volume.DeadzoneType;
				deadzoneTypeButton.AddLabel("Deadzone Type", ESleekSide.RIGHT);
				deadzoneTypeButton.onSwappedState += OnSwappedState;
				AddChild(deadzoneTypeButton);
				offset += deadzoneTypeButton.SizeOffset_Y + 10.0f;

				ISleekFloat32Field unprotectedDamageField = Glazier.Get().CreateFloat32Field();
				unprotectedDamageField.PositionOffset_Y = offset;
				unprotectedDamageField.SizeOffset_X = 200;
				unprotectedDamageField.SizeOffset_Y = 30;
				unprotectedDamageField.Value = volume.UnprotectedDamagePerSecond;
				unprotectedDamageField.AddLabel("Damage per Second (Unprotected)", ESleekSide.RIGHT);
				unprotectedDamageField.OnValueChanged += OnUnprotectedDamageChanged;
				AddChild(unprotectedDamageField);
				offset += unprotectedDamageField.SizeOffset_Y + 10;

				ISleekFloat32Field protectedDamageField = Glazier.Get().CreateFloat32Field();
				protectedDamageField.PositionOffset_Y = offset;
				protectedDamageField.SizeOffset_X = 200;
				protectedDamageField.SizeOffset_Y = 30;
				protectedDamageField.Value = volume.ProtectedDamagePerSecond;
				protectedDamageField.AddLabel("Damage per Second (Protected)", ESleekSide.RIGHT);
				protectedDamageField.OnValueChanged += OnProtectedDamageChanged;
				AddChild(protectedDamageField);
				offset += protectedDamageField.SizeOffset_Y + 10;

				ISleekFloat32Field unprotectedRadiationField = Glazier.Get().CreateFloat32Field();
				unprotectedRadiationField.PositionOffset_Y = offset;
				unprotectedRadiationField.SizeOffset_X = 200;
				unprotectedRadiationField.SizeOffset_Y = 30;
				unprotectedRadiationField.Value = volume.UnprotectedRadiationPerSecond;
				unprotectedRadiationField.AddLabel("Radiation per Second", ESleekSide.RIGHT);
				unprotectedRadiationField.OnValueChanged += OnUnprotectedRadiationChanged;
				AddChild(unprotectedRadiationField);
				offset += unprotectedRadiationField.SizeOffset_Y + 10;

				ISleekFloat32Field maskFilterDamageField = Glazier.Get().CreateFloat32Field();
				maskFilterDamageField.PositionOffset_Y = offset;
				maskFilterDamageField.SizeOffset_X = 200;
				maskFilterDamageField.SizeOffset_Y = 30;
				maskFilterDamageField.Value = volume.MaskFilterDamagePerSecond;
				maskFilterDamageField.AddLabel("Mask Filter Degradation per Second", ESleekSide.RIGHT);
				maskFilterDamageField.OnValueChanged += OnMaskFilterDamageChanged;
				AddChild(maskFilterDamageField);
				offset += maskFilterDamageField.SizeOffset_Y + 10;

				SizeOffset_Y = offset - 10;
			}

			private void OnSwappedState(SleekButtonState button, int state)
			{
				volume.DeadzoneType = (EDeadzoneType) state;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnUnprotectedDamageChanged(ISleekFloat32Field field, float value)
			{
				volume.UnprotectedDamagePerSecond = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnProtectedDamageChanged(ISleekFloat32Field field, float value)
			{
				volume.ProtectedDamagePerSecond = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnUnprotectedRadiationChanged(ISleekFloat32Field field, float value)
			{
				volume.UnprotectedRadiationPerSecond = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private void OnMaskFilterDamageChanged(ISleekFloat32Field field, float value)
			{
				volume.MaskFilterDamagePerSecond = value;
				SDG.Framework.Devkit.LevelHierarchy.MarkDirty();
			}

			private DeadzoneVolume volume;
		}
	}
}
