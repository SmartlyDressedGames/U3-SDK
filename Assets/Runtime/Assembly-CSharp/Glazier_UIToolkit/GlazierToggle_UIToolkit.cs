////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierToggle_UIToolkit : GlazierElementBase_UIToolkit, ISleekToggle
	{
		public event Toggled OnValueChanged;

		public bool Value
		{
			get
			{
				ValidateNotDestroyed();
				return control.value;
			}

			set
			{
				ValidateNotDestroyed();
				control.SetValueWithoutNotify(value);
			}
		}

		public string TooltipText
		{
			get;
			set;
		}

		private SleekColor _backgroundColor = GlazierConst.DefaultToggleBackgroundColor;
		public SleekColor BackgroundColor
		{
			get
			{
				ValidateNotDestroyed();
				return _backgroundColor;
			}

			set
			{
				ValidateNotDestroyed();
				_backgroundColor = value;
				backgroundElement.style.unityBackgroundImageTintColor = _backgroundColor;
			}
		}

		private SleekColor _foregroundColor = GlazierConst.DefaultToggleForegroundColor;
		public SleekColor ForegroundColor
		{
			get
			{
				ValidateNotDestroyed();
				return _foregroundColor;
			}

			set
			{
				ValidateNotDestroyed();
				_foregroundColor = value;
				checkmarkElement.style.unityBackgroundImageTintColor = _foregroundColor;
			}
		}

		public bool IsInteractable
		{
			get
			{
				ValidateNotDestroyed();
				return control.enabledSelf;
			}

			set
			{
				ValidateNotDestroyed();
				control.SetEnabled(value);
			}
		}

		public GlazierToggle_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			SizeOffset_X = 40;
			SizeOffset_Y = 40;

			control = new Toggle();
			control.userData = this;
			control.AddToClassList("unturned-toggle");
			control.RegisterValueChangedCallback(OnControlValueChanged);
			backgroundElement = control.Q(className: "unity-toggle__input");
			checkmarkElement = control.Q(name: "unity-checkmark");

			visualElement = control;
		}

		internal override void SynchronizeColors()
		{
			backgroundElement.style.unityBackgroundImageTintColor = _backgroundColor;
			checkmarkElement.style.unityBackgroundImageTintColor = _foregroundColor;
		}

		internal override bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
		{
			tooltipText = this.TooltipText;
			tooltipColor = new SleekColor(ESleekTint.FONT);
			return true;
		}

		private void OnControlValueChanged(ChangeEvent<bool> changeEvent)
		{
			OnValueChanged?.Invoke(this, changeEvent.newValue);
		}

		private Toggle control;
		private VisualElement backgroundElement;
		private VisualElement checkmarkElement;
	}
}
