////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
// #define LOG_CUSTOM_LOD

using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	internal class LODGroupManager
	{
		public static LODGroupManager Get()
		{
			return instance;
		}

		public void Register(LODGroupAdditionalData component)
		{
			if (component.LODBiasOverride == LODGroupAdditionalData.ELODBiasOverride.None)
				return;

			LODGroup unityComponent = component.GetComponent<LODGroup>();
			if (unityComponent == null)
			{
				UnturnedLog.warn("Additional Data without LOD Group: {0}", component.GetSceneHierarchyPath());
				return;
			}

			ComponentData data = components.AddDefaulted();
			data.extensionComponent = component;
			data.unityComponent = unityComponent;
			data.originalLODs = data.unityComponent.GetLODs();
			data.modifiedLODs = data.unityComponent.GetLODs(); // Unfortunately we need a deep copy for renderers list.
			UpdateComponent(data);
		}

		public void Unregister(LODGroupAdditionalData component)
		{
			for (int index = components.Count - 1; index >= 0; --index)
			{
				if (components[index].extensionComponent == component)
				{
					components.RemoveAtFast(index);
					return;
				}
			}
		}

		/// <summary>
		/// Called after lod bias may have changed.
		/// </summary>
		public void SynchronizeLODBias()
		{
			float lodBias = QualitySettings.lodBias;
			if (MathfEx.IsNearlyEqual(cachedLODBias, lodBias))
				return;

			cachedLODBias = lodBias;
			foreach (ComponentData data in components)
			{
				UpdateComponent(data);
			}
		}

		private void UpdateComponent(ComponentData data)
		{
			for (int lodIndex = 0; lodIndex < data.originalLODs.Length; ++lodIndex)
			{
				ref LOD originalLOD = ref data.originalLODs[lodIndex];
				ref LOD modifiedLOD = ref data.modifiedLODs[lodIndex];
				modifiedLOD.screenRelativeTransitionHeight = originalLOD.screenRelativeTransitionHeight * cachedLODBias;
			}

			// Workaround because SetLODs logs an error if LOD1 goes past 100%.
			const float lodPadding = 0.001f;
			for (int lodIndex = 1; lodIndex < data.originalLODs.Length; ++lodIndex)
			{
				ref LOD higherLOD = ref data.modifiedLODs[lodIndex - 1];
				ref LOD lowerLOD = ref data.modifiedLODs[lodIndex];
				lowerLOD.screenRelativeTransitionHeight = MathfEx.Min(1.0f - lodPadding,
					lowerLOD.screenRelativeTransitionHeight,
					higherLOD.screenRelativeTransitionHeight - lodPadding);
			}

#if LOG_CUSTOM_LOD
			for(int lodIndex = 0; lodIndex < data.originalLODs.Length; ++lodIndex)
			{
				ref LOD originalLOD = ref data.originalLODs[lodIndex];
				ref LOD modifiedLOD = ref data.modifiedLODs[lodIndex];
				UnturnedLog.info("{0} LOD{1} {2}:{3}",
					data.extensionComponent.GetSceneHierarchyPath(),
					lodIndex,
					originalLOD.screenRelativeTransitionHeight,
					modifiedLOD.screenRelativeTransitionHeight);
			}
#endif // LOG_CUSTOM_LOD

			data.unityComponent.SetLODs(data.modifiedLODs);
		}

		private class ComponentData
		{
			public LODGroupAdditionalData extensionComponent;
			public LODGroup unityComponent;
			public LOD[] originalLODs;
			public LOD[] modifiedLODs;
		}

		private static LODGroupManager instance = new LODGroupManager();
		private List<ComponentData> components = new List<ComponentData>();
		private float cachedLODBias = 1.0f;
	}
}
