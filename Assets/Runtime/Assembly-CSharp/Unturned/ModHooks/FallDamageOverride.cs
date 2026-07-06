////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Overrides how fall damage is calculated when landing on this game object or its descendants.
	/// </summary>
	[AddComponentMenu("Unturned/Fall Damage Override")]
	public class FallDamageOverride : MonoBehaviour
	{
		/// <summary>
		/// Could be extended in the future to increase, decrease, or set fall damage.
		/// </summary>
		public enum EMode
		{
			/// <summary>
			/// Potentially useful for an event to toggle the override.
			/// </summary>
			None,

			/// <summary>
			/// Character will not take any fall damage.
			/// </summary>
			PreventFallDamage,
		}

		public EMode Mode = EMode.PreventFallDamage;
	}
}
