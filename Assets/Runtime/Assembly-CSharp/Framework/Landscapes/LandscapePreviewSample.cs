////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Framework.Landscapes
{
	public struct LandscapePreviewSample
	{
		public Vector3 position;
		public float weight;

		public LandscapePreviewSample(Vector3 newPosition, float newWeight)
		{
			position = newPosition;
			weight = newWeight;
		}
	}
}
