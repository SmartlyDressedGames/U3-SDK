////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if !WITH_NOREDIST
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	class HighlightFallback : MonoBehaviour
	{
		public void SetColor(Color color)
		{
			if (managedRenderers != null)
			{
				foreach (ManagedRenderer renderer in managedRenderers)
				{
					renderer.ownedMaterial.SetColor(mainColorId, color);
				}
			}
		}

		public void Awake()
		{
			tempRenderers.Clear();
			GetComponentsInChildren(true, tempRenderers);
			if (tempRenderers.Count < 1)
			{
				return;
			}

			managedRenderers = new List<ManagedRenderer>();
			foreach (Renderer rendererComponent in tempRenderers)
			{
				Material originalMaterial = rendererComponent.sharedMaterial;
				if (originalMaterial == null || !originalMaterial.HasColor(mainColorId))
				{
					return;
				}

				// Renderer's material may already be owned by something else, so we instantiate a new
				// one that we'll manage the lifecycle of.
				Material ownedMaterial = Instantiate(originalMaterial);
				rendererComponent.sharedMaterial = ownedMaterial;
				managedRenderers.Add(new ManagedRenderer()
				{
					renderer = rendererComponent,
					ownedMaterial = ownedMaterial,
					originalMaterial = originalMaterial,
				});
			}
		}

		public void OnDestroy()
		{
			if (managedRenderers != null)
			{
				foreach (ManagedRenderer managedRenderer in managedRenderers)
				{
					if (managedRenderer.renderer != null)
					{
						managedRenderer.renderer.sharedMaterial = managedRenderer.originalMaterial;
					}

					if (managedRenderer.ownedMaterial != null)
					{
						Destroy(managedRenderer.ownedMaterial);
					}
				}
				managedRenderers = null;
			}
		}

		struct ManagedRenderer
		{
			public Renderer renderer;
			public Material ownedMaterial;
			public Material originalMaterial;
		}

		private static List<Renderer> tempRenderers = new List<Renderer>();
		private List<ManagedRenderer> managedRenderers;
		private static int mainColorId = Shader.PropertyToID("_Color");
	}
}
#endif // !WITH_NOREDIST
