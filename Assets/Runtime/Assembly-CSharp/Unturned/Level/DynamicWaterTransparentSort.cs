////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR || DEVELOPMENT_BUILD
// #define LOG_DYNAMIC_WATER_TRANSPARENT_SORT_REGISTRATIONS
#endif
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	/// <summary>
	/// Manages render queue for transparent materials on non-stationary objects.
	/// Updates one material per frame.
	/// </summary>
	public class DynamicWaterTransparentSort : MonoBehaviour
	{
		public static DynamicWaterTransparentSort Get()
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("DynamicWaterTransparentSort");
				DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				instance = gameObject.AddComponent<DynamicWaterTransparentSort>();
			}

			return instance;
		}

		public object Register(Transform transform, Material material)
		{
			if (transform == null || material == null)
				return null;

#if LOG_DYNAMIC_WATER_TRANSPARENT_SORT_REGISTRATIONS
			UnturnedLog.info($"Register {transform.GetSceneHierarchyPath()}.{material.name}");
#endif
			TransparentObject transparentObject = new TransparentObject(transform, material);
			transparentObject.UpdatePosition();
			transparentObject.UpdateRenderQueue(LevelLighting.isSea);
			managedObjects.Add(transparentObject);
			return transparentObject; // Used as a handle.
		}

		public void Unregister(object handle)
		{
			if (handle == null)
				return;

			for (int index = managedObjects.Count - 1; index >= 0; --index)
			{
				if (managedObjects[index] == handle)
				{
#if LOG_DYNAMIC_WATER_TRANSPARENT_SORT_REGISTRATIONS
					UnturnedLog.info($"Unregister {managedObjects[index].transform?.GetSceneHierarchyPath()}.{managedObjects[index].material?.name}");
#endif
					managedObjects.RemoveAtFast(index);
					return;
				}
			}
		}

		/// <summary>
		/// Callback when camera above/under water changes.
		/// </summary>
		private void HandleIsSeaChanged(bool isSea)
		{
			for (int index = managedObjects.Count - 1; index >= 0; --index)
			{
				TransparentObject updateObject = managedObjects[index];
				if (updateObject.IsValid)
				{
					updateObject.UpdateRenderQueue(isSea);
				}
				else
				{
					managedObjects.RemoveAtFast(index);
				}
			}
		}

		private void Start()
		{
			LevelLighting.isSeaChanged += HandleIsSeaChanged;
			CommandLogMemoryUsage.OnExecuted += OnLogMemoryUsage;
		}

		private void OnDestroy()
		{
			LevelLighting.isSeaChanged -= HandleIsSeaChanged;
			CommandLogMemoryUsage.OnExecuted -= OnLogMemoryUsage;
		}

		private void OnLogMemoryUsage(List<string> results)
		{
			results.Add($"Water transparent sort managed objects: {managedObjects?.Count}");
		}

		private void Update()
		{
			if (managedObjects.Count < 1)
			{
				return;
			}

			++updateIndex;
			if (updateIndex >= managedObjects.Count)
			{
				updateIndex = 0;
			}

			TransparentObject updateObject = managedObjects[updateIndex];
			if (updateObject.IsValid)
			{
				updateObject.UpdatePosition();
				updateObject.UpdateRenderQueue(LevelLighting.isSea);
			}
			else
			{
				managedObjects.RemoveAtFast(updateIndex);
			}
		}

		private class TransparentObject
		{
			public TransparentObject(Transform transform, Material material)
			{
				this.transform = transform;
				this.material = material;
			}

			public bool IsValid => transform != null && material != null;

			public void UpdatePosition()
			{
				wasTransformUnderwater = SDG.Framework.Water.WaterUtility.isPointUnderwater(transform.position);
			}

			public void UpdateRenderQueue(bool isCameraUnderwater)
			{
				if (wasTransformUnderwater)
				{
					if (isCameraUnderwater)
					{
						material.renderQueue = 3100; // render after water
					}
					else
					{
						material.renderQueue = 2900; // render before water
					}
				}
				else
				{
					if (isCameraUnderwater)
					{
						material.renderQueue = 2900; // render before water
					}
					else
					{
						material.renderQueue = 3100; // render after water
					}
				}
			}

			public Transform transform;
			public Material material;
			public bool wasTransformUnderwater;
		}

		private int updateIndex = 0;
		private List<TransparentObject> managedObjects = new List<TransparentObject>();
		private static DynamicWaterTransparentSort instance;
	}
}
