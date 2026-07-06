////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Merges textures used in the level into an atlas to assist runtime draw call batching.
	/// </summary>
	internal class LevelBatching
	{
		public static LevelBatching Get()
		{
			return instance;
		}

		public void Reset()
		{
			if (standardDecalableOpaque == null)
			{
				standardDecalableOpaque = new ShaderGroup();
				standardDecalableOpaque.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/StandardDecalableOpaque");
			}
			else
			{
				standardDecalableOpaque.Clear();
			}

			if (standardSpecularSetupDecalableOpaque == null)
			{
				standardSpecularSetupDecalableOpaque = new ShaderGroup();
				standardSpecularSetupDecalableOpaque.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/StandardSpecularSetupDecalableOpaque");
			}
			else
			{
				standardSpecularSetupDecalableOpaque.Clear();
			}

			if (batchableCard == null)
			{
				batchableCard = new ShaderGroup();
				batchableCard.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/Card");
			}
			else
			{
				batchableCard.Clear();
			}

			if (batchableFoliage == null)
			{
				batchableFoliage = new ShaderGroup();
				batchableFoliage.materialTemplate = Resources.Load<Material>("MaterialBatchingTemplates/Foliage");
				batchableFoliage.filterMode = FilterMode.Trilinear;
			}
			else
			{
				batchableFoliage.Clear();
			}

			if (blitMaterial == null)
			{
				blitMaterial = Object.Instantiate(Resources.Load<Material>("Materials/AtlasBlit"));
				blitMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			loggedMeshes.Clear();
			loggedTextures.Clear();
			loggedMaterials.Clear();
			staticBatchingMeshRenderers.Clear();

			maxTextureSize = Level.info.configData?.Batching_Max_Texture_Size ?? -1;
			if (maxTextureSize < 1)
			{
				maxTextureSize = 128;
			}
		}

		public void Destroy()
		{
			foreach (Object obj in objectsToDestroy)
			{
				Object.Destroy(obj);
			}
			objectsToDestroy.Clear();
		}

		public void AddLevelObject(LevelObject levelObject)
		{
			if (levelObject == null || levelObject.asset == null || levelObject.asset.shouldExcludeFromLevelBatching)
			{
				return;
			}

			if (levelObject.transform != null)
			{
				// Ignore rubble physics debris templates.
				if (levelObject.rubble != null && levelObject.rubble.rubbleInfos != null)
				{
					foreach (RubbleInfo rubbleInfo in levelObject.rubble.rubbleInfos)
					{
						if (rubbleInfo.ragdolls != null)
						{
							foreach (RubbleRagdollInfo ragdollInfo in rubbleInfo.ragdolls)
							{
								if (ragdollInfo.ragdollGameObject != null)
								{
									ignoreTransforms.Add(ragdollInfo.ragdollGameObject.transform);
								}
							}
						}
					}
				}

				AddGameObject(levelObject.transform.gameObject);
				ignoreTransforms.Clear();
			}

			if (levelObject.skybox != null)
			{
				AddGameObject(levelObject.skybox.gameObject);
			}
		}

		public void AddResourceSpawnpoint(ResourceSpawnpoint resourceSpawnpoint)
		{
			if (resourceSpawnpoint == null || resourceSpawnpoint.asset == null || resourceSpawnpoint.asset.shouldExcludeFromLevelBatching)
			{
				return;
			}

			if (resourceSpawnpoint.model != null)
			{
				AddGameObject(resourceSpawnpoint.model.gameObject);
			}

			if (resourceSpawnpoint.skybox != null)
			{
				AddGameObject(resourceSpawnpoint.skybox.gameObject);
			}
		}

		public void AddRoad(Road road)
		{
			if (road == null || road.segmentRenderers == null)
			{
				return;
			}

			staticBatchingMeshRenderers.AddRange(road.segmentRenderers);
		}

		public void ApplyTextureAtlas()
		{
			System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

			bool shouldPreview = Provider.isServer && wantsToPreviewTextureAtlas;
			ApplyTextureAtlas(standardDecalableOpaque, shouldPreview);
			ApplyTextureAtlas(standardSpecularSetupDecalableOpaque, shouldPreview);
			ApplyTextureAtlas(batchableCard, shouldPreview);
			ApplyTextureAtlas(batchableFoliage, shouldPreview);

			// 1x1 textures are unused after atlas is generated.
			DestroyColorTextures();

			watch.Stop();
			UnturnedLog.info($"Level texture atlas generation took: {watch.ElapsedMilliseconds}ms");
		}

		public void ApplyStaticBatching()
		{
			System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

			GameObject[] gameObjects = new GameObject[staticBatchingMeshRenderers.Count];
			StaticBatchingInitialState[] initialStates = new StaticBatchingInitialState[gameObjects.Length];

			// Disabling audio sources is a workaround to avoid going over audio channel limit during batching.
			// Otherwise, some maps with lots of audio sources became silent.
			List<AudioSource> activeAudioSources = new List<AudioSource>(gameObjects.Length);
			List<AudioSource> tempAudioSources = new List<AudioSource>(16);

			bool shouldPreview = Provider.isServer && wantsToPreviewUniqueMaterials;
			Dictionary<Material, List<MeshRenderer>> uniqueMaterialToMeshRenderers = null;
			if (shouldPreview)
			{
				uniqueMaterialToMeshRenderers = new Dictionary<Material, List<MeshRenderer>>();
			}

			for (int index = 0; index < gameObjects.Length; ++index)
			{
				MeshRenderer meshRenderer = staticBatchingMeshRenderers[index];
				Transform transform = meshRenderer.transform;
				GameObject gameObject = meshRenderer.gameObject;

				StaticBatchingInitialState initialState = new StaticBatchingInitialState();
				initialState.parent = transform.parent;
				initialState.wasEnabled = meshRenderer.enabled;
				initialState.wasActive = gameObject.activeSelf;

				gameObjects[index] = gameObject;
				initialStates[index] = initialState;

				meshRenderer.enabled = true;
				if (initialState.parent != null) // There was a relatively minor (~100ms total) benefit to doing these checks.
					transform.parent = null;
				if (!initialState.wasActive) // See above.
					gameObject.SetActive(true);

				gameObject.GetComponentsInChildren(/*includeInactive*/ true, tempAudioSources);
				foreach (AudioSource audioSource in tempAudioSources)
				{
					if (audioSource.enabled)
					{
						audioSource.enabled = false;
						activeAudioSources.Add(audioSource);
					}
				}

				if (shouldPreview && meshRenderer.sharedMaterial != null)
				{
					if (!uniqueMaterialToMeshRenderers.TryGetValue(meshRenderer.sharedMaterial, out List<MeshRenderer> meshRenderers))
					{
						meshRenderers = new List<MeshRenderer>();
						uniqueMaterialToMeshRenderers[meshRenderer.sharedMaterial] = meshRenderers;
					}
					meshRenderers.Add(meshRenderer);
				}
			}

			if (shouldPreview)
			{
				List<List<MeshRenderer>> meshRenderers = uniqueMaterialToMeshRenderers.Values.ToList();
				for (int listIndex = meshRenderers.Count - 1; listIndex >= 0; --listIndex)
				{
					List<MeshRenderer> list = meshRenderers[listIndex];
					if (list.Count < 2)
					{
						// Material is unique, only used by one renderer.
						meshRenderers.RemoveAtFast(listIndex);
					}
				}

				meshRenderers.Sort((List<MeshRenderer> lhs, List<MeshRenderer> rhs) =>
				{
					return rhs.Count - lhs.Count;
				});

				// Lists are now sorted by most common to least common material.

				int numberOfColors = meshRenderers.Count;
				float hueChangePerMaterial = 1.0f / numberOfColors;

				float hue = Random.value;
				for (int listIndex = 0; listIndex < numberOfColors; ++listIndex)
				{
					float alpha = listIndex / (float) numberOfColors;
					float saturation = 1.0f;
					float value = 1.0f - alpha * 0.75f;
					Color color = Color.HSVToRGB(hue, saturation, value);
					hue = (hue + hueChangePerMaterial) % 1.0f;

					Material debugMaterial = Object.Instantiate(standardDecalableOpaque.materialTemplate);
					objectsToDestroy.Add(debugMaterial);
					debugMaterial.SetColor(propertyID_Color, color);
					foreach (MeshRenderer meshRenderer in meshRenderers[listIndex])
					{
						meshRenderer.sharedMaterial = debugMaterial;
					}
				}
			}

			// Submitting every game object may seem suboptimal, but compared with doing our own
			// spatial clustering Unity seems to do a better job.
			GameObject staticBatchingRoot = new GameObject("Static Batching Root (LevelBatching)");
			StaticBatchingUtility.Combine(gameObjects, staticBatchingRoot);

			for (int index = 0; index < gameObjects.Length; ++index)
			{
				MeshRenderer meshRenderer = staticBatchingMeshRenderers[index];
				Transform transform = meshRenderer.transform;
				GameObject gameObject = meshRenderer.gameObject;
				StaticBatchingInitialState initialState = initialStates[index];

				meshRenderer.enabled = initialState.wasEnabled;
				if (!initialState.wasActive) // See above.
					gameObject.SetActive(initialState.wasActive);
				if (initialState.parent != null) // See above.
					transform.parent = initialState.parent;
			}

			foreach (AudioSource audioSource in activeAudioSources)
			{
				audioSource.enabled = true;
			}

			watch.Stop();
			UnturnedLog.info($"Level static batching took: {watch.ElapsedMilliseconds}ms");
		}

		/// <summary>
		/// Skip renderer children of these transforms, if any.
		/// For example we skip lights with material instances and rubble debris.
		/// </summary>
		private List<Transform> ignoreTransforms = new List<Transform>();

		private List<Renderer> renderers = new List<Renderer>();
		private List<Vector2> uvs = new List<Vector2>();
		private List<Material> sharedMaterials = new List<Material>();

		private ShaderGroup standardDecalableOpaque;
		private ShaderGroup standardSpecularSetupDecalableOpaque;
		private ShaderGroup batchableCard;
		private ShaderGroup batchableFoliage;
		private Material blitMaterial;

		private void AddGameObject(GameObject gameObject)
		{
			bool shouldPreview = Provider.isServer && wantsToPreviewMeshExclusions;

			renderers.Clear();
			gameObject.GetComponentsInChildren(/*includeInactive*/ true, renderers);
			foreach (Renderer renderer in renderers)
			{
				if (ignoreTransforms.Count > 0)
				{
					bool shouldIgnore = false;
					foreach (Transform parent in ignoreTransforms)
					{
						if (renderer.transform.IsChildOf(parent))
						{
							shouldIgnore = true;
							break;
						}
					}

					if (shouldIgnore)
						continue;
				}

				bool shouldAssignDebugMaterial = false;

				if (renderer is MeshRenderer meshRenderer)
				{
					try
					{
						MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
						Mesh mesh = meshFilter?.sharedMesh;
						if (mesh != null)
						{
							if (CanBatchMesh(mesh, renderer))
							{
								AddMesh(meshFilter, meshRenderer);
								staticBatchingMeshRenderers.Add(meshRenderer);
							}
							else
							{
								shouldAssignDebugMaterial = true;
							}
						}
					}
					catch (UnityEngine.MissingComponentException exception)
					{
						UnturnedLog.exception(exception, $"Caught MissingComponentException looking for MeshFilter component on MeshRenderer at {meshRenderer.GetSceneHierarchyPath()}");
					}
				}
				else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
				{
					Mesh mesh = skinnedMeshRenderer.sharedMesh;
					if (mesh != null)
					{
						if (CanBatchMesh(mesh, renderer))
						{
							AddSkinnedMesh(skinnedMeshRenderer);
						}
						else
						{
							shouldAssignDebugMaterial = true;
						}
					}
				}

				if (shouldAssignDebugMaterial && shouldPreview)
				{
					Material debugMaterial = Object.Instantiate(standardDecalableOpaque.materialTemplate);
					debugMaterial.name = "Excluded mesh preview";
					objectsToDestroy.Add(debugMaterial);
					debugMaterial.SetColor(propertyID_Color, Random.ColorHSV());
					renderer.sharedMaterial = debugMaterial;
				}
			}
		}

		private TextureUsers AddMeshCommon(Renderer renderer)
		{
			sharedMaterials.Clear();
			renderer.GetSharedMaterials(sharedMaterials);
			if (sharedMaterials.Count < 1)
			{
				if (shouldLogTextureAtlasExclusions)
				{
					UnturnedLog.info($"Excluding renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because it has no materials");
				}
				return null;
			}
			else if (sharedMaterials.Count > 1)
			{
				// 2023-02-10: most vanilla content has one material per renderer, but lots of modded content uses multiple.
				// If each material uses different shaders however their textures would go into different atlas as-is.
				// To allow multiple materials maybe this should be rewritten to share an atlas texture between ALL shaders?
				if (shouldLogTextureAtlasExclusions)
				{
					UnturnedLog.info($"Excluding renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because more than one material is not supported (yet?)");
				}
				return null;
			}

			Material material = sharedMaterials[0];
			if (material == null)
			{
				if (shouldLogTextureAtlasExclusions)
				{
					UnturnedLog.info($"Excluding renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because material is null");
				}
				return null;
			}

			if (material.name.EndsWith(" (Instance)", System.StringComparison.Ordinal))
			{
				if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
				{
					UnturnedLog.info($"Excluding material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because it was probably instantiated for dynamic use");
				}
				return null;
			}

			Shader shader = material.shader;
			if (shader == null)
				return null;

			if (!material.HasProperty(propertyID_MainTex))
			{
				if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
				{
					UnturnedLog.info($"Excluding material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because shader \"{shader.name}\" does not use a main texture");
				}
				return null;
			}

			ShaderGroup group = null;
			Texture2D texture = material.mainTexture as Texture2D;
			UniqueTextureConfiguration textureConfiguration = new UniqueTextureConfiguration();
			textureConfiguration.texture = texture;
			textureConfiguration.color = Color.white;
			bool isGeneratedTexture = false;
			if (texture == null)
			{
				if (shader.name == "Standard (Decalable)")
				{
					if (CanAtlasStandardMaterialSimpleOpaque(material, renderer, /*isSpecular*/ false))
					{
						textureConfiguration.texture = GetOrAddColorTexture(material);
						isGeneratedTexture = true;
						group = standardDecalableOpaque;
					}
				}
				else if (shader.name == "Standard (Specular setup) (Decalable)")
				{
					if (CanAtlasStandardMaterialSimpleOpaque(material, renderer, /*isSpecular*/ true))
					{
						textureConfiguration.texture = GetOrAddColorTexture(material);
						isGeneratedTexture = true;
						group = standardSpecularSetupDecalableOpaque;
					}
				}
			}
			else
			{
				if (texture.width > maxTextureSize || texture.height > maxTextureSize)
				{
					if (shouldLogTextureAtlasExclusions && loggedTextures.Add(texture))
					{
						UnturnedLog.info($"Excluding texture \"{texture.name}\" in material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because dimensions ({texture.width}x{texture.height}) are higher than limit ({maxTextureSize}x{maxTextureSize})");
					}
					return null;
				}

				if (texture.wrapMode != TextureWrapMode.Clamp)
				{
					if (shouldLogTextureAtlasExclusions && loggedTextures.Add(texture))
					{
						UnturnedLog.info($"Excluding texture \"{texture.name}\" in material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Wrap Mode ({texture.wrapMode}) is not Clamp");
					}
					return null;
				}

				if (shader.name == "Standard (Decalable)")
				{
					if (CanAtlasStandardMaterialSimpleOpaque(material, renderer, /*isSpecular*/ false) && CanAtlasTextureFilterMode(texture, material, renderer, FilterMode.Point))
					{
						group = standardDecalableOpaque;
						textureConfiguration.color = material.GetColor(propertyID_Color);
					}
				}
				else if (shader.name == "Standard (Specular setup) (Decalable)")
				{
					if (CanAtlasStandardMaterialSimpleOpaque(material, renderer, /*isSpecular*/ true) && CanAtlasTextureFilterMode(texture, material, renderer, FilterMode.Point))
					{
						group = standardSpecularSetupDecalableOpaque;
						textureConfiguration.color = material.GetColor(propertyID_Color);
					}
				}
				else if (shader.name == "Custom/Card")
				{
					group = batchableCard;
				}
				else if (shader.name == "Custom/Foliage")
				{
					if (CanAtlasTextureFilterMode(texture, material, renderer, FilterMode.Trilinear))
					{
						group = batchableFoliage;
					}
				}
			}

			if (group != null)
			{
				TextureUsers batchable = group.GetOrAddListForTexture(textureConfiguration);
				batchable.isGeneratedTexture = isGeneratedTexture;
				batchable.renderersUsingTexture.Add(renderer);
				return batchable;
			}
			else
			{
				return null;
			}
		}

		private void AddMesh(MeshFilter meshFilter, MeshRenderer meshRenderer)
		{
			TextureUsers batchable = AddMeshCommon(meshRenderer);
			if (batchable != null)
			{
				batchable.AddMeshFilter(meshFilter);
			}
		}

		private void AddSkinnedMesh(SkinnedMeshRenderer skinnedMeshRenderer)
		{
			TextureUsers batchable = AddMeshCommon(skinnedMeshRenderer);
			if (batchable != null)
			{
				batchable.AddSkinnedMeshRenderer(skinnedMeshRenderer);
			}
		}

		private bool CanBatchMesh(Mesh mesh, Renderer renderer)
		{
			if (mesh.isReadable)
			{
				return true;
			}

			if (shouldLogTextureAtlasExclusions && loggedMeshes.Add(mesh))
			{
				UnturnedLog.info($"Excluding mesh \"{mesh.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from level batching because it is not CPU-readable");
			}
			return false;
		}

		private bool CanAtlasTextureFilterMode(Texture2D texture, Material material, Renderer renderer, FilterMode requiredFilterMode)
		{
			if (texture.filterMode == requiredFilterMode)
			{
				return true;
			}
			else
			{
				if (shouldLogTextureAtlasExclusions && loggedTextures.Add(texture))
				{
					UnturnedLog.info($"Excluding texture \"{texture.name}\" in material \"{material.name}\" renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Filter Mode ({texture.filterMode}) is not {requiredFilterMode}");
				}
				return false;
			}
		}

		/// <summary>
		/// Most objects in Unturned use the standard shader without transparency/emissive/detail/etc.
		/// </summary>
		private bool CanAtlasStandardMaterialSimpleOpaque(Material material, Renderer renderer, bool isSpecular)
		{
			if (!Mathf.Approximately(material.GetFloat(propertyID_Mode), 0.0f))
			{
				if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
				{
					UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Mode is not Opaque");
				}
				return false;
			}

			if (isSpecular)
			{
				if (!material.GetColor(propertyID_SpecColor).IsNearlyBlack())
				{
					if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
					{
						UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Specular Color is not black");
					}
					return false;
				}

				if (material.IsKeywordEnabled("_SPECGLOSSMAP"))
				{
					if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
					{
						UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Specular Map is enabled");
					}
					return false;
				}
			}
			else
			{
				if (!Mathf.Approximately(material.GetFloat(propertyID_Metallic), 0.0f))
				{
					if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
					{
						UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Metallic is not zero");
					}
					return false;
				}

				if (material.IsKeywordEnabled("_METALLICGLOSSMAP"))
				{
					if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
					{
						UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Metallic Map is enabled");
					}
					return false;
				}
			}

			if (!Mathf.Approximately(material.GetFloat(propertyID_Glossiness), 0.0f)) // Smoothness
			{
				if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
				{
					UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Smoothness is not zero");
				}
				return false;
			}

			if (material.IsKeywordEnabled("_NORMALMAP"))
			{
				if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
				{
					UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Normal Map is enabled");
				}
				return false;
			}

			if (material.IsKeywordEnabled("_EMISSION"))
			{
				if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
				{
					UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Emission is enabled");
				}
				return false;
			}

			if (material.IsKeywordEnabled("_PARALLAXMAP"))
			{
				if (shouldLogTextureAtlasExclusions && loggedMaterials.Add(material))
				{
					UnturnedLog.info($"Excluding material \"{material.name}\" in renderer \"{renderer.GetSceneHierarchyPath()}\" from atlas because Parallax Map is enabled");
				}
				return false;
			}

			return true;
		}

		private Texture2D GetOrAddColorTexture(Material material)
		{
			Texture2D texture;
			if (!colorTextures.TryGetValue(material, out texture))
			{
				texture = new Texture2D(1, 1, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				texture.name = material.name + " (albedo texture for atlas)";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				texture.wrapMode = TextureWrapMode.Clamp;
				texture.filterMode = FilterMode.Point;
				texture.SetPixel(0, 0, material.GetColor(propertyID_Color));
				texture.Apply(/*updateMipmaps*/ false, /*makeNoLongerReadable*/ false);
				colorTextures.Add(material, texture);
			}
			return texture;
		}

		private void DestroyColorTextures()
		{
			foreach (KeyValuePair<Material, Texture2D> pair in colorTextures)
			{
				Object.Destroy(pair.Value);
			}
			colorTextures.Clear();
		}

		private void ApplyTextureAtlas(ShaderGroup group, bool shouldPreview)
		{
			Material materialTemplate = group.materialTemplate;
			Dictionary<UniqueTextureConfiguration, TextureUsers> batchableTextures = group.batchableTextures;

			UnturnedLog.info($"{batchableTextures.Count} texture(s) in {group.materialTemplate.shader.name} group");

			if (batchableTextures.Count > 0)
			{
				Texture2D atlas = new Texture2D(16, 16); // Initial dimensions don't matter.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				atlas.name = materialTemplate.shader.name + " texture atlas";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				atlas.wrapMode = TextureWrapMode.Clamp;
				atlas.filterMode = group.filterMode;

				// Unfortunately Texture2D.PackTextures requires the input textures to be CPU-readable,
				// so we duplicate them using a render texture. Edit 2023-02-15: this has the added benefit
				// of editing the texture e.g. baking in color setting.
				Texture2D[] duplicatedTextures = new Texture2D[batchableTextures.Count];
				TextureUsers[] textureUsers = new TextureUsers[batchableTextures.Count];
				RenderTexture previouslyActiveRenderTexture = RenderTexture.active;
				int copyFromDictionaryIndex = 0;
				foreach (KeyValuePair<UniqueTextureConfiguration, TextureUsers> pair in batchableTextures)
				{
					UniqueTextureConfiguration textureConfiguration = pair.Key;
					Texture2D original = textureConfiguration.texture;

					RenderTexture temp = RenderTexture.GetTemporary(original.width, original.height, /*depthBuffer*/ 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
					blitMaterial.SetColor(propertyID_Color, textureConfiguration.color);
					Graphics.Blit(original, temp, blitMaterial);
					RenderTexture.active = temp;

					Texture2D texture = new Texture2D(original.width, original.height, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					texture.name = original.name + " (copy for atlas)";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					texture.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);
					duplicatedTextures[copyFromDictionaryIndex] = texture;
					textureUsers[copyFromDictionaryIndex] = pair.Value;

					RenderTexture.ReleaseTemporary(temp);
					++copyFromDictionaryIndex;
				}
				RenderTexture.active = previouslyActiveRenderTexture;

				Rect[] rects = atlas.PackTextures(duplicatedTextures, /*padding*/ 0, /*maximumAtlasSize*/ 4096, /*makeNoLongerReadable*/ true);
				if (rects != null)
				{
					objectsToDestroy.Add(atlas);

					Material material = Object.Instantiate(materialTemplate);
					objectsToDestroy.Add(material);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					material.name = materialTemplate.shader.name + " material atlas";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					if (!shouldPreview)
					{
						material.mainTexture = atlas;
					}

					// Very slightly pad inward to help meshes with UVs exactly on the 0.0 or 1.0 border. (public issue #3731)
					Vector2 uvPositionOffset = atlas.texelSize * 0.001f;
					Vector2 uvSizeOffset = uvPositionOffset * 2.0f;

					for (int textureIndex = 0; textureIndex < textureUsers.Length; ++textureIndex)
					{
						TextureUsers batchable = textureUsers[textureIndex];

						foreach (KeyValuePair<Mesh, MeshUsers> pair in batchable.componentsUsingMesh)
						{
							Mesh originalMesh = pair.Key;
							Mesh copyMesh = Object.Instantiate(originalMesh);
							objectsToDestroy.Add(copyMesh);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
							copyMesh.name = originalMesh.name + " (copy for atlas)";
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
							uvs.Clear();
							copyMesh.GetUVs(0, uvs);
							if (batchable.isGeneratedTexture)
							{
								Rect packedRect = rects[textureIndex];
								Vector2 centerUV = new Vector2(packedRect.x + (0.5f * packedRect.width), packedRect.y + (0.5f * packedRect.height));
								for (int uvIndex = 0; uvIndex < uvs.Count; ++uvIndex)
								{
									uvs[uvIndex] = centerUV;
								}
							}
							else
							{
								if (shouldValidateUVs)
								{
									ValidateUVs(originalMesh, pair.Value);
								}

								for (int uvIndex = 0; uvIndex < uvs.Count; ++uvIndex)
								{
									Rect packedRect = rects[textureIndex];
									Vector2 uvCoord = uvs[uvIndex];
									uvCoord.x = packedRect.x + uvPositionOffset.x + (uvCoord.x * (packedRect.width - uvSizeOffset.x));
									uvCoord.y = packedRect.y + uvPositionOffset.y + (uvCoord.y * (packedRect.height - uvSizeOffset.y));
									uvs[uvIndex] = uvCoord;
								}
							}
							copyMesh.SetUVs(0, uvs, 0, uvs.Count);

							foreach (MeshFilter meshFilter in pair.Value.meshFilters)
							{
								meshFilter.sharedMesh = copyMesh;
							}

							foreach (SkinnedMeshRenderer skinnedMeshRenderer in pair.Value.skinnedMeshRenderers)
							{
								skinnedMeshRenderer.sharedMesh = copyMesh;
							}
						}

						foreach (Renderer renderer in batchable.renderersUsingTexture)
						{
							renderer.sharedMaterial = material;
						}
					}
				}
				else
				{
					UnturnedLog.warn($"Failed to pack textures for {materialTemplate.shader.name} atlas!");
					Object.Destroy(atlas);
				}

				foreach (Texture2D texture in duplicatedTextures)
				{
					Object.Destroy(texture);
				}
			}
		}

		private void ValidateUVs(Mesh originalMesh, MeshUsers meshUsers)
		{
			bool isOutsideBounds = false;
			foreach (Vector2 originalUv in uvs)
			{
				if (originalUv.x < 0.0f || originalUv.y < 0.0f || originalUv.x > 1.0f || originalUv.y > 1.0f)
				{
					isOutsideBounds = true;
					break;
				}
			}
			if (isOutsideBounds && loggedMeshes.Add(originalMesh))
			{
				Component exampleComponent = meshUsers.meshFilters.HeadOrDefault();
				if (exampleComponent == null)
				{
					exampleComponent = meshUsers.skinnedMeshRenderers.HeadOrDefault();
				}
				UnturnedLog.error($"Mesh \"{originalMesh.name}\" in renderer \"{exampleComponent?.GetSceneHierarchyPath()}\" has UVs outside [0, 1] range (should be excluded from level batching)");
			}
		}

		internal static LevelBatching instance;

		private int propertyID_MainTex = Shader.PropertyToID("_MainTex");
		private int propertyID_Mode = Shader.PropertyToID("_Mode");
		private int propertyID_Color = Shader.PropertyToID("_Color");
		private int propertyID_SpecColor = Shader.PropertyToID("_SpecColor");
		private int propertyID_Metallic = Shader.PropertyToID("_Metallic");
		private int propertyID_Glossiness = Shader.PropertyToID("_Glossiness");

		private int maxTextureSize;

		/// <summary>
		/// Meshes we logged an explanation for as to why they can't be atlased.
		/// </summary>
		private HashSet<Mesh> loggedMeshes = new HashSet<Mesh>();

		/// <summary>
		/// Textures we logged an explanation for as to why they can't be atlased.
		/// </summary>
		private HashSet<Texture2D> loggedTextures = new HashSet<Texture2D>();

		/// <summary>
		/// Materials we logged an explanation for as to why they can't be atlased.
		/// </summary>
		private HashSet<Material> loggedMaterials = new HashSet<Material>();

		private List<MeshRenderer> staticBatchingMeshRenderers = new List<MeshRenderer>();

		/// <summary>
		/// Objects instantiated for the lifetime of the level that should be destroyed when exiting the level.
		/// </summary>
		private List<Object> objectsToDestroy = new List<Object>();

		/// <summary>
		/// If true, don't assign texture atlas to material so batched materials are obvious.
		/// </summary>
		private CommandLineFlag wantsToPreviewTextureAtlas = new CommandLineFlag(false, "-PreviewLevelBatchingTextureAtlas");

		/// <summary>
		/// If true, assign a red material to excluded meshes so they are obvious.
		/// </summary>
		private CommandLineFlag wantsToPreviewMeshExclusions = new CommandLineFlag(false, "-PreviewLevelBatchingMeshExclusions");

		/// <summary>
		/// If true, replace each unique material with a colored one before static batching.
		/// </summary>
		private CommandLineFlag wantsToPreviewUniqueMaterials = new CommandLineFlag(false, "-PreviewLevelBatchingUniqueMaterials");

		/// <summary>
		/// If true, log why texture/material can't be included in atlas.
		/// </summary>
		private CommandLineFlag shouldLogTextureAtlasExclusions = new CommandLineFlag(false, "-LogLevelBatchingTextureAtlasExclusions");

		/// <summary>
		/// If true, log if mesh has UVs outside [0, 1] range.
		/// </summary>
		private CommandLineFlag shouldValidateUVs = new CommandLineFlag(false, "-ValidateLevelBatchingUVs");

		/// <summary>
		/// We generate a 1x1 texture for materials without one.
		/// </summary>
		private Dictionary<Material, Texture2D> colorTextures = new Dictionary<Material, Texture2D>();

		/// <summary>
		/// Tracks which mesh filters and skinned mesh renderers were referencing a given mesh.
		/// </summary>
		private class MeshUsers
		{
			public List<MeshFilter> meshFilters = new List<MeshFilter>();
			public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
		}

		private struct UniqueTextureConfiguration : System.IEquatable<UniqueTextureConfiguration>
		{
			public Texture2D texture;
			public Color color;

			public bool Equals(UniqueTextureConfiguration other)
			{
				return texture == other.texture && color == other.color;
			}

			public override int GetHashCode()
			{
				return texture.GetHashCode() ^ color.GetHashCode();
			}
		}

		/// <summary>
		/// Tracks which meshes and materials were referencing a given texture.
		/// </summary>
		private class TextureUsers
		{
			/// <summary>
			/// If true, UVs should be centered and overridden because original mesh was not textured. 
			/// </summary>
			public bool isGeneratedTexture;

			/// <summary>
			/// Maps original mesh to any mesh filters using it.
			/// When mesh's UVs are modified the mesh filters need to be pointed at the copied mesh.
			/// </summary>
			public Dictionary<Mesh, MeshUsers> componentsUsingMesh = new Dictionary<Mesh, MeshUsers>();

			/// <summary>
			/// Renderers with a material using the texture.
			/// After combining texture the renderers need to be pointed at the combined material.
			/// </summary>
			public List<Renderer> renderersUsingTexture = new List<Renderer>();

			public void AddMeshFilter(MeshFilter meshFilter)
			{
				GetOrAddListForMesh(meshFilter.sharedMesh).meshFilters.Add(meshFilter);
			}

			public void AddSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
			{
				GetOrAddListForMesh(skinnedMeshRenderer.sharedMesh).skinnedMeshRenderers.Add(skinnedMeshRenderer);
			}

			private MeshUsers GetOrAddListForMesh(Mesh mesh)
			{
				MeshUsers meshUsers;
				if (!componentsUsingMesh.TryGetValue(mesh, out meshUsers))
				{
					meshUsers = new MeshUsers();
					componentsUsingMesh.Add(mesh, meshUsers);
				}
				return meshUsers;
			}
		}

		/// <summary>
		/// Tracks which textures were referencing a given shader.
		/// </summary>
		private class ShaderGroup
		{
			public Material materialTemplate;
			public Dictionary<UniqueTextureConfiguration, TextureUsers> batchableTextures = new Dictionary<UniqueTextureConfiguration, TextureUsers>();
			public FilterMode filterMode = FilterMode.Point;

			public TextureUsers GetOrAddListForTexture(UniqueTextureConfiguration texture)
			{
				TextureUsers textureUsers;
				if (!batchableTextures.TryGetValue(texture, out textureUsers))
				{
					textureUsers = new TextureUsers();
					batchableTextures.Add(texture, textureUsers);
				}
				return textureUsers;
			}

			public void Clear()
			{
				batchableTextures.Clear();
			}
		}

		/// <summary>
		/// StaticBatchingUtility.Combine requires input renderers are enabled and active in hierarchy,
		/// so we temporarily activate/enable them to keep this logic out of LevelObject/ResourceSpawnpoint.
		/// </summary>
		private struct StaticBatchingInitialState
		{
			public Transform parent;
			public bool wasEnabled;
			public bool wasActive;
		}
	}
}
