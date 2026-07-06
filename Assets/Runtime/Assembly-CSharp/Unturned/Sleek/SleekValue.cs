////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public delegate void Valued(SleekValue value, float state);

	public class SleekValue : SleekWrapper
	{
		public Valued onValued;

		private float _state;
		public float state
		{
			get
			{
				ValidateNotDestroyed();
				return _state;
			}

			set
			{
				ValidateNotDestroyed();
				_state = value;

				field.Value = state;
				slider.Value = state;
			}
		}

		private void onTypedSingleField(ISleekFloat32Field field, float state)
		{
			onValued?.Invoke(this, state);

			_state = state;
			slider.Value = state;
		}

		private void onDraggedSlider(ISleekSlider slider, float state)
		{
			onValued?.Invoke(this, state);

			_state = state;
			field.Value = state;
		}

		public SleekValue() : base()
		{
			field = Glazier.Get().CreateFloat32Field();
			field.SizeOffset_X = -5;
			field.SizeScale_X = 0.4f;
			field.SizeScale_Y = 1;
			field.OnValueChanged += onTypedSingleField;
			AddChild(field);

			slider = Glazier.Get().CreateSlider();
			slider.PositionOffset_X = 5;
			slider.PositionOffset_Y = -10;
			slider.PositionScale_X = 0.4f;
			slider.PositionScale_Y = 0.5f;
			slider.SizeOffset_X = -5;
			slider.SizeOffset_Y = 20;
			slider.SizeScale_X = 0.6f;
			slider.Orientation = ESleekOrientation.HORIZONTAL;
			slider.OnValueChanged += onDraggedSlider;
			AddChild(slider);
		}

		private ISleekFloat32Field field;
		private ISleekSlider slider;
	}
}
