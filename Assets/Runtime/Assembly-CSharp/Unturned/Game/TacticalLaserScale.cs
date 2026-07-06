////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Component for the tactical laser attachment's red dot.
	/// Resizes itself per-camera to maintain a constant on-screen size.
	/// </summary>
	public class TacticalLaserScale : MonoBehaviour
	{
		public float scaleMultiplier = 0.1f;

		/// <summary>
		/// Used to tune the scale by distance so that far away laser is not quite as comically large.
		/// </summary>
		public AnimationCurve scalingCurve;

		public void OnWillRenderObject()
		{
			Camera camera = Camera.current;
			float distanceFromCamera = (transform.position - camera.transform.position).magnitude;
			float halfVerticalFovDegrees = camera.fieldOfView * 0.5f;
			float halfVerticalFovRadians = Mathf.Deg2Rad * halfVerticalFovDegrees;
			float screenRatio = Mathf.Tan(halfVerticalFovRadians);
			float newScale = scalingCurve.Evaluate(distanceFromCamera * screenRatio) * scaleMultiplier;
			transform.localScale = new Vector3(newScale, newScale, newScale);
		}
	}
}
