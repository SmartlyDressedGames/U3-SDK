////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Scrolling : MonoBehaviour
	{
		public Material material;

		public float x;
		public float y;

		private void Update()
		{
			material.mainTextureOffset = new Vector2(x * Time.time, y * Time.time);
		}
	}
}