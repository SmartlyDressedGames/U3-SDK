////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class Snow : MonoBehaviour
	{
		private int _Snow_Sparkle_Map = -1;

		public Texture2D Sparkle_Map;

#if !DEDICATED_SERVER
		private void OnEnable()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (_Snow_Sparkle_Map == -1)
			{
				_Snow_Sparkle_Map = Shader.PropertyToID("_Snow_Sparkle_Map");
				Shader.SetGlobalTexture(_Snow_Sparkle_Map, Sparkle_Map);
			}
		}
#endif // !DEDICATED_SERVER
	}
}
