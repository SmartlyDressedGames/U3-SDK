////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Registers renderers with DynamicWaterTransparentSort manager.
	/// </summary>
	public class DynamicWaterTransparentSortComponent : MonoBehaviour
	{
		public Renderer[] renderers;

		private void Awake()
		{
			if (renderers != null && renderers.Length > 0)
			{
				managedMaterials = new List<ManagedMaterial>(renderers.Length);
				foreach (Renderer component in renderers)
				{
					Transform rendererTransform = component.transform;
					Material[] materials = component.materials;
					foreach (Material instantiatedMaterial in materials)
					{
						ManagedMaterial data = new ManagedMaterial();
						data.rendererTransform = rendererTransform;
						data.instantiatedMaterial = instantiatedMaterial;
						managedMaterials.Add(data);
					}
				}
			}
		}

		private void OnEnable()
		{
			if (managedMaterials != null)
			{
				DynamicWaterTransparentSort manager = DynamicWaterTransparentSort.Get();
				for (int index = managedMaterials.Count - 1; index >= 0; --index)
				{
					ManagedMaterial data = managedMaterials[index];
					data.handle = manager.Register(data.rendererTransform, data.instantiatedMaterial);
					managedMaterials[index] = data;
				}
			}
		}

		private void OnDisable()
		{
			if (managedMaterials != null)
			{
				DynamicWaterTransparentSort manager = DynamicWaterTransparentSort.Get();
				foreach (ManagedMaterial data in managedMaterials)
				{
					manager.Unregister(data.handle);
				}
			}
		}

		private void OnDestroy()
		{
			if (managedMaterials != null)
			{
				foreach (ManagedMaterial data in managedMaterials)
				{
					Destroy(data.instantiatedMaterial);
				}
				managedMaterials = null;
			}
		}

		private List<ManagedMaterial> managedMaterials;

		private struct ManagedMaterial
		{
			public Transform rendererTransform;
			public Material instantiatedMaterial;
			public object handle;
		}
	}
}
