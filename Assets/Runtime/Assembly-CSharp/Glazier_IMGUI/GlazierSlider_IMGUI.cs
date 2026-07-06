////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal class GlazierSlider_IMGUI : GlazierElementBase_IMGUI, ISleekSlider
	{
		public event Dragged OnValueChanged;

		public ESleekOrientation Orientation
		{
			get;
			set;
		} = ESleekOrientation.VERTICAL;

		private const float NormalizedHandleSize = 0.25f;

		private float scroll;

		private float _state;
		public float Value
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
				scroll = Value * (1 - NormalizedHandleSize);
			}
		}

		public SleekColor BackgroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultSliderBackgroundColor;

		public SleekColor ForegroundColor
		{
			get;
			set;
		} = GlazierConst.DefaultSliderForegroundColor;

		public bool IsInteractable
		{
			get;
			set;
		} = true;

		public override void OnGUI()
		{
			bool wasGUIenabled = GUI.enabled;
			GUI.enabled = IsInteractable;
			float value = GlazierUtils_IMGUI.drawSlider(drawRect, Orientation, scroll, NormalizedHandleSize, BackgroundColor);
			GUI.enabled = wasGUIenabled;

			if (value != scroll)
			{
				_state = value / (1.0f - NormalizedHandleSize);

				if (Value < 0.0f)
				{
					Value = 0.0f;
				}
				else if (Value > 1.0f)
				{
					Value = 1.0f;
				}

				OnValueChanged?.Invoke(this, Value);
			}

			scroll = value;

			ChildrenOnGUI();
		}
	}
}
