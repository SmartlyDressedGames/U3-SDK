////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void SwappedState(SleekButtonState button, int index);

	public class SleekButtonState : SleekWrapper
	{
		public GUIContent[] states
		{
			get;
			private set;
		}

		private int _state;
		public int state
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
				synchronizeActiveContent();
			}
		}

		public string tooltip
		{
			get
			{
				ValidateNotDestroyed();
				return button.tooltip;
			}
			set
			{
				ValidateNotDestroyed();
				button.tooltip = value;
			}
		}

		public bool isInteractable
		{
			get
			{
				ValidateNotDestroyed();
				return button.isClickable;
			}
			set
			{
				ValidateNotDestroyed();
				button.isClickable = value;
			}
		}

		private bool _useContentTooltip;
		/// <summary>
		/// If true, button tooltip will be overridden with tooltip from states array.
		/// </summary>
		public bool UseContentTooltip
		{
			get => _useContentTooltip;
			set
			{
				ValidateNotDestroyed();
				_useContentTooltip = value;
				if (_useContentTooltip)
				{
					if (states != null && state >= 0 && state < states.Length && states[state] != null)
					{
						button.tooltip = states[state].tooltip;
					}
					else
					{
						button.tooltip = string.Empty;
					}
				}
			}
		}

		public SwappedState onSwappedState;

		private void synchronizeActiveContent()
		{
			if (states != null && state >= 0 && state < states.Length && states[state] != null)
			{
				button.text = states[state].text;
				button.icon = states[state].image as Texture2D;

				if (_useContentTooltip)
				{
					button.tooltip = states[state].tooltip;
				}
			}
			else
			{
				button.text = string.Empty;
				button.icon = null;

				if (_useContentTooltip)
				{
					button.tooltip = string.Empty;
				}
			}
		}

		protected virtual void onClickedState(ISleekElement button)
		{
			_state++;
			if (state >= states.Length)
			{
				_state = 0;
			}

			synchronizeActiveContent();
			onSwappedState?.Invoke(this, state);
		}

		protected virtual void onRightClickedState(ISleekElement button)
		{
			_state--;
			if (state < 0)
			{
				_state = states.Length - 1;
			}

			synchronizeActiveContent();
			onSwappedState?.Invoke(this, state);
		}

		public void setContent(params GUIContent[] newStates)
		{
			ValidateNotDestroyed();
			states = newStates;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			for (int index = 0; index < states.Length; ++index)
			{
				if (states[index] == null)
				{
					throw new System.ArgumentNullException($"{nameof(newStates)}[{index}");
				}
			}
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

			if (state >= states.Length)
			{
				_state = 0;
			}

			synchronizeActiveContent();
		}

		public SleekButtonState(params GUIContent[] newStates) : this(0, newStates)
		{}

		public SleekButtonState(int iconSize, params GUIContent[] newStates) : base()
		{
			_state = 0;

			button = new SleekButtonIcon(null, iconSize);
			button.SizeScale_X = 1.0f;
			button.SizeScale_Y = 1.0f;
			AddChild(button);

			if (newStates != null)
			{
				setContent(newStates);
			}

			button.onClickedButton += onClickedState;
			button.onRightClickedButton += onRightClickedState;
		}

		internal SleekButtonIcon button;
	}
}
