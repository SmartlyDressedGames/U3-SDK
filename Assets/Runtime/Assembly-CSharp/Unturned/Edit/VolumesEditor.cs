////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using SDG.Framework.Devkit;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned
{
	public class VolumesEditor : SelectionTool
	{
		private VolumeManagerBase _activeVolumeManager;
		public VolumeManagerBase activeVolumeManager
		{
			get => _activeVolumeManager;
			set
			{
				DevkitSelectionManager.clear();
				_activeVolumeManager = value;
			}
		}

		protected override bool RaycastSelectableObjects(Ray ray, out RaycastHit hitInfo)
		{
			if (activeVolumeManager != null)
			{
				return activeVolumeManager.Raycast(ray, out hitInfo, 8192.0f);
			}
			else
			{
				hitInfo = default;
				return false;
			}
		}

		protected override void RequestInstantiation(Vector3 position)
		{
			if (activeVolumeManager != null)
			{
				activeVolumeManager.InstantiateVolume(position, Quaternion.identity, Vector3.one);
			}
		}

		protected override bool HasBoxSelectableObjects()
		{
			return activeVolumeManager != null;
		}

		protected override IEnumerable<GameObject> EnumerateBoxSelectableObjects()
		{
			if (activeVolumeManager == null)
				yield break;

			foreach (VolumeBase volume in activeVolumeManager.EnumerateAllVolumes())
			{
				if (!volume.CanBeSelected)
					continue;

				yield return volume.areaSelectGameObject;
			}
		}
	}
}
