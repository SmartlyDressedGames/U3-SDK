////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
using SDG.Unturned;
using UnityEngine;

/// <summary>
/// Debug component in the editor to see if GetRandomForwardVectorInCone seems correct. :)
/// Not much of a mathematician. :(
/// </summary>
public class RandomConeTest : MonoBehaviour
{
	public void OnDrawGizmos()
	{
		for (int i = 0; i < iterations; ++i)
		{
			float angleRadians = angleDegrees * Mathf.Deg2Rad;
			Vector3 dir = RandomEx.GetRandomForwardVectorInCone(angleRadians);
			Debug.Assert(dir.IsNormalized());
			Gizmos.DrawLine(transform.position, transform.position + (dir * length));
		}
	}

	public float angleDegrees;

	public int iterations = 100;
	public float length = 1.0f;
}
#endif // UNITY_EDITOR
