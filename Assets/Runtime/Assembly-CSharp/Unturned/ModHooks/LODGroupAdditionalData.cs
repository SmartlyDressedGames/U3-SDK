////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Optional Unturned extensions to the LOD Group component.
	/// </summary>
	[AddComponentMenu("Unturned/LOD Group Additional Data")]
	[RequireComponent(typeof(LODGroup))]
	public class LODGroupAdditionalData : MonoBehaviour
	{
		/// <summary>
		/// Could be extended, e.g. to clamp cull size separately from the per-LOD sizes.
		/// </summary>
		public enum ELODBiasOverride
		{
			None,

			/// <summary>
			/// Unturned will adjust per-LOD sizes to counteract LOD bias.
			/// Some maps have very carefully tuned LOD sizes, so LOD bias affecting them is undesirable.
			/// Note that due to a Unity bug only LOD0 can be greater than 100%.
			/// </summary>
			IgnoreLODBias,
		}

		public ELODBiasOverride LODBiasOverride = ELODBiasOverride.IgnoreLODBias;

#if GAME
		private void Start()
		{
			LODGroupManager.Get().Register(this);
		}

		private void OnDestroy()
		{
			LODGroupManager.Get().Unregister(this);
		}
#endif // GAME
	}
}
