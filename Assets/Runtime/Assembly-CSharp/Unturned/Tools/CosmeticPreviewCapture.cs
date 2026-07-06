////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
//#define LOG_BOUNDS
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unturned.SystemEx;
using Unturned.UnityEx;

namespace SDG.Unturned
{
	public class CosmeticPreviewCapture : MonoBehaviour
	{
		public Camera shirtCamera;
		public Camera pantsCamera;
		public Camera backpackCamera;
		public Camera vestCamera;
		public Camera hatCamera;
		public Camera outfitCamera;

		public void CaptureCosmetics()
		{
			StartCoroutine(CaptureAllCosmeticsCoroutine());
		}

		public void CaptureOutfit(System.Guid guid)
		{
			OutfitAsset asset = Assets.find(guid) as OutfitAsset;
			if (asset != null)
			{
				List<OutfitAsset> outfitAssets = new List<OutfitAsset>() { asset };
				StartCoroutine(CaptureOutfitsCoroutine(outfitAssets));
			}
			else
			{
				UnturnedLog.warn($"Unable to find outfit matching {guid:N} ({Assets.find(guid)?.name})");
			}
		}

		public void CaptureAllOutfits()
		{
			List<OutfitAsset> outfitAssets = new List<OutfitAsset>();
			Assets.find(outfitAssets);
			StartCoroutine(CaptureOutfitsCoroutine(outfitAssets));
		}

		private IEnumerator CaptureAllCosmeticsCoroutine()
		{
			yield return RenderDefaultCharacter();

			string dirPath2048 = PathEx.Join(UnturnedPaths.RootDirectory, "Extras", "CosmeticPreviews_2048x2048");
			string dirPath400 = PathEx.Join(UnturnedPaths.RootDirectory, "Extras", "CosmeticPreviews_400x400");
			string dirPath200 = PathEx.Join(UnturnedPaths.RootDirectory, "Extras", "CosmeticPreviews_200x200");
#if UNITY_EDITOR
			string resourcesPath = PathEx.Join(UnityPaths.AssetsDirectory, "Resources", "Economy", "CosmeticPreviews");
#endif // UNITY_EDITOR

			List<ItemAsset> itemAssets = new List<ItemAsset>();
			Assets.find(itemAssets);
			foreach (ItemAsset itemAsset in itemAssets)
			{
				if (!itemAsset.isPro)
					continue;

				if (itemAsset.type != EItemType.SHIRT &&
					itemAsset.type != EItemType.PANTS &&
					itemAsset.type != EItemType.HAT &&
					itemAsset.type != EItemType.BACKPACK &&
					itemAsset.type != EItemType.VEST &&
					itemAsset.type != EItemType.GLASSES &&
					itemAsset.type != EItemType.MASK)
				{
					continue;
				}

				string filePath2048 = Path.Combine(dirPath2048, itemAsset.GUID.ToString("N") + ".png");
				string filePath400 = Path.Combine(dirPath400, itemAsset.GUID.ToString("N") + ".png");
				string filePath200 = Path.Combine(dirPath200, itemAsset.GUID.ToString("N") + ".png");

				bool alreadyExists = File.Exists(filePath2048) && File.Exists(filePath400) && File.Exists(filePath200);

#if UNITY_EDITOR
				string resourcesFilePath = Path.Combine(resourcesPath, itemAsset.GUID.ToString("N") + ".png");
				alreadyExists &= File.Exists(resourcesFilePath);
#endif // UNITY_EDITOR

				if (alreadyExists)
				{
					// Avoid unnecessarily recapturing saves time and reduces both redundant uploads to CDN and redundant commits.
					continue;
				}

				UnturnedLog.info($"Capture cosmetic {itemAsset.FriendlyName} ({itemAsset.GUID:N})");

				ResetOutfit();
				ApplyItemToOutfit(itemAsset);
				clothes.apply();

				Camera itemCamera = GetCamera(itemAsset);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (clothes.overrideMaskMythicId > 0)
				{
					// Minor delay to ensure mythic particle effects have time to animate.
					yield return new WaitForSeconds(1.0f);

					Transform effectHook = clothes.maskModel?.Find("Effect");
					MythicalEffectController mythicalInstance = effectHook?.GetComponent<MythicalEffectController>();
					if (mythicalInstance?.systemTransform != null)
					{
						Bounds mythicBounds = GetWorldBounds(mythicalInstance.systemTransform.gameObject);
						FitCameraToBounds(itemCamera, mythicBounds);
					}
				}
				else
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
				{
					itemCamera = GetCamera(itemAsset);

					GameObject clothingGameObject = GetClothingGameObject(itemAsset);
					if (clothingGameObject != null)
					{
						Bounds clothingBounds = GetWorldBounds(clothingGameObject);
						FitCameraToBounds(itemCamera, clothingBounds);
					}
				}

				yield return Render(itemCamera, targetTexture4096, downsampleTexture2048, exportTexture2048, filePath2048);
				yield return Render(itemCamera, targetTexture800, downsampleTexture400, exportTexture400, filePath400);
				yield return Render(itemCamera, targetTexture400, downsampleTexture200, exportTexture200, filePath200);

#if UNITY_EDITOR
				File.Copy(filePath400, resourcesFilePath, /*overwrite*/ true);
#endif // UNITY_EDITOR
			}
		}

		private IEnumerator CaptureOutfitsCoroutine(List<OutfitAsset> outfitAssets)
		{
			yield return RenderDefaultCharacter();

			string dirPath2048 = PathEx.Join(UnturnedPaths.RootDirectory, "Extras", "OutfitPreviews_2048x2048");
			string dirPath400 = PathEx.Join(UnturnedPaths.RootDirectory, "Extras", "OutfitPreviews_400x400");
			string dirPath200 = PathEx.Join(UnturnedPaths.RootDirectory, "Extras", "OutfitPreviews_200x200");

			foreach (OutfitAsset outfit in outfitAssets)
			{
				ResetOutfit();
				foreach (AssetReference<ItemAsset> itemAssetRef in outfit.itemAssets)
				{
					ItemAsset item = itemAssetRef.Find();
					if (item == null)
					{
						UnturnedLog.warn($"Missing item {itemAssetRef} for outfit {outfit}");
						continue;
					}

					ApplyItemToOutfit(item);
				}

				clothes.apply();

				Bounds outfitBounds = new Bounds(transform.position + new Vector3(0.0f, 0.95f, 0.0f), new Vector3(0.1f, 1.9f, 0.1f));

				if (clothes.hatModel != null)
					outfitBounds.Encapsulate(GetWorldBounds(clothes.hatModel.gameObject));

				if (clothes.backpackModel != null)
					outfitBounds.Encapsulate(GetWorldBounds(clothes.backpackModel.gameObject));

				if (clothes.vestModel != null)
					outfitBounds.Encapsulate(GetWorldBounds(clothes.vestModel.gameObject));

				if (clothes.glassesModel != null)
					outfitBounds.Encapsulate(GetWorldBounds(clothes.glassesModel.gameObject));

				if (clothes.maskModel != null)
					outfitBounds.Encapsulate(GetWorldBounds(clothes.maskModel.gameObject));

				// Shrink because models tend to exaggerate the size.
				outfitBounds.Expand(-0.2f);

				FitCameraToBounds(outfitCamera, outfitBounds);

				string filePath2048 = Path.Combine(dirPath2048, outfit.GUID.ToString("N") + ".png");
				string filePath400 = Path.Combine(dirPath400, outfit.GUID.ToString("N") + ".png");
				string filePath200 = Path.Combine(dirPath200, outfit.GUID.ToString("N") + ".png");
				yield return Render(outfitCamera, targetTexture4096, downsampleTexture2048, exportTexture2048, filePath2048);
				yield return Render(outfitCamera, targetTexture800, downsampleTexture400, exportTexture400, filePath400);
				yield return Render(outfitCamera, targetTexture400, downsampleTexture200, exportTexture200, filePath200);
			}
		}

		private void ResetOutfit()
		{
			clothes.shirtGuid = System.Guid.Empty;
			clothes.pantsGuid = System.Guid.Empty;
			clothes.hatGuid = System.Guid.Empty;
			clothes.backpackGuid = System.Guid.Empty;
			clothes.vestGuid = System.Guid.Empty;
			clothes.glassesGuid = System.Guid.Empty;
			clothes.maskGuid = System.Guid.Empty;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			clothes.overrideMaskMythicId = 0;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
		}

		private void ApplyItemToOutfit(ItemAsset itemAsset)
		{
			switch (itemAsset.type)
			{
				case EItemType.SHIRT:
					clothes.shirtGuid = itemAsset.GUID;
					break;
				case EItemType.PANTS:
					clothes.pantsGuid = itemAsset.GUID;
					break;
				case EItemType.HAT:
					clothes.hatGuid = itemAsset.GUID;
					break;
				case EItemType.BACKPACK:
					clothes.backpackGuid = itemAsset.GUID;
					break;
				case EItemType.VEST:
					clothes.vestGuid = itemAsset.GUID;
					break;
				case EItemType.GLASSES:
					clothes.glassesGuid = itemAsset.GUID;
					break;
				case EItemType.MASK:
					clothes.maskGuid = itemAsset.GUID;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					clothes.overrideMaskMythicId = ((ItemMaskAsset) itemAsset).cosmeticPreviewMythicId;
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
					break;
			}
		}

		private GameObject GetClothingGameObject(ItemAsset itemAsset)
		{
			switch (itemAsset.type)
			{
				case EItemType.HAT:
					return clothes.hatModel?.gameObject;
				case EItemType.BACKPACK:
					return clothes.backpackModel?.gameObject;
				case EItemType.VEST:
					return clothes.vestModel?.gameObject;
				case EItemType.GLASSES:
					return clothes.glassesModel?.gameObject;
				case EItemType.MASK:
					return clothes.maskModel?.gameObject;
			}

			return null;
		}

		private Camera GetCamera(ItemAsset itemAsset)
		{
			switch (itemAsset.type)
			{
				case EItemType.SHIRT:
					return shirtCamera;
				case EItemType.PANTS:
					return pantsCamera;
				case EItemType.HAT:
					return hatCamera;
				case EItemType.BACKPACK:
					return backpackCamera;
				case EItemType.VEST:
					return vestCamera;
				case EItemType.GLASSES:
					return hatCamera;
				case EItemType.MASK:
					return hatCamera;
			}

			return null;
		}

		/// <summary>
		/// Render character with hair and skin otherwise it might be cyan.
		/// (public issue #3615)
		/// </summary>
		private IEnumerator RenderDefaultCharacter()
		{
			yield return new WaitForEndOfFrame();

			clothes.hair = 1;
			clothes.apply();

			outfitCamera.targetTexture = targetTexture400;
			outfitCamera.Render();

			outfitCamera.targetTexture = null;

			clothes.hair = 0;

			// 2023-07-06: still getting that weird cyan character, trying a longer delay...
			yield return new WaitForSeconds(1.0f);
		}

		private IEnumerator Render(Camera cameraComponent, RenderTexture targetTexture, RenderTexture downsampleTexture, Texture2D exportTexture, string exportFilePath)
		{
			yield return new WaitForEndOfFrame();

			cameraComponent.targetTexture = targetTexture;
			cameraComponent.Render();
			cameraComponent.targetTexture = null;

			Graphics.Blit(targetTexture, downsampleTexture);

			// Copy rendered data from GPU to CPU texture.
			RenderTexture.active = downsampleTexture;
			exportTexture.ReadPixels(new Rect(0, 0, downsampleTexture.width, downsampleTexture.height), 0, 0);
			//exportTexture.Apply(/*updateMipmaps*/ false, /*makeNoLongerReadable*/ false);
			RenderTexture.active = null;

			byte[] exportData = exportTexture.EncodeToPNG();
			File.WriteAllBytes(exportFilePath, exportData);
		}

		private void OnEnable()
		{
			clothes = GetComponent<HumanClothes>();
			clothes.skin = new Color32(210, 210, 210, byte.MaxValue); // in-game mannequin color
			clothes.color = new Color32(175, 175, 175, byte.MaxValue); // hair color
			clothes.BeardColor = clothes.color;
			clothes.isCosmeticPreview = true;
			clothes.ShouldHairOverridesUseFallbackColor = true;

			targetTexture4096 = RenderTexture.GetTemporary(4096, 4096, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			targetTexture4096.filterMode = FilterMode.Bilinear;
			targetTexture800 = RenderTexture.GetTemporary(800, 800, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			targetTexture800.filterMode = FilterMode.Bilinear;
			targetTexture400 = RenderTexture.GetTemporary(400, 400, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			targetTexture400.filterMode = FilterMode.Bilinear;

			downsampleTexture2048 = RenderTexture.GetTemporary(2048, 2048, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			downsampleTexture400 = RenderTexture.GetTemporary(400, 400, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			downsampleTexture200 = RenderTexture.GetTemporary(200, 200, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

			exportTexture2048 = new Texture2D(2048, 2048, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ false);
			exportTexture400 = new Texture2D(400, 400, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ false);
			exportTexture200 = new Texture2D(200, 200, TextureFormat.ARGB32, /*mipChain*/ false, /*linear*/ false);

			GetComponent<Animation>()["Idle_Stand"].speed = 0.0f;
			GetComponent<Animation>()["Idle_Stand"].normalizedTime = 0.2f;
		}

		private void OnDisable()
		{
			RenderTexture.ReleaseTemporary(targetTexture4096);
			RenderTexture.ReleaseTemporary(targetTexture800);
			RenderTexture.ReleaseTemporary(targetTexture400);
			RenderTexture.ReleaseTemporary(downsampleTexture2048);
			RenderTexture.ReleaseTemporary(downsampleTexture400);
			RenderTexture.ReleaseTemporary(downsampleTexture200);
			Destroy(exportTexture2048);
			Destroy(exportTexture400);
			Destroy(exportTexture200);
		}

		private List<Renderer> renderers = new List<Renderer>();
		private Bounds GetWorldBounds(GameObject parent)
		{
#if LOG_BOUNDS
			UnturnedLog.info($"Bounds get for {parent.GetSceneHierarchyPath()}");
#endif

			Bounds bounds = new Bounds();
			bool hasBounds = false;

			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1024];

			parent.GetComponentsInChildren(renderers);
			foreach (Renderer component in renderers)
			{
				if (component is ParticleSystemRenderer particleSystemRenderer)
				{
					ParticleSystem particleSystem = particleSystemRenderer.GetComponent<ParticleSystem>();
					Transform particleSpace = null;
					switch (particleSystem.main.simulationSpace)
					{
						case ParticleSystemSimulationSpace.Local:
							particleSpace = particleSystem.transform;
							break;
						case ParticleSystemSimulationSpace.World:
							particleSpace = null;
							break;
						case ParticleSystemSimulationSpace.Custom:
							particleSpace = particleSystem.main.customSimulationSpace;
							break;
					}
					int particleCount = particleSystem.GetParticles(particles);
					for (int particleIndex = 0; particleIndex < particleCount; ++particleIndex)
					{
						ParticleSystem.Particle particle = particles[particleIndex];
						Vector3 worldPosition = particleSpace != null ? particleSpace.TransformPoint(particle.position) : particle.position;
						if (hasBounds)
						{
							bounds.Encapsulate(new Bounds(worldPosition, new Vector3(0.1f, 0.1f, 0.1f)));
#if LOG_BOUNDS
							UnturnedLog.info($"Bounds encapsulating particle {particleIndex} at {worldPosition} from {component.GetSceneHierarchyPath()}");
#endif
						}
						else
						{
							bounds = new Bounds(worldPosition, Vector3.zero);
#if LOG_BOUNDS
							UnturnedLog.info($"Bounds initialized with particle 0 at {worldPosition} from {component.GetSceneHierarchyPath()}");
#endif
							hasBounds = true;
						}
					}
				}
				else if (component is MeshRenderer || component is SkinnedMeshRenderer)
				{
					if (hasBounds)
					{
#if LOG_BOUNDS
						UnturnedLog.info($"Bounds encapsulating {component.bounds} from {component.GetSceneHierarchyPath()}");
#endif
						bounds.Encapsulate(component.bounds);
					}
					else
					{
#if LOG_BOUNDS
						UnturnedLog.info($"Bounds initialized with {component.bounds} from {component.GetSceneHierarchyPath()}");
#endif
						bounds = component.bounds;
						hasBounds = true;
					}
				}
			}

#if LOG_BOUNDS
			UnturnedLog.info($"Bounds calculated {bounds} for {parent.GetSceneHierarchyPath()}");
#endif

			if (bounds.size.magnitude > 64.0f)
			{
				UnturnedLog.warn($"CosmeticPreviewCapture bounds {bounds} for {parent.GetSceneHierarchyPath()} are likely too big");
			}

			return bounds;
		}

		private void FitCameraToBounds(Camera cameraComponent, Bounds worldBounds)
		{
			float sphereRadius = worldBounds.extents.magnitude;
			float halfCamFovRadians = cameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad;
			// We have a right angle triangle where:
			// - The adjacent edge is the camera frustum.
			// - The opposite edge is tangent to the camera frustrum toward the sphere center.
			// - The hypotenuse is the line directly from the camera to the center of bounds.
			// We know the angle and the opposite edge length, so can calculate the hypotenuse with h = o/sin
			float minDistanceFromCameraToCenter = sphereRadius / Mathf.Sin(halfCamFovRadians);

			// Some items like Rudolph's red nose or the monocle have small bounds,
			// so we back off the camera a bit to give more context of their placement.
			const float MIN_DISTANCE = 0.55f;
			minDistanceFromCameraToCenter = Mathf.Max(MIN_DISTANCE, minDistanceFromCameraToCenter);

			float distanceFromCameraToCenter = minDistanceFromCameraToCenter + cameraComponent.nearClipPlane;
			cameraComponent.transform.position = worldBounds.center - (cameraComponent.transform.forward * distanceFromCameraToCenter);
		}

		private HumanClothes clothes;
		private RenderTexture targetTexture4096;
		private RenderTexture targetTexture800;
		private RenderTexture targetTexture400;
		private RenderTexture downsampleTexture2048;
		private RenderTexture downsampleTexture400;
		private RenderTexture downsampleTexture200;
		private Texture2D exportTexture2048;
		private Texture2D exportTexture400;
		private Texture2D exportTexture200;
	}
}
