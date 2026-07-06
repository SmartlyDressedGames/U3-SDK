////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public static class AssetValidation
	{
		public static void ValidateLayersEqual(Asset owningAsset, GameObject gameObject, int expectedLayer)
		{
			int actualLayer = gameObject.layer;
			if (actualLayer != expectedLayer)
			{
				Assets.ReportError(owningAsset, "expected '{0}' to have layer {1}, but it actually has layer {2}", gameObject.name, expectedLayer, actualLayer);
			}
		}

		public static void ValidateLayersEqualRecursive(Asset owningAsset, GameObject gameObject, int expectedLayer)
		{
			ValidateLayersEqual(owningAsset, gameObject, expectedLayer);
			foreach (Transform child in gameObject.transform)
			{
				ValidateLayersEqualRecursive(owningAsset, child.gameObject, expectedLayer);
			}
		}

		public static void ValidateClothComponents(Asset owningAsset, GameObject gameObject)
		{
			clothComponents.Clear();
			gameObject.GetComponentsInChildren(clothComponents);
			foreach (Cloth component in clothComponents)
			{
				if (component.capsuleColliders.Length > 0 || component.sphereColliders.Length > 0)
				{
					Assets.ReportError(owningAsset, $"{gameObject.name} cloth component \"{component.name}\" has colliders which is problematic because unfortunately the game does not yet have a way for weapons to ignore them");
				}
			}
		}

		/// <summary>
		/// Relatively efficiently find mesh components, and log an error if their mesh is missing, among other checks.
		/// </summary>
		public static void searchGameObjectForErrors(Asset owningAsset, GameObject gameObject)
		{
			if (gameObject == null)
				throw new ArgumentNullException("gameObject");

			if (Assets.shouldValidateAssets == false)
				return;

			staticMeshComponents.Clear();
			gameObject.GetComponentsInChildren(staticMeshComponents);
			foreach (MeshFilter component in staticMeshComponents)
			{
				internalValidateMesh(owningAsset, gameObject, component, component.sharedMesh, 50000);
			}

			meshColliderComponents.Clear();
			gameObject.GetComponentsInChildren(meshColliderComponents);
			foreach (MeshCollider component in meshColliderComponents)
			{
				internalValidateMesh(owningAsset, gameObject, component, component.sharedMesh, 25000);
			}

			allRenderers.Clear();
			gameObject.GetComponentsInChildren(allRenderers);
			foreach (Renderer component in allRenderers)
			{
				if (component.motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion)
				{
					Assets.ReportError(owningAsset, "{0} Renderer \"{1}\" motion vectors disabled could be a problem for TAA", gameObject.name, component.name);
				}
			}

			meshRenderers.Clear();
			gameObject.GetComponentsInChildren(meshRenderers);
			foreach (MeshRenderer component in meshRenderers)
			{
				MeshFilter pairedMeshFilter = component.GetComponent<MeshFilter>();
				if (pairedMeshFilter == null)
				{
					TMPro.TextMeshPro textMesh = component.GetComponent<TMPro.TextMeshPro>();
					if (textMesh == null)
					{
						Assets.ReportError(owningAsset, "{0} missing MeshFilter or TextMesh for MeshRenderer '{1}'", gameObject.name, component.name);
					}
				}
				else if (component.name != "DepthMask") // DepthMask is assigned by game.
				{
					internalValidateRendererMaterials(owningAsset, gameObject, component);
				}
			}

			skinnedMeshRenderers.Clear();
			gameObject.GetComponentsInChildren(skinnedMeshRenderers);
			foreach (SkinnedMeshRenderer component in skinnedMeshRenderers)
			{
				internalValidateMesh(owningAsset, gameObject, component, component.sharedMesh, 50000);
				internalValidateRendererMaterials(owningAsset, gameObject, component);
			}

			// Nelson 2024-05-23: This warning is no longer useful since lots of newer maps have really long audio clips.
// 			audioSources.Clear();
// 			gameObject.GetComponentsInChildren(audioSources);
// 			foreach (AudioSource audioSource in audioSources)
// 			{
// 				AudioClip clip = audioSource.clip;
// 				if (clip != null && clip.samples > 2000000)
// 				{
// 					Assets.reportError(owningAsset, "{0} clip '{1}' for AudioSource '{2}' has {3} samples (ideal maximum of {4}) and could be compressed.", gameObject.name, clip.name, audioSource.name, clip.samples, 2000000);
// 				}
// 			}

			lodGroupComponents.Clear();
			gameObject.GetComponentsInChildren(true, lodGroupComponents);
			foreach (LODGroup component in lodGroupComponents)
			{
				InternalValidateLodGroupComponent(owningAsset, component);
			}

			InternalValidateRendererMultiLodRegistration(owningAsset);
		}

		private static void internalValidateMesh(Asset owningAsset, GameObject gameObject, Component component, Mesh sharedMesh, int maximumVertexCount)
		{
			if (sharedMesh == null)
			{
				if (component.GetComponent<TMPro.TextMeshPro>() != null)
				{
					// TMP creates a mesh filter with empty mesh until text is built.
					return;
				}

				Assets.ReportError(owningAsset, "{0} missing mesh for {1} '{2}'", gameObject.name, component.GetType().Name, component.name);
			}
			else
			{
				if (sharedMesh.vertexCount > maximumVertexCount)
				{
					Assets.ReportError(owningAsset, "{0} mesh for {1} '{2}' has {3} vertices (ideal maximum of {4}) and might have room for optimization.", gameObject.name, component.GetType().Name, component.name, sharedMesh.vertexCount, maximumVertexCount);
				}

				/*
					Mesh read/write situation is questionable at the moment.

					// Navmesh needs to be readable for recast.
					bool shouldBeReadable = sharedMesh.name.StartsWith("Nav", StringComparison.InvariantCultureIgnoreCase);
					if(shouldBeReadable == false) shouldBeReadable |= gameObject.name.StartsWith("Nav", StringComparison.InvariantCultureIgnoreCase);

					// Collision mesh is sometimes used for navigation object as well.
					if(shouldBeReadable == false) shouldBeReadable |= sharedMesh.name.StartsWith("Clip", StringComparison.InvariantCultureIgnoreCase);
					if(shouldBeReadable == false) shouldBeReadable |= sharedMesh.name.StartsWith("Collision", StringComparison.InvariantCultureIgnoreCase);

					if(sharedMesh.isReadable != shouldBeReadable)
					{
						if(shouldBeReadable)
						{
							Assets.reportError(owningAsset, "{0} mesh '{1}' for {2} '{3}' should be readable for navmesh.", gameObject.name, sharedMesh.name, component.GetType().Name, component.name);
						}
						else
						{
							Assets.reportError(owningAsset, "{0} mesh '{1}' for {2} '{3}' can save memory by disabling read/write.", gameObject.name, sharedMesh.name, component.GetType().Name, component.name);
						}
					}
				*/
			}
		}

		private static void internalValidateRendererMaterials(Asset owningAsset, GameObject gameObject, Renderer component)
		{
			int materialCount = component.sharedMaterials.Length;
			if (materialCount == 0)
			{
				Assets.ReportError(owningAsset, "{0} missing materials for Renderer '{1}'", gameObject.name, component.name);
			}
			else
			{
				if (materialCount > 4)
				{
					Assets.ReportError(owningAsset, "{0} Renderer '{1}' has {2} separate materials (ideal maximum of {3}) which should be optimized to reduce draw calls.", gameObject.name, component.name, materialCount, 4);
				}

				for (int materialIndex = 0; materialIndex < materialCount; ++materialIndex)
				{
					Material sharedMaterial = component.sharedMaterials[materialIndex];
					if (sharedMaterial == null)
					{
						Assets.ReportError(owningAsset, "{0} missing material[{1}] for Renderer '{2}'", gameObject.name, materialIndex, component.name);
					}
					else
					{
						internalValidateMaterialTextures(owningAsset, gameObject, component, sharedMaterial);
					}
				}
			}
		}

		private static List<int> texturePropertyNameIDs = new List<int>();
		private static void internalValidateMaterialTextures(Asset owningAsset, GameObject gameObject, Renderer component, Material sharedMaterial)
		{
			texturePropertyNameIDs.Clear();
			sharedMaterial.GetTexturePropertyNameIDs(texturePropertyNameIDs);
			foreach (int nameID in texturePropertyNameIDs)
			{
				Texture texture = sharedMaterial.GetTexture(nameID);
				// Nelson 2024-10-08: Exempting RenderTextures because they report as readable, but don't have an option
				// to *not* be readable. Presumably their data is only copied to RAM when requested anyway.
				if (texture != null && owningAsset.ignoreTextureReadable == false && !(texture is RenderTexture) && texture.isReadable)
				{
					Assets.ReportError(owningAsset, "{0} texture '{1}' referenced by material '{2}' used by Renderer '{3}' can save memory by disabling read/write.", gameObject.name, texture.name, sharedMaterial.name, component.name);
				}

				Texture2D texture2 = texture as Texture2D;
				if (texture2 != null && owningAsset.ignoreNPOT == false)
				{
					if (Mathf.IsPowerOfTwo(texture2.width) == false || Mathf.IsPowerOfTwo(texture2.height) == false)
					{
						Assets.ReportError(owningAsset, "{0} texture '{1}' referenced by material '{2}' used by Renderer '{3}' has NPOT dimensions ({4} x {5})", gameObject.name, texture.name, sharedMaterial.name, component.name, texture2.width, texture2.height);
					}
				}
			}
		}

		private static void InternalValidateLodGroupComponent(Asset owningAsset, LODGroup component)
		{
			LOD[] lods = component.GetLODs();
			for (int lodIndex = 0; lodIndex < lods.Length; ++lodIndex)
			{
				LOD lod = lods[lodIndex];
				if (lod.renderers.Length < 1)
				{
					Assets.ReportError(owningAsset, "LOD group on \"{0}\" LOD level {1} is empty", component.GetSceneHierarchyPath(), lodIndex);
					continue;
				}

				int missingRendererCount = 0;
				for (int rendererIndex = 0; rendererIndex < lod.renderers.Length; ++rendererIndex)
				{
					Renderer renderer = lod.renderers[rendererIndex];
					if (renderer == null)
					{
						++missingRendererCount;
					}
				}

				if (missingRendererCount > 0)
				{
					Assets.ReportError(owningAsset, "LOD group on \"{0}\" LOD level {1} missing {2} renderer(s)", component.GetSceneHierarchyPath(), lodIndex, missingRendererCount);
				}
			}
		}

		/// <summary>
		/// Unity warns about renderers registered with more than one LOD group, so we do our own validation as part of
		/// asset loading to make it easier to find these.
		/// </summary>
		private static void InternalValidateRendererMultiLodRegistration(Asset owningAsset)
		{
			foreach (Renderer validatingRenderer in allRenderers)
			{
				LODGroup firstFoundLodGroup = null;
				int firstFoundLodIndex = 0;

				foreach (LODGroup lodGroup in lodGroupComponents)
				{
					LOD[] lods = lodGroup.GetLODs();
					for (int lodIndex = 0; lodIndex < lods.Length; ++lodIndex)
					{
						LOD lod = lods[lodIndex];
						foreach (Renderer lodRenderer in lod.renderers)
						{
							if (lodRenderer == validatingRenderer)
							{
								if (firstFoundLodGroup == null)
								{
									firstFoundLodGroup = lodGroup;
									firstFoundLodIndex = lodIndex;
								}
								else if (lodGroup != firstFoundLodGroup)
								{
									Assets.ReportError(owningAsset, "renderer on \"{0}\" is registered with more than one LOD group, found in \"{1}\" LOD level {2} and \"{3}\" LOD level {4}",
										validatingRenderer.GetSceneHierarchyPath(), firstFoundLodGroup.GetSceneHierarchyPath(),
										firstFoundLodIndex, lodGroup.GetSceneHierarchyPath(), lodIndex);
									goto NextRenderer;
								}
							}
						}
					}
				}

				NextRenderer:;
			}
		}

		private static List<MeshFilter> staticMeshComponents = new List<MeshFilter>();
		private static List<MeshCollider> meshColliderComponents = new List<MeshCollider>();
		private static List<Renderer> allRenderers = new List<Renderer>();
		private static List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
		private static List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
		//private static List<AudioSource> audioSources = new List<AudioSource>();
		private static List<Cloth> clothComponents = new List<Cloth>();
		private static List<LODGroup> lodGroupComponents = new List<LODGroup>();
	}
}
