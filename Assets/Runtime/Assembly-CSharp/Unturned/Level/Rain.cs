////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Water;
using UnityEngine;

namespace SDG.Unturned
{
	public class Rain : MonoBehaviour
	{
		private int _Rain_Puddle_Map = -1;
		private int _Rain_Ripple_Map = -1;
		private int _Rain_Water_Level = -1;
		private int _Rain_Intensity = -1;
		private int _Rain_Min_Height = -1;

		public Texture2D Puddle_Map;
		public Texture2D Ripple_Map;
		public float Water_Level;
		public float Intensity;

		private bool isRaining; // IS_RAINING
		private bool needsIsRainingUpdate;

#if !DEDICATED_SERVER
		private void onGraphicsSettingsApplied()
		{
			needsIsRainingUpdate = true;
		}

		private void Update()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			if (_Rain_Water_Level != -1)
			{
				Shader.SetGlobalFloat(_Rain_Water_Level, Water_Level);
			}

			if (_Rain_Intensity != -1)
			{
				Shader.SetGlobalFloat(_Rain_Intensity, Intensity);
			}

			if (_Rain_Min_Height != -1)
			{
				Shader.SetGlobalFloat(_Rain_Min_Height, WaterVolumeManager.worldSeaLevel);
			}

			if (Water_Level > 0.01f)
			{
				if (!isRaining)
				{
					isRaining = true;
					needsIsRainingUpdate = true;
				}
			}
			else
			{
				if (isRaining)
				{
					isRaining = false;
					needsIsRainingUpdate = true;
				}
			}

			if (needsIsRainingUpdate)
			{
				if (isRaining && GraphicsSettings.puddle)
				{
					Shader.EnableKeyword("IS_RAINING");
				}
				else
				{
					Shader.DisableKeyword("IS_RAINING");
				}

				needsIsRainingUpdate = false;
			}
		}

		private void OnEnable()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			GraphicsSettings.graphicsSettingsApplied += onGraphicsSettingsApplied;

			if (_Rain_Puddle_Map == -1)
			{
				_Rain_Puddle_Map = Shader.PropertyToID("_Rain_Puddle_Map");
				Shader.SetGlobalTexture(_Rain_Puddle_Map, Puddle_Map);
			}

			if (_Rain_Ripple_Map == -1)
			{
				_Rain_Ripple_Map = Shader.PropertyToID("_Rain_Ripple_Map");
				Shader.SetGlobalTexture(_Rain_Ripple_Map, Ripple_Map);
			}

			if (_Rain_Water_Level == -1)
			{
				_Rain_Water_Level = Shader.PropertyToID("_Rain_Water_Level");
			}

			if (_Rain_Intensity == -1)
			{
				_Rain_Intensity = Shader.PropertyToID("_Rain_Intensity");
			}

			if (_Rain_Min_Height == -1)
			{
				_Rain_Min_Height = Shader.PropertyToID("_Rain_Min_Height");
			}

			isRaining = false;
			needsIsRainingUpdate = true;
		}

		private void OnDisable()
		{
			if (Dedicator.IsDedicatedServer)
			{
				return;
			}

			GraphicsSettings.graphicsSettingsApplied -= onGraphicsSettingsApplied;
		}
#endif // !DEDICATED_SERVER
	}
}
