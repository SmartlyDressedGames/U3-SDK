////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public delegate void ClickedJar(SleekJars jars, int index);

	public class SleekJars : SleekWrapper
	{
		public ClickedJar onClickedJar;

		private void onClickedButton(SleekItem item)
		{
			int index = FindIndexOfChild(item);

			if (index != -1)
			{
				onClickedJar?.Invoke(this, index);
			}
		}

		public SleekJars(float radius, List<PlayerInventorySearchResultV2> search, float startAngle = 0.0f) : base()
		{
			float angleIncrement = Mathf.PI * 2f / search.Count;
			for (int index = 0; index < search.Count; index++)
			{
				ItemJar jar = search[index].Jar;
				ItemAsset asset = jar.GetAsset();

				if (asset != null)
				{
					SleekItem button = new SleekItem(jar);
					button.PositionOffset_X = (int) (Mathf.Cos(startAngle + (angleIncrement * index)) * radius) - (button.SizeOffset_X / 2);
					button.PositionOffset_Y = (int) (Mathf.Sin(startAngle + (angleIncrement * index)) * radius) - (button.SizeOffset_Y / 2);
					button.PositionScale_X = 0.5f;
					button.PositionScale_Y = 0.5f;
					button.onClickedItem = onClickedButton;
					button.onDraggedItem = onClickedButton;
					AddChild(button);
				}
			}
		}
	}
}
