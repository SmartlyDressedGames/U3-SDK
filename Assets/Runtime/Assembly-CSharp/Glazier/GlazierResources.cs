////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	internal static class GlazierResources
	{
		/// <summary>
		/// White 1x1 texture for solid colored images.
		/// uGUI empty image draws like this, but we need the texture for IMGUI backwards compatibility.
		/// </summary>
		public static StaticResourceRef<Texture2D> PixelTexture
		{
			get;
			private set;
		} = new StaticResourceRef<Texture2D>("Materials/Pixel");
	}
}
