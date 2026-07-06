////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class GrassDisplacement : MonoBehaviour
	{
		private int _Grass_Displacement_Point = -1;

		private void Update()
		{
			Shader.SetGlobalVector(_Grass_Displacement_Point, new Vector4(transform.position.x, transform.position.y + 0.5f, transform.position.z, 0.0f));
		}

		private void OnEnable()
		{
			if (_Grass_Displacement_Point == -1)
			{
				_Grass_Displacement_Point = Shader.PropertyToID("_Grass_Displacement_Point");
			}
		}
	}
}