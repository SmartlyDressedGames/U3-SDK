////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Unturned
{
	/// <summary>
	/// Almost every menu has a container element for its contents which spans the entire screen. This element is then
	/// animated into and out of view. In the IMGUI implementation this was fine because containers off-screen were not
	/// processed, but with uGUI they were still considered active. To solve the uGUI performance overhead this class
	/// was introduced to disable visibility after animating out of view.
	/// </summary>
	public class SleekFullscreenBox : SleekWrapper
	{
		public void AnimateIntoView()
		{
			ValidateNotDestroyed();
			IsVisible = true;
			AnimatePositionScale(0.0f, 0.0f, ESleekLerp.EXPONENTIAL, 20.0f);
		}

		public void AnimateOutOfView(float x, float y)
		{
			ValidateNotDestroyed();
			IsVisible = true;
			AnimatePositionScale(x, y, ESleekLerp.EXPONENTIAL, 20.0f);
		}

		public override void OnUpdate()
		{
			// OnUpdate is only called while isVisible, and isAnimatingTransform will be false once destination is reached.
			if (!IsAnimatingTransform)
			{
				// We compare actual values rather than a simple flag because it may be spawned off-screen.
				float x = PositionScale_X;
				float y = PositionScale_Y;
				if (x > 0.999f || y > 0.999f || x + 1.0f < 0.001f || y + 1.0f < 0.001f)
				{
					IsVisible = false;
				}
			}
		}
	}
}
