////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class CartographyVolume : LevelVolume<CartographyVolume, CartographyVolumeManager>
	{
		public void GetSatelliteCaptureTransform(out Vector3 position, out Quaternion rotation)
		{
			position = transform.TransformPoint(new Vector3(0.0f, 0.5f, 0.0f));
			// Pitch down 90 degrees.
			rotation = transform.rotation * Quaternion.Euler(90.0f, 0.0f, 0.0f);
		}

		protected override void Awake()
		{
			supportsSphereShape = false;
			base.Awake();
		}
	}
}
