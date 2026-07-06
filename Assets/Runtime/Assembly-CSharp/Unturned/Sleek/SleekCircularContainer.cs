////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Non-item replacement for SleekJars.
	/// Arranges children in an equally-spaced ring around the center.
	/// </summary>
	public class SleekCircularContainer : SleekWrapper
	{
		public float StartAngleRadians
		{
			get;
			set;
		}

		public float Radius
		{
			get;
			set;
		}

		public void UpdateLayout()
		{
			int childCount = GetChildCount();
			if (childCount < 1)
			{
				return;
			}

			float angleIncrement = Mathf.PI * 2f / childCount;
			for (int index = 0; index < childCount; ++index)
			{
				ISleekElement child = GetChildAtIndex(index);
				child.PositionOffset_X = (Mathf.Cos(StartAngleRadians + (angleIncrement * index)) * Radius) - (child.SizeOffset_X / 2);
				child.PositionOffset_Y = (Mathf.Sin(StartAngleRadians + (angleIncrement * index)) * Radius) - (child.SizeOffset_Y / 2);
				child.PositionScale_X = 0.5f;
				child.PositionScale_Y = 0.5f;
			}
		}

		public SleekCircularContainer(float radius, float startAngleRadians) : base()
		{
			StartAngleRadians = startAngleRadians;
			Radius = radius;
		}
	}
}
