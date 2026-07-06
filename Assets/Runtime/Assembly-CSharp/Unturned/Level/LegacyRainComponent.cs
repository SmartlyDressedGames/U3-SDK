////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	public class LegacyRainComponent : CustomWeatherComponent
	{
		public override void OnBeginTransitionIn()
		{
			SetRain(ELightingRain.PRE_DRIZZLE);
		}

		public override void OnEndTransitionIn()
		{
			SetRain(ELightingRain.DRIZZLE);
		}

		public override void OnBeginTransitionOut()
		{
			SetRain(ELightingRain.POST_DRIZZLE);
		}

		public override void OnEndTransitionOut()
		{
			SetRain(ELightingRain.NONE);
		}

		private void SetRain(ELightingRain rain)
		{
			LevelLighting.rainyness = rain;
			LightingManager.broadcastRainUpdated(rain);
		}

		private void OnEnable()
		{
			puddleWaterLevel = 0.75f;
			puddleIntensity = 2.0f;
		}
	}
}
