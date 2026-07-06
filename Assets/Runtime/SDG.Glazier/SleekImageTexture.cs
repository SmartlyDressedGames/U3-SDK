////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public interface ISleekImage : ISleekElement
	{
		Texture Texture
		{
			get;
			set;
		}

		/// <summary>
		/// Rotation around the center of the image.
		/// Positive values are clockwise, negative values are counter-clockwise.
		/// Zero orients +X right, 90 orients +X down, -90 orients +X up.
		/// </summary>
		float RotationAngle
		{
			get;
			set;
		}

		/// <summary>
		/// If true, <seealso cref="RotationAngle"/> is applied. Otherwise, angle is ignored.
		/// </summary>
		bool CanRotate
		{
			get;
			set;
		}

		bool ShouldDestroyTexture
		{
			get;
			set;
		}

		SleekColor TintColor
		{
			get;
			set;
		}

		/// <summary>
		/// Sort of hacked together. Unturned IMGUI had a lot of places that checked whether the mouse position was over
		/// an image during click, so this controls whether to add an event handler.
		/// </summary>
		event System.Action OnClicked;

		event System.Action OnRightClicked;

		void UpdateTexture(Texture2D newTexture);
		void SetTextureAndShouldDestroy(Texture2D texture, bool shouldDestroyTexture);
	}
}
