////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

// Used in the menu item inspect screen
public class ItemLook : MonoBehaviour
{
	public Camera inspectCamera;

	public float _yaw;
	public float yaw;
	public GameObject target;

	private void Update()
	{
		if (target == null)
			return;

		renderers.Clear();
		const bool includeInactive = false; // Inactive components might have invalid bounds?
		target.GetComponentsInChildren(includeInactive, renderers);

		Bounds worldBounds = new Bounds();
		bool hasBounds = false;
		foreach (Renderer modelRenderer in renderers)
		{
			if ((modelRenderer is MeshRenderer) || (modelRenderer is SkinnedMeshRenderer))
			{
				if (hasBounds)
				{
					worldBounds.Encapsulate(modelRenderer.bounds);
				}
				else
				{
					hasBounds = true;
					worldBounds = modelRenderer.bounds;
				}
			}
		}

		if (!hasBounds)
		{
			return;
		}

		// Now that unity defers updating physics transforms the bounds aren't accurate during setup
		Vector3 center = worldBounds.center;
		float distance = worldBounds.extents.magnitude * 2.25f;

		_yaw = Mathf.Lerp(_yaw, yaw, 4 * Time.deltaTime);
		inspectCamera.transform.rotation = Quaternion.Euler(20, -_yaw, 0);
		inspectCamera.transform.position = center - (inspectCamera.transform.forward * distance);
	}

	private List<Renderer> renderers = new List<Renderer>();
}
