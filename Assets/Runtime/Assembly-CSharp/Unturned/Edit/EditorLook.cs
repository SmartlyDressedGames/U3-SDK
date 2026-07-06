////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorLook : MonoBehaviour
	{
		private static float _pitch;
		public static float pitch => _pitch;

		private static float _yaw;
		public static float yaw => _yaw;

		private Camera highlightCamera;

		private void Update()
		{
			if (EditorInteract.isFlying && Level.isEditor)
			{
				float sprintFovBoost = (EditorMovement.isMoving && InputEx.GetKey(ControlsSettings.modify)) ? (OptionsSettings.sprintFovBoostIntensity * 10f) : 0f;
				MainCamera.instance.fieldOfView = Mathf.Lerp(MainCamera.instance.fieldOfView, OptionsSettings.DesiredVerticalFieldOfView + sprintFovBoost, 8 * Time.deltaTime);
				highlightCamera.fieldOfView = MainCamera.instance.fieldOfView;

				_yaw += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_x");

				if (ControlsSettings.invert)
				{
					_pitch += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y");
				}
				else
				{
					_pitch -= ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y");
				}

				if (pitch > 90)
				{
					_pitch = 90;
				}
				else if (pitch < -90)
				{
					_pitch = -90;
				}

				MainCamera.instance.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
				transform.rotation = Quaternion.Euler(0, yaw, 0);
			}
		}

		private void Start()
		{
			MainCamera.instance.fieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
			highlightCamera = MainCamera.instance.transform.Find("HighlightCamera").GetComponent<Camera>();
			highlightCamera.fieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
			highlightCamera.eventMask = 0;

			_pitch = MainCamera.instance.transform.localRotation.eulerAngles.x;

			if (pitch > 90)
			{
				_pitch = -360 + pitch;
			}

			_yaw = transform.rotation.eulerAngles.y;

			//UnityStandardAssets.CinematicEffects.AntiAliasing ppaa = highlightCamera.GetComponent<UnityStandardAssets.CinematicEffects.AntiAliasing>();
			//if(ppaa != null)
			//{
			//	EAntiAliasingType antiAliasingType = GraphicsSettings.antiAliasingType;

			//	ppaa.enabled = antiAliasingType == EAntiAliasingType.FXAA || antiAliasingType == EAntiAliasingType.TAA;
			//	if(antiAliasingType == EAntiAliasingType.FXAA)
			//	{
			//		ppaa.method = 1;
			//	}
			//	else if(antiAliasingType == EAntiAliasingType.TAA)
			//	{
			//		ppaa.method = 0;
			//	}
			//}
		}
	}
}
