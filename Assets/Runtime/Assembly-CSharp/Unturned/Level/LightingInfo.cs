////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class LightingInfo
	{
		private Color[] _colors;
		public Color[] colors => _colors;

		private float[] _singles;
		public float[] singles => _singles;

		public LightingInfo(Color[] newColors, float[] newSingles)
		{
			_colors = newColors;
			_singles = newSingles;
		}
	}
}
