////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

/// <summary>
/// Hacky workaround to fix item skin material leak. Unfortunately none of the original item skin code destroyed
/// instantiated materials, and did not keep a reference to the instantiated materials, so until that code gets a
/// rewrite this will take care of cleanup.
/// </summary>
public class DestroyMaterialOnDestroy : MonoBehaviour
{
	public Material instantiatedMaterial;

	private void OnDestroy()
	{
		Destroy(instantiatedMaterial);
	}
}
