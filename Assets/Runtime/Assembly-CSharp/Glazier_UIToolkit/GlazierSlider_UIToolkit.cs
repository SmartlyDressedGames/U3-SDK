////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine.UIElements;

namespace SDG.Unturned
{
	internal class GlazierSlider_UIToolkit : GlazierElementBase_UIToolkit, ISleekSlider
	{
		public event Dragged OnValueChanged;

		private ESleekOrientation _orientation = ESleekOrientation.VERTICAL;
		public ESleekOrientation Orientation
		{
			get
			{
				ValidateNotDestroyed();
				return _orientation;
			}

			set
			{
				ValidateNotDestroyed();
				if (_orientation != value)
				{
					_orientation = value;
					UpdateOrientation();
				}
			}
		}

		public float Value
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

		private SleekColor _backgroundColor = GlazierConst.DefaultSliderBackgroundColor;
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
				trackerElement.style.unityBackgroundImageTintColor = _backgroundColor;
			}
		}

		private SleekColor _foregroundColor = GlazierConst.DefaultSliderForegroundColor;
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
				draggerElement.style.unityBackgroundImageTintColor = _foregroundColor;
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

		public GlazierSlider_UIToolkit(Glazier_UIToolkit glazier) : base(glazier)
		{
			control = new Slider();
			control.userData = this;
			control.AddToClassList("unturned-slider");
			control.lowValue = 0.0f;
			control.highValue = 1.0f;
			control.RegisterValueChangedCallback(OnControlValueChanged);
			UpdateOrientation();

			VisualElement dragContainer = control.Q(className: "unity-base-slider__input").Q(className: "unity-base-slider__drag-container");
			trackerElement = dragContainer.Q(className: "unity-base-slider__tracker");
			draggerElement = dragContainer.Q(className: "unity-base-slider__dragger");

			visualElement = control;
		}

		internal override void SynchronizeColors()
		{
			trackerElement.style.unityBackgroundImageTintColor = _backgroundColor;
			draggerElement.style.unityBackgroundImageTintColor = _foregroundColor;
		}

		private void UpdateOrientation()
		{
			switch (_orientation)
			{
				case ESleekOrientation.HORIZONTAL:
					control.direction = SliderDirection.Horizontal;
					control.inverted = false;
					break;

				case ESleekOrientation.VERTICAL:
					control.direction = SliderDirection.Vertical;
					control.inverted = true; // Otherwise 0 is at the bottom and 1 is at the top.
					break;
			}
		}

		private void OnControlValueChanged(ChangeEvent<float> changeEvent)
		{
			OnValueChanged?.Invoke(this, changeEvent.newValue);
		}

		private Slider control;
		private VisualElement trackerElement;
		private VisualElement draggerElement;
	}
}
