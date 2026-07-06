////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class GraphicsSettingsResolution
	{
		public int Width { get; set; }
		public int Height { get; set; }

		public GraphicsSettingsResolution(Resolution resolution)
		{
			this.Width = resolution.width;
			this.Height = resolution.height;
		}

		public GraphicsSettingsResolution()
		{
			this.Width = 0;
			this.Height = 0;
		}
	}
}
