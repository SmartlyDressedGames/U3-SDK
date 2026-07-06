////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class HumanAnimator : CharacterAnimator
	{
		public static readonly float LEAN = 20;

		private float _lean;
		public float lean;

		private float _pitch = 0f; // Not 90 because _pitch is pitch-90.
		public float pitch = 90.0f;

		private float _offset;
		public float offset;

		public void force()
		{
			this._lean = Mathf.Clamp(lean, -1.0f, 1.0f);
			this._pitch = Mathf.Clamp(pitch, 1.0f, 179.0f) - 90.0f;
			this._offset = offset;
		}

		public void apply()
		{
			UnityEngine.Profiling.Profiler.BeginSample("GetAnimationPlaying");
			bool animate = getAnimationPlaying();
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Parent");
			if (animate)
			{
				leftShoulder.parent = skull;
				rightShoulder.parent = skull;
				spineHook.parent = skull;
			}
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Rotate");
			spine.Rotate(0, _pitch * 0.5f, _lean * LEAN);
			skull.Rotate(0, _pitch * 0.5f, 0);
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Offset");
			skull.position += skull.forward * offset;
			UnityEngine.Profiling.Profiler.EndSample();

			if (animate)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Animate");
				skull.Rotate(0, -spine.localRotation.eulerAngles.x + (_pitch * 0.5f), 0);

				leftShoulder.parent = spine;
				rightShoulder.parent = spine;
				spineHook.parent = spine;

				skull.Rotate(0, spine.localRotation.eulerAngles.x - (_pitch * 0.5f), 0);
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		private void LateUpdate()
		{
			UnityEngine.Profiling.Profiler.BeginSample("Lerp");
			_lean = Mathf.LerpAngle(_lean, Mathf.Clamp(lean, -1.0f, 1.0f), 4 * Time.deltaTime);
			_pitch = Mathf.LerpAngle(_pitch, Mathf.Clamp(pitch, 1.0f, 179.0f) - 90.0f, 8 * Time.deltaTime);
			_offset = Mathf.Lerp(_offset, offset, 4 * Time.deltaTime);
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Apply");
			apply();
			UnityEngine.Profiling.Profiler.EndSample();
		}

		private void Awake()
		{
			init();
		}
	}
}
