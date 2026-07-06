////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class CullingVolumeManager : VolumeManager<CullingVolume, CullingVolumeManager>
	{
#if !DEDICATED_SERVER
		protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
		{
			base.OnUpdateGizmos(runtimeGizmos);

			gizmoLabelSampler.Begin();
			foreach (CullingVolume volume in volumesWithObjects)
			{
				Color color = volume.isCulled ? Color.red : Color.green;
				runtimeGizmos.Label(volume.transform.position, volume.objects.Count.ToString(), color);
			}
			gizmoLabelSampler.End();
		}

		public CullingVolumeManager()
		{
			FriendlyName = "Manual Object Culling";
			SetDebugColor(new Color32(150, 30, 150, 255));
			SDG.Framework.Utilities.TimeUtility.updated += OnUpdateCullingVolumes;
		}

		/// <summary>
		/// Called by navmesh baking to complete pending object changes that may affect which nav objects are enabled.
		/// </summary>
		internal void ImmediatelySyncAllVolumes()
		{
			wasViewTeleported = true;
			OnUpdateCullingVolumes();
		}

		internal void ClearOverlappingObjects()
		{
			foreach (CullingVolume volume in allVolumes)
			{
				if (volume.objects != null && volume.objects.Count > 0)
				{
					volume.ClearObjects();
				}
			}
		}

		internal void RefreshOverlappingObjects()
		{
			foreach (CullingVolume volume in allVolumes)
			{
				volume.FindObjectsInsideVolume();
			}
		}

		internal void AddVolumeWithObjects(CullingVolume volume)
		{
			volumesWithObjects.Add(volume);
		}

		internal void RemoveVolumeWithObjects(CullingVolume volume)
		{
			volumesWithObjects.RemoveFast(volume);
			volumesWithVisibilityUpdates.Remove(volume);
		}

		internal void OnPlayerTeleported()
		{
			wasViewTeleported = true;
		}

		/// <summary>
		/// Hide culling volume by default because new mappers might wonder what these purple boxes
		/// are and why their number goes away after moving objects.
		/// </summary>
		protected override ELevelVolumeVisibility DefaultVisibility => ELevelVolumeVisibility.Hidden;

		/// <summary>
		/// Check a fixed number of volumes for visibility updates per frame.
		/// </summary>
		private void UpdateRelevantCullingVolumes()
		{
			bool forceCull = Level.isEditor && EditorVolumesUI.EditorWantsToPreviewCulling;
			Vector3 viewPosition = MainCamera.RenderingPosition;

			// If view was just teleported we update all volumes rather than a subset.
			int cullingUpdatesPerFrame = wasViewTeleported ? volumesWithObjects.Count : Mathf.Min(32, volumesWithObjects.Count);

			for (int perFrameUpdateIndex = 0; perFrameUpdateIndex < cullingUpdatesPerFrame; ++perFrameUpdateIndex)
			{
				++cullingUpdateIndex;
				if (cullingUpdateIndex >= volumesWithObjects.Count)
				{
					cullingUpdateIndex = 0;
				}

				CullingVolume volume = volumesWithObjects[cullingUpdateIndex];
				bool changed = volume.UpdateCulling(viewPosition, forceCull);
				if (changed)
				{
					volumesWithVisibilityUpdates.Add(volume);
				}
			}
		}

		private void SyncAllVolumesVisibility()
		{
			foreach (CullingVolume volume in volumesWithVisibilityUpdates)
			{
				volume.SyncAllObjectsVisibility();
			}

			volumesWithVisibilityUpdates.Clear();
			volumesToRemoveFromUpdatesList.Clear();
		}

		/// <summary>
		/// Any volumes in the process of enabling/disabling get updated once per frame.
		/// </summary>
		private void UpdateObjectsVisibility()
		{
			foreach (CullingVolume volume in volumesWithVisibilityUpdates)
			{
				volume.UpdateObjectsVisibility();
				if (!volume.HasPendingVisibilityUpdates)
				{
					volumesToRemoveFromUpdatesList.Add(volume);
				}
			}
			volumesWithVisibilityUpdates.ExceptWith(volumesToRemoveFromUpdatesList);
			volumesToRemoveFromUpdatesList.Clear();
		}

		private void OnUpdateCullingVolumes()
		{
			if (MainCamera.instance == null)
				return;

			relevanceSampler.Begin();
			UpdateRelevantCullingVolumes();
			relevanceSampler.End();

			if (wasViewTeleported)
			{
				SyncAllVolumesVisibility();
			}
			else
			{
				visibilitySampler.Begin();
				UpdateObjectsVisibility();
				visibilitySampler.End();
			}

			wasViewTeleported = false;
		}

		/// <summary>
		/// True for the next update after the player is teleported.
		/// </summary>
		private bool wasViewTeleported;
		private int cullingUpdateIndex = 0;
		private List<CullingVolume> volumesWithObjects = new List<CullingVolume>();
		private HashSet<CullingVolume> volumesWithVisibilityUpdates = new HashSet<CullingVolume>();
		private List<CullingVolume> volumesToRemoveFromUpdatesList = new List<CullingVolume>();
		private UnityEngine.Profiling.CustomSampler relevanceSampler = UnityEngine.Profiling.CustomSampler.Create("CullingVolumeManager.UpdateRelevantCullingVolumes");
		private UnityEngine.Profiling.CustomSampler visibilitySampler = UnityEngine.Profiling.CustomSampler.Create("CullingVolumeManager.UpdateObjectsVisibility");
		private UnityEngine.Profiling.CustomSampler gizmoLabelSampler = UnityEngine.Profiling.CustomSampler.Create("CullingVolumeManager.LabelGizmos");
#endif // !DEDICATED_SERVER
	}
}
