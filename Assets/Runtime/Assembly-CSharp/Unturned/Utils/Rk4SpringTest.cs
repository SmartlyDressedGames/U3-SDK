////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using SDG.Unturned;
using UnityEngine;

public class Rk4SpringTest : MonoBehaviour
{
	public Rk4Spring3 spring;
	public Rk4SpringQ rotationSpring;

	public void Update()
	{
		spring.Update(Time.deltaTime);
		transform.position = spring.currentPosition;

		rotationSpring.currentRotation = transform.rotation;
		rotationSpring.Update(Time.deltaTime);
		transform.rotation = rotationSpring.currentRotation;
	}

	public void Awake()
	{
		rotationSpring.targetRotation = Quaternion.identity;
	}
}
#endif // UNITY_EDITOR
