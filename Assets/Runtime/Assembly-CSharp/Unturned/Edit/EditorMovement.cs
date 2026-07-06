////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class EditorMovement : MonoBehaviour
	{
		private static bool _isMoving;
		public static bool isMoving => _isMoving;

		private float speed = 32;
		private Vector3 input;

		private void Update()
		{
			if (EditorInteract.isFlying && Level.isEditor)
			{
				if (InputEx.GetKey(ControlsSettings.left))
				{
					input.x = -1;//Mathf.Lerp(input.x, -1, InputSettings.moveSensitivity*Time.deltaTime);
				}
				else if (InputEx.GetKey(ControlsSettings.right))
				{
					input.x = 1;//Mathf.Lerp(input.x, 1, InputSettings.moveSensitivity*Time.deltaTime);
				}
				else
				{
					input.x = 0;//Mathf.Lerp(input.x, 0, InputSettings.moveSensitivity*Time.deltaTime);
				}

				if (InputEx.GetKey(ControlsSettings.up))
				{
					input.z = 1;//Mathf.Lerp(input.z, 1, InputSettings.moveSensitivity*Time.deltaTime);
				}
				else if (InputEx.GetKey(ControlsSettings.down))
				{
					input.z = -1;//Mathf.Lerp(input.z, -1, InputSettings.moveSensitivity*Time.deltaTime);
				}
				else
				{
					input.z = 0;//Mathf.Lerp(input.z, 0, InputSettings.moveSensitivity*Time.deltaTime);
				}

				_isMoving = input.x != 0 || input.z != 0;

				speed = Mathf.Clamp(speed + (Input.GetAxis("mouse_z") * 0.2f * speed), 0.5f, 2048);

				float height = 0;
				if (InputEx.GetKey(ControlsSettings.ascend))
				{
					height = 1;
				}
				else if (InputEx.GetKey(ControlsSettings.descend))
				{
					height = -1;
				}

				transform.position += (MainCamera.instance.transform.rotation * input * speed * Time.deltaTime) + (Vector3.up * height * Time.deltaTime * speed);

				/*
					Modders have requested disabling this clamp, especially since
					the devkit editor can build outside the map.
				
				Vector3 point = transform.position;
				point.x = Mathf.Clamp(point.x, -Level.size, Level.size);
				point.y = Mathf.Clamp(point.y, 0, Level.HEIGHT);
				point.z = Mathf.Clamp(point.z, -Level.size, Level.size);
				transform.position = point;
				*/
			}
		}
	}
}
