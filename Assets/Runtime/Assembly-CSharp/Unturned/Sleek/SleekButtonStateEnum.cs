////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class SleekButtonStateEnum<T> : SleekButtonState where T : struct, System.Enum
	{
		public T GetEnum()
		{
			return (T) System.Enum.ToObject(typeof(T), state);
		}

		public void SetEnum(T value)
		{
			state = System.Convert.ToInt32(value);
		}

		public System.Action<SleekButtonStateEnum<T>, T> OnSwappedEnum;

		protected override void onClickedState(ISleekElement button)
		{
			base.onClickedState(button);
			OnSwappedEnum.Invoke(this, GetEnum());
		}

		protected override void onRightClickedState(ISleekElement button)
		{
			base.onRightClickedState(button);
			OnSwappedEnum.Invoke(this, GetEnum());
		}

		public SleekButtonStateEnum() : base()
		{
			string[] names = System.Enum.GetNames(typeof(T));
			GUIContent[] newStates = new GUIContent[names.Length];
			for (int index = 0; index < names.Length; ++index)
			{
				string codeName = names[index];
				bool isAllUppercase = true;
				foreach (char c in codeName)
				{
					if (char.IsLower(c))
					{
						isAllUppercase = false;
					}
				}

				nameSb.Clear();
				if (isAllUppercase)
				{
					nameSb.Append(codeName[0]);
					for (int letterIndex = 1; letterIndex < codeName.Length; ++letterIndex)
					{
						nameSb.Append(char.ToLower(codeName[letterIndex]));
					}
				}
				else
				{
					for (int letterIndex = 0; letterIndex < codeName.Length; ++letterIndex)
					{
						char letter = codeName[letterIndex];
						if (letterIndex > 0 && char.IsUpper(letter) && !char.IsUpper(codeName[letterIndex - 1]))
						{
							nameSb.Append(' ');
						}
						nameSb.Append(letter);
					}
				}

				newStates[index] = new GUIContent(nameSb.ToString());
			}
			setContent(newStates);
		}

		private static System.Text.StringBuilder nameSb = new System.Text.StringBuilder(32);
	}
}
