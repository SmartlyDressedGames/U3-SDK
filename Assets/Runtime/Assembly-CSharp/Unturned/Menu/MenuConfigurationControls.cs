////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	// This class doesn't seem to be used?
	public class MenuConfigurationControls : MonoBehaviour
	{
		private static byte _binding;
		public static byte binding
		{
			get => _binding;

			set => _binding = value;
		}

		private static void cancel()
		{
			MenuConfigurationControlsUI.cancel();
			binding = 255;
		}

		private static void bind(KeyCode key)
		{
			MenuConfigurationControlsUI.bind(key);
			binding = 255;
		}

		private void Update()
		{
			if (binding != 255)
			{
				if (Event.current.type == EventType.KeyDown)
				{
					if (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Escape)
					{
						cancel();
					}
					else
					{
						bind(Event.current.keyCode);
					}
				}
				else if (Event.current.type == EventType.MouseDown)
				{
					if (Event.current.button == 0)
					{
						bind(KeyCode.Mouse0);
					}
					else if (Event.current.button == 1)
					{
						bind(KeyCode.Mouse1);
					}
					else if (Event.current.button == 2)
					{
						bind(KeyCode.Mouse2);
					}
					else if (Event.current.button == 3)
					{
						bind(KeyCode.Mouse3);
					}
					else if (Event.current.button == 4)
					{
						bind(KeyCode.Mouse4);
					}
					else if (Event.current.button == 5)
					{
						bind(KeyCode.Mouse5);
					}
					else if (Event.current.button == 6)
					{
						bind(KeyCode.Mouse6);
					}
				}
				else if (Event.current.shift)
				{
					bind(KeyCode.LeftShift);
				}
			}
		}

		private void Awake()
		{
			binding = 255;
		}
	}
}
