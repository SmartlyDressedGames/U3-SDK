////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Reflection;

namespace SDG.Unturned
{
	internal class SleekConfigProperty : SleekWrapper
	{
		public FieldInfo fieldInfo;
		public object defaultValue;

		public event System.Action<SleekConfigProperty, bool, object> OnValueChanged;

		public void SetOverrideState(bool isOverridden, object overrideValue)
		{
			overrideToggle.Value = isOverridden;

			object value = isOverridden ? overrideValue : defaultValue;
			System.Type valueType = fieldInfo.FieldType;
			if (valueType == typeof(uint))
			{
				ISleekUInt32Field uint32Field = (ISleekUInt32Field) valueWidget;
				uint32Field.Value = (uint) value;
				uint32Field.IsClickable = isOverridden;
			}
			else if (valueType == typeof(float))
			{
				ISleekFloat32Field singleField = (ISleekFloat32Field) valueWidget;
				singleField.Value = (float) value;
				singleField.IsClickable = isOverridden;
			}
			else if (valueType == typeof(bool))
			{
				ISleekToggle boolToggle = (ISleekToggle) valueWidget;
				boolToggle.Value = (bool) value;
				boolToggle.IsInteractable = isOverridden;
			}
		}

		public SleekConfigProperty(FieldInfo fieldInfo, string tooltip)
		{
			this.fieldInfo = fieldInfo;

			System.Type valueType = fieldInfo.FieldType;
			if (valueType == typeof(uint))
			{
				ISleekUInt32Field uint32Field = Glazier.Get().CreateUInt32Field();
				uint32Field.SizeOffset_X = 200;
				uint32Field.SizeOffset_Y = 30;
				uint32Field.AddLabel(MenuPlayConfigUI.sanitizeName(fieldInfo.Name), ESleekSide.RIGHT);
				uint32Field.OnValueChanged += OnTypedUInt32Value;
				uint32Field.TooltipText = tooltip;
				AddChild(uint32Field);
				valueWidget = uint32Field;

				SizeOffset_Y = 30;
			}
			else if (valueType == typeof(float))
			{
				ISleekFloat32Field singleField = Glazier.Get().CreateFloat32Field();
				singleField.SizeOffset_X = 200;
				singleField.SizeOffset_Y = 30;
				singleField.AddLabel(MenuPlayConfigUI.sanitizeName(fieldInfo.Name), ESleekSide.RIGHT);
				singleField.OnValueChanged += OnTypedSingleValue;
				singleField.TooltipText = tooltip;
				AddChild(singleField);
				valueWidget = singleField;

				SizeOffset_Y = 30;
			}
			else if (valueType == typeof(bool))
			{
				ISleekToggle toggle = Glazier.Get().CreateToggle();
				toggle.SizeOffset_X = 40;
				toggle.SizeOffset_Y = 40;
				toggle.AddLabel(MenuPlayConfigUI.sanitizeName(fieldInfo.Name), ESleekSide.RIGHT);
				toggle.OnValueChanged += OnToggledValue;
				toggle.TooltipText = tooltip;
				AddChild(toggle);
				valueWidget = toggle;

				SizeOffset_Y = 40;
			}
			else
			{
				throw new System.NotSupportedException(fieldInfo.ToString());
			}

			overrideToggle = Glazier.Get().CreateToggle();
			overrideToggle.PositionOffset_X = -40;
			overrideToggle.PositionOffset_Y = -20;
			overrideToggle.PositionScale_Y = 0.5f;
			overrideToggle.SizeOffset_X = 40;
			overrideToggle.SizeOffset_Y = 40;
			overrideToggle.OnValueChanged += OnOverrideToggled;
			overrideToggle.TooltipText = MenuPlayConfigUI.localization.format("Override_Tooltip");
			AddChild(overrideToggle);
		}

		private void OnTypedUInt32Value(ISleekUInt32Field uint32Field, uint state)
		{
			OnValueChanged?.Invoke(this, true, state);
		}

		private void OnTypedSingleValue(ISleekFloat32Field singleField, float state)
		{
			OnValueChanged?.Invoke(this, true, state);
		}

		private void OnToggledValue(ISleekToggle toggle, bool state)
		{
			OnValueChanged?.Invoke(this, true, state);
		}

		private void OnOverrideToggled(ISleekToggle toggle, bool state)
		{
			OnValueChanged?.Invoke(this, state, defaultValue);
			SetOverrideState(state, defaultValue);
		}

		private ISleekToggle overrideToggle;
		private ISleekElement valueWidget;
	}
}
