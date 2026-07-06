////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
namespace SDG.Framework.Foliage
{
	public class FoliageSettings
	{
		private static bool _enabled = false;
		public static bool enabled
		{
			get => _enabled;
			set => _enabled = value;
		}

		private static bool _drawFocus = false;
		public static bool drawFocus
		{
			get => _drawFocus;
			set => _drawFocus = value;
		}

		private static int _drawDistance = 0;
		public static int drawDistance
		{
			get => _drawDistance;
			set => _drawDistance = value;
		}

		private static int _drawFocusDistance = 0;
		public static int drawFocusDistance
		{
			get => _drawFocusDistance;
			set => _drawFocusDistance = value;
		}

		internal static float _instanceDensity = 0;
		public static float instanceDensity
		{
			get => _instanceDensity;
			set => _instanceDensity = value;
		}

		private static bool _forceInstancingOff = false;
		public static bool forceInstancingOff
		{
			get => _forceInstancingOff;
			set => _forceInstancingOff = value;
		}

		private static float _focusDistance = 0;
		public static float focusDistance
		{
			get => _focusDistance;
			set => _focusDistance = value;
		}
	}
}
